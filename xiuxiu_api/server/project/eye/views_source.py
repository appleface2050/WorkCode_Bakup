# -*- coding: utf-8 -*-

import base64
import csv
import datetime
import calendar
import os
import json
import random
import operator
import chardet
from PIL import Image
import cStringIO
from math import *
import urllib
import urllib2
from django.conf import settings
from django.contrib import auth
from django.contrib.auth.models import User
from django.contrib.sessions.backends.db import SessionStore
from django.core import serializers
from django.db.models import Q, Count
from django.http import HttpResponse, HttpResponseRedirect
from django.shortcuts import render
from django.views.decorators.csrf import csrf_exempt
import django_filters
from django_filters import Filter, FilterSet
from rest_framework import viewsets, filters, status
from rest_framework.authtoken.models import Token
from rest_framework.decorators import api_view, permission_classes, list_route
from rest_framework.permissions import AllowAny, IsAuthenticated
from rest_framework.response import Response
from .models import UserExtension, Shop, Level, Device, Publish, Address, UserPublishRelations, RedEnvelope, PhoneInfos, \
    ForumCategory, ShopDeal
from .models import Comment, China, ShopCategory, UserShopRelations, Feedback, ClickTitleRecord, Game
from .models import ForumPost, ForumReply, Detector, ForumCategoryCarousel, Coupon, Channel, Use, HomePageCarousel
from .models import DetectorRelation, Company, Product, PopWindow, PushShop, ShareStatistics
from .serializers import UserExtensionSerializer, UserSerializer, UserPublishRelationsSerializer, \
    ForumCategorySerializer, ForumPostSourceSerializer, ForumReplySourceSerializer, ChannelSerializer
from .serializers import ShopSerializer, LevelSerializer, DeviceSerializer, DetectorSerializer, GameSerializer
from .serializers import PublishSerializer, AddressSerializer, ForumPostSerializer, ForumReplySerializer
from .serializers import CommentSerializer, ChinaSerializer, ShopCategorySerializer, UserShopRelationsSerializer
from .serializers import ForumCategoryCarouselSerializer, CouponSerializer, UseSerializer, HomePageCarouselSerializer
from .serializers import DetectorRelationSerializer, CompanySerializer, ProductSerializer, PopWindowSerializer
from .serializers import PushShopSerializer, ShareStatisticsSerializer
from .constant import PUBLISH_SUBJECTIVE_CONSTANT, DeviceIcon, USER_DEFAULT_ICON, DEVICE_BRAND, SHOP_DEFAULT_ICON, \
    FORUM_CATEGORY_DEFAULT_ICON, DIANPING_RATING, USER_DEFAULT_MALE_ICON, USER_DEFAULT_FEMALE_ICON
from .rules import RULES_CONSTANT
from common.weathers import Weathers
from common.coupons import Coupons
from common.datetimes import Datetimes
from common.messages import Messages
from common.users import Users
from common.detectors import Detectors
from common.games import Games
from common.logs import Logs
from common.redenvelopes import RedEnvelopes
from common.games import GraphColor
from common.publish_categories import PublishCategories
from common.publishes import Publishes
from common.companies import Companies
from common.decorators import json_response, execute_time, authenticated
from common.digit import Digit
from common.addresses import Addresses
from common.shop_categories import ShopCategories
from common.shops import Shops
from common.discovery import Discovery
from common.home_page import HomePage
from common.tokens import Tokens
from common.comments import Comments
from common.files import Files
from common.devices import Devices
from common.user_publish_relations import UserPublishRelationMethods
from common.scores import Scores
from common.methods import Methods
from common.user_shop_relations import UserShopRelationMethods
from common.red_envelope_configuration import RedEnvelopeConfiguration
from common.characters import Characters
from common.dianpings import DianPings
from common.hot import Hot
from common import files
from common import dianpings
from common import datetimes
from common import scores
from common import messages
from common import shop_categories
from common import pushes

DEFAULT_CITY = u"北京"


class UserViewSet(viewsets.ModelViewSet):
    """
    基本的用户操作，当生成新的用户时，自动在用户的扩展表生成相应的数据 \n
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： username， email
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/users/?username=admin&email=admin@newbeeair.com
        查询：
            字段: username, email
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/users/?search=dmi
        排序：
            字段：username, email
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/users/?ordering=username
            http://192.168.2.134:8000/rest/api/users/?ordering=-username
            http://192.168.2.134:8000/rest/api/users/?ordering=username,email
            默认排序字段：username
    更新： PUT  \n
    删除： DELETE
    """
    queryset = User.objects.all()
    serializer_class = UserSerializer
    filter_fields = ('username', 'email')

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter)
    search_fields = ('username', 'email')

    ordering_fields = ('username', 'email', 'id',)
    ordering = ('-id',)


class ShopViewSet(viewsets.ModelViewSet):
    """
    商铺基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： dianping_id， recommended
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/shops/?dianping_id=2015&recommended=1
        查询：
            字段: dianping_id
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/shops/?search=1
        排序：
            字段：dianping_id， recommended
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/shops/?ordering=dianping_id
            http://192.168.2.134:8000/rest/api/shops/?ordering=-recommended
            http://192.168.2.134:8000/rest/api/shops/?ordering=dianping_id,recommended
            默认排序字段：-recommended
    更新： PUT  \n
    删除： DELETE
    """
    queryset = Shop.objects.all()
    serializer_class = ShopSerializer
    filter_fields = ('dianping_business_id', 'id')

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('dianping_business_id', 'id')

    ordering_fields = ('dianping_business_id', 'id')
    ordering = ('id',)


class ShopCategoryViewSet(viewsets.ModelViewSet):
    """
    商铺分类基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： name， parent_id
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/shopcategories/?name=亲子
        查询：
            字段: name
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/shopcategories/?name=亲子
        排序：
            字段：name
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/shopcategories/?ordering=name
            http://192.168.2.134:8000/rest/api/shopcategories/?ordering=-name
            http://192.168.2.134:8000/rest/api/shopcategories/?ordering=name
            默认排序字段：name
    更新： PUT  \n
    删除： DELETE
    """
    queryset = ShopCategory.objects.all()
    serializer_class = ShopCategorySerializer
    filter_fields = ('name', 'parent_id', 'id')

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('name', 'id')

    ordering_fields = ('name', 'id')
    ordering = ('id',)


class ChinaViewSet(viewsets.ModelViewSet):
    """
    中国地区基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： name， parent_id
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/china/?name=北京&parent_id=1
        查询：
            字段: name
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/china/?name=北京
        排序：
            字段：name
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/china/?ordering=name
            http://192.168.2.134:8000/rest/api/china/?ordering=-name
            http://192.168.2.134:8000/rest/api/china/?ordering=name
            默认排序字段：name
    更新： PUT  \n
    删除： DELETE
    """
    queryset = China.objects.all()
    serializer_class = ChinaSerializer
    # permission_classes = (IsAuthenticatedByToken,)
    # from .permissions import IsAuthenticated
    # # authentication_classes = (BasicAuthentication, SessionAuthentication)
    # permission_classes = (IsAuthenticated,)
    filter_fields = ('name', 'parent_id')

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('name', 'id')

    ordering_fields = ('name', 'id')
    ordering = ('id',)


class LevelViewSet(viewsets.ModelViewSet):
    """
    级别基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： level
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/levels/?level=3
        查询：
            字段: level
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/levels/?search=1
        排序：
            字段：level
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/levels/?ordering=level
            默认排序字段：level
    更新： PUT  \n
    删除： DELETE
    """
    queryset = Level.objects.all()
    serializer_class = LevelSerializer
    filter_fields = ('level',)

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('level',)

    ordering_fields = ('level',)
    ordering = ('level',)


class ListFilter(Filter):
    def filter(self, qs, value):
        if not value:
            return qs

        self.lookup_type = 'in'
        values = value.split(',')
        return super(ListFilter, self).filter(qs, values)


class DeviceFilter(django_filters.FilterSet):
    min_used_time = django_filters.NumberFilter(name="used_time", lookup_type='gte')
    max_used_time = django_filters.NumberFilter(name="used_time", lookup_type='lte')
    sequences = ListFilter(name="sequence")

    class Meta:
        model = Device
        fields = [
            'nickname',
            'sequence',
            'type',
            'brand',
            'used_time',
            'min_used_time',
            'max_used_time',
            'sequences']


class DeviceViewSet(viewsets.ModelViewSet):
    """
    设备基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： nickname, sequence, type, brand, used_time, min_used_time, max_used_time, sequences
            参数的名称是字段名称。 \n
            注意：sequence查询单个设备， sequences可以通过具体的sequence获取多个设备
            如：
            http://192.168.2.134:8000/rest/api/devices/?nickname=sangebaba
            http://192.168.2.134:8000/rest/api/devices/?sequences=1111111,Tv221u-045154D0
        查询：
            字段: sequence, nickname, nick, used_time
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/devices/?search=sangebaba
        排序：
            字段：nickname, sequence, version, type, brand, used_time
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/devices/?ordering=name
            http://192.168.2.134:8000/rest/api/devices/?ordering=-used_time
            http://192.168.2.134:8000/rest/api/devices/?ordering=name,nick
            默认排序字段：sequence
    更新： PUT  \n
    删除： DELETE
    """
    queryset = Device.objects.all()
    serializer_class = DeviceSerializer
    filter_class = DeviceFilter
    permission_classes = (AllowAny,)

    # versioning_class = AppVersion

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('sequence', 'nickname', 'used_time')

    ordering_fields = ('sequence', 'nickname', 'used_time', 'version', 'type', 'brand')
    ordering = ('sequence',)


class AddressViewSet(viewsets.ModelViewSet):
    """
    地址基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： longitude, latitude, detail_address
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/addresses/?longitude=100
        查询：
            字段: longitude, latitude, detail_address
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/addresses/?search=100
        排序：
            字段：longitude, latitude, detail_address
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/addresses/?ordering=longitude
            http://192.168.2.134:8000/rest/api/addresses/?ordering=-longitude
            http://192.168.2.134:8000/rest/api/addresses/?ordering=longitude,latitude,detail_address
            默认排序字段：longitude
    更新： PUT  \n
    删除： DELETE
    """
    queryset = Address.objects.all()
    serializer_class = AddressSerializer
    filter_fields = ('longitude', 'latitude', 'detail_address')

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('longitude', 'latitude', 'detail_address',)

    ordering_fields = ('longitude', 'latitude', 'detail_address',)
    ordering = ('longitude',)


class UserExtensionFilter(django_filters.FilterSet):
    username = django_filters.CharFilter(name="user__username")
    email = django_filters.CharFilter(name="user__email")
    min_level = django_filters.NumberFilter(name="level__level", lookup_type='gte')
    max_level = django_filters.NumberFilter(name="level__level", lookup_type='lte')

    min_child_birth = django_filters.DateTimeFilter(name="child_birth", lookup_type='gte')
    max_child_birth = django_filters.DateTimeFilter(name="child_birth", lookup_type='lte')
    min_account = django_filters.NumberFilter(name="account", lookup_type='gte')
    max_account = django_filters.NumberFilter(name="account", lookup_type='lte')
    min_score = django_filters.NumberFilter(name="score", lookup_type='gte')
    max_score = django_filters.NumberFilter(name="score", lookup_type='lte')

    class Meta:
        model = UserExtension
        fields = [
            'username', 'email',
            'phone', 'nickname', 'gender', 'has_child',
            'child_birth', 'account', 'score', 'level',
            'min_child_birth', 'max_child_birth',
            'min_account', 'max_account',
            'min_score', 'max_score',
            'min_level', 'max_level',
        ]


class UserExtensionViewSet(viewsets.ModelViewSet):
    """
    扩展用户的基本操作，由于用户在创建时已经在本表中创建了相应项，所以本表的创建项（POST）在此无用
    创建： 请使用User中的POST添加用户 \n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： username, email,phone, nickname, gender, has_child, child_birth, account, score, level,
            min_child_birth, max_child_birth, min_account, max_account, min_score, max_score, min_level, max_level,
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/userextensions/?username=admin
        查询：
            字段: username, email, nickname, phone, score, level
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/userextensions/?search=dmi
        排序：
            字段：nickname, phone, child_birth, account, score, has_child, gender
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/userextensions/?ordering=nickname
            http://192.168.2.134:8000/rest/api/userextensions/?ordering=-account
            http://192.168.2.134:8000/rest/api/userextensions/?ordering=nickname,phone,-has_child
            默认排序字段：-account
    更新： PUT  \n
    删除： DELETE
    """
    queryset = UserExtension.objects.all()
    serializer_class = UserExtensionSerializer
    filter_class = UserExtensionFilter

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('user__username', 'user__email', 'nickname', 'phone', 'score', 'level__level', 'id')

    ordering_fields = ('nickname', 'phone', 'child_birth', 'account', 'score', 'has_child', 'gender', 'user__id')
    ordering = ('-user__id',)


class PublishFilter(django_filters.FilterSet):
    min_PM2_5 = django_filters.NumberFilter(name="PM2_5", lookup_type='gte')
    max_PM2_5 = django_filters.NumberFilter(name="PM2_5", lookup_type='lte')
    min_checked_at = django_filters.DateTimeFilter(name="checked_at", lookup_type='gte')
    max_checked_at = django_filters.DateTimeFilter(name="checked_at", lookup_type='lte')
    min_formaldehyde = django_filters.NumberFilter(name="formaldehyde", lookup_type='gte')
    max_formaldehyde = django_filters.NumberFilter(name="formaldehyde", lookup_type='lte')
    min_temperature = django_filters.NumberFilter(name="temperature", lookup_type='gte')
    max_temperature = django_filters.NumberFilter(name="temperature", lookup_type='lte')
    min_humidity = django_filters.NumberFilter(name="humidity", lookup_type='gte')
    max_humidity = django_filters.NumberFilter(name="humidity", lookup_type='lte')
    min_noise = django_filters.NumberFilter(name="noise", lookup_type='gte')
    max_noise = django_filters.NumberFilter(name="noise", lookup_type='lte')
    min_crowd_level = django_filters.NumberFilter(name="crowd_level", lookup_type='gte')
    max_crowd_level = django_filters.NumberFilter(name="crowd_level", lookup_type='lte')
    min_quiet_level = django_filters.NumberFilter(name="quiet_level", lookup_type='gte')
    max_quiet_level = django_filters.NumberFilter(name="quiet_level", lookup_type='lte')
    min_food_level = django_filters.NumberFilter(name="food_level", lookup_type='gte')
    max_food_level = django_filters.NumberFilter(name="food_level", lookup_type='lte')

    longitude = django_filters.NumberFilter(name="address__longitude")
    latitude = django_filters.NumberFilter(name="address__latitude")
    detail_address = django_filters.NumberFilter(name="address__detail_address")
    sequence = django_filters.CharFilter(name="device__sequence")
    dianping_id = django_filters.NumberFilter(name="shop__dianping_id")
    username = django_filters.CharFilter(name="user__username")

    is_recommend = django_filters.NumberFilter(name="is_recommended")

    class Meta:
        model = Publish
        # fields = [
        #     'min_PM2_5', 'max_PM2_5',
        #     'min_formaldehyde', 'max_formaldehyde',
        #     'min_temperature', 'max_temperature',
        #     'min_humidity', 'max_humidity',
        #     'min_noise', 'max_noise',
        #     'PM2_5', 'formaldehyde', 'temperature', 'humidity',
        #     'content', 'has_WIFI', 'has_parking_charge',
        #     'has_monitor','noise',
        #     'longitude', 'latitude', 'detail_address',
        #     'dianping_id', 'username', 'sequence',
        # ]


class PublishViewSet(viewsets.ModelViewSet):
    """
    发布的基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： min_PM2_5, max_PM2_5, min_formaldehyde, max_formaldehyde, min_temperature, max_temperature,
                   min_humidity, max_humidity, min_noise, max_noise, PM2_5, formaldehyde, temperature, humidity,
                   content, has_WIFI, has_parking_charge, has_monitor, noise, longitude, latitude, detail_address,
                   dianping_id, username, sequence
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/publishes/?max_PM2_5=50
        查询：
            字段：  PM2_5, formaldehyde, temperature, humidity, content, noise, address__longitude,
                    address__latitude, address__detail_address, device__sequence, device__name, device__nick,
                    user__username, user__email, shop__dianping_id
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/publishes/?search=50
        排序：
            字段：  PM2_5, formaldehyde, temperature, humidity, content, noise, has_WIFI,
                    has_parking_charge,has_monitor, crowd_level, quiet_level, food_level
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/publishes/?ordering=PM2_5
            http://192.168.2.134:8000/rest/api/publishes/?ordering=-has_WIFI
            http://192.168.2.134:8000/rest/api/publishes/?ordering=PM2_5,temperature,-has_monitor
            默认排序字段：PM2_5
    更新： PUT  \n
    删除： DELETE
    """
    queryset = Publish.objects.all()
    serializer_class = PublishSerializer
    filter_class = PublishFilter
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = (
        'PM2_5', 'formaldehyde', 'temperature',
        'humidity', 'content', 'noise', 'shop__address__longitude',
        'shop__address__latitude', 'shop__address__detail_address',
        'device__sequence', 'device__nickname', 'id',
        'user__username', 'user__email', 'shop__dianping_business_id'
    )

    ordering_fields = (
        'PM2_5', 'formaldehyde', 'temperature',
        'humidity', 'content', 'noise', 'has_WIFI',
        'has_parking_charge', 'has_monitor', 'crowd_level',
        'quiet_level', 'food_level', 'id'
    )
    ordering = ('PM2_5',)


class CommentFilter(django_filters.FilterSet):
    publish_id = django_filters.NumberFilter(name="publish__id")
    username = django_filters.CharFilter(name="user__username")

    class Meta:
        model = Comment
        fields = [
            'content', 'id',
            'publish_id', 'username'
        ]


class CommentViewSet(viewsets.ModelViewSet):
    """
    评论的基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： content, attribute, comment_id, publish_id, username
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/comments/?attribute=1
        查询：
            字段: content, username, email
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/comments/?search=1
        排序：
            字段：content, attribute, created_at
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/comments/?ordering=created_at
            http://192.168.2.134:8000/rest/api/comments/?ordering=-attribute
            http://192.168.2.134:8000/rest/api/comments/?ordering=-attribute,created_at
            默认排序字段：-attribute
    更新： PUT  \n
    删除： DELETE
    """
    queryset = Comment.objects.all()
    serializer_class = CommentSerializer
    filter_class = CommentFilter

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('content', 'user__username', 'user__email', 'id')

    ordering_fields = ('content', 'created_at',)
    ordering = ('-created_at',)


class UserShopRelationsFilter(django_filters.FilterSet):
    dianping_name = django_filters.NumberFilter(name="shop__dianping_name")
    username = django_filters.CharFilter(name="user__username")

    class Meta:
        model = UserShopRelations
        fields = [
            'is_recommended',
            'dianping_name',
            'username',
        ]


class UserShopRelationsViewSet(viewsets.ModelViewSet):
    """
    用户与商铺的关系基本操作
    创建： POST （具体字段参见下面的POST）\n
    查找： GET  （包括过滤，查询和排序）\n
        过滤：
            字段： is_recommended, dianping_name, username
            参数的名称是字段名称。
            如：
            http://192.168.2.134:8000/rest/api/comments/?is_commended=1
        查询：
            字段: is_recommended, shop__dianping_name, user__username
            参数的名称只能是search，只要查找的数据在字段数据中存在就会查到。
            如：
            http://192.168.2.134:8000/rest/api/comments/?search=1
        排序：
            字段：is_recommended
            参数的名称只能是ordering，参数的值是上面的某些字段。
            如：
            http://192.168.2.134:8000/rest/api/publishoperations/?ordering=is_recommended
            http://192.168.2.134:8000/rest/api/publishoperations/?ordering=-is_recommended
            默认排序字段：-is_recommended
    更新： PUT  \n
    删除： DELETE
    """
    queryset = UserShopRelations.objects.all()
    serializer_class = UserShopRelationsSerializer
    filter_class = UserShopRelationsFilter

    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    search_fields = ('is_recommended', 'user__username', 'shop__dianping_name',)

    ordering_fields = ('is_recommended',)
    ordering = ('-is_recommended',)


class UserPublishRelationsViewSet(viewsets.ModelViewSet):
    queryset = UserPublishRelations.objects.all()
    serializer_class = UserPublishRelationsSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ForumCategoryViewSet(viewsets.ModelViewSet):
    queryset = ForumCategory.objects.all()
    serializer_class = ForumCategorySerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ForumPostViewSet(viewsets.ModelViewSet):
    queryset = ForumPost.objects.all()
    serializer_class = ForumPostSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('content', 'id', "category__id", "title", "owner__id")
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ForumReplyViewSet(viewsets.ModelViewSet):
    queryset = ForumReply.objects.all()
    serializer_class = ForumReplySerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('content', 'id', "post_id")
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ForumPostSourceViewSet(viewsets.ModelViewSet):
    queryset = ForumPost.objects.all()
    serializer_class = ForumPostSourceSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('content', 'id', "category__id", "title", "owner__id")
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ForumReplySourceViewSet(viewsets.ModelViewSet):
    queryset = ForumReply.objects.all()
    serializer_class = ForumReplySourceSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('content', 'id', "post_id")
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ForumCategoryCarouselViewSet(viewsets.ModelViewSet):
    queryset = ForumCategoryCarousel.objects.all()
    serializer_class = ForumCategoryCarouselSerializer
    ordering_fields = ('-id',)
    ordering = ('-id',)


class CouponViewSet(viewsets.ModelViewSet):
    queryset = Coupon.objects.all()
    serializer_class = CouponSerializer
    filter_fields = ('sequence', 'youzan_sequence', 'id')
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ChannelViewSet(viewsets.ModelViewSet):
    queryset = Channel.objects.all()
    serializer_class = ChannelSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('name', 'audit', 'shortcut')
    ordering_fields = ('-id',)
    ordering = ('-id',)


class HomePageCarouselViewSet(viewsets.ModelViewSet):
    queryset = HomePageCarousel.objects.all()
    serializer_class = HomePageCarouselSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('name',)
    ordering_fields = ('-id',)
    ordering = ('-id',)


class UseViewSet(viewsets.ModelViewSet):
    queryset = Use.objects.all()
    serializer_class = UseSerializer
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('id',)
    ordering_fields = ('-id',)
    ordering = ('-id',)


class DetectorViewSet(viewsets.ModelViewSet):
    queryset = Detector.objects.all()
    serializer_class = DetectorSerializer
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('mac_address',)
    ordering_fields = ('-id',)
    ordering = ('-id',)


class GameViewSet(viewsets.ModelViewSet):
    queryset = Game.objects.all()
    serializer_class = GameSerializer
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('score', 'user_id')
    ordering_fields = ('-id',)
    ordering = ('-id',)


class CompanyViewSet(viewsets.ModelViewSet):
    queryset = Company.objects.all()
    serializer_class = CompanySerializer
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('id', 'name')
    ordering_fields = ('-id',)
    ordering = ('-id',)


class ProductViewSet(viewsets.ModelViewSet):
    queryset = Product.objects.all()
    serializer_class = ProductSerializer
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('id', 'name')
    ordering_fields = ('-id',)
    ordering = ('-id',)


class PopWindowViewSet(viewsets.ModelViewSet):
    queryset = PopWindow.objects.all()
    serializer_class = PopWindowSerializer
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter,)
    filter_fields = ('id', 'name')
    ordering_fields = ('-id',)
    ordering = ('-id',)


class DetectorRelationViewSet(viewsets.ModelViewSet):
    queryset = DetectorRelation.objects.all()
    serializer_class = DetectorRelationSerializer
    filter_fields = ('mac_address', 'user_id', 'address')
    search_fields = ('mac_address', 'user_id', 'address')
    ordering_fields = ('-id',)
    ordering = ('-id',)
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter)


class PushShopViewSet(viewsets.ModelViewSet):
    queryset = PushShop.objects.all()
    serializer_class = PushShopSerializer
    filter_fields = ('name', 'id')
    search_fields = ('name', 'id')
    ordering_fields = ('-id',)
    ordering = ('-id',)
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter)


class ShareStatisticsViewSet(viewsets.ModelViewSet):
    queryset = ShareStatistics.objects.all()
    serializer_class = ShareStatisticsSerializer
    filter_fields = ('user_id', 'behavior_name', 'id')
    search_fields = ('user_id', 'behavior_name', 'id')
    ordering_fields = ('-id',)
    ordering = ('-id',)
    permission_classes = (AllowAny,)
    filter_backends = (filters.DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter)


# 短信********************************************************


# 用户是否存在
def user_exist_dict(username):
    result = dict()
    user = Users.get_user(username)
    if user is not None:
        result["success"] = True
        result["info"] = "phone does exist"
    else:
        result["success"] = False
        result["info"] = "phone does not exist, please register firstly"
    return result


# 对外接口 -- 用户是否存在
@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def user_exist(request):
    result = dict()
    username = request.data.get("phone", None)
    if not username:
        result["success"] = False
        result["info"] = "parameter(phone) does not exist"
    else:
        result = user_exist_dict(username)
    return Response(result)


def send_message_dict(phone, length=4):
    result = dict()
    key = phone
    session_store = SessionStore()
    verification_code = Messages.generate_verification_code(length)
    session_store[key] = verification_code
    session_store.save()
    result["key"] = session_store.session_key
    result["verify_code"] = verification_code
    try:
        # data是有顺序的，所以只能用列表或元组，不能用集合
        data = [verification_code, messages.SMS_INTERVAL]
        temp = Messages.send_message_sms(phone, data, messages.SMS_TEMPLATE_VERIFICATION_ID)
        result["success"] = temp["success"]
        result["status_code"] = temp["status_code"]
        result["status_message"] = temp["status_message"]
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["success"] = False
        result["info"] = ex.message
    return result


# 对外接口 -- 发送验证码信息
@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def send_message(request):
    result = dict()
    phone = request.data.get("phone", None)
    length = int(request.data.get("length", '4'))
    if not phone:
        result["success"] = False
        result["info"] = "please add phone parameter"
        result["key"] = None
        result["verify_code"] = None
    else:
        result = send_message_dict(phone, length)
    return Response(result)


def verify_code_dict(verification_code, session_key, key):
    result = {}

    if Messages.check_verification_code(verification_code, session_key, key):
        result["success"] = True
        result["info"] = "the verify code is correct"
    else:
        result["success"] = False
        result["info"] = "the verify code is not correct"
    return result


# 对外接口 -- 检查验证码是否正确
@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def check_code(request):
    result = dict()
    session_key = request.data.get("key", None)
    key = request.data.get("phone", None)
    verification_code = request.data.get("code", None)

    if not session_key:
        result["success"] = False
        result["info"] = "session key is None"
        return Response(result)
    if not key:
        result["success"] = False
        result["info"] = "phone is None"
        return Response(result)
    if not verification_code:
        result["success"] = False
        result["info"] = "verification_code is None"
        return Response(result)

    result = verify_code_dict(verification_code, session_key, key)
    return Response(result)


# 短信********************************************************
# 接口***********************************************************


def add_user_json(user_dict):
    result = {}
    try:
        result = Users.add(user_dict)
    except Exception as ex:
        result["success"] = False
        result["info"] = ex.message
    return result


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def register_json(request):
    username = request.data["phone"]
    phone = request.data["phone"]
    nickname = request.data["nickname"]
    password = request.data["password"]
    user_dict = dict()
    user_dict["username"] = username
    user_dict["phone"] = phone
    user_dict["nickname"] = nickname
    user_dict["password"] = password

    result = add_user_json(user_dict)
    return Response(result)


def reset_password_json(username, password):
    result = {}
    try:
        user = Users.get_user(username)
        if not user:
            result["success"] = False
            result["info"] = "the user does not exist"
            return result
        user.set_password(password)
        user.save()
        result["success"] = True
        result["info"] = "change password successfully"
    except Exception as ex:
        result["success"] = False
        result["info"] = ex.message
    return result


@api_view(http_method_names=["POST"])
@csrf_exempt
def reset_password_action(request):
    result = dict()
    try:
        username = request.data.get("phone", None)
        password = request.data.get("password", None)
        if not username or not password:
            result["success"] = False
            result["info"] = "phone or password is null"
        else:
            result = reset_password_json(username, password)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["success"] = False
        result["info"] = ex.message
    return Response(result)


def get_basic_user_info_dict(phone, key):
    result = dict()
    result["userinfo"] = dict()

    try:
        user_id_from_key = Tokens.get(key).user_id
        username = phone
        user = Users.get_user(username)

        if user_id_from_key == user.id:
            pass
        else:
            result["userinfo"]['id'] = 0
            result["userinfo"]['username'] = None
            result["userinfo"]["nickname"] = None
            result["success"] = False
            result["info"] = "phone is not correct!"
            return result

        result["userinfo"]['id'] = user.id
        result["userinfo"]['username'] = phone
        result["userinfo"]["nickname"] = user.user_extension.nickname
        result["success"] = True
        result["info"] = "success"
    except Exception as ex:
        result["userinfo"]['id'] = 0
        result["userinfo"]['username'] = None
        result["userinfo"]["nickname"] = None
        result["success"] = False
        result["info"] = ex.message
    return result


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def get_basic_user_info(request):
    result = dict()
    phone = request.data.get('phone', None)
    key = request.data.get("key", None)
    result["userinfo"] = dict()

    if not key:
        result["userinfo"]['id'] = 0
        result["userinfo"]['username'] = None
        result["userinfo"]["nickname"] = None
        result["success"] = False
        result["info"] = "parameter key is required!"
        return result

    if not phone:
        result["userinfo"]['id'] = 0
        result["userinfo"]['username'] = None
        result["userinfo"]["nickname"] = None
        result["success"] = False
        result["info"] = "parameter phone is required!"
        return result

    result = get_basic_user_info_dict(phone, key)

    return Response(result)


@api_view(http_method_names=["POST"])
def logout_action(request):
    result = dict()
    user_id = request.data.get('user_id', 0)

    try:
        user = User.objects.get(id=user_id)
        request.user = user
        request.user.auth_token.delete()
        auth.logout(request)
        result["success"] = True
        result["info"] = "logout successfully"
    except Exception as ex:
        result["success"] = False
        result["info"] = ex.message
    return Response(result)


def get_address_id(longitude, latitude, detail_address):
    result = 0
    start_longitude = Digit.get_floor_float(float(longitude))
    start_latitude = Digit.get_floor_float(float(latitude))

    params = dict()
    params["longitude"] = start_longitude
    params["latitude"] = start_latitude
    params["detail_address"] = detail_address

    the_addresses = Addresses.get_addresses(params)

    if addresses:
        result = the_addresses[0].id
    return result


def is_address_id_valid(address_id):
    try:
        the_address = Addresses.get_address(address_id)
        if the_address is not None:
            return True
        else:
            return False
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return False


def get_address_id_json(params):
    address_id = params.get("address_id", 0)
    if address_id:
        if is_address_id_valid(address_id):
            return address_id
        else:
            return 0

    longitude = params.get("longitude", 0)
    latitude = params.get("latitude", 0)
    detail_address = params.get("detail_address", None)
    if not longitude or not latitude:
        return 0

    address_id = get_address_id(longitude, latitude, detail_address)
    if address_id:
        return address_id

    the_address = Address(longitude=longitude, latitude=latitude, detail_address=detail_address)
    the_address.save()
    address_id = get_address_id(longitude, latitude, detail_address)
    if address_id:
        return address_id
    else:
        return 0


def is_category_id_valid(category_id):
    category = ShopCategories.get_category(category_id)
    if category:
        return True
    else:
        return False


def get_shop_id(name, address_id, category_id):
    data = dict()
    shops = Shop.objects.filter(name=name, address_id=address_id, category_id=category_id)
    if len(shops) == 1:
        data["success"] = True
        data["id"] = shops[0].id
        data["info"] = "get shop id successfully"
    elif len(shops) > 1:
        data["success"] = False
        data["id"] = shops[0].id
        data["info"] = "there is more than one such shop"
    else:
        data["success"] = False
        data["id"] = 0
        data["info"] = "there is no shop"
    return data


def get_shop_id_dict(request):
    data = dict()
    city = request.GET.get("city", None)
    name = request.GET.get("name", None)
    category_id = request.GET.get("category_id", 0)

    params = dict()
    params["address_id"] = request.GET.get("address_id", 0)
    params["longitude"] = request.GET.get("longitude", 0)
    params["latitude"] = request.GET.get("latitude", 0)
    params["detail_address"] = request.GET.get("detail_address", 0)

    address_id = get_address_id_json(params)
    if not address_id:
        data["success"] = False
        data["info"] = "no valid address"
        data["id"] = 0
        return data

    if not is_category_id_valid(category_id):
        category_id = 0

    if not name or not address_id or not category_id:
        data["success"] = False
        data["info"] = "name, address_id and category_id are necessary"
        data["id"] = 0
        return data

    result = get_shop_id(name, address_id, category_id)

    if result["success"]:
        data["success"] = True
        data["info"] = "shop has been added"
        data["id"] = result["id"]
        return data
    else:
        if result["id"] > 0:
            data["success"] = False
            data["info"] = "there is more than one such shop"
            data["id"] = 0
            return data

    address_objects = Address.objects.filter(id=address_id)
    address_object = None
    if address_objects.count() > 0:
        address_object = address_objects[0]
    else:
        data["success"] = False
        data["info"] = "there is no such address"
        data["id"] = 0
        return data

    the_shop = Shop(
        name=name,
        address_id=address_id,
        category_id=category_id,
        dianping_city=city,
        dianping_name=name,
        dianping_longitude=address_object.longitude,
        dianping_latitude=address_object.latitude
    )
    the_shop.save()

    result = get_shop_id(name, address_id, category_id)
    data["success"] = result["success"]

    if result["success"]:
        data["id"] = result["id"]
        data["info"] = "add shop successfully"
    else:
        data["id"] = 0
        data["info"] = "failed to add shop"
    return data


@api_view(http_method_names=["GET"])
def get_shop_id_json(request):
    return Response(get_shop_id_dict(request))


# ***********************************获取发布常量信息*********************************************

@api_view(http_method_names=["GET"])
def get_publish_constant_all(request):
    return Response(PublishCategories.get_all())


# ***********************************获取发布常量信息*********************************************
# **********************************dianping API******************************************


def get_value(kwargs, name, default=None):
    try:
        return kwargs.GET.get(name, default)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return None


def add_value(parameter_set, name, kwargs, default=None):
    value = get_value(kwargs, name, default=default)
    if type(value) == "int":
        value = str(value)
    if type(value) == "str":
        value = value.encode("utf8")
    if value:
        parameter_set.append((name, value))


def get_extreme_value(kwargs, name, min_value, max_value, default=0):
    value = get_value(kwargs, name)
    if not value:
        return default
    if int(value) > int(max_value):
        return max_value
    if int(value) < int(min_value):
        return min_value


def set_business_parameter(kwargs):
    parameter_set = []
    add_value(parameter_set, dianpings.PARAMETER_FORMAT, kwargs, default="json")
    add_value(parameter_set, dianpings.PARAMETER_CATEGORY, kwargs)
    add_value(parameter_set, dianpings.PARAMETER_CITY, kwargs)
    add_value(parameter_set, dianpings.PARAMETER_HAS_COUPON, kwargs)
    add_value(parameter_set, dianpings.PARAMETER_HAS_DEAL, kwargs)
    add_value(parameter_set, dianpings.PARAMETER_KEYWORD, kwargs)
    add_value(parameter_set, dianpings.PARAMETER_LATITUDE, kwargs)
    add_value(parameter_set, dianpings.PARAMETER_LONGITUDE, kwargs)
    extreme_value = get_extreme_value(kwargs, dianpings.PARAMETER_LIMIT, 1, 40, 20)
    add_value(parameter_set, dianpings.PARAMETER_LIMIT, kwargs, default=extreme_value)
    extreme_value = get_extreme_value(kwargs, dianpings.PARAMETER_RADIUS, 1, 5000, 1000)
    add_value(parameter_set, dianpings.PARAMETER_RADIUS, kwargs, default=extreme_value)
    extreme_value = get_extreme_value(kwargs, dianpings.PARAMETER_OFFSET_TYPE, 1, 2, 1)
    add_value(parameter_set, dianpings.PARAMETER_OFFSET_TYPE, kwargs, default=extreme_value)
    add_value(parameter_set, dianpings.PARAMETER_REGION, kwargs)
    extreme_value = get_extreme_value(kwargs, dianpings.PARAMETER_SORT, 1, 9, 1)
    add_value(parameter_set, dianpings.PARAMETER_SORT, kwargs, default=extreme_value)

    return parameter_set


