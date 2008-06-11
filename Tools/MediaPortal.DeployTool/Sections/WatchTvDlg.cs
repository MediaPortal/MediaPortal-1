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
  public partial class WatchTVDlg : DeployDialog, IDeployDialog
  {
    bool rbYesChecked;

    public WatchTVDlg()
    {
      // First install screen: check if OS is compliant !!!
      Utils.CheckPrerequisites();

      InitializeComponent();
      type = DialogType.WatchTV;
      labelSectionHeader.Text = "";
      imgYes.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      rbYesChecked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Localizer.Instance.GetString("WatchTV_labelSectionHeader");
      rbYesWatchTv.Text = Localizer.Instance.GetString("WatchTV_on");
      rbNoWatchTv.Text = Localizer.Instance.GetString("WatchTV_off");
    }

    public override DeployDialog GetNextDialog()
    {
      if (rbYesChecked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.WatchHDTv);
      else
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    private void imgYes_Click(object sender, EventArgs e)
    {
      imgYes.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      imgNo.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      rbYesChecked = true;
    }

    private void imgNo_Click(object sender, EventArgs e)
    {
      imgYes.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      imgNo.Image = global::MediaPortal.DeployTool.Images.Choose_button_on;
      rbYesChecked = false;
    }
  }
}
