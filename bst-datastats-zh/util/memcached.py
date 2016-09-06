#coding=utf-8

import bmemcached

from bst_server.settings import ALIYUN_MEMCACHE_HOST,ALIYUN_MEMCACHE_USERNAME,ALIYUN_MEMCACHE_PASSWORD


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
mc_client = bmemcached.Client((ALIYUN_MEMCACHE_HOST,),
                               ALIYUN_MEMCACHE_USERNAME,
                               ALIYUN_MEMCACHE_PASSWORD
                                        )
#
# if __name__ == '__main__':
#     a = AliyunMemcached()
#     AliyunMemcached.mgr().set("abc", "cba", time=3600)
#

# import redis
#
# #这里替换为连接的实例host和port
# host = '5c1cd0b3b9d84412.redis.rds.aliyuncs.com'
# port = 6379
#
# #这里替换为实例id和实例password
# user = '5c1cd0b3b9d84412'
# pwd = 'Bluestackscn2016'
#
# #连接时通过password参数指定AUTH信息，由user,pwd通过":"拼接而成
# r = redis.StrictRedis(host=host, port=port, password=user+':'+pwd)
