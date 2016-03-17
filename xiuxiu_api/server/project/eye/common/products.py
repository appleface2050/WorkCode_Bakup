# -*- coding: utf-8 -*-

from ..models import Product
from logs import Logs


class Products(object):
    @staticmethod
    def get_all():
        try:
            return Product.objects.all()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Product.objects.none()

    @staticmethod
    def get(product_id):
        try:
            return Product.objects.get(id=product_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Product.objects.none()

    @staticmethod
    def get_by_name(name):
        try:
            return Product.objects.get(name=name)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Product.objects.none()
