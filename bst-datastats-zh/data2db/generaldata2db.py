# coding=utf-8
# from django.core.wsgi import get_wsgi_application
# application = get_wsgi_application()

import datetime
import os
import sys
import json


# if ENVIRONMENT == "aliyun_test":

sys.path.append('/var/www/html/bst-datastats-zh')


from django.db import connection, transaction
from django.core.wsgi import get_wsgi_application
from django.db.models import Sum
from bst_server.settings import ENVIRONMENT

from util.appcenter import cal_day

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()

from datastats.models import GeneralData,ResultGeneralOsVersion,ResultGeneralEngineInstError,ResultGeneralAPKInstError,ResultGeneralEngInitError,ResultGeneralEngineInstall, \
    ResultGeneralUninstallReason


class GeneralData2db(object):
    def __init__(self):
        self._op_type_ = ["osver",
                        "eng_init_error",
                        "apk_inst_error",
                        "engine_install",
                        "engine_inst_error"
                     ]

    def delete_general_result_date(self, start, end):
        """
        删除某一天的数据
        """
        tables = [ResultGeneralOsVersion,ResultGeneralEngineInstError,ResultGeneralAPKInstError,
                  ResultGeneralEngInitError,ResultGeneralEngineInstall,ResultGeneralUninstallReason]
        for i in tables:
            q = i.objects.filter(datetime__gte=start, datetime__lt=end)
            for j in q:
                j.delete()


    def general_date2db(self, start, end):
        print "general_date2db"
        data = GeneralData.objects.filter(uptime__gte=start.strftime('%Y-%m-%d'), uptime__lt=end.strftime('%Y-%m-%d'))
        for i in data:
            uptime = i.uptime
            try:
                json_data = json.loads(i.json)["json"]
            except Exception,e:
                print e
                continue
            op = json_data.get("op","")
            if not op:
                continue
            if op == "osver":
                try:
                    ResultGeneralOsVersion.add_data(json_data, uptime)
                except Exception, e:
                    print e
                    print json_data
                    continue
            elif op == "eng_init_error":
                try:
                    ResultGeneralEngInitError.add_data(json_data, uptime)
                except Exception, e:
                    print e
                    print json_data
                    continue
            elif op == "apk_inst_error":
                try:
                    ResultGeneralAPKInstError.add_data(json_data, uptime)
                except Exception, e:
                    print e
                    print json_data
                    continue
            elif op == "engine_install":
                try:
                    ResultGeneralEngineInstall.add_data(json_data,uptime)
                except Exception, e:
                    print e
                    print json_data
                    continue
            elif op == "engine_inst_error":
                try:
                    ResultGeneralEngineInstError.add_data(json_data, uptime)
                except Exception, e:
                    print e
                    print json_data
                    continue
            elif op == "uninst_reason":
                try:
                    ResultGeneralUninstallReason.add_data(json_data, uptime)
                except Exception, e:
                    print e
                    print json_data
                    continue

            else:
                print json_data

if __name__ == '__main__':
    now = datetime.datetime.now()
    today = datetime.date.today() #- datetime.timedelta(days=1)
    print "today:", today
    yest = datetime.timedelta(days=-1) + today
    tomorrow = datetime.timedelta(days=1) + today

    a = GeneralData2db()
    a.general_date2db(yest, today)


    # a.delete_general_result_date(yest, today)


    print "finish used time: ",datetime.datetime.now() - now
