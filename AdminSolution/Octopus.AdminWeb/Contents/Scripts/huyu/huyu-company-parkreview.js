define("staticHuyu/huyu-company-parkreview", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var tools = require("staticHuyu/huyu-company-tools");
	
	$(function () {
		//  评分效果
		$("#NewComment .sec-body .score-info .hidestar li").mouseover(function () {
			$(this).closest("div.hidestar").prev(".star-level").attr("class", "star-level lv-" + $(this).attr("lang") * 10);
			$(this).closest("div.hidestar").prev(".star-level").find("i").html(tools.ToString($(this).attr("lang"), 1, { 0: "0", 10: "10" }));
		}).mouseout(function () {
			$(this).closest("div.hidestar").prev(".star-level").attr("class", "star-level lv-" + $(this).closest("div.hidestar").find("input").val() * 10);
			$(this).closest("div.hidestar").prev(".star-level").find("i").html(tools.ToString($(this).closest("div.hidestar").find("input").val(), 1, { 0: "0", 10: "10" }));
		}).click(function () {
			$(this).closest("div.hidestar").find("input").val($(this).attr("lang"));
			$(this).closest("div.hidestar").prev(".star-level").find("i").html(tools.ToString($(this).attr("lang"), 1, { 0: "0", 10: "10" }));
		});
		//  提交评论
		$("#PublishComment").click(function () {
			var comment = $("#Comment").val();
			var score12 = $("#NewComment #score12").val();
			var score13 = $("#NewComment #score13").val();
			var score14 = $("#NewComment #score14").val();
			var score15 = $("#NewComment #score15").val();
			var score16 = $("#NewComment #score16").val();
			if (score12 == "0" || score13 == "0" || score14 == "0" || score15 == "0" || score16 == "0") {
			    $.joy.showMsg("您还没有对园区进行评分");
				return false;
			}
			if (!comment.match(/\S+/)) {
				$("#Comment").focus();
				return false;
			}
			$("#form").submit();
			return false;
		});
	});
});