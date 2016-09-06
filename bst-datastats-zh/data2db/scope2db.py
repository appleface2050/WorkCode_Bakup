# coding=utf-8

import datetime
import os
import sys
import getopt
import logging
import re

sys.path.append('/var/www/html/master/bst-datastats-zh')

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")
from django.core.wsgi import get_wsgi_application
application = get_wsgi_application()

from django.db import connection, transaction

from util.data_lib import get_next_day, get_next_number_day


from datastats.models import EngineInstall,EngineActivity,EmulatorInstall,EmulatorActivity, ResultEngineInstall, \
    ResultEngineActivity, ResultEmulatorInstall, ResultEmulatorActivity, ResultRetention, ResultEmulatorSession, \
    AppActivity, AppInstall, ResultAppInstall, ResultAppActivity, MidResultInstallInitEmulator, ResultAppSession, \
    ResultEmulatorUninstallCount, ResultEmulatorUninstallNextDayCount, ResultEmulatorInstallCount, \
    MidResultInstallInitEngine, ResultRetentionEngine, ResultEngineDAU, ResultEngineUninstallRate, ResultAppTotal, \
    ResultAppLocalTop500, ScopeEngine, EngineStats, ScopeEmulator, EmulatorStats, ScopeAppTotal, AppTotalStat, \
    ScopeAppPackagename, AppPackagenameStat

