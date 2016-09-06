from django.conf.urls import url

from datastats.views_app_total2 import app_total2
from datastats.views_emulator2 import emulator2
from datastats.views_engine2 import engine2
from datastats.views_error import apk_install_error,uninstall_reason,uninstall_reason2
from . import views
from . import views_data_collection
from . import views_app
from views_user_computer_info import user_computer_info_os, user_computer_info_memory, user_computer_info_cpu
from views_app_total import app_total,app_local_50

urlpatterns = [
    #web page
    url(r'^$', views.index, name='index'),
    url(r'^engine$', engine2, name='engine'),
    # url(r'^engine2$', engine2, name='engine2'),
    url(r'^emulator$', emulator2, name='emulator'),
    url(r'^appcenter$', views.appcenter, name='appcenter'),
    url(r'^app_page1$', views_app.app_page1, name='app_page1'),
    url(r'^app_page2$', views_app.app_page2, name='app_page2'),
    url(r'^app_total$', app_total2, name='app_total'),
    url(r'^app_local_50$', app_local_50, name='app_local_50'),

    url(r'^apk_install_error$', apk_install_error, name='apk_install_error'),
    url(r'^uninstall_reason$', uninstall_reason, name='uninstall_reason'),
    url(r'^uninstall_reason2$', uninstall_reason2, name='uninstall_reason2'),

    # url(r'^uninstall_reason$', uninstall_code, name='uninstall_reason'),

    # url(r'^app_total$', views_app.app_total, name='app_total'),
    url(r'^user_computer_info_os$', user_computer_info_os, name='user_computer_info_os'),
    url(r'^user_computer_info_memory$', user_computer_info_memory, name='user_computer_info_memory'),
    url(r'^user_computer_info_cpu$', user_computer_info_cpu, name='user_computer_info_cpu'),


    #data collection
    url(r'^engine_install$', views_data_collection.engine_install, name='engine_install'),
    url(r'^engine_activity$', views_data_collection.engine_activity, name='engine_activity'),
    url(r'^emulator_install$', views_data_collection.emulator_install, name='emulator_install'),
    url(r'^emulator_activity$', views_data_collection.emulator_activity, name='emulator_activity'),
    url(r'^app_install$', views_data_collection.app_install, name='app_install'),
    url(r'^app_activity$', views_data_collection.app_activity, name='app_activity'),
    url(r'^general_json$', views_data_collection.general_json, name='general_json'),


    #migrate data
    url(r'^migrate$', views_data_collection.migrate, name='migrate'),


]