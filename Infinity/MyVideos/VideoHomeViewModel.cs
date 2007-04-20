//
// TODO: - Add support for different languages (databindings)
//
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using ProjectInfinity;
using ProjectInfinity.Localisation;
using System.IO;
using System.Collections;
using System.Windows.Data;
using Dialogs;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using System.Windows.Media;
using System.Windows.Navigation;
using ProjectInfinity.Playlist;

namespace MyVideos
{
  public class VideoHomeViewModel : INotifyPropertyChanged
  {
    #region variables
    public event PropertyChangedEventHandler PropertyChanged;

    public enum SortType
    {
      Name,
      Date,
      Size,
      Year,
      Rating,
      DVD
    }

    public enum ViewType
    {
      FilmStrip,
      List,
      Icon
    }

    VideoCollectionView _videosView;
    VideoDatabaseModel _dataModel;

    ICommand _sortCommand;
    ICommand _viewCommand;
    ICommand _switchCommand;
    ICommand _playlistCommand;
    ICommand _dvdCommand;
    ICommand _itemCommand;
    ICommand _fullscreenCommand;

    Window _window;
    Page _page;
    ViewType _viewType = ViewType.List;
    SortType _sortType = SortType.Name;
    #endregion

    #region ctor
    public VideoHomeViewModel(Page page)
    {
      _dataModel = new VideoDatabaseModel();
      _videosView = new VideoCollectionView(_dataModel);

      _page = page;
      _window = Window.GetWindow(_page);
    }
    #endregion

    #region ILocalisation Properties

