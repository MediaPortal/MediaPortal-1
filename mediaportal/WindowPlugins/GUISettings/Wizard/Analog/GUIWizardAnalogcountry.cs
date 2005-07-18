using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogcountry.
	/// </summary>
	public class GUIWizardAnalogcountry:GUIWindow
	{
		public GUIWizardAnalogcountry()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_COUNTRY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_analog_county.xml");
		}
	}
}
