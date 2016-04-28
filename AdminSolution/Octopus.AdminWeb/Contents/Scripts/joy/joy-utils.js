define("staticCommon/joy-utils", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	//字符串格式化
	String.prototype.format = function () {
		var args = arguments;
		return this.replace(/\{(\d+)\}/g,
				function (m, i) {
					return args[i];
				});
	};
	//去除尾部指定字符
	String.prototype.trimEnd = function(trimChar) {
		var str = this, i = str.length - 1;
		for (; i >= 0; i--) {
			if (str.charAt(i) != trimChar) {
				break;
			}
		}
		if (i >= 0 && i < (str.length - 1)) {
			return str.substring(0, i + 1);
		}
		return str;
	};
	var $ = require("jquery");
	var emailDomains = {
		'qq.com': 'http://mail.qq.com',
		'vip.qq.com': 'http://mail.qq.com',
		'163.com': 'http://mail.163.com',
		'yeah.net': 'http://mail.yeah.net',
		'126.com': 'http://mail.126.com',
		'sina.com': 'http://mail.sina.com',
		'sohu.com': 'http://mail.sohu.com',
		'21cn.com': 'http://mail.21cn.com',
		'gmail.com': 'http://www.gmail.com',
		'hotmail.com': 'http://www.hotmail.com',
		'outlook.com': 'http://www.outlook.com'
	};
	var theModule = {
		getEmailDomain: function (email) {
			var li = email.lastIndexOf("@");
			var domain = email.substr(li + 1).toLowerCase();
			return domain;
		},
		isWellknownEmail: function (email) {
			var domain = theModule.getEmailDomain(email);
			return emailDomains[domain];
		},
		emailComplete: function (eInput, container) {//邮件地址补齐提示
			eInput = $(eInput);
			container = $(container);
			var conHtml = '';
			$.map(emailDomains, function(value, domain) {
				conHtml += '<a href="javascript:;" lang="@' + domain + '" data-title="' + value + '"><span></span>@' + domain + '</a>';
			});
			container.html(conHtml);
			var alist = container.find("a");
			var currentIndex = null;
			function hideContainer(focus) {
				alist.removeClass("cur");
				currentIndex = null;
				container.hide();
				if (focus) {
					eInput.focus();
				}
			}
			function showContainer(currentKey) {
				var val = eInput.val();
				if (val) {
					var atindex = val.indexOf('@');
					var islast = atindex == (val.length - 1);
					if (atindex >= 0 && !islast) {
						hideContainer();
					} else {
						if (currentKey == 13) { //回车键
							if (currentIndex != null) { //已手动选择
								var domain = $(alist[currentIndex]).attr("lang");
								eInput.val(val.trimEnd("@") + domain);
								hideContainer();
							}
						} else {
							if (islast) {
								val = val.substring(0, val.length - 1);
							}
							container.find("a span").html(val);
							container.show();
						}
					}
				} else {
					hideContainer();
				}
			}
			alist.click(function () {
				var $a = $(this);
				var val = eInput.val();
				var domain = $a.attr("lang");
				eInput.val(val.trimEnd("@") + domain);
				hideContainer(true);
			});
			eInput.keydown(function (e) {
				e = e || event;
				var currentKey = e.keyCode || e.which || e.charCode;
				if (currentKey == 38) {//向上键
					if (currentIndex == null || currentIndex == 0) { //未选择或是第一个，则选中最后一个
						currentIndex = alist.length - 1;
					} else {
						currentIndex--;
					}
					alist.removeClass("cur");
					$(alist[currentIndex]).addClass("cur");
					return false;
				} else if (currentKey == 40) { //向下键
					if (currentIndex == null || currentIndex == (alist.length - 1)) { //未选择或是最后一个，则选中第一个
						currentIndex = 0;
					} else {
						currentIndex++;
					}
					alist.removeClass("cur");
					$(alist[currentIndex]).addClass("cur");
					return false;
				} else if (currentKey == 13) { //回车键
					if (currentIndex != null) { //已手动选择
						return false;
					}
				}
			}).keyup(function (e) {
				e = e || event;
				var currentKey = e.keyCode || e.which || e.charCode;
				showContainer(currentKey);
			}).blur(function () {
				setTimeout(hideContainer, 200);
			}).focus(function() {
				showContainer();
			});
		},
		passwordComplexity: function (eInput, container) {//密码强度判断
			eInput = $(eInput);
			container = $(container);
			eInput.keyup(function () {
				var val = eInput.val();
				container.removeClass("level-1 level-2 level-3");
				if (!val) {//没输入密码
					return;
				}
				var level = 0;
				/\d+/.test(val) && level++;
				/[a-zA-Z]+/.test(val) && level++;
				/[^a-zA-Z\d]+/.test(val) && level++;
				if (level) {
					container.addClass("level-" + level);
				}
			});
		},
		queryString: function (name, url) {//获取url请求的kv值
			if (!url) {
				url = window.location.href;
			}
			var svalue = url.match(new RegExp("[\?\&]" + name + "=([^\&]*)(\&?)", "i"));
			return svalue ? decodeURIComponent(svalue[1]) : svalue;
		},
		addQueryString: function (name, value, url) {//添加参数到url中
			if (!url) {
				url = window.location.href;
			}
			url += url.indexOf("?") == -1 ? "?" : "&";
			url += encodeURIComponent(name) + "=" + encodeURIComponent(value);
			return url;
		},
		removeQueryString: function (name, url) {//删除参数到url中
			if (!url) {
				url = window.location.href;
			}
			var reg = new RegExp("[\?\&]" + name + "=([^\&]*)(\&?)", "i");
			return url.replace(reg, "");
		}
	};
	return theModule;
});