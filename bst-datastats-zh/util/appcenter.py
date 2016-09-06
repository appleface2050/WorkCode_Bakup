# coding=utf-8

import datetime

def screenshots_modify(screenshots):
    """
    规整截屏
    """
    return screenshots.split(",")


def app_size_modify(size):
    """
    app大小规整
    """
    type = {
        0:"B",
        1:"KB",
        2:"MB",
        3:"GB",
        4:"TB",
    }
    count = 0
    while size > 1024:
        size /= 1024.
        count += 1
    try:
        res = "%.01f"%size + type[count]
    except Exception as e:
        res = "0MB"
    return res

def cal_day(start, end, days):
    """
    计算留存天
    """
    days -= 1
    start = start - datetime.timedelta(days=days)
    end = end - datetime.timedelta(days=days)
    return start, end

def cal_day2(start, end, days):
    """
    计算留存天
    """
    # days -= 1
    start = datetime.datetime.strptime(start,'%Y-%m-%d')
    end = datetime.datetime.strptime(end,'%Y-%m-%d')
    start = start - datetime.timedelta(days=days)
    end = end - datetime.timedelta(days=days)
    return start, end

def convert_to_percentage(f):
    """
    float返回为保留小数点两位的百分数的字符串
    """
    return "%.2f%%" % (f*100)