# coding:utf-8
import datetime
from django.db import models
from django.contrib.auth.models import User
from django.db.models.signals import post_save
from ckeditor.fields import RichTextField
import sys

reload(sys)
sys.setdefaultencoding('utf8')

AUDIT_CHOICES = ((-1, '未审核'), (0, '审核未通过'), (1, '审核通过'))
ATTRIBUTE_PUBLISH_CHOICES = ((0, "踩"), (-1, "无"), (1, "赞"))
ATTRIBUTE_CHOICES = ((-1, "不推荐"), (0, "无"), (1, "推荐"))
GENDER_CHOICES = (('M', '男'), ('F', '女'))
CHILD_CHOICES = (('Y', '有'), ('N', '没有'))
NOISE_CHOICES = tuple([(x + 1, str(x + 1) + "分贝") for x in xrange(100)])
IS_CHOICES = ((0, '否'), (1, '是'))
HAS_CHOICES = ((0, '没有'), (1, '有'))
CROWD_CHOICES = ((0, '没人'), (1, '人很少'), (2, '一般'), (3, '有点多'), (4, '座位都满了'), (5, '这是春运嘛！'))
QUIET_CHOICES = ((0, '一直很安静'), (1, '让我轻轻地告诉你'), (2, '一般'), (3, '说话靠吼'), (4, '请在耳边吼'),
                 (5, '请不要吼了，我真听不清！'))
FOOD_CHOICES = ((0, '等你一万年'), (1, '龟速'), (2, '一般'), (3, '豹的速度'), (4, '闪电侠'), (5, '曹操'))
SORT_CHOICES = (('PM2_5', 'PM2.5'), ('checked_at', '检测时间'), ('formaldehyde', '甲醛'),
                ('temperature', '温度'), ('humidity', '湿度'), ('noise', '噪音'),
                ('has_WIFI', '拥有WIFI'), ('has_parking_charge', '拥有停车收费'),
                ('has_monitor', '拥有监控'),
                ('crowd_level', '人数'), ('quiet_level', '安静'), ('food_level', '上菜速度'))
LEVEL_CHOICES = tuple([(x + 1, "第" + str(x + 1) + "级") for x in xrange(100)])


def get_image_upload_to(the_models, file_name):
    now = datetime.datetime.now()
    name = ""
    # return "publish/%s" %(file_name)
    try:
        model_name = the_models.__name__
    except Exception as ex:
        model_name = type(the_models).__name__
    if model_name == "Publish":
        name = "publish"
    elif model_name == "ForumPost":
        name = "forum/post"
    elif model_name == "ForumReply":
        name = "forum/reply"
    elif model_name == "ForumCategoryCarousel":
        name = "forum/category/carousel"
    elif model_name == "Channel":
        name = "forum/shop/coupon/channel"
    elif model_name == "HomePageCarousel":
        name = "forum/shop/carousel"
    elif model_name == "PopWindow":
        name = "forum/shop/pop_window"
    elif model_name == "Company":
        name = "forum/user/company"
    elif model_name == "ForumArticle":
        name = "forum/article"
    path = "%s/%s/%s/%s/%s" % (name, str(now.year), str(now.month), str(now.day), file_name)

    import os
    if os.path.exists(path):
        pass
    else:
        print "create path %s" % path
        os.makedirs(path)

    return path


class Level(models.Model):
    level = models.SmallIntegerField(verbose_name="等级", choices=LEVEL_CHOICES, unique=True)
    icon = models.ImageField(verbose_name="图标", blank=True, upload_to="level/", null=True, default=None)

    class Meta:
        ordering = ['level']
        verbose_name = u"级别"
        verbose_name_plural = u"级别"

    def __unicode__(self):
        return str(self.level)


class China(models.Model):
    name = models.CharField(verbose_name="名称", max_length=30, default="")
    parent_id = models.IntegerField(verbose_name="上级目录")

    def __unicode__(self):
        return self.name


class Address(models.Model):
    name = models.CharField(verbose_name="地址名称", max_length=254, blank=True, default="")
    china = models.ForeignKey(China, verbose_name="地区", null=True, blank=True)
    longitude = models.FloatField(verbose_name="经度")
    latitude = models.FloatField(verbose_name="纬度")
    detail_address = models.CharField(verbose_name="详细地址", max_length=254, blank=True, default="", null=True)
    geohash_code = models.CharField(verbose_name="geohash编码", default=None, max_length=20, null=True, blank=True)
    audit = models.SmallIntegerField(verbose_name="软删除标志", default=-1)

    def __unicode__(self):
        return str(self.longitude) + "_" + str(self.latitude) + "_" + self.detail_address

    class Meta:
        unique_together = ('longitude', 'latitude', 'detail_address')
        verbose_name = u"地址"
        verbose_name_plural = u"地址"


class ShopCategory(models.Model):
    name = models.CharField(verbose_name="名称", max_length=30, default="", unique=True)
    icon = models.CharField(verbose_name="默认图片路径", max_length=300, default=None, blank=True, null=True)
    parent_id = models.IntegerField(verbose_name="上级目录")

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"场所类别"
        verbose_name_plural = u"场所类别"


