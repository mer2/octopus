define("staticHuyu/huyu-user-updatedemail", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var ju = require("staticCommon/joy-utils");
	ju.passwordComplexity("#newPassword", ".level");
	var security = require("staticCommon/joy-security");
	security.showBroker({
		loaded: function(result, options) {
			$("#theForm").validate($.extend(true, {
				rules: {
					newPassword: {
						required: true,
						rangelength: [6, 30],
						continuousByte: true
					},
					confirmPassword: {
						required: true,
						equalTo: "#newPassword"
					}
				},
				messages: {
					newPassword: {
						defaultMessage: "请保持在6-30个字符内，建议包含英文、数字、符号",
						required: "请输入新密码",
						rangelength: "请保持在6-30个字符内",
						continuousByte: "密码不允许使用连续字符，请更换密码"
					},
					confirmPassword: {
						defaultMessage: "请输入确认密码",
						required: "请重新输入新密码",
						equalTo: "两次密码输入的不相同"
					}
				},
				errorElement: "span",
				errorPlacement: function (error, element) {
					element.next().html(error);
				},
				success: "valid",
				submitHandler: function (form) {
					form.submit();
				},
				showDefaultMessage: function (theItem, defaultMessage) {
					theItem.siblings("p").html('<cite class="prompt">' + defaultMessage + '</cite>');
				}
			}, options.formValidation));
		}
	});
});