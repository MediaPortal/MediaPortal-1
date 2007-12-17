using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Players
{
  public class PlayerStopMessage : Message
  {
  };
  public class PlayerStartMessage : Message
  {
  };

  public class PlayerStartFailedMessage : Message
  {
    Exception _exception;
    public PlayerStartFailedMessage(Exception exception)
    {
      _exception = exception;
    }
    public Exception ErrorException
    {
      get
      {
        return _exception;
      }
    }
  };
  public class PlayerEndedMessage : Message
  {
  };

  public enum PlayerMediaType
  {
    Music,
    Movie,
    TvRecording,
    TvLive,
    DVD
  };
   
  public interface IPlayer : IDisposable
  {

    //
    // Summary:
    //     Occurs when an error is encountered
    [MessagePublication(typeof(PlayerStartFailedMessage))]
    event MessageHandler<PlayerStartFailedMessage> MediaFailed;
    //
    // Summary:
    //     Occurs when the media is opened.
    [MessagePublication(typeof(PlayerStartMessage))]
    event MessageHandler<PlayerStartMessage> MediaOpened;
    //
    // Summary:
    //     Occurs when the media has ended.
    [MessagePublication(typeof(PlayerEndedMessage))]
    event MessageHandler<PlayerEndedMessage> MediaEnded;

    //
    // Summary:
    //     Occurs when the media has stopped.
    [MessagePublication(typeof(PlayerStopMessage))]
    event MessageHandler<PlayerStopMessage> MediaStopped;

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

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    int Width { get;}

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    int Height { get;}
  }
}
