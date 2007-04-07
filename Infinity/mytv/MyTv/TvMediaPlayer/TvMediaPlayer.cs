using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TvControl;
using Microsoft.Win32;
using ProjectInfinity;
namespace MyTv
{
  public class TvMediaPlayer : MediaPlayer
  {
    #region delegates
    private delegate void StopTimeshiftingDelegate(VirtualCard card);
    #endregion
    #region variables
    VirtualCard _card;
    Exception _exception;
    bool _paused=false;
    bool _isStream = false;
    string _fileName;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvMediaPlayer"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public TvMediaPlayer(VirtualCard card,string fileName)
    {
      //ScrubbingEnabled = true;
      _fileName = fileName;
      _card = card;
      _exception = null;
      MediaFailed += new EventHandler<ExceptionEventArgs>(TvMediaPlayer_MediaFailed);
    }

    void TvMediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      _exception = e.ErrorException;
    }
    #endregion

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
    /// Pauses/Continues media playback.
    /// </summary>
    public new void Pause()
    {
      _paused = !_paused;
      if (_paused)
      {
        base.Pause();
      }
      else
      {
        base.Play();
      }
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
              int totalMilliSecs = (int)subkey.GetValue("duration");
              TimeSpan ts = new TimeSpan(0, 0, 0, 0,totalMilliSecs);
              //this.NaturalDuration = new System.Windows.Duration(ts);
              return ts;
              
            }
          }
        }
        if (NaturalDuration.HasTimeSpan) return NaturalDuration.TimeSpan;
        return  new TimeSpan(0, 0, 0, 0);
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


    #region IDisposable
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose(bool stopTimeShifting)
    {
      base.Stop();
      base.Close();
      if (_card != null && stopTimeShifting)
      {
        StopTimeshiftingDelegate starter = new StopTimeshiftingDelegate(this.DoStopTimeshifting);
        starter.BeginInvoke(_card,null, null);
      }
      ServiceScope.Get<ITvPlayerCollection>().Release(this);
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
  }
}
