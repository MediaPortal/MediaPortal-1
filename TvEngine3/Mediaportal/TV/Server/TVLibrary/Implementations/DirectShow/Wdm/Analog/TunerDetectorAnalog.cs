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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects analog tuners and
  /// capture devices with WDM/DirectShow drivers.
  /// </summary>
  internal class TunerDetectorAnalog : ITunerDetectorSystem
  {
    #region variables

    // key = device path
    private IDictionary<string, ITVCard> _knownTuners = new Dictionary<string, ITVCard>();

    #endregion

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "WDM";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITVCard> DetectTuners()
    {
      this.LogDebug("WDM detector: detect tuners");
      List<ITVCard> tuners = new List<ITVCard>();
      IDictionary<string, ITVCard> knownTuners = new Dictionary<string, ITVCard>();
      HashSet<string> crossbarProductIds = new HashSet<string>();

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        // Is this a new device?
        ITVCard tuner;
        if (_knownTuners.TryGetValue(devicePath, out tuner))
        {
          device.Dispose();
          tuners.Add(tuner);
          knownTuners.Add(devicePath, tuner);
          if (tuner.ProductInstanceId != null)
          {
            crossbarProductIds.Add(tuner.ProductInstanceId);
          }
          continue;
        }

        this.LogDebug("WDM detector: crossbar {0} {1}", name, devicePath);
        tuner = new TunerAnalog(device, FilterCategory.AMKSCrossbar);
        tuners.Add(tuner);
        knownTuners.Add(devicePath, tuner);
        if (tuner.ProductInstanceId != null)
        {
          crossbarProductIds.Add(tuner.ProductInstanceId);
        }
      }

      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        // Is this a new device?
        ITVCard tuner;
        if (_knownTuners.TryGetValue(devicePath, out tuner))
        {
          device.Dispose();
          if (tuner != null)
          {
            tuners.Add(tuner);
          }
          knownTuners.Add(devicePath, tuner);
          continue;
        }

        // We don't want to add duplicate entries for multi-input capture
        // devices (already detected via crossbar).
        this.LogDebug("WDM detector: capture {0} {1}", name, devicePath);
        tuner = new TunerAnalog(device, FilterCategory.AMKSCapture);
        if (tuner.ProductInstanceId != null && crossbarProductIds.Contains(tuner.ProductInstanceId))
        {
          // This source has a crossbar. Don't use it.
          this.LogDebug("  already detected crossbar", name, devicePath);
          tuner.Dispose();
          knownTuners.Add(devicePath, null);
          continue;
        }

        tuners.Add(tuner);
        knownTuners.Add(devicePath, tuner);
      }

      _knownTuners = knownTuners;
      return tuners;
    }
  }
}