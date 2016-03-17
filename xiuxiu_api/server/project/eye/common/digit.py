# -*- coding:utf-8 -*-

import random
from math import floor


class Digit(object):
    @staticmethod
    def get_random_int(min_int, max_int):
        """
        :param min_int: 最小的整数
        :param max_int: 最大的整数
        :return: 整数value， min_int =< value <= max_int
        """
        return random.randint(min_int, max_int)

    @staticmethod
    def get_floor_float(float_number, factor=100):
        float_number *= factor
        float_number = floor(float_number)
        return float_number / factor

    @staticmethod
    def init_int(value, default=0):
        result = value
        if result:
            result = int(result)
        else:
            result = default
        return result

    @staticmethod
    def init_float(value, default=0):
        result = value
        if result:
            result = float(result)
        else:
            result = default
        return result
