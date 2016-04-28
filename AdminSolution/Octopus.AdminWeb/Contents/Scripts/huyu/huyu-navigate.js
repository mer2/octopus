//互娱通栏
define("staticHuyu/huyu-navigate", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("staticCommon/joy-security");
	var $ = require("jquery");
	var globalSettings = require("staticCommon/joy-config");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({}, globalSettings.urls, params);
	var args = $.extend({
		nru: false, //不添加返回地址
		nhl: false, //是否不显示通栏，no headline
		nbl: false //是否不显示低栏，no bottomline
	}, params);
	args.domain = globalSettings.currentDomain;//当前主域名
	args.userProfileUrl = args.userUrl + "/HuyuUser/UserState";//获取用户信息的地址
	args.userMessageUrl = args.profileUrl + "/Message/UserMessages";//获取用户消息的地址
	$.joy = $.extend({}, $.joy);
	var nav = $.joy.passport = {};
	$.extend(nav, {
		currentUser: null, //保存当前用户
		options: {}, //选项
		showLogin: function (options) {//显示登录层
			if (typeof (options) == 'function') {
				options = { success: options };
			}
			//每次都必须到服务器上获取用户是否登录
			nav.currentUser = null;
			nav.getUserStateInternal(function (user) {//用户已经登录了
				nav.currentUser = user;
			}, function () {
				nav.options = options = $.extend({ layerId: "passportLayer", title: "账号登录" }, options);
				if (nav.currentUser) {//已登录
					if (typeof (options.success) == 'function') {
						options.success(nav.currentUser);
					}
					return;
				}
				//未登录
				$.joy.layer({
					layerId: options.layerId,
					layer_title: options.title,
					width: 405,
					closeIcon: "关闭",
					url: args.passportUrl + "/HuyuPassport/LoginDialog",
					ajaxOptions: {
						crossDomain: true,
						dataType: 'jsonp'
					}
				});
				//触发层被打开的事件
				$("#passportLoginSuccess").trigger("dialogopen");
				//绑定关闭按钮事件
				$("#" + options.layerId + " .close").click(function () {
					nav.closeLayer(true);
				});
			});
		},
		closeLayer: function (closed) {//层被关闭时的调用
			var options = $.extend({}, nav.options);
			if (!closed && options.layerId) {//关闭登录层
				$.joy.closeLayer(options.layerId, true);
			}
			if (typeof (options.onDialogClose) == 'function') {
				options.onDialogClose();
			}
			//触发层被关闭事件
			$("#passportLoginSuccess").trigger("dialogclose");
		},
		loginSuccess: function (user) {//供外部调用，弹出层或第三方登录成功后被调用
			//登录成功，关闭层，调用myjoy的获取完整用户信息的方法
			var options = $.extend({}, nav.options);
			nav.getUserState(function (fu) {
				nav.closeLayer(false); //关闭登录层
				if (typeof (options.success) == 'function') {
					options.success(fu);
				}
			});
		},
		onauthenticated: function (user) {
			//UserID: 用户ID, Name, UserName: 用户名, DisplayName: 昵称, UserScore：积分, UserAmount：骄阳币, UserMessages：未读消息数
			if (user && user.Name) {
				//触发登录成功事件
				$("#passportLoginSuccess").trigger("click", user);
				nav.update(user);
			}
		},
		update: function (user) {//更新通栏上的信息
			if (!user) {
				user = nav.currentUser;
			}
			if (!user) {
				return;
			}
			//把用户信息保存起来
			nav.currentUser = user;
			//显示昵称
			$("#passportUserDisplay").text(user.DisplayName);
			//显示积分
			//$("#navigateScore").attr("title", user.UserScore > 0 ? "您有 " + user.UserScore + " 点阳光" : "您没有阳光了，赶紧去赚取吧！").text(user.UserScore);
			//显示资金
			$("#nav_UserAmount").text("￥" + user.UserAmount.toFixed(2));
			$("#nav_SaleAmount").text("￥" + user.SaleAmount.toFixed(2));
			$("#nav_PaidAmount").text("￥" + user.PaidAmount.toFixed(2));
			//显示未读消息
			var title0 = $.data(document, "documentTitle");
			if (user.UserMessages) {
				if (!title0) {//标题未保存
					title0 = document.title;
					$.data(document, "documentTitle", title0);
				}
				//在标题上显示
				document.title = "【您有新消息】" + title0;
				$(".navigateMessage .new-message").show().attr("title", "您有 " + user.UserMessages + " 条新消息").children("i").text("(" + (user.UserMessages > 99 ? "99+" : user.UserMessages) + ")");
				if (user.UserMessageCounts) {
					$.map(user.UserMessageCounts, function (val, key) {
						var it = $(".navigateMessage a.msg_type_" + key);
						it.attr("title", "").children("i").text("");
						if (val) {
							if (val > 99) {
								it.attr("title", val + " 条新消息").children("i").text("(99+)");
							} else {
								it.children("i").text("(" + val + ")");
							}
						}
					});
				}
				$(".navigateMessage").show();
			} else {
				if (title0) {
					document.title = title0;
				}
				$(".navigateMessage").hide();
			}
			//切换登录的状态
			$(".passportUnauthenticated").hide();
			$(".passportAuthenticated").show();
		},
		updateUserScore: function (score) {//通栏上的积分变化
			var user = nav.currentUser;
			if (user) {
				user.UserScore = user.UserScore ? user.UserScore + score : score;
				nav.update(user);
			}
		},
		updateUserMessages: function (counts) {//通栏上未读消息数变化
			var user = nav.currentUser;
			if (user) {
				user.UserMessageCounts = counts;
				var count = 0;
				$.map(counts, function (val) {//计算新消息总数
					count += val;
				});
				user.UserMessages = count;
				nav.update(user);
			}
		},
		fetchUserMessages: function () {//定时AJAX获取用户未读消息数
			$.ajax(args.userMessageUrl, {
				crossDomain: true,
				dataType: 'jsonp',
				success: function (result) {
					if (result && result.ResultNo == 0) {
						var count = result.ResultAttachObject;
						nav.updateUserMessages(count);
					}
				},
				complete: function () {
					nav.setTimeoutForUserMessages();
				}
			});
		},
		setTimeoutForUserMessages: function () {
			window.setTimeout("$.joy.passport.fetchUserMessages()", args.Interval || (1000 * 60 * 10)); //间隔10分钟
		},
		getUserStateInternal: function (callback, successHandler, options) {//AJAX请求用户当前用户信息
			$.ajax(args.userProfileUrl, $.extend({
				crossDomain: true,
				dataType: 'jsonp',
				data: { loc: args.loc },
				success: function (result) {
					if (result && result.ResultNo == 0 && result.ResultAttachObject) {
						var user = result.ResultAttachObject;
						if (typeof (callback) == 'function') {
							callback(user);
						}
					}
					if (typeof (successHandler) == 'function') {
						successHandler(result);
					}
				}
			}, options));
		},
		loginFromPartner: function (a, t) {//打开第三方登录框
			var $this = $(a);
			var url = $this.attr("href") + "?ReturnUrl=";
			if (t) { //本页面打开
				url += window.encodeURIComponent(window.location);
				window.location = url;
			} else {//弹窗打开
				try {
					document.domain = args.domain;
				} catch (e) { }
				url += window.encodeURIComponent("/HuyuPassport/LoginEmbed");
				var tg = $this.attr("target");
				if (!tg || tg == "_blank") {
					tg = "partner_win";
				}
				var b = window.open(url, tg, "status=no,resizable=no,scrollbars=yes,personalbar=no,directories=no,location=no,toolbar=no,menubar=no,width=680,height=500,left=50,top=40");
				window.focus && b.focus();
			}
			return false;
		},
		userStateHandler: function (result, callback) {
			if (typeof (callback) == 'function') {
				callback(result);
			}
			//底栏处理
			if (args.nbl) {//不显示底栏
				return;
			}
		},
		getUserState: function (callback, successHandler, options) {//AJAX请求用户当前用户信息
			nav.getUserStateInternal(function (user) {
				nav.onauthenticated(user);
				nav.setTimeoutForUserMessages();
				if (typeof (callback) == 'function') {
					callback(user);
				}
				nav.fetchUserMessages();//消息已分离到其他站点，需要主动调用一次消息
			}, function (result) {
				nav.userStateHandler(result, successHandler);
			}, options);
		}
	});
	$(function () {
		var html;
		if (args.nhl) {//不显示通栏
			html = '<li id="passportLoginSuccess" style="display:none"></li>'; //用来登录成功后的通知
		} else {//需要显示通栏
			html = '<div class="header-nav">' +
						'<div class="wp clearfix">' +
							'<div class="web-action f-l">' +
								'<span><a href="javascript:;" class="setHome" title="设为首页">设为首页</a></span>|' +
								'<span><a href="javascript:;" class="addFavorite" title="加入收藏">加入收藏</a></span>' +
								'<span class="weibo">' +
									'<a href="http://weibo.com/ppgame" class="sina" target="_blank" title="关注我们">&nbsp;</a>' +
									'<a href="http://e.t.qq.com/ppjoygame" class="qq" target="_blank" title="关注我们">&nbsp;</a>' +
								'</span>' +
							'</div>' +
							'<ul class="account-action f-r">' +
								'<li class="passportAuthenticated">您好，</li>' +
								'<li class="passportAuthenticated loggedin info-li" style="display:none">' +
									'<a class="new-info" href="' + args.profileUrl + '/"><i id="passportUserDisplay"></i></a>' +
									'<div class="info-list">' +
										'<p class="hack"><a href="' + args.profileUrl + '/" title="帐号管理">帐号管理</a></p>' +
										'<p class="hack"><a href="' + args.payUrl + '/NewPay/PayLogs" title="收入">收入：<i id="nav_SaleAmount">￥0.00</i></a></p>' +
										'<p class="hack"><a href="' + args.payUrl + '/NewPay/PayLogs" title="支出">支出：<i id="nav_PaidAmount">￥0.00</i></a></p>' +
										'<p><a href="' + args.payUrl + '/NewPay/Cashes" title="提现" class="f-r">提现</a><a href="' + args.payUrl + '/NewPay/RechargeOrders" title="充值" class="f-r">充值</a><span>余额：<i id="nav_UserAmount">￥0.00</i></span></p>' +
										'<p><a href="' + args.huyuUrl + '/MyTrading/FavoriteOffer" class="f-l" title="我的收藏">我的收藏</a><a class="out f-r" title="退出" href="' + args.passportUrl + '/HuyuPassport/Logout" id="passportLogout">[退出]</a></p>' +
									'</div>' +
								'</li>' +

								'<li class="navigateMessage mes-li" style="display:none">' +
									'<a href="javascript:;" class="new-message" title="新消息"><i></i></a>' +
									'<div class="mes-list">' +
										'<p><a href="' + args.profileUrl + '/Message/ChatRecords" title="聊天记录" class="msg_type_1">聊天记录<i></i></a></p>' +
										'<p><a href="' + args.profileUrl + '/Message/TradingInfos" title="交易通知" class="msg_type_4">交易通知<i></i></a></p>' +
										'<p><a href="' + args.profileUrl + '/Message/SystemNotifications" title="系统通知" class="msg_type_3">系统通知<i></i></a></p>' +
									'</div>' +
								'</li>' +

								'<li class="passportUnauthenticated login"><a id="passportLogin" href="' + args.passportUrl + '/HuyuPassport/Login" title="登录">请登录</a></li>' +
								'<li class="passportUnauthenticated"><a id="passportRegister" href="' + args.passportUrl + '/HuyuPassport/Register" title="免费注册">免费注册</a></li>' +

								//'<li><a href="' + args.profileUrl + '/" title="个人中心">个人中心</a></li>' +

								'<li class="pulldown">' +
									'<a href="' + args.huyuUrl + '/User/Index.html" title="我的交易" class="arrow">我的交易</a>' +
									'<div class="contbox">' +
										'<a href="' + args.huyuUrl + '/MyTrading/PublishTenderOffer1" title="立即发布招标">立即发布招标</a>' +
										'<a href="' + args.huyuUrl + '/MyTrading/MyTenderOffers" title="我发布的招标">我发布的招标</a>' +
										'<a href="' + args.huyuUrl + '/Hire/MyHires" title="我发起的雇佣">我发起的雇佣</a>' +
										'<a href="' + args.huyuUrl + '/Supply/PurchasedSupplies" title="我购买的服务">我购买的服务</a>' +
									'</div>' +
								'</li>' +

								//'<li><a href="' + args.huyuUrl + '/Company/Index" title="我的黄页">我的黄页</a></li>' +

								'<li class="pulldown">' +
									'<a href="' + args.huyuUrl + '/Company/Index" title="我的黄页" class="arrow">我的黄页</a>' +
									'<div class="contbox">' +
										'<a href="' + args.huyuUrl + '/MyTrading/TenderOffers" title="参与的竞标">参与的竞标</a>' +
										'<a href="' + args.huyuUrl + '/Hire/MyHireds" title="接受的雇佣">接受的雇佣</a>' +
										'<a href="' + args.huyuUrl + '/Supply/MySupplies" title="发布的服务">发布的服务</a>' +
										'<a href="' + args.huyuUrl + '/Supply/SaledSupplies" title="卖出的服务">卖出的服务</a>' +
									'</div>' +
								'</li>' +

								'<li><a href="' + args.huyuUrl + '/Helpers/" title="帮助中心">帮助中心</a></li>' +

								'<li id="passportLoginSuccess" style="display:none"></li>' + //用来登录成功后的通知
							'</ul>' +
						'</div>' +
					'</div>';
		}
		var container = $("#passportNavigate");
		if (container.length == 0) { //没找到？
			container = $("#header");
			if (container.length == 0) { //还是没找到？
				container = $("body");
			}
			$(html).insertBefore(container.contents().first());
		} else {
			container.html(html);
		}
		//设为首页等事件绑定
		$(".header-nav a.setHome").click(function () {
			var url = args.huyuUrl;
			if (document.all) {
				document.body.style.behavior = 'url(#default#homepage)';
				document.body.setHomePage(url);
			} else {
				alert("您好，您的浏览器不支持自动设置页面为首页功能，请您手动在浏览器里设置该页面为首页！");
			}
			return false;
		});
		$(".header-nav a.addFavorite").click(function () {
			var url = encodeURI(args.huyuUrl);
			var sTitle = '中国互动娱乐产业服务交易平台';
			try {
				window.external.addFavorite(url, sTitle);
			} catch (e) {
				try {
					window.sidebar.addPanel(sTitle, url, "");
				} catch (e) {
					alert("加入收藏失败，请使用Ctrl+D进行添加，或手动在浏览器里进行设置。");
				}
			}
			return false;
		});
		$(".header-nav .account-action li.pulldown").mouseover(function () {
			$(this).addClass("cur");
		}).mouseout(function () {
			$(this).removeClass("cur");
		});
		$(".navigateMessage").mouseover(function () {
			$(this).addClass("msg-hover");
		}).mouseout(function () {
			$(this).removeClass("msg-hover");
		});
		$(".header-nav .account-action li.info-li").mouseover(function () {
			$(this).addClass("info-hover");
		}).mouseout(function () {
			$(this).removeClass("info-hover");
		});
		var paramUrl = args.nru ? "" : ("?ReturnUrl=" + window.encodeURIComponent(window.location));
		args.paramUrl = paramUrl;
		$("#passportLogin").attr("href", args.passportUrl + "/HuyuPassport/Login" + paramUrl);
		$("#passportRegister").attr("href", args.passportUrl + "/HuyuPassport/Register" + paramUrl);
		$("#passportLogout").attr("href", args.passportUrl + "/HuyuPassport/Logout" + paramUrl);

		//第一次载入需要获取用户信息
		nav.getUserState();
	});
	return nav;
});