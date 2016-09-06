
(function () {
	//alert($(window).height());
    var ParseOrientation = function() {
        /// <summary>解析方向</summary>
        /// <field name="designWidth" type="Number">设计宽度</field>
        /// <field name="htmlWidth" type="Number">网页实际宽度</field>
        /// <field name="htmlHeight" type="Number">网页实际高度</field>
        /// <field name="ratioWidth" type="Number">网页实际宽度同设计宽度比</field>
        /// <field name="isOpenForceWindow" type="Bool">是否开启强制横竖屏</field>
        var html = document.documentElement;
        this.designWidth = 1920;
        this.htmlWidth = html.offsetWidth;
        this.htmlHeight = html.offsetHeight;
        this.ratioWidth = html.clientWidth / this.designWidth;
        this.isOpenForceWindow = false;
        html = null;
    };
    ParseOrientation.prototype.isOrientation = function() {
        /// <summary>是否是竖屏</summary>
        return this.htmlWidth < this.htmlHeight;
    };
    ParseOrientation.prototype.getWindowWidth = function() {
        /// <summary>获取当前窗口宽度</summary>
        var self = this;
        var html = document.documentElement;
        self.htmlWidth = html.clientWidth;
        self.htmlHeight = html.clientHeight;
        if (self.isOrientation() && self.isOpenForceWindow) {
            return self.htmlHeight;
        } else {
            return self.htmlWidth;
        }
    };
    ParseOrientation.prototype.change = function() {
        /// <summary>窗口改变后计算</summary>
        var self = this;
        /// <var type="Element">当前页面</var>
        var html = document.documentElement;
        /// <var type="Number">当前窗口物理分辨率和显示清晰度</var>
        var dpr = Math.round(window.devicePixelRatio || 1);
        /// <var type="Element">获取meta标签</var>
        var metaEl = document.querySelector('meta[name="viewport"]');
        /// <var type="Element">获取style标签</var>
        var fontEl = document.querySelector("#styleHtml");
        if (!metaEl) {
            metaEl = document.createElement("meta");
            metaEl.setAttribute("name", "viewport");
            html.firstElementChild.appendChild(metaEl);
        };
        if (!fontEl) {
            fontEl = document.createElement('style');
            fontEl.id = "styleHtml";
            html.firstElementChild.appendChild(fontEl);
        };
        //正常比例显示页面
        metaEl.setAttribute('content', 'initial-scale=1,maximum-scale=1, minimum-scale=1,user-scalable=no');
        //写入高清倍数
        html.setAttribute('data-dpr', dpr);
        //动态写入样式
        fontEl.innerHTML = 'html{font-size:' + (self.getWindowWidth() / 10) + 'px!important;}';
        //强制横竖屏
        if (self.isOpenForceWindow) {
            if (!self.isOrientation()) {
                //已经是竖屏
                document.body.style.cssText += "width:" + self.htmlWidth + "px;height:" + self.htmlHeight + "px;-webkit-transform: rotate(0);transform: rotate(0);";
            } else {
                //横屏
                var x = -((self.htmlHeight - self.htmlWidth) / 2);
                document.body.style.cssText += "width:" + self.htmlHeight + "px;height:" + self.htmlWidth + "px;-webkit-transform: rotate(-90deg) translate(" + x + "px," + x + "px);transform: rotate(-90deg) translate(" + x + "px," + x + "px);";
            }
        };
        dpr = html = metaEl = self = fontEl = null;
    };
    var windowRatio = new ParseOrientation();
    (function() {
        var handle;
        window.addEventListener("resize", function() {
            clearTimeout(handle);
            handle = setTimeout(function() {
                windowRatio.change();
            });
        }, false);
    })();
    windowRatio.change();
	
/*分割线*/	


})();








































