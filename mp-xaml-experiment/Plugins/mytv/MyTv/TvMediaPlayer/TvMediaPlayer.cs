using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TvControl;
using Microsoft.Win32;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Players;
using ProjectInfinity.Messaging;

namespace MyTv
{
  public class TvMediaPlayer : IPlayer, IDisposable
  {
    #region IPlayer events
    //
    // Summary:
    //     Occurs when an error is encountered
    public event MessageHandler<PlayerStartFailedMessage> MediaFailed;
    //
    // Summary:
    //     Occurs when the media is opened.
    public event MessageHandler<PlayerStartMessage> MediaOpened;
    //
    // Summary:
    //     Occurs when the media has ended.
    public event MessageHandler<PlayerEndedMessage> MediaEnded;
    //
    // Summary:
    //     Occurs when the media has ended.
    public event MessageHandler<PlayerStopMessage> MediaStopped;
    #endregion

    #region delegates
    private delegate void StopTimeshiftingDelegate(VirtualCard card);
    #endregion

    #region variables
    PlayerMediaType _mediaType;
    MediaPlayer _underlyingPlayer;
    VirtualCard _card;
    Exception _exception;
    bool _paused = false;
    bool _isStream = false;
    bool _isPlaying = false;
    string _fileName;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvMediaPlayer"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public TvMediaPlayer(VirtualCard card, string fileName)
    {
      //ScrubbingEnabled = true;
      _fileName = fileName;
      _card = card;
      _exception = null;
      _underlyingPlayer = new MediaPlayer();
      ServiceScope.Get<IMessageBroker>().Register(this);

    }

    #endregion

    #region IPlayer interface
    /// <summary>
    /// Opens the specified file name.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public void Open(PlayerMediaType mediaType, string fileName)
    {
      _mediaType = mediaType;

      bool isStream = false;
      ServiceScope.Get<ILogger>().Info("Tv:  start playing:{0}", fileName);
      string orgFileName = fileName;
      if (!File.Exists(fileName))
      {
        ServiceScope.Get<ILogger>().Info("Tv:  file does not exists, get rtsp stream");
        TvServer server = new TvServer();
        if (_card != null)
        {
          if (fileName == _card.TimeShiftFileName)
            fileName = _card.RTSPUrl;
          else
            fileName = server.GetRtspUrlForFile(fileName);
        }
        else
          fileName = server.GetRtspUrlForFile(fileName);
        isStream = true;

        ServiceScope.Get<ILogger>().Info("Tv:  start playing stream:{0}", fileName);
      }
      string fname = fileName;
      if (fileName.StartsWith("rtsp://"))
      {
        isStream = true;
        fname = String.Format(@"{0}\1.tsp", Directory.GetCurrentDirectory());
        if (File.Exists(fname))
        {
          File.Delete(fname);
        }

        ServiceScope.Get<ILogger>().Info("Tv:  create :{0}", fname);
        using (FileStream stream = new FileStream(fname, FileMode.OpenOrCreate))
        {
          using (BinaryWriter writer = new BinaryWriter(stream))
          {
            byte k = 0x12;
            for (int i = 0; i < 99; ++i) writer.Write(k);
            writer.Write(fileName);
          }
        }
      }

      ServiceScope.Get<ILogger>().Info("Tv:  open :{0}", fname);
      _isStream = isStream;
      _underlyingPlayer.MediaFailed += new EventHandler<ExceptionEventArgs>(TvMediaPlayer_MediaFailed);
      _underlyingPlayer.MediaOpened += new EventHandler(TvMediaPlayer_MediaOpened);
      _underlyingPlayer.MediaEnded += new EventHandler(_underlyingPlayer_MediaEnded);
      _underlyingPlayer.Open(new Uri(fname, UriKind.Absolute));
      ServiceScope.Get<ILogger>().Info("Tv:  player opened");
    }

    void _underlyingPlayer_MediaEnded(object sender, EventArgs e)
    {
      _isPlaying = false;
      if (MediaEnded != null)
      {
        MediaEnded( new PlayerEndedMessage());
      }
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
      if (_card != null)
      {
        StopTimeshiftingDelegate starter = new StopTimeshiftingDelegate(this.DoStopTimeshifting);
        starter.BeginInvoke(_card, null, null);
      }
    }

