#coding=utf-8
from __future__ import unicode_literals

from django.db.models import Q
from django.contrib.auth.base_user import AbstractBaseUser, BaseUserManager
from django.forms import ModelForm
from django.utils.translation import ugettext_lazy as _
from django.db import models

from django.utils import timezone

# Create your models here.
from bluestacks.oss_models import OssFile
from util.appcenter import screenshots_modify, app_size_modify, content_chinese_word, modify_date_modify, \
    handle_tojson_type
from util.basemodel import JSONBaseModel

from bst_server.settings import ENVIRONMENT, USE_MC_ENVIRONMENT

import datetime

# from __future__ import unicode_literals

from django.db import models

# Create your models here.
from util.memcached import mc_client


class IconTag(JSONBaseModel):
    title = models.CharField(max_length=64, null=False, unique=True)
    comment = models.CharField(max_length=64, null=True)

    @classmethod
    def get_icon_tag_title_by_id(cls, id):
        if id and cls.objects.filter(pk=id).exists():
            return cls.objects.get(pk=id).title
        else:
            print "error"
            return False


class AppCenterGame(JSONBaseModel):
    """
    配置进去的展示用的游戏
    """
    title = models.CharField(max_length=64, null=False)
    platform_game_ids = models.CharField(max_length=256, null=False, unique=False)
    type = models.CharField(max_length=32, null=True)
    icon_url = models.CharField(max_length=1024, null=True)
    size = models.IntegerField(null=True)
    download_count = models.IntegerField(default=1000, null=False)
    screenshots = models.TextField(max_length=2048, null=True, help_text=u'截图地址')
    version = models.CharField(max_length=32, null=True)
    tags = models.CharField(max_length=512, null=True)
    level = models.FloatField(default=5.0, null=False)
    instruction = models.CharField(max_length=2048, null=True, verbose_name=u'简介' )
    description = models.TextField(help_text=u'描述', null=True)
    status = models.IntegerField(default=1, null=False, verbose_name=u'游戏状态 1表示正常')
    icon_tag_id = models.IntegerField(null=True, verbose_name=u'icon_tag')
    small_icon_url = models.CharField(max_length=1024, null=True)
    search_weight = models.IntegerField(null=True, verbose_name=u'搜索权重')
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    # @classmethod
    # def get_app_center_game_platform_game_id_modified_data(cls):
    #     """
    #     appcentergame里面全部 id plateform_game_id plateform_game_id_list数据
    #     """
    #     result = []
    #     app_center_game_platform_game_id_modified_data = []
    #     if ENVIRONMENT in USE_MC_ENVIRONMENT:
    #         app_center_game_platform_game_id_modified_data = mc_client.get("app_center_game_platform_game_id_modified_data")
    #     if not app_center_game_platform_game_id_modified_data:
    #         q = cls.objects.all()
    #         for i in q:
    #             platform_game_id_list = [int(j) for j in i.platform_game_ids.split(",")]
    #             app_center_game_platform_game_id_modified_data.append(
    #                 {"id":i.id, "platform_game_ids":i.platform_game_ids,"platform_game_id_list":platform_game_id_list}
    #             )
    #         if ENVIRONMENT in USE_MC_ENVIRONMENT:
    #             mc_client.set("app_center_game_platform_game_id_modified_data", app_center_game_platform_game_id_modified_data, time=3600*24)
    #     return app_center_game_platform_game_id_modified_data

    @classmethod
    def find_appcentergame_type_by_platform_game_id(cls, platform_game_id):
        type = ""
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            type = mc_client.get("platform_game_id_type@%s" % str(platform_game_id))
        if not type:
            type = ""
            id = cls.get_id_by_platform_game_id(platform_game_id)
            if cls.objects.filter(pk=id).exists():
                type = handle_tojson_type(cls.objects.get(pk=id).type)
                if ENVIRONMENT in USE_MC_ENVIRONMENT:
                    mc_client.set("platform_game_id_type@%s" % str(platform_game_id), type, time=3600*24)
        return type

    @classmethod
    def get_id_by_platform_game_id(cls, platform_game_id):
        result = {}
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            result = mc_client.get("platform_game_id_vs_appcenterid")
        if not result:
            result = {}
            if platform_game_id in cls.get_all_platform_game_ids():
                all = AppCenterGame.objects.filter(status=True)
                for i in all:
                    for j in i.platform_game_ids.split(","):
                        try:
                            # platform_game_ids.append(int(j))
                            result[int(j)] = int(i.pk)

                        except Exception,e:
                            print e
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("platform_game_id_vs_appcenterid", result, time=3600*24)
        return result.get(int(platform_game_id),None)


    @classmethod
    def get_level_by_id(cls, id):
        """
        通过id获取游戏level
        """
        if cls.objects.filter(pk=id).exists():
            level = cls.objects.get(pk=id).level
            if not level:
                return 0
            else:
                return level
        else:
            return 0

    @classmethod
    def get_sum_download_count(cls, platform_game_ids):
        download_count = 0
        if not platform_game_ids:
            return download_count
        else:
            game_library_ids = platform_game_ids.split(",")
            for game_id in game_library_ids:
                if not game_id:      #防止platform_game_ids写错的情况
                    continue
                download_count += int(GameLibrary.get_download_count_by_id(game_id))
        return download_count

    # @classmethod
    # def get_all_platform_game_ids(cls):
    #     all_platform_game_ids = []
    #     result = AppCenterGame.objects.filter(status=True)
    #     for i in result:
    #         if not i.platform_game_ids:
    #             continue
    #         else:
    #             tmp = i.platform_game_ids.split(",")
    #             for i in tmp:
    #                 if i:           #防止platform_game_ids为"1,2,,,,,"
    #                     all_platform_game_ids.append(int(i))
    #     return set(all_platform_game_ids)


    @classmethod
    def get_all_platform_game_ids(cls):
        all_platform_game_ids = []
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            all_platform_game_ids = mc_client.get("all_platform_game_ids")
        if not all_platform_game_ids:
            all_platform_game_ids = []
            result = AppCenterGame.objects.filter(status=True)
            for i in result:
                if not i.platform_game_ids:
                    continue
                else:
                    tmp = i.platform_game_ids.split(",")
                    for i in tmp:
                        if i:           #防止platform_game_ids为"1,2,,,,,"
                            all_platform_game_ids.append(int(i))
            all_platform_game_ids = set(all_platform_game_ids)
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("all_platform_game_ids", all_platform_game_ids, time=3600*24)
        return all_platform_game_ids


    # @classmethod
    # def find_game9_game(cls, platform_game_id):
    #     """
    #     查找game9的对应的游戏，没有返回为None
    #     """
    #     if not platform_game_id in cls.get_all_platform_game_ids():
    #         return None
    #     else:
    #         game9_platform_id = cls.find_game9_platform_game_id_using_a_platform_game_id_which_is_not_game9_platform(platform_game_id)
    #         if not game9_platform_id:
    #             return None
    #         else:
    #             try:
    #                 game9 = GameLibrary.objects.get(pk=game9_platform_id)
    #                 return game9
    #             except Exception, e:
    #                 print e
    #                 return None

    # @classmethod
    # def find_game9_platform_game_id_using_a_platform_game_id_which_is_not_game9_platform(cls, platform_game_id):
    #     app_center_game_platform_game_id_modified_data = AppCenterGame.get_app_center_game_platform_game_id_modified_data()
    #     for data in app_center_game_platform_game_id_modified_data:
    #         if platform_game_id in data["platform_game_id_list"]:
    #             game9_platform_id = GameLibrary.find_game9_platform_id(data["platform_game_id_list"])
    #             return game9_platform_id
    #     return None




    @classmethod
    def get_all_app_center_game_id(cls):
        ids = []
        result = AppCenterGame.objects.filter(status=True)
        for i in result:
            ids.append(i.pk)
        return ids


    @classmethod
    def search(cls, query):
        if not query:
            return False
        else:
            result = []
            query = query.strip()
            data = cls.objects.filter(title__contains=query)
            for i in data:
                # result.append(i.toJSON())
                result.append(super(AppCenterGame, i).toJSON())
            return result



    @classmethod
    def find_tag(cls, tag_name):
        apps = cls.objects.filter(type__contains=tag_name)
        return apps


    @classmethod
    def if_app_center_id_exists(cls, app_center_id):
        return cls.objects.filter(pk=app_center_id).exists()

    @classmethod
    def get_all_app(cls):
        res = []
        data = cls.objects.all()
        for i in data:
            res.append(i.toJSON_manage())
        return res

    def find_unique_plateform_game_id(self):
        game_id = self.platform_game_ids
        if game_id.find(",") != -1:
            game_id = self.platform_game_ids.split(",")[0]
        return game_id

    def set_status(self, status):
        self.status = status
        self.save()
        return self

    def update_app_data(self, game):
        self.title = game.game_name
        self.type = game.type
        self.icon_url = game.icon_url
        self.size = game.size
        self.download_count = game.download_count
        self.screenshots = game.screenshots
        self.version = game.version
        self.tags = game.tags
        self.level = game.level
        self.instruction = game.instruction
        self.description = game.description
        try:
            self.save()
            return True
        except Exception as e:
            print e
            return False

    # @classmethod
    # def get_app_detail_page_data(cls, id):
    #     if cls.objects.filter(pk=id, status=True).exists():
    #         return cls.objects.get(pk=id)
    #     else:
    #         return False

    @classmethod
    def get_platform_game_ids(cls, id):
        if cls.objects.filter(pk=id, status=True).exists():
            return cls.objects.get(pk=id).platform_game_ids
        else:
            return False

    @classmethod
    def get_app_by_id(cls, id):
        # return cls.objects.filter(pk__in=id_list).values("title","level","icon_url","type","download_count")
        if cls.objects.filter(pk=id).exists():
            return cls.objects.get(pk=id)
        else:
            print "AppCenterGame id not exist",id
            # raise Exception
            return False

    def toJSON_manage(self):
        """
        用于manage数据规整
        """
        d = super(AppCenterGame, self).toJSON()
        #处理type
        type_tmp = []
        if d["type"].find(",") != -1:
            d["type"] = d["type"].split(",")
            for i in d["type"]:
                if i.find(":") != -1:
                    type_tmp.append(i.split(":")[1])
                else:
                    type_tmp.append(i)
        elif d['type'].find(":") != -1:
            type_tmp.append(d['type'].split(":")[1])
        else:
            type_tmp.append(d['type'])
        d["type"] = ",".join(type_tmp)
        d["platform_name"] = GameLibrary.get_platform_name_by_game_ids(d["platform_game_ids"].split(","))


        #处理download_count
        d["download_count"] = AppCenterGame.get_sum_download_count(self.platform_game_ids)

        # d["platform_game_ids"] = d["platform_game_ids"].split(",")
        # tmp = [Platform.get_platform_name_by(id) for id in d["platform_game_ids"]]
        # d["platform_name"] = ",".join(tmp)

        #处理size
        d["size"] = app_size_modify(d["size"])
        return d

    def toJSON(self):
        d = super(AppCenterGame, self).toJSON()
        # if d["id"] == 9:
        #     pass
        #处理type
        type_tmp = []
        if d["type"].find(",") != -1:
            d["type"] = d["type"].split(",")
            for i in d["type"]:
                if i.find(":") != -1:
                    type_tmp.append(i.split(":")[1])
                else:
                    type_tmp.append(i)
        elif d['type'].find(":") != -1:
            type_tmp.append(d['type'].split(":")[1])
        else:
            type_tmp.append(d['type'])
        d["type"] = type_tmp
        #处理title
        if len(d["title"]) > 7:
            d["title"] = d["title"][:7] + ".."
            # d["title"] = d["title"].decode('utf-8')[0:6].encode('utf-8')+".."
        #处理level
        # if d["level"] > 5:
        #     d["level"] = d["level"]/2.

        #处理download_count
        d["download_count"] = AppCenterGame.get_sum_download_count(self.platform_game_ids)

        #删除多余字段
        try:
            del d["tags"]
            del d["description"]
            del d["instruction"]
            del d["screenshots"]
            del d['uptime']
            del d["status"]
            del d["version"]
        except Exception as e:
            print e
        return d


