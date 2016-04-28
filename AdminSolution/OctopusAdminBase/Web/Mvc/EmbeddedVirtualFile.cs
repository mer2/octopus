using System.IO;
using System.Web.Hosting;

namespace Octopus.Web.Mvc
{
	internal class EmbeddedVirtualFile : VirtualFile
	{
		public EmbeddedVirtualFile(string virtualPath, Stream stream) : base(virtualPath) {
			this.EmbeddedStream = stream;
		}

		public Stream EmbeddedStream { get; set; }

		public override Stream Open() {
			return this.EmbeddedStream;
		}
	}
}