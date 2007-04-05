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
  /// <summary>
  /// View Model for the recorded tv GUI
  /// </summary>
  class TvRecordedViewModel : INotifyPropertyChanged
  {
    #region variables and enums
    /// <summary>
    /// Different ways the recordings view can be sorted
    /// </summary>
    public enum SortType
    {
      Duration,
      Channel,
      Date,
      Title,
      Genre,
      Watched
    };

    /// <summary>
    /// Different views on the recordings
    /// </summary>
    public enum ViewType
    {
      List,
      Icon
    };
    ICommand _sortCommand;
    ICommand _viewCommand;
    ICommand _cleanUpCommand;
    ICommand _fullScreenTvCommand;
    ICommand _playCommand;
    ICommand _deleteCommand;
    ICommand _contextMenuCommand;
    ICommand _fullScreenCommand;
    Window _window;
    Page _page;
    ViewType _viewMode = ViewType.Icon;
    RecordingCollectionView _recordingView;
    RecordingDatabaseModel _dataModel;
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvRecordedViewModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public TvRecordedViewModel(Page page)
    {
      //create a new data model
      _dataModel = new RecordingDatabaseModel();

      //store page & window
      _page = page;
      _window = Window.GetWindow(_page);
    }
    #endregion

    #region properties
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
    public CollectionView Recordings
    {
      get
      {
        if (_recordingView == null)
        {
          _recordingView = new RecordingCollectionView(_dataModel);
        }
        return _recordingView;
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
    /// Gets the localized-label for the Switch button
    /// </summary>
    /// <value>The localized label.</value>
    public string SwitchLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 81);//Switch
      }
    }
    /// <summary>
    /// Gets the localized-label for the Cleanup button
    /// </summary>
    /// <value>The localized label.</value>
    public string CleanupLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 82);//Cleanup
      }
    }
    /// <summary>
    /// Gets the localized-label for the Compress button
    /// </summary>
    /// <value>The localized label.</value>
    public string CompressLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 83);//Compress
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
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 78);//recorded
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
    /// Gets the localized-label for the Sort button
    /// </summary>
    /// <value>The localized label.</value>
    public string SortLabel
    {
      get
      {

        switch (_recordingView.SortMode)
        {
          case SortType.Channel:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Sort:Channel";
          case SortType.Date:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 85);//"Sort:Date";
          case SortType.Duration:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 86);//"Sort:Duration";
          case SortType.Genre:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 87);//"Sort:Genre";
          case SortType.Title:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 88);//"Sort:Title";
          case SortType.Watched:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 89);//"Sort:Watched";
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
            return (DataTemplate)_page.Resources["recordingItemListTemplate"];
          default:
            return (DataTemplate)_page.Resources["recordingItemIconTemplate"];
        }
      }
    }

    /// <summary>
    /// Notifies subscribers that property has been changed
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public void ChangeProperty(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
    /// Returns a ICommand for playing a recording
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
    /// <summary>
    /// Returns a ICommand for deleting a recording
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
    #endregion
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


    #region Commands subclasses
    #region base command class
    public abstract class RecordedCommand : ICommand
    {
      protected TvRecordedViewModel _viewModel;
      public event EventHandler CanExecuteChanged;

      public RecordedCommand(TvRecordedViewModel viewModel)
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
      public SortCommand(TvRecordedViewModel viewModel)
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
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 99)/*Genre*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 100)/*Watched*/));
        RecordingCollectionView view = (RecordingCollectionView)_viewModel.Recordings;
        dlgMenu.SelectedIndex = (int)view.SortMode;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected

        //tell the view to sort
        view.SortMode = (TvRecordedViewModel.SortType)dlgMenu.SelectedIndex;

        //and tell the model that the sort property is changed
        _viewModel.ChangeProperty("SortLabel");
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
      public ViewCommand(TvRecordedViewModel viewModel)
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
      public CleanUpCommand(TvRecordedViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        MpDialogYesNo dlgMenu = new MpDialogYesNo();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
        dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 101);//"This will delete all recordings you have watched. Are you sure?";
        dlgMenu.ShowDialog();
        if (dlgMenu.DialogResult == DialogResult.No) return;
        IList itemlist = Recording.ListAll();
        foreach (Recording rec in itemlist)
        {
          if (rec.TimesWatched > 0)
          {
            TvServer server = new TvServer();
            server.DeleteRecording(rec.IdRecording);
          }
        }
        _viewModel.ChangeProperty("Recordings");
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
      public FullScreenTvCommand(TvRecordedViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        _viewModel.Page.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
      }
      public override bool CanExecute(object parameter)
      {
        return (TvPlayerCollection.Instance.Count != 0);
      }
    }
    #endregion

    #region Play command class
    /// <summary>
    /// Play command will start playing a recording
    /// </summary>
    public class PlayCommand : RecordedCommand
    {
      private delegate void MediaPlayerErrorDelegate();
      private delegate void MediaPlayerOpenDelegate();
      /// <summary>
      /// Initializes a new instance of the <see cref="CleanUpCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public PlayCommand(TvRecordedViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        string fileName = parameter as string;
        if (!System.IO.File.Exists(fileName))
        {
          MpDialogOk dlgError = new MpDialogOk();
          dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgError.Owner = _viewModel.Window;
          dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
          dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
          dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 96)/*File not found*/+ " " + fileName;
          dlgError.ShowDialog();
          return;
        }
        if (TvPlayerCollection.Instance.Count > 0)
        {
          TvPlayerCollection.Instance.DisposeAll();
          _viewModel.ChangeProperty("VideoBrush");
          _viewModel.ChangeProperty("FullScreen");
        }
        TvMediaPlayer player = TvPlayerCollection.Instance.Get(null, fileName);
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
      public DeleteCommand(TvRecordedViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        Recording recording = parameter as Recording;
        MpDialogYesNo dlgMenu = new MpDialogYesNo();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
        dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 95);//"Are you sure to delete this recording ?";
        dlgMenu.ShowDialog();
        if (dlgMenu.DialogResult == DialogResult.No) return;

        TvServer server = new TvServer();
        server.DeleteRecording(recording.IdRecording);
        _viewModel.ChangeProperty("Recordings");
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
      public ContextMenuCommand(TvRecordedViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        RecordingModel item = parameter as RecordingModel;
        Recording recording = item.Recording;
        if (recording == null) return;
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 92)/*Play recording*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 93)/*Delete recording*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 94)/*Settings*/));
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected
        switch (dlgMenu.SelectedIndex)
        {
          case 0:
            {
              ICommand command = _viewModel.Play;
              command.Execute(recording.FileName);
            }
            break;

          case 1:
            {
              ICommand command = _viewModel.Delete;
              command.Execute(recording);
            }
            break;

          case 2:
            {
              TvRecordedInfo infopage = new TvRecordedInfo(recording);
              _viewModel.Page.NavigationService.Navigate(infopage);
            }
            break;
        }
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
      public FullScreenCommand(TvRecordedViewModel viewModel)
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
    #endregion

    #region RecordingDatabaseModel class
    /// <summary>
    /// Class representing our database model.
    /// It simply retrieves all recordings from the tv database and 
    /// creates a list of RecordingModel
    /// </summary>
    class RecordingDatabaseModel
    {
      #region variables
      List<RecordingModel> _listRecordings = new List<RecordingModel>();
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="RecordingDatabaseModel"/> class.
      /// </summary>
      public RecordingDatabaseModel()
      {
        Reload();
      }
      /// <summary>
      /// Refreshes the list with the database.
      /// </summary>
      public void Reload()
      {
        _listRecordings.Clear();
        IList recordings = Recording.ListAll();

        foreach (Recording recording in recordings)
        {
          RecordingModel item = new RecordingModel(recording);
          _listRecordings.Add(item);
        }
      }

      /// <summary>
      /// Gets the recordings.
      /// </summary>
      /// <value>IList containing 0 or more RecordingModel instances.</value>
      public IList Recordings
      {
        get
        {
          return _listRecordings;
        }
      }
    }
    #endregion

    #region RecordingCollectionView class
    /// <summary>
    /// This class represents the recording view
    /// </summary>
    class RecordingCollectionView : ListCollectionView
    {
      #region variables
      SortType _sortMode = SortType.Date;
      private RecordingDatabaseModel _model;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="RecordingCollectionView"/> class.
      /// </summary>
      /// <param name="model">The database model.</param>
      public RecordingCollectionView(RecordingDatabaseModel model)
        : base(model.Recordings)
      {
        _model = model;
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
            this.CustomSort = new RecordingComparer(_sortMode);
          }
        }
      }
    }
    #endregion

    #region RecordingComparer class
    /// <summary>
    /// Helper class to compare 2 RecordingModels
    /// </summary>
    public class RecordingComparer : IComparer
    {
      #region variables
      SortType _sortMode;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="RecordingComparer"/> class.
      /// </summary>
      /// <param name="sortMode">The sort mode.</param>
      public RecordingComparer(SortType sortMode)
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
        RecordingModel model1 = (RecordingModel)x;
        RecordingModel model2 = (RecordingModel)y;
        switch (_sortMode)
        {
          case SortType.Channel:
            return String.Compare(model1.Channel, model2.Channel, true);
          case SortType.Date:
            return model1.StartTime.CompareTo(model2.StartTime);
          case SortType.Duration:
            TimeSpan t1 = model1.EndTime - model1.StartTime;
            TimeSpan t2 = model2.EndTime - model2.StartTime;
            return t1.CompareTo(t2);
          case SortType.Genre:
            return String.Compare(model1.Genre, model2.Genre, true);
          case SortType.Title:
            return String.Compare(model1.Title, model2.Title, true);
          case SortType.Watched:
            return model1.TimesWatched.CompareTo(model2.TimesWatched);
        }
        return 0;
      }
    }
    #endregion

  }
}
