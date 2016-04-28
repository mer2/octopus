define("staticHuyu/huyu-mytrading-tenderoffers", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $layer = require("layer");
	var params = require("plugin-params")(module, "huyu");

	$(function () {
		$("#status").val(params.Status);

		$(".look-more").click(function () {
			var $tr = $(this).parents("tr");
			var offerNo = $(this).attr("lang");
			var $next = $tr.next();
			if ($next.attr("lang") == "2") {
				$layer.showMsg($next.attr("data"));
			} else if ($next.attr("lang") == "") {
				$.post("/AjaxJson/GetTenderMilestones.html", { offerNo: offerNo }, function (data) {
					if (data.ResultNo == 0) {
						$next.attr("lang", "1");
						if ($next.find(".tips").eq(0).html() == "") {
							$next.find(".tips").eq(0).html('<strong>乙方：' + data.TenderBidder.UserName + '</strong>');
						}
						$next.find(".tips").eq(1).html('<span class="f-r" style="margin-right:10px;"><a target="_blank" href="/Tender/TenderOfferManager/' + offerNo + '.html" title="招标管理">查看招标管理</a></span><span>最后交付日期：<i>' + data.MaxDate + '</i></span><span>总金额：<i>￥' + data.TotalMoney + '</i></span><span>已支付金额：<i>￥' + data.PayMoney + '</i></span>');
						var html = '';
						for (var i = 0; i < data.TenderMilestones.length; i++) {
							var obj = data.TenderMilestones[i];
							html += '<tr>';
							html += '<td class="title2">' + (i + 1) + '. ' + obj.Title + '</td>';
							html += '<td><i>￥' + obj.TotalCommission + '</i></td>';
							html += '<td>' + GetDate(obj.StartTime) + '</td>';
							html += '<td>' + GetDate(obj.EndTime) + '</td>';
							html += '<td>' + GetStatusName(obj.Status) + '</td>';
							html += '</tr>';
						}
						$next.find("table").append(html);
						$next.show();
					} else {
						$next.attr("lang", "2");
						$next.attr("data", data.ResultDescription);
						$layer.showMsg(data.ResultDescription);
					}
				});
			}
			return false;
		});
	});

	function GetDate(val) {
		var date = new Date(parseInt(val.substring(val.indexOf("(") + 1, val.indexOf(")"))));
		var str = date.getFullYear() + ".";
		if (date.getMonth() + 1 < 10) {
			str += "0" + (date.getMonth() + 1) + ".";
		} else {
			str += (date.getMonth() + 1) + ".";
		}
		if (date.getDate() < 10) {
			str += "0" + date.getDate();
		} else {
			str += date.getDate();
		}
		return str;
	}

	function GetStatusName(status) {
		switch (status) {
			case 10:
				return "待确认";
			case 20:
				return "已确认";
			case 30:
				return "工作中";
			case 40:
				return "已完成";
			case 50:
				return "交易完成";
			default:
				return "";
		}
	}
});