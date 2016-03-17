from django.conf.urls import include, url
from rest_framework import routers
from . import views

router = routers.DefaultRouter()
router.register(r'forumcategories', views.ForumCategoryViewSet)
router.register(r'forumposts4editor', views.ForumPostSourceViewSet)
router.register(r'forumreplies4editor', views.ForumReplySourceViewSet)

urlpatterns = [
    url(r'^', include(router.urls)),

]
