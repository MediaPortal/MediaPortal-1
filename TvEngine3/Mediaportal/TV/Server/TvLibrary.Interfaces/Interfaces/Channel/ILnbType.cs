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
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Channel
{
  /// <summary>
  /// An interface describing a type or class of low noise block downconverter
  /// (LNB).
  /// </summary>
  /// <remarks>
  /// LNBs are used for the reception of satellite signals. They are the
  /// "thing" mounted on the satellite dish that the cable connects to.
  /// </remarks>
  public interface ILnbType : ICloneable
  {
    /// <summary>
    /// Get the LNB type's identifier.
    /// </summary>
    int Id
    {
      get;
    }

    /// <summary>
    /// Get the LNB type's name.
    /// </summary>
    string Name
    {
      get;
    }

    /// <summary>
    /// Get the local LNB-dependent tuning parameters for a satellite
    /// transponder.
    /// </summary>
    /// <param name="transponderFrequency">The LNB-independent transponder frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="transponderPolarisation">The LNB-independent transponder polarisation.</param>
    /// <param name="lnbSelectionTone">The 22 kHz tone state needed to switch/connect to the LNB.</param>
    /// <param name="localOscillatorFrequency">The local oscillator frequency that the LNB will use for downconversion. The unit is killo-Hertz (kHz).</param>
    /// <param name="bandSelectionTone">The 22 kHz tone that must be supplied to the LNB to select the correct local oscillator.</param>
    /// <param name="bandSelectionPolarisation">The polarisation (voltage) that must be supplied to the LNB to select the correct local oscillator and band.</param>
    void GetTuningParameters(int transponderFrequency, Polarisation transponderPolarisation, Tone22kState lnbSelectionTone, out int localOscillatorFrequency, out Tone22kState bandSelectionTone, out Polarisation bandSelectionPolarisation);

    /// <summary>
    /// Get the local LNB-dependent tuning parameters for a satellite
    /// transponder.
    /// </summary>
    /// <param name="transponderFrequency">The LNB-independent transponder frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="transponderPolarisation">The LNB-independent transponder polarisation.</param>
    /// <param name="lnbSelectionTone">The 22 kHz tone state needed to switch/connect to the LNB.</param>
    /// <param name="localOscillatorFrequency">The local oscillator frequency that the LNB will use for downconversion. The unit is killo-Hertz (kHz).</param>
    /// <param name="oscillatorSwitchFrequency">The LNB's oscillator switch frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="bandSelectionTone">The 22 kHz tone that must be supplied to the LNB to select the correct local oscillator.</param>
    /// <param name="bandSelectionPolarisation">The polarisation (voltage) that must be supplied to the LNB to select the correct local oscillator and band.</param>
    void GetTuningParameters(int transponderFrequency, Polarisation transponderPolarisation, Tone22kState lnbSelectionTone, out int localOscillatorFrequency, out int oscillatorSwitchFrequency, out Tone22kState bandSelectionTone, out Polarisation bandSelectionPolarisation);

    /// <summary>
    /// Get the local LNB-dependent tuning parameters for a satellite
    /// transponder.
    /// </summary>
    /// <param name="transponderFrequency">The LNB-independent transponder frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="transponderPolarisation">The LNB-independent transponder polarisation.</param>
    /// <param name="lnbSelectionTone">The 22 kHz tone state needed to switch/connect to the LNB.</param>
    /// <param name="localOscillatorFrequencyLow">The LNB's first local oscillator frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="localOscillatorFrequencyLow">The LNB's second local oscillator frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="oscillatorSwitchFrequency">The LNB's oscillator switch frequency. The unit is killo-Hertz (kHz).</param>
    /// <param name="bandSelectionTone">The 22 kHz tone that must be supplied to the LNB to select the correct local oscillator.</param>
    /// <param name="bandSelectionPolarisation">The polarisation (voltage) that must be supplied to the LNB to select the correct local oscillator and band.</param>
    void GetTuningParameters(int transponderFrequency, Polarisation transponderPolarisation, Tone22kState lnbSelectionTone, out int localOscillatorFrequencyLow, out int localOscillatorFrequencyHigh, out int oscillatorSwitchFrequency, out Tone22kState bandSelectionTone, out Polarisation bandSelectionPolarisation);
  }
}