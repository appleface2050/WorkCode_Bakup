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
    ResultEngineDAU, ResultEngineUninstallRate, ScopeEngine, EngineStats, EmulatorStats
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec

def engine2(request):
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

    scope = ScopeEngine.get_scope(version,channel,osver)

    if scope:
        print scope.pk
        while(start < end):
            print start
            es = EngineStats.objects.filter(result_date=start,scope_id=scope.pk)
            if es:
                es = es[0]
                try:
                    install_fail_rate = float(es.install_fail_user)/float(es.install_fail_user+es.install_success_user)
                except ZeroDivisionError:
                    install_fail_rate = 0
                try:
                    download_begin_minues_download_ok_div_begin_download = float(es.download_begin_user-es.download_ok_user)/float(es.download_begin_user)
                except ZeroDivisionError:
                    download_begin_minues_download_ok_div_begin_download = 0
                try:
                    download_ok_minus_daily_install_div_download_ok = float(es.download_ok_user-es.install_success_user)/float(es.download_ok_user)
                except ZeroDivisionError:
                    download_ok_minus_daily_install_div_download_ok = 0
                try:
                    daily_install_minus_first_init_div_daily_install = float(es.install_success_user-es.first_init_success_user)/float(es.install_success_user)
                except ZeroDivisionError:
                    daily_install_minus_first_init_div_daily_install = 0
                try:
                    install_success_rate = float(es.install_success_user)/float(es.install_success_user+es.init_fail_user)
                except ZeroDivisionError:
                    install_success_rate = 0
                try:
                    first_init_fail_rate = float(es.first_init_fail_user)/float(es.first_init_fail_user+es.first_init_success_user)
                except ZeroDivisionError:
                    first_init_fail_rate = 0
                try:
                    init_not_success_rate = float(es.init_not_success_user) / float(es.init_not_success_user+es.init_success_user)
                except ZeroDivisionError:
                    init_not_success_rate = 0
                try:
                    init_fail_rate = float(es.init_fail_user)/float(es.init_not_success_user+es.init_success_user)
                except ZeroDivisionError:
                    init_fail_rate = 0
                try:
                    emulator_stat = EmulatorStats.objects.filter(result_date=start,scope_id=scope.pk)[0]
                    first_init = emulator_stat.engine_install_and_init_success_user
                except Exception:
                    first_init = 0

                tmp = {"date":start.strftime('%Y-%m-%d'),
                       "daily_install":es.install_success_user,
                       "acc_install_success_user": es.acc_install_success_user,
                       "acc_install":0,
                       "acc_install_fail":0,
                       "daily_uninstall":0,
                       "acc_uninstall":0,
                       "daily_user_init":es.init_success_user,
                       "init_not_success_rate": convert_to_percentage(init_not_success_rate),
                       "init_fail_rate": convert_to_percentage(init_fail_rate),
                       "install_fail_rate": convert_to_percentage(install_fail_rate),
                       "begin_download": es.download_begin_user,
                       "download_ok":es.download_ok_user,
                       "first_init":first_init,
                       # "first_init":es.engine_install_and_init_success_user,
                       "download_begin_minues_download_ok_div_begin_download": convert_to_percentage(download_begin_minues_download_ok_div_begin_download),
                       "download_ok_minus_daily_install_div_download_ok": convert_to_percentage(download_ok_minus_daily_install_div_download_ok),
                       "daily_install_minus_first_init_div_daily_install": convert_to_percentage(daily_install_minus_first_init_div_daily_install),
                       "first_init_fail": es.first_init_fail_user,
                       "first_init_fail_rate": convert_to_percentage(first_init_fail_rate),
                       "install_success_rate": convert_to_percentage(install_success_rate),
                       }
                data.append(tmp)
            start += datetime.timedelta(days=1)
    return render(request, "engine.html", {"datas":data, "start":_start, "end": _end, "channel":channel, "version":version, "osver":osver})



