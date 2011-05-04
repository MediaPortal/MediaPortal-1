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
using System.Text;
using DirectShowLib;
using TvDatabase;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// This is a class which is used to remember which
  /// software encoders are currently in use and how many
  /// instances of each have been instantiated.
  /// </summary>
  public class EncodersInUse
  {
    private static EncodersInUse _instance;
    private readonly Dictionary<DsDevice, int> _encodersInUse;
    private readonly TvBusinessLayer _layer;

    /// <summary>
    /// static method to access this class
    /// </summary>
    public static EncodersInUse Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new EncodersInUse();
        }
        return _instance;
      }
    }

    /// <summary>
    /// ctor - private since this is a singleton class
    /// </summary>
    private EncodersInUse()
    {
      _encodersInUse = new Dictionary<DsDevice, int>();
      _layer = new TvBusinessLayer();
    }

    /// <summary>
    /// call this function before using an encoder to check
    /// whether it is possible to use it
    /// </summary>
    /// <param name="device">the encoder device</param>
    /// <param name="dbEncoder">the preferences for dealing with the encoder</param>
    /// <returns><c>true</c> if the encoder can be used, otherwise <c>false</c></returns>
    public bool Add(DsDevice device, SoftwareEncoder dbEncoder)
    {
      if (device == null)
      {
        return false;
      }

      int reuseLimit = Convert.ToInt32(_layer.GetSetting("softwareEncoderReuseLimit", "0").Value);
      lock (_encodersInUse)
      {
        DsDevice key = null;
        foreach (DsDevice dev in _encodersInUse.Keys)
        {
          if (dev.Name == device.Name && device.Mon.IsEqual(dev.Mon) == 0 && dev.DevicePath == device.DevicePath)
          {
            Log.Log.WriteFile("analog:  compressor {0} is in use, checking reuse limit...", dev.Name);
            key = dev;
            break;
          }
        }

        // Encoder not yet used -> always okay to use.
        if (key == null)
        {
          Log.Log.WriteFile("analog:  compressor {0} is usable", device.Name);
          _encodersInUse.Add(device, 1);
          return true;
        }

        // Encoder not yet in DB -> assume reusable.
        if (dbEncoder == null)
        {
          Log.Log.WriteFile("analog:  unrecognised compressor, assuming usable");
          _encodersInUse[key]++;
          return true;
        }

        // If the encoder is reusable then check
        // the existing usage against the cap.
        if (dbEncoder.Reusable)
        {
          if (reuseLimit <= 0 || _encodersInUse[key] < reuseLimit)
          {
            Log.Log.WriteFile("analog:  reusable compressor, usage under limit (usage: {0}, limit: {1})",
                              _encodersInUse[key], reuseLimit == 0 ? "[unlimited]" : reuseLimit.ToString());
            _encodersInUse[key]++;
            return true;
          }
          else
          {
            Log.Log.WriteFile("analog:  reusable compressor, usage already at limit (usage: {0}, limit: {1})",
                              _encodersInUse[key], reuseLimit);
            return false;
          }
        }
      }

      // If we get to here then the encoder isn't reusable
      // and it is in use, which means the limit has already
      // been reached. The encoder wouldn't be in _encodersInUse
      // if it wasn't in use...
      Log.Log.WriteFile("analog:  non-reusable compressor, already used");
      return false;
    }

    /// <summary>
    /// use this method to indicate that the device specified no longer in use
    /// </summary>
    /// <param name="device">device</param>
    public void Remove(DsDevice device)
    {
      if (device == null)
      {
        return;
      }

      lock (_encodersInUse)
      {
        foreach (DsDevice dev in _encodersInUse.Keys)
        {
          if (dev.Name == device.Name && device.Mon.IsEqual(dev.Mon) == 0 && dev.DevicePath == device.DevicePath)
          {
            Log.Log.WriteFile("analog: removing instance of compressor {0} from use", dev.Name);
            _encodersInUse[dev]--;
            if (_encodersInUse[dev] == 0)
            {
              Log.Log.WriteFile("analog: compressor is no longer in use");
              _encodersInUse.Remove(dev);
            }
            break;
          }
        }
      }
    }
  }
}
