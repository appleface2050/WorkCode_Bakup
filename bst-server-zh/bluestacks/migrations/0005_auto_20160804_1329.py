# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-08-04 13:29
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('bluestacks', '0004_osspackage_desc'),
    ]

    operations = [
        migrations.AlterField(
            model_name='osspackage',
            name='desc',
            field=models.CharField(blank=True, default=b'', max_length=512, verbose_name='\u63cf\u8ff0'),
        ),
    ]