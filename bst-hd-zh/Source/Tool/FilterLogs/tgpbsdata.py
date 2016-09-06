'''
Created on 2015/11/18

@author: Peace
'''

from enum import Enum

class BsDataStatus(Enum):
    none = 0
    notMatched = 1
    matched = 2
    program=3
    hand=4

class TgpBsData(object):
    '''
    
    '''
    fieldToSheet = {'id':'ID','qq':'QQ','date':'Date','issueType':'Issue Type','desc':'Desc','link':'Link','matchedLogs':'Matched Log','status':'Status','fixedTime':'Fixed Time','remark':'Remark'}
    def __init__(self):
        '''
        
        '''
        self.id = None
        self.qq = ''
        self.date = None
        self.issueType = ''
        self.desc = ''
        self.link = ''
        self.matchedLogs = []
        self.status = BsDataStatus.none
        self.fixedTime = None
        self.remark = ''
        
        
        self.rowNumbwr_ = -1
        