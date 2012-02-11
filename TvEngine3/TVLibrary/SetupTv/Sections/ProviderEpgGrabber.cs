#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Windows.Forms;
using TvControl;
using Gentle.Framework;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using MediaPortal.UserInterface.Controls;
using SetupTv.Sections.Dialogs;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Helper.Providers;

namespace SetupTv.Sections
{
  public partial class ProviderEPGs : SectionSettings
  {

    public ProviderEPGs()
      : base("Provider Epgs") 
    {
        InitializeComponent();
    }

    public override void LoadSettings()
    {
      LoadSkyUKSettings();
      LoadSkyItalySettings();

      base.LoadSettings();
    }

    /// <summary>
    /// Loads the settings fo Sky UK
    /// </summary>
    private void LoadSkyUKSettings()
    {
      lbSkyUKFrequencyValue.Text = SkyUK.Instance.DefaultTransponderFrequency.ToString();
      lbSkyUKPolarisationValue.Text = SkyUK.Instance.DefaultTransponderPolarisationString;
      lbSkyUKSymbolRateValue.Text = SkyUK.Instance.DefaultTransponderSymbolRate.ToString();
      lbSkyUKFECValue.Text = SkyUK.Instance.DefaultTransponderInnerFECString;

      lbSkyUKNetworkIdValue.Text = "0x" + SkyUK.Instance.NetworkId.ToString("X");

      cbGrabSkyUKEpg.Checked = SkyUK.Instance.EPGGrabbingEnabled;
    }

    /// <summary>
    /// Loads the settings for Sky Italy
    /// </summary>
    private void LoadSkyItalySettings()
    {
      lbSkyItalyFrequencyValue.Text = SkyItaly.Instance.DefaultTransponderFrequency.ToString();
      lbSkyItalyPolarisationValue.Text = SkyItaly.Instance.DefaultTransponderPolarisationString;
      lbSkyItalySymbolRateValue.Text = SkyItaly.Instance.DefaultTransponderSymbolRate.ToString();
      lbSkyItalyFECValue.Text = SkyItaly.Instance.DefaultTransponderInnerFECString;

      lbSkyItalyNetworkIdValue.Text = "0x" + SkyItaly.Instance.NetworkId.ToString("X");

      cbGrabSkyItalyEpg.Checked = SkyItaly.Instance.EPGGrabbingEnabled;
    }

    public override void SaveSettings()
    {
      SkyUK.Instance.EPGGrabbingEnabled = cbGrabSkyUKEpg.Checked;
      SkyItaly.Instance.EPGGrabbingEnabled = cbGrabSkyItalyEpg.Checked;

      base.SaveSettings();
    }

    /// <summary>
    /// Fired when the edit default sky uk transponder button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btEditDefaultSkyUKTransponder_Click(object sender, EventArgs e)
    {
      SkyDefaultTransponderSetup defaultTransponderSetup = new SkyDefaultTransponderSetup(SkyDefaultTransponderSetup.eSkyCounntry.UK);
      DialogResult result = defaultTransponderSetup.ShowDialog();

      //  If ok was clicked, reload the sky uk settings
      if (result == DialogResult.OK)
        LoadSkyUKSettings();
    }

    /// <summary>
    /// Fired when the reset sky uk default transponder settings button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btResetDefaultSkyUKTransponder_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("This will reset the parameters for the default transponder.\r\n\r\nAre you sure?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        SkyUK.Instance.ResetDefaultTransponderParameters();
        LoadSkyUKSettings();
      }
    }

    /// <summary>
    /// Fired when the edit Sky Italy default transponder parameters button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btEditSkyItalyDefaultTransponder_Click(object sender, EventArgs e)
    {
      SkyDefaultTransponderSetup defaultTransponderSetup = new SkyDefaultTransponderSetup(SkyDefaultTransponderSetup.eSkyCounntry.Italy);
      DialogResult result = defaultTransponderSetup.ShowDialog();

      //  If ok was clicked, reload the sky uk settings
      if (result == DialogResult.OK)
        LoadSkyItalySettings();
    }

    /// <summary>
    /// Fired when the reset Sky Italy default transponder parameters button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btResetSkyItalyDefaultTransponder_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("This will reset the parameters for the default transponder.\r\n\r\nAre you sure?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        SkyItaly.Instance.ResetDefaultTransponderParameters();
        LoadSkyItalySettings();
      }
    }


  }
}
