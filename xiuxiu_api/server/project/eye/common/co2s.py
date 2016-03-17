# -*- coding: utf-8 -*-

from xmls import Xmls


class Co2s(object):
    CO2S_PATH = "eye/configurations/co2s.xml"

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Co2s.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get(file_path):
        root = Co2s.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "co2")

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
        result["min"] = Co2s.get_min(node)
        result["max"] = Co2s.get_max(node)
        result["score"] = Co2s.get_score(node)
        result["text"] = Co2s.get_node_text(node)
        return result

    @staticmethod
    def get_some_node(file_path, value):
        co2s = Co2s.get(file_path)
        for co2 in co2s:
            min = Co2s.get_min(co2)
            max = Co2s.get_max(co2)
            if min == 0:
                if min <= value <= max:
                    return co2
            else:
                if min < value <= max:
                    return co2
        else:
            return None

    @staticmethod
    def get_some_node_info(file_path, value):
        node = Co2s.get_some_node(file_path, value)
        if node:
            return Co2s.get_node_info(node)
        else:
            return dict()
