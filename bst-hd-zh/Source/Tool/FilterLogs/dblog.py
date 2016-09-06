'''
Created on 2015/11/24

@author: Peace
'''
import datetime,os
import sqlite3
from fileutils import FileUtils
from datalogs import KeyWord, LogInfo, LogState
from macpath import curdir

class DbLog(object):
    '''
    classdocs
    '''
    def __init__(self, fileName='',notAutoCommit=False):
        '''
        Constructor
        '''
        if fileName == None or len(fileName) < 1:
            fileName = os.path.join(FileUtils.getExecutePath(),'logs.db')
        self.dbFile = fileName
        tcon = None
        if notAutoCommit:
            tcon = sqlite3.connect(self.dbFile, detect_types=sqlite3.PARSE_DECLTYPES|sqlite3.PARSE_COLNAMES,check_same_thread=False)
        else:
            tcon = sqlite3.connect(self.dbFile, detect_types=sqlite3.PARSE_DECLTYPES|sqlite3.PARSE_COLNAMES,check_same_thread=False, isolation_level=None)
        self.conn = tcon
        if True:
            c = self.conn.cursor()
            sql='''\
            CREATE TABLE if not exists [loginfo] (
            [shortName] NVARCHAR(128)  NOT NULL PRIMARY KEY,
            [url] NVARCHAR(512)  NULL,
            [name] NVARCHAR(512)  NULL,
            [state] NVARCHAR(32)  NULL,
            [keyword] NVARCHAR(64)  NULL,
            [number] INTEGER  NULL,
            [tgpString] NVARCHAR(128)  NULL,
            [qqdate] TIMESTAMP  NULL,
            [qqid] NVARCHAR(32)  NULL,
            [emaildate] TIMESTAMP  NULL,
            [interpretation] NVARCHAR(128)  NULL,
            [unifycode] NVARCHAR(64)  NULL,
            [updatedate] TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL
            )
            '''
            c.execute(sql)
            self.conn.commit()
            sql = '''\
            CREATE TABLE if not exists [keywords] (
            [unifycode] NVARCHAR(64)  NOT NULL PRIMARY KEY,
            [interpretation] NVARCHAR(128)  NULL,
            [behaviour] NVARCHAR(256)  NULL,
            [solution] NVARCHAR(1024)  NULL
            )
            '''
            c.execute(sql)
            self.conn.commit()
            
            sql = '''\
            CREATE TABLE if not exists [logschina] (
            [url] NVARCHAR(512)  UNIQUE NULL PRIMARY KEY,
            [errorReason] NVARCHAR(512)  NULL,
            [errorCode] INTEGER  NULL,
            [guid] NVARCHAR(37)  NULL,
            [version] NVARCHAR(24)  NULL,
            [oem] NVARCHAR(16)  NULL,
            [package] NVARCHAR(128)  NULL,
            [date] TIMESTAMP  NULL,
            [type] NVARCHAR(16)  NULL,
            [installType] NVARCHAR(16)  NULL,
            [country] NVARCHAR(8)  NULL
            )
            '''
            c.execute(sql)
            self.conn.commit()
            
    def close(self):
        if self.conn != None: 
            self.conn.commit()
            self.conn.close()
    def insertOrUpdateLogInfo(self, logInfo, cur=None):
        c = cur
        if c == None:
            c = self.conn.cursor()
        sql = '''\
        insert or replace into loginfo ([shortName],[url],[name],[state],[number],[tgpString],[qqdate],[qqid],[emaildate],[interpretation],[unifycode]) 
                                  values( ?,          ?,     ?,     ?,      ?,      ?,           ?,      ?,      ?,          ?,               ?)
        '''
        unifycode = ''
        interpretation = ''
        if logInfo.keyword != None:
            unifycode = logInfo.keyword.unifycode
            interpretation = logInfo.keyword.interpretation
        pars = (logInfo.shortName, logInfo.url, logInfo.name, logInfo.state.name,logInfo.number,logInfo.tgpString, 
#                 datetime.datetime.strftime(logInfo.date, '%Y/%m/%d %H:%M:%S'),
                logInfo.date,
                logInfo.qqid, 
#                 datetime.datetime.strftime(logInfo.emaildate, '%Y-%m-%d'),
                logInfo.emaildate,
                interpretation, unifycode,)
        c.execute(sql, pars)
