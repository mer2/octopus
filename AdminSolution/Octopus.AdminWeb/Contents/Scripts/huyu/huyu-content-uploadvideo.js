//互娱内容添加 供应信息
define("staticHuyu/huyu-content-uploadvideo", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		$(".manage-menu").eq(1).find(".sec-body li").eq(5).addClass("cur");
		if (params.msg == "OK") {
			layer.showConfirm('添加视频成功，继续添加？', {
				cancelfun: function () {
					location.href = "/HuyuContent/Videos.html";
				}
			});
		} else if (params.msg == "Error") {
			layer.showMsg("添加视频失败，请重新尝试！");
		}
		$("#videoUrl").focus(function () {
			$(this).next().html('请填写视频的地址，目前支持土豆、优酷、酷六等视频。');
		});
		$("#tjspForm").validate({
			rules: {
				videoTitle: {
					required: true
				},
				videoUrl: {
					required: true
				}
			},
			messages: {
				videoTitle: {
					required: "请填写视频名称"
				},
				videoUrl: {
					required: "请填写视频地址"
				}
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
				if ('country_province_city_area'.indexOf(element[0].id) > -1) {
					$(".validate_area").css('left', 350).html(error);
				} else {
					element.next().html(error);
				}
			},
			success: "valid",
			submitHandler: function (form) {
				if ($("#videoUrl").val().indexOf("http://") == -1) {
					$("#videoUrl").next().html('<span class="error">请正确填写视频url地址</span>');
					return false;
				}
				form.submit();
			}
		});
	});
});