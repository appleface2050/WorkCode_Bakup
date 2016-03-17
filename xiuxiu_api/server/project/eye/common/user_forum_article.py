# -*- coding: utf-8 -*-

from ..models import UserForumArticle
from logs import Logs
from objects import Objects
import operator


class UserForumArticleMethod(object):
    STATUS_CREATE = 0
    STATUS_WIN = 1
    STATUS_COLLECT = 2
    STATUS_BROWSE = 3

    @staticmethod
    def get_none():
        return Objects.get_none(UserForumArticle)

    @staticmethod
    def get(user_id, article_id, status):
        """
        :param user_id: 用户ID
        :param article_id: 文章ID
        :param status: 状态 0-创建， 1-赞， 2-收藏， 3-浏览
        :return:
        """
        try:
            ufa = UserForumArticle.objects.filter(user_id=user_id, article_id=article_id, status=status)
            if ufa.count() == 1:
                return ufa[0]
            else:
                return UserForumArticleMethod.get_none()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return UserForumArticleMethod.get_none()

    @staticmethod
    def add(user_id, article_id, status):
        try:
            ufa = UserForumArticle(user_id=user_id, article_id=article_id, status=status)
            ufa.save()
            return ufa
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return UserForumArticleMethod.get_none()

    @staticmethod
    def update(user_forum_article, increment=1):
        try:
            user_forum_article.count += increment
            user_forum_article.save()
            return user_forum_article
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return UserForumArticleMethod.get_none()

    @staticmethod
    def get_count_by_article(article_id, status):
        try:
            ufs = UserForumArticle.objects.filter(article_id=article_id, status=status)
            counts = [o.count for o in ufs]
            return reduce(operator.add, counts, 0)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0

    @staticmethod
    def get_count_by_user(user_id, status):
        try:
            ufs = UserForumArticle.objects.filter(user_id=user_id, status=status)
            counts = [o.count for o in ufs]
            return reduce(operator.add, counts, 0)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0

    @staticmethod
    def get_articles(user_id, status):
        try:
            ufs = UserForumArticle.objects.filter(user_id=user_id, status=status, count__gt=0)
            return ufs
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return UserForumArticleMethod.get_none()
