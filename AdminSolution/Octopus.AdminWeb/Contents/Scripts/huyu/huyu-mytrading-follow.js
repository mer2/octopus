define("staticHuyu/huyu-mytrading-follow", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var layer = require("layer"); 
	$(function () {
		$(".cancelfollow").click(function () {
			var $li = $(this).parents("li");
			$.post("/AjaxJson/CancelFollow/" + $(this).attr("lang") + ".html", function (data) {
				if (data.ResultNo == 0) {
					$li.remove();
				} else {
					layer.showMsg("取消关注失败！");
				}
			});
		});

		$(".follow").click(function () {
			var $div = $(this).parent();
			$.post("/AjaxJson/Follow/" + $(this).attr("lang") + ".html", function (data) {
				if (data.ResultNo == 0) {
					$div.html('<span class="icon icon2">已互听</span>');
				} else {
					layer.showMsg("关注失败！");
				}
			});
		});
	}); 
});