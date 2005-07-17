using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
	/// <summary>
	/// Summary description for GUIWizardDVBSLNBDetails.
	/// </summary>
	public class GUIWizardDVBSLNBDetails : GUIWindow
	{
		public GUIWizardDVBSLNBDetails()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SELECT_DETAILS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbs_LNB2.xml");
		}
	}
}
