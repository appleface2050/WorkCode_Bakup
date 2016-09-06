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


class GameNameLower(object):
    """
    生成小写游戏名
    """
    def convert(self):
        i = 0
        game_library_data = GameLibrary.objects.all()
        for data in game_library_data:
            game_name = data.game_name
            print game_name
            try:
                data.game_name_lower = game_name.lower()
                data.save()
                i += 1
            except Exception as e:
                print data.game_name
                print e


if __name__ == '__main__':
    g = GameNameLower()
    now = datetime.datetime.now()
    print "convert start: ", now
    amount = g.convert()
    print "handle data amount: ", amount
    print "convert end, using time: ", datetime.datetime.now() - now



