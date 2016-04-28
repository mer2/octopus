define("staticHuyu/huyu-user-utils", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	return {
		showTips: function (msg) {
			if (msg) {
				$(".msgTips").html(msg).parents("tbody").first().show();
			} else {
				$(".msgTips").html(msg).parents("tbody").first().hide();
			}
		}
	};
});