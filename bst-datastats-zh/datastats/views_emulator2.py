# coding=utf-8
import datetime

from django.shortcuts import render
from django.db import connection, transaction
from django.db.models import Sum
from django.contrib.auth.decorators import login_required

# Create your views here.
from bst_server.settings import MAX_DAYS_QUERY
from datastats.models import ScopeEngine, EngineStats, EmulatorStats, ScopeEmulator
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec, divide


def emulator2(request):
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

    scope_emulator = ScopeEmulator.get_scope(version,channel,osver)
    scope_engine = ScopeEngine.get_scope(version,channel,osver)

    if scope_emulator:
        print scope_emulator.pk
        while(start < end):
            print start
            emulator_stats = EmulatorStats.objects.filter(result_date=start,scope_id=scope_emulator.pk)
            if scope_engine and EngineStats.objects.filter(result_date=start,scope_id=scope_engine.pk).exists():
                engine_stats = EngineStats.objects.filter(result_date=start,scope_id=scope_engine.pk)[0]
            else:
                engine_stats = None

            if engine_stats:
                engine_first_init_next_day_uninstall_rate_fenzi = engine_stats.first_init_no_update_and_uninstall_emulator_user+engine_stats.first_init_no_update_and_next_day_uninstall_emulator_user

            else:
                engine_first_init_next_day_uninstall_rate_fenzi = 0

            if emulator_stats:
                emulator_stats = emulator_stats[0]
                # engine_stats = engine_stats[0]
                engine_first_init_today_uninstall_rate, engine_first_init_next_day_uninstall_rate = 0, 0
                engine_first_init_today_uninstall_rate_fenzi, engine_first_init_today_uninstall_rate_fenmu = 0, 0

                if emulator_stats and engine_stats:
                    engine_first_init_next_day_uninstall_rate = convert_to_percentage(divide(engine_stats.first_init_no_update_and_uninstall_emulator_user+engine_stats.first_init_no_update_and_next_day_uninstall_emulator_user,
                                                                                                emulator_stats.engine_install_and_init_success_user))
                    if engine_stats.first_init_no_update_and_next_day_uninstall_emulator_user == 0 or engine_stats.first_init_no_update_and_uninstall_emulator_user == 0:
                        engine_first_init_next_day_uninstall_rate = "0%"
                    engine_first_init_today_uninstall_rate = convert_to_percentage(divide(engine_stats.first_init_no_update_and_uninstall_emulator_user,
                                                                                              emulator_stats.engine_install_and_init_success_user))
                    engine_first_init_today_uninstall_rate_fenzi = engine_stats.first_init_no_update_and_uninstall_emulator_user
                    engine_first_init_today_uninstall_rate_fenmu = engine_stats.first_init_no_update_success_user

                next_day_uninstall_rate = convert_to_percentage(divide(emulator_stats.next_day_uninstall_success_user+emulator_stats.uninstall_success_user,
                                                                               emulator_stats.install_success_user))
                if emulator_stats.next_day_uninstall_success_user == 0:
                    next_day_uninstall_rate = "0%"

                tmp = {"date":start.strftime('%Y-%m-%d'),
                       "daily_install": emulator_stats.install_success_user,
                       "daily_uninstall": emulator_stats.uninstall_success_user,
                       "acc_install_success_user": emulator_stats.acc_install_success_user,
                       "today_uninstall_rate": convert_to_percentage(divide(emulator_stats.uninstall_success_user,
                                                                            emulator_stats.install_success_user)),
                       "next_day_uninstall_rate": next_day_uninstall_rate,
                       "engine_first_init_today_uninstall_rate_fenzi": engine_first_init_today_uninstall_rate_fenzi,
                       "engine_first_init_today_uninstall_rate_fenmu": emulator_stats.engine_install_and_init_success_user,
                       "engine_first_init_today_uninstall_rate": engine_first_init_today_uninstall_rate,
                       "engine_first_init_next_day_uninstall_rate_fenzi": engine_first_init_next_day_uninstall_rate_fenzi,
                       "engine_first_init_next_day_uninstall_rate_fenmu":emulator_stats.engine_install_and_init_success_user,
                       "engine_first_init_next_day_uninstall_rate": engine_first_init_next_day_uninstall_rate,

                       # "install_fail_rate":0,
                       "daily_user_init": emulator_stats.init_success_user,
                       "init_fail_rate": convert_to_percentage(divide(emulator_stats.init_fail_user, emulator_stats.init_user)),
                       "retention_2": convert_to_percentage(divide(emulator_stats.retention_2,emulator_stats.install_and_init_success_user)),
                       "retention_7": convert_to_percentage(divide(emulator_stats.retention_7,emulator_stats.install_and_init_success_user)),
                       "retention_engine_2_fenzi": emulator_stats.retention_2_base_on_engine,
                       "retention_engine_2_fenmu": emulator_stats.engine_install_and_init_success_user,
                       "retention_engine_2": convert_to_percentage(divide(emulator_stats.retention_2_base_on_engine, emulator_stats.engine_install_and_init_success_user)),

                       "base_on_engine_retention_7_normal": convert_to_percentage(divide(emulator_stats.retention_7_base_on_engine_normal,emulator_stats.install_and_init_success_user)),
                       "base_on_engine_retention_14_normal": convert_to_percentage(divide(emulator_stats.retention_14_base_on_engine_normal,emulator_stats.install_and_init_success_user)),
                       "daily_no_of_session": "%.1f" % divide(emulator_stats.init_success_count, emulator_stats.init_success_user)

                       # "retention_engine_2_2_fenzi": emulator_stats.retention_2_base_on_engine2,
                       # "retention_engine_2_2_fenmu": emulator_stats.engine_install_and_init_success_user2,
                       # "retention_engine_2_2" : convert_to_percentage(divide(emulator_stats.retention_2_base_on_engine2, emulator_stats.engine_install_and_init_success_user2)),

                       }
                data.append(tmp)
            start += datetime.timedelta(days=1)
    return render(request, "emulator.html", {"datas":data, "start":_start, "end": _end, "channel":channel, "osver":osver, "version":version})

