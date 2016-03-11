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
using System.IO;

namespace MediaPortal.MusicPlayer.BASS
{
  /// <summary>
  /// Various Helper Functions
  /// </summary>
  public static class Utils
  {
    public static MusicStream.FileType GetFileType(string filePath)
    {
      filePath = filePath.Trim().ToLowerInvariant();

      MusicStream.FileType fileType;

      if (String.IsNullOrEmpty(filePath))
      {
        fileType.FileMainType = FileMainType.Unknown;
        fileType.FileSubType = FileSubType.None;
      }

      else if (IsCDDA(filePath))
      {
        fileType.FileMainType = FileMainType.CDTrack;
        fileType.FileSubType = FileSubType.None;
      }

      else if (IsWebStream(filePath))
      {
        fileType.FileMainType = FileMainType.WebStream;
        if (Util.Utils.IsLastFMStream(filePath))
          fileType.FileSubType = FileSubType.LastFmWebStream;
        else
          fileType.FileSubType = FileSubType.None;
      }

      else if (IsASXFile(filePath))
      {
        fileType.FileMainType = FileMainType.WebStream;
        fileType.FileSubType = FileSubType.ASXWebStream;
      }

      else if (IsMODFile(filePath))
      {
        fileType.FileMainType = FileMainType.MODFile;
        fileType.FileSubType = FileSubType.None;
      }

      else if (IsMidiFile(filePath))
      {
        fileType.FileMainType = FileMainType.MidiFile;
        fileType.FileSubType = FileSubType.None;
      }

      else
      {
        fileType.FileMainType = FileMainType.AudioFile;
        fileType.FileSubType = FileSubType.None;
      }
      return fileType;
    }

    private static bool IsMODFile(string filePath)
    {
      string ext = Path.GetExtension(filePath).ToLowerInvariant();

      switch (ext)
      {
        case ".mod":
        case ".mo3":
        case ".it":
        case ".xm":
        case ".s3m":
        case ".mtm":
        case ".umx":
          return true;

        default:
          return false;
      }
    }

    /// <summary>
    /// Is this a MIDI file?
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static bool IsMidiFile(string filePath)
    {
      string ext = Path.GetExtension(filePath).ToLowerInvariant();

      switch (ext)
      {
        case ".midi":
        case ".mid":
        case ".rmi":
        case ".kar":
          return true;

        default:
          return false;
      }
    }

    private static bool IsASXFile(string filePath)
    {
      return (Path.GetExtension(filePath).ToLowerInvariant() == ".asx");
    }

    private static bool IsWebStream(string filePath)
    {
      return
        (filePath.StartsWith(@"http://") ||
        filePath.StartsWith(@"https://") ||
        filePath.StartsWith(@"mms://") ||
        filePath.StartsWith(@"rtsp://"));
    }

    private static bool IsCDDA(string filePath)
    {
      return
        (filePath.IndexOf("cdda:") >= 0 ||
        filePath.IndexOf(".cda") >= 0);
    }
  }
}
