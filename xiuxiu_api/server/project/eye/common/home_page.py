# -*- coding: utf-8 -*-

from hot import Hot
from nearby import Nearby
from discovery import Discovery
from logs import Logs


class HomePage(object):
    @staticmethod
    def clear_city(city):
        if city and len(city) == 2:
            return city
        if city:
            if city.endswith(u"市"):
                city = city[:-1]
        return city

    @staticmethod
    def add_city(city):
        if city:
            if city.endswith(u"市"):
                return city
            else:
                return city + u"市"
        return city

    @staticmethod
    def get_hot_shops_info(city, user_id, start_id, count, longitude, latitude, loaded_shop_ids):
        result = dict()
        city = HomePage.clear_city(city)
        info_with_weight = Hot.get_shop_info_with_weight(
            city, user_id, start_id, count, longitude, latitude, loaded_shop_ids
        )
        # Logs.print_log("info_with_weight", info_with_weight)
        info_without_weight = Hot.get_shop_info_without_weight(
            city, user_id, start_id, count, longitude, latitude, loaded_shop_ids, info_with_weight["index"]
        )
        result["data"] = info_with_weight["data"] + info_without_weight["data"]
        return result

    @staticmethod
    def get_nearby_shops_info(user_id, start_id, count, longitude, latitude, loaded_shop_ids):
        shops = Nearby.get_shops(longitude, latitude, user_id, loaded_shop_ids)
        return Nearby.get_shop_info(shops, user_id, start_id, count, longitude, latitude)

    @staticmethod
    def get_discovery_publishes_info(
            user_id,
            start_id,
            count,
            longitude,
            latitude,
            loaded_publish_ids,
            city,
    ):
        city = HomePage.clear_city(city)
        return Discovery.get_publish_info(user_id, start_id, city, count, longitude, latitude, loaded_publish_ids)