def set_deal_parameter(kwargs):
    parameter_set = list()
    parameter_set.append(("city", kwargs["city"]))
    parameter_set.append(("business_id", kwargs["business_id"]))
    return parameter_set


def get_codec(parameter_set):
    # 参数排序与拼接
    parameter_map = {}
    for pair in parameter_set:
        parameter_map[pair[0]] = pair[1]

    codec = dianpings.DIANPING_APP_KEY
    for key in sorted(parameter_map.iterkeys()):
        # print key
        if isinstance(parameter_map[key], unicode):
            codec += key + str(parameter_map[key].encode("utf8"))
        else:
            codec += key + str(parameter_map[key])

    codec += dianpings.DIANPING_SECRET
    return codec


def get_sign(codec):
    return (hashlib.sha1(codec).hexdigest()).upper()


def get_url(sign, parameter_set, source_url=dianpings.DIANPING_API_URL):
    # 拼接访问的URL
    url_trail = "appkey=" + dianpings.DIANPING_APP_KEY + "&sign=" + sign
    for pair in parameter_set:
        if isinstance(pair[1], unicode):
            url_trail += "&" + pair[0] + "=" + str(pair[1].encode("utf8"))
        else:
            url_trail += "&" + pair[0] + "=" + str(pair[1])

    request_url = source_url + "?" + url_trail
    return request_url


def get_info_from_dianping(parameter_set, source_url=dianpings.DIANPING_API_URL, data_key="businesses"):
    # 示例参数
    # parameter_set = set_parameter(kwargs)
    # 参数排序与拼接
    codec = get_codec(parameter_set)
    # 签名计算
    sign = get_sign(codec)
    # 拼接访问的URL
    request_url = get_url(sign, parameter_set, source_url)
    # 模拟请求
    response = urllib.urlopen(request_url)
    # print request_url
    content = response.read()
    # print content
    businesses = json.loads(content).get(data_key, None)
    # Logs.print_log("businesses", businesses)
    if businesses:
        return businesses
    else:
        return []


def has_dianping_shop(dianping_business_id):
    shops = Shop.objects.filter(dianping_business_id=dianping_business_id)
    if shops.exists():
        return True
    else:
        return False


@api_view(http_method_names=["GET"])
def show_deal_info(request):
    parameters = dict()
    parameters["city"] = request.GET.get("city", None)
    parameters["business_id"] = request.GET.get("business_id", 0)
    parameter_deal_set = DianPings.set_deal_parameter(parameters)
    deal_info = DianPings.get_info_from_dianping(
        parameter_deal_set,
        source_url=dianpings.DIANPING_API_DEALS_URL,
        data_key="deals")
    return Response(deal_info)


def add_one_shop(kwargs):
    data = {}
    business_id = str(kwargs.get("business_id", "business_id"))
    now = datetime.datetime.now()
    base_url_4_shop_img = "shop/" + str(now.year) + "/" + str(now.month) + "/" + str(now.day)
    try:
        rating_img_url = base_url_4_shop_img + "/rating_img_url_" \
                         + kwargs.get("rating_img_url", None).split("/")[-1]
        rating_s_img_url = base_url_4_shop_img + "/rating_s_img_url_" \
                           + kwargs.get("rating_s_img_url", None).split("/")[-1]
        photo_url = base_url_4_shop_img + "/photo_url_" \
                    + kwargs.get("photo_url", None).split("/")[-1]
        s_photo_url = base_url_4_shop_img + "/s_photo_url_" \
                      + kwargs.get("s_photo_url", None).split("/")[-1]
        categories = kwargs.get("categories", "")
        # Logs.print_log("categories", categories)
        category_id = 1
        try:
            category_id = ShopCategory.objects.get(name=shop_categories.DEFAULT_CATEGORY_NAME).id
            if categories:
                category_id = ShopCategory.objects.get(name=categories[-1]).id
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

        dianping_business_id = kwargs.get("business_id", 0)
        if dianping_business_id:
            s = Shop.objects.filter(dianping_business_id=dianping_business_id)
            if s.count() > 0:
                data["success"] = False
                data["info"] = "shop has been added!"
                return data
        dianping_deals = kwargs.get("deals", "")
        if dianping_deals:
            dianping_deals_url = dianping_deals[0].get("url", "")
            dianping_deals_id = dianping_deals[0].get("id", "")
            dianping_deals_description = dianping_deals[0].get("description", "")
        else:
            dianping_deals_url = ""
            dianping_deals_id = ""
            dianping_deals_description = ""

        the_shop = Shop(dianping_business_id=kwargs.get("business_id", None),
                        dianping_name=kwargs.get("name", ""),

                        dianping_categories=','.join(kwargs.get("categories", "")),
                        dianping_address=kwargs.get("address", ""),
                        dianping_city=kwargs.get("city", ""),
                        dianping_telephone=kwargs.get("telephone", ""),
                        dianping_avg_price=kwargs.get("avg_price", None),
                        dianping_regions=kwargs.get("regions", ""),
                        dianping_avg_rating=kwargs.get("avg_rating", None),
                        dianping_longitude=kwargs.get("longitude", None),
                        dianping_latitude=kwargs.get("latitude", None),
                        dianping_business_url=kwargs.get("business_url", ""),
                        dianping_coupon_description=kwargs.get("coupon_description", ""),
                        dianping_coupon_id=kwargs.get("coupon_id", 0),
                        dianping_coupon_url=kwargs.get("coupon_url", ""),
                        dianping_deal_count=kwargs.get("deal_count", 0),
                        dianping_deals=kwargs.get("deals", ""),
                        dianping_has_coupon=kwargs.get("has_coupon", 0),
                        dianping_has_deal=kwargs.get("has_deal", 0),
                        dianping_has_online_reservation=kwargs.get("has_online_reservation", ""),
                        dianping_online_reservation_url=kwargs.get("online_reservation_url", ""),
                        dianping_photo_url=kwargs.get("photo_url", ""),
                        dianping_rating_img_url=kwargs.get("rating_img_url", ""),
                        dianping_rating_s_img_url=kwargs.get("rating_s_img_url", ""),
                        dianping_s_photo_url=kwargs.get("s_photo_url", ""),
                        dianping_deals_description=dianping_deals_description,
                        dianping_deals_id=dianping_deals_id,
                        dianping_deals_url=dianping_deals_url,

                        name=kwargs.get("name", ""),
                        category_id=category_id,

                        rating_img_url=rating_img_url,
                        rating_s_img_url=rating_s_img_url,
                        photo_url=photo_url,
                        s_photo_url=s_photo_url,
                        )

        image_save(kwargs.get("photo_url", None), "photo_url", business_id, "shop")
        image_save(kwargs.get("s_photo_url", None), "s_photo_url", business_id, "shop")
        image_save(kwargs.get("rating_img_url", None), "rating_img_url", business_id, "shop")
        image_save(kwargs.get("rating_s_img_url", None), "rating_s_img_url", business_id, "shop")
        the_shop.save()
        data["success"] = True
    except Exception as ex:
        data["success"] = False
        data["info"] = ex.message
        Logs.print_current_function_name_and_line_number(ex)
    return data


def update_one_shop(request):
    data = {}
    try:
        business_id = request.get("business_id", None)
        if not business_id:
            data["success"] = False
            data["info"] = "business id is null"
            return data
        shop = Shop.objects.get(dianping_business_id=business_id)
        if request.get("name", None):
            shop.dianping_name = request.get("name", None)
        if request.get("address", None):
            shop.dianping_address = request.get("address", None)
        if request.get("categories", None):
            shop.dianping_categories = ','.join(request.get("categories", None))
        if request.get("city", None):
            shop.dianping_city = request.get("city", None)
        if request.get("telephone", None):
            shop.dianping_telephone = request.get("telephone", None)
        if request.get("avg_price", None):
            shop.dianping_avg_price = request.get("avg_price", None)
        if request.get("regions", None):
            shop.dianping_regions = request.get("regions", None)
        if request.get("avg_rating", None):
            shop.dianping_avg_rating = request.get("avg_rating", None)
        if request.get("longitude", 0):
            shop.dianping_longitude = request.get("longitude", 0)
        if request.get("latitude", 0):
            shop.dianping_latitude = request.get("latitude", 0)
        if request.get("business_url", None):
            shop.dianping_business_url = request.get("business_url", None)
        if request.get("coupon_description", None):
            shop.dianping_coupon_description = request.get("coupon_description", None)
        if request.get("coupon_id", 0):
            shop.dianping_coupon_id = request.get("coupon_id", 0)
        if request.get("coupon_url", None):
            shop.dianping_coupon_url = request.get("coupon_url", None)
        if request.get("deal_count", 0):
            shop.dianping_deal_count = request.get("deal_count", 0)
        if request.get("deals", None):
            shop.dianping_deals = request.get("deals", None)
        if request.get("has_coupon", 0):
            shop.dianping_has_coupon = request.get("has_coupon", 0)
        if request.get("has_deal", 0):
            shop.dianping_has_deal = request.get("has_deal", 0)
        if request.get("has_online_reservation", None):
            shop.dianping_has_online_reservation = request.get("has_online_reservation", None)
        if request.get("online_reservation_url", None):
            shop.dianping_online_reservation_url = request.get("online_reservation_url", None)
        if request.get("photo_url", None):
            shop.dianping_photo_url = request.get("photo_url", None)
            image_save(request.get("photo_url", None), "photo_url", business_id)
        if request.get("rating_img_url", None):
            shop.dianping_rating_img_url = request.get("rating_img_url", None)
            image_save(request.get("rating_img_url", None), "rating_img_url", business_id)
        if request.get("rating_s_img_url", None):
            shop.dianping_rating_s_img_url = request.get("rating_s_img_url", None)
            image_save(request.get("rating_s_img_url", None), "rating_s_img_url", business_id)
        if request.get("s_photo_url", None):
            shop.dianping_s_photo_url = request.get("s_photo_url", None)
            image_save(request.get("s_photo_url", None), "s_photo_url", business_id)
        if request.get("deals.description", None):
            shop.dianping_deals_description = request.get("deals.description", None)
        if request.get("deals.id", None):
            shop.dianping_deals_id = request.get("deals.id", None)
        if request.get("deals.url", None):
            shop.dianping_deals_url = request.get("deals.url", None)

        shop.save()
        data["success"] = True
        data["info"] = "update shop" + str(business_id) + " successfully"
    except Exception as ex:
        data["success"] = False
        data["info"] = ex.message
    return data


def get_delta_days(old_date):
    delta_time = Datetimes.get_delta_time(old_date)
    return delta_time.days


def add_shop_by_dict(kwargs):
    # Logs.print_log("add_shop_by_dict", "start")
    parameter_business_set = set_business_parameter(kwargs)
    shop_info = get_info_from_dianping(parameter_business_set)
    data = dict()
    for shop in shop_info:
        if has_dianping_shop(int(shop["business_id"])):
            yesterday = Datetimes.get_some_day(1)
            yesterday_string = str(yesterday)
            shop_in_db = Shop.objects.filter(dianping_business_id=shop["business_id"])
            if shop_in_db.count() > 1:
                data[shop["business_id"]] = {"success": False,
                                             "info": "there is more than one shop, this is not permitted", "id": 0}
                continue
            shop_in_db_id = shop_in_db[0].id
            changed_at = shop_in_db.values()[0]["changed_at"]
            changed_at_local = Datetimes.transfer_datetime(changed_at)

            if not Datetimes.is_out_of_date(changed_at):
                data[shop["business_id"]] = {"success": False, "info": "data is still valid", "id": shop_in_db_id}
            else:
                ret_data = update_one_shop(shop)
                ret_data["id"] = shop_in_db_id
                data[shop["business_id"]] = ret_data
        else:
            ret_data = add_one_shop(shop)
            try:
                ret_data["id"] = Shop.objects.get(dianping_business_id=shop["business_id"]).id
                data[shop["business_id"]] = ret_data
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)
    # Logs.print_log("add_shop_by_dict", "end")
    return data


def get_shop_list(request):
    data = {}
    business_list = []
    parameter_set = set_business_parameter(request)
    businesses = get_info_from_dianping(parameter_set)
    data["success"] = True
    data["info"] = "get shop info successfully"
    for business in businesses:
        try:
            shop = Shop.objects.get(dianping_business_id=business["business_id"])
            business_dict = dict()
            business_dict["id"] = shop.id
            business_dict["address"] = shop.dianping_city + shop.dianping_address
            business_dict["name"] = shop.dianping_name
            category_name = shop.dianping_categories.split(',')[-1]
            category = {"name": category_name}
            category_data = get_shop_category(category)
            business_dict["category"] = category_data["name"]
            business_dict["category_info"] = category_data["info"]
            business_list.append(business_dict)
        except Exception as ex:
            data["success"] = False
            data["info"] = "get shop exception:" + ex.message
            data["category"] = None

    data["shop"] = business_list
    return data


