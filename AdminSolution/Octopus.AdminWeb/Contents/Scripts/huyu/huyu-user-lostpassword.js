define("staticHuyu/huyu-user-lostpassword", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({
		url: "/HuyuUser/LoginNameExists"
	}, params);
	var security = require("staticCommon/joy-security");
	security.clientOptions.views["ValidateCaptcha"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
		validationHtml: '<tr><td class="title"><i>*</i><label for="{0}_securityValue">验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="false" maxlength="4" placeholder="点击输入验证码" /><span id="{0}_captcha_focus"></span><p></p></td></tr>',
		focusHtml: '<a id="{0}_captcha_update" href="#" title="看不清？换一张"><img id="{0}_captcha_image" class="code-img" alt="看不清？换一张" border="0" />换一张</a>'
	};
	security.showBroker({
		loaded: function(result, options) {
			$("#theForm").validate($.extend(true, {
				rules: {
					email: {
						required: true,
						email: true,
						remote: {
							url: params.url,
							type: "POST",
							dataType: "json",
							data: { exists: 'True', loginName: function () { return $("#email").val(); } }
						}
					}
				},
				messages: {
					email: {
						defaultMessage: "请输入您注册时填写的邮箱地址",
						required: "请输入您注册时填写的邮箱地址",
						email: "请输入正确格式的邮箱",
						remote: "此邮箱地址不存在"
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
			}, options.formValidation));
		}
	});
});