using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for IWakeable.
	/// </summary>
	public interface IWakeable
	{
		DateTime GetNextEvent(DateTime earliestWakeuptime);
		bool DisallowShutdown();
		String PluginName();
	}
}
