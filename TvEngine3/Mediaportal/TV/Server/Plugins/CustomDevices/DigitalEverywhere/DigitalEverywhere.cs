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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.DigitalEverywhere
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC and PID filtering for Digital Everywhere devices.
  /// </summary>
  public class DigitalEverywhere : BaseCustomDevice, IPowerDevice, IPidFilterController, ICustomTuner, IConditionalAccessProvider, ICiMenuActions, IDiseqcDevice
  {
    #region enums

    private enum BdaExtensionProperty
    {
      SelectMultiplexDvbS = 0,
      SelectServiceDvbS,
      SelectPidsDvbS,                   // Use for DVB-S and DVB-C.
      SignalStrength,
      DriverVersion,
      SelectMultiplexDvbT,
      SelectPidsDvbT,
      SelectMultiplexDvbC,
      SelectPidsDvbC,                   // Don't use.
      FrontendStatus,
      SystemInfo,
      FirmwareVersion,
      LnbControl,
      GetLnbParams,
      SetLnbParams,
      LnbPower,
      AutoTuneStatus,
      FirmwareUpdate,
      FirmwareUpdateStatus,
      CiReset,
      CiWriteTpdu,
      CiReadTpdu,
      MmiHostToCam,
      MmiCamToHost,
      Temperature,
      TuneQpsk,
      RemoteControlRegister,
      RemoteControlCancel,
      CiStatus,
      TestInterface,
      CheckTuningFlag
    }

    private enum DeCiMessageTag : byte
    {
      Reset = 0,
      ApplicationInfo,
      Pmt,
      PmtReply,
      DateTime,
      Mmi,
      DebugError,
      EnterMenu,
      SendServiceId
    }

    private enum DeLnbPower : byte
    {
      Off = 0x60,
      On = 0x70
    }

    private enum DeResetType : byte
    {
      ForcedHardwareReset = 0
    }

    private enum DeErrorCode
    {
      Success = 0,
      Error,
      InvalidDeviceHandle,
      InvalidValue,
      AlreadyInUse,
      NotSupportedByTuner
    }

    private enum DeFecRate : byte
    {
      Auto = 0,
      Rate1_2,            // 1/2
      Rate2_3,            // 2/3
      Rate3_4,            // 3/4
      Rate5_6,            // 5/6
      Rate7_8             // 7/8
    }

    private enum DePolarisation : byte
    {
      None = 0xff,
      Horizontal = 0,
      Vertical = 1
    }

    private enum De22k : byte
    {
      Undefined = 0xff,
      Off = 0,
      On = 1
    }

    private enum DeToneBurst : byte
    {
      Undefined = 0xff,
      ToneBurst = 0,
      DataBurst = 1
    }

    private enum DeOfdmConstellation : byte
    {
      Auto = 0xff,
      DvbtQpsk = 0,
      Qam16,
      Qam64
    }

    private enum DeOfdmHierarchy : byte
    {
      Auto = 0xff,
      None = 0,
      One,
      Two,
      Four
    }

    private enum DeOfdmCodeRate : byte
    {
      Auto = 0xff,
      Rate1_2 = 0,        // 1/2
      Rate2_3,            // 2/3
      Rate3_4,            // 3/4
      Rate5_6,            // 5/6
      Rate7_8             // 7/8
    }

    private enum DeOfdmGuardInterval : byte
    {
      Auto = 0xff,
      Interval1_32 = 0,   // 1/32
      Interval1_16,       // 1/16
      Interval1_8,        // 1/8
      Interval1_4         // 1/4
    }

    private enum DeOfdmTransmissionMode : byte
    {
      Auto = 0xff,
      Mode2k = 0,
      Mode8k
    }

    private enum DeOfdmBandwidth : byte
    {
      Bandwidth8 = 0,     // 8 MHz
      Bandwidth7,         // 7 MHz
      Bandwidth6          // 6 MHz
    }

    [Flags]
    private enum DeFrontEndState : byte
    {
      PowerSupply = 0x01,
      PowerStatus = 0x02,
      AutoTune = 0x04,
      AntennaError = 0x08,
      FrontEndError = 0x10,
      VoltageValid = 0x40,
      FlagsValid = 0x80
    }

    [Flags]
    private enum DeCiState : ushort
    {
      Empty = 0,
      ErrorMessageAvailable = 0x0001,       // CI_ERR_MSG_AVAILABLE

      // CAM states.
      CamReady = 0x0002,                    // CI_MODULE_INIT_READY
      CamError = 0x0004,                    // CI_MODULE_ERROR
      CamIsDvb = 0x0008,                    // CI_MODULE_IS_DVB
      CamPresent = 0x0010,                  // CI_MODULE_PRESENT

      // MMI response states.
      ApplicationInfoAvailable = 0x0020,    // CI_APP_INFO_AVAILABLE - indicates whether the CAM is able to descramble or not
      DateTimeRequest = 0x0040,             // CI_DATE_TIME_REQEST
      PmtReply = 0x0080,                    // CI_PMT_REPLY
      MmiRequest = 0x0100                   // CI_MMI_REQUEST
    }

    private enum DeAntennaType : byte
    {
      Fixed = 0,
      Movable,
      Mobile
    }

    private enum DeBroadcastSystem : byte
    {
      Undefined = 0,
      DvbS = 0x01,
      DvbC = 0x02,
      DvbT = 0x03,
      AnalogAudio = 0x10,
      AnalogVideo = 0x11,
      Dvb = 0x20,
      Dab = 0x21,
      Atsc = 0x22
    }

    private enum DeTransportType : byte
    {
      Undefined = 0,
      Satellite,
      Cable,
      Terrestrial
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    private struct DvbsMultiplexParams
    {
      public UInt32 Frequency;            // unit = kHz, range = 9750000 - 12750000
      public UInt32 SymbolRate;           // unit = ks/s, range = 1000 - 40000

      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;
      public byte Lnb;                    // index (0..3) of the LNB parameters set with SetLnbParams
      private byte Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DvbsServiceParams
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool CurrentTransponder;
      public UInt32 Lnb;                  // index (0..3) of the LNB parameters set with SetLnbParams

      public UInt32 Frequency;            // unit = kHz, range = 9750000 - 12750000
      public UInt32 SymbolRate;           // unit = ks/s, range = 1000 - 40000

      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;
      private UInt16 Padding;

      public UInt16 OriginalNetworkId;
      public UInt16 TransportStreamId;
      public UInt16 ServiceId;
      public UInt16 VideoPid;
      public UInt16 AudioPid;
      public UInt16 PcrPid;
      public UInt16 TeletextPid;
      public UInt16 PmtPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DvbsPidFilterParams
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool CurrentTransponder;
      [MarshalAs(UnmanagedType.Bool)]
      public bool FullTransponder;

      public byte Lnb;                    // index (0..3) of the LNB parameters set with SetLnbParams
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding1;

      public UInt32 Frequency;            // unit = kHz, range = 9750000 - 12750000
      public UInt32 SymbolRate;           // unit = ks/s, range = 1000 - 40000

      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;
      private UInt16 Padding2;

      public byte NumberOfValidPids;
      private byte Padding3;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidFilterPids)]
      public UInt16[] FilterPids;
      private UInt16 Padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DvbtMultiplexParams
    {
      public UInt32 Frequency;            // unit = kHz, range = 47000 - 860000
      public DeOfdmBandwidth Bandwidth;
      public DeOfdmConstellation Constellation;
      public DeOfdmCodeRate CodeRateHp;
      public DeOfdmCodeRate CodeRateLp;
      public DeOfdmGuardInterval GuardInterval;
      public DeOfdmTransmissionMode TransmissionMode;
      public DeOfdmHierarchy Hierarchy;
      private byte Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DvbtPidFilterParams
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool CurrentTransponder;
      [MarshalAs(UnmanagedType.Bool)]
      public bool FullTransponder;

      public DvbtMultiplexParams MultiplexParams;

      public byte NumberOfValidPids;
      private byte Padding1;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidFilterPids)]
      public UInt16[] FilterPids;
      private UInt16 Padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FirmwareVersionInfo
    {
      public byte HardwareMajor;
      public byte HardwareMiddle;
      public byte HardwareMinor;
      public byte SoftwareMajor;
      public byte SoftwareMiddle;
      public byte SoftwareMinor;
      public byte BuildNumberMsb;
      public byte BuildNumberLsb;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FrontEndStatusInfo
    {
      public UInt32 Frequency;            // unit = kHz, intermediate frequency for DVB-S/2
      public UInt32 BitErrorRate;

      public byte SignalStrength;         // range = 0 - 100%
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding1;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLocked;

      public UInt16 CarrierToNoiseRatio;
      public byte AutomaticGainControl;
      private byte Value;                 // ???

      public DeFrontEndState FrontEndState;
      private byte Padding2;
      public DeCiState CiState;

      public byte SupplyVoltage;
      public byte AntennaVoltage;
      public byte BusVoltage;
      private byte Padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemInfo
    {
      public byte NumberOfAntennas;       // range = 0 - 3
      public DeAntennaType AntennaType;
      public DeBroadcastSystem BroadcastSystem;
      public DeTransportType TransportType;

      [MarshalAs(UnmanagedType.Bool)]
      public bool Lists;                  // ???
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DiseqcMessage
    {
      public byte MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LnbCommand
    {
      public byte Voltage;
      public De22k Tone22k;
      public DeToneBurst ToneBurst;
      public byte NumberOfMessages;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageCount)]
      public DiseqcMessage[] DiseqcMessages;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LnbParams
    {
      public UInt32 AntennaNumber;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsEast;

      public UInt16 OrbitalPosition;
      public UInt16 LowBandLof;           // unit = MHz
      public UInt16 SwitchFrequency;      // unit = MHz
      public UInt16 HighBandLof;          // unit = MHz
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LnbParamInfo
    {
      public Int32 NumberOfAntennas;       // range = 0 - 3
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLnbParamCount)]
      public LnbParams[] LnbParams;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct QpskTuneParams
    {
      public UInt32 Frequency;            // unit = kHz, range = 950000 - 2150000

      public UInt16 SymbolRate;           // unit = ks/s, range = 1000 - 40000
      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsHighBand;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct CiErrorDebugMessage
    {
      public byte MessageType;
      public byte MessageLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxCiErrorDebugMessageLength)]
      public String Message;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CaData
    {
      public byte Slot;
      public DeCiMessageTag Tag;
      private UInt16 Padding1;

      [MarshalAs(UnmanagedType.Bool)]
      public bool More;

      public UInt16 DataLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPmtLength)]
      public byte[] Data;
      private UInt16 Padding2;

      public CaData(DeCiMessageTag tag)
      {
        Slot = 0;
        Tag = tag;
        Padding1 = 0;
        More = false;
        DataLength = 0;
        Data = new byte[MaxPmtLength];
        Padding2 = 0;
      }
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba, 0xf3);

    private const int MaxPmtLength = 1024;
    private const int DvbsMultiplexParamsSize = 12;
    private const int DvbsServiceParamsSize = 36;
    private const int DvbsPidFilterParamsSize = 60;
    private const int DvbtMultiplexParamsSize = 12;
    private const int DvbtPidFilterParamsSize = 56;
    private const int MaxPidFilterPids = 16;
    private const int FirmwareVersionInfoSize = 8;
    private const int FrontEndStatusInfoSize = 28;
    private const int SystemInfoSize = 8;
    private const int DiseqcMessageSize = 7;
    private const int MaxDiseqcMessageLength = 6;
    private const int LnbCommandSize = 25;
    private const int MaxDiseqcMessageCount = 3;
    private const int LnbParamsSize = 16;
    private const int LnbParamInfoSize = 68;
    private const int MaxLnbParamCount = 4;
    private const int QpskTuneParamsSize = 12;
    private const int CiErrorDebugMessageLength = 258;
    private const int MaxCiErrorDebugMessageLength = 256;
    private const int CaDataSize = 1036;
    private const int DriverVersionInfoSize = 32;
    private const int TemperatureInfoSize = 4;

    private const int MmiHandlerThreadSleepTime = 500;    // unit = ms

    #endregion

    #region variables

    private bool _isDigitalEverywhere = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;

    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;
    private CardType _tunerType = CardType.Unknown;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread = false;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Get the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out DeCiState ciState)
    {
      ciState = DeCiState.Empty;

      // Use a local buffer here because this function is called from the MMI
      // polling thread as well as indirectly from the main TV service thread.
      int bufferSize = sizeof(DeCiState);   // 2 bytes
      IntPtr responsebuffer = Marshal.AllocCoTaskMem(bufferSize);
      int hr = 1;
      try
      {
        for (int i = 0; i < bufferSize; i++)
        {
          Marshal.WriteByte(responsebuffer, i, 0);
        }
        int returnedByteCount;
        hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.CiStatus,
          responsebuffer, bufferSize,
          responsebuffer, bufferSize,
          out returnedByteCount
        );
        if (hr == 0 && returnedByteCount == 2)
        {
          ciState = (DeCiState)Marshal.ReadInt16(responsebuffer, 0);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(responsebuffer);
      }
      return hr;
    }

    /// <summary>
    /// Send an MMI message to the CAM.
    /// </summary>
    /// <param name="data">The message.</param>
    /// <returns><c>true</c> if the message is successfully sent, otherwise <c>false</c></returns>
    private bool SendMmi(CaData data)
    {
      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("Digital Everywhere: the CAM is not ready");
        return false;
      }

      Marshal.StructureToPtr(data, _mmiBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiHostToCam,
        _mmiBuffer, CaDataSize,
        _mmiBuffer, CaDataSize
      );
      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region hardware/software information

    /// <summary>
    /// Attempt to read the driver information from the device.
    /// </summary>
    private void ReadDriverInfo()
    {
      Log.Debug("Digital Everywhere: read driver information");
      for (int i = 0; i < DriverVersionInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DriverVersion,
        _generalBuffer, DriverVersionInfoSize,
        _generalBuffer, DriverVersionInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != DriverVersionInfoSize)
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      //DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);
      Log.Debug("  driver version   = {0}", Marshal.PtrToStringAnsi(_generalBuffer));
    }

    /// <summary>
    /// Attempt to read the hardware and firmware information from the device.
    /// </summary>
    private void ReadHardwareInfo()
    {
      Log.Debug("Digital Everywhere: read hardware/firmware information");
      for (int i = 0; i < FirmwareVersionInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.FirmwareVersion,
        _generalBuffer, FirmwareVersionInfoSize,
        _generalBuffer, FirmwareVersionInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != FirmwareVersionInfoSize)
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      byte[] b = { 0, 0, 0, 0, 0, 0, 0, 0 };
      Marshal.Copy(_generalBuffer, b, 0, 8);
      Log.Debug("  hardware version = {0:x}.{1:x}.{2:x2}", b[0], b[1], b[2]);
      Log.Debug("  firmware version = {0:x}.{1:x}.{2:x2}", b[3], b[4], b[5]);
      Log.Debug("  firmware build # = {0}", (b[6] * 256) + b[7]);
    }

    /// <summary>
    /// Attempt to read the temperature from the device.
    /// </summary>
    private void ReadTemperature()
    {
      Log.Debug("Digital Everywhere: read temperature");
      for (int i = 0; i < TemperatureInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.Temperature,
        _generalBuffer, TemperatureInfoSize,
        _generalBuffer, TemperatureInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != TemperatureInfoSize)
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      // The output is all-zeroes for my FloppyDTV-S2 with the following details:
      //   driver version   = 5.0 (6201-3000) x64
      //   hardware version = 1.24.04
      //   firmware version = 1.5.02
      //   firmware build # = 30740
      DVB_MMI.DumpBinary(_generalBuffer, 0, TemperatureInfoSize);
    }

    /// <summary>
    /// Attempt to read the front end status from the device.
    /// </summary>
    private void ReadFrontEndStatus()
    {
      Log.Debug("Digital Everywhere: read front end status information");
      for (int i = 0; i < FrontEndStatusInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.FrontendStatus,
        _generalBuffer, FrontEndStatusInfoSize,
        _generalBuffer, FrontEndStatusInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != FrontEndStatusInfoSize)
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      // Most of this info is not very useful.
      //DVB_MMI.DumpBinary(_generalBuffer, 0, FrontEndStatusInfoSize);
      FrontEndStatusInfo status = (FrontEndStatusInfo)Marshal.PtrToStructure(_generalBuffer, typeof(FrontEndStatusInfo));
      Log.Debug("  frequency        = {0} kHz", status.Frequency);
      Log.Debug("  bit error rate   = {0}", status.BitErrorRate);
      Log.Debug("  signal strength  = {0}", status.SignalStrength);
      Log.Debug("  is locked        = {0}", status.IsLocked);
      Log.Debug("  CNR              = {0}", status.CarrierToNoiseRatio);
      Log.Debug("  auto gain ctrl   = {0}", status.AutomaticGainControl);
      Log.Debug("  front end state  = {0}", status.FrontEndState.ToString());
      Log.Debug("  CI state         = {0}", status.CiState.ToString());
      Log.Debug("  supply voltage   = {0}", status.SupplyVoltage);
      Log.Debug("  antenna voltage  = {0}", status.AntennaVoltage);
      Log.Debug("  bus voltage      = {0}", status.BusVoltage);
    }

    /// <summary>
    /// Read the conditional access application information from the CAM.
    /// </summary>
    private void ReadApplicationInformation()
    {
      Log.Debug("Digital Everywhere: request application information");
      CaData data = new CaData(DeCiMessageTag.ApplicationInfo);
      Marshal.StructureToPtr(data, _generalBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiCamToHost,
        _generalBuffer, CaDataSize,
        _generalBuffer, CaDataSize
      );
      if (hr != 0)
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Log.Debug("Digital Everywhere: read application information");
      for (int i = 0; i < CaDataSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiCamToHost,
        _generalBuffer, CaDataSize,
        _generalBuffer, CaDataSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != CaDataSize)
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      data = (CaData)Marshal.PtrToStructure(_generalBuffer, typeof(CaData));
      Log.Debug("  manufacturer = 0x{0:x}{1:x}", data.Data[0], data.Data[1]);
      Log.Debug("  code         = 0x{0:x}{1:x}", data.Data[2], data.Data[3]);
      Log.Debug("  menu title   = {0}", System.Text.Encoding.ASCII.GetString(data.Data, 5, data.Data[4]));
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if there is no purpose for it.
      if (!_isDigitalEverywhere)
      {
        return;
      }

      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        Log.Debug("Digital Everywhere: aborting old MMI handler thread");
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Debug("Digital Everywhere: starting new MMI handler thread");
        _stopMmiHandlerThread = false;
        _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
        _mmiHandlerThread.Name = "Digital Everywhere MMI handler";
        _mmiHandlerThread.IsBackground = true;
        _mmiHandlerThread.Priority = ThreadPriority.Lowest;
        _mmiHandlerThread.Start();
      }
    }

    /// <summary>
    /// Thread function for handling message passing to and from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      Log.Debug("Digital Everywhere: MMI handler thread start polling");
      DeCiState ciState = DeCiState.Empty;
      DeCiState prevCiState = DeCiState.Empty;

      try
      {
        while (!_stopMmiHandlerThread)
        {
          Thread.Sleep(MmiHandlerThreadSleepTime);

          int hr = GetCiStatus(out ciState);
          if (hr != 0)
          {
            Log.Debug("Digital Everywhere: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            Log.Debug("Digital Everywhere: CI state change");
            Log.Debug("  old state = {0}", prevCiState.ToString());
            Log.Debug("  new state = {0}", ciState.ToString());
            prevCiState = ciState;

            if ((ciState & DeCiState.CamPresent) != 0 &&
              (ciState & DeCiState.CamIsDvb) != 0)
            {
              _isCamPresent = true;
              if ((ciState & DeCiState.CamError) == 0 &&
                (ciState & DeCiState.CamReady) != 0 &&
                (ciState & DeCiState.ApplicationInfoAvailable) != 0)
              {
                _isCamReady = true;
              }
              else
              {
                _isCamReady = false;
              }
            }
            else
            {
              _isCamPresent = false;
            }
          }

          // If there is no CAM present or the CAM is not ready for interaction
          // then don't attempt to communicate with the CI.
          if (!_isCamReady)
          {
            continue;
          }

          // Check for MMI responses and requests.
          if ((ciState & DeCiState.MmiRequest) != 0)
          {
            Log.Debug("Digital Everywhere: MMI data available, sending request");
            CaData data = new CaData(DeCiMessageTag.Mmi);
            Marshal.StructureToPtr(data, _mmiBuffer, true);
            hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiCamToHost,
              _mmiBuffer, CaDataSize,
              _mmiBuffer, CaDataSize
            );
            if (hr != 0)
            {
              Log.Debug("Digital Everywhere: request failed, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              continue;
            }

            Log.Debug("Digital Everywhere: retrieving data");
            for (int i = 0; i < CaDataSize; i++)
            {
              Marshal.WriteByte(_mmiBuffer, i, 0);
            }
            int returnedByteCount;
            hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiCamToHost,
              _mmiBuffer, CaDataSize,
              _mmiBuffer, CaDataSize,
              out returnedByteCount
            );
            if (hr != 0)
            {
              Log.Debug("Digital Everywhere: failed to retrieve data, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              continue;
            }

            Log.Debug("Digital Everywhere: handling data");
            data = (CaData)Marshal.PtrToStructure(_mmiBuffer, typeof(CaData));
            byte[] objectData = new byte[data.DataLength];
            Array.Copy(data.Data, objectData, data.DataLength);
            DvbMmiHandler.HandleMmiData(objectData, ref _ciMenuCallbacks);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Debug("Digital Everywhere: exception in MMI handler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "Digital Everywhere";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("Digital Everywhere: initialising device");

      if (tunerFilter == null)
      {
        Log.Debug("Digital Everywhere: tuner filter is null");
        return false;
      }
      if (_isDigitalEverywhere)
      {
        Log.Debug("Digital Everywhere: device is already initialised");
        return true;
      }

      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Debug("Digital Everywhere: tuner filter is not a property set");
        return false;
      }

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.TestInterface,
        IntPtr.Zero, 0, IntPtr.Zero, 0
      );
      if (hr != 0)
      {
        Log.Debug("Digital Everywhere: device does not support the Digital Everywhere property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      Log.Debug("Digital Everywhere: supported device detected");
      _isDigitalEverywhere = true;
      _tunerType = tunerType;
      _generalBuffer = Marshal.AllocCoTaskMem(CaDataSize);

      ReadDriverInfo();
      ReadHardwareInfo();
      ReadTemperature();
      ReadFrontEndStatus();
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      Log.Debug("Digital Everywhere: on before tune callback");
      action = DeviceAction.Default;

      if (!_isDigitalEverywhere)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return;
      }

      // We need to tweak the modulation and inner FEC rate, but only for DVB-S/2 channels.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }
      if (ch.ModulationType == ModulationType.ModQpsk)
      {
        ch.ModulationType = ModulationType.ModNbcQpsk;
      }
      else if (ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.ModNbc8Psk;
      }
      else if (ch.ModulationType == ModulationType.ModNotSet)
      {
        ch.ModulationType = ModulationType.ModQpsk;
        // For DVB-S, pilot and roll-off should be "not set", however we're not
        // going to force this.
      }
      Log.Debug("  modulation     = {0}", ch.ModulationType);

      if (ch.InnerFecRate != BinaryConvolutionCodeRate.RateNotSet)
      {
        // Digital Everywhere uses the inner FEC rate parameter to encode the
        // pilot and roll-off as well as the FEC rate.
        int rate = (int)ch.InnerFecRate;
        if (ch.Pilot == Pilot.Off)
        {
          rate += 64;
        }
        else if (ch.Pilot == Pilot.On)
        {
          rate += 128;
        }

        if (ch.RollOff == RollOff.Twenty)
        {
          rate += 16;
        }
        else if (ch.RollOff == RollOff.TwentyFive)
        {
          rate += 32;
        }
        else if (ch.RollOff == RollOff.ThirtyFive)
        {
          rate += 48;
        }
        ch.InnerFecRate = (BinaryConvolutionCodeRate)rate;
      }
      Log.Debug("  inner FEC rate = {0}", ch.InnerFecRate);
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted, when the device is running but before
    /// signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnRunning(ITVCard tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      Log.Debug("Digital Everywhere: set power state, on = {0}", powerOn);

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }

      // The FloppyDTV and FireDTV S and S2 support this function; the other Digital Everywhere tuners do not.
      // Apparently the FireDTV T also supports active antennas but it is unclear whether and how that power
      // supply might be turned on or off.
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Digital Everywhere: property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      if (powerOn)
      {
        Marshal.WriteByte(_generalBuffer, 0, (byte)DeLnbPower.On);
      }
      else
      {
        Marshal.WriteByte(_generalBuffer, 0, (byte)DeLnbPower.Off);
      }
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower,
        _generalBuffer, 1,
        _generalBuffer, 1
      );
      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IPidFilterController

    /// <summary>
    /// Configure the PID filter.
    /// </summary>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <param name="modulation">The current multiplex/transponder modulation scheme.</param>
    /// <param name="forceEnable">Set this parameter to <c>true</c> to force the filter to be enabled.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    public bool SetFilterPids(HashSet<UInt16> pids, ModulationType modulation, bool forceEnable)
    {
      Log.Debug("Digital Everywhere: set PID filter PIDs, modulation = {0}, force enable = {1}", modulation, forceEnable);

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }

      if (_tunerType != CardType.DvbS && _tunerType != CardType.DvbT && _tunerType != CardType.DvbC)
      {
        Log.Debug("Digital Everywhere: PID filtering not supported");
        // Don't bother retrying...
        return true;
      }

      // It is not ideal to have to enable PID filtering because doing so can limit
      // the number of channels that can be viewed/recorded simultaneously. However,
      // it does seem that there is a need for filtering on satellite transponders
      // with high data rates. Problems have been observed with transponders on Thor
      // 5/6, Intelsat 10-02 (0.8W) if the filter is not enabled:
      //   Symbol Rate: 27500, Modulation: 8 PSK, FEC rate: 5/6, Pilot: On, Roll-Off: 0.35
      //   Symbol Rate: 30000, Modulation: 8 PSK, FEC rate: 3/4, Pilot: On, Roll-Off: 0.35
      bool fullTransponder = true;
      ushort[] filterPids = new ushort[MaxPidFilterPids];
      byte validPidCount = 0;
      if (pids == null || pids.Count == 0 || (modulation != ModulationType.Mod8Psk && !forceEnable))
      {
        Log.Debug("Digital Everywhere: disabling PID filter");
      }
      else
      {
        // If we get to here then the default approach is to enable the filter, but
        // there is one other constraint that applies: the filter PID limit.
        fullTransponder = false;
        if (pids.Count > MaxPidFilterPids)
        {
          Log.Debug("Digital Everywhere: too many PIDs, hardware limit = {0}, actual count = {1}", MaxPidFilterPids, pids.Count);
          // When the forceEnable flag is set, we just set as many PIDs as possible.
          if (!forceEnable)
          {
            Log.Debug("Digital Everywhere: disabling PID filter");
            fullTransponder = true;
          }
        }

        if (!fullTransponder)
        {
          Log.Debug("Digital Everywhere: enabling PID filter");

          fullTransponder = false;
          HashSet<UInt16>.Enumerator en = pids.GetEnumerator();
          while (en.MoveNext() && validPidCount < MaxPidFilterPids)
          {
            filterPids[validPidCount++] = en.Current;
          }
        }
      }

      BdaExtensionProperty property = BdaExtensionProperty.SelectPidsDvbS;
      int bufferSize = DvbsPidFilterParamsSize;
      if (_tunerType == CardType.DvbS)
      {
        DvbsPidFilterParams filter = new DvbsPidFilterParams();
        filter.CurrentTransponder = true;
        filter.FullTransponder = fullTransponder;
        filter.NumberOfValidPids = validPidCount;
        filter.FilterPids = filterPids;
        Marshal.StructureToPtr(filter, _generalBuffer, true);
      }
      else if (_tunerType == CardType.DvbT || _tunerType == CardType.DvbC)
      {
        property = BdaExtensionProperty.SelectPidsDvbT;
        bufferSize = DvbtPidFilterParamsSize;
        DvbtPidFilterParams filter = new DvbtPidFilterParams();
        filter.CurrentTransponder = true;
        filter.FullTransponder = fullTransponder;
        filter.NumberOfValidPids = validPidCount;
        filter.FilterPids = filterPids;
        Marshal.StructureToPtr(filter, _generalBuffer, true);
      }

      //DVB_MMI.DumpBinary(_generalBuffer, 0, bufferSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)property,
        _generalBuffer, bufferSize,
        _generalBuffer, bufferSize
      );
      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the device implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      // Tuning of DVB-S/2 and DVB-T/2 channels is supported with an appropriate tuner. DVB-C tuning may also be
      // supported but documentation is missing.
      if ((channel is DVBSChannel && _tunerType == CardType.DvbS) || (channel is DVBTChannel && _tunerType == CardType.DvbT))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      Log.Debug("Digital Everywhere: tune to channel");

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }

      int hr;

      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel != null && _tunerType == CardType.DvbS)
      {
        // LNB settings must be applied.
        LnbParamInfo lnbParams = new LnbParamInfo();
        lnbParams.NumberOfAntennas = 1;
        lnbParams.LnbParams = new LnbParams[MaxLnbParamCount];
        lnbParams.LnbParams[0].AntennaNumber = 0;
        lnbParams.LnbParams[0].IsEast = true;
        lnbParams.LnbParams[0].OrbitalPosition = 160;
        lnbParams.LnbParams[0].LowBandLof = (UInt16)(dvbsChannel.LnbType.LowBandFrequency / 1000);
        lnbParams.LnbParams[0].SwitchFrequency = (UInt16)(dvbsChannel.LnbType.SwitchFrequency / 1000);
        lnbParams.LnbParams[0].HighBandLof = (UInt16)(dvbsChannel.LnbType.HighBandFrequency / 1000);

        Marshal.StructureToPtr(lnbParams, _generalBuffer, true);
        //DVB_MMI.DumpBinary(_generalBuffer, 0, LnbParamInfoSize);

        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.SetLnbParams,
          _generalBuffer, LnbParamInfoSize,
          _generalBuffer, LnbParamInfoSize
        );
        if (hr != 0)
        {
          Log.Debug("Digital Everywhere: failed to apply LNB settings, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }

        DvbsMultiplexParams tuneRequest = new DvbsMultiplexParams();
        tuneRequest.Frequency = (UInt32)dvbsChannel.Frequency;
        tuneRequest.SymbolRate = (UInt32)dvbsChannel.SymbolRate;
        tuneRequest.Lnb = 0;    // To match the AntennaNumber value above.

        // OnBeforeTune() mixed pilot and roll-off settings into the top four bits of the least significant
        // inner FEC rate byte. It seemms that this custom tuning method doesn't support setting pilot and
        // roll-off in this way, so we throw the bits away.
        BinaryConvolutionCodeRate rate = (BinaryConvolutionCodeRate)((int)dvbsChannel.InnerFecRate & 0xf);
        if (rate == BinaryConvolutionCodeRate.Rate1_2)
        {
          tuneRequest.InnerFecRate = DeFecRate.Rate1_2;
        }
        else if (rate == BinaryConvolutionCodeRate.Rate2_3)
        {
          tuneRequest.InnerFecRate = DeFecRate.Rate2_3;
        }
        else if (rate == BinaryConvolutionCodeRate.Rate3_4)
        {
          tuneRequest.InnerFecRate = DeFecRate.Rate3_4;
        }
        else if (rate == BinaryConvolutionCodeRate.Rate5_6)
        {
          tuneRequest.InnerFecRate = DeFecRate.Rate5_6;
        }
        else if (rate == BinaryConvolutionCodeRate.Rate7_8)
        {
          tuneRequest.InnerFecRate = DeFecRate.Rate7_8;
        }
        else
        {
          tuneRequest.InnerFecRate = DeFecRate.Auto;
        }

        tuneRequest.Polarisation = DePolarisation.Vertical;
        if (dvbsChannel.Polarisation == Polarisation.LinearH || dvbsChannel.Polarisation == Polarisation.CircularL)
        {
          tuneRequest.Polarisation = DePolarisation.Horizontal;
        }

        Marshal.StructureToPtr(tuneRequest, _generalBuffer, true);
        //DVB_MMI.DumpBinary(_generalBuffer, 0, DvbsMultiplexParamsSize);

        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.SelectMultiplexDvbS,
          _generalBuffer, DvbsMultiplexParamsSize,
          _generalBuffer, DvbsMultiplexParamsSize
        );
      }
      else
      {
        DVBTChannel dvbtChannel = channel as DVBTChannel;
        if (dvbtChannel is DVBTChannel && _tunerType == CardType.DvbT)
        {
          DvbtMultiplexParams tuneRequest = new DvbtMultiplexParams();
          tuneRequest.Frequency = (UInt32)dvbtChannel.Frequency;
          tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth8;
          if (dvbtChannel.BandWidth == 7)
          {
            tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth7;
          }
          else if (dvbtChannel.BandWidth == 6)
          {
            tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth6;
          }
          tuneRequest.Constellation = DeOfdmConstellation.Auto;
          tuneRequest.CodeRateHp = DeOfdmCodeRate.Auto;
          tuneRequest.CodeRateLp = DeOfdmCodeRate.Auto;
          tuneRequest.GuardInterval = DeOfdmGuardInterval.Auto;
          tuneRequest.TransmissionMode = DeOfdmTransmissionMode.Auto;
          tuneRequest.Hierarchy = DeOfdmHierarchy.Auto;

          Marshal.StructureToPtr(tuneRequest, _generalBuffer, true);
          DVB_MMI.DumpBinary(_generalBuffer, 0, DvbtMultiplexParamsSize);
          hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.SelectMultiplexDvbT,
            _generalBuffer, DvbtMultiplexParamsSize,
            _generalBuffer, DvbtMultiplexParamsSize
          );
        }
        else
        {
          Log.Debug("Digital Everywhere: tuning is not supported for this channel");
          return false;
        }
      }

      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenInterface()
    {
      Log.Debug("Digital Everywhere: open conditional access interface");

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }
      if (_mmiBuffer != IntPtr.Zero)
      {
        Log.Debug("Digital Everywhere: interface is already open");
        return false;
      }

      _mmiBuffer = Marshal.AllocCoTaskMem(CaDataSize);
      _pmtBuffer = Marshal.AllocCoTaskMem(CaDataSize);
      _isCamReady = IsInterfaceReady();
      if (_isCamReady)
      {
        ReadApplicationInformation();
      }

      StartMmiHandlerThread();

      Log.Debug("Digital Everywhere: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      Log.Debug("Digital Everywhere: close conditional access interface");
      if (_mmiHandlerThread != null && _mmiHandlerThread.IsAlive)
      {
        _stopMmiHandlerThread = true;
        // In the worst case scenario it should take approximately
        // twice the thread sleep time to cleanly stop the thread.
        Thread.Sleep(MmiHandlerThreadSleepTime * 2);
        _mmiHandlerThread = null;
      }

      _isCamReady = false;
      if (_mmiBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
      }
      if (_pmtBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_pmtBuffer);
        _pmtBuffer = IntPtr.Zero;
      }

      Log.Debug("Digital Everywhere: result = true");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      Log.Debug("Digital Everywhere: reset conditional access interface");

      rebuildGraph = false;

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }

      bool success = CloseInterface();

      CaData data = new CaData(DeCiMessageTag.Reset);
      data.DataLength = 1;
      data.Data[0] = (byte)DeResetType.ForcedHardwareReset;

      Marshal.StructureToPtr(data, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, CaDataSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiHostToCam,
        _generalBuffer, CaDataSize,
        _generalBuffer, CaDataSize
      );
      if (hr == 0)
      {
        Log.WriteFile("Digital Everywhere: result = success");
      }
      else
      {
        Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }
      return success && OpenInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      Log.Debug("Digital Everywhere: is conditional access interface ready");

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }

      DeCiState ciState;
      int hr = GetCiStatus(out ciState);
      if (hr != 0)
      {
        Log.Debug("Digital Everywhere: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      Log.Debug("Digital Everywhere: CI state = {0}", ciState.ToString());
      bool camReady = false;
      if ((ciState & DeCiState.CamPresent) != 0 &&
        (ciState & DeCiState.CamIsDvb) != 0 &&
        (ciState & DeCiState.CamError) == 0 &&
        (ciState & DeCiState.CamReady) != 0 &&
        (ciState & DeCiState.ApplicationInfoAvailable) != 0)
      {
        camReady = true;
      }
      Log.Debug("Digital Everywhere: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The programme map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      Log.Debug("Digital Everywhere: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }
      if (pmt == null)
      {
        Log.Debug("Digital Everywhere: PMT not supplied");
        return true;
      }

      ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
      if (rawPmt.Count > MaxPmtLength - 2)
      {
        Log.Debug("Digital Everywhere: buffer capacity too small");
        return false;
      }

      CaData data = new CaData(DeCiMessageTag.Pmt);
      data.DataLength = (ushort)(rawPmt.Count + 2);
      data.Data[0] = (byte)listAction;
      data.Data[1] = (byte)command;

      rawPmt.CopyTo(data.Data, 2);

      int hr;
      lock (this)
      {
        Marshal.StructureToPtr(data, _pmtBuffer, true);
        //DVB_MMI.DumpBinary(_pmtBuffer, 0, CaDataSize);

        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiHostToCam,
          _pmtBuffer, CaDataSize,
          _pmtBuffer, CaDataSize
        );
      }
      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      // Failure indicates a Firewire communication problem.
      // Success does *not* indicate that the service will be descrambled.
      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        StartMmiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      Log.Debug("Digital Everywhere: enter menu");
      CaData data = new CaData(DeCiMessageTag.EnterMenu);
      return SendMmi(data);
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Debug("Digital Everywhere: close menu");
      CaData data = new CaData(DeCiMessageTag.Mmi);
      byte[] apdu = DvbMmiHandler.CreateMmiClose(0);
      data.DataLength = (UInt16)apdu.Length;
      Buffer.BlockCopy(apdu, 0, data.Data, 0, apdu.Length);
      return SendMmi(data);
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Debug("Digital Everywhere: select menu entry, choice = {0}", choice);
      CaData data = new CaData(DeCiMessageTag.Mmi);
      byte[] apdu = DvbMmiHandler.CreateMmiMenuAnswer(choice);
      data.DataLength = (UInt16)apdu.Length;
      Buffer.BlockCopy(apdu, 0, data.Data, 0, apdu.Length);
      return SendMmi(data);
    }

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Debug("Digital Everywhere: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      CaData data = new CaData(DeCiMessageTag.Mmi);
      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }
      byte[] apdu = DvbMmiHandler.CreateMmiEnquiryAnswer(responseType, answer);
      data.DataLength = (UInt16)apdu.Length;
      Buffer.BlockCopy(apdu, 0, data.Data, 0, apdu.Length);
      return SendMmi(data);
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      // TODO: this function needs to be tested. I'm uncertain whether
      // the driver will accept commands with no DiSEqC messages.
      Log.Debug("Digital Everywhere: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }

      LnbCommand lnbCommand = new LnbCommand();
      lnbCommand.Voltage = 0xff;
      lnbCommand.Tone22k = De22k.Off;
      if (tone22kState == Tone22k.On)
      {
        lnbCommand.Tone22k = De22k.On;
      }
      lnbCommand.ToneBurst = DeToneBurst.Undefined;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        lnbCommand.ToneBurst = DeToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        lnbCommand.ToneBurst = DeToneBurst.DataBurst;
      }
      lnbCommand.NumberOfMessages = 0;

      Marshal.StructureToPtr(lnbCommand, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, LnbCommandSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LnbCommandSize,
        _generalBuffer, LnbCommandSize
      );
      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      Log.Debug("Digital Everywhere: send DiSEqC command");

      if (!_isDigitalEverywhere || _propertySet == null)
      {
        Log.Debug("Digital Everywhere: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Digital Everywhere: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Debug("Digital Everywhere: command too long, length = {0}", command.Length);
        return false;
      }

      LnbCommand lnbCommand = new LnbCommand();
      lnbCommand.Voltage = 0xff;
      lnbCommand.Tone22k = De22k.Undefined;
      lnbCommand.ToneBurst = DeToneBurst.Undefined;
      lnbCommand.NumberOfMessages = 1;
      lnbCommand.DiseqcMessages = new DiseqcMessage[MaxDiseqcMessageCount];
      lnbCommand.DiseqcMessages[0].MessageLength = (byte)command.Length;
      lnbCommand.DiseqcMessages[0].Message = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, lnbCommand.DiseqcMessages[0].Message, 0, command.Length);

      Marshal.StructureToPtr(lnbCommand, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, LnbCommandSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LnbCommandSize,
        _generalBuffer, LnbCommandSize
      );
      if (hr == 0)
      {
        Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      CloseInterface();
      _propertySet = null;
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isDigitalEverywhere = false;
    }

    #endregion
  }
}