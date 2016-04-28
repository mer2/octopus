define("staticHuyu/huyu-company-systemnotifications", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	$(function () {
		$(".deleteBtn").click(function () {
			var _ths = $(this);
			showConfirm("确认删除当前消息？", {
				okfun: function () {
					$.ajax({
						url: "/Message/DeleteMessage.html",
						dataType: "json",
						data: { id: _ths.attr("lang") },
						type: "post",
						success: function (res) {
							if (res && res.ResultNo == 0) {
								location.reload();
							} else {
								showMsg("删除失败，请重试");
							}
						},
						error: function () {
							showMsg("删除失败，请重试");
						}
					});
				}
			});
		});
	});
});