using System;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUITV
{
	/// <summary>
	/// Summary description for GUITVCompressMain.
	/// </summary>
	public class GUITVCompressMain : GUIWindow	
	{
		public GUITVCompressMain()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\mytvcompressmain.xml");
		}

	}
}
