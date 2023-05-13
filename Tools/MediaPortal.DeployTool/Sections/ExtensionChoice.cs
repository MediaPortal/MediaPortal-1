#region Copyright (C) 2005-2023 Team MediaPortal

// Copyright (C) 2005-2023 Team MediaPortal
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

using System.Windows.Forms;

using MediaPortal.DeployTool.InstallationChecks;

namespace MediaPortal.DeployTool.Sections
{
  public partial class ExtensionChoice : DeployDialog
  {

    public ExtensionChoice()
    {
      InitializeComponent();
      ExtensionInstalledCheck();
      type = DialogType.ExtensionChoice;
      labelSectionHeader.Text = string.Empty;

      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      linkExtensions.Text = Localizer.GetBestTranslation("ExtensionChoice_OtherExtensions");
      lblRecommended.Text = Localizer.GetBestTranslation("ExtensionChoice_Title");

      lblLAV.Text = Localizer.GetBestTranslation("ExtensionChoice_LAV");
      linkLAV.Text = Localizer.GetBestTranslation("ExtensionChoice_MoreInfo");

      lblTitan.Text = Localizer.GetBestTranslation("ExtensionChoice_Titan");
      linkTitan.Text = Localizer.GetBestTranslation("ExtensionChoice_MoreInfo");
    }

    public void ExtensionInstalledCheck()
    {
      IInstallationPackage package = new LAVFilterMPEInstall();
      CheckResult result = package.CheckStatus();
      if (result.state == CheckState.INSTALLED)
      {
        this.chkLAV.Checked = false;
      }

      package = new TitanExtensionInstall();
      result = package.CheckStatus();
      if (result.state == CheckState.INSTALLED)
      {
        this.chkTitan.Checked = false;
      }
    }

    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("ConfigureMediaPortalLAV", chkLAV.Checked ? "1" : "0");
      InstallationProperties.Instance.Set("ConfigureMediaPortalTitanExtended", chkTitan.Checked ? "1" : "0");
    }

    #endregion

    #region Hyperlink handler

    private void linkExtensions_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Utils.OpenURL("https://www.team-mediaportal.com/extensions");
    }

    private void linkLAV_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Utils.OpenURL("https://www.team-mediaportal.com/wiki/display/MediaPortal1/LAV-Filters");
    }

    private void linkTitan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Utils.OpenURL("https://www.team-mediaportal.com/wiki/display/MediaPortal1/Titan+Extended");
    }

    #endregion

  }
}
