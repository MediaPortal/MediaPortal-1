using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogCity.
	/// </summary>
	public class GUIWizardAnalogCity :GUIWindow
	{
		public GUIWizardAnalogCity()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_CITY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_analog_city.xml");
		}
	}
}
