# coding: utf-8

from common_functions import HttpOperations, DatetimeOperations
from log4stage import print_current_function_name_and_line_number
import json
from models import Weather

KEY = "ef55fb15d30a42feb76124dbf5b0c080"
URL = "https://api.heweather.com/x3/weather"
DATA_ROOT = "HeWeather data service 3.0"
DATA_ROOT_AQI = "aqi"
DATA_ROOT_AQI_CITY = "city"
DATA_ROOT_AQI_CITY_PM25 = "pm25"
DATA_ROOT_NOW = "now"
DATA_ROOT_NOW_TMP = "tmp"
DATA_ROOT_NOW_COND = "cond"
DATA_ROOT_NOW_COND_TXT = "txt"
DATA_ROOT_NOW_COND_CODE = "code"

THREE_HOUR_SECONDS = (3 + 8) * 3600


# 内部方法
def get_all_from_heweather(city):
    url = URL + "?key=" + KEY + "&city=" + city
    return HttpOperations.http_get_action(url)


def parse_heweather(data):
    result = dict()
    try:
        json_data = json.loads(data)
        root = json_data[DATA_ROOT]
        try:
            aqi = root[0][DATA_ROOT_AQI]
            result["pm2_5"] = aqi[DATA_ROOT_AQI_CITY][DATA_ROOT_AQI_CITY_PM25]
        except Exception as ex:
            result["pm2_5"] = -1
            print_current_function_name_and_line_number(ex)

        now = root[0][DATA_ROOT_NOW]
        result["temperature"] = now[DATA_ROOT_NOW_TMP]
        result["condition"] = now[DATA_ROOT_NOW_COND][DATA_ROOT_NOW_COND_TXT]
        result["code"] = now[DATA_ROOT_NOW_COND][DATA_ROOT_NOW_COND_CODE]
    except Exception as ex:
        print_current_function_name_and_line_number(ex)

    return result


def get_weather_from_heweather(city):
    data = get_all_from_heweather(city)
    result = parse_heweather(data)
    result["city"] = city
    return result


def get_weather_from_db(city):
    try:
        return Weather.objects.get(city=city)
    except Exception as ex:
        print_current_function_name_and_line_number(ex)
        return None


def create_weather(data):
    weather = Weather(
        city=data["city"],
        pm2_5=data["pm2_5"],
        temperature=data["temperature"],
        condition=unicode(data["condition"]),
        code=data["code"]
    )
    weather.save()


def update_weather(weather, data):
    pm2_5 = int(data.get("pm2_5", -1))
    if pm2_5 == -1:
        pass
    else:
        weather.pm2_5 = pm2_5

    temperature = float(data.get("temperature", -1))
    if temperature == -1:
        pass
    else:
        weather.temperature = temperature

    condition = data.get("condition", None)
    if condition:
        weather.condition = condition

    code = data.get("code", 0)
    if code:
        weather.code = code

    weather.changed_at = DatetimeOperations.get_today()
    weather.save()


def is_weather_in_db_timeout(weather):
    now = DatetimeOperations.get_today()
    dt = weather.changed_at
    dt = dt.replace(tzinfo=None)
    print now
    print dt
    delta = DatetimeOperations.get_delta_time(dt, now)
    print delta.days
    print delta.seconds
    if delta.days > 0 or delta.seconds >= THREE_HOUR_SECONDS:
        print "timeout"
        return True
    else:
        print "no timeout"
        return False
