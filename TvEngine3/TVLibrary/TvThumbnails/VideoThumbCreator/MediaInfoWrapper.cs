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
using System.IO;
using System.Globalization;
using TvLibrary.Log;

#region API

# endregion

namespace MediaInfoWrapper
{
  public class MediaInfoWrapper
  {
    #region private vars

    private MediaInfo _mI = null;

    //Video
    private double _framerate = 0;
    private int _width = 0;
    private int _height = 0;
    private string _aspectRatio = string.Empty;
    private string _videoCodec = string.Empty;
    private string _scanType = string.Empty;
    private bool _isInterlaced = false;
    private string _videoResolution = string.Empty;
    private int _videoDuration = 0;
    private bool _DVDenabled = false;
    private string _ParseSpeed;

    //Audio
    private int _audioRate = 0;
    private int _audioChannels = 0;
    private string _audioChannelsFriendly = string.Empty;
    private string _audioCodec = string.Empty;

    //Detection
    private bool _hasAudio;
    private bool _hasVideo;

    //Subtitles
    private int _numsubtitles = 0;
    private bool _hasSubtitles = false;
    private static HashSet<string> _subTitleExtensions = new HashSet<string>();

    #endregion

    #region ctor's

/*    public static bool MediaInfoExist()
    {
      string dll = Configuration.Config.GetFolder(Configuration.Config.Dir.Base) + "\\MediaInfo.dll";
      bool enable = File.Exists(dll);
      if (!enable)
      {
        Log.Error("MediaInfoWrapper: disabled because \"{0}\" is missing", dll);
      }
      return enable;
    }*/

    public MediaInfoWrapper(string strFile)
    {
      /*if (!MediaInfoExist())
      {
        return;
      }*/


      try
      {
        _mI = new MediaInfo();
        _mI.Open(strFile);

        if (_videoDuration == 0)
        {
          int.TryParse(_mI.Get(StreamKind.Video, 0, "Duration"), out _videoDuration);
        }
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Inspecting media : {0}", strFile);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: VideoDuration    : {0}", _videoDuration);
      }
      catch (Exception)
      {
        Log.Error(
          "MediaInfoWrapper.MediaInfoWrapper: Error occurred while scanning media: '{0}'",
          strFile);
      }
      finally
      {
        if (_mI != null)
        {
          _mI.Close();
        }
      }
    }

    #endregion

    #region private methods

