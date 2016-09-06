# coding=utf-8
from __future__ import unicode_literals


import datetime

from django.db import models

# Create your models here.
from basemodel import JSONBaseModel

class DataDB(JSONBaseModel):
    """
    数据父类
    """
    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=64, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    status = models.CharField(max_length=32, default="success", unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False)
    datetime = models.DateTimeField(null=False, verbose_name=u'数据插入时间')
    # uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    class Meta:
        abstract = True

    @classmethod
    def add_data(cls, guid, op, status, version, channel):
        if channel == "": channel="bscn"
        data = cls()
        data.guid = guid
        data.op = op
        data.status = status
        data.version = version
        data.channel = channel
        data.datetime = datetime.datetime.now()
        try:
            data.save()
            return True
        except Exception as e:
            print e
            return False



class EngineInstall(DataDB):
    """
    引擎下载安装
    """

class EngineActivity(DataDB):
    """
    引擎活动
    """

class EmulatorInstall(DataDB):
    """
    模拟器下载安装
    """

class EmulatorActivity(DataDB):
    """
    模拟器活动
    """

class ResultData(JSONBaseModel):
    """
    结果表父类
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    op = models.CharField(max_length=64, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    status = models.CharField(max_length=20, default="success", unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    count = models.IntegerField(default=0, null=False)
    dst_count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    class Meta:
        abstract = True

    @classmethod
    def insert_a_data(cls, result_date, op, version, status, count, dst_count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.op = op
        a.version = version
        a.status = status
        a.channel = channel
        a.count = count
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class ResultEngineInstall(ResultData):
    """
    引擎安装卸载量结果表
    """

class ResultEngineActivity(ResultData):
    """
    引擎安装卸载量结果表
    """

class ResultEmulatorInstall(ResultData):
    """
    引擎安装卸载量结果表
    """

class ResultEmulatorActivity(ResultData):
    """
    引擎安装卸载量结果表
    """


class ResultRetention(JSONBaseModel):
    """
    留存数据
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    count = models.IntegerField(default=0, null=False)
    retention = models.CharField(default="1", max_length=10, unique=False, null=False, blank=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, version, count, retention, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.version = version
        a.count = count
        a.channel = channel
        a.retention = retention
        try:
            a.save()
        except Exception, e:
            print e


class ResultRetentionEngine(JSONBaseModel):
    """
    第二种算法算的留存
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    count = models.IntegerField(default=0, null=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    retention = models.CharField(default="1", max_length=10, unique=False, null=False, blank=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, version, count, retention, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.version = version
        a.channel = channel
        a.count = count
        a.retention = retention
        try:
            a.save()
        except Exception, e:
            print e


class AppInstall(DataDB):
    package_name = models.CharField(max_length=256, unique=False, null=False, blank=False)

    @classmethod
    def add_data(cls, guid, op, status, version, package_name, channel):
        if channel == "": channel="bscn"
        data = cls()
        data.guid = guid
        data.op = op
        data.status = status
        data.version = version
        data.package_name = package_name
        data.channel = channel
        data.datetime = datetime.datetime.now()
        try:
            data.save()
            return True
        except Exception as e:
            print e
            return False


class AppActivity(DataDB):
    package_name = models.CharField(max_length=256, unique=False, null=False, blank=False)

    @classmethod
    def add_data(cls, guid, op, status, version, package_name, channel):
        if channel == "": channel="bscn"
        data = cls()
        data.guid = guid
        data.op = op
        data.status = status
        data.version = version
        data.package_name = package_name
        data.channel = channel
        data.datetime = datetime.datetime.now()
        try:
            data.save()
            return True
        except Exception as e:
            print e
            return False


class ResultEmulatorSession(JSONBaseModel):
    """
    模拟器使用时长
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=True)
    # channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dus = models.FloatField(default=0.0, null=False)
    count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def add_data(cls,result_data,version,dus,count):
        data = cls()
        data.result_date = result_data
        data.version = version
        # data.channel = channel
        data.dus = dus
        data.count = count
        data.save()
        try:
            data.save()
            return True
        except Exception as e:
            print e
            return False


