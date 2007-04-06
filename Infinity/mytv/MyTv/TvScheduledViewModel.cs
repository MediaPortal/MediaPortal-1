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
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  public class TvScheduledViewModel : INotifyPropertyChanged
  {
    #region variables
    /// <summary>
    /// Different ways the recordings view can be sorted
    /// </summary>
    public enum SortType
    {
      Duration,
      Channel,
      Date,
      Title
    };
    /// <summary>
    /// Different views on the recordings
    /// </summary>
    public enum ViewType
    {
      List,
      Icon
    };
    public event PropertyChangedEventHandler PropertyChanged;
    Window _window;
    Page _page;
    ScheduleCollectionView _scheduleView;
    ScheduleDatabaseModel _dataModel;
    ViewType _viewMode = ViewType.Icon;

    ICommand _sortCommand;
    ICommand _viewCommand;
    ICommand _cleanUpCommand;
    ICommand _deleteCommand;
    ICommand _playCommand;
    ICommand _newCommand;
    ICommand _fullScreenTvCommand;
    ICommand _fullScreenCommand;
    ICommand _contextMenuCommand;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvScheduledViewModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public TvScheduledViewModel(Page page)
    {
      //create a new data model
      _dataModel = new ScheduleDatabaseModel();

      //store page & window
      _page = page;
      _window = Window.GetWindow(_page);
    }
    #endregion

    #region properties
    /// <summary>
    /// Notifies subscribers that property has been changed
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public void ChangeProperty(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets the data model.
    /// </summary>
    /// <value>The data model.</value>
    public ScheduleDatabaseModel DataModel
    {
      get
      {
        return _dataModel;
      }
    }

    /// <summary>
    /// Gets the localized-label for the header
    /// </summary>
    /// <value>The localized label.</value>
    public string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 111);// "scheduled";
      }
    }
    /// <summary>
    /// Gets the localized-label for the current date/time
    /// </summary>
    /// <value>The localized label.</value>
    public string DateLabel
    {
      get
      {
        return DateTime.Now.ToString("dd-MM HH:mm");
      }
    }
    /// <summary>
    /// Gets the video brush.
    /// </summary>
    /// <value>The video brush.</value>
    public Brush VideoBrush
    {
      get
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          MediaPlayer player = TvPlayerCollection.Instance[0];
          VideoDrawing videoDrawing = new VideoDrawing();
          videoDrawing.Player = player;
          videoDrawing.Rect = new Rect(0, 0, player.NaturalVideoWidth, player.NaturalVideoHeight);
          DrawingBrush videoBrush = new DrawingBrush();
          videoBrush.Drawing = videoDrawing;
          return videoBrush;
        }
        return new SolidColorBrush(Color.FromArgb(0xff, 0, 0, 0));
      }
    }
    /// <summary>
    /// Returns the ListViewCollection containing the recordings
    /// </summary>
    /// <value>The recordings.</value>
    public CollectionView Schedules
    {
      get
      {
        if (_scheduleView == null)
        {
          _scheduleView = new ScheduleCollectionView(_dataModel);
        }
        return _scheduleView;
      }
    }
    /// <summary>
    /// Gets the current Window.
    /// </summary>
    /// <value>The window.</value>
    public Window Window
    {
      get
      {
        return _window;
      }
    }
    /// <summary>
    /// Gets the current Page.
    /// </summary>
    /// <value>The page.</value>
    public Page Page
    {
      get
      {
        return _page;
      }
    }
    /// <summary>
    /// Gets the localized-label for the Sort button
    /// </summary>
    /// <value>The localized label.</value>
    public string SortLabel
    {
      get
      {

        switch (_scheduleView.SortMode)
        {
          case SortType.Channel:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Sort:Channel";
          case SortType.Date:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 85);//"Sort:Date";
          case SortType.Duration:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 86);//"Sort:Duration";
          case SortType.Title:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 88);//"Sort:Title";
        }
        return "";
      }
    }
    /// <summary>
    /// Gets the localized-label for the View button
    /// </summary>
    /// <value>The localized label.</value>
    public string ViewLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 79);//"View";
      }
    }
    /// <summary>
    /// Gets the localized-label for the Cleanup button
    /// </summary>
    /// <value>The localized label.</value>
    public string CleanUpLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 82);//Cleanup
      }
    }
    /// <summary>
    /// Gets the localized-label for the New button
    /// </summary>
    /// <value>The localized label.</value>
    public string NewLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 123);//New
      }
    }
    /// <summary>
    /// Gets the localized-label for the Priorities button
    /// </summary>
    /// <value>The localized label.</value>
    public string PrioritiesLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 124);//Priorities
      }
    }
    /// <summary>
    /// Gets the localized-label for the Conflicts button
    /// </summary>
    /// <value>The localized label.</value>
    public string ConflictsLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 125);//Conflicts
      }
    }
    /// <summary>
    /// Gets or sets the view mode.
    /// </summary>
    /// <value>The view mode.</value>
    public ViewType ViewMode
    {
      get
      {
        return _viewMode;
      }
      set
      {
        if (_viewMode != value)
        {
          _viewMode = value;
          ChangeProperty("ItemTemplate");
        }
      }
    }
    /// <summary>
    /// Returns the datatemplate for the listbox items
    /// </summary>
    /// <value>The datatemplate.</value>
    public DataTemplate ItemTemplate
    {
      get
      {
        switch (_viewMode)
        {
          case ViewType.List:
            return (DataTemplate)_page.Resources["scheduleItemListTemplate"];
          default:
            return (DataTemplate)_page.Resources["scheduleItemIconTemplate"];
        }
      }
    }

    /// <summary>
    /// Returns whether video is present or not.
    /// </summary>
    /// <value>Visibility.Visible when video is present otherwise Visibility.Collapsed</value>
    public Visibility IsVideoPresent
    {
      get
      {
        return (TvPlayerCollection.Instance.Count != 0) ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    #endregion

    #region commands
    /// <summary>
    /// Returns a ICommand for sorting
    /// </summary>
    /// <value>The command.</value>
    public ICommand Sort
    {
      get
      {
        if (_sortCommand == null)
        {
          _sortCommand = new SortCommand(this);
        }
        return _sortCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for changing the view mode.
    /// </summary>
    /// <value>The command.</value>
    public ICommand View
    {
      get
      {
        if (_viewCommand == null)
        {
          _viewCommand = new ViewCommand(this);
        }
        return _viewCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for cleaning up watched recordings
    /// </summary>
    /// <value>The command.</value>
    public ICommand CleanUp
    {
      get
      {
        if (_cleanUpCommand == null)
        {
          _cleanUpCommand = new CleanUpCommand(this);
        }
        return _cleanUpCommand;
      }
    }

    /// <summary>
    /// Returns a ICommand for cleaning up watched recordings
    /// </summary>
    /// <value>The command.</value>
    public ICommand FullScreenTv
    {
      get
      {
        if (_fullScreenTvCommand == null)
        {
          _fullScreenTvCommand = new FullScreenTvCommand(this);
        }
        return _fullScreenTvCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for toggeling between fullscreen mode and windowed mode
    /// </summary>
    /// <value>The command.</value>
    public ICommand FullScreen
    {
      get
      {
        if (_fullScreenCommand == null)
        {
          _fullScreenCommand = new FullScreenCommand(this);
        }
        return _fullScreenCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for deleting a schedule
    /// </summary>
    /// <value>The command.</value>
    public ICommand Delete
    {
      get
      {
        if (_deleteCommand == null)
        {
          _deleteCommand = new DeleteCommand(this);
        }
        return _deleteCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for creating a schedule
    /// </summary>
    /// <value>The command.</value>
    public ICommand New
    {
      get
      {
        if (_newCommand == null)
        {
          _newCommand = new NewCommand(this);
        }
        return _newCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for showing the context menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand ContextMenu
    {
      get
      {
        if (_contextMenuCommand == null)
        {
          _contextMenuCommand = new ContextMenuCommand(this);
        }
        return _contextMenuCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for showing the context menu
    /// </summary>
    /// <value>The command.</value>
    public ICommand Play
    {
      get
      {
        if (_playCommand == null)
        {
          _playCommand = new PlayCommand(this);
        }
        return _playCommand;
      }
    }
    #endregion

    #region Commands subclasses
    #region base command class
    public abstract class RecordedCommand : ICommand
    {
      protected TvScheduledViewModel _viewModel;
      public event EventHandler CanExecuteChanged;

      public RecordedCommand(TvScheduledViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      public abstract void Execute(object parameter);

      public virtual bool CanExecute(object parameter)
      {
        return true;
      }

      protected void OnCanExecuteChanged()
      {
        if (this.CanExecuteChanged != null)
        {
          this.CanExecuteChanged(this, EventArgs.Empty);
        }
      }
    }
    #endregion

    #region sort command class
    /// <summary>
    /// SortCommand changes the way the view gets sorted
    /// </summary>
    public class SortCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SortCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SortCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //show dialog menu with all sorting options
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 97)/*Duration*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 2)/*Channel*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 73)/*Date*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*Title*/));
        ScheduleCollectionView view = (ScheduleCollectionView)_viewModel.Schedules;
        dlgMenu.SelectedIndex = (int)view.SortMode;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected

        //tell the view to sort
        view.SortMode = (TvScheduledViewModel.SortType)dlgMenu.SelectedIndex;

        //and tell the model that the sort property is changed
        _viewModel.ChangeProperty("SortLabel");
        _viewModel.ChangeProperty("Schedules");
      }
    }
    #endregion

    #region view command class
    /// <summary>
    /// ViewCommand changes the way each listbox item gets displayed
    /// </summary>
    public class ViewCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="ViewCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public ViewCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //change the viewmode
        switch (_viewModel.ViewMode)
        {
          case ViewType.Icon:
            _viewModel.ViewMode = ViewType.List;
            break;
          case ViewType.List:
            _viewModel.ViewMode = ViewType.Icon;
            break;
        }
      }
    }
    #endregion

    #region cleanup command class
    /// <summary>
    /// Cleanup command will delete recordings which have been watched
    /// </summary>
    public class CleanUpCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public CleanUpCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        int iCleaned = 0;
        IList itemlist = Schedule.ListAll();
        foreach (Schedule rec in itemlist)
        {
          if (rec.IsDone() || rec.Canceled != Schedule.MinSchedule)
          {
            iCleaned++;
            _viewModel.DataModel.Delete(rec.IdSchedule);
          }
        }
        MpDialogOk dlgMenu = new MpDialogOk();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Cleanup";
        dlgMenu.Header = "";
        dlgMenu.Content = String.Format(ServiceScope.Get<ILocalisation>().ToString("mytv", 116)/*Cleaned up {0} schedules "*/, iCleaned);
        dlgMenu.ShowDialog();
        _viewModel.ChangeProperty("Schedules");
      }
    }
    #endregion

    #region FullscreenTv command class
    /// <summary>
    /// Fullscreen command will navigate to fullscreen window
    /// </summary>
    public class FullScreenTvCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public FullScreenTvCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (TvPlayerCollection.Instance.Count != 0)
        {
          _viewModel.Page.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
        }
      }
    }
    #endregion


    #region Delete command class
    /// <summary>
    /// Delete command will delete a recoring
    /// </summary>
    public class DeleteCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public DeleteCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        _viewModel.ChangeProperty("Schedules");
      }
    }
    #endregion

    #region New command class
    /// <summary>
    /// Delete command will create a recoring
    /// </summary>
    public class NewCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public NewCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        _viewModel.Page.NavigationService.Navigate(new Uri("/MyTv;component/TvNewSchedule.xaml", UriKind.Relative));
      }
    }
    #endregion

    #region FullScreenCommand  class
    /// <summary>
    /// FullScreenCommand will toggle application between normal and fullscreen mode
    /// </summary> 
    public class FullScreenCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="FullScreenCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public FullScreenCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        Window window = _viewModel.Window;
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
      }
    }
    #endregion


    #region ContextMenu command class
    /// <summary>
    /// ContextMenuCommand will show the context menu
    /// </summary> 
    public class ContextMenuCommand : RecordedCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public ContextMenuCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ScheduleModel item = parameter as ScheduleModel;
        if (item == null) return;
        Schedule rec = item.Schedule;
        if (rec == null) return;

        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
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
            _viewModel.Page.NavigationService.Navigate(infopage);
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
                dlgYesNo.Owner = _viewModel.Window;
                dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
                dlgYesNo.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording? This schedule is recording. If you delete the schedule then the recording is stopped.";
                dlgYesNo.ShowDialog();
                if (dlgYesNo.DialogResult == DialogResult.No) return;
                server.StopRecordingSchedule(rec.IdSchedule);
                _viewModel.DataModel.CancelSchedule(rec.IdSchedule, rec.StartTime);
              }
              else
              {
                server.StopRecordingSchedule(rec.IdSchedule);
                _viewModel.DataModel.CancelSchedule(rec.IdSchedule, rec.StartTime);
              }
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
                dlgYesNo.Owner = _viewModel.Window;
                dlgYesNo.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
                dlgYesNo.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 57);// "Delete this recording? This schedule is recording. If you delete the schedule then the recording is stopped.";
                dlgYesNo.ShowDialog();
                if (dlgYesNo.DialogResult == DialogResult.No) return;
                server.StopRecordingSchedule(rec.IdSchedule);
                _viewModel.DataModel.Delete(rec.IdSchedule);
              }
              else
              {
                _viewModel.DataModel.Delete(rec.IdSchedule);

              }
            }
            break;

          case 979: // Play recording from beginning
            {
              ICommand cmd = _viewModel.Play;
              cmd.Execute(new PlayCommand.PlayParameter(fileName, false));
            }
            return;

          case 980: // Play recording from live point
            {
              ICommand cmd = _viewModel.Play;
              cmd.Execute(new PlayCommand.PlayParameter(fileName, true));
            }
            break;
        }
      }
    }
    #endregion

    #region Play command class
    /// <summary>
    /// Play command will start playing a recording
    /// </summary>
    public class PlayCommand : RecordedCommand
    {
      public class PlayParameter
      {
        public string FileName;
        public bool StartFromLivePoint;
        public PlayParameter(string filename, bool startFromLivePoint)
        {
          FileName = filename;
          StartFromLivePoint = startFromLivePoint;
        }
      }
      private delegate void MediaPlayerErrorDelegate();
      private delegate void MediaPlayerOpenDelegate();
      PlayParameter _playParameter;
      /// <summary>
      /// Initializes a new instance of the <see cref="PlayCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public PlayCommand(TvScheduledViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          TvPlayerCollection.Instance.DisposeAll();
          _viewModel.ChangeProperty("VideoBrush");
          _viewModel.ChangeProperty("FullScreen");
          _viewModel.ChangeProperty("IsVideoPresent");
        }
        _playParameter = parameter as PlayParameter;


        TvMediaPlayer player = TvPlayerCollection.Instance.Get(null, _playParameter.FileName);
        player.MediaFailed += new EventHandler<ExceptionEventArgs>(_mediaPlayer_MediaFailed);
        player.MediaOpened += new EventHandler(player_MediaOpened);
        player.Play();
      }

      void player_MediaOpened(object sender, EventArgs e)
      {
        _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerOpenDelegate(OnMediaOpened));
      }
      void OnMediaOpened()
      {
        _viewModel.ChangeProperty("VideoBrush");
        _viewModel.ChangeProperty("FullScreen");
        _viewModel.ChangeProperty("IsVideoPresent");
        if (_playParameter.StartFromLivePoint)
        {
          TvMediaPlayer player = TvPlayerCollection.Instance[0];
          if (player.NaturalDuration.HasTimeSpan)
          {
            TimeSpan duration = player.Duration;
            TimeSpan newPos = duration + new TimeSpan(0, 0, 0, 0, -500);
            player.Position = newPos;
          }
        }
      }
      void _mediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
      {
        _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
      }
      void OnMediaPlayerError()
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          if (TvPlayerCollection.Instance[0].HasError)
          {
            MpDialogOk dlgError = new MpDialogOk();
            dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlgError.Owner = _viewModel.Window;
            dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
            dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
            dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 38)/*Unable to open the file*/+ " " + TvPlayerCollection.Instance[0].ErrorMessage;
            dlgError.ShowDialog();
          }
        }
        TvPlayerCollection.Instance.DisposeAll();
      }
    }
    #endregion
    #endregion

    #region ScheduleDatabaseModel class
    /// <summary>
    /// Class representing our database model.
    /// It simply retrieves all recordings from the tv database and 
    /// creates a list of ScheduleModel
    /// </summary>
    public class ScheduleDatabaseModel : INotifyPropertyChanged
    {
      #region variables
      public event PropertyChangedEventHandler PropertyChanged;
      List<ScheduleModel> _listSchedules = new List<ScheduleModel>();
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleDatabaseModel"/> class.
      /// </summary>
      public ScheduleDatabaseModel()
      {
        Reload();
      }
      public void CancelSchedule(int idSchedule, DateTime cancelTime)
      {
        CanceledSchedule schedule = new CanceledSchedule(idSchedule, cancelTime);
        schedule.Persist();
        TvServer server = new TvServer();
        server.OnNewSchedule();
        Reload();
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("Schedules"));
        }
      }
      /// <summary>
      /// Deletes this instance.
      /// </summary>
      public void Delete(int idSchedule)
      {
        TvServer server = new TvServer();
        for (int i = 0; i < _listSchedules.Count; ++i)
        {
          if (_listSchedules[i].Schedule.IdSchedule == idSchedule)
          {
            _listSchedules[i].Schedule.Delete();
            server.OnNewSchedule();
            _listSchedules.RemoveAt(i);
            if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs("Schedules"));
            }
            break;
          }
        }
      }
      /// <summary>
      /// Refreshes the list with the database.
      /// </summary>
      public void Reload()
      {
        _listSchedules.Clear();
        IList schedules = Schedule.ListAll();

        foreach (Schedule schedule in schedules)
        {
          ScheduleModel item = new ScheduleModel(schedule);
          _listSchedules.Add(item);
        }
      }

      /// <summary>
      /// Gets the recordings.
      /// </summary>
      /// <value>IList containing 0 or more ScheduleModel instances.</value>
      public IList Schedules
      {
        get
        {
          return _listSchedules;
        }
      }
    }
    #endregion

    #region ScheduleCollectionView class
    /// <summary>
    /// This class represents the schedule view
    /// </summary>
    class ScheduleCollectionView : ListCollectionView
    {
      #region variables
      SortType _sortMode = SortType.Date;
      private ScheduleDatabaseModel _model;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleCollectionView"/> class.
      /// </summary>
      /// <param name="model">The database model.</param>
      public ScheduleCollectionView(ScheduleDatabaseModel model)
        : base(model.Schedules)
      {
        _model = model;
        _model.PropertyChanged += new PropertyChangedEventHandler(onDatabaseChanged);
      }

      void onDatabaseChanged(object sender, PropertyChangedEventArgs e)
      {
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      /// <summary>
      /// Gets or sets the sort mode.
      /// </summary>
      /// <value>The sort mode.</value>
      public SortType SortMode
      {
        get
        {
          return _sortMode;
        }
        set
        {
          if (_sortMode != value)
          {
            _sortMode = value;
            this.CustomSort = new ScheduleComparer(_sortMode);
          }
        }
      }
    }
    #endregion

    #region ScheduleComparer class
    /// <summary>
    /// Helper class to compare 2 RecordingModels
    /// </summary>
    public class ScheduleComparer : IComparer
    {
      #region variables
      SortType _sortMode;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="ScheduleComparer"/> class.
      /// </summary>
      /// <param name="sortMode">The sort mode.</param>
      public ScheduleComparer(SortType sortMode)
      {
        _sortMode = sortMode;
      }
      /// <summary>
      /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      /// <returns>
      /// Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.
      /// </returns>
      /// <exception cref="T:System.ArgumentException">Neither x nor y implements the <see cref="T:System.IComparable"></see> interface.-or- x and y are of different types and neither one can handle comparisons with the other. </exception>
      public int Compare(object x, object y)
      {
        ScheduleModel model1 = (ScheduleModel)x;
        ScheduleModel model2 = (ScheduleModel)y;
        switch (_sortMode)
        {
          case SortType.Channel:
            return String.Compare(model1.Channel.ToString(), model2.Channel, true);
          case SortType.Date:
            return model1.StartTime.CompareTo(model2.StartTime);
          case SortType.Duration:
            TimeSpan t1 = model1.EndTime - model1.StartTime;
            TimeSpan t2 = model2.EndTime - model2.StartTime;
            return t1.CompareTo(t2);
          case SortType.Title:
            return String.Compare(model1.Title, model2.Title, true);
        }
        return 0;
      }
    }
    #endregion
  }
}
