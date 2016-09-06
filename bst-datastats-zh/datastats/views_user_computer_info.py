# coding=utf-8
import datetime

from django.shortcuts import render
from django.db import connection, transaction
from django.db.models import Sum

from django.core.paginator import Paginator, EmptyPage, PageNotAnInteger

# Create your views here.
from bst_server.settings import MAX_DAYS_QUERY
from datastats.models import ResultEngineInstall, ResultEngineActivity, ResultEmulatorInstall, ResultEmulatorActivity, \
    ResultRetention, ResultEmulatorSession, MidResultInstallInitEmulator, ResultEmulatorInstallCount, \
    ResultEmulatorUninstallCount, ResultEmulatorUninstallNextDayCount, MidResultInstallInitEngine, ResultRetentionEngine, \
    ResultEngineDAU, ResultGeneralOsVersion, ResultUserComputerInfoOS, ResultUserComputerInfoMemory, \
    ResultUserComputerInfoCPU, OSVersion
from util.appcenter import convert_to_percentage
from util.data_lib import convert_sec_to_min_sec


def user_computer_info_os(request):
    start = request.GET.get("start","")
    package_name = request.GET.get("package_name","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    else:
        start = (datetime.timedelta(days=-1) + today)
    _start = start.strftime('%Y-%m-%d')

    """os_info 操作系统信息"""
    result = []
    os_info = ResultUserComputerInfoOS.objects.filter(result_date=start.strftime("%Y-%m-%d")).order_by("-dst_count")
    # memory_info = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d")).order_by("-dst_count")
    # cpu_info = ResultUserComputerInfoCPU.objects.filter(result_date=start.strftime("%Y-%m-%d")).order_by("-dst_count")
    for i in os_info:
        os_version = ""
        if OSVersion.objects.filter(os=i.os).exists():
            os_version = OSVersion.objects.get(os=i.os).info
        result.append(
            {
             "os":i.os,
             "result_date":i.result_date,
             "dst_count":i.dst_count,
             "os_version":os_version
            }
        )
    res_number = 30
    page = int(request.GET.get('page',"1"))
    paginator = Paginator(result, res_number)
    try:
        result = paginator.page(page)
    except PageNotAnInteger:
        # If page is not an integer, deliver first page.
        result = paginator.page(1)
    except EmptyPage:
        # If page is out of range (e.g. 9999), deliver last page of results.
        result = paginator.page(paginator.num_pages)

    return render(request, "user_computer_info_os.html", {"start":_start,"os_info":result, 'page_range':paginator.page_range})


def user_computer_info_memory(request):
    start = request.GET.get("start","")
    package_name = request.GET.get("package_name","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    else:
        start = (datetime.timedelta(days=-1) + today)
    _start = start.strftime('%Y-%m-%d')

    """memory_info 内存信息"""

    # memory_info = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d")).order_by("-dst_count")
    memory_info = []
    g1 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__lte=800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"1G以下","dst_count":g1})
    g1_2 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=800,memory__lte=1800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"1G-2G","dst_count":g1_2})
    g2_3 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=1800,memory__lte=2800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"2G-3G","dst_count":g2_3})
    g3_4 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=2800,memory__lte=3800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"3G-4G","dst_count":g3_4})
    g4_8 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=3800,memory__lte=7800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"4G-8G","dst_count":g4_8})
    g8_16 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=7800,memory__lte=15800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"8G-16G","dst_count":g8_16})
    g16_32 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=15800,memory__lte=31800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"16G-32G","dst_count":g16_32})
    g_32 = ResultUserComputerInfoMemory.objects.filter(result_date=start.strftime("%Y-%m-%d"),memory__gt=31800).aggregate(Sum('dst_count'))["dst_count__sum"] or 0
    memory_info.append({"result_date":start.strftime("%Y-%m-%d"),"memory":"32G以上","dst_count":g_32})

    res_number = 30
    page = int(request.GET.get('page',"1"))
    paginator = Paginator(memory_info, res_number)
    try:
        memory_info = paginator.page(page)
    except PageNotAnInteger:
        # If page is not an integer, deliver first page.
        memory_info = paginator.page(1)
    except EmptyPage:
        # If page is out of range (e.g. 9999), deliver last page of results.
        memory_info = paginator.page(paginator.num_pages)

    return render(request, "user_computer_info_memory.html", {"start":_start,"memory_info":memory_info,'page_range':paginator.page_range})


def user_computer_info_cpu(request):
    start = request.GET.get("start","")
    today = datetime.date.today()
    if start:
        start = datetime.datetime.strptime(start,'%Y-%m-%d')
    else:
        start = (datetime.timedelta(days=-1) + today)
    _start = start.strftime('%Y-%m-%d')

    """memory_info 内存信息"""

    cpu_info = ResultUserComputerInfoCPU.objects.filter(result_date=start.strftime("%Y-%m-%d")).order_by("-dst_count")
    res_number = 30
    page = int(request.GET.get('page',"1"))
    paginator = Paginator(cpu_info, res_number)
    try:
        cpu_info = paginator.page(page)
    except PageNotAnInteger:
        # If page is not an integer, deliver first page.
        cpu_info = paginator.page(1)
    except EmptyPage:
        # If page is out of range (e.g. 9999), deliver last page of results.
        cpu_info = paginator.page(paginator.num_pages)

    return render(request, "user_computer_info_cpu_info.html", {"start":_start,"cpu_info":cpu_info,'page_range':paginator.page_range})







