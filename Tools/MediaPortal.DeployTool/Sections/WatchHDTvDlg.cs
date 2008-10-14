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

namespace MediaPortal.DeployTool.Sections
{
  public partial class WatchHDTvDlg : DeployDialog
  {
    bool rbYesChecked;

    public WatchHDTvDlg()
    {
      InitializeComponent();
      type = DialogType.WatchHDTv;
      labelSectionHeader.Text = "";
      imgYesHD.Image = Images.Choose_button_on;
      rbYesChecked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Utils.GetBestTranslation("WatchHDTv_labelSectionHeader");
      rbYesHD.Text = Utils.GetBestTranslation("WatchHDTv_yes");
      rbNoHD.Text = Utils.GetBestTranslation("WatchHDTv_no");
      rbMaybeHD.Text = Utils.GetBestTranslation("WatchHDTv_maybe");
    }
    public override DeployDialog GetNextDialog()
    {
      if (rbYesChecked)
      {
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE);
      }
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.TvEngineType);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    private void imgYesHD_Click(object sender, EventArgs e)
    {
      imgYesHD.Image = Images.Choose_button_on;
      imgNoHD.Image = Images.Choose_button_off;
      imgMaybeHD.Image = Images.Choose_button_off;
      rbYesChecked = true;
    }

    private void imgNoHD_Click(object sender, EventArgs e)
    {
      imgYesHD.Image = Images.Choose_button_off;
      imgNoHD.Image = Images.Choose_button_on;
      imgMaybeHD.Image = Images.Choose_button_off;
      rbYesChecked = false;
    }

    private void imgMaybeHD_Click(object sender, EventArgs e)
    {
      imgYesHD.Image = Images.Choose_button_off;
      imgNoHD.Image = Images.Choose_button_off;
      imgMaybeHD.Image = Images.Choose_button_on;
      rbYesChecked = false;
    }
  }
}