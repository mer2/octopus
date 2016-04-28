define("staticHuyu/huyu-passport-registerbyemail", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var security = require("staticCommon/joy-security");
	var layer = require("layer");
	var ju = require("staticCommon/joy-utils");
	ju.emailComplete("#userName", ".email-tips");
	ju.passwordComplexity("#password", ".level");
	var globalSettings = require("staticCommon/joy-config");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({}, globalSettings.urls, params);

	$.validator.addMethod("isDisplayNameWithTrim", function (value, element) {
		var val = value.replace(/\s/g, "");
		$(element).val(val);
		var tel = /^[\u0391-\uffe5a-zA-Z0-9_\-]{2,10}$/;
		return this.optional(element) || (tel.test(val));
	}, "请正确填写您的姓名");

	security.clientOptions.views["ValidateCaptcha"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" /><input type="hidden" name="hashPassword" id="{0}_hashPassword" />',
		validationHtml: '<tr><td class="title"><i>*</i><label for="{0}_securityValue">验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="false" maxlength="4" tabindex="5" placeholder="点击显示验证码" /><span id="{0}_captcha_focus"></span><p></p></td></tr>',
		focusHtml: '<a id="{0}_captcha_update" href="#" title="看不清？换一张"><img id="{0}_captcha_image" class="code-img" alt="看不清？换一张" border="0" />换一张</a>'
	};
	var securityOptions = {
		securityUrl: params.securityUrl + "/SecurityService/RequestToken/PassportRegister",
		viewName: "ValidateCaptcha",
		formValidation: {
			event: "blur",
			onkeyup: false,
			rules: {
				userName: {
					required: true,
					email: true,
					remote: {
						url: "/HuyuPassport/UserExists",
						type: "POST",
						dataType: "json",
						data: { userName: function () { return $("#userName").val(); } }
					}
				},
				password: {
					required: true,
					rangelength: [6, 30],
					continuousByte: true
				},
				confirmPassword: {
					required: true,
					equalTo: "#password"
				},
				displayName: {
					required: true,
					isDisplayNameWithTrim: true,
					remote: {
						url: "/HuyuPassport/DisplayNameExists",
						type: "POST",
						dataType: "json",
						data: { displayName: function () { return $("#displayName").val(); } }
					}
				}
			},
			messages: {
				userName: {
					defaultMessage: "请输入您常用的邮箱地址",
					required: "请输入您常用的邮箱地址",
					email: "请输入正确格式的邮箱",
					remote: "邮箱已被注册"
				},
				password: {
					defaultMessage: "请保持在6-30个字符内，推荐使用英文加数字或符号的组合密码",
					required: "请输入密码",
					rangelength: "请保持在6-30个字符内",
					continuousByte: "密码不允许使用连续字符，请更换密码"
				},
				confirmPassword: {
					defaultMessage: "请输入确认密码",
					required: "请输入确认密码",
					equalTo: "两次输入的密码不一致"
				},
				displayName: {
					defaultMessage: "请输入您的昵称，2-10个字",
					required: "请输入您的昵称",
					isDisplayNameWithTrim: "2-10个字，仅中英文/数字/下划线/减号",
					remote: "此昵称太受欢迎了，换一个吧"
				}
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
				element.siblings("p").html(error);
			},
			success: "valid",
			submitHandler: function (form) {
				var txtAgreement = $("#agreement");
				if (!txtAgreement.is(":checked")) {
					alert("请阅读并同意《互娱平台用户服务协议》");
					txtAgreement.focus();
					return;
				}
				var btnSubmit = $("#theForm input[type=submit]");
				btnSubmit.attr("disabled", "disabled").val("正在提交");
				var tf = $(form);
				var jumping = false;
				$.ajax({
					url: tf.attr("action"),
					type: tf.attr("method"),
					data: tf.serialize(),
					success: function (data) {
						if (data.ResultNo == 302) { //成功，需要跳转
							jumping = true;
							window.location.href = data.ResultAttachObject;
						} else if (data.ResultNo == 0) {
							$(".ajaxcontent").html(data.ResultView);
						} else {
							var desc = data.ResultDescription;
							layer.showMsg(desc, null, 'fail');
							security.showBroker($.extend(true, securityOptions, {
								loaded: function () {
									if (desc.indexOf("密码错误") >= 0) {
										$("#password").focus();
									} else if (desc.indexOf("验证码") >= 0) {
										$("#securityContainer_securityValue").focus();
									}
								}
							}));
						}
					},
					complete: function () {
						if (!jumping) {
							btnSubmit.removeAttr("disabled").val("同意条款并注册");
						}
					}
				});
				//form.submit();
			},
			showDefaultMessage: function (theItem, defaultMessage) {
				theItem.siblings("p").html('<cite class="prompt">' + defaultMessage + '</cite>');
			}			
		},
		loaded: function(result, options) {
			$("#theForm").validate(options.formValidation);
		}
	};
	security.showBroker(securityOptions);
});