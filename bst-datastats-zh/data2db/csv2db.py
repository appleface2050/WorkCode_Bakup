# coding=utf-8
# from django.core.wsgi import get_wsgi_application
# application = get_wsgi_application()

import datetime
import os
from django.core.wsgi import get_wsgi_application
from django.db.models import Sum

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()

from datastats.models import AppCenterData


class CSV2db(object):
    def __init__(self):
        self._file_dir = "C:\\csv\\"

    def data2db(self, start):
        file_name = "AppcenterData_" + start.strftime('%Y%m%d') + ".csv"
        full_dir = self._file_dir + file_name
        print full_dir

        f = open(full_dir)
        for line in f.readlines():
            l = line.decode("gbk")
            if l.startswith(","):
                continue
            print l
            tmp = l.split(",")
            AppCenterData.add_data(start.strftime('%Y-%m-%d'),tmp[0],tmp[1],tmp[2],tmp[3],tmp[4],tmp[5])
        f.close()

    def run(self, start):
        print "processing:",start.strftime('%Y-%m-%d')
        self.data2db(start)


if __name__ == '__main__':
    a = CSV2db()
    today = datetime.date.today()
    yest = datetime.date.today() + datetime.timedelta(days=-1)
    a.run(yest)
