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

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// holds media info for video files in the database
  /// </summary>
  public class VideoFilesMediaInfo
  {
    private string _videoCodec = string.Empty;
    private string _videoResolution = string.Empty;
    private string _aspectRatio = string.Empty;
    private bool _hasSubtitles;
    private string _audioCodec = string.Empty;
    private string _audioChannels = string.Empty;
    private double _duration = 0;

    public string VideoCodec
    {
      get { return _videoCodec; }
      set { _videoCodec = value; }
    }

    public string VideoResolution
    {
      get { return _videoResolution; }
      set { _videoResolution = value; }
    }

    public string AspectRatio
    {
      get { return _aspectRatio; }
      set { _aspectRatio = value; }
    }

    public bool HasSubtitles
    {
      get { return _hasSubtitles; }
      set { _hasSubtitles = value; }
    }

    public string AudioCodec
    {
      get { return _audioCodec; }
      set { _audioCodec = value; }
    }

    public string AudioChannels
    {
      get { return _audioChannels; }
      set { _audioChannels = value; }
    }

    public double Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }
  }
}