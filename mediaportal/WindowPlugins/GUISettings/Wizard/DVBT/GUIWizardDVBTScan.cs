using System;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUISettings.Wizard.DVBT
{
	/// <summary>
	/// Summary description for GUIWizardDVBTCountry.
	/// </summary>
	public class GUIWizardDVBTScan : GUIWindow
	{
		public GUIWizardDVBTScan()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbt_scan.xml");
		}
	}
}
