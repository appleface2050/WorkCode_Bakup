#coding=utf-8

import os
import sys

import datetime

from xpinyin import Pinyin
p = Pinyin()


BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
print BASE_DIR
# sys.path.append('/var/www/html/bst_datastats')
sys.path.append(BASE_DIR)


from django.core.wsgi import get_wsgi_application


os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()

from bluestacks.models import GameLibrary, GameLibraryModify
from bst_server.settings import ENVIRONMENT


class ConvertPinyin(object):
    """
    转换为拼音和拼音首字母
    """
    def convert(self):
        i = 0
        game_library_data = GameLibrary.objects.all()
        for data in game_library_data:
            game_name = data.game_name
            print game_name
            data.pinyin = p.get_pinyin(game_name,"").lower()
            data.initails_pinyin = p.get_initials(game_name,"").lower()
            try:
                data.save()
                i += 1
            except Exception as e:
                print data.game_name
                print e

if __name__ == '__main__':
    c = ConvertPinyin()
    now = datetime.datetime.now()
    print "convert start: ", now
    amount = c.convert()
    print "handle data amount: ", amount
    print "convert end, using time: ", datetime.datetime.now() - now











