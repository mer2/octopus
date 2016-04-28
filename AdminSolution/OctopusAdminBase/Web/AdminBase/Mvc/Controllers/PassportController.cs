using System;
using System.Web.Mvc;
using HTB.DevFx;
using HTB.DevFx.Utils;

namespace Octopus.Web.AdminBase.Mvc.Controllers
{
	public class PassportController : Controller
	{
		[HttpGet]
		public ActionResult Login(string returnUrl) {
			returnUrl = WebHelper.GetReturnUrl(returnUrl:returnUrl, uriKind:UriKind.Absolute);
			var svr = ObjectService.GetObject<IPassportAuthentication>();
			var loginUrl = svr.Setting.LoginUrl;
			return this.Redirect(string.Format(loginUrl, this.Server.UrlEncode(returnUrl)));
		}

		[HttpGet]
		public ActionResult Logout() {
			System.Web.Security.FormsAuthentication.SignOut();
			return this.Login("~/");
		}

		[HttpGet]
		public ActionResult Error(string id, string returnUrl) {
			return this.Content("<h1>Access Denied!</h1>");
		}
	}
}