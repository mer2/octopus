using System;
using System.Web.Mvc;
using HTB.DevFx;
using HTB.DevFx.Utils;

namespace Octopus.Web.AdminBase.Mvc.Controllers
{
	public class AccountController : AdminControllerBase
	{
		[HttpGet]
		public ActionResult LogOn(string returnUrl) {
			returnUrl = WebHelper.UriCombine(this.Request.Url.GetLeftPart(UriPartial.Path), "~/Account/SignIn?returnUrl=" + this.Server.UrlEncode(returnUrl), true);
			var authService = ObjectService.GetObject<IAuthenticationService>();
			var url = authService.GetAuthenticationUrl(this.HttpContext, returnUrl);
			return this.Redirect(url);
		}

		[HttpGet]
		public ActionResult SignIn(string ticket) {
			var authService = ObjectService.GetObject<IAuthenticationService>();
			var result = authService.Validate(this.HttpContext, ticket);
			if(result.ResultNo == 0) {
				return this.Redirect(result.ResultAttachObjectEx);
			}
			return this.Content(result.ToString());
		}
	}
}
