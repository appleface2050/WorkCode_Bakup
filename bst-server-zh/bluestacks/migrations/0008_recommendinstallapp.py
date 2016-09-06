# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-08-18 13:57
from __future__ import unicode_literals

from django.db import migrations, models
import django.utils.timezone


class Migration(migrations.Migration):

    dependencies = [
        ('bluestacks', '0007_osspackage_mandatory'),
    ]

    operations = [
        migrations.CreateModel(
            name='RecommendInstallAPP',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('game_library_id', models.IntegerField()),
                ('order', models.IntegerField(default=0)),
                ('uptime', models.DateTimeField(default=django.utils.timezone.now, verbose_name='\u6570\u636e\u66f4\u65b0\u65f6\u95f4')),
            ],
            options={
                'abstract': False,
            },
        ),
    ]