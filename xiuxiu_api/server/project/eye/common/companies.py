# -*- coding: utf-8 -*-

from ..models import Company
from logs import Logs
from objects import Objects


class Companies(object):
    @staticmethod
    def get_none():
        return Objects.get_none(Company)

    @staticmethod
    def get(name):
        try:
            # Logs.print_log("name", name)
            return Company.objects.get(name=name)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def add(company_dict):
        pass
