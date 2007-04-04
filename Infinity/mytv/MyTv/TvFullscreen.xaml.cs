using System;
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
    new System.Windows.Threading.DispatcherTimer _seekTimeoutTimer;

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
      _seekTimeoutTimer = new System.Windows.Threading.DispatcherTimer();
      _seekTimeoutTimer.Interval = new TimeSpan(0, 0, 1);
      _seekTimeoutTimer.IsEnabled = false;
      _seekTimeoutTimer.Tick += new EventHandler(seekTimeoutEvent);

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
      UpdateOsd();
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
          UpdateOsd();
        }
        return;
      }
    }
    /// <summary>
    /// Updates the osd.
    /// </summary>
    void UpdateOsd()
    {
      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
      if (player.IsPaused || _seekDirection != SeekDirection.Unknown)
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
              UpdateOsd();
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
                UpdateOsd();
              }
            }
            else
            {
              _seekDirection = SeekDirection.Unknown;
              _currentSeekStep = 0;
              _reachedEnd = _reachedStart = false;
              UpdateOsd();
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
              UpdateOsd();
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
              UpdateOsd();
            }
            else
            {
              _seekDirection = SeekDirection.Unknown;
              _currentSeekStep = 0;
              _reachedEnd = _reachedStart = false;
              UpdateOsd();
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
      UpdateOsd();
    }
  }
}