def get_shop_category(category):
    data = {}
    try:
        name = category.get("name", None)
        shop_category_id = category.get("id", 0)
        cn = None
        if not name and not id:
            data["name"] = shop_categories.DEFAULT_CATEGORY_NAME
            data["success"] = False
            data["info"] = "parameters are not correct"
            return data
        elif name:
            cn = ShopCategory.objects.get(name=name)
        elif id:
            cn = ShopCategory.objects.get(id=shop_category_id)
        if cn.parent_id == 1:
            data["name"] = cn.name
            data["success"] = True
            data["info"] = "just the category"
        elif cn.parent_id > 1:
            cn_parent = ShopCategory.objects.get(id=cn.parent_id)
            if cn_parent.parent_id == 1:
                data["name"] = cn_parent.name
                data["success"] = True
                data["info"] = "the parent category is what we need"
            else:
                data["success"] = False
                data["name"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
                data["info"] = "The category's deep level is more than 3, and this is not planned"
        else:
            data["success"] = False
            data["name"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
            data["info"] = "The category is in the top, and we could not get the subcategory"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        data["success"] = False
        data["name"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
        data["info"] = "There is no category named " + str(category)
    return data


@json_response
def get_shop_json(request):
    data = dict()
    shop_from_dianping = get_shop_list(request)
    shop_from_db = get_shop_info_not_from_dianping(request)

    data["success"] = shop_from_db["success"] and shop_from_dianping["success"]
    data["info"] = shop_from_dianping["info"] + " " + shop_from_db["info"]
    data["shop"] = shop_from_dianping["shop"] + shop_from_db["shop"]
    return data


@json_response
def add_business(request):
    # Logs.print_log("add_business", "start")
    return add_shop_by_dict(request)


@csrf_exempt
def find_business(request):
    import thread
    thread.start_new_thread(add_business, (request,))
    return get_shop_json(request)


def get_address_by_id(address_id):
    addresses = Address.objects.filter(id=address_id)
    if addresses.count() == 1:
        return addresses[0].detail_address
    else:
        return None


def get_shop_info_base_distance(shop, latitude, longitude, radius):
    data = dict()
    shop_list = list()
    if not shop:
        data["success"] = False
        data["info"] = "shop is None in database"
        data["shop"] = shop_list
        data["category"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
        return data
    for s in shop:
        distance = Shops.get_distance(float(latitude), float(longitude), float(s.address.latitude),
                                      float(s.address.longitude))
        s_dict = {}
        if distance <= radius:
            s_dict["id"] = s.id
            s_dict["address"] = get_address_by_id(s.address_id)
            s_dict["name"] = s.name
            category = {"id": s.category_id}
            category_data = get_shop_category(category)
            s_dict["category"] = category_data["name"]
            s_dict["category_info"] = category_data["info"]
            shop_list.append(s_dict)

    data["success"] = True
    data["info"] = "get shop not from dianping successfully"
    data["shop"] = shop_list
    return data


# 暂时未用，当大众点评不能用时，需要用这个函数代替 get_shop_list
def get_shop_info_from_local_dianping(request):
    data = {}
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    radius = int(request.GET.get("radius", 500))
    shop_list = []

    if not longitude or not latitude:
        data["success"] = False
        data["info"] = "longitude or latitude is None"
        data["shop"] = shop_list
        data["category"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
        return data
    shop = Shop.objects.exclude(dianping_business_id=0, audit=0)
    data = get_shop_info_base_distance(shop, float(latitude), float(longitude), int(radius))
    return data


def get_shop_info_not_from_dianping(request):
    data = {}
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    radius = int(request.GET.get("radius", 500))
    shop_list = []

    if not longitude or not latitude:
        data["success"] = False
        data["info"] = "longitude or latitude is None"
        data["shop"] = shop_list
        data["category"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
        return data
    shops = Shop.objects.filter(dianping_business_id=0, audit=1)
    data = get_shop_info_base_distance(shops, float(latitude), float(longitude), int(radius))
    return data


def last_modified(request):
    try:
        return Publish.objects.all().latest('created_at').created_at
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return datetime.datetime.now()


def get_etag(request):
    try:
        time_array = time.strptime(Datetimes.clock_to_string(Publish.objects.all().latest('created_at').created_at),
                                   datetimes.DATETIME_FORMAT)
        result = time.mktime(time_array)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result = time.time()

    return str(result)


# *************************************************************************discovery publish****************
# 发现接口
@execute_time
@api_view(http_method_names=["GET"])
def get_publish_infos_discovery(request):
    result = dict()
    start_id = int(request.GET.get("start_id", 0))
    count = int(request.GET.get("count", 40))
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    user_id = int(request.GET.get("id", 0))
    city = request.GET.get("city", None)
    publish_ids = request.GET.get("publish_ids", "")
    if publish_ids:
        loaded_publish_ids = publish_ids.split(",")
    else:
        loaded_publish_ids = []
    result["data"] = HomePage.get_discovery_publishes_info(user_id, start_id, count, longitude, latitude,
                                                           loaded_publish_ids, city)
    return Response(result)


# *************************************************************************discovery publish****************
# *************************************************************************nearby shop ****************
@execute_time
@api_view(http_method_names=["GET"])
def get_shop_infos_nearby(request):
    result = dict()
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    start_id = int(request.GET.get("start_id", 0))
    end_id = int(request.GET.get("end_id", 0))
    count = int(request.GET.get("count", 40))
    user_id = int(request.GET.get("id", 0))
    shop_ids = request.GET.get("shop_ids", "")
    loaded_shop_ids = shop_ids.split(",")
    result["data"] = HomePage.get_nearby_shops_info(user_id, start_id, count, longitude, latitude, loaded_shop_ids)

    return Response(result)


# *************************************************************************nearby shop****************
# *************************************************************************hot shop ****************


@execute_time
@api_view(http_method_names=["GET"])
def get_publish_infos_hot(request):
    start_id = int(request.GET.get("start_id", 0))
    count = int(request.GET.get("count", 40))
    latitude = float(request.GET.get("latitude", 0))
    longitude = float(request.GET.get("longitude", 0))
    user_id = int(request.GET.get("id", 0))
    city = request.GET.get("city", DEFAULT_CITY)
    city = HomePage.clear_city(city)
    shop_ids = request.GET.get("shop_ids", "")
    loaded_shop_ids = shop_ids.split(",")
    result = HomePage.get_hot_shops_info(city, user_id, start_id, count, longitude, latitude, loaded_shop_ids)
    return Response(result)


# *************************************************************************hot shop ****************


def get_device_sequence(user_id):
    try:
        devices = Device.objects.filter(user_id=user_id).order_by("-changed_at")
        if devices.count() > 0:
            return devices[0].sequence
        else:
            return None
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return None


def get_needed_ids(all_ids, start_id, end_id=0):
    data = {}
    start_index = -1
    end_index = 0

    try:
        if start_id == 0:
            start_index = 0
        else:
            start_index = all_ids.index(start_id) + 1
        if end_id > 0:
            end_index = all_ids.index(end_id)
            if end_index < start_index:
                end_index = 0
    except Exception as ex:
        data["info"] = ex.message
        data["id_list"] = []
    if end_index > 0:
        data["id_list"] = all_ids[start_index:end_index]
        data["info"] = "got from " + str(start_index) + " to " + str(end_index)
        data["next_id"] = end_id
    elif end_index == 0 and end_id > 0:
        data["id_list"] = []
        data["info"] = "no data"
        data["next_id"] = end_id
    else:
        data["id_list"] = all_ids[start_index:]
        data["info"] = "got from " + str(start_index) + " to end"
        data["next_id"] = -1
    # Logs.print_log("discovery data", data)

    return data


# 发现
# 根据最新发布的信息排序
def delete_redundant(ids):
    func = lambda x, y: x if y in x else x + [y]
    distinct_ids = reduce(func, [[], ] + ids)
    return distinct_ids


def get_objects_by_time(model_object, start_time, end_time):
    # Logs.print_log("start_time", start_time)
    # Logs.print_log("end_time", end_time)
    if start_time and end_time:
        objects = model_object.objects.filter(created_at__gte=start_time, created_at__lte=end_time)
    elif start_time and not end_time:
        objects = model_object.objects.filter(created_at__gte=start_time)
    elif not start_time and end_time:
        objects = model_object.objects.filter(created_at__lte=end_time)
    else:
        objects = model_object.objects.all()
    # object_names = [o.shop.id for o in objects]
    # # Logs.print_log("get_objects_by_time", object_names)
    return objects


@api_view(http_method_names=["GET"])
def get_publish_infos_user(request):
    result = dict()
    data = list()
    start_id = int(request.GET.get("start_id", 0))
    count = int(request.GET.get("count", 40))
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    user_id = int(request.GET.get("id", 0))
    publish_ids = request.GET.get("publish_ids", "")
    loaded_publish_ids = publish_ids.split(",")
    publishes = Publishes.get_publishes_by_user(user_id).order_by("-id")

    result["data"] = Discovery.get_the_publishes_info(
        publishes, user_id, start_id, count, longitude, latitude, loaded_publish_ids)
    return result


def get_category_info_by_shop(shop, user_id):
    result = []
    big_category = get_shop_big_category(shop)
    publishes_by_shop = Publishes.get_shown_publishes(user_id).filter(shop_id=shop.id)
    if big_category:
        all_operation = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_SHOW.get(str(big_category["id"]), {})
        # print all_operation
        for key in all_operation.keys():
            temp_dict = {}
            temp = publishes_by_shop.values(key).annotate(count=Count(key))
            if temp.count() == 0:
                return result
            temp_sort_list = list(temp)
            temp_sort_list.sort(key=operator.itemgetter('count'), reverse=True)
            needed_item = temp_sort_list[0]
            category_answer_key = needed_item[key]
            category_answer_chinese = PUBLISH_SUBJECTIVE_CONSTANT.ANSWERS_SHOW.get(key)[category_answer_key]
            needed_item[key] = category_answer_chinese
            temp_dict["key"] = key
            temp_dict["answer_description"] = category_answer_chinese
            temp_dict["count"] = needed_item["count"]
            result.append(temp_dict)

    return result


def get_deals_from_dianping(kwargs):
    parameter_deal_set = set_deal_parameter(kwargs)
    return get_info_from_dianping(parameter_deal_set, source_url=dianpings.DIANPING_API_DEALS_URL, data_key="deals")


def is_deal_in_db(deal_id):
    deal = ShopDeal.objects.filter(dianping_deals_id=deal_id)
    if deal.count() > 0:
        return True
    else:
        return False


def add_deals_to_db(kwargs):
    deal_info = get_deals_from_dianping(kwargs)
    result = []
    for deal in deal_info:
        # print deal
        image_name = "image_url_" + deal["image_url"].split("/")[-1]
        s_image_name = "s_image_url_" + deal["s_image_url"].split("/")[-1]
        deal_middle_path = "deal/" + deal["deal_id"]
        part_image_name = deal_middle_path + "/" + image_name
        part_s_image_name = deal_middle_path + "/" + s_image_name

        temp_dict = dict()
        temp_dict["business_id"] = deal["businesses"][0].get("id")
        temp_dict["city"] = deal.get("city")
        temp_dict["id"] = deal.get("deal_id")
        temp_dict["description"] = deal["description"]
        temp_dict["url"] = deal["deal_url"]
        temp_dict["list_price"] = deal["list_price"]
        temp_dict["current_price"] = deal["current_price"]
        temp_dict["purchase_count"] = deal["purchase_count"]
        temp_dict["is_refundable"] = deal["restrictions"]["is_refundable"]
        temp_dict["is_reservation_required"] = deal["restrictions"]["is_reservation_required"]
        temp_dict["title"] = deal["title"]
        temp_dict["s_image_url"] = files.BASE_URL_4_IMAGE + part_s_image_name
        temp_dict["image_url"] = files.BASE_URL_4_IMAGE + part_image_name
        result.append(temp_dict)

        if is_deal_in_db(deal["deal_id"]):
            # 存在就更新
            d = ShopDeal.objects.get(dianping_deals_id=deal["deal_id"])
            d.dianping_business_id = deal["businesses"][0].get("id")
            d.dianping_city = deal["city"]
            d.dianping_deals_description = deal["description"]
            d.dianping_deals_url = deal["deal_url"]
            d.dianping_deals_list_price = deal["list_price"]
            d.dianping_deals_current_price = deal["current_price"]
            d.dianping_deals_purchase_count = deal["purchase_count"]
            d.dianping_deals_is_refundable = deal["restrictions"]["is_refundable"]
            d.dianping_deals_is_reservation_required = deal["restrictions"]["is_reservation_required"]
            d.dianping_deals_title = deal["title"]
            d.dianping_deals_image_url = part_image_name
            d.dianping_deals_s_image_url = part_s_image_name
        else:
            # 不存在就添加
            d = ShopDeal(
                dianping_business_id=deal["businesses"][0].get("id"),
                dianping_city=deal["city"],
                dianping_deals_id=deal["deal_id"],
                dianping_deals_description=deal["description"],
                dianping_deals_url=deal["deal_url"],
                dianping_deals_list_price=deal["list_price"],
                dianping_deals_current_price=deal["current_price"],
                dianping_deals_purchase_count=deal["purchase_count"],
                dianping_deals_is_refundable=deal["restrictions"]["is_refundable"],
                dianping_deals_is_reservation_required=deal["restrictions"]["is_reservation_required"],
                dianping_deals_title=deal["title"],
                dianping_deals_image_url=part_image_name,
                dianping_deals_s_image_url=part_s_image_name
            )
        d.save()
        image_save(deal["image_url"], "image_url", deal_middle_path)
        image_save(deal["s_image_url"], "s_image_url", deal_middle_path)

    return result


def get_deals(kwargs):
    result = []
    business_id = kwargs.get("business_id", None)
    city = kwargs.get("city", None)
    if business_id and city:
        deals = ShopDeal.objects.filter(dianping_business_id=business_id, dianping_city=city)
        if deals.count() > 0:
            for deal in deals:
                # 只要有一个无效了，就直接从大众点评更新
                if Datetimes.is_out_of_date(deal.changed_at):
                    result = add_deals_to_db(kwargs)
                    break
                else:
                    # 从本地获取数据
                    temp_dict = dict()
                    temp_dict["business_id"] = deal.dianping_business_id
                    temp_dict["city"] = deal.dianping_city
                    temp_dict["id"] = deal.dianping_deals_id
                    temp_dict["description"] = deal.dianping_deals_description
                    temp_dict["url"] = deal.dianping_deals_url
                    temp_dict["list_price"] = deal.dianping_deals_list_price
                    temp_dict["current_price"] = deal.dianping_deals_current_price
                    temp_dict["purchase_count"] = deal.dianping_deals_purchase_count
                    temp_dict["is_refundable"] = deal.dianping_deals_is_refundable
                    temp_dict["is_reservation_required"] = deal.dianping_deals_is_reservation_required
                    temp_dict["title"] = deal.dianping_deals_title
                    temp_dict["image_url"] = files.BASE_URL_4_IMAGE + deal.dianping_deals_image_url
                    temp_dict["s_image_url"] = files.BASE_URL_4_IMAGE + deal.dianping_deals_s_image_url
                    result.append(temp_dict)
        else:
            result = add_deals_to_db(kwargs)
    return result


# 一家场所的所有发布信息
@api_view(http_method_names=["GET"])
def get_publishes_by_shop(request):
    result = dict()
    start_id = int(request.GET.get("start_id", 0))
    count = int(request.GET.get("count", 0))
    shop_id = int(request.GET.get("id", 0))
    user_id = int(request.GET.get("user_id", 0))
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    publish_ids = request.GET.get("publish_ids", "")
    loaded_publish_ids = publish_ids.split(",")

    publishes = Publishes.get_publishes_by_shop(shop_id)
    result["data"] = Discovery.get_the_publishes_info(
        publishes, user_id, start_id, count, longitude, latitude, loaded_publish_ids)
    return result


def set_interval(start_date, start_time, end_date, end_time):
    start_datetime = Datetimes.string_to_clock(start_date + " " + start_time)
    end_datetime = Datetimes.string_to_clock(end_date + " " + end_time)
    return [start_datetime, end_datetime]


def get_publish_id_recommend(city, user_id=0):
    interval = Datetimes.get_previous_interval()
    result_dict = {}

    result = []
    publish = Publishes.get_shown_publishes(user_id).filter(is_recommended=1)
    if publish and city:
        publish_city = publish.filter(shop__dianping_city__contains=city)
    else:
        publish_city = None
    if publish_city:
        result_dict["local"] = True
    else:
        publish_city = publish.filter(shop__dianping_city__contains=DEFAULT_CITY)
        result_dict["local"] = False
    publish = publish_city.order_by("-recommend_weight")
    for p in publish:
        result.append(p.id)
    result_dict["ids"] = result
    # print result
    return result_dict


def get_publish_id_not_recommend_by_score(city, user_id=0):
    result_dict = {}
    interval = Datetimes.get_previous_interval()
    result = []
    not_publish = Publishes.get_shown_publishes(user_id).filter(Q(created_at__gte=interval[0]),
                                                                Q(created_at__lte=interval[1]),
                                                                Q(is_recommended__isnull=True) | Q(is_recommended=0))
    if not_publish and city:
        not_publish_city = not_publish.filter(shop__dianping_city__exact=city)
    else:
        not_publish_city = None
    if not_publish_city:
        not_publish = not_publish_city
        result_dict["local"] = True
    else:
        result_dict["local"] = False
        not_publish = not_publish.filter(shop__dianping_city__exact=DEFAULT_CITY)

    for p in not_publish:
        temp = dict()
        score = 0
        if p.PM2_5:
            score += 5
        score += get_attribute_count(p.id)
        score += 3 * get_comment_count(p.id)
        temp["score"] = score
        temp["id"] = p.id
        result.append(temp)
    result.sort(lambda x, y: cmp(x["score"], y["score"]))
    publish_ids = [x["id"] for x in result]
    publish_ids.reverse()
    result_dict["ids"] = publish_ids
    return result_dict


def get_publish_id_not_recommend_by_datetime(city, user_id=0):
    result_dict = dict()
    interval = Datetimes.get_previous_interval()
    publishes = Publishes.get_shown_publishes(user_id).filter(
        Q(created_at__lt=interval[0]), Q(is_recommended=0) | Q(is_recommended__isnull=True))

    if publishes and city:
        publishes_city = publishes.filter(shop__dianping_city__exact=city)
    else:
        publishes_city = None
    if publishes_city:
        publishes = publishes_city.order_by("-created_at").values("id")
        result_dict["local"] = True
    else:
        publishes = publishes.filter(shop__dianping_city__exact=DEFAULT_CITY).order_by("-created_at").values("id")
        result_dict["local"] = False
    result = [x["id"] for x in publishes]
    result_dict["ids"] = result
    return result_dict


# 根据发布的id获取踩或赞的数量
def get_attribute_count(publish_id):
    interval = Datetimes.get_previous_interval()

    return UserPublishRelations.objects.filter(publish_id=publish_id,
                                               last_modified_at__gte=interval[0],
                                               last_modified_at__lte=interval[1]
                                               ).count()


def get_comment_count(publish_id):
    interval = Datetimes.get_previous_interval()

    return Comment.objects.filter(publish_id=publish_id,
                                  created_at__gte=interval[0],
                                  created_at__lte=interval[1]
                                  ).count()


# 将对象数组字典列表
def change_objects_to_list(obj_list):
    for i in range(len(obj_list)):
        obj_list[i].created_at = Datetimes.transfer_datetime(obj_list[i].created_at)
    return json.loads(serializers.serialize('json', obj_list))


def is_home_or_company(big_category_key):
    # Logs.print_log("is_home_or_company", big_category_key)
    if str(big_category_key) == PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_HOME_KEY \
            or str(big_category_key) == PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_COMPANY_KEY:
        return True
    else:
        return False


def is_weighted_by_editor(shop):
    if shop.weight > 0:
        return True
    else:
        return False


def is_shown_in_hot(shop):
    if is_weighted_by_editor(shop):
        return True
    elif is_home_or_company(get_shop_big_category(shop)["id"]):
        return False
    else:
        return True


# 根据分类获取内容
def get_content_by_category(publish_id, big_category_key, ignore_home_or_company=True):
    result = {}

    if ignore_home_or_company and is_home_or_company(big_category_key):
        return result
    all_operation = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_SHOW.get(str(big_category_key), {})
    all_answers = PUBLISH_SUBJECTIVE_CONSTANT.ANSWERS_SHOW
    keys = [key for key, value in all_operation.items()]
    try:
        publishes = Publish.objects.filter(id=publish_id)
        if publishes.count() > 0:
            all_dictionaries = change_objects_to_list(publishes)[0]["fields"]
            for k in keys:
                result[k] = all_answers[k].get(int(all_dictionaries[k]), None)
        else:
            for k in keys:
                result[k] = 0
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        pass
    return result


@api_view(http_method_names=["GET"])
def get_attribute_count_by_publish_id_json(request):
    result = {}
    publish_id = request.GET.get("id", None)
    publish = Publishes.get(publish_id)
    if publish:
        result["win"] = UserPublishRelationMethods.get_win_attribute_count_by_publish(publish)
        result["lost"] = UserPublishRelationMethods.get_lost_attribute_count_by_publish(publish)
    else:
        result = {"info": "publish id is necessary", "win": 0, "lost": 0}
    return Response(result)


def get_attribute_count_by_publish_id(publish_id):
    user_publish_relation = UserPublishRelations.objects.filter(publish_id=publish_id)
    result = dict()
    result["win"] = user_publish_relation.filter(attribute=PUBLISH_SUBJECTIVE_CONSTANT.WIN_KEY).count()
    result["lost"] = user_publish_relation.filter(attribute=PUBLISH_SUBJECTIVE_CONSTANT.LOST_KEY).count()
    return result


def get_recommended_count_by_shop_id(shop_id):
    user_shop_relation = UserShopRelations.objects.filter(shop_id=shop_id)
    result = dict()
    result["recommended"] = user_shop_relation.filter(
        is_recommended=PUBLISH_SUBJECTIVE_CONSTANT.RECOMMENDED_KEY).count()
    result["not_recommended"] = user_shop_relation.filter(
        is_recommended=PUBLISH_SUBJECTIVE_CONSTANT.NOT_RECOMMENDED_KEY).count()
    return result


# 根据发布的id获取前三条评论信息
def get_comment_by_publish_id(publish_id):
    comment = Comment.objects.filter(publish_id=publish_id).order_by("-created_at")
    result = dict()
    data = []
    if comment.count() > 0:
        result["count"] = comment.count()
        # comment_list = change_objects_to_list(comment)
        if comment.count() > 3:
            comment_needed_list = comment[:3]
        else:
            comment_needed_list = comment
        for c in comment_needed_list:
            temp = dict()
            temp["content"] = c.content
            temp["user_id"] = c.user.id
            temp["publish_id"] = c.publish_id
            temp_image = get_user_image(c.user)
            temp["user_big_image"] = temp_image["big_user_image"]
            temp["user_small_image"] = temp_image["small_user_image"]
            temp["created_at"] = Datetimes.transfer_datetime(c.created_at)
            temp["user_nickname"] = c.user.userextension.nickname
            data.append(temp)
        result["data"] = data
    else:
        result["count"] = 0
        result["data"] = []
    return result


def get_PM2_5_degree(PM2_5):
    if PM2_5 < 0:
        return ""

    for p in RULES_CONSTANT.PM2_5_DEGREE:
        if p.get("min_value") <= PM2_5 and p.get("max_value") >= PM2_5:
            return p.get("name")
    return ""


def get_shop_big_category(shop):
    result = {}
    if shop.category.parent_id == 1 and shop.category.id != 1:
        result["name"] = shop.category.name
        result["id"] = shop.category.id
    else:
        shop_category = ShopCategory.objects.filter(id=shop.category.parent_id)
        if shop_category.count() == 1:
            result["name"] = shop_category[0].name
            result["id"] = shop_category[0].id
    return result


def get_publish_count_by_shop(shop_id, user_id=0):
    # Logs.print_log("get_publish_count_by_shop", "start")
    return Publishes.get_shown_publishes(user_id).filter(shop_id=shop_id).count()


def get_attribute_by_user_and_publish(user_id, publish_id):
    result = dict()
    upr = UserPublishRelations.objects.filter(user_id=user_id, publish_id=publish_id)
    if upr.count() == 1:
        result["attribute"] = upr[0].attribute
        result["attribute_id"] = upr[0].id
    else:
        result["attribute"] = -1
        result["attribute_id"] = 0
    return result


def get_recommend_by_user_and_shop(user_id, shop_id):
    result = dict()
    usr = UserShopRelations.objects.filter(user_id=user_id, shop_id=shop_id)
    if usr.count() == 1:
        result["is_recommended"] = usr[0].is_recommended
    else:
        result["is_recommended"] = -1
    return result


def get_device_brand_name(key):
    for brand in DEVICE_BRAND:
        if int(brand.get("key", -1)) == int(key):
            return brand.get("name")
    return None


def get_publish_images(publish):
    result = dict()
    # Logs.print_log("get_publish_images", "start")
    if publish.big_image:
        # Logs.print_log("get_publish_images", "middle0")
        name_parts = publish.big_image.name.split(".")
        path_parts = publish.big_image.name.split("/")
        mid_path = "/".join(path_parts[:-1])
        big_name = ".".join(name_parts[:-1]) + "_big." + name_parts[-1]
        medium_name = ".".join(name_parts[:-1]) + "_medium." + name_parts[-1]
        small_name = ".".join(name_parts[:-1]) + "_small." + name_parts[-1]
        # Logs.print_log("get_publish_images", "middle1")
        result["publish_image_url"] = files.BASE_URL_4_IMAGE + publish.big_image.name
        result["publish_image_big_url"] = files.BASE_URL_4_IMAGE + big_name
        result["publish_image_medium_url"] = files.BASE_URL_4_IMAGE + medium_name
        result["publish_image_small_url"] = files.BASE_URL_4_IMAGE + small_name

        # Logs.print_log("get_publish_images", "middle2")
        img_path = result["publish_image_url"]
        img_big_path = result["publish_image_big_url"]
        img_medium_path = result["publish_image_medium_url"]
        img_small_path = result["publish_image_small_url"]
        # print img_path
        # print img_medium_path
        # print img_small_path
        # Logs.print_log("get_publish_images", "middle3")
        resize_img(img_path, img_big_path, 1280, mid_path)
        # Logs.print_log("get_publish_images", 640)
        resize_img(img_path, img_medium_path, 640, mid_path)
        # Logs.print_log("get_publish_images", 120)
        resize_img(img_path, img_small_path, 120, mid_path)
        # Logs.print_log("get_publish_images", "middle4")
    else:
        result["publish_image_url"] = ""
        result["publish_image_medium_url"] = ""
        result["publish_image_small_url"] = ""
    # Logs.print_log("get_publish_images", "start")
    return result


# @execute_time
def get_publish_infos(publish, longitude, latitude, user_id):
    result = {}
    if publish.device:
        result["sequence"] = publish.device.sequence
    else:
        result["sequence"] = None
    if user_id == 0:
        result["attribute"] = -1
        result["attribute_id"] = 0
    else:
        attribute_dict = get_attribute_by_user_and_publish(user_id, publish.id)
        result["attribute"] = attribute_dict["attribute"]
        result["attribute_id"] = attribute_dict["attribute_id"]

    result["publish_id"] = publish.id
    result["created_at"] = Datetimes.transfer_datetime(publish.created_at)
    result["show_time"] = get_show_time(publish.created_at)

    days = get_delta_days(Datetimes.transfer_datetime(publish.created_at).date())
    result["days"] = days
    result["month"] = days / 30

    attributes = get_attribute_count_by_publish_id(publish.id)
    if publish.user:
        if publish.user.userextension:
            result["user_nickname"] = publish.user.userextension.nickname
        else:
            result["user_nickname"] = ""
    else:
        result["user_nickname"] = ""

    temp_image = get_user_image(publish.user)
    # result["user_big_image"] = temp_image["big_user_image"]
    result["user_small_image"] = temp_image["small_user_image"]
    # result["win"] = attributes["win"]
    # result["lost"] = attributes["lost"]

    temp_publish_images = get_publish_images(publish)
    result["publish_image_url"] = temp_publish_images["publish_image_url"]
    result["publish_image_big_url"] = temp_publish_images["publish_image_big_url"]
    result["publish_image_medium_url"] = temp_publish_images["publish_image_medium_url"]
    result["publish_image_small_url"] = temp_publish_images["publish_image_small_url"]
    result["PM2_5"] = publish.PM2_5

    result["PM2_5_degree"] = get_PM2_5_degree(publish.PM2_5)
    # result["comment"] = get_comment_by_publish_id(publish.id)
    result["content"] = publish.content

    result["bonus"] = RedEnvelopes.get_red_envelope_by_publish(publish.id)
    if publish.device:
        result["device_brand"] = get_device_brand_name(publish.device.brand)
    else:
        result["device_brand"] = None

    s = publish.shop
    big_category = get_shop_big_category(s)

    if big_category:
        big_category_key = big_category["id"]
        result["big_category_name"] = big_category["name"]
        result["big_category_key"] = big_category["id"]
    else:
        big_category_key = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_KEY
        result["big_category_name"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_VALUE
        result["big_category_key"] = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_KEY

    if not big_category_key:
        result["category_operation"] = {}
    else:
        result["category_operation"] = get_content_by_category(publish.id, big_category_key, False)
    shop_result = get_shop_concrete_information(s, longitude, latitude, user_id)
    result = dict(result.items() + shop_result.items())

    return result


def get_distance_from_shop(shop_object, latitude, longitude):
    distance = -1
    try:
        if shop_object.dianping_business_id:
            distance = Shops.get_distance(latitude, longitude, shop_object.dianping_latitude,
                                          shop_object.dianping_longitude)
        else:
            distance = Shops.get_distance(latitude, longitude, shop_object.address.latitude,
                                          shop_object.address.longitude)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        pass
    return distance


def get_shop_image(s):
    if s.photo_url:
        return files.BASE_URL_4_IMAGE + s.photo_url
    else:
        if s.category:
            if s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_ENTERTAINMENT_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[
                    PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_ENTERTAINMENT_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_HOTEL_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_HOTEL_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_BEAUTY_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[
                    PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_BEAUTY_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_PATERNITY_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[
                    PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_PATERNITY_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_COMPANY_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[
                    PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_COMPANY_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_FOOD_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_FOOD_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_SPORT_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_SPORT_KEY]
            elif s.category.id == int(PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_HOME_KEY):
                return PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_DEFAULT_ICON[PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_HOME_KEY]

        return SHOP_DEFAULT_ICON


def get_shop_name(s):
    if s.name:
        return s.name
    else:
        return s.dianping_name


def get_city(s):
    return HomePage.clear_city(s.dianping_city)


def get_shop_category_name(s):
    return s.category.name


def get_shop_price(s):
    if s.dianping_avg_price:
        return s.dianping_avg_price
    else:
        return 0.0


def get_shop_score_key(s, user_id=0):
    publishes = get_all_publishes_by_shop(s, user_id)
    result = "--"
    score = -1000

    if publishes.count() < 0:
        pass
    else:
        score_dict = Scores.get_total_score(s, user_id)
        score = Scores.get_concrete_score(score_dict)
        score_level = Scores.get_level_info(score)
        result = score_level.get("key", "--")

    return result


def is_shop_valid_score(s):
    if not s.formaldehyde:
        return False
    else:
        return True


def is_from_dianping(s):
    if s.dianping_business_id:
        return True
    else:
        return False


def get_shop_concrete_information(shop_object, longitude, latitude, user_id):
    result = dict()
    result["shop_id"] = shop_object.id
    result["business_url"] = shop_object.dianping_business_url
    result["deals_url"] = shop_object.dianping_deals_url
    result["shop_name"] = get_shop_name(shop_object)
    result["shop_price"] = shop_object.dianping_avg_price
    result["shop_category"] = shop_object.category.name
    if shop_object.dianping_business_id:
        result["city"] = HomePage.clear_city(shop_object.dianping_city)
        result["shop_address"] = shop_object.dianping_city + shop_object.dianping_address
        result["is_from_dianping"] = True
    else:
        result["is_from_dianping"] = False
        if shop_object.address.china:
            result["city"] = HomePage.clear_city(shop_object.address.china.name)
            result["shop_address"] = shop_object.address.china.name + shop_object.address.detail_address
        else:
            result["city"] = HomePage.clear_city(shop_object.dianping_city)
            result["shop_address"] = shop_object.address.detail_address
    result["shop_image"] = get_shop_image(shop_object)

    result["shop_rate"] = get_rating_image_by_dianping_avg_rating(shop_object.dianping_avg_rating,
                                                                  result["is_from_dianping"])

    result["distance"] = get_distance_from_shop(shop_object, latitude, longitude)
    result["publish_count"] = get_publish_count_by_shop(shop_object.id, user_id)
    result["has_coupon"] = shop_object.dianping_has_coupon
    result["has_deal"] = shop_object.dianping_has_deal

    publishes = Publishes.get_publishes_by_shop(shop_object)
    score = -1000
    if publishes.count() < 0:
        pass
    else:
        score_dict = get_total_score(shop_object, user_id)

        co2_level = score_dict.get("CO2_OBJECT", None)
        if co2_level:
            result["CO2_level"] = co2_level.get("name", "--")
        else:
            result["CO2_level"] = "--"

        pm2_5_level = score_dict.get("PM2_5_OBJECT", None)
        if pm2_5_level:
            result["PM2_5_level"] = pm2_5_level.get("name", "--")
        else:
            result["PM2_5_level"] = "--"

        formaldehyde_level = score_dict.get("FORMALDEHYDE_OBJECT", None)
        if formaldehyde_level:
            result["FORMALDEHYDE_level"] = formaldehyde_level.get("name", "--")
        else:
            result["FORMALDEHYDE_level"] = "--"

        tvoc_level = score_dict.get("TVOC_OBJECT", None)
        if tvoc_level:
            result["TVOC_level"] = tvoc_level.get("name", "--")
        else:
            result["TVOC_level"] = "--"

        score = Scores.get_concrete_score(score_dict)
    if score == -1000:
        result["score"] = -1000
        result["score_key"] = "--"
        result["score_name"] = "--"
    else:
        result["score"] = score
        score_level = Scores.get_level_info(score)
        result["score_key"] = score_level.get("key", "--")
        result["score_name"] = score_level.get("name", "--")
    result["formaldehyde"] = shop_object.formaldehyde
    if shop_object.formaldehyde_image:
        result["formaldehyde_image"] = files.BASE_URL_4_IMAGE + shop_object.formaldehyde_image.name
    else:
        result["formaldehyde_image"] = None
    result["TVOC"] = shop_object.TVOC
    result["CO2"] = shop_object.CO2
    result["valid_score"] = is_shop_valid_score(shop_object)

    recommend_attribute = get_recommended_count_by_shop_id(shop_object.id)
    result["recommended_count"] = recommend_attribute["recommended"]
    result["not_recommended_count"] = recommend_attribute["not_recommended"]
    result["is_recommended"] = get_recommend_by_user_and_shop(user_id, shop_object.id)["is_recommended"]
    return result


def get_detail_by_shop(shop_id, longitude, latitude, user_id):
    result = {}
    try:
        shops = Shop.objects.filter(id=shop_id)
        if shops.count() > 0:
            shop_object = shops[0]
        else:
            shop_object = None
        if not shop_object:
            pass
        else:
            publishes = Publishes.get_shown_publishes(user_id).filter(shop_id=shop_id).order_by("-created_at")
            if publishes.count() > 0:
                publish_object = publishes[0]
                result = get_publish_infos(publish_object, longitude, latitude, user_id)
            else:
                # print "has no publish"
                pass
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        pass
    return result


def get_detail_by_publish(publish_id, longitude, latitude, user_id, is_discovery=False):
    result = {}
    try:
        if is_discovery:
            p = Publishes.get_shown_publishes(-1).get(id=publish_id)
        else:
            p = Publishes.get_shown_publishes(user_id).get(id=publish_id)
        result = get_publish_infos(p, longitude, latitude, user_id)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
    return result


def get_rating_image_by_dianping_avg_rating(dianping_avg_rating, parameter_is_from_dianping=True):
    if not parameter_is_from_dianping:
        return ""
    else:
        rating = float(dianping_avg_rating)
    if rating == 0:
        return DIANPING_RATING[0]
    elif rating == 0.5:
        return DIANPING_RATING[1]
    elif rating == 1.0:
        return DIANPING_RATING[2]
    elif rating == 1.5:
        return DIANPING_RATING[3]
    elif rating == 2.0:
        return DIANPING_RATING[4]
    elif rating == 2.5:
        return DIANPING_RATING[5]
    elif rating == 3.0:
        return DIANPING_RATING[6]
    elif rating == 3.5:
        return DIANPING_RATING[7]
    elif rating == 4.0:
        return DIANPING_RATING[8]
    elif rating == 4.5:
        return DIANPING_RATING[9]
    elif rating == 5.0:
        return DIANPING_RATING[10]


# **********************************dianping API******************************************
# ***********************************我的**********************************


@api_view(http_method_names=["GET"])
def get_infos_about_user(request):
    result_dict = dict()

    bonus = 0
    user_id = request.GET.get('id', 0)
    if not user_id:
        result_dict["info"] = "user_id does not exist!"
        pass
    else:
        result_dict["unread_comment_count"] = 0
        result_dict["unread_win_count"] = Publishes.get_unread_win_count(user_id)
        result_dict["unread_lost_count"] = Publishes.get_unread_lost_count(user_id)
        publishes = Publishes.get_shown_publishes(user_id).filter(user_id=user_id)
        user = Users.get_user_by_id(user_id)
        if not user:
            result_dict["info"] = "user does not exist"
            return Response(result_dict)
        if not publishes:
            result_dict["count"] = 0
            result_dict["bonus"] = 0
        else:
            for p in publishes:
                bonus += RedEnvelopes.get_red_envelope_by_publish(p.id)
                unread_comments = Comments.get_unread_by_publish(p.id)
                result_dict["unread_comment_count"] += unread_comments.count()
            result_dict["count"] = publishes.count()
            result_dict["bonus"] = bonus
        result_dict["user_id"] = user.id
        result_dict["username"] = user.username
        user_images = Users.get_user_image(user)
        if user_images:
            result_dict["big_image"] = user_images["big_user_image"]
        else:
            result_dict["big_image"] = None

        ue = Users.get_user_extension(user.username)
        if ue:
            ue.account = bonus
            ue.save()
            result_dict["nickname"] = ue.nickname
            result_dict["account"] = ue.account
            result_dict["level"] = ue.level_id
            result_dict["user_extension_id"] = ue.id
            result_dict["gender"] = ue.gender
            result_dict["city"] = ue.city

    return Response(result_dict)


# ***********************************我的**********************************
# ***********************************红包**********************************


def set_configuration():
    RULES_CONSTANT.set_red_envelope_configuration()
    RULES_CONSTANT.print_configuration()
    # RULES_CONSTANT.set_configuration_device()
    # RULES_CONSTANT.set_configuration_extra()
    # RULES_CONSTANT.set_configuration_rain()


def get_valid_or_request_red_envelope(start, end, red_type=RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA):
    """
    :param start:  起始时间， 如 2015-9-1 00:00:00
    :param end: 结束时间， 如 2015-9-30 23:59:59
    :param red_type: 红包类型，默认为连续红包
    :return: 一定时间内已经被申请（包括已经发放的和正在发放的红包）的红包金额
    """
    result = 0
    pools = RedEnvelope.objects.filter(
        Q(created_at__gte=start),
        Q(created_at__lte=end),
        Q(type=red_type),
        Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST) | Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_VALID)
    )
    if pools.count() == 0:
        return result
    else:
        for p in pools:
            result += p.bonus
    return result


def get_invalid_bonus_from_red_envelope(start, end, red_type=RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA):
    """
    :param start:  起始时间， 如 2015-9-1 00:00:00
    :param end: 结束时间， 如 2015-9-30 23:59:59
    :param red_type: 红包的类型，默认为连续红包
    :return: 一定时间内已经被申请的无效（红包雨或连续红包中的无效红包,不包括设备红包，因为设备红包没有限额）红包金额
    """
    result = 0
    rds = RedEnvelope.objects.filter(
        created_at__gte=start,
        created_at__lte=end,
        state=RULES_CONSTANT.RED_ENVELOPE_STATE_INVALID,
        type=red_type
    )
    if rds.count() == 0:
        return result
    for r in rds:
        result += r.bonus
    return result


# 默认为已经发布过信息（如果出错）
def is_published_by_device(sequence):
    result = {}
    try:
        device = Device.objects.get(sequence=sequence)
        publishes = Publish.objects.filter(device_id=device.id)
        if publishes.count() > 0:
            is_publish = True
        else:
            is_publish = False
        result["success"] = is_publish
        result["info"] = "OK"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["info"] = e.message
        result["success"] = True
    return result


# 默认为已经发布过信息（如果出错）
def is_published_by_user(user_id):
    result = {}
    try:
        publishes = Publish.objects.filter(user_id=user_id)
        if publishes.count() > 0:
            is_publish = True
        else:
            is_publish = False
        result["success"] = is_publish
        result["info"] = "OK"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["info"] = e.message
        result["success"] = True
    return result


# rest是剩余金额， min_value是最小的金额, min_money,max_money是金钱按比例换算的整数
def compute_red_envelope(rest, min_money, min_value, max_value, possibility=0.7, factor=10):
    """
    :param rest: 剩余金额
    :param min_money: 最小的金额
    :param min_value: 最小金额变整数
    :param max_value: 最大金额变整数
    :param possibility: 获得红包的机率
    :param factor: 金额变整数时所乘的因子
    :return: 红包金额
    """
    if rest < min_money:
        return 0

    value = random.randint(min_value, max_value)
    possibility_value = min_value + (max_value - min_value) * possibility

    if value > possibility_value:
        return 0
    bonus = float(value) / factor
    if bonus > rest:
        bonus = rest
    bonus = float(floor(bonus * factor)) / factor

    return bonus


def is_not_published_by_device(sequence):
    if not sequence:
        return False
    try:
        id = Device.objects.get(sequence=sequence).id
        re = RedEnvelope.objects.filter(device_id=id, state=RULES_CONSTANT.RED_ENVELOPE_STATE_VALID,
                                        type=RULES_CONSTANT.RED_ENVELOPE_TYPE_DEVICE)
        if re.count() == 0:
            return True
        return False
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return False


def generate_red_envelope(bonus, user_id, device_id, rd_type, rd_state):
    """
    :param bonus:  红包金额
    :param user_id:  用户ID
    :param device_id:  设备ID
    :param rd_type:  红包类型
    :param rd_state: 红包状态
    :return: 创建新的红包
    """
    rd = RedEnvelope(bonus=bonus, user_id=user_id, device_id=device_id, type=rd_type, state=rd_state)
    rd.save()


def get_device_id_by_sequence(sequence):
    devices = Device.objects.filter(sequence=sequence)
    if devices.count() == 0:
        return 0
    else:
        return devices[0].id


def get_red_envelope_by_device(sequence, user_id):
    """
    新用户（第一次发布的用户）和 新设备（第一次发布的设备）获取设备红包
    :param sequence:  设备UUID
    :param user_id:  用户ID
    :return: 红包数据，获取红包是否成功，说明信息
    """
    RULES_CONSTANT.set_red_envelope_configuration()
    result = {}
    device_id = get_device_id_by_sequence(sequence)
    rd_type = RULES_CONSTANT.RED_ENVELOPE_TYPE_DEVICE
    rd_state = RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST
    bonus = RULES_CONSTANT.RED_ENVELOPE_BY_DEVICE
    if device_id == 0:
        result["bonus"] = 0
        result["info"] = "the device does not exist"
        result["success"] = False
        return result
    is_publish = is_published_by_device(sequence)
    is_publish_user = is_published_by_user(user_id)
    if not is_publish["success"] and not is_publish_user["success"]:
        RedEnvelopes.generate(bonus, user_id, device_id, rd_type, rd_state)
        result["bonus"] = RULES_CONSTANT.RED_ENVELOPE_BY_DEVICE
        result["info"] = "OK"
        result["success"] = True
    else:
        if is_publish["info"] == "OK":
            result["bonus"] = 0
            result["info"] = "the device has got a bonus"
            result["success"] = True
        else:
            result["bonus"] = 0
            result["info"] = is_publish["info"]
            result["success"] = False
    return result


def get_month_range():
    """
    :return: 本月的起始时间，如 2015-9-1 00:00:00  -- 2015-9-30 23:59:59
    """
    result = dict()
    now = datetime.datetime.now()
    year = now.year
    month = now.month
    month_range = calendar.monthrange(year, month)
    start = str(year) + "-" + str(month) + "-1 00:00:00"
    end = str(year) + "-" + str(month) + "-" + str(month_range[1]) + " 23:59:59"
    result["start"] = Datetimes.string_to_clock(start)
    result["end"] = Datetimes.string_to_clock(end)
    return result


def get_today_date():
    return datetime.datetime.now().date()


def date_to_string(dt):
    return dt.strftime(DATE_FORMAT)


def time_to_string(t):
    return t.strftime(TIME_FORMAT)


def get_today_start():
    today_date = get_today_date()
    return Datetimes.string_to_clock(date_to_string(today_date) + " 00:00:00")


def valid_user(request):
    result = dict()
    if not request.user.is_authenticated():
        result["bonus"] = 0
        result["success"] = False
        result["info"] = "login is required"
        return result
    today_start = get_today_start()
    publishes = Publishes.get_shown_publishes(request.user.id).filter(created_at__gte=today_start,
                                                                      user_id=request.user.id)
    if publishes.count() > 0:
        for p in publishes:
            if p.bonus > 0:
                result["bonus"] = 0
                result["success"] = False
                result["info"] = "the user has got bonus"
                return result
    result["success"] = True
    result["info"] = "the user is valid"
    return result


def is_some_day_publish(user_id, start, end):
    publishes = Publish.objects.filter(user_id=user_id, created_at__gte=start, created_at__lte=end)
    if publishes.count() > 0:
        return True
    return False


def get_some_day_start_end(days):
    result = []
    if days > 0:
        for i in range(1, days):
            day = {}
            dt = Datetimes.get_some_day(i)
            day["start"] = Datetimes.get_day_start(dt)
            day["end"] = Datetimes.get_day_end(dt)
            result.append(day)
    return result


def is_get_extra_red_envelope(user_id):
    """
    前4天内是否得过红包（没）， 前4天是否每天都有发布
    :param user_id:
    :return:
    """
    days = RULES_CONSTANT.RED_ENVELOPE_EXTRA_KEEP - 1
    four_days_ago = Datetimes.get_some_day(days)
    day_start = Datetimes.get_day_start(four_days_ago)

    rds = RedEnvelope.objects.filter(Q(user_id=user_id),
                                     Q(created_at__gte=day_start),
                                     Q(type=RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA),
                                     Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST) |
                                     Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_VALID))

    if rds.count() == 0:
        day_list = get_some_day_start_end(5)
        for day in day_list:
            if not is_some_day_publish(user_id, day["start"], day["end"]):
                return False
        return True
    return False


def get_extra_red_envelope(sequence, user_id):
    """
    :param sequence:  设备UUID， 获取连续红包时必须连接设备
    :param user_id:  用户ID
    :return: 红包金额，说明信息，是否成功
    在本月内， 根据剩余金额和红包限额获取红包， 并创建新的红包（请求状态)
    """
    RULES_CONSTANT.set_red_envelope_configuration()
    result = {}
    device_id = get_device_id_by_sequence(sequence)
    rd_type = RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA
    rd_state = RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST
    if device_id == 0 or not is_get_extra_red_envelope(user_id):
        result["bonus"] = 0
        result["info"] = "no device or no extra red envelope"
        result["success"] = True
        return result
    try:
        time_limit = get_month_range()
        start = time_limit["start"]
        end = time_limit["end"]
        given_bonus = get_valid_or_request_red_envelope(start, end)
        rest = RULES_CONSTANT.RED_ENVELOPE_EXTRA_THRESHOLD - given_bonus
        factor = RULES_CONSTANT.RED_ENVELOPE_FACTOR
        min_money = RULES_CONSTANT.RED_ENVELOPE_EXTRA_MIN
        min_value = RULES_CONSTANT.RED_ENVELOPE_EXTRA_MIN * factor
        max_value = RULES_CONSTANT.RED_ENVELOPE_EXTRA_MAX * factor
        bonus = compute_red_envelope(
            rest,
            min_money,
            min_value,
            max_value,
            RULES_CONSTANT.RED_ENVELOPE_EXTRA_POSSIBILITY,
            factor
        )
        if bonus == 0:
            result["bonus"] = bonus
            result["info"] = "no bonus it is possible"
            result["success"] = True
        else:
            RedEnvelopes.generate(bonus, user_id, device_id, rd_type, rd_state)
            result["bonus"] = bonus
            result["info"] = "OK"
            result["success"] = True
    except Exception as e:
        result["bonus"] = 0
        result["info"] = e.message
        result["success"] = False
    return result


def is_rain_red_envelope_time():
    now = datetime.datetime.now().date()
    start = Datetimes.string_to_clock(
        date_to_string(now) + " " + time_to_string(RULES_CONSTANT.RED_ENVELOPE_RAIN_START))
    end = Datetimes.string_to_clock(date_to_string(now) + " " + time_to_string(RULES_CONSTANT.RED_ENVELOPE_RAIN_END))
    LOCAL_TIME_DELTA = datetime.timedelta(hours=8)
    local_now = datetime.datetime.now()
    # local_now = datetime.datetime.now().replace(tzinfo=None) + LOCAL_TIME_DELTA
    if start <= local_now <= end:
        return True
    return False


def is_get_rain_red_envelope(user_id):
    # return False
    now = datetime.datetime.now()
    start = Datetimes.get_day_start(now)
    end = Datetimes.get_day_end(now)
    rds = RedEnvelope.objects.filter(Q(user_id=user_id),
                                     Q(created_at__gte=start),
                                     Q(created_at__lte=end),
                                     Q(type=RULES_CONSTANT.RED_ENVELOPE_TYPE_RAIN),
                                     Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST) |
                                     Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_VALID))
    if rds.count() > 0:
        return True
    return False


def get_rain_start_and_end():
    set_configuration()
    result = {}
    dt = datetime.datetime.now().date()
    start_str = date_to_string(dt) + " " + time_to_string(RULES_CONSTANT.RED_ENVELOPE_RAIN_START)
    end_str = date_to_string(dt) + " " + time_to_string(RULES_CONSTANT.RED_ENVELOPE_RAIN_END)
    result["start"] = Datetimes.string_to_clock(start_str)
    result["end"] = Datetimes.string_to_clock(end_str)
    return result


def get_rain_count(user_id):
    start_and_end = get_rain_start_and_end()
    start = start_and_end["start"]
    end = start_and_end["end"]
    rds = RedEnvelope.objects.filter(Q(user_id=user_id),
                                     Q(created_at__gte=start),
                                     Q(created_at__lte=end),
                                     Q(type=RULES_CONSTANT.RED_ENVELOPE_TYPE_RAIN),
                                     Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST) |
                                     Q(state=RULES_CONSTANT.RED_ENVELOPE_STATE_VALID))
    return rds.count()


def get_rain_red_envelope(sequence, user_id):
    """
    :param sequence:  设备UUID
    :param user_id:  用户ID
    :return: 红包雨 -- 是否成功， 说明信息，红包金额
    查看红包雨是否已经发放了足够数量； 查看今天是否获取了请求红包或有效红包；
    """
    result = {}
    count = get_rain_count(user_id)
    if count >= RULES_CONSTANT.RED_ENVELOPE_RAIN_COUNT:
        result["success"] = False
        result["info"] = "the rain stopped"
        result["bonus"] = 0
        return result

    if is_get_rain_red_envelope(user_id):
        result["success"] = False
        result["info"] = "you have got the rain today"
        result["bonus"] = 0
        return result
    device_id = get_device_id_by_sequence(sequence)
    if device_id == 0:
        result["success"] = False
        result["info"] = "There is no device"
        result["bonus"] = 0
        return result
    rd_type = RULES_CONSTANT.RED_ENVELOPE_TYPE_RAIN
    rd_state = RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST

    start_and_end = get_rain_start_and_end()
    start = start_and_end["start"]
    end = start_and_end["end"]
    given_bonus = get_valid_or_request_red_envelope(start, end, rd_type)
    rest = RULES_CONSTANT.RED_ENVELOPE_RAIN_THRESHOLD - given_bonus
    factor = RULES_CONSTANT.RED_ENVELOPE_FACTOR
    min_money = RULES_CONSTANT.RED_ENVELOPE_RAIN_MIN
    min_value = RULES_CONSTANT.RED_ENVELOPE_RAIN_MIN * factor
    max_value = RULES_CONSTANT.RED_ENVELOPE_RAIN_MAX * factor
    bonus = compute_red_envelope(
        rest,
        min_money,
        min_value,
        max_value,
        RULES_CONSTANT.RED_ENVELOPE_RAIN_POSSIBILITY,
        factor
    )
    result["bonus"] = bonus
    if bonus == 0:
        result["success"] = False
        result["info"] = "There is no enough money for red envelope or it is possible"
    else:
        RedEnvelopes.generate(bonus, user_id, device_id, rd_type, rd_state)
        result["success"] = True
        result["info"] = "OK"
    return result


@api_view(http_method_names=["GET"])
def get_red_envelope(request):
    sequence = request.GET.get('sequence', None)
    user_id = request.GET.get('user_id', 0)
    return RedEnvelopes.get_red_envelope(sequence, user_id)


@api_view(http_method_names=["GET"])
def get_red_envelope_by_user(request):
    result = {}
    user_id = request.GET.get("user_id", 0)
    if user_id == 0:
        result["info"] = "user id is needed"
        result["success"] = False
        return result
    result = RedEnvelopes.get_valid_red_envelope(user_id)
    result["success"] = True
    result["info"] = "OK"
    return Response(result)


def get_red_envelope_object(user_id, rd_type):
    rds = RedEnvelope.objects.filter(user_id=user_id,
                                     state=RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST,
                                     type=rd_type).order_by('-id')
    count = rds.count()
    i = 0
    if count > 1:
        for rd in rds:
            if i == 0:
                i += 1
                continue
            i += 1
            rd.state = RULES_CONSTANT.RED_ENVELOPE_STATE_INVALID
            rd.save()
        return rds[0]
    elif count >= 1:
        return rds[0]
    else:
        return None


def set_red_envelope_object(obj, publish_id):
    if not obj:
        return False
    if not publish_id:
        return False
    obj.state = RULES_CONSTANT.RED_ENVELOPE_STATE_VALID
    obj.publish_id = publish_id
    obj.save()
    return True


@api_view(http_method_names=["POST"])
def set_red_envelope(request):
    result = {}
    user_id = request.data.get('user_id', 0)
    publish_id = request.data.get('publish_id', 0)
    sequence = request.data.get('sequence', None)

    if not user_id or not publish_id or not sequence:
        result["success"] = False
        result["info"] = "parameters (user_id, publish_id, sequence) do not exist"
        return Response(result)
    try:
        extra = RedEnvelopes.get_request_by_user_and_type(user_id, RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA)
        RedEnvelopes.bind(extra, publish_id)
        device = RedEnvelopes.get_request_by_user_and_type(user_id, RedEnvelopes.RED_ENVELOPE_TYPE_DEVICE)
        RedEnvelopes.bind(device, publish_id)
        device = Devices.get(sequence)
        if device:
            Devices.set_is_published(device)
        rain = RedEnvelopes.get_request_by_user_and_type(user_id, RedEnvelopes.RED_ENVELOPE_TYPE_RAIN)
        RedEnvelopes.bind(rain, publish_id)
        result["success"] = True
        result["info"] = "OK"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["success"] = False
        result["info"] = ex.message
    return Response(result)


def get_valid_red_envelope_by_user():
    result_list = []
    temp_list = []
    rds = RedEnvelope.objects.filter(state=RULES_CONSTANT.RED_ENVELOPE_STATE_VALID).order_by("user_id")
    previous_user_id = 0
    previous_user_username = ""
    previous_user_nickname = ""
    total_bonus = 0
    user_count = 0
    sub_total_bonus = 0
    sub_total_device_bonus = 0
    sub_total_extra_bonus = 0
    sub_total_rain_bonus = 0
    for rd in rds:
        result = dict()
        result["user_id"] = rd.user.id
        result["user_username"] = rd.user.username
        result["user_nickname"] = rd.user.userextension.nickname
        result["bonus"] = rd.bonus
        result["type"] = rd.type
        total_bonus += rd.bonus
        result_list.append(result)
    for rl in result_list:
        if rl["user_id"] == previous_user_id:
            sub_total_bonus += rl["bonus"]
            if int(rl["type"]) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA):
                sub_total_extra_bonus += rl["bonus"]
            elif int(rl["type"]) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_RAIN):
                sub_total_rain_bonus += rl["bonus"]
            else:
                sub_total_device_bonus += rl["bonus"]
        else:
            if previous_user_id == 0:
                pass
            else:
                temp = dict()
                temp["user_id"] = previous_user_id
                temp["user_username"] = previous_user_username
                temp["user_nickname"] = previous_user_nickname
                temp["bonus"] = sub_total_bonus
                temp["device_bonus"] = sub_total_device_bonus
                temp["extra_bonus"] = sub_total_extra_bonus
                temp["rain_bonus"] = sub_total_rain_bonus
                temp_list.append(temp)
            previous_user_id = rl["user_id"]
            previous_user_username = rl["user_username"]
            previous_user_nickname = rl["user_nickname"]
            sub_total_bonus = rl["bonus"]
            if int(rl["type"]) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA):
                sub_total_extra_bonus = rl["bonus"]
            elif int(rl["type"]) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_RAIN):
                sub_total_rain_bonus = rl["bonus"]
            else:
                sub_total_device_bonus = rl["bonus"]
            user_count += 1

        temp_list = sorted(temp_list, key=operator.itemgetter("bonus"), reverse=True)

        result_dict = dict()
        result_dict["total_bonus"] = total_bonus
        result_dict["user_count"] = user_count
        result_dict["detail"] = temp_list
        return result_dict


def show_valid_red_envelope_by_user(request):
    result = get_valid_red_envelope_by_user()
    if result:
        pass
    else:
        result = dict()
    result["total_threshold"] = RULES_CONSTANT.RED_ENVELOPE_EXTRA_THRESHOLD + RULES_CONSTANT.RED_ENVELOPE_RAIN_THRESHOLD
    result["extra_threshold"] = RULES_CONSTANT.RED_ENVELOPE_EXTRA_THRESHOLD
    result["rain_threshold"] = RULES_CONSTANT.RED_ENVELOPE_RAIN_THRESHOLD
    return render(request, "valid_red_envelope.html", {"red_envelope": result})


def get_request_red_envelope_by_user():
    result_dict = dict()
    result_list = []
    rds = RedEnvelope.objects.filter(state=RULES_CONSTANT.RED_ENVELOPE_STATE_REQUEST).order_by("user_id")
    total_bonus = 0
    user_count = 0
    for rd in rds:
        result = dict()
        total_bonus += rd.bonus
        user_count += 1
        result["user_username"] = rd.user.username
        result["user_nickname"] = rd.user.userextension.nickname
        if int(rd.type) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_DEVICE):
            result["type"] = "设备红包"
        elif int(rd.type) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_EXTRA):
            result["type"] = "连续发布红包"
        elif int(rd.type) == int(RULES_CONSTANT.RED_ENVELOPE_TYPE_RAIN):
            result["type"] = "红包雨"
        result["bonus"] = rd.bonus
        result_list.append(result)
    result_list = sorted(result_list, key=operator.itemgetter("bonus"), reverse=True)
    result_dict["user_count"] = user_count
    result_dict["total_bonus"] = total_bonus
    result_dict["detail"] = result_list
    return result_dict


def show_request_red_envelope_by_user(request):
    result = get_request_red_envelope_by_user()
    return render(request, "request_red_envelope.html", {"red_envelope": result})


# ***********************************红包**********************************

# ***********************************评分**********************************


