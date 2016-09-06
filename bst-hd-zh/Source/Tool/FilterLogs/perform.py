'''
Created on 2015/12/16

@author: Peace

query the logs that execution time is longer
'''
import argparse,logutils,os,logging
import shutil,datetime
from fileutils import FileUtils


def queryDate(file, seconds):
    '''
    query logs that runed time is greater than parameter seconds
    '''
    df = '%Y-%m-%d %H:%M:%S'
    result = False
    text = FileUtils.readAll(file)
    linesep = '\n'
    preIndex = 0
    nextIndex = text.find(linesep,preIndex)
    preLine = ''
    nextLine = ''
    preDate = None
    nextDate = None
    while nextIndex >= 0:
        preLine = nextLine
        nextLine = text[preIndex:nextIndex]
        preIndex = nextIndex+1
        nextIndex = text.find(linesep,preIndex)
        preDate = nextDate
        if len(nextLine) > 19:
            try:
                nextDate = datetime.datetime.strptime(nextLine[0:19],df)
            except:
                break
            if preDate != None and nextDate != None:
                d  = (nextDate - preDate).total_seconds()
                if d >= seconds:
                    result = True
                    logging.info('___find line___'+nextLine)
    return result    
        
def eachFiles(dirPath, seconds):
    for root,dirs,fs in os.walk(dirPath):
        for f in fs:
            if f.endswith('.log'):
                tf = os.path.join(root,f)
                if queryDate(tf, seconds):
                    logging.info(tf)
            elif f.endswith('.zip'):
                folder = f.rstrip('.zip')
                folder = os.path.join(root,folder)
                if os.path.isdir(folder): shutil.rmtree(folder)
                tf = os.path.join(root,f)
                shutil.unpack_archive(tf,folder)
                eachFiles(folder, seconds)
                if os.path.isdir(folder): shutil.rmtree(folder)
                    
def main():
    parser = argparse.ArgumentParser(description='Automtic filter log files.',
        formatter_class=logutils.MyFormat,
        epilog='''\
sample1:  local logs
py perform -log="c:\\logs"
''')
    
    parser.add_argument('-log', help='file or folder',default ='')
    parser.add_argument('-s',help='',default=120,type=int)
    args = parser.parse_args() 
    
    log = args.log
    s = args.s
    if os.path.isfile(log):
        if queryDate(log, s):
            logging.info(log)
    elif os.path.isdir(log):
        eachFiles(log, s)
    else:
        logging.error('parameter log is not a file/folder')
        exit()

if __name__ == '__main__':
    logutils.initLog()
    startTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    main()
    endTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    logging.info('end time: ' + endTime.ctime())
    logging.info('spend time(s): {}'.format((endTime - startTime).total_seconds()))