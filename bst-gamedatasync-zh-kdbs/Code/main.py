#!/usr/bin/ python
# -*- coding: utf-8 -*-

import sys, socket
import traceback
from common import utils
from providers import _360,game9,providerBase
import requests

def get_local_ip():
    try:
        csock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        csock.connect(('8.8.8.8', 80))
        (addr, port) = csock.getsockname()
        csock.close()
        return addr
    except socket.error:
        return ""

# 入口函数
if __name__ == '__main__':
    utils.printlog(u'开始接口同步任务')
    print('...\n\n')
    try:
        # instance = _360._360()
        # instance.sync()
        instance = game9.game9()
        instance.sync()
    except Exception,e:
        exstr = traceback.format_exc()
        utils.printlog(u"发现异常，异常:%s" % (exstr),3)
    print(u'...\n\n')

    #增加刷新app center操作
    #ip = str(get_local_ip())
    #r = requests.get('http://%s/bs/app_center_flush'%ip)
    #print r.status_code
    #print r.text

    utils.printlog(u'所有接口同步完成...')

