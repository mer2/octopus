//弹层登录
define("staticHuyu/huyu-logindialog", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var jStorage = require("jstorage");
	var security = require("staticCommon/joy-security");
	return function (passportUrl, securityUrl) {
		//partner login
		$(".account-other a").click(function () {
			return $.joy.passport.loginFromPartner(this);
		});
		$("#long-login").mouseover(function () {
			$(this).addClass("span-hover");
			$(".long-login-prompt").show();
		}).mouseout(function () {
			$(this).removeClass("span-hover");
			$(".long-login-prompt").hide();
		});

		var tips = $(".login-tips");
		var showTips = function (msg) {
			if (msg) {
				tips.html('<span class="error">' + msg + '</span>').show();
			} else {
				tips.html("").hide();
			}
		};

		var defaultUserNameValue = "手机/邮箱";
		var ln = $("#loginName");
		ln.focus(function () {
			var $this = $(this);
			$this.removeClass("input-default");
			if ($this.val() == defaultUserNameValue) {
				$this.val("");
			}
		}).blur(function () {
			var $this = $(this);
			if ($this.val() == "") {
				$this.addClass("input-default").val(defaultUserNameValue);
			}
		});
		//默认从localStorage读取记住的帐户名
		var cLogin = jStorage.get("cPassportLoginName");
		if (cLogin) {
			if (cLogin.loginName) {
				ln.val(cLogin.loginName).removeClass("input-default");
				$("#rememberAccount").attr("checked", "checked");
			} else { //用户主动取消记住账号，则保持这个状态
				$("#rememberAccount").removeAttr("checked");
			}
		} else {
			ln.val(defaultUserNameValue);
		}
		var validation = {
			rules: {
				loginName: {
					required: true,
					no_equalTo: "#defaultUserName"
				},
				password: { required: true }
			},
			messages: {
				loginName: {
					required: "请填写登录账号",
					no_equalTo: "请填写登录账号"
				},
				password: { required: "请填写登录密码" }
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
				var $tips = $(".login-tips");
				var firstElement = $tips.data("firstError");
				if (firstElement == $(element).attr("id")) {
					$tips.html(error).show();
				}
			},
			showErrors: function (errorMap, errorList) {
				var $tips = $(".login-tips");
				if (errorList.length > 0) {
					var element = errorList[0].element;
					$tips.data("firstError", element.id);
					$(element).focus();
				} else {
					$tips.data("firstError", null);
					$tips.html("").hide();
				}
				this.defaultShowErrors();
			},
			event: "keyup",
			onclick: false,
			onkeyup: false,
			onfocusout: false,
			checkFirstClick: false,
			submitHandler: function (f) {
				var loginName = $("#loginName").val();
				if (loginName == defaultUserNameValue) {
					$(".login-tips").html('<span class="error">请填写登录账号</span>').show();
					return;
				}
				showTips(); //关闭提示
				$("#loginSubmit").attr("disabled", "disabled").val("正在加载");

				//默认为记住账号，unchecked记录用户是否主动取消记住账号
				var unchecked = $("#rememberAccount").attr("checked") != "checked";
				jStorage.set('cPassportLoginName', { loginName: unchecked ? null : loginName });

				$.ajax({
					url: passportUrl + "/HuyuPassport/AjaxLogin",
					crossDomain: true,
					dataType: "jsonp",
					data: $(f).serialize(),
					success: function (result) {
						if (result.ResultNo != 0) {
							showTips(result.ResultDescription);
							if (result.ResultNo == -200000003) { //验证码错误
								showSecurity();
							}
						} else {
							$.joy.passport.loginSuccess(result.ResultAttachObject);
						}
					},
					complete: function (xhr, status) {
						if (status != 'success') {
							showTips("系统内部错误，请重试！");
						}
						$("#loginSubmit").removeAttr("disabled").val("登 录");
						$("#password").val("");
					}
				});
				return false;
			}
		};

		security.clientOptions.views["PassportLoginDialogView"] = {
			formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
			validationHtml: '<div class="login-code"><label class="tit" for="{0}_securityValue">验证码: </label><div class="txt-input"><input type="text" class="input-2" autocomplete="false" maxlength="4" tabindex="3" id="{0}_securityValue" name="securityValue" /><span id="{0}_captcha_focus"></span></div></div>',
			focusHtml: '<a id="{0}_captcha_update" href="javascript:;" title="看不清？换一张"><img id="{0}_captcha_image" alt="" class="code-img" alt="换一张" />换一张</a>'
		};

		function showSecurity() {
			security.showBroker({
				securityUrl: securityUrl + "/SecurityService/RequestToken/PassportLogin",
				viewName: "PassportLoginDialogView",
				hashPasswordClientId: "password",
				captchaHeight: 26,
				showTips: showTips,
				formValidation: validation,
				loaded: function (r, d) {
					$("#dialogForm").validate(d.formValidation);
				}
			});
		}

		showSecurity();
	};
});