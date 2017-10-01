using System;
using HTB.DevFx.Core;

namespace Octopus.Esb
{
	[Serializable]
	public class EsbResult<T> : AOPResult<T>, IAOPResult<T>
	{
		private T attachObject;
		public new T ResultAttachObject {
			get => this.attachObject;
			set {
				this.attachObject = value;
				base.ResultAttachObject = value;
			}
		}

		public T ResultAttachObjectEx => this.ResultAttachObject;
	}
}
