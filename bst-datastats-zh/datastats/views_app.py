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
    ScopeAppPackagename
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec, data_merge, find_app_name


def app_page2(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    package_name = request.GET.get("package_name","com.nianticlabs.pokemongo")
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
    data = []
    while(start < end):
        print start
        sql = """
        SELECT a.package_name,
        b.daily_user_init, b.daily_user_init_count, b.daily_init_fail,b.daily_init_fail_count,
        b.daily_install,b.daily_install_count,b.daily_install_fail,b.daily_install_fail_count,
        b.daily_download,b.daily_download_count,b.daily_download_fail,b.daily_download_fail_count,
        b.daily_install_from_download,b.daily_install_from_download_count,b.daily_install_from_download_fail,b.daily_install_from_download_fail_count
        FROM scope_app_package_name a
        JOIN stats_app_package_name b ON a.id = b.scope_id
        WHERE a.modes='%s' AND b.result_date = '%s' and a.package_name = "%s"
        """ % ("day", start.strftime('%Y-%m-%d'), package_name)
        # print sql
        cursor = connection.cursor()
        cursor.execute(sql)
        res = cursor.fetchall()
        for i in res:
            tmp = {
                "date":start.strftime('%Y-%m-%d'),
                "package_name":i[0],
                "daily_user_init":i[1],
                "daily_user_init_count":i[2],
                "daily_init_fail":i[3],
                "daily_init_fail_count":i[4],

                "daily_install":i[5],
                "daily_install_count":i[6],
                "daily_install_fail":i[7],
                "daily_install_fail_count":i[8],

                "daily_download":i[9],
                "daily_download_count":i[10],
                "daily_download_fail":i[11],
                "daily_download_fail_count":i[12],

                "daily_install_from_download":i[13],
                "daily_install_from_download_count":i[14],
                "daily_install_from_download_fail":i[15],
                "daily_install_from_download_fail_count":i[16],
            }
            data.append(tmp)
        start += datetime.timedelta(days=1)

    sum_daily_user_init, sum_daily_user_init_count, sum_daily_init_fail, sum_daily_init_fail_count = 0, 0, 0, 0
    sum_daily_download, sum_daily_download_count, sum_daily_download_fail, sum_daily_download_fail_count = 0, 0, 0, 0
    sum_daily_install, sum_daily_install_count, sum_daily_install_fail, sum_daily_install_fail_count = 0, 0, 0, 0

    if package_name:
        for i in data:
            sum_daily_user_init += i["daily_user_init"]
            sum_daily_user_init_count += i["daily_user_init_count"]
            sum_daily_init_fail += i["daily_init_fail"]
            sum_daily_init_fail_count += i["daily_init_fail_count"]

            sum_daily_download += i["daily_download"]
            sum_daily_download_count += i["daily_download_count"]
            sum_daily_download_fail += i["daily_download_fail"]
            sum_daily_download_fail_count += i["daily_download_fail_count"]

            sum_daily_install+= i["daily_install"]
            sum_daily_install_count += i["daily_install_count"]
            sum_daily_install_fail += i["daily_install_fail"]
            sum_daily_install_fail_count += i["daily_install_fail_count"]

    _sum = {
        "date": "总计",
        "daily_user_init":sum_daily_user_init,
        "daily_user_init_count":sum_daily_user_init_count,
        "daily_init_fail":sum_daily_init_fail,
        "daily_init_fail_count":sum_daily_init_fail_count,

        "daily_download":sum_daily_download,
        "daily_download_count":sum_daily_download_count,
        "daily_download_fail":sum_daily_download_fail,
        "daily_download_fail_count":sum_daily_download_fail_count,

        "daily_install":sum_daily_install,
        "daily_install_count":sum_daily_install_count,
        "daily_install_fail":sum_daily_install_fail,
        "daily_install_fail_count":sum_daily_install_fail_count,
    }
    data.append(_sum)
    return render(request, "app_page2.html", {"datas":data, "start":_start, "end": _end, "package_name":package_name})


def app_page1(request):
    start = request.GET.get("start","")
    package_name = request.GET.get("package_name","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    else:
        start = (datetime.timedelta(days=-1) + today)
    _start = start.strftime('%Y-%m-%d')
    data = []
    sql = """
    SELECT a.package_name,
    b.daily_user_init, b.daily_user_init_count, b.daily_init_fail,b.daily_init_fail_count,
    b.daily_install,b.daily_install_count,b.daily_install_fail,b.daily_install_fail_count,
    b.daily_download,b.daily_download_count,b.daily_download_fail,b.daily_download_fail_count,
    b.daily_install_from_download,b.daily_install_from_download_count,b.daily_install_from_download_fail,b.daily_install_from_download_fail_count
    FROM scope_app_package_name a
    JOIN stats_app_package_name b ON a.id = b.scope_id
    WHERE a.modes='%s' AND b.result_date = '%s' and b.daily_user_init !=0
    """ % ("day", start.strftime('%Y-%m-%d'))
    # print sql
    cursor = connection.cursor()
    cursor.execute(sql)
    res = cursor.fetchall()
    for i in res:
        tmp = {
                "package_name":i[0],
               "daily_user_init":i[1],
               "daily_user_init_count":i[2],
               "daily_init_fail":i[3],
               "daily_init_fail_count":i[4],

               "daily_install":i[5],
               "daily_install_count":i[6],
               "daily_install_fail":i[7],
               "daily_install_fail_count":i[8],

               "daily_download":i[9],
               "daily_download_count":i[10],
               "daily_download_fail":i[11],
               "daily_download_fail_count":i[12],

               "daily_install_from_download":i[13],
               "daily_install_from_download_count":i[14],
               "daily_install_from_download_fail":i[15],
               "daily_install_from_download_fail_count":i[16],
               }
        data.append(tmp)

    for i in data:
        try:
            i['init_success_rate'] = convert_to_percentage(float(i['daily_user_init']) / float(i['daily_user_init'] + i['daily_init_fail']))
        except ZeroDivisionError:
            i['init_success_rate'] = 0
        try:
            i['install_success_rate'] = convert_to_percentage(float(i['daily_install']) / float(i['daily_install'] + i['daily_install_fail']))
        except ZeroDivisionError:
            i['install_success_rate'] = 0
        try:
            i['download_success_rate'] = convert_to_percentage(float(i['daily_download']) / float(i['daily_download'] + i['daily_download_fail']))
        except ZeroDivisionError:
            i['download_success_rate'] = 0
        try:
            i['install_fail_rate'] = convert_to_percentage(float(i['daily_install_fail']) / float(i['daily_install'] + i['daily_install_fail']))
        except ZeroDivisionError:
            i['install_fail_rate'] = 0

    data.sort(lambda y,x:cmp(x["daily_user_init"],y["daily_user_init"]))
    data = find_app_name(data)

    page = int(request.GET.get('page',"1"))
    paginator = Paginator(data, PAGINATOR_RESULT_NUMBER)

    try:
        data = paginator.page(page)
    except PageNotAnInteger:
        data = paginator.page(1)
    except EmptyPage:
        data = paginator.page(paginator.num_pages)

    return render(request, "app_page1.html", {"datas":data, "start":_start, "package_name":package_name, 'page_range':paginator.page_range})











