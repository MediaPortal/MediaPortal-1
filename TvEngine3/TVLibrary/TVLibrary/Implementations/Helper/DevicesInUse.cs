#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace TvLibrary.Implementations
{
  /// <summary>
  /// class which is used to remember which devices are currently in use
  /// </summary>
  public class DevicesInUse
  {
    private static DevicesInUse _instance;
    private readonly List<DsDevice> _devicesInUse;

    /// <summary>
    /// static method to access this class
    /// </summary>
    public static DevicesInUse Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new DevicesInUse();
        }
        return _instance;
      }
    }

    /// <summary>
    /// ctor
    /// </summary>
    public DevicesInUse()
    {
      _devicesInUse = new List<DsDevice>();
    }

    /// <summary>
    /// use this method to indicate that the device specified is in use
    /// </summary>
    /// <param name="device">device</param>
    public void Add(DsDevice device)
    {
      _devicesInUse.Add(device);
    }


    /// <summary>
    /// use this method to indicate that the device specified no longer in use
    /// </summary>
    /// <param name="device">device</param>
    public void Remove(DsDevice device)
    {
      for (int i = 0; i < _devicesInUse.Count; ++i)
      {
        if (_devicesInUse[i].Mon == device.Mon && _devicesInUse[i].Name == device.Name)
        {
          _devicesInUse.RemoveAt(i);
          return;
        }
      }
    }

    /// <summary>
    /// returns true when the device specified is in use otherwise false
    /// </summary>
    /// <param name="device">device to check</param>
    /// <returns></returns>
    public bool IsUsed(DsDevice device)
    {
      for (int i = 0; i < _devicesInUse.Count; ++i)
      {
        if (_devicesInUse[i].Mon == device.Mon && _devicesInUse[i].Name == device.Name &&
            _devicesInUse[i].DevicePath == device.DevicePath)
        {
          Log.Log.WriteFile("device in use", device.Name);
          Log.Log.WriteFile("  moniker   :{0} ", device.Mon);
          Log.Log.WriteFile("  name      :{0} ", device.Name);
          Log.Log.WriteFile("  devicepath:{0} ", device.DevicePath);
          return true;
        }
      }
      return false;
    }
  }
}