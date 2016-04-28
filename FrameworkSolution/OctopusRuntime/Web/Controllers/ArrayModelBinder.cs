using System.Web.Mvc;

namespace Octopus.Web.Controllers
{
	//数组绑定
	public class ArrayModelBinder : DefaultModelBinder
	{
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
			if(bindingContext.ModelType.IsArray) {
				var key = bindingContext.ModelName + "[]";
				var valueResult = bindingContext.ValueProvider.GetValue(key);
				if(valueResult != null && !string.IsNullOrEmpty(valueResult.AttemptedValue)) {
					bindingContext.ModelName = key;
				}
			}
			return base.BindModel(controllerContext, bindingContext);
		}
	}
}
