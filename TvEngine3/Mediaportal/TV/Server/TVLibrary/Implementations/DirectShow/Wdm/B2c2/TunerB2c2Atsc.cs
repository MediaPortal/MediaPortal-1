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
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for TechniSat ATSC tuners
  /// with B2C2 chipsets and WDM drivers.
  /// </summary>
  internal class TunerB2c2Atsc : TunerB2c2Base
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TunerB2c2Atsc"/> class.
    /// </summary>
    /// <param name="info">The B2C2-specific information (<see cref="DeviceInfo"/>) about the tuner.</param>
    public TunerB2c2Atsc(DeviceInfo info)
      : base(info, CardType.Atsc)
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
      ATSCChannel atscChannel = channel as ATSCChannel;
      // Channels delivered using switched digital video are not supported.
      if (atscChannel == null || (atscChannel.PhysicalChannel <= 0 && atscChannel.Frequency <= 0))
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
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      lock (_tunerAccessLock)
      {
        HResult.ThrowException(_interfaceData.SelectDevice(_deviceInfo.DeviceId), "Failed to select device.");

        // If the channel modulation scheme is 8 VSB then it is an over-the-air ATSC channel,
        // otherwise it is a cable (SCTE ITU-T annex B) channel.
        int frequency;
        Modulation modulation = Modulation.Vsb8;
        if (atscChannel.ModulationType == ModulationType.Mod8Vsb)
        {
          frequency = ATSCChannel.GetTerrestrialFrequencyFromPhysicalChannel(atscChannel.PhysicalChannel);
          this.LogDebug("B2C2 ATSC: translated ATSC physical channel number {0} to {1} kHz", atscChannel.PhysicalChannel, frequency);
        }
        else
        {
          frequency = (int)atscChannel.Frequency;
          modulation = Modulation.Qam256AnnexB;
          if (atscChannel.ModulationType == ModulationType.Mod64Qam)
          {
            modulation = Modulation.Qam64AnnexB;
          }
        }

        HResult.ThrowException(_interfaceTuner.SetFrequency(frequency / 1000), "Failed to set frequency.");
        HResult.ThrowException(_interfaceTuner.SetModulation(modulation), "Failed to set modulation.");
        base.PerformTuning(channel);
      }
    }

    #endregion

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ICustomDevice> PerformLoading()
    {
      IList<ICustomDevice> extensions = base.PerformLoading();

      // ATSC/SCTE EPG grabbing currently not supported.
      _epgGrabber = null;

      if (_channelScanner != null)
      {
        _channelScanner.Helper = new ChannelScannerHelperAtsc();
      }
      return extensions;
    }

    #endregion
  }
}