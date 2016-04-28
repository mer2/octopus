define("staticHuyu/huyu-company-tools", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	exports.ToString = function (num, format, replaceList) {
		if (isNaN(num)) {
			return num;
		}
		var _num = new Number(num);
		var res = _num.toFixed(format);
		if (replaceList) {
			$.each(replaceList, function (i, n) {
				var _temp = new Number(i);
				if (!isNaN(i) && _num.valueOf() == _temp.valueOf()) {
					res = n;
					return;
				}
			});
		}
		return res;
	};
	exports.ScrollTo = function (selector) {
		$("html,body").animate({ scrollTop: $(selector).offset().top });
	};
	exports.DateToString = function (d) {
		if (d) {
			var date = new Date(parseInt(d.replace("/Date(", "").replace(")/", ""), 10));
			var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
			var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
			var hh = date.getHours() < 10 ? "0" + date.getHours() : date.getHours();
			var mm = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
			var ss = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
			return date.getFullYear() + "-" + month + "-" + currentDate + " " + hh + ":" + mm + ":" + ss;
		}
		return "";
	};
	exports.DateToShortString = function (d, splitChar) {
		if (d) {
			var _char = splitChar || '-';
			var date = new Date(parseInt(d.replace("/Date(", "").replace(")/", ""), 10));
			var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
			var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
			return date.getFullYear() + _char + month + _char + currentDate;
		}
		return "";
	};
	exports.Single = function (lst, testfun) {
		var temp = null;
		for (var i = 0; i < lst.length; i++) {
			if (testfun(lst[i])) {
				temp = lst[i];
				break;
			}
		}
		return temp;
	};
	exports.HasAuthentic = function (callback) {
		$.ajax({
			url: "/AjaxJson/IsAuthentic",
			dataType: "json",
			type: "post",
			success: function (res) {
				callback(res.ResultNo == 0);
			},
			error: function () {
				callback(false);
			}
		});
	};
	//  是否为Int
	exports.IsInt = function (val) {
		var reg = new RegExp(/^\s*(0|-{0,1}[1-9]+\d*)\s*$/);
		return reg.test(val);
	};
	//  弹窗
	exports.ShowLayer = function (options) {
	    var layer = require("layer");
		var layerId = options.LayerID;
		var submitSelector = options.SubmitID;
		var submitFun = options.SubmitFun;
		var cancleFun = options.CancleFun;
		if (!layerId) {
			return;
		}
		layer.layer({ layerId: layerId });
		if ($.isFunction(submitFun)) {
			$(submitSelector).off("click").click(function () {
				submitFun();
			});
		}
		if ($.isFunction(cancleFun)) {
			$(".close").off("click").click(function () {
				cancleFun();
			});
		}
	};
	return exports;
});