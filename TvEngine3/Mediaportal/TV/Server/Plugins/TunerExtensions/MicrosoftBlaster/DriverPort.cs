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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using Microsoft.Win32.SafeHandles;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  /// <summary>
  /// This class supports the port driver interface exposed by the eHome driver
  /// supplied with Windows Vista and newer.
  /// </summary>
  /// <remarks>
  /// For technical information about this code refer to the specifications
  /// ("Remote Control and Receiver-Transceiver Specifications and Requirements
  /// for Windows Media Center in Windows Operating Systems", "Port Driver
  /// Requirements" section):
  /// http://download.microsoft.com/download/0/7/E/07EF37BB-33EA-4454-856A-7D663BC109DF/Windows-Media-Center-RC-IR-Collection-Green-Button-Specification-03-08-2011-V2.pdf
  /// </remarks>
  internal class DriverPort : Driver
  {
    #region enums

    [Flags]
    private enum DeviceCapability
    {
      // Protocol v1.
      SupportsLegacySigning = 0x0001,   // DEV_CAPS_SUPPORTS_LEGACY_SIGNING
      HasUniqueSerial = 0x0002,         // DEV_CAPS_HAS_UNIQUE_SERIAL
      CanFlashReceiverLed = 0x0004,     // DEV_CAPS_CAN_FLASH_RECEIVER_LED
      IsLegacy = 0x0008,                // DEV_CAPS_IS_LEGACY

      // Protocol v2.
      SupportsWake = 0x0010,            // V2_DEV_CAPS_SUPPORTS_WAKE
      MultipleWake = 0x0020,            // V2_DEV_CAPS_MULTIPLE_WAKE
      ProgrammableWake = 0x0040,        // V2_DEV_CAPS_PROGRAMMABLE_WAKE
      VolatileWakePattern = 0x0080,     // V2_DEV_CAPS_VOLATILE_WAKE_PATTERN
      LearningOnly = 0x0100,            // V2_DEV_CAPS_LEARNING_ONLY
      NarrowBpf = 0x0200,               // V2_DEV_CAPS_NARROW_BPF
      NoSoftwareDecodeInput = 0x0400,   // V2_DEV_CAPS_NO_SWDECODE_INPUT
      HardwareDecodeInput = 0x0800,     // V2_DEV_CAPS_HWDECODE_INPUT
      EmulatorVersion1 = 0x1000,        // V2_DEV_CAPS_EMULATOR_V1
      EmulatorVersion2 = 0x2000,        // V2_DEV_CAPS_EMULATOR_V2
      AttachedToTuner = 0x4000          // V2_DEV_CAPS_ATTACHED_TO_TUNER
    }

    [Flags]
    private enum TransmitMode
    {
      Normal = 0,
      PulseMode = 1,    // TRANSMIT_FLAGS_PULSE_MODE
      DcMode = 2        // TRANSMIT_FLAGS_DC_MODE
    }

    // From Irclass_ioctl.h
    // https://msdn.microsoft.com/en-us/library/ff539494%28v=vs.85%29.aspx
    private class IoControlCode
    {
      private const NativeMethods.FileDevice FILE_DEVICE_IRCLASS = (NativeMethods.FileDevice)0x0f60;

      public static readonly uint IOCTL_IR_GET_DEV_CAPS = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 1, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_READ_ACCESS);
      public static readonly uint IOCTL_IR_GET_EMITTERS = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 2, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_READ_ACCESS);
      public static readonly uint IOCTL_IR_FLASH_RECEIVER = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 3, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_RESET_DEVICE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 4, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_TRANSMIT = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 5, NativeMethods.Method.METHOD_IN_DIRECT, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_RECEIVE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 6, NativeMethods.Method.METHOD_OUT_DIRECT, NativeMethods.FileAccess.FILE_READ_ACCESS);

      public static readonly uint IOCTL_IR_PRIORITY_RECEIVE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 8, NativeMethods.Method.METHOD_OUT_DIRECT, NativeMethods.FileAccess.FILE_READ_ACCESS);
      public static readonly uint IOCTL_IR_HANDSHAKE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 9, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_ANY_ACCESS);
      public static readonly uint IOCTL_IR_ENTER_PRIORITY_RECEIVE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 10, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_EXIT_PRIORITY_RECEIVE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 11, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_USER_OPEN = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 12, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_USER_CLOSE = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 13, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
      public static readonly uint IOCTL_IR_SET_WAKE_PATTERN = NativeMethods.CTL_CODE(FILE_DEVICE_IRCLASS, 14, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_WRITE_ACCESS);
    }

    #endregion

    #region variables

    private int _portIndexLearn = -1;
    private int _portIndexReceive = -1;
    private int _transmitPortCount = 0;

    private SafeFileHandle _handle = null;
    private bool _isReceiving = false;
    private int _carrierFrequency = -1;

    // Almost all the I/O struct members are of type ULONG_PTR. The size (in
    // bytes) of a ULONG_PTR varies depending on whether the operating system
    // is 32 or 64 bit. Unfortunately marshaling is a bit of a pain because the
    // kernel can't do the thunking for us. We marshal manually for each IOCTL.
    // https://msdn.microsoft.com/en-us/library/ff563897.aspx
    // http://stackoverflow.com/questions/732642/using-64-bits-driver-from-32-bits-application
    private readonly int _sizeOfUlongPtr = 4;    // sizeof(ULONG_PTR) for 32 bit operating systems

    #endregion

    public DriverPort(string devicePath)
      : base(devicePath)
    {
      if (Environment.Is64BitOperatingSystem)
      {
        _sizeOfUlongPtr = 8;
      }
    }

    #region abstract Driver function implementation

    /// <summary>
    /// Open access handle(s).
    /// </summary>
    /// <returns><c>true</c> if the handle(s) are opened successfully, otherwise <c>false</c></returns>
    protected override bool OpenHandles()
    {
      _handle = NativeMethods.CreateFile(_devicePath, NativeMethods.AccessMask.GENERIC_READ | NativeMethods.AccessMask.GENERIC_WRITE, FileShare.None,
                                          IntPtr.Zero, FileMode.Open, (uint)NativeMethods.FileFlag.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
      if (_handle.IsInvalid)
      {
        _handle = null;
        this.LogError("Microsoft blaster port driver: failed to open handle (is this device being used by other software?), error = {0}, device path = {1}", Marshal.GetLastWin32Error(), _devicePath);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Close the access handle(s).
    /// </summary>
    protected override void CloseHandles()
    {
      try
      {
        StopReceiving();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster port driver: stop IOCTL failed");
        _isReceiving = false;
      }

      if (_handle != null)
      {
        _handle.Close();
        _handle.Dispose();
        _handle = null;
      }
    }

    /// <summary>
    /// Get the device's capabilities.
    /// </summary>
    protected override void GetCapabilities()
    {
      _transmitPortCount = 0;
      _portIndexLearn = -1;
      _portIndexReceive = -1;

      int tunerPnpIdWcharCount = 256;
      int bufferSize = (6 * _sizeOfUlongPtr) + (tunerPnpIdWcharCount * 2);
      IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
      try
      {
        IoControl(IoControlCode.IOCTL_IR_GET_DEV_CAPS, IntPtr.Zero, 0, buffer, (uint)bufferSize);

        int protocolVersion = Marshal.ReadInt32(buffer, 0);
        _transmitPortCount = Marshal.ReadInt32(buffer, _sizeOfUlongPtr);
        int portCountReceive = Marshal.ReadInt32(buffer, _sizeOfUlongPtr * 2);
        int learningReceiverMask = Marshal.ReadInt32(buffer, _sizeOfUlongPtr * 3);
        DeviceCapability capabilities = (DeviceCapability)Marshal.ReadInt32(buffer, _sizeOfUlongPtr * 4);
        WakeProtocol wakeProtocols = 0;
        string tunerPnpId = string.Empty;
        if (protocolVersion == 0x200) // version 2.0.0
        {
          wakeProtocols = (WakeProtocol)Marshal.ReadInt32(buffer, _sizeOfUlongPtr * 5);
          tunerPnpId = Marshal.PtrToStringUni(IntPtr.Add(buffer, _sizeOfUlongPtr * 6), tunerPnpIdWcharCount);
        }

        for (int p = 0; p < _transmitPortCount; p++)
        {
          _availablePortsTransmit |= GetPortByIndex(p);
        }

        if (portCountReceive > 0)
        {
          if (!capabilities.HasFlag(DeviceCapability.LearningOnly))
          {
            for (int p = 0; p < portCountReceive; p++)
            {
              if ((learningReceiverMask & (1 << p)) == 0)
              {
                _portIndexReceive = p;
                break;
              }
            }
          }

          for (int p = 0; p < portCountReceive; p++)
          {
            if ((learningReceiverMask & (1 << p)) != 0)
            {
              _portIndexLearn = p;
              break;
            }
          }
        }

        this.LogDebug("  protocol version           = {0}", protocolVersion);
        this.LogDebug("  available transmit port(s) = {0}", _availablePortsTransmit);
        this.LogDebug("  available receive port(s)  = {0}", _availablePortsReceive);
        this.LogDebug("  learning receiver mask     = 0x{0:x}", learningReceiverMask);
        this.LogDebug("  device capabilities        = {0}", capabilities);
        this.LogDebug("  wake protocols             = {0}", wakeProtocols);
        this.LogDebug("  tuner PNP ID               = {0}", tunerPnpId);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster port driver: assuming default transceiver capabilities");
        _availablePortsReceive = ReceivePort.Receive | ReceivePort.Learn;
        _availablePortsTransmit = TransmitPort.Port1 | TransmitPort.Port2;
        _portIndexReceive = 0;
        _portIndexLearn = 1;
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }
    }

    #region transmit

    /// <summary>
    /// Update the status for each transmit port.
    /// </summary>
    protected override void UpdateTransmitPortStatus()
    {
      uint portMask = 0xffffffff;
      IntPtr buffer = Marshal.AllocHGlobal(_sizeOfUlongPtr);
      try
      {
        IoControl(IoControlCode.IOCTL_IR_GET_EMITTERS, IntPtr.Zero, 0, buffer, (uint)_sizeOfUlongPtr);
        portMask = (uint)Marshal.ReadInt32(buffer, 0);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster port driver: assuming all transmit ports have an emitter connected");
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }

      for (int p = 0; p < _transmitPortCount; p++)
      {
        TransmitPort port = GetPortByIndex(p);
        bool isEmitterConnected = (portMask & GetTransmitPortMask(port)) != 0;
        if (_connectedTransmitPorts.HasFlag(port) && !isEmitterConnected)
        {
          this.LogDebug("Microsoft blaster port driver: transmit port {0}'s emitter has been disconnected", p + 1);
          _connectedTransmitPorts &= ~port;
        }
        else if (!_connectedTransmitPorts.HasFlag(port) && isEmitterConnected)
        {
          this.LogDebug("Microsoft blaster port driver: transmit port {0} has an emitter connected", p + 1);
          _connectedTransmitPorts |= port;
        }
      }
    }

    /// <summary>
    /// Use the device to transmit an IR command.
    /// </summary>
    /// <param name="portMask">A bit-mask indicating which port(s) to transmit on.</param>
    /// <param name="carrierFrequency">The carrier frequency to use when modulating the command.</param>
    /// <param name="command">The command, encoded as a sequence of pulse and space durations.</param>
    protected override void Transmit(uint portMask, uint carrierFrequency, int[] command)
    {
      int dataByteCount = command.Length * sizeof(int);
      int bufferSizeTransmitChunk = (3 * _sizeOfUlongPtr) + dataByteCount;

      // Transmit chunk buffer size must be a multiple of sizeof(ULONG_PTR).
      int padding = bufferSizeTransmitChunk % _sizeOfUlongPtr;
      if (padding != 0)
      {
        bufferSizeTransmitChunk += (_sizeOfUlongPtr - padding);
      }

      int bufferSizeTransmitParams = 4 * _sizeOfUlongPtr;
      IntPtr bufferTransmitParams = Marshal.AllocHGlobal(bufferSizeTransmitParams);
      IntPtr bufferTransmitChunk = Marshal.AllocHGlobal(bufferSizeTransmitChunk);
      try
      {
        Marshal.WriteInt32(bufferTransmitParams, 0, (int)portMask);
        if (carrierFrequency == 0)  // DC (no carrier)
        {
          // DC and/or pulse mode probably won't work. The documentation for
          // the flags and pulse size members is incomplete and essentially
          // useless.
          Marshal.WriteInt32(bufferTransmitParams, _sizeOfUlongPtr, 0);   // carrier period
          Marshal.WriteInt32(bufferTransmitParams, _sizeOfUlongPtr * 2, (int)TransmitMode.PulseMode);
        }
        else
        {
          Marshal.WriteInt32(bufferTransmitParams, _sizeOfUlongPtr, (int)Math.Round(1000000.0 / carrierFrequency));  // frequency to period in micro-seconds
          Marshal.WriteInt32(bufferTransmitParams, _sizeOfUlongPtr * 2, (int)TransmitMode.Normal);
        }
        Marshal.WriteInt32(bufferTransmitParams, _sizeOfUlongPtr, 0);     // pulse size (pulse mode only)

        Marshal.WriteInt32(bufferTransmitChunk, 0, 0);                    // offset to next chunk
        Marshal.WriteInt32(bufferTransmitChunk, _sizeOfUlongPtr, 1);      // repeat count
        Marshal.WriteInt32(bufferTransmitChunk, _sizeOfUlongPtr * 2, dataByteCount);
        Marshal.Copy(command, 0, IntPtr.Add(bufferTransmitChunk, _sizeOfUlongPtr * 3), command.Length);

        IoControl(IoControlCode.IOCTL_IR_TRANSMIT, bufferTransmitParams, (uint)bufferSizeTransmitParams, bufferTransmitChunk, (uint)bufferSizeTransmitChunk);
      }
      finally
      {
        Marshal.FreeHGlobal(bufferTransmitParams);
        Marshal.FreeHGlobal(bufferTransmitChunk);
      }
    }

    #endregion

    #region receive/learn

    /// <summary>
    /// Get the size of buffer to create for receiving.
    /// </summary>
    protected override int GetReceiveBufferSize()
    {
      int bufferSize = (3 * _sizeOfUlongPtr) + RECEIVE_BUFFER_SIZE;

      // Buffer size may need to be a multiple of sizeof(ULONG_PTR).
      int padding = bufferSize % _sizeOfUlongPtr;
      if (padding != 0)
      {
        bufferSize += (_sizeOfUlongPtr - padding);
      }
      return bufferSize;
    }

    /// <summary>
    /// Set the receive configuration.
    /// </summary>
    /// <param name="port">The port to receive from.</param>
    /// <param name="timeOut">The receive time-out.</param>
    protected override void SetReceiveConfiguration(ReceivePort port, TimeSpan timeOut)
    {
      StopReceiving();

      int portIndex = _portIndexReceive;
      if (port == ReceivePort.Learn)
      {
        portIndex = _portIndexLearn;
      }

      int bufferSize = 2 * _sizeOfUlongPtr;
      IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
      try
      {
        Marshal.WriteInt32(buffer, 0, portIndex);
        Marshal.WriteInt32(buffer, _sizeOfUlongPtr, (int)timeOut.TotalMilliseconds);
        IoControl(IoControlCode.IOCTL_IR_ENTER_PRIORITY_RECEIVE, buffer, (uint)bufferSize, IntPtr.Zero, 0);
        _isReceiving = true;
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }
    }

    /// <summary>
    /// Receive IR timing and/or command response data from the device.
    /// </summary>
    /// <param name="receiveBuffer">A buffer to use to hold received data.</param>
    /// <param name="receiveBufferSize">The size of <paramref name="receiveBuffer"/>.</param>
    /// <param name="overlappedBuffer">A buffer containing an OVERLAPPED structure which can be used by an asynchronous implementation.</param>
    /// <param name="completeEvent">An event that can be used by an asynchronous implementation.</param>
    /// <param name="abortEvent">An event that will be signalled when receiving should be aborted.</param>
    /// <param name="timingData">The IR timing data received from the device, if any; <c>null</c> if no data was available.</param>
    protected override void Receive(IntPtr receiveBuffer, int receiveBufferSize, IntPtr overlappedBuffer, ManualResetEvent completeEvent, ManualResetEvent abortEvent, out int[] timingData)
    {
      timingData = null;
      if (!_isReceiving)
      {
        return;
      }

      Marshal.WriteInt32(receiveBuffer, _sizeOfUlongPtr, RECEIVE_BUFFER_SIZE);
      uint returnedByteCount;
      if (NativeMethods.DeviceIoControl(_handle.DangerousGetHandle(), IoControlCode.IOCTL_IR_PRIORITY_RECEIVE, IntPtr.Zero, 0, receiveBuffer, (uint)receiveBufferSize, out returnedByteCount, overlappedBuffer))
      {
        return;
      }

      int error = WaitForAsyncIoCompletion(_handle, overlappedBuffer, completeEvent, abortEvent, new TimeSpan(0, 0, 0, 0, -1), out returnedByteCount);
      if (error != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS)
      {
        return;
      }

      int dataMemberByteCount = 3 * _sizeOfUlongPtr;
      if (returnedByteCount < dataMemberByteCount)
      {
        return;
      }

      if (returnedByteCount > dataMemberByteCount)
      {
        //this.LogDebug("Microsoft blaster port driver: receive, byte count = {0}, device path = {1}", returnedByteCount, _devicePath);
        //Dump.DumpBinary(receiveBuffer, (int)returnedByteCount);

        timingData = new int[(returnedByteCount - dataMemberByteCount) / sizeof(int)];
        Marshal.Copy(IntPtr.Add(receiveBuffer, dataMemberByteCount), timingData, 0, timingData.Length);
      }

      // Cache the carrier frequency when DataEnd is set.
      _carrierFrequency = -1;
      if (Marshal.ReadInt32(receiveBuffer, 0) != 0)
      {
        _carrierFrequency = Marshal.ReadInt32(receiveBuffer, _sizeOfUlongPtr * 2);
      }
    }

    /// <summary>
    /// Learn the carrier frequency for a command.
    /// </summary>
    /// <param name="timingData">The timing data that has been learned so far.</param>
    /// <param name="carrierFrequency">The carrier frequency. The unit is Hertz (Hz).</param>
    /// <returns><c>true</c> if the carrier frequency has been learned (indicating learning is complete), otherwise <c>false</c></returns>
    protected override bool LearnCarrierFrequency(IEnumerable<int> timingData, out int carrierFrequency)
    {
      carrierFrequency = _carrierFrequency;
      return _carrierFrequency > 0;
    }

    #endregion

    #endregion

    /// <summary>
    /// Execute an I/O control.
    /// </summary>
    /// <param name="code">The control code.</param>
    /// <param name="inputBuffer">A buffer containing input for the control.</param>
    /// <param name="inputBufferSize">The size of <paramref name="inputBuffer"/>.</param>
    /// <param name="outputBuffer">A buffer to be used to receive output from the control.</param>
    /// <param name="outputBufferSize">The size of <paramref name="outputBuffer"/></param>
    private void IoControl(uint code, IntPtr inputBuffer, uint inputBufferSize, IntPtr outputBuffer, uint outputBufferSize)
    {
      if (_handle == null)
      {
        throw new TvException("IOCTL failed. Device is not open. Control code = {0}, device path = {1}.", code, _devicePath);
      }

      ManualResetEvent completeEvent = new ManualResetEvent(false);
      IntPtr overlappedBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeOverlapped)));
      try
      {
        NativeOverlapped overlapped = new NativeOverlapped();
        overlapped.EventHandle = completeEvent.SafeWaitHandle.DangerousGetHandle();
        Marshal.StructureToPtr(overlapped, overlappedBuffer, false);

        uint returnedByteCount;
        if (NativeMethods.DeviceIoControl(_handle.DangerousGetHandle(), code, inputBuffer, inputBufferSize, outputBuffer, outputBufferSize, out returnedByteCount, overlappedBuffer))
        {
          return;
        }

        int error = WaitForAsyncIoCompletion(_handle, overlappedBuffer, completeEvent, _abortEventWrite, TIMEOUT_WRITE, out returnedByteCount);
        if (error != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS)
        {
          throw new TvException("IOCTL failed. Control code = {0}, error = {1}, device path = {2}.", code, error, _devicePath);
        }
      }
      finally
      {
        completeEvent.Close();
        completeEvent.Dispose();
        Marshal.FreeHGlobal(overlappedBuffer);
      }
    }

    private void StopReceiving()
    {
      if (_isReceiving)
      {
        IoControl(IoControlCode.IOCTL_IR_EXIT_PRIORITY_RECEIVE, IntPtr.Zero, 0, IntPtr.Zero, 0);
        _isReceiving = false;
      }
    }
  }
}