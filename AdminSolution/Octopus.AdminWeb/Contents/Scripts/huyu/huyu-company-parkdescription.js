define("staticHuyu/huyu-company-parkdescription", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    $(function () {
        $("#btnEditFeature").click(function () {
            var category = $(this).closest("ul").find("li.cur").attr("lang");
            var title;
            switch (category) {
                case "1":
                    title = "交通";
                    break;
                case "2":
                    title = "车位";
                    break;
                case "3":
                    title = "银行";
                    break;
                case "4":
                    title = "餐饮";
                    break;
                case "5":
                    title = "住宿";
                    break;
                case "6":
                    title = "孵化器";
                    break;
                case "7":
                    title = "其他";
                    break;
                default:
                    title = "";
                    break;
            }
            if (title == "") {
                location.reload("/Company/Description");
                return false;
            }
            $("#park_feature_editor #Category").val(category);
            $("#park_feature_editor #Title").html(title);
            $("#park_feature_editor #Content").val("");
            $.ajax({
                url: "/Company/GetParkFeature.html",
                cache: false,
                type: "post",
                dataType: "json",
                data: {
                    category: category
                },
                success: function (res) {
                    $("#park_feature_editor #Content").val(res.Content);
                },
                complete: function () {
                    $.joy.layer({
                        layerId: "park_feature_editor"
                    });
                }
            });
            return false;
        });
    });
    $("#park_feature_editor :button").click(function () {
        $.joy.closeLayer("park_feature_editor");
    });
    $("#park_feature_editor :submit").click(function () {
        var category = $("#park_feature_editor #Category").val();
        var content = $("#park_feature_editor #Content").val();
        $.ajax({
            url: "/Company/EditParkFeature.html",
            cache: false,
            type: "post",
            dataType: "json",
            data: {
                category: category,
                content: content
            },
            complete: function () {
                $("#btnEditFeature").closest("div.sec-body").find("div.contbox:eq(" + category + ")").html(content);
                $.joy.closeLayer("park_feature_editor");
            },
            success: function (res) {
                if (res.Status == 0) {
                    $.joy.showMsg("保存成功");
                } else {
                    $.joy.showMsg("保存失败");
                }
            },
            error: function () {
                $.joy.showMsg("保存失败");
            }
        });
    });
});