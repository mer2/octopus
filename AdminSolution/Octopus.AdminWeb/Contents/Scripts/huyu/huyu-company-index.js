define("staticHuyu/huyu-company-index", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var params = require("plugin-params")(module, "huyu");
    $(function () {
        switch (params.ContentType) {
            case 5:
                var tp = require("staticHuyu/huyu-company-sjzs");
                tp.atlas(".contatlas", { step_width: 145, isajax: true, ajaxfun: function (start) { tp.GetMedias(params, start); } });
                break;
            case 6:
                var sp = require("staticHuyu/huyu-company-shipin");
                sp.atlas(".contvideo", { step_width: 145, isajax: true, ajaxfun: function (start) { sp.GetMedias(params, start); } });
                break;
            case 9:
                require("staticHuyu/huyu-company-contactlist");
                break;
        }
    });
});