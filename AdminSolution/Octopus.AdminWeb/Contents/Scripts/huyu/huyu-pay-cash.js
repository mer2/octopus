define("staticHuyu/huyu-pay-cash", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var options = require("staticHuyu/huyu-pay-cashbase");

	$(function () {
		var form = $("#theForm");
		form.validate(options.formValidation);
	});
});