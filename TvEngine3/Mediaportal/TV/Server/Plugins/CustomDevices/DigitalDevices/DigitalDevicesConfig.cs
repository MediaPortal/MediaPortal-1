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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices
{
  internal partial class DigitalDevicesConfig : SectionSettings
  {
    private class CiContext
    {
      public string CamMenuTitle;
      public DigitalDevicesCiSlotConfig Config;
      public NumericUpDown DecryptLimitControl;
      public MPTextBox ProviderListControl;
    }

    private List<CiContext> _ciContexts = null;
    private bool _isFirstActivation = true;

    public DigitalDevicesConfig()
      : this("Digital Devices CI")
    {
    }

    public DigitalDevicesConfig(string name)
      : base("Digital Devices CI")
    {
      this.LogDebug("Digital Devices config: constructing");

      // Get the details for the slots we've seen in the past.
      IDictionary<string, DigitalDevicesCiSlotConfig> dbConfig = DigitalDevicesCiSlotConfig.ReadAllSettings();

      // Now read the details for the currently available slots and merge settings from the DB
      // to fill the _ciSlots list.
      _ciContexts = new List<CiContext>();
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      Guid filterClsid = typeof(IBaseFilter).GUID;
      foreach (DsDevice device in captureDevices)
      {
        try
        {
          if (!DigitalDevicesCiSlot.IsDigitalDevicesCiDevice(device))
          {
            continue;
          }
          string devicePath = device.DevicePath;
          string deviceName = device.Name;
          this.LogDebug("Digital Devices config: slot {0} {1}...", deviceName, devicePath);
          CiContext context = new CiContext();
          if (dbConfig.TryGetValue(devicePath, out context.Config))
          {
            this.LogDebug("  found existing configuration");
          }
          else
          {
            this.LogDebug("  new configuration");
            context.Config = new DigitalDevicesCiSlotConfig(devicePath, deviceName);
          }

          this.LogDebug("  decrypt limit  = {0}", context.Config.DecryptLimit);
          string[] providerList = new string[context.Config.Providers.Count];
          context.Config.Providers.CopyTo(providerList);
          this.LogDebug("  provider list  = {0}", string.Join(", ", providerList));

          // If possible, read the root menu title for the CAM in the slot.
          object obj = null;
          try
          {
            device.Mon.BindToObject(null, null, ref filterClsid, out obj);
            IBaseFilter ciFilter = obj as IBaseFilter;
            if (ciFilter != null)
            {
              DigitalDevicesCiSlot slot = new DigitalDevicesCiSlot(ciFilter);
              int hr = slot.GetCamMenuTitle(out context.CamMenuTitle);
              if (hr != (int)HResult.Severity.Success)
              {
                context.CamMenuTitle = "(empty)";
              }
              ciFilter = null;
            }
          }
          catch (Exception)
          {
          }
          finally
          {
            Release.ComObject("Digital Devices config device object", ref obj);
          }
          this.LogDebug("  CAM name/title = {0}", context.CamMenuTitle);

          _ciContexts.Add(context);
        }
        finally
        {
          device.Dispose();
        }
      }
      _ciContexts.Sort(
        delegate(CiContext context1, CiContext context2)
        {
          return context1.Config.DeviceName.CompareTo(context2.Config.DeviceName);
        }
      );

      // Now that we have the details, we can build the user interface.
      InitializeComponent();
      this.LogDebug("Digital Devices config: constructed, slot count = {0}", _ciContexts.Count);
    }

    public override void SaveSettings()
    {
      this.LogDebug("Digital Devices config: saving settings, slot count = {0}", _ciContexts.Count);
      
      foreach (CiContext context in _ciContexts)
      {
        context.Config.DecryptLimit = (int)context.DecryptLimitControl.Value;
        context.Config.Providers = new HashSet<string>(Regex.Split(context.ProviderListControl.Text.Trim(), @"\s*,\s*"));

        this.LogDebug("Digital Devices config: slot {0} {1}...", context.Config.DeviceName, context.Config.DevicePath);
        this.LogDebug("  decrypt limit  = {0}", context.Config.DecryptLimit);
        this.LogDebug("  provider list  = {0}", string.Join(", ", context.Config.Providers));
        this.LogDebug("  CAM name/title = {0}", context.CamMenuTitle);

        context.Config.SaveSettings();
      }

      base.SaveSettings();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("Digital Devices config: activated");

      // On first load in the constructor we merge configuration from the database with details for
      // the currently installed and registered CI slots, then construct the UI. The UI remains
      // static after that. On first deactivation the settings are saved back to the DB, so the
      // number of CI slots in the DB matches the UI. After that, we read the settings from the DB
      // each time the section is activated to avoid potential issues with the number of registered
      // slots not matching the number of sets of settings in the UI.
      foreach (CiContext context in _ciContexts)
      {
        if (!_isFirstActivation)
        {
          context.Config.LoadSettings();
        }

        context.DecryptLimitControl.Value = context.Config.DecryptLimit;
        context.ProviderListControl.Text = string.Join(", ", context.Config.Providers);

        this.LogDebug("Digital Devices config: slot {0} {1}...", context.Config.DeviceName, context.Config.DevicePath);
        this.LogDebug("  decrypt limit  = {0}", context.Config.DecryptLimit);
        this.LogDebug("  provider list  = {0}", context.ProviderListControl.Text);
        this.LogDebug("  CAM name/title = {0}", context.CamMenuTitle);
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