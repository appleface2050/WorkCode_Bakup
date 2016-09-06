"""
WSGI config for bst_server project.

It exposes the WSGI callable as a module-level variable named ``application``.

For more information on this file, see
https://docs.djangoproject.com/en/1.9/howto/deployment/wsgi/
"""

import os
import sys

if sys.platform <> "win32":  
    os.environ['PYTHON_EGG_CACHE'] = '/tmp/.python-eggs'

# sys.path.append('/var/www/html/bst-datastats-zh')
sys.path.append('/var/www/html/master/bst-datastats-zh')

from django.core.wsgi import get_wsgi_application

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "bst_server.settings")

application = get_wsgi_application()
