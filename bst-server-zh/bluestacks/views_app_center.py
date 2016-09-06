# coding=utf-8
import datetime
import hashlib
import json
import requests
from django.contrib.auth.decorators import login_required

from django.utils.http import urlquote

from django.views.decorators.csrf import csrf_exempt
from django.views.generic import FormView, DetailView
from django.views.generic.list import ListView
from django.http import HttpResponse, Http404, HttpResponseRedirect
from django.shortcuts import render
from django.core.paginator import Paginator, EmptyPage, PageNotAnInteger
from django.db.models import F
from django.core.urlresolvers import reverse
from bluestacks.forms import ArticlePublishForm
from bluestacks.models import GameLibrary, AppCenterGame, AppCenterList, RecommendList, UserFeedBack, \
    RatingComment, TopicGame, Topic, IconTag, GameLibraryModify, SearchKeyWord, Total360Game
from bst_server.settings import URL_SERVER_HOST, ENVIRONMENT, USE_MC_ENVIRONMENT
from util.appcenter import cal_total_page
from util.decorators import check_signature
from util.jsonresult import getResult, getJsonpResult, getSimpleResult
from util.memcached import mc_client
from oss_models import OssPackage, ObsInfo
from util.network import get_local_ip
from util.oss_opt import check_for_last_file_upload_success

def pts(request):
    """
    pts
    """
    return render(request,"webhtml/d39b239616dca997e674d7d82060e17f.html")


def index(request):
    """
    index
    """
    return HttpResponseRedirect("/bs_browser/app_center_html")

def app_center_board(request):
    """
    榜单页面
    """
    def get_app_center_board():
        data = {}
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            data = mc_client.get("app_center_board")
        if not data:
            data = {}
            AppCenterList.get_app_and_order_by_flag("hot_list")
            data["hot_list"] = AppCenterList.get_app_and_order_by_flag("hot_list")
            data["sell_well_list"] = AppCenterList.get_app_and_order_by_flag("sell_well_list")
            data["app_list"] = AppCenterList.get_app_and_order_by_flag("app_list")
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("app_center_board", data, time=3600*24)
        return data

    if request.method == 'POST':
        return HttpResponse(u"you should use GET method")
    if request.method == 'GET':
        data = get_app_center_board()
        return getResult(True, "app center recommend success", data)


def app_center_topic(request):
    """
    专题列表
    """
    if request.method == 'POST':
        return HttpResponse(u"you should use GET method")
    if request.method == 'GET':
        pass


def app_center_recommend(request):
    """
    推荐页面
    """
    def get_app_center_recommend_data():
        data = {}
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            data = mc_client.get("app_center_recommend")
        if not data:
            data = {}
            data["online_game_recommend"] = AppCenterList.get_app_and_order_by_flag("online_game_recommend")
            data["console_game_recommend"] = AppCenterList.get_app_and_order_by_flag("console_game_recommend")
            data["app_recommend"] = AppCenterList.get_app_and_order_by_flag("app_recommend")
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("app_center_recommend", data, time=3600*24)
        return data

    if request.method == 'POST':
        return HttpResponse(u"you should use GET method")
    if request.method == 'GET':
        data = get_app_center_recommend_data()
        return getResult(True, "app center recommend success", data)


def appsearch_html(request):
    """
    搜索html
    """
    if request.method == 'GET':
        return render(request, 'webhtml/appsearch.html')

