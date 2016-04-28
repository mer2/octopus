define("staticHuyu/huyu-user-lostpasswordbymobile", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
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
					data: { exists: 'True', loginName: function () { return $("#mobile").val(); } }
				}
			}
		},
		messages: {
			mobile: {
				defaultMessage: "请输入您的手机号码",
				required: "请输入您的手机号码",
				isMobile: "请输入正确格式的手机号码",
				remote: "此手机号码不存在"
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
			extensionToken: "ResetPassword"
		},
		formValidation: validation,
		loaded: function (result, options) {
			$("#theForm").validate(options.formValidation);
		}
	});
});