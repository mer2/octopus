(function ($) {
	$.joyadmin = $.extend({}, $.joyadmin);
	$.joyadmin.f_addTab = typeof (f_addTab) == "function" ? f_addTab : function () {
		alert("Not Support!");
	};
	$.joyadmin.f_removeTab = typeof (f_removeTab) == "function" ? f_removeTab : function () {
		alert("Not Support!");
	};
	$.joyadmin.openUrl = function (url, text, tabid) {
		if (window == top) {
			window.open(url, "_self");
		} else {
			try {
				var reg = new RegExp("^(https?://.+?)(/.*)$", "gi");
				if (!reg.test(url)) {//不是绝对地址
					var ch = url.substr(0, 1);
					if (ch != "/") {
						url = "/" + url;
					}
					var href = window.location.href;
					url = href.replace(reg, "$1" + url);
				}
				parent.$.joyadmin.f_addTab(text, url, tabid);
			} catch (e) {
				alert(e.message);
			}
		}
		return false;
	};
	$.joyadmin.closeWindow = function () {
		if (window == top) {
			window.close();
		} else {
			try {
				parent.$.joyadmin.f_removeTab(window.location.href);
			} catch (e) {
				alert(e.message);
			}
		}
		return false;
	};
})(jQuery);