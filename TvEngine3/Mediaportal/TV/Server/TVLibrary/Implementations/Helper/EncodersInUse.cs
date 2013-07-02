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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  /// <summary>
  /// This is a class which is used to remember which software encoders are
  /// currently in use and how many instances of each have been instantiated.
  /// </summary>
  public class EncodersInUse
  {
    private static EncodersInUse _instance;
    private readonly Dictionary<DsDevice, int> _encodersInUse;

    /// <summary>
    /// A static method to access the singleton instance of this class.
    /// </summary>
    public static EncodersInUse Instance
    {
      get
      {
        lock (_instance)
        {
          if (_instance == null)
          {
            _instance = new EncodersInUse();
          }
        }
        return _instance;
      }
    }

    /// <summary>
    /// Constructor, private since this is a singleton class.
    /// </summary>
    private EncodersInUse()
    {
      _encodersInUse = new Dictionary<DsDevice, int>();      
    }

    /// <summary>
    /// Call this function before using an encoder to check whether it is
    /// possible to use it.
    /// </summary>
    /// <param name="device">The encoder device to check.</param>
    /// <param name="dbEncoder">The preferences for dealing with the encoder.</param>
    /// <returns><c>true</c> if the encoder can be used, otherwise <c>false</c></returns>
    public bool Add(DsDevice device, SoftwareEncoder dbEncoder)
    {
      if (device == null)
      {
        return false;
      }

      int reuseLimit = SettingsManagement.GetValue("softwareEncoderReuseLimit", 0);
      lock (_encodersInUse)
      {
        this.LogDebug("Encoders-in-use: add {0}...", device.Name);
        DsDevice key = null;
        foreach (DsDevice dev in _encodersInUse.Keys)
        {
          if (dev.Name.Equals(device.Name) && device.Mon.IsEqual(dev.Mon) == 0 && dev.DevicePath.Equals(device.DevicePath))
          {
            this.LogDebug("Encoders-in-use: in use, check reuse limit");
            key = dev;
            break;
          }
        }

        // Encoder not yet used -> always okay to use.
        if (key == null)
        {
          this.LogDebug("Encoders-in-use: not yet in use, usable");
          _encodersInUse.Add(device, 1);
          return true;
        }

        // Encoder not yet in DB -> assume reusable.
        if (dbEncoder == null)
        {
          this.LogDebug("Encoders-in-use: unrecognised, assuming usable");
          _encodersInUse[key]++;
          return true;
        }

        // If the encoder is reusable then check the existing usage against the
        // cap.
        if (dbEncoder.Reusable)
        {
          if (reuseLimit <= 0 || _encodersInUse[key] < reuseLimit)
          {
            _encodersInUse[key]++;
            this.LogDebug("Encoders-in-use: usable, usage = {0}, limit = {1}",
                              _encodersInUse[key], reuseLimit == 0 ? "[unlimited]" : reuseLimit.ToString());
            return true;
          }
          else
          {
            this.LogDebug("Encoders-in-use: at reuse limit {0}", reuseLimit);
            return false;
          }
        }
      }

      // If we get to here then the encoder isn't reusable and it is in use,
      // which means the limit has already been reached. The encoder wouldn't
      // be in _encodersInUse if it wasn't in use...
      this.LogDebug("Encoders-in-use: not reusable");
      return false;
    }

    /// <summary>
    /// Call this function when an encoder is no longer required.
    /// </summary>
    /// <param name="device">The encoder device.</param>
    public void Remove(DsDevice device)
    {
      if (device == null)
      {
        return;
      }

      lock (_encodersInUse)
      {
        this.LogDebug("Encoders-in-use: remove {0}...", device.Name);
        foreach (DsDevice dev in _encodersInUse.Keys)
        {
          if (dev.Name.Equals(device.Name) && device.Mon.IsEqual(dev.Mon) == 0 && dev.DevicePath.Equals(device.DevicePath))
          {
            _encodersInUse[dev]--;
            this.LogDebug("Encoders-in-use: usage = {0}", _encodersInUse[dev]);
            if (_encodersInUse[dev] == 0)
            {
              _encodersInUse.Remove(dev);
            }
            break;
          }
        }
      }
    }
  }
}
