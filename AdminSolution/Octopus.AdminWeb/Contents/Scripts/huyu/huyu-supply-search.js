define("staticHuyu/huyu-supply-search", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    $("#Province").change(function () {
        $("#SearchForm").submit();
        return false;
    });
});