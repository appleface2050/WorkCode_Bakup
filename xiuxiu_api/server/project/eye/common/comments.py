# -*- coding: utf-8 -*-

from objects import Objects
from ..models import Comment
from django.db.models import Count
from logs import Logs
from methods import Methods
from datetimes import Datetimes


class Comments(object):
    @staticmethod
    def get_none():
        return Comment.objects.none()

    @staticmethod
    def get_info(comment):
        return Objects.get_object_info(comment)

    @staticmethod
    def get_unread_by_publish(publish_id):
        return Comment.objects.filter(publish_id=publish_id, is_read=False)

    @staticmethod
    def get_unread():
        return Comment.objects.filter(is_read=False)

    @staticmethod
    def get_unread_by_user(user_id):
        return Comment.objects.filter(is_read=False, publish__user_id=user_id)

    @staticmethod
    def get_count_group_by_user():
        """
        :return:用户ID，用户发表的评论数量，像[{'user_id':17L, 'count':21},{'user_id':18L, 'count':2}]
        """
        return Comment.objects.values("user_id").annotate(count=Count("user_id"))

    @staticmethod
    def update_unread(comment):
        try:
            comment.is_read = True
            comment.save()
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def bulk_update_unread(comments):
        result = dict()
        info = "the comments failed: "
        try:
            for c in comments:
                temp = Comments.update_unread(c)
                if temp:
                    continue
                else:
                    info += str(c.id) + ","
            result["success"] = True
            result["info"] = info
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result["success"] = False
            result["info"] = ex.message
        return result

    @staticmethod
    def get_by_publish(publish_id):
        try:
            return Comment.objects.filter(publish_id=publish_id).order_by("-id")
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_count_by_publish(publish_id):
        return Comment.objects.filter(
            publish_id=publish_id
        ).count()

    @staticmethod
    def get_information(comments, start_id, count, get_comment_info, loaded_comment_ids):
        result = list()
        try:
            if not start_id:
                result = Methods.get_one_object_info_from_scratch(
                    comments, get_comment_info, count, loaded_comment_ids)
            else:
                result = Methods.get_one_object_info_not_from_scratch(
                    comments, get_comment_info, start_id, count, loaded_comment_ids)

        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

        return result

    @staticmethod
    def is_single_person_by_comment(comment):
        user_ids = comment.values("user_id").distinct()
        if len(user_ids) == 1:
            return True
        else:
            return False
