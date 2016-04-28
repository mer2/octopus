define("staticHuyu/huyu-company-parkvideo", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");    var params = require("plugin-params")(module, "huyu");
    var sp = require("staticHuyu/huyu-company-shipin");
    $(function () {
        sp.atlas(".contvideo", { step_width: 145, isajax: true, ajaxfun: function (start) { sp.GetMedias(params, start); } });
    });
});