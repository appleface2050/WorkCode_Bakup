# coding=utf-8

from django.views.generic import TemplateView

from django.utils.http import urlencode

class BaseView(TemplateView):
    '''
    后台管理View基类
    by:范俊伟 at:2015-01-21
    前台界面基类，默认不需要登录
    by:王健 at:2015-01-25
    '''

    # 是否需要登录,默认需要登录
    # by:范俊伟 at:2015-01-21
    # 默认不需要登录
    # by:范俊伟 at:2015-01-21
    need_site_permission = False

    # 页面模板
    # by:范俊伟 at:2015-01-21
    template_name = 'webhtml/base.html'

    def get_context_data(self, **kwargs):
        '''
        获取模板所需的变量
        by:范俊伟 at:2015-01-21
        客服系统所需变量
        by: 范俊伟 at:2015-05-21
        '''
        kwargs = super(BaseView, self).get_context_data(**kwargs)
        kwargs['url'] = self.request.get_full_path()
        # kwargs['kf_url'] = settings.NEED_KF_BASE_URL
        kwargs['sessionid'] = self.request.session.session_key
        if hasattr(self, 'form'):
            kwargs['form'] = self.form
        return kwargs

    def get_query_string(self, new_params=None, remove=None):
        '''
        返回当前url的查询参数(query string)
        by:范俊伟 at:2015-01-21
        :param new_params:所要添加的新参数,以dic形式提供
        :param remove:所要去除的字段,以array形式提供
        '''
        if new_params is None:
            new_params = {}
        if remove is None:
            remove = []
        p = dict(self.request.GET.items()).copy()
        for r in remove:
            for k in p.keys():
                if k.startswith(r):
                    del p[k]
        for k, v in new_params.items():
            if v is None:
                if k in p:
                    del p[k]
            else:
                p[k] = v
        qs = urlencode(p)
        if qs:
            return '?%s' % qs
        else:
            return ''

    def isPhoneRequest(self):
        """
        判断是否为手机请求
        by: 范俊伟 at:2015-03-11
        """
        if self.kwargs.get('isPhone'):
            return True
        elif self.kwargs.get('isPC'):
            return False
        elif self.request.browserGroup == 'smart_phone' or self.request.browserGroup == 'feature_phone':
            return True
        else:
            return False

    def isSmartPhone(self):
        """
        判断是否是智能手机
        by: 尚宗凯 at: 2015-03-27
        """
        if self.request.browserGroup == 'smart_phone':
            return True
        else:
            return False

    # @classonlymethod
    # @admin_view_decorator
    def as_view(cls, **initkwargs):
        '''
        创建url文件中所需的view方法
        by:范俊伟 at:2015-01-21
        '''
        return super(BaseView, cls).as_view(**initkwargs)