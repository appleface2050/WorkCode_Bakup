'''
Created on 2015/10/28

@author: Peace
'''

from datalogs import LogState, KeyWord, KeyExPress
import zipfile, os, shutil, csv,re
import xml.etree.ElementTree
import tgpstring
import logging
from logutils import GlobalData
from fileutils import FileUtils

class QueryLogs(object):
    '''
    classdocs
    '''


    def __init__(self, params):
        '''
        Constructor
        '''
       
    @staticmethod
    def query(data, logInfo): 
        
        # unzip logFile
        if os.path.isdir(logInfo.name):
            folder = logInfo.name
        elif logInfo.name.endswith('.zip'):
            folder = logInfo.name.rstrip('.zip')
            tname = os.path.basename(folder)
            folder = folder.rstrip(tname)
            folder = os.path.join(folder,data.unzipFolder,tname)
            
            if os.path.isdir(folder): shutil.rmtree(folder)
            with zipfile.ZipFile(logInfo.name) as zfile:
                zfile.extractall(folder)
                zfile.close()
        else:
            folder = logInfo.name
            
        if len(data.keywords) > 0: QueryLogs.queryFiles(data,logInfo,folder)
        logInfo.tgpString, logInfo.date = tgpstring.getTgpString(folder)
        re = False
        if logInfo.state == LogState.found : 
            re = True
            if data.dellog:
                try:
                    if os.path.isfile(logInfo.name):
                        os.remove(logInfo.name)
                    elif os.path.isdir(logInfo.name):
                        shutil.rmtree(folder)
                    if os.path.isfile(folder):
                        os.remove(folder)
                    elif os.path.isdir(folder):
                        shutil.rmtree(folder)
                except:
                    logging.exception('Exception(delete {}/{})'.format(logInfo.name, folder))
        if data.delUnzip:
            if logInfo.name.endswith('.zip') and os.path.exists(folder):
                try:
                    shutil.rmtree(folder)
                except:
                    logging.exception('Exception(delete {})'.format(folder))
        return re
    @staticmethod
    def parseKeyword(data):
        try:
            node = xml.etree.ElementTree.parse(data.keywordFile).getroot()
            node = node.find('words')
            for w in node:
                kw = KeyWord()
                kw.xmlText = xml.etree.ElementTree.tostring(w).decode()
                
                dses = w.findall('conditions/str')
                if dses != None and len(dses) > 0:
                    for ds in dses:
                        if ds.text !=None and len(ds.text) > 0:
                            kw.orExpresss.strs.append(ds.text)
                
                dses = w.findall('conditions/reg')
                if dses != None and len(dses) > 0:
                    for ds in dses:
                        if ds.text !=None and len(ds.text) > 0:
                            kw.orExpresss.regs.append(re.compile(ds.text))
                
                df = w.find('conditions/logfile')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.logFile = df.text 
                
                df = w.find('conditions/errorType')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.errorType = df.text
                
                dsAnds = w.findall('conditions/and')
                if dsAnds != None and len(dsAnds) > 0:
                    
                    for dsAnd in dsAnds:
                        keyEx = KeyExPress()
                        dses = dsAnd.findall('str')
                        if dses != None and len(dses) > 0:
                            for ds in dses:
                                if ds.text !=None and len(ds.text) > 0:
                                    keyEx.strs.append(ds.text)
                        
                        dses = dsAnd.findall('reg')
                        if dses != None and len(dses) > 0:
                            for ds in dses:
                                if ds.text !=None and len(ds.text) > 0:
                                    keyEx.regs.append(re.compile(ds.text))
                        if len(keyEx.strs) > 0 or len(keyEx.regs) >0 :
                            kw.andList.append(keyEx)
                
                
                df = w.find('unifycode')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.unifycode = df.text 
                df = w.find('interpretation')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.interpretation = df.text 
                df = w.find('behaviour')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.behaviour = df.text 
                df = w.find('check')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.check = df.text 
                df = w.find('solution')
                if df != None and df.text != None and len(df.text) > 0:
                    kw.solution = df.text 
            
                if kw.unifycode != None and len(kw.unifycode) > 0 : data.addKeyword(kw)
        except:
            logging.exception('Exception({})'.format(data.keywordFile))
        
        QueryLogs.initBeforeKeywords(data)
        
        return True
    
    @staticmethod
    def queryFiles(data,loginfo,fd):
        
        if QueryLogs.runBeforeKeywords(data, loginfo, fd):
            return
        
        files = []
        if(os.path.isfile(fd)):
            files.append(fd)
        else:
            for root,dirs, fs in os.walk(fd):
                for f in fs:
                    files.append(os.path.join(root,f))
        for fd in files:
            #print('start ' + fd)
            try:
                allText = FileUtils.readAll(fd)
                if allText == None or len(allText) < 1: continue
                #print('end ' + fd)
                oldstate = loginfo.state
                for kw in data.keywords:
                    loginfo.state = oldstate
                    if kw.fun != None:
                        kw.fun(data, loginfo, kw, fd, allText)
                    if loginfo.state == LogState.found:
                        if data.allkeys:
                            loginfo.matchKeys.append(loginfo.keyword)
                        else:
                            break;
                if data.allkeys and len(loginfo.matchKeys) > 0:
                    loginfo.state = LogState.found
            except:
                logging.exception('Exception({})'.format(fd))
            if loginfo.state == LogState.found: break
        if loginfo.state != LogState.found: loginfo.state = LogState.nofound
        return None
    
    @staticmethod
    def runBeforeKeywords(data, logInfo, fd):
        result = False
        for keyword in QueryLogs.beforeKeywords:
            if keyword.fun != None:
                result = keyword.fun(data,logInfo, keyword,fd, '')
                if result:
                    break
        return result
    @staticmethod
    def initBeforeKeywords(data):
        QueryLogs.beforeKeywords = []
        key = KeyWord()
        key.fun = FilterArithmetic.checkNotExistLogsFolder
        key.unifycode = 'oth50'
        QueryLogs.beforeKeywords.append(key)
        indexes = []
        for index, val in enumerate(data.keywords):
            find = False
            for itKey in QueryLogs.beforeKeywords:
                if itKey.unifycode == val.unifycode:
                    itKey.interpretation = val.interpretation
                    indexes.append(index)
                    find = True
                    break
            if not find:
                val.fun = FilterArithmetic.common
            
        for index in reversed(indexes):
            data.keywords.pop(index)
        
    beforeKeywords = []