@check_signature
def search(request):
    """
    新搜索功能
    """
    #todo: 1解决搜索拼音只能返回一个页的问题
    #todo：2让拼音搜索结果和百度搜索结果合并一起后计算返回的页面
    def get_baidu_result_package_name(query):
        baidu = json.loads(search_baidu(query=query,resNum=32,pageNum=1))
        package_name_list = []
        for i in baidu["datas"]:
            package_name_list.append(i["packageName"])
        return package_name_list

    def get_baidu_result_package_name_and_game_name(query):
        baidu = json.loads(search_baidu(query=query,resNum=32,pageNum=1))
        package_name_list, game_name_list = [],[]
        for i in baidu["datas"]:
            package_name_list.append(i["packageName"])
            game_name_list.append(i["name"])
        return package_name_list, game_name_list


    def merge_baidu_search_result(lib_result, resNum, pageNum):
        pageNum = int(pageNum)
        lib_count = len(lib_result)
        assert lib_count<int(resNum)
        result = []
        total_page = json.loads(search_baidu(query=query,resNum=resNum,pageNum=pageNum))["totalPage"]
        if pageNum >= total_page:
            result = []
        if pageNum == 1:  #第一次       #不可以用elif 保留这个地方 因为feifei需要这样才能正确显示
            result_baidu = json.loads(search_baidu(query=query,resNum=resNum,pageNum=pageNum))
            result = lib_result + result_baidu["datas"][:int(resNum)-lib_count]
        elif pageNum > 1:
            baidu_result_before = json.loads(search_baidu(query=query,resNum=resNum,pageNum=pageNum-1))["datas"][:lib_count]
            baidu_result_after = json.loads(search_baidu(query=query,resNum=resNum,pageNum=pageNum))["datas"][lib_count:]
            result = baidu_result_before + baidu_result_after
        # assert len(result) == int(resNum)

        #修改百度字段名
        for i in result:
            if "game_name" not in i.keys():
                if "title" in i.keys():
                    i["game_name"] = i["title"]      #把appcentergame里面title改为game_name
                elif "name" in i.keys():
                    i["game_name"] = i["name"]      #baidu
                else:
                    raise Exception

            if "icon_url" not in i.keys():
                i["icon_url"] = i["iconUrl"]
            if "platform_name" not in i.keys():
                i["platform_name"] = "百度"
            if "screenshots" not in i.keys():
                i["screenshots"] = ",".join([i["imageUrl1"],i["imageUrl2"],i["imageUrl3"],i["imageUrl4"],i["imageUrl5"]])
            if "modify_date" not in i.keys():
                i["modify_date"] = i["modifyDate"]
            if "instruction" not in i.keys():
                i["instruction"] = i["desc"]
            if "download_url" not in i.keys():
                i["download_url"] = i["apkUrl"]

            if len(i["instruction"]) > 18:
                i["instruction"] = i["instruction"][:18] + ".."
        return result, total_page


    def search_appcenter(query, number_per_page, version, guid, baidu_package_name_list, baidu_game_name_list):
        result = GameLibrary.search(query,number_per_page,baidu_package_name_list,baidu_game_name_list)
        res = []
        for i in result:
            i["data_type"] = "game_library"
            #暂时解决搜索bug，如果这个id不是app_center_game pk就删掉

            #如果这个游戏在appcentergame不存在，就删掉
            # if i["id"] in all_platform_game_ids:
            res.append(i)
        return res

    def search_baidu(query, pageNum, resNum):
        baidu_url = "http://frsearch.duoku.com/MGameQuery/query.do"
        query_data = {"query":urlquote(query).lower(),
                      "channelId":"5004166",          #bluestack channelid
                      "resNum":str(resNum),           #每页返回结果条数
                      "pageNum":str(pageNum)}         #页数
        r = requests.post(baidu_url, json.dumps(query_data), timeout=10).text
        return r


    #function start
    query = request.GET.get("query","")
    resNum = request.GET.get("resNum",16) #每页的游戏个数
    pageNum = request.GET.get("pageNum",1) #页码
    if not query:
        return getResult(False, "keyword empty")
    else:
        # query = query.strip().lower()
        SearchKeyWord.add_query(query)
        baidu_package_name_list, baidu_game_name_list = get_baidu_result_package_name_and_game_name(query)
        lib_result = search_appcenter(query=query,number_per_page=int(resNum),version="",guid="",baidu_package_name_list=baidu_package_name_list,baidu_game_name_list=baidu_game_name_list)

        try:
            result, total_page = merge_baidu_search_result(lib_result,resNum,pageNum)
        except Exception, e:
            print e
            # return getResult(False, "search fail")
            total_page = cal_total_page(len(lib_result),int(resNum))
            return getResult(True, "search success", {"search_result":lib_result, "total_page":total_page})

        return getResult(True, "search success", {"search_result":result, "total_page":total_page})


def memcached_flush_all(request):
    if request.method == "GET":
        try:
            mc_client.flush_all()
            # return HttpResponse("memcached flush all success")
            return HttpResponseRedirect("app_center_html")

        except Exception as e:
            print e
            return HttpResponse("memcached flush all fail")

def app_center_flush(request):
    if request.method == "GET":
        apps = AppCenterGame.objects.all()
        for app in apps:
            game_id = app.find_unique_plateform_game_id()
            if not game_id:
                print "error: ",game_id
                continue
            # print app.pk
            if GameLibrary.objects.filter(pk=game_id).exists():
                game = GameLibrary.objects.get(pk=game_id)
                app.update_app_data(game)
            else:
                app.set_status(False)
        mc_client.flush_all()
        return HttpResponse("success")


def app_center_topic_detail_html(request):
    """
    跳转至专题html
    """
    if request.method == "GET":
        topic_id = request.GET.get("topic_id",None)
        return render(request, 'webhtml/apptopicmuban.html', {"topic_id":topic_id})


def app_center_topic_data(request):
    """
    获取专题详情数据
    """
    if request.method == "GET":
        client = request.GET.get("client","")
        topic_id = request.GET.get("topic_id","")
        if not topic_id or not Topic.objects.filter(pk=int(topic_id)).exists():
            return getResult(False, "topic_id is empty or topic_id do not exist")
        else:
            topic = {}
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                topic = mc_client.get("topic_data@%s" % str(topic_id))
            if not topic:
                topic = Topic.get_topic_data(topic_id)
                if ENVIRONMENT in USE_MC_ENVIRONMENT:
                    mc_client.set("topic_data@%s" % str(topic_id), topic, time=3600*24)
            if not topic["url"]:
                if client == "browser":
                    topic["url"] = "http://%s/bs_browser/app_center_topic_detail_html?topic_id=%s" % (URL_SERVER_HOST, str(topic_id))
                else:
                    topic["url"] = "http://%s/bs/app_center_topic_detail_html?topic_id=%s" % (URL_SERVER_HOST, str(topic_id))
            topic_game = {}
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                topic_game = mc_client.get("topic_game_data@%s" % str(topic_id))
            if not topic_game:
                topic_game = TopicGame.get_game_by_topic_id(topic_id)
                if ENVIRONMENT in USE_MC_ENVIRONMENT:
                    mc_client.set("topic_game_data@%s" % str(topic_id), topic_game, time=3600*24)
            game_data = []
            for i in topic_game:
                app = {}
                if ENVIRONMENT in USE_MC_ENVIRONMENT:
                    app = mc_client.get("app_center_game_data@%s" % str(i["app_center_game_id"]))
                if not app:
                    app = AppCenterGame.get_app_by_id(i["app_center_game_id"])
                    if ENVIRONMENT in USE_MC_ENVIRONMENT:
                        mc_client.set("app_center_game_data@%s" % str(i["app_center_game_id"]), app, time=3600*24)
                tmp = app.toJSON()
                tmp["order"] = i["order"]
                if tmp["icon_tag_id"]:
                    tmp["icon_tag"] = IconTag.get_icon_tag_title_by_id(tmp["icon_tag_id"])
                else:
                    tmp["icon_tag"] = ""
                tmp["quick_download_download_url"], tmp["quick_download_package_name"] = GameLibrary.get_quick_download_info_by_platform_game_ids(tmp["platform_game_ids"])
                game_data.append(tmp)

            return getResult(True, "success", {"topic":topic, "topic_game":game_data})
    else:
        return HttpResponse("use GET method")


