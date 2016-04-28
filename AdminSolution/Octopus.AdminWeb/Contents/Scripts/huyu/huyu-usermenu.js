define("staticHuyu/huyu-usermenu", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	$(function () {
		$($("#content .sec-nav p").children().toArray().reverse()).each(function () {
			var lang = $(this).attr("lang");
			if (lang && lang != "") {
				$("#content .sec-sidebar .manage-menu ul li").has("a[lang='" + lang + "']").addClass("cur").parents(".manage-menu").addClass("active");
				return false;
			}
		});
	});
});