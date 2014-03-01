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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVControl.ServiceAgents;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices
{
  /// <summary>
  /// Configuration properties for a Digital Devices CI slot.
  /// </summary>
  public class DigitalDevicesCiSlotConfig
  {
    /// <summary>
    /// The device path of the CI slot device.
    /// </summary>
    public string DevicePath;

    /// <summary>
    /// The name of the CI slot device.
    /// </summary>
    public string DeviceName;

    /// <summary>
    /// The number of services that the slot/CAM is capable of decrypting simultaneously.
    /// </summary>
    /// <remarks>
    /// If set to zero, the limit is considered not known or disabled.
    /// </remarks>
    public int DecryptLimit;

    /// <summary>
    /// A hash set of provider names (eg. ORF, Canal Digitaal, BSkyB).
    /// </summary>
    /// <remarks>
    /// The slot/CAM is able to decrypt any service supplied by any one of these providers. If the
    /// set is empty, the slot/CAM is considered to be able to decrypt any service.
    /// </remarks>
    public HashSet<string> Providers;

    private readonly ISettingService _settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="devicePath">The device path of the corresponding CI device.</param>
    /// <param name="deviceName">The name of the corresponding CI device.</param>
    public DigitalDevicesCiSlotConfig(string devicePath, string deviceName)
    {
      DevicePath = devicePath;
      DeviceName = deviceName;
      DecryptLimit = 0;
      Providers = new HashSet<string>();
    }

    /// <summary>
    /// Load the CI slot settings from the database.
    /// </summary>
    public void LoadSettings()
    {
      DecryptLimit = 0;
      Providers = new HashSet<string>();

      byte i = 0;
      while (true)  // Loop until we don't find any more settings.
      {
        string devicePath = _settingServiceAgent.GetValue("digitalDevicesCiDevicePath" + i, string.Empty);
        if (string.IsNullOrEmpty(devicePath))
        {
          break;
        }
        if (devicePath.Equals(DevicePath))
        {
          DeviceName = _settingServiceAgent.GetValue("digitalDevicesCiDeviceName" + i, DeviceName);
          DecryptLimit = _settingServiceAgent.GetValue("digitalDevicesCiDecryptLimit" + i, 0);
          string providers = _settingServiceAgent.GetValue("digitalDevicesCiProviderList" + i, string.Empty);
          Providers = new HashSet<string>(providers.Split('|'));
          break;
        }
        i++;
      }
    }

    /// <summary>
    /// Save the CI slot settings to the database.
    /// </summary>
    public void SaveSettings()
    {
      byte i = 0;
      while (true)  // Loop until we find existing settings or a free space to store new settings.
      {
        string devicePath = _settingServiceAgent.GetValue("digitalDevicesCiDevicePath" + i, DevicePath);
        if (devicePath.Equals(DevicePath))
        {
          _settingServiceAgent.SaveValue("digitalDevicesCiDevicePath" + i, DevicePath);
          _settingServiceAgent.SaveValue("digitalDevicesCiDeviceName" + i, DeviceName);
          _settingServiceAgent.SaveValue("digitalDevicesCiDecryptLimit" + i, DecryptLimit);
          _settingServiceAgent.SaveValue("digitalDevicesCiProviderList" + i, string.Join("|", Providers));
          break;
        }
        i++;
      }
    }

    /// <summary>
    /// Get a list containing the current settings for each of the known Digital Devices CI slots.
    /// </summary>
    /// <returns>the CI slot configuration list</returns>
    public static IDictionary<string, DigitalDevicesCiSlotConfig> ReadAllSettings()
    {
      ISettingService settingServiceAgent = ServiceAgents.Instance.SettingServiceAgent;
      Dictionary<string, DigitalDevicesCiSlotConfig> settings = new Dictionary<string, DigitalDevicesCiSlotConfig>();
      byte i = 0;
      while (true)  // Loop until we don't find any more settings.
      {
        string devicePath = settingServiceAgent.GetValue("digitalDevicesCiDevicePath" + i, string.Empty);
        if (string.IsNullOrEmpty(devicePath))
        {
          break;
        }

        DigitalDevicesCiSlotConfig slot = new DigitalDevicesCiSlotConfig(devicePath, string.Empty);
        slot.DeviceName = settingServiceAgent.GetValue("digitalDevicesCiDeviceName" + i, string.Empty);
        slot.DecryptLimit = settingServiceAgent.GetValue("digitalDevicesCiDecryptLimit" + i, 0);
        string providers = settingServiceAgent.GetValue("digitalDevicesCiProviderList" + i, string.Empty);
        slot.Providers = new HashSet<string>(providers.Split('|'));

        // Use the first settings found. Settings found later could be invalid left-overs.
        if (!settings.ContainsKey(devicePath))
        {
          settings.Add(devicePath, slot);
        }
        i++;
      }

      return settings;
    }
  }
}
