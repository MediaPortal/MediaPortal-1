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
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvFullscreen.xaml
  /// </summary>

  public partial class TvFullscreen : System.Windows.Controls.Page
  {
    private delegate void StartTimeShiftingDelegate(Channel channel);
    private delegate void EndTimeShiftingDelegate(TvResult result, VirtualCard card);
    private delegate void SeekToEndDelegate();
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

      if (TvPlayerCollection.Instance.Count > 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        player.MediaEnded += new EventHandler(player_MediaEnded);
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, gridMain.ActualWidth, gridMain.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        gridMain.Background = videoBrush;
      }
      Keyboard.Focus(gridMain);
      UpdateTopOsd();
    }


    /// <summary>
    /// Handles the MediaEnded event of the player control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void player_MediaEnded(object sender, EventArgs e)
    {

      this.NavigationService.GoBack();
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt))
      {
        if (Keyboard.IsKeyDown(System.Windows.Input.Key.Enter))
        {
          Window window = Window.GetWindow(this);
          if (window.WindowState == System.Windows.WindowState.Maximized)
          {
            window.ShowInTaskbar = true;
            WindowTaskbar.Show(); ;
            window.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            window.WindowState = System.Windows.WindowState.Normal;
          }
          else
          {
            window.ShowInTaskbar = false;
            window.WindowStyle = System.Windows.WindowStyle.None;
            WindowTaskbar.Hide(); ;
            window.WindowState = System.Windows.WindowState.Maximized;
          }
          e.Handled = true;
          return;
        }
      }
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
        this.NavigationService.GoBack();
        return;
      }
      if (e.Key == Key.Space)
      {
        e.Handled = true;
        if (TvPlayerCollection.Instance.Count > 0)
        {
          TvMediaPlayer player = TvPlayerCollection.Instance[0];
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
      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
      if (player.IsPaused || _seekDirection != SeekDirection.Unknown || _bottomOsdVisible)
        gridOSD.Visibility = Visibility.Visible;
      else
        gridOSD.Visibility = Visibility.Hidden;

      double totalWidth = gridProgressBack.ActualWidth;
      if (player.Card != null)
      {
        if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
        {
          Channel channel = ChannelNavigator.Instance.SelectedChannel;
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
      if (TvPlayerCollection.Instance.Count == 0) return;
      Channel ch = null;
      if (_zapChannel != "")
      {
        int channelNr = 0;
        if (Int32.TryParse(_zapChannel, out channelNr))
        {
          channelNr--;
          ChannelGroup group = ChannelNavigator.Instance.CurrentGroup;
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
        ch = ChannelNavigator.Instance.SelectedChannel;
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
      if (TvPlayerCollection.Instance.Count == 0) return false;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
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
      if (TvPlayerCollection.Instance.Count != 0)
      {
        TvMediaPlayer player = TvPlayerCollection.Instance[0];
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
        ChannelGroup group = ChannelNavigator.Instance.CurrentGroup;
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
        ChannelGroup group = ChannelNavigator.Instance.CurrentGroup;
        if (group != null)
        {
          IList maps = group.ReferringGroupMap();
          for (int i=0; i < maps.Count;++i)
          {
            GroupMap map = (GroupMap)maps[i];
            if (map.ReferencedChannel() == ChannelNavigator.Instance.SelectedChannel)
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
        ChannelGroup group = ChannelNavigator.Instance.CurrentGroup;
        if (group != null)
        {
          IList maps = group.ReferringGroupMap();
          for (int i = 0; i < maps.Count; ++i)
          {
            GroupMap map = (GroupMap)maps[i];
            if (map.ReferencedChannel() == ChannelNavigator.Instance.SelectedChannel)
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
        ChannelGroup group = ChannelNavigator.Instance.CurrentGroup;
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
      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
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
      ChannelNavigator.Instance.SelectedChannel = channel;
      //tell server to start timeshifting the channel
      //we do this in the background so GUI stays responsive...
      StartTimeShiftingDelegate starter = new StartTimeShiftingDelegate(this.StartTimeShiftingBackGroundWorker);
      starter.BeginInvoke(channel, null, null);
    }

    /// <summary>
    /// Starts the timeshifting 
    /// this is done in the background so the GUI stays responsive
    /// </summary>
    /// <param name="channel">The channel.</param>
    private void StartTimeShiftingBackGroundWorker(Channel channel)
    {
      ServiceScope.Get<ILogger>().Info("Tv:  start timeshifting channel:{0}", channel.Name);
      TvServer server = new TvServer();
      VirtualCard card;

      User user = new User();
      TvResult succeeded = TvResult.Succeeded;
      succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);

      // Schedule the update function in the UI thread.
      this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new EndTimeShiftingDelegate(OnStartTimeShiftingResult), succeeded, card);
    }
    /// <summary>
    /// Called from dispatcher when StartTimeShiftingBackGroundWorker() has a result for us
    /// we check the result and if needed start a new media player to playback the tv timeshifting file
    /// </summary>
    /// <param name="succeeded">The result.</param>
    /// <param name="card">The card.</param>
    private void OnStartTimeShiftingResult(TvResult succeeded, VirtualCard card)
    {
      ServiceScope.Get<ILogger>().Info("Tv:  timeshifting channel:{0} result:{1}", ChannelNavigator.Instance.SelectedChannel.Name, succeeded);
      if (succeeded == TvResult.Succeeded)
      {
        //timeshifting worked, now view the channel
        ChannelNavigator.Instance.Card = card;
        //do we already have a media player ?
        if (TvPlayerCollection.Instance.Count != 0)
        {
          if (TvPlayerCollection.Instance[0].FileName != card.TimeShiftFileName)
          {
            gridMain.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            TvPlayerCollection.Instance.DisposeAll();
          }
        }
        if (TvPlayerCollection.Instance.Count != 0)
        {
          this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new SeekToEndDelegate(OnSeekToEnd));
          return;
        }
        //create a new media player 
        ServiceScope.Get<ILogger>().Info("Tv:  open file", card.TimeShiftFileName);
        MediaPlayer player = TvPlayerCollection.Instance.Get(card, card.TimeShiftFileName);
        player.MediaFailed += new EventHandler<ExceptionEventArgs>(_mediaPlayer_MediaFailed);
        player.MediaOpened += new EventHandler(_mediaPlayer_MediaOpened);

        //create video drawing which draws the video in the video window
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, gridMain.ActualWidth, gridMain.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        gridMain.Background = videoBrush;
        videoDrawing.Player.Play();

      }
      else
      {
        //close media player
        if (TvPlayerCollection.Instance.Count != 0)
        {
          TvPlayerCollection.Instance.DisposeAll();
        }

        //show error to user
        MpDialogOk dlg = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 23);//"Failed to start TV;
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);/*(Error)*/
        switch (succeeded)
        {
          case TvResult.AllCardsBusy:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 24); //"All cards are currently busy";
            break;
          case TvResult.CardIsDisabled:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 25);// "Card is disabled";
            break;
          case TvResult.ChannelIsScrambled:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 26);//"Channel is scrambled";
            break;
          case TvResult.ChannelNotMappedToAnyCard:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 27);//"Channel is not mapped to any tv card";
            break;
          case TvResult.ConnectionToSlaveFailed:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 28);//"Failed to connect to slave server";
            break;
          case TvResult.NotTheOwner:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 29);//"Card is owned by another user";
            break;
          case TvResult.NoTuningDetails:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 30);//"Channel does not have tuning information";
            break;
          case TvResult.NoVideoAudioDetected:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 31);//"No Video/Audio streams detected";
            break;
          case TvResult.UnableToStartGraph:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 32);//"Unable to start graph";
            break;
          case TvResult.UnknownChannel:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 33);//"Unknown channel";
            break;
          case TvResult.UnknownError:
            dlg.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 34);//"Unknown error occured";
            break;
        }
        dlg.ShowDialog();
      }
    }
    #region media player events & dispatcher methods
    void OnSeekToEnd()
    {
      if (TvPlayerCollection.Instance.Count != 0)
      {
        ServiceScope.Get<ILogger>().Info("Tv:  seek to livepoint");
        TvMediaPlayer player = TvPlayerCollection.Instance[0];

        if (player.NaturalDuration.HasTimeSpan)
        {
          TimeSpan duration = player.Duration;
          TimeSpan newPos = duration + new TimeSpan(0, 0, 0, 0, -500);
          ServiceScope.Get<ILogger>().Info("MyTv: OnSeekToEnd current {0}/{1}", newPos, player.Duration);
          if (!player.IsStream)
          {
            ServiceScope.Get<ILogger>().Info("MyTv: Seek to {0}/{1}", newPos, duration);
            player.Position = newPos;
          }
        }

      }
    }

    /// <summary>
    /// Called when media player has an error condition
    /// show messagebox to user and close media playback
    /// </summary>
    void OnMediaPlayerError()
    {

      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
      ServiceScope.Get<ILogger>().Info("Tv:  failed to open file {0} error:{1}", player.FileName, player.ErrorMessage);
      gridMain.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
      if (player.HasError)
      {
        MpDialogOk dlgError = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgError.Owner = w;
        dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);//"Cannot open file";
        dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);//"Error";
        dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 38)/*Unable to open the file*/ + player.ErrorMessage;
        dlgError.ShowDialog();
      }
      TvPlayerCollection.Instance.DisposeAll();

    }
    /// <summary>
    /// Handles the MediaOpened event of the _mediaPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void _mediaPlayer_MediaOpened(object sender, EventArgs e)
    {
      //media is opened, seek to end (via dispatcher)
      this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new SeekToEndDelegate(OnSeekToEnd));
    }

    /// <summary>
    /// Handles the MediaFailed event of the _mediaPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Media.ExceptionEventArgs"/> instance containing the event data.</param>
    void _mediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      // media player failed to open file
      // show error dialog (via dispatcher)
      this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
    }

    #endregion
    #endregion
  }
}