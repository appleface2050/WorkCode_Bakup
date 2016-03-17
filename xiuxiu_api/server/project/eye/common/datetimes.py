# -*- coding: utf-8 -*-
import datetime
import time
import calendar
import pytz
from logs import Logs
from django.utils.timezone import utc, make_aware, is_naive, is_aware

DATETIME_FORMAT = "%Y-%m-%d %H:%M:%S"
DATE_FORMAT = "%Y-%m-%d"
DATE_FORMAT_FOR_FORUM = "%y.%m.%d"
TIME_FORMAT = "%H:%M:%S"

ZERO_CLOCK = "0:00:00"
SIX_CLOCK = "6:00:00"
TWELVE_CLOCK = "12:00:00"
EIGHTEEN_CLOCK = "18:00:00"


class Datetimes(object):
    @staticmethod
    def get_2010():
        return datetime.datetime(2010, 1, 1, 0, 0, 0)

    @staticmethod
    def get_now():
        return datetime.datetime.now()

    @staticmethod
    def get_date(someday):
        return someday.date()

    @staticmethod
    def get_time(someday):
        return someday.time()

    @staticmethod
    # days, seconds, microseconds
    def get_delta_time(start_datetime, end_datetime=datetime.datetime.now()):
        start_datetime = Datetimes.aware_to_naive(start_datetime)
        end_datetime = Datetimes.aware_to_naive(end_datetime)
        return end_datetime - start_datetime

    @staticmethod
    def get_some_day(days_before, seconds_before=0, base_day=None):
        """
        几天前的日期
        :param days_before: 当some为正数时是以前的日期，是负数时是以后的日期
        :param seconds_before: 秒
        :param base_day: 起始时间
        :return:
        """
        if not base_day:
            base_day = Datetimes.get_now()
        delta_day = datetime.timedelta(days=days_before, seconds=seconds_before)
        the_day = base_day - delta_day
        return the_day

    @staticmethod
    def string_to_clock(clock_string):
        return datetime.datetime.strptime(clock_string, DATETIME_FORMAT)

    @staticmethod
    def clock_to_string(clock):
        return clock.strftime(DATETIME_FORMAT)

    @staticmethod
    def string_to_time(clock_string):
        return datetime.datetime.strptime(clock_string, TIME_FORMAT).time()

    @staticmethod
    def time_to_string(clock):
        return clock.strftime(TIME_FORMAT)

    @staticmethod
    def date_to_string(dt):
        return dt.strftime(DATE_FORMAT)

    @staticmethod
    def date_to_string_for_forum(dt):
        return dt.strftime(DATE_FORMAT_FOR_FORUM)

    @staticmethod
    def string_to_date(dt_string):
        return datetime.datetime.strptime(dt_string, DATE_FORMAT).date()

    @staticmethod
    def get_day_start(dt):
        day_date = dt.date()
        return Datetimes.string_to_clock(Datetimes.date_to_string(day_date) + " 00:00:00")

    @staticmethod
    def get_day_end(dt):
        day_date = dt.date()
        return Datetimes.string_to_clock(Datetimes.date_to_string(day_date) + " 23:59:59")

    @staticmethod
    def transfer_datetime(the_time, is_utc=True):
        """
        UTC时间与本地时间的转换
        :param the_time:
        :param is_utc:
        :return:
        """
        now_stamp = time.time()
        local_time = datetime.datetime.fromtimestamp(now_stamp)
        utc_time = datetime.datetime.utcfromtimestamp(now_stamp)
        offset = local_time - utc_time
        if is_utc:  # utc 转成本地时间
            the_datetime = the_time + offset
        else:  # 本地时间转换成utc时间
            the_datetime = the_time - offset
        return the_datetime

    @staticmethod
    def aware_to_naive(dt):
        """
        offset-aware 包含时区的时间； offset-naive 不包含时区的时间
        :param dt:  包含时区的时间
        :return: 不包含时区的时间
        """
        if is_aware(dt):
            return dt.replace(tzinfo=None)
        else:
            return dt

    @staticmethod
    def naive_to_aware(dt):
        """
        offset-aware 包含时区的时间； offset-naive 不包含时区的时间
        :param dt:  包含时区的时间
        :return: 不包含时区的时间
        """
        if is_naive(dt):
            return make_aware(dt)
        else:
            return dt

    @staticmethod
    def get_month_range():
        """
        :return: 本月的起始时间，如 2015-9-1 00:00:00  -- 2015-9-30 23:59:59
        """
        result = dict()
        now = datetime.datetime.now()
        year = now.year
        month = now.month
        month_range = calendar.monthrange(year, month)
        start = str(year) + "-" + str(month) + "-1 00:00:00"
        end = str(year) + "-" + str(month) + "-" + str(month_range[1]) + " 23:59:59"
        result["start"] = Datetimes.string_to_clock(start)
        result["end"] = Datetimes.string_to_clock(end)
        return result

    @staticmethod
    def utc2east8(utc_st):
        return utc_st.replace(tzinfo=pytz.utc).astimezone(pytz.timezone('Asia/Shanghai'))

    @staticmethod
    def get_continuous_days_start_and_end(days, base_day=None):
        result = []
        if not base_day:
            base_day = Datetimes.get_now()

        if days > 0:
            for i in range(1, days):
                day = {}
                dt = Datetimes.get_some_day(i, base_day)
                day["start"] = Datetimes.get_day_start(dt)
                day["end"] = Datetimes.get_day_end(dt)
                result.append(day)
        return result

    @staticmethod
    def get_show_time(created_at):
        # utc时间转换为本地时间
        created_at = Datetimes.transfer_datetime(created_at)
        # 获取本地的当时时间
        now = Datetimes.get_now()
        # 获取时间差
        delta = now - created_at
        seconds = delta.seconds
        days = delta.days
        if days > 2:
            return Datetimes.utc2east8(created_at).strftime("%Y/%m/%d")
        elif days > 1:
            return u"前天"
        elif days > 0:
            return u"昨天"
        else:
            hour = seconds / 3600
            if hour > 0:
                return str(hour) + u"小时前"
            else:
                return u"刚刚"

    @staticmethod
    def is_out_of_date(local_start_datetime, days=1):
        now = Datetimes.get_now()
        delta = now - local_start_datetime
        if delta.days > days:
            return True
        else:
            return False

    @staticmethod
    def to_utc(dt):
        try:
            dt = Datetimes.transfer_datetime(dt, is_utc=False)
            return dt.replace(tzinfo=utc)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def string_to_utc(dt_string):
        try:
            dt = Datetimes.string_to_clock(dt_string)
            if dt:
                return Datetimes.to_utc(dt)
            else:
                return None
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return None

    @staticmethod
    def set_interval(start_date, start_time, end_date, end_time):
        start_datetime = Datetimes.transfer_datetime(Datetimes.string_to_clock(start_date + " " + start_time),
                                                     is_utc=False)
        end_datetime = Datetimes.transfer_datetime(Datetimes.string_to_clock(end_date + " " + end_time),
                                                   is_utc=False)
        return [start_datetime, end_datetime]

    @staticmethod
    def get_previous_interval():
        now = Datetimes.get_now()
        now_date_string = Datetimes.clock_to_string(Datetimes.get_date(now))
        yesterday = Datetimes.get_some_day(1, 0, now)
        yesterday_date_string = Datetimes.clock_to_string(Datetimes.get_date(yesterday))

        hour = now.hour
        if 0 <= hour < 6:
            interval = Datetimes.set_interval(yesterday_date_string, EIGHTEEN_CLOCK, now_date_string, ZERO_CLOCK)
        elif 6 <= hour < 12:
            interval = Datetimes.set_interval(now_date_string, ZERO_CLOCK, now_date_string, SIX_CLOCK)
        elif 12 <= hour < 18:
            interval = Datetimes.set_interval(now_date_string, SIX_CLOCK, now_date_string, TWELVE_CLOCK)
        else:
            interval = Datetimes.set_interval(now_date_string, TWELVE_CLOCK, now_date_string, EIGHTEEN_CLOCK)
        return interval

    @staticmethod
    def get_weekday(dt):
        return dt.weekday()
