#!/usr/bin/env python
# -*- coding: utf-8 -*-

import mysqlHelper


def convertToModelList(list,converter):
    u'转换数据源列表为db数据行列表 converter 为转换方法'
    modelList = []
    for i in list:
        item = converter(i)
        if item == None:
            continue
        modelList.append(item)

    return modelList

def sqlStrFormat(s):
    u'sql字符格式化转移，非字符串原文返回'
    #if type(s) != type('') or type(s) != type(u''):
    #    return s
    if s == None:
        return ''
    if isinstance(s, str) or isinstance(s, unicode):
        s = s.replace("'","\\'")
        s = s.replace('"','\\"')
    return s


def getConnectStr():
    u'获取连接字符串'
    s = ''
    for key,val in getConnectDict():
        s+=('%s=%s;' % key,val)
    return s

def getWhereSql(conditionDict):
    u'构建where语句'
    where = '\rwhere\r\t'
    moreCondition = False
    for key,val in conditionDict.items():
        if moreCondition:
            where+='\r\tand '    
        where+=("`%s` = '%s'" % (key,sqlStrFormat(val)))
        moreCondition = True
    return where

def getInsertSql(tableName,model):
    u'获取插入sql语句'
    try:
        s = field = values = ''
    
        for i in  model.keys():
            field += ("`%s`\r\n" % i) + ','
        field = field[0:-1]

        for key,val in  model.items():
            val = sqlStrFormat(val)
            values += ("'%s'\r\n" % val) + ","
        values = values[0:-1]

        sql = u'''
INSERT INTO `%s` 
(%s) 
VALUES 
(%s);
        ''' % (tableName, field,values)
        return sql

    except Exception, e:
        print e.arg[1],e.args[1]
    
def getUpdateSql(tableName,primarykey,model):
    u'获取更新sql语句'
    try:
        set = ''
        for key,val in model.items():
            set+=("\r\n%s = '%s'," % (key,sqlStrFormat(val)))
        set = set[:-1]
        where = getWhereSql({primarykey:model[primarykey]})
        s = u'''
UPDATE `%s`
SET    %s
%s;
        ''' % (tableName,set,where)
        return s
    except Exception, e:
        print e.message
 

def getSelectSql(tableName, fileds, conditionDict):
    u'获取条件查询记录sql语句'
    try:
        where = getWhereSql(conditionDict)
        s = 'SELECT %s \r\tFROM `%s` %s' % (fileds,tableName,where) 
        return s 
    except Exception ,e :
        return '错误：' , e.args[0],e.args[1]
    
def getDeleteSql(tableName,conditionDict):
    u'获取条件删除记录sql语句'
    try:
        where = getWhereSql(conditionDict)
        s = 'DELETE FROM `%s` %s' % (tableName,where) 
        return s 
    except Exception ,e :
        return '错误：' , e.args[0],e.args[1]

def getExistSql(tableName,conditionDict):
    u'获取根据条件生成一个对象是否存在的sql'
    return getSelectSql(tableName,1,conditionDict)


def getDbInstance():
    u'获取数据示例'
    #dbconfig = settings.getDbSetting()
    #db = mysqlHelper.MySQL(dbconfig)
    #return db
    pass