define("staticHuyu/huyu-hire-hired", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	var tools = require("staticHuyu/huyu-company-tools");
	var ju = require("staticCommon/joy-upload");
	ju.attachedFiles(".attach-list1"); //显示附件
	//  取消雇佣
	$("#CancelHire").click(function () {
		tools.ShowLayer({
			LayerID: "CancelHire-layer", SubmitID: "#CancelHire-layer :button", CancleFun: "", SubmitFun: function () {
				var remark = $("#CancelHire-layer :selected").val();
				if (remark == "") {
				    $.joy.showMsg("请选择拒绝理由");
					return;
				}
				if (remark == "其他") {
					remark = $("#CancelHire-layer textarea[name='remark']").val();
					if (remark == "") {
					    $.joy.showMsg("请填写拒绝理由");
						return;
					}
				}
				$.ajax({
					url: "/Hire/CancleHire.html",
					type: "post",
					dataType: "json",
					data: {
						offerNo: params.OfferNo,
						remark: remark
					},
					success: function (res) {
						if (res.ResultNo == 0) {
							location.reload();
						} else {
						    $.joy.showMsg("操作失败");
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败");
					}
				});
				$.joy.closeLayer("CancelHire-layer");
			}
		});
		//showConfirm("确定拒绝雇佣吗?", {
		//	okfun: function () {
		//		$.ajax({
		//			url: "/Hire/CancleHire.html",
		//			type: "post",
		//			dataType: "json",
		//			data: {
		//				offerNo: params.OfferNo,
		//				remark: ""
		//			},
		//			success: function (res) {
		//				if (res.ResultNo == 0) {
		//					location.reload();
		//				} else {
		//					showMsg("操作失败");
		//				}
		//			},
		//			error: function () {
		//				showMsg("操作失败");
		//			}
		//		});
		//	}
		//});
	});
	//  有合作意向
	$("#AccepteHire").click(function () {
	    $.joy.showConfirm("确定接受雇佣吗?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/AccepteHire.html",
					type: "post",
					dataType: "json",
					data: {
						offerNo: params.OfferNo
					},
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("操作成功");
							location.reload();
						} else {
						    $.joy.showMsg("操作失败");
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败");
					}
				});
			}
		});
	});
});