    public string SortLabel
    {
      get
      {
        switch (_videosView.SortMode)
        {
          case SortType.Name:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 5); // Sort by: Name
          case SortType.Date:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 6); // Sort by: Date
          case SortType.Size:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 7); // Sort by: Size
          case SortType.Year:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 8); // Sort by: Year
          case SortType.Rating:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 9); // Sort by: Rating
          case SortType.DVD:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 10); // Sort by: DVD
          default:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 1); // Sort
        }
      }
    }

    public string ViewLabel
    {
      get
      {
        switch (_viewType)
        {
          case ViewType.FilmStrip:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 18);  // View: Film strip
          case ViewType.Icon:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 20);  // View: Icon
          case ViewType.List:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 19);  // View: List
          default:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 19);  // View: List
        }
      }//return ServiceScope.Get<ILocalisation>().ToString("myvideos", 0); } // View
    }

    public string SwitchLabel
    {
      get { return ServiceScope.Get<ILocalisation>().ToString("myvideos", 2); } // Switch view
    }

    public string PlaylistLabel
    {
      get { return ServiceScope.Get<ILocalisation>().ToString("myvideos", 3); } // Playlist
    }

    public string DVDLabel
    {
      get { return ServiceScope.Get<ILocalisation>().ToString("myvideos", 4); } // Play DVD
    }

    #endregion

    #region ICommand Properties
    public ICommand Play
    {
      get
      {
        if (_itemCommand == null)
          _itemCommand = new PlayCommand(this);

        return _itemCommand;
      }
    }

    public ICommand Fullscreen
    {
      get
      {
        if (_fullscreenCommand == null)
          _fullscreenCommand = new FullscreenCommand(this);

        return _fullscreenCommand;
      }
    }

    public ICommand Sort
    {
      get
      {
        if (_sortCommand == null)
          _sortCommand = new SortCommand(this);

        return _sortCommand;
      }
    }

    public ICommand View
    {
      get
      {
        if (_viewCommand == null)
          _viewCommand = new ViewCommand(this);

        return _viewCommand;
      }
    }

    public ICommand DVD
    {
      get
      {
        if (_dvdCommand == null)
          _dvdCommand = new DVDCommand(this);

        return _dvdCommand;
      }
    }

    public ICommand Playlist
    {
      get
      {
        if (_playlistCommand == null)
          _playlistCommand = new PlaylistCommand(this);

        return _playlistCommand;
      }
    }
    #endregion

    #region properties
    public ViewType ViewMode
    {
      get { return _viewType; }
      set
      {
        if (_viewType != value)
        {
          _viewType = value;
          ChangeProperty("ViewTypeMode");
        }
      }
    }

    public string ViewTypeMode
    {
      get
      {
        switch (_viewType)
        {
          case ViewType.Icon:
            return "Icon";
          default:
            return "List";
        }
      }
    }

    public Brush VideoBrush
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          MediaPlayer player = (MediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0].UnderlyingPlayer;

          VideoDrawing videoDrawing = new VideoDrawing();
          videoDrawing.Player = player;
          videoDrawing.Rect = new Rect(0, 0, player.NaturalVideoWidth, player.NaturalVideoHeight);
          DrawingBrush videoBrush = new DrawingBrush();
          videoBrush.Drawing = videoDrawing;

          return videoBrush;
        }

        return new SolidColorBrush(Colors.Black);
      }
    }

    public Visibility IsVideoPresent
    {
      get { return (ServiceScope.Get<IPlayerCollectionService>().Count != 0) ? Visibility.Visible : Visibility.Collapsed; }
    }

    public CollectionView Videos
    {
      get
      {
        if (_videosView == null)
          _videosView = new VideoCollectionView(_dataModel);

        return _videosView;
      }
    }

    public DataTemplate ItemTemplate
    {
      get
      {
        switch (_viewType)
        {
          case ViewType.List:
            return (DataTemplate)_page.Resources["videoItemListTemplate"];
          default:
            return (DataTemplate)_page.Resources["videoItemListTemplate"];
        }
      }
    }

    public string DateLabel
    {
      get { return DateTime.Now.ToString("dd-MM HH:mm"); }
    }

    public Window Window
    {
      get { return _window; }
    }

    public Page Page
    {
      get { return _page; }
    }
    #endregion

    public void ChangeProperty(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  #region add to playlist command class
  public class AddToPlaylistCommand : BaseCommand
  {
    public AddToPlaylistCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      VideoModel model = (VideoModel)parameter;

      PlaylistManager _manager = (PlaylistManager)ServiceScope.Get<IPlaylistManager>();

      if (_manager == null)
      {
        _manager = new PlaylistManager();
        ServiceScope.Add<IPlaylistManager>(_manager);
      }

      ServiceScope.Get<IPlaylistManager>().Add(model);
    }
  }
  #endregion

  #region playlist command class
  public class PlaylistCommand : BaseCommand
  {
    public PlaylistCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      _viewModel.Page.NavigationService.Navigate(new Uri("/MyVideos;component/VideoPlaylist.xaml", UriKind.Relative));
    }
  }
  #endregion

  #region fullscreen command class
  class FullscreenCommand : BaseCommand
  {
    public FullscreenCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      Window window = _viewModel.Window;

      if (window.WindowState == WindowState.Maximized)
      {
        window.ShowInTaskbar = true;
        window.WindowStyle = WindowStyle.SingleBorderWindow;
        window.WindowState = WindowState.Normal;
      }
      else
      {
        window.ShowInTaskbar = false;
        window.WindowStyle = WindowStyle.None;
        window.WindowState = WindowState.Maximized;
      }
    }
  }
  #endregion

  #region play command class
  public class PlayCommand : BaseCommand
  {
    private delegate void MediaPlayerErrorDelegate();
    private delegate void MediaPlayerOpenDelegate();

    public PlayCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      // set the media to be played and then toggle fullscreen
      VideoModel videoModel = (VideoModel)parameter;
      string fileName = videoModel.Path;
      //ServiceScope.Get<ILogger>().Info("Video:  path: " + fileName);

      if (!File.Exists(fileName))
      {
        MpDialogOk dlgError = new MpDialogOk();
        dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgError.Owner = _viewModel.Window;
        dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
        dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
        dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 96)/*File not found*/+ "\r\n" + fileName;
        dlgError.ShowDialog();
        return;
      }

      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        ServiceScope.Get<IPlayerCollectionService>().Clear();
        _viewModel.ChangeProperty("VideoBrush");
        _viewModel.ChangeProperty("Fullscreen");
        _viewModel.ChangeProperty("IsVideoPresent");
      }

      VideoPlayer player = new VideoPlayer(fileName);
      ServiceScope.Get<IPlayerCollectionService>().Add(player);
      player.MediaFailed += new EventHandler<MediaExceptionEventArgs>(player_MediaFailed);
      player.MediaOpened += new EventHandler(player_MediaOpened);
      player.Open(PlayerMediaType.Movie, fileName);
      player.Play();
    }

    void player_MediaOpened(object sender, EventArgs e)
    {
      _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerOpenDelegate(OnMediaOpened));
    }

    private void OnMediaOpened()
    {
      _viewModel.ChangeProperty("VideoBrush");
      _viewModel.ChangeProperty("Fullscreen");
      _viewModel.ChangeProperty("IsVideoPresent");
    }

    void player_MediaFailed(object sender, MediaExceptionEventArgs e)
    {
      ServiceScope.Get<ILogger>().Error("Video:  error while playing: " + e.ErrorException.Message);
      _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
    }

    private void OnMediaPlayerError()
    {
      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        VideoPlayer player = (VideoPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.HasError)
        {
          MpDialogOk dlgError = new MpDialogOk();
          dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgError.Owner = _viewModel.Window;
          dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
          dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
          dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 38)/*Unable to open the file*/+ "\r\n" + player.ErrorMessage;
          dlgError.ShowDialog();
        }
      }
      ServiceScope.Get<IPlayerCollectionService>().Clear();
    }

    public override bool CanExecute(object parameter)
    {
      return base.CanExecute(parameter);
    }
  }
  #endregion

  #region dvd command class
  public class DVDCommand : BaseCommand
  {
    private delegate void MediaPlayerErrorDelegate();
    private delegate void MediaPlayerOpenDelegate();

    public DVDCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      // Show a dialog with all sorting options
      MpMenu dlgMenu = new MpMenu();
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = _viewModel.Window;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("myvideos", 25);// "DVD Playback";
      dlgMenu.SubTitle = ServiceScope.Get<ILocalisation>().ToString("myvideos", 24); // Please select drive to play from

      string[] drives = Directory.GetLogicalDrives();

      foreach (string drive in drives)
      {
        DriveInfo dInfo = new DriveInfo(drive);

        if (dInfo.DriveType == DriveType.CDRom)
          dlgMenu.Items.Add(new DialogMenuItem(drive));
      }

      dlgMenu.ShowDialog();

      if (dlgMenu.SelectedIndex < 0) return; // no selected drive

      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        ServiceScope.Get<IPlayerCollectionService>().Clear();
        _viewModel.ChangeProperty("VideoBrush");
        _viewModel.ChangeProperty("Fullscreen");
        _viewModel.ChangeProperty("IsVideoPresent");
      }

      VideoPlayer player = new VideoPlayer(dlgMenu.SelectedItem.Label1);
      ServiceScope.Get<IPlayerCollectionService>().Add(player);
      player.MediaFailed += new EventHandler<MediaExceptionEventArgs>(player_MediaFailed);
      player.MediaOpened += new EventHandler(player_MediaOpened);
      player.Open(PlayerMediaType.DVD, dlgMenu.SelectedItem.Label1);
      player.Play();
    }

    void player_MediaOpened(object sender, EventArgs e)
    {
      _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerOpenDelegate(OnMediaOpened));
    }

    private void OnMediaOpened()
    {
      _viewModel.ChangeProperty("VideoBrush");
      _viewModel.ChangeProperty("Fullscreen");
      _viewModel.ChangeProperty("IsVideoPresent");
    }

    void player_MediaFailed(object sender, MediaExceptionEventArgs e)
    {
      ServiceScope.Get<ILogger>().Error("Video: error while playing DVD: " + e.ErrorException.Message);
      _viewModel.Page.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
    }

    private void OnMediaPlayerError()
    {
      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        VideoPlayer player = (VideoPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];
        if (player.HasError)
        {
          MpDialogOk dlgError = new MpDialogOk();
          dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgError.Owner = _viewModel.Window;
          dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
          dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
          dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("myvideos", 26)/*Unable to open the file*/+ "\r\n" + player.ErrorMessage;
          dlgError.ShowDialog();
        }
      }
      ServiceScope.Get<IPlayerCollectionService>().Clear();
    }
  }
  #endregion

  #region view command class
  public class ViewCommand : BaseCommand
  {
    public ViewCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      // Show a dialog with all sorting options
      MpMenu dlgMenu = new MpMenu();
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = _viewModel.Window;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("myvideos", 11);// "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 21)/*Film strip*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 22)/*List*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 23)/*Icon*/));
      dlgMenu.ShowDialog();
    }
  }
  #endregion

  #region sort command class
  public class SortCommand : BaseCommand
  {
    public SortCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      // Show a dialog with all sorting options
      MpMenu dlgMenu = new MpMenu();
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = _viewModel.Window;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("myvideos", 11);// "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 12)/*Name*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 13)/*Date*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 14)/*Size*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 15)/*Year*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 16)/*Rating*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 17)/*DVD*/));
      dlgMenu.ShowDialog();

      if (dlgMenu.SelectedIndex < 0) return;    // no menu item selected

      (_viewModel.Videos as VideoCollectionView).SortMode = (VideoHomeViewModel.SortType)dlgMenu.SelectedIndex;

      _viewModel.ChangeProperty("SortLabel");
    }
  }
  #endregion

  #region base command class
  public abstract class BaseCommand : ICommand
  {
    protected VideoHomeViewModel _viewModel;
    public event EventHandler CanExecuteChanged;

    public BaseCommand(VideoHomeViewModel viewModel)
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
        this.CanExecuteChanged(this, EventArgs.Empty);
    }
  }
  #endregion

  #region VideoDatabaseModel class
  class VideoDatabaseModel
  {
    List<VideoModel> _listVideos = new List<VideoModel>();

    /// <summary>
    /// Initializes a new instance of <see cref="VideoDatabaseModel" /> class.
    /// </summary>
    public VideoDatabaseModel()
    {
      Reload();
    }

    /// <summary>
    /// Reloads the list with videos.
    /// </summary>
    public void Reload()
    {
      _listVideos.Clear();

      // Temporary code, needs to be replaced when we get a real media collector
      string[] files = Directory.GetFiles("C:\\", "*.mpg");

      foreach (string file in files)
      {
        FileInfo fi = new FileInfo(file);
        VideoModel item = new VideoModel(fi.Name, (int)fi.Length, file);

        _listVideos.Add(item);
      }
    }

    /// <summary>
    /// Gets the videos.
    /// </summary>
    public IList Videos
    {
      get { return _listVideos; }
    }
  }
  #endregion

  #region VideoCollectionView class
  /// <summary>
  /// This class represents the video view.
  /// </summary>
  class VideoCollectionView : ListCollectionView
  {
    private MyVideos.VideoHomeViewModel.SortType _sortMode = VideoHomeViewModel.SortType.Date;
    private VideoDatabaseModel _model;

    /// <summary>
    /// Initializes a new instance of <see cref="VideoCollectionView" /> class.
    /// </summary>
    /// <param name="model">The database model.</param>
    public VideoCollectionView(VideoDatabaseModel model)
      : base(model.Videos)
    {
      _model = model;
    }

    /// <summary>
    /// Gets or sets the sort mode.
    /// </summary>
    public MyVideos.VideoHomeViewModel.SortType SortMode
    {
      get { return _sortMode; }
      set
      {
        if (_sortMode != value)
        {
          _sortMode = value;
          this.CustomSort = new VideoComparer(_sortMode);
        }
      }
    }
  }
  #endregion

  #region VideoComparer class
  public class VideoComparer : IComparer
  {
    private VideoHomeViewModel.SortType _sortMode;

    /// <summary>
    /// Initializes a new instance of the VideoComparer class.
    /// </summary>
    /// <param name="sortMode"></param>
    public VideoComparer(VideoHomeViewModel.SortType sortMode)
    {
      _sortMode = sortMode;
    }

    public int Compare(object x, object y)
    {
      VideoModel model1 = (VideoModel)x;
      VideoModel model2 = (VideoModel)y;

      switch (_sortMode)
      {
        case VideoHomeViewModel.SortType.Date:
          return new FileInfo(model1.Path).CreationTime.CompareTo(new FileInfo(model2.Path).CreationTime);
        case VideoHomeViewModel.SortType.DVD:
          return 0;
        case VideoHomeViewModel.SortType.Name:
          return String.Compare(model1.Title, model2.Title);
        case VideoHomeViewModel.SortType.Rating:
          return 0;
        case VideoHomeViewModel.SortType.Size:
          return (int)(model2.RealSize - model1.RealSize);
        case VideoHomeViewModel.SortType.Year:
          return 0;
      }

      return 0;
    }
  }
  #endregion
}
