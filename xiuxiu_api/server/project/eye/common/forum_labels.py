# -*- coding: utf-8 -*-

from ..models import ForumLabel
from objects import Objects
from logs import Logs


class ForumLabels(object):
    @staticmethod
    def get_none():
        return Objects.get_none(ForumLabel)

    @staticmethod
    def get_all():
        return ForumLabel.objects.all()

    @staticmethod
    def get_info(label):
        return Objects.get_object_info(label)

    @staticmethod
    def get_name(label_id):
        try:
            return ForumLabel.objects.get(id=label_id).name
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return ""
