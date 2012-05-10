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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using SetupTv;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class WinTvCiConfig : SectionSettings
  {
    // A private class for use with the tuner selection combo.
    private class TunerInfo
    {
      private Card _tuner = null;

      private TunerInfo()
      {
      }

      public TunerInfo(Card tuner)
      {
        _tuner = tuner;
      }

      public Card Tuner
      {
        get
        {
          return _tuner;
        }
      }

      public override string ToString()
      {
        if (_tuner != null)
        {
          return _tuner.Name;
        }
 	      return base.ToString();
      }
    }

    private bool _isWinTvCiPresent = false;
    private bool _isBdaDriverInstalled = false;

    public WinTvCiConfig()
      : this("WinTV-CI")
    {
    }

    public WinTvCiConfig(string name)
      : base(name)
    {
      InitializeComponent();

      // Check whether the WinTV-CI is installed in the system. The section can only be
      // activated if it is installed.
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      foreach (DsDevice device in captureDevices)
      {
        if (device.Name != null && device.Name.ToLowerInvariant().StartsWith("wintvci"))
        {
          _isWinTvCiPresent = true;
          if (device.Name.ToLowerInvariant().Equals("wintvciusbbda source"))
          {
            _isBdaDriverInstalled = true;
          }
          break;
        }
      }
    }

    public override void SaveSettings()
    {
      TunerInfo selectedTuner = (TunerInfo)tunerSelectionCombo.SelectedItem;
      if (_isBdaDriverInstalled && selectedTuner != null)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Setting setting = layer.GetSetting("winTvCiTuner", "-1");
        setting.Value = selectedTuner.Tuner.IdCard.ToString();
        setting.Persist();
      }
      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      // Read the tuner selection setting from the database.
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("winTvCiTuner", "-1");

      // Load the list of tuners into the tuner selection field.
      tunerSelectionCombo.Items.Clear();
      tunerSelectionCombo.SelectedIndex = -1;
      IList<Card> dbTuners = Card.ListAll();
      foreach (Card tuner in dbTuners)
      {
        TunerInfo info = new TunerInfo(tuner);
        tunerSelectionCombo.Items.Add(info);
        if (tuner.IdCard.ToString().Equals(setting.Value))
        {
          tunerSelectionCombo.SelectedItem = info;
        }
      }

      // Enable/disable the tuner selection field and set the install state label text and colour.
      tunerSelectionCombo.Enabled = _isWinTvCiPresent && _isBdaDriverInstalled;
      if (_isWinTvCiPresent)
      {
        if (_isBdaDriverInstalled)
        {
          installStateLabel.Text = "The WinTV-CI is installed correctly.";
          installStateLabel.ForeColor = Color.ForestGreen;
        }
        else
        {
          installStateLabel.Text = "The WinTV-CI is installed with the WDM driver.";
          installStateLabel.ForeColor = Color.Red;
        }
      }
      else
      {
        installStateLabel.Text = "The WinTV-CI is not detected.";
        installStateLabel.ForeColor = Color.Red;
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override bool CanActivate
    {
      get
      {
        // The section can only be activated if the WinTV-CI is installed.
        return _isWinTvCiPresent;
      }
    }
  }
}
