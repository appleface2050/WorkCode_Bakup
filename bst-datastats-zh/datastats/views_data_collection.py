# coding=utf-8
import datetime


from django.http import HttpResponse
from django.shortcuts import render
from django.views.decorators.csrf import csrf_exempt

# from models import EngineInstall,EngineActivity
from datastats.models import EngineInstall, EmulatorInstall, EngineActivity, EmulatorActivity, AppInstall, AppActivity, \
    GeneralData


@csrf_exempt
def general_json(request):
    """
    普通json数据
    """
    if request.method == "POST":
        type = request.POST.get("type","")
        if type == "json":
            json = request.POST.get("json","")
            GeneralData.insert_a_data("json", json)
            return HttpResponse("success")
        else:
            return HttpResponse("type should not empty")
    else:
        return HttpResponse("use post method")


@csrf_exempt
def engine_install(request):
    """
    op:
    install
    uninstall
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        op = request.POST.get("op","")
        status = request.POST.get("status","success")
        version = request.POST.get("version","")
        channel = request.POST.get("channel","bscn")
        EngineInstall.add_data(guid,op,status,version,channel)
        return HttpResponse("success")

    # if request.method == "GET":
    #     guid = request.GET.get("guid","")
    #     op = request.GET.get("op","")
    #     status = request.GET.get("status","success")
    #     version = request.GET.get("version","")
    #     EngineInstall.add_data(guid,op,status,version)
    #     return HttpResponse("success")


@csrf_exempt
def engine_activity(request):
    """
    op:
    init
    abort
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        op = request.POST.get("op","")
        status = request.POST.get("status","success")
        version = request.POST.get("version","")
        channel = request.POST.get("channel","bscn")
        EngineActivity.add_data(guid,op,status,version,channel)
        # EngineActivity.add_data(guid,op,status)
        return HttpResponse("success")

@csrf_exempt
def emulator_install(request):
    """
    op:
    install
    uninstall
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        op = request.POST.get("op","")
        status = request.POST.get("status","success")
        version = request.POST.get("version","")
        channel = request.POST.get("channel","bscn")
        EmulatorInstall.add_data(guid,op,status,version,channel)
        return HttpResponse("success")

@csrf_exempt
def emulator_activity(request):
    """
    op:
    init
    abort
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        op = request.POST.get("op","")
        status = request.POST.get("status","success")
        version = request.POST.get("version","")
        channel = request.POST.get("channel","bscn")
        EmulatorActivity.add_data(guid,op,status,version,channel)
        return HttpResponse("success")

@csrf_exempt
def app_install(request):
    """
    op:
    install
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        package_name = request.POST.get("package","")
        op = request.POST.get("op","")
        status = request.POST.get("status","success")
        version = request.POST.get("version","")
        channel = request.POST.get("channel","bscn")
        AppInstall.add_data(guid,op,status,version,package_name,channel)
        return HttpResponse("success")

@csrf_exempt
def app_activity(request):
    """
    op:
    init
    abort
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        package_name = request.POST.get("package","")
        op = request.POST.get("op","")
        status = request.POST.get("status","success")
        version = request.POST.get("version","")
        channel = request.POST.get("channel","bscn")
        AppActivity.add_data(guid,op,status,version,package_name,channel)
        return HttpResponse("success")


def migrate(request):
    """
    将数据倒到测试环境
    """
    today = datetime.date.today()
    engine_install = EngineInstall.objects.using("production").filter(datetime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in engine_install:
        a = EngineInstall()
        a.guid = i.guid
        a.op = i.op
        a.version = i.version
        a.status = i.status
        a.channel = i.channel
        a.datetime = i.datetime
        try:
            a.save()
        except Exception, e:
            print e
            print a

    engine_activity = EngineActivity.objects.using("production").filter(datetime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in engine_activity:
        a = EngineActivity()
        a.guid = i.guid
        a.op = i.op
        a.version = i.version
        a.status = i.status
        a.channel = i.channel
        a.datetime = i.datetime
        try:
            a.save()
        except Exception, e:
            print e
            print a

    data = EmulatorInstall.objects.using("production").filter(datetime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in data:
        a = EmulatorInstall()
        a.guid = i.guid
        a.op = i.op
        a.version = i.version
        a.status = i.status
        a.channel = i.channel
        a.datetime = i.datetime
        try:
            a.save()
        except Exception, e:
            print e
            print a

    data = EmulatorActivity.objects.using("production").filter(datetime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in data:
        a = EmulatorActivity()
        a.guid = i.guid
        a.op = i.op
        a.version = i.version
        a.status = i.status
        a.channel = i.channel
        a.datetime = i.datetime
        try:
            a.save()
        except Exception, e:
            print e
            print a

    data = AppInstall.objects.using("production").filter(datetime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in data:
        a = AppInstall()
        a.guid = i.guid
        a.op = i.op
        a.version = i.version
        a.status = i.status
        a.channel = i.channel
        a.datetime = i.datetime
        a.package_name = i.package_name
        try:
            a.save()
        except Exception, e:
            print e
            print a

    data = AppActivity.objects.using("production").filter(datetime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in data:
        a = AppActivity()
        a.guid = i.guid
        a.op = i.op
        a.version = i.version
        a.status = i.status
        a.channel = i.channel
        a.datetime = i.datetime
        a.package_name = i.package_name
        try:
            a.save()
        except Exception, e:
            print e
            print a

    data = GeneralData.objects.using("production").filter(uptime__gt=today.strftime("%Y-%m-%d"))[:200]
    for i in data:
        a = GeneralData()
        a.type = i.type
        a.json = i.json
        a.uptime = i.uptime
        try:
            a.save()
        except Exception, e:
            print e
            print a

    return HttpResponse("done")
