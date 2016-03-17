# -*- coding: utf-8 -*-

from ..models import ForumCategory
from ..models import ForumPost
from ..models import ForumReply
from users import Users
from objects import Objects
from logs import Logs


class Forums(object):
    @staticmethod
    def get_category_info(category):
        result = Objects.get_object_info(category)
        owners = list()
        for owner in category.owners.all():
            user_extension = Users.get_user_extension(owner.username)
            if user_extension:
                owners.append(Users.get_user_info(user_extension))

        result["owners"] = owners
        return result

    @staticmethod
    def get_all_categories():
        return ForumCategory.objects.all()

    @staticmethod
    def get_valid_categories():
        return ForumCategory.objects.filter(status=True)

    @staticmethod
    def get_post():
        return ForumPost.objects.all()

    @staticmethod
    def get_post_by_user(user_id):
        return ForumPost.objects.filter(owner_id=user_id)

    @staticmethod
    def get_reply_by_user(user_id):
        return ForumReply.objects.filter(owner_id=user_id)

    @staticmethod
    def get_post_win_count_by_user(user_id):
        try:
            user = Users.get_user_by_id(user_id)
            user_extension = Users.get_user_extension(user.username)
            count = 0
            for fp in ForumPost.objects.all():
                if user_extension in fp.win_users.all():
                    count += 1
            return count
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0

    @staticmethod
    def get_users_from_post():
        return ForumPost.objects.values("owner_id").distinct()

    @staticmethod
    def get_users_from_reply():
        return ForumReply.objects.values("owner_id").distinct()

    @staticmethod
    def get_reply_for_post(post_id):
        return ForumReply.objects.filter(post_id=post_id)

    @staticmethod
    def get_forum_category_count(category):
        result = {}
        posts = ForumPost.objects.filter(status=True, category_id=category.id)
        post_count = posts.count()
        result["post_count"] = post_count
        reply_count = 0
        for p in posts:
            replies = ForumReply.objects.filter(status=True, post_id=p.id)
            reply_count += replies.count()

        result["reply_count"] = reply_count
        result["total"] = post_count + reply_count
        return result
