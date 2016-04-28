define("staticHuyu/huyu-passport-login", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var security = require("staticCommon/joy-security");
	var thisModule = require("staticHuyu/huyu-passport-loginbase");
	var bgs = require("staticHuyu/huyu-passport-loginbg");

	//登录页背景切换效果
	function loadingBg() {
		var total = bgs.length;
		var index = parseInt(Math.random() * 10000) % total;
		var bg = bgs[index];
		$("body").css("background-color", $(bg).css("background-color"));
		$(".login-bg ul.slide").append(bg);
		$(".login-bg ul.slide li").fadeIn(200);
	}

	$(function () {
		$("#long-login").mouseover(function () {//免登录提示
			$(this).addClass("span-hover");
			$(".long-login-prompt").show();
		}).mouseout(function () {
			$(this).removeClass("span-hover");
			$(".long-login-prompt").hide();
		});
		loadingBg();
	});
	security.clientOptions.views["PassportLoginView"] = {
		formHtml: '<input type="hidden" name="securityToken" id="{0}_securityToken" />',
		validationHtml: '<div class="login-code"><label class="tit" for="{0}_securityValue">验证码：</label><div class="txt-input"><input type="text" class="input-2" autocomplete="off" maxlength="4" tabindex="3" id="{0}_securityValue" name="securityValue" /><span id="{0}_captcha_focus"></span></div></div>',
		focusHtml: '<a id="{0}_captcha_update" href="#" title="看不清？换一张" class="blue"><img id="{0}_captcha_image" alt="验证码" class="code-img" />换一张</a>'
	};
	thisModule.securityOptions = $.extend(true, thisModule.securityOptions, {
		formValidation: {
			onclick: false,
			onkeyup: false,
			onfocusout: false,
			checkFirstClick: false
		}
	});
	security.showBroker(thisModule.securityOptions);
	return thisModule;
});