define("staticHuyu/huyu-select-category", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = {
		CompanyCategory: $("#companyCategory").val(),
		Industry: $("#industry").val(),
		Businesses: $("#businesses").val(),
	};
	//选择框初始化
	$(function () {
		SelectValue("category", params.CompanyCategory, true);//初始化
		CategorySelect($(".category .cur").attr("lang"), true);
		$(".category li").click(function () {
			$(".industry").html("");
			$(".businesses").html("");
			$("#industry").val("");
			$("#businesses").val("");
			var lang = $(this).attr("lang");
			CategorySelect(lang, false);
		});
		$(".industry").on("click", "li", function () {
			$(".businesses").html("");
			$("#businesses").val("");
			var lang = $(this).attr("lang");
			IndustrySelect(lang, false);
		});
		$(".businesses").on("click", "li", function () {
			$(".businesses").find("li").removeClass("cur");
			$(this).addClass("cur");
			$(this).next("p").hide();
			var lang = $(this).attr("lang");
			$("#companyCategory").val($(".category .cur").attr("lang"));
			$("#industry").val($(".industry .cur").attr("lang"));
			$("#businesses").val(lang);
			ChangeSelectTxt();
		});
		$(".select-mask").click(function () {
			if ($("#companyCategory").val() == "" || $("#industry").val() == "" || $("#businesses").val() == "") {
				$("#businesses").next("p").html('<span class="error">请选择项目所需行业和业务</span>').show();
				$("#category-industry-businesses").addClass("error");
			}
		});
		$(".select-open").click(function () {
			$("#category-industry-businesses").removeClass("error");
			$("#businesses").next("p").hide();
		});
	});
	function CategorySelect(parent, isInit) {
		$(".category").find("li").removeClass("cur");
		$(".category" + "-" + parent).addClass("cur");
		$.post("/AjaxJson/GetItems", { parentNo: parent }, function (data) {
			if (data.ResultNo == 0) {
				var obj = data.ResultAttachObject;
				var html = '';
				for (var i = 0; i < obj.length; i++) {
					if (!IsHidden(obj[i].ItemGroup, 2)) {
						html += '<li class="industry-' + obj[i].ItemNo + IsSelected(obj[i].ItemNo, params.Industry, isInit) + '" lang="' + obj[i].ItemNo + '">' + obj[i].Title + '</li>';
					}
				}
				$(".industry").html(html);
				if ($(".industry").find(".cur").length == 0) {
					$(".industry").find("li").eq(0).addClass("cur");
				}
				IndustrySelect($(".industry .cur").attr("lang"), isInit);
			}
			if (isInit) { 
				if (parseInt(params.CompanyCategory) > 0) {
					ChangeSelectTxt();
				}
			} else {
				ChangeSelectTxt();
			}
		});
	}

	function IndustrySelect(parent, isInit) {
		$(".industry").find("li").removeClass("cur");
		$(".industry" + "-" + parent).addClass("cur");
		$.post("/AjaxJson/GetItems", { parentNo: parent }, function (data) {
			if (data.ResultNo == 0) {
				var obj = data.ResultAttachObject;
				var html = '';
				for (var i = 0; i < obj.length; i++) {
					if (!IsHidden(obj[i].ItemGroup, 2)) {
						html += '<li class="businesses-' + obj[i].ItemNo + IsSelected(obj[i].ItemNo, params.Industry, isInit) + '" lang="' + obj[i].ItemNo + '">' + obj[i].Title + '</li>';
					}
				}
				$(".businesses").html(html);
			}
			if (isInit) {
				$(".businesses-" + params.Businesses).click();
				if (parseInt(params.Industry) > 0) {
					ChangeSelectTxt();
				}
			} else {
				ChangeSelectTxt();
			}
		});
	}

	function IsSelected(itemNo, val, isInit) {
		if (isInit) {
			if (itemNo == val) {
				return ' cur';
			}
		}
		return '';
	}

	function IsHidden(s, length) {
		if (s != null && s != "" && s.Length >= length) {
			var i = parseInt(s[length - 1]);
			if (i == 1) return true;
		}
		return false;
	}

	function SelectValue(category, val, hasDefault) {
		$("." + category).find("li").removeClass("cur");
		$("." + category + "-" + val).addClass("cur");
		if (hasDefault == true) {
			if ($("." + category).find(".cur").length == 0) {
				$("." + category).find("li").eq(0).addClass("cur");
			}
		}
	}
	function ChangeSelectTxt() {
		$(".category").parents("dl").find("dt label").html('<span>' + $(".category .cur").html() + ' &gt; ' + $(".industry .cur").html() + ' &gt; ' + ($(".businesses .cur").length == 0 ? "" : $(".businesses .cur").html()) + '</span><em></em>');
	}
});