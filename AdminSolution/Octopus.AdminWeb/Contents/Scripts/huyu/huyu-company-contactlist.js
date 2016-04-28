define("staticHuyu/huyu-company-contactlist", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    //黄页管理后台联系方式操作显示隐藏效果
    $(function () {
        $(".management .management-cont .contact li").mouseover(function () {
            $(this).addClass("cur");
        }).mouseout(function () {
            $(this).removeClass("cur");
        });
    });
    // 绑定删除事件
    $("#contactlist .action a[title='删除']").click(function () {
        var ele = $(this);
        $.ajax({
            url: "/Company/DeleteContact",
            type: "post",
            data: { "id": ele.attr("lang") },
            dataType: "json",
            success: function (res) {
                if (res.ResultNo == 0) {
                    ele.parents("li").remove();
                    $(".contbox ul.contact li.add-pic").show().find("dd i").html(6 - $(".contbox ul.contact li").length);
                } else {
                    alert("删除失败");
                }
            },
            error: function () {
                alert("删除失败");
            }
        });
    });
});