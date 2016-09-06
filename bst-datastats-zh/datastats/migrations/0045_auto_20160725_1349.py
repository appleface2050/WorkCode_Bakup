# -*- coding: utf-8 -*-
# Generated by Django 1.9.4 on 2016-07-25 13:49
from __future__ import unicode_literals

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('datastats', '0044_confuninstallreasonmeaning'),
    ]

    operations = [
        migrations.AddField(
            model_name='appactivity',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='appinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='emulatoractivity',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='emulatorinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='engineactivity',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='engineinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='midresultinstallinitemulator',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='midresultinstallinitengine',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultappactivity',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultappinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultappsession',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultapptotal',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultemulatoractivity',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultemulatorinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultemulatorinstallcount',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultemulatorsession',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultemulatoruninstallcount',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultemulatoruninstallnextdaycount',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultengineactivity',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultenginedau',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultengineinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultengineuninstallrate',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultgeneralapkinsterror',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultgeneralengineinstall',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultgeneralengineinsterror',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultgeneralenginiterror',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultgeneralosversion',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultgeneraluninstallreason',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultretention',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultretentionengine',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultusercomputerinfocpu',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultusercomputerinfomemory',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
        migrations.AddField(
            model_name='resultusercomputerinfoos',
            name='channel',
            field=models.CharField(blank=True, max_length=64),
        ),
    ]
