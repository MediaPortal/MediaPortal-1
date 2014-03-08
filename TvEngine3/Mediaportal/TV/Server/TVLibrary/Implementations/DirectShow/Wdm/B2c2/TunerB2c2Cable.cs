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

using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat cable tuners with B2C2 chipsets and WDM drivers.
  /// </summary>
  public class TunerB2c2Cable : TunerB2c2Base
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Cable"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Cable(DeviceInfo info)
      : base(info)
    {
      _tunerType = CardType.DvbC;
    }

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBCChannel;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 cable: set tuning parameters");
      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      int hr = _interfaceTuner.SetFrequency((int)dvbcChannel.Frequency / 1000);
      HResult.ThrowException(hr, "Failed to set frequency.");

      hr = _interfaceTuner.SetSymbolRate(dvbcChannel.SymbolRate);
      HResult.ThrowException(hr, "Failed to set symbol rate.");

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
      hr = _interfaceTuner.SetModulation(modulation);
      HResult.ThrowException(hr, "Failed to set modulation.");

      this.LogDebug("B2C2 cable: apply tuning parameters");
      HResult.ThrowException(_interfaceTuner.SetTunerStatus(), "Failed to apply tuning parameters.");
    }

    #endregion
  }
}