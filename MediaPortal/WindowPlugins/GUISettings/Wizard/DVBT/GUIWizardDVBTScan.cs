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

namespace WindowPlugins.GUISettings.Wizard.DVBT
{
  /// <summary>
  /// Summary description for GUIWizardDVBTCountry.
  /// </summary>
  public class GUIWizardDVBTScan : GUIWizardScanBase
  {
    public GUIWizardDVBTScan()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_dvbt_scan.xml");
    }

    protected override ITuning GetTuningInterface(TVCaptureDevice captureCard)
    {
      string country = GUIPropertyManager.GetProperty("#WizardCountry");

      ITuning tuning = new DVBTTuning();
      String[] parameters = new String[1];
      parameters[0] = country;
      tuning.AutoTuneTV(captureCard, this, parameters);
      return tuning;
    }
    protected override void OnScanDone()
    {
      GUIPropertyManager.SetProperty("#Wizard.DVBT.Done", "yes");
    }
    protected override NetworkType Network()
    {
      return NetworkType.DVBT;
    }

  }
}