    /// <summary>
    /// Pauses/Continues media playback.
    /// </summary>
    public void Pause()
    {
      _paused = !_paused;
      if (_paused)
      {
        _underlyingPlayer.Pause();
      }
      else
      {
        _underlyingPlayer.Play();
      }
    }

    /// <summary>
    /// stops media playback.
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
    /// starts playing.
    /// </summary>
    public void Play()
    {
      _isPlaying = true;
      _underlyingPlayer.Play();
    }

    /// <summary>
    /// Gets a value indicating whether this instance is paused.
    /// </summary>
    /// <value><c>true</c> if this instance is paused; otherwise, <c>false</c>.</value>
    public bool IsPaused
    {
      get
      {
        return _paused;
      }
    }

    /// <summary>
    /// Gets the duration.
    /// </summary>
    /// <value>The duration.</value>
    public TimeSpan Duration
    {
      get
      {
        if (_card != null)
        {
          if (_card.IsTimeShifting || _card.IsRecording)
          {
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Mediaportal\TsReader"))
            {
              if (subkey == null)
                return TimeSpan.Zero;
              int totalMilliSecs = (int)subkey.GetValue("duration");
              TimeSpan ts = new TimeSpan(0, 0, 0, 0, totalMilliSecs);
              //this.NaturalDuration = new System.Windows.Duration(ts);
              return ts;

            }
          }
        }
        if (_underlyingPlayer.NaturalDuration.HasTimeSpan) return _underlyingPlayer.NaturalDuration.TimeSpan;
        return TimeSpan.Zero;
      }
    }

    /// <summary>
    /// Gets/Sets the position.
    /// </summary>
    /// <value>The position.</value>
    public TimeSpan Position
    {
      get
      {
        return _underlyingPlayer.Position;
      }
      set
      {
        _underlyingPlayer.Position = value;
      }
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
    /// Gets a value indicating whether this instance has error.
    /// </summary>
    /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
    public bool HasError
    {
      get
      {
        return (_exception != null);
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is a rtsp stream or local file.
    /// </summary>
    /// <value><c>true</c> if this instance is stream; otherwise, <c>false</c>.</value>
    public bool IsStream
    {
      get
      {
        return _isStream;
      }
      set
      {
        _isStream = value;
      }
    }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {
      get
      {
        return _fileName;
      }
    }

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public VirtualCard Card
    {
      get
      {
        return _card;
      }
    }


    /// <summary>
    /// Gets the type of the media.
    /// </summary>
    /// <value>The type of the media.</value>
    public PlayerMediaType MediaType
    {
      get
      {
        return _mediaType;
      }
    }

    /// <summary>
    /// Gets the underlying media player.
    /// </summary>
    /// <value>The underlying media player.</value>
    public object UnderlyingPlayer
    {
      get
      {
        return _underlyingPlayer;
      }
    }
    #endregion

    #region private methods
    void TvMediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      _exception = e.ErrorException;
      if (MediaFailed != null)
      {
        MediaFailed( new PlayerStartFailedMessage(e.ErrorException));
      }
    }

    void TvMediaPlayer_MediaOpened(object sender, EventArgs e)
    {

      _isPlaying = true;
      ServiceScope.Get<ILogger>().Info("MyTv: media opened {0}x{1} {2}", _underlyingPlayer.NaturalVideoWidth, _underlyingPlayer.NaturalVideoHeight, Duration);
      if (MediaOpened != null)
      {
        MediaOpened( new PlayerStartMessage());
      }
      if (!IsStream)
      {
        SeekToEnd();
      }
    }
    void DoStopTimeshifting(VirtualCard card)
    {
      if (card != null)
      {
        if (card.IsTimeShifting)
        {
          card.StopTimeShifting();
        }
      }
    }
    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      Stop();
      Close();
      ServiceScope.Get<IMessageBroker>().Unregister(this);
    }
    public void SeekToEnd()
    {
      TimeSpan duration = Duration;
      TimeSpan newPos = duration + new TimeSpan(0, 0, 0, 0, -500);
      ServiceScope.Get<ILogger>().Info("MyTv: OnSeekToEnd current {0}/{1}", newPos, Duration);
      ServiceScope.Get<ILogger>().Info("MyTv: Seek to {0}/{1}", newPos, duration);
      Position = newPos;
    }

    #endregion
  }
}
