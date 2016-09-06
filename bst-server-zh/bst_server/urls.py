"""bst_server URL Configuration

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/1.9/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  url(r'^$', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  url(r'^$', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.conf.urls import url, include
    2. Add a URL to urlpatterns:  url(r'^blog/', include('blog.urls'))
"""
from django.conf.urls import url,include
from django.contrib import admin

from django.views.generic.base import RedirectView

# from bluestacks.views import index
from bluestacks.views_app_center import app_center_html,index, pts
from bst_server.settings import ENVIRONMENT
from bs_manage.views_user import register,user_login,sign_out

urlpatterns = [
    url(r'^$', index, name='index'),
    # url(r'^$', app_center_html, name=''),
    url(r'^captcha/', include('captcha.urls')),
    url(r'^accounts/', include('django.contrib.auth.urls')),
    # url(r'^register$', RegisterView.as_view(), name='register'),

    # url(r'^favicon\.ico$', 'django.views.generic.simple.redirect_to', {'url': '/static/images/favicon.ico'}),
    url(r'^favicon\.ico$', RedirectView.as_view(url='/static/favicon.ico', permanent=True)),
    url(r'^admin/', admin.site.urls),
    # url(r'^cr/', include('clash_royale_app.urls')),
    url(r'^bs/', include('bluestacks.urls')),
    url(r'^bs_browser/', include('bluestacks_browser.urls')),

    url(r'^d39b239616dca997e674d7d82060e17f\.html$', pts, name="pts"),     #use for pts test
]


if ENVIRONMENT != "aliyun":

    urlpatterns += [
        url(r'^manage/', include('bs_manage.urls', namespace='bs_manage')),
        # url(r'^sign_up$', register, name='sign_up'),
        # url(r'^sign_in$', user_login, name='sign_in'),
        # url(r'^sign_out$', sign_out, name='sign_out')

    ]

urlpatterns += [
        url(r'^sign_up$', register, name='sign_up'),
        url(r'^sign_in$', user_login, name='sign_in'),
        url(r'^sign_out$', sign_out, name='sign_out')

]