class AppCenterData(JSONBaseModel):
    """
    应用中心csv
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    event_number = models.IntegerField(default=0, null=False)
    event_type = models.CharField(max_length=100, unique=False, null=False, blank=True)
    event_count = models.IntegerField(default=0, null=False)
    event_uv = models.IntegerField(default=0, null=False)
    event_value = models.IntegerField(default=0, null=False)
    event_avg_value = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def add_data(cls,result_data,event_number,event_type,event_count,event_uv,event_value,event_avg_value):
        data = cls()
        data.result_date = result_data
        data.event_number = event_number
        data.event_type = event_type
        data.event_count = event_count
        data.event_uv = event_uv
        data.event_value = event_value
        data.event_avg_value = event_avg_value
        data.save()
        try:
            data.save()
            return True
        except Exception as e:
            print e
            return False


class ResultAppInstall(ResultData):
    """
    app 安装结果数据
    """
    package_name = models.CharField(max_length=255, unique=True, null=False, blank=False)
    game_name = models.CharField(max_length=255, unique=False, null=True)

    @classmethod
    def insert_a_data(cls, result_date, op, version, status, count, dst_count, package_name, game_name, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.op = op
        a.version = version
        a.status = status
        a.count = count
        a.dst_count = dst_count
        a.package_name = package_name
        a.game_name = game_name
        a.channel = channel
        try:
            a.save()
        except Exception, e:
            print e


class ResultAppActivity(ResultData):
    """
    app 活动结果数据
    """
    package_name = models.CharField(max_length=255, unique=False, null=False, blank=False)
    game_name = models.CharField(max_length=255, unique=False, null=True)

    @classmethod
    def insert_a_data(cls, result_date, op, version, status, count, dst_count, package_name, game_name, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.op = op
        a.version = version
        a.status = status
        a.count = count
        a.dst_count = dst_count
        a.package_name = package_name
        a.game_name = game_name
        a.channel = channel
        try:
            a.save()
        except Exception, e:
            print e


class ResultAppSession(JSONBaseModel):
    """
    app使用时长
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    package_name = models.CharField(max_length=256, unique=False, null=False, blank=True)
    # channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    das = models.IntegerField(default=0, null=False, verbose_name=u'daily app session')
    count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def add_data(cls,result_data,package_name,das,count):
        data = cls()
        data.result_date = result_data
        data.package_name = package_name
        # data.channel = channel
        data.das = das
        data.count = count
        data.save()
        try:
            data.save()
            return True
        except Exception as e:
            print e
            return False


class MidResultInstallInitEmulator(JSONBaseModel):
    """
    既安装又启动的用户数量
    中间结果
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, dst_count, version, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.channel = channel
        a.dst_count = dst_count
        a.version = version
        try:
            a.save()
        except Exception, e:
            print e


class MidResultInstallInitEngine(JSONBaseModel):
    """
    既安装又启动引擎的用户数
    用于根据引擎算留存的中间结果
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, dst_count, version, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.channel = channel
        a.dst_count = dst_count
        a.version = version
        try:
            a.save()
        except Exception, e:
            print e


class GeneralData(JSONBaseModel):
    """
    通用数据类
    """
    type = models.CharField(max_length=50, unique=False, null=False, blank=False)
    json = models.TextField(max_length=512, null=False, help_text=u'json数据')
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, type, json):
        a = cls()
        a.type = type
        a.json = json
        try:
            a.save()
        except Exception, e:
            print e


