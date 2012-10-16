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
using System.Drawing;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.SmarDtvUsbCi
{
  public partial class SmarDtvUsbCiConfig : SectionSettings
  {
    private ReadOnlyCollection<SmarDtvUsbCiProduct> _products = null;
    private MPComboBox[] _tunerSelections = null;
    private Label[] _installStateLabels = null;

    private readonly ISettingService _settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;
    private readonly ICardService _cardServiceAgent = ServiceAgents.Instance.CardServiceAgent;
    private readonly IControllerService _controllerServiceAgent = ServiceAgents.Instance.ControllerServiceAgent;

    public SmarDtvUsbCiConfig()
      : this("SmarDTV USB CI")
    {
    }

    public SmarDtvUsbCiConfig(string name)
      : base(name)
    {
      Log.Debug("SmarDTV USB CI config: constructing");
      _products = SmarDtvUsbCiProducts.GetProductList();
      _tunerSelections = new MPComboBox[_products.Count];
      _installStateLabels = new Label[_products.Count];
      InitializeComponent();
      Log.Debug("SmarDTV USB CI config: constructed");
    }

    public override void SaveSettings()
    {
      Log.Debug("SmarDTV USB CI config: saving settings");
      for (int i = 0; i < _products.Count; i++)
      {
        Card selectedTuner = (Card)_tunerSelections[i].SelectedItem;
        if (_tunerSelections[i].Enabled && selectedTuner != null)
        {
          Log.Debug("  {0} linked to tuner {1} ({2})", _products[i].ProductName, selectedTuner.IdCard, selectedTuner.Name);
          _settingServiceAgent.SaveSetting("digitalDevicesCiDevicePath" + i, selectedTuner.IdCard.ToString());

        }
      }
      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      Log.Debug("SmarDTV USB CI config: activated");
      IList<Card> dbTuners = _cardServiceAgent.ListAllCards();
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);

      for (int i = 0; i < _products.Count; i++)
      {
        Log.Debug("SmarDTV USB CI config: product {0}...", _products[i].ProductName);

        // Populate the tuner selection fields and set current values.
        Setting setting = _settingServiceAgent.GetSettingWithDefaultValue(_products[i].DbSettingName, "-1");
        _tunerSelections[i].Items.Clear();
        _tunerSelections[i].SelectedIndex = -1;

        foreach (Card tuner in dbTuners)
        {
          CardType tunerType = _controllerServiceAgent.Type(tuner.IdCard);
          if (tunerType == CardType.Analog || tunerType == CardType.RadioWebStream || tunerType == CardType.Unknown)
          {
            continue;
          }
          _tunerSelections[i].Items.Add(tuner);
          if (tuner.IdCard.ToString().Equals(setting.Value))
          {
            Log.Debug("  currently linked to tuner {0} ({1})", tuner.IdCard, tuner.Name);
            _tunerSelections[i].SelectedItem = tuner;
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
              Log.Debug("  WDM driver installed");
              _installStateLabels[i].Text = "The " + _products[i].ProductName + " is installed with the WDM driver.";
              _installStateLabels[i].ForeColor = Color.Orange;
              found = true;
              break;
            }
            else if (device.Name.Equals(_products[i].BdaDeviceName))
            {
              Log.Debug("  BDA driver installed");
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
          Log.Debug("  driver not installed");
          _installStateLabels[i].Text = "The " + _products[i].ProductName + " is not detected.";
          _installStateLabels[i].ForeColor = Color.Red;
        }
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      Log.Debug("SmarDTV USB CI config: deactivated");
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