class Shop(models.Model):
    audit = models.SmallIntegerField(verbose_name="小编审核", choices=AUDIT_CHOICES, default=-1, null=True)
    name = models.CharField(verbose_name="场所名称", max_length=254, default="")
    category = models.ForeignKey(ShopCategory, verbose_name="场所类别", null=True, default=None)
    address = models.ForeignKey(Address, verbose_name="地址", null=True, default=None)
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    rating_img_url = models.CharField(verbose_name="星级图片URL", max_length=254, default="")
    rating_s_img_url = models.CharField(verbose_name="星级小图片URL", max_length=254, default="")
    photo_url = models.CharField(verbose_name="图片URL", max_length=254, default="")
    s_photo_url = models.CharField(verbose_name="小图片URL", max_length=254, default="")

    formaldehyde = models.FloatField(verbose_name="甲醛", null=True, blank=True)
    formaldehyde_image = models.ImageField(verbose_name="甲醛图片",
                                           null=True, blank=True, upload_to="shop/formaldehyde/")
    temperature = models.FloatField(verbose_name="温度", null=True, blank=True)
    humidity = models.FloatField(verbose_name="湿度", null=True, blank=True)
    CO2 = models.FloatField(verbose_name="二氧化碳", null=True, blank=True)
    TVOC = models.FloatField(verbose_name="总挥发性有机物", null=True, blank=True)

    dianping_business_id = models.IntegerField(verbose_name="大众点评场所ID", null=True, default=0)
    dianping_name = models.CharField(verbose_name="大众点评场所名称", max_length=254, default="")
    dianping_address = models.CharField(verbose_name="大众点评地址", max_length=254, default="")
    dianping_telephone = models.CharField(verbose_name="大众点评带区号的电话", max_length=254, default="")
    dianping_city = models.CharField(verbose_name="大众点评所在城市", max_length=254, default="")
    dianping_regions = models.CharField(verbose_name="大众点评所在区域列表", max_length=254, default="")
    dianping_categories = models.CharField(verbose_name="大众点评所属分类信息列表", max_length=254, default="")
    dianping_avg_rating = models.FloatField(verbose_name="大众点评星级评分", null=True, default=0)
    dianping_avg_price = models.FloatField(verbose_name="大众点评人均价格（没有为-1)", null=True, default=None, blank=True)
    dianping_longitude = models.FloatField(verbose_name="大众点评经度值", default=0)
    dianping_latitude = models.FloatField(verbose_name="大众点评纬度值", default=0)
    geohash_code = models.CharField(verbose_name="geohash编码", default=None, null=True, blank=True, max_length=20)

    dianping_has_coupon = models.SmallIntegerField(verbose_name="是否有优惠券", default=0, null=True)
    dianping_coupon_id = models.IntegerField(verbose_name="优惠券ID", default=0, null=True)
    dianping_coupon_description = models.CharField(verbose_name="优惠券页面描述", max_length=254, default="")
    dianping_coupon_url = models.CharField(verbose_name="优惠券页面链接", max_length=254, default="")
    dianping_has_deal = models.SmallIntegerField(verbose_name="是否有团购", default=0, null=True)
    dianping_deal_count = models.IntegerField(verbose_name="场所当前在线团购数量", default=0, null=True)
    dianping_deals = models.CharField(verbose_name="团购列表", max_length=1024, default="")
    dianping_deals_id = models.CharField(verbose_name="团购ID", max_length=254, default="")
    dianping_deals_description = models.CharField(verbose_name="团购描述", max_length=254, default="")
    dianping_deals_url = models.CharField(verbose_name="团购页面链接", max_length=254, default="")
    dianping_has_online_reservation = models.SmallIntegerField(verbose_name="是否在线预订", default=0, null=True)
    dianping_online_reservation_url = models.CharField(verbose_name="在线预订页面链接，目前仅返回HTML5站点链接",
                                                       max_length=254, default="")
    dianping_business_url = models.CharField(verbose_name="场所页面链接", max_length=254, default="")
    dianping_rating_img_url = models.CharField(verbose_name="大众点评星级图片URL", max_length=254, default="")
    dianping_rating_s_img_url = models.CharField(verbose_name="大众点评星级小图片URL", max_length=254, default="")
    dianping_photo_url = models.CharField(verbose_name="大众点评图片URL", max_length=254, default="")
    dianping_s_photo_url = models.CharField(verbose_name="大众点评小图片URL", max_length=254, default="")

    weight = models.IntegerField(verbose_name="小编的场所权重", default=0)
    has_detector = models.BooleanField(verbose_name="是否绑定商用检测器", default=False)

    def __unicode__(self):
        if self.dianping_name:
            name = self.dianping_name
        else:
            name = self.name
        return name

    class Meta:
        verbose_name = u"场所"
        verbose_name_plural = u"场所"


