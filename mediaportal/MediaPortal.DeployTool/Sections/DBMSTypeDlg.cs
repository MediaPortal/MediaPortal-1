using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace MediaPortal.DeployTool
{
  public partial class DBMSTypeDlg : DeployDialog, IDeployDialog
  {
    public DBMSTypeDlg()
    {
      InitializeComponent();
      type = DialogType.DBMSType;
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.Instance.GetString("DBMSType_labelHeading");
      rbMSSQL.Text = Localizer.Instance.GetString("DBMSType_rbMSSQL");
      rbMySQL.Text = Localizer.Instance.GetString("DBMSType_rbMySQL");
    }
    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DBMSSettings);
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      if (rbMSSQL.Checked)
        InstallationProperties.Instance.Set("DBMSType", "mssql");
      else
        InstallationProperties.Instance.Set("DBMSType", "mysql");
    }
    #endregion
  }
}
