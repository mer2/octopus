define("staticHuyu/huyu-passport-registerbymail", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var security = require("staticCommon/joy-security");
	var layer = require("layer");
	var countDown = require("staticHuyu/huyu-shared-tipslayout");
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

	var securityOptions = {
		securityUrl: params.securityUrl + "/SecurityService/RequestToken/PassportRegisterByMail",
		mobileClientId: "userName",
		validatorName: "ValidateMailWithPassword",
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
					defaultMessage: "请输入常用的邮箱地址",
					required: "请输入常用的邮箱地址",
					email: "请输入正确格式的邮箱",
					remote: '该邮箱已被注册，请<a href="/HuyuPassport/Login.html" target="_blank">直接登录</a>'
				},
				password: {
					defaultMessage: "请保持在6-30个字符内，可使用英文数字符号的组合密码",
					required: "请输入密码",
					rangelength: "请保持在6-30个字符内",
					continuousByte: "密码不允许使用连续字符，请更换密码"
				},
				confirmPassword: {
					defaultMessage: "请再次输入密码",
					required: "请再次输入密码",
					equalTo: "两次输入的密码不一致"
				},
				displayName: {
					defaultMessage: "请输入您的昵称，2-10个字",
					required: "请输入您的昵称",
					isDisplayNameWithTrim: "2-10个字，仅中英文/数字/下划线/减号",
					remote: "该昵称已存在，换一个重新注册"
				}
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
				element.siblings("p").html(error);
			},
			success: function (label, element) {
				label.removeClass("error").addClass("valid");
				if (element && element.name == "userName") {
					var $e = $(element);
					var val = $e.val();
					var $v = $("#securityContainer_securityValue");
					var validationCodeSent = $v.data("mobile") && $v.data("mobile") == val;
					if (!validationCodeSent) {
						return;
					}
					var domain = ju.isWellknownEmail(val);
					var title = "";
					if (domain) {
						title = '<a href="{0}" target="_blank">邮件验证码已发送，点击查看验证邮件</a>'.format(domain);
					} else {
						title = "<em>邮件验证码已发送，请登录您的邮箱查看</em>".format(title);
					}
					label.html(title);
				}
			},
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
							countDown.countDown();//倒计时进入首页
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
			$(".sms_getter").click(function () {
				$("#userName").focus();
			});
		}
	};
	security.showBroker(securityOptions);
});