@execute_time
@api_view(http_method_names=["GET"])
def get_score_by_publish(request):
    result = {}
    publish_id = request.GET.get("id", None)
    user_id = request.GET.get("user_id", 0)
    if not id:
        result["success"] = False
        result["level"] = {}
        result["info"] = "there is no such publish"
        return Response(result)
    else:
        publish = Publishes.get(publish_id)
        score_dict = Scores.get_total_score(publish.shop, user_id)
        score = Scores.get_concrete_score(score_dict)
        level = Scores.get_level_info(score)
        result["level"] = level
        result["info"] = "OK"
        result["success"] = True
    return Response(result)


def get_all_publishes_by_shop(shop, user_id=0):
    limit_day = Datetimes.get_some_day(30)
    return Publishes.get_shown_publishes(user_id).filter(shop_id=shop.id, created_at__gte=limit_day)


def get_pm2_5_score(shop, user_id=0):
    publishes = get_all_publishes_by_shop(shop, user_id)
    count = 0
    score = 0
    for p in publishes:
        if int(p.PM2_5) >= 0:
            score += p.PM2_5
            count += 1
    if count <= 0:
        return -1000
    else:
        return float(score) / count


def get_total_score(shop, user_id):
    result = {}
    average_pm2_5 = get_pm2_5_score(shop, user_id)
    p = get_some_score(average_pm2_5, RULES_CONSTANT.PM2_5_DEGREE)
    if p or p == 0:
        result["PM2_5_OBJECT"] = p
        result["PM2_5"] = p.get("score")

    f = get_some_score(shop.formaldehyde, RULES_CONSTANT.FORMALDEHYDE_DEGREE)
    if f or f == 0:
        result["FORMALDEHYDE_OBJECT"] = f
        result["FORMALDEHYDE"] = f.get("score", RULES_CONSTANT.DEFAULT_VALUE["FORMALDEHYDE"])
    c = get_some_score(shop.CO2, RULES_CONSTANT.CO2_DEGREE)
    if c:
        result["C02_OBJECT"] = c
        result["CO2"] = c.get("score")
    t = get_some_score(shop.TVOC, RULES_CONSTANT.TVOC_DEGREE)
    if t:
        result["TVOC_OBJECT"] = t
        result["TVOC"] = t.get("score")
    result["SUBJECTIVE"] = get_average_subjective_score(shop)
    return result


def get_some_score(value, some_list):
    if value < 0:
        return {}
    for some in some_list:
        min_value = some.get("min_value", None)
        max_value = some.get("max_value", None)
        if min_value and max_value:
            if min_value < value <= max_value:
                return some
            else:
                continue
        elif not min_value and max_value:
            if value <= max_value:
                return some
            else:
                continue
        elif not max_value and min_value:
            if min_value < value:
                return some
            else:
                continue
    return {}


def get_subjective_all_percent_by_category(big_category_key):
    publish_constant_all = PUBLISH_SUBJECTIVE_CONSTANT.PUBLISH_CONSTANT_ALL
    items = []
    for c in publish_constant_all:
        if int(c["key"]) == int(big_category_key):
            items = c["items"]
            break
    return items


def compute_single_subjective_score(item, value):
    score = 0
    percentage = int(item["percentage"])
    if percentage == 0:
        return 0

    flag = True
    for s in item["show"]:
        if s["name"] == value:
            flag = False
            score = percentage * int(s["score"])
            break
    if flag:
        return RULES_CONSTANT.DEFAULT_VALUE["SUBJECTIVE"] * percentage
    return score


def get_subjective_score(publish_object):
    big_category = get_shop_big_category(publish_object.shop)
    if big_category:
        big_category_key = big_category["id"]
    else:
        big_category_key = PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_OTHER_KEY
    if str(big_category_key) in (
            PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_COMPANY_KEY, PUBLISH_SUBJECTIVE_CONSTANT.CATEGORY_HOME_KEY):
        return RULES_CONSTANT.DEFAULT_VALUE["SUBJECTIVE"]
    content = get_content_by_category(publish_object.id, big_category_key)
    items = get_subjective_all_percent_by_category(big_category_key)

    score = 0
    for key, value in content.items():
        for item in items:
            if str(key) == str(item["key"]):
                score += compute_single_subjective_score(item, value)

    return float(score) / 100


def get_average_subjective_score(shop, user_id=0):
    publishes = get_all_publishes_by_shop(shop, user_id)
    score = 0
    # count = publishes.count()
    count = 0

    for p in publishes:
        score += get_subjective_score(p)
        count += 1
    if count == 0:
        return RULES_CONSTANT.DEFAULT_VALUE["SUBJECTIVE"]

    return float(score) / count


# ***********************************评分**********************************
# ***********************************获取评论信息**********************************


def get_comment_info(comment):
    result = dict()
    temp_image = Users.get_user_image(comment.user)
    result["user_big_image"] = temp_image["big_user_image"]
    result["user_small_image"] = temp_image["small_user_image"]
    ue = Users.get_user_extension(comment.user.username)
    result["user_nickname"] = ue.nickname
    result["content"] = comment.content
    return result


@api_view(http_method_names=["GET"])
def get_all_comments(request):
    result = list()
    publish_id = request.GET.get("id", None)
    count = int(request.GET.get("count", 20))
    start_id = int(request.GET.get("start_id", 0))
    comment_ids = request.GET.get("comment_ids", "")
    loaded_comment_ids = comment_ids.split(',')
    if not id:
        return Response(result)
    comments = Comments.get_by_publish(publish_id)
    result = Comments.get_information(
        comments, start_id, count, get_comment_info, loaded_comment_ids
    )
    # count_index = 0
    # for comment in comments:
    #     if count_index > (count - 1):
    #         break
    #     count_index += 1
    #     result = dict()
    #     temp_image = get_user_image(comment.user)
    #     result["user_big_image"] = temp_image["big_user_image"]
    #     result["user_small_image"] = temp_image["small_user_image"]
    #     result["user_nickname"] = comment.user.userextension.nickname
    #     result["content"] = comment.content
    #     result_list.append(result)
    return Response(result)


def get_all_publishes_by_user(user):
    publishes = Publishes.get_shown_publishes(user.id).filter(user_id=user.id)
    return publishes


def get_unread_comment_count_by_publish(publish):
    unread_comment = Comment.objects.filter(publish_id=publish.id, is_read=False)
    return unread_comment.count()


def get_latest_unread_comment_by_publish(publish):
    result = {}
    unread_comment = Comments.get_unread_by_publish(publish.id).order_by("-id")
    if unread_comment.count() > 0:
        result["is_single_person"] = Comments.is_single_person_by_comment(unread_comment)
        result["created_at"] = unread_comment[0].created_at
        result["content"] = unread_comment[0].content
        temp_image = Users.get_user_image(unread_comment[0].user)
        result["user_small_image"] = temp_image["small_user_image"]
        # result["user_big_image"] = temp_image["big_user_image"]
        temp_publish_images = Publishes.get_publish_images(publish)
        result["publish_image_small_url"] = temp_publish_images["publish_image_small_url"]
        result["publish_id"] = publish.id
        ue = Users.get_user_extension(unread_comment[0].user.username)
        result["user_nickname"] = ue.nickname
    return result


def update_unread_comment_by_publish(p):
    unread_comments = Comments.get_unread_by_publish(p.id).order_by("-id")
    try:
        Comments.bulk_update_unread(unread_comments)
        return True
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return False


@api_view(http_method_names=["GET"])
def get_comment_count_by_user(request):
    result = dict()
    user_id = request.GET.get("user_id", 0)
    # count = 0
    # win_count = 0
    # lost_count = 0
    # if user_id:
    #     user = Users.get_user_by_id(user_id)
    #     if user:
    #         publishes = Publishes.get_publishes_by_user(user.id)
    #         for p in publishes:
    #             count += get_unread_comment_count_by_publish(p)
    #             win_count += Publishes.get_unread_win_count(user.id)
    #             lost_count += p.lost_count
    result["count"] = Comments.get_unread_by_user(user_id)
    result["win_count"] = Publishes.get_unread_win_count(user_id)
    result["lost_count"] = Publishes.get_unread_lost_count(user_id)
    return Response(result)


@api_view(http_method_names=["GET"])
def get_comment_by_user(request):
    user_id = request.GET.get("user_id", 0)
    result = []
    # user = Users.get_user_by_id(user_id)
    if user_id:
        publishes = Publishes.get_publishes_by_user(user_id)
        for p in publishes:
            comment = get_latest_unread_comment_by_publish(p)
            if comment:
                result.append(comment)
    return Response(result)


@api_view(http_method_names=["PUT"])
def update_unread_comment_by_user(request):
    # print request.data
    user_id = request.data.get("user_id", 0)
    result = {}
    flag = True

    if user_id:
        users = User.objects.filter(id=user_id)
        if users.count() > 0:
            user = users[0]
            publishes = get_all_publishes_by_user(user)
            for p in publishes:
                if update_unread_comment_by_publish(p):
                    continue
                else:
                    result["info"] = "update unread comment failure"
                    flag = False
                    break
    result["success"] = flag
    return Response(result)


@api_view(http_method_names=["PUT"])
def update_unread_comment_by_publish_interface(request):
    # print request.data
    publish_id = request.data.get("publish_id", 0)
    result = {}

    if publish_id:
        try:
            # p = Publishes.get_shown_publishes(-1).get(id=publish_id)
            comments = Comments.get_by_publish(publish_id)
            Comments.bulk_update_unread(comments)
            # update_unread_comment_by_publish(p)
            result["info"] = "update unread comment successfully"
            result["success"] = True
        except Exception as ex:
            result["info"] = "update unread comment failure! message:" + ex.message
            result["success"] = False
    else:
        result["info"] = "there is no publish_id"
        result["success"] = False
    return Response(result)


# ***********************************获取评论信息**********************************
# ***********************************赞和踩**********************************


@api_view(http_method_names=["GET"])
def get_win_and_lost(request):
    return Response(UserPublishRelationMethods.get_win_and_lost())
    # return Response(PUBLISH_SUBJECTIVE_CONSTANT.ATTRIBUTES)


@api_view(http_method_names=["POST"])
def set_attribute(request):
    result = {}
    attribute = int(request.data.get("attribute", -1))
    user_id = request.data.get("user_id", 0)
    publish_id = request.data.get("publish_id", 0)
    try:
        publish = Publishes.get_shown_publishes(user_id).get(id=publish_id)
    except Exception as ex:
        publish = None

    if attribute == -1:
        result["info"] = "no attribute"
        return Response(result, status=status.HTTP_400_BAD_REQUEST)
    # if attribute == 1:
    #     attribute = True
    # else:
    #     attribute = False

    if not user_id or not publish_id:
        result["info"] = "parameters are not enough, user_id is " + str(user_id) \
                         + " and publish_id is " + str(publish_id)
        return Response(result, status=status.HTTP_400_BAD_REQUEST)

    upr = UserPublishRelationMethods.get(user_id, publish_id)
    if not upr:
        UserPublishRelationMethods.add(user_id, publish_id, attribute)

        if publish:
            win_or_lost_push(publish, attribute)

        result["new"] = True
        result["changed"] = True
        result["attribute"] = attribute
        result["info"] = "add new successfully"
        result["success"] = True
        return Response(result, status=status.HTTP_200_OK)
    else:
        if upr.attribute == attribute:
            result["changed"] = False
        else:
            UserPublishRelationMethods.set(upr, attribute)
            if publish:
                win_or_lost_push(publish, attribute, True)

            result["changed"] = True
        result["new"] = False
        result["attribute"] = attribute
        result["info"] = "update successfully"
        result["success"] = True
        if result["success"]:
            return Response(result, status=status.HTTP_200_OK)
        else:
            return Response(result, status=status.HTTP_204_NO_CONTENT)


def get_unread_win_and_lost_count_by_user(user_id):
    result = dict()
    publishes = Publishes.get_shown_publishes(user_id).filter(user_id=user_id)
    win_count = 0
    lost_count = 0
    for p in publishes:
        win_count += p.win_count
        lost_count += p.lost_count

    result["win_count"] = win_count
    result["lost_count"] = lost_count
    return result


# ***********************************赞和踩**********************************
# ***********************************推荐与不推荐**********************************


@api_view(http_method_names=["GET"])
def get_recommended_attributes(request):
    return Response(PUBLISH_SUBJECTIVE_CONSTANT.RECOMMENDED_ATTRIBUTES)


@api_view(http_method_names=["POST"])
def set_recommended_attribute(request):
    result = {}
    is_recommended = int(request.data.get("recommended", -1))
    user_id = int(request.data.get("user_id", 0))
    shop_id = int(request.data.get("shop_id", 0))
    if is_recommended == -1:
        result["info"] = "no attribute"
        return Response(result, status=status.HTTP_400_BAD_REQUEST)
    if user_id == 0 or shop_id == 0:
        result["info"] = "parameters are not enough, user_id is " + str(user_id) \
                         + " and publish_id is " + str(shop_id)
        return Response(result, status=status.HTTP_400_BAD_REQUEST)
    usr = UserShopRelations.objects.filter(user_id=user_id, shop_id=shop_id)
    usr = UserShopRelationMethods.get_by_shop_and_user(shop_id, user_id)
    if not usr:
        UserShopRelationMethods.add(user_id, shop_id, is_recommended)
        result["new"] = True
        result["changed"] = True
        result["recommended"] = is_recommended
        result["info"] = "add new successfully"
        result["success"] = True
        return Response(result, status=status.HTTP_201_CREATED)
    else:
        if usr.is_recommended == is_recommended:
            result["changed"] = False
        else:
            UserShopRelationMethods.update(usr, is_recommended)
            result["changed"] = True
        result["new"] = False
        result["attribute"] = is_recommended
        result["info"] = "update successfully"
        result["success"] = True
        if result["success"]:
            return Response(result, status=status.HTTP_200_OK)
        else:
            return Response(result, status=status.HTTP_204_NO_CONTENT)


# ***********************************推荐与不推荐**********************************
# ***********************************高德云图基本操作**********************************


CLOUD_MAP_KEY = "77593ca7870b278c804d841732d52b63"
# CLOUD_MAP_TABLE_ID = "55e003b3e4b0d6431a0bf3d7"  # 正式线
CLOUD_MAP_TABLE_ID = "56259fb5e4b0d9f82680e9da"  # 测试线
CLOUD_BASE_CREATE_URL = "http://yuntuapi.amap.com/datamanage/table/create"
CLOUD_BASE_SINGLE_INSERT_URL = "http://yuntuapi.amap.com/datamanage/data/create"
CLOUD_BASE_SINGLE_UPDATE_URL = "http://yuntuapi.amap.com/datamanage/data/update"
CLOUD_BASE_SINGLE_DELETE_URL = "http://yuntuapi.amap.com/datamanage/data/delete"
CLOUD_BASE_SEARCH_BY_ID_URL = "http://yuntuapi.amap.com/datasearch/id"
CLOUD_BASE_SEARCH_BY_CONDITIONS_URL = "http://yuntuapi.amap.com/datamanage/data/list"


def http_post_action(url, data):
    post_data = urllib.urlencode(data)

    # 提交，发送数据
    req = urllib2.Request(url, post_data)
    ret = urllib2.urlopen(req)

    # 获取提交后返回的信息
    content = ret.read()
    return content


def http_get_action(url):
    ret = urllib2.urlopen(url)
    content = ret.read()
    # print content
    return content


def json2object(content):
    return json.loads(content)


def exist_id(single_id):
    url = CLOUD_BASE_SEARCH_BY_ID_URL + "?key=" + CLOUD_MAP_KEY + "&tableid=" + CLOUD_MAP_TABLE_ID + "&_id=" + single_id
    content = http_get_action(url)
    content_dict = json2object(content)
    if content_dict["count"] > 0:
        return True
    else:
        return False


def insert_map_data(data):
    params = dict()
    params["key"] = CLOUD_MAP_KEY
    params["tableid"] = CLOUD_MAP_TABLE_ID
    params["loctype"] = 1
    must_data = data
    must_data["_name"] = data["name"]
    must_data["_location"] = str(data["geo-x"]) + "," + str(data["geo-y"])
    must_data["_address"] = data["address"]

    params["data"] = must_data
    # print "insert_map_data params:"
    # print params
    url = CLOUD_BASE_SINGLE_INSERT_URL
    return http_post_action(url, params)


def update_map_data(data, data_id):
    params = dict()
    params["key"] = CLOUD_MAP_KEY
    params["tableid"] = CLOUD_MAP_TABLE_ID
    params["loctype"] = 1

    must_data = data
    must_data["_id"] = str(data_id)
    must_data["_name"] = data["name"]
    must_data["_location"] = str(data["geo-x"]) + "," + str(data["geo-y"])
    must_data["_address"] = data["address"]

    params["data"] = must_data
    # print "update_map_data params:"
    # print params

    url = CLOUD_BASE_SINGLE_UPDATE_URL
    return http_post_action(url, params)


def delete_map_data(ids):
    params = dict()
    params["key"] = CLOUD_MAP_KEY
    params["tableid"] = CLOUD_MAP_TABLE_ID
    params["ids"] = ids
    url = CLOUD_BASE_SINGLE_DELETE_URL
    return http_post_action(url, params)


# 比较是否是days天前的数据(自然天）
def compare_beijing_time(utc_time, update_time, days=1):
    update_time = datetime.datetime.strptime(update_time, datetimes.DATETIME_FORMAT)
    beijing_time = Datetimes.transfer_datetime(utc_time)
    beijing_time_date = beijing_time.date()
    delta_time = beijing_time_date - update_time.date()
    the_days = 0
    if delta_time.days >= 0:
        the_days = delta_time.days + 1
    else:
        the_days = - delta_time.days
    if the_days > days:
        return True
    else:
        return False


# 获取 days 天前的数据id
def select_map_ids_before_days(days):
    now = datetime.datetime.now()
    url = CLOUD_BASE_SEARCH_BY_CONDITIONS_URL \
          + "?key=" + CLOUD_MAP_KEY \
          + "&tableid=" + CLOUD_MAP_TABLE_ID
    result = http_get_action(url)

    result_object = json2object(result)
    return [ro["_id"] for ro in result_object["datas"]
            if compare_beijing_time(now, ro["_updatetime"], days)]


def get_exist_shop_ids(data):
    ret = dict()
    filter_string = "shop_id:" + str(data["shop_id"])
    url = CLOUD_BASE_SEARCH_BY_CONDITIONS_URL \
          + "?key=" + CLOUD_MAP_KEY \
          + "&tableid=" + CLOUD_MAP_TABLE_ID + "&filter=" + filter_string
    # print url
    result = http_get_action(url)
    result_object = json2object(result)
    # print "does_exist_map_by_name_longitude_and_latitude:"
    # print result_object
    if result_object["count"] > 0:
        ret["exist"] = True
        ret["ids"] = [ro["_id"] for ro in result_object["datas"]]
    else:
        ret["exist"] = False
        ret["ids"] = []
    return ret


def delete_all_map_data(ids):
    if type(ids) == "str":
        ids = ids.split(",")
    quotient = len(ids) / 50
    reminder = len(ids) % 50

    if quotient == 0:
        pass
    else:
        for i in range(quotient):
            delete_ids = ",".join(ids[(i * 50): ((i + 1) * 50)])
            delete_map_data(delete_ids)
    if reminder > 0:
        delete_left_ids = ",".join(ids[(quotient * 50):len(ids)])
        delete_map_data(delete_left_ids)


def insert_or_update(data):
    ids_info = get_exist_shop_ids(data)
    if ids_info["exist"]:
        ids = ids_info["ids"]
        # print ids
        delete_all_map_data(ids)
        # print "insert again"
        return insert_map_data(data)
    else:
        # print "insert"
        return insert_map_data(data)


# 删除 days 天前的数据
def delete_map_data_some_days_before(days=1):
    ids = select_map_ids_before_days(days)
    ids_str = ",".join(ids)
    # print "delete_map_data_some_days_before:" + ids_str
    return delete_all_map_data(ids_str)


# 场所信息： PM2.5， longitude, latitude
def get_map_shops():
    shops = Shop.objects.all().exclude(dianping_longitude=0, dianping_latitude=0)
    result = []
    for s in shops:
        counts = get_hot_shop_compute_data(s)
        if counts["PM2_5_count"] > 0:
            result.append(s)
    return result


def get_shop_latest_pm2_5(s, user_id=0):
    today_date = datetime.datetime.now().date()
    today_start = Datetimes.string_to_clock(date_to_string(today_date) + " 00:00:00")
    publishes = Publishes.get_shown_publishes(user_id).exclude(
        shop_id=s.id, PM2_5=-1, created_at__gt=today_start).order_by("-id")
    if publishes:
        return publishes[0].PM2_5
    else:
        return -1


# 获取场所字段信息
def get_map_shop_model(s):
    result = dict()
    if is_from_dianping(s):
        flag = True
        result["is_from_dianping"] = "YES"
    else:
        flag = False
        result["is_from_dianping"] = "NO"
    result["shop_id"] = s.id
    # result["shop_image"] = Characters.unicode_to_concrete(get_shop_image(s))
    result["shop_image"] = json.dumps(Characters.unicode_to_concrete(get_shop_image(s)))
    # result["shop_image"] = get_shop_image(s)
    result["shop_rate"] = get_rating_image_by_dianping_avg_rating(s.dianping_avg_rating, flag)
    # result["shop_name"] = Characters.unicode_to_concrete(get_shop_name(s))
    result["shop_name"] = json.dumps(Characters.unicode_to_concrete(get_shop_name(s)))
    # result["city"] = Characters.unicode_to_concrete(get_city(s))
    result["city"] = json.dumps(Characters.unicode_to_concrete(get_city(s)))
    # result["shop_category"] = Characters.unicode_to_concrete(get_shop_category_name(s))
    result["shop_category"] = json.dumps(Characters.unicode_to_concrete(get_shop_category_name(s)))
    result["shop_price"] = get_shop_price(s)
    # result["score_key"] = Characters.unicode_to_concrete(get_shop_score_key(s))
    result["score_key"] = json.dumps(Characters.unicode_to_concrete(get_shop_score_key(s)))
    if is_shop_valid_score(s):
        result["valid_score"] = "YES"
    else:
        result["valid_score"] = "NO"

    result["latest_PM2_5"] = get_shop_latest_pm2_5(s)

    return result


# 获取发送到高德地图的一个场所的信息
def get_map_shop_info(s):
    result = get_map_shop_model(s)

    if s.dianping_business_id:
        # result["name"] = Characters.unicode_to_concrete(s.dianping_name)
        result["name"] = json.dumps(Characters.unicode_to_concrete(s.dianping_name))
        result["geo-x"] = s.dianping_longitude
        result["geo-y"] = s.dianping_latitude
        # result["address"] = Characters.unicode_to_concrete(s.dianping_city + s.dianping_address)
        result["address"] = json.dumps(Characters.unicode_to_concrete(s.dianping_city + s.dianping_address))
        # result["shop_model"] = get_map_shop_model(s)
    else:
        result["name"] = json.dumps(Characters.unicode_to_concrete(s.name))
        if s.address:
            result["geo-x"] = s.address.longitude
            result["geo-y"] = s.address.latitude
            # result["address"] = Characters.unicode_to_concrete(s.address.detail_address)
            result["address"] = json.dumps(Characters.unicode_to_concrete(s.address.detail_address))
            # result["shop_model"] = get_map_shop_model(s)
    return result


def get_all_map_shop_info():
    result = []
    shops = get_map_shops()
    for s in shops:
        temp = get_map_shop_info(s)
        if temp:
            result.append(temp)
    return result


@api_view(http_method_names=["GET"])
def get_map_shops_info(request):
    result = get_all_map_shop_info()
    return Response(result)


@api_view(http_method_names=["POST"])
def insert_or_update_shop_info(request):
    # 删除过期数据
    delete_map_data_some_days_before()

    publish_id = int(request.data.get("publish_id", 0))

    p = None
    result = {}
    if publish_id:
        publishes = Publish.objects.filter(id=publish_id)
        if publishes:
            p = publishes[0]

    if p:
        shop_info = get_map_shop_info(p.shop)
        result = json.loads(insert_or_update(shop_info))

    return Response(result)


def execute_cron_function():
    # 删除过期数据
    # print "start"
    # delete_map_data_some_days_before()
    # 创建或更新数据
    data = get_all_map_shop_info()
    # print len(data)
    for d in data:
        insert_or_update(d)
        # print "end"


# ***********************************高德云图基本操作**********************************
# ***********************************云图基本操作**********************************


def get_pm2_5_shop_ids_for_cloud():
    result = list()
    today_start = get_today_start()
    publishes = Publishes.get_shown_publishes(0).filter(created_at__gte=today_start, PM2_5__gte=0)
    for p in publishes:
        result.append(p.shop_id)
    return list(set(result))


def get_formaldehyde_shop_ids_for_cloud():
    result = list()
    today_start = get_today_start()
    publishes = Publishes.get_shown_publishes(0).exclude(created_at__gte=today_start, PM2_5__gte=0).filter(
        formaldehyde__gte=0)
    for p in publishes:
        result.append(p.shop_id)
    return list(set(result))


def get_formaldehyde_shop_ids_for_cloud():
    result = list()
    today_start = get_today_start()
    publishes = Publishes.get_shown_publishes(0).exclude(created_at__gte=today_start, PM2_5__gte=0).filter(
        formaldehyde__gte=0)
    for p in publishes:
        result.append(p.shop_id)
    return list(set(result))


def is_the_shop_id_around(shop_id, longitude, latitude, radius):
    try:
        shop = Shop.objects.get(id=shop_id)
        distance = get_distance_from_shop(shop, latitude, longitude)
        if 0 < distance < radius:
            return True
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    return False


def get_pm2_5_shop_ids_around_for_cloud(longitude, latitude, radius):
    result = list()
    today_shop_ids = get_pm2_5_shop_ids_for_cloud()
    for shop_id in today_shop_ids:
        if is_the_shop_id_around(shop_id, longitude, latitude, radius):
            result.append(shop_id)
    return result


def get_formaldehyde_shop_ids_around_for_cloud(longitude, latitude, radius):
    result = list()
    formaldehyde_shop_ids = get_formaldehyde_shop_ids_for_cloud()
    for shop_id in formaldehyde_shop_ids:
        if is_the_shop_id_around(shop_id, longitude, latitude, radius):
            result.append(shop_id)
    return result


def get_shop_address(s):
    result = dict()
    if s.dianping_business_id > 0:
        result["longitude"] = s.dianping_longitude
        result["latitude"] = s.dianping_latitude
    else:
        if s.address:
            result["longitude"] = s.address.longitude
            result["latitude"] = s.address.latitude
        else:
            result["longitude"] = 0
            result["latitude"] = 0
    return result


def get_pm2_5_shop_info_for_cloud(shop_ids):
    result = list()
    for shop_id in shop_ids:
        temp = dict()
        try:
            s = Shop.objects.get(id=shop_id)
            temp = get_shop_address(s)
            temp["site_id"] = s.id
            pm2_5 = Publishes.get_shown_publishes(0).filter(shop_id=shop_id).order_by("-id")[0].PM2_5
            temp["site_name"] = get_shop_name(s)
            temp["type"] = 0
            temp["data"] = pm2_5
            result.append(temp)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
    return result


def get_formaldehyde_shop_info_for_cloud(shop_ids):
    result = list()
    for shop_id in shop_ids:
        temp = dict()
        try:
            s = Shop.objects.get(id=shop_id)
            temp = get_shop_address(s)
            temp["site_id"] = s.id
            formaldehyde = Publishes.get_shown_publishes(0).filter(shop_id=shop_id).order_by("-id")[0].formaldehyde
            temp["site_name"] = get_shop_name(s)
            temp["type"] = 1
            temp["data"] = formaldehyde
            result.append(temp)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
    return result

    formaldehyde_ids = get_formaldehyde_shop_ids_around_for_cloud(longitude, latitude, radius)
    formaldehyde_data_list = get_formaldehyde_shop_info_for_cloud(formaldehyde_ids)


@api_view(http_method_names=["GET"])
def get_cloud_shops(request):
    longitude = float(request.GET.get("longitude", 0))
    latitude = float(request.GET.get("latitude", 0))
    radius = int(request.GET.get("radius", 20))

    ids = get_pm2_5_shop_ids_around_for_cloud(longitude, latitude, radius)
    pm2_5_data_list = get_pm2_5_shop_info_for_cloud(ids)

    formaldehyde_ids = get_formaldehyde_shop_ids_around_for_cloud(longitude, latitude, radius)
    formaldehyde_data_list = get_formaldehyde_shop_info_for_cloud(formaldehyde_ids)

    result = pm2_5_data_list + formaldehyde_data_list

    return Response(result)

    result = pm2_5_data_list + formaldehyde_data_list

    return Response(result)


# ***********************************云图基本操作**********************************
# ***********************************用户反馈**********************************


@api_view(http_method_names=["POST"])
def set_feedback(request):
    result = dict()
    content = request.data.get("content", "")
    content = content.strip()
    user_id = request.data.get("user_id", 0)
    phone_id = request.data.get("phone_id", 0)
    os_version = request.data.get("os_version", "")
    phone_type = request.data.get("phone_type", "")
    phone_number = request.data.get("phone_number", "")

    if not user_id or not content or not phone_id:
        result["success"] = False
        result["info"] = "user id, content and phone id are needed"
        return Response(result)
    try:
        phones = PhoneInfos.objects.filter(phone_id=phone_id)
        if phones.count() == 0:
            phone = PhoneInfos(phone_id=phone_id, os_version=os_version, phone_type=phone_type,
                               phone_number=phone_number)
            phone.save()
        # user = User.objects.get(id=user_id)

        feedback = Feedback(content=content, user_id=user_id, phone_id=phone_id)
        feedback.save()
        result["success"] = True
        result["info"] = "add feed back successfully"
    except Exception as ex:
        result["success"] = False
        result["info"] = ex.message
        Logs.print_current_function_name_and_line_number(ex)
    return Response(result)


@api_view(http_method_names=["GET"])
# @ip_record
def show_feedback(request):
    result_list = get_feedback()
    return render(request, "feedback.html", {"feed_backs": result_list})


def get_feedback():
    feed_backs = Feedback.objects.all().order_by("-id")
    result_list = list()
    for feedback in feed_backs:
        result = dict()
        result["username"] = feedback.user.username
        result["nickname"] = feedback.user.userextension.nickname
        result["created_at"] = Datetimes.transfer_datetime(feedback.created_at)
        result["phone_number"] = feedback.phone.phone_number
        result["phone_id"] = feedback.phone.phone_id
        result["os_version"] = feedback.phone.os_version
        result["phone_type"] = feedback.phone.phone_type
        result["content"] = feedback.content
        result_list.append(result)
    return result_list


# ***********************************用户反馈**********************************
# ***********************************微信分享点击**********************************


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def set_click_title_record(request):
    result = {}
    try:
        title_id = int(request.data.get("title_id", 0))
        title = request.data.get("title", "")

        # if not title_id:
        #     result["info"] = "there is no title_id"
        #     result["success"] = False
        # else:
        record = ClickTitleRecord(title_id=title_id, title=title)
        record.save()
        result["info"] = "OK"
        result["success"] = True
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
    return Response(result)


@api_view(http_method_names=["GET"])
# @ip_record
def show_click_title_record(request):
    # print request.META.get("REMOTE_ADDR", "DEFAULT")
    records = ClickTitleRecord.objects.order_by("id")
    result_list = list()
    result = dict()
    if records:
        result["start_datetime"] = records[0].created_at
    else:
        result["start_datetime"] = datetime.datetime.now()
    record_list = []
    for record in records:
        record_list.append(record.title_id)
    record_set = set(record_list)
    for title_id in record_set:
        result_dict = dict()
        result_dict["title_id"] = title_id
        result_dict["click_count"] = record_list.count(title_id)
        result_list.append(result_dict)
    result["click_title_records"] = result_list
    return render(request, "click_title_record.html", {"result": result})


# ***********************************微信分享点击**********************************

# *******************************通过url保存图片到本地************************************


def create_path(business_id, parent_name=None):
    now = datetime.datetime.now()
    if not parent_name:
        path = "media_root/" + str(business_id) + "/"
    else:
        path = "media_root/" + parent_name + "/" + str(now.year) + "/" + str(now.month) + "/" + str(now.day) + "/"
    if not os.path.exists(path):
        try:
            os.makedirs(path)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

    return path


def get_picture_path(url, name, business_id, parent_name=None):
    # url = request.get(name, None)
    if not url:
        return None
    try:
        path = create_path(business_id, parent_name)
        file_name = url.split('/')[-1]
        path = path + name + "_" + str(file_name)
        return str(path)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return None


def image_save(url, name, business_id, parent_name=None):
    data = {}
    try:
        # url = request.get(name, None)
        if not url:
            data["success"] = False
            data["info"] = "no picture"
            return data
        file = cStringIO.StringIO(urllib2.urlopen(url).read())
        path = get_picture_path(url, name, business_id, parent_name)
        if not path:
            data["success"] = False
            data["info"] = "no real path"
            return data
        img = Image.open(file)
        img.save(path)
        data["success"] = True
        data["info"] = "save successfully"
    except Exception as e:
        data["success"] = False
        data["info"] = e.message
    return data


def resize_img(img_path, out_path, new_width=1280, part="publish"):
    # from PIL import Image
    path = "media_root/" + part + "/" + out_path.split('/')[-1]
    if os.path.exists(path):
        return None

    the_file = None
    try:
        content = urllib2.urlopen(img_path, data=None, timeout=10).read()
        the_file = cStringIO.StringIO(content)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return

    # 读取图像
    im = Image.open(the_file)
    # 获得图像的宽度和高度
    width, height = im.size
    # 计算高宽比
    ratio = 1.0 * height / width
    # 计算新的高度
    new_height = int(new_width * ratio)
    new_size = (new_width, new_height)
    # print new_width
    # 插值缩放图像，
    out = im.resize(new_size, Image.ANTIALIAS)

    out.save(path)


# *******************************通过url保存图片到本地************************************

# *********************************publish小编审核**********************************************


def get_user_image(user):
    result = dict()
    if user:
        if user.userextension.big_image:
            user_image = files.BASE_URL_4_IMAGE + user.userextension.big_image.name
            # Logs.print_log("user_image", user_image)
        else:
            if user.userextension.gender == "M":
                user_image = USER_DEFAULT_MALE_ICON
            else:
                user_image = USER_DEFAULT_FEMALE_ICON
    else:
        user_image = USER_DEFAULT_ICON
    user_image_path = "/".join(user_image.split("/")[:-1])
    user_image_name = user_image.split("/")[-1]
    big_user_image_name = ".".join(user_image_name.split(".")[:-1]) + "_big." + user_image_name.split(".")[-1]
    small_user_image_name = ".".join(user_image_name.split(".")[:-1]) + "_small." + user_image_name.split(".")[-1]
    big_user_image = user_image_path + "/" + big_user_image_name
    small_user_image = user_image_path + "/" + small_user_image_name
    try:
        resize_img(user_image, big_user_image, 240, part="user")
        resize_img(user_image, small_user_image, 96, part="user")
        result["big_user_image"] = big_user_image
        result["small_user_image"] = small_user_image
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        if user.userextension.gender == "M":
            result["big_user_image"] = USER_DEFAULT_MALE_ICON
            result["small_user_image"] = USER_DEFAULT_MALE_ICON
        else:
            result["big_user_image"] = USER_DEFAULT_FEMALE_ICON
            result["small_user_image"] = USER_DEFAULT_FEMALE_ICON

    return result


def my_publish_data_by_publish(publish):
    result = {}
    result["id"] = publish.id
    username = publish.user.username
    result["username"] = username
    result["nickname"] = publish.user.userextension.nickname

    temp_image = get_user_image(publish.user)
    result["big_user_image"] = temp_image["big_user_image"]
    result["small_user_image"] = temp_image["small_user_image"]

    photo = publish.big_image.name
    result["photo"] = files.BASE_URL_4_IMAGE + photo
    description = publish.content
    result["description"] = description
    PM2_5 = publish.PM2_5
    result["PM2_5"] = PM2_5
    big_category = get_shop_big_category(publish.shop)
    label = ""
    if big_category:
        label_dict = get_content_by_category(publish.id, big_category["id"])
        for k, v in label_dict.items():
            if v:
                label = label + v.encode("utf-8") + "\n"

    result["label"] = label

    attribute = get_attribute_count_by_publish_id(publish.id)
    win = attribute.get("win")
    result["win"] = win
    lost = attribute.get("lost")
    result["lost"] = lost
    comment = get_comment_count(publish.id)
    result["comment"] = comment
    result["created_at"] = Datetimes.transfer_datetime(publish.created_at)
    if publish.recommend_weight:
        result["weight"] = publish.recommend_weight
    else:
        result["weight"] = 0
    result["audit"] = publish.audit
    result["shop_name"] = publish.shop.name
    result["shop_id"] = publish.shop.id
    result["shop_audit"] = publish.shop.audit

    if publish.shop.address:
        result["detail_address"] = publish.shop.address.detail_address
        result["longitude"] = publish.shop.address.longitude
        result["latitude"] = publish.shop.address.latitude
    else:
        result["detail_address"] = publish.shop.dianping_address
        result["longitude"] = publish.shop.dianping_longitude
        result["latitude"] = publish.shop.dianping_latitude

    if publish.shop.address:
        result["detail_address"] = publish.shop.address.detail_address
        result["longitude"] = publish.shop.address.longitude
        result["latitude"] = publish.shop.address.latitude
    else:
        result["detail_address"] = publish.shop.dianping_address
        result["longitude"] = publish.shop.dianping_longitude
        result["latitude"] = publish.shop.dianping_latitude

    return result


