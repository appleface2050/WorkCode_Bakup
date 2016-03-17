# -*- coding: utf-8 -*-

from django.contrib.sessions.backends.db import SessionStore
from django.contrib.sessions.models import Session
from logs import Logs


class Sessions(object):
    @classmethod
    def set_session(cls, key, value):
        if cls.sessionStore:
            cls.sessionStore[key] = value
        else:
            cls.sessionStore = SessionStore()
            cls.sessionStore[key] = value

    @classmethod
    def get_session_key(cls):
        if cls.sessionStore:
            return cls.sessionStore.session_key
        else:
            return None

    @classmethod
    def get_keys(cls):
        if cls.sessionStore:
            return cls.sessionStore.get_keys()
        else:
            return []

    @classmethod
    def get_session(cls, session_key):
        try:
            return Session.objects.get(pk=session_key)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @classmethod
    def get_session_data(cls, session):
        if session:
            return session.session_data
        else:
            return None

    @classmethod
    def get_session_data_decoded(cls, session):
        if session:
            return session.get_decoded()
        else:
            return None

    @classmethod
    def get_session_expire(cls, session):
        if session:
            return session.expire_date
        else:
            return None
