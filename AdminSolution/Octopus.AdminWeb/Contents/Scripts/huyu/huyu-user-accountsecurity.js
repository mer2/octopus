define("staticHuyu/huyu-user-accountsecurity", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	$(function () {
		var sl = $(".state-level");
		var level = sl.attr("lang");
		if (level) {
			var ll = parseInt(level);
			sl.addClass("level" + (ll + 1));
			var title;
			if (level <= 0) {
				title = "低";
			} else if (level == 1) {
				title = "中";
			} else {
				title = "高";
			}
			sl.find("em").html(title);
		}
	});
});