# -*- coding: utf-8 -*-

from datetimes import Datetimes
from publishes import Publishes
from comments import Comments
from shop_categories import ShopCategories
from logs import Logs
from user_publish_relations import UserPublishRelationMethods
from user_shop_relations import UserShopRelationMethods
from shops import Shops


class Hot(object):
    @staticmethod
    def get_hot_shop_compute_data(shop, user_id=0):
        result = dict()
        now = Datetimes.get_now()
        now_utc = Datetimes.transfer_datetime(now, is_utc=False)
        now_utc = Datetimes.naive_to_aware(now_utc)
        seven_days_before = Datetimes.get_some_day(7, 0, now)
        seven_days_before_utc = Datetimes.transfer_datetime(seven_days_before, is_utc=False)
        seven_days_before_utc = Datetimes.naive_to_aware(seven_days_before_utc)

        publishes = Publishes.get_shown_publishes(user_id).filter(
            shop_id=shop.id, created_at__gte=seven_days_before_utc, created_at__lte=now_utc)
        result["publish_count"] = publishes.count()
        persons = publishes.values("user_id").distinct()
        result["person_count"] = persons.count()
        pm2_5s = publishes.filter(PM2_5__gt=-1)
        result["PM2_5_count"] = pm2_5s.count()
        result["comment_count"] = sum([Comments.get_count_by_publish(p.id) for p in publishes])
        result["win_count"] = \
            sum([UserPublishRelationMethods.get_win_attribute_count_by_publish(p) for p in publishes])
        result["lost_count"] = \
            sum([UserPublishRelationMethods.get_lost_attribute_count_by_publish(p) for p in publishes])
        result["recommended_count"] = UserShopRelationMethods.get_recommended_count_by_shop(shop)
        result["not_recommended_count"] = UserShopRelationMethods.get_not_recommended_count_by_shop(shop)
        return result

    @staticmethod
    def hot_shop_compute(shop, user_id=0):
        """
        :param shop: 场所
        :param user_id: 用户ID
        :return:
        1. 总发布条数 * 3
        2. 发布人数 * 3
        3. pm2.5数 * 5
        4. 发布的总回复数 * 2
        5. 总点赞数量 * 1
        6. 总点踩数量 * 1
        7. 推荐 +2
        8. 不推荐 -2
        9. 最近七天内
        """

        total_score = 0
        counts = Hot.get_hot_shop_compute_data(shop, user_id)

        total_score += counts["publish_count"] * 3
        total_score += counts["person_count"] * 3
        total_score += counts["PM2_5_count"] * 5
        total_score += counts["comment_count"] * 2
        total_score += counts["win_count"] * 1
        total_score += counts["lost_count"] * 1
        total_score += counts["recommended_count"] * 2
        total_score -= counts["not_recommended_count"] * 2
        return total_score

    @staticmethod
    def is_shown_in_hot(shop):
        if ShopCategories.is_weighted(shop):
            return True
        elif ShopCategories.is_home_or_company(shop.category):
            return False
        else:
            return True

    @staticmethod
    def get_info4hot(shop, longitude, latitude, user_id):
        result = dict()
        publishes = Publishes.get_publishes_by_shop(shop.id).order_by("-id")
        if publishes:
            publish = publishes[0]
            result = Publishes.get_publish_info4hot(publish, longitude, latitude, user_id)

        shop_result = Shops.get_shop_information4hot(shop, longitude, latitude, user_id)
        result = dict(result.items() + shop_result.items())
        return result

    @staticmethod
    def get_shop_info_with_weight(city, user_id, start_id, count, longitude, latitude, loaded_shop_ids):
        result = dict()
        index = 0
        try:
            data = list()
            shops = Shops.get_shop_with_weight(city)
            if not shops:
                result["data"] = list()
                result["index"] = index
                return result
            shop_ids = [s.id for s in shops]
            start_flag = 0

            if (not start_id) or (start_id in shop_ids):
                for s in shops:
                    if str(s.id) in loaded_shop_ids:
                        continue
                    if not Hot.is_shown_in_hot(s):
                        continue
                    if (start_id == s.id) or (not start_id):
                        start_flag = 1
                    if start_flag:
                        temp_score = Hot.hot_shop_compute(s, user_id)
                        temp = Hot.get_info4hot(s, longitude, latitude, user_id)
                        if temp:
                            temp["hot_score"] = temp_score
                            data.append(temp)
                            index += 1
                        if index >= count:
                            break
                result["data"] = data
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result["data"] = list()
        result["index"] = index
        return result

    @staticmethod
    def get_shop_info_without_weight(city, user_id, start_id, count, longitude, latitude, loaded_shop_ids, index):
        result = dict()
        if index >= count:
            result["index"] = list()
            return result
        the_index = index
        try:
            data_positive = list()
            data_zero = list()
            data_negative = list()
            shops = Shops.get_shop_without_weight(city)
            if not shops:
                result["index"] = list()
                return result
            shop_ids = [s.id for s in shops]
            start_flag = 0

            if (not start_id) or (index > 0) or (start_id in shop_ids):
                for s in shops:
                    if str(s.id) in loaded_shop_ids:
                        continue
                    if not Hot.is_shown_in_hot(s):
                        continue
                    if (index > 0) or (start_id == s.id) or (not start_id):
                        start_flag = 1
                    if start_flag:
                        temp_score = Hot.hot_shop_compute(s, user_id)
                        temp = Hot.get_info4hot(s, longitude, latitude, user_id)
                        if temp:
                            temp["hot_score"] = temp_score
                            if temp_score > 0:
                                data_positive.append(temp)
                            elif temp_score == 0:
                                data_zero.append(temp)
                            else:
                                data_negative.append(temp)
                            the_index += 1
                        if the_index >= count:
                            break
                    data_positive = sorted(data_positive, key=lambda e: e["hot_score"], reverse=True)
                    data_zero = sorted(data_zero, key=lambda e: e["hot_score"], reverse=True)
                    data_negative = sorted(data_negative, key=lambda e: e["hot_score"], reverse=True)
                    result["data"] = data_positive + data_zero + data_negative
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result["data"] = list()
        return result
