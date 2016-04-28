define("staticHuyu/huyu-follow", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var layer = require("layer");
	var $passport = require("staticHuyu/huyu-navigate");

	function GetObj(array, targetValue) {
		for (var i = 0; i < array.length; i++) {
			if (array[i][1] == targetValue) return array[i][0];
		}
		return null;
	}

	return {
		FollowInit: function (elements, callback) {
			var userids = '';
			var arr = new Array();
			elements.each(function () {
				arr.push([$(this), $(this).attr("lang")]);
				userids += $(this).attr("lang") + ',';
			});
			$.get("/AjaxJson/IsFollow", { users: userids }, function (data) {
				if (data.ResultNo == 0) {
					if (data.ResultAttachObject != null && data.ResultAttachObject.length > 0) {
						for (var i = 0; i < data.ResultAttachObject.length; i++) {
							var obj = data.ResultAttachObject[i];
							if (obj.IsFollow) {
								var $element = GetObj(arr, obj.UserID);
								if ($element != null && typeof (callback) == "function") {
									callback($element);
								}
							}
						}
					}
				}
			});
		},
		Follow: function (friendId, callback) {
			$passport.showLogin(function () {
				$.post("/AjaxJson/AddFollow.html", { userId: friendId }, function (data) {
					if (data.ResultNo == 0) {
						if (typeof (callback) == "function") {
							callback();
						}
					} else {
						layer.showMsg("关注失败！");
					}
				});
			});
		},
		CancelFollow: function (friendId, callback) {
			$.post("/AjaxJson/DeleteFollow.html", { userId: friendId }, function (data) {
				if (data.ResultNo == 0) {
					if (typeof (callback) == "function") {
						callback();
					}
				} else {
					layer.showMsg("关注失败！");
				}
			});
		}
	};
});