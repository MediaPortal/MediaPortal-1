using System;
using System.IO;
using System.Windows.Media;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Music
{
  //[Plugin("WMPMusicPlayer", "Music using Windows Media Player for playback")]
  public class WMPMusicPlayer : IMusicPlayer
  {
    private MediaPlayer mediaPlayer;

    #region IMusicPlayer Members

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
      MusicStartMessage e = new MusicStartMessage();
      e.Artist = "The Infinity Project";
      e.Album = "Mystical Experiences";
      e.TrackNo = 8;
      e.Title = "Blue Aura";
      OnMusicStart(e);
    }

    /// <summary>
    /// Stops playback
    /// </summary>
    public void Stop()
    {
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

    #region IPlugin Members

    #endregion

    public void Initialize()
    {
      ServiceScope.Get<IMessageBroker>().Register(this);
      ServiceScope.Add<IMusicPlayer>(this);
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      ServiceScope.Remove<IMusicPlayer>();
    }

    #endregion

    /// <summary>
    /// Triggers sending of the MusicStart message
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnMusicStart(MusicStartMessage e)
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
  }
}