using System;

namespace MediaPortal.Dispatcher
{
	[Flags]
	public enum JobFlags
	{
		None,
		Session,
		Intensive,
		Wake,
	}
}
