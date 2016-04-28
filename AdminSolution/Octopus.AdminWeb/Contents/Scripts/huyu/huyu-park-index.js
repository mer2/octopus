//互娱园区首页
define("staticHuyu/huyu-park-index", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var params = require("plugin-params")(module, "huyu");

	var arr;
	$(function () {
		var map = new BMap.Map("parkMap"); // 创建地图实例   
		var myGeo = new BMap.Geocoder();
		myGeo.getPoint(params.Map.MapValue, function (point) {
			if (point) {
				map.centerAndZoom(point, parseInt(params.Map.Zoom));
			}
		}, params.Map.MapValue);

		//map.addControl(new BMap.NavigationControl()); // 调用缩放比例控件
		//map.addControl(new BMap.ScaleControl()); // 比例尺控件
		//map.addControl(new BMap.OverviewMapControl()); // 缩略图 
		//map.addControl(new BMap.MapTypeControl()); // 切换图片模式(图片/卫星/三纬) 
		if (parseInt(params.Total) > 0) {
			arr = eval(params.MapJson.replace(/&quot;/g, "\""));
			for (var i = 0; i < arr.length; i++) {
				var obj = arr[i];
				myGeo.getPoint(obj.Address, function (point, data) {
					if (point) {
						var marker = new BMap.Marker(point);
						map.addOverlay(marker);
						WriteFloat(data.address, marker);
					}
				}, params.Map.MapValue);
			}
		} 
	});
	function WriteFloat(address, marker) {
		var obj;
		for (var i = 0; i < arr.length; i++) {
			if (arr[i].Address == address) {
				obj = arr[i];
				break;
			}
		}
		if (obj != null) {
			marker.addEventListener("click", function () {
				$.get("/AjaxJson/GetPark?id=" + obj.ID, function (data) {
					if (data.ResultNo == 0) {
						var html = '';
						var park = data.ResultAttachObject;
						html += '<dl class="parkinfo_' + obj.ID + '">';
						html += '<dt><a href="/Company/Index/' + park.ID + '.html" target="_blank" title="' + park.Title + '"><img src="' + park.LogoUrl + '" alt="园区协会"><span>' + park.Title + '</span></a></dt>';
						html += '<dd><strong>园区面积：</strong>' + (park.ParkArea == null ? '暂无' : park.ParkArea + '平方米') + '</dd><dd><strong>租赁价格：</strong>' + (park.RentalPrice == null ? '暂无' : park.RentalPrice + '元/平米/天') + '</dd><dd><strong>招商电话：</strong>' + (park.CompanyPhone == null ? '暂无' : park.CompanyPhone) + '</dd>';
						html += '<dd style="display:none;"><strong>园区评分：</strong>分</dd>';
						html += '</dl>';
						html += '<div class="icon parkicon_' + obj.ID + '" style="display:none;"></div>';
						$(".mapparkinfo").html(html).show();
						$.ajax({
							url: "/AjaxJson/GetScoresByTargetValues?target=11&targetValues=" + obj.ID,
							type: "Post",
							success: function (scoredata) {
								if (scoredata.ResultNo == 0) {
									$(".parkinfo_" + obj.TargetValue + " dd:last").html("<strong>园区评分：</strong>" + obj.ResultAttachObject + "分").show();
								}
							}
						});
						$.post("/AjaxJson/GetParkIcon", { companyID: obj.ID }, function (icondata) {
							if (icondata.ResultNo == 0) {
								var iconhtml = '';
								for (var m = 0; m < icondata.ResultAttachObject.length; m++) {
									iconhtml += ParkIcon(icondata.ResultAttachObject[m]);
								}
								if (iconhtml != '') {
									$(".parkicon_" + obj.ID).html(iconhtml).show();
								}
							}
						});
					}
				});
			});
		}
	}



	$(function () {
		$(".conttab li").click(function () {
			var index = $(".conttab li").index($(this));
			$(".conttab li").removeClass("cur");
			$(this).addClass("cur");
			$(".sec-body .contbox").attr("style", "display: none;");
			$(".sec-body .contbox").eq(index).attr("style", "display: block");
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
						$(".starpark_" + obj.TargetValue).find(".num").html("综合评分<em>" + obj.ResultAttachObject + "</em><span>" + obj.Count + "家企业评分</span>");
					}
				}
				$(".starpark").find(".num").show();
			}
		});
	});

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

});