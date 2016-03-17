# -*- coding: utf-8 -*-

from publishes import Publishes
from shops import Shops
from shop_categories import ShopCategories
from hot import Hot
from logs import Logs


class Nearby(object):
    @staticmethod
    def get_shops(longitude, latitude, user_id, loaded_shop_ids):
        publish_shop_ids = Publishes.get_shown_publishes(
            user_id
        ).all().values("shop_id").distinct().order_by("-shop_id")

        shops = list()
        for sid in publish_shop_ids:
            if str(sid["shop_id"]) in loaded_shop_ids:
                continue
            s = Shops.get_shop(sid["shop_id"])
            if s:
                shops.append(s)

        data = list()
        for shop in shops:
            temp = dict()
            temp["shop"] = shop
            temp["distance"] = Shops.get_distance_from_shop(shop, longitude, latitude)
            if temp["distance"] > 50:
                continue
            if ShopCategories.is_home_or_company(shop.category):
                continue
            data.append(temp)
        data.sort(lambda x, y: cmp(x["distance"], y["distance"]))
        return [d["shop"] for d in data]

    @staticmethod
    def get_shop_info(shops, user_id, start_id, count, longitude, latitude):
        result = list()
        try:
            index = 0
            flag = 0
            shop_ids = [s.id for s in shops]
            for shop in shops:
                if start_id and (start_id in shop_ids):
                    if shop.id == start_id:
                        flag = 1
                    if flag:
                        result.append(Hot.get_info4hot(shop, longitude, latitude, user_id))
                        index += 1
                        if index >= count:
                            break
                elif not start_id:
                    result.append(Hot.get_info4hot(shop, longitude, latitude, user_id))
                    index += 1
                    if index >= count:
                        break
                else:
                    pass
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
        return result
