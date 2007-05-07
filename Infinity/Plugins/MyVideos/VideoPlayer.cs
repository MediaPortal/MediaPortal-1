using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using ProjectInfinity.Players;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using System.IO;

namespace MyVideos
{
  public class VideoPlayer : IPlayer
  {
    #region messages
    public event MessageHandler<PlayerStartMessage> MediaOpened;
    public event MessageHandler<PlayerStartFailedMessage> MediaFailed;
    public event MessageHandler<PlayerEndedMessage> MediaEnded;
    public event MessageHandler<PlayerStopMessage> MediaStopped;
    #endregion

    #region variables
    private MediaPlayer _underlyingPlayer;
    private PlayerMediaType _mediaType = PlayerMediaType.Movie;
    private string _fileName;
    private bool _paused = false;
    private bool _isStream = false;
    private bool _hasMedia = false;
    private bool _isPlaying = false;
    private Exception _exception;
    #endregion

    #region ctor
    public VideoPlayer(string fileName)
    {
      _fileName = fileName;
      _underlyingPlayer = new MediaPlayer();

      _hasMedia = true;
      ServiceScope.Get<IMessageBroker>().Register(this);
    }
    #endregion

    #region media event handlers
    /// <summary>
    /// Handles the MediaEnded event of the _underlyingPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void _underlyingPlayer_MediaEnded(object sender, EventArgs e)
    {
      _isPlaying = false;
      if (MediaEnded != null)
      {
        MediaEnded(new PlayerEndedMessage());
      }
    }

    /// <summary>
    /// Handles the MediaOpened event of the _underlyingPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void _underlyingPlayer_MediaOpened(object sender, EventArgs e)
    {
      _isPlaying = true;
      if (MediaOpened != null)
      {
        MediaOpened( new PlayerStartMessage());
      }
    }

    /// <summary>
    /// Handles the MediaFailed event of the _underlyingPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Media.ExceptionEventArgs"/> instance containing the event data.</param>
    private void _underlyingPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      _exception = e.ErrorException;
      if (MediaFailed != null)
      {
        MediaFailed( new PlayerStartFailedMessage(e.ErrorException));
        _hasMedia = false;
      }
    }
    #endregion

    #region properties

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {
      get { return _fileName; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has media.
    /// </summary>
    /// <value><c>true</c> if this instance has media; otherwise, <c>false</c>.</value>
    public bool HasMedia
    {
      get { return _hasMedia; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has error.
    /// </summary>
    /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
    public bool HasError
    {
      get { return (_exception != null); }
    }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    /// <value>The error message.</value>
    public string ErrorMessage
    {
      get
      {
        if (_exception == null) return "";
        return _exception.Message;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this player is paused.
    /// </summary>
    /// <value><c>true</c> if this instance is paused; otherwise, <c>false</c>.</value>
    public bool IsPaused
    {
      get { return _paused; }
    }

    /// <summary>
    /// Gets/Sets the position.
    /// </summary>
    /// <value>The position.</value>
    public TimeSpan Position
    {
      get { return _underlyingPlayer.Position; }
      set { _underlyingPlayer.Position = value; }
    }

    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>
    public int Width
    {
      get
      {
        return _underlyingPlayer.NaturalVideoWidth;
      }
    }

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Height
    {
      get
      {
        return _underlyingPlayer.NaturalVideoHeight;
      }
    }

    /// <summary>
    /// Determines if it's a stream or not we're playing.
    /// (Not implented yet, only returns false)
    /// </summary>
    public bool IsStream
    {
      get { return false; }
    }

    /// <summary>
    /// Gets the type of the media.
    /// </summary>
    /// <value>The type of the media.</value>
    public PlayerMediaType MediaType
    {
      get { return _mediaType; }
    }

    /// <summary>
    /// Gets the duration of the file played
    /// </summary>
    /// <value>The duration.</value>
    public TimeSpan Duration
    {
      get { return _underlyingPlayer.NaturalDuration.TimeSpan; }
    }

    /// <summary>
    /// Gets the underlying media player.
    /// </summary>
    /// <value>The underlying media player.</value>
    public object UnderlyingPlayer
    {
      get { return _underlyingPlayer; }
    }
    #endregion

    #region public methods

    /// <summary>
    /// Opens the specified file name.
    /// </summary>
    /// <param name="mediaType">type of the file</param>
    /// <param name="fileName">Name of the file.</param>
    public void Open(PlayerMediaType mediaType, string fileName)
    {
      _mediaType = mediaType;

      bool isStream = false;
      ServiceScope.Get<ILogger>().Info("Video:  start playing: {0}", fileName);

      if (!File.Exists(fileName) && _mediaType == PlayerMediaType.Movie)
      {
        Dialogs.MpDialogOk diag = new Dialogs.MpDialogOk();
        diag.Header = "Error";
        diag.Content = "File not found.";
        diag.ShowDialog();
        return;
      }

      ServiceScope.Get<ILogger>().Info("Video:  open {0}", fileName);
      _isStream = isStream;
      _underlyingPlayer.MediaFailed += new EventHandler<ExceptionEventArgs>(_underlyingPlayer_MediaFailed);
      _underlyingPlayer.MediaOpened += new EventHandler(_underlyingPlayer_MediaOpened);
      _underlyingPlayer.MediaEnded += new EventHandler(_underlyingPlayer_MediaEnded);
      if (_mediaType != PlayerMediaType.DVD)
        _underlyingPlayer.Open(new Uri(fileName, UriKind.Absolute));
      else
        _underlyingPlayer.Open(new Uri(fileName));
      ServiceScope.Get<ILogger>().Info("Video:  player opened");

      _hasMedia = true;
    }

    /// <summary>
    /// Closes the player.
    /// </summary>
    public void Close()
    {
      if (_isPlaying)
      {
        _isPlaying = false;

        if (MediaStopped != null)
        {
          MediaStopped(new PlayerStopMessage());
        }
      }
      _underlyingPlayer.Stop();
      _underlyingPlayer.Close();

      _hasMedia = false;
    }

    /// <summary>
    /// starts playing.
    /// </summary>
    public void Play()
    {
      _underlyingPlayer.Play();
      _isPlaying = true;
    }

    /// <summary>
    /// stops playing.
    /// </summary>
    public void Stop()
    {
      if (_isPlaying)
      {
        _isPlaying = false;

        if (MediaStopped != null)
        {
          MediaStopped(new PlayerStopMessage());
        }
      }
      _underlyingPlayer.Stop();
    }

    /// <summary>
    /// Pauses the player.
    /// </summary>
    public void Pause()
    {
      _paused = !_paused;

      if (_paused)
        _underlyingPlayer.Pause();
      else
        _underlyingPlayer.Play();
    }
    #endregion

    #region idisposable members
    public void Dispose()
    {
      Stop();
      Close();
      ServiceScope.Get<IMessageBroker>().Unregister(this);
    }
    #endregion
  }
}
