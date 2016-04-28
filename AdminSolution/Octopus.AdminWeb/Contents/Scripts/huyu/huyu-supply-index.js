define("staticHuyu/huyu-supply-index", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var params = require("plugin-params")(module, "huyu");
    $("#pause").click(function () {
        $.joy.showConfirm("确定要暂停服务吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Supply/OffOn.html",
                    dataType: "json",
                    type: "post",
                    data: {
                        id: params.SupplyID
                    },
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            location.reload();
                        } else {
                            $.joy.showMsg("暂停失败");
                        }
                    },
                    error: function () {
                        $.joy.showMsg("暂停失败");
                    }
                });
            }
        });
        return false;
    });
    $("#delete").click(function () {
        $.joy.showConfirm("确定要取消服务吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Supply/Delete.html",
                    dataType: "json",
                    type: "post",
                    data: {
                        id: params.SupplyID
                    },
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            location.reload();
                        } else {
                            $.joy.showMsg("取消失败");
                        }
                    },
                    error: function () {
                        $.joy.showMsg("取消失败");
                    }
                });
            }
        });
        return false;
    });
});