# coding:utf-8
import datetime
from .models import RedEnvelopeConstant
from common_functions import XmlOperations
from xml.etree import ElementTree
from common.logs import Logs

REDENVELOPE_CONFIGURATION_FILE_PATH = "project/configuration.xml"
REDENVELOPE_NAME = "redenvelope"

REDENVELOPE_ATTRIBUTE_FACTOR = "factor"
REDENVELOPE_ATTRIBUTE_THRESHOLD = "person_threshold"
REDENVELOPE_DEVICE_BONUS_PATH = "redenvelope/device/bonus"
REDENVELOPE_EXTRA_MAX_PATH = "redenvelope/extra/max"
REDENVELOPE_EXTRA_MIN_PATH = "redenvelope/extra/min"
REDENVELOPE_EXTRA_POSSIBILITY_PATH = "redenvelope/extra/possibility"
REDENVELOPE_EXTRA_THRESHOLD_PATH = "redenvelope/extra/threshold"
REDENVELOPE_EXTRA_KEEP_PATH = "redenvelope/extra/keep"
REDENVELOPE_RAIN_MAX_PATH = "redenvelope/rain/max"
REDENVELOPE_RAIN_MIN_PATH = "redenvelope/rain/min"
REDENVELOPE_RAIN_POSSIBILITY_PATH = "redenvelope/rain/possibility"
REDENVELOPE_RAIN_THRESHOLD_PATH = "redenvelope/rain/threshold"
REDENVELOPE_RAIN_START_DATE_PATH = "redenvelope/rain/start_date"
REDENVELOPE_RAIN_END_DATE_PATH = "redenvelope/rain/end_date"
REDENVELOPE_RAIN_WEEK_PATH = "redenvelope/rain/week"
REDENVELOPE_RAIN_START_TIME_PATH = "redenvelope/rain/start_time"
REDENVELOPE_RAIN_END_TIME_PATH = "redenvelope/rain/end_time"
REDENVELOPE_RAIN_COUNT_PATH = "redenvelope/rain/count"

PURCHASE_URL = "purchase/url"

TIME_FORMAT = "%H:%M:%S"
DATE_FORMAT = "%Y:%m:%d"