class Topic(JSONBaseModel):
    """
    专题
    """
    flag = models.CharField(default="flag", max_length=100, null=False, verbose_name=u'专题flag')
    topic_name = models.CharField(max_length=100, null=True, verbose_name=u'专题名称')
    big_image_url = models.CharField(max_length=1024, null=True, verbose_name=u'专题大图')
    small_image_url = models.CharField(max_length=1024, null=True, verbose_name=u'专题小图')
    url = models.CharField(max_length=1024, null=True, verbose_name=u'专题的链接')
    order = models.IntegerField(default=0, null=False, verbose_name=u'排序 0排在第一个')
    status = models.BooleanField(default=True, null=False, verbose_name=u'专题状态')
    type = models.CharField(default="zhuanti", max_length=32, null=False, verbose_name=u'lunbo or zhuanti')
    game_libaray_id = models.IntegerField(null=True, verbose_name=u'渠道包id')
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def add_topic(cls, topic_name):
        topic = cls()
        topic.topic_name = topic_name
        try:
            topic.save()
            return True
        except Exception as e:
            print e
            return False

    @classmethod
    def delete_topic_by_id(cls, topic_id):
        if cls.objects.filter(pk=topic_id).exists():
            topic_games = TopicGame.objects.filter(topic_id=topic_id)
            for topic_game in topic_games:
                topic_game.delete()

            topic = cls.objects.get(pk=topic_id)
            topic.delete()
            return True
        else:
            return False

    @classmethod
    def get_topic_data(cls, topic_id):
        return cls.objects.get(pk=topic_id).toJSON()

    @classmethod
    def get_topic_data_by_type_without_game(cls, type):
        result = []
        data = []
        if not type:
            data = cls.objects.filter(status=True,game_libaray_id=None).order_by("order")
        elif type == "lunbo":
            data = cls.objects.filter(status=True,type="lunbo",game_libaray_id=None).order_by("order")
        elif type == "zhuanti":
            data = cls.objects.filter(status=True,type="zhuanti",game_libaray_id=None).order_by("order")
        else:
            data = []
        for i in data:
            result.append(i.toJSON())

        # #让配置为game的显示出来
        # for i in result:
        #     if i["game_libaray_id"] and i["game_libaray_id"] not in ("None", "none") and GameLibrary.objects.filter(pk=i["game_libaray_id"]).exists():
        #         i["topic_game"] = GameLibrary.objects.get(pk=i["game_libaray_id"]).toJSON()
        return result

    @classmethod
    def get_topic_data_by_type(cls, type):
        result = []
        data = []
        if not type:
            data = cls.objects.filter(status=True).order_by("order")
        elif type == "lunbo":
            data = cls.objects.filter(status=True,type="lunbo").order_by("order")
        elif type == "zhuanti":
            data = cls.objects.filter(status=True,type="zhuanti").order_by("order")
        else:
            data = []
        for i in data:
            result.append(i.toJSON())

        #让配置为game的显示出来
        for i in result:
            if i["game_libaray_id"] and i["game_libaray_id"] not in ("None", "none") and GameLibrary.objects.filter(pk=i["game_libaray_id"]).exists():
                # i["topic_game"] = GameLibrary.objects.get(pk=i["game_libaray_id"]).toJSON()
                i["topic_game"] = GameLibrary.get_game(i["game_libaray_id"], True).toJSON()
        return result


