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
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles SAT>IP DVB-C
  /// tuners.
  /// </summary>
  public class TunerSatIpCable : TunerSatIpBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpCable"/> class.
    /// </summary>
    /// <param name="name">The SAT>IP server's name.</param>
    /// <param name="uuid">A unique identifier for the SAT>IP server.</param>
    /// <param name="ipAddress">The SAT>IP server's current IP address.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerSatIpCable(string name, string uuid, string ipAddress, int sequenceNumber)
      : base(name + " C Tuner " + sequenceNumber, uuid + "C" + sequenceNumber, ipAddress, sequenceNumber)
    {
      _tunerType = CardType.DvbC;
      _uuid = uuid;
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("SAT>IP cable: construct URL");
      DVBCChannel cableChannel = channel as DVBCChannel;
      if (cableChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      StringBuilder url = new StringBuilder();
      // TODO remove pids=all when PID filter support can be added
      url.Append("rtsp://").Append(_ipAddress).Append(":554/pids=all&msys=dvbc");
      url.Append("&freq=").Append((int)(cableChannel.Frequency / 1000));
      url.Append("&sr=").Append(cableChannel.SymbolRate);
      if (cableChannel.ModulationType == ModulationType.Mod64Qam)
      {
        url.Append("&mtype=64qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod80Qam)
      {
        url.Append("&mtype=80qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod96Qam)
      {
        url.Append("&mtype=96qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod112Qam)
      {
        url.Append("&mtype=112qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod128Qam)
      {
        url.Append("&mtype=128qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod160Qam)
      {
        url.Append("&mtype=160qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod192Qam)
      {
        url.Append("&mtype=192qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod224Qam)
      {
        url.Append("&mtype=224qam");
      }
      else if (cableChannel.ModulationType == ModulationType.Mod256Qam)
      {
        url.Append("&mtype=256qam");
      }
      else
      {
        this.LogWarn("SAT>IP cable: unsupported modulation type {0}, assuming 256 QAM", cableChannel.ModulationType);
        url.Append("&mtype=256qam");
      }

      PerformTuning(url.ToString());
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBCChannel;
    }

    #endregion

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      DVBCChannel cableChannel = _currentTuningDetail as DVBCChannel;
      if (cableChannel == null)
      {
        _isSignalPresent = false;
        _isSignalLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      // TODO add more tuning details here to avoid any possibility of selecting an incorrect stream
      PerformSignalStatusUpdate(((int)(cableChannel.Frequency / 1000)).ToString());
    }

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBCChannel();
    }
  }
}