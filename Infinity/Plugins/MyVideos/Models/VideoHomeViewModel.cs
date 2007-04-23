//
// TODO: - Add support for different languages (databindings)
//
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Threading;
using Dialogs;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using System.Windows.Media;
using System.Windows.Navigation;
using ProjectInfinity.Playlist;
using ProjectInfinity.Settings;
using ProjectInfinity.Navigation;
using Dialogs;
using MediaLibrary;

namespace MyVideos
{
  public class VideoHomeViewModel : DispatcherObject, INotifyPropertyChanged
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
    ICommand _navfullscreenCommand;
    ICommand _contextMenuCommand;

    Window _window;
    Page _page;
    ViewType _viewType = ViewType.List;
    SortType _sortType = SortType.Name;
    string _currentFolder = null;
    #endregion

    #region ctor
    public VideoHomeViewModel()
    {
      _dataModel = new VideoDatabaseModel(this);
      _videosView = new VideoCollectionView(_dataModel);

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
    public ICommand NavFullscreen
    {
      get
      {
        if (_navfullscreenCommand == null)
          _navfullscreenCommand = new NavFullscreenCommand(this);

        return _navfullscreenCommand;
      }
    }

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
    public ICommand ContextMenu
    {
      get
      {
        if (_contextMenuCommand == null)
          _contextMenuCommand = new ContextMenuCommand(this);

        return _contextMenuCommand;
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
          ChangeProperty("ViewModeType");
          ChangeProperty("ViewLabel");
        }
      }
    }
    public string CurrentFolder
    {
      get
      {
        return _currentFolder;
      }
      set
      {
        _currentFolder = value;
      }
    }


    public string ViewModeType
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

    public string DateLabel
    {
      get { return DateTime.Now.ToString("dd-MM HH:mm"); }
    }

    public Window Window
    {
      get { return ServiceScope.Get<INavigationService>().GetWindow(); }
    }

