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
    ResultAppTotal, ResultAppLocalTop500, ScopeAppTotal, AppTotalStat, AppPackagenameStat
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec, data_merge, find_app_name


def app_total2(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    version = request.GET.get("version","")
    channel = request.GET.get("channel","")
    osver = request.GET.get("osver","")
    today = datetime.date.today()

    data = []
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

    scope = ScopeAppTotal.get_scope(version,channel,osver)

    if scope:
        print scope.pk
        while(start < end):
            print start
            app_total_stat = AppTotalStat.objects.filter(result_date=start,scope_id=scope.pk)
            if app_total_stat:
                app_total_stat = app_total_stat[0]

                tmp = {
                        "result_date":start,
                        "daily_user_init": app_total_stat.daily_user_init,
                        "daily_user_init_count": app_total_stat.daily_user_init,
                        "daily_init_fail": app_total_stat.daily_init_fail,
                        "daily_init_fail_count": app_total_stat.daily_init_fail_count,

                        "daily_install": app_total_stat.daily_install,
                        "daily_install_count": app_total_stat.daily_install_count,
                        "daily_install_fail": app_total_stat.daily_install_fail,
                        "daily_install_fail_count": app_total_stat.daily_install_fail_count,

                        "daily_download": app_total_stat.daily_download,
                        "daily_download_count": app_total_stat.daily_download_count,
                        "daily_download_fail": app_total_stat.daily_download_fail,
                        "daily_download_fail_count": app_total_stat.daily_download_fail_count,

                        "daily_install_from_download": app_total_stat.daily_install_from_download,
                        "daily_install_from_download_count": app_total_stat.daily_install_from_download_count,
                        "daily_install_from_download_fail": app_total_stat.daily_install_from_download_fail,
                        "daily_install_from_download_fail_count": app_total_stat.daily_install_from_download_fail_count
                    }

                try:
                    tmp['install_success_count_rate'] = convert_to_percentage(float(tmp['daily_install_count'])/ float(tmp['daily_install_count']+tmp['daily_install_fail_count']))
                except ZeroDivisionError, e:
                    tmp['install_success_count_rate'] = 0
                try:
                    tmp['install_success_rate'] = convert_to_percentage(float(tmp['daily_install'])/ float(tmp['daily_install']+tmp['daily_install_fail']))
                except ZeroDivisionError, e:
                    tmp['install_success_rate'] = 0
                try:
                    tmp['download_success_count_rate'] = convert_to_percentage(float(tmp['daily_download_count'])/ float(tmp['daily_download_count']+tmp['daily_install_from_download_fail']))
                except ZeroDivisionError, e:
                    tmp['download_success_count_rate'] = 0
                try:
                    tmp['download_success_rate'] = convert_to_percentage(float(tmp['daily_download'])/ float(tmp['daily_download']+tmp['daily_download_fail']))
                except ZeroDivisionError, e:
                    tmp['download_success_rate'] = 0

                try:
                    tmp['download_install_rate'] = convert_to_percentage(float(tmp['daily_install_from_download'])/ float(tmp['daily_install']))
                except ZeroDivisionError, e:
                    tmp['download_install_rate'] = 0
                try:
                    tmp['download_install_count_rate'] = convert_to_percentage(float(tmp['daily_install_from_download_count'])/ float(tmp['daily_install_count']))
                except ZeroDivisionError, e:
                    tmp['download_install_count_rate'] = 0

                data.append(tmp)
            start += datetime.timedelta(days=1)
    return render(request, "app_total.html", {"datas":data, "start":_start, "end": _end, "channel":channel, "osver":osver, "version":version})

