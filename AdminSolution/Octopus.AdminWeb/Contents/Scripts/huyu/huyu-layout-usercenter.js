define("staticHuyu/huyu-layout-usercenter", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var nav = require("staticHuyu/huyu-navigate");
	var $follow = require("staticHuyu/huyu-follow");
	$(function () {
		$follow.FollowInit($(".tofollow"), function ($element) {
			$element.html("已关注").off("click").removeClass("btn-h23").addClass("btn-h23-c");
			$element.attr("data", "1");
		});
	});
	$(".tofollow").click(function () {
		var $this = $(this);
		var userId = $this.attr("lang");
		var data = $this.attr("data");
		if (data == 1) {
			//$follow.CancelFollow(userId, function () {
			//	_ths.addClass("disable").html("已关注").off("click").removeClass("btn-h23").addClass("btn-h23-c");
			//	$this.html("关注").attr("data", "");
			//});
		} else {
			$follow.Follow(userId, function () {
				$this.addClass("disable").html("已关注").off("click").removeClass("btn-h23").addClass("btn-h23-c");
				//$this.html("已关注").attr("data", "1");
			});
		}
	});

	$(function () {
		$(".com-binding").mouseover(function () {
			$(this).find("dd").addClass("cur");
		}).mouseout(function () {
			$(this).find("dd").removeClass("cur");
		});
	});

	//$(function () {
	//	$(".attBtn").click(function () {
	//		var _ths = $(this);
	//		nav.showLogin(function () {
	//			$.ajax({
	//				url: "/AjaxJson/AddFollow.html",
	//				dataType: "json",
	//				type: "post",
	//				data: {
	//					userId: _ths.attr("lang")
	//				},
	//				success: function (res) {
	//					if (res && res.ResultNo == 0) {
	//						_ths.addClass("disable").html("已关注").off("click").removeClass("btn-h23").addClass("btn-h23-c");
	//						showMsg("关注成功");
	//					} else {
	//						showMsg("关注失败");
	//					}
	//				},
	//				error: function () {
	//					showMsg("关注失败");
	//				}
	//			});
	//		});
	//	});
	//});
});