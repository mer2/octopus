define("staticHuyu/admin/huyu-www-index", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var settings = require("staticHuyu/admin/huyu-config");
	if (window != top) {
		window.open(window.location, "_top");
	}
	try {
		document.domain = settings.mainDomain;
	} catch (e) { }

	var $ = require("jquery");
	var md5 = require("md5");
	var ju = require("staticCommon/joy-utils");
	var params = require("plugin-params")(module, "huyu");
	params = $.extend({}, params);
	var tab = null;
	var accordion = null;

	var thisModule = {
		f_addTab: function (text, url, tabid) {
			if (!tabid) {
				tabid = md5(url);
			}
			if (!text) {
				text = "新页面";
			}
			if (ju.queryString("_tabid", url)) {
				url = ju.removeQueryString("_tabid", url);
			}
			url = ju.addQueryString("_tabid", tabid, url);
			tab.addTabItem({ tabid: tabid, text: text, url: url });
		},
		f_removeTab: function (url, tabid) {
			if (!tabid) {
				tabid = ju.queryString("_tabid", url);
				if (!tabid) {
					tabid = md5(url);
				}
			}
			tab.removeTabItem(tabid);
		}
	};
	$.joyadmin = $.extend(true, {}, $.joyadmin, thisModule);

	$(function () {
		//布局
		$("#joyManageLayout").ligerLayout({
			leftWidth: 190, height: '100%', heightDiff: -34, space: 4,
			onHeightChanged: function(options) {
				if (tab) {
					tab.addHeight(options.diff);
				}
				if (accordion && options.middleHeight - 24 > 0) {
					accordion.setHeight(options.middleHeight - 24);
				}
			}
		});
		var height = $(".l-layout-center").height();
		//Tab
		$("#framecenter").ligerTab({ height: height });

		$(".l-link").hover(function () {
			$(this).addClass("l-link-over");
		}, function () {
			$(this).removeClass("l-link-over");
		});
		//树
		var permissions = params.permissions ? params.permissions : [];
		var indexdata = new Array();
		var ps = {};
		$.each(permissions, function (idx, val) {
			var key = val.PermissionNo;
			var node = { id: key, text: val.Title, url: val.Url, parentNo: val.ParentNo, isexpand: true };
			indexdata.push(node);
			ps[key] = node;
		});
		var idata = new Array();
		for(var i = 0; i < indexdata.length; i++) {
			var n = indexdata[i];
			if(n.parentNo) {
				var pn = ps[n.parentNo];
				if(pn) {
					var ch = pn["children"];
					if(!ch) {
						ch = new Array();
						pn["children"] = ch;
					}
					ch.push(n);
					n = null;
				}
			}
			if(n) {
				idata.push(n);
			}
		}
		//把排在第一层有链接的菜单放到“功能菜单”里面
		var fchildren = new Array();
		var iidata = new Array();
		for(i = 0; i < idata.length; i++) {
			var ndd = idata[i];
			if(ndd.url) {
				fchildren.push(ndd);
			} else {
				iidata.push(ndd);
			}
		}
		if(fchildren.length > 0) {
			var fnode = { id: "Admin.FunctionMenu", text: "功能菜单", children: fchildren, url: null, parentNo: null, isexpand: true };
			iidata.push(fnode);
		}
		idata = iidata;
		var accordionHtml = '';
		for(i = 0; i < idata.length; i++) {
			var nd = idata[i];
			accordionHtml += '<div title="' + nd.text + '" class="l-scroll"><ul id="tree' + nd.id.replace(".", "_") + '" style="margin-top: 3px;"></ul></div>';
		}
		$("#joyAccordion").html(accordionHtml);
		for(i = 0; i < idata.length; i++) {
			nd = idata[i];
			$("#tree" + nd.id.replace(".", "_")).ligerTree({
				data: nd.children,
				checkbox: false,
				slide: false,
				nodeWidth: 100,
				attribute: ['nodename', 'url'],
				onClick: function (node) {
					if (!node.data.url) return;
					var tabid = $(node.target).attr("tabid");
					if (!tabid) {
						tabid = md5(node.data.url);
						$(node.target).attr("tabid", tabid);
					}
					thisModule.f_addTab(node.data.text, node.data.url, tabid);
				}
			});
		}
		//面板
		$("#joyAccordion").ligerAccordion({ height: height - 24, speed: null });
		
		tab = $("#framecenter").ligerGetTabManager();
		accordion = $("#joyAccordion").ligerGetAccordionManager();

		var url = params.url;
		if(url) {
			var title = params.title;
			thisModule.f_addTab(title, url);
		}
		$("#joyAccordion ul li span").each(function () {
			var $this = $(this);
			var $url = $this.parents("li").attr("url");
			if($url) {
				$this.html('<a href="' + $url + '">' + $this.text() + '</a>');
			}
		});
		$(".l-tree a").click(function () {
			var $this = $(this);
			$this.parents("span").click();
			return false;
		});
		$("#pageloading").hide();
	});
});