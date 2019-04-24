#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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
using System.Linq;
using System.Text;
using MediaInfo;
using MediaPortal.Player;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// This class caches the Properties read by Taglib sharp
  /// </summary>
  public class TagCache
  {
    #region Properties

    public string FileType { get; set; }
    public string Codec { get; set; }
    public string BitRateMode { get; set; }
    public int Year { get; set; }
    public int BitRate { get; set; }
    public int DiscId { get; set; }
    public int DiscTotal { get; set; }
    public int Duration { get; set; }
    public int Channels { get; set; }
    public int SampleRate { get; set; }

    #endregion

    #region ctor

    public TagCache()
    {
      FileType = string.Empty;
      Codec = string.Empty;
      BitRateMode = string.Empty;
      Year = 0;
      BitRate = 0;
      DiscId = 0;
      DiscTotal = 0;
      Duration = 0;
      Channels = 0;
      SampleRate = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the Tags and Properties from the TagLib File object
    /// </summary>
    /// <param name="file"></param>
    public void CopyTags(TagLib.File file)
    {
      FileType = file.MimeType.Substring(file.MimeType.IndexOf("/", StringComparison.Ordinal) + 1);
      Codec = file.Properties.Description;
      Year = (int)file.Tag.Year;
      BitRate = file.Properties.AudioBitrate;
      DiscId = (int)file.Tag.Disc;
      DiscTotal = (int)file.Tag.DiscCount;
      Duration = (int) file.Properties.Duration.TotalSeconds;
      Channels = file.Properties.AudioChannels;
      SampleRate = file.Properties.AudioSampleRate;
      BitRateMode = file.Properties.Description.IndexOf("VBR") > -1 ? "VBR" : "CBR";
    }

    /// <summary>
    /// Get properties from MediaInfo
    /// </summary>
    /// <param name="fname"></param>
    public bool CopyMediaInfo(string fname)
    {
      try
      {
        var mi = new MediaInfoWrapper(fname);
        if (!mi.MediaInfoNotloaded)
        { 
          var audioStream = mi.BestAudioStream;
          if (audioStream != null)
          {
            FileType = audioStream.Format;
            Codec = audioStream.Codec.ToString();
            BitRate = (int) (audioStream.Bitrate / 1000);
            Duration = (int) audioStream.Duration.TotalSeconds;
            Channels = audioStream.Channel;
            SampleRate = (int) audioStream.SamplingRate;
            // TODO: Add bitrate mode
            BitRateMode = string.Empty;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    #endregion
  }
}
