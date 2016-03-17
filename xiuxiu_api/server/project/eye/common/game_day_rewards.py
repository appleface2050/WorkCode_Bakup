# -*- coding: utf-8 -*-

from xmls import Xmls


class GameDayRewards(object):
    GAME_DAY_REWARDS_PATH = "eye/configurations/game_day_rewards.xml"

    @staticmethod
    def get_text(file_path=None):
        if file_path is None:
            file_path = GameDayRewards.GAME_DAY_REWARDS_PATH
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path=None):
        text = GameDayRewards.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get_rewards(file_path=None):
        root = GameDayRewards.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "reward")

    @staticmethod
    def get_reward_max(node):
        return int(Xmls.get_node_attributes(node).get("max", 99999))

    @staticmethod
    def get_reward_min(node):
        return int(Xmls.get_node_attributes(node).get("min", 99999))

    @staticmethod
    def get_reward_text(node):
        return Xmls.get_node_text(node)

    @staticmethod
    def get_node_info(node):
        result = dict()
        result["min"] = GameDayRewards.get_reward_min(node)
        result["max"] = GameDayRewards.get_reward_max(node)
        result["text"] = GameDayRewards.get_reward_text(node)
        return result

    @staticmethod
    def get_some_node(value, file_path=None):
        rewards = GameDayRewards.get_rewards(file_path)
        for reward in rewards:
            min = GameDayRewards.get_reward_min(reward)
            max = GameDayRewards.get_reward_max(reward)
            if min == 99999 or max == 99999:
                return None
            if min <= value <= max:
                return reward
        else:
            return None

    @staticmethod
    def get_some_node_info(value, file_path=None):
        node = GameDayRewards.get_some_node(value, file_path)
        if node is not None:
            return GameDayRewards.get_node_info(node)
        else:
            return dict()
