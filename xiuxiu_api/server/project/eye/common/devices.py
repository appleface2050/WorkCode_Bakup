# -*- coding: utf-8 -*-

from ..models import Device
from logs import Logs
from objects import Objects

DEVICE_BRAND = [
    {"key": 0, "name": u"三个爸爸"},
]


class Devices(object):
    @staticmethod
    def get_info(device):
        result = Objects.get_object_info(device)
        return result

    @staticmethod
    def get(sequence):
        try:
            return Device.objects.get(sequence=sequence)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_all():
        try:
            return Device.objects.all()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_all_info():
        result = list()
        devices = Devices.get_all()
        for dev in devices:
            result.append(Devices.get_info(dev))
        return result

    @staticmethod
    def get_brand(key):
        for brand in DEVICE_BRAND:
            if int(brand.get("key", -1)) == int(key):
                return brand
        else:
            return None

    @staticmethod
    def set_is_published(device):
        device.is_published = True
        device.save()
