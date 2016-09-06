#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import time
import datetime
import requests
import json
import traceback
from common import utils
from dal import dbtools
from config import settings
from providers import providerBase

# 当前接口提供商 ,请配置成数据库的值
PROVIDER_NAME = 'game9'
# 这次同步任务获取的Id列表
RequestIdList = []
# 打印日志
printlog = utils.printlog
#游戏分类编号 游戏分类名称
categoryList = {
'1': u'休闲',
'2': u'竞速',
'3': u'角色',
'4': u'策略',
'5': u'冒险',
'6': u'动作',
'7': u'模拟',
'8': u'体育',
'9': u'射击',
'10':u'棋牌',
'11':u'格斗',
'12':u'益智',
'13':u'回合',
'14':u'即时',
'18':u'赛车',
'19':u'其他',
'25':u'养成',
'35':u'页面',         
'36':u'策略·页面'       ,
'37':u'角色·页面'       ,
'38':u'休闲·页面'       ,
'39':u'策略·页面·端游'  ,
'40':u'模拟·页面'       ,
'41':u'社交'           ,
'48':u'角色·页面·端游'  ,
'49':u'休闲·页面·端游'  ,
'50':u'模拟·页面·端游'  ,
'51':u'其他·页面·端游'  ,
'53':u'塔防'           ,
'54':u'创意奇趣'        ,
'55':u'XBOX LIVE'     ,
'56':u'软件',
'57':u'音乐'}

