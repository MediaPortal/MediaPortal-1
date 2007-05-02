using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ProjectInfinity;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Players;
using ProjectInfinity.Settings;

namespace MyVideos
{
  public class VideoFullscreenViewModel : VideoHomeViewModel
  {
    private readonly Visibility _bottomOsdVisibility = Visibility.Hidden;
    private readonly List<int> _seekSteps = new List<int>();
    private int _currentSeekStep = 0;

    private SeekDirection _seekDirection = SeekDirection.Unknown;
    private bool _reachedEnd = false;
    private bool _reachedStart = false;
    private readonly DispatcherTimer _seekTimeoutTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoFullscreenViewModel"/> class.
    /// </summary>
    public VideoFullscreenViewModel()
    {
      //get the seeking steps from the configuration file...
      VideoSettings settings = new VideoSettings();
      ServiceScope.Get<ISettingsManager>().Load(settings);
      string[] steps = settings.SeekSteps.Split(new char[] {','});
      for (int i = 0; i < steps.Length; ++i)
      {
        _seekSteps.Add(Int32.Parse(steps[i]));
      }

      _seekTimeoutTimer = new DispatcherTimer();
      _seekTimeoutTimer.Interval = new TimeSpan(0, 0, 1);
      _seekTimeoutTimer.IsEnabled = false;
      _seekTimeoutTimer.Tick += seekTimeoutEvent;
      _currentSeekStep = 0;
      _seekDirection = SeekDirection.Unknown;
      _reachedEnd = false;
      _reachedStart = false;
    }

