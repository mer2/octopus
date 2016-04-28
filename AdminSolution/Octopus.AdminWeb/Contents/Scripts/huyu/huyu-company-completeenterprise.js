define("staticHuyu/huyu-company-completeenterprise", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var companyValidateHelper = require("staticHuyu/huyu-company-validate");
	companyValidateHelper.InitCountryArea("Country");
	//日历插件
	$(function () {
		$("#FoundationDate").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy-mm-dd",
			changeMonth: true,
			numberOfMonths: 1
		});
		$("#FoundationDate").datepicker($.datepicker.regional["zh-CN"]);
		$("#IPODate").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy-mm-dd",
			changeMonth: true,
			numberOfMonths: 1
		});
		$("#IPODate").datepicker($.datepicker.regional["zh-CN"]);
	});
	$.validator.setDefaults({ ignore: '' });
	$("#companyForm").validate({
		rules: {
			CompanyType: { required: true },
			Businesses: { required: true },
			RegisteredCapital: { number: true },
			Country: { HasCompanyAddress: true },
			Province: { HasCompanyAddress: true },
			City: { HasCompanyAddress: true },
			Address: { HasCompanyAddress: true },
			foreign: { HasCompanyAddress: true }
		},
		messages: {
			CompanyType: { required: "请选择身份类型" },
			Businesses: { required: "请选择业务和类型" }
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