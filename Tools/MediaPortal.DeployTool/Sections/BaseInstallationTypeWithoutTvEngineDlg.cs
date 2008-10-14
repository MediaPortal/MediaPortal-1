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
  public partial class BaseInstallationTypeWithoutTvEngineDlg : DeployDialog
  {
    bool rbOneClickChecked;

    public BaseInstallationTypeWithoutTvEngineDlg()
    {
      InitializeComponent();
      type = DialogType.BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE;
      labelSectionHeader.Text = "";
      imgOneClick.Image = Images.Choose_button_on;
      rbOneClickChecked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelOneClickCaption.Text = Utils.GetBestTranslation("BaseInstallation_labelOneClickCaption");
      labelOneClickDesc.Text = Utils.GetBestTranslation("BaseInstallationNoTvEngine_labelOneClickDesc");
      rbOneClick.Text = Utils.GetBestTranslation("BaseInstallation_rbOneClick");
      labelAdvancedCaption.Text = Utils.GetBestTranslation("BaseInstallation_labelAdvancedCaption");
      labelAdvancedDesc.Text = Utils.GetBestTranslation("BaseInstallationNoTvEngine_labelAdvancedDesc");
      rbAdvanced.Text = Utils.GetBestTranslation("BaseInstallation_rbAdvanced");
    }
    public override DeployDialog GetNextDialog()
    {
      InstallationProperties.Instance.Set("InstallType", "mp_only");
      if (rbOneClickChecked)
      {
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
      }
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.MPSettings);
    }
    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    private void imgOneClick_Click(object sender, EventArgs e)
    {
      imgOneClick.Image = Images.Choose_button_on;
      imgAdvanced.Image = Images.Choose_button_off;
      rbOneClickChecked = true;
    }

    private void rbOneClick_Click(object sender, EventArgs e)
    {
      imgOneClick_Click(sender, e);
    }

    private void imgAdvanced_Click(object sender, EventArgs e)
    {
      imgOneClick.Image = Images.Choose_button_off;
      imgAdvanced.Image = Images.Choose_button_on;
      rbOneClickChecked = false;
    }
    private void rbAdvanced_Click(object sender, EventArgs e)
    {
      imgAdvanced_Click(sender, e);
    }
  }
}