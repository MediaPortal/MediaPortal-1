using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class  GUIWizardWelcome : GUIWindow
	{

		public  GUIWizardWelcome()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_WELCOME;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_welcome.xml");
		}
	}
}
