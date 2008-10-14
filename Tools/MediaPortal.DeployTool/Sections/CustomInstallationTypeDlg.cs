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
  public partial class CustomInstallationTypeDlg : DeployDialog
  {
    int installType;
    public CustomInstallationTypeDlg()
    {
      InitializeComponent();
      type = DialogType.CUSTOM_INSTALLATION_TYPE;
      imgSingle.Image = Images.Choose_button_on;
      installType = 1;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Utils.GetBestTranslation("CustomInstallation_labelSectionHeader");
      rbSingleSeat.Text = Utils.GetBestTranslation("CustomInstallation_rbSingleSeat");
      labelSingleSeat.Text = Utils.GetBestTranslation("CustomInstallation_labelSingleSeat");
      rbTvServerMaster.Text = Utils.GetBestTranslation("CustomInstallation_rbTvServerMaster");
      labelMaster.Text = Utils.GetBestTranslation("CustomInstallation_labelMaster");
      rbClient.Text = Utils.GetBestTranslation("CustomInstallation_rbClient");
      labelClient.Text = Utils.GetBestTranslation("CustomInstallation_labelClient");
    }
    public override DeployDialog GetNextDialog()
    {
      switch (installType)
      {
        case 1:
          return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DBMSType);
        case 2:
          return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DBMSType);
        case 3:
          return DialogFlowHandler.Instance.GetDialogInstance(DialogType.MPSettings);
        default:
          return null;
      }
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      switch (installType)
      {
        case 1:
          InstallationProperties.Instance.Set("InstallTypeHeader", rbSingleSeat.Text);
          InstallationProperties.Instance.Set("InstallType", "singleseat");
          break;
        case 2:
          InstallationProperties.Instance.Set("InstallTypeHeader", rbTvServerMaster.Text);
          InstallationProperties.Instance.Set("InstallType", "tvserver_master");
          break;
        case 3:
          InstallationProperties.Instance.Set("InstallTypeHeader", rbClient.Text);
          InstallationProperties.Instance.Set("InstallType", "client");
          break;
      }
    }
    #endregion

    private void imgSingle_Click(object sender, EventArgs e)
    {
      imgSingle.Image = Images.Choose_button_on;
      imgMaster.Image = Images.Choose_button_off;
      imgClient.Image = Images.Choose_button_off;
      installType = 1;
    }

    private void imgMaster_Click(object sender, EventArgs e)
    {
      imgSingle.Image = Images.Choose_button_off;
      imgMaster.Image = Images.Choose_button_on;
      imgClient.Image = Images.Choose_button_off;
      installType = 2;
    }

    private void imgClient_Click(object sender, EventArgs e)
    {
      imgSingle.Image = Images.Choose_button_off;
      imgMaster.Image = Images.Choose_button_off;
      imgClient.Image = Images.Choose_button_on;
      installType = 3;
    }
  }
}