//换一换
define("staticHuyu/huyu-tender-changeoffer", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $changed = require("staticHuyu/huyu-changed");
	$(".change-tenderoffer").each(function () {
		var options = eval('(' + $(this).attr("data-views") + ')');
		var $element = $(this);
		$changed.Init(options, function (data) {
			ResponseHtml($element, data);
		});
	});

	$(".change-tenderoffer").click(function () {
		var startIndex = $(this).attr("lang");
		var options = eval('(' + $(this).attr("data-views") + ')');
		var $element = $(this);
		$changed.GetChanged("/AjaxJson/RecommendOffers?startIndex=" + startIndex, options, function (data) {
			ResponseHtml($element, data);
		});
	});

	function ResponseHtml($element, data) {
		if (data.ResultNo == 0) {
			$element.attr("lang", data.StartIndex);
			var html = '<ul>';
			for (var i = 0; i < data.TenderOffers.length; i++) {
				var offer = data.TenderOffers[i];
				html += '<li>';
				html += '<i>' + (parseInt(offer.MinBudget) > 0 ? "￥" + parseInt(offer.MinBudget) : "待协商") + '</i><a title="' + offer.Title + '" href="/Tender/TenderOffer/' + offer.OfferNo + '.html" target="_blank">' + offer.Title + '</a>';
				html += '</li>';
			}
			html += '</ul>';
			$element.parents(".sec-head").next().html(html);
			if (data.StartIndex >= data.Total) {
				$element.remove();
			}
		} else {
			$element.remove();
		}
	}
});