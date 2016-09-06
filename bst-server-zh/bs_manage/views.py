#coding=utf-8
import datetime

from django.contrib.auth.decorators import login_required
from django.http import HttpResponse, HttpResponseRedirect
from django.shortcuts import render
from django.core.paginator import Paginator, EmptyPage, PageNotAnInteger

from django.db.models import Sum,Count

# Create your views here.
from bluestacks.models import UserFeedBack,Topic, TopicGame, AppCenterGame, RecommendList, AppCenterList, GameLibrary, \
    Platform, GameLibraryModify, SearchKeyWord, IconTag, SearchDefaultQuery, RecommendInstallAPP, PartnerPreInstallGame, PopWindow

from django.views.decorators.csrf import csrf_exempt

from util.jsonresult import getResult

@login_required
def index(request):
    return render(request, 'manage_webhtml/index.html')

# @login_required
def feedback(request):
    # if request.method == "GET":
    #     return HttpResponse("feedback")
    feedback_list = UserFeedBack.objects.all().order_by('-create_time')
    for i in feedback_list:
        if i.type is None:
            i.type = ""
    paginator = Paginator(feedback_list, 10)
    page = request.GET.get('page')
    try:
        feedbacks = paginator.page(page)
    except PageNotAnInteger:
        # If page is not an integer, deliver first page.
        feedbacks = paginator.page(1)
    except EmptyPage:
        # If page is out of range (e.g. 9999), deliver last page of results.
        feedbacks = paginator.page(paginator.num_pages)
    return render(request, 'manage_webhtml/feedback.html', {'feedbacks': feedbacks, 'page_range':paginator.page_range})

@login_required
def topic(request):
    data = {"name":"Bluestacks Topic"}
    topic = Topic.objects.all().order_by("-uptime")
    topic_list = []
    for i in topic:
        topic_list.append(i.toJSON())
    data["topic"] = topic_list
    return render(request, 'manage_webhtml/topic.html', data)

@login_required
def topic_game(request):
    if request.method == "GET":
        topic_id = request.GET.get("topic_id","")
        if not topic_id:
            return HttpResponse("topic_id=%s do not exist" % str(topic_id))
        else:
            topic_game = TopicGame.get_game_by_topic_id(topic_id)
            for game in topic_game:
                app = AppCenterGame.get_app_by_id(game["app_center_game_id"])
                if not app:
                    continue
                else:
                    app = super(AppCenterGame, app).toJSON()
                    if not app:
                        game["title"] = ""
                    else:
                        game["title"] = app["title"]
                        game["app"] = app

            return render(request, 'manage_webhtml/topic_game.html', {"topic_game":topic_game, "topic_id":topic_id})

@login_required
def app_center_game_detail(request):
    if request.method == "GET":
        app_center_id = request.GET.get("app_center_id","")
        app = AppCenterGame.get_app_by_id(app_center_id)
        app = app.toJSON_manage()
        return render(request, 'manage_webhtml/app_center_game_detail.html', {"app":app})

@login_required
@csrf_exempt
def topic_game_order_edit(request):
    if request.method == "POST":
        topic_id = request.POST.get("topic_id","")
        app_center_game_id = request.POST.get("app_center_game_id","")
        order = request.POST.get("order","")
        TopicGame.change_order(topic_id,app_center_game_id,order)
        return getResult(True, "order changed", {"topic_id":topic_id})
        # return HttpResponseRedirect("topic_game?topic_id=%s" % str(topic_id))

@login_required
def app_center_game_edit(request):
    if request.method == "GET":
        app_center_id = request.GET.get("app_center_id","")
        app = AppCenterGame.get_app_by_id(app_center_id)
        app = app.toJSON_manage()
        return render(request, 'manage_webhtml/app_center_game_detail.html', {"app":app})

    if request.method == "POST":
        app_center_id = request.POST.get("app_center_id","")
        app = AppCenterGame.objects.get(id=app_center_id)
        app.description = request.POST.get("description","")
        app.icon_tag_id = request.POST.get("icon_tag_id","")
        app.instruction = request.POST.get("instruction","")
        app.level = request.POST.get("level","")

        status = request.POST.get("level","")
        if status == "1":
            app.status = True
        else:
            app.status = False
        app.title = request.POST.get("title","")
        app.icon_url = request.POST.get("icon_url","")
        app.type = request.POST.get("type","")
        app.save()
        return render(request, 'manage_webhtml/app_center_game_detail.html', {"app":app})



