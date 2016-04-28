define("staticHuyu/huyu-passport-loginbase", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var jStorage = require("jstorage");
	var security = require("staticCommon/joy-security");
	var globalSettings = require("staticCommon/joy-config");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({}, globalSettings.urls, params);

	var thisModule = {
		defaultUserName: "手机/邮箱",
		showTips: function (msg) {
			var tips = $(".login-tips");
			if (msg) {
				tips.html('<span class="error">' + msg + '</span>').show();
			} else {
				tips.html("").hide();
			}
		},
		successHandler: function(data) {
			if (data.ResultNo == 302) { //登录成功，需要跳转
				window.location.href = data.ResultAttachObject;
			} else {
				var desc = data.ResultDescription;
				thisModule.showTips(desc);
				security.showBroker($.extend(true, thisModule.securityOptions, {
					loaded: function () {
						if (desc.indexOf("密码错误") >= 0) {
							$("#password").focus();
						} else if (desc.indexOf("验证码") >= 0) {
							$("#securityContainer_securityValue").focus();
						}
					}
				}));
			}
		}
	};

	$(function () {
		var ln = $("#loginName").focus(function () {
			$(this).removeClass("input-default");
			if ($(this).val() == thisModule.defaultUserName) {
				$(this).val("");
			}
		}).blur(function () {
			if ($(this).val() == "") {
				$(this).addClass("input-default").val(thisModule.defaultUserName);
			}
		});
		//默认从localStorage读取记住的帐户名
		var cLogin = jStorage.get("cPassportLoginName");
		if (cLogin) {
			if (cLogin.loginName) {
				ln.val(cLogin.loginName);
				$("#rememberAccount").attr("checked", "checked");
			} else { //用户主动取消记住账号，则保持这个状态
				$("#rememberAccount").removeAttr("checked");
			}
		} else {
			ln.val(thisModule.defaultUserName);
		}
		if (ln.val()) {
			ln.removeClass("input-default");
		} else {
			ln.val(thisModule.defaultUserName);
		}
		if (params.errMsg) {
			thisModule.showTips(params.errMsg);
		}
	});
	thisModule.securityOptions = {
		securityUrl: params.securityUrl + "/SecurityService/RequestToken/PassportLogin",
		viewName: "PassportLoginView",
		hashPasswordClientId: "password_hashPassword",
		showTips: thisModule.showTips,
		formValidation: {
			rules: {
				loginName: {
					required: true,
					no_equalTo: "#defaultUserName"
				},
				passwordInput: { required: true }
			},
			messages: {
				loginName: {
					required: "请填写登录账号",
					no_equalTo: "请填写登录账号"
				},
				passwordInput: { required: "请填写登录密码" }
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
			submitHandler: function (f) {
				var loginName = $("#loginName").val();
				if (loginName == thisModule.defaultUserName) {
					thisModule.showTips("请填写登录账号");
					return;
				}
				$("#loginSubmit").attr("disabled", "disabled").val("正在登录");
				//默认为记住账号，unchecked记录用户是否主动取消记住账号
				var unchecked = $("#rememberAccount").attr("checked") != "checked";
				jStorage.set('cPassportLoginName', { loginName: unchecked ? null : loginName });
				//使用AJAX提交
				var tf = $(f);
				//剔除原始密码
				$("#password").removeAttr("name");
				var sdata = tf.serialize();
				$("#password").attr("name", "passwordInput");
				var jumping = false;
				$.ajax({
					url: tf.attr("action"),
					type: tf.attr("method"),
					data: sdata,
					success: function (data) {
						jumping = data.ResultNo == 302;
						thisModule.successHandler(data);
					},
					complete: function () {
						if (!jumping) {
							$("#loginSubmit").removeAttr("disabled").val("登 录");
						}
					}
				});
				//f.submit();
			},
			showDefaultMessage: function () {
			}
		},
		loaded: function (result, options) {
			$("#theForm").validate(options.formValidation);
		}
	};
	return thisModule;
});