define("staticHuyu/huyu-pay-cashbase", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var layer = require("layer");

	var formValidation = {
		event: "blur",
		onkeyup: false,
		rules: {
			bankName: {
				required: true
			},
			accountName: {
				required: true
			},
			accountNo: {
				required: true,
				newCreditcard: true
			},
			confirmAccountNo: {
				required: true,
				equalTo: "#accountNo"
			},
			branchName: {
				required: true
			},
			amount: {
				required: true,
				isNewAmount: true,
				maxlength: 9
			}
		},
		messages: {
			bankName: {
				defaultMessage: "请填写收款银行",
				required: "请填写收款银行"
			},
			accountName: {
				defaultMessage: "请填写银行开户名",
				required: "请填写银行开户名"
			},
			accountNo: {
				defaultMessage: "请填写银行账号",
				required: "请填写银行账号",
				newCreditcard: "请正确填写16或19位数字的银行账号"
			},
			confirmAccountNo: {
				defaultMessage: "请再次输入银行账号",
				required: "请再次输入银行账号",
				equalTo: "2次输入的银行账号不一致"
			},
			branchName: {
				defaultMessage: "请填写银行分行名称",
				required: "请填写银行分行名称"
			},
			amount: {
				defaultMessage: "请输入提现金额",
				required: "请输入提现金额",
				isNewAmount: "请输入正确的金额",
				maxlength: "请输入正确的金额"
			}
		},
		errorElement: "span",
		errorPlacement: function (error, element) {
			element.siblings("p").html(error);
		},
		success: "valid",
		submitHandler: function (frm) {
			var $bankLocation = $("#bankLocation");
			if ($bankLocation.length > 0) {
				var bankLocation = $("#province").val() + $("#city").val() + $("#area").val();
				if (!bankLocation) {
					layer.showMsg("请选择开户行所在地", 'fail');
					return false;
				}
				$("#bankLocation").val(bankLocation);
			}
			var aa = parseFloat($("#availableAmount").text());
			var ia = parseFloat($("#amount").val());
			if (ia > aa) {
				layer.showMsg("提现金额超出了可提现额度", 'fail');
				return false;
			}
			if (ia <= 1.5 || ia > 20000) {
				layer.showMsg("提现金额需大于 1.50 元且小于等于 20000.00 元", 'fail');
				return false;
			}
			var f = $(frm);
			var sbtn = f.find("[type=submit]").attr("disabled", "disabled").val("正在提交 ...");
			$.ajax({
				type: "POST",
				url: f.attr("action"),
				data: f.serialize(),
				success: function (r) {
					if (r.ResultNo == 302) { //重定向
						window.location.href = r.ResultAttachObject;
					} else if (r.ResultNo == 0) {
						layer.showMsg(r.ResultDescription, function () {
							window.location.href = r.ResultAttachObject;
						}, "success");
					} else {
						layer.showMsg(r.ResultDescription, "fail");
					}
				},
				complete: function () {
					sbtn.removeAttr("disabled").val("立即提现");
				}
			});
			return false;
		}
	};

	$(function () {
		$.validator.addMethod("isNewAmount", function (value, element) {
			var amount = /^(([1-9]\d{0,4})|0)(\.\d{1,2})?$/;
			return this.optional(element) || (parseFloat(value) > 0 && amount.test(value));
		}, "请正确填写金额");

		$.validator.addMethod("newCreditcard", function (value, element) {
			var exp = /^\d{16,19}$/;
			return this.optional(element) || exp.test(value);
		}, "请正确填写16或19位数字的银行账号");

		//计算手续费
		$("#amount").keyup(function () {
			var ia = parseFloat($(this).val());
			var fee = 0.00;
			if (ia <= 1.5) {
			} else if (ia <= 300) {
				fee = 1.50;
			} else if (ia <= 500) {
				fee = 3.00;
			} else if (ia <= 5000) {
				fee = 5.00;
			} else if (ia <= 20000) {
				fee = 10.00;
			}
			var ifee = fee > 0 ? fee : 0.00;
			var icash = fee > 0 ? ia - fee : 0.00;
			$("#feeAmount").text(ifee.toFixed(2));
			$("#cashAmount").text(icash.toFixed(2));
		});
		//未绑定手机提示
		var tips = $(".valid-tips");
		if (!tips.attr("lang")) {
			tips.slideDown().find(".close").click(function () {
				tips.slideUp();
			});
		}
	});

	return { formValidation: formValidation };
});