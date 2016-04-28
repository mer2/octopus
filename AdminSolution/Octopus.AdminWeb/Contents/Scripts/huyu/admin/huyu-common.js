define("staticHuyu/admin/huyu-common", function(require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var thisModule = {
		openUrl: function(url, text, tabid) {
			if (window == top) {
				window.open(url, "_self");
			} else {
				try {
					var reg = new RegExp("^(https?://.+?)(/.*)$", "gi");
					if (!reg.test(url)) { //不是绝对地址
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
		},
		closeWindow: function() {
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
		}
	};
	thisModule.f_addTab = typeof (f_addTab) == "function" ? f_addTab : function() {
		alert("Not Support!");
	};
	thisModule.f_removeTab = typeof (f_removeTab) == "function" ? f_removeTab : function() {
		alert("Not Support!");
	};
	thisModule = $.joyadmin = $.extend(true, {}, $.joyadmin, thisModule);
	return thisModule;
});