# -*- coding: utf-8 -*-

from xmls import Xmls


class Pm25s(object):
    PM25S_PATH = "eye/configurations/pm25s.xml"

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Pm25s.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get(file_path):
        root = Pm25s.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "pm25")

    @staticmethod
    def get_max(node):
        return int(Xmls.get_node_attributes(node).get("max", 99999))

    @staticmethod
    def get_min(node):
        return int(Xmls.get_node_attributes(node).get("min", 0))

    @staticmethod
    def get_score(node):
        return int(Xmls.get_node_attributes(node).get("score", 0))

    @staticmethod
    def get_node_text(node):
        return Xmls.get_node_text(node)

    @staticmethod
    def get_node_info(node):
        result = dict()
        result["min"] = Pm25s.get_min(node)
        result["max"] = Pm25s.get_max(node)
        result["score"] = Pm25s.get_score(node)
        result["text"] = Pm25s.get_node_text(node)
        return result

    @staticmethod
    def get_some_node(file_path, value):
        pm25s = Pm25s.get(file_path)
        for pm25 in pm25s:
            min = Pm25s.get_min(pm25)
            max = Pm25s.get_max(pm25)
            if min <= value <= max:
                return pm25
        else:
            return None

    @staticmethod
    def get_some_node_info(file_path, value):
        node = Pm25s.get_some_node(file_path, value)
        if node is not None:
            return Pm25s.get_node_info(node)
        else:
            return dict()
