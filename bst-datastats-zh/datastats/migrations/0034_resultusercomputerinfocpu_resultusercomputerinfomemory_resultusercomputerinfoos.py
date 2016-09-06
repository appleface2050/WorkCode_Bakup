# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-07-06 14:29
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0033_resultenginedau'),
    ]

    operations = [
        migrations.CreateModel(
            name='ResultUserComputerInfoCPU',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('result_date', models.DateField(verbose_name='\u65e5\u671f')),
                ('cpu', models.CharField(max_length=128)),
                ('dst_count', models.IntegerField(default=0)),
                ('uptime', models.DateTimeField(auto_now=True, verbose_name='\u6570\u636e\u66f4\u65b0\u65f6\u95f4')),
            ],
            options={
                'abstract': False,
            },
        ),
        migrations.CreateModel(
            name='ResultUserComputerInfoMemory',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('result_date', models.DateField(verbose_name='\u65e5\u671f')),
                ('memory', models.IntegerField(default=0)),
                ('dst_count', models.IntegerField(default=0)),
                ('uptime', models.DateTimeField(auto_now=True, verbose_name='\u6570\u636e\u66f4\u65b0\u65f6\u95f4')),
            ],
            options={
                'abstract': False,
            },
        ),
        migrations.CreateModel(
            name='ResultUserComputerInfoOS',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('result_date', models.DateField(verbose_name='\u65e5\u671f')),
                ('os', models.CharField(max_length=16)),
                ('dst_count', models.IntegerField(default=0)),
                ('uptime', models.DateTimeField(auto_now=True, verbose_name='\u6570\u636e\u66f4\u65b0\u65f6\u95f4')),
            ],
            options={
                'abstract': False,
            },
        ),
    ]
