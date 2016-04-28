define("staticHuyu/huyu-hire-myhires", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	//交易竞标信息显示隐藏效果
	$(".management .management-cont .table-tr1 .more").toggle(
		function () {
			$(this).parents("tr").next(".table-tr2").show();
		},
		function () {
			$(this).parents("tr").next(".table-tr2").hide();
		}
	);
});