def outResult(data):
    tgpstring.getQqidsByData(data)
    outFile = data.resultOutFile
    if os.path.isfile(outFile):
        os.remove(outFile)
    if not os.path.exists(os.path.dirname(outFile)):
        os.makedirs(os.path.dirname(outFile))
    
    findLog = 0
    try:
        logInfoOut =GlobalData.getLogInfoOutName(outFile)
        with open(logInfoOut,'w', encoding="utf-8",newline='') as w:
            writer = csv.writer(w)
            if data.allkeys:
                for logInfo in data.logInfos:
                    if LogState.found == logInfo.state:
                        findLog += 1
                    row = logInfo.toRowMatchKeys()
                    writer.writerow(row)
            else:
                for logInfo in data.logInfos:
                    if LogState.found == logInfo.state:
                        findLog += 1
                    row = logInfo.toRow()
                    writer.writerow(row)
    except:
        logging.exception('Exception({})'.format(outFile))
        
    logging.info('\r\nTotal log: {}  find log: {}\r\n'.format(len(data.logInfos), findLog))
        
class FilterArithmetic(object):
    '''
    arithmetic of filter log
    '''
    @staticmethod
    def checkNotExistLogsFolder(data,logInfo, kw,fd, allText):
        '''
        if the folder 'Logs' exist, return true
        '''
        result = False
        if os.path.isdir(fd):
            lpath = os.path.join(fd, 'Logs')
            if not os.path.isdir(lpath):
                logInfo.state = LogState.found
                logInfo.keyword = kw
                result = True
        return result
    
    @staticmethod
    def common(data,logInfo, kw, fd, allText):
        '''
        calculate the "and reg str" in keywords.xml
        '''
        result = False
        
        if len(kw.logFile) > 0 and (not os.path.basename(fd).startswith(kw.logFile)) : return result
                    
        if (kw.errorType != None) and (len(kw.errorType) > 0) and (data.errorType != None) and (len(data.errorType) > 0) and  (kw.errorType != data.errorType): return result
        
        for one in kw.orExpresss.strs:
            if len(one) > 0 and (one in allText):
                logInfo.state = LogState.found
                logInfo.keyword = kw
                result = True
                return result
        
        for one in kw.orExpresss.regs:
            if one != None and one.search(allText) != None:
                logInfo.state = LogState.found
                logInfo.keyword = kw
                result = True
                return result
            
        if len(kw.andList) > 0:
            for andEs in kw.andList:
                andMatch = True
                for one in andEs.strs:
                    if len(one) > 0 and (one not in allText):
                        andMatch = False
                        break
                if not andMatch: continue
                for one in andEs.regs:
                    if one != None and one.search(allText) == None:
                        andMatch = False
                        break
                
                if andMatch:
                    logInfo.state = LogState.found
                    logInfo.keyword = kw
                    result = True
                    break
        return result
    