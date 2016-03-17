# -*- coding: utf-8 -*-

from operator import itemgetter


class Operations(object):
    @staticmethod
    def merge_dicts(a, b):
        return dict(list(a.items()) + list(b.items()))

    @staticmethod
    def sort_dict(a, index, reverse):
        """
        :param a: 字典
        :param index: 下标值0，1
        :param reverse: 是否倒序True, False
        :return: 排序的列表
        """
        return sorted(a.iteritems(), key=itemgetter(index), reverse=reverse)

    @staticmethod
    def sort_list_with_dict(a, key, reverse):
        return sorted(a, key=itemgetter(key), reverse=reverse)
