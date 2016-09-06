'''
Created on 2016/2/22/

@author: Peace


GUI tools, category log, it direct call filterlogs‘s function
'''

import tkinter as tk, tkinter.ttk
import os
import fileutils
import filterlogs
from datalogs import LogState


class DlgArgs(tk.Frame):
    def __init__(self, master=tk.Frame):
        tk.Frame.__init__(self, master) 
        self.createWidgets()


    def createWidgets(self):
        '''
        Create window
        '''
        master = self
        
        #Log Path/file
        rowIndex = 0
        tk.Label(master,text="Log Path/File").grid(row=rowIndex,column=0, sticky=tk.E)
        self.sheetFile = tk.Entry(master)
        self.sheetFile.grid(row=rowIndex, column=1, sticky=tk.E + tk.W)
        if True:
            def selectLogpath(event):
                file = tk.filedialog.askopenfilename()
                if file != None and len(file) > 0:
                    self.sheetFile.delete(0,tk.END)
                    self.sheetFile.insert(0, file)
            self.sheetFile.bind('<Double-Button-1>', selectLogpath)
        
        #out file
        rowIndex +=1
        tk.Label(master,text="out file").grid(row=rowIndex, column=0, sticky=tk.E)
        self.outKeyword = tk.Entry(master)
        self.outKeyword.grid(row=rowIndex, column=1, sticky=tk.E + tk.W)
        if True:
            self.outKeyword.insert(0,os.path.join(fileutils.FileUtils.getExecutePath(), 'out__/out.csv'))
            def selectLogpath(event):
                file = tk.filedialog.asksaveasfilename(filetypes=[('All', '*.*')])
                if file != None and len(file) > 0:
                    self.outKeyword.delete(0,tk.END)
                    self.outKeyword.insert(0, file)
            self.outKeyword.bind('<Double-Button-1>', selectLogpath)
        
        #keyworkds file
        rowIndex += 1
        tk.Label(master, text="Keywords").grid(row=rowIndex, column=0, sticky=tk.E)
        self.keywordFile = tk.Entry(master)
        self.keywordFile.grid(row=rowIndex, column=1, sticky=tk.E + tk.W)
        if True:
            self.keywordFile.insert(0,os.path.join(fileutils.FileUtils.getExecutePath(), 'keywords.xml'))
            def selectLogpath(event):
                file = tk.filedialog.askopenfilename(filetypes=[('xml','*.xml'),('All', '*.*')])
                if file != None and len(file) > 0:
                    self.keywordFile.delete(0,tk.END)
                    self.keywordFile.insert(0, file)
            self.keywordFile.bind('<Double-Button-1>', selectLogpath)

        #Search type
        rowIndex += 1
        tk.Label(master, text="Search type").grid(row=rowIndex, column=0, sticky=tk.E)
        if True:
            fr = tk.Frame(master)
            self.searchVar = tk.IntVar()
            oneSearch = tk.Radiobutton(fr, text='search one',variable=self.searchVar, value=0)
            oneSearch.grid(row=0,column=0)
            allSearch = tk.Radiobutton(fr, text='search all',variable=self.searchVar,value=1)
            allSearch.grid(row=0,column=1)
            allSearch.select()
            fr.grid(row=rowIndex, column=1,sticky=tk.E + tk.W)
        
        rowIndex += 1
        if True:
            bt = tk.Button(master,text='Analyze Logs')
            bt.grid(row=rowIndex, column=1)
            bt.bind('<Button-1>', self.runFilterlogs, self)
        
        #out result
        rowIndex += 1
        tk.Label(master, text="Result").grid(row=rowIndex,column=0, sticky=tk.E)
        self.resultTreeView = tkinter.ttk.Treeview(master)
        if True:
            sy = tk.Scrollbar(master, command=self.resultTreeView.yview)
            self.resultTreeView.config(yscrollcommand=sy.set)
            tree = self.resultTreeView
            tree['columns'] = ('one','two')
            tree.column('one', width=200)
            tree.heading('one', text=' ')    
            tree.column('two', width=200)
            tree.heading('two', text=' ') 
            
            
        self.resultTreeView.grid(row=rowIndex, column=1,sticky=tk.N + tk.S + tk.E + tk.W)

        master.columnconfigure(1, weight=1)
        master.rowconfigure(rowIndex, weight=1)
        self.grid(padx=10, pady=10,sticky=tk.N + tk.S + tk.E + tk.W)
    
    def runFilterlogs(self, event):
        args = filterlogs.handleParameters()
        if True:
            log = self.sheetFile.get()
            if log != None and (os.path.isfile(log) or os.path.isdir(log)):
                args.l = log
            else:
                tk.messagebox.showwarning('Parameter(s) error', 'Log Path/File do not exist')
                self.sheetFile.focus()
                return
        
        if True:
            keyFile = self.keywordFile.get()
            if keyFile != None and (os.path.isfile(keyFile)):
                args.k = keyFile
            else:
                tk.messagebox.showwarning('Parameter(s) error', 'Keywords file do not exist')
                self.keywordFile.focus()
                return
        if True:
            outFile = self.outKeyword.get()
            if outFile != None:
                if os.path.exists(outFile):
                    args.o = outFile
                else:
                    try:
                        os.makedirs(os.path.dirname(outFile),exist_ok=True)
                        args.o = outFile
                    except:
                        tk.messagebox.showwarning('Parameter(s) error', 'out file do not correct')
                        self.outKeyword.focus()
                        return
        if True:
            temp = self.searchVar.get()
            if temp == 1:
                args.allkeys = True
            else:
                args.allkeys = False
                
        if True:
            args.refilter = True
            args.delunzip = True
        
        if True:
            
            data = filterlogs.doFilter(args)
            tree = self.resultTreeView
            
            if data.allkeys:
                for logInfo in data.logInfos:
                    ftext = 'Not match'
                    if LogState.found == logInfo.state:
                        ftext = 'Match'
                    tid = tree.insert('','end',text=logInfo.shortName,values=(ftext,' '))
                    for oneKey in logInfo.matchKeys:
                        if oneKey != None:
                            tree.insert(tid,'end',text=oneKey.unifycode, values=(oneKey.interpretation,oneKey.xmlText))
            else:
                for logInfo in data.logInfos:
                    ftext = 'Not match'
                    if LogState.found == logInfo.state:
                        ftext = 'Match'
                    tid = tree.insert('','end',text=logInfo.shortName,values=(ftext,' '))
                    oneKey = logInfo.keyword
                    if oneKey != None:
                        tree.insert(tid,'end',text=oneKey.unifycode, values=(oneKey.interpretation,oneKey.xmlText))
        
def Main():
    root = tk.Tk()
    root.title('Analyze log')
    root.geometry("800x600")
    root.columnconfigure(0, weight=1)
    root.rowconfigure(0, weight=1)
    app = DlgArgs(master=root)
    app.mainloop()

if __name__ == '__main__':
    Main()