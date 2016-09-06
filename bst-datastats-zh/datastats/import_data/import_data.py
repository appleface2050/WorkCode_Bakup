# coding=utf-8

import sys

# from datastats.models import EngineInstall
# from util.sql import make_group, grp
#
#
# def get_install_success_user(start, modes, scope):
#     if modes == "day":
#         # select, groupby = get_sql(scope)
#         group = make_group(scope)
#         groupby = ('GROUP BY %s' % group) if group else ''
#         sql = """
#     SELECT id, COUNT(DISTINCT q.guid)count %s FROM
#     (SELECT id, a.guid,a.op,a.version,a.status,a.channel,b.osver
#     FROM datastats_engineinstall a
#     INNER JOIN view_guid_osver b ON a.guid = b.guid
#     WHERE DATE_FORMAT(a.datetime,'%%%%Y-%%%%m-%%%%d')='%s' )q
#     WHERE q.op='install' AND q.status = 'success'
#     %s
#         """ % (grp(group), start.strftime("%Y-%m-%d"), groupby)
#         print sql
#         ckey = '%s_%s_%s_%s'%(sys._getframe().f_code.co_name,start.strftime("%Y-%m-%d"),modes,group)
#         print ckey
#         res = self.cache.get(ckey,None)
#         data = EngineInstall.objects.raw(sql)
#         for i in data:
#             print i.count





