# coding=utf-8
# from django.core.wsgi import get_wsgi_application
# application = get_wsgi_application()

import datetime
import os
import sys
import getopt


# if ENVIRONMENT == "aliyun_test":


sys.path.append('/var/www/html/bst-datastats-zh')

from django.db import connection, transaction
from django.core.wsgi import get_wsgi_application
from django.db.models import Sum,Count
from bst_server.settings import ENVIRONMENT

from util.data_lib import check_package_name, app_total_data_merge
from util.appcenter import cal_day

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()

from datastats.models import EngineInstall,EngineActivity,EmulatorInstall,EmulatorActivity, ResultEngineInstall, \
    ResultEngineActivity, ResultEmulatorInstall, ResultEmulatorActivity, ResultRetention, ResultEmulatorSession, \
    AppActivity, AppInstall, ResultAppInstall, ResultAppActivity, MidResultInstallInitEmulator, ResultAppSession, \
    ResultEmulatorUninstallCount, ResultEmulatorUninstallNextDayCount, ResultEmulatorInstallCount, \
    MidResultInstallInitEngine, ResultRetentionEngine, ResultEngineDAU, ResultEngineUninstallRate, ResultAppTotal, \
    ResultAppLocalTop500

from generaldata2db import GeneralData2db
from user_computer_info_data import UserComputerInfo


