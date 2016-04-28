define("staticHuyu/huyu-mytrading-follow", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $layer = require("layer");
	var params = require("plugin-params")(module, "huyu");

	$(function () {
		$("#status").val(params.Status);
	}); 
});