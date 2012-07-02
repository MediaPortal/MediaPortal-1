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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using MediaPortal.UserInterface.Controls;
using SetupTv;
using SmarDtvUsbCi;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class SmarDtvUsbCiConfig : SectionSettings
  {
    // A private class for use with the tuner selection combos.
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

    private ReadOnlyCollection<SmarDtvUsbCiProduct> _products = null;
    private MPComboBox[] _tunerSelections = null;
    private Label[] _installStateLabels = null;

    public SmarDtvUsbCiConfig()
      : this("SmarDTV USB CI")
    {
    }

    public SmarDtvUsbCiConfig(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void SaveSettings()
    {
      for (int i = 0; i < _products.Count; i++)
      {
        TunerInfo selectedTuner = (TunerInfo)_tunerSelections[i].SelectedItem;
        if (_tunerSelections[i].Enabled && selectedTuner != null)
        {
          TvBusinessLayer layer = new TvBusinessLayer();
          Setting setting = layer.GetSetting(_products[i].DbSettingName, "-1");
          setting.Value = selectedTuner.Tuner.IdCard.ToString();
          setting.Persist();
        }
      }
      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      IList<Card> dbTuners = Card.ListAll();
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);

      TvBusinessLayer layer = new TvBusinessLayer();
      for (int i = 0; i < _products.Count; i++)
      {
        // Populate the tuner selection fields and set current values.
        Setting setting = layer.GetSetting(_products[i].DbSettingName, "-1");
        _tunerSelections[i].Items.Clear();
        _tunerSelections[i].SelectedIndex = -1;

        foreach (Card tuner in dbTuners)
        {
          TunerInfo info = new TunerInfo(tuner);
          _tunerSelections[i].Items.Add(info);
          if (tuner.IdCard.ToString().Equals(setting.Value))
          {
            _tunerSelections[i].SelectedItem = info;
          }
        }

        // Check whether the CI is installed in the system. We disable the selection field if it is
        // not installed.
        bool found = false;
        _tunerSelections[i].Enabled = false;
        foreach (DsDevice device in captureDevices)
        {
          if (device.Name != null)
          {
            if (device.Name.Equals(_products[i].WdmDeviceName))
            {
              _installStateLabels[i].Text = "The " + _products[i].ProductName + " is installed with the WDM driver.";
              _installStateLabels[i].ForeColor = Color.Orange;
              found = true;
              break;
            }
            else if (device.Name.Equals(_products[i].BdaDeviceName))
            {
              _installStateLabels[i].Text = "The " + _products[i].ProductName + " is installed correctly.";
              _installStateLabels[i].ForeColor = Color.ForestGreen;
              _tunerSelections[i].Enabled = true;
              found = true;
              break;
            }
          }
        }
        if (!found)
        {
          _installStateLabels[i].Text = "The " + _products[i].ProductName + " is not detected.";
          _installStateLabels[i].ForeColor = Color.Red;
        }
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
        // The section can always be activated (disabling it might be confusing for people), but we don't
        // necessarily enable all of the tuner selection fields.
        return true;
      }
    }
  }
}
