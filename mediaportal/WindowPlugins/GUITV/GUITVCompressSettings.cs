using System;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUITV
{
	/// <summary>
	/// Summary description for GUITVCompressSettings.
	/// </summary>
	public class GUITVCompressSettings : GUIWindow	
	{
		public GUITVCompressSettings()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\mytvcompresssettings.xml");
		}

	}
}
