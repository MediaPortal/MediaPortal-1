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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations
{
  /// <summary>
  /// This class implements tuning parameter calculation and conversion
  /// </summary>
  public static class SatelliteLnbHandler
  {
    // LNB frequency settings used for tuning. ThThese are Ku band "universal"
    // LNB settings, chosen because they're very well supported by tuner
    // drivers and en. The unit is kilo-Hertz (kHz).
    public const int LOW_BAND_LOF = 9750000;
    public const int HIGH_BAND_LOF = 10600000;
    public const int SWITCH_FREQUENCY = 11700000;

    public static void Convert(ref IChannelSatellite channel, int satIpSource, int lnbLowBandLof, int lnbHighBandLof, int lnbSwitchFrequency, bool isBandStackedLnb, Tone22kState toneState, bool isToroidalDish)
    {
      if (satIpSource > 0)
      {
        Log.Debug("LNB: convert, SAT>IP source = {0}, polarisation = {1}, is toroidal dish = {2}", satIpSource, channel.Polarisation, isToroidalDish);
        channel.Longitude = satIpSource;
      }
      else
      {
        Log.Debug("LNB: convert, transponder frequency = {0} kHz, polarisation = {1}, tone state = {2}, is toroidal dish = {3}", channel.Frequency, channel.Polarisation, toneState, isToroidalDish);
        Log.Debug("LNB: LNB settings, low band LOF = {0} kHz, high band LOF = {1} kHz, switch frequency = {2} kHz, is band-stacked = {3}", lnbLowBandLof, lnbHighBandLof, lnbSwitchFrequency, isBandStackedLnb);
      }

      // Circularly polarised signals are inverted by toroidal (dual-reflector)
      // dishes.
      if (isToroidalDish)
      {
        if (channel.Polarisation == Polarisation.CircularLeft)
        {
          channel.Polarisation = Polarisation.CircularRight;
        }
        else if (channel.Polarisation == Polarisation.CircularRight)
        {
          channel.Polarisation = Polarisation.CircularLeft;
        }
      }

      if (satIpSource > 0)
      {
        // LNB (and DiSEqC) config is handled by the SAT>IP server. We have to
        // trust the user to configure it correctly, and trust the SAT>IP
        // server to handle band-stacked LNBs etc. properly.
        return;
      }

      // Determine the local oscillator frequency and tone state.
      // ---------------------------------------------------------
      // Band-stacked LNBs simultaneously down-convert all received signal.
      // Interference in the intermediate frequency range is avoided by the use
      // of two LOFs - one for each polarity - and careful LOF selection. To
      // access transponders with horizontal or circular left polarity we use
      // the "high band" LOF; otherwise we use the "low band" LOF. The only
      // other unusual requirement for band-stacked LNBs is that they must
      // always be supplied with 18 Volts for reliable operation.
      int localOscillatorFrequency = LOW_BAND_LOF;
      bool isToneOn = false;
      if (isBandStackedLnb)
      {
        if (IsHighVoltage(channel.Polarisation))
        {
          localOscillatorFrequency = lnbHighBandLof;
        }
        else
        {
          localOscillatorFrequency = lnbLowBandLof;
        }
        channel.Polarisation = Polarisation.LinearHorizontal;   // 18 Volts
        if (toneState == Tone22kState.On)
        {
          isToneOn = true;
        }
      }
      // Dual-oscillator LNBs down-convert one quarter of the received signal.
      // The desired quarter must be selected using:
      // 1. Voltage
      // 13 Volts for transponders with vertical and circular right polarity.
      // 18 Volts for transponders with horizontal and circular left polarity.
      // 2. 22 kHz tone
      // Off for transponders in the low band of the input frequency range.
      // On for transponders in the high band of the input frequency range.
      // We use the low band LOF to access low band transponders, and the high
      // band LOF to access high band transponders.
      else if (lnbSwitchFrequency > 0)
      {
        if (toneState != Tone22kState.Automatic)
        {
          throw new TvException("Not possible to use a 22 kHz tone switch with a universal LNB.");
        }
        if (channel.Frequency >= lnbSwitchFrequency)
        {
          localOscillatorFrequency = lnbHighBandLof;
          isToneOn = true;
        }
        else
        {
          localOscillatorFrequency = lnbLowBandLof;
        }
      }
      else
      {
        localOscillatorFrequency = lnbLowBandLof;
        isToneOn = toneState == Tone22kState.On;
      }

      // Calculate the intermediate frequency and virtual transponder frequency
      // for a universal LNB.
      int intermediateFrequency = Math.Abs(localOscillatorFrequency - channel.Frequency);
      if (intermediateFrequency < 950 || intermediateFrequency > 2150)
      {
        throw new TvException("Invalid intermediate frequency result. Transponder frequency = {0} kHz, LOF = {1} kHz.", channel.Frequency, localOscillatorFrequency);
      }
      if (isToneOn)
      {
        channel.Frequency = HIGH_BAND_LOF + intermediateFrequency;
      }
      else
      {
        channel.Frequency = LOW_BAND_LOF + intermediateFrequency;
      }
      Log.Debug("LNB: active LOF = {0} kHz, tone state = {1}, polarisation = {2}, tuning frequency = {3}", localOscillatorFrequency, toneState, channel.Polarisation, channel.Frequency);
    }

    public static bool Is22kToneOn(int transponderFrequency)
    {
      return transponderFrequency >= SWITCH_FREQUENCY;
    }

    public static int GetLocalOscillatorFrequency(int transponderFrequency)
    {
      if (Is22kToneOn(transponderFrequency))
      {
        return HIGH_BAND_LOF;
      }
      return LOW_BAND_LOF;
    }

    public static bool IsHighVoltage(Polarisation polarisation)
    {
      return polarisation == Polarisation.LinearHorizontal || polarisation == Polarisation.CircularLeft;
    }
  }
}