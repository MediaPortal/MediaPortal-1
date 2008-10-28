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
  public partial class TvEngineTypeDlg : DeployDialog
  {
    bool tve3Checked;
    public TvEngineTypeDlg()
    {
      InitializeComponent();
      type = DialogType.TvEngineType;
      labelSectionHeader.Text = "";
      bTVE3.Image = Images.Choose_button_on;
      tve3Checked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Utils.GetBestTranslation("TvEngineType_labelSectionHeader");
      rbTV2.Text = Utils.GetBestTranslation("TvEngineType_rbTvBuildIn");
      rbTV3.Text = Utils.GetBestTranslation("TvEngineType_rbTvEngine");
      labelTV3.Text = Utils.GetBestTranslation("TvEngineType_labelTvEngine");
    }
    public override DeployDialog GetNextDialog()
    {
      if (tve3Checked)
      {
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE);
      }
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

   
    private void bTVE2_Click(object sender, EventArgs e)
    {
      bTVE2.Image = Images.Choose_button_on;
      bTVE3.Image = Images.Choose_button_off;
      tve3Checked = false;
    }

    private void bTVE3_Click(object sender, EventArgs e)
    {
      bTVE2.Image = Images.Choose_button_off;
      bTVE3.Image = Images.Choose_button_on;
      tve3Checked = true;
    }

  }
}