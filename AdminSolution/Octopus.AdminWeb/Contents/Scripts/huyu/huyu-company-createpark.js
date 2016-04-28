define("staticHuyu/huyu-company-createpark", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    if ($("#swfUploadButton").length != 0) {
        var upload = require("staticCommon/joy-upload");
        upload.create({
            button_placeholder_id: "swfUploadButton",
            file_types: "*.jpg;*.jpeg;*.png",
            file_types_description: "图片文件（*.jpg;*.jpeg;*.png）",
            file_size_limit: "200 KB",
            button_text: "",
            button_width: 158,
            button_height: 46,
            button_action: -100, //SWFUpload.BUTTON_ACTION.SELECT_FILE
            button_cursor: -2, //SWFUpload.CURSOR.HAND
            post_params: {
                'maxBytes': 1024 * 200
	        	, 'imageSize': true
                , 'minWidth': 276
                , 'minHeight': 207
                , 'enabled': true
            },
            upload_success_handler: function (file, serverData) {
                var data = $.parseJSON(serverData);
                if (data.ResultNo == 0) {
                    var fi = data.ResultAttachObject; //fi为上传成功后返回的文件信息
                    var fileUrl = fi.FileUrl; //文件的完整地址
                    var fileNo = fi.FileNo;//文件编码
                    $("#LogoReview").html("<img src=\"" + fileUrl + "\" alt=\"头像\" />");
                    $("#LogoUrl").val(fileNo).focus();
                } else {
                    alert("上传失败：" + data.ResultDescription);
                }
            }
        });
    }
    var companyValidateHelper = require("staticHuyu/huyu-company-validate");
    companyValidateHelper.InitCountryArea("Country");
    $.validator.setDefaults({ ignore: '' });
    $("#companyForm").validate({
        rules: {
            CompanyType: {
                required: true
            },
            Industry: {
                required: true
            },
            Businesses: {
                required: true
            },
            Country: {
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
            LogoUrl: {
                required: true
            },
            ContactName: {
                required: true,
                IsMatch: [/^[a-zA-Z\u2E80-\u9FFF]{2,20}$/]
            },
            ContactMobile: {
                optionalRequired: { "group": "phone" },
                isMobile: true
            },
            ContactPhone: {
                optionalRequired: {
                    "group": "phone", callback: function () {
                        return $("#countryCode").val() != "" && $("#areaCode").val() != "" && $("#phoneNumber").val() != "";
                    }
                },
                isCompanyPhone: true
            }
        },
        messages: {
            CompanyType: {
                required: "请选择您的园区身份"
            },
            Industry: {
                required: "请选择您的园区性质"
            },
            Businesses: {
                required: "请选择您的园区级别"
            },
            LogoUrl: {
                required: "请上传真实的园区头像"
            },
            ContactName: {
                required: "请填写联系人名称",
                IsMatch: "请输入2-20个字符，支持中文、英文"
            },
            ContactMobile: {
                optionalRequired: "手机和电话至少填写一项",
                isMobile: "请输入正确的手机号码"
            },
            ContactPhone: {
                optionalRequired: "手机和电话至少填写一项"
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