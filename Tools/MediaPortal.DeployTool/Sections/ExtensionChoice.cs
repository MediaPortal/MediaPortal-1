#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class ExtensionChoice : DeployDialog
  {

    public ExtensionChoice()
    {
      InitializeComponent();
      type = DialogType.ExtensionChoice;
      labelSectionHeader.Text = "";

      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      //chkLAV.Text = Localizer.GetBestTranslation("MPSettings_checkBoxLAV");
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
      InstallationProperties.Instance.Set("ConfigureMediaPortalTitanExt", chkTitan.Checked ? "Yes" : "No");
      InstallationProperties.Instance.Set("ConfigureMediaPortalLAV", chkLAV.Checked ? "1" : "0");
    }

    #endregion

    #region Hyperlink handler

    private static void OpenURL(string url)
    {
      try
      {
        System.Diagnostics.Process.Start(url);
      }
      catch (System.Exception) { }
    }

    private void linkExtensions_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenURL("http://www.team-mediaportal.com/extensions");
    }

    private void linkLAV_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenURL("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/17_Extensions/3_Plugins/IMDb");
    }

    private void linkTitan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenURL("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/17_Extensions/4_Skins/Avalon");
    }

    #endregion

  }
}
