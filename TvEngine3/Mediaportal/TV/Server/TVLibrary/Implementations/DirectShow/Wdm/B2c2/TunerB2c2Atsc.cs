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

using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for TechniSat ATSC tuners with
  /// B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerB2c2Atsc : TunerB2c2Base
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Atsc"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Atsc(DeviceInfo info)
      : base(info, BroadcastStandard.Atsc | BroadcastStandard.Scte)
    {
    }

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (!base.CanTune(channel))
      {
        return false;
      }

      // Channels delivered using switched digital video are not supported.
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel != null && scteChannel.Frequency <= 0)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("B2C2 ATSC: set tuning parameters");
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      ChannelScte scteChannel = channel as ChannelScte;
      if (atscChannel == null && scteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      lock (_tunerAccessLock)
      {
        TvExceptionDirectShowError.Throw(_interfaceData.SelectDevice(_deviceInfo.DeviceId), "Failed to select device.");

        int frequency;
        Modulation modulation;
        if (atscChannel != null)
        {
          frequency = atscChannel.Frequency;
          switch (atscChannel.ModulationScheme)
          {
            case ModulationSchemeVsb.Vsb8:
              modulation = Modulation.Vsb8;
              break;
            case ModulationSchemeVsb.Vsb16:
              modulation = Modulation.Vsb16;
              break;
            case ModulationSchemeVsb.Automatic:
              this.LogWarn("B2C2 ATSC: falling back to unknown modulation scheme");
              modulation = Modulation.Unknown;
              break;
            default:
              modulation = (Modulation)atscChannel.ModulationScheme;
              break;
          }
        }
        else
        {
          frequency = scteChannel.Frequency;
          switch (scteChannel.ModulationScheme)
          {
            case ModulationSchemeQam.Qam16:
              modulation = Modulation.Qam16;
              break;
            case ModulationSchemeQam.Qam32:
              modulation = Modulation.Qam32;
              break;
            case ModulationSchemeQam.Qam64:
              modulation = Modulation.Qam64AnnexB;
              break;
            case ModulationSchemeQam.Qam128:
              modulation = Modulation.Qam128;
              break;
            case ModulationSchemeQam.Qam256:
              modulation = Modulation.Qam256AnnexB;
              break;
            case ModulationSchemeQam.Qam512:
            case ModulationSchemeQam.Qam1024:
            case ModulationSchemeQam.Qam2048:
            case ModulationSchemeQam.Qam4096:
              this.LogWarn("B2C2 ATSC: unsupported modulation scheme {0}, falling back to unknown", scteChannel.ModulationScheme);
              modulation = Modulation.Unknown;
              break;
            case ModulationSchemeQam.Automatic:
              this.LogWarn("B2C2 ATSC: falling back to unknown modulation scheme");
              modulation = Modulation.Unknown;
              break;
            default:
              modulation = (Modulation)scteChannel.ModulationScheme;
              break;
          }
        }

        TvExceptionDirectShowError.Throw(_interfaceTuner.SetFrequency(frequency / 1000), "Failed to set frequency.");
        TvExceptionDirectShowError.Throw(_interfaceTuner.SetModulation(modulation), "Failed to set modulation.");
        base.PerformTuning(channel);
      }
    }

    #endregion

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Atsc | StreamFormat.Scte;
      }
      return base.PerformLoading(streamFormat);
    }

    #endregion
  }
}