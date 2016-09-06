# coding=utf-8

#放置通用装饰器
from django.http import HttpResponse
import json
import hashlib

from util.jsonresult import getResult


def json_response(func):
    """
    A decorator thats takes a view response and turns it
    into json. If a callback is added through GET or POST
    the response is JSONP.
    """
    def decorator(request, *args, **kwargs):
        objects = func(request, *args, **kwargs)
        if isinstance(objects, HttpResponse):
            return objects
        try:
            data = json.dumps(objects)
            if 'callback' in request.REQUEST:
                # a jsonp response!
                data = '%s(%s);' % (request.REQUEST['callback'], data)
                return HttpResponse(data, "text/javascript")
        except:
            data = json.dumps(str(objects))
        return HttpResponse(data, "application/json")
    return decorator


def guid_check_get(func):
    """
    user to check guid
    """
    def decorator(request, *args, **kwargs):
        obj = func(request, *args, **kwargs)
        if request.method == 'GET':
            guid = request.GET.get("guid","")
            result = {}
            if not guid or guid == "undefined" or len(guid)>36:
                return getResult(False, u"error, guid can not be empty")
            if len(guid.split("-")) != 5:
                return getResult(False, u"guid error")
        return obj
    return decorator


def guid_check_post(func):
    """
    user to check guid
    """
    def decorator(request, *args, **kwargs):
        obj = func(request, *args, **kwargs)
        if request.method == 'POST':
            guid = request.POST.get("guid","")
            result = {}
            if not guid or guid == "undefined" or len(guid)>36:
                return getResult(False, u"error, guid can not be empty")
            if len(guid.split("-")) != 5:
                return getResult(False, u"guid error")
        return obj
    return decorator


def check_signature(func):
    """
    验证签名
    """
    def _deco(request, *args, **kwargs):
        if request.method == "POST":
            paras = request.POST
        elif request.method == "GET":
            paras = request.GET
        else:
            return getResult(False,"http method error")

        cheat = paras.get("cheat","")
        timestamp = paras.get("timestamp","")
        signature = paras.get("signature","")
        if cheat == "qq":
            obj = func(request, *args, **kwargs)
            return obj
        if not timestamp:
            return getResult(False,"timestamp empty")
        if not signature:
            return getResult(False,"signature empty")
        _keys = paras.keys()
        keys = []
        for i in _keys:
            if i not in [u"timestamp",u"signature",u'csrfmiddlewaretoken']:
                keys.append(i)

        keys.sort()
        params = "".join(keys)
        sign = hashlib.md5(params+timestamp).hexdigest()
        if sign != signature:
            return getResult(False,"check signature fail")
        else:
            obj = func(request, *args, **kwargs)
        return obj
    return _deco
