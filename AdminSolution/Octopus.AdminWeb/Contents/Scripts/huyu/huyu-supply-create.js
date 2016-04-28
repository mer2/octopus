define("staticHuyu/huyu-supply-create", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var ju = require("staticCommon/joy-upload");
	var tools = require("staticHuyu/huyu-company-tools");
	require("staticHuyu/huyu-company-validate");
	$(".other input[name='Other']").change(function () {
		var ths = $(this);
		if ($.trim(ths.val()) == "") {
			ths.closest(".other").find("input[name='ServiceProvider']").attr("checked", false);
		} else {
			ths.closest(".other").find("input[name='ServiceProvider']").attr("checked", true);
		}
	});
	$(".other input[name='CommissionPercent']").change(function () {
		var ths = $(this);
		if (!tools.IsInt(ths.val())) {
			ths.val("");
		}
		if ($.trim(ths.val()) == "") {
			ths.closest(".other").find("input[name='ServiceRequire']").attr("checked", false);
		} else {
			ths.closest(".other").find("input[name='ServiceRequire']").attr("checked", true);
		}
	});
	$(".management .management-form .deal-form .icon-service li").click(function () {
		$(this).addClass("cur").siblings().removeClass("cur");
		$("#IconUrl").val($(this).attr("lang"));
		$("#LogoReview img").attr("src", $(this).attr("lang"));
	});
	$(".management .management-form .deal-form .icon-service li").hover(
	  function () {
	  	$(this).find("div").show();
	  },
	  function () {
	  	$(this).find("div").hide();
	  }
	);
	ju.create({
		button_placeholder_id: "swfUploadButton",
		file_types: "*.jpg;*.jpeg;*.png",
		file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
		file_size_limit: "100KB",
		button_text: "",
		button_width: 158,
		button_height: 46,
		button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
		button_cursor: -2, //SWFUpload.CURSOR.HAND
		post_params: {
		    'maxBytes': 1024 * 100
			, 'imageSize': true
			, 'minWidth': 1
			, 'minHeight': 1
		},
		upload_success_handler: function (file, serverData) {
			var data = $.parseJSON(serverData);
			if (data.ResultNo == 0) {
				var fi = data.ResultAttachObject;//fi为上传成功后返回的文件信息
				var fileUrl = fi.FileUrl;//文件的完整地址
				$("#LogoReview img").attr("src", fileUrl);
				$("#IconUrl").val(fileUrl);
				$("#IconUrlNo").val(fi.FileNo);
				$(".management .management-form .deal-form .icon-service li.cur").removeClass("cur");
			} else {
				alert("上传失败");
			}
		}
	});
	$.validator.setDefaults({ ignore: '' });
	$("#form").validate({
		rules: {
			Title: { required: true },
			Businesses: { required: true },
			Content: { required: true },
			IconUrl: { required: true },
			ServicePrice: { required: true, IsMatch: [/^\s*[1-9]+\d*\s*$/] }
		},
		messages: {
			Title: { required: "请一句话描述您的服务" },
			Businesses: { required: "请选择项目所需行业和业务" },
			Content: { required: "请描述您的服务具体内容" },
			IconUrl: { required: "请给您的服务配图" },
			ServicePrice: { required: "请填写您的服务售价", IsMatch: "请填写大于0的整数" }
		},
		errorElement: "span",
		errorPlacement: function (error, element) {
		    if ($(error).text() == "0") {
		        return;
		    } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
		        $("#" + $(element).attr("name") + "Msg").html(error);
		    } else {
		        $(element).nextAll("p").html(error);
		    }
		},
		success: "valid",
		submitHandler: function (form) {
			form.submit();
		}
	});
});