class TopicGame(JSONBaseModel):
    """
    专题包含的游戏
    """
    topic_id = models.IntegerField(null=True, verbose_name=u'topic_id')
    app_center_game_id = models.IntegerField(null=True, verbose_name=u'app_center_game_id')
    order = models.IntegerField(default=0, null=False, verbose_name=u'排序 0排在第一个')
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def change_order(cls, topic_id, app_center_game_id, order):
        if not cls.objects.filter(topic_id=topic_id,app_center_game_id=app_center_game_id).exists():
            return
        else:
            a = cls.objects.get(topic_id=topic_id,app_center_game_id=app_center_game_id)
            a.order = int(order)
            a.save()
            return True

    @classmethod
    def add_topic_game_by_topic_id_and_app_center_game_id(cls, topic_id, app_center_game_id):
        if not AppCenterGame.objects.filter(pk=app_center_game_id).exists():
            return False
        if cls.objects.filter(topic_id=topic_id,app_center_game_id=app_center_game_id).exists():
            return
        else:
            topic_game = cls()
            topic_game.topic_id = topic_id
            topic_game.app_center_game_id = app_center_game_id
            topic_game.save()
            return True

    @classmethod
    def get_game_by_topic_id(cls, topic_id):
        topic_game = cls.objects.filter(topic_id=topic_id).order_by("order")
        result = []
        for i in topic_game:
            result.append(i.toJSON())
        return result

    @classmethod
    def delete_topic_game_by_topic_id_and_app_center_game_id(cls, topic_id, app_center_game_id):
        if not cls.objects.filter(topic_id=topic_id, app_center_game_id=app_center_game_id):
            return False
        else:
            topic_game = cls.objects.get(topic_id=topic_id, app_center_game_id=app_center_game_id)
            topic_game.delete()
            return True

class RatingComment(JSONBaseModel):
    """
    评分系统
    """
    guid = models.CharField(max_length=100, unique=True, null=False, blank=False)
    app_center_id = models.IntegerField(null=False, verbose_name=u'APP center id')
    level = models.FloatField(default=5.0, null=False)
    title = models.CharField(max_length=100, unique=False, null=False, blank=False)
    content = models.CharField(max_length=2048, null=True)
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')


    @classmethod
    def add_rating_comment(cls, guid, app_center_id, level, title, content):
        if cls.objects.filter(guid=guid,app_center_id=app_center_id).count() >= 50:     #每人最多评论一个游戏50次
            return 50
        if not AppCenterGame.if_app_center_id_exists(app_center_id):
            return False
        else:
            data = cls()
            data.guid = guid
            data.app_center_id = app_center_id
            data.level = level
            data.title = title
            data.content = content
            data.save()
            return True

    @classmethod
    def get_comment_by_app_center_id(cls,app_center_id):
        result = []
        res = cls.objects.filter(app_center_id=app_center_id).order_by("-uptime").values("title","content","uptime")
        for i in res:
            tmp = {}
            tmp["title"] = i["title"]
            tmp["content"] = i["content"]
            tmp["uptime"] = i["uptime"].strftime('%Y-%m-%d %H:%M:%S')
            result.append(tmp)
        return result


