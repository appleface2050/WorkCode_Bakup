# coding:utf-8
from django.core.management.base import BaseCommand, CommandError
from eye.views import execute_cron_function

class Command(BaseCommand):
    def handle(self, *args, **options):
        execute_cron_function()
        print "hello world"