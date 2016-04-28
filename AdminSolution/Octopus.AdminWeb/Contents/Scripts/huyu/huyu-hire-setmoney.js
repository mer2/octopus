define("staticHuyu/huyu-hire-setmoney", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var tools = require("staticHuyu/huyu-company-tools");
	require("staticHuyu/huyu-company-validate");
	SetMinBudget();
	$("#MinBudget").change(function () {
		SetMinBudget();
	});
	function SetMinBudget() {
		var _budget = $("#MinBudget").val();
		if (!isNaN(_budget) && _budget != 0) {
			var _min = 0;
			if (_budget > 500000) {
				_min = _budget * 0.05;
			} else if (_budget > 100000) {
				_min = _budget * 0.1;
			} else if (_budget > 10000) {
				_min = _budget * 0.3;
			} else if (_budget > 1000) {
				_min = _budget * 0.5;
			} else if (_budget > 500) {
				_min = _budget * 0.8;
			} else if (_budget > 0) {
				_min = _budget;
			}
			$("#minCommissionAmount").html(_min.toString().indexOf(".") > 0 ? _min.toString().substring(0, _min.toString().indexOf(".")) : _min.toString());
		}
	}
	$(".other input[name='OtherRequire']").change(function () {
		var _ths = $(this);
		if ($.trim(_ths.val()) != "") {
			$("#otherrequire").attr("checked", true);
		} else {
			$("#otherrequire").attr("checked", false);
		}
	});
	$("#CommissionAmount").change(function () {
		if (!tools.IsInt($("#CommissionAmount").val())) {
			$("#CommissionAmount").val("");
			return;
		}
		var _commission = parseInt($("#CommissionAmount").val());
		var _minCommission = parseInt($("#minCommissionAmount").html());
		if (_commission != "") {
			if (!isNaN(_commission) && !isNaN(_minCommission) && (_commission < _minCommission)) {
				$("#CommissionAmount").val("");
			}
		}
	});
	function CheckCommissionAmount() {
		var _commission = parseInt($("#CommissionAmount").val());
		var _minCommission = parseInt($("#minCommissionAmount").html());
		if (_commission != "") {
			if (!isNaN(_commission) && !isNaN(_minCommission) && (_commission < _minCommission)) {
				return false;
			}
		}
		return true;
	}
	$.validator.addMethod("CheckCommissionAmount", CheckCommissionAmount, "托管佣金金额不符合要求");
	$("#form").validate({
		rules: {
			MinBudget: { required: true, IsMatch: [/^[1-9]{1}\d*$/] },
			CommissionAmount: { IsMatch: [/^[1-9]{1}\d*$/], CheckCommissionAmount: true }
		},
		messages: {
			MinBudget: {
				required: "请填写您的项目预算",
				IsMatch: "请填写正确的金额，不能包含小数位"
			},
			CommissionAmount: { IsMatch: "请填写正确的金额，不能包含小数位" }
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