using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  [Serializable]
  public class DVBAudioStream : IAudioStream
  {
    #region variables
    string _language;
    AudioStreamType _streamType;
    int _pid;
    #endregion

    
    #region ctor
    public DVBAudioStream()
    {
      _language = "";
      _streamType = AudioStreamType.Mpeg2;
      _pid = 0;
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

    /// <summary>
    /// gets/sets the audio pid for this stream
    /// </summary>
    public int Pid
    {
      get
      {
        return _pid;
      }
      set
      {
        _pid = value;
      }
    }
    #endregion

    public override bool Equals(object obj)
    {
      DVBAudioStream stream = obj as DVBAudioStream;
      if (stream == null) return false;
      if (_language == stream.Language && _streamType == stream.StreamType && _pid == stream.Pid) return true;
      return false;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _language.GetHashCode() ^_streamType.GetHashCode() ^ _pid.GetHashCode();
    }
  }
}
