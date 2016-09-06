# coding=utf-8
import datetime

from django.shortcuts import render
from django.db import connection, transaction
from django.db.models import Sum
from django.contrib.auth.decorators import login_required

# Create your views here.
from bst_server.settings import MAX_DAYS_QUERY
from datastats.models import ResultEngineInstall, ResultEngineActivity, ResultEmulatorInstall, ResultEmulatorActivity, \
    ResultRetention, ResultEmulatorSession, MidResultInstallInitEmulator, ResultEmulatorInstallCount, \
    ResultEmulatorUninstallCount, ResultEmulatorUninstallNextDayCount, MidResultInstallInitEngine, ResultRetentionEngine, \
    ResultEngineDAU, ResultEngineUninstallRate
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec

@login_required
def index(request):
    return render(request, 'index.html')


def engine(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    version = request.GET.get("version","")
    channel = request.GET.get("channel","")
    today = datetime.date.today()

    # print start,end
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

    while(start<end):
        print start

        #daily_install
        # cursor = connection.cursor()
        # if not version:
        #     sql = "SELECT SUM(COUNT) FROM datastats_resultengineinstall WHERE result_date = '%s' AND op='install' AND STATUS='success'" % start.strftime('%Y-%m-%d')
        # else:
        #     sql = "SELECT SUM(COUNT) FROM datastats_resultengineinstall WHERE result_date = '%s' AND op='install' AND STATUS='success' AND VERSION='%s'" % (start.strftime('%Y-%m-%d'),version)
        # cursor.execute(sql)
        # row = cursor.fetchone()
        # daily_install = row[0]
        #
        # #acc_install
        # if not version:
        #     sql = ""
        # else:
        #     sql = ""
        # cursor.execute(sql)

        #daily_install
        q_daily_install = ResultEngineInstall.objects.filter(result_date=start,status="success",op="install")#.exclude(version="2.3.40.6019")
        if version:
            q_daily_install = q_daily_install.filter(version=version)
        if channel:
            q_daily_install = q_daily_install.filter(channel=channel)
        daily_install = q_daily_install.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #acc_install
        q_acc_install = ResultEngineInstall.objects.filter(result_date__lte=start,status="success",op="install")#.exclude(version="2.3.40.6019")
        if version:
            q_acc_install = q_acc_install.filter(version=version)
        if channel:
            q_acc_install = q_acc_install.filter(channel=channel)
        acc_install = q_acc_install.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #acc_install_fail
        q = ResultEngineInstall.objects.filter(result_date__lte=start,status="fail",op="install")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        acc_install_fail = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #daily_uninstall
        q = ResultEngineInstall.objects.filter(result_date=start,status="success",op="uninstall")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        daily_uninstall = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #acc_uninstall
        q = ResultEngineInstall.objects.filter(result_date__lte=start,status="success",op="uninstall")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        acc_uninstall = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #daily_user_init
        q = ResultEngineDAU.objects.filter(result_date=start)    #.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        daily_user_init = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        # q = ResultEngineActivity.objects.filter(result_date=start,status="success",op__in=["init","first_init"])
        # if version:
        #     q = q.filter(version=version)
        # daily_user_init = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #init_not_success_rate
        init_not_success_rate = 0
        init_success_count = 0
        init_not_success_count = 0
        q_init_success = ResultEngineActivity.objects.filter(result_date=start,status="success",op="init")#.exclude(version="2.3.40.6019")
        q_init_not_success = ResultEngineActivity.objects.filter(result_date=start,op="init").exclude(status="success")#.exclude(version="2.3.40.6019")
        # q_init_not_success = ResultEngineActivity.objects.filter(result_date=start,op="init",status='fail')
        if version:
            q_init_success = q_init_success.filter(version=version)
            q_init_not_success = q_init_not_success.filter(version=version)
        if channel:
            q_init_success = q_init_success.filter(channel=channel)
            q_init_not_success = q_init_not_success.filter(channel=channel)
        init_success_count = q_init_success.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        init_not_success_count = q_init_not_success.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        try:
            init_not_success_rate = float(init_not_success_count) / float(init_not_success_count+init_success_count)
        except Exception:
            init_not_success_rate = 0

        #init_fail_rate
        init_fail_rate = 0
        init_success_count = 0
        init_not_success_count = 0
        # q_init_success = ResultEngineActivity.objects.filter(result_date=start,status="success",op="init")
        q_init_all = ResultEngineActivity.objects.filter(result_date=start,op="init")#.exclude(version="2.3.40.6019")
        # q_init_not_success = ResultEngineActivity.objects.filter(result_date=start,op="init").exclude(status="fail")
        q_init_not_success = ResultEngineActivity.objects.filter(result_date=start,op="init",status='fail')#.exclude(version="2.3.40.6019")
        if version:
            # q_init_success = q_init_success.filter(version=version)
            q_init_not_success = q_init_not_success.filter(version=version)
            q_init_all = q_init_all.filter(version=version)
        if channel:
            # q_init_success = q_init_success.filter(channel=channel)
            q_init_not_success = q_init_not_success.filter(channel=channel)
            q_init_all = q_init_all.filter(channel=channel)
        # init_success_count = q_init_success.aggregate(Sum('dst_count'))["dst_count__sum"] or 0
        init_not_success_count = q_init_not_success.aggregate(Sum('dst_count'))["dst_count__sum"] or 0
        init_all_count = q_init_all.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        try:
            init_fail_rate = float(init_not_success_count) / float(init_all_count)
        except Exception:
            init_fail_rate = 0




        #install_fail_rate
        install_fail_rate = 0
        install_success_count = 0
        install_fail_count = 0
        q_install_success = ResultEngineInstall.objects.filter(result_date=start,status="success",op="install")#.exclude(version="2.3.40.6019")
        q_install_fail = ResultEngineInstall.objects.filter(result_date=start,status="fail",op="install")#.exclude(version="2.3.40.6019")
        # q_install_fail = ResultEngineInstall.objects.filter(result_date=start,op="install").exclude(status="success").exclude(status="download_begin").exclude("download_ok")
        if version:
            q_install_success = q_install_success.filter(version=version)
            q_install_fail = q_install_fail.filter(version=version)
        if channel:
            q_install_success = q_install_success.filter(channel=channel)
            q_install_fail = q_install_fail.filter(channel=channel)
        result_install_success = [i.toJSON() for i in q_install_success]
        if result_install_success:
            for i in result_install_success:
                install_success_count += i["dst_count"]
        result_install_fail = [i.toJSON() for i in q_install_fail]
        if result_install_fail:
            for i in result_install_fail:
                install_fail_count += i["dst_count"]
        try:
             install_fail_rate = float(install_fail_count) / float(install_fail_count+install_success_count)
        except Exception:
            install_fail_rate = 0

        #begin_download
        q = ResultEngineInstall.objects.filter(result_date=start,status="download_begin",op="install")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        begin_download = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #download_ok
        q = ResultEngineInstall.objects.filter(result_date=start,status="download_ok",op="install")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        download_ok = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #first_init
        q = ResultEngineActivity.objects.filter(result_date=start,status="success",op="first_init")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        first_init = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #first_init_fail
        q = ResultEngineActivity.objects.filter(result_date=start,status="fail",op="first_init")#.exclude(version="2.3.40.6019")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        first_init_fail = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        # download_ok_div_begin_download,daily_install_div_download_ok,first_init_div_daily_install = 0,0,0
        try:
            download_ok_div_begin_download = convert_to_percentage((float(begin_download)-float(download_ok))/begin_download)
        except ZeroDivisionError, e:
            download_ok_div_begin_download = 0
        try:
            daily_install_div_download_ok = convert_to_percentage((float(download_ok)-float(daily_install))/download_ok)
        except ZeroDivisionError, e:
            daily_install_div_download_ok = 0
        try:
            first_init_div_daily_install = convert_to_percentage((float(daily_install)-float(first_init))/daily_install)
        except ZeroDivisionError, e:
            first_init_div_daily_install = 0
        try:
            first_init_fail_rate = convert_to_percentage(float(first_init_fail)/float(daily_install))
        except ZeroDivisionError, e:
            first_init_fail_rate = 0
        try:
            install_success_rate = convert_to_percentage(float(first_init)/float(begin_download))
        except ZeroDivisionError, e:
            install_success_rate = 0

        tmp = {"date":start.strftime('%Y-%m-%d'),
               "daily_install":daily_install,
               "acc_install":acc_install,
               "acc_install_fail":acc_install_fail,
               "daily_uninstall":daily_uninstall,
               "acc_uninstall":acc_uninstall,
               "daily_user_init":daily_user_init,
               "init_not_success_rate": convert_to_percentage(init_not_success_rate),
               "init_fail_rate": convert_to_percentage(init_fail_rate),
               "install_fail_rate": convert_to_percentage(install_fail_rate),
               "begin_download": begin_download,
               "download_ok":download_ok,
               "first_init":first_init,
               "download_ok_div_begin_download": download_ok_div_begin_download,
               "daily_install_div_download_ok": daily_install_div_download_ok,
               "first_init_div_daily_install": first_init_div_daily_install,
               "first_init_fail": first_init_fail,
               "first_init_fail_rate":first_init_fail_rate,
               "install_success_rate":install_success_rate,
               }
        data.append(tmp)
        start += datetime.timedelta(days=1)

    # print data
    # data = [
    #     {"date":"2016-05-01","daily_install":23365,"acc_install":22121,"daily_uninstall":123,"acc_uninstall":321,"daily_install_failrate":"30%"},
    #     {"date":"2016-05-02","daily_install":23365,"acc_install":22121,"daily_uninstall":123,"acc_uninstall":321,"daily_install_failrate":"30%"},
    #     {"date":"2016-05-03","daily_install":23365,"acc_install":22121,"daily_uninstall":123,"acc_uninstall":321,"daily_install_failrate":"30%"},
    #     {"date":"2016-05-04","daily_install":23365,"acc_install":22121,"daily_uninstall":123,"acc_uninstall":321,"daily_install_failrate":"30%"}
    #
    # ]
    return render(request, "engine.html", {"datas":data, "start":_start, "end": _end, "channel":channel})


def emulator(request):
    start = request.GET.get("start","")
    end = request.GET.get("end","")
    version = request.GET.get("version","")
    channel = request.GET.get("channel","")
    today = datetime.date.today()
    # print start,end
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

    while(start < end):
        print start

        #daily_install
        q_daily_install = ResultEmulatorInstall.objects.filter(result_date=start,status="success",op="install")
        if version:
            q_daily_install = q_daily_install.filter(version=version)
        if channel:
            q_daily_install = q_daily_install.filter(channel=channel)
        daily_install = q_daily_install.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #acc_install
        q_acc_install = ResultEmulatorInstall.objects.filter(result_date__lte=start,status="success",op="install")
        if version:
            q_acc_install = q_acc_install.filter(version=version)
        if channel:
            q_acc_install = q_acc_install.filter(channel=channel)
        acc_install = q_acc_install.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #acc_install_fail
        q = ResultEmulatorInstall.objects.filter(result_date__lte=start,status="fail",op="install")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        acc_install_fail = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #daily_uninstall
        q_daily_uninstall = ResultEmulatorInstall.objects.filter(result_date=start,status="success",op="uninstall")
        if version:
            q_daily_uninstall = q_daily_uninstall.filter(version=version)
        if channel:
            q_daily_uninstall = q_daily_uninstall.filter(channel=channel)
        daily_uninstall = q_daily_uninstall.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        # #acc_uninstall
        # q = ResultEmulatorInstall.objects.filter(result_date__lte=start,status="success",op="uninstall")
        # if version:
        #     q = q.filter(version=version)
        # acc_uninstall = q.aggregate(Sum('count'))["count__sum"] or 0

        #daily_user_init
        q = ResultEmulatorActivity.objects.filter(result_date=start,status="success",op="init")
        if version:
            q = q.filter(version=version)
        if channel:
            q = q.filter(channel=channel)
        daily_user_init = q.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        #init_fail_rate
        init_fail_rate = 0
        init_success_count = 0
        init_fail_count = 0
        q_init_success = ResultEmulatorActivity.objects.filter(result_date=start,status="success",op="init")
        q_init_fail = ResultEmulatorActivity.objects.filter(result_date=start,status="fail",op="init")
        if version:
            q_init_success = q_init_success.filter(version=version)
            q_init_fail = q_init_fail.filter(version=version)
        if channel:
            q_init_success = q_init_success.filter(channel=channel)
            q_init_fail = q_init_fail.filter(channel=channel)
        init_success_count = q_init_success.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        init_fail_count = q_init_fail.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        try:
            init_fail_rate = float(init_fail_count) / float(init_fail_count+init_success_count)
        except Exception:
            init_fail_rate = 0

        #install_fail_rate
        install_fail_rate = 0
        install_success_count = 0
        install_fail_count = 0
        q_install_success = ResultEmulatorInstall.objects.filter(result_date=start,status="success",op="install")
        q_install_fail = ResultEmulatorInstall.objects.filter(result_date=start,status="fail",op="install")
        if version:
            q_install_success = q_install_success.filter(version=version)
            q_install_fail = q_install_fail.filter(version=version)
        if channel:
            q_install_success = q_install_success.filter(channel=channel)
            q_install_fail = q_install_fail.filter(channel=channel)
        result_install_success = [i.toJSON() for i in q_install_success]
        if result_install_success:
            for i in result_install_success:
                install_success_count += i["dst_count"]
        result_install_fail = [i.toJSON() for i in q_init_fail]
        if result_install_fail:
            for i in result_install_fail:
                install_fail_count += i["dst_count"]
        try:
            install_fail_rate = float(install_fail_count) / float(install_fail_count+install_success_count)
        except Exception:
            install_fail_rate = 0


        #install_and_init use for retention
        q_install_and_init = MidResultInstallInitEmulator.objects.filter(result_date=start)
        if version:
            q_install_and_init = q_install_and_init.filter(version=version)
        if channel:
            q_install_and_init = q_install_and_init.filter(channel=channel)
        install_and_init = q_install_and_init.aggregate(Sum('dst_count'))["dst_count__sum"] or 0


        #install_and_init_engin use for retention
        q_install_and_init_engine = MidResultInstallInitEngine.objects.filter(result_date=start)
        if version:
            q_install_and_init_engine = q_install_and_init_engine.filter(version=version)
        if channel:
            q_install_and_init_engine = q_install_and_init_engine.filter(channel=channel)
        install_and_init_engine = q_install_and_init_engine.aggregate(Sum('dst_count'))["dst_count__sum"] or 0


        #retention
        q_retention_2 = ResultRetention.objects.filter(result_date=start,retention="2")
        if version:
            q_retention_2 = q_retention_2.filter(version=version)
        if channel:
            q_retention_2 = q_retention_2.filter(channel=channel)
        retention_2 = q_retention_2.aggregate(Sum('count'))["count__sum"] or 0
        try:
            retention_2 = float(retention_2) / float(install_and_init)
        except Exception:
            retention_2 = 0

        #retention
        q_retention_7 = ResultRetention.objects.filter(result_date=start,retention="7")
        if version:
            q_retention_7 = q_retention_7.filter(version=version)
        if channel:
            q_retention_7 = q_retention_7.filter(channel=channel)
        retention_7 = q_retention_7.aggregate(Sum('count'))["count__sum"] or 0
        try:
            retention_7 = float(retention_7) / float(install_and_init)
        except Exception:
            retention_7 = 0


        #retention_engine
        q_retention_engine_2 = ResultRetentionEngine.objects.filter(result_date=start,retention="2")
        if version:
            q_retention_engine_2 = q_retention_engine_2.filter(version=version)
        if channel:
            q_retention_engine_2 = q_retention_engine_2.filter(channel=channel)
        retention_engine_2 = q_retention_engine_2.aggregate(Sum('count'))["count__sum"] or 0
        try:
            retention_engine_2 = float(retention_engine_2) / float(install_and_init_engine)
        except Exception:
            retention_engine_2 = 0

        # #retention
        # q_retention_14 = ResultRetention.objects.filter(result_date=start,retention="14")
        # if version:
        #     q_retention_14 = q_retention_14.filter(version=version)
        # retention_14 = q_retention_14.aggregate(Sum('count'))["count__sum"] or 0
        # try:
        #     retention_14 = float(retention_14) / float(install_and_init)
        # except Exception:
        #     retention_14 = 0

        # #retention
        # q_retention_30 = ResultRetention.objects.filter(result_date=start,retention="30")
        # if version:
        #     q_retention_30 = q_retention_30.filter(version=version)
        # retention_30 = q_retention_30.aggregate(Sum('count'))["count__sum"] or 0
        # try:
        #     retention_30 = float(retention_30) / float(install_and_init)
        # except Exception:
        #     retention_30 = 0


        #session Daily No of Session (times)
        #数据是通过各个版本的数量相加，这种计算方式是可以的，因为一个用户通常只用一个版本，未去重数据量极少，可以忽略
        q_daily_no_of_session = ResultEmulatorActivity.objects.filter(result_date=start,status="success",op="init")
        if version:
            q_daily_no_of_session = q_daily_no_of_session.filter(version=version)
        if channel:
            q_daily_no_of_session = q_daily_no_of_session.filter(channel=channel)
        dst_count= q_daily_no_of_session.aggregate(Sum('dst_count'))["dst_count__sum"] or 0
        count = q_daily_no_of_session.aggregate(Sum('count'))["count__sum"] or 0
        try:
            daily_no_of_session = float(count) / float(dst_count)
        except Exception as e:
            daily_no_of_session = 0
        daily_no_of_session = "%.01f"% (daily_no_of_session)


        #session Daily User Session (length)
        q_daily_user_session = ResultEmulatorSession.objects.filter(result_date=start)
        if version:
            q_daily_user_session = q_daily_user_session.filter(version=version)
        else:
            q_daily_user_session = q_daily_user_session.filter(version="")
        # if channel:
        #     q_daily_user_session = q_daily_user_session.filter(channel=channel)
        # else:
        #     q_daily_user_session = q_daily_user_session.filter(channel="")


        if q_daily_user_session:
            dus = q_daily_user_session[0].dus
        else:
            dus = 0
        dus = convert_sec_to_min_sec(dus)

        #uninstall rate
        q_emulator_intall_count = ResultEmulatorInstallCount.objects.filter(result_date=start)
        if version:
            q_emulator_intall_count = q_emulator_intall_count.filter(version=version)
        if channel:
            q_emulator_intall_count = q_emulator_intall_count.filter(channel=channel)
        install_dst_count = q_emulator_intall_count.aggregate(Sum('dst_count'))["dst_count__sum"] or 0
        # print install_dst_count

        q_emulator_uninstall_dst_count = ResultEmulatorUninstallCount.objects.filter(result_date=start)
        if version:
            q_emulator_uninstall_dst_count = q_emulator_uninstall_dst_count.filter(version=version)
        if channel:
            q_emulator_uninstall_dst_count = q_emulator_uninstall_dst_count.filter(channel=channel)
        uninstall_dst_count = q_emulator_uninstall_dst_count.aggregate(Sum('dst_count'))["dst_count__sum"] or 0
        # print uninstall_dst_count

        q_emulator_uninstall_next_day = ResultEmulatorUninstallNextDayCount.objects.filter(result_date=start)
        if version:
            q_emulator_uninstall_next_day = q_emulator_uninstall_next_day.filter(version=version)
        if channel:
            q_emulator_uninstall_next_day = q_emulator_uninstall_next_day.filter(channel=channel)
        uninstall_next_day_dst_count = q_emulator_uninstall_next_day.aggregate(Sum('dst_count'))["dst_count__sum"] or 0
        # print uninstall_next_day_dst_count

        try:
            today_uninstall_rate = float(uninstall_dst_count) / float(install_dst_count)
        except Exception as e:
            today_uninstall_rate = 0
        # today_uninstall_rate = "%.01f"% (today_uninstall_rate)

        try:
            next_day_uninstall_rate = float(uninstall_dst_count+uninstall_next_day_dst_count) / float(install_dst_count)
        except Exception as e:
            next_day_uninstall_rate = 0
        # next_day_uninstall_rate = "%.01f"% (next_day_uninstall_rate)
        yesterday = datetime.date.today() + datetime.timedelta(days=-1)
        if start.strftime('%Y-%m-%d') == yesterday.strftime('%Y-%m-%d'):
            next_day_uninstall_rate = 0


        #engine_uninstall_rate
        q_engine_first_init = ResultEngineUninstallRate.objects.filter(result_date=start,field="first_init")
        if version:
            q_engine_first_init = q_engine_first_init.filter(version=version)
        if channel:
            q_engine_first_init = q_engine_first_init.filter(channel=channel)
        first_init_count = q_engine_first_init.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        q_engine_today_uninstall = ResultEngineUninstallRate.objects.filter(result_date=start,field="today_uninstall")
        if version:
            q_engine_today_uninstall = q_engine_today_uninstall.filter(version=version)
        if channel:
            q_engine_today_uninstall = q_engine_today_uninstall.filter(channel=channel)
        today_uninstall_count = q_engine_today_uninstall.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        q_engine_next_day_uninstall = ResultEngineUninstallRate.objects.filter(result_date=start,field="next_day_uninstall")
        if version:
            q_engine_next_day_uninstall = q_engine_next_day_uninstall.filter(version=version)
        if channel:
            q_engine_next_day_uninstall = q_engine_next_day_uninstall.filter(channel=channel)
        next_day_uninstall_count = q_engine_next_day_uninstall.aggregate(Sum('dst_count'))["dst_count__sum"] or 0

        try:
            engine_first_init_today_uninstall_rate = float(today_uninstall_count) / float(first_init_count)
        except ZeroDivisionError:
            engine_first_init_today_uninstall_rate = 0
        try:
            engine_first_init_next_day_uninstall_rate = float(next_day_uninstall_count) / float(first_init_count)
        except ZeroDivisionError:
            engine_first_init_next_day_uninstall_rate = 0

        data_for_render = {"date":start.strftime('%Y-%m-%d'),
               "daily_install":daily_install,
               "acc_install":acc_install,
               "acc_install_fail":acc_install_fail,
               "daily_uninstall":daily_uninstall,
               "today_uninstall_rate":convert_to_percentage(today_uninstall_rate),
               "next_day_uninstall_rate":convert_to_percentage(next_day_uninstall_rate),
               "engine_first_init_today_uninstall_rate":convert_to_percentage(engine_first_init_today_uninstall_rate),
               "engine_first_init_next_day_uninstall_rate":convert_to_percentage(engine_first_init_next_day_uninstall_rate),
               # "acc_uninstall":acc_uninstall,
               "daily_user_init":daily_user_init,
               "init_fail_rate": convert_to_percentage(init_fail_rate),
               "install_fail_rate": convert_to_percentage(install_fail_rate),
               "retention_2":convert_to_percentage(retention_2),
               "retention_7":convert_to_percentage(retention_7),
               "retention_engine_2":convert_to_percentage(retention_engine_2),
               # "retention_14":convert_to_percentage(retention_14),
               # "retention_30":convert_to_percentage(retention_30),
               "daily_no_of_session":daily_no_of_session,
               "daily_user_session":dus
               }
        data.append(data_for_render)
        start += datetime.timedelta(days=1)

    return render(request, "emulator.html", {"datas":data, "start":_start, "end": _end, "channel":channel})

def appcenter(request):
    return render(request, "c.html")




