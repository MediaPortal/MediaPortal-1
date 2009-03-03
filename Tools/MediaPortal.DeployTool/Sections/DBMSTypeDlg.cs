using System;

namespace MediaPortal.DeployTool.Sections
{
  public partial class DBMSTypeDlg : DeployDialog
  {
    int dbmsType;
    public DBMSTypeDlg()
    {
      InitializeComponent();
      type = DialogType.DBMSType;
      bMS.Image = Images.Choose_button_on;
      dbmsType = 1;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.GetBestTranslation("DBMSType_labelHeading");
      rbMSSQL.Text = Localizer.GetBestTranslation("DBMSType_rbMSSQL");
      rbMySQL.Text = Localizer.GetBestTranslation("DBMSType_rbMySQL");
      rbDBAlreadyInstalled.Text = Localizer.GetBestTranslation("DBMSType_rbDBAlreadyInstalled");
    }
    public override DeployDialog GetNextDialog()
    {
      if (dbmsType == 3)
      {
        if (InstallationProperties.Instance["InstallType"] == "singleseat")
        {
          return DialogFlowHandler.Instance.GetDialogInstance(DialogType.MPSettings);
        }
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.TvServerSettings);
      }
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

    private void bMS_Click(object sender, EventArgs e)
    {
      bMS.Image = Images.Choose_button_on;
      bMySQL.Image = Images.Choose_button_off;
      bExists.Image = Images.Choose_button_off;
      dbmsType = 1;
    }

    private void bMySQL_Click(object sender, EventArgs e)
    {
      bMS.Image = Images.Choose_button_off;
      bMySQL.Image = Images.Choose_button_on;
      bExists.Image = Images.Choose_button_off;
      dbmsType = 2;
    }

    private void bExists_Click(object sender, EventArgs e)
    {
      bMS.Image = Images.Choose_button_off;
      bMySQL.Image = Images.Choose_button_off;
      bExists.Image = Images.Choose_button_on;
      dbmsType = 3;
    }
  }
}