using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  public partial class DeployDialog : UserControl,IDeployDialog
  {
    public DialogType type;
    public DeployDialog()
    {
      InitializeComponent();
    }
    public virtual DeployDialog GetNextDialog()
    {
      return null;
    }
  }
}
