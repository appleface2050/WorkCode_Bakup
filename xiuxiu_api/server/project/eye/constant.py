# coding:utf-8
from django.conf import settings

BASE_URL_4_IMAGE = settings.CDN_URL + settings.MEDIA_URL
BASE_URL_4_SHOP = BASE_URL_4_IMAGE + "shop/default/"


def get_clean(name):
    return {
        0: name + u"干净",
        1: name + u"较干净",
        2: name + u"脏",
    }


def get_clean_4_all(name):
    return [
        {"key": 0, "name": name + u"干净", "score": 100},
        {"key": 1, "name": name + u"较干净", "score": 50},
        {"key": 2, "name": name + u"脏", "score": 0}
    ]


def get_has(name):
    return {
        0: u"无" + name,
        1: u"有" + name,
    }


def get_has_4_all(name, zero_score=0):
    if not zero_score:
        one_score = 100
    else:
        one_score = 0
    return [
        {"key": 0, "name": u"无" + name, "score": zero_score},
        {"key": 1, "name": u"有" + name, "score": one_score},
    ]


class PUBLISH_SUBJECTIVE_CONSTANT():
    FIELDS_TABLE_KEY = "table"
    FIELDS_FLOOR_KEY = "floor"
    FIELDS_TABLEWARE_KEY = "tableware"
    FIELDS_PEOPLE_KEY = "people"
    FIELDS_QUIET_KEY = "quiet"
    FIELDS_CHILD_KEY = "child"
    FIELDS_LAMPBLACK_KEY = "lampblack"
    FIELDS_HAS_MONITOR_KEY = "has_monitor"
    FIELDS_SMELL_KEY = "smell"
    FIELDS_EQUIPMENT_NEW_KEY = "equipment_new"
    FIELDS_EQUIPMENT_CLEAN_KEY = "equipment_clean"
    FIELDS_AREA_KEY = "area"
    FIELDS_WATER_KEY = "water"
    FIELDS_LIGHT_KEY = "light"
    FIELDS_BEDSHEET_KEY = "bedsheet"
    FIELDS_SOUNDPROOF_KEY = "soundproof"
    FIELDS_WET_KEY = "wet"
    FIELDS_PET_KEY = "pet"
    FIELDS_TOILET_KEY = "toilet"
    FIELDS_WATCHMAN_KEY = "watchman"

    FIELDS_TABLE_VALUE = u"桌子干不干净"
    FIELDS_FLOOR_VALUE = u"环境是否干净"
    FIELDS_TABLEWARE_VALUE = u"餐具是否干净"
    FIELDS_PEOPLE_VALUE = u"人多不多"
    FIELDS_QUIET_VALUE = u"是否安静"
    FIELDS_CHILD_VALUE = u"是否适合带孩子玩"
    FIELDS_LAMPBLACK_VALUE = u"有无油烟"
    FIELDS_HAS_MONITOR_VALUE = u"有无摄像头"
    FIELDS_SMELL_VALUE = u"有无异味"
    FIELDS_EQUIPMENT_NEW_VALUE = u"器材新旧"
    FIELDS_EQUIPMENT_CLEAN_VALUE = u"设施是否干净"
    FIELDS_AREA_VALUE = u"是否宽敞"
    FIELDS_WATER_VALUE = u"游泳池水质"
    FIELDS_LIGHT_VALUE = u"光线如何"
    FIELDS_BEDSHEET_VALUE = u"床单是否干净"
    FIELDS_SOUNDPROOF_VALUE = u"隔音好不好"
    FIELDS_WET_VALUE = u"是否潮湿"
    FIELDS_PET_VALUE = u"能不能带宠物"
    FIELDS_TOILET_VALUE = u"有无厕所"
    FIELDS_WATCHMAN_VALUE = u"有无看护人员"

    CLEAN_ITEMS = {
        0: u"干净",
        1: u"一般",
        2: u"脏",
    }
    CLEAN_ITEMS_4_ALL = [
        {"key": 0, "name": u"干净"},
        {"key": 1, "name": u"一般"},
        {"key": 2, "name": u"脏"},
    ]

    CROWD_ITEMS = {
        0: u"少",
        1: u"较多",
        2: u"多",
    }
    CROWD_ITEMS_4_ALL = [
        {"key": 0, "name": u"少"},
        {"key": 1, "name": u"较多"},
        {"key": 2, "name": u"多"},
    ]
    QUIET_ITEMS = {
        0: u"安静",
        1: u"一般",
        2: u"吵闹",
    }
    QUIET_ITEMS_4_ALL = [
        {"key": 0, "name": u"安静"},
        {"key": 1, "name": u"一般"},
        {"key": 2, "name": u"吵闹"},
    ]

    ADAPT_ITEMS = {
        0: u"不宜",
        1: u"适合",
    }
    ADAPT_ITEMS_4_ALL = [
        {"key": 0, "name": u"不宜"},
        {"key": 1, "name": u"适合"},
    ]
    HAS_ITEMS = {
        0: u"无",
        1: u"有",
    }
    HAS_ITEMS_4_ALL = [
        {"key": 0, "name": u"无"},
        {"key": 1, "name": u"有"},
    ]
    NEW_ITEMS = {
        0: u"旧",
        1: u"新",
    }
    NEW_ITEMS_4_ALL = [
        {"key": 0, "name": u"旧"},
        {"key": 1, "name": u"新"},
    ]
    ROOMY_ITEMS = {
        0: u"狭窄",
        1: u"宽敞",
    }
    ROOMY_ITEMS_4_ALL = [
        {"key": 0, "name": u"狭窄"},
        {"key": 1, "name": u"宽敞"},
    ]
    LIGHT_ITEMS = {
        0: u"较暗",
        1: u"舒适",
        2: u"明亮",
    }
    LIGHT_ITEMS_4_ALL = [
        {"key": 0, "name": u"较暗"},
        {"key": 1, "name": u"舒适"},
        {"key": 2, "name": u"明亮"},
    ]
    GOOD_ITEMS = {
        0: u"不好",
        1: u"好"
    }
    GOOD_ITEMS_4_ALL = [
        {"key": 0, "name": u"不好"},
        {"key": 1, "name": u"好"},
    ]
    WET_ITEMS = {
        0: u"干燥",
        1: u"潮湿",
    }
    WET_ITEMS_4_ALL = [
        {"key": 0, "name": u"干燥"},
        {"key": 1, "name": u"潮湿"},
    ]
    WATER_ITEMS = {
        0: u"无泳池",
        1: u"干净",
        2: u"脏",
    }
    WATER_ITEMS_4_ALL = [
        {"key": 0, "name": u"无泳池"},
        {"key": 1, "name": u"干净"},
        {"key": 2, "name": u"脏"},
    ]
    CAN_ITEMS = {
        0: u"不能",
        1: u"能",
    }
    CAN_ITEMS_4_ALL = [
        {"key": 0, "name": u"不能"},
        {"key": 1, "name": u"能"},
    ]
    QUESTION_ANSWERS = {
        FIELDS_TABLE_KEY: CLEAN_ITEMS,
        FIELDS_FLOOR_KEY: CLEAN_ITEMS,
        FIELDS_TABLEWARE_KEY: CLEAN_ITEMS,
        FIELDS_BEDSHEET_KEY: CLEAN_ITEMS,
        FIELDS_EQUIPMENT_CLEAN_KEY: CLEAN_ITEMS,
        FIELDS_HAS_MONITOR_KEY: HAS_ITEMS,
        FIELDS_LAMPBLACK_KEY: HAS_ITEMS,
        FIELDS_TOILET_KEY: HAS_ITEMS,
        FIELDS_SMELL_KEY: HAS_ITEMS,
        FIELDS_WATCHMAN_KEY: HAS_ITEMS,
        FIELDS_PEOPLE_KEY: CROWD_ITEMS,
        FIELDS_QUIET_KEY: QUIET_ITEMS,
        FIELDS_CHILD_KEY: ADAPT_ITEMS,
        FIELDS_EQUIPMENT_NEW_KEY: NEW_ITEMS,
        FIELDS_AREA_KEY: ROOMY_ITEMS,
        FIELDS_WATER_KEY: WATER_ITEMS,
        FIELDS_LIGHT_KEY: LIGHT_ITEMS,
        FIELDS_SOUNDPROOF_KEY: GOOD_ITEMS,
        FIELDS_WET_KEY: WET_ITEMS,
        FIELDS_PET_KEY: CAN_ITEMS,
    }

    FIELDS_TABLE_SHOW = get_clean(u"桌子")
    FIELDS_TABLE_SHOW_4_ALL = get_clean_4_all(u"桌子")
    FIELDS_FLOOR_SHOW = get_clean(u"环境")
    FIELDS_FLOOR_SHOW_4_ALL = get_clean_4_all(u"环境")
    FIELDS_TABLEWARE_SHOW = get_clean(u"餐具")
    FIELDS_TABLEWARE_SHOW_4_ALL = get_clean_4_all(u"餐具")
    FIELDS_BEDSHEET_SHOW = get_clean(u"床单")
    FIELDS_BEDSHEET_SHOW_4_ALL = get_clean_4_all(u"床单")
    FIELDS_EQUIPMENT_CLEAN_SHOW = get_clean(u"设施")
    FIELDS_EQUIPMENT_CLEAN_SHOW_4_ALL = get_clean_4_all(u"设施")
    FIELDS_HAS_MONITOR_SHOW = get_has(u"摄像头")
    FIELDS_HAS_MONITOR_SHOW_4_ALL = get_has_4_all(u"摄像头")
    FIELDS_TOILET_SHOW = get_has(u"厕所")
    FIELDS_TOILET_SHOW_4_ALL = get_has_4_all(u"厕所")
    FIELDS_WATCHMAN_SHOW = get_has(u"看护人员")
    FIELDS_WATCHMAN_SHOW_4_ALL = get_has_4_all(u"看护人员")
    FIELDS_LAMPBLACK_SHOW = get_has(u"油烟")
    FIELDS_LAMPBLACK_SHOW_4_ALL = get_has_4_all(u"油烟", zero_score=100)
    FIELDS_SMELL_SHOW = get_has(u"异味")
    FIELDS_SMELL_SHOW_4_ALL = get_has_4_all(u"异味", zero_score=100)

    FIELDS_AREA_SHOW = {
        0: u"狭窄",
        1: u"宽敞",
    }
    FIELDS_AREA_SHOW_4_ALL = [
        {"key": 0, "name": u"狭窄", "score": 0},
        {"key": 1, "name": u"宽敞", "score": 100}
    ]
    FIELDS_QUIET_SHOW = {
        0: u"安静",
        1: u"较安静",
        2: u"吵闹"
    }
    FIELDS_QUIET_SHOW_4_ALL = [
        {"key": 0, "name": u"安静", "score": 100},
        {"key": 1, "name": u"较安静", "score": 50},
        {"key": 2, "name": u"吵闹", "score": 0}
    ]
    FIELDS_WET_SHOW = {
        0: u"干燥",
        1: u"潮湿"
    }
    FIELDS_WET_SHOW_4_ALL = [
        {"key": 0, "name": u"干燥", "score": 100},
        {"key": 1, "name": u"潮湿", "score": 50}
    ]
    FIELDS_CHILD_SHOW = {
        0: u"不宜带孩子",
        1: u"适合带孩子",
    }
    FIELDS_CHILD_SHOW_4_ALL = [
        {"key": 0, "name": u"不宜带孩子", "score": 0},
        {"key": 1, "name": u"适合带孩子", "score": 100}
    ]
    FIELDS_PEOPLE_SHOW = {
        0: u"人少",
        1: u"人较多",
        2: u"人多"
    }
    FIELDS_PEOPLE_SHOW_4_ALL = [
        {"key": 0, "name": u"人少", "score": 0},
        {"key": 1, "name": u"人较多", "score": 0},
        {"key": 2, "name": u"人多", "score": 0}
    ]
    FIELDS_EQUIPMENT_NEW_SHOW = {
        0: u"旧器材",
        1: u"新器材",
    }
    FIELDS_EQUIPMENT_NEW_SHOW_4_ALL = [
        {"key": 0, "name": u"旧器材", "score": 0},
        {"key": 1, "name": u"新器材", "score": 100}
    ]
    FIELDS_WATER_SHOW = {
        0: u"无泳池",
        1: u"泳池干净",
        2: u"泳池脏"
    }
    FIELDS_WATER_SHOW_4_ALL = [
        {"key": 0, "name": u"无泳池", "score": 50},
        {"key": 1, "name": u"泳池干净", "score": 100},
        {"key": 2, "name": u"泳池脏", "score": 0}
    ]
    FIELDS_SOUNDPROOF_SHOW = {
        0: u"不隔音",
        1: u"隔音"
    }
    FIELDS_SOUNDPROOF_SHOW_4_ALL = [
        {"key": 0, "name": u"不隔音", "score": 0},
        {"key": 1, "name": u"隔音", "score": 100}
    ]
    FIELDS_LIGHT_SHOW = {
        0: u"光线暗",
        1: u"光线合适",
        2: u"光线明亮"
    }
    FIELDS_LIGHT_SHOW_4_ALL = [
        {"key": 0, "name": u"光线暗", "score": 50},
        {"key": 1, "name": u"光线合适", "score": 100},
        {"key": 2, "name": u"光线明亮", "score": 50}
    ]
    FIELDS_PET_SHOW = {
        0: u"不能带宠物",
        1: u"能带宠物"
    }
    FIELDS_PET_SHOW_4_ALL = [
        {"key": 0, "name": u"不能带宠物", "score": 100},
        {"key": 1, "name": u"能带宠物", "score": 0}
    ]
    ANSWERS_SHOW = {
        FIELDS_TABLE_KEY: FIELDS_TABLE_SHOW,
        FIELDS_FLOOR_KEY: FIELDS_FLOOR_SHOW,
        FIELDS_TABLEWARE_KEY: FIELDS_TABLEWARE_SHOW,
        FIELDS_BEDSHEET_KEY: FIELDS_BEDSHEET_SHOW,
        FIELDS_EQUIPMENT_CLEAN_KEY: FIELDS_EQUIPMENT_CLEAN_SHOW,
        FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_SHOW,
        FIELDS_LAMPBLACK_KEY: FIELDS_LAMPBLACK_SHOW,
        FIELDS_TOILET_KEY: FIELDS_TOILET_SHOW,
        FIELDS_SMELL_KEY: FIELDS_SMELL_SHOW,
        FIELDS_WATCHMAN_KEY: FIELDS_WATCHMAN_SHOW,
        FIELDS_PEOPLE_KEY: FIELDS_PEOPLE_SHOW,
        FIELDS_QUIET_KEY: FIELDS_QUIET_SHOW,
        FIELDS_CHILD_KEY: FIELDS_CHILD_SHOW,
        FIELDS_EQUIPMENT_NEW_KEY: FIELDS_EQUIPMENT_NEW_SHOW,
        FIELDS_AREA_KEY: FIELDS_AREA_SHOW,
        FIELDS_WATER_KEY: FIELDS_WATER_SHOW,
        FIELDS_LIGHT_KEY: FIELDS_LIGHT_SHOW,
        FIELDS_SOUNDPROOF_KEY: FIELDS_SOUNDPROOF_SHOW,
        FIELDS_WET_KEY: FIELDS_WET_SHOW,
        FIELDS_PET_KEY: FIELDS_PET_SHOW,
    }

    FIELDS_TABLE_ITEM = {
        'key': FIELDS_TABLE_KEY,
        'name': FIELDS_TABLE_VALUE,
        'items': CLEAN_ITEMS_4_ALL,
        'show': FIELDS_TABLE_SHOW_4_ALL,
    }
    FIELDS_FLOOR_ITEM = {
        'key': FIELDS_FLOOR_KEY,
        'name': FIELDS_FLOOR_VALUE,
        'items': CLEAN_ITEMS_4_ALL,
        'show': FIELDS_FLOOR_SHOW_4_ALL,
    }
    FIELDS_TABLEWARE_ITEM = {
        'key': FIELDS_TABLEWARE_KEY,
        'name': FIELDS_TABLEWARE_VALUE,
        'items': CLEAN_ITEMS_4_ALL,
        'show': FIELDS_TABLEWARE_SHOW_4_ALL,
    }
    FIELDS_BEDSHEET_ITEM = {
        'key': FIELDS_BEDSHEET_KEY,
        'name': FIELDS_BEDSHEET_VALUE,
        'items': CLEAN_ITEMS_4_ALL,
        'show': FIELDS_BEDSHEET_SHOW_4_ALL,
    }
    FIELDS_EQUIPMENT_CLEAN_ITEM = {
        'key': FIELDS_EQUIPMENT_CLEAN_KEY,
        'name': FIELDS_EQUIPMENT_CLEAN_VALUE,
        'items': CLEAN_ITEMS_4_ALL,
        'show': FIELDS_EQUIPMENT_CLEAN_SHOW_4_ALL,
    }
    FIELDS_HAS_MONITOR_ITEM = {
        'key': FIELDS_HAS_MONITOR_KEY,
        'name': FIELDS_HAS_MONITOR_VALUE,
        'items': HAS_ITEMS_4_ALL,
        'show': FIELDS_HAS_MONITOR_SHOW_4_ALL,
    }
    FIELDS_LAMPBLACK_ITEM = {
        'key': FIELDS_LAMPBLACK_KEY,
        'name': FIELDS_LAMPBLACK_VALUE,
        'items': HAS_ITEMS_4_ALL,
        'show': FIELDS_LAMPBLACK_SHOW_4_ALL,
    }
    FIELDS_TOILET_ITEM = {
        'key': FIELDS_TOILET_KEY,
        'name': FIELDS_TOILET_VALUE,
        'items': HAS_ITEMS_4_ALL,
        'show': FIELDS_TOILET_SHOW_4_ALL,
    }
    FIELDS_SMELL_ITEM = {
        'key': FIELDS_SMELL_KEY,
        'name': FIELDS_SMELL_VALUE,
        'items': HAS_ITEMS_4_ALL,
        'show': FIELDS_SMELL_SHOW_4_ALL,
    }
    FIELDS_WATCHMAN_ITEM = {
        'key': FIELDS_WATCHMAN_KEY,
        'name': FIELDS_WATCHMAN_VALUE,
        'items': HAS_ITEMS_4_ALL,
        'show': FIELDS_WATCHMAN_SHOW_4_ALL,
    }
    FIELDS_PEOPLE_ITEM = {
        'key': FIELDS_PEOPLE_KEY,
        'name': FIELDS_PEOPLE_VALUE,
        'items': CROWD_ITEMS_4_ALL,
        'show': FIELDS_PEOPLE_SHOW_4_ALL,
    }
    FIELDS_QUIET_ITEM = {
        'key': FIELDS_QUIET_KEY,
        'name': FIELDS_QUIET_VALUE,
        'items': QUIET_ITEMS_4_ALL,
        'show': FIELDS_QUIET_SHOW_4_ALL,
    }
    FIELDS_CHILD_ITEM = {
        'key': FIELDS_CHILD_KEY,
        'name': FIELDS_CHILD_VALUE,
        'items': ADAPT_ITEMS_4_ALL,
        'show': FIELDS_CHILD_SHOW_4_ALL,
    }
    FIELDS_EQUIPMENT_NEW_ITEM = {
        'key': FIELDS_EQUIPMENT_NEW_KEY,
        'name': FIELDS_EQUIPMENT_NEW_VALUE,
        'items': NEW_ITEMS_4_ALL,
        'show': FIELDS_EQUIPMENT_NEW_SHOW_4_ALL,
    }
    FIELDS_AREA_ITEM = {
        'key': FIELDS_AREA_KEY,
        'name': FIELDS_AREA_VALUE,
        'items': ROOMY_ITEMS_4_ALL,
        'show': FIELDS_AREA_SHOW_4_ALL,
    }
    FIELDS_WATER_ITEM = {
        'key': FIELDS_WATER_KEY,
        'name': FIELDS_WATER_VALUE,
        'items': WATER_ITEMS_4_ALL,
        'show': FIELDS_WATER_SHOW_4_ALL,
    }
    FIELDS_LIGHT_ITEM = {
        'key': FIELDS_LIGHT_KEY,
        'name': FIELDS_LIGHT_VALUE,
        'items': LIGHT_ITEMS_4_ALL,
        'show': FIELDS_LIGHT_SHOW_4_ALL,
    }
    FIELDS_SOUNDPROOF_ITEM = {
        'key': FIELDS_SOUNDPROOF_KEY,
        'name': FIELDS_SOUNDPROOF_VALUE,
        'items': GOOD_ITEMS_4_ALL,
        'show': FIELDS_SOUNDPROOF_SHOW_4_ALL,
    }
    FIELDS_WET_ITEM = {
        'key': FIELDS_WET_KEY,
        'name': FIELDS_WET_VALUE,
        'items': WET_ITEMS_4_ALL,
        'show': FIELDS_WET_SHOW_4_ALL,
    }
    FIELDS_PET_ITEM = {
        'key': FIELDS_PET_KEY,
        'name': FIELDS_PET_VALUE,
        'items': CAN_ITEMS_4_ALL,
        'show': FIELDS_PET_SHOW_4_ALL,
    }

    PERCENTAGE_ZERO = {"percentage": 0}
    PERCENTAGE_TEN = {"percentage": 10}
    PERCENTAGE_FIFTEEN = {"percentage": 15}
    PERCENTAGE_TWENTY = {"percentage": 20}
    PERCENTAGE_TWENTY_FIVE = {"percentage": 25}
    PERCENTAGE_THIRTY = {"percentage": 30}
    PERCENTAGE_THIRTY_FIVE = {"percentage": 35}
    PERCENTAGE_FIFTY = {"percentage": 50}

    CATEGORY_FOOD_KEY = "2"  # food
    CATEGORY_SPORT_KEY = "3"  # sport
    CATEGORY_HOTEL_KEY = "4"  # hotel
    CATEGORY_BEAUTY_KEY = "5"  # beauty
    CATEGORY_ENTERTAINMENT_KEY = "6"  # entertainment
    CATEGORY_PATERNITY_KEY = "7"  # paternity
    CATEGORY_OTHER_KEY = "8"  # other
    CATEGORY_HOME_KEY = "10"  # home
    CATEGORY_COMPANY_KEY = "9"  # company
    CATEGORY_SHOP_KEY = "130"  # shop
    CATEGORY_TEST_KEY = "133"  # shop

    CATEGORY_FOOD_VALUE = u"美食"
    CATEGORY_SPORT_VALUE = u"运动"
    CATEGORY_HOTEL_VALUE = u"酒店"
    CATEGORY_BEAUTY_VALUE = u"丽人"
    CATEGORY_ENTERTAINMENT_VALUE = u'娱乐'
    CATEGORY_PATERNITY_VALUE = u'亲子'
    CATEGORY_OTHER_VALUE = u'其他'
    CATEGORY_HOME_VALUE = u"家"
    CATEGORY_COMPANY_VALUE = u"公司"
    CATEGORY_SHOP_VALUE = u"购物"
    # CATEGORY_TEST_VALUE = u"可可"

    CATEGORIES = {
        CATEGORY_HOME_KEY: CATEGORY_HOME_VALUE,
        CATEGORY_COMPANY_KEY: CATEGORY_COMPANY_VALUE,
        CATEGORY_FOOD_KEY: CATEGORY_FOOD_VALUE,
        CATEGORY_SPORT_KEY: CATEGORY_SPORT_VALUE,
        CATEGORY_HOTEL_KEY: CATEGORY_HOTEL_VALUE,
        CATEGORY_BEAUTY_KEY: CATEGORY_BEAUTY_VALUE,
        CATEGORY_ENTERTAINMENT_KEY: CATEGORY_ENTERTAINMENT_VALUE,
        CATEGORY_PATERNITY_KEY: CATEGORY_PATERNITY_VALUE,
        CATEGORY_OTHER_KEY: CATEGORY_OTHER_VALUE,
        CATEGORY_SHOP_KEY: CATEGORY_SHOP_VALUE,
        # CATEGORY_TEST_KEY: CATEGORY_TEST_VALUE,
    }

    CATEGORY_HOME_ITEMS = {}
    # CATEGORY_TEST_ITEM = {
    #     'key': CATEGORY_TEST_KEY,
    #     'name': CATEGORY_TEST_VALUE,
    #     'items': []
    # }
    CATEGORY_HOME_ITEMS_4_ALL = []
    CATEGORY_HOME_ITEM = {
        'key': CATEGORY_HOME_KEY,
        'name': CATEGORY_HOME_VALUE,
        'items': CATEGORY_HOME_ITEMS_4_ALL,
    }
    CATEGORY_COMPANY_ITEMS = {}
    CATEGORY_COMPANY_ITEMS_4_ALL = []
    CATEGORY_COMPANY_ITEM = {
        'key': CATEGORY_COMPANY_KEY,
        'name': CATEGORY_COMPANY_VALUE,
        'items': CATEGORY_COMPANY_ITEMS_4_ALL,
    }
    CATEGORY_FOOD_ITEMS = {
        # FIELDS_TABLE_KEY: FIELDS_TABLE_VALUE,
        FIELDS_FLOOR_KEY: FIELDS_FLOOR_VALUE,
        FIELDS_TABLEWARE_KEY: FIELDS_TABLEWARE_VALUE,
        # FIELDS_PEOPLE_KEY: FIELDS_PEOPLE_VALUE,
        # FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
        FIELDS_CHILD_KEY: FIELDS_CHILD_VALUE,
        # FIELDS_LAMPBLACK_KEY: FIELDS_LAMPBLACK_VALUE,
        # FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_VALUE,
    }

    CATEGORY_FOOD_ITEMS_4_ALL = [
        # dict(FIELDS_TABLE_ITEM.items() + PERCENTAGE_FIFTEEN.items()),
        dict(FIELDS_FLOOR_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
        dict(FIELDS_TABLEWARE_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
        # dict(FIELDS_PEOPLE_ITEM.items() + PERCENTAGE_ZERO.items()),
        # dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_TEN.items()),
        dict(FIELDS_CHILD_ITEM.items() + PERCENTAGE_THIRTY.items()),
        # dict(FIELDS_LAMPBLACK_ITEM.items() + PERCENTAGE_TEN.items()),
        # dict(FIELDS_HAS_MONITOR_ITEM.items() + PERCENTAGE_TEN.items())
    ]

    CATEGORY_FOOD_ITEM = {
        'key': CATEGORY_FOOD_KEY,
        'name': CATEGORY_FOOD_VALUE,
        'items': CATEGORY_FOOD_ITEMS_4_ALL,
    }

    CATEGORY_SPORT_ITEMS = {
        FIELDS_SMELL_KEY: FIELDS_SMELL_VALUE,
        # FIELDS_FLOOR_KEY: FIELDS_FLOOR_VALUE,
        FIELDS_EQUIPMENT_NEW_KEY: FIELDS_EQUIPMENT_NEW_VALUE,
        # FIELDS_AREA_KEY: FIELDS_AREA_VALUE,
        # FIELDS_PEOPLE_KEY: FIELDS_PEOPLE_VALUE,
        FIELDS_WATER_KEY: FIELDS_WATER_VALUE,
        # FIELDS_LIGHT_KEY: FIELDS_LIGHT_VALUE,
        # FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_VALUE,
    }

    CATEGORY_SPORT_ITEMS_4_ALL = [
        dict(FIELDS_SMELL_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
        # dict(FIELDS_FLOOR_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
        dict(FIELDS_EQUIPMENT_NEW_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
        # dict(FIELDS_AREA_ITEM.items() + PERCENTAGE_FIFTEEN.items()),
        # dict(FIELDS_PEOPLE_ITEM.items() + PERCENTAGE_ZERO.items()),
        dict(FIELDS_WATER_ITEM.items() + PERCENTAGE_THIRTY.items()),
        # dict(FIELDS_LIGHT_ITEM.items() + PERCENTAGE_FIFTEEN.items()),
        # dict(FIELDS_HAS_MONITOR_ITEM.items() + PERCENTAGE_TWENTY.items()),
    ]

    CATEGORY_SPORT_ITEM = {
        'key': CATEGORY_SPORT_KEY,
        'name': CATEGORY_SPORT_VALUE,
        'items': CATEGORY_SPORT_ITEMS_4_ALL,
    }

    CATEGORY_HOTEL_ITEMS = {
        # FIELDS_CHILD_KEY: FIELDS_CHILD_VALUE,
        # FIELDS_BEDSHEET_KEY: FIELDS_BEDSHEET_VALUE,
        FIELDS_SOUNDPROOF_KEY: FIELDS_SOUNDPROOF_VALUE,
        # FIELDS_WET_KEY: FIELDS_WET_VALUE,
        FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
        FIELDS_FLOOR_KEY: FIELDS_FLOOR_VALUE,
    }
    CATEGORY_HOTEL_ITEMS_4_ALL = [
        # dict(FIELDS_CHILD_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        # dict(FIELDS_BEDSHEET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        dict(FIELDS_SOUNDPROOF_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
        # dict(FIELDS_WET_ITEM.items() + PERCENTAGE_TEN.items()),
        dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_THIRTY.items()),
        dict(FIELDS_FLOOR_ITEM.items() + PERCENTAGE_THIRTY_FIVE.items()),
    ]
    CATEGORY_HOTEL_ITEM = {
        'key': CATEGORY_HOTEL_KEY,
        'name': CATEGORY_HOTEL_VALUE,
        'items': CATEGORY_HOTEL_ITEMS_4_ALL,
    }

    CATEGORY_BEAUTY_ITEMS = {
        # FIELDS_EQUIPMENT_CLEAN_KEY: FIELDS_EQUIPMENT_CLEAN_VALUE,
        FIELDS_FLOOR_KEY: FIELDS_FLOOR_VALUE,
        FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
    }
    CATEGORY_BEAUTY_ITEMS_4_ALL = [
        # dict(FIELDS_EQUIPMENT_CLEAN_ITEM.items() + PERCENTAGE_FIFTY.items()),
        dict(FIELDS_FLOOR_ITEM.items() + PERCENTAGE_FIFTY.items()),
        dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_FIFTY.items())
    ]
    CATEGORY_BEAUTY_ITEM = {
        'key': CATEGORY_BEAUTY_KEY,
        'name': CATEGORY_BEAUTY_VALUE,
        'items': CATEGORY_BEAUTY_ITEMS_4_ALL,
    }
    CATEGORY_ENTERTAINMENT_ITEMS = {
        # FIELDS_TOILET_KEY: FIELDS_TOILET_VALUE,
        FIELDS_CHILD_KEY: FIELDS_CHILD_VALUE,
        FIELDS_FLOOR_KEY: FIELDS_FLOOR_VALUE,
        # FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
        # FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_VALUE,
    }
    CATEGORY_ENTERTAINMENT_ITEMS_4_ALL = [
        # dict(FIELDS_TOILET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        dict(FIELDS_CHILD_ITEM.items() + PERCENTAGE_FIFTY.items()),
        dict(FIELDS_FLOOR_ITEM.items() + PERCENTAGE_FIFTY.items()),
        # dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        # dict(FIELDS_HAS_MONITOR_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items())
    ]
    CATEGORY_ENTERTAINMENT_ITEM = {
        'key': CATEGORY_ENTERTAINMENT_KEY,
        'name': CATEGORY_ENTERTAINMENT_VALUE,
        'items': CATEGORY_ENTERTAINMENT_ITEMS_4_ALL,
    }
    CATEGORY_SHOP_ITEMS = {
        # FIELDS_TOILET_KEY: FIELDS_TOILET_VALUE,
        FIELDS_CHILD_KEY: FIELDS_CHILD_VALUE,
        FIELDS_FLOOR_KEY: FIELDS_FLOOR_VALUE,
        # FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
        # FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_VALUE,
    }
    CATEGORY_SHOP_ITEMS_4_ALL = [
        # dict(FIELDS_TOILET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        dict(FIELDS_CHILD_ITEM.items() + PERCENTAGE_FIFTY.items()),
        dict(FIELDS_FLOOR_ITEM.items() + PERCENTAGE_FIFTY.items()),
        # dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        # dict(FIELDS_HAS_MONITOR_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items())
    ]
    CATEGORY_SHOP_ITEM = {
        'key': CATEGORY_SHOP_KEY,
        'name': CATEGORY_SHOP_VALUE,
        'items': CATEGORY_SHOP_ITEMS_4_ALL,
    }
    CATEGORY_PATERNITY_ITEMS = {
        # FIELDS_PEOPLE_KEY: FIELDS_PEOPLE_VALUE,
        # FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
        # FIELDS_SMELL_KEY: FIELDS_SMELL_VALUE,
        FIELDS_WATCHMAN_KEY: FIELDS_WATCHMAN_VALUE,
        FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_VALUE,
    }

    CATEGORY_PATERNITY_ITEMS_4_ALL = [
        # dict(FIELDS_PEOPLE_ITEM.items() + PERCENTAGE_ZERO.items()),
        # dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        # dict(FIELDS_SMELL_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        dict(FIELDS_WATCHMAN_ITEM.items() + PERCENTAGE_FIFTY.items()),
        dict(FIELDS_HAS_MONITOR_ITEM.items() + PERCENTAGE_FIFTY.items())
    ]
    CATEGORY_PATERNITY_ITEM = {
        'key': CATEGORY_PATERNITY_KEY,
        'name': CATEGORY_PATERNITY_VALUE,
        'items': CATEGORY_PATERNITY_ITEMS_4_ALL,
    }
    CATEGORY_OTHER_ITEMS = {
        FIELDS_PET_KEY: FIELDS_PET_VALUE,
        # FIELDS_QUIET_KEY: FIELDS_QUIET_VALUE,
        # FIELDS_SMELL_KEY: FIELDS_SMELL_VALUE,
        FIELDS_CHILD_KEY: FIELDS_CHILD_VALUE,
        # FIELDS_PEOPLE_KEY: FIELDS_PEOPLE_VALUE,
        # FIELDS_HAS_MONITOR_KEY: FIELDS_HAS_MONITOR_VALUE,
    }
    CATEGORY_OTHER_ITEMS_4_ALL = [
        dict(FIELDS_PET_ITEM.items() + PERCENTAGE_FIFTY.items()),
        # dict(FIELDS_QUIET_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        # dict(FIELDS_SMELL_ITEM.items() + PERCENTAGE_TWENTY_FIVE.items()),
        dict(FIELDS_CHILD_ITEM.items() + PERCENTAGE_FIFTY.items()),
        # dict(FIELDS_PEOPLE_ITEM.items() + PERCENTAGE_ZERO.items()),
        # dict(FIELDS_HAS_MONITOR_ITEM.items() + PERCENTAGE_FIFTEEN.items())
    ]
    CATEGORY_OTHER_ITEM = {
        'key': CATEGORY_OTHER_KEY,
        'name': CATEGORY_OTHER_VALUE,
        'items': CATEGORY_OTHER_ITEMS_4_ALL,
    }

    CATEGORY_SHOW = {
        CATEGORY_HOME_KEY: CATEGORY_HOME_ITEMS,
        CATEGORY_COMPANY_KEY: CATEGORY_COMPANY_ITEMS,
        CATEGORY_FOOD_KEY: CATEGORY_FOOD_ITEMS,
        CATEGORY_SPORT_KEY: CATEGORY_SPORT_ITEMS,
        CATEGORY_HOTEL_KEY: CATEGORY_HOTEL_ITEMS,
        CATEGORY_BEAUTY_KEY: CATEGORY_BEAUTY_ITEMS,
        CATEGORY_ENTERTAINMENT_KEY: CATEGORY_ENTERTAINMENT_ITEMS,
        CATEGORY_SHOP_KEY: CATEGORY_SHOP_ITEMS,
        CATEGORY_PATERNITY_KEY: CATEGORY_PATERNITY_ITEMS,
        CATEGORY_OTHER_KEY: CATEGORY_OTHER_ITEMS
    }

    PUBLISH_CONSTANT_ALL = [
        CATEGORY_HOME_ITEM, CATEGORY_COMPANY_ITEM, CATEGORY_FOOD_ITEM,
        CATEGORY_SPORT_ITEM, CATEGORY_HOTEL_ITEM, CATEGORY_BEAUTY_ITEM,
        CATEGORY_ENTERTAINMENT_ITEM, CATEGORY_PATERNITY_ITEM, CATEGORY_OTHER_ITEM,
        CATEGORY_SHOP_ITEM,
        # CATEGORY_TEST_ITEM,
    ]

    WIN_KEY = 1
    WIN_VALUE = u"赞"
    LOST_KEY = 0
    LOST_VALUE = u"踩"
    WIN_ITEM = {
        "key": WIN_KEY,
        "name": WIN_VALUE
    }
    LOST_ITEM = {
        "key": LOST_KEY,
        "name": LOST_VALUE
    }
    ATTRIBUTES = [
        WIN_ITEM, LOST_ITEM
    ]

    RECOMMENDED_KEY = 1
    RECOMMENDED_VALUE = u"推荐"
    NOT_RECOMMENDED_KEY = 0
    NOT_RECOMMENDED_VALUE = u"不推荐"
    RECOMMENDED_ITEM = {
        "key": RECOMMENDED_KEY,
        "name": RECOMMENDED_VALUE
    }
    NOT_RECOMMENDED_ITEM = {
        "key": NOT_RECOMMENDED_KEY,
        "name": NOT_RECOMMENDED_VALUE
    }
    RECOMMENDED_ATTRIBUTES = [
        RECOMMENDED_ITEM, NOT_RECOMMENDED_ITEM
    ]
    CATEGORY_DEFAULT_ICON = {
        CATEGORY_HOME_KEY: BASE_URL_4_SHOP + "home.png",
        CATEGORY_COMPANY_KEY: BASE_URL_4_SHOP + "company.png",
        CATEGORY_BEAUTY_KEY: BASE_URL_4_SHOP + "beauty.png",
        CATEGORY_HOTEL_KEY: BASE_URL_4_SHOP + "hotel.png",
        CATEGORY_ENTERTAINMENT_KEY: BASE_URL_4_SHOP + "entertainment.png",
        CATEGORY_FOOD_KEY: BASE_URL_4_SHOP + "food.png",
        CATEGORY_OTHER_KEY: BASE_URL_4_SHOP + "other.png",
        CATEGORY_PATERNITY_KEY: BASE_URL_4_SHOP + "paternity.png",
        CATEGORY_SPORT_KEY: BASE_URL_4_SHOP + "sport.png",
    }


class DeviceIcon():
    BASE_URL_4_DEVICE = BASE_URL_4_IMAGE + "device/"
    ZERO_ICON_NAME = "5eeaf22249e38a6138e9a095b721336a.jpeg"
    ZERO_ICON_NICKNAME = u"零号"
    ONE_ICON_NAME = "5eeaf22249e38a6138e9a095b721336a.jpeg"
    ONE_ICON_NICKNAME = u"壹号"
    TWO_ICON_NAME = "09be2efb0ba282fa4ddc47560691a835.jpeg"
    TWO_ICON_NICKNAME = u"贰号"
    THREE_ICON_NAME = "257ee790ba886e5136d6f29fdef905fc.jpeg"
    THREE_ICON_NICKNAME = u"叁号"
    FOUR_ICON_NAME = "3616e573732c1566a58da41172d5bfc7.jpeg"
    FOUR_ICON_NICKNAME = u"肆号"
    FIVE_ICON_NAME = "7917801135dd4d777a6490ff024fb550.jpeg"
    FIVE_ICON_NICKNAME = u"伍号"
    SIX_ICON_NAME = "b4480837e01833d9d9e5db2025952407.jpeg"
    SIX_ICON_NICKNAME = u"陆号"
    SEVEN_ICON_NAME = "c3e8f7b4b2d1453cedb51dbe02652416.jpeg"
    SEVEN_ICON_NICKNAME = u"染号"
    EIGHT_ICON_NAME = "d6b20bc1adc115ccd4828e35209adecd.jpeg"
    EIGHT_ICON_NICKNAME = u"捌号"
    NINE_ICON_NAME = "e8d6baa1a190388366ca1d895d79dd68.jpeg"
    NINE_ICON_NICKNAME = u"玖号"
    TEN_ICON_NAME = "f0db22928081831e94a760c187ac2696.jpeg"
    TEN_ICON_NICKNAME = u"拾号"
    ELEVEN_ICON_NAME = "lvbu.jpg"
    ELEVEN_ICON_NICKNAME = u"吕布"
    TWELVE_ICON_NAME = "xuchu.jpg"
    TWELVE_ICON_NICKNAME = u"许杵"
    THIRTEEN_ICON_NAME = "xiahouchun.jpg"
    THIRTEEN_ICON_NICKNAME = u"夏侯惇"
    FOURTEEN_ICON_NAME = "daqiao.jpg"
    FOURTEEN_ICON_NICKNAME = u"大乔"
    FIFTEEN_ICON_NAME = "lvmeng.jpg"
    FIFTEEN_ICON_NICKNAME = u"吕蒙"
    SIXTEEN_ICON_NAME = "sunshangxiang.jpg"
    SIXTEEN_ICON_NICKNAME = u"孙尚香"
    SEVENTEEN_ICON_NAME = "yangxiu.jpg"
    SEVENTEEN_ICON_NICKNAME = u"杨修"

    ICONS = [
        {"key": 0, "url": BASE_URL_4_DEVICE + ZERO_ICON_NAME, 'name': ZERO_ICON_NAME, 'nickname': ZERO_ICON_NICKNAME},
        {"key": 1, "url": BASE_URL_4_DEVICE + ONE_ICON_NAME, 'name': ONE_ICON_NAME, 'nickname': ONE_ICON_NICKNAME},
        {"key": 2, "url": BASE_URL_4_DEVICE + TWO_ICON_NAME, 'name': TWO_ICON_NAME, 'nickname': TWO_ICON_NICKNAME},
        {"key": 3, "url": BASE_URL_4_DEVICE + THREE_ICON_NAME, 'name': THREE_ICON_NAME,
         'nickname': THREE_ICON_NICKNAME},
        {"key": 4, "url": BASE_URL_4_DEVICE + FOUR_ICON_NAME, 'name': FOUR_ICON_NAME, 'nickname': FOUR_ICON_NICKNAME},
        {"key": 5, "url": BASE_URL_4_DEVICE + FIVE_ICON_NAME, 'name': FIVE_ICON_NAME, 'nickname': FIVE_ICON_NICKNAME},
        {"key": 6, "url": BASE_URL_4_DEVICE + SIX_ICON_NAME, 'name': SIX_ICON_NAME, 'nickname': SIX_ICON_NICKNAME},
        {"key": 7, "url": BASE_URL_4_DEVICE + SEVEN_ICON_NAME, 'name': SEVEN_ICON_NAME,
         'nickname': SEVEN_ICON_NICKNAME},
        {"key": 8, "url": BASE_URL_4_DEVICE + EIGHT_ICON_NAME, 'name': EIGHT_ICON_NAME,
         'nickname': EIGHT_ICON_NICKNAME},
        {"key": 9, "url": BASE_URL_4_DEVICE + NINE_ICON_NAME, 'name': NINE_ICON_NAME, 'nickname': NINE_ICON_NICKNAME},
        {"key": 10, "url": BASE_URL_4_DEVICE + TEN_ICON_NAME, 'name': TEN_ICON_NAME, 'nickname': TEN_ICON_NICKNAME},
        {"key": 11, "url": BASE_URL_4_DEVICE + ELEVEN_ICON_NAME, 'name': ELEVEN_ICON_NAME,
         'nickname': ELEVEN_ICON_NICKNAME},
        {"key": 12, "url": BASE_URL_4_DEVICE + TWELVE_ICON_NAME, 'name': TWELVE_ICON_NAME,
         'nickname': TWELVE_ICON_NICKNAME},
        {"key": 13, "url": BASE_URL_4_DEVICE + THIRTEEN_ICON_NAME, 'name': THIRTEEN_ICON_NAME,
         'nickname': THIRTEEN_ICON_NICKNAME},
        {"key": 14, "url": BASE_URL_4_DEVICE + FOURTEEN_ICON_NAME, 'name': FOURTEEN_ICON_NAME,
         'nickname': FOURTEEN_ICON_NICKNAME},
        {"key": 15, "url": BASE_URL_4_DEVICE + FIFTEEN_ICON_NAME, 'name': FIFTEEN_ICON_NAME,
         'nickname': FIFTEEN_ICON_NICKNAME},
        {"key": 16, "url": BASE_URL_4_DEVICE + SIXTEEN_ICON_NAME, 'name': SIXTEEN_ICON_NAME,
         'nickname': SIXTEEN_ICON_NICKNAME},
        {"key": 17, "url": BASE_URL_4_DEVICE + SEVENTEEN_ICON_NAME, 'name': SEVENTEEN_ICON_NAME,
         'nickname': SEVENTEEN_ICON_NICKNAME},
    ]


DEVICE_BRAND = [
    {"key": 0, "name": u"三个爸爸"},
]

USER_DEFAULT_ICON = BASE_URL_4_IMAGE + "user/default.jpg"
USER_DEFAULT_MALE_ICON = BASE_URL_4_IMAGE + "user/default_male.png"
USER_DEFAULT_FEMALE_ICON = BASE_URL_4_IMAGE + "user/default_female.png"
PUBLISH_DEFAULT_ICON = BASE_URL_4_IMAGE + "publish/titleimg_UoP0jBx.jpg"
SHOP_DEFAULT_ICON = BASE_URL_4_IMAGE + "shop/default/default.png"
FORUM_CATEGORY_DEFAULT_ICON = BASE_URL_4_IMAGE + "forum/default.jpg"

DIANPING_RATING = [
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_0_0star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_0_5star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_1_0star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_1_5star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_2_0star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_2_5star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_3_0star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_3_5star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_4_0star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_4_5star.png",
    BASE_URL_4_IMAGE + "dianping_rating/rating_img_s_5_0star.png"
]
