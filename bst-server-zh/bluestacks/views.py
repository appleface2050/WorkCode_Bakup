#coding=utf-8
import hashlib
import logging
import socket
import httplib
import base64
import md5
import urllib2 
import time
import datetime
import json
import hmac
from hashlib import sha1 as sha


# from DjangoCaptcha import Captcha,Code

# from captcha.helpers import captcha_image_url
# from captcha.models import CaptchaStore
from captcha.fields import CaptchaField
from captcha.helpers import captcha_image_url
from captcha.models import CaptchaStore
from django.db import transaction
from django.shortcuts import render, render_to_response
from django import forms

# from captcha.fields import CaptchaField

# Create your views here.

from django.template import loader
from django.http import HttpResponse, HttpResponseRedirect, JsonResponse
from django.views.generic import TemplateView
from django.shortcuts import render
from django.views.decorators.csrf import csrf_protect,csrf_exempt
from django.contrib.auth import login as auth_login, logout as auth_logout, authenticate
from django.contrib.auth import get_user_model

from bluestacks.forms import UploadFileForm

from bluestacks.models import UserFeedBack, ChargeReturn, RecommendInstallAPP, SearchDefaultQuery, PopWindow, GameLibrary, PartnerPreInstallGame

from bst_server.settings import PUBLIC_OSS_BEIJING_HOST, BUCKET_APPCENTER, USE_MC_ENVIRONMENT, ENVIRONMENT
from util.decorators import json_response, guid_check_get, guid_check_post, check_signature
from util.jsonresult import getResult, getJsonpResult, getSimpleResult
from oss_models import OssPackage

from util.redis_ import r as redis_client


import datetime
import json

from util.memcached import mc_client
from util.network import get_local_ip


def index(request):
    # return HttpResponse("Hello, world")
    # context = {"id":11}
    # return render(request, 'webhtml/index.html', context)
    return HttpResponseRedirect("app_center_html")



def js_csrf(request):
    if request.method == 'POST':
        drugs_id = request.POST.get('drugs')
        drugs_id_list = drugs_id.strip().split(' ')
        return HttpResponse(','.join(drugs_id_list))
    else:
        # drugs = BioDrug.objects.filter(inputer__isnull=True)
        drugs = [{"id":1,"name":"one"}, {"id":2,"name":"two"}]
        return render(request, 'webhtml/jscsrf.html', {'drugs': drugs})


def upload_token(request):
    accessKeyId = 't8z6fuZ2B1FldqmL'
    accessKeySecret = 'ES2YRYWBREeBNeGCBczpc63FCLJvc9'
    # host = 'http://bs-for-test.oss-cn-beijing.aliyuncs.com'
    expire_time = 30
    # upload_dir = 'for-test/'
    host = ""
    upload_dir = ""

    def get_iso_8601(expire):
        print expire
        gmt = datetime.datetime.fromtimestamp(expire).isoformat()
        gmt += 'Z'
        return gmt

    def get_token(bucket,upload_dir):
        upload_dir = upload_dir + "/"
        host = "http://" + bucket + "." + PUBLIC_OSS_BEIJING_HOST

        now = int(time.time())
        expire_syncpoint  = now + expire_time
        expire = get_iso_8601(expire_syncpoint)

        policy_dict = {}
        policy_dict['expiration'] = expire
        condition_array = []
        array_item = []
        array_item.append('starts-with');
        array_item.append('$key');
        array_item.append(upload_dir);
        condition_array.append(array_item)
        policy_dict['conditions'] = condition_array
        policy = json.dumps(policy_dict).strip()
        policy_encode = base64.b64encode(policy)
        print policy_encode
        h = hmac.new(accessKeySecret, policy_encode, sha)
        sign_result = base64.encodestring(h.digest()).strip()

        # callback_dict = {}
        # callback_dict['callbackUrl'] = callback_url
        # callback_dict['callbackBody'] = 'filename=${object}&size=${size}&mimeType=${mimeType}&height=${imageInfo.height}&width=${imageInfo.width}'
        # # callback_dict['callbackBodyType'] = 'application/x-www-form-urlencoded';
        # callback_dict['callbackBodyType'] = 'application/json';
        # print callback_dict['callbackBody']

        # callback_param = json.dumps(callback_dict).strip()
        # base64_callback_body = base64.b64encode(callback_param);

        token_dict = {}
        token_dict['accessid'] = accessKeyId
        token_dict['host'] = host
        token_dict['policy'] = policy_encode
        token_dict['signature'] = sign_result
        token_dict['expire'] = expire_syncpoint
        token_dict['dir'] = upload_dir
        # token_dict['callback'] = base64_callback_body

        # web.header("Access-Control-Allow-Methods","POST")
        # web.header("Access-Control-Allow-Origin","*")
        #web.header('Content-Type', 'text/html; charset=UTF-8')
        result = json.dumps(token_dict)
        return result

    if request.method == 'GET':
        bucket = request.GET.get("bucket","")
        upload_dir = request.GET.get("upload_dir","")
        if not bucket or not upload_dir:
            return HttpResponse("Fail")
        token = get_token(bucket,upload_dir)
        response = HttpResponse(token)
        response['Access-Control-Allow-Methods'] = "POST"
        response['Access-Control-Allow-Origin'] = "*"
        response['Content-Type'] = "text/html; charset=UTF-8"
        return response



