define(function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = {
		search: {
		    1: { "title": "招标", "jsonurl": "http://game.joyyang.com/content/games", "gotourl": "http://huyu.joyyang.com/Site/Companies.html" },
		    2: { "title": "服务", "jsonurl": "http://infos.joyyang.com/content/news", "gotourl": "http://huyu.joyyang.com/Site/Supplies.html" },
		    3: { "title": "企业", "jsonurl": "http://trade.joyyang.com/content/products", "gotourl": "http://huyu.joyyang.com/Site/Requirements.html" },
		    4: { "title": "园区", "jsonurl": "staticLib/joyyang/search-data-pictures", "gotourl": "http://huyu.joyyang.com/Site/ProductCases.html" },
		    5: { "title": "新闻", "jsonurl": "staticLib/joyyang/search-data-pictures", "gotourl": "http://huyu.joyyang.com/Site/News-0-0-0-1.html" }
		}
	};
	$(function () {
		$(".search #keyword").addClass("keyword");
		$(".search #search_type").addClass("search_type");
		var searchContainer = $(".search"); //搜索容器
		var defaultVal = ["请填写招标", "请填写服务", "请填写企业", "请填写园区", "请填写新闻"];
		var keywords = $(".keyword");
		if (!keywords.length) {//没有搜索框？
			return;
		}
		keywords.css("color", "#999").get(0).defaultValue = defaultVal[0];
		searchContainer.find(".option a").click(function () {
			var thistype = this.lang;
			$(this).closest(".search").find(".search_type").val(thistype);
			$(this).addClass("cur").siblings("a").removeClass("cur");
			$(this).closest(".search").find(".keyword").focus();
			$(this).closest(".search").find(".keyword").get(0).defaultValue = defaultVal[$(this).index()];
			$(this).closest(".search").find(".hot-key .key-box").hide();
			$(this).closest(".search").find(".hot-key .key-box").eq($(this).index()).show();
		});
		$(".searchbtn").click(function () {
			var _keyword = $(this).closest(".search").find(".keyword");
			if (_keyword.val() == _keyword[0].defaultValue) {
				_keyword.val('');
			}
			var title = encodeURI(_keyword.val());
			var searchType = $(this).closest(".search").find(".search_type").val();
			var gotourl = params.search[searchType].gotourl;
			window.location.href = gotourl + (title == "" ? "" : "?title=" + title);
			/*if(navigator.userAgent.toLowerCase().indexOf("firefox") > -1){
			window.location.href = gotourl + title;
			}else{window.open(gotourl + title, "_blank");}	*/
			return false;
		});
		$(".keyword").each(function () {
			$(this).data("closeMonitor", 0);
		});

		$(".keyword").focus(function () {
			$(this).css("color", "#333");
			if ($(this).val() == this.defaultValue) {
				$(this).val("");
			}
			var searchType = $(this).closest(".search").find(".search_type").val();
			var theData = params.search[searchType];
			if (!theData) {
				return;
			}
			require.async(theData.jsonurl, function (d) {//加载数据
				theData.searchData = d;
			});
		}).blur(function () {
			$(this).css("color", "#999");
			var _hideElem = $(this).closest(".search").find(".search-tips");
			if ($(this).val() == "" || $(this).val() == this.defaultValue) {
				$(this).val(this.defaultValue);
			}
			setTimeout(function () {
				_hideElem.html('');
				_hideElem.hide();
			}, 600);
		}).on("keydown", function (e) {
			e = e || event;
			var currKey = e.keyCode || e.which || e.charCode;
			//currKey   38  ↑  40↓
			if ($(this).val() != '' && $(this).closest(".search").find(".search-tips a").length > 0) {
				var total = $(this).closest(".search").find(".search-tips a").length;
				$(this).data("original_val", $(this).data("original_val") ? $(this).data("original_val") : $(this).val());
				if (currKey == 38) {
					$(this).data("closeMonitor", 1);
					if ($(this).closest(".search").find(".search-tips .cur").length == 0) {
						$(this).closest(".search").find(".search-tips a:last").addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(this).closest(".search").find(".search-tips .cur").text());
					} else if ($(this).closest(".search").find(".search-tips .cur").index() + 1 == 1) {
						$(this).closest(".search").find(".search-tips a:first").removeClass("cur"); $(this).val($(this).data("original_val"));
					} else {
						$(this).closest(".search").find(".search-tips .cur").prev().addClass("cur").siblings("a").removeClass("cur"); $(this).val($(this).closest(".search").find(".search-tips .cur").text());
					}
					return false;
				} else if (currKey == 40) {
					$(this).data("closeMonitor", 1);
					if ($(this).closest(".search").find(".search-tips .cur").length == 0) {
						$(this).closest(".search").find(".search-tips a:first").addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(this).closest(".search").find(".search-tips .cur").text());
					} else if ($(this).closest(".search").find(".search-tips .cur").index() + 1 == total) {
						$(this).closest(".search").find(".search-tips a:last").removeClass("cur");
						$(this).val($(this).data("original_val"));
					}
					else {
						$(this).closest(".search").find(".search-tips .cur").next().addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(this).closest(".search").find(".search-tips .cur").text());
					}
					return false;
				} else {
					$(this).data("closeMonitor", 0);
					$(this).data("original_val", 0);
				}

			}
			if (currKey == 13) {
				$(this).closest(".search").find(".sm_search").trigger("click");
			}
		}).on("keyup paste cut propertychange input", function () {
			if ($(this).data("closeMonitor")) return;
			keyLinkSearch(this);
		});

		$(".search-tips").on("click", "a.topSearch", function () {
			var title = $(this).text();
			var $thiscon = $(this).closest(".search");
			$thiscon.find(".keyword").val(title);
			$thiscon.find(".search-tips").hide().html("");
			$thiscon.find(".sm_search").trigger("click");
			return false;
		});

		function keyLinkSearch(t) {
			var $con = $(t).closest(".search"); //.find(".
			var searchType = $con.find(".search_type").val();
			var theData = params.search[searchType];
			if (!theData) {
				return;
			}
			var searchData = theData.searchData;
			if (!searchData || searchData.length <= 0) {//数据还没加载完成或者没有数据
				return;
			}
			$con.find(".search-tips").html("").hide();
			var val = $con.find(".keyword").val();
			var lastVal = theData.lastVal; //上次输入的
			theData.lastVal = val; //把本次输入的保存
			if (!val) {//没有输入
				return;
			}
			var data = searchData; //是一个数组，元素{i: 数字ID，t: "标题", k: "被查询的关键字"}
			if (lastVal && val.indexOf(lastVal) == 0) {//如果本次是上次的继续输入，则在上次的结果里查找
				var lastFound = theData.lastFound;
				if (typeof (lastFound) != "undefined") {//上次都没有找到，这次肯定找不到
					if (lastFound.length <= 0) {
						return;
					} else {
						data = lastFound;
					}
				}
			}
			var result = new Array(); //查询好了的结果放在这里
			var lowerVal = val.toLowerCase();
			$.each(data, function (index, e) {//查找所有
				if (e && e.k && e.k.toLowerCase().indexOf(lowerVal) >= 0) {//找到了
					result.push(e);
				}
			});
			theData.lastFound = result; //把本次查找到的结果保存起来，提高效率
			if (result.length <= 0) {
				return;
			}
			//显示
			var html = "";
			for (var i = 0; i < 5 && i < result.length; i++) {
				var searchTitle = result[i].t;
				html += '<a href="javascript:;" class="topSearch">' + searchTitle.replace(val, '<span>' + val + '</span>') + '</a>';
			}
			$con.find(".search-tips").html(html).show();
		}
	});
});