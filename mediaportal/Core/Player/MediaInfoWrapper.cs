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
    private bool _isAC3 = false;  // AC3
    private bool _isMP3 = false; // MPEG-1 Audio layer 3
    private bool _isMP2 = false; // MPEG-1 Audio layer 2
    private bool _isDTS = false; // DTS
    private bool _isOGG = false; // OGG VORBIS
    private bool _isAAC = false;  //AAC

    #endregion

    #region ctor's

    public MediaInfoWrapper(string strFile)
    {

      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isDVD = Util.Utils.IsDVD(strFile);
      bool isVideo = Util.Utils.IsVideo(strFile);
      bool IsAVStream = Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.

      //currently mediainfo is only used for video related material.
      if (!isTV && !isDVD && !isVideo)
      {
        return;
      }

      try
      {
        _mI = new MediaInfo();
        _mI.Open(strFile);

        NumberFormatInfo providerNumber = new NumberFormatInfo();
        providerNumber.NumberDecimalSeparator = ".";

        double.TryParse(_mI.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, providerNumber, out _framerate);
        Int32.TryParse(_mI.Get(StreamKind.Video, 0, "Width"), NumberStyles.Integer, providerNumber, out _width);
        Int32.TryParse(_mI.Get(StreamKind.Video, 0, "Height"), NumberStyles.Integer, providerNumber, out _height);
        Int32.TryParse(_mI.Get(StreamKind.Audio, 0, "Audiochannels"), NumberStyles.Integer, providerNumber, out _audiochannels);
        Int32.TryParse(_mI.Get(StreamKind.Audio, 0, "Audiorate"), NumberStyles.Integer, providerNumber, out _audiorate);

        _aspectRatio = _mI.Get(StreamKind.Video, 0, "AspectRatio/String");

        string codec = _mI.Get(StreamKind.Audio, 0, "Codec/String").ToLower();;

        _isAC3 = (codec.IndexOf("ac3") > -1);
        _isMP3 = (codec.IndexOf("mpeg-1 audio layer 3") > -1);
        _isMP2 = (codec.IndexOf("mpeg-1 audio layer 2") > -1);
        _isDTS = (codec.IndexOf("dts") > -1);
        _isOGG = (codec.IndexOf("ogg") > -1); 
        _isAAC = (codec.IndexOf("aac") > -1);

        Log.Info("MediaInfoWrapper.MediaInfoWrapper: inspecting media : {0}", strFile);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: FrameRate : {0}", _framerate);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Width : {0}", _width);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Height : {0}", _height);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Audiochannels : {0}", _audiochannels);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Audiorate : {0}", _audiorate);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: AspectRatio : {0}", _aspectRatio);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: Codec : {0}", codec);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsAC3 : {0}", _isAC3);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP3 : {0}", _isMP3);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsMP2 : {0}", _isMP2);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsDTS : {0}", _isDTS);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsOGG : {0}", _isOGG);
        Log.Info("MediaInfoWrapper.MediaInfoWrapper: IsAAC : {0}", _isAAC);


      }
      catch (Exception ex)
      {
        Log.Error(
          "MediaInfoWrapper.MediaInfoWrapper: unable to call external DLL - medialib info (make sure 'MediaInfo.dll' is located in MP root dir.) {0}",
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

    #region public video related properties

    public string AspectRatio
    {
      get { return _aspectRatio; }
    }

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

    #endregion

    #region public audio related properties

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

    public bool IsMP2
    {
      get { return _isMP2; }
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
  }
}
