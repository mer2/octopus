define("staticHuyu/huyu-pay-prepaid", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var security = require("staticCommon/joy-security");
	var layer = require("layer");
	var globalSettings = require("staticCommon/joy-config");
	var params = require("plugin-params")(module, "huyu");
	$(function() {
		var $mobile = $("#mobile");
		if (!$mobile.attr("lang")) { //未绑定手机
			layer.layer({ layerId: "pay-action" });
			return;
		}
		params = $.extend({}, globalSettings.urls, params);
		var validation = {
			/*rules: {
				mobile: {
					required: true,
					isMobile: true
				}
			},
			messages: {
				mobile: {
					defaultMessage: "请输入您常用的手机号码",
					required: "请输入您常用的手机号码",
					isMobile: "请输入正确格式的手机号码"
				}
			},*/
			errorElement: "span",
			errorPlacement: function (error, element) {
				element.siblings("p").html(error);
			},
			success: "valid",
			submitHandler: function (form) {
				var f = $(form);
				var sbtn = f.find("[type=submit]").attr("disabled", "disabled").val("正在提交 ...");
				$.ajax({
					type: "POST",
					url: f.attr("action"),
					data: f.serialize(),
					success: function(r) {
						if (r.ResultNo == 302) { //重定向
							window.location.href = r.ResultAttachObject;
						} else if (r.ResultNo != 0) {
							layer.showMsg(r.ResultDescription);
						} else {
							//弹提示层
							layer.showMsg("付款成功", "success");
						}
					},
					complete: function () {
						sbtn.removeAttr("disabled").val("确认付款");
					}
				});
				return false;
			},
			showDefaultMessage: function (theItem, defaultMessage) {
				theItem.siblings("p").html('<cite class="prompt">' + defaultMessage + '</cite>');
			}
		};
		//mobile
		security.clientOptions.views["ValidateMobile"] = {
			formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
			validationHtml: '<dt><label for="{0}_securityValue">短信验证码：</label></dt><dd><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="off" maxlength="6" tabindex="2" placeholder="输入短信验证码" />&nbsp;<input type="button" class="btns btn-h33-a sms_getter" href="javascript:;" title="免费获取短信验证码" value="免费发送短信" /><p></p></dd>'
		};
		security.showBroker({
			validatorName: 'ValidateMobile',
			securityUrl: params.securityUrl + "/SecurityService/RequestToken/SecurityMobile",
			data: {
				userData: params.mobile,
				extensionToken: "PrePaid"
			},
			formValidation: validation,
			loaded: function (result, options) {
				$("#theForm").validate(options.formValidation);
			}
		});
	});
});