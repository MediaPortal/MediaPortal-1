using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Players
{
  public enum PlayerMediaType
  {
    Music,
    Movie,
    TvRecording,
    TvLive
  };
  public interface IPlayer : IDisposable
  {

    /// <summary>
    /// Opens the specified file name.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    void Open(PlayerMediaType mediaType, string fileName);
    /// <summary>
    /// Closes the player.
    /// </summary>
    void Close();
    /// <summary>
    /// Pauses the player.
    /// </summary>
    void Pause();
    /// <summary>
    /// starts playing.
    /// </summary>
    void Play();
    /// <summary>
    /// stops playing.
    /// </summary>
    void Stop();
    /// <summary>
    /// Gets a value indicating whether this player is paused.
    /// </summary>
    /// <value><c>true</c> if this instance is paused; otherwise, <c>false</c>.</value>
    bool IsPaused { get;}
    /// <summary>
    /// Gets a value indicating whether this player is playing a stream or local file
    /// </summary>
    /// <value><c>true</c> if this instance is stream; otherwise, <c>false</c>.</value>
    bool IsStream { get;}

    /// <summary>
    /// Gets the type of the media.
    /// </summary>
    /// <value>The type of the media.</value>
    PlayerMediaType MediaType { get;}

    /// <summary>
    /// Gets the duration of the file played
    /// </summary>
    /// <value>The duration.</value>
    TimeSpan Duration { get;}
    /// <summary>
    /// Gets/Sets the position.
    /// </summary>
    /// <value>The position.</value>
    TimeSpan Position { get;set;}
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    string FileName { get;}
    /// <summary>
    /// Gets the underlying media player.
    /// </summary>
    /// <value>The underlying media player.</value>
    object UnderlyingPlayer { get;}
  }
}
