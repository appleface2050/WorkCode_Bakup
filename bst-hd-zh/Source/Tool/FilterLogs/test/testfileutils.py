'''
Created on 2015/11/24

@author: Peace
'''
import unittest

from fileutils import FileUtils

class TestFileUtils(unittest.TestCase):


    def setUp(self):
        pass

    def tearDown(self):
        pass

    def testGetShortName(self):
        file = r'c:\peace\temp\abc\txt.t'
        name = FileUtils.getShortName(file, 1)
        self.assertEqual('txt.t', name)
        
        name = FileUtils.getShortName(file, 2)
        self.assertEqual(r'abc\txt.t', name)
        
        name = FileUtils.getShortName(file, 5)
        self.assertEqual(file, name)
        
        name = FileUtils.getShortName(file, 6)
        self.assertEqual(file, name)
        
        file = 'txt.t'
        name = FileUtils.getShortName(file, 1)
        self.assertEqual('txt.t', name)
        
        file = ''
        name = FileUtils.getShortName(file, 1)
        self.assertEqual('', name)
        
        file = None
        name = FileUtils.getShortName(file, 1)
        self.assertEqual(None, name)
        
        file = '.'
        name = FileUtils.getShortName(file, 1)
        self.assertNotEqual(None, name)
        
        file = 'tkaab'
        name = FileUtils.getShortName(file, 1)
        self.assertEqual(file, name)
        
        file = r'sdfsdjfl\tkaab'
        name = FileUtils.getShortName(file, 1)
        self.assertEqual('tkaab', name)

if __name__ == "__main__":
    #import sys;sys.argv = ['', 'Test.testName']
    unittest.main()