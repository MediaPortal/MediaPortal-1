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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for captured channels.
  /// </summary>
  [DataContract]
  public class ChannelCapture : ChannelBase
  {
    #region variables

    [DataMember]
    private CaptureSourceVideo _videoSource = CaptureSourceVideo.None;

    [DataMember]
    private CaptureSourceAudio _audioSource = CaptureSourceAudio.None;

    [DataMember]
    private bool _isVcrSignal = false;

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel's video source.
    /// </summary>
    public CaptureSourceVideo VideoSource
    {
      get
      {
        return _videoSource;
      }
      set
      {
        _videoSource = value;
      }
    }

    /// <summary>
    /// Get/set the channel's audio source.
    /// </summary>
    public CaptureSourceAudio AudioSource
    {
      get
      {
        return _audioSource;
      }
      set
      {
        _audioSource = value;
      }
    }

    /// <summary>
    /// Get/set whether the channel is sourced from a VCR.
    /// </summary>
    public bool IsVcrSignal
    {
      get
      {
        return _isVcrSignal;
      }
      set
      {
        _isVcrSignal = value;
      }
    }

    #endregion

    #region IChannel members

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public override bool IsDifferentTransmitter(IChannel channel)
    {
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (
        captureChannel == null ||
        VideoSource != captureChannel.VideoSource ||
        AudioSource != captureChannel.AudioSource ||
        (
          LogicalChannelNumber != channel.LogicalChannelNumber &&
          (
            VideoSource == CaptureSourceVideo.TunerDefault ||
            AudioSource == CaptureSourceAudio.TunerDefault
          )
        )
      )
      {
        return true;
      }
      return false;
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>, otherwise <c>false</c></returns>
    public override bool Equals(object obj)
    {
      ChannelCapture channel = obj as ChannelCapture;
      if (
        channel == null ||
        !base.Equals(obj) ||
        VideoSource != channel.VideoSource ||
        AudioSource != channel.AudioSource ||
        IsVcrSignal != channel.IsVcrSignal
      )
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// A hash function for this type.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"/></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ VideoSource.GetHashCode() ^
              AudioSource.GetHashCode() ^ IsVcrSignal.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("capture, {0}, video source = {1}, audio source = {2}, is VCR signal = {3}",
                            base.ToString(), VideoSource, AudioSource,
                            IsVcrSignal);
    }

    #endregion
  }
}