# coding=utf-8

from django.db import models
import time
from django.utils import timezone

from bst_server.settings import BUCKET_APPCENTER, PUBLIC_OSS_BEIJING_HOST, INTERNAL_OSS_BEIJING_HOST, ALIYUN_IMG_HOST
from util.basemodel import JSONBaseModel


class OssFile(JSONBaseModel):
    """
    OSS文件基类
    """
    file_url = models.CharField(null=False, max_length=250, unique=False, verbose_name=u'文件存储位置')
    status = models.BooleanField(default=True, verbose_name=u'状态')
    file_type = models.CharField(max_length=20,null=True, verbose_name=u'文件类型')
    img_size = models.CharField(max_length=20, null=True, verbose_name=u'图片大小', help_text=u'格式:80x80')
    bucket = models.CharField(default=BUCKET_APPCENTER, max_length=20, verbose_name=u'BUCKET')
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')

    def get_url(self, type="public"):
        """
        拼接获取url
        """
        host = ""
        if type == "public":
            host = PUBLIC_OSS_BEIJING_HOST
        elif type == "internal":
            host = INTERNAL_OSS_BEIJING_HOST
        elif type == "img":
            host = ALIYUN_IMG_HOST
        return host + self.file_url

    def get_file_type(self):
        if not self.file_type:
            if self.file_url.find(".") != -1:
                self.file_type = self.file_url.split(".")[1]
            else:
                return False
        else:
            return self.file_type

    class Meta:
        abstract = True


class OssPackage(OssFile):
    """
    游戏安装包
    """
    upload_dir = models.CharField(default="package", null=False, max_length=250, unique=False, verbose_name=u'文件存储位置')
    type = models.CharField(null=False, max_length=50, unique=False, verbose_name=u'包类型')
    version = models.CharField(null=True, max_length=20, unique=False, verbose_name=u'版本号')
    key = models.CharField(null=True, max_length=300, unique=False, verbose_name=u'key')
    md5 = models.CharField(null=True, max_length=256, unique=False, verbose_name=u'md5')
    desc = models.TextField(default="", blank=True, null=False, unique=False, verbose_name=u'描述')
    mandatory = models.BooleanField(default=False, null=False, verbose_name=u'是否强制升级')


class ObsInfo(JSONBaseModel):
    """
    obs包
    """
    file_url = models.CharField(null=False, max_length=250, unique=False, verbose_name=u'文件存储位置')
    status = models.BooleanField(default=True, verbose_name=u'状态')
    bucket = models.CharField(default=BUCKET_APPCENTER, max_length=20, verbose_name=u'BUCKET')
    type = models.CharField(null=False, max_length=50, unique=False, verbose_name=u'包类型')
    version = models.CharField(null=True, max_length=20, unique=False, verbose_name=u'版本号')
    md5 = models.CharField(null=True, max_length=256, unique=False, verbose_name=u'md5')
    desc = models.TextField(default="", blank=True, null=False, unique=False, verbose_name=u'描述')
    mandatory = models.BooleanField(default=False, null=False, verbose_name=u'是否强制升级')
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')



