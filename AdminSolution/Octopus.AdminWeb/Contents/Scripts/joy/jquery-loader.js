//jQuery载入器，如果页面上已经载入了jQuery，则不再载入，否则异步载入
define(function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	return function (callback) {
		if (typeof (callback) != "function") {
			callback = function () { };
		}
		if (window.jQuery) {//jQuery已引用，直接回调
			callback(window.jQuery);
		} else {//jQuery未引用？
			require.async("jquery", function ($) {
				callback($);
			});
		}
	};
});