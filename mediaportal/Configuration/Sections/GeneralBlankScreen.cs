using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralBlankScreen : MediaPortal.Configuration.SectionSettings
  {
    public GeneralBlankScreen()
      : this("Blank Screen")
    {
    }

    
    public GeneralBlankScreen(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        chbEnabled.Checked = xmlreader.GetValueAsBool("general", "screensaver", true);
        nudIdleTime.Value = xmlreader.GetValueAsInt("general", "screensavertime", 60);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("general", "screensaver", chbEnabled.Checked);
        xmlwriter.SetValue("general", "screensavertime", nudIdleTime.Value);
      }
    }

    
    
  }
}