class AppCenterList(JSONBaseModel):
    """
    游戏推荐列表与app对应表
    """
    app_list_id = models.IntegerField(null=False, verbose_name=u'游戏推荐list id')
    app_center_id = models.IntegerField(null=False, verbose_name=u'APP id')
    order = models.IntegerField(default=0, null=False, verbose_name=u'排序 0排在第一个')
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def change_order(cls, app_center_game_id, rec_id, order):
        if not cls.objects.filter(app_list_id=rec_id,app_center_id=app_center_game_id):
            return False
        else:
            acl = cls.objects.get(app_list_id=rec_id,app_center_id=app_center_game_id)
            acl.order = order
            try:
                acl.save()
                return True
            except Exception as e:
                print e
                print False


    @classmethod
    def add_game_by_rec_id_app_center_game_id(cls, rec_id, app_center_game_id):
        if not AppCenterGame.objects.filter(pk=app_center_game_id).exists():
            return False
        if cls.objects.filter(app_list_id=rec_id, app_center_id=app_center_game_id).exists():
           return False
        else:
            rec = cls()
            rec.app_list_id = int(rec_id)
            rec.app_center_id = int(app_center_game_id)
            rec.order = 999
            try:
                rec.save()
                return True
            except Exception as e:
                print e
                return False

    @classmethod
    def delete_game_by_rec_id_app_center_game_id(cls, rec_id, app_center_game_id):
        if cls.objects.filter(app_list_id=rec_id, app_center_id=app_center_game_id).exists():
            game = cls.objects.get(app_list_id=rec_id, app_center_id=app_center_game_id)
            try:
                game.delete()
                return True
            except Exception as e:
                print e
                return False
        else:
            return False

    @classmethod
    def get_game_by_rec_id(cls, rec_id):
        result = []
        apps = cls.objects.filter(app_list_id=rec_id).order_by("order")
        for i in apps:
            app = AppCenterGame.get_app_by_id(i.app_center_id)
            tmp = app.toJSON_manage()
            tmp["order"] = i.order
            # if tmp["icon_tag_id"]:
            #     tmp["icon_tag"] = IconTag.get_icon_tag_title_by_id(tmp["icon_tag_id"])
            # else:
            #     tmp["icon_tag"] = ""
            result.append(tmp)
        return result

    @classmethod
    def get_app_and_order_by_flag(cls, flag):
        if not flag:
            return False
        reco_list_id = RecommendList.get_reco_id_by_flag(flag)
        if not cls.objects.filter(app_list_id=reco_list_id).exists():
            return False
        else:
            result = []
            #app_id_list = [i.app_center_id for i in cls.objects.filter(app_list_id=reco_list_id).order_by("order")]
            app_id_order = cls.objects.filter(app_list_id=reco_list_id).order_by("order").values("app_center_id","order","app_list_id")
            for i in app_id_order:
                # result.append({"app_center_id":i["app_center_id"], "order":i["order"], "app_list_id":i["app_list_id"]})
                app = AppCenterGame.get_app_by_id(i["app_center_id"])
                tmp = app.toJSON()
                tmp["order"] = i["order"]
                if tmp["icon_tag_id"]:
                    tmp["icon_tag"] = IconTag.get_icon_tag_title_by_id(tmp["icon_tag_id"])
                else:
                    tmp["icon_tag"] = ""
                tmp["quick_download_download_url"], tmp["quick_download_package_name"] = GameLibrary.get_quick_download_info_by_platform_game_ids(tmp["platform_game_ids"])
                result.append(tmp)
            return result[:50]


class RecommendList(JSONBaseModel):
    """
    游戏推荐列表
    """
    title = models.CharField(max_length=64, null=False ,verbose_name=u'推荐列表名称')
    flag = models.CharField(max_length=64, null=True ,verbose_name=u'推荐列表英文名称')
    uptime = models.DateTimeField(auto_now =True, verbose_name=u'数据更新时间')

    @classmethod
    def delete_rec_by_id(cls, rec_id):
        if not RecommendList.objects.filter(pk=rec_id).exists():
            return False
        else:
            for i in AppCenterList.objects.filter(app_list_id=rec_id):
                i.delete()
            rec = RecommendList.objects.get(pk=rec_id)
            rec.delete()
            return True


    @classmethod
    def get_reco_id_by_flag(cls, flag):
        if cls.objects.filter(flag=flag).exists():
            return cls.objects.get(flag=flag).pk

    @classmethod
    def get_all_recommend(cls):
        return cls.objects.all()

    @classmethod
    def add_recommend(cls, title):
        rec = cls()
        rec.title = title
        try:
            rec.save()
            return True
        except Exception as e:
            print e
            return False

    @classmethod
    def get_rec_data(cls, rec_id):
        return cls.objects.get(pk=rec_id)


