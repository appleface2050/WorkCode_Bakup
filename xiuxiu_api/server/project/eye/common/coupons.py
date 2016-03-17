# -*- coding: utf-8 -*-
import uuid
from ..models import Coupon
from datetimes import Datetimes
from objects import Objects
from logs import Logs
from users import Users
from messages import Messages


class Coupons(object):
    @staticmethod
    def get_none():
        return Objects.get_none(Coupon)

    PRODUCT = [
        {"key": 1, "name": u"全场", "descriptions": ""},
        {"key": 2, "name": u"守护天使滤芯", "descriptions": ""},
        {"key": 3, "name": u"守护天使", "descriptions": u"减600"},
        {"key": 4, "name": u"高达卫士", "descriptions": u"减1000"},
        {"key": 5, "name": u"伊娃宝贝", "descriptions": u"减100"},
        {"key": 6, "name": u"三个爸爸随身pm2.5检测器", "descriptions": u"减60"},
        {"key": 7, "name": u"嗅嗅口罩", "descriptions": u"减50"},
        {"key": 8, "name": u"成人口罩", "descriptions": ""},
    ]

    @staticmethod
    def generate_uuid():
        return uuid.uuid1()

    @staticmethod
    def get_coupon(sequence):
        try:
            return Coupon.objects.get(sequence=sequence)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Coupons.get_none()

    @staticmethod
    def get_youzan_coupon(youzan_sequence):
        try:
            coupons = Coupon.objects.filter(youzan_sequence=youzan_sequence, is_valid=True)
            for c in coupons:
                if Coupons.is_out_of_coupon_date(c):
                    continue
                return c
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Coupons.get_none()

    @staticmethod
    def is_out_of_date(coupon):
        """
        :param coupon:
        :return: 根据激活日期和有效天数计算是否过期
        """
        now = Datetimes.get_now()
        utc_now = Datetimes.transfer_datetime(now, is_utc=False)
        the_days = Datetimes.get_delta_time(coupon.activated_at, utc_now).days
        # 是否过期
        if the_days < coupon.valid_days:
            return False
        else:
            return True

    @staticmethod
    def is_shown_out_of_date(coupon, days):
        """
        :param coupon:
        :param days: （有效期天数 + 过期几天）
        :return: 是否显示优惠券
        """
        now = Datetimes.get_now()
        utc_now = Datetimes.transfer_datetime(now, is_utc=False)
        the_days = Datetimes.get_delta_time(coupon.activated_at, utc_now).days
        # 是否过期
        if the_days < days:
            return False
        else:
            return True

    @staticmethod
    def get_coupon_info(tc):
        temp = Objects.get_object_info(tc)
        temp["is_out_of_date"] = Coupons.is_out_of_date(tc)
        return temp

    @staticmethod
    def is_coupon_activated(coupon):
        """
        :param coupon:
        :return: 是否被激活了（有激活时间和激活用户）
        """
        if coupon.activated_at and coupon.user_id:
            return True
        else:
            return False

    @staticmethod
    def is_coupon_valid(coupon):
        if coupon.is_valid:
            # 注册的需要验证有效期
            if Coupons.is_coupon_activated(coupon) and Coupons.is_out_of_coupon_date(coupon):
                return False
            # 未注册的和激活未过期的有效
            else:
                return True
        else:
            return False

    @staticmethod
    def get_valid_coupons():
        result = list()
        # 未消费的优惠券
        the_coupons = Coupon.objects.filter(is_valid=True)

        # 未过期的优惠券
        for tc in the_coupons:
            if Coupons.is_coupon_activated(tc) and Coupons.is_out_of_coupon_date(tc):
                continue
            temp = Coupons.get_coupon_info(tc)
            result.append(temp)
        return result

    @staticmethod
    def get_user_coupons(user_id, shown_days=7):
        # 先放未过期的,未过期的按激活时间排序， 后放已经过期的
        result = list()
        # out_of_date_result = list()
        # 未消费的优惠券
        the_coupons = Coupon.objects.filter(user_id=user_id).order_by("activated_at")

        # 未过期的优惠券
        for tc in the_coupons:
            if not Coupons.is_out_of_coupon_date(tc):
                temp = Coupons.get_coupon_info(tc)
                result.append(temp)
        return result

    @staticmethod
    def is_out_of_coupon_date(coupon):
        now = Datetimes.get_now()
        start_datetime = coupon.valid_start
        end_datetime = coupon.valid_end

        positive_days = Datetimes.get_delta_time(start_datetime, now)
        negative_days = Datetimes.get_delta_time(now, end_datetime)

        if positive_days.days >= 0 and positive_days.seconds >= 0 \
                and negative_days.days >= 0 and negative_days.seconds >= 0:
            return False
        else:
            return True

    @staticmethod
    def is_it_out(coupon):
        if coupon.is_out:
            return True
        else:
            return False

    @staticmethod
    def set_it_out(coupon):
        coupon.is_out = True
        coupon.save()
        return coupon

    @staticmethod
    def get_them(coupon_dict):
        try:
            is_out = coupon_dict.get("is_out", False)
            product = coupon_dict.get("product", 0)
            valid_start = coupon_dict.get("valid_start", None)
            valid_end = coupon_dict.get("valid_end", None)
            descriptions = coupon_dict.get("descriptions", None)
            coupon_value = coupon_dict.get("coupon_value", None)
            channel_id = coupon_dict.get("channel_id", 0)
            use_id = coupon_dict.get("use_id", 0)

            the_coupons = Coupon.objects.filter(is_out=is_out)
            if product:
                the_coupons = the_coupons.filter(product=product)
            if valid_start:
                the_coupons = the_coupons.filter(valid_start__lte=valid_start)
            if valid_end:
                the_coupons = the_coupons.filter(valid_end__gte=valid_end)
            if descriptions:
                the_coupons = the_coupons.filter(descriptions=descriptions)
            if channel_id:
                the_coupons = the_coupons.filter(channel_id=channel_id)
            if use_id:
                the_coupons = the_coupons.filter(use_id=use_id)
            if coupon_value:
                the_coupons = the_coupons.filter(coupon_value=coupon_value)

            return the_coupons
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_one_coupon(coupon_dict):
        them = Coupons.get_them(coupon_dict)
        if them:
            for i in xrange(them.count()):
                one = them[i]
                if one.is_out:
                    continue
                else:
                    Coupons.set_it_out(one)
                    return one
        else:
            return None

    @staticmethod
    def get_coupon_package(channel_id=1):
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
        for i in xrange(len(product_list)):
            coupon_dict["descriptions"] = descriptions_list[i]
            coupon_dict["product"] = product_list[i]
            coupon_dict["channel_id"] = channel_id
            coupon_object = Coupons.get_one_coupon(coupon_dict)
            if coupon_object:
                result.append(coupon_object)

        return result

    @staticmethod
    def get_some_coupon_package(product_list, coupon_value_list, channel_id_list):
        result = list()
        coupon_dict = dict()
        for i in xrange(len(product_list)):
            coupon_object = Coupons.get_some_coupon(
                coupon_value_list[i],
                product_list[i],
                channel_id_list[i]
            )
            if coupon_object:
                result.append(coupon_object)

        return result

    @staticmethod
    def get_mask_coupon():
        coupon_dict = dict()
        coupon_dict["descriptions"] = Coupons.PRODUCT[6]["descriptions"]
        coupon_dict["product_id"] = Coupons.PRODUCT[6]["key"]
        coupon_dict["channel_id"] = 1
        return Coupons.get_one_coupon(coupon_dict)

    @staticmethod
    def get_some_coupon(coupon_value, product_id, channel_id):
        coupon_dict = dict()
        coupon_dict["coupon_value"] = coupon_value
        coupon_dict["product_id"] = product_id
        coupon_dict["channel_id"] = channel_id
        return Coupons.get_one_coupon(coupon_dict)

    @staticmethod
    def activate_coupon_action(user, coupons):
        try:
            for coupon in coupons:
                coupon.user_id = user.id
                coupon.activated_at = Datetimes.get_now()
                coupon.save()
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False
