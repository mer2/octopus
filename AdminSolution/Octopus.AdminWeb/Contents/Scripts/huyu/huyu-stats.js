define("staticHuyu/huyu-stats", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var setting = require("staticHuyu/huyu-config");
	$(function () {
		if ($(".user-stats").length > 0) {
			$(".user-stats").each(function () {
				Ajax($(this));
			});
		}
	});

	function Ajax($this) {
		var userId = $this.attr("data-views");
		var vals = $this.attr("lang");
		$.ajax({
			url: setting.urls.myUrl + "/AjaxJson/GetUserStat?userid=" + userId + "&vals=" + vals,
			dataType: "jsonp",
			type: "GET",
			success: function (data) {
				if (data.ResultNo == 0) {
					if (data.ResultAttachObject.length > 0 || $this.attr("data-view") == "show") {
						var html = '';
						var strs = vals.split(',');
						for (var i = 0; i < strs.length; i++) {
							html += GetHtml(GetCount(data.ResultAttachObject, strs[i]), strs[i]);
						}
						$this.html(html).show();
					}
				}
			}
		});
	}

	function GetCount(objs, category) {
		if (objs == null || objs.length == 0) return 0;
		for (var i = 0; i < objs.length; i++) {
			if (objs[i].TargetType == category) {
				return objs[i].StatCount;
			}
		}
		return 0;
	}

	function GetHtml(count, category) {
		switch (category) {
			case "Joy.Huyu.All.Offer.Publish":
				return "<li>发布的招标（<i>" + count + "</i>）</li>";
			case "Joy.Huyu.All.Hire.Publish":
				return "<li>发起的雇佣（<i>" + count + "</i>）</li>";
			case "Joy.Account.All.ReleaseAmount.Buyer":
				return "<li>待您去付款（<i>" + count + "</i>）</li>";
			case "Joy.Huyu.All.Service.Publish":
				return "<li>发布的服务（<i>" + count + "</i>）</li>";
			case "Joy.Huyu.All.Offer.Bidder":
				return "<li>参与的竞标（<i>" + count + "</i>）</li>";
			case "Joy.Huyu.All.Hire.Hire":
				return "<li>参与的雇佣（<i>" + count + "</i>）</li>";
			case "Joy.Account.All.ReleaseAmount.Seller":
				return "<li>待甲方付款（<i>" + count + "</i>）</li>";
		}
		return "";
	}
});