define("staticHuyu/huyu-company-shipin", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var setting = require("staticHuyu/huyu-config");
    
    $.extend(exports, {
        atlas: function (obj, options) {
            var o = $.extend({
                items_container: ".video-btn",	//滚动图片容器id
                scroll_c: ".video-btn ul",       //滚动效果的容器
                items: "li",						//滚动图片
                picspre: ".action-prev",						//滚动图片容器内前一个按钮
                picsnext: ".action-next",					//滚动图片容器内后一个按钮
                bigimg_con: ".video-big",		//大图容器id
                bigimg_intro: "p",
                picmore_con: ".video-artwork",		//图片的个数信息显示框
                step_width: 129,				    //包含边、margin、padding
                delay: 300,
                interval: "",
                scrollobj: false,
                isajax: false,
                ajaxfun: false
            }, options);
            var $smallpics = $(obj).find(o.items_container); //滚动区大容器
            var $scroll_con = $(obj).find(o.scroll_c);
            var $bigpic = $(obj).find(o.bigimg_con);         //大图容器
            var $picinfo = $(obj).find(o.bigimg_intro);				 //大图图片信息
            var $picpages = $(obj).find(o.picmore_con);		 //图片数目信息框
            var $originalpic = $(obj).find("#originalPic");
            var $leftcursor = $(obj).find(o.picspre), $rightcursor = $(obj).find(o.picsnext);


            var length = $scroll_con.find(o.items).length;
            var current = 1;
            var pic_c_width = o.step_width * length;
            //alert($("#ascrail2000-hr").width());
            var scrollbar_p = o.step_width;

            $smallpics.find(".smallbtn:visible").click(function () {
                var gonum;
                if (o.isajax) {
                    var _margin_left = parseInt($scroll_con.css("margin-left").toLowerCase().replace("px", ""));
                    if ($(this).hasClass("left")) {
                        if ($smallpics.find(".left").data("scrolling")) return false;
                        $smallpics.find(".left").data("scrolling", true);
                        if (_margin_left != 0) $scroll_con.animate({ marginLeft: _margin_left + scrollbar_p * 4 }, function () { $smallpics.find(".left").data("scrolling", false); });

                    } else if ($(this).hasClass("right")) {
                        if ($smallpics.find(".right").data("scrolling")) return false;
                        $smallpics.find(".right").data("scrolling", true);
                        if (!$(".contvideo .smallslide ul").data("loadAll")) {
                            o.ajaxfun();
                            $scroll_con.animate({ marginLeft: _margin_left - scrollbar_p * 4 }, function () { $smallpics.find(".right").data("scrolling", false); });
                        } else {
                            if (($scroll_con.find(o.items).length - 4) * scrollbar_p > Math.abs(_margin_left)) {
                                $scroll_con.animate({ marginLeft: _margin_left - scrollbar_p * 4 }, function () { $smallpics.find(".right").data("scrolling", false); });
                            } else {
                                $smallpics.find(".right").data("scrolling", false);
                            }
                        }
                    }
                    //o.ajaxfun();
                } else {
                    if ($(this).hasClass("left")) {
                        gonum = $scroll_con.find(".current").parent().index();
                    } else if ($(this).hasClass("right")) {
                        gonum = $scroll_con.find(".current").parent().index() + 2;
                    }
                    if ($(obj).data("bigpicloaded")) {
                        if (gonum > 0 && gonum < length + 1) {
                            $(obj).data("bigpicloaded", 0);
                            showPic(gonum);
                            $(this).data("nothing", 0);

                        } else if (gonum == 1 && $(this).hasClass("left")) {
                            $(this).data("nothing", 1);
                            window.location.href = $smallpics.find(".atlas-prev a").attr("href");
                        } else if (gonum == length && $(this).hasClass("right")) {
                            $(this).data("nothing", 1);
                            window.location.href = $smallpics.find(".atlas-next a").attr("href");
                        }
                    }
                }
                return false;
            }).mouseenter(function () {
                $(this).find("a").addClass("hover");
            }).mouseleave(function () { $(this).find("a").removeClass("hover"); });

            if (o.isajax) {
                ajaxinit();
            } else {
                init();
            }

            function init() {
                //$picpages.find('span').html('<em>'+current+'</em>/'+length);
                $scroll_con.width(pic_c_width);
                if (length <= 5) { $smallpics.find(".smallbtn").hide(); } else { $smallpics.find(".smallbtn").show(); }
                showPic(current);
                $leftcursor.click(function () {
                    var gonum = $scroll_con.find(".current").parent().index();
                    if ($(obj).data("bigpicloaded")) {
                        if (gonum == 0) {
                            window.location.href = $smallpics.find(".atlas-prev a").attr("href");
                        } else {
                            $(obj).data("bigpicloaded", 0);

                            showPic(gonum);
                        }
                    }
                    return false;
                });
                $rightcursor.click(function () {
                    var gonum = $scroll_con.find(".current").parent().index() + 2;
                    if ($(obj).data("bigpicloaded")) {
                        if (gonum == length + 1) {
                            window.location.href = $smallpics.find(".atlas-next a").attr("href");
                        } else {
                            $(obj).data("bigpicloaded", 0);
                            showPic(gonum);
                        }
                    }
                    return false;
                });

                $scroll_con.find(o.items).find('a').click(function () {
                    var gonum = $(this).parent().index() + 1;

                    if ($(obj).data("bigpicloaded") && gonum != $scroll_con.find(".current").parent().index() + 1) {
                        $(obj).data("bigpicloaded", 0);
                        showPic(gonum);
                    }
                    return false;
                });
                return false;
            }

            function ajaxinit() {
                $scroll_con.width(999999);
                if (!o.isajax) return false;
                if (typeof (o.ajaxfun) == "function") {
                    o.ajaxfun(0);
                    showPic(0);
                }
                $scroll_con.on("click", "li a", function () {
                    var gonum = $(this).parent().attr("class").toLowerCase().replace("picid_", "");
                    if ($(obj).data("bigpicloaded") && $scroll_con.find(".picID_" + gonum).length > 0) {
                        $(obj).data("bigpicloaded", 0);
                        showPic(gonum);
                    }
                    return false;
                });


                return false;
            }
            function showPic(num) {
                if (!num) num = 0;
                $scroll_con.find(o.items).find('a').removeClass("current");
                var $curli;
                if (o.isajax && num) {
                    $curli = $scroll_con.find(".picID_" + num).find('img');
                } else {
                    $curli = $scroll_con.find(o.items).eq(num).find('img');
                }


                //data-original
                var curpic_src = $curli.attr("data-original"), curpic_info = $curli.attr("lang");//,curpic_id = parseInt($curli.attr("name"));
                $curli.parent("a").addClass("current");
                $originalpic.attr("href", curpic_src);
                $(obj).data("bigpicloaded", 0); $(obj).data("picscrolled", 0);
                var _page = parseInt($scroll_con.css("margin-left").toLowerCase().replace("px", "")) / (scrollbar_p * 4);
                _page = Math.abs(_page);
                _thispage = parseInt($scroll_con.find(".current").parent("li").index() / 4);
                if (_thispage != _page) $scroll_con.animate({ marginLeft: -scrollbar_p * _thispage * 4 });
                if (length > 5) {
                    //$scroll_con.animate({marginLeft:-scrollbar_p*(num-3>0?(num+3>length?length-4:num-3):0)});
                }
                //o.scrollobj[0].setScrollLeft(scrollbar_p*(num-3>0?(num+3>length?length-5:num-3):0));
                var embedStr = '<embed src="' + curpic_src + '" allowFullScreen="true" quality="high" width="643" height="368" align="middle" allowScriptAccess="always" autostart="false" type="application/x-shockwave-flash"></embed>';
                $bigpic.find(".cur_show").html(embedStr);
                $(obj).data("bigpicloaded", 1);
                $picinfo.html(curpic_info);
                $("#bdshare").attr('data', '{\'text\':\'' + curpic_info + '\',\'pic\':\'' + $curli.attr("src") + '\',\'url\':\'' + setting.urls.huyuUrl + '\'}');
            }

        }
    });
    $.extend(exports, {
        GetMedias: function getPics(params, start) {
            if (!start) start = 0;
            var op = { companyID: params.CompanyID || 0, contentType: params.ContentType || 0, layoutID: params.LayoutID || 0 };
            var _length = 4, htmlstr = '';
            if ($(".contvideo .smallslide ul").data("loadedPicNum")) start += parseInt($(".contvideo .smallslide ul").data("loadedPicNum"));
            var picsurl = '/AjaxJson/GetCompanyMedias?companyID=' + op.companyID + '&contentType=' + op.contentType + '&layoutID=' + op.layoutID + '&startIndex=' + start + '&length=' + _length;
            $.ajax({
                url: picsurl,
                dataType: "json",
                type: "GET",
                async: false,
                success: function (data) {
                    //$(this).find("a").removeClass("hover").css("cursor","default");							
                    if (data.ResultNo == 0) {
                        var picArr = data.ResultAttachObjectEx;
                        for (var i = 0; i < picArr.length; i++) {
                            //'<li lang="'+序号(0~图片总数)+'"><a href="#" class="current" lang="图片id"><img src="小图地址" data-original="大图地址" lang="图片信息" /></a></li>'
                            htmlstr += '<li class="picID_' + picArr[i].ID + '" lang="' + (start + i) + '"><a href="javascript:;"><img src="' + picArr[i].IconUrl + '" width="122" height="92" data-original="' + picArr[i].VideoUrl + '" lang="' + picArr[i].Title + '" /><span></span></a></li>';
                        }
                        $(".contvideo .smallslide ul").data("loadedPicNum", start + picArr.length);
                        if ((start + picArr.length) == data.Total) $(".contvideo .smallslide ul").data("loadAll", true);
                        if (start == 0) {
                            $(".contvideo .smallslide ul").html(htmlstr);
                        } else {
                            $(".contvideo .smallslide ul").append($(htmlstr));
                        }
                    } else {
                        $(".contvideo .smallslide ul").data("loadAll", true);
                    }
                    return false;
                }
            });
            return false;
        }
    });
});