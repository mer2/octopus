using System;
using System.Collections;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Collections.Generic;

namespace Octopus.Web.Mvc
{
	internal class EmbeddedVirtualPathProvider : VirtualPathProvider
	{
		public EmbeddedVirtualPathProvider() {
			this.EmbeddedVirtualFiles = EmbeddedPathAttribute.GetEmbeddedVirtualFiles();
		}
		internal IDictionary<string, EmbeddedVirtualFile> EmbeddedVirtualFiles { get; private set; }

		internal string GetVirtualPath(string virtualPath) {
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
			return this.EmbeddedVirtualFiles.ContainsKey(this.GetVirtualPath(virtualPath));
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
			if(this.EmbeddedVirtualFiles.TryGetValue(this.GetVirtualPath(virtualPath), out virtualFile)) {
				return virtualFile;
			}
			return null;
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart) {
			return null;
		}
	}
}