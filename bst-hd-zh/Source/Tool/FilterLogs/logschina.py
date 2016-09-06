'''
Created on 2015-12-14

@author: Peace

write or update the infomation of LogsChina_tc_dt into db

'''

import argparse,logutils,os,datetime
import logging,re
from dblog import DbLog
from fileutils import FileUtils
from datalogs import LogsChina
from logutils import GlobalData


def rowToLogsChina(row, headIndexes):
    logsChina = LogsChina()
    for key, value in headIndexes.items():
        logsChina.__dict__[key] = row[value]
    if logsChina.errorCode != 0:
        logsChina.errorCode = int(logsChina.errorCode)
    if logsChina.date != None:
        if not isinstance(logsChina.date, datetime.datetime):
            logsChina.date = datetime.datetime.strptime(logsChina.date, GlobalData.logDateFormat)
    return logsChina

def main():
    parser = argparse.ArgumentParser(description='full logschina table.',
        formatter_class=logutils.MyFormat,
        epilog='''\
sample1:  local logs
py logschina.py -r="c:\\logs"
        ''')
    parser.add_argument('-r','-result', help='full file or folder',default='')
    args = parser.parse_args() 
    r = args.r
    if (not os.path.isfile(r)) and (not os.path.isdir(r)):
        logging.error('Parameter r is not a file or folder')
        exit()
    
    files = []
    if True:
        reg = re.compile('LogsChina_tc_dt_(\\d{4}-\\d{1,2}-\\d{1,2})')
        if os.path.isfile(r) and reg.search(os.path.basename(r)) != None:
            files.append(r)
        elif os.path.isdir(r):
            for it1 in os.listdir(r):
                f = os.path.join(r,it1)
                if os.path.isfile(f):
                    if reg.search(os.path.basename(it1)) != None:
                        files.append(f)
                elif os.path.isdir(f):
                    for it2 in os.listdir(f):
                        f2 = os.path.join(f, it2)
                        if os.path.isfile(f2):
                            if reg.search(os.path.basename(it2)) != None:
                                files.append(f2)
                        elif os.path.isdir(f2):
                            for it3 in os.listdir(f2):
                                f3 = os.path.join(f2, it3)
                                if os.path.isfile(f3):
                                    if reg.search(os.path.basename(it3)) != None:
                                        files.append(f3)
                                else:
                                    logging.info('do not search folder :' + f3)
                                    pass # done nothing
    if True:
        maxCount = 1000
        dbLog = DbLog(notAutoCommit=True)
        sheets = logutils.GlobalData.sheets
        for f in files:
            print(f)
            heads = []
            types = [LogsChina.TypeApk, LogsChina.TypeBoot ]
            if True:
                head = {'url':'Log_File_Path','package':'Package_Name','date':'Timestamp','version':'Version','oem':'Oem',
                     'errorReason':'Error_Reason', 'guid':'Guid','errorCode':'Error_Code'}
                heads.append(head)
                head = {'url':'Log_File_Path','date':'Timestamp','version':'Version','oem':'Oem',
                     'errorReason':'Error_Reason', 'guid':'Guid','errorCode':'Error_Code'}
                heads.append(head)
            rowCount = 0
            for i in [0,1]:
                firstRow = True
                headIndexes = {}
                thead = heads[i]
                for row in FileUtils.readerRows(f, sheets[i]):
                    if firstRow:
                        firstRow = False
                        for key, value in thead.items():
                            if value in row:
                                headIndexes[key] = row.index(value)
                        continue
                    else:
                        logsChina = rowToLogsChina(row, headIndexes)
                        LogsChina.type = types[i]
                        if logsChina.url != None: 
                            dbLog.insertOrUpdateLogsChina(logsChina)
                            rowCount += 1
                            if rowCount >= maxCount:
                                rowCount = 0
                                dbLog.commit()
                print(types[i])

            thead = {'url':'install_log','date':'timestamp','version':'prod_ver','oem':'oem',
                     'errorReason':'failure_reason', 'guid':'guid','errorCode':'Error_Code','installType':'install_type','country':'country'}
            firstRow = True
            headIndexes = {}
            rowCount = 0
            for row in FileUtils.readerRows(f, sheets[2]):
                if firstRow:
                    firstRow = False
                    for key, value in thead.items():
                        if value in row:
                            headIndexes[key] = row.index(value)
                    continue
                else:
                    logsChina = rowToLogsChina(row, headIndexes)
                    LogsChina.type = LogsChina.TypeDeploy
                    if logsChina.url != None: 
                        logsChina.url = GlobalData.deployPreUrl + logsChina.url
                        dbLog.insertOrUpdateLogsChina(logsChina)
                        rowCount += 1
                        if rowCount >= maxCount:
                            rowCount = 0
                            dbLog.commit()
            print(LogsChina.TypeDeploy)
            dbLog.commit()
if __name__ == '__main__':
    logutils.initLog('')
    startTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    main()
    endTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    logging.info('end time: ' + endTime.ctime())
    logging.info('spend time(s): {}'.format((endTime - startTime).total_seconds()))
    