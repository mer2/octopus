define("staticHuyu/huyu-company-parkpicture", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var params = require("plugin-params")(module, "huyu");
    var tp = require("staticHuyu/huyu-company-sjzs");
    $(function () {
        tp.atlas(".contatlas", { step_width: 145, isajax: true, ajaxfun: function (start) { tp.GetMedias(params, start); } });
    });
});