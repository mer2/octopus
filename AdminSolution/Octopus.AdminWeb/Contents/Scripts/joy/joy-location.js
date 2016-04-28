//国家省市地区操作类库
define("staticCommon/joy-location", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	var locationData = require("staticCommon/joy-location-data");
	var init = function (initData) {
		if (!initData) {
			return;
		}
		var options = $.extend({
			nohide: false, //不自动隐藏
			emptyText: "（请选择）"//未选中时显示的文字
		}, initData.options);
		var items = [], i = 0;
		$.map(initData, function (value, key) {
			if (key == "options") {
				return;
			}
			var item = items[i++] = $("#" + key);
			item.data("value", value);
			item.change(function () {
				var it = $(this);
				var child = it.data("child");
				if (!child) {
					return;
				}
				var opt = it.children("option:selected");
				var val = opt ? opt.attr("lang") : null;
				fillData(val, child);
				child.change();
			});
		});
		if (i <= 0) {//没有数据？
			return;
		}
		for (var j = 0; j < i - 1; j++) {
			items[j].data("child", items[j + 1]);
		}

		function fillData(parent, current) {
			var value = current.data("value");
			current.empty().append('<option value="">' + options.emptyText + '</option>');
			var count = 0; //有多少项
			$.map(locationData, function (item, key) {
				var cnname = item[0]; //中文名
				var parentKey = item[1]; //上级
				//var pyname = value[2]; //拼音名（英文名）
				if (parent == parentKey) {
					count++;
					if (value == cnname) {
						current.append('<option lang="' + key + '" selected="selected">' + cnname + '</option>');
					} else {
						current.append('<option lang="' + key + '">' + cnname + '</option>');
					}
				}
			});
			if (count == 1) { //如果仅有一项，直接选中
				$(current.children("option")[1]).attr("selected", "selected");
			}
			if (!options.nohide) {
				//没有选项，则隐藏
				count == 0 ? current.hide() : current.show();
			}
			return current;
		}

		//填充国家数据
		if (items[0].length) {//没有国家的选项？
			fillData('0', items[0]).change();
		} else {
			fillData('1', items[1]).change();
		}
	};
	if (params) {//有参数，则立即初始化
		$(function() {
			init(params);
		});
	}
	return {
		init: init,
		locationData: locationData
	};
});