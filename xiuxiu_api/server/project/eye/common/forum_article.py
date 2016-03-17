# -*- coding: utf-8 -*-

from ..models import ForumArticle
from objects import Objects
from logs import Logs


class ForumArticleMethod(object):
    @staticmethod
    def get_none():
        return Objects.get_none(ForumArticle)

    @staticmethod
    def get_all():
        return ForumArticle.objects.all()

    @staticmethod
    def get(article_id):
        try:
            fa = ForumArticle.objects.filter(id=article_id)
            if fa.count() == 1:
                return fa[0]
            else:
                return ForumArticleMethod.get_none()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return ForumArticleMethod.get_none()

    @staticmethod
    def get_info(article):
        return Objects.get_object_info(article)

    @staticmethod
    def get_recommended_articles(label_id, count):
        try:
            articles = ForumArticleMethod.get_all()
            if label_id:
                articles = articles.filter(label_id=label_id)
            articles = articles.order_by("-weight", "-id")[:count]
            return articles
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return ForumArticleMethod.get_none()

    @staticmethod
    def get_labels(is_preview=False):
        if is_preview:
            return ForumArticle.objects.all().distinct().values("forum_label_id").order_by("forum_label_id")
        else:
            return ForumArticle.objects.all().exclude(
                forum_label_id=1).distinct().values("forum_label_id").order_by("forum_label_id")
