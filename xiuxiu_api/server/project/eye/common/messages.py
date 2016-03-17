# -*- coding: utf-8 -*-

import random
from SDK.yuntongxun.CCPRestSDK import REST
from sessions import Sessions
from logs import Logs

SMS_TEST_FLAG = False

if SMS_TEST_FLAG:
    # 主帐号
    SMS_ACCOUNT_SID = 'aaf98f894dd24f24014dd6fa0ada0356'
    # 主帐号Token
    SMS_ACCOUNT_TOKEN = '7d18400b80f7475caad39bce60daff68'
    # 应用Id
    SMS_APP_ID = '8a48b5514dd25566014dd6fc92940329'
    # 请求端口
    SMS_SERVER_PORT = '8883'
    # 请求地址，格式如下，不需要写http://
    SMS_SERVER_IP = 'sandboxapp.cloopen.com'
    # 短信模板ID
    SMS_TEMPLATE_ID = '1'
    SMS_TEMPLATE_NOTIFICATION_ID = '50212'
    SMS_TEMPLATE_ACTIVITY_NOTIFICATION_ID = '53956'
    SMS_TEMPLATE_MASK_NOTIFICATION_ID = '55415'
else:
    # 主帐号
    SMS_ACCOUNT_SID = '8a48b551473976010147437b66bb051d'
    # 主帐号Token
    SMS_ACCOUNT_TOKEN = 'fb19559f95584bddb49066dd27f7d1fe'
    # 应用Id
    SMS_APP_ID = 'aaf98f894fd44d15014fd51a0e5101e4'
    # 请求端口
    SMS_SERVER_PORT = '8883'
    # 请求地址，格式如下，不需要写http://
    SMS_SERVER_IP = 'app.cloopen.com'
    # 短信模板ID
    SMS_TEMPLATE_VERIFICATION_ID = '37438'
    SMS_TEMPLATE_NOTIFICATION_ID = '50212'
    SMS_TEMPLATE_ACTIVITY_NOTIFICATION_ID = '53956'
    SMS_TEMPLATE_MASK_NOTIFICATION_ID = '55415'

# REST版本号
SMS_VERSION = '2013-12-26'

SMS_INTERVAL = 20


class Messages(object):
    # 创建验证码
    @staticmethod
    def generate_verification_code(length):
        code_list = []
        for i in xrange(10):
            code_list.append(str(i))
        v_code = random.sample(code_list, length)
        return ''.join(v_code)

    # 发送验证码
    @staticmethod
    def send_message_sms(to, data, template_id):
        # 初始化REST SDK
        # data:{验证码，时间}
        result = {}
        try:
            rest = REST(SMS_SERVER_IP, SMS_SERVER_PORT, SMS_VERSION)
            rest.setAccount(SMS_ACCOUNT_SID, SMS_ACCOUNT_TOKEN)
            rest.setAppId(SMS_APP_ID)

            temp = rest.sendTemplateSMS(to, data, template_id)
            if temp['statusCode'] == '000000':
                result['success'] = True
                result['status_code'] = temp['statusCode']
                result['status_message'] = "OK"
            else:
                result['success'] = False
                result['status_code'] = temp['statusCode']
                result['status_message'] = "there is an error, please check it out by the status_code!"
        except Exception as ex:
            result["success"] = False
            result['status_code'] = ''
            result["status_message"] = ex.message
        return result

    @staticmethod
    def send_notification(phone, data_list, template_id=SMS_TEMPLATE_NOTIFICATION_ID):
        result = dict()
        try:
            result["info"] = Messages.send_message_sms(phone, data_list, template_id)
            result["success"] = True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result["success"] = False
            result["info"] = ex.message
        return result

    @staticmethod
    def send_verification_code(key, length=4):
        data = dict()
        verification_code = Messages.generate_verification_code(length)
        Sessions.set_session(key, verification_code)
        session_key = Sessions.get_session_key()
        data["key"] = session_key
        data["verify_code"] = verification_code
        try:
            # datas是有顺序的，所以只能用列表或元组，不能用集合
            datas = [verification_code, SMS_INTERVAL]
            data["info"] = Messages.send_message_sms(key, datas, SMS_TEMPLATE_ID)
            data["success"] = True
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            data["success"] = False
            data["info"] = ex.message
        return data

    @staticmethod
    def check_verification_code(verification_code, session_key, key):
        session = Sessions.get_session(session_key)
        data_decoded = Sessions.get_session_data_decoded(session)
        session_code = data_decoded[key]
        if str(verification_code) == str(session_code):
            return True
        else:
            return False
