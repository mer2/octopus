using System.Web.Mvc;
using HTB.DevFx;
using Octopus.Performance;

namespace Octopus.Web.Controllers
{
	public class OctopusController : Controller
	{
		public ActionResult State() {
			var svr = ObjectService.GetObject<PerformanceService>();
			var result = svr.GetPerformanceResult();
			return this.Content(string.Format("|{0}|{1}|", result.ResultNo, result.ResultDescription));
		}
	}
}