class ShopDeal(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    dianping_business_id = models.IntegerField(verbose_name="大众点评场所ID")
    dianping_city = models.CharField(verbose_name="大众点评场所所在城市", max_length=300)
    status = models.BooleanField(verbose_name="团购状态", default=True)
    dianping_deals_id = models.CharField(verbose_name="团购ID", max_length=254, unique=True)
    dianping_deals_description = models.CharField(verbose_name="团购描述", max_length=254, default="")
    dianping_deals_url = models.CharField(verbose_name="团购页面链接", max_length=254, default="")
    dianping_deals_list_price = models.FloatField(verbose_name="团购包含商品原价值", null=True,
                                                  default=None, blank=True)
    dianping_deals_current_price = models.FloatField(verbose_name="团购价格", null=True, default=None, blank=True)
    dianping_deals_purchase_count = models.IntegerField(verbose_name="团购当前已购买数", default=0, null=True)
    dianping_deals_is_refundable = models.BooleanField(verbose_name="是否支持随时退款", default=False)
    dianping_deals_is_reservation_required = models.BooleanField(verbose_name="是否需要预约", default=False)
    dianping_deals_title = models.CharField(verbose_name="团购标题", max_length=300, default=None)
    dianping_deals_image_url = models.CharField(verbose_name="大众点评团购图片URL", max_length=254, default="")
    dianping_deals_s_image_url = models.CharField(verbose_name="大众点评团购小图片URL", max_length=254, default="")

    class Meta:
        verbose_name = u"团购"
        verbose_name_plural = u"团购"


class ForumCategory(models.Model):
    name = models.CharField(verbose_name="版块名称", max_length=150)
    subheading = models.CharField(verbose_name="版块副标题", max_length=150, default=None)
    icon = models.ImageField(verbose_name="版块图片", null=True, blank=True, default=None, upload_to="forum/category/")
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    changed_at = models.DateTimeField(verbose_name="修改于", auto_now=True, null=True)
    black_men = models.CharField(verbose_name="被封用户IDs", max_length=500, null=True)
    status = models.BooleanField(verbose_name="分类状态（有效，无效）", default=True)
    owners = models.ManyToManyField(User)
    weight = models.IntegerField(verbose_name="排序权重", default=0)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"社区类别"
        verbose_name_plural = u"社区类别"


class Company(models.Model):
    name = models.CharField(verbose_name="公司名称", max_length=255, unique=True)
    image = models.ImageField(verbose_name="公司图片", default=None, null=True, blank=True,
                              upload_to=get_image_upload_to)
    site_url = models.CharField(verbose_name="公司网址", max_length=300)
    start_datetime = models.DateTimeField(verbose_name="活动开始日期", null=True, blank=True)
    end_datetime = models.DateTimeField(verbose_name="活动结束日期", null=True, blank=True)
    logo = models.ImageField(verbose_name="公司logo", default=None, null=True, blank=True,
                             upload_to=get_image_upload_to)
    weight = models.IntegerField(verbose_name="排序权重", default=0, null=True, blank=True)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"公司"
        verbose_name_plural = u"公司"


class UserExtension(models.Model):
    user = models.OneToOneField(User, verbose_name="用户", null=True)
    level = models.ForeignKey(Level, verbose_name="级别", null=True)

    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True)

    nickname = models.CharField(verbose_name="显示名称", max_length=30, blank=True, default="")
    real_name = models.CharField(verbose_name="真实姓名", max_length=50, blank=True, default="")
    gender = models.CharField(verbose_name="性别", choices=GENDER_CHOICES, max_length=1, blank=True, default='M')
    has_child = models.CharField(verbose_name="有孩子", choices=CHILD_CHOICES, max_length=1, blank=True, default='N')
    child_birth = models.DateField(verbose_name="孩子出生日期", blank=True, null=True)
    account = models.FloatField(verbose_name="账户余额", blank=True, null=True, default=0)
    last_bonus = models.FloatField(verbose_name="上次红包", blank=True, null=True, default=0)
    phone = models.CharField(verbose_name="手机号码", max_length=20, blank=True, default="")
    score = models.IntegerField(verbose_name="积分", default=0, null=True)
    city = models.CharField(verbose_name="城市", blank=True, default="", null=True, max_length=50)
    address = models.CharField(verbose_name="详细地址", blank=True, default="", null=True, max_length=100)

    collected_shop = models.ManyToManyField(Shop, verbose_name="收藏的场所", blank=True)

    big_image = models.ImageField(verbose_name="大图片", blank=True, upload_to='user/', null=True)
    medium_image = models.ImageField(verbose_name="中图片", blank=True, upload_to='user/', null=True)
    small_image = models.ImageField(verbose_name="小图片", blank=True, upload_to='user/', null=True)

    forum_category = models.ManyToManyField(ForumCategory, verbose_name="关注版块")

    device_token = models.CharField(verbose_name="设备TOKEN", max_length=64, blank=True, null=True, default=None)
    win_count = models.IntegerField(verbose_name="未读的赞数量", default=0, null=True)
    lost_count = models.IntegerField(verbose_name="未读的踩数量", default=0, null=True)
    company = models.ForeignKey(Company, verbose_name="公司", blank=True, null=True)
    get_coupon_flags = models.CharField(verbose_name="获得优惠券的标志", max_length=300, blank=True, default=None, null=True)

    activity_id = models.IntegerField(verbose_name="活动ID", default=0, blank=True)

    age_period = models.IntegerField(verbose_name="年龄段", default=-1, blank=True)

    def __unicode__(self):
        if self.user:
            return self.user.username
        else:
            return u""

    class Meta:
        verbose_name = u"用户信息"
        verbose_name_plural = u"用户信息"


