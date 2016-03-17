# -*- coding: utf-8 -*-

import random
from math import floor
from ..models import RedEnvelope
from logs import Logs
from objects import Objects
from django.db.models import Q
from devices import Devices
from datetimes import Datetimes
from red_envelope_configuration import RedEnvelopeConfiguration
from publishes import Publishes
from games import Games
from users import Users
from special_activity import SpecialActivity


class RedEnvelopes(object):
    RED_ENVELOPE_STATE_INVALID = -1
    RED_ENVELOPE_STATE_REQUEST = 0
    RED_ENVELOPE_STATE_VALID = 1

    RED_ENVELOPE_TYPE_DEVICE = 0
    RED_ENVELOPE_TYPE_EXTRA = 1
    RED_ENVELOPE_TYPE_RAIN = 2

    @staticmethod
    def compute_red_envelope(rest, min_money, min_value, max_value, possibility=0.7, factor=10):
        """
        :param rest: 剩余金额
        :param min_money: 最小的金额
        :param min_value: 最小金额变整数
        :param max_value: 最大金额变整数
        :param possibility: 获得红包的机率
        :param factor: 金额变整数时所乘的因子
        :return: 红包金额
        """
        if rest < min_money:
            return 0

        value = random.randint(min_value, max_value)
        possibility_value = min_value + (max_value - min_value) * possibility

        if value > possibility_value:
            return 0
        bonus = float(value) / factor
        if bonus > rest:
            bonus = rest
        bonus = float("%0.2s" % (float(floor(bonus * factor)) / factor))

        return bonus

    @staticmethod
    def compute_red_envelope_for_special(rest):
        if rest <= 0.03:
            return 0.01
        else:
            return RedEnvelopes.compute_red_envelope(rest - 0.03, 1, 27, 1, 100)

    @staticmethod
    def can_get_extra(user_id):
        """
        前4天内是否得过红包（没）， 前4天是否每天都有发布
        :param user_id:
        :return:
        """
        RedEnvelopeConfiguration.set_red_envelope_configuration()

        days = RedEnvelopeConfiguration.RED_ENVELOPE_EXTRA_KEEP - 1
        four_days_ago = Datetimes.get_some_day(days)
        day_start = Datetimes.transfer_datetime(Datetimes.get_day_start(four_days_ago), is_utc=False)
        day_start = Datetimes.naive_to_aware(day_start)

        rds = RedEnvelope.objects.filter(Q(user_id=user_id),
                                         Q(created_at__gte=day_start),
                                         Q(type=RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA),
                                         Q(state=RedEnvelopes.RED_ENVELOPE_STATE_REQUEST) |
                                         Q(state=RedEnvelopes.RED_ENVELOPE_STATE_VALID))

        if rds.count() == 0:
            day_list = Datetimes.get_continuous_days_start_and_end(5)
            for day in day_list:
                if not Publishes.is_somebody_published_someday(user_id, day["start"], day["end"]):
                    return False
            return True
        return False

    @staticmethod
    def has_got_rain(user_id):
        now = Datetimes.get_now()
        start = Datetimes.transfer_datetime(Datetimes.get_day_start(now), is_utc=False)
        start = Datetimes.naive_to_aware(start)
        end = Datetimes.transfer_datetime(Datetimes.get_day_end(now), is_utc=False)
        end = Datetimes.naive_to_aware(end)

        rds = RedEnvelope.objects.filter(Q(user_id=user_id),
                                         Q(created_at__gte=start),
                                         Q(created_at__lte=end),
                                         Q(type=RedEnvelopes.RED_ENVELOPE_TYPE_RAIN),
                                         Q(state=RedEnvelopes.RED_ENVELOPE_STATE_REQUEST) |
                                         Q(state=RedEnvelopes.RED_ENVELOPE_STATE_VALID))
        if rds.count() > 0:
            return True
        else:
            return False

    @staticmethod
    def is_rain_time():
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        now = Datetimes.get_now()
        start = Datetimes.string_to_clock(
            Datetimes.date_to_string(now)) + " " + RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_START_STR
        end = Datetimes.string_to_clock(
            Datetimes.date_to_string(now)) + " " + RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_END_STR
        start_datetime = Datetimes.string_to_clock(start)
        end_datetime = Datetimes.string_to_clock(end)

        if start_datetime <= now <= end_datetime:
            return True
        else:
            return False

    @staticmethod
    def get_rain_start_and_end():
        # configuration = RedEnvelopeConfiguration.get_configuration()
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        result = {}
        dt = Datetimes.get_now()
        start_str = Datetimes.date_to_string(dt) + " " + RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_START_STR
        end_str = Datetimes.date_to_string(dt) + " " + RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_END_STR
        result["start"] = Datetimes.string_to_clock(start_str)
        result["end"] = Datetimes.string_to_clock(end_str)
        return result

    @staticmethod
    def get_rain_count(user_id):
        start_and_end = RedEnvelopes.get_rain_start_and_end()
        start = Datetimes.transfer_datetime(start_and_end["start"], is_utc=False)
        start = Datetimes.naive_to_aware(start)
        end = Datetimes.transfer_datetime(start_and_end["end"], is_utc=False)
        end = Datetimes.naive_to_aware(end)
        rds = RedEnvelope.objects.filter(Q(user_id=user_id),
                                         Q(created_at__gte=start),
                                         Q(created_at__lte=end),
                                         Q(type=RedEnvelopes.RED_ENVELOPE_TYPE_RAIN),
                                         Q(state=RedEnvelopes.RED_ENVELOPE_STATE_REQUEST) |
                                         Q(state=RedEnvelopes.RED_ENVELOPE_STATE_VALID))
        return rds.count()

    @staticmethod
    def get_valid_and_request_bonus(start, end, red_type=None):
        """
        :param start:  起始时间， 如 2015-9-1 00:00:00
        :param end: 结束时间， 如 2015-9-30 23:59:59
        :param red_type: 红包类型，默认为连续红包
        :return: 一定时间内已经被申请（包括已经发放的和正在发放的红包）的红包金额
        """
        result = 0
        if not red_type:
            red_type = RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA

        start = Datetimes.transfer_datetime(start, is_utc=False)
        start = Datetimes.naive_to_aware(start)
        end = Datetimes.transfer_datetime(end, is_utc=False)
        end = Datetimes.naive_to_aware(end)

        pools = RedEnvelope.objects.filter(
            Q(created_at__gte=start),
            Q(created_at__lte=end),
            Q(type=red_type),
            Q(state=RedEnvelopes.RED_ENVELOPE_STATE_REQUEST) | Q(state=RedEnvelopes.RED_ENVELOPE_STATE_VALID)
        )
        if pools.count() == 0:
            return result
        else:
            for p in pools:
                result += p.bonus
        return result

    @staticmethod
    def get_invalid_bonus(start, end, red_type=RED_ENVELOPE_TYPE_EXTRA):
        """
        :param start:  起始时间， 如 2015-9-1 00:00:00
        :param end: 结束时间， 如 2015-9-30 23:59:59
        :param red_type: 红包的类型，默认为连续红包
        :return: 一定时间内已经被申请的无效（红包雨或连续红包中的无效红包,不包括设备红包，因为设备红包没有限额）红包金额
        """
        result = 0

        start = Datetimes.transfer_datetime(start, is_utc=False)
        start = Datetimes.naive_to_aware(start)
        end = Datetimes.transfer_datetime(end, is_utc=False)
        end = Datetimes.naive_to_aware(end)

        rds = RedEnvelope.objects.filter(
            created_at__gte=start,
            created_at__lte=end,
            state=RedEnvelopes.RED_ENVELOPE_STATE_INVALID,
            type=red_type
        )
        if rds.count() == 0:
            return result
        for r in rds:
            result += r.bonus
        return result

    @staticmethod
    def get_info(red_envelope):
        try:
            result = Objects.get_object_info(red_envelope)
            return result
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return dict()

    @staticmethod
    def generate(bonus, user_id, device_id, rd_type, rd_state=None):
        """
        :param bonus:  红包金额
        :param user_id:  用户ID
        :param device_id:  设备ID
        :param rd_type:  红包类型
        :param rd_state: 红包状态
        :return: 创建新的红包
        """
        if not rd_state:
            rd_state = RedEnvelopes.RED_ENVELOPE_STATE_REQUEST
        rd = RedEnvelope(bonus=bonus, user_id=user_id, device_id=device_id, type=rd_type, state=rd_state)
        rd.save()
        return rd

    @staticmethod
    def get4device(sequence, user_id):
        """
        新用户（第一次发布的用户）和 新设备（第一次发布的设备）获取设备红包
        :param sequence:  设备UUID
        :param user_id:  用户ID
        :return: 红包数据，获取红包是否成功，说明信息
        """
        result = {}
        # configuration = RedEnvelopeConfiguration.get_configuration()
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        device = Devices.get(sequence)
        rd_type = RedEnvelopes.RED_ENVELOPE_TYPE_DEVICE
        rd_state = RedEnvelopes.RED_ENVELOPE_STATE_REQUEST
        bonus = RedEnvelopeConfiguration.RED_ENVELOPE_BY_DEVICE
        if not device:
            result["bonus"] = 0
            result["info"] = "the device does not exist"
            result["success"] = False
            return result

        if device:
            publishes_device = Publishes.get_publishes_by_device(device.id)
        else:
            publishes_device = None
        if user_id:
            publishes_user = Publishes.get_publishes_by_user(user_id)
        else:
            publishes_user = None

        if not publishes_device and not publishes_user:
            RedEnvelopes.generate(bonus, user_id, device.id, rd_type, rd_state)
            result["bonus"] = bonus
            result["info"] = "OK"
            result["success"] = True
        else:
            if publishes_device:
                result["bonus"] = 0
                result["info"] = "the device has got a bonus"
                result["success"] = True
            else:
                result["bonus"] = 0
                result["info"] = "the user has published before"
                result["success"] = False

        return result

    @staticmethod
    def get4extra(sequence, user_id):
        """
        :param sequence:  设备UUID， 获取连续红包时必须连接设备
        :param user_id:  用户ID
        :return: 红包金额，说明信息，是否成功
        在本月内， 根据剩余金额和红包限额获取红包， 并创建新的红包（请求状态)
        """
        # configurations = RedEnvelopeConfiguration.get_configuration()
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        extra_threshold = RedEnvelopeConfiguration.RED_ENVELOPE_EXTRA_THRESHOLD
        result = {}
        device = Devices.get(sequence)
        rd_type = RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA
        rd_state = RedEnvelopes.RED_ENVELOPE_STATE_REQUEST
        can = RedEnvelopes.can_get_extra(user_id)
        if not device or not can:
            result["bonus"] = 0
            result["info"] = "no device or no extra red envelope"
            result["success"] = True
            return result
        try:
            time_limit = Datetimes.get_month_range()
            start = time_limit["start"]
            end = time_limit["end"]
            given_bonus = RedEnvelopes.get_valid_and_request_bonus(start, end, RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA)
            rest = extra_threshold - given_bonus
            factor = RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR
            min_money = RedEnvelopeConfiguration.RED_ENVELOPE_EXTRA_MIN
            max_money = RedEnvelopeConfiguration.RED_ENVELOPE_EXTRA_MAX
            min_value = min_money * factor
            max_value = max_money * factor
            possibility = RedEnvelopeConfiguration.RED_ENVELOPE_EXTRA_POSSIBILITY
            bonus = RedEnvelopes.compute_red_envelope(
                rest,
                min_money,
                min_value,
                max_value,
                possibility,
                factor
            )
            if bonus == 0:
                result["bonus"] = bonus
                result["info"] = "no bonus it is possible"
                result["success"] = True
            else:
                RedEnvelopes.generate(bonus, user_id, device.id, rd_type, rd_state)
                result["bonus"] = bonus
                result["info"] = "OK"
                result["success"] = True
        except Exception as e:
            result["bonus"] = 0
            result["info"] = e.message
            result["success"] = False
        return result

    @staticmethod
    def get4rain(sequence, user_id):
        """
        :param sequence:  设备UUID
        :param user_id:  用户ID
        :return: 红包雨 -- 是否成功， 说明信息，红包金额
        查看红包雨是否已经发放了足够数量； 查看今天是否获取了请求红包或有效红包；
        """
        result = {}
        RedEnvelopeConfiguration.set_red_envelope_configuration()

        bonus = 0
        user = Users.get_user_by_id(user_id)
        phone = user.username

        if SpecialActivity.exist_in_99(phone):
            today = Datetimes.get_now()
            today_start = Datetimes.get_day_start(today)
            today_end = Datetimes.get_day_end(today)
            today_red_envelope = RedEnvelopes.get_valid_or_request_red_envelope(
                today_start, today_end, RedEnvelopes.RED_ENVELOPE_TYPE_RAIN)
            rest = 0.27 - today_red_envelope
            min_money = 0.01
            min_value = 1
            max_value = 27
            possibility = 1
            factor = 100
            bonus = RedEnvelopes.compute_red_envelope(
                rest,
                min_money,
                min_value,
                max_value,
                possibility,
                factor
            )
        elif SpecialActivity.exist_in_119(phone):
            today = Datetimes.get_now()
            today_start = Datetimes.get_day_start(today)
            today_end = Datetimes.get_day_end(today)
            today_red_envelope = RedEnvelopes.get_valid_or_request_red_envelope(
                today_start, today_end, RedEnvelopes.RED_ENVELOPE_TYPE_RAIN)
            rest = 0.32 - today_red_envelope
            min_money = 0.01
            min_value = 1
            max_value = 32
            possibility = 1
            factor = 100
            bonus = RedEnvelopes.compute_red_envelope(
                rest,
                min_money,
                min_value,
                max_value,
                possibility,
                factor
            )
        else:

            count = RedEnvelopes.get_rain_count(user_id)
            if count >= RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_COUNT:
                result["success"] = False
                result["info"] = "the rain stopped"
                result["bonus"] = 0
                return result

            if RedEnvelopes.has_got_rain(user_id):
                result["success"] = False
                result["info"] = "you have got the rain today"
                result["bonus"] = 0
                return result
            device = Devices.get(sequence)
            if not device:
                result["success"] = False
                result["info"] = "There is no device"
                result["bonus"] = 0
                return result
            rd_type = RedEnvelopes.RED_ENVELOPE_TYPE_RAIN
            rd_state = RedEnvelopes.RED_ENVELOPE_STATE_REQUEST

            start_and_end = RedEnvelopes.get_rain_start_and_end()
            start = start_and_end["start"]
            end = start_and_end["end"]
            given_bonus = RedEnvelopes.get_valid_and_request_bonus(start, end, rd_type)
            rest = RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_THRESHOLD - given_bonus
            factor = RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR
            min_money = RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_MIN
            min_value = RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_MIN * factor
            max_value = RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_MAX * factor
            bonus = RedEnvelopes.compute_red_envelope(
                rest,
                min_money,
                min_value,
                max_value,
                RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_POSSIBILITY,
                factor
            )
        result["bonus"] = bonus
        if bonus == 0:
            result["success"] = False
            result["info"] = "There is no enough money for red envelope or it is possible"
        else:
            RedEnvelopes.generate(bonus, user_id, device.id, rd_type, rd_state)
            result["success"] = True
            result["info"] = "OK"
        return result

    @staticmethod
    def get_info_by_user(user_id):
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        result = dict()
        if user_id == 0:
            result["info"] = "user id is needed"
            result["success"] = False
            return result
        publishes = Publishes.get_shown_publishes(user_id).filter(user_id=user_id).order_by("-id")
        rds = []
        for p in publishes:
            try:
                res = RedEnvelope.objects.filter(
                    publish_id=p.id,
                    user_id=user_id,
                    state=RedEnvelopes.RED_ENVELOPE_STATE_VALID)
                for re in res:
                    rds.append(re)
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)

        result_list = list()
        total = 0
        not_withdraw_total = 0
        for rd in rds:
            total += rd.bonus
            if not rd.is_withdraw:
                not_withdraw_total += rd.bonus
            temp = dict()
            temp["bonus"] = rd.bonus
            temp["datetime"] = Datetimes.clock_to_string(Datetimes.utc2east8(rd.changed_at))
            temp["is_withdraw"] = rd.is_withdraw
            if rd.type == RedEnvelopes.RED_ENVELOPE_TYPE_DEVICE:
                temp["type"] = u"设备红包"
            elif rd.type == RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA:
                temp["type"] = u"连续红包"
            elif rd.type == RedEnvelopes.RED_ENVELOPE_TYPE_RAIN:
                temp["type"] = u"红包雨"
            result_list.append(temp)

        result["count"] = len(rds)
        result["total"] = float(
            round(total * RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR)) / RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR
        result["not_withdraw_total"] = float(round(
            not_withdraw_total * RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR)) / RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR
        result["red_envelope"] = result_list
        result["success"] = True
        result["info"] = "OK"
        return result

    @staticmethod
    def get_request_by_user_and_type(user_id, rd_type):
        state = RedEnvelopes.RED_ENVELOPE_STATE_REQUEST
        result = None
        rds = RedEnvelope.objects.filter(user_id=user_id,
                                         state=state,
                                         type=rd_type).order_by('-id')
        count = rds.count()
        i = 0
        if count > 1:
            for rd in rds:
                # 只取最近的那一个
                if i == 0:
                    result = rd
                    i += 1
                    continue
                i += 1
                rd.state = RedEnvelopes.RED_ENVELOPE_STATE_INVALID
                rd.save()
            return result
        elif count >= 1:
            return rds[0]
        else:
            return None

    @staticmethod
    def get_red_envelope(sequence, user_id):
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        result = dict()
        red_envelope_info = RedEnvelopes.get_info_by_user(user_id)
        try:
            total_by_user = float(red_envelope_info["total"])
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            total_by_user = 0
        if total_by_user >= RedEnvelopeConfiguration.RED_ENVELOPE_PERSON_THRESHOLD:
            result['extra'] = 0
            result['rain'] = 0
            result['device'] = 0
            result['info'] = 'personal bonus is enough'
            result['success'] = False
            return result

        if not sequence or not user_id:
            result['extra'] = 0
            result['rain'] = 0
            result['device'] = 0
            result['info'] = 'no sequence or no user_id'
            result['success'] = False
            return result
        extra = RedEnvelopes.get4extra(sequence, user_id)
        if RedEnvelopes.is_rain_time():
            rain = RedEnvelopes.get4rain(sequence, user_id)
        else:
            rain = {
                "bonus": 0,
                "success": False,
                "info": "it is not rain time, the start time is " + Datetimes.time_to_string(
                    RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_START
                ) + " and the end time is " + Datetimes.time_to_string(
                    RedEnvelopeConfiguration.RED_ENVELOPE_RAIN_END
                )
            }
        device = RedEnvelopes.get4device(sequence, user_id)
        result['extra'] = extra['bonus']
        result['rain'] = rain['bonus']
        result['device'] = device['bonus']
        result['info'] = extra['info'] + "; " + rain["info"] + "; " + device["info"]
        result["success"] = True
        return result

    @staticmethod
    def bind(red_envelope, publish_id):
        red_envelope.state = RedEnvelopes.RED_ENVELOPE_STATE_VALID
        red_envelope.publish_id = publish_id
        red_envelope.save()

    @staticmethod
    def set_device_publish_state(sequence):
        result = dict()
        try:
            device = Devices.get(sequence)
            if device:
                device.is_published = True
                device.save()
                result["info"] = "set is_published successfully"
                result["success"] = True
            else:
                result["success"] = False
                result["info"] = "device does not exist"
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result["success"] = False
            result["info"] = ex.message
        return result

    @staticmethod
    def get_valid_red_envelope(user_id):
        result = dict()
        rds = RedEnvelope.objects.filter(user_id=user_id, state=RedEnvelopes.RED_ENVELOPE_STATE_VALID)
        extra_bonus = 0
        device_bonus = 0
        rain_bonus = 0
        game_bonus = 0
        total_bonus = 0
        withdraw_bonus = 0
        not_withdraw_bonus = 0
        for rd in rds:
            total_bonus += rd.bonus
            if rd.is_withdraw:
                withdraw_bonus += rd.bonus
            else:
                not_withdraw_bonus += rd.bonus
            if int(rd.type) == RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA:
                extra_bonus += rd.bonus
            elif int(rd.type) == RedEnvelopes.RED_ENVELOPE_TYPE_DEVICE:
                device_bonus += rd.bonus
            elif int(rd.type) == RedEnvelopes.RED_ENVELOPE_TYPE_RAIN:
                rain_bonus += rd.bonus
            elif int(rd.type) == Games.RED_ENVELOPE_TYPE:
                game_bonus += rd.bonus

        result["extra_bonus"] = extra_bonus
        result["device_bonus"] = device_bonus
        result["rain_bonus"] = rain_bonus
        result["game_bonus"] = game_bonus
        result["total_bonus"] = total_bonus
        result["withdraw_bonus"] = withdraw_bonus
        result["not_withdraw_bonus"] = not_withdraw_bonus
        return result

    @staticmethod
    def get_valid_user_ids():
        return RedEnvelope.objects.filter(state=RedEnvelopes.RED_ENVELOPE_STATE_VALID).values('user_id').distinct()

    @staticmethod
    def get_request_red_envelope(user_id):
        return RedEnvelopes.get_request_by_user_and_type(user_id, RedEnvelopes.RED_ENVELOPE_TYPE_DEVICE) | \
               RedEnvelopes.get_request_by_user_and_type(user_id, RedEnvelopes.RED_ENVELOPE_TYPE_EXTRA) | \
               RedEnvelopes.get_request_by_user_and_type(user_id, RedEnvelopes.RED_ENVELOPE_TYPE_RAIN)

    @staticmethod
    def get_red_envelope_by_publish(publish_id):
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        result = 0
        rds = RedEnvelope.objects.filter(publish_id=publish_id, state=RedEnvelopes.RED_ENVELOPE_STATE_VALID)
        count = rds.count()
        if count == 0:
            return result

        for rd in rds:
            result += rd.bonus

        result = float(round(result * RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR)) \
                 / RedEnvelopeConfiguration.RED_ENVELOPE_FACTOR
        return result

    @staticmethod
    def has_game_red_envelope(user_id, start, end):
        rds = RedEnvelope.objects.filter(
            user_id=user_id,
            created_at__gte=start,
            created_at__lte=end,
            type=Games.RED_ENVELOPE_TYPE
        )
        if rds.count() > 0:
            return True
        else:
            return False

    @staticmethod
    def can_get_red_envelope(user_id):
        RedEnvelopeConfiguration.set_red_envelope_configuration()
        data = RedEnvelopes.get_valid_red_envelope(user_id)
        total = float(data.get("total_bonus"))
        threshold = float(RedEnvelopeConfiguration.RED_ENVELOPE_PERSON_THRESHOLD)
        if total >= threshold:
            return False
        return True
