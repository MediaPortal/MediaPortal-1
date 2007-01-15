using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
  public partial class VMR9Config : MediaPortal.Configuration.SectionSettings
  {

    bool _init = false;

    public VMR9Config()
      :
      this("Video Mixing Renderer 9 Settings")
    {
    }
    public VMR9Config(string name):
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
          mpNonsquare.Checked = xmlreader.GetValueAsBool("mytv", "nonsquare", false);
          DXEclusiveCheckbox.Checked = xmlreader.GetValueAsBool("general", "exclusivemode", true);
          mpVMR9FilterMethod.Text = xmlreader.GetValueAsString("mytv", "dx9filteringmode", "None");
        }
        _init = true;
      }
    }

    public override void SaveSettings()
    {
      if (_init == false) return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("mytv", "nonsquare", mpNonsquare.Checked);
        xmlwriter.SetValueAsBool("general", "exclusivemode", DXEclusiveCheckbox.Checked);
        xmlwriter.SetValue("mytv", "dx9filteringmode", mpVMR9FilterMethod.Text);
        
      
      }
    }

  }
}
