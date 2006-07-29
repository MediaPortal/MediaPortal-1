using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  [Serializable]
  public class AnalogAudioStream: IAudioStream
  {
    #region variables
    string _language;
    AudioStreamType _streamType;
    DirectShowLib.TVAudioMode _audioMode;
    #endregion

    #region ctor
    public AnalogAudioStream()
    {
      _language = "";
      _streamType = AudioStreamType.Mpeg2;
      _audioMode = DirectShowLib.TVAudioMode.Stereo;
    }
    #endregion

    #region properties
    /// <summary>
    /// gets/sets  Audio language
    /// </summary>
    public string Language
    {
      get
      {
        return _language;
      }
      set
      {
        _language = value;
      }
    }

    /// <summary>
    /// gets/sets the audio stream type
    /// </summary>
    public AudioStreamType StreamType
    {
      get
      {
        return _streamType;
      }
      set
      {
        _streamType = value;
      }
    }
    public DirectShowLib.TVAudioMode AudioMode
    {
      get
      {
        return _audioMode;
      }
      set
      {
        _audioMode = value;
      }
    }

    #endregion

    public override bool Equals(object obj)
    {
      AnalogAudioStream stream = obj as AnalogAudioStream;
      if (stream == null) return false;
      if (_language == stream.Language && _streamType == stream.StreamType && AudioMode==stream.AudioMode) return true;
      return false;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _language.GetHashCode() ^ _streamType.GetHashCode()  ^_audioMode.GetHashCode();
    }
    public override string ToString()
    {
      return String.Format("mode:{0} type:{1} language:{2}",
          AudioMode, StreamType, Language);
    }
  }
}