def my_publish_data(publish_sort="-id", city=u"北京", count=20, page=1, publish_search=""):
    result = {}
    publishes = []
    all_publish = Publish.objects.filter(shop__dianping_city__contains=city, content__contains=publish_search).order_by(
        publish_sort)
    result["count"] = all_publish.count()
    result["page"] = int(ceil(float(all_publish.count()) / float(count)))
    publish_ids = list()
    shop_ids = list()
    index = 0
    current_count = int(count) * (int(page) - 1)
    count_index = 0
    for publish in all_publish:
        index += 1
        if index > current_count:
            if count_index < count:
                publish_ids.append(publish.id)
                shop_ids.append(publish.shop.id)
                publishes.append(my_publish_data_by_publish(publish))
                count_index += 1
            else:
                break
    result["publishes"] = publishes

    shop_ids = list(set(shop_ids))
    shop_ids_str = [str(id) for id in shop_ids]
    result["shop_ids"] = "_".join(shop_ids_str)

    publish_ids = list(set(publish_ids))
    publish_ids_str = [str(id) for id in publish_ids]
    result["publish_ids"] = "_".join(publish_ids_str)

    return result


def get_my_publish_count(current_user_phone):
    if not current_user_phone:
        return 0
    user = User.objects.filter(username=current_user_phone)
    if not user:
        return 0
    user_id = user[0].id
    publishes = Publish.objects.filter(user_id=user_id)
    return publishes.count()


@authenticated
def my_publish(request):
    publish_sort = request.GET.get("publish_sort", "-id")
    publish_search = request.GET.get("publish_search", "")
    current_user_phone = request.GET.get("phone", None)
    city = request.GET.get("city", u"北京")
    page = request.GET.get("page", 1)
    count = request.GET.get("count", 10)
    publishes = my_publish_data(publish_sort, city, count, page, publish_search)
    current_page = int(page)
    count_per_page = 10
    count_total_page = int(publishes["page"])
    pagination_result = pagination(current_page, count_per_page, count_total_page)
    current_user_count = get_my_publish_count(current_user_phone)

    return render(request, "publish_4_editor.html",
                  {
                      "publishes": publishes["publishes"],
                      "publish_ids": publishes["publish_ids"],
                      "shop_ids": publishes["shop_ids"],
                      "count": publishes["count"],
                      "page_list": pagination_result["show_page_indexes"],
                      "current_page": page,
                      "city": city,
                      "current_user_count": current_user_count,
                      "publish_sort": publish_sort,
                      "publish_search": publish_search,
                      "previous_page": pagination_result["previous_page_index"],
                      "next_page": pagination_result["next_page_index"],
                      "total_page": count_total_page,
                  })


def my_shop_check(request):
    page = str(request.GET.get("page", 1))
    city = request.GET.get("city", u"北京")
    publish_search = request.GET.get("publish_search", "")
    publish_sort = request.GET.get("publish_sort", "-id")
    try:
        audit = int(request.GET.get("checked", 1))
        shop_id = int(request.GET.get("shop_id", 0))
        shop = Shop.objects.get(id=shop_id)
        if shop:
            shop.audit = audit
            shop.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
    return HttpResponseRedirect(
        "/support/publish?page=" + page + "&city=" + city + "&publish_search=" + publish_search + "&publish_sort=" + publish_sort)


def my_publish_check(request):
    page = str(request.GET.get("page", 1))
    city = request.GET.get("city", u"北京")
    publish_search = request.GET.get("publish_search", "")
    publish_sort = request.GET.get("publish_sort", "-id")
    try:
        audit = int(request.GET.get("checked", 1))
        publish_id = int(request.GET.get("publish_id", 0))
        publish = Publish.objects.get(id=publish_id)
        if publish:
            publish.audit = audit
            publish.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
    return HttpResponseRedirect(
        "/support/publish?page=" + page + "&city=" + city + "&publish_search=" + publish_search + "&publish_sort=" + publish_sort)


def update_recommend_weight(id, weight):
    result = {}
    try:
        publish = Publish.objects.get(id=id)
        publish.recommend_weight = weight
        publish.is_recommended = 1
        publish.save()
        result["success"] = True
        result["info"] = "update recommend_weight successfully"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["success"] = False
        result["info"] = ex.message
    return result


def parse_post_list(request):
    result_list = []

    for key in request.data:
        if key.startswith("checkbox_"):
            result = {}
            result["id"] = key.split("_")[-1]
            result["recommend_weight"] = request.data.get("weight_" + str(key.split("_")[-1]))
            result_list.append(result)
    return result_list


def audit_publish(publish_id, audit):
    try:
        p = Publish.objects.get(id=publish_id)
        p.audit = audit
        p.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


def audit_shop(shop_id, audit):
    try:
        shop = Shop.objects.get(id=shop_id)
        shop.audit = audit
        shop.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


def my_publish_actions(request):
    sort = request.data.get("sort", None)
    page = request.data.get("current_page", 1)
    publish_sort = request.data.get("publish_sort", "-id")
    publish_search = request.data.get("publish_search", "")
    checkbox_all = request.data.get("checkbox_all", "off")
    publish_ids = request.data.get("publish_ids", "off")
    shop_ids = request.data.get("shop_ids", "off")
    city = request.data.get("city", u"北京")

    if not sort:
        if checkbox_all == "on":
            publish_ids = publish_ids.split("_")
            for id in publish_ids:
                audit_publish(int(id), 1)
            shop_ids = shop_ids.split("_")
            for id in shop_ids:
                audit_shop(int(id), 1)
            return HttpResponseRedirect(
                "/support/publish?publish_sort=" + publish_sort + "&publish_search=" + publish_search + "&city=" + city + "&page=" + page)

        publish_list = parse_post_list(request)
        for publish in publish_list:
            update_recommend_weight(publish.get("id"), publish.get("recommend_weight"))
        return HttpResponseRedirect(
            "/support/publish?publish_sort=" + publish_sort + "&publish_search=" + publish_search + "&city=" + city + "&page=" + page)
    else:
        return HttpResponseRedirect(
            "/support/publish?publish_sort=" + publish_sort + "&publish_search=" + publish_search + "&city=" + city + "&page=" + page)


def my_shop_data_by_shop(shop):
    result = get_one_shop_check_data(shop)
    result["id"] = shop.id
    result["name"] = shop.name
    if shop.address and shop.address.china:
        result["city"] = shop.address.china.name
    else:
        result["city"] = ""
    if shop.address:
        result["detail_address"] = shop.address.detail_address
    else:
        result["detail_address"] = None
    if shop.category:
        result["category"] = shop.category.name
    else:
        result["category"] = u"其它"
    result["audit"] = shop.audit
    result["weight"] = shop.weight
    return result


def my_shop_data(shop_sort="-id", city=u"北京", count=20, page=1, shop_search="", local=0):
    result_dict = dict()
    # Logs.print_log("shop search in my_shop_data", shop_search)
    result = []
    index = 0
    count_index = 0
    current_index = int(count) * (int(page) - 1)

    if int(local) == 1:
        all_shop = Shop.objects.filter(dianping_business_id=0, dianping_city__contains=city,
                                       name__contains=shop_search).order_by(shop_sort)

        if not all_shop:
            pass
        else:
            for shop in all_shop:
                index += 1
                if index >= current_index:
                    if count_index < count:
                        result.append(my_shop_data_by_shop(shop))
                        count_index += 1
                    else:
                        break
        result_dict["count"] = int(ceil((all_shop.count()) / float(count)))
        result_dict["data"] = result
    elif int(local) == 0:
        all_shop = Shop.objects.filter(dianping_business_id=0, dianping_city__contains=city,
                                       name__contains=shop_search).order_by(shop_sort)

        if not all_shop:
            pass
        else:
            for shop in all_shop:
                index += 1
                if index >= current_index:
                    if count_index < count:
                        result.append(my_shop_data_by_shop(shop))
                        count_index += 1
                    else:
                        break

        dianping_all_shop = Shop.objects.filter(dianping_city__contains=city, name__contains=shop_search).exclude(
            dianping_business_id=0).order_by(shop_sort)

        if not dianping_all_shop:
            pass
        else:
            for s in dianping_all_shop:
                index += 1
                if index >= current_index:
                    if count_index < count:
                        result.append(my_shop_data_by_shop(s))
                        count_index += 1
                    else:
                        break

        result_dict["count"] = int(ceil((all_shop.count() + dianping_all_shop.count()) / float(count)))
        result_dict["data"] = result

    else:
        dianping_all_shop = Shop.objects.filter(dianping_city__contains=city, name__contains=shop_search) \
            .exclude(dianping_business_id=0).order_by(shop_sort)
        if not dianping_all_shop:
            pass
        else:
            for s in dianping_all_shop:
                index += 1
                if index >= current_index:
                    if count_index < count:
                        result.append(my_shop_data_by_shop(s))
                        count_index += 1
                    else:
                        break
        result_dict["count"] = int(ceil((dianping_all_shop.count()) / float(count)))
        result_dict["data"] = result
    return result_dict


def shop_parse_post_list(request):
    result_list = []

    for key in request.data:
        if key.startswith("checkbox_"):
            result = dict()
            result["id"] = key.split("_")[-1]
            # result["audit"] = request.data.get("audit_" + str(key.split("_")[-1]))
            result_list.append(result)

    return result_list


def update_shop_info(id, the_checkbox, weight):
    result = {}
    try:
        shop = Shop.objects.get(id=id)
        shop.audit = the_checkbox
        shop.weight = weight
        # if not shop.audit or shop.audit == -1:
        #     shop.audit = 1
        # else:
        #     shop.audit = 0
        shop.save()
        result["success"] = True
        result["info"] = "OK"
    except Exception as e:
        result["success"] = False
        result["info"] = e.message
    return result


def my_shop_actions(request):
    sort = request.data.get("sort", None)
    search = request.data.get("search", None)
    shop_city = request.data.get("search_city", None)
    local = request.data.get("local", 0)
    shop_search = request.data.get("search_value", "")
    shop_sort = request.data.get("shop_sort", "id")

    if not shop_city:
        shop_city = u"北京"
    if sort:
        return HttpResponseRedirect("/shop/show?shop_sort=" + shop_sort + "&city=" + shop_city + "&local=" + local)
    elif search:

        return HttpResponseRedirect("/shop/show?shop_sort="
                                    + shop_sort + "&search_value="
                                    + shop_search + "&city=" + shop_city + "&local=" + local)
    else:
        shop_list = shop_parse_post_list(request)
        for shop in shop_list:
            the_checkbox = request.data.get(shop.get("id") + "_checkout", "off")
            if the_checkbox == "on":
                the_checkbox = True
            else:
                the_checkbox = False
            weight = request.data.get("weight_" + shop.get("id"), 0)
            print "my_shop_actions"
            print weight
            update_shop_info(shop.get("id"), the_checkbox, weight)
        return HttpResponseRedirect("/shop/show?shop_sort="
                                    + shop_sort + "&search_value="
                                    + shop_search + "&city=" + shop_city + "&local=" + local)


def my_shop(request):
    shop_sort = request.GET.get("shop_sort", "-id")
    shops = my_shop_data(shop_sort)
    return render(request, "shop_4_editor.html", {"shops": shops})


# shop 审核后台
def get_one_shop_check_data(shop_object):
    result = dict()
    result = Hot.get_hot_shop_compute_data(shop_object)
    result["total_score"] = Hot.hot_shop_compute(shop_object)
    result["shop_name"] = shop_object.name
    result["shop_weight"] = shop_object.weight
    result["shop_id"] = shop_object.id
    result["detail_url"] = ""
    result["audit"] = shop_object.audit

    return result


def get_shop_weight_count():
    return Shop.objects.exclude(weight=0).count()


# def shop_check_actions(request):
#     sort = request.data.get("sort", None)
#     # Logs.print_log("sort in shop_check_actions", sort)
#     search = request.data.get("search", None)
#     # Logs.print_log("search in shop_check_actions", search)
#
#     if sort:
#         shop_sort = request.data.get("shop_sort", "-id")
#         return HttpResponseRedirect("/shop/show?shop_sort=" + shop_sort)
#     elif search:
#         shop_search = request.data.get("search_value", "")
#         # Logs.print_log("shop_search in shop_check_actions", shop_search)
#         return HttpResponseRedirect("/shop/show?shop_sort=-id&search_value=" + shop_search)
#     else:
#         shop_list = shop_parse_post_list(request)
#         for shop in shop_list:
#             weight = request.data.get("weight_" + shop.get("id"), 0)
#             update_shop_info(shop.get("id"),)
#         return HttpResponseRedirect("/shop/show")

def pagination(current_page, count_per_page, count_total_page):
    result = dict()
    if current_page / count_per_page > 0:
        previous_page = current_page / count_per_page - 1
        previous_page_index = previous_page * count_per_page + 1
    else:
        previous_page_index = 0

    if current_page / count_per_page == count_total_page / count_per_page or count_total_page == count_per_page:
        next_page = 0
        next_page_index = 0
    else:
        next_page = current_page / count_per_page + 1
        next_page_index = next_page * count_per_page + 1

    if next_page == 0:
        show_page_indexes = range(current_page / count_per_page * count_per_page + 1, count_total_page + 1)
    else:
        show_page_indexes = range(current_page / count_per_page * count_per_page + 1,
                                  (current_page / count_per_page + 1) * count_per_page + 1)

    result["previous_page_index"] = previous_page_index
    result["next_page_index"] = next_page_index
    result["show_page_indexes"] = show_page_indexes
    return result


@authenticated
def shop_check_4_editor_interface(request):
    shop_sort = request.GET.get("shop_sort", "-id")
    shop_search = request.GET.get("search_value", "")
    city = request.GET.get("city", u"北京")
    count = request.GET.get("count", 10)
    page = request.GET.get("page", 1)
    local = request.GET.get("local", 0)

    shops = my_shop_data(shop_sort, city, count, page, shop_search, local)

    int_page = int(page)
    total_page = shops["count"]
    page_show_count = 10
    pagination_result = pagination(int_page, page_show_count, total_page)

    return render(
        request,
        "editor/shop_check_4_editor.html",
        {
            "shops_info": shops["data"],
            "shops_count": pagination_result["show_page_indexes"],
            "shops_page": page,
            "shops_city": city,
            "previous_page": pagination_result["previous_page_index"],
            "next_page": pagination_result["next_page_index"],
            "total_page": total_page,
            "local": local,
            "shop_sort": shop_sort,
            "shops_search": shop_search
        },
    )


# **************************************添加甲醛


# 获取数据
def get_shop_data_4_add_formaldehyde(shop_name, shop_city, shop_address):
    # print "start get data"
    result = []
    shops_added = Shop.objects.all().filter(dianping_business_id=0)
    shops = Shop.objects.all().exclude(dianping_business_id=0)
    if shop_name:
        if shop_name.startswith("^") and shop_name.endswith("$"):
            shop_name = shop_name[1: -1]
            shops_added = shops_added.filter(name=shop_name)
            shops = shops.filter(dianping_name=shop_name)
        elif shop_name.startswith("^"):
            shop_name = shop_name[1:]
            shops_added = shops_added.filter(name__istartswith=shop_name)
            shops = shops.filter(dianping_name__istartswith=shop_name)
        elif shop_name.endswith("$"):
            shop_name = shop_name[:-1]
            shops_added = shops_added.filter(name__iendswith=shop_name)
            shops = shops.filter(dianping_name__iendswith=shop_name)
        else:
            shops_added = shops_added.filter(name__icontains=shop_name)
            shops = shops.filter(dianping_name__icontains=shop_name)

    if shop_city:
        shops = shops.filter(dianping_city__icontains=shop_city)
        shops_added = shops_added.filter(dianping_city__icontains=shop_city)

    if shop_address:
        shops = shops.filter(dianping_address__icontains=shop_address)
        try:
            shops_added = shops_added.filter(address__detail_address__icontains=shop_address)
        except:
            pass

    for s in shops_added:
        # Logs.print_log("added shop", s)
        temp = dict()
        temp["shop_id"] = s.id
        temp["shop_business_id"] = s.dianping_business_id
        temp["shop_name"] = s.name
        temp["shop_city"] = s.dianping_city
        temp["shop_address"] = s.address.detail_address
        temp["formaldehyde"] = s.formaldehyde
        result.append(temp)
    # Logs.print_log("middle result", result)

    for s in shops:
        temp = dict()
        temp["shop_id"] = s.id
        temp["shop_business_id"] = s.dianping_business_id
        temp["shop_name"] = s.name
        temp["shop_city"] = s.dianping_city
        temp["shop_address"] = s.dianping_address
        temp["formaldehyde"] = s.formaldehyde
        result.append(temp)

    return result


# 解析post数据获取场所ID和新甲醛数据
@permission_classes((AllowAny,))
def parse_add_formaldehyde_post(request):
    post = request.data
    # Logs.print_log("post", post)
    result = []
    for key in post:
        if key.startswith("shop_id_"):
            temp = dict()
            shop_id = key.split("_")[-1]
            temp["shop_id"] = shop_id
            formaldehyde = post.get("formaldehyde_" + shop_id)
            temp["formaldehyde"] = formaldehyde
            if formaldehyde < 0 or not str(formaldehyde).replace(".", "", 1).isdigit():
                pass
            else:
                result.append(temp)
    # Logs.print_log("formaldehyde data", result)
    return result


@permission_classes((AllowAny,))
def update_shop_formaldehyde(shop, formaldehyde_image=None):
    try:
        # Logs.print_log("formaldehyde image", formaldehyde_image)
        s = Shop.objects.get(id=shop["shop_id"])
        s.formaldehyde = shop["formaldehyde"]
        if formaldehyde_image:
            # path = "shop/formaldehyde/" + str(formaldehyde_image)
            s.formaldehyde_image = formaldehyde_image
            # img = Image.open(formaldehyde_image)
            # img.save(formaldehyde_image)
        s.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


@permission_classes((AllowAny,))
def add_formaldehyde_actions(request):
    sort = request.data.get("sort", None)
    shop_name = request.data.get("shop_name", None)
    shop_city = request.data.get("shop_city", None)
    shop_address = request.data.get("shop_address", None)
    if not sort:
        shop_list = parse_add_formaldehyde_post(request)
        for s in shop_list:
            update_shop_formaldehyde(s)

    return HttpResponseRedirect("/shop/editor/add?shop_name="
                                + shop_name + "&shop_city=" + shop_city + "&shop_address=" + shop_address)


def add_shop_formaldehyde_4_editor_interface(request):
    # return HttpResponse(request.GET)
    shop_name = request.GET.get("shop_name", None)
    shop_city = request.GET.get("shop_city", u"北京")
    shop_address = request.GET.get("shop_address", None)
    if not shop_name:
        shop_name = ""
    if not shop_address:
        shop_address = ""

    shops = get_shop_data_4_add_formaldehyde(shop_name, shop_city, shop_address)
    return render(request, "editor/add_formaldehyde.html", {
        "shops_info": shops, "name": shop_name, "city": shop_city, "address": shop_address})


def add_formaldehyde_4_publish_actions(request):
    sort = request.data.get("sort", None)
    shop_name = request.data.get("shop_name", None)
    shop_city = request.data.get("shop_city", None)
    shop_address = request.data.get("shop_address", None)

    # Logs.print_log("POST", request.data)

    username = request.data.get("username", None)
    formaldehyde_image = request.FILES.get("formaldehyde_image")
    # Logs.print_log("files", request.FILES)
    # # Logs.print_log("formaldehyde image in post", formaldehyde_image.__dict__)
    # # Logs.print_log("formaldehyde image in post file", formaldehyde_image.file)
    formaldehyde_date = request.data.get("formaldehyde_checked_date")
    formaldehyde_time = request.data.get("formaldehyde_checked_time")
    formaldehyde_content = request.data.get("formaldehyde_content")
    if not sort:
        shop_list = parse_add_formaldehyde_post(request)
        for s in shop_list:
            # print "start shop_list"
            update_shop_formaldehyde(s, formaldehyde_image)
            try:
                formaldehyde = s["formaldehyde"]
                shop_id = s["shop_id"]
                user_id = User.objects.get(username=username).id
                formaldehyde_datetime = formaldehyde_date + " " + formaldehyde_time + ":00"
                checked_at = Datetimes.string_to_clock(formaldehyde_datetime)
                p = Publish(user_id=user_id, checked_at=checked_at, formaldehyde=formaldehyde,
                            big_image=formaldehyde_image, shop_id=shop_id, content=formaldehyde_content)
                p.save()
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)

    return HttpResponseRedirect("/shop/editor/publish/add?shop_name="
                                + shop_name + "&shop_city=" + shop_city + "&shop_address=" + shop_address)


def add_shop_formaldehyde_4_publish_editor_interface(request):
    # return HttpResponse(request.GET)
    shop_name = request.GET.get("shop_name", None)
    shop_city = request.GET.get("shop_city", u"北京")
    shop_address = request.GET.get("shop_address", None)
    if not shop_name:
        shop_name = ""
    if not shop_address:
        shop_address = ""

    shops = get_shop_data_4_add_formaldehyde(shop_name, shop_city, shop_address)
    # shops = []
    return render(request, "editor/add_formaldehyde_4_publish.html", {
        "shops_info": shops, "name": shop_name, "city": shop_city, "address": shop_address})


# **************************************添加甲醛
# *********************************publish小编审核**********************************************
# ********************************地图**********************************************


def get_data_from_shops():
    shops = Shop.objects.all()
    return [get_data_from_one_shop(s) for s in shops if get_data_from_one_shop(s)]


def get_data_from_one_shop(s):
    if s:
        if s.dianping_business_id:
            dianping = get_data_from_dianping(s)
            return dianping
        else:
            db = get_data_from_db(s)
            return db
    else:
        return tuple()


def get_data_from_dianping(s):
    result = []
    pm2_5 = get_today_latest_PM2_5(s)
    if pm2_5 is None:
        return tuple()
    result.append(s.id)
    result.append(s.dianping_name)
    result.append(s.dianping_address)
    result.append(pm2_5)
    result.append(s.dianping_longitude)
    result.append(s.dianping_latitude)
    result.append(Datetimes.clock_to_string(Datetimes.transfer_datetime(s.created_at)))
    result.append(Datetimes.clock_to_string(Datetimes.transfer_datetime(s.changed_at)))
    result.append(s.dianping_s_photo_url)
    result.append(s.dianping_avg_price)
    if s.dianping_regions:
        result.append(s.dianping_regions.split(",")[0][1:])
    else:
        result.append("")
    result.append(0)
    return tuple(result)


def get_data_from_db(s):
    result = []
    pm2_5 = get_today_latest_PM2_5(s)
    if pm2_5 is None:
        return tuple()
    result.append(s.id)
    result.append(s.name)
    result.append(s.address.detail_address)
    result.append(pm2_5)
    result.append(s.address.longitude)
    result.append(s.address.latitude)
    result.append(Datetimes.clock_to_string(Datetimes.transfer_datetime(s.created_at)))
    result.append(Datetimes.clock_to_string(Datetimes.transfer_datetime(s.changed_at)))
    result.append(s.s_photo_url)
    result.append(0)
    result.append("")
    result.append(0)
    return tuple(result)


def get_today_latest_PM2_5(s):
    result = {}
    today_start = get_today_start()
    yesterday = Datetimes.get_some_day(2)
    yesterday_start = Datetimes.get_day_start(yesterday)

    time_start = yesterday_start
    pms = Publishes.get_shown_publishes().filter(PM2_5__gte=0, created_at__gte=time_start, shop_id=s.id).order_by(
        "-id").values("PM2_5")
    if pms:
        return pms[0]["PM2_5"]
    return None


@api_view(http_method_names=["GET"])
def generate_csv_file(request):
    data = get_data_from_shops()
    # print data
    data = Characters.unicode_to_concrete(data)
    Files.save_csv(data)
    return Response(data)


# *********************************地图**********************************************


# *********************************添加分类**********************************************


def add_category(categories, parent_name):
    try:
        if parent_name:
            parent_id = ShopCategory.objects.get(name=parent_name).id
        else:
            parent_id = 0
        category_list = []
        category_names = categories.split(',')
        exist_names = []
        for name in category_names:
            category_exist = ShopCategory.objects.filter(name=name)
            if category_exist.count() == 1:
                exist_names.append(name)
                continue
            c = ShopCategory(name=name, parent_id=parent_id)
            category_list.append(c)
        ShopCategory.objects.bulk_create(category_list)
        # ShopCategory.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


def add_head():
    add_category('全部', None)


def add_big_category():
    add_category('美食,运动,酒店,丽人,娱乐,亲子,其他,公司,家,购物', '全部')


def add_food():
    add_category('火锅,小吃快餐,湘菜,新疆菜,烧烤,自助,江浙菜,湖北菜,咖啡厅,日本,\
    东北菜,面包甜点,西餐,素菜,韩国料理,北京菜,清真菜,粤菜,东南亚菜,\
    云南菜,海鲜,川菜,西北菜	', '美食')


def add_sport():
    add_category('健身中心,瑜伽,舞蹈,游泳馆,台球馆,羽毛球馆,高尔夫球场,武术场所,篮球场,\
    足球场,马术场,保龄球馆,乒乓球馆,体育场所,滑雪场,更多', '运动')


def add_hotel():
    add_category('经济型酒店,五星级酒店,四星级酒店,青年旅社,公寓式酒店,三星级酒店,度假村,精品酒店,客栈旅舍,农家院', '酒店')


def add_beauty():
    add_category('美发,瑜伽,美甲,美容/SPA,个性写真,瘦身纤体,齿科,美睫,整形,化妆品,纹身,产后塑形', '丽人')


def add_other():
    add_category('景点/郊游,温泉,动植物园,游乐场,建材,文化艺术,古镇,影视基地,漂流,家用电器,中医养生,\
    采摘/农家乐,公园,银行,医院,洗车,快照/冲印,家政,洗衣店,电信营业厅,维修保养,体检中心,家具家居,\
    厨房卫浴,家装卖场,4S/店/汽车销售', '其他')


def add_entertainment():
    add_category('服饰鞋包,超市/便利店,综合商场,药店,私人影院,洗浴,密室,茶馆,桌面游戏,眼镜店,花店,\
    化妆品,特色集市,KTV,DIY手工坊,轰趴馆,真人cs,珠宝饰品,数码产品,运动户外,电影院,足疗按摩,酒吧,\
    网吧网咖,游乐游艺,棋牌室', '娱乐')


def add_paternity():
    add_category('亲子摄影,幼儿教育,亲子游乐,孕妇写真,孕产护理,亲子购物,书店', '亲子')


# def add_market():
#     add_category('超市/便利店,综合商场', '购物')


def add_category_all(request):
    add_head()
    add_big_category()
    add_food()
    add_sport()
    add_hotel()
    add_beauty()
    add_entertainment()
    add_paternity()
    add_other()

    return HttpResponse("add category successfully")


# 修改新的分类--购物
# 修改 超市/便利店和综合商场 的父类为购物


# *********************************添加分类**********************************************
# *********************************用户是否登录**********************************************


@api_view(http_method_names=["GET"])
def is_login_from_app_by_username(request):
    username = request.GET.get("username", None)
    if not username:
        return Response(False)
    user = Users.get_user(username)
    if user:
        return Response(Users.does_user_login(user.id))
    else:
        return Response(False)


# *********************************用户是否登录**********************************************

# ****************************forum*************************************************

STRING_4_IMAGE = "!@#$%^"


def is_after_yesterday(created_date):
    yesterday = Datetimes.get_some_day(1)
    created_date = created_date.replace(tzinfo=None)
    if created_date > yesterday:
        return True
    else:
        return False


def get_show_time(created_date):
    created_date = created_date.replace(tzinfo=None)
    now = datetime.datetime.now()
    delta = now - created_date
    seconds = delta.seconds - 3600 * 8
    days = delta.days
    if days > 2:
        return Datetimes.transfer_datetime(created_date).strftime("%Y/%m/%d")
    elif days > 1:
        return u"前天"
    elif days > 0:
        return u"昨天"
    else:
        hour = seconds / 3600
        if hour > 0:
            return str(hour) + u"小时前"
        else:
            return u"刚刚"


def is_category_hot(category_id):
    return False


def get_forum_category_count(category_id):
    result = {}
    posts = ForumPost.objects.filter(status=True, category_id=category_id)
    post_count = posts.count()
    result["post_count"] = post_count
    reply_count = 0
    for p in posts:
        replies = ForumReply.objects.filter(status=True, post_id=p.id)
        reply_count += replies.count()

    result["reply_count"] = reply_count
    result["total"] = post_count + reply_count
    return result


def get_forum_category_info(c, user, user_is_login):
    d = dict()

    d = Forums.get_category_info(c)

    d["is_login"] = user_is_login
    d["is_concerned"] = False

    if user_is_login:
        if user:
            ue = Users.get_user_extension(user.username)
            category = ue.forum_category.filter(id=c.id)
            if category.count() > 0:
                d["is_concerned"] = True

    yesterday = Datetimes.get_some_day(1, 0)
    yesterday = Datetimes.naive_to_aware(yesterday)
    if yesterday > c.created_at:
        d["is_new"] = False
    else:
        d["is_new"] = True

    d["is_hot"] = False
    d["next_url"] = files.BASE_POST_LIST_URL + str(c.id) + "/"

    forum_category_count = Forums.get_forum_category_count(c)
    d["total"] = forum_category_count["total"]
    return d


def is_login_by_app(user_id, key):
    if not (user_id and key):
        # print "miss user_id or key"
        return False
    token = Token.objects.filter(key=key)
    if token.count() == 1:
        if int(user_id) == token[0].user_id:
            return True
        else:
            # print "user_id:" + str(user_id) + ", token_user_id:" + str(token[0].user_id)
            return False
    else:
        return False


def get_category_data(fc, user, user_is_login):
    data_concerned = []
    data_not_concerned = []
    ue = Users.get_user_extension(user.username)
    for c in fc:
        if user_is_login:
            if c in ue.forum_category.all():
                data_concerned.append(get_forum_category_info(c, user, user_is_login))
            else:
                data_not_concerned.append(get_forum_category_info(c, user, user_is_login))
        else:
            data_not_concerned.append(get_forum_category_info(c, user, user_is_login))
    return data_concerned + data_not_concerned


