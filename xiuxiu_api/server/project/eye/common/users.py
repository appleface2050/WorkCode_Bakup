# -*- coding: utf-8 -*-

from django.contrib.auth.models import User
from django.contrib.auth import authenticate, login, logout
from rest_framework.authtoken.models import Token
from ..models import UserExtension
from logs import Logs
from images import Images
from objects import Objects
import files
from messages import Messages
from django.db.models.signals import post_save
from datetimes import Datetimes

USER_DEFAULT_ICON = files.BASE_URL_4_IMAGE + "user/default.jpg"
USER_DEFAULT_MALE_ICON = files.BASE_URL_4_IMAGE + "user/default_male.png"
USER_DEFAULT_FEMALE_ICON = files.BASE_URL_4_IMAGE + "user/default_female.png"


class Users(object):
    PACKAGE = "1"
    MASK = "2"

    @staticmethod
    def update_user(user, user_extension_dict, user_dict):
        if user_extension_dict:
            try:
                ue = user.userextension
                for (key, value) in user_extension_dict.items():
                    Objects.set_value(ue, key, value)
                ue.save()
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)
                return Users.get_none()
        if user_dict:
            for (key, value) in user_dict.items():
                Objects.set_value(user, key, value)
            user.save()

        return user

    @staticmethod
    def get_none():
        return Objects.get_none(User)

    @staticmethod
    def get_user_extension_none():
        return Objects.get_none(UserExtension)

    @staticmethod
    def add(user_dict):
        """
        用户名（手机号）， 密码， 昵称
        :param user_dict:
        :return:
        """
        try:
            username = user_dict.get("username", None)
            email = user_dict.get("email", None)
            first_name = user_dict.get("first_name", None)
            last_name = user_dict.get("last_name", None)
            password = user_dict.get("password", None)
            phone = user_dict.get("phone", None)
            nickname = user_dict.get("nickname", None)
            company_id = int(user_dict.get("company_id", 0))

            u = User(
                username=username,
                is_staff=True,
                is_active=True
            )
            u.set_password(password)

            if email:
                u.email = email
            if first_name:
                u.first_name = first_name
            if last_name:
                u.last_name = last_name

            u.save()

            user_id = User.objects.get(username=username).id
            # Logs.print_log("user_id", user_id)
            user_extension = UserExtension.objects.get(user_id=user_id)
            # Logs.print_log("start", "start")
            if phone:
                user_extension.phone = phone
            if nickname:
                user_extension.nickname = nickname
            if company_id:
                user_extension.company_id = company_id
            # Logs.print_log("middle", "middle")
            user_extension.save()
            result = u
            # Logs.print_log("end", "end")
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result = None
        return result

    @staticmethod
    def change_password(user_dict):
        try:
            username = user_dict.get("username", None)
            password = user_dict("password", None)
            u = User.objects.get(username=username)
            u.set_password(password)
            u.save()
            result = u
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            result = None
        return result

    @staticmethod
    def get_user_extension(username):
        try:
            u = UserExtension.objects.get(user__username=username)
            return u
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_none()

    @staticmethod
    def get_user(username):
        try:
            u = User.objects.get(username=username)
            return u
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_none()

    @staticmethod
    def get_user_by_id(user_id):
        try:
            u = User.objects.get(id=user_id)
            return u
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_none()

    @staticmethod
    def get_user_info(user_extension):
        result = dict()
        result = Objects.get_object_info(user_extension)
        temp = Objects.get_object_info(user_extension.user)
        temp["user_id"] = temp["id"]
        result["userextension_id"] = result["id"]
        result.update(temp)
        return result

    @staticmethod
    def does_user_exist(username):
        try:
            user = User.objects.get(username=username)
            if user:
                return True
            else:
                return False
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def is_user_valid(username, password):
        user = authenticate(username=username, password=password)
        if user is not None and user.is_active:
            return True
        else:
            return False

    @staticmethod
    def user_login(request, user):
        login(request, user)

    @staticmethod
    def user_logout(request):
        logout(request)

    @staticmethod
    def get_user_id_from_token(key):
        try:
            return Token.objects.get(key=key).user_id
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return 0

    @staticmethod
    def does_user_login(user_id):
        try:
            user = Token.objects.get(user_id=user_id)
            if user is not None:
                return True
            else:
                return False
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def check_user_login(user_id, key):
        user_id_from_token = Users.get_user_id_from_token(key)
        if user_id_from_token:
            if user_id_from_token == user_id:
                return True
        return False

    @staticmethod
    def reset_user_extension_win_and_lost_count(user_extension):
        user_extension.win_count = 0
        user_extension.lost_count = 0
        user_extension.save()

    @staticmethod
    def add_user_extension_win_count(user_extension):
        user_extension.win_count += 1
        user_extension.save()

    @staticmethod
    def minus_user_extension_win_count(user_extension):
        user_extension.win_count -= 1
        user_extension.save()

    @staticmethod
    def add_user_extension_lost_count(user_extension):
        user_extension.lost_count += 1
        user_extension.save()

    @staticmethod
    def minus_user_extension_lost_count(user_extension):
        user_extension.lost_count -= 1
        user_extension.save()

    @staticmethod
    def get_user_image(user):
        result = dict()
        if user:
            if user.userextension.big_image:
                user_image = files.BASE_URL_4_IMAGE + user.userextension.big_image.name
                # Logs.print_log("user_image", user_image)
            else:
                if user.userextension.gender == "M":
                    user_image = USER_DEFAULT_MALE_ICON
                else:
                    user_image = USER_DEFAULT_FEMALE_ICON
        else:
            user_image = USER_DEFAULT_ICON
        memory_file = files.Files.get_memory_file(user_image)
        user_image_path = "/".join(user_image.split("/")[:-1])
        user_image_name = user_image.split("/")[-1]
        big_user_image_name = ".".join(user_image_name.split(".")[:-1]) + "_big." + user_image_name.split(".")[-1]
        small_user_image_name = ".".join(user_image_name.split(".")[:-1]) + "_small." + user_image_name.split(".")[-1]
        big_user_image = user_image_path + "/" + big_user_image_name
        small_user_image = user_image_path + "/" + small_user_image_name
        try:
            Images.resize_image(memory_file, big_user_image, 240)
            Images.resize_image(memory_file, small_user_image, 96)
            result["big_user_image"] = big_user_image
            result["small_user_image"] = small_user_image
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            if user.userextension.gender == "M":
                result["big_user_image"] = USER_DEFAULT_MALE_ICON
                result["small_user_image"] = USER_DEFAULT_MALE_ICON
            else:
                result["big_user_image"] = USER_DEFAULT_FEMALE_ICON
                result["small_user_image"] = USER_DEFAULT_FEMALE_ICON

        return result

    @staticmethod
    def get_user_count():
        return User.objects.all().count()

    @staticmethod
    def get_user_extensions_by_company(company_id):
        try:
            return UserExtension.objects.filter(company_id=company_id)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_user_extension_none()

    @staticmethod
    def get_user_extensions_with_company():
        try:
            return UserExtension.objects.filter(company_id__isnull=False)
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_user_extension_none()

    @staticmethod
    def is_get_coupon_flag(user, value):
        try:
            value += ","
            flags = user.userextension.get_coupon_flags
            if flags:
                position = flags.find(value)
                # Logs.print_log("position", position)
                if position == -1:
                    return False
                else:
                    return True
            else:
                return False
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return False

    @staticmethod
    def set_get_coupon_flag(user, value):
        try:
            if user.userextension.get_coupon_flags:
                user.userextension.get_coupon_flags += value + ","
            else:
                user.userextension.get_coupon_flags = value + ","
            user.userextension.save()
            return user
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_none()

    @staticmethod
    def get_user_or_auto_register(user_dict):
        try:
            username = user_dict.get("username", None)
            phone = user_dict.get("phone", None)
            if not username:
                return Users.get_none()
            user = Users.get_user(username)
            if not user:
                user = Users.add(user_dict)
                if not user:
                    return Users.get_none()
                else:
                    data_list = [phone]
                    Messages.send_notification(phone, data_list)
                    return user
            return user
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return Users.get_none()

    @staticmethod
    def get_users_by_datetime(start, end):
        try:
            if start:
                start = Datetimes.transfer_datetime(start, is_utc=False)
                start = Datetimes.naive_to_aware(start)
                users = UserExtension.objects.filter(created_at__gte=start)
            else:
                users = UserExtension.objects.all()
            if end:
                end = Datetimes.transfer_datetime(end, is_utc=False)
                end = Datetimes.naive_to_aware(end)
                users = users.filter(created_at__lte=end)
            return users
        except Exception as ex:
            Logs.print_current_function_name_and_line_number(ex)
            return User.objects.none()

    @staticmethod
    def update_unread_win_and_lost_count(ue, win, lost):
        win_count = int(ue.win_count)
        lost_count = int(ue.lost_count)
        win = int(win)
        lost = int(lost)
        win_count = (win_count - win) if (win_count > win) else 0
        lost_count = (lost_count - lost) if (lost_count > lost) else 0
        ue.win_count = win_count
        ue.lost_count = lost_count
        ue.save()
