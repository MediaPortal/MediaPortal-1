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
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class MPSettingsDlg : DeployDialog
  {
    public MPSettingsDlg()
    {
      InitializeComponent();
      type = DialogType.MPSettings;
      textBoxDir.Text = installationPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal";
      checkBoxFirewall.Text = Localizer.GetBestTranslation("MPSettings_checkBoxFirewall");
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
    }
    #endregion

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.Description = Localizer.GetBestTranslation("MPSettings_msgSelectDir");
      dlg.SelectedPath = textBoxDir.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        if (dlg.SelectedPath.EndsWith("\\"))
          textBoxDir.Text = installationPath = dlg.SelectedPath + "MediaPortal";
        else
          textBoxDir.Text = installationPath = dlg.SelectedPath + "\\MediaPortal";
      }
    }

    private void MPSettingsDlg_Load(object sender, EventArgs e)
    {

    }
  }
}