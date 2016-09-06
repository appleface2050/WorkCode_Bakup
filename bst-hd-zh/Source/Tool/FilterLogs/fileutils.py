'''
Created on 2015/11/3

@author: Peace
'''
import io,csv,xlrd
import sys,os,logging

class FileUtils(object):
    '''
    classdocs
    '''


    def __init__(self, params):
        '''
        Constructor
        '''
    @staticmethod
    def readAll(file):
        codes = ['utf-8','utf-16','gb2312','gbk','gb18030','big5']
        for c in codes:
            try:
                with open(file,encoding=c) as f:
                    data = f.read()
                    return data
            except:
                pass
       
        try:
            with open(file) as f:
                data = f.read()
                return data
        except:
            pass
        
        return None
    @staticmethod
    def readerRows(file, sheetName = ''):
        rows = []
        filelower = file.lower()
        if filelower.endswith('.csv'):
            data = FileUtils.readAll(file)
            if data != None:
                fd = io.StringIO(data)
                reader = csv.reader(fd)
                rows = [row for row in reader]
        elif filelower.endswith('.xls') or filelower.endswith('.xlsx'):
            wb = xlrd.open_workbook(file)
            sheet = None
            if len(sheetName) > 0:
                sheet = wb.sheet_by_name(sheetName)
            else:
                sheet = wb.sheet_by_index(0)
            rows = [[sheet.cell_value(r, col) 
                for col in range(sheet.ncols)] 
                    for r in range(sheet.nrows)]
#         elif  filelower.endswith('.xlsm'):
#             wb = load_workbook(filename = file)
#             
        else:
            logging.error('not support the file :' + file)
            
        return rows
    
    @staticmethod
    def getShortName(file, level):
        result = None
        if file != None:
            items = os.path.abspath(file).split(os.path.sep)
            if len(file) == 0:
                result = ''
            elif level >= len(items):
                result = file
            else:
                result = os.path.join(*items[len(items) - level:])
        return result
    @staticmethod
    def getExecutePath():
        path = None
        if getattr(sys, 'frozen', False):
            path = os.path.dirname(sys.executable)
        elif __file__:
            path = os.path.dirname(os.path.realpath(__file__))
        return path
    
    