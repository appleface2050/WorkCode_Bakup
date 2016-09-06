#!/usr/bin/env python
# -*- coding: utf-8 -*-

import time
import requests
import json
import sys
import os
import traceback
from config import settings
from dal import dbtools,mysqlHelper
from common import utils

printlog = utils.printlog

class providerBase(object):

    def getDbInstance(self):
        u'获取数据示例'
        dbconfig = settings.getDbSetting() 
        db = mysqlHelper.MySQL(dbconfig)
        return db

    def syncToDB(self,list):
        u'''
        同步数据到数据库,返回成功的id列表
        list 必须为数据库实体(dict)列表
        '''
        tablename = 'game_library'
        primarykey = 'platform_game_id'
        succedCount = 0L
        succedList = []
        try:
            #db = dbtools.getDbInstance()
            db = self.getDbInstance()
            for model in list:
                id = model[primarykey]
                # 保存ID到请求列表中，为增量同步处理
                conditionDict = {}
                conditionDict[primarykey] = id 
                conditionDict['platformId'] = self.PLATFORM_ID
                sql = dbtools.getExistSql(tablename,conditionDict)
                # 根据游戏唯一id判断数据是否存在
                exist = db.query(sql)
                if exist > 0:
                    #update
                    sql = dbtools.getUpdateSql(tablename, primarykey, model)
                    result = db.update(sql)
                else:
                    #insert
                    if model["deleted"] == 1:
                        # 下架的游戏，且库中也不存在，则不新增到库中
                        continue
                    if not model["package_name"]:
                        continue
    
                    sql = dbtools.getInsertSql(tablename, model)
                    result = db.insert(sql)
        
                if result[0] == True:
                    succedList.append(id)
                else:
                    e = result[1]
                    printlog(u'syncToDb faild:' + result[1] + ",model:" + model,2)

        except Exception,e :
            exstr = traceback.format_exc()
            printlog(u"发现异常，异常:%s" % (exstr),3)
        finally:
            db.close()

        return succedList
        
    def getPlatformId(self,providerName):
        u'根据平台名称获取平台ID'
        tablename = 'platform'
        conditionDict = {"platform_name":providerName}
        sql = dbtools.getSelectSql(tablename,'*',conditionDict)
        db = self.getDbInstance()
        try:
            rowsCount = db.query(sql)
            if rowsCount <= 0:
                print u'获取平台ID失败'
            item = db.fetchOneRow()
            return int(item[0])
        except Exception,e :
            exstr = traceback.format_exc()
            printlog(u"发现异常，异常:%s" % (exstr),3)
        finally:
            db.close()

    def __init__(self,providerName):
        u'初始化操作，初始化平台信息'
        self.PROVIDER_NAME = providerName

        if providerName == '':
            printlog(u'%s数据接口提供商名称错误')
        try:
            self.PLATFORM_ID = self.getPlatformId(self.PROVIDER_NAME)
            if self.PLATFORM_ID <= 0:
                errMsg = u'初始化失败：获取平台ID失败'
                printlog(errMsg)
                raise AssertionError(errMsg)
        except Exception,e :
            exstr = traceback.format_exc()
            printlog(u"发现异常，异常:%s" % (exstr),3)
        

# 入口函数
if __name__ == '__main__':
    pass

