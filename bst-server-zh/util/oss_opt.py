# coding=utf-8

import oss2
import time
import datetime


# ALIYUN_ACCESS_ID = "t8z6fuZ2B1FldqmL"
# ALIYUN_ACCESS_SECRET_KEY = "ES2YRYWBREeBNeGCBczpc63FCLJvc9 "
# ALIYUN_OSS_ENDPOINT = "oss-cn-beijing.aliyuncs.com"

from bst_server.settings import ALIYUN_ACCESS_SECRET_KEY, ALIYUN_ACCESS_ID, ALIYUN_OSS_ENDPOINT

def check_for_last_file_upload_success(url, bucket, uptime, key):
    """
    检查该文件在服务器的uptime时间是否小于等于对应oss文件的时间
    """
    if not url or not bucket or not uptime:
        raise Exception
    auth = oss2.Auth(ALIYUN_ACCESS_ID, ALIYUN_ACCESS_SECRET_KEY)
    service = oss2.Service(auth, ALIYUN_OSS_ENDPOINT)
    bucket = oss2.Bucket(auth, ALIYUN_OSS_ENDPOINT, bucket)

    oss_file_last_modified = 0

    for obj in oss2.ObjectIterator(bucket, prefix=key):
        # print obj.key
        if obj.key == key:
            oss_file_last_modified = obj.last_modified
            break
    # print oss_file_last_modified
    oss_server_time = time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(oss_file_last_modified))
    oss_server_time = datetime.datetime.strptime(oss_server_time, '%Y-%m-%d %H:%M:%S')
    if oss_server_time >= uptime:
        return True
    else:
        return False









