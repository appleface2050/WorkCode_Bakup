'''
Created on 2015/11/2

@author: Peace
'''

import os,logging
import datetime
from logutils import GlobalData
from idlelib.IOBinding import encoding

from fileutils import FileUtils

import subprocess

def getTgpString(log):
    '''
    
    parameter log: 
    '''
    date=None
    pt = False
    try:
        tgpString = ''
        if os.path.isfile(log):
            ks = '<TGP String>'
            with open(log, encoding='utf-8') as dl:
                data = dl.read()
                index = data.rfind('\n', 0, len(data)-2)
                if index > 0:
                    dstr = data[index+1:]
                    dstr = dstr[0: 19]
                    date = datetime.datetime.strptime(dstr, GlobalData.logDateFormat)
        elif os.path.isdir(log):
            tgpPath = os.path.join(log, r'Logs\TGP.txt')
            if os.path.isfile(tgpPath):
                with open(tgpPath) as tg:
                    tgpString = tg.read()
            else:
                if pt : print('error:  file {} do not exit'.format(tgpPath))
            dateLog = os.path.join(log, r'Logs\BlueStacksUsers.log')
            if os.path.isfile(dateLog):
                with open(dateLog, encoding='utf-8') as f:
                    data = f.read()
                    index = data.rfind('\n', 0, len(data)-2)
                    if index > 0:
                        dstr = data[index+1:]
                        dstr = dstr[0: 19]
                        date = datetime.datetime.strptime(dstr, GlobalData.logDateFormat)
        else:
            if pt: print('error:  file {} do exit'.format(log))
    except:
        logging.exception('Exception({})'.format(log))
    return tgpString,date
def getQqid(tgps):
    tool =  os.path.join(FileUtils.getExecutePath(),r'bin\decypt_uid_str.exe')
    cmd = '"{}" -s {}'.format(tool, tgps)
    qqid = subprocess.check_output(cmd,shell=False)
    if qqid != None:
        qqid = qqid.decode('utf-8')
        qqid = qqid.rstrip('\r\n')
        qqid = qqid.rstrip('\n')
    return qqid
def getQqidsByData(data):
    tool =  os.path.join(FileUtils.getExecutePath(),r'bin\decypt_uid_str.exe')
    tgpFile = os.path.join(FileUtils.getExecutePath(),r'bin\uins.txt')
    qqFile = os.path.join(FileUtils.getExecutePath(),r'bin\uins_decoded.txt')
    #make tgpFile 
    if os.path.isfile(tgpFile) : os.remove(tgpFile)
    with open(tgpFile, 'w') as tgp:
        for info in data.logInfos:
            tgp.write(info.tgpString + '\n')
    if os.path.isfile(qqFile) : os.remove(qqFile)
    os.system('{} -f {}'.format(tool, tgpFile))
    if os.path.isfile(qqFile):
        with open(qqFile) as qq:
            for row, info in zip(qq.readlines(), data.logInfos):
                if row.endswith('\n'): row = row[0: len(row) -1]
                info.qqid = row