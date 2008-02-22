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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Specialized;

namespace MediaPortal.DeployTool
{
  public partial class TvServerSettingsDlg : DeployDialog, IDeployDialog
  {
    public TvServerSettingsDlg()
    {
      InitializeComponent();
      type=DialogType.TvServerSettings;
      textBoxDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal TV Server";
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.Instance.GetString("TvServerSettings_labelHeading");
      labelInstDir.Text = Localizer.Instance.GetString("TvServerSettings_labelInstDir");
      checkBoxFirewall.Text = Localizer.Instance.GetString("TvServerSettings_checkBoxFirewall");
      buttonBrowse.Text = Localizer.Instance.GetString("TvServerSettings_buttonBrowse");
    }
    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }
    public override bool SettingsValid()
    {
      if (!Utils.CheckTargetDir(textBoxDir.Text))
      {
        Utils.ErrorDlg(Localizer.Instance.GetString("TvServerSettings_errInvalidPath"));
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
      dlg.Description = Localizer.Instance.GetString("TvServerSettings_msgSelectDir");
      dlg.SelectedPath = textBoxDir.Text;
      if (dlg.ShowDialog() == DialogResult.OK)
        textBoxDir.Text = dlg.SelectedPath;
    }
  }
}
