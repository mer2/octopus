//互娱基础URL配置
define("staticHuyu/huyu-config", function(require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var settings = require("staticCommon/joy-config");
	var domain = "huyu123.com";
	var theModule = $.extend(true, settings, {
		currentDomain: domain,
		cookieName: '.huyuauth',
		urls: {//站点URL配置
			resourceUrl: 'http://static.shengjoy.dev',
			passportUrl: 'http://passport.' + domain,
			securityUrl: 'http://security.' + domain,
			uploadUrl: 'http://upload.' + domain,
			//uploadUrl: 'http://local.huyu123.com:10223',
			userUrl: 'http://users.' + domain,
			//userUrl: 'http://local.huyu123.com:4595',
			profileUrl: 'http://my.' + domain,
			myUrl: 'http://my.' + domain,
			huyuUrl: 'http://www.' + domain,
			payUrl: 'http://pay.' + domain
		}
	});
	return theModule;
});