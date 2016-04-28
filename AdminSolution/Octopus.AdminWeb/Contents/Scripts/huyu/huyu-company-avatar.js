define("staticHuyu/huyu-company-avatar", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var settings = require("staticCommon/joy-config");
    var swfobject = require("swfobject");
    var layer = require("layer");
    $(function () {
        $.joy = $.extend({}, $.joy);
        $.joy.avatar = {
            callback: function (status) {//定义回调函数
                status += '';
                switch (status) {
                    case '1':
                        layer.showMsg("上传成功", function () {
                            window.location.href = "/Company/Description";
                        }, "success");
                        break;
                    case '2':
                        break;
                    case '-1'://用户点击了取消
                        window.location.href = "/Company/Description";
                        break;
                    case '-2':
                        layer.showMsg("上传失败，请重试", "fail");
                        break;
                }
            },
            lang: function () {//定义文字资源
                return {
                    CX0189: "图片会自动生成三种尺寸，请注意中小尺寸是否清晰"//提示文字
                };
            }
        };
        swfobject.embedSWF(settings.urls.resourceUrl + "/libs/faustcplus2/FaustCplus.swf",
			"flashUploadAvatar",//Flash所在的容器ID，如<div class="updateAvatar"><div id="flashUploadAvatar"></div></div>
			"940", "500", "9.0.0", "expressInstall.swf", {
			    "jsfunc": "$.joy.avatar.callback",//回调函数
			    "uploadSrc": false,//是否上传原文件
			    "showBrow": true,//是否显示“本地照片”按钮
			    "showCame": true,//是否显示“拍照上传”按钮
			    "showColorAdj": true,//是否显示“图像调整”面板
			    "uploadUrl": "/Company/UploadAvatar",//文件上传地址
			    "pSize": "300|300|276|207|200|150|80|60",//头像格式（括号里的为可选）：选择框宽|选择框高|大头像宽|大头像高（|中头像宽|中头像高|小头像宽|小头像高）
			    "jslang": "$.joy.avatar.lang"//定义文字资源回调函数
			}, {//flash选项
			    menu: "false",
			    scale: "noScale",
			    allowFullscreen: "false",
			    allowScriptAccess: "always",
			    wmode: "transparent",
			    bgcolor: "#FFFFFF"
			}, {//其他属性
			    id: "FaustCplus"//Flash对象的ID
			}
		);
    });
});