#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace MediaPortal.Core.Transcoding
{
  public enum VideoFormat
  {
    Dvr_ms,
    Mpeg2,
    Wmv,
    Divx,
    MP4
  } ;

  public enum Quality
  {
    Portable = 0,
    Low,
    Medium,
    High,
    VeryHigh,
    HiDef,
    Custom
  }

  public enum Standard
  {
    Film,
    NTSC,
    PAL
  } ;

  /// <summary>
  /// Class giving all information for a file
  /// </summary>
  public class TranscodeInfo
  {
    public string file = string.Empty; //local filename+path
    public string Author = string.Empty; //author of file
    public string Copyright = string.Empty; //copyright notice
    public string Description = string.Empty; //description of file
    public string Rating = string.Empty; //rating for file
    public string Title = string.Empty; //title of file
    public string Channel = string.Empty; //TVChannel name
    public int Duration = -1; //duration in secs
    public DateTime Start; //Start time&date of recording
    public DateTime End; //end time&date of recording
  }

  /// <summary>
  /// interface for all transcoders.
  /// Any transcoder should implement this and make sure that TranscodeFactory can create such ITranscode instance
  /// </summary>
  public interface ITranscode
  {
    /// <summary>
    /// Check if the requested video format is supported by this transcoder
    /// </summary>
    /// <param name="format">Video format</param>
    /// <returns>true: transcoder can encode a file to the requested video format</returns>
    /// <returns>true: transcoder cannot encode a file to the requested video format</returns>
    bool Supports(VideoFormat format);

    /// <summary>
    /// Transcode a file
    /// </summary>
    /// <param name="info">instance of TranscodeInfo which gives all info of the file to transcode</param>
    /// <param name="format">Video format to which the file needs tobe converted</param>
    /// <param name="quality">Desired quality</param>
    /// <returns>
    /// true: transcoding has started
    /// false:failed to transcode file
    /// </returns>
    bool Transcode(TranscodeInfo info, VideoFormat format, Quality quality, Standard standard);

    /// <summary>
    /// Property to check if transcoding has finished
    /// </summary>
    /// <returns>
    /// false: transcoding still busy
    /// true: transcoding has ended
    /// </returns>
    bool IsFinished();

    /// <summary>
    /// Property which returns how many % of the total amount of work has been done
    /// </summary>
    /// <returns>0-100</returns>
    int Percentage();


    /// <summary>
    /// Property to check if we're transcoding
    /// </summary>
    /// <returns>
    /// true: file is being transcoded
    /// false: idle
    /// </returns>
    bool IsTranscoding();
  }
}