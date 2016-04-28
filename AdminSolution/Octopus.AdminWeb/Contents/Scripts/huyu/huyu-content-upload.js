//互娱内容添加 供应信息
define("staticHuyu/huyu-content-upload", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	var upload = require("staticCommon/joy-upload");
	var swfu = upload.create({
		button_placeholder_id: "uploadfile"
			, file_types: "*.jpg;*.png;.gif;*.bmp;*.jpeg"
			, file_types_description: "图片文件（*.jpg;*.png;.gif;*.bmp;*.jpeg）"
			, file_size_limit: "1024 * 1024 * 2"
			, button_text: ""
			, button_width: 168
			, button_height: 56
			, post_params: params.post_params
			, file_dialog_complete_handler: function (fileCount, fileQueued) {
				if (fileCount > 0) {
					this.startUpload();
					$(".upload-list").show();
					$(".upload-btn").show();
				}
			}, upload_progress_handler: function (file, bytesLoaded) {
				FileUploadStart(file); //自定义操作上传中
				var v = "uploadfile_" + file.id;
				var percent = Math.ceil((bytesLoaded / file.size) * 100);
				UploadAnimate(v, percent);
			}
			, upload_success_handler: function (file, serverData) {
				var data = $.parseJSON(serverData);
				FileUploadSuccess(file, data);
			}
	});
	$(function () {
		$(".manage-menu").eq(1).find(".sec-body li").eq(4).addClass("cur");
		$("#submitBtn").click(function () {
			var ids = "";
			$(".uploads .upload-box").each(function () {
				ids += $(this).attr("lang") + ",";
			});
			if (ids) {
				$("#ids").val(ids);
				$("form").submit();
			} else {
				layer.showMsg("请上传图片！");
			}
		});
		if (params.msg == "OK") {
			layer.showConfirm('上传成功，继续上传？', {
				cancelfun: function () {
					location.href = "/HuyuContent/Pics.html";
				}
			});
		}
		$(".uploads").on("click", ".del", null, function () { 
			var fileName = $(this).attr("lang"); 
			DeleteImage(fileName);
			return false;
		});
	});

	function FileUploadStart(file) {
		if ($(".uploads .upload-box").length >= 10) {
			return;
		}
		$("#submitBtn").attr("disabled", true);
		var fileName = "uploadfile_" + file.id;
		if ($("#" + fileName).html() == null) {
			var startHtml = '<li id="' + fileName + '">';
			startHtml += '<div class="clearfix">';
			startHtml += '<label class="pic-name">' + file.name + '</label>';
			startHtml += '<div class="info">上传中，<span>1%</span></div>';
			startHtml += '</div>';
			startHtml += '<div class="progress-box">';
			startHtml += '<div class="progress-bg"><span style="width: 1%" class="progress"></span>';
			startHtml += '</div>';
			startHtml += '</div>';
			startHtml += '</li>';
			$(".upload-loading ul").append(startHtml);
		}
		if ($("." + fileName).html() == null) {
			var sussesHtml = "";
			sussesHtml += '<div lang="' + file.id + '" class="upload-box ' + fileName + '" style=" display:none;">';
			sussesHtml += '<a class="del" lang="' + fileName + '" href="javascript:void(0)">删除</a>';
			sussesHtml += '<div class="pic f-l">';
			sussesHtml += '<img alt="" src="">';
			sussesHtml += '<input type="hidden" value="" id="" name="fileUrl_' + file.id + '" />';
			sussesHtml += '<input type="hidden" value="" id="" name="fileNo_' + file.id + '" />';
			sussesHtml += '<input type="hidden" value="" id="" name="remark1_' + file.id + '" />';
			sussesHtml += '<input type="hidden" value="" id="" name="remark2_' + file.id + '" />';
			sussesHtml += '</div>';
			sussesHtml += '<div class="info f-r">';
			sussesHtml += '<div class="description">';
			sussesHtml += '图片描述：<br>';
			sussesHtml += '<input type="text" name="description_' + file.id + '" />';
			sussesHtml += '</div>';
			sussesHtml += '</div>';
			sussesHtml += '</div>';
			$(".uploads").append(sussesHtml);
		}
	}

	function UploadAnimate(id, percent) {
		if (percent < 100) {
			percent += 1;
			$("#" + id).find(".info span").html(percent + "%");
			$("#" + id).find(".progress-bg span").attr("style", "width: " + percent + "%");
		}
	}

	function FileUploadSuccess(file, data) {
		$("#submitBtn").attr("disabled", false);
		var fileName = "uploadfile_" + file.id;
		if (data.ResultNo == 0) {
			var obj = data.ResultAttachObject;
			$("#" + fileName).remove();
			var $file = $("." + fileName);
			$file.find("input").eq(0).val(obj.FileUrl);
			$file.find("input").eq(1).val(obj.FileNo);
			$file.find("input").eq(2).val(obj.Width + "*" + obj.Height);
			$file.find("input").eq(3).val(obj.ContentLength);
			$file.find("img").attr("src", obj.FileUrl);
			$file.show();
		} else {
			layer.showMsg("上传失败 " + data.ResultDescription, function () {
				DeleteImage(fileName);
			});
		}
	}

	function DeleteImage(fileName) {
		$("." + fileName).remove();
		$("#" + fileName).remove();
	}
});