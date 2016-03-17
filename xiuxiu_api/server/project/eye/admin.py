from django.contrib import admin
from .models import UserExtension, Device, Level, Address, Shop, Company, DetectorRelation
from .models import Publish, Comment, UserShopRelations, RedEnvelopeConstant, ForumPost
from .models import ForumLabel, ForumArticle, UserForumArticle

# class UserExtensionAdmin(admin.ModelAdmin):
#     # fields = ('user', 'nickname', 'gender', 'has_child', 'child_birth', 'phone')
#     readonly_fields = ('account', 'score', 'level', 'collected_shop')
admin.site.register(UserExtension)
# class DeviceAdmin(admin.ModelAdmin):
#     fields = ('name', 'nick', 'sequence')
admin.site.register(Device)
admin.site.register(Level)
admin.site.register(Address)
admin.site.register(Shop)


class PublishAdmin(admin.ModelAdmin):
    fields = ('user', 'big_image', 'content', 'PM2_5', 'created_at')
    readonly_fields = ('created_at',)


admin.site.register(Publish, PublishAdmin)
admin.site.register(Comment)
admin.site.register(UserShopRelations)
admin.site.register(RedEnvelopeConstant)


class ForumPostAdmin(admin.ModelAdmin):
    fields = ['title', 'content', 'category', 'status', 'is_category_rule', 'is_digest', 'is_top', 'top_weight']


admin.site.register(ForumPost, ForumPostAdmin)
admin.site.register(Company)


class DetectorRelationAdmin(admin.ModelAdmin):
    fields = ('mac_address', 'address', 'city', 'threshold',)


admin.site.register(DetectorRelation, DetectorRelationAdmin)
admin.site.register(ForumLabel)
admin.site.register(ForumArticle)
admin.site.register(UserForumArticle)


# Register your models here.
