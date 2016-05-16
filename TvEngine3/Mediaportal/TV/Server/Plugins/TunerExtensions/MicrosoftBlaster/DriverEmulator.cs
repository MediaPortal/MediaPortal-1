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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using Microsoft.Win32.SafeHandles;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  /// <summary>
  /// This class supports the emulation interface exposed by Windows XP's eHome
  /// driver and the community-developed "replacement driver" (MceIr.dll).
  /// </summary>
  /// <remarks>
  /// For technical information about this code refer to the specifications
  /// ("Remote Control and Receiver-Transceiver Specifications and Requirements
  /// for Windows Media Center in Windows Operating Systems", "Emulation
  /// Requirements" section):
  /// http://download.microsoft.com/download/0/7/E/07EF37BB-33EA-4454-856A-7D663BC109DF/Windows-Media-Center-RC-IR-Collection-Green-Button-Specification-03-08-2011-V2.pdf
  /// 
  /// For information about the replacment driver refer to:
  /// http://forum.team-mediaportal.com/threads/obsolete-mce-replacement-driver.29024/
  /// http://www.eventghost.org/mediawiki/index.php?title=MCE_Remote_Bruno
  /// 
  /// For information about the Linux kernel driver refer to:
  /// https://github.com/torvalds/linux/blob/master/drivers/media/rc/mceusb.c
  /// </remarks>
  internal class DriverEmulator : Driver
  {
    #region enums

    private enum CommandType : byte
    {
      UnknownInit = 0x05,               // undocumented - unknown command used during initialisation
      SetIrCarrierFrequency = 0x06,     // CMD_SETIRCFS
      GetIrCarrierFrequency = 0x07,     // CMD_GETIRCFS
      SetIrTransmitPorts = 0x08,        // CMD_SETIRTXPORTS

      GetHwSwFwVersion = 0x0b,          // undocumented - hardware, software or firmware version
      SetIrTimeOut = 0x0c,              // CMD_SETIRTIMEOUT
      GetIrTimeOut = 0x0d,              // CMD_GETIRTIMEOUT

      GetPortStatus = 0x11,             // CMD_GETPORTSTATUS

      GetIrTransmitPorts = 0x13,        // CMD_GETIRTXPORTS
      SetIrReceivePort = 0x14,          // CMD_SETIRRXPORTEN
      GetIrReceivePort = 0x15,          // CMD_GETIRRXPORTEN
      GetIrPortCount = 0x16,            // CMD_GETIRNUMPORTS
      GetWakeSource = 0x17,             // CMD_GETWAKESOURCE
      GetWakeVersion = 0x18,            // CMD_GETWAKEVERSION [emulator v2]

      GetWakeSupport = 0x20,            // CMD_GETWAKESUPPORT [emulator v2]
      GetDeviceDetails = 0x21,          // CMD_GETDEVDETAILS [emulator v2]
      GetEmulatorVersion = 0x22,        // CMD_GETEMVER [emulator v2]
      FlashLed = 0x23,                  // CMD_FLASHLED [emulator v2]

      Resume = 0xaa,                    // CMD_RESUME

      BootLoaderSetWakePattern = 0xef,  // CMD_BOOT_SETWAKEPATTERN [emulator v2]
      BootLoaderWriteBlock = 0xf0,      // CMD_BOOT_WRITEBLOCK [emulator v2]

      BootLoaderExit = 0xf4,            // CMD_BOOT_EXIT [emulator v2]
      BootLoaderGetVersion = 0xf5,      // CMD_BOOT_GETVERSION [emulator v2]
      BootLoaderWriteAuthorise = 0xf6,  // CMD_BOOT_WRITEAUTH [emulator v2]

      Reset = 0xfe,                     // CMD_RESET
      NoOperation = 0xff                // CMD_NOP
    }

    private enum ResponseType : byte
    {
      BootLoaderBlockWritten = 0x01,    // RSP_BOOT_BLOCKWRITTEN [emulator v2]

      BootLoaderVersion = 0x04,         // RSP_BOOT_VERSION [emulator v2]
      ErrorBootLoaderBadWakePattern = 0x05,   // RSP_BOOT_BADPATTERN [emulator v2]
      IrCarrierFrequency = 0x06,        // RSP_EQIRCFS

      IrTransmitPorts = 0x08,           // RSP_EQIRTXPORTS

      HwSwFwVersionMajorMinor = 0x0b,   // undocumented - hardware, software or firmware major and minor version numbers
      IrTimeOut = 0x0c,                 // RSP_EQIRTIMEOUT

      PortStatus = 0x11,                // RSP_GETPORTSTATUS

      IrReceivePort = 0x14,             // RSP_EQIRRXPORTEN
      IrReceiveCarrierFrequencyCount = 0x15,  // RSP_EQIRRXCFCNT
      IrPortCount = 0x16,               // RSP_EQIRNUMPORTS
      WakeSource = 0x17,                // RSP_GETWAKESOURCE
      WakeVersion = 0x18,               // RSP_EQWAKEVERSION [emulator v2]

      HwSwFwVersionMicroBuild = 0x1b,   // undocumented - hardware, software or firmware micro and build version numbers

      WakeSupport = 0x20,               // RSP_EQWAKESUPPORT [emulator v2]
      DeviceDetails = 0x21,             // RSP_EQDEVDETAILS [emulator v2]
      EmulatorVersion = 0x22,           // RSP_EQEMVER [emulator v2]
      FlashedLed = 0x23,                // RSP_FLASHLED [emulator v2]

      ErrorTransmitTimeOut = 0x81,      // RSP_TX_TIMEOUT

      BootLoaderWakePatternSet = 0xef,  // RSP_BOOT_SETWAKEPATTERN [emulator v2]
      ErrorBootLoaderBadChecksum = 0xf0,      // RSP_BOOT_BADSERIALCHECKSUM [emulator v2]

      ErrorBootLoaderBadWriteAuthorisation = 0xf2,  // RSP_BOOT_BADWRITEAUTH [emulator v2]

      ErrorIllegalCommand = 0xfe        // RSP_CMD_ILLEGAL
    }

    [Flags]
    private enum PortReceive : byte
    {
      None = 0,
      LongRange = 1,
      WideBand = 2    // for learning
    }

    [Flags]
    private enum DeviceDetail : byte
    {
      None = 0,
      TiedToTuner = 0x01,
      LearningOnly = 0x02,          // long range receive port not supported
      NarrowBandPassFilter = 0x04,  // long range receive port has a narrow BPF (ie. can't be used for parse-and-match learning)
      NoInput = 0x08,               // transmit only
      CanFlash = 0x10,              // CMD_FLASHLED supported
      HasBootLoader = 0x20
    }

    #endregion

    #region constants

    private const string VENDOR_ID_FINTEK = "vid_1934";
    private const string VENDOR_ID_MICROSOFT = "vid_045e";
    private const string VENDOR_ID_PHILIPS = "vid_0471";
    private const string VENDOR_ID_PINNACLE = "vid_2304";
    private const string VENDOR_ID_SMK = "vid_0609";
    private const string VENDOR_ID_TOPSEED = "vid_1784";

    // Refer to Linux kernel driver tx_mask_normal assignment.
    private static readonly HashSet<string> DEVICE_IDS_STANDARD_TRANSMIT_PORT_HANDLING = new HashSet<string>
    {
      VENDOR_ID_FINTEK + "&pid_5168",     // Fintek eHome infrared transceiver (HP-branded)
      VENDOR_ID_MICROSOFT + "&pid_006d",  // original Microsoft MCE IR transceiver (often HP-branded)
      VENDOR_ID_PHILIPS + "&pid_060c",    // Philips infrared transceiver (HP-branded)
      VENDOR_ID_PINNACLE + "&pid_0225",   // Pinnacle remote kit
      VENDOR_ID_SMK + "&pid_031d",        // SMK/Toshiba G83C0004D410
      VENDOR_ID_SMK + "&pid_0322",        // SMK eHome infrared transceiver (Sony VAIO)
      VENDOR_ID_SMK + "&pid_0334",        // bundled with Hauppauge PVR-150

      // Topseed eHome infrared transceiver
      VENDOR_ID_TOPSEED + "&pid_0001",
      VENDOR_ID_TOPSEED + "&pid_0006",
      VENDOR_ID_TOPSEED + "&pid_0007",
      VENDOR_ID_TOPSEED + "&pid_0008",
      VENDOR_ID_TOPSEED + "&pid_000a",
      VENDOR_ID_TOPSEED + "&pid_0011"
    };

    private const byte CMD_PORT_SYS = 0xff;
    private const byte CMD_PORT_IR = 0x9f;
    private const byte CMD_PORT_SER = 0xdf;
    private const byte CMD_DATA_END = 0x80;

    private const byte RSP_PORT_SYS = 0xff;
    private const byte RSP_PORT_IR = 0x9f;

    private static readonly byte[] STOP_PACKET =
    {
      0xff, 0xbb,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
      0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
    };

    private static readonly TimeSpan TIMEOUT_COMMAND_RESPONSE = new TimeSpan(0, 0, 0, 0, 50);

    #endregion

    #region variables

    private readonly bool _isReplacementDriver = false;
    private byte _emulatorVersion = 0;
    private DeviceDetail _deviceDetails = DeviceDetail.None;

    private SafeFileHandle _readHandle = null;
    private SafeFileHandle _writeHandle = null;

    private object _writeLock = new object();
    private ManualResetEvent _responseWaitEvent = null;

    private uint _decodeCarryByteCount = 0;
    private int _decodeCarryDuration = 0;
    private int _decodeCarrierCount = -1;

    #endregion

    public DriverEmulator(string devicePath, bool isReplacementDriver = false)
      : base(devicePath)
    {
      _isReplacementDriver = isReplacementDriver;

      Match m = Regex.Match(devicePath.ToLowerInvariant(), @"vid_[0-9a-f]{4}\&pid_[0-9a-f]{4}");
      _useStandardTransmitPortHandling = m.Success && DEVICE_IDS_STANDARD_TRANSMIT_PORT_HANDLING.Contains(m.Value);
    }

    #region abstract Driver function implementation

    /// <summary>
    /// Open access handle(s).
    /// </summary>
    /// <returns><c>true</c> if the handle(s) are opened successfully, otherwise <c>false</c></returns>
    protected override bool OpenHandles()
    {
      if (_isReplacementDriver)
      {
        _readHandle = NativeMethods.CreateFile(_devicePath + "\\Pipe01", NativeMethods.AccessMask.GENERIC_READ, FileShare.None,
                                                IntPtr.Zero, FileMode.Open, (uint)NativeMethods.FileFlag.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
      }
      else
      {
        _readHandle = NativeMethods.CreateFile(_devicePath, NativeMethods.AccessMask.GENERIC_READ | NativeMethods.AccessMask.GENERIC_WRITE, FileShare.None,
                                                IntPtr.Zero, FileMode.Open, (uint)NativeMethods.FileFlag.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
      }
      if (_readHandle.IsInvalid)
      {
        _readHandle = null;
        this.LogError("Microsoft blaster emulation driver: failed to open read handle (is this device being used by other software?), error = {0}, device path = {1}, is replacement driver = {2}", Marshal.GetLastWin32Error(), _devicePath, _isReplacementDriver);
        return false;
      }

      lock (_writeLock)
      {
        if (!_isReplacementDriver)
        {
          _writeHandle = _readHandle;
        }
        else
        {
          _writeHandle = NativeMethods.CreateFile(_devicePath + "\\Pipe00", NativeMethods.AccessMask.GENERIC_WRITE, FileShare.None,
                                                  IntPtr.Zero, FileMode.Open, (uint)NativeMethods.FileFlag.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
          if (_writeHandle.IsInvalid)
          {
            _writeHandle = null;
            _readHandle.Close();
            _readHandle.Dispose();
            _readHandle = null;
            this.LogError("Microsoft blaster emulation driver: failed to open write handle (is this device being used by other software?), error = {0}, device path = {1}, is replacement driver = {2}", Marshal.GetLastWin32Error(), _devicePath, _isReplacementDriver);
            return false;
          }
        }
      }

      _responseWaitEvent = new ManualResetEvent(false);
      _decodeCarryByteCount = 0;
      _decodeCarryDuration = 0;
      _decodeCarrierCount = -1;

      try
      {
        Resume();
        return true;
      }
      catch (Exception ex)
      {
        CloseHandles();
        this.LogError(ex, "Microsoft blaster emulation driver: resume command failed, device path = {0}, is replacement driver = {1}", _devicePath, _isReplacementDriver);
        return false;
      }
    }

    /// <summary>
    /// Close the access handle(s).
    /// </summary>
    protected override void CloseHandles()
    {
      try
      {
        Write(STOP_PACKET);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster emulation driver: stop command failed");
      }

      if (_responseWaitEvent != null)
      {
        _responseWaitEvent.Close();
        _responseWaitEvent.Dispose();
        _responseWaitEvent = null;
      }

      lock (_writeLock)
      {
        if (_writeHandle != null && !object.ReferenceEquals(_readHandle, _writeHandle))
        {
          _writeHandle.Close();
          _writeHandle.Dispose();
        }
        _writeHandle = null;
      }

      if (_readHandle != null)
      {
        _readHandle.Close();
        _readHandle.Dispose();
        _readHandle = null;
      }
    }

    /// <summary>
    /// Get the device's capabilities.
    /// </summary>
    protected override void GetCapabilities()
    {
      // Undocumented commands from IRSS. These are also sent by the Linux
      // kernel driver, so maybe they're required initialisation (...or maybe
      // they just copied the IRSS code too)???
      this.LogDebug("Microsoft blaster emulation driver: get hardware/software/firmware version");
      try
      {
        Write(new byte[2] { CMD_PORT_SYS, (byte)CommandType.GetHwSwFwVersion });
        Write(new byte[2] { CMD_PORT_IR, (byte)CommandType.UnknownInit });
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster emulation driver: undocumented initialisation commands failed");
      }

      // Get the emulator version. If the emulator is version 1, the response
      // should be RSP_CMD_ILLEGAL because the command is not supported. The
      // receiver thread will automatically send the required CMD_RESET.
      this.LogDebug("Microsoft blaster emulation driver: get emulation version");
      _emulatorVersion = 0;
      try
      {
        Write(new byte[2] { CMD_PORT_SYS, (byte)CommandType.GetEmulatorVersion }, true);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster emulation driver: get emulator version command failed, assuming version 1 emulation");
        if (_emulatorVersion == 0)
        {
          _emulatorVersion = 1;
        }
      }

      // Determine which receive and transmit ports are available.
      _deviceDetails = DeviceDetail.None;
      if (_emulatorVersion == 2)
      {
        this.LogDebug("Microsoft blaster emulation driver: get device details");
        try
        {
          Write(new byte[2] { CMD_PORT_SYS, (byte)CommandType.GetDeviceDetails });
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "Microsoft blaster emulation driver: get device details command failed, assuming default device details");
        }
      }

      this.LogDebug("Microsoft blaster emulation driver: get IR port count");
      try
      {
        Write(new byte[2] { CMD_PORT_IR, (byte)CommandType.GetIrPortCount }, true);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "Microsoft blaster emulation driver: get IR port count command failed");
      }
      if (_availablePortsReceive == ReceivePort.None && _availablePortsTransmit == TransmitPort.None)
      {
        if (_deviceDetails.HasFlag(DeviceDetail.LearningOnly))
        {
          this.LogWarn("Microsoft blaster emulation driver: assuming default transmitter port availability");
          _availablePortsReceive = ReceivePort.Learn;
        }
        else if (!_deviceDetails.HasFlag(DeviceDetail.NoInput))
        {
          this.LogWarn("Microsoft blaster emulation driver: assuming default transceiver port availability");
          _availablePortsReceive = ReceivePort.Receive | ReceivePort.Learn;
        }
        _availablePortsTransmit = TransmitPort.Port1 | TransmitPort.Port2;
      }
    }

    #region transmit

    /// <summary>
    /// Update the status for each transmit port.
    /// </summary>
    protected override void UpdateTransmitPortStatus()
    {
      // The documentation for "P" (port number) does not specify whether it
      // should be zero-indexed, one-indexed or other. Nor does it specify how
      // "P" maps back to CMD_SETIRTXPORTS's "P" bitmap. The only other code I
      // could find to base this implementation on is the Linux kernel driver
      // code. In other words: this code could be completely wrong.
      TransmitPort port;
      for (byte p = 0; p < 8; p++)
      {
        port = GetPortByIndex(p);
        if (_availablePortsTransmit.HasFlag(port))
        {
          try
          {
            Write(new byte[3] { CMD_PORT_SYS, (byte)CommandType.GetPortStatus, p }, true);
          }
          catch (Exception ex)
          {
            this.LogWarn(ex, "Microsoft blaster emulation driver: assuming transmit port {0} has an emitter connected", p + 1);
            _connectedTransmitPorts |= port;
          }
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
      this.LogDebug("Microsoft blaster emulation driver: set port mask, mask = 0x{0:x8}", portMask);
      Write(new byte[3] { CMD_PORT_IR, (byte)CommandType.SetIrTransmitPorts, (byte)portMask });

      this.LogDebug("Microsoft blaster emulation driver: set carrier frequency, frequency = {0} Hz", carrierFrequency);
      byte[] packet = new byte[4] { CMD_PORT_IR, (byte)CommandType.SetIrCarrierFrequency, 0x00, 0x00 };   // default = DC mode
      if (carrierFrequency > 0)
      {
        byte preScalar = 0;
        while (true)
        {
          int period = (int)Math.Round((double)10000000 / (carrierFrequency * (1 << (preScalar * 2)))) - 1;
          if (period <= 0xff)
          {
            packet[2] = preScalar;
            packet[3] = (byte)period;
            break;
          }
        }
      }
      Write(packet);

      // Assemble a message from the command timing data.
      List<byte> message = new List<byte>();
      foreach (int duration in command)
      {
        uint sampleCount = (uint)Math.Abs(Math.Round((double)duration / SAMPLE_PERIOD));
        byte pulseOrSpace = (byte)(duration > 0 ? 0x80 : 0);
        while (sampleCount > 0x7f)
        {
          message.Add((byte)(pulseOrSpace | 0x7f));
          sampleCount -= 0x7f;
        }

        message.Add((byte)(pulseOrSpace | sampleCount));
      }

      // Split the message into packets.
      byte standardPacketMessageByteCount = 4;    // as far as I know this is arbitrary
      int packetCount = message.Count / standardPacketMessageByteCount;
      if (message.Count % standardPacketMessageByteCount != 0)
      {
        packetCount++;
      }

      // Limit the full packet data set to 341 bytes. This limit comes from
      // the old IRSS code. I can't find any reference to a limit in the specs.
      // Note that the Linux kernel driver seems to limit to 384 bytes
      // (MCE_CMDBUF_SIZE).
      int packetDataSize = message.Count + packetCount;
      if (packetDataSize > 340)
      {
        this.LogWarn("Microsoft blaster emulation driver: IR command is too long and will be truncated, pulse/space count = {0}, encoded byte count = {1}", command.Length, message.Count);
        packetDataSize = 340;
      }

      byte[] packetData = new byte[packetDataSize + 1];   // + 1 for CMD_DATA_END
      int messageIndex = 0;
      for (uint i = 0; i < packetDataSize; i++)
      {
        byte packetMessageByteCount = standardPacketMessageByteCount;
        if (message.Count - messageIndex < standardPacketMessageByteCount)
        {
          packetMessageByteCount = (byte)(message.Count - messageIndex);
        }
        packetData[i++] = (byte)(0x80 | packetMessageByteCount);

        for (int j = 0; j < packetMessageByteCount; j++)
        {
          packetData[i++] = message[messageIndex++];
        }
      }

      packetData[packetDataSize] = CMD_DATA_END;

      this.LogDebug("Microsoft blaster emulation driver: transmit IR command, pulse/space count = {0}, encoded byte count = {1}", command.Length, packetData.Length);
      //Dump.DumpBinary(packetData);
      Write(packetData);
    }

    #endregion

    #region receive/learn

    /// <summary>
    /// Get the size of buffer to create for receiving.
    /// </summary>
    protected override int GetReceiveBufferSize()
    {
      return RECEIVE_BUFFER_SIZE;
    }

    /// <summary>
    /// Set the receive configuration.
    /// </summary>
    /// <param name="port">The port to receive from.</param>
    /// <param name="timeOut">The receive time-out.</param>
    protected override void SetReceiveConfiguration(ReceivePort port, TimeSpan timeOut)
    {
      PortReceive tempPort = PortReceive.LongRange;
      if (port == ReceivePort.Learn)
      {
        tempPort = PortReceive.WideBand;
        _decodeCarrierCount = -1;
      }
      Write(new byte[3] { CMD_PORT_IR, (byte)CommandType.SetIrReceivePort, (byte)tempPort });

      uint timeOutSampleCount = (uint)(1000 * timeOut.TotalMilliseconds / SAMPLE_PERIOD);
      Write(new byte[4] { CMD_PORT_IR, (byte)CommandType.SetIrTimeOut, (byte)(timeOutSampleCount >> 8), (byte)(timeOutSampleCount & 0xff) });
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

      uint readByteCount;
      if (!NativeMethods.ReadFile(_readHandle, receiveBuffer, (uint)receiveBufferSize, out readByteCount, overlappedBuffer))
      {
        int error = WaitForAsyncIoCompletion(_readHandle, overlappedBuffer, completeEvent, abortEvent, new TimeSpan(0, 0, 0, 0, -1), out readByteCount);
        if (error != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS)
        {
          return;
        }
      }

      if (readByteCount == 0)
      {
        return;
      }

      byte[] packetBytes = new byte[readByteCount];
      Marshal.Copy(receiveBuffer, packetBytes, 0, (int)readByteCount);

      //this.LogDebug("Microsoft blaster emulation driver: receive, byte count = {0}, device path = {1}", readByteCount, _devicePath);
      //Dump.DumpBinary(packetBytes);

      timingData = InterpretReceivedData(packetBytes);
    }

    /// <summary>
    /// Learn the carrier frequency for a command.
    /// </summary>
    /// <param name="timingData">The timing data that has been learned so far.</param>
    /// <param name="carrierFrequency">The carrier frequency. The unit is Hertz (Hz).</param>
    /// <returns><c>true</c> if the carrier frequency has been learned (indicating learning is complete), otherwise <c>false</c></returns>
    protected override bool LearnCarrierFrequency(IEnumerable<int> timingData, out int carrierFrequency)
    {
      carrierFrequency = -1;
      if (_decodeCarrierCount < 0)
      {
        return false;
      }

      int pulseTime = 0;
      int pulseCount = 0;
      foreach (int duration in timingData)
      {
        if (duration > 0)
        {
          pulseTime += duration;
          pulseCount++;
        }
      }

      if (pulseCount == 0)
      {
        return false;
      }

      if ((double)_decodeCarrierCount / pulseCount < 2.0)
      {
        carrierFrequency = 0;   // DC (no carrier)
      }
      else
      {
        carrierFrequency = (int)(1000000 * (double)_decodeCarrierCount / pulseTime);
      }
      return true;
    }

    #endregion

    #endregion

    /// <summary>
    /// Synchronously write one or more packets to the device.
    /// </summary>
    /// <param name="data">The packet data to write.</param>
    /// <param name="waitForResponse"><c>True</c> to wait for a response to be received.</param>
    private void Write(byte[] data, bool waitForResponse = false)
    {
      //this.LogDebug("Microsoft blaster emulation driver: write, byte count = {0}, wait for response = {1}", data.Length, waitForResponse);
      //Dump.DumpBinary(data);

      lock (_writeLock)
      {
        if (_writeHandle == null)
        {
          throw new TvException("Write failed. Device is not open. Device path = {0}.", _devicePath);
        }

        if (waitForResponse)
        {
          _responseWaitEvent.Reset();
        }

        // With regard to the OVERLAPPED structure and buffer...
        // I'm not certain that calling WriteFile() from a WOW64 process will
        // work (because OVERLAPPED contains pointer members). However, my
        // understanding is that it's very important that the OVERLAPPED goes
        // in global memory. That way we won't have any issues with memory
        // being freed while the async call is still completing.
        ManualResetEvent completeEvent = new ManualResetEvent(false);
        IntPtr overlappedBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeOverlapped)));
        try
        {
          NativeOverlapped overlapped = new NativeOverlapped();
          overlapped.EventHandle = completeEvent.SafeWaitHandle.DangerousGetHandle();
          Marshal.StructureToPtr(overlapped, overlappedBuffer, false);

          uint writtenByteCount;
          if (!NativeMethods.WriteFile(_writeHandle, data, (uint)data.Length, out writtenByteCount, overlappedBuffer))
          {
            int error = WaitForAsyncIoCompletion(_writeHandle, overlappedBuffer, completeEvent, _abortEventWrite, TIMEOUT_WRITE, out writtenByteCount);
            if (error != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS)
            {
              throw new TvException("Asynchronous write failed. Error = {0}, device path = {1}.", error, _devicePath);
            }
          }

          if (waitForResponse && !_responseWaitEvent.WaitOne(TIMEOUT_COMMAND_RESPONSE))
          {
            this.LogWarn("Microsoft blaster emulation driver: timed out waiting for command response, command = {0}", (CommandType)data[1]);
          }
        }
        finally
        {
          completeEvent.Close();
          completeEvent.Dispose();
          Marshal.FreeHGlobal(overlappedBuffer);
        }
      }
    }

    /// <summary>
    /// Send a resume command to recover after an error.
    /// </summary>
    private void Resume()
    {
      this.LogDebug("Microsoft blaster emulation driver: resume");
      // The leading 0 comes from IRSS code. I have no idea if/why it is needed.
      Write(new byte[3] { 0x00, CMD_PORT_SYS, (byte)CommandType.Resume });
    }

    #region received data interpretation

    /// <summary>
    /// Interpret data received from the device.
    /// </summary>
    /// <param name="data">The received data.</param>
    /// <returns>any IR timing data received, otherwise <c>null</c></returns>
    private int[] InterpretReceivedData(byte[] receivedData)
    {
      if (receivedData == null || receivedData.Length == 0)
      {
        return null;
      }

      // First comes the run-length-coded IR data (if any)...
      int consumedByteCount = 0;
      int[] timingData = null;
      if (_decodeCarryByteCount != 0 || (receivedData[0] >= 0x81 && receivedData[0] <= 0x9e))
      {
        timingData = InterpretRlcIrData(receivedData, out consumedByteCount);
        if (timingData == null)
        {
          return null;
        }
      }

      // ...then comes other command response packets.
      // Note we don't handle RSP_BOOT_* responses because they don't have a
      // leading RSP_PORT_IR or RSP_PORT_SYS, which makes them harder to
      // recognise. This shouldn't be a problem considering that we don't send
      // CMD_BOOT_* commands (so we wouldn't expect to receive any RSP_BOOT_*
      // responses).
      byte hwSwFwVersionMajor = 0;
      byte hwSwFwVersionMinor = 0;
      byte hwSwFwVersionMicro = 0;
      byte hwSwFwVersionBuild = 0;
      while (consumedByteCount < receivedData.Length)
      {
        byte responseGroup = receivedData[consumedByteCount++];
        if (responseGroup != RSP_PORT_SYS && responseGroup != RSP_PORT_IR)
        {
          continue;
        }

        try
        {
          if (consumedByteCount >= receivedData.Length)
          {
            this.LogWarn("Microsoft blaster emulation driver: system or IR port command response is split (not supported), device path = {0}", _devicePath);
            return timingData;
          }

          ResponseType responseType = (ResponseType)receivedData[consumedByteCount++];
          if (
            responseType == ResponseType.ErrorIllegalCommand ||
            (responseGroup == RSP_PORT_IR && responseType == ResponseType.ErrorTransmitTimeOut)
          )
          {
            if (responseType == ResponseType.ErrorIllegalCommand && _emulatorVersion == 0)
            {
              this.LogDebug("Microsoft blaster emulation driver: emulator version = 1 [by illegal command response]");
              _emulatorVersion = 1;
            }
            else
            {
              this.LogError("Microsoft blaster emulation driver: received error response, error = {0}, device path = {1}", responseType, _devicePath);
            }
            ThreadPool.QueueUserWorkItem(delegate
            {
              try
              {
                Resume();
              }
              catch (Exception ex)
              {
                this.LogError(ex, "Microsoft blaster emulation driver: resume command after error failed, device path = {0}", _devicePath);
              }
            });
            continue;
          }

          byte requiredByteCount = 0;
          if (
            responseGroup == RSP_PORT_SYS &&
            (
              responseType == ResponseType.DeviceDetails ||
              responseType == ResponseType.EmulatorVersion ||
              responseType == ResponseType.HwSwFwVersionMajorMinor ||
              responseType == ResponseType.HwSwFwVersionMicroBuild
            )
          )
          {
            requiredByteCount = 1;
          }
          else if (
            responseGroup == RSP_PORT_IR &&
            (
              responseType == ResponseType.IrPortCount ||
              responseType == ResponseType.IrReceiveCarrierFrequencyCount
            )
          )
          {
            requiredByteCount = 2;
          }
          else if (responseGroup == RSP_PORT_SYS && responseType == ResponseType.PortStatus)
          {
            requiredByteCount = 5;
          }
          else
          {
            // Don't care about this response.
            continue;
          }
          if (consumedByteCount + requiredByteCount - 1 >= receivedData.Length)
          {
            this.LogWarn("Microsoft blaster emulation driver: {0} response is split (not supported), device path = {1}", responseType, _devicePath);
            return timingData;
          }

          if (responseType == ResponseType.DeviceDetails)
          {
            _deviceDetails = (DeviceDetail)receivedData[consumedByteCount++];
            this.LogDebug("Microsoft blaster emulation driver: device details = {0}", _deviceDetails);
          }
          else if (responseType == ResponseType.EmulatorVersion)
          {
            _emulatorVersion = receivedData[consumedByteCount++];
            this.LogDebug("Microsoft blaster emulation driver: emulator version = {0}", _emulatorVersion);
          }
          else if (responseType == ResponseType.HwSwFwVersionMajorMinor)
          {
            byte version = receivedData[consumedByteCount++];   // commonly 0x45 or 0x50
            hwSwFwVersionMajor = (byte)(version >> 4);
            hwSwFwVersionMinor = (byte)(version & 0xf);
          }
          else if (responseType == ResponseType.HwSwFwVersionMicroBuild)
          {
            byte version = receivedData[consumedByteCount++];   // commonly 0x08 or 0x42
            hwSwFwVersionMicro = (byte)(version >> 4);
            hwSwFwVersionBuild = (byte)(version & 0xf);
          }
          else if (responseType == ResponseType.IrPortCount)
          {
            byte portCountTransmit = receivedData[consumedByteCount++];
            byte portCountReceive = receivedData[consumedByteCount++];

            _availablePortsTransmit = TransmitPort.None;
            for (byte p = 0; p < portCountTransmit; p++)
            {
              _availablePortsTransmit |= GetPortByIndex(p);
            }
            this.LogDebug("Microsoft blaster emulation driver: available transmit port(s) = {0}", _availablePortsTransmit);

            _availablePortsReceive = ReceivePort.Receive | ReceivePort.Learn;
            if (portCountReceive == 0 || _deviceDetails.HasFlag(DeviceDetail.NoInput))
            {
              _availablePortsReceive = ReceivePort.None;
              _deviceDetails |= DeviceDetail.NoInput;
            }
            else if (
              _deviceDetails.HasFlag(DeviceDetail.LearningOnly) ||
              (_emulatorVersion < 2 && portCountReceive == 1 && portCountTransmit > 0)
            )
            {
              _availablePortsReceive = ReceivePort.Learn;
              _deviceDetails |= DeviceDetail.LearningOnly;
            }
            else if (portCountReceive == 1)
            {
              _availablePortsReceive = ReceivePort.Receive;
            }
            this.LogDebug("Microsoft blaster emulation driver: available receive port(s) = {0}", _availablePortsReceive);
          }
          else if (responseType == ResponseType.IrReceiveCarrierFrequencyCount)
          {
            _decodeCarrierCount = (receivedData[consumedByteCount] << 8) | receivedData[consumedByteCount + 1];
            consumedByteCount += 2;
          }
          else if (responseGroup == RSP_PORT_SYS && responseType == ResponseType.PortStatus)
          {
            byte portIndex = receivedData[consumedByteCount++];
            byte voltsAtRestHigh = receivedData[consumedByteCount++];
            byte voltsAtRestLow = receivedData[consumedByteCount++];
            byte voltsWhenDrivenHigh = receivedData[consumedByteCount++];
            byte voltsWhenDrivenLow = receivedData[consumedByteCount++];
            if (voltsAtRestHigh == 0 && voltsAtRestLow == 0 && voltsWhenDrivenLow == 0)
            {
              TransmitPort port = GetPortByIndex(portIndex);
              bool isEmitterConnected = voltsWhenDrivenHigh == 0;
              if (_connectedTransmitPorts.HasFlag(port) && !isEmitterConnected)
              {
                this.LogDebug("Microsoft blaster emulation driver: transmit port {0}'s emitter has been disconnected", portIndex + 1);
                _connectedTransmitPorts &= ~port;
              }
              else if (!_connectedTransmitPorts.HasFlag(port) && isEmitterConnected)
              {
                this.LogDebug("Microsoft blaster emulation driver: transmit port {0} has an emitter connected", portIndex + 1);
                _connectedTransmitPorts |= port;
              }
            }
          }
        }
        finally
        {
          _responseWaitEvent.Set();
        }
      }

      if (hwSwFwVersionMajor != 0 || hwSwFwVersionMinor != 0 || hwSwFwVersionMicro != 0 || hwSwFwVersionBuild != 0)
      {
        this.LogDebug("Microsoft blaster emulation driver: hardware/software/firmware version = {0}.{1}.{2}.{3}", hwSwFwVersionMajor, hwSwFwVersionMinor, hwSwFwVersionMicro, hwSwFwVersionBuild);
      }

      return timingData;
    }

    /// <summary>
    /// Interpet run-length-coded IR data.
    /// </summary>
    /// <param name="data">The RLC IR data.</param>
    /// <param name="consumedByteCount">The number of bytes interpretted by this function.</param>
    /// <returns>timing data equivalent to the RLC IR data if successful, otherwise <c>null</c></returns>
    private int[] InterpretRlcIrData(byte[] data, out int consumedByteCount)
    {
      consumedByteCount = 0;
      List<int> timingData = new List<int>();
      for (uint index = 0; index < data.Length; )
      {
        uint stopIndex = index + _decodeCarryByteCount;
        if (_decodeCarryByteCount == 0)
        {
          // Prefix Byte Format
          // The 3 most significant bits should be set as 100. The 5 remaining
          // bits encode the number of bytes in the message. Maximum byte count
          // is 30 (0x1e); minimum byte count is 1. Those constraints avoid
          // confusion with RSP_PORT_IR (0x9f) and CMD_DATA_END (0x80).
          byte prefixByte = data[index++];
          if (prefixByte == RSP_PORT_SYS || prefixByte == RSP_PORT_IR || prefixByte == CMD_DATA_END)
          {
            // End of IR data. Note we don't actually expect to receive
            // CMD_DATA_END because a RSP_EQIRRXCFCNT message should be
            // received last.
            break;
          }
          if ((prefixByte & 0xe0) != 0x80)
          {
            this.LogError("Microsoft blaster emulation driver: invalid IR data prefix byte, prefix byte = {0}, index = {1}, device path = {2}", prefixByte, index - 1, _devicePath);
            Dump.DumpBinary(data);
            return null;
          }

          consumedByteCount++;
          stopIndex = index + (uint)(prefixByte & 0x7f);
          _decodeCarryDuration = 0;
        }
        _decodeCarryByteCount = 0;

        // Truncate the stop index and set the decode carry byte count if we
        // haven't received the complete packet/message in this receive cycle.
        if (stopIndex > data.Length)
        {
          _decodeCarryByteCount = stopIndex - (uint)data.Length;
          stopIndex -= _decodeCarryByteCount;
        }

        while (index < stopIndex)
        {
          byte currentByte = data[index++];
          consumedByteCount++;
          int duration = currentByte & 0x7f;
          if ((currentByte & 0x80) == 0)  // MSB set indicates pulse; unset indicates space
          {
            duration *= -1;
          }
          // Theoretically pulse/space should alternate unless duration is
          // 0x7f (ie. longer than can be encoded in one byte), but we don't
          // make that assumption.
          if (_decodeCarryDuration != 0 && Math.Sign(_decodeCarryDuration) != Math.Sign(duration))
          {
            timingData.Add(_decodeCarryDuration * SAMPLE_PERIOD);
            _decodeCarryDuration = 0;
          }
          _decodeCarryDuration += duration;
        }
      }

      if (_decodeCarryByteCount == 0 && _decodeCarryDuration != 0)
      {
        timingData.Add(_decodeCarryDuration * SAMPLE_PERIOD);
        _decodeCarryDuration = 0;
      }
      return timingData.ToArray();
    }

    #endregion
  }
}