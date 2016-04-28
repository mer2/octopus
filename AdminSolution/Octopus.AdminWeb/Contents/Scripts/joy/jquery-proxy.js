//jQuery代理，配置文件已经预先加载了jQuery，所以这里简单的返回jQuery
define(function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	return window.jQuery;
});