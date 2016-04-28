//互娱园区首页
define("staticHuyu/huyu-park-parklist", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var params = require("plugin-params")(module, "huyu");

	function ChangeParks(s) {
		$.post("/AjaxJson/GetParks", { startIndex: s, length: 3 }, function (data) {
			if (data.ResultNo == 0) {
				var html = "";
				var obj = data.ResultObject;
				if (obj != null && obj.length > 0) {
					for (var i = 0; i < obj.length; i++) {
						html += '<dl lang="' + obj[i].ID + '" class="clearfix toppark_' + obj[i].ID + '">';
						html += '<dt class="f-l"><a href="/Company/UserCompany/' + obj[i].UserID + '.html" target="_blank" title="">';
						html += '<img src="' + params.ImgUrl + '/u/t/hyavatar!' + obj[i].UserID + '.jpg" width="80" height="60" alt=""></a></dt>';
						html += '<dd class="f-r"><a href="/Company/Index/' + obj[i].ID + '.html" target="_blank" title="' + obj[i].Title + '">' + obj[i].Title + '</a>';
						html += '<span class="f-l" style="display:none;">园区评分：<i>0</i>分</span>';
						html += '</dd>';
						html += '</dl>';
					}
				}
				if (html != '') {
					$(".info-similar .sec-body").html(html);
					$(".info-similar .more a").attr("lang", data.StartIndex);
					$(".info-similar").show();
					GetScores();
				}
			}
		});
	}

	function GetScores() {
		var targetValues = '';
		$(".info-similar .sec-body dl").each(function () {
			targetValues += $(this).attr("lang");
		});
		$.ajax({
			url: "/AjaxJson/GetScoresByTargetValues?target=11&targetValues=" + targetValues,
			type: "Post",
			success: function (data) {
				if (data.ResultNo == 0) {
					var lst = data.ResultAttachObject;
					for (var i = 0; i < lst.length; i++) {
						var obj = lst[i];
						$(".toppark_" + obj.TargetValue).find(".num").html("综合评分<em>" + obj.ResultAttachObject + "</em><span>" + obj.Count + "家企业评分</span>").show();
					}
				}
			}
		});
	}

	function ParkIcon(obj) {
		var html = '';
		switch (obj.Title) {
			case "交通":
				html += '<a class="association-icon ass-icon1" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
			case "车位":
				html += '<a class="association-icon ass-icon2" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
			case "银行":
				html += '<a class="association-icon ass-icon3" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
			case "餐饮":
				html += '<a class="association-icon ass-icon4" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
			case "住宿":
				html += '<a class="association-icon ass-icon5" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
			case "孵化器":
				html += '<a class="association-icon ass-icon6" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
			case "其他":
				html += '<a class="association-icon ass-icon7" href="javascript:void(0);"><span>' + obj.Title + '<i></i></span></a>';
				break;
		}
		return html;
	}

	$(function () {
		ChangeParks(0);
		$(".info-similar .more a").click(function () {
			ChangeParks($(this).attr("lang"));
		});
		var targerValue = "";
		$(".starpark").each(function () {
			targerValue += $(this).attr("lang") + ",";
		});
		$(".starpark .icon").html("&nbsp;").show();
		$(".starpark").find(".num").html('<em class="noscore">暂无</em>');
		$.post("/AjaxJson/GetParksIcon", { companys: targerValue }, function (data) {
			if (data.ResultNo == 0) {
				for (var i = 0; i < data.ResultAttachObject.length; i++) {
					var lst = data.ResultAttachObject[i];
					var html = '';
					for (var j = 0; j < lst.ResultAttachObject.length; j++) {
						var obj = lst.ResultAttachObject[j];
						html += ParkIcon(obj);
					}
					$(".starpark_" + lst.CompanyID).find(".icon").html(html).show();
				}
			}
		});
		$.ajax({
			url: "/AjaxJson/GetScoresByTargetValues?target=11&targetValues=" + targerValue,
			type: "Post",
			success: function (data) {
				if (data.ResultNo == 0) {
					var lst = data.ResultAttachObject;
					for (var i = 0; i < lst.length; i++) {
						var obj = lst[i];
						$(".starpark_" + obj.TargetValue).find(".num").html("综合评分<em>" + obj.ResultAttachObject + "</em><span>" + obj.Count + "家企业评分</span>").show();
					}
				}
				$(".starpark").find(".num").show();
			}
		});
	});
});