def app_center_html(request):
    if request.method == "GET":
        display = False
        ENVIRONMENT_cn = ENVIRONMENT
        if ENVIRONMENT != "aliyun":
            display = True
            if ENVIRONMENT == "aliyun_test_preview" and str(get_local_ip())=="101.201.42.93":
                ENVIRONMENT_cn = "预发环境"
        return render(request, 'webhtml/appcenter.html',{"ENVIRONMENT":ENVIRONMENT, "ip":str(get_local_ip()), "MC":ENVIRONMENT in USE_MC_ENVIRONMENT, "display":display,"ENVIRONMENT_cn":ENVIRONMENT_cn})

def app_center_recommend_html(request):
    if request.method == "GET":
        return render(request, 'webhtml/apprecommend.html')

def app_center_applist_html(request):
    if request.method == "GET":
        return render(request, 'webhtml/applist.html')

def app_center_topic_html(request):
    if request.method == "GET":
        return render(request, 'webhtml/apptopic.html')

def app_center_topic_all_html(request):
    if request.method == "GET":
        return render(request, 'webhtml/apptopicdetail.html')

def app_center_game(request):
    if request.method == "GET":
        return render(request, 'webhtml/app_center_game.html')


def app_center_tag_html(request):
    if request.method == "GET":
        tag_name = request.GET.get("tag","")
        if not tag_name:
            return render(request, 'webhtml/error_404.html')
        else:
            return render(request, "webhtml/apptagmuban.html", {"tag":tag_name})


def app_center_tag_data(request):
    if request.method == "GET":
        tag_name = request.GET.get("tag","")
        if not tag_name:
            return render(request, 'webhtml/error_404.html')
        else:
            # apps = mc_client.get("app_center_tag_data@%s"%tag_name)
            # if not apps:
            apps = AppCenterGame.find_tag(tag_name)
            # mc_client.set("app_center_tag_data@%s"%tag_name, apps, time=3600*24)
            result = []
            for app in apps:
                if app.platform_game_ids == "175":
                    pass
                tmp = app.toJSON()
                tmp["quick_download_download_url"], tmp["quick_download_package_name"] = GameLibrary.get_quick_download_info_by_platform_game_ids(tmp["platform_game_ids"])
                result.append(tmp)
            return getResult(True, "find tag success", {"tag":result})



def app_center_topic_list(request):
    """
    获取topic list
    """
    if request.method == "GET":
        client = request.GET.get("client","")

        data = {}
        data["lunbo"] = Topic.get_topic_data_by_type_without_game("lunbo")
        data['zhuanti'] = Topic.get_topic_data_by_type_without_game("zhuanti")
        # for i in data["lunbo"]:
        #     if not i['url']:
        #         if client == "browser":
        #             i['url'] = "http://%s/bs_browser/app_center_topic_detail_html?topic_id=%s" % (URL_SERVER_HOST, str(i["id"]))
        #         else:
        #             i['url'] = "http://%s/bs/app_center_topic_detail_html?topic_id=%s" % (URL_SERVER_HOST, str(i["id"]))
        # for i in data["zhuanti"]:
        #     if not i['url']:
        #         if client == "browser":
        #             i['url'] = "http://%s/bs_browser/app_center_topic_detail_html?topic_id=%s" % (URL_SERVER_HOST, str(i["id"]))
        #         else:
        #             i['url'] = "http://%s/bs/app_center_topic_detail_html?topic_id=%s" % (URL_SERVER_HOST, str(i["id"]))
        return getResult(True, "app center home success", data)


def app_center_home(request):
    """
    打开首页
    """
    def get_app_center_home_data(client):
        data = {}
        # ENVIRONMENT = "aliyun_test"
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            data = mc_client.get("app_center_home")
        if not data:
            data = {}
            data["selected_game"] = AppCenterList.get_app_and_order_by_flag("selected_game")
            data["latest_game"] = AppCenterList.get_app_and_order_by_flag("latest_game")
            data["hot_list"] = AppCenterList.get_app_and_order_by_flag("hot_list")[:10]
            data["sell_well_list"] = AppCenterList.get_app_and_order_by_flag("sell_well_list")[:10]
            data["app_list"] = AppCenterList.get_app_and_order_by_flag("app_list")[:10]

            data["lunbo"] = Topic.get_topic_data_by_type("lunbo")
            data['zhuanti'] = Topic.get_topic_data_by_type("zhuanti")

            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("app_center_home", data, time=3600*24)
        return data

    if request.method == 'POST':
        return HttpResponse(u"you should use GET method")
    if request.method == 'GET':
        client = request.GET.get("client","")
        data = get_app_center_home_data(client)
        return getResult(True, "app center home success", data)


