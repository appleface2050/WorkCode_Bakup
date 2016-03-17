# -*- coding: utf-8 -*-

import hashlib
import json
from logs import Logs
from https import Https
import files

# 请替换appkey和secret
# DIANPING_APP_KEY = "1178164590"
# DIANPING_SECRET = "e99d8ba260a441f5952ba4743f00a310"
DIANPING_APP_KEY = "9909159835"
DIANPING_SECRET = "96bc99542c19430c900999ee24d4a9ab"
DIANPING_API_URL = "http://api.dianping.com/v1/business/find_businesses"
DIANPING_API_DEALS_URL = "http://api.dianping.com/v1/deal/get_deals_by_business_id"
# parameters
PARAMETER_FORMAT = "format"
PARAMETER_CITY = "city"
PARAMETER_LATITUDE = "latitude"
PARAMETER_LONGITUDE = "longitude"
PARAMETER_CATEGORY = "category"
PARAMETER_REGION = "region"
PARAMETER_LIMIT = "limit"
PARAMETER_RADIUS = "radius"
PARAMETER_OFFSET_TYPE = "offset_type"
PARAMETER_HAS_COUPON = "has_coupon"
PARAMETER_HAS_DEAL = "has_deal"
PARAMETER_KEYWORD = "keyword"
PARAMETER_SORT = "sort"
PARAMETER_BUSINESS_ID = "business_id"


