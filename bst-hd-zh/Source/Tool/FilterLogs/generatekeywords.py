'''
Created on 2015/11/11
@author: Peace

generate keywords.xml by Log Filtering Sheet.xls(https://docs.google.com/spreadsheets/d/19mpT2akbZGwdEqLpOKkTnWg5sGzW22Kk6I_TVhPPyuo/edit#gid=0)

'''

import tkinter as tk
import tkinter.messagebox, tkinter.filedialog
import argparse,os,xml.etree.ElementTree,datetime,sys
from fileutils import FileUtils
import logutils, logging
from xml.etree.ElementTree import Element, SubElement,ElementTree,Comment

class MyFormat(argparse.ArgumentDefaultsHelpFormatter, argparse.RawTextHelpFormatter):
    pass

def generateKeyWordsBySheet(words,xlsFile, sheetName, logFileName=''):
    cols = ['String Searched','unify code','Interpretation']
    tags = ['conditions','unifycode','interpretation']
    indexes = []
    rows = FileUtils.readerRows(xlsFile, sheetName)
    header = True
    fileIndex = -1
    words.append(Comment('Start ' + sheetName))
    for row in rows:
        if header:
            indexes = [row.index(col) for col in cols]
            if 'File' in row:
                fileIndex = row.index('File')
            header = False
        else:
            word = SubElement(words, 'word')
            for it in range(1,3):
                item = SubElement(word, tags[it])
                item.text = row[indexes[it]]
            temp = '<{}>{}</{}>'.format(tags[0],row[indexes[0]],tags[0])
            try:
                item = xml.etree.ElementTree.fromstring(temp)
                if fileIndex >= 0:
                    file = row[fileIndex]
                    SubElement(item, 'logfile').text = file
                elif len(logFileName) > 0:
                    SubElement(item, 'logfile').text = logFileName
                word.append(item)
            except:
                logging.exception('Exception({}->{}, row:{})'.format(xlsFile,sheetName,str(row)))
    words.append(Comment('End ' + sheetName))        
def handleParameters():
    parser = argparse.ArgumentParser(description='generate keywords.xml',
        formatter_class=MyFormat,
        epilog='''\
sample1:
  py logstcdt.py -f="c:\\Log Filtering Sheet.xls"
        ''')
    #parser.add_argument('--h',help='show help message',default =False, action='store_true')
    parser.add_argument('-f','--filtersheet',metavar='filepath',help='Log Filtering Sheet file',default='')
    parser.add_argument('-k','--keywords',metavar='filepath',help='full file of keywords file',default=os.path.join(os.getcwd(),'keywords.xml'))

    args = parser.parse_args() 
    if len(sys.argv) < 2:
        temp = DlgArgs.makeParameterByGui()
        if temp == None:
            parser.print_help()
            exit()
            return
        else:
            for k, v in temp.items():
                setattr(args, k, v)
    if not os.path.isfile(args.filtersheet):
        logging.error('parameter filtersheet is not a file')
        exit()
    return args

def indent(elem, level=0, more_sibs=False):
    i = "\n"
    if level:
        i += (level-1) * '  '
    num_kids = len(elem)
    if num_kids:
        if not elem.text or not elem.text.strip():
            elem.text = i + "  "
            if level:
                elem.text += '  '
        count = 0
        for kid in elem:
            indent(kid, level+1, count < num_kids - 1)
            count += 1
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
            if more_sibs:
                elem.tail += '  '
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i
            if more_sibs:
                elem.tail += '  '

