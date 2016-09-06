'''
Created on 2015/11/18

@author: Peace
'''

import argparse,os
from enum import Enum
from fileutils import FileUtils
from logging.handlers import TimedRotatingFileHandler
import logging.config

class MyFormat(argparse.ArgumentDefaultsHelpFormatter, argparse.RawTextHelpFormatter):
    pass

class TimedRotatingFileHandlerMakeFolder(TimedRotatingFileHandler):
    def __init__(self, filename, when='h', interval=1, backupCount=0, encoding=None, delay=False, utc=False, atTime=None):
        path = os.path.dirname(filename)
        if not os.path.isdir(path):
            os.makedirs(path)
        TimedRotatingFileHandler.__init__(self, filename, when, interval, backupCount, encoding, delay, utc, atTime)
class ErrorType(Enum):
    apk = 1  #install apk fail
    deploy = 2 #deploy app player fail
    boot = 3  #boot app fail
    other = 4 #other

def addSharedParameters(parser):
    #parser.add_argument('--h',help='show help message',default =False, action='store_true')
    parser.add_argument('-l','-logpath', help='full path/file of local log file',default='')
    parser.add_argument('-r','-result', help='full file of result file',default='')
    parser.add_argument('-o','-out', help='full file of out result file',default='')
    parser.add_argument('-k','-keywords', help='full file of keywords file',default=os.path.join(FileUtils.getExecutePath(),'keywords.xml'))
    parser.add_argument('-j','-justdownload', help='just download log, do not query it',default =False, action='store_true')
    parser.add_argument('-overwrite', help='true: overwrite exist files downloaded',default =False, action='store_true')
    parser.add_argument('-proxy', help="proxy, sample: 'http':'10.10.21.88:3129'",default='')
    parser.add_argument('-d','-delete', help='delete the log files that had matched in keyword',default =False, action='store_true')
    parser.add_argument('-maxthread', help='Max thread', type=int, default=8)
    parser.add_argument('-maxlog', help='Max handle log files, if it is zero, handle all logs', type=int, default=0)
    parser.add_argument('-minlog', help='Min handle log files, if it is zero, handle all logs', type=int, default=0)
    parser.add_argument('-delunzip', help='delete the unzip file',default =False, action='store_true')
    parser.add_argument('-errorType','-errortype',help='full path/file of local log file',default='')
    parser.add_argument('-refilter', help='redo filter', default=False, action='store_true')
    parser.add_argument('-allkeys',help='count all keys', default=False, action='store_true')

class GlobalData:
    sheets = ['AppInstallFailureLogs', 'BootFailureLogs','BSInstallStats_Failure']
    outLogInfo = '_logInfo'
    deploys= [False, False,True]
    logDateFormat = '%Y-%m-%d %H:%M:%S'
    sheetDateFormat = '%m/%d/%Y %H:%M:%S'
    logsChinaTcDt = '%Y-%m-%d'
    errorTypes = [ErrorType.apk.name,None,ErrorType.deploy.name]
    deployPreUrl = 'http://cloud.bluestacks.com/analytics/GetBlob/'
    @staticmethod
    def getLogInfoOutName(name):
        #re ='{}{}.csv'.format(name[0: len(name)-4],GlobalData.outLogInfo)
        return name


def initLog(configFile=''):
    if len(configFile) < 1:  configFile = os.path.join(FileUtils.getExecutePath(), 'logging.conf')
    logging.config.fileConfig(configFile)
    
def initLogByYaml(yamlFile=''):
    if len(yamlFile) < 1:  yamlFile = os.path.join(FileUtils.getExecutePath(), 'logging.yaml')
    
    logging.config.fileConfig(yamlFile)
if __name__ == '__main__':
    pass