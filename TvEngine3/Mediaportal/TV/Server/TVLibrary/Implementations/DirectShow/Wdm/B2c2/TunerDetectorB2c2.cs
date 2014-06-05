using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects TechniSat tuners with
  /// B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerDetectorB2c2 : ITunerDetectorSystem
  {
    private static readonly int DEVICE_INFO_SIZE = Marshal.SizeOf(typeof(DeviceInfo));                // 416

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
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITVCard> DetectTuners()
    {
      this.LogDebug("B2C2 detector: detect tuners");
      List<ITVCard> tuners = new List<ITVCard>();

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
        int size = DEVICE_INFO_SIZE * Constants.MAX_DEVICE_COUNT;
        int deviceCount = Constants.MAX_DEVICE_COUNT;
        IntPtr structurePtr = Marshal.AllocCoTaskMem(size);
        try
        {
          for (int i = 0; i < size; i++)
          {
            Marshal.WriteByte(structurePtr, i, 0);
          }
          int hr = dataInterface.GetDeviceList(structurePtr, ref size, ref deviceCount);
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogError("B2C2 detector: failed to get device list, hr = 0x{0:x}", hr);
          }
          else
          {
            //Dump.DumpBinary(structurePtr, size);
            this.LogDebug("B2C2 detector: device count = {0}", deviceCount);
            for (int i = 0; i < deviceCount; i++)
            {
              this.LogDebug("B2C2 detector: device {0}", i + 1);
              DeviceInfo d = (DeviceInfo)Marshal.PtrToStructure(structurePtr, typeof(DeviceInfo));
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
                  tuners.Add(new TunerB2c2Satellite(d));
                  break;
                case TunerType.Cable:
                  tuners.Add(new TunerB2c2Cable(d));
                  break;
                case TunerType.Terrestrial:
                  tuners.Add(new TunerB2c2Terrestrial(d));
                  break;
                case TunerType.Atsc:
                  tuners.Add(new TunerB2c2Atsc(d));
                  break;
                default:
                  // The tuner may not be redetected properly after standby in some cases.
                  this.LogWarn("B2C2 detector: unknown tuner type {0}, cannot use this tuner", d.TunerType);
                  break;
              }

              structurePtr = IntPtr.Add(structurePtr, DEVICE_INFO_SIZE);
            }
            this.LogDebug("B2C2 detector: result = success");
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(structurePtr);
        }
      }
      finally
      {
        Release.ComObject("B2C2 source filter", ref b2c2Source);
      }

      return tuners;
    }
  }
}