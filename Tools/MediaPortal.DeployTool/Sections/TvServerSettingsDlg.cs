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
  public partial class TvServerSettingsDlg : DeployDialog
  {
    public TvServerSettingsDlg()
    {
      InitializeComponent();
      type = DialogType.TvServerSettings;
      textBoxDir.Text =
        installationPath =
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal TV Server";
      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.GetBestTranslation("TvServerSettings_labelHeading");
      labelInstDir.Text = Localizer.GetBestTranslation("TvServerSettings_labelInstDir");
      checkBoxFirewall.Text = Localizer.GetBestTranslation("TvServerSettings_checkBoxFirewall");
      buttonBrowse.Text = Localizer.GetBestTranslation("MainWindow_browseButton");
    }

    public override DeployDialog GetNextDialog()
    {
      if (InstallationProperties.Instance.Get("InstallType") != "tvserver_master")
      {
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.SkinChoice);  
      }
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }

    public override bool SettingsValid()
    {
      if (!Utils.CheckTargetDir(textBoxDir.Text))
      {
        Utils.ErrorDlg(Localizer.GetBestTranslation("TvServerSettings_errInvalidPath"));
        return false;
      }
      return true;
    }

    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("TVServerDir", textBoxDir.Text);
      if (checkBoxFirewall.Checked)
        InstallationProperties.Instance.Set("ConfigureTVServerFirewall", "1");
      else
        InstallationProperties.Instance.Set("ConfigureTVServerFirewall", "0");
    }

    #endregion

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.Description = Localizer.GetBestTranslation("TvServerSettings_msgSelectDir");
      dlg.SelectedPath = textBoxDir.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxDir.Text = Path.Combine(dlg.SelectedPath, "MediaPortal TV Server");
      }
    }

    private void textBoxDir_TextChanged(object sender, EventArgs e)
    {
      installationPath = textBoxDir.Text;
    }
  }
}