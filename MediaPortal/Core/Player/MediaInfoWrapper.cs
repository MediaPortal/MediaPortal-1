using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using MediaPortal.GUI.Library;

#region API

# endregion

namespace MediaPortal.Player
{
  public class MediaInfoWrapper
  {
    #region private vars

    private  MediaInfo _mI = null;

    private double _framerate = 0;
    private int _width = 0;
    private int _height = 0;
    private int _audiorate = 0;
    private int _audiochannels = 0;
    private string _aspectRatio = "";
    private string _videoCodec = string.Empty;
    private string _audioCodec = string.Empty;
    private string _scanType = string.Empty;
    private bool _isDIVX = false; // mpeg4 DivX
    private bool _isXVID = false; // mpeg4 asp
    private bool _isH264 = false; // mpeg4 avc h264/x264
    private bool _isMP1V = false; // mpeg1 video (VCD)
    private bool _isMP2V = false; // mpeg2 video
    private bool _isMP4V = false; // mpeg4 generic
    private bool _isWMV = false;  // WMV 7-9
    private bool _is720P = false; // is 1280x720 video
    private bool _is1080P = false; // is 1980x1080 video, progressive
    private bool _is1080I = false; // is 1920x1080 video, interlaced
    private bool _isInterlaced = false; // is interlaced
    private bool _isHDTV = false; // is HDTV resolution
    private bool _isSDTV = false; // is SDTV resolution
    private bool _isAC3 = false;  // AC3
    private bool _isMP3 = false;  // MPEG-1 Audio layer 3
    private bool _isMP2A = false; // MPEG-1 Audio layer 2
    private bool _isDTS = false;  // DTS
    private bool _isOGG = false;  // OGG VORBIS
    private bool _isAAC = false;  // AAC
    private bool _isWMA = false;  // Windows Media Audio
    private bool _isPCM = false;  // RAW audio

    private bool _hasSubtitles = false;
    private static List<string> _subTitleExtensions = new List<string>();   

    #endregion

    #region ctor's