class GameLibrary(JSONBaseModel):
    class Meta:
        db_table = "game_library"

    game_name = models.CharField(max_length=64, null=False)
    game_name_lower = models.CharField(max_length=64, null=True)
    type = models.CharField(max_length=32, null=True)
    platformId = models.IntegerField(null=False)
    platform_game_id = models.IntegerField(null=False)
    icon_url = models.CharField(max_length=1024, null=True)
    size = models.IntegerField(null=True)
    download_count = models.IntegerField(default=1000, null=False)
    modify_date = models.DateTimeField(verbose_name=u'修改时间')
    screenshots = models.TextField(max_length=2048, null=True, help_text=u'截图地址')
    screenshots_type = models.CharField(default="shu", max_length=32, null=False)
    version = models.CharField(max_length=32, null=True)
    tags = models.CharField(max_length=512, null=True)
    level = models.IntegerField(default=0, null=True)
    instruction = models.CharField(max_length=2048, null=True)
    description = models.TextField(help_text=u'描述')
    deleted = models.IntegerField(default=0, null=False)
    bs_deleted = models.IntegerField(default=0, null=True)
    download_url = models.CharField(max_length=512, null=True, verbose_name=u'下载链接')
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')
    downloadqrcode_url = models.CharField(max_length=512, null=True, verbose_name=u'下载地址的二维码URL')
    package_name = models.CharField(max_length=256, null=True, verbose_name=u'包名')
    pinyin = models.CharField(max_length=256, null=True, verbose_name=u'拼音')
    initails_pinyin = models.CharField(max_length=256, null=True, verbose_name=u'拼音首字母')


    @classmethod
    def check_package_name(cls, package_name):
        if cls.objects.filter(package_name=package_name).exists():
            app = cls.objects.filter(package_name=package_name)[0]
            return app.toJSON()
        else:
            return None


    @classmethod
    def get_qrcode_by_id(cls, id):
        if not cls.objects.filter(pk=id).exists():
            return None
        else:
            return cls.objects.get(pk=id).downloadqrcode_url


    @classmethod
    def get_quick_download_info_by_platform_game_ids(cls, platform_game_ids):
        """
        获取快速下载的package name和 download_url
        """
        quick_download_download_url, quick_download_package_name = "", ""
        platform_game_id = 0
        if platform_game_ids:
            try:
                platform_game_id = platform_game_ids.split(",")[0]
            except Exception, e:
                print e
            if platform_game_id and cls.objects.filter(pk=platform_game_id).exists():
                game = cls.get_game(id=platform_game_id, use_modify=True)
                quick_download_download_url = game.download_url
                quick_download_package_name = game.package_name
        return quick_download_download_url, quick_download_package_name

    @classmethod
    def get_game(cls, id, use_modify):
        if not id:
            raise Exception
        elif not GameLibrary.objects.filter(pk=id).exists():
            return None
        else:
            game = GameLibrary.objects.get(pk=id)
            return game


    @classmethod
    def get_download_count_by_id(cls, id):
        if not id:
            raise Exception
        else:
            # if GameLibraryModify.objects.filter(pk=id).exists() and GameLibraryModify.objects.get(pk=id).download_count:
            #     return GameLibraryModify.objects.get(pk=id).download_count
            if not cls.objects.filter(pk=id).exists():
                # raise Exception
                return 1000
            else:
                return cls.objects.get(pk=id).download_count


    @classmethod
    def get_attr_value_by_id(cls, id, attr):
        if not cls.objects.filter(pk=id).exists():
            raise Exception
            print "game id not exsit"
        else:
            game = cls.objects.get(pk=id)
            return super(GameLibrary, game).toJSON()[attr]
            # return game.toJSON()[attr]         #使用规整后的数据


    @classmethod
    def get_platform_name_by_game_ids(cls, platform_game_ids):
        """
        通过传入多个platform_game_ids获取所属的渠道
        """
        result = []
        for game_id in platform_game_ids:
            if not game_id:
                continue
            if not cls.objects.filter(pk=game_id).exists():
                return False
            else:
                game = cls.objects.get(pk=game_id)
                platform_name = Platform.get_platform_name_by_id(game.platformId)
                result.append(platform_name)
        return ",".join(result)

    @classmethod
    def get_query_suggest_app(cls, query):
        if not query:
            return []
        q = GameLibrary.objects.filter(Q(game_name__startswith=query) | Q(game_name_lower__startswith=query)).exclude(deleted=True).exclude(bs_deleted=True)
        return [i.toJSON() for i in q]


    @classmethod
    def search(cls, query, number_per_page, baidu_package_name_list,baidu_game_name_list):
        """
        搜索，如果query不包含中文字符，则使用inital和pinyin，否则使用game_name
        如果package_name和百度相同，则去掉
        如果游戏名称和百度搜索游戏名称相同，则去掉
        如果自己游戏库没有，则搜360全量库
        """
        if not query:
            return False
        else:
            result = []
            query = query.strip()

            if not content_chinese_word(query) and query not in ("QQ"):
                initials_data = cls.objects.filter(deleted=False, bs_deleted=False, initails_pinyin__startswith=query, download_url__isnull=False).exclude(package_name__in=baidu_package_name_list)
                pinyin_data = cls.objects.filter(deleted=False, bs_deleted=False, pinyin__contains=query, download_url__isnull=False).exclude(package_name__in=baidu_package_name_list)

                for i in initials_data:
                    if i.platformId == 306 and i.game_name in baidu_game_name_list: #baidu 渠道包
                        continue

                    result.append(i.toJSON())
                for i in pinyin_data:
                    if i.platformId == 306 and i.game_name in baidu_game_name_list: #baidu 渠道包
                        continue

                    result.append(i.toJSON())

            else:
                # data = cls.objects.filter(game_name__contains=query).exclude(deleted=True).exclude(bs_deleted=True).exclude(package_name__in=baidu_package_name_list)[:10]
                data = cls.objects.filter(Q(game_name__contains=query) | Q(game_name_lower__contains=query) ).exclude(deleted=True).exclude(bs_deleted=True).exclude(package_name__in=baidu_package_name_list)[:10]
                if data:
                    for i in data:
                        if i.platformId == 306 and i.game_name in baidu_game_name_list: #baidu 渠道包
                            continue

                        result.append(i.toJSON())
                else: #搜索360全量库数据
                    data = Total360Game.search(query)
                    for i in data:
                        result.append(i.toJSON())

            ids = []      #去重
            res = []
            for i in result:
                if i["id"] not in ids:
                    res.append(i)
                    ids.append(i["id"])
            return res[:number_per_page]


    @classmethod
    def get_app_detail_by_ids(cls, platform_game_ids):
        result = []
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            result = mc_client.get("get_app_detail_by_ids@%s" % str(platform_game_ids))
        if not result:
            result = []
            for game_id in platform_game_ids:
                if GameLibrary.objects.filter(pk=game_id).exists():
                    game = GameLibrary.objects.get(pk=game_id).toJSON()
                    game["type"] = AppCenterGame.find_appcentergame_type_by_platform_game_id(game["id"])
                    result.append(game)
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("get_app_detail_by_ids@%s" % str(platform_game_ids), result, time=3600*24)
        return result


    def toJSON(self):
        d = super(GameLibrary, self).toJSON()
        # if d["id"] == 9:
        #     pass

        #处理platform
        d["platform_name"] = Platform.get_platform_name_by_id(d["platformId"])

        #处理type
        d["type"] = handle_tojson_type(d["type"])
        d["screenshots"] = screenshots_modify(d["screenshots"])
        d["size"] = app_size_modify(d["size"])
        d["modify_date"] = modify_date_modify(d["modify_date"])

        #处理title
        # if len(d["title"]) > 6:
        #     d["title"] = d["title"][:6] + ".."
        #     # d["title"] = d["title"].decode('utf-8')[0:6].encode('utf-8')+".."
        #处理level
        # if d["level"] > 5:
        #     d["level"] = d["level"]/2.
        #删除多余字段
        # try:
        #     del d["tags"]
        #     del d["description"]
        #     del d["instruction"]
        #     del d["screenshots"]
        #     del d['uptime']
        #     del d["status"]
        #     del d["version"]
        # except Exception as e:
        #     print e
        return d


