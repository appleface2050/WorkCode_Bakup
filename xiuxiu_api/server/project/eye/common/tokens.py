# -*- coding: utf-8 -*-

from rest_framework.authtoken.models import Token
from logs import Logs


class Tokens(object):
    @staticmethod
    def get(key):
        try:
            return Token.objects.get(key=key)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Token.objects.none()
