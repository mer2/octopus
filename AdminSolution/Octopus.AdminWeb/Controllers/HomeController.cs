using System.Web.Mvc;
using Newtonsoft.Json;
using Octopus.SecurityPermissions;
using Octopus.Web.AdminBase;
using Octopus.Web.Controllers;

namespace Octopus.Admin.Web.Controllers
{
	[Authorize]
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
	}
}
