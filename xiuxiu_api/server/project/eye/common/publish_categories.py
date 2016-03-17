# -*- coding: utf-8 -*-
from xmls import Xmls
from shop_categories import ShopCategories


class PublishCategories(object):
    CATEGORIES_PATH = "eye/configurations/categories.xml"
    FIELDS = [
        {
            "name": "tableware",
            "db_name": "tableware",
        },
        {
            "name": "environment",
            "db_name": "floor",
        },
        {
            "name": "child",
            "db_name": "child",
        },
        {
            "name": "smell",
            "db_name": "smell",
        },
        {
            "name": "equipment",
            "db_name": "equipment_new",
        },
        {
            "name": "water",
            "db_name": "water",
        },
        {
            "name": "soundproof",
            "db_name": "soundproof",
        },
        {
            "name": "quiet",
            "db_name": "quiet",
        },
        {
            "name": "pet",
            "db_name": "pet",
        },
        {
            "name": "watchman",
            "db_name": "watchman",
        },
        {
            "name": "monitor",
            "db_name": "has_monitor",
        },
    ]

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Xmls.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get_categories(file_path):
        root = PublishCategories.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "category")

    @staticmethod
    def get_category_percentage(file_path):
        root = PublishCategories.get_root(file_path)
        return Xmls.get_node_attributes(root).get("percentage", 0)

    @staticmethod
    def get_category_name(category):
        return Xmls.get_node_attributes(category).get("name", None)

    @staticmethod
    def get_shop_categories(category):
        return Xmls.get_all_path_nodes(category, "shop_categories/shop_category")

    @staticmethod
    def get_shop_category_percentage(shop_category):
        return Xmls.get_node_attributes(shop_category).get("percentage", 0)

    @staticmethod
    def get_shop_category_info(shop_category):
        result = dict()
        result["name"] = shop_category.text
        result["id"] = ShopCategories.get_category_id(shop_category.text)
        result["percentage"] = PublishCategories.get_shop_category_percentage(shop_category)
        return result

    @staticmethod
    def get_shop_category_infos(shop_categories):
        result = list()
        for shop_category in shop_categories:
            result.append(PublishCategories.get_shop_category_info(shop_category))
        return result

    @staticmethod
    def get_question(category):
        return Xmls.get_all_path_nodes(category, "question/description")

    @staticmethod
    def get_question_info(question):
        return question.text

    @staticmethod
    def get_answers(category):
        return Xmls.get_all_path_nodes(category, "answer")

    @staticmethod
    def get_answer_id(answer):
        return Xmls.get_node_attributes(answer).get("id", 0)

    @staticmethod
    def get_answer_choice(answer):
        return Xmls.get_the_first_node(answer, "choice")

    @staticmethod
    def get_answer_description(answer):
        return Xmls.get_the_first_node(answer, "description")

    @staticmethod
    def get_answer_score(answer):
        return Xmls.get_the_first_node(answer, "score")

    @staticmethod
    def get_answer_info(answer):
        result = dict()
        result["id"] = int(PublishCategories.get_answer_id(answer))
        result["choice"] = PublishCategories.get_answer_choice(answer).text
        result["description"] = PublishCategories.get_answer_description(answer).text
        result["score"] = int(PublishCategories.get_answer_score(answer).text)
        return result

    @staticmethod
    def get_answer_infos(answers):
        result = list()
        for answer in answers:
            result.append(PublishCategories.get_answer_info(answer))
        return result

    @staticmethod
    def get_category_info(category):
        result = dict()
        result["name"] = PublishCategories.get_category_name(category)
        shop_categories = PublishCategories.get_shop_categories(category)
        result["shop_categories"] = PublishCategories.get_shop_category_infos(shop_categories)
        question = PublishCategories.get_question(category)
        if question:
            result["question"] = PublishCategories.get_question_info(question[0])
        else:
            result["question"] = None
        answers = PublishCategories.get_answers(category)
        result["answers"] = PublishCategories.get_answer_infos(answers)
        return result

    @staticmethod
    def get_category_infos(categories):
        result = list()
        for category in categories:
            result.append(PublishCategories.get_category_info(category))
        return result

    @staticmethod
    def get_all(file_path=CATEGORIES_PATH):
        result = dict()
        # result["percentage"] = PublishCategories.get_category_percentage(file_path)
        categories = PublishCategories.get_categories(file_path)
        result["categories"] = PublishCategories.get_category_infos(categories)
        return result

    @staticmethod
    def get_category_by_shop_category(shop_category, file_path=CATEGORIES_PATH):
        the_categories = list()
        categories = PublishCategories.get_categories(file_path)
        for category in categories:
            shop_categories = PublishCategories.get_shop_categories(category)
            for sc in shop_categories:
                sci = PublishCategories.get_shop_category_info(sc)
                if sci["name"] == shop_category:
                    the_categories.append(category)
                    break
        return the_categories

    @staticmethod
    def get_category_info_by_shop_category(shop_category, file_path=CATEGORIES_PATH):
        result = list()
        the_categories = PublishCategories.get_category_by_shop_category(shop_category, file_path)
        for category in the_categories:
            result.append(PublishCategories.get_category_info(category))
        return result

    @staticmethod
    def get_db_category_name(name):
        for item in PublishCategories.FIELDS:
            category_name = item.get("name", None)
            if category_name == name:
                return item.get("db_name", None)
        return None
