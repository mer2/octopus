using System.Text.RegularExpressions;
using System.Web.Mvc;
using HTB.DevFx.Core;
using Newtonsoft.Json;
using Octopus.Admin.Crowd;
using Octopus.SecurityPermissions;
using Octopus.Web.AdminBase;
using Octopus.Web.Controllers;

namespace Octopus.Admin.Web.Controllers
{
	//[Authorize]
	public class HomeController : Controller
	{
		[HttpGet]
		public ActionResult Index(string url, string title) {
			var list = PermissionClientService.GetViewablePermissionResources("Admin");
			this.ViewBag.Permissions = JsonConvert.SerializeObject(list);
			this.ViewBag.DefaultUrl = ControllerExtensions.GetReturnUrl(url, false);
			this.ViewBag.DefaultTitle = title;
			this.ViewBag.DisplayName = (this.User.Identity as PassportIdentity).DisplayName;
			return this.View();
		}

		[HttpGet]
		public ActionResult Welcome() {
			return this.View();
		}

		[HttpGet]
		public ActionResult UpdatePassword() {
			return this.View();
		}

		internal static Regex StrongPassword = new Regex("^(?=.{6,})(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*\\W).*$");
		[HttpPost]
		public ActionResult UpdatePassword(string oldPassword, string newPassword) {
			if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword)) {
				this.ViewBag.Result = AOPResult.Failed("参数错误");
				return this.View();
			}
			if (!StrongPassword.IsMatch(newPassword)) {
				this.ViewBag.Result = AOPResult.Failed("密码必须同时包含数字、大小写字母、特殊字符");
				return this.View();
			}
			var cwd = CrowdService.Current;
			var userName = this.User.Identity.Name;
			var authResult = cwd.Authenticate(userName, oldPassword);
			if (authResult.ResultNo != 0) {
				this.ViewBag.Result = authResult;
				return this.View();
			}
			var updateResult = cwd.ChangePassword(this.User.Identity.Name, newPassword);
			this.ViewBag.Result = updateResult;
			return this.View();
		}
	}
}
