using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
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

  public partial class TvScheduled : System.Windows.Controls.Page, IComparer<Schedule>
  {
    private delegate void SeekToEndDelegate();
    private delegate void MediaPlayerErrorDelegate();
    enum SortMode
    {
      Duration,
      Channel,
      Date,
      Title
    };
    SortMode _sortMode = SortMode.Date;
    public TvScheduled()
    {
      InitializeComponent();
    }
    /// <summary>
    /// Called when mouse enters a button
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
        Button button = sender as Button;
        if (button != null)
        {
          ContentPresenter content = button.TemplatedParent as ContentPresenter;
          if (content != null)
          {
            if (content.Content != null)
            {
              gridList.SelectedItem = content.Content;
            }
          }
        }
      }
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Left)
      {
        Keyboard.Focus(buttonSort);
        e.Handled = true;
        return;
      }
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
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
    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      labelHeader.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 111);// "scheduled";
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Keyboard.AddGotKeyboardFocusHandler(gridList, new KeyboardFocusChangedEventHandler(onKeyboardFocus));

      this.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click));
      Keyboard.Focus(buttonSort);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      gridList.SelectionMode = SelectionMode.Single;
      gridList.BorderThickness = new Thickness(0);
      


      if (TvPlayerCollection.Instance.Count > 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        videoWindow.Fill = videoBrush;
      }

      LoadSchedules();

    }
    void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    {
      if (gridList.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
      {
        ListBoxItem focusedItem = gridList.ItemContainerGenerator.ContainerFromIndex(gridList.SelectedIndex) as ListBoxItem;
        if (focusedItem != null)
        {
          Border border = (Border)VisualTreeHelper.GetChild(focusedItem, 0);
          ContentPresenter contentPresenter = VisualTreeHelper.GetChild(border, 0) as ContentPresenter;
          Button b = gridList.ItemTemplate.FindName("PART_Button", contentPresenter) as Button;
          Keyboard.Focus(b);
          b.Focus();
        }
      }
    }
    void onKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      ListBoxItem focusedItem = e.NewFocus as ListBoxItem;
      if (focusedItem != null)
      {
        Border border = (Border)VisualTreeHelper.GetChild(focusedItem, 0);
        ContentPresenter contentPresenter = VisualTreeHelper.GetChild(border, 0) as ContentPresenter;
        Button b = gridList.ItemTemplate.FindName("PART_Button", contentPresenter) as Button;
        Keyboard.Focus(b);
        b.Focus();
        e.Handled = true;
      }
    }

    /// <summary>
    /// Gets the content for right label of each schedule.
    /// This depends on the current sort mode
    /// </summary>
    /// <param name="schedule">The schedule.</param>
    /// <returns></returns>
    string GetContentForRightLabel(Schedule schedule)
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          return schedule.ReferencedChannel().Name;
        case SortMode.Date:
          return schedule.StartTime.ToLongDateString();
        case SortMode.Duration:
          {
            TimeSpan ts = schedule.EndTime - schedule.StartTime;
            if (ts.Minutes < 10)
              return String.Format("{0}:0{1}", ts.Hours, ts.Minutes);
            else
              return String.Format("{0}:{1}", ts.Hours, ts.Minutes);
          }
        case SortMode.Title:
          {
            TimeSpan ts = schedule.EndTime - schedule.StartTime;
            if (ts.Minutes < 10)
              return String.Format("{0}:0{1}", ts.Hours, ts.Minutes);
            else
              return String.Format("{0}:{1}", ts.Hours, ts.Minutes);
          }
      }
      return "";
    }
    /// <summary>
    /// Loads the schedules and shows them onscreen.
    /// </summary>
    void LoadSchedules()
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Sort:Channel";
          break;
        case SortMode.Date:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 85);//"Sort:Date";
          break;
        case SortMode.Title:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 88);//"Sort:Title";
          break;
      }
      DialogMenuItemCollection collection = new DialogMenuItemCollection();

      IList schedules = Schedule.ListAll();
      List<Schedule> listSchedules = new List<Schedule>();
      foreach (Schedule schedule in schedules)
        listSchedules.Add(schedule);
      listSchedules.Sort(this);
      int row = 0;
      foreach (Schedule schedule in listSchedules)
      {
        string logo = Thumbs.GetLogoFileName(schedule.ReferencedChannel().Name + ".png");
        if (!System.IO.File.Exists(logo))
        {
          logo = "";
        }

        DialogMenuItem item = new DialogMenuItem(logo, schedule.ProgramName, schedule.ReferencedChannel().Name, GetContentForRightLabel(schedule));
        item.Tag = schedule;
        collection.Add(item);
      }
      gridList.ItemsSource = collection;
    }

    void button_GotFocus(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      Recording recording = b.Tag as Recording;
      if (recording == null) return;

      labelTitle.Text = recording.Title;
      labelDescription.Text = recording.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", recording.StartTime.ToString("HH:mm"), recording.EndTime.ToString("HH:mm"));
      labelGenre.Text = recording.Genre;
    }

    void gridSub_Loaded(object sender, RoutedEventArgs e)
    {
      Grid g = sender as Grid;
      if (g == null) return;
      g.Width = ((Button)(g.Parent)).ActualWidth;
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
      if (e.Source == gridList)
      {
        OnScheduleClicked();
      }
    }

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
      videoWindow.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
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


    void OnCleanupClicked(object sender, RoutedEventArgs e)
    {
      int iCleaned = 0;
      IList itemlist = Schedule.ListAll();
      foreach (Schedule rec in itemlist)
      {
        if (rec.IsDone() || rec.Canceled != Schedule.MinSchedule)
        {
          iCleaned++;
          Schedule r = Schedule.Retrieve(rec.IdSchedule);
          r.Delete();
        }
      }
      MpDialogOk dlgMenu = new MpDialogOk();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Cleanup";
      dlgMenu.Header = "";
      dlgMenu.Content = String.Format(ServiceScope.Get<ILocalisation>().ToString("mytv", 116)/*Cleaned up {0} schedules "*/, iCleaned);
      dlgMenu.ShowDialog();
    }
    void OnNewClicked(object sender, RoutedEventArgs e)
    {
      this.NavigationService.Navigate(new Uri("/MyTv;component/TvNewSchedule.xaml", UriKind.Relative));
    }

    void OnPrioritiesClicked(object sender, RoutedEventArgs e)
    {
    }

    void OnConflictsClicked(object sender, RoutedEventArgs e)
    {
    }

    /// <summary>
    /// Called when sort button is clicked
    /// show sort dialog-menu
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnSortClicked(object sender, RoutedEventArgs e)
    {
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 97)/*Duration*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 2)/*Channel*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 73)/*Date*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*Title*/));
      dlgMenu.SelectedIndex = (int)_sortMode;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      _sortMode = (SortMode)dlgMenu.SelectedIndex;
      LoadSchedules();
    }
    #region IComparer<Schedule> Members

    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
    /// </returns>
    public int Compare(Schedule x, Schedule y)
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          return String.Compare(x.ReferencedChannel().Name.ToString(), y.ReferencedChannel().Name, true);
        case SortMode.Date:
          return x.StartTime.CompareTo(y.StartTime);
        case SortMode.Duration:
          TimeSpan t1 = x.EndTime - x.StartTime;
          TimeSpan t2 = y.EndTime - y.StartTime;
          return t1.CompareTo(t2);
        case SortMode.Title:
          return String.Compare(x.ProgramName, y.ProgramName, true);
      }
      return 0;
    }

    #endregion
  }
}