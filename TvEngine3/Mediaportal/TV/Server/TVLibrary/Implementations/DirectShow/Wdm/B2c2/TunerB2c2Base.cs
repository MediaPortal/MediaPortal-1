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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat tuners with B2C2 chipsets and WDM drivers.
  /// </summary>
  public abstract class TunerB2c2Base : TunerDirectShowBase, ICustomDevice, IMpeg2PidFilter
  {
    #pragma warning disable 1591
    #region enums

    protected enum B2c2Error
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

    public enum B2c2TunerType
    {
      Unknown = -1,
      Satellite,
      Cable,
      Terrestrial,
      Atsc
    }

    public enum B2c2BusType
    {
      Pci = 0,
      Usb1 = 1    // USB 1.1
    }

    [Flags]
    protected enum B2c2PerformanceMonitoringCapability
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
    protected enum B2c2AcquisionCapability
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

    protected enum B2c2Modulation
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

    protected enum B2c2Polarisation
    {
      Horizontal = 0,   // 18 V - also use for circular left.
      Vertical,         // 13 V - also use for circular right.
      PowerOff = 10     // Turn the power supply to the LNB completely off.
    }

    protected enum B2c2FecRate
    {
      Rate1_2 = 1,
      Rate2_3,
      Rate3_4,
      Rate5_6,
      Rate7_8,
      Auto
    }

    protected enum B2c2Tone
    {
      Off = 0,
      Tone22k,
      Tone33k,
      Tone44k
    }

    protected enum B2c2DiseqcPort
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

    protected enum B2c2GuardInterval
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

    [ComVisible(false), ComImport,
      Guid("61a9051f-04c4-435e-8742-9edd2c543ce9"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    protected interface IB2c2Mpeg2TunerCtrl4
    {
      #region IB2c2Mpeg2TunerCtrl

      #region setters

      /// <summary>
      /// Set the multiplex frequency.
      /// </summary>
      /// <param name="frequency">The frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the frequency was successfully set</returns>
      [PreserveSig]
      int SetFrequency(int frequency);

      /// <summary>
      /// Set the multiplex symbol rate. Only applicable for DVB-S and DVB-C tuners.
      /// </summary>
      /// <param name="symbolRate">The symbol rate in ks/s.</param>
      /// <returns>an HRESULT indicating whether the symbol rate was successfully set</returns>
      [PreserveSig]
      int SetSymbolRate(int symbolRate);

      /// <summary>
      /// Set the LNB local oscillator frequency. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="lnbFrequency">The local oscillator frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the local oscillator frequency was successfully set</returns>
      [PreserveSig]
      int SetLnbFrequency(int lnbFrequency);

      /// <summary>
      /// Set the multiplex FEC rate. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="fecRate">The FEC rate.</param>
      /// <returns>an HRESULT indicating whether the FEC rate was successfully set</returns>
      [PreserveSig]
      int SetFec(B2c2FecRate fecRate);

      /// <summary>
      /// Set the multiplex polarisation. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="polarisation">The polarisation.</param>
      /// <returns>an HRESULT indicating whether the polarisation was successfully set</returns>
      [PreserveSig]
      int SetPolarity(B2c2Polarisation polarisation);

      /// <summary>
      /// Set the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="toneState">The tone state.</param>
      /// <returns>an HRESULT indicating whether the tone state was successfully set</returns>
      [PreserveSig]
      int SetLnbKHz(B2c2Tone toneState);

      /// <summary>
      /// Switch to a specific satellite. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
      /// <returns>an HRESULT indicating whether the DiSEqC command was successfully sent</returns>
      [PreserveSig]
      int SetDiseqc(B2c2DiseqcPort diseqcPort);

      /// <summary>
      /// Set the multiplex modulation scheme. Only applicable for DVB-C tuners.
      /// </summary>
      /// <param name="modulation">The modulation scheme.</param>
      /// <returns>an HRESULT indicating whether the modulation scheme was successfully set</returns>
      [PreserveSig]
      int SetModulation(B2c2Modulation modulation);

      #endregion

      /// <summary>
      /// Initialise the tuner control interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the interface was successfully initialised</returns>
      [PreserveSig]
      int Initialize();

      /// <summary>
      /// Apply previously set tuning parameter values.
      /// </summary>
      /// <returns>an HRESULT indicating whether the tuner is locked on signal</returns>
      [PreserveSig]
      int SetTunerStatus();

      /// <summary>
      /// Check the lock status of the tuner.
      /// </summary>
      /// <returns>an HRESULT indicating whether the tuner is locked or not</returns>
      [PreserveSig]
      int CheckLock();

      /// <summary>
      /// Get the tuner capabilities.
      /// </summary>
      /// <param name="capabilities">A pointer to a tuner capabilities structure.</param>
      /// <param name="size">The size of the capabilities structure.</param>
      /// <returns>an HRESULT indicating whether the capabilities were successfully retrieved</returns>
      [PreserveSig]
      int GetTunerCapabilities(IntPtr capabilities, [In, Out] ref int size);

      #region getters

      /// <summary>
      /// Get the tuned multiplex frequency. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="frequency">The multiplex frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the multiplex frequency was successfully retrieved</returns>
      [PreserveSig]
      int GetFrequency([Out] out int frequency);

      /// <summary>
      /// Get the tuned multiplex symbol rate in ks/s. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="symbolRate">The multiplex symbol rate in ks/s.</param>
      /// <returns>an HRESULT indicating whether the multiplex symbol rate was successfully retrieved</returns>
      [PreserveSig]
      int GetSymbolRate([Out] out int symbolRate);

      /// <summary>
      /// Get the modulation scheme for the tuned multiplex. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="modulation">The multiplex modulation scheme.</param>
      /// <returns>an HRESULT indicating whether the multiplex modulation scheme was successfully retrieved</returns>
      [PreserveSig]
      int GetModulation([Out] out B2c2Modulation modulation);

      #endregion

      #region signal strength/quality metrics

      /// <summary>
      /// Get the tuner/demodulator signal strength statistic.
      /// </summary>
      /// <param name="signalStrength">The signal strength as a percentage.</param>
      /// <returns>an HRESULT indicating whether the signal strength was successfully retrieved</returns>
      [PreserveSig]
      int GetSignalStrength([Out] out int signalStrength);

      /// <summary>
      /// Obsolete. Use GetSignalStrength() or GetSignalQuality() instead.
      /// </summary>
      /// <param name="signalLevel">The signal level in dBm.</param>
      /// <returns>an HRESULT indicating whether the signal level was successfully retrieved</returns>
      [PreserveSig]
      int GetSignalLevel([Out] out float signalLevel);

      /// <summary>
      /// Get the tuner/demodulator signal to noise ratio (SNR) statistic. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="snr">The signal to noise ratio.</param>
      /// <returns>an HRESULT indicating whether the signal to noise ratio was successfully retrieved</returns>
      [PreserveSig]
      int GetSNR([Out] out float snr);

      /// <summary>
      /// Get the pre-error-correction bit error rate (BER) statistic. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="ber">The bit error rate.</param>
      /// <param name="wait">(Not used.)</param>
      /// <returns>an HRESULT indicating whether the bit error rate was successfully retrieved</returns>
      [PreserveSig]
      int GetPreErrorCorrectionBER([Out] out float ber, bool wait);

      /// <summary>
      /// Get the uncorrected block count since the last call to this function. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      int GetUncorrectedBlocks([Out] out int blockCount);

      /// <summary>
      /// Get the total block count since the last call to this function. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      int GetTotalBlocks([Out] out int blockCount);

      #endregion

      /// <summary>
      /// Get the tuned channel number. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="channel">The channel number.</param>
      /// <returns>an HRESULT indicating whether the channel number was successfully retrieved</returns>
      [PreserveSig]
      int GetChannel([Out] out int channel);

      /// <summary>
      /// Set the tuner channel number. Only applicable for ATSC tuners.
      /// </summary>
      /// <param name="channel">The channel number.</param>
      /// <returns>an HRESULT indicating whether the channel number was successfully set</returns>
      [PreserveSig]
      int SetChannel(int channel);

      #endregion

      #region IB2c2Mpeg2TunerCtrl2

      /// <summary>
      /// Apply previously set tuning parameter values.
      /// </summary>
      /// <param name="lockCheckCount">The number of times the interface should check for lock after applying
      ///   the tuning parameter values. The delay between checks is 50 ms.</param>
      /// <returns>an HRESULT indicating whether the tuner is locked on signal</returns>
      [PreserveSig]
      int SetTunerStatusEx(int lockCheckCount);

      #region get/set tuning parameters

      /// <summary>
      /// Set the multiplex frequency.
      /// </summary>
      /// <param name="frequency">The frequency in kHz.</param>
      /// <returns>an HRESULT indicating whether the frequency was successfully set</returns>
      [PreserveSig]
      int SetFrequencyKHz(int frequency);

      /// <summary>
      /// Set the multiplex guard interval. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="interval">The guard interval.</param>
      /// <returns>an HRESULT indicating whether the guard interval was successfully set</returns>
      [PreserveSig]
      int SetGuardInterval(B2c2GuardInterval interval);

      /// <summary>
      /// Get the multiplex guard interval. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="interval">The guard interval.</param>
      /// <returns>an HRESULT indicating whether the guard interval was successfully retrieved</returns>
      [PreserveSig]
      int GetGuardInterval([Out] out B2c2GuardInterval interval);

      /// <summary>
      /// Get the multiplex FEC rate. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="fecRate">The FEC rate.</param>
      /// <returns>an HRESULT indicating whether the FEC rate was successfully retrieved</returns>
      [PreserveSig]
      int GetFec([Out] out B2c2FecRate fecRate);

      /// <summary>
      /// Get the multiplex polarisation. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="polarisation">The polarisation.</param>
      /// <returns>an HRESULT indicating whether the polarisation was successfully retrieved</returns>
      [PreserveSig]
      int GetPolarity([Out] out B2c2Polarisation polarisation);

      /// <summary>
      /// Get the currenly selected satellite. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
      /// <returns>an HRESULT indicating whether the setting was successfully retrieved</returns>
      [PreserveSig]
      int GetDiseqc([Out] out B2c2DiseqcPort diseqcPort);

      /// <summary>
      /// Get the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="toneState">The tone state.</param>
      /// <returns>an HRESULT indicating whether the tone state was successfully retrieved</returns>
      [PreserveSig]
      int GetLnbKHz([Out] out B2c2Tone toneState);

      /// <summary>
      /// Get the LNB local oscillator frequency. Only applicable for DVB-S tuners.
      /// </summary>
      /// <param name="lnbFrequency">The local oscillator frequency in MHz.</param>
      /// <returns>an HRESULT indicating whether the local oscillator frequency was successfully retrieved</returns>
      [PreserveSig]
      int GetLnbFrequency([Out] out int lnbFrequency);

      #endregion

      /// <summary>
      /// Get the corrected block count since the last call to this function.
      /// </summary>
      /// <param name="blockCount">The block count.</param>
      /// <returns>an HRESULT indicating whether the block count was successfully retrieved</returns>
      [PreserveSig]
      int GetCorrectedBlocks([Out] out int blockCount);

      /// <summary>
      /// Get the tuner/demodulator signal quality statistic.
      /// </summary>
      /// <param name="signalQuality">The signal quality as a percentage.</param>
      /// <returns>an HRESULT indicating whether the signal quality was successfully retrieved</returns>
      [PreserveSig]
      int GetSignalQuality([Out] out int signalQuality);

      #endregion

      #region IB2c2Mpeg2TunerCtrl3

      /// <summary>
      /// Set the multiplex bandwidth. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="bandwidth">The bandwidth in MHz, expected to be 6, 7 or 8.</param>
      /// <returns>an HRESULT indicating whether the bandwidth was successfully set</returns>
      [PreserveSig]
      int SetBandwidth(int bandwidth);

      /// <summary>
      /// Get the multiplex bandwidth. Only applicable for DVB-T tuners.
      /// </summary>
      /// <param name="bandwidth">The bandwidth in MHz, expected to be 6, 7 or 8.</param>
      /// <returns>an HRESULT indicating whether the bandwidth was successfully retrieved</returns>
      [PreserveSig]
      int GetBandwidth([Out] out int bandwidth);

      #endregion

      #region IB2c2Mpeg2TunerCtrl4

      /// <summary>
      /// Send a raw DiSEqC command.
      /// </summary>
      /// <param name="length">The length of the command in bytes.</param>
      /// <param name="command">The command.</param>
      /// <returns>an HRESULT indicating whether the command was successfully sent</returns>
      int SendDiSEqCCommand(int length, IntPtr command);

      #endregion
    }

    #endregion

    #region data control

    [ComVisible(false), ComImport,
      Guid("a12a4531-72d2-40fc-b17d-8f9b0004444f"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2c2Mpeg2DataCtrl6
    {
      #region IB2c2Mpeg2DataCtrl

      #region transport stream PIDs

      /// <summary>
      /// Get the maximum number of transport stream class PIDs that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      int GetMaxPIDCount([Out] out int maxPidCount);

      /// <summary>
      /// Obsolete. Use AddPIDsToPin() or AddTsPIDs() instead.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to register.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      int AddPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids);

      /// <summary>
      /// Obsolete. Use DeletePIDsFromPin() or DeleteTsPIDs() instead.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      int DeletePIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids);

      #endregion

      #region IP PIDs

      /// <summary>
      /// Get the maximum number of IP class PIDs that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      int GetMaxIpPIDCount([Out] out int maxPidCount);

      /// <summary>
      /// Register IP class PID(s) that are of interest to the application. Packets marked with these PIDs
      /// will be passed on the B2C2 filter's first data output pin.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to register.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      int AddIpPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids);

      /// <summary>
      /// Deregister IP class PID(s) that are no longer of interest to the application. Packets marked with
      /// these PIDs will no longer be passed on the B2C2 filter's first data output pin.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      int DeleteIpPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids);

      /// <summary>
      /// Get the list of IP class PIDs that are currently registered with the interface.
      /// </summary>
      /// <param name="pidCount">The number of PIDs registered.</param>
      /// <param name="pids">The registered PIDs.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully retrieved</returns>
      [PreserveSig]
      int GetIpPIDs([Out] out int pidCount,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out int[] pids
      );

      #endregion

      #region all PIDs

      /// <summary>
      /// Deregister all PIDs currently registered with the interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      int PurgeGlobalPIDs();

      /// <summary>
      /// Get the maximum number of PIDs of any class that may be registered at any given time.
      /// </summary>
      /// <param name="maxPidCount">The maximum number of PIDs that may be registered.</param>
      /// <returns>an HRESULT indicating whether the maximum PID count was successfully retrieved</returns>
      [PreserveSig]
      int GetMaxGlobalPIDCount([Out] out int maxPidCount);

      /// <summary>
      /// Get the list of PIDs of all classes that are currently registered with the interface.
      /// </summary>
      /// <param name="pidCount">The number of PIDs registered.</param>
      /// <param name="pids">The registered PIDs.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully retrieved</returns>
      [PreserveSig]
      int GetGlobalPIDs([Out] out int pidCount,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out int[] pids
      );

      #endregion

      /// <summary>
      /// Reset the values of the statistics retrieved by GetDataReceptionStats().
      /// </summary>
      /// <returns>an HRESULT indicating whether the statistics were successfully reset</returns>
      [PreserveSig]
      int ResetDataReceptionStats();

      /// <summary>
      /// Get the current values of statistics that can be used for monitoring signal quality. The statistics
      /// are measured since the last call to this function or to ResetDataReceptionStats().
      /// </summary>
      /// <param name="ipRatio">The ratio of correctly received IP class packets to total IP packets.</param>
      /// <param name="tsRatio">The ratio of correctly received TS class packets to total TS packets.</param>
      /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
      [PreserveSig]
      int GetDataReceptionStats([Out] out int ipRatio, [Out] out int tsRatio);

      #endregion

      #region IB2c2Mpeg2DataCtrl2

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
      int AddPIDsToPin([In, Out] ref int pidCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids, int pidIndex);

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
      int DeletePIDsFromPin(int pidCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids, int pidIndex);

      #endregion

      #region IB2c2Mpeg2DataCtrl3

      #region transport stream PIDs

      /// <summary>
      /// Register transport stream class PID(s) that are of interest to the application. Packets marked
      /// with these PIDs will be passed on the first data output pin of the B2C2 filter.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to register.</param>
      /// <param name="pids">The PIDs to register.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      int AddTsPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids);

      /// <summary>
      /// Deregister transport stream class PID(s) when they are no longer of interest to the application.
      /// Packets marked with these PIDs will no longer be passed on the first data output pin of the B2C2
      /// filter.
      /// </summary>
      /// <param name="pidCount">The number of PIDs to deregister.</param>
      /// <param name="pids">The PIDs to deregister.</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully deregistered</returns>
      [PreserveSig]
      int DeleteTsPIDs(int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pids);

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
      int GetTsState([Out] out int openPidCount, [Out] out int runningPidCount,
              [In, Out] ref int pidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] pidList
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
      int GetIpState([Out] out int openPidCount, [Out] out int runningPidCount,
              [In, Out] ref int pidCount,
              [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] out int[] pidList
      );

      /// <summary>
      /// Get the number of IP class PID bytes and packets that have been received.
      /// </summary>
      /// <param name="byteCount">The number of bytes received.</param>
      /// <param name="packetCount">The number of packets received.</param>
      /// <returns>an HRESULT indicating whether the statistics were successfully retrieved</returns>
      [PreserveSig]
      int GetReceivedDataIp(long byteCount, long packetCount);

      #endregion

      #region multicast

      /// <summary>
      /// Register the given multicast MAC addresses with the interface.
      /// </summary>
      /// <remarks>
      /// The maximum number of addresses that may be registered can be retrieved from MAX_MAC_ADDRESS_COUNT.
      /// </remarks>
      /// <param name="addressList">The list of addresses to register.</param>
      /// <returns>an HRESULT indicating whether the MAC addresses were successfully registered</returns>
      [PreserveSig]
      int AddMulticastMacAddress(MacAddressList addressList);

      /// <summary>
      /// Get the list of multicast MAC addresses that are registered with the interface.
      /// </summary>
      /// <param name="addressList">The list of addresses.</param>
      /// <returns>an HRESULT indicating whether the address list was successfully retrieved</returns>
      [PreserveSig]
      int GetMulticastMacAddressList([Out] out MacAddressList addressList);

      /// <summary>
      /// Deregister the given multicast MAC addresses from the interface.
      /// </summary>
      /// <remarks>
      /// The maximum number of addresses that may be deregistered is set at MAX_MAC_ADDRESS_COUNT.
      /// </remarks>
      /// <param name="addressList">The list of addresses to deregister.</param>
      /// <returns>an HRESULT indicating whether the MAC addresses were successfully deregistered</returns>
      [PreserveSig]
      int DeleteMulticastMacAddress(MacAddressList addressList);

      #endregion

      #region unicast

      /// <summary>
      /// Set the device's unicast MAC address.
      /// </summary>
      /// <param name="address">The address to set.</param>
      /// <returns>an HRESULT indicating whether the address was successfully set</returns>
      [PreserveSig]
      int SetUnicastMacAddress(MacAddress address);

      /// <summary>
      /// Get the device's current unicast MAC address.
      /// </summary>
      /// <param name="address">The current address.</param>
      /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
      [PreserveSig]
      int GetUnicastMacAddress([Out] out MacAddress address);

      /// <summary>
      /// Restore the unicast MAC address to the default address for the device.
      /// </summary>
      /// <returns>an HRESULT indicating whether the address was successfully restored</returns>
      [PreserveSig]
      int RestoreUnicastMacAddress();

      #endregion

      #endregion

      #region IB2c2Mpeg2DataCtrl4

      /// <summary>
      /// Get the device's MAC address.
      /// </summary>
      /// <param name="macAddress">The MAC address.</param>
      /// <returns>an HRESULT indicating whether the MAC address was successfully retrieved</returns>
      [PreserveSig]
      int GetHardwareMacAddress(IntPtr macAddress);

      [PreserveSig]
      int SetTableId(int tableId);

      [PreserveSig]
      int GetTableId([Out] out int tableId);

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
      int GetKeyCount([Out] out int totalKeyCount, [Out] out int pidTscKeyCount, [Out] out int pidKeyCount, [Out] out int globalKeyCount);

      /// <summary>
      /// Get the details for the keys that are registered with and being used by the interface.
      /// </summary>
      /// <param name="keyCount">The number of keys in use.</param>
      /// <param name="keyTypes">A pointer to an array listing the type of each key.</param>
      /// <param name="pids">A pointer to an array listing the PID associated with each key.</param>
      /// <returns>an HRESULT indicating whether the key details were successfully retrieved</returns>
      [PreserveSig]
      int GetKeysInUse([Out] out int keyCount, [Out] out IntPtr keyTypes, [Out] out IntPtr pids);

      /// <summary>
      /// Register a decryption key with the interface.
      /// </summary>
      /// <param name="keyType">The key type.</param>
      /// <param name="pid">The PID to use the key to decrypt.</param>
      /// <param name="key">The key.</param>
      /// <param name="keyLength">The length of the key.</param>
      /// <returns>an HRESULT indicating whether the key was successfully registered</returns>
      [PreserveSig]
      int AddKey(B2c2KeyType keyType, uint pid, IntPtr key, int keyLength);

      /// <summary>
      /// Deregister a decryption key.
      /// </summary>
      /// <param name="keyType">The key type.</param>
      /// <param name="pid">The PID that the key is associated with.</param>
      /// <returns>an HRESULT indicating whether the key was successfully deregistered</returns>
      [PreserveSig]
      int DeleteKey(B2c2KeyType keyType, uint pid);

      /// <summary>
      /// Deregister all decryption keys.
      /// </summary>
      /// <returns>an HRESULT indicating whether the keys were successfully deregistered</returns>
      [PreserveSig]
      int PurgeKeys();

      #endregion

      #endregion

      #region IB2c2Mpeg2DataCtrl5

      /// <summary>
      /// Register a callback delegate that the interface can use to pass raw transport stream packets
      /// directly to the application. The packets passed correspond with the transport stream class PIDs
      /// registered with the interface.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      int SetCallbackForTransportStream(OnB2c2TsData callback);

      #endregion

      #region IB2c2Mpeg2DataCtrl6

      /// <summary>
      /// Get information about the B2C2 compatible devices installed in the system.
      /// </summary>
      /// <param name="deviceInfo">A pointer to an array of DeviceInfo instances.</param>
      /// <param name="infoSize">The number of bytes of device information.</param>
      /// <param name="deviceCount">As an input, the number of devices supported by the application; as an
      ///   output, the number of devices installed in the system.</param>
      /// <returns>an HRESULT indicating whether the device information was successfully retrieved</returns>
      [PreserveSig]
      int GetDeviceList(IntPtr deviceInfo, [In, Out] ref int infoSize, [In, Out] ref int deviceCount);

      /// <summary>
      /// Select (activate) a specific B2C2 device.
      /// </summary>
      /// <param name="deviceId">The identifier of the device to select.</param>
      /// <returns>an HRESULT indicating whether the device was successfully selected</returns>
      [PreserveSig]
      int SelectDevice(uint deviceId);

      #endregion
    }

    #endregion

    #region AV control

    [ComVisible(false), ComImport,
      Guid("3ca933bb-4378-4e03-8abd-02450169aa5e"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2c2Mpeg2AVCtrl3
    {
      #region IB2c2Mpeg2AVCtrl

      /// <summary>
      /// Register the audio and/or video PID(s) that are of interest to the application. Packets marked
      /// with these PIDs will be passed on B2C2 filter audio and video output pins.
      /// </summary>
      /// <param name="audioPid">The audio PID (or zero to not register an audio PID).</param>
      /// <param name="videoPid">The video PID (or zero to not register a video PID).</param>
      /// <returns>an HRESULT indicating whether the PIDs were successfully registered</returns>
      [PreserveSig]
      int SetAudioVideoPIDs(int audioPid, int videoPid);

      #endregion

      #region IB2c2Mpeg2AVCtrl2

      /// <summary>
      /// Register a callback delegate that the interface can use to notify the application about video
      /// stream information.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      int SetCallbackForVideoMode(OnB2c2VideoInfo callback);

      /// <summary>
      /// Deregister the current audio and/or video PID(s) when they are no longer of interest to the
      /// application. Packets marked with the previously registered PIDs will no longer be passed on the
      /// B2C2 filter audio and video output pins.
      /// </summary>
      /// <param name="audioPid">A non-zero value to deregister the current audio PID.</param>
      /// <param name="videoPid">A non-zero value to deregister the current video PID.</param>
      /// <returns>an HRESULT indicating whether the PID(s) were successfully deregistered</returns>
      [PreserveSig]
      int DeleteAudioVideoPIDs(int audioPid, int videoPid);

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
      int GetAudioVideoState([Out] out int openAudioStreamCount, [Out] out int openVideoStreamCount,
            [Out] out int totalAudioStreamCount, [Out] out int totalVideoStreamCount,
            [Out] out int audioPid, [Out] out int videoPid
      );

      #endregion

      #region IB2c2Mpeg2AVCtrl3

      /// <summary>
      /// Get IR data from the interface. The size of each code is two bytes, and up to 4 codes may be
      /// retrieved in one call.
      /// </summary>
      /// <param name="dataBuffer">A pointer to a buffer for the interface to populate.</param>
      /// <param name="bufferCapacity">The number of IR codes that the buffer is able to hold. Note that this
      ///   is not the same as the size of the buffer since the code size is two bytes not one.</param>
      /// <returns>an HRESULT indicating whether the IR data was successfully retrieved</returns>
      [PreserveSig]
      int GetIRData([Out] out IntPtr dataBuffer, [In, Out] ref int bufferCapacity);

      #endregion
    }

    #endregion

    #region timeshifting

    [ComVisible(false), ComImport,
      Guid("a306af1c-51d9-496d-9e7a-1cfe28f51fda"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2c2Mpeg2TimeshiftCtrl
    {
      /// <summary>
      /// Set the name of the file to use for timeshifting. Default is C:\Timeshift.ts.
      /// </summary>
      /// <param name="fileName">The name of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file name was successfully set</returns>
      [PreserveSig]
      int SetFilename([MarshalAs(UnmanagedType.LPWStr)] string fileName);

      /// <summary>
      /// Get the name of the file configured for use for timeshifting.
      /// </summary>
      /// <param name="fileName">The name of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file name was successfully retrieved</returns>
      [PreserveSig]
      int GetFilename([Out, MarshalAs(UnmanagedType.LPWStr)] out string fileName);

      /// <summary>
      /// Start recording during live streaming. Recording is usually only started when live streaming is
      /// paused.
      /// </summary>
      /// <returns>an HRESULT indicating whether recording was successfully started</returns>
      [PreserveSig]
      int StartRecord();

      /// <summary>
      /// Stop recording immediately. Recording is usually stopped when timeshifting catches up with the
      /// live position.
      /// </summary>
      /// <returns>an HRESULT indicating whether recording was successfully stopped</returns>
      [PreserveSig]
      int StopRecord();

      /// <summary>
      /// Enable the timeshifting capability.
      /// </summary>
      /// <returns>an HRESULT indicating whether the timeshifting capability was successfully enabled</returns>
      [PreserveSig]
      int Enable();

      /// <summary>
      /// Disable the timeshifting capability.
      /// </summary>
      /// <returns>an HRESULT indicating whether the timeshifting capability was successfully disabled</returns>
      [PreserveSig]
      int Disable();

      /// <summary>
      /// Set the value of one of the timeshifting interface options.
      /// </summary>
      /// <param name="option">The option to set.</param>
      /// <param name="value">The option value to set.</param>
      /// <returns>an HRESULT indicating whether the option value was successfully set</returns>
      [PreserveSig]
      int SetOption(B2c2PvrOption option, int value);

      /// <summary>
      /// Get the value of one of the timeshifting interface options.
      /// </summary>
      /// <param name="option">The option to get.</param>
      /// <param name="value">The option value.</param>
      /// <returns>an HRESULT indicating whether the option value was successfully retrieved</returns>
      [PreserveSig]
      int GetOption(B2c2PvrOption option, [Out] out int value);

      /// <summary>
      /// Set the playback marker position within the timeshifting file.
      /// </summary>
      /// <param name="filePosition">The playback marker position.</param>
      /// <returns>an HRESULT indicating whether the marker position was successfully set</returns>
      [PreserveSig]
      int SetFilePosition(long filePosition);

      /// <summary>
      /// Get the current size of the file configured for use for timeshifting.
      /// </summary>
      /// <param name="fileSize">The size of the timeshifting file.</param>
      /// <returns>an HRESULT indicating whether the file size was successfully retrieved</returns>
      [PreserveSig]
      int GetFileSize([Out] out long fileSize);

      /// <summary>
      /// Get the playback marker position within the timeshifting file.
      /// </summary>
      /// <param name="filePosition">The playback marker position.</param>
      /// <returns>an HRESULT indicating whether the marker position was successfully retrieved</returns>
      [PreserveSig]
      int GetFilePosition([Out] out long filePosition);

      /// <summary>
      /// Register a callback delegate that the interface can use to notify the application about critical
      /// playback/recording state changes.
      /// </summary>
      /// <param name="callback">A pointer to the callback delegate.</param>
      /// <returns>an HRESULT indicating whether the callback delegate was successfully registered</returns>
      [PreserveSig]
      int SetCallback(OnB2c2TimeShiftState callback);
    }

    #endregion

    #region multicast

    [ComVisible(false), ComImport,
      Guid("0b5a8a87-7133-4a37-846e-77f568a52155"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IB2c2Mpeg2MulticastCtrl
    {
      /// <summary>
      /// Enable the video/audio multicasting capability. Any streams that were previously active will be
      /// re-enabled.
      /// </summary>
      /// <returns>an HRESULT indicating whether the multicasting capability was successfully enabled</returns>
      [PreserveSig]
      int Enable();

      /// <summary>
      /// Disable the video/audio multicasting capability. All active streams will be disabled.
      /// </summary>
      /// <returns>an HRESULT indicating whether the multicasting capability was successfully disabled</returns>
      [PreserveSig]
      int Disable();

      /// <summary>
      /// Start multicasting the content from a specific set of PIDs on a specific network interface.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1) to multicast on.</param>
      /// <param name="port">The network interface port to multicast from.</param>
      /// <param name="pidCount">The number of PIDs in the list of PIDs to multicast.</param>
      /// <param name="pidList">A pointer to a list of PIDs (Int32) to multicast.</param>
      /// <returns>an HRESULT indicating whether the multicast was successfully started</returns>
      [PreserveSig]
      int StartMulticast([MarshalAs(UnmanagedType.LPStr)] string address, ushort port, int pidCount, IntPtr pidList);

      /// <summary>
      /// Stop multicasting on a specific network interface.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1) to stop multicasting on.</param>
      /// <param name="port">The network interface port to stop multicasting from.</param>
      /// <returns>an HRESULT indicating whether the multicast was successfully stopped</returns>
      [PreserveSig]
      int StopMulticast([MarshalAs(UnmanagedType.LPStr)] string address, ushort port);

      /// <summary>
      /// Set the network interface to use for multicast operations.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
      /// <returns>an HRESULT indicating whether the address was successfully set</returns>
      [PreserveSig]
      int SetNetworkInterface([MarshalAs(UnmanagedType.LPStr)] string address);

      /// <summary>
      /// Get the network interface configured for use for multicast operations.
      /// </summary>
      /// <param name="address">The IPv4 network interface address (eg. 192.168.1.1).</param>
      /// <returns>an HRESULT indicating whether the address was successfully retrieved</returns>
      [PreserveSig]
      int GetNetworkInterface([Out, MarshalAs(UnmanagedType.LPStr)] out string address);
    }

    #endregion

    #endregion

    #region structs

    #pragma warning disable 0649, 0169
    // Some of these structs are used when marshaling from unmanaged to managed memory, like in ReadDeviceInfo().

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct TunerCapabilities
    {
      public B2c2TunerType TunerType;
      [MarshalAs(UnmanagedType.Bool)]
      public bool ConstellationSupported;         // Is SetModulation() supported?
      [MarshalAs(UnmanagedType.Bool)]
      public bool FecSupported;                   // Is SetFec() suppoted?
      public uint MinTransponderFrequency;      // unit = kHz
      public uint MaxTransponderFrequency;      // unit = kHz
      public uint MinTunerFrequency;            // unit = kHz
      public uint MaxTunerFrequency;            // unit = kHz
      public uint MinSymbolRate;                // unit = Baud
      public uint MaxSymbolRate;                // unit = Baud
      private uint AutoSymbolRate;              // Obsolete		
      public B2c2PerformanceMonitoringCapability PerformanceMonitoringCapabilities;
      public uint LockTime;                     // unit = ms
      public uint KernelLockTime;               // unit = ms
      public B2c2AcquisionCapability AcquisitionCapabilities;
    }

    public struct MacAddress
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAC_ADDRESS_LENGTH)]
      public byte[] Address;
    }

    private struct MacAddressList
    {
      public int AddressCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_MAC_ADDRESS_COUNT)]
      public MacAddress[] Address;
    }

    private struct VideoInfo
    {
      public ushort HorizontalResolution;
      public ushort VerticalResolution;
      public B2c2VideoAspectRatio AspectRatio;
      public B2c2VideoFrameRate FrameRate;
    }
    #pragma warning restore 0649, 0169

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode), ComVisible(false)]
    public struct DeviceInfo
    {
      public uint DeviceId;
      public MacAddress MacAddress;
      private ushort Padding1;
      public B2c2TunerType TunerType;
      public B2c2BusType BusInterface;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsInUse;
      private byte Padding2;
      private ushort Padding3;
      public uint ProductId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
      public string ProductName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
      public string ProductDescription;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
      public string ProductRevision;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 61)]
      public string ProductFrontEnd;
    }

    #endregion
    #pragma warning restore 1591

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
    private delegate uint OnB2c2TsData(ushort pid, IntPtr data);

    /// <summary>
    /// Called by the AV interface when the interface wants to notify the controlling application about the
    /// video stream information.
    /// </summary>
    /// <param name="info">The video stream information.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint OnB2c2VideoInfo(VideoInfo info);

    /// <summary>
    /// Called by the timeshifting interface when the interface needs to notify the controlling application
    /// about critical playback/recording status changes.
    /// </summary>
    /// <param name="state">The current timeshifting interface state.</param>
    /// <returns>???</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint OnB2c2TimeShiftState(B2c2PvrCallbackState state);

    #endregion

    #region constants

    private static readonly Guid B2C2_ADAPTER_CLSID = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x00, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);

    private const int MAX_DEVICE_COUNT = 16;
    private const int MAC_ADDRESS_LENGTH = 6;
    private const int MAX_MAC_ADDRESS_COUNT = 32;
    private static readonly int DEVICE_INFO_SIZE = Marshal.SizeOf(typeof(DeviceInfo));                // 416
    private static readonly int TUNER_CAPABILITIES_SIZE = Marshal.SizeOf(typeof(TunerCapabilities));  // 56

    #endregion

    #region variables

    private IBaseFilter _filterB2c2Adapter = null;
    private IB2c2Mpeg2DataCtrl6 _interfaceData = null;
    /// <summary>
    /// The main tuning control interface.
    /// </summary>
    protected IB2c2Mpeg2TunerCtrl4 _interfaceTuner = null;

    private IBaseFilter _filterInfiniteTee = null;

    private DeviceInfo _deviceInfo;
    /// <summary>
    /// B2C2-specific tuner hardware capability information.
    /// </summary>
    protected TunerCapabilities _capabilities;

    // PID filter variables - especially important for DVB-S2 tuners.
    private int _maxPidCount = 0;
    private HashSet<int> _filterPids = new HashSet<int>();

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Base"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Base(DeviceInfo info)
      : base(info.ProductName, "B2C2 tuner " + info.DeviceId)
    {
      _deviceInfo = info;
    }

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (_interfaceTuner == null)
      {
        _isSignalLocked = false;
        _isSignalPresent = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      _isSignalLocked = (_interfaceTuner.CheckLock() == 0);
      _isSignalPresent = _isSignalLocked;
      if (!onlyUpdateLock)
      {
        _interfaceTuner.GetSignalStrength(out _signalLevel);
        _interfaceTuner.GetSignalQuality(out _signalQuality);
      }
    }

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("TunerB2c2Base: perform loading");
      InitialiseGraph();

      // Create, add and initialise the B2C2 source filter.
      _filterB2c2Adapter = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, B2C2_ADAPTER_CLSID, "B2C2 Source");
      _interfaceData = _filterB2c2Adapter as IB2c2Mpeg2DataCtrl6;
      _interfaceTuner = _filterB2c2Adapter as IB2c2Mpeg2TunerCtrl4;
      if (_interfaceTuner == null || _interfaceData == null)
      {
        throw new TvException("Failed to find interfaces on source filter.");
      }
      int hr = _interfaceTuner.Initialize();
      HResult.ThrowException(hr, "Failed to initialise tuner interface.");

      // This line is a remnant from old code. I don't know if/why it is necessary, but no harm
      // in leaving it...
      _interfaceTuner.CheckLock();

      // The source filter has multiple output pins, and connecting to the right one is critical.
      // Plugins can't handle this automatically, so we add an extra infinite tee in between the source
      // filter and any plugin filters.
      _filterInfiniteTee = (IBaseFilter)new InfTee();
      FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterInfiniteTee, "Infinite Tee", _filterB2c2Adapter, 2, 0);

      // Load and open plugins.
      IBaseFilter lastFilter = _filterInfiniteTee;
      LoadPlugins(_filterB2c2Adapter, _graph, ref lastFilter);

      // This class implements the plugin interface and should be considered as the main plugin.
      _customDeviceInterfaces.Add(this);

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      SetFilterState(null, new HashSet<ushort>(), false);

      ReadTunerInfo();
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    protected override void PerformUnloading()
    {
      this.LogDebug("TunerB2c2Base: perform unloading");
      _interfaceData = null;
      _interfaceTuner = null;

      if (_graph != null)
      {
        _graph.RemoveFilter(_filterInfiniteTee);
        _graph.RemoveFilter(_filterB2c2Adapter);
      }
      Release.ComObject("B2C2 infinite tee", ref _filterInfiniteTee);
      Release.ComObject("B2C2 source filter", ref _filterB2c2Adapter);

      CleanUpGraph();
    }

    #endregion

    /// <summary>
    /// Detect the compatible tuners installed in the system.
    /// </summary>
    /// <returns>an enumerable collection of <see cref="T:TvLibrary.Interfaces.ITVCard"/></returns>
    public static IEnumerable<ITVCard> DetectTuners()
    {
      Log.Debug("TunerB2c2Base: detect tuners");
      List<ITVCard> tuners = new List<ITVCard>();

      // Instanciate a data interface so we can check how many tuners are installed.
      IBaseFilter b2c2Source = null;
      try
      {
        b2c2Source = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(B2C2_ADAPTER_CLSID));
      }
      catch (Exception ex)
      {
        Log.Error(ex, "TunerB2c2Base: failed to create source filter instance");
        return tuners;
      }

      try
      {
        IB2c2Mpeg2DataCtrl6 dataInterface = b2c2Source as IB2c2Mpeg2DataCtrl6;
        if (dataInterface == null)
        {
          Log.Debug("TunerB2c2Base: failed to find B2C2 data interface on filter");
          Release.ComObject("B2C2 source filter", ref b2c2Source);
          return tuners;
        }

        // Get device details...
        int size = DEVICE_INFO_SIZE * MAX_DEVICE_COUNT;
        int deviceCount = MAX_DEVICE_COUNT;
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
            Log.Debug("TunerB2c2Base: failed to get device list, hr = 0x{0:x}", hr);
          }
          else
          {
            //DVB_MMI.DumpBinary(tempBuffer, 0, size);
            Log.Debug("TunerB2c2Base: device count = {0}", deviceCount);
            for (int i = 0; i < deviceCount; i++)
            {
              Log.Debug("TunerB2c2Base: device {0}", i + 1);
              DeviceInfo d = (DeviceInfo)Marshal.PtrToStructure(structurePtr, typeof(DeviceInfo));
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

              switch (d.TunerType)
              {
                case B2c2TunerType.Satellite:
                  tuners.Add(new TunerB2c2Satellite(d));
                  break;
                case B2c2TunerType.Cable:
                  tuners.Add(new TunerB2c2Cable(d));
                  break;
                case B2c2TunerType.Terrestrial:
                  tuners.Add(new TunerB2c2Terrestrial(d));
                  break;
                case B2c2TunerType.Atsc:
                  tuners.Add(new TunerB2c2Atsc(d));
                  break;
                default:
                  // The tuner may not be redetected properly after standby in some cases.
                  Log.Warn("TunerB2c2Base: unknown tuner type, cannot use this tuner");
                  break;
              }

              structurePtr = IntPtr.Add(structurePtr, DEVICE_INFO_SIZE);
            }
            Log.Debug("TunerB2c2Base: result = success");
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

    /// <summary>
    /// Attempt to read the tuner information.
    /// </summary>
    private void ReadTunerInfo()
    {
      this.LogDebug("TunerB2c2Base: read tuner information");

      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogDebug("TunerB2c2Base: failed to select device, hr = 0x{0:x}", hr);
        return;
      }

      this.LogDebug("TunerB2c2Base: reading capabilities");
      IntPtr buffer = Marshal.AllocCoTaskMem(TUNER_CAPABILITIES_SIZE);
      try
      {
        for (int i = 0; i < TUNER_CAPABILITIES_SIZE; i++)
        {
          Marshal.WriteByte(buffer, i, 0);
        }
        int returnedByteCount = TUNER_CAPABILITIES_SIZE;
        hr = _interfaceTuner.GetTunerCapabilities(buffer, ref returnedByteCount);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogDebug("TunerB2c2Base: result = failure, hr = 0x{0:x}", hr);
        }
        else
        {
          //DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);
          TunerCapabilities capabilities = (TunerCapabilities)Marshal.PtrToStructure(buffer, typeof(TunerCapabilities));
          this.LogDebug("  tuner type                = {0}", capabilities.TunerType);
          this.LogDebug("  set constellation?        = {0}", capabilities.ConstellationSupported);
          this.LogDebug("  set FEC rate?             = {0}", capabilities.FecSupported);
          this.LogDebug("  min transponder frequency = {0} kHz", capabilities.MinTransponderFrequency);
          this.LogDebug("  max transponder frequency = {0} kHz", capabilities.MaxTransponderFrequency);
          this.LogDebug("  min tuner frequency       = {0} kHz", capabilities.MinTunerFrequency);
          this.LogDebug("  max tuner frequency       = {0} kHz", capabilities.MaxTunerFrequency);
          this.LogDebug("  min symbol rate           = {0} baud", capabilities.MinSymbolRate);
          this.LogDebug("  max symbol rate           = {0} baud", capabilities.MaxSymbolRate);
          this.LogDebug("  performance monitoring    = {0}", capabilities.PerformanceMonitoringCapabilities.ToString());
          this.LogDebug("  lock time                 = {0} ms", capabilities.LockTime);
          this.LogDebug("  kernel lock time          = {0} ms", capabilities.KernelLockTime);
          this.LogDebug("  acquisition capabilities  = {0}", capabilities.AcquisitionCapabilities.ToString());
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }

      this.LogDebug("TunerB2c2Base: reading max PID counts");
      int count = 0;
      hr = _interfaceData.GetMaxGlobalPIDCount(out count);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  global max                = {0}", count);
      }
      else
      {
        this.LogDebug("TunerB2c2Base: result = failure, hr = 0x{0:x}", hr);
      }
      hr = _interfaceData.GetMaxIpPIDCount(out count);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  IP max                    = {0}", count);
      }
      else
      {
        this.LogDebug("TunerB2c2Base: result = failure, hr = 0x{0:x}", hr);
      }
      hr = _interfaceData.GetMaxPIDCount(out count);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  TS max                    = {0}", count);
        _maxPidCount = count;
      }
      else
      {
        this.LogDebug("TunerB2c2Base: result = failure, hr = 0x{0:x}", hr);
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
    /// <param name="tunerExternalIdentifier">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalIdentifier, CardType tunerType, object context)
    {
      // This is a "special" implementation. We do initialisation in other functions.
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked when the device has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITVCard tuner, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
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
    /// This callback is invoked after a tune request is submitted, when the device is started but before
    /// signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked before the device is stopped.
    /// </summary>
    /// <param name="tuner">The device instance that this device instance is associated with.</param>
    /// <param name="action">As an input, the action that TV Server wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref TunerAction action)
    {
    }

    #endregion

    #endregion

    #region IMpeg2PidFilter member

    /// <summary>
    /// Configure the PID filter.
    /// </summary>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <param name="forceEnable">Set this parameter to <c>true</c> to force the filter to be enabled.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    public bool SetFilterState(IChannel tuningDetail, ICollection<ushort> pids, bool forceEnable)
    {
      this.LogDebug("TunerB2c2Base: set PID filter state, force enable = {0}", forceEnable);
      bool fullTransponder = true;
      bool success = true;

      int hr = _interfaceData.SelectDevice(_deviceInfo.DeviceId);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogDebug("TunerB2c2Base: failed to select device, hr = 0x{0:x}", hr);
        return false;
      }

      DVBSChannel satelliteTuningDetail = tuningDetail as DVBSChannel;
      if (pids == null || pids.Count == 0 ||
        (
          _deviceInfo.BusInterface != B2c2BusType.Usb1 &&
          (
            satelliteTuningDetail == null ||
            (satelliteTuningDetail.ModulationType != ModulationType.ModQpsk && satelliteTuningDetail.ModulationType != ModulationType.Mod8Psk)
          ) &&
          !forceEnable
        )
      )
      {
        this.LogDebug("TunerB2c2Base: disabling PID filter");
      }
      else
      {
        // If we get to here then the default approach is to enable the filter, but
        // there is one other constraint that applies: the filter PID limit.
        fullTransponder = false;
        if (pids.Count > _maxPidCount)
        {
          this.LogDebug("TunerB2c2Base: too many PIDs, hardware limit = {0}, actual count = {1}", _maxPidCount, pids.Count);
          // When the forceEnable flag is set, we just set as many PIDs as possible.
          if (!forceEnable)
          {
            this.LogDebug("TunerB2c2Base: disabling PID filter");
            fullTransponder = true;
          }
        }

        if (!fullTransponder)
        {
          this.LogDebug("TunerB2c2Base: enabling PID filter");
          fullTransponder = false;
        }
      }

      int openPidCount;
      int runningPidCount;
      int totalPidCount = _maxPidCount;
      int[] currentPids = new int[_maxPidCount];
      int availablePidCount = 0;
      int changingPidCount = 1;
      hr = _interfaceData.GetTsState(out openPidCount, out runningPidCount, ref totalPidCount, currentPids);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogDebug("TunerB2c2Base: failed to retrieve current PIDs, hr = 0x{0:x}", hr);
        fullTransponder = true;
        success = false;
      }
      else
      {
        this.LogDebug("TunerB2c2Base: current PID details (before update)");
        this.LogDebug("  open count     = {0}", openPidCount);
        this.LogDebug("  running count  = {0}", runningPidCount);
        this.LogDebug("  returned count = {0}", totalPidCount);
        if (currentPids != null)
        {
          for (int i = 0; i < totalPidCount; i++)
          {
            this.LogDebug("  {0,-2}             = {1} (0x{1:x})", i + 1, currentPids[i]);
          }
        }
        availablePidCount = _maxPidCount - totalPidCount;
      }

      if (!fullTransponder)
      {
        // Remove the PIDs that are no longer needed.
        for (byte i = 0; i < totalPidCount; i++)
        {
          if (currentPids != null && !pids.Contains((ushort)currentPids[i]))
          {
            hr = _interfaceData.DeletePIDsFromPin(1, new int[1] { currentPids[i] }, 0);
            if (hr != (int)HResult.Severity.Success)
            {
              this.LogDebug("TunerB2c2Base: failed to remove PID {0} (0x{0:x}), hr = 0x{1:x}", currentPids[i], hr);
              success = false;
            }
            else
            {
              this.LogDebug("  delete PID {0} (0x{0:x})...", currentPids[i]);
              availablePidCount++;
            }
          }
        }
        // Add the new PIDs.
        if (pids != null)
        {
          HashSet<int> currentPidsAsHash = new HashSet<int>(currentPids);
          IEnumerator<ushort> en = pids.GetEnumerator();
          while (en.MoveNext() && availablePidCount > 0)
          {
            if (currentPidsAsHash != null && !currentPidsAsHash.Contains(en.Current))
            {
              hr = _interfaceData.AddPIDsToPin(ref changingPidCount, new int[1] { en.Current }, 0);
              if (hr != (int)HResult.Severity.Success)
              {
                this.LogDebug("TunerB2c2Base: failed to add PID {0} (0x{0:x}), hr = 0x{1:x}", en.Current, hr);
                success = false;
              }
              else
              {
                this.LogDebug("  add PID {0} (0x{0:x})...", en.Current);
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
            hr = _interfaceData.DeletePIDsFromPin(1, new int[1] { currentPids[i] }, 0);
            if (hr != (int)HResult.Severity.Success)
            {
              this.LogDebug("TunerB2c2Base: failed to remove PID {0} (0x{0:x}), hr = 0x{1:x}", currentPids[i], hr);
              success = false;
            }
            else
            {
              this.LogDebug("  delete all PIDs...");
            }
          }
        }
        // Allow all PIDs.
        hr = _interfaceData.AddPIDsToPin(ref changingPidCount, new int[1] { (int)B2c2PidFilterMode.AllExcludingNull }, 0);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogDebug("TunerB2c2Base: failed to enable all PIDs, hr = 0x{0:x}", hr);
          success = false;
        }
        else
        {
          this.LogDebug("  allow all excluding NULL...");
        }
      }

      if (success)
      {
        this.LogDebug("TunerB2c2Base: updates complete, result = success");
      }

      return success;
    }

    #endregion
  }
}