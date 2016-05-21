#region Copyright (C) 2005-2009 Team MediaPortal

// Copyright (C) 2005-2009 Team MediaPortal
// http://www.team-mediaportal.com
// 
// This Program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2, or (at your option)
// any later version.
// 
// This Program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with GNU Make; see the file COPYING.  If not, write to
// the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
// http://www.gnu.org/copyleft/gpl.html

#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt
{
  /// <summary>
  /// This class supports the USB-UIRT.
  /// </summary>
  /// <remarks>
  /// For general information about the product refer to:
  /// http://usbuirt.com/
  /// 
  /// For information about the UIRT raw and struct protocols refer to:
  /// http://www.usbuirt.com/USB-UIRT%20Command%20Protocol.doc
  /// </remarks>
  internal class Driver
  {
    #region DLL imports

    // Not documented (v2.7.0.0):
    // - UUIRTConvertProntoCode
    // - UUIRTEEProgram
    // - UUIRTEERead
    // - UUIRTGetUUIRTGPIOCfg
    // - UUIRTSetUUIRTGPIOCfg
    // - UUIRTSetRawReceiveCallback

    /// <summary>
    /// Get information about the USB-UIRT driver.
    /// </summary>
    /// <param name="driverInterfaceVersion">The version number of the interface exposed by the USB-UIRT driver and consumed by the USB-UIRT driver API (uuirtdrv.dll).</param>
    /// <returns><c>true</c> if the driver information is retrieved successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTGetDrvInfo(out uint driverInterfaceVersion);

    /// <summary>
    /// Get the version number of the USB-UIRT driver API (uuirtdrv.dll).
    /// </summary>
    /// <param name="apiVersion">The version number of the USB-UIRT driver API.</param>
    /// <returns><c>true</c> if the version number is retrieved successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTGetDrvVersion(out uint apiVersion);

    /// <summary>
    /// Open communication with the first USB-UIRT.  
    /// </summary>
    /// <returns>a valid device handle if successful, otherwise <c>INVALID_HANDLE_VALUE</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    private static extern IntPtr UUIRTOpen();

    /// <summary>
    /// Open communication with a specific USB-UIRT.  
    /// </summary>
    /// <param name="name">The name of the target USB-UIRT.</param>
    /// <param name="options">Options relevant for establishing communication.</param>
    /// <param name="reserved0">Reserved for future expansion. Set to <c>IntPtr.Zero</c>.</param>
    /// <param name="reserved1">Reserved for future expansion. Set to <c>IntPtr.Zero</c>.</param>
    /// <returns>a valid device handle if successful, otherwise <c>INVALID_HANDLE_VALUE</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr UUIRTOpenEx(string name, OpenExOption options, IntPtr reserved0, IntPtr reserved1);

    /// <summary>
    /// Terminate communication with a USB-UIRT.
    /// </summary>
    /// <param name="handle">The handle allocated to the USB-UIRT when it was opened.</param>
    /// <returns><c>true</c> if communication is terminated successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTClose(IntPtr handle);

    /// <summary>
    /// Retrieve information about a USB-UIRT's firmware.
    /// </summary>
    /// <param name="handle">The handle allocated to a USB-UIRT when it was opened.</param>
    /// <param name="info">A structure containing the firmware information.</param>
    /// <returns><c>true</c> if the firmware information is retreived successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTGetUUIRTInfo(IntPtr handle, ref UsbUirtInfo info);

    /// <summary>
    /// Retrieve a USB-UIRT's current configuration.
    /// </summary>
    /// <param name="handle">The handle allocated to a USB-UIRT when it was opened.</param>
    /// <param name="config">The configuration, encoded with one bit to represent each option.</param>
    /// <returns><c>true</c> if the configuration is retrieved successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTGetUUIRTConfig(IntPtr handle, out ConfigOption config);

    /// <summary>
    /// Set a USB-UIRT's configuration.
    /// </summary>
    /// <param name="handle">The handle allocated to the USB-UIRT when it was opened.</param>
    /// <param name="config">The configuration, encoded with one bit to represent each option.</param>
    /// <returns><c>true</c> if the configuration is set successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTSetUUIRTConfig(IntPtr handle, ConfigOption config);

    /// <summary>
    /// Use a USB-UIRT to transmit an infra red signal.
    /// </summary>
    /// <param name="handle">The handle allocated to the USB-UIRT when it was opened.</param>
    /// <param name="code">The encoded representation of the signal to be transmitted.</param>
    /// <param name="codeFormat">The encoding format of <paramref name="code"/>.</param>
    /// <param name="repeatCount">The number of times to repeat the transmission. For 2-part codes, the first part is transmitted once followed by repeatCount transmissions of the second part.</param>
    /// <param name="preTransmitDelay">A delay to apply in the case when infra red signal is detected before transmission. The unit is milli-seconds (ms).</param>
    /// <param name="asyncCompleteEvent">An optional pointer to an event handle enabling asynchronous implementation. If supplied, the calling thread will not be blocked and the event will be signalled when transmission is complete. Otherwise the calling thread will be blocked until transmission is complete.</param>
    /// <param name="reserved0">Reserved for future expansion. Set to <c>IntPtr.Zero</c>.</param>
    /// <param name="reserved1">Reserved for future expansion. Set to <c>IntPtr.Zero</c>.</param>
    /// <returns><c>true</c> if the signal is transmitted successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTTransmitIR(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string code, CodeFormat codeFormat, int repeatCount, int preTransmitDelay, IntPtr asyncCompleteEvent, IntPtr reserved0, IntPtr reserved1);

    /// <summary>
    /// Instruct a USB-UIRT to start learning an infra red signal.
    /// </summary>
    /// <param name="handle">The handle allocated to the USB-UIRT when it was opened.</param>
    /// <param name="codeFormat">The format the driver should use to encode the learned signal.</param>
    /// <param name="codeBuffer">A caller-allocated buffer to hold the learned signal. The buffer format is determined by <paramref name="codeFormat"/>. Size recommendation is at least 2048 bytes.</param>
    /// <param name="learnProgressCallBack">A delegate that will be invoked periodically during the learning process to indicate progress.</param>
    /// <param name="callBackContext">Optional context that will be forwarded to the <paramref name="learnProgressCallBack"/> delegate when it is invoked.</param>
    /// <param name="abortFlag">A pointer to a BOOL. Set the BOOL to a non-zero value to abort the learning process.</param>
    /// <param name="param1">When <paramref name="codeFormat"/> includes the LearnForceFrequency flag, param1 specifies the carrier frequency. The unit is Hertz (Hz). In all other cases this parameter is currently not used.</param>
    /// <param name="reserved0">Reserved for future expansion. Set to <c>IntPtr.Zero</c>.</param>
    /// <param name="reserved1">Reserved for future expansion. Set to <c>IntPtr.Zero</c>.</param>
    /// <returns><c>true</c> if the signal is learned successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTLearnIR(IntPtr handle, CodeFormat codeFormat, [MarshalAs(UnmanagedType.LPStr)] StringBuilder codeBuffer, OnUsbUirtLearnProgress learnProgressCallBack, IntPtr callBackContext, IntPtr abortFlag, uint param1, IntPtr reserved0, IntPtr reserved1);

    /// <summary>
    /// Register a delegate for the USB-UIRT driver to invoke when an infra red
    /// signal is detected.
    /// </summary>
    /// <remarks>
    /// Set <paramref name="receiveCallBack"/> to <c>null</c> to unregister the
    /// previously registered delegate.
    /// </remarks>
    /// <param name="handle">The handle allocated to the USB-UIRT when it was opened.</param>
    /// <param name="receiveCallBack">The delegate that will be invoked when an infra red signal is detected.</param>
    /// <param name="callBackContext">Optional context that will be forwarded to the delegate when it is invoked.</param>
    /// <returns><c>true</c> if the delegate is registered successfully, otherwise <c>false</c></returns>
    [DllImport("uuirtdrv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UUIRTSetReceiveCallback(IntPtr handle, OnUsbUirtReceive receiveCallBack, IntPtr callBackContext);

    #endregion

    #region delegate definitions

    /// <summary>
    /// Invoked periodically during the learning process.
    /// </summary>
    /// <param name="progress">A measure of the learning process's progress. The unit is percent (%).</param>
    /// <param name="signalQuality">A measure of the infra red signal quality.</param>
    /// <param name="carrierFrequency">The detected infra red signal carrier frequency. The unit is Hertz (Hz).</param>
    /// <param name="context">Context supplied to UUIRTLearnIR().</param>
    private delegate void OnUsbUirtLearnProgress(uint progress, uint signalQuality, uint carrierFrequency, IntPtr context);

    /// <summary>
    /// Invoked when an infra red signal is detected.
    /// </summary>
    /// <param name="code">A unique, twelve character (hexadecimal representation of 6 bytes???) string that represents the detected signal. The encoding format is "like IRMAN".</param>
    /// <param name="context">Context supplied to UUIRTSetReceiveCallback().</param>
    private delegate void OnUsbUirtReceive([MarshalAs(UnmanagedType.LPStr)] string code, IntPtr userData);

    #endregion

    #region enums

    [Flags]
    private enum CodeFormat : int
    {
      Uirt = 0,                         // UUIRTDRV_IRFMT_UUIRT - defaults to "raw" format
      Pronto = 0x10,                    // UUIRTDRV_IRFMT_PRONTO

      TransmitDc = 0x0080,              // UUIRTDRV_IRFMT_TRANSMIT_DC

      LearnForceRaw = 0x0100,           // UUIRTDRV_IRFMT_LEARN_FORCERAW - UIRT raw
      LearnForceStruct = 0x0200,        // UUIRTDRV_IRFMT_LEARN_FORCESTRUC - UIRT struct (compressed); smallest memory foot-print, but limits encoding flexibility
      LearnForceFrequency = 0x0400,     // UUIRTDRV_IRFMT_LEARN_FORCEFREQ - learn with known carrier frequency
      LearnFrequencyDetect = 0x0800,    // UUIRTDRV_IRFMT_LEARN_FREQDETECT

      LearnUir = 0x4000,                // UUIRTDRV_IRFMT_LEARN_UIR - learn with same code format as receive (not transmittable)
      LearnDebug = 0x8000               // UUIRTDRV_IRFMT_LEARN_DEBUG
    }

    private enum DriverError
    {
      NoDeviceFound = 0x20000001,       // UUIRTDRV_ERR_NO_DEVICE
      NoResponse = 0x20000002,          // UUIRTDRV_ERR_NO_RESP
      NoDll = 0x20000003,               // UUIRTDRV_ERR_NO_DLL - missing the API DLL (uuirtdrv.dll)
      IncompatibleVersion = 0x20000004, // UUIRTDRV_ERR_VERSION - firmware and API versions are not compatible
      InUse = 0x20000005,               // UUIRTDRV_ERR_IN_USE

      OperationAborted = 0x200000fc,    // ERROR_OPERATION_ABORTED
      IoPending = 0x200000fd,           // ERROR_IO_PENDING
      InvalidParameter = 0x200000fe,    // ERROR_INVALID_PARAMETER
      InvalidHandle = 0x200000ff        // ERROR_INVALID_HANDLE
    }

    [Flags]
    private enum OpenExOption : uint
    {
      None = 0,
      Exclusive = 1   // UUIRTDRV_OPENEX_ATTRIBUTE_EXCLUSIVE
    }

    [Flags]
    private enum ConfigOption : uint
    {
      None = 0,
      IndicateReceive = 0x01,           // UUIRTDRV_CFG_LEDRX
      IndicateTransmit = 0x02,          // UUIRTDRV_CFG_LEDTX
      LegacyUirtReceiveFormat = 0x04,   // UUIRTDRV_CFG_LEGACYRX - receive 6 characters instead of 12???
      Reserved0 = 0x08,
      Reserved1 = 0x10
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    private struct UsbUirtInfo
    {
      public uint FirmWareVersion;
      public uint ProtocolVersion;
      public byte FirmWareDay;
      public byte FirmWareMonth;
      public byte FirmWareYear;
    }

    #endregion

    #region constants

    public const uint MAXIMUM_DEVICE_INDEX = 7;

    #endregion

    #region variables

    private IntPtr _handle = IntPtr.Zero;
    private bool _isOpen = false;
    private uint _index = 0;
    private string _name = null;
    private TransmitZone _availableTransmitZones = TransmitZone.All;
    private IntPtr _learnAbortFlag = IntPtr.Zero;

    #endregion

    public Driver(uint deviceIndex)
    {
      _index = deviceIndex;
      _name = GetDeviceNameFromIndex(deviceIndex);
    }

    #region properties

    public uint Index
    {
      get
      {
        return _index;
      }
    }

    public string Name
    {
      get
      {
        return _name;
      }
    }

    #endregion

    /// <summary>
    /// Determine if a particular device is currently connected.
    /// </summary>
    /// <param name="deviceIndex">The device's index number.</param>
    /// <returns><c>true</c> if the device is connected, otherwise <c>false</c></returns>
    public static bool IsConnected(uint deviceIndex)
    {
      try
      {
        IntPtr handle = UUIRTOpenEx(GetDeviceNameFromIndex(deviceIndex), OpenExOption.None, IntPtr.Zero, IntPtr.Zero);
        if (handle == IntPtr.Zero || handle == NativeMethods.INVALID_HANDLE_VALUE)
        {
          return Marshal.GetLastWin32Error() != (int)DriverError.NoDeviceFound;
        }
        UUIRTClose(handle);
        return true;
      }
      catch
      {
        // Assume driver not installed.
        return false;
      }
    }

    /// <summary>
    /// Get the default (uurename) device name associated with a given index.
    /// </summary>
    /// <param name="index">The device index number.</param>
    /// <returns>the device name associated with <paramref name="index"/></returns>
    private static string GetDeviceNameFromIndex(uint index)
    {
      if (index == 0)
      {
        return "USB-UIRT";
      }
      return string.Format("USB-UIRT-{0}", index + 1);
    }

    /// <summary>
    /// Open the device.
    /// </summary>
    /// <returns><c>true</c> if the device is opened successfully, otherwise <c>false</c></returns>
    public bool Open()
    {
      this.LogDebug("USB-UIRT driver: open, device = {0}", _name);
      if (_isOpen)
      {
        this.LogWarn("USB-UIRT driver: already open, device = {0}", _name);
        return true;
      }

      this.LogDebug("USB-UIRT driver: get driver information");
      uint version;
      try
      {
        if (UUIRTGetDrvInfo(out version))
        {
          this.LogDebug("  interface version = {0}.{1}.{2}", version >> 8, (version >> 4) & 0xf, version & 0xf);
        }
        else
        {
          this.LogWarn("USB-UIRT driver: failed to get driver info, error = {0}", Marshal.GetLastWin32Error());
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "USB-UIRT driver: failed to get driver info, is the driver installed?");
        return false;
      }

      this.LogDebug("USB-UIRT driver: get driver version");
      _availableTransmitZones = TransmitZone.All;
      if (UUIRTGetDrvVersion(out version))
      {
        this.LogDebug("  API version    = {0}", version);

        // Non-blocking learn support?
        // Available in API driver 2.6.1.0 or later.

        // Multi-zone support?
        // http://www.usbuirt.com/phpBB3/viewtopic.php?t=266
        // USB-UIRT's manufactured since Mid-February 2004 have added multi-zone
        // support in the hardware. The serial number on the back of the unit
        // will have a small letter 'Z' after it if multi-zone commands are
        // supported. API driver 2.6.9.2 or later is also required. We have no
        // way to access the serial number, so we only consider the driver
        // version.
        this.LogDebug("  is multi-zone? = {0}", version >= 2692);
        if (version >= 2692)
        {
          _availableTransmitZones = TransmitZone.All | TransmitZone.Zone1 | TransmitZone.Zone2 | TransmitZone.Zone3;
        }
      }
      else
      {
        this.LogWarn("USB-UIRT driver: failed to get driver version, error = {0}", Marshal.GetLastWin32Error());
      }

      _handle = UUIRTOpenEx(_name, OpenExOption.None, IntPtr.Zero, IntPtr.Zero);
      if (_handle == IntPtr.Zero || _handle == NativeMethods.INVALID_HANDLE_VALUE)
      {
        _handle = IntPtr.Zero;
        this.LogError("USB-UIRT driver: failed to open handle (is this device being used by other software?), error = {0}, device = {1}", (DriverError)Marshal.GetLastWin32Error(), _name);
        return false;
      }

      if (!UUIRTSetReceiveCallback(_handle, OnReceive, _handle))
      {
        this.LogError("USB-UIRT driver: failed to set receive call-back, error = {0}, device = {1}", Marshal.GetLastWin32Error(), _name);
        UUIRTClose(_handle);
        _handle = IntPtr.Zero;
        return false;
      }

      _isOpen = true;
      _learnAbortFlag = Marshal.AllocHGlobal(sizeof(int));

      this.LogDebug("USB-UIRT driver: get hardware info");
      UsbUirtInfo info = new UsbUirtInfo();
      if (UUIRTGetUUIRTInfo(_handle, ref info))
      {
        this.LogDebug("  firmware version = {0}", info.FirmWareVersion);
        this.LogDebug("  protocol version = {0}", info.ProtocolVersion);
        this.LogDebug("  firmware date    = {0}", new DateTime(2000 + info.FirmWareYear, info.FirmWareMonth, info.FirmWareDay).ToString("yyyy-MM-dd"));
      }
      else
      {
        this.LogWarn("USB-UIRT driver: failed to get hardware info, error = {0}", Marshal.GetLastWin32Error());
      }

      this.LogDebug("USB-UIRT driver: get config");
      ConfigOption config;
      if (UUIRTGetUUIRTConfig(_handle, out config))
      {
        this.LogDebug("  config = {0}", config);
      }
      else
      {
        this.LogWarn("USB-UIRT driver: failed to get config, error = {0}", Marshal.GetLastWin32Error());
      }
      return true;
    }

    /// <summary>
    /// Close the device.
    /// </summary>
    public void Close()
    {
      if (!_isOpen)
      {
        return;
      }
      this.LogDebug("USB-UIRT driver: close, device = {0}", _name);

      // Cancel learning.
      Marshal.WriteInt32(_learnAbortFlag, 0, 1);

      if (!UUIRTSetReceiveCallback(_handle, null, IntPtr.Zero))
      {
        this.LogWarn("USB-UIRT driver: failed to clear receive call-back, error = {0}, device = {1}", Marshal.GetLastWin32Error(), _name);
      }
      if (!UUIRTClose(_handle))
      {
        this.LogWarn("USB-UIRT driver: failed to close handle, error = {0}, device = {1}", Marshal.GetLastWin32Error(), _name);
      }

      Marshal.FreeHGlobal(_learnAbortFlag);
      _handle = IntPtr.Zero;
      _isOpen = false;
    }

    /// <summary>
    /// Transmit an IR command.
    /// </summary>
    /// <param name="commandName">A human-readable name for the command.</param>
    /// <param name="commandString">The command to transmit as a Pronto-formatted string.</param>
    /// <param name="zones">The zone(s) in which the command should be transmitted.</param>
    /// <returns>the result of the transmit process</returns>
    public TransmitResult Transmit(string commandName, string commandString, TransmitZone zones)
    {
      this.LogDebug("USB-UIRT driver: transmit, command = {0}, zone(s) = [{1}], device = {2}", commandName, zones, _name);

      if (!_isOpen)
      {
        this.LogError("USB-UIRT driver: failed to transmit, the device is not open, device = {0}", _name);
        return TransmitResult.NotOpen;
      }
      if (!_availableTransmitZones.HasFlag(zones))
      {
        this.LogError("Microsoft blaster driver: failed to transmit, the specified zone(s) are not available, specified zone(s) = [{0}], available zones = [{1}], device = {2}", zones, _availableTransmitZones, _name);
        return TransmitResult.ZoneNotAvailable;
      }

      int carrierFrequency;
      int[] timingData;
      if (!Pronto.Decode(commandString, out carrierFrequency, out timingData))
      {
        this.LogError("USB-UIRT driver: failed to transmit, the command is not valid, command name = {0}, command = {1}", commandName, commandString);
        return TransmitResult.InvalidCommand;
      }

      // Truncate inherent inter-command delay. The caller controls the delay
      // based on configuration.
      timingData[timingData.Length - 1] = 1;

      if (carrierFrequency < 0)
      {
        carrierFrequency = 36000;   // 36 kHz = RC-5 and RC-6 carrier frequency
        this.LogWarn("USB-UIRT driver: carrier frequency not specified, using default {0} Hz", carrierFrequency);
      }

      // Re-encode. In theory there should be little if any difference between
      // the original command and the re-encoded command. However, re-encoding
      // ensures that we don't pass through non-raw (eg. RC-5, RC-6, NEC)
      // commands which the USB-UIRT API/driver doesn't support.
      commandString = Pronto.EncodeRaw(carrierFrequency, timingData);
      if (commandString == null)
      {
        this.LogError("USB-UIRT driver: failed to Pronto-encode command for transmission, command name = {0}, command = {1}", commandName, commandString);
        return TransmitResult.InvalidCommand;
      }

      if (!zones.HasFlag(TransmitZone.All))
      {
        if (zones == TransmitZone.Zone1)
        {
          commandString = "Z1" + commandString;
        }
        else if (zones == TransmitZone.Zone2)
        {
          commandString = "Z2" + commandString;
        }
        else if (zones == TransmitZone.Zone3)
        {
          commandString = "Z3" + commandString;
        }
      }

      ManualResetEvent completeEvent = new ManualResetEvent(false);
      try
      {
        // Note: repeat once for safety. I'm not confident that the driver
        // would send the command once if repeat count is set to zero.
        if (!UUIRTTransmitIR(_handle, commandString, CodeFormat.Pronto, 1, 0, completeEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, IntPtr.Zero))
        {
          this.LogError("USB-UIRT driver: transmit failed, command name = {0}, zone(s) = [{1}], device = {2}, command = {3}", commandName, zones, _name, commandString);
          return TransmitResult.Fail;
        }
        if (!completeEvent.WaitOne(10000))
        {
          this.LogError("USB-UIRT driver: transmit time limit reached, time limit = 10 ms, command name = {0}, zone(s) = [{1}], device = {2}, command = {3}", commandName, zones, _name, commandString);
          return TransmitResult.TimeOut;
        }
        return TransmitResult.Success;
      }
      finally
      {
        completeEvent.Close();
        completeEvent.Dispose();
      }
    }

    /// <summary>
    /// Learn an IR command.
    /// </summary>
    /// <param name="timeLimit">The time limit for the learning process.</param>
    /// <param name="command">The learnt command as a Pronto-formatted string.</param>
    /// <returns>the result of the learning process</returns>
    public LearnResult Learn(TimeSpan timeLimit, out string command)
    {
      this.LogDebug("USB-UIRT driver: learn, time limit = {0} ms, device = {1}", timeLimit.TotalMilliseconds, _name);
      command = null;

      if (!_isOpen)
      {
        this.LogError("USB-UIRT driver: failed to learn, the device is not open, device = {0}", _name);
        return LearnResult.NotOpen;
      }

      StringBuilder buffer = new StringBuilder(4096);
      Marshal.WriteInt32(_learnAbortFlag, 0, 0);
      bool result = false;
      ManualResetEvent completeEvent = new ManualResetEvent(false);
      try
      {
        ThreadPool.QueueUserWorkItem(delegate
        {
          result = UUIRTLearnIR(_handle, CodeFormat.Pronto, buffer, OnLearnProgress, _handle, _learnAbortFlag, 0, IntPtr.Zero, IntPtr.Zero);
          completeEvent.Set();
        });

        if (!completeEvent.WaitOne(timeLimit))
        {
          Marshal.WriteInt32(_learnAbortFlag, 0, 1);
          bool abortSucceeded = completeEvent.WaitOne(timeLimit);
          this.LogError("USB-UIRT driver: learn time limit reached, time limit = {0} ms, abort succeeded = {1}, device = {2}", timeLimit.TotalMilliseconds, abortSucceeded, _name);
          return LearnResult.TimeOut;
        }

        if (!result)
        {
          this.LogError("USB-UIRT driver: failed to learn, error = {0}, device = {1}", Marshal.GetLastWin32Error(), _name);
          return LearnResult.Fail;
        }

        command = buffer.ToString();
        this.LogDebug("USB-UIRT driver: learning succeeded, command = {0}", command);
        return LearnResult.Success;
      }
      finally
      {
        completeEvent.Close();
        completeEvent.Dispose();
      }
    }

    public UsbUirtDetail GetDetail()
    {
      UsbUirtDetail detail = new UsbUirtDetail();
      detail.Name = _name;
      detail.Index = (int)_index;
      detail.TransmitZones = _availableTransmitZones;
      return detail;
    }

    private void OnLearnProgress(uint progress, uint signalQuality, uint carrierFrequency, IntPtr userData)
    {
      this.LogDebug("USB-UIRT driver: learn progress, device = {0}, progress = {1}%, signal quality = {2}, carrier frequency = {3} Hz", _name, progress, signalQuality, carrierFrequency);
    }

    private void OnReceive(string code, IntPtr userData)
    {
      this.LogDebug("USB-UIRT driver: remote control key press, device = {0}, code = {1}", _name, code);
    }
  }
}