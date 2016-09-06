# coding=utf-8
from clash_royale_app.models import CRUserInfo

import random

PRICE_DICT = {
    1:"powerbank",
    2:"6yuan",
    3:"10yuan",
    4:"30yuan",
    5:"notwinning"
}

class Lottery(object):
    """
    powerbank：10
    6yuan: 200
    10yuan: 100
    30yuan: 50
    notwinning: 500
    """

    def __init__(self):
        """
        根据已有的奖项，生成奖品池
        """
        self._pool_ = []
        if CRUserInfo.objects.filter(prize__in=["powerbank","6yuan","10yuan","30yuan","notwinning"]).count() >= 860:
            self._pool_ = []
        else:
            powerbank_count = CRUserInfo.objects.filter(prize="powerbank").count()
            yuan6_count = CRUserInfo.objects.filter(prize="6yuan").count()
            yuan10_count = CRUserInfo.objects.filter(prize="10yuan").count()
            yuan30_count = CRUserInfo.objects.filter(prize="30yuan").count()
            notwinning_count = CRUserInfo.objects.filter(prize="notwinning").count()
            if powerbank_count >=10:
                powerbank = 0
            else:
                powerbank = 10 - powerbank_count
            if yuan6_count >= 200:
                yuan6 = 0
            else:
                yuan6 = 200 - yuan6_count
            if yuan10_count >= 100:
                yuan10 = 0
            else:
                yuan10 = 100- yuan10_count
            if yuan30_count >= 50:
                yuan30 = 0
            else:
                yuan30 = 50 - yuan30_count
            if notwinning_count >= 500:
                notwinning = 0
            else:
                notwinning = 500 - notwinning_count

            self._pool_ = [1 for i in xrange(powerbank)] + [2 for i in xrange(yuan6)] \
                         + [3 for i in xrange(yuan10)] + [4 for i in xrange(yuan30)] + [5 for i in xrange(notwinning)]


    def make_prize(self):
        if not self._pool_:
            return "notwinning"
        else:
            try:
                c = random.choice(self._pool_)
            except Exception as e:
                print e
                return "notwinning"

            prize = PRICE_DICT.get(c,"notwinning")
            return prize