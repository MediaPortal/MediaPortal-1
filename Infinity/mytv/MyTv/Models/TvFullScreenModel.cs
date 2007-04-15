using System;
using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TvDatabase;
using TvControl;
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Settings;

namespace MyTv
{
  public class TvFullScreenModel : TvBaseViewModel
  {
    #region variables
    Visibility _bottomOsdVisibility = Visibility.Hidden;
    enum SeekDirection
    {
      Unknown,
      Past,
      Future
    }
    List<int> _seekSteps = new List<int>();
    int _currentSeekStep = 0;

    SeekDirection _seekDirection = SeekDirection.Unknown;
    bool _reachedEnd = false;
    bool _reachedStart = false;
    System.Windows.Threading.DispatcherTimer _seekTimeoutTimer;
    System.Windows.Threading.DispatcherTimer _zapTimeoutTimer;
    double _topOsdBarWidth;
    double _bottomOsdBarWidth;
    string _zapChannel;

    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvFullScreenModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public TvFullScreenModel(Page page)
      : base(page)
    {
      //get the seeking steps from the configuration file...
      TvSettings settings = new TvSettings();
      ServiceScope.Get<ISettingsManager>().Load(settings, "configuration.xml");
      string[] steps=settings.SeekSteps.Split(new char[]{','});
      for (int i = 0; i < steps.Length; ++i)
      {
        _seekSteps.Add(Int32.Parse(steps[i]));
      }

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
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the top osd visibility.
    /// </summary>
    /// <value>The top osd visibility.</value>
    public Visibility TopOsdVisibility
    {
      get
      {
        if (_seekDirection != SeekDirection.Unknown || _bottomOsdVisibility==Visibility.Visible)
          return Visibility.Visible;
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return Visibility.Hidden;
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.IsPaused)
          return Visibility.Visible;
        return Visibility.Hidden;
      }
    }
    /// <summary>
    /// Gets the bottom osd visibility.
    /// </summary>
    /// <value>The bottom osd visibility.</value>
    public Visibility BottomOsdVisibility
    {
      get
      {
        return _bottomOsdVisibility;
      }
    }

