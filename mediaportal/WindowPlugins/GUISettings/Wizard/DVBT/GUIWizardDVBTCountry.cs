using System;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUISettings.Wizard.DVBT
{
	/// <summary>
	/// Summary description for GUIWizardDVBTCountry.
	/// </summary>
	public class GUIWizardDVBTCountry : GUIWindow
	{
		public GUIWizardDVBTCountry()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBT_COUNTRY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbt_country.xml");
		}
	}
}
