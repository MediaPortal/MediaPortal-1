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

using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for TechniSat cable tuners with
  /// B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerB2c2Cable : TunerB2c2Base
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Cable"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Cable(DeviceInfo info)
      : base(info, BroadcastStandard.DvbC)
    {
    }

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 cable: set tuning parameters");
      ChannelDvbC dvbcChannel = channel as ChannelDvbC;
      if (dvbcChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      lock (_tunerAccessLock)
      {
        TvExceptionDirectShowError.Throw(_interfaceData.SelectDevice(_deviceInfo.DeviceId), "Failed to select device.");
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetFrequency(dvbcChannel.Frequency / 1000), "Failed to set frequency.");
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetSymbolRate(dvbcChannel.SymbolRate), "Failed to set symbol rate.");

        Modulation modulation;
        switch (dvbcChannel.ModulationScheme)
        {
          case ModulationSchemeQam.Qam16:
            modulation = Modulation.Qam16;
            break;
          case ModulationSchemeQam.Qam32:
            modulation = Modulation.Qam32;
            break;
          case ModulationSchemeQam.Qam64:
            modulation = Modulation.Qam64;
            break;
          case ModulationSchemeQam.Qam128:
            modulation = Modulation.Qam128;
            break;
          case ModulationSchemeQam.Qam256:
            modulation = Modulation.Qam256;
            break;
          case ModulationSchemeQam.Qam512:
          case ModulationSchemeQam.Qam1024:
          case ModulationSchemeQam.Qam2048:
          case ModulationSchemeQam.Qam4096:
            this.LogWarn("B2C2 cable: unsupported modulation scheme {0}, falling back to unknown", dvbcChannel.ModulationScheme);
            modulation = Modulation.Unknown;
            break;
          case ModulationSchemeQam.Automatic:
            this.LogWarn("B2C2 cable: falling back to unknown modulation scheme");
            modulation = Modulation.Unknown;
            break;
          default:
            modulation = (Modulation)dvbcChannel.ModulationScheme;
            break;
        }
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetModulation(modulation), "Failed to set modulation.");

        base.PerformTuning(channel);
      }
    }

    #endregion
  }
}