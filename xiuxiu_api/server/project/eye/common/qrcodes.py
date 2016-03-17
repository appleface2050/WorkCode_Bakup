# -*- coding: utf-8 -*-

import qrcode
from cStringIO import StringIO


class Qrcodes(object):
    @staticmethod
    def generate_image_stream(data):
        img = qrcode.make(data)

        buf = StringIO()
        img.save(buf)
        return buf.getvalue()
