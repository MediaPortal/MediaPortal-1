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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat terrestrial tuners with B2C2 chipsets and WDM drivers.
  /// </summary>
  public class TunerB2c2Terrestrial : TunerB2c2Base
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Terrestrial"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Terrestrial(DeviceInfo info)
      : base(info)
    {
      _tunerType = CardType.DvbT;
    }

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBTChannel;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 terrestrial: set tuning parameters");
      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      int hr = _interfaceTuner.SetFrequency((int)dvbtChannel.Frequency / 1000);
      HResult.ThrowException(hr, "Failed to set frequency.");

      hr = _interfaceTuner.SetBandwidth(dvbtChannel.Bandwidth / 1000);
      HResult.ThrowException(hr, "Failed to set bandwidth.");

      // Note: it is not guaranteed that guard interval auto detection is supported, but if it isn't
      // then we can't tune - we have no idea what the actual value should be.
      hr = _interfaceTuner.SetGuardInterval(B2c2GuardInterval.Auto);
      HResult.ThrowException(hr, "Failed to use automatic guard interval detection.");

      this.LogDebug("B2C2 terrestrial: apply tuning parameters");
      HResult.ThrowException(_interfaceTuner.SetTunerStatus(), "Failed to apply tuning parameters.");
    }

    #endregion
  }
}