class DlgArgs(tk.Frame):
    def __init__(self, master=tk.Frame):
        tk.Frame.__init__(self, master) 
        self.createWidgets()
        self.args = {}


    def createWidgets(self):
        '''
        Create window
        '''
        master = self
        
        #Log Filtering Sheet file
        rowIndex = 0
        tk.Label(master,text="Log Filtering Sheet file").grid(row=rowIndex,column=0, sticky=tk.E)
        self.sheetFile = tk.Entry(master)
        self.sheetFile.grid(row=rowIndex, column=1, sticky=tk.E + tk.W)
        if True:
            def selectLogpath(event):
                file = tkinter.filedialog.askopenfilename()
                if file != None and len(file) > 0:
                    self.sheetFile.delete(0,tk.END)
                    self.sheetFile.insert(0, file)
            self.sheetFile.bind('<Double-Button-1>', selectLogpath)
        
        #out keyword file
        rowIndex +=1
        tk.Label(master,text="out keyword file").grid(row=rowIndex, column=0, sticky=tk.E)
        self.outKeyword = tk.Entry(master)
        self.outKeyword.grid(row=rowIndex, column=1, sticky=tk.E + tk.W)
        if True:
            self.outKeyword.insert(0,os.path.join(FileUtils.getExecutePath(), 'out__/keywords.xml'))
            def selectLogpath(event):
                file = tk.filedialog.asksaveasfilename(filetypes=[('xml','*.xml')])
                if file != None and len(file) > 0:
                    self.outKeyword.delete(0,tk.END)
                    self.outKeyword.insert(0, file)
            self.outKeyword.bind('<Double-Button-1>', selectLogpath)
        
        rowIndex += 1
        if True:
            bt = tk.Button(master,text='Analyze Logs')
            bt.grid(row=rowIndex, column=1)
            bt.bind('<Button-1>', self.runFilterlogs, self)

        master.columnconfigure(1, weight=1)
        master.rowconfigure(rowIndex, weight=1)
        self.grid(padx=10, pady=10,sticky=tk.N + tk.S + tk.E + tk.W)
    
    def runFilterlogs(self, event):
        tempArgs = {}
        if True:
            file = self.sheetFile.get()
            if file != None and (os.path.isfile(file) or os.path.isdir(file)):
                tempArgs['filtersheet'] = file
            else:
                tkinter.messagebox.showwarning('Parameter(s) error', 'Log Filtering Sheet file do not exist')
                self.sheetFile.focus()
                return

        if True:
            outFile = self.outKeyword.get()
            if outFile != None:
                if os.path.exists(outFile):
                    tempArgs['keywords'] = outFile
                else:
                    try:
                        os.makedirs(os.path.dirname(outFile),exist_ok=True)
                        tempArgs['keywords'] = outFile
                    except:
                        tk.messagebox.showwarning('Parameter(s) error', 'out file do not correct')
                        self.outKeyword.focus()
                        return

        self.args = tempArgs
        self.master.destroy()
    @staticmethod
    def makeParameterByGui():
        root = tk.Tk()
        root.title('Generate keyword file')
        root.geometry("900x400")
        root.columnconfigure(0, weight=1)
        root.rowconfigure(0, weight=1)
        app = DlgArgs(master=root)
        def closeWindow():
            root.destroy()
            app.args = None
        root.protocol('WM_DELETE_WINDOW', closeWindow)
        app.mainloop()
        
        return app.args
def main():
    logutils.initLog('')
    args = handleParameters()
    logging.info(args)
    keyword = args.keywords
    
    if True: #validate parameter
        if os.path.isfile(keyword):
            os.remove(keyword)
        dirKey = os.path.dirname(keyword)
        if len(dirKey) > 0 and (not os.path.exists(dirKey)):
            os.makedirs(dirKey)
        
    root = Element('root')
    if True:
        words = SubElement(root, 'words')
        #sheets = ['BlueStacksUsers.log','Boot Failure','ApkHandler','DeploytoolFailureLogs']
        sheets = ['BlueStacksUsers.log','Boot Failure','ApkHandler','DeploytoolFailureLogs']
        files =['BlueStacksUsers','','', '']
        for sh, f in zip(sheets, files):
            if len(sh) > 0: 
                generateKeyWordsBySheet(words,args.filtersheet, sh,f)
    indent(root)
    book = ElementTree()
    book._setroot(root)
    book.write(args.keywords,"utf-8",True)

if __name__ == '__main__':
    startTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    main()
    endTime = datetime.datetime.now()
    logging.info('start time: ' + startTime.ctime())
    logging.info('end time: ' + endTime.ctime())
    logging.info('spend time(s): {}'.format((endTime - startTime).total_seconds()))
    