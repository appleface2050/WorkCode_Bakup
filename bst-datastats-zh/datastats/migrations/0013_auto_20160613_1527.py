# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-06-13 15:27
from __future__ import unicode_literals

from django.db import migrations


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0012_emulatorsession'),
    ]

    operations = [
        migrations.RenameModel(
            old_name='EmulatorSession',
            new_name='ResultEmulatorSession',
        ),
    ]
