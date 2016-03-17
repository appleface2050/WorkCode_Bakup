# coding: utf-8

import geohash


class GeohashCode(object):

    def __init__(self, latitude, longitude, length=6):
        self._longitude = longitude
        self._latitude = latitude
        self._length = length

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

    @property
    def latitude(self):
        return self._latitude

    @latitude.setter
    def latitude(self, value):
        if not isinstance(value, float):
            raise ValueError("latitude must be a float!")
        if value < 0:
            raise ValueError("latitude must be not less than 0")
        self._latitude = value

    @property
    def length(self):
        return self._length

    @length.setter
    def length(self, value):
        if not isinstance(value, int):
            raise ValueError("length must be an int!")
        if value <= 0:
            raise ValueError("length must be more than zero!")
        self._length = value

    def get(self):
        return geohash.encode(self._latitude, self._longitude, self._length)

    def get_bigger_block(self):
        the_length = self._length - 1
        return geohash.encode(self._latitude, self._longitude, the_length)
