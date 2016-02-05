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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;
using Polarisation = Mediaportal.TV.Server.Common.Types.Enum.Polarisation;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalEverywhere
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC and PID filtering for Digital Everywhere tuners.
  /// </summary>
  public class DigitalEverywhere : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, ICustomTuner, IDiseqcDevice, IDisposable, IMpeg2PidFilter, IPowerDevice, IRemoteControlListener
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

    private enum DeFecCodeRate : byte
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

    private enum DeTone22kState : byte
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

    private enum DeRemoteCode
    {
      Power = 768,
      Sleep,
      StopEject,
      Okay,
      Right,
      One,
      Two,
      Three,
      Left,
      Four,
      Five,
      Six,
      Up, // 780
      Seven,
      Eight,
      Nine,
      Down,
      OnScreenDisplay,
      Zero,
      AspectRatio16_9,    // text: 16:9
      FullScreen,         // text: full
      Mute,
      Subtitles,  // 790
      Record,
      Teletext,
      Audio,
      Red,
      SkipBack,
      Rewind,
      PlayPause,
      SkipForward,
      VolumeUp, // 799

      ChannelUp = 832,
      AspectRatio4_3,     // text: 4:3
      Tv,
      Dvd,
      Vcr,
      Aux,
      Green,
      Yellow,
      Blue, // 840
      ChannelList,
      Ci,                 // (common interface)
      VolumeDown,
      ChannelDown,
      Recall,             // text: last
      Info,
      FastForward,
      List,
      Favourites,
      Menu,
      Epg,
      Exit  // 852
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DvbsMultiplexParams
    {
      public int Frequency;               // unit = kHz, range = 9750000 - 12750000
      public int SymbolRate;              // unit = ks/s, range = 1000 - 40000

      public DeFecCodeRate InnerFecRate;
      public DePolarisation Polarisation;
      public byte Lnb;                    // index (0..3) of the LNB parameters set with SetLnbParams
      private byte Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DvbsServiceParams
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool CurrentTransponder;
      public uint Lnb;                    // index (0..3) of the LNB parameters set with SetLnbParams

      public uint Frequency;              // unit = kHz, range = 9750000 - 12750000
      public uint SymbolRate;             // unit = ks/s, range = 1000 - 40000

      public DeFecCodeRate InnerFecRate;
      public DePolarisation Polarisation;
      private ushort Padding;

      public ushort OriginalNetworkId;
      public ushort TransportStreamId;
      public ushort ServiceId;
      public ushort VideoPid;
      public ushort AudioPid;
      public ushort PcrPid;
      public ushort TeletextPid;
      public ushort PmtPid;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DvbsPidFilterParams
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool CurrentTransponder;
      [MarshalAs(UnmanagedType.Bool)]
      public bool FullTransponder;

      public byte Lnb;                    // index (0..3) of the LNB parameters set with SetLnbParams
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding1;

      public uint Frequency;              // unit = kHz, range = 9750000 - 12750000
      public uint SymbolRate;             // unit = ks/s, range = 1000 - 40000

      public DeFecCodeRate InnerFecRate;
      public DePolarisation Polarisation;
      private ushort Padding2;

      public byte NumberOfValidPids;
      private byte Padding3;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PID_FILTER_PID_COUNT)]
      public ushort[] FilterPids;
      private ushort Padding4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DvbtMultiplexParams
    {
      public uint Frequency;              // unit = kHz, range = 47000 - 860000
      public DeOfdmBandwidth Bandwidth;
      public DeOfdmConstellation Constellation;
      public DeOfdmCodeRate CodeRateHp;
      public DeOfdmCodeRate CodeRateLp;
      public DeOfdmGuardInterval GuardInterval;
      public DeOfdmTransmissionMode TransmissionMode;
      public DeOfdmHierarchy Hierarchy;
      private byte Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DvbtPidFilterParams
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool CurrentTransponder;
      [MarshalAs(UnmanagedType.Bool)]
      public bool FullTransponder;

      public DvbtMultiplexParams MultiplexParams;

      public byte NumberOfValidPids;
      private byte Padding1;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PID_FILTER_PID_COUNT)]
      public ushort[] FilterPids;
      private ushort Padding2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FrontEndStatusInfo
    {
      public uint Frequency;              // unit = kHz, intermediate frequency for DVB-S/2
      public uint BitErrorRate;

      public byte SignalStrength;         // range = 0 - 100%
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding1;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLocked;

      public ushort CarrierToNoiseRatio;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SystemInfo
    {
      public byte NumberOfAntennas;       // range = 0 - 3
      public DeAntennaType AntennaType;
      public DeBroadcastSystem BroadcastSystem;
      public DeTransportType TransportType;

      [MarshalAs(UnmanagedType.Bool)]
      public bool Lists;                  // ???
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessage
    {
      public byte MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] Message;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LnbCommand
    {
      public byte Voltage;
      public DeTone22kState Tone22kState;
      public DeToneBurst ToneBurst;
      public byte NumberOfMessages;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_COUNT)]
      public DiseqcMessage[] DiseqcMessages;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LnbParams
    {
      public uint AntennaNumber;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsEast;

      public ushort OrbitalPosition;
      public ushort LowBandLof;             // unit = MHz
      public ushort SwitchFrequency;        // unit = MHz
      public ushort HighBandLof;            // unit = MHz
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LnbParamInfo
    {
      public int NumberOfAntennas;         // range = 0 - 3
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_LNB_PARAM_COUNT)]
      public LnbParams[] LnbParams;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct QpskTuneParams
    {
      public uint Frequency;                // unit = kHz, range = 950000 - 2150000

      public ushort SymbolRate;             // unit = ks/s, range = 1000 - 40000
      public DeFecCodeRate InnerFecRate;
      public DePolarisation Polarisation;

      [MarshalAs(UnmanagedType.Bool)]
      public bool IsHighBand;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct CiErrorDebugMessage
    {
      public byte MessageType;
      public byte MessageLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_CI_ERROR_DEBUG_MESSAGE_LENGTH)]
      public string Message;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CaData
    {
      public byte Slot;
      public DeCiMessageTag Tag;
      private ushort Padding1;

      [MarshalAs(UnmanagedType.Bool)]
      public bool More;

      public ushort DataLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PMT_LENGTH)]
      public byte[] Data;
      private ushort Padding2;

      public CaData(DeCiMessageTag tag)
      {
        Slot = 0;
        Tag = tag;
        Padding1 = 0;
        More = false;
        DataLength = 0;
        Data = new byte[MAX_PMT_LENGTH];
        Padding2 = 0;
      }
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba, 0xf3);

    private const int MAX_PMT_LENGTH = 1024;
    private static readonly int DVBS_MULTIPLEX_PARAMS_SIZE = Marshal.SizeOf(typeof(DvbsMultiplexParams));     // 12
    private static readonly int DVBS_SERVICE_PARAMS_SIZE = Marshal.SizeOf(typeof(DvbsServiceParams));         // 36
    private static readonly int DVBS_PID_FILTER_PARAMS_SIZE = Marshal.SizeOf(typeof(DvbsPidFilterParams));    // 60
    private static readonly int DVBT_MULTIPLEX_PARAMS_SIZE = Marshal.SizeOf(typeof(DvbtMultiplexParams));     // 12
    private static readonly int DVBT_PID_FILTER_PARAMS_SIZE = Marshal.SizeOf(typeof(DvbtPidFilterParams));    // 56
    private const int MAX_PID_FILTER_PID_COUNT = 16;
    private static readonly int FIRMWARE_VERSION_INFO_SIZE = Marshal.SizeOf(typeof(FirmwareVersionInfo));     // 8
    private static readonly int FRONT_END_STATUS_INFO_SIZE = Marshal.SizeOf(typeof(FrontEndStatusInfo));      // 28
    private static readonly int SYSTEM_INFO_SIZE = Marshal.SizeOf(typeof(SystemInfo));            // 8
    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));      // 7
    private const int MAX_DISEQC_MESSAGE_LENGTH = 6;
    private static readonly int LNB_COMMAND_SIZE = Marshal.SizeOf(typeof(LnbCommand));            // 25
    private const int MAX_DISEQC_MESSAGE_COUNT = 3;
    private static readonly int LNB_PARAMS_SIZE = Marshal.SizeOf(typeof(LnbParams));              // 16
    private static readonly int LNB_PARAM_INFO_SIZE = Marshal.SizeOf(typeof(LnbParamInfo));       // 68
    private const int MAX_LNB_PARAM_COUNT = 4;
    private static readonly int QPSK_TUNE_PARAMS_SIZE = Marshal.SizeOf(typeof(QpskTuneParams));   // 12
    private static readonly int CI_ERROR_DEBUG_MESSAGE_LENGTH = Marshal.SizeOf(typeof(CiErrorDebugMessage));  // 258
    private const int MAX_CI_ERROR_DEBUG_MESSAGE_LENGTH = 256;
    private static readonly int CA_DATA_SIZE = Marshal.SizeOf(typeof(CaData));  // 1036
    private const int DRIVER_VERSION_INFO_SIZE = 32;
    private const int TEMPERATURE_INFO_SIZE = 4;
    private const int REMOTE_CONTROL_DATA_SIZE = 2;

    private static readonly int GENERAL_BUFFER_SIZE = new int[]
      {
        CA_DATA_SIZE, DRIVER_VERSION_INFO_SIZE, DVBS_MULTIPLEX_PARAMS_SIZE, DVBT_MULTIPLEX_PARAMS_SIZE,
        FIRMWARE_VERSION_INFO_SIZE, FRONT_END_STATUS_INFO_SIZE, LNB_COMMAND_SIZE, LNB_PARAM_INFO_SIZE,
        TEMPERATURE_INFO_SIZE
      }.Max();

    private const int MMI_HANDLER_THREAD_WAIT_TIME = 500;                 // unit = ms
    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    private bool _isDigitalEverywhere = false;
    private bool _isCaInterfaceOpen = false;
    #pragma warning disable 0414
    private bool _isCamPresent = false;
    #pragma warning restore 0414
    private bool _isCamReady = false;
    private HashSet<ushort> _pidFilterPids = new HashSet<ushort>();

    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;
    private BroadcastStandard _tunerSupportedBroadcastStandards = BroadcastStandard.Unknown;

    private Thread _mmiHandlerThread = null;
    private ManualResetEvent _mmiHandlerThreadStopEvent = null;
    private object _mmiLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    private bool _isRemoteControlInterfaceOpen = false;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;
    private Thread _remoteControlListenerThread = null;
    private ManualResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    /// <summary>
    /// Get the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out DeCiState ciState)
    {
      ciState = DeCiState.Empty;

      int bufferSize = sizeof(DeCiState);   // 2 bytes
      int hr = (int)NativeMethods.HResult.E_FAIL;
      lock (_mmiLock)
      {
        for (int i = 0; i < bufferSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        int returnedByteCount;
        hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.CiStatus,
          _mmiBuffer, bufferSize,
          _mmiBuffer, bufferSize,
          out returnedByteCount
        );
        if (hr == (int)NativeMethods.HResult.E_FAIL && returnedByteCount == bufferSize)
        {
          ciState = (DeCiState)Marshal.ReadInt16(_mmiBuffer, 0);
        }
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
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamReady)
      {
        this.LogError("Digital Everywhere: failed to send to CAM, the CAM is not ready");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        Marshal.StructureToPtr(data, _mmiBuffer, false);
        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiHostToCam,
          _mmiBuffer, CA_DATA_SIZE,
          _mmiBuffer, CA_DATA_SIZE
        );
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to send to CAM, hr = 0x{0:x}", hr);
      return false;
    }

    #region hardware/software information

    /// <summary>
    /// Attempt to read the driver information from the tuner.
    /// </summary>
    private void ReadDriverInfo()
    {
      this.LogDebug("Digital Everywhere: read driver information");
      for (int i = 0; i < DRIVER_VERSION_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DriverVersion,
        _generalBuffer, DRIVER_VERSION_INFO_SIZE,
        _generalBuffer, DRIVER_VERSION_INFO_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DRIVER_VERSION_INFO_SIZE)
      {
        this.LogWarn("Digital Everywhere: failed to read driver version, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      //Dump.DumpBinary(_generalBuffer, returnedByteCount);
      this.LogDebug("  driver version   = {0}", Marshal.PtrToStringAnsi(_generalBuffer));
    }

    /// <summary>
    /// Attempt to read the hardware and firmware information from the tuner.
    /// </summary>
    private void ReadHardwareInfo()
    {
      this.LogDebug("Digital Everywhere: read hardware/firmware information");
      for (int i = 0; i < FIRMWARE_VERSION_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.FirmwareVersion,
        _generalBuffer, FIRMWARE_VERSION_INFO_SIZE,
        _generalBuffer, FIRMWARE_VERSION_INFO_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != FIRMWARE_VERSION_INFO_SIZE)
      {
        this.LogWarn("Digital Everywhere: failed to read firmware version, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      byte[] b = { 0, 0, 0, 0, 0, 0, 0, 0 };
      Marshal.Copy(_generalBuffer, b, 0, 8);
      this.LogDebug("  hardware version = {0:x}.{1:x}.{2:x2}", b[0], b[1], b[2]);
      this.LogDebug("  firmware version = {0:x}.{1:x}.{2:x2}", b[3], b[4], b[5]);
      this.LogDebug("  firmware build # = {0}", (b[6] * 256) + b[7]);
    }

    /// <summary>
    /// Attempt to read the temperature from the tuner.
    /// </summary>
    private void ReadTemperature()
    {
      this.LogDebug("Digital Everywhere: read temperature");
      for (int i = 0; i < TEMPERATURE_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Temperature,
        _generalBuffer, TEMPERATURE_INFO_SIZE,
        _generalBuffer, TEMPERATURE_INFO_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != TEMPERATURE_INFO_SIZE)
      {
        this.LogWarn("Digital Everywhere: failed to read temperature, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      // The output is all-zeroes for my FloppyDTV-S2 with the following details:
      //   driver version   = 5.0 (6201-3000) x64
      //   hardware version = 1.24.04
      //   firmware version = 1.5.02
      //   firmware build # = 30740
      Dump.DumpBinary(_generalBuffer, TEMPERATURE_INFO_SIZE);
    }

    /// <summary>
    /// Attempt to read the front end status from the tuner.
    /// </summary>
    private void ReadFrontEndStatus()
    {
      this.LogDebug("Digital Everywhere: read front end status information");
      for (int i = 0; i < FRONT_END_STATUS_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.FrontendStatus,
        _generalBuffer, FRONT_END_STATUS_INFO_SIZE,
        _generalBuffer, FRONT_END_STATUS_INFO_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != FRONT_END_STATUS_INFO_SIZE)
      {
        this.LogWarn("Digital Everywhere: failed to read front end status information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      // Most of this info is not very useful.
      //Dump.DumpBinary(_generalBuffer, FRONT_END_STATUS_INFO_SIZE);
      FrontEndStatusInfo status = (FrontEndStatusInfo)Marshal.PtrToStructure(_generalBuffer, typeof(FrontEndStatusInfo));
      this.LogDebug("  frequency        = {0} kHz", status.Frequency);
      this.LogDebug("  bit error rate   = {0}", status.BitErrorRate);
      this.LogDebug("  signal strength  = {0}", status.SignalStrength);
      this.LogDebug("  is locked        = {0}", status.IsLocked);
      this.LogDebug("  CNR              = {0}", status.CarrierToNoiseRatio);
      this.LogDebug("  auto gain ctrl   = {0}", status.AutomaticGainControl);
      this.LogDebug("  front end state  = {0}", status.FrontEndState);
      this.LogDebug("  CI state         = {0}", status.CiState);
      this.LogDebug("  supply voltage   = {0}", status.SupplyVoltage);
      this.LogDebug("  antenna voltage  = {0}", status.AntennaVoltage);
      this.LogDebug("  bus voltage      = {0}", status.BusVoltage);
    }

    /// <summary>
    /// Read the conditional access application information from the CAM.
    /// </summary>
    private void ReadApplicationInformation()
    {
      this.LogDebug("Digital Everywhere: request application information");
      CaData data = new CaData(DeCiMessageTag.ApplicationInfo);
      lock (_mmiLock)
      {
        Marshal.StructureToPtr(data, _mmiBuffer, false);
        int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiCamToHost,
          _mmiBuffer, CA_DATA_SIZE,
          _mmiBuffer, CA_DATA_SIZE
        );
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Digital Everywhere: failed to request application information, hr = 0x{0:x}", hr);
          return;
        }

        this.LogDebug("Digital Everywhere: read application information");
        for (int i = 0; i < CA_DATA_SIZE; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        int returnedByteCount;
        hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiCamToHost,
          _mmiBuffer, CA_DATA_SIZE,
          _mmiBuffer, CA_DATA_SIZE,
          out returnedByteCount
        );
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != CA_DATA_SIZE)
        {
          this.LogWarn("Digital Everywhere: failed to read application information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return;
        }

        data = (CaData)Marshal.PtrToStructure(_mmiBuffer, typeof(CaData));
      }
      this.LogDebug("  manufacturer = 0x{0:x2}{1:x2}", data.Data[0], data.Data[1]);
      this.LogDebug("  code         = 0x{0:x2}{1:x2}", data.Data[2], data.Data[3]);
      this.LogDebug("  menu title   = {0}", DvbTextConverter.Convert(data.Data, data.Data[4], 5));
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread to receive MMI messages from the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isCaInterfaceOpen)
      {
        return;
      }

      lock (_mmiLock)
      {
        // Kill the existing thread if it is in "zombie" state.
        if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
        {
          StopMmiHandlerThread();
        }

        if (_mmiHandlerThread == null)
        {
          this.LogDebug("Digital Everywhere: starting new MMI handler thread");
          _mmiHandlerThreadStopEvent = new ManualResetEvent(false);
          _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
          _mmiHandlerThread.Name = "Digital Everywhere MMI handler";
          _mmiHandlerThread.IsBackground = true;
          _mmiHandlerThread.Priority = ThreadPriority.Lowest;
          _mmiHandlerThread.Start();
        }
      }
    }

    /// <summary>
    /// Stop the thread that receives MMI messages from the CAM.
    /// </summary>
    private void StopMmiHandlerThread()
    {
      lock (_mmiLock)
      {
        if (_mmiHandlerThread != null)
        {
          if (!_mmiHandlerThread.IsAlive)
          {
            this.LogWarn("Digital Everywhere: aborting old MMI handler thread");
            _mmiHandlerThread.Abort();
          }
          else
          {
            _mmiHandlerThreadStopEvent.Set();
            if (!_mmiHandlerThread.Join(MMI_HANDLER_THREAD_WAIT_TIME * 2))
            {
              this.LogWarn("Digital Everywhere: failed to join MMI handler thread, aborting thread");
              _mmiHandlerThread.Abort();
            }
          }
          _mmiHandlerThread = null;
          if (_mmiHandlerThreadStopEvent != null)
          {
            _mmiHandlerThreadStopEvent.Close();
            _mmiHandlerThreadStopEvent = null;
          }
        }
      }
    }

    /// <summary>
    /// Thread function for receiving MMI messages from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      this.LogDebug("Digital Everywhere: MMI handler thread start polling");
      DeCiState ciState = DeCiState.Empty;
      DeCiState prevCiState = DeCiState.Empty;

      try
      {
        while (!_mmiHandlerThreadStopEvent.WaitOne(MMI_HANDLER_THREAD_WAIT_TIME))
        {
          int hr = GetCiStatus(out ciState);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Digital Everywhere: failed to get CI status, hr = 0x{0:x}", hr);
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            this.LogInfo("Digital Everywhere: CI state change");
            this.LogInfo("  old state = {0}", prevCiState);
            this.LogInfo("  new state = {0}", ciState);
            prevCiState = ciState;

            if (ciState.HasFlag(DeCiState.CamPresent | DeCiState.CamIsDvb))
            {
              _isCamPresent = true;
              if (!ciState.HasFlag(DeCiState.CamError) &&
                ciState.HasFlag(DeCiState.CamReady | DeCiState.ApplicationInfoAvailable))
              {
                if (!_isCamReady)
                {
                  ReadApplicationInformation();
                }
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
              _isCamReady = false;
            }
          }

          // If there is no CAM present or the CAM is not ready for interaction
          // then don't attempt to communicate with the CI.
          if (!_isCamReady)
          {
            continue;
          }

          // Check for MMI responses and requests.
          if (ciState.HasFlag(DeCiState.MmiRequest))
          {
            this.LogDebug("Digital Everywhere: MMI data available, sending request");
            CaData data = new CaData(DeCiMessageTag.Mmi);
            lock (_mmiLock)
            {
              Marshal.StructureToPtr(data, _mmiBuffer, false);
              hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiCamToHost,
                _mmiBuffer, CA_DATA_SIZE,
                _mmiBuffer, CA_DATA_SIZE
              );
              if (hr != (int)NativeMethods.HResult.S_OK)
              {
                this.LogError("Digital Everywhere: MMI request failed, hr = 0x{0:x}", hr);
                continue;
              }

              this.LogDebug("Digital Everywhere: retrieving data");
              for (int i = 0; i < CA_DATA_SIZE; i++)
              {
                Marshal.WriteByte(_mmiBuffer, i, 0);
              }
              int returnedByteCount;
              hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiCamToHost,
                _mmiBuffer, CA_DATA_SIZE,
                _mmiBuffer, CA_DATA_SIZE,
                out returnedByteCount
              );
              if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != CA_DATA_SIZE)
              {
                this.LogError("Digital Everywhere: failed to retrieve MMI data, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
                continue;
              }

              this.LogDebug("Digital Everywhere: handling data");
              data = (CaData)Marshal.PtrToStructure(_mmiBuffer, typeof(CaData));
            }
            lock (_caMenuCallBackLock)
            {
              DvbMmiHandler.HandleMmiData(data.Data, _caMenuCallBack, data.DataLength);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Digital Everywhere: MMI handler thread exception");
        return;
      }
      this.LogDebug("Digital Everywhere: MMI handler thread stop polling");
    }

    #endregion

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isRemoteControlInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_remoteControlListenerThread != null && !_remoteControlListenerThread.IsAlive)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlListenerThread == null)
      {
        this.LogDebug("Digital Everywhere: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new ManualResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Digital Everywhere remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("Digital Everywhere: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("Digital Everywhere: failed to join remote control listener thread, aborting thread");
            _remoteControlListenerThread.Abort();
          }
        }
        _remoteControlListenerThread = null;
        if (_remoteControlListenerThreadStopEvent != null)
        {
          _remoteControlListenerThreadStopEvent.Close();
          _remoteControlListenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("Digital Everywhere: remote control listener thread start polling");
      int hr;
      int returnedByteCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          for (int i = 0; i < REMOTE_CONTROL_DATA_SIZE; i++)
          {
            Marshal.WriteByte(_remoteControlBuffer, i, 0);
          }
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.RemoteControlRegister,
            _remoteControlBuffer, REMOTE_CONTROL_DATA_SIZE,
            _remoteControlBuffer, REMOTE_CONTROL_DATA_SIZE,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != REMOTE_CONTROL_DATA_SIZE)
          {
            this.LogError("Digital Everywhere: failed to read remote control register, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            short code = Marshal.ReadInt16(_remoteControlBuffer, 0);
            if (code != 0)
            {
              this.LogDebug("Digital Everywhere: remote control key press, code = {0}", (DeRemoteCode)code);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Digital Everywhere: remote control listener thread exception");
        return;
      }
      this.LogDebug("Digital Everywhere: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Digital Everywhere";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Digital Everywhere: initialising");

      if (_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskDigital) == 0)
      {
        this.LogDebug("Digital Everywhere: tuner type not supported");
        return false;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Digital Everywhere: context is not a property set");
        return false;
      }

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.TestInterface,
        IntPtr.Zero, 0, IntPtr.Zero, 0
      );
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: property set not supported, hr = 0x{0:x}", hr);
        _propertySet = null;
        return false;
      }

      this.LogInfo("Digital Everywhere: extension supported");
      _isDigitalEverywhere = true;
      _tunerSupportedBroadcastStandards = tunerSupportedBroadcastStandards;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);

      ReadDriverInfo();
      ReadHardwareInfo();
      ReadTemperature();
      ReadFrontEndStatus();
      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Digital Everywhere: on before tune call back");
      action = TunerAction.Default;

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return;
      }

      // We need to tweak the modulation and inner FEC rate, but only for DVB-S/S2 channels.
      ChannelDvbS dvbsChannel = channel as ChannelDvbS;
      if (dvbsChannel != null)
      {
        this.LogDebug("  modulation    = {0}", ModulationType.ModQpsk);
        dvbsChannel.ModulationScheme = (ModulationSchemePsk)ModulationType.ModQpsk;
        return;
      }

      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (dvbs2Channel != null)
      {
        ModulationType bdaModulation = ModulationType.ModNotSet;
        switch (dvbs2Channel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk4:
            bdaModulation = ModulationType.ModNbcQpsk;
            break;
          case ModulationSchemePsk.Psk8:
            bdaModulation = ModulationType.ModNbc8Psk;
            break;
          default:
            this.LogWarn("Digital Everywhere: DVB-S2 tune request uses unsupported modulation scheme {0}, falling back to automatic", dvbs2Channel.ModulationScheme);
            break;
        }
        if (bdaModulation != ModulationType.ModNotSet)
        {
          this.LogDebug("  modulation    = {0}", bdaModulation);
          dvbs2Channel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
        }

        if (dvbs2Channel.FecCodeRate != FecCodeRate.Automatic)
        {
          // Digital Everywhere uses the inner FEC rate parameter to encode the
          // pilot tones state and roll-off factor as well as the FEC rate.
          BinaryConvolutionCodeRate bdaCodeRate = BinaryConvolutionCodeRate.RateNotSet;
          switch (dvbs2Channel.FecCodeRate)
          {
            case FecCodeRate.Rate1_2:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate1_2;
              break;
            case FecCodeRate.Rate1_3:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate1_3;
              break;
            case FecCodeRate.Rate1_4:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate1_4;
              break;
            case FecCodeRate.Rate2_3:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate2_3;
              break;
            case FecCodeRate.Rate2_5:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate2_5;
              break;
            case FecCodeRate.Rate3_4:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate3_4;
              break;
            case FecCodeRate.Rate3_5:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate3_5;
              break;
            case FecCodeRate.Rate4_5:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate4_5;
              break;
            case FecCodeRate.Rate5_11:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate5_11;
              break;
            case FecCodeRate.Rate5_6:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate5_6;
              break;
            case FecCodeRate.Rate6_7:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate6_7;
              break;
            case FecCodeRate.Rate7_8:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate7_8;
              break;
            case FecCodeRate.Rate8_9:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate8_9;
              break;
            case FecCodeRate.Rate9_10:
              bdaCodeRate = BinaryConvolutionCodeRate.Rate9_10;
              break;
            default:
              this.LogWarn("Digital Everywhere: tune request uses unsupported FEC code rate {0}, falling back to automatic", dvbs2Channel.FecCodeRate);
              break;
          }

          int codeRate = (int)bdaCodeRate;
          if (dvbs2Channel.PilotTonesState == PilotTonesState.Off)
          {
            codeRate += 64;
          }
          else if (dvbs2Channel.PilotTonesState == PilotTonesState.On)
          {
            codeRate += 128;
          }

          switch (dvbs2Channel.RollOffFactor)
          {
            case RollOffFactor.Twenty:
              codeRate += 16;
              break;
            case RollOffFactor.TwentyFive:
              codeRate += 32;
              break;
            case RollOffFactor.ThirtyFive:
              codeRate += 48;
              break;
            default:
              this.LogWarn("Digital Everywhere: DVB-S2 tune request uses unsupported roll-off factor {0}", dvbs2Channel.RollOffFactor);
              break;
          }

          dvbs2Channel.FecCodeRate = (FecCodeRate)codeRate;
          this.LogDebug("  FEC code rate = {0}", codeRate);
        }
      }
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnStarted(ITuner tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Digital Everywhere: set power state, state = {0}", state);

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      // The FloppyDTV and FireDTV S and S2 support this function; the other
      // Digital Everywhere tuners do not. Apparently the FireDTV T also
      // supports active antennas but it is unclear whether and how that power
      // supply might be turned on or off.
      if ((_tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Digital Everywhere: property not supported");
        return false;
      }
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Digital Everywhere: property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      if (state == PowerState.On)
      {
        Marshal.WriteByte(_generalBuffer, 0, (byte)DeLnbPower.On);
      }
      else
      {
        Marshal.WriteByte(_generalBuffer, 0, (byte)DeLnbPower.Off);
      }
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbPower,
        _generalBuffer, sizeof(Byte),
        _generalBuffer, sizeof(Byte)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to set power state, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IMpeg2PidFilter members

    /// <summary>
    /// Should the filter be enabled for a given transmitter.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning parameters.</param>
    /// <returns><c>true</c> if the filter should be enabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ShouldEnable(IChannel tuningDetail)
    {
      if ((_tunerSupportedBroadcastStandards & BroadcastStandard.MaskDvb) == 0)
      {
        // PID filtering not supported.
        return false;
      }

      // It is not ideal to have to enable PID filtering because doing so can
      // limit the number of channels that can be viewed/recorded
      // simultaneously. However, it does seem that there is a need for
      // filtering on satellite transponders with high bit rates. Problems have
      // been observed with transponders on Thor 5/6 and Intelsat 10-02 (0.8W)
      // if the filter is not enabled:
      //   Symbol Rate: 27500, Modulation: 8 PSK, FEC rate: 5/6, Pilot: On, Roll-Off: 0.35
      //   Symbol Rate: 30000, Modulation: 8 PSK, FEC rate: 3/4, Pilot: On, Roll-Off: 0.35
      int bitRate = 0;
      IChannelSatellite satelliteTuningDetail = tuningDetail as IChannelSatellite;
      if (satelliteTuningDetail != null)
      {
        int bitsPerSymbol = 2;  // QPSK
        switch (satelliteTuningDetail.ModulationScheme)
        {
          case ModulationSchemePsk.Psk2:
          case ModulationSchemePsk.Psk4SplitI:
          case ModulationSchemePsk.Psk4SplitQ:
            bitsPerSymbol = 1;
            break;
          case ModulationSchemePsk.Psk4:
          case ModulationSchemePsk.Psk4Offset:
            bitsPerSymbol = 2;
            break;
          case ModulationSchemePsk.Psk8:
            bitsPerSymbol = 3;
            break;

          // Not supported by the hardware.
          case ModulationSchemePsk.Psk16:
            bitsPerSymbol = 4;
            break;
          case ModulationSchemePsk.Psk32:
            bitsPerSymbol = 5;
            break;
          case ModulationSchemePsk.Psk64:
            bitsPerSymbol = 6;
            break;
          case ModulationSchemePsk.Psk128:
            bitsPerSymbol = 7;
            break;
          case ModulationSchemePsk.Psk256:
            bitsPerSymbol = 8;
            break;
        }

        // Other FEC code rates not supported by the hardware.
        bitRate = bitsPerSymbol * satelliteTuningDetail.SymbolRate; // kb/s
        switch (satelliteTuningDetail.FecCodeRate)
        {
          case FecCodeRate.Rate1_2:
            bitRate /= 2;
            break;
          case FecCodeRate.Rate1_3:
            bitRate /= 3;
            break;
          case FecCodeRate.Rate1_4:
            bitRate /= 4;
            break;
          case FecCodeRate.Rate2_3:
            bitRate = bitRate * 2 / 3;
            break;
          case FecCodeRate.Rate2_5:
            bitRate = bitRate * 2 / 5;
            break;
          case FecCodeRate.Rate3_4:
            bitRate = bitRate * 3 / 4;
            break;
          case FecCodeRate.Rate3_5:
            bitRate = bitRate * 3 / 5;
            break;
          case FecCodeRate.Rate4_5:
            bitRate = bitRate * 4 / 5;
            break;
          case FecCodeRate.Rate5_11:
            bitRate = bitRate * 5 / 11;
            break;
          case FecCodeRate.Rate5_6:
            bitRate = bitRate * 5 / 6;
            break;
          case FecCodeRate.Rate6_7:
            bitRate = bitRate * 6 / 7;
            break;
          case FecCodeRate.Rate7_8:
            bitRate = bitRate * 7 / 8;
            break;
          case FecCodeRate.Rate8_9:
            bitRate = bitRate * 8 / 9;
            break;
          case FecCodeRate.Rate9_10:
            bitRate = bitRate * 9 / 10;
            break;
        }
      }
      else
      {
        ChannelDvbC dvbcTuningDetail = tuningDetail as ChannelDvbC;
        if (dvbcTuningDetail == null)
        {
          return false;
        }

        int bitsPerSymbol = 6;  // 64 QAM
        switch (dvbcTuningDetail.ModulationScheme)
        {
          case ModulationSchemeQam.Qam16:
            bitsPerSymbol = 4;
            break;
          case ModulationSchemeQam.Qam32:
            bitsPerSymbol = 5;
            break;
          case ModulationSchemeQam.Qam64:
            bitsPerSymbol = 6;
            break;
          case ModulationSchemeQam.Qam128:
            bitsPerSymbol = 7;
            break;
          case ModulationSchemeQam.Qam256:
            bitsPerSymbol = 8;
            break;

          // Not supported by the hardware.
          case ModulationSchemeQam.Qam512:
            bitsPerSymbol = 9;
            break;
          case ModulationSchemeQam.Qam1024:
            bitsPerSymbol = 10;
            break;
          case ModulationSchemeQam.Qam2048:
            bitsPerSymbol = 11;
            break;
          case ModulationSchemeQam.Qam4096:
            bitsPerSymbol = 12;
            break;
        }
        bitRate = bitsPerSymbol * dvbcTuningDetail.SymbolRate;  // kb/s
      }

      // Rough approximation: enable PID filtering when bit rate is over 60 Mb/s.
      bool enableFilter = (bitRate >= 60000);
      this.LogDebug("Digital Everywhere: transport stream bit rate = {0} kb/s, need PID filter = {1}", bitRate, enableFilter);
      return enableFilter;
    }

    /// <summary>
    /// Disable the filter.
    /// </summary>
    /// <returns><c>true</c> if the filter is successfully disabled, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.Disable()
    {
      this.LogDebug("Digital Everywhere: disable PID filter");
      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      _pidFilterPids.Clear();
      int hr = ConfigurePidFilter(false);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to diable PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Get the maximum number of streams that the filter can allow.
    /// </summary>
    int IMpeg2PidFilter.MaximumPidCount
    {
      get
      {
        return MAX_PID_FILTER_PID_COUNT;
      }
    }

    /// <summary>
    /// Configure the filter to allow one or more streams to pass through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.AllowStreams(ICollection<ushort> pids)
    {
      _pidFilterPids.UnionWith(pids);
    }

    /// <summary>
    /// Configure the filter to stop one or more streams from passing through the filter.
    /// </summary>
    /// <param name="pids">A collection of stream identifiers.</param>
    void IMpeg2PidFilter.BlockStreams(ICollection<ushort> pids)
    {
      _pidFilterPids.ExceptWith(pids);
    }

    /// <summary>
    /// Apply the current filter configuration.
    /// </summary>
    /// <returns><c>true</c> if the filter configuration is successfully applied, otherwise <c>false</c></returns>
    bool IMpeg2PidFilter.ApplyConfiguration()
    {
      this.LogDebug("Digital Everywhere: apply PID filter configuration");
      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      int hr = ConfigurePidFilter(true);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to apply PID filter, hr = 0x{0:x}", hr);
      return false;
    }

    private int ConfigurePidFilter(bool enable)
    {
      ushort[] pids = new ushort[MAX_PID_FILTER_PID_COUNT];
      byte actualPidCount = (byte)Math.Min(_pidFilterPids.Count, MAX_PID_FILTER_PID_COUNT);
      _pidFilterPids.CopyTo(pids, 0, actualPidCount);

      BdaExtensionProperty property = BdaExtensionProperty.SelectPidsDvbS;
      int bufferSize = DVBS_PID_FILTER_PARAMS_SIZE;
      if ((_tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0)
      {
        DvbsPidFilterParams filter = new DvbsPidFilterParams();
        filter.CurrentTransponder = true;
        filter.FullTransponder = !enable;
        filter.NumberOfValidPids = actualPidCount;
        filter.FilterPids = pids;
        Marshal.StructureToPtr(filter, _generalBuffer, false);
      }
      else
      {
        // Yes, use the "DVB-T" structure for DVB-C! That is intentional.
        property = BdaExtensionProperty.SelectPidsDvbT;
        bufferSize = DVBT_PID_FILTER_PARAMS_SIZE;
        DvbtPidFilterParams filter = new DvbtPidFilterParams();
        filter.CurrentTransponder = true;
        filter.FullTransponder = !enable;
        filter.NumberOfValidPids = actualPidCount;
        filter.FilterPids = pids;
        Marshal.StructureToPtr(filter, _generalBuffer, false);
      }

      //Dump.DumpBinary(_generalBuffer, bufferSize);

      return _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)property,
        _generalBuffer, bufferSize,
        _generalBuffer, bufferSize
      );
    }

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the extension implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the extension supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      // Tuning of DVB-S and DVB-T channels is supported with an appropriate
      // tuner.
      // DVB-C tuning may also be supported but documentation is missing.
      // DVB-S2 tuning appears not to work.
      if (channel is ChannelDvbT && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT))
      {
        return true;
      }
      ChannelDvbS dvbsChannel = channel as ChannelDvbS;
      if (dvbsChannel != null && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbS))
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
      this.LogDebug("Digital Everywhere: tune to channel");

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      int hr;

      ChannelDvbS dvbsChannel = channel as ChannelDvbS;
      if (dvbsChannel != null && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbS))
      {
        // LNB settings must be applied.
        int lnbLowBandLof;
        int lnbHighBandLof;
        int lnbLofSwitch;
        Tone22kState bandSelectionTone;
        Polarisation bandSelectionPolarisation;
        dvbsChannel.LnbType.GetTuningParameters(dvbsChannel.Frequency, dvbsChannel.Polarisation, Tone22kState.Automatic, out lnbLowBandLof, out lnbHighBandLof, out lnbLofSwitch, out bandSelectionTone, out bandSelectionPolarisation);

        LnbParamInfo lnbParams = new LnbParamInfo();
        lnbParams.NumberOfAntennas = 1;
        lnbParams.LnbParams = new LnbParams[MAX_LNB_PARAM_COUNT];
        lnbParams.LnbParams[0].AntennaNumber = 0;
        lnbParams.LnbParams[0].IsEast = true;
        lnbParams.LnbParams[0].OrbitalPosition = 160;
        lnbParams.LnbParams[0].LowBandLof = (ushort)(lnbLowBandLof / 1000);
        lnbParams.LnbParams[0].SwitchFrequency = (ushort)(lnbLofSwitch / 1000);
        lnbParams.LnbParams[0].HighBandLof = (ushort)(lnbHighBandLof / 1000);

        Marshal.StructureToPtr(lnbParams, _generalBuffer, false);
        //Dump.DumpBinary(_generalBuffer, LNB_PARAM_INFO_SIZE);

        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.SetLnbParams,
          _generalBuffer, LNB_PARAM_INFO_SIZE,
          _generalBuffer, LNB_PARAM_INFO_SIZE
        );
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Digital Everywhere: failed to apply LNB settings, hr = 0x{0:x}", hr);
        }

        DvbsMultiplexParams tuneRequest = new DvbsMultiplexParams();
        tuneRequest.Frequency = dvbsChannel.Frequency;
        tuneRequest.SymbolRate = dvbsChannel.SymbolRate;
        tuneRequest.Lnb = 0;    // To match the AntennaNumber value above.

        switch (dvbsChannel.FecCodeRate)
        {
          case FecCodeRate.Rate1_2:
            tuneRequest.InnerFecRate = DeFecCodeRate.Rate1_2;
            break;
          case FecCodeRate.Rate2_3:
            tuneRequest.InnerFecRate = DeFecCodeRate.Rate2_3;
            break;
          case FecCodeRate.Rate3_4:
            tuneRequest.InnerFecRate = DeFecCodeRate.Rate3_4;
            break;
          case FecCodeRate.Rate5_6:
            tuneRequest.InnerFecRate = DeFecCodeRate.Rate5_6;
            break;
          case FecCodeRate.Rate7_8:
            tuneRequest.InnerFecRate = DeFecCodeRate.Rate7_8;
            break;
          default:
            this.LogWarn("Digital Everywhere: tune request uses unsupported FEC code rate {0}, falling back to automatic", dvbsChannel.FecCodeRate);
            tuneRequest.InnerFecRate = DeFecCodeRate.Auto;
            break;
        }

        tuneRequest.Polarisation = DePolarisation.Vertical;
        if (bandSelectionPolarisation == Polarisation.LinearHorizontal || bandSelectionPolarisation == Polarisation.CircularLeft)
        {
          tuneRequest.Polarisation = DePolarisation.Horizontal;
        }

        Marshal.StructureToPtr(tuneRequest, _generalBuffer, false);
        //Dump.DumpBinary(_generalBuffer, DVBS_MULTIPLEX_PARAMS_SIZE);

        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.SelectMultiplexDvbS,
          _generalBuffer, DVBS_MULTIPLEX_PARAMS_SIZE,
          _generalBuffer, DVBS_MULTIPLEX_PARAMS_SIZE
        );
      }
      else
      {
        ChannelDvbT dvbtChannel = channel as ChannelDvbT;
        if (dvbtChannel != null && _tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT))
        {
          DvbtMultiplexParams tuneRequest = new DvbtMultiplexParams();
          tuneRequest.Frequency = (uint)dvbtChannel.Frequency;
          switch (dvbtChannel.Bandwidth)
          {
            case 8000:
              tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth8;
              break;
            case 7000:
              tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth7;
              break;
            case 6000:
              tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth6;
              break;
            default:
              this.LogWarn("Digital Everywhere: tune request uses unsupported bandwidth {0} kHz, falling back to 8000 kHz", dvbtChannel.Bandwidth);
              tuneRequest.Bandwidth = DeOfdmBandwidth.Bandwidth8;
              break;
          }
          tuneRequest.Constellation = DeOfdmConstellation.Auto;
          tuneRequest.CodeRateHp = DeOfdmCodeRate.Auto;
          tuneRequest.CodeRateLp = DeOfdmCodeRate.Auto;
          tuneRequest.GuardInterval = DeOfdmGuardInterval.Auto;
          tuneRequest.TransmissionMode = DeOfdmTransmissionMode.Auto;
          tuneRequest.Hierarchy = DeOfdmHierarchy.Auto;

          Marshal.StructureToPtr(tuneRequest, _generalBuffer, false);
          Dump.DumpBinary(_generalBuffer, DVBT_MULTIPLEX_PARAMS_SIZE);
          hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.SelectMultiplexDvbT,
            _generalBuffer, DVBT_MULTIPLEX_PARAMS_SIZE,
            _generalBuffer, DVBT_MULTIPLEX_PARAMS_SIZE
          );
        }
        else
        {
          this.LogError("Digital Everywhere: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
          return false;
        }
      }

      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to tune, hr = 0x{0:x}{1}{2}", hr, Environment.NewLine, channel);
      return false;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("Digital Everywhere: open conditional access interface");

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Everywhere: conditional access interface is already open");
        return true;
      }

      _mmiBuffer = Marshal.AllocCoTaskMem(CA_DATA_SIZE);
      _pmtBuffer = Marshal.AllocCoTaskMem(CA_DATA_SIZE);
      _isCaInterfaceOpen = true;
      StartMmiHandlerThread();

      this.LogDebug("Digital Everywhere: result = success");
      return true;
    }

    /// <summary>
    /// Determine if the conditional access interface is open.
    /// </summary>
    /// <value><c>true</c> if the conditional access interface is open, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsOpen
    {
      get
      {
        return _isCaInterfaceOpen;
      }
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Close()
    {
      return CloseConditionalAccessInterface(true);
    }

    private bool CloseConditionalAccessInterface(bool isDisposing)
    {
      this.LogDebug("Digital Everywhere: close conditional access interface");

      if (isDisposing)
      {
        StopMmiHandlerThread();
      }

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

      _isCamPresent = false;
      _isCamReady = false;
      _isCaInterfaceOpen = false;

      this.LogDebug("Digital Everywhere: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      this.LogDebug("Digital Everywhere: reset conditional access interface");

      if (!_isDigitalEverywhere)
      {
        this.LogDebug("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      bool success = (this as IConditionalAccessProvider).Close();

      CaData data = new CaData(DeCiMessageTag.Reset);
      data.DataLength = 1;
      data.Data[0] = (byte)DeResetType.ForcedHardwareReset;

      Marshal.StructureToPtr(data, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, CA_DATA_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiHostToCam,
        _generalBuffer, CA_DATA_SIZE,
        _generalBuffer, CA_DATA_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
      }
      else
      {
        this.LogError("Digital Everywhere: failed to reset conditional access interface, hr = 0x{0:x}", hr);
        success = false;
      }
      return success && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("Digital Everywhere: is conditional access interface ready");

      if (!_isCaInterfaceOpen)
      {
        this.LogDebug("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      // The CAM state is updated by the MMI handler thread.
      this.LogDebug("Digital Everywhere: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <value><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsConditionalAccessTableRequiredForDecryption
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider)
    {
      this.LogDebug("Digital Everywhere: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("Digital Everywhere: failed to send conditional access command, the CAM is not ready");
        return false;
      }
      if (pmt == null)
      {
        this.LogError("Digital Everywhere: failed to send conditional access command, PMT not supplied");
        return true;
      }

      ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
      if (rawPmt.Count > MAX_PMT_LENGTH - 2)
      {
        this.LogError("Digital Everywhere: conditional access command too long, length = {0}", rawPmt.Count);
        return false;
      }

      CaData data = new CaData(DeCiMessageTag.Pmt);
      data.DataLength = (ushort)(rawPmt.Count + 2);
      data.Data[0] = (byte)listAction;
      data.Data[1] = (byte)command;

      rawPmt.CopyTo(data.Data, 2);

      Marshal.StructureToPtr(data, _pmtBuffer, false);
      //Dump.DumpBinary(_pmtBuffer, CA_DATA_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MmiHostToCam,
        _pmtBuffer, CA_DATA_SIZE,
        _pmtBuffer, CA_DATA_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      // Failure indicates a Firewire communication problem.
      // Success does *not* indicate that the service will be descrambled.
      this.LogError("Digital Everywhere: failed to send conditional access command, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
      StartMmiHandlerThread();
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      this.LogDebug("Digital Everywhere: enter menu");
      CaData data = new CaData(DeCiMessageTag.EnterMenu);
      return SendMmi(data);
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("Digital Everywhere: close menu");
      CaData data = new CaData(DeCiMessageTag.Mmi);
      byte[] apdu = DvbMmiHandler.CreateMmiClose(0);
      data.DataLength = (ushort)apdu.Length;
      Buffer.BlockCopy(apdu, 0, data.Data, 0, apdu.Length);
      return SendMmi(data);
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("Digital Everywhere: select menu entry, choice = {0}", choice);
      CaData data = new CaData(DeCiMessageTag.Mmi);
      byte[] apdu = DvbMmiHandler.CreateMmiMenuAnswer(choice);
      data.DataLength = (ushort)apdu.Length;
      Buffer.BlockCopy(apdu, 0, data.Data, 0, apdu.Length);
      return SendMmi(data);
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("Digital Everywhere: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      CaData data = new CaData(DeCiMessageTag.Mmi);
      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }
      byte[] apdu = DvbMmiHandler.CreateMmiEnquiryAnswer(responseType, answer);
      data.DataLength = (ushort)apdu.Length;
      Buffer.BlockCopy(apdu, 0, data.Data, 0, apdu.Length);
      return SendMmi(data);
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      this.LogDebug("Digital Everywhere: send DiSEqC command");

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Digital Everywhere: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Digital Everywhere: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      LnbCommand lnbCommand = new LnbCommand();
      lnbCommand.Voltage = 0xff;
      lnbCommand.Tone22kState = DeTone22kState.Undefined;
      lnbCommand.ToneBurst = DeToneBurst.Undefined;
      lnbCommand.NumberOfMessages = 1;
      lnbCommand.DiseqcMessages = new DiseqcMessage[MAX_DISEQC_MESSAGE_COUNT];
      lnbCommand.DiseqcMessages[0].MessageLength = (byte)command.Length;
      lnbCommand.DiseqcMessages[0].Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, lnbCommand.DiseqcMessages[0].Message, 0, command.Length);

      Marshal.StructureToPtr(lnbCommand, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, LNB_COMMAND_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LNB_COMMAND_SIZE,
        _generalBuffer, LNB_COMMAND_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <remarks>
    /// Don't know whether the driver will send a tone burst command without a
    /// DiSEqC command.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Digital Everywhere: send tone burst command, command = {0}", command);

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      LnbCommand lnbCommand = new LnbCommand();
      lnbCommand.Voltage = 0xff;
      lnbCommand.Tone22kState = DeTone22kState.Undefined;
      if (command == ToneBurst.ToneBurst)
      {
        lnbCommand.ToneBurst = DeToneBurst.ToneBurst;
      }
      else if (command == ToneBurst.DataBurst)
      {
        lnbCommand.ToneBurst = DeToneBurst.DataBurst;
      }
      lnbCommand.NumberOfMessages = 0;

      Marshal.StructureToPtr(lnbCommand, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, LNB_COMMAND_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LNB_COMMAND_SIZE,
        _generalBuffer, LNB_COMMAND_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// Don't know whether the driver will set the tone state without a DiSEqC
    /// command.
    /// </remarks>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("Digital Everywhere: set tone state, state = {0}", state);

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }

      LnbCommand lnbCommand = new LnbCommand();
      lnbCommand.Voltage = 0xff;
      lnbCommand.ToneBurst = DeToneBurst.Undefined;
      if (state == Tone22kState.Off)
      {
        lnbCommand.Tone22kState = DeTone22kState.Off;
      }
      else if (state == Tone22kState.On)
      {
        lnbCommand.Tone22kState = DeTone22kState.On;
      }
      lnbCommand.NumberOfMessages = 0;

      Marshal.StructureToPtr(lnbCommand, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, LNB_COMMAND_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LNB_COMMAND_SIZE,
        _generalBuffer, LNB_COMMAND_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Everywhere: result = success");
        return true;
      }

      this.LogError("Digital Everywhere: failed to set tone state, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("Digital Everywhere: open remote control interface");

      if (!_isDigitalEverywhere)
      {
        this.LogWarn("Digital Everywhere: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Digital Everywhere: conditional access interface is already open");
        return true;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.RemoteControlRegister, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Digital Everywhere: property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      _remoteControlBuffer = Marshal.AllocCoTaskMem(REMOTE_CONTROL_DATA_SIZE);
      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();

      this.LogDebug("Digital Everywhere: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Close()
    {
      return CloseRemoteControlListenerInterface(true);
    }

    private bool CloseRemoteControlListenerInterface(bool isDisposing)
    {
      this.LogDebug("Digital Everywhere: close remote control interface");

      if (isDisposing)
      {
        StopRemoteControlListenerThread();

        if (_isRemoteControlInterfaceOpen)
        {
          if (_propertySet != null)
          {
            int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.RemoteControlCancel, IntPtr.Zero, 0, IntPtr.Zero, 0);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogWarn("Digital Everywhere: failed to cancel remote control, hr = 0x{0:x}", hr);
            }
          }
          else
          {
            this.LogWarn("Digital Everywhere: remote control interface is open but property set is null");
          }
        }
      }

      if (_remoteControlBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Digital Everywhere: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~DigitalEverywhere()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isDigitalEverywhere)
      {
        CloseConditionalAccessInterface(isDisposing);
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (isDisposing)
      {
        _propertySet = null;
      }
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