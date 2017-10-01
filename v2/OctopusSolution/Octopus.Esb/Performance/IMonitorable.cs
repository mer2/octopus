using System.Collections.Generic;
using HTB.DevFx.Core;

namespace Octopus.Performance
{
	public interface IMonitorable
	{
		void Monitor(IList<IAOPResult> results);
	}
}