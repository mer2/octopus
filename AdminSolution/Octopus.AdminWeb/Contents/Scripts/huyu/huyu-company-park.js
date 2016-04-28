define("staticHuyu/huyu-company-park", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var params = require("plugin-params")(module, "huyu");
    var tools = require("staticHuyu/huyu-company-tools");
    var nav = require("staticHuyu/huyu-navigate");
    var huyuinform = require("staticHuyu/huyu-inform");
    $(function () {
        $($("#content .sec-nav p").children().toArray().reverse()).each(function () {
            var lang = $(this).attr("lang");
            if (lang && lang != "") {
                $("#submenu ul li").has("a[lang='" + lang + "']").addClass("cur");
                return false;
            }
        });
    });
    $("#park_feature_editor :button").click(function () {
        $.joy.closeLayer("park_feature_editor");
    });
    $("#park_feature_editor :submit").click(function () {
        $.ajax({
            url: "/Company/EditParkFeature.html",
            cache: false,
            type: "post",
            dataType: "json",
            data: {
                category: $("#park_feature_editor #Category").val(),
                content: $("#park_feature_editor #Content").val()
            },
            complete: function () {
                $.joy.closeLayer("park_feature_editor");
            },
            success: function (res) {
                if (res.Status == 0) {
                    $.joy.showMsg("保存成功");
                } else {
                    $.joy.showMsg("保存失败");
                }
            },
            error: function () {
                $.joy.showMsg("保存失败");
            }
        });
    });
    $(function () {
        $("#park_feature a").click(function () {
            var category = $(this).attr("lang");
            var title;
            switch (category) {
                case "1":
                    title = "交通";
                    break;
                case "2":
                    title = "车位";
                    break;
                case "3":
                    title = "银行";
                    break;
                case "4":
                    title = "餐饮";
                    break;
                case "5":
                    title = "住宿";
                    break;
                case "6":
                    title = "孵化器";
                    break;
                case "7":
                    title = "其他";
                    break;
                default:
                    title = "";
                    break;
            }
            if (title == "") {
                return false;
            }
            $("#park_feature_editor #Category").val(category);
            $("#park_feature_editor #Title").html(title);
            $("#park_feature_editor #Content").val("");
            $.ajax({
                url: "/Company/GetParkFeature.html",
                cache: false,
                type: "post",
                dataType: "json",
                data: {
                    category: category
                },
                success: function (res) {
                    $("#park_feature_editor #Content").val(res.Content);
                },
                complete: function () {
                    $.joy.layer({
                        layerId: "park_feature_editor"
                    });
                }
            });
        });
    });
    //
    var _size = 5;
    $(function () {
        //  评分效果
        $("#NewComment .sec-body .score-info .hidestar li").mouseover(function () {
            $(this).closest("div.hidestar").prev(".star-level").attr("class", "star-level lv-" + $(this).attr("lang") * 10);
            $(this).closest("div.hidestar").prev(".star-level").find("i").html($(this).attr("lang"));
        }).mouseout(function () {
            $(this).closest("div.hidestar").prev(".star-level").attr("class", "star-level lv-" + $(this).closest("div.hidestar").find("input").val() * 10);
            $(this).closest("div.hidestar").prev(".star-level").find("i").html($(this).closest("div.hidestar").find("input").val());
        }).click(function () {
            $(this).closest("div.hidestar").find("input").val($(this).attr("lang"));
            $(this).closest("div.hidestar").prev(".star-level").find("i").html($(this).attr("lang"));
        });
        //  更多评论
        $("#CommentList .sec-body .listmore a").click(function () {
            loadComment($("#CommentList .sec-body .info-cont dl").length);
        });
        //  举报按钮
        $("#CommentList .sec-body .info-cont").on("click", "dl .date a", function () {
            huyuinform.HuyuInform($(this), function () {
                $.joy.showMsg('举报成功');
            }, function (btn, msg) {
                $.joy.showMsg(msg.Message);
            });
        });
        //  加载评论
        loadComment(0);
        //  提交评论
        $("#PublishComment").click(function () {
            var _comment = $("#Comment").val();
            var _scores;
            var _score12 = $("#NewComment #score12").val();
            var _score13 = $("#NewComment #score13").val();
            var _score14 = $("#NewComment #score14").val();
            var _score15 = $("#NewComment #score15").val();
            var _score16 = $("#NewComment #score16").val();
            if (_score12 == "0" || _score13 == "0" || _score14 == "0" || _score15 == "0" || _score16 == "0") {
                $.joy.showMsg("您还没有对园区进行评分");
                return false;
            }
            if (!_comment.match(/\S+/)) {
                $("#Comment").focus();
                return false;
            }
            //  是否已评论
            //  是否认证
            nav.showLogin(function (user) {
                tools.HasAuthentic(function (flag) {
                    if (!flag) {
                        $.joy.showMsg("只有认证用户才能点评");
                        return;
                    }
                    $.ajax({
                        url: "/Company/ParkComment.html",
                        cache: false,
                        data: {
                            id: params.ID,
                            comment: _comment,
                            score1: _score12,
                            score2: _score13,
                            score3: _score14,
                            score4: _score15,
                            score5: _score16
                        },
                        dataType: "json",
                        type: "post",
                        success: function (res) {
                            if (res && res.ResultNo == 0) {
                                $.joy.showMsg("评论成功");
                                var total = res.Total;
                                var _moreCnt = total - $("#CommentList .sec-body .info-cont dl").length;
                                if (_moreCnt <= 0) {
                                    $("#CommentList .sec-body .listmore a").hide();
                                    $("#CommentList .sec-body .listmore a").html("查看更多点评（" + 0 + "条）");
                                } else {
                                    $("#CommentList .sec-body").removeClass("nothing");
                                    $("#CommentList .sec-body .info-cont").html("");
                                    $("#CommentList .sec-body .listmore a").html("查看更多点评（" + _moreCnt + "条）");
                                    $("#CommentList .sec-body .listmore a").show();
                                }
                            } else {
                                $.joy.showMsg(res ? res.ResultDescription : "评论失败");
                            }
                        }
                    });
                });
            });
            return false;
        });
    });
    //  加载评论
    function loadComment(startIdx) {
        $.ajax({
            url: "/Company/GetParkComments.html",
            data: {
                id: params.ID,
                startIndex: startIdx,
                length: _size
            },
            dataType: "json",
            type: "post",
            success: function (res) {
                if (res && res.ResultNo == 0 && res.Total != 0) {
                    $("#bottomPageContainer").children().remove();
                    $(res.ResultAttachObjectEx).each(function (idx, item) {
                        var s1 = tools.ToString(tools.Single(item.RateScores, function (a) { return a.RateName == "价格"; }).RateValue, 1, { 0: "0", 10: "10" });
                        var s2 = tools.ToString(tools.Single(item.RateScores, function (a) { return a.RateName == "环境"; }).RateValue, 1, { 0: "0", 10: "10" });
                        var s3 = tools.ToString(tools.Single(item.RateScores, function (a) { return a.RateName == "物业"; }).RateValue, 1, { 0: "0", 10: "10" });
                        var s4 = tools.ToString(tools.Single(item.RateScores, function (a) { return a.RateName == "服务"; }).RateValue, 1, { 0: "0", 10: "10" });
                        var s5 = tools.ToString(tools.Single(item.RateScores, function (a) { return a.RateName == "配套"; }).RateValue, 1, { 0: "0", 10: "10" });
                        var _ele = $('<dl class="clearfix">' +
							'<dd>' +
							'<div class="info">' +
							'<div class="date f-r">' + tools.DateToString(item.CreateTime) + (params.ShowInform ? ' | <a href="javascript:;" lang=' + '{"Type":"JoyHuyuComment.Comments","Value":"' + item.ID + '","Title":"' + item.CommentContent + '"}' + '>举报</a>' : '') + '</div>' +
							'<a href="/Company/UserCompany/' + item.UserID + '" target="_blank" class="commenta" title="' + item.UserName + '">' + item.UserName + '</a>' +
							'<a class="btns btn-h23" href="/Message/Chat/' + item.UserID + '" target="_blank">联系TA</a>' +
							'</div>' +
							'<div class="num clear"><span>价格：<i>' + s1 + '</i>分</span><span>环境：<i>' + s2 + '</i>分</span><span>物业：<i>' + s3 + '</i>分</span><span>服务：<i>' + s4 + '</i>分</span><span>配套：<i>' + s5 + '</i>分</span></div>' +
							'<p>' + item.CommentContent + '</p>' +
							'</dd>' +
							'</dl>');
                        $("#CommentList .sec-body .info-cont").append(_ele);
                    });
                    var total = res.Total;
                    var _moreCnt = total - $("#CommentList .sec-body .info-cont dl").length;
                    if (_moreCnt <= 0) {
                        $("#CommentList .sec-body .listmore a").hide();
                        $("#CommentList .sec-body .listmore a").html("查看更多点评（" + 0 + "条）");
                    } else {
                        $("#CommentList .sec-body .listmore a").html("查看更多点评（" + _moreCnt + "条）");
                        $("#CommentList .sec-body .listmore a").show();
                    }
                    $("#CommentList .sec-body").removeClass("nothing");
                } else {
                    $("#CommentList .sec-body").addClass("nothing");
                    $("#CommentList .sec-body .info-cont").html("暂无评价");
                    $("#CommentList .sec-body .listmore a").hide();
                    $("#CommentList .sec-body .listmore a").html("查看更多点评（" + 0 + "条）");
                }
            }
        });
    }
});