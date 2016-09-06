# -*- coding: utf-8 -*-

import os
import sys
import datetime
import time


BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
print BASE_DIR
# sys.path.append('/var/www/html/bst_datastats')
sys.path.append(BASE_DIR)


from django.core.wsgi import get_wsgi_application


os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()

from django.db.models import Sum,Count

from bluestacks.models import SearchKeyWord
from util.sendemail import SendEmail
from util.sendemail import SEARCH_TOP100_RECEIVER


if __name__ == '__main__':
    today = datetime.date.today()# + datetime.timedelta(days=1)
    print "today:", today
    yest = datetime.timedelta(days=-1) + today
    a_week_before = datetime.timedelta(days=-7) + today

    #mailto_list = ["andy@bluestacks.com", "cathy@bluestacks.com", "feifei@bluestacks.com"]
    #mailto_list = ["andy@bluestacks.com"]
    mailto_list = SEARCH_TOP100_RECEIVER

    subject = "搜索关键词 %s - %s " % (a_week_before.strftime("%Y-%m-%d"),yest.strftime("%Y-%m-%d"))

    search = SearchKeyWord.objects.using("production").filter(uptime__gte=a_week_before.strftime("%Y-%m-%d"),uptime__lt=today.strftime("%Y-%m-%d")).values('word').annotate(count=Count(1)).order_by("-count")

    # h3 = yest.strftime("%Y-%m-%d")
    tr = ""
    for i in search:
        if i["count"] < 100:
            break
        # print i["word"], i["count"]
        tr += "<tr><th>%s</th><th>%s</th></tr>" % (i["word"], str(i["count"]))

    table = """
    <table border="1">%s</table>
    """ % tr

    html= """
        <html>
        <body>
        %s
        </body>
        </html>
    """ % (table)

    #print html

    for mailto in mailto_list:
        print mailto
        s = SendEmail([mailto], subject, html)
        if s.send_mail_html():
            print ("发送成功")
        else:
            print ("发送失败")
        time.sleep(10)







