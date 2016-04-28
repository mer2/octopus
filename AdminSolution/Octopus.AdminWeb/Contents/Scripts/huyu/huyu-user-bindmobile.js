define("staticHuyu/huyu-user-bindmobile", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({
		url : "/HuyuUser/LoginNameExists"
	}, params);
	$("#theForm").validate({
		rules: {
			oldMobile: {
				required: true,
				isMobile: true
			},
			mobile: {
				required: true,
				isMobile: true,
				remote: {
					url: params.url,
					type: "POST",
					dataType: "json",
					data: { loginName: function () { return $("#mobile").val(); } }
				}
			}
		},
		messages: {
			oldMobile: {
				defaultMessage: "请补全当前登记的手机号码",
				required: "请补全当前登记的手机号码",
				email: "请输入正确格式的手机号码"
			},
			mobile: {
				defaultMessage: "请输入您常用的手机号码",
				required: "请输入您常用的手机号码",
				email: "请输入正确格式的手机号码",
				remote: "此手机号码已被使用"
			}
		},
		errorElement: "span",
		errorPlacement: function(error, element) {
			element.siblings("p").html(error);
		},
		success: "valid",
		submitHandler: function(form) {
			form.submit();
		},
		showDefaultMessage: function(theItem, defaultMessage) {
			theItem.siblings("p").html('<cite class="prompt">' + defaultMessage + '</cite>');
		}
	});
});