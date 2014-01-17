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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles SAT>IP
  /// terrestrial tuners.
  /// </summary>
  public class TunerSatIpTerrestrial : TunerSatIpBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerSatIpTerrestrial"/> class.
    /// </summary>
    /// <param name="name">The SAT>IP server's name.</param>
    /// <param name="uuid">A unique identifier for the SAT>IP server.</param>
    /// <param name="ipAddress">The SAT>IP server's current IP address.</param>
    /// <param name="sequenceNumber">A unique sequence number or index for this instance.</param>
    public TunerSatIpTerrestrial(string name, string uuid, string ipAddress, int sequenceNumber)
      : base(name + " T/T2 Tuner " + sequenceNumber, uuid + "T" + sequenceNumber, ipAddress, sequenceNumber)
    {
      _tunerType = CardType.DvbT;
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
      this.LogDebug("SAT>IP terrestrial: construct URL");
      DVBTChannel terrestrialChannel = channel as DVBTChannel;
      if (terrestrialChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      StringBuilder url = new StringBuilder();
      // TODO remove pids=all when PID filter support can be added
      // TODO add DVB-T2 support when we can distinguish DVB-T and DVB-T2
      url.Append("rtsp://").Append(_ipAddress).Append(":554/pids=all&msys=dvbt");
      url.Append("&freq=").Append((int)(terrestrialChannel.Frequency / 1000));
      url.Append("&bw=").Append(terrestrialChannel.Bandwidth / 1000);

      // We don't store these tuning parameters as they're not normally required. Can we omit them?
      url.Append("&tmode=8k&mtype=64qam&gi=116fec=34");
      PerformTuning(url.ToString());
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBTChannel;
    }

    #endregion

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      DVBTChannel terrestrialChannel = _currentTuningDetail as DVBTChannel;
      if (terrestrialChannel == null)
      {
        _isSignalPresent = false;
        _isSignalLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      PerformSignalStatusUpdate(((int)(terrestrialChannel.Frequency / 1000)).ToString() + "," + (terrestrialChannel.Bandwidth / 1000).ToString());
    }

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBTChannel();
    }
  }
}