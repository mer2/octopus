define("staticHuyu/huyu-hire", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var tools = require("staticHuyu/huyu-company-tools");
	require("staticHuyu/huyu-company-validate");
	//  取消雇佣
	$("#CancelHire").click(function () {
		var ths = $(this);
		$.joy.showConfirm("确定取消雇佣吗?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/CancleHire.html",
					type: "post",
					dataType: "json",
					data: {
						offerNo: ths.attr("lang"),
						remark: ""
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
			}
		});
	});
	//  追加佣金
	$("#AddMoreProjectCommission-layer input[name='amount']").keyup(function () {
		if (tools.IsInt($(this).val())) {
			$("#AddMoreProjectCommission-layer input[name='total']").val(parseInt($(this).val()) + parseInt($(this).attr("lang")));
		} else {
			$("#AddMoreProjectCommission-layer input[name='total']").val(parseInt($(this).attr("lang")));
		}
	});
	$("#AddCommission").click(function () {
		$.joy.layer({ layerId: "AddMoreProjectCommission-layer", close_layer: "#AddMoreProjectCommission-layer .resetBtn" });
	});
	//  托管佣金
	$("#AddProjectCommission-layer input[name=amount]").keyup(function () {
		var amount = 0;
		if (tools.IsInt($("#AddProjectCommission-layer input[name=amount]").val())) {
			amount = parseInt($("#AddProjectCommission-layer input[name=amount]").val());
		}
		if ($(this).attr("checked")) {
			$("#AddProjectCommission-layer table tr:eq(2) td i").html("￥" + (amount + parseInt($(this).attr("lang"))) + "元");
		} else {
			$("#AddProjectCommission-layer table tr:eq(2) td i").html("￥" + amount + "元");
		}
	});
	$("#AddProjectCommission-layer input:checkbox").change(function () {
		var amount = 0;
		if (tools.IsInt($("#AddProjectCommission-layer input[name=amount]").val())) {
			amount = parseInt($("#AddProjectCommission-layer input[name=amount]").val());
		}
		if ($(this).attr("checked")) {
			$("#AddProjectCommission-layer table tr:eq(2) td i").html("￥" + (amount + parseInt($(this).attr("lang"))) + "元");
		} else {
			$("#AddProjectCommission-layer table tr:eq(2) td i").html("￥" + amount + "元");
		}
	});
	$("#PayCommission").click(function () {
		$.joy.layer({ layerId: "AddProjectCommission-layer", close_layer: "#AddProjectCommission-layer .resetBtn" });
	});
	$("#AddMoreProjectCommissionForm").validate({
		rules: {
			amount: { required: true, IsMatch: [/^\s*[1-9]+\d*\s*$/] }
		},
		messages: {
			amount: {
				required: "请填写托管佣金金额",
				IsMatch: "金额必须为大于0的整数"
			}
		},
		errorElement: "span",
		errorPlacement: function (error, element) {
		    if ($(error).text() == "0") {
		        return;
		    } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
		        $("#" + $(element).attr("name") + "Msg").html(error);
		    } else {
		        $(element).nextAll("p").html(error);
		    }
		},
		success: "valid",
		submitHandler: function (form) {
			form.submit();
		}
	});
	$("#AddProjectCommissionForm").validate({
		rules: {
			amount: { required: true, IsMatch: [/^\s*[1-9]+\d*\s*$/] }
		},
		messages: {
			amount: {
				required: "请填写托管佣金金额",
				IsMatch: "金额必须为大于0的整数"
			}
		},
		errorElement: "span",
		errorPlacement: function (error, element) {
		    if ($(error).text() == "0") {
		        return;
		    } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
		        $("#" + $(element).attr("name") + "Msg").html(error);
		    } else {
		        $(element).nextAll("p").html(error);
		    }
		},
		success: "valid",
		submitHandler: function (form) {
			form.submit();
		}
	});
});

$("#HiredMilestones .table-tr1").has("span[style='color:#c6040d;']").each(function () {
    $(this).find(".icon").addClass("icon-open");
    $(this).find(".icon").removeClass("icon-close");
    $(this).next(".table-tr2").show();
});
$("#HiredMilestones .table-tr2").has("div.change").each(function () {
    var tr1 = $(this).prev(".table-tr1");
    tr1.find(".icon").addClass("icon-open");
    tr1.find(".icon").removeClass("icon-close");
    tr1.next(".table-tr2").show();
});