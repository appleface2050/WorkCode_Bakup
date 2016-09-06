#!/usr/bin/ python
# -*- coding: utf-8 -*-

import sys
import traceback
from common import utils
from providers import _360,game9,providerBase

# 入口函数
if __name__ == '__main__':
    utils.printlog(u'开始接口同步任务')
    print('...\n\n')
    try:
        instance = _360._360()
        instance.sync()
        instance = game9.game9()
        instance.sync()
    except Exception,e:
        exstr = traceback.format_exc()
        utils.printlog(u"发现异常，异常:%s" % (exstr),3)
    print(u'...\n\n')
    utils.printlog(u'所有接口同步完成...')

