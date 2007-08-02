using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  public partial class SingleSeatDlg : DeployDialog,IDeployDialog
  {
    public SingleSeatDlg()
    {
      InitializeComponent();
      type = DialogType.SINGLESEAT;
    }
    public override DeployDialog GetNextDialog()
    {
      return null;
    }
  }
}
