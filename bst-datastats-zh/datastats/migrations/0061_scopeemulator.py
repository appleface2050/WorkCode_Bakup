# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-08-09 12:13
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0060_auto_20160804_1628'),
    ]

    operations = [
        migrations.CreateModel(
            name='ScopeEmulator',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('channel', models.CharField(blank=True, default='', max_length=64)),
                ('osver', models.CharField(blank=True, default='', max_length=20)),
                ('modes', models.CharField(default='day', max_length=20)),
                ('uptime', models.DateTimeField(auto_now=True, verbose_name='\u6570\u636e\u66f4\u65b0\u65f6\u95f4')),
                ('version', models.CharField(blank=True, default='', max_length=50)),
            ],
            options={
                'db_table': 'scope_emulator',
            },
        ),
    ]
