# coding: utf-8
from django.test import TestCase
from django.core.files import File
from common.addresses import Addresses
from models import Address
from models import ForumArticle
from common.geohash_code import GeohashCode
from views import *
from rest_framework.test import APIClient
from rest_framework.test import APIRequestFactory


class TestGeohashCode(TestCase):
    def setUp(self):
        self.latitude = 35
        self.longitude = 113.56
        self.length = 6
        self.gc = GeohashCode(self.latitude, self.longitude, self.length)

    def test_get(self):
        expected = "ww0z05"
        actual = self.gc.get()
        self.assertEqual(actual, expected)

    def test_get_bigger_block(self):
        expected = "ww0z0"
        actual = self.gc.get_bigger_block()
        self.assertEqual(actual, expected)

    def tearDown(self):
        pass


class TestAddresses(TestCase):
    def setUp(self):
        self.address = Addresses(112.0, 33.5, u"北京丰台区南方庄南路方安苑", u"安富大厦")
        self.object = self.address.add()

    def test_add(self):
        expected = u"安富大厦"
        actual = self.object.name
        self.assertEqual(actual, expected)

    def test_reset(self):
        Addresses.reset(self.object)
        result = self.address.get()

        expected = 1
        actual = result[0].audit
        self.assertEqual(actual, expected)

    def test_get(self):
        Addresses.reset(self.object)
        address = self.address.get()

        expected = u"安富大厦"
        actual = address[0].name
        self.assertEqual(actual, expected)

    def test_update(self):
        Addresses.reset(self.object)
        address = self.address.get()
        self.address.name = u"三个爸爸"
        self.address.update(address[0])
        result = self.address.get()

        expected = u"三个爸爸"
        actual = result[0].name
        self.assertEqual(actual, expected)

    def test_delete(self):
        Addresses.reset(self.object)
        address = self.address.get()
        Addresses.delete(address[0])
        result = self.address.get()

        expected = 0
        actual = result.count()
        self.assertEqual(actual, expected)

    def get_get_none(self):
        actual = list(Addresses.get_none())
        expected = list(Address.objects.none())
        self.assertEqual(actual, expected)

    def tearDown(self):
        pass


class TestUser(TestCase):
    def setUp(self):
        self.client = APIClient()

        self.phone = "13161916433"
        self.nickname = "moon"
        User.objects.create_user(
            username=self.phone,
            email="leliang.li@163.com",
            password='111111')
        self.user_id = User.objects.get(username=self.phone).id
        ue = UserExtension.objects.get(user_id=self.user_id)
        ue.nickname = self.nickname
        ue.save()
        Token.objects.create(user_id=self.user_id)
        self.key = Token.objects.get(user__username=self.phone).key

        self.session_store = SessionStore()
        self.code = "1358"
        self.check_phone = "13161916434"
        self.session_store[self.check_phone] = self.code
        self.session_store.save()
        self.session_key = self.session_store.session_key

    def test_register(self):
        url = "/rest/api/register/"
        data = {"phone": '13161916437', "nickname": "moon", "password": "111111"}
        response_format = "json"
        response = self.client.post(url, data=data, format=response_format)

        self.assertEqual(response.status_code, status.HTTP_201_CREATED)

        expected = {"info": "", "success": True}
        actual = response.data
        self.assertEqual(actual, expected)

    def test_user_exist(self):
        url = "/rest/api/SMS/exist/"
        data = {"phone": self.phone}
        response_format = "json"
        response = self.client.post(url, data=data, format=response_format)

        self.assertEqual(response.status_code, status.HTTP_200_OK)

        expected = {"info": "phone does exist", "success": True}
        actual = response.data
        self.assertEqual(actual, expected)

    def test_check_code(self):
        url = "/rest/api/SMS/check"
        data = {"key": self.session_key, "phone": self.check_phone, "code": self.code}
        response = self.client.post(url, data, format="json")

        self.assertEqual(response.status_code, status.HTTP_200_OK)

        expected = {"info": "the verify code is correct", self.check_phone: self.code, "success": True}
        actual = response.data
        self.assertEqual(actual, expected)

    def test_reset_password(self):
        url = "/rest/api/reset_password/"
        data = {"phone": self.phone, "password": "222222", "user_id": self.user_id, "key": self.key}

        response = self.client.post(url, data, format="json")

        self.assertEqual(response.status_code, status.HTTP_200_OK)

        expected = {"info": "change password successfully", "success": True}
        actual = response.data
        self.assertEqual(actual, expected)

    def test_get_basic_user_info(self):
        url = "/rest/api/userinfo/"
        data = {"phone": self.phone, "key": self.key}

        response = self.client.post(url, data, format="json")

        self.assertEqual(response.status_code, status.HTTP_200_OK)

        expected = True
        actual = response.data.get("success", False)
        self.assertEqual(actual, expected)

    def tearDown(self):
        pass


