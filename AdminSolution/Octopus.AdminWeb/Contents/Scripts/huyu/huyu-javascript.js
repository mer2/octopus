define("staticHuyu/huyu-javascript", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	//回到顶部效果
	$(".gotop").remove();
	
	function getScroll() {
		var sTop = document.body.scrollTop + document.documentElement.scrollTop;
		if (sTop == 0) { $("#joy_gotop").hide(); } else {
			if ($(".wp").width()) {
				if ($(".fullscreen").length > 0) {
					$("#joy_gotop").css({ "margin-left": ($(window).width() / 2 - 71) + 'px' }).show();
				} else { $("#joy_gotop").css({ "margin-left": ($(".wp").width() / 2 + 30) + 'px' }).show(); }
			}
		}
	}
	if (!$("#joy_gotop").length && !$("#specail_gotop").length) {
		$('<div id="joy_gotop"></div>').appendTo($('body')).html('<a href="javascript:;" class="joy_gotop" title="回到顶部"></a</div>');
		getScroll();
		$(window).scroll(function () {
			getScroll();
		});
		$("#joy_gotop .joy_gotop").click(function() {
			if ($("#top").length) {
				$("html,body").animate({ scrollTop: $("#top").offset().top }, 'slow');
			} else {
				$("html,body").animate({ scrollTop: $(".header-nav").offset().top }, 'slow');
			}
		});
	};

    //公共头展开效果
	$(function () {
	    $("#header .header-nav .account-action li.loggedin").mouseover(function () {
	        $(this).addClass("info-hover");
	    });
	    $("#header .header-nav .account-action li.loggedin").mouseout(function () {
	        $(this).removeClass("info-hover");
	    });
	});
	$(function () {
		$("#header .header-nav .account-action li.navigateMessage").mouseover(function () {
			$(this).addClass("msg-hover");
		});
		$("#header .header-nav .account-action li.navigateMessage").mouseout(function() {
			$(this).removeClass("msg-hover");
		});
	});
	$(function () {
		$("#header .header-nav .account-action li.pulldown").mouseover(function () {
			$(this).addClass("cur");
		});
		$("#header .header-nav .account-action li.pulldown").mouseout(function() {
			$(this).removeClass("cur");
		});
	});

	//手机邮箱绑定信息显示隐藏效果
	$(function () {
		$(".com-binding dl").live({
			mouseover: function () {
				$(this).find("dd").addClass("cur");
			},
			mouseout: function () {
				$(this).find("dd").removeClass("cur");
			}
		});
	});
});