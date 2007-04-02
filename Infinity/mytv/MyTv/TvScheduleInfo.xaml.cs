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
using TvControl;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvScheduleInfo.xaml
  /// </summary>

  public partial class TvScheduleInfo : System.Windows.Controls.Page
  {
    Schedule _schedule ;
    bool _isRecording=true;
    public TvScheduleInfo(Schedule schedule)
    {
      _schedule = schedule;
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
    void ShowUpcomingEpisodes()
    {
      labelHeader.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 110);//schedule info
      buttonRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 13);//Record
      buttonAdvancedRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 42);//Advanced Record
      buttonKeepUntil.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 47);//Keep Until
      buttonQuality.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 49);//Quality setting
      buttonEpisodes.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 50);//Episodes management
      buttonPreRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 51);//Pre-record
      buttonPostRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 52);//Post-record
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      
      
      //set program description
      string strTime = String.Format("{0} {1} - {2}", _schedule.StartTime.ToString("dd-MM"), _schedule.StartTime.ToString("HH:mm"), _schedule.EndTime.ToString("HH:mm"));

//      labelGenre.Text = _schedule.Genre;
      labelStartEnd.Text = strTime;
      //      labelDescription.Text = _schedule.Description;
      labelTitle.Text = _schedule.ProgramName;

      //check if we are recording this program
      bool isSeries = ((ScheduleRecordingType)_schedule.ScheduleType != ScheduleRecordingType.Once);

      if (_isRecording)
      {
        buttonRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 53);//Dont Record
        buttonAdvancedRecord.IsEnabled = false;
        buttonKeepUntil.IsEnabled = true;
        buttonQuality.IsEnabled = true;
        buttonEpisodes.IsEnabled = isSeries;
        buttonPreRecord.IsEnabled = true;
        buttonPostRecord.IsEnabled = true;
      }
      else
      {
        buttonRecord.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 13);//Record
        buttonAdvancedRecord.IsEnabled = true;
        buttonKeepUntil.IsEnabled = false;
        buttonQuality.IsEnabled = true;
        buttonEpisodes.IsEnabled = false;
        buttonPreRecord.IsEnabled = false;
        buttonPostRecord.IsEnabled = false;
      }
      
      //find upcoming episodes

      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime dtDay = DateTime.Now;
      IList episodes = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(14), _schedule.ProgramName, null);
      int row = 0;
      DialogMenuItemCollection collection =  new DialogMenuItemCollection();
      foreach (Program episode in episodes)
      {
        string logo = System.IO.Path.ChangeExtension(episode.ReferencedChannel().Name, ".png");
        if (!System.IO.File.Exists(logo))
        {
          logo="";
        }
        Schedule recordingSchedule;
        string recIcon = "";
        if (IsRecordingProgram(episode, out recordingSchedule, false))
        {
          if (false == recordingSchedule.IsSerieIsCanceled(episode.StartTime))
          {
            if (recordingSchedule.ReferringConflicts().Count > 0)
            {
              recIcon = Thumbs.TvConflictRecordingIcon;
            }
            else
            {
              recIcon = Thumbs.TvRecordingIcon;
            }
          }
          //item.TVTag = recordingSchedule;
        }
        /*
        if (recIcon != "")
        {
          if (System.IO.File.Exists(recIcon))
          {
            Image image = new Image();
            PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(recIcon, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            image.Source = decoder.Frames[0];
            image.Height = 32;
            image.Width = 32;
            image.VerticalAlignment = VerticalAlignment.Center;
            image.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(image, 1);
            Grid.SetRow(image, 0);
            Grid.SetColumnSpan(image, 8);
            gridSub.Children.Add(image);
          }
        }
        */
        DialogMenuItem item = new DialogMenuItem(logo,episode.Title,strTime,episode.ReferencedChannel().Name);
        item.Tag=episode;
        collection.Add(item);
      }
      gridList.ItemsSource = collection;
    }

    void button_GotFocus(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      Program recording = b.Tag as Program;
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


    bool IsRecordingProgram(Program program, out Schedule recordingSchedule, bool filterCanceledRecordings)
    {
      recordingSchedule = null;
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(program, filterCanceledRecordings))
        {
          recordingSchedule = schedule;
          return true;
        }
      }
      return false;
    }


    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Keyboard.AddGotKeyboardFocusHandler(gridList, new KeyboardFocusChangedEventHandler(onKeyboardFocus));

      this.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click));
      Keyboard.Focus(buttonRecord);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      ShowUpcomingEpisodes();


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
      labelTitle.Text = _schedule.ProgramName;
      //labelDescription.Text = _schedule.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", _schedule.StartTime.ToString("HH:mm"), _schedule.EndTime.ToString("HH:mm"));
      //labelGenre.Text = _schedule.Genre;


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
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
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
    void Button_Click(object sender, RoutedEventArgs e)
    {
      if (e.Source == gridList)
      {
        OnUpcomingEpisodeClicked();
      }
    }


    void OnUpcomingEpisodeClicked()
    {
      DialogMenuItem item = gridList.SelectedItem as DialogMenuItem;
      Program p = item.Tag as Program;
      if (p == null) return;
      OnRecordProgram(p);
    }
    
    void OnRecordClicked(object sender, EventArgs e)
    {
      //OnRecordProgram(_schedule);
    }

    void OnRecordProgram(Program program)
    {
      Schedule recordingSchedule;
      if (IsRecordingProgram(program, out  recordingSchedule, false))
      {
        //already recording this program
        if (recordingSchedule.ScheduleType != (int)ScheduleRecordingType.Once)
        {
          MpMenu dlgMenu = new MpMenu();
          Window w = Window.GetWindow(this);
          dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgMenu.Owner = w;
          dlgMenu.Items.Clear();
          dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68); //"Menu";
          dlgMenu.SubTitle = "";
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 54)/*"Delete this recording*/));
          dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 55)/*"Delete series recording*/));
          dlgMenu.ShowDialog();
          if (dlgMenu.SelectedIndex == -1)
            return;
          switch (dlgMenu.SelectedIndex)
          {
            case 0: //Delete this recording only
              {
                if (CheckIfRecording(recordingSchedule))
                {
                  //delete specific series
                  CanceledSchedule canceledSchedule = new CanceledSchedule(recordingSchedule.IdSchedule, program.StartTime);
                  canceledSchedule.Persist();
                  TvServer server = new TvServer();
                  server.StopRecordingSchedule(recordingSchedule.IdSchedule);
                  server.OnNewSchedule();
                }
              }
              break;
            case 1: //Delete entire recording
              {
                if (CheckIfRecording(recordingSchedule))
                {
                  //cancel recording
                  TvServer server = new TvServer();
                  server.StopRecordingSchedule(recordingSchedule.IdSchedule);
                  recordingSchedule.Delete();
                  server.OnNewSchedule();
                }
              }
              break;
          }
        }
        else
        {
          if (CheckIfRecording(recordingSchedule))
          {
            TvServer server = new TvServer();
            server.StopRecordingSchedule(recordingSchedule.IdSchedule);
            recordingSchedule.Delete();
            server.OnNewSchedule();
          }
        }
      }
      else
      {
        //not recording this program
        // check if this program is conflicting with any other already scheduled recording
        TvBusinessLayer layer = new TvBusinessLayer();
        Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        if (SkipForConflictingRecording(rec)) return;

        rec.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
      }
      ShowUpcomingEpisodes();
    }
    bool CheckIfRecording(Schedule rec)
    {

      VirtualCard card;
      TvServer server = new TvServer();
      if (!server.IsRecordingSchedule(rec.IdSchedule, out card)) return true;
      MpDialogYesNo dlgMenu = new MpDialogYesNo();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 56);// "Delete";
      dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording ? This schedule is recording. If you delete the schedule then the recording will be stopped.";
      dlgMenu.ShowDialog();
      if (dlgMenu.DialogResult == DialogResult.Yes)
      {
        return true;
      }
      return false;
    }
    private bool SkipForConflictingRecording(Schedule rec)
    {
      /*
            Log.Info("SkipForConflictingRecording: Schedule = " + rec.ToString());

            TvBusinessLayer layer = new TvBusinessLayer();
            IList conflicts = rec.ConflictingSchedules();
            if (conflicts.Count > 0)
            {
              GUIDialogTVConflict dlg = (GUIDialogTVConflict)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TVCONFLICT);
              if (dlg != null)
              {
                dlg.Reset();
                dlg.SetHeading(GUILocalizeStrings.Get(879));   // "recording conflict"
                foreach (Schedule conflict in conflicts)
                {
                  Log.Info("SkipForConflictingRecording: Conflicts = " + conflict.ToString());

                  GUIListItem item = new GUIListItem(conflict.ProgramName);
                  item.Label2 = GetRecordingDateTime(conflict);
                  item.Label3 = conflict.IdChannel.ToString();
                  item.TVTag = conflict;
                  dlg.AddConflictRecording(item);
                }
                dlg.DoModal(GetID);
                switch (dlg.SelectedLabel)
                {
                  case 0: return true;   // Skip new Recording
                  case 1:                // Don't record the already scheduled one(s)
                    {
                      foreach (Schedule conflict in conflicts)
                      {
                        Program prog = new Program(conflict.IdChannel, conflict.StartTime, conflict.EndTime, conflict.ProgramName, "-", "-", false);
                        OnRecordProgram(prog);
                      }
                      break;
                    }
                  case 2: return false;   // No Skipping new Recording
                  default: return true;   // Skipping new Recording
                }
              }
            }
      */
      return false;
    }
    void OnAdvancedRecordClicked(object sender, EventArgs e)
    {
      if (_schedule == null)
        return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 13);//Record
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 58)/* "None"*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 59)/*"Record once"*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 60)/*"Record everytime on this channel"*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 61)/*"Record everytime on every channel"*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 62)/*"Record every week at this time"*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 63)/*"Record every day at this time"*/)); ;
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 64)/*"Record Mon-fri"*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 65)/*"Record Sat-Sun"*/));
      dlgMenu.ShowDialog();

      if (dlgMenu.SelectedIndex < 1) return;

      Schedule rec = new Schedule(_schedule.IdChannel, _schedule.ProgramName, _schedule.StartTime, _schedule.EndTime);
      switch (dlgMenu.SelectedIndex)
      {
        case 1://once
          rec.ScheduleType = (int)ScheduleRecordingType.Once;
          break;
        case 2://everytime, this channel
          rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
          break;
        case 3://everytime, all channels
          rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
          break;
        case 4://weekly
          rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
          break;
        case 5://daily
          rec.ScheduleType = (int)ScheduleRecordingType.Daily;
          break;
        case 6://Mo-Fi
          rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
          break;
        case 7://Record Sat-Sun
          rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
          break;
      }
      if (SkipForConflictingRecording(rec)) return;

      TvBusinessLayer layer = new TvBusinessLayer();
      rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();

      //check if this program is interrupted (for example by a news bulletin)
      //ifso ask the user if he wants to record the 2nd part also
      IList programs = new ArrayList();
      DateTime dtStart = rec.EndTime.AddMinutes(1);
      DateTime dtEnd = dtStart.AddHours(3);
      programs = layer.GetPrograms(rec.ReferencedChannel(), dtStart, dtEnd);
      if (programs.Count >= 2)
      {
        Program next = programs[0] as Program;
        Program nextNext = programs[1] as Program;
        if (nextNext.Title == rec.ProgramName)
        {
          TimeSpan ts = next.EndTime - next.StartTime;
          if (ts.TotalMinutes <= 40)
          {
            MpDialogYesNo dlgYesNo = new MpDialogYesNo();
            Window ww = Window.GetWindow(this);
            dlgYesNo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlgYesNo.Owner = ww;
            dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 66);//""Multipart";
            dlgYesNo.Content = String.Format(ServiceScope.Get<ILocalisation>().ToString("mytv", 667)/*This program will be interrupted by {0} Would you like to record the second part also?")*/, next.Title);
            dlgYesNo.ShowDialog();
            if (dlgYesNo.DialogResult == DialogResult.Yes)
            {
              rec = new Schedule(_schedule.IdChannel, _schedule.ProgramName, nextNext.StartTime, nextNext.EndTime);

              rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
              rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
              rec.Persist();
              server.OnNewSchedule();
            }
          }
        }
      }
      ShowUpcomingEpisodes();
    }


    void OnKeepUntil(object sender, EventArgs args)
    {

      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
      dlgMenu.SubTitle = ServiceScope.Get<ILocalisation>().ToString("mytv", 47);// Keep Until";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 69)/*Until space needed*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 70)/*Until watched*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 71)/*Until Date*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 72)/*Always*/));
      dlgMenu.SelectedIndex = (int)_schedule.KeepMethod;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      _schedule.KeepMethod = dlgMenu.SelectedIndex;
      _schedule.Persist();
      if (dlgMenu.SelectedIndex == 2)
      {
        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = w;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68); //"Menu";
        dlgMenu.SubTitle = ServiceScope.Get<ILocalisation>().ToString("mytv", 73); //"Date";
        int selected = 0;
        for (int days = 1; days <= 31; days++)
        {
          DateTime dt = DateTime.Now.AddDays(days);
          dlgMenu.Items.Add(new DialogMenuItem(dt.ToLongDateString()));
          if (dt.Date == _schedule.KeepDate) selected = days - 1;
        }
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected
        int daysChoosen = dlgMenu.SelectedIndex + 1;
        _schedule.KeepDate = DateTime.Now.AddDays(daysChoosen);
        _schedule.Persist();
      }
    }
    void OnPreRecordInterval(object sender, EventArgs args)
    {
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 51);//"Pre-record";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 74)/*Default*/));
      for (int minute = 0; minute < 20; minute++)
      {
        dlgMenu.Items.Add(new DialogMenuItem(String.Format("{0} {1}", minute, ServiceScope.Get<ILocalisation>().ToString("mytv", 75)/*minutes*/)));
      }

      if (_schedule.PreRecordInterval < 0) dlgMenu.SelectedIndex = 0;
      else dlgMenu.SelectedIndex = _schedule.PreRecordInterval + 1;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;
      _schedule.PreRecordInterval = dlgMenu.SelectedIndex - 1;
      _schedule.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      ShowUpcomingEpisodes();
    }
    void OnPostRecordInterval(object sender, EventArgs args)
    {
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 52);//Post-record
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 74)/*Default*/));
      for (int minute = 0; minute < 20; minute++)
      {
        dlgMenu.Items.Add(new DialogMenuItem(String.Format("{0} {1}", minute, ServiceScope.Get<ILocalisation>().ToString("mytv", 75)/*minutes*/)));
      }
      if (_schedule.PostRecordInterval < 0) dlgMenu.SelectedIndex = 0;
      else dlgMenu.SelectedIndex = _schedule.PostRecordInterval + 1;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;
      _schedule.PostRecordInterval = dlgMenu.SelectedIndex - 1;
      _schedule.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      ShowUpcomingEpisodes();
    }

    void OnSetEpisodes(object sender, EventArgs args)
    {
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 50);//Episodes management
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 76)/*All*/));
      for (int i = 1; i < 40; ++i)
        dlgMenu.Items.Add(new DialogMenuItem(i.ToString() + ServiceScope.Get<ILocalisation>().ToString("mytv", 77)/*episodes*/));
      if (_schedule.MaxAirings == Int32.MaxValue)
        dlgMenu.SelectedIndex = 0;
      else
        dlgMenu.SelectedIndex = _schedule.MaxAirings;

      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex == -1) return;

      if (dlgMenu.SelectedIndex == 0) _schedule.MaxAirings = Int32.MaxValue;
      else _schedule.MaxAirings = dlgMenu.SelectedIndex;
      _schedule.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      ShowUpcomingEpisodes();
    }
  }
}