def get_test_desc(request):
    a = UserFeedBack.objects.get(pk=20)
    return getResult(True, "成功", a.toJSON())


def append_appcenter_board_data(request):
    def init_120_app_for_board():
        data = GameLibrary.objects.all()[100:220]
        for i in data:
            app = AppCenterGame()
            app.title = i.game_name
            app.platform_game_ids = i.pk
            if i.type.find(":"):
                app.type = i.type.split(":")[1]
            else:
                app.type = i.type
            app.icon_url = i.icon_url
            app.size = i.size
            app.download_count = i.download_count
            app.screenshots = i.screenshots
            app.version = i.version
            app.tags = i.tags
            app.level = i.level
            app.instruction = i.instruction
            app.description = i.description
            app.status = 1
            try:
                app.save()
            except Exception as e:
                print e

    def append_120_app_for_board():
        hot_list = RecommendList.objects.get(flag="hot_list")
        sell_well_list = RecommendList.objects.get(flag="sell_well_list")
        app_list = RecommendList.objects.get(flag="app_list")

        app_game_data = AppCenterGame.objects.all()[100:220] #前100个为第一次加入的游戏

        order = 10
        for i in app_game_data[:40]:
            acl = AppCenterList()
            acl.app_list_id = hot_list.pk
            acl.app_center_id = i.pk
            acl.order = order
            acl.save()
            order += 1

        order = 10
        for i in app_game_data[40:80]:
            acl = AppCenterList()
            acl.app_list_id = sell_well_list.pk
            acl.app_center_id = i.pk
            acl.order = order
            acl.save()
            order += 1

        order = 10
        for i in app_game_data[80:120]:
            acl = AppCenterList()
            acl.app_list_id = app_list.pk
            acl.app_center_id = i.pk
            acl.order = order
            acl.save()
            order += 1


    if request.method == "GET":
        # init_120_app_for_board()
        append_120_app_for_board()
        return HttpResponse("success")


def init_appcenter_test_data(request):
    def init_100_app():
        data = GameLibrary.objects.all()[:100]
        for i in data:
            app = AppCenterGame()
            app.title = i.game_name
            app.platform_game_ids = i.pk
            if i.type.find(":"):
                app.type = i.type.split(":")[1]
            else:
                app.type = i.type
            app.icon_url = i.icon_url
            app.size = i.size
            app.download_count = i.download_count
            app.screenshots = i.screenshots
            app.version = i.version
            app.tags = i.tags
            app.level = i.level
            app.instruction = i.instruction
            app.description = i.description
            app.status = 1
            try:
                app.save()
            except Exception as e:
                print e

    def init_appcenterlist():
        selected_game = RecommendList.objects.get(flag="selected_game")
        latest_game = RecommendList.objects.get(flag="latest_game")
        hot_list = RecommendList.objects.get(flag="hot_list")
        sell_well_list = RecommendList.objects.get(flag="sell_well_list")
        app_list = RecommendList.objects.get(flag="app_list")

        online_game_recommend = RecommendList.objects.get(flag="online_game_recommend")
        console_game_recommend = RecommendList.objects.get(flag="console_game_recommend")
        app_recommend = RecommendList.objects.get(flag="app_recommend")

        app_game_data = AppCenterGame.objects.all()[:80]

        order = 0
        for i in app_game_data[:10]:
            acl = AppCenterList()
            acl.app_list_id = selected_game.pk
            acl.app_center_id = i.pk
            acl.order = order
            #acl.save()
            order += 1

        order = 0
        for i in app_game_data[10:20]:
            acl = AppCenterList()
            acl.app_list_id = latest_game.pk
            acl.app_center_id = i.pk
            acl.order = order
            #acl.save()
            order += 1

        order = 0
        for i in app_game_data[20:30]:
            acl = AppCenterList()
            acl.app_list_id = hot_list.pk
            acl.app_center_id = i.pk
            acl.order = order
            #acl.save()
            order += 1

        order = 0
        for i in app_game_data[30:40]:
            acl = AppCenterList()
            acl.app_list_id = sell_well_list.pk
            acl.app_center_id = i.pk
            acl.order = order
            #acl.save()
            order += 1

        order = 0
        for i in app_game_data[40:50]:
            acl = AppCenterList()
            acl.app_list_id = app_list.pk
            acl.app_center_id = i.pk
            acl.order = order
            #acl.save()
            order += 1

        order = 0
        for i in app_game_data[50:60]:
            acl = AppCenterList()
            acl.app_list_id = online_game_recommend.pk
            acl.app_center_id = i.pk
            acl.order = order
            acl.save()
            order += 1

        order = 0
        for i in app_game_data[60:70]:
            acl = AppCenterList()
            acl.app_list_id = console_game_recommend.pk
            acl.app_center_id = i.pk
            acl.order = order
            acl.save()
            order += 1

        order = 0
        for i in app_game_data[70:80]:
            acl = AppCenterList()
            acl.app_list_id = app_recommend.pk
            acl.app_center_id = i.pk
            acl.order = order
            acl.save()
            order += 1

    if request.method == 'GET':
        #获取100条数据
        # init_100_app()
        init_appcenterlist()

        return getResult(True, u"success")