    public MediaInfoWrapper(string strFile)
    {

      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isRadio = Util.Utils.IsLiveRadio(strFile);
      bool isDVD = Util.Utils.IsDVD(strFile);
      bool isVideo = Util.Utils.IsVideo(strFile);
      bool isAVStream = Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.

      if (isTV || isRadio || isAVStream)
      {
        return;
      }

      //currently mediainfo is only used for video related material
      if (!isDVD && !isVideo)
      {
        return;
      }

      try
      {
        _hasSubtitles = checkHasSubtitles(strFile);

        _mI = new MediaInfo();
        _mI.Open(strFile);

        NumberFormatInfo providerNumber = new NumberFormatInfo();
        providerNumber.NumberDecimalSeparator = ".";

        double.TryParse(_mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, providerNumber, out _framerate);
        _videoCodec = _mI.Get(StreamKind.Video, 0, "Codec").ToLower();
        _scanType = _mI.Get(StreamKind.Video, 0, "ScanType").ToLower();
        Int32.TryParse(_mI.Get(StreamKind.Video, 0, "Width"), NumberStyles.Integer, providerNumber, out _width);
        Int32.TryParse(_mI.Get(StreamKind.Video, 0, "Height"), NumberStyles.Integer, providerNumber, out _height);
        Int32.TryParse(_mI.Get(StreamKind.Audio, 0, "Audiochannels"), NumberStyles.Integer, providerNumber, out _audiochannels);
        Int32.TryParse(_mI.Get(StreamKind.Audio, 0, "Audiorate"), NumberStyles.Integer, providerNumber, out _audiorate);

        _aspectRatio = _mI.Get(StreamKind.Video, 0, "AspectRatio/String");

        _audioCodec = _mI.Get(StreamKind.Audio, 0, "Codec/String").ToLower();

        _isInterlaced = (_scanType.IndexOf("interlaced") > -1);
        if (_height >= 720)
        {
            _isHDTV = true;
        }
        else
        {
            _isSDTV = true;
        }

        if (_width == 1280 && _height == 720 && _scanType.IndexOf("progressive") > 1)
            _is720P = true;

        if (_width == 1920 && _height == 1080 && _scanType.IndexOf("progressive") > 1)
            _is1080P = true;

        if (_width == 1920 && _height == 1080 && _scanType.IndexOf("interlaced") > 1)
            _is1080I = true;

        _isDIVX = (_videoCodec.IndexOf("dx50") > -1); // DivX 5
        _isXVID = (_videoCodec.IndexOf("xvid") > -1);
        _isH264 = (_videoCodec.IndexOf("avc") > -1);
        _isMP1V = (_videoCodec.IndexOf("mpeg-1v") > -1);
        _isMP2V = (_videoCodec.IndexOf("mpeg-2v") > -1);
        _isMP4V = (_videoCodec.IndexOf("fmp4") > -1); // add more
        _isWMV = (_videoCodec.IndexOf("wmv") > -1); // wmv3 = WMV9
        // missing cvid etc
        _isAC3 = (_audioCodec.IndexOf("ac3") > -1);
        _isMP3 = (_audioCodec.IndexOf("mpeg-1 audio layer 3") > -1);
        _isMP2A = (_audioCodec.IndexOf("mpeg-1 audio layer 2") > -1);
        _isDTS = (_audioCodec.IndexOf("dts") > -1);
        _isOGG = (_audioCodec.IndexOf("ogg") > -1);
        _isAAC = (_audioCodec.IndexOf("aac") > -1);
        _isWMA = (_audioCodec.IndexOf("wma") > -1); // e.g. wma3
        _isPCM = (_audioCodec.IndexOf("pcm") > -1);

        Log.Info("MediaInfoWrapper.MediaInfoWrapper: inspecting media : {0}", strFile);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: FrameRate : {0}", _framerate);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: VideoCodec : {0}", _videoCodec);
        if (_isDIVX)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsDIVX: {0}", _isDIVX);
        if(_isXVID)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsXVID: {0}", _isXVID);
        if (_isH264)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsH264: {0}", _isH264);
        if (_isMP1V)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP1V: {0}", _isMP1V);
        if (_isMP2V)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP2V: {0}", _isMP2V);
          if (_isMP4V)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP4V: {0}", _isMP4V);
        if (_isWMV)
          Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsWMV: {0}", _isWMV);

        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Scan type : {0}", _scanType);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsInterlaced: {0}", _isInterlaced);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Width : {0}", _width);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Height : {0}", _height);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Audiochannels : {0}", _audiochannels);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Audiorate : {0}", _audiorate);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AspectRatio : {0}", _aspectRatio);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AudioCodec : {0}", _audioCodec);
        if(_isAC3)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsAC3 : {0}", _isAC3);
        if(_isMP3)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP3 : {0}", _isMP3);
        if(_isMP2A)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP2A: {0}", _isMP2A);
        if(_isDTS)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsDTS : {0}", _isDTS);
        if(_isOGG)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsOGG : {0}", _isOGG);
        if(_isAAC)
            Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsAAC : {0}", _isAAC);
        if (_isWMA)
          Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsWMA: {0}", _isWMA);
        if (_isPCM)
          Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsPCM: {0}", _isPCM);
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

    private bool checkHasSubtitles(string strFile)
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
      string filenameNoExt = System.IO.Path.GetFileNameWithoutExtension(strFile);
      try
      {
        foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(strFile), filenameNoExt + "*"))
        {
          System.IO.FileInfo fi = new System.IO.FileInfo(file);
          if (_subTitleExtensions.Contains(fi.Extension.ToLower())) return true;
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

    public string AspectRatio
    {
      get { return _aspectRatio; }
    }

    public string VideoCodec
    {
      get { return _videoCodec; }
    }

    public double Framerate
    {
      get { return _framerate; }
    }

    public bool IsDIVX
    {
      get { return _isDIVX; }
    }
    
    public bool IsXVID
    {
      get { return _isXVID; }
    }

    public bool IsH264
    {
       get { return _isH264; }
    }

    public bool IsMP1V
    {
     get { return _isMP1V; }
    }
    
    public bool IsMP2V
    {
      get { return _isMP2V; }
    }

    public bool IsMP4V
    {
      get { return _isMP4V; }
    }

    public bool IsWMV
    {
      get { return _isWMV; }
    }

    public bool Is720P
    {
      get { return _is720P; }
    }

    public bool Is1080P
    {
      get { return _is1080P; }
    }

    public bool Is1080I
    {
       get { return _is1080I; }
    }

    public bool IsHDTV
    {
       get { return _isHDTV; }
    }

    public bool IsSDTV
    {
       get { return _isSDTV; }
    }

    public bool IsInterlaced
    {
       get { return _isInterlaced; }
    }

    public int Width
    {
      get { return _width; }
    }

    public int Height
    {
      get { return _height; }
    }

    #endregion

    #region public audio related properties

    public string AudioCodec
    {
      get { return _audioCodec; }
    }

    public int Audiorate
    {
      get { return _audiorate; }
    }

    public int Audiochannels
    {
      get { return _audiochannels; }
    }   

    public bool IsAC3
    {
      get { return _isAC3; }
    }

    public bool IsMP3
    {
      get { return _isMP3; }
    }

    public bool IsMP2A
    {
      get { return _isMP2A; }
    }

    public bool IsWMA
    {
      get { return _isWMA; }
    }

    public bool IsPCM
    {
      get { return _isPCM; }
    }

    public bool IsDTS
    {
      get { return _isDTS; }
    }

    public bool IsOGG
    {
      get { return _isOGG; }
    }

    public bool IsAAC
    {
      get { return _isAAC; }
    }

    #endregion

    #region public misc properties

    public bool HasSubtitles
    {
      get { return _hasSubtitles; }
    }    

    #endregion
  }
}
