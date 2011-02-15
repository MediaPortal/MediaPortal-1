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

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Video stream types
  /// </summary>
  public enum VideoStreamType
  {
    /// <summary>
    /// MPEG2 video
    /// </summary>
    MPEG2,
    /// <summary>
    /// MPEG4 video video
    /// </summary>
    MPEG4,
    /// <summary>
    /// H264 video video
    /// </summary>
    H264,
    /// <summary>
    /// unknown video
    /// </summary>
    Unknown
  }

  /// <summary>
  /// interface which describes a single video stream
  /// </summary>
  public interface IVideoStream
  {
    /// <summary>
    /// gets/sets the video stream type
    /// </summary>
    VideoStreamType StreamType { get; set; }

    /// <summary>
    /// gets/sets the video stream PID
    /// </summary>
    int Pid { get; set; }

    /// <summary>
    /// gets/sets the channel's PCR PID
    /// </summary>
    int PcrPid { get; set; }
  }
}