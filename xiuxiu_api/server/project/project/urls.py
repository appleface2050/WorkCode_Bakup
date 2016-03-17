# coding: utf-8
"""project URL Configuration

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/1.8/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  url(r'^$', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  url(r'^$', Home.as_view(), name='home')
Including another URLconf
    1. Add an import:  from blog import urls as blog_urls
    2. Add a URL to urlpatterns:  url(r'^blog/', include(blog_urls))
"""
# from django.conf.urls import include, url, patterns
# from django.contrib import admin
# import settings
#
# urlpatterns = [
#     url(r'^admin/', include(admin.site.urls)),
# ]

# urls.py
from django.contrib import admin
from django.conf.urls import patterns, url, include
from rest_framework.authtoken import views as token_views

from . import settings
from django.views.generic import RedirectView
from eye.views import WeixinInterface

urlpatterns = [
    url(r'^admin/', include(admin.site.urls)),

    url(r'^api-auth/', include('rest_framework.urls', namespace='rest_framework')),
    url(r'^accounts/profile/$', RedirectView.as_view(url='/'), name='profile-redirect'),
    url(r'^api-token-auth/', token_views.obtain_auth_token),

    url(r'^rest/api/', include("eye.urls")),
    # url(r'^rest/api/v1/', include("eye.urls", namespace="v1")),
    # url(r'^rest/api/v2/', include("eye.urls", namespace="v2")),
    url(r'^rest/api/auth/', include('rest_auth.urls')),
    url(r'^rest/api/auth/log/out/', 'eye.views.logout_action'),

    url(r'^api-auth/', include('rest_framework.urls', namespace='rest_framework')),
    url(r'^accounts/profile/$', RedirectView.as_view(url='/'), name='profile-redirect'),

    url(r'^rest/api/SMS/exist/', 'eye.views.user_exist'),
    url(r'^rest/api/SMS/send', 'eye.views.send_message'),
    url(r'^rest/api/SMS/check', 'eye.views.check_code'),

    url(r'^rest/api/register', 'eye.views.register_json'),
    url(r'^rest/api/dianping/find_business', 'eye.views.find_business'),
    url(r'^rest/api/dianping/add_business', 'eye.views.add_business'),
    url(r'^rest/api/reset_password', 'eye.views.reset_password_action'),
    url(r'^rest/api/userinfo', 'eye.views.get_basic_user_info'),
    url(r'^rest/api/shop/get_id', 'eye.views.get_shop_id_json'),
    url(r'^rest/api/main/hot', 'eye.views.get_publish_infos_hot'),
    url(r'^rest/api/main/nearby', 'eye.views.get_shop_infos_nearby'),
    url(r'^rest/api/main/discovery', 'eye.views.get_publish_infos_discovery'),
    url(r'^rest/api/main/key', 'eye.views.get_shops_search_key'),
    url(r'^rest/api/main/search', 'eye.views.get_shop_4_search'),
    url(r'^rest/api/main/user', 'eye.views.get_publish_infos_user'),
    url(r'^rest/api/detail/publishes', 'eye.views.get_publishes_by_shop'),
    url(r'^rest/api/mine/get_info', 'eye.views.get_infos_about_user'),

    url(r'^rest/api/red_envelope/send', 'eye.views.get_red_envelope'),
    url(r'^rest/api/red_envelope/receive', 'eye.views.set_red_envelope'),
    url(r'^rest/api/red_envelope/user', 'eye.views.get_red_envelope_by_user'),

    url(r'^rest/api/forum/category/get', 'eye.views.get_category_data_interface', name="get_category_data"),
    url(r'^rest/api/youmeng/push', 'eye.views.youmeng_push', name="youmeng_push"),
    url(r'^rest/api/is_login', 'eye.views.is_login_from_app_by_username'),

    url(r'^rest/api/attribute/get', 'eye.views.get_attribute_count_by_publish_id_json'),
    url(r'^rest/api/publish/score', 'eye.views.get_score_by_publish'),

    url(r'^rest/api/comment/get', 'eye.views.get_all_comments'),
    url(r'^rest/api/comment/unread/count', 'eye.views.get_comment_count_by_user'),
    url(r'^rest/api/comment/unread/get', 'eye.views.get_comment_by_user'),

    url(r'^rest/api/comment/unread/update', 'eye.views.update_unread_comment_by_publish_interface'),
    url(r'^rest/api/attribute/set', 'eye.views.set_attribute'),
    url(r'^rest/api/attribute/clear', 'eye.views.clear_unread_win_and_lost'),
    url(r'^rest/api/attribute/push', 'eye.views.push_win_and_lost'),
    url(r'^rest/api/attribute/ios/push', 'eye.views.push_win_and_lost_ios'),
    url(r'^rest/api/attribute/unread/get', 'eye.views.get_all_unread_win_and_lost_by_publish'),
    url(r'^rest/api/recommend/set', 'eye.views.set_recommended_attribute'),

    # not restructure start
    url(r'^rest/api/category/create', 'eye.views.add_category_all'),
    url(r'^rest/api/feedback/create', 'eye.views.set_feedback'),
    url(r'^rest/api/record/create', 'eye.views.set_click_title_record'),
    url(r'^rest/api/purchase/create', 'eye.views.change_purchase_url'),
    url(r'^rest/api/purchase/change', 'eye.views.change_purchase_url_interface'),

    url(r'^rest/api/cloud/get', 'eye.views.get_map_shops_info'),
    url(r'^rest/api/cloud/info/get', 'eye.views.get_cloud_shops'),
    url(r'^rest/api/cloud/create_or_update', 'eye.views.insert_or_update_shop_info'),
    # not restructure end

    url(r'^rest/api/weather/get', 'eye.views.get_weather_info'),
    url(r'^rest/api/TV/get', 'eye.views.get_tv_data'),
    url(r'^rest/api/TV/app/get', 'eye.views.get_tv_data_4_app'),

    # url(r'^rest/api/constant/get_categories', 'eye.views.get_publish_big_categories'),
    # url(r'^rest/api/constant/get_category_items', 'eye.views.get_publish_category_items'),
    # url(r'^rest/api/constant/get_answers', 'eye.views.get_publish_answers'),
    # url(r'^rest/api/constant/get_show', 'eye.views.get_answers_show'),
    url(r'^rest/api/constant/get_all', 'eye.views.get_publish_constant_all'),
    # url(r'^rest/api/constant/get_device_icons', 'eye.views.get_device_icons'),
    url(r'^rest/api/constant/get_attributes', 'eye.views.get_win_and_lost'),
    url(r'^rest/api/constant/purchase/url', 'eye.views.get_purchase_url'),

    url(r'^rest/api/download/$', 'eye.views.download'),
    url(r'^rest/api/app/download/$', 'eye.views.channel_app'),
    # url(r'^rest/api/image_save', 'eye.views.image_save'),
    # url(r'^rest/api/add_picture/', 'eye.views.add_picture'),

    # restructure in here
    url(r'^rest/api/coupon/activate', 'eye.views.activate_coupon'),
    url(r'^rest/api/coupon/again/activate', 'eye.views.activate_coupon_again'),
    url(r'^rest/api/coupon/waste', 'eye.views.waste_coupon'),
    url(r'^rest/api/coupon/get', 'eye.views.get_user_coupons'),
    url(r'^rest/api/coupon/my', 'eye.views.my_coupon'),
    url(r'^rest/api/coupon/url', 'eye.views.get_my_coupon_url'),
    url(r'^rest/api/coupon/channel', 'eye.views.coupon_channel'),
    url(r'^rest/api/coupon/index', 'eye.views.coupon'),
    url(r'^rest/api/coupon/success', 'eye.views.coupon_success'),

    url(r'^rest/api/activity/register/$', 'eye.views.auto_register_interface'),
    url(r'^rest/api/activity/ranking/$', 'eye.views.get_activity_ranking_interface'),
    url(r'^rest/api/activity/carousel/$', 'eye.views.get_activity_carousel_data'),
    url(r'^rest/api/activity/bad_air/$', 'eye.views.get_activity_bad_air'),
    url(r'^rest/api/activity/rank_of_bad_air/$', 'eye.views.get_activity_bad_air_rank'),
    url(r'^rest/api/activity/company_sign/$', 'eye.views.get_activity_company_sign'),
    url(r'^rest/api/activity/coupon/$', 'eye.views.get_activity_coupon'),
    url(r'^rest/api/activity/c600/$', 'eye.views.get_activity_coupon_600'),
    url(r'^rest/api/activity/send_coupons/$', 'eye.views.send_coupons_from_activity'),
    url(r'^rest/api/activity/prize_of_sign/$', 'eye.views.get_activity_prize_of_sign'),

    url(r'^rest/api/activity/activity1/$', 'eye.views.registration_activity_1'),



    url(r'^rest/api/games/graph', 'eye.views.get_graph_color_data'),
    url(r'^rest/api/games/set', 'eye.views.set_score'),
    url(r'^rest/api/games/ranking', 'eye.views.get_sorted_ranking'),
    url(r'^rest/api/games/reward', 'eye.views.send_reward'),
    url(r'^rest/api/games/index', 'eye.views.games_index'),
    url(r'^rest/api/games/rank_index', 'eye.views.ranking_index'),

    url(r'^rest/api/spring', 'eye.views.get_spring'),
    # url(r'^rest/api/festival/lantern', 'eye.views.the_lantern_festival'),
    # url(r'^rest/api/festival/greasy', 'eye.views.dissolve_greasy'),

    url(r'^rest/api/forum/win', 'eye.views.click_win'),
    url(r'^rest/api/forum/collect', 'eye.views.click_collect'),
    url(r'^rest/api/forum/browse', 'eye.views.click_browse'),
    url(r'^rest/api/forum/article', 'eye.views.get_articles'),
    url(r'^rest/api/forum/recommended/article', 'eye.views.get_recommended_articles'),
    url(r'^rest/api/forum/stored/article', 'eye.views.get_collected_articles'),
    url(r'^rest/api/forum/label', 'eye.views.get_labels'),
    url(r'^rest/api/forum/one/article', 'eye.views.get_one_article'),

    url(r'^rest/api/outer/info', 'eye.views.get_pm2_5'),
    url(r'^rest/api/outer/detector', 'eye.views.get_detectors'),
    url(r'^rest/api/outer/d/info', 'eye.views.get_detectors_info'),

    # url(r'^add/', 'eye.views.add'),
    # url(r'^login_action/', 'eye.views.login_action'),
    # url(r'^address_action/', 'eye.views.address_action'),
    # url(r'^shop_action/', 'eye.views.shop_action'),
    # url(r'^publish_action/', 'eye.views.publish_action'),
    url(r'^feedback/show', 'eye.views.show_feedback'),
    url(r'^record/show', 'eye.views.show_click_title_record'),
    url(r'^deal/show', 'eye.views.show_deal_info'),
    # url(r'^address_all_action/', 'eye.views.address_all_action'),
    # url(r'^shop_all_action/', 'eye.views.shop_all_action'),
    # url(r'^publish_all_action/', 'eye.views.publish_all_action'),

    # url(r'^logout/', 'eye.views.logout'),


    # url(r'^test/get_current_user/', 'eye.views.get_current_user'),
    # url(r'^test/formaldehyde/add/', 'eye.views.add_formaldehyde'),
    url(r'^test/$', 'eye.views.test'),
    # url(r'^test/business/get/$', 'eye.views.get_single_business_from_dianping_interface'),
    # url(r'^test/no/such/url/$', 'eye.views.test'),
    # url(r'^test/user/count/$', 'eye.views.get_user_count'),
    # url(r'^test/get_request_user/', 'eye.views.get_request_user'),
    # url(r'^test/index/', 'eye.views.test_index'),
    # url(r'^test/shop/image/', 'eye.views.get_shop_image_test'),

    url(r'^test/shop/get/', 'eye.views.get_shops_by_publish_pm_2_5'),
    url(r'^test/download/', 'eye.views.download_file'),
    url(r'^test/login/', 'eye.views.test_login'),
    url(r'^test/address/', 'eye.views.test_address'),
    url(r'^test/statistics/', 'eye.views.publish_statistics'),
    url(r'^test/s/$', 'eye.views.test_address'),
    url(r'^test/qrcode/$', 'eye.views.get_qrcode'),
    url(r'^test/bulk/coupon/create/$', 'eye.views.bulk_create_coupons'),
    url(r'^test/bulk/coupon/show/$', 'eye.views.show_coupons'),
    url(r'^test/test/$', 'eye.views.test'),
    url(r'^test/address_update$', 'eye.views.update_address_geohash_code'),

    url(r'^test/weixin/jiekou/',  WeixinInterface.as_view(), name="jiekou"),

    url(r'^editor/', include("eye.urls4editor")),
    # url(r'^logout/', 'eye.views.logout'),

    # not restructure start
    url(r'^publish/show/', 'eye.views.my_publish'),
    url(r'^publish/actions/', 'eye.views.my_publish_actions'),
    url(r'^publish/check/', 'eye.views.my_publish_check'),

    url(r'^shop/show/', 'eye.views.shop_check_4_editor_interface'),
    url(r'^shop/check/', 'eye.views.my_shop_check'),
    url(r'^shop/editor/add', 'eye.views.add_shop_formaldehyde_4_editor_interface'),
    url(r'^shop/editor/publish/add', 'eye.views.add_shop_formaldehyde_4_publish_editor_interface'),
    url(r'^shop/editor/actions', 'eye.views.add_formaldehyde_actions'),
    url(r'^shop/editor/publish/actions', 'eye.views.add_formaldehyde_4_publish_actions'),
    url(r'^shop/actions/', 'eye.views.my_shop_actions'),
    url(r'^show/valid/', 'eye.views.show_valid_red_envelope_by_user'),
    url(r'^show/request/', 'eye.views.show_request_red_envelope_by_user'),
    url(r'^shop/create/', 'eye.views.generate_csv_file'),

    url(r'^forum/index/$', 'eye.views.show_category', name="category_index"),
    url(r'^forum/download/$', 'eye.views.download_file', name="download_file"),
    url(r'^forum/post/list/(?P<category_id>\d+)/$', 'eye.views.show_post_list', name="post_list"),
    url(r'^forum/post/list/data/$', 'eye.views.get_post_list_data', name="post_list_data"),
    url(r'^forum/post/detail/(?P<post_id>\d+)/$', 'eye.views.show_post_detail', name="post_detail"),
    url(r'^forum/post/share/(?P<post_id>\d+)/$', 'eye.views.show_post_detail_share', name="post_share"),
    url(r'^forum/post/create/$', 'eye.views.show_post_create', name="post_create"),
    url(r'^forum/post/detail/add/win/$', 'eye.views.add_forum_post_win', name="add_win"),
    url(r'^forum/post/detail/cancel/win/$', 'eye.views.cancel_forum_post_win', name="cancel_win"),
    url(r'^forum/post/list/add/concern/$', 'eye.views.add_forum_category_concern', name="add_concern"),
    url(r'^forum/post/list/cancel/concern/$', 'eye.views.cancel_forum_category_concern', name="cancel_concern"),
    url(r'^forum/post/save/base64/image/$', 'eye.views.post_save_base64_to_image', name="post_save_base64_to_image"),
    url(r'^forum/reply/save/base64/image/$', 'eye.views.reply_save_base64_to_image', name="reply_save_base64_to_image"),
    url(r'^forum/reply/list/data/$', 'eye.views.get_reply_list_data', name="reply_list_data"),
    url(r'^forum/statistics/$', 'eye.views.forum_statistics', name="forum_statistics"),

    url(r'^support/user/$', 'eye.views.user_show_change', name="support_user_show"),
    url(r'^support/shop/$', 'eye.views.shop_check_4_editor_interface', name="support_shop_show"),
    url(r'^support/shop/get/$', 'eye.views.get_shops_by_publish_pm_2_5'),
    url(r'^support/publish/$', 'eye.views.my_publish', name="support_publish_show"),
    url(r'^support/publish_count/$', 'eye.views.show_publish_count_by_shop'),
    url(r'^support/configuration/$', 'eye.views.configuration'),
    url(r'^support/address/$', 'eye.views.addresses'),
    url(r'^support/index/$', 'eye.views.editor_index'),
    url(r'^support/login/$', 'eye.views.app_login'),
    url(r'^support/logout/$', 'eye.views.app_logout'),
    url(r'^support/feedback/$', 'eye.views.show_feedback'),
    url(r'^support/webmaster/$', 'eye.views.webmaster_index'),
    url(r'^support/webmaster/post/$', 'eye.views.category_owner_post'),
    url(r'^support/webmaster/post/reply/$', 'eye.views.category_owner_reply'),
    url(r'^support/webmaster/post/modify/$', 'eye.views.category_owner_post_modify'),
    url(r'^support/statistics/$', 'eye.views.get_statistics'),
    url(r'^support/coupon/create/$', 'eye.views.create_coupon'),
    url(r'^support/user/count/$', 'eye.views.get_user_count_by_datetime'),

    url(r'^support/detector/add/$', 'eye.views.detector_add'),
    url(r'^support/detector/update/$', 'eye.views.detector_update'),
    url(r'^support/detector/list/$', 'eye.views.detector_list'),
    url(r'^support/detector/search/$', 'eye.views.detector_search_shop'),
    url(r'^support/detector/sequence/$', 'eye.views.get_all_sequence'),

    url(r'^support/share/list/$', 'eye.views.share_statistics_list'),

    # url(r'^test/get_current_user/', 'eye.views.get_current_user'),
    url(r'^test/formaldehyde/add/', 'eye.views.add_formaldehyde'),
    # url(r'^test/$', 'eye.views.test'),
    url(r'^test/business/get/$', 'eye.views.get_single_business_from_dianping_interface'),
    # url(r'^test/no/such/url/$', 'eye.views.test'),
    url(r'^test/user/count/$', 'eye.views.get_user_count'),
    url(r'^test/get_request_user/', 'eye.views.get_request_user'),
    url(r'^test/index/', 'eye.views.test_index'),
    url(r'^test/categories/', 'eye.views.test_categories'),
    url(r'^test/forum/category/convert/', 'eye.views.convert_category_data_interface'),
    url(r'^test/version/', 'eye.views.test_version'),
    url(r'^test/shop/image/', 'eye.views.get_shop_image_test'),
    url(r'^test/shop/get/', 'eye.views.get_shops_by_publish_pm_2_5'),
    url(r'^test/shop/revise/', 'eye.views.revise_not_dianping_shop_address'),
    url(r'^test/download/', 'eye.views.download_file'),
    url(r'^test/coupon/create', 'eye.views.auto_add_coupons'),
    url(r'^test/weixin/jiekou/',  WeixinInterface.as_view(), name="jiekou"),
    # url(r'^test/game/',  'eye.views.test_game'),
    url(r'^test/pm25/',  'eye.views.test_pm25'),
    url(r'^test/category/',  'eye.views.test_publish_categories'),
    url(r'^test/publish/statistics/',  'eye.views.get_publish_data'),
    url(r'^test/shop/data/',  'eye.views.get_shop_data'),
    url(r'^test/place/',  'eye.views.place'),
    url(r'^test/warnings/',  'eye.views.test_warnings'),

    url(r'^ckeditor/$', include('ckeditor_uploader.urls')),
    url(r'^uploadimg/$', 'eye.views.file_upload'),
    url(r'^show/post/picture$', 'eye.views.show_post_picture'),
    url(r'^show/post/add$', 'eye.views.add_article_images'),
    url(r'^copy$', 'eye.views.copy'),
    # not restructure end

]

# Wire up our API using automatic URL routing.
# Additionally, we include login URLs for the browseable API.
# urlpatterns = patterns('',
#     url(r'^', include(router.urls)),
#     url(r'^api-auth/', include('rest_framework.urls', namespace='rest_framework'))
# )

# urlpatterns += format_suffix_patterns(urlpatterns)
#
# if settings.DEBUG is False:
    # static files (images, css, javascript, etc.)
urlpatterns += patterns('', (
    r'^media_root/(?P<path>.*)$', 'django.views.static.serve',
    {'document_root': settings.MEDIA_ROOT, 'show_indexes': True}
    ))

