//文件上传封装
define("staticCommon/joy-upload", function (require, exports, module) {
	"require:nomunge,exports:nomunge,module:nomunge";
	var $ = require("jquery");
	var globalSettings = require("staticCommon/joy-config");
	var urls = $.extend({ uploadUrl: 'http://upload.joyyang.com' }, globalSettings.urls);
	var swfupload = require("swfupload");
	var layer = require("layer");
	require("staticCommon/joy-utils");
	var cookieName = globalSettings.cookieName;
	var thisModule = {
		options: null,
		showMessage: function (msg) {
			var options = thisModule.options;
			if (options && typeof (options.showMessage) == 'function') {
				options.showMessage(msg);
			} else {
				layer.showMsg(msg, null, 'fail');
			}
		},
		attachedFiles: function(className) {
			var containers = $(className);
			if (containers.length <= 0) {
				return;
			}
			var values = new Array();
			containers.each(function () {
				var $this = $(this);
				var value = $this.attr("lang");
				if (value) {
					values.push(value);
				}
			});
			if (values.length <= 0) {
				return;
			}
			var targetValues = values.join(",");
			$.ajax({
				url: urls.uploadUrl + "/Joy/TargetFiles",
				crossDomain: true,
				cache: false,
				dataType: 'jsonp',
				data: { values: targetValues },
				success: function (result) {
					if (result.ResultNo != 0) {
						return;
					}
					var items = result.ResultAttachObject;
					if (!items || items.length <= 0) {
						return;
					}
					for (var i = 0; i < items.length; i++) {
						var item = items[i];
						//模板：{0}文件下载地址，{1}文件类型，{2}文件名，{3}文件编码
						var htmlTemplate = '<li><a href="{0}" target="_blank" title="下载" class="f-r">下载</a><span class="{1}"></span><em><a href="{0}" target="_blank" title="{2}">{2}</a></em></li>';
						$(className + '[lang="' + item.Target + ':' + item.TargetValue + '"]').append(htmlTemplate.format(thisModule.getFileUrl(item.FileUrl), item.FileExtension, item.DisplayFileName, item.FileNo));
					}
				}
			});
		},
		getFileUrl: function(url) {
			if (thisModule.options.isAdmin) {
				url = url.replace("http://img." + globalSettings.mainDomain + "/u/", "http://img.admin." + globalSettings.mainDomain + "/ua/");
			}
			return url;
		},
		create: function (options) {
			if (options != null) {
				thisModule.upload_success_handler = options.upload_success_handler;
			}
			thisModule.options = options = $.extend(true, {
				isAdmin: false,//是否是后台页面
				cookie_fix: true,//是否绕过Firefox无法传递Cookie的BUG
				cookie_name: cookieName,
				cookie_url: "/Passport/SsoTicket.ashx",
				upload_url: urls.uploadUrl + "/Joy/Upload",
				flash_url: urls.resourceUrl + "/libs/swfupload/v2.2/flash/swfupload.swf",
				file_size_limit: "2 MB",
				button_window_mode: "transparent",
				preserve_relative_urls: true,
				prevent_swf_caching: false,
				file_dialog_complete_handler: function (fileCount, fileQueued) {
					if (fileQueued > 0) {
						this.setButtonDisabled(true);
						//绕过Firefox无法传递Cookie的BUG
						var ua = navigator.userAgent.toLowerCase();
						if (this.settings.cookie_fix && ua.indexOf("firefox") != -1) {//Firefox浏览器
							//为了保证每次的正确请求，不管原先有没有Cookie都需要每次获取Cookie // && !this.settings.post_params[this.settings.cookie_name]
							//把原先存储的值删除
							this.settings.post_params[this.settings.cookie_name] = null;
							this.setPostParams(this.settings.post_params);
							var up = this;
							$.ajax({
								type: "POST",
								url: this.settings.cookie_url,
								success: function(data, status, xhr) {
									var cookieValue = xhr.getResponseHeader(up.settings.cookie_name);
									if (cookieValue) {
										var ck = {};
										ck[up.settings.cookie_name] = cookieValue;
										var params = $.extend(up.settings.post_params, ck);
										up.setPostParams(params);
									}
								},
								complete: function() {
									up.startUpload();
								}
							});
						} else {//其他浏览器
							this.startUpload();
						}
					}
				},
				upload_complete_handler: function () {
					/*  I want the next upload to continue automatically so I'll call startUpload here */
					if (this.getStats().files_queued > 0) {
						this.startUpload();
					} else {
						this.setButtonDisabled(false);
					}
				},
				upload_error_handler: function (file, errNo, message) {
					var msg = "上传失败，请重试。文件名：" + file.name + "，错误信息：" + errNo + "/" + message;
					thisModule.showMessage(msg);
					this.setButtonDisabled(false);
				},
				file_queue_error_handler: function (file, errNo, message) {
					var msg = "您选择的文件 " + file.name + " 不符合要求，请检查文件类型和大小后重选，谢谢。错误信息：" + errNo + "/" + message;
					thisModule.showMessage(msg);
					this.setButtonDisabled(false);
				}
			}, options);
			var upload_success_handler0 = options.upload_success_handler;
			options.upload_success_handler = function (file, serverData) {
				if (upload_success_handler0 != null) {
					upload_success_handler0.apply(this, [file, serverData]);
				} else if (data.ResultNo != 0) {
					thisModule.showMessage("上传失败：" + data.ResultDescription);
				}
				this.setButtonDisabled(false);
			};
			var holder = $("#" + options.button_placeholder_id);
			var lang = holder.attr("lang");
			if (lang) {
				var ids = lang.split(':');//已上传文件隐藏框ID:已上传文件显示容器:类别:类别值
				if (ids.length >= 2) {
					var input = $(ids[0]);
					var container = $(ids[1]);
					var values = input.val();
					var files = {};
					if (values) { //获取当前已上传的文件列表
						var vals = values.split(',');
						for (var i = 0; i < vals.length; i++) {
							files[vals[i]] = true;
						}
						$.ajax({
							url: urls.uploadUrl + "/Joy/Files",
							crossDomain: true,
							cache: false,
							dataType: 'jsonp',
							data: { values: values },
							success: function (result) {
								if (result.ResultNo != 0) {
									return;
								}
								var items = result.ResultAttachObject;
								if (!items || items.length <= 0) {
									return;
								}
								for (var j = 0; j < items.length; j++) {
									var item = items[j];
									var htmlTemplate = '<li><a href="javascript:;" title="删除" class="f-r delete" lang="{3}">删除</a><span class="{1}"></span><em><a href="{0}" target="_blank" title="{2}">{2}</a></em></li>';
									container.append(htmlTemplate.format(thisModule.getFileUrl(item.FileUrl), item.FileExtension, item.DisplayFileName, item.FileNo));
								}
							}
						});
					}
					if (ids.length >= 4) {//已上传文件隐藏框ID:已上传文件显示容器:类别:类别值
						$.ajax({
							url: urls.uploadUrl + "/Joy/TargetFiles",
							crossDomain: true,
							cache: false,
							dataType: 'jsonp',
							data: { values: ids[2] + ":" + ids[3] },
							success: function (result) {
								if (result.ResultNo != 0) {
									return;
								}
								var items = result.ResultAttachObject;
								if (!items || items.length <= 0) {
									return;
								}
								for (var k = 0; k < items.length; k++) {
									var item = items[k];
									if (!files[item.FileNo]) {
										files[item.FileNo] = true;
										//模板：{0}文件下载地址，{1}文件类型，{2}文件名，{3}文件编码
										var htmlTemplate = '<li><a href="javascript:;" title="删除" class="f-r delete" lang="{3}">删除</a><span class="{1}"></span><em><a href="{0}" target="_blank" title="{2}">{2}</a></em></li>';
										container.append(htmlTemplate.format(thisModule.getFileUrl(item.FileUrl), item.FileExtension, item.DisplayFileName, item.FileNo));
									}
								}
							}
						});
					}
					function setFileNo() {
						var keys = new Array();
						$.map(files, function (value, key) {
							if (value) {
								keys.push(key);
							}
						});
						input.val(keys.join(','));
					}
					container.on("click", "a.delete", function () {
						var $this = $(this);
						var fileNo = $this.attr("lang");
						files[fileNo] = false;
						setFileNo();
						$this.parents("li").first().remove();
						return false;
					});
					var upload_success_handler00 = options.upload_success_handler;
					options.upload_success_handler = function (file, serverData) {
						var data = $.parseJSON(serverData);
						//把ID加到已上传字段里
						if (data.ResultNo == 0) {
							var item = data.ResultAttachObject;//fi为上传成功后返回的文件信息
							files[item.FileNo] = true;
							setFileNo();
							var htmlTemplate = '<li><a href="javascript:;" title="删除" class="f-r delete" lang="{3}">删除</a><span class="{1}"></span><em><a href="{0}" target="_blank" title="{2}">{2}</a></em></li>';
							container.append(htmlTemplate.format(thisModule.getFileUrl(item.FileUrl), item.FileExtension, item.DisplayFileName, item.FileNo));
						}
						if (upload_success_handler00 != null) {
							upload_success_handler00.apply(this, [file, serverData]);
						}
					};
				}
			}
			return new swfupload(options);
		}
	};
	$.joy = $.extend($.joy, {});
	$.joy.upload = $.extend($.joy.upload, thisModule);
	return thisModule;
});