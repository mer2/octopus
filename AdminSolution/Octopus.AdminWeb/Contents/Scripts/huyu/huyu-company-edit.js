define("staticHuyu/huyu-company-edit", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var tools = require("staticHuyu/huyu-company-tools");
    (function () {
        var IdentityID = 0;  //  新增版块ID
        var Layouts = [];
        $.ajax({
            url: "/Company/GetLayouts.html",
            cache: false,
            dataType: "json",
            error: function () {
                $.joy.showMsg("获取数据失败");
            },
            success: function (data) {
                if (data == null) {
                    $.joy.showMsg("获取数据失败");
                    return false;
                }
                Layouts = data.Layouts;

                Layouts.sort(function (a, b) { return a.NavIndex - b.NavIndex; });
                $.each(Layouts, function (i, n) {
                    n.NavIndex = i;
                });
                Layouts.sort(function (a, b) { return a.DispIndex - b.DispIndex; });
                $.each(Layouts, function (i, n) {
                    n.DispIndex = i;
                });
                SetMenu(Layouts);

                //    保存发布
                $("#content").attr("class", "company edit");
                var editHeader = "<div class=\"edit-title sec-tem m-b\">" +
                    "<div class=\"sec-head\">" +
                    "<h2>布局管理模式</h2>" +
                    "<span></span>" +
                    "<div class=\"edit-save\"><a id=\"btnSaveConfigChanges\" href=\"javascript:;\" title=\"保存并发布\" class=\"btns btn-h23\">保存并发布</a></div>" +
                    "</div>" +
                    "</div>";
                $("#submenu").before(editHeader);
                $("#btnSaveConfigChanges").click(function () {
                    ///  设置版块显示顺序
                    $(".edit .sec-main .sec-tem").each(function (i, n) {
                        var _layoutId = $(n).attr("lang");
                        GetItem(Layouts, function (a) { return a.ID == _layoutId; }).DispIndex = i;
                    });
                    $.ajax({
                        url: "/Company/SaveConfigChanges.html",
                        cache: false,
                        type: "post",
                        dataType: "json",
                        data: { "Layouts": Translate(Layouts) },
                        success: function (res) {
                            if (res.Status == 0) {
                                $.joy.showMsg("保存成功", function () { location.href = "/qiye.html"; });
                            } else {
                                $.joy.showMsg(res.Msg);
                            }
                        },
                        error: function () {
                            $.joy.showMsg("保存失败");
                        }
                    });
                });
                //    导航编辑
                var menuEditBox = "<div class=\"edit-action\">" +
                    "<a href=\"javascript:void(0);\" title=\"设置\" class=\"btns btn-h23 a\">设置</a>" +
                    "</div>" +
                    "<div class=\"edit-mask\"><span></span></div>";
                $("#submenu").append(menuEditBox);
                //   导航编辑模式弹窗效果
                $(".edit .sec-submenu .edit-action").on("click", "a.btns", function () {
                    Layouts.sort(function (a, b) { return a.NavIndex - b.NavIndex; });
                    var menuList = "";
                    $.each(Layouts, function (i, n) {
                        if (n.State != "deleted") {
                            n.NavIndex = i;
                            menuList += '<tr lang="' + i + '">' +
                                '<td class="title ' + (n.IsNavTop ? '' : 'disabled') + '"><span>' + n.Title + '</span>' +
                                '</td>' +
                                '<td><a href="javascript:;" title="向上" class="btns btn-h23">向上</a><a href="javascript:;"' +
                                'title="向下" class="btns btn-h23">向下</a><a href="javascript:;" title="' + (n.IsNavTop ? '隐藏' : '显示') + '" class="btns btn-h23">' + (n.IsNavTop ? '隐藏' : '显示') + '</a>' +
                                '</td>' +
                                '</tr>';
                        }
                    });
                    $("#edit-nav table").children().remove().append(menuList);
                    $("#edit-nav table").append(menuList);
                    //   设置导航栏相关按钮事件
                    $("#edit-nav .pop-body td").on("click", "a", function () {
                        var _thisCon = $(this).closest("tr");
                        if ($(this).html() == "向上") {
                            var _thisUpCon = $(this).closest("tr");
                            if (_thisUpCon.prev("tr").length > 0) {
                                _thisUpCon.insertBefore(_thisUpCon.prev("tr"));
                            }
                        }
                        if ($(this).html() == "向下") {
                            var _thisDownCon = $(this).closest("tr");
                            if (_thisDownCon.next("tr").length > 0) {
                                _thisDownCon.insertAfter(_thisDownCon.next("tr"));
                            }
                        }
                        if ($(this).html() == "显示" || $(this).html() == "隐藏") {
                            if ($(this).html() == "显示") {
                                if (Count(Layouts, function (a) { return a.IsNavTop; }) >= 7) {
                                    $.joy.showMsg("最多只能显示7个导航");
                                    return;
                                }
                                $(this).closest("tr").find(".title").removeClass("disabled");
                                $(this).html("隐藏").attr("title", "隐藏");
                            } else {
                                $(this).closest("tr").find(".title").addClass("disabled");
                                $(this).html("显示").attr("title", "显示");
                            }
                            Layouts.sort(function (a, b) { return a.NavIndex - b.NavIndex; });
                            $("#edit-nav table tr").each(function (i, n) {
                                Layouts[n.lang].NavIndex = i;
                                Layouts[n.lang].IsNavTop = $(n).find(".title").attr("class").indexOf("disabled") < 0;
                            });
                        }
                    });
                    $.joy.layer({ layerId: "edit-nav" });
                });
                $("#edit-nav input[type=submit]").on("click", function () {
                    Layouts.sort(function (a, b) { return a.NavIndex - b.NavIndex; });
                    $("#edit-nav table tr").each(function (i, n) {
                        Layouts[n.lang].NavIndex = i;
                        Layouts[n.lang].IsNavTop = $(n).find(".title").attr("class").indexOf("disabled") < 0;
                    });
                    SetMenu(Layouts);
                    $.joy.closeLayer("edit-nav");
                });
                //    添加版块
                //定义板块类型以及插入时的格式
                var addTypeObj = {
                    "1": { "name": "供应信息", "html": '<div class="com-list sec-tem m-b"><div class="sec-head"><h2>供应信息</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "2": { "name": "需求信息", "html": '<div class="com-txt sec-tem m-b"><div class="sec-head"><h2>需求信息</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "3": { "name": "产品案例", "html": '<div class="com-list sec-tem m-b"><div class="sec-head"><h2>产品案例</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "4": { "name": "公告栏", "html": '<div class="com-list sec-tem m-b"><div class="sec-head"><h2>公告栏</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "5": { "name": "图片", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>图片</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "6": { "name": "视频", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>视频</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "7": { "name": "新闻", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>新闻</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "8": { "name": "介绍", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>介绍</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' },
                    "9": { "name": "联系方式", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>联系方式</h2></div><div class="sec-body"></div><div class="edit-action"><a href="/Company/Contact.html" target="_blank" title="添加联系人" class="btns btn-h23">添加联系人</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a></div><div class="edit-mask"><span></span></div></div>' },
                    "10": { "name": "动态", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>动态</h2></div><div class="sec-body"></div><div class="edit-action"><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a></div><div class="edit-mask"><span></span></div></div>' },
                    "11": { "name": "服务版块", "html": '<div class="com-news sec-tem m-b"><div class="sec-head"><h2>服务版块</h2></div><div class="sec-body"></div><div class="edit-action"><a class="arrow up" title="上移" href="javascript:;"></a><a class="arrow down" title="下移" href="javascript:;"></a><a class="btns btn-h23 set" title="设置" href="javascript:;">设置</a><a class="btns btn-h23 warn" title="删除" href="javascript:;">删除</a><a class="btns btn-h23 hideit" title="隐藏" href="javascript:;">隐藏</a></div><div class="edit-mask"><span></span></div></div>' }
                };

                var addModuleBox = "<div class=\"edit-add m-b layout\">" +
                    "<div class=\"edit-action\">" +
                    "<a href=\"javascript:;\" title=\"添加版块\" class=\"btns btn-h23\">添加版块</a>" +
                    "</div>" +
                    "<div class=\"edit-mask\"><span></span></div>" +
                    "</div>";
                $(addModuleBox).prependTo($(".edit .sec-main")).mouseover(function () { $(this).addClass("cur"); }).mouseout(function () { $(this).removeClass("cur"); });
                //添加板块弹层
                $(".edit .edit-add .edit-action").on("click", "a.btns", function () {
                    var cnt = Count(Layouts, function (a) { return a.State != "deleted"; });
                    $("#edit-addbox .pop-head h2 i").text(cnt);
                    $.joy.layer({ layerId: "edit-addbox" });
                });
                //添加板块弹层的添加按钮点击效果
                $("#edit-addbox .pop-body td a").on("click", function () {
                    if (Count(Layouts, function (a) { return a.State != "deleted"; }) >= 10) {
                        $.joy.showMsg("您只能添加10个版块");
                        return false;
                    }
                    var _type = this.lang;
                    if (!_type || addTypeObj[_type] == "undefined") return false;
                    $.joy.closeLayer("edit-addbox");
                    var layout = {
                        "ID": --IdentityID,
                        "Title": addTypeObj[_type].name,
                        "ContentType": _type,
                        "IsHomeTop": true,
                        "DispIndex": Layouts.length,
                        "NavIndex": Layouts.length,
                        "NavTitle": addTypeObj[_type].name,
                        "IsNavTop": true,
                        "DispStyle": 0,
                        "DispCount": 3,
                        "IsInternal": false,
                        "State": "Add"
                    };
                    switch (_type) {
                        case "8":
                            if (Count(Layouts, function (a) { return a.ContentType == "9"; }) > 0) {
                                $.joy.showMsg("您已经添加介绍版块");
                                return false;
                            }
                            break;
                        case "9":
                            if (Count(Layouts, function (a) { return a.ContentType == "9"; }) > 0) {
                                $.joy.showMsg("您已经添加联系人版块");
                                return false;
                            }
                            $("#ComContact").append(addTypeObj[_type].html);
                            $("#ComContact .sec-tem:last").attr("lang", layout.ID);
                            $("html,body").animate({ scrollTop: $("#ComContact").offset().top });
                            break;
                        case "10":
                            if (Count(Layouts, function (a) { return a.ContentType == "10"; }) > 0) {
                                $.joy.showMsg("您已经添加动态版块");
                                return false;
                            }
                            $("#ComDynamic").append(addTypeObj[_type].html);
                            $("#ComDynamic .sec-tem:last").attr("lang", layout.ID);
                            $("html,body").animate({ scrollTop: $("#ComDynamic").offset().top });
                            break;
                        default:
                            $(".edit .sec-main").append(addTypeObj[_type].html);
                            $(".edit .sec-main .sec-tem:last").attr("lang", layout.ID).mouseover(function () { $(this).addClass("cur"); }).mouseout(function () { $(this).removeClass("cur"); });
                            $("html,body").animate({ scrollTop: $(".edit .sec-main .sec-tem:last").offset().top });
                            break;
                    }

                    if (Count(Layouts, function (a) { return a.IsNavTop; }) >= 7) {
                        layout.IsNavTop = false;
                    }
                    Layouts.push(layout);
                    SetMenu(Layouts);
                    return false;
                });
                //   每个版块的操作按钮和遮罩层
                $(".edit .sec-main .sec-tem").each(function (i, n) {
                    var layoutId = $(n).attr("lang");
                    var layout = GetItem(Layouts, function (a) { return a.ID == layoutId; });
                    if (layout == undefined) {
                        return;
                    }
                    if (!layout.IsHomeTop) {
                        $(n).addClass("disabled");
                    }
                    $(n).append("<div class=\"edit-action\">" +
                        "<a href=\"javascript:;\" title=\"上移\" class=\"arrow up\"></a>" +
                        "<a href=\"javascript:;\" title=\"下移\" class=\"arrow down\"></a>" +
                        "<a href=\"javascript:;\" title=\"设置\" class=\"btns btn-h23 set\">设置</a>" +
                        "<a href=\"javascript:;\" title=\"删除\" class=\"btns btn-h23 warn\">删除</a>" +
                        "<a href=\"javascript:;\" title=\"隐藏\" class=\"btns btn-h23 hideit\">隐藏</a>" +
                        "</div>" +
                        "<div class=\"edit-mask\"><span></span></div>");
                });
                //  黄页编辑模式鼠标移上遮罩层
                $(".edit .edit-mask").parent().mouseover(function () { $(this).addClass("cur"); }).mouseout(function () { $(this).removeClass("cur"); });

                //   版块设置按钮 弹层
                $(".edit .sec-main").on("click", ".sec-tem .edit-action a.set", function () {
                    var _layoutId = $(this).parents(".sec-tem").attr("lang");
                    if (loadLayoutData(_layoutId)) {
                        tools.ShowLayer({
                            LayerID: "edit-set",
                            SubmitID: "#edit-set [type='submit']",
                            SubmitFun: function () {
                                if (saveLayoutData(_layoutId)) {
                                    ///  设置版块Title
                                    setLayoutAndMenuTitle(_layoutId);
                                    $.joy.closeLayer('edit-set');
                                }
                            }
                        });
                    }
                });

                //栏目设置中根据显示类型来改变显示条数的选择
                $(".edit-set .showType input[type=radio]").on("click", function () {
                    var _shownum = {
                        "list": '<input name="DispCount" type="radio" value="3" /><label>3条</label><input name="DispCount" type="radio" value="5" /><label>5条</label>',
                        "grid": '<input name="DispCount" type="radio" value="5" /><label>5条</label><input name="DispCount" type="radio" value="10" /><label>10条</label>'
                    };
                    if (this.lang && _shownum[this.lang]) {
                        $(".edit-set .showNum").html(_shownum[this.lang]);
                    }
                });

                //板块删除 删除按钮
                $(".edit").on("click", ".sec-tem .edit-action a.warn", function () {
                    var _this = $(this).parents("div .sec-tem");
                    $.joy.showConfirm("确认删除当前的版块？", {
                        okfun: function () {
                            var _layout = GetItem(Layouts, function (a) { return a.ID == _this.attr("lang"); });
                            if (_layout.IsInternal) {
                                $.joy.showMsg("固定版块,不能删除");
                            } else {
                                RemoveItem(Layouts, function (a) { return a.ID == _this.attr("lang"); });
                                SetMenu(Layouts);
                                _this.remove();
                            }
                        }
                    });
                });
                //板块隐藏 隐藏按钮
                $(".edit .sec-main ").on("click", ".sec-tem .edit-action a.hideit", function () {
                    var _this = $(this);
                    var _flag = _this.html() == "显示" ? true : false;
                    $.joy.showConfirm("确认" + (_flag ? "显示" : "隐藏") + "当前的版块？", {
                        okfun: function () {
                            GetItem(Layouts, function (a) { return a.ID == _this.parents("div .sec-tem").attr("lang"); }).IsHomeTop = _flag;
                            _this.attr("title", _flag ? "隐藏" : "显示").html(_flag ? "隐藏" : "显示");
                            if (_flag) {
                                _this.parents("div .sec-tem").removeClass("disabled");
                            } else {
                                _this.parents("div .sec-tem").addClass("disabled");
                            }
                        }
                    });
                });

                //向上移动
                $(".edit .sec-main").on("click", ".sec-tem .edit-action a.up", function () {
                    var _thisCon = $(this).closest(".sec-tem");
                    if (_thisCon.prev(".sec-tem:visible").length > 0) {
                        _thisCon.insertBefore(_thisCon.prev(".sec-tem:visible"));
                    }
                });

                //向下移动
                $(".edit .sec-main").on("click", ".sec-tem .edit-action a.down", function () {
                    var _thisCon = $(this).closest(".sec-tem");
                    if (_thisCon.next(".sec-tem:visible").length > 0) {
                        _thisCon.insertAfter(_thisCon.next(".sec-tem:visible"));
                    }
                });
                return false;
            }
        });

        function loadLayoutData(layoutId) {
            var layout = GetItem(Layouts, function (a) { return a.ID == layoutId; });
            if (!layout) {
                return false;
            }
            var contentType = layout.ContentType;
            var editForm = $("#edit-set div.pop-body div.contbox").hide().eq(contentType - 1);
            editForm.show();
            if (contentType == 4) {
                editForm.find("[name='Content']").html(getBulletinContent(layoutId));
            }
            editForm.find("input[name='Title']").val(layout.Title);
            editForm.find("input[name=DispStyle][value=" + layout.DispStyle + "]").click();
            editForm.find("input[name=DispCount][value=" + layout.DispCount + "]").click();
            return true;
        }

        function saveLayoutData(layoutId) {
            var layout = GetItem(Layouts, function (a) { return a.ID == layoutId; });
            if (!layout) {
                return false;
            }
            var contentType = layout.ContentType;
            var editForm = $("#edit-set div.pop-body div.contbox").eq(contentType - 1);
            var title = editForm.find("input[name='Title']").val();
            var content = editForm.find("[name='Content']").val();
            var dispStyle = editForm.find("input[name=DispStyle]:checked").val();
            var dispCount = editForm.find("input[name=DispCount]:checked").val();
            if (!RegExp(/\S+/).test(title)) {
                $.joy.showMsg("标题不能为空");
                return false;
            }
            layout.Title = $.trim(title);
            if (layout.ContentType == 4) {
                layout.Content = content;
                setBulletinContent(layoutId, content);
            }
            if (!isNaN(dispStyle)) {
                layout.DispStyle = dispStyle;
            }
            if (!isNaN(dispCount)) {
                layout.DispCount = dispCount;
            }
            return true;
        }

        function setBulletinContent(layoutId, content) {
            if ($.trim(content) != "") {
                $("<pre></pre>").appendTo($(".edit .sec-main .sec-tem[lang=" + layoutId + "] .sec-body").empty().removeClass("nothing")).text(content);
            } else {
                $(".edit .sec-main .sec-tem[lang=" + layoutId + "] .sec-body").addClass("nothing").html("暂无内容");
            }
        }

        function getBulletinContent(layoutId) {
            return $(".edit .sec-main .sec-tem[lang=" + layoutId + "] .sec-body pre").html();
        }

        function setLayoutAndMenuTitle(layoutId) {
            var layout = GetItem(Layouts, function (a) { return a.ID == layoutId; });
            if (layout) {
                $(".edit .sec-main .sec-tem[lang=" + layoutId + "] .sec-head h2").text(layout.Title);
                $("#submenu ul li a[lang=" + layoutId + "]").text(layout.Title);
            }
        }

        function SetMenu(layouts) {
            layouts.sort(function (a, b) { return a.NavIndex - b.NavIndex; });
            var menuList = '<li class="first cur"><a title="主页" href="javascript:;">主页</a></li>';
            $.each(layouts, function (i, n) {
                if (n.IsNavTop && n.State != "deleted") {
                    menuList += '<li><a title="' + n.Title + '" href="javascript:;" lang="' + n.ID + '">' + n.Title + '</a></li>';
                }
            });
            $("#submenu ul").empty().append(menuList);
        }

        function Count(layouts, a) {
            var cnt = 0;
            $.each(layouts, function (i, n) {
                if (a(n)) {
                    cnt++;
                }
            });
            return cnt;
        }

        function RemoveItem(layouts, a) {
            for (var i = 0; i < layouts.length; i++) {
                if (a(layouts[i])) {
                    if (layouts[i].ID < 0) {
                        layouts.splice(i, 1);
                    } else {
                        layouts[i].State = "deleted";
                        layouts[i].DispIndex = 999;
                        layouts[i].NavIndex = 999;
                    }
                    break;
                }
            }
        }

        function GetItem(layouts, a) {
            for (var i = 0; i < layouts.length; i++) {
                if (a(layouts[i])) {
                    return layouts[i];
                }
            }
            return undefined;
        }

        function Translate(layouts) {
            var _text = "[";
            $.each(layouts, function (i, n) {
                _text += "{";
                _text += "\"ID\":\"" + n.ID + "\",";
                _text += "\"Title\":\"" + n.Title + "\",";
                _text += "\"ContentType\":\"" + n.ContentType + "\",";
                _text += "\"IsHomeTop\":\"" + n.IsHomeTop + "\",";
                _text += "\"DispIndex\":\"" + n.DispIndex + "\",";
                _text += "\"NavIndex\":\"" + n.NavIndex + "\",";
                _text += "\"NavTitle\":\"" + n.NavTitle + "\",";
                _text += "\"IsNavTop\":\"" + n.IsNavTop + "\",";
                _text += "\"DispStyle\":\"" + n.DispStyle + "\",";
                _text += "\"DispCount\":\"" + n.DispCount + "\",";
                _text += "\"Content\":\"" + n.Content + "\",";
                _text += "\"State\":\"" + n.State + "\"";
                _text += "},";
            });
            if (_text.length > 1) {
                _text = _text.substring(0, _text.length - 1);
            }
            return _text + "]";
        }
    })();
});