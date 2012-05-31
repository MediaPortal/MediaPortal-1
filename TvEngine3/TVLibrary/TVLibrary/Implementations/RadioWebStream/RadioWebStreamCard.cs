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
using System.Collections.Generic;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB;
using TvLibrary.ChannelLinkage;

namespace TvLibrary.Implementations.RadioWebStream
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
      _cardType = CardType.RadioWebStream;
      _stopGraph = true;  // Pause graph not supported.
    }

    #endregion

     /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    protected static bool CheckThreadId()
    {
      return true;
      /* unreachable 
      if (_managedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
      {
        Log.Log.WriteFile("RadioWebStream:Invalid thread id!!!");
        return false;
      }
      return true;
      */
    }

    #region tuning & recording

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public override bool CanTune(IChannel channel)
    {
      if (!channel.IsRadio) return false;
      if ((channel as RadioWebStreamChannel) == null) return false;
      return true;
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("RadioWebStream:  Tune:{0}", channel);
      return null;
    }

    /// <summary>
    /// Scans the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("RadioWebStream:  Scan:{0}", channel);
      return null;
    }

    #endregion

    #region properties

    /// <summary>
    /// Stops the current graph
    /// </summary>
    /// <returns></returns>
    public override void StopGraph()
    {
      if (!CheckThreadId()) return;
    }

    /// <summary>
    /// Gets wether or not card supports pausing the graph.
    /// </summary>
    public override bool SupportsPauseGraph
    {
      get { return false; }
    }

    /// <summary>
    /// Pause the current graph
    /// </summary>
    /// <returns></returns>
    public override void PauseGraph()
    {
      if (!CheckThreadId()) return;
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
      Log.Log.WriteFile("RadioWebStream:Dispose()");
      if (!CheckThreadId()) return;
    }

    #endregion

    #region audio streams

    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        List<IAudioStream> streams = new List<IAudioStream>();
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.Stereo;
        stream.Language = "Stereo";
        streams.Add(stream);
        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public IAudioStream CurrentAudioStream
    {
      get { return AvailableAudioStreams[0]; }
    }

    #endregion
  }
}