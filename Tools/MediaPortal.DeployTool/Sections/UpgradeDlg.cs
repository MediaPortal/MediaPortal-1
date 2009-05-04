#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using Microsoft.Win32;

namespace MediaPortal.DeployTool.Sections
{
  public partial class UpgradeDlg : DeployDialog
  {
    bool rbFreshChecked;

    public UpgradeDlg()
    {
      InitializeComponent();
      type = DialogType.Upgrade;
      labelSectionHeader.Text = "";
      bFresh.Image = Images.Choose_button_on;
      rbFreshChecked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      //
      // Disable upgrade if installed version is different from previous or SVN is installed
      // es1.  1.0.1 is current, installed must be 1.0.0
      // es2.  1.0 SVN XXXXX is installed
      //
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal");
      string MpVer = string.Empty;
      string MpBuild = string.Empty;
      if (key != null)
      {
        MpVer = key.GetValue("VersionMajor") + ".";
        MpVer += key.GetValue("VersionMinor") + ".";
        MpVer += key.GetValue("VersionRevision");
        MpBuild = key.GetValue("VersionBuild").ToString();
        key.Close();
      }
      if (MpVer != Utils.GetPackageVersion(false) || MpBuild != "0")
      {
        rbUpdate.Enabled = false;
        bUpdate.Enabled = false;
      }

      labelSectionHeader.Text = MpBuild != "0" ? String.Format(Localizer.GetBestTranslation("Upgrade_labelSectionHeader_SVN"), MpVer, MpBuild) : String.Format(Localizer.GetBestTranslation("Upgrade_labelSectionHeader"), MpVer);
      rbUpdate.Text = String.Format(Localizer.GetBestTranslation("Upgrade_yes"), Utils.GetPackageVersion(true));
      rbFresh.Text = Localizer.GetBestTranslation("Upgrade_no");
      labelNote.Text = Localizer.GetBestTranslation("Upgrade_note");
    }

    public override DeployDialog GetNextDialog()
    {
      if (rbFreshChecked)
      {
        // Normal deploy...
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.WatchTV);
      }
      // Direct to upgrade
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    private void bUpdate_Click(object sender, EventArgs e)
    {
      bUpdate.Image = Images.Choose_button_on;
      bFresh.Image = Images.Choose_button_off;
      rbFreshChecked = false;
      InstallationProperties.Instance.Set("UpdateMode", "yes");

      CheckResult resultTvServer = Utils.CheckNSISUninstallString("MediaPortal TV Server", "MementoSection_SecServer");
      CheckResult resultTvClient = Utils.CheckNSISUninstallString("Mediaportal Tv Server", "MementoSection_SecClient");

      bool TvServer = resultTvServer.state != CheckState.NOT_INSTALLED;
      bool TvClient = resultTvClient.state != CheckState.NOT_INSTALLED;

      if (!TvServer && !TvClient) InstallationProperties.Instance.Set("InstallType", "mp_only");
      if (!TvServer && TvClient) InstallationProperties.Instance.Set("InstallType", "client");
      if (TvServer && !TvClient) InstallationProperties.Instance.Set("InstallType", "tvserver_master");
      if (TvServer && TvClient) InstallationProperties.Instance.Set("InstallType", "singleseat");

    }

    private void bFresh_Click(object sender, EventArgs e)
    {
      bUpdate.Image = Images.Choose_button_off;
      bFresh.Image = Images.Choose_button_on;
      rbFreshChecked = true;
      InstallationProperties.Instance.Set("UpdateMode", "no");
    }

  }
}