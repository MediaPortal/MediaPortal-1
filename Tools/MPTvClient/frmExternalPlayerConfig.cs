using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MPTvClient
{
  public partial class frmExternalPlayerConfig : Form
  {
    public frmExternalPlayerConfig()
    {
      InitializeComponent();
    }
    public void InitForm(string exe, string parameters, bool useOverride, string overrideUrl)
    {
      edExe.Text = exe;
      edParams.Text = parameters;
      cbURLOverride.Checked = useOverride;
      edVLCURL.Text = overrideUrl;
    }
    public void GetConfig(ref string exe, ref string parameters, ref bool useOverride,ref string overrideUrl)
    {
      exe = edExe.Text;
      parameters = edParams.Text;
      useOverride = cbURLOverride.Checked;
      overrideUrl = edVLCURL.Text;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
      DialogResult result = OpenDlg.ShowDialog();
      if (result == DialogResult.OK)
        edExe.Text = OpenDlg.FileName;
    }
  }
}