#coding=utf-8

import bmemcached

from bst_server.settings import ALIYUN_MEMCACHE_HOST,ALIYUN_MEMCACHE_USERNAME,ALIYUN_MEMCACHE_PASSWORD,ENVIRONMENT,USE_MC_ENVIRONMENT, \
    ALIYUN_MEMCACHE_PREVIEW_HOST, ALIYUN_MEMCACHE_PREVIEW_USERNAME, ALIYUN_MEMCACHE_PREVIEW_PASSWORD

# class Singleton(type):
#     def __init__(cls, name, bases, dict):
#         super(Singleton, cls).__init__(name, bases, dict)
#         cls._instance = None
#
#     def __call__(cls, *args, **kwargs):
#         if cls._instance is None:
#             cls._instance = super(Singleton, cls).__call__(*args, **kwargs)
#         return cls._instance
#
# class AliyunMemcached(object):
#     __metaclass__ = Singleton
#
#     def __init__(self):
#         super(AliyunMemcached)
#         # self.client = bmemcached.Client(('da9b304ea6e0492b.m.cnbjalinu16pub001.ocs.aliyuncs.com:11211', ),
#         #                                 'da9b304ea6e0492b',
#         #                                 'Bluestacks2016')
#         self.client = bmemcached.Client((ALIYUN_MEMCACHE_HOST,),
#                                         ALIYUN_MEMCACHE_USERNAME,
#                                         ALIYUN_MEMCACHE_PASSWORD
#                                         )
#     @classmethod
#     def mgr(cls):
#         return cls._instance.client


# mc_client = AliyunMemcached()

if ENVIRONMENT in USE_MC_ENVIRONMENT and ENVIRONMENT == "aliyun":   #正式环境
    mc_client = bmemcached.Client((ALIYUN_MEMCACHE_HOST,),
                                   ALIYUN_MEMCACHE_USERNAME,
                                   ALIYUN_MEMCACHE_PASSWORD)
elif ENVIRONMENT in USE_MC_ENVIRONMENT and ENVIRONMENT == "aliyun_test_preview":     #预发环境

    mc_client = bmemcached.Client((ALIYUN_MEMCACHE_PREVIEW_HOST,),
                                   ALIYUN_MEMCACHE_PREVIEW_USERNAME,
                                   ALIYUN_MEMCACHE_PREVIEW_PASSWORD)
else: #默认使用aliyun环境的
    mc_client = bmemcached.Client((ALIYUN_MEMCACHE_HOST,),
                                   ALIYUN_MEMCACHE_USERNAME,
                                   ALIYUN_MEMCACHE_PASSWORD)


#
# if __name__ == '__main__':
#     a = AliyunMemcached()
#     AliyunMemcached.mgr().set("abc", "cba", time=3600)
#

