define("staticHuyu/huyu-inform", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	var nav = require("staticHuyu/huyu-navigate");
	var tools = require("staticHuyu/huyu-company-tools");
	var config = require("staticHuyu/huyu-config");
	var $layer = require("layer");
	var inforLayerId = "huyu-inform-layer";
	var inforHtml = '<div class="pop-layer business-action ui-draggable" id="' + inforLayerId + '">' +
        '<div class="pop-tem">' +
            '<div class="pop-head">' +
                '<h2>举报</h2>' +
                '<div class="close"><a href="javascript:;" title="关闭">关闭</a></div>' +
            '</div>' +
            '<div class="pop-body">' +
                '<div class="contbox">' +
                    '<table class="form">' +
                        '<tbody><tr>' +
                            '<td>您要举报的是 <span></span> 。</td>' +
                            '<td class="radiobox">' +
                                '<div class="title">' +
                                    '<label>请选择举报类型：</label>' +
                                '</div>' +
                                '<ul class="clearfix">' +
                                    '<li>' +
                                        '<input id="huyuinformtype1" name="huyuinformtype" type="radio" />' +
                                        '<label for="huyuinformtype1">发布垃圾广告</label></li>' +
                                    '<li>' +
                                        '<input id="huyuinformtype2" name="huyuinformtype" type="radio" />' +
                                        '<label for="huyuinformtype2">发布色情信息</label></li>' +
                                    '<li>' +
                                        '<input id="huyuinformtype3" name="huyuinformtype" type="radio" />' +
                                        '<label for="huyuinformtype3">发布敏感信息</label></li>' +
                                    '<li>' +
                                        '<input id="huyuinformtype4" name="huyuinformtype" type="radio" />' +
                                        '<label for="huyuinformtype4">发布虚假信息</label></li>' +
                                    '<li>' +
                                        '<input id="huyuinformtype5" name="huyuinformtype" type="radio" />' +
                                        '<label for="huyuinformtype5">其它</label> ' +
                                        '<input type="text" value="" /></li>' +
                                '</ul>' +
                            '</td>' +
                        '</tr>' +
                        '<tr>' +
                            '<td class="form-btn">' +
                                '<input class="btns btn-h37-b" type="submit" value="确 认">' +
								'<input class="btns btn-h37-a close" type="reset" value="取 消"></td>' +
                        '</tr>' +
                    '</tbody></table>' +
                '</div>' +
            '</div>' +
        '</div>' +
    '</div>';
	exports.HuyuInform = function (ele, success, error) {
		var _params = $.parseJSON(ele.attr("lang"));
		if (_params.Type && _params.Value && _params.Title) {
			nav.showLogin(function () {
				if ($("#" + inforLayerId).length == 0) {
					$("body").append(inforHtml);
				}
				$("#" + inforLayerId + " table tr:eq(0) td:eq(0) span").html(_params.Title);
				$("#" + inforLayerId + " [name='huyuinformtype']").attr("checked", false);
				$("#" + inforLayerId + " #huyuinformtype5").next().next().val("");
				tools.ShowLayer({
					"LayerID": inforLayerId,
					"SubmitID": "#" + inforLayerId + " input[type='submit']",
					"SubmitFun": function () {
						var ckt = $("#" + inforLayerId + " [name='huyuinformtype']:checked");
						var content = ckt.next().html();
						if (!content) {
							$.joy.showMsg("请选择举报类型");
							return false;
						}
						if (content == "其它") {
							content = $.trim(ckt.next().next().val());
							if (content == "") {
								$.joy.showMsg("请填写举报内容");
								return false;
							}
						}
						$.ajax({
							url: config.urls.myUrl + "/Message/UserInform.html",
							dataType: "jsonp",
							data: { InformValue: _params.Value, InformType: _params.Type, InformContent: content },
							type: "get",
							success: function (res) {
								if (res && res.ResultNo == 0) {
									success(ele, res);
								} else {
									error(ele, res);
								}
							},
							error: function () {
								error(ele);
							},
							complete: function () {
								$.joy.closeLayer(inforLayerId);
							}
						});
						return false;
					}
				});
			});
		}
	};
	(function () {
		var opts = $.extend({
			selector: ".huyuinform",
			success: function () {
				$layer.showMsg('举报成功');
			},
			error: function (btn, msg) {
				if (msg != undefined) {
					$layer.showMsg(msg.Message);
				} else {
					$layer.showMsg("未知错误");
				}
			}
		}, params);
		$(opts.selector).click(function () {
			exports.HuyuInform($(this), opts.success, opts.error);
		});
	})();
});