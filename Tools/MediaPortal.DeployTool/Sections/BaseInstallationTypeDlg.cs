#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;

using MediaPortal.DeployTool.InstallationChecks;

namespace MediaPortal.DeployTool.Sections
{
  public partial class BaseInstallationTypeDlg : DeployDialog
  {
    private bool rbOneClickChecked;

    public BaseInstallationTypeDlg()
    {
      InitializeComponent();
      type = DialogType.BASE_INSTALLATION_TYPE;
      labelSectionHeader.Text = "";
      bOneClick.Image = Images.Choose_button_on;
      rbOneClickChecked = true;
      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      labelOneClickCaption.Text = Localizer.GetBestTranslation("BaseInstallation_labelOneClickCaption");
      labelOneClickDesc.Text = Localizer.GetBestTranslation("BaseInstallation_labelOneClickDesc");
      rbOneClick.Text = Localizer.GetBestTranslation("BaseInstallation_rbOneClick");
      labelAdvancedCaption.Text = Localizer.GetBestTranslation("BaseInstallation_labelAdvancedCaption");
      labelAdvancedDesc.Text = Localizer.GetBestTranslation("BaseInstallation_labelAdvancedDesc");
      rbAdvanced.Text = Localizer.GetBestTranslation("BaseInstallation_rbAdvanced");
    }

    public override DeployDialog GetNextDialog()
    {
      InstallationProperties.Instance["OneClickInstallation"] = rbOneClickChecked ? "1" : "0";

      if (rbOneClickChecked)
      {
#if NO_TV_SERVER
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.ExtensionChoice);
#else
        return DialogFlowHandler.Instance.GetDialogInstance(Utils.Is64bit() ? DialogType.TvServerWarning : DialogType.ExtensionChoice);
#endif
      }
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.CUSTOM_INSTALLATION_TYPE);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    public override void SetProperties()
    {
      if (rbOneClickChecked)
      {
        InstallationProperties.Instance.Set("InstallTypeHeader",
                                            Localizer.GetBestTranslation("BaseInstallation_rbOneClick"));
#if NO_TV_SERVER
        InstallationProperties.Instance.Set("InstallType", "client");
#else
        InstallationProperties.Instance.Set("InstallType", "singleseat");
#endif
        InstallationProperties.Instance.Set("ConfigureTVServerFirewall", "1");
        InstallationProperties.Instance.Set("ConfigureMediaPortalFirewall", "1");
        InstallationProperties.Instance.Set("ConfigureDBMSFirewall", "1");
        InstallationProperties.Instance.Set("DBMSPassword", "MediaPortal");
        // Default DBMS
        IInstallationPackage packageMSSQL = new MSSQLExpressChecker();
        CheckResult resultMSSQL = packageMSSQL.CheckStatus();
        IInstallationPackage packageMySQL = new MySQLChecker();
        CheckResult resultMySQL = packageMySQL.CheckStatus();
        IInstallationPackage packageMariaDB = new MariaDBChecker();
        CheckResult resultMariaDB = packageMariaDB.CheckStatus();
        if (resultMSSQL.state == CheckState.INSTALLED || resultMSSQL.state == CheckState.VERSION_MISMATCH ||
            resultMySQL.state == CheckState.INSTALLED || resultMySQL.state == CheckState.VERSION_MISMATCH ||
            resultMariaDB.state == CheckState.INSTALLED || resultMariaDB.state == CheckState.VERSION_MISMATCH)
        {
          InstallationProperties.Instance.Set("DBMSType", "DBAlreadyInstalled");
        }
        else
        {
          if (OSInfo.OSInfo.Win10OrLater() && Utils.Is64bitOS)
          {
            InstallationProperties.Instance.Set("DBMSType", "MariaDB");
            InstallationProperties.Instance.Set("DBMSDir", InstallationProperties.Instance["ProgramFiles"] + "\\MariaDB\\MariaDB 10.0");
          }
          else
          {
            InstallationProperties.Instance.Set("DBMSType", "MySQL");
            InstallationProperties.Instance.Set("DBMSDir", InstallationProperties.Instance["ProgramFiles"] + "\\MySQL\\MySQL Server 5.6");
          }
        }
      }
    }

#endregion

    private void rbOneClick_Click(object sender, EventArgs e)
    {
      bOneClick_Click(sender, e);
    }

    private void rbAdvanced_Click(object sender, EventArgs e)
    {
      bAdvanced_Click(sender, e);
    }

    private void bOneClick_Click(object sender, EventArgs e)
    {
      bOneClick.Image = Images.Choose_button_on;
      bAdvanced.Image = Images.Choose_button_off;
      rbOneClickChecked = true;
    }

    private void bAdvanced_Click(object sender, EventArgs e)
    {
      bOneClick.Image = Images.Choose_button_off;
      bAdvanced.Image = Images.Choose_button_on;
      rbOneClickChecked = false;
    }
  }
}
