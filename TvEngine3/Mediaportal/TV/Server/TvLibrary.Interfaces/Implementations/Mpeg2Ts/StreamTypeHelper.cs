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

using System.Collections.ObjectModel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts
{
  /// <summary>
  /// A simple helper class to centralise logic that needs to distinguish between different stream
  /// categories or types.
  /// </summary>
  public static class StreamTypeHelper
  {
    /// <summary>
    /// Determine whether an elementary stream is a video stream.
    /// </summary>
    /// <param name="streamType">The logical stream type of the elementary stream.</param>
    /// <returns><c>true</c> if the elementary stream is a video stream, otherwise <c>false</c></returns>
    public static bool IsVideoStream(LogicalStreamType streamType)
    {
      if (streamType == LogicalStreamType.VideoMpeg1 ||
        streamType == LogicalStreamType.VideoMpeg2 ||
        streamType == LogicalStreamType.VideoMpeg4Part2 ||
        streamType == LogicalStreamType.VideoMpeg4Part10 ||
        streamType == LogicalStreamType.VideoMpegC ||
        streamType == LogicalStreamType.VideoMpeg4Part10AnnexG ||
        streamType == LogicalStreamType.VideoMpeg4Part10AnnexH ||
        streamType == LogicalStreamType.VideoJpeg ||
        streamType == LogicalStreamType.VideoMpeg2StereoscopicAdditionalView ||
        streamType == LogicalStreamType.VideoMpeg4Part10StereoscopicAdditionalView ||
        streamType == LogicalStreamType.VideoMpegHPart2 ||
        streamType == LogicalStreamType.VideoVc1)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determine whether an elementary stream is an audio stream.
    /// </summary>
    /// <param name="streamType">The logical stream type of the elementary stream.</param>
    /// <returns><c>true</c> if the elementary stream is an audio stream, otherwise <c>false</c></returns>
    public static bool IsAudioStream(LogicalStreamType streamType)
    {
      if (streamType == LogicalStreamType.AudioMpeg1 ||
        streamType == LogicalStreamType.AudioMpeg2 ||
        streamType == LogicalStreamType.AudioMpeg2Part7 ||
        streamType == LogicalStreamType.AudioMpeg4Part3Latm ||
        streamType == LogicalStreamType.AudioMpeg4Part3 ||
        streamType == LogicalStreamType.AudioAc3 ||
        streamType == LogicalStreamType.AudioEnhancedAc3 ||
        streamType == LogicalStreamType.AudioDts ||
        streamType == LogicalStreamType.AudioDtsHd ||
        streamType == LogicalStreamType.AudioAc4)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determine whether an elementary stream is a valid DigiCipher II stream.
    /// </summary>
    /// <param name="streamType">The raw stream type of the elementary stream.</param>
    /// <returns><c>true</c> if the elementary stream is a valid DigiCipher II stream, otherwise <c>false</c></returns>
    public static bool IsValidDigiCipher2Stream(StreamType streamType)
    {
      if (streamType == StreamType.DigiCipher2Video ||
        streamType == StreamType.Ac3Audio ||
        streamType == StreamType.EnhancedAc3Audio ||
        streamType == StreamType.Subtitles)
      {
        return true;
      }
      return false;
    }
  }
}