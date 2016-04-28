using System.Web.Mvc;
using System.Web.Security;
using HTB.DevFx;
using Octopus.Admin.Server;
using Octopus.Web.AdminBase;
using Octopus.Web.Controllers;

namespace Joy.Admin.LoginWeb.Controllers
{
	public class WikiController : Controller
	{
		private ActionResult RedirectTo(string returnUrl) {
			returnUrl = ControllerExtensions.GetReturnUrl(returnUrl, false);
			if(string.IsNullOrEmpty(returnUrl)) {
				var svr = ObjectService.GetObject<IPassportAuthentication>();
				returnUrl = svr.Setting.DefaultUrl;
			}
			return this.Redirect(returnUrl);
		}

		[HttpGet]
		public ActionResult Login(string returnUrl) {
			if(this.Request.IsAuthenticated) {
				return this.RedirectTo(returnUrl);
			}
			FormsAuthentication.SignOut();
			return this.View();
		}

		[HttpPost]
		public ActionResult Login(string userName, string password, string returnUrl) {
			if(this.Request.IsAuthenticated) {
				return this.RedirectTo(returnUrl);
			}
			if(string.IsNullOrEmpty(userName)) {
				this.ViewBag.Message = "请输入用户名";
				return this.View();
			}
			this.ViewBag.UserName = userName;
			if(string.IsNullOrEmpty(password)) {
				this.ViewBag.Message = "请输入密码";
				return this.View();
			}
			var svr = ObjectService.GetObject<IPassportServerService>();
			var result = svr.Login(this.HttpContext, userName, password, null);
			if(result.ResultNo != 0) {
				this.ViewBag.Message = "登录失败：" + result.ResultDescription;
				return this.View();
			}
			return this.RedirectTo(returnUrl);
		}

		[HttpGet]
		public ActionResult Logout() {
			FormsAuthentication.SignOut();
			return this.Redirect("~/");
		}
	}
}