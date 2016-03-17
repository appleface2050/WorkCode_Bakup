# -*- coding: utf-8 -*-

import os
from xmls import Xmls
from datetimes import Datetimes
from logs import Logs

REDENVELOPE_CONFIGURATION_FILE_PATH = "project/configuration.xml"
# REDENVELOPE_NAME = "root"
REDENVELOPE_NAME = "redenvelope"
PURCHASE_NAME = "purchase"

REDENVELOPE_ATTRIBUTE_FACTOR = "factor"
REDENVELOPE_ATTRIBUTE_THRESHOLD = "person_threshold"
REDENVELOPE_DEVICE_BONUS_PATH = "device/bonus"
REDENVELOPE_EXTRA_MAX_PATH = "extra/max"
REDENVELOPE_EXTRA_MIN_PATH = "extra/min"
REDENVELOPE_EXTRA_POSSIBILITY_PATH = "extra/possibility"
REDENVELOPE_EXTRA_THRESHOLD_PATH = "extra/threshold"
REDENVELOPE_EXTRA_KEEP_PATH = "extra/keep"
REDENVELOPE_RAIN_MAX_PATH = "rain/max"
REDENVELOPE_RAIN_MIN_PATH = "rain/min"
REDENVELOPE_RAIN_POSSIBILITY_PATH = "rain/possibility"
REDENVELOPE_RAIN_THRESHOLD_PATH = "rain/threshold"
REDENVELOPE_RAIN_START_DATE_PATH = "rain/start_date"
REDENVELOPE_RAIN_END_DATE_PATH = "rain/end_date"
REDENVELOPE_RAIN_WEEK_PATH = "rain/week"
REDENVELOPE_RAIN_START_TIME_PATH = "rain/start_time"
REDENVELOPE_RAIN_END_TIME_PATH = "rain/end_time"
REDENVELOPE_RAIN_COUNT_PATH = "rain/count"

PURCHASE_URL = "url"

TIME_FORMAT = "%H:%M:%S"
DATE_FORMAT = "%Y:%m:%d"


