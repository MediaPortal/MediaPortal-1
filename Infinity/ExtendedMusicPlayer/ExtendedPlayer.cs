using System;
using System.IO;
using System.Windows.Media;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Music
{
  /// <summary>
  /// Demo of an external service implementation that extends the 
  /// </summary>
  [Plugin("ExtendedPlayer", "Test for extending plugin interface")]
  public class ExtendedPlayer : IMusicPlayer
  {
    private MediaPlayer mediaPlayer;

    /// <summary>
    /// Start playing the given file.
    /// </summary>
    /// <param name="file">the file to play</param>
    public void Play(string file)
    {
      ServiceScope.Get<ILogger>().Info("WMPMusicPlayer: Starting playback for file {0}", file);
      FileInfo fi = new FileInfo(file);
      if (!fi.Exists)
      {
        throw new ArgumentException("File does not exist", "file");
      }
      mediaPlayer = new MediaPlayer();
      mediaPlayer.Open(new Uri(file, UriKind.Relative));
      mediaPlayer.Play();
      ExtendedMusicStartMessage e = new ExtendedMusicStartMessage();
      e.Artist = "The Infinity Project";
      e.Album = "Mystical Experiences";
      e.TrackNo = 8;
      e.Title = "Blue Aura";
      e.Rating = 5;
      OnMusicStart(e);
    }

    /// <summary>
    /// Stops playback
    /// </summary>
    public void Stop()
    {
      if (mediaPlayer == null)
      {
        return;
      }
      ServiceScope.Get<ILogger>().Info("WMPMusicPlayer: Stopping playback");
      mediaPlayer.Stop();
      mediaPlayer.Close();
      mediaPlayer = null;
      OnMusicStop(EventArgs.Empty);
    }

    /// <summary>
    /// Message to broadcast when playback has started
    /// </summary>
    public event EventHandler<MusicStartMessage> MusicStart;


    /// <summary>
    /// Message to broadcast when playback has stopped
    /// </summary>
    public event EventHandler MusicStop;

    /// <summary>
    /// Triggers sending of the MusicStart message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnMusicStart(ExtendedMusicStartMessage e)
    {
      if (MusicStart != null)
      {
        MusicStart(this, e);
      }
    }

    /// <summary>
    /// Triggers sending of the MusicStop message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnMusicStop(EventArgs e)
    {
      if (MusicStop != null)
      {
        MusicStop(this, e);
      }
    }

    public void Initialize()
    {
      ServiceScope.Get<IMessageBroker>().Register(this);
      ServiceScope.Add<IMusicPlayer>(this);
    }

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      throw new NotImplementedException();
    }
  }
}