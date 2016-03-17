# coding:utf-8

from ..models import PopWindow
from objects import Objects


class PopWindows(object):
    @staticmethod
    def get_all():
        return PopWindow.objects.all()

    @staticmethod
    def get_info(pw):
        return Objects.get_object_info(pw)
