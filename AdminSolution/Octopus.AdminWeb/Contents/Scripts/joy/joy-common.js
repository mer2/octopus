//基础工具类库
define("staticCommon/joy-common", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	//字符串格式化
	String.prototype.format = function () {
		var args = arguments;
		return this.replace(/\{(\d+)\}/g,
				function (m, i) {
					return args[i];
				});
	};
});