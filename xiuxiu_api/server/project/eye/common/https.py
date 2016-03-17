# -*- coding: utf-8 -*-

import urllib
import urllib2


class Https(object):
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
