define("staticHuyu/huyu-pay-invoice", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var layer = require("layer");

	$(function () {
		$.validator.addMethod("isNewAmount", function (value, element) {
			var amount = /^(([1-9]\d{0,4})|0)(\.\d{2})?$/;
			return this.optional(element) || (parseFloat(value) > 0 && amount.test(value));
		}, "请正确填写金额");

		var form = $("#theForm");
		form.validate({
			event: "blur",
			onkeyup: false,
			rules: {
				invoiceType: {
					required: true
				},
				invoiceTitleType: {
					required: true
				},
				invoiceTitle: {
					required: true
				},
				invoiceDescription: {
					required: true
				},
				invoiceAmount: {
					required: true,
					isNewAmount: true,
					maxlength: 9,
					min: 100.00
				},
				realName: {
					required: true
				},
				contactPhone: {
					required: true,
					isPhone: true,
				},
				addressDetail: {
					required: true
				},
				zipCode: {
					required: true
				},
				logisticsType: {
					required: true
				}
			},
			messages: {
				invoiceType: {
					defaultMessage: "请选择发票类型",
					required: "请选择发票类型"
				},
				invoiceTitleType: {
					defaultMessage: "请选择发票抬头类型",
					required: "请选择发票抬头类型"
				},
				invoiceTitle: {
					defaultMessage: "请填写发票抬头",
					required: "请填写发票抬头"
				},
				invoiceDescription: {
					defaultMessage: "请选择发票内容",
					required: "请选择发票内容"
				},
				invoiceAmount: {
					defaultMessage: "请输入发票金额",
					required: "请输入发票金额",
					isNewAmount: "请输入正确的金额",
					maxlength: "请输入正确的金额",
					min: "单次发票金额不能低于100.00元"
				},
				realName: {
					defaultMessage: "请填写收票人姓名",
					required: "请填写收票人姓名"
				},
				contactPhone: {
					defaultMessage: "请填写收票人联系电话",
					required: "请填写收票人联系电话",
					isPhone: "请填写正确的联系电话"
				},
				addressDetail: {
					defaultMessage: "请填写收票人详细地址",
					required: "请填写收票人详细地址"
				},
				zipCode: {
					defaultMessage: "请填写收票人邮政编码",
					required: "请填写收票人邮政编码"
				},
				logisticsType: {
					defaultMessage: "请选择寄送方式",
					required: "请选择寄送方式"
				}
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
				element.siblings("p").html(error);
			},
			success: "valid",
			submitHandler: function (frm) {
				var address = $("#country").val() + $("#province").val() + $("#city").val() + $("#area").val();
				if (!address) {
					layer.showMsg("请选择收票人地址", 'fail');
					return false;
				}
				$("#address").val(address);
				var aa = parseFloat($("#availableAmount").text());
				var ia = parseFloat($("#invoiceAmount").val());
				if (ia > aa) {
					layer.showMsg("申请金额超出了可开票额度", 'fail');
					return false;
				}
				var f = $(frm);
				var sbtn = f.find("[type=submit]").attr("disabled", "disabled").val("正在提交 ...");
				$.ajax({
					type: "POST",
					url: f.attr("action"),
					data: f.serialize(),
					success: function(r) {
						if (r.ResultNo == 302) { //重定向
							window.location.href = r.ResultAttachObject;
						} else if (r.ResultNo == 0) {
							if (r.ResultAttachObject) {
								//运费免费就不要提示支付了
								if (parseInt($("#logisticsType option:selected").attr("lang")) > 0) {
									$("#pay-action .payInvoice").attr("href", r.ResultAttachObject).show();
								}
							}
							layer.layer({ layerId: "pay-action" });
						} else {
							layer.showMsg(r.ResultDescription, "fail");
						}
					},
					complete: function () {
						sbtn.removeAttr("disabled").val("立即申请");
					}
				});
				return false;
			}
		});
	});
});