﻿# coding=utf-8
"""
Django settings for bst_server project.

Generated by 'django-admin startproject' using Django 1.9.4.

For more information on this file, see
https://docs.djangoproject.com/en/1.9/topics/settings/

For the full list of settings and their values, see
https://docs.djangoproject.com/en/1.9/ref/settings/
"""

import os
import sys
# 解决 字符编码问题
reload(sys)
sys.setdefaultencoding('utf-8')


# Build paths inside the project like this: os.path.join(BASE_DIR, ...)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

print "BASE_DIR:",BASE_DIR

# BASE_DIR = os.path.dirname(os.path.dirname(__file__))

from util.network import get_local_ip

host = get_local_ip()

# ENVIRONMENT = 'win_dev'
ENVIRONMENT = 'aliyun'
# ENVIRONMENT = 'win_test'
# ENVIRONMENT = 'aliyun_test'
# ENVIRONMENT = 'aliyun_test_preview'

print host


if host == "123.56.202.57" or host == "101.201.39.219":
    ENVIRONMENT = 'aliyun'
elif host == "101.201.42.93" and BASE_DIR == "/var/www/html/bst-server-zh":
    ENVIRONMENT = 'aliyun_test'
elif host == "101.201.42.93" and BASE_DIR == "/var/www/html/preview_bst-server-zh/bst-server-zh":
    ENVIRONMENT = 'aliyun_test_preview'
else:
    ENVIRONMENT = 'win_test'

print "ENVIRONMENT:",ENVIRONMENT

if ENVIRONMENT == 'win_test':
    URL_SERVER_HOST = "127.0.0.1"
elif ENVIRONMENT == 'aliyun_test':
    URL_SERVER_HOST = host
elif ENVIRONMENT == 'aliyun':
    URL_SERVER_HOST = host
elif ENVIRONMENT == 'aliyun_test_preview':
    URL_SERVER_HOST = host

USE_MC_ENVIRONMENT = ("aliyun","aliyun_test_preview")

ALIYUN_MEMCACHE_HOST = "da9b304ea6e0492b.m.cnbjalinu16pub001.ocs.aliyuncs.com:11211"
ALIYUN_MEMCACHE_USERNAME = "da9b304ea6e0492b"
ALIYUN_MEMCACHE_PASSWORD = "Bluestacks2016"

ALIYUN_MEMCACHE_PREVIEW_HOST = "ec8c5722e776438c.m.cnbjalinu16pub001.ocs.aliyuncs.com:11211"
ALIYUN_MEMCACHE_PREVIEW_USERNAME = "ec8c5722e776438c"
ALIYUN_MEMCACHE_PREVIEW_PASSWORD = "Bluestacks2016"

ALIYUN_ACCESS_ID = "t8z6fuZ2B1FldqmL"
ALIYUN_ACCESS_SECRET_KEY = "ES2YRYWBREeBNeGCBczpc63FCLJvc9"
ALIYUN_OSS_ENDPOINT = "oss-cn-beijing.aliyuncs.com"


# Quick-start development settings - unsuitable for production
# See https://docs.djangoproject.com/en/1.9/howto/deployment/checklist/

# SECURITY WARNING: keep the secret key used in production secret!
SECRET_KEY = 'fv5dhmk78%3u*3@#+m@3(=^hh51-4w3y3ji1#b-a9$bgs)2wf&'

# SECURITY WARNING: don't run with debug turned on in production!
DEBUG = False

ALLOWED_HOSTS = ['*']


# AUTH_USER_MODEL = "bluestacks.BSUser"


# Application definition

INSTALLED_APPS = [
    'django.contrib.admin',
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'django.contrib.staticfiles',
    # 'clash_royale_app',
    'corsheaders',
    'bluestacks',
    'bluestacks_browser',
    'bs_manage',
    'captcha',
]

MIDDLEWARE_CLASSES = [
    'django.middleware.security.SecurityMiddleware',
    'django.contrib.sessions.middleware.SessionMiddleware',
    'corsheaders.middleware.CorsMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.auth.middleware.SessionAuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
]

ROOT_URLCONF = 'bst_server.urls'

TEMPLATES = [
    {
        'BACKEND': 'django.template.backends.django.DjangoTemplates',
        'DIRS': [],
        'APP_DIRS': True,
        'OPTIONS': {
            'context_processors': [
                'django.template.context_processors.debug',
                'django.template.context_processors.request',
                'django.contrib.auth.context_processors.auth',
                'django.contrib.messages.context_processors.messages',
            ],
        },
    },
]

WSGI_APPLICATION = 'bst_server.wsgi.application'


# Database
# https://docs.djangoproject.com/en/1.9/ref/settings/#databases

# DATABASES = {
#    'default': {
#        'ENGINE': 'django.db.backends.sqlite3',
#        'NAME': os.path.join(BASE_DIR, 'db.sqlite3'),
#    }
# }

#vmware mysql
# if ENVIRONMENT == "win_dev":
#     DATABASES = {
#         'default': {
#             'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
#             'NAME': 'cn_bst_server',  # Or path to database file if using sqlite3.
#             # 'NAME': 'preview_cn_bst_server',  # Or path to database file if using sqlite3.
#             'USER': 'root',  # Not used with sqlite3.
#             'PASSWORD': 'root',  # Not used with sqlite3.
#             # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
#             'HOST': '192.168.48.128',
#             # Set to empty string for localhost. Not used with sqlite3.
#             'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
#             'ATOMIC_REQUEST': True,
#         }
#     }
#     DEBUG = True

