define("staticHuyu/huyu-mytrading-publishtenderoffer2", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	$(function () { 
		$.post("/AjaxJson/GetTenderOfferProperties", { type: "Require", offerNo: params.OfferNo }, function (data) {
			if (data.ResultNo == 0) {
				var obj = data.ResultAttachObject;
				for (var i = 0; i < obj.length; i++) {
					$("." + obj[i].PropertyTitle).eq(0).find("input").attr("checked", true).attr("disabled", false);
					$("." + obj[i].PropertyTitle).eq(1).addClass("disabled").find("input").attr("checked", false).attr("disabled", true);
				}
			}
		});
		$.post("/AjaxJson/GetTenderOfferProperties", { type: "Priority", offerNo: params.OfferNo }, function (data) {
			if (data.ResultNo == 0) {
				var obj = data.ResultAttachObject;
				for (var i = 0; i < obj.length; i++) {
					if (obj[i].PropertyTitle == 0) {
						$(".other").find(":checkbox").attr("checked", true);
						$(".other").find(":text").val(obj[i].PropertyValue);
					} else {
						$("." + obj[i].PropertyTitle).eq(1).find("input").attr("checked", true).attr("disabled", false);
						$("." + obj[i].PropertyTitle).eq(0).addClass("disabled").find("input").attr("checked", false).attr("disabled", true);
					}
				}
			}
		});

		$("#minBudget-cbx").click(function () {
			if (this.checked) {
				$("#minBudget").attr("data-lastVal", $("#minBudget").val()).val("").attr("disabled", true).removeClass("error");
				$(this).parents("tr").next().find(".title i").html(0);
			} else {
				$("#minBudget").val($("#minBudget").attr("data-lastVal")).attr("disabled", false);
				var commission = CommissionRule($("#minBudget").attr("data-lastVal"));
				$(this).parents("tr").next().find(".title i").html(commission);
			}
		});
		$("#minBudget").focus(function () {
			$(this).removeClass("error").next("p").hide().html("");
		});
		$("#minBudget").blur(function () {
			if ($(this).val() == "" && !$("#minBudget-cbx").attr("checked")) {
				$(this).addClass("error").next("p").html('<span class="error">请输入您的项目预算</span>').show();
			} else {
				var val = parseInt(this.value);
				if (isNaN(val)) {
					this.value = "";
				} else {
					this.value = val;
					var commission = CommissionRule(this.value);
					$(this).parents("tr").next().find(".title i").html(commission);
				}
			}
		});
		$("#minBudget").keyup(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
				var commission = CommissionRule(this.value);
				$(this).parents("tr").next().find(".title i").html(commission);
			}
		});
		$("#expirationTime").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy-mm-dd",
			changeMonth: true,
			numberOfMonths: 1,
			minDate: params.MinDate,
			onSelect: function (selectedDate) {
				var days = getDateDiff(params.DateTimeNow, selectedDate);
				if (days > 0 && days < 32) {
					$("#expirationTime").parents("tr").find(".title i").eq(1).html(days);
				} else {
					$layer.showMsg("招标时间应小于一个月");
					$("#expirationTime").val(document.getElementById("expirationTime").defaultValue);
					days = getDateDiff(params.DateTimeNow, $("#expirationTime").val());
					$("#expirationTime").parents("tr").find(".title i").eq(1).html(days);
				}

			}
		});
		$("#totalCommission").focus(function () {
			$("#minBudget").blur();
			$(this).next("p").hide().html("");
		});
		$("#totalCommission").blur(function () {
			if ($(this).val() == "") {

			} else {
				var val = parseInt(this.value);
				if (isNaN(val)) {
					this.value = "";
				} else {
					this.value = val;
					if (this.value >= ServiceMoney) {
						$(".servicemoney").removeClass("disabled").find("input").attr("checked", false).attr("disabled", false);
					} else {
						$(".servicemoney").addClass("disabled").find("input").attr("checked", false).attr("disabled", true);
					}
					var commission = CommissionRule($("#minBudget").val());
					if (val < commission) {
						this.value = "";
					}
				}
			}
		});
		$("#totalCommission").keyup(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
				if (this.value >= ServiceMoney) {
					$(".servicemoney").removeClass("disabled").find("input").attr("checked", false).attr("disabled", false);
				} else {
					$(".servicemoney").addClass("disabled").find("input").attr("checked", false).attr("disabled", true);
				}
			}
		});
		$("#projectDuration").keyup(function () {
			var val = parseInt($(this).val());
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$(":checkbox").click(function () {
			var val = $(this).val();
			if ($(this).attr("checked")) {
				$("." + val).addClass("disabled").find("input").attr("checked", false).attr("disabled", true);
				$(this).parent().removeClass("disabled").find("input").attr("checked", true).attr("disabled", false);
			} else {
				$("." + val).removeClass("disabled").find("input").attr("checked", false).attr("disabled", false);
			}
		});
		$("#submitBtn").click(function () {
			if ($("#minBudget").val() == "" && !$("#minBudget-cbx").attr("checked")) {
				$("#minBudget").addClass("error").next("p").html('<span class="error">请输入您的项目预算</span>').show();
				return false;
			}
			var totalCommission = parseInt($("#totalCommission").val());
			if (totalCommission > 0) {
				var needMoney = CommissionRule(parseInt($("#minBudget").val()));
				if (totalCommission < needMoney) {
					$layer.showMsg("很抱歉，您托管的金额不符合要求，您至少需托管" + needMoney + "元。！");
					return false;
				}
			}
			var selectedDate = $("#expirationTime").val();
			if (selectedDate == "") {
				$layer.showMsg("请输入招标截至时间");
				return false;
			}
			var days = getDateDiff(params.DateTimeNow, selectedDate);
			if (days < 0 || days > 32) {
				$layer.showMsg("招标时间应小于一个月");
				return false;
			}
			var val = parseInt($("#projectDuration").val());
			if (isNaN(val)) {
				$("#projectDuration").val("");
			} else {
				$("#projectDuration").val(val);
			}
			$("form").submit();
		});
	});

	//乙方保证金
	var ServiceMoney = 10000;

	function getDateDiff(date1, date2) {
		var arr1 = date1.split('-');
		var arr2 = date2.split('-');
		var d1 = new Date(arr1[0], arr1[1], arr1[2]);
		var d2 = new Date(arr2[0], arr2[1], arr2[2]);
		return (d2.getTime() - d1.getTime()) / (1000 * 3600 * 24);
	}


	function CommissionRule(_budget) {
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
		var money = parseInt(_min);
		if (_min > money) {
			_min = money + 1;
		}
		return _min;
	}
});