class SearchDefaultQuery(JSONBaseModel):
    query = models.CharField(max_length=128, null=True)
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')

    @classmethod
    def get_default_query(cls):
        if SearchDefaultQuery.objects.all().exists():
            try:
                return SearchDefaultQuery.objects.all().order_by("-uptime")[0].query
            except Exception, e:
                print e
                return ""
        else:
            return ""


class GameLibraryModify(JSONBaseModel):
    class Meta:
        db_table = "game_library_modify"

    game_name = models.CharField(max_length=64, null=True)
    type = models.CharField(max_length=32, null=True)
    icon_url = models.CharField(max_length=1024, null=True)
    size = models.IntegerField(null=True)
    download_count = models.IntegerField(null=True)
    screenshots = models.TextField(max_length=2048, null=True, help_text=u'截图地址')
    version = models.CharField(max_length=32, null=True)
    tags = models.CharField(max_length=512, null=True)
    level = models.IntegerField(null=True)
    instruction = models.CharField(max_length=2048, null=True)
    description = models.TextField(help_text=u'描述', null=True)
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')
    screenshots_type = models.CharField(default="shu", max_length=32, null=True)
    download_url = models.CharField(max_length=512, null=True, verbose_name=u'下载链接')
    pinyin = models.CharField(max_length=256, null=True, verbose_name=u'拼音')
    initails_pinyin = models.CharField(max_length=256, null=True, verbose_name=u'拼音首字母')


    # @classmethod
    # def get_app_detail_by_ids(cls, platform_game_ids):
    #     result = []
    #     for game_id in platform_game_ids:
    #         game = cls.objects.filter(pk=game_id)[0].toJSON()
    #         result.append(game)
    #     return result
    #
    #
    def toJSON(self):
        """
        需要逐个字段判断是否为空，如果为空则回去取值
        """
        # for attr in self.__dict__:
        #     if self.__dict__[attr] is None:
        #         self.attr = GameLibrary.find_attr_value_by_id(self.pk)

        d = super(GameLibraryModify, self).toJSON()
        for attr in d.keys():
            if d[attr] is None:
                d[attr] = GameLibrary.get_attr_value_by_id(d["id"], attr)



        # if d["id"] == 9:
        #     pass

        # #处理platform
        # d["platform_name"] = Platform.get_platform_name_by_id(d["platformId"])

        #处理type
        type_tmp = []
        # if d["type"].find(",") != -1:
        #     d["type"] = d["type"].split(",")
        #     for i in d["type"]:
        #         if i.find(":") != -1:
        #             type_tmp.append(i.split(":")[1])
        #         else:
        #             type_tmp.append(i)
        # else:
        #     if d['type'].find(":") != -1:
        #         type_tmp.append(d['type'].split(":")[1])
        type_tmp.append(d["type"])
        d["type"] = type_tmp
        d["screenshots"] = screenshots_modify(d["screenshots"])

        d["size"] = app_size_modify(d["size"])
        # d["modify_date"] = modify_date_modify(d["modify_date"])
        return d


class Platform(JSONBaseModel):
    class Meta:
        db_table = "platform"

    platform_name = models.CharField(max_length=32, unique=False, null=True)

    @classmethod
    def get_platform_name_by_id(cls, id):
        if cls.objects.filter(pk=id).exists():
            return cls.objects.get(pk=id).platform_name
        else:
            return

    @classmethod
    def get_platfor_id_by_name(cls, name):
        if cls.objects.filter(platform_name=name).exists():
            return cls.objects.get(platform_name=name).pk
        else:
            return

class UserFeedBack(JSONBaseModel):
    """
    用户反馈
    """
    class Meta:
        db_table = "user_feed_back"

    email = models.CharField(max_length=100, unique=False, null=False, blank=False, help_text=u'邮箱')
    content = models.TextField(max_length=2048, help_text=u'反馈内容')
    type = models.TextField(max_length=512, help_text=u'反馈内容类型',null=True)
    create_time = models.DateTimeField(default=timezone.now, verbose_name=u'创建时间')

    @classmethod
    def post_in_one_min(cls, email):
        if cls.objects.filter(email=email).exists():
            data = cls.objects.filter(email=email).order_by("-create_time")[0]
            if datetime.datetime.now() - data.create_time > datetime.timedelta(minutes=1):
                return False
            else:
                return True


class SearchKeyWord(JSONBaseModel):
    """
    用户反馈
    """
    class Meta:
        db_table = "manage_search_key_word"

    client = models.IntegerField(default=1, null=False)
    word = models.CharField(max_length=128, unique=False, null=False, blank=True, help_text=u'搜索关键词')
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')

    @classmethod
    def add_query(cls, query):
        a = cls()
        a.word = query
        try:
            a.save()
        except Exception, e:
            print e


class Total360Game(JSONBaseModel):
    """
    360全量数据
    """
    class Meta:
        db_table = "total_360_game"

    platformId = models.IntegerField(null=False)
    platform_game_id = models.IntegerField(null=False,unique=True)
    game_name = models.CharField(max_length=64, null=False, help_text=u'name')
    type = models.CharField(max_length=32, null=True, help_text=u'categoryName')
    icon_url = models.CharField(max_length=1024, null=True, help_text=u'iconUrl')
    size = models.IntegerField(null=True, help_text=u'apkSize')
    download_count = models.IntegerField(default=1000, null=False, help_text=u'downloadTimes')
    modify_date = models.DateTimeField(verbose_name=u'修改时间',help_text=u'updateTime')
    screenshots = models.TextField(max_length=2048, null=True, help_text=u'screenshotsUrl')
    screenshots_type = models.CharField(default="shu", max_length=32, null=False, help_text=u'')
    version = models.CharField(max_length=32, null=True, help_text=u'versionName')
    tags = models.CharField(max_length=512, null=True, help_text=u'tag')
    level = models.FloatField(default=0.0, null=True, help_text=u'rating')
    instruction = models.CharField(max_length=2048, null=True, help_text=u'brief')
    description = models.TextField(help_text=u'描述')
    deleted = models.IntegerField(default=0, null=False, help_text=u'截图地址')
    download_url = models.CharField(max_length=512, null=True, verbose_name=u'downloadUrl')
    downloadqrcode_url = models.CharField(max_length=512, null=True, verbose_name=u'下载地址的二维码URL')
    package_name = models.CharField(max_length=256, null=True, verbose_name=u'packageName')
    apk_md5 = models.CharField(max_length=32, null=True, help_text=u'apkMd5')
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')


    @classmethod
    def check_package_name(cls, package_name):
        if cls.objects.filter(package_name=package_name).exists():
            app = cls.objects.get(package_name=package_name)
            return app.toJSON()
        else:
            return None

    @classmethod
    def search(cls, query):
        return Total360Game.objects.filter(game_name__startswith=query)[:5]


    def toJSON(self):
        d = super(Total360Game, self).toJSON()
        d["platform_name"] = "total_360"
        d["type"] = handle_tojson_type(d["type"])
        d["screenshots"] = screenshots_modify(d["screenshots"])
        d["size"] = app_size_modify(d["size"])
        d["modify_date"] = modify_date_modify(d["modify_date"])
        return d

    @classmethod
    def get_app_detail_by_id(cls, total_360_id):
        game = {}
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            game = mc_client.get("total_360_detail@%s" % str(total_360_id))
        if not game:
            if cls.objects.filter(pk=total_360_id).exists():
                game = cls.objects.get(pk=total_360_id).toJSON()
                mc_client.set("total_360_detail@%s" % str(total_360_id), game, time=3600*24)
        return game

