define("staticHuyu/huyu-company-contact", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    require("staticHuyu/huyu-company-validate");
    $("#companyForm").validate({
        rules: {
            ContactName: {
                required: true,
                IsMatch: [/^[a-zA-Z\u2E80-\u9FFF]{2,20}$/]
            },
            Mobile: {
                optionalRequired: { "group": "phone" },
                isMobile: true
            },
            Email: {
                required: true,
                email: true
            },
            Telephone: {
                optionalRequired: {
                    "group": "phone", callback: function () {
                        return $("#countryCode").val() != "" && $("#areaCode").val() != "" && $("#phoneNumber").val() != "";
                    }
                },
                isCompanyPhone: true
            }
        },
        messages: {
            ContactName: {
                required: "请填写你联系人姓名",
                IsMatch: "请输入2-20个字符，支持中文、英文"
            },
            Mobile: {
                optionalRequired: "手机和电话至少填写一项",
                isMobile: "请正确填写业务联系手机"
            },
            Email: {
                required: "请填写业务联系邮箱",
                email: "请正确填写业务联系邮箱"
            },
            Telephone: {
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