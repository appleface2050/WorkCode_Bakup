# -*- coding: utf-8 -*-

from user_publish_relations import UserPublishRelationMethods
from datetimes import Datetimes
from users import Users
from redenvelopes import RedEnvelopes
from publishes import Publishes
from comments import Comments
from shop_categories import ShopCategories
from publish_categories import PublishCategories
from logs import Logs
from methods import Methods


class Discovery(object):
    @staticmethod
    # def get_publish_info4discovery(publish, longitude, latitude, user_id):
    def get_publish_info4discovery(publish, *params):
        result = {}
        longitude = params[0]
        latitude = params[1]
        user_id = params[2]
        try:
            if user_id == 0:
                result["attribute"] = -1
            else:
                result["attribute"] = UserPublishRelationMethods.get_attribute_by_user_and_publish(user_id, publish.id)

            result["publish_id"] = publish.id
            result["created_at"] = Datetimes.utc2east8(publish.created_at)
            result["win"] = UserPublishRelationMethods.get_win_attribute_count_by_publish(publish)
            result["lost"] = UserPublishRelationMethods.get_lost_attribute_count_by_publish(publish)

            if publish.user and publish.user.userextension:
                result["user_nickname"] = publish.user.userextension.nickname
            else:
                result["user_nickname"] = ""

            temp_image = Users.get_user_image(publish.user)
            result["user_small_image"] = temp_image["small_user_image"]

            temp_publish_images = Publishes.get_publish_images(publish)
            result["publish_image_url"] = temp_publish_images["publish_image_url"]
            result["publish_image_big_url"] = temp_publish_images["publish_image_big_url"]
            result["publish_image_medium_url"] = temp_publish_images["publish_image_medium_url"]
            result["publish_image_small_url"] = temp_publish_images["publish_image_small_url"]
            result["PM2_5"] = publish.PM2_5

            result["comment"] = Comments.get_by_publish(publish.id)
            result["content"] = publish.content

            result["bonus"] = RedEnvelopes.get_red_envelope_by_publish(publish.id)

            s = publish.shop
            big_category = ShopCategories.get_shop_big_category(publish.shop.category)

            if not big_category:
                big_category = ShopCategories.get_category_default()
            big_category_key = big_category.id
            result["big_category_name"] = big_category.name
            result["big_category_key"] = big_category.id

            if not big_category_key:
                result["category_operation"] = {}
            else:
                result["category_operation"] = PublishCategories.get_category_info_by_shop_category(big_category)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

        return result

    @staticmethod
    def get_the_publishes_info(publishes, user_id, start_id, count, longitude, latitude, loaded_publish_ids):
        result = list()
        # index = 0
        # params = [longitude, latitude, user_id]
        if not start_id:
            result = Methods.get_one_object_info_from_scratch(
                publishes,
                Discovery.get_publish_info4discovery,
                count,
                loaded_publish_ids,
                longitude,
                latitude,
                user_id
            )
            # for publish in publishes:
            #     if str(publish.id) in loaded_publish_ids:
            #         continue
            #     temp = Discovery.get_publish_info4discovery(publish, longitude, latitude, user_id)
            #     if temp and (index < count):
            #         result.append(temp)
            #         index += 1
            #         if index == count:
            #             break
        else:
            result = Methods.get_one_object_info_not_from_scratch(
                publishes,
                Discovery.get_publish_info4discovery,
                start_id,
                count,
                loaded_publish_ids,
                longitude,
                latitude,
                user_id
            )
            # publish_ids = [publish.id for publish in publishes]
            # if start_id in publish_ids:
            #     index_id = publish_ids.index(start_id)
            #     length = len(publish_ids) - index_id
            #     for i in xrange(length):
            #         publish = publishes[index_id + i]
            #         if str(publish.id) in loaded_publish_ids:
            #             continue
            #         temp = Discovery.get_publish_info4discovery(publish, longitude, latitude, user_id)
            #         if temp and (index < count):
            #             result.append(temp)
            #             index += 1
            #             if index == count:
            #                 break
        return result

    @staticmethod
    def get_publish_info(user_id, start_id, city, count, longitude, latitude, loaded_publish_ids):
        publishes = Publishes.get_shown_publishes(user_id)
        # Logs.print_log("get_publish_info count", publishes.count())
        if city:
            publishes = publishes.filter(shop__dianping_city__contains=city)
        # Logs.print_log("get_publish_info again count", publishes.count())
        return Discovery.get_the_publishes_info(
            publishes, user_id, start_id, count, longitude, latitude, loaded_publish_ids)
