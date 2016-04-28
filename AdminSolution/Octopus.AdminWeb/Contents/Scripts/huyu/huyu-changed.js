//换一换
define("staticHuyu/huyu-changed", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $jStorage = require("jstorage");

	return {
		Init: function (options, callback) {
			if (options.key != null) {
				var data = $jStorage.get(options.key);
				if (data != null) {
					if (typeof (callback) == "function") {
						callback(data);
					}
				}
			}
		},
		GetChanged: function (url, options, callback) {
			$.post(url, options, function (data) {
				if (data.ResultNo == 0) {
					if (options.key != null) {
						if (data.StartIndex < data.Total) {
							$jStorage.set(options.key, data);
						} else {
							$jStorage.set(options.key, null);
						}
					}
				}
				if (typeof (callback) == "function") {
					callback(data);
				}
			});
		}
	};

});