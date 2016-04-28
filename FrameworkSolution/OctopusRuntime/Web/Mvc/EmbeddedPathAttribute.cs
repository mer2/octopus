using System;
using System.Collections.Generic;
using HTB.DevFx.Utils;

namespace Octopus.Web.Mvc
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class EmbeddedPathAttribute : Attribute
	{
		public EmbeddedPathAttribute(string path, string virtualPath) {
			if(string.IsNullOrEmpty(path)) {
				throw new ArgumentNullException("path");
			}
			if(string.IsNullOrEmpty(virtualPath)) {
				throw new ArgumentNullException("virtualPath");
			}
			this.Path = path;
			this.VirtualPath = virtualPath;
		}
		
		public string Path { get; private set; }
		public string VirtualPath { get; private set; }

		internal static IDictionary<string, EmbeddedVirtualFile> GetEmbeddedVirtualFiles() {
			var dict = new Dictionary<string, EmbeddedVirtualFile>();
			TypeHelper.GetAttributeFromAssembly<EmbeddedPathAttribute>(null, (p, a) => {
				if(p == null || p.Length <= 0) {
					return;
				}
				var names = a.GetManifestResourceNames();
				if(names.Length <= 0) {
					return;
				}
				var ns = new List<string>(names);
				foreach(var path in p) {
					var ph = path.Path;
					for(var i = 0; i < ns.Count;) {
						var n = ns[i];
						if(n.StartsWith(ph, true, null)) {
							var virtualPath = path.VirtualPath + n.Substring(ph.Length);
							dict.Add(virtualPath.ToLower(), new EmbeddedVirtualFile(virtualPath, a.GetManifestResourceStream(n)));
							ns.RemoveAt(i);
						} else {
							i++;
						}
					}
				}
			});
			return dict;
		}
	}
}
