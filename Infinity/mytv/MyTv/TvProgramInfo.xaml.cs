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

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvProgramInfo.xaml
  /// </summary>

  public partial class TvProgramInfo : System.Windows.Controls.Page
  {
    Program _program;
    public TvProgramInfo(Program program)
    {
      _program = program;
      InitializeComponent();
      ShowUpcomingEpisodes();
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
      }
    }
    void ShowUpcomingEpisodes()
    {
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      gridList.Children.Clear();
      Grid grid = new Grid();
      //set program description
      string strTime = String.Format("{0} {1} - {2}", _program.StartTime.ToString("dd-MM"), _program.StartTime.ToString("HH:mm"), _program.EndTime.ToString("HH:mm"));

      labelGenre.Text = _program.Genre;
      labelStartEnd.Text = strTime;
      labelDescription.Text = _program.Description;
      labelTitle.Text = _program.Title;

      //check if we are recording this program
      IList schedules = Schedule.ListAll();
      bool isRecording = false;
      bool isSeries = false;
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(_program, true))
        {
          if (!schedule.IsSerieIsCanceled(_program.StartTime))
          {
            if ((ScheduleRecordingType)schedule.ScheduleType != ScheduleRecordingType.Once)
              isSeries = true;
            isRecording = true;
            break;
          }
        }
      }

      if (isRecording)
      {
        buttonRecord.Content = "Dont record";
        buttonAdvancedRecord.IsEnabled = false;
        buttonKeepUntil.IsEnabled = true;
        buttonQuality.IsEnabled = true;
        buttonEpisodes.IsEnabled = isSeries;
        buttonPreRecord.IsEnabled = true;
        buttonPostRecord.IsEnabled = true;
      }
      else
      {
        buttonRecord.Content = "Record";
        buttonAdvancedRecord.IsEnabled = true;
        buttonKeepUntil.IsEnabled = false;
        buttonQuality.IsEnabled = true;
        buttonEpisodes.IsEnabled = false;
        buttonPreRecord.IsEnabled = false;
        buttonPostRecord.IsEnabled = false;
      }
      buttonAlertMe.IsChecked = _program.Notify;

      //find upcoming episodes

      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime dtDay = DateTime.Now;
      IList episodes = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(14), _program.Title, null);
      int row = 0;
      foreach (Program episode in episodes)
      {
        grid.RowDefinitions.Add(new RowDefinition());
        Button button = new Button();
        button.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
        Grid gridSub = new Grid();
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.RowDefinitions.Add(new RowDefinition());
        gridSub.RowDefinitions.Add(new RowDefinition());

        string logo = System.IO.Path.ChangeExtension(episode.ReferencedChannel().Name, ".png");
        if (System.IO.File.Exists(logo))
        {
          Image image = new Image();
          PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(logo, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

          image.Source = decoder.Frames[0];
          Grid.SetColumn(image, 0);
          Grid.SetRow(image, 0);
          Grid.SetRowSpan(image, 2);
          gridSub.Children.Add(image);
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
        Label label = new Label();
        label.Content = episode.Title;
        label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
        Grid.SetColumn(label, 1);
        Grid.SetRow(label, 0);
        Grid.SetColumnSpan(label, 8);
        gridSub.Children.Add(label);
        //item.MusicTag = episode;
        //item.ThumbnailImage = logo;
        //item.IconImageBig = logo;
        //item.IconImage = logo;
        strTime = String.Format("{0} {1} - {2}", episode.StartTime.ToString("dd-MM"), episode.StartTime.ToString("HH:mm"), episode.EndTime.ToString("HH:mm"));

        label = new Label();
        label.Content = strTime;
        label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
        Grid.SetColumn(label, 1);
        Grid.SetColumnSpan(label, 6);
        Grid.SetRow(label, 1);
        gridSub.Children.Add(label);

        label = new Label();
        label.Content = episode.ReferencedChannel().Name;
        label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
        label.HorizontalAlignment = HorizontalAlignment.Right;
        //label.Margin = new Thickness(0, 0, 60, 0);
        Grid.SetColumn(label, 7);
        Grid.SetColumnSpan(label, 2);
        Grid.SetRow(label, 1);
        gridSub.Children.Add(label);

        gridSub.Loaded += new RoutedEventHandler(gridSub_Loaded);
        button.Tag = episode;
        button.GotFocus += new RoutedEventHandler(button_GotFocus);
        button.MouseEnter += new MouseEventHandler(OnMouseEnter);
        button.Content = gridSub;
        button.Click += new RoutedEventHandler(OnUpcomingEpisodeClicked);
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, row);
        grid.Children.Add(button);
        row++;
      }
      gridList.Children.Add(grid);
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
      Keyboard.Focus(buttonRecord);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");


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
      labelTitle.Text = _program.Title;
      labelDescription.Text = _program.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", _program.StartTime.ToString("HH:mm"), _program.EndTime.ToString("HH:mm"));
      labelGenre.Text = _program.Genre;


    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
        return;
      }
    }


    void OnUpcomingEpisodeClicked(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      Program p = b.Tag as Program;
      if (p == null) return;
      OnRecordProgram(p);
    }
    void OnRecordClicked(object sender, EventArgs e)
    {
      OnRecordProgram(_program);
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
          dlgMenu.Header = "Menu";
          dlgMenu.SubTitle = "";
          dlgMenu.Items.Add(new DialogMenuItem("Delete this recording"));
          dlgMenu.Items.Add(new DialogMenuItem("Delete series recording"));
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
      dlgMenu.Header = "Delete";
      dlgMenu.Content = "Delete this recording ? This schedule is recording. If you delete the schedule then the recording will be stopped.";
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
      if (_program == null)
        return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "Record";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("None"));
      dlgMenu.Items.Add(new DialogMenuItem("Record once"));
      dlgMenu.Items.Add(new DialogMenuItem("Record everytime on this channel"));
      dlgMenu.Items.Add(new DialogMenuItem("Record everytime on every channel"));
      dlgMenu.Items.Add(new DialogMenuItem("Record every week at this time"));
      dlgMenu.Items.Add(new DialogMenuItem("Record every day at this time")); ;
      dlgMenu.Items.Add(new DialogMenuItem("Record Mon-fri"));
      dlgMenu.Items.Add(new DialogMenuItem("Record Sat-Sun"));
      dlgMenu.ShowDialog();

      if (dlgMenu.SelectedIndex < 1) return;

      Schedule rec = new Schedule(_program.IdChannel, _program.Title, _program.StartTime, _program.EndTime);
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
            dlgYesNo.Header = "Multipart";
            dlgYesNo.Content = String.Format("This program will be interrupted by {0} Would you like to record the second part also?");
            dlgYesNo.ShowDialog();
            if (dlgYesNo.DialogResult == DialogResult.Yes)
            {
              rec = new Schedule(_program.IdChannel, _program.Title, nextNext.StartTime, nextNext.EndTime);

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

    void OnNotify(object sender, EventArgs args)
    {
      _program.Notify = !_program.Notify;
      _program.Persist();
      ShowUpcomingEpisodes();
    }

    void OnKeepUntil(object sender, EventArgs args)
    {
      Schedule rec;
      if (false == IsRecordingProgram(_program, out  rec, false)) return;

      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "Menu";
      dlgMenu.SubTitle = "Until";
      dlgMenu.Items.Add(new DialogMenuItem("Until space needed"));
      dlgMenu.Items.Add(new DialogMenuItem("Until watched"));
      dlgMenu.Items.Add(new DialogMenuItem("Until Date"));
      dlgMenu.Items.Add(new DialogMenuItem("Always"));
      dlgMenu.SelectedIndex = (int)rec.KeepMethod;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      rec.KeepMethod = dlgMenu.SelectedIndex;
      rec.Persist();
      if (dlgMenu.SelectedIndex == 2)
      {
        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = w;
        dlgMenu.Items.Clear();
        dlgMenu.Header = "Menu";
        dlgMenu.SubTitle = "Date";
        int selected = 0;
        for (int days = 1; days <= 31; days++)
        {
          DateTime dt = DateTime.Now.AddDays(days);
          dlgMenu.Items.Add(new DialogMenuItem(dt.ToLongDateString()));
          if (dt.Date == rec.KeepDate) selected = days - 1;
        }
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected
        int daysChoosen = dlgMenu.SelectedIndex + 1;
        rec.KeepDate = DateTime.Now.AddDays(daysChoosen);
        rec.Persist();
      }
    }
    void OnPreRecordInterval(object sender, EventArgs args)
    {
      Schedule rec;
      if (false == IsRecordingProgram(_program, out  rec, false)) return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "PreRecord";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("Default"));
      for (int minute = 0; minute < 20; minute++)
      {
        dlgMenu.Items.Add(new DialogMenuItem(String.Format("{0} mins", minute)));
      }
      if (rec.PreRecordInterval < 0) dlgMenu.SelectedIndex = 0;
      else dlgMenu.SelectedIndex = rec.PreRecordInterval + 1;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;
      rec.PreRecordInterval = dlgMenu.SelectedIndex - 1;
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      ShowUpcomingEpisodes();
    }
    void OnPostRecordInterval(object sender, EventArgs args)
    {
      Schedule rec;
      if (false == IsRecordingProgram(_program, out  rec, false)) return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "PostRecord";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("Default"));
      for (int minute = 0; minute < 20; minute++)
      {
        dlgMenu.Items.Add(new DialogMenuItem(String.Format("{0} mins", minute)));
      }
      if (rec.PostRecordInterval < 0) dlgMenu.SelectedIndex = 0;
      else dlgMenu.SelectedIndex = rec.PostRecordInterval + 1;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;
      rec.PostRecordInterval = dlgMenu.SelectedIndex - 1;
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      ShowUpcomingEpisodes();
    }

    void OnSetEpisodes(object sender, EventArgs args)
    {
      Schedule schedule;
      if (false == IsRecordingProgram(_program, out  schedule, false)) return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "Episodes";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("All"));
      for (int i = 1; i < 40; ++i)
        dlgMenu.Items.Add(new DialogMenuItem(i.ToString() + " episodes"));
      if (schedule.MaxAirings == Int32.MaxValue)
        dlgMenu.SelectedIndex = 0;
      else
        dlgMenu.SelectedIndex = schedule.MaxAirings;

      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex == -1) return;

      if (dlgMenu.SelectedIndex == 0) schedule.MaxAirings = Int32.MaxValue;
      else schedule.MaxAirings = dlgMenu.SelectedIndex;
      schedule.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      ShowUpcomingEpisodes();
    }
  }
}