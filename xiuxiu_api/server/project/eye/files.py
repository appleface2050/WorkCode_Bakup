# coding: utf-8

import os


# import sys

class FileOperation(object):
    @staticmethod
    def getcwd():
        return os.getcwd()

    @staticmethod
    def listdir(path):
        return os.listdir(path)

    @staticmethod
    def isfile(path):
        return os.path.isfile(path)

    @staticmethod
    def isdir(path):
        return os.path.isdir(path)

    @staticmethod
    def exists(path):
        return os.path.exists(path)

    @staticmethod
    def rename(old_name, new_name):
        return os.name(old_name, new_name)

    @staticmethod
    def getsize(f):
        return os.path.getsize(f)
