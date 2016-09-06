# coding=utf-8

import datetime

from django.db import connection, transaction

from util.appcenter import convert_to_percentage


def convert_sec_to_min_sec(sec):
    """
    秒转换为分秒
    """
    sec = int(sec)
    min = sec/60
    sec = sec%60
    return str(min)+"m"+str(sec)+"s"

# def data_merge(*args):
#     print args
#     package_names = []
#     result = []
#     # package_names = [i[0].keys() for i in args[0]]
#
#     for i in args[0]:
#         package_names.append(i.keys()[0])
#     for data in args:
#         type = data[0]["type"]
#         for package_name in package_names:
#             tmp = {
#                 "package_name":package_name,
#                 type:data[package_name]
#             }
#             result.append(tmp)


# def data_merge(**kargs):
#     result = []
#     package_names = []
#     # tmp = {
#     #     ""
#     # }
#
#     package_names = [i.keys()[0] for i in kargs["daily_user_init"]]
#     print package_names
#     for package_name in package_names:
#         result.append({"package_name":package_name,
#                        "daily_user_init":kargs["daily_user_init"][package_name]["daily_user_init"]
#                        })
#
#     print result


def data_merge(**kwargs):
    """
    """
    result = []

    package_names = kwargs["daily_user_init"].keys()
    # print package_names

    for package_name in package_names:
        result.append(
            {"package_name":package_name,
            "daily_user_init":kwargs["daily_user_init"][package_name]["daily_user_init"]
            }
        )
    for i in result:
        try:
            i["daily_user_init_count"] = kwargs["daily_user_init_count"][i["package_name"]]["daily_user_init_count"]
        except Exception, e:
            print e
            i["daily_user_init_count"] = 0
        try:
            i["daily_init_fail"] = kwargs["daily_init_fail"][i["package_name"]]["daily_init_fail"]
        except Exception, e:
            print e
            i["daily_init_fail"] = 0
        try:
            i["daily_init_fail_count"] = kwargs["daily_init_fail_count"][i["package_name"]]["daily_init_fail_count"]
        except Exception, e:
            print e
            i["daily_init_fail_count"] = 0
        try:
            i["daily_install"] = kwargs["daily_install"][i["package_name"]]["daily_install"]
        except Exception, e:
            print e
            i["daily_install"] = 0
        try:
            i["daily_user_session"] = kwargs["daily_user_session"][i["package_name"]]["das"]
        except Exception, e:
            print e
            i["daily_user_session"] = 0
        try:
            i["daily_install_count"] = kwargs["daily_install_count"][i["package_name"]]["daily_install_count"]
        except Exception, e:
            print e
            i["daily_install_count"] = 0
        try:
            i["daily_install_fail"] = kwargs["daily_install_fail"][i["package_name"]]["daily_install_fail"]
        except Exception, e:
            print e
            i["daily_install_fail"] = 0
        try:
            i["daily_install_fail_count"] = kwargs["daily_install_fail_count"][i["package_name"]]["daily_install_fail_count"]
        except Exception, e:
            print e
            i["daily_install_fail_count"] = 0

        #下载数据
        try:
            i["daily_download"] = kwargs["daily_download"][i["package_name"]]["daily_download"]
        except Exception, e:
            print e
            i["daily_download"] = 0
        try:
            i["daily_download_count"] = kwargs["daily_download_count"][i["package_name"]]["daily_download_count"]
        except Exception, e:
            print e
            i["daily_download_count"] = 0

        try:
            i["daily_download_fail"] = kwargs["daily_download_fail"][i["package_name"]]["daily_download_fail"]
        except Exception, e:
            print e
            i["daily_download_fail"] = 0
        try:
            i["daily_download_fail_count"] = kwargs["daily_download_fail_count"][i["package_name"]]["daily_download_fail_count"]
        except Exception, e:
            print e
            i["daily_download_fail_count"] = 0

        try:
            i['init_success_rate'] = convert_to_percentage(float(i['daily_user_init']) / float(i['daily_user_init'] + i['daily_init_fail']))
        except ZeroDivisionError:
            i['init_success_rate'] = 0
        try:
            i['install_success_rate'] = convert_to_percentage(float(i['daily_install']) / float(i['daily_install'] + i['daily_install_fail']))
        except ZeroDivisionError:
            i['install_success_rate'] = 0
        try:
            i['download_success_rate'] = convert_to_percentage(float(i['daily_download']) / float(i['daily_download'] + i['daily_download_fail']))
        except ZeroDivisionError:
            i['download_success_rate'] = 0
        try:
            i['install_fail_rate'] = convert_to_percentage(float(i['daily_install_fail']) / float(i['daily_install'] + i['daily_install_fail']))
        except ZeroDivisionError:
            i['install_fail_rate'] = 0


    return result

def app_total_data_merge(**kwargs):
    """
    合并app total data
    """
    result = []

    #find all version
    version_all = [i["version"] for i in kwargs["app_init_success"]]
    for i in version_all:
        result.append({"version":i})

    # merge
    for i in kwargs:
        for j in kwargs[i]:
            for version in result:
                if j["version"] == version["version"]:
                    dst_count_key = i
                    count_key = i+"_count"
                    version[dst_count_key] = j["dst_count"]
                    version[count_key] = j["count"]

    # print result
    return result


def get_package_name():
    """
    从数据库里获取package name 游戏名称对应关系
    """
    package = {}
    cursor = connection.cursor()
    cursor.execute("select package_name,game_name from view_package_name")
    row = cursor.fetchall()
    for i in row:
        package[i[0]] = i[1]
    return package


def find_app_name(data):
    """
    通过package name获取app name
    """
    package = get_package_name()

    for i in data:
        package_name = i["package_name"]
        try:
            i['name'] = package[package_name]
        except KeyError:
            i['name'] = ""
    return data


def check_package_name(package_names):
    """
    去掉package_name里面包含 ‘ " 的
    """
    result = []
    for i in package_names:
        if i.find("'") == -1 or i.find('"') == -1:
            continue
        else:
            result.append(i)
    return result


def cal_uninstall_reason_meaning(data):
    """
    计算含义
    """
    count = {}
    for i in xrange(1,9):
        count[i] = 0

    for i in data:
        # print i["uninst_reason_dword"]
        dword = bin(i["uninst_reason_dword"])[2:]
        # print dword
        for j in xrange(1,len(dword)+1):
            if dword[-j] == "1":
                # print dword
                count[j] += 1
    return count

def get_last_day(start):
    """
    计算前一天的日期
    """
    return start + datetime.timedelta(days=-1)

def get_last_number_day(start, n):
    """
    计算前某一天的数据
    """
    return start - datetime.timedelta(days=n)

def get_next_number_day(start, n):
    """
    计算未来以后某一天的数据
    """
    return start + datetime.timedelta(days=n)

def get_next_day(start):
    """
    计算前一天的日期
    """
    return start + datetime.timedelta(days=1)

def divide(numerator, denominator):
    """
    除以
    """
    try:
        result = float(numerator) / float(denominator)
    except ZeroDivisionError:
        result = 0
    return result