@login_required
def app_center_game_manage(request):
    query = request.GET.get("query","")
    res_number = 50
    page = int(request.GET.get('page',"1"))
    if query:
        apps = AppCenterGame.objects.filter(title__contains=query).order_by("-uptime")
    else:
        apps = AppCenterGame.objects.filter().order_by("-uptime")
    paginator = Paginator(apps, res_number)
    try:
        apps = paginator.page(page)
    except PageNotAnInteger:
        # If page is not an integer, deliver first page.
        apps = paginator.page(1)
    except EmptyPage:
        # If page is out of range (e.g. 9999), deliver last page of results.
        apps = paginator.page(paginator.num_pages)
    return render(request, 'manage_webhtml/app_center_game_manage.html', {"apps":apps,'page_range':paginator.page_range})

@login_required
@csrf_exempt
def topic_game_delete(request):
    if request.method == "POST":
        topic_id = request.POST.get("topic_id","")
        app_center_game_id = request.POST.get("app_center_game_id","")
        TopicGame.delete_topic_game_by_topic_id_and_app_center_game_id(topic_id,app_center_game_id)
        return getResult(True, "topic game deleted", {"topic_id":topic_id})

@login_required
@csrf_exempt
def topic_game_add(request):
    if request.method == "POST":
        topic_id = request.POST.get("topic_id","")
        app_center_game_id = request.POST.get("app_center_game_id","")
        if not TopicGame.add_topic_game_by_topic_id_and_app_center_game_id(topic_id,app_center_game_id):
            return HttpResponse("add game fail")
        else:
            return HttpResponseRedirect("topic_game?topic_id=%s" % str(topic_id))

@login_required
@csrf_exempt
def topic_delete(request):
    if request.method == "POST":
        topic_id = request.POST.get("topic_id","")
        Topic.delete_topic_by_id(topic_id)
        return HttpResponseRedirect("topic")

@login_required
@csrf_exempt
def topic_edit(request):
    if request.method == "GET":
        topic_id = request.GET.get("topic_id","")
        topic = Topic.get_topic_data(topic_id)
        return render(request, 'manage_webhtml/topic_edit.html', {"topic":topic})

    elif request.method == "POST":
        topic_id = request.POST.get("topic_id","")
        flag = request.POST.get("flag","")
        big_image_url = request.POST.get("big_image_url","")
        small_image_url = request.POST.get("small_image_url","")
        order = request.POST.get("order","")
        topic_name = request.POST.get("topic_name","")
        type = request.POST.get("type","")
        url = request.POST.get("url","")
        status = request.POST.get("status_name","")
        game_libaray_id = request.POST.get("game_libaray_id","")
        if status == "False":
            status = False
        else:
            status = True

        if not Topic.objects.filter(pk=topic_id).exists():
            return HttpResponse("error topic id not exist")
        else:
            topic = Topic.objects.get(pk=topic_id)
            topic.flag = flag
            topic.big_image_url = big_image_url
            topic.small_image_url = small_image_url
            topic.order = order
            topic.topic_name = topic_name
            topic.type = type
            if url == "None":
                url = None
            topic.url = url
            topic.status = status
            if game_libaray_id == "None" or game_libaray_id == "":
                game_libaray_id = None
            topic.game_libaray_id = game_libaray_id
            topic.save()
            # return HttpResponseRedirect("topic_edit?topic_id=%s"%str(topic_id))
            return HttpResponseRedirect("topic")

@login_required
@csrf_exempt
def topic_add(request):
    if request.method != "POST":
        return HttpResponse("use POST method")
    else:
        topic_name = request.POST.get("topic_name","")
        if topic_name:
            Topic.add_topic(topic_name)
        return HttpResponseRedirect("topic")

@login_required
def recommend_game(request):
    if request.method == "GET":
        rec_id = request.GET.get("rec_id","")
        if not rec_id:
            return HttpResponse("rec_id=%s do not exist" % str(rec_id))
        else:
            recommend_games = AppCenterList.get_game_by_rec_id(rec_id)
            return render(request, 'manage_webhtml/recommend_game.html', {"recommend_games":recommend_games, "rec_id":rec_id})

@login_required
def recommend(request):
    apps = RecommendList.get_all_recommend()
    recommend = []
    for app in apps:
        recommend.append(app)
    return render(request, 'manage_webhtml/recommend.html', {"recommend":recommend})

@login_required
def recommend_add(request):
    if request.method != "POST":
        return HttpResponse("use POST method")
    else:
        title = request.POST.get("title","")
        if title:
            RecommendList.add_recommend(title)
        return HttpResponseRedirect("recommend")

@login_required
@csrf_exempt
def recommend_game_delete(request):
    if request.method == "POST":
        app_center_game_id = request.POST.get("app_center_game_id","")
        rec_id = request.POST.get("rec_id","")
        if AppCenterList.delete_game_by_rec_id_app_center_game_id(rec_id,app_center_game_id):
            return getResult(True, "success")
        else:
            return getResult(False, "fail")