class ResultGeneralEngineInstall(JSONBaseModel):
    """
    通用结果数据，
    op : engine_install
    engine_type ：plus or legacy
    """
    class Meta:
        db_table = "result_general_engine_install"

    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=20, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    engine_type = models.CharField(max_length=20, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    datetime = models.DateTimeField(null=False, verbose_name=u'GeneralData uptime字段')

    @classmethod
    def add_data(cls, data, uptime):
        a = cls()
        a.guid = data.get("guid","")
        a.op = data.get("op","")
        a.version = data.get("version","")
        a.engine_type = data.get("engine_type","")
        a.channel = data.get("channel","bscn")
        a.datetime = uptime
        try:
            a.save()
        except Exception, e:
            print e


class ResultGeneralEngInitError(JSONBaseModel):
    """
    通用结果数据
    eng_init_error
    errcode : xxxx
    """
    class Meta:
        db_table = "result_general_eng_init_error"

    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=20, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    errcode = models.CharField(max_length=20, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    datetime = models.DateTimeField(null=False, verbose_name=u'GeneralData uptime字段')

    @classmethod
    def add_data(cls, data, uptime):
        a = cls()
        a.guid = data.get("guid","")
        a.op = data.get("op","")
        a.version = data.get("version","")
        a.errcode = data.get("errcode","")
        a.channel = data.get("channel","bscn")
        a.datetime = uptime
        try:
            a.save()
        except Exception, e:
            print e


class ResultGeneralAPKInstError(JSONBaseModel):
    """
    通用结果数据
    apk_inst_error
    errcode : xxxx
    """
    class Meta:
        db_table = "result_general_apk_inst_error"

    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=20, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    errcode = models.CharField(max_length=20, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    datetime = models.DateTimeField(null=False, verbose_name=u'GeneralData uptime字段')

    @classmethod
    def add_data(cls, data, uptime):
        a = cls()
        a.guid = data.get("guid","")
        a.op = data.get("op","")
        a.version = data.get("version","")
        a.errcode = data.get("errcode","")
        a.channel = data.get("channel","bscn")
        a.datetime = uptime
        try:
            a.save()
        except Exception, e:
            print e


class ConfUninstallReasonMeaning(JSONBaseModel):
    """
    卸载原因
    """
    class Meta:
        db_table = "conf_uninstall_reason_meaning"
    code = models.IntegerField(default=0, null=False, unique=True)
    meaning = models.CharField(max_length=64, unique=False, null=False, blank=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def get_meaning(cls, dword):
        try:
            a = cls.objects.get(pk=dword)
            return a.meaning
        except Exception, e:
            return ""

class ResultGeneralUninstallReason(JSONBaseModel):
    """
    卸载原因
    op:uninst_reason
    uninst_reason_string:手写原因
    uninst_reason_dword：选择第几个
    """
    class Meta:
        db_table = "result_general_uninstall_reason"

    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=20, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    uninst_reason_string = models.CharField(default="",max_length=512, unique=False, null=False, blank=True)
    uninst_reason_dword = models.IntegerField(default=0, null=False)
    datetime = models.DateTimeField(null=False, verbose_name=u'GeneralData uptime字段')

    @classmethod
    def add_data(cls, data, uptime):
        a = cls()
        a.guid = data.get("guid","")
        a.op = data.get("op","")
        a.version = data.get("version","")
        a.channel = data.get("channel","bscn")
        a.uninst_reason_string = data.get("uninst_reason_string","")
        a.uninst_reason_dword = data.get("uninst_reason_dword","")
        a.datetime = uptime
        try:
            a.save()
        except Exception, e:
            print e


class ResultGeneralEngineInstError(JSONBaseModel):
    """
    通用结果数据
    engine_inst_error
    errcode : xxxx
    """
    class Meta:
        db_table = "result_general_engine_inst_error"

    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=20, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    errcode = models.CharField(max_length=20, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    datetime = models.DateTimeField(null=False, verbose_name=u'GeneralData uptime字段')

    @classmethod
    def add_data(cls, data, uptime):
        a = cls()
        a.guid = data.get("guid","")
        a.op = data.get("op","")
        a.version = data.get("version","")
        a.errcode = data.get("errcode","")
        a.channel = data.get("channel","bscn")
        a.datetime = uptime
        try:
            a.save()
        except Exception, e:
            print e


class ResultGeneralOsVersion(JSONBaseModel):
    """
    通用结果数据
    osver
    osver memory cpu
    """
    class Meta:
        db_table = "result_general_osver"

    guid = models.CharField(max_length=100, unique=False, null=False, blank=False)
    op = models.CharField(max_length=20, unique=False, null=False, blank=False)
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    status = models.CharField(max_length=20, default="success", unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    osver = models.CharField(max_length=20, null=False, blank=False)
    memory = models.CharField(max_length=20, null=False, blank=False)
    cpu = models.CharField(max_length=128, null=False, blank=False)
    datetime = models.DateTimeField(null=False, verbose_name=u'GeneralData uptime字段')

    @classmethod
    def add_data(cls, data, uptime):
        a = cls()
        a.guid = data.get("guid","")
        a.op = data.get("op","")
        a.status = data.get("status","")
        a.osver = data.get("osver","")
        a.memory = data.get("memory","")
        a.cpu = data.get("cpu","")
        a.version = data.get("version","")
        a.channel = data.get("channel","bscn")
        a.datetime = uptime
        try:
            a.save()
        except Exception, e:
            print e


class ResultEmulatorInstallCount(JSONBaseModel):
    """
    日安装量，用于计算卸载率
    """
    class Meta:
        db_table = "result_emulator_install_count"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, version, dst_count, count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.version = version
        a.channel = channel
        a.count = count
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e

class ResultEmulatorUninstallCount(JSONBaseModel):
    """
    当日安装并且卸载量，用于计算卸载率
    """
    class Meta:
        db_table = "result_emulator_uninstall_count"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, version, dst_count, count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.version = version
        a.channel = channel
        a.count = count
        a.dst_count = dst_count

        try:
            a.save()
        except Exception, e:
            print e


class ResultEngineUninstallRate(JSONBaseModel):
    """
    引擎卸载率
    """
    class Meta:
        db_table = "result_engine_uninstall_rate"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    field = models.CharField(max_length=32, unique=False, null=False, blank=False)
    version = models.CharField(max_length=32, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, field, version, dst_count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.field = field
        a.version = version
        a.channel = channel
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class ResultEmulatorUninstallNextDayCount(JSONBaseModel):
    """
    次日卸载量，用于计算卸载率
    """
    class Meta:
        db_table = "result_emulator_uninstall_next_day_count"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, version, dst_count, count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.version = version
        a.channel = channel
        a.count = count
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class ResultEngineDAU(JSONBaseModel):
    """
    用于计算engine dau
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(max_length=50, unique=False, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, version, dst_count, count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.version = version
        a.channel = channel
        a.count = count
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class ResultUserComputerInfoOS(JSONBaseModel):
    """
    用户信息 os
    """
    class Meta:
        db_table = "result_usercomputerinfo_os"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    os = models.CharField(max_length=16, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, os, dst_count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.channel = channel
        a.os = os
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class ResultUserComputerInfoMemory(JSONBaseModel):
    """
    用户信息 内存
    """
    class Meta:
        db_table = "result_usercomputerinfo_memory"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    memory = models.IntegerField(default=0, null=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, memory, dst_count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.memory = memory
        a.channel = channel
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class ResultUserComputerInfoCPU(JSONBaseModel):
    """
    用户信息 内存
    """
    class Meta:
        db_table = "result_usercomputerinfo_cpu"

    result_date = models.DateField(null=False, verbose_name=u'日期')
    cpu = models.CharField(max_length=128, null=False, blank=False)
    channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, result_date, cpu, dst_count, channel):
        if channel == "": channel="bscn"
        a = cls()
        a.result_date = result_date
        a.cpu = cpu
        a.channel = channel
        a.dst_count = dst_count
        try:
            a.save()
        except Exception, e:
            print e


class OSVersion(JSONBaseModel):
    """
    os 版本信息
    """
    os = models.CharField(max_length=16, null=False, blank=False, unique=True)
    info = models.CharField(max_length=64, null=False, blank=False)


class ResultAppTotal(JSONBaseModel):
    """
    app 总体信息
    """
    #todo:channel
    result_date = models.DateField(null=False, verbose_name=u'日期')
    version = models.CharField(default="", max_length=32, unique=False, null=False)
    # channel = models.CharField(default="bscn", max_length=64, unique=False, null=False, blank=True)

    daily_user_init = models.IntegerField(default=0, null=False)
    daily_user_init_count = models.IntegerField(default=0, null=False)
    daily_init_fail = models.IntegerField(default=0, null=False)
    daily_init_fail_count = models.IntegerField(default=0, null=False)

    daily_install = models.IntegerField(default=0, null=False)
    daily_install_count = models.IntegerField(default=0, null=False)
    daily_install_fail = models.IntegerField(default=0, null=False)
    daily_install_fail_count = models.IntegerField(default=0, null=False)

    daily_download = models.IntegerField(default=0, null=False)
    daily_download_count = models.IntegerField(default=0, null=False)
    daily_download_fail = models.IntegerField(default=0, null=False)
    daily_download_fail_count = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_a_data(cls, **kwargs):
        a = cls()
        try:
            a.result_date = kwargs["result_date"]
            # a.channel = kwargs.get("channel","")
            a.version = kwargs["version"]
            a.daily_user_init = kwargs["daily_user_init"]
            a.daily_user_init_count = kwargs["daily_user_init_count"]
            a.daily_init_fail = kwargs["daily_init_fail"]
            a.daily_init_fail_count = kwargs["daily_init_fail_count"]

            a.daily_install = kwargs["daily_install"]
            a.daily_install_count = kwargs["daily_install_count"]
            a.daily_install_fail = kwargs["daily_install_fail"]
            a.daily_install_fail_count = kwargs["daily_install_fail_count"]

            a.daily_download = kwargs["daily_download"]
            a.daily_download_count = kwargs["daily_download_count"]
            a.daily_download_fail = kwargs["daily_download_fail"]
            a.daily_download_fail_count = kwargs["daily_download_fail_count"]

            a.save()
        except Exception, e:
            print e


class ResultAppLocalTop500(JSONBaseModel):
    """
    本地安装top500
    """
    result_date = models.DateField(null=False, verbose_name=u'日期')
    package_name = models.CharField(max_length=255, unique=False, null=False, blank=True)
    dst_count = models.IntegerField(default=0, null=False)
    count = models.IntegerField(default=0, null=False)
    version = models.CharField(default="", max_length=32, unique=False, null=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def insert_data(cls, result_date, package_name, version, dst_count, count):
        a = cls()
        a.result_date = result_date
        a.package_name = package_name
        a.version = version
        a.dst_count = dst_count
        a.count = count
        try:
            a.save()
        except Exception, e:
            print e


class Scope(JSONBaseModel):
    class Meta:
        abstract = True

    channel = models.CharField(default="", max_length=64, unique=False, null=False, blank=True)
    osver = models.CharField(default="", max_length=20, null=False, blank=True)
    modes = models.CharField(default="day", max_length=20, null=False, blank=False)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')




# class ScopeApp(Scope):
#     class Meta:
#         db_table = "scope_app"
#
#     package_name = models.CharField(default="", max_length=256, unique=False, null=False, blank=True)
#     version = models.CharField(default="", max_length=50, unique=False, null=False, blank=True)
#
#     @classmethod
#     def get_scope(cls, package_name, version, channel, osver):
#         if cls.objects.filter(modes="day",package_name=package_name,version=version,channel=channel,osver=osver).exists():
#             return cls.objects.get(modes="day",package_name=package_name,version=version,channel=channel,osver=osver)
#         else:
#             return None
#
#     @classmethod
#     def add_scope(cls, package_name, version, channel, osver):
#         if not cls.objects.filter(package_name=package_name,version=version,channel=channel,osver=osver).exists():
#             a = cls()
#             try:
#                 a.package_name = package_name
#                 a.version = version
#                 a.channel = channel
#                 a.osver = osver
#                 a.save()
#                 print "add scope...",package_name,version,channel,osver
#                 return True
#             except Exception, e:
#                 print e
#                 print package_name,version,channel,osver
#                 return False


class ScopeEngine(Scope):
    class Meta:
        db_table = "scope_engine"

    version = models.CharField(default="", max_length=50, unique=False, null=False, blank=True)

    @classmethod
    def get_scope(cls, version, channel, osver):
        if cls.objects.filter(modes="day",version=version,channel=channel,osver=osver).exists():
            return cls.objects.get(modes="day",version=version,channel=channel,osver=osver)
        else:
            return None

    @classmethod
    def add_scope(cls, version, channel, osver):
        if not cls.objects.filter(version=version,channel=channel,osver=osver).exists():
            a = cls()
            try:
                a.version = version
                a.channel = channel
                a.osver = osver
                a.save()
                print "add scope...",version,channel,osver
                return True
            except Exception, e:
                print e
                print version,channel,osver
                return False

class ScopeAppPackagename(JSONBaseModel):
    class Meta:
        db_table = "scope_app_package_name"

    modes = models.CharField(default="day", max_length=20, null=False, blank=False)
    package_name = models.CharField(default="", max_length=255, unique=False, null=False, blank=True)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def get_scope(cls, package_name, modes):
        if cls.objects.filter(modes=modes,package_name=package_name).exists():
            return cls.objects.get(modes=modes,package_name=package_name)
        else:
            return None

    @classmethod
    def add_scope(cls, modes, package_name):
        if not cls.objects.filter(modes=modes,package_name=package_name).exists():
            a = cls()
            try:
                a.modes = modes
                a.package_name = package_name
                a.save()
                print "add scope...",modes,package_name
                return True
            except Exception, e:
                print e
                print modes,package_name
                return False


class ScopeAppTotal(Scope):
    class Meta:
        db_table = "scope_app_total"

    version = models.CharField(default="", max_length=50, unique=False, null=False, blank=True)

    @classmethod
    def get_scope(cls, version, channel, osver):
        if cls.objects.filter(modes="day",version=version,channel=channel,osver=osver).exists():
            return cls.objects.get(modes="day",version=version,channel=channel,osver=osver)
        else:
            return None

    @classmethod
    def add_scope(cls, version, channel, osver):
        if not cls.objects.filter(version=version,channel=channel,osver=osver).exists():
            a = cls()
            try:
                a.version = version
                a.channel = channel
                a.osver = osver
                a.save()
                print "add scope...",version,channel,osver
                return True
            except Exception, e:
                print e
                print version,channel,osver
                return False


class ScopeEmulator(Scope):
    class Meta:
        db_table = "scope_emulator"

    version = models.CharField(default="", max_length=50, unique=False, null=False, blank=True)

    @classmethod
    def get_scope(cls, version, channel, osver):
        if cls.objects.filter(modes="day",version=version,channel=channel,osver=osver).exists():
            return cls.objects.get(modes="day",version=version,channel=channel,osver=osver)
        else:
            return None

    @classmethod
    def add_scope(cls, version, channel, osver):
        if not cls.objects.filter(version=version,channel=channel,osver=osver).exists():
            a = cls()
            try:
                a.version = version
                a.channel = channel
                a.osver = osver
                a.save()
                print "add scope...",version,channel,osver
                return True
            except Exception, e:
                print e
                print version,channel,osver
                return False


class AppTotalStat(JSONBaseModel):
    """
    app total 数据
    """
    class Meta:
        db_table = "stats_app_total"

    scope_id = models.PositiveIntegerField(default=0, null=False)
    result_date = models.DateField(null=False, verbose_name=u'日期')

    daily_user_init = models.PositiveIntegerField(default=0, null=False)
    daily_user_init_count = models.PositiveIntegerField(default=0, null=False)
    daily_init_fail = models.PositiveIntegerField(default=0, null=False)
    daily_init_fail_count = models.PositiveIntegerField(default=0, null=False)

    daily_install = models.PositiveIntegerField(default=0, null=False)
    daily_install_count = models.PositiveIntegerField(default=0, null=False)
    daily_install_fail = models.PositiveIntegerField(default=0, null=False)
    daily_install_fail_count = models.PositiveIntegerField(default=0, null=False)

    daily_download = models.PositiveIntegerField(default=0, null=False)
    daily_download_count = models.PositiveIntegerField(default=0, null=False)
    daily_download_fail = models.PositiveIntegerField(default=0, null=False)
    daily_download_fail_count = models.PositiveIntegerField(default=0, null=False)

    daily_install_from_download = models.PositiveIntegerField(default=0, null=False)
    daily_install_from_download_count = models.PositiveIntegerField(default=0, null=False)
    daily_install_from_download_fail = models.PositiveIntegerField(default=0, null=False)
    daily_install_from_download_fail_count = models.PositiveIntegerField(default=0, null=False)

    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')


class AppPackagenameStat(JSONBaseModel):
    """
    app total 数据
    """
    class Meta:
        db_table = "stats_app_package_name"

    scope_id = models.PositiveIntegerField(default=0, null=False)
    result_date = models.DateField(null=False, verbose_name=u'日期')

    daily_user_init = models.PositiveIntegerField(default=0, null=False)
    daily_user_init_count = models.PositiveIntegerField(default=0, null=False)
    daily_init_fail = models.PositiveIntegerField(default=0, null=False)
    daily_init_fail_count = models.PositiveIntegerField(default=0, null=False)

    daily_install = models.PositiveIntegerField(default=0, null=False)
    daily_install_count = models.PositiveIntegerField(default=0, null=False)
    daily_install_fail = models.PositiveIntegerField(default=0, null=False)
    daily_install_fail_count = models.PositiveIntegerField(default=0, null=False)

    daily_download = models.PositiveIntegerField(default=0, null=False)
    daily_download_count = models.PositiveIntegerField(default=0, null=False)
    daily_download_fail = models.PositiveIntegerField(default=0, null=False)
    daily_download_fail_count = models.PositiveIntegerField(default=0, null=False)

    daily_install_from_download = models.PositiveIntegerField(default=0, null=False)
    daily_install_from_download_count = models.PositiveIntegerField(default=0, null=False)
    daily_install_from_download_fail = models.PositiveIntegerField(default=0, null=False)
    daily_install_from_download_fail_count = models.PositiveIntegerField(default=0, null=False)

    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')


class EngineStats(JSONBaseModel):
    """
    引擎数据
    """
    class Meta:
        db_table = "stats_engine"

    scope_id = models.IntegerField(default=0, null=False)
    result_date = models.DateField(null=False, verbose_name=u'日期')
    install_success_user = models.IntegerField(default=0, null=False)
    acc_install_success_user = models.IntegerField(default=0, null=False)
    install_fail_user = models.IntegerField(default=0, null=False)
    download_begin_user = models.IntegerField(default=0, null=False)
    download_ok_user = models.IntegerField(default=0, null=False)

    init_success_user = models.IntegerField(default=0, null=False)
    init_not_success_user = models.IntegerField(default=0, null=False)
    init_fail_user = models.IntegerField(default=0, null=False)
    first_init_success_user = models.IntegerField(default=0, null=False)    #first_init update_init
    first_init_fail_user = models.IntegerField(default=0, null=False)

    first_init_no_update_success_user = models.IntegerField(default=0, null=False) #first_init
    first_init_no_update_and_uninstall_emulator_user = models.IntegerField(default=0, null=False) #当日初始化引擎并卸载客户端的人数
    first_init_no_update_and_next_day_uninstall_emulator_user = models.IntegerField(default=0, null=False) #当日初始化引擎并次日卸载客户端的人数

    update_success_user = models.IntegerField(default=0, null=False)
    update_fail_user = models.IntegerField(default=0, null=False)
    update_init_success_user = models.IntegerField(default=0, null=False)
    update_init_fail_user = models.IntegerField(default=0, null=False)

    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')


class EmulatorStats(JSONBaseModel):
    """
    模拟器数据
    """
    class Meta:
        db_table = "stats_emulator"

    scope_id = models.IntegerField(default=0, null=False)
    result_date = models.DateField(null=False, verbose_name=u'日期')
    install_success_user = models.IntegerField(default=0, null=False)
    acc_install_success_user = models.IntegerField(default=0, null=False)
    install_fail_user = models.IntegerField(default=0, null=False)
    uninstall_success_user = models.IntegerField(default=0, null=False) #当日安装成功用户里面卸载的
    next_day_uninstall_success_user = models.IntegerField(default=0, null=False) #当日安装成功用户转天卸载的
    uninstall_success_user_base_on_engine = models.IntegerField(default=0, null=False) #基于引擎的当日卸载人数
    next_day_uninstall_success_user_base_on_engine = models.IntegerField(default=0, null=False) #基于引擎的次日卸载人数
    install_and_init_success_user = models.IntegerField(default=0, null=False) #当日安装并启动成功的用户数量
    retention_2 = models.IntegerField(default=0, null=False) #次日留存人数
    retention_7 = models.IntegerField(default=0, null=False) #7日留存人数
    init_success_user = models.IntegerField(default=0, null=False)  #日活跃用户
    init_success_count = models.IntegerField(default=0, null=False)  #用户登录次数
    init_fail_user = models.IntegerField(default=0, null=False) #启动失败用户
    init_user = models.IntegerField(default=0, null=False) #试图启动的用户
    engine_install_and_init_success_user = models.IntegerField(default=0, null=False) #当日引擎安装并启动成功的用户数量且包括初次启动成功的
    retention_2_base_on_engine = models.IntegerField(default=0, null=False) #当日引擎安装并启动成功的且次日模拟器启动成功的用户数量
    engine_install_and_init_success_user2 = models.IntegerField(default=0, null=False)#当日引擎安装并启动成功的用户数量 不包括初次启动成功的
    retention_2_base_on_engine2 = models.IntegerField(default=0, null=False)

    retention_7_base_on_engine_normal = models.IntegerField(default=0, null=False) #当日引擎安装并启动成功且在后面第七天模拟器启动成功的人数
    retention_14_base_on_engine_normal = models.IntegerField(default=0, null=False) #当日引擎安装并启动成功且在后面第十四天模拟器启动成功的人数

    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')



