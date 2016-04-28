define("staticHuyu/huyu-hire-create", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var upload = require("staticCommon/joy-upload");
	upload.create({
		//upload_url: params.upload_url,
		button_placeholder_id: "swfUploadButton",
		file_types: "*.jpg;*.jpeg;*.png;*.txt",
		file_types_description: "图片文件（*.jpg;*.jpeg;*.png;*.txt）",
		file_size_limit: "200KB",
		button_text: "",
		button_width: 158,
		button_height: 46,
		button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
		button_cursor: -2, //SWFUpload.CURSOR.HAND
		post_params: {
		    'maxBytes': 1024 * 200
	        , 'imageSize': true
			, 'minWidth': 1
			, 'minHeight': 1
			, limited: true
		},
		upload_success_handler: function (file, serverData) {
			var data = $.parseJSON(serverData);
			if (data.ResultNo == 0) {
				var fi = data.ResultAttachObject;//fi为上传成功后返回的文件信息
				var fileNo = fi.FileNo;//文件编码
				var fileUrl = fi.FileUrl;//文件的完整地址
				//$("#LogoReview").html("<img src=\"" + fileUrl + "\" alt=\"头像\" />");
				//$("#IdentityData").val(fileNo);
			} else {
				alert("上传失败：" + data.ResultDescription);
			}
		}
	});
	$.validator.setDefaults({ ignore: '' });
	$("#form").validate({
		rules: {
			Title: { required: true },
			Content: { required: true },
			ContactName: { required: true },
			ContactPhone: { required: true, isPhone: true }
		},
		messages: {
			Title: {
				required: "请填写项目描述"
			},
			Content: { required: "请填写具体内容" },
			ContactName: { required: "请填写您的联系方式" },
			ContactPhone: {
				required: "请填写您的联系方式",
				isPhone: "请正确填写您的联系方式"
			}
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