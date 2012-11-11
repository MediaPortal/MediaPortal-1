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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.DigitalDevices
{
  public partial class DigitalDevicesConfig : SectionSettings
  {


    private List<DigitalDevicesCiSlot> _ciSlots = null;
    private NumericUpDown[] _decryptLimits = null;
    private MPTextBox[] _providerLists = null;
    private bool _isFirstActivation = true;

    private readonly ISettingService _settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;

    public DigitalDevicesConfig()
      : this("Digital Devices CI")
    {
    }

    public DigitalDevicesConfig(string name)
      : base("Digital Devices CI")
    {
      this.LogDebug("Digital Devices config: constructing");

      // Get the details for the slots we've seen in the past.
      Dictionary<String, DigitalDevicesCiSlot> dbSlots = DigitalDevicesCiSlots.GetDatabaseSettings();

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
        this.LogDebug("Digital Devices config: device {0} ({1})...", device.Name, device.DevicePath);
        DigitalDevicesCiSlot slot = new DigitalDevicesCiSlot(device.DevicePath);
        if (dbSlots.ContainsKey(device.DevicePath))
        {
          this.LogDebug("  found existing configuration");
          slot = dbSlots[device.DevicePath];
        }
        else
        {
          this.LogDebug("  new configuration");
        }
        slot.DeviceName = device.Name;
        this.LogDebug("  decrypt limit  = {0}", slot.DecryptLimit);
        String[] providerList = new String[slot.Providers.Count];
        slot.Providers.CopyTo(providerList);
        this.LogDebug("  provider list  = {0}", String.Join(", ", providerList));

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
        this.LogDebug("  CAM name/title = {0}", slot.CamRootMenuTitle);

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
      this.LogDebug("Digital Devices config: constructed, slot count = {0}", _ciSlots.Count);
    }

    public override void SaveSettings()
    {
      this.LogDebug("Digital Devices config: saving settings, slot count = {0}", _ciSlots.Count);
      
      byte i = 0;
      foreach (DigitalDevicesCiSlot slot in _ciSlots)
      {
        this.LogDebug("Digital Devices config: slot {0}...", slot.CamRootMenuTitle);
        _settingServiceAgent.SaveSetting("digitalDevicesCiDeviceName" + i, slot.DeviceName);
        // Persist the slot related settings. The other struct properties are dynamic.
        this.LogDebug("  device name   = {0}", slot.DeviceName);

        _settingServiceAgent.SaveSetting("digitalDevicesCiDevicePath" + i, slot.DevicePath);
        this.LogDebug("  device path   = {0}", slot.DevicePath);

        string decryptLimitValue = _decryptLimits[i].Value.ToString();
        _settingServiceAgent.SaveSetting("digitalDevicesCiDecryptLimit" + i, decryptLimitValue);
        this.LogDebug("  decrypt limit = {0}", decryptLimitValue);

        string digitalDevicesCiProviderList = String.Join("|", Regex.Split(_providerLists[i].Text.Trim(), @"\s*,\s*"));
        _settingServiceAgent.SaveSetting("digitalDevicesCiProviderList" + i, digitalDevicesCiProviderList);
        this.LogDebug("  provider list = {0}", digitalDevicesCiProviderList);

        i++;
      }

      // Invalidate any other settings in the DB.
      _settingServiceAgent.SaveSetting("digitalDevicesCiDevicePath" + i, String.Empty);

      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("Digital Devices config: activated");

      // On first load in the constructor we merge details from the database with details for the
      // currently installed and registered CI slots, then construct the UI. The UI remains static
      // after that. On first deactivation the settings are saved back to the DB, so the number of
      // CI setting sets in the DB matches the UI. After that, we read the settings from the
      // database each time the section is activated to avoid potential issues with the number of
      // registered slots not matching the number of sets of settings in the UI.
      if (!_isFirstActivation)
      {
        _ciSlots = null;
        IEnumerator<DigitalDevicesCiSlot> en = DigitalDevicesCiSlots.GetDatabaseSettings().Values.GetEnumerator();
        while (en.MoveNext())
        {
          _ciSlots.Add(en.Current);
        }
      }

      int i = 0;
      this.LogDebug("Digital Devices config: slot count = {0}", _ciSlots.Count);
      foreach (DigitalDevicesCiSlot slot in _ciSlots)
      {
        this.LogDebug("Digital Devices config: slot {0}...", slot.CamRootMenuTitle);
        _decryptLimits[i].Value = slot.DecryptLimit;
        this.LogDebug("  decrypt limit = {0}", slot.DecryptLimit);
        String[] providers = new String[slot.Providers.Count];
        slot.Providers.CopyTo(providers);
        _providerLists[i].Text = String.Join(", ", providers);
        this.LogDebug("  provider list = {0}", String.Join("|", providers));
        i++;
      }
      base.OnSectionActivated();
      _isFirstActivation = false;
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("Digital Devices config: deactivated");
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