    /// <summary>
    /// Gets the top osd start time.
    /// </summary>
    /// <value>The top osd start time.</value>
    public string StartTime
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return "00:00";
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.Card != null)
        {
          if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
          {
            Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
            Program program = channel.CurrentProgram;
            return channel.CurrentProgram.StartTime.ToString("HH:mm");
          }
        }
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
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return "00:00";
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.Card != null)
        {
          if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
          {
            Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
            Program program = channel.CurrentProgram;
            return channel.CurrentProgram.EndTime.ToString("HH:mm");
          }
        }

        string endTime = "00:00";
        if (player.Duration.Minutes < 10)
          endTime = String.Format("{0}:0{1}", player.Duration.Hours, player.Duration.Minutes);
        else
          endTime = String.Format("{0}:{1}", player.Duration.Hours, player.Duration.Minutes);
        return endTime;
      }
    }

    /// <summary>
    /// Gets the value of the orange part of the top progress bar .
    /// </summary>
    /// <value>The top progress bar orange.</value>
    public double TopProgressBarOrange
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return 0;
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.Card != null)
        {
          if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
          {
            Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
            Program program = channel.CurrentProgram;

            // caclulate total duration of the current program
            TimeSpan ts = (channel.CurrentProgram.EndTime - channel.CurrentProgram.StartTime);
            double programDuration = ts.TotalSeconds;

            //calculate where the program is at this time
            ts = (DateTime.Now - channel.CurrentProgram.StartTime);
            double livePoint = ts.TotalSeconds;

            double percentLivePoint = ((double)livePoint) / ((double)programDuration);
            if (percentLivePoint < 0) percentLivePoint = 0;
            return percentLivePoint * 100;
          }
        }
        return 0;
      }
    }
    /// <summary>
    /// Gets the value of the red part of the top progress bar .
    /// </summary>
    /// <value>The top progress bar orange.</value>
    public double TopProgressBarRed
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return 0;
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.Card != null)
        {
          if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
          {
            Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
            Program program = channel.CurrentProgram;


            // caclulate total duration of the current program
            TimeSpan ts = (channel.CurrentProgram.EndTime - channel.CurrentProgram.StartTime);
            double programDuration = ts.TotalSeconds;

            //calculate where the program is at this time
            ts = (DateTime.Now - channel.CurrentProgram.StartTime);
            double livePoint = ts.TotalSeconds;

            //calculate when timeshifting was started
            double timeShiftStartPoint = livePoint - player.Duration.TotalSeconds;
            if (timeShiftStartPoint < 0) timeShiftStartPoint = 0;

            double timeShiftStartPointPercent = ((double)timeShiftStartPoint) / ((double)programDuration);
            if (timeShiftStartPointPercent < 0) timeShiftStartPointPercent = 0;
            return timeShiftStartPointPercent * 100;
          }
        }
        return 0;
      }
    }
    /// <summary>
    /// Gets the value of the green part of the top progress bar .
    /// </summary>
    /// <value>The top progress bar green.</value>
    public double TopProgressBarGreen
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return 0;
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.Card != null)
        {
          if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
          {
            Channel channel = ServiceScope.Get<ITvChannelNavigator>().SelectedChannel;
            Program program = channel.CurrentProgram;


            // caclulate total duration of the current program
            TimeSpan ts = (channel.CurrentProgram.EndTime - channel.CurrentProgram.StartTime);
            double programDuration = ts.TotalSeconds;

            //calculate where the program is at this time
            ts = (DateTime.Now - channel.CurrentProgram.StartTime);
            double livePoint = ts.TotalSeconds;

            //calculate where we the current playing point is
            double playingPoint = player.Duration.TotalSeconds - player.Position.TotalSeconds;
            playingPoint = (livePoint - playingPoint);
            double playingPointPercent = ((double)playingPoint) / ((double)programDuration);
            if (playingPointPercent < 0) playingPointPercent = 0;
            return playingPointPercent * 100;
          }
        }

        float percent = (float)(player.Position.TotalSeconds / player.Duration.TotalSeconds);
        return percent * 100;
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
        if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return "";
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];

        string labelState = "";
        if (player.IsPaused)
          labelState = ServiceScope.Get<ILocalisation>().ToString("mytv", 117);//|| Paused;

        if (_reachedEnd)
        {
          labelState = ServiceScope.Get<ILocalisation>().ToString("mytv", 122)/*end*/;
        }
        else if (_reachedStart)
        {
          labelState = ServiceScope.Get<ILocalisation>().ToString("mytv", 121)/*start*/;
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
          labelState = label;
        }
        if (_zapChannel != "")
        {
          labelState = _zapChannel;
        }
        return labelState;
      }
    }
    /// <summary>
    /// Gets the channel wher are going to zap to.
    /// </summary>
    /// <value>The  channel.</value>
    public Channel ZapChannel
    {
      get
      {
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
        return ch;
      }
    }
    /// <summary>
    /// Gets the start time of the current program on the channel we're going to zap to
    /// </summary>
    /// <value>The zap channel start time.</value>
    public string ZapChannelStartTime
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        Program program = ch.CurrentProgram;
        if (program == null) return "";
        return program.StartTime.ToString("HH:mm"); 
      }
    }
    /// <summary>
    /// Gets the end time of the current program on the channel we're going to zap to
    /// </summary>
    /// <value>The zap channel end time.</value>
    public string ZapChannelEndTime
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        Program program = ch.CurrentProgram;
        if (program == null) return "";
        return program.EndTime.ToString("HH:mm"); 
      }
    }
    /// <summary>
    /// Gets the genre of the current program on the channel we're going to zap to
    /// </summary>
    /// <value>The zap channel genre.</value>
    public string ZapChannelGenre
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        Program program = ch.CurrentProgram;
        if (program == null) return "";
        return program.Genre;
      }
    }
    /// <summary>
    /// Gets the title of the current program on the channel we're going to zap to
    /// </summary>
    /// <value>The zap channel title.</value>
    public string ZapChannelTitle
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        Program program = ch.CurrentProgram;
        if (program == null) return "";
        return program.Title;
      }
    }
    /// <summary>
    /// Gets the name of the channel on the channel we're going to zap to
    /// </summary>
    /// <value>The name of the zap channel.</value>
    public string ZapChannelName
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        return ch.Name;
      }
    }
    /// <summary>
    /// Gets the description of the current program on the channel we're going to zap to
    /// </summary>
    /// <value>The zap channel description.</value>
    public string ZapChannelDescription
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        Program program = ch.CurrentProgram;
        if (program == null) return "";
        return program.Description;
      }
    }
    /// <summary>
    /// gets the value (0-100) how far the program is for the channel we're zapping too.
    /// </summary>
    /// <value>The width of the zap channel.</value>
    public double ZapChannelWidth
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return 0;
        Program program = ch.CurrentProgram;
        if (program == null) return 0;

        // caclulate total duration of the current program
        TimeSpan ts = (program.EndTime - program.StartTime);
        double programDuration = ts.TotalSeconds;

        //calculate where the program is at this time
        ts = (DateTime.Now - program.StartTime);
        double livePoint = ts.TotalSeconds;
        double percentLivePoint = ((double)livePoint) / ((double)programDuration);
        if (percentLivePoint < 0) percentLivePoint = 0;
        return percentLivePoint * 100;
      }
    }
    /// <summary>
    /// Gets the logo of  the channel we're going to zap to
    /// </summary>
    /// <value>The zap channel logo.</value>
    public string ZapChannelLogo
    {
      get
      {
        Channel ch = ZapChannel;
        if (ch == null) return "";
        string logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(ch.Name));
        if (!System.IO.File.Exists(logo))
        {
          logo = "";
        }
        return logo;
      }
    }
    #endregion

    #region seeking
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
    /// <summary>
    /// Handles left/right keys for seeking
    /// </summary>
    /// <param name="key">The key.</param>
    public void OnSeek(Key key)
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
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("LabelState");
      ChangeProperty("TopProgressBarOrange");
      ChangeProperty("TopProgressBarRed");
      ChangeProperty("TopProgressBarGreen");
    }

    #endregion

    #region zapping
    /// <summary>
    /// Timer callback
    /// When occurs, this method will do the actual zapping
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    public void zapTimeoutEvent(object sender, EventArgs e)
    {
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = false;
      _bottomOsdVisibility = Visibility.Hidden;
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
      ChangeProperty("BottomOsdVisibility");
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("StartTime");
      ChangeProperty("EndTime");
      ChangeProperty("TopProgressBarOrange");
      ChangeProperty("TopProgressBarRed");
      ChangeProperty("TopProgressBarGreen");
      ChangeProperty("LabelState");
    }

    /// <summary>
    /// Zap to previous channel
    /// </summary>
    public void OnChannelDown()
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
      _bottomOsdVisibility = Visibility.Visible;
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = true;
      _zapTimeoutTimer.Start();
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("BottomOsdVisibility");
      ChangeProperty("LabelState");
      ChangeProperty("ZapChannel");
      ChangeProperty("ZapChannelStartTime");
      ChangeProperty("ZapChannelEndTime");
      ChangeProperty("ZapChannelTitle");
      ChangeProperty("ZapChannelName");
      ChangeProperty("ZapChannelDescription");
      ChangeProperty("ZapChannelWidth");
      ChangeProperty("ZapChannelLogo");
    }
    /// <summary>
    /// Zap to next channel
    /// </summary>
    public void OnChannelUp()
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
      _bottomOsdVisibility = Visibility.Visible;
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = true;
      _zapTimeoutTimer.Start();
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("BottomOsdVisibility");
      ChangeProperty("LabelState");
      ChangeProperty("ZapChannel");
      ChangeProperty("ZapChannelStartTime");
      ChangeProperty("ZapChannelEndTime");
      ChangeProperty("ZapChannelTitle");
      ChangeProperty("ZapChannelName");
      ChangeProperty("ZapChannelDescription");
      ChangeProperty("ZapChannelWidth");
      ChangeProperty("ZapChannelLogo");
      return;
    }
    /// <summary>
    /// Handles keypresses 0-9
    /// to directly zap to a channel number
    /// </summary>
    /// <param name="key">The key.</param>
    public void OnChannelKey(Key key)
    {
      if (_seekDirection != SeekDirection.Unknown) return;
      if (ServiceScope.Get<IPlayerCollectionService>().Count == 0) return;
      TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
      if (player.Card == null) return;
      if (player.Card.IsTimeShifting == false && player.Card.IsRecording == false) return;

      int number = key - Key.D0;
      _bottomOsdVisibility = Visibility.Visible;
      _zapTimeoutTimer.Stop();
      _zapTimeoutTimer.IsEnabled = true;
      _zapTimeoutTimer.Start();
      _zapChannel += number.ToString();
      _bottomOsdVisibility = Visibility.Visible;
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("BottomOsdVisibility");
      ChangeProperty("LabelState");
      ChangeProperty("ZapChannel");
      ChangeProperty("ZapChannelStartTime");
      ChangeProperty("ZapChannelEndTime");
      ChangeProperty("ZapChannelTitle");
      ChangeProperty("ZapChannelName");
      ChangeProperty("ZapChannelDescription");
      ChangeProperty("ZapChannelWidth");
      ChangeProperty("ZapChannelLogo");
    }
    
    /// <summary>
    /// Start viewing the tv channel 
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void ViewChannel(Channel channel)
    {
      ServiceScope.Get<ILogger>().Info("Tv: view channel:{0}", channel.Name);
      ICommand cmd = TimeShift;
      cmd.Execute(channel);
      ChangeProperty("TopOsdVisibility");
      ChangeProperty("LabelState");
      ChangeProperty("StartTime");
      ChangeProperty("EndTime");
      ChangeProperty("TopProgressBarOrange");
      ChangeProperty("TopProgressBarRed");
      ChangeProperty("TopProgressBarGreen");
      ChangeProperty("BottomOsdVisibility");
      ChangeProperty("ZapChannel");
      ChangeProperty("ZapChannelStartTime");
      ChangeProperty("ZapChannelEndTime");
      ChangeProperty("ZapChannelTitle");
      ChangeProperty("ZapChannelName");
      ChangeProperty("ZapChannelDescription");
      ChangeProperty("ZapChannelWidth");
      ChangeProperty("ZapChannelLogo");
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public void Pause()
    {
      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        TvMediaPlayer player = (TvMediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        player.Pause();
        ChangeProperty("TopOsdVisibility");
        ChangeProperty("LabelState");
      }
    }
    #endregion
  }
}