@login_required
@csrf_exempt
def recommend_game_add(request):
    if request.method == "POST":
        rec_id = request.POST.get("rec_id","")
        app_center_game_id = request.POST.get("app_center_game_id","")
        if AppCenterList.add_game_by_rec_id_app_center_game_id(rec_id,app_center_game_id):
            # return getResult(True, "success")
            return HttpResponseRedirect("recommend_game?rec_id=%s"%str(rec_id))
        else:
            return getResult(False, "fail, check if the game id exsit")

@login_required
@csrf_exempt
def conf_partner_preinstall_game_info(request):
    if request.method == "GET":
        result = []
        data = PartnerPreInstallGame.objects.all()
        for i in data:
            result.append(i.toJSON())
        return render(request, "manage_webhtml/conf_partner_preinstall_game_info.html", {"datas":result})

    elif request.method == "POST":
        id = request.POST.get("id","")
        partner = request.POST.get("partner","")
        partner_name_zh = request.POST.get("partner_name_zh","")
        game_name = request.POST.get("game_name","")
        package_name = request.POST.get("package_name","")
        download_url = request.POST.get("download_url","")
        icon_url = request.POST.get("icon_url","")

        if not id:
            return HttpResponse("id empty")
        if not partner:
            return HttpResponse("partner empty")
        if not package_name:
            return HttpResponse("package_name empty")
        if not download_url:
            return HttpResponse("download_url empty")
        if not icon_url:
            return HttpResponse("icon_url empty")
        if PartnerPreInstallGame.objects.filter(pk=id).exists():
            a = PartnerPreInstallGame.objects.get(pk=id)
            a.partner = partner
            a.partner_name_zh = partner_name_zh
            a.game_name = game_name
            a.package_name = package_name
            a.download_url = download_url
            a.icon_url = icon_url
            try:
                a.save()
                return HttpResponse("success")
            except Exception,e:
                print e
                return HttpResponse("fail")
        # HttpResponseRedirect("conf_partner_preinstall_game_info")

@login_required
@csrf_exempt
def conf_partner_preinstall_game_info_delete(request):
    if request.method == "POST":
        data_id = request.POST.get("data_id","")
        try:
            PartnerPreInstallGame.objects.get(pk=data_id).delete()
        except Exception,e:
            print e
        return HttpResponseRedirect("conf_partner_preinstall_game_info")


@login_required
def conf_partner_preinstall_game_info_add(request):
    if request.method == "POST":
        partner = request.POST.get("partner","")
        partner_name_zh = request.POST.get("partner_name_zh","")
        game_name = request.POST.get("game_name","")
        package_name = request.POST.get("package_name","")
        download_url = request.POST.get("download_url","")
        icon_url = request.POST.get("icon_url","")

        if not partner:
            return HttpResponse("partner empty")
        if not package_name:
            return HttpResponse("package_name empty")
        if not download_url:
            return HttpResponse("download_url empty")
        if not icon_url:
            return HttpResponse("icon_url empty")

        PartnerPreInstallGame.add(partner,partner_name_zh,game_name,package_name,download_url,icon_url)
        return HttpResponseRedirect("conf_partner_preinstall_game_info")


