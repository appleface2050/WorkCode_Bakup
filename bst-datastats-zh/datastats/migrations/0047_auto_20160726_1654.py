# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-07-26 16:54
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0046_auto_20160725_1500'),
    ]

    operations = [
        migrations.AlterField(
            model_name='appactivity',
            name='op',
            field=models.CharField(max_length=64),
        ),
        migrations.AlterField(
            model_name='appactivity',
            name='status',
            field=models.CharField(default='success', max_length=32),
        ),
        migrations.AlterField(
            model_name='appinstall',
            name='op',
            field=models.CharField(max_length=64),
        ),
        migrations.AlterField(
            model_name='appinstall',
            name='status',
            field=models.CharField(default='success', max_length=32),
        ),
        migrations.AlterField(
            model_name='emulatoractivity',
            name='op',
            field=models.CharField(max_length=64),
        ),
        migrations.AlterField(
            model_name='emulatoractivity',
            name='status',
            field=models.CharField(default='success', max_length=32),
        ),
        migrations.AlterField(
            model_name='emulatorinstall',
            name='op',
            field=models.CharField(max_length=64),
        ),
        migrations.AlterField(
            model_name='emulatorinstall',
            name='status',
            field=models.CharField(default='success', max_length=32),
        ),
        migrations.AlterField(
            model_name='engineactivity',
            name='op',
            field=models.CharField(max_length=64),
        ),
        migrations.AlterField(
            model_name='engineactivity',
            name='status',
            field=models.CharField(default='success', max_length=32),
        ),
        migrations.AlterField(
            model_name='engineinstall',
            name='op',
            field=models.CharField(max_length=64),
        ),
        migrations.AlterField(
            model_name='engineinstall',
            name='status',
            field=models.CharField(default='success', max_length=32),
        ),
    ]
