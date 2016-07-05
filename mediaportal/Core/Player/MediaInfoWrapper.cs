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
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

#region API

# endregion

namespace MediaPortal.Player
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
    private bool _BDenabled = false;
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

    private bool _mediaInfoNotloaded = false;

    #endregion

    #region ctor's

    public static bool MediaInfoExist()
    {
      string dll = Configuration.Config.GetFolder(Configuration.Config.Dir.Base) + "\\MediaInfo.dll";
      bool enable = File.Exists(dll);
      if (!enable)
      {
        Log.Warn("MediaInfoWrapper: disabled because \"{0}\" is missing", dll);
      }
      return enable;
    }

    public MediaInfoWrapper(string strFile)
    {
      if (!MediaInfoExist())
      {
        _mediaInfoNotloaded = true;
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        _DVDenabled = xmlreader.GetValueAsBool("dvdplayer", "mediainfoused", false);
        _BDenabled = xmlreader.GetValueAsBool("bdplayer", "mediainfoused", false);
        _ParseSpeed = xmlreader.GetValueAsString("debug", "MediaInfoParsespeed", "0.3");
        // fix delay introduced after 0.7.26: http://sourceforge.net/tracker/?func=detail&aid=3013548&group_id=86862&atid=581181
      }
      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isRadio = Util.Utils.IsLiveRadio(strFile);
      bool isRTSP = Util.Utils.IsRTSP(strFile); //rtsp for live TV and recordings.
      bool isDVD = Util.Utils.IsDVD(strFile);
      bool isVideo = Util.Utils.IsVideo(strFile);
      bool isAVStream = Util.Utils.IsAVStream(strFile); //other AV streams

      //currently disabled for all tv/radio/streaming video
      if (isTV || isRadio || isRTSP || isAVStream)
      {
        Log.Debug("MediaInfoWrapper: isTv:{0}, isRadio:{1}, isRTSP:{2}, isAVStream:{3}", isTV, isRadio, isRTSP,
                  isAVStream);
        Log.Debug("MediaInfoWrapper: disabled for this content");
        _mediaInfoNotloaded = true;
        return;
      }

      if (strFile.ToLowerInvariant().EndsWith(".wtv"))
      {
        Log.Debug("MediaInfoWrapper: WTV file is not handled");
        _mediaInfoNotloaded = true;
        return;
      }

      // Check if video file is from image file
      string vDrive = DaemonTools.GetVirtualDrive();
      string bDrive = Path.GetPathRoot(strFile);

      if (vDrive == Util.Utils.RemoveTrailingSlash(bDrive))
        isDVD = false;

      //currently mediainfo is only used for local video related material (if enabled)
      if ((!isVideo && !isDVD) || (isDVD && !_DVDenabled) || (isDVD && _BDenabled))
      {
        Log.Debug("MediaInfoWrapper: isVideo:{0}, isDVD:{1}[enabled:{2}]", isVideo, isDVD, _DVDenabled);
        Log.Debug("MediaInfoWrapper: disabled for this content");
        _mediaInfoNotloaded = true;
        return;
      }

      try
      {
        _mI = new MediaInfo();
        _mI.Option("ParseSpeed", _ParseSpeed);

        if (Util.VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(strFile)))
        {
          strFile = Util.DaemonTools.GetVirtualDrive() + @"\VIDEO_TS\VIDEO_TS.IFO";

          if (!File.Exists(strFile))
          {
            strFile = Util.DaemonTools.GetVirtualDrive() + @"\BDMV\index.bdmv";

            if (!File.Exists(strFile))
            {
              _mediaInfoNotloaded = true;
              return;
            }
          }
        }
        
        if (strFile.ToLowerInvariant().EndsWith(".ifo"))
        {
          string path = Path.GetDirectoryName(strFile);
          string mainTitle = GetLargestFileInDirectory(path, "VTS_*1.VOB");
          string titleSearch = Path.GetFileName(mainTitle);
          titleSearch = titleSearch.Substring(0, titleSearch.LastIndexOf('_')) + "*.VOB";
          string[] vobs = Directory.GetFiles(path, titleSearch, SearchOption.TopDirectoryOnly);

          foreach (string vob in vobs)
          {
            int vobDuration = 0;
            _mI.Open(vob);
            int.TryParse(_mI.Get(StreamKind.General, 0, "Duration"), out vobDuration);
            _mI.Close();
            _videoDuration += vobDuration;
          }
          // get all other info from main title's 1st vob
          strFile = mainTitle;
        }
        else if (strFile.ToLowerInvariant().EndsWith(".bdmv"))
        {
          string path = Path.GetDirectoryName(strFile) + @"\STREAM";
          strFile = GetLargestFileInDirectory(path, "*.m2ts");
        }

        if (strFile != null)
        {
          Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Opening file : {0}", strFile);
          _mI.Open(strFile);
        }
        else
        {
          _mediaInfoNotloaded = true;
          return;
        }

        NumberFormatInfo providerNumber = new NumberFormatInfo();
        providerNumber.NumberDecimalSeparator = ".";

        //Video
        double.TryParse(_mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, providerNumber,
                        out _framerate);
        int.TryParse(_mI.Get(StreamKind.Video, 0, "Width"), out _width);
        int.TryParse(_mI.Get(StreamKind.Video, 0, "Height"), out _height);
        _aspectRatio = _mI.Get(StreamKind.Video, 0, "DisplayAspectRatio");

        if ((_aspectRatio == "4:3") || (_aspectRatio == "1.333"))
        {
          _aspectRatio = "fullscreen";
        }
        else
        {
          _aspectRatio = "widescreen";
        }

        _videoCodec = GetFullCodecName(StreamKind.Video);
        _scanType = _mI.Get(StreamKind.Video, 0, "ScanType").ToLowerInvariant();
        _isInterlaced = _scanType.Contains("interlaced");

        if (_width >= 1280 || _height >= 720)
        {
          _videoResolution = "HD";
        }
        else
        {
          _videoResolution = "SD";
        }

        if (_videoResolution == "HD")
        {
          if ((_width >= 7680 || _height >= 4320) && !_isInterlaced)
          {
            if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\4320P.png")) ||
              File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\resolution\4320P.png")))
            {
              _videoResolution = "4320P";
            }
          }
          else if ((_width >= 3840 || _height >= 2160) && !_isInterlaced)
          {
            if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\2160P.png")) ||
             File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\resolution\2160P.png")))
            {
              _videoResolution = "2160P";
            }
          }
          else if ((_width >= 1920 || _height >= 1080) && _isInterlaced)
          {
            _videoResolution = "1080I";
          }
          else if ((_width >= 1920 || _height >= 1080) && !_isInterlaced)
          {
            _videoResolution = "1080P";
          }
          else if ((_width >= 1280 || _height >= 720) && !_isInterlaced)
          {
            _videoResolution = "720P";
          }
        }
        else 
        {
          if (_height >= 576)
          {
            if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\576.png")) ||
              File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\resolution\576.png")))
            {
              _videoResolution = "576";
            }
          }
          else if (_height >= 480)
          {
            if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\480.png")) ||
              File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\resolution\480.png")))
            {
              _videoResolution = "480";
            }
          }
          else if (_height >= 360)
          {
            if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\360.png")) ||
              File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\resolution\360.png")))
            {
              _videoResolution = "360";
            }
          }
          else if (_height >= 240)
          {
            if (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\240.png")) ||
              File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\Media\Logos\resolution\240.png")))
            {
              _videoResolution = "240";
            }
          }
        }

        if (_videoDuration == 0)
        {
          int.TryParse(_mI.Get(StreamKind.Video, 0, "Duration"), out _videoDuration);
        }

        //Audio
        int iAudioStreams = _mI.Count_Get(StreamKind.Audio);
        for (int i = 0; i < iAudioStreams; i++)
        {
          int intValue;

          string sChannels = _mI.Get(StreamKind.Audio, i, "Channel(s)").Split(new char[] {'/'})[0].Trim();

          if (int.TryParse(sChannels, out intValue) && intValue > _audioChannels)
          {
            int.TryParse(_mI.Get(StreamKind.Audio, i, "SamplingRate"), out _audioRate);
            _audioChannels = intValue;
            _audioCodec = GetFullCodecName(StreamKind.Audio, i);
          }
        }

        switch (_audioChannels)
        {
          case 8:
            _audioChannelsFriendly = "7.1";
            break;
          case 7:
            _audioChannelsFriendly = "6.1";
            break;
          case 6:
            _audioChannelsFriendly = "5.1";
            break;
          case 2:
            _audioChannelsFriendly = "stereo";
            break;
          case 1:
            _audioChannelsFriendly = "mono";
            break;
          default:
            _audioChannelsFriendly = _audioChannels.ToString();
            break;
        }

        //Detection
        _hasAudio = _mI.Count_Get(StreamKind.Audio) > 0;
        _hasVideo = _mI.Count_Get(StreamKind.Video) > 0;

        //Subtitles
        _numsubtitles = _mI.Count_Get(StreamKind.Text);

        if (checkHasExternalSubtitles(strFile))
        {
          _hasSubtitles = true;
        }
        else
        {
          _hasSubtitles = _numsubtitles > 0;
        }

        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: DLL Version      : {0}", _mI.Option("Info_Version"));
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Inspecting media : {0}", strFile);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Parse speed      : {0}", _ParseSpeed);
        //Video
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: FrameRate        : {0}", _framerate);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Width            : {0}", _width);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Height           : {0}", _height);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: AspectRatio      : {0}", _aspectRatio);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: VideoCodec       : {0} [ \"{1}.png\" ]", _videoCodec,
                 Util.Utils.MakeFileName(_videoCodec).ToLowerInvariant());
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Scan type        : {0}", _scanType);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: IsInterlaced     : {0}", _isInterlaced);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: VideoResolution  : {0}", _videoResolution);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: VideoDuration    : {0}", _videoDuration);
        //Audio
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: AudioRate        : {0}", _audioRate);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: AudioChannels    : {0} [ \"{1}.png\" ]", _audioChannels,
                 _audioChannelsFriendly);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: AudioCodec       : {0} [ \"{1}.png\" ]", _audioCodec,
                 Util.Utils.MakeFileName(_audioCodec).ToLowerInvariant());
        //Detection
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: HasAudio         : {0}", _hasAudio);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: HasVideo         : {0}", _hasVideo);
        //Subtitles
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: HasSubtitles     : {0}", _hasSubtitles);
        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: NumSubtitles     : {0}", _numsubtitles);
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
          Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Closing file : {0}", strFile);
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

    public bool MediaInfoNotloaded
    {
      get { return _mediaInfoNotloaded; }
    }

    #endregion
  }
}