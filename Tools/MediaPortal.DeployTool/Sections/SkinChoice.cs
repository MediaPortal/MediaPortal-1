﻿#region Copyright (C) 2005-2023 Team MediaPortal

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

using System;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class SkinChoice : DeployDialog
  {

    public SkinChoice()
    {
      InitializeComponent();
      type = DialogType.SkinChoice;
      labelSectionHeader.Text = string.Empty;

      InstallationProperties.Instance.Set("ConfigureMediaPortalAresSkin", "0");

      if (InstallationProperties.Instance.Get("UpdateMode") == "yes")
      {
        lblExisting.Text = Localizer.GetBestTranslation("SkinChoice_labelExistingSkin");
        btnExisting.Visible = true;
        lblExisting.Visible = true;
        btnExisting.Image = Images.Choose_button_on;
        pbSkin.Image = Images.preview_existing;
        InstallationProperties.Instance.Set("ChosenSkin", "[Existing]");
      }
      else
      {
        float screenHeight = Screen.PrimaryScreen.Bounds.Height;
        float screenWidth = Screen.PrimaryScreen.Bounds.Width;
        float screenRatio = (screenWidth / screenHeight);
        bool widescreen = screenRatio > 1.5;
        if (widescreen)
        { // default to Titan
          btnTitan.Image = Images.Choose_button_on;
          pbSkin.Image = Images.preview_titan;
          InstallationProperties.Instance.Set("ChosenSkin", "Titan");
        }
      }

      UpdateUI();
    }

    #region Hyperlink handler

    private void linkSkins_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Utils.OpenURL("https://www.team-mediaportal.com/extensions/widescreen");
    }

    #endregion

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      lblChooseSkin.Text = Localizer.GetBestTranslation("SkinChoice_labelSectionHeader");
      lblExtendedDescription.Text = Localizer.GetBestTranslation("SkinChoice_ExtendedDescription");
      linkSkins.Text = Localizer.GetBestTranslation("SkinChoice_OtherSkins");
    }

    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.ExtensionChoice);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    private void btnSkin1_Click(object sender, EventArgs e)
    {
      btnExisting.Image = Images.Choose_button_off;
      btnTitan.Image = Images.Choose_button_on;
      btnDefaultWide.Image = Images.Choose_button_off;
      btnAres.Image = Images.Choose_button_off;
      pbSkin.Image = Images.preview_titan;
      InstallationProperties.Instance.Set("ChosenSkin", "Titan");
      InstallationProperties.Instance.Set("ConfigureMediaPortalAresSkin", "0");
    }

    private void btnSkin2_Click(object sender, EventArgs e)
    {
      btnExisting.Image = Images.Choose_button_off;
      btnTitan.Image = Images.Choose_button_off;
      btnDefaultWide.Image = Images.Choose_button_on;
      btnAres.Image = Images.Choose_button_off;
      pbSkin.Image = Images.preview_DefaultWide_HD;
      InstallationProperties.Instance.Set("ChosenSkin", "DefaultWideHD");
      InstallationProperties.Instance.Set("ConfigureMediaPortalAresSkin", "0");
    }

    private void btnSkin3_Click(object sender, EventArgs e)
    {
      btnExisting.Image = Images.Choose_button_off;
      btnTitan.Image = Images.Choose_button_off;
      btnDefaultWide.Image = Images.Choose_button_off;
      btnAres.Image = Images.Choose_button_on;
      pbSkin.Image = Images.preview_ares;
      InstallationProperties.Instance.Set("ChosenSkin", "Ares");
      InstallationProperties.Instance.Set("ConfigureMediaPortalAresSkin", "1");
    }

    private void btnExisting_Click(object sender, EventArgs e)
    {
      btnExisting.Image = Images.Choose_button_on;
      btnTitan.Image = Images.Choose_button_off;
      btnDefaultWide.Image = Images.Choose_button_off;
      btnAres.Image = Images.Choose_button_off;
      pbSkin.Image = Images.preview_existing;
      InstallationProperties.Instance.Set("ChosenSkin", "[Existing]");
      InstallationProperties.Instance.Set("ConfigureMediaPortalAresSkin", "0");
    }

  }
}
