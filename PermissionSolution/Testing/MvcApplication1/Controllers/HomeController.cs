using System.Web.Mvc;
using Octopus.SecurityPermissions;
using Octopus.SecurityPermissions.Web;

namespace MvcApplication1.Controllers
{
	[PermissionAuthorize("Company.UserCompany", AppNo = "Company")]
	public class HomeController : Controller
	{
		[PermissionAuthorize("GetUsers", AppNo = "Joy")]
		public ActionResult Index() {
			//...
			if(PermissionClientService.Authorize("GetUsers")) {
				
			}

			var permissions = PermissionClientService.GetUserPermissions("Company.UserCompany");
			if(permissions != null && permissions.Length > 0) {
				var campanyId = permissions[0].PermissionValue;
			}
			return View();
		}

		public ActionResult Error() {
			return this.View();
		}

		protected string GetUserCompany(string userName) {
			string company = null;
			var permissions = PermissionClientService.GetUserPermissions("Company.UserCompany", userName);
			if(permissions != null && permissions.Length > 0) {
				company = permissions[0].PermissionValue;
			}
			if(string.IsNullOrEmpty(company)) {
				throw new AuthorizationException();
			}
			return company;
		}
	}
}
