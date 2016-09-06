# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-07-12 17:57
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0038_resultengineuninstallrate'),
    ]

    operations = [
        migrations.CreateModel(
            name='ResultAppTotal',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('result_date', models.DateField(verbose_name='\u65e5\u671f')),
                ('daily_user_init', models.IntegerField(default=0)),
                ('daily_user_init_count', models.IntegerField(default=0)),
                ('daily_init_fail', models.IntegerField(default=0)),
                ('daily_init_fail_count', models.IntegerField(default=0)),
                ('daily_install', models.IntegerField(default=0)),
                ('daily_install_count', models.IntegerField(default=0)),
                ('daily_install_fail', models.IntegerField(default=0)),
                ('daily_install_fail_count', models.IntegerField(default=0)),
                ('daily_download', models.IntegerField(default=0)),
                ('daily_download_count', models.IntegerField(default=0)),
                ('daily_download_fail', models.IntegerField(default=0)),
                ('daily_download_fail_count', models.IntegerField(default=0)),
                ('uptime', models.DateTimeField(auto_now=True, verbose_name='\u6570\u636e\u66f4\u65b0\u65f6\u95f4')),
            ],
            options={
                'abstract': False,
            },
        ),
    ]
