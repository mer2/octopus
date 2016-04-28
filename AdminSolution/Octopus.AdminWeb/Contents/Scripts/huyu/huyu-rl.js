define("staticHuyu/huyu-rl", function (require, exports, module) {
    var $ = require("jquery");
    $(".sec-submenu .claim .btns").click(function () {
        $.joy.layer({ layerId: "claim-layer", close_layer: "#resetBtn" });
    });

    $("#ContactPosition").bind("focus", function () { $(this).next().html('例如：创始人，部门经理等。'); });
    $("#Mobile").bind("focus", function () { $(this).next().html('请填写真实的手机号，方便客服人员联系'); });
    $("#claimForm").validate({
        rules: {
            ContactName: {
                required: true,
                rangelength: [2, 20]
            },
            ContactPosition: {
                required: true,
                rangelength: [2, 30]
            },
            Mobile: {
                required: true,
                isMobile: true
            }
        },
        messages: {
            ContactName: {
                required: "请输入您的名字",
                rangelength: "请保持在2-20个字符内"
            },
            ContactPosition: {
                required: "请输入您的职位",
                rangelength: "请保持在2-30个字符内"
            },
            Mobile: {
                required: "请输入您的联系方式",
                isMobile: "请正确输入您的联系方式"
            }
        },
        errorElement: "span",
        errorPlacement: function (error, element) {
            if (element[0].id == "code") {
                element.next().next().next().html(error);
            } else {
                element.next().html(error);
            } 		//if(element.is(":checkbox")){alert("请阅读并同意《骄阳网用户服务协议》");}
        },
        success: "valid",
        //focusCleanup:true,
        submitHandler: function () {
            if ($("#IdentityData").val() == "") {
                $.joy.showMsg("请上传证件名片");
                return false;
            }
            $.ajax({
                url: "/Company/Claim.html",
                dataType: "json",
                cache: false,
                type: "post",
                data: {
                    ContactName: $("#ContactName").val(),
                    ContactPosition: $("#ContactPosition").val(),
                    Mobile: $("#Mobile").val(),
                    Email: $("#Email").val(),
                    IdentityData: $("#IdentityData").val(),
                    IdentityDescription: $("#IdentityDescription").val(),
                    companyId: $("#CompanyID").val()
                },
                success: function (res) {
                    switch (res) {
                        case 0:
                            alert("提交成功");
                            break;
                        case 1:
                            break;
                        case 2:
                            alert("请先登录");
                            break;
                        case 3:
                            alert("您的认领正在审核中");
                            break;
                        case 4:
                            alert("企业已经被认领");
                            break;
                        case 5:
                            alert("您已经创建了企业,不能再认领");
                            break;
                        default:
                            location.href = location.hre;
                            break;
                    }
                    $.joy.closeLayer("claim-layer");
                    return false;
                }
            });
            return false;
        }
    });
});