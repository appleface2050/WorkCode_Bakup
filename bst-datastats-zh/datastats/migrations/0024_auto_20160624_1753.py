# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-06-24 17:53
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0023_generaldata'),
    ]

    operations = [
        migrations.AlterField(
            model_name='resultappsession',
            name='das',
            field=models.IntegerField(default=0, verbose_name='daily app session'),
        ),
    ]