@login_required
@csrf_exempt
def conf_pop_window(request):
    if request.method == "GET":
        return render(request, "manage_webhtml/conf_pop_window.html")

    elif request.method == "POST":
        big_image_url = request.POST.get("big_image_url","")
        big_image_download_url = request.POST.get("big_image_download_url","")
        big_image_height = request.POST.get("big_image_height","")
        big_image_width = request.POST.get("big_image_width","")

        small_image_url = request.POST.get("small_image_url","")

        exit_button_icon_url = request.POST.get("exit_button_icon_url","")
        exit_button_height = request.POST.get("exit_button_height","")
        exit_button_width = request.POST.get("exit_button_width","")
        exit_button_margin_top = request.POST.get("exit_button_margin_top","")
        exit_button_margin_right = request.POST.get("exit_button_margin_right","")

        button1_type = request.POST.get("button1_type","")
        button1_redirect_url = request.POST.get("button1_redirect_url","")
        button1_icon_url = request.POST.get("button1_icon_url","")
        button1_download_url = request.POST.get("button1_download_url","")
        button1_height = request.POST.get("button1_height","")
        button1_width = request.POST.get("button1_width","")
        button1_margin_top = request.POST.get("button1_margin_top","")
        button1_margin_left = request.POST.get("button1_margin_left","")
        button1_app_name = request.POST.get("button1_app_name","")
        button1_package_name = request.POST.get("button1_package_name","")

        button2_type = request.POST.get("button2_type","")
        button2_redirect_url = request.POST.get("button2_redirect_url","")
        button2_icon_url = request.POST.get("button2_icon_url","")
        button2_download_url = request.POST.get("button2_download_url","")
        button2_height = request.POST.get("button2_height","")
        button2_width = request.POST.get("button2_width","")
        button2_margin_top = request.POST.get("button2_margin_top","")
        button2_margin_left = request.POST.get("button2_margin_left","")
        button2_app_name = request.POST.get("button2_app_name","")
        button2_package_name = request.POST.get("button2_package_name","")


        button3_type = request.POST.get("button3_type","")
        button3_redirect_url = request.POST.get("button3_redirect_url","")
        button3_icon_url = request.POST.get("button3_icon_url","")
        button3_download_url = request.POST.get("button3_download_url","")
        button3_height = request.POST.get("button3_height","")
        button3_width = request.POST.get("button3_width","")
        button3_margin_top = request.POST.get("button3_margin_top","")
        button3_margin_left = request.POST.get("button3_margin_left","")
        button3_app_name = request.POST.get("button3_app_name","")
        button3_package_name = request.POST.get("button3_package_name","")

        pop = PopWindow()

        pop.big_image_url = big_image_url
        pop.big_image_download_url = big_image_download_url
        pop.big_image_height = big_image_height
        pop.big_image_weight = big_image_width
        pop.small_image_url = small_image_url
        pop.exit_button_icon_url = exit_button_icon_url
        pop.exit_button_height = exit_button_height
        pop.exit_button_width = exit_button_width
        pop.exit_button_margin_top = exit_button_margin_top
        pop.exit_button_margin_right = exit_button_margin_right

        pop.button1_type = button1_type
        pop.button1_redirect_url = button1_redirect_url
        pop.button1_icon_url = button1_icon_url
        pop.button1_download_url = button1_download_url
        pop.button1_height = button1_height
        pop.button1_width = button1_width
        pop.button1_margin_top = button1_margin_top
        pop.button1_margin_left = button1_margin_left
        pop.button1_app_name = button1_app_name
        pop.button1_package_name = button1_package_name

        pop.button2_type = button2_type
        pop.button2_redirect_url = button2_redirect_url
        pop.button2_icon_url = button2_icon_url
        pop.button2_download_url = button2_download_url
        pop.button2_height = button2_height
        pop.button2_width = button2_width
        pop.button2_margin_top = button2_margin_top
        pop.button2_margin_left = button2_margin_left
        pop.button2_app_name = button2_app_name
        pop.button2_package_name = button2_package_name

        pop.button3_type = button3_type
        pop.button3_redirect_url = button3_redirect_url
        pop.button3_icon_url = button3_icon_url
        pop.button3_download_url = button3_download_url
        pop.button3_height = button3_height
        pop.button3_width = button3_width
        pop.button3_margin_top = button3_margin_top
        pop.button3_margin_left = button3_margin_left
        pop.button3_app_name = button3_app_name
        pop.button3_package_name = button3_package_name

        try:
            pop.save()
            return HttpResponse("success")
        except Exception, e:
            print e
            return HttpResponse("fail")


@login_required
@csrf_exempt
def recommend_game_order_edit(request):
    if request.method == "POST":
        app_center_game_id = request.POST.get("app_center_game_id","")
        rec_id = request.POST.get("rec_id","")
        order = request.POST.get("order","")
        if AppCenterList.change_order(app_center_game_id,rec_id,order):
            return HttpResponseRedirect("recommend_game?rec_id=%s"%str(rec_id))
        else:
            return getResult(False, "fail, change order fail")


@login_required
def rec_install_app_delete(request):
    if request.method == "POST":
        rec_id = request.POST.get("rec_id","")
        if RecommendInstallAPP.objects.filter(id=rec_id).exists():
            a = RecommendInstallAPP.objects.get(id=rec_id)
            try:
                a.delete()
                return HttpResponse("success")
            except Exception, e:
                print e
                return HttpResponse("fail")
        else:
            return HttpResponse("rec do not exisit")

@login_required
def rec_install_app_add(request):
    if request.method == "POST":
        game_library_id = request.POST.get("game_library_id","")
        order = request.POST.get("order",'')
        if not order:
            order = 0
        if not game_library_id:
            return HttpResponse("error game_library_id should not empty")
        if not GameLibrary.objects.filter(pk=game_library_id).exists():
            return HttpResponse("error game_library_id do not exist ")
        else:
            a = RecommendInstallAPP()
            a.game_library_id = game_library_id
            a.order = order
            a.save()
            return HttpResponseRedirect("manage_rec_install_app")


@login_required
def manage_rec_install_app(request):
    if request.method == "GET":
        result = []
        apps = RecommendInstallAPP.objects.all().order_by("order")
        for app in apps:
            result.append(app.toJSON())
        return render(request, 'manage_webhtml/rec_install_app.html', {"apps":result})

