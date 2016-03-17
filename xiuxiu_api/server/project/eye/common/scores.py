# -*- coding: utf-8 -*-

from publishes import Publishes
from pm25s import Pm25s
from formaldehydes import Formaldehydes
from co2s import Co2s
from tvocs import Tvocs
from percentages import Percentages
from levels import Levels


class Scores(object):
    @staticmethod
    def get_average_pm2_5(shop, user_id):
        publishes = Publishes.get_publishes_by_shop(shop.id)
        count = 0
        score = 0
        for p in publishes:
            if int(p.PM2_5) >= 0:
                score += p.PM2_5
                count += 1
        if count <= 0:
            return -1000
        else:
            return float(score) / count

    @staticmethod
    def get_pm25_node_info(value):
        return Pm25s.get_some_node_info(Pm25s.PM25S_PATH, value)

    @staticmethod
    def get_formaldehyde_node_info(value):
        return Formaldehydes.get_some_node_info(Formaldehydes.FORMALDEHYDES_PATH, value)

    @staticmethod
    def get_co2_node_info(value):
        return Co2s.get_some_node_info(Co2s.CO2S_PATH, value)

    @staticmethod
    def get_tvoc_node_info(value):
        return Tvocs.get_some_node_info(Tvocs.TVOCS_PATH, value)

    @staticmethod
    def get_percentage_default(name):
        percentage_node_info = Percentages.get_some_node_info(Percentages.PERCENTAGES_PATH, name)
        return percentage_node_info.get("default")

    @staticmethod
    def get_total_score(shop, user_id):
        result = dict()
        average_pm2_5 = Publishes.get_average_publish_pm25(shop, user_id)
        pm25_node_info = Scores.get_pm25_node_info(average_pm2_5)
        result["PM2_5_NODE"] = pm25_node_info
        result["PM2_5"] = pm25_node_info.get("score")

        formaldehyde = Scores.get_formaldehyde_node_info(shop.formaldehyde)
        if formaldehyde >= 0:
            formaldehyde_node_info = Scores.get_formaldehyde_node_info(formaldehyde)
            result["FORMALDEHYDE_OBJECT"] = formaldehyde_node_info
            result["FORMALDEHYDE"] = formaldehyde_node_info.get("score")
        else:
            result["FORMALDEHYDE_OBJECT"] = None
            name = "formaldehyde"
            result["FORMALDEHYDE"] = Scores.get_percentage_default(name)

        co2 = Scores.get_co2_node_info(shop.CO2)
        if co2 >= 0:
            co2_node_info = Scores.get_co2_node_info(co2)
            result["CO2_OBJECT"] = co2_node_info
            result["CO2"] = co2_node_info.get("score")
        else:
            result["CO2_OBJECT"] = None
            name = "co2"
            result["CO2"] = Scores.get_percentage_default(name)

        tvoc = Scores.get_tvoc_node_info(shop.TVOC)
        if tvoc >= 0:
            tvoc_node_info = Scores.get_tvoc_node_info(tvoc)
            result["TVOC_OBJECT"] = tvoc_node_info
            result["TVOC"] = tvoc_node_info.get("score")
        else:
            result["TVOC_OBJECT"] = None
            name = "tvoc"
            result["TVOC"] = Scores.get_percentage_default(name)

        result["CATEGORY"] = Publishes.get_average_publish_category_score(shop, user_id)
        return result

    @staticmethod
    def get_percentage_text(name):
        percentage_node_info = Percentages.get_some_node_info(Percentages.PERCENTAGES_PATH, name)
        return percentage_node_info.get("text")

    @staticmethod
    def get_concrete_score(score_dict):
        score = 0

        name = "pm25"
        text = Scores.get_percentage_text(name)
        pm2_5_score = score_dict.get("PM2_5")
        score += float(pm2_5_score) * int(text)

        name = "formaldehyde"
        text = Scores.get_percentage_text(name)
        formaldehyde_score = score_dict.get("formaldehyde")
        score += float(formaldehyde_score) * int(text)

        name = "co2"
        text = Scores.get_percentage_text(name)
        co2_score = score_dict.get("co2")
        score += float(co2_score) * int(text)

        name = "tvoc"
        text = Scores.get_percentage_text(name)
        co2_score = score_dict.get("tvoc")
        score += float(co2_score) * int(text)

        name = "category"
        text = Scores.get_percentage_text(name)
        category_score = score_dict.get("category")
        score += float(category_score) * int(text)

        return int(round(score / 100))

    @staticmethod
    def get_level_info(score):
        return Levels.get_some_node_info(Levels.LEVELS_PATH, score)
