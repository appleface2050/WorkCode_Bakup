# -*- coding: utf-8 -*-

from datetimes import Datetimes
from images import Images


class Objects(object):
    @staticmethod
    def get_none(model):
        return model.objects.none()

    @staticmethod
    def get_type_name(data):
        return type(data).__name__

    @staticmethod
    def get_object_info(obj):
        obj_dict = obj.__dict__
        result = dict()
        for (key, value) in obj_dict.items():
            if not key.startswith("_"):
                if Objects.get_type_name(value) == "datetime":
                    result[key] = Datetimes.clock_to_string(Datetimes.utc2east8(value))
                elif Objects.get_type_name(value) \
                        == "ImageFieldFile":
                    result[key] = Images.get_url(value.name)
                # elif Objects.get_type_name(value) == "ManyRelatedManager":
                #     temp = dict()
                #     result[key] = temp
                else:
                    result[key] = value
        return result

    @staticmethod
    def set_value(obj, name, value):
        return setattr(obj, name, value)

    @staticmethod
    def get_value(obj, name):
        return getattr(obj, name)
