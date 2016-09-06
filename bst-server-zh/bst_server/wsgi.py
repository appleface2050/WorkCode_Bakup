"""
WSGI config for bst_server project.

It exposes the WSGI callable as a module-level variable named ``application``.

For more information on this file, see
https://docs.djangoproject.com/en/1.9/howto/deployment/wsgi/
"""

import os
import sys

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# from settings import ENVIRONMENT

if sys.platform <> "win32":  
    os.environ['PYTHON_EGG_CACHE'] = '/tmp/.python-eggs'

# if ENVIRONMENT == "aliyun_test_preview":
#     sys.path.append('/var/www/html/preview_bst_server/bst_server')
# else:
#     sys.path.append('/var/www/html/bst_server')

sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


from django.core.wsgi import get_wsgi_application

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

from django.core.management import execute_from_command_line
try:
    execute_from_command_line(['manage.py', 'collectstatic','--noinput'])
except Exception, e:
    print str(e)


application = get_wsgi_application()
