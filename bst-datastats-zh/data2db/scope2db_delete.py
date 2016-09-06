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


class Scope2dbDelete(object):
    def delete(self, start):
        count = 0
        for db in [EngineStats,EmulatorStats,AppTotalStat,AppPackagenameStat]:
            a = db.objects.filter(result_date=start.strftime('%Y-%m-%d'))
            for i in a:
                i.delete()
                count += 1
        print count


if __name__ == '__main__':
    now = datetime.datetime.now()

    today = datetime.date.today()# + datetime.timedelta(days=1)
    print "today:", today

    yest = datetime.timedelta(days=-1) + today
    tomorrow = datetime.timedelta(days=1) + today
    a = Scope2dbDelete()
    a.delete(yest)