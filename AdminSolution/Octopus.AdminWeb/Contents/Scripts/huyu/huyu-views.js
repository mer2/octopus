define("staticHuyu/huyu-views", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");

	$(function () {
		if ($(".huyu-views").length > 0) {
			var str = $(".huyu-views").attr("data-views");
			var json = eval("(" + str + ")");
			SetViewStat(json, null, function (statValue) {
				$(".huyu-views").html(statValue + "人浏览");
			});
		}
	});

	function SetViewStat(options, $element, callback) {
		$.post("/AjaxJson/ViewStat", options, function (data) {
			if (data.ResultNo == 0) {
				if (typeof (callback) == "function") {
					callback(data.StatValue, $element);
				}
			}
		});
	}

	return {
		ViewStat: function (options, callback) {
			SetViewStat(options, null, callback);
		},
		ViewStatByelement: function ($element, callback) {
			var str = $element.attr("data-views");
			var json = eval("(" + str + ")");
			SetViewStat(json, $element, callback);
		}
	};

});