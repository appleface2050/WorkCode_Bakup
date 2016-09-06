'''
Created on 2015/10/28

@author: Peace
'''

import urllib.request, os, re, logging
from datalogs import LogInfo, LogState
from fileutils import FileUtils
from logutils import GlobalData
class DownloadLogs(object):
    '''
    classdocs
    '''

    def __init__(self, params):
        '''
        Constructor
        '''
    
    @staticmethod
    def dUrl(url, name):
        if os.path.isfile(name): os.remove(name)
        urllib.request.urlretrieve(url, name)
        return os.path.isfile(name)
    
    @staticmethod
    def setProxy(proxies):
        if len(proxies) > 0:
            proxy = urllib.request.ProxyHandler(proxies)
            opener = urllib.request.build_opener(proxy)
            urllib.request.install_opener(opener)
    
    @staticmethod
    def parsLogsZip(data):
        """.
    
        result -- csv file
        """
        result = data.resultFile
        logpath = data.logpath
        
        if not os.path.isfile(result): return False
    
        if len(logpath) == 0:
            logpath = os.path.join(os.path.dirname(result),'log');
    
        if (not os.path.isdir(logpath)):
            os.makedirs(logpath);
        logbase = os.path.join(logpath ,r'log_');
        logbase += '{}.zip'
        
        try:
            rows = FileUtils.readerRows(result, data.sheetName)
            rownum = 1;
            index = -1
            for row in rows:
                if 1 != rownum:
                    u = row[index]
                    n = logbase.format(rownum)
                    logInfo = LogInfo(u,n)
                    logInfo.number = rownum
                    data.addLogInfo(logInfo)
                else:
                    index = row.index('Log_File_Path')
                rownum += 1
        except:
            logging.exception('Exception({})'.format(result))
    @staticmethod
    def parsLogsDep(data):
        """.
    
        result -- csv file
        """
        result = data.resultFile
        logpath = data.logpath
        
        if not os.path.isfile(result): return False
    
        if len(logpath) == 0:
            logpath = os.path.join(os.path.dirname(result),'log');
    
        if (not os.path.isdir(logpath)):
            os.makedirs(logpath);
        logbase = os.path.join(logpath ,r'Deploytool_');
        logbase += '{}.log'
        try:
            rows = FileUtils.readerRows(result, data.sheetName)
            rownum = 1
            index = -1
            for row in rows:
                if 1 != rownum:
                    u = GlobalData.deployPreUrl + row[index]
                    n = logbase.format(rownum)
                    logInfo = LogInfo(u,n)
                    logInfo.number = rownum
                    data.addLogInfo(logInfo)
                else:
                    index = row.index('install_log')
                rownum += 1
        except:
            logging.exception('Exception({})'.format(result))
    @staticmethod
    def parsLogsLocal(data):
        logpath = data.logpath
        regNumber = re.compile(r'\d+')
        if os.path.isfile(logpath): #single log file
            log = LogInfo('', logpath)
            ma = regNumber.search(os.path.basename(logpath))
            if ma != None:
                log.number = int(ma.group())
            data.addLogInfo(log)
        elif os.path.isdir(logpath): #directory
            for f in os.listdir(logpath):
                if f.endswith(data.unzipFolder): continue
                log = LogInfo('', os.path.join(logpath,f));
                log.shortName = FileUtils.getShortName(log.name, 3)
                ma = regNumber.search(f)
                if ma != None:
                    log.number = int(ma.group())
                data.addLogInfo(log)
        data.logInfos = sorted(data.logInfos, key= lambda item: item.number)
        
    @staticmethod
    def downloadLog(data, logInfo):
        re = None
        if data.overwrite:
            re = DownloadLogs.dUrl(logInfo.url, logInfo.name)
        elif (not os.path.isfile(logInfo.name)) or (os.path.getsize(logInfo.name) < 1):
            re = DownloadLogs.dUrl(logInfo.url, logInfo.name)
        else:
            pass
        if re == None:
            logInfo.state = LogState.fileExist
        elif re:
            logInfo.state = LogState.downloaded
        else:
            logInfo.state = LogState.downloadFail
        return re
    
    
def main():
    
    pass
if __name__ == '__main__':
    main()
    