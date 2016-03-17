# coding:utf-8
from django.contrib.auth.models import User
import pytz
from rest_framework.decorators import list_route
# from rest_framework.permissions import IsAuthenticatedByToken

from .models import UserExtension, Shop, Address, Level, Device, UserShopRelations, ForumCategory, DetectorRelation
from .models import Publish, Comment, China, ShopCategory, RedEnvelopePool, Coupon, Channel, Use, Game, Company, \
    PopWindow
from .models import UserPublishRelations, ForumPost, ForumReply, Detector, ForumCategoryCarousel, HomePageCarousel
from .models import Product, PushShop, ShareStatistics, Credit, DevicePhone, ForumLabel, ForumArticle, UserForumArticle
# from .models import DetectorMacAddress

from rest_framework import serializers
from django.utils import timezone
from drf_extra_fields.fields import Base64ImageField


class DateTimeFieldWithTZ(serializers.DateTimeField):
    def to_representation(self, value):
        # value = timezone.localtime(value)
        value = value.replace(tzinfo=pytz.utc).astimezone(pytz.timezone('Asia/Shanghai'))
        return super(DateTimeFieldWithTZ, self).to_representation(value)


class LevelSerializer(serializers.HyperlinkedModelSerializer):
    icon = serializers.ImageField(allow_null=True, label='图标')

    class Meta:
        model = Level
        fields = ('url', 'level', 'icon', 'id')
        read_only_fields = ('id',)


class ShopSerializer(serializers.HyperlinkedModelSerializer):
    # created_at = DateTimeFieldWithTZ(label="创建时间", allow_null=True)
    # changed_at = DateTimeFieldWithTZ(label="更改时间", allow_null=True)

    class Meta:
        model = Shop
        # fields = ('url', 'id', 'name', 'category', 'address',
        #
        #           'dianping_business_id',
        #           'dianping_name',
        #           'dianping_address',
        #           'dianping_telephone',
        #           'dianping_city',
        #           'dianping_regions',
        #           'dianping_categories',
        #           'dianping_avg_rating',
        #           'dianping_avg_price',
        #           'dianping_longitude',
        #           'dianping_latitude',
        #           'dianping_business_url',
        #           'dianping_coupon_description',
        #           'dianping_coupon_id',
        #           'dianping_coupon_url',
        #           'dianping_deal_count',
        #           'dianping_deals',
        #           'dianping_has_coupon',
        #           'dianping_has_deal',
        #           'dianping_has_online_reservation',
        #           'dianping_online_reservation_url',
        #           'dianping_photo_url',
        #           'dianping_rating_img_url',
        #            'dianping_rating_s_img_url',
        #           'dianping_s_photo_url',
        #           'dianping_deals_description',
        #           'dianping_deals_id',
        #           'dianping_deals_url',
        #           )
        read_only_fields = ('id',)


class ShopCategorySerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ShopCategory


class ChinaSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = China
        fields = ('url', 'name', 'parent_id', 'id')


class DeviceSerializer(serializers.HyperlinkedModelSerializer):
    # used_time = serializers.IntegerField(allow_null=True, label='已使用时间（秒）')
    # nickname = serializers.CharField(allow_null=True, label='显示名称')
    # created_at = DateTimeFieldWithTZ(label="创建时间", allow_null=True)

    class Meta:
        model = Device
        # fields = ('url', 'sequence', 'id',
        #          'nickname', 'version', 'type', 'brand', 'used_time', 'user', 'password')
        # read_only_fields = ('id',)


class AddressSerializer(serializers.HyperlinkedModelSerializer):
    # CITY_CHOICES = (('BJ','北京'),)
    # DISTRICT_CHOICES = (
    #                     ('CP','昌平区'),
    #                     ('CW','崇文区'),
    #                     ('CY','朝阳区'),
    #                     ('DC','东城区'),
    #                     ('DX','大兴区'),
    #                     ('FS','房山区'),
    #                     ('FT','丰台区'),

    #                     ('HD','海淀区'),
    #                     ('HR','怀柔区'),
    #                     ('MTG','门头沟区'),
    #                     ('MY','密云县'),
    #                     ('PG','平谷区'),

    #                     ('SJS','石景山区'),
    #                     ('SY','顺义区'),
    #                     ('TZ','通州区'),
    #                     ('XC','西城区'),
    #                     ('XW','宣武区'),

    #                     ('YQ','延庆县'),
    #                     )
    # detail_address = serializers.CharField(allow_null=True, label='详细地址')

    class Meta:
        model = Address
        # fields = ('url', 'name', 'longitude', 'latitude', 'detail_address', 'china', 'id')
        # read_only_fields = ('-id',)


