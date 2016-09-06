'''
Created on 2015/10/28

@author: Peace
'''
import os
from enum import Enum
from logutils import GlobalData, ErrorType
import datetime
from fileutils import FileUtils

class LogState(Enum):
    add = 1
    downloaded = 2
    downloadFail = 3
    fileExist = 4
    found = 5
    nofound = 6
    
    @staticmethod
    def strToLoState(name):
        re = None
        if name == 'add':
            re = LogState.add
        elif name == 'downloaded':
            re = LogState.downloaded
        elif name == 'downloadFail':
            re = LogState.downloadFail
        elif name == 'fileExist':
            re = LogState.fileExist
        elif name == 'found':
            re = LogState.found
        elif name == 'nofound':
            re = LogState.nofound
        return re
class KeyExPress(object):
    '''
    '''
    def __init__(self):
        self.strs = []
        self.regs = []

class KeyWord(object):
    '''
            <unifycode></unifycode>
            <interpretation></interpretation>
            <behaviour></behaviour>
            <check></check>
            <solution></solution>
    '''    
    
    def __init__(self):
        self.orExpresss = KeyExPress()
        self.andList = []
        self.index=-1
        
        self.logFile = ''
        self.unifycode = ''
        self.interpretation = ''
        self.behaviour = ''
        self.check = ''
        self.solution = ''
        
        self.fun = None
        
        self.errorType = None
        
        self.xmlText = ''
    def hasCondition(self):
        result = (len(self.orExpresss.regs) > 0 or len(self.orExpresss.strs) > 0 or len(self.andList) > 0)
        return result
    
    @staticmethod
    def toRow(keyword):
        row = None
        if keyword == None:
            row = ['None','None','','']
        else:
            row = [keyword.unifycode, keyword.interpretation, keyword.behaviour,keyword.solution]
        return row
    def toObject(self, row):
        if row[0] != 'None':
            self.unifycode = row[0]
        if row[1] != 'None':
            self.interpretation = row[1]
        
        self.behaviour = row[2]
        self.solution = row[3]
        return 3
    
class LogInfo(object):
    '''
    
    '''
    
    def __init__(self, url='', name=''):
        self.shortName = ''
        self.url = url
        self.name = name
        self.state = LogState.add
        self.keyword = None
        self.number = 0
        self.tgpString=''
        self.date=None
        self.qqid=''
        self.emaildate = None
        self.matchKeys = [] #keys that match
        
        #
        self.logsChinaId=''

    def toRow(self):
        temp = self.date and datetime.datetime.strftime(self.date, GlobalData.logDateFormat) or ''
        ks = KeyWord.toRow(self.keyword)
        row = [self.url,self.name,self.state.name,self.number,self.tgpString,temp,self.qqid]
        ks.extend(row)
        return ks
    def toRowMatchKeys(self):
        temp = self.date and datetime.datetime.strftime(self.date, GlobalData.logDateFormat) or ''
        row = [self.url,self.name,self.state.name,self.number,self.tgpString,temp,self.qqid]
        for k in self.matchKeys:
            row.extend(KeyWord.toRow(k))
        return row
    def toLogInfo(self,row):
        if self.keyword == None:
            self.keyword = KeyWord()
        si = self.keyword.toObject(row)
        si += 1
        self.url = row[si+0]
        self.name = row[si+1]
        self.state = LogState.strToLoState(row[si+2])
        self.number = int(row[si+3])
        self.tgpString=row[si+4]
        temp = row[si+5]
        if temp != None and len(temp) > 0:
            temp = datetime.datetime.strptime(temp, GlobalData.logDateFormat)
        else:
            temp = None
        self.date=temp
        self.qqid=row[si+6]
    
class LogsChina(object):
    def __init__(self):
        self.url = ''
        self.errorReason = ''
        self.errorCode = 0
        self.guid = ''
        self.version = ''
        self.oem = ''
        self.package=''
        self.date=None
        self.type=''
        self.installType=''
        self.country=''
        
    InstallTypeUpdate = 'update'
    InstallTypeInstall= 'install'
    TypeDeploy = ErrorType.deploy.name
    TypeApk = ErrorType.apk.name
    TypeBoot = ErrorType.boot.name
    
class DataLogs(object):
    '''
    classdocs
    '''


    def __init__(self):
        '''
        Constructor
        '''
        self.resultFile = ''
        self.resultOutFile = ''
        self.keywordFile = ''
        self.dellog = False
        self.delUnzip = False
        self.justdownload = False
        self.overwrite = False
        self.proxy = None
        self.localLogs = False
        self.refilter = False
        self.allkeys = False
        
        self.errorType = None
        
        self.logpath = ''
        self.sheetName = ''
        self.logInfos = []
        self.callPasreLogs = lambda data :[]
        self.callDownload = lambda data, loginfo: False
        self.callQuery = lambda data, loginfo:False
        
        self.keywords = []
        
        self.minLog = 1
        self.unzipFolder = 'unzip_'
        
        self.dblog = None
        
    def addLogInfo(self, logInfo):
        if logInfo.shortName == None or len(logInfo.shortName) < 1:
            logInfo.shortName = FileUtils.getShortName(logInfo.name, 3)
        if logInfo.emaildate == None and (logInfo.shortName != None) and (len(logInfo.shortName) > 0):
            temp = logInfo.shortName[0: logInfo.shortName.index(os.path.sep)]
            if temp != None:
                items = temp.split('-')
                if len(items) ==3:
                    try:
                        logInfo.emaildate = datetime.datetime(int(items[0]),int(items[1]), int(items[2]))
                    except:
                        pass
        self.logInfos.append(logInfo)
    
    def getLogInfo(self, index):
        return self.logInfos[index]
    
    def addKeyword(self, keyWord):
        self.keywords.append(keyWord)
    
    def getKeyword(self, index):
        return self.keywords[index]
            