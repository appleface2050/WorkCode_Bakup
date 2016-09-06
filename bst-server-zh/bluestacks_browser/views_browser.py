# coding=utf-8
from django.shortcuts import render

# Create your views here.
from bluestacks.models import AppCenterGame
from util.jsonresult import getResult

from django.http import HttpResponse, Http404, HttpResponseRedirect

def index(request):
    # return HttpResponse("Hello, world")
    # context = {"id":11}
    # return render(request, 'webhtml_browser/index.html', context)
    # return render(request, 'webhtml_browser/index.html')
    return HttpResponseRedirect("app_center_html")


def feedback_html(request):
    """
    feedback 页面
    """
    return render(request, 'webhtml_browser/feedback.html')


def app_center_html(request):
    if request.method == "GET":
        return render(request, 'webhtml_browser/appcenter.html')

def app_center_recommend_html(request):
    if request.method == "GET":
        return render(request, 'webhtml_browser/apprecommend.html')

def app_center_applist_html(request):
    if request.method == "GET":
        return render(request, 'webhtml_browser/applist.html')

def app_center_topic_html(request):
    if request.method == "GET":
        return render(request, 'webhtml_browser/apptopic.html')

def app_center_topic_all_html(request):
    if request.method == "GET":
        return render(request, 'webhtml_browser/apptopicdetail.html')

def app_center_game(request):
    if request.method == "GET":
        return render(request, 'webhtml_browser/app_center_game.html')


def app_center_tag_html(request):
    if request.method == "GET":
        tag_name = request.GET.get("tag","")
        if not tag_name:
            return render(request, 'webhtml_browser/error_404.html')
        else:
            return render(request, "webhtml_browser/apptagmuban.html", {"tag":tag_name})


def app_detail_html(request):
    """
    返回详情页html
    """
    if request.method == "GET":
        cid = request.GET.get("cid",None)
        game_library_id = request.GET.get("game_library_id",None)
        total_360_id = request.GET.get("total_360_id",None)
        if game_library_id:
            return render(request, 'webhtml_browser/appgamedetail_search.html', {"game_library_id":game_library_id})
        elif cid:
            return render(request, 'webhtml_browser/appgamedetail_baidu.html', {"cid":cid})
        elif total_360_id:
            return render(request, 'webhtml_browser/appgamedetail_total_360.html', {"total_360_id":total_360_id})
        else:
            id = request.GET.get("id",None)
            return render(request, 'webhtml_browser/appgamedetail.html', {"id":id})


def app_recommend_html(request):
    """
    返回推荐详情html
    """
    if request.method == "GET":
        flag = request.GET.get("flag",None)
        return render(request, 'webhtml_browser/appgamelist.html', {"flag":flag})


def app_center_topic_detail_html(request):
    """
    跳转至专题html
    """
    if request.method == "GET":
        topic_id = request.GET.get("topic_id",None)
        return render(request, 'webhtml_browser/apptopicmuban.html', {"topic_id":topic_id})


def appsearch_html(request):
    """
    搜索html
    """
    if request.method == 'GET':
        return render(request, 'webhtml_browser/appsearch.html')


def topic_pokemongo(request):
    return render(request, "webhtml_browser/topic_pokemongo.html")


