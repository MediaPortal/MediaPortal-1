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
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC and PID filtering for
  /// Digital Everywhere tuners.
  /// </summary>
  public class DigitalEverywhere : IDiSEqCController, ICiMenuActions, IDisposable
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
      CiWriteTdpu,
      CiReadTdpu,
      MmiHostToCam,
      MmiCamToHost,
      Temperature,
      TuneQpsk,
      RemoteControlRegister,
      RemoteControlCancel,
      CiStatus,
      TestInterface
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
      EnterMenu
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

    private enum DeDiseqcPort : byte
    {
      Null = 0xff,
      PortA = 0,
      PortB,
      PortC,
      PortD
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

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DvbsMultiplexParams
    {
      public UInt32 Frequency;            // unit = kHz, range = 9750000 - 12750000
      public UInt32 SymbolRate;           // unit = ksps, range = 1000 - 40000
      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;
      public DeDiseqcPort Diseqc;
      private byte Padding;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DvbsServiceParams
    {
      public bool CurrentTransponder;
      public DeDiseqcPort Diseqc;
      public UInt32 Frequency;            // unit = kHz, range = 9750000 - 12750000
      public UInt32 SymbolRate;           // unit = ksps, range = 1000 - 40000
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

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DvbsPidFilterParams
    {
      public bool CurrentTransponder;
      public bool FullTransponder;
      public DeDiseqcPort Diseqc;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding1;
      public UInt32 Frequency;            // unit = kHz, range = 9750000 - 12750000
      public UInt32 SymbolRate;           // unit = ksps, range = 1000 - 40000
      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;
      private UInt16 Padding2;
      public byte NumberOfValidPids;
      private byte Padding3;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidFilterPids)]
      public UInt16[] FilterPids;
      private UInt16 Padding4;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
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

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DvbtPidFilterParams
    {
      public bool CurrentTransponder;
      public bool FullTransponder;
      public DvbtMultiplexParams MultiplexParams;
      public byte NumberOfValidPids;
      private byte Padding1;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidFilterPids)]
      public UInt16[] FilterPids;
      private UInt16 Padding2;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
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

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct FrontEndStatusInfo
    {
      public UInt32 Frequency;            // unit = kHz
      public UInt32 BitErrorRate;
      public byte SignalStrength;         // range = 0 - 100%
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      private byte[] Padding1;
      public bool IsLocked;

      public UInt16 CarrierToNoiseRatio;
      public byte AutomaticGainControl;
      private byte Value;                 // ???

      public byte FrontEndState;          // (DeFrontEndState)
      private byte Padding2;
      public UInt16 CiState;              // (DeCiState)

      public byte SupplyVoltage;
      public byte AntennaVoltage;
      public byte BusVoltage;
      private byte Padding3;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct SystemInfo
    {
      public byte NumberOfAntennas;       // range = 0 - 3
      public DeAntennaType AntennaType;
      public DeBroadcastSystem BroadcastSystem;
      public DeTransportType TransportType;
      public bool Lists;                  // ???
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DiseqcMessage
    {
      public byte MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct LnbCommand
    {
      public byte Voltage;
      public De22k Tone22k;
      public DeToneBurst ToneBurst;
      public byte NumberOfMessages;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageCount)]
      public DiseqcMessage[] DiseqcMessages;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct LnbParams
    {
      public byte AntennaNumber;
      public byte IsEast;
      public UInt16 OrbitalPosition;
      public UInt16 LowBandLof;           // unit = MHz
      public UInt16 SwitchFrequency;      // unit = MHz
      public UInt16 HighBandLof;          // unit = MHz
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct LnbParamInfo
    {
      public byte NumberOfAntennas;       // range = 0 - 3
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLnbParamCount)]
      public LnbParams[] LnbParams;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct QpskTuneParams
    {
      public UInt32 Frequency;            // unit = kHz, range = 950000 - 2150000
      public UInt16 SymbolRate;           // unit = ksps, range = 1000 - 40000
      public DeFecRate InnerFecRate;
      public DePolarisation Polarisation;
      public bool IsHighBand;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct CiErrorDebugMessage
    {
      public byte MessageType;
      public byte MessageLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxCiErrorDebugMessageLength)]
      public String Message;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct CaData
    {
      public byte Slot;
      public DeCiMessageTag Tag;
      private UInt16 Padding1;
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
    private const int LnbParamsSize = 42;
    private const int MaxLnbParamCount = 4;
    private const int CiErrorDebugMessageLength = 258;
    private const int MaxCiErrorDebugMessageLength = 256;
    private const int CaDataSize = 1036;
    private const int DriverVersionInfoSize = 64;
    private const int TemperatureInfoSize = 4;

    #endregion

    #region variables

    private bool _isDigitalEverywhere = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;

    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet;
    private CardType _tunerType = CardType.Unknown;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread = false;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="DigitalEverywhere"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    public DigitalEverywhere(IBaseFilter tunerFilter, CardType tunerType)
    {
      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.TestInterface,
        IntPtr.Zero, 0, IntPtr.Zero, 0
      );
      if (hr != 0)
      {
        return;
      }

      Log.Log.Debug("Digital Everywhere: supported tuner detected");
      _isDigitalEverywhere = true;
      _tunerType = tunerType;
      _generalBuffer = Marshal.AllocCoTaskMem(CaDataSize);

      _isCiSlotPresent = IsCiSlotPresent();
      if (_isCiSlotPresent)
      {
        _isCamPresent = IsCamPresent();
        if (_isCamPresent)
        {
          _isCamReady = IsCamReady();
          if (_isCamReady)
          {
            ReadApplicationInformation();
          }
        }
      }
      ReadDriverInfo();
      ReadHardwareInfo();
      ReadTemperature();
      ReadFrontEndStatus();

      SetPowerState(true);
      StartMmiHandlerThread();
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Digital Everywhere-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Digital Everywhere-compatible tuner, otherwise <c>false</c></value>
    public bool IsDigitalEverywhere
    {
      get
      {
        return _isDigitalEverywhere;
      }
    }

    /// <summary>
    /// Turn the LNB or aerial power supply on or off. 
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(bool powerOn)
    {
      Log.Log.Debug("Digital Everywhere: set power state, on = {0}", powerOn);

      // The FloppyDTV and FireDTV S and S2 should support this function. Apparently
      // the FireDTV T also supports active antennas but it is unclear whether and how
      // that power supply might be turned on or off.
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Digital Everywhere: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Digital Everywhere: property not supported");
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
        Log.Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      // This function needs to be tested. I'm uncertain whether
      // the driver will accept commands with no DiSEqC messages.
      Log.Log.Debug("Digital Everywhere: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

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
      DVB_MMI.DumpBinary(_generalBuffer, 0, LnbCommandSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LnbCommandSize,
        _generalBuffer, LnbCommandSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Digital Everywhere: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
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
      Log.Log.Debug("  modulation     = {0}", ch.ModulationType);

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

        if (ch.Rolloff == RollOff.Twenty)
        {
          rate += 16;
        }
        else if (ch.Rolloff == RollOff.TwentyFive)
        {
          rate += 32;
        }
        else if (ch.Rolloff == RollOff.ThirtyFive)
        {
          rate = 48;
        }
        ch.InnerFecRate = (BinaryConvolutionCodeRate)rate;
      }
      Log.Log.Debug("  inner FEC rate = {0}", ch.InnerFecRate);

      return ch as DVBBaseChannel;
    }

    /// <summary>
    /// Set the PIDs for PID filtering.
    /// </summary>
    /// <param name="modulation">The current multiplex/transponder modulation scheme.</param>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    public bool SetHardwareFilterPids(ModulationType modulation, List<ushort> pids)
    {
      Log.Log.Debug("Digital Everywhere: set PID filter PIDs, modulation = {0}", modulation);
      if (_tunerType != CardType.DvbS && _tunerType != CardType.DvbT && _tunerType != CardType.DvbC)
      {
        Log.Log.Debug("Digital Everywhere: PID filtering not supported");
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
      if (modulation != ModulationType.Mod8Psk || pids == null || pids.Count == 0)
      {
        Log.Log.Debug("Digital Everywhere: disabling PID filter");
      }
      else
      {
        Log.Log.Debug("Digital Everywhere: enabling PID filter");
        fullTransponder = false;
        for (int i = 0; i < pids.Count; i++)
        {
          if (i == MaxPidFilterPids)
          {
            Log.Log.Debug("Digital Everywhere: too many PIDs, hardware limit = {0}, actual count = {1}", MaxPidFilterPids, pids.Count);
            break;
          }
          Log.Log.Debug("  {0,-2} = 0x{1:x}", i + 1, pids[i]);
          filterPids[i] = pids[i];
          validPidCount++;
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

      DVB_MMI.DumpBinary(_generalBuffer, 0, bufferSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)property,
        _generalBuffer, bufferSize,
        _generalBuffer, bufferSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region hardware/software information

    /// <summary>
    /// Attempt to read the driver information from the tuner.
    /// </summary>
    private void ReadDriverInfo()
    {
      Log.Log.Debug("Digital Everywhere: read driver information");
      for (int i = 0; i < DriverVersionInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.FirmwareVersion,
        _generalBuffer, DriverVersionInfoSize,
        _generalBuffer, DriverVersionInfoSize,
        out returnedByteCount
      );
      if (hr != 0)
      {
        Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Log.Log.Debug("  driver version   = {0}", Marshal.PtrToStringAuto(_generalBuffer));
    }

    /// <summary>
    /// Attempt to read the hardware and firmware information from the tuner.
    /// </summary>
    private void ReadHardwareInfo()
    {
      Log.Log.Debug("Digital Everywhere: read hardware/firmware information");
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
        Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      byte[] b = { 0, 0, 0, 0, 0, 0, 0, 0 };
      Marshal.Copy(_generalBuffer, b, 0, 8);
      Log.Log.Debug("  hardware version = {0:x}.{1:x}.{2:x2}", b[0], b[1], b[2]);
      Log.Log.Debug("  firmware version = {0:x}.{1:x}.{2:x2}", b[3], b[4], b[5]);
      Log.Log.Debug("  firmware build # = {0}", (b[6] * 256) + b[7]);
    }

    /// <summary>
    /// Attempt to read the temperature from the tuner.
    /// </summary>
    private void ReadTemperature()
    {
      Log.Log.Debug("Digital Everywhere: read temperature");
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
        Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      // No idea what the output looks like at this stage...
      DVB_MMI.DumpBinary(_generalBuffer, 0, TemperatureInfoSize);
    }

    /// <summary>
    /// Attempt to read the front end status from the tuner.
    /// </summary>
    private void ReadFrontEndStatus()
    {
      Log.Log.Debug("Digital Everywhere: read front end status information");
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
        Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      // No idea whether this will work at this stage...
      DVB_MMI.DumpBinary(_generalBuffer, 0, FrontEndStatusInfoSize);
      //FrontEndStatusInfo status = (FrontEndStatusInfo)Marshal.PtrToStructure(_generalBuffer, typeof(FrontEndStatusInfo));
    }

    #endregion

    #region conditional access

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    public void ResetCi()
    {
      Log.Log.Debug("Digital Everywhere: reset CI");

      CaData data = new CaData(DeCiMessageTag.Reset);
      data.DataLength = 1;
      data.Data[0] = (byte)DeResetType.ForcedHardwareReset;

      Marshal.StructureToPtr(data, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, CaDataSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiHostToCam,
        _generalBuffer, CaDataSize,
        _generalBuffer, CaDataSize
      );
      if (hr == 0)
      {
        Log.Log.WriteFile("Digital Everywhere: result = success");
        return;
      }

      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
    }

    /// <summary>
    /// Reads the conditional access application information from the CAM.
    /// </summary>
    private void ReadApplicationInformation()
    {
      Log.Log.Debug("Digital Everywhere: request application information");
      CaData data = new CaData(DeCiMessageTag.ApplicationInfo);
      Marshal.StructureToPtr(data, _generalBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiCamToHost,
        _generalBuffer, CaDataSize,
        _generalBuffer, CaDataSize
      );
      if (hr != 0)
      {
        Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Log.Log.Debug("Digital Everywhere: read application information");
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
      if (hr != 0 || returnedByteCount == 0)
      {
        Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      data = (CaData)Marshal.PtrToStructure(_generalBuffer, typeof(CaData));
      Log.Log.Debug("  manufacturer = 0x{0:x}", BitConverter.ToInt16(data.Data, 0));
      Log.Log.Debug("  code         = 0x{0:x}", BitConverter.ToInt16(data.Data, 2));
      Log.Log.Debug("  menu title   = {0}", DVB_MMI.BytesToString(data.Data, 5, data.Data[4]));
    }

    /// <summary>
    /// Gets the conditional access interface status.
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
      for (int i = 0; i < bufferSize; i++)
      {
        Marshal.WriteByte(responsebuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.CiStatus,
        responsebuffer, bufferSize,
        responsebuffer, bufferSize,
        out returnedByteCount
      );
      if (hr == 0 && returnedByteCount == 2)
      {
        ciState = (DeCiState)Marshal.ReadInt16(responsebuffer, 0);
      }
      return hr;
    }

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("Digital Everywhere: is CI slot present");
      // As far as we're aware, all Digital Everywhere tuners have CI slots.
      Log.Log.Debug("Digital Everywhere: result = {0}", true);
      return true;
    }

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("Digital Everywhere: is CAM present");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Digital Everywhere: CI slot not present");
        return false;
      }

      DeCiState ciState;
      int hr = GetCiStatus(out ciState);
      if (hr != 0)
      {
        Log.Log.Debug("Digital Everywhere: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      Log.Log.Debug("Digital Everywhere: CI state = {0}", ciState.ToString());
      bool camPresent = false;
      if ((ciState & DeCiState.CamError) == 0 &&
        (ciState & DeCiState.CamIsDvb) != 0 &&
        (ciState & DeCiState.CamPresent) != 0)
      {
        camPresent = true;
      }
      Log.Log.Debug("Digital Everywhere: result = {0}", camPresent);
      return camPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("Digital Everywhere: is CAM ready");
      if (!_isCamPresent)
      {
        Log.Log.Debug("Digital Everywhere: CAM not present");
        return false;
      }

      DeCiState ciState;
      int hr = GetCiStatus(out ciState);
      if (hr != 0)
      {
        Log.Log.Debug("Digital Everywhere: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      Log.Log.Debug("Digital Everywhere: CI state = {0}", ciState.ToString());
      bool camReady = false;
      if ((ciState & DeCiState.CamPresent) != 0 &&
        (ciState & DeCiState.CamIsDvb) != 0 &&
        (ciState & DeCiState.CamError) == 0 &&
        (ciState & DeCiState.CamReady) != 0 &&
        (ciState & DeCiState.ApplicationInfoAvailable) != 0)
      {
        camReady = true;
      }
      Log.Log.Debug("Digital Everywhere: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the service is successfully descrambled, otherwise <c>false</c></returns>
    public bool SendPmt(CaPmtListManagementAction listAction, CaPmtCommand command, byte[] pmt, int length)
    {
      Log.Log.Debug("Digital Everywhere: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("Digital Everywhere: CAM not available");
        return true;
      }
      if (length > MaxPmtLength - 2)
      {
        Log.Log.Debug("Digital Everywhere: buffer capacity too small");
        return false;
      }

      // TODO: get application information here? (ie. force wait until CAM is ready for up to 10 seconds)

      CaData data = new CaData(DeCiMessageTag.Pmt);
      data.DataLength = (ushort)(length + 2);
      data.Data[0] = (byte)listAction;
      data.Data[1] = (byte)command;
      int j = 2;
      for (int i = 0; i < length; i++)
      {
        data.Data[j] = pmt[i];
        j++;
      }

      IntPtr buffer = Marshal.AllocCoTaskMem(CaDataSize);
      Marshal.StructureToPtr(data, buffer, true);
      DVB_MMI.DumpBinary(buffer, 0, CaDataSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiHostToCam,
        buffer, CaDataSize,
        buffer, CaDataSize
      );
      if (hr == 0)
      {
        Log.Log.WriteFile("Digital Everywhere: result = success", hr);
        return true;
      }
      // Failure indicates a Firewire communication problem.
      // Success does *not* indicate that the service will be descrambled.
      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));

      // TODO: reset CAM here?
      return false;
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Log.Debug("Digital Everywhere: starting new MMI handler thread");
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
      Log.Log.Debug("Digital Everywhere: MMI handler thread start polling");
      DVB_MMI_Handler handler = new DVB_MMI_Handler("Digital Everywhere", ref _ciMenuCallbacks);
      DeCiState ciState = DeCiState.Empty;
      DeCiState prevCiState = DeCiState.Empty;

      try
      {
        while (!_stopMmiHandlerThread)
        {
          Thread.Sleep(500);

          int hr = GetCiStatus(out ciState);
          if (hr != 0)
          {
            Log.Log.Debug("Digital Everywhere: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            Log.Log.Debug("Digital Everywhere: CI state change");
            Log.Log.Debug("  old state = {0}", prevCiState.ToString());
            Log.Log.Debug("  new state = {0}", ciState.ToString());
            prevCiState = ciState;

            if ((ciState & DeCiState.CamError) == 0 &&
              (ciState & DeCiState.CamIsDvb) != 0 &&
              (ciState & DeCiState.CamPresent) != 0)
            {
              _isCamPresent = true;
              if ((ciState & DeCiState.CamReady) != 0 &&
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
          if (_isCamReady)
          {
            continue;
          }

          // Check for MMI responses and requests.
          if ((ciState & DeCiState.MmiRequest) != 0)
          {
            Log.Log.Debug("Digital Everywhere: MMI object available, requesting object");
            CaData data = new CaData(DeCiMessageTag.Mmi);
            Marshal.StructureToPtr(data, _mmiBuffer, true);
            hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiCamToHost,
              _mmiBuffer, CaDataSize,
              _mmiBuffer, CaDataSize
            );
            if (hr != 0)
            {
              Log.Log.Debug("Digital Everywhere: failed to request object, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              continue;
            }

            Log.Log.Debug("Digital Everywhere: retrieving object");
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
              Log.Log.Debug("Digital Everywhere: failed to retrieve object, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              continue;
            }

            Log.Log.Debug("Digital Everywhere: handling object");
            data = (CaData)Marshal.PtrToStructure(_mmiBuffer, typeof(CaData));
            handler.HandleMMI(data.Data, data.DataLength);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Digital Everywhere: error in MMI handler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    private bool SendMmi(CaData data)
    {
      Marshal.StructureToPtr(data, _mmiBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.MmiHostToCam,
        _mmiBuffer, CaDataSize,
        _mmiBuffer, CaDataSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Sets the CAM callback handler functions.
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
    /// Sends a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Digital Everywhere: enter menu");

      CaData data = new CaData(DeCiMessageTag.EnterMenu);
      return SendMmi(data);
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Digital Everywhere: close menu");

      CaData data = new CaData(DeCiMessageTag.Mmi);
      byte[] apdu = DVB_MMI.CreateMMIClose();
      data.DataLength = (UInt16)apdu.Length;
      for (int i = 0; i < apdu.Length; i++)
      {
        data.Data[i] = apdu[i];
      }
      return SendMmi(data);
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Digital Everywhere: select menu entry, choice = {0}", choice);

      CaData data = new CaData(DeCiMessageTag.Mmi);
      byte[] apdu = DVB_MMI.CreateMMISelect(choice);
      data.DataLength = (UInt16)apdu.Length;
      for (int i = 0; i < apdu.Length; i++)
      {
        data.Data[i] = apdu[i];
      }
      return SendMmi(data);
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("Digital Everywhere: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      CaData data = new CaData(DeCiMessageTag.Mmi);
      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }
      byte[] apdu = DVB_MMI.CreateMMIAnswer(responseType, answer);
      data.DataLength = (UInt16)apdu.Length;
      for (int i = 0; i < apdu.Length; i++)
      {
        data.Data[i] = apdu[i];
      }
      return SendMmi(data);
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
      bool successDiseqc = true;
      if (channel.DisEqc == DisEqcType.SimpleA)
      {
        toneBurst = ToneBurst.ToneBurst;
      }
      else if (channel.DisEqc == DisEqcType.SimpleB)
      {
        toneBurst = ToneBurst.DataBurst;
      }
      else if (channel.DisEqc != DisEqcType.None)
      {
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        successDiseqc = SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }

      Tone22k tone22k = Tone22k.Off;
      if (isHighBand)
      {
        tone22k = Tone22k.On;
      }
      bool successTone = SetToneState(toneBurst, tone22k);

      return (successDiseqc && successTone);
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Digital Everywhere: send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Digital Everywhere: command too long, length = {0}", command.Length);
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
      for (int i = 0; i < command.Length; i++)
      {
        lnbCommand.DiseqcMessages[0].Message[i] = command[i];
      }

      Marshal.StructureToPtr(lnbCommand, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, LnbCommandSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbControl,
        _generalBuffer, LnbCommandSize,
        _generalBuffer, LnbCommandSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Digital Everywhere: result = success");
        return true;
      }

      Log.Log.Debug("Digital Everywhere: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command. This function is untested.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      // Not implemented.
      reply = null;
      return false;
    }

    #endregion

    #region IDisposable Member

    /// <summary>
    /// Disposes DE class and free up memory
    /// </summary>
    public void Dispose()
    {
      if (!_isDigitalEverywhere)
      {
        return;
      }
      if (_mmiHandlerThread != null)
      {
        _stopMmiHandlerThread = true;
        Thread.Sleep(3000);
      }
      SetPowerState(false);
      Marshal.FreeCoTaskMem(_generalBuffer);
      Marshal.FreeCoTaskMem(_mmiBuffer);
    }

    #endregion
  }
}