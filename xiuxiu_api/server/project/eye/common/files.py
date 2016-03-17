# -*- coding: utf-8 -*-

import os
import csv
import cStringIO
import urllib2
from django.conf import settings
from logs import Logs
from characters import Characters
from datetimes import Datetimes

BASE_URL = settings.BASE_SERVER_URL
CDN_URL = settings.CDN_URL
MEDIA_URL = settings.MEDIA_URL
MEDIA_URL_NAME = settings.MEDIA_URL_NAME

BASE_URL_4_IMAGE = CDN_URL + MEDIA_URL
BASE_URL_4_SHOP = BASE_URL_4_IMAGE + "shop/default/"
BASE_POST_LIST_URL = BASE_URL + "/forum/post/list/"
BASE_REPLY_LIST_URL = BASE_URL + "/forum/post/detail/"


class Files(object):
    @staticmethod
    def getcwd():
        return os.getcwd()

    @staticmethod
    def listdir(path):
        return os.listdir(path)

    @staticmethod
    def isfile(path):
        return os.path.isfile(path)

    @staticmethod
    def isdir(path):
        return os.path.isdir(path)

    @staticmethod
    def exists(path):
        return os.path.exists(path)

    @staticmethod
    def rename(old_name, new_name):
        return os.name(old_name, new_name)

    @staticmethod
    def getsize(f):
        return os.path.getsize(f)

    @staticmethod
    def makedirs(path):
        return os.makedirs(path)

    @staticmethod
    def upload_file(the_file, file_path):
        """
        :param the_file: 文件
        :param file_path: 文件路径（包括文件路径和文件名称）
        :return:是否添加文件成功
        """
        try:
            destination = open(file_path, 'wb+')
            for chunk in the_file.chunks():
                destination.write(chunk)
            destination.close()
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def save_csv(data, title=None, path="csv_test.csv"):
        if not title:
            title = ["name", "age", "telephone"]
        csv_file = file(path, "wb")
        writer = csv.writer(csv_file)

        utf_title = Characters.unicode_to_concrete(title)
        writer.writerow(utf_title)
        utf_data = Characters.unicode_to_concrete(data)
        writer.writerow(utf_data)

        csv_file.close()

    @staticmethod
    def create_directory_path(path):
        if not Files.exists(path):
            try:
                Files.makedirs(path)
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)
                return None
        return path

    @staticmethod
    def create_file_path(path, name):
        result = Files.create_directory_path(path)
        if result:
            result += name
        else:
            result = None
        return result

    @staticmethod
    def get_memory_file(url):
        try:
            # Logs.print_log("get memory file url", url)
            return cStringIO.StringIO(urllib2.urlopen(url, data=None, timeout=10).read())
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_today_folders():
        now = Datetimes.get_now()
        folder = str(now.year) + "/" + str(now.month) + "/" + str(now.day) + "/"
        return folder

    @staticmethod
    def get_file_name(url):
        try:
            names = url.split("/")
            return names[-1]
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_file_content(path):
        try:
            f = open(path, "r")
            return f.read()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None
