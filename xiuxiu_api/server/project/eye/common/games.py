# -*- coding: utf-8 -*-

from ..models import Game
from digit import Digit
from logs import Logs
from objects import Objects
from operations import Operations
from game_day_rewards import GameDayRewards
from users import Users
from datetimes import Datetimes


# 名称及16进制码红色 #FF0000
# 橙色 #FF7F00
# 黄色 #FFFF00
# 绿色 #00FF00
# 蓝色 #0000FF
# 青色 #00FFFF
# 紫色 #8B00FF
class GraphColor(object):
    COLORS = {
        "RED": "#FF0000",
        "ORANGE": "#FF7F00",
        "YELLOW": "#FFFF00",
        "GREEN": "#00FF00",
        "BLUE": "#0000FF",
        "CYAN": "#00FFFF",
        "PURPLE": "#8B00FF"
    }
    KEYS = {
        "RED": u"红",
        "ORANGE": u"橙",
        "YELLOW": u"黄",
        "GREEN": u"绿",
        "BLUE": u"蓝",
        "CYAN": u"靛",
        "PURPLE": u"紫"
    }

    @staticmethod
    def get_sets():
        result = dict()
        correct = list()
        incorrect = list()
        for (key1, value1) in GraphColor.COLORS.items():
            for (key2, value2) in GraphColor.KEYS.items():
                temp = dict()
                temp["color"] = value1
                temp["name"] = value2
                if key1 == key2:
                    correct.append(temp)
                else:
                    incorrect.append(temp)
        result["correct"] = correct
        result["incorrect"] = incorrect

        return result

    @staticmethod
    def get_one(correct, incorrect):
        result = dict()
        questions = list()
        min_int = 0
        correct_max_int = len(correct) - 1
        incorrect_max_int = len(incorrect) - 1
        correct_random = Digit.get_random_int(min_int, correct_max_int)
        incorrect_random = Digit.get_random_int(min_int, incorrect_max_int)
        answer = Digit.get_random_int(0, 1)
        if answer:
            questions.append(incorrect[incorrect_random])
            questions.append(correct[correct_random])
        else:
            questions.append(correct[correct_random])
            questions.append(incorrect[incorrect_random])
        result["questions"] = questions
        result["answer"] = answer
        return result


class Games(object):
    RED_ENVELOPE_TYPE = 3

    @staticmethod
    def set(games_dict):
        try:
            game = Game(
                score=games_dict.get("score"),
                username=games_dict.get("username")
            )
            game.save()
            return game
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_ranking(score, start_datetime, end_datetime):
        all_username_and_score = Games.get_all_username_and_score(start_datetime, end_datetime)
        ranking = 1
        for us in all_username_and_score:
            # Logs.print_log("us score", us["score"])
            if int(us["score"]) >= int(score):
                ranking += 1
            else:
                break
        # 去掉自己
        return ranking - 1

    @staticmethod
    def get_info(game):
        result = Objects.get_object_info(game)
        result["ranking"] = Games.get_ranking(game.score)
        return result

    @staticmethod
    def get(username, start_datetime, end_datetime):
        try:
            start_datetime = Datetimes.string_to_utc(start_datetime)
            end_datetime = Datetimes.string_to_utc(end_datetime)
            return Game.objects.filter(username=username, created_at__gte=start_datetime,
                                       created_at__lte=end_datetime).order_by("id")
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_max_score(username, start_datetime, end_datetime):
        max_score = 0
        try:
            games = Games.get(username, start_datetime, end_datetime)
            # Logs.print_log("games count in get_max_score", games.count())
            for game in games:
                if game.score > max_score:
                    max_score = game.score
            # Logs.print_log("max_score", max_score)
            return max_score
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return max_score

    @staticmethod
    def get_all_username(start_datetime, end_datetime):
        try:
            start_datetime = Datetimes.transfer_datetime(start_datetime, False)
            start_datetime = Datetimes.naive_to_aware(start_datetime)
            end_datetime = Datetimes.transfer_datetime(end_datetime, False)
            end_datetime = Datetimes.naive_to_aware(end_datetime)
            return Game.objects.filter(created_at__gte=start_datetime, created_at__lte=end_datetime) \
                .values('username').distinct()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return list()

    @staticmethod
    def get_all_username_and_score(start_datetime, end_datetime, count=20):
        result = list()

        usernames = Games.get_all_username(start_datetime, end_datetime)
        for username in usernames:
            temp = dict()
            temp["username"] = username["username"][:3] + "****" + username["username"][7:]
            u = Users.get_user(username["username"])
            if u is not None:
                temp["user_id"] = u.id
            else:
                temp["user_id"] = 0
            temp["score"] = Games.get_max_score(username["username"], start_datetime, end_datetime)
            result.append(temp)

        result = Operations.sort_list_with_dict(result, "score", True)

        # before = 0
        ranking = 1
        index = 1
        for r in result:
            if index > count:
                break
            # if r["score"] == before:
            #     r["ranking"] = ranking
            #     reward_node_info = GameDayRewards.get_some_node_info(ranking)
            #     if reward_node_info:
            #         r["reward"] = reward_node_info.get("text")
            #     else:
            #         r["reward"] = 0
            #     index += 1
            #     continue
            # else:
            #     before = r["score"]
            ranking = index
            r["ranking"] = ranking
            reward_node_info = GameDayRewards.get_some_node_info(ranking)
            if reward_node_info:
                r["reward"] = reward_node_info.get("text")
            else:
                r["reward"] = 0

            index += 1

        return result
