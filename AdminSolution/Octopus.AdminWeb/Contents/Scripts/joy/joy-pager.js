//分页插件
define("staticCommon/joy-pager", function(require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("staticCommon/joy-utils");
	var $ = require("jquery");
	var defaultOpts = {
		topPageContainer: "#topPageContainer",//分页顶部容器的ID
		bottomPageContainer: "#bottomPageContainer",//分页底部容器的ID
		pageSize: 10,//每页的大小
		itemCount: 0,//总记录数
		startIndex: 0,//起始记录位置
		displayPages: 3,//显示的页码数目
		templates: {//页码显示模板
			containerPage: '<div class="page-list"><ul class="clearfix">{0}</ul></div>', //页码容器
			firstPage: '<li><a href="javascript:$.joy.pager.gotoPage({0}, \'{1}\')">首页</a></li>', //第一页
			lastPage: '<li><a href="javascript:$.joy.pager.gotoPage({0}, \'{1}\')">末页</a></li>', //最后一页
			prePage: '<li><a href="javascript:$.joy.pager.gotoPage({0}, \'{1}\')">上页</a></li>', //上一页
			nextPage: '<li><a href="javascript:$.joy.pager.gotoPage({0}, \'{1}\')">下页</a></li>', //下一页
			numPage: '<li><a href="javascript:$.joy.pager.gotoPage({0}, \'{2}\')">{1}</a></li>', //数字页
			morePage: '<li><a href="javascript:$.joy.pager.gotoPage({0}, \'{1}\')">...</a></li>', //更多页面
			summaryPage: '<div class="sum_page f_l">共<span>{0}</span>项 共<span>{1}</span>页</div>', //汇总信息
			currentPage: '<li class="current"><a>{0}</a></li>'//当前页
		}
	};
	var thePage = function(opts) {
		if (typeof(opts) == 'string') {
			var content = opts;
			opts = { content: content };
		}
		opts = $.extend(true, {}, defaultOpts, opts);
		this.opts = opts;
		return this;
	};
	thePage.prototype = {
		gotoPage: function(i) {
			var opts = this.opts;
			if ($.isFunction(opts.gotoPage)) {
				opts.gotoPage(opts.pageSize * i);
			}
			return false;
		}
	};
	var defaultInstance = new thePage();
	var thisModule = {
		instances: {},
		options: defaultOpts,
		gotoPage: function (i, id) { //转到页
			var instance = null;
			if (id) {
				instance = thisModule.instances[id];
			}
			if (!instance) {
				instance = defaultInstance;
			}
			instance.gotoPage(i);
		},
		show: function(options) { //显示分页
			var instance = new thePage(options);
			options = instance.opts;
			if (instance.opts.itemCount <= 0) { //总记录数为0，不显示
				return;
			}
			var pageCount = Math.floor((options.itemCount - 1) / options.pageSize) + 1; //计算页数
			if (pageCount <= 1) { //只有一页，不显示
				return;
			}
			var instanceId = Math.random();
			var startIndex = options.startIndex;
			var currentPageIndex = Math.floor((startIndex - 1) / options.pageSize) + 1; //计算当前页
			if (currentPageIndex < 0) {
				currentPageIndex = 0;
			}
			if (currentPageIndex > (pageCount - 1)) {
				currentPageIndex = pageCount - 1;
			}
			var prePageIndex = currentPageIndex > 0 ? currentPageIndex - 1 : 0; //上一页
			var nextPageIndex = currentPageIndex < pageCount - 1 ? currentPageIndex + 1 : pageCount - 1; //下一页

			var numHtml = '';
			if (prePageIndex != currentPageIndex) {
				numHtml += options.templates.firstPage.format(0, instanceId);
				numHtml += options.templates.prePage.format(prePageIndex, instanceId);
			}
			var startPageIndex = currentPageIndex - options.displayPages;
			if (startPageIndex < 0) {
				startPageIndex = 0;
			}
			var endPageIndex = startPageIndex + options.displayPages * 2;
			if (endPageIndex > (pageCount - 1)) {
				endPageIndex = pageCount - 1;
			}
			startPageIndex = endPageIndex - options.displayPages * 2;
			if (startPageIndex < 0) {
				startPageIndex = 0;
			}
			if (startPageIndex > 0) {
				numHtml += options.templates.morePage.format(startPageIndex - 1, instanceId);
			}
			for (var k = startPageIndex; k <= endPageIndex; k++) {
				if (k == currentPageIndex) {
					numHtml += options.templates.currentPage.format(k + 1, instanceId);
				} else {
					numHtml += options.templates.numPage.format(k, k + 1, instanceId);
				}
			}
			if (endPageIndex < (pageCount - 1)) {
				numHtml += options.templates.morePage.format(endPageIndex + 1, instanceId);
			}
			if (nextPageIndex <= (pageCount - 1) && currentPageIndex < (pageCount - 1)) {
				numHtml += options.templates.nextPage.format(nextPageIndex, instanceId);
				numHtml += options.templates.lastPage.format(pageCount - 1, instanceId);
			}
			numHtml = options.templates.containerPage.format(numHtml);

			$(options.topPageContainer).html(numHtml);
			numHtml += options.templates.summaryPage.format(options.itemCount, pageCount);
			$(options.bottomPageContainer).html(numHtml);
			if ($.browser.msie && ($.browser.version == "6.0")) {
				var rwidth = 0, realwidth = [];
				$(options.bottomPageContainer).find(".page-list ul li").each(function() { realwidth.push($(this).width()); });
				for (var i in realwidth) {
					rwidth += realwidth[i];
				}
				$(options.bottomPageContainer).find(".page-list").width(rwidth + 6 * realwidth.length);
			}
			//保存实例
			thisModule.instances[instanceId] = instance;
		}
	};
	$.joy = $.extend({}, $.joy);
	$.joy.pager = thisModule;
	return thisModule;
});