@api_view(http_method_names=["GET"])
def get_category_data_interface(request):
    fc = Forums.get_valid_categories().order_by("-weight", "-id")
    user_id = request.GET.get("user_id", 0)
    key = request.GET.get("key", None)
    user = None
    user_is_login = Users.check_user_login(user_id, key)

    data = dict()

    if user_id:
        try:
            user = Users.get_user_by_id(user_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
    data["data"] = get_category_data(fc, user, user_is_login)
    return Response(data)


def get_category_carousel():
    result = list()
    carousels = ForumCategoryCarousel.objects.all().order_by("-weight", "-id")
    for c in carousels:
        temp = dict()
        temp["url"] = c.url_address
        temp["descriptions"] = c.descriptions
        temp["image"] = make_image_url(c.image.name)
        result.append(temp)
    return result


def show_category(request):
    fc = ForumCategory.objects.filter(status=True).order_by("-weight", "-id")
    user_id = request.GET.get("user_id", 0)
    key = request.GET.get("key", None)
    user = get_user(user_id)
    user_is_login = is_login_by_app(user_id, key)

    data = get_category_data(fc, user, user_is_login)
    carousel = get_category_carousel()

    return render(request, "forum/category_menu.html", {
        "categories": data,
        "user_id": user_id,
        "key": key,
        "carousel": carousel
    })


def get_forum_post_reply_count(post_id):
    categories = ForumReply.objects.filter(status=True, post_id=post_id)
    return categories.count()


def make_string_short(text, length):
    text = text.replace(STRING_4_IMAGE, "")
    if len(text) > length:
        return text[0:length] + "..."
    else:
        return text


def combine_text_and_image(text, image0, image1, image2, image3, image4, image5, image6, image7, image8):
    for i in range(10):
        try:
            if i == 0:
                img_src = image0
            elif i == 1:
                img_src = image1
            elif i == 2:
                img_src = image2
            elif i == 3:
                img_src = image3
            elif i == 4:
                img_src = image4
            elif i == 5:
                img_src = image5
            elif i == 6:
                img_src = image6
            elif i == 7:
                img_src = image7
            elif i == 8:
                img_src = image8
            image_string = '<img src="' + img_src + '" alt="picture" />'

            text = text.replace(STRING_4_IMAGE, image_string, 1)
        except:
            break
    return text


def make_image_url(name, new_width=240):
    if name:
        try:
            # Logs.print_log("name", name)
            source_image_url = files.BASE_URL_4_IMAGE + name
            name_parts = name.split(".")
            source_name_extension = None
            source_name_name = name_parts[0]

            middle_parts = "/".join(name.split("/")[:-1])

            if len(name_parts) > 1:
                source_name_extension = name_parts[1]

            new_name_name = source_name_name + "_small"
            new_name = new_name_name + "." + source_name_extension
            # Logs.print_log("new name", files.BASE_URL_4_IMAGE + new_name)
            if os.path.exists("media_root/" + new_name):
                # Logs.print_log("new name exist", new_name)
                return files.BASE_URL_4_IMAGE + new_name
            else:
                # Logs.print_log("new name does not exist", new_name)
                resize_img(source_image_url, new_name, new_width, middle_parts)

            # if source_name_extension == "jpg":
            #     new_name_name = source_name_name + "_small"
            #
            #     new_name = new_name_name + "." + source_name_extension
            #
            #     resize_img(source_image_url, new_name, new_width, middle_parts)
            # else:
            #     return source_image_url

            return files.BASE_URL_4_IMAGE + new_name
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return files.BASE_URL_4_IMAGE + name


def make_download_url(name):
    filename = files.MEDIA_URL_NAME + "/" + name
    if os.path.exists(filename):
        return filename
    else:
        return None


def get_post_info(post_instance, flag=True):
    d = dict()
    d["id"] = post_instance.id
    d["next_url"] = files.BASE_REPLY_LIST_URL + str(post_instance.id) + "/"

    d["is_category_rule"] = post_instance.is_category_rule
    d["is_digest"] = post_instance.is_digest
    d["is_top"] = post_instance.is_top
    d["top_weight"] = post_instance.top_weight
    if post_instance.owner:
        d["user_nickname"] = post_instance.owner.userextension.nickname
    else:
        d["user_nickname"] = ""
    temp_image = get_user_image(post_instance.owner)
    d["big_user_image"] = temp_image["big_user_image"]
    d["small_user_image"] = temp_image["small_user_image"]
    d["created_at"] = post_instance.created_at
    d["show_time"] = get_show_time(post_instance.created_at)
    d["reply_count"] = get_forum_post_reply_count(post_instance.id)
    d["win_count"] = post_instance.win_users.count()
    win_users = post_instance.win_users.all()
    win_users_id = list()
    for u in win_users:
        win_users_id.append(u.id)
    d["win_users"] = win_users_id
    d["category_id"] = post_instance.category_id
    d["category_name"] = post_instance.category.name

    d["image1"] = make_image_url(post_instance.post_image_1.name)
    d["image2"] = make_image_url(post_instance.post_image_2.name)
    d["image3"] = make_image_url(post_instance.post_image_3.name)
    d["image4"] = make_image_url(post_instance.post_image_4.name)
    d["image5"] = make_image_url(post_instance.post_image_5.name)
    d["image6"] = make_image_url(post_instance.post_image_6.name)
    d["image7"] = make_image_url(post_instance.post_image_7.name)
    d["image8"] = make_image_url(post_instance.post_image_8.name)
    d["image9"] = make_image_url(post_instance.post_image_9.name)

    d["download_image1"] = make_download_url(post_instance.post_image_1.name)
    d["download_image2"] = make_download_url(post_instance.post_image_2.name)
    d["download_image3"] = make_download_url(post_instance.post_image_3.name)
    d["download_image4"] = make_download_url(post_instance.post_image_4.name)
    d["download_image5"] = make_download_url(post_instance.post_image_5.name)
    d["download_image6"] = make_download_url(post_instance.post_image_6.name)
    d["download_image7"] = make_download_url(post_instance.post_image_7.name)
    d["download_image8"] = make_download_url(post_instance.post_image_8.name)
    d["download_image9"] = make_download_url(post_instance.post_image_9.name)

    if flag:
        d["title"] = make_string_short(post_instance.title, 30)
        d["content"] = make_string_short(post_instance.content, 80)
    else:
        d["title"] = post_instance.title
        d["content"] = post_instance.content.split(STRING_4_IMAGE)
        d["content_length"] = len(d["content"])

    return d


def get_one_reply_info(r, layer):
    d = dict()
    d["id"] = r.id
    d["content"] = r.content
    d["created_at"] = r.created_at
    d["show_time"] = get_show_time(r.created_at)
    d["owner_nickname"] = r.owner.userextension.nickname
    temp_image = get_user_image(r.owner)
    d["big_owner_image"] = temp_image["big_user_image"]
    d["small_owner_image"] = temp_image["small_user_image"]
    d["parent_reply"] = r.parent_reply
    if r.parent_reply == 0:
        d["parent_reply_content"] = ""
        d["parent_reply_owner_nickname"] = ""
    else:
        reply = ForumReply.objects.filter(id=r.parent_reply)
        if reply.count() > 0:
            d["parent_reply_content"] = make_string_short(reply[0].content, 80)
            d["parent_reply_owner_nickname"] = reply[0].owner.userextension.nickname
        else:
            d["parent_reply_content"] = ""
            d["parent_reply_owner_nickname"] = ""
    d["layer"] = layer

    d["image1"] = make_image_url(r.reply_image_1.name, 1280)
    d["image2"] = make_image_url(r.reply_image_2.name, 1280)
    d["image3"] = make_image_url(r.reply_image_3.name, 1280)
    d["image4"] = make_image_url(r.reply_image_4.name, 1280)
    d["image5"] = make_image_url(r.reply_image_5.name, 1280)
    d["image6"] = make_image_url(r.reply_image_6.name, 1280)
    d["image7"] = make_image_url(r.reply_image_7.name, 1280)
    d["image8"] = make_image_url(r.reply_image_8.name, 1280)
    d["image9"] = make_image_url(r.reply_image_9.name, 1280)

    d["download_image1"] = make_image_url(r.reply_image_1.name)
    d["download_image2"] = make_image_url(r.reply_image_2.name)
    d["download_image3"] = make_image_url(r.reply_image_3.name)
    d["download_image4"] = make_image_url(r.reply_image_4.name)
    d["download_image5"] = make_image_url(r.reply_image_5.name)
    d["download_image6"] = make_image_url(r.reply_image_6.name)
    d["download_image7"] = make_image_url(r.reply_image_7.name)
    d["download_image8"] = make_image_url(r.reply_image_8.name)
    d["download_image9"] = make_image_url(r.reply_image_9.name)
    return d


def get_reply_info(p, start_id=0, count=20, status=True):
    result_dict = dict()
    fr = ForumReply.objects.filter(status=status, post_id=p.id).order_by("id")
    total = fr.count()
    print("reply total", total)

    if count == 0:
        count = total

    result = []
    layer = 0
    pre_index = 0
    index = 0
    next_start_id = -1

    # Logs.print_log("start id", start_id)
    # Logs.print_log("count", count)

    for r in fr:
        layer += 1
        if start_id:
            if r.id < start_id:
                pre_index += 1
                continue

        index += 1
        if index > count:
            next_start_id = r.id
            break

        d = get_one_reply_info(r, layer)
        result.append(d)

    # Logs.print_log("reply result", result)
    if total - pre_index <= count:
        next_start_id = -1
    result_dict["start_id"] = next_start_id
    result_dict["data"] = result

    return result_dict


@api_view(http_method_names=["GET"])
def get_post_list_data(request):
    result_dict = dict()
    result = list()
    category_id = int(request.GET.get("category_id", 0))
    start_id = int(request.GET.get("start_id", 0))
    count = int(request.GET.get("count", 20))
    index = 0
    pre_index = 0
    next_start_id = -1
    if category_id:
        ofp = ForumPost.objects.filter(status=True, category_id=category_id, is_top=False).order_by("-id")
        total = ofp.count()
        for p in ofp:
            if start_id:
                if p.id < start_id:
                    result.append(get_post_info(p))
                    index += 1
                    if index == count:
                        next_start_id = p.id
                        break
                else:
                    pre_index += 1
            else:
                result.append(get_post_info(p))
                index += 1
                if index == count:
                    next_start_id = p.id
                    break
        if total <= 20 or (total - pre_index) <= 20:
            next_start_id = -1
    result_dict["data"] = result
    result_dict["start_id"] = next_start_id
    return Response(result_dict)


@api_view(http_method_names=["GET"])
def get_reply_list_data(request):
    post_id = int(request.GET.get("post_id", 0))
    start_id = int(request.GET.get("start_id", 0))
    count = int(request.GET.get("count", 20))
    result = dict()

    posts = ForumPost.objects.filter(id=post_id)

    if posts:
        p = posts[0]
        result = get_reply_info(p, start_id, count)
    else:
        result["data"] = []
        result["start_id"] = -1
    return Response(result)


def show_post_list(request, category_id, page_count=3):
    user_id = request.GET.get("user_id", 0)
    key = request.GET.get("key", None)
    user = get_user(user_id)
    user_is_login = is_login_by_app(user_id, key)

    c = ForumCategory.objects.filter(id=category_id)
    if c.count() > 0:
        category = get_forum_category_info(c[0], user, user_is_login)
    else:
        return HttpResponseRedirect("/forum/index")
    category_count = get_forum_category_count(category_id)

    # 置顶排序
    tfp = ForumPost.objects.filter(status=True, category_id=category_id, is_top=True).order_by("-top_weight")
    data = []
    for p in tfp:
        data.append(get_post_info(p))

    ofp = ForumPost.objects.filter(status=True, category_id=category_id, is_top=False).order_by("-id")
    total = ofp.count()
    count = 0
    for p in ofp:
        data.append(get_post_info(p))
        count += 1
        if count == page_count:
            start_id = p.id
            break
    if total <= page_count:
        start_id = -1

    response = render(request, "forum/post_index.html",
                      {
                          "posts": data,
                          "category": category,
                          "category_count": category_count,
                          "start_id": start_id,
                          # "category_id": category_id,
                          "user_id": user_id,
                          "key": key
                      })
    return response


def show_post_detail_function(request, post_id, render_html):
    user_id = request.GET.get("user_id", 0)
    key = request.GET.get("key", None)
    category_id = request.GET.get("category_id", 0)
    start_id = request.GET.get("start_id", 0)
    count = request.GET.get("count", 20)

    user = get_user(user_id)
    user_is_login = is_login_by_app(user_id, key)
    posts = ForumPost.objects.filter(id=post_id)
    # start_id = -1
    if posts:
        p = posts[0]
        post = get_post_info(p, False)

        post["win_users_nickname"] = []
        for u in post["win_users"]:
            user_extension = UserExtension.objects.get(id=u)
            post["win_users_nickname"].append(user_extension.nickname)
        post["win_users_nickname"] = u"、 ".join(post["win_users_nickname"])
        post["win_users_count"] = len(post["win_users"])

        if user_is_login:
            post["is_win"] = False
            for u in post["win_users"]:
                user_extension = UserExtension.objects.get(id=u)
                if str(user_extension.user.username) == str(user):
                    # print "is_win is True"
                    post["is_win"] = True
                    break
        else:
            post["is_win"] = False

        replies = get_reply_info(p, start_id, count)
        reply = replies["data"]
        # Logs.print_log("reply", reply)
        start_id = replies["start_id"]
    else:
        post = {"title": "", "content": ""}
        reply = []

    if start_id:
        pass
    else:
        start_id = -1
    response = render(request, render_html,
                      {
                          "post": post,
                          "reply": reply,
                          "start_id": start_id,
                          # "post_id": post_id,
                          "user_id": user_id,
                          "key": key,
                          # "category_id": category_id
                      })
    return response


def show_post_detail(request, post_id):
    return show_post_detail_function(request, post_id, "forum/post_content.html")


def show_post_detail_share(request, post_id):
    return show_post_detail_function(request, post_id, "forum/share_content.html")


def show_post_create(request):
    user_id = request.GET.get("user_id", 0)
    category_id = request.GET.get("category_id", 0)
    key = request.GET.get("key", None)
    user_is_login = is_login_by_app(user_id, key)
    response = None
    if user_is_login:
        response = render(request, "forum/post_create.html",
                          {
                              "user_id": user_id,
                              "category_id": category_id,
                              "key": key
                          }
                          )
    else:
        response_string = u"请先从APP中登录！ <a href=\"/forum/index?user_id=" \
                          + user_id + u"&key=" + key + u"\">返回版块列表</a>"
        response = HttpResponse(response_string)

    return response


def get_user(user_id):
    users = User.objects.filter(id=user_id)
    if users.count() > 0:
        return users[0]
    else:
        return None


def get_forum_category(forum_category_id):
    forum_categories = ForumCategory.objects.filter(id=forum_category_id, status=True)
    if forum_categories.count() > 0:
        return forum_categories[0]
    else:
        return None


def get_forum_post(forum_post_id):
    forum_posts = ForumPost.objects.filter(id=forum_post_id, status=True)
    if forum_posts.count() > 0:
        return forum_posts[0]
    else:
        return None


# 添加关注
@execute_time
@api_view(http_method_names=["POST"])
def add_forum_category_concern(request):
    result = {}
    post = request.data
    # Logs.print_log("post", post)
    user_id = post.get("user_id", 0)
    key = post.get("key", None)
    user_is_login = is_login_by_app(user_id, key)
    result["is_login"] = user_is_login
    if not user_is_login:
        result["success"] = False
        result["info"] = "please login firstly"
        return Response(result)

    # Logs.print_log("user_id", user_id)
    if user_id:
        user = get_user(user_id)
        if user:
            user_extension = user.userextension
        else:
            result["success"] = False
            result["info"] = "user_id=" + str(user_id) + ", no user"
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no user_id"
        return Response(result)

    forum_category_id = post.get("forum_category_id", 0)
    # Logs.print_log("forum category id", forum_category_id)
    if forum_category_id:
        forum_category = get_forum_category(forum_category_id)
        if forum_category:
            pass
        else:
            result["success"] = False
            result["info"] = "forum_category_id=" + str(forum_category_id) + ", no forum_category"
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no forum_category_id"
        return Response(result)

    if forum_category in user_extension.forum_category.all():
        result["success"] = True
        result["info"] = "user has concerned the category"
        return Response(result)

    user_extension.forum_category.add(forum_category)
    user_extension.save()
    result["success"] = True
    result["info"] = "OK"
    # print result
    return Response(result)


# 取消关注
@execute_time
@api_view(http_method_names=["POST"])
def cancel_forum_category_concern(request):
    result = {}
    post = request.data
    user_id = post.get("user_id", 0)
    key = post.get("key", None)
    user_is_login = is_login_by_app(user_id, key)
    result["is_login"] = user_is_login
    if not user_is_login:
        result["success"] = False
        result["info"] = "please login firstly"
        return Response(result)

    if user_id:
        user = get_user(user_id)
        if user:
            user_extension = user.userextension
        else:
            result["success"] = False
            result["info"] = "user_id=" + str(user_id) + ", but no user"
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no user_id"
        return Response(result)

    forum_category_id = post.get("forum_category_id", 0)
    if forum_category_id:
        forum_category = get_forum_category(forum_category_id)
        if forum_category:
            pass
        else:
            result["success"] = False
            result["info"] = "forum_category_id=" + str(forum_category_id) + ", but no forum_category"
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no forum_category_id"
        return Response(result)

    if forum_category in user_extension.forum_category.all():
        user_extension.forum_category.remove(forum_category)
        user_extension.save()
        result["success"] = True
        result["info"] = "OK"
        return Response(result)
    result["success"] = True
    result["info"] = "forum_category is not concerned"
    return Response(result)


@execute_time
@api_view(http_method_names=['POST'])
def add_forum_post_win(request):
    result = {}
    post = request.data
    user_id = post.get("user_id", 0)
    key = post.get("key", None)
    user_is_login = is_login_by_app(user_id, key)
    result["is_login"] = user_is_login
    if not user_is_login:
        result["success"] = False
        result["info"] = "please login firstly"
        return Response(result)

    if user_id:
        user = get_user(user_id)
        if user:
            user_extension = user.userextension
        else:
            result["success"] = False
            result["info"] = "user_id=" + str(user_id) + ", but no user."
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no user_id."
        return Response(result)

    forum_post_id = post.get("forum_post_id", 0)
    if forum_post_id:
        forum_post = get_forum_post(forum_post_id)
        if forum_post:
            pass
        else:
            result["success"] = False
            result["info"] = "forum_post_id=" + str(forum_post_id) + ", but no forum_post."
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no forum_post_id."
        return Response(result)

    if user_extension in forum_post.win_users.all():
        result["success"] = True
        result["info"] = "you have done this before"
        result["nickname"] = user_extension.nickname
        return Response(result)

    forum_post.win_users.add(user_extension)
    forum_post.save()
    result["success"] = True
    result["info"] = "OK"
    result["nickname"] = user_extension.nickname
    return Response(result)


@execute_time
@api_view(http_method_names=['POST'])
def cancel_forum_post_win(request):
    result = {}
    post = request.data
    user_id = post.get("user_id", 0)
    key = post.get("key", None)
    user_is_login = is_login_by_app(user_id, key)
    result["is_login"] = user_is_login
    if not user_is_login:
        result["success"] = False
        result["info"] = "please login firstly"
        return Response(result)

    if user_id:
        user = get_user(user_id)
        if user:
            user_extension = user.userextension
        else:
            result["success"] = False
            result["info"] = "user_id=" + str(user_id) + ", but no user."
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no user_id."
        return Response(result)

    forum_post_id = post.get("forum_post_id", 0)
    if forum_post_id:
        forum_post = get_forum_post(forum_post_id)
        if forum_post:
            pass
        else:
            result["success"] = False
            result["info"] = "forum_post_id=" + str(forum_post_id) + ", but no forum_post."
            return Response(result)
    else:
        result["success"] = False
        result["info"] = "no forum_post_id."
        return Response(result)

    if user_extension in forum_post.win_users.all():
        forum_post.win_users.remove(user_extension)
        forum_post.save()
        result["success"] = True
        result["info"] = "OK"
        result["nickname"] = user_extension.nickname
        return Response(result)

    result["success"] = True
    result["info"] = "you have not concerned the post before(do nothing)"
    result["nickname"] = user_extension.nickname
    return Response(result)


def get_current_timestamp():
    now = datetime.datetime.now()
    now_tuple = now.timetuple()
    timestamp = int(time.mktime(now_tuple))
    return timestamp


def convert_base64_to_image(base64string, base64type="png", part="post"):
    img_data = base64.b64decode(base64string)
    timestamp = get_current_timestamp()
    timestamp_image = str(timestamp) + "." + base64type
    path = get_picture_path(timestamp_image, part, part)
    img = open(path, "wb")
    img.write(img_data)
    img.close()
    return part + "/" + part + "_" + timestamp_image


def save_base64_to_image(base64_array, part):
    result = []
    # base64_array = request.data.get("base64_array", None)
    # part = request.data.get("part", "post")
    if not base64_array:
        return result
    for b in base64_array:
        base64s = b.split(";base64,")
        if len(base64s) > 1:
            base64string = base64s[1]
            base64type = base64s[0].split("/")[-1]
        else:
            base64string = base64s[0]
            base64type = "png"
        result.append(convert_base64_to_image(base64string, base64type, part))
    return result


@api_view(http_method_names=["POST"])
def post_save_base64_to_image(request):
    base64_array = request.data.get("base64_array", None)
    part = "post"
    return Response(save_base64_to_image(base64_array, part))


@api_view(http_method_names=["POST"])
def reply_save_base64_to_image(request):
    base64_array = request.data.get("base64_array", None)
    part = "reply"
    return Response(save_base64_to_image(base64_array, part))


# ****************************forum*************************************************

# *********************************友盟推送**********************************************


def md5(s):
    m = hashlib.md5(s)
    return m.hexdigest()


# def push_unicast(app_key, app_master_secret, device_token, ticker, title, text,
#                  after_open='go_app', display_type='notification'):
#     timestamp = int(time.time() * 1000)
#     method = 'POST'
#     url = 'http://msg.umeng.com/api/send'
#     params = {'appkey': app_key,
#               'timestamp': timestamp,
#               'device_tokens': device_token,
#               'type': 'unicast',
#               'payload': {
#                   'body': {
#                       "aps": {
#                           "alert": "xxx"
#                       },  # iOS必备参数
#                       'ticker': ticker,
#                       'title': title,
#                       'text': text,
#                       'after_open': after_open
#                   },
#                   'display_type': display_type
#               }
#               }
#     post_body = json.dumps(params)
#     # print post_body
#     sign = md5('%s%s%s%s' % (method, url, post_body, app_master_secret))
#     try:
#         r = urllib2.urlopen(url + '?sign=' + sign, data=post_body)
#         return r.read()
#     except urllib2.HTTPError, e:
#         return e.reason, e.read()
#     except urllib2.URLError, e:
#         return e.reason
#
#
# @api_view(http_method_names=["POST"])
# def youmeng_push(request):
#     device_token = request.data.get("device_token", None)
#     ticker = request.data.get("ticker", None)
#     title = request.data.get("title", None)
#     text = request.data.get("text", None)
#     after_open = request.data.get("after_open", "go_app")
#     display_type = request.data.get("display_type", "notification")
#     push_unicast(YOUMENG_APP_KEY, YOUMENG_APP_MASTER_SECRET,
#                  device_token, ticker, title, text, after_open, display_type)


def attributes_count_compute(win_account, sub_win_account):
    if win_account > sub_win_account:
        return win_account - sub_win_account
    else:
        return 0


def update_user_unread_win_and_lost(user, win, lost):
    win_count = int(user.userextension.win_count)
    lost_count = int(user.userextension.lost_count)
    win = int(win)
    lost = int(lost)
    win_count = attributes_count_compute(win_count, win)
    lost_count = attributes_count_compute(lost_count, lost)
    user.userextension.win_count = win_count
    user.userextension.lost_count = lost_count
    user.userextension.save()


def clear_publish_unread_win_and_lost(publish_object):
    publish_object.win_count = 0
    publish_object.lost_count = 0
    publish_object.save()


def increment_unread_win_or_lost(obj, is_win, is_change):
    if is_change:
        if is_win:
            obj.win_count += 1
            obj.lost_count -= 1
        else:
            obj.win_count -= 1
            obj.lost_count += 1
    else:
        if is_win:
            obj.win_count += 1
        else:
            obj.lost_count += 1
    obj.save()


def decrease_unread_win_or_lost(obj, is_win):
    if is_win:
        obj.win_count -= 1
    else:
        obj.lost_count -= 1
    obj.save()


def win_or_lost_push(publish_object, is_win, is_change=False):
    Methods.increment_unread_win_or_lost(publish_object, is_win, is_change)
    ue = Users.get_user_extension(publish_object.user.username)
    Methods.increment_unread_win_or_lost(ue, is_win, is_change)


@api_view(http_method_names=["PUT"])
def clear_unread_win_and_lost(request):
    result = dict()
    publish_id = request.data.get("publish_id", 0)
    if publish_id:
        publish = Publishes.get(publish_id)
        if publish:
            win = publish.win_count
            lost = publish.lost_count
            Publishes.reset_win_and_lost_count(publish)
            ue = Users.get_user_extension(publish.user.username)
            if ue:
                Users.update_unread_win_and_lost_count(ue, win, lost)
            result["success"] = True
    result["success"] = False
    return Response(result)


# @api_view(http_method_names=["POST"])
# def push_win_and_lost(request):
#     result = dict()
#     user_id = request.data.get("user_id", 0)
#     user = Users.get_user_by_id(user_id)
#
#     if not user:
#         result["success"] = False
#         return Response(result)
#
#     ue = Users.get_user_extension(user.username)
#     win_count = ue.win_count
#     lost_count = ue.lost_count
#
#     if ue.device_token:
#         if int(win_count) == 0 and int(lost_count) == 0:
#             pass
#         else:
#             push_unicast(
#                 pushes.YOUMENG_APP_KEY,
#                 pushes.YOUMENG_APP_MASTER_SECRET,
#                 ue.device_token,
#                 pushes.WIN_AND_LOST_APP_TICKER,
#                 pushes.WIN_AND_LOST_APP_TITLE,
#                 pushes.WIN_AND_LOST_APP_TEXT % (win_count, lost_count))
#         result["success"] = True
#     else:
#         result["success"] = False
#     return Response(result)


# @api_view(http_method_names=["POST"])
# def push_win_and_lost_ios(request):
#     result = dict()
#     user_id = request.data.get("user_id", 0)
#     user = Users.get_user_by_id(user_id)
#     # print "user_id:"
#     # print user_id
#     if not user:
#         result["success"] = False
#         return Response(result)
#
#     ue = Users.get_user_extension(user.username)
#     win_count = ue.win_count
#     lost_count = ue.lost_count
#
#     # print "devie_token:"
#     # print user.userextension.device_token
#     if ue.device_token:
#         if int(win_count) == 0 and int(lost_count) == 0:
#             pass
#         else:
#             push_unicast(
#                 pushes.YOUMENG_IOS_APP_KEY,
#                 pushes.YOUMENG_IOS_APP_MASTER_SECRET,
#                 ue.device_token,
#                 pushes.WIN_AND_LOST_APP_TICKER,
#                 pushes.WIN_AND_LOST_APP_TITLE,
#                 pushes.WIN_AND_LOST_APP_TEXT % (win_count, lost_count))
#         result["success"] = True
#     else:
#         result["success"] = False
#     return Response(result)


@api_view(http_method_names=["GET"])
def get_all_unread_win_and_lost_by_publish(request):
    user_id = int(request.GET.get("user_id", 0))

    publishes = Publishes.get_shown_publishes(user_id).exclude(win_count=0, lost_count=0)
    result = {}
    data = []
    for p in publishes:
        if not p.user_id == user_id:
            continue
        temp = dict()
        temp["publish_image"] = Publishes.get_publish_images(p)["publish_image_small_url"]
        temp["win_count"] = p.win_count
        temp["lost_count"] = p.lost_count
        temp["publish_id"] = p.id
        data.append(temp)
    result["data"] = data
    return Response(result)


# *********************************友盟推送**********************************************
# *********************************统计**************************************************


@api_view(http_method_names=["GET"])
def get_user_count(request):
    result = dict()
    result["all_user_count"] = User.objects.all().count()
    result["publish_user_count"] = Publishes.get_shown_publishes().values("user_id").distinct().count()
    result["publish_user_PM2_5_count"] = Publishes.get_shown_publishes().exclude(PM2_5=-1).values(
        "user_id").distinct().count()
    return Response(result)


# *********************************统计**************************************************
# *********************************购买链接**************************************************

URL_4_PURCHASE = "http://www.sangebaba.com/"


def get_purchase_url_from_text(file_path="media_root/shop/purchase_url.txt"):
    # if os.path.exists(file_path):
    #     print "file exist"
    #     pass
    # else:
    #     print "file does not exist"
    file = open(file_path, "r")
    for r in file.readlines():
        return r


def change_purchase_url_in_text(content_list=["http://www.sangebaba.com/", ],
                                file_path="media_root/shop/purchase_url.txt"):
    result = {}
    try:
        # print "start in change_purchase_url_in_text"
        file = open(file_path, "w")
        # print "open file in change_purchase_url_in_text"
        if content_list:
            content_list = content_list[:1]
        file.writelines(content_list)
        # print "write to file in change_purchase_url_in_text"
        file.flush()
        # print "execute in change_purchase_url_in_text"
        result["success"] = True
        result["info"] = "OK"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["success"] = False
        result["info"] = "exception:" + ex.message
    return result


@api_view(http_method_names=["GET"])
def get_purchase_url(request):
    result = dict()
    result["url"] = RedEnvelopeConfiguration.get_configuration()["purchase_url"]
    return Response(result)


@api_view(http_method_names=["POST"])
def change_purchase_url(request):
    content_list = list()
    result = {}
    content_list.append(request.data.get("url", ""))
    result = change_purchase_url_in_text(content_list)

    return Response(result)


def change_purchase_url_interface(request):
    # print "I am in"
    return render(request, "purchase/change_purchase_url.html")


# *********************************购买链接**************************************************
# *********************************下载页面**************************************************


def download(request):
    return render(request, "forum/download.html")


# *********************************下载页面**************************************************


# **********************************红包和购买链接设置***************************************************

@authenticated
def configuration(request):
    post = request.data

    if post:
        parameters = dict()
        add_parameters_to_dictionary(post, parameters, "factor")
        add_parameters_to_dictionary(post, parameters, "person_threshold")
        add_parameters_to_dictionary(post, parameters, "device_bonus")
        add_parameters_to_dictionary(post, parameters, "extra_max")
        add_parameters_to_dictionary(post, parameters, "extra_min")
        add_parameters_to_dictionary(post, parameters, "extra_possibility")
        add_parameters_to_dictionary(post, parameters, "extra_threshold")
        add_parameters_to_dictionary(post, parameters, "extra_keep")
        add_parameters_to_dictionary(post, parameters, "rain_max")
        add_parameters_to_dictionary(post, parameters, "rain_min")
        add_parameters_to_dictionary(post, parameters, "rain_possibility")
        add_parameters_to_dictionary(post, parameters, "rain_threshold")
        add_parameters_to_dictionary(post, parameters, "rain_start")
        add_parameters_to_dictionary(post, parameters, "rain_end")
        add_parameters_to_dictionary(post, parameters, "rain_count")
        add_parameters_to_dictionary(post, parameters, "rain_start_date")
        add_parameters_to_dictionary(post, parameters, "rain_end_date")
        add_parameters_to_dictionary(post, parameters, "rain_week")
        add_parameters_to_dictionary(post, parameters, "purchase_url")
        RULES_CONSTANT.modify_configuration(parameters)
    redenvelope = RULES_CONSTANT.get_configuration()

    return render(request, "editor/configuration.html", {"redenvelope": redenvelope})


def add_parameters_to_dictionary(source, parameters, name):
    value = source.get(name, None)
    if value:
        parameters[name] = value


def configuration_action(request):
    post = request.data
    parameters = dict()
    add_parameters_to_dictionary(post, parameters, "factor")
    add_parameters_to_dictionary(post, parameters, "person_threshold")
    add_parameters_to_dictionary(post, parameters, "device_bonus")
    add_parameters_to_dictionary(post, parameters, "extra_max")
    add_parameters_to_dictionary(post, parameters, "extra_min")
    add_parameters_to_dictionary(post, parameters, "extra_possibility")
    add_parameters_to_dictionary(post, parameters, "extra_threshold")
    add_parameters_to_dictionary(post, parameters, "extra_keep")
    add_parameters_to_dictionary(post, parameters, "rain_max")
    add_parameters_to_dictionary(post, parameters, "rain_min")
    add_parameters_to_dictionary(post, parameters, "rain_possibility")
    add_parameters_to_dictionary(post, parameters, "rain_threshold")
    add_parameters_to_dictionary(post, parameters, "rain_start")
    add_parameters_to_dictionary(post, parameters, "rain_end")
    add_parameters_to_dictionary(post, parameters, "rain_count")
    add_parameters_to_dictionary(post, parameters, "rain_start_date")
    add_parameters_to_dictionary(post, parameters, "rain_end_date")
    add_parameters_to_dictionary(post, parameters, "rain_week")
    add_parameters_to_dictionary(post, parameters, "purchase_url")
    RULES_CONSTANT.modify_configuration(parameters)
    return HttpResponseRedirect("/configuration/show")


# **********************************红包和购买链接设置***************************************************


# **********************************用户信息***************************************************

@authenticated
def user_show(request):
    username = request.data.get("username", "")
    user = dict()
    try:
        if username:
            user_object = UserExtension.objects.get(user__username=username)
            user["id"] = user_object.user.id
            user["username"] = username
            user["nickname"] = user_object.nickname

            if user_object.gender == "M":
                user["gender"] = u"男"
            elif user_object.gender == "F":
                user["gender"] = u"女"
            else:
                user["gender"] = u"未知"

            user["city"] = user_object.city

            user_online = Token.objects.filter(user__username=username)
            if user_online.count() > 0:
                user["online"] = u"是"
            else:
                user["online"] = u"否"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    return render(request, "user/index.html", {"user": user, "search_username": username})


@authenticated
def user_show_change(request):
    username = request.data.get("username", "")
    nickname = request.data.get("nickname", "")
    result = list()
    user = dict()
    try:
        if username:
            user_object = UserExtension.objects.get(user__username=username)
            user["id"] = user_object.user.id
            user["username"] = username
            user["nickname"] = user_object.nickname

            if user_object.gender == "M":
                user["gender"] = u"男"
            elif user_object.gender == "F":
                user["gender"] = u"女"
            else:
                user["gender"] = u"未知"

            user["city"] = user_object.city

            user_online = Token.objects.filter(user__username=username)
            if user_online.count() > 0:
                user["online"] = u"是"
            else:
                user["online"] = u"否"
            result.append(user)
        elif nickname:
            uos = UserExtension.objects.filter(nickname=nickname)
            for user_object in uos:
                user = dict()
                user["id"] = user_object.user.id
                user["username"] = user_object.user.username
                user["nickname"] = user_object.nickname

                if user_object.gender == "M":
                    user["gender"] = u"男"
                elif user_object.gender == "F":
                    user["gender"] = u"女"
                else:
                    user["gender"] = u"未知"

                user["city"] = user_object.city

                user_online = Token.objects.filter(user__username=user_object.user.username)
                if user_online.count() > 0:
                    user["online"] = u"是"
                else:
                    user["online"] = u"否"
                result.append(user)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    return render(request, "user/index.html", {
        "result": result,
        "search_username": username,
        "search_nickname": nickname
    })


# **********************************用户信息***************************************************
# **********************************合并地址***************************************************


def update_shop_address(new_address_id, old_address_id):
    shops = Shop.objects.filter(address_id=old_address_id)
    for s in shops:
        s.address_id = new_address_id
        s.save()


def clean_one_address(old_address_id):
    try:
        address = Address.objects.get(id=old_address_id)
        address.audit = 0
        address.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


@authenticated
def addresses(request):
    current_page = request.GET.get("page", 1)
    count_per_page = 10

    row_per_page = 10
    start_index = (int(current_page) - 1) * row_per_page
    end_index = start_index + row_per_page

    post = request.data
    address_objects = Address.objects.all().exclude(audit=0).order_by("id")

    longitude = post.get("longitude", "")
    latitude = post.get("latitude", "")
    detail_address = post.get("detail_address", "")
    address_id = post.get("address_id", "")

    search = post.get("search", None)
    ok = post.get("ok", None)

    if longitude:
        float_start_longitude = float(longitude)
        float_end_longitude = float_start_longitude + 0.01
        address_objects = address_objects.filter(longitude__gte=float_start_longitude,
                                                 longitude__lte=float_end_longitude)
    if latitude:
        float_start_latitude = float(latitude)
        float_end_latitude = float_start_latitude + 0.01
        address_objects = address_objects.filter(latitude__gte=float_start_latitude, latitude__lte=float_end_latitude)
    if detail_address:
        address_objects = address_objects.filter(detail_address__contains=detail_address)

    count_total_page = ceil(address_objects.count() / float(row_per_page))

    if start_index > address_objects.count():
        start_index = 0
        end_index = row_per_page
    address_show = address_objects[start_index:end_index]

    pagination_result = pagination(int(current_page), int(count_per_page), int(count_total_page))

    if ok:
        if address_id:
            for ao in address_objects:
                if int(address_id) == ao.id:
                    continue
                update_shop_address(address_id, ao.id)
                clean_one_address(ao.id)
    else:
        pass

    return render(request, "address/index.html",
                  {
                      "addresses": address_show,
                      "longitude": longitude,
                      "latitude": latitude,
                      "detail_address": detail_address,
                      "address_id": address_id,

                      "previous_page": pagination_result["previous_page_index"],
                      "next_page": pagination_result["next_page_index"],
                      "show_page_indexes": pagination_result["show_page_indexes"],
                      "total_page": int(count_total_page)
                  })


# **********************************合并地址***************************************************
# **********************************小编首页***************************************************

@authenticated
def editor_index(request):
    editor_base_url = files.BASE_URL + "/"
    return render(request, "editor/index.html", {"base_url": editor_base_url})


# **********************************小编首页***************************************************
# **********************************登录***************************************************


def app_login(request):
    next_index = request.GET.get("next", "/support/index")
    username = request.data.get("username", None)
    password = request.data.get("password", None)
    if username and password:
        user = auth.authenticate(username=username, password=password)
        if user and user.is_active:
            auth.login(request, user)
            return HttpResponseRedirect(next_index)
    return render(request, "login/index.html")


def app_logout(request):
    if request.user.is_authenticated():
        auth.logout(request)
    return HttpResponseRedirect("/support/login")


# **********************************登录***************************************************
# **********************************社区版主后台***************************************************


@authenticated
def webmaster_index(request):
    params_logout = request.GET.get("logout", None)
    if params_logout:
        return app_logout(request)

    categories = ForumCategory.objects.all()
    user = request.user
    own_categories = list()
    for c in categories:
        if user in c.owners.all():
            temp = get_forum_category_info(c, user, True)
            own_categories.append(temp)
    if own_categories:
        return render(request, "forum/webmaster_index.html", {"categories": own_categories})
    else:
        return HttpResponse("您不是版主")


def get_shown_post_list(posts, current_page, post_count_per_page=10):
    start_index = (int(current_page) - 1) * int(post_count_per_page)
    end_index = int(current_page) * int(post_count_per_page)
    index = 0
    post_list = list()
    for p in posts:
        if start_index <= index < end_index:
            post_info = get_post_info(p, True)
            post_list.append(post_info)
        index += 1
    return post_list


def is_category_owner(user, category_id):
    result = dict()
    try:
        category = ForumCategory.objects.get(id=category_id)
        if user in category.owners.all():
            result["success"] = True
        else:
            result["success"] = False
            result["info"] = "您不是版主"
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        result["success"] = False
        result["info"] = "查找版块失败"
    return result


@authenticated
def category_owner_post(request):
    status = int(request.GET.get("status", 1))
    page = int(request.GET.get("page", 1))
    post_id = int(request.GET.get("post_id", 0))
    user = request.user
    category_id = int(request.GET.get("category_id", 0))

    if not category_id:
        return HttpResponse("没有版块的ID")
    else:
        pass

    params_is_category_owner = is_category_owner(user, category_id)

    if params_is_category_owner["success"]:
        pass
    else:
        return HttpResponse(params_is_category_owner["info"])

    try:
        p = ForumPost.objects.get(id=post_id, category_id=category_id)
        p.status = not p.status
        p.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    posts = ForumPost.objects.filter(category_id=category_id, status=status)

    current_page = page
    count_per_page = 5
    count_total_page = int(ceil(posts.count() / float(count_per_page)))

    pagination_result = pagination(current_page, count_per_page, count_total_page)

    post_list = get_shown_post_list(posts, current_page)

    return render(request, "forum/category_owner_post_index.html", {
        "posts": post_list,
        "status": status,
        "category_id": category_id,

        "current_page": current_page,
        "previous_page": pagination_result["previous_page_index"],
        "next_page": pagination_result["next_page_index"],
        "show_page_indexes": pagination_result["show_page_indexes"],
        "total_page": int(count_total_page)
    })


def get_post_file(files, name, result):
    data = files.get(name, None)
    if data:
        path = settings.MEDIA_URL_NAME + "/forum/post/" + time.strftime('%Y/%m/%d/')
        the_name = handle_uploaded_file(data, path, True)
        if the_name:
            result[name] = the_name


def get_post_parameters(post, files):
    result = dict()

    title = post.get("title", "")
    result["title"] = title

    content = post.get("content", "")
    result["content"] = content

    if post.get("is_category_rule"):
        result["is_category_rule"] = True
    else:
        result["is_category_rule"] = False
    if post.get("is_digest"):
        result["is_digest"] = True
    else:
        result["is_digest"] = False
    if post.get("is_top"):
        result["is_top"] = True
    else:
        result["is_top"] = False
    # top_weight = post.get("top_weight")
    result["top_weight"] = post.get("top_weight", 0)

    get_post_file(files, "image1", result)
    get_post_file(files, "image2", result)
    get_post_file(files, "image3", result)
    get_post_file(files, "image4", result)
    get_post_file(files, "image5", result)
    get_post_file(files, "image6", result)
    get_post_file(files, "image7", result)
    get_post_file(files, "image8", result)
    get_post_file(files, "image9", result)
    return result


def update_one_forum_post(post_id, params):
    try:
        the_post = ForumPost.objects.get(id=post_id)
        for key, value in params.iteritems():
            if key == "title":
                the_post.title = value
            elif key == "content":
                the_post.content = value
            elif key == "is_category_rule":
                the_post.is_category_rule = value
            elif key == "is_digest":
                the_post.is_digest = value
            elif key == "is_top":
                the_post.is_top = value
            elif key == "top_weight":
                the_post.top_weight = value
            elif key == "image1":
                the_post.post_image_1.name = value
            elif key == "image2":
                the_post.post_image_2.name = value
            elif key == "image3":
                the_post.post_image_3.name = value
            elif key == "image4":
                the_post.post_image_4.name = value
            elif key == "image5":
                the_post.post_image_5.name = value
            elif key == "image6":
                the_post.post_image_6.name = value
            elif key == "image7":
                the_post.post_image_7.name = value
            elif key == "image8":
                the_post.post_image_8.name = value
            elif key == "image9":
                the_post.post_image_9.name = value
            else:
                pass
        the_post.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


@authenticated
def category_owner_post_modify(request):
    post_id = request.GET.get("post_id", 0)
    post = dict()
    category_id = request.GET.get("category_id", 0)
    user = request.user

    params_is_category_owner = is_category_owner(user, category_id)
    if not params_is_category_owner["success"]:
        return HttpResponse(params_is_category_owner["info"])

    try:
        post_instance = ForumPost.objects.get(id=post_id)
        post = get_post_info(post_instance, False)
        post["content"] = "\r\n".join(post["content"])
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    ok = request.data.get("ok", None)
    cancel = request.data.get("cancel", None)
    back = request.data.get("back", None)
    if ok:
        post = request.data
        files = request.FILES
        params = get_post_parameters(post, files)
        update_one_forum_post(post_id, params)
    elif back:
        return HttpResponseRedirect("/support/webmaster/post?category_id=" + str(category_id))
    elif cancel:
        pass

    return render(request, "forum/category_owner_post_modify.html", {"post": post})


def category_owner_reply(request):
    post_id = request.GET.get("post_id", 0)
    reply_status = int(request.GET.get("status", 1))
    if reply_status == 1:
        reply_status_status = True
    else:
        reply_status_status = False
    page = int(request.GET.get("page", 1))
    category_id = 0
    reply_id = int(request.GET.get("reply_id", 0))

    try:
        reply = ForumReply.objects.get(id=reply_id, post_id=post_id)
        reply.status = not reply.status
        reply.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    reply_list = list()
    if post_id:
        try:
            post = ForumPost.objects.get(id=post_id)
            category_id = post.category.id
            reply_list = get_reply_info(post, 0, 0, reply_status_status)
            if reply_list:
                reply_list = reply_list["data"]
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

    current_page = page
    count_per_page = 10
    reply_count_per_page = 5
    count_total_page = int(ceil(len(reply_list) / float(reply_count_per_page)))
    pagination_result = pagination(current_page, count_per_page, count_total_page)

    return render(request, "forum/category_owner_reply_index.html", {
        "reply": reply_list,
        "status": reply_status,
        "post_id": post_id,
        "category_id": category_id,

        "show_page_indexes": pagination_result["show_page_indexes"],
        "current_page": current_page,
        "previous_page": pagination_result["previous_page_index"],
        "next_page": pagination_result["next_page_index"],
        "total_page": count_total_page,
    })


# **********************************社区版主后台***************************************************
# **********************************WEATHER***************************************************

@api_view(http_method_names=["GET"])
def get_weather_info(request):
    result = dict()
    city = request.GET.get("city", None)
    if city:
        city = HomePage.clear_city(city)
    else:
        return result

    # weather_in_db = weather.get_weather_from_db(city)
    weather_in_db = Weathers.get_weather_from_db(city)
    print weather_in_db
    if weather_in_db:
        if Weathers.is_weather_in_db_timeout(weather_in_db):
            result = Weathers.get_weather_from_heweather(city)
            Weathers.update_weather(weather_in_db, result)
        else:
            result["pm2_5"] = weather_in_db.pm2_5
            result["condition"] = weather_in_db.condition
            result["temperature"] = weather_in_db.temperature
            result["code"] = weather_in_db.code
            result["city"] = city
    else:
        result = Weathers.get_weather_from_heweather(city)
        Weathers.create_weather(result)
    result.pop("city")
    return Response(result)


# **********************************WEATHER***************************************************

# **********************************POST PICTURE***************************************************

def get_post_file_path():
    from files import FileOperation
    cwd = FileOperation.getcwd()
    local_path = "forum/article_images/"
    path = cwd + settings.MEDIA_URL + local_path
    http_path = files.BASE_URL_4_IMAGE + local_path
    listdir = FileOperation.listdir(path)
    result = list()
    for f in listdir:
        if FileOperation.isfile(path + f):
            temp = dict()
            temp["url"] = http_path + f
            temp["size"] = FileOperation.getsize(path + f) / 1024
            result.append(temp)
    result = sort_list_with_dict(result, "size")
    return result


def copy(request):
    import pyperclip
    data = request.GET.get("data", "")
    pyperclip.copy(data)


def show_post_picture(request):
    post_path = get_post_file_path()
    return render(request, "forum/picture_show.html", {"path": post_path})


def handle_uploaded_file(f, path=None, relative_name=False):
    file_name = ""
    try:
        if not path:
            path = settings.MEDIA_URL_NAME + "/publish/big/" + time.strftime('%Y/%m/%d/%H/%M/%S/')
        if not os.path.exists(path):
            os.makedirs(path)
        file_name = path + f.name
        while os.path.exists(file_name):
            file_name_parts = file_name.split(".")
            file_name = "_".join(file_name_parts[:-1]) + time.strftime('%Y%m%d%H%M%S') + "." + file_name_parts[-1]
        destination = open(file_name, 'wb+')
        for chunk in f.chunks():
            destination.write(chunk)
        destination.close()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
    if relative_name:
        return file_name[len(settings.MEDIA_URL_NAME + "/"):]
    else:
        return file_name


def add_article_images(request):
    file = request.FILES.get("my_picture", None)
    path = settings.MEDIA_URL_NAME + "/forum/article_images/"
    print path
    if file:
        handle_uploaded_file(file, path, True)
    return HttpResponseRedirect("/show/post/picture")


# **********************************POST PICTURE***************************************************
# **********************************COUPON优惠券***************************************************


def check_verification_code(verification_code, session_key, key):
    return Messages.check_verification_code(verification_code, session_key, key)


def create_coupon(request):
    coupon = dict()
    coupon["sequence"] = Coupons.generate_uuid()

    # user_extension_objects = UserExtension.objects.all()
    # users = [{"id": u.user.id, "nickname": u.nickname} for u in user_extension_objects]

    channels_objects = Channel.objects.all()
    channels = [{"id": c.id, "name": c.name} for c in channels_objects]

    uses_objects = Use.objects.all()
    uses = [{"id": c.id, "name": c.name} for c in uses_objects]

    post = request.data
    if post:
        print "create coupon"
        post_sequence = post.get("sequence", None)
        post_coupon_value = post.get("coupon_value", 0)
        if post_coupon_value:
            post_coupon_value = float(post_coupon_value)
        # post_use_id = int(post.get("use_id", 0))
        post_channel_id = int(post.get("channel_id", 0))
        if post.get("valid_days", 3):
            post_valid_days = int(post.get("valid_days"))
        else:
            post_valid_days = 3
        print post_valid_days
        post_user_id = int(post.get("user_id", 0))
        post_coupon = Coupon(
            sequence=post_sequence,
            coupon_value=post_coupon_value,
            is_valid=True,
            valid_days=post_valid_days)
        # if post_use_id:
        #     post_coupon.use_id = post_use_id
        if post_channel_id:
            post_coupon.channel_id = post_channel_id
        if post_user_id:
            post_coupon.user_id = post_user_id
        post_coupon.save()

    return render(request, "coupon/index.html",
                  {
                      "coupon": coupon,
                      "uses": uses,
                      "channels": channels,
                      # "users": users
                  })


def activate_coupon_action(phone, sequence, verification_code, session_key):
    result = dict()

    # 优惠券是否有效并未被激活
    coupon = Coupons.get_youzan_coupon(sequence)
    if coupon and Coupons.is_coupon_valid(coupon) and not Coupons.is_coupon_activated(coupon):
        # 验证码是否正确
        if not check_verification_code(verification_code, session_key, phone):
            result["success"] = False
            result["type"] = 3
            result["info"] = "the verification is not correct"
            return result

        # 用户是否存在
        username = phone
        user = Users.get_user(username)
        if not user:
            user_dict = dict()
            user_dict["username"] = username
            user_dict["phone"] = phone
            user_dict["password"] = username[-6:]
            user = Users.add(user_dict)
            if not user:
                result["success"] = False
                result["type"] = 2
                result["info"] = "no such user"
                return result
            else:
                data_list = [phone]
                Messages.send_notification(phone, data_list)
        coupon.user_id = user.id
        coupon.activated_at = Datetimes.get_now()
        coupon.save()
        result["success"] = True
        result["type"] = 0
        result["info"] = "the coupon is activated successfully"
    else:
        result["success"] = False
        result["type"] = 1
        result["info"] = "the coupon is not valid"
    print result
    return result


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def activate_coupon(request):
    result = dict()
    phone = request.data.get("phone", None)
    sequence = request.data.get("sequence", None)
    verification_code = request.data.get("verification_code", None)
    session_key = request.data.get("session_key", None)

    if phone and sequence and verification_code and session_key:
        result = activate_coupon_action(phone, sequence, verification_code, session_key)
    else:
        result["info"] = "the parameter phone or sequence does not exist"
        result["success"] = False
    return Response(result)


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def activate_coupon_again(request):
    result = dict()
    phone = request.data.get("phone", None)
    verification_code = request.data.get("verification_code", None)
    # Logs.print_log("verification_code", verification_code)
    session_key = request.data.get("session_key", None)
    # Logs.print_log("session_key", session_key)

    coupon_dict = dict()
    coupon_dict["descriptions"] = Coupons.PRODUCT[2]["descriptions"]
    coupon_dict["product"] = Coupons.PRODUCT[2]["key"]
    coupon_dict["channel_id"] = 1
    the_coupon = Coupons.get_one_coupon(coupon_dict)
    sequence = None
    if the_coupon:
        sequence = the_coupon.youzan_sequence

    if phone and sequence and verification_code and session_key:
        result = activate_coupon_action(phone, sequence, verification_code, session_key)
    else:
        result["info"] = "the parameter phone does not exist or there is no coupon"
        result["success"] = False
    return Response(result)


def is_coupon_out_of_date(coupon):
    now = Datetimes.get_now()
    utc_now = Datetimes.transfer_datetime(now, is_utc=False)
    the_days = Datetimes.get_delta_time(coupon.activated_at, utc_now).days
    # 是否过期
    if the_days < coupon.valid_days:
        return False
    else:
        return True


@api_view(http_method_names=["POST"])
def waste_coupon(request):
    result = dict()
    phone = request.data.get("phone", None)
    sequence = request.data.get("sequence", None)

    if phone and sequence:
        the_coupon = Coupons.get_coupon(sequence)
        if the_coupon:
            # 是否被激活
            if the_coupon.user_id:
                # 是否过期
                if not is_coupon_out_of_date(the_coupon):
                    ue = user_exist_dict(phone)
                    # 激活的用户是否存在
                    if ue["success"]:
                        # 是否本用户激活的
                        user = Users.get_user(username=phone)
                        if the_coupon.user_id == user.id:
                            the_coupon.is_valid = False
                            the_coupon.save()
                            result["success"] = True
                        else:
                            result["success"] = False
                            result["info"] = "the coupon is not yours"
                    else:
                        result["success"] = False
                        result["info"] = "the user does not exist"
                else:
                    result["success"] = False
                    result["info"] = "the coupon is out of date"
            else:
                result["success"] = False
                result["info"] = "the coupon is not activated"
        else:
            result["success"] = False
            result["info"] = "the coupon does not exist"
    else:
        result["success"] = False
        result["info"] = "phone or sequence does not exist"

    return Response(result)


@api_view(http_method_names=["GET"])
def get_user_coupons(request):
    result = list()
    user_id = request.GET.get("user_id", 0)
    shown_days = request.GET.get("shown_days", 30)
    if user_id:
        result = Coupons.get_user_coupons(user_id, shown_days)
    return Response(result)


# 我的优惠券
def my_coupon(request):
    return render(request, "coupon/mycoupon.html")


@api_view(http_method_names=["GET"])
def get_my_coupon_url(request):
    result = dict()
    result["my_coupon_url"] = files.BASE_URL + "/rest/api/coupon/my"
    return Response(result)


def coupon_channel(request):
    return render(request, "coupon/channel.html")


def coupon(request):
    return render(request, "coupon/coupon.html")


def coupon_success(request):
    return render(request, "coupon/success.html")


# **********************************COUPON优惠券***************************************************
# **********************************ACTIVITY***************************************************

def enroll(phone, nickname, company_id):
    result = dict()
    # 用户是否存在
    username = phone
    user = Users.get_user(username)
    if not user:
        user_dict = dict()
        user_dict["username"] = username
        user_dict["phone"] = phone
        user_dict["password"] = username[-6:]
        user_dict["nickname"] = nickname
        if int(company_id) > 0:
            user_dict["company_id"] = company_id
        user = Users.add(user_dict)
        if not user:
            result["success"] = False
            result["info"] = "no such user"
        else:
            data_list = [phone]
            Messages.send_notification(phone, data_list)
            result["success"] = True
            result["info"] = "add successfully!"
    else:
        ue = Users.get_user_extension(user.username)
        ue.nickname = nickname
        if int(company_id) > 0:
            ue.company_id = company_id
        ue.save()
        result["success"] = True
        result["info"] = "enroll successfully!"
    return result


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def auto_register_interface(request):
    result = dict()
    phone = request.data.get("phone", None)
    nickname = request.data.get("nickname", None)
    verification_code = request.data.get("verification_code", None)
    session_key = request.data.get("session_key", None)
    company_name = request.data.get("company_name", None)
    company_name = Characters.unicode_to_concrete(company_name)

    if phone and nickname and verification_code and session_key and company_name:
        if not check_verification_code(verification_code, session_key, phone):
            result["success"] = False
            result["info"] = "the verification code is incorrect"
            return Response(result)
        company = Companies.get(company_name)
        if company:
            company_id = company.id
        else:
            company_id = 0
        result = enroll(phone, nickname, company_id)
    else:
        result["success"] = False
        result["info"] = "the parameters are incorrect"
    return Response(result)


def get_somebody_max_pm2_5(user_id, start, end):
    result = dict()
    max_pm2_5 = -1
    created_at = Datetimes.get_now()
    publishes = Publishes.get_publishes_by_user_sometime(user_id, start, end)
    if not publishes:
        result["pm2_5"] = max_pm2_5
        result["created_at"] = created_at

    for p in publishes:
        if p.PM2_5 > max_pm2_5:
            max_pm2_5 = p.PM2_5
            created_at = p.created_at
    result["pm2_5"] = max_pm2_5
    result["created_at"] = created_at
    return result


def get_activity_ranking(company_name):
    result = dict()
    if company_name:
        company = Companies.get(company_name)
        if not company:
            result["success"] = False
            result["info"] = 'the company does not exist'
            return result
        start = company.start_datetime
        if not start:
            result["success"] = False
            result["info"] = 'the company start_datetime is not configured'
            return result
        end = company.end_datetime
        if not end:
            result["success"] = False
            result["info"] = 'the company end_datetime is not configured'
            return result
        result["start"] = Datetimes.clock_to_string(Datetimes.transfer_datetime(start))
        result["end"] = Datetimes.clock_to_string(Datetimes.transfer_datetime(end))

        user_extensions_in_company = Users.get_user_extensions_by_company(company.id)
        if not user_extensions_in_company:
            result["success"] = False
            result["info"] = 'the company does not have users'
            return result
    else:
        start = Datetimes.get_2010()
        end = Datetimes.get_now()
        user_extensions_in_company = Users.get_user_extensions_with_company()

    data = list()
    if user_extensions_in_company:
        for u in user_extensions_in_company:
            pm2_5s = get_somebody_max_pm2_5(u.user_id, start, end)
            if pm2_5s["pm2_5"] < 0:
                continue
            temp = dict()
            temp["nickname"] = u.nickname
            temp["pm2_5"] = pm2_5s["pm2_5"]
            temp["created_at"] = Datetimes.clock_to_string(pm2_5s["created_at"])
            data.append(temp)

    if not data:
        result["success"] = False
        result["info"] = 'no company user has got pm2.5'
        return result

    data = sorted(data, key=operator.itemgetter("pm2_5", "created_at"), reverse=True)
    index = 1
    for d in data:
        d["ranking"] = index
        index += 1

    result["success"] = True
    result["info"] = 'you have got what you want'
    result["data"] = data
    return result


@api_view(http_method_names=["GET"])
def get_activity_ranking_interface(request):
    result = dict()
    company_name = request.GET.get("company_name", None)

    if company_name:
        company_name = Characters.unicode_to_concrete(company_name)
        result = get_activity_ranking(company_name)

    return Response(result)


@api_view(http_method_names=["GET"])
def get_activity_carousel_data(request):
    count = int(request.GET.get("count", 20))
    result = get_activity_ranking(None)
    data_count = len(result["data"])
    if count > data_count:
        count = data_count

    the_result = ""
    for i in xrange(count):
        the_result += result["data"][i]["nickname"] + u": PM2.5值为" \
                      + str(result["data"][i]["pm2_5"]) + u"μg/m³；"
    if the_result:
        return Response(the_result[:-1])
    else:
        return Response(the_result)


def get_coupon_package():
    result = list()
    coupon_dict = dict()
    product_list = [
        Coupons.PRODUCT[2]["key"],
        Coupons.PRODUCT[3]["key"],
        Coupons.PRODUCT[4]["key"],
        Coupons.PRODUCT[5]["key"],
    ]
    descriptions_list = [
        Coupons.PRODUCT[2]["descriptions"],
        Coupons.PRODUCT[3]["descriptions"],
        Coupons.PRODUCT[4]["descriptions"],
        Coupons.PRODUCT[5]["descriptions"],
    ]
    # product_name = [
    #     Coupons.PRODUCT[2]["name"],
    #     Coupons.PRODUCT[3]["name"],
    #     Coupons.PRODUCT[4]["name"],
    #     Coupons.PRODUCT[5]["name"],
    # ]
    for i in xrange(len(product_list)):
        coupon_dict["descriptions"] = descriptions_list[i]
        coupon_dict["product"] = product_list[i]
        coupon_dict["channel_id"] = 1
        coupon_object = Coupons.get_one_coupon(coupon_dict)
        if coupon_object:
            result.append(coupon_object.youzan_sequence)

    return result


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def send_coupons_from_activity(request):
    result = dict()
    phone = request.data.get("phone", None)
    if not phone:
        result["success"] = False
        result["info"] = "the phone is needed!"
        return Response(result)

    data_list = list()
    data_list.append(phone)
    data_list_coupons = Coupons.get_coupon_package()
    data_list += data_list_coupons

    if data_list:
        Messages.send_notification(phone, data_list, messages.SMS_TEMPLATE_ACTIVITY_NOTIFICATION_ID)
        result["success"] = True
        result["info"] = "OK"
        result["data"] = data_list
    else:
        result["success"] = False
        result["info"] = "there is no coupon!"
    return Response(result)


@api_view(http_method_names=["GET"])
def get_activity_bad_air(request):
    return render(request, "activity/badair.html")


@api_view(http_method_names=["GET"])
def get_activity_bad_air_rank(request):
    return render(request, "activity/badairrank.html")


@api_view(http_method_names=["GET"])
def get_activity_company_sign(request):
    return render(request, "activity/companysign.html")


@api_view(http_method_names=["GET"])
def get_activity_coupon(request):
    return render(request, "activity/noequip-coupon.html")


@api_view(http_method_names=["GET"])
def get_activity_prize_of_sign(request):
    return render(request, "activity/prizeofsign.html")


@api_view(http_method_names=["GET"])
def get_activity_coupon_600(request):
    return render(request, "activity/coupon600.html")


# **********************************ACTIVITY***************************************************
# **********************************后台统计***************************************************


def get_statistics(request):
    result_list = list()
    result = dict()

    shops = Shop.objects.filter(Q(formaldehyde__gte=0) |
                                Q(publish__PM2_5__gte=0)) \
        .values("dianping_city") \
        .annotate(count=Count("dianping_city"))

    publishes = Publish.objects.filter(Q(shop__formaldehyde__gte=0) |
                                       Q(PM2_5__gte=0)) \
        .values("shop__dianping_city") \
        .annotate(count=Count("shop__dianping_city"))

    for s in shops:
        for p in publishes:
            if s.dianping_city == p.shop__dianping_city:
                result["city"] = s.dianping_city
                result["shop_count"] = s.count
                result["count"] = p.count
                result_list.append(result)
                break

    return render(request, "editor/statistics.html", {"result": result_list})


# **********************************后台统计***************************************************

def get_post_file_path():
    from files import FileOperation
    cwd = FileOperation.getcwd()
    local_path = "forum/article_images/"
    path = cwd + settings.MEDIA_URL + local_path
    http_path = files.BASE_URL_4_IMAGE + local_path
    listdir = FileOperation.listdir(path)
    result = list()
    for f in listdir:
        if FileOperation.isfile(path + f):
            temp = dict()
            temp["url"] = http_path + f
            temp["size"] = FileOperation.getsize(path + f) / 1024
            result.append(temp)
    result = sort_list_with_dict(result, "size")
    return result


def copy(request):
    import pyperclip
    data = request.GET.get("data", "")
    pyperclip.copy(data)


def show_post_picture(request):
    post_path = get_post_file_path()
    return render(request, "forum/picture_show.html", {"path": post_path})


def add_article_images(request):
    file = request.FILES.get("my_picture", None)
    path = settings.MEDIA_URL_NAME + "/forum/article_images/"
    print path
    if file:
        handle_uploaded_file(file, path, True)
    return HttpResponseRedirect("/show/post/picture")


# **********************************POST PICTURE***************************************************
# # **********************************COUPON优惠券***************************************************
#
#
# def check_verification_code(verification_code, session_key, key):
#     return Messages.check_verification_code(verification_code, session_key, key)
#
#
# def create_coupon(request):
#     coupon = dict()
#     coupon["sequence"] = Coupons.generate_uuid()
#
#     # user_extension_objects = UserExtension.objects.all()
#     # users = [{"id": u.user.id, "nickname": u.nickname} for u in user_extension_objects]
#
#     channels_objects = Channel.objects.all()
#     channels = [{"id": c.id, "name": c.name} for c in channels_objects]
#
#     uses_objects = Use.objects.all()
#     uses = [{"id": c.id, "name": c.name} for c in uses_objects]
#
#     post = request.data
#     if post:
#         print "create coupon"
#         post_sequence = post.get("sequence", None)
#         post_coupon_value = post.get("coupon_value", 0)
#         if post_coupon_value:
#             post_coupon_value = float(post_coupon_value)
#         # post_use_id = int(post.get("use_id", 0))
#         post_channel_id = int(post.get("channel_id", 0))
#         if post.get("valid_days", 3):
#             post_valid_days = int(post.get("valid_days"))
#         else:
#             post_valid_days = 3
#         print post_valid_days
#         post_user_id = int(post.get("user_id", 0))
#         post_coupon = Coupon(
#             sequence=post_sequence,
#             coupon_value=post_coupon_value,
#             is_valid=True,
#             valid_days=post_valid_days)
#         # if post_use_id:
#         #     post_coupon.use_id = post_use_id
#         if post_channel_id:
#             post_coupon.channel_id = post_channel_id
#         if post_user_id:
#             post_coupon.user_id = post_user_id
#         post_coupon.save()
#
#     return render(request, "coupon/index.html",
#             {
#                 "coupon": coupon,
#                 "uses": uses,
#                 "channels": channels,
#                 # "users": users
#             })
#
#
# def activate_coupon_action(phone, sequence, verification_code, session_key):
#     result = dict()
#     # 验证码是否正确
#     if not check_verification_code(verification_code, session_key, phone):
#         result["success"] = False
#         result["type"] = 3
#         result["info"] = "the verification is not correct"
#         return result
#
#     # 用户是否存在
#     username = phone
#     user = Users.get_user(username)
#     if not user:
#         user_dict = dict()
#         user_dict["username"] = username
#         user_dict["phone"] = phone
#         user_dict["password"] = username[-6:]
#         user = Users.add(user_dict)
#         if not user:
#             result["success"] = False
#             result["type"] = 2
#             result["info"] = "no such user"
#             return result
#         else:
#             data_list = [phone]
#             Messages.send_notification(phone, phone)
#
#     # 优惠券是否有效并未被激活
#     coupon = Coupons.get_coupon(sequence)
#     if coupon and Coupons.is_coupon_valid(coupon) and not Coupons.is_coupon_activated(coupon):
#         coupon.user_id = user.id
#         coupon.activated_at = Datetimes.get_now()
#         coupon.save()
#         result["success"] = True
#         result["type"] = 0
#         result["info"] = "the coupon is activated successfully"
#     else:
#         result["success"] = False
#         result["type"] = 1
#         result["info"] = "the coupon is not valid"
#
#     return result
#
#
# @api_view(http_method_names=["POST"])
# @permission_classes((AllowAny,))
# def activate_coupon(request):
#     result = dict()
#     phone = request.data.get("phone", None)
#     sequence = request.data.get("sequence", None)
#     verification_code = request.data.get("verification_code", None)
#     session_key = request.data.get("session_key", None)
#
#     if phone and sequence and verification_code and session_key:
#         print "start"
#         result = activate_coupon_action(phone, sequence, verification_code, session_key)
#     else:
#         result["info"] = "the parameter phone or sequence does not exist"
#         result["success"] = False
#     print result
#     return Response(result)
#
#
# def is_coupon_out_of_date(coupon):
#     now = datetime.datetime.now()
#     utc_now = Datetimes.transfer_datetime(now, is_utc=False)
#     the_days = Datetimes.get_delta_time(coupon.activated_at, utc_now).days
#     # 是否过期
#     if the_days < coupon.valid_days:
#         return False
#     else:
#         return True
#
#
# @api_view(http_method_names=["POST"])
# def waste_coupon(request):
#     result = dict()
#     phone = request.data.get("phone", None)
#     sequence = request.data.get("sequence", None)
#
#     if phone and sequence:
#         the_coupon = Coupons.get_coupon(sequence)
#         if the_coupon:
#             # 是否被激活
#             if the_coupon.user_id:
#                 # 是否过期
#                 if not is_coupon_out_of_date(the_coupon):
#                     ue = user_exist_dict(phone)
#                     # 激活的用户是否存在
#                     if ue["success"]:
#                         # 是否本用户激活的
#                         if the_coupon.user_id == User.obejcts.get(username=phone).id:
#                             the_coupon.is_valid = False
#                             the_coupon.save()
#                             result["success"] = True
#                         else:
#                             result["success"] = False
#                             result["info"] = "the coupon is not yours"
#                     else:
#                         result["success"] = False
#                         result["info"] = "the user does not exist"
#                 else:
#                     result["success"] = False
#                     result["info"] = "the coupon is out of date"
#             else:
#                 result["success"] = False
#                 result["info"] = "the coupon is not activated"
#         else:
#             result["success"] = False
#             result["info"] = "the coupon does not exist"
#     else:
#         result["success"] = False
#         result["info"] = "phone or sequence does not exist"
#
#     return Response(result)
#
# @api_view(http_method_names=["GET"])
# def get_valid_coupon(request):
#     return Response(Coupons.get_valid_coupon())
#
# @api_view(http_method_names=["GET"])
# def get_user_coupons(request):
#     result = list()
#     user_id = request.GET.get("user_id", 0)
#     shown_days = request.GET.get("shown_days", 30)
#     if user_id:
#         result = Coupons.get_user_coupons(user_id, shown_days)
#     return Response(result)
#
#
# # 我的优惠券
# def my_coupon(request):
#     return render(request, "coupon/mycoupon.html")
# @api_view(http_method_names=["GET"])
# def get_my_coupon_url(request):
#     result = dict()
#     result["my_coupon_url"] = BASE_URL + "/rest/api/coupon/my"
#     return Response(result)
# def coupon_channel(request):
#     return render(request, "coupon/channel.html")
# def coupon(request):
#     return render(request, "coupon/coupon.html")
# def coupon_success(request):
#     return render(request, "coupon/success.html")
#
# # **********************************COUPON优惠券***************************************************
# **********************************后台统计***************************************************


def get_statistics(request):
    result_list = list()
    result = dict()
    values = Shop.objects.filter(Q(formaldehyde__gte=0) |
                                 Q(publish__PM2_5__gte=0)) \
        .values("dianping_city", "id") \
        .annotate(count=Count("id"))
    for v in values:
        city = v.get("dianping_city")
        city = HomePage.clear_city(city)
        if city in result.keys():
            result[city]["count"] += int(v.get("count"))
            result[city]["shop_count"] += 1
        else:
            result[city] = dict()
            result[city]["count"] = int(v.get("count"))
            result[city]["shop_count"] = 1
    for key, value in result.iteritems():
        temp = dict()
        temp["city"] = key
        temp["shop_count"] = value["shop_count"]
        temp["count"] = value["count"]
        result_list.append(temp)

    return render(request, "editor/statistics.html", {"result": result_list})


# **********************************后台统计***************************************************

# **********************************渠道下载***************************************************


def channel_app(request):
    return render(request, "forum/channelapp.html")


# **********************************渠道下载***************************************************

# *******************************GAMES************************************************


@api_view(http_method_names=["GET"])
def get_graph_color_data(request):
    sets = GraphColor.get_sets()
    result = dict()
    data = list()
    for i in xrange(300):
        temp = GraphColor.get_one(sets["correct"], sets["incorrect"])
        data.append(temp)
    result["count"] = len(data)
    result["data"] = data
    return Response(result)


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def set_score(request):
    result = dict()
    result["ranking"] = 0
    result["success"] = False
    phone = request.data.get("phone", 0)
    score = request.data.get("score", 0)
    if phone and score:
        game_dict = dict()
        game_dict["username"] = phone
        game_dict["score"] = score
        game = Games.set(game_dict)
        if game:
            now = Datetimes.get_now()
            start_datetime = Datetimes.get_day_start(now)
            end_datetime = Datetimes.get_day_end(now)
            result["ranking"] = Games.get_ranking(score, start_datetime, end_datetime)
            result["success"] = True
        else:
            result["info"] = "the score does not saved successfully!"
    else:
        result["info"] = "the phone or score is invalid!"
    return Response(result)


def get_sorted_data(days, count):
    the_day = Datetimes.get_some_day(days)
    start_datetime = Datetimes.get_day_start(the_day)
    yesterday = Datetimes.get_some_day(1)

    today = Datetimes.get_now()
    end_datetime = Datetimes.get_day_end(today)

    return Games.get_all_username_and_score(start_datetime, end_datetime, count)


@api_view(http_method_names=["GET"])
def get_sorted_ranking(request):
    days = request.GET.get("days", 1)
    count = request.GET.get("count", 20)
    return Response(get_sorted_data(days, count))


def has_got_red_envelope(user_id, days):
    the_day = Datetimes.get_some_day(days)
    start_datetime = Datetimes.get_day_start(the_day)
    yesterday = Datetimes.get_some_day(1)

    today = Datetimes.get_now()
    end_datetime = Datetimes.get_day_end(today)
    if RedEnvelopes.has_game_red_envelope(user_id, start_datetime, end_datetime):
        return True
    else:
        return False


def send_red_envelope(bonus, user_id):
    # rd_type: 3 代表游戏红包
    RedEnvelopes.generate(bonus, user_id, 1, Games.RED_ENVELOPE_TYPE, RedEnvelopes.RED_ENVELOPE_STATE_VALID)


@api_view(http_method_names=["POST"])
@permission_classes((AllowAny,))
def send_reward(request):
    result = dict()
    user_id = request.data.get("user_id", 0)
    days = request.data.get("days", 1)
    count = request.data.get("count", 20)

    if user_id == 0:
        result["success"] = False
        result["type"] = 1
        result["info"] = "the user id is not valid -- 0"
        return Response(result)

    data = get_sorted_data(days, count)
    for datum in data:
        if int(user_id) == int(datum.get("user_id")):
            if not RedEnvelopes.can_get_red_envelope(user_id):
                result["success"] = False
                result["type"] = 2
                result["info"] = "you have too mucah red envelope!"
                return Response(result)
            if has_got_red_envelope(user_id, days):
                result["success"] = False
                result["type"] = 3
                result["info"] = "you have got the red envelope today!"
                return Response(result)
            send_red_envelope(datum.get("reward"), user_id)
            result["success"] = True
            result["info"] = "send red envelope successfully!"
            return Response(result)
    else:
        result["success"] = False
        result["type"] = 4
        result["info"] = "you have no red envelope"
        return Response(result)


def games_index(request):
    return render(request, "games/index.html")


def ranking_index(request):
    return render(request, "games/rank.html")


# *******************************GAMES*************************************************

# ****************************test parts*************************************************


def test_login(request):
    return render(request, "login/index.html")


@api_view(http_method_names=["GET"])
def test_categories(request):
    file_path = "eye/configurations/categories.xml"
    from common.publish_categories import PublishCategories
    return Response(PublishCategories.get_all(file_path))


def test_index(request):
    from files import FileOperation
    cwd = FileOperation.getcwd()
    path = cwd + settings.MEDIA_URL + "forum/post"
    http_path = files.BASE_URL_4_IMAGE + "forum/post/"
    listdir = FileOperation.listdir(path)
    result = list()
    for f in listdir:
        if FileOperation.isfile(path + "/" + f):
            result.append(http_path + f)
    return HttpResponse(result)
    return render(request, "forum/index.html")


def test_version(request):
    version = request.version
    return HttpResponse(version)
    return render(request, "forum/forum_index.html")


def test_video(request):
    return render(request, "forum/test_video.html")


def get_request_user(request):
    data = {}
    key = request.GET.get("key", None)
    if not key:
        data["success"] = False
        data["info"] = "parameter key is None"
    else:
        user_token = Token.objects.filter(key=key)
        if user_token.count() == 1:
            data["success"] = True
            data["info"] = "the user login and the user is " + user_token[0].user.username
        else:
            data["success"] = False
            data["info"] = "the user does not login or the key is error"
    return HttpResponse(data["info"])


@api_view(http_method_names=["GET"])
@permission_classes((AllowAny,))
def get_shop_image_test(request):
    shop_id = request.GET.get("shop_id", 0)
    shop = Shop.objects.get(id=shop_id)
    the_str = get_shop_image(shop)
    return Response(the_str)


@permission_classes((AllowAny,))
def add_formaldehyde(request):
    dictionary = dict()
    dictionary["shop_id"] = request.data.get("shop_id", 0)
    dictionary["formaldehyde"] = request.data.get("formaldehyde", -1)
    dictionary["formaldehyde_image"] = request.FILES.get("formaldehyde_image", None)
    dictionary["content"] = request.data.get("content", None)
    if request.data.get("checked_at", None):
        dictionary["checked_at"] = Datetimes.string_to_clock(request.data.get("checked_at"))
    else:
        dictionary["checked_at"] = datetime.datetime.now()
    dictionary["user_id"] = request.data.get("user_id", 0)
    # print "here"
    return HttpResponse(add_one_formaldehyde(dictionary))


def add_one_formaldehyde(dictionary):
    if dictionary["shop_id"]:
        shop_id = int(dictionary["shop_id"])
    else:
        shop_id = -1
    try:
        shop = Shop.objects.get(id=shop_id)
    except:
        shop = None
    # print shop
    if not shop:
        return False

    if dictionary["formaldehyde"]:
        formaldehyde = float(dictionary["formaldehyde"])
    else:
        formaldehyde = -1
    formaldehyde_image = dictionary["formaldehyde_image"]
    shop.formaldehyde = formaldehyde
    shop.formaldehyde_image = formaldehyde_image
    shop.save()

    content = dictionary["content"]
    checked_at = dictionary["checked_at"]
    user_id = dictionary["user_id"]
    p = Publish(
        user_id=user_id,
        shop_id=shop_id,
        formaldehyde=formaldehyde,
        big_image=formaldehyde_image,
        checked_at=checked_at,
        content=content
    )
    p.save()

    return True


@api_view(http_method_names=["GET"])
def get_single_business_from_dianping_interface(request):
    business_id = request.GET.get("business_id")
    content = get_single_business_from_dianping(business_id)
    if content:
        result = add_one_shop(content)
        result["business_id"] = business_id
        result["shop_id"] = get_shop_id_by_business_id(business_id)
        return Response(result)
    else:
        result = dict()
        result["info"] = "there is some exception"
        result["business_id"] = business_id
        result["shop_id"] = 0
        return Response(result)


def get_single_business_from_dianping(business_id):
    parameter_set = list()
    parameter_set.append(("business_id", business_id))

    sign = get_sign(get_codec(parameter_set))
    url = "http://api.dianping.com/v1/business/get_single_business"
    url += "?business_id=" + str(business_id)
    url += "&appkey=" + str(dianpings.DIANPING_APP_KEY)
    url += "&sign=" + sign
    try:
        content = json.loads(http_get_action(url)).get("businesses")
        # print content
        content = content[0]
        content["business_id"] = business_id
        return content
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return None


def get_shop_id_by_business_id(business_id):
    try:
        shop = Shop.objects.get(dianping_business_id=business_id)
        # return shop.id
    except Exception as ex:
        print ex.message
        return 0


def get_shop_name_4_test(shop_id):
    try:
        return Shop.objects.get(id=shop_id).name
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return None


def get_shop_nearest_pm2_5(obj):
    sorted_obj = obj.order_by("-created_at")
    result = []
    shop_ids = []
    for o in sorted_obj:
        if o.shop_id in shop_ids:
            continue
        shop_ids.append(o.shop_id)
        d = dict()
        d["shop_id"] = o.shop_id
        d["shop_name"] = get_shop_name_4_test(o.shop_id)
        d["PM2_5"] = o.PM2_5
        d["publish_id"] = o.id
        result.append(d)

    return result


def get_shop_today_nearest_pm2_5(obj):
    start = Datetimes.get_day_start(datetime.datetime.now())
    end = Datetimes.get_day_end(datetime.datetime.now())

    sorted_obj = obj.filter(created_at__gte=start, created_at__lte=end).order_by("-created_at")
    result = []
    shop_ids = []
    for o in sorted_obj:
        if o.shop_id in shop_ids:
            continue
        shop_ids.append(o.shop_id)
        d = dict()
        d["shop_id"] = o.shop_id
        d["shop_name"] = get_shop_name_4_test(o.shop_id)
        d["PM2_5"] = o.PM2_5
        d["publish_id"] = o.id
        result.append(d)

    return result


def get_shop_average_pm2_5(obj):
    result = []
    shop_ids = obj.values("shop_id").distinct()
    for s_id in shop_ids:
        sub_obj = obj.filter(shop_id=s_id["shop_id"])
        sum_pm2_5 = sum([s.PM2_5 for s in sub_obj])
        temp = dict()
        temp["PM2_5"] = sum_pm2_5 / len(sub_obj)
        temp["shop_id"] = s_id["shop_id"]
        temp["shop_name"] = get_shop_name_4_test(s_id["shop_id"])
        result.append(temp)

    return result


def sort_list_with_dict(the_list, dict_item, reverse=False):
    return sorted(the_list, key=operator.itemgetter(dict_item), reverse=reverse)


# @api_view(http_method_names=["GET"])
def get_shops_by_publish_pm_2_5(request):
    """
    在一段时间内，按场所最新一条发布的PM2.5值对场所进行排序
    :param request:
    :return:
    """
    try:
        average = int(request.GET.get("average", 0))
    except Exception as ex:
        average = 0
    city = request.GET.get("city", u"北京")
    count = int(request.GET.get("count", 20))
    page = int(request.GET.get("page", 1))

    if average == 0 or average == 2:
        start_time = None
        end_time = Datetimes.get_now()
    else:
        today_weekday = Datetimes.get_weekday(Datetimes.get_now())
        end_date = Datetimes.get_some_day((int(today_weekday) + 1))
        start_date = Datetimes.get_some_day((int(today_weekday) + 1 + 7))
        start_time = Datetimes.get_day_start(start_date)
        end_time = Datetimes.get_day_end(end_date)

    publishes_by_time = Publishes.get_publishes_by_datetime(start_time, end_time)
    publishes_by_time_pm2_5 = publishes_by_time.exclude(
        PM2_5=-1).filter(
        Q(shop__dianping_city__contains=city) | Q(shop__address__detail_address__contains=city))
    if average == 1:
        the_list = get_shop_average_pm2_5(publishes_by_time_pm2_5)
    elif average == 2:
        the_list = get_shop_today_nearest_pm2_5(publishes_by_time_pm2_5)
    else:
        the_list = get_shop_nearest_pm2_5(publishes_by_time_pm2_5)
    shop_publish_pm2_5 = sort_list_with_dict(the_list, dict_item="PM2_5")

    start_index = count * (page - 1)
    end_index = count * page
    shop_list_length = len(shop_publish_pm2_5)
    shops_count = range(int(ceil(shop_list_length / float(count))) + 1)
    shops_count.remove(0)
    if end_index > shop_list_length:
        this_list = shop_publish_pm2_5[start_index:]
    else:
        this_list = shop_publish_pm2_5[start_index:end_index]
    return render(
        request,
        "test/show_current_pm2_5.html",
        {"shops_info": this_list, "shops_count": shops_count, "shops_page": page, "shops_city": city,
         "average": average},
    )


@api_view(http_method_names=["GET"])
def delete_publishes_by_user_id(request):
    telephone = request.GET.get("telephone", 0)
    result = dict()
    if telephone:
        user_id = User.objects.get(username=telephone).id
        publishes = Publishes.get_shown_publishes(user_id).filter(user_id=user_id)
        for p in publishes:
            p.audit = 0
            p.save()
        result["success"] = True
        result["info"] = "OK"
    else:
        result["success"] = False
        result["info"] = "telephone is " + str(telephone) + ", but it is not registered"
    return Response(result)


# 微信接口
from django.views.generic.base import View
import hashlib
from django.http import HttpResponse
import time
from django.views.decorators.csrf import csrf_exempt

try:
    import xml.etree.cElementTree as ET
except ImportError:
    import xml.etree.ElementTree as ET


class WeixinInterface(View):
    def __init__(self):
        self.TOKEN = "14adPyKz0SHbiq0N"
        self.APP_ID = "wx3affc014033ad098"
        self.APP_SECRET = "26d150d01991b75a64e5907a26b4ecb9"

    @staticmethod
    def http_get(url):
        ret = urllib2.urlopen(url)
        content = ret.read()
        return content

    @staticmethod
    def http_post(url, data, headers=None):
        post_data = urllib.urlencode(data)

        # 提交，发送数据
        if headers:
            req = urllib2.Request(url, post_data, headers)
        else:
            req = urllib2.Request(url, post_data)
        ret = urllib2.urlopen(req)

        # 获取提交后返回的信息
        content = ret.read()
        return content

    def get_access_token(self):
        url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + \
              self.APP_ID + "&secret=" + self.APP_SECRET
        return json.loads(WeixinInterface.http_get(url))["access_token"]

    def create_menu(self):
        print "开始自定义菜单"
        url = "https://api.weixin.qq.com/cgi-bin/menu/create?access_token=" + self.get_access_token()
        print url
        data = {
            "button": [
                {
                    "name": "有爱会员",
                    "sub_button": [
                        {
                            "type": "click",
                            "name": "会员活动",
                            "key": "VK001"
                        },
                        {
                            "type": "click",
                            "name": "有爱下午茶",
                            "key": "VK002"
                        },
                        {
                            "type": "click",
                            "name": "有爱一家亲",
                            "key": "VK003"
                        },
                        {
                            "type": "click",
                            "name": "会员积分查询",
                            "key": "VK004"
                        },
                        {
                            "type": "click",
                            "name": "积分政策",
                            "key": "VK005"
                        },
                    ]
                },
                {
                    "name": "肠道健康",
                    "sub_button": [
                        {
                            "type": "click",
                            "name": "你不知道的爱益维",
                            "key": "VK006"
                        },
                        {
                            "type": "click",
                            "name": "肠道健康咨询",
                            "key": "VK007"
                        },
                        {
                            "type": "click",
                            "name": "菌种知识",
                            "key": "VK008"
                        },
                    ]

                },
                {
                    "name": "立即享用",
                    "sub_button": [
                        {
                            "type": "click",
                            "name": "优惠活动",
                            "key": "VK009"
                        },
                        {
                            "type": "click",
                            "name": "爱益维微店",
                            "key": "VK010"
                        },
                        {
                            "type": "click",
                            "name": "1号店寻我",
                            "key": "VK011"
                        },
                        {
                            "type": "click",
                            "name": "淘宝捞我",
                            "key": "VK012"
                        },
                    ]
                },
            ]
        }
        WeixinInterface.http_post(url, data)

    def get(self, request, *args, **kwargs):
        signature = request.GET.get("signature")
        timestamp = request.GET.get("timestamp")
        nonce = request.GET.get("nonce")
        echostr = request.GET.get("echostr")
        alist = [self.TOKEN, timestamp, nonce]
        alist.sort()
        sha1 = hashlib.sha1()
        map(sha1.update, alist)
        # Logs.print_log("echostr in get", echostr)

        if signature == sha1.hexdigest():
            return HttpResponse(echostr)

    def post(self, request, *args, **kwargs):

        self.create_menu()

        xml_str = request.body
        # Logs.print_log("xml", xml_str)
        xml = ET.fromstring(xml_str)
        msgType = xml.find('MsgType').text
        fromUserName = xml.find('FromUserName').text
        toUserName = xml.find('ToUserName').text
        createTime = xml.find('CreateTime').text
        if msgType == "event":
            event = xml.find('Event').text
            event_key = xml.find('EventKey').text
            if event == "subscribe":
                content = u"感谢您的关注，我们将为您奉上丰富的健康资讯和产品福利！" \
                          u"<a href=\"http://mp.weixin.qq.com/s" \
                          u"?__biz=MzA3OTM5NTczMg==&mid=200765097&idx=1&sn=9bce8d3212cc25fde3dccd56256b05e6#rd\">" \
                          u"点击了解产品详情</a>"
                # print "subscribe"
            elif event == "unsubscribe":
                content = u"亲，我们有什么地方做的令您不满意吗？"
                # print "unsubscribe"
            elif event == "CLICK":
                if event_key == "VK001":
                    print "VK001"
                    content = u"4月 北京 春暖花开 精彩会员活动敬请期待！"
                elif event_key == "VK003":
                    content = u"有爱下午茶 和菌菌姐、菌小妹一起品茶、聊天、谈理想 每次不同主题每次不同体验 每次不同收获 我们在等你！ 有爱下午茶仅限爱益维会员参加！"
                elif event_key == "VK004":
                    content = u"请赐给我您的名字，菌小妹给您查查！"
                elif event_key == "VK005":
                    content = "1"
                elif event_key == "VK006":
                    content = u"""爱益维 无添加益生菌的倡导者，爱益维拥有爱分享、爱生活的团队小伙伴儿！
                    我们不断努力，带给爱益维会员线上购物线下活动的双重体验。
                    我们用心用爱不断创新，不断努力，致力于用无添加的食品激活肠道本源动力。
                    我们在全球范围内，用心甄选优质合作伙伴，只选用健康、纯天然、高质量、高标准的产品。
                    我们比你更加关注你的健康，我们比你更加关注产品品质，因为我们坚信，只有更加优质的产品才是你真正的需要。
                    爱益维优质益生菌固体饮料，旗下拥有力护、乐畅两个子品牌，优选来自全球顶级益生菌供应商美国杜邦的优质菌种，
                    搭配100%水果粉，通过美国杜邦精湛工艺，制成在常温下依然保持菌株活力的益生菌固体饮料。
                    临床实验表明：益生菌的补充有助于免疫系统、肠道健康、预防癌症等支持作用。
                    爱益维优质益生菌固体饮料，独立小包装，每小包活力菌株≥100亿CFU，
                    随时随地自己冲着喝，方便，有效，平衡肠道每日所需益生菌，持续改善肠道菌群环境，
                    建立健康的肠道菌群系统。 优质体质，从肠道健康开始。
                    爱益维优质益生菌，激活你的肠道细胞，力护全家健康。"""
                elif event_key == "VK007":
                    content = u"""搬个小凳子，听菌菌姐讲讲肠道的故事！
                    输入您的问题，菌菌姐会很快回复哟！
                    凳子太沉？ 登录 www.iewee.com 爱益维官网了解更多肠道健康知识"""
                elif event_key == "VK008":
                    content = "http://www.iewee.com"
                elif event_key == "VK010":
                    content = "http://weidian.com/?userid=762451&wfr=wx"
                elif event_key == "VK011":
                    content = u"进入1号店www.yhd.com，搜索”爱益维“来寻我"
                elif event_key == "VK012":
                    content = u"""淘宝搜索”爱益维“进入淘宝官方旗舰店或网络授权经销商店铺购买
                    爱益维淘宝官方旗舰店 http://atwee.taobao.com/"""

        elif msgType == "text":
            content = xml.find('Content').text

        myMsgType = "text"
        # device_type = xml.find("DeviceType").text
        # device_id = xml.find("DeviceID").text
        # # Logs.print_log("DeviceType", device_type)
        # # Logs.print_log("DeviceID", device_id)
        reply = '''
            <xml>
            <ToUserName>%s</ToUserName>
            <FromUserName>%s</FromUserName>
            <CreateTime>%s</CreateTime>
            <MsgType>%s</MsgType>
            <Content>%s</Content>
            </xml>
        ''' % (fromUserName, toUserName, str(int(time.time())), myMsgType, content,
               # device_type, device_id
               )
        return HttpResponse(reply, content_type="application/xml")

    @csrf_exempt
    def dispatch(self, *args, **kwargs):
        return super(WeixinInterface, self).dispatch(*args, **kwargs)


# 读取文件内容
def read_file(file_path="media_root/shop/purchase_url.txt", buf_size=262144):
    # print "start read file"
    f = open(file_path, "rb")
    # print "read file OK"
    while True:
        c = f.read(buf_size)
        if c:
            yield c
        else:
            break
    f.close()


@api_view(http_method_names=["GET"])
def download_file(request):
    file_path = "media_root/user/image_small.jpeg"
    # file_path = request.GET.get("filename", None)
    from django.http import StreamingHttpResponse
    response = StreamingHttpResponse(read_file(file_path))
    response['Content-Type'] = 'application/octet-stream'
    response['Content-Disposition'] = 'attachment;filename="{0}"'.format(file_path)
    return response


# *******************************修正非大众点评场所的地址*************************************************
def get_address_by_longitude_and_latitude(longitude, latitude):
    result = None
    start_longitude = Digit.get_floor_float(float(longitude))
    end_longitude = start_longitude + 0.01
    start_latitude = Digit.get_floor_float(float(latitude))
    end_latitude = start_latitude + 0.01

    addresses = Address.objects.filter(
        longitude__gt=start_longitude,
        longitude__lt=end_longitude,
        latitude__gt=start_latitude,
        latitude__lt=end_latitude,
    ).order_by("id")

    if addresses:
        result = addresses[0]
        for address in addresses:
            if address.id == result.id:
                continue
            clean_one_address(address.id)

    return result


def reset_address():
    addresses = Address.objects.all()
    for address in addresses:
        address.audit = -1
        address.save()


@api_view(http_method_names=["GET"])
def revise_not_dianping_shop_address(request):
    reset_address()
    not_dianping_shop = Shop.objects.filter(dianping_business_id=0)
    for p in not_dianping_shop:
        longitude = p.dianping_longitude
        latitude = p.dianping_latitude
        address = get_address_by_longitude_and_latitude(longitude, latitude)
        if address:
            p.address_id = address.id
        else:
            p.address_id = 0
        p.save()
    return Response("OK")


# *******************************修正非大众点评场所的地址*************************************************
# *******************************版块数据转换*************************************************

def get_all_categories():
    result = list()
    cs = ForumCategory.objects.all()
    for c in cs:
        temp = dict()
        temp["id"] = c.id
        temp["name"] = c.name
        result.append(temp)
    return result


def get_category_id(name):
    try:
        category = ForumCategory.objects.get(name=name)
        return category.id
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return 0


def convert_post_data(current_category_id, new_category_id):
    posts = ForumPost.objects.filter(category_id=current_category_id)
    for p in posts:
        p.category_id = new_category_id
        p.save()


def convert_category_data(current_category_id, new_category_id):
    try:
        # current_category_id = get_category_id(current_category)
        # new_category_id = get_category_id(new_category)
        convert_post_data(current_category_id, new_category_id)
        return True
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return False


def convert_category_data_interface(request):
    result = dict()
    result["success"] = False
    current_category_id = request.GET.get("current_category", 0)
    new_category_id = request.GET.get("new_category", 0)
    categories = get_all_categories()
    if current_category_id and new_category_id:
        result["success"] = convert_category_data(current_category_id, new_category_id)

    return render(request, "forum/convert_category_data.html",
                  {
                      "success": str(result["success"]),
                      "categories": categories,
                      "current_category_id": int(current_category_id),
                      "new_category_id": int(new_category_id)
                  })


# *******************************版块数据转换*************************************************
# *******************************TV 数据接口*************************************************

@api_view(http_method_names=["POST", "GET"])
def get_tv_data_old(request):
    mac_address = request.data.get("mac_address", None)
    if not mac_address:
        mac_address = request.GET.get("mac_address", None)
    if mac_address:
        latest = Detectors.get_latest(mac_address)
        if latest:
            info = Detectors.get_info(latest)
        else:
            info = dict()
    else:
        info = dict()
    return Response(info)


@api_view(http_method_names=["POST"])
def get_tv_data(request):
    result = list()
    user_id = request.data.get("user_id", 0)
    if not user_id:
        return Response(result)
    dr_none = Detectors.get_none()
    drs = Detectors.get_detector_relations_by_user(dr_none, user_id)
    if not drs:
        return Response(result)
    for dr in drs:
        latest = Detectors.get_latest(dr.mac_address)
        if latest:
            info = Detectors.get_info(latest)
        else:
            info = dict()
        result.append(info)

    return Response(result)


@api_view(http_method_names=["GET", "POST"])
def get_tv_data_4_app_old(request):
    # shop_id = request.data.get("shop_id", 0)
    shop_id = request.data.get("shop_id", 0)
    if not shop_id:
        shop_id = request.GET.get("shop_id", 0)
    mac_address = None
    if shop_id:
        detector = Detectors.get_detector_relation_by_shop(shop_id)
        if detector:
            mac_address = detector.mac_address

    if mac_address:
        latest = Detectors.get_latest(mac_address)
        if latest:
            info = Detectors.get_info(latest)
        else:
            info = dict()
    else:
        info = dict()
    return Response(info)


@api_view(http_method_names=["POST"])
def get_tv_data_4_app(request):
    result = list()
    shop_id = request.data.get("shop_id", 0)
    mac_address = None
    if shop_id:
        drs = Detectors.get_detector_relation_by_shop(shop_id)
        if not drs:
            return result
        for dr in drs:
            if dr:
                mac_address = dr.mac_address
            if mac_address:
                latest = Detectors.get_latest(mac_address)
                if latest:
                    info = Detectors.get_info(latest)
                else:
                    info = dict()
            else:
                info = dict()
            result.append(info)
    return Response(result)


# *******************************TV 数据接口*************************************************
# *******************************自动添加优惠券*************************************************

def add_coupon(coupon_dict):
    try:
        youzan_sequence = coupon_dict.get("youzan_sequence", None)
        sequence = coupon_dict.get("sequence", None)
        coupon_value = coupon_dict.get("coupon_value", 0)
        valid_days = coupon_dict.get("valid_days", 30)
        valid_start = coupon_dict.get("valid_start", None)
        valid_end = coupon_dict.get("valid_end", None)
        is_out = coupon_dict.get("is_out", False)

        if not (youzan_sequence and sequence and coupon_value and valid_days and valid_start and valid_end):
            return None
        coupon_object = Coupon(
            sequence=sequence,
            youzan_sequence=youzan_sequence,
            coupon_value=coupon_value,
            is_valid=True,
            valid_days=valid_days,
            valid_start=valid_start,
            valid_end=valid_end,
            is_out=is_out
        )

        descriptions = coupon_dict.get("descriptions", None)
        product = coupon_dict.get("product", 0)
        channel_id = coupon_dict.get("channel_id", 0)
        use_id = coupon_dict.get("use_id", 0)

        if descriptions:
            coupon_object.descriptions = descriptions
        if product:
            coupon_object.product = product
        if channel_id:
            coupon_object.channel_id = channel_id
        if use_id:
            coupon_object.use_id = use_id
        coupon_object.save()
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)