def test_blog(request):
    context = {
        'test': 'just for test.',
        'welcome': 'hello world.'
    }
    return render(request, 'test_html/blog_index.html', context)


def blog_index(request):
    return HttpResponse("Hello, world.")


# def app_center_type(request):
#     """
#     标签页面
#     """
#     if request.method == "GET":
#         type = request.GET.get("type","")
#         return HttpResponse(type)


# def app_detail(request):
#     """
#     应用详情页面
#     """
#     if request.method == "GET":
#         data = {}
#         id = request.GET.get("id",None)
#         if not id:
#             return render(request, 'webhtml_browser/error_404.html')
#         platform_game_ids = AppCenterGame.get_platform_game_ids(id)
#         if not platform_game_ids:
#             return render(request, 'webhtml_browser/error_404.html')
#         else:
#             platform_game_ids = [int(i) for i in platform_game_ids.split(",")]
#             result = GameLibrary.get_app_detail_by_ids(platform_game_ids)
#             for i in result:
#                 data[i.get("platform_name")] = i
#             return render(request, 'webhtml_browser/appgamedetail.html', {"game_data":data})
#     else:
#         return HttpResponse("use GET method")


def ejs_test(request):
    return render(request, 'test_html/ejs_test/ejs_test.html')


def app_detail_html(request):
    """
    返回详情页html
    """
    if request.method == "GET":
        cid = request.GET.get("cid",None)
        game_library_id = request.GET.get("game_library_id",None)
        total_360_id = request.GET.get("total_360_id",None)
        if game_library_id:
            return render(request, 'webhtml/appgamedetail_search.html', {"game_library_id":game_library_id})
        elif cid:
            return render(request, 'webhtml/appgamedetail_baidu.html', {"cid":cid})
        elif total_360_id:
            return render(request, 'webhtml/appgamedetail_total_360.html', {"total_360_id":total_360_id})
        else:
            id = request.GET.get("id",None)
            return render(request, 'webhtml/appgamedetail.html', {"id":id})


def app_recommend_html(request):
    """
    返回推荐详情html
    """
    if request.method == "GET":
        flag = request.GET.get("flag",None)
        return render(request, 'webhtml/appgamelist.html', {"flag":flag})


def app_recommend_data(request):
    """
    获取推荐详情
    """
    if request.method == "GET":
        flag = request.GET.get("flag",None)
        if not flag:
            return render(request, 'webhtml/error_404.html')
        # reco_id = RecommendList.get_reco_id_by_flag(flag)
        apps = AppCenterList.get_app_and_order_by_flag(flag)
        if not apps:
            return render(request, 'webhtml/error_404.html')
        else:
            return getResult(True, "app recommend data success", apps)
    else:
        return HttpResponse("use GET method")


def app_detail_data(request):
    """
    获取应用详情
    评星：对于搜索的，评星跟渠道包走 对于推荐的，平行跟设置的分数走
    """

    if request.method == "GET":
        # data = {}
        game_library_id = request.GET.get("game_library_id",None)
        if game_library_id:
            game = GameLibrary.get_app_detail_by_ids([game_library_id])
            return getResult(True, "success", {"comments":[],"game_data":game[0]})

        total_360_id = request.GET.get("total_360_id",None)
        if total_360_id:
            _360 = Total360Game.get_app_detail_by_id(total_360_id)
            return getResult(True, "success", {"comments":[],"game_data":_360})

        cid = request.GET.get("cid",None)
        #百度使用
        if cid:
            url = "http://frsearch.duoku.com/MGameQuery/queryDetail.do"
            data = {"cid":str(cid),"channelId":"5004166"}
            r = requests.post(url, json.dumps(data), timeout=5)
            result_data = json.loads(r.text)
            if result_data:
                result_data['datas'][0]["screenshots"] = [result_data['datas'][0]["imageUrl1"],
                                                          result_data['datas'][0]["imageUrl2"],
                                                          result_data['datas'][0]["imageUrl3"],
                                                          result_data['datas'][0]["imageUrl4"],
                                                          result_data['datas'][0]["imageUrl5"]]
                result_data['datas'][0]["type"] = [result_data['datas'][0]["type"]]
                result_data['datas'][0]["modifyDate"] = result_data['datas'][0]["modifyDate"][:10]
            return getResult(True, "success", {"game_data":result_data,"platform":"百度"})
        else:
            game_data = []
            id = request.GET.get("id",None)
            # platform_name = request.GET.get("platform",None)
            if not id:      #搜索appgamecenter
                # return render(request, 'webhtml/error_404.html')
                return getResult(False, "id can not be empty")
            else:
                platform_game_ids = {}
                if ENVIRONMENT in USE_MC_ENVIRONMENT:
                    platform_game_ids = mc_client.get("plateform_game_ids@%s" % str(id))
                if not platform_game_ids:
                    platform_game_ids = AppCenterGame.get_platform_game_ids(id)
                    if ENVIRONMENT in USE_MC_ENVIRONMENT:
                        mc_client.set("plateform_game_ids@%s" % str(id), platform_game_ids, time=3600*24)
                if not platform_game_ids:
                    # return render(request, 'webhtml/error_404.html')
                    return getResult(False, "id is empty")
                else:
                    platform_game_ids = {}
                    if ENVIRONMENT in USE_MC_ENVIRONMENT:
                        platform_game_ids = mc_client.get("plateform_game_ids@%s" % str(id))
                    if not platform_game_ids:
                        platform_game_ids = AppCenterGame.get_platform_game_ids(id)
                        if ENVIRONMENT in USE_MC_ENVIRONMENT:
                            mc_client.set("plateform_game_ids@%s" % str(id), platform_game_ids, time=3600*24)
                    if not platform_game_ids:
                        # return render(request, 'webhtml/error_404.html')
                        return getResult(False, "platform_game_ids is empty")
                    else:
                        # comments = mc_client.get("comments@%s" % str(id))
                        # if not comments:
                        comments = RatingComment.get_comment_by_app_center_id(id)
                            # mc_client.set("comments@%s" % str(id), comments, time=3600*24)
                        platform_game_ids = [int(i) for i in platform_game_ids.split(",")]
                        result = {}
                        if ENVIRONMENT in USE_MC_ENVIRONMENT:
                            result = mc_client.get("app_detail_data@%s" % str(platform_game_ids))
                        if not result:
                            result = GameLibrary.get_app_detail_by_ids(platform_game_ids)
                            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                                mc_client.set("app_detail_data@%s" % str(platform_game_ids), result, time=3600*24)
                        game_platform = []
                        for i in result:
                            game_platform.append(i["platform_name"])
                            i["level"] = AppCenterGame.get_level_by_id(id)
                        return getResult(True, "success", {"comments":comments,"game_data":result,"game_count":len(result),"platform":game_platform})

    else:
        return HttpResponse("use GET method")


