using System.IO;
using System.Web.Hosting;

namespace Octopus.Web.Mvc
{
	internal class EmbeddedVirtualFile : VirtualFile
	{
		public EmbeddedVirtualFile(string virtualPath, Stream stream) : base(virtualPath) {
			var ms = this.EmbeddedStream = new MemoryStream();
			stream.CopyTo(ms);
			ms.Position = 0;
		}

		public Stream EmbeddedStream { get; set; }

		public override Stream Open() {
			return this.EmbeddedStream;
		}
	}
}