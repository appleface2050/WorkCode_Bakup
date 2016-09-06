'''
Created on 2015/11/6

@author: Peace

read the file like LogsChina_tc_dt_2016-03-09, and download logs from google drive, then call the filterlog's function to category log
'''
import shutil
import concurrent.futures
import argparse,os,datetime,filterlogs
import logutils
import logging
from logutils import GlobalData

def handleParameters():
    #parameters
    parser = argparse.ArgumentParser(description='Automtic logs china tc_dt.',
        formatter_class=logutils.MyFormat,
        epilog='''\
sample1:
py logstcdt.py -r="c:\\LogsChina_tc_dt_2015-11-01.xls"
        ''')
    logutils.addSharedParameters(parser)
    args = parser.parse_args() 
    
    return args
    
if __name__ == '__main__':
    logutils.initLog('')
    startTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    args = handleParameters()
    logging.info(args)
    
    sheets = GlobalData.sheets
    deploys= GlobalData.deploys
    errorTypes = GlobalData.errorTypes
    
    def zipfile(path):
        logging.info('zip ' + path)
        shutil.make_archive(path, 'zip', path)
        if os.path.isfile(path + '.zip'):
            logging.info('delete ' + path)
            shutil.rmtree(path, ignore_errors=True)
        logging.info('zip finish' + path)
    
    if len(args.r) > 0:
        rs = []
        
        if os.path.isfile(args.r):
                rs.append(args.r)
        elif os.path.isdir(args.r):
            for root,dirs, fs in os.walk(args.r):
                for f in fs:
                    if f.startswith('LogsChina_tc_dt'):
                        rs.append(os.path.join(root,f))
        else:
            logging.error('parameter "result" must be a folder or file')
            exit()
    
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as ex:

            for r in rs:
                args.r = r
                logbase = os.path.dirname(args.r)
                for sh, de,er in zip(sheets, deploys,errorTypes):
                    args.sheetName = sh
                    args.deploy = de
                    args.l = os.path.join(logbase, sh)
                    args.o = os.path.join(logbase, sh) + '.csv'
                    args.errorType = er
                    filterlogs.doFilter(args)
                ex.submit(zipfile, logbase)
                
            ex.shutdown(wait=True)
                
    else:
        ls = []
        lsmap = {}
        logpath = args.l
        if os.path.isdir(logpath):
            for d1 in os.listdir(logpath):
                b1 = os.path.join(logpath, d1)
                if os.path.isfile(b1): continue
                if d1 in sheets:
                    lsmap[logpath] = ''
                    break
                for d2 in os.listdir(b1):
                    b2 = os.path.join(b1, d2)
                    if os.path.isfile(b2): continue
                    if d2 in sheets:
                        lsmap[b1] = ''
                        break
                    for d3 in os.listdir(b2):
                        b3 = os.path.join(b2, d3)
                        if os.path.isfile(b3): continue
                        if d1 in sheets:
                            lsmap[b2] = ''
                            break
                        for d4 in os.listdir(b3):
                            b4 = os.path.join(logpath, d4)
                            if os.path.isfile(b4): continue
                            if d1 in sheets:
                                lsmap[b3] = ''
                                break
            ls = [l for l in lsmap.keys()]
        else:
            logging.error('parameter "logpath" must be a folder')
            exit()
    
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as ex:
            for l in ls:
                logbase = l
                for sh, de,er in zip(sheets, deploys,errorTypes):
                    args.sheetName = sh
                    args.deploy = de
                    args.l = os.path.join(logbase, sh)
                    args.o = os.path.join(logbase, sh) + '.csv'
                    args.errorType = er
                    if os.path.isdir(args.l) : filterlogs.doFilter(args)
                ex.submit(zipfile, logbase)
                
            ex.shutdown(wait=True)
    endTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    logging.info('end time: ' + endTime.ctime())
    logging.info('spend time(s): {}'.format((endTime - startTime).total_seconds()))
    
    