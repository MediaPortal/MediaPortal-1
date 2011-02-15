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
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// class which holds the video stream details for a channel
  /// </summary>
  [Serializable]
  public class VideoStream : IVideoStream
  {
    #region variables

    private VideoStreamType _streamType;
    private int _videoPid;
    private int _pcrPid;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoStream"/> class.
    /// </summary>
    public VideoStream()
    {
      _streamType = VideoStreamType.MPEG2;
      _videoPid = -1;
      _pcrPid = -1;
    }

    #endregion

    #region properties

    /// <summary>
    /// gets/sets the video stream type
    /// </summary>
    public VideoStreamType StreamType
    {
      get { return _streamType; }
      set { _streamType = value; }
    }

    /// <summary>
    /// gets/sets the video pid for this stream
    /// </summary>
    public int Pid
    {
      get { return _videoPid; }
      set { _videoPid = value; }
    }

    /// <summary>
    /// gets/sets the video pid for this stream
    /// </summary>
    public int PcrPid
    {
      get { return _pcrPid; }
      set { _pcrPid = value; }
    }

    #endregion

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      VideoStream stream = obj as VideoStream;
      if (stream == null)
      {
        return false;
      }
      if (_streamType == stream.StreamType && _videoPid == stream.Pid && _pcrPid == stream.Pid)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return String.Format("pid:{0:X} type:{1} pcr:{2}", Pid, StreamType, PcrPid);
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _streamType.GetHashCode() ^ _videoPid.GetHashCode() ^ _pcrPid;
    }
  }
}