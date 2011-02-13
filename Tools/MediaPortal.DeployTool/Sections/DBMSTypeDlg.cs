#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Diagnostics;
using System.Drawing;

namespace MediaPortal.DeployTool.Sections
{
  public partial class DBMSTypeDlg : DeployDialog
  {
    private int dbmsType;

    public DBMSTypeDlg()
    {
      InitializeComponent();
      type = DialogType.DBMSType;

      bMySQL.Image = Images.Choose_button_on;
      dbmsType = 2;
      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.GetBestTranslation("DBMSType_labelHeading");
      rbMSSQL.Text = Localizer.GetBestTranslation("DBMSType_rbMSSQL");
      rbMySQL.Text = Localizer.GetBestTranslation("DBMSType_rbMySQL");
      rbDBAlreadyInstalled.Text = Localizer.GetBestTranslation("DBMSType_rbDBAlreadyInstalled");

      //MSSQL2005 is not supported on Windows 7 and later
      if (OSInfo.OSInfo.Win7OrLater())
      {
        bMS.Enabled = false;
        // For better readability label is not disabled, only the eventhandler is removed
        rbMSSQL.Click -= bMS_Click;
        rbMSSQL.Cursor = DefaultCursor;
        rbMSSQL.ForeColor = Color.DimGray;
        rbMSSQL.Text = Localizer.GetBestTranslation("DBMSType_rbMSSQL_disabled");
        lbMSSQL.Visible = true;
        lbMSSQL.Enabled = true;
        lbMSSQL.Text = Localizer.GetBestTranslation("DBMSType_lbMSSQL_disabled");
      }
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
          InstallationProperties.Instance.Set("DBMSType", "msSQL2005");
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

    private void lbMSSQL_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start("http://www.team-mediaportal.com/manual/TV-Server/install-SQL-Server-2008");
      }
      catch (Exception) {}
    }
  }
}