define("staticHuyu/huyu-mytrading-publishtenderoffer1", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var $layer = require("layer");
	var $upload = require("staticCommon/joy-upload");
	var params = require("plugin-params")(module, "huyu");
	//选择框初始化
	$(function () {
		$("#title").focus(function () {
			$(this).removeClass("error").next("p").hide().html("");
		});
		$("#title").blur(function () {
			if ($(this).val() == "") {
				$(this).addClass("error").next("p").html('<span class="error">请输入一句话描述</span>').show();
			}
		});
		$("#tenderContent").focus(function () {
			$(this).removeClass("error").next("p").hide().html("");
		});
		$("#tenderContent").blur(function () {
			if ($(this).val() == "") {
				$(this).addClass("error").next("p").html('<span class="error">请输入您项目的具体描述</span>').show();
			}
		});
		$("#contactName").focus(function () {
			if ($(this).val() == "联系人") {
				this.value = "";
			}
			$(this).removeClass("error").next("p").hide().html("");
		});
		$("#contactName").blur(function () {
			if ($(this).val() == "" || $(this).val() == "联系人") {
				$(this).addClass("error").val("联系人").parent().find("p").html('<span class="error">请填写联系人</span>').show();
			}
		});
		$("#contactPhone").focus(function () {
			if ($(this).val() == "联系电话") {
				this.value = "";
			}
			$(this).removeClass("error").next("p").hide().html("");
		});
		$("#contactPhone").blur(function () {
			var val = parseInt($(this).val()) + "";
			if (val.length != 11) {
				$(this).addClass("error").parent().find("p").html('<span class="error">请输入正确的联系电话</span>').show();
			}
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
		$("#submitBtn").click(function () {
			var canSubmit = true;
			if ($("#title").val() == "") {
				$("#title").addClass("error").next("p").html('<span class="error">请输入一句话描述</span>').show();
				canSubmit = false;
			}
			if ($("#companyCategory").val() == "" || $("#industry").val() == "" || $("#businesses").val() == "") {
				$("#businesses").next("p").html('<span class="error">请选择项目所需行业和业务</span>').show();
				$("#category-industry-businesses").addClass("error");
				canSubmit = false;
			}
			if ($("#tenderContent").val() == "") {
				$("#tenderContent").addClass("error").next("p").html('<span class="error">请输入您项目的具体描述</span>').show();
				canSubmit = false;
			}
			if ($("#contactName").val() == "" || $("#contactName").val() == "联系人") {
				$("#contactName").addClass("error").parent().find("p").html('<span class="error">请填写联系人</span>').show();
				canSubmit = false;
			}
			var contactPhone = parseInt($("#contactPhone").val()) + "";
			if (contactPhone.length != 11) {
				$("#contactPhone").addClass("error").parent().find("p").html('<span class="error">请输入正确的联系电话</span>').show();
				canSubmit = false;
			}
			//if ($("#savetags").val() == "") {
			//	//$("#tags").addClass("error").next("p").html('<span class="error">请给您的项目添加标签</span>').show();
			//	//canSubmit = false;
			//}
			if (canSubmit) {
				$("form").submit();
			}
		});
	});

	var setting = require("staticHuyu/huyu-config");

	$upload.attachedFiles(".attach-list");//显示附件            
	var swfu = $upload.create({                //
		upload_url: setting.urls.uploadUrl + "/Joy/Upload",
		button_placeholder_id: "swfUploadButton",
		file_types: "*.jpg;*.png;*.txt;*.doc;*.docx;*.ppt;*.pptx;*.xls;*.xlsx;*.pdf;",//*.rar;*.zip",
		file_types_description: "",
		file_size_limit: "5 MB",
		button_text: "",
		button_width: 200,
		button_height: 20,
		button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE                
		button_cursor: -2, //SWFUpload.CURSOR.HAND               
		post_params: params.post_params,
		upload_success_handler: function (file, serverData) {
			var data = $.parseJSON(serverData);
			if (data.ResultNo == 0) {

			} else {
				$layer.showMsg("上传失败：" + data.ResultDescription);
			}
		}
	});
});