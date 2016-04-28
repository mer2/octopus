define("staticHuyu/huyu-shared-tipslayout", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var url;
	function cd() {
		var $cd = $("#countDown");
		if ($cd.length <= 0) {
			return;
		}
		var count = parseInt($cd.text());
		if (count < 1) {
			window.location.href = url;
			return;
		}
		$cd.html(count - 1);
		window.setTimeout(cd, 1000);
	}
	function init() {
		var $url = $("#gotoUrl");
		url = $url.attr("href");
		if (!url || url == "~/") {
			url = "/";
		}
		$url.attr("href", url);
		cd();
	}

	$(init);
	return { countDown: init };
});