class UserExtensionSerializer(serializers.HyperlinkedModelSerializer):
    # big_image = serializers.ImageField(allow_null=True, label='大图片')
    # medium_image = serializers.ImageField(allow_null=True, label='中图片')
    # small_image = serializers.ImageField(allow_null=True, label='小图片')
    # account = serializers.FloatField(allow_null=True, label='账户余额')
    # nickname = serializers.CharField(allow_null=True, label='显示名称')
    # phone = serializers.CharField(allow_null=True, label='手机')
    # child_birth = serializers.DateField(allow_null=True, label="孩子出生日期")
    # created_at = DateTimeFieldWithTZ(label="创建时间", allow_null=True)
    forum_category = serializers.HyperlinkedRelatedField(
        many=True,
        read_only=True,
        view_name="forumcategory-detail"
    )

    class Meta:
        model = UserExtension
        fields = ('url', 'level', 'user', 'id',
                  'nickname', 'gender', 'has_child', 'child_birth',
                  'account', 'phone', 'score', 'big_image', 'medium_image',
                  'small_image', 'city', 'forum_category',
                  'device_token', 'company')
        read_only_fields = ('id', 'last_login', 'forum_category')
        extra_kwargs = {}


class UserSerializer(serializers.HyperlinkedModelSerializer):
    STAFF_CHOICES = ((False, '否'), (True, '是'))
    password = serializers.CharField(label='密码', write_only=True, style={'input_type': 'password'})
    username = serializers.CharField(label='用户名')
    is_staff = serializers.ChoiceField(choices=STAFF_CHOICES, label='员工', default=False)
    email = serializers.CharField(label='邮箱', allow_null=True)
    first_name = serializers.CharField(label='名字', allow_null=True)
    last_name = serializers.CharField(label='姓氏', allow_null=True)

    class Meta:
        model = User
        fields = ('url', 'username', 'password', 'first_name', 'last_name', 'is_staff', 'email', 'id')
        read_only_fields = ('id', 'passowrd')

    def create(self, validated_data):
        user = User(username=validated_data["username"], is_staff=validated_data["is_staff"])
        user.is_staff = validated_data.get("is_staff", False)
        if validated_data.get("email", None):
            user.email = validated_data.get("email", None)
        if validated_data.get("first_name", None):
            user.first_name = validated_data.get("first_name", None)
        if validated_data.get("last_name", None):
            user.last_name = validated_data.get("last_name", None)
        user.set_password(validated_data["password"])
        user.save()
        return user

    def update(self, instance, validated_data):
        instance.set_password(validated_data["password"])
        instance.is_staff = validated_data.get("is_staff", instance.is_staff)
        if validated_data.get("email", instance.email):
            instance.email = validated_data.get("email", instance.email)
        if validated_data.get("first_name", instance.first_name):
            instance.first_name = validated_data.get("first_name", instance.first_name)
        if validated_data.get("last_name", instance.last_name):
            instance.last_name = validated_data.get("last_name", instance.last_name)
        instance.save()
        return instance


class PublishSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Publish
        read_only_fields = ('id',)


class CommentSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Comment


class UserShopRelationsSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = UserShopRelations


class UserPublishRelationsSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = UserPublishRelations


class RedEnvelopePoolSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = RedEnvelopePool


class ForumCategorySerializer(serializers.HyperlinkedModelSerializer):
    # icon = serializers.ImageField(read_only=True)
    class Meta:
        model = ForumCategory


class ForumPostSerializer(serializers.HyperlinkedModelSerializer):
    post_image_1 = Base64ImageField(required=False)
    post_image_2 = Base64ImageField(required=False)
    post_image_3 = Base64ImageField(required=False)
    post_image_4 = Base64ImageField(required=False)
    post_image_5 = Base64ImageField(required=False)
    post_image_6 = Base64ImageField(required=False)
    post_image_7 = Base64ImageField(required=False)
    post_image_8 = Base64ImageField(required=False)
    post_image_9 = Base64ImageField(required=False)

    class Meta:
        model = ForumPost


class ForumPostSourceSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ForumPost


class ForumReplySerializer(serializers.HyperlinkedModelSerializer):
    reply_image_1 = Base64ImageField(required=False)
    reply_image_2 = Base64ImageField(required=False)
    reply_image_3 = Base64ImageField(required=False)
    reply_image_4 = Base64ImageField(required=False)
    reply_image_5 = Base64ImageField(required=False)
    reply_image_6 = Base64ImageField(required=False)
    reply_image_7 = Base64ImageField(required=False)
    reply_image_8 = Base64ImageField(required=False)
    reply_image_9 = Base64ImageField(required=False)

    class Meta:
        model = ForumReply


class ForumReplySourceSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ForumReply


class DetectorSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Detector


# class DetectorMacAddressSerializer(serializers.HyperlinkedModelSerializer):
#
#     class Meta:
#         model = DetectorMacAddress


class ForumCategoryCarouselSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ForumCategoryCarousel


class ChannelSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Channel


class ProductSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Product


class HomePageCarouselSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = HomePageCarousel


class UseSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Use


class CouponSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Coupon


class GameSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Game


class DetectorRelationSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = DetectorRelation


class CompanySerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Company


class PopWindowSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = PopWindow


class PushShopSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = PushShop
        fields = ["name", "id"]


class ShareStatisticsSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ShareStatistics


class CreditSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = Credit


class DevicePhoneSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = DevicePhone


class ForumLabelSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ForumLabel


class ForumArticleSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = ForumArticle


class UserForumArticleSerializer(serializers.HyperlinkedModelSerializer):
    class Meta:
        model = UserForumArticle
