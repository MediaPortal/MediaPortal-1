using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class  GUIWizardFinished : GUIWindow
	{

		public  GUIWizardFinished()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_FINISHED;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_Finished.xml");
		}
	}
}