@csrf_exempt
def upload_token_with_callback(request):
    accessKeyId = 't8z6fuZ2B1FldqmL'
    accessKeySecret = 'ES2YRYWBREeBNeGCBczpc63FCLJvc9'
    host = 'http://bs-for-test.oss-cn-beijing.aliyuncs.com'
    expire_time = 30
    upload_dir = 'for-test/'
    # callback_url = "101.201.42.93:23450"
    # callback_url = "http://127.0.0.1/bs/upload_callback"
    # callback_url = "http://%s/bs/upload_callback" % get_local_ip()
    callback_url = "http://101.201.42.93/bs/upload_callback"
    # callback_url = "http://101.201.42.93:10001/bs/upload_callback"
    # callback_url = "http://oss-demo.aliyuncs.com:23450"
    # callback_url = "http://101.201.42.93:5000/flask_callback"

    def get_iso_8601(expire):
        print expire
        gmt = datetime.datetime.fromtimestamp(expire).isoformat()
        gmt += 'Z'
        return gmt

    def get_token():
        now = int(time.time())
        expire_syncpoint  = now + expire_time
        expire = get_iso_8601(expire_syncpoint)

        policy_dict = {}
        policy_dict['expiration'] = expire
        condition_array = []
        array_item = []
        array_item.append('starts-with');
        array_item.append('$key');
        array_item.append(upload_dir);
        condition_array.append(array_item)
        policy_dict['conditions'] = condition_array
        policy = json.dumps(policy_dict).strip()
        policy_encode = base64.b64encode(policy)
        print policy_encode
        h = hmac.new(accessKeySecret, policy_encode, sha)
        sign_result = base64.encodestring(h.digest()).strip()

        callback_dict = {}
        callback_dict['callbackUrl'] = callback_url
        callback_dict['callbackBody'] = 'filename=${object}&size=${size}&mimeType=${mimeType}&height=${imageInfo.height}&width=${imageInfo.width}'
        # callback_dict['callbackBodyType'] = 'application/x-www-form-urlencoded';
        callback_dict['callbackBodyType'] = 'application/json';
        print callback_dict['callbackBody']

        callback_param = json.dumps(callback_dict).strip()
        base64_callback_body = base64.b64encode(callback_param);

        token_dict = {}
        token_dict['accessid'] = accessKeyId
        token_dict['host'] = host
        token_dict['policy'] = policy_encode
        token_dict['signature'] = sign_result
        token_dict['expire'] = expire_syncpoint
        token_dict['dir'] = upload_dir
        token_dict['callback'] = base64_callback_body

        # web.header("Access-Control-Allow-Methods","POST")
        # web.header("Access-Control-Allow-Origin","*")
        #web.header('Content-Type', 'text/html; charset=UTF-8')
        result = json.dumps(token_dict)
        return result

    if request.method == 'GET':
        token = get_token()
        print token
        response = HttpResponse(token)
        response['Access-Control-Allow-Methods'] = "POST"
        response['Access-Control-Allow-Origin'] = "*"
        response['Content-Type'] = "text/html; charset=UTF-8"
        return response