@login_required
def game9_data_copy(request):
    if request.method == "GET":
        return render(request,"manage_webhtml/game9_data_copy.html")
    elif request.method == "POST":
        game9_id = request.POST.get("game9_id","")
        game_id = request.POST.get("game_id","")
        if not game9_id or not game_id:
            return getResult(False,"can not empty")
        if not GameLibrary.objects.filter(pk=game9_id).exists():
            return getResult(False,"%s not exist"%game9_id)
        if not GameLibrary.objects.filter(pk=game_id).exists():
            return getResult(False,"%s not exist"%game_id)
        game9_data = GameLibrary.objects.get(pk=game9_id)
        if int(game9_data.platformId) != int(Platform.objects.get(platform_name="game9").pk):
            return getResult(False,"%s not a game9 game"%game9_id)
        else:
            game = GameLibrary.objects.get(pk=game_id)
            game.icon_url = game9_data.icon_url
            game.type = game9_data.type
            game.screenshots = game9_data.screenshots
            game.screenshots_type = game9_data.screenshots_type
            game.instruction = game9_data.instruction
            game.description = game9_data.description
            game.size = game9_data.size
            try:
                game.save()
                # return getResult(True,"success")
            except Exception,e:
                print e
                # return getResult(False,"error, please contact andy")

            game = GameLibraryModify.objects.get(pk=game_id)
            game.icon_url = game9_data.icon_url
            game.type = game9_data.type
            game.screenshots = game9_data.screenshots
            game.screenshots_type = game9_data.screenshots_type
            game.instruction = game9_data.instruction
            game.description = game9_data.description
            game.size = game9_data.size
            try:
                game.save()
                return getResult(True,"success")
            except Exception,e:
                print e
                return getResult(False,"error, please contact andy")

@login_required
@csrf_exempt
def rec_delete(request):
    if request.method == "POST":
        rec_id = request.POST.get("rec_id","")
        if rec_id:
            if RecommendList.delete_rec_by_id(rec_id):
                return HttpResponse("success")
            else:
                return HttpResponse("fail, rec_id %s not exsist "%str(rec_id))
        else:
            return HttpResponse("fail, rec_id not exsist")

@login_required
@csrf_exempt
def rec_edit(request):
    if request.method == "GET":
        rec_id = request.GET.get("rec_id","")
        recommend = RecommendList.get_rec_data(rec_id).toJSON()
        return render(request, 'manage_webhtml/recommend_edit.html', {"recommend":recommend})

    elif request.method == "POST":
        rec_id = request.POST.get("rec_id","")
        title = request.POST.get("title","")
        flag = request.POST.get("flag","")

        if not RecommendList.objects.filter(pk=rec_id).exists():
            return HttpResponse("error recommend id not exist")
        else:
            recommend = RecommendList.objects.get(pk=rec_id)
            recommend.title = title
            recommend.flag = flag
            recommend.save()
            # return HttpResponseRedirect("topic_edit?topic_id=%s"%str(topic_id))
            return HttpResponseRedirect("recommend")


def tmp_copy_modify(request):
    gms = GameLibrary.objects.all()
    for gm in gms:
        if GameLibraryModify.objects.filter(id=gm.pk).exists():
            modify = GameLibraryModify.objects.get(id=gm.pk)
            if modify.game_name:
                gm.game_name = modify.game_name
            if modify.type:
                gm.type = modify.type
            if modify.icon_url:
                gm.icon_url = modify.icon_url
            if modify.size:
                gm.size = modify.size
            if modify.download_count:
                gm.download_count = modify.download_count
            if modify.screenshots:
                gm.screenshots = modify.screenshots
            if modify.version:
                gm.version = modify.version
            if modify.tags:
                gm.tags = modify.tags
            if modify.level:
                gm.level = modify.level
            if modify.instruction:
                gm.instruction = modify.instruction
            if modify.description:
                gm.description = modify.description
            if modify.screenshots_type:
                gm.screenshots_type = modify.screenshots_type
            if modify.download_url:
                gm.download_url = modify.download_url
            gm.save()
    return HttpResponse("success")


