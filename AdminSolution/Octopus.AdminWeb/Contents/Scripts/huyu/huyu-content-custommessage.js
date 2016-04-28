//互娱内容添加 供应信息
define("staticHuyu/huyu-content-custommessage", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    require("formValidate");
    var layer = require("layer");
    var params = require("plugin-params")(module, "huyu");
    var locations = require("staticCommon/joy-location");
    $(function () {
        $("#layoutId").val(params.LayoutID);
        $(".manage-menu").eq(1).find(".sec-body li").eq(6).addClass("cur");
        $("#country").change(function () {
            if ($(this).val() == "海外") {
                $("#foreign").show();
                $("#province").hide();
                $("#city").hide();
            } else {
                $("#foreign").hide();
                $("#province").show();
                $("#city").show();
            }
        });
        if (params.Country == "海外") {
            locations.init({ country: "海外", province: null, city: null });
            $("#foreign").val(params.Province).show();
            $("#province").hide();
            $("#city").hide();
        } else {
            locations.init({ country: "中国", province: params.Province, city: params.City, nohide: true });
            $("#foreign").hide();
        }
        if (params.msg == "OK") {
            layer.showMsg("操作成功！", function () {
                location.href = "/HuyuContent/CustomMessages.html";
            });
        } else if (params.msg == "Error") {
            layer.showMsg("操作失败！");
        }
    });
    if ($("#newsLabel").val() == '') $(".tips a").removeClass("cur");
    $(".tips a").bind("click", function () {
        $(this).addClass("cur").siblings("a").removeClass("cur");
        $("#newsLabel").val($(this).html());
    });

    var ue = require("ueditor");
    var editor = ue.getEditor("ubbContent", {
        initialFrameWidth: "100%",
        toolbars: [[
					 'bold', 'italic', 'underline', 'removeformat', 'formatmatch', 'autotypeset', 'insertorderedlist', 'insertunorderedlist',
					 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', 'link', 'unlink', 'insertimage'
        ]]
    });
    $(function () {
        $("#zxyxxForm").validate({
            rules: {
                title: {
                    required: true
                },
                source: {
                    required: true
                },
                author: {
                    required: true
                },
                summary: {
                    required: true
                }
            },
            messages: {
                title: {
                    required: "请填写标题"
                },
                source: {
                    required: "请填写来源出处"
                },
                author: {
                    required: "请填写作者"
                },
                summary: {
                    required: "请填写摘要"
                }
            },
            errorElement: "span",
            errorPlacement: function (error, element) {
                if ('country_province_city_area'.indexOf(element[0].id) > -1) {
                    $(".validate_area").css('left', 300).html(error);
                } else {
                    element.next().html(error);
                }
            },
            success: "valid",
            submitHandler: function (form) {
                editor.sync("ubbContent");
                if (editor.getContentTxt().replace(/\s/g, "") == "") {
                    $(".text-editor").html('<p><span class="error">请填写内容</span></p>');
                    return false;
                }
                if ($("#country").val() == "") {
                    $(".validate_area").html('<span class="error">请选择国家</span>');
                    $("html,body").animate({ scrollTop: $(".validate_area").offset().top });
                    return false;
                } else {
                    if ($("#country").val() == "中国" && ($("#province").val() == "" || $("#city").val() == "")) {
                        $(".validate_area").html('<span class="error">请选择省市或地区</span>');
                        $("html,body").animate({ scrollTop: $(".validate_area").offset().top });
                        return false;
                    } else if ($("#country").val() == "海外" && $("#foreign").val() == "") {
                        $(".validate_area").html('<span class="error">请填写你的国家</span>');
                        $("html,body").animate({ scrollTop: $(".validate_area").offset().top });
                        return false;
                    }
                }
                form.submit();
            }
        });
    });
});