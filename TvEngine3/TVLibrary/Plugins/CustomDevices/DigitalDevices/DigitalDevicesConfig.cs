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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DigitalDevices;
using DirectShowLib;
using MediaPortal.UserInterface.Controls;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class DigitalDevicesConfig : SectionSettings
  {
    private List<DigitalDevicesCiSlot> _ciSlots = null;
    private NumericUpDown[] _decryptLimits = null;
    private MPTextBox[] _providerLists = null;
    private bool _isFirstActivation = true;

    public DigitalDevicesConfig()
      : this("Digital Devices CI")
    {
    }

    public DigitalDevicesConfig(string name)
      : base("Digital Devices CI")
    {
      Log.Debug("Digital Devices config: constructing");

      // Get the details for the slots we've seen in the past.
      List<DigitalDevicesCiSlot> dbSlots = DigitalDevicesCiSlots.GetDatabaseSettings();

      // Now read the details for the currently available slots and merge settings from the DB
      // to fill the _ciSlots list.
      _ciSlots = new List<DigitalDevicesCiSlot>();
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      Guid baseFilterGuid = typeof(IBaseFilter).GUID;
      foreach (DsDevice device in captureDevices)
      {
        if (!DigitalDevicesCiSlots.IsDigitalDevicesCiDevice(device))
        {
          continue;
        }
        Log.Debug("Digital Devices config: device {0} ({1})...", device.Name, device.DevicePath);
        DigitalDevicesCiSlot slot = new DigitalDevicesCiSlot();
        bool found = false;
        foreach (DigitalDevicesCiSlot s in dbSlots)
        {
          if (device.DevicePath.Equals(s.DevicePath))
          {
            Log.Debug("  found existing configuration");
            found = true;
            slot = s;
            break;
          }
        }
        if (!found)
        {
          Log.Debug("  new configuration");
          slot.DevicePath = device.DevicePath;
          slot.DecryptLimit = 0;
          slot.Providers = new HashSet<String>();
        }
        Log.Debug("  decrypt limit  = {0}", slot.DecryptLimit);
        String[] providerList = new String[slot.Providers.Count];
        slot.Providers.CopyTo(providerList);
        Log.Debug("  provider list  = {0}", String.Join(", ", providerList));
        slot.DeviceName = device.Name;
        slot.CamRootMenuTitle = "(empty)";

        // If possible, read the root menu title for the CAM in the slot.
        object obj = null;
        try
        {
          device.Mon.BindToObject(null, null, ref baseFilterGuid, out obj);
        }
        catch (Exception)
        {
        }
        IBaseFilter ciFilter = obj as IBaseFilter;
        if (ciFilter != null)
        {
          String menuTitle;
          int hr = DigitalDevicesCiSlots.GetMenuTitle(ciFilter, out menuTitle);
          if (hr == 0)
          {
            slot.CamRootMenuTitle = menuTitle;
          }
          DsUtils.ReleaseComObject(ciFilter);
          ciFilter = null;
          obj = null;
        }
        Log.Debug("  CAM name/title = {0}", slot.CamRootMenuTitle);

        _ciSlots.Add(slot);
      }
      _ciSlots.Sort(
        delegate(DigitalDevicesCiSlot slot1, DigitalDevicesCiSlot slot2)
        {
          return slot1.DeviceName.CompareTo(slot2.DeviceName);
        }
      );

      _decryptLimits = new NumericUpDown[_ciSlots.Count];
      _providerLists = new MPTextBox[_ciSlots.Count];

      // Now that we have the details, we can build the user interface.
      InitializeComponent();
      Log.Debug("Digital Devices config: constructed, slot count = {0}", _ciSlots.Count);
    }

    public override void SaveSettings()
    {
      Log.Debug("Digital Devices config: saving settings, slot count = {0}", _ciSlots.Count);
      TvBusinessLayer layer = new TvBusinessLayer();
      byte i = 0;
      foreach (DigitalDevicesCiSlot slot in _ciSlots)
      {
        Log.Debug("Digital Devices config: slot {0}...", slot.CamRootMenuTitle);

        // Persist the slot related settings. The other struct properties are dynamic.
        Setting deviceName = layer.GetSetting("digitalDevicesCiDeviceName" + i, String.Empty);
        deviceName.Value = slot.DeviceName;
        deviceName.Persist();
        Log.Debug("  device name   = {0}", deviceName.Value);

        Setting devicePath = layer.GetSetting("digitalDevicesCiDevicePath" + i, String.Empty);
        devicePath.Value = slot.DevicePath;
        devicePath.Persist();
        Log.Debug("  device path   = {0}", devicePath.Value);

        Setting decryptLimit = layer.GetSetting("digitalDevicesCiDecryptLimit" + i, "0");
        decryptLimit.Value = _decryptLimits[i].Value.ToString();
        decryptLimit.Persist();
        Log.Debug("  decrypt limit = {0}", decryptLimit.Value);

        Setting providerList = layer.GetSetting("digitalDevicesCiProviderList" + i, String.Empty);
        // (Convert comma-separated list in the UI to pipe separated for the DB, removing extra spaces).
        providerList.Value = String.Join("|", Regex.Split(_providerLists[i].Text.Trim(), @"\s*,\s*"));
        providerList.Persist();
        Log.Debug("  provider list = {0}", providerList.Value);

        i++;
      }

      // Invalidate any other settings in the DB.
      Setting nextDevicePath = layer.GetSetting("digitalDevicesCiDevicePath" + i, String.Empty);
      nextDevicePath.Value = String.Empty;
      nextDevicePath.Persist();

      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      Log.Debug("Digital Devices config: activated");

      // On first load in the constructor we merge details from the database with details for the
      // currently installed and registered CI slots, then construct the UI. The UI remains static
      // after that. On first deactivation the settings are saved back to the DB, so the number of
      // CI setting sets in the DB matches the UI. After that, we read the settings from the
      // database each time the section is activated to avoid potential issues with the number of
      // registered slots not matching the number of sets of settings in the UI.
      if (!_isFirstActivation)
      {
        _ciSlots = DigitalDevicesCiSlots.GetDatabaseSettings();
      }

      int i = 0;
      Log.Debug("Digital Devices config: slot count = {0}", _ciSlots.Count);
      foreach (DigitalDevicesCiSlot slot in _ciSlots)
      {
        Log.Debug("Digital Devices config: slot {0}...", slot.CamRootMenuTitle);
        _decryptLimits[i].Value = slot.DecryptLimit;
        Log.Debug("  decrypt limit = {0}", slot.DecryptLimit);
        String[] providers = new String[slot.Providers.Count];
        slot.Providers.CopyTo(providers);
        _providerLists[i].Text = String.Join(", ", providers);
        Log.Debug("  provider list = {0}", String.Join("|", providers));
        i++;
      }
      base.OnSectionActivated();
      _isFirstActivation = false;
    }

    public override void OnSectionDeActivated()
    {
      Log.Debug("Digital Devices config: deactivated");
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override bool CanActivate
    {
      get
      {
        // The section can always be activated (disabling it might be confusing for people). If there
        // are no CI slots detected then we show a message in a label to say this.
        return true;
      }
    }
  }
}
