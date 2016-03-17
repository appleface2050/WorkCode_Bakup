# -*- coding: utf-8 -*-

from ..models import Address
from logs import Logs
from objects import Objects
from geohash_code import GeohashCode


class Addresses(object):
    @staticmethod
    def get_geohash_code(latitude, longitude, length=6):
        gc = GeohashCode(latitude, longitude, length)
        return gc.get()

    @staticmethod
    def get_none():
        return Objects.get_none(Address)

    @staticmethod
    def get_all():
        return Address.objects.all()

    def __init__(self, longitude, latitude, detail_address="", name="", china_id=0):
        self._longitude = longitude
        self._latitude = latitude
        self._geohash_code = Addresses.get_geohash_code(latitude, longitude)
        self._detail_address = detail_address
        self._china_id = china_id
        self._name = name

    @property
    def longitude(self):
        return self._longitude

    @longitude.setter
    def longitude(self, value):
        if not isinstance(value, float):
            raise ValueError("longitude must be a float!")
        if value < 0:
            raise ValueError("longitude must be not less than 0")
        self._longitude = value
        self._geohash_code = Addresses.get_geohash_code(self._latitude, self.longitude)

    @property
    def latitude(self):
        return self._latitude

    @latitude.setter
    def latitude(self, value):
        if not isinstance(value, float):
            raise ValueError("latitude must be a float!")
        if value < 0:
            raise ValueError("latitude must be not less than 0!")
        self._latitude = value
        self._geohash_code = Addresses.get_geohash_code(self._latitude, self.longitude)

    @property
    def detail_address(self):
        return self._detail_address

    @detail_address.setter
    def detail_address(self, value):
        if not isinstance(value, str):
            raise ValueError("detail_address must be a string!")
        if len(value) > 254:
            raise ValueError("detail_address's length must be less than 254")
        self._detail_address = value

    @property
    def name(self):
        return self._name

    @name.setter
    def name(self, value):
        if not isinstance(value, str) and not isinstance(value, unicode):
            raise ValueError("name must be a string!")
        if len(value) > 254:
            raise ValueError("name's length must be less than 254")
        self._name = value

    @property
    def china_id(self):
        return self._china_id

    @china_id.setter
    def china_id(self, value):
        if not isinstance(value, int):
            raise ValueError("china_id must be an int!")
        if value <= 0:
            raise ValueError("china_id must be an int more than 0!")
        self._china_id = value

    # 增
    def add(self):
        try:
            address = Address(
                longitude=self._longitude,
                latitude=self._latitude,
                geohash_code=self._geohash_code,
                detail_address=self._detail_address,
            )
            if self._name:
                address.name = self._name
            if self._china_id:
                address.china_id = self._china_id
            address.save()
            return address
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Addresses.get_none()

    @staticmethod
    def delete(address):
        try:
            if not isinstance(address, Address):
                raise ValueError("object in addresses must be an Address!")
            address.audit = 0
            address.save()
            return address
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Addresses.get_none()

    @staticmethod
    def reset(address):
        try:
            if not isinstance(address, Address):
                raise ValueError("object in addresses must be an Address!")
            address.audit = 1
            address.save()
            return address
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Addresses.get_none()

    # 改
    def update(self, address):
        try:
            if not isinstance(address, Address):
                raise ValueError("address must be an Address!")
            address.longitude = self._longitude
            address.latitude = self._latitude
            address.geohash_code = self._geohash_code
            if self._name:
                address.name = self._name
            if self._detail_address:
                address.detail_address = self._detail_address
            if self._china_id:
                address.china_id = self._china_id
            address.save()
            return address
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Addresses.get_none()

    # 查
    def get(self):
        try:
            addresses = Addresses.get_all()
            valid_addresses = addresses.filter(audit=1)

            the_addresses = valid_addresses.filter(
                geohash_code__startswith=self._geohash_code,
                name__contains=self._name,
                detail_address__contains=self._detail_address,
            )
            if self._china_id:
                the_addresses = the_addresses.filter(china_id=self._china_id)
            return the_addresses
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Addresses.get_none()

    @staticmethod
    def static_update(address, address_dict):
        try:
            for (key, value) in address_dict.items():
                Objects.set_value(address, key, value)
            address.save()
            return address
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Address.get_none()

    @staticmethod
    def get_info(address):
        return Objects.get_object_info(address)

    @staticmethod
    def get_addresses_by_latitude_and_longitude(latitude, longitude):
        addresses = Addresses.get_all().filter(audit=1)
        gc = GeohashCode(latitude, longitude)
        geohash_code = gc.get_bigger_block()
        return addresses.filter(geohash_code__startswith=geohash_code)

    @staticmethod
    def get_addresses_by_geohash_code(geohash_code):
        addresses = Addresses.get_all().exclude(audit=0)
        return addresses.filter(geohash_code__contains=geohash_code)

    @staticmethod
    def get_addresses(params):
        address_objects = Address.objects.all().exclude(audit=0).order_by("id")
        longitude = float(params.get("longitude", 0))
        latitude = float(params.get("latitude", 0))
        detail_address = params.get("detail_address", None)
        if longitude:
            float_start_longitude = float(longitude)
            float_end_longitude = float_start_longitude + 0.01
            address_objects = address_objects.filter(longitude__gte=float_start_longitude,
                                                     longitude__lte=float_end_longitude)
        if latitude:
            float_start_latitude = float(latitude)
            float_end_latitude = float_start_latitude + 0.01
            address_objects = address_objects.filter(latitude__gte=float_start_latitude,
                                                     latitude__lte=float_end_latitude)
        if detail_address:
            address_objects = address_objects.filter(detail_address__contains=detail_address)

        address_objects = address_objects.order_by("-id")

        return address_objects

    @staticmethod
    def static_add(params):
        try:
            name = params.get("name", None)
            longitude = float(params.get("longitude", 0))
            latitude = float(params.get("latitude", 0))
            geohash_code = Addresses.get_geohash_code(latitude, longitude)
            china_id = int(params.get("china_id", 0))
            detail_address = params.get("detail_address", None)

            address = Address(
                longitude=longitude,
                latitude=latitude,
                geohash_code=geohash_code,
                detail_address=detail_address
            )
            if name is not None:
                address.name = name
            if china_id:
                address.china_id = china_id
            address.save()
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def reset_addresses():
        addresses = Address.objects.all()
        for address in addresses:
            address.audit = -1
            address.save()
        return addresses

    @staticmethod
    def static_get(address_id):
        try:
            return Address.objects.get(id=address_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def clear_city(city):
        if city and len(city) == 2:
            return city
        if city:
            if city.endswith(u"市"):
                city = city[:-1]
        return city

    @staticmethod
    def add_city(city):
        if city:
            if city.endswith(u"市"):
                return city
            else:
                return city + u"市"
        return city
