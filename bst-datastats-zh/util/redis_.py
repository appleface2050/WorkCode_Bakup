#-*- coding: utf-8 -*-
import redis

#这里替换为连接的实例host和port
host = '5c1cd0b3b9d84412.redis.rds.aliyuncs.com'
port = 6379

#这里替换为实例id和实例password
user = '5c1cd0b3b9d84412'
pwd = 'Bluestackscn2016'

#连接时通过password参数指定AUTH信息，由user,pwd通过":"拼接而成
r = redis.StrictRedis(host=host, port=port, password=user+':'+pwd)




if __name__ == '__main__':
    print r.keys()
    print r.dbsize()
    print r.hget("package_user_init","qq")
    print type(r.hget("package_user_init","qq"))
    #print r.hgetall("package_user_init")
    print r.hget("package_user_init","com.nianticlabs.pokemongo")











