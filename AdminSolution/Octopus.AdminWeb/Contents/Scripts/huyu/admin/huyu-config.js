//互娱基础后台URL配置
define("staticHuyu/admin/huyu-config", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var settings = require("staticHuyu/huyu-config");
	var mainDomain = "huyu123.com";
	var domain = "admin." + mainDomain;
	var theModule = $.extend(true, settings, {
		mainDomain: mainDomain,
		currentDomain: domain,
		cookieName: '.sjauth',
		urls: {//站点URL配置
			uploadUrl: 'http://upload.' + domain,
			adminUrl: 'http://www.' + domain
		}
	});
	return theModule;
});