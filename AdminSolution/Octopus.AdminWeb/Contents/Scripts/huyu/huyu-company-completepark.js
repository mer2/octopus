define("staticHuyu/huyu-company-completepark", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var companyValidateHelper = require("staticHuyu/huyu-company-validate");
	companyValidateHelper.InitCountryArea("Country");
	$.validator.setDefaults({ ignore: '' });
	//日历插件
	$(function () {
		$("#FoundationDate").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy-mm-dd",
			changeMonth: true,
			numberOfMonths: 1
		});
		$("#FoundationDate").datepicker($.datepicker.regional["zh-CN"]);
	});
	$("#companyForm").validate({
		rules: {
			Industry: { required: true },
			CompanyType: { required: true },
			Businesses: { required: true },
			RentalPrice: { number: true },
			PropertyExpense: { number: true },
			OccupancyRate: { number: true },
			ParkArea: { number: true },
			Country: { HasCompanyAddress: true },
			Province: { HasCompanyAddress: true },
			City: { HasCompanyAddress: true },
			Address: { HasCompanyAddress: true },
			foreign: { HasCompanyAddress: true }
		},
		messages: {
			CompanyType: { required: "请选择您的园区身份" },
			Industry: { required: "请选择您的园区性质" },
			Businesses: { required: "请选择您的园区级别" }
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