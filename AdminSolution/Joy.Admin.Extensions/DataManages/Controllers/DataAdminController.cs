using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using HTB.DevFx;
using HTB.DevFx.Data;
using Joy.Admin.Extensions.DataManages.Data.Server;
using Octopus.Web;
[assembly: EmbeddedPath("Joy.Admin.Extensions.DataManages.Views.", "~/Views/DataAdmin/")]

namespace Joy.Admin.Extensions.DataManages.Controllers
{
    public class DataAdminController : Controller
    {
		private static IDictionary GetDictionary(NameValueCollection nvc) {
			var data = new Hashtable();
			foreach (var key in nvc.AllKeys) {
				var value = nvc[key];
				if (!string.IsNullOrEmpty(value)) {
					data.Add(key, value);
				}
			}
			return data;
		} 

        public ActionResult List(string id, bool query = false, int startIndex = 0, int length = 30) {
			if (string.IsNullOrEmpty(id)) {
				return this.Content("Error Item Name");
			}
            var svr = ObjectService.GetObject<IDataManageService>();
	        var item = svr.GetDataItem(id);
			if (item == null) {
				return this.Content("Error Item");
			}
			if (query || this.Request.HttpMethod == "POST") {
				var queryValues = GetDictionary(this.Request.Form);
				this.ViewBag.ItemColumns = item.GetListColumns();
				this.ViewBag.QueryValues = queryValues;
				this.ViewBag.ItemList = item.GetList(queryValues, startIndex, length);
			}
	        this.ViewBag.StartIndex = startIndex;
			this.ViewBag.PageSize = length;
	        this.ViewBag.DataItem = item;
	        this.ViewBag.Title = item.Title;
			this.ViewBag.QueryColumns = item.GetQueryColumns();
            return this.View();
        }

        public ActionResult Edit(string name, string Id) {
            var svr = ObjectService.GetObject<IManageServer>();
            svr.TableName = name;
            var options = new Dictionary<string, string>();
            options.Add(svr.KeyField, Id);
            var dt = svr.GetDataList(options, null);

            this.ViewBag.EditColumns = svr.GetEditColumns();
            this.ViewBag.Data = dt;

            return View("~/Plugin/Joy.Admin.Extensions.dll/Joy.Admin.Extensions/DataManages.Views.Edit.cshtml");
        }

        [HttpPost]
        public ActionResult Edit(string name) {
            var svr = ObjectService.GetObject<IManageServer>();
            svr.TableName = name;
            var columns = svr.GetEditColumns();

            var options = new Dictionary<string, string>();
            foreach(var column in columns) {
                var field = column.Field;
                options.Add(field, Request.Form[field]);
            }

            svr.UpdateData(options);
            return this.RedirectToAction("List");
        }

        public ActionResult Add(string name) {
            var svr = ObjectService.GetObject<IManageServer>();
            svr.TableName = name;
            var columns = svr.GetAddColumns();

            this.ViewBag.Columns = columns;
            return View("~/Plugin/Joy.Admin.Extensions.dll/Joy.Admin.Extensions/DataManages.Views.Add.cshtml");
        }

        [HttpPost]
        public ActionResult Add(string name, string id = null) {
            var svr = ObjectService.GetObject<IManageServer>();
            svr.TableName = name;
            var columns = svr.GetAddColumns();

            var options = new Dictionary<string, string>();
            foreach(var column in columns) {
                var field = column.Field;
                options.Add(field, Request.Form[field]);
            }

            svr.InsertData(options);

            return this.RedirectToAction("List");
        }
    }
}