define("staticHuyu/huyu-user-updatemobile", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var ju = require("staticCommon/joy-utils");
	ju.passwordComplexity("#newPassword", ".level");
	var security = require("staticCommon/joy-security");
	var globalSettings = require("staticCommon/joy-config");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({
		url: "/HuyuUser/LoginNameExists"
	}, globalSettings.urls, params);
	var validation = {
		rules: {
			mobile: {
				required: true,
				isMobile: true,
				remote: {
					url: params.url,
					type: "POST",
					dataType: "json",
					data: { loginName: function () { return $("#mobile").val(); } }
				}
			},
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
			mobile: {
				defaultMessage: "请输入您常用的手机号码",
				required: "请输入您常用的手机号码",
				isMobile: "请输入正确格式的手机号码",
				remote: "此手机号码已被使用"
			},
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
			element.siblings("p").html(error);
		},
		success: "valid",
		submitHandler: function (form) {
			form.submit();
		},
		showDefaultMessage: function (theItem, defaultMessage) {
			theItem.siblings("p").html('<cite class="prompt">' + defaultMessage + '</cite>');
		}
	};
	security.showBroker({
		mobileClientId: "mobile",
		validatorName: 'ValidateMobile',
		securityUrl: params.securityUrl + "/SecurityService/RequestToken/SecurityMobile",
		data: {
			userData: params.mobile,
			extensionToken: "UpdateMobile"
		},
		formValidation: validation,
		loaded: function (result, options) {
			$("#theForm").validate(options.formValidation);
		}
	});
});