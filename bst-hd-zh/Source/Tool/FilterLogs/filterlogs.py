'''
Created on 2015/10/28

@author: Peace

Automatic filter log files
'''
from downloadlogs import DownloadLogs
from datalogs import DataLogs, LogState
from querylogs import QueryLogs 
import concurrent.futures, argparse,os,querylogs,datetime
import logutils
import logging

from dblog import DbLog
import tgpstring


def handleParameters():   
    #parameters
    parser = argparse.ArgumentParser(description='Automatic filter log files.',
        formatter_class=logutils.MyFormat,
        epilog='''\
sample1:  local logs
py filterlogs.py -l="c:\\logs" -o="c:\\result\\outResult.csv"

sample2: deploy logs(need download logs)
py filterlogs.py -deploy -r="c:\\deploy_result.csv" -o="c:\\result\\outResult.csv"

sample3: not deploy logs(need download logs)
py filterlogs.py -r="c:\\app_result.csv" -o="c:\\result\\outResult.csv"

sample4: delete matach logs, after search logs
py filterlogs.py -d -r="c:\\app_result.csv" -o="c:\\result\\outResult.csv"

sample5: just download logs
py filterlogs.py -j -r="c:\\app_result.csv" -o="c:\\result\\outResult.csv"

sample6:  set max thead and max logs
py filterlogs.py -maxthreads=30 -maxlog=1000 -l="c:\\logs" -o="c:\\result\\outResult.csv"
        ''')
    
    logutils.addSharedParameters(parser)
    parser.add_argument('-deploy', help='whether is deploy tool log, just one log file',default =False, action='store_true')
    args = parser.parse_args() 
    args.sheetName = ''
    logging.info(args)
    return args


def HandleLog(data, loginfo):

    try:
        if not data.refilter:
            old = data.dblog.getOldLogInfo(loginfo)
            if old != None and (old.state == LogState.found or old.state == LogState.nofound) :
                DbLog.copyLogInfo(old, loginfo)
                return
        
        logging.info('start({})'.format(loginfo.name))
        if data.localLogs:
            data.callQuery(data, loginfo)
        elif len(loginfo.url) > 0 and len(loginfo.name) > 0:
            if data.overwrite or (not os.path.isfile(loginfo.name)):
                if data.callDownload(data,loginfo): 
                    logging.info('download success: log file({}), url({})'.format(loginfo.name, loginfo.url))
                else:
                    logging.info('download failed: log file({}), url({})'.format(loginfo.name, loginfo.url))
            if not data.justdownload: 
                data.callQuery(data, loginfo)
                logging.info("{},{}".format(loginfo.name, loginfo.state == LogState.found))
        logging.info('end({})'.format(loginfo.name))
        if loginfo.tgpString != None and len(loginfo.tgpString) > 0:
            loginfo.qqid = tgpstring.getQqid(loginfo.tgpString)
        data.dblog.insertOrUpdateLogInfo(loginfo)
    except:
        logging.exception('Exception({})'.format(loginfo.name))

def doFilter(args):
    
    #validate parameters
    logpath = args.l
    if len(args.r) > 0:
        if not os.path.isfile(args.r):
            logging.error('parameter "result" is not valid file\r\n')
            exit()
        args.localLogs = False
    else:
        if not (os.path.isfile(logpath) or os.path.isdir(logpath)):
            logging.error('parameter "logpath" is not valid file\r\n')
            exit()
        args.localLogs = True
    if not os.path.isfile(args.k):
        logging.error('the parameter "keywords" is not valid file\r\n')
        exit()
    if len(args.o) < 1:
        if len(args.r) > 0:
            args.o = args.r + 'out.csv'
        else:
            args.o = os.path.join(os.path.dirname(logpath), 'result_out.csv')
    if args.maxlog !=0 and args.minlog > args.maxlog:
        logging.error('parameter maxlog must greater than parameter minlog')
        exit()
    if len(args.proxy) > 0:
        try:
            eval('{{{}}}'.format(args.proxy))
        except:
            logging.exception('parameter proxy is invalide')
            exit()
    #set data value
    data = DataLogs()
    data.resultFile = args.r
    data.resultOutFile = args.o
    data.keywordFile = args.k
    data.dellog = args.d
    data.justdownload = args.j
    data.overwrite = args.overwrite
    if len(args.proxy) > 0: data.proxy = eval('{{{}}}'.format(args.proxy))
    data.localLogs = args.localLogs
    data.logpath = logpath
    data.delUnzip = args.delunzip
    data.sheetName = args.sheetName
    data.errorType = args.errorType
    data.refilter = args.refilter
    data.allkeys = args.allkeys
    
    if data.localLogs:
        data.callPasreLogs = DownloadLogs.parsLogsLocal
    elif args.deploy:
        data.callPasreLogs = DownloadLogs.parsLogsDep
    else:
        data.callPasreLogs = DownloadLogs.parsLogsZip
    data.callParseKeyword = QueryLogs.parseKeyword
    data.callQuery = QueryLogs.query
    data.callDownload = DownloadLogs.downloadLog
    
    data.dblog = DbLog()
    
    data.callPasreLogs(data)
    
    #read and parse keywords.xml
    data.callParseKeyword(data)
    
    #fix the max logs
    if args.minlog > len(data.logInfos):
        logging.error('parameter minlog must less than total logs')
        exit()
    if True:
        minIndex = args.minlog -1
        if minIndex < 0:
            minIndex = 0
        maxLog = args.maxlog
        if maxLog > len(data.logInfos) or maxLog == 0:
            maxLog = len(data.logInfos)
        
        data.logInfos = data.logInfos[minIndex:maxLog]
        data.minLog = minIndex + 1
    
    if data.proxy != None: DownloadLogs.setProxy(data.proxy)

    #concurrence handle logs
    with concurrent.futures.ThreadPoolExecutor(max_workers=args.maxthread) as ex:
        for logInfo in data.logInfos:
            ex.submit(HandleLog, data, logInfo)
        ex.shutdown(wait=True)
    #out result
    #if not data.justdownload: 
    querylogs.outResult(data)
    
    data.dblog.close()
    
    return data

if __name__ == '__main__':
    logutils.initLog('')
    startTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    args = handleParameters()
    doFilter(args)
    endTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    logging.info('end time: ' + endTime.ctime())
    logging.info('spend time(s): {}'.format((endTime - startTime).total_seconds()))
    