@api_view(http_method_names=["GET"])
def auto_add_coupons(request):
    path = request.GET.get("path", None)
    if path:
        path = "media_root/shop/coupon/" + path
    else:
        return Response(u"请输入文件名！")
    coupon_value = int(request.GET.get("coupon_value", 0))
    valid_days = int(request.GET.get("valid_days", 30))
    descriptions = request.GET.get("descriptions", None)
    product = int(request.GET.get("product", 1))
    channel_id = int(request.GET.get("channel_id", 0))
    use_id = int(request.GET.get("use_id", 0))
    valid_start_str = "2015-11-27 14:14:06"
    valid_end_str = "2015-12-31 00:00:00"
    valid_start = Datetimes.string_to_clock(valid_start_str)
    valid_end = Datetimes.string_to_clock(valid_end_str)

    from common.files import Files
    youzan_sequences = Files.get_file_content(path)
    if youzan_sequences:
        # youzan_sequences = youzan_sequences.split(",")
        youzan_sequences = youzan_sequences.split("\n")

    else:
        return Response(u"没有相应的文件或文件中没有数据！")

    for youzan in youzan_sequences:
        if not youzan:
            continue
        coupon_dict = dict()
        coupon_dict["youzan_sequence"] = youzan
        coupon_dict["sequence"] = Coupons.generate_uuid()
        coupon_dict["coupon_value"] = coupon_value
        coupon_dict["valid_days"] = valid_days
        coupon_dict["descriptions"] = descriptions
        coupon_dict["product"] = product
        coupon_dict["channel_id"] = channel_id
        coupon_dict["use_id"] = use_id
        coupon_dict["valid_start"] = valid_start
        coupon_dict["valid_end"] = valid_end

        add_coupon(coupon_dict)
    return Response("OK")