class Scope2db(object):
    def __init__(self):
        self.cache = {}

    def reset_cache(self):
        self.cache = {}

    def grp(self, group):
        return ',%s'%group if group else ''

    def make_group(self, scope):
        """
        make group
        """
        group = []
        version = scope.version
        channel = scope.channel
        osver = scope.osver
        if version != "":
            group.append("version")
        if channel != "":
            group.append("channel")
        if osver != "":
            group.append("osver")
        return ','.join(group)

    def group_keys(self, scope):
        group = []
        version = scope.version
        channel = scope.channel
        osver = scope.osver
        if version != "":
            group.append(version)
        if channel != "":
            group.append(channel)
        if osver != "":
            group.append(osver)
        return ','.join(group)

    def generate_scope(self, start, modes):
        """
        添加新的scope
        """
        assert modes == "day"
        self.generate_engine_scope(start, modes)
        self.generate_emulator_scope(start, modes)
        self.generate_app_total_scope(start, modes)
        self.generate_app_package_name_scope(start, modes)


    def generate_app_package_name_scope(self, start, modes):
        sql = """
        SELECT DISTINCT package_name FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s'
        UNION SELECT DISTINCT package_name FROM datastats_appinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s'
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        cursor = connection.cursor()
        cursor.execute(sql)
        data = cursor.fetchall()
        for i in data:
            package_name = i[0]
            if re.match(r'^[0-9a-zA-Z_.]{1,}$',package_name):
                ScopeAppPackagename.add_scope(modes,package_name)


    def generate_app_total_scope(self, start, modes):
        sql = """
        SELECT DISTINCT id,q.version,q.channel,q.osver FROM(
        SELECT a.id,a.guid,a.package_name,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%%%Y-%%%%m-%%%%d')='%s')q
        GROUP BY q.version,q.channel,q.osver
        """ % start.strftime('%Y-%m-%d')
        # print sql
        data = EngineActivity.objects.raw(sql)
        all_version, all_channel, all_osver = [""], [""], [""]
        for i in data:
            all_version.append(i.version)
            all_channel.append(i.channel)
            all_osver.append(i.osver)
            # print i.version, i.channel, i.osver
        all_version = list(set(all_version))
        all_channel = list(set(all_channel))
        all_osver = list(set(all_osver))
        for version in all_version:
            for channel in all_channel:
                for osver in all_osver:
                    ScopeAppTotal.add_scope(version,channel,osver)


    def generate_emulator_scope(self, start, modes):
        sql = """
        SELECT DISTINCT id,q.version,q.channel,q.osver FROM
        (SELECT a.id,a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_emulatoractivity a INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')q
        GROUP BY q.version,q.channel,q.osver
        """ % start.strftime('%Y-%m-%d')
        # print sql
        data = EngineActivity.objects.raw(sql)
        all_version, all_channel, all_osver = [""], [""], [""]
        for i in data:
            all_version.append(i.version)
            all_channel.append(i.channel)
            all_osver.append(i.osver)
            # print i.version, i.channel, i.osver
        all_version = list(set(all_version))
        all_channel = list(set(all_channel))
        all_osver = list(set(all_osver))
        for version in all_version:
            for channel in all_channel:
                for osver in all_osver:
                    ScopeEmulator.add_scope(version,channel,osver)


    def generate_engine_scope(self, start, modes):
        sql = """
    SELECT DISTINCT id,q.version,q.channel,q.osver FROM
    (SELECT a.id,a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_engineactivity a INNER JOIN view_guid_osver b ON a.guid = b.guid
    WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')q
    GROUP BY q.version,q.channel,q.osver
        """ % start.strftime('%Y-%m-%d')
        # print sql
        data = EngineActivity.objects.raw(sql)
        all_version, all_channel, all_osver = [""], [""], [""]
        for i in data:
            all_version.append(i.version)
            all_channel.append(i.channel)
            all_osver.append(i.osver)
            # print i.version, i.channel, i.osver
        all_version = list(set(all_version))
        all_channel = list(set(all_channel))
        all_osver = list(set(all_osver))
        for version in all_version:
            for channel in all_channel:
                for osver in all_osver:
                    ScopeEngine.add_scope(version,channel,osver)


    # def generate_app_scope(self, start, modes):
    #     sql = """
    #     SELECT DISTINCT id,q.package_name,q.version,q.channel,q.osver FROM(
    #     SELECT a.id,a.guid,a.package_name,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appactivity a
    #     INNER JOIN view_guid_osver b ON a.guid = b.guid
    #     WHERE DATE_FORMAT(a.datetime,'%%%%Y-%%%%m-%%%%d')='%s')q
    #     GROUP BY q.package_name,q.version,q.channel,q.osver
    #     """ % start.strftime('%Y-%m-%d')
    #     # print sql
    #     data = AppActivity.objects.raw(sql)
    #     all_package_name, all_version, all_channel, all_osver = [""], [""], [""], [""]
    #     for i in data:
    #         if re.search(r'^[0-9a-zA-Z_.]{1,}$',i.package_name):
    #             all_package_name.append(i.package_name)
    #             all_version.append(i.version)
    #             all_channel.append(i.channel)
    #             all_osver.append(i.osver)
    #         # print i.version, i.channel, i.osver
    #     all_package_name = list(set(all_package_name))
    #     all_version = list(set(all_version))
    #     all_channel = list(set(all_channel))
    #     all_osver = list(set(all_osver))
    #     for package_name in all_package_name:
    #         for version in all_version:
    #             for channel in all_channel:
    #                 for osver in all_osver:
    #                     ScopeApp.add_scope(package_name, version, channel, osver)


    def start_importing(self, modes, start, stats):
        if not stats or not "no_scope" in stats:
            self.generate_scope(start, modes)

        if not stats or "engine" in stats or "other" in stats:
            self.start_import_engine_data(start, modes, stats)
        if not stats or "emulator" in stats or "other" in stats:
            self.start_import_emulator_data(start, modes, stats)
        if not stats or "app" in stats:
            self.start_import_app_data(start, modes)
            self.start_import_app_package_name_data(start, modes)
        if stats and "acc" in stats:
            self.reset_cache()
            scopes = ScopeEngine.objects.filter(modes=modes)
            for scope in scopes:
                self.handle_engine_acc_data(start, modes, scope)
            self.reset_cache()
            scopes = ScopeEmulator.objects.filter(modes=modes)
            for scope in scopes:
                self.handle_emulator_acc_data(start, modes, scope)

        if stats and "tmp" in stats:    #确定正常跑数据有这项内容后再重跑
            self.reset_cache()
            scopes = ScopeEmulator.objects.filter(modes=modes)
            for scope in scopes:
                # self.handle_emulator_init_success_count(start, modes, scope)
                self.handle_emulator_retention_normal(start, modes, scope)

    def start_import_engine_data(self, start, modes, stats):
        """,
        处理引擎
        """
        print "start import engine data"
        self.reset_cache()
        scopes = ScopeEngine.objects.filter(modes=modes)
        for scope in scopes:
            if not stats or "other" not in stats:
                self.hanle_engine_data(start, modes, scope)
            if not stats or "other" in stats:
                self.handle_engine_other_data(start, modes, scope)
            # if "acc" in stats:
            #     self.handle_engine_acc_data(start, modes, scope)


    def start_import_app_data(self, start, modes):
        """
        处理total app
        """
        print "start import total app data"
        self.reset_cache()
        scopes = ScopeAppTotal.objects.filter(modes=modes)
        for scope in scopes:
            self.handle_app_total_data(start, modes, scope)


    def start_import_app_package_name_data(self, start, modes):
        """
        处理非total app
        """
        print "start import app data"
        self.reset_cache()
        scopes = ScopeAppPackagename.objects.filter(modes=modes)
        for scope in scopes:
            self.handle_app_package_name_data(start, modes, scope)

    def handle_app_package_name_data(self, start, modes, scope):
        print "scope id:",scope.pk
        a = AppPackagenameStat()
        a.scope_id = scope.pk
        a.result_date = start.strftime("%Y-%m-%d")
        a.daily_user_init = self.get_app_package_name_daily_user_init(start, modes, scope)
        a.daily_user_init_count = self.get_app_package_name_daily_user_init_count(start, modes, scope)
        a.daily_init_fail = self.get_app_package_name_daily_init_fail(start, modes, scope)
        a.daily_init_fail_count = self.get_app_package_name_daily_init_fail_count(start, modes, scope)

        a.daily_install = self.get_app_total_package_name_install(start, modes, scope)
        a.daily_install_count = self.get_app_total_package_name_install_count(start, modes, scope)
        a.daily_install_fail = self.get_app_total_package_name_install_fail(start, modes, scope)
        a.daily_install_fail_count = self.get_app_package_name_daily_install_fail_count(start, modes, scope)

        a.daily_download = self.get_app_total_package_name_download(start, modes, scope)
        a.daily_download_count = self.get_app_package_name_daily_download_count(start, modes, scope)
        a.daily_download_fail = self.get_app_package_name_daily_download_fail(start, modes, scope)
        a.daily_download_fail_count = self.get_app_package_name_daily_download_fail_count(start, modes, scope)

        a.daily_install_from_download = self.get_app_package_name_daily_install_from_download(start, modes, scope)
        a.daily_install_from_download_count = self.get_app_package_name_daily_install_from_download_count(start, modes, scope)
        a.daily_install_from_download_fail = self.get_app_package_name_daily_install_from_download_fail(start, modes, scope)
        a.daily_install_from_download_fail_count = self.get_app_package_name_daily_install_from_download_fail_count(start, modes, scope)

        try:
            a.save()
        except Exception, e:
            print e
            print a


    def handle_app_total_data(self, start, modes, scope):
        print "scope id:",scope.pk
        a = AppTotalStat()
        a.scope_id = scope.pk
        a.result_date = start.strftime("%Y-%m-%d")
        a.daily_user_init = self.get_app_total_daily_user_init(start, modes, scope)
        a.daily_user_init_count = self.get_app_total_daily_user_init_count(start, modes, scope)
        a.daily_init_fail = self.get_app_total_daily_init_fail(start, modes, scope)
        a.daily_init_fail_count = self.get_app_total_daily_init_fail_count(start, modes, scope)

        a.daily_install = self.get_app_total_daily_install(start, modes, scope)
        a.daily_install_count = self.get_app_total_daily_install_count(start, modes, scope)
        a.daily_install_fail = self.get_app_total_daily_install_fail(start, modes, scope)
        a.daily_install_fail_count = self.get_app_total_daily_install_fail_count(start, modes, scope)

        a.daily_download = self.get_app_total_daily_download(start, modes, scope)
        a.daily_download_count = self.get_app_total_daily_download_count(start, modes, scope)
        a.daily_download_fail = self.get_app_total_daily_download_fail(start, modes, scope)
        a.daily_download_fail_count = self.get_app_total_daily_download_fail_count(start, modes, scope)

        a.daily_install_from_download = self.get_app_total_daily_install_from_download(start, modes, scope)
        a.daily_install_from_download_count = self.get_app_total_daily_install_from_download_count(start, modes, scope)
        a.daily_install_from_download_fail = self.get_app_total_daily_install_from_download_fail(start, modes, scope)
        a.daily_install_from_download_fail_count = self.get_app_total_daily_install_from_download_fail_count(start, modes, scope)

        try:
            a.save()
        except Exception, e:
            print e
            print a

    def start_import_emulator_data(self, start, modes, stats):
        """
        处理模拟器
        """
        print "start import emulator data"
        self.reset_cache()
        scopes = ScopeEmulator.objects.filter(modes=modes)
        for scope in scopes:
            print "scope id:",scope.pk
            if not stats or "other" not in stats:
                self.handle_emulator_data(start, modes, scope)
            if not stats or "other" in stats:
                self.handle_emulator_other_data(start, modes, scope)
            # if "acc" in stats:
            #     self.handle_emulator_acc_data(start, modes, scope)

    def handle_emulator_other_data(self, start, modes, scope):
        last_day = start - datetime.timedelta(days=1)

        if EmulatorStats.objects.filter(result_date=last_day,scope_id=scope.pk).exists():
            last_day_a = EmulatorStats.objects.get(result_date=last_day,scope_id=scope.pk)

            last_day_a.next_day_uninstall_success_user = self.get_emulator_next_day_uninstall_success_user(last_day, modes, scope)
            last_day_a.next_day_uninstall_success_user_base_on_engine = self.get_emulator_next_day_uninstall_success_user_base_on_engine(last_day, modes, scope)
            last_day_a.retention_2 = self.get_emulator_retention_2(last_day, modes, scope)
            last_day_a.retention_2_base_on_engine = self.get_emulator_retention_2_base_on_engine(last_day, modes, scope)
            last_day_a.retention_2_base_on_engine2 = self.get_emulator_retention_2_base_on_engine2(last_day, modes, scope)
            try:
                last_day_a.save()
            except Exception, e:
                print e
                print last_day_a

        last_7_day = start - datetime.timedelta(days=6)
        if EmulatorStats.objects.filter(result_date=last_7_day,scope_id=scope.pk).exists():
            last_7_day_a = EmulatorStats.objects.get(result_date=last_7_day,scope_id=scope.pk)
            """ next_day_uninstall_success_user """
            last_7_day_a.retention_7 = self.get_emulator_retention_7(last_7_day, modes, scope)
            """ retention_7_base_on_engine_normal """
            last_7_day_a.retention_7_base_on_engine_normal = self.get_emulator_retention_7_base_on_engine_normal(last_7_day, modes, scope)
            try:
                last_7_day_a.save()
            except Exception, e:
                print e
                print last_7_day_a


        last_14_day = start - datetime.timedelta(days=13)
        if EmulatorStats.objects.filter(result_date=last_14_day,scope_id=scope.pk).exists():
            last_14_day_a = EmulatorStats.objects.get(result_date=last_14_day,scope_id=scope.pk)
            """ retention_14_base_on_engine_normal """
            last_14_day_a.retention_14_base_on_engine_normal = self.get_emulator_retention_14_base_on_engine_normal(last_14_day, modes, scope)
            try:
                last_14_day_a.save()
            except Exception, e:
                print e
                print last_14_day_a


    def handle_engine_other_data(self, start, modes, scope):
        last_day = start - datetime.timedelta(days=1)
        if EngineStats.objects.filter(result_date=last_day,scope_id=scope.pk).exists():
            last_day_a = EngineStats.objects.get(result_date=last_day,scope_id=scope.pk)
            last_day_a.first_init_no_update_and_next_day_uninstall_emulator_user = self.get_first_init_no_update_and_next_day_uninstall_emulator_user(last_day, modes, scope)
            try:
                last_day_a.save()
            except Exception, e:
                print e
                print last_day_a

    def handle_engine_acc_data(self, start, modes, scope):
        if EngineStats.objects.filter(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
            a = EngineStats.objects.get(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk)
            last_day = start - datetime.timedelta(days=1)
            if EngineStats.objects.filter(result_date=last_day.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
                last_day_a = EngineStats.objects.get(result_date=last_day.strftime("%Y-%m-%d"),scope_id=scope.pk)
                a.acc_install_success_user = a.install_success_user + last_day_a.acc_install_success_user
                a.save()

    def handle_emulator_acc_data(self, start, modes, scope):
        if EmulatorStats.objects.filter(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
            a = EmulatorStats.objects.get(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk)
            last_day = start - datetime.timedelta(days=1)
            if EmulatorStats.objects.filter(result_date=last_day.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
                last_day_a = EmulatorStats.objects.get(result_date=last_day.strftime("%Y-%m-%d"),scope_id=scope.pk)
                a.acc_install_success_user = a.install_success_user + last_day_a.acc_install_success_user
                a.save()

    def handle_emulator_init_success_count(self, start, modes, scope):
        """
        单独计算用户每日启动次数总数
        """
        if EmulatorStats.objects.filter(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
            a = EmulatorStats.objects.get(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk)
            a.init_success_count = self.get_emulator_init_success_count(start, modes, scope)
            try:
                a.save()
            except Exception, e:
                print e

    def handle_emulator_retention_normal(self, start, modes, scope):
        """
        单独跑普通留存7日 14日
        """
        last_7_day = start - datetime.timedelta(days=6)
        if EmulatorStats.objects.filter(result_date=last_7_day,scope_id=scope.pk).exists():
            last_7_day_a = EmulatorStats.objects.get(result_date=last_7_day,scope_id=scope.pk)
            last_7_day_a.retention_7_base_on_engine_normal = self.get_emulator_retention_7_base_on_engine_normal(last_7_day,modes,scope)
            try:
                last_7_day_a.save()
            except Exception, e:
                print e

        last_14_day = start - datetime.timedelta(days=13)
        if EmulatorStats.objects.filter(result_date=last_14_day,scope_id=scope.pk).exists():
            last_14_day_a = EmulatorStats.objects.get(result_date=last_14_day,scope_id=scope.pk)
            last_14_day_a.retention_14_base_on_engine_normal = self.get_emulator_retention_14_base_on_engine_normal(last_14_day,modes,scope)
            try:
                last_14_day_a.save()
            except Exception, e:
                print e

    def handle_emulator_data(self, start, modes, scope):
        a = EmulatorStats()
        a.scope_id = scope.pk
        a.result_date = start.strftime("%Y-%m-%d")
        a.install_success_user = self.get_emulator_install_success_user(start, modes, scope)
        a.acc_install_success_user = self.get_emulator_last_day_acc_install_success_user(start, modes, scope) + a.install_success_user
        a.install_fail_user = self.get_emulator_install_fail_user(start, modes, scope)
        a.uninstall_success_user = self.get_emulator_uninstall_success_user(start, modes, scope)
        # a.next_day_uninstall_success_user = self.get_emulator_next_day_uninstall_success_user(start, modes, scope)
        a.uninstall_success_user_base_on_engine = self.get_emulator_uninstall_success_user_base_on_engine(start, modes, scope)
        # a.next_day_uninstall_success_user_base_on_engine = self.get_emulator_next_day_uninstall_success_user_base_on_engine(start, modes, scope)
        a.install_and_init_success_user = self.get_emulator_install_and_init_success_user(start, modes, scope)
        # a.retention_2 = self.get_emulator_retention_2(start, modes, scope)
        # a.retention_7 = self.get_emulator_retention_7(start, modes, scope)
        a.engine_install_and_init_success_user = self.get_emulator_engine_install_and_init_success_user(start, modes, scope) #基于引擎的次日留存分母
        # a.retention_2_base_on_engine = self.get_emulator_retention_2_base_on_engine(start, modes, scope)
        a.engine_install_and_init_success_user2 = self.get_emulator_engine_install_and_init_success_user2(start, modes, scope)
        # a.retention_2_base_on_engine2 = self.get_emulator_retention_2_base_on_engine2(start, modes, scope)

        a.init_success_user = self.get_emulator_init_success_user(start, modes, scope)
        a.init_success_count = self.get_emulator_init_success_count(start, modes, scope)
        a.init_fail_user = self.get_emulator_init_fail_user(start, modes, scope)
        a.init_user = self.get_emulator_init_user(start, modes, scope)


        try:
            a.save()
        except Exception, e:
            print e
            print a




    def hanle_engine_data(self, start, modes, scope):
        # print "hanle engine data"
        print "scope id:",scope.pk
        a = EngineStats()
        a.scope_id = scope.pk
        a.result_date = start.strftime("%Y-%m-%d")
        a.install_success_user = self.get_install_success_user(start, modes, scope)
        a.acc_install_success_user = self.get_engine_last_day_acc_install_success_user(start, modes, scope) + a.install_success_user
        a.install_fail_user = self.get_install_fail_user(start, modes, scope)
        a.download_begin_user = self.get_download_begin_user(start, modes, scope)
        a.download_ok_user = self.get_download_ok_user(start, modes, scope)

        a.init_success_user = self.get_init_success_user(start, modes, scope)
        a.init_not_success_user = self.get_init_not_success_user(start, modes, scope)
        a.init_fail_user = self.get_init_fail_user(start, modes, scope)
        a.first_init_success_user = self.get_first_init_success_user(start, modes, scope)
        a.first_init_fail_user = self.get_first_init_fail_user(start, modes, scope)
        a.update_success_user = self.get_update_success_user(start, modes, scope)
        a.update_fail_user = self.get_update_fail_user(start, modes, scope)
        a.update_init_success_user = self.get_update_init_success_user(start, modes, scope)
        a.update_init_fail_user = self.get_update_init_fail_user(start, modes, scope)
        a.first_init_no_update_success_user = self.get_first_init_no_update_success_user(start, modes, scope)
        a.first_init_no_update_and_uninstall_emulator_user = self.get_first_init_no_update_and_uninstall_emulator_user(start, modes, scope)
        # a.first_init_no_update_and_next_day_uninstall_emulator_user = self.get_first_init_no_update_and_next_day_uninstall_emulator_user(start, modes, scope)

        try:
            a.save()
        except Exception, e:
            print e
            print a

    def get_count_by_ckey(self, ckey, sql, scope):
        res = self.cache.get(ckey, None)
        if res is None:
            # print sql
            res = {}
            cursor = connection.cursor()
            cursor.execute(sql)
            data = cursor.fetchall()
            for i in data:
                key,val = ','.join(i[1:]),int(i[0])
                res[key] = val
            self.cache[ckey] = res
        key = self.group_keys(scope)
        count = res.get(key,0)
        return count

    def get_update_init_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='update_init' AND q.status = 'fail'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count

    def get_first_init_no_update_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='first_init' AND q.status = 'success'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count

    def get_first_init_no_update_and_uninstall_emulator_user(self, start, modes, scope):
        """
        算法改为当日引擎安装且(引擎first_init或引擎init)且卸载客户端的人数
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)COUNT %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op IN ('init','first_init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='uninstall' AND STATUS= 'success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_first_init_no_update_and_next_day_uninstall_emulator_user(self, start, modes, scope):
        """
        算法改为当日引擎安装且(引擎first_init或引擎init)且卸载次日客户端的人数
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)COUNT %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op IN ('init','first_init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='uninstall' AND STATUS='success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), get_next_day(start).strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count




    def get_update_init_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='update_init' AND q.status = 'success'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_update_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='update' AND q.status = 'fail'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_update_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='update' AND q.status = 'success'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_first_init_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op in ('first_init','update_init') AND q.status = 'fail'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_first_init_success_user(self, start, modes, scope):
        """
        首次启动用户，op="first_init" update_init
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
        #     sql = """
        # SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op in ('first_init','update_init') AND q.status = 'success'
        # %s
        #     """ % (self.grp(group), from_sql, groupby)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op in ('first_init') AND q.status = 'success'
        %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_init_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='init' AND q.status = 'fail'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count



    def get_init_not_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='init' AND q.status != 'success'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count



    def get_init_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineactivity(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op in ('first_init','init','update_init') AND q.status = 'success'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_download_ok_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineinstall(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='install' AND q.status = 'download_ok'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count



    def get_download_begin_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineinstall(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='install' AND q.status = 'download_begin'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count


    def get_install_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineinstall(start)
            sql = """
        SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='install' AND q.status = 'fail'
        %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            count = self.get_count_by_ckey(ckey, sql, scope)
            return count

    def get_from_sql_emulatorinstall(self, start):
        sql = """
        (SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatorinstall a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
        """ % start.strftime("%Y-%m-%d")
        return sql

    def get_from_sql_emulatoractivity(self, start):
        sql = """
        (SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatoractivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
        """ % start.strftime("%Y-%m-%d")
        return sql

    def get_from_sql_engineinstall(self, start):
        sql = """
        (SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineinstall a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
        """ % start.strftime("%Y-%m-%d")
        return sql

    def get_from_sql_engineactivity(self, start):
        sql = """
        (SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
        """ % start.strftime("%Y-%m-%d")
        return sql

    def get_emulator_install_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_emulatorinstall(start)
            sql = """
            SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='install' AND q.status = 'success'
            %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_init_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_emulatoractivity(start)
            sql = """
            SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='init' AND q.status = 'fail'
            %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_init_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_emulatoractivity(start)
            sql = """
            SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='init'
            %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_init_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_emulatoractivity(start)
            sql = """
            SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='init' AND q.status = 'success'
            %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_init_success_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_emulatoractivity(start)
            sql = """
            SELECT COUNT(1)count %s FROM %s WHERE q.op='init' AND q.status = 'success'
            %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_install_and_init_success_user(self, start, modes, scope):
        """
        安装并且启动的用户数量
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatoractivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='init' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        ) %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_retention_2(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatoractivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='init' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), get_next_day(start).strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_retention_2_base_on_engine2(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op in ('init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN (SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), get_next_day(start).strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_retention_2_base_on_engine(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op in ('init','first_init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN (SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), get_next_day(start).strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_engine_install_and_init_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op in ('init','first_init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        ) %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_retention_7_base_on_engine_normal(self, start, modes, scope):
        """
        当日引擎安装并启动成功且在后面第七天模拟器启动成功的人数
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)COUNT %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op IN ('init','first_init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN (SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), get_next_number_day(start, 6).strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_retention_14_base_on_engine_normal(self, start, modes, scope):
        """
        当日引擎安装并启动成功且在后面第十四天模拟器启动成功的人数
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)COUNT %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op IN ('init','first_init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN (SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), get_next_number_day(start, 13).strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_engine_install_and_init_success_user2(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_engineactivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op in ('init') AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        ) %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_retention_7(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatoractivity a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='init' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
        )
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        AND c.guid IN ( SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS= 'success')
        %s
            """ % (self.grp(group),
                   start.strftime("%Y-%m-%d"),
                   start.strftime("%Y-%m-%d"),
                   get_next_number_day(start,1).strftime("%Y-%m-%d"),
                   get_next_number_day(start,2).strftime("%Y-%m-%d"),
                   get_next_number_day(start,3).strftime("%Y-%m-%d"),
                   get_next_number_day(start,4).strftime("%Y-%m-%d"),
                   get_next_number_day(start,5).strftime("%Y-%m-%d"),
                   get_next_number_day(start,6).strftime("%Y-%m-%d"),
                   groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_next_day_uninstall_success_user_base_on_engine(self, start, modes, scope):
        """
        基于引擎的次日卸人数
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatorinstall a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='uninstall' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineactivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS='success'
        ) %s
            """ % (self.grp(group), get_next_day(start).strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_uninstall_success_user_base_on_engine(self, start, modes, scope):
        """
        基于引擎的当日卸载率
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
	    SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatorinstall a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='uninstall' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_engineactivity WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS='success'
        ) %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_emulator_next_day_uninstall_success_user(self, start, modes, scope):
        """
        次日卸载用户数量
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
        SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatorinstall a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='uninstall' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS= 'success'
        ) %s
            """ % (self.grp(group), get_next_day(start).strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_uninstall_success_user(self, start, modes, scope):
        """
        卸载用户，当日安装并卸载的用户数量
        """
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
        SELECT COUNT(DISTINCT c.guid)count %s FROM (
	    SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver
        FROM datastats_emulatorinstall a
        INNER JOIN view_guid_osver b ON a.guid = b.guid
        WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')c WHERE c.op='uninstall' AND c.status='success' AND c.guid IN (
        SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS= 'success'
        ) %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), start.strftime("%Y-%m-%d"), groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_emulator_install_fail_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_emulatorinstall(start)
            sql = """
            SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='install' AND q.status = 'fail'
            %s
            """ % (self.grp(group), from_sql, groupby)
            # print sql
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_engine_last_day_acc_install_success_user(self, start, modes, scope):
        count = 0
        start = start - datetime.timedelta(days=1)
        if modes == "day":
            if EngineStats.objects.filter(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
                count = EngineStats.objects.get(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).acc_install_success_user
            return count

    def get_emulator_last_day_acc_install_success_user(self, start, modes, scope):
        count = 0
        start = start - datetime.timedelta(days=1)
        if modes == "day":
            if EmulatorStats.objects.filter(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).exists():
                count = EmulatorStats.objects.get(result_date=start.strftime("%Y-%m-%d"),scope_id=scope.pk).acc_install_success_user
            return count

    def get_install_success_user(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            from_sql = self.get_from_sql_engineinstall(start)
            sql = """
            SELECT COUNT(DISTINCT q.guid)count %s FROM %s WHERE q.op='install' AND q.status = 'success'
            %s
            """ % (self.grp(group), from_sql, groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            # print ckey
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count




    def get_app_total_daily_install_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_install_fail_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_install_fail(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_download_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_download_fail(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_download_fail_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_install_from_download_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install_from_download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_install_from_download_fail(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install_from_download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_app_total_daily_install_from_download_fail_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install_from_download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count

    def get_app_total_daily_install_from_download(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install_from_download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_download(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_install(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appinstall a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='install' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_init_fail(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appactivity a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='init' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_total_daily_init_fail_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appactivity a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='init' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count




    def get_app_total_daily_user_init_count(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appactivity a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='init' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_install_from_download_fail_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install_from_download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_install_from_download_fail(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install_from_download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_install_from_download_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install_from_download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count

    def get_app_package_name_daily_install_from_download(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install_from_download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count



    def get_app_package_name_daily_download_fail_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_download_fail(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='download' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_download_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_total_package_name_download(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='download' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_install_fail_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count



    def get_app_total_package_name_install_fail(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count

    def get_app_total_package_name_install_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_total_package_name_install(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appinstall
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='install' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_package_name_daily_init_fail_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appactivity
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count

    def get_app_package_name_daily_init_fail(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appactivity
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS='fail'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count

    def get_app_package_name_daily_user_init_count(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(1) %s
            FROM datastats_appactivity
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count

    def get_app_package_name_daily_user_init(self, start, modes, scope):
        if modes == "day":
            group = "package_name"
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid) %s
            FROM datastats_appactivity
            WHERE DATE_FORMAT(DATETIME,'%%Y-%%m-%%d')='%s' AND op='init' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            # key = self.group_keys(scope)
            key = scope.package_name
            count = res.get(key,0)
            return count


    def get_app_total_daily_user_init(self, start, modes, scope):
        if modes == "day":
            group = self.make_group(scope)
            groupby = ('GROUP BY %s' % group) if group else ''
            sql = """
            SELECT COUNT(DISTINCT guid)count %s FROM(
            SELECT a.guid,a.op,a.version,a.status,a.channel,b.osver FROM datastats_appactivity a
            INNER JOIN view_guid_osver b ON a.guid = b.guid
            WHERE DATE_FORMAT(a.datetime,'%%Y-%%m-%%d')='%s')q
            WHERE op='init' AND STATUS='success'
            %s
            """ % (self.grp(group), start.strftime("%Y-%m-%d"), groupby)
            ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
            res = self.cache.get(ckey, None)
            if res is None:
                res = {}
                cursor = connection.cursor()
                cursor.execute(sql)
                data = cursor.fetchall()
                for i in data:
                    key,val = ','.join(i[1:]),int(i[0])
                    res[key] = val
                self.cache[ckey] = res
            key = self.group_keys(scope)
            count = res.get(key,0)
            return count


if __name__ == '__main__':
    s = Scope2db()

    try:
        opts,args = getopt.getopt(sys.argv[1:],'',['modes=','stats=','start=','end='])
    except getopt.GetoptError,e:
        logging.error('%s\n',str(e),exc_info=True)
        sys.exit(2)
    modes,stats,start,end = None,None,None,None
    for o, a in opts:
        if o == '--modes':
            mode = a
        if o == '--stats':
            stats = a
        if o == '--start':
            start = datetime.datetime.strptime(a,'%Y-%m-%d')
        if o == '--end':
            end = datetime.datetime.strptime(a,'%Y-%m-%d')
    if not modes:
        modes = 'day'
    assert modes in ('day','week','month')
    stats = stats.split(',') if stats else None
    now = datetime.datetime.now()
    if not start or not end:
        today = datetime.date.today()
        yest = today + datetime.timedelta(days=-1)
        start = yest
        end = today

    if start and end:
        while start < end:
            print 'start...',modes,start,stats
            s.start_importing(modes,start,stats)
            print 'processed...',modes,start,stats
            start += datetime.timedelta(days=1)
    else:
        s.start_importing(modes,None,stats)
    print modes,datetime.datetime.now()-now




