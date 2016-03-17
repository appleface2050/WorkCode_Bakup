# -*- coding: utf-8 -*-

from ..models import Channel
from logs import Logs
from objects import Objects


class Channels(object):
    @staticmethod
    def get_none():
        return Objects.get_none(Channel)

    @staticmethod
    def get_all():
        try:
            return Channel.objects.filter(audit=1)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Channels.get_none()

    @staticmethod
    def get(channel_id):
        try:
            return Channel.objects.get(id=channel_id, audit=1)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Channel.objects.none()

    @staticmethod
    def get_by_name(channel_name):
        try:
            return Channel.objects.get(name=channel_name, audit=1)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Channel.objects.none()
