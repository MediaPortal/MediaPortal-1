using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvScheduled.xaml
  /// </summary>

  public partial class TvScheduled : System.Windows.Controls.Page
  {
    TvScheduledViewModel _model;
    public TvScheduled()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _model = new TvScheduledViewModel(this);
      gridMain.DataContext = _model;

      //this.InputBindings.Add(new KeyBinding(_model.FullScreenTv, new KeyGesture(System.Windows.Input.Key.X, ModifierKeys.None)));
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      // Sets keyboard focus on the first Button in the sample.
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(OnPreviewKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMove));
      Keyboard.Focus(buttonSort);
      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseDownEvent), true);
      this.KeyDown += new KeyEventHandler(OnKeyDown);
    }

    void OnMouseMove(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element != null)
      {
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
        if (element as ListBoxItem != null)
        {
          Keyboard.Focus((ListBoxItem)element);
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
    }
    void OnKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        ListBox box = e.Source as ListBox;
        //OnScheduleClicked(box);
        e.Handled = true;
        return;
      }
    }
    void OnMouseDownEvent(object sender, RoutedEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      ListBox box = e.Source as ListBox;
      //OnScheduleClicked(box);
    }
    protected void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Left)
      {
        Keyboard.Focus(buttonSort);
        e.Handled = true;
        return;
      }
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          this.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
    }

#if NOTUSE
    /// <summary>
    /// Called when user has clicked on a recording
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnScheduleClicked()
    {
      DialogMenuItem item = gridList.SelectedItem as DialogMenuItem;
      if (item == null) return;
      Schedule rec = item.Tag as Schedule;
      if (rec == null) return;

      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.SelectedIndex = (int)0;
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      int[] options = new int[10];
      int option = 0;
      if (rec.Series == false)
      {
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 56)/*Delete*/));
        options[option++] = 618;
      }
      else
      {
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 112)/*Cancel this show*/));
        options[option++] = 981;
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 113)/*Delete this entire recording*/));
        options[option++] = 982;
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 50)/*Episodes management*/));
        options[option++] = 888;
      }
      VirtualCard card;
      TvServer server = new TvServer();
      if (server.IsRecordingSchedule(rec.IdSchedule, out card))
      {
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 114)/*Play recording from beginning*/));
        options[option++] = 979;
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 115)/*Play recording from live point*/));
        options[option++] = 980;
      }
      else
      {
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 49)/*Quality settings*/));
        options[option++] = 882;
      }
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 94)/*Settings*/));
      options[option++] = 1048;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex == -1) return;

      string fileName = "";
      if (server.IsRecordingSchedule(rec.IdSchedule, out card))
      {
        fileName = card.RecordingFileName;
      }
      switch (options[dlgMenu.SelectedIndex])
      {
        case 888:////Episodes management
          //TvPriorities.OnSetEpisodesToKeep(rec);
          break;

        case 1048:////settings
          TvScheduleInfo infopage = new TvScheduleInfo(rec);
          this.NavigationService.Navigate(infopage);
          //TVProgramInfo.CurrentRecording = item.MusicTag as Schedule;
          //GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
          return;
        case 882:////Quality settings
          //GUITVPriorities.OnSetQuality(rec);
          break;

        case 981: //Cancel this show
          {
            if (server.IsRecordingSchedule(rec.IdSchedule, out card))
            {

              MpDialogYesNo dlgYesNo = new MpDialogYesNo();
              dlgYesNo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
              dlgYesNo.Owner = w;
              dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
              dlgYesNo.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording? This schedule is recording. If you delete the schedule then the recording is stopped.";
              dlgYesNo.ShowDialog();
              if (dlgYesNo.DialogResult == DialogResult.No) return;
              server.StopRecordingSchedule(rec.IdSchedule);
              CanceledSchedule schedule = new CanceledSchedule(rec.IdSchedule, rec.StartTime);
              schedule.Persist();
              server.OnNewSchedule();
            }
            else
            {
              server.StopRecordingSchedule(rec.IdSchedule);
              CanceledSchedule schedule = new CanceledSchedule(rec.IdSchedule, rec.StartTime);
              schedule.Persist();
              server.OnNewSchedule();
            }
            LoadSchedules();
          }
          break;

        case 982: //Delete series recording
          goto case 618;

        case 618: // delete entire recording
          {
            if (server.IsRecordingSchedule(rec.IdSchedule, out card))
            {
              MpDialogYesNo dlgYesNo = new MpDialogYesNo();
              dlgYesNo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
              dlgYesNo.Owner = w;
              dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
              dlgYesNo.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording? This schedule is recording. If you delete the schedule then the recording is stopped.";
              dlgYesNo.ShowDialog();
              if (dlgYesNo.DialogResult == DialogResult.No) return;
              server.StopRecordingSchedule(rec.IdSchedule);
            }
            else
            {
              rec = Schedule.Retrieve(rec.IdSchedule);
              rec.Delete();
              server.OnNewSchedule();

            }
            LoadSchedules();
          }
          break;

        case 979: // Play recording from beginning
          {
            Play(fileName, true);
          }
          return;

        case 980: // Play recording from live point
          {
            Play(fileName, false);
          }
          break;
      }
    }
    void Play(string fileName, bool fromBeginning)
    {
      //videoWindow.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
      TvPlayerCollection.Instance.DisposeAll();
      TvMediaPlayer player = TvPlayerCollection.Instance.Get(null, fileName);
      player.MediaFailed += new EventHandler<ExceptionEventArgs>(_mediaPlayer_MediaFailed);
      if (fromBeginning == false)
      {
        player.MediaOpened += new EventHandler(_mediaPlayer_MediaOpened);
      }
      //create video drawing which draws the video in the video window
      VideoDrawing videoDrawing = new VideoDrawing();
      videoDrawing.Player = player;
      videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
      DrawingBrush videoBrush = new DrawingBrush();
      videoBrush.Drawing = videoDrawing;
      videoWindow.Fill = videoBrush;
      videoDrawing.Player.Play();
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
      buttonSort.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
    }
    /// <summary>
    /// Called when media player has an error condition
    /// show messagebox to user and close media playback
    /// </summary>
    void OnMediaPlayerError()
    {

      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
      videoWindow.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
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
      buttonSort.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new SeekToEndDelegate(OnSeekToEnd));
    }
    void OnSeekToEnd()
    {
      if (TvPlayerCollection.Instance.Count != 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        if (player.NaturalDuration.HasTimeSpan)
        {
          TimeSpan duration = player.NaturalDuration.TimeSpan;
          player.Position = duration;
        }
      }
    }



#endif
  }
}