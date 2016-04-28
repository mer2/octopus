define("staticHuyu/huyu-passport-loginbind", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var security = require("staticCommon/joy-security");
	var thisModule = require("staticHuyu/huyu-passport-loginbase");
	var layer = require("layer");

	security.clientOptions.views["PassportLoginView"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" /><input type="hidden" name="hashPassword" id="{0}_hashPassword" />',
		validationHtml: '<tr><td class="title"><label for="{0}_securityValue">验证码：</label></td><td class="captcha"><input id="{0}_securityValue" name="securityValue" type="text" autocomplete="off" maxlength="4" tabindex="3" placeholder="点击显示验证码" /><span id="{0}_captcha_focus"></span><p></p></td></tr>',
		focusHtml: '<a id="{0}_captcha_update" href="#" title="看不清？换一张"><img id="{0}_captcha_image" class="code-img" alt="看不清？换一张" border="0" />换一张</a>'
	};
	thisModule.showTips = function (msg) {
		layer.showMsg(msg, null, 'fail');
	};
	thisModule.securityOptions = $.extend(true, thisModule.securityOptions, {
		formValidation: {
			errorPlacement: function (error, element) {
				element.siblings("p").html(error);
			},
			showErrors: null,
			event: "blur",
			success: "valid"
		}
	});
	var successHandler0 = thisModule.successHandler;
	thisModule.successHandler = function(data) {
		if (data.ResultNo == 302 || data.ResultNo == 0) {
			layer.showMsg("绑定成功", function () {
				window.location.href = data.ResultAttachObject;
			}, 'success');
		} else if (data.ResultNo == -199) { //绑定失败，但登录成功
			$("#bind-action span.alertspan").html("账号绑定失败：" + data.ResultDescription);
			$("#bind-action a.cancel").attr("href", data.ResultAttachObject);
			layer.layer({ layerId: "bind-action" });
		} else {
			successHandler0(data);
		}
	};
	security.showBroker(thisModule.securityOptions);
	return thisModule;
});