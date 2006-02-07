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
namespace WindowPlugins.GUISettings.Wizard.ATSC
{
  /// <summary>
  /// Summary description for GUIWizardATSCCountry.
  /// </summary>
  public class GUIWizardATSCScan : GUIWizardScanBase
  {
    
    public GUIWizardATSCScan()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_ATSC_SCAN;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_ATSC_scan.xml");
    }

    protected override ITuning GetTuningInterface(TVCaptureDevice captureCard)
    {
      ITuning tuning = new ATSCTuning();
      tuning.AutoTuneTV(captureCard, this);
      return tuning;
    }
    protected override void OnScanDone()
    {
      GUIPropertyManager.SetProperty("#Wizard.ATSC.Done", "yes");
    }
    protected override NetworkType Network()
    {
      return NetworkType.ATSC;
    }
  }
}
