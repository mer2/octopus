define("staticHuyu/huyu-tender-mytenderoffer", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	var $layer = require("layer");
	var $passport = require("staticHuyu/huyu-navigate");

	$(function () {
		MilestoneInit();
		if ($(".comment-for-1").length > 0) {
			$.post("/AjaxJson/GetComment.html", { userId: params.UserID, usertype: 1, target: 11, targetValue: params.ID }, function (data) {
				if (data.ResultNo == 0) {
					$.post("/AjaxJson/GetScore.html", { userId: params.UserID, target: 11, targetValue: params.ID }, function (data1) {
						if (data1.ResultNo == 0) {
							var objs = data1.ResultAttachObject;
							var score = 0;
							var strs = '';
							if (objs != null && objs.length > 0) {
								for (var i = 0; i < objs.length; i++) {
									if (objs[i].IsDefault) {
										score = objs[i].RateValue;
									} else {
										strs += '<li>' + objs[i].RateName + '：<span class="star-level lv-' + objs[i].RateValue + '0"><em></em></span><label><i>' + objs[i].RateValue + '</i>分</label></li>';
									}
								}
							}
							var html = '<dt>乙方对您的评价：<strong class="commentscore"><em class="score' + score + '"></em>' + GetCommentScore(score) + '</strong></dt>';
							html += '<dd>';
							html += '<div class="info-star">';
							html += '<ul class="score-info">';
							html += strs;
							html += '</ul>';
							html += '</div>';
							html += '<div class="txt">' + data.ResultAttachObject.CommentContent + '</div>';
							html += '</dd>';
							$(".comment-for-1").html(html);
						}
					});
				}
			});
		}
		if ($(".comment-for-2").length > 0) {
			$.post("/AjaxJson/GetComment.html", { userId: params.OfferUserID, usertype: 1, target: 11, targetValue: params.ID }, function (data) {
				if (data.ResultNo == 0) {
					$.post("/AjaxJson/GetScore.html", { userId: params.OfferUserID, target: 11, targetValue: params.ID }, function (data1) {
						if (data1.ResultNo == 0) {
							var objs = data1.ResultAttachObject;
							var score = 0;
							var strs = '';
							if (objs != null && objs.length > 0) {
								for (var i = 0; i < objs.length; i++) {
									if (objs[i].IsDefault) {
										score = objs[i].RateValue;
									} else {
										strs += '<li>' + objs[i].RateName + '：<span class="star-level lv-' + objs[i].RateValue + '0"><em></em></span><label><i>' + objs[i].RateValue + '</i>分</label></li>';
									}
								}
							}
							var html = '<dt>您对乙方的评价：<strong class="commentscore"><em class="score' + score + '"></em>' + GetCommentScore(score) + '</strong></dt>';
							html += '<dd>';
							html += '<div class="info-star">';
							html += '<ul class="score-info">';
							html += strs;
							html += '</ul>';
							html += '</div>';
							html += '<div class="txt">' + data.ResultAttachObject.CommentContent + '</div>';
							html += '</dd>';
							$(".comment-for-2").html(html);
						}
					});
				}
			});
		}
		$(".show-history").click(function () {
			var version = $(this).attr("lang");
			$.post("/AjaxJson/GetProject.html", { version: version, offerNo: params.OfferNo }, function (data) {
				if (data.ResultNo == 0) {
					var project = data.Project;
					$("#show-history-layer pre").html(project.ContractTerms);
					var html = '<tr><th class="style1">项目名称</th><th class="style2">佣金金额</th><th class="style3">开始日期</th><th class="style3">交付日期</th></tr>';
					for (var i = 0; i < data.Milestones.length; i++) {
						var obj = data.Milestones[i];
						html += '<tr><td class="style1">' + (i + 1) + '.' + obj.Title + '</td><td class="style2">￥' + parseInt(obj.TotalCommission) + '</td><td class="style3">' + GetDate(obj.StartTime) + '</td><td class="style3">' + GetDate(obj.EndTime) + '</td></tr>';
					}
					$("#show-history-layer table").html(html);
					$layer.layer({ layerId: "show-history-layer", close_layer: ".close" });
				} else {
					$layer.showMsg("查询失败");
				}
			});
			return false;
		});
		$("#cancelchoosetenderoffer").click(function () {
			var id = $(this).attr("lang");
			$passport.showLogin(function () {
				$layer.showConfirm("您确定要取消此标重新选择吗？", {
					okfun: function () {
						$.post("/AjaxJson/CancelChooseTenderBidder.html", { id: id }, function (data) {
							if (data.ResultNo == 0) {
								$layer.showMsg("取消选标成功！", function () {
									location.href = "/Tender/TenderOffer/" + params.OfferNo + ".html#selectoffer";
								});
							} else {
								$layer.showMsg(data.ResultDescription);
							}
						});
					}
				});
			});
		});
		$(".user-phone").click(function () {
			$layer.layer({ layerId: "user-phone-layer", close_layer: ".close" });
			return false;
		});
		$(".append-money").click(function () {
			$layer.layer({ layerId: "append-money-layer", close_layer: ".close" });
			return false;
		});
		$("#append-money-layer .append-input").keyup(function () {
			var val = parseInt($(this).val());
			if (isNaN(val)) {
				$(this).val("");
				$(this).parents("tr").next().find("input").val("");
			} else {
				$(this).val(val);
				var oldMoney = parseInt($(this).parents("tr").attr("lang"));
				if (!isNaN(oldMoney) && oldMoney > 0) {
					$(this).parents("tr").next().find("input").val(val + oldMoney);
				}
			}
		});

		$("#append-money-layer .append-input").focus(function () {
			var val = parseInt($(this).val());
			if (isNaN(val)) {
				$(this).val("");
			} else {
				$(this).val(val);
			}
		});

		$("#append-money-layer .append-input").blur(function () {
			var val = parseInt($(this).val());
			if (isNaN(val)) {
				$(this).val("");
				$(this).parents("tr").next().find("input").val("");
			} else {
				$(this).val(val);
				var oldMoney = parseInt($(this).parents("tr").attr("lang"));
				if (!isNaN(oldMoney) && oldMoney > 0) {
					$(this).parents("tr").next().find("input").val(val + oldMoney);
				}
			}
		});

		$("#append-money-layer .submit").click(function () {
			var val = parseInt($("#append-money-layer .append-input").val());
			if (isNaN(val)) {
				$layer.showMsg("输入的金额无效！");
			} else {
				location.href = "/Tender/AppendCollocationMoney/" + params.OfferNo + "/" + val + ".html";
			}
		});

		$("#add-tender-milestone").click(function () {
			$(".tender-milestone-list tr").removeClass("updateing");
			$layer.layer({ layerId: "tender-milestone", close_layer: ".close" });
			$("#title").val("");
			$("#startTime").val("");
			$("#endTime").val("");
			$("#remark").val("");
			$("#totalCommission").val("");
			$("#remarkContent").val("");
			ShowEdit();
			return false;
		});
		$("#startTime").datepicker({
			dateFormat: "yy.mm.dd",
			changeMonth: true,
			numberOfMonths: 1,
			onSelect: function (selectedDate) {
				$("#endTime").datepicker("option", "minDate", selectedDate);
			}
		});
		$("#endTime").datepicker({
			defaultDate: "+1w",
			dateFormat: "yy.mm.dd",
			changeMonth: true,
			numberOfMonths: 1
		});
		$("#totalCommission").blur(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$("#totalCommission").keyup(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$(".tender-milestone-list").on("click", ".delete-row", function () {
			$(".tender-milestone-list tr").removeClass("updateing");
			var $tr = $(this).parents("tr");
			$tr.addClass("updateing");
			$layer.layer({ layerId: "delete-row-layer", close_layer: ".close" });
			return false;
		});

		$("#delete-row-layer .submitBtn").click(function () {
			var content = $("#delete-row-layer textarea").eq(0).val();
			if (content == '') {
				$layer.showMsg("请输入删除理由");
				return false;
			}
			var $tr = $(".tender-milestone-list .updateing").eq(0);
			$tr.addClass("del");
			if ($tr.next().find(".change").length > 0) {
				$tr.next().find("pre").eq(1).html(content);
			} else {
				$tr.next().find("td:last").append('<div class="change"><span>修改理由：</span><pre>' + content + '</pre></div>');
			}
			ShowEdit();
			$(".close").click();
			$tr.addClass("updateing");
			return false;
		});

		$(".tender-milestone-list").on("click", ".update-row", function () {
			$(".tender-milestone-list tr").removeClass("updateing");
			var $tr = $(this).parents("tr");
			$tr.next().addClass("updateing");
			$tr.addClass("updateing");
			$("#title").val($tr.find("td").eq(1).text().split("、")[1]);
			var totalCommission = parseInt($tr.find("td").eq(2).text().replace("￥", ""));
			$("#totalCommission").val(totalCommission == isNaN ? "" : totalCommission);
			$("#startTime").val($tr.find("td").eq(3).text());
			$("#endTime").val($tr.find("td").eq(4).text());
			$("#remark").val($tr.next().find("pre").eq(0).text());
			$("#remarkContent").val($tr.next().find("pre").eq(1).text());
			$layer.layer({ layerId: "tender-milestone", close_layer: ".close" });
			ShowEdit();
			return false;
		});
		$(".tender-milestone-list").on("click", ".icon", function () {
			if ($(this).hasClass("icon-close")) {
				$(this).addClass("icon-open");
				$(this).removeClass("icon-close");
				$(this).parents("tr").next(".table-tr2").show();
			} else {
				$(this).addClass("icon-close");
				$(this).removeClass("icon-open");
				$(this).parents("tr").next(".table-tr2").hide();
			}
		});
		$(".update-tender-project").click(function () {
			$(".contractterms dl dd pre").hide();
			$(".contractterms dl dd div").show();
			$(".tender-project-remark").show();
			$(this).hide();
			ShowEdit();
			return false;
		});
		$("#submitBtn").click(function () {
			var title = $("#title").val();
			if (title == "") {
				$layer.showMsg("请输入项目阶段名称");
				return false;
			}
			var startTime = $("#startTime").val();
			if (startTime == "") {
				$layer.showMsg("请输入项目阶段开始时间");
				return false;
			}
			var endTime = $("#endTime").val();
			if (endTime == "") {
				$layer.showMsg("请输入项目阶段交付时间");
				return false;
			}
			var remark = $("#remark").val();
			if (remark == "") {
				$layer.showMsg("请输入项目阶段说明");
				return false;
			}
			var totalCommission = $("#totalCommission").val();
			if (totalCommission == "") {
				$layer.showMsg("请输入项目阶段佣金金额");
				return false;
			}
			var content = '';
			if ($("#remarkContent").length > 0) {
				content = $("#remarkContent").val();
				if (content == "") {
					$layer.showMsg("请填写理由");
					return false;
				}
			}
			if ($(".updateing").length == 0) {
				var i = $(".tender-milestone-list .table-tr1").length + 1;
				var html = '<tr class="odd table-tr1">';
				html += '<td class="style1"><span class="icon icon-close"></span></td>';
				html += '<td class="style2">' + i + '、' + title + '</td>';
				html += '<td class="style3"><i>￥</i>' + totalCommission + '</td>';
				html += '<td class="style4">' + startTime + '</td>';
				html += '<td class="style4">' + endTime + '</td>';
				html += '<td class="style5">草稿</td>';
				html += '<td class="style6"><span><a title="编辑" href="javascript:void(0);" class="update-row">编辑</a></span><span><a title="删除" href="javascript:void(0);" class="delete-row">删除</a></span></td>';
				html += '</tr>';
				html += '<tr class="table-tr2">';
				html += '<td></td>';
				html += '<td colspan="6">';
				html += '<pre class="txt">' + remark + '</pre>';
				html += '<div class="change"><span>修改理由：</span><pre>' + content + '</pre></div>';
				html += '</td>';
				html += '</tr>';
				$(".tender-milestone-list").append(html);
			} else {
				var $tr = $(".updateing").eq(0);
				$tr.next().addClass("updateing");
				$tr.addClass("updateing");
				$tr.find("td").eq(1).html($tr.find("td").eq(1).text().split("、")[0] + "、" + title);
				$tr.find("td").eq(2).html("<i>￥</i>" + totalCommission);
				$tr.find("td").eq(3).html(startTime);
				$tr.find("td").eq(4).html(endTime);
				$tr.next().find("pre").eq(0).html(remark);
				if (content != "") {
					if ($tr.next().find(".change").length > 0) {
						$tr.next().find("pre").eq(1).html(content);
					} else {
						$tr.next().find("td:last").append('<div class="change"><span>修改理由：</span><pre>' + content + '</pre></div>');
					}
				}
				$(".tender-milestone-list tr").removeClass("updateing");
			}
			$("#title").val("");
			$("#startTime").val("");
			$("#endTime").val("");
			$("#remark").val("");
			$("#totalCommission").val("");
			$("#remarkContent").val("");
			$(".close").click();
		});

		$("#send-project-btn").click(function () {
			if ($("tbody .table-tr1").length == 0) {
				$layer.showMsg("请创建项目阶段！");
				return false;
			}
			if ($(".contractterms dl dd div .short").val() == "") {
				$layer.showMsg("请输入合作条款具体内容！");
				return false;
			}
			var json = '[';
			var i = 0;
			$("tbody .table-tr1").each(function () {
				var $tr = $(this);
				if (i > 0) {
					json += ',';
				}
				var milestoneID = parseInt($tr.attr("lang"));
				milestoneID = isNaN(milestoneID) ? 0 : milestoneID;
				json += '{';
				json += '"MilestoneID":"' + milestoneID + '",';
				json += '"Title":"' + $tr.find("td").eq(1).html().split("、")[1] + '",';
				json += '"TotalCommission":"' + parseInt($tr.find("td").eq(2).text().replace("￥", "")) + '",';
				json += '"StartTime":"' + $tr.find("td").eq(3).html() + '",';
				json += '"EndTime":"' + $tr.find("td").eq(4).html() + '",';
				json += '"Content":"' + $tr.next().find("pre").eq(0).html() + '"';
				if ($tr.next().find("pre").length > 1) {
					json += ',"Remark":"' + $tr.next().find("pre").eq(1).html() + '"';
				}

				if ($tr.hasClass("del")) {
					json += ',"IsDelete":' + true;
				}
				json += '}';
				i++;
			});
			json += ']';
			$layer.showConfirm("你确定提交吗？", {
				okfun: function () {
					var html = '<form method="POST" id="sendprojectform" style="display:none;" action="/Tender/SendTenderProject/' + params.OfferNo + '.html">';
					html += '<input type="hidden" value="" name="json" />';
					html += '<input type="hidden" value="" name="contractTerms" />';
					html += '<input type="hidden" value="" name="remark" />';
					html += '</form>';
					$("body").append(html);
					$("#sendprojectform input").eq(0).val(json);
					$("#sendprojectform input").eq(1).val($(".contractterms dl dd div .short").val());
					$("#sendprojectform input").eq(2).val($("#update-tender-project-remark").val());
					//$("#sendprojectform").submit();
					$.post("/AjaxJson/SendTenderProject/" + params.OfferNo + ".html", $("#sendprojectform").serialize(), function (data) {
						if (data.ResultNo == 0) {
							location.href = location.href;
						} else {
							$("#sendprojectform").remove();
							$layer.showMsg(data.ResultDescription);
						}
					});
				}
			});
		});
		$("#cancellation-terms").click(function () {
			$layer.showConfirm("您确定要撤销合作条款吗？", {
				okfun: function () {
					var html = '<form method="POST" id="cancellationtermsform" style="display:none;" action="/Tender/CancellationTerms/' + params.OfferNo + '.html">';
					html += '<input type="hidden" value="" name="remark" />';
					html += '</form>';
					$("body").append(html);
					$("#cancellationtermsform input").eq(0).val("");
					//$("#cancellationtermsform").submit();
					$.post("/AjaxJson/CancellationTerms/" + params.OfferNo + ".html", $("#cancellationtermsform").serialize(), function (data) {
						if (data.ResultNo == 0) {
							location.href = location.href;
						} else {
							$("#cancellationtermsform").remove();
							$layer.showMsg(data.ResultDescription);
						}
					});
				}
			});
		});
		$(".action-add").click(function () {
			var $tr = $(this).parents("tr");
			var $title = $tr.find("td").eq(1);
			var title = $title.text().substring($title.text().indexOf("、") + 1, $title.text().length - $title.text().indexOf("、") + 1);
			var $money = $tr.find("td").eq(2);
			var money = parseInt($money.attr("lang"));
			$("#business-action7 tr").eq(0).find("td").html('项目阶段：' + title);
			$("#business-action7 tr").eq(1).find("td").html('需托管佣金：<i>￥' + money + '元</i>');
			$("#business-action7 tr").eq(1).attr("lang", money);
			var totalMoney = parseInt($("#business-action7 tr").eq(2).attr("lang"));
			if (totalMoney >= money) {
				$("#business-action7 tr").eq(3).find("i").html("￥" + 0 + "元");
			} else {
				$("#business-action7 tr").eq(3).find("i").html("￥" + (money - totalMoney) + "元");
			}
			$("#business-action7 .submitBtn").attr("lang", $tr.attr("lang"));
			$("#business-action7 :checkbox").attr("checked", true);
			$layer.layer({ layerId: "business-action7", close_layer: ".close" });
		});
		$("#business-action7 :checkbox").click(function () {
			var money = parseInt($("#business-action7 tr").eq(1).attr("lang"));
			var totalMoney = parseInt($("#business-action7 tr").eq(2).attr("lang"));
			if ($(this).attr("checked")) {
				if (totalMoney >= money) {
					$("#business-action7 tr").eq(3).find("i").html("￥" + 0 + "元");
				} else {
					$("#business-action7 tr").eq(3).find("i").html("￥" + (money - totalMoney) + "元");
				}
			} else {
				$("#business-action7 tr").eq(3).find("i").html("￥" + (money) + "元");
			}
		});
		$("#business-action7 .submitBtn").click(function () {
			var html = '<form method="POST" id="collocationmoneyfrom" style="display:none;" action="/Tender/CollocationMoney/' + $(this).attr("lang") + '.html">';
			html += '<input type="hidden" value="' + ($("#business-action7 :checked").length == 1 ? 0 : 1) + '" name="allpay" />';
			html += '</form>';
			$("body").append(html);
			$("#collocationmoneyfrom").submit();
		});
		$(".pay-money").click(function () {
			var $tr = $(this).parents("tr");
			var $money = $tr.find("td").eq(2);
			var money = parseInt($money.attr("data"));
			$("#business-action8 tr").eq(1).find("td").html('<strong>支付金额：<i>￥' + money + '元</i></strong>');
			$("#pay-money-btn").attr("lang", $tr.attr("lang"));
			$layer.layer({ layerId: "business-action8", close_layer: ".close" });
			return false;
		});
		$("#pay-money-btn").click(function () {
			var id = $(this).attr("lang");
			var html = '<form method="POST" id="pay-money-from" style="display:none;" action="/Tender/PayMoney/' + id + '.html">';
			html += '</form>';
			$("body").append(html);
			$("#pay-money-from").submit();
		});

		$(".rejected-pay-money").click(function () {
			var $tr = $(this).parents("tr");
			$("#rejected-pay-money-layer .submitBtn").attr("lang", $tr.attr("lang"));
			$layer.layer({ layerId: "rejected-pay-money-layer", close_layer: ".close" });
			return false;
		});
		$("#rejected-pay-money-layer .submitBtn").click(function () {
			var remark = $("#rejected-pay-money-layer .rejected-remark").val();
			if (remark == "") {
				$layer.showMsg("请输入拒绝理由！");
				return false;
			}
			var id = $(this).attr("lang");
			$.post("/AjaxJson/RejectedPayMoney/" + id + ".html", { remark: remark }, function (data) {
				if (data.ResultNo == 0) {
					location.href = location.href;
				} else {
					$layer.showMsg(data.ResultDescription);
				}
			});
		});
		$(".close-project").click(function () {
			var val = $(this).attr("lang");
			var v = $(this).attr("data-views");
			$layer.showConfirm(v, {
				okfun: function () {
					$.post("/AjaxJson/CloseProject/" + params.OfferNo + ".html", { val: val }, function (data) {
						if (data.ResultNo == 0) {
							location.href = location.href;
						} else {
							$layer.showMsg(data.ResultDescription);
						}
					});
				}
			});
		});
		$(".show-comment").click(function () {
			$layer.layer({ layerId: "show-comment-layer", close_layer: ".close" });
			return false;
		});
		$(".hidestar li").mouseover(function () {
			var score = $(this).attr("lang");
			var $li = $(this).parents("li");
			$li.find("label").show();
			$li.find("i").html(score);
			$li.find("span").attr("class", "star-level lv-" + score + "0");
		});
		$(".hidestar li").click(function () {
			var score = $(this).attr("lang");
			var $li = $(this).parents("li");
			$li.find("span").attr("class", "star-level lv-" + score + "0");
			$li.find("i").html(score);
			$li.find("ul").attr("lang", score);
		});
		$(".hidestar ul").mouseout(function () {
			var $li = $(this).parents("li");
			var score = $li.find("ul").attr("lang");
			$li.find("span").attr("class", "star-level lv-" + score + "0");
			$li.find("i").html(score);
		});
		$("#show-comment-layer .submitBtn").click(function () {
			var defaultscore = parseInt($("#show-comment-layer .radiobox :radio:checked").val());
			if (isNaN(defaultscore) || defaultscore == 0) {
				$layer.showMsg("请选择满意度");
				return false;
			}
			var content = $("#show-comment-layer .short").val();
			if (content == "") {
				$layer.showMsg("请输入评论内容");
				return false;
			}
			var json = '[{"RateID":"1","RateName":"满意度","RateValue":"' + defaultscore + '","IsDefault":true}';
			$(".hidestar ul").each(function () {
				var score = $(this).attr("lang");
				var s = parseInt(score);
				if (isNaN(s) || s == 0) {
					$layer.showMsg("请为甲方打分！");
					return false;
				}
				var $li = $(this).parents("li");
				json += ',{"RateID":"' + $li.attr("lang") + '","RateName":"' + $li.attr("data") + '","RateValue":"' + score + '"}';
			});
			json += ']';
			var tags = $("#savetags").val();
			$.post("/AjaxJson/AddComment.html", { target: 11, targetValue: params.ID, userType: 2, userId: params.UserID, userName: params.UserName, targetName: params.Title, tags: tags, content: content, score: json, url: params.Url }, function (data) {
				if (data.ResultNo == 0) {
					$.post("/AjaxJson/CommentOffer.html", { id: params.OfferNo, val: "10" });
					$layer.showMsg("评论成功", function () {
						location.href = location.href;
					});
				} else {
					$layer.showMsg("评论失败！");
				}
			});
		});
	});

	function ShowEdit() {
		$("#send-project-btn").parents("li").show();//.next(".edit").show();
		$(".business .business-detailed .edit").show();
	}

	function GetDate(val) {
		var date = new Date(parseInt(val.substring(val.indexOf("(") + 1, val.indexOf(")"))));
		var str = date.getFullYear() + ".";
		if (date.getMonth() + 1 < 10) {
			str += "0" + (date.getMonth() + 1) + ".";
		} else {
			str += (date.getMonth() + 1) + ".";
		}
		if (date.getDate() < 10) {
			str += "0" + date.getDate();
		} else {
			str += date.getDate();
		}
		return str;
	}

	function GetCommentScore(v) {
		switch (v) {
			case 5:
				return "非常满意";
			case 4:
				return "满意";
			case 3:
				return "一般";
			case 2:
				return "不满意";
			case 1:
				return "非常不满意";
		}
	}
	//关闭项目
	$(function () {
		$(".close-project-remark").click(function () {
			$("#close-project-remark-layer h2").html(this.value);
			$("#close-project-remark-layer .close-project-remark").val($(this).attr("data-views"));
			$("#close-project-remark-layer .submitBtn").attr("lang", this.lang);
			$layer.layer({ layerId: "close-project-remark-layer", close_layer: ".close" });
			return false;
		});

		$("#close-project-remark-layer .submitBtn").click(function () {
			$.post("/AjaxJson/CloseProject", { id: params.OfferNo, val: $(this).attr("lang"), remark: $("#close-project-remark-layer .close-project-remark").val() }, function (data) {
				if (data.ResultNo == 0) {
					location.href = location.href;
				} else {
					$layer.showMsg(data.ResultDescription);
				}
			});
			return false;
		});

		$(".wait-comment").click(function () {
			$.post("/AjaxJson/WaitComment", { id: params.OfferNo }, function (data) {
				if (data.ResultNo == 0) {
					$layer.showMsg("提醒成功");
				} else {
					$layer.showMsg("提醒失败");
				}
			});
			return false;
		});
	});
	
	function MilestoneInit() {
		$.post("/AjaxJson/GetTenderMilestonesByNo.html", { id: params.OfferNo }, function (data) {
			if (data.ResultNo == 0) {
				var objs = data.ResultAttachObject;
				for (var i = 0; i < objs.length; i++) {
					var hasChange = false;
					var obj = objs[i];
					var $tr = $(".tender-milestone-list .table-tr1[lang='" + obj.ID + "']");
					var $title = $tr.find("td").eq(1);
					var title = $title.text().substring($title.text().indexOf("、") + 1, $title.text().length - $title.text().indexOf("、") + 1);
					if (title != obj.Title) {
						$title.html($title.text().substring(0, $title.text().indexOf("、")) + "、" + "<i>" + title + "</i>");
						hasChange = true;
					}
					var $money = $tr.find("td").eq(2);
					var money = parseInt($money.text().replace(/[ ]/g, "").replace("￥", ""));
					if (money != obj.TotalCommission) {
						$money.html("<i>￥" + money + "<i>");
						hasChange = true;
					}
					var $starttime = $tr.find("td").eq(3);
					var startTime = $starttime.text().replace(/[ ]/g, "");
					if (startTime != GetDate(obj.StartTime)) {
						$starttime.html("<i>" + startTime + "</i>");
						hasChange = true;
					}
					var $endtime = $tr.find("td").eq(4);
					var endtime = $endtime.text().replace(/[ ]/g, "");
					if (endtime != GetDate(obj.EndTime)) {
						$endtime.html("<i>" + endtime + "</i>");
						hasChange = true;
					}
					var $tr2 = $tr.next();
					var $p = $tr2.find("pre");
					var content = $p.html();
					if (content != obj.Content) {
						$p.html("<i>" + content + "<i>");
						hasChange = true;
					}
					if (hasChange) {
						$tr.next().find(".change").show();
					}
				}
			}
		});
	}
});
