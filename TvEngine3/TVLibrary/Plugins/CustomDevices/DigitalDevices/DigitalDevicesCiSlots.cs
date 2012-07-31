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
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
using TvDatabase;
using TvLibrary.Log;

namespace DigitalDevices
{
  /// <summary>
  /// A struct that is capable of holding the relevant details for a Digital Devices CI slot.
  /// </summary>
  public struct DigitalDevicesCiSlot
  {
    /// <summary>
    /// The name of the CI slot device.
    /// </summary>
    public String DeviceName;

    /// <summary>
    /// The device path of the CI slot device.
    /// </summary>
    public String DevicePath;

    /// <summary>
    /// The root menu title for the CAM in the slot.
    /// </summary>
    public String CamRootMenuTitle;

    /// <summary>
    /// The number of services that the slot/CAM is capable of decrypting simultaneously.
    /// </summary>
    /// <remarks>
    /// If set to zero, the limit is considered not known or disabled.
    /// </remarks>
    public int DecryptLimit;

    /// <summary>
    /// A set of device paths. Each entry represents a tuner that is using this CI slot to decrypt
    /// a service.
    /// </summary>
    public HashSet<String> CurrentTunerSet;

    /// <summary>
    /// A hash set of provider names (eg. ORF, Canal Digitaal, BSkyB).
    /// </summary>
    /// <remarks>
    /// The slot/CAM is able to decrypt any service supplied by any one of these providers. If the
    /// set is empty, the slot/CAM is considered to be able to decrypt any service.
    /// </remarks>
    public HashSet<String> Providers;
  }

  /// <summary>
  /// Digital Devices CI slot properties.
  /// </summary>
  public enum CommonInterfaceProperty
  {
    DecryptProgram = 0,
    CamMenuTitle,
  }

  /// <summary>
  /// A static class for common variables and functions shared between DigitalDevicesConfig and
  /// DigitalDevices.
  /// </summary>
  public class DigitalDevicesCiSlots
  {
    /// <summary>
    /// The property set used for accessing and controlling CI slot properties.
    /// </summary>
    public static readonly Guid CommonInterfacePropertySet = new Guid(0x0aa8a501, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);

    /// <summary>
    /// A DsDevice device path fragment that is common to all Digital Devices KS software components.
    /// </summary>
    public static readonly String CommonDevicePathSection = "fbca-11de-b16f-000000004d56";

    /// <summary>
    /// Get a list containing the current settings for each of the known Digital Devices CI slots.
    /// </summary>
    /// <returns>the CI list</returns>
    public static List<DigitalDevicesCiSlot> GetDatabaseSettings()
    {
      List<DigitalDevicesCiSlot> slotSettings = new List<DigitalDevicesCiSlot>();
      TvBusinessLayer layer = new TvBusinessLayer();
      byte i = 1;
      while (true)  // Loop until we don't find any more settings.
      {
        Setting devicePath = layer.GetSetting("digitalDevicesCiDevicePath" + i, String.Empty);
        if (devicePath.Value.Equals(String.Empty))
        {
          break;
        }

        DigitalDevicesCiSlot slot = new DigitalDevicesCiSlot();
        slot.DevicePath = devicePath.Value;
        Setting deviceName = layer.GetSetting("digitalDevicesCiDeviceName" + i, String.Empty);
        slot.DeviceName = deviceName.Value;
        Setting decryptLimit = layer.GetSetting("digitalDevicesCiDecryptLimit" + i, "0");
        try
        {
          slot.DecryptLimit = Int32.Parse(decryptLimit.Value);
        }
        catch (Exception)
        {
          slot.DecryptLimit = 0;
        }
        Setting providerList = layer.GetSetting("digitalDevicesCiProviderList" + i, String.Empty);
        slot.Providers = new HashSet<String>(providerList.Value.Split('|'));
        slot.CamRootMenuTitle = "<empty>";
        slotSettings.Add(slot);
        i++;
      }

      return slotSettings;
    }

    /// <summary>
    /// Read the CAM menu title from the CAM in a specific CI slot.
    /// </summary>
    /// <param name="ciFilter">The CI filter associated with the slot containing the CAM.</param>
    /// <param name="title">The CAM menu title.</param>
    /// <returns>an HRESULT indicating whether the CAM menu title was successfully retrieved</returns>
    public static int GetMenuTitle(IBaseFilter ciFilter, out String title)
    {
      title = String.Empty;

      int bufferSize = 2048;
      IntPtr buffer = Marshal.AllocCoTaskMem(bufferSize);
      for (int i = 0; i < bufferSize; i++)
      {
        Marshal.WriteByte(buffer, i, 0);
      }

      try
      {
        int returnedByteCount;
        int hr = ((IKsPropertySet)ciFilter).Get(CommonInterfacePropertySet, (int)CommonInterfaceProperty.CamMenuTitle,
          buffer, bufferSize,
          buffer, bufferSize,
          out returnedByteCount
        );
        if (hr == 0)
        {
          title = Marshal.PtrToStringAnsi(buffer, returnedByteCount).TrimEnd();
        }
        return hr;
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
        buffer = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Check if a device is a Digital Devices common interface device.
    /// </summary>
    /// <param name="device">The device to check.</param>
    /// <returns><c>true</c> if the device is a Digital Device common interface device, otherwise <c>false</c></returns>
    public static bool IsDigitalDevicesCiDevice(DsDevice device)
    {
      if (device != null && device.Name != null &&
        device.DevicePath.ToLowerInvariant().Contains(CommonDevicePathSection) &&
        device.Name.ToLowerInvariant().Contains("common interface"))
      {
        return true;
      }
      return false;
    }
  }
}
