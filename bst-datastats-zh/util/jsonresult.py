#coding=utf-8

import collections
import logging
import json
from django import db

from django.core.serializers import deserialize, serialize
from django.db.models.query import QuerySet
from django.db import models
from django.http import HttpResponse


def getResult(success, message, result=None):
    """
    返回数据
    """
    map = {'success': success, 'message': message}
    if result:
        map['result'] = result

    jsonstr = json.dumps(map)
    # if cachename:
    #     cache.set(str(cachename), jsonstr, 3600 * 24)
    return HttpResponse(jsonstr, u'application/json')

def getSimpleResult(success, message, result=None):
    """
    为了和jsonp返回格式保持一致，临时的返回数据
    """
    jsonstr = json.dumps(result)
    return HttpResponse(jsonstr, u'application/json')

def getJsonpResult(success, message, result=None):
    """
    返回dict数据, 用于jsonp
    """
    map = {'success': success, 'message': message}
    if result:
        map['result'] = result

    # jsonstr = json.dumps(map)
    # # if cachename:
    # #     cache.set(str(cachename), jsonstr, 3600 * 24)
    return map

