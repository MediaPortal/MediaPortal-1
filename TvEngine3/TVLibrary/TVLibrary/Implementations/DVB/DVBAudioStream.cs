using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  public class DVBAudioStream : IAudioStream
  {
    #region variables
    string _language;
    AudioStreamType _streamType;
    int _pid;
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

  }
}
