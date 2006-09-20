using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;

namespace WindowPlugins.GUISettings.Wizard.DVBC
{
  /// <summary>
  /// Summary description for GUIWizardDVBCCountry.
  /// </summary>
  public class GUIWizardDVBCScan : GUIWizardScanBase
  {

    public GUIWizardDVBCScan()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_DVBC_SCAN;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_DVBC_scan.xml");
    }


    protected override ITuning GetTuningInterface(TVCaptureDevice captureCard)
    {
      string country = GUIPropertyManager.GetProperty("#WizardCountry");
      ITuning tuning = new DVBCTuning();
      String[] parameters = new String[1];
      parameters[0] = country;
      tuning.AutoTuneTV(captureCard, this, parameters);
      return tuning;
    }
    protected override void OnScanDone()
    {
      GUIPropertyManager.SetProperty("#Wizard.DVBC.Done", "yes");
    }
    protected override NetworkType Network()
    {
      return NetworkType.DVBC;
    }
  }
}
