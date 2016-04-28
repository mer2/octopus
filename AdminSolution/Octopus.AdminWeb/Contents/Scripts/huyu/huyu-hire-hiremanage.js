define("staticHuyu/huyu-hire-hiremanage", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var tools = require("staticHuyu/huyu-company-tools");
    var layer = require("layer");
    require("staticHuyu/huyu-hire");
    var params = require("plugin-params")(module, "huyu");
    var hireProject = {
        IdentityID: -1,
        IsUpdated: false,
        OfferNo: params.OfferNo,
        ContractTerms: "",
        Milestones: []
    };

    function hasUpdated(updated) {
        hireProject.IsUpdated = updated;
        if (updated) {
            $(".edit").show();
        }
    }

    $.ajax({
        url: "/Hire/GetHiredMilestones.html",
        data: { offerNo: params.OfferNo },
        dataType: "json",
        type: "post",
        success: function (res) {
            if (res.ResultNo == 0) {
                hireProject.ContractTerms = $("#ContractTerms").html();
                hireProject.Milestones = res.ResultAttachObjectEx;
                $.each(hireProject.Milestones, function () {
                    this.IsDelete = false;
                    this.StartTime = tools.DateToShortString(this.StartTime, '.');
                    this.EndTime = tools.DateToShortString(this.EndTime, '.');
                    this.Remark = "";
                    if (this.ID <= hireProject.IdentityID) {  //  防止生成重复ID
                        hireProject.IdentityID = this.ID - 1;
                    }
                });
            } else {
                layer.showMsg("加载数据失败,请刷新页面后再试");
            }
        },
        error: function () {
            layer.showMsg("加载数据失败,请刷新页面后再试");
        }
    });
    //日历插件
    $(function () {
        $("#StartTime").datepicker({
            defaultDate: "+1w",
            dateFormat: "yy.mm.dd",
            changeMonth: true,
            numberOfMonths: 1,
            onSelect: function (selectedDate) {
                $("#EndTime").datepicker("option", "minDate", selectedDate);
            },
            onClose: function () {
                $("#StartTime").focus();
            }
        });
        $("#StartTime").datepicker($.datepicker.regional["zh-CN"]);
        $("#EndTime").datepicker({
            defaultDate: "+1w",
            dateFormat: "yy.mm.dd",
            changeMonth: true,
            numberOfMonths: 1,
            onSelect: function (selectedDate) {
                $("#StartTime").datepicker("option", "maxDate", selectedDate);
            },
            onClose: function () {
                $("#EndTime").focus();
            }
        });
        $("#EndTime").datepicker($.datepicker.regional["zh-CN"]);
    });
    //  历史版本
    $("#HistoryVersion a[title='查看']").click(function () {
        $.ajax({
            url: "/Hire/GetHistoryVersion.html",
            type: "post",
            dataType: "json",
            data: {
                version: $(this).attr("lang")
            },
            success: function (res) {
                if (res.ResultNo == 0) {
                    $("#ProjectHistory-layer .pop-body div:eq(0) div:eq(0)").html("甲方：" + res.ResultAttachObjectEx.UserName);
                    $("#ProjectHistory-layer .pop-body div:eq(0) div:eq(1)").html("乙方：" + res.ResultAttachObjectEx.HiredUserName);
                    $("#ProjectHistory-layer .pop-body pre").html(res.ResultAttachObjectEx.ContractTerms);
                    var html = '<tr>' +
									'<th class="style1">项目名称</th>' +
									'<th class="style2">佣金金额</th>' +
									'<th class="style3">开始日期</th>' +
									'<th class="style3">交付日期</th>' +
								'</tr>';
                    $.each(res.ResultAttachObjectEx.Milestones, function (idx, item) {
                        html += '<tr>' +
									'<td class="style1">' + (idx + 1) + '. ' + item.Title + '</td>' +
									'<td class="style2">￥' + item.TotalCommission + '</td>' +
									'<td class="style3">' + tools.DateToShortString(item.StartTime, ',') + '</td>' +
									'<td class="style3">' + tools.DateToShortString(item.EndTime, ',') + '</td>' +
								'</tr>';
                    });
                    $("#ProjectHistory-layer .pop-body tbody").html(html);
                    layer.layer({ layerId: "ProjectHistory-layer" });
                } else {
                    layer.showMsg("获取数据失败,请刷新页面后再试");
                }
            },
            error: function () {
                layer.showMsg("获取数据失败,请刷新页面后再试");
            }
        });
    });
    //  显示详细
    $(".business .business-detailed .stage .look-more").toggle(
	  function () {
	      $(this).parents("tr").find(".icon").addClass("icon-open");
	      $(this).parents("tr").find(".icon").removeClass("icon-close");
	      $(this).parents("tr").next(".table-tr2").show();
	  },
	  function () {
	      $(this).parents("tr").find(".icon").addClass("icon-close");
	      $(this).parents("tr").find(".icon").removeClass("icon-open");
	      $(this).parents("tr").next(".table-tr2").hide();
	  }
	);
    //  编辑项目阶段
    $("#HiredMilestones").on("click", "a.edtMilestone", function () {
        var ths = $(this);
        showMilestoneLayer(ths.attr("lang"));
    });
    //  删除项目阶段
    $("#HiredMilestones").on("click", "a.delMilestone", function () {
        var ths = $(this);
        var mil = tools.Single(hireProject.Milestones, function (a) { return a.ID == ths.attr("lang"); });
        if (mil.ID < 0) {
            layer.showConfirm("你确定删除此阶段吗?", {
                okfun: function () {
                    deleteMilestone(mil.ID);
                }
            });
            return;
        }
        $("#deletemilestone-layer .submitBtn").attr("lang", mil.ID);
        layer.layer({ layerId: "deletemilestone-layer" });
    });

    function deleteMilestone(id, remark) {
        var mil = tools.Single(hireProject.Milestones, function (a) { return a.ID == id; });
        if (remark) {
            mil.Remark = remark;
        }
        removeItem(hireProject.Milestones, function (a) {
            return a.ID == mil.ID;
        });
        var ths = $("#HiredMilestones a[lang='" + id + "']").closest("tr");
        ths.closest("tr").next().remove();
        ths.closest("tr").remove();
        setItemSeq();
        setCountInfo();
        hasUpdated(true);
        $("#EdtContractTerms a[title='编辑']").click();
    }

    $("#deletemilestoneform").validate({
        rules: {
            Remark: { required: true }
        },
        messages: {
            Remark: { required: "请填写理由" }
        },
        errorElement: "span",
        errorPlacement: function (error, element) {
            if ($(error).text() == "0") {
                return;
            } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
                $("#" + $(element).attr("name") + "Msg").html(error);
            } else {
                $(element).nextAll("p").html(error);
            }
        },
        success: "valid",
        submitHandler: function () {
            deleteMilestone($("#deletemilestone-layer .submitBtn").attr("lang"), $("#deletemilestone-layer input[name='Remark']").val());
            layer.closeLayer("deletemilestone-layer");
        }
    });

    //  托管项目阶段
    $("#HiredMilestones").on("click", "a.trusteeshipMilestone", function () {
        var ths = $(this);
        trusteeshipCommission(ths.attr("lang"));
    });
    //  支付项目阶段
    $("#HiredMilestones").on("click", "a.payMilestone", function () {
        var ths = $(this);
        payMilestoneCommission(ths.attr("lang"));
    });

    //  设置项目阶段编号
    function setItemSeq() {
        $("#HiredMilestones tr.table-tr1").each(function (idx) {
            var html = $(this).find("td").eq(1).html().replace(/^\d/, idx + 1);
            $(this).find("td").eq(1).html(html);
        });
    }
    //  设置最终交付日期,佣金金额
    function setCountInfo() {
        var endDate = "";
        var totalCommission = 0;
        var avaCommission = 0;
        var paidCommission = 0;
        $.each(hireProject.Milestones, function (idx, item) {
            if (!item.IsDelete) {
                if (item.EndTime > endDate) {
                    endDate = item.EndTime;
                }
                totalCommission += isNaN(item.TotalCommission) ? 0 : parseFloat(item.TotalCommission);
                avaCommission += isNaN(item.AvailableCommission) ? 0 : parseFloat(item.AvailableCommission);
                if (item.Status == 50) {
                    paidCommission += item.TotalCommission;
                }
            }
        });
        $("#divCountInfo").replaceWith('<div id="divCountInfo" class="num">' +
                            '<span>最终交付日期：' + endDate + '</span>' +
                            '<span>总佣金金额：<i>￥' + totalCommission + '</i></span>' +
                            '<span>已托管金额：<i>￥' + avaCommission + '</i></span>' +
                            '<span>已支付金额：<i>￥' + paidCommission + '</i></span>' +
                        '</div>');
    }
    //  添加项目阶段保存按钮
    $("#milestoneform").validate({
        rules: {
            Title: { required: true },
            StartTime: { required: true, IsMatch: [/^\d{4}\.\d{2}\.\d{2}$/] },
            EndTime: { required: true, IsMatch: [/^\d{4}\.\d{2}\.\d{2}$/] },
            Content: { required: true },
            TotalCommission: { required: true, IsMatch: [/^\s*[1-9]+\d*\s*$/] },
            Remark: { required: true }
        },
        messages: {
            Title: { required: "请填写项目名称" },
            StartTime: { required: "请选择开始日期", IsMatch: "日期格式如:2013.08.12" },
            EndTime: { required: "请选择交付日期", IsMatch: "日期格式如:2013.08.12" },
            Content: { required: "请填写描述" },
            TotalCommission: { required: "请填写佣金", IsMatch: "金额必须为大于0的整数" },
            Remark: { required: "请填写理由" }
        },
        errorElement: "span",
        errorPlacement: function (error, element) {
            if ($(error).text() == "0") {
                return;
            } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
                $("#" + $(element).attr("name") + "Msg").html(error);
            } else {
                $(element).nextAll("p").html(error);
            }
        },
        success: "valid",
        submitHandler: function () {
            var ths = $("#milestone-layer .submitBtn");
            var title = $("#milestone-layer #Title").val();
            var content = $("#milestone-layer #Content").val();
            var remark = $("#milestone-layer #Remark").val() || "";
            var totalCommission = $("#milestone-layer #TotalCommission").val();
            var startTime = $("#milestone-layer #StartTime").val();
            var endTime = $("#milestone-layer #EndTime").val();
            var id = ths.attr("lang");
            var mil;
            if (id == 0) {
                mil = {
                    ID: hireProject.IdentityID--,
                    DisplayIndex: hireProject.Milestones.length,
                    Title: title,
                    Content: content,
                    Remark: remark,
                    TotalCommission: totalCommission,
                    IsDelete: false,
                    StartTime: startTime,
                    EndTime: endTime
                };
                addProjectMil(mil);
            } else {
                mil = tools.Single(hireProject.Milestones, function (a) {
                    return a.ID == id;
                });
                mil.DisplayIndex = hireProject.Milestones.length;
                mil.Title = title;
                mil.Content = content;
                mil.Remark = remark;
                mil.TotalCommission = totalCommission;
                mil.StartTime = startTime;
                mil.EndTime = endTime;
            }
            var ele;
            if (id == 0) {
                ele = $(projectMilestoneRow(mil) + projectMilestoneContentRow(mil)).appendTo($("#HiredMilestones"));
            } else {
                var temp = $("#HiredMilestones a[lang='" + id + "']").closest("tr");
                temp.next().replaceWith(projectMilestoneContentRow(mil));
                ele = $(projectMilestoneRow(mil)).replaceAll(temp);
            }
            ele.find(".look-more").toggle(function () {
                $(this).parents("tr").find(".icon").addClass("icon-open");
                $(this).parents("tr").find(".icon").removeClass("icon-close");
                $(this).parents("tr").next(".table-tr2").show();
            }, function () {
                $(this).parents("tr").find(".icon").addClass("icon-close");
                $(this).parents("tr").find(".icon").removeClass("icon-open");
                $(this).parents("tr").next(".table-tr2").hide();
            });
            hasUpdated(true);
            setItemSeq();
            setCountInfo();
            layer.closeLayer("milestone-layer");
            $("p.noalign").html("");
            //$("#EdtContractTerms a[title='编辑']").click();
            //$(".edit").show();
            return false;
        }
    });

    function projectMilestoneRow(obj) {
        return '<tr class="table-tr1">' +
            '<td class="style1"><span class="icon icon-close  look-more"></span></td>' +
            '<td class="style2">' + hireProject.Milestones.length + '. ' + obj.Title + '</td>' +
            '<td class="style3"><i>￥</i>' + obj.TotalCommission + '</td>' +
            '<td class="style4">' + obj.StartTime + '</td>' +
            '<td class="style4">' + obj.EndTime + '</td>' +
            '<td class="style5">等待中</td>' +
            '<td class="style6"><span><a class="edtMilestone" href="javascript:;" title="编辑" lang="' + obj.ID + '">编辑</a></span>' +
            '<span><a class="delMilestone" href="javascript:;" title="删除" lang="' + obj.ID + '">删除</a></span></td>' +
            '</tr>';
    }

    function projectMilestoneContentRow(obj) {
        var type = obj.ID < 0 ? "新增理由" : "修改理由";
        return '<tr class="table-tr2">' +
            '<td></td>' +
            '<td colspan="6">' +
            '<pre class="txt">' + obj.Content + '</pre>' +
            '<div class="change"><span>' + type + '：</span><pre class="txt">' + obj.Remark + '</pre></div>' +
            '</td>' +
            '</tr>';
    }

    //  添加项目阶段按钮
    $("#AddMilestone").click(function () {
        showMilestoneLayer(0);
    });
    //  加载初始数据
    function showMilestoneLayer(id) {
        $("#milestone-layer .submitBtn").attr("lang", id);
        if (id == 0) {
            $("#milestone-layer input[type='text']").val("");
            $("#milestone-layer textarea").val("");
            $("#milestone-layer table tr:eq(4) .title label").text("新增理由：");
        } else {
            var mil = tools.Single(hireProject.Milestones, function (a) {
                return a.ID == id;
            });
            $("#milestone-layer #Title").val(mil.Title);
            $("#milestone-layer #Content").val(mil.Content);
            $("#milestone-layer #Remark").val(mil.Remark);
            $("#milestone-layer #TotalCommission").val(mil.TotalCommission);
            $("#milestone-layer #StartTime").val(mil.StartTime);
            $("#milestone-layer #EndTime").val(mil.EndTime);
            $("#milestone-layer table tr:eq(4) .title label").text("修改理由：");
        }
        $("#milestone-layer #StartTime").datepicker("option", "maxDate", new Date("2999-12-31"));
        $("#milestone-layer #EndTime").datepicker("option", "minDate", new Date("1980-01-01"));
        layer.layer({ layerId: "milestone-layer", close_layer: "#milestone-layer .resetBtn" });
    }
    //  编辑合作条款
    $("#EdtContractTerms h3 a").click(function () {
        $("#contractterms-layer [name='ContractTerms']").val(hireProject.ContractTerms);
        $("#contractterms-layer [name='Remark']").val(hireProject.Remark);
        layer.layer({ layerId: "contractterms-layer" });
    });
    $("#termsForm").validate({
        rules: {
            ContractTerms: { required: true },
            Remark: { required: true }
        },
        messages: {
            ContractTerms: { required: "请填写合作条款" },
            Remark: { required: "请填写理由" }
        },
        errorElement: "span",
        errorPlacement: function (error, element) {
            if ($(error).text() == "0") {
                return;
            } else if ($("#" + $(element).attr("name") + "Msg").length != 0) {
                $("#" + $(element).attr("name") + "Msg").html(error);
            } else {
                $(element).nextAll("p").html(error);
            }
        },
        success: "valid",
        submitHandler: function () {
            hasUpdated(true);
            hireProject.ContractTerms = $("#contractterms-layer [name='ContractTerms']").val();
            hireProject.Remark = $("#contractterms-layer [name='Remark']").val() || "";
            var html = '<pre id="ContractTerms" class="article">' + hireProject.ContractTerms + '</pre>';
            if (hireProject.Remark != "") {
                html += '<div class="change"><span>修改理由：</span><pre>' + hireProject.Remark + '</pre></div>';
            }
            $("#ContractTerms").parent()
                .empty().html(html);
            layer.closeLayer("contractterms-layer");
            return false;
        }
    });
    //  发送合作协议
    $("#SendProject").click(function () {
        if (hireProject.ContractTerms == "") {
            layer.showMsg("请填写合作条款");
            return false;
        }
        if (count(hireProject.Milestones, function (a) { return !a.IsDelete; }) == 0) {
            layer.showMsg("请添加项目阶段");
            return false;
        }
        if (!hireProject.IsUpdated) {
            layer.showMsg("没有任何修改");
            return false;
        }
        layer.showConfirm("确定发送合作协议吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Hire/SaveHiredProjectAndMilestones.html",
                    data: {
                        OfferNo: hireProject.OfferNo,
                        ContractTerms: hireProject.ContractTerms,
                        HiredMilestones: translate(hireProject.Milestones),
                        Remark: hireProject.Remark
                    },
                    dataType: "json",
                    type: "post",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            layer.showMsg("发送成功", function () {
                                location.reload();
                            });
                        } else {
                            layer.showMsg("发送失败", function () {
                                location.reload();
                            });
                        }
                    },
                    error: function () {
                        layer.showMsg("发送失败", function () {
                            location.reload();
                        });
                    }
                });
            }
        });
        return false;
    });
    //  撤销合作协议
    $("#RevokeProject").click(function () {
        layer.showConfirm("确定撤销合作协议吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Hire/RevokeHiredProject.html",
                    data: {
                        OfferNo: hireProject.OfferNo
                    },
                    dataType: "json",
                    type: "post",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            layer.showMsg("操作成功", function () {
                                location.reload();
                            });
                        } else {
                            layer.showMsg("操作失败", function () {
                                location.reload();
                            });
                        }
                    },
                    error: function () {
                        layer.showMsg("操作失败", function () {
                            location.reload();
                        });
                    }
                });
            }
        });
    });
    ///  添加项目阶段
    function addProjectMil(obj) {
        hireProject.Milestones.push(obj);
    }
    //  托管项目阶段佣金
    function trusteeshipCommission(id) {
        getHiredMilestone(id, function (obj) {
            $("#TrusteeshipCommission-layer .submitBtn").attr("lang", id);
            $("#TrusteeshipCommission-layer tr").eq(0).html("项目阶段：" + obj.Title + "");
            $("#TrusteeshipCommission-layer tr:eq(1) i").html("￥" + obj.TotalCommission + "元");
            $("#TrusteeshipCommission-layer tr:eq(2) i").html("￥" + obj.AvailableCommission + "元");
            $("#TrusteeshipCommission-layer tr:eq(3) i").html("￥" + (obj.AvailableCommission >= obj.TotalCommission ? 0 : (obj.TotalCommission - obj.AvailableCommission)) + "元");
            $("#TrusteeshipCommission-layer tr:eq(2) input[type='checkbox']").off("click");
            $("#TrusteeshipCommission-layer tr:eq(2) input[type='checkbox']").click(obj, function (data) {
                if ($(this).attr("checked")) {
                    $("#TrusteeshipCommission-layer tr:eq(3) i").html("￥" + (data.data.AvailableCommission >= data.data.TotalCommission ? 0 : (data.data.TotalCommission - data.data.AvailableCommission)) + "元");
                } else {
                    $("#TrusteeshipCommission-layer tr:eq(3) i").html("￥" + data.data.TotalCommission + "元");
                }
            });
            $("#TrusteeshipCommission-layer input[name='id']").val(id);
            layer.layer({ layerId: "TrusteeshipCommission-layer", close_layer: "#TrusteeshipCommission-layer .resetBtn" });
        });
    }
    //  支付项目阶段佣金
    function payMilestoneCommission(id) {
        getHiredMilestone(id, function (obj) {
            $("#PayMilestoneCommission-layer input[name='id']").val(id);
            $("#PayMilestoneCommission-layer .submitBtn").attr("lang", id);
            $("#PayMilestoneCommission-layer tr:eq(1) i").html("￥" + obj.TotalCommission + "元");
            layer.layer({ layerId: "PayMilestoneCommission-layer", close_layer: "#PayMilestoneCommission-layer .resetBtn" });
        });
    }
    //  关闭项目
    $("#CloseProject").click(function () {
        layer.showConfirm("确定要关闭项目吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Hire/CloseProject.html",
                    data: { offerNo: params.OfferNo },
                    dataType: "json",
                    type: "post",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            layer.showMsg("操作成功", function () {
                                location.reload();
                            });
                        } else {
                            layer.showMsg("操作失败", function () {
                                location.reload();
                            });
                        }
                    },
                    error: function () {
                        layer.showMsg("操作失败", function () {
                            location.reload();
                        });
                    }
                });
            }
        });
    });
    //  同意关闭项目
    $("#ConfirmCloseProject").click(function () {
        layer.showConfirm("确定同意关闭项目吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Hire/ConfirmCloseProject.html",
                    data: { offerNo: params.OfferNo },
                    dataType: "json",
                    type: "post",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            layer.showMsg("操作成功", function () {
                                location.reload();
                            });
                        } else {
                            layer.showMsg("操作失败", function () {
                                location.reload();
                            });
                        }
                    },
                    error: function () {
                        layer.showMsg("操作失败", function () {
                            location.reload();
                        });
                    }
                });
            }
        });
    });
    //  拒绝关闭项目
    $("#RefuseCloseProject").click(function () {
        layer.showConfirm("确定要拒绝关闭项目吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Hire/RefuseCloseProject.html",
                    data: { offerNo: params.OfferNo },
                    dataType: "json",
                    type: "post",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            layer.showMsg("操作成功", function () {
                                location.reload();
                            });
                        } else {
                            layer.showMsg("操作失败", function () {
                                location.reload();
                            });
                        }
                    },
                    error: function () {
                        layer.showMsg("操作失败", function () {
                            location.reload();
                        });
                    }
                });
            }
        });
    });
    //  评分效果
    $("#Comment-layer .score-info .hidestar li").mouseover(function () {
        $(this).closest("div.hidestar").prev(".star-level").attr("class", "star-level lv-" + $(this).attr("lang") * 10);
        $(this).closest("div.hidestar").prev(".star-level").find("i").html($(this).attr("lang"));
    }).mouseout(function () {
        $(this).closest("div.hidestar").prev(".star-level").attr("class", "star-level lv-" + $(this).closest("div.hidestar").find("input").val() * 10);
        $(this).closest("div.hidestar").prev(".star-level").find("i").html($(this).closest("div.hidestar").find("input").val());
    }).click(function () {
        $(this).closest("div.hidestar").find("input").val($(this).attr("lang"));
        $(this).closest("div.hidestar").prev(".star-level").find("i").html($(this).attr("lang"));
    });
    //  评论
    $("#btnComment").click(function () {
        layer.layer({ layerId: "Comment-layer", close_layer: "#Comment-layer .resetBtn" });
    });
    $("#Comment-layer .submitBtn").click(function () {
        if ($("#CommentContent").val() == "") {
            layer.showMsg("请填写评论");
        }
        layer.showConfirm("确定提交评论吗?", {
            okfun: function () {
                $.ajax({
                    url: "/Hire/Comment.html",
                    data: {
                        offerNo: params.OfferNo,
                        CommentContent: $("#CommentContent").val(),
                        Tags: $("#savetags").val(),
                        score1: $("input:checked[name='ItemOne']").val(),
                        score2: $("#score2").val(),
                        score3: $("#score3").val(),
                        score4: $("#score4").val()
                    },
                    dataType: "json",
                    type: "post",
                    success: function (res) {
                        if (res.ResultNo == 0) {
                            location.reload();
                        } else {
                            layer.showMsg("操作失败,请刷新页面后再试");
                        }
                    },
                    error: function () {
                        layer.showMsg("操作失败,请刷新页面后再试");
                    }
                });
            }
        });
    });
    //  提醒对方评价
    $("#RemindComment").click(function () {
        $.ajax({
            url: "/Hire/RemindComment.html",
            data: { offerNo: params.OfferNo },
            dataType: "json",
            type: "post",
            success: function (res) {
                if (res.ResultNo == 0) {
                    layer.showMsg("操作成功");
                } else {
                    layer.showMsg("操作失败,请刷新页面后再试");
                }
            },
            error: function () {
                layer.showMsg("操作失败,请刷新页面后再试");
            }
        });
    });
    //  联系方式
    $("#contacts").click(function () {
        layer.layer({ layerId: "hiredusercontacts-layer" });
    });
    function getHiredMilestone(id, okfun) {
        $.ajax({
            url: "/Hire/GetHiredMilestone.html",
            data: { id: id },
            dataType: "json",
            type: "post",
            success: function (res) {
                if (res.ResultNo == 0) {
                    okfun(res.ResultAttachObjectEx);
                } else {
                    layer.showMsg("获取数据失败,请刷新页面后再试");
                }
            },
            error: function () {
                layer.showMsg("获取数据失败,请刷新页面后再试");
            }
        });
    }

    function removeItem(data, a) {
        for (var i = 0; i < data.length; i++) {
            if (a(data[i])) {
                if (data[i].ID <= 0) {
                    data.splice(i, 1);
                } else {
                    data[i].IsDelete = true;
                }
                break;
            }
        }
    }

    function count(data, a) {
        var cnt = 0;
        $.each(data, function (i, n) {
            if (a(n)) {
                cnt++;
            }
        });
        return cnt;
    }

    function translate(data) {
        var text = "[";
        $.each(data, function (i, n) {
            text += "{";
            text += "\"ID\":\"" + n.ID + "\",";
            text += "\"DisplayIndex\":\"" + n.DisplayIndex + "\",";
            text += "\"Title\":\"" + n.Title + "\",";
            text += "\"Content\":\"" + n.Content + "\",";
            text += "\"Remark\":\"" + n.Remark + "\",";
            text += "\"TotalCommission\":\"" + n.TotalCommission + "\",";
            text += "\"IsDelete\":\"" + n.IsDelete + "\",";
            text += "\"StartTime\":\"" + n.StartTime + "\",";
            text += "\"EndTime\":\"" + n.EndTime + "\"";
            text += "},";
        });
        if (text.length > 1) {
            text = text.substring(0, text.length - 1);
        }
        return text + "]";
    }
});