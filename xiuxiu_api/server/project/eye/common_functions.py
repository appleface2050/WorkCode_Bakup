# -*- coding:utf-8 -*-
from xml.etree import ElementTree
import urllib
import urllib2
import datetime


class XmlOperations(object):
    def __init__(self, file_path):
        self.file_path = file_path

    def get_text(self):
        try:
            self.text = open(self.file_path).read()
        except Exception as ex:
            print ex.message
            self.text = ""

    def get_root(self):
        try:
            return ElementTree.fromstring(self.text)
        except Exception as ex:
            print ex.message
            return []

    @classmethod
    def get_all_sub_nodes(cls, node, node_name):
        try:
            return node.getiterator(node_name)
        except Exception as ex:
            return []

    @classmethod
    def get_all_path_nodes(cls, node, node_path):
        try:
            return node.findall(node_path)
        except Exception as ex:
            return []

    @classmethod
    def get_node_children(cls, node):
        try:
            return node.getchildren()
        except Exception as ex:
            return []

    @classmethod
    def get_the_first_node(cls, node, node_name):
        try:
            return node.find(node_name)
        except Exception as ex:
            return {}

    @classmethod
    def get_node_attributes(cls, node):
        try:
            return node.attrib
        except Exception as ex:
            return {}

    @classmethod
    def get_node_attribute_value(cls, node, attribute_key):
        try:
            return node.attrib[attribute_key]
        except Exception as ex:
            return None


class HttpOperations(object):
    @staticmethod
    def http_post_action(url_params, data):
        post_data = urllib.urlencode(data)

        # 提交，发送数据
        req = urllib2.Request(url_params, post_data)
        ret = urllib2.urlopen(req)

        # 获取提交后返回的信息
        content = ret.read()
        return content

    @staticmethod
    def http_get_action(url_params):
        ret = urllib2.urlopen(url_params)
        content = ret.read()
        # print content
        return content


class DatetimeOperations(object):
    @staticmethod
    def get_today():
        return datetime.datetime.now()

    @staticmethod
    def get_date(someday):
        return someday.date()

    @staticmethod
    def get_delta_date(start_date, end_date):
        return (end_date - start_date).days

    @staticmethod
    # days, seconds, microseconds
    def get_delta_time(start_datetime, end_datetime):
        return end_datetime - start_datetime