@csrf_exempt
def upload_callback(request):
    try:
        from M2Crypto import RSA
        from M2Crypto import BIO
    except Exception as e:
        log = logging.getLogger('django')
        log.error(str(e))
        return HttpResponse("fail")

    if request.method == "GET":
        log = logging.getLogger('django')
        log.error(str(get_local_ip()))
        return HttpResponse(get_local_ip())

    if request.method == "POST":
    #get public key
        try:
            pub_key_url_base64 = request.META['HTTP_X_OSS_PUB_KEY_URL']
            #print "pub_key_url_base64",pub_key_url_base64
            pub_key_url = pub_key_url_base64.decode('base64')
            url_reader = urllib2.urlopen(pub_key_url)
            pub_key = url_reader.read()
        except:
            print 'Get pub key failed!'
            return HttpResponse("fail")

        #get authorization
        authorization_base64 = request.META.get('HTTP_AUTHORIZATION',"")

        # authorization_base64 = ",".join(request.META.keys())
        # return HttpResponse('{"Status":"%s"}' % authorization_base64)
        authorization = authorization_base64.decode('base64')

        #get callback body
        content_length = request.META['CONTENT_LENGTH']
        callback_body = request.body
        #callback_body = request.read(int(content_length))
        print callback_body
        # return HttpResponse('{"callback_body":"%s"}' % callback_body)

        #compose authorization string
        auth_str = ''
        pos = request.path.find('?')
        if -1 == pos:
            auth_str = request.path + '\n' + callback_body
        else:
            auth_str = urllib2.unquote(request.path[0:pos]) + request.path[pos:] + '\n' + callback_body
        print "auth_str:",auth_str

        #verify authorization
        auth_md5 = md5.new(auth_str).digest()
        bio = BIO.MemoryBuffer(pub_key)
        rsa_pub = RSA.load_pub_key_bio(bio)
        try:
            result = rsa_pub.verify(auth_md5, authorization, 'md5')
        except Exception as e:
            result = False

        if not result:
            print 'Authorization verify failed!'
            print 'Public key : %s' % (pub_key)
            print 'Auth string : %s' % (auth_str)
            # self.send_response(400)
            # self.end_headers()
            return

        #do something accoding to callback_body

        #response to OSS

        resp_body = '{"Status":"www"}'
        response = HttpResponse(resp_body)
        # self.send_response(200)
        response['Content-Type'] = 'application/json'
        response['Content-Length'] = str(len(resp_body))
        # self.end_headers()
        # self.wfile.write(resp_body)
        return HttpResponse(resp_body)


# @csrf_exempt
# def upload_callback(request):
#     return HttpResponse('{"Status":"OK"}')

def bs_message(request):
    if request.method == "GET":
        guid = request.GET.get("guid","")
        return getResult(True, "bs message success", {"url":"http://www.bluestacks.cn"})

def feedback_html(request):
    """
    feedback 页面
    """
    return render(request, 'webhtml/feedback.html')



#@csrf_protect


@check_signature
@csrf_exempt
def feedback(request):
    """
    用户反馈系统
    """
    if request.method == 'POST':
        email = request.POST.get("email","")
        content = request.POST.get("content","")
        type = request.POST.get("type","")

        if not email or not content:
            return getResult(False, u"feedback fail")
        if UserFeedBack.objects.filter(email=email,content=content).exists():
            return getResult(False, u"do not post duplicate content")
        if UserFeedBack.post_in_one_min(email):
            return getResult(False, u"post in 1 min")
        else:
            fb = UserFeedBack()
            fb.email = str(email.strip())
            fb.content = str(content.strip())
            fb.type = str(type.strip())
            fb.save()
            return getResult(True, u"feedback success")
    # if request.method == "GET":
    #     content = request.GET.get("content","")
    #     a = request.GET.get("a","")
    #     return getResult(False, content)


    else:
        return getResult(False, u"use POST method")


@transaction.atomic()
def reg_user(request):
    if request.user.is_anonymous() or (hasattr(request.user, 'tel') and request.user.tel):
        user = get_user_model()
    elif not request.user.tel:
        user = request.user

    user.objects.all()
    return getResult(True, u"reg_user success")





class CaptchaTestForm(forms.Form):
    # email = AnyOtherField()
    captcha = CaptchaField()


def some_view(request):
    if request.POST:
        form = CaptchaTestForm(request.POST)
        email = request.POST.get("email","")

        # Validate the form: the captcha field will automatically
        # check the input
        if form.is_valid():
            human = True
            return HttpResponse('Thank you you')
        else:
            human = False
            return HttpResponse('error')
    else:
        form = CaptchaTestForm()

    # return render_to_response('webhtml_browser/template.html',locals())
    return render(request, 'webhtml/template.html' , {"form":form})

# def captcha(request):
#
#     # return render_to_response('webhtml_browser/template.html')
#     return render(request, 'webhtml_browser/template.html')


