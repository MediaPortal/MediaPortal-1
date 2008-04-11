using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralWatchdog : SectionSettings
  {
    #region ctor
    public GeneralWatchdog()
      : this("Watchdog")
    {
    }
    public GeneralWatchdog(string name)
      : base("Watchdog")
    {
      InitializeComponent();
    }
    #endregion

    #region Persistance
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxEnableWatchdog.Checked = xmlreader.GetValueAsBool("general", "watchdogEnabled", true);
        checkBoxAutoRestart.Checked = xmlreader.GetValueAsBool("general", "restartOnError", false);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "restart delay", 10);
      }
    }
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValueAsBool("general", "watchdogEnabled", checkBoxEnableWatchdog.Checked);
        xmlreader.SetValueAsBool("general", "restartOnError", checkBoxAutoRestart.Checked);
        xmlreader.SetValue("general", "restart delay", numericUpDownDelay.Value);
      }
    }
    #endregion
  }
}
