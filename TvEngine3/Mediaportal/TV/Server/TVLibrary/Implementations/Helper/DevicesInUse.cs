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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  /// <summary>
  /// This is a class which is used to remember which devices are currently in
  /// use.
  /// </summary>
  public class DevicesInUse
  {
    private static DevicesInUse _instance;
    private readonly List<DsDevice> _devicesInUse;

    /// <summary>
    /// A static method to access the singleton instance of this class.
    /// </summary>
    public static DevicesInUse Instance
    {
      get
      {
        lock (_instance)
        {
          if (_instance == null)
          {
            _instance = new DevicesInUse();
          }
        }
        return _instance;
      }
    }

    /// <summary>
    /// Constructor, private since this is a singleton class.
    /// </summary>
    private DevicesInUse()
    {
      _devicesInUse = new List<DsDevice>();
    }

    /// <summary>
    /// Call this function before using a device to check whether it is
    /// possible to use it.
    /// </summary>
    /// <param name="device">The device to check.</param>
    /// <returns><c>true</c> if the device can be used, otherwise <c>false</c></returns>
    public bool Add(DsDevice device)
    {
      if (device == null)
      {
        return false;
      }

      lock (_devicesInUse)
      {
        this.LogDebug("Devices-in-use: add {0}...", device.Name);
        foreach (DsDevice dev in _devicesInUse)
        {
          if (dev.Name.Equals(device.Name) && device.Mon.IsEqual(dev.Mon) == 0 && dev.DevicePath.Equals(device.DevicePath))
          {
            this.LogDebug("Devices-in-use: in use, can't be used");
            return false;
          }
        }
        this.LogDebug("Devices-in-use: not yet in use, usable");
        _devicesInUse.Add(device);
      }
      return true;
    }


    /// <summary>
    /// Call this function when a device is no longer required.
    /// </summary>
    /// <param name="device">The device.</param>
    public void Remove(DsDevice device)
    {
      if (device == null)
      {
        return;
      }

      lock (_devicesInUse)
      {
        this.LogDebug("Devices-in-use: remove {0}...", device.Name);
        for (int i = 0; i < _devicesInUse.Count; ++i)
        {
          DsDevice dev = _devicesInUse[i];
          if (dev.Name.Equals(device.Name) && device.Mon.IsEqual(dev.Mon) == 0 && dev.DevicePath.Equals(device.DevicePath))
          {
            _devicesInUse.RemoveAt(i);
            return;
          }
        }
      }
    }
  }
}