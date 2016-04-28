define("staticHuyu/huyu-pay-paybanks", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	require("formValidate");
	var $ = require("jquery");
	var jStorage = require("jstorage");
	var layer = require("layer");

	$(function () {
		$(".banks-box .category-select li").click(function () {//选标签
			var $this = $(this);
			var cn = $this.attr("lang");
			if (!cn || $this.hasClass("cur")) {
				return;
			}
			if ($this.attr("data-nosubmit")) {
				$("#theForm input[type=submit]").hide();
			} else {
				$("#theForm input[type=submit]").show();
			}
			$(".banks-box .contbox").hide();
			$(".banks-box .category-select li").removeClass("cur");
			$this.addClass("cur");
			$(".banks-box #" + cn).show();
			//清除已选择的银行
			$(".banks-box input[name=bankNo]:checked").removeAttr("checked");
			//恢复上次选择的银行
			$(".banks-box #" + cn + " li.cur").click();
		});
		$(".banks-box #category-onlinePay .bank-select li").click(function () {//选银行
			var $this = $(this);
			var bn = $this.attr("lang").toLowerCase();
			var ci = $this.find("input");
			ci.attr("checked", "checked");
			$(".banks-box .bank-info-list #table-bank-" + bn).show();
			if ($this.hasClass("cur")) {
				return;
			}
			$(".banks-box .bank-select li").removeClass("cur");
			$this.addClass("cur");
			$(".banks-box .bank-info-list .bank-table").hide();
		});
		$(".banks-box #category-thirdPay .thirdways-select li").click(function () {//选平台
			var $this = $(this);
			var bn = $this.attr("lang").toLowerCase();
			var ci = $("#bank-" + bn);
			ci.attr("checked", "checked");
			if ($this.hasClass("cur")) {
				return;
			}
			$(".banks-box .thirdways-select li").removeClass("cur");
			$this.addClass("cur");
		});

		var $useBalance = $("#theForm input[name=useBalance]");
		var leftAmount = parseFloat($useBalance.attr("lang"));
		$useBalance.click(function () {//使用余额支付点击处理
			var $this = $(this);
			if ($this.is(":checked")) {
				$(".totalAmount").hide();
				$(".leftAmount").show();
				if (leftAmount <= 0) {
					$(".pay-cont").hide();
				}
			} else {
				$(".totalAmount").show();
				$(".leftAmount").hide();
				if (leftAmount <= 0) {
					$(".pay-cont").show();
				}
			}
		});
		if (leftAmount <= 0) {
			$useBalance.removeAttr("checked").click();
		}

		$.validator.addMethod("isNewAmount", function (value, element) {
			var amount = /^(([1-9]\d{0,4})|0)(\.\d{2})?$/;
			return this.optional(element) || (parseFloat(value) > 0 && amount.test(value));
		}, "请正确填写金额");

		var form = $(".banks-box").parents("form").first();
		form.validate({
			event: "blur",
			onkeyup: false,
			rules: {
				amount: {
					required: true,
					isNewAmount: true,
					maxlength: 9
				},
				userName: {
					required: true,
					remote: {
						url: "/NewPay/UserExists",
						type: "POST",
						dataType: "json"
					}
				}

			},
			messages: {
				amount: {
					required: "请输入金额",
					isNewAmount: "请输入正确的金额",
					maxlength: "请输入正确的金额"
				},
				userName: {
					defaultMessage: "请填写收款人",
					required: "请填写收款人",
					remote: "收款人不存在"
				}
			},
			errorElement: "span",
			errorPlacement: function (error, element) {
				element.siblings("p").html(error);
			},
			success: "valid",
			submitHandler: function (frm) {
				var f = $(frm);
				var bank = f.find("input[name=bankNo]:checked");
				if (bank.length != 1) {//未选择银行
					if ($useBalance.length && leftAmount <= 0 && $useBalance.is(":checked")) {
						//使用余额支付
					} else {//不是使用余额支付
						layer.showMsg("请选择银行或支付平台", "fail");
						return;
					}
				}
				//保存选择的银行和支付平台
				var bankNo = bank.val();
				if (bankNo) {
					jStorage.set("cPayCurrentBankNo", bankNo);
				}
				frm.submit();
				layer.layer({ layerId: "pay-action" });
				//ajax post，弊端太多，不用此方式了
				/*$.ajax({
					type: "POST",
					url: f.attr("action"),
					data: f.serialize(),
					async: false,//设置为同步，否则新弹出的窗口会被拦截
					success: function(r) {
						if (r.ResultNo == 302) { //重定向
							window.location.href = r.ResultAttachObject;
						} else if (r.ResultNo != 0) {
							layer.showMsg(r.ResultDescription);
						} else {
							$(r.ResultAttachObject).appendTo("body").submit().remove();
							//弹提示层
							layer.layer({ layerId: "pay-action" });
						}
					}
				});*/
			}
		});
		//提示层移到body下面
		$("#pay-action").appendTo("body");
		//恢复上一次的提交选项
		var currentBank = jStorage.get("cPayCurrentBankNo");
		if (currentBank) {//有
			var cb = $(".banks-box li[lang=" + currentBank + "]");
			cb.addClass("cur");
			var cid = cb.parents(".contbox").first().attr("id");
			$(".banks-box .category-select li[lang=" + cid + "]").removeClass("cur").click();
		}
		//未绑定手机提示
		var tips = $(".valid-tips");
		if (!tips.attr("lang")) {
			tips.slideDown().find(".close").click(function () {
				tips.slideUp();
			});
		}
	});
});