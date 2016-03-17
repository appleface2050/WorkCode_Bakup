# -*- coding: utf-8 -*-

from ..models import UserShopRelations
from logs import Logs


class UserShopRelationMethods(object):
    RECOMMENDED = 1
    NOT_RECOMMENDED = 0

    @staticmethod
    def add(user_id, shop_id, is_recommended):
        usr = UserShopRelations(user_id=user_id, shop_id=shop_id, is_recommended=is_recommended)
        usr.save()

    @staticmethod
    def update(usr, is_recommended):
        usr.is_recommended = is_recommended
        usr.save()

    @staticmethod
    def get_by_shop(shop):
        try:
            return UserShopRelations.objects.filter(shop_id=shop.id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return UserShopRelations.objects.none()

    @staticmethod
    def get_by_shop_and_user(shop_id, user_id):
        try:
            return UserShopRelations.objects.get(shop_id=shop_id, user_id=user_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return UserShopRelations.objects.none()

    @staticmethod
    def get_recommended_count_by_shop(shop):
        try:
            return UserShopRelationMethods.get_by_shop(shop).filter(
                is_recommended=UserShopRelationMethods.RECOMMENDED).count()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0

    @staticmethod
    def get_not_recommended_count_by_shop(shop):
        try:
            return UserShopRelationMethods.get_by_shop(shop).filter(
                is_recommended=UserShopRelationMethods.NOT_RECOMMENDED).count()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0
