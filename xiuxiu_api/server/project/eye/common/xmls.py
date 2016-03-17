# -*- coding: utf-8 -*-

from xml.etree import ElementTree
from logs import Logs


class Xmls(object):
    @staticmethod
    def get_text(file_path):
        try:
            return open(file_path).read()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def parse_xml(file_path):
        try:
            return ElementTree.parse(file_path)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_root(text):
        try:
            return ElementTree.fromstring(text)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @classmethod
    def get_all_sub_nodes(cls, node, node_name):
        try:
            return node.getiterator(node_name)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return []

    @classmethod
    def get_all_path_nodes(cls, node, node_path):
        try:
            return node.findall(node_path)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return []

    @classmethod
    def get_node_children(cls, node):
        try:
            return node.getchildren()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return []

    @classmethod
    def get_the_first_node(cls, node, node_name):
        try:
            return node.find(node_name)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return {}

    @classmethod
    def get_node_attributes(cls, node):
        try:
            return node.attrib
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return {}

    @classmethod
    def get_node_attribute_value(cls, node, attribute_key):
        try:
            return node.attrib[attribute_key]
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_node_text(node):
        return node.text

    @staticmethod
    def get_some_node_text(node, node_name):
        try:
            some_node = Xmls.get_the_first_node(node, node_name)
            if some_node is not None:
                return Xmls.get_node_text(some_node)
            else:
                return ""
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return ""

    @staticmethod
    def set_node_text(node, text):
        try:
            node.text = text
            return node
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def set_node_attribute(node, attribute_key, attribute_value):
        try:
            node.set(attribute_key, attribute_value)
            return node
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None
