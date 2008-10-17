#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
  public partial class TvServerSettingsDlg : DeployDialog
  {
    public TvServerSettingsDlg()
    {
      InitializeComponent();
      type = DialogType.TvServerSettings;
      textBoxDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal TV Server";
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Utils.GetBestTranslation("TvServerSettings_labelHeading");
      labelInstDir.Text = Utils.GetBestTranslation("TvServerSettings_labelInstDir");
      checkBoxFirewall.Text = Utils.GetBestTranslation("TvServerSettings_checkBoxFirewall");
      buttonBrowse.Text = Utils.GetBestTranslation("MainWindow_browseButton");
    }
    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }
    public override bool SettingsValid()
    {
      if (!Utils.CheckTargetDir(textBoxDir.Text))
      {
        Utils.ErrorDlg(Utils.GetBestTranslation("TvServerSettings_errInvalidPath"));
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
      dlg.Description = Utils.GetBestTranslation("TvServerSettings_msgSelectDir");
      dlg.SelectedPath = textBoxDir.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        if (dlg.SelectedPath.EndsWith("\\"))
          textBoxDir.Text = dlg.SelectedPath + "MediaPortal TV Server";
        else
          textBoxDir.Text = dlg.SelectedPath + "\\MediaPortal TV Server";
      }
    }
  }
}