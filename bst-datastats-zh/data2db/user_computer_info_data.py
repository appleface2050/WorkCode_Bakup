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

from datastats.models import ResultGeneralOsVersion, ResultUserComputerInfoOS, ResultUserComputerInfoMemory, \
    ResultUserComputerInfoCPU


class UserComputerInfo(object):
    """
    处理用户系统信息
    """
    def run(self, start):
        """
        跑所有的数据
        """
        self.handle_osver_result_data(start)
        self.handle_memery(start)
        self.handle_cpu(start)

    def handle_osver_result_data(self, start):
        """
        处理操作系统版本数据
        """
        print "handle user computer info os"
        sql = """
        SELECT '%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,osver,channel
        FROM result_general_osver
        WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
        AND STATUS='success'
        GROUP BY osver,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))

        data = ResultGeneralOsVersion.objects.raw(sql)
        for i in data:
            ResultUserComputerInfoOS.insert_a_data(result_date=i.result_date,os=i.osver,dst_count=i.dst_count,channel=i.channel)

    def handle_memery(self, start):
        """
        处理内存数据
        """
        print "handle user computer info memery"
        sql = """
        SELECT '%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,memory,channel
        FROM result_general_osver
        WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
        AND STATUS='success'
        GROUP BY memory,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))

        data = ResultGeneralOsVersion.objects.raw(sql)
        for i in data:
            # print i.result_date, i.dst_count,i.osver
            ResultUserComputerInfoMemory.insert_a_data(result_date=i.result_date,memory=i.memory,dst_count=i.dst_count,channel=i.channel)

    def handle_cpu(self, start):
        """
        处理cpu数据
        """
        print "handle user computer info cpu"
        sql = """
        SELECT '%s' AS result_date,id,COUNT(DISTINCT guid)dst_count,cpu,channel
        FROM result_general_osver
        WHERE DATE_FORMAT(DATETIME,'%%%%Y-%%%%m-%%%%d')='%s'
        AND STATUS='success'
        GROUP BY cpu,channel
        """ % (start.strftime('%Y-%m-%d'),start.strftime('%Y-%m-%d'))

        data = ResultGeneralOsVersion.objects.raw(sql)
        for i in data:
            # print i.result_date, i.dst_count,i.osver
            ResultUserComputerInfoCPU.insert_a_data(result_date=i.result_date,cpu=i.cpu,dst_count=i.dst_count,channel=i.channel)

    def delete_one_day_data(self, start):
        """
        删除某一天数据
        """
        print "deleting user computer info data %s" % start.strftime('%Y-%m-%d')
        a = ResultUserComputerInfoOS.objects.filter(result_date=start.strftime('%Y-%m-%d'))
        for i in a:
            i.delete()
        a = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime('%Y-%m-%d'))
        for i in a:
            i.delete()
        a = ResultUserComputerInfoCPU.objects.filter(result_date=start.strftime('%Y-%m-%d'))
        for i in a:
            i.delete()


if __name__ == '__main__':
    now = datetime.datetime.now()
    today = datetime.date.today() #- datetime.timedelta(days=1)
    print "today:", today
    yest = datetime.timedelta(days=-1) + today
    tomorrow = datetime.timedelta(days=1) + today

    a = UserComputerInfo()
    a.delete_one_day_data(yest)
    a.run(yest)
    print "finish used time: ",datetime.datetime.now() - now





