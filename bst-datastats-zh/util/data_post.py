# !/usr/bin/env python
# -*- coding:utf-8 -*-

import Queue
import threading
import time
import requests
import uuid
import random

# URL = "http://127.0.0.1:8001/"
# URL = "http://101.201.42.93:8095/"
URL = "http://data.bluestacks.cn/"


class WorkManager(object):
    def __init__(self, work_num=1000,thread_num=2):
        self.work_queue = Queue.Queue()
        self.threads = []
        self.__init_work_queue(work_num)
        self.__init_thread_pool(thread_num)

    """
        初始化线程
    """
    def __init_thread_pool(self,thread_num):
        for i in range(thread_num):
            self.threads.append(Work(self.work_queue))

    """
        初始化工作队列
    """
    def __init_work_queue(self, jobs_num):
        for i in range(jobs_num):
            self.add_job(do_job, i)

    """
        添加一项工作入队
    """
    def add_job(self, func, *args):
        self.work_queue.put((func, list(args)))#任务入队，Queue内部实现了同步机制

    """
        等待所有线程运行完毕
    """
    def wait_allcomplete(self):
        for item in self.threads:
            if item.isAlive():item.join()

class Work(threading.Thread):
    def __init__(self, work_queue):
        threading.Thread.__init__(self)
        self.work_queue = work_queue
        self.start()

    def run(self):
        #死循环，从而让创建的线程在一定条件下关闭退出
        while True:
            try:
                do, args = self.work_queue.get(block=False)#任务异步出队，Queue内部实现了同步机制
                do(args)
                self.work_queue.task_done()#通知系统任务完成
            except Exception as e:
                print e
                break

def generate_request():
    urls = ["data/engine_install","data/engine_activity","data/emulator_install","data/emulator_activity",
            "data/app_install","data/app_activity"]

    # urls = ["data/app_install","data/app_activity"]

    url = random.sample(urls,1)
    # guid = uuid.uuid1()
    guid = str(random.randint(0,9999))

    if url[0] in ["data/engine_install", "data/emulator_install", "data/app_install"]:
        op = random.sample(["install","uninstall"],1)
    else:
        op = random.sample(["init","abort"],1)


    status = random.sample(["success","fail"],1)
    version = random.sample(["1.0.0.1","1.0.0.2","1.0.0.3"],1)

    data = {'guid': guid,
            'op': op,
            "status": status,
            "version": version
            }

    if url[0] in ["data/app_install","data/app_activity"]:
        package_name = random.sample(["com.supercell.clashroyale.qihoo",
                                      "com.supercell.clashofclans.qihoo",
                                      "zwsUC_6612232_200646f47d43.apk"],1)
        data["package_name"] = package_name

    return url, data


#具体要做的任务
def do_job(args):
    url, data = generate_request()
    # time.sleep(0.1)#模拟处理时间
    # data = {'guid':uuid.uuid1(),
    #         'op':'install',
    #         "status":"success",
    #         "version":"1.0.0"
    #         }
    r = requests.post(URL + url[0], data)
    # print r.content
    # print data
    print threading.current_thread(), list(args)
    time.sleep(0.033)

if __name__ == '__main__':
    start = time.time()
    work_manager = WorkManager(100, 30)   #或者work_manager =  WorkManager(10000, 20)
    work_manager.wait_allcomplete()
    end = time.time()
    print "cost all time: %s" % (end-start)



