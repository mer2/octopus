define("staticHuyu/huyu-company-chat", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var ju = require("staticCommon/joy-upload");
	var params = require("plugin-params")(module, "huyu");
	ju.attachedFiles(".attach-list");//显示附件
	ju.create({
		button_placeholder_id: "swfUploadButton",
		file_types: "*.png;*.jpg;*.txt;*.doc;*.docx;*.ppt;*.xls;*.xlsx;*.rar;*.zip;*.pdf",
		file_types_description: "PDF，EXCEL，WORD，图片格式",
		file_size_limit: "1 MB",
		button_text: "",
		button_width: 20,
		button_height: 20,
		button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
		button_cursor: -2, //SWFUpload.CURSOR.HAND
		post_params: $.extend(params.post_params, {
			"maxBytes": 1024 * 1024 * 1,//最大上传文件字节数，这是2MB
			"imageSize": false,//是否需要验证为图片
			limited: true//上传的文件需要安全控制，比如身份证或者是站内信附件等
		}),
		upload_success_handler: function (file, serverData) {
			var data = $.parseJSON(serverData);
			if (data.ResultNo != 0) {
				alert("上传失败：" + data.ResultDescription);
			}
		}
	});	 
});