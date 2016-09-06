'''
Created on 2015/11/23

@author: Peace
'''

import argparse,os
import logutils, dblog, tgpstring
import shutil

def cleanData(path):
    name = os.path.basename(path)
    if name == 'unzip_':
        shutil.rmtree(path, ignore_errors=True)
        return
    for d in os.listdir(path):
        dp = os.path.join(path, d)
        if os.path.isdir(dp):
            if d == 'unzip_':
                shutil.rmtree(dp, ignore_errors=True)
            else:
                cleanData(dp)
        else:
            if d.endswith('.csv'):
                os.remove(dp)    

if __name__ == '__main__':
    import fileutils
    s = fileutils.FileUtils.getExecutePath()
    print(s)
    parser = argparse.ArgumentParser(description='Automtic filter log files.',
        formatter_class=logutils.MyFormat,
        epilog='''\
sample1:  local path
py clean.py -l="c:\\logs"
    ''')
    parser.add_argument('-l', help='full path cleaned',type=str, nargs='+', default=[])
    parser.add_argument('-updateqqid', help='update qqid', default=False, action='store_true')
    parser.add_argument('-namedeploy', help='rename deploy file name', default=False, action='store_true')
    args = parser.parse_args()
    print(args)
    
    if args.updateqqid:
        db = dblog.DbLog()
        count = 0
        for logInfo in db.getLogInfos():
            tgp = logInfo.tgpString
            if tgp != None and len(tgp) > 0:
                qqid = tgpstring.getQqid(tgp)
                if (qqid != None and len(qqid) > 0) and (qqid != logInfo.qqid):
                    logInfo.qqid = qqid
                    db.insertOrUpdateLogInfo(logInfo)
                    print('tgp:{}, qqid:{}'.format(logInfo.tgpString, logInfo.qqid))
                    count += 1
        print('modify {} record(s)'.format(count))
        print('finished')
        db.close()
        exit()
    if args.namedeploy:
        if len(args.l) > 0:
            for folder in args.l:
                if os.path.isdir(folder):
                    for fd in os.listdir(folder):
                        f = os.path.join(folder, fd)
                        if os.path.isfile(f):
                            nf = fd.replace('log_', 'Deploy_')
                            if nf != fd:
                                nf = os.path.join(folder, nf)
                                os.renames(f,nf)
        else:
            print('parameter "-l" must be a folder(s)')
            exit()
        print('finished')
        exit()
        
    if len(args.l) > 0:
        for folder in args.l:
            if os.path.isdir(folder):
                cleanData(folder)
        print('finished')
    else:
        print('parameter "l" is not a path')
        exit()
       
    