class Data2db(object):
    """
    数据分析，将计算结果存放在数据库中
    """
    def _find_game_name_by_package_name(self, package_name):
        """
        获取游戏名称
        """
        if not package_name:
            return ""
        else:
            cursor = connection.cursor()
            cursor.execute("select game_name from view_package_name where package_name='%s'" % package_name)
            row = cursor.fetchone()
            if row and row[0]:
                return row[0]

    def handle_engine_dau(self, start, end):
        """
        用于计算engine dau特殊算法
        """
        # end = start + datetime.timedelta(days=-1)
        # data = EngineActivity.objects.filter(datetime__gte=start.strftime('%Y-%m-%d'), datetime__lt=end.strftime('%Y-%m-%d'),op__in=["init","first_init"],status="success").query
        # data.group_by = ["version"]
        print "handle_engine_dau"
        sql = """
        SELECT id, "%s" AS result_date, COUNT(DISTINCT guid)dst_count,COUNT(guid)count,VERSION,channel FROM datastats_engineactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op IN ('init','first_init') AND
        status = 'success' GROUP BY VERSION,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        # print sql
        data = EngineActivity.objects.raw(sql)
        for i in data:
            ResultEngineDAU.insert_a_data(i.result_date,i.version,i.dst_count,i.count,i.channel)


    def handle_engine_install(self, start, end):
        print "handle_engine_install"
        sql = """
          SELECT "%s" AS result_date, id, COUNT(1)count, COUNT(DISTINCT guid)dst_count, op,VERSION, STATUS, channel
          FROM datastats_engineinstall
          WHERE DATETIME >="%s" AND DATETIME<"%s"
          GROUP BY op,VERSION,status,channel
        """ % (start, start, end)
        # print sql
        data = EngineInstall.objects.raw(sql)
        for i in data:
            ResultEngineInstall.insert_a_data(i.result_date,i.op,i.version,i.status,i.count,i.dst_count,i.channel)


    def handle_engine_activity(self, start, end):
        print "handle_engine_activity"
        sql = """
          SELECT "%s" AS result_date, id, COUNT(1)count, COUNT(DISTINCT guid)dst_count, op,VERSION, STATUS,channel
          FROM datastats_engineactivity
          WHERE DATETIME >="%s" AND DATETIME<"%s"
          GROUP BY op,VERSION,status,channel
        """ % (start, start, end)
        # print sql
        data = EngineActivity.objects.raw(sql)
        for i in data:
            ResultEngineActivity.insert_a_data(i.result_date,i.op,i.version,i.status,i.count,i.dst_count,i.channel)


    def handle_emulator_install(self, start, end):
        print "handle_emulator_install"
        sql = """
          SELECT "%s" AS result_date, id, COUNT(1)count, COUNT(DISTINCT guid)dst_count, op,VERSION, STATUS,channel
          FROM datastats_emulatorinstall
          WHERE DATETIME >="%s" AND DATETIME<"%s"
          GROUP BY op,VERSION,status,channel
        """ % (start, start, end)
        # print sql
        data = EmulatorInstall.objects.raw(sql)

        for i in data:
            ResultEmulatorInstall.insert_a_data(i.result_date,i.op,i.version,i.status,i.count,i.dst_count,i.channel)

    def handle_emulator_activity(self, start, end):
        print "handle_emulator_activity"
        sql = """
          SELECT "%s" AS result_date, id, COUNT(1)count, COUNT(DISTINCT guid)dst_count, op,VERSION, STATUS,channel
          FROM datastats_emulatoractivity
          WHERE DATETIME >="%s" AND DATETIME<"%s"
          GROUP BY op,VERSION,status,channel
        """ % (start, start, end)
        # print sql
        data = EmulatorActivity.objects.raw(sql)
        for i in data:
            ResultEmulatorActivity.insert_a_data(i.result_date,i.op,i.version,i.status,i.count,i.dst_count,i.channel)

    def handle_total_app(self, start):
        """
        app全部数据
        """
        end = start + datetime.timedelta(days=1)
        print "handle_total_app"
        #APP启动成功用户数 APP启动成功次数
        #SELECT COUNT(1),COUNT(DISTINCT guid), VERSION FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,"%Y-%m-%d") = "2016-07-11" AND op="init" AND STATUS ="success" GROUP BY VERSION
        #todo: channel
        app_init_success = AppActivity.objects.filter(op="init",status="success",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('version').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
        app_init_fail = AppActivity.objects.filter(op="init",status="fail",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('version').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
        app_install_success = AppInstall.objects.filter(op="install",status="success",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('version').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
        app_install_fail = AppInstall.objects.filter(op="install",status="fail",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('version').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
        app_download_success = AppInstall.objects.filter(op="download",status="success",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('version').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
        app_download_fail = AppInstall.objects.filter(op="download",status="fail",datetime__gte=start.strftime("%Y-%m-%d"),datetime__lt=end.strftime("%Y-%m-%d")).values('version').annotate(count=Count(1),dst_count=Count('guid',distinct=True))
        data = app_total_data_merge(app_init_success=app_init_success,
                                    app_init_fail=app_init_fail,
                                    app_install_success=app_install_success,
                                    app_install_fail=app_install_fail,
                                    app_download_success=app_download_success,
                                    app_download_fail=app_download_fail
                                    )
        for i in data:
            ResultAppTotal.insert_a_data(result_date=start.strftime("%Y-%m-%d"),
                                         version=i.get("version",""),
                                        daily_user_init = i.get("app_init_success",0),
                                        daily_user_init_count = i.get("app_init_success_count",0),
                                        daily_init_fail = i.get("app_init_fail",0),
                                        daily_init_fail_count = i.get("app_init_fail_count",0),
                                        daily_install = i.get("app_install_success",0),
                                        daily_install_count = i.get("app_install_success_count",0),
                                        daily_install_fail = i.get("app_install_fail",0),
                                        daily_install_fail_count = i.get("app_install_fail_count",0),
                                        daily_download = i.get("app_download_success",0),
                                        daily_download_count = i.get("app_download_success_count",0),
                                        daily_download_fail = i.get("app_download_fail",0),
                                        daily_download_fail_count = i.get("app_download_fail_count",0)
                                        )


    def handle_app_install(self, start, end):
        print "handle_app_install"
        sql = """
          SELECT "%s" AS result_date, id, COUNT(1)count, COUNT(DISTINCT guid)dst_count, op,VERSION, STATUS, package_name, channel
          FROM datastats_appinstall
          WHERE DATETIME >="%s" AND DATETIME<"%s"
          GROUP BY op,VERSION,status,package_name,channel
        """ % (start, start, end)
        # print sql
        data = AppInstall.objects.raw(sql)

        for i in data:
            game_name = self._find_game_name_by_package_name(i.package_name)
            try:
                ResultAppInstall.insert_a_data(i.result_date,i.op,i.version,i.status,i.count,i.dst_count,i.package_name, game_name, i.channel)
            except Exception as e:
                print e
                print "handle_app_install"

    def handle_app_activity(self, start, end):
        print "handle_app_activity"
        sql = """
          SELECT "%s" AS result_date, id, COUNT(1)count, COUNT(DISTINCT guid)dst_count, op,VERSION, STATUS, package_name, channel
          FROM datastats_appactivity
          WHERE DATETIME >="%s" AND DATETIME<"%s"
          GROUP BY op,VERSION,status,package_name,channel
        """ % (start, start, end)
        # print sql
        data = AppActivity.objects.raw(sql)
        for i in data:
            # game_name = self._find_game_name_by_package_name(i.package_name)
            try:
                ResultAppActivity.insert_a_data(i.result_date,i.op,i.version,i.status,i.count,i.dst_count,i.package_name, "",i.channel)
            except Exception as e:
                print e
                print "handle_app_activity"

    def delete_one_day_all_data(self, table_name, date):
        from django.db import connection, transaction
        cursor = connection.cursor()
        cursor.execute("delete from %s where result_date='%s'"% (table_name,date))

        # transaction.commit_unless_managed()

    def delete_one_day_all_result_data(self, date):
        print "deleting data: %s" % date
        result_table_names = ["datastats_resultemulatoractivity",
                              "datastats_resultemulatorinstall",
                              "datastats_resultemulatorsession",
                              "datastats_resultengineactivity",
                              "datastats_resultengineinstall",
                              "datastats_resultretention",
                              ]
        cursor = connection.cursor()
        for table in result_table_names:
            cursor.execute("delete from %s where result_date='%s'"% (table,date))

    def make_init_install_sql(self, start, flag, ret_day):
        if flag == "emulator":
            table = "datastats_emulatoractivity"

        today = datetime.date.today()
        _cal_day = start
        # start = datetime.datetime.strptime(start.strftime('%Y-%m-%d'),'%Y-%m-%d')
        print "cal_day:",cal_day
        # print today

        start += datetime.timedelta(days=1)
        ret_days = []
        end = start + datetime.timedelta(days=ret_day-1)
        while (start<end):
            ret_days.append(start)
            start += datetime.timedelta(days=1)

        print "ret_days:",ret_days
        sql_select = """
    SELECT "%s" AS retention,"%s" AS result_date,id,COUNT(DISTINCT guid)count,VERSION FROM %s
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s'
        """ % (ret_day,_cal_day.strftime('%Y-%m-%d'), table, _cal_day.strftime('%Y-%m-%d'))

        sql_and_install = """
        AND guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE op="install" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
        """ % (_cal_day.strftime('%Y-%m-%d'))

        sql_group_by = "GROUP BY VERSION"
        sql = sql_select + sql_and_install + sql_group_by
        return sql


    def make_engine_retention_sql(self, start, ret_day):
        # table = "datastats_engineactivity"

        today = datetime.date.today()
        _cal_day = start
        # start = datetime.datetime.strptime(start.strftime('%Y-%m-%d'),'%Y-%m-%d')
        print "cal_day:",cal_day
        # print today

        start += datetime.timedelta(days=1)
        ret_days = []
        end = start + datetime.timedelta(days=ret_day-1)
        while (start<end):
            ret_days.append(start)
            start += datetime.timedelta(days=1)

        print "ret_days:",ret_days
        sql_select = """
    SELECT "%s" AS retention,"%s" AS result_date,id,COUNT(DISTINCT guid)count,VERSION,channel FROM %s
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s'
        """ % (ret_day,_cal_day.strftime('%Y-%m-%d'), "datastats_engineactivity", _cal_day.strftime('%Y-%m-%d'))

        sql_and_install = """
        AND guid IN (SELECT DISTINCT guid FROM %s WHERE op="install" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
        """ % ("datastats_engineinstall",_cal_day.strftime('%Y-%m-%d'))

        sql_and = """
    AND guid IN (SELECT DISTINCT guid FROM %s
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
        """
        sql_group_by = "GROUP BY VERSION,channel"
        sql = sql_select + sql_and_install
        for day in ret_days:
            sql += sql_and % ("datastats_emulatoractivity", day.strftime('%Y-%m-%d'))
        sql += sql_group_by
        return sql


    def make_retention_sql(self, start, flag, ret_day):
        if flag == "emulator":
            table = "datastats_emulatoractivity"

        today = datetime.date.today()
        _cal_day = start
        # start = datetime.datetime.strptime(start.strftime('%Y-%m-%d'),'%Y-%m-%d')
        print "cal_day:",cal_day
        # print today

        start += datetime.timedelta(days=1)
        ret_days = []
        end = start + datetime.timedelta(days=ret_day-1)
        while (start<end):
            ret_days.append(start)
            start += datetime.timedelta(days=1)

        print "ret_days:",ret_days
        sql_select = """
    SELECT "%s" AS retention,"%s" AS result_date,id,COUNT(DISTINCT guid)count,VERSION, channel FROM %s
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s'
        """ % (ret_day,_cal_day.strftime('%Y-%m-%d'), table, _cal_day.strftime('%Y-%m-%d'))

        sql_and_install = """
        AND guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE op="install" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
        """ % (_cal_day.strftime('%Y-%m-%d'))

        sql_and = """
    AND guid IN (SELECT DISTINCT guid FROM %s
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
        """
        sql_group_by = "GROUP BY VERSION, channel"
        sql = sql_select + sql_and_install
        for day in ret_days:
            sql += sql_and % (table, day.strftime('%Y-%m-%d'))
        sql += sql_group_by
        return sql


    def day_retention_engine(self, start, end, ret_day):
        start, end = cal_day(start, end, days=ret_day)
        sql = self.make_engine_retention_sql(start,ret_day)
        # print sql
        data = EngineActivity.objects.raw(sql)
        for i in data:
            print "day_retention_engine:",i.retention, i.result_date, i.count, i.version, i.channel
            ResultRetentionEngine.insert_a_data(i.result_date,i.version,i.count,i.retention, i.channel)

    def day_retention(self, start, end, flag, ret_day):
        start, end = cal_day(start, end, days=ret_day)
        sql = self.make_retention_sql(start, flag, ret_day)
        # print sql
        data = EmulatorActivity.objects.raw(sql)
        for i in data:
            print "day_retention:",i.retention, i.result_date, i.count, i.version, i.channel
            ResultRetention.insert_a_data(i.result_date,i.version,i.count,i.retention,i.channel)


    def handle_install_and_init_engine(self, start, end):
        print "handle_install_and_init_engine"
        sql = """
    SELECT '%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,VERSION,channel FROM datastats_engineactivity
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s'
    AND guid IN (SELECT DISTINCT guid FROM datastats_engineinstall WHERE op="install" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
    GROUP BY VERSION,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        # print sql
        data = EngineActivity.objects.raw(sql)
        for i in data:
            print "install_and_init", i.result_date, i.dst_count, i.version
            MidResultInstallInitEngine.insert_a_data(i.result_date, i.dst_count, i.version, i.channel)


    def handle_install_and_init_emulator(self, start, end):
        print "handle_install_and_init_emulator"
        sql = """
    SELECT '%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,VERSION,channel FROM datastats_emulatoractivity
    WHERE op="init" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s'
    AND guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE op="install" AND STATUS = "success" AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s')
    GROUP BY VERSION,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        # print sql
        data = EmulatorActivity.objects.raw(sql)
        for i in data:
            # print "install_and_init", i.result_date, i.dst_count, i.version
            MidResultInstallInitEmulator.insert_a_data(i.result_date, i.dst_count, i.version, i.channel)

    def handle_engine_retention(self, start, end):
        print "handle_engine_retention"
        self.day_retention_engine(start,end,2)
        self.day_retention_engine(start,end,7)

    def handle_emulator_retention(self, start, end):
        print "handle_emulator_retention"
        self.day_retention(start,end,"emulator",2)
        self.day_retention(start,end,"emulator",7)
        # self.day_retention(start,end,"emulator",14)
        # self.day_retention(start,end,"emulator",30)


    def _cal_daily_app_session(self, start, end, package_name):
        print "cal app session %s" % package_name
        if package_name:
            #计算样本数量
            sql_init_count = """
            SELECT id,count(1)cnt FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='init' AND STATUS='success' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort'AND package_name= '%s'))
            """ % (start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name)
            # print sql_init_count
            data = AppActivity.objects.raw(sql_init_count)
            count_init = data[0].cnt

            sql_abort_count = """
            SELECT id,count(1)cnt FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='abort' AND STATUS='success' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort' AND package_name= '%s'))
            """ % (start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name)
            # print sql_abort_count
            data = EmulatorActivity.objects.raw(sql_abort_count)
            count_abort = data[0].cnt

            if (count_init>=count_abort):
                limit_count = count_abort
            else:
                limit_count = count_init

            if limit_count == 0:
                return

            sql_init = """
    SELECT id,SUM(UNIX_TIMESTAMP(DATETIME))unix_timestamp_,COUNT(1)cnt FROM (
    SELECT * FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='init' AND STATUS='success' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort' AND package_name= '%s')
    ) LIMIT %s
    ) AS use_table
            """ % (start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name, str(limit_count))


            sql_abort = """
    SELECT id,SUM(UNIX_TIMESTAMP(DATETIME))unix_timestamp_,COUNT(1)cnt FROM (
    SELECT * FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='abort' AND STATUS='success' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND package_name= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_appactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort' AND package_name= '%s')
    ) LIMIT %s
    ) AS use_table
            """ % (start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name,start.strftime('%Y-%m-%d'),package_name, str(limit_count))

            # print sql_init
            # print sql_abort

            init_data = EmulatorActivity.objects.raw(sql_init)
            abort_data = EmulatorActivity.objects.raw(sql_abort)
            init_unix_timestamp = init_data[0].unix_timestamp_ or 0
            abort_unix_timestamp = abort_data[0].unix_timestamp_ or 0

            # print init_unix_timestamp,abort_unix_timestamp
            # print abs(init_unix_timestamp-abort_unix_timestamp)
            try:
                das = float(abs(init_unix_timestamp-abort_unix_timestamp))/limit_count
            except ZeroDivisionError:
                das = 0
            # print int(das)

            ResultAppSession.add_data(start.strftime('%Y-%m-%d'),package_name,int(das),limit_count)


    def _cal_daily_user_session(self, start, end, version):
        dus = 0.0
        if not version:
            limit_count = ResultEmulatorSession.objects.filter(result_date=start.strftime('%Y-%m-%d')).aggregate(Sum('count'))["count__sum"]
            data = ResultEmulatorSession.objects.filter(result_date=start.strftime('%Y-%m-%d'))
            sum = 0
            for i in data:
                sum += i.count*i.dus
            try:
                dus = float(sum)/limit_count
            except ZeroDivisionError:
                dus = 0
            print dus

        else:
            sql_init_count = """
            SELECT id,count(1)cnt FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='init' AND STATUS='success' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort'AND version= '%s'))
            """ % (start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version)
            # print sql_init_count
            data = EmulatorActivity.objects.raw(sql_init_count)
            count_init = data[0].cnt

            sql_abort_count = """
            SELECT id,count(1)cnt FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='abort' AND STATUS='success' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort' AND version= '%s'))
            """ % (start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version)
            # print sql_abort_count
            data = EmulatorActivity.objects.raw(sql_abort_count)
            count_abort = data[0].cnt

            if (count_init>=count_abort):
                limit_count = count_abort
            else:
                limit_count = count_init

            sql_init = """
    SELECT id,SUM(UNIX_TIMESTAMP(DATETIME))unix_timestamp_,COUNT(1)cnt FROM (
    SELECT * FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='init' AND STATUS='success' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort' AND version= '%s')
    ) LIMIT %s
    ) AS use_table
            """ % (start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version, str(limit_count))

            sql_abort = """
    SELECT id,SUM(UNIX_TIMESTAMP(DATETIME))unix_timestamp_,COUNT(1)cnt FROM (
    SELECT * FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND op='abort' AND STATUS='success' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='init' AND version= '%s' AND guid IN (
    SELECT DISTINCT guid FROM datastats_emulatoractivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') = '%s' AND STATUS = 'success' AND op='abort' AND version= '%s')
    ) LIMIT %s
    ) AS use_table
            """ % (start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version,start.strftime('%Y-%m-%d'),version, str(limit_count))

            # print sql_init
            # print sql_abort
            init_data = EmulatorActivity.objects.raw(sql_init)
            abort_data = EmulatorActivity.objects.raw(sql_abort)
            init_unix_timestamp = init_data[0].unix_timestamp_ or 0
            abort_unix_timestamp = abort_data[0].unix_timestamp_ or 0
            print init_unix_timestamp,abort_unix_timestamp
            print abs(init_unix_timestamp-abort_unix_timestamp)
            try:
                dus = float(abs(init_unix_timestamp-abort_unix_timestamp))/limit_count
            except ZeroDivisionError:
                dus = 0
            print dus

        ResultEmulatorSession.add_data(start.strftime('%Y-%m-%d'),version,dus,limit_count)


    def handle_emulator_session(self, start, end):
        # no_of_session = self._cal_daily_no_session(start,end)
        # print no_of_session
        print "start calculate emulator session"
        version = ""
        q_version = EmulatorActivity.objects.filter(op="init",status="success").values("version").distinct()
        versions = [i["version"] for i in q_version]
        versions.append("")
        print versions
        for version in versions:
            self._cal_daily_user_session(start,end,version)


    def handle_app_local_500(self, start):
        sql = """
SELECT '%s' AS result_date,id,package_name,VERSION,COUNT(DISTINCT guid)dst_count,COUNT(1)count FROM datastats_appinstall
WHERE guid NOT IN
(SELECT DISTINCT guid FROM datastats_appinstall WHERE op = 'install_from_download' AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s' AND STATUS='success')
AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s' AND op = 'install' AND STATUS='success'
GROUP BY package_name,VERSION
ORDER BY dst_count DESC LIMIT 500
        """ % (start.strftime("%Y-%m-%d"),start.strftime("%Y-%m-%d"),start.strftime("%Y-%m-%d"))
        data = AppInstall.objects.raw(sql)
        for i in data:
            ResultAppLocalTop500.insert_data(i.result_date,i.package_name,i.version,i.dst_count,i.count)


    def handle_app_session(self, start, end):
        _end = start + datetime.timedelta(days=1)
        print "start calculate app session"
        q_app = AppActivity.objects.filter(status="success",datetime__gte=start.strftime('%Y-%m-%d'),datetime__lt=_end.strftime('%Y-%m-%d')).values("package_name").distinct()
        apps = [i["package_name"] for i in q_app]
        apps = check_package_name(apps)
        print "app number:",len(apps)
        for app in apps:
            try:
                self._cal_daily_app_session(start,end,app)
            except Exception, e:
                print e

    def handle_emulator_download_rate(self, start, end):
        """
        处理当日卸载率和隔日卸载率

        当日安装并卸载人数
SELECT id, VERSION, COUNT(DISTINCT guid)dst_count,COUNT(guid)COUNT FROM datastats_emulatorinstall WHERE op="install" AND STATUS="success" AND DATE_FORMAT(DATETIME,"%Y-%m-%d")="2016-06-29"
AND guid IN ( SELECT DISTINCT(guid) FROM datastats_emulatorinstall WHERE op="uninstall" AND STATUS="success" AND DATE_FORMAT(DATETIME,"%Y-%m-%d")="2016-06-29"
) GROUP BY VERSION

        隔日卸载人数
SELECT id, VERSION, COUNT(DISTINCT guid)dst_count, COUNT(guid)count FROM datastats_emulatorinstall WHERE op="install" AND STATUS="success" AND DATE_FORMAT(DATETIME,"%Y-%m-%d")="2016-06-27"
AND guid IN ( SELECT DISTINCT(guid) FROM datastats_emulatorinstall WHERE op="uninstall" AND STATUS="success" AND DATE_FORMAT(DATETIME,"%Y-%m-%d")="2016-06-28"
) GROUP BY VERSION

        当日安装人数
SELECT id,VERSION,dst_count,count FROM datastats_resultemulatorinstall WHERE op="install" AND result_date = "2016-06-27" AND STATUS = "success" GROUP BY VERSION
        """
        the_day_before_yest = start - datetime.timedelta(days=1)
        print start
        print end
        print the_day_before_yest

        # 当日安装并卸载人数
        print "处理当日卸载率和隔日卸载率"
        sql = """
SELECT '%s' AS result_date, id, VERSION,channel, COUNT(DISTINCT guid)dst_count,COUNT(guid)count FROM datastats_emulatorinstall WHERE op='install' AND STATUS='success' AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
AND guid IN ( SELECT DISTINCT(guid) FROM datastats_emulatorinstall WHERE op='uninstall' AND STATUS='success' AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
) GROUP BY VERSION,channel
        """ % (start.strftime('%Y-%m-%d'), start.strftime('%Y-%m-%d'), start.strftime('%Y-%m-%d'))
        # print sql
        install_and_uninstall_data = EmulatorInstall.objects.raw(sql)
        for i in install_and_uninstall_data:
            ResultEmulatorUninstallCount.insert_a_data(i.result_date,i.version,i.dst_count,i.count,i.channel)

        # 隔日卸载人数
        sql = """
SELECT '%s' AS result_date,id, VERSION,channel, COUNT(DISTINCT guid)dst_count, COUNT(guid)count FROM datastats_emulatorinstall WHERE op='install' AND STATUS='success' AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
AND guid IN ( SELECT DISTINCT(guid) FROM datastats_emulatorinstall WHERE op='uninstall' AND STATUS='success' AND DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
) GROUP BY VERSION,channel
        """ % (the_day_before_yest.strftime('%Y-%m-%d'), the_day_before_yest.strftime('%Y-%m-%d'), start.strftime('%Y-%m-%d'))
        # print sql
        tomorrow_uninstall_data = EmulatorInstall.objects.raw(sql)
        for i in tomorrow_uninstall_data:
            ResultEmulatorUninstallNextDayCount.insert_a_data(i.result_date,i.version,i.dst_count,i.count,i.channel)

        # 当日安装人数
        sql = """
SELECT '%s' AS result_date,id,VERSION,dst_count,count FROM datastats_resultemulatorinstall WHERE op='install' AND STATUS = 'success' AND result_date = '%s'  GROUP BY VERSION,channel
        """% (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        # print sql
        install_data = ResultEmulatorInstall.objects.raw(sql)
        for i in install_data:
            # i.result_date,i.version,i.dst_count,i.count
            ResultEmulatorInstallCount.insert_a_data(i.result_date,i.version,i.dst_count,i.count,i.channel)

    def handle_engine_uninstall_rate(self, start, end):
        """
        处理基于引擎的当日和次日卸载率
        """
        print "handle_engine_uninstall_rate"
        sql = """
SELECT 'first_init' AS field ,'%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,VERSION,channel FROM datastats_engineactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s' AND op='first_init' AND STATUS = 'success'
GROUP BY VERSION,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        # print sql
        first_init_data = EngineActivity.objects.raw(sql)
        for i in first_init_data:
            # print i.field, i.result_date, i.dst_count, i.version
            ResultEngineUninstallRate.insert_a_data(i.result_date, i.field, i.version, i.dst_count, i.channel)

        sql = """
        SELECT 'today_uninstall' AS field ,'%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,VERSION,channel FROM datastats_engineactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s' AND op='first_init' AND STATUS = 'success'
AND guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') IN ('%s') AND op='uninstall' AND STATUS='success')
GROUP BY VERSION,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))
        today_uninstall_data = EngineActivity.objects.raw(sql)
        for i in today_uninstall_data:
            ResultEngineUninstallRate.insert_a_data(i.result_date, i.field, i.version, i.dst_count, i.channel)

        _yest = start + datetime.timedelta(days=-1)
        sql = """
        SELECT 'next_day_uninstall' AS field ,'%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,VERSION,channel FROM datastats_engineactivity WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s' AND op='first_init' AND STATUS = 'success'
AND guid IN (SELECT DISTINCT guid FROM datastats_emulatorinstall WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d') IN ('%s','%s') AND op='uninstall' AND STATUS='success')
GROUP BY VERSION,channel
        """ % (_yest.strftime('%Y-%m-%d'),_yest.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'),_yest.strftime('%Y-%m-%d'))
        next_day_uninstall_data = EngineActivity.objects.raw(sql)
        for i in next_day_uninstall_data:
            ResultEngineUninstallRate.insert_a_data(i.result_date, i.field, i.version, i.dst_count, i.channel)

if __name__ == '__main__':
    now = datetime.datetime.now()

    today = datetime.date.today()# + datetime.timedelta(days=1)
    print "today:", today
    yest = datetime.timedelta(days=-1) + today
    tomorrow = datetime.timedelta(days=1) + today

    a = Data2db()
    #emulator engine data

    a.handle_engine_install(yest.strftime('%Y-%m-%d'), today.strftime('%Y-%m-%d'))
    a.handle_engine_activity(yest.strftime('%Y-%m-%d'), today.strftime('%Y-%m-%d'))
    a.handle_emulator_install(yest.strftime('%Y-%m-%d'), today.strftime('%Y-%m-%d'))
    a.handle_emulator_activity(yest.strftime('%Y-%m-%d'), today.strftime('%Y-%m-%d'))

    print "finish handle engine emulator install activity, used time: ",datetime.datetime.now() - now
    a.handle_engine_dau(yest, today)
    print "finish handle engine dau, used time: ",datetime.datetime.now() - now
    a.handle_emulator_download_rate(yest, today)
    print "finish handle emulator uninstall rate, used time:",datetime.datetime.now() - now
    a.handle_install_and_init_emulator(yest, today)     #handle_emulator_retention的先决数据
    a.handle_emulator_retention(yest, today)
    print "finish handle engine uninstall rate, used time:",datetime.datetime.now() - now

    a.handle_engine_uninstall_rate(yest,today)
    print "finish handle emulator retention, used time: ",datetime.datetime.now() - now

    a.handle_emulator_session(yest, today)
    print "finish handle emulator session, used time: ",datetime.datetime.now() - now
    a.handle_install_and_init_engine(yest, today)       #handle_engine_retention的先决数据
    a.handle_engine_retention(yest, today)
    print "finish handle engine session, used time: ",datetime.datetime.now() - now

    #general data
    b = GeneralData2db()
    b.general_date2db(yest, today)
    print "finish grab all general data, used time: ",datetime.datetime.now() - now
    #处理用户系统信息
    user_computer_info = UserComputerInfo()
    user_computer_info.run(yest)
    print "finish handle user computor information data, used time: ",datetime.datetime.now() - now

    #app total
    a.handle_total_app(yest)
    print "finish handle total app, used time: ",datetime.datetime.now() - now

    #app
    a.handle_app_install(yest.strftime('%Y-%m-%d'), today.strftime('%Y-%m-%d'))
    a.handle_app_activity(yest.strftime('%Y-%m-%d'), today.strftime('%Y-%m-%d'))
    print "finish handle app install activity, used time: ",datetime.datetime.now() - now
    a.handle_app_session(yest, today)
    print "finish handle app session data, used time: ",datetime.datetime.now() - now

    a.handle_app_local_500(yest)
    print "finish handle app local 500",datetime.datetime.now() - now


    print "finish all, used time: ",datetime.datetime.now() - now



    # a.delete_one_day_all_result_data("2016-07-26")