@login_required
def copy_data_2_db(request):
    """
    preview_cn_bst_server -> cn_bst_server
    """
    def sync_appcenterlist(origin_db, target_db):
        """
        bluestacks_appcenterlist
        """
        AppCenterList.objects.using(target_db).all().delete()
        data = AppCenterList.objects.using(origin_db).all()
        for i in data:
            if AppCenterList.objects.using(target_db).filter(pk=i.pk).exists():
                a = AppCenterList.objects.using(target_db).get(pk=i.pk)
            else:
                a = AppCenterList()
                a.id = i.pk
            a.app_list_id = i.app_list_id
            a.app_center_id = i.app_center_id
            a.order = i.order
            a.save(using=target_db)

    def bluestacks_icontag(origin_db, target_db):
        IconTag.objects.using(target_db).all().delete()
        data = IconTag.objects.using(origin_db).all()
        for i in data:
            if IconTag.objects.using(target_db).filter(pk=i.pk).exists():
                a = IconTag.objects.using(target_db).get(pk=i.pk)
            else:
                a = IconTag()
                a.id = i.pk
            a.title = i.title
            a.comment = i.comment
            a.save(using=target_db)

    def bluestacks_recommendinstallapp(origin_db, target_db):
        RecommendInstallAPP.objects.using(target_db).all().delete()
        data = RecommendInstallAPP.objects.using(origin_db).all()
        for i in data:
            if RecommendInstallAPP.objects.using(target_db).filter(pk=i.pk).exists():
                a = RecommendInstallAPP.objects.using(target_db).get(pk=i.pk)
            else:
                a = RecommendInstallAPP()
                a.id = i.pk
            a.game_library_id = i.game_library_id
            a.order = i.order
            a.save(using=target_db)

    def bluestacks_recommendlist(origin_db, target_db):
        RecommendList.objects.using(target_db).all().delete()
        data = RecommendList.objects.using(origin_db).all()
        for i in data:
            if RecommendList.objects.using(target_db).filter(pk=i.pk).exists():
                a = RecommendList.objects.using(target_db).get(pk=i.pk)
            else:
                a = RecommendList()
                a.id = i.pk
            a.title = i.title
            a.flag = i.flag
            a.save(using=target_db)

    def bluestacks_topic(origin_db, target_db):
        Topic.objects.using(target_db).all().delete()
        data = Topic.objects.using(origin_db).all()
        for i in data:
            if Topic.objects.using(target_db).filter(pk=i.pk).exists():
                a = Topic.objects.using(target_db).get(pk=i.pk)
            else:
                a = Topic()
                a.id = i.pk
            a.big_image_url = i.big_image_url
            a.small_image_url = i.small_image_url
            a.url = i.url
            a.order = i.order
            a.status = i.status
            a.flag = i.flag
            a.topic_name = i.topic_name
            a.type = i.type
            a.game_libaray_id = i.game_libaray_id
            a.save(using=target_db)

    def bluestacks_topicgame(origin_db, target_db):
        TopicGame.objects.using(target_db).all().delete()
        data = TopicGame.objects.using(origin_db).all()
        for i in data:
            if TopicGame.objects.using(target_db).filter(pk=i.pk).exists():
                a = TopicGame.objects.using(target_db).get(pk=i.pk)
            else:
                a = TopicGame()
                a.id = i.pk
            a.topic_id = i.topic_id
            a.app_center_game_id = i.app_center_game_id
            a.order = i.order
            a.save(using=target_db)


    def bluestacks_searchdefaultquery(origin_db, target_db):
        SearchDefaultQuery.objects.using(target_db).all().delete()
        data = SearchDefaultQuery.objects.using(origin_db).all()
        for i in data:
            if SearchDefaultQuery.objects.using(target_db).filter(pk=i.pk).exists():
                a = SearchDefaultQuery.objects.using(target_db).get(pk=i.pk)
            else:
                a = SearchDefaultQuery()
                a.id = i.pk
            a.query = i.query
            a.save(using=target_db)


    def game_library(origin_db, target_db):
        GameLibrary.objects.using(target_db).all().delete()
        data = GameLibrary.objects.using(origin_db).all()
        for i in data:
            if GameLibrary.objects.using(target_db).filter(pk=i.pk).exists():
                a = GameLibrary.objects.using(target_db).get(pk=i.pk)
            else:
                a = GameLibrary()
                a.id = i.pk
            a.game_name = i.game_name
            a.game_name_lower = i.game_name_lower
            a.type = i.type
            a.platformId = i.platformId
            a.platform_game_id = i.platform_game_id
            a.icon_url = i.icon_url
            a.size = i.size
            a.download_count = i.download_count
            a.modify_date = i.modify_date
            a.screenshots = i.screenshots
            a.version = i.version
            a.tags = i.tags
            a.level = i.level
            a.instruction = i.instruction
            a.description = i.description
            a.deleted = i.deleted
            a.bs_deleted = i.bs_deleted
            a.screenshots_type = i.screenshots_type
            a.download_url = i.download_url
            a.downloadqrcode_url = i.downloadqrcode_url
            a.package_name = i.package_name
            a.pinyin = i.pinyin
            a.initails_pinyin = i.initails_pinyin
            a.save(using=target_db)


    def platform(origin_db, target_db):
        Platform.objects.using(target_db).all().delete()
        data = Platform.objects.using(origin_db).all()
        for i in data:
            if Platform.objects.using(target_db).filter(pk=i.pk).exists():
                a = Platform.objects.using(target_db).get(pk=i.pk)
            else:
                a = Platform()
                a.id = i.pk
            a.platform_name = i.platform_name
            a.save(using=target_db)

    def bluestacks_popwindow(origin_db,target_db):
        """
        bluestacks_partnerpreinstallgame
        """
        PopWindow.objects.using(target_db).all().delete()
        data = PopWindow.objects.using(origin_db).all()
        for i in data:
            if PopWindow.objects.using(target_db).filter(pk=i.pk).exists():
                a = PopWindow.objects.using(target_db).get(pk=i.pk)
            else:
                a = PopWindow()
                a.id = i.pk
            a.big_image_url = i.big_image_url
            a.big_image_download_url = i.big_image_download_url
            a.big_image_height = i.big_image_height
            a.big_image_width = i.big_image_width
            a.small_image_url = i.small_image_url
            a.exit_button_icon_url = i.exit_button_icon_url
            a.exit_button_height = i.exit_button_height
            a.exit_button_width  = i.exit_button_width
            a.exit_button_margin_top  = i.exit_button_margin_top
            a.exit_button_margin_right = i.exit_button_margin_right
            a.button1_app_name = i.button1_app_name
            a.button1_package_name = i.button1_package_name
            a.button1_type = i.button1_type
            a.button1_redirect_url = i.button1_redirect_url
            a.button1_icon_url = i.button1_icon_url
            a.button1_download_url = i.button1_download_url
            a.button1_height = i.button1_height
            a.button1_width = i.button1_width
            a.button1_margin_top = i.button1_margin_top
            a.button1_margin_left = i.button1_margin_left
            a.button2_app_name = i.button2_app_name
            a.button2_package_name = i.button2_package_name
            a.button2_type = i.button2_type
            a.button2_redirect_url = i.button2_redirect_url
            a.button2_icon_url = i.button2_icon_url
            a.button2_download_url = i.button2_download_url
            a.button2_height = i.button2_height
            a.button2_width = i.button2_width
            a.button2_margin_top = i.button2_margin_top
            a.button2_margin_left = i.button2_margin_left
            a.button3_app_name = i.button3_app_name
            a.button3_package_name = i.button3_package_name
            a.button3_type = i.button3_type
            a.button3_redirect_url = i.button3_redirect_url
            a.button3_icon_url = i.button3_icon_url
            a.button3_download_url = i.button3_download_url
            a.button3_height = i.button3_height
            a.button3_width = i.button3_width
            a.button3_margin_top = i.button3_margin_top
            a.button3_margin_left = i.button3_margin_left
            a.save(using=target_db)


    def bluestacks_partnerpreinstallgame(origin_db,target_db):
        """
        bluestacks_partnerpreinstallgame
        """
        PartnerPreInstallGame.objects.using(target_db).all().delete()
        data = PartnerPreInstallGame.objects.using(origin_db).all()
        for i in data:
            if PartnerPreInstallGame.objects.using(target_db).filter(pk=i.pk).exists():
                a = PartnerPreInstallGame.objects.using(target_db).get(pk=i.pk)
            else:
                a = PartnerPreInstallGame()
                a.id = i.pk
            a.partner = i.partner
            a.partner_name_zh = i.partner_name_zh
            a.game_name = i.game_name
            a.package_name = i.package_name
            a.download_url = i.download_url
            a.icon_url = i.icon_url
            a.save(using=target_db)

    def sync_appcentergame(origin_db,target_db):
        """
        bluestacks_appcentergame
        """
        AppCenterGame.objects.using(target_db).all().delete()
        data = AppCenterGame.objects.using(origin_db).all()
        for i in data:
            if AppCenterGame.objects.using(target_db).filter(pk=i.pk).exists():
                a = AppCenterGame.objects.using(target_db).get(pk=i.pk)
            else:
                a = AppCenterGame()
                a.id = i.pk
            a.title = i.title
            a.platform_game_ids = i.platform_game_ids
            a.type = i.type
            a.icon_url = i.icon_url
            a.size = i.size
            a.download_count = i.download_count
            a.screenshots = i.screenshots
            a.version = i.version
            a.tags = i.tags
            a.level = i.level
            a.instruction = i.instruction
            a.description = i.description
            a.status = i.status
            a.icon_tag_id = i.icon_tag_id
            a.small_icon_url = i.small_icon_url
            a.save(using=target_db)


    # start
    type = request.GET.get("type","")
    if not type:
        return HttpResponse("Error")
    elif type == "backup":
        origin_db = "production"
        target_db = "backup"
    elif type == "syncdb":
        origin_db = "default"
        target_db = "production"

    now = datetime.datetime.now()
    platform(origin_db,target_db)
    # game_library_modify(origin_db,target_db)
    game_library(origin_db,target_db)
    bluestacks_icontag(origin_db,target_db)
    bluestacks_topicgame(origin_db,target_db)
    bluestacks_topic(origin_db,target_db)
    bluestacks_recommendlist(origin_db,target_db)
    sync_appcentergame(origin_db,target_db)
    sync_appcenterlist(origin_db,target_db)
    bluestacks_recommendinstallapp(origin_db,target_db)
    bluestacks_partnerpreinstallgame(origin_db,target_db)
    bluestacks_popwindow(origin_db,target_db)
    used_time = datetime.datetime.now() - now
    return HttpResponse(used_time)


