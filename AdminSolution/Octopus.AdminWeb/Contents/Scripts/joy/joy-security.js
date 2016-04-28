//验证码等安全工具类库
define("staticCommon/joy-security", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var utils = require("staticCommon/joy-utils");
	var $ = require("jquery");
	var globalSettings = require("staticCommon/joy-config");
	var urls = $.extend({ securityUrl: 'http://security.joyyang.com' }, globalSettings.urls);
	var layer = require("layer");
	var md5 = require("md5");
	$.joy = $.extend({}, $.joy);
	var thisModule = $.joy.security = {};
	$.extend(thisModule, {
		notAvailable: -100,
		clientOptions: {
			views: {},
			validators: {},
			containerClientId: 'securityContainer',
			tokenClientId: 'securityToken',
			valueClientId: 'securityValue',
			securityUrl: urls.securityUrl + "/SecurityService/RequestToken/SecurityCaptcha",
			showTips: function (msg) {
				layer.showMsg(msg);
			},
			securityHandler: function (result, options) {
				if (result.ResultNo == 0) {
					var resultObject = result.ResultAttachObject;
					var validationType = resultObject.ValidationData.ValidationType;
					if (!validationType) {
						validationType = resultObject.SecurityLevel;
					}
					if (options.validatorName) {
						validationType = options.validatorName;
					}
					var validator = options.validators[validationType];
					if (validator && $.isFunction(validator.showBroker)) {
						validator.showBroker(result, options);
					}
					var token = resultObject.Token;
					$("#" + options.containerClientId + "_" + options.tokenClientId).val(token);
				} else if (result.ResultNo != thisModule.notAvailable) {
					options.showTips(result.ResultDescription);
				}
				if ($.isFunction(options.loaded)) {
					options.loaded(result, options);
				}
			}
		},
		showBroker: function (options) {
			var opts = $.extend(true, {}, thisModule.clientOptions, options);
			$.extend(true, options, opts);
			$.ajax({
				url: options.securityUrl,
				crossDomain: true,
				dataType: 'jsonp',
				data: options.data,
				success: function (result) {
					options.securityHandler(result, options);
				}
			});
		}
	});
	//captcha
	thisModule.clientOptions.views["ValidateCaptcha"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
		validationHtml: '<div class="input"><label for="{0}_securityValue">验证码：</label><em>*</em><input type="text" id="{0}_securityValue" name="securityValue" class="small" maxlength="4" autocomplete="off" /><span id="{0}_captcha_focus"></span><p></p></div>',
		focusHtml: '<a id="{0}_captcha_update" href="#" title="看不清？换一张"><img id="{0}_captcha_image" alt="" border="0" style="position:absolute;bottom:0px;margin-left:5px" /></a>'
	};
	thisModule.clientOptions.validators["ValidateCaptcha"] = {
		showBroker: function (result, options) {
			options.formValidation = $.extend(true, options.formValidation, {
				rules: {
					securityValue: {
						required: true
					}
				},
				messages: {
					securityValue: {
						defaultMessage: "请输入验证码",
						required: "请输入验证码"
					}
				}
			});
			var resultData = result.ResultAttachObject.ValidationData;
			var viewName = options.viewName ? options.viewName : resultData.ViewName;
			if (!viewName) {
				viewName = "ValidateCaptcha";
			}
			var viewHtmls = thisModule.clientOptions.views[viewName];
			if (!viewHtmls) {
				return;
			}
			var viewHtml, focusHtml = "", formHtml = "";
			if (typeof (viewHtmls) == 'object') {
				viewHtml = viewHtmls.validationHtml;
				focusHtml = viewHtmls.focusHtml;
				formHtml = viewHtmls.formHtml;
			} else {
				viewHtml = viewHtmls;
			}
			var container = $("#" + options.containerClientId);
			var captchaUrl = resultData.CaptchaUrl;
			if (!captchaUrl) {//不需要验证码
				if (formHtml) {
					container.html(formHtml.format(options.containerClientId));
				}
				return;
			}
			if (options.captchaHeight) {
				captchaUrl += "&height=" + options.captchaHeight;
			}
			var prefix = "#" + options.containerClientId + "_";
			container.html((viewHtml + formHtml).format(options.containerClientId)).show();
			if (focusHtml) {
				var showCaptcha = function () {
					$(prefix + "captcha_focus").html(focusHtml.format(options.containerClientId));
					$(prefix + "captcha_image").attr("src", captchaUrl + "&_=" + Math.random());
					$(prefix + "captcha_update").click(function () {
						$(prefix + "captcha_image").attr("src", captchaUrl + "&_=" + Math.random());
						$(prefix + options.valueClientId).focus().val("");
						return false;
					});
				};
				if (options.showImmediately || thisModule.showImmediately) {
					showCaptcha();
				} else {
					thisModule.showImmediately = true;
					container.parents("form:first").one("focus", "input", function () {
						showCaptcha();
					});
				}
			}
		}
	};
	//password
	thisModule.clientOptions.views["ValidatePassword"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" /><input type="hidden" name="hashPassword" id="{0}_hashPassword" />',
		validationHtml: '<div class="input"><label for="{0}_securityValue">验证码：</label><em>*</em><input type="text" class="small" autocomplete="off" maxlength="4" id="{0}_securityValue" name="securityValue" /><span id="{0}_captcha_focus"></span><p></p></div>',
		focusHtml: '<a id="{0}_captcha_update" href="#" title="看不清？换一张"><img id="{0}_captcha_image" alt="" /></a>'
	};
	thisModule.clientOptions.validators.ValidatePassword = {
		showBroker: function (result, options) {
			var opts = $.extend(true, {
				passwordClientId: "password",
				hashPasswordClientId: options.containerClientId + "_hashPassword",
				viewName: "ValidatePassword"
			}, options);
			$.extend(true, options, opts);
			thisModule.clientOptions.validators.ValidateCaptcha.showBroker(result, options);
			thisModule.clientOptions.passwordKey = result.ResultAttachObject.ValidationData.PasswordKey;
			var passwordHandler = function () {
				var txtPassword = $("#" + options.passwordClientId);
				var password = txtPassword.val();
				var hashPassword = password ? md5(md5(password).substr(8, 16) + thisModule.clientOptions.passwordKey) : "";
				$("#" + options.hashPasswordClientId).val(hashPassword);
			};
			if (options.formValidation) {//使用了formValidate
				var validation = options.formValidation;
				var submitHandler0 = validation.submitHandler;
				validation.submitHandler = function (f) {
					passwordHandler();
					if (submitHandler0) {
						submitHandler0(f);
					} else {
						f.submit();
					}
				};
			} else {
				var theForm = $("#" + options.containerClientId).parents("form:first");
				theForm.submit(passwordHandler);
			}
		}
	};
	//mobile
	thisModule.clientOptions.views["ValidateMobile"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
		validationHtml: '<tr><td class="title"><i>*</i><label for="{0}_securityValue">短信验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="off" maxlength="6" tabindex="2" placeholder="输入短信验证码" />&nbsp;<input type="button" class="btns btn-h33-a sms_getter" href="javascript:;" title="免费获取短信验证码" value="免费发送短信" /><p class="noalign"></p></td></tr>'
	};
	thisModule.clientOptions.validators.ValidateMobile = {
		showBroker: function (result, options) {
			var opts = $.extend(true, {
				mobileClientId: "mobile",
				viewName: "ValidateMobile",
				getterTitle: "免费发送短信",
				gettingTitle: "正在发送 ...",
				mobileChangeTitle: "手机号码已改，请重新获取验证码",
				mobileSuccessTitle: "验证码已发出，请注意查收。"
			}, options);
			$.extend(true, options, opts);
			var resultData = result.ResultAttachObject.ValidationData;
			var viewName = options.viewName ? options.viewName : resultData.ViewName;
			if (!viewName) {
				viewName = "ValidateMobile";
			}
			var viewHtmls = thisModule.clientOptions.views[viewName];
			if (!viewHtmls) {
				return;
			}
			var viewHtml, formHtml = "";
			if (typeof (viewHtmls) == 'object') {
				viewHtml = viewHtmls.validationHtml;
				formHtml = viewHtmls.formHtml;
			} else {
				viewHtml = viewHtmls;
			}
			var container = $("#" + options.containerClientId);
			var theForm = $("#" + options.containerClientId).parents("form:first");
			container.html((viewHtml + formHtml).format(options.containerClientId)).show();
			var mobileContainer = $("#" + options.mobileClientId);
			var securityValueContainer = $("#" + options.containerClientId + "_securityValue");
			var smsGetter = container.find(".sms_getter");
			var mobileDuration = 0;
			var timer = function () {
				mobileDuration--;
				//时间到了，或者是否更改了验证对象
				if (mobileDuration <= 0 || (securityValueContainer.data("mobile") && securityValueContainer.data("mobile") != mobileContainer.val())) {
					smsGetter.removeAttr("disabled").removeClass("btn-h33-c").addClass("btn-h33-a").val(opts.getterTitle);
					return;
				}
				smsGetter.val(mobileDuration + " 秒后重发");
				window.setTimeout(timer, 1000);
			};
			var onMobileDuration = function (data) {//倒计时
				if (data && data.MobileDuration) {
					mobileDuration = data.MobileDuration;
					smsGetter.attr("disabled", "disabled").removeClass("btn-h33-a").addClass("btn-h33-c");
					window.setTimeout(timer, 1000);
				}
			};
			onMobileDuration(resultData);
			$.validator.addMethod("mobileValidate", function (value, element) {
				var $e = $(element);
				return this.optional(element) || !$e.data("mobile") || $e.data("mobile") == mobileContainer.val();
			}, opts.mobileChangeTitle);

			function smsGetterOnClick() {
				var validator = theForm.validate();
				if (!validator.element("#" + options.mobileClientId)) {
					mobileContainer.focus();
					return false;
				}
				var mobile = mobileContainer.val();
				var url = resultData.MobileUrl;
				if (url) {
					$.ajax({
						url: url,
						crossDomain: true,
						dataType: 'jsonp',
						data: { mobile: mobile },
						success: function (r) {
							if (r.ResultNo != 0) {
								layer.showMsg(r.ResultDescription, null, r.ResultNo != -202 ? "fail" : "");
								if (r.ResultNo != -202) {
									smsGetter.removeAttr("disabled").removeClass("btn-h33-c").addClass("btn-h33-a").val(opts.getterTitle);
									return;
								} //重发时间还没到
							} else {
								securityValueContainer.data("mobile", mobile); //把手机号码保存起来
								var tips = typeof(opts.mobileSuccessTitle) == 'function' ? opts.mobileSuccessTitle(mobile) : opts.mobileSuccessTitle.format(mobile);
								layer.showMsg(tips, null, "success");
							}
							var attachObject = r.ResultAttachObject;
							if (attachObject && attachObject.ValidationData) {
								onMobileDuration(attachObject.ValidationData); //倒计时
							}
						}
					});
				}
				smsGetter.attr("disabled", "disabled").removeClass("btn-h33-a").addClass("btn-h33-c").val(opts.gettingTitle);
				return false;
			}

			smsGetter.click(function () {//获取验证码
				var validator = theForm.validate();
				if (!validator.element("#" + options.mobileClientId)) { //延时
					mobileContainer.focus();
					setTimeout(smsGetterOnClick, 100);
				} else {
					smsGetterOnClick();
				}
				return false;
			});
			options.formValidation = $.extend(true, options.formValidation, {
				rules: {
					securityValue: {
						required: true,
						mobileValidate: true
					}
				},
				messages: {
					securityValue: {
						defaultMessage: "请输入验证码",
						required: "请输入验证码",
						mobileValidate: opts.mobileChangeTitle
					}
				}
			});
			if (!options.passwordClientId) {//没有带密码的，直接返回
				return;
			}
			thisModule.clientOptions.passwordKey = result.ResultAttachObject.ValidationData.PasswordKey;
			var passwordHandler = function () {
				var txtPassword = $("#" + options.passwordClientId);
				var password = txtPassword.val();
				var hashPassword = password ? md5(md5(password).substr(8, 16) + thisModule.clientOptions.passwordKey) : "";
				$("#" + options.hashPasswordClientId).val(hashPassword);
			};
			var validation = options.formValidation;
			var _submitHandler = validation.submitHandler;
			validation.submitHandler = function (f) {
				passwordHandler();
				if (_submitHandler) {
					_submitHandler(f);
				} else {
					f.submit();
				}
			};
		}
	};
	//mobile with password
	thisModule.clientOptions.views["ValidateMobileWithPassword"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" /><input type="hidden" name="hashPassword" id="{0}_hashPassword" />',
		validationHtml: '<tr><td class="title"><i>*</i><label for="{0}_securityValue">短信验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="off" maxlength="6" tabindex="2" placeholder="输入短信验证码" />&nbsp;<input type="button" class="btns btn-h33-a sms_getter" href="javascript:;" title="免费获取短信验证码" value="免费发送短信" /><p></p></td></tr>'
	};
	thisModule.clientOptions.validators.ValidateMobileWithPassword = {
		showBroker: function (result, options) {
			var opts = $.extend(true, {
				passwordClientId: "password",
				hashPasswordClientId: options.containerClientId + "_hashPassword",
				viewName: "ValidateMobileWithPassword"
			}, options);
			$.extend(true, options, opts);
			thisModule.clientOptions.validators.ValidateMobile.showBroker(result, options);
		}
	};
	//mail
	thisModule.clientOptions.views["ValidateMail"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
		validationHtml: '<tr><td class="title"><i>*</i><label for="{0}_securityValue">邮件验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="off" maxlength="6" tabindex="2" placeholder="输入邮件验证码" />&nbsp;<input type="button" class="btns btn-h33-a sms_getter" href="javascript:;" title="发送验证码到您的邮箱" value="发送验证码" /><p></p></td></tr>'
	};
	thisModule.clientOptions.validators.ValidateMail = {
		showBroker: function (result, options) {
			var opts = $.extend(true, {
				viewName: "ValidateMail",
				getterTitle: "发送验证码",
				mobileChangeTitle: "邮箱地址已改，请重新获取验证码",
				mobileSuccessTitle: function(mobile) {
					var domain = utils.isWellknownEmail(mobile);
					if (domain) {
						return '<a href="{0}" target="_blank">邮件验证码已发送，点击查看验证邮件</a>'.format(domain);
					} else {
						return '验证码已发出，请登录您的邮箱查看';
					}
				}
			}, options);
			$.extend(true, options, opts);
			thisModule.clientOptions.validators.ValidateMobile.showBroker(result, options);
		}
	};
	//mail with password
	thisModule.clientOptions.views["ValidateMailWithPassword"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" /><input type="hidden" name="hashPassword" id="{0}_hashPassword" />',
		validationHtml: '<tr><td class="title"><i>*</i><label for="{0}_securityValue">邮件验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="off" maxlength="6" tabindex="2" placeholder="输入邮件验证码" />&nbsp;<input type="button" class="btns btn-h33-a sms_getter" href="javascript:;" title="发送验证码到您的邮箱" value="发送验证码" /><p></p></td></tr>'
	};
	thisModule.clientOptions.validators.ValidateMailWithPassword = {
		showBroker: function (result, options) {
			var opts = $.extend(true, {
				passwordClientId: "password",
				hashPasswordClientId: options.containerClientId + "_hashPassword",
				viewName: "ValidateMailWithPassword"
			}, options);
			$.extend(true, options, opts);
			thisModule.clientOptions.validators.ValidateMail.showBroker(result, options);
		}
	};
	return thisModule;
});