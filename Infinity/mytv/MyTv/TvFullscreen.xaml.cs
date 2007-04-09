using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dialogs;
using TvDatabase;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvFullscreen.xaml
  /// </summary>

  public partial class TvFullscreen : System.Windows.Controls.Page
  {
    TvBaseViewModel _model;
    private delegate void StartTimeShiftingDelegate(Channel channel);
    private delegate void EndTimeShiftingDelegate(TvResult result, VirtualCard card);
    private delegate void MediaPlayerErrorDelegate();
    
    enum SeekDirection
    {
      Unknown,
      Past,
      Future
    }
    int[] _seekSteps = { 0, 15, 30, 60, 3 * 60, 5 * 60, 10 * 60, 15 * 60, 30 * 60, 60 * 60, 120 * 60 };
    int _currentSeekStep = 0;

    SeekDirection _seekDirection = SeekDirection.Unknown;
    bool _reachedEnd = false;
    bool _reachedStart = false;
    bool _bottomOsdVisible = false;
    System.Windows.Threading.DispatcherTimer _seekTimeoutTimer;
    System.Windows.Threading.DispatcherTimer _zapTimeoutTimer;
    string _zapChannel;

    /// <summary>
    /// Initializes a new instance of the <see cref="TvFullscreen"/> class.
    /// </summary>
    public TvFullscreen()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when [loaded].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _model = new TvBaseViewModel(this);
      gridMain.DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      _zapChannel = "";
      _seekTimeoutTimer = new System.Windows.Threading.DispatcherTimer();
      _seekTimeoutTimer.Interval = new TimeSpan(0, 0, 1);
      _seekTimeoutTimer.IsEnabled = false;
      _seekTimeoutTimer.Tick += new EventHandler(seekTimeoutEvent);

      _zapTimeoutTimer = new System.Windows.Threading.DispatcherTimer();
      _zapTimeoutTimer.Interval = new TimeSpan(0, 0, 3);
      _zapTimeoutTimer.IsEnabled = false;
      _zapTimeoutTimer.Tick += new EventHandler(zapTimeoutEvent);
      _currentSeekStep = 0;
      _seekDirection = SeekDirection.Unknown;
      _reachedEnd = false;
      _reachedStart = false;
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
       
      UpdateTopOsd();
    }


    /// <summary>
    /// Handles the MediaEnded event of the player control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void player_MediaEnded(object sender, EventArgs e)
    {

      ServiceScope.Get<INavigationService>().GoBack();
    }

    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Up)
      {
        OnChannelUp();
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Down)
      {
        OnChannelDown();
        e.Handled = true;
        return;
      }
      if (e.Key >= Key.D0 && e.Key <= Key.D9)
      {
        OnChannelKey(e.Key);
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Left || e.Key == Key.Right)
      {
        OnSeek(e.Key);
        e.Handled = true;
        return;
      }
      if (e.Key == Key.Escape || e.Key == Key.X)
      {
        //return to previous screen
        e.Handled = true;
        ServiceScope.Get<INavigationService>().GoBack();
        return;
      }
      if (e.Key == Key.Space)
      {
        e.Handled = true;
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
          player.Pause();
          UpdateTopOsd();
        }
        return;
      }
    }

    /// <summary>
    /// Updates the osd.
    /// </summary>
    void UpdateTopOsd()
    {
      if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return;
      TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
      if (player.IsPaused || _seekDirection != SeekDirection.Unknown || _bottomOsdVisible)
        gridOSD.Visibility = Visibility.Visible;
      else
        gridOSD.Visibility = Visibility.Hidden;

      double totalWidth = gridProgressBack.ActualWidth;
      if (player.Card != null)
      {
        if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
        {
          Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
          Program program = channel.CurrentProgram;
          labelStart.Content = channel.CurrentProgram.StartTime.ToString("HH:mm");
          labelEnd.Content = channel.CurrentProgram.EndTime.ToString("HH:mm");


          TimeSpan duration = player.Duration;
          TimeSpan position = player.Position;

          // <red><Green><orange>

          // caclulate total duration of the current program
          TimeSpan ts = (channel.CurrentProgram.EndTime - channel.CurrentProgram.StartTime);
          double programDuration = ts.TotalSeconds;

          //calculate where the program is at this time
          ts = (DateTime.Now - channel.CurrentProgram.StartTime);
          double livePoint = ts.TotalSeconds;

          //calculate when timeshifting was started
          double timeShiftStartPoint = livePoint - player.Duration.TotalSeconds;
          if (timeShiftStartPoint < 0) timeShiftStartPoint = 0;

          //calculate where we the current playing point is
          double playingPoint = player.Duration.TotalSeconds - player.Position.TotalSeconds;
          playingPoint = (livePoint - playingPoint);

          double timeShiftStartPointPercent = ((double)timeShiftStartPoint) / ((double)programDuration);
          double playingPointPercent = ((double)playingPoint) / ((double)programDuration);
          double percentLivePoint = ((double)livePoint) / ((double)programDuration);
          if (timeShiftStartPointPercent < 0) timeShiftStartPointPercent = 0;
          if (playingPointPercent < 0) playingPointPercent = 0;
          if (percentLivePoint < 0) percentLivePoint = 0;

          partOrange.Width = percentLivePoint * totalWidth;
          partGreen.Width = playingPointPercent * totalWidth;
          partRed.Width = timeShiftStartPointPercent * totalWidth;


        }
        else
        {
          partOrange.Width = 0;
          partRed.Width = 0;
          float percent = (float)(player.Position.TotalSeconds / player.Duration.TotalSeconds);
          partGreen.Width = percent * totalWidth;

          labelStart.Content = "00:00";
          if (player.Duration.Minutes < 10)
            labelEnd.Content = String.Format("{0}:0{1}", player.Duration.Hours, player.Duration.Minutes);
          else
            labelEnd.Content = String.Format("{0}:{1}", player.Duration.Hours, player.Duration.Minutes);
        }
      }
      else
      {
        partOrange.Width = 0;
        partRed.Width = 0;
        float percent = (float)(player.Position.TotalSeconds / player.Duration.TotalSeconds);
        partGreen.Width = percent * totalWidth;

        labelStart.Content = "00:00";
        if (player.Duration.Minutes < 10)
          labelEnd.Content = String.Format("{0}:0{1}", player.Duration.Hours, player.Duration.Minutes);
        else
          labelEnd.Content = String.Format("{0}:{1}", player.Duration.Hours, player.Duration.Minutes);
      }
      if (player.IsPaused)
        labelState.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 117);//|| Paused;
      else
        labelState.Content = "";

      if (_reachedEnd)
      {
        labelState.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 122)/*end*/;
      }
      else if (_reachedStart)
      {
        labelState.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 121)/*start*/;
      }
      else if (_currentSeekStep > 0)
      {
        TimeSpan ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep]);
        string secs = ts.Seconds.ToString();
        string mins = ts.Minutes.ToString();
        string hours = ts.Hours.ToString();
        string label = "";
        if (ts.Hours > 0)
          label = String.Format("{0} {1}", hours, ServiceScope.Get<ILocalisation>().ToString("mytv", 120)/*hours*/);
        else if (ts.Minutes > 0)
          label = String.Format("{0} {1}", mins, ServiceScope.Get<ILocalisation>().ToString("mytv", 119)/*mins*/);
        else
          label = String.Format("{0} {1}", secs, ServiceScope.Get<ILocalisation>().ToString("mytv", 118)/*secs*/);
        if (_seekDirection == SeekDirection.Past)
          label = "-" + label;
        labelState.Content = label;
      }
      if (_zapChannel != "")
      {
        labelState.Content = _zapChannel;
      }
      UpdateBottomOsd();
    }
    void UpdateBottomOsd()
    {
      gridOSDBottom.Visibility = _bottomOsdVisible ? Visibility.Visible : Visibility.Hidden;
      if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return;
      Channel ch = null;
      if (_zapChannel != "")
      {
        int channelNr = 0;
        if (Int32.TryParse(_zapChannel, out channelNr))
        {
          channelNr--;
          ChannelGroup group = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup;
          if (group != null)
          {
            IList maps = group.ReferringGroupMap();
            if (channelNr >= 0 && channelNr < maps.Count)
            {
              GroupMap map = (GroupMap)maps[channelNr];
              ch = map.ReferencedChannel();
            }
          }
        }
      }
      else
      {
        ch = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
      }
      if (ch == null) return;
      Program program = ch.CurrentProgram;
      if (program == null) return;
      double totalWidth = osdBottomProgressBackground.ActualWidth;
      // caclulate total duration of the current program
      TimeSpan ts = (program.EndTime - program.StartTime);
      double programDuration = ts.TotalSeconds;

      //calculate where the program is at this time
      ts = (DateTime.Now - program.StartTime);
      double livePoint = ts.TotalSeconds;
      double percentLivePoint = ((double)livePoint) / ((double)programDuration);
      if (percentLivePoint < 0) percentLivePoint = 0;
      osdBottomProgressBarGreen.Width = percentLivePoint * totalWidth;

      osdBottomStartTime.Content = program.StartTime.ToString("HH:mm");
      osdBottomEndTime.Content = program.EndTime.ToString("HH:mm");
      osdBottomGenre.Content = program.Genre;
      osdBottomTitle.Content = program.Title;
      osdBottomChannelName.Content = ch.Name;
    }

    bool CanSeek(TimeSpan ts, ref bool reachedStart, ref bool reachedEnd)
    {
      _seekTimeoutTimer.Stop();
      _seekTimeoutTimer.IsEnabled = true;
      _seekTimeoutTimer.Start();
      if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return false;
      TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
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
    void OnSeek(Key key)
    {
      if (_zapChannel != "") return;
      if (_seekDirection == SeekDirection.Unknown)
      {
        if (key == Key.Right) _seekDirection = SeekDirection.Future;
        if (key == Key.Left) _seekDirection = SeekDirection.Past;
      }
      switch (_seekDirection)
      {
        case SeekDirection.Past:
          if (key == Key.Left)
          {
            if (_currentSeekStep + 1 < _seekSteps.Length)
            {
              TimeSpan ts = new TimeSpan(0, 0, -_seekSteps[_currentSeekStep + 1]);
              if (CanSeek(ts, ref _reachedStart, ref _reachedEnd))
              {
                _currentSeekStep++;
              }
              UpdateTopOsd();
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
                UpdateTopOsd();
              }
            }
            else
            {
              _seekDirection = SeekDirection.Unknown;
              _currentSeekStep = 0;
              _reachedEnd = _reachedStart = false;
              UpdateTopOsd();
            }
          }
          break;
        case SeekDirection.Future:
          if (key == Key.Right)
          {
            if (_currentSeekStep + 1 < _seekSteps.Length)
            {
              TimeSpan ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep + 1]);
              if (CanSeek(ts, ref _reachedStart, ref _reachedEnd))
              {
                _currentSeekStep++;
              }
              UpdateTopOsd();
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
              UpdateTopOsd();
            }
            else
            {
              _seekDirection = SeekDirection.Unknown;
              _currentSeekStep = 0;
              _reachedEnd = _reachedStart = false;
              UpdateTopOsd();
            }
          }
          break;
      }
    }
    void seekTimeoutEvent(object sender, EventArgs e)
    {
      _seekTimeoutTimer.Stop();
      _seekTimeoutTimer.IsEnabled = false;
      if (ServiceScope.Get<IPlayerCollectionService>().Count != 0)
      {
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (_reachedStart)
          player.Position = new TimeSpan(0, 0, 0);
        else if (_reachedEnd)
        {
          TimeSpan newPos = player.Duration + new TimeSpan(0, 0, 0, 0, -100);
          player.Position = newPos;
        }
        else
        {
          TimeSpan ts;
          if (_seekDirection == SeekDirection.Past)
            ts = new TimeSpan(0, 0, -_seekSteps[_currentSeekStep]);
          else
            ts = new TimeSpan(0, 0, _seekSteps[_currentSeekStep]);

          TimeSpan newPosition = ts + player.Position;
          player.Position = ts;
        }
      }
      _seekDirection = SeekDirection.Unknown;
      _currentSeekStep = 0;
      _reachedEnd = _reachedStart = false;
      UpdateTopOsd();
    }
    void zapTimeoutEvent(object sender, EventArgs e)
    {
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = false;
      _bottomOsdVisible = false;
      int channelNr = 0;
      if (Int32.TryParse(_zapChannel, out channelNr))
      {
        channelNr--;
        ChannelGroup group = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup;
        if (group != null)
        {
          IList maps = group.ReferringGroupMap();
          if (channelNr >= 0 && channelNr < maps.Count)
          {
            GroupMap map = (GroupMap)maps[channelNr];
            ViewChannel(map.ReferencedChannel());
          }
        }
      }
      _zapChannel = "";
      UpdateTopOsd();
    }

    void OnChannelDown()
    {
      if (_zapChannel.Length == 0)
      {
        _zapChannel = "2";
        ChannelGroup group = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup;
        if (group != null)
        {
          IList maps = group.ReferringGroupMap();
          for (int i = 0; i < maps.Count; ++i)
          {
            GroupMap map = (GroupMap)maps[i];
            if (map.ReferencedChannel() == ServiceScope.Get<ITvChannelNavigator>().SelectedChannel)
            {
              i++;
              _zapChannel = i.ToString();
              break;
            }
          }
        }
      }
      int channelNr = 0;
      if (Int32.TryParse(_zapChannel, out channelNr))
      {
        channelNr--;
        if (channelNr >= 1)
        {
          _zapChannel = channelNr.ToString();
        }
      }
      _bottomOsdVisible = true;
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = true;
      _zapTimeoutTimer.Start();
      UpdateTopOsd();
    }
    void OnChannelUp()
    {
      if (_zapChannel.Length == 0)
      {
        _zapChannel = "0";
        ChannelGroup group = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup;
        if (group != null)
        {
          IList maps = group.ReferringGroupMap();
          for (int i = 0; i < maps.Count; ++i)
          {
            GroupMap map = (GroupMap)maps[i];
            if (map.ReferencedChannel() == ServiceScope.Get<ITvChannelNavigator>().SelectedChannel)
            {
              i++;
              _zapChannel = i.ToString();
              break;
            }
          }
        }
      }
      int channelNr = 0;
      if (Int32.TryParse(_zapChannel, out channelNr))
      {
        channelNr++;
        ChannelGroup group = ServiceScope.Get<ITvChannelNavigator>().CurrentGroup;
        if (group != null)
        {
          IList maps = group.ReferringGroupMap();
          if ((channelNr - 1) >= 0 && (channelNr - 1) < maps.Count)
          {
            _zapChannel = channelNr.ToString();
          }
        }
      }
      _bottomOsdVisible = true;
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = true;
      _zapTimeoutTimer.Start();
      UpdateTopOsd();
      return;
    }
    void OnChannelKey(Key key)
    {
      if (_seekDirection != SeekDirection.Unknown) return;
      if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return;
      TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
      if (player.Card == null) return;
      if (player.Card.IsTimeShifting == false && player.Card.IsRecording == false) return;

      int number = key - Key.D0;
      _bottomOsdVisible = true;
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = true;
      _zapTimeoutTimer.Start();
      _zapChannel += number.ToString();
      UpdateTopOsd();
    }

    #region zapping
    /// <summary>
    /// Start viewing the tv channel 
    /// </summary>
    /// <param name="channel">The channel.</param>
    void ViewChannel(Channel channel)
    {
      ServiceScope.Get<ILogger>().Info("Tv: view channel:{0}", channel.Name);
      ICommand cmd=_model.TimeShift;
    }

    #endregion
  }
}