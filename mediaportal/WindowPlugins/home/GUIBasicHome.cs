using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.home
{
	/// <summary>
	/// Summary description for GUIBasicHome.
	/// </summary>
	public class GUIBasicHome : GUIWindow
	{
		public GUIBasicHome()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SECOND_HOME;
		}
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\BasicHome.xml");
		}

	}
}