#         self.conn.commit()
        c.close()
    def insertOrUpdateLogsChina(self, logsChina, cur=None):
        '''
        insert or update the record
        '''
        c = cur
        if c == None:
            c = self.conn.cursor()

        sql = '''\
        insert or replace into logschina ([url],[errorReason],[errorCode],[guid],[version],[oem],[package],[date],[type],[installType],[country]) 
                                  values( ?,     ?,            ?,          ?,     ?,        ?,    ?,        ?,     ?,     ?,            ?)
        '''
        date = None
        if logsChina.date != None:
            date = datetime.datetime.strftime(logsChina.date, '%Y/%m/%d %H:%M:%S')
        pars = (logsChina.url, logsChina.errorReason, logsChina.errorCode, logsChina.guid,logsChina.version,logsChina.oem, 
                logsChina.package,
                date, 
                logsChina.type,
                logsChina.installType,
                logsChina.country)
        c.execute(sql, pars)
#         self.conn.commit()
        c.close()
    
    def commit(self):
        self.conn.commit()
        
    def __rowToLogInfo(self, row):
        old = None
        if row != None:
            old = LogInfo()
            old.shortName = row[0]
            old.url = row[1]
            old.name = row[2]
            old.state = LogState.strToLoState(row[3])
            old.number = row[4]
            old.tgpString = row[5]
            old.date = row[6]
            old.qqid = row[7]
            old.emaildate = row[8]
            old.keyword = KeyWord()
            old.keyword.interpretation = row[9]
            old.keyword.unifycode = row[10]
        return old

    def getOldLogInfo(self, logInfo):
        c = self.conn.cursor()
        sql = '''\
        select [shortName],[url],[name],[state],[number],[tgpString],[qqdate],[qqid],[emaildate],[interpretation],[unifycode] from loginfo
        where shortName=?
        '''
        c.execute(sql, (logInfo.shortName,))
        row = c.fetchone()
        old = self.__rowToLogInfo(row)
        c.close()
        return old
    def getLogInfos(self):
        c = self.conn.cursor()
        sql = '''\
        select [shortName],[url],[name],[state],[number],[tgpString],[qqdate],[qqid],[emaildate],[interpretation],[unifycode] from loginfo
        '''
        c.execute(sql)
        for row in c:
            logInfo = self.__rowToLogInfo(row)
            yield logInfo
        c.close()
    
    def matchTgbLog(self, qqid, date):
        logs = [] 
        if qqid == None or len(qqid) < 1 or date ==None:
            return logs
        c = self.conn.cursor()
        sql = '''\
        select [shortName],[url],[name],[state],[number],[tgpString],[qqdate],[qqid],[emaildate],[interpretation],[unifycode] from loginfo
        where qqid=? and qqdate < ? and qqdate > ? 
        '''
        date2 = date - datetime.timedelta(days=2)
        rows = c.execute(sql, (qqid,date, date2,))
        
        for row in rows:
            log = self.__rowToLogInfo(row)
            if log != None:
                logs.append(log)
        c.close()
        return logs
    
    @staticmethod
    def copyLogInfo(source, dest):
        dest.shortName = source.shortName
        dest.url       = source.url
        dest.name      = source.name
        dest.state     = source.state
        dest.number    = source.number
        dest.tgpString = source.tgpString
        dest.date      = source.date
        dest.qqid      = source.qqid
        dest.emaildate = source.emaildate
        dest.keyword   = source.keyword
        return dest
    
        
