# -*- coding: utf-8 -*-

from ..models import ShopCategory
from logs import Logs
from objects import Objects

DEFAULT_CATEGORY_NAME = "其他"


class ShopCategories(object):
    @staticmethod
    def get_big_categories():
        category_list = list()
        categories = ShopCategory.objects.filter(parent_id=1)
        for c in categories:
            temp = Objects.get_object_info(c)
            category_list.append(temp)

        return category_list

    @staticmethod
    def get_category_id(name):
        try:
            category = ShopCategory.objects.get(name=name)
            return category.id
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0

    @staticmethod
    def get_category(category_id):
        try:
            return ShopCategory.objects.get(id=category_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_category_name(category_name):
        try:
            return ShopCategory.objects.get(name=category_name)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_category_default():
        try:
            return ShopCategory.objects.get(name=DEFAULT_CATEGORY_NAME)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_shop_big_category(category):
        if category.id == 1:
            return None
        elif category.parent_id == 1:
            return category
        else:
            parent = ShopCategories.get_category(category.parent_id)
            if parent:
                return ShopCategories.get_shop_big_category(parent)
            else:
                return None

    @staticmethod
    def is_home_or_company(category):
        if category.name == u"家" or category.name == u"公司":
            return True
        else:
            return False

    @staticmethod
    def is_weighted(shop):
        if shop.weight > 0:
            return True
        else:
            return False
