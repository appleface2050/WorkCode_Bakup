# coding=utf-8
import datetime

from django.shortcuts import render
from django.db import connection, transaction
from django.db.models import Sum

from django.core.paginator import Paginator, EmptyPage, PageNotAnInteger

# Create your views here.
from bst_server.settings import MAX_DAYS_QUERY, PAGINATOR_RESULT_NUMBER
from datastats.models import ResultEngineInstall, ResultEngineActivity, ResultEmulatorInstall, ResultEmulatorActivity, \
    ResultRetention, ResultEmulatorSession, MidResultInstallInitEmulator, ResultAppActivity, ResultAppInstall, ResultAppSession, \
    ResultAppTotal, ResultAppLocalTop500
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec, data_merge, find_app_name


def app_total(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    version = request.GET.get("version","")
    filter = request.GET.get("filter","all")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    if end:
        end = datetime.datetime.strptime(end,'%Y-%m-%d')
    if not start or not end:
        start = (datetime.timedelta(days=-7) + today)
        end = today

    if (end - start)>datetime.timedelta(days=MAX_DAYS_QUERY):
        start = end - datetime.timedelta(days=MAX_DAYS_QUERY)
    _start = start.strftime('%Y-%m-%d')
    _end = end.strftime('%Y-%m-%d')

    result = []

    if filter == "all":
        while(start < end):
            print start
            q = ResultAppTotal.objects.filter(result_date=start)
            if version:
                q = q.filter(version=version)

            tmp = {
                    "result_date":start,
                    "daily_user_init":q.aggregate(Sum('daily_user_init'))["daily_user_init__sum"] or 0,
                    "daily_user_init_count":q.aggregate(Sum('daily_user_init_count'))["daily_user_init_count__sum"] or 0,
                    "daily_init_fail":q.aggregate(Sum('daily_init_fail'))["daily_init_fail__sum"] or 0,
                    "daily_init_fail_count":q.aggregate(Sum('daily_init_fail_count'))["daily_init_fail_count__sum"] or 0,

                    "daily_install":q.aggregate(Sum('daily_install'))["daily_install__sum"] or 0,
                    "daily_install_count":q.aggregate(Sum('daily_install_count'))["daily_install_count__sum"] or 0,
                    "daily_install_fail":q.aggregate(Sum('daily_install_fail'))["daily_install_fail__sum"] or 0,
                    "daily_install_fail_count":q.aggregate(Sum('daily_install_fail_count'))["daily_install_fail_count__sum"] or 0,

                    "daily_download":q.aggregate(Sum('daily_download'))["daily_download__sum"] or 0,
                    "daily_download_count":q.aggregate(Sum('daily_download_count'))["daily_download_count__sum"] or 0,
                    "daily_download_fail":q.aggregate(Sum('daily_download_fail'))["daily_download_fail__sum"] or 0,
                    "daily_download_fail_count":q.aggregate(Sum('daily_download_fail_count'))["daily_download_fail_count__sum"] or 0,
                }
            try:
                tmp['init_success_rate'] = convert_to_percentage(float(tmp['daily_user_init'])/ float(tmp['daily_user_init']+tmp['daily_init_fail']))
            except ZeroDivisionError, e:
                tmp['init_success_rate'] = 0

            try:
                tmp['install_success_rate'] = convert_to_percentage(float(tmp['daily_install'])/ float(tmp['daily_install']+tmp['daily_install_fail']))
            except ZeroDivisionError, e:
                tmp['install_success_rate'] = 0

            try:
                tmp['download_success_rate'] = convert_to_percentage(float(tmp['daily_download'])/ float(tmp['daily_download']+tmp['daily_download_fail']))
            except ZeroDivisionError, e:
                tmp['download_success_rate'] = 0

            result.append(tmp)
            start += datetime.timedelta(days=1)

    elif filter == "local_install":
        pass
    elif filter == "no_default_app":
        pass
    return render(request, "app_total.html", {"datas":result, "start":_start, "end": _end, "filter":filter})


def app_local_50(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    version = request.GET.get("version","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    if end:
        end = datetime.datetime.strptime(end,'%Y-%m-%d')
    if not start or not end:
        start = (datetime.timedelta(days=-1) + today)
        end = today

    if (end - start)>datetime.timedelta(days=MAX_DAYS_QUERY):
        start = end - datetime.timedelta(days=MAX_DAYS_QUERY)
    _start = start.strftime('%Y-%m-%d')
    _end = end.strftime('%Y-%m-%d')

    result = []

    q = ResultAppLocalTop500.objects.filter(result_date__gte=start, result_date__lt=end).values("package_name")
    data = q.annotate(dst_count=Sum('dst_count'),count=Sum('count'))
    start += datetime.timedelta(days=1)
    for i in data:
        if i["package_name"]:
            result.append(i)
    result.sort(lambda y,x:cmp(x["dst_count"],y["dst_count"]))
    result = result[:50]
    return render(request, "app_local_50.html", {"datas":result, "start":_start, "end": _end})


