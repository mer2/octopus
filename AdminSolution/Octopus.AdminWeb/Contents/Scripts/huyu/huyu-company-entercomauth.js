define("staticHuyu/huyu-company-entercomauth", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var ju = require("staticCommon/joy-utils");
	var security = require("staticCommon/joy-security");
	var globalSettings = require("staticCommon/joy-config");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({}, globalSettings.urls, params);
	var upload = require("staticCommon/joy-upload");
	upload.create({
		//upload_url: params.upload_url,
		button_placeholder_id: "swfUploadButton",
		file_types: "*.jpg;*.jpeg;*.png",
		file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
		file_size_limit: "200KB",
		button_text: "",
		button_width: 158,
		button_height: 46,
		button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
		button_cursor: -2, //SWFUpload.CURSOR.HAND
		post_params: {
		    'maxBytes': 1024 * 200
	        , 'imageSize': true
			, 'minWidth': 1
			, 'minHeight': 1
			, limited: true
		},
		upload_success_handler: function (file, serverData) {
			var data = $.parseJSON(serverData);
			if (data.ResultNo == 0) {
				var fi = data.ResultAttachObject;//fi为上传成功后返回的文件信息
				var fileNo = fi.FileNo;//文件编码
				var fileUrl = fi.FileUrl;//文件的完整地址
				$("#LogoReview img").attr("src", fileUrl);
				$("#IdentityData").val(fileNo).focus();
			} else {
				alert("上传失败：" + data.ResultDescription);
			}
		}
	});
	var companyValidateHelper = require("staticHuyu/huyu-company-validate");
	companyValidateHelper.InitCountryArea("Country");
	$.validator.addMethod("HasCompanyAddress", companyValidateHelper.HasCompanyAddress, "0");
	$.validator.setDefaults({ ignore: '' });
	security.showBroker({
		securityUrl: params.securityUrl + "/SecurityService/RequestToken/PassportRegisterByMail",
		mobileClientId: "ContactEmail",
		validatorName: "ValidateMailWithPassword",
		formValidation: {
			rules: {
				Title: {
					required: true,
					rangelength: [2, 50],
					remote: {
						type: "post",
						url: "/AjaxJson/ValidateCompanyTitle",
						data: {
							title: function () {
								return $("#Title").val();
							}
						}
					}
				},
				Country: {
					HasCompanyAddress: true
				},
				Province: {
					HasCompanyAddress: true
				},
				City: {
					HasCompanyAddress: true
				},
				Address: {
					HasCompanyAddress: true
				},
				foreign: {
					HasCompanyAddress: true
				},
				IdentityData: {
					required: true
				},
				ContactName: {
					required: true,
					IsMatch: [/^[a-zA-Z\u2E80-\u9FFF]{2,20}$/]
				},
				ContactMobile: {
					required: true,
					isMobile: true
				},
				ContactEmail: {
					required: true,
					email: true
				},
				Telephone: {
					required: true,
					isTel: true
				}
			},
			messages: {
				Title: {
					required: "请填写您的单位名称",
					rangelength: "请保持在2-50个字符内",
					remote: "该名称已被认证，如对此有疑问，请联系客服"
				},
				IdentityData: {
					required: "请上传营业执照"
				},
				ContactName: {
					required: "请填写联系人名称",
					IsMatch: "请输入2-20个字符，支持中文、英文"
				},
				ContactMobile: {
					required: "请填写业务联系电话",
					isMobile: "请正确填写业务联系电话"
				},
				ContactEmail: {
					required: "请填写联系email",
					email: "请正确填写联系email"
				},
				Telephone: {
					required: "请填写企业总机",
					isTel: "请正确填写企业总机"
				}
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
			    if ($(error).text() == "0") {
			        return;
			    } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
			        $("#" + $(element).attr("name") + "Msg").html(error);
			    } else {
			        $(element).nextAll("p").html(error);
			    }
			},
			success: function (label, element) {
				label.removeClass("error").addClass("valid");
				if (element.name == "ContactEmail") {
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
						title = '<a href="{0}" target="_blank">验证码已发送，请点击这里登录邮箱查看</a>'.format(domain);
					} else {
						title = "<em>验证码已发送，请登录您的邮箱查看</em>".format(title);
					}
					label.html(title);
				}
			},
			submitHandler: function (form) {
				form.submit();
			}
		},
		loaded: function (result, options) {
			$("#form").validate(options.formValidation);
			$(".sms_getter").click(function () {
				$("#ContactEmail").focus();
			});
		}
	});
});