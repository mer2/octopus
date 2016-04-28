using System;

namespace Octopus.Web
{
	/// <summary>
	/// 页面内嵌（虚拟路径）属性定义
	/// </summary>
	[Serializable, AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class EmbeddedPathAttribute : Attribute
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="path">内嵌资源完整路径前缀，如“Octopus.Web.AdminBase.Mvc.Views.Shared.”</param>
		/// <param name="virtualPath">被映射后的Web虚拟目录，如“~/Views/Shared/”</param>
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

		/// <summary>
		/// 内嵌资源完整路径前缀，如“Octopus.Web.AdminBase.Mvc.Views.Shared.”
		/// </summary>
		public string Path { get; private set; }
		/// <summary>
		/// 被映射后的Web虚拟目录，如“~/Views/Shared/”
		/// </summary>
		public string VirtualPath { get; private set; }
	}
}