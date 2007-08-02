using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  public partial class BaseInstallationTypeDlg : DeployDialog, IDeployDialog
  {
    public BaseInstallationTypeDlg()
    {
      InitializeComponent();
      type = DialogType.BASE_INSTALLATION_TYPE;
      rbSingleSeat.Checked = true;
    }
    public override DeployDialog GetNextDialog()
    {
      if (rbSingleSeat.Checked)
        return (DeployDialog)new SingleSeatDlg();
      return null;
    }
  }
}