class TestDetector(TestCase):
    def setUp(self):
        self.client = APIClient()
        User.objects.create_user(username='13161916437', password="111111")
        Shop.objects.create(name="test")
        self.shop_id = Shop.objects.get(name="test").id

        self.user_id = User.objects.get(username="13161916437").id
        Token.objects.create(user_id=self.user_id)
        self.key = Token.objects.get(user__username='13161916437').key

        self.version = "1.0.0"
        self.mac_address = "ACCF233C966C"
        self.pm2_5 = 50
        self.carbon_dioxide = 900
        self.temperature = 10.5
        self.humidity = 35.8
        Detector.objects.create(
            version=self.version,
            mac_address=self.mac_address,
            pm2_5=self.pm2_5,
            carbon_dioxide=self.carbon_dioxide,
            temperature=self.temperature,
            humidity=self.humidity,
        )
        self.address = u"北京"
        self.city = "beijing"

        DetectorRelation.objects.create(
            user_id=self.user_id,
            mac_address=self.mac_address,
            address=self.address,
            city=self.city,
            shop_id=self.shop_id,
        )

    def test_get_tv_data(self):
        url = "/rest/api/TV/get"
        data = {"shop_id": self.shop_id, "user_id": self.user_id, "key": self.key}
        response = self.client.post(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data[0].get("pm2_5"), self.pm2_5)

    def test_get_tv_data_4_app(self):
        url = "/rest/api/TV/app/get"
        data = {"shop_id": self.shop_id}
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = {
            'carbon_dioxide': self.carbon_dioxide,
            'humidity': self.humidity,
            'is_bound': True,
            'pm2_5': self.pm2_5,
            'temperature': self.temperature
        }
        actual = response.data
        self.assertEqual(actual, expected)

    def tearDown(self):
        pass


class TestShop(TestCase):
    def setUp(self):
        self.city = u"北京"
        self.name = u"安富大厦"
        self.category_name = u"公司"
        ShopCategory.objects.create(name=self.category_name, parent_id=0)
        self.category_id = ShopCategory.objects.get(name=self.category_name).id
        Address.objects.create(longitude=137.89, latitude=36.82, detail_address=self.city, name=self.name)
        self.address_id = Address.objects.get(name=self.name).id
        Shop.objects.create(
            name=self.name,
            dianping_city=self.city,
            address_id=self.address_id,
            category_id=self.category_id
        )

    def test_get_id(self):
        url = "/rest/api/shop/get_id"
        data = {"city": self.city, "name": self.name, "address_id": self.address_id, "category_id": self.category_id}
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = True
        actual = response.data.get("success", False)
        self.assertEqual(actual, expected)

    def tearDown(self):
        pass


