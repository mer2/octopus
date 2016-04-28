using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace Octopus.Esb.Server
{
	internal class FormValueProviderFactory : ValueProviderFactory
	{
		public override IValueProvider GetValueProvider(RequestContext requestContext) {
			if(requestContext == null) {
				throw new ArgumentNullException("requestContext");
			}
			var forms = new Dictionary<string, object>();
			foreach(string key in requestContext.HttpContext.Request.Form.Keys) {
				forms.Add(key, requestContext.HttpContext.Request.Form[key]);
			}
			var files = requestContext.HttpContext.Request.Files;
			if (files.Count > 0) {
				foreach(string key in files.Keys) {
					forms.Add(key, files[key]);
				}
				forms.Add(KeyOfAllFiles, files);
			}
			if (forms.Count > 0) {
				if (forms.ContainsKey(KeyOfAllValues)) {
					forms.Add(KeyOfAllValues, forms);
				}
			}
			return forms.Count > 0 ? new DictionaryValueProvider(forms, null) : null;
		}
	}
}
