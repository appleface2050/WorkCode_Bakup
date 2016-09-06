#-*- coding: utf-8 -*-
import datetime
import sys
import os

sys.path.append('/var/www/html/master/bst-datastats-zh')

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

from util.redis_ import r

from django.db import connection, transaction


from django.core.wsgi import get_wsgi_application
application = get_wsgi_application()

# r.hset("package_user_init","com.android.vending","1780")
# r.hset("package_user_init","com.location.provider","1")
#
# print r.hgetall("package_user_init")
#
# #r.hset("package_user_init","com.android.vending","178")
#
# print r.hgetall("package_user_init")

class AppUserInit2redis(object):
    """
    app用户启动数据增加到redis
    """
    def data2redis(self, start):
        result = {}
        yest = start + datetime.timedelta(days=-1)
        sql = """
        SELECT a.package_name, b.daily_user_init FROM scope_app_package_name a INNER JOIN stats_app_package_name b ON
        a.id = b.scope_id AND b.result_date in ('%s', '%s')
        """ % (yest.strftime('%Y-%m-%d'), start.strftime('%Y-%m-%d'))
        print sql
        cursor = connection.cursor()
        cursor.execute(sql)
        data = cursor.fetchall()

        for i in data:
            if i[1] != 0:
                r.hset("package_user_init", i[0], i[1])
        # print r.hgetall("package_user_init")

        # for i in data:
            # if i[1] != 0:
            #     result[i[0]] = i[1]
        #print result

        # for key in result.keys():
        #     r.hset("package_user_init",key,result["key"])


if __name__ == '__main__':
    today = datetime.date.today()
    yest = today + datetime.timedelta(days=-1)
    start = yest
    a = AppUserInit2redis()
    a.data2redis(start)