class DianPings(object):
    DIANPING_RATING = [
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_0_0star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_0_5star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_1_0star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_1_5star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_2_0star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_2_5star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_3_0star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_3_5star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_4_0star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_4_5star.png",
        files.BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_5_0star.png"
    ]

    @staticmethod
    def get_value(kwargs, name, default=None):
        try:
            return kwargs.GET.get(name, default)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def add_value(parameter_set, name, kwargs, default=None):
        value = DianPings.get_value(kwargs, name, default=default)
        if type(value) == "int":
            value = str(value)
        if type(value) == "str":
            value = value.encode("utf8")
        if value:
            parameter_set.append((name, value))

    @staticmethod
    def get_extreme_value(kwargs, name, min_value, max_value, default=0):
        value = DianPings.get_value(kwargs, name)
        if not value:
            return default
        if int(value) > int(max_value):
            return max_value
        if int(value) < int(min_value):
            return min_value

    @staticmethod
    def set_business_parameter(kwargs):
        parameter_set = []
        DianPings.add_value(parameter_set, PARAMETER_FORMAT, kwargs, default="json")
        DianPings.add_value(parameter_set, PARAMETER_CATEGORY, kwargs)
        DianPings.add_value(parameter_set, PARAMETER_CITY, kwargs)
        DianPings.add_value(parameter_set, PARAMETER_HAS_COUPON, kwargs)
        DianPings.add_value(parameter_set, PARAMETER_HAS_DEAL, kwargs)
        DianPings.add_value(parameter_set, PARAMETER_KEYWORD, kwargs)
        DianPings.add_value(parameter_set, PARAMETER_LATITUDE, kwargs)
        DianPings.add_value(parameter_set, PARAMETER_LONGITUDE, kwargs)
        extreme_value = DianPings.get_extreme_value(kwargs, PARAMETER_LIMIT, 1, 40, 20)
        DianPings.add_value(parameter_set, PARAMETER_LIMIT, kwargs, default=extreme_value)
        extreme_value = DianPings.get_extreme_value(kwargs, PARAMETER_RADIUS, 1, 5000, 1000)
        DianPings.add_value(parameter_set, PARAMETER_RADIUS, kwargs, default=extreme_value)
        extreme_value = DianPings.get_extreme_value(kwargs, PARAMETER_OFFSET_TYPE, 1, 2, 1)
        DianPings.add_value(parameter_set, PARAMETER_OFFSET_TYPE, kwargs, default=extreme_value)
        DianPings.add_value(parameter_set, PARAMETER_REGION, kwargs)
        extreme_value = DianPings.get_extreme_value(kwargs, PARAMETER_SORT, 1, 9, 1)
        DianPings.add_value(parameter_set, PARAMETER_SORT, kwargs, default=extreme_value)
        return parameter_set

    @staticmethod
    def set_deal_parameter(kwargs):
        parameter_set = list()
        parameter_set.append(("city", kwargs["city"]))
        parameter_set.append(("business_id", kwargs["business_id"]))
        return parameter_set

    @staticmethod
    def get_codec(parameter_set):
        # 参数排序与拼接
        parameter_map = {}
        for pair in parameter_set:
            parameter_map[pair[0]] = pair[1]

        codec = DIANPING_APP_KEY
        for key in sorted(parameter_map.iterkeys()):
            # print key
            if isinstance(parameter_map[key], unicode):
                codec += key + str(parameter_map[key].encode("utf8"))
            else:
                codec += key + str(parameter_map[key])

        codec += DIANPING_SECRET
        return codec

    @staticmethod
    def get_sign(codec):
        return (hashlib.sha1(codec).hexdigest()).upper()

    @staticmethod
    def get_url(sign, parameter_set, source_url=DIANPING_API_URL):
        # 拼接访问的URL
        url_trail = "appkey=" + DIANPING_APP_KEY + "&sign=" + sign
        for pair in parameter_set:
            if isinstance(pair[1], unicode):
                url_trail += "&" + pair[0] + "=" + str(pair[1].encode("utf8"))
            else:
                url_trail += "&" + pair[0] + "=" + str(pair[1])

        request_url = source_url + "?" + url_trail
        return request_url

    @staticmethod
    def get_info_from_dianping(parameter_set, source_url=DIANPING_API_URL, data_key="businesses"):
        # 示例参数
        # parameter_set = set_parameter(kwargs)
        # 参数排序与拼接
        codec = DianPings.get_codec(parameter_set)
        # 签名计算
        sign = DianPings.get_sign(codec)
        # 拼接访问的URL
        request_url = DianPings.get_url(sign, parameter_set, source_url)
        # 模拟请求
        content = Https.http_get_action(request_url)
        businesses = json.loads(content).get(data_key, None)
        # Logs.print_log("businesses", businesses)
        if businesses:
            return businesses
        else:
            return []

    @staticmethod
    def parse_dianping_shop_info(dianping):
        result = dict()
        result["dianping_business_id"] = dianping.get("business_id", 0)
        result["dianping_name"] = dianping.get("name", None)
        result["dianping_address"] = dianping.get("address", None)
        result["dianping_categories"] = dianping.get("categories", None)
        result["dianping_city"] = dianping.get("city", None)
        result["dianping_telephone"] = dianping.get("telephone", None)
        result["dianping_regions"] = dianping.get("regions", None)
        result["dianping_avg_rating"] = dianping.get("avg_rating", None)
        result["dianping_longitude"] = dianping.get("longitude", None)
        result["dianping_latitude"] = dianping.get("latitude", None)
        result["dianping_business_url"] = dianping.get("business_url", None)
        result["dianping_coupon_description"] = dianping.get("coupon_description", None)
        result["dianping_coupon_id"] = dianping.get("coupon_id", None)
        result["dianping_coupon_url"] = dianping.get("coupon_url", None)
        result["dianping_deal_count"] = dianping.get("deal_count", None)
        result["dianping_deals"] = dianping.get("deals", None)
        result["dianping_has_coupon"] = dianping.get("has_coupon", None)
        result["dianping_has_deal"] = dianping.get("has_deal", None)
        result["dianping_has_online_reservation"] = dianping.get("has_online_reservation", None)
        result["dianping_online_reservation_url"] = dianping.get("online_reservation_url", None)
        result["dianping_photo_url"] = dianping.get("photo_url", None)
        result["dianping_s_photo_url"] = dianping.get("s_photo_url", None)
        result["dianping_rating_img_url"] = dianping.get("rating_img_url", None)
        result["dianping_rating_s_img_url"] = dianping.get("rating_s_img_url", None)
        result["dianping_deals_description"] = dianping.get("deals.description", None)
        result["dianping_deals_id"] = dianping.get("deals.id", None)
        result["dianping_deals_url"] = dianping.get("deals.url", None)
        return result
