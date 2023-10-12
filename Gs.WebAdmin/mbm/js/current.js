var ua = navigator.userAgent.toLowerCase();
var browser = {
        versions: function() {
            var u = navigator.userAgent,
                app = navigator.appVersion;
            return { //移动终端浏览器版本信息
                trident: u.indexOf('Trident') > -1,
                //IE内核
                presto: u.indexOf('Presto') > -1,
                //opera内核
                webKit: u.indexOf('AppleWebKit') > -1,
                //苹果、谷歌内核
                gecko: u.indexOf('Gecko') > -1 && u.indexOf('KHTML') == -1,
                //火狐内核
                mobile: !!u.match(/AppleWebKit.*Mobile.*/),
                //是否为移动终端
                ios: !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/),
                //ios终端
                android: u.indexOf('Android') > -1 || u.indexOf('Linux') > -1,
                //android终端或uc浏览器
                iPhone: u.indexOf('iPhone') > -1,
                //是否为iPhone或者QQHD浏览器
                iPad: u.indexOf('iPad') > -1,
                //是否iPad
                webApp: u.indexOf('Safari') == -1 //是否web应该程序，没有头部与底部
            };
        }(),
        language: (navigator.browserLanguage || navigator.language).toLowerCase()
    }
    //获取浏览器标识
function systemInfo() {
    if (ua.match(/MicroMessenger/i) == "micromessenger") {
        return "wechat";
    }
    if (ua.match(/WeiBo/i) == "weibo") {
        return "weibo";
    }
    if (ua.match(/QQ/i) == "qq") {
        return "qqzone";
    }
    if (browser.versions.ios) {
        return "apple";
    }
    if (browser.versions.android) {
        return "android";
    }
    return null;
}

function getParams(name) {
    var reg = new RegExp('(^|&)' + name + '=([^&]*)(&|$)', 'i');
    var r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return unescape(r[2]);
    }
    return null;
}

function copyText(text) {
    let textarea = document.createElement("input");
    let scroll_top = $(document).scrollTop();
    document.body.appendChild(textarea);
    textarea.value = text;
    textarea.focus();
    if (textarea.setSelectionRange) {
        textarea.setSelectionRange(0, textarea.value.length);
    } else {
        textarea.select();
    }
    try {
        var flag = document.execCommand("copy");
    } catch (eo) {
        var flag = false;
    }
    document.body.removeChild(textarea); //删除元素
    $(window).scrollTop(scroll_top);
    return flag;
}

//Post请求
function asyncPost(url, body, callback, errback) {
    $.ajax({
        type: "post",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(body),
        async: true,
        success: function(data) {
            callback(data);
            return;
        },
        error: function(jqXHR, textStatus, errorThrown) {
            errback();
            return;
        }
    });
}

function asyncGet(url, callback, errback) {
    $.ajax({
        type: "get",
        url: url,
        async: true,
        success: function(data) {
            callback(data);
            return;
        },
        error: function(jqXHR, textStatus, errorThrown) {
            errback();
            return;
        }
    });
}

//统一保存类
function set_cache(name, value, minute) {
    if (!(document.cookie || navigator.cookieEnabled)) {
        return set_storage(name, value, minute);
    }
    return set_cookie(name, value, minute);
}

//统一获取类
function get_cache(name) {
    if (!(document.cookie || navigator.cookieEnabled)) {
        return get_storage(name);
    }
    return get_cookie(name);
}

//保存Cookie
function set_cookie(name, value, minute) {
    if (minute == null || minute == undefined) {
        minute = 30 * 24 * 60;
    }
    var exp = new Date();
    exp.setMinutes(exp.getMinutes() + minute);
    document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
    return true;
}

function get_cookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
    if (arr = document.cookie.match(reg)) {
        return unescape(arr[2]);
    }
    return null;
}

//保存本地
function set_storage(name, value, minute) {
    var source = [];
    if (minute == null || minute == undefined) {
        minute = 30 * 24 * 60;
    }
    var exp = new Date();
    exp.setMinutes(exp.getMinutes() + minute);
    source[name] = value;
    source["expired"] = exp;
    localStorage.setItem(name, JSON.stringify(source));
    return true;
}

//读取本地
function get_storage(name) {
    var source = [];
    var cache = localStorage.getItem(name);
    if (!cache) {
        return null;
    }
    var now = new Date();
    var source = JSON.parse(cache);
    if (now > source["expired"]) {
        localStorage.removeItem(name);
        return null;
    }
    return source[name];
}