class TestMain(TestCase):
    def setUp(self):
        self.city = u"北京"
        self.address_name = u"安富大厦"
        self.category_name = u"公司"
        self.shop_ids = ""
        self.shop_name = "三个爸爸"
        self.pm2_5 = 50
        self.content = u"这是内容"
        self.weight = 100
        self.longitude = 137.89
        self.latitude = 36.82
        self.geohash_code = Addresses.get_geohash_code(self.latitude, self.longitude)
        self.username = "13161916437"
        self.password = "111111"
        User.objects.create(username=self.username, password=self.password)
        self.user_id = User.objects.get(username=self.username).id
        ShopCategory.objects.create(name=self.category_name, parent_id=0)
        self.category_id = ShopCategory.objects.get(name=self.category_name).id
        Address.objects.create(
            longitude=self.longitude,
            latitude=self.latitude,
            detail_address=self.city,
            name=self.address_name,
            geohash_code=self.geohash_code
        )
        self.address_id = Address.objects.get(name=self.address_name).id
        Shop.objects.create(
            name=self.shop_name,
            dianping_business_id=10,
            dianping_city=self.city,
            dianping_longitude=self.longitude,
            dianping_latitude=self.latitude,
            address_id=self.address_id,
            category_id=self.category_id,
            weight=self.weight,
            geohash_code=self.geohash_code
        )
        self.shop_id = Shop.objects.get(name=self.shop_name).id
        Publish.objects.create(
            PM2_5=self.pm2_5,
            content=self.content,
            shop_id=self.shop_id,
            user_id=self.user_id
        )
        self.push_shop_name = u"三个爸爸"
        PushShop.objects.create(
            name=self.push_shop_name
        )

    def test_get_publish_infos_hot(self):
        url = "/rest/api/main/hot"
        data = {
            "start_id": 0,
            "count": 40,
            "latitude": 0,
            "longitude": 0,
            "id": self.user_id,
            "city": self.city,
            "shop_ids": self.shop_ids,
            "shop_name": self.shop_name
        }
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = self.pm2_5
        actual = response.data.get("data")[0].get("PM2_5")
        self.assertEqual(actual, expected)

    def test_get_shop_infos_nearby(self):
        url = "/rest/api/main/nearby"
        data = {
            "start_id": 0,
            "count": 40,
            "latitude": self.latitude,
            "longitude": self.longitude,
            "id": self.user_id,
            "city": self.city,
            "shop_ids": self.shop_ids,
            "shop_name": self.shop_name
        }
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = self.pm2_5
        actual = response.data.get("data")[0].get("PM2_5")
        self.assertEqual(actual, expected)

    def test_get_publish_infos_discovery(self):
        url = "/rest/api/main/discovery"
        data = {
            "start_id": 0,
            "count": 40,
            "latitude": self.latitude,
            "longitude": self.longitude,
            "id": self.user_id,
            "city": self.city,
            "shop_ids": self.shop_ids,
            "shop_name": self.shop_name
        }
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = self.pm2_5
        actual = response.data.get("data")[0].get("PM2_5")
        self.assertEqual(actual, expected)

    def test_key(self):
        url = "/rest/api/main/key"
        response = self.client.get(url)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = self.push_shop_name
        actual = response.data.get("data")[0]
        self.assertEqual(actual, expected)

    def test_search(self):
        url = "/rest/api/main/search"
        data = {
            "name": self.push_shop_name,
            "city": self.city,
            "longitude": self.longitude,
            "latitude": self.latitude,
            "user_id": self.user_id,
        }
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = self.pm2_5
        actual = response.data.get("data")[0].get("PM2_5")
        self.assertEqual(actual, expected)

    def test_user(self):
        url = "/rest/api/main/user"
        data = {
            "longitude": self.longitude,
            "latitude": self.latitude,
            "id": self.user_id,
        }
        response = self.client.get(url, data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        expected = self.pm2_5
        actual = response.data.get("data")[0].get("PM2_5")
        self.assertEqual(actual, expected)

    def tearDown(self):
        pass


class TestCredit(TestCase):
    def setUp(self):
        pass

    def tearDown(self):
        pass

    def test_first_day_sign_in(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_second_day_sign_in(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_day_second_sign_in(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_seventh_day_sign_in(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_first_day_publish(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_second_day_different_place_publish(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_day_same_place_publish(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)

    def test_eleventh_day_publish(self):
        yesterday = Datetimes.get_some_day(1)
        yesterday_start = Datetimes.get_day_start(yesterday)
        yesterday_end = Datetimes.get_day_end(yesterday)

        # sign_in()

        expected = None
        actual = None
        self.assertEqual(actual, expected)


class TestDevicePhone(TestCase):
    def setUp(self):
        self.sequence = "Tv221u-04515EC0"
        self.phone_id = "00000000-3f3e-70ae-58ab-806f0033c587"
        self.os_version = "8.3"
        self.phone_number = "18513583658"
        self.phone_type = "ios"
        self.client = APIClient()

    def tearDown(self):
        pass

    def test_create(self):
        url = "/rest/api/devicephones/"
        data = {
            "sequence": self.sequence,
            "phone_id": self.phone_id,
            "os_version": self.os_version,
            "phone_number": self.phone_number,
            "phone_type": self.phone_type,
        }

        response = self.client.post(url, data)

        actual = response.data
        self.assertEqual(actual.get("phone_number"), self.phone_number)
        self.assertEqual(actual.get("phone_type"), self.phone_type)
        self.assertEqual(actual.get("sequence"), self.sequence)
        self.assertEqual(actual.get("phone_id"), self.phone_id)
        self.assertEqual(actual.get("os_version"), self.os_version)


class TestForum(TestCase):
    def setUp(self):
        self.forum_name = "test007"
        self.forum_label = ForumLabel.objects.create(name=self.forum_name)
        self.article = ForumArticle.objects.create(
            title="title",
            author="author",
            content="content",
            image=File(open("/abc.png")),
            forum_label_id=self.forum_label.id
        )

        collected_article = ForumArticle.objects.create(
            title="title_collected",
            author="author_collected",
            content="content_collected",
            image=File(open("/abc.png")),
            forum_label_id=self.forum_label.id
        )

        self.collected_article_id = collected_article.id

        user = User.objects.create(username="test")
        user.set_password("123456")
        user.save()

        self.user_id = user.id
        self.token = Token.objects.create(user_id=self.user_id)
        self.key = self.token.key
        self.article_id = self.article.id
        UserForumArticle.objects.create(
            user_id=self.user_id,
            article_id=self.collected_article_id,
            status=UserForumArticleMethod.STATUS_COLLECT
        )
        self.client = APIClient()

    def tearDown(self):
        pass

    def test_get_article(self):
        url = "/rest/api/forum/article"
        response = self.client.get(url)
        actual = response.data.get("articles")
        self.assertEqual(actual[1].get("title"), "title")
        self.assertEqual(actual[1].get("time_label"), Datetimes.date_to_string_for_forum(datetime.datetime.now()))
        self.assertEqual(actual[1].get("browse_count"), 0)
        self.assertEqual(actual[1].get("win_count"), 0)

    def test_click_win(self):
        url = "/rest/api/forum/win"
        data = {
            "user_id": self.user_id,
            "key": self.key,
            "article_id": self.article_id
        }
        response = self.client.post(url, data)
        actual = response.data
        self.assertEqual(actual.get("status"), 1)
        self.assertEqual(actual.get("success"), True)
        self.assertEqual(actual.get("info"), "OK")

    def test_click_collect(self):
        url = "/rest/api/forum/collect"
        data = {
            "user_id": self.user_id,
            "key": self.key,
            "article_id": self.article_id
        }
        response = self.client.post(url, data)
        actual = response.data
        self.assertEqual(actual.get("status"), 1)
        self.assertEqual(actual.get("success"), True)
        self.assertEqual(actual.get("info"), "OK")

    def test_get_recommended_article(self):
        url = "/rest/api/forum/recommended/article"
        data = {

        }
        response = self.client.get(url, data)
        actual = response.data.get("recommended_articles")
        # print actual
        self.assertEqual(actual[1].get("title"), "title")
        self.assertEqual(actual[1].get("time_label"), Datetimes.date_to_string_for_forum(datetime.datetime.now()))
        self.assertEqual(actual[1].get("browse_count"), 0)
        self.assertEqual(actual[1].get("win_count"), 0)

    def test_get_stored_article(self):
        url = "/rest/api/forum/stored/article"
        data = {
            "user_id": self.user_id,
        }
        response = self.client.get(url, data)
        actual = response.data.get("stored_articles")
        # print actual
        self.assertEqual(actual[0].get("title"), "title_collected")
        self.assertEqual(actual[0].get("time_label"), Datetimes.date_to_string_for_forum(datetime.datetime.now()))
        self.assertEqual(actual[0].get("browse_count"), 0)
        self.assertEqual(actual[0].get("win_count"), 0)

    def test_get_label(self):
        url = "/rest/api/forum/label"
        data = {}
        response = self.client.get(url, data)
        actual = response.data.get("labels")
        # print actual
        expected = self.forum_name
        self.assertEqual(actual[0].get("name"), expected)

    def test_click_browse(self):
        url = "/rest/api/forum/browse"
        data = {
            "user_id": self.user_id,
            "key": self.key,
            "article_id": self.article_id
        }
        response = self.client.post(url, data)
        actual = response.data
        self.assertEqual(actual.get("status"), 1)
        self.assertEqual(actual.get("success"), True)
        self.assertEqual(actual.get("info"), "OK")
