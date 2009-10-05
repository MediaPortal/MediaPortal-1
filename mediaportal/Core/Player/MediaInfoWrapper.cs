using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

using MediaPortal.GUI.Library;
using MediaPortal.Profile;

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

    //Audio
    private int _audioRate = 0;
    private int _audioChannels = 0;
    private string _audioChannelsFriendly = string.Empty;
    private string _audioCodec = string.Empty;

    //Subtitles
    private int _numsubtitles = 0;
    private bool _hasSubtitles = false;
    private static List<string> _subTitleExtensions = new List<string>();

    #endregion

    #region ctor's

    public MediaInfoWrapper(string strFile)
    {
      using (Settings xmlreader = new MPSettings())
      {
        _DVDenabled = xmlreader.GetValueAsBool("dvdplayer", "mediainfoused", false);
      }
      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isRadio = Util.Utils.IsLiveRadio(strFile);
      bool isRTSP = Util.Utils.IsRTSP(strFile);                //rtsp for live TV and recordings.
      bool isDVD = Util.Utils.IsDVD(strFile);
      bool isVideo = Util.Utils.IsVideo(strFile);
      bool isAVStream = Util.Utils.IsAVStream(strFile);        //other AV streams

      //currently disabled for all tv/radio/streaming video
      if (isTV || isRadio || isRTSP || isAVStream)
      {
        return;
      }

      //currently mediainfo is only used for local video related material (if enabled)
      if ((!isVideo && !isDVD) || (isDVD && !_DVDenabled))
      {
        return;
      }

      try
      {
        _mI = new MediaInfo();

        if (Util.VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(strFile)))
          strFile = Util.DaemonTools.GetVirtualDrive() + @"\VIDEO_TS\VIDEO_TS.IFO";

        if (strFile.ToLower().EndsWith(".ifo"))
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
            int.TryParse(_mI.Get(StreamKind.General, 0, "PlayTime"), out vobDuration);
            _mI.Close();
            _videoDuration += vobDuration;
          }
          // get all other info from main title's 1st vob
          strFile = mainTitle;
        }

        _mI.Open(strFile);

        NumberFormatInfo providerNumber = new NumberFormatInfo();
        providerNumber.NumberDecimalSeparator = ".";

        //Video
        double.TryParse(_mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, providerNumber, out _framerate);
        int.TryParse(_mI.Get(StreamKind.Video, 0, "Width"), out _width);
        int.TryParse(_mI.Get(StreamKind.Video, 0, "Height"), out _height);
        _aspectRatio = _mI.Get(StreamKind.Video, 0, "AspectRatio/String") == "4/3" ? "fullscreen" : "widescreen";
        _videoCodec = _mI.Get(StreamKind.Video, 0, "Codec").ToUpper();
        _scanType = _mI.Get(StreamKind.Video, 0, "ScanType").ToLower();
        _isInterlaced = _scanType.Contains("interlaced");

        _videoResolution = "SD";
        if ((_width == 1280 || _height == 720) && !_isInterlaced)
        {
          _videoResolution = "720P";
        } if ((_width == 1920 || _height == 1080) && !_isInterlaced)
        {
          _videoResolution = "1080P";
        } if ((_width == 1920 || _height == 1080) && _isInterlaced)
        {
          _videoResolution = "1080I";
        }

        if (_videoDuration == 0)
        {
          int.TryParse(_mI.Get(StreamKind.Video, 0, "PlayTime"), out _videoDuration);
        }

        //Audio
        int iAudioStreams = _mI.Count_Get(StreamKind.Audio);
        for (int i = 0; i < iAudioStreams; i++)
        {
          int intValue;
          if (int.TryParse(_mI.Get(StreamKind.Audio, i, "Channel(s)"), out intValue) && intValue > _audioChannels)
          {
            int.TryParse(_mI.Get(StreamKind.Audio, i, "SamplingRate"), out _audioRate);
            _audioChannels = intValue;
            _audioCodec = _mI.Get(StreamKind.Audio, i, "Codec/String").ToUpper();
          }
        }

        switch (_audioChannels)
        {
          case 8:
            _audioChannelsFriendly = "7.1";
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

        //Subtitles
        int.TryParse(_mI.Get(StreamKind.General, 0, "TextCount"), out _numsubtitles);

        if (checkHasExternalSubtitles(strFile))
        {
          _hasSubtitles = true;
        }
        else
        {
          _hasSubtitles = _numsubtitles > 0;
        }

        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Inspecting media : {0}", strFile);
        //Video
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: FrameRate        : {0}", _framerate);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Width            : {0}", _width);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Height           : {0}", _height);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AspectRatio      : {0}", _aspectRatio);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: VideoCodec       : {0}", _videoCodec);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Scan type        : {0}", _scanType);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsInterlaced     : {0}", _isInterlaced);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: VideoResolution  : {0}", _videoResolution);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: VideoDuration    : {0}", _videoDuration);
        //Audio
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AudioRate        : {0}", _audioRate);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AudioChannels    : {0}", _audioChannels);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AudioCodec       : {0}", _audioCodec);
        //Subtitles
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: HasSubtitles     : {0}", _hasSubtitles);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: NumSubtitles     : {0}", _numsubtitles);
      }
      catch (Exception ex)
      {
        Log.Error(
          "MediaInfoWrapper.MediaInfoWrapper: unable to call external DLL - mediainfo (make sure 'MediaInfo.dll' is located in MP root dir.) {0}",
          ex.Message);
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
          if (_subTitleExtensions.Contains(fi.Extension.ToLower()))
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
