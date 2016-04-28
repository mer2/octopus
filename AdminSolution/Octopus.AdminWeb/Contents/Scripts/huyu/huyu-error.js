//错误提示
define("staticHuyu/huyu-error", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	//尝试从约定的地方读取错误信息，然后显示
	var $ = require("jquery");
	var layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	if (params) {
		$(function() {
			var errMsg = [];
			if ((params.ResultNo == 0 || params.ResultNo == '0') && params.ResultDescription) { //成功的消息
				layer.showMsg(params.ResultDescription, null, 'success');
			} else {
				if (params.ResultDescription) {
					errMsg.push(params.ResultDescription);
				}
				$.map(params, function (value, key) {
					if (value == true) {
						errMsg.push(key);
					}
				});
				if (errMsg.length > 0) {
					layer.showMsg(errMsg.join("<br />"), null, 'fail');
				}
			}
		});
	}
});