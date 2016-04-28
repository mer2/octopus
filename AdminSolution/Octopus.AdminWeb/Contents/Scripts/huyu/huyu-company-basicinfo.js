define("staticHuyu/huyu-company-basicinfo", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var companyValidateHelper = require("staticHuyu/huyu-company-validate");
	companyValidateHelper.InitCountryArea("Country");
	$.validator.addMethod("HasCompanyAddress", companyValidateHelper.HasCompanyAddress, "0");
	$.validator.setDefaults({ ignore: '' });
	$("#companyForm").validate({
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
			ContactName: {
				required: true,
				IsMatch: [/^[a-zA-Z\u2E80-\u9FFF]{2,20}$/]
			},
			Mobile: {
			    optionalRequired: { "group": "phone" },
				isMobile: true
			},
			ContactPhone: {
			    optionalRequired: {
			        "group": "phone", callback: function () {
			            return $("#countryCode").val() != "" && $("#areaCode").val() != "" && $("#phoneNumber").val() != "";
			        }
			    },
				isCompanyPhone: true
			}
		},
		messages: {
			Title: {
				required: "填写您的公司名称",
				rangelength: "公司名称应在2-50个字符内",
				remote: "该名称已被认证，如对此有疑问，请联系客服"
			},
			ContactName: {
				required: "请填写联系人名称",
				IsMatch: "请输入2-20个字符，支持中文、英文"
			},
			Mobile: {
			    optionalRequired: "手机和电话至少填写一项",
				isMobile: "请输入正确的手机号码"
			},
			ContactPhone: {
			    optionalRequired: "手机和电话至少填写一项"
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
		success: "valid",
		submitHandler: function (form) {
			form.submit();
		}
	});
});