# aliyun
if ENVIRONMENT == "aliyun":
    DATABASES = {
        'default': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        }
    }
    # DEBUG = True


if ENVIRONMENT == "win_test":
    DATABASES = {
        'default': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.

            'NAME': 'preview_cn_bst_server',  # Or path to database file if using sqlite3.
            # 'NAME': 'cn_bst_server_use_to_migrate',
            # 'NAME': 'cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscntest',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016test',  # Not used with sqlite3.
            'HOST': 'rdsk75k0anuj28956uzlo.mysql.rds.aliyuncs.com',       #mysql_test cn_bst_server

            # 'USER': 'bluestackscn_tmp',  # Not used with sqlite3.
            # 'PASSWORD': 'Bluestacks2016_tmp',  # Not used with sqlite3.
            # 'HOST': 'rds7d5s0r6101zr11097o.mysql.rds.aliyuncs.com',         #mysql_official cn_bst_server

            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        },
        'production': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'tmp_cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097o.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        },
        'backup': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'backup_cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097o.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        }
    }
    DEBUG = True

if ENVIRONMENT == "aliyun_test":
    DATABASES = {
        'default': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscntest',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016test',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rdsk75k0anuj28956uzl.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        },
        'production': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'tmp_cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097o.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        },
        'backup': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'backup_cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        }
    }
    DEBUG = True

if ENVIRONMENT == "aliyun_test_preview":
    DATABASES = {
        'default': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'preview_cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscntest',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016test',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rdsk75k0anuj28956uzl.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        },
        'production': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        },
        'backup': {
            'ENGINE': 'django.db.backends.mysql',  # Add 'postgresql_psycopg2', 'mysql', 'sqlite3' or 'oracle'.
            'NAME': 'backup_cn_bst_server',  # Or path to database file if using sqlite3.
            'USER': 'bluestackscn',  # Not used with sqlite3.
            'PASSWORD': 'Bluestacks2016',  # Not used with sqlite3.
            # 'HOST': 'rdsmizy48ivz81cwa9uvt.mysql.rds.aliyuncs.com',
            'HOST': 'rds7d5s0r6101zr11097.mysql.rds.aliyuncs.com',
            # Set to empty string for localhost. Not used with sqlite3.
            'PORT': '3306',  # Set to empty string for default. Not used with sqlite3.
            'ATOMIC_REQUEST': True,
        }
    }
    DEBUG = True

# Password validation
# https://docs.djangoproject.com/en/1.9/ref/settings/#auth-password-validators

AUTH_PASSWORD_VALIDATORS = [
    {
        'NAME': 'django.contrib.auth.password_validation.UserAttributeSimilarityValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.MinimumLengthValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.CommonPasswordValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.NumericPasswordValidator',
    },
]


# Internationalization
# https://docs.djangoproject.com/en/1.9/topics/i18n/

# LANGUAGE_CODE = 'en-us'

# TIME_ZONE = 'UTC'

LANGUAGE_CODE = 'zh-cn'

TIME_ZONE = 'Asia/Shanghai'


USE_I18N = True

USE_L10N = True

USE_TZ = False


# Static files (CSS, JavaScript, Images)
# https://docs.djangoproject.com/en/1.9/howto/static-files/



STATIC_URL = '/static/'
STATIC_ROOT = os.path.join(BASE_DIR, 'static_all')
STATICFILES_DIRS = [
    os.path.join(BASE_DIR, 'static'),
]



TEMPLATE_DIRS = (
    # os.path.join(BASE_DIR, 'bs_manage/templates'),
    # os.path.join(BASE_DIR, 'bluestacks/templates'),
    os.path.join(BASE_DIR, 'templates'),
)


CORS_ORIGIN_ALLOW_ALL = True
CORS_ORIGIN_REGEX_WHITELIST = ()
CORS_ALLOW_METHODS = (
        'GET',
        'POST',
        'PUT',
        'PATCH',
        'DELETE',
        'OPTIONS'
    )
CORS_ALLOW_HEADERS = (
        'x-requested-with',
        'content-type',
        'accept',
        'origin',
        'authorization',
        'x-csrftoken'
    )
CORS_EXPOSE_HEADERS = ()
CORS_PREFLIGHT_MAX_AGE = 86400
CORS_ALLOW_CREDENTIALS = False
CORS_REPLACE_HTTPS_REFERER = False


BUCKET_APPCENTER = "bst-appcenter"

PUBLIC_OSS_BEIJING_HOST = "oss-cn-beijing.aliyuncs.com"
INTERNAL_OSS_BEIJING_HOST = "oss-cn-beijing-internal.aliyuncs.com"

ALIYUN_IMG_HOST = "img-cn-beijing.aliyuncs.com"



#if ENVIRONMENT == "aliyun_test":
#    LOGGING = {
#        'version': 1,
#        'disable_existing_loggers': False,
#        'handlers': {
#            'file': {
#                'level': 'DEBUG',
#                'class': 'logging.FileHandler',
#                'filename': '/etc/httpd/logs/bst_server/debug.log',
#                #'filename': 'root/debug.log',
#            },
#        },
#        'loggers': {
#            'django': {
#                'handlers': ['file'],
#                'level': 'DEBUG',
#                'propagate': True,
#            },
#        },
#    }

LOGIN_URL = 'sign_in'

# print "DATABASES",DATABASES