    /// <summary>
    /// Gets the top osd visibility.
    /// </summary>
    /// <value>The top osd visibility.</value>
    public Visibility TopOsdVisibility
    {
      get
      {
        if (_seekDirection != SeekDirection.Unknown || _bottomOsdVisibility == Visibility.Visible)
        {
          return Visibility.Visible;
        }
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0)
        {
          return Visibility.Hidden;
        }
        VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.IsPaused)
        {
          return Visibility.Visible;
        }
        return Visibility.Hidden;
      }
    }

    /// <summary>
    /// Gets the bottom osd visibility.
    /// </summary>
    /// <value>The bottom osd visibility.</value>
    public Visibility BottomOsdVisibility
    {
      get { return _bottomOsdVisibility; }
    }

    /// <summary>
    /// Gets the top osd start time.
    /// </summary>
    /// <value>The top osd start time.</value>
    public string StartTime
    {
      get
      {
        return "00:00";
      }
    }

    /// <summary>
    /// Gets the top osd end time.
    /// </summary>
    /// <value>The top osd end time.</value>
    public string EndTime
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0)
        {
          return "00:00";
        }
        VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];

        string endTime;
        if (player.Duration.Minutes < 1)
        {
          endTime = String.Format("{0} {1}", player.Duration.Seconds, ServiceScope.Get<ILocalisation>().ToString("mytv",118));//secs
        }
        else if (player.Duration.Minutes < 10)
        {
          endTime = String.Format("{0}:0{1}", player.Duration.Hours, player.Duration.Minutes);
        }
        else
        {
          endTime = String.Format("{0}:{1}", player.Duration.Hours, player.Duration.Minutes);
        }
        return endTime;
      }
    }

    /// <summary>
    /// Gets the value of the orange part of the top progress bar .
    /// </summary>
    /// <value>The top progress bar orange.</value>
    public double TopProgressBarOrange
    {
      get { return 0; }
    }

    /// <summary>
    /// Gets the value of the red part of the top progress bar .
    /// </summary>
    /// <value>The top progress bar orange.</value>
    public double TopProgressBarRed
    {
      get { return 0; }
    }

    /// <summary>
    /// Gets the value of the green part of the top progress bar .
    /// </summary>
    /// <value>The top progress bar green.</value>
    public double TopProgressBarGreen
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0)
        {
          return 0;
        }
        VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];
        float percent = (float) (player.Position.TotalSeconds/player.Duration.TotalSeconds);
        return percent*100;
      }
    }

    /// <summary>
    /// Gets the localized state for the top osd
    /// </summary>
    /// <value>The state.</value>
    public string LabelState
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0)
        {
          return "";
        }
        VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];

        string labelState = "";
        if (player.IsPaused)
        {
          labelState = ServiceScope.Get<ILocalisation>().ToString("mytv", 117); //|| Paused;
        }

        if (_reachedEnd)
        {
          labelState = ServiceScope.Get<ILocalisation>().ToString("mytv", 122) /*end*/;
        }
        else if (_reachedStart)
        {
          labelState = ServiceScope.Get<ILocalisation>().ToString("mytv", 121) /*start*/;
        }
        else if (_currentSeekStep > 0)
        {
          TimeSpan ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep]);
          string secs = ts.Seconds.ToString();
          string mins = ts.Minutes.ToString();
          string hours = ts.Hours.ToString();
          string label;
          if (ts.Hours > 0)
          {
            label = String.Format("{0} {1}", hours, ServiceScope.Get<ILocalisation>().ToString("mytv", 120) /*hours*/);
          }
          else if (ts.Minutes > 0)
          {
            label = String.Format("{0} {1}", mins, ServiceScope.Get<ILocalisation>().ToString("mytv", 119) /*mins*/);
          }
          else
          {
            label = String.Format("{0} {1}", secs, ServiceScope.Get<ILocalisation>().ToString("mytv", 118) /*secs*/);
          }
          if (_seekDirection == SeekDirection.Past)
          {
            label = "-" + label;
          }
          labelState = label;
        }
        return labelState;
      }
    }

    #region Event Handlers

    /// <summary>
    /// Timer callback
    /// When occurs this method will do the actual seeking
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public void seekTimeoutEvent(object sender, EventArgs e)
    {
      _seekTimeoutTimer.Stop();
      _seekTimeoutTimer.IsEnabled = false;
      if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
      {
        VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];
        if (_reachedStart)
        {
          player.Position = new TimeSpan(0, 0, 0);
        }
        else if (_reachedEnd)
        {
          TimeSpan newPos = player.Duration + new TimeSpan(0, 0, 0, 0, -100);
          player.Position = newPos;
        }
        else
        {
          TimeSpan ts;
          if (_seekDirection == SeekDirection.Past)
          {
            ts = new TimeSpan(0, 0, -_seekSteps[_currentSeekStep]);
          }
          else
          {
            ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep]);
          }

          TimeSpan newPosition = player.Position + ts;
          player.Position = newPosition;
        }
      }
      _seekDirection = SeekDirection.Unknown;
      _currentSeekStep = 0;
      _reachedEnd = _reachedStart = false;
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("LabelState");
      ChangeProperty("TopProgressBarOrange");
      ChangeProperty("TopProgressBarRed");
      ChangeProperty("TopProgressBarGreen");
    }

    
    protected override void OnMediaPlayerEnded()
    {
      base.OnMediaPlayerEnded();
      ServiceScope.Get<INavigationService>().GoBack();
    }

    /// <summary>
    /// Determines whether we can seek the specified timespan.
    /// </summary>
    /// <param name="ts">The timespan we want to seek.</param>
    /// <param name="reachedStart">if set to <c>true</c> we reached the start.</param>
    /// <param name="reachedEnd">if set to <c>true</c> we reached the end (livepoint).</param>
    /// <returns>
    /// 	<c>true</c> if this instance can seek the specified timespan; otherwise, <c>false</c>.
    /// </returns>
    public bool CanSeek(TimeSpan ts, ref bool reachedStart, ref bool reachedEnd)
    {
      _seekTimeoutTimer.Stop();
      _seekTimeoutTimer.IsEnabled = true;
      _seekTimeoutTimer.Start();
      if (ServiceScope.Get<IPlayerCollectionService>().Count == 0)
      {
        return false;
      }
      VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];
      TimeSpan newPosition = ts + player.Position;
      if (newPosition.TotalSeconds > player.Duration.TotalSeconds)
      {
        reachedEnd = true;
        reachedStart = false;
        return false;
      }
      else if (newPosition.TotalSeconds < 0)
      {
        reachedEnd = false;
        reachedStart = true;
        return false;
      }
      reachedEnd = false;
      reachedStart = false;
      return true;
    }

    /// <summary>
    /// Handles left/right keys for seeking
    /// </summary>
    /// <param name="key">The key.</param>
    public void OnSeek(Key key)
    {
      if (_seekDirection == SeekDirection.Unknown)
      {
        if (key == Key.Right)
        {
          _seekDirection = SeekDirection.Future;
        }
        if (key == Key.Left)
        {
          _seekDirection = SeekDirection.Past;
        }
      }
      switch (_seekDirection)
      {
        case SeekDirection.Past:
          if (key == Key.Left)
          {
            if (_currentSeekStep + 1 < _seekSteps.Count)
            {
              TimeSpan ts = new TimeSpan(0, 0, -_seekSteps[_currentSeekStep + 1]);
              if (CanSeek(ts, ref _reachedStart, ref _reachedEnd))
              {
                _currentSeekStep++;
              }
              ChangeProperty("TopOsdVisibility");
              ChangeProperty("LabelState");
            }
          }
          if (key == Key.Right)
          {
            if (_currentSeekStep - 1 > 0)
            {
              TimeSpan ts = new TimeSpan(0, 0, -_seekSteps[_currentSeekStep - 1]);
              if (CanSeek(ts, ref _reachedStart, ref _reachedEnd))
              {
                _currentSeekStep--;
                ChangeProperty("TopOsdVisibility");
                ChangeProperty("LabelState");
              }
            }
            else
            {
              _seekDirection = SeekDirection.Unknown;
              _currentSeekStep = 0;
              _reachedEnd = _reachedStart = false;
              ChangeProperty("TopOsdVisibility");
              ChangeProperty("LabelState");
            }
          }
          break;
        case SeekDirection.Future:
          if (key == Key.Right)
          {
            if (_currentSeekStep + 1 < _seekSteps.Count)
            {
              TimeSpan ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep + 1]);
              if (CanSeek(ts, ref _reachedStart, ref _reachedEnd))
              {
                _currentSeekStep++;
              }
              ChangeProperty("TopOsdVisibility");
              ChangeProperty("LabelState");
            }
          }
          if (key == Key.Left)
          {
            if (_currentSeekStep - 1 > 0)
            {
              TimeSpan ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep - 1]);
              if (CanSeek(ts, ref _reachedStart, ref _reachedEnd))
              {
                _currentSeekStep--;
              }
              ChangeProperty("TopOsdVisibility");
              ChangeProperty("LabelState");
            }
            else
            {
              _seekDirection = SeekDirection.Unknown;
              _currentSeekStep = 0;
              _reachedEnd = _reachedStart = false;
              ChangeProperty("TopOsdVisibility");
              ChangeProperty("LabelState");
            }
          }
          break;
      }
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public void Pause()
    {
      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        VideoPlayer player = (VideoPlayer) ServiceScope.Get<IPlayerCollectionService>()[0];
        player.Pause();
        ChangeProperty("TopOsdVisibility");
        ChangeProperty("LabelState");
        ChangeProperty("EndTime");
        ChangeProperty("TopProgressBarOrange");
        ChangeProperty("TopProgressBarRed");
        ChangeProperty("TopProgressBarGreen");
      }
    }

    private enum SeekDirection
    {
      Unknown,
      Past,
      Future
    }

    #endregion
  }
}