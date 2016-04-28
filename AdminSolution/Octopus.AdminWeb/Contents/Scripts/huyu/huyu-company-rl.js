define("staticHuyu/huyu-company-rl", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var tools = require("staticHuyu/huyu-company-tools");
	$(".sec-submenu .claim .btns").click(function () {
		tools.ShowLayer({ "LayerID": "claim-layer" });
	});
});