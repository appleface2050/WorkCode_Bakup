# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-07-06 14:29
from __future__ import unicode_literals

from django.db import migrations


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0034_resultusercomputerinfocpu_resultusercomputerinfomemory_resultusercomputerinfoos'),
    ]

    operations = [
        migrations.DeleteModel(
            name='ResultUserComputerInfoCPU',
        ),
        migrations.DeleteModel(
            name='ResultUserComputerInfoMemory',
        ),
        migrations.DeleteModel(
            name='ResultUserComputerInfoOS',
        ),
    ]