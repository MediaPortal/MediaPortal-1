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
using System.Collections.Specialized;
using System.Resources;

namespace MediaPortal.DeployTool
{
  public partial class DownloadOnlyDlg : DeployDialog, IDeployDialog
  {
    bool rbDownloadOnlyChecked;

    public DownloadOnlyDlg()
    {
      InitializeComponent();
      type = DialogType.DownloadOnly;
      labelSectionHeader.Text = "";
      imgInstallNow.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      rbDownloadOnlyChecked = false;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Utils.GetBestTranslation("DownloadOnly_labelSectionHeader");
      rbDownloadOnly.Text = Utils.GetBestTranslation("DownloadOnly_no");
      rbInstallNow.Text = Utils.GetBestTranslation("DownloadOnly_yes");
    }
    public override DeployDialog GetNextDialog()
    {
      if (rbDownloadOnlyChecked)
      {
        InstallationProperties.Instance.Set("InstallType", "download_only");
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
      }
      else
      {
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.WatchTV);
      }
    }
    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    private void imgInstallNow_Click(object sender, EventArgs e)
    {
      imgInstallNow.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      imgDownloadOnly.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      rbDownloadOnlyChecked = false;
    }

    private void imgDownloadOnly_Click(object sender, EventArgs e)
    {
      imgInstallNow.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgDownloadOnly.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      rbDownloadOnlyChecked = true;
    }

      private void labelSectionHeader_Click(object sender, EventArgs e)
      {

      }


  }
}