@csrf_exempt
@login_required
def obs_info_edit(request):
    """
    obs信息编辑
    """
    if request.method == "GET":
        if request.user.username == "mingbin":
            return render(request, 'webhtml/obs_info_edit.html')
        else:
            return getResult(False, "you don't have the right to access this function")

    if request.method == "POST":
        if request.user.username != "mingbin":
            return getResult(False, "you don't have the right to access this function")

        version = request.POST.get("version", "")
        file_url = request.POST.get("download_url", "")
        md5 = request.POST.get("md5","")
        desc = request.POST.get("desc", "")
        mandatory = request.POST.get("mandatory", False)

        if not file_url or not version:
            return HttpResponse("download_url and version can not be empty")
        else:
            obs = ObsInfo()
            obs.version = version
            obs.file_url = file_url
            obs.md5 = md5
            obs.desc = desc
            obs.mandatory = mandatory
            try:
                obs.save()
                return HttpResponse("success")
            except Exception, e:
                print e
                return HttpResponse("fail, contact andy@bluestacks.com please")


@csrf_exempt
@login_required
def update_info_edit(request):
    """
    增加升级信息
    """
    if request.method == "GET":
        if request.user.username == "mingbin":
            return render(request, 'webhtml/update_info_edit.html')
        else:
            return getResult(False, "you don't have the right to access this function")

    if request.method == "POST":
        if request.user.username != "mingbin":
            return getResult(False, "you don't have the right to access this function")

        last_version = request.POST.get("last_version", "")
        download_url = request.POST.get("download_url", "")
        md5 = request.POST.get("md5","")
        engine_desc = request.POST.get("engine_desc", "")
        mandatory_engine = request.POST.get("mandatory_engine", False)
        if mandatory_engine == "True":
            mandatory_engine = True
        else:
            mandatory_engine = False


        last_version_product = request.POST.get("last_version_product", "")
        download_url_product = request.POST.get("download_url_product", "")
        md5_product = request.POST.get("md5_product","")
        product_desc = request.POST.get("product_desc","")
        mandatory_product = request.POST.get("mandatory_product", False)
        if mandatory_product == "True":
            mandatory_product = True
        else:
            mandatory_product = False


        new_last_version = request.POST.get("new_last_version", "")
        new_download_url = request.POST.get("new_download_url", "")
        new_md5 = request.POST.get("new_md5", "")
        engine_new_desc = request.POST.get("engine_new_desc", "")

        if last_version and download_url:  #engine
            a = OssPackage()
            a.file_url = download_url.strip()
            a.status = True
            a.file_type = "zip"
            a.bucket = "bst-appcenter"
            a.upload_dir = "package"
            a.type = "BlueStacksKK_DeployTool"
            a.version = last_version.strip()
            a.md5 = md5
            a.desc = engine_desc
            a.mandatory = mandatory_engine
            try:
                a.save()
            except Exception, e:
                print e
                return HttpResponse("fail, please contact andy")
        elif last_version_product and download_url_product:   #product
            a = OssPackage()
            a.file_url = download_url_product.strip()
            a.status = True
            a.file_type = "exe"
            a.bucket = "bst-appcenter"
            a.upload_dir = "package"
            a.type = "BluestacksCnSetup"
            a.version = last_version_product.strip()
            a.md5 = md5_product
            a.desc = product_desc
            a.mandatory = mandatory_product
            try:
                a.save()
            except Exception, e:
                print e
                return HttpResponse("fail, please contact andy")
        elif new_last_version and new_download_url:         #new engine
            a = OssPackage()
            a.file_url = new_download_url.strip()
            a.status = True
            a.file_type = "zip"
            a.bucket = "bst-appcenter"
            a.upload_dir = "package"
            a.type = "BlueStacksKK_DeployTool_new"
            a.version = new_last_version.strip()
            a.md5 = new_md5
            a.desc = engine_new_desc
            a.mandatory = False
            try:
                a.save()
            except Exception, e:
                print e
                return HttpResponse("fail, please contact andy")
        else:
            return HttpResponse("fail")

        return HttpResponse("success")