class RULES_CONSTANT():
    # 总分百分比
    TOTAL_PERCENTAGE = {
        "PM2_5": 40,
        "TVOC": 0,
        "FORMALDEHYDE": 40,
        "CO2": 0,
        "SUBJECTIVE": 20
    }

    DEFAULT_VALUE = {
        "PM2_5": 0,
        "TVOC": 0,
        "FORMALDEHYDE": 50,
        "CO2": 0,
        "SUBJECTIVE": 100
    }

    # 总分与级别的换算
    LEVEL_A = {
        "key": u"优",
        "name": u"好",
        "min_value": 76,
        "max_value": 100
    }

    LEVEL_B = {
        "key": u"良",
        "name": u"满意",
        "min_value": 51,
        "max_value": 75
    }

    LEVEL_C = {
        "key": u"中",
        "name": u"一般",
        "min_value": 26,
        "max_value": 50
    }

    LEVEL_D = {
        "key": u"差",
        "name": u"不好",
        "min_value": -10,
        "max_value": 25
    }
    LEVELS = [LEVEL_A, LEVEL_B, LEVEL_C, LEVEL_D]

    # PM2.5的级别和分数换算 min_value < VALUE <= max_value
    PM2_5_DEGREE = [
        {'name': u'空气良好', 'max_value': 35, 'score': 100},
        {'name': u'空气正常', 'min_value': 36, 'max_value': 75, 'score': 80},
        {'name': u'轻度污染', 'min_value': 76, 'max_value': 115, 'score': 0},
        {'name': u'中度污染', 'min_value': 116, 'max_value': 150, 'score': -5},
        {'name': u'重度污染', 'min_value': 151, 'max_value': 250, 'score': -10},
        {'name': u'严重污染', 'min_value': 251, 'max_value': 350, 'score': -15},
        {'name': u'不宜生存', 'min_value': 351, 'max_value': 99999, 'score': -20},
    ]

    # 甲醛的级别和分数换算 min_value < VALUE <= max_value
    FORMALDEHYDE_DEGREE = [
        {'name': u'舒适', 'max_value': 0.05, 'score': 100},
        {'name': u'偏高', 'min_value': 0.05, 'max_value': 0.08, 'score': 50},
        {'name': u'超标', 'min_value': 0.08, 'score': -10},
    ]

    # 二氧化碳的级别和分数换算 min_value < VALUE <= max_value
    CO2_DEGREE = [
        {'name': u'优', 'max_value': 1400, 'score': 100},
        {'name': u'良', 'min_value': 1400, 'max_value': 1800, 'score': 80},
        {'name': u'中', 'min_value': 1800, 'max_value': 2000, 'score': 40},
        {'name': u'差', 'min_value': 2000, 'score': 0},
    ]

    # TVOC的级别和分数换算min_value < VALUE <= max_value
    TVOC_DEGREE = [
        {'name': u'优', 'max_value': 0.3, 'score': 100},
        {'name': u'良', 'min_value': 0.3, 'max_value': 0.5, 'score': 70},
        {'name': u'中', 'min_value': 0.5, 'max_value': 0.6, 'score': 30},
        {'name': u'差', 'min_value': 0.6, 'score': -10},
    ]

    RED_ENVELOPE_STATE_INVALID = -1
    RED_ENVELOPE_STATE_REQUEST = 0
    RED_ENVELOPE_STATE_VALID = 1

    RED_ENVELOPE_TYPE_DEVICE = 0
    RED_ENVELOPE_TYPE_EXTRA = 1
    RED_ENVELOPE_TYPE_RAIN = 2

    # RED_ENVELOPE_EXTRA_POSSIBILITY = 0.5
    # RED_ENVELOPE_RAIN_POSSIBILITY = 0.5
    #
    # RED_ENVELOPE_BY_DEVICE = 0.2
    # RED_ENVELOPE_EXTRA_THRESHOLD = 0
    # # RED_ENVELOPE_EXTRA_TIME = 30
    # RED_ENVELOPE_EXTRA_MIN = 0.01
    # RED_ENVELOPE_EXTRA_MAX = 1
    # RED_ENVELOPE_EXTRA_KEEP = 5
    # RED_ENVELOPE_RAIN_THRESHOLD = 5000
    # # RED_ENVELOPE_RAIN_TIME = 7
    # RED_ENVELOPE_RAIN_MIN = 0.01
    # RED_ENVELOPE_RAIN_MAX = 0.5
    # RED_ENVELOPE_FACTOR = 100
    # TIME_FORMAT = "%H:%M:%S"
    # RED_ENVELOPE_RAIN_START = datetime.datetime.strptime("00:00:00", TIME_FORMAT)
    # RED_ENVELOPE_RAIN_END = datetime.datetime.strptime("23:59:00", TIME_FORMAT)
    # RED_ENVELOPE_RAIN_COUNT = 90
    #
    # RED_ENVELOPE_PERSON_THRESHOLD = 119.0

    # @classmethod
    # def set_configuration_device(cls):
    #     result = {}
    #     devices = RedEnvelopeConstant.objects.filter(type=cls.RED_ENVELOPE_TYPE_DEVICE)
    #     if devices.count() == 1:
    #         device = devices[0]
    #         cls.RED_ENVELOPE_BY_DEVICE = device.bonus_min
    #
    # @classmethod
    # def set_configuration_extra(cls):
    #     result = {}
    #     extras = RedEnvelopeConstant.objects.filter(type=cls.RED_ENVELOPE_TYPE_EXTRA)
    #     if extras.count() == 1:
    #         extra = extras[0]
    #         cls.RED_ENVELOPE_EXTRA_MIN = extra.bonus_min
    #         cls.RED_ENVELOPE_EXTRA_MAX = extra.bonus_max
    #         cls.RED_ENVELOPE_EXTRA_THRESHOLD = extra.threshold
    #         cls.RED_ENVELOPE_EXTRA_POSSIBILITY = extra.possibility
    #         cls.RED_ENVELOPE_EXTRA_KEEP = extra.extra_keep
    #
    # @classmethod
    # def set_configuration_rain(cls):
    #     result = {}
    #     rains = RedEnvelopeConstant.objects.filter(type=cls.RED_ENVELOPE_TYPE_RAIN)
    #     if rains.count() == 1:
    #         rain = rains[0]
    #         cls.RED_ENVELOPE_RAIN_MIN = rain.bonus_min
    #         cls.RED_ENVELOPE_RAIN_MAX = rain.bonus_max
    #         cls.RED_ENVELOPE_RAIN_THRESHOLD = rain.threshold
    #         cls.RED_ENVELOPE_RAIN_POSSIBILITY = rain.possibility
    #         cls.RED_ENVELOPE_RAIN_START = rain.start_time
    #         cls.RED_ENVELOPE_RAIN_END = rain.end_time
    #         cls.RED_ENVELOPE_RAIN_COUNT = rain.rain_count

    # xml operations
    @classmethod
    def get_root(cls):
        try:
            xo = XmlOperations(REDENVELOPE_CONFIGURATION_FILE_PATH)
            xo.get_text()
            cls.root = xo.get_root()
        except Exception as ex:
            print ex.message
            cls.root = []

    @classmethod
    def get_node_attribute(cls, node, attribute_key):
        return XmlOperations.get_node_attribute_value(node, attribute_key)

    @classmethod
    def get_node_text(cls, node_path):
        node = XmlOperations.get_all_path_nodes(cls.root, node_path)
        if node:
            return node[0].text
        else:
            return ""

    @classmethod
    def set_red_envelope_configuration(cls):
        cls.get_root()
        try:
            redenvelope = XmlOperations.get_all_sub_nodes(cls.root, "redenvelope")[0]
            cls.RED_ENVELOPE_FACTOR = int(cls.get_node_attribute(redenvelope, REDENVELOPE_ATTRIBUTE_FACTOR))
            cls.RED_ENVELOPE_PERSON_THRESHOLD = float(cls.get_node_attribute(redenvelope, REDENVELOPE_ATTRIBUTE_THRESHOLD))

            cls.RED_ENVELOPE_BY_DEVICE = float(cls.get_node_text(REDENVELOPE_DEVICE_BONUS_PATH))

            cls.RED_ENVELOPE_EXTRA_POSSIBILITY = float(cls.get_node_text(REDENVELOPE_EXTRA_POSSIBILITY_PATH))
            cls.RED_ENVELOPE_EXTRA_THRESHOLD = float(cls.get_node_text(REDENVELOPE_EXTRA_THRESHOLD_PATH))
            cls.RED_ENVELOPE_EXTRA_MIN = float(cls.get_node_text(REDENVELOPE_EXTRA_MIN_PATH))
            cls.RED_ENVELOPE_EXTRA_MAX = float(cls.get_node_text(REDENVELOPE_EXTRA_MAX_PATH))
            cls.RED_ENVELOPE_EXTRA_KEEP = float(cls.get_node_text(REDENVELOPE_EXTRA_KEEP_PATH))

            cls.RED_ENVELOPE_RAIN_POSSIBILITY = float(cls.get_node_text(REDENVELOPE_RAIN_POSSIBILITY_PATH))
            cls.RED_ENVELOPE_RAIN_THRESHOLD = float(cls.get_node_text(REDENVELOPE_RAIN_THRESHOLD_PATH))
            cls.RED_ENVELOPE_RAIN_MIN = float(cls.get_node_text(REDENVELOPE_RAIN_MIN_PATH))
            cls.RED_ENVELOPE_RAIN_MAX = float(cls.get_node_text(REDENVELOPE_RAIN_MAX_PATH))
            # time_format = "%H:%M:%S"
            cls.RED_ENVELOPE_RAIN_START_STR = cls.get_node_text(REDENVELOPE_RAIN_START_TIME_PATH)
            cls.RED_ENVELOPE_RAIN_START = datetime.datetime.strptime(cls.get_node_text(REDENVELOPE_RAIN_START_TIME_PATH),
                                                                     TIME_FORMAT)
            cls.RED_ENVELOPE_RAIN_END_STR = cls.get_node_text(REDENVELOPE_RAIN_END_TIME_PATH)
            cls.RED_ENVELOPE_RAIN_END = datetime.datetime.strptime(cls.get_node_text(REDENVELOPE_RAIN_END_TIME_PATH),
                                                                   TIME_FORMAT)
            cls.RED_ENVELOPE_RAIN_COUNT = int(cls.get_node_text(REDENVELOPE_RAIN_COUNT_PATH))

            week = cls.get_node_text(REDENVELOPE_RAIN_WEEK_PATH)
            cls.RED_ENVELOPE_RAIN_WEEK_STR = week
            if week:
                cls.RED_ENVELOPE_RAIN_WEEK = week.split(",")
            else:
                cls.RED_ENVELOPE_RAIN_WEEK = []
                cls.RED_ENVELOPE_RAIN_WEEK_STR = ""
            # date_format = "%Y:%m:%d"
            start_date = cls.get_node_text(REDENVELOPE_RAIN_START_DATE_PATH)
            cls.RED_ENVELOPE_RAIN_START_DATE_STR = start_date
            if start_date:
                cls.RED_ENVELOPE_RAIN_START_DATE = datetime.datetime.strptime(start_date, DATE_FORMAT)
            else:
                cls.RED_ENVELOPE_RAIN_START_DATE = ""
                cls.RED_ENVELOPE_RAIN_START_DATE_STR = ""
            end_date = cls.get_node_text(REDENVELOPE_RAIN_END_DATE_PATH)
            cls.RED_ENVELOPE_RAIN_END_DATE_STR = end_date
            if end_date:
                cls.RED_ENVELOPE_RAIN_END_DATE = datetime.datetime.strptime(end_date, DATE_FORMAT)
            else:
                cls.RED_ENVELOPE_RAIN_END_DATE = ""
                cls.RED_ENVELOPE_RAIN_END_DATE_STR = ""

            url = cls.get_node_text(PURCHASE_URL)
            if url:
                cls.PURCHASE_URL = url
            else:
                cls.PURCHASE_URL = ""
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

    @classmethod
    def set_node_text(cls, root, node_path, text):
        try:
            node = XmlOperations.get_all_path_nodes(root, node_path)[0]
            node.text = text
        except Exception as ex:
            print ex.message

    @classmethod
    def set_node_attribute(cls, root, node_path, attribute_key, attribute_value):
        try:
            node = XmlOperations.get_all_path_nodes(root, node_path)[0]
            node.set(attribute_key, attribute_value)
        except Exception as ex:
            print ex.message

    @classmethod
    def modify_configuration(cls, new_items):
        root = ElementTree.parse(REDENVELOPE_CONFIGURATION_FILE_PATH)
        for k, v in new_items.iteritems():
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

        root.write(REDENVELOPE_CONFIGURATION_FILE_PATH)

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
        print cls.RED_ENVELOPE_FACTOR
        print cls.RED_ENVELOPE_PERSON_THRESHOLD
        print cls.RED_ENVELOPE_BY_DEVICE
        print cls.RED_ENVELOPE_EXTRA_POSSIBILITY
        print cls.RED_ENVELOPE_EXTRA_THRESHOLD
        print cls.RED_ENVELOPE_EXTRA_MIN
        print cls.RED_ENVELOPE_EXTRA_MAX
        print cls.RED_ENVELOPE_EXTRA_KEEP
        print cls.RED_ENVELOPE_RAIN_POSSIBILITY
        print cls.RED_ENVELOPE_RAIN_THRESHOLD
        print cls.RED_ENVELOPE_RAIN_MIN
        print cls.RED_ENVELOPE_RAIN_MAX
        print cls.RED_ENVELOPE_RAIN_START
        print cls.RED_ENVELOPE_RAIN_END
        print cls.RED_ENVELOPE_RAIN_COUNT
        print cls.RED_ENVELOPE_RAIN_WEEK
        print cls.RED_ENVELOPE_RAIN_START_DATE
        print cls.RED_ENVELOPE_RAIN_END_DATE
        print cls.PURCHASE_URL
