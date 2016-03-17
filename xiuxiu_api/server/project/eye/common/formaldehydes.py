# -*- coding: utf-8 -*-

from xmls import Xmls


class Formaldehydes(object):
    FORMALDEHYDES_PATH = "eye/configurations/formaldehydes.xml"

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Formaldehydes.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get(file_path):
        root = Formaldehydes.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "formaldehyde")

    @staticmethod
    def get_max(node):
        return float(Xmls.get_node_attributes(node).get("max", 99999))

    @staticmethod
    def get_min(node):
        return float(Xmls.get_node_attributes(node).get("min", 0))

    @staticmethod
    def get_score(node):
        return int(Xmls.get_node_attributes(node).get("score", 0))

    @staticmethod
    def get_node_text(node):
        return Xmls.get_node_text(node)

    @staticmethod
    def get_node_info(node):
        result = dict()
        result["min"] = Formaldehydes.get_min(node)
        result["max"] = Formaldehydes.get_max(node)
        result["score"] = Formaldehydes.get_score(node)
        result["text"] = Formaldehydes.get_node_text(node)
        return result

    @staticmethod
    def get_some_node(file_path, value):
        all_formaldehyde = Formaldehydes.get(file_path)
        for f in all_formaldehyde:
            min_value = Formaldehydes.get_min(f)
            max_value = Formaldehydes.get_max(f)

            if min_value == 0:
                if min_value <= value <= max_value:
                    return f
            else:
                if min_value < value <= max_value:
                    return f
        else:
            return None

    @staticmethod
    def get_some_node_info(file_path, value):
        node = Formaldehydes.get_some_node(file_path, value)
        if node:
            return Formaldehydes.get_node_info(node)
        else:
            return dict()
