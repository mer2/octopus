define("staticHuyu/huyu-company-selectcategory", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    $(function () {
        $(".account .com-createstep1 .select-type a.btns").click(function () {
            var category = $(".select-type .cur input").val();
            if (category == null) {
                $.joy.showMsg("请选择类型");
            } else {
                location.href = "/Company/CreateCompany.html?category=" + category;
            }
            return false;
        });
    });
});