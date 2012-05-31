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

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Audio stream types
  /// </summary>
  public enum AudioStreamType
  {
    /// <summary>
    /// MPEG 1 audio
    /// </summary>
    MPEG1,
    /// <summary>
    /// MPEG 2 audio
    /// </summary>
    MPEG2,
    /// <summary>
    /// AC-3 audio
    /// </summary>
    AC3,
    /// <summary>
    /// AAC audio
    /// </summary>
    AAC,
    /// <summary>
    /// LATM-AAC audio
    /// </summary>
    LATMAAC,
    /// <summary>
    /// Extended AC-3 audio
    /// </summary>
    EAC3,
    /// <summary>
    /// unknown audio
    /// </summary>
    Unknown
  }

  /// <summary>
  /// interface which describes a single audio stream
  /// </summary>
  public interface IAudioStream
  {
    /// <summary>
    /// gets/sets the Audio language
    /// </summary>
    string Language { get; set; }

    /// <summary>
    /// gets/sets the audio stream type
    /// </summary>
    AudioStreamType StreamType { get; set; }

    /// <summary>
    /// gets/sets the audio stream PID
    /// </summary>
    UInt16 Pid { get; set; }
  }
}