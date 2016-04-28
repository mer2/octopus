define("staticHuyu/huyu-tender-mytenderbidder", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	var $layer = require("layer");
	$(function () {
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
							var html = '<dt>您对甲方的评价：<strong class="commentscore"><em class="score' + score + '"></em>' + GetCommentScore(score) + '</strong></dt>';
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
							var html = '<dt>甲方对您的评价：<strong class="commentscore"><em class="score' + score + '"></em>' + GetCommentScore(score) + '</strong></dt>';
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
		MilestoneInit();
		$("#confirm-tender-project").click(function () {
			$layer.showConfirm("您确定同意最新的合作条款吗？", {
				okfun: function () {
					ConfirmTenderProject(20, "");
				}
			});
		});
		$("#cancel-tender-project").click(function () {
			$layer.layer({ layerId: "cancel-tender-project-layer", close_layer: ".close" });
			//$layer.showConfirm("您确定拒绝最新的合作条款吗？", {
			//	okfun: function () {
			//		ConfirmTenderProject(99);
			//	}
			//});
		});

		$("#cancel-tender-project-layer .submitBtn").click(function () {
			var val = $("#cancel-tender-project-layer .cancel-tender-project-remark").val();
			if (val == "") {
				$layer.showMsg("请输入拒绝理由");
				return false;
			}
			ConfirmTenderProject(99, val);
			return false;
		});

		//$(".wait-pay").click(function () {
		//	var id = $(this).attr("lang");
		//	$layer.showConfirm("您确定项目已做完，需要向甲方申请付款吗？", {
		//		okfun: function () {
		//			$.post("/AjaxJson/WaitPay/" + id + ".html", function (data) {
		//				if (data.ResultNo == 0) {
		//					location.href = location.href;
		//				} else {
		//					$layer.showMsg(data.ResultDescription);
		//				}
		//			});
		//		}
		//	});
		//});

		$(".wait-pay").click(function () {
			var id = $(this).attr("lang");
			$("#wait-pay-layer .submitBtn").attr("lang", id);
			$.post("/AjaxJson/IsWaitPayMoney", { id: id }, function (data) {
				if (data.ResultNo == 0) {
					$layer.layer({ layerId: "wait-pay-layer", close_layer: ".close" });
				} else {
					$layer.showMsg(data.ResultDescription);
				}
			});
			return false;
		});

		$("#wait-pay-layer .submitBtn").click(function () {
			var id = $(this).attr("lang");
			var val = $("#wait-pay-layer .wait-pay-remark").val();
			$.post("/AjaxJson/WaitPayMoney", { id: id, remark: val }, function (data) {
				if (data.ResultNo == 0) {
					$.joy.closeLayer("wait-pay-layer");
				} else {
					$layer.showMsg(data.ResultDescription);
				}
			});
		});

		$(".wait-collocation").click(function () {
			var id = $(this).attr("lang");
			$("#wait-collocation-layer .submitBtn").attr("lang", id);
			$.post("/AjaxJson/IsWaitCollocation", { id: id }, function (data) {
				if (data.ResultNo == 0) {
					$layer.layer({ layerId: "wait-collocation-layer", close_layer: ".close" });
				} else {
					$layer.showMsg(data.ResultDescription);
				}
			});
			return false;
		});

		$("#wait-collocation-layer .submitBtn").click(function () {
			var id = $(this).attr("lang");
			var val = $("#wait-collocation-layer .wait-collocation-remark").val();
			$.post("/AjaxJson/WaitCollocation", { id: id, remark: val }, function (data) {
				if (data.ResultNo == 0) {
					$.joy.closeLayer("wait-collocation-layer");
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
		$(".user-phone").click(function () {
			$layer.layer({ layerId: "user-phone-layer", close_layer: ".close" });
			return false;
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
			var json = '[{"RateID":"11","RateName":"满意度","RateValue":"' + defaultscore + '","IsDefault":true}';
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
			$.post("/AjaxJson/AddComment.html", { target: 11, targetValue: params.ID, userType: 1, userId: params.OfferUserID, userName: params.OfferUserName, targetName: params.Title, tags: tags, content: content, score: json, url: params.Url }, function (data) {
				if (data.ResultNo == 0) {
					$.post("/AjaxJson/CommentOffer.html", { id: params.OfferNo, val: "01" });
					$layer.showMsg("评论成功", function () {
						location.href = location.href;
					});
				} else {
					$layer.showMsg("评论失败！");
				}
			});
		});
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
	});

	function ConfirmTenderProject(status, remark) {
		var html = '<form method="POST" id="confirmprojectform" style="display:none;" action="/Tender/ConfirmTenderProject/' + params.OfferNo + '.html">';
		html += '<input type="hidden" value="' + status + '" name="status" />';
		html += '<input type="hidden" value="' + remark + '" name="remark" />';
		html += '</form>';
		$("body").append(html);
		//$("#confirmprojectform").submit();
		$.post("/AjaxJson/ConfirmTenderProject/" + params.OfferNo + ".html", $("#confirmprojectform").serialize(), function (data) {
			if (data.ResultNo == 0) {
				location.href = location.href;
			} else {
				$("#confirmprojectform").remove();
				$layer.showMsg(data.ResultDescription);
			}
		});
	}

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
});