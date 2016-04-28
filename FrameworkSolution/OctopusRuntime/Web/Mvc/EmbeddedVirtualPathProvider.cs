using System;
using System.Collections;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Collections.Generic;
using HTB.DevFx.Utils;

namespace Octopus.Web.Mvc
{
	internal class EmbeddedVirtualPathProvider : VirtualPathProvider
	{
		public EmbeddedVirtualPathProvider() {
			this.EmbeddedVirtualFiles = GetEmbeddedVirtualFiles();
		}
		internal IDictionary<string, EmbeddedVirtualFile> EmbeddedVirtualFiles { get; private set; }
		internal static IDictionary<string, EmbeddedVirtualFile> GetEmbeddedVirtualFiles() {//从应用程序的bin里查找所有的属性
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
					for(var i = 0; i < ns.Count; ) {
						var n = ns[i];
						if(n.StartsWith(ph, true, null)) {
							var virtualPath = path.VirtualPath + n.Substring(ph.Length);
							dict[virtualPath.ToLower()] = new EmbeddedVirtualFile(virtualPath, a.GetManifestResourceStream(n));
							ns.RemoveAt(i);
						} else {
							i++;
						}
					}
				}
			});
			return dict;
		}

		internal static string GetVirtualPath(string virtualPath) {
			if (virtualPath == null) throw new ArgumentNullException("virtualPath");
			if (virtualPath.StartsWith("/"))
				virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
			if (!virtualPath.StartsWith("~"))
				virtualPath = !virtualPath.StartsWith("/") ? "~/" + virtualPath : "~" + virtualPath;
			return virtualPath.ToLower();
		}

		/// <summary>
		/// Gets a value that indicates whether a file exists in the virtual file system.
		/// </summary>
		/// <returns>
		/// true if the file exists in the virtual file system; otherwise, false.
		/// </returns>
		/// <param name="virtualPath">The path to the virtual file.</param>
		public override bool FileExists(string virtualPath) {
			if(this.Previous.FileExists(virtualPath)) {
				return true;
			}
			return this.EmbeddedVirtualFiles.ContainsKey(GetVirtualPath(virtualPath));
		}

		/// <summary>
		/// Gets a virtual file from the virtual file system.
		/// </summary>
		/// <returns>
		/// A descendent of the <see cref="T:System.Web.Hosting.VirtualFile"/> class that represents a file in the virtual file system.
		/// </returns>
		/// <param name="virtualPath">The path to the virtual file.</param>
		public override VirtualFile GetFile(string virtualPath) {
			if (Previous.FileExists(virtualPath)) {
				return Previous.GetFile(virtualPath);
			}
			EmbeddedVirtualFile virtualFile;
			if(this.EmbeddedVirtualFiles.TryGetValue(GetVirtualPath(virtualPath), out virtualFile)) {
				return virtualFile;
			}
			return null;
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart) {
			return null;
		}
	}
}