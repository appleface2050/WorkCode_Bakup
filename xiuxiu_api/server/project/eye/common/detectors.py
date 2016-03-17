# -*- coding: utf-8 -*-

from ..models import Detector
from ..models import DetectorRelation
from objects import Objects
from logs import Logs
from operations import Operations
from weathers import Weathers
from datetimes import Datetimes
from characters import Characters


class Detectors(object):
    @staticmethod
    def get_none():
        return DetectorRelation.objects.none()

    @staticmethod
    def add_detector_relations(detector_relation):
        try:
            user_id = detector_relation.get("user_id")
            mac_address = detector_relation.get("mac_address")
            city = detector_relation.get("city")
            address = detector_relation.get("address")
            shop_id = detector_relation.get("shop_id")
            threshold = detector_relation.get("threshold")
            dr = DetectorRelation(
                user_id=user_id,
                mac_address=mac_address,
                city=city,
                address=address,
                shop_id=shop_id,
                threshold=threshold
            )
            dr.save()
            return dr
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Detectors.get_none()

    @staticmethod
    def update_detector_relation(dr, detector_relation):
        try:
            # Logs.print_log("detector relation", detector_relation)
            # Logs.print_log("update_detector_relation", "start")
            dr.user_id = int(detector_relation.get("user_id"))
            # Logs.print_log("mac_address", detector_relation.get("mac_address"))
            dr.mac_address = detector_relation.get("mac_address")
            # Logs.print_log("city", detector_relation.get("city"))
            dr.city = detector_relation.get("city")
            # Logs.print_log("address", Characters.unicode_to_concrete(detector_relation.get("shop_address")))
            dr.address = detector_relation.get("shop_address")
            # Logs.print_log("shop_id", detector_relation.get("shop_id"))
            dr.shop_id = int(detector_relation.get("shop_id"))
            # Logs.print_log("threshold", detector_relation.get("threshold"))
            dr.threshold = int(detector_relation.get("threshold"))
            # Logs.print_log("user_id", detector_relation.get("user_id"))
            dr.save()
            return dr
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_info(detector):

        result = Objects.get_object_info(detector)
        result_weather = None
        try:
            relation = DetectorRelation.objects.exclude(state=False).get(mac_address=detector.mac_address)
            result["shop_address"] = relation.address
            # Logs.print_log("shop_address", relation.address)
            # if relation.shop:
            #     if relation.shop.dianping_business_id:
            #         result["shop_address"] = relation.shop.dianping_city + relation.shop.dianping_address
            #     else:
            #         if relation.shop.address:
            #             result["shop_address"] = relation.shop.address.detail_address
            #         else:
            #             result["shop_address"] = None
            # Logs.print_log("relation.city", relation.city)
            if relation.city:
                weather = Weathers.get_weather_from_db(relation.city)
                result["address"] = relation.address

                if not weather or Weathers.is_weather_in_db_timeout(weather):
                    weather = Weathers.get_weather_from_heweather(relation.city)
                    # Logs.print_log("weather", weather)
                    Weathers.create_weather(weather)
                    result_weather = weather
                else:
                    result_weather = Weathers.get_weather_info(weather)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result_weather = None
        if result_weather:
            for (key, value) in result_weather.items():
                result["weather_" + key] = value

        return result

    @staticmethod
    def get_latest(mac_address):
        latest_datetime = Datetimes.get_some_day(0, 60)
        latest_datetime = Datetimes.to_utc(latest_datetime)
        # Logs.print_log("latest_datetime", latest_datetime)
        detectors = Detector.objects.filter(mac_address=mac_address, created_at__gte=latest_datetime).order_by("-id")
        if detectors:
            return detectors[0]
        else:
            return Detectors.get_none()

    @staticmethod
    def get_latest_without_limit(mac_address):
        detectors = Detector.objects.filter(mac_address=mac_address).order_by("-id")
        if detectors:
            return detectors[0]
        else:
            return Detectors.get_none()

    @staticmethod
    def get_detector_relation_by_shop(shop_id):
        drs = DetectorRelation.objects.exclude(state=False).filter(shop_id=shop_id).order_by("-id").select_related(
            "user")
        if drs.exists():
            return drs
        else:
            return Detectors.get_none()

    @staticmethod
    def get_detector_relations_by_user(detector_relations, user_id):
        if detector_relations.exists():
            drs = detector_relations.exclude(state=False).filter(user_id=user_id).order_by("-id").select_related("user")
        else:
            drs = DetectorRelation.objects.exclude(state=False).filter(user_id=user_id).order_by("-id").select_related(
                "user")
        if drs:
            return drs
        else:
            return Detectors.get_none()

    @staticmethod
    def get_detector_relation_by_mac_address(detector_relations, mac_address):
        if detector_relations.exists():
            return detector_relations.exclude(state=False).filter(mac_address=mac_address).select_related("user")
        else:
            return DetectorRelation.objects.exclude(state=False).filter(mac_address=mac_address).select_related("user")

    @staticmethod
    def get_all_detector_relations():
        return DetectorRelation.objects.all()

    @staticmethod
    def get_all_detector_relations_by_user(detector_relations, user_id):
        if detector_relations.exists():
            drs = detector_relations.filter(user_id=user_id).order_by("-id").select_related("user").order_by("-state")
        else:
            drs = DetectorRelation.objects.filter(user_id=user_id).order_by("-id").select_related("user").order_by(
                "-state")
        if drs:
            return drs
        else:
            return Detectors.get_none()

    @staticmethod
    def get_all_detector_relation_by_mac_address(detector_relations, mac_address):
        if detector_relations.exists():
            return detector_relations.filter(mac_address=mac_address).select_related("user").order_by("-state")
        else:
            return DetectorRelation.objects.filter(mac_address=mac_address).select_related("user").order_by("-state")

    @staticmethod
    def get_detector_relation_info(detector_relation):
        return Objects.get_object_info(detector_relation)

    @staticmethod
    def deactivate_detector_relation(detector_relation):
        try:
            detector_relation.state = False
            detector_relation.save()
            return detector_relation
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def activate_detector_relation(detector_relation):
        try:
            detector_relation.state = True
            detector_relation.save()
            return detector_relation
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def get_sequence_unique():
        return Detector.objects.values("mac_address").distinct()

    @staticmethod
    def get(mac_address):
        try:
            return Detector.objects.filter(mac_address=mac_address).order_by("-id")
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Detectors.get_none()

    @staticmethod
    def get_one(detector):
        return Objects.get_object_info(detector)