# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-08-10 16:01
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0071_auto_20160810_1532'),
    ]

    operations = [
        migrations.AlterField(
            model_name='apptotalstat',
            name='result_date',
            field=models.DateField(verbose_name='\u65e5\u671f'),
        ),
    ]