    private string GetLargestFileInDirectory(string targetDir, string fileMask)
    {
      string largestFile = null;
      long largestSize = 0;
      DirectoryInfo dir = new DirectoryInfo(targetDir);
      try
      {
        FileInfo[] files = dir.GetFiles(fileMask, SearchOption.TopDirectoryOnly);
        foreach (FileInfo file in files)
        {
          long fileSize = file.Length;
          if (fileSize > largestSize)
          {
            largestSize = fileSize;
            largestFile = file.FullName;
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("Error while retrieving files for: {0} {1}" + targetDir, e.Message);
      }
      return largestFile;
    }

    private bool checkHasExternalSubtitles(string strFile)
    {
      if (_subTitleExtensions.Count == 0)
      {
        // load them in first time
        _subTitleExtensions.Add(".aqt");
        _subTitleExtensions.Add(".asc");
        _subTitleExtensions.Add(".ass");
        _subTitleExtensions.Add(".dat");
        _subTitleExtensions.Add(".dks");
        _subTitleExtensions.Add(".idx");
        _subTitleExtensions.Add(".js");
        _subTitleExtensions.Add(".jss");
        _subTitleExtensions.Add(".lrc");
        _subTitleExtensions.Add(".mpl");
        _subTitleExtensions.Add(".ovr");
        _subTitleExtensions.Add(".pan");
        _subTitleExtensions.Add(".pjs");
        _subTitleExtensions.Add(".psb");
        _subTitleExtensions.Add(".rt");
        _subTitleExtensions.Add(".rtf");
        _subTitleExtensions.Add(".s2k");
        _subTitleExtensions.Add(".sbt");
        _subTitleExtensions.Add(".scr");
        _subTitleExtensions.Add(".smi");
        _subTitleExtensions.Add(".son");
        _subTitleExtensions.Add(".srt");
        _subTitleExtensions.Add(".ssa");
        _subTitleExtensions.Add(".sst");
        _subTitleExtensions.Add(".ssts");
        _subTitleExtensions.Add(".stl");
        _subTitleExtensions.Add(".sub");
        _subTitleExtensions.Add(".txt");
        _subTitleExtensions.Add(".vkt");
        _subTitleExtensions.Add(".vsf");
        _subTitleExtensions.Add(".zeg");
      }
      string filenameNoExt = Path.GetFileNameWithoutExtension(strFile);
      try
      {
        foreach (string file in Directory.GetFiles(Path.GetDirectoryName(strFile), filenameNoExt + "*"))
        {
          System.IO.FileInfo fi = new FileInfo(file);
          if (_subTitleExtensions.Contains(fi.Extension.ToLowerInvariant()))
          {
            return true;
          }
        }
      }
      catch (Exception)
      {
        // most likley path not available
      }

      return false;
    }

    private string GetFullCodecName(StreamKind type, int audiotrack)
    {
      string strCodec = _mI.Get(type, 0, "Format").ToUpperInvariant();
      string strCodecVer = _mI.Get(type, 0, "Format_Version").ToUpperInvariant();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = _mI.Get(type, 0, "CodecID").ToUpperInvariant();
      }
      else
      {
        if (!String.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
          string strCodecProf = _mI.Get(type, 0, "Format_Profile").ToUpperInvariant();
          if (type == StreamKind.Video && strCodecProf != "MAIN@MAIN")
          {
            strCodec = (strCodec + " " + strCodecProf).Trim();
          }
        }
      }
      if (type == StreamKind.Audio)
      {
        strCodec =
          (strCodec + " " + _mI.Get(type, audiotrack, "Format_Profile").Split(new char[] {'/'})[0].ToUpperInvariant()).Trim();
      }
      //
      // Workarround because skin engine ( string.equals/string.contains ) doesn't handle the "+" as last digit
      //
      return strCodec.Replace("+", "PLUS");
    }

    private string GetFullCodecName(StreamKind type)
    {
      return GetFullCodecName(type, 0);
    }

    #endregion

    #region public video related properties

    public double Framerate
    {
      get { return _framerate; }
    }

    public int Width
    {
      get { return _width; }
    }

    public int Height
    {
      get { return _height; }
    }

    public string AspectRatio
    {
      get { return _aspectRatio; }
    }

    public string VideoCodec
    {
      get { return _videoCodec; }
    }

    public string ScanType
    {
      get { return _scanType; }
    }

    public bool IsInterlaced
    {
      get { return _isInterlaced; }
    }

    public string VideoResolution
    {
      get { return _videoResolution; }
    }

    public int VideoDuration
    {
      get { return _videoDuration; }
    }

    #endregion

    #region public audio related properties

    public int AudioRate
    {
      get { return _audioRate; }
    }

    public int AudioChannels
    {
      get { return _audioChannels; }
    }

    public string AudioCodec
    {
      get { return _audioCodec; }
    }

    public string AudioChannelsFriendly
    {
      get { return _audioChannelsFriendly; }
    }

    #endregion

    #region public detection properties

    public bool hasVideo
    {
      get { return _hasVideo; }
    }

    public bool hasAudio
    {
      get { return _hasAudio; }
    }

    #endregion

    #region public subtitles related properties

    public int NumSubtitles
    {
      get { return _numsubtitles; }
    }

    public bool HasSubtitles
    {
      get { return _hasSubtitles; }
    }

    #endregion
  }
}