# 改为使用自带的refresh功能
def refresh_captcha(request):
    to_json_response = dict()
    to_json_response['status'] = 1
    to_json_response['new_cptch_key'] = CaptchaStore.generate_key()
    to_json_response['new_cptch_image'] = captcha_image_url(to_json_response['new_cptch_key'])
    return HttpResponse(json.dumps(to_json_response), content_type='application/json')


def ajax_val(request):
    if request.is_ajax():
        cs = CaptchaStore.objects.filter(response=request.GET['response'],
                                     hashkey=request.GET['hashkey'])
        if cs:
            json_data={'status':1}
        else:
            json_data = {'status':0}
        return JsonResponse(json_data)
    else:
        # raise Http404
        json_data = {'status':0}
        return JsonResponse(json_data) #需要导入  from django.http import JsonResponse


# def upload_file(request):
#     if request.method == 'POST':
#         form = UploadFileForm(request.POST, request.FILES)
#         if form.is_valid():
#             handle_uploaded_file(request.FILES['file'])
#             return HttpResponseRedirect('/bs/')
#     else:
#         form = UploadFileForm()
#     return render(request, 'test_html/upload.html', {'form': form})


def oss_upload_html(request):
    return render(request,"webhtml/oss_upload.html")



@csrf_exempt
def post_upload_file_name(request):
    if request.method == "POST":
        body = request.body
        if body:
            file_name = body.split("=")[1]
            # print file_name
            # package/BluestacksCnSetup_1.0.0.9.exe
            # package/BlueStacksKK_DeployTool_2.2.23.5969_china_gmgr.zip
            tmp = file_name.strip().split("/")
            file = OssPackage()
            if tmp[1].startswith("BlueStacksKK_DeployTool"):
                file.type = tmp[1].split("_")[0] + "_" + tmp[1].split("_")[1]
                file.file_type = tmp[1][-3:]
                file.upload_dir = tmp[0]
                file.version = tmp[1].split("_")[2]
                file.file_url = "http://" + BUCKET_APPCENTER + "." + PUBLIC_OSS_BEIJING_HOST + "/" + file_name
                file.key = file_name
            elif tmp[1].startswith("BluestacksCnSetup"):
                file.type = tmp[1].split("_")[0]
                file.file_type = tmp[1].split("_")[1][-3:]
                file.upload_dir = tmp[0]
                file.version = tmp[1].split("_")[1][:-4]
                file.file_url = "http://" + BUCKET_APPCENTER + "." + PUBLIC_OSS_BEIJING_HOST + "/" + file_name
                file.key = file_name

            else:
                return HttpResponse("fail")
            try:
                file.save()
                return HttpResponse("success")
            except Exception as e:
                print e
                return HttpResponse("fail")


def upload_file(request):
    def handle_uploaded_file(f, filename):
        with open(filename, 'wb+') as destination:
            for chunk in f.chunks():
                destination.write(chunk)

    if request.method == 'POST':
        handle_uploaded_file(request.FILES['file'], str(request.FILES['file']))
        return HttpResponse('success！')
        # form = UploadFileForm(request.POST, request.FILES)
        # if form.is_valid():
        #     # file is saved
        #     instance = UploadFileForm(file_field=request.FILES['file'])
        #     instance.save()
        #     return HttpResponseRedirect('/bs/')
    else:
        form = UploadFileForm()
    return render(request, 'test_html/upload.html', {'form': form})


@csrf_exempt
def panda_auto_signin(request):
    response = HttpResponse()
    response.write(
        '''
<HTML>
<HEAD>
    <meta charset="UTF-8">
    <meta http-equiv="pragma" content="no-cache" />
    <TITLE>BCTV</TITLE>
</HEAD>
<BODY>
</BODY>
</HTML>
        '''
    )
    response['X-Frame-Options'] = "*"
    return response


@csrf_exempt
def panda_verification(request):
    response = HttpResponse()
    response.write(
        '''
<HTML>
<HEAD>
    <meta charset="UTF-8">
    <meta http-equiv="pragma" content="no-cache" />
    <TITLE>BCTV</TITLE>
</HEAD>
<BODY>
</BODY>
</HTML>
        '''
    )
    response['X-Frame-Options'] = "*"
    return response
    # return render(request, 'webhtml/panda_verification.html')


