# -*- coding: utf-8 -*-

from xmls import Xmls


class Levels(object):
    LEVELS_PATH = "eye/configurations/levels.xml"

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Levels.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get(file_path):
        root = Levels.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "level")

    @staticmethod
    def get_key(node):
        return Xmls.get_node_attributes(node).get("key", None)

    @staticmethod
    def get_min(node):
        return int(Xmls.get_node_attributes(node).get("min", 0))

    @staticmethod
    def get_max(node):
        return int(Xmls.get_node_attributes(node).get("max", 100))

    @staticmethod
    def get_node_text(node):
        return int(Xmls.get_node_text(node))

    @staticmethod
    def get_level_info(node):
        result = dict()
        result["key"] = Levels.get_key(node)
        result["min"] = Levels.get_min(node)
        result["max"] = Levels.get_max(node)
        result["name"] = Levels.get_node_text(node)
        return result

    @staticmethod
    def get_some_node(file_path, value):
        levels = Levels.get(file_path)
        for p in levels:
            min_value = Levels.get_min(p)
            max_value = Levels.get_max(p)
            if min_value <= value <= max_value:
                return p
        else:
            return None

    @staticmethod
    def get_some_node_info(file_path, value):
        node = Levels.get_some_node(file_path, value)
        if node:
            return Levels.get_level_info(node)
        else:
            return dict()
