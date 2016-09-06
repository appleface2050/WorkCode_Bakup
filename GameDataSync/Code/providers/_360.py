#!/usr/bin/env python
# -*- coding: utf-8 -*-

import time
import requests
import json
import os
import traceback
from common import utils
from dal import dbtools
from config import settings
from providers import providerBase

# 这次同步任务获取的Id列表
RequestIdList = []

printlog = utils.printlog


class _360(providerBase.providerBase):

    def modelConverter(self,item):
        
        # target
        # id:自增索引
        # game_name：游戏名称
        # type：游戏类型
        # platformId：游戏平台（对应platform表的id字段）
        # platform_game_id：游戏对应游戏平台的游戏id(同一个游戏，在各渠道里id不同)
        # icon_url：icon地址
        # size：游戏大小
        # download_count：下载数量
        # modify_date：更新时间(每天更新时用来对比的)
        # screenshots:截图：可以是多个地址
        # version：版本
        # tags：游戏tags
        # level：游戏评分（评价）
        # instruction：游戏简介（短）
        # description：游戏描述（长game_library）
        # deleted：是否删除
        data = {}
        data["game_name"] = item["name"]
        data["type"] = item["categoryName"]
        data["platformId"] = self.PLATFORM_ID
        data["platform_game_id"] = item["id"]
        data["icon_url"] = item["iconUrl"]
        data["size"] = item["apkSize"]
        data["download_count"] = item["downloadTimes"]
        data["modify_date"] = item["updateTime"]
        data["screenshots"] = item["screenshotsUrl"]
        data["version"] = item["versionCode"]
        data["tags"] = item["tag"]
        data["level"] = item["rating"]
        data["instruction"] = item["brief"]
        data["description"] = item["description"]
        data["deleted"] = 0 
        data['uptime'] = utils.timestampToStr(time.time())
        return data


    def getData(self,start,pageSize,starttime):
        u'获取接口数据'
        # from 渠道号（由360分配） 是
        # start 分页使用的起始偏移量，默认0 否
        # num 每次请求返回的记录最大数量，默认20，最大300 否
        # starttime 按时间返回获取应用的起始时间（Unix时间戳），单位为秒 否
        # endtime 按时间返回获取应用的结束时间（Unix时间戳），单位为秒 否
        # 该接口只提供给服务端获取数据使用，每日限制频次2000；请不要用于客户端；

        url = 'http://api.np.mobilem.360.cn/app/cpsgames'    
        key = 'b3f3bc510098b1a36d0e8da9e2bfb0e5'
        params = {}
        params["from"] = 'lm_227852' 
        params["start"] = start
        params["num"] = pageSize
        params["starttime"] = starttime 
        #params["endtime"] = 0
        tmpPa = utils.dictSorted(params, reverse=True)
        params['sign'] = utils.md5(tmpPa + key)

        r = requests.get(url, params=params)
        data = r.json()
    
        return data

    # 同步下架游戏
    def syncOffline(self):
        printlog(u'开始同步%s下架游戏' % self.PROVIDER_NAME)

        global RequestIdList

        # 获取库中的所有Id
        tableName = 'game_library'
        primaryKey = 'platform_game_id'
        conditionsDict = {"platformId":self.PLATFORM_ID}
        sql = dbtools.getSelectSql(tableName,primaryKey,conditionsDict)
        db = self.getDbInstance()
        rowsCount = db.query(sql)
        if rowsCount <= 0:
            raise AssertionError(u'无法获取数据库中的id')
            return

    
        offLineIdList = [] #下架的游戏ID列表
        for item in db.fetchAllRows():
            id = item[0]
            # 请求中不存在ID，需要下架
            if id in RequestIdList:
                offLineIdList.append(id)
    
        if len(offLineIdList) <= 0:
            printlog(u'完成%s同步下架游戏：没有需要下架的游戏\r\n' % self.PROVIDER_NAME)
            return 

        for id in offLineIdList:
            model = {
                primaryKey:id,
                "deleted":1    
            }
            sql = dbtools.getUpdateSql(tableName,primaryKey,model)
            result = db.update(sql)
            if result[0] != True:
                printlog(u'同步%s下架游戏失败,%s:%s，%s' % (primaryKey , id, result[1],self.PROVIDER_NAME))
    
        printlog(u'完成%s同步下架游戏：需要下架%d款游戏\r\n' % (len(offLineIdList),self.PROVIDER_NAME))

    def sync(self):
        u'同步操作'
        #1.  获取上次同步时间
        #2.  不存在则初始化为同步全部
        #3.  存在则同步判
    
        global RequestIdList
        printlog(u'开始同步[' + self.PROVIDER_NAME + u']数据')
        # 获取上次同步时间 返回一个时间戳
        starttime = settings.getSyncTime(self.PROVIDER_NAME)
        starttime = 0 if starttime == None else starttime
        retryMax = 5
        faildCount = 0
        dbSyncCount = 0
        isSyncAll = False   #是否全量同步
        
        if starttime == None or starttime == 0 or len(starttime) == 0:
            isSyncAll = True
            printlog(u'未获取到%s上次同步时间，设置为全量同步模式' % self.PROVIDER_NAME)
        else:
            printlog(u'%s上次同步时间%s，设置为增量同步模式' % (self.PROVIDER_NAME,utils.timestampToStr(starttime)))
        # http 请求参数
        start = 0       # 分页使用的起始偏移量，默认0
        pageSize = 100  # 每次请求返回的记录最大数量，默认20，最大300

        while(True):
            try: 
                print('')
                if faildCount >= retryMax:
                    printlog(u'获取%s接口数据失败，超过重试次数' % self.PROVIDER_NAME)
                    return

                # 获取接口数据
                data = self.getData(start,pageSize,starttime)
                if data == None:
                    printlog(u'获取%s接口返回数据异常' % self.PROVIDER_NAME,2)

                # 输出错误
                if data.has_key('errno'):
                    errMsg = u'errcode %s ,errMsg:%s' % (data['errno'],data['errMsg'].decode('utf8'))
                    printlog(u'获取%s接口数据失败，详细失败原因：%s' % (self.PROVIDER_NAME, errMsg),2)
                    return
        
                # total 满足条件的总数据数量
                # start 分页使用的起始偏移量
                # num 本次请求返回的数据数量
                # items 具体的应用信息（数组）
                start = int(data["start"]) 
                total = int(data["total"]) 
                num = int(data["num"])
                
                start += pageSize
                printlog(u'%s接口请求 %s 条数据, 偏移量:%d, 总量：%d' % (self.PROVIDER_NAME, pageSize,start,total))

                if total <= 0 and num <= 0:
                    printlog(u'%s接口请求已完成：没有需要同步的数据。' % self.PROVIDER_NAME)
                    break

                modelList = dbtools.convertToModelList(data["items"],self.modelConverter)
                            
                succedList = []
                #有数据需要同步,获取成功同步的game id列表
                succedList = self.syncToDB(modelList)
                dbSyncCount += len(succedList)
                RequestIdList.append(succedList)
                printlog(u'%s数据库已成功同步 %s 条数据, 累计: %s' % (self.PROVIDER_NAME,len(succedList),dbSyncCount))
                if start >= total:
                    printlog(u'%s接口请求已完成：已获取所有数据' % self.PROVIDER_NAME)
                    break
            
                time.sleep(0.2)
            except Exception,e:
                faildCount+=1
                exstr = traceback.format_exc()
                printlog(u"发现异常，异常:%s" % (exstr),3)
                continue

        
        printlog(u'完成%s接口请求 %s 条数据' % (self.PROVIDER_NAME,start))
        printlog(u'完成%s数据库同步 %s 条数据\r\n' % (self.PROVIDER_NAME,dbSyncCount))

        if not isSyncAll and starttime > 0 and len(RequestIdList) > 0:
            self.syncOffline()

        logResult = settings.setSyncTime(self.PROVIDER_NAME,int(time.time()))
        if logResult == False:
            printlog(u'记录%s同步时间失败' % self.PROVIDER_NAME,2)
    
        printlog(u'完成同步[' + self.PROVIDER_NAME + u']数据\n')
        print ('=' * 100 + '\n') * 3
        
    def __init__(self):  
        # 当前接口提供商 ,请配置成数据库的值
        PROVIDER_NAME = '360'
        u'初始化'
        self.PROVIDER_NAME = PROVIDER_NAME
        super(_360,self).__init__(PROVIDER_NAME)


# 入口函数
if __name__ == '__main__':
    pass

