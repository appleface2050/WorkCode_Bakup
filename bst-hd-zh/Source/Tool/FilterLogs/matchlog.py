'''
Created on 2015/11/19

@author: Peace

'''
import socket,socks
# from idlelib.IOBinding import encoding

import argparse,datetime
import logutils
import logging
from datalogs import LogState
from logutils import GlobalData
from tgpbsdata import TgpBsData

import json
import gspread
from oauth2client.client import SignedJwtAssertionCredentials

from dblog import DbLog

def handleParameters():
    #parameters
    parser = argparse.ArgumentParser(description='Match logs china tc_dt.',
        formatter_class=logutils.MyFormat,
        epilog='''\
sample1:
py logstcdt.py -r="c:\\LogsChina_tc_dt_2015-11-01.xls"
        ''')
    logutils.addSharedParameters(parser)
    args = parser.parse_args() 
    
    return args

def OpenSheet(name=''):
    sh = None
    if True:
        json_key = json.load(open('TGPIssues-1a755a02b36d.json'))
        scope = ['https://spreadsheets.google.com/feeds']
        credentials = SignedJwtAssertionCredentials(json_key['client_email'], json_key['private_key'], scope)
        gc = gspread.authorize(credentials)
        wks = gc.open_by_key('1I11IAh7ykWwNlUybdksr_OOHkWhNF_nrafW1bOYaKYI')
        if len(name) < 1:
            sh = wks.get_worksheet(0)
        else:
            sh = wks.worksheet(name)
    return sh

def makeFieldToIndexes(sh):
    fieldToIndex = {}
    if sh != None:
        header = sh.row_values(1)
        for key, value in TgpBsData.fieldToSheet.items():
            if value in header:
                fieldToIndex[key] = header.index(value)
    return fieldToIndex
def tgpBsItems(sh,fieldToIndexes, dbLog):

    mIndex = fieldToIndexes.get('matchedLogs')
    if mIndex == None: mIndex = -1
    matchCount = 0
    rowNumber = 0
    step = 300
    
    try:
        rowCount = sh.row_count
        while(rowNumber < rowCount):
            if(rowNumber != 0):
                sh = OpenSheet(sh.title)
            rows = sh.get_all_values()
            rowCount = len(rows)
            maxCount = (rowNumber + step > rowCount) and rowCount or (rowNumber + step)
            rows = rows[rowNumber: maxCount + 1]
            for row in rows:
                rowNumber +=1
                if rowNumber == 1: continue
                
                matchStr = ''
                tgp = TgpBsData()
                if row == None or len(row) < 1 : continue
                for key, value in fieldToIndexes.items():
                    if value >= len(row): continue
                    if key == 'date':
                        t = row[value]
                        if len(t) > 0:
                            tgp.date = datetime.datetime.strptime(t, GlobalData.sheetDateFormat)
                    else:
                        tgp.__dict__[key] = row[value]
                    tgp.rowNumbwr_ = rowNumber
                    
                
                if tgp.date != None and ((datetime.datetime.now() - tgp.date).days > 15):
                    continue
                
        
                for m in dbLog.matchTgbLog(tgp.qq, tgp.date):
                    name = m.shortName
                    if m.state == LogState.found and m.keyword != None:
                        name += ' : ' + m.keyword.interpretation
                    matchStr += '\n<log>{}</log>'.format(name)
        
                if len(matchStr) > 0 and mIndex >=0 :
                    sh.update_cell(rowNumber, mIndex+1, matchStr)
                    matchCount += 1
                    logging.info(matchStr)
    except:
        logging.exception('rowNumber({})'.format(rowNumber))
    logging.info('total: {}, matched:{}'.format(len(rows) -1, matchCount))
if __name__ == '__main__':
    
    logutils.initLog('')
    startTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    args = handleParameters()
    if True:
        if len(args.proxy) > 0:
            try:
                pro = None
                ps = eval('{{{}}}'.format(args.proxy.upper()))
                if ps != None:
                    pro = ps.get('HTTP')
                    if pro == None:
                        pro = ps.get('ALL')
                if pro != None:
                    sps = pro.split(':')
                    if len(sps) == 2:
#                         socks.setdefaultproxy(socks.PROXY_TYPE_SOCKS5, "10.10.21.88", 3129)
#                         socks.setdefaultproxy(socks.PROXY_TYPE_SOCKS4, "10.10.21.88", 3129)
                        socks.setdefaultproxy(socks.PROXY_TYPE_HTTP, sps[0], int(sps[1]))
                        socket.socket = socks.socksocket
                        args.proxy = ''
                
            except:
                logging.exception('parameter proxy is invalide')
                exit()
    
    logging.info(args)
    
    dbLog = DbLog()

    logging.info('sheet: IssuesTracking')
    sh = OpenSheet('IssuesTracking')
    fieldToIndexes = makeFieldToIndexes(sh)
    tgpBsItems(sh,fieldToIndexes,dbLog)

    logging.info('sheet: Issues-in-Contact')
    sh = OpenSheet('Issues-in-Contact')
    fieldToIndexes = makeFieldToIndexes(sh)
    tgpBsItems(sh,fieldToIndexes,dbLog)
        
    endTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    logging.info('end time: ' + endTime.ctime())
    logging.info('spend time(s): {}'.format((endTime - startTime).total_seconds()))