def create_user_profile(sender, instance, created, **kwargs):
    if created:
        profile, created = UserExtension.objects.get_or_create(user=instance)


post_save.connect(create_user_profile, sender=User)


class Device(models.Model):
    TYPE_CHOICES = ((0, 'PM2.5'), (1, '甲醛'))
    BRAND_CHOICES = ((0, '三个爸爸'),)
    nickname = models.CharField(verbose_name="设备昵称", max_length=30, blank=True, default="")
    sequence = models.CharField(verbose_name="设备序列号", max_length=128, unique=True)
    version = models.CharField(verbose_name="设备版本号", max_length=20, blank=True, default="")
    type = models.IntegerField(verbose_name="设备类型", choices=TYPE_CHOICES, default=0, null=True, blank=True)
    brand = models.IntegerField(verbose_name="品牌", choices=BRAND_CHOICES, default=0, null=True, blank=True)
    used_time = models.BigIntegerField(verbose_name="已使用时间(秒）", blank=True, default=0, null=True)
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, blank=True, null=True)
    changed_at = models.DateTimeField(verbose_name="修改时间", auto_now=True, null=True, blank=True)
    is_published = models.BooleanField(verbose_name="是否发布", blank=True, default=False)
    icon = models.IntegerField(verbose_name="设备图标", default=0, null=True, blank=True)
    # icon = models.ImageField(verbose_name="设备图标", blank=True, upload_to='device/', null=True)

    user = models.ForeignKey(User, verbose_name="用户", default=1)
    password = models.CharField(verbose_name="设备密码", max_length=6, default="ffffff")

    def __unicode__(self):
        return self.nickname

    class Meta:
        verbose_name = u"设备"
        verbose_name_plural = u"设备"


class Publish(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True)

    PM2_5 = models.IntegerField(verbose_name="PM2.5", default=-1, null=True, blank=True)
    checked_at = models.DateTimeField(verbose_name="测量时间", null=True, blank=True)
    formaldehyde = models.FloatField(verbose_name="甲醛", null=True, blank=True)
    temperature = models.FloatField(verbose_name="温度", null=True, blank=True)
    humidity = models.FloatField(verbose_name="湿度", null=True, blank=True)
    content = models.CharField(verbose_name="内容", max_length=254, blank=True, default="")

    table = models.SmallIntegerField(verbose_name="桌子是否干净", default=0, null=True, blank=True)
    floor = models.SmallIntegerField(verbose_name="地板是否干净", default=0, null=True, blank=True)
    tableware = models.SmallIntegerField(verbose_name="餐具是否干净", default=0, null=True, blank=True)
    people = models.SmallIntegerField(verbose_name="人多不多", default=0, null=True, blank=True)
    quiet = models.SmallIntegerField(verbose_name="是否安静", default=0, null=True, blank=True)
    child = models.SmallIntegerField(verbose_name="是否适合带孩子玩", default=0, null=True, blank=True)
    lampblack = models.SmallIntegerField(verbose_name="有无油烟", default=0, null=True, blank=True)
    smell = models.SmallIntegerField(verbose_name="有无异味", default=0, null=True, blank=True)
    equipment_new = models.SmallIntegerField(verbose_name="器材新旧", default=0, null=True, blank=True)
    equipment_clean = models.SmallIntegerField(verbose_name="设施干不干净", default=0, null=True, blank=True)
    area = models.SmallIntegerField(verbose_name="是否宽敞", default=0, null=True, blank=True)
    water = models.SmallIntegerField(verbose_name="游泳池水是否干净", default=0, null=True, blank=True)
    light = models.SmallIntegerField(verbose_name="光线如何", default=0, null=True, blank=True)
    bedsheet = models.SmallIntegerField(verbose_name="床单是否干净", default=0, null=True, blank=True)
    soundproof = models.SmallIntegerField(verbose_name="隔音好不好", default=0, null=True, blank=True)
    wet = models.SmallIntegerField(verbose_name="是否潮湿", default=0, null=True, blank=True)
    pet = models.SmallIntegerField(verbose_name="能不能带宠物", default=0, null=True, blank=True)
    watchman = models.SmallIntegerField(verbose_name="有无看护人员", default=0, null=True, blank=True)
    has_monitor = models.SmallIntegerField(verbose_name="拥有监控", default=0, null=True, blank=True)
    toilet = models.SmallIntegerField(verbose_name="有无公厕", default=0, null=True, blank=True)

    user = models.ForeignKey(User, verbose_name="用户")
    device = models.ForeignKey(Device, verbose_name="设备", blank=True, null=True)
    shop = models.ForeignKey(Shop, verbose_name="场所")

    audit = models.SmallIntegerField(verbose_name="小编审核", choices=AUDIT_CHOICES, default=-1, null=True)
    is_recommended = models.SmallIntegerField(verbose_name="是否推荐", choices=IS_CHOICES, null=True, default=None)
    recommend_weight = models.SmallIntegerField(verbose_name="推荐权重", default=None, null=True)
    is_hot = models.SmallIntegerField(verbose_name="是否热门", choices=IS_CHOICES, null=True, default=None)
    is_reported = models.SmallIntegerField(verbose_name="是否被举报", choices=IS_CHOICES, null=True, default=None)
    report_reason = models.CharField(verbose_name="举报原因", max_length=254, default="")
    recommend_sort = models.CharField(verbose_name="推荐排序", max_length=254, choices=SORT_CHOICES, default='PM2_5')

    big_image = models.ImageField(verbose_name="大图片", upload_to=get_image_upload_to,
                                  null=True, blank=True, default=None)
    medium_image = models.ImageField(verbose_name="中图片", upload_to=get_image_upload_to,
                                     null=True, blank=True, default=None)
    small_image = models.ImageField(verbose_name="小图片", upload_to=get_image_upload_to,
                                    null=True, blank=True, default=None)

    win_count = models.IntegerField(verbose_name="未读的赞数量", default=0)
    lost_count = models.IntegerField(verbose_name="未读的踩数量", default=0)

    def __unicode__(self):
        return str(self.checked_at) + "_" + str(self.PM2_5) + "_" + self.shop.dianping_name

    class Meta:
        verbose_name = u"发布"
        verbose_name_plural = u"发布"