@login_required
def check_data(request):
    """
    监测配置数据是否正确
    """
    errors = []
    if request.method == "GET":
        return render(request, "manage_webhtml/check_data.html", {"datas":errors})
    elif request.method == "POST":
        # errors = []
        a = AppCenterGame.objects.filter(status=True)
        for i in a:
            # print i.title
            if not i.icon_url:
                errors.append({"item":"游戏库", "id":i.pk, "message":"游戏库icon_url不存在","game_name":i.title, "level":"error"})
            if not i.platform_game_ids or i.platform_game_ids.find(",,") != -1 or i.platform_game_ids[-1] == ",":
                errors.append({"item":"游戏库", "id":i.pk, "message":"游戏库关联关系platform_game_ids字段有误","game_name":i.title,"level":"error"})
            if not type:
                errors.append({"item":"游戏库", "id":i.pk, "message":"游戏库type不存在","game_name":i.title, "level":"warning"})
        b = GameLibrary.objects.filter(bs_deleted=False,deleted=False)
        for i in b:
            if i.pk == 4266:
                print i
            game = GameLibrary.get_game(id=i.pk, use_modify=True)
            # print game.game_name
            if not game.icon_url:
                errors.append({"item":"渠道包", "id":game.pk, "message":"渠道包icon_url不存在","game_name":game.game_name, "level":"error"})
            if not game.size:
                errors.append({"item":"渠道包", "id":game.pk, "message":"渠道包size不存在","game_name":game.game_name, "level":"warning"})
            if not game.screenshots:
                errors.append({"item":"渠道包", "id":game.pk, "message":"渠道包screenshots不存在","game_name":game.game_name, "level":"warning"})
            if not game.instruction:
                errors.append({"item":"渠道包", "id":game.pk, "message":"渠道包instruction不存在","game_name":game.game_name, "level":"warning"})
            if not game.download_url:
                errors.append({"item":"渠道包", "id":game.pk, "message":"渠道包download_url不存在","game_name":game.game_name, "level":"warning"})
            if not game.package_name or len(game.package_name.split("-")) == 4:
                errors.append(
                    {"item":"渠道包", "id":game.pk, "message":"渠道包package_name错误","game_name":game.game_name, "level":"warning"}
                )

        # data = [{"a":"b"}]
        # res_number = 30
        # page = int(request.GET.get('page',"1"))
        # paginator = Paginator(errors, res_number)
        # try:
        #     errors = paginator.page(page)
        # except PageNotAnInteger:
        #     # If page is not an integer, deliver first page.
        #     errors = paginator.page(1)
        # except EmptyPage:
        #     # If page is out of range (e.g. 9999), deliver last page of results.
        #     errors = paginator.page(paginator.num_pages)
    return render(request, "manage_webhtml/check_data.html", {"datas":errors})
    # return render(request, "manage_webhtml/check_data.html", {"datas":errors, 'page_range':paginator.page_range})


