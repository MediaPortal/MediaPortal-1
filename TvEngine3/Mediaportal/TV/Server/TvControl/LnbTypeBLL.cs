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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVControl
{
  /// <summary>
  /// The base implementation of <see cref="T:ILnbType"/>.
  /// </summary>
  [Serializable]
  public class LnbTypeBLL : ILnbType
  {
    private LnbType _lnbType = null;

    public LnbTypeBLL(LnbType lnbType)
    {
      _lnbType = lnbType;
    }

    #region ILnbType members

    // Golden rule: you should never pass zero to a BDA tuner driver for any
    // LNB frequency setting. Very often people who don't know any better will
    // set the high oscillator frequency and/or switch frequency for single
    // oscillator LNBs to zero to indicate that they are irrelevant. Driver
    // behaviour with respect to the 22 kHz tone should be considered undefined
    // in that situation. In some cases the driver behaviour wouldn't matter,
    // however consider:
    // - some single oscillator LNBs have been known to respond to the 22 kHz
    //    tone
    // - the tone may degrade the signal quality
    // - the 22 kHz tone state is still important in an environment with mixed
    //    LNB types or 22 kHz tone switches
    // 
    // We set the high oscillator frequency to [low LOF] + 500000 kHz for
    // single oscillator LNBs. Our intention is to ensure that the low and high
    // oscillator frequencies are different as some drivers (for example Anysee
    // E7, SkyStar 2 [BDA]) don't turn the 22 kHz tone on or off (!!!) if the
    // frequencies are not different. Other drivers (for example KNC TV-Station
    // PCI) require that the frequencies be the same in order to turn the 22
    // kHz tone off - these drivers should be handled with ILnbType overrides
    // or wrappers in tuner extensions. Note that the 500000 kHz value is
    // arbitrary.

    /// <summary>
    /// Get the LNB type's identifier.
    /// </summary>
    public int Id
    {
      get
      {
        return _lnbType.IdLnbType;
      }
    }

    /// <summary>
    /// Get the LNB type's name.
    /// </summary>
    public string Name
    {
      get
      {
        return _lnbType.Name;
      }
    }

    /// <summary>
    /// Get the local LNB-dependent tuning parameters for a satellite
    /// transponder.
    /// </summary>
    /// <param name="transponderFrequency">The LNB-independent transponder frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="transponderPolarisation">The LNB-independent transponder polarisation.</param>
    /// <param name="lnbSelectionTone">The 22 kHz tone state needed to switch/connect to the LNB.</param>
    /// <param name="localOscillatorFrequency">The local oscillator frequency that the LNB will use for downconversion. The unit is kilo-Hertz (kHz).</param>
    /// <param name="bandSelectionTone">The 22 kHz tone that must be supplied to the LNB to select the correct local oscillator.</param>
    /// <param name="bandSelectionPolarisation">The polarisation (voltage) that must be supplied to the LNB to select the correct local oscillator and band.</param>
    public void GetTuningParameters(int transponderFrequency, Polarisation transponderPolarisation, Tone22kState lnbSelectionTone, out int localOscillatorFrequency, out Tone22kState bandSelectionTone, out Polarisation bandSelectionPolarisation)
    {
      this.LogDebug("LNB: settings, low LOF = {0} kHz, high LOF = {1} kHz, switch = {2} kHz, bandstacked = {3}",
                      _lnbType.LowBandFrequency, _lnbType.HighBandFrequency,
                      _lnbType.SwitchFrequency, _lnbType.IsBandStacked);

      if (_lnbType.SwitchFrequency > 0 && lnbSelectionTone != Tone22kState.Automatic)
      {
        // Universal LNBs have a 22 kHz tone switch built in.
        throw new TvException("Not possible to use a 22 kHz tone switch with a universal LNB.");
      }
      if (transponderFrequency >= 7000000 && transponderFrequency - _lnbType.LowBandFrequency < 950000)
      {
        throw new TvException("Invalid transponder and LNB combination. C band transponder with Ku band LNB setting?");
      }
      else if (transponderFrequency < 7000000 && _lnbType.LowBandFrequency - transponderFrequency < 950000)
      {
        throw new TvException("Invalid transponder and LNB combination. Ku band transponder with C band LNB setting?");
      }

      // For bandstacked LNBs, if the transponder polarisation is horizontal
      // or circular left then we should use the nominal high oscillator
      // frequency. Otherwise we use the nominal low oscillator frequency. In
      // addition, we should always supply bandstacked LNBs with 18 V for
      // reliable operation.
      if (_lnbType.IsBandStacked)
      {
        if (transponderPolarisation == Polarisation.LinearHorizontal || transponderPolarisation == Polarisation.CircularLeft)
        {
          localOscillatorFrequency = _lnbType.HighBandFrequency;
        }
        else
        {
          localOscillatorFrequency = _lnbType.LowBandFrequency;
        }
        if (lnbSelectionTone == Tone22kState.Automatic)
        {
          bandSelectionTone = Tone22kState.Off;
        }
        else
        {
          bandSelectionTone = lnbSelectionTone;
        }
        bandSelectionPolarisation = Polarisation.LinearHorizontal;   // 18 V
      }
      else
      {
        localOscillatorFrequency = _lnbType.LowBandFrequency;
        bandSelectionTone = lnbSelectionTone;
        bandSelectionPolarisation = transponderPolarisation;
        if (_lnbType.SwitchFrequency > 0 && transponderFrequency >= _lnbType.SwitchFrequency)
        {
          localOscillatorFrequency = _lnbType.HighBandFrequency;
          bandSelectionTone = Tone22kState.On;
        }
        else if (lnbSelectionTone == Tone22kState.Automatic)
        {
          bandSelectionTone = Tone22kState.Off;
        }
      }
    }

    /// <summary>
    /// Get the local LNB-dependent tuning parameters for a satellite
    /// transponder.
    /// </summary>
    /// <param name="transponderFrequency">The LNB-independent transponder frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="transponderPolarisation">The LNB-independent transponder polarisation.</param>
    /// <param name="lnbSelectionTone">The 22 kHz tone state needed to switch/connect to the LNB.</param>
    /// <param name="localOscillatorFrequency">The local oscillator frequency that the LNB will use for downconversion. The unit is kilo-Hertz (kHz).</param>
    /// <param name="oscillatorSwitchFrequency">The LNB's oscillator switch frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="bandSelectionTone">The 22 kHz tone that must be supplied to the LNB to select the correct local oscillator.</param>
    /// <param name="bandSelectionPolarisation">The polarisation (voltage) that must be supplied to the LNB to select the correct local oscillator and band.</param>
    public void GetTuningParameters(int transponderFrequency, Polarisation transponderPolarisation, Tone22kState lnbSelectionTone, out int localOscillatorFrequency, out int oscillatorSwitchFrequency, out Tone22kState bandSelectionTone, out Polarisation bandSelectionPolarisation)
    {
      GetTuningParameters(transponderFrequency, transponderPolarisation, lnbSelectionTone, out localOscillatorFrequency, out bandSelectionTone, out bandSelectionPolarisation);
      if (_lnbType.SwitchFrequency > 0)
      {
        oscillatorSwitchFrequency = _lnbType.SwitchFrequency;
      }
      // This code might not handle C band LNBs with 22 kHz tone switches
      // properly... or maybe behaviour is driver-dependent.
      else if (bandSelectionTone == Tone22kState.Off)
      {
        // This value is *not* arbitrary. It needs to be greater than both the
        // highest transponder frequency and the high local oscillator
        // frequency. However, some drivers (for example Genpix SkyWalker)
        // treat 20 GHz as a signal to always use high voltage (which is useful
        // for bandstacked LNBs).
        oscillatorSwitchFrequency = 18000000;
      }
      else
      {
        // This value is *not* arbitrary. It needs to be less than the
        // transponder frequency and greater than the high local oscillator
        // frequency.
        oscillatorSwitchFrequency = transponderFrequency - 500000;
      }
    }

    /// <summary>
    /// Get the local LNB-dependent tuning parameters for a satellite
    /// transponder.
    /// </summary>
    /// <param name="transponderFrequency">The LNB-independent transponder frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="transponderPolarisation">The LNB-independent transponder polarisation.</param>
    /// <param name="lnbSelectionTone">The 22 kHz tone state needed to switch/connect to the LNB.</param>
    /// <param name="localOscillatorFrequencyLow">The LNB's first local oscillator frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="localOscillatorFrequencyLow">The LNB's second local oscillator frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="oscillatorSwitchFrequency">The LNB's oscillator switch frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="bandSelectionTone">The 22 kHz tone that must be supplied to the LNB to select the correct local oscillator.</param>
    /// <param name="bandSelectionPolarisation">The polarisation (voltage) that must be supplied to the LNB to select the correct local oscillator and band.</param>
    public void GetTuningParameters(int transponderFrequency, Polarisation transponderPolarisation, Tone22kState lnbSelectionTone, out int localOscillatorFrequencyLow, out int localOscillatorFrequencyHigh, out int oscillatorSwitchFrequency, out Tone22kState bandSelectionTone, out Polarisation bandSelectionPolarisation)
    {
      GetTuningParameters(transponderFrequency, transponderPolarisation, lnbSelectionTone, out localOscillatorFrequencyLow, out oscillatorSwitchFrequency, out bandSelectionTone, out bandSelectionPolarisation);

      // Prefer to use the database LOF settings if available. Otherwise
      // calculate them.
      if (bandSelectionTone == Tone22kState.On)
      {
        localOscillatorFrequencyHigh = localOscillatorFrequencyLow;
        if (_lnbType.SwitchFrequency > 0)
        {
          localOscillatorFrequencyLow = _lnbType.LowBandFrequency;
        }
        else
        {
          localOscillatorFrequencyLow = localOscillatorFrequencyLow - 500000;
        }
      }
      else
      {
        if (_lnbType.SwitchFrequency > 0)
        {
          localOscillatorFrequencyHigh = _lnbType.HighBandFrequency;
        }
        else
        {
          localOscillatorFrequencyHigh = localOscillatorFrequencyLow + 500000;
        }
      }
    }

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the LNB type instance.
    /// </summary>
    /// <returns>a shallow clone of the LNB type instance</returns>
    public object Clone()
    {
      return new LnbTypeBLL(_lnbType.Clone());
    }

    #endregion

    public override string ToString()
    {
      if (_lnbType.SwitchFrequency > 0)
      {
        return string.Format("{0} [{1}, {2}, {3}, {4}]", _lnbType.Name, _lnbType.LowBandFrequency / 1000, _lnbType.HighBandFrequency / 1000, _lnbType.SwitchFrequency / 1000, _lnbType.IsBandStacked);
      }
      if (_lnbType.HighBandFrequency > 0)
      {
        return string.Format("{0} [{1}, {2}, {3}]", _lnbType.Name, _lnbType.LowBandFrequency / 1000, _lnbType.HighBandFrequency / 1000, _lnbType.IsBandStacked);
      }
      return string.Format("{0} [{1}, {2}]", _lnbType.Name, _lnbType.LowBandFrequency / 1000, _lnbType.IsBandStacked);
    }
  }
}