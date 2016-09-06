'''
Created on 2015/11/24

@author: Peace
'''
import unittest
import datetime

from dblog import DbLog
from datalogs import LogInfo


class TestDbLog(unittest.TestCase):


    def setUp(self):
        self.db = DbLog()


    def tearDown(self):
        self.db.close()


    def testInsertOrUpdateLogInfo(self):
        logInfo = LogInfo('','')
        logInfo.shortName = '2015-11-18\BootFailureLogs\log_2.zip'
        logInfo.date = datetime.datetime.now()
        logInfo.emaildate = datetime.datetime.strptime('2015-11-18','%Y-%m-%d')
        logInfo.number = 10
        self.db.insertOrUpdateLogInfo(logInfo)
        
        logInfo.number = 20
        self.db.insertOrUpdateLogInfo(logInfo)
    def testGetOldLogInfo(self):
        logInfo = LogInfo('','')
        logInfo.shortName = '2015-11-18\BootFailureLogs\log_3.zip'
        logInfo.date = datetime.datetime.now()
        logInfo.emaildate = datetime.datetime.strptime('2015-11-18','%Y-%m-%d')
        logInfo.number = 10
        self.db.insertOrUpdateLogInfo(logInfo)
        
        old = self.db.getOldLogInfo(logInfo)
        self.assertEqual(logInfo.shortName, old.shortName)
        self.assertEqual(logInfo.number, old.number)
        self.assertEqual(logInfo.emaildate, old.emaildate)
        self.assertEqual(logInfo.date, old.date)
        
        logInfo.shortName='No record'
        old = self.db.insertOrUpdateLogInfo(logInfo)
        self.assertEqual(None, old)

if __name__ == "__main__":
    #import sys;sys.argv = ['', 'Test.testName']
    unittest.main()