using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralStartupDelay : SectionSettings
  {
    #region ctor
    public GeneralStartupDelay()
      : this("Startup Delay")
    {
    }
    public GeneralStartupDelay(string name)
      : base("Startup Delay")
    {
      InitializeComponent();
    }
    #endregion

    #region Persistance
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        nudDelay.Value = xmlreader.GetValueAsInt("general", "startup delay", 0);
        cbWaitForTvService.Checked = xmlreader.GetValueAsBool("general", "wait for tvserver", false);
      }
    }
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("general", "startup delay", nudDelay.Value);
        xmlreader.SetValueAsBool("general", "wait for tvserver", cbWaitForTvService.Checked);
      }
    }
    #endregion
  }
}
