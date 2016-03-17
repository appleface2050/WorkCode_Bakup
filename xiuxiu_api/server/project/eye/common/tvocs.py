# -*- coding: utf-8 -*-

from xmls import Xmls


class Tvocs(object):
    TVOCS_PATH = "eye/configurations/tvocs.xml"

    @staticmethod
    def get_text(file_path):
        return Xmls.get_text(file_path)

    @staticmethod
    def get_root(file_path):
        text = Tvocs.get_text(file_path)
        return Xmls.get_root(text)

    @staticmethod
    def get_Tvocs(file_path):
        root = Tvocs.get_root(file_path)
        return Xmls.get_all_path_nodes(root, "tvoc")

    @staticmethod
    def get_tvoc_max(node):
        return int(Xmls.get_node_attributes(node).get("max", 99999))

    @staticmethod
    def get_tvoc_min(node):
        return int(Xmls.get_node_attributes(node).get("min", 0))

    @staticmethod
    def get_tvoc_score(node):
        return int(Xmls.get_node_attributes(node).get("score", 0))

    @staticmethod
    def get_tvoc_text(node):
        return Xmls.get_node_text(node)

    @staticmethod
    def get_node_info(node):
        result = dict()
        result["min"] = Tvocs.get_tvoc_min(node)
        result["max"] = Tvocs.get_tvoc_max(node)
        result["score"] = Tvocs.get_tvoc_score(node)
        result["text"] = Tvocs.get_tvoc_text(node)
        return result

    @staticmethod
    def get_some_node(file_path, value):
        tvocs = Tvocs.get_Tvocs(file_path)
        for tvoc in tvocs:
            min = Tvocs.get_tvoc_min(tvoc)
            max = Tvocs.get_tvoc_max(tvoc)
            if min == 0:
                if min <= value <= max:
                    return tvoc
            else:
                if min < value <= max:
                    return tvoc
        else:
            return None

    @staticmethod
    def get_some_node_info(file_path, value):
        node = Tvocs.get_some_node(file_path, value)
        if node:
            return Tvocs.get_node_info(node)
        else:
            return dict()

# # -*- coding: utf-8 -*-
#
# from xmls import Xmls
#
#
# class Tvocs(object):
#     TVOCS_PATH = "eye/configurations/tvocs.xml"
#
#     @staticmethod
#     def get_text(file_path):
#         return Xmls.get_text(file_path)
#
#     @staticmethod
#     def get_root(file_path):
#         text = Tvocs.get_text(file_path)
#         return Xmls.get_root(text)
#
#     @staticmethod
#     def get_tvocs(file_path):
#         root = Tvocs.get_root(file_path)
#         return Xmls.get_all_path_nodes(root, "tvoc")
#
#     @staticmethod
#     def get_tvoc_max(node):
#         return float(Xmls.get_node_attributes(node).get("max", 99999))
#
#     @staticmethod
#     def get_tvoc_min(node):
#         return float(Xmls.get_node_attributes(node).get("min", 0))
#
#     @staticmethod
#     def get_tvoc_score(node):
#         return int(Xmls.get_node_attributes(node).get("score", 0))
#
#     @staticmethod
#     def get_tvoc_text(node):
#         return Xmls.get_node_text(node)
#
#     @staticmethod
#     def get_node_info(node):
#         result = dict()
#         result["min"] = Tvocs.get_tvoc_min(node)
#         result["max"] = Tvocs.get_tvoc_max(node)
#         result["score"] = Tvocs.get_tvoc_score(node)
#         result["text"] = Tvocs.get_tvoc_text(node)
#         return result
#
#     @staticmethod
#     def get_some_node(file_path, value):
#         tvocs = Tvocs.get_tvocs(file_path)
#         for tvoc in tvocs:
#             min = Tvocs.get_tvoc_min(tvoc)
#             max = Tvocs.get_tvoc_max(tvoc)
#             if min <= value <= max:
#                 return tvoc
#         else:
#             return None
#
#     @staticmethod
#     def get_some_node_info(file_path, value):
#         node = Tvocs.get_some_node(file_path, value)
#         if node:
#             return Tvocs.get_node_info(node)
#         else:
#             return dict()
