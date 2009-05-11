using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralScreensaver : SectionSettings
  {
    #region ctor
    public GeneralScreensaver()
      : this("Screensaver")
    {
    }
    public GeneralScreensaver(string name)
      : base(name)
    {
      InitializeComponent();
    }
    #endregion

    #region Persistance
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxEnableScreensaver.Checked = xmlreader.GetValueAsBool("general", "screensaver", false);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "screensavertime", 60);
      }
    }
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValueAsBool("general", "screensaver", checkBoxEnableScreensaver.Checked);
        xmlreader.SetValue("general", "screensavertime", numericUpDownDelay.Value);
      }
    }
    #endregion
  }
}
