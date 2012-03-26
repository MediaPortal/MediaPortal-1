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
using System.IO;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class MPSettingsDlg : DeployDialog
  {
    public MPSettingsDlg()
    {
      InitializeComponent();
      type = DialogType.MPSettings;
      textBoxDir.Text =
        installationPath =
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal";
      checkBoxFirewall.Text = Localizer.GetBestTranslation("MPSettings_checkBoxFirewall");
	  checkBoxLAV.Text = Localizer.GetBestTranslation("MPSettings_checkBoxLAV");
      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.GetBestTranslation("MPSettings_labelHeading");
      labelInstDir.Text = Localizer.GetBestTranslation("MPSettings_labelInstDir");
      buttonBrowse.Text = Localizer.GetBestTranslation("MainWindow_browseButton");
    }

    public override DeployDialog GetNextDialog()
    {
      if (InstallationProperties.Instance["InstallType"] == "client" ||
          InstallationProperties.Instance["InstallType"] == "mp_only")
      {
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
      }
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.TvServerSettings);
    }

    public override bool SettingsValid()
    {
      if (!Utils.CheckTargetDir(textBoxDir.Text))
      {
        Utils.ErrorDlg(Localizer.GetBestTranslation("MPSettings_errInvalidPath"));
        return false;
      }
      return true;
    }

    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("MPDir", textBoxDir.Text);
      if (checkBoxFirewall.Checked)
        InstallationProperties.Instance.Set("ConfigureMediaPortalFirewall", "1");
      else
        InstallationProperties.Instance.Set("ConfigureMediaPortalFirewall", "0");
	  if (checkBoxLAV.Checked)
		  InstallationProperties.Instance.Set("ConfigureMediaPortalLAV", "1");
	  else
		  InstallationProperties.Instance.Set("ConfigureMediaPortalLAV", "0");
    }

    #endregion

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.Description = Localizer.GetBestTranslation("MPSettings_msgSelectDir");
      dlg.SelectedPath = textBoxDir.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxDir.Text = Path.Combine(dlg.SelectedPath, "MediaPortal");
      }
    }

    private void textBoxDir_TextChanged(object sender, EventArgs e)
    {
      installationPath = textBoxDir.Text;
    }
  }
}