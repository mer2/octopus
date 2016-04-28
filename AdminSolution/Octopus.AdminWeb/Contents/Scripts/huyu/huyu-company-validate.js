define("staticHuyu/huyu-company-validate", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    require("formValidate");
    var $ = require("jquery");
    var companyValidate = {};
    $.extend(companyValidate, {
        InitCountryArea: function (id) {
            $("#" + id).bind("change", function () {
                var country = $(this).children("option:selected").val();
                if (country == "海外") {
                    $(".china-other").hide();
                    $("#foreign").show();
                } else if (country == "中国") {
                    $(".china-other").show();
                    $("#foreign").hide();
                } else {
                    $(".china-other").hide();
                    $("#foreign").hide();
                }
            });
        },
        HasCompanyAddress: function (value, element) {
            var flag = true;
            if ($("#Country").val() == "") {
                $(element).removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请选择国家</span>');
                flag = false;
            } else if ($("#Country").val() == "中国" && $("#Province").val() == "") {
                $(element).removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请选择省份</span>');
                flag = false;
            } else if ($("#Country").val() == "中国" && $("#City").val() == "") {
                $(element).removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请选择城市</span>');
                flag = false;
            } else if ($("#Country").val() == "海外" && $("#foreign").val() == "") {
                $(element).removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请填写国家</span>');
                flag = false;
            } else {
                $(element).removeClass("valid").addClass("error").nextAll("p").html('<span class="valid"></span>');
            }
            if ($("#Address").length != 0) {
                if ($("#Country").val() == "中国" && $("#Address").val() == "") {
                    $("#Address").next().html('<span class="error">请填写单位详细地址</span>');
                    flag = false;
                } else {
                    $("#Address").next().html('<span class="valid"></span>');
                }
            }
            return flag;
        },
        IsCompanyPhone: function (value, element) {
            var validateor = $(element.form).validate();
            var flag = true;
            var countryCode = $("#countryCode");
            var areaCode = $("#areaCode");
            var phoneNumber = $("#phoneNumber");
            var extension = $("#extension");
            //if (countryCode.val() == "") {
            //    countryCode.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">国家代码/区号/电话号码不能为空</span>');
            //    flag = false;
            //} else if (!RegExp(/^\d{1,4}$/).test(countryCode.val())) {
            //    countryCode.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写国家代码</span>');
            //    flag = false;
            //} else {
            //    countryCode.removeClass("error").addClass("valid");
            //}
            //if (areaCode.val() == "") {
            //    areaCode.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">国家代码/区号/电话号码不能为空</span>');
            //    flag = false;
            //} else if (!RegExp(/^\d{1,4}$/).test(areaCode.val())) {
            //    areaCode.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写区号</span>');
            //    flag = false;
            //} else {
            //    areaCode.removeClass("error").addClass("valid");
            //}
            //if (phoneNumber.val() == "") {
            //    phoneNumber.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">国家代码/区号/电话号码不能为空</span>');
            //    flag = false;
            //} else if (!phoneNumber.val().match(/^\d{7,9}$/)) {
            //    phoneNumber.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写电话号码</span>');
            //    flag = false;
            //} else {
            //    phoneNumber.removeClass("error").addClass("valid");
            //}
            if (countryCode.val() != "" && !RegExp(/^\d{1,4}$/).test(countryCode.val())) {
                countryCode.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写国家代码</span>');
            } else if (areaCode.val() != "" && !RegExp(/^\d{1,4}$/).test(areaCode.val())) {
                areaCode.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写区号</span>');
            } else if (phoneNumber.val() != "" && !phoneNumber.val().match(/^\d{7,9}$/)) {
                phoneNumber.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写电话号码</span>');
            } else if (extension.val() != "" && !extension.val().match(/^\d{1,6}$/)) {
                extension.removeClass("valid").addClass("error").nextAll("p").html('<span class="error">请正确填写分机号码</span>');
            } else {
                if (this.settings.unhighlight) {
                    $("[name='" + element.name + "']").each(function (idx, item) {
                        validateor.settings.unhighlight.call(validateor, item, validateor.settings.errorClass, validateor.settings.validClass);
                    });
                }
                return this.optional(element) || true;
            }
            return false;
        },
        IsMatch: function (value, element, params) {
            var flag = true;
            if (value && value != "") {
                $.each(params, function (i, n) {
                    if (!RegExp(n).test(value)) {
                        flag = false;
                        return;
                    }
                });
            }
            return this.optional(element) || flag;
        },
        optionalRequired: function (value, element, options) {
            var group = options.group;
            var defCallback = function (val) { return $.trim(val) != ""; };
            if (!group) {
                return this.optional(element) || true;
            }
            var validateor = $(element.form).validate();
            var hasValidEle = false;
            var groupNames = [];
            $.each(validateor.settings.rules, function (key, val) {
                var optionalRequiredRule = val.optionalRequired;
                if (optionalRequiredRule) {
                    if (optionalRequiredRule.group == group) {
                        if ((optionalRequiredRule.callback || defCallback)($("[name='" + key + "']").val())) {
                            hasValidEle = true;
                        }
                        if (key != element.name) {
                            groupNames.push(key);
                        }
                    }
                }
            });
            if (hasValidEle) {
                $.each(groupNames, function (i, val) {
                    delete validateor.invalid[val];
                    $("[name='" + val + "']").each(function (idx, item) {
                        validateor.addWrapper(validateor.errorsFor(item)).not("." + validateor.settings.validClass).hide();
                        if (validateor.settings.unhighlight) {
                            validateor.settings.unhighlight.call(validateor, item, validateor.settings.errorClass, validateor.settings.validClass);
                        }
                    });
                });
            }
            return hasValidEle;
        }
    });
    $.validator.addMethod("HasCompanyAddress", companyValidate.HasCompanyAddress, "0");
    $.validator.addMethod("isCompanyPhone", companyValidate.IsCompanyPhone, "0");
    $.validator.addMethod("IsMatch", companyValidate.IsMatch, "内容不符合要求");
    $.validator.addMethod("optionalRequired", companyValidate.optionalRequired, "请至少填写一项");
    return companyValidate;
});