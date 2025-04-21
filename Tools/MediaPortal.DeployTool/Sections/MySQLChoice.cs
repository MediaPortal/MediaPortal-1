﻿#region Copyright (C) 2005-2024 Team MediaPortal

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
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class MySQLChoice : DeployDialog
  {

    public MySQLChoice()
    {
      InitializeComponent();
      type = DialogType.MysqlUpgrade;
      labelSectionHeader.Text = "";

      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      linkMySQL.Text = Localizer.GetBestTranslation("MySQLChoice_MoreInfo");
      lblRecommended.Text = Localizer.GetBestTranslation("MySQLChoice_Title");
      lblMySQLText.Text = Localizer.GetBestTranslation("MySQLChoice_Information");
    }

    public override DeployDialog GetNextDialog()
    {
      if (UpgradeDlg.rbFreshChecked)
      {
        // Normal deploy...
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.WatchTV);
      }
      // Direct to upgrade
      if (InstallationProperties.Instance.Get("InstallType") != "tvserver_master")
      {  // install includes MP so check skin choice
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.SkinChoice);
      }
      // tv server only install so no need for skin choice
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("ConfigureMediaPortalMySQL", checkMySQL.Checked ? "Yes" : "No");

      // Check here if we need to upgrade
      if (UpgradeDlg.MySQL56)
      {
        if (InstallationProperties.Instance["ConfigureMediaPortalMySQL"] == "Yes")
        {
          // Set SQL setting needed for MySQL upgrade
          InstallationProperties.Instance.Set("ConfigureTVServerFirewall", "1");
          InstallationProperties.Instance.Set("ConfigureMediaPortalFirewall", "1");
          InstallationProperties.Instance.Set("ConfigureDBMSFirewall", "1");
          InstallationProperties.Instance.Set("DBMSPassword", "MediaPortal");
          // Default DBMS
          InstallationProperties.Instance.Set("DBMSType", "MySQL");
          InstallationProperties.Instance.Set("DBMSDir",
            InstallationProperties.Instance["ProgramFiles"] +
            "\\MySQL\\MySQL Server 8.3");
        }
      }
    }

    #endregion

    #region Hyperlink handler

    private void linkMySQL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Utils.OpenURL("https://www.team-mediaportal.com/wiki/display/MediaPortal1/1.8.x%20-%20MySQL%20Upgrade%20choice");
    }

    #endregion

  }
}