class Comment(models.Model):
    content = models.CharField(verbose_name="评论内容", max_length=254, blank=True, default="")
    # attribute = models.SmallIntegerField(verbose_name="态度", choices=ATTRIBUTE_PUBLISH_CHOICES, default=0)

    user = models.ForeignKey(User, verbose_name="用户")
    publish = models.ForeignKey(Publish, verbose_name="发布信息")
    # comment_id = models.IntegerField(verbose_name="上一评论的ID", blank=True, default=0, null=True)
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True)
    is_read = models.BooleanField(verbose_name="已经读过", default=False)

    # big_image = models.ImageField(verbose_name="大图片", upload_to='comment/', null=True)
    # medium_image = models.ImageField(verbose_name="中图片", upload_to='comment/', null=True)
    # small_image = models.ImageField(verbose_name="小图片", upload_to='comment/', null=True)

    class Meta:
        verbose_name = u"发布的评论"
        verbose_name_plural = u"发布的评论"


class UserShopRelations(models.Model):
    user = models.ForeignKey(User, verbose_name="用户")
    shop = models.ForeignKey(Shop, verbose_name="场所")
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True)
    last_modified_at = models.DateTimeField(verbose_name="更改于", auto_now=True)
    is_recommended = models.SmallIntegerField(verbose_name="是否推荐", choices=ATTRIBUTE_CHOICES, default=-1, null=True)

    def __unicode__(self):
        return str(self.user.username) + "_" + self.shop.dianping_name

    class Meta:
        unique_together = ('user', 'shop')
        verbose_name = u"用户与场所"
        verbose_name_plural = u"用户与场所"


class UserPublishRelations(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True)
    last_modified_at = models.DateTimeField(verbose_name="更改于", auto_now=True)
    user = models.ForeignKey(User, verbose_name="用户")
    publish = models.ForeignKey(Publish, verbose_name="发布信息")
    attribute = models.SmallIntegerField(verbose_name="态度", choices=ATTRIBUTE_PUBLISH_CHOICES, default=-1, null=True)

    def __unicode__(self):
        return str(self.user.username) + "_" + self.publish.PM2_5

    class Meta:
        unique_together = ('user', 'publish')
        verbose_name = u"用户与发布"
        verbose_name_plural = u"用户与发布"


class RedEnvelope(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True, null=True, blank=True)
    changed_at = models.DateTimeField(verbose_name="修改时间", auto_now=True, null=True, blank=True)
    publish = models.ForeignKey(Publish, null=True, blank=True)
    user = models.ForeignKey(User)
    device = models.ForeignKey(Device)
    type = models.SmallIntegerField(verbose_name="红包类型")
    bonus = models.FloatField(verbose_name="红包数额")
    state = models.SmallIntegerField(verbose_name="红包状态")
    is_withdraw = models.BooleanField(verbose_name="是否已提现", default=False)


