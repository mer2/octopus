define("staticHuyu/huyu-layout-park", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		//  认领
		if (params.ShowRl) {
			var rl = require("staticHuyu/huyu-company-rl");
			rl.Init(params);
		}
		
		$($("#content .sec-nav p").children().toArray().reverse()).each(function () {
			var lang = $(this).attr("lang");
			if (lang && lang != "") {
				$("#submenu ul li").has("a[lang='" + lang + "']").addClass("cur");
				return false;
			}
		});
	});
});