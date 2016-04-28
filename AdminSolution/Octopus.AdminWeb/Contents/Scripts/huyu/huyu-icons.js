define("staticHuyu/huyu-icons", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var setting = require("staticHuyu/huyu-config");
	$(function () {
		if ($(".icon").length > 0) {
			var users = {};
			var icons = {};
			$(".icon").each(function () {
				//GetIconsHtml($(this)); 
				var $this = $(this);
				var vals = $this.attr("lang");
				var user = $this.attr("data-user");
				if (user != undefined && vals != undefined && user != '' && vals != '') {
					$this.addClass("icon-" + user);
					if (users[user] != 1) {
						users[user] = 1;
					}
					var strs = vals.split(",");
					for (var i = 0; i < strs.length; i++) {
						if (icons[strs[i]] != 1) {
							icons[strs[i]] = 1;
						}
					}
				}
			});
			var userids = '';
			$.map(users, function (v, k) {
				if (userids != '') {
					userids += ',';
				}
				userids += k;
			});
			var iconkeys = '';
			$.map(icons, function (v, k) {
				if (iconkeys != '') {
					iconkeys += ',';
				}
				iconkeys += k;
			});
			getResponse(userids, iconkeys);
		}

		if ($(".user-icon").length > 0) {
			var jusers = {}, jicons = {};
			$(".user-icon").each(function () {
				var $this = $(this);
				var vals = $this.attr("lang");
				var user = $this.attr("data-user");
				if (user != undefined && vals != undefined && user != '' && vals != '') {
					$this.addClass("user-icon-" + user);
					if (jusers[user] != 1) {
						jusers[user] = 1;
					}
					var strs = vals.split(",");
					for (var i = 0; i < strs.length; i++) {
						if (jicons[strs[i]] != 1) {
							jicons[strs[i]] = 1;
						}
					}
				}
			});
			var juserids = '';
			$.map(jusers, function (v, k) {
				if (juserids != '') {
					juserids += ',';
				}
				juserids += k;
			});
			var jiconkeys = '';
			$.map(jicons, function (v, k) {
				if (jiconkeys != '') {
					jiconkeys += ',';
				}
				jiconkeys += k;
			});
			getjResponse(juserids, jiconkeys);
		}
	});

	function getResponse(users, vals) {
		if (users != "" && vals != "") {
			$.ajax({
				url: setting.urls.huyuUrl + "/AjaxJson/GetCompaniesIcons?users=" + users + "&vals=" + vals,
				dataType: "jsonp",
				type: "GET",
				success: function (data) {
					if (data.ResultNo == 0) {
						if (data.ResultAttachObject.length > 0) {
							for (var i = 0; i < data.ResultAttachObject.length; i++) {
								loadUserIcons(data.ResultAttachObject[i]);
							}
						}
					}
				}
			});
		}
	}

	function loadUserIcons(obj) {
		var userId = obj.User;
		var icons = obj.Icons;
		if (parseInt(userId) > 0 && icons != null) {
			$(".icon-" + userId).each(function () {
				var $this = $(this);
				var size = $this.attr("data-size");
				var only = $this.attr("data-only");
				var vals = $this.attr("lang").split(",");
				var html = '';
				for (var i = 0; i < vals.length; i++) {
					html += loadIconHtml(vals[i], icons[vals[i]], size, only);
				}
				$this.html(html).removeClass("icon-" + userId);
			});
		}
	}

	function loadIconHtml(val, hasIcon, size, only) {
		var html = '';
		if (only != 1 || hasIcon == 1) {
			switch (val) {
				case "user-company-park-auth":
					html = '<a class="state-icon' + size + ' member-state' + size + '-b' + (hasIcon == 1 ? "ok" : "") + '" href="javascript:void(0);"><span>' + (hasIcon == 1 ? "已认证" : "未认证") + '<i></i></span></a>';
					break;
				case "trans-security":
					html = '<a class="state-icon' + size + ' member-state' + size + '-a' + (hasIcon == 1 ? "ok" : "") + '" href="' + setting.urls.huyuUrl + '/baozhang.html"><span>' + (hasIcon == 1 ? "已担保" : "未担保") + '<i></i></span></a>';
					break;
				case "user-vip":
					html = '<a class="state-icon' + size + ' member-state' + size + '-c' + (hasIcon == 1 ? "ok" : "") + '" href="javascript:void(0);"><span>' + (hasIcon == 1 ? "Vip" : "普通会员") + '<i></i></span></a>';
					break;
				case "user-level":
					var userLevel = hasIcon.Level;
					html += '<a title="等级' + userLevel + '" class="rank-icon' + size + ' rank' + size + '-' + userLevel + '" href="javascript:void(0);"></a>';
					break;
			}
		}
		return html;
	}

	function getjResponse(users, vals) {
		if (users != "" && vals != "") {
			$.ajax({
				url: setting.urls.huyuUrl + "/AjaxJson/GetUsersIcons?users=" + users + "&vals=" + vals,
				dataType: "jsonp",
				type: "GET",
				success: function (data) {
					if (data.ResultNo == 0) {
						if (data.ResultAttachObject.length > 0) {
							for (var i = 0; i < data.ResultAttachObject.length; i++) {
								loadjUserIcons(data.ResultAttachObject[i]);
							}
						}
					}
				}
			});
		}
	}

	function loadjIconHtml(val, hasIcon, size, only, own) {
		var html = '';
		if (only != 1 || hasIcon == 1) {
			switch (val) {
				case "mobile":
					html = '<dl class="' + (hasIcon == 1 ? "finish" : "") + '">';
					html += '<dt class="phone"></dt>';
					if (own == 1) {
						if (hasIcon == 1) {
							html += '<dd class="">';
							html += '<p>已经绑定手机，<a href="' + setting.urls.userUrl + '/HuyuUser/ChangeMobile" title="点击修改" target="_blank">点击修改</a>。</p>';
							html += '<span class="arrow"></span>';
							html += '</dd>';
						} else {
							html += '<dd class="">';
							html += '<p>还未绑定手机，<a href="' + setting.urls.userUrl + '/HuyuUser/BindMobile" title="立即绑定" target="_blank">立即绑定</a>。</p>';
							html += '<span class="arrow"></span>';
							html += '</dd>';
						}
					}
					html += '</dl>';
					break;
				case "email":
					html = '<dl class="' + (hasIcon == 1 ? "finish" : "") + '">';
					html += '<dt class="email"></dt>';
					if (own == 1) {
						if (hasIcon == 1) {
							html += '<dd class="">';
							html += '<p>已经绑定邮箱，<a href="' + setting.urls.userUrl + '/HuyuUser/ChangeEmail" title="点击修改" target="_blank">点击修改</a>。</p>';
							html += '<span class="arrow"></span>';
							html += '</dd>';
						} else {
							html += '<dd class="">';
							html += '<p>还未绑定邮箱，<a href="' + setting.urls.userUrl + '/HuyuUser/BindEmail" title="立即绑定" target="_blank">立即绑定</a>。</p>';
							html += '<span class="arrow"></span>';
							html += '</dd>';
						}
					}
					html += '</dl>';
					break;
			}
		}
		return html;
	}

	function loadjUserIcons(obj) {
		var userId = obj.User;
		var icons = obj.Icons;
		var own = obj.Own;
		if (parseInt(userId) > 0 && icons != null) {
			$(".user-icon-" + userId).each(function () {
				var $this = $(this);
				var size = $this.attr("data-size");
				var only = $this.attr("data-only");
				var vals = $this.attr("lang").split(",");
				var html = '';
				for (var i = 0; i < vals.length; i++) {
					html += loadjIconHtml(vals[i], icons[vals[i]], size, only, own);
				}
				$this.html(html).removeClass("user-icon-" + userId);
			});
		}
	}
});