    public void Reload()
    {
      _dataModel.Reload();
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
      ServiceScope.Get<INavigationService>().Navigate(new VideoPlaylist());
    }
  }
  #endregion

  #region navigatefullscreen command class
  public class NavFullscreenCommand : BaseCommand
  {
    public NavFullscreenCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      ServiceScope.Get<INavigationService>().Navigate(new VideoFullscreen());
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
      if (parameter == null) return;  // if the parameter variable is null they have clicked outside an item

      // set the media to be played and then toggle fullscreen
      VideoModel videoModel = (VideoModel)parameter;
      if (videoModel.IsFolder)
      {
        _viewModel.CurrentFolder = videoModel.Path;
        _viewModel.Reload();
        return;
      }
      string fileName = videoModel.Path;

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
      _viewModel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerOpenDelegate(OnMediaOpened));
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
      _viewModel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
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
      _viewModel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerOpenDelegate(OnMediaOpened));
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
      _viewModel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
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
      dlgMenu.SelectedIndex = (int)_viewModel.ViewMode;
      dlgMenu.ShowDialog();
      switch (dlgMenu.SelectedIndex)
      {
        case 0:
          break;
        case 1:
          _viewModel.ViewMode = VideoHomeViewModel.ViewType.List;
          break;
        case 2:
          _viewModel.ViewMode = VideoHomeViewModel.ViewType.Icon;
          break;
      }
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
      dlgMenu.SelectedIndex = (int)(_viewModel.Videos as VideoCollectionView).SortMode;
      dlgMenu.ShowDialog();

      if (dlgMenu.SelectedIndex < 0) return;    // no menu item selected

      (_viewModel.Videos as VideoCollectionView).SortMode = (VideoHomeViewModel.SortType)dlgMenu.SelectedIndex;

      _viewModel.ChangeProperty("SortLabel");
    }
  }
  #endregion

  #region contextmenu command class
  public class ContextMenuCommand : BaseCommand
  {
    public ContextMenuCommand(VideoHomeViewModel viewModel)
      : base(viewModel)
    {
    }

    public override void Execute(object parameter)
    {
      if (parameter == null) return;
      VideoModel model = parameter as VideoModel;
      if (model == null) return;

      MpMenu dlgMenu = new Dialogs.MpMenu();
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = _viewModel.Window;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68); // Menu
      dlgMenu.SubTitle = model.Title;
      dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 30))); // Add to playlist
      dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 29))); // View information
      dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 27))); // Download information
      dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 28))); // Delete from disk
      dlgMenu.ShowDialog();

      switch (dlgMenu.SelectedIndex)
      {
        case 0:
          // Add to playlist
          ICommand addToPlaylist = new AddToPlaylistCommand(_viewModel);
          addToPlaylist.Execute(model);
          break;
      }
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
  class VideoDatabaseModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    VideoHomeViewModel _viewModel;
    List<VideoModel> _listVideos = new List<VideoModel>();

    /// <summary>
    /// Initializes a new instance of <see cref="VideoDatabaseModel" /> class.
    /// </summary>
    public VideoDatabaseModel(VideoHomeViewModel model)
    {
      _viewModel = model;
      Reload();
    }

    /// <summary>
    /// Reloads the list with videos.
    /// </summary>
    public void Reload()
    {
      _listVideos.Clear();


      VideoSettings settings = new VideoSettings();
      ServiceScope.Get<ISettingsManager>().Load(settings);
      if (_viewModel.CurrentFolder == null)
      {
        //begin-just some testcode to show how you can ask the user to select 1 or more folders
        if (settings.Shares == null)
        {
          settings.Shares = new List<string>();
        }
        if (settings.Shares.Count == 0)
        {
          FolderDialog dlg = new FolderDialog();
          Window w = ServiceScope.Get<INavigationService>().GetWindow();
          dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlg.Owner = w;
          dlg.Title = "";
          dlg.Header = ServiceScope.Get<ILocalisation>().ToString("myvideos", 35);//Video folders
          dlg.ShowDialog();
          List<Folder> shares = dlg.SelectedFolders;

          foreach (Folder share in shares)
          {
            settings.Shares.Add(share.FullPath);
          }
          ServiceScope.Get<ISettingsManager>().Save(settings);
        }
        //end-just some testcode to show how you can ask the user to select 1 or more folders

        if (settings.Shares != null)
        {
          foreach (string share in settings.Shares)
          {
            string pathName = share;
            int pos = pathName.LastIndexOf(@"\");
            if (pos >= 0)
              pathName = pathName.Substring(1 + pos);
            VideoModel item = new VideoModel(pathName, 0, share);
            item.IsFolder = true;
            _listVideos.Add(item);
          }
        }
      }
      else
      {
        bool reachedRoot = false;
        foreach (string share in settings.Shares)
        {
          if (String.Compare(share, _viewModel.CurrentFolder, true) == 0) reachedRoot = true;
        }
        string pathName = _viewModel.CurrentFolder;
        int pos = pathName.LastIndexOf(@"\");
        if (pos >= 0)
          pathName = pathName.Substring(0, pos);
        VideoModel item = new VideoModel("..", 0, pathName);
        if (reachedRoot)
        {
          item.Path = null;
        }
        item.IsFolder = true;
        _listVideos.Add(item);

        string[] folders = Directory.GetDirectories(_viewModel.CurrentFolder);
        foreach (string folder in folders)
        {
           pathName = folder;
          pos = folder.LastIndexOf(@"\");
          if (pos >= 0)
            pathName = pathName.Substring(1 + pos);

          item = new VideoModel(pathName, 0, folder);
          item.IsFolder = true;
          _listVideos.Add(item);
        }
        string[] files = Directory.GetFiles(_viewModel.CurrentFolder);

        foreach (string file in files)
        {
          string ext = System.IO.Path.GetExtension(file).ToLower();
          if (settings.VideoExtensions.IndexOf(ext) >= 0)
          {
            FileInfo fi = new FileInfo(file);
            string fileName = fi.Name;
            if (!settings.ShowExtensions)
              fileName = fileName.Substring(0, fileName.Length - ext.Length);

            item = new VideoModel(fileName, (int)fi.Length, file);
            _listVideos.Add(item);
          }
        }
      }
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs("Videos"));
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
      _model.PropertyChanged += new PropertyChangedEventHandler(_model_PropertyChanged);
    }

    void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
