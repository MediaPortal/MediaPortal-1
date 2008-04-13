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

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.Instance.GetString("DBMSType_labelHeading");
      rbMSSQL.Text = Localizer.Instance.GetString("DBMSType_rbMSSQL");
      rbMySQL.Text = Localizer.Instance.GetString("DBMSType_rbMySQL");
      rbDBAlreadyInstalled.Text = Localizer.Instance.GetString("DBMSType_rbDBAlreadyInstalled");
    }
    public override DeployDialog GetNextDialog()
    {
        if (rbDBAlreadyInstalled.Checked)
        {
            if (InstallationProperties.Instance["InstallType"] == "singleseat")
                return DialogFlowHandler.Instance.GetDialogInstance(DialogType.MPSettings);
            else
                return DialogFlowHandler.Instance.GetDialogInstance(DialogType.TvServerSettings);
        }
        else
            return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DBMSSettings);
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      if (rbMSSQL.Checked)
        InstallationProperties.Instance.Set("DBMSType", "mssql2005");
      if (rbMySQL.Checked)
        InstallationProperties.Instance.Set("DBMSType", "mysql");
      if (rbDBAlreadyInstalled.Checked)
          InstallationProperties.Instance.Set("DBMSType", "DBAlreadyInstalled");
    }
    #endregion
  }
}
