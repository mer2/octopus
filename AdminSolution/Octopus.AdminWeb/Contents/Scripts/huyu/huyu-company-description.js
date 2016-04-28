define("staticHuyu/huyu-company-description", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var upload = require("staticCommon/joy-upload");
    if ($("#swfUploadButton").length != 0) {
        upload.create({
            button_placeholder_id: "swfUploadButton",
            file_types: "*.jpg;*.jpeg;*.png",
            file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
            file_size_limit: "1MB",
            button_text: "",
            button_width: 158,
            button_height: 46,
            button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
            button_cursor: -2, //SWFUpload.CURSOR.HAND
            post_params: {
                'maxBytes': 1024 * 1024
	        	, 'imageSize': true
                , 'minWidth': 1
                , 'minHeight': 1
            },
            upload_success_handler: function (file, serverData) {
                var data = $.parseJSON(serverData);
                if (data.ResultNo == 0) {
                    var fi = data.ResultAttachObject; //fi为上传成功后返回的文件信息
                    var fileUrl = fi.FileUrl; //文件的完整地址
                    var fileNo = fi.FileNo; //文件编码
                    $("#LogoReview").html("<img src=\"" + fileUrl + "\" alt=\"头像\" />");
                    $("#LogoUrl").val(fileNo);
                } else {
                    alert("上传失败：" + data.ResultDescription);
                }
            },
            upload_error_handler: function (file, errNo, message) {
                var msg = "上传失败，请重试。";
                $.joy.upload.showMessage(msg);
            },
            file_queue_error_handler: function (file, errNo, message) {
                var msg = "您选择的文件 " + file.name + " 不符合要求，请检查文件类型和大小后重选，谢谢。";
                $.joy.upload.showMessage(msg);
            }
        });
    }
    if ($("#swfAddPic").length != 0) {
        upload.create({
            button_placeholder_id: "swfAddPic",
            file_types: "*.jpg;*.jpeg;*.png",
            file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
            file_size_limit: "2 MB",
            button_text: "",
            button_width: 98,
            button_height: 74,
            button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
            button_cursor: -2, //SWFUpload.CURSOR.HAND
            post_params: {
                'maxBytes': 1024 * 1024 * 2,
                'imageSize': true,
                'minWidth': 1,
                'minHeight': 1
            },
            upload_success_handler: function (file, serverData) {
                var data = $.parseJSON(serverData);
                if (data.ResultNo == 0) {
                    var fi = data.ResultAttachObject; //fi为上传成功后返回的文件信息
                    var fileUrl = fi.FileUrl; //文件的完整地址
                    $.ajax({
                        url: "/Company/AddComPic",
                        type: "post",
                        data: { "url": fileUrl },
                        dataType: "json",
                        success: function (res) {
                            if (res.ResultNo == 0) {
                                if ($("#com-pic dd").length < 10) {
                                    $("#com-pic dd").last().before('<dd><a href="javascript:void(0);"><img src="' + fileUrl + '" width="96" height="72" /><span lang="' + res.ResultAttachObjectEx + '"></span></a></dd>');
                                    $("#com-pic dt i").html(11 - $("#com-pic dd").length);
                                } else if ($("#com-pic dd").length == 10) {
                                    $("#com-pic dd").last().hide();
                                    $("#com-pic dd").last().before('<dd><a href="javascript:void(0);"><img src="' + fileUrl + '" width="96" height="72" /><span lang="' + res.ResultAttachObjectEx + '"></span></a></dd>');
                                    $("#com-pic dt i").html(0);
                                }
                            } else {
                                alert("上传失败");
                            }
                        },
                        error: function () {
                            alert("上传失败");
                        }
                    });
                } else {
                    alert("上传失败：" + data.ResultDescription);
                }
            }
        });
        // 绑定删除事件
        $("#com-pic").on("click", "dd[class!='add-pic'] span", function () {
            var ele = $(this);
            $.ajax({
                url: "/Company/DeleteComPic",
                type: "post",
                data: { "id": ele.attr("lang") },
                dataType: "json",
                success: function (res) {
                    if (res.ResultNo == 0) {
                        ele.parents("dd").remove();
                    } else {
                        alert("删除失败");
                    }
                },
                error: function () {
                    alert("删除失败");
                }
            });
        });
    }
    $.validator.setDefaults({ ignore: '' });
    $("#companyForm").validate({
        rules: {
            LogoUrl: "required",
            Description: "required"
        },
        messages: {
            LogoUrl: "请上传工作室图像",
            Description: "请填写简介"
        },
        success: "valid",
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
        submitHandler: function (form) {
            form.submit();
        }
    });
});