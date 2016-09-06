#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import ConfigParser
#import utils
from common import utils

def getDbSetting():
    u'获取连接字符串配置,返回一个字典'
    host = 'rdsk75k0anuj28956uzl.mysql.rds.aliyuncs.com'
    port = 3306
    user = 'bluestackscntest'
    pwd = 'Bluestacks2016test'
    #db = 'bs_gamelibrary_cn'
    db = 'cn_bst_server'
    charset = "utf8"

    d = {}
    d["host"] = host
    d["port"] = port
    d["user"] = user
    d["passwd"] = pwd
    d["db"] = db
    d["charset"] = charset
    return d

def getSyncfilePath(provider):
    u'获取同步文件路径'
    logName = '/%s' % provider
    #path = os.getcwd() + logName + '.synctime'
    path = "/root/GameDataSync" + logName + '.synctime'
    #print path
    return path

# 配置管理
def getSyncTime(provider):
    u'获取同步时间戳，未同步过返回0'
    path = getSyncfilePath(provider)
    lines = utils.readFile(path,'r')
    if lines != None and len(lines) > 0:
        return lines[0].replace("\n",'').replace('\r','')
    return ''

def setSyncTime(provider,timestamp):
    u'设置同步时间戳，未同步过返回0'
    path = getSyncfilePath(provider)
    utils.writeFile(path,timestamp,'w')

    #验证配置
    tmp = getSyncTime(provider)
    return tmp == str(timestamp)


def syncTime(oper,configFile,timestamp):
    u'加载或设置同步时间, oper = "get" or "set"'
    
    if oper == 'get':
        return getSyncTime(configFile)
    elif oper == 'set':
        return setSyncTime(configFile,timestamp)
    else:
        print 'error parameter'


