define("staticHuyu/huyu-user-resetpasswordbymobile", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var ju = require("staticCommon/joy-utils");
	ju.passwordComplexity("#newPassword", ".level");
	$("#theForm").validate({
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
				defaultMessage: "请输入新密码",
				required: "请输入新密码",
				rangelength: "请保持在6-30个字符内",
				continuousByte: "密码不允许使用连续字符，请更换密码"
			},
			confirmPassword: {
				required: "请输入确认密码",
				equalTo: "2次输入的密码不一致"
			}
		},
		errorElement: "span",
		errorPlacement: function (error, element) {
			element.siblings("p").html(error);
		},
		success: "valid",
		submitHandler: function (form) {
			form.submit();
		},
		showDefaultMessage: function (theItem, defaultMessage) {
			theItem.siblings("p").html('<cite class="prompt">' + defaultMessage + '</cite>');
		}
	});
});