class RedEnvelopeConfiguration(object):
    modified_time = None
    text = None
    root = None
    RED_ENVELOPE_FACTOR = None
    RED_ENVELOPE_PERSON_THRESHOLD = None
    RED_ENVELOPE_BY_DEVICE = None
    RED_ENVELOPE_EXTRA_POSSIBILITY = None
    RED_ENVELOPE_EXTRA_THRESHOLD = None
    RED_ENVELOPE_EXTRA_MIN = None
    RED_ENVELOPE_EXTRA_MAX = None
    RED_ENVELOPE_EXTRA_KEEP = None
    RED_ENVELOPE_RAIN_POSSIBILITY = None
    RED_ENVELOPE_RAIN_THRESHOLD = None
    RED_ENVELOPE_RAIN_MIN = None
    RED_ENVELOPE_RAIN_MAX = None
    RED_ENVELOPE_RAIN_START_STR = None
    RED_ENVELOPE_RAIN_START = None
    RED_ENVELOPE_RAIN_END_STR = None
    RED_ENVELOPE_RAIN_END = None
    RED_ENVELOPE_RAIN_COUNT = None
    RED_ENVELOPE_RAIN_WEEK = []
    RED_ENVELOPE_RAIN_WEEK_STR = ""
    RED_ENVELOPE_RAIN_START_DATE = ""
    RED_ENVELOPE_RAIN_START_DATE_STR = ""
    RED_ENVELOPE_RAIN_END_DATE = ""
    RED_ENVELOPE_RAIN_END_DATE_STR = ""
    PURCHASE_URL = ""

    @classmethod
    def get_file_text(cls):
        """
        查看文件是否被改变， 如果改变了，重新读取；否则用原来的数据
        """
        modified_time = os.path.getmtime(REDENVELOPE_CONFIGURATION_FILE_PATH)
        # Logs.print_log("modified_time", modified_time)
        if cls.modified_time is not None and cls.modified_time == modified_time:
            return cls.text
        else:
            cls.modified_time = modified_time
            cls.text = Xmls.get_text(REDENVELOPE_CONFIGURATION_FILE_PATH)

    @classmethod
    def get_file_root(cls):
        cls.get_file_text()
        cls.root = Xmls.get_root(cls.text)

    @classmethod
    def set_red_envelope_configuration(cls):
        try:
            cls.get_file_root()
            redenvelope = Xmls.get_all_sub_nodes(cls.root, REDENVELOPE_NAME)[0]
            cls.RED_ENVELOPE_FACTOR = int(Xmls.get_node_attribute_value(redenvelope, REDENVELOPE_ATTRIBUTE_FACTOR))
            cls.RED_ENVELOPE_PERSON_THRESHOLD = float(
                Xmls.get_node_attribute_value(redenvelope, REDENVELOPE_ATTRIBUTE_THRESHOLD))
            cls.RED_ENVELOPE_BY_DEVICE = float(Xmls.get_some_node_text(redenvelope, REDENVELOPE_DEVICE_BONUS_PATH))

            cls.RED_ENVELOPE_EXTRA_POSSIBILITY = float(
                Xmls.get_some_node_text(redenvelope, REDENVELOPE_EXTRA_POSSIBILITY_PATH))
            cls.RED_ENVELOPE_EXTRA_THRESHOLD = float(
                Xmls.get_some_node_text(redenvelope, REDENVELOPE_EXTRA_THRESHOLD_PATH))
            cls.RED_ENVELOPE_EXTRA_MIN = float(Xmls.get_some_node_text(redenvelope, REDENVELOPE_EXTRA_MIN_PATH))
            cls.RED_ENVELOPE_EXTRA_MAX = float(Xmls.get_some_node_text(redenvelope, REDENVELOPE_EXTRA_MAX_PATH))
            cls.RED_ENVELOPE_EXTRA_KEEP = float(Xmls.get_some_node_text(redenvelope, REDENVELOPE_EXTRA_KEEP_PATH))
            cls.RED_ENVELOPE_RAIN_POSSIBILITY = float(
                Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_POSSIBILITY_PATH))
            cls.RED_ENVELOPE_RAIN_THRESHOLD = float(
                Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_THRESHOLD_PATH))
            cls.RED_ENVELOPE_RAIN_MIN = float(Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_MIN_PATH))
            cls.RED_ENVELOPE_RAIN_MAX = float(Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_MAX_PATH))
            # time_format = "%H:%M:%S"
            cls.RED_ENVELOPE_RAIN_START_STR = Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_START_TIME_PATH)
            cls.RED_ENVELOPE_RAIN_START = Datetimes.string_to_time(
                Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_START_TIME_PATH))
            cls.RED_ENVELOPE_RAIN_END_STR = Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_END_TIME_PATH)
            cls.RED_ENVELOPE_RAIN_END = Datetimes.string_to_time(
                Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_END_TIME_PATH))
            cls.RED_ENVELOPE_RAIN_COUNT = int(Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_COUNT_PATH))

            week = Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_WEEK_PATH)
            cls.RED_ENVELOPE_RAIN_WEEK_STR = week
            if week:
                cls.RED_ENVELOPE_RAIN_WEEK = week.split(",")
            else:
                cls.RED_ENVELOPE_RAIN_WEEK = []
                cls.RED_ENVELOPE_RAIN_WEEK_STR = ""
            # date_format = "%Y:%m:%d"
            start_date = Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_START_DATE_PATH)
            cls.RED_ENVELOPE_RAIN_START_DATE_STR = start_date
            if start_date:
                cls.RED_ENVELOPE_RAIN_START_DATE = Datetimes.string_to_date(start_date)
            else:
                cls.RED_ENVELOPE_RAIN_START_DATE = ""
                cls.RED_ENVELOPE_RAIN_START_DATE_STR = ""
            end_date = Xmls.get_some_node_text(redenvelope, REDENVELOPE_RAIN_END_DATE_PATH)
            cls.RED_ENVELOPE_RAIN_END_DATE_STR = end_date
            if end_date:
                cls.RED_ENVELOPE_RAIN_END_DATE = Datetimes.string_to_date(end_date)
            else:
                cls.RED_ENVELOPE_RAIN_END_DATE = ""
                cls.RED_ENVELOPE_RAIN_END_DATE_STR = ""

            purchase = Xmls.get_all_sub_nodes(cls.root, PURCHASE_NAME)[0]
            url = Xmls.get_some_node_text(purchase, PURCHASE_URL)
            if url:
                cls.PURCHASE_URL = url
            else:
                cls.PURCHASE_URL = ""
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False
        return True

    @classmethod
    def set_node_attribute(cls, root, path, key, value):
        try:
            node = Xmls.get_the_first_node(root, path)
            Xmls.set_node_attribute(node, key, value)
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @classmethod
    def set_node_text(cls, root, path, text):
        try:
            node = Xmls.get_the_first_node(root, path)
            Xmls.set_node_text(node, text)
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @classmethod
    def modify_configuration(cls, new_items):
        root = Xmls.parse_xml(REDENVELOPE_CONFIGURATION_FILE_PATH)
        if root:
            pass
        else:
            return False
        for k, v in new_items.iteritems():
            try:
                if k == "factor":
                    cls.set_node_attribute(root, REDENVELOPE_NAME, k, v)
                elif k == "person_threshold":
                    cls.set_node_attribute(root, REDENVELOPE_NAME, k, v)

                elif k == "device_bonus":
                    cls.set_node_text(root, REDENVELOPE_DEVICE_BONUS_PATH, v)

                elif k == "extra_possibility":
                    cls.set_node_text(root, REDENVELOPE_EXTRA_POSSIBILITY_PATH, v)
                elif k == "extra_threshold":
                    cls.set_node_text(root, REDENVELOPE_EXTRA_THRESHOLD_PATH, v)
                elif k == "extra_max":
                    cls.set_node_text(root, REDENVELOPE_EXTRA_MAX_PATH, v)
                elif k == "extra_min":
                    cls.set_node_text(root, REDENVELOPE_EXTRA_MIN_PATH, v)
                elif k == "extra_keep":
                    cls.set_node_text(root, REDENVELOPE_EXTRA_KEEP_PATH, v)

                elif k == "rain_possibility":
                    cls.set_node_text(root, REDENVELOPE_RAIN_POSSIBILITY_PATH, v)
                elif k == "rain_threshold":
                    cls.set_node_text(root, REDENVELOPE_RAIN_THRESHOLD_PATH, v)
                elif k == "rain_max":
                    cls.set_node_text(root, REDENVELOPE_RAIN_MAX_PATH, v)
                elif k == "rain_min":
                    cls.set_node_text(root, REDENVELOPE_RAIN_MIN_PATH, v)

                elif k == "rain_start":
                    cls.set_node_text(root, REDENVELOPE_RAIN_START_TIME_PATH, v)
                elif k == "rain_end":
                    cls.set_node_text(root, REDENVELOPE_RAIN_END_TIME_PATH, v)
                elif k == "rain_count":
                    cls.set_node_text(root, REDENVELOPE_RAIN_COUNT_PATH, v)

                elif k == "rain_week":
                    cls.set_node_text(root, REDENVELOPE_RAIN_WEEK_PATH, v)
                elif k == "rain_start_date":
                    cls.set_node_text(root, REDENVELOPE_RAIN_START_DATE_PATH, v)
                elif k == "rain_end_date":
                    cls.set_node_text(root, REDENVELOPE_RAIN_END_DATE_PATH, v)
                elif k == "purchase_url":
                    cls.set_node_text(root, PURCHASE_URL, v)
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)

        root.write(REDENVELOPE_CONFIGURATION_FILE_PATH)
        return True

    @classmethod
    def get_configuration(cls):
        cls.set_red_envelope_configuration()
        redenvelope = dict()
        redenvelope["factor"] = cls.RED_ENVELOPE_FACTOR
        redenvelope["person_threshold"] = cls.RED_ENVELOPE_PERSON_THRESHOLD
        redenvelope["device_bonus"] = cls.RED_ENVELOPE_BY_DEVICE
        redenvelope["extra_possibility"] = cls.RED_ENVELOPE_EXTRA_POSSIBILITY
        redenvelope["extra_threshold"] = cls.RED_ENVELOPE_EXTRA_THRESHOLD
        redenvelope["extra_min"] = cls.RED_ENVELOPE_EXTRA_MIN
        redenvelope["extra_max"] = cls.RED_ENVELOPE_EXTRA_MAX
        redenvelope["extra_keep"] = cls.RED_ENVELOPE_EXTRA_KEEP
        redenvelope["rain_possibility"] = cls.RED_ENVELOPE_RAIN_POSSIBILITY
        redenvelope["rain_threshold"] = cls.RED_ENVELOPE_RAIN_THRESHOLD
        redenvelope["rain_min"] = cls.RED_ENVELOPE_RAIN_MIN
        redenvelope["rain_max"] = cls.RED_ENVELOPE_RAIN_MAX
        redenvelope["rain_start"] = cls.RED_ENVELOPE_RAIN_START_STR
        redenvelope["rain_end"] = cls.RED_ENVELOPE_RAIN_END_STR
        redenvelope["rain_count"] = cls.RED_ENVELOPE_RAIN_COUNT
        redenvelope["rain_week"] = cls.RED_ENVELOPE_RAIN_WEEK_STR
        redenvelope["rain_start_date"] = cls.RED_ENVELOPE_RAIN_START_DATE_STR
        redenvelope["rain_end_date"] = cls.RED_ENVELOPE_RAIN_END_DATE_STR
        redenvelope["purchase_url"] = cls.PURCHASE_URL
        return redenvelope

    @classmethod
    def print_configuration(cls):
        pass
        # print cls.RED_ENVELOPE_FACTOR
        # print cls.RED_ENVELOPE_PERSON_THRESHOLD
        # print cls.RED_ENVELOPE_BY_DEVICE
        # print cls.RED_ENVELOPE_EXTRA_POSSIBILITY
        # print cls.RED_ENVELOPE_EXTRA_THRESHOLD
        # print cls.RED_ENVELOPE_EXTRA_MIN
        # print cls.RED_ENVELOPE_EXTRA_MAX
        # print cls.RED_ENVELOPE_EXTRA_KEEP
        # print cls.RED_ENVELOPE_RAIN_POSSIBILITY
        # print cls.RED_ENVELOPE_RAIN_THRESHOLD
        # print cls.RED_ENVELOPE_RAIN_MIN
        # print cls.RED_ENVELOPE_RAIN_MAX
        # print cls.RED_ENVELOPE_RAIN_START
        # print cls.RED_ENVELOPE_RAIN_END
        # print cls.RED_ENVELOPE_RAIN_COUNT
        # print cls.RED_ENVELOPE_RAIN_WEEK
        # print cls.RED_ENVELOPE_RAIN_START_DATE
        # print cls.RED_ENVELOPE_RAIN_END_DATE
        # print cls.PURCHASE_URL