class game9(providerBase.providerBase):
    u'game 9 接口处理类'
        
    def getCatagoryName(self,index):
        #global categoryList
        key = str(index)
        if categoryList.has_key(key):
            return categoryList[key].decode('utf8')
        return ''
    

    def getAndroidPlatformInfo(self,item):
        u'获取安卓平台信息'
        for p in item["platforms"]:
            if p["platformId"] == 2:
                return p
        return None

    def getLastPackage(self,item):
        u'获取最后一个包'
        packagesList = item["packages"]
        if len(packagesList) == 0:
            return None
        # 返回第一个
        result = packagesList[0]
        return result

    def formatTime(self,str):
        '格式化时间'
        if len(str) == 0:
            return None
        ts = utils.strToTimestamp(str,"%Y%m%d%H%M%S")
        return utils.timestampToStr(ts)

    def convertToModel(self,item):
        u'返回数据项目转换为数据库实体'
    #{
    #	"id": 4976,
    #	"name": "江湖Q传",
    #	"createTime": "20110927115655",
    #	"modifyTime": "20151217153953",
    #	"deleted": 0,
    #	"categoryId": 3,
    #	"keywords": "特色|修炼,特色|装备,适合友友|武侠粉丝,适合友友|宅女宅男,江湖特色|副本",
    #	"packages": [],
    #	"platforms": [{
    #		"id": 42590,
    #		"platformId": 2,
    #		"createTime": "20110927115655",
    #		"modifyTime": "20150601211744",
    #		"active": 1,
    #		"deleted": 0,
    #		"score": 3.9,
    #		"logoImageUrl": "http://image.game.uc.cn/2011/9/27/4042590.jpg",
    #		"screenshotImageUrls": ["http://image.game.uc.cn/2011/9/27/5043432.jpg",
    #		"http://image.game.uc.cn/2011/9/27/5043443.jpg",
    #		"http://image.game.uc.cn/2011/9/27/5043444.jpg",
    #		"http://image.game.uc.cn/2011/9/27/5043445.jpg",
    #		"version": "",
    #		"instruction": "一款集副本、师徒、修炼等系统于一身,风格诙谐幽默的武侠网页手游<br />",
    #		"description": "[特色]副本.师徒.修炼.装备<br />rn[适合]喜欢诙谐、Q版的友友."
    #	}]
    #}

        #1.  根据平台类型筛选手游 platformId=2 安卓 安卓平台
        #2.  有的游戏没有游戏包，无法获取大小
        
        data = {}
        
        data["platformId"] = self.PLATFORM_ID
        data["platform_game_id"] = item["id"]
        data["game_name"] = '' if item["name"] == None else item["name"]
        data["deleted"] = item["deleted"]
        if data["deleted"] == 1:
            # 下架的数据只返回id,name,modifyTime,createTime ,下架游戏时间设置为当前本地时间
            data["modify_date"] = utils.timestampToStr(time.time())
            return data

        if len(item["modifyTime"]) == 0:
            return None

        platform = self.getAndroidPlatformInfo(item)
        package = self.getLastPackage(item)
        if platform == None:
            return None
        if package == None:
            return None
        
        data["modify_date"] = self.formatTime(platform["modifyTime"])
        data["type"] = self.getCatagoryName(item["categoryId"])
        data["icon_url"] = platform["logoImageUrl"]
        data["size"] = package["fileSize"]
        data["download_count"] = 0 #没有下载量
        data["screenshots"] = ','.join(platform["screenshotImageUrls"])
        data["version"] = platform["version"] #有可能为空字符串
        data["tags"] = item["keywords"]
        data["level"] = platform["score"]
        data["instruction"] = platform["instruction"]
        data["description"] = platform["description"]
        data['uptime'] = utils.timestampToStr(time.time())
        return data

    def getSign(self,data,caller,signKey):
        '生成签名'
        dataStr = utils.dictSorted(data, reverse=True)
        dataStr = dataStr.replace('&', '').replace('\r', '')

        forSignStr = '%s%s%s' % (caller, dataStr, signKey)

        result = utils.md5(forSignStr)
        return result

    def getData(self,syncId,pageNum,pageSize,starttime,endtime):
        u'获取接口数据'
    
        # 测试环境
        #caller = "9gameTest"
        #signKey = "3ba620bfbf84974d3f01fa3e017c5e5b"
        #url = 'http://gdc.test4.9game.cn:8039/datasync/getdata'
        # 生产环境
        caller = "UM_WM_21287"
        signKey = "8a20ffadb7309c5335f5a67732f5e66e"
        url = 'http://interface.9game.cn/datasync/getdata'
        encrypt = '' #'base64' 或空字符串

        client = {}
        client["caller"] = caller

        data = {}
        # 需要同步的类型 1：时间范围同步“游戏”信息，2：按照gameId列表查询“游戏”信息 3：查询“排行榜”信息
        data["syncType"] = 1  
        # 需要同步的实体
        data["syncEntity"] = "game,platform,package"
        # 需要同步的数据字段
        data["syncField"] = "game.id,game.name,game.categoryId,platform.logoImageUrl,package.fileSize,platform.screenshotImageUrls,platform.version,game.keywords,platform.score,platform.instruction,platform.description"
        data["dateFrom"] = str(starttime)  # 同步起始日期 所有数据'20000101000000'
        data["dateTo"] = endtime # 同步结束日期
        data["pageSize"] = pageSize  # 每页数量
        data["pageNum"] = pageNum # 请求页码

        params = {}
        params["data"] = data
        params["id"] = syncId
        params["client"] = client
        params["encrypt"] = encrypt 
        params["sign"] = self.getSign(data,caller,signKey)
        #print params["sign"]

        headers = {"Content-Type": "application/json"}
        postData = json.dumps(params)
        r = requests.post(url,data=postData , headers=headers)
        data = r.json()
    
        if encrypt == 'base':
            dataStr = base64.b64decode(data["data"])
            data["data"] = json.loads(dataStr)
        else:
            data["data"] = json.loads(data["data"])
    
        return data

    def filterItem(self,dataList):
        u'过滤下架的项'
        list = []
        for item in dataList:
            # 过滤deleted数据
            if int(item["deleted"]) == 1:
                continue
            # 过滤非安卓平台的包
            platform = self.getAndroidPlatformInfo(item)
            if platform == None:
                continue
            # 过滤没有渠道包的
            package = self.getLastPackage(item)
            if package == None:
                continue

            list.append(item)
        return list

   
    def sync(self):
        '同步操作'
        #1.  获取上次同步时间
        #2.  不存在则初始化为同步全部
        #3.  存在则需要进行增量同步
    
        global RequestIdList
        printlog(u'开始同步[' + self.PROVIDER_NAME + u']数据')
                
        starttime = settings.getSyncTime(PROVIDER_NAME) #获取上次请求时间，返回一个时间戳
        
        faildCount = 0
        retryMax = 5
        dbSyncCount = 0
        isSyncAll = False   #是否全量同步
    
        # http 请求参数
        syncId = int(time.time())   #同步id，同一次同步任务必须相同
        pageSize = 100  #分页大小 最大100
        pageNum = 1     #分页索引
        if starttime == None or starttime == 0 or len(starttime) == 0:
            starttime = '20000101000000'
            isSyncAll = True
            printlog(u'未获取到%s上次同步时间，设置为全量同步模式' % self.PROVIDER_NAME)
        else:
            printlog(u'%s上次同步时间%s，设置为增量同步模式' % (self.PROVIDER_NAME,utils.timestampToStr(starttime)))
            starttime = utils.timestampToStr(starttime,'%Y%m%d%H%M%S')
        endTime = time.strftime('%Y%m%d000000')

        while True:
            try:
                print('')
                if faildCount >= retryMax:
                    printlog(u'获取%s接口数据失败，超过重试次数' % self.PROVIDER_NAME)
                    return

                # 获取接口数据
                data = self.getData(syncId,pageNum,pageSize,starttime,endTime)
            
                if data == None:
                    printlog(u'获取%s接口返回数据异常' % self.PROVIDER_NAME)
                # data
                # total 满足条件的总数据数量
                # state 请求信息
                # list 返回的数据列表
                state = data["state"]
                data = data["data"]
                total = int(data["total"]) 
                start = pageNum * pageSize
                
                printlog(u'%s接口请求 %s 条数据, 偏移量:%d, 总量：%d' % (self.PROVIDER_NAME, pageSize,start,total))

                # state.code 非200 输出错误
                if state["code"] != 200:
                    printlog(u'获取%s接口数据失败，详细失败原因：%s'(state, self.PROVIDER_NAME),2)
                    return

                if total <= 0:
                    printlog(u'获取%s接口数据异常，数据总量为0。3秒后重试...' % self.PROVIDER_NAME,2)
                    faildCount = faildCount + 1
                    time.sleep(3)
                    continue
                
                dataList = data["list"]
                if len(data["list"]) == 0:
                    printlog(u'获取%s接口数据异常，接口列表为空，3秒后重试...' % self.PROVIDER_NAME,2)
                    faildCount = faildCount + 1
                    time.sleep(3)
                    continue
                
                if isSyncAll:
                    # 全量同步过滤deleted数据，增量同步不过滤
                    dataList = self.filterItem(dataList)
                    if len(dataList) == 0:
                        pageNum = pageNum + 1
                        printlog(u'%s返回数据都已下架，跳过操作' % self.PROVIDER_NAME)
                            
                        if start >= total:
                            printlog(u'%s接口请求已完成：已获取所有数据' % self.PROVIDER_NAME)
                            break
                        else:
                            continue
        
                modelList = dbtools.convertToModelList(dataList,self.convertToModel)

                #有数据需要同步,获取成功同步的id列表
                succedList = []
                succedList = self.syncToDB(modelList)
                dbSyncCount += len(succedList)
                #RequestIdList.append(succedList)
                printlog(u'%s数据库已成功同步 %s 条数据, 累计: %s' % (self.PROVIDER_NAME,len(succedList),dbSyncCount))
                        
                pageNum = pageNum + 1
                if start >= total:
                    printlog(u'%s接口请求已完成：已获取所有数据' % self.PROVIDER_NAME)
                    break
        
                time.sleep(0.2)
            except requests.RequestException,e:
                printlog(u"发现异常，异常:http请求错误,%s" % (e.message),3)
                time.sleep(1)
                continue

            except Exception,e:
                faildCount+=1
                exstr = traceback.format_exc()
                printlog(u"发现异常，异常:%s" % (exstr),3)
                continue
            
        printlog(u'完成%s接口请求 %s 条数据' % (self.PROVIDER_NAME,start))
        printlog(u'完成%s数据库同步 %s 条数据\r\n' % (self.PROVIDER_NAME,dbSyncCount))
        
        #if starttime > 0 and len(RequestIdList) > 0:
        #    self.syncOffline()
        
        logResult = settings.setSyncTime(self.PROVIDER_NAME,int(time.time()))
        if logResult == False:
            printlog(u'记录%s同步时间失败' % self.PROVIDER_NAME)
    
        printlog(u'完成同步[' + self.PROVIDER_NAME + u']数据\n')
        print ('=' * 100 + '\n') * 3
    
    def __init__(self):    
        u'初始化'
        self.PROVIDER_NAME = PROVIDER_NAME
        super(game9,self).__init__(PROVIDER_NAME)

            
# 入口函数
if __name__ == '__main__':
    pass
