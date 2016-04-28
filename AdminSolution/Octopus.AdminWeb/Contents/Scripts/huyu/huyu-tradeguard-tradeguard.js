define("staticHuyu/huyu-tradeguard-tradeguard", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var tools = require("staticHuyu/huyu-company-tools");
    var layer = require("layer");
    $("#btnPayTradeDeposit").click(function () {
        layer.layer({ layerId: "PayTradeDeposit-layer" });
    });
    $(".clsRefundTradeDeposit").click(function () {
        tools.ShowLayer({
            LayerID: "RefundTradeDeposit-layer",
            SubmitID: "#RefundTradeDeposit-layer .submitBtn",
            SubmitFun: function () {
                $.ajax({
                    url: "/TradeGuard/RefundTradeDeposit",
                    type: "post",
                    dataType: "json",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            layer.layer({ layerId: "Success-layer" });
                        } else {
                            layer.showMsg(res.Message || "关闭失败,请重试或联系客服", function () {
                                location.reload();
                            });
                        }
                    },
                    error: function () {
                        layer.showMsg("关闭失败,请重试或联系客服", function () {
                            location.reload();
                        });
                    }
                });
            }
        });
    });
});