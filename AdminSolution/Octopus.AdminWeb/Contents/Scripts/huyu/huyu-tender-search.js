define("staticHuyu/huyu-tender-search", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		$("#province").val(params.Province);
		$("#province").change(function () {
			var url = "/Tender/Search-" + this.value + "-" + params.SortBy + "-1.html";
			if (params.Title != "") url += "?title=" + params.Title;
			location.href = url;
		});
	});
});