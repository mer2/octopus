define("staticHuyu/huyu-supply-setcase", function (require, exports, module) {
    "require:nomunge,exports:nomunge,module:nomunge";
    var $ = require("jquery");
    var params = require("plugin-params")(module, "huyu");
    var upload = require("staticCommon/joy-upload");
    var swfu = upload.create({
        button_placeholder_id: "swfUploadButton"
			, file_types: "*.jpg;*.png;.gif;*.bmp;*.jpeg"
			, file_types_description: "图片文件（*.jpg;*.png;.gif;*.bmp;*.jpeg）"
			, file_size_limit: "1024 * 1024 * 2"
			, button_text: ""
			, button_width: 158
			, button_height: 46
			, post_params: {
			    'maxBytes': 1024 * 1024 * 2
			, 'imageSize': true
			, 'minWidth': 1
			, 'minHeight': 1
			}
			, file_dialog_complete_handler: function (fileCount, fileQueued) {
			    if (fileCount > 0) {
			        this.startUpload();
			    }
			}, upload_progress_handler: function (file, bytesLoaded) {
			    FileUploadStart(file); //自定义操作上传中
			    var v = "uploadfile_" + file.id;
			    var percent = Math.ceil((bytesLoaded / file.size) * 100);
			    UploadAnimate(v, percent);
			}
			, upload_success_handler: function (file, serverData) {
			    var data = $.parseJSON(serverData);
			    FileUploadSuccess(file, data);
			}
    });
    $(".uploads").on("click", ".del", null, function () {
        var fileName = $(this).attr("lang");
        DeleteImage(fileName);
        return false;
    });

    function FileUploadStart(file) {
        if ($(".uploads .upload-box").length >= 5) {
            $.joy.showMsg("最多只能上传5张图片");
        } else {
            //$("#submitBtn").attr("disabled", true);
            var fileName = "uploadfile_" + file.id;
            if ($("#" + fileName).html() == null) {
                var startHtml = '<li id="' + fileName + '">';
                startHtml += '<div class="clearfix">';
                startHtml += '<label class="pic-name">' + file.name + '</label>';
                startHtml += '<div class="info">上传中，<span>1%</span></div>';
                startHtml += '</div>';
                startHtml += '<div class="progress-box">';
                startHtml += '<div class="progress-bg"><span style="width: 1%" class="progress"></span>';
                startHtml += '</div>';
                startHtml += '</div>';
                startHtml += '</li>';
                $(".upload-loading ul").append(startHtml);
            }
            if ($("." + fileName).html() == null) {
                var sussesHtml = "";
                sussesHtml += '<div lang="' + file.id + '" class="upload-box ' + fileName + '" style=" display:none;">';
                sussesHtml += '<a class="del" lang="' + fileName + '" href="javascript:void(0)">删除</a>';
                sussesHtml += '<div class="pic f-l">';
                sussesHtml += '<img alt="" src=""/>';
                sussesHtml += '</div>';
                sussesHtml += '<div class="info f-r">';
                sussesHtml += '<div class="description">';
                sussesHtml += '图片描述：<br>';
                sussesHtml += '<input type="text" /><input type="hidden" />';
                sussesHtml += '</div>';
                sussesHtml += '</div>';
                sussesHtml += '</div>';
                $(".uploads").append(sussesHtml);
            }
        }
    }

    function UploadAnimate(id, percent) {
        if (percent < 100) {
            percent += 1;
            $("#" + id).find(".info span").html(percent + "%");
            $("#" + id).find(".progress-bg span").attr("style", "width: " + percent + "%");
        }
    }

    function FileUploadSuccess(file, data) {
        //$("#submitBtn").attr("disabled", false);
        var fileName = "uploadfile_" + file.id;
        if (data.ResultNo == 0) {
            var obj = data.ResultAttachObject;
            $("#" + fileName).remove();
            var $file = $("." + fileName);
            $file.find("img").attr("src", obj.FileUrl);
            $file.find("input:eq(1)").val(obj.DisplayFileName);
            $file.show();
        } else {
            $.joy.showMsg("上传失败 " + data.ResultDescription, function () {
                DeleteImage(fileName);
            });
        }
    }

    function DeleteImage(fileName) {
        $("." + fileName).remove();
        $("#" + fileName).remove();
    }
    $("#form").validate({
        rules: {
        },
        messages: {
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
        submitHandler: function (form) {
            var flag = true;
            var pics = [];
            $(".uploads div.upload-box").each(function (idx, itm) {
                var _url = $(itm).find("img").attr("src");
                var _name = $(itm).find("input:eq(0)").val();
                var _fileName = $(itm).find("input:eq(1)").val();
                if (_name == "") {
                    $.joy.showMsg("请填写图片描述");
                    flag = false;
                    $("html,body").animate({ scrollTop: $(itm).find("input").offset().top });
                    return false;
                }
                pics.push({ Name: _name, Url: _url, FileName: _fileName });
            });
            var res = Translate(pics);
            $("#Pictures").val(res);
            if (flag) {
                form.submit();
            } else {
                return false;
            }
        }
    });
    $("#pagebtns").on("click", "a", function () {
        var _idx = $(this).attr("lang");
        if (!isNaN(_idx)) {
            LoadSuccessCase(_idx);
        }
    });
    $("#pagebtns").closest("div.case-list").find("div.case-cont ul").on("click", "input:radio", function () {
        var _id = $(this).val();
        $("#SuccessCaseID").val(_id);
    });
    //  成功案例
    function LoadSuccessCase(idx) {
        $.ajax({
            url: "/Supply/GetSuccessCase.html",
            dataType: "json",
            type: "post",
            success: function (res) {
                if (res.ResultNo == 0) {
                    var total = res.Total;
                    if (total == 0) {
                        $("#pagebtns").closest("div.case-list").find("div.case-cont").addClass("nothing").html("暂无实例");
                        return;
                    }
                    var lst = res.ResultAttachObjectEx;
                    var pagebtns = $("#pagebtns a");
                    pagebtns.remove();
                    if (idx != 0) {
                        pagebtns.append('<a href="javascript:;" title="上一页" lang="' + (idx < 5 ? 0 : idx - 5) + '">上一页</a>');
                    }
                    if (total > idx + 5) {
                        pagebtns.append('<a href="javascript:;" title="下一页" lang="' + (idx + 5) + '>下一页</a>');
                    }
                    var ctner = $("#pagebtns").closest("div.case-list").find("div.case-cont ul");
                    ctner.children().remove();
                    $.each(lst, function (i, itn) {
                        var _id = itn.Category + ":" + itn.OfferNo;
                        ctner.append('<li class="clearfix">' +
                                                '<div class="date f-r">' + (itn.IsHire ? '<a href="javascript:;" class="state-icon16 deal-state16-d"><span>这是雇佣信息，别人将无法查看<i></i></span></a>' : '') + '<span>' + itn.CreateTime + '</span></div>' +
												'<h4><a href="' + itn.Url + '" target="_blank" title="' + itn.Title + '">' + itn.Title + '</a><span class="state">' + itn.Status + '</span></h4>' +
                                                '<div class="money">￥' + itn.Commission + '</div>' +
                                                '<div class="info clearfix">' +
                                                    '<div class="customer f-l">甲方：<a href="/User/Index/' + itn.UserID + '.html" target="_blank" title="' + itn.UserName + '">' + itn.UserName + '</a></div>' +
                                                    '<div class="num f-l"><strong class="commentscore"><em class="score' + itn.DefaultScore + '"></em>' + itn.DefaultText + '</strong><span>服务态度：<i>' + itn.Score1 + '</i>分</span><span>工作效率：<i>' + itn.Score2 + '</i>分</span><span>完成质量：<i>' + itn.Score3 + '</i>分</span></div>' +
                                                '</div>' +
                                                '<input type="radio" name="rdoSuccessCaseID" class="radiobox" value="' + _id + '" ' + ($("#SuccessCaseID").val() == _id ? 'checked="checked"' : '') + '/>' +
                                            '</li>');
                    });
                } else {
                    $.joy.showMsg("加载失败");
                }
            },
            error: function () {
                $.joy.showMsg("加载失败");
            }
        });
    }
    LoadSuccessCase(0);

    function Translate(data) {
        var _text = "[";
        $.each(data, function (i, n) {
            _text += "{";
            _text += "\"Name\":\"" + n.Name + "\",";
            _text += "\"Url\":\"" + n.Url + "\",";
            _text += "\"FileName\":\"" + n.FileName + "\"";
            _text += "},";
        });
        if (_text.length > 1) {
            _text = _text.substring(0, _text.length - 1);
        }
        return _text + "]";
    }
});