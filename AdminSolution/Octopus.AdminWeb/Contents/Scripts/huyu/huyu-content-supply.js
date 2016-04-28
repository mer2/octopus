//互娱内容添加 供应信息
define("staticHuyu/huyu-content-supply", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	require("formValidate");
	var layer = require("layer");
	var params = require("plugin-params")(module, "huyu");
	$(function () {
		if (params.msg == "OK") {
			layer.showMsg("操作成功！", function () {
				location.href = "/HuyuContent/Supplies.html";
			});
		} else if (params.msg == "Error") {
			layer.showMsg("操作失败！");
		} 
		$("#industry").val(params.industry == '0' ? "" : params.industry);
		$("#layoutId").val(params.LayoutID);
		$(".manage-menu").eq(1).find(".sec-body li").eq(1).addClass("cur");
		GetBusiness($("#industry"), true);
		$(".servicetype :checkbox").each(function () {
			var array = params.Businesses.split(" ");
			if (array.indexOf(this.value)) {
				this.checked = true;
			}
		});
		$("#industry").change(function () {
			GetBusiness(this, false);
		});
	});

	function GetBusiness(_this, bl) {
		var parentNo = $(_this).val();
		$.post("/AjaxJson/GetItemDictionaries", { category: "Companies.Businesses", parentNo: parentNo + "1" }, function (data) {
			if (data.ResultNo == 0) {
				var obj = data.ResultAttachObjectEx;
				var html = "";
				for (var i = 0; i < obj.length; i++) {
					html += '<div>';
					html += '<input type="checkbox" value="' + obj[i].Title + '" ' + IsChecked(obj[i].Title, bl) + ' id="checkbox' + obj[i].ItemNo + '" name="business" />';
					html += '<label for="checkbox' + obj[i].ItemNo + '">' + obj[i].Title + '</label>';
					html += '</div>';
				}
				$(_this).parents("tr").next().show().find("dd").html(html);
			} else {
				$(_this).parents("tr").next().hide().find("dd").html("");
			}
		});
	}

	function IsChecked(val, bl) {
		if (bl) {
			var businesses = " " + params.Businesses + " ";
			if (businesses.indexOf(" " + val + " ") >= 0) {
				return 'checked="checked"';
			}
		}
		return "";
	}

	var ue = require("ueditor");
	var editor = ue.getEditor("supplyContent", {
		initialFrameWidth: "100%",
		maxInputCount: 1,
		toolbars: [['source',
					 'bold', 'italic', 'underline', 'removeformat', 'formatmatch', 'autotypeset', 'insertorderedlist', 'insertunorderedlist',
					 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', 'link', 'unlink', 'insertimage'
				]]
	});
	var upload = require("staticCommon/joy-upload");
	var swfu = upload.create({
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
					showMsg("上传失败 " + obj.ResultDescription);
				}
			}
	});

	$(function () {
		$("#fbgyxxForm").validate({
			rules: {
				title: {
					required: true,
					rangelength: [2, 50]
				},
				industry: {
					required: true
				}
			},
			messages: {
				title: {
					required: "请填写名称",
					rangelength: "请保持在2-50个字符内"
				},
				industry: {
					required: "请选择行业"
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
				if ($(".servicetype input[type=checkbox]").length > 0 && $(".servicetype input[type=checkbox]:checked").length == 0) {
					$(".servicetype dt").css("color", "red").html("请选择至少一个行业");
					$("html,body").animate({ scrollTop: $(".servicetype dt").offset().top });
					return false;
				}
				editor.sync("supplyContent");
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