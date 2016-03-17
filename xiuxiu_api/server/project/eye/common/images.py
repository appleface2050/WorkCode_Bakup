# -*- encoding: utf-8 -*-

import urllib2
import cStringIO
from PIL import Image
import files
from logs import Logs


class Images(object):
    @staticmethod
    def get_url(relative_url):
        if relative_url:
            return files.BASE_URL_4_IMAGE + relative_url
        else:
            return ""

    @staticmethod
    def get_shop_default_url(shop_category):
        pass

    @staticmethod
    def save_image(memory_file, path):
        data = {}
        try:
            img = Image.open(memory_file)
            img.save(path)
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def resize_image(memory_file, new_path, new_width=1280):
        try:
            # 读取图像
            im = Image.open(memory_file)
            # 获得图像的宽度和高度
            width, height = im.size
            # 计算原来的高宽比
            ratio = 1.0 * height / width
            # 计算新的高度
            new_height = int(new_width * ratio)
            new_size = (new_width, new_height)
            # 插值缩放图像
            out = im.resize(new_size, Image.ANTIALIAS)
            out.save(new_path)
            return True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False
