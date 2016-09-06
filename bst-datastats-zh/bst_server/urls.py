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

from datastats.views_user import register,user_login,sign_out,index

# from bluestacks.views import index
# from bluestacks.views_app_center import RegisterView


urlpatterns = [
    url(r'^$', index, name='index'),


    url(r'^admin/', admin.site.urls),
    # url(r'^cr/', include('clash_royale_app.urls')),
    # url(r'^bs/', include('bluestacks.urls')),
    # url(r'^manage/', include('bs_manage.urls', namespace='bs_manage')),
    url(r'^data/', include('datastats.urls')),


    url(r'^sign_up$', register, name='sign_up'), # ADD NEW PATTERN!
    url(r'^sign_in$', user_login, name='sign_in'),
    url(r'^sign_out$', sign_out, name='sign_out'),



]
