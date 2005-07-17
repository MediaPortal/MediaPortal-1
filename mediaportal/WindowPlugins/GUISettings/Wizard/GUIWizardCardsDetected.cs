using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// Summary description for GUIWizardCardsDetected.
	/// </summary>
	public class GUIWizardCardsDetected: GUIWindow
	{
		public GUIWizardCardsDetected()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_CARDS_DETECTED;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcards_detected.xml");
		}
	}
}
