# -*- coding: utf-8 -*-

from ..models import Shop
from objects import Objects
from math import radians, atan, tan, acos, cos, sin
from logs import Logs
import shop_categories
from dianpings import DianPings
import files
from publishes import Publishes
from scores import Scores
from user_shop_relations import UserShopRelationMethods
from geohash_code import GeohashCode


class Shops(object):
    @staticmethod
    # input lat_a 纬度A
    # input lng_a 经度A
    # input lat_b 纬度B
    # input lng_b 经度B
    # output distance 距离(km)
    def get_distance(lat_a, lng_a, lat_b, lng_b):
        if lat_a == lat_b and lng_a == lng_b:
            return 0
        ra = 6378.140  # 赤道半径 (km)
        rb = 6356.755  # 极半径 (km)
        flatten = (ra - rb) / ra  # 地球扁率
        rad_lat_a = radians(lat_a)
        rad_lng_a = radians(lng_a)
        rad_lat_b = radians(lat_b)
        rad_lng_b = radians(lng_b)
        pa = atan(rb / ra * tan(rad_lat_a))
        pb = atan(rb / ra * tan(rad_lat_b))
        xx = acos(sin(pa) * sin(pb) + cos(pa) * cos(pb) * cos(rad_lng_a - rad_lng_b))
        c1 = (sin(xx) - xx) * (sin(pa) + sin(pb)) ** 2 / cos(xx / 2) ** 2
        c2 = (sin(xx) + xx) * (sin(pa) - sin(pb)) ** 2 / sin(xx / 2) ** 2
        dr = flatten / 8 * (c1 - c2)
        distance = ra * (xx + dr)
        return distance

    @staticmethod
    def get_geohash_code(latitude, longitude, length=6):
        gc = GeohashCode(latitude, longitude, length)
        return gc.get()

    @staticmethod
    def add_shop(shop_dict):
        try:
            name = shop_dict.get("name", None)
            if name:
                shop = Shop(name=name)
                return Shops.update_shop(shop, shop_dict)
            else:
                return None
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Shops.get_none()

    @staticmethod
    def update_shop(shop, shop_dict):
        try:
            for (key, value) in shop_dict.items():
                Objects.set_value(shop, key, value)
            shop.save()
            return shop
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Shops.get_none()

    @staticmethod
    def get_shop(shop_id):
        try:
            return Shop.objects.get(id=shop_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Shops.get_none()

    @staticmethod
    def set_shop_has_detector(shop, has_detector):
        try:
            shop.has_detector = has_detector
            shop.save()
            return shop
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Shops.get_none()

    @staticmethod
    def get_shop_info(shop):
        result = Objects.get_object_info(shop)
        return result

    @staticmethod
    def get_distance_from_shop(shop, latitude, longitude):
        distance = -1
        try:
            if shop.dianping_business_id:
                distance = Shops.get_distance(latitude, longitude, shop.dianping_latitude, shop.dianping_longitude)
            else:
                distance = Shops.get_distance(latitude, longitude, shop.address.latitude, shop.address.longitude)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
        return distance

    @staticmethod
    def get_shop_information4hot(shop, longitude, latitude, user_id):
        result = dict()
        try:
            result["shop_id"] = shop.id
            result["shop_name"] = shop.name
            result["shop_price"] = shop.dianping_avg_price
            if shop.category:
                result["shop_category"] = shop.category.name
            else:
                result["shop_category"] = None
            if shop.dianping_city:
                result["city"] = shop.dianping_city
            else:
                result["city"] = None

            result["shop_rate"] = DianPings.DIANPING_RATING[int(2 * shop.dianping_avg_rating)]
            if shop.photo_url:
                result["shop_image"] = files.BASE_URL_4_IMAGE + shop.photo_url
            else:
                if shop.category:
                    result["shop_image"] = files.BASE_URL_4_IMAGE + shop.category.icon
                else:
                    result["shop_image"] = None
            result["distance"] = Shops.get_distance_from_shop(shop, latitude, longitude)
            if shop.dianping_business_id:
                result["is_from_dianping"] = True
            else:
                result["is_from_dianping"] = False

            publishes = Publishes.get_publishes_by_shop(shop.id)
            if publishes:
                result["publish_count"] = publishes.count()
            else:
                result["publish_count"] = 0

            score_dict = Scores.get_total_score(shop, user_id)
            score = Scores.get_concrete_score(score_dict)
            score_level = Scores.get_level_info(score)
            if score_level:
                result["score_key"] = score_level.get("key", "--")
            else:
                result["score_key"] = "--"

        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

        return result

    @staticmethod
    def get_shop_information4discovery(shop, longitude, latitude, user_id):
        result = dict()
        result["shop_id"] = shop.id
        result["business_url"] = shop.dianping_business_url
        result["deals_url"] = shop.dianping_deals_url
        result["shop_name"] = shop.name
        result["shop_price"] = shop.dianping_avg_price
        result["shop_category"] = shop.category.name
        if shop.dianping_business_id:
            result["city"] = shop.dianping_city
            result["shop_address"] = shop.dianping_city + shop.dianping_address
            result["is_from_dianping"] = True
        else:
            result["is_from_dianping"] = False
            if shop.address.china:
                result["city"] = shop.address.china.name
                result["shop_address"] = shop.address.china.name + shop.address.detail_address
            else:
                result["city"] = shop.dianping_city
                result["shop_address"] = shop.address.detail_address
        if shop.photo_url:
            result["shop_image"] = files.BASE_URL_4_IMAGE + shop.photo_url
        else:
            result["shop_image"] = None

        result["shop_rate"] = DianPings.DIANPING_RATING[int(2 * shop.dianping_avg_rating)]

        result["distance"] = Shops.get_distance_from_shop(shop, latitude, longitude)
        publishes = Publishes.get_publishes_by_shop(shop.id)
        result["publish_count"] = publishes.count()
        result["has_coupon"] = shop.dianping_has_coupon
        result["has_deal"] = shop.dianping_has_deal

        score = -1000
        if publishes.count() < 0:
            pass
        else:
            score_dict = Scores.get_total_score(shop, user_id)
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
        result["formaldehyde"] = shop.formaldehyde
        if shop.formaldehyde_image:
            result["formaldehyde_image"] = files.BASE_URL_4_IMAGE + shop.formaldehyde_image.name
            result["valid_score"] = True
        else:
            result["formaldehyde_image"] = None
            result["valid_score"] = False

        result["recommended_count"] = UserShopRelationMethods.get_recommended_count_by_shop(shop)
        result["not_recommended_count"] = UserShopRelationMethods.get_not_recommended_count_by_shop(shop)
        usr = UserShopRelationMethods.get_by_shop_and_user(shop.id, user_id)
        if usr:
            result["is_recommended"] = usr.is_recommended
        else:
            result["is_recommended"] = -1
        return result

    @staticmethod
    def get_shop_with_weight(city):
        return Shop.objects.filter(
            dianping_city__contains=city,
            weight__gt=0
        ).exclude(audit=0).order_by("-weight", "-id")

    @staticmethod
    def get_shop_without_weight(city):
        return Shop.objects.filter(
            dianping_city__contains=city
        ).exclude(
            weight__gt=0
        ).exclude(audit=0).order_by("-weight", "-id")

    @staticmethod
    def get_shop_by_city(shops, city):
        if shops.exists() and city:
            return shops.filter(
                dianping_city__contains=city
            ).exclude(audit=0).order_by("-id")
        elif city:
            return Shop.objects.filter(
                dianping_city__contains=city
            ).exclude(audit=0).order_by("-id")
        elif shops.exists():
            return shops
        else:
            return Shop.objects.exclude(audit=0).order_by("-id")

    @staticmethod
    def get_shop_by_name(shops, name):
        if shops.exists() and name:
            return shops.filter(
                name__contains=name
            ).exclude(audit=0).order_by("-id")
        elif name:
            return Shop.objects.filter(
                name__contains=name
            ).exclude(audit=0).order_by("-id")
        elif shops.exists():
            return shops
        else:
            return Shop.objects.exclude(audit=0).order_by("-id")

    @staticmethod
    def get_all_shop_categories(city=u"北京"):
        return Shop.objects.filter(dianping_city__contains=city).values("dianping_categories").distinct()

    @staticmethod
    def get_all_shop_by_category(category, city=u"北京"):
        return Shop.objects.filter(dianping_categories=category, dianping_city__contains=city)

    @staticmethod
    def get_shops(shop_dict):
        city = shop_dict.get("city", None)
        shop_name = shop_dict.get("shop_name", None)
        shop_address = shop_dict.get("shop_address", None)
        shops = Shop.objects.all()
        if city:
            shops = shops.filter(dianping_city__contains=city)
        if shop_name:
            shops = shops.filter(dianping_name__contains=shop_name)
        if shop_address:
            shops = shops.filter(dianping_address__contains=shop_address)

        return shops

    @staticmethod
    def get_none():
        return Shop.objects.none()

    @staticmethod
    def get_shops_by_geohash_code(latitude, longitude):
        gc = GeohashCode(latitude, longitude)
        bigger_block = gc.get_bigger_block()
        return Shop.objects.filter(geohash_code__startswith=bigger_block)

