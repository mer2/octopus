define("staticHuyu/huyu-pay-cash2alipay", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var options = require("staticHuyu/huyu-pay-cashbase");

	$(function () {
		var form = $("#theForm");
		options.formValidation = $.extend(true, options.formValidation, {
			rules: {
				accountNo: {
					required: true,
					newCreditcard: false,
					email: true
				}
			},
			messages: {
				accountNo: {
					defaultMessage: "请填写支付宝账号",
					required: "请填写支付宝账号",
					email: "请填写正确的支付宝账号"
				},
				confirmAccountNo: {
					defaultMessage: "请再次输入支付宝账号",
					required: "请再次输入支付宝账号",
					equalTo: "两次输入的支付宝账号不一致"
				}
			}
		});
		form.validate(options.formValidation);
	});
});