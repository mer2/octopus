//互娱内容添加 供应信息
define("staticHuyu/huyu-content-productcase", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		$("#comType").val(params.CompanyCategory == '0' ? '1' : params.CompanyCategory);
		$("#layoutId").val(params.LayoutID);
		$(".manage-menu").eq(1).find(".sec-body li").eq(3).addClass("cur");
	});
	var upload = require("staticCommon/joy-upload");
	upload.create({
		button_placeholder_id: "uploadfile"
		, file_types: "*.jpg;*.png;.gif;*.bmp;*.jpeg"
		, file_types_description: "图片文件（*.jpg;*.png;.gif;*.bmp;*.jpeg）"
		, file_size_limit: "300 KB"
		, button_text: ""
		, button_width: 168
		, button_height: 56
		, post_params: params.post_params
		, file_dialog_complete_handler: function (fileCount, fileQueued) {
			if (fileCount > 0) {
				this.startUpload();
			}
		}
		, upload_success_handler: function (file, serverData) {
			var obj = $.parseJSON(serverData);
			if (obj.ResultNo == 0) {
				$("#uploadImg").html('<img src="' + obj.ResultAttachObject.FileUrl + '" alt=""/>');
				$("#upload_file").val(obj.ResultAttachObject.FileUrl);
			} else {
				layer.showMsg("上传失败 " + obj.ResultDescription);
			}
		}
	});
	var ue = require("ueditor");
	var editor = ue.getEditor("productCaseContent", {
		initialFrameWidth: "100%",
		toolbars: [[
					 'bold', 'italic', 'underline', 'removeformat', 'formatmatch', 'autotypeset', 'insertorderedlist', 'insertunorderedlist',
					 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', 'link', 'unlink', 'insertimage'
				]]
	});
	$(function () {
		if (params.msg == "OK") {
			layer.showMsg("操作成功！", function () {
				location.href = "/HuyuContent/ProductCases.html";
			});
		} else if (params.msg == "Error") {
			layer.showMsg("操作失败！");
		}
		$("#fbcpalForm").validate({
			rules: {
				title: {
					required: true,
					rangelength: [2, 50]
				},
				comType: {
					required: true
				}
			},
			messages: {
				title: {
					required: "请填写案例名称",
					rangelength: "请保持在2-50个字符内"
				},
				comType: {
					required: "请选择案例企业类型"
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
				editor.sync("productCaseContent");
				if (editor.getContentTxt().replace(/\s/g, "") == "") {
					$(".text-editor").html('<p><span class="error">请填写内容</span></p>');
					return false;
				}
				if ($("#upload_file").val() == "") {
					$(".uploadpic p").html('<span class="error">请上传配图</span>');
					$("html,body").animate({ scrollTop: $(".uploadpic").offset().top });
					return false;
				}
				form.submit();
			}
		});
	});
});