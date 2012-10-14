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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.RadioWebStream
{
  /// <summary>
  /// Dummy card for radio web streams
  /// Timeshifting is not supported, the stream is played back on the client
  /// Recording is supported
  /// </summary>
  public class RadioWebStreamCard : TvCardBase
  {
    #region variables

    private DateTime _dateRecordingStarted = DateTime.MinValue;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioWebStreamCard"/> class.
    /// </summary>
    public RadioWebStreamCard()
      : base(null)
    {
      _name = "RadioWebStream Card (builtin)";
      _devicePath = "(builtin)";
      _supportsSubChannels = true;
      _isHybrid = false;
      _isScanning = false;
      _epgGrabbing = false;
      _tunerType = CardType.RadioWebStream;
      _idleMode = DeviceIdleMode.Stop;
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (channel is RadioWebStreamChannel && channel.MediaType == MediaTypeEnum.Radio)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the subchannel associated with the tuned channel</returns>
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.WriteFile("RadioWebStream:  Tune:{0}", channel);
      return null;
    }

    /// <summary>
    /// Scan a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being scanned.</param>
    /// <param name="channel">The channel to scan.</param>
    /// <returns>the subchannel associated with the scanned channel</returns>
    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.WriteFile("RadioWebStream:  Scan:{0}", channel);
      return null;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      Log.WriteFile("RadioWebStream: perform tuning");
    }

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="channel">The service or channel to associate with the subchannel.</param>
    /// <returns>a handle for the subchannel</returns>
    protected override int CreateNewSubChannel(IChannel channel)
    {
      return -1;
    }

    #endregion

    #region properties

    /// <summary>
    /// Stops the current graph
    /// </summary>
    /// <returns></returns>
    public override void Stop()
    {
    }

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected override void UpdateSignalStatus(bool force)
    {
      _tunerLocked = true;
      _signalLevel = 100;
      _signalQuality = 100;
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public override void Dispose()
    {
      Log.WriteFile("RadioWebStream:Dispose()");
    }

    #endregion
  }
}