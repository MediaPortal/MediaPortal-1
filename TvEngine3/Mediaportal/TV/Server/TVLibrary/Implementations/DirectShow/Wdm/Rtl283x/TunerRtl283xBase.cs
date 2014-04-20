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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Rtl283x
{
  internal class TunerRtl283xBase
  {
    #region enums

    private enum BdaExtensionFilterControlProperty
    {
      Frequency = 2,
      Bandwidth = 3,
      Modulation = 10,
      SymbolRate = 11
    }

    private enum BdaExtensionFilterModeProperty
    {
      GetDemodSupportedModes = 2,
      SetDemodMode = 3
    }

    private enum BdaExtensionDeviceControlProperty
    {
      EnablePid = 2,
      DisablePid = 3,
      PidFilterStatus = 5
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

    private static readonly Guid BDA_EXTENSION_FILTER_CONTROL_PROPERTY_SET = new Guid(0xf25c6bc9, 0xdbd7, 0x4a0c, 0xb2, 0x28, 0x9a, 0x58, 0x78, 0x9f, 0x2e, 0x1d);
    private static readonly Guid BDA_EXTENSION_FILTER_MODE_PROPERTY_SET = new Guid(0xbb992b31, 0x931d, 0x41b1, 0x85, 0xea, 0xa0, 0x1b, 0x4e, 0x30, 0x6c, 0xa5);
    private static readonly Guid BDA_EXTENSION_DEVICE_CONTROL_PROPERTY_SET = new Guid(0x1bfb70f7, 0xadfb, 0x4414, 0x9f, 0xd4, 0x60, 0xe9, 0xe5, 0x40, 0xa5, 0x59);

    #endregion

    /// <summary>
    /// Detect the compatible tuners connected to the system.
    /// </summary>
    /// <returns>an enumerable collection of <see cref="T:TvLibrary.Interfaces.ITVCard"/></returns>
    private static IEnumerable<ITVCard> DetectTuners()
    {
      Log.Debug("RTL283x base: detect tuners");
      List<ITVCard> tuners = new List<ITVCard>();

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        // First check if we have an RTL283x based tuner.
        Log.Debug("RTL283x base: check tuner {0} {1}", name, devicePath);
        Guid filterClsid = typeof(IBaseFilter).GUID;
        object obj = null;
        try
        {
          device.Mon.BindToObject(null, null, ref filterClsid, out obj);
        }
        catch (Exception ex)
        {
          Log.Error(ex, "RTL283x base: failed to create filter instance for {0} {1}", name, devicePath);
          device.Dispose();
          continue;
        }

        try
        {
          IKsPropertySet ps = obj as IKsPropertySet;
          if (ps == null)
          {
            Log.Debug("RTL283x base: filter is not a property set");
            device.Dispose();
            continue;
          }
          KSPropertySupport support;
          int hr = ps.QuerySupported(BDA_EXTENSION_FILTER_MODE_PROPERTY_SET, (int)BdaExtensionFilterModeProperty.GetDemodSupportedModes, out support);
          if (hr != (int)HResult.Severity.Success || support.HasFlag(KSPropertySupport.Get))
          {
            Log.Debug("RTL283x base: property set not supported, hr = 0x{0:x}", hr);
            device.Dispose();
            continue;
          }

          Log.Debug("RTL283x base: property set supported, checking supported modes");
          IntPtr buffer = Marshal.AllocCoTaskMem(4);
          Rtl283xBroadcastStandard supportedModes;
          try
          {
            int returnedByteCount;
            hr = ps.Get(BDA_EXTENSION_FILTER_MODE_PROPERTY_SET, (int)BdaExtensionFilterModeProperty.GetDemodSupportedModes, IntPtr.Zero, 0, buffer, 4, out returnedByteCount);
            if (hr != (int)HResult.Severity.Success || returnedByteCount != 4)
            {
              Log.Error("RTL283x base: failed to read supported modes, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
              device.Dispose();
              continue;
            }
            supportedModes = (Rtl283xBroadcastStandard)Marshal.ReadInt32(buffer);
          }
          finally
          {
            Marshal.FreeCoTaskMem(buffer);
          }

          Log.Debug("  modes = {0}", supportedModes);

          if (supportedModes.HasFlag(Rtl283xBroadcastStandard.Fm))
          {
            tuners.Add(new TunerRtl283xFm(device));
          }
        }
        finally
        {
          Release.ComObject("RTL283x detection BDA source filter", ref obj);
        }
      }

      return tuners;
    }
  }
}