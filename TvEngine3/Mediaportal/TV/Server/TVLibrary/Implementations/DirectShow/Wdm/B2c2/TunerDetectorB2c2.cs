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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects TechniSat tuners with
  /// B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerDetectorB2c2 : ITunerDetectorSystem
  {
    private static readonly int DEVICE_INFO_SIZE = Marshal.SizeOf(typeof(DeviceInfo));    // 416

    private IDictionary<uint, ITuner> _knownTuners = new Dictionary<uint, ITuner>();      // key = B2C2 device ID

    #region ITunerDetectorSystem members

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "B2C2";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a system device
    /// interface.
    /// </summary>
    /// <param name="classGuid">The identifier for the interface's class.</param>
    /// <param name="devicePath">The interface's device path.</param>
    /// <returns>the compatible tuners exposed by the interface</returns>
    public ICollection<ITuner> DetectTuners(Guid classGuid, string devicePath)
    {
      return new List<ITuner>(0);
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITuner> DetectTuners()
    {
      this.LogDebug("B2C2 detector: detect tuners");
      List<ITuner> tuners = new List<ITuner>();
      IDictionary<uint, ITuner> knownTuners = new Dictionary<uint, ITuner>();

      // Instanciate a data interface so we can check how many tuners are installed.
      IBaseFilter b2c2Source = null;
      try
      {
        b2c2Source = Activator.CreateInstance(Type.GetTypeFromCLSID(Constants.B2C2_ADAPTER_CLSID)) as IBaseFilter;
      }
      catch
      {
        // Hardware/driver might not be installed => not an error.
        //this.LogError(ex, "B2C2 detector: failed to create source filter instance");
        return tuners;
      }

      try
      {
        IMpeg2DataCtrl6 dataInterface = b2c2Source as IMpeg2DataCtrl6;
        if (dataInterface == null)
        {
          this.LogError("B2C2 detector: failed to find B2C2 data interface on filter");
          Release.ComObject("B2C2 source filter", ref b2c2Source);
          return tuners;
        }

        // Get device details...
        DeviceInfo[] info = new DeviceInfo[Constants.MAX_DEVICE_COUNT];
        int size = DEVICE_INFO_SIZE * Constants.MAX_DEVICE_COUNT;
        int deviceCount = Constants.MAX_DEVICE_COUNT;
        int hr = dataInterface.GetDeviceList(info, ref size, ref deviceCount);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("B2C2 detector: failed to get device list, hr = 0x{0:x}", hr);
        }
        else
        {
          this.LogDebug("B2C2 detector: device count = {0}", deviceCount);
          for (int i = 0; i < deviceCount; i++)
          {
            DeviceInfo d = info[i];

            // Is this a new tuner?
            ITuner t;
            if (_knownTuners.TryGetValue(d.DeviceId, out t))
            {
              // No. Reuse the tuner instance we've previously created.
              tuners.Add(t);
              knownTuners.Add(d.DeviceId, t);
              continue;
            }
            t = null;

            this.LogDebug("B2C2 detector: device {0}", i + 1);
            this.LogDebug("  device ID           = {0}", d.DeviceId);
            this.LogDebug("  MAC address         = {0}", BitConverter.ToString(d.MacAddress.Address).ToLowerInvariant());
            this.LogDebug("  tuner type          = {0}", d.TunerType);
            this.LogDebug("  bus interface       = {0}", d.BusInterface);
            this.LogDebug("  is in use?          = {0}", d.IsInUse);
            this.LogDebug("  product ID          = {0}", d.ProductId);
            this.LogDebug("  product name        = {0}", d.ProductName);
            this.LogDebug("  product description = {0}", d.ProductDescription);
            this.LogDebug("  product revision    = {0}", d.ProductRevision);
            this.LogDebug("  product front end   = {0}", d.ProductFrontEnd);

            switch (d.TunerType)
            {
              case TunerType.Satellite:
                t = new TunerB2c2Satellite(d);
                break;
              case TunerType.Cable:
                t = new TunerB2c2Cable(d);
                break;
              case TunerType.Terrestrial:
                t = new TunerB2c2Terrestrial(d);
                break;
              case TunerType.Atsc:
                t = new TunerB2c2Atsc(d);
                break;
              default:
                // The tuner may not be redetected properly after standby in some cases.
                this.LogWarn("B2C2 detector: unknown tuner type {0}, cannot use this tuner", d.TunerType);
                break;
            }

            if (t != null)
            {
              tuners.Add(t);
              knownTuners.Add(d.DeviceId, t);
            }
          }
          this.LogDebug("B2C2 detector: result = success");
        }
      }
      finally
      {
        Release.ComObject("B2C2 source filter", ref b2c2Source);
      }

      _knownTuners = knownTuners;
      return tuners;
    }

    #endregion
  }
}