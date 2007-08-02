using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  public partial class DeployTool : Form
  {
    private DeployDialog _currentDialog;
    public DeployTool()
    {
      InitializeComponent();
      _currentDialog = DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE);
      splitContainer2.Panel1.Controls.Add(_currentDialog);
      backButton.Visible = false;
    }
    private void SwitchDialog(DeployDialog dlg)
    {
      splitContainer2.Panel1.Controls.Clear();
      splitContainer2.Panel1.Controls.Add(dlg);
    }

    private void nextButton_Click(object sender, EventArgs e)
    {
      _currentDialog = _currentDialog.GetNextDialog();
      SwitchDialog(_currentDialog);
      if (!backButton.Visible)
        backButton.Visible = true;
    }

    private void backButton_Click(object sender, EventArgs e)
    {
      _currentDialog = DialogFlowHandler.Instance.GetPreviousDlg();
      SwitchDialog(_currentDialog);
    }
  }
}