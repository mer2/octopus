define("staticHuyu/huyu-tender-tenderoffer", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var params = require("plugin-params")(module, "huyu");
	var $layer = require("layer");
	var $upload = require("staticCommon/joy-upload");
	var $passport = require("staticHuyu/huyu-navigate");
	var setting = require("staticHuyu/huyu-config");

	$upload.attachedFiles(".attach-list");
	$(function () { 
		$(window).scroll(function () {
			var wt = $(window).height();
			var st = $(document).scrollTop();
			var mt = $(".business .business-detailed .edit").next().offset().top - wt;
			if (st < mt) {
				$(".business .business-detailed .edit").addClass("fixed");
			} else {
				$(".business .business-detailed .edit").removeClass("fixed");
			};
		});
		$(".wantlogin").click(function () {
			$passport.showLogin(function (user) {
				location.href = location.href;
			});
			return false;
		});

		$(".append-money").click(function () {
			$layer.layer({ layerId: "append-money-layer", close_layer: ".close" });
			return false;
		});

		$(".tip-off").click(function () {
			$layer.layer({ layerId: "tip-off-layer", close_layer: ".close" });
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
				var money = parseInt($(this).parents("table").attr("lang"));
				if (money > 0) {
					if (money > oldMoney + val) {
						this.value = "";
						return false;
					}
				}
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

		$(".look-close").click(function () {
			$.post("/AjaxJson/LookTenderBidder.html", { id: $(this).attr("lang") });
		});

		$(".choose").on("click", ".choosebidder", function () {
			var id = $(this).parent().attr("lang");
			$passport.showLogin(function () {
				$layer.showConfirm("您确定要选择此标吗？", {
					okfun: function () {
						$.post("/AjaxJson/ChooseTenderBidder.html", { id: id }, function (data) {
							if (data.ResultNo == 0) {
								$layer.showMsg("选标成功", function () {
									location.href = "/Tender/TenderOfferManager/" + params.OfferNo + ".html";
								});
							} else {
								$layer.showMsg(data.ResultDescription);
							}
						});
					}
				});
			});
		});
		$(".choose").on("click", ".choosebidder2", function () {
			var id = $(this).parent().attr("lang");
			var $this = $(this);
			$passport.showLogin(function () {
				$layer.showConfirm("您确定要备选此标吗？", {
					okfun: function () {
						$.post("/AjaxJson/RemarkTenderBidder.html", { id: id }, function (data) {
							if (data.ResultNo == 0) {
								$this.parents(".detail").next(".choose-state").show();
								$this.parent().append('<a href="javascript:void(0)" class="btns btn-h23-c">已备选</a><a href="javascript:void(0)" title="取消备选" class="agreement cancel-bidder-status-2">取消备选</a>');
								$this.remove();
							} else {
								$layer.showMsg(data.ResultDescription);
							}
						});
					}
				});
			});
		});

		$(".choose").on("click", ".cancel-bidder-status-2", function () {
			var id = $(this).parent().attr("lang");
			var $this = $(this);
			$passport.showLogin(function () {
				$layer.showConfirm("您确定要取消备选吗？", {
					okfun: function () {
						$.post("/AjaxJson/RemarkCancelTenderBidder.html", { id: id }, function (data) {
							if (data.ResultNo == 0) {
								var $parent = $this.parent();
								$this.parents(".detail").next(".choose-state").hide();
								if ($parent.find(".choosebidder").length > 0) {
									$parent.html('<a href="javascript:void(0)" class="btns btn-h23-b choosebidder">选择此方案</a><a href="javascript:void(0)" title="备选" class="btns btn-h23 choosebidder2">备选</a>');
								} else {
									$parent.html('<a href="javascript:void(0)" title="备选" class="btns btn-h23 choosebidder2">备选</a>');
								}
							} else {
								$layer.showMsg(data.ResultDescription);
							}
						});
					}
				});
			});
		});

		$("#cancelchoosetenderoffer").click(function () {
			var id = $(this).parent().attr("lang");
			$passport.showLogin(function () {
				$layer.showConfirm("您确定要取消此标重新选择吗？", {
					okfun: function () {
						$.post("/AjaxJson/CancelChooseTenderBidder.html", { id: id }, function (data) {
							if (data.ResultNo == 0) {
								$layer.showMsg("取消选标成功！", function () {
									location.href = location.href;
								});
							} else {
								$layer.showMsg(data.ResultDescription);
							}
						});
					}
				});
			});
		});
		$("#additionalremarksBtn").click(function () {
			var remark = $("#additionalremarks .short").val();
			if (remark == "") {
				$layer.showMsg("请输入补充说明！");
				return false;
			}
			var html = '<form method="POST" id="additionalremarksform" action="/Tender/AppendRemark/' + params.OfferNo + '.html">';
			html += '<input name="remark" value="' + remark + '" />';
			html += '<input name="attachedFiles" value="' + $("#attachedFilesRemark").val() + '"/>';
			html += '</form>';
			$("body").append(html);
			$("#additionalremarksform").submit();
		});
		$("#quoteAmount").keyup(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$("#quoteAmount").blur(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$("#projectDuration").keyup(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$("#projectDuration").blur(function () {
			var val = parseInt(this.value);
			if (isNaN(val)) {
				this.value = "";
			} else {
				this.value = val;
			}
		});
		$("#submitBtn").click(function () {
			var quoteAmount = parseInt($("#quoteAmount").val());
			if (isNaN(quoteAmount) || quoteAmount <= 0) {
				$layer.showMsg("请输入正确的竞标报价！");
				return false;
			}
			var projectDuration = parseInt($("#projectDuration").val());
			if (isNaN(projectDuration) || projectDuration <= 0) {
				$layer.showMsg("请输入正确的工作周期！");
				return false;
			}
			$("form").submit();
		});
		$("#appendremark").click(function () {
			$layer.layer({ layerId: "additionalremarks", close_layer: ".close" });
			return false;
		});
	});
	if ($("#swfUploadButton").length > 0) {
		var swfu = $upload.create({
			upload_url: setting.urls.uploadUrl + "/Joy/Upload",
			button_placeholder_id: "swfUploadButton",
			file_types: "*.jpg;*.png;*.txt;*.doc;*.docx;*.ppt;*.pptx;*.xls;*.xlsx;*.pdf;*.rar;*.zip",
			file_types_description: "",
			file_size_limit: "2 MB",
			button_text: "",
			button_width: 200,
			button_height: 20,
			button_action: -100,
			button_cursor: -2,
			post_params: {
				'maxBytes': 1024 * 1024 * 5,
				'imageSize': false,
				'limited': true
			},
			upload_success_handler: function (file, serverData) {
				var data = $.parseJSON(serverData);
				if (data.ResultNo == 0) {

				} else {
					$layer.showMsg("上传失败：" + data.ResultDescription);
				}
			}
		});
	}
	if ($("#swfUploadButtonRemark").length > 0) {
		var swfu1 = $upload.create({
			upload_url: setting.urls.uploadUrl + "/Joy/Upload",
			button_placeholder_id: "swfUploadButtonRemark",
			file_types: "*.jpg;*.png;*.txt;*.doc;*.docx;*.ppt;*.pptx;*.xls;*.xlsx;*.pdf;",//*.rar;*.zip",
			file_types_description: "",
			file_size_limit: "2 MB",
			button_text: "",
			button_width: 200,
			button_height: 20,
			button_action: -100,
			button_cursor: -2,
			post_params: {
				'maxBytes': 1024 * 1024 * 5
			},
			upload_success_handler: function (file, serverData) {
				var data = $.parseJSON(serverData);
				if (data.ResultNo == 0) {

				} else {
					$layer.showMsg("上传失败：" + data.ResultDescription);
				}
			}
		});
	}
});