using System;
using System.Threading;

namespace MediaPortal.Dispatcher
{
	public enum JobPriority
	{
		AboveNormal = ThreadPriority.AboveNormal,
		BelowNormal = ThreadPriority.BelowNormal,
		Highest = ThreadPriority.Highest,
		Lowest = ThreadPriority.Lowest,
		Normal = ThreadPriority.Normal,
	}
}
