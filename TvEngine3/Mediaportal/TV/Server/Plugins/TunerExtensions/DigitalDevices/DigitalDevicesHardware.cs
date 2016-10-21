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
using System.Text.RegularExpressions;
using DirectShowLib;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices
{
  /// <summary>
  /// A wrapper class for Digital Devices hardware information  slot properties and methods.
  /// </summary>
  /// <remarks>
  /// The indices produced by this class are unique among the component devices of a given type
  /// (tuner, capture or common interface) connected to a base device (Cine, Octopus CI, bridge).
  /// They are not unique across base devices.
  /// </remarks>
  internal class DigitalDevicesHardware
  {
    /// <summary>
    /// Check if a device is a Digital Devices device.
    /// </summary>
    /// <param name="devicePath">The device's device path.</param>
    /// <param name="index">The device's index if the device is a Digital Devices device, otherwise <c>-1</c>.</param>
    /// <returns><c>true</c> if the device is a Digital Devices device, otherwise <c>false</c></returns>
    public static bool IsDevice(string devicePath, out int index)
    {
      index = -1;
      if (devicePath == null)
      {
        return false;
      }
      Match m = Regex.Match(devicePath, @"\{8b884[0-9a-f]{2}([0-9a-f])\-fbca-11de-b16f-000000004d56", RegexOptions.IgnoreCase);
      if (m.Success)
      {
        index = Convert.ToInt32(m.Groups[1].Captures[0].Value, 16) + 1;   // + 1 to match the filter names => less confusing
        return true;
      }
      return false;
    }

    /// <summary>
    /// Check if a device is a Digital Devices common interface device.
    /// </summary>
    /// <param name="device">The device to check.</param>
    /// <param name="index">The device's index if the device is a Digital Devices common interface device, otherwise <c>-1</c>.</param>
    /// <returns><c>true</c> if the device is a Digital Devices common interface device, otherwise <c>false</c></returns>
    public static bool IsCiDevice(DsDevice device, out int index)
    {
      index = -1;
      if (device != null && IsDevice(device.DevicePath, out index) && device.Name != null && device.Name.ToLowerInvariant().Contains("common interface"))
      {
        return true;
      }
      return false;
    }
  }
}