class ChargeReturn(JSONBaseModel):
    """
    充值返现
    """
    class Meta:
        db_table = "manage_charge_return"

    game_id = models.CharField(max_length=128, unique=False, null=False, blank=False)
    order_id = models.CharField(max_length=128, unique=True, null=False, blank=False)
    amount = models.IntegerField(default=0, null=False)
    contact = models.CharField(max_length=256, unique=False, null=False, blank=False)
    game_name = models.CharField(max_length=64, unique=False, null=True, blank=False)
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')

    @classmethod
    def add_data(cls, game_id, order_id, amount, contact, game_name):
        a = cls()
        a.game_name = game_name
        a.game_id = game_id
        a.order_id = order_id
        a.amount = int(amount)
        a.contact = contact
        try:
            a.save()
            return True
        except Exception, e:
            print e
            return False


class RecommendInstallAPP(JSONBaseModel):
    """
    首次安装推荐APP
    """
    game_library_id = models.IntegerField(null=False, unique=True)
    order = models.IntegerField(default=0, null=False)
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')

    @classmethod
    def get_rec_app(cls):
        result = []
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            result = mc_client.get("recommend_install_app")
        if not result:
            result = []
            apps = cls.objects.all().order_by("-uptime")[:8]
            for i in apps:
                if GameLibrary.objects.filter(id=i.game_library_id).exists():
                    game = GameLibrary.objects.get(id=i.game_library_id)
                    result.append(game.toJSON())
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("recommend_install_app", result, time=3600 * 24)
        return result

class PopWindow(JSONBaseModel):
    """
    弹窗配置
    """
    big_image_url = models.CharField(max_length=1024, null=True, blank=False)
    big_image_download_url = models.CharField(max_length=1024, null=True, blank=False)
    big_image_height = models.CharField(max_length=127, null=True, blank=False)
    big_image_width = models.CharField(max_length=127, null=True, blank=False)

    small_image_url = models.CharField(max_length=1024, null=True, blank=False)

    exit_button_icon_url = models.CharField(max_length=1024, null=True, blank=False)
    exit_button_height = models.CharField(max_length=127, null=True, blank=False)
    exit_button_width = models.CharField(max_length=127, null=True, blank=False)
    exit_button_margin_top = models.CharField(max_length=127, null=True, blank=False)
    exit_button_margin_right = models.CharField(max_length=127, null=True, blank=False)

    button1_app_name = models.CharField(max_length=127, null=True, blank=False)
    button1_package_name = models.CharField(max_length=255, null=True, blank=False)
    button1_type = models.CharField(default='normal', max_length=127, null=True, blank=False)
    button1_redirect_url = models.CharField(max_length=1024, null=True, blank=False)
    button1_icon_url = models.CharField(max_length=1024, null=True, blank=False)
    button1_download_url = models.CharField(max_length=1024, null=True, blank=False)
    button1_height = models.CharField(max_length=127, null=True, blank=False)
    button1_width = models.CharField(max_length=127, null=True, blank=False)
    button1_margin_top = models.CharField(max_length=127, null=True, blank=False)
    button1_margin_left = models.CharField(max_length=127, null=True, blank=False)

    button2_app_name = models.CharField(max_length=127, null=True, blank=False)
    button2_package_name = models.CharField(max_length=255, null=True, blank=False)
    button2_type = models.CharField(default='normal', max_length=127, null=True, blank=False)
    button2_redirect_url = models.CharField(max_length=1024, null=True, blank=False)
    button2_icon_url = models.CharField(max_length=1024, null=True, blank=False)
    button2_download_url = models.CharField(max_length=1024, null=True, blank=False)
    button2_height = models.CharField(max_length=127, null=True, blank=False)
    button2_width = models.CharField(max_length=127, null=True, blank=False)
    button2_margin_top = models.CharField(max_length=127, null=True, blank=False)
    button2_margin_left = models.CharField(max_length=127, null=True, blank=False)

    button3_app_name = models.CharField(max_length=127, null=True, blank=False)
    button3_package_name = models.CharField(max_length=255, null=True, blank=False)
    button3_type = models.CharField(default='normal', max_length=127, null=True, blank=False)
    button3_redirect_url = models.CharField(max_length=1024, null=True, blank=False)
    button3_icon_url = models.CharField(max_length=1024, null=True, blank=False)
    button3_download_url = models.CharField(max_length=1024, null=True, blank=False)
    button3_height = models.CharField(max_length=127, null=True, blank=False)
    button3_width = models.CharField(max_length=127, null=True, blank=False)
    button3_margin_top = models.CharField(max_length=127, null=True, blank=False)
    button3_margin_left = models.CharField(max_length=127, null=True, blank=False)

    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')





