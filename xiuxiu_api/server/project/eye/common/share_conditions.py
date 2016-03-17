# coding: utf-8

from django.db.models import Count
from ..models import ShareStatistics
from datetimes import Datetimes
from logs import Logs


class ShareConditions(object):
    BEHAVIOR_NAMES = {
        u"CF_QRCSB_WXT_0": u"朋友圈（二维码）分享",
        u"CF_QRCSB_WXT_1": u"朋友圈（二维码）分享成功",
        u"CF_QRCSB_WXT_2": u"朋友圈（二维码）打开下载页",
        u"CF_QRCSB_WXT_3": u"朋友圈（二维码）点击下载",
        u"CF_URLSB_WXS_0": u"微信聊天（URL）分享",
        u"CF_URLSB_WXS_1": u"微信聊天（URL）分享成功",
        u"CF_URLSB_WXS_2": u"微信聊天（URL）打开下载页",
        u"CF_URLSB_WXS_3": u"微信聊天（URL）点击下载",
        u"CF_URLSB_WB_0": u"微博（URL）分享",
        u"CF_URLSB_WB_1": u"微博（URL）分享成功",
        u"CF_URLSB_WB_2": u"微博（URL）打开下载页",
        u"CF_URLSB_WB_3": u"微博（URL）点击下载",
    }

    @staticmethod
    def get_none():
        return ShareStatistics.objects.none()

    @staticmethod
    def get_all():
        return ShareStatistics.objects.all()

    @staticmethod
    def get_by_datetime(share_statistics, start, end):
        result = ShareConditions.get_none()
        try:
            result = share_statistics

            if start:
                start = Datetimes.transfer_datetime(start, is_utc=False)
                start = Datetimes.naive_to_aware(start)
                result = result.filter(created_at__gte=start)

            if end:
                end = Datetimes.transfer_datetime(end, is_utc=False)
                end = Datetimes.naive_to_aware(end)
                result = result.filter(created_at__lte=end)
            return result
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return result

    @staticmethod
    def get_by_user_id(share_statistics, user_ids):
        return share_statistics.filter(user_id__in=user_ids)

    @staticmethod
    def get_group_by_behavior_name(share_statistics):
        result = list()
        share = 0
        share_success = 0
        open_download = 0
        click_download = 0
        data = share_statistics.values("behavior_name").annotate(count=Count("behavior_name"))
        print data
        for d in data:
            temp = dict()
            name = ShareConditions.BEHAVIOR_NAMES.get(d["behavior_name"], None)
            if name:
                temp["name"] = name
                temp["count"] = d["count"]
                result.append(temp)
            if d["behavior_name"] in ShareConditions.BEHAVIOR_NAMES.keys() and d["behavior_name"].endswith("_0"):
                share += d["count"]
            if d["behavior_name"] in ShareConditions.BEHAVIOR_NAMES.keys() and d["behavior_name"].endswith("_1"):
                share_success += d["count"]
            if d["behavior_name"] in ShareConditions.BEHAVIOR_NAMES.keys() and d["behavior_name"].endswith("_2"):
                open_download += d["count"]
            if d["behavior_name"] in ShareConditions.BEHAVIOR_NAMES.keys() and d["behavior_name"].endswith("_3"):
                click_download += d["count"]
        result.append({"name": u"分享", "count": share})
        result.append({"name": u"分享成功", "count": share_success})
        result.append({"name": u"打开下载页", "count": open_download})
        result.append({"name": u"点击下载", "count": click_download})
        return result

    @staticmethod
    def get_group_by(share_statistics, order_by="user_id"):
        result = ShareConditions.get_none()
        try:
            result = share_statistics

            return result.values(
                "user_id", "behavior_name").annotate(
                count=Count("user_id", "behavior_name")
            ).order_by(order_by)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return result

