//互娱内容添加 供应信息
define("staticHuyu/huyu-content-requirement", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		var swfu = $.joy.upload.create({
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
	});
	var ue = require("ueditor");
	var editor = ue.getEditor("requirementContent", {
		initialFrameWidth: "100%",
		toolbars: [[
					 'bold', 'italic', 'underline', 'removeformat', 'formatmatch', 'autotypeset', 'insertorderedlist', 'insertunorderedlist',
					 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', 'link', 'unlink', 'insertimage'
				]]
	});
	$(function () {
		if (params.msg == "OK") {
			layer.showMsg("操作成功！", function () {
				location.href = "/HuyuContent/Requirements.html";
			});
		} else if (params.msg == "Error") {
			layer.showMsg("操作失败！");
		}
		$("#unitType").val(params.CompanyCategory == '0' ? '1' : params.CompanyCategory);
		$("#layoutId").val(params.LayoutID);
		$(".manage-menu").eq(1).find(".sec-body li").eq(2).addClass("cur");
		$("#unitName").bind("focus", function () { $(this).next().html('填写您的单位全称') });
		$("#unitType").bind("focus", function () { $(this).next().html('请选择您单位的类型') });
		$("#identityType").bind("focus", function () { $(this).next().html('请选择您单位的对应身份，无营业执照请选"工作室"') });
		$("#industry").bind("focus", function () { $(this).next().html('请选择您的工作室所属行业') });
		$(".servicetype input").bind("focus", function () { $(".servicetype dt").css("color", "#666").html('请选择您的工作室的业务类型，可以勾选多个。') });
		$("#address").bind("focus", function () { $(this).next().html('请填写街道、门牌号、大厦名称等') });
		$("#mobile").bind("focus", function () { $(this).next().html('请填写真实的手机号，方便客服人员联系') });
		$("#fbxqxxForm").validate({
			rules: {
				title: {
					required: true,
					rangelength: [2, 50]
				},
				unitType: {
					required: true
				}
			},
			messages: {
				title: {
					required: "请填写名称",
					rangelength: "请保持在2-50个字符内"
				},
				unitType: {
					required: "请选择单位类型"
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
				editor.sync("requirementContent");
				if (editor.getContentTxt().replace(/\s/g, "") == "") {
					$(".text-editor").html('<p><span class="error">请填写内容</span></p>');
					return false;
				}
				if ($("#upload_file").val() == "") {
					$(".uploadpic p").html('<span class="error">请上传工作室图像</span>');
					$("html,body").animate({ scrollTop: $(".uploadpic").offset().top });
					return false;
				}
				form.submit();
			}
		});
	});
});