class PartnerPreInstallGame(JSONBaseModel):
    """
    合作方预装游戏
    """
    partner = models.CharField(max_length=128, null=False, blank=False)
    partner_name_zh = models.CharField(max_length=128, null=True, blank=False)
    game_name = models.CharField(max_length=128, null=True, blank=False)
    package_name = models.CharField(max_length=256, null=False, verbose_name=u'包名')
    download_url = models.CharField(max_length=1024, null=False, verbose_name=u'download_url')
    icon_url = models.CharField(max_length=1024, null=False, verbose_name=u'icon_url')
    uptime = models.DateTimeField(default=timezone.now, verbose_name=u'数据更新时间')

    @classmethod
    def get_info_by_partner_package_name(cls, partner, package_name):
        if cls.objects.filter(partner=partner,package_name=package_name).exists():
            app = cls.objects.get(partner=partner,package_name=package_name)
            return app.toJSON()
        else:
            return

    @classmethod
    def add(cls, partner, partner_name_zh, game_name, package_name, download_url, icon_url):
        if cls.objects.filter(partner=partner,package_name=package_name).exists():
            return False
        else:
            a = cls()
            a.partner = partner
            a.partner_name_zh = partner_name_zh
            a.game_name = game_name
            a.package_name = package_name
            a.download_url = download_url
            a.icon_url = icon_url
            try:
                a.save()
            except Exception, e:
                print e
            return True


# class BSUser(AbstractBaseUser, JSONBaseModel):
#     """
#     用户表
#     """
#     username = models.CharField(max_length=30, null=True, blank=True, unique=True)
#     tel = models.CharField(max_length=20, unique=True, null=True, blank=True, help_text=u'手机号')
#     email = models.CharField(_('email'), null=True, max_length=128)
#     # password = models.CharField(_('password'), max_length=128)
#     # last_login = models.DateTimeField(_('last login'), blank=True, null=True)
#     realname = models.CharField(max_length=30, null=True, blank=True, verbose_name=u'真实姓名', help_text=u'真实姓名',default="")
#     sex = models.NullBooleanField(default=None, verbose_name=u'性别')
#     # icon_url = models.ForeignKey(BaseFile, null=True, blank=True, verbose_name=u'默认头像')
#
#     is_staff = models.BooleanField(_('staff status'), default=False,
#                                    help_text=_('Designates whether the user can log into this admin '
#                                                'site.'))
#     is_active = models.BooleanField(_('active'), default=True,
#                                     help_text=_('Designates whether this user should be treated as '
#                                                 'active. Unselect this instead of deleting accounts.'))
#     create_time = models.DateTimeField(_('date joined'), default=timezone.now)
#     # hxpassword = models.CharField(max_length=50, null=True, verbose_name=u'环信密码')
#     # hx_reg = models.BooleanField(default=False, db_index=True, verbose_name=u'是否注册过环信')
#
#     # guanzhu = models.ManyToManyField('Project', null=True, blank=True, verbose_name=u'关注项目')
#
#     objects = BaseUserManager()
#
#     USERNAME_FIELD = 'username'
#     REQUIRED_FIELDS = []
#
#     def __unicode__(self):
#         return unicode(self.name)


    # def save(self, *args, **kwargs):
    #     """
    #
    #     """
    #     # if not self.pk:
    #     #     import uuid
    #     #
    #     #     u = None
    #     #     self.hxpassword = str(uuid.uuid4()).replace('-', '')[:12]
    #     # else:
    #     #     u = NSUser.objects.get(pk=self.pk)
    #
    #     super(NSUser, self).save(*args, **kwargs)
    #     # cache.delete(USERINFO_INFO % self.pk)
    #     if not u or self.name != u.name or self.nickname != u.nickname or self.sex != u.sex or u.icon_url_id != self.icon_url_id:
    #         for p in self.person_set.filter(is_active=True):
    #             p.save()
    #     if not self.hx_reg:
    #         if not self.hxpassword:
    #             import uuid
    #
    #             self.hxpassword = str(uuid.uuid4()).replace('-', '')[:12]
    #             # if False and not self.hx_reg:    #测试用
    #         from easemob.client import register_new_user
    #
    #         result, errormsg = register_new_user(self.pk, self.hxpassword)
    #         if result:
    #             self.hx_reg = True
    #             if kwargs.has_key('update_fields'):
    #                 kwargs['update_fields'].append('hx_reg')
    #                 kwargs['update_fields'].append('hxpassword')
    #             super(NSUser, self).save(*args, **kwargs)


    # def get_user_map(self, myself=False):
    #     """
    #     组织用户的字典数据，根据参数放入密码
    #     by:王健 at:2015-1-20
    #     增加关注的项目的id列表，修复guanzhu 错误
    #     by:王健 at:2015-1-30
    #     修复bug icon_url 空值，变量名冲突
    #     by:王健 at:2015-1-31
    #     增加参与项目 id列表
    #     by:王健 at:2015-2-2
    #     修改了 昵称字段
    #     by:王健 at:2015-3-4
    #     游客账号 不输入环信密码
    #     by:王健 at:2015-4-8
    #     通过七牛处理功能,产生头像缩略图
    #     by: 范俊伟 at:2015-04-08
		# 增加realname
		# by：尚宗凯 at：2015-06-26
    #     """
    #     p = {}
    #     p['tel'] = self.tel
    #     if self.name:
    #         p['name'] = self.name
    #     else:
    #         p['name'] = ''
    #     p['nickname'] = self.nickname
    #     if self.icon_url:
    #         p['icon_url'] = self.icon_url.get_url('imageView2/5/w/80/h/80')
    #         p['big_icon_url'] = self.icon_url.get_url()
    #     else:
    #         p['icon_url'] = ''
    #         p['big_icon_url'] = ''
    #     p['uid'] = self.pk
    #     p['sex'] = self.sex
    #     p['hx_reg'] = self.hx_reg
    #     p['realname'] = self.realname
    #     if myself:
    #         if self.pk != settings.SHOW_USER_ID:
    #             p['hxpassword'] = self.hxpassword
    #             p['guanzhuprojectlist'] = [u[0] for u in self.guanzhu.values_list('id')]
    #             if hasattr(self, 'person_set'):
    #                 p['canyuprojectlist'] = [u[0] for u in
    #                                          self.person_set.filter(is_active=True).values_list('project_id')]
    #             else:
    #                 p['canyuprojectlist'] = []
    #         else:
    #             p['canyuprojectlist'] = []
    #             p['guanzhuprojectlist'] = []
    #             p['hxpassword'] = []
    #             p['tel'] = ''
    #     return p
    #
    # class Meta:
    #     verbose_name = _('user')
    #     verbose_name_plural = _('users')
    #
    # # def email_user(self, subject, message, from_email=None):
    # # """
    # # Sends an email to this User.
    # # """
    # # send_mail(subject, message, from_email, [self.email])
    #
    # def get_short_name(self):
    #     return self.name
    #
    # def get_full_name(self):
    #     return self.name





