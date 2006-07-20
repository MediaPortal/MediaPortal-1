using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Audio stream types
  /// </summary>
  public enum AudioStreamType
  {
    Mpeg1,
    Mpeg2,
    AC3,
  }

  /// <summary>
  /// interface which describes a single audio stream
  /// </summary>
  public interface IAudioStream
  {
    /// <summary>
    /// gets/sets  Audio language
    /// </summary>
    string Language { get;set;}

    /// <summary>
    /// gets/sets the audio stream type
    /// </summary>
    AudioStreamType StreamType { get;set;}
  }
}
