define("staticHuyu/huyu-supply-mysupplies", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	//  推荐
	$("a.recommend").click(function () {
		var id = $(this).attr("lang");
		$.ajax({
			url: "/Supply/Recommend.html",
			dataType: "json",
			type: "post",
			data: {
				id: id
			},
			success: function (res) {
				if (res.ResultNo == 0) {
				    $.joy.showMsg("推荐成功");
				} else {
				    $.joy.showMsg("推荐失败");
				}
			},
			error: function () {
			    $.joy.showMsg("推荐失败");
			}
		});
		return false;
	});
	//  上下架
	$("a.offon").click(function () {
		var ths = $(this);
		var id = $(this).attr("lang");
		var title = $(this).attr("title");
		$.joy.showConfirm("确定要" + title + "服务吗?", {
			okfun: function () {
				$.ajax({
					url: "/Supply/OffOn.html",
					dataType: "json",
					type: "post",
					data: {
						id: id
					},
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg(title + "成功");
							var temp = (title == "上架" ? "下架" : "上架");
							ths.attr("title", temp).html(temp);
							ths.closest("tr").find("td:eq(2)").html(title);
						} else {
						    $.joy.showMsg(title + "失败");
						}
					},
					error: function () {
					    $.joy.showMsg(title + "失败");
					}
				});
			}
		});
		return false;
	});
	//  删除
	$("a.delete").click(function () {
		var id = $(this).attr("lang");
		$.joy.showConfirm("确定要删除服务吗?", {
			okfun: function () {
				$.ajax({
					url: "/Supply/Delete.html",
					dataType: "json",
					type: "post",
					data: {
						id: id
					},
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("删除成功");
							location.reload();
						} else {
						    $.joy.showMsg("删除失败");
						}
					},
					error: function () {
					    $.joy.showMsg("删除失败");
					}
				});
			}
		});
		return false;
	});
});