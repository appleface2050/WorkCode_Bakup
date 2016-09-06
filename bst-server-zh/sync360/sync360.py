# coding=utf-8

import datetime
import hashlib
import requests
import os
import sys
import urllib
import json
import time

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
print BASE_DIR
# sys.path.append('/var/www/html/bst_datastats')
sys.path.append(BASE_DIR)


from django.core.wsgi import get_wsgi_application


os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()

from bluestacks.models import Total360Game, Platform


class Sync360(object):
    """
    360全量数据同步
    """
    def __init__(self):
        self._from_ = "lm_227852"
        self._url_ = "http://api.np.mobilem.360.cn/app/list"
        self._secret_key_ = "b3f3bc510098b1a36d0e8da9e2bfb0e5"
        self._pagesize_ = 300
        now = datetime.datetime.now()
        today = datetime.date.today()
        yest = datetime.timedelta(days=-1) + today
        # self.today =

    def fetch(self, url):
        try:
            r = requests.get(url,timeout=30)
            data = json.loads(r.text)
            return data
        except Exception, e:
            print e
            return False

    def make_sign(self, para):
        keys = para.keys()
        keys.sort()
        key = ""
        for i in keys:
            tmp = i + "=" + str(para[i])
            key = key + tmp + "&"
        key = key.strip("&") + self._secret_key_
        sign = hashlib.md5(key).hexdigest()
        # print sign
        return sign

    def make_url(self, para):
        sign = self.make_sign(para)
        para["sign"] = sign
        url = self._url_ + "?"+ urllib.urlencode(para)
        return url


    def sync_first_time(self):
        """
        第一次同步
        """
        total_para = {
            "from":self._from_,
            "page":1,
            "pagesize":1
        }
        total_url = self.make_url(total_para)
        total = self.get_total(total_url)

        page_total = total/self._pagesize_
        page = 1
        print "total page: ",page_total
        while page <= page_total:
            now = datetime.datetime.now()
            print "page:",page,now
            para = {
                "from":self._from_,
                "page":page,
                "pagesize": self._pagesize_
            }
            url = self.make_url(para)
            data = self.fetch(url)
            if not data:
                continue
            else:
                items = data.get("items",[])
                self.data2db(items)
                time.sleep(5)
            page += 1


    def data2db(self, items):
        platform_id = Platform.get_platfor_id_by_name("360")
        if not platform_id:
            raise Exception("plaform not exist")
        for item in items:
            a = Total360Game()
            a.platformId = platform_id
            a.platform_game_id = item.get('id',"")
            a.game_name = item.get("name","")
            a.type = item.get("categoryName","")
            a.icon_url = item.get("iconUrl","")
            a.size = item.get("apkSize","")
            a.download_count = item.get("downloadTimes","")
            a.modify_date = item.get("updateTime","")
            a.screenshots = item.get("screenshotsUrl","")
            a.screenshots_type = "shu"
            a.version = item.get("versionName","")
            a.tags = item.get("tag","")
            a.level = item.get("rating","")
            a.instruction = item.get("brief","")
            a.description = item.get("description","")
            # a.deleted = False
            a.download_url = item.get("downloadUrl","")
            # a.downloadqrcode_url = item.get("downloadqrcode_url","")
            a.package_name = item.get("packageName","")
            a.apk_md5 = item.get("apkMd5","")
            try:
                a.save()
            except Exception,e :
                print e
                print a.game_name



    def get_total(self, url):
        data = requests.get(url).text
        data = json.loads(data)
        total = data.get("total",0)
        return total

    def sync(self):
        """
        每日同步
        """


    def check_offline(self):
        """
        检查下线游戏
        """


if __name__ == '__main__':
    now = datetime.datetime.now()
    a = Sync360()
    a.sync_first_time()
    print "sync_first_time, using time: ", datetime.datetime.now() - now
    # a.run()