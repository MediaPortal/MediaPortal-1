using System;
using ProjectInfinity.Messaging;
using ProjectInfinity.Messaging.MusicMessages;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Music
{
  /// <summary>
  /// Interface that all music players need to implement.
  /// </summary>
  public interface IMusicPlayer
  {
    /// <summary>
    /// Start playing the given file.
    /// </summary>
    /// <param name="file">the file to play</param>
    void Play(string file);

    /// <summary>
    /// Stops playback
    /// </summary>
    void Stop();

    /// <summary>
    /// Message to broadcast when playback has started
    /// </summary>
    [MessagePublication(typeof (MusicStartMessage))]
    event EventHandler<MusicStartMessage> MusicStart;

    /// <summary>
    /// Message to broadcast when playback has stopped
    /// </summary>
    [MessagePublication(typeof (Stop))]
    event EventHandler MusicStop;
  }
}