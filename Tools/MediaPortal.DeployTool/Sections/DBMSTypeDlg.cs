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
    int dbmsType;
    public DBMSTypeDlg()
    {
      InitializeComponent();
      type = DialogType.DBMSType;
      imgMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      dbmsType = 1;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Utils.GetBestTranslation("DBMSType_labelHeading");
      rbMSSQL.Text = Utils.GetBestTranslation("DBMSType_rbMSSQL");
      rbMySQL.Text = Utils.GetBestTranslation("DBMSType_rbMySQL");
      rbDBAlreadyInstalled.Text = Utils.GetBestTranslation("DBMSType_rbDBAlreadyInstalled");
    }
    public override DeployDialog GetNextDialog()
    {
      if (dbmsType == 3)
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
      switch (dbmsType)
      {
        case 1:
          InstallationProperties.Instance.Set("DBMSType", "mssql2005");
          break;
        case 2:
          InstallationProperties.Instance.Set("DBMSType", "mysql");
          break;
        case 3:
          InstallationProperties.Instance.Set("DBMSType", "DBAlreadyInstalled");
          break;
      }
    }
    #endregion

    private void imgMS_Click(object sender, EventArgs e)
    {
      imgMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      imgMySQL.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgExists.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      dbmsType = 1;
    }

    private void imgMySQL_Click(object sender, EventArgs e)
    {
      imgMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgMySQL.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      imgExists.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      dbmsType = 2;
    }

    private void imgExists_Click(object sender, EventArgs e)
    {
      imgMS.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgMySQL.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgExists.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      dbmsType = 3;
    }
  }
}
