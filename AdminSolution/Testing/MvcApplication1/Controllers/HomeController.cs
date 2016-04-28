using System.Web.Mvc;

namespace MvcApplication1.Controllers
{
	[Authorize]
    public class HomeController : Controller
	{
		public ActionResult Index() {
			return this.View();
		}
	}
}
