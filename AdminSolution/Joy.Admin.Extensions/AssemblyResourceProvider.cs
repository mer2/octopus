using System;
using System.Data;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Hosting;
using System.IO;
using System.Reflection;

namespace Joy.Admin.Extensions
{
    public class AssemblyResourceProvider : System.Web.Hosting.VirtualPathProvider
    {
        public AssemblyResourceProvider() { }
        private bool IsAppResourcePath(string virtualPath)
        {
            String checkPath =
               VirtualPathUtility.ToAppRelative(virtualPath);
            return checkPath.StartsWith("~/Plugin/", StringComparison.InvariantCultureIgnoreCase);
        }
        public override bool FileExists(string virtualPath)
        {
            return (AssemblyResourceVirtualFile.GetViewStream(virtualPath) != null || base.FileExists(virtualPath));
        }
        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsAppResourcePath(virtualPath))
                return new AssemblyResourceVirtualFile(virtualPath);
            else
                return base.GetFile(virtualPath);
        }
        public override System.Web.Caching.CacheDependency GetCacheDependency(string virtualPath, System.Collections.IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsAppResourcePath(virtualPath))
                return null;
            else
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }

    class AssemblyResourceVirtualFile : VirtualFile
    {
        string path;
        public AssemblyResourceVirtualFile(string virtualPath)
            : base(virtualPath)
        {
            path = VirtualPathUtility.ToAppRelative(virtualPath);
        }
        public override System.IO.Stream Open() {
            return GetViewStream(this.path);
        }

        public static Stream GetViewStream(string path) {
            string[] parts = path.Split('/');
            if (parts.Length < 4) {
                return null;
            }
            string assemblyName = parts[2];
            if (!assemblyName.EndsWith(".dll")) {
                return null;
            }
            string resourceName = parts[3];

            assemblyName = Path.Combine(HttpRuntime.BinDirectory, assemblyName);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyName);
            if(assembly != null) {
                Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                return resourceStream;
            }
            return null;
        }
    }
}
