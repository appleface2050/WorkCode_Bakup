# -*- coding: utf-8 -*-

from xmls import Xmls


class Percentages(object):
    PERCENTAGES_PATH = "eye/configurations/percentages.xml"

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Percentages.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get(file_path):
        root = Percentages.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "percentage")

    @staticmethod
    def get_name(node):
        return Xmls.get_node_attributes(node).get("name", None)

    @staticmethod
    def get_default(node):
        return int(Xmls.get_node_attributes(node).get("default", 0))

    @staticmethod
    def get_node_text(node):
        return int(Xmls.get_node_text(node))

    @staticmethod
    def get_info(node):
        result = dict()
        result["name"] = Percentages.get_name(node)
        result["default"] = Percentages.get_default(node)
        result["text"] = Percentages.get_node_text(node)
        return result

    @staticmethod
    def get_some_node(file_path, name):
        percentages = Percentages.get(file_path)
        for p in percentages:
            p_name = Percentages.get_name(p)
            if p_name == name:
                return p
        else:
            return None

    @staticmethod
    def get_some_node_info(file_path, name):
        node = Percentages.get_some_node(file_path, name)
        if node is not None:
            return Percentages.get_info(node)
        else:
            return dict()
