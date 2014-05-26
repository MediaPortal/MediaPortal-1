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
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Interface
{
  [Guid("61a9051f-04c4-435e-8742-9edd2c543ce9"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IMpeg2TunerCtrl4
  {
    #region IMpeg2TunerCtrl

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
    int SetFec(FecRate fecRate);

    /// <summary>
    /// Set the multiplex polarisation. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="polarisation">The polarisation.</param>
    /// <returns>an HRESULT indicating whether the polarisation was successfully set</returns>
    [PreserveSig]
    int SetPolarity(Polarisation polarisation);

    /// <summary>
    /// Set the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="toneState">The tone state.</param>
    /// <returns>an HRESULT indicating whether the tone state was successfully set</returns>
    [PreserveSig]
    int SetLnbKHz(Tone toneState);

    /// <summary>
    /// Switch to a specific satellite. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
    /// <returns>an HRESULT indicating whether the DiSEqC command was successfully sent</returns>
    [PreserveSig]
    int SetDiseqc(DiseqcPort diseqcPort);

    /// <summary>
    /// Set the multiplex modulation scheme. Only applicable for DVB-C tuners.
    /// </summary>
    /// <param name="modulation">The modulation scheme.</param>
    /// <returns>an HRESULT indicating whether the modulation scheme was successfully set</returns>
    [PreserveSig]
    int SetModulation(Modulation modulation);

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
    int GetModulation([Out] out Modulation modulation);

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

    #region IMpeg2TunerCtrl2

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
    int SetGuardInterval(GuardInterval interval);

    /// <summary>
    /// Get the multiplex guard interval. Only applicable for DVB-T tuners.
    /// </summary>
    /// <param name="interval">The guard interval.</param>
    /// <returns>an HRESULT indicating whether the guard interval was successfully retrieved</returns>
    [PreserveSig]
    int GetGuardInterval([Out] out GuardInterval interval);

    /// <summary>
    /// Get the multiplex FEC rate. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="fecRate">The FEC rate.</param>
    /// <returns>an HRESULT indicating whether the FEC rate was successfully retrieved</returns>
    [PreserveSig]
    int GetFec([Out] out FecRate fecRate);

    /// <summary>
    /// Get the multiplex polarisation. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="polarisation">The polarisation.</param>
    /// <returns>an HRESULT indicating whether the polarisation was successfully retrieved</returns>
    [PreserveSig]
    int GetPolarity([Out] out Polarisation polarisation);

    /// <summary>
    /// Get the currenly selected satellite. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="diseqcPort">The DiSEqC switch port associated with the satellite.</param>
    /// <returns>an HRESULT indicating whether the setting was successfully retrieved</returns>
    [PreserveSig]
    int GetDiseqc([Out] out DiseqcPort diseqcPort);

    /// <summary>
    /// Get the satellite/LNB/band selection tone state. Only applicable for DVB-S tuners.
    /// </summary>
    /// <param name="toneState">The tone state.</param>
    /// <returns>an HRESULT indicating whether the tone state was successfully retrieved</returns>
    [PreserveSig]
    int GetLnbKHz([Out] out Tone toneState);

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

    #region IMpeg2TunerCtrl3

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

    #region IMpeg2TunerCtrl4

    /// <summary>
    /// Send a raw DiSEqC command.
    /// </summary>
    /// <param name="length">The length of the command in bytes.</param>
    /// <param name="command">The command.</param>
    /// <returns>an HRESULT indicating whether the command was successfully sent</returns>
    int SendDiSEqCCommand(int length, IntPtr command);

    #endregion
  }
}