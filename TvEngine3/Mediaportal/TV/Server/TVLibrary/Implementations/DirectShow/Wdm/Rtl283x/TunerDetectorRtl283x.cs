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
using System.ComponentModel;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Rtl283x
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects the supported
  /// proprietary modes for BDA tuners based on Realtek's RTL283x chipset.
  /// </summary>
  internal class TunerDetectorRtl283x : ITunerDetectorSystem
  {
    #region enums

    private enum BdaExtensionFilterModeProperty
    {
      GetDemodSupportedModes = 2,
      SetDemodMode = 3
    }

    [Flags]
    private enum Rtl283xBroadcastStandard
    {
      None = 0,
      [Description("DVB-T")]
      Dvbt = 0x0001,
      [Description("DTMB")]
      Dtmb = 0x0002,
      [Description("FM")]
      Fm = 0x0004,
      [Description("DAB/ISDB-T 1 Seg")]
      DabIsdbt1seg = 0x0008,
      [Description("DVB-H")]
      Dvbh = 0x0010,
      [Description("DVB-C")]
      Dvbc = 0x0020,
      [Description("ATSC")]
      Atsc = 0x0040,
      [Description("ATSC-MH")]
      AtscMh = 0x0080,
      [Description("ISDB-T Full Seg")]
      IsdbtFseg = 0x0100,
      [Description("ISDB-T SB")]
      IsdbtSb = 0x0200,
      [Description("DVB-S")]
      Dvbs = 0x0400,
      [Description("CMMB")]
      Cmmb = 0x0800
    }

    private enum Rtl283xDemodulatorMode
    {
      [Description("DVB-T")]
      Dvbt = 0,
      [Description("DTMB")]
      Dtmb,
      [Description("DVB-C")]
      Dvbc,
      [Description("ATSC")]
      Atsc,
      [Description("Clear QAM")]
      ClearQam
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_FILTER_MODE_PROPERTY_SET = new Guid(0xbb992b31, 0x931d, 0x41b1, 0x85, 0xea, 0xa0, 0x1b, 0x4e, 0x30, 0x6c, 0xa5);

    #endregion

    #region variables

    private IDictionary<string, ICollection<ITuner>> _knownTuners = new Dictionary<string, ICollection<ITuner>>();    // key = device path

    #endregion

    #region ITunerDetectorSystem members

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "RTL283x";
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
      ICollection<ITuner> tuners = new List<ITuner>(0);

      // Is the interface a BDA source?
      if (classGuid != FilterCategory.BDASourceFiltersCategory || string.IsNullOrEmpty(devicePath))
      {
        return tuners;
      }

      // Detect the tuners associated with the device interface.
      DsDevice device = DsDevice.FromDevicePath(devicePath);
      if (device == null)
      {
        return tuners;
      }
      tuners = DetectTunersForDevice(device);
      if (tuners == null || tuners.Count == 0)
      {
        device.Dispose();
        return tuners;
      }

      this.LogInfo("RTL283x detector: tuner added");
      _knownTuners[devicePath] = tuners;
      return tuners;
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITuner> DetectTuners()
    {
      this.LogDebug("RTL283x detector: detect tuners");
      List<ITuner> tuners = new List<ITuner>();
      IDictionary<string, ICollection<ITuner>> knownTuners = new Dictionary<string, ICollection<ITuner>>();

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        // Is this a new device?
        string devicePath = device.DevicePath;
        ICollection<ITuner> deviceTuners = null;
        if (!string.IsNullOrEmpty(devicePath) && _knownTuners.TryGetValue(devicePath, out deviceTuners))
        {
          // No. Reuse the tuner instances we've previously created.
          device.Dispose();
          tuners.AddRange(deviceTuners);
          knownTuners.Add(devicePath, deviceTuners);
          continue;
        }

        deviceTuners = DetectTunersForDevice(device);
        if (deviceTuners == null || deviceTuners.Count == 0)
        {
          device.Dispose();
          continue;
        }

        tuners.AddRange(deviceTuners);
        knownTuners.Add(devicePath, deviceTuners);
      }

      _knownTuners = knownTuners;
      return tuners;
    }

    #endregion

    private static ICollection<ITuner> DetectTunersForDevice(DsDevice device)
    {
      ICollection<ITuner> tuners = new List<ITuner>(4);
      string name = device.Name;
      string devicePath = device.DevicePath;
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
      {
        return tuners;
      }

      // Check if we have an RTL283x based tuner.
      Log.Debug("RTL283x detector: check tuner, name = {0}, device path = {1}", name, devicePath);
      Guid filterClsid = typeof(IBaseFilter).GUID;
      object obj = null;
      try
      {
        device.Mon.BindToObject(null, null, ref filterClsid, out obj);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "RTL283x detector: failed to create filter instance, name = {0}, device path = {1}", name, devicePath);
        return tuners;
      }

      try
      {
        IKsPropertySet ps = obj as IKsPropertySet;
        if (ps == null)
        {
          Log.Debug("RTL283x detector: filter is not a property set");
          return tuners;
        }
        KSPropertySupport support;
        int hr = ps.QuerySupported(BDA_EXTENSION_FILTER_MODE_PROPERTY_SET, (int)BdaExtensionFilterModeProperty.GetDemodSupportedModes, out support);
        if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
        {
          Log.Debug("RTL283x detector: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
          return tuners;
        }

        Log.Debug("RTL283x detector: property set supported, checking supported modes");
        IntPtr buffer = Marshal.AllocCoTaskMem(4);
        Rtl283xBroadcastStandard supportedModes;
        try
        {
          int returnedByteCount;
          hr = ps.Get(BDA_EXTENSION_FILTER_MODE_PROPERTY_SET, (int)BdaExtensionFilterModeProperty.GetDemodSupportedModes, IntPtr.Zero, 0, buffer, 4, out returnedByteCount);
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != 4)
          {
            Log.Error("RTL283x detector: failed to read supported modes, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
            return tuners;
          }
          supportedModes = (Rtl283xBroadcastStandard)Marshal.ReadInt32(buffer, 0);
        }
        finally
        {
          Marshal.FreeCoTaskMem(buffer);
        }

        Log.Debug("  modes = [{0}]", supportedModes);

        if (supportedModes.HasFlag(Rtl283xBroadcastStandard.Fm))
        {
          tuners.Add(new TunerRtl283xFm(device));
        }
        return tuners;
      }
      finally
      {
        Release.ComObject("RTL283x detection BDA source filter", ref obj);
      }
    }
  }
}