# *******************************自动添加优惠券*************************************************
# *******************************address test*************************************************

@api_view(http_method_names=["GET"])
def test_address(request):
    from common.addresses import Addresses
    address = Address.objects.all()[0]
    return Response(Addresses.get_info(address))


# *******************************address test*************************************************
# *******************************pm25 test*************************************************


@api_view(http_method_names=["GET"])
def test_pm25(request):
    from common.scores import Scores
    return Response(Scores.get_pm25_node_info(100))


# *******************************pm25 test*************************************************
# *******************************publish categories by shop category test*************************************************

@api_view(http_method_names=["GET"])
def test_publish_categories(request):
    shop_category = request.GET.get("shop_category", None)
    return Response(
        PublishCategories.get_category_info_by_shop_category(PublishCategories.CATEGORIES_PATH, shop_category))


# *******************************publish categories by shop category test*************************************************
# *******************************发布统计*************************************************


def publish_statistics(request):
    result = list()
    now = datetime.datetime.now()
    start = request.GET.get("start", None)
    end = request.GET.get("end", now)
    phone = request.GET.get("phone", None)
    try:
        user = None
        if phone:
            user = Users.get_user(username=phone)
        if user is not None:
            publishes = Publishes.get_publishes_by_user_sometime(user.id, start, end)
        else:
            publishes = Publishes.get_publishes_by_datetime(start, end)

        publishes_by_user = publishes.values("user_id").annotate(count=Count("user_id"))
        publishes_by_user = list(publishes_by_user)
        for p in publishes_by_user:
            temp = dict()
            temp["username"] = User.objects.get(id=p["user_id"])
            temp["count"] = p["count"]
            result.append(temp)
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)

    return render(request, "publish/statistics.html", {"data": result})


# *******************************发布统计*************************************************
# *******************************产生二维码*************************************************
from common.qrcodes import Qrcodes


def get_qrcode(request):
    data = request.GET.get("data", "http://www.sangebaba.com/")
    image_stream = Qrcodes.generate_image_stream(data)
    response = HttpResponse(image_stream, content_type="image/png")
    response['Last-Modified'] = Datetimes.get_now()
    response['Cache-Control'] = 'max-age=31536000'
    return response


# *******************************产生二维码*************************************************
# *******************************统计发布信息*************************************************

from common.shops import Shops


@api_view(http_method_names=["GET"])
def get_publish_data(request):
    data = list()
    days = int(request.GET.get("days", 14))
    start = Datetimes.get_some_day(days)
    day_start = Datetimes.get_day_start(start)
    today = Datetimes.get_now()
    day_end = Datetimes.get_day_end(today)

    publishes = Publishes.get_publishes_by_datetime(day_start, day_end)
    if publishes:
        for p in publishes:
            temp = Publishes.get_info(p)
            temp_needed = dict()
            temp_needed["created_at"] = temp["created_at"]
            temp_needed["audit"] = temp["audit"]
            shop = Shops.get_shop(temp["shop_id"])
            if shop:
                temp_needed["shop"] = Shops.get_shop_info(shop)
            else:
                temp_needed["shop"] = dict()

            user = Users.get_user_by_id(temp["user_id"])
            if user:

                temp_needed["user"] = Users.get_user_info(user.userextension)
            else:
                temp_needed["user"] = dict()

            data.append(temp_needed)
    # return Response(data)
    return render(request, "publish/statistics_detail.html",
                  {"data": data, "count": len(data), "start": day_start, "end": day_end})


# *******************************统计发布信息*************************************************
# *******************************批量添加优惠券*************************************************


def get_name_and_ids(objs):
    try:
        return [{"id": obj.id, "name": obj.name} for obj in objs]
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
        return list()


def get_coupon_by_product_and_channel(product, channel, is_out=False):
    result = dict()
    try:
        coupon_dict = dict()
        coupon_dict["product"] = product.id
        coupon_dict["channel_id"] = channel.id
        coupon_dict["is_out"] = is_out
        coupons = Coupons.get_them(coupon_dict)
        if coupons:
            result["product"] = product.name
            result["channel"] = channel.name
            result["coupon_value"] = ",".join(
                [str(c["coupon_value"]) for c in coupons.values("coupon_value").distinct()])
            result["count"] = coupons.count()
            result["descriptions"] = ",".join([c["descriptions"] for c in coupons.values("descriptions").distinct()])
    except Exception as ex:
        Logs.print_current_function_name_and_line_number(ex)
    return result


def show_coupons(request):
    from common.products import Products
    from common.channels import Channels
    product_object = Products.get_all()
    channel_object = Channels.get_all()
    products = get_name_and_ids(product_object)
    channels = get_name_and_ids(channel_object)

    product_id = request.GET.get("product", 0)
    channel_id = request.GET.get("channel", 0)
    search = request.GET.get("search", None)
    search_all = request.GET.get("search_all", None)

    coupon_info = None
    coupon_all = []
    if search:
        if product_id and channel_id:
            product = Products.get(product_id)
            channel = Channels.get(channel_id)
            coupon_info = get_coupon_by_product_and_channel(product, channel)
    elif search_all:
        for product in list(product_object):
            for channel in list(channel_object):
                temp = get_coupon_by_product_and_channel(product, channel)
                if temp:
                    coupon_all.append(temp)
                temp = get_coupon_by_product_and_channel(product, channel, True)
                if temp:
                    coupon_all.append(temp)

    return render(request, "test/show_coupons.html",
                  {
                      # "file": the_file,
                      # "content": content,
                      "products": products,
                      "channels": channels,
                      "coupon": coupon_info,
                      "coupons": coupon_all
                      # "count": count,
                  })


def bulk_create_coupons(request):
    from common.products import Products
    from common.channels import Channels
    submit = request.data.get("submit", None)
    product_object = Products.get_all()
    channel_object = Channels.get_all()
    products = get_name_and_ids(product_object)
    channels = get_name_and_ids(channel_object)
    count = 0
    the_file = None
    content = []

    if submit:
        coupon_value = request.data.get("coupon_value", 0)
        descriptions = request.data.get("descriptions", "")
        product = request.data.get("product", 0)
        channel = request.data.get("channel", 0)
        is_out = request.data.get("is_out", False)

        valid_start_str = "2015-11-27 14:14:06"
        valid_end_str = "2015-12-31 00:00:00"
        valid_start = Datetimes.string_to_clock(valid_start_str)
        valid_end = Datetimes.string_to_clock(valid_end_str)

        valid_start = request.data.get("valid_start", valid_start)
        valid_end = request.data.get("valid_end", valid_end)

        try:
            the_file = request.FILES["txt_file"]
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            the_file = None

        if the_file:
            content = the_file.read().split("\r\n")

            for c in content:
                if not c:
                    continue
                coupon_dict = dict()
                coupon_dict["youzan_sequence"] = c
                coupon_dict["sequence"] = Coupons.generate_uuid()
                coupon_dict["coupon_value"] = coupon_value
                coupon_dict["valid_days"] = 30
                coupon_dict["descriptions"] = descriptions
                coupon_dict["product"] = product
                coupon_dict["channel_id"] = channel
                coupon_dict["use_id"] = 0
                coupon_dict["valid_start"] = valid_start
                coupon_dict["valid_end"] = valid_end
                coupon_dict["is_out"] = is_out
                add_coupon(coupon_dict)
                count += 1

    return render(request, "test/bulk_create_coupons.html",
                  {
                      "file": the_file,
                      "content": content,
                      "products": products,
                      "channels": channels,
                      "count": count,
                  })
    # return Response(request.data.get("txt_file"))


# *******************************批量添加优惠券*************************************************
# *******************************社区统计*************************************************
from common.forums import Forums


def get_all_users_from_post_and_reply():
    post_users = Forums.get_users_from_post()
    post_user_ids = [p["owner_id"] for p in post_users]
    reply_users = Forums.get_users_from_reply()
    reply_user_ids = [r["owner_id"] for r in reply_users]
    all_ids = post_user_ids + reply_user_ids
    return list(set(all_ids))


def forum_statistics(request):
    data = list()
    user_ids = get_all_users_from_post_and_reply()
    for uid in user_ids:
        temp = dict()
        user = Users.get_user_by_id(uid)
        temp["nickname"] = user.userextension.nickname
        posts = Forums.get_post_by_user(uid)
        temp["post_count"] = posts.count()

        won_count = 0
        reply_count_from_post = 0
        for p in posts:
            p_users = p.win_users.all()
            won_count += p_users.count()
            reply_count_from_post += Forums.get_reply_for_post(p.id).count()
        temp["post_won_count"] = won_count
        temp["reply_count_from_post"] = reply_count_from_post

        win_count = 0
        all_post = Forums.get_post()
        for p in all_post:
            p_users = p.win_users.all()
            p_user_ids = [pu.user_id for pu in p_users]
            if uid in p_user_ids:
                win_count += 1
        temp["post_win_count"] = win_count

        replies = Forums.get_reply_by_user(uid)
        temp["reply_count"] = replies.count()
        data.append(temp)

    return render(request, "forum/statistics.html", {"data": data})


# *******************************社区统计*************************************************
# *******************************人员统计*************************************************


def get_user_count_by_datetime(request):
    submit = request.GET.get("submit", None)
    start = request.GET.get("start", None)
    end = request.GET.get("end", None)
    if start:
        starts = start.split("T")
        start_str = starts[0] + " " + starts[1] + ":00"
        start_clock = Datetimes.string_to_clock(start_str)
    else:
        start_clock = None
    if end:
        ends = end.split("T")
        end_str = ends[0] + " " + ends[1] + ":59"
        end_clock = Datetimes.string_to_clock(end_str)
    else:
        end_clock = None
    if submit:
        count = Users.get_users_by_datetime(start_clock, end_clock).count()
    else:
        count = -1
    return render(request, "user/statistics.html", {"count": count, "start": start, "end": end})


# *******************************人员统计*************************************************
# *******************************TEST*************************************************

@api_view(http_method_names=["GET"])
def test(request):
    # from common.forums import Forums
    result = dict()
    caches = cache.get("online_ips", [])
    result["count"] = len(caches)
    result["online_ips"] = caches
    return Response(result)
    user_id = request.GET.get("user_id", 1)
    user = Users.get_user_by_id(user_id)
    return Response(Users.get_user_info(user.userextension))


# *******************************TEST*************************************************
# *******************************场所分类显示*************************************************

@api_view(http_method_names=["GET"])
def get_shop_data(request):
    data = list()
    title = ["category", "name", "dianping_address", "dianping_telephone"]
    categories = Shops.get_all_shop_categories()
    for c in categories:
        the_shops = Shops.get_all_shop_by_category(c["dianping_categories"])
        for s in the_shops:
            temp_shop = list()
            temp_shop.append(c["dianping_categories"])
            temp_shop.append(s.name)
            temp_shop.append(s.dianping_address)
            temp_shop.append(s.dianping_telephone)
            data.append(temp_shop)

    Files.save_csv(data, title)
    return Response("OK")


# *******************************场所分类显示*************************************************

# *******************************活动后台显示*************************************************
from common.special_activity import SpecialActivity
from common import special_activity


@api_view(http_method_names=["GET"])
def place(request):
    result = dict()
    place_phones = SpecialActivity.get_phones(special_activity.PLACE_PHONE)
    red_envelope_99 = SpecialActivity.get_phones(special_activity.RED_ENVELOPE_99)
    red_envelope_119 = SpecialActivity.get_phones(special_activity.RED_ENVELOPE_119)
    temp = list()
    for u in place_phones:
        not_exist_phone = SpecialActivity.get_not_exist_phone(u)
        if not_exist_phone:
            temp.append(not_exist_phone)
    result["place"] = temp
    temp = list()
    for u in red_envelope_99:
        not_exist_phone = SpecialActivity.get_not_exist_phone(u)
        if not_exist_phone:
            temp.append(not_exist_phone)
    result["99"] = temp
    temp = list()
    for u in red_envelope_119:
        not_exist_phone = SpecialActivity.get_not_exist_phone(u)
        if not_exist_phone:
            temp.append(not_exist_phone)
    result["119"] = temp

    return Response(result)


# *******************************活动后台显示*************************************************

# *******************************商用检测器后台*************************************************
def detector_add(request):
    username = request.data.get("username", None)
    mac_address = request.data.get("mac_address", None)
    city = request.data.get("city", None)
    address = request.data.get("address", None)
    shop_id = request.data.get("post_id", 0)
    submit = request.data.get("submit", None)
    threshold = int(request.data.get("threshold", 0))
    success = u"否"
    if submit and username and mac_address and city and address and shop_id:
        user = Users.get_user(username)
        detector_relation = dict()
        detector_relation["user_id"] = user.id
        detector_relation["mac_address"] = mac_address
        detector_relation["city"] = city
        detector_relation["address"] = address
        detector_relation["shop_id"] = shop_id
        detector_relation["threshold"] = threshold
        if Detectors.add_detector_relations(detector_relation):
            success = u"是"
    return render(request, "detector/add.html", {"success": success})


def get_some_shop_ids(city, shop_name, shop_address):
    shop_dict = dict()
    shop_dict["city"] = city
    shop_dict["shop_name"] = shop_name
    shop_dict["shop_address"] = shop_address
    return [str(s.id) for s in Shops.get_shops(shop_dict)]


def detector_search_shop(request):
    search = request.GET.get("search", None)
    city = request.GET.get("city", "")
    shop_name = request.GET.get("shop_name", "")
    shop_address = request.GET.get("shop_address", "")
    shop_ids_str = ""
    shop_info = list()
    if search:
        shop_ids = get_some_shop_ids(city, shop_name, shop_address)
        shop_ids_str = ",".join(shop_ids)
        for sid in shop_ids:
            s = Shops.get_shop(sid)
            temp = Shops.get_shop_info(s)
            category = ShopCategories.get_category(s.category_id)
            if category:
                temp["category_name"] = category.name
            if temp:
                shop_info.append(temp)

    return render(request, "detector/search.html", {
        "shop_ids": shop_ids_str,
        "city": city,
        "shop_name": shop_name,
        "shop_address": shop_address,
        "shop_info": shop_info
    })

# *******************************商用检测器后台*************************************************


# ****************************test parts*************************************************
