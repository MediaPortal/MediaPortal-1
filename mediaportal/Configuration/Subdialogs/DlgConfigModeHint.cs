using System;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration
{
  public partial class DlgConfigModeHint : MPConfigForm
  {
    public DlgConfigModeHint()
    {
      InitializeComponent();
    }

    private void btnContinue_Click(object sender, EventArgs e)
    {
      if (checkBoxConfirmed.Checked)
      {
        this.DialogResult = DialogResult.OK;
      }

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "AdvancedConfigMode", radioButtonAdvanced.Checked);
      }

      this.Close();
    }
  }
}