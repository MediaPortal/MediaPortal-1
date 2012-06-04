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
using DirectShowLib.BDA;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;
using TvDatabase;
using TvLibrary.Interfaces.Device;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles the SkyStar 2 DVB-S card
  /// </summary>
  public class TvCardDvbSS2 : TvCardDvbBase, IDisposable, ITVCard, IDiseqcController
  {
    #region enums

    /*private enum SkyStarError
    {
      // For AddPIDsToPin() or AddPIDs()...
      AlreadyExists = 0x10011000,         // PID already registered.
      PidError = (int)0x90011001,
      AlreadyFull,                        // Max PID count exceeded.

      // General...
      CreateInterface = (int)0x90020001,  // Not all interfaces could be created correctly.
      UnsupportedDevice,                  // The given device is not B2C2-compatible (Linux).
      NotInitialised,                     // Initialize() needs to be called.

      // (B2C2MPEG2AdapterWin.cpp code...)
      InvalidPin,
      NoTsFilter,
      PinAlreadyConnected,
      NoInputPin,

      InvalidTid,                         // Invalid table ID passed to SetTableId().

      // Decrypt keys...
      SetGlobalFixedKey,
      SetPidFixedKey,
      GetPidFixedKeys,
      DeletePidFixedKey,
      PurgeFixedKey,

      DiseqcInProgress,
      Diseqc12NotSupported,
      NoDeviceAvailable
    }*/

    private enum SkyStarTunerType
    {
      Unknown = -1,
      Satellite,
      Cable,
      Terrestrial,
      Atsc
    }

    private enum SkyStarBusType
    {
      Pci = 0,
      Usb1 = 1    // USB 1.1
    }

    [Flags]
    private enum SkyStarPerformanceMonitoringCapability
    {
      None = 0,
      BitErrorRate = 1,           // BER reporting via GetPreErrorCorrectionBER().
      BlockCount = 2,             // Block count reporting via GetTotalBlocks().
      CorrectedBlockCount = 4,    // Corrected block count reporting via GetCorrectedBlocks().
      UncorrectedBlockCount = 8,  // Uncorrected block count reporting via GetUncorrectedBlocks().
      SignalToNoiseRatio = 16,    // SNR reporting via GetSNR().
      SignalStrength = 32,        // Signal strength percentage reporting via GetSignalStrength().
      SignalQuality = 64          // Signal quality percentage reporting via GetSignalQuality().
    }

    [Flags]
    private enum SkyStarAcquisionCapability
    {
      None = 0,
      AutoSymbolRate = 1,       // DVB-S automatic symbol rate detection support.
      AutoGuardInterval = 2,    // DVB-T automatic guard interval detection support.
      AutoFrequencyOffset = 4,  // DVB-T automatic frequency offset handling support.
      VhfSupport = 8,           // DVB-T VHF (30 - 300 MHz) support.
      Bandwidth6 = 16,          // DVB-T 6 MHz transponder support.
      Bandwidth7 = 32,          // DVB-T 7 MHz transponder support.
      Bandwidth8 = 64,          // DVB-T 8 MHz transponder support.
      RawDiseqc = 128,          // DVB-S DiSEqC 1.2 16 port switch and motor control support.
      IrInput = 256             // IR remote input support.
    }

    private enum SkyStarModulation
    {
      Unknown = -1,
      Qam4 = 2,
      Qam16,
      Qam32,
      Qam64,
      Qam128,
      Qam256,
      Qam64AnnexB,
      Qam256AnnexB,
      Vsb8,
      Vsb16
    }

    private enum SkyStarPolarisation
    {
      Horizontal = 0,   // 18 V - also use for circular left.
      Vertical,         // 13 V - also use for circular right.
      PowerOff = 10     // Turn the power supply to the LNB completely off.
    }

    private enum SkyStarFecRate
    {
      Rate1_2 = 1,
      Rate2_3,
      Rate3_4,
      Rate5_6,
      Rate7_8,
      Auto
    }

    private enum SkyStarTone
    {
      Off = 0,
      Tone22k,
      Tone33k,
      Tone44k
    }

    private enum SkyStarDiseqcPort
    {
      None = 0,
      // Simple DiSEqC (tone burst).
      SimpleA,
      SimpleB,
      // DiSEqC 1.0 (option + position).
      PortA,
      PortB,
      PortC,
      PortD
    }

    private enum SkyStarGuardInterval
    {
      Guard1_32 = 0,
      Guard1_16,
      Guard1_8,
      Guard1_4,
      Auto
    }

    private enum SkyStarPidFilterMode
    {
      AllIncludingNull = 0x2000,
      AllExcludingNull = 0x2001
    }

    private enum SkyStarTableId
    {
      Dvb = 0x3E,
      Atsc = 0x3F,
      Auto = 0xFF
    }

    private enum SkyStarKeyType
    {
      // PID TSC Keys. Up to 7 Keys possible; highest priority used.
      PidTscReserved = 1,   // Key is used if the PID matches and the scrambling control bits are '01' (reserved).
      PidTscEven = 2,       // Key is used if the PID matches and the scrambling control bits are '10' (even).
      PidTscOdd = 3,        // Key is used if the PID matches and the scrambling control bits are '11' (odd).
      // PID-only Key. 1 key only; used if no global key is set.
      PidTscAny,            // Key is used if none of the scrambling patterns matches and the PID value matches.
      // Global Key. 1 key only; used if no PID-only key is set.
      Global,               // Key is used if no other key matches.
    }

    private enum SkyStarPvrOption
    {
      AutoDeleteRecordFile = 0,
      AutoRecordWithPlay
    }

    private enum SkyStarPvrCallbackState
    {
      EndOfFile = 0,
      FileError       // Indicates file access or disk space issues.
    }

    private enum SkyStarVideoAspectRatio : byte
    {
      Invalid = 0,
      Square,
      Standard4x3,
      Wide16x9,
      Other
    }

    private enum SkyStarVideoFrameRate : byte
    {
      Forbidden = 0,
      Rate23_97,
      Rate24,
      Rate25,
      Rate29_97,
      Rate30,
      Rate50,
      Rate59_94,
      Rate60
    }

    #endregion

    #region COM interfaces

    #region tuner control

    [ComVisible(true), ComImport,
      Guid("d875d4a9-0749-4fe8-adb9-cc13f9b3dd45"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TunerCtrl
    {
      /// <summary>
      /// Initialise the tuner control interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the interface was successfully initialised</returns>
      [PreserveSig]
      Int32 Initialize();

      /// <summary>
      /// Get the tuner capabilities.
      /// </summary>
      /// <param name="capabilities">A pointer to a tuner capabilities structure.</param>
      /// <param name="size">The size of the capabilities structure.</param>
      /// <returns>an HRESULT indicating whether the capabilities were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetTunerCapabilities(IntPtr capabilities, [Out] out Int32 size);

      /// <summary>
      /// Check the lock status of the tuner.
      /// </summary>
      /// <returns>an HRESULT indicating whether the tuner is locked or not</returns>
      [PreserveSig]
      Int32 CheckLock();

      /// <summary>
      /// Apply previously set tuning parameter values.
      /// </summary>
      /// <returns>an HRESULT indicating whether the tuner is locked on signal</returns>
      [PreserveSig]
      Int32 SetTunerStatus();

      #region get/set tuning parameters

      /// <summary>
      /// Get the tuned channel number. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="channel">The channel number.</param>
      /// <returns>an HRESULT indicating whether the channel number was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetChannel([Out] out Int32 channel);

      /// <summary>
      /// Set the tuner channel number. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="channel">The channel number.</param>
      /// <returns>an HRESULT indicating whether the channel number was successfully set</returns>
      [PreserveSig]
      Int32 SetChannel(Int32 channel);

      /// <summary>
      /// Get the tuned multiplex frequency. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="frequency">The multiplex frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the multiplex frequency was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFrequency([Out] out Int32 frequency);

      /// <summary>
      /// Set the multiplex frequency.
      /// </summary>
      /// <param name="frequency">The frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the frequency was successfully set</returns>
      [PreserveSig]
      Int32 SetFrequency(Int32 frequency);

      /// <summary>
      /// Get the modulation scheme for the tuned multiplex. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="modulation">The multiplex modulation scheme.</param>
      /// <returns>an HRESULT indicating whether the multiplex modulation scheme was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetModulation([Out] out SkyStarModulation modulation);

      /// <summary>
      /// Set the multiplex modulation scheme. Only applicable for DVB-C tuners.
      /// </summary>
      /// <param name="modulation">The modulation scheme.</param>
      /// <returns>an HRESULT indicating whether the modulation scheme was successfully set</returns>
      [PreserveSig]
      Int32 SetModulation(SkyStarModulation modulation);

      /// <summary>
      /// Get the tuned multiplex symbol rate in ks/s. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="symbolRate">The multiplex symbol rate in ks/s.</param>
      /// <returns>an HRESULT indicating whether the multiplex symbol rate was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSymbolRate([Out] out Int32 symbolRate);

      /// <summary>
      /// Set the multiplex symbol rate. Only applicable for DVB-S and DVB-C tuners.
      /// </summary>
      /// <param name="symbolRate">The symbol rate in ks/s.</param>
      /// <returns>an HRESULT indicating whether the symbol rate was successfully set</returns>
      [PreserveSig]
      Int32 SetSymbolRate(Int32 symbolRate);

      /// <summary>
      /// Set the multiplex polarisation. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="polarisation">The polarisation.</param>
      /// <returns>an HRESULT indicating whether the polarisation was successfully set</returns>
      [PreserveSig]
      Int32 SetPolarity(SkyStarPolarisation polarisation);

      /// <summary>
      /// Set the multiplex FEC rate. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="fecRate">The FEC rate.</param>
      /// <returns>an HRESULT indicating whether the FEC rate was successfully set</returns>
      [PreserveSig]
      Int32 SetFec(SkyStarFecRate fecRate);

      /// <summary>
      /// Set the LNB local oscillator frequency. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="lnbFrequency">The local oscillator frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the local oscillator frequency was successfully set</returns>
      [PreserveSig]
      Int32 SetLnbFrequency(Int32 lnbFrequency);

      /// <summary>
      /// Set the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="toneState">The tone state.</param>
      /// <returns>an HRESULT indicating whether the tone state was successfully set</returns>
      [PreserveSig]
      Int32 SetLnbKHz(SkyStarTone toneState);

      /// <summary>
      /// Switch to a specific satellite. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
      /// <returns>an HRESULT indicating whether the DiSEqC command was successfully sent</returns>
      [PreserveSig]
      Int32 SetDiseqc(SkyStarDiseqcPort diseqcPort);

      #endregion

      #region signal strength/quality metrics

      /// <summary>
      /// Obsolete. Use GetSignalStrength() or GetSignalQuality() instead.
      /// </summary>
      /// <param name="signalLevel">The signal level in dBm.</param>
      /// <returns>an HRESULT indicating whether the signal level was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSignalLevel([Out] out float signalLevel);

      /// <summary>
      /// Get the tuner/demodulator signal quality statistic.
      /// </summary>
      /// <param name="signalQuality">The signal quality as a percentage.</param>
      /// <returns>an HRESULT indicating whether the signal quality was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSignalQuality([Out] out Int32 signalQuality);

      /// <summary>
      /// Get the tuner/demodulator signal strength statistic.
      /// </summary>
      /// <param name="signalStrength">The signal strength as a percentage.</param>
      /// <returns>an HRESULT indicating whether the signal strength was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSignalStrength([Out] out Int32 signalStrength);

      /// <summary>
      /// Get the tuner/demodulator signal to noise ratio (SNR) statistic. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="snr">The signal to noise ratio.</param>
      /// <returns>an HRESULT indicating whether the signal to noise ratio was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSNR([Out] out float snr);

      /// <summary>
      /// Get the pre-error-correction bit error rate (BER) statistic. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="ber">The bit error rate.</param>
      /// <param name="wait">(Not used.)</param>
      /// <returns>an HRESULT indicating whether the bit error rate was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetPreErrorCorrectionBER([Out] out float ber, bool wait);

      /// <summary>
      /// Get the total block count since the last call to this function. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetTotalBlocks([Out] out Int32 blockCount);

      /// <summary>
      /// Get the uncorrected block count since the last call to this function. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetUncorrectedBlocks([Out] out Int32 blockCount);

      #endregion
    }

    [ComVisible(true), ComImport,
      Guid("cd900832-50df-4f8f-882d-1c358f90b3f2"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TunerCtrl2 : IB2C2MPEG2TunerCtrl
    {
      /// <summary>
      /// Apply previously set tuning parameter values.
      /// </summary>
      /// <param name="lockCheckCount">The number of times the interface should check for lock after applying
      ///   the tuning parameter values. The delay between checks is 50 ms.</param>
      /// <returns>an HRESULT indicating whether the tuner is locked on signal</returns>
      [PreserveSig]
      Int32 SetTunerStatusEx(Int32 lockCheckCount);

      #region get/set tuning parameters

      /// <summary>
      /// Set the multiplex frequency.
      /// </summary>
      /// <param name="frequency">The frequency in kHz.</param>
      /// <returns>an HRESULT indicating whether the frequency was successfully set</returns>
      [PreserveSig]
      Int32 SetFrequencyKHz(Int32 frequency);

      /// <summary>
      /// Get the multiplex guard interval. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="interval">The guard interval.</param>
      /// <returns>an HRESULT indicating whether the guard interval was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetGuardInterval([Out] out SkyStarGuardInterval interval);

      /// <summary>
      /// Set the multiplex guard interval. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="interval">The guard interval.</param>
      /// <returns>an HRESULT indicating whether the guard interval was successfully set</returns>
      [PreserveSig]
      Int32 SetGuardInterval(SkyStarGuardInterval interval);

      /// <summary>
      /// Get the multiplex polarisation. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="polarisation">The polarisation.</param>
      /// <returns>an HRESULT indicating whether the polarisation was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetPolarity([Out] out SkyStarPolarisation polarisation);

      /// <summary>
      /// Get the multiplex FEC rate. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="fecRate">The FEC rate.</param>
      /// <returns>an HRESULT indicating whether the FEC rate was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFec([Out] out SkyStarFecRate fecRate);

      /// <summary>
      /// Get the LNB local oscillator frequency. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="lnbFrequency">The local oscillator frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the local oscillator frequency was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetLnbFrequency([Out] out Int32 lnbFrequency);

      /// <summary>
      /// Get the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="toneState">The tone state.</param>
      /// <returns>an HRESULT indicating whether the tone state was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetLnbKHz([Out] out SkyStarTone toneState);

      /// <summary>
      /// Get the currenly selected satellite. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
      /// <returns>an HRESULT indicating whether the setting was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetDiseqc([Out] out SkyStarDiseqcPort diseqcPort);

      #endregion

      /// <summary>
      /// Get the corrected block count since the last call to this function.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetCorrectedBlocks([Out] out Int32 blockCount);
    }

    [ComVisible(true), ComImport,
      Guid("4b39eb78-d3cd-4223-b682-46ae66968118"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TunerCtrl3 : IB2C2MPEG2TunerCtrl2
    {
      /// <summary>
      /// Get the multiplex bandwidth. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="bandwidth">The bandwidth in MHz, expected to be 6, 7 or 8.</param>
      /// <returns>an HRESULT indicating whether the bandwidth was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetBandwidth([Out] out Int32 bandwidth);

      /// <summary>
      /// Set the multiplex bandwidth. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="bandwidth">The bandwidth in MHz, expected to be 6, 7 or 8.</param>
      /// <returns>an HRESULT indicating whether the bandwidth was successfully set</returns>
      [PreserveSig]
      Int32 SetBandwidth(Int32 bandwidth);
    }

    [ComVisible(true), ComImport,
      Guid("61a9051f-04c4-435e-8742-9edd2c543ce9"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TunerCtrl4 : IB2C2MPEG2TunerCtrl3
    {
      /// <summary>
      /// Send a raw DiSEqC command.
      /// </summary>
      /// <param name="length">The length of the command in bytes.</param>
      /// <param name="command">The command.</param>
      /// <returns>an HRESULT indicating whether the command was successfully sent</returns>
      Int32 SendDiSEqCCommand(Int32 length, IntPtr command);
    }

    #endregion

    #region data control

    [ComVisible(true), ComImport,
      Guid("7f35c560-08b9-11d5-a469-00d0d7b2c2d7"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl
    {
      #region all PIDs

      /// <summary>
      /// Get the list of PIDs of all classes that are currently registered with the interface.
      /// </summary>
      /// <param name="pidCount">The number of PIDs registered.</param>
      /// <param name="pids">The registered PIDs.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetGlobalPIDs([Out] out Int32 pidCount,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out Int32[] pids
      );

      /// <summary>
      /// Get the maximum number of PIDs of any class that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMaxGlobalPIDCount([Out] out Int32 maxPidCount);

      /// <summary>
      /// Deregister all PIDs currently registered with the interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      Int32 PurgeGlobalPIDs();

      #endregion

      #region IP PIDs

      /// <summary>
      /// Register IP class PID(s) that are of interest to the application. Packets marked with these PIDs
      /// will be passed on the B2C2 filter's first data output pin.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to register.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      Int32 AddIpPIDs(Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids);

      /// <summary>
      /// Deregister IP class PID(s) that are no longer of interest to the application. Packets marked with
      /// these PIDs will no longer be passed on the B2C2 filter's first data output pin.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      Int32 DeleteIpPIDs(Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids);

      /// <summary>
      /// Get the list of IP class PIDs that are currently registered with the interface.
      /// </summary>
      /// <param name="pidCount">The number of PIDs registered.</param>
      /// <param name="pids">The registered PIDs.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetIpPIDs([Out] out Int32 pidCount,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out Int32[] pids
      );

      /// <summary>
      /// Get the maximum number of IP class PIDs that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMaxIpPIDCount([Out] out Int32 maxPidCount);

      #endregion

      #region transport stream PIDs

      /// <summary>
      /// Obsolete. Use AddPIDsToPin() or AddTsPIDs() instead.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to register.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      Int32 AddPIDs(Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids);

      /// <summary>
      /// Obsolete. Use DeletePIDsFromPin() or DeleteTsPIDs() instead.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      Int32 DeletePIDs(Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids);

      /// <summary>
      /// Get the maximum number of transport stream class PIDs that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMaxPIDCount([Out] out Int32 maxPidCount);

      #endregion

      /// <summary>
      /// Get the current values of statistics that can be used for monitoring signal quality. The statistics
      /// are measured since the last call to this function or to ResetDataReceptionStats().
      /// </summary>
      /// <param name="ipRatio">The ratio of correctly received IP class packets to total IP packets.</param>
      /// <param name="tsRatio">The ratio of correctly received TS class packets to total TS packets.</param>
      /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetDataReceptionStats([Out] out Int32 ipRatio, [Out] out Int32 tsRatio);

      /// <summary>
      /// Reset the values of the statistics retrieved by GetDataReceptionStats().
      /// </summary>
      /// <returns>an HRESULT indicating whether the statistics were successfully reset</returns>
      [PreserveSig]
      Int32 ResetDataReceptionStats();
    }

    [ComVisible(true), ComImport,
      Guid("b0666b7c-8c7d-4c20-bb9b-4a7fe0f313a8"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl2 : IB2C2MPEG2DataCtrl
    {
      /// <summary>
      /// Register transport stream class PID(s) that are of interest to the application. Packets marked
      /// with these PIDs will be passed on the corrresponding B2C2 filter data output pin.
      /// </summary>
      /// <param name="pidCount">As an input, the number of PIDs to attempt to register; as an output, the
      ///   number of PIDs that were successfully registered.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <param name="pidIndex">The index (zero-based) of the data output pin to register with.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      Int32 AddPIDsToPin([In, Out] ref Int32 pidCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids, Int32 pidIndex);

      /// <summary>
      /// Deregister transport stream class PID(s) that are no longer of interest to the application.
      /// Packets marked with these PIDs will no longer be passed on the corrresponding B2C2 filter data
      /// output pin.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <param name="pidIndex">The index (zero-based) of the data output pin to deregister with.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      Int32 DeletePIDsFromPin(Int32 pidCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids, Int32 pidIndex);
    }

    [ComVisible(true), ComImport,
      Guid("e2857b5b-84e7-48b7-b842-4ef5e175f315"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl3 : IB2C2MPEG2DataCtrl2
    {
      #region IP PIDs

      /// <summary>
      /// Get the details of IP class PIDs that are registered with the interface.
      /// </summary>
      /// <param name="openPidCount">The number of registered PIDs.</param>
      /// <param name="runningPidCount">The number of PIDs that are currently running.</param>
      /// <param name="pidCount">As an input, the number of PIDs to retrieve; as an output, the number of
      ///   PIDs actually retrieved.</param>
      /// <param name="pidList">The list of registered PIDs.</param>
      /// <returns>an HRESULT indicating whether the state information was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetIpState([Out] out Int32 openPidCount, [Out] out Int32 runningPidCount,
              [In, Out] ref Int32 pidCount,
              [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] out Int32[] pidList
      );

      /// <summary>
      /// Get the number of IP class PID bytes and packets that have been received.
      /// </summary>
      /// <param name="byteCount">The number of bytes received.</param>
      /// <param name="packetCount">The number of packets received.</param>
      /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetReceivedDataIp(Int64 byteCount, Int64 packetCount);

      #endregion

      #region transport stream PIDs

      /// <summary>
      /// Register transport stream class PID(s) that are of interest to the application. Packets marked
      /// with these PIDs will be passed on the first data output pin of the B2C2 filter.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to register.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      Int32 AddTsPIDs(Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids);

      /// <summary>
      /// Deregister transport stream class PID(s) when they are no longer of interest to the application.
      /// Packets marked with these PIDs will no longer be passed on the first data output pin of the B2C2
      /// filter.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      Int32 DeleteTsPIDs(Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Int32[] pids);

      /// <summary>
      /// Get the details of transport stream class PIDs that are registered with the interface.
      /// </summary>
      /// <param name="openPidCount">The number of registered PIDs.</param>
      /// <param name="runningPidCount">The number of PIDs that are currently running.</param>
      /// <param name="pidCount">As an input, the number of PIDs to retrieve; as an output, the number of
      ///   PIDs actually retrieved.</param>
      /// <param name="pidList">The list of registered PIDs.</param>
      /// <returns>an HRESULT indicating whether the state information was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetTsState([Out] out Int32 openPidCount, [Out] out Int32 runningPidCount,
              [In, Out] ref Int32 pidCount, [Out] out Int32[] pidList
      );

      #endregion

      #region multicast

      /// <summary>
      /// Register the given multicast MAC addresses with the interface.
      /// </summary>
      /// <remarks>
      /// The maximum number of addresses that may be registered can be retrieved from MaxMacAddressCount.
      /// </remarks>
      /// <param name="addressList">The list of addresses to register.</param>
      /// <returns>an HRESULT indicating whether the MAC addresses were successfully registered</returns>
      [PreserveSig]
      Int32 AddMulticastMacAddress(MacAddressList addressList);

      /// <summary>
      /// Deregister the given multicast MAC addresses from the interface.
      /// </summary>
      /// <remarks>
      /// The maximum number of addresses that may be deregistered is set at MaxMacAddressCount.
      /// </remarks>
      /// <param name="addressList">The list of addresses to deregister.</param>
      /// <returns>an HRESULT indicating whether the MAC addresses were successfully deregistered</returns>
      [PreserveSig]
      Int32 DeleteMulticastMacAddress(MacAddressList addressList);

      /// <summary>
      /// Get the list of multicast MAC addresses that are registered with the interface.
      /// </summary>
      /// <param name="addressList">The list of addresses.</param>
      /// <returns>an HRESULT indicating whether the address list was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMulticastMacAddressList([Out] out MacAddressList addressList);

      #endregion

      #region unicast

      /// <summary>
      /// Get the device's current unicast MAC address.
      /// </summary>
      /// <param name="address">The current address.</param>
      /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetUnicastMacAddress([Out] out MacAddress address);

      /// <summary>
      /// Set the device's unicast MAC address.
      /// </summary>
      /// <param name="address">The address to set.</param>
      /// <returns>an HRESULT indicating whether the address was successfully set</returns>
      [PreserveSig]
      Int32 SetUnicastMacAddress(MacAddress address);

      /// <summary>
      /// Restore the unicast MAC address to the default address for the device.
      /// </summary>
      /// <returns>an HRESULT indicating whether the address was successfully restored</returns>
      [PreserveSig]
      Int32 RestoreUnicastMacAddress();

      #endregion
    }

    [ComVisible(true), ComImport,
      Guid("5927db1c-b2ac-441f-89a7-c61194d15392"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl4 : IB2C2MPEG2DataCtrl3
    {
      /// <summary>
      /// Get the device's MAC address.
      /// </summary>
      /// <param name="macAddress">The MAC address.</param>
      /// <returns>an HRESULT indicating whether the MAC address was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetHardwareMacAddress(MacAddress macAddress);

      [PreserveSig]
      Int32 SetTableId(Int32 tableId);

      [PreserveSig]
      Int32 GetTableId([Out] out Int32 tableId);

      #region decrypt keys

      /// <summary>
      /// Register a decryption key with the interface.
      /// </summary>
      /// <param name="keyType">The key type.</param>
      /// <param name="pid">The PID to use the key to decrypt.</param>
      /// <param name="key">The key.</param>
      /// <param name="keyLength">The length of the key.</param>
      /// <returns>an HRESULT indicating whether the key was successfully registered</returns>
      [PreserveSig]
      Int32 AddKey(SkyStarKeyType keyType, UInt32 pid, IntPtr key, Int32 keyLength);

      /// <summary>
      /// Deregister a decryption key.
      /// </summary>
      /// <param name="keyType">The key type.</param>
      /// <param name="pid">The PID that the key is associated with.</param>
      /// <returns>an HRESULT indicating whether the key was successfully deregistered</returns>
      [PreserveSig]
      Int32 DeleteKey(SkyStarKeyType keyType, UInt32 pid);

      /// <summary>
      /// Get the details for the keys that are registered with and being used by the interface.
      /// </summary>
      /// <param name="keyCount">The number of keys in use.</param>
      /// <param name="keyTypes">A pointer to an array listing the type of each key.</param>
      /// <param name="pids">A pointer to an array listing the PID associated with each key.</param>
      /// <returns>an HRESULT indicating whether the key details were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetKeysInUse([Out] out Int32 keyCount, [Out] out IntPtr keyTypes, [Out] out IntPtr pids);

      /// <summary>
      /// Get counts of each of the types of keys that are registered with the interface.
      /// </summary>
      /// <param name="totalKeyCount">The total number of registered keys.</param>
      /// <param name="pidTscKeyCount">The number of PID-specific keys registered.</param>
      /// <param name="pidKeyCount">The number of fallback PID keys registered.</param>
      /// <param name="globalKeyCount">The number of global keys registered.</param>
      /// <returns>an HRESULT indicating whether the key counts were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetKeyCount([Out] out Int32 totalKeyCount, [Out] out Int32 pidTscKeyCount, [Out] out Int32 pidKeyCount, [Out] out Int32 globalKeyCount);

      /// <summary>
      /// Deregister all decryption keys.
      /// </summary>
      /// <returns>an HRESULT indicating whether the keys were successfully deregistered</returns>
      [PreserveSig]
      Int32 PurgeKeys();

      #endregion
    }

    [ComVisible(true), ComImport,
      Guid("b5afa7f3-2fbc-4d66-ad9c-ff8616141c26"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl5 : IB2C2MPEG2DataCtrl4
    {
      /// <summary>
      /// Register a callback delegate that the interface can use to pass raw transport stream packets
      /// directly to the application. The packets passed correspond with the transport stream class PIDs
      /// registered with the interface.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      Int32 SetCallbackForTransportStream(OnSkyStarTsData callback);
    }

    [ComVisible(true), ComImport,
      Guid("a12a4531-72d2-40fc-b17d-8f9b0004444f"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl6 : IB2C2MPEG2DataCtrl5
    {
      /// <summary>
      /// Get information about the B2C2 compatible devices installed in the system.
      /// </summary>
      /// <param name="deviceInfo">A pointer to an array of DeviceInfo instances.</param>
      /// <param name="infoSize">The number of bytes of device information.</param>
      /// <param name="deviceCount">As an input, the number of devices supported by the application; as an
      ///   output, the number of devices installed in the system.</param>
      /// <returns>an HRESULT indicating whether the device information was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetDeviceList(IntPtr deviceInfo, [In, Out] ref Int32 infoSize, [In, Out] ref Int32 deviceCount);

      /// <summary>
      /// Select (activate) a specific B2C2 device.
      /// </summary>
      /// <param name="deviceId">The identifier of the device to select.</param>
      /// <returns>an HRESULT indicating whether the device was successfully selected</returns>
      [PreserveSig]
      Int32 SelectDevice(UInt32 deviceId);
    }

    #endregion

    #region AV control

    [ComVisible(true), ComImport,
      Guid("295950b0-696d-4a04-9ee3-c031a0bfbede"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2AVCtrl
    {
      /// <summary>
      /// Register the audio and/or video PID(s) that are of interest to the application. Packets marked
      /// with these PIDs will be passed on B2C2 filter audio and video output pins.
      /// </summary>
      /// <param name="audioPid">The audio PID (or zero to not register an audio PID).</param>
      /// <param name="videoPid">The video PID (or zero to not register a video PID).</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      Int32 SetAudioVideoPIDs(Int32 audioPid, Int32 videoPid);
    }

    [ComVisible(true), ComImport,
      Guid("9c0563ce-2ef7-4568-a297-88c7bb824075"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2AVCtrl2 : IB2C2MPEG2AVCtrl
    {
      /// <summary>
      /// Deregister the current audio and/or video PID(s) when they are no longer of interest to the
      /// application. Packets marked with the previously registered PIDs will no longer be passed on the
      /// B2C2 filter audio and video output pins.
      /// </summary>
      /// <param name="audioPid">A non-zero value to deregister the current audio PID.</param>
      /// <param name="videoPid">A non-zero value to deregister the current video PID.</param>
      /// <returns>an HRESULT indicating whether the PID(s) were successfully deregistered</returns>
      [PreserveSig]
      Int32 DeleteAudioVideoPIDs(Int32 audioPid, Int32 videoPid);

      /// <summary>
      /// Get the current audio and video settings.
      /// </summary>
      /// <param name="openAudioStreamCount">The number of currently open audio streams.</param>
      /// <param name="openVideoStreamCount">The number of currently open video streams.</param>
      /// <param name="totalAudioStreamCount">The number of audio streams in the full transport stream.</param>
      /// <param name="totalVideoStreamCount">The number of video streams in the full transport stream.</param>
      /// <param name="audioPid">The current audio PID.</param>
      /// <param name="videoPid">The current video PID.</param>
      /// <returns>an HRESULT indicating whether the settings were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetAudioVideoState([Out] out Int32 openAudioStreamCount, [Out] out Int32 openVideoStreamCount,
            [Out] out Int32 totalAudioStreamCount, [Out] out Int32 totalVideoStreamCount,
            [Out] out Int32 audioPid, [Out] out Int32 videoPid
      );

      /// <summary>
      /// Register a callback delegate that the interface can use to notify the application about video
      /// stream information.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      Int32 SetCallbackForVideoMode(OnSkyStarVideoInfo callback);
    }

    [ComVisible(true), ComImport,
      Guid("3ca933bb-4378-4e03-8abd-02450169aa5e"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2AVCtrl3 : IB2C2MPEG2AVCtrl2
    {
      /// <summary>
      /// Get IR data from the interface. The size of each code is two bytes, and up to 4 codes may be
      /// retrieved in one call.
      /// </summary>
      /// <param name="dataBuffer">A pointer to a buffer for the interface to populate.</param>
      /// <param name="bufferCapacity">The number of IR codes that the buffer is able to hold. Note that this
      ///   is not the same as the size of the buffer since the code size is two bytes not one.</param>
      /// <returns>an HRESULT indicating whether the IR data was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetIRData([Out] out IntPtr dataBuffer, [In, Out] ref Int32 bufferCapacity);
    }

    #endregion

    #region timeshifting

    [ComVisible(true), ComImport,
      Guid("a306af1c-51d9-496d-9e7a-1cfe28f51fda"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TimeshiftCtrl
    {
      /// <summary>
      /// Enable the timeshifting capability.
      /// </summary>
      /// <returns>an HRESULT indicating whether the timeshifting capability was successfully enabled</returns>
      [PreserveSig]
      Int32 Enable();

      /// <summary>
      /// Disable the timeshifting capability.
      /// </summary>
      /// <returns>an HRESULT indicating whether the timeshifting capability was successfully disabled</returns>
      [PreserveSig]
      Int32 Disable();

      /// <summary>
      /// Get the name of the file configured for use for timeshifting.
      /// </summary>
      /// <param name="fileName">The name of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file name was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFilename([Out, MarshalAs(UnmanagedType.LPWStr)] out String fileName);

      /// <summary>
      /// Set the name of the file to use for timeshifting. Default is C:\Timeshift.ts.
      /// </summary>
      /// <param name="fileName">The name of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file name was successfully set</returns>
      [PreserveSig]
      Int32 SetFilename([MarshalAs(UnmanagedType.LPWStr)] String fileName);

      /// <summary>
      /// Get the playback marker position within the timeshifting file.
      /// </summary>
      /// <param name="filePosition">The playback marker position.</param>
      /// <returns>an HRESULT indicating whether the marker position was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFilePosition([Out] out Int64 filePosition);

      /// <summary>
      /// Set the playback marker position within the timeshifting file.
      /// </summary>
      /// <param name="filePosition">The playback marker position.</param>
      /// <returns>an HRESULT indicating whether the marker position was successfully set</returns>
      [PreserveSig]
      Int32 SetFilePosition(Int64 filePosition);

      /// <summary>
      /// Get the current size of the file configured for use for timeshifting.
      /// </summary>
      /// <param name="fileSize">The size of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file size was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFileSize([Out] out Int64 fileSize);

      /// <summary>
      /// Get the value of one of the timeshifting interface options.
      /// </summary>
      /// <param name="option">The option to get.</param>
      /// <param name="value">The option value.</param>
      /// <returns>an HRESULT indicating whether the option value was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetOption(SkyStarPvrOption option, [Out] out Int32 value);

      /// <summary>
      /// Set the value of one of the timeshifting interface options.
      /// </summary>
      /// <param name="option">The option to set.</param>
      /// <param name="value">The option value to set.</param>
      /// <returns>an HRESULT indicating whether the option value was successfully set</returns>
      [PreserveSig]
      Int32 SetOption(SkyStarPvrOption option, Int32 value);

      /// <summary>
      /// Start recording during live streaming. Recording is usually only started when live streaming is
      /// paused.
      /// </summary>
      /// <returns>an HRESULT indicating whether recording was successfully started</returns>
      [PreserveSig]
      Int32 StartRecord();

      /// <summary>
      /// Stop recording immediately. Recording is usually stopped when timeshifting catches up with the
      /// live position.
      /// </summary>
      /// <returns>an HRESULT indicating whether recording was successfully stopped</returns>
      [PreserveSig]
      Int32 StopRecord();

      /// <summary>
      /// Register a callback delegate that the interface can use to notify the application about critical
      /// playback/recording state changes.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      Int32 SetCallback(OnSkyStarTimeShiftState callback);
    }

    #endregion

    #region multicast

    [ComVisible(true), ComImport,
      Guid("0b5a8a87-7133-4a37-846e-77f568a52155"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2MulticastCtrl
    {
      /// <summary>
      /// Enable the video/audio multicasting capability. Any streams that were previously active will be
      /// re-enabled.
      /// </summary>
      /// <returns>an HRESULT indicating whether the multicasting capability was successfully enabled</returns>
      [PreserveSig]
      Int32 Enable();

      /// <summary>
      /// Disable the video/audio multicasting capability. All active streams will be disabled.
      /// </summary>
      /// <returns>an HRESULT indicating whether the multicasting capability was successfully disabled</returns>
      [PreserveSig]
      Int32 Disable();

      /// <summary>
      /// Get the network interface configured for use for multicast operations.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
      /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetNetworkInterface([Out, MarshalAs(UnmanagedType.LPStr)] out String address);

      /// <summary>
      /// Set the network interface to use for multicast operations.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
      /// <returns>an HRESULT indicating whether the address was successfully set</returns>
      [PreserveSig]
      Int32 SetNetworkInterface([MarshalAs(UnmanagedType.LPStr)] String address);

      /// <summary>
      /// Start multicasting the content from a specific set of PIDs on a specific network interface.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1) to multicast on.</param>
      /// <param name="port">The network interface port to multicast from.</param>
      /// <param name="pidCount">The number of PIDs in the list of PIDs to multicast.</param>
      /// <param name="pidList">A pointer to a list of PIDs (Int32) to multicast.</param>
      /// <returns>an HRESULT indicating whether the multicast was successfully started</returns>
      [PreserveSig]
      Int32 StartMulticast([MarshalAs(UnmanagedType.LPStr)] String address, UInt16 port, Int32 pidCount, IntPtr pidList);

      /// <summary>
      /// Stop multicasting on a specific network interface.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1) to stop multicasting on.</param>
      /// <param name="port">The network interface port to stop multicasting from.</param>
      /// <returns>an HRESULT indicating whether the multicast was successfully stopped</returns>
      [PreserveSig]
      Int32 StopMulticast([MarshalAs(UnmanagedType.LPStr)] String address, UInt16 port);
    }

    #endregion

    #endregion

    #region structs

    private struct TunerCapabilities
    {
      public SkyStarTunerType TunerType;
      public UInt32 ConstellationSupported;       // Is SetModulation() supported?
      public UInt32 FecSupported;                 // Is SetFec() suppoted?
      public UInt32 MinTransponderFrequency;      // unit = kHz
      public UInt32 MaxTransponderFrequency;      // unit = kHz
      public UInt32 MinTunerFrequency;            // unit = kHz
      public UInt32 MaxTunerFrequency;            // unit = kHz
      public UInt32 MinSymbolRate;                // unit = Baud
      public UInt32 MaxSymbolRate;                // unit = Baud
      private UInt32 AutoSymbolRate;              // Obsolete		
      public SkyStarPerformanceMonitoringCapability PerformanceMonitoringCapabilities;
      public UInt32 LockTime;                     // unit = ms
      public UInt32 KernelLockTime;               // unit = ms
      public SkyStarAcquisionCapability AcquisitionCapabilities;
    }

    private struct MacAddress
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public byte[] Address;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DeviceInfo
    {
      public UInt32 DeviceId;
      public MacAddress MacAddress;
      public SkyStarModulation Modulation;
      public SkyStarBusType BusInterface;
      public UInt16 InUse;
      public UInt32 ProductId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
      public String ProductName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
      public String ProductDescription;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
      public String ProductRevision;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 61)]
      public String ProductFrontEnd;
    }

    private struct MacAddressList
    {
      public Int32 AddressCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxMacAddressCount)]
      public MacAddress[] Address;
    }

    private struct VideoInfo
    {
      public UInt16 HorizontalResolution;
      public UInt16 VerticalResolution;
      public SkyStarVideoAspectRatio AspectRatio;
      public SkyStarVideoFrameRate FrameRate;
    }

    #endregion

    #region callback definitions

    /// <summary>
    /// Called by the data interface to pass raw transport stream packet data to the application.
    /// </summary>
    /// <remarks>
    /// The data must be copied out of the buffer and return control to the interface as quickly as possible.
    /// </remarks>
    /// <param name="pid">The PID for the stream that the packet is associated with.</param>
    /// <param name="data">The packet.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate UInt32 OnSkyStarTsData(UInt16 pid, IntPtr data);

    /// <summary>
    /// Called by the AV interface when the interface wants to notify the controlling application about the
    /// video stream information.
    /// </summary>
    /// <param name="info">The video stream information.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate UInt32 OnSkyStarVideoInfo(VideoInfo info);

    /// <summary>
    /// Called by the timeshifting interface when the interface needs to notify the controlling application
    /// about critical playback/recording status changes.
    /// </summary>
    /// <param name="state">The current timeshifting interface state.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate UInt32 OnSkyStarTimeShiftState(SkyStarPvrCallbackState state);

    #endregion

    #region constants

    private static readonly Guid B2C2AdapterClass = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x00, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);

    private const int MaxDeviceCount = 16;
    private const int MaxMacAddressCount = 32;
    private const int TunerCapabilitiesSize = 56;

    #endregion




    #region variables

    private IBaseFilter _filterB2C2Adapter;
    private IB2C2MPEG2DataCtrl6 _dataInterface;
    private IB2C2MPEG2TunerCtrl4 _tunerInterface;
    private readonly IntPtr _ptrDisEqc;
    private readonly DiSEqCMotor _diseqcMotor;
    private readonly bool _useDISEqCMotor;

    #endregion

    #region imports

    /*[DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SetPidToPin(IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin, UInt16 pid);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool DeleteAllPIDs(IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetSNR(IB2C2MPEG2TunerCtrl2 tunerCtrl, [Out] out int a, [Out] out int b);*/

    #endregion

    #region ctor

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardDvbSS2"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDvbSS2(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _useDISEqCMotor = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(device.DevicePath);
      if (card != null)
      {
        Setting setting = layer.GetSetting("dvbs" + card.IdCard + "motorEnabled", "no");
        if (setting.Value == "yes")
          _useDISEqCMotor = true;
      }


      _ptrDisEqc = Marshal.AllocCoTaskMem(20);
      _diseqcMotor = new DiSEqCMotor(this);
      GetTunerCapabilities();
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The subchannel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbs ss2: Scan:{0}", channel);

      try
      {
        if (!BeforeTune(ref subChannelId, channel))
        {
          return null;
        }
        if (!DoTune())
        {
          return null;
        }
        AfterTune(subChannelId, true);

        Log.Log.WriteFile("ss2:scan done");
        return _mapSubChannels[subChannelId];
      }
      catch (TvExceptionNoSignal)
      {
        throw;
      }
      catch (TvExceptionNoPMT)
      {
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The subchannel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbs ss2: Tune:{0}", channel);

      try
      {
        if (!BeforeTune(ref subChannelId, channel))
        {
          return null;
        }
        if (!DoTune())
        {
          return null;
        }
        AfterTune(subChannelId, false);

        Log.Log.WriteFile("ss2:tune done");
        return _mapSubChannels[subChannelId];
      }
      catch (Exception)
      {
        if (subChannelId > -1)
        {
          FreeSubChannel(subChannelId);
        }
        throw;
      }
    }

    private void AfterTune(int subChannelId, bool ignorePMT)
    {
      _tunerInterface.CheckLock();
      _lastSignalUpdate = DateTime.MinValue;
      SendHwPids(new List<ushort>());
      _mapSubChannels[subChannelId].OnAfterTune();
      try
      {
        try
        {
          RunGraph(subChannelId);
        }
        catch (TvExceptionNoPMT)
        {
          if (!ignorePMT)
          {
            throw;
          }
        }
      }
      catch (Exception)
      {
        FreeSubChannel(subChannelId);
        throw;
      }
    }

    private bool DoTune()
    {
      int hr = -1;
      int lockRetries = 0;
      while
        (((uint)hr == 0x90010115 || hr == -1) && lockRetries < 5)
      {
        hr = _tunerInterface.SetTunerStatus();
        _tunerInterface.CheckLock();
        if (((uint)hr) == 0x90010115)
        {
          Log.Log.Info("ss2:could not lock tuner...sleep 20ms");
          System.Threading.Thread.Sleep(20);
          lockRetries++;
        }
      }

      if (((uint)hr) == 0x90010115)
      {
        Log.Log.Info("ss2:could not lock tuner after {0} attempts", lockRetries);
        throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
      }
      if (lockRetries > 0)
      {
        Log.Log.Info("ss2:locked tuner after {0} attempts", lockRetries);
      }

      if (hr != 0)
      {
        hr = _tunerInterface.SetTunerStatus();
        if (hr != 0)
          hr = _tunerInterface.SetTunerStatus();
        if (hr != 0)
        {
          //Log.Log.Error("ss2:SetTunerStatus failed:0x{0:X}", hr);
          throw new TvExceptionGraphBuildingFailed("Graph building failed");
        }
      }
      return true;
    }

    private bool BeforeTune(ref int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("ss2:Tune({0})", channel);
      if (_epgGrabbing)
      {
        _epgGrabbing = false;
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          _epgGrabberCallback.OnEpgCancelled();
        }
      }


      long frequency = 0;
      SkyStarModulation modulation = SkyStarModulation.Qam64;
      int hr;
      switch (_cardType)
      {
        case CardType.DvbS:
          DVBSChannel dvbsChannel = channel as DVBSChannel;
          if (dvbsChannel == null)
          {
            Log.Log.Debug("TvCardDvbSs2: channel is not a DVB-S channel!!! {0}", channel.GetType().ToString());
            return false;
          }

          uint lnbLof;
          uint lnbSwitchFrequency;
          Polarisation polarisation;
          BandTypeConverter.GetLnbTuningParameters(dvbsChannel, _parameters, out lnbLof, out lnbSwitchFrequency, out polarisation);

          frequency = dvbsChannel.Frequency;
          SkyStarPolarisation ss2Polarisation = SkyStarPolarisation.Horizontal;
          if (polarisation == Polarisation.LinearV || polarisation == Polarisation.CircularR)
          {
            ss2Polarisation = SkyStarPolarisation.Vertical;
          }
          hr = _tunerInterface.SetPolarity(ss2Polarisation);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set polarisation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          hr = _tunerInterface.SetSymbolRate(dvbsChannel.SymbolRate);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set symbol rate, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          SkyStarFecRate fec = SkyStarFecRate.Auto;
          switch (dvbsChannel.InnerFecRate)
          {
            case BinaryConvolutionCodeRate.Rate1_2:
              fec = SkyStarFecRate.Rate1_2;
              break;
            case BinaryConvolutionCodeRate.Rate2_3:
              fec = SkyStarFecRate.Rate2_3;
              break;
            case BinaryConvolutionCodeRate.Rate3_4:
              fec = SkyStarFecRate.Rate3_4;
              break;
            case BinaryConvolutionCodeRate.Rate5_6:
              fec = SkyStarFecRate.Rate5_6;
              break;
            case BinaryConvolutionCodeRate.Rate7_8:
              fec = SkyStarFecRate.Rate7_8;
              break;
          }
          hr = _tunerInterface.SetFec(fec);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set FEC rate, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          SkyStarTone toneState = SkyStarTone.Off;
          bool highBand = BandTypeConverter.IsHighBand(dvbsChannel, _parameters);
          if (highBand)
          {
            toneState = SkyStarTone.Tone22k;
          }
          hr = _tunerInterface.SetLnbKHz(toneState);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set tone state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          hr = _tunerInterface.SetLnbFrequency((int)lnbLof);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set LNB LOF frequency, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          /*satelliteIndex = dvbsChannel.SatelliteIndex;
          SkyStarDiseqcPort
          switch (dvbsChannel.Diseqc)
          {
            case DiseqcPort.None:
              disType = SkyStarDiseqcPort.None;
              break;
            case DiseqcPort.SimpleA:
              disType = SkyStarDiseqcPort.SimpleA;
              break;
            case DiseqcPort.SimpleB:
              disType = SkyStarDiseqcPort.SimpleB;
              break;
            case DiseqcPort.PortA:
              disType = SkyStarDiseqcPort.PortA;
              break;
            case DiseqcPort.PortB:
              disType = SkyStarDiseqcPort.PortB;
              break;
            case DiseqcPort.PortC:
              disType = SkyStarDiseqcPort.PortC;
              break;
            case DiseqcPort.PortD:
              disType = SkyStarDiseqcPort.PortD;
              break;
          }
          DVBSChannel dvbsChannel = channel as DVBSChannel;
          ToneBurst toneBurst = ToneBurst.Off;
          if (dvbsChannel.Diseqc == DiseqcPort.SimpleA)
          {
            toneBurst = ToneBurst.ToneBurst;
          }
          else if (dvbsChannel.Diseqc == DiseqcPort.SimpleB)
          {
            toneBurst = ToneBurst.DataBurst;
          }
          Tone22k tone22k = Tone22k.Off;
          if (BandTypeConverter.IsHighBand(dvbsChannel, _parameters))
          {
            tone22k = Tone22k.On;
          }

          //SendCommand();
          SetToneState(toneBurst, tone22k);

          Log.Log.WriteFile("ss2:  Diseqc:{0} {1}", disType, disType);
          hr = _tunerInterface.SetDiseqc((int)disType);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetDiseqc() failed:0x{0:X}", hr);
            return false;
          }
          if (_useDISEqCMotor)
          {
            if (satelliteIndex > 0)
            {
              DisEqcGotoPosition((byte)satelliteIndex);
            }
          }*/

          break;
        case CardType.DvbT:
          DVBTChannel dvbtChannel = channel as DVBTChannel;
          if (dvbtChannel == null)
          {
            Log.Log.Debug("TvCardDvbSs2: channel is not a DVB-T channel!!! {0}", channel.GetType().ToString());
            return false;
          }

          frequency = dvbtChannel.Frequency;
          hr = _tunerInterface.SetBandwidth(dvbtChannel.Bandwidth);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set bandwidth, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }
          // Note: it is not guaranteed that guard interval auto detection is supported, but if it isn't
          // then we can't tune - we have no idea what the actual value should be.
          hr = _tunerInterface.SetGuardInterval(SkyStarGuardInterval.Auto);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to use automatic guard interval detection, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }
          break;
        case CardType.DvbC:
          DVBCChannel dvbcChannel = channel as DVBCChannel;
          if (dvbcChannel == null)
          {
            Log.Log.Debug("TvCardDvbSs2: channel is not a DVB-C channel!!! {0}", channel.GetType().ToString());
            return false;
          }

          frequency = dvbcChannel.Frequency;
          hr = _tunerInterface.SetSymbolRate(dvbcChannel.SymbolRate);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set symbol rate, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          modulation = SkyStarModulation.Qam64;
          switch (dvbcChannel.ModulationType)
          {
            case ModulationType.Mod16Qam:
              modulation = SkyStarModulation.Qam16;
              break;
            case ModulationType.Mod32Qam:
              modulation = SkyStarModulation.Qam32;
              break;
            case ModulationType.Mod128Qam:
              modulation = SkyStarModulation.Qam128;
              break;
            case ModulationType.Mod256Qam:
              modulation = SkyStarModulation.Qam256;
              break;
          }
          hr = _tunerInterface.SetModulation(modulation);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }
          break;
        case CardType.Atsc:
          ATSCChannel atscChannel = channel as ATSCChannel;
          if (atscChannel == null)
          {
            Log.Log.Debug("TvCardDvbSs2: channel is not an ATSC channel!!! {0}", channel.GetType().ToString());
            return false;
          }

          // If the channel modulation scheme is 8 VSB then it is an over-the-air ATSC channel, otherwise
          // it is a cable (QAM annex B) channel.
          modulation = SkyStarModulation.Vsb8;
          if (atscChannel.ModulationType == ModulationType.Mod8Vsb)
          {
            // We normally tune ATSC channels by physical channel number, however we have to tune them by
            // frequency with the SkyStar.
            if (atscChannel.PhysicalChannel <= 6)
            {
              frequency = 45 + (atscChannel.PhysicalChannel * 6);
            }
            else if (atscChannel.PhysicalChannel >= 7 && atscChannel.PhysicalChannel <= 13)
            {
              frequency = 177 + ((atscChannel.PhysicalChannel - 7) * 6);
            }
            else
            {
              frequency = 473 + ((atscChannel.PhysicalChannel - 14) * 6);
            }
            Log.Log.Debug("TvCardDvbSs2: translated ATSC physical channel number {0} to {1} MHz", atscChannel.PhysicalChannel, frequency);
          }
          else
          {
            frequency = atscChannel.Frequency;
            modulation = SkyStarModulation.Qam256AnnexB;
            if (atscChannel.ModulationType == ModulationType.Mod64Qam)
            {
              modulation = SkyStarModulation.Qam64AnnexB;
            }
          }
          hr = _tunerInterface.SetModulation(modulation);
          if (hr != 0)
          {
            Log.Log.Debug("TvCardDvbSs2: failed to set modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }
          break;
      }
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }
      if (_mapSubChannels.ContainsKey(subChannelId) == false)
      {
        subChannelId = GetNewSubChannel(channel);
      }
      _mapSubChannels[subChannelId].CurrentChannel = channel;
      _mapSubChannels[subChannelId].OnBeforeTune();
      if (_interfaceEpgGrabber != null)
      {
        _interfaceEpgGrabber.Reset();
      }
      if (frequency > 13000)
        frequency /= 1000;
      Log.Log.WriteFile("ss2:  Transponder Frequency:{0} MHz", frequency);
      hr = _tunerInterface.SetFrequency((int)frequency);
      if (hr != 0)
      {
        Log.Log.Error("ss2:SetFrequencyKHz() failed:0x{0:X}", hr);
        return false;
      }
      return true;
    }

    #endregion

    #region epg & scanning

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId())
          return null;
        switch (CardType)
        {
          case CardType.DvbT:
            return new DVBTScanning(this);
          case CardType.DvbS:
            return new DVBSScanning(this);
          case CardType.DvbC:
            return new DVBCScanning(this);
          case CardType.Atsc:
            return new ATSCScanning(this);
        }
        return null;
      }
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _tunerDevice.Name;
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public override bool CanTune(IChannel channel)
    {
      if (_cardType == CardType.DvbS)
      {
        if ((channel as DVBSChannel) == null)
          return false;
        return true;
      }
      if (_cardType == CardType.DvbT)
      {
        if ((channel as DVBTChannel) == null)
          return false;
        return true;
      }
      if (_cardType == CardType.DvbC)
      {
        if ((channel as DVBCChannel) == null)
          return false;
        return true;
      }
      if (_cardType == CardType.Atsc)
      {
        if ((channel as ATSCChannel) == null)
          return false;
        return true;
      }
      return false;
    }

    #region SS2 specific

    ///<summary>
    /// Checks if the tuner is locked in and a sginal is present
    ///</summary>
    ///<returns>true, when the tuner is locked and a signal is present</returns>
    public override bool LockedInOnSignal()
    {
      bool isLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!isLocked && ts.TotalSeconds < _parameters.TimeOutTune)
      {
        int hr = _tunerInterface.SetTunerStatus();
        _tunerInterface.CheckLock();
        if (((uint)hr) == 0x90010115)
        {
          ts = DateTime.Now - timeStart;
          Log.Log.WriteFile("dvb-s ss2:  LockedInOnSignal waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
        else
        {
          isLocked = true;
        }
      }

      if (!isLocked)
      {
        Log.Log.WriteFile("dvb-s ss2:  LockedInOnSignal could not lock onto channel - no signal or bad signal");
      }
      else
      {
        Log.Log.WriteFile("dvb-s ss2:  LockedInOnSignal ok");
      }
      return isLocked;
    }

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        Log.Log.WriteFile("ss2: build graph");
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Error("ss2: Graph already built");
          throw new TvException("Graph already built");
        }
        DevicesInUse.Instance.Add(_tunerDevice);
        _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        //=========================================================================================================
        // add the skystar 2 specific filters
        //=========================================================================================================
        Log.Log.WriteFile("ss2:CreateGraph() create B2C2 adapter");
        _filterB2C2Adapter =
          (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(B2C2AdapterClass, false));
        if (_filterB2C2Adapter == null)
        {
          Log.Log.Error("ss2:creategraph() _filterB2C2Adapter not found");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        Log.Log.WriteFile("ss2:creategraph() add filters to graph");
        int hr = _graphBuilder.AddFilter(_filterB2C2Adapter, "B2C2-Source");
        if (hr != 0)
        {
          Log.Log.Error("ss2: FAILED to add B2C2-Adapter");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        // get interfaces
        _dataInterface = _filterB2C2Adapter as IB2C2MPEG2DataCtrl6;
        if (_dataInterface == null)
        {
          Log.Log.Error("ss2: cannot get IB2C2MPEG2DataCtrl6");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        _tunerInterface = _filterB2C2Adapter as IB2C2MPEG2TunerCtrl4;
        if (_tunerInterface == null)
        {
          Log.Log.Error("ss2: cannot get IB2C2MPEG2TunerCtrl4");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        //=========================================================================================================
        // initialize skystar 2 tuner
        //=========================================================================================================
        Log.Log.WriteFile("ss2: Initialize Tuner()");
        hr = _tunerInterface.Initialize();
        if (hr != 0)
        {
          //System.Diagnostics.Debugger.Launch();
          Log.Log.Error("ss2: Tuner initialize failed:0x{0:X}", hr);
          // if the skystar2 card is detected as analogue, it needs a device reset 

          ((IMediaControl)_graphBuilder).Stop();
          FreeAllSubChannels();
          FilterGraphTools.RemoveAllFilters(_graphBuilder);

          if (_graphBuilder != null)
          {
            Release.ComObject("graph builder", _graphBuilder);
            _graphBuilder = null;
          }

          if (_capBuilder != null)
          {
            Release.ComObject("capBuilder", _capBuilder);
            _capBuilder = null;
          }

          DevicesInUse.Instance.Remove(_tunerDevice);

          /*
          if (initResetTries == 0)
          {
            Log.Log.Error("ss2: resetting driver");
            HardwareHelperLib.HH_Lib hwHelper = new HardwareHelperLib.HH_Lib();
            string[] deviceDriverName = new string[1];
            deviceDriverName[0] = DEVICE_DRIVER_NAME;
            hwHelper.SetDeviceState(deviceDriverName, false);
            hwHelper.SetDeviceState(deviceDriverName, true);
            initResetTries++;          

            BuildGraph();
          }
          else
          {
            Log.Log.Error("ss2: resetting driver did not help");          
            CardPresent = false;
          }    
          */
          CardPresent = false;
          return;
        }
        // call checklock once, the return value dont matter
        _tunerInterface.CheckLock();
        AddTsWriterFilterToGraph();
        IBaseFilter lastFilter;
        ConnectInfTeeToSS2(out lastFilter);

        // TODO: ICustomDevice loading goes here.

        if (!ConnectTsWriter(lastFilter))
        {
          throw new TvExceptionGraphBuildingFailed("Graph building failed");
        }
        SendHwPids(new List<ushort>());
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw new TvExceptionGraphBuildingFailed("Graph building failed", ex);
      }
    }

    /// <summary>
    /// Connects the SS2 filter to the infTee
    /// </summary>
    private void ConnectInfTeeToSS2(out IBaseFilter lastFilter)
    {
      Log.Log.WriteFile("dvb:add Inf Tee filter");
      _infTee = (IBaseFilter)new InfTee();
      int hr = _graphBuilder.AddFilter(_infTee, "Inf Tee");
      if (hr != 0)
      {
        Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }

      Log.Log.WriteFile("ss2:ConnectMainTee()");
      IPin pinOut = DsFindPin.ByDirection(_filterB2C2Adapter, PinDirection.Output, 2);
      IPin pinIn = DsFindPin.ByDirection(_infTee, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Error("ss2:unable to find pin 2 of b2c2adapter");
        throw new TvException("unable to find pin 2 of b2c2adapter");
      }
      if (pinIn == null)
      {
        Log.Log.Error("ss2:unable to find pin 0 of _infTeeMain");
        throw new TvException("unable to find pin 0 of _infTeeMain");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      Release.ComObject("b2c2pin2", pinOut);
      Release.ComObject("mpeg2demux pinin", pinIn);
      if (hr != 0)
      {
        Log.Log.Error("ss2:unable to connect b2c2->_infTeeMain");
        throw new TvException("unable to connect b2c2->_infTeeMain");
      }
      lastFilter = _infTee;
    }

    /// <summary>
    /// Sends the HW pidSet.
    /// </summary>
    /// <param name="pidSet">The pidSet.</param>
    private void SendHwPids(List<ushort> pids)
    {
      const int PID_CAPTURE_ALL_INCLUDING_NULLS = 0x2000;
      //Enables reception of all PIDs in the transport stream including the NULL PID
      //const int PID_CAPTURE_ALL_EXCLUDING_NULLS = 0x2001;//Enables reception of all PIDs in the transport stream excluding the NULL PID.

      // Delete all PIDs.
      /*do
      {
        int hr = _dataInterface->GetTsState(NULL, NULL, &pidCount, pidSet);
        if (SUCCEEDED(hr))
        {
          hr = pB2C2FilterDataCtrl->DeletePIDsFromPin(pidCount, pidSet, pin);
        }
        else
        {
          Log.Log.Error("ss2:DeleteAllPIDs() failed pid:0x2000");
        }
      } while (pidCount > 0);

      //if (pidSet.Count == 0 || true)
      {
        Log.Log.WriteFile("ss2:hw pidSet:all");
        //int added = SetPidToPin(_interfaceB2C2DataCtrl, 0, PID_CAPTURE_ALL_INCLUDING_NULLS);
	long	count=1;
	long	pidSet[2];
	HRESULT hr;

	pidSet[0]=pid;

	if(pB2C2FilterDataCtrl)
	{
		hr=pB2C2FilterDataCtrl->AddPIDsToPin(&count,pidSet,pin);
		if(SUCCEEDED(hr))
			return count;
	}
	return 0;
        if (added != 1)
        {
          Log.Log.Error("ss2:SetPidToPin() failed pid:0x2000");
        }
      }*/
      /* unreachable
      else
      {
        int maxPids;
        _interfaceB2C2DataCtrl.GetMaxPIDCount(out maxPids);
        for (int i = 0; i < pidSet.Count && i < maxPids; ++i)
        {
          ushort pid = (ushort)pidSet[i];
          Log.Log.WriteFile("ss2:hw pidSet:0x{0:X}", pid);
          SetPidToPin(_interfaceB2C2DataCtrl, 0, pid);
        }
      }
      */
    }

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected override void UpdateSignalStatus(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
        {
          return;
        }
      }
      if (!GraphRunning() ||
        CurrentChannel == null ||
        _tunerInterface == null)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }

      int level, quality;
      _tunerLocked = (_tunerInterface.CheckLock() == 0);
      _tunerInterface.GetSignalStrength(out level);
      _tunerInterface.GetSignalQuality(out quality);
      if (level < 0)
        level = 0;
      if (level > 100)
        level = 100;
      if (quality < 0)
        quality = 0;
      if (quality > 100)
        quality = 100;
      _signalQuality = quality;
      _signalLevel = level;
      _lastSignalUpdate = DateTime.Now;
    }

    private void GetTunerCapabilities()
    {
      Log.Log.WriteFile("ss2: GetTunerCapabilities");
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);
      _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      _capBuilder.SetFiltergraph(_graphBuilder);
      //=========================================================================================================
      // add the skystar 2 specific filters
      //=========================================================================================================
      Log.Log.WriteFile("ss2:GetTunerCapabilities() create B2C2 adapter");
      _filterB2C2Adapter =
        (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(B2C2AdapterClass, false));
      if (_filterB2C2Adapter == null)
      {
        Log.Log.Error("ss2:GetTunerCapabilities() _filterB2C2Adapter not found");
        return;
      }
      _tunerInterface = _filterB2C2Adapter as IB2C2MPEG2TunerCtrl4;
      if (_tunerInterface == null)
      {
        Log.Log.Error("ss2: cannot get IB2C2MPEG2TunerCtrl4");
        return;
      }
      //=========================================================================================================
      // initialize skystar 2 tuner
      //=========================================================================================================
      /* Not necessary for query-only application
       
      Log.Log.WriteFile("ss2: Initialize Tuner()");
      hr = _interfaceB2C2TunerCtrl.Initialize();
      if (hr != 0)
      {
        Log.Log.Error("ss2: Tuner initialize failed:0x{0:X}", hr);
        //return;
      }*/
      //=========================================================================================================
      // Get tuner type (DVBS, DVBC, DVBT, ATSC)
      //=========================================================================================================
      IntPtr ptCaps = Marshal.AllocHGlobal(TunerCapabilitiesSize);
      int byteCount;
      int hr = _tunerInterface.GetTunerCapabilities(ptCaps, out byteCount);
      if (hr != 0)
      {
        Log.Log.Error("ss2: Tuner Type failed:0x{0:X}", hr);
        return;
      }
      TunerCapabilities tc = (TunerCapabilities)Marshal.PtrToStructure(ptCaps, typeof(TunerCapabilities));
      switch (tc.TunerType)
      {
        case SkyStarTunerType.Satellite:
          Log.Log.WriteFile("ss2: Card type = DVBS");
          _cardType = CardType.DvbS;
          break;
        case SkyStarTunerType.Cable:
          Log.Log.WriteFile("ss2: Card type = DVBC");
          _cardType = CardType.DvbC;
          break;
        case SkyStarTunerType.Terrestrial:
          Log.Log.WriteFile("ss2: Card type = DVBT");
          _cardType = CardType.DvbT;
          break;
        case SkyStarTunerType.Atsc:
          Log.Log.WriteFile("ss2: Card type = ATSC");
          _cardType = CardType.Atsc;
          break;
        case SkyStarTunerType.Unknown:
          Log.Log.WriteFile("ss2: Card type = unknown?");
          _cardType = CardType.Unknown;
          break;
      }
      Marshal.FreeHGlobal(ptCaps);
      // Release all used object
      if (_filterB2C2Adapter != null)
      {
        Release.ComObject("tuner filter", _filterB2C2Adapter);
        _filterB2C2Adapter = null;
      }
      _rotEntry.Dispose();
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder);
        _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        Release.ComObject("graph builder", _graphBuilder);
        _graphBuilder = null;
      }
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public override void Dispose()
    {
      if (_graphBuilder == null)
        return;
      if (!CheckThreadId())
        return;

      base.Dispose();

      Log.Log.WriteFile("ss2:Decompose");

      _dataInterface = null;
      _tunerInterface = null;

      if (_filterB2C2Adapter != null)
      {
        Release.ComObject("tuner filter", _filterB2C2Adapter);
        _filterB2C2Adapter = null;
      }
    }

    #endregion

    private void DisEqcGotoPosition(byte position)
    {
      _diseqcMotor.GotoPosition(position);
    }

    /// <summary>
    /// Handles DiSEqC motor operations
    /// </summary>
    public override IDiSEqCMotor DiSEqCMotor
    {
      get { return _diseqcMotor; }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      return false;
    }

    #region graph state change callbacks

    /// <summary>
    /// This callback is invoked when the device BDA graph construction is complete.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="startGraphImmediately">Ensure that the tuner's BDA graph is started immediately.</param>
    public virtual void OnGraphBuilt(ITVCard tuner, out bool startGraphImmediately)
    {
      startGraphImmediately = false;
    }

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="forceGraphStart">Ensure that the tuner's BDA graph is running when the tune request is submitted.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out bool forceGraphStart)
    {
      forceGraphStart = false;
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted but before the device's BDA graph is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted, when the device's BDA graph is running
    /// but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnGraphRunning(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked before the device's BDA graph is stopped.
    /// </summary>
    /// <param name="tuner">The device instance that this device instance is associated with.</param>
    /// <param name="preventGraphStop">Prevent the device's BDA graph from being stopped.</param>
    /// <param name="restartGraph">Allow the device's BDA graph to be stopped, but then restart it immediately.</param>
    public virtual void OnGraphStop(ITVCard tuner, out bool preventGraphStop, out bool restartGraph)
    {
      preventGraphStop = false;
      restartGraph = false;
    }

    /// <summary>
    /// This callback is invoked before the device's BDA graph is paused.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="preventGraphPause">Prevent the device's BDA graph from being paused.</param>
    /// <param name="restartGraph">Stop the device's BDA graph, and then restart it immediately.</param>
    public virtual void OnGraphPause(ITVCard tuner, out bool preventGraphPause, out bool restartGraph)
    {
      preventGraphPause = false;
      restartGraph = false;
    }

    #endregion

    #endregion

    #region IDiseqcController members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("SS2: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);
      bool success = true;
      int hr;

      SkyStarDiseqcPort burst = SkyStarDiseqcPort.None;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        burst = SkyStarDiseqcPort.SimpleA;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        burst = SkyStarDiseqcPort.SimpleB;
      }
      if (burst != SkyStarDiseqcPort.None)
      {
        hr = _tunerInterface.SetDiseqc(burst);
        if (hr != 0)
        {
          Log.Log.Error("SS2: set burst failed, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      SkyStarTone tone = SkyStarTone.Off;
      if (tone22kState == Tone22k.On)
      {
        tone = SkyStarTone.Tone22k;
      }
      hr = _tunerInterface.SetLnbKHz(tone);
      if (hr != 0)
      {
        Log.Log.Error("SS2: set 22k failed, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }
      return success;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      IB2C2MPEG2TunerCtrl4 tuner4 = _filterB2C2Adapter as IB2C2MPEG2TunerCtrl4;
      if (tuner4 == null || command == null)
      {
        return false;
      }
      Marshal.Copy(command, 0, _ptrDisEqc, command.Length);
      tuner4.SendDiSEqCCommand(command.Length, _ptrDisEqc);
      return true;
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

    protected override DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name)
    {
      DVBSChannel channel = new DVBSChannel();
      channel.NetworkId = networkid;
      channel.TransportId = transportid;
      channel.ServiceId = serviceid;
      channel.Name = name;
      return channel;
    }
  }
}