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
using DirectShowLib;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.Service
{
  internal class DigitalDevicesConfigService : IDigitalDevicesConfigService
  {
    public ICollection<DigitalDevicesCiSlotConfig> GetAllSlotConfiguration()
    {
      ICollection<DigitalDevicesCiSlotConfig> settings = DigitalDevicesCiSlotConfig.ReadAllSettings();
      HashSet<string> keys = new HashSet<string>();
      foreach (DigitalDevicesCiSlotConfig config in settings)
      {
        keys.Add(config.DevicePath);
      }

      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      foreach (DsDevice device in captureDevices)
      {
        try
        {
          int index;
          if (!DigitalDevicesHardware.IsCiDevice(device, out index) || keys.Contains(device.DevicePath))
          {
            continue;
          }
          settings.Add(new DigitalDevicesCiSlotConfig(device.DevicePath, device.Name));
        }
        finally
        {
          device.Dispose();
        }
      }
      return settings;
    }

    public void SaveAllSlotConfiguration(ICollection<DigitalDevicesCiSlotConfig> settings)
    {
      foreach (DigitalDevicesCiSlotConfig config in settings)
      {
        config.SaveSettings();
      }
    }

    public string GetCamNameForSlot(string devicePath)
    {
      string name = null;
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      Guid filterClsid = typeof(IBaseFilter).GUID;
      foreach (DsDevice device in captureDevices)
      {
        try
        {
          int index;
          if (!string.Equals(devicePath, device.DevicePath) || !DigitalDevicesHardware.IsCiDevice(device, out index))
          {
            continue;
          }

          // If possible, read the root menu title for the CAM in the slot.
          name = string.Empty;
          object obj = null;
          try
          {
            device.Mon.BindToObject(null, null, ref filterClsid, out obj);
            IBaseFilter ciFilter = obj as IBaseFilter;
            if (ciFilter != null)
            {
              DigitalDevicesCiSlot slot = new DigitalDevicesCiSlot(index, ciFilter);
              slot.GetCamMenuTitle(out name);
            }
          }
          finally
          {
            Release.ComObject("Digital Devices config service device object", ref obj);
          }
        }
        finally
        {
          device.Dispose();
        }
      }
      return name;
    }
  }
}