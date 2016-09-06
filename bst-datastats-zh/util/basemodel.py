# coding=utf-8

import json
from django.db import models
from django.db.models import DateTimeField, DateField, CommaSeparatedIntegerField, ImageField, DecimalField


BASE_DATE_FROMATE = '%Y-%m-%d'
BASE_DATETIME_FROMATE = '%Y-%m-%d %H:%M:%S'

class JSONBaseModel(models.Model):
    """
    带json序列化的基础Model
    """
    def toJSON(self):
        """
        序列化成 dict类型
        """

        fields = []
        for field in self._meta.fields:
            fields.append((field.name, field.attname, type(field)))

        d = {}
        for attr, attname, t in fields:
            if getattr(self, attname, None) == None:
                d[attr] = None
            else:
                if t == DateTimeField and not isinstance(getattr(self, attname), (str, unicode)):
                    d[attr] = getattr(self, attname).strftime(BASE_DATETIME_FROMATE)
                elif t == DateField and not isinstance(getattr(self, attname), (str, unicode)):
                    d[attr] = getattr(self, attname).strftime(BASE_DATE_FROMATE)
                elif t == CommaSeparatedIntegerField:
                    if isinstance(getattr(self, attname), (str,unicode)):
                        d[attr] = json.loads(getattr(self, attname, '[]'))
                    else:
                        d[attr] = getattr(self, attname)
                else:
                    d[attr] = getattr(self, attname)

        return d

    class Meta:
        abstract = True