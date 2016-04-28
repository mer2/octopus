define("staticHuyu/admin/huyu-shared-layout", function(require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var settings = require("staticCommon/joy-config");
	try { document.domain = settings.mainDomain; } catch (e) { }
	var ju = require("staticCommon/joy-utils");
	if (window == top) {
		if (!ju.queryString("nif")) {
			window.location.href = settings.urls.adminUrl + "/?url=" + encodeURIComponent(window.location.href) + "&title=" + encodeURIComponent(window.document.title);
		}
	}
	var $ = require("jquery");
	var joyadmin = require("staticHuyu/admin/huyu-common");
	$(function () {
		$(".admin_openTab").click(function () {
			var $this = $(this);
			joyadmin.openUrl($this.attr("href"), $this.attr("title"));
			return false;
		});
		$(".admin_closeTab").click(function () {
			joyadmin.closeWindow();
			return false;
		});
	});
});