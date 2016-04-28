define("staticHuyu/huyu-company-enterproauth", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var upload = require("staticCommon/joy-upload");
    upload.create({
        //upload_url: params.upload_url,
        button_placeholder_id: "swfUploadButton",
        file_types: "*.jpg;*.jpeg;*.png",
        file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
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
                $("#LogoReview img").attr("src", fileUrl);
                $("#IdentityData").val(fileNo).focus();
            } else {
                alert("上传失败：" + data.ResultDescription);
            }
        }
    });
    var companyValidateHelper = require("staticHuyu/huyu-company-validate");
    companyValidateHelper.InitCountryArea("Country");
    $.validator.setDefaults({ ignore: '' });
    $("#form").validate({
        rules: {
            Title: {
                required: true,
                rangelength: [2, 50],
                remote: {
                    type: "post",
                    url: "/AjaxJson/ValidateCompanyTitle",
                    data: {
                        title: function () {
                            return $("#Title").val();
                        }
                    }
                }
            },
            Country: {
                required: true,
                HasCompanyAddress: true
            },
            Province: {
                HasCompanyAddress: true
            },
            City: {
                HasCompanyAddress: true
            },
            Address: {
                HasCompanyAddress: true
            },
            foreign: {
                HasCompanyAddress: true
            },
            ContactName: {
                required: true,
                IsMatch: [/^[a-zA-Z\u2E80-\u9FFF]{2,20}$/]
            },
            ContactMobile: {
                required: true,
                isMobile: true
            },
            ContactEmail: {
                required: true,
                email: true
            },
            IdentityNo: {
                required: true,
                isIdCardNo: true
            },
            IdentityData: {
                required: true
            }
        },
        messages: {
            Title: {
                required: "请填写您的工作室名称",
                rangelength: "请保持在2-50个字符内",
                remote: "该名称已被认证，如对此有疑问，请联系客服"
            },
            IdentityData: {
                required: "请上传证件照片"
            },
            ContactName: {
                required: "请填写联系人名称",
                IsMatch: "请输入2-20个字符，支持中文、英文"
            },
            ContactMobile: {
                required: "请填写业务联系电话",
                isMobile: "请正确填写业务联系电话"
            },
            ContactEmail: {
                required: "请填写联系email",
                email: "请正确填写联系email"
            },
            IdentityNo: {
                required: "请填写您的身份证号",
                isIdCardNo: "请正确填写您的身份证号"
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