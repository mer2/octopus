define("staticHuyu/huyu-pay-query", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var pager = require("staticCommon/joy-pager");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		$(".contbox table tr:even").addClass("even");//隔行颜色不同
		$(".contbox table td .money").each(function () {//金额特效
			var it = $(this);
			var amount = parseFloat(it.text());
			if (amount > 0) {
				it.addClass("income");
				it.text("+" + it.text());
			} else if (amount < 0) {
				it.addClass("expenses");
			}
		});

		$("#theForm select").each(function() {//下拉框赋值
			var it = $(this);
			if (it.attr("lang")) {
				it.val(it.attr("lang"));
			}
		});

		$("#startTime").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy-mm-dd",
			changeMonth: true,
			numberOfMonths: 1,
			onSelect: function (selectedDate) {
				$("#endTime").datepicker("option", "minDate", selectedDate);
			}
		});
		$("#endTime").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy-mm-dd",
			changeMonth: true,
			numberOfMonths: 1,
			onSelect: function (selectedDate) {
				$("#startTime").datepicker("option", "maxDate", selectedDate);
			}
		});

		var opts = $.extend(true, {
			templates: {
				summaryPage: ''
			},
			gotoPage: function (index) {
				$('#startIndex').val(index);
				$('#theForm').submit();
			}
		}, params);
		pager.show(opts);

		$(".download").click(function () {
			$("#download").val(true);
			$('#theForm').submit();
			$("#download").val('');
			return false;
		});
	});
});