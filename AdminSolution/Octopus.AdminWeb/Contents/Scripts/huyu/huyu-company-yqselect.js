define(function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery"); 
	var params = {
		search: {
			1: { "title": "园区", "jsonurl": "/AjaxJson/SearchPark"}
		}
	};
	$(function () {
		var searchContainer = $(".yqSelect"); //搜索容器
		var $tip = searchContainer.find(".relevance-tips"); //下拉大框
		var $inputarea = searchContainer.find("#ParentName");
		var $hiddenID = searchContainer.find("#ParentID");

		$("#ParentName").focus(function () {
			if ($(this).val() == $inputarea[0].defaultValue) {
				$(this).val("");
			}
			$inputarea.data("closeMonitor", 0);
			var searchType = 1;
			var theData = params.search[searchType];
			if (!theData) {
				return;
			}
			//            require.async(theData.jsonurl, function (d) {//加载数据
			//                theData.searchData = d;
			//            });
			$.ajax({
				url: "/AjaxJson/SearchPark",
				dataType: "json",
				type: "post",
				cache: false,
				data: { "keyword": $inputarea.val() },
				success: function (d) {
					theData.searchData = d;
				}
			});
		}).blur(function () {
			if ($(this).val() == "" || $(this).val() == $inputarea[0].defaultValue) {
				$(this).val($inputarea[0].defaultValue);
			}
			setTimeout(function () {
				$(".relevance-tips").html('');
				$(".relevance-tips").hide();
				searchContainer.css({ "z-index": 0 });
			}, 600);
		}).keydown(function (e) {
			e = e || event;
			var currKey = e.keyCode || e.which || e.charCode;

			if ($(this).val() != '' && $(".relevance-tips").find("a").length > 0) {
				var total = $(".relevance-tips").find("a").length;
				$(this).data("original_val", $(this).data("original_val") ? $(this).data("original_val") : $(this).val());
				$hiddenID.data("original_val", $hiddenID.data("original_val") ? $hiddenID.data("original_val") : $hiddenID.val());
				if (currKey == 38) {
					$(this).data("closeMonitor", 1);
					if ($(".relevance-tips").find(".cur").length == 0) {
						$(".relevance-tips").find("a:last").addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(".relevance-tips").find(".cur").text());
						$hiddenID.val($(".relevance-tips").find(".cur").attr("lang"));
					} else if ($(".relevance-tips").find(".cur").index() + 1 == 1) {
						$(".relevance-tips").find("a:first").removeClass("cur");
						$(this).val($(this).data("original_val"));
						$hiddenID.val($hiddenID.data("original_val"));
					} else {
						$(".relevance-tips").find(".cur").prev().addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(".relevance-tips").find(".cur").text());
						$hiddenID.val($(".relevance-tips").find(".cur").attr("lang"));
					}
					return false;
				} else if (currKey == 40) {
					$(this).data("closeMonitor", 1);
					if ($(".relevance-tips").find(".cur").length == 0) {
						$(".relevance-tips").find("a:first").addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(".relevance-tips").find(".cur").text());
						$hiddenID.val($(".relevance-tips").find(".cur").attr("lang"));
					} else if ($(".relevance-tips").find(".cur").index() + 1 == total) {
						$(".relevance-tips").find("a:last").removeClass("cur");
						$(this).val($(this).data("original_val"));
						$hiddenID.val($hiddenID.data("original_val"));
					}
					else {
						$(".relevance-tips").find(".cur").next().addClass("cur").siblings("a").removeClass("cur");
						$(this).val($(".relevance-tips").find(".cur").text());
						$hiddenID.val($(".relevance-tips").find(".cur").attr("lang"));
					}
					return false;
				} else {
					$(this).data("closeMonitor", 0);
					$(this).data("original_val", 0);
					$hiddenID.data("original_val", 0);
				}

			}
			if (currKey == 13) {
				$(".relevance-tips").hide().html("");
				searchContainer.css({ "z-index": 0 });
				//searchContainer.find(".sm_search").trigger("click");
			}
		}).on("keyup paste cut propertychange input", function () {
			if ($(this).data("closeMonitor")) return;
			keyLinkSearch();
		});

		$(".relevance-tips").on("click", "a.topsearch", function () {
			var title = $(this).text();
			$("#ParentName").val(title);
			$hiddenID.val($(this).attr("lang"));
			$(".relevance-tips").hide().html("");
			searchContainer.css({ "z-index": 0 });
			//searchContainer.find(".sm_search").trigger("click");
			return false;
		});


		function keyLinkSearch() {
			var searchType = 1; //$("#search_type").val();
			var theData = params.search[searchType];
			if (!theData) {
				return;
			}
			var searchData = theData.searchData;
			if (!searchData || searchData.length <= 0) {//数据还没加载完成或者没有数据
				return;
			}
			$(".relevance-tips").html("").hide();
			var val = $inputarea.val();
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
				if (e && e.Title && e.Title.toLowerCase().indexOf(lowerVal) >= 0) {//找到了
					result.push(e);
				}
			});
			theData.lastFound = result; //把本次查找到的结果保存起来，提高效率
			if (result.length <= 0) {
				return;
			}
			//显示
			var html = '';
			for (var j = 0; j < 5 && j < result.length; j++) {
				var id = result[j].ID;
				var searchTitle = result[j].Title;
				html += '<a class="topsearch" href="javascript:;" lang="' + id + '">' + searchTitle.replace(val, '<em>' + val + '</em>') + '</a>';
			}
			searchContainer.css({ "z-index": 99 });
			$(".relevance-tips").html(html).css({ "z-index": 100 }).show();
		}
	});

});