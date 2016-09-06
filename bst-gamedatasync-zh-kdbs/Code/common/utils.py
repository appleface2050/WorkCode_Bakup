#!/usr/bin/env python
# -*- coding: utf-8 -*-

import hashlib
import time
import os
import sys



reload(sys)
sys.setdefaultencoding("utf-8")



def timestructToTimestamp(timestruct):
    u'时间结构转时间戳'
    return time.mktime(timestruct)

def timestructToTimestamp(timestruct):
    u'时间结构转时间戳'
    return time.mktime(timestruct)

def timestampToTimestruct(timestamp):
    u'时间戳转时间结构'
    return time.localtime(float(timestamp))

def timestampToStr(timestamp,format='%Y-%m-%d %H:%M:%S'):
    u'时间戳转字符串'
    timestruct = time.localtime(float(timestamp))
    return time.strftime(format,timestruct)

def strToTimestruct(str,format='%Y-%m-%d %H:%M:%S'):
    u'字符串转时间结构'
    #time.strptime("2013-05-21 09:50:35","%Y-%m-%d %H:%M:%S")
    return time.strptime(str,format)

def strToTimestamp(str,format='%Y-%m-%d %H:%M:%S'):
    u'字符串转时间戳'
    timestruct = time.strptime(str,format) #strToTimestruct
    return time.mktime(timestruct)


def printmsg(msg):
    u'打印消息'
    print "%s: %s" % (time.strftime('%Y-%m-%d %H:%M:%S'),msg)

def writeFile(filePath,fileContent,fileMode='w'):
    u'''写文件
        w     以写方式打开，
        a     以追加模式打开 (从 EOF 开始, 必要时创建新文件)
        r+    以读写模式打开
        w+    以读写模式打开 (参见 w )
        a+    以读写模式打开 (参见 a )
        rb    以二进制读模式打开
        wb    以二进制写模式打开 (参见 w )
        ab    以二进制追加模式打开 (参见 a )
        rb+   以二进制读写模式打开 (参见 r+ )
        wb+   以二进制读写模式打开 (参见 w+ )
        ab+   以二进制读写模式打开 (参见 a+ )
    '''
    dirName = os.path.dirname(filePath)
    if not os.path.isdir(dirName):
        return
    if not os.path.exists(dirName):
        os.makedirs(dirPath)
    #if not os.path.exists(filePath):
    #    return
    
    fp = open(filePath,fileMode)
    if  not isinstance(fileContent,unicode) or isinstance(fileContent,str) :
        fileContent = str(fileContent)
    fp.write(fileContent)
    fp.close()

def readFile(filePath,fileMode='r'):
    u'''读取文件,
        w     以写方式打开，
        a     以追加模式打开 (从 EOF 开始, 必要时创建新文件)
        r+    以读写模式打开
        w+    以读写模式打开 (参见 w )
        a+    以读写模式打开 (参见 a )
        rb    以二进制读模式打开
        wb    以二进制写模式打开 (参见 w )
        ab    以二进制追加模式打开 (参见 a )
        rb+   以二进制读写模式打开 (参见 r+ )
        wb+   以二进制读写模式打开 (参见 w+ )
        ab+   以二进制读写模式打开 (参见 a+ )
    '''
    if not os.path.isfile(filePath):
        return ''
    dirName = os.path.dirname(filePath)
    if not os.path.exists(filePath):
        return ''
    
    fp = open(filePath,fileMode)
    s = fp.readlines()
    fp.close()
    return s

def logFile(log,logType):
    u'''写日志文件(文件名为年_月_日)
        logType:1 消息，2 错误 3 异常 '''
    name = time.strftime('%Y_%m_%d')
    logName = name + '.log'
    #dirPath = os.getcwd() + '/logs/' 
    dirPath = "/tmp/GameDataSynclogs/"
    filePath = dirPath + logName
    if not os.path.exists(dirPath):
        os.mkdir(dirPath)
    if not os.path.exists(filePath):
        writeFile(filePath,'','w')
    
    typeName = ''
    if logType == 1:
        typeName = 'Message'
    elif logType == 2:
        typeName = 'Error'
    elif logType == 3:
        typeName = 'Exception'
    else:
        typeName = 'Others'

    logContent = '\nDate: ' + time.strftime("%Y-%m-%d %H:%M:%S",time.localtime())
    logContent+= '\nType: ' + typeName
    logContent+= '\nDetails: ' + log
    logContent+= '\n' + ('=' * 50)
    logContent = logContent

    writeFile(filePath,logContent,'a')

def printlog(msg,logType=1):
    u'打印并写日志,logType 默认为1 消息，2 错误，3 异常'
    printmsg(msg)
    logFile(msg,logType)
        

def dictSorted(d,reverse=False):
    u'字典排序'
    u'false'
    strPa = ''
    list = sorted(d.keys())
    for key in list:
        strPa+=  (str(key) + '=' + str(d[key]) + '&')

    if strPa[-1] == '&':
        return strPa[:-1]
    return strPa

def md5(s):
    u'MD5加密，返回加密后小写的字符串'
    s = str(s)
    return hashlib.md5(s).hexdigest()
def md5Upper(s):
    s = md5(s)
    return s.upper()

def _decode_list(data):
    u'列表转为utf8编码'
    rv = []
    for item in data:
        if isinstance(item, unicode):
            item = item.encode('utf-8')
        elif isinstance(item, list):
            item = _decode_list(item)
        elif isinstance(item, dict):
            item = _decode_dict(item)
        rv.append(item)
    return rv

def _decode_dict(data):
    u'字典转为utf8编码'
    rv = {}
    for key, value in data.iteritems():
        if isinstance(key, unicode):
            key = key.encode('utf-8')
        if isinstance(value, unicode):
            value = value.encode('utf-8')
        elif isinstance(value, list):
            value = _decode_list(value)
        elif isinstance(value, dict):
            value = _decode_dict(value)
        rv[key] = value
    return rv


def post(url,data={},headers={}):
    u'post 请求一个地址,返回一个 requests 中的 response 对象'
    url = url
    if headers == None or len(headers) == 0:
        headers = {
            "Content-Type":"application/json",
            'Connection':'keep-alive'
        }
    headers = headers

    #paramaters = urllib.urlencode(data)
    try:
        resp = requests.post(url, data=data,headers=headers)
    except Exception,e :
        print 'Exception:' + e.message

    return resp

def postText(url,data):
    u'post 请求一个地址，返回请求的文本'
    headers = {
            "Content-Type":"text/plain",
            'Connection':'keep-alive'
        }
    resp = post(url,post,headers)
    if resp != None:
        return resp.content
    return ''
