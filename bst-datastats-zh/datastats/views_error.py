# coding=utf-8
import datetime
import json

from django.shortcuts import render
from django.db import connection, transaction
from django.db.models import Sum,Count

from django.core.paginator import Paginator, EmptyPage, PageNotAnInteger

# Create your views here.
from bst_server.settings import MAX_DAYS_QUERY, PAGINATOR_RESULT_NUMBER
from datastats.models import ResultEngineInstall, ResultEngineActivity, ResultEmulatorInstall, ResultEmulatorActivity, \
    ResultRetention, ResultEmulatorSession, MidResultInstallInitEmulator, ResultAppActivity, ResultAppInstall, ResultAppSession, \
    ResultGeneralAPKInstError, ResultGeneralUninstallReason, ConfUninstallReasonMeaning
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec, data_merge, find_app_name, cal_uninstall_reason_meaning


def uninstall_reason(request):
    start = request.GET.get("start","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    else:
        start = (datetime.timedelta(days=-1) + today)
    _start = start.strftime('%Y-%m-%d')
    end = start + datetime.timedelta(days=1)
    reason = ResultGeneralUninstallReason.objects.filter(op="uninst_reason",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).exclude(uninst_reason_string="")
    # res_number = 30
    # page = int(request.GET.get('page',"1"))
    # paginator = Paginator(reason, res_number)
    # try:
    #     reason = paginator.page(page)
    # except PageNotAnInteger:
    #     # If page is not an integer, deliver first page.
    #     reason = paginator.page(1)
    # except EmptyPage:
    #     # If page is out of range (e.g. 9999), deliver last page of results.
    #     reason = paginator.page(paginator.num_pages)
    # return render(request, "uninstall_reason.html", {"start":_start,"datas":reason, 'page_range':paginator.page_range})
    return render(request, "uninstall_reason.html", {"start":_start,"datas":reason})


def uninstall_reason2(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    if end:
        end = datetime.datetime.strptime(end,'%Y-%m-%d')
    if not start or not end:
        start = (datetime.timedelta(days=-1) + today)
        end = today
    _start = start.strftime('%Y-%m-%d')
    _end = end.strftime('%Y-%m-%d')
    # end = start + datetime.timedelta(days=1)
    reason = []
    res = ResultGeneralUninstallReason.objects.filter(op="uninst_reason",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('uninst_reason_dword').annotate(count=Count(1),dst_count=Count('guid',distinct=True)).order_by("-count")
    total = 0
    count = cal_uninstall_reason_meaning(res)
    for i in count:
        total += count[i]
    for i in count.keys():
        try:
            percentage = convert_to_percentage(float(count[i])/total)
        except ZeroDivisionError:
            percentage = 0
        reason.append(
            {"uninst_reason_dword":i,
             "count":count[i],
             "meaning":ConfUninstallReasonMeaning.get_meaning(i),
             "percentage":percentage
             }
        )

    reason.sort(lambda y,x:cmp(x["count"],y["count"]))

    # res_number = 30
    # page = int(request.GET.get('page',"1"))
    # paginator = Paginator(reason, res_number)
    # try:
    #     reason = paginator.page(page)
    # except PageNotAnInteger:
    #     # If page is not an integer, deliver first page.
    #     reason = paginator.page(1)
    # except EmptyPage:
    #     # If page is out of range (e.g. 9999), deliver last page of results.
    #     reason = paginator.page(paginator.num_pages)
    # data = {"data": [{'value':155, 'name':'视频广告'},
    #                 {'value':274, 'name':'联盟广告'},
    #                 {'value':310, 'name':'邮件营销'},
    #                 {'value':335, 'name':'直接访问'},
    #                 {'value':400, 'name':'搜索引擎'}]}
    result = []
    for i in reason:
        result.append(
            {'value':i["count"], 'name':i['meaning']}
        )
    data = {"data":result}
    return render(request, "uninstall_reason2.html", {"start":_start, "end":_end, "datas":reason, "data":json.dumps(data)})


def apk_install_error(request):
    start = request.GET.get("start","")
    package_name = request.GET.get("package_name","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    else:
        start = (datetime.timedelta(days=-1) + today)
    _start = start.strftime('%Y-%m-%d')
    end = start + datetime.timedelta(days=1)
    # while(start<end):
    #     print start
    #     q = ResultGeneralAPKInstError.objects.filter(op="apk_inst_error",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('errcode').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
    #     data.append(q)
    #     start += datetime.timedelta(days=1)
    # return render(request, "apk_install_error.html", {"datas":data, "start":_start, "end": _end})

    # os_info = ResultUserComputerInfoOS.objects.filter(result_date=start.strftime("%Y-%m-%d")).order_by("-dst_count")

    error = ResultGeneralAPKInstError.objects.filter(op="apk_inst_error",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('errcode').annotate(count=Count(1),dst_count=Count('guid',distinct=True)).order_by("-count")

    res_number = 30
    page = int(request.GET.get('page',"1"))
    paginator = Paginator(error, res_number)
    try:
        os_info = paginator.page(page)
    except PageNotAnInteger:
        # If page is not an integer, deliver first page.
        os_info = paginator.page(1)
    except EmptyPage:
        # If page is out of range (e.g. 9999), deliver last page of results.
        os_info = paginator.page(paginator.num_pages)

    return render(request, "apk_install_error.html", {"start":_start,"datas":error, 'page_range':paginator.page_range})