define("staticHuyu/huyu-search", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	String.prototype.totrim = function () {
		return this.replace(/\s+ /g, "");
	};
	
	var $ = require("jquery");
	var globalSetting = require("staticHuyu/huyu-config");
	var setting = globalSetting.urls;
	var params = {
		keys: ['zb', 'fw', 'qy', 'yq', 'xw'],
		menus: {
			zb: { menu: 1, tab: 1 },
			fw: { menu: 1, tab: 1 },
			qy: { menu: 1, tab: 1 },
			yq: { menu: 1, tab: 1 },
			xw: { menu: 0, tab: 1 },
			jybz: { menu: 1, tab: 0 }
		},
		options: {
			zb: { title: "招标", lang: "zb", dataurl: "staticHuyu/huyu-search-tender", defaultValue: '招标搜索', url: setting.huyuUrl + '/Tender/Search.html', name: 'title', data: null },
			fw: { title: "服务", lang: "fw", dataurl: "staticHuyu/huyu-search-service", defaultValue: '服务搜索', url: setting.huyuUrl + '/Supply/SearchSupply.html', name: 'Keyword', data: null },
			qy: { title: "企业", lang: "qy", dataurl: "staticHuyu/huyu-search-company", defaultValue: '企业搜索', url: setting.huyuUrl + '/Company/SearchCompany.html', name: 'Keyword', data: null },
			yq: { title: "园区", lang: "yq", dataurl: "staticHuyu/huyu-search-park", defaultValue: '园区搜索', url: setting.huyuUrl + '/Park/Search.html', name: 'title', data: null },
			xw: { title: "新闻", lang: "xw", dataurl: "staticHuyu/huyu-search-news", defaultValue: '新闻搜索', url: setting.huyuUrl + '/Site/NewsSearch.html', name: 'title', data: null }
		}
	};

	$(function () {
		//选项卡初始化
		var html = '';
		$.each(params.keys, function (idx, a) {
			var b = params.options[a];
			html += '<a lang="' + b.lang + '" data-defaultvalue="' + b.defaultValue + '" title="' + b.title + '" href="javascript:;">' + b.title + '</a>';
		});
		$(".search .option").html(html);
		var tabParam = require("plugin-params")(module, "huyu");
		var currentkey = 'zb';
		var isIndex = true;
		if (tabParam != undefined) {
			if (tabParam.TabKey != null && tabParam.TabKey != "") {
				var menu = params.menus[tabParam.TabKey];
				if (menu != null) {
					if (menu.menu == 1) {
						isIndex = false;
						$(".header-menu ." + tabParam.TabKey).addClass("cur");
					}
					if (menu.tab == 1) {
						currentkey = tabParam.TabKey;
					}
				}
			}
		}
		if (isIndex) {
			$(".header-menu li").eq(0).addClass("cur");
		}
		var current = params.options[currentkey];
		$(".search .option a[lang='" + currentkey + "']").addClass("cur");
		$(".search .input input:text").attr("name", current.name).attr("autocomplete", "off");
		$(".search .input input:text").attr("data-defaultValue", current.defaultValue).val(current.defaultValue);
		$(".search .hot-key div[lang='" + currentkey + "']").show();

		$(".search .option").on("click", "a", function () {
			current = params.options[$(this).attr("lang")];
			$(".search .option a").removeClass("cur");
			$(this).addClass("cur");
			$(".search .input input:text").attr("data-defaultValue", $(this).attr("data-defaultvalue")).val($(this).attr("data-defaultvalue")).attr("name", current.name);
			$("#search-tips").html("").hide();
			$(".search .hot-key div").hide();
			$(".search .hot-key div[lang='" + $(this).attr("lang") + "']").show();
			return false;
		});

		$(".search .input input:submit").click(function () {
			var val = $(".search .input input:text").val().totrim();
			if (val != "" && val != current.defaultValue) {
				$(this).parents("form").attr("action", current.url);
				$(this).parents("form").submit();
			} else {
				return false;
			}
		});

		$(".search .input").on("focus", "input:text", function () {
			var $this = $(this);
			var val = $this.val();
			if ($this.attr("data-defaultValue") == val) {
				$this.val("");
			}
			if ($this.val() != "") {
				Search($this, false);
			}
			if (current.data == null) {
				require.async(current.dataurl, function (data) {//加载数据
					current.data = data;
				});
			}
		});
		$(".search .input").on("blur", "input:text", function () {
			var $this = $(this);
			if ("" == $this.val()) {
				$this.val($this.attr("data-defaultValue"));
			}
			if ($(".search-tips a").length > 0) {
				setTimeout(function () {
					$("#search-tips").html("").hide();
				}, 60);
			}
		});
		$(".search #search-tips").on("click", "a", function () {
			if ($(this).attr("href").indexOf("http://") != 0) {
				$(".search .input input:text").val($(this).attr("data-title"));
				$(".search .input input:submit").click();
				return false;
			}
		});
		$(".search #search-tips").on("mouseover", "a", function () {
			$(".search #search-tips .cur").removeClass("cur");
			$(this).addClass("cur");
		});
		$(".search .input").on("keydown", "input:text", function (e) {
			e = e || event;
			var currKey = e.keyCode || e.which || e.charCode;//38  ↑  40↓ 
			var $tips = $("#search-tips");
			if (currKey == 38 || currKey == 40 || currKey == 13 || currKey == 27) {
				if (currKey == 40) {
					if ($tips.find(".cur").length == 0) {
						$tips.find("a:first").addClass("cur");
						$(this).val($tips.find("a:first").attr("data-title"));
					} else {
						var $next = $tips.find(".cur").next();
						if ($next.length > 0) {
							$tips.find(".cur").removeClass("cur");
							$next.addClass("cur");
							$(this).val($next.attr("data-title"));
						} else {
							$tips.find(".cur").removeClass("cur");
							$(this).val(current.lastVal);
						}
					}
					return false;
				}
				if (currKey == 38) {
					var $cur = $tips.find(".cur");
					if ($cur.length == 0) {
						$tips.find("a:last").addClass("cur");
						$(this).val($tips.find("a:last").attr("data-title"));
					} else {
						var index = $tips.find("a").index($cur) - 1;
						if (index >= 0) {
							$cur.removeClass("cur");
							$tips.find("a").eq(index).addClass("cur");
							$(this).val($tips.find("a").eq(index).attr("data-title"));
						} else {
							$tips.find(".cur").removeClass("cur");
							$(this).val(current.lastVal);
						}
					}
					return false;
				}
				if (currKey == 13) {
					var submit = true;
					if ($("#search-tips .cur").length > 0) {
						var url = $("#search-tips .cur").attr("href");
						if (url.indexOf("http://") == 0) {
							document.location.href = url;
							submit = false;
						}
					}
					if (submit) {
						$(".search .input input:submit").click();
					}
				}
				if (currKey == 27) {
					$("#search-tips").html("").hide();
				}
				return false;
			}
		});
		$(".search .input").on("keyup", "input:text", function (e) {
			e = e || event;
			var currKey = e.keyCode || e.which || e.charCode;//38  ↑  40↓  
			if (currKey == 38 || currKey == 40 || currKey == 13 || currKey == 27) {
				return false;
			}
			Search($(this), true);
		});

		function Search($this, b) {
			$this.val($this.val().totrim());
			var lastVal = current.lastVal;
			var val = $this.val();
			current.lastVal = val;
			if (val == lastVal && b) {
				return;//没有输入
			}
			var searchData = current.data;
			if (!searchData || searchData.length <= 0) {//数据还没加载完成或者没有数据
				return;
			}
			if (lastVal && val.indexOf(lastVal) == 0) {//如果本次是上次的继续输入，则在上次的结果里查找
				var lastFound = current.lastFound;
				if (typeof (lastFound) != "undefined") {//上次都没有找到，这次肯定找不到
					if (lastFound.length <= 0) {
						return;
					} else {
						searchData = lastFound;
					}
				}
			}
			var result = new Array(); //查询好了的结果放在这里
			var lowerVal = val.toLowerCase();
			$.each(searchData, function (index, a) {//查找所有
				if (a && a.k && a.k.toLowerCase().indexOf(lowerVal) >= 0) {//找到了
					result.push(a);
				}
			});
			current.lastFound = result; //把本次查找到的结果保存起来，提高效率
			if (result.length <= 0) {
				$("#search-tips").hide();
				return;
			}
			//显示
			var tipshtml = "";
			for (var i = 0; i < 5 && i < result.length; i++) {
				var searchTitle = result[i].t;
				var url = result[i].u;
				if (url == null || url == "") {
					url = "javascript:;";
				}
				tipshtml += '<a href="' + url + '" data-title="' + searchTitle + '">' + searchTitle.replace(val, '<span>' + val + '</span>') + '</a>';
			}
			$("#search-tips").html(tipshtml).show();
		}
	});
});