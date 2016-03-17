# coding: utf-8

from https import Https
from datetimes import Datetimes
from logs import Logs
import json
from ..models import Weather
from objects import Objects

KEY = "ef55fb15d30a42feb76124dbf5b0c080"
URL = "https://api.heweather.com/x3/weather"
DATA_ROOT = "HeWeather data service 3.0"
DATA_ROOT_AQI = "aqi"
DATA_ROOT_AQI_CITY = "city"
DATA_ROOT_AQI_CITY_PM25 = "pm25"
DATA_ROOT_NOW = "now"
DATA_ROOT_NOW_TMP = "tmp"
DATA_ROOT_NOW_HUM = "hum"
DATA_ROOT_NOW_COND = "cond"
DATA_ROOT_NOW_COND_TXT = "txt"
DATA_ROOT_NOW_COND_CODE = "code"

THREE_HOUR_SECONDS = (3 + 8) * 3600


class Weathers(object):
    @staticmethod
    def get_none():
        return Objects.get_none(Weather)

    # 内部方法
    @staticmethod
    def get_all_from_heweather(city):
        url = URL + "?key=" + KEY + "&city=" + city
        return Https.http_get_action(url)

    @staticmethod
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
                Logs.print_current_function_name_and_line_number(ex)

            now = root[0][DATA_ROOT_NOW]
            result["temperature"] = now[DATA_ROOT_NOW_TMP]
            result["humidity"] = now[DATA_ROOT_NOW_HUM]
            result["condition"] = now[DATA_ROOT_NOW_COND][DATA_ROOT_NOW_COND_TXT]
            result["code"] = now[DATA_ROOT_NOW_COND][DATA_ROOT_NOW_COND_CODE]
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)

        return result

    @staticmethod
    def get_weather_from_heweather(city):
        data = Weathers.get_all_from_heweather(city)
        result = Weathers.parse_heweather(data)
        result["city"] = city
        return result

    @staticmethod
    def get_weather_from_db(city):
        try:
            weather = Weather.objects.filter(city=city)
            if weather.count() == 1:
                return weather[0]
            else:
                return Weathers.get_none()
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Weathers.get_none()

    @staticmethod
    def create_weather(data):
        weather = Weather(
            city=data["city"],
            pm2_5=data["pm2_5"],
            temperature=data["temperature"],
            humidity=data["humidity"],
            condition=unicode(data["condition"]),
            code=data["code"]
        )
        weather.save()

    @staticmethod
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

        humidity = data.get("humidity", 0)
        if humidity:
            weather.humidity = humidity

        weather.changed_at = Datetimes.get_now()
        weather.save()

    @staticmethod
    def is_weather_in_db_timeout(weather):
        now = Datetimes.get_now()
        dt = weather.changed_at
        dt = dt.replace(tzinfo=None)
        delta = Datetimes.get_delta_time(dt, now)

        if delta.days > 0 or delta.seconds >= THREE_HOUR_SECONDS:
            return True
        else:
            return False

    @staticmethod
    def get_weather_info(weather):
        try:
            return Objects.get_object_info(weather)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None
