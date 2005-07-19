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
		protected override void OnPageLoad()
		{
			GUIPropertyManager.SetProperty("#Wizard.DVBT.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.DVBC.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.DVBS.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.ATSC.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.Analog.Done","no");
			GUIPropertyManager.SetProperty("#Wizard.Remote.Done","no");
			GUIPropertyManager.SetProperty("#WizardCard","0");
				
			base.OnPageLoad ();
		}

	}
}
