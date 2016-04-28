define("staticHuyu/huyu-company-parkenterprise", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	$(function () {
		$("#EnterpriseList ul li .action a").click(function () {
			$.ajax({
				url: "/Company/Recommend/" + $(this).attr("lang"),
				dataType: "json",
				type: "post",
				success: function (res) {
					if (res && res.ResultNo == 0) {
					    $.joy.showMsg("推荐成功");
					} else {
					    $.joy.showMsg("推荐失败");
					}
				},
				error: function () {
				    $.joy.showMsg("推荐失败,请刷新页面后再试");
				}
			});
		});
	});
});