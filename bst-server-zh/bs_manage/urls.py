#coding=utf-8

from django.conf.urls import url

from bs_manage.views import search_key_word,check_data,copy_data_2_db, tmp_copy_modify
from . import views

from bluestacks.views import oss_upload_html

urlpatterns = [
    # url(r'^$', views.index, name='index'),
    # url(r'^js_csrf$', views.js_csrf, name='js_csrf'),
    # # url(r'^$', HomeView.as_view(), name='index'),
    # url(r'^feedback$', views.feedback, name='feedback'),

    # url(r'^feedback_test$', views.feedback_test, name='feedback_test'),
    # url(r'^beedba$', views.feedback_test, name='feedback_test'),
    url(r'^$', views.index, name='index'),
    url(r'^feedback$', views.feedback, name='feedback'),
    url(r'^topic$', views.topic, name='topic'),
    url(r'^topic_game$', views.topic_game, name='topic_game'),
    url(r'^app_center_game_detail$', views.app_center_game_detail, name='app_center_detail'),
    url(r'^topic_game_delete$', views.topic_game_delete, name='topic_game_delete'),
    url(r'^topic_game_add$', views.topic_game_add, name='topic_game_add'),
    url(r'^topic_game_order_edit$', views.topic_game_order_edit, name='topic_game_order_edit'),

    url(r'^app_center_game_manage$', views.app_center_game_manage, name='app_center_game'),
    url(r'^app_center_game_edit$', views.app_center_game_edit, name='app_center_game_edit'),

    url(r'^topic_delete$', views.topic_delete, name='topic_delete'),
    url(r'^topic_edit$', views.topic_edit, name='topic_edit'),
    url(r'^topic_add$', views.topic_add, name='topic_add'),

    url(r'^recommend_game$', views.recommend_game, name='recommend_game'),
    url(r'^recommend$', views.recommend, name='recommend'),
    url(r'^recommend_add$', views.recommend_add, name='recommend_add'),
    url(r'^rec_edit$', views.rec_edit, name='rec_edit'),
    url(r'^rec_delete$', views.rec_delete, name='rec_delete'),
    url(r'^recommend_game_delete$', views.recommend_game_delete, name='recommend_game_delete'),
    url(r'^recommend_game_add$', views.recommend_game_add, name='recommend_game_add'),
    url(r'^recommend_game_order_edit$', views.recommend_game_order_edit, name='recommend_game_order_edit'),

    url(r'^game9_data_copy', views.game9_data_copy, name='game9_data_copy'),

    url(r'^oss_upload_html$', oss_upload_html, name='oss_upload_html'),

    url(r'^search_key_word$', search_key_word, name='search_key_word'),

    url(r'^check_data$', check_data, name='check_data'),

    url(r'^copy_data_2_db$', copy_data_2_db, name='copy_data_2_db'),

    url(r'^conf_pop_window$', views.conf_pop_window, name='conf_pop_window'),
    url(r'^conf_partner_preinstall_game_info$', views.conf_partner_preinstall_game_info, name='conf_partner_preinstall_game_info'),
    url(r'^conf_partner_preinstall_game_info_add$', views.conf_partner_preinstall_game_info_add, name='conf_partner_preinstall_game_info_add'),
    url(r'^conf_partner_preinstall_game_info_delete$', views.conf_partner_preinstall_game_info_delete, name='conf_partner_preinstall_game_info_delete'),


    # url(r'^tmp_copy_modify$', tmp_copy_modify, name='tmp_copy_modify'),

    #首次安装推荐游戏配置
    url(r'^manage_rec_install_app$', views.manage_rec_install_app, name='manage_rec_install_app'),
    url(r'^rec_install_app_delete$', views.rec_install_app_delete, name='rec_install_app_delete'),
    url(r'^rec_install_app_add$', views.rec_install_app_add, name='rec_install_app_add'),



]