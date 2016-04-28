define("staticHuyu/huyu-hire-hire", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var ju = require("staticCommon/joy-upload");
	require("staticHuyu/huyu-hire");
	ju.attachedFiles(".attach-list1"); //显示附件
	ju.create({
		button_placeholder_id: "swfUploadButton",
		file_types: "*.jpg;*.jpeg;*.png",
		file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
		file_size_limit: "200KB",
		button_text: "",
		button_width: 20,
		button_height: 20,
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
	//  补充说明
	$("#AddRemark").click(function () {
		$.joy.layer({ layerId: "remark-layer", close_layer: "#remark-layer .resetBtn" });
	});
	$("#remark-layer .submitBtn").click(function () {
		$.ajax({
			url: "/Hire/AddHiredOfferProperty.html",
			type: "post",
			dataType: "json",
			data: {
				OfferNo: $("#OfferNo").val(),
				PropertyType: $("#PropertyType").val(),
				PropertyTitle: $("#PropertyTitle").val(),
				PropertyValue: $("#PropertyValue").val(),
				RichValue: $("#RichValue").val()
			},
			success: function (res) {
				if (res.ResultNo == 0) {
					//var p = res.ResultAttachObjectEx;
					//$("#OfferRemarkList").append('<dd>' +
					//	'<pre>' + p.RichValue + '</pre>' +
					//		'<ul class="attach-list clearfix attach-list1" lang="HiredOfferProperties:' + p.ID + '" id="OfferRemark' + p.ID + '">' +
					//		'</ul>' +
					//		'<div class="date">' + tools.DateToString(p.CreateTime) + '</div>' +
					//	'</dd>');
					//ju.attachedFiles("#OfferRemark" + p.ID);
				    $.joy.showMsg("保存成功", function () {
						location.reload();
					});
				} else {
				    $.joy.showMsg("保存失败");
				}
			},
			error: function () {
			    $.joy.showMsg("保存失败");
			},
			complete: function () {
				$.joy.closeLayer("remark-layer");
			}
		});
	});
});