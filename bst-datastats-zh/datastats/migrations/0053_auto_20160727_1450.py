# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-07-27 14:50
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0052_auto_20160727_1443'),
    ]

    operations = [
        migrations.AlterField(
            model_name='appactivity',
            name='channel',
            field=models.CharField(default='bscn', max_length=64),
        ),
        migrations.AlterField(
            model_name='appinstall',
            name='channel',
            field=models.CharField(default='bscn', max_length=64),
        ),
        migrations.AlterField(
            model_name='emulatoractivity',
            name='channel',
            field=models.CharField(default='bscn', max_length=64),
        ),
        migrations.AlterField(
            model_name='emulatorinstall',
            name='channel',
            field=models.CharField(default='bscn', max_length=64),
        ),
        migrations.AlterField(
            model_name='engineactivity',
            name='channel',
            field=models.CharField(default='bscn', max_length=64),
        ),
        migrations.AlterField(
            model_name='engineinstall',
            name='channel',
            field=models.CharField(default='bscn', max_length=64),
        ),
    ]
