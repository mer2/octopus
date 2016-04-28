define("staticHuyu/huyu-hire-hiredmanage", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var tools = require("staticHuyu/huyu-company-tools");
	var params = require("plugin-params")(module, "huyu");
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
							'<td class="style3">' + changeDateFormat(item.StartTime) + '</td>' +
							'<td class="style3">' + changeDateFormat(item.EndTime) + '</td>' +
							'</tr>';
					});
					$("#ProjectHistory-layer .pop-body tbody").html(html);
					$.joy.layer({ layerId: "ProjectHistory-layer" });
				} else {
				    $.joy.showMsg("获取数据失败,请刷新页面后再试");
				}
			},
			error: function () {
			    $.joy.showMsg("获取数据失败,请刷新页面后再试");
			}
		});
	});
	//  同意条款
	$("#Confirm").click(function () {
	    $.joy.showConfirm("确定同意协议?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/ConfirmHiredProject.html",
					data: { offerNo: params.OfferNo },
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("操作成功", function () {
								location.reload();
							});
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
					}
				});
			}
		});
	});
	//  拒绝条款
	$("#Refuse").click(function () {
		tools.ShowLayer({
			LayerID: "RefuseHiredProject-layer", SubmitID: "#RefuseHiredProject-layer :button", CancleFun: "", SubmitFun: function () {
				var remark = $("#RefuseHiredProject-layer :selected").val();
				if (remark == "") {
				    $.joy.showMsg("请选择拒绝理由");
					return;
				}
				if (remark == "其他") {
					remark = $("#RefuseHiredProject-layer textarea[name='remark']").val();
					if (remark == "") {
					    $.joy.showMsg("请填写拒绝理由");
						return;
					}
				}
				$.ajax({
					url: "/Hire/RefuseHiredProject.html",
					data: { offerNo: params.OfferNo, remark: remark },
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("操作成功", function () {
								location.reload();
							});
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
					}
				});
				$.joy.closeLayer("RefuseHiredProject-layer");
			}
		});
	});
	//  取消雇佣
	$("#CancelHire").click(function () {
		tools.ShowLayer({
			LayerID: "CancelHire-layer", SubmitID: "#CancelHire-layer :button", CancleFun: "", SubmitFun: function () {
				var remark = $("#CancelHire-layer :selected").val();
				if (remark == "") {
				    $.joy.showMsg("请选择拒绝理由");
					return;
				}
				if (remark == "其他") {
					remark = $("#CancelHire-layer textarea[name='remark']").val();
					if (remark == "") {
					    $.joy.showMsg("请填写拒绝理由");
						return;
					}
				}
				$.ajax({
					url: "/Hire/CancleHire.html",
					type: "post",
					dataType: "json",
					data: {
						offerNo: params.OfferNo,
						remark: remark
					},
					success: function (res) {
						if (res.ResultNo == 0) {
							location.reload();
						} else {
						    $.joy.showMsg("操作失败");
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败");
					}
				});
				$.joy.closeLayer("CancelHire-layer");
			}
		});
	});
	//  关闭项目
	$("#CloseProject").click(function () {
	    $.joy.showConfirm("确定要关闭项目吗?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/CloseProject.html",
					data: { offerNo: params.OfferNo },
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("操作成功", function () {
								location.reload();
							});
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
					}
				});
			}
		});
	});
	//  同意关闭项目
	$("#ConfirmCloseProject").click(function () {
	    $.joy.showConfirm("确定同意关闭项目吗?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/ConfirmCloseProject.html",
					data: { offerNo: params.OfferNo },
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("操作成功", function () {
								location.reload();
							});
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
					}
				});
			}
		});
	});
	//  拒绝关闭项目
	$("#RefuseCloseProject").click(function () {
	    $.joy.showConfirm("确定要拒绝关闭项目吗?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/RefuseCloseProject.html",
					data: { offerNo: params.OfferNo },
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("操作成功", function () {
								location.reload();
							});
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
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
		$.joy.layer({ layerId: "Comment-layer", close_layer: "#Comment-layer .resetBtn" });
	});
	$("#Comment-layer .submitBtn").click(function () {
		if ($("#CommentContent").val() == "") {
		    $.joy.showMsg("请填写评论");
		}
		$.joy.showConfirm("确定提交评论吗?", {
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
						    $.joy.showMsg("操作成功", function () {
								location.reload();
							});
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
					}
				});
			}
		});
	});
	//  请求托管
	$("#HiredMilestones tr.table-tr1 td a[title='请求托管']").click(function () {
		var id = $(this).attr("lang");
		$.joy.showConfirm("您确定发送请求?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/AskWarrant.html",
					data: {
						id: id
					},
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("发送成功");
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
					}
				});
			}
		});
	});
	//  请求付款
	$("#HiredMilestones tr.table-tr1 td a[title='请求付款']").click(function () {
		var id = $(this).attr("lang");
		$.joy.showConfirm("您确定发送请求?", {
			okfun: function () {
				$.ajax({
					url: "/Hire/AskPay.html",
					data: {
						id: id
					},
					dataType: "json",
					type: "post",
					success: function (res) {
						if (res.ResultNo == 0) {
						    $.joy.showMsg("发送成功");
						} else {
						    $.joy.showMsg("操作失败", function () {
								location.reload();
							});
						}
					},
					error: function () {
					    $.joy.showMsg("操作失败", function () {
							location.reload();
						});
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
				    $.joy.showMsg("操作成功");
				} else {
				    $.joy.showMsg("操作失败,请刷新页面后再试");
				}
			},
			error: function () {
			    $.joy.showMsg("操作失败,请刷新页面后再试");
			}
		});
	});
	//  联系方式
	$("#contacts").click(function () {
		$.joy.layer({ layerId: "hiredusercontacts-layer" });
	});
	function changeDateFormat(time) {
		if (time != null) {
			var date = new Date(parseInt(time.replace("/Date(", "").replace(")/", ""), 10));
			var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
			var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
			return date.getFullYear() + "." + month + "." + currentDate;
		}
		return "";
	}
});