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
    public event MessageHandler<PlayerStartFailedMessage> MediaFailed;
    public event MessageHandler<PlayerStartMessage> MediaOpened;
    public event MessageHandler<PlayerEndedMessage> MediaEnded;

    private MediaPlayer _underlyingPlayer;
    private PlayerMediaType _mediaType = PlayerMediaType.Movie;
    private string _fileName;
    private bool _paused = false;
    private bool _isStream = false;
    private bool _hasMedia = false;
    private Exception _exception;

    public VideoPlayer(string fileName)
    {
      _fileName = fileName;
      _underlyingPlayer = new MediaPlayer();

      _hasMedia = true;
      ServiceScope.Get<IMessageBroker>().Register(this);
    }

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

    void _underlyingPlayer_MediaEnded(object sender, EventArgs e)
    {

      if (MediaEnded != null)
      {
        MediaEnded( new PlayerEndedMessage());
      }
    }

    public void Close()
    {
      _underlyingPlayer.Stop();
      _underlyingPlayer.Close();

      _hasMedia = false;
    }

    public void Play()
    {
      _underlyingPlayer.Play();
    }

    public void Stop()
    {
      _underlyingPlayer.Stop();
    }

    private void _underlyingPlayer_MediaOpened(object sender, EventArgs e)
    {
      if (MediaOpened != null)
      {
        MediaOpened( new PlayerStartMessage());
      }
    }

    private void _underlyingPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      _exception = e.ErrorException;
      if (MediaFailed != null)
      {
        MediaFailed( new PlayerStartFailedMessage(e.ErrorException));
        _hasMedia = false;
      }
    }

    public string FileName
    {
      get { return _fileName; }
    }

    public bool HasMedia
    {
      get { return _hasMedia; }
    }

    public bool HasError
    {
      get { return (_exception != null); }
    }

    public string ErrorMessage
    {
      get
      {
        if (_exception == null) return "";
        return _exception.Message;
      }
    }

    public void Pause()
    {
      _paused = !_paused;

      if (_paused)
        _underlyingPlayer.Pause();
      else
        _underlyingPlayer.Play();
    }

    public bool IsPaused
    {
      get { return _paused; }
    }

    public TimeSpan Position
    {
      get { return _underlyingPlayer.Position; }
      set { _underlyingPlayer.Position = value; }
    }

    /// <summary>
    /// Determines if it's a stream or not we're playing.
    /// (Not implented yet, only returns false)
    /// </summary>
    public bool IsStream
    {
      get { return false; }
    }

    public PlayerMediaType MediaType
    {
      get { return _mediaType; }
    }

    public TimeSpan Duration
    {
      get { return _underlyingPlayer.NaturalDuration.TimeSpan; }
    }

    public object UnderlyingPlayer
    {
      get { return _underlyingPlayer; }
    }

    public void Dispose()
    {
      Stop();
      Close();
      ServiceScope.Get<IMessageBroker>().Unregister(this);
    }
  }
}