@check_signature
def pop_window(request):
    pop = {}
    if request.method == "GET":
        if request.GET.get("disable_mc","") != 'true' and ENVIRONMENT in USE_MC_ENVIRONMENT:
            pop = mc_client.get("pop_window")
        if not pop:
            if PopWindow.objects.all().exists():
                pop = PopWindow.objects.all().order_by("-uptime")[0]
                pop = pop.toJSON()
            else:
                pop = {}
            mc_client.set("pop_window", pop, time=3600*24)
        return getResult(True, "pop windows success", {"pop":pop})


def topic_pokemongo(request):
    return render(request, "webhtml/topic_pokemongo.html")


@check_signature
def get_rec_install_app(request):
    if request.method == "GET":
        apps = RecommendInstallAPP.get_rec_app()
        return getResult(True, u"get rec install app success", {"apps": apps})

def get_rec_install_app_html(request):
    if request.method == "GET":
        return render(request, "webhtml/preinstall.html")

@check_signature
def get_default_query(request):
    if request.method != "GET":
        return HttpResponse("use GET method")
    else:
        query = SearchDefaultQuery.get_default_query()
        return getResult(True, "get default query success", {"query":query})


@check_signature
def query_suggest(request):
    if request.method != "GET":
        return HttpResponse("use GET method")
    else:
        query = request.GET.get("query","")

        data = GameLibrary.get_query_suggest_app(query=query)
        # cnt = redis_client.hget("package_user_init",package_name)
        for app in data:
            try:
                cnt = redis_client.hget("package_user_init",app["package_name"])
                if cnt:
                    app["cnt"] = int(cnt)
                else:
                    app["cnt"] = 0
            except Exception,e:
                app["cnt"] = 0
        data.sort(lambda y,x:cmp(x["cnt"],y["cnt"]))
        return getResult(True, "get query suggest success", {"data":data[:4]})


@check_signature
def partner_preinstall_game_info(request):
    if request.method == "GET":
        partner = request.GET.get("partner","")
        package_name = request.GET.get("package_name","")
        if not partner or not package_name:
            return getResult(False,"partner or pakage_name empty")
        else:
            app = PartnerPreInstallGame.get_info_by_partner_package_name(partner,package_name)
            return getResult(True, "get preinstall game info success", {"info":app})

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
        if not game_name:
            return HttpResponse("game_name empty")
        if not package_name:
            return HttpResponse("package_name empty")
        if not download_url:
            return HttpResponse("download_url empty")
        if not icon_url:
            return HttpResponse("icon_url empty")
        if not PartnerPreInstallGame.objects.filter(pk=id).exists():
            return HttpResponse("not exist")
        if PartnerPreInstallGame.objects.filter(partner=partner,package_name=package_name).exists():
            return HttpResponse("partner package_name exist")
        app = PartnerPreInstallGame.objects.get(pk=id)
        app.partner = partner
        app.partner_name_zh = partner_name_zh
        app.game_name = game_name
        app.package_name = package_name
        app.download_url = download_url
        app.icon_url = icon_url
        try:
            app.save()
            return HttpResponse("success")
        except Exception,e:
            print e
            return HttpResponse("fail")


@csrf_exempt
@check_signature
def op_charge_return(request):
    if request.method == "POST":
        game_id = request.POST.get("game_id","")
        order_id = request.POST.get("order_id","")
        amount = request.POST.get("amount","")
        contact = request.POST.get("contact","")
        game_name = request.POST.get("game_name","")
        if not game_id or not order_id or not amount or not contact or not game_name:
            return HttpResponse("parameter can not empty")
        else:
            if ChargeReturn.objects.filter(order_id=order_id).exists():
                return HttpResponse("order_id already exist")
            res = ChargeReturn.add_data(game_id,order_id,amount,contact,game_name)
            if res:
                return HttpResponse("success")
            else:
                return HttpResponse("fail")
    # elif request.method == "GET":
    #     return render(request, "webhtml/")


# def captcha(request):
#     if request.method == "GET":
#         width = request.GET.get("width",200)
#         height = request.GET.get("height",50)
#         if not width or not height:
#             return getResult(False, "get captcha fail")
#         else:
#             ca = Captcha(request)
#             ca.words = ['hello','world','helloworld']
#             ca.img_width = width
#             ca.img_height = height
#             ca.type = 'number'
#             return ca.display()
#
#
# def verify_captcha(request):
#     _code = request.GET.get('code') or ''
#     if not _code:
#         return render('index.html',locals())
#
#     code = Code(request)
#     if code.check(_code):
#         return HttpResponse('验证成功')
#     else:
#         return HttpResponse('验证失败')





