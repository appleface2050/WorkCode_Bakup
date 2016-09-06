from django.conf.urls import url

# from bluestacks.views_app_center import ArticleListView, ArticlePublishView, ArticleDetailView, ArticleEditView
from . import views_browser
# from views import HomeView


urlpatterns = [
    url(r'^$', views_browser.index, name='index'),
    # url(r'^js_csrf$', views.js_csrf, name='js_csrf'),
    # # url(r'^$', HomeView.as_view(), name='index'),
    url(r'^feedback_html$', views_browser.feedback_html, name='feedback_html'),
    # url(r'^feedback$', views.feedback, name='feedback'),
    # url(r'^feedback_test$', views.feedback_test, name='feedback_test'),
    #
    # # url(r'^feedback$', views.feedback, name='feedback'),
    # url(r'^upload_token$', views.upload_token, name='get_upload_token'),
    # url(r'^upload_callback$', views.upload_callback, name='upload_callback'),
    #
    # url(r'^reg_user$', views.reg_user, name='reg_user'),
    # url(r'^captcha_test$', views.some_view, name='captcha_test'),
    #
    #
    # # url(r'^captcha$', views.captcha, name='captcha'),
    # # url(r'^refresh_captcha$', views.refresh_captcha, name='refresh_captcha'),
    #
    # url(r'^upload_file', views.upload_file, name='upload_file'),
    # # url(r'^upyun_upload_test$', views.upyun_upload_test, name='upyun_upload_test'),
    #
    # url(r'^post_upload_file_name', views.post_upload_file_name, name='post_upload_file_name'),
    #
    # #oss_upload_html
    # url(r'^oss_upload_html$', views.oss_upload_html, name='oss_upload_html'),
    #
    #app center html
    url(r'^app_center_html$', views_browser.app_center_html, name='app_center_html'),
    url(r'^app_center_recommend_html$', views_browser.app_center_recommend_html, name='app_center_recommend_html'),
    url(r'^app_center_applist_html$', views_browser.app_center_applist_html, name='app_center_applist_html'),
    url(r'^app_center_topic_html$', views_browser.app_center_topic_html, name='app_center_topic_html'),
    #
    # #app center
    # url(r'^app_center_home$', views_app_center.app_center_home, name='app_center_home'),
    # url(r'^app_center_recommend$', views_app_center.app_center_recommend, name='app_center_recommend'),
    # url(r'^app_center_board$', views_app_center.app_center_board, name='app_center_board'),
    # # url(r'^app_center_topic$', views_app_center.app_center_topic, name='app_center_topic'),
    # # url(r'^app_center_type', views_app_center.app_center_type, name='app_center_type'),
    #
    #
    # # url(r'^app_detail$', views_app_center.app_detail, name='app_detail'),
    url(r'^app_detail_html$', views_browser.app_detail_html, name='app_detail_html'),
    # url(r'^app_detail_data$', views_app_center.app_detail_data, name='app_detail_data'),
    #
    #
    url(r'^app_recommend_html$', views_browser.app_recommend_html, name='app_recommend_html'),
    # url(r'^app_recommend_data$', views_app_center.app_recommend_data, name='app_recommend_data'),
    #
    url(r'^app_center_topic_detail_html$', views_browser.app_center_topic_detail_html, name='app_center_topic_detail_html'),
    # url(r'^app_center_topic_list$', views_app_center.app_center_topic_list, name='app_center_topic_list'),
    # url(r'^app_center_topic_data$', views_app_center.app_center_topic_data, name='app_center_topic_data'),
    #
    url(r'^app_center_topic_all_html$', views_browser.app_center_topic_all_html, name='app_center_topic_data'),
    #
    url(r'^app_center_game$', views_browser.app_center_game, name='app_center_game'),
    #
    url(r'^app_center_tag_html$', views_browser.app_center_tag_html, name='app_center_tag_html'),
    # url(r'^app_center_tag_data$', views_app_center.app_center_tag_data, name='app_center_tag_data'),
    #
    # url(r'^update_info$', views_app_center.update_info, name='update_info'),
    # url(r'^rating_comment$', views_app_center.rating_comment, name='rating_comment'),
    #
    #search function
    url(r'^appsearch_html$', views_browser.appsearch_html, name='appsearch'),
    url(r'^topic_pokemongo$', views_browser.topic_pokemongo, name='topic_pokemongo'),
    # url(r'^search$', views_browser.search, name='search'),
    #
    # #flash memchaced
    # url(r'^memcached_flush_all$', views_app_center.memcached_flush_all, name='memcached_flush_all'),
    #
    # #flash app center game data
    # url(r'^app_center_flush$', views_app_center.app_center_flush, name='app_center_flush'),
    #
    # url(r'^captcha$', views.captcha, name='captcha'),
    # url(r'^verify_captcha$', views.verify_captcha, name='verify_captcha'),
    #
    # # url(r'^test_blog$', views_app_center.test_blog, name='test_blog'),
    # # url(r'^blog/$', views_app_center.blog_index, name='blog_index'),
    # url(r'^test_blog', ArticleListView.as_view(), name='test_blog'),
    # url(r'^publish$', ArticlePublishView.as_view(), name='article_publish'),
    # # url(r'^article/(?P<title>\S+)$', ArticleDetailView.as_view(), name='article_detail'),
    # url(r'^article/(?P<title>\w+\.?\w+)$', ArticleDetailView.as_view(), name='article_detail'),
    # url(r'^article/(?P<title>\w+\.?\w+)/edit$', ArticleEditView.as_view(), name='article_edit'),
    #
    #
    # #push message
    # url(r'^bs_message$', views.bs_message, name='bs_message'),




]
