# -*- coding: utf-8 -*-
import json
import time
from django.http import HttpResponse, HttpResponseRedirect


def json_response(func):
    def _json_response(request, **kwargs):
        if kwargs:
            data = func(request, kwargs)
        else:
            data = func(request)
        return HttpResponse(json.dumps(data), content_type="application/json")

    return _json_response


# 进入页面时需要登录
def authenticated(func):
    def _authenticated(request, **kwargs):
        if request.user.is_authenticated():
            pass
        else:
            return HttpResponseRedirect("/support/login/?next=" + request.path)
        if kwargs:
            data = func(request, kwargs)
        else:
            data = func(request)
        return data

    return _authenticated


# 执行时间
def execute_time(func):
    def _execute_time(*args):
        start = time.time()
        result = func(*args)
        end = time.time()
        print "[%s] execute time: %.5fs" % (func.__name__, (end - start))
        return result

    return _execute_time
