define("staticHuyu/huyu-favorite", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $layer = require("layer");
	var $passport = require("staticHuyu/huyu-navigate");
	$(function () {
		var array = new Array();
		var targetValues = '';
		var target = 0;
		$(".toFavorite").each(function () {
			var val = $(this).attr("data-views");
			if (val == undefined) {
				val = $(this).attr("data");
			}
			var json = eval('(' + val + ')');
			array.push([$(this), json.TargetValue]);
			targetValues += json.TargetValue + ",";
			target = json.Target;
		});
		$.get("/AjaxJson/IsFavorite", { target: target, targetValues: targetValues }, function (data) {
			if (data.ResultNo == 0) {
				if (data.ResultAttachObject != null && data.ResultAttachObject.length > 0) {
					for (var i = 0; i < data.ResultAttachObject.length; i++) {
						var obj = data.ResultAttachObject[i];
						if (obj.HasFavorite) {
							var $a = GetObj(array, obj.TargetValue);
							if ($a != null) {
								$a.attr("lang", obj.ID);
								$a.attr("title", "取消收藏").html("取消收藏");
							}
						}
					}
				}
			}
		});
	});

	function GetObj(array, targetValue) {
		for (var i = 0; i < array.length; i++) {
			if (array[i][1] == targetValue) return array[i][0];
		}
		return null;
	}

	$(".toFavorite").click(function () {
		var id = parseInt($(this).attr("lang"));
		if (!isNaN(id) && id > 0) {
			CancelFavorite(id, $(this));
		} else {
			var val = $(this).attr("data-views");
			if (val == "") {
				val = $(this).attr("data");
			}
			var json = eval('(' + val + ')');
			ToFavorite(json, $(this));
		}
		return false;
	});

	function ToFavorite(json, $a) {
		$passport.showLogin(function () {
			$.post("/AjaxJson/Favorite.html", json, function (data) {
				if (data.ResultNo == 0) {
					if (json.NeedRefurbish) {
						location.href = location.href;
					} else {
						$a.attr("lang", data.ID).attr("title", "取消收藏").html("取消收藏");
					}
				} else {
					$layer.showMsg("收藏失败");
				}
			});
		});
	}

	function CancelFavorite(id, $a) {
		$passport.showLogin(function () {
			$.post("/AjaxJson/CancelFavorite.html", { id: id }, function (data) {
				if (data.ResultNo == 0) {
					$a.attr("lang", "").attr("title", "收藏").html("收藏");
				} else {
					$layer.showMsg("取消收藏失败");
				}
			});
		});
	}

});