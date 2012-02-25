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

namespace TvLibrary.Implementations.DVB
{

  /// <summary>
  /// In order to receive programs, the broadband receiver must first lock onto a channel. This is accomplished by controlling the tuner. IB2C2MPEG2TunerCtrl3 supports satellite, cable, and terrestrial DVB and terrestrial ATSC tuners. IB2C2MPEG2TunerCtrl4 methods allow software to lock onto a channel, including the monitoring of receiver (tuner module) performance statistics such as BER and Uncorrected Blocks.
  /// </summary>
  [ComVisible(true), ComImport,
   Guid("61A9051F-04C4-435e-8742-9EDD2C543CE9"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IB2C2MPEG2TunerCtrl4
  {
    /// <summary>
    /// Sets Transponder Frequency value in MHz
    /// </summary>
    /// <param name="frequency">Transponder Frequency in MHz. Must be greater than or equal to zero. The upper limit is tuner dependent</param>
    /// <returns></returns>
    [PreserveSig]
    int SetFrequency(
      int frequency
      );

    /// <summary>
    /// Sets Symbol Rate value
    /// </summary>
    /// <param name="symbolRate">Symbol Rate in KS/s. Must be greater than or equal to zero. The upper limit is tuner dependent and can be queried by GetTunerCapabilities</param>
    /// <returns></returns>
    [PreserveSig]
    int SetSymbolRate(
      int symbolRate
      );

    /// <summary>
    /// Sets LNB Frequency value
    /// </summary>
    /// <param name="lnbFrequency">LNB Frequency in MHz. Must be greater than or equal to zero and less than Transponder Frequency set by IB2C2MPEG2TunerCtrl2::SetFrequency or by IB2C2MPEG2TunerCtrl2::SetFrequencyKHz</param>
    /// <returns></returns>
    [PreserveSig]
    int SetLnbFrequency(
      int lnbFrequency
      );

    /// <summary>
    /// Sets FEC value
    /// </summary>
    /// <param name="fec">FEC value. Use eFEC enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// FEC_1_2
    /// FEC_2_3
    /// FEC_3_4 
    /// FEC_5_6 
    /// FEC_7_8 
    /// FEC_AUTO</param>
    /// <returns></returns>
    [PreserveSig]
    int SetFec(
      int fec
      );

    /// <summary>
    /// Sets Polarity value
    /// </summary>
    /// <param name="polarity">Polarity value. Use ePolarity enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// POLARITY_HORIZONTAL 
    /// POLARITY_VERTICAL</param>
    /// <returns></returns>
    [PreserveSig]
    int SetPolarity(
      int polarity
      );

    /// <summary>
    /// Sets LNB kHz selection value
    /// </summary>
    /// <param name="lnbKhz">LNB kHz Selection value. Use eLNBSelection enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// LNB_SELECTION_0 
    /// LNB_SELECTION_22 
    /// LNB_SELECTION_33 
    /// LNB_SELECTION_44</param>
    /// <returns></returns>
    [PreserveSig]
    int SetLnbKHz(
      int lnbKhz
      );

    /// <summary>
    /// Sets DiSEqC value
    /// </summary>
    /// <param name="diseqc">DiSEqC value. Use eDiseqc enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// DISEQC_NONE 
    /// DISEQC_SIMPLE_A 
    /// DISEQC_SIMPLE_B 
    /// DISEQC_LEVEL_1_A_A 
    /// DISEQC_LEVEL_1_B_A 
    /// DISEQC_LEVEL_1_A_B 
    /// DISEQC_LEVEL_1_B_B</param>
    /// <returns></returns>
    [PreserveSig]
    int SetDiseqc(
      int diseqc
      );

    /// <summary>
    /// Sets Modulation value
    /// </summary>
    /// <param name="modulation"></param>
    /// <returns></returns>
    [PreserveSig]
    int SetModulation(
      int modulation
      );

    /// <summary>
    /// Creates connection to Tuner Control interface of B2C2MPEG2Filter. (See sample applications for more information.)
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Initialize();

    /// <summary>
    /// Sends tuner parameter values to the tuner and waits until the tuner gets in lock or times out. The time-out value depends on the tuner type
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int SetTunerStatus();

    /// <summary>
    /// Checks lock status of tuner
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int CheckLock();

    /// <summary>
    /// Identifies capabilities of particular tuner
    /// </summary>
    /// <param name="tunerCaps">Pointer to a structure defined in the header file b2c2_defs.h. The caller provides the structure</param>
    /// <param name="count">Pointer to a long variable created by the caller. Variable will hold the size of the structure (bytes) returned</param>
    /// <returns></returns>
    [PreserveSig]
    int GetTunerCapabilities(
      IntPtr tunerCaps,
      ref int count
      );

    /// <summary>
    /// Gets current Transponder Frequency in MHz
    /// </summary>
    /// <param name="freq">Pointer to a long variable created by the caller. Variable will hold the Transponder Frequency in MHz</param>
    /// <returns></returns>
    [PreserveSig]
    int GetFrequency(
      [Out] out int freq
      );

    /// <summary>
    /// Gets current Symbol Rate in MS/s
    /// </summary>
    /// <param name="symbRate">Pointer to a variable created by the caller. Variable will hold the Symbol Rate in KS/s</param>
    /// <returns></returns>
    [PreserveSig]
    int GetSymbolRate(
      [Out] out int symbRate
      );

    /// <summary>
    /// Gets current Modulation value
    /// </summary>
    /// <param name="modulation">Pointer to a long variable created by the caller. Variable will hold the Modulation value. (Use eModulation enumerated type defined in header file b2c2_defs.h delivered as part of the SDK.) Possible values are: 
    /// QAM_4 
    /// QAM_16 
    /// QAM_32 
    /// QAM_64 
    /// QAM_128
    /// QAM_256</param>
    /// <returns></returns>
    [PreserveSig]
    int GetModulation(
      [Out] out int modulation
      );

    /// <summary>
    /// Gets current Signal Strength value in %
    /// </summary>
    /// <param name="signalStrength">Pointer to a variable created by the caller. Variable will hold Signal Strength in %</param>
    /// <returns></returns>
    [PreserveSig]
    int GetSignalStrength(
      [Out] out int signalStrength
      );

    /// <summary>
    /// The GetSignalLevel function is obsolete and is implemented for backwards compatibility only. Current version uses GetSignalStrength or GetSignalQuality method, depending on the tuner type
    /// </summary>
    /// <param name="signalLevel">Pointer to a variable created by the caller. Variable will hold Signal Level in dBm</param>
    /// <returns></returns>
    [PreserveSig]
    int GetSignalLevel(
      [Out] out float signalLevel
      );

    /// <summary>
    /// Gets current Signal to Noise Ratio (SNR) value
    /// </summary>
    /// <param name="snr">Pointer to a variable created by the caller. Variable will hold Signal to Noise Ratio</param>
    /// <returns></returns>
    [PreserveSig]
    int GetSNR(
      [Out] out float snr
      );

    /// <summary>
    /// Gets current pre-error-correction Bit Error Rate (BER) value
    /// </summary>
    /// <param name="ber">Pointer to a variable created by the caller. Variable will hold Bit Error Rate</param>
    /// <param name="flag">(Not used.)</param>
    /// <returns></returns>
    [PreserveSig]
    int GetPreErrorCorrectionBER(
      [Out] out float ber,
      bool flag
      );

    /// <summary>
    /// Gets current count of Uncorrected Blocks
    /// </summary>
    /// <param name="uncorrectedBlocks">Pointer to a variable created by the caller. Variable will hold count of Uncorrected Blocks</param>
    /// <returns></returns>
    [PreserveSig]
    int GetUncorrectedBlocks(
      [Out] out int uncorrectedBlocks
      );

    /// <summary>
    /// Gets current count of Total Blocks
    /// </summary>
    /// <param name="correctedBlocks">Pointer to a variable created by the caller. Variable will hold count of Total Blocks</param>
    /// <returns></returns>
    [PreserveSig]
    int GetTotalBlocks(
      [Out] out int correctedBlocks
      );

    /// <summary>
    /// Gets current Channel value
    /// </summary>
    /// <param name="channel">Pointer to a variable created by the caller. Variable will hold Channel number</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChannel(
      [Out] out int channel
      );

    /// <summary>
    /// Sets Channel value
    /// </summary>
    /// <param name="channel">Channel number</param>
    /// <returns></returns>
    [PreserveSig]
    int SetChannel(
      int channel
      );

    /// <summary>
    /// Sends values to tuner for tuning with optional argument for defining how many times the SDK should check whether the tuner is in lock. The wait time between each check is 50 ms
    /// </summary>
    /// <param name="count">Number of times the SDK should check whether the tuner is in lock.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetTunerStatusEx(
      int count
      );

    /// <summary>
    /// Sets Transponder Frequency value in kHz.
    /// </summary>
    /// <param name="freqKhz">Transponder Frequency in kHz. Must be greater than or equal to zero. The upper limit is tuner dependent</param>
    /// <returns></returns>
    [PreserveSig]
    int SetFrequencyKHz(
      long freqKhz
      );

    /// <summary>
    /// Sets Guard Interval value
    /// </summary>
    /// <param name="interval">Guard Interval value. Use eGuardInterval enumerated type defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// GUARD_INTERVAL_1_32
    /// GUARD_INTERVAL_1_16
    /// GUARD_INTERVAL_1_8
    /// GUARD_INTERVAL_1_4
    /// GUARD_INTERVAL_AUTO </param>
    /// <returns></returns>
    [PreserveSig]
    int SetGuardInterval(
      int interval
      );

    /// <summary>
    /// Gets current Guard Interval value
    /// </summary>
    /// <param name="interval">Pointer to long variable where value for Guard Interval will be stored. eGuardInterval enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// GUARD_INTERVAL_1_32
    /// GUARD_INTERVAL_1_16
    /// GUARD_INTERVAL_1_8
    /// GUARD_INTERVAL_1_4
    /// GUARD_INTERVAL_AUTO </param>
    /// <returns></returns>
    [PreserveSig]
    int GetGuardInterval(
      [Out] out int interval
      );

    /// <summary>
    /// Gets current FEC value
    /// </summary>
    /// <param name="plFec">Pointer to a long variable created by the user where the FEC value will be stored. eFEC enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are:
    /// FEC_1_2
    /// FEC_2_3
    /// FEC_3_4 
    /// FEC_5_6 
    /// FEC_7_8 
    /// FEC_AUTO</param>
    /// <returns></returns>
    [PreserveSig]
    int GetFec(
      [Out] out int plFec
      );

    /// <summary>
    /// Gets current Polarity value
    /// </summary>
    /// <param name="plPolarity">Pointer to a long variable created by the user where the Polarity value will be stored. ePolarity enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are:
    /// POLARITY_HORIZONTAL 
    /// POLARITY_VERTICAL </param>
    /// <returns></returns>
    [PreserveSig]
    int GetPolarity(
      [Out] out int plPolarity
      );

    /// <summary>
    /// Gets current DiSEqC value
    /// </summary>
    /// <param name="plDiseqc">Pointer to a long variable created by the user where the DiSEqC value will be stored. eDiseqc enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are: 
    /// DISEQC_NONE 
    /// DISEQC_SIMPLE_A 
    /// DISEQC_SIMPLE_B 
    /// DISEQC_LEVEL_1_A_A 
    /// DISEQC_LEVEL_1_B_A 
    /// DISEQC_LEVEL_1_A_B 
    /// DISEQC_LEVEL_1_B_B</param>
    /// <returns></returns>
    [PreserveSig]
    int GetDiseqc(
      [Out] out int plDiseqc
      );

    /// <summary>
    /// Gets current LNB kHz selection value
    /// </summary>
    /// <param name="plLnbKHz">Pointer to a long variable created by the user where the LNB kHz Selection value will be stored. eLNBSelection enumerated types are defined in header file b2c2_defs.h delivered as part of the SDK. Possible values are:
    /// LNB_SELECTION_0 
    /// LNB_SELECTION_22 
    /// LNB_SELECTION_33 
    /// LNB_SELECTION_44</param>
    /// <returns></returns>
    [PreserveSig]
    int GetLnbKHz(
      [Out] out int plLnbKHz
      );

    /// <summary>
    /// Gets current LNB Frequency value
    /// </summary>
    /// <param name="plFrequencyMHz">Pointer to a long variable created by the user where the LNB Frequency value in MHz will be stored</param>
    /// <returns></returns>
    [PreserveSig]
    int GetLnbFrequency(
      [Out] out int plFrequencyMHz
      );

    /// <summary>
    /// Gets current count of Corrected Blocks
    /// </summary>
    /// <param name="plCorrectedBlocks">Pointer to a variable created by the caller. Variable will hold count of Corrected Blocks</param>
    /// <returns></returns>
    [PreserveSig]
    int GetCorrectedBlocks(
      [Out] out int plCorrectedBlocks
      );

    /// <summary>
    /// Gets current Signal Quality value in %
    /// </summary>
    /// <param name="pdwSignalQuality">Pointer to a variable created by the caller. Variable will hold Signal Quality in %</param>
    /// <returns></returns>
    [PreserveSig]
    int GetSignalQuality(
      [Out] out int pdwSignalQuality
      );

    /// <summary>
    /// Sets the channel bandwidth.
    /// </summary>
    /// <param name="bandwidth">A long variable created by the user where the Bandwidth is stored. Possible values are:
    /// 6 MHZ
    /// 7 MHZ
    /// 8 MHZ</param>
    /// <returns></returns>
    [PreserveSig]
    int SetBandwidth(
      int bandwidth
      );

    /// <summary>
    /// Gets the channel bandwidth
    /// </summary>
    /// <param name="bandwidth">Pointer to a long variable created by the user where the Bandwidth value will be stored. Possible values are:
    /// 6 MHZ
    /// 7 MHZ
    /// 8 MHZ</param>
    /// <returns></returns>
    [PreserveSig]
    int GetBandwidth(
      [Out] out int bandwidth
      );

    /// <summary>
    /// Sends a DiSEqC command to a DiSEqC compatible device connected to the card
    /// </summary>
    /// <param name="length">A integer variable that contains the number of bytes in the DiSEqC message</param>
    /// <param name="disEqcCommand">A pointer to a sequence of bytes which is the actual DiSEqC bytes to be sent according to the length specified</param>
    /// <returns></returns>
    [PreserveSig]
    int SendDiSEqCCommand(
      int length, IntPtr disEqcCommand
      );
  } ;


}

