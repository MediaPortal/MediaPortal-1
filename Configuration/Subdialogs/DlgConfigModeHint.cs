using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Profile;

namespace MediaPortal.Configuration
{
  public partial class DlgConfigModeHint : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    public DlgConfigModeHint()
    {
      InitializeComponent();
    }

    private void btnContinue_Click(object sender, EventArgs e)
    {
      if (checkBoxConfirmed.Checked)
        this.DialogResult = DialogResult.OK;

      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("general", "AdvancedConfigMode", radioButtonAdvanced.Checked);
      }

      this.Close();
    }

  }
}