using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
  public partial class EVRConfig : MediaPortal.Configuration.SectionSettings
  {

    bool _init = false;

    public EVRConfig()
      :
      this("Enhanced Video Renderer Settings")
    {
    }
    public EVRConfig(string name):
      base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          EVRCheckBox.Checked = xmlreader.GetValueAsBool("general", "useevr", false);
        }
        _init = true;
      }
    }

    public override void SaveSettings()
    {
      if (_init == false) return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("general", "useevr", EVRCheckBox.Checked);
      }
    }

  }
}
