using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.TV
{
	/// <summary>
	/// Summary description for GUISettingsRecordings.
	/// </summary>
	public class GUISettingsRecordings : GUIWindow
	{
		public GUISettingsRecordings()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_RECORDINGS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settings_recording.xml");
		}

	}
}
