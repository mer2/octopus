define("staticHuyu/huyu-company-policydetail", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    $("#form").validate({
        rules: {
        },
        messages: {
        },
        success: "valid",
        errorElement: "span",
        errorPlacement: function (error, element) {
            if (error.length != 0 && error[0].innerText == "0") {
                return;
            } else if ($("#" + element[0].id + "Msg").length != 0) {
                $("#" + element[0].id + "Msg").html(error);
            } else {
                $(element).nextAll("p").html(error);
            }
        },
        submitHandler: function (form) {
            if ($("#Comment").val() == "") {
                $.joy.showMsg("请填写评论");
                return false;
            }
            form.submit();
            return true;
        }
    });
});