# -*- coding: utf-8 -*-

from ..models import Publish
from ..models import UserPublishRelations
from django.db.models import Count
from logs import Logs
from objects import Objects
from datetimes import Datetimes
import files
from images import Images
from shop_categories import ShopCategories
from publish_categories import PublishCategories
from percentages import Percentages
from users import Users


class Publishes(object):
    @staticmethod
    def update_attribute(publish, name, value):
        Objects.set_value(publish, name, value)

    @staticmethod
    def get_attribute(publish, name):
        return Objects.get_value(publish, name)

    @staticmethod
    def add(publish_dict):
        try:
            shop_id = publish_dict.get("shop_id", 0)
            if shop_id:
                publish = Publish(shop_id=shop_id)
                return Publishes.update(publish, publish_dict)
            else:
                return None
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get(publish_id):
        try:
            return Publish.objects.get(id=publish_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def update(publish, publish_dict):
        try:
            for (key, value) in publish_dict.items():
                Objects.set_value(publish, key, value)
            publish.save()
            return publish
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_info(publish):
        result = dict()
        result = Objects.get_object_info(publish)
        return result

    @staticmethod
    def get_shown_publishes(user_id=0, time_limit=True):
        # 用户自己的所有发布 + 用户存在返回大众点评所有的已经通过的和未审核的发布 + 已经审核通过的非大众点评的发布
        # 用户不存在（默认）返回大众点评所有的已经通过的和未审核的发布 + 已经审核通过的非大众点评的发布

        month_ago = Datetimes.get_some_day(30, 0)
        month_ago = Datetimes.naive_to_aware(month_ago)

        if time_limit:
            if user_id > 0:
                # 自己发布的数据，在哪儿都能看到（如果审核没有通过，别人看不到）
                return Publish.objects.filter(user_id=user_id) | \
                       Publish.objects.exclude(audit=0).exclude(shop__dianping_business_id=0) | \
                       Publish.objects.filter(shop__dianping_business_id=0, audit=1).exclude(user_id=user_id)
            elif user_id == 0:
                return Publish.objects.filter(
                    created_at__gte=month_ago).exclude(audit=0).exclude(shop__dianping_business_id=0) | \
                       Publish.objects.filter(shop__dianping_business_id=0, audit=1, created_at__gte=month_ago)
            else:
                return Publish.objects.exclude(audit=0, created_at__gte=month_ago)
        else:
            if user_id > 0:
                # 自己发布的数据，在哪儿都能看到（如果审核没有通过，别人看不到）
                return Publish.objects.filter(user_id=user_id) | \
                       Publish.objects.exclude(audit=0).exclude(shop__dianping_business_id=0) | \
                       Publish.objects.filter(shop__dianping_business_id=0, audit=1).exclude(user_id=user_id)
            elif user_id == 0:
                return Publish.objects.exclude(audit=0).exclude(shop__dianping_business_id=0) | \
                       Publish.objects.filter(shop__dianping_business_id=0, audit=1)
            else:
                return Publish.objects.exclude(audit=0)

        # if user_id > 0:
        #     return Publish.objects.filter(
        #         created_at__gte=month_ago
        #     ).exclude(audit=0).exclude(
        #         shop__dianping_business_id=0
        #     ) | Publish.objects.filter(
        #         shop__dianping_business_id=0,
        #         user_id=user_id,
        #         created_at__gte=month_ago
        #     ) | Publish.objects.filter(
        #         shop__dianping_business_id=0,
        #         audit=1,
        #         created_at__gte=month_ago
        #     ).exclude(user_id=user_id)
        # elif user_id == 0:
        #     return Publish.objects.filter(
        #         created_at__gte=month_ago
        #     ).exclude(
        #         audit=0
        #     ).exclude(shop__dianping_business_id=0) | Publish.objects.filter(
        #         shop__dianping_business_id=0,
        #         audit=1,
        #         created_at__gte=month_ago)
        # else:
        #     return Publish.objects.exclude(
        #         audit=0,
        #         created_at__gte=month_ago)

    @staticmethod
    def get_publishes_by_datetime(start, end):
        try:
            start = Datetimes.naive_to_aware(start)
            end = Datetimes.naive_to_aware(end)
            publishes = Publish.objects.filter(
                created_at__gte=start,
                created_at__lte=end).order_by("-id")
            return publishes
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_publishes_by_user_sometime(user_id, start, end):
        try:
            start = Datetimes.naive_to_aware(start)
            end = Datetimes.naive_to_aware(end)
            return Publish.objects.filter(
                user_id=user_id,
                created_at__gte=start,
                created_at__lte=end
            ).order_by("-id", "-user_id", "-audit")
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_publishes_by_device(device_id):
        try:
            publishes = Publish.objects.filter(device_id=device_id)
            return publishes
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def is_somebody_published_someday(user_id, start, end):
        start = Datetimes.naive_to_aware(start)
        end = Datetimes.naive_to_aware(end)
        publishes = Publish.objects.filter(
            user_id=user_id,
            created_at__gte=start,
            created_at__lte=end)
        if publishes.count() > 0:
            return True
        else:
            return False

    @staticmethod
    def get_publishes_by_shop(shop_id):
        year_ago = Datetimes.get_some_day(365, 0)
        year_ago = Datetimes.naive_to_aware(year_ago)
        return Publish.objects.filter(shop_id=shop_id, created_at__gte=year_ago).exclude(audit=0)

    @staticmethod
    def get_publishes_by_user(user_id):
        year_ago = Datetimes.get_some_day(365, 0)
        year_ago = Datetimes.naive_to_aware(year_ago)
        return Publish.objects.filter(user_id=user_id, created_at__gte=year_ago)

    @staticmethod
    def get_average_publish_pm25(shop, user_id):
        publishes = Publishes.get_publishes_by_shop(shop.id)
        score = 0
        count = 0
        for publish in publishes:
            if publish.PM2_5 >= 0:
                score += publish.PM2_5
                count += 1
        if count == 0:
            name = "pm25"
            percentage_node_info = Percentages.get_some_node_info(Percentages.PERCENTAGES_PATH, name)
            default_score = percentage_node_info.get("score")
            return default_score
        else:
            return 1.0 * int(score) / count

    @staticmethod
    def get_publish_category_score(publish):
        score = -1000

        # 获取场所的分类
        category = ShopCategories.get_category(publish.shop.category.id)
        if category is not None:
            # 获取场所分类下发布的全部分类
            publish_categories = PublishCategories.get_category_by_shop_category(PublishCategories.CATEGORIES_PATH,
                                                                                 category.name)
            for pc in publish_categories:
                name = pc.get("name", None)
                if name is not None:
                    # 根据发布的分类名称（配置文件中的名称）获取数据库中字段的名称
                    db_name = PublishCategories.get_db_category_name(category.name)
                    if db_name is not None:
                        # 根据数据库中字段的值，取得发布中分类的成绩
                        value = Publishes.get_attribute(publish, db_name)
                        answers = PublishCategories.get_answers(pc)
                        for answer in answers:
                            if int(answer.get("id", -1000)) == int(value):
                                # 如果score是初始值，则把它置0，使它变得有效
                                if score == -1000:
                                    score = 0
                                score += int(answer.get("score", 0))
                                break
        if score == -1000:
            name = "category"
            percentage_node_info = Percentages.get_some_node_info(Percentages.PERCENTAGES_PATH, name)
            default_score = int(percentage_node_info.get("default"))
            score = default_score
        return score

    @staticmethod
    def get_average_publish_category_score(shop, user_id):
        publishes = Publishes.get_publishes_by_shop(shop.id)
        score = 0
        count = 0

        for publish in publishes:
            publish_score = Publishes.get_publish_category_score(publish)
            score += publish_score
            count += 1

        if count == 0:
            name = "category"
            percentage_node_info = Percentages.get_some_node_info(Percentages.PERCENTAGES_PATH, name)
            default_score = int(percentage_node_info.get("default"))
            return default_score

        return 1.0 * int(score) / count

    @staticmethod
    def get_publish_images(publish):
        result = dict()
        if publish.big_image:
            name_parts = publish.big_image.name.split(".")
            path_parts = publish.big_image.name.split("/")
            mid_path = "/".join(path_parts[:-1])
            big_name = ".".join(name_parts[:-1]) + "_big." + name_parts[-1]
            medium_name = ".".join(name_parts[:-1]) + "_medium." + name_parts[-1]
            small_name = ".".join(name_parts[:-1]) + "_small." + name_parts[-1]
            result["publish_image_url"] = files.BASE_URL_4_IMAGE + publish.big_image.name
            result["publish_image_big_url"] = files.BASE_URL_4_IMAGE + big_name
            result["publish_image_medium_url"] = files.BASE_URL_4_IMAGE + medium_name
            result["publish_image_small_url"] = files.BASE_URL_4_IMAGE + small_name

            img_path = result["publish_image_url"]

            memory_file = files.Files.get_memory_file(img_path)
            new_path = "media_root/" + mid_path + "/"

            new_big_path = new_path + big_name
            new_big_width = 1280
            if not files.Files.exists(new_big_path):
                Images.resize_image(memory_file, new_big_path, new_big_width)

            new_medium_path = new_path + medium_name
            new_medium_width = 640
            if not files.Files.exists(new_medium_path):
                Images.resize_image(memory_file, new_medium_path, new_medium_width)

            new_small_path = new_path + small_name
            new_small_width = 120
            if not files.Files.exists(new_small_path):
                Images.resize_image(memory_file, new_small_path, new_small_width)
        else:
            result["publish_image_url"] = ""
            result["publish_image_medium_url"] = ""
            result["publish_image_small_url"] = ""
        return result

    @staticmethod
    def get_unread_win_count(user_id):
        result = 0
        publishes = Publishes.get_publishes_by_user(user_id)
        for p in publishes:
            result += p.win_count
        return result

    @staticmethod
    def get_unread_lost_count(user_id):
        result = 0
        publishes = Publishes.get_publishes_by_user(user_id)
        for p in publishes:
            result += p.lost_count
        return result

    @staticmethod
    def add_win_count(publish):
        publish.win_count += 1
        publish.save()

    @staticmethod
    def minus_win_count(publish):
        publish.win_count -= 1
        publish.save()

    @staticmethod
    def add_lost_count(publish):
        publish.lost_count += 1
        publish.save()

    @staticmethod
    def minus_lost_count(publish):
        publish.lost_count -= 1
        publish.save()

    @staticmethod
    def reset_win_and_lost_count(publish):
        publish.win_count = 0
        publish.lost_count = 0
        publish.save()

    @staticmethod
    def get_user_count():
        return Publishes.get_shown_publishes(0).values("user_id").distinct().count()

    @staticmethod
    def get_has_pm2_5_user_count():
        return Publishes.get_shown_publishes(0).exclude(PM2_5=-1).values("user_id").distinct().count()

    @staticmethod
    def get_user_and_publish_count():
        return Publishes.get_shown_publishes(0).values("user_id").distinct().annotate(count=Count("user_id"))

    @staticmethod
    def get_publish_info4hot(publish, longitude, latitude, user_id):
        result = dict()

        result["created_at"] = Datetimes.transfer_datetime(publish.created_at)
        if publish.user:
            if publish.user.userextension:
                result["user_nickname"] = publish.user.userextension.nickname
            else:
                result["user_nickname"] = ""
        else:
            result["user_nickname"] = ""

        temp_image = Users.get_user_image(publish.user)
        result["user_small_image"] = temp_image["small_user_image"]
        result["PM2_5"] = publish.PM2_5

        result["content"] = publish.content

        category = publish.shop.category
        big_category = ShopCategories.get_shop_big_category(category)

        if not big_category:
            big_category = ShopCategories.get_category_default()

        if big_category:
            result["big_category_name"] = big_category.name
            result["big_category_key"] = big_category.id
        else:
            result["big_category_name"] = ""
            result["big_category_key"] = ""

        if not big_category:
            result["category_operation"] = {}
        else:
            result["category_operation"] = PublishCategories.get_category_info_by_shop_category(big_category)

        return result

    @staticmethod
    def get_none():
        return Objects.get_none(Publish)

    @staticmethod
    def get_latest_publish(shop_id):
        try:
            return Publish.objects.filter(shop_id=shop_id).order_by("-id")[0]
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Publishes.get_none()