@login_required
def search_key_word(request):
    if request.method == "GET":
        start = request.GET.get("start","")
        end = request.GET.get("end","")
        today = datetime.date.today()+datetime.timedelta(days=1)

        # print start,end
        if start:
            start = datetime.datetime.strptime(start,'%Y-%m-%d')
        if end:
            end = datetime.datetime.strptime(end,'%Y-%m-%d')
        if not start or not end:
            start = (datetime.timedelta(days=-1) + today)
            end = today
        _start = start.strftime('%Y-%m-%d')
        _end = end.strftime('%Y-%m-%d')

        search = SearchKeyWord.objects.using("production").filter(uptime__gte=start,uptime__lt=end).values('word').annotate(count=Count(1)).order_by("-count")
        # for i in data:
            # print i
        res_number = 30
        # page = int(request.GET.get('page',"1"))
        # paginator = Paginator(search, res_number)
        # try:
        #     search = paginator.page(page)
        # except PageNotAnInteger:
        #     # If page is not an integer, deliver first page.
        #     search = paginator.page(1)
        # except EmptyPage:
        #     # If page is out of range (e.g. 9999), deliver last page of results.
        #     search = paginator.page(paginator.num_pages)

        # return render(request, "manage_webhtml/search_key_word.html", {"start":_start,"end":_end,"datas":search, 'page_range':paginator.page_range})
        return render(request, "manage_webhtml/search_key_word.html", {"start":_start,"end":_end,"datas":search})