class RedEnvelopePool(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    last_modified_at = models.DateTimeField(verbose_name="更改于", auto_now=True, null=True)

    bonus = models.FloatField(verbose_name="已经发放但没有接收到的红包", default=0)


class RedEnvelopeConstant(models.Model):
    name = models.CharField(verbose_name="红包类型的名称", max_length=50, default=None)
    bonus_min = models.FloatField(verbose_name="红包的最小值")
    bonus_max = models.FloatField(verbose_name="红包的最大值")
    type = models.SmallIntegerField(verbose_name="红包的类型")
    threshold = models.FloatField(verbose_name="红包的限额", default=0)
    possibility = models.FloatField(verbose_name="得红包的概率", default=1)
    extra_keep = models.SmallIntegerField(verbose_name="能获取红包的连续天数", blank=True, null=True)
    start_time = models.TimeField(verbose_name="红包雨的起始时间", blank=True, null=True)
    end_time = models.TimeField(verbose_name="红包雨的结束时间", blank=True, null=True)
    rain_count = models.IntegerField(verbose_name="红包雨的数量", blank=True, null=True)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"红包常量"
        verbose_name_plural = u"红包常量"


class PhoneInfos(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    phone_id = models.CharField(verbose_name="手机ID", max_length=50, unique=True, primary_key=True)
    os_version = models.CharField(verbose_name="系统版本", max_length=100)
    phone_number = models.CharField(verbose_name="手机号", max_length=20)
    phone_type = models.CharField(verbose_name="手机型号", max_length=50)

    def __unicode___(self):
        return self.phone_number

    class Meta:
        verbose_name = u"手机信息"
        verbose_name_plural = u"手机信息"


class Feedback(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    user = models.ForeignKey(User, verbose_name="用户", null=True)
    phone = models.ForeignKey(PhoneInfos, verbose_name="手机信息")
    content = models.CharField(verbose_name="反馈内容", max_length=500)

    class Meta:
        verbose_name = u"反馈信息"
        verbose_name_plural = u"反馈信息"


class ClickTitleRecord(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    title_id = models.IntegerField(verbose_name="微信标题ID", default=0)
    title = models.CharField(verbose_name="标题", max_length=150, default=None, null=True, blank=True)

    class Meta:
        verbose_name = u"微信点击记录"
        verbose_name_plural = u"微信点击记录"


class ForumPost(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    changed_at = models.DateTimeField(verbose_name="修改于", auto_now=True, null=True)
    title = models.CharField(verbose_name="标题", max_length=150, default=None)
    # content = models.TextField(verbose_name="内容", max_length=5000, default=None))
    content = RichTextField(verbose_name="富文本正文")
    binary_content = models.BinaryField(verbose_name="二进制富文本正文", default=None, null=True)
    category = models.ForeignKey(ForumCategory, verbose_name="版块", default=None)
    owner = models.ForeignKey(User, verbose_name="发贴用户", null=True, blank=True)
    status = models.BooleanField(verbose_name="发贴状态(有效，无效)", default=True)
    is_category_rule = models.BooleanField(verbose_name="是否版规", default=False)
    is_digest = models.BooleanField(verbose_name="是否加精", default=False)
    is_top = models.BooleanField(verbose_name="是否置顶", default=False)
    top_weight = models.IntegerField(verbose_name="置顶权重", default=0)
    win_users = models.ManyToManyField(UserExtension, verbose_name="点赞的用户们", default=None, blank=True)

    post_image_1 = models.ImageField(verbose_name="图片1", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_2 = models.ImageField(verbose_name="图片2", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_3 = models.ImageField(verbose_name="图片3", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_4 = models.ImageField(verbose_name="图片4", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_5 = models.ImageField(verbose_name="图片5", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_6 = models.ImageField(verbose_name="图片6", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_7 = models.ImageField(verbose_name="图片7", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_8 = models.ImageField(verbose_name="图片8", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)
    post_image_9 = models.ImageField(verbose_name="图片9", default=None, null=True, blank=True,
                                     upload_to=get_image_upload_to)

    def __unicode__(self):
        return self.title

    class Meta:
        verbose_name = u"社区贴子"
        verbose_name_plural = u"社区贴子"


class ForumReply(models.Model):
    created_at = models.DateTimeField(verbose_name="创建于", auto_now_add=True, null=True)
    changed_at = models.DateTimeField(verbose_name="修改于", auto_now=True, null=True)
    content = models.CharField(verbose_name="内容", max_length=5000, default=None)
    post = models.ForeignKey(ForumPost, verbose_name="主帖", null=True, blank=True)
    owner = models.ForeignKey(User, verbose_name="回贴用户", null=True, blank=True)
    status = models.BooleanField(verbose_name="回贴状态(有效，无效)", default=True, blank=True)
    parent_reply = models.IntegerField(verbose_name="回复的ID", default=0)

    reply_image_1 = models.ImageField(verbose_name="图片1", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_2 = models.ImageField(verbose_name="图片2", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_3 = models.ImageField(verbose_name="图片3", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_4 = models.ImageField(verbose_name="图片4", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_5 = models.ImageField(verbose_name="图片5", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_6 = models.ImageField(verbose_name="图片6", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_7 = models.ImageField(verbose_name="图片7", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_8 = models.ImageField(verbose_name="图片8", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)
    reply_image_9 = models.ImageField(verbose_name="图片9", default=None, null=True, blank=True,
                                      upload_to=get_image_upload_to)

    class Meta:
        verbose_name = u"社区贴子的回复"
        verbose_name_plural = u"社区贴子的回复"


class Detector(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True, null=True, blank=True)
    version = models.CharField(verbose_name="版本号", default="1.0.0", max_length=10, null=True, blank=True)
    mac_address = models.CharField(verbose_name="设备MAC地址", max_length=50)
    pm2_5 = models.IntegerField(verbose_name="PM2.5", default=-1)
    carbon_dioxide = models.IntegerField(verbose_name="CO2", default=-1)
    temperature = models.FloatField(verbose_name="温度", default=-1)
    humidity = models.FloatField(verbose_name="湿度", default=-1)

    class Meta:
        verbose_name = u"检测器及其获取的信息"
        verbose_name_plural = u"检测器及其获取的信息"


class ForumCategoryCarousel(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True, null=True, blank=True)
    url_address = models.CharField(verbose_name="链接地址", max_length=150, default=None)
    descriptions = models.CharField(verbose_name="描述", max_length=300, default=None)
    image = models.ImageField(verbose_name="走马灯图片", default=None, null=True, blank=True,
                              upload_to=get_image_upload_to)
    weight = models.IntegerField(verbose_name="排序权重", default=0)

    def __unicode__(self):
        return self.descriptions

    class Meta:
        verbose_name = u"社区类别的走马灯"
        verbose_name_plural = u"社区类别的走马灯"


class Weather(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True, null=True, blank=True)
    changed_at = models.DateTimeField(verbose_name="修改于", auto_now=True, null=True)
    city = models.CharField(verbose_name="城市名称", max_length=50, primary_key=True)
    pm2_5 = models.IntegerField(verbose_name="PM2.5", default=-1)
    temperature = models.FloatField(verbose_name="温度", default=-1)
    humidity = models.FloatField(verbose_name="湿度", default=-1)
    condition = models.CharField(verbose_name="天气状况描述", max_length=100, default=None, null=True, blank=True)
    code = models.IntegerField(verbose_name="天气状况CODE", default=0)

    def __unicode__(self):
        return self.city

    class Meta:
        verbose_name = u"天气"
        verbose_name_plural = u"天气"


class Channel(models.Model):
    name = models.CharField(verbose_name="渠道名称", default=None, null=True, unique=True, max_length=100)
    icon = models.ImageField(verbose_name="渠道图片", default=None, null=True, blank=True,
                             upload_to=get_image_upload_to)
    rules = models.CharField(verbose_name="活动规则", default=None, null=True, max_length=1000)
    shortcut = models.CharField(verbose_name="简称", default=None, null=True, max_length=50)
    audit = models.IntegerField(verbose_name="审核", default=1)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"渠道"
        verbose_name_plural = u"渠道"


class HomePageCarousel(models.Model):
    name = models.CharField(verbose_name="轮播图名称", default=None, unique=True, max_length=100)
    src = models.CharField(verbose_name="链接", default=None, null=True, max_length=300)
    icon = models.ImageField(verbose_name="轮播图图片", default=None, null=True, blank=True,
                             upload_to=get_image_upload_to)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"主页走马灯"
        verbose_name_plural = u"主页走马灯"


class Use(models.Model):
    name = models.CharField(verbose_name="用途名称", default=None, null=True, unique=True, max_length=100)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"用途"
        verbose_name_plural = u"用途"


class Product(models.Model):
    name = models.CharField(verbose_name="产品名称", default=None, null=True, max_length=100)
    key = models.IntegerField(verbose_name="产品关键字", default=0, null=True, unique=True)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"产品"
        verbose_name_plural = u"产品"


class Coupon(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True, null=True, blank=True)
    changed_at = models.DateTimeField(verbose_name="修改于", auto_now=True, null=True)
    activated_at = models.DateTimeField(verbose_name="激活于", null=True, default=None)
    valid_start = models.DateTimeField(verbose_name="有效期开始于", null=True, default=None)
    valid_end = models.DateTimeField(verbose_name="有效期结束于", null=True, default=None)
    sequence = models.CharField(verbose_name="优惠券序列号", max_length=100, unique=True, default=0)
    youzan_sequence = models.CharField(verbose_name="有赞优惠券序列号", max_length=100, default=None, null=True)
    coupon_value = models.FloatField(verbose_name="优惠券券值", default=0)
    descriptions = models.CharField(verbose_name="描述", max_length=100, default=None, null=True)
    channel = models.ForeignKey(Channel, verbose_name="渠道ID", default=None, null=True)
    use = models.ForeignKey(Use, verbose_name="用途", default=None, null=True)
    user = models.ForeignKey(User, verbose_name="用户", default=None, null=True)
    is_valid = models.BooleanField(verbose_name="是否有效", default=True)
    valid_days = models.IntegerField(verbose_name="有效天数", default=0)
    # product = models.IntegerField(verbose_name="产品", default=0)
    product = models.ForeignKey(Product, verbose_name="产品", default=None, null=True)
    is_out = models.BooleanField(verbose_name="是否被抛弃了", default=False)

    def __unicode__(self):
        return self.sequence

    class Meta:
        verbose_name = u"优惠券"
        verbose_name_plural = u"优惠券"


class Game(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True, null=True, blank=True)
    changed_at = models.DateTimeField(verbose_name="修改于", auto_now=True, null=True)
    score = models.IntegerField(verbose_name="答对的题数", default=0)
    username = models.CharField(verbose_name="用户名", max_length=100, default=None)

    # user = models.ForeignKey(User, verbose_name="用户")

    class Meta:
        verbose_name = u"游戏"
        verbose_name_plural = u"游戏"


class DetectorRelation(models.Model):
    user = models.ForeignKey(User, verbose_name="用户")
    mac_address = models.CharField(verbose_name="设备MAC地址", max_length=50, unique=True)
    address = models.CharField(verbose_name="设备所在地址", max_length=100)
    city = models.CharField(verbose_name="城市", max_length=100, default=None)
    shop = models.ForeignKey(Shop, verbose_name="场所", blank=True, null=True)
    threshold = models.IntegerField(verbose_name="阈值", blank=True, null=True, default=0)
    state = models.BooleanField(verbose_name="状态", default=True)

    class Meta:
        unique_together = ('user', 'mac_address')
        verbose_name = u"检测器与用户的联系"
        verbose_name_plural = u"检测器与用户的联系"

    def __unicode__(self):
        return self.mac_address


class PopWindow(models.Model):
    name = models.CharField(verbose_name="弹窗名称", default=None, unique=True, max_length=100)
    src = models.CharField(verbose_name="弹窗链接", default=None, null=True, max_length=300)
    icon = models.ImageField(verbose_name="弹窗图片", default=None, null=True, blank=True,
                             upload_to=get_image_upload_to)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u"弹窗"
        verbose_name_plural = u"弹窗"


class PushShop(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    name = models.CharField(verbose_name="场所名称", max_length=100)

    def __unicode__(self):
        return self.name or u""

    class Meta:
        verbose_name = u"推送场所"
        verbose_name_plural = u"推送场所"


class ShareStatistics(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    user_id = models.CharField(verbose_name="用户ID", max_length=100)
    behavior_name = models.CharField(verbose_name="行为名称", max_length=100)

    def __unicode__(self):
        return self.user_id + "_" + self.behavior_name or u""

    class Meta:
        verbose_name = u"分享统计"
        verbose_name_plural = u"分享统计"


class Credit(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    user = models.ForeignKey(User, verbose_name="用户ID")
    source = models.IntegerField(verbose_name="积分来源", default=-1)
    credit = models.IntegerField(verbose_name="积分值")
    continuity = models.IntegerField(verbose_name="连续值", default=1)

    def __unicode__(self):
        return str(self.credit) or u""

    class Meta:
        verbose_name = u"积分"
        verbose_name_plural = u"积分"


class DevicePhone(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    sequence = models.CharField(verbose_name="设备序列号", max_length=128)
    phone_id = models.CharField(verbose_name="手机ID", max_length=50)
    os_version = models.CharField(verbose_name="系统版本", max_length=100, blank=True, default=None)
    phone_number = models.CharField(verbose_name="手机号", max_length=20, blank=True, default=None)
    phone_type = models.CharField(verbose_name="手机型号", max_length=50, blank=True, default=None)

    class Meta:
        unique_together = ("sequence", "phone_id")
        verbose_name = u"设备与手机"
        verbose_name_plural = u"设备与手机"


class ForumLabel(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    name = models.CharField(verbose_name="名称", max_length=50, unique=True)

    def __unicode__(self):
        return str(self.name) or u""

    class Meta:
        verbose_name = u"社区文章标签"
        verbose_name_plural = u"社区文章标签"


class ForumArticle(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    title = models.CharField(verbose_name="标题", max_length=250, default=None)
    author = models.CharField(verbose_name="作者", max_length=100, default=None)
    content = RichTextField(verbose_name="内容")
    image = models.ImageField(verbose_name="标题背景图", upload_to=get_image_upload_to)
    forum_label = models.ForeignKey(ForumLabel, verbose_name="文章标签")
    weight = models.IntegerField(verbose_name="排序权重", default=0, blank=True)

    def __unicode__(self):
        return str(self.content[:10]) or u""

    class Meta:
        verbose_name = u"社区文章"
        verbose_name_plural = u"社区文章"


class UserForumArticle(models.Model):
    created_at = models.DateTimeField(verbose_name="创建时间", auto_now_add=True)
    changed_at = models.DateTimeField(verbose_name="更新时间", auto_now=True)
    user = models.ForeignKey(User, verbose_name="用户")
    article = models.ForeignKey(ForumArticle, verbose_name="文章")
    status = models.IntegerField(verbose_name="状态")  # 创建， 赞， 收藏， 浏览
    count = models.IntegerField(verbose_name="次数", default=1, blank=True)

    def __unicode__(self):
        return str(self.user__username) + "_" + str(self.article__content[:10]) + "_" + str(self.status) or u""

    class Meta:
        unique_together = ("user", "article", "status")
        verbose_name_plural = u"用户与文章"
        verbose_name = u"用户与文章"
