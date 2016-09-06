'''
Created on 2015/11/24

@author: Peace
'''
import unittest

import datetime

from datalogs import DataLogs, LogInfo

class TestDataLogs(unittest.TestCase):


    def setUp(self):
        pass

    def tearDown(self):
        pass

    def testDataLogs_AddLogInfo(self):
        data = DataLogs()
        logInfo = LogInfo('', r'2015-11-18\ab\Log_2.zip')
        data.addLogInfo(logInfo)
        
        
        self.assertEqual(logInfo.emaildate, datetime.datetime(2015,11,18))
        self.assertEqual(logInfo.shortName, r'2015-11-18\ab\Log_2.zip')

if __name__ == "__main__":
    #import sys;sys.argv = ['', 'Test.testName']
    unittest.main()