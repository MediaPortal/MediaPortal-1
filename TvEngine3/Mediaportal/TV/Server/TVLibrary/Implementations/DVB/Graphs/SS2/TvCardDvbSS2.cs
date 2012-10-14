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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.ATSC;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.SS2
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles B2C2 based TechniSat tuners.
  /// </summary>
  public class TvCardDvbSS2 : TvCardDvbBase, ICustomDevice, IPidFilterController, IDiseqcDevice
  {
    #region enums

    private enum B2c2Error
    {
      NotLockedOnSignal = -1878982377,    // [0x90010115]

      // For AddPIDsToPin() or AddPIDs()...
      AlreadyExists = 0x10011000,         // PID already registered.
      PidError = -1878978558,             // [0x90011001]
      AlreadyFull = -1878978557,          // Max PID count exceeded. [0x90011002]

      // General...
      CreateInterface = -1878917118,      // Not all interfaces could be created correctly. [0x90020001]
      UnsupportedDevice = -1878917117,    // The given device is not B2C2-compatible (Linux). [0x90020002]
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

      DiseqcInProgress = 1878917105,      // [0x9002000f]
      Diseqc12NotSupported,
      NoDeviceAvailable
    }

    private enum B2c2TunerType
    {
      Unknown = -1,
      Satellite,
      Cable,
      Terrestrial,
      Atsc
    }

    private enum B2c2BusType
    {
      Pci = 0,
      Usb1 = 1    // USB 1.1
    }

    [Flags]
    private enum B2c2PerformanceMonitoringCapability
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
    private enum B2c2AcquisionCapability
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

    private enum B2c2Modulation
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

    private enum B2c2Polarisation
    {
      Horizontal = 0,   // 18 V - also use for circular left.
      Vertical,         // 13 V - also use for circular right.
      PowerOff = 10     // Turn the power supply to the LNB completely off.
    }

    private enum B2c2FecRate
    {
      Rate1_2 = 1,
      Rate2_3,
      Rate3_4,
      Rate5_6,
      Rate7_8,
      Auto
    }

    private enum B2c2Tone
    {
      Off = 0,
      Tone22k,
      Tone33k,
      Tone44k
    }

    private enum B2c2DiseqcPort
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

    private enum B2c2GuardInterval
    {
      Guard1_32 = 0,
      Guard1_16,
      Guard1_8,
      Guard1_4,
      Auto
    }

    private enum B2c2PidFilterMode
    {
      AllIncludingNull = 0x2000,
      AllExcludingNull = 0x2001
    }

    private enum B2c2TableId
    {
      Dvb = 0x3e,
      Atsc = 0x3f,
      Auto = 0xff
    }

    private enum B2c2KeyType
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

    private enum B2c2PvrOption
    {
      AutoDeleteRecordFile = 0,
      AutoRecordWithPlay
    }

    private enum B2c2PvrCallbackState
    {
      EndOfFile = 0,
      FileError       // Indicates file access or disk space issues.
    }

    private enum B2c2VideoAspectRatio : byte
    {
      Invalid = 0,
      Square,
      Standard4x3,
      Wide16x9,
      Other
    }

    private enum B2c2VideoFrameRate : byte
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
      Guid("61a9051f-04c4-435e-8742-9edd2c543ce9"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TunerCtrl4
    {
      #region IB2C2MPEG2TunerCtrl

      #region setters

      /// <summary>
      /// Set the multiplex frequency.
      /// </summary>
      /// <param name="frequency">The frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the frequency was successfully set</returns>
      [PreserveSig]
      Int32 SetFrequency(Int32 frequency);

      /// <summary>
      /// Set the multiplex symbol rate. Only applicable for DVB-S and DVB-C tuners.
      /// </summary>
      /// <param name="symbolRate">The symbol rate in ks/s.</param>
      /// <returns>an HRESULT indicating whether the symbol rate was successfully set</returns>
      [PreserveSig]
      Int32 SetSymbolRate(Int32 symbolRate);

      /// <summary>
      /// Set the LNB local oscillator frequency. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="lnbFrequency">The local oscillator frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the local oscillator frequency was successfully set</returns>
      [PreserveSig]
      Int32 SetLnbFrequency(Int32 lnbFrequency);

      /// <summary>
      /// Set the multiplex FEC rate. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="fecRate">The FEC rate.</param>
      /// <returns>an HRESULT indicating whether the FEC rate was successfully set</returns>
      [PreserveSig]
      Int32 SetFec(B2c2FecRate fecRate);

      /// <summary>
      /// Set the multiplex polarisation. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="polarisation">The polarisation.</param>
      /// <returns>an HRESULT indicating whether the polarisation was successfully set</returns>
      [PreserveSig]
      Int32 SetPolarity(B2c2Polarisation polarisation);

      /// <summary>
      /// Set the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="toneState">The tone state.</param>
      /// <returns>an HRESULT indicating whether the tone state was successfully set</returns>
      [PreserveSig]
      Int32 SetLnbKHz(B2c2Tone toneState);

      /// <summary>
      /// Switch to a specific satellite. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
      /// <returns>an HRESULT indicating whether the DiSEqC command was successfully sent</returns>
      [PreserveSig]
      Int32 SetDiseqc(B2c2DiseqcPort diseqcPort);

      /// <summary>
      /// Set the multiplex modulation scheme. Only applicable for DVB-C tuners.
      /// </summary>
      /// <param name="modulation">The modulation scheme.</param>
      /// <returns>an HRESULT indicating whether the modulation scheme was successfully set</returns>
      [PreserveSig]
      Int32 SetModulation(B2c2Modulation modulation);

      #endregion

      /// <summary>
      /// Initialise the tuner control interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the interface was successfully initialised</returns>
      [PreserveSig]
      Int32 Initialize();

      /// <summary>
      /// Apply previously set tuning parameter values.
      /// </summary>
      /// <returns>an HRESULT indicating whether the tuner is locked on signal</returns>
      [PreserveSig]
      Int32 SetTunerStatus();

      /// <summary>
      /// Check the lock status of the tuner.
      /// </summary>
      /// <returns>an HRESULT indicating whether the tuner is locked or not</returns>
      [PreserveSig]
      Int32 CheckLock();

      /// <summary>
      /// Get the tuner capabilities.
      /// </summary>
      /// <param name="capabilities">A pointer to a tuner capabilities structure.</param>
      /// <param name="size">The size of the capabilities structure.</param>
      /// <returns>an HRESULT indicating whether the capabilities were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetTunerCapabilities(IntPtr capabilities, [In, Out] ref Int32 size);

      #region getters

      /// <summary>
      /// Get the tuned multiplex frequency. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="frequency">The multiplex frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the multiplex frequency was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFrequency([Out] out Int32 frequency);

      /// <summary>
      /// Get the tuned multiplex symbol rate in ks/s. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="symbolRate">The multiplex symbol rate in ks/s.</param>
      /// <returns>an HRESULT indicating whether the multiplex symbol rate was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSymbolRate([Out] out Int32 symbolRate);

      /// <summary>
      /// Get the modulation scheme for the tuned multiplex. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="modulation">The multiplex modulation scheme.</param>
      /// <returns>an HRESULT indicating whether the multiplex modulation scheme was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetModulation([Out] out B2c2Modulation modulation);

      #endregion

      #region signal strength/quality metrics

      /// <summary>
      /// Get the tuner/demodulator signal strength statistic.
      /// </summary>
      /// <param name="signalStrength">The signal strength as a percentage.</param>
      /// <returns>an HRESULT indicating whether the signal strength was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSignalStrength([Out] out Int32 signalStrength);

      /// <summary>
      /// Obsolete. Use GetSignalStrength() or GetSignalQuality() instead.
      /// </summary>
      /// <param name="signalLevel">The signal level in dBm.</param>
      /// <returns>an HRESULT indicating whether the signal level was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSignalLevel([Out] out float signalLevel);

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
      /// Get the uncorrected block count since the last call to this function. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetUncorrectedBlocks([Out] out Int32 blockCount);

      /// <summary>
      /// Get the total block count since the last call to this function. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetTotalBlocks([Out] out Int32 blockCount);

      #endregion

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

      #endregion

      #region IB2C2MPEG2TunerCtrl2

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
      /// Set the multiplex guard interval. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="interval">The guard interval.</param>
      /// <returns>an HRESULT indicating whether the guard interval was successfully set</returns>
      [PreserveSig]
      Int32 SetGuardInterval(B2c2GuardInterval interval);

      /// <summary>
      /// Get the multiplex guard interval. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="interval">The guard interval.</param>
      /// <returns>an HRESULT indicating whether the guard interval was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetGuardInterval([Out] out B2c2GuardInterval interval);

      /// <summary>
      /// Get the multiplex FEC rate. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="fecRate">The FEC rate.</param>
      /// <returns>an HRESULT indicating whether the FEC rate was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFec([Out] out B2c2FecRate fecRate);

      /// <summary>
      /// Get the multiplex polarisation. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="polarisation">The polarisation.</param>
      /// <returns>an HRESULT indicating whether the polarisation was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetPolarity([Out] out B2c2Polarisation polarisation);

      /// <summary>
      /// Get the currenly selected satellite. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
      /// <returns>an HRESULT indicating whether the setting was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetDiseqc([Out] out B2c2DiseqcPort diseqcPort);

      /// <summary>
      /// Get the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="toneState">The tone state.</param>
      /// <returns>an HRESULT indicating whether the tone state was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetLnbKHz([Out] out B2c2Tone toneState);

      /// <summary>
      /// Get the LNB local oscillator frequency. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="lnbFrequency">The local oscillator frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the local oscillator frequency was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetLnbFrequency([Out] out Int32 lnbFrequency);

      #endregion

      /// <summary>
      /// Get the corrected block count since the last call to this function.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetCorrectedBlocks([Out] out Int32 blockCount);

      /// <summary>
      /// Get the tuner/demodulator signal quality statistic.
      /// </summary>
      /// <param name="signalQuality">The signal quality as a percentage.</param>
      /// <returns>an HRESULT indicating whether the signal quality was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetSignalQuality([Out] out Int32 signalQuality);

      #endregion

      #region IB2C2MPEG2TunerCtrl3

      /// <summary>
      /// Set the multiplex bandwidth. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="bandwidth">The bandwidth in MHz, expected to be 6, 7 or 8.</param>
      /// <returns>an HRESULT indicating whether the bandwidth was successfully set</returns>
      [PreserveSig]
      Int32 SetBandwidth(Int32 bandwidth);

      /// <summary>
      /// Get the multiplex bandwidth. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="bandwidth">The bandwidth in MHz, expected to be 6, 7 or 8.</param>
      /// <returns>an HRESULT indicating whether the bandwidth was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetBandwidth([Out] out Int32 bandwidth);

      #endregion

      #region IB2C2MPEG2TunerCtrl4

      /// <summary>
      /// Send a raw DiSEqC command.
      /// </summary>
      /// <param name="length">The length of the command in bytes.</param>
      /// <param name="command">The command.</param>
      /// <returns>an HRESULT indicating whether the command was successfully sent</returns>
      Int32 SendDiSEqCCommand(Int32 length, IntPtr command);

      #endregion
    }

    #endregion

    #region data control

    [ComVisible(true), ComImport,
      Guid("a12a4531-72d2-40fc-b17d-8f9b0004444f"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2DataCtrl6
    {
      #region IB2C2MPEG2DataCtrl

      #region transport stream PIDs

      /// <summary>
      /// Get the maximum number of transport stream class PIDs that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMaxPIDCount([Out] out Int32 maxPidCount);

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

      #endregion

      #region IP PIDs

      /// <summary>
      /// Get the maximum number of IP class PIDs that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMaxIpPIDCount([Out] out Int32 maxPidCount);

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

      #endregion

      #region all PIDs

      /// <summary>
      /// Deregister all PIDs currently registered with the interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      Int32 PurgeGlobalPIDs();

      /// <summary>
      /// Get the maximum number of PIDs of any class that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMaxGlobalPIDCount([Out] out Int32 maxPidCount);

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

      #endregion

      /// <summary>
      /// Reset the values of the statistics retrieved by GetDataReceptionStats().
      /// </summary>
      /// <returns>an HRESULT indicating whether the statistics were successfully reset</returns>
      [PreserveSig]
      Int32 ResetDataReceptionStats();

      /// <summary>
      /// Get the current values of statistics that can be used for monitoring signal quality. The statistics
      /// are measured since the last call to this function or to ResetDataReceptionStats().
      /// </summary>
      /// <param name="ipRatio">The ratio of correctly received IP class packets to total IP packets.</param>
      /// <param name="tsRatio">The ratio of correctly received TS class packets to total TS packets.</param>
      /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetDataReceptionStats([Out] out Int32 ipRatio, [Out] out Int32 tsRatio);

      #endregion

      #region IB2C2MPEG2DataCtrl2

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

      #endregion

      #region IB2C2MPEG2DataCtrl3

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
              [In, Out] ref Int32 pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] Int32[] pidList
      );

      #endregion

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
      /// Get the list of multicast MAC addresses that are registered with the interface.
      /// </summary>
      /// <param name="addressList">The list of addresses.</param>
      /// <returns>an HRESULT indicating whether the address list was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetMulticastMacAddressList([Out] out MacAddressList addressList);

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

      #endregion

      #region unicast

      /// <summary>
      /// Set the device's unicast MAC address.
      /// </summary>
      /// <param name="address">The address to set.</param>
      /// <returns>an HRESULT indicating whether the address was successfully set</returns>
      [PreserveSig]
      Int32 SetUnicastMacAddress(MacAddress address);

      /// <summary>
      /// Get the device's current unicast MAC address.
      /// </summary>
      /// <param name="address">The current address.</param>
      /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetUnicastMacAddress([Out] out MacAddress address);

      /// <summary>
      /// Restore the unicast MAC address to the default address for the device.
      /// </summary>
      /// <returns>an HRESULT indicating whether the address was successfully restored</returns>
      [PreserveSig]
      Int32 RestoreUnicastMacAddress();

      #endregion

      #endregion

      #region IB2C2MPEG2DataCtrl4

      /// <summary>
      /// Get the device's MAC address.
      /// </summary>
      /// <param name="macAddress">The MAC address.</param>
      /// <returns>an HRESULT indicating whether the MAC address was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetHardwareMacAddress(IntPtr macAddress);

      [PreserveSig]
      Int32 SetTableId(Int32 tableId);

      [PreserveSig]
      Int32 GetTableId([Out] out Int32 tableId);

      #region decrypt keys

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
      /// Get the details for the keys that are registered with and being used by the interface.
      /// </summary>
      /// <param name="keyCount">The number of keys in use.</param>
      /// <param name="keyTypes">A pointer to an array listing the type of each key.</param>
      /// <param name="pids">A pointer to an array listing the PID associated with each key.</param>
      /// <returns>an HRESULT indicating whether the key details were successfully retrieved</returns>
      [PreserveSig]
      Int32 GetKeysInUse([Out] out Int32 keyCount, [Out] out IntPtr keyTypes, [Out] out IntPtr pids);

      /// <summary>
      /// Register a decryption key with the interface.
      /// </summary>
      /// <param name="keyType">The key type.</param>
      /// <param name="pid">The PID to use the key to decrypt.</param>
      /// <param name="key">The key.</param>
      /// <param name="keyLength">The length of the key.</param>
      /// <returns>an HRESULT indicating whether the key was successfully registered</returns>
      [PreserveSig]
      Int32 AddKey(B2c2KeyType keyType, UInt32 pid, IntPtr key, Int32 keyLength);

      /// <summary>
      /// Deregister a decryption key.
      /// </summary>
      /// <param name="keyType">The key type.</param>
      /// <param name="pid">The PID that the key is associated with.</param>
      /// <returns>an HRESULT indicating whether the key was successfully deregistered</returns>
      [PreserveSig]
      Int32 DeleteKey(B2c2KeyType keyType, UInt32 pid);

      /// <summary>
      /// Deregister all decryption keys.
      /// </summary>
      /// <returns>an HRESULT indicating whether the keys were successfully deregistered</returns>
      [PreserveSig]
      Int32 PurgeKeys();

      #endregion

      #endregion

      #region IB2C2MPEG2DataCtrl5

      /// <summary>
      /// Register a callback delegate that the interface can use to pass raw transport stream packets
      /// directly to the application. The packets passed correspond with the transport stream class PIDs
      /// registered with the interface.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      Int32 SetCallbackForTransportStream(OnB2c2TsData callback);

      #endregion

      #region IB2C2MPEG2DataCtrl6

      /// <summary>
      /// Get information about the B2C2 compatible devices installed in the system.
      /// </summary>
      /// <param name="deviceInfo">A pointer to an array of DeviceInfo instances.</param>
      /// <param name="infoSize">The number of bytes of device information.</param>
      /// <param name="deviceCount">As an input, the number of devices supported by the application; as an
      ///   output, the number of devices installed in the system.</param>
      /// <returns>an HRESULT indicating whether the device information was successfully retrieved</returns>
      [PreserveSig]
      int GetDeviceList(IntPtr deviceInfo, [In, Out] ref Int32 infoSize, [In, Out] ref Int32 deviceCount);

      /// <summary>
      /// Select (activate) a specific B2C2 device.
      /// </summary>
      /// <param name="deviceId">The identifier of the device to select.</param>
      /// <returns>an HRESULT indicating whether the device was successfully selected</returns>
      [PreserveSig]
      Int32 SelectDevice(UInt32 deviceId);

      #endregion
    }

    #endregion

    #region AV control

    [ComVisible(true), ComImport,
      Guid("3ca933bb-4378-4e03-8abd-02450169aa5e"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2AVCtrl3
    {
      #region IB2C2MPEG2AVCtrl

      /// <summary>
      /// Register the audio and/or video PID(s) that are of interest to the application. Packets marked
      /// with these PIDs will be passed on B2C2 filter audio and video output pins.
      /// </summary>
      /// <param name="audioPid">The audio PID (or zero to not register an audio PID).</param>
      /// <param name="videoPid">The video PID (or zero to not register a video PID).</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      Int32 SetAudioVideoPIDs(Int32 audioPid, Int32 videoPid);

      #endregion

      #region IB2C2MPEG2AVCtrl2

      /// <summary>
      /// Register a callback delegate that the interface can use to notify the application about video
      /// stream information.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      Int32 SetCallbackForVideoMode(OnB2c2VideoInfo callback);

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

      #endregion

      #region IB2C2MPEG2AVCtrl3

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

      #endregion
    }

    #endregion

    #region timeshifting

    [ComVisible(true), ComImport,
      Guid("a306af1c-51d9-496d-9e7a-1cfe28f51fda"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2C2MPEG2TimeshiftCtrl
    {
      /// <summary>
      /// Set the name of the file to use for timeshifting. Default is C:\Timeshift.ts.
      /// </summary>
      /// <param name="fileName">The name of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file name was successfully set</returns>
      [PreserveSig]
      Int32 SetFilename([MarshalAs(UnmanagedType.LPWStr)] String fileName);

      /// <summary>
      /// Get the name of the file configured for use for timeshifting.
      /// </summary>
      /// <param name="fileName">The name of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file name was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFilename([Out, MarshalAs(UnmanagedType.LPWStr)] out String fileName);

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
      /// Set the value of one of the timeshifting interface options.
      /// </summary>
      /// <param name="option">The option to set.</param>
      /// <param name="value">The option value to set.</param>
      /// <returns>an HRESULT indicating whether the option value was successfully set</returns>
      [PreserveSig]
      Int32 SetOption(B2c2PvrOption option, Int32 value);

      /// <summary>
      /// Get the value of one of the timeshifting interface options.
      /// </summary>
      /// <param name="option">The option to get.</param>
      /// <param name="value">The option value.</param>
      /// <returns>an HRESULT indicating whether the option value was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetOption(B2c2PvrOption option, [Out] out Int32 value);

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
      /// Get the playback marker position within the timeshifting file.
      /// </summary>
      /// <param name="filePosition">The playback marker position.</param>
      /// <returns>an HRESULT indicating whether the marker position was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetFilePosition([Out] out Int64 filePosition);

      /// <summary>
      /// Register a callback delegate that the interface can use to notify the application about critical
      /// playback/recording state changes.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      Int32 SetCallback(OnB2c2TimeShiftState callback);
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

      /// <summary>
      /// Set the network interface to use for multicast operations.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
      /// <returns>an HRESULT indicating whether the address was successfully set</returns>
      [PreserveSig]
      Int32 SetNetworkInterface([MarshalAs(UnmanagedType.LPStr)] String address);

      /// <summary>
      /// Get the network interface configured for use for multicast operations.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
      /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
      [PreserveSig]
      Int32 GetNetworkInterface([Out, MarshalAs(UnmanagedType.LPStr)] out String address);
    }

    #endregion

    #endregion

    #region structs

#pragma warning disable 0649, 0169
    // Some of these structs are used when marshaling from unmanaged to managed memory, like in ReadDeviceInfo().
    private struct TunerCapabilities
    {
      public B2c2TunerType TunerType;
      [MarshalAs(UnmanagedType.Bool)]
      public bool ConstellationSupported;         // Is SetModulation() supported?
      [MarshalAs(UnmanagedType.Bool)]
      public bool FecSupported;                   // Is SetFec() suppoted?
      public UInt32 MinTransponderFrequency;      // unit = kHz
      public UInt32 MaxTransponderFrequency;      // unit = kHz
      public UInt32 MinTunerFrequency;            // unit = kHz
      public UInt32 MaxTunerFrequency;            // unit = kHz
      public UInt32 MinSymbolRate;                // unit = Baud
      public UInt32 MaxSymbolRate;                // unit = Baud
      private UInt32 AutoSymbolRate;              // Obsolete		
      public B2c2PerformanceMonitoringCapability PerformanceMonitoringCapabilities;
      public UInt32 LockTime;                     // unit = ms
      public UInt32 KernelLockTime;               // unit = ms
      public B2c2AcquisionCapability AcquisitionCapabilities;
    }

    private struct MacAddress
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MacAddressLength)]
      public byte[] Address;
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
      public B2c2VideoAspectRatio AspectRatio;
      public B2c2VideoFrameRate FrameRate;
    }
#pragma warning restore 0649, 0169

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), ComVisible(true)]
    private struct DeviceInfo
    {
      public UInt32 DeviceId;
      public MacAddress MacAddress;
      private UInt16 Padding1;
      public B2c2TunerType TunerType;
      public B2c2BusType BusInterface;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsInUse;
      private byte Padding2;
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
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate UInt32 OnB2c2TsData(UInt16 pid, IntPtr data);

    /// <summary>
    /// Called by the AV interface when the interface wants to notify the controlling application about the
    /// video stream information.
    /// </summary>
    /// <param name="info">The video stream information.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate UInt32 OnB2c2VideoInfo(VideoInfo info);

    /// <summary>
    /// Called by the timeshifting interface when the interface needs to notify the controlling application
    /// about critical playback/recording status changes.
    /// </summary>
    /// <param name="state">The current timeshifting interface state.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate UInt32 OnB2c2TimeShiftState(B2c2PvrCallbackState state);

    #endregion

    #region constants

    private static readonly Guid B2c2AdapterClass = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x00, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);

    private const int MaxDeviceCount = 16;
    private const int MacAddressLength = 6;
    private const int MaxMacAddressCount = 32;
    private const int DeviceInfoSize = 416;
    private const int TunerCapabilitiesSize = 56;

    #endregion

    #region variables

    private IBaseFilter _filterB2c2Adapter = null;
    private IB2C2MPEG2DataCtrl6 _dataInterface = null;
    private IB2C2MPEG2TunerCtrl4 _tunerInterface = null;

    private DeviceInfo _deviceContext;
    private IntPtr _generalBuffer = IntPtr.Zero;

    // PID filter variables - especially important for DVB-S2 devices.
    private int _maxPidCount = 0;
    private HashSet<Int32> _filterPids = new HashSet<Int32>();

    #region DiSEqC

    /// <summary>
    /// The DiSEqC control interface for this device.
    /// </summary>
    private IDiseqcController _diseqcController = null;

    /// <summary>
    /// <c>True</c> if the device is capable of sending raw DiSEqC commands, otherwise <c>false</c>.
    /// </summary>
    private bool _isRawDiseqcSupported = false;

    /// <summary>
    /// Enable or disable always sending DiSEqC commands.
    /// </summary>
    /// <remarks>
    /// DiSEqC commands are usually only sent when changing to a channel on a different switch port or at a
    /// different positioner location. Enabling this option will cause DiSEqC commands to be sent on each
    /// channel change.
    /// </remarks>
    private bool _alwaysSendDiseqcCommands = false;

    /// <summary>
    /// The number of times to repeat DiSEqC commands.
    /// </summary>
    /// <remarks>
    /// When set to zero, commands are sent once; when set to one, commands are sent twice... etc.
    /// </remarks>
    private ushort _diseqcCommandRepeatCount = 0;

    #endregion

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardDvbSS2"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    /// <param name="context">The B2C2-specific device context (DeviceInfo) associated with this device.</param>
    public TvCardDvbSS2(IEpgEvents epgEvents, DsDevice device, object context)
      : base(epgEvents, device)
    {
      _deviceContext = (DeviceInfo)context;
      switch (_deviceContext.TunerType)
      {
        case B2c2TunerType.Satellite:
          _tunerType = CardType.DvbS;
          if (_devicePath != null)
          {
            Card c = CardManagement.GetCardByDevicePath(device.DevicePath, CardIncludeRelationEnum.None);
            if (c != null)
            {
              _alwaysSendDiseqcCommands = c.AlwaysSendDiseqcCommands;
              _diseqcCommandRepeatCount = (ushort)c.DiseqcCommandRepeatCount;
              if (_diseqcCommandRepeatCount > 5)
              {
                // It would be rare that commands would need to be repeated more than twice. Five times
                // is a more than reasonable practical limit.
                _diseqcCommandRepeatCount = 5;
              }
            }
          }
          break;
        case B2c2TunerType.Cable:
          _tunerType = CardType.DvbC;
          break;
        case B2c2TunerType.Terrestrial:
          _tunerType = CardType.DvbT;
          break;
        case B2c2TunerType.Atsc:
          _tunerType = CardType.Atsc;
          break;
        default:
          // The tuner may not be redetected properly after standby in some cases. In those cases, mark it as
          // "not present" and "unknown" so that TV Server won't use it until after a service restart.
          _tunerType = CardType.Unknown;
          _cardPresent = false;
          break;
      }

      _generalBuffer = Marshal.AllocCoTaskMem(TunerCapabilitiesSize);
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
        _signalPresent = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      int level, quality;
      _tunerLocked = (_tunerInterface.CheckLock() == 0);
      _tunerInterface.GetSignalStrength(out level);
      _tunerInterface.GetSignalQuality(out quality);
      if (level < 0)
      {
        level = 0;
      }
      else if (level > 100)
      {
        level = 100;
      }
      if (quality < 0)
      {
        quality = 0;
      }
      else if (quality > 100)
      {
        quality = 100;
      }
      _signalQuality = quality;
      _signalLevel = level;
      _lastSignalUpdate = DateTime.Now;
    }

    #region tuning & scanning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (_tunerType == CardType.DvbS)
      {
        if (channel is DVBSChannel)
        {
          return true;
        }
      }
      else if (_tunerType == CardType.DvbT)
      {
        if (channel is DVBTChannel)
        {
          return true;
        }
      }
      else if (_tunerType == CardType.DvbC)
      {
        if (channel is DVBCChannel)
        {
          return true;
        }
      }
      else if (_tunerType == CardType.Atsc)
      {
        if (channel is ATSCChannel)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      Log.Debug("TvCardDvbSs2: set tuning parameters");
      bool result = false;
      switch (_tunerType)
      {
        case CardType.DvbS:
          result = PerformTuneDvbS(channel);
          break;
        case CardType.DvbT:
          result = PerformTuneDvbT(channel);
          break;
        case CardType.DvbC:
          result = PerformTuneDvbC(channel);
          break;
        case CardType.Atsc:
          result = PerformTuneAtsc(channel);
          break;
      }

      if (!result)
      {
        throw new TvException("TvCardDvbSs2: failed to set tuning parameters");
      }

      Log.Debug("TvCardDvbSs2: apply tuning parameters");
      int hr = _tunerInterface.SetTunerStatus();
      if (hr != 0)
      {
        throw new TvException("TvCardDvbSs2: failed to apply tuning parameters");
      }
    }

    private bool PerformTuneDvbS(IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        Log.Debug("TvCardDvbSs2: channel is not a DVB-S channel!!! {0}", channel.GetType().ToString());
        return false;
      }

      int hr = _tunerInterface.SetFrequency((Int32)dvbsChannel.Frequency / 1000);
      if (hr != 0)
      {
        Log.Error("TvCardDvbSs2: failed to set frequency, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      hr = _tunerInterface.SetSymbolRate(dvbsChannel.SymbolRate);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set symbol rate, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      B2c2FecRate fec = B2c2FecRate.Auto;
      switch (dvbsChannel.InnerFecRate)
      {
        case BinaryConvolutionCodeRate.Rate1_2:
          fec = B2c2FecRate.Rate1_2;
          break;
        case BinaryConvolutionCodeRate.Rate2_3:
          fec = B2c2FecRate.Rate2_3;
          break;
        case BinaryConvolutionCodeRate.Rate3_4:
          fec = B2c2FecRate.Rate3_4;
          break;
        case BinaryConvolutionCodeRate.Rate5_6:
          fec = B2c2FecRate.Rate5_6;
          break;
        case BinaryConvolutionCodeRate.Rate7_8:
          fec = B2c2FecRate.Rate7_8;
          break;
      }
      hr = _tunerInterface.SetFec(fec);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set FEC rate, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      B2c2Polarisation b2c2Polarisation = B2c2Polarisation.Horizontal;
      if (dvbsChannel.Polarisation == Polarisation.LinearV || dvbsChannel.Polarisation == Polarisation.CircularR)
      {
        b2c2Polarisation = B2c2Polarisation.Vertical;
      }
      hr = _tunerInterface.SetPolarity(b2c2Polarisation);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set polarisation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      _diseqcController.SwitchToChannel(dvbsChannel);

      if (dvbsChannel.Frequency > dvbsChannel.LnbType.SwitchFrequency)
      {
        hr = _tunerInterface.SetLnbFrequency(dvbsChannel.LnbType.HighBandFrequency / 1000);
      }
      else
      {
        hr = _tunerInterface.SetLnbFrequency(dvbsChannel.LnbType.LowBandFrequency / 1000);
      }
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set LNB LOF frequency, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      return true;
    }

    private bool PerformTuneDvbT(IChannel channel)
    {
      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel == null)
      {
        Log.Debug("TvCardDvbSs2: channel is not a DVB-T channel!!! {0}", channel.GetType().ToString());
        return false;
      }

      int hr = _tunerInterface.SetFrequency((Int32)dvbtChannel.Frequency / 1000);
      if (hr != 0)
      {
        Log.Error("TvCardDvbSs2: failed to set frequency, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      hr = _tunerInterface.SetBandwidth(dvbtChannel.BandWidth);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set bandwidth, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Note: it is not guaranteed that guard interval auto detection is supported, but if it isn't
      // then we can't tune - we have no idea what the actual value should be.
      hr = _tunerInterface.SetGuardInterval(B2c2GuardInterval.Auto);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to use automatic guard interval detection, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      return true;
    }

    private bool PerformTuneDvbC(IChannel channel)
    {
      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        Log.Debug("TvCardDvbSs2: channel is not a DVB-C channel!!! {0}", channel.GetType().ToString());
        return false;
      }

      int hr = _tunerInterface.SetFrequency((Int32)dvbcChannel.Frequency / 1000);
      if (hr != 0)
      {
        Log.Error("TvCardDvbSs2: failed to set frequency, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      hr = _tunerInterface.SetSymbolRate(dvbcChannel.SymbolRate);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set symbol rate, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      B2c2Modulation modulation = B2c2Modulation.Qam64;
      switch (dvbcChannel.ModulationType)
      {
        case ModulationType.Mod16Qam:
          modulation = B2c2Modulation.Qam16;
          break;
        case ModulationType.Mod32Qam:
          modulation = B2c2Modulation.Qam32;
          break;
        case ModulationType.Mod128Qam:
          modulation = B2c2Modulation.Qam128;
          break;
        case ModulationType.Mod256Qam:
          modulation = B2c2Modulation.Qam256;
          break;
      }
      hr = _tunerInterface.SetModulation(modulation);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      return true;
    }

    private bool PerformTuneAtsc(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        Log.Debug("TvCardDvbSs2: channel is not an ATSC channel!!! {0}", channel.GetType().ToString());
        return false;
      }

      // If the channel modulation scheme is 8 VSB then it is an over-the-air ATSC channel, otherwise
      // it is a cable (QAM annex B) channel.
      int frequency;
      B2c2Modulation modulation = B2c2Modulation.Vsb8;
      if (atscChannel.ModulationType == ModulationType.Mod8Vsb)
      {
        // We normally tune ATSC channels by physical channel number, however we have to tune them by
        // frequency with the SkyStar tuners. This code is a conversion between channel number and
        // frequency (in MHz), probably for the US only.
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
        Log.Debug("TvCardDvbSs2: translated ATSC physical channel number {0} to {1} MHz", atscChannel.PhysicalChannel, frequency);
      }
      else
      {
        frequency = (Int32)atscChannel.Frequency / 1000;
        modulation = B2c2Modulation.Qam256AnnexB;
        if (atscChannel.ModulationType == ModulationType.Mod64Qam)
        {
          modulation = B2c2Modulation.Qam64AnnexB;
        }
      }

      int hr = _tunerInterface.SetFrequency(frequency);
      if (hr != 0)
      {
        Log.Error("TvCardDvbSs2: failed to set frequency, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      hr = _tunerInterface.SetModulation(modulation);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to set modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      return true;
    }

    /// <summary>
    /// Get the device's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        if (_tunerType == CardType.Atsc)
        {
          return new ATSCScanning(this);
        }
        return new DvbBaseScanning(this);
      }
    }

    #endregion

    #region graph building

    /// <summary>
    /// Build the DirectShow filter graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        Log.Debug("TvCardDvbSs2: build graph");
        if (_isDeviceInitialised)
        {
          Log.Error("TvCardDvbSs2: the graph is already built");
          throw new TvException("The graph is already built.");
        }
        if (_tunerType == CardType.Unknown || !_cardPresent)
        {
          Log.Error("TvCardDvbSs2: device is not present, driver restart required");
          throw new TvExceptionGraphBuildingFailed("TvCardDvbSs2: device is not present, driver restart required");
        }

        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);

        DevicesInUse.Instance.Add(_tunerDevice);
        try
        {
          // Create, add and initialise the B2C2 source filter.
          Log.Debug("TvCardDvbSs2: create B2C2 source filter");
          _filterB2c2Adapter = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(B2c2AdapterClass, false));
          if (_filterB2c2Adapter == null)
          {
            Log.Error("TvCardDvbSs2: failed to create B2C2 source filter");
            return;
          }
          Log.Debug("TvCardDvbSs2: add source filter to graph");
          int hr = _graphBuilder.AddFilter(_filterB2c2Adapter, "B2C2-Source");
          if (hr != 0)
          {
            Log.Error("TvCardDvbSs2: failed to add source filter to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return;
          }
          Log.Debug("TvCardDvbSs2: get device interface handles");
          _dataInterface = _filterB2c2Adapter as IB2C2MPEG2DataCtrl6;
          _tunerInterface = _filterB2c2Adapter as IB2C2MPEG2TunerCtrl4;
          if (_tunerInterface == null || _dataInterface == null)
          {
            Log.Error("TvCardDvbSs2: failed to get device interface handles");
            return;
          }
          Log.Debug("TvCardDvbSs2: initialise tuner interface");
          hr = _tunerInterface.Initialize();
          if (hr != 0)
          {
            Log.Error("TvCardDvbSs2: failed to initialise tuner interface, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return;
          }
          // This line is a remnant from old code. I don't know if/why it is necessary, but no harm
          // in leaving it...
          _tunerInterface.CheckLock();

          // The source filter has multiple output pins, and connecting to the right one is critical.
          // Plugins can't handle this automatically, so we add an extra infinite tee in between the source
          // filter and any plugin filters.
          IBaseFilter lastFilter = _filterB2c2Adapter;
          AddInfiniteTeeToGraph(ref lastFilter);

          // Load and open plugins.
          LoadPlugins(_filterB2c2Adapter, ref lastFilter);
          // This class implements the plugin interface and should be considered as the main plugin.
          _customDeviceInterfaces.Add(this);

          // Complete the graph.
          AddTsWriterFilterToGraph();
          ConnectTsWriterIntoGraph(lastFilter);
          _isDeviceInitialised = true;
        }
        finally
        {
          if (!_isDeviceInitialised)
          {
            DevicesInUse.Instance.Remove(_tunerDevice);
          }
        }

        OpenPlugins();
        SetFilterPids(new HashSet<UInt16>(), ModulationType.ModNotSet, false);
        _diseqcController = new DiseqcController(this, _alwaysSendDiseqcCommands, _diseqcCommandRepeatCount);

        // Plugins can request to pause or start the device - other actions don't make sense here. The running
        // state is considered more compatible than the paused state, so start takes precedence.
        DeviceAction actualAction = DeviceAction.Default;
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          DeviceAction action;
          deviceInterface.OnInitialised(this, out action);
          if (action == DeviceAction.Pause)
          {
            if (actualAction == DeviceAction.Default)
            {
              Log.Debug("TvCardDvbBase: plugin \"{0}\" will cause device pause", deviceInterface.Name);
              actualAction = DeviceAction.Pause;
            }
            else
            {
              Log.Debug("TvCardDvbBase: plugin \"{0}\" wants to pause the device, overriden", deviceInterface.Name);
            }
          }
          else if (action == DeviceAction.Start)
          {
            Log.Debug("TvCardDvbBase: plugin \"{0}\" will cause device start", deviceInterface.Name);
            actualAction = action;
          }
          else if (action != DeviceAction.Default)
          {
            Log.Debug("TvCardDvbBase: plugin \"{0}\" wants unsupported action {1}", deviceInterface.Name, action);
          }
        }
        if (actualAction == DeviceAction.Start || _idleMode == DeviceIdleMode.AlwaysOn)
        {
          SetGraphState(FilterState.Running);
        }
        else if (actualAction == DeviceAction.Pause)
        {
          SetGraphState(FilterState.Paused);
        }

        ReadDeviceInfo();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        Dispose();
        _isDeviceInitialised = false;
        throw new TvExceptionGraphBuildingFailed("Graph building failed", ex);
      }
    }

    /// <summary>
    /// Add and connect an infinite tee into the BDA filter graph.
    /// </summary>
    /// <param name="lastFilter">The filter in the filter chain that the infinite tee should be connected to.</param>
    protected override void AddInfiniteTeeToGraph(ref IBaseFilter lastFilter)
    {
      Log.Debug("TvCardDvbSs2: add infinite tee filter");
      _infTee = (IBaseFilter)new InfTee();
      int hr = _graphBuilder.AddFilter(_infTee, "Infinite Tee");
      if (hr != 0)
      {
        Log.Error("TvCardDvbSs2: failed to add infinite tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbSs2: failed to add infinite tee");
      }

      Log.Debug("TvCardDvbSs2: connect infinite tee filter");
      IPin infTeeIn = DsFindPin.ByDirection(_infTee, PinDirection.Input, 0);
      IPin sourceOut = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 2); // Note: pin number is important!
      hr = _graphBuilder.Connect(sourceOut, infTeeIn);
      Release.ComObject("Infinite tee input pin", infTeeIn);
      Release.ComObject("MPEG 2 demux input pin", sourceOut);
      if (hr != 0)
      {
        Log.Error("TvCardDvbSs2: failed to connect infinite tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbSs2: failed to connect infinite tee");
      }
      lastFilter = _infTee;
    }

    #endregion

    /// <summary>
    /// Stop the device. The actual result of this function depends on device configuration:
    /// </summary>
    public override void Stop()
    {
      base.Stop();
      // Force the DiSEqC controller to forget the previously tuned channel. This guarantees that the
      // next call to SwitchToChannel() will actually cause commands to be sent.
      if (_diseqcController != null)
      {
        _diseqcController.SwitchToChannel(null);
      }
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public override void Dispose()
    {
      if (_graphBuilder == null)
      {
        return;
      }

      Log.WriteFile("TvCardDvbSs2: dispose");

      base.Dispose();

      _dataInterface = null;
      _tunerInterface = null;
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }

      if (_filterB2c2Adapter != null)
      {
        Release.ComObject("B2C2 source filter", _filterB2c2Adapter);
        _filterB2c2Adapter = null;
      }
    }

    /// <summary>
    /// Get the device's DiSEqC control interface. This interface is only applicable for satellite tuners.
    /// It is used for controlling switch, positioner and LNB settings.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner does not support sending/receiving
    /// DiSEqC commands</value>
    public override IDiseqcController DiseqcController
    {
      get { return _diseqcController; }
    }

    /// <summary>
    /// Detect the compatible devices installed in the system.
    /// </summary>
    /// <returns>an array of device contexts if one or more devices are detected, otherwise <c>null</c>; the
    ///   caller can instantiate one device per returned array element, passing the element as a parameter to
    ///   the constructor for each device instance</returns>
    public static object[] DetectDevices()
    {
      Log.Debug("TvCardDvbSs2: detect devices");
      object[] contexts = null;

      // Instanciate a data interface so we can check how many tuners are installed.
      IBaseFilter b2c2Source = null;
      try
      {
        b2c2Source = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(B2c2AdapterClass, false));
      }
      catch (Exception ex)
      {
        Log.Debug("TvCardDvbSs2: failed to instanciate source filter\r\n{0}", ex.ToString());
      }
      IB2C2MPEG2DataCtrl6 dataInterface = b2c2Source as IB2C2MPEG2DataCtrl6;
      if (dataInterface == null)
      {
        Log.Debug("TvCardDvbSs2: failed to get B2C2 data interface handle");
        Release.ComObject("B2C2 Source Filter", b2c2Source);
        return contexts;
      }

      // Get device details...
      int size = DeviceInfoSize * MaxDeviceCount;
      int deviceCount = MaxDeviceCount;
      IntPtr tempBuffer = Marshal.AllocCoTaskMem(size);
      for (int i = 0; i < size; i++)
      {
        Marshal.WriteByte(tempBuffer, i, 0);
      }
      int hr = dataInterface.GetDeviceList(tempBuffer, ref size, ref deviceCount);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to get device list, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        //DVB_MMI.DumpBinary(tempBuffer, 0, size);
        Log.Debug("TvCardDvbSs2: device count = {0}", deviceCount);
        Int64 structurePtr = tempBuffer.ToInt64();
        contexts = new object[deviceCount];
        for (int i = 0; i < deviceCount; i++)
        {
          Log.Debug("TvCardDvbSs2: device {0}", i + 1);
          DeviceInfo d = (DeviceInfo)Marshal.PtrToStructure(new IntPtr(structurePtr), typeof(DeviceInfo));
          Log.Debug("  device ID           = {0}", d.DeviceId);
          Log.Debug("  MAC address         = {0}", BitConverter.ToString(d.MacAddress.Address).ToLowerInvariant());
          Log.Debug("  tuner type          = {0}", d.TunerType);
          Log.Debug("  bus interface       = {0}", d.BusInterface);
          Log.Debug("  is in use?          = {0}", d.IsInUse);
          Log.Debug("  product ID          = {0}", d.ProductId);
          Log.Debug("  product name        = {0}", d.ProductName);
          Log.Debug("  product description = {0}", d.ProductDescription);
          Log.Debug("  product revision    = {0}", d.ProductRevision);
          Log.Debug("  product front end   = {0}", d.ProductFrontEnd);
          structurePtr += DeviceInfoSize;
          contexts[i] = d;
        }
        Log.Debug("TvCardDvbSs2: result = success");
      }

      // Clean up...
      Marshal.FreeCoTaskMem(tempBuffer);
      Release.ComObject("B2C2 Source Filter", b2c2Source);

      return contexts;
    }

    /// <summary>
    /// Attempt to read the device information from the device.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Debug("TvCardDvbSs2: read device information");

      int hr = _dataInterface.SelectDevice(_deviceContext.DeviceId);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to select device, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Log.Debug("TvCardDvbSs2: reading capabilities");
      for (int i = 0; i < TunerCapabilitiesSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount = TunerCapabilitiesSize;
      hr = _tunerInterface.GetTunerCapabilities(_generalBuffer, ref returnedByteCount);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        //DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);
        TunerCapabilities capabilities = (TunerCapabilities)Marshal.PtrToStructure(_generalBuffer, typeof(TunerCapabilities));
        Log.Debug("  tuner type                = {0}", capabilities.TunerType);
        Log.Debug("  set constellation?        = {0}", capabilities.ConstellationSupported);
        Log.Debug("  set FEC rate?             = {0}", capabilities.FecSupported);
        Log.Debug("  min transponder frequency = {0} kHz", capabilities.MinTransponderFrequency);
        Log.Debug("  max transponder frequency = {0} kHz", capabilities.MaxTransponderFrequency);
        Log.Debug("  min tuner frequency       = {0} kHz", capabilities.MinTunerFrequency);
        Log.Debug("  max tuner frequency       = {0} kHz", capabilities.MaxTunerFrequency);
        Log.Debug("  min symbol rate           = {0} baud", capabilities.MinSymbolRate);
        Log.Debug("  max symbol rate           = {0} baud", capabilities.MaxSymbolRate);
        Log.Debug("  performance monitoring    = {0}", capabilities.PerformanceMonitoringCapabilities.ToString());
        Log.Debug("  lock time                 = {0} ms", capabilities.LockTime);
        Log.Debug("  max symbol rate           = {0} ms", capabilities.KernelLockTime);
        Log.Debug("  acquisition capabilities  = {0}", capabilities.AcquisitionCapabilities);
        _isRawDiseqcSupported = (capabilities.AcquisitionCapabilities & B2c2AcquisionCapability.RawDiseqc) != 0;
      }

      Log.Debug("TvCardDvbSs2: reading max PID counts");
      int count = 0;
      hr = _dataInterface.GetMaxGlobalPIDCount(out count);
      if (hr == 0)
      {
        Log.Debug("  global max                = {0}", count);
      }
      else
      {
        Log.Debug("TvCardDvbSs2: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      hr = _dataInterface.GetMaxIpPIDCount(out count);
      if (hr == 0)
      {
        Log.Debug("  IP max                    = {0}", count);
      }
      else
      {
        Log.Debug("TvCardDvbSs2: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      hr = _dataInterface.GetMaxPIDCount(out count);
      if (hr == 0)
      {
        Log.Debug("  TS max                    = {0}", count);
        _maxPidCount = count;
      }
      else
      {
        Log.Debug("TvCardDvbSs2: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
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
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked when device initialisation is complete.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnInitialised(ITVCard tuner, out DeviceAction action)
    {
      action = DeviceAction.Default;
    }

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      action = DeviceAction.Default;
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted, when the device is running but before
    /// signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnRunning(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked before the device is stopped.
    /// </summary>
    /// <param name="tuner">The device instance that this device instance is associated with.</param>
    /// <param name="action">As an input, the action that TV Server wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref DeviceAction action)
    {
    }

    #endregion

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
      Log.Debug("TvCardDvbSs2: set PID filter PIDs, modulation = {0}, force enable = {1}", modulation, forceEnable);
      bool fullTransponder = true;
      bool success = true;

      int hr = _dataInterface.SelectDevice(_deviceContext.DeviceId);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to select device, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      if (pids == null || pids.Count == 0 || (modulation != ModulationType.ModQpsk && modulation != ModulationType.Mod8Psk && !forceEnable))
      {
        Log.Debug("TvCardDvbSs2: disabling PID filter");
      }
      else
      {
        // If we get to here then the default approach is to enable the filter, but
        // there is one other constraint that applies: the filter PID limit.
        fullTransponder = false;
        if (pids.Count > _maxPidCount)
        {
          Log.Debug("TvCardDvbSs2: too many PIDs, hardware limit = {0}, actual count = {1}", _maxPidCount, pids.Count);
          // When the forceEnable flag is set, we just set as many PIDs as possible.
          if (!forceEnable)
          {
            Log.Debug("TvCardDvbSs2: disabling PID filter");
            fullTransponder = true;
          }
        }

        if (!fullTransponder)
        {
          Log.Debug("TvCardDvbSs2: enabling PID filter");
          fullTransponder = false;
        }
      }

      int openPidCount;
      int runningPidCount;
      int totalPidCount = _maxPidCount;
      int[] currentPids = new int[_maxPidCount];
      int availablePidCount = 0;
      int changingPidCount = 1;
      hr = _dataInterface.GetTsState(out openPidCount, out runningPidCount, ref totalPidCount, currentPids);
      if (hr != 0)
      {
        Log.Debug("TvCardDvbSs2: failed to retrieve current PIDs, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        fullTransponder = true;
        success = false;
      }
      else
      {
        Log.Debug("TvCardDvbSs2: current PID details (before update)");
        Log.Debug("  open count     = {0}", openPidCount);
        Log.Debug("  running count  = {0}", runningPidCount);
        Log.Debug("  returned count = {0}", totalPidCount);
        if (currentPids != null)
        {
          for (int i = 0; i < totalPidCount; i++)
          {
            Log.Debug("  {0,-2}             = {1} (0x{1:x})", i + 1, currentPids[i]);
          }
        }
        availablePidCount = _maxPidCount - totalPidCount;
      }

      if (!fullTransponder)
      {
        // Remove the PIDs that are no longer needed.
        for (byte i = 0; i < totalPidCount; i++)
        {
          if (currentPids != null && !pids.Contains((UInt16)currentPids[i]))
          {
            hr = _dataInterface.DeletePIDsFromPin(1, new int[1] { currentPids[i] }, 0);
            if (hr != 0)
            {
              Log.Debug("TvCardDvbSs2: failed to remove PID {0} (0x{0:x}), hr = 0x{1:x} ({2})", currentPids[i], hr, HResult.GetDXErrorString(hr));
              success = false;
            }
            else
            {
              Log.Debug("  delete PID {0} (0x{0:x})...", currentPids[i]);
              availablePidCount++;
            }
          }
        }
        // Add the new PIDs.
        if (pids != null)
        {
          HashSet<Int32> currentPidsAsHash = new HashSet<Int32>(currentPids);
          HashSet<UInt16>.Enumerator en = pids.GetEnumerator();
          while (en.MoveNext() && availablePidCount > 0)
          {
            if (currentPidsAsHash != null && !currentPidsAsHash.Contains(en.Current))
            {
              hr = _dataInterface.AddPIDsToPin(ref changingPidCount, new int[1] { en.Current }, 0);
              if (hr != 0)
              {
                Log.Debug("TvCardDvbSs2: failed to add PID {0} (0x{0:x}), hr = 0x{1:x} ({2})", en.Current, hr, HResult.GetDXErrorString(hr));
                success = false;
              }
              else
              {
                Log.Debug("  add PID {0} (0x{0:x})...", en.Current);
                availablePidCount--;
              }
            }
          }
        }
      }
      else
      {
        // Remove all current PIDs.
        if (currentPids != null)
        {
          for (byte i = 0; i < totalPidCount; i++)
          {
            hr = _dataInterface.DeletePIDsFromPin(1, new int[1] { currentPids[i] }, 0);
            if (hr != 0)
            {
              Log.Debug("TvCardDvbSs2: failed to remove PID {0} (0x{0:x}), hr = 0x{1:x} ({2})", currentPids[i], hr, HResult.GetDXErrorString(hr));
              success = false;
            }
            else
            {
              Log.Debug("  delete all PIDs...");
            }
          }
        }
        // Allow all PIDs.
        hr = _dataInterface.AddPIDsToPin(ref changingPidCount, new int[1] { (int)B2c2PidFilterMode.AllExcludingNull }, 0);
        if (hr != 0)
        {
          Log.Debug("TvCardDvbSs2: failed to enable all PIDs, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
        else
        {
          Log.Debug("  allow all excluding NULL...");
        }
      }

      if (success)
      {
        Log.Debug("TvCardDvbSs2: updates complete, result = success");
      }

      return success;
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
      Log.Debug("TvCardDvbSs2: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);
      if (_tunerInterface == null)
      {
        Log.Debug("TvCardDvbSs2: device not initialised or interface not supported");
      }

      bool success = true;
      int hr = 0;

      B2c2DiseqcPort burst = B2c2DiseqcPort.None;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        burst = B2c2DiseqcPort.SimpleA;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        burst = B2c2DiseqcPort.SimpleB;
      }
      if (burst != B2c2DiseqcPort.None)
      {
        hr = _tunerInterface.SetDiseqc(burst);
        if (hr == 0)
        {
          Log.Debug("TvCardDvbSs2: burst result = success");
        }
        else
        {
          Log.Debug("TvCardDvbSs2: burst result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      B2c2Tone tone = B2c2Tone.Off;
      if (tone22kState == Tone22k.On)
      {
        tone = B2c2Tone.Tone22k;
      }
      hr = _tunerInterface.SetLnbKHz(tone);
      if (hr == 0)
      {
        Log.Debug("TvCardDvbSs2: 22 kHz result = success");
      }
      else
      {
        Log.Debug("TvCardDvbSs2: 22 kHz result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      Log.Debug("TvCardDvbSs2: send DiSEqC command");

      if (_tunerInterface == null)
      {
        Log.Debug("TvCardDvbSs2: device not initialised or interface not supported");
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("TvCardDvbSs2: command not supplied");
        return true;
      }

      Marshal.Copy(command, 0, _generalBuffer, command.Length);
      int hr = 0;
      if (_isRawDiseqcSupported)
      {
        try
        {
          hr = _tunerInterface.SendDiSEqCCommand(command.Length, _generalBuffer);
          if (hr == 0)
          {
            Log.Debug("TvCardDvbSs2: result = success");
            return true;
          }
        }
        catch (COMException ex)
        {
          if ((B2c2Error)ex.ErrorCode == B2c2Error.Diseqc12NotSupported)
          {
            // DiSEqC 1.2 commands not supported. This is a little unexpected given that the
            // driver previously reported that it supports them.
            Log.Debug("TvCardDvbSs2: raw DiSEqC commands not supported");
          }
          else
          {
            Log.Debug("TvCardDvbSs2: failed to send raw DiSEqC command, hr = 0x{0:x}\r\n{1}", ex.ErrorCode, ex.StackTrace);
          }
        }
      }

      // If we get to here then the driver/hardware doesn't support raw commands. We'll attempt to send
      // non-raw commands if the command is a DiSEqC 1.0 switch command.
      if (command.Length != 4 ||
        (command[0] != (byte)DiseqcFrame.CommandFirstTransmissionNoReply &&
        command[0] != (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) ||
        command[1] != (byte)DiseqcAddress.AnySwitch ||
        command[2] != (byte)DiseqcCommand.WriteN0)
      {
        Log.Debug("TvCardDvbSs2: command not supported");
        return false;
      }

      // Port A = 3, Port B = 4 etc.
      B2c2DiseqcPort port = (B2c2DiseqcPort)((command[3] & 0xc) >> 2) + 3;
      hr = _tunerInterface.SetDiseqc(port);
      if (hr == 0)
      {
        Log.Debug("TvCardDvbSs2: result = success");
        return true;
      }

      Log.Debug("TvCardDvbSs2: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
  }
}