def check_package_name(request):
    """
    检查包名是否是我们的游戏
    """
    if request.method != "GET":
        return getResult(False, "use GET method")
    else:
        package_name = request.GET.get("package_name","")
        if not package_name:
            return getResult(False, "package_name is empty")
        else:
            app = None
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                app = mc_client.get("check_package_name@%s" % str(package_name))
            if not app:
                app = GameLibrary.check_package_name(package_name) or Total360Game.check_package_name(package_name)
                if ENVIRONMENT in USE_MC_ENVIRONMENT:
                    mc_client.set("check_package_name@%s" % str(package_name), app, time=3600*24)
            if not app:
                return getResult(False, "bluestacks do not have this app")
            else:
                return getResult(True, {"app":app})


@check_signature
def emulator_update_info(request):
    """
    模拟器版本信息
    """
    try:
        a = {}
        if ENVIRONMENT in USE_MC_ENVIRONMENT:
            a = mc_client.get("emulator_update_info")
        if not a:
            a = OssPackage.objects.filter(type="BluestacksCnSetup").order_by("-uptime")[0]
            a = a.toJSON()
            if a['file_url']:
                r = requests.head(a['file_url'])
                size = r.headers.get("Content-Length",0)
            else:
                size = 0
            a["size"] = size
            if ENVIRONMENT in USE_MC_ENVIRONMENT:
                mc_client.set("emulator_update_info", a, time=3600)
        return getResult(True, {"info":a})
    except Exception, e:
        return getResult(False,{"info": "None"})




@check_signature
def obs_info(request):
    """
    obs信息
    """
    if request.method != "GET":
        return getResult(False, "use GET method")
    else:
        try:
            obs = ObsInfo.objects.filter(status=True).order_by("-uptime")
            if obs.exists():
                obs = obs[0]
                return getResult(True, "get obs success", {
                    "download_url" : obs.file_url,
                    "version": obs.version,
                    "md5": obs.md5,
                    "desc": obs.desc,
                    "mandatory": obs.mandatory
                })
            else:
                return getResult(False, "obs do not exist", {"obs":None})
        except Exception,e:
            print e
            return HttpResponse("error")


def update_info(request):
    """
    更新信息
    """
    if request.method != "GET":
        return getResult(False, "use GET method")
    else:
        product = OssPackage.objects.filter(status=True,type="BluestacksCnSetup").order_by("-uptime")[0]
        download = OssPackage.objects.filter(status=True,type="BlueStacksKK_DeployTool").order_by("-uptime")[0]
        if OssPackage.objects.filter(status=True,type="BlueStacksKK_DeployTool_new"):
            new_download = OssPackage.objects.filter(status=True,type="BlueStacksKK_DeployTool_new").order_by("-uptime")[0]
        else:
            new_download = OssPackage.objects.filter(status=True,type="BlueStacksKK_DeployTool").order_by("-uptime")[0]
            new_download.file_url = ""
            new_download.version = ""
            new_download.md5 = ""

        if download.mandatory:
            mandatory_engine = "true"
        else:
            mandatory_engine = "false"
        if product.mandatory:
            mandatory_product = "true"
        else:
            mandatory_product = "false"

        return getResult(True, "get update info success", {"download_url":download.file_url,
                                                           "last_version":download.version,
                                                           "md5":download.md5,
                                                           "engine_desc":download.desc,
                                                           "mandatory_engine":mandatory_engine,

                                                            "new_download_url":new_download.file_url,
                                                            "new_last_version":new_download.version,
                                                            "new_md5":new_download.md5,
                                                            "engine_new_desc":new_download.desc,

                                                           "download_url_product":product.file_url,
                                                           "last_version_product":product.version,
                                                           "md5_product":product.md5,
                                                           "product":product.desc,
                                                           "mandatory_product":mandatory_product

                                                           })


# def update_info(request):
#     """
#     更新信息
#     """
#     if request.method != "GET":
#         return getResult(False, "use GET method")
#     else:
#         download = ""
#         product = ""
#
#         BluestacksCnSetups = OssPackage.objects.filter(status=True,type="BluestacksCnSetup").order_by("-uptime")
#         for BluestacksCnSetup in BluestacksCnSetups:
#             if check_for_last_file_upload_success(url=BluestacksCnSetup.file_url,
#                                               bucket=BluestacksCnSetup.bucket,
#                                               uptime=BluestacksCnSetup.uptime,key=BluestacksCnSetup.key):
#                 product = BluestacksCnSetup
#                 break
#
#
#
#         BlueStacksKK_DeployTools = OssPackage.objects.filter(status=True,type="BlueStacksKK_DeployTool").order_by("-version")
#         for BlueStacksKK_DeployTool in BlueStacksKK_DeployTools:
#             if check_for_last_file_upload_success(url=BlueStacksKK_DeployTool.file_url,
#                                               bucket=BlueStacksKK_DeployTool.bucket,
#                                               uptime=BlueStacksKK_DeployTool.uptime,key=BlueStacksKK_DeployTool.key):
#                 download = BlueStacksKK_DeployTool
#                 break
#
#
#
#         return getResult(True, "get update info success", {"download_url":download.file_url,
#                                                            "last_version":download.version,
#                                                            "download_url_product":product.file_url,
#                                                             "last_version_product":product.version
#                                                            })



@csrf_exempt
@check_signature
def rating_comment(request):
    """
    提交评分和评论
    """
    if request.method == "POST":
        guid = request.POST.get("guid","")
        app_center_id = request.POST.get("app_center_id","")
        # level = float(request.POST.get("level",0.0))
        level = request.POST.get("level",0.0)
        if not level: #菲菲会传""
            level = 0.0
        title = request.POST.get("title","")
        content = request.POST.get("content","")
        if not guid or not title or not app_center_id:
            # return HttpResponseRedirect("app_detail_html?id=%s"%str(app_center_id))
            return getResult(False, "parameter error")
        else:
            try:
                result = RatingComment.add_rating_comment(guid,app_center_id,level,title,content)
                if result == 50:
                    return getResult(False, "more than 50 times")
                elif result:
                    # return HttpResponseRedirect("app_detail_html?id=%s"%str(app_center_id))
                    return getResult(True, "add rating comment success" ,{"title":title, "content":content, "uptime":datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')})
                else:
                    # return HttpResponseRedirect("app_detail_html?id=%s"%str(app_center_id))
                    return getResult(False, "this user already rating this game OR app center game id not exsit")
            except Exception as e:
                print e
                # return HttpResponseRedirect("app_detail_html?id=%s"%str(app_center_id))
                return getResult(False, "add rating comment fail")

    elif request.method == "GET":
        app_center_id = request.GET.get("app_center_id","")
        if not app_center_id:
            return getResult(False, "add rating comment fail", [])
        else:
            result = RatingComment.get_comment_by_app_center_id(app_center_id)
            return getResult(True, "get comment success", result)

@csrf_exempt
def post_test(request):
    if request.method == "POST":
        post = request.POST.get("data","")
        return HttpResponse(post)
    elif request.method == "GET":
        get = request.GET.get("data","")
        return HttpResponse(get)





################test###############################3
# from django.contrib.admin.views.decorators import staff_member_required
#
# class AdminRequiredMixin(object):
#     @classmethod
#     def as_view(cls, **initkwargs):
#         view = super(AdminRequiredMixin, cls).as_view(**initkwargs)
#         return staff_member_required(view)
#
#
#
#
# class ArticleListView(ListView):
#     template_name = 'test_html/blog_index.html'
#
#     def get_queryset(self, **kwargs):
#         object_list = Article.objects.all().order_by(F('created').desc())[:100]
#         paginator = Paginator(object_list, 10)
#         page = self.request.GET.get('page')
#         try:
#             object_list = paginator.page(page)
#         except PageNotAnInteger:
#             # If page is not an integer, deliver first page.
#             object_list = paginator.page(1)
#         except EmptyPage:
#             # If page is out of range (e.g. 9999), deliver last page of results.
#             object_list = paginator.page(paginator.num_pages)
#         return object_list
#
#
# class ArticlePublishView(AdminRequiredMixin, FormView):
#     template_name = 'test_html/article_publish.html'
#     form_class = ArticlePublishForm
#     success_url = '/bs/test_blog'
#
#     def form_valid(self, form):
#         form.save(self.request.user.username)
#         return super(ArticlePublishView, self).form_valid(form)


# class ArticleDetailView(DetailView):
#     template_name = 'test_html/article_detail.html'
#
#     def get_object(self, **kwargs):
#         title = self.kwargs.get('title')
#         try:
#             article = Article.objects.get(title=title)
#             article.views += 1
#             article.save()
#             article.tags = article.tags.split()
#         except Article.DoesNotExist:
#             raise Http404("Article does not exist")
#         return article
#
#
# class ArticleEditView(AdminRequiredMixin, FormView):
#     template_name = 'test_html/article_publish.html'
#     form_class = ArticlePublishForm
#     article = None
#
#     def get_initial(self, **kwargs):
#         title = self.kwargs.get('title')
#         try:
#             self.article = Article.objects.get(title=title)
#             initial = {
#                 'title': title,
#                 'content': self.article.content_md,
#                 'tags': self.article.tags,
#             }
#             return initial
#         except Article.DoesNotExist:
#             raise Http404("Article does not exist")
#
#     def form_valid(self, form):
#         form.save(self.request, self.article)
#         return super(ArticleEditView, self).form_valid(form)
#
#     def get_success_url(self):
#         title = self.request.POST.get('title')
#         success_url = reverse('article_detail', args=(title,))
#         return success_url

# from django.core.urlresolvers import reverse_lazy
# from django.views.generic.edit import FormView
# from django.contrib.auth import authenticate, login
# from forms import RegisterForm
#
# class RegisterView(FormView):
#     template_name = 'test_html/register.html'
#     form_class = RegisterForm
#     success_url = reverse_lazy('blog_index')
#
#     def form_valid(self, form):
#         form.save()
#         username = form.cleaned_data.get('username')
#         password = form.cleaned_data.get('password')
#         user = authenticate(username=username, password=password)
#         login(self.request, user)
#         return super(RegisterView, self).form_valid(form)
