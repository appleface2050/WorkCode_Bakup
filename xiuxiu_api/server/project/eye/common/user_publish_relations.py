# -*- coding: utf-8 -*-

from ..models import UserPublishRelations
from logs import Logs

WIN = {
    "key": 1,
    "name": "赞"
}
LOST = {
    "key": 0,
    "name": "踩"
}


class UserPublishRelationMethods(object):
    @staticmethod
    def get_win_and_lost():
        result = list()
        result.append(WIN)
        result.append(LOST)
        return result

    @staticmethod
    def add(user_id, publish_id, attribute):
        upr = UserPublishRelations(user_id=user_id, publish_id=publish_id, attribute=attribute)
        upr.save()

    @staticmethod
    def exists(user_id, publish_id):
        upr = UserPublishRelations.objects.filter(user_id=user_id, publish_id=publish_id)
        if upr.count() == 0:
            return False
        return True

    @staticmethod
    def set(upr, attribute):
        upr.attribute = attribute
        upr.save()

    @staticmethod
    def get(user_id, publish_id):
        try:
            return UserPublishRelations.objects.get(user_id=user_id, publish_id=publish_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_win_attribute_count_by_publish(publish):
        user_publish_relation = UserPublishRelations.objects.filter(publish_id=publish.id)
        return user_publish_relation.filter(attribute=WIN.get("key")).count()

    @staticmethod
    def get_lost_attribute_count_by_publish(publish):
        user_publish_relation = UserPublishRelations.objects.filter(publish_id=publish.id)
        return user_publish_relation.filter(attribute=LOST.get("key")).count()

    @staticmethod
    def get_attribute_by_user_and_publish(user_id, publish_id):
        try:
            upr = UserPublishRelations.objects.get(user_id=user_id, publish_id=publish_id)
            return upr.attribute
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return -1
