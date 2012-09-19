#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using CSScriptLibrary;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using MediaPortal.Player.Subtitles;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

#pragma warning disable 108

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// MyVideo GUI class when not using DB driven views.
  /// </summary>
  [PluginIcons("WindowPlugins.GUIVideoFiles.Video.gif", "WindowPlugins.GUIVideoFiles.VideoDisabled.gif")]
  public class GUIVideoFiles : GUIVideoBaseWindow, ISetupForm, IShowPlugin, IMDB.IProgress
  {
    #region map settings

    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _Stack;
      protected bool _SortAscending;

      public MapSettings()
      {
        _SortBy = 0; //name
        _ViewAs = 0; //list
        _Stack = true;
        _SortAscending = true;
      }

      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy; }
        set { _SortBy = value; }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }

      [XmlElement("Stack")]
      public bool Stack
      {
        get { return _Stack; }
        set { _Stack = value; }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }
    }

    #endregion
    
    #region variables

    private static bool _askBeforePlayingDVDImage;
    private static VirtualDirectory _virtualDirectory;
    private static string _currentFolder = string.Empty;
    private static PlayListPlayer _playlistPlayer;

    private MapSettings _mapSettings = new MapSettings();
    private DirectoryHistory _history = new DirectoryHistory();
    private string _virtualStartDirectory = string.Empty;
    private int _currentSelectedItem = -1;

    // File menu
    private string _fileMenuDestinationDir = string.Empty;
    private bool _fileMenuEnabled;
    private string _fileMenuPinCode = string.Empty;

    private bool _scanning;
    private int _scanningFileNumber = 1;
    private int _scanningFileTotal = 1;
    private bool _isFuzzyMatching;
    private bool _scanSkipExisting;
    private bool _getActors = true;
    private bool _markWatchedFiles = true;
    private ArrayList _conflictFiles = new ArrayList();
    private bool _switchRemovableDrives;
    // Stacked files duration - for watched status/also used in GUIVideoTitle
    public static int TotalMovieDuration;
    public static ArrayList StackedMovieFiles = new ArrayList();
    public static bool IsStacked;

    private List<GUIListItem> _cachedItems = new List<GUIListItem>();
    private string _cachedDir;

    private bool _resetSMSsearch;
    private bool _oldStateSMSsearch;
    private DateTime _resetSMSsearchDelay;

    private int _howToPlayAll = 3;
    //Video info in share view before play
    private bool _videoInfoInShare;
    private bool _playClicked;

    // external player
    private static bool _useInternalVideoPlayer = true;
    private static bool _useInternalDVDVideoPlayer = true;
    private static string _externalPlayerExtensions = string.Empty;
	
    private int _watchedPercentage = 95;
    public static GUIListItem CurrentSelectedGUIItem;

    private Thread _setThumbs;
    private List<GUIListItem> _threadGUIItems = new List<GUIListItem>();
    private ISelectDVDHandler _threadISelectDVDHandler;
    private bool _setThumbsThreadAborted;
    
    // grabber index holds information/urls of available grabbers to download
    private static string _grabberIndexFile = Config.GetFile(Config.Dir.Config, "MovieInfoGrabber.xml");
    private static string _grabberIndexUrl = @"http://install.team-mediaportal.com/MP1/MovieInfoGrabber.xml";
    private static Dictionary<string, IIMDBScriptGrabber> _grabberList;

    private int _resetCount;
    private string _selectedFilename = string.Empty;

    private bool _useSortTitle = false;
    private bool _useOnlyNfoScraper = false;

    private static IMDB.InternalMovieInfoScraper _internalGrabber = new IMDB.InternalMovieInfoScraper();

    #endregion

    #region constructors

    static GUIVideoFiles()
    {
      _playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public GUIVideoFiles()
    {
      GetID = (int)Window.WINDOW_VIDEOS;
    }

    #endregion

    #region Overrides

    protected override bool CurrentSortAsc
    {
      get { return _mapSettings.SortAscending; }
      set { _mapSettings.SortAscending = value; }
    }

    protected override VideoSort.SortMethod CurrentSortMethod
    {
      get { return (VideoSort.SortMethod) _mapSettings.SortBy; }
      set { _mapSettings.SortBy = (int) value; }
    }

    protected override Layout CurrentLayout
    {
      get { return (Layout) _mapSettings.ViewAs; }
      set { _mapSettings.ViewAs = (int) value; }
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\myVideo.xml"));
    }

    public override void OnAdded()
    {
      base.OnAdded();
      new IMDB(this);
      g_Player.PlayBackStopped += OnPlayBackStopped;
      g_Player.PlayBackEnded += OnPlayBackEnded;
      g_Player.PlayBackStarted += OnPlayBackStarted;
      g_Player.PlayBackChanged += OnPlayBackChanged;
      GUIWindowManager.Receivers += GUIWindowManager_OnNewMessage;
      LoadSettings();
    }

    protected override void OnSearchNew()
    {
      int maximumShares = 128;
      ArrayList availablePaths = new ArrayList();

      using (Profile.Settings xmlreader = new MPSettings())
      {
        for (int index = 0; index < maximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string shareDir = xmlreader.GetValueAsString("movies", sharePath, "");
          string shareScan = String.Format("sharescan{0}", index);
          bool shareScanData = xmlreader.GetValueAsBool("movies", shareScan, true);

          if (shareScanData && shareDir != string.Empty)
          {
            availablePaths.Add(shareDir);
          }
        }

        if (!_useOnlyNfoScraper)
        {
          IMDBFetcher.ScanIMDB(this, availablePaths, true, true, true, false);
        }
        else
        {
          ArrayList nfoFiles = new ArrayList();

          foreach (string availablePath in availablePaths)
          {
            GetNfoFiles(availablePath, ref nfoFiles);
          }

          IMDBFetcher fetcher = new IMDBFetcher(this);
          fetcher.FetchNfo(nfoFiles, true, false);
        }
        
        // Send global message that movie is refreshed/scanned
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);

        _currentSelectedItem = facadeLayout.SelectedListItemIndex;

        if (_currentSelectedItem > 0)
        {
          _currentSelectedItem--;
        }

        LoadDirectory(_currentFolder);

        if (_currentSelectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _currentSelectedItem);
        }
      }
    }

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        currentLayout = (Layout) xmlreader.GetValueAsInt(SerializeName, "layout", (int) Layout.List);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);
        VideoState.StartWindow = xmlreader.GetValueAsInt("movies", "startWindow", GetID);
        VideoState.View = xmlreader.GetValueAsString("movies", "startview", "369");

        // Prevent unaccesible My Videos from corrupted config
        if (!IsVideoWindow(VideoState.StartWindow))
        {
          VideoState.StartWindow = GetID;
          VideoState.View = "369";
        }

        _isFuzzyMatching = xmlreader.GetValueAsBool("movies", "fuzzyMatching", false);
        _scanSkipExisting = xmlreader.GetValueAsBool("moviedatabase", "scanskipexisting", false);
        _getActors = xmlreader.GetValueAsBool("moviedatabase", "getactors", true);
        _markWatchedFiles = xmlreader.GetValueAsBool("movies", "markwatched", true);
        //_eachFolderIsMovie = xmlreader.GetValueAsBool("movies", "eachFolderIsMovie", false);
        _fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        _fileMenuPinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
        _howToPlayAll = xmlreader.GetValueAsInt("movies", "playallinfolder", 3);
        _watchedPercentage = xmlreader.GetValueAsInt("movies", "playedpercentagewatched", 95);
        _videoInfoInShare = xmlreader.GetValueAsBool("moviedatabase", "movieinfoshareview", false);
        
        _virtualDirectory = VirtualDirectories.Instance.Movies;
		    // External player
        _useInternalVideoPlayer = xmlreader.GetValueAsBool("movieplayer", "internal", true);
        _useInternalDVDVideoPlayer = xmlreader.GetValueAsBool("dvdplayer", "internal", true);
        _externalPlayerExtensions = xmlreader.GetValueAsString("movieplayer", "extensions", "");
        
        if (_virtualStartDirectory == string.Empty)
        {
          if (_virtualDirectory.DefaultShare != null)
          {
            if (_virtualDirectory.DefaultShare.IsFtpShare)
            {
              //remote:hostname?port?login?password?folder
              _currentFolder = _virtualDirectory.GetShareRemoteURL(_virtualDirectory.DefaultShare);
              _virtualStartDirectory = _currentFolder;
            }
            else
            {
              _currentFolder = _virtualDirectory.DefaultShare.Path;
              _virtualStartDirectory = _virtualDirectory.DefaultShare.Path;
            }
          }
        }

        _askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);

        if (xmlreader.GetValueAsBool("movies", "rememberlastfolder", false))
        {
          string lastFolder = xmlreader.GetValueAsString("movies", "lastfolder", _currentFolder);
          if (VirtualDirectory.IsImageFile(Path.GetExtension(lastFolder)))
          {
            lastFolder = "root";
          }
          if (lastFolder != "root")
          {
            _currentFolder = lastFolder;
          }
        }

        _switchRemovableDrives = xmlreader.GetValueAsBool("movies", "SwitchRemovableDrives", true);
        _useOnlyNfoScraper = xmlreader.GetValueAsBool("moviedatabase", "useonlynfoscraper", false);

      }

      if (_currentFolder.Length > 0 && _currentFolder == _virtualStartDirectory)
      {
        VirtualDirectory vDir = new VirtualDirectory();
        vDir.LoadSettings("movies");
      }
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      
      using (Profile.Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "layout", (int) currentLayout);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
      }
    }

    #endregion

    protected override string SerializeName
    {
      get { return "myvideo"; }
    }

    public override void OnAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) && (facadeLayout.Focus))
      {
        GUIListItem item = facadeLayout[0];
        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          _history.Set("0", _currentFolder);
          LoadDirectory(item.Path);
          return;
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeLayout[0];
        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          _history.Set("0", _currentFolder);
          LoadDirectory(item.Path);
        }
        return;
      }

      if (action.wID == Action.ActionType.ACTION_DELETE_ITEM && _fileMenuEnabled)
      {
        ShowFileMenu(true);
      }

      if (action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
      {
        _playClicked = true;
      }
      
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      base.LoadSettings();
      
      // This can't be in LoadSettings beacuse settings are loaded on MP start and it will disable SortTitle setting always
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Don't sort by sorttitle
        _useSortTitle = xmlreader.GetValueAsBool("moviedatabase", "usesorttitle", false);
        xmlreader.SetValueAsBool("moviedatabase", "usesorttitle", false);
      }
      
      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        Reset();
      }
      
      if (!IsVideoWindow(PreviousWindowId) && IsFolderPinProtected(_cachedDir))
      {
        //when the user left MyVideos completely make sure that we don't use the cache
        //if folder is pin protected and reload the dir completly including PIN request etc.
        _cachedItems.Clear();
        _cachedDir = null;
      }

      if (VideoState.StartWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(VideoState.StartWindow);
        return;
      }

      _resetCount = 0;

      // Go to default share from main MP menu
      if (_currentFolder == string.Empty)
      {
        _currentFolder = _virtualStartDirectory;
      }

      LoadFolderSettings(_currentFolder);

      //OnPageLoad is sometimes called when stopping playback.
      //So we use the cached version of the function here.
      LoadDirectory(_currentFolder, true);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      // set back Sort by sorttitle
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("moviedatabase", "usesorttitle", _useSortTitle);
      }

      if (_setThumbs != null && _setThumbs.IsAlive)
      {
        _setThumbs.Abort();
        _setThumbs = null;
      }
      
      // Set folder history when video window is not actually left (ie.: play movie, video info..)
      if (IsVideoWindow(newWindowId))
      {
        if (facadeLayout != null)
        {
          _history.Set(facadeLayout.SelectedListItemIndex.ToString(), _currentFolder);
        }
      }
      else // For real exit (ie.: to home, reset folder history)
      {
        _history.Set("0", _currentFolder);
      }

      SaveFolderSettings(_currentFolder);
      base.OnPageDestroy(newWindowId);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
          if (g_Player.Playing && g_Player.IsDVD &&
              message.Label.Equals(g_Player.CurrentFile.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase))
          // test if it is our drive
          {
            Log.Info("GUIVideoFiles: Stop dvd since DVD is ejected");
            g_Player.Stop();
          }

          if (GUIWindowManager.ActiveWindow == GetID)
          {
            if (Util.Utils.IsDVD(_currentFolder))
            {
              _currentFolder = string.Empty;
              LoadDirectory(_currentFolder);
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          facadeLayout.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:

          facadeLayout.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
          // Make sure file view is the current window
          if (VideoState.StartWindow != GetID)
          {
            VideoState.StartWindow = GetID;
            Reset();
            GUIWindowManager.ReplaceWindow(GetID);
          }
          _currentFolder = message.Label;
          LoadDirectory(_currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE:
          if (_switchRemovableDrives)
          {
            _currentFolder = message.Label;
            if (!Util.Utils.IsRemovable(message.Label))
            {
              _virtualDirectory.AddRemovableDrive(message.Label, message.Label2);
            }
          }
          LoadDirectory(_currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE:
          if (!Util.Utils.IsRemovable(message.Label))
          {
            _virtualDirectory.Remove(message.Label);
          }
          if (_currentFolder.Contains(message.Label))
          {
            _currentFolder = string.Empty;
          }
          LoadDirectory(_currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          if (_currentFolder == string.Empty || _currentFolder.Substring(0, 2) == message.Label)
          {
            _currentFolder = string.Empty;
            LoadDirectory(_currentFolder);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAY_DVD:
          OnPlayDVD(message.Label, GetID);
          break;
      }
      return base.OnMessage(message);
    }

    protected override void LoadDirectory(string newFolderName)
    {
      this.LoadDirectory(newFolderName, false);
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeLayout.SelectedListItem;
      TotalMovieDuration = 0;
      IsStacked = false;
      StackedMovieFiles.Clear();

      if (item == null)
      {
        _playClicked = false;
        return;
      }

      string path = item.Path;

      if (item.IsBdDvdFolder)
      {
        // Check if folder is actually a DVD. If so don't browse this folder, but play the DVD!
        if (File.Exists(path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          path = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        // Then it's a Blu-Ray. Play the Blu-Ray!
        else
        {
          path = item.Path + @"\BDMV\index.bdmv";
        }
      }

      if ((item.IsFolder && !item.IsBdDvdFolder))
      {
        _currentSelectedItem = -1;
        
        if (facadeLayout != null)
        {
          _history.Set(facadeLayout.SelectedListItemIndex.ToString(), _currentFolder);
        }
        
        LoadDirectory(path);
      }
      else
      {
        if (!_virtualDirectory.RequestPin(path))
        {
          _playClicked = false;
          return;
        }
        
        if (_virtualDirectory.IsRemote(path))
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(path, item.FileInfo.Length))
          {
            if (!_virtualDirectory.ShouldWeDownloadFile(path))
            {
              _playClicked = false;
              return;
            }
            
            if (!_virtualDirectory.DownloadRemoteFile(path, item.FileInfo.Length))
            {
              //show message that we are unable to download the file
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
              msg.Param1 = 916;
              msg.Param2 = 920;
              msg.Param3 = 0;
              msg.Param4 = 0;
              GUIWindowManager.SendMessage(msg);
              _playClicked = false;
              return;
            }
            //download subtitle files
            Thread subLoaderThread = new Thread(DownloadSubtitles);
            subLoaderThread.IsBackground = true;
            subLoaderThread.Name = "SubtitleLoader";
            subLoaderThread.Start();
          }
        }

        if (item.FileInfo != null)
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(path, item.FileInfo.Length))
          {
            _playClicked = false;
            return;
          }
        }
        
        string movieFileName = path;
        movieFileName = _virtualDirectory.GetLocalFilename(movieFileName);

        if (PlayListFactory.IsPlayList(movieFileName))
        {
          LoadPlayList(movieFileName);
          _playClicked = false;
          return;
        }

        if (!CheckMovie(movieFileName))
        {
          _playClicked = false;
          return;
        }

        if (_videoInfoInShare && VideoDatabase.HasMovieInfo(movieFileName) && !_playClicked)
        {
          OnInfo(facadeLayout.SelectedListItemIndex);
          return;
        }

        bool askForResumeMovie = true;
        _playClicked = false;

        #region StackEnabled and file is stackable

        // Proceed to file stack check if file is stackable
        if (_mapSettings.Stack && IsFileStackable(movieFileName))
        {
          IsStacked = true;
          int selectedFileIndex = 0;
          int movieDuration = 0;
          ArrayList movies = new ArrayList();
          List<GUIListItem> items = new List<GUIListItem>();
          IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
          
          // This will return all stackable files for current clicked item
          // Also will add all stackable files to videodatabase if they are not exist
          StackedMovieFiles = AddFileToDatabase(movieFileName);
          
          // Add movie files to list
          foreach (string file in StackedMovieFiles)
          {
            FileInformation fi = new FileInformation();
            GUIListItem itemMovie = new GUIListItem(Util.Utils.GetFilename(file), "", file, false, fi);
            items.Add(itemMovie);
          }
          
          // In the list must be at least 2 files so we check stackable movie for resume 
          // (which file have a stop time)
          bool asked = false;
          //ArrayList newItems = new ArrayList();
          string title = item.Label; // Dlg title

          if (movie != null && !movie.IsEmpty)
          {
            title = movie.Title;
          }
          
          for (int i = 0; i < items.Count; ++i)
          {
            IMDBMovie.SetMovieData(items[i]); // This will set IMDBMovie object and it will never be null
            movie = items[i].AlbumInfoTag as IMDBMovie;
             
            if (!asked)
            {
              selectedFileIndex++;
            }

            int idFile = VideoDatabase.GetFileId(movie.VideoFileName);

            if (idFile != -1)
            {
              int timeMovieStopped = VideoDatabase.GetMovieStopTime(idFile);
                
              if (timeMovieStopped > 0)
              {
                if (!asked)
                {
                  asked = true;
                  GUIResumeDialog.Result result =
                    GUIResumeDialog.ShowResumeDialog(title, movieDuration + timeMovieStopped,
                                                      GUIResumeDialog.MediaType.Video);

                  if (result == GUIResumeDialog.Result.Abort)
                  {
                    return;
                  }

                  if (result == GUIResumeDialog.Result.PlayFromBeginning)
                  {
                    VideoDatabase.DeleteMovieStopTime(idFile);
                  }
                  else
                  {
                    askForResumeMovie = false;
                  }
                }
              }
              
              // Total movie duration
              movieDuration += VideoDatabase.GetVideoDuration(idFile);
              TotalMovieDuration = movieDuration;
            }
          }

          for (int i = 0; i < items.Count; ++i)
          {
            GUIListItem temporaryListItem = (GUIListItem)items[i];
            movie = (IMDBMovie)temporaryListItem.AlbumInfoTag;

            if (Util.Utils.IsVideo(movie.VideoFileName) && !PlayListFactory.IsPlayList(movie.VideoFileName))
            {
              movies.Add(movie.VideoFileName);
            }
          }
          
          if (movies.Count == 0)
          {
            movies.Add(movieFileName);
          }

          if (movies.Count > 1)
          {
            movies.Sort();

            // Stacked movies duration
            if (TotalMovieDuration == 0)
            {
              MovieDuration(movies, false);
              StackedMovieFiles = movies;
            }

            // Ask for resume
            if (askForResumeMovie)
            {
              GUIDialogFileStacking dlg =
                (GUIDialogFileStacking) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_FILESTACKING);
              
              if (null != dlg)
              {
                dlg.SetFiles(movies);
                dlg.DoModal(GetID);
                selectedFileIndex = dlg.SelectedFile;
                
                if (selectedFileIndex < 1)
                {
                  return;
                }
              }
            }
          }
          // This can happen if user have one file and it is stackable (filename ends with CDx...)
          else
          {
            MovieDuration(movies, false);
          }
          
          _playlistPlayer.Reset();
          _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
          PlayList playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
          playlist.Clear();
          
          for (int i = 0; i < (int) movies.Count; ++i)
          {
            movieFileName = (string) movies[i];
            PlayListItem itemNew = new PlayListItem();
            itemNew.FileName = movieFileName;
            itemNew.Type = PlayListItem.PlayListItemType.Video;
            playlist.Add(itemNew);
          }

          // play movie...(2 or more files)
          PlayMovieFromPlayList(askForResumeMovie, selectedFileIndex - 1, true);
          return;
        }

        #endregion

        #region StackDisabled or file not stackable

        // play movie...(only 1 file)
        AddFileToDatabase(movieFileName);
        _playlistPlayer.Reset();
        _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
        PlayList newPlayList = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
        newPlayList.Clear();
        PlayListItem NewItem = new PlayListItem();
        NewItem.FileName = movieFileName;
        NewItem.Type = PlayListItem.PlayListItemType.Video;
        newPlayList.Add(NewItem);
        PlayMovieFromPlayList(true, true);

        #endregion
      }
    }

    protected override void OnQueueItem(int itemIndex)
    {
      // add item 2 playlist
      GUIListItem listItem = facadeLayout[itemIndex];

      if (listItem == null)
      {
        return;
      }
      
      if (listItem.IsRemote)
      {
        return;
      }
      
      if (!_virtualDirectory.RequestPin(listItem.Path))
      {
        return;
      }

      if (PlayListFactory.IsPlayList(listItem.Path))
      {
        LoadPlayList(listItem.Path);
        return;
      }

      AddItemToPlayList(listItem);

      //move to next item
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex + 1);
    }

    protected override void AddItemToPlayList(GUIListItem listItem)
    {
      if (listItem.IsFolder)
      {
        // recursive
        if (listItem.Label == "..")
        {
          return;
        }
        
        List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryUnProtectedExt(listItem.Path, true);
        
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
      }
      else
      {
        if (Util.Utils.IsVideo(listItem.Path) && !PlayListFactory.IsPlayList(listItem.Path))
        {
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.FileName = listItem.Path;
          playlistItem.Description = listItem.Label;
          playlistItem.Duration = listItem.Duration;
          playlistItem.Type = PlayListItem.PlayListItemType.Video;
          _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(playlistItem);
        }
      }
    }
    
    protected override void OnInfo(int iItem)
    {
      GUIListItem pItem = facadeLayout.SelectedListItem;
      CurrentSelectedGUIItem = pItem;

      if (pItem == null)
      {
        return;
      }

      if (pItem.IsRemote)
      {
        return;
      }

      if (!_virtualDirectory.RequestPin(pItem.Path))
      {
        return;
      }

      string strFile = pItem.Path;
      string strMovie = pItem.Label;
      bool bFoundFile = true;

      if ((pItem.IsFolder) && (!Util.Utils.IsDVD(pItem.Path)))
      {
        ISelectDVDHandler selectDVDHandler = GetSelectDvdHandler();
        ISelectBDHandler selectBDHandler = GetSelectBDHandler();
				
        if (pItem.Label == ".." || !pItem.IsBdDvdFolder)
        {
          return;
        }

        strFile = selectDVDHandler.GetFolderVideoFile(pItem.Path);

        if (strFile == string.Empty)
        {
          strFile = selectBDHandler.GetFolderVideoFile(pItem.Path);
        }

        if (strFile == string.Empty)
        {
          bFoundFile = false;
          strFile = pItem.Path;
        }
        else if (strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          //DVD folder
          string dvdFolder = strFile.Substring(0, strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase));
          strMovie = Path.GetFileName(dvdFolder);
        }
        else if (strFile.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          //BD folder
          string bdFolder = strFile.Substring(0, strFile.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase));
          strMovie = Path.GetFileName(bdFolder);
        }
        else
        {
          //Movie 
          strMovie = Path.GetFileNameWithoutExtension(strFile);
        }
      }
      // Use DVD label as movie name
      else if (Util.Utils.IsDVD(pItem.Path) && (pItem.DVDLabel != string.Empty))
      {
        if (File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          strFile = pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        if (File.Exists(pItem.Path + @"\BDMV\index.bdmv"))
        {
          strFile = pItem.Path + @"\BDMV\index.bdmv";
        }
        strMovie = pItem.DVDLabel;
      }

      IMDBMovie movieDetails = new IMDBMovie();

      if ((VideoDatabase.GetMovieInfo(strFile, ref movieDetails) == -1) ||
          (movieDetails.IsEmpty))
      {
        IMDBMovie.FetchMovieNfo(pItem.Path, strFile, ref movieDetails);

        if (!movieDetails.IsEmpty)
        {
          VideoDatabase.ImportNfo(movieDetails.File, false, false);
          VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        }
        else
        {
          if (_useOnlyNfoScraper)
          {
            // Notify user that new fanart are downloaded
            GUIDialogNotify dlgNotify =
              (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
            if (null != dlgNotify)
            {
              dlgNotify.SetHeading(GUILocalizeStrings.Get(1020)); //Information
              dlgNotify.SetText(string.Format("Use only nfo import is active and no nfo file found for {0}", pItem.Label));
              dlgNotify.DoModal(GetID);
            }
            return;
          }
          // Check Internet connection
          if (!Win32API.IsConnectedToInternet())
          {
            GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
            dlgOk.SetHeading(257);
            dlgOk.SetLine(1, GUILocalizeStrings.Get(703));
            dlgOk.DoModal(GUIWindowManager.ActiveWindow);
            return;
          }

          // Movie is not in the database
          if (bFoundFile)
          {
            AddFileToDatabase(strFile);
          }

          if (!pItem.IsRemote && Util.Utils.IsFolderDedicatedMovieFolder(pItem.Path))
          {
            movieDetails.SearchString = Path.GetFileName(Path.GetDirectoryName(strMovie));
          }
          else
          {
            if (Util.Utils.IsVideo(strMovie))
            {
              movieDetails.SearchString = Path.GetFileNameWithoutExtension(strMovie);
            }
            else
            {
              movieDetails.SearchString = strMovie;
            }
          }

          movieDetails.File = Path.GetFileName(strFile);

          if (movieDetails.File == string.Empty)
          {
            movieDetails.Path = strFile;
          }
          else
          {
            movieDetails.Path = strFile.Substring(0, strFile.IndexOf(movieDetails.File) - 1);
          }

          Log.Info("GUIVideoFiles: IMDB search: {0}, file:{1}, path:{2}", movieDetails.SearchString, movieDetails.File,
                   movieDetails.Path);

          if (!IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, _isFuzzyMatching, false))
          {
            return;
          }
        }
        // Send global message that movie is refreshed/scanned
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
      // Movie is in the database
      GUIVideoInfo videoInfo = (GUIVideoInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_VIDEO_INFO);
      videoInfo.Movie = movieDetails;
      
      if (pItem.IsFolder)
      {
        videoInfo.FolderForThumbs = pItem.Path;
      }
      else
      {
        videoInfo.FolderForThumbs = string.Empty;
      }
      
      GUIWindowManager.ActivateWindow((int) Window.WINDOW_VIDEO_INFO);

      if (movieDetails != null)
      {
        // Add file name beacuse in the movie.details it's empty -> FanArt on shares helper
        movieDetails.File = Path.GetFileName(strFile);

        // Title suffix for problem with covers and movie with the same name
        //string thumbPath = Util.Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title);
        string titleExt = movieDetails.Title + "{" + movieDetails.ID + "}";
        string thumbPath = Util.Utils.GetCoverArt(Thumbs.MovieTitle, titleExt);

        if (string.IsNullOrEmpty(thumbPath) || !Util.Utils.FileExistsInCache(thumbPath))
        {
          thumbPath = string.Format(@"{0}\{1}", Thumbs.MovieTitle,
                                    Util.Utils.MakeFileName(
                                      Util.Utils.SplitFilename(Path.ChangeExtension(pItem.Path, ".jpg"))));
        }

        if (Util.Utils.FileExistsInCache(thumbPath))
        {
          pItem.RefreshCoverArt();
          pItem.IconImage = thumbPath;
          pItem.IconImageBig = thumbPath;
          string thumbLargePath = Util.Utils.ConvertToLargeCoverArt(thumbPath);
          
          if (Util.Utils.FileExistsInCache(thumbLargePath))
          {
            pItem.ThumbnailImage = thumbLargePath;
          }
          else
          {
            pItem.ThumbnailImage = thumbPath;
          }
        }
      }
    }

    protected override void SelectCurrentItem()
    {
      if (_currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _currentSelectedItem);
      }
      else
      {
        GUIPropertyManager.SetProperty("#watchedcount", "-1");
        GUIPropertyManager.SetProperty("#videoruntime", string.Empty);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeLayout.SelectedListItem;
      int itemNo = facadeLayout.SelectedListItemIndex;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (item == null)
      {
        dlg.AddLocalizedString(868); // Reset virtual directory
      }
      else if (item.IsRemote || (item.IsFolder) && (item.Label == ".."))
      {
        return;
      }
      else
      {
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

        if (!facadeLayout.Focus)
        {
          // Menu button context menuu
          if (!_virtualDirectory.IsRemote(_currentFolder))
          {
            dlg.AddLocalizedString(102); //Scan
            dlg.AddLocalizedString(368); //IMDB
          }
        }
        else
        {
          // DVD & files
          if ((Path.GetFileName(item.Path) != string.Empty) || Util.Utils.IsDVD(item.Path))
          {
            // DVD disc drive
            if (Util.Utils.IsDVD(item.Path))
            {
              if (File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO") ||
                  File.Exists(item.Path + @"\BDMV\index.bdmv"))
              {
                dlg.AddLocalizedString(341); //play
              }
              else
              {
                dlg.AddLocalizedString(926); //Queue
                dlg.AddLocalizedString(102); //Scan
              }

              dlg.AddLocalizedString(368); //IMDB
              dlg.AddLocalizedString(654); //Eject
            }
            // Folder
            else if (item.IsFolder)
            {
              bool useMediaInfo = false;

              if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
              {
                dlg.AddLocalizedString(208); //play
                useMediaInfo = true;
              }

              if (!VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
              {
                // Simple folder
                if (!item.IsBdDvdFolder)
                {
                  dlg.AddLocalizedString(1204); // Play All in selected folder
                  dlg.AddLocalizedString(926); //Queue
                  dlg.AddLocalizedString(102); //Scan 
                  dlg.AddLocalizedString(1280); // Scan using nfo files
                }
                // DVD folder
                else if (item.IsBdDvdFolder)
                {
                  useMediaInfo = true;
                  dlg.AddLocalizedString(208); //play             
                  dlg.AddLocalizedString(926); //Queue
                  dlg.AddLocalizedString(368); //IMDB
                  
                  if (movie != null && !movie.IsEmpty)
                  {
                    if (item.IsPlayed)
                    {
                      {
                        dlg.AddLocalizedString(830); //Reset watched status for DVD folder
                      }
                    }
                    else
                    {
                      dlg.AddLocalizedString(1260); // Set watched status
                    }
                  }
                }
              }

              if (Util.Utils.getDriveType(item.Path) == 5)
              {
                dlg.AddLocalizedString(654); //Eject            
              }

              if (!IsFolderPinProtected(item.Path) && _fileMenuEnabled)
              {
                dlg.AddLocalizedString(500); // FileMenu            
              }

              if (useMediaInfo)
              {
                dlg.AddLocalizedString(1264); //Media info
              }
            }
            else
            {
              dlg.AddLocalizedString(208); //Play
              dlg.AddLocalizedString(926); //Queue
              dlg.AddLocalizedString(368); //IMDB

              if (item.IsPlayed)
              {
                dlg.AddLocalizedString(830); //Reset watched status
              }
              else
              {
                dlg.AddLocalizedString(1260); // Set watched status
              }

              if (!IsFolderPinProtected(item.Path) && !item.IsRemote && _fileMenuEnabled)
              {
                dlg.AddLocalizedString(500); // FileMenu
              }
              dlg.AddLocalizedString(1264); //Media info
            }
          }
          else if (Util.Utils.IsNetwork(item.Path)) // Process network root with drive letter
          {
            dlg.AddLocalizedString(1204); // Play All in selected folder
          }
        }
        if (!_mapSettings.Stack)
        {
          dlg.AddLocalizedString(346); //Stack
        }
        else
        {
          dlg.AddLocalizedString(347); //Unstack
        }
        if (Util.Utils.IsRemovable(item.Path))
        {
          dlg.AddLocalizedString(831);
        }

        dlg.AddLocalizedString(1299); // Refresh current directory
        dlg.AddLocalizedString(1262); // Update grabber scripts
        dlg.AddLocalizedString(1307); // Update internal grabber scripts
        dlg.AddLocalizedString(1263); // Set default grabber
      }

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 368: // IMDB
          OnInfo(itemNo);
          break;

        case 208: // play
          _playClicked = true;
          OnClick(itemNo);
          break;

        case 926: // add to playlist
          OnQueueItem(itemNo);
          break;

        case 136: // show playlist
          GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_PLAYLIST);
          break;

        case 654: // Eject
          if (Util.Utils.getDriveType(item.Path) != 5)
          {
            Util.Utils.EjectCDROM();
          }
          else
          {
            Util.Utils.EjectCDROM(Path.GetPathRoot(item.Path));
          }
          LoadDirectory(string.Empty);
          break;

        case 341: //Play dvd
          OnPlayDVD(item.Path, GetID);
          break;

        case 346: //Stack
          _mapSettings.Stack = true;
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
          break;

        case 347: //Unstack
          _mapSettings.Stack = false;
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
          break;

        case 102: //Scan
          if (facadeLayout.Focus)
          {
            if (item.IsFolder)
            {
              if (item.Label == "..")
              {
                return;
              }
              if (item.IsRemote)
              {
                return;
              }
            }
          }

          if (!_virtualDirectory.RequestPin(item.Path))
          {
            return;
          }

          int currentIndex = facadeLayout.SelectedListItemIndex;
          ArrayList availablePaths = new ArrayList();
          availablePaths.Add(item.Path);
          IMDBFetcher.ScanIMDB(this, availablePaths, _isFuzzyMatching, _scanSkipExisting, _getActors, false);
          // Send global message that movie is refreshed/scanned
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msg);
          LoadDirectory(_currentFolder);
          facadeLayout.SelectedListItemIndex = currentIndex;
          break;

        case 1280: //Scan using nfo files
          if (facadeLayout.Focus)
          {
            if (item.IsFolder)
            {
              if (item.Label == "..")
              {
                return;
              }
              if (item.IsRemote)
              {
                return;
              }
            }
          }

          if (!_virtualDirectory.RequestPin(item.Path))
          {
            return;
          }

          currentIndex = facadeLayout.SelectedListItemIndex;

          ArrayList nfoFiles = new ArrayList();
          GetNfoFiles(item.Path, ref nfoFiles);

          IMDBFetcher fetcher = new IMDBFetcher(this);
          fetcher.FetchNfo(nfoFiles, true, false);
          // Send global message that movie is refreshed/scanned
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msg);
          LoadDirectory(_currentFolder);
          facadeLayout.SelectedListItemIndex = currentIndex;

          break;

        case 830: // Reset watched status
          SetMovieWatchStatus(item.Path, item.IsFolder, false);
          int selectedIndex = facadeLayout.SelectedListItemIndex;
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
          facadeLayout.SelectedListItemIndex = selectedIndex;
          break;

        case 1260: // Set watched status
          SetMovieWatchStatus(item.Path, item.IsFolder, true);
          selectedIndex = facadeLayout.SelectedListItemIndex;
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
          facadeLayout.SelectedListItemIndex = selectedIndex;
          break;

        case 500: // File menu
          {
            ShowFileMenu(false);
          }
          break;

        case 831:
          string message;

          if (!RemovableDriveHelper.EjectDrive(item.Path, out message))
          {
            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            pDlgOK.SetHeading(831);
            pDlgOK.SetLine(1, GUILocalizeStrings.Get(832));
            pDlgOK.SetLine(2, string.Empty);
            pDlgOK.SetLine(3, message);
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
          else
          {
            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            pDlgOK.SetHeading(831);
            pDlgOK.SetLine(1, GUILocalizeStrings.Get(833));
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;

        case 1204: // Play all
          {
            if (!_virtualDirectory.RequestPin(item.Path))
            {
              return;
            }
            OnPlayAll(item.Path);
          }
          break;

        case 1299: // Refresh current directory
          {
            if (facadeLayout.ListLayout.ListItems.Count > 0 && !string.IsNullOrEmpty(_currentFolder))
            {
              facadeLayout.SelectedListItemIndex = 0;
              LoadDirectory(_currentFolder, false);
            }
          }
          break;

        case 868: // Reset V.directory
          {
            ResetShares();

            if (_virtualDirectory.DefaultShare != null && _virtualDirectory.DefaultShare.Path != string.Empty)
            {
              LoadDirectory(_virtualDirectory.DefaultShare.Path, false);
            }
            else
            {
              LoadDirectory(string.Empty, false);
            }
          }
          break;

        case 1262: // Update grabber scripts
          UpdateGrabberScripts(false);
          break;
        case 1307: // Update internal grabber scripts
          UpdateGrabberScripts(true);
          break;
        case 1263: // Set deault grabber script
          SetDefaultGrabber();
          break;
        case 1264: // Get media info (refresh mediainfo and duration)
          if (item != null)
          {
            string file = item.Path;
            SelectDVDHandler sdh = new SelectDVDHandler();
            SelectBDHandler bdh = new SelectBDHandler();

            if (sdh.IsDvdDirectory(item.Path))
            {
              if (File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
              {
                file = file + @"\VIDEO_TS\VIDEO_TS.IFO";
              }
            }

            if (bdh.IsBDDirectory(item.Path))
            {
              if (File.Exists(item.Path + @"\BDMV\INDEX.BDMV"))
              {
                file = file + @"\BDMV\INDEX.BDMV";
              }
            }

            ArrayList files = new ArrayList();
            files = AddFileToDatabase(file);
            MovieDuration(files, true);
            int movieId = VideoDatabase.GetMovieId(file);
            IMDBMovie mInfo = new IMDBMovie();
            mInfo.SetMediaInfoProperties(file, true);
            mInfo.SetDurationProperty(movieId);
            IMDBMovie.SetMovieData(item);
            SelectCurrentItem();
          }
          break;
      }
    }

    public override void Process()
    {
      if ((_resetSMSsearch == true) && (_resetSMSsearchDelay.Subtract(DateTime.Now).Seconds < -2))
      {
        _resetSMSsearchDelay = DateTime.Now;
        _resetSMSsearch = true;
        facadeLayout.EnableSMSsearch = _oldStateSMSsearch;
      }

      base.Process();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Total video duration in seconds (single or multiple -> stacked file(s))
    /// Also sets duration into videodatabase (movie table) -> full lenght with stacked parts
    /// Parameter refresh force duration update even if data is in the videodatabase
    /// </summary>
    /// <param name="files"></param>
    /// <param name="refresh"></param>
    /// <returns></returns>
    public static int MovieDuration(ArrayList files, bool refresh)
    {
      TotalMovieDuration = 0;

      if (files == null || files.Count == 0)
      {
        return TotalMovieDuration;
      }

      try
      {
        foreach (string file in files)
        {
          int fileID = VideoDatabase.GetFileId(file);
          int tempDuration = VideoDatabase.GetVideoDuration(fileID);

          if (tempDuration > 0 && !refresh)
          {
            TotalMovieDuration += tempDuration;
          }
          else
          {
            MediaInfoWrapper mInfo = new MediaInfoWrapper(file);

            if (fileID > -1)
            {
              // Set video file duration
              VideoDatabase.SetVideoDuration(fileID, mInfo.VideoDuration / 1000);
              TotalMovieDuration += mInfo.VideoDuration / 1000;
            }
          }
        }
        // Set movie duration
        VideoDatabase.SetMovieDuration(VideoDatabase.GetMovieId(files[0].ToString()), TotalMovieDuration);
      }
      catch (Exception ex)
      {
        Log.Error("GUIVideoFiles: Movie duration exception: {0}", ex.Message);
      }

      return TotalMovieDuration;
    }

    public bool CheckMovie(string movieFileName)
    {
      if (!VideoDatabase.HasMovieInfo(movieFileName))
      {
        return true;
      }

      IMDBMovie movieDetails = new IMDBMovie();
      int idMovie = VideoDatabase.GetMovieInfo(movieFileName, ref movieDetails);

      if (idMovie < 0)
      {
        return true;
      }

      return CheckMovie(idMovie);
    }

    public static bool CheckMovie(int idMovie)
    {
      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);

      if (!Util.Utils.IsDVD(movieDetails.Path))
      {
        return true;
      }

      string cdlabel = string.Empty;
      cdlabel = Util.Utils.GetDriveSerial(movieDetails.Path);

      if (cdlabel.Equals(movieDetails.CDLabel))
      {
        return true;
      }

      GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (dlg == null)
      {
        return true;
      }

      while (true)
      {
        dlg.SetHeading(428);
        dlg.SetLine(1, 429);
        dlg.SetLine(2, movieDetails.DVDLabel);
        dlg.SetLine(3, movieDetails.Title);
        dlg.SetYesLabel(GUILocalizeStrings.Get(186)); //OK
        dlg.SetNoLabel(GUILocalizeStrings.Get(222)); //Cancel
        dlg.SetDefaultToYes(true);
        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.IsConfirmed)
        {
          if (movieDetails.CDLabel.StartsWith("nolabel"))
          {
            ArrayList movies = new ArrayList();
            VideoDatabase.GetFilesForMovie(idMovie, ref movies);
            if (File.Exists( /*movieDetails.Path+movieDetails.File*/(string)movies[0]))
            {
              cdlabel = Util.Utils.GetDriveSerial(movieDetails.Path);
              VideoDatabase.UpdateCDLabel(movieDetails, cdlabel);
              movieDetails.CDLabel = cdlabel;
              return true;
            }
          }
          else
          {
            cdlabel = Util.Utils.GetDriveSerial(movieDetails.Path);

            if (cdlabel.Equals(movieDetails.CDLabel))
            {
              return true;
            }
          }
        }
        else
        {
          break;
        }
      }
      return false;
    }

    public static void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);

      if (null == keyboard)
      {
        return;
      }
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      strLine = string.Empty;

      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    [Obsolete("This method is obsolete; use method PlayMovieFromPlayList(bool askForResumeMovie, bool requestPin) instead.")]
    public static void PlayMovieFromPlayList(bool askForResumeMovie)
    {
      PlayMovieFromPlayList(askForResumeMovie, -1, false);
    }

    public static void PlayMovieFromPlayList(bool askForResumeMovie, bool requestPin)
    {
      PlayMovieFromPlayList(askForResumeMovie, -1, requestPin);
    }

    public static void PlayMovieFromPlayList(bool askForResumeMovie, int iMovieIndex, bool requestPin)
    {
      string filename;
      if (iMovieIndex == -1)
      {
        filename = _playlistPlayer.GetNext();
      }
      else
      {
        filename = _playlistPlayer.Get(iMovieIndex);
      }

      // If the file is an image file, it should be mounted before playing
      bool isImage = false;
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(filename)))
      {
        if (!MountImageFile(GUIWindowManager.ActiveWindow, filename, requestPin))
          return;
        isImage = true;
      }

      // Convert BD ISO filenames for to BD index file (...BDMV\index.bdmv)
      if (isImage)
      {
        // Covert filename
        if (Util.Utils.IsBDImage(filename, ref filename))
        {
          // Change also playlist filename
          int index = iMovieIndex;

          if (iMovieIndex == -1)
            index = 0;

          _playlistPlayer.GetPlaylist(_playlistPlayer.CurrentPlaylistType)[index].FileName = filename;
          isImage = false;
        }
      }

      int timeMovieStopped = 0;
      byte[] resumeData = null;
      

      // Skip resume for external player and BluRays (not implemented yet in BDLIB)
      if (!CheckExternalPlayer(filename, isImage) && !filename.ToUpperInvariant().Contains("INDEX.BDMV"))
      {
        IMDBMovie movieDetails = new IMDBMovie();
        VideoDatabase.GetMovieInfo(filename, ref movieDetails);
        int idFile = VideoDatabase.GetFileId(filename);
        int idMovie = VideoDatabase.GetMovieId(filename);

        if ((idMovie >= 0) && (idFile >= 0))
        {
          timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
          if (timeMovieStopped > 0)
          {
            string title = Path.GetFileName(filename);
            VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
            if (movieDetails.Title != string.Empty)
            {
              title = movieDetails.Title;
            }
            if (askForResumeMovie)
            {
              GUIResumeDialog.Result result =
                GUIResumeDialog.ShowResumeDialog(title, timeMovieStopped,
                                                 GUIResumeDialog.MediaType.Video);

              if (result == GUIResumeDialog.Result.Abort)
                return;

              if (result == GUIResumeDialog.Result.PlayFromBeginning)
                timeMovieStopped = 0;
            }
          }
        }
      }

      if (g_Player.Playing && !g_Player.IsDVD)
        g_Player.Stop();

      string currentFile = g_Player.CurrentFile;
      if (Util.Utils.IsISOImage(currentFile))
      {
        if (!String.IsNullOrEmpty(Util.DaemonTools.GetVirtualDrive()) &&
            g_Player.IsDvdDirectory(Util.DaemonTools.GetVirtualDrive()))
          Util.DaemonTools.UnMount();
      }

      if (iMovieIndex == -1)
      {
        _playlistPlayer.PlayNext();
      }
      else
      {
        _playlistPlayer.Play(iMovieIndex);
      }

      if (g_Player.Playing && timeMovieStopped > 0)
      {
        if (g_Player.IsDVD)
        {
          g_Player.Player.SetResumeState(resumeData);
        }
        else
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
          msg.Param1 = timeMovieStopped;
          GUIGraphicsContext.SendMessage(msg);
        }
      }
    }

    [Obsolete("This method is obsolete; use method MountImageFile(int WindowID, string file) instead.")]
    public static bool MountImageFile(int windowID, string file)
    {
      return MountImageFile(windowID, file, false);
    }

    public static bool MountImageFile(int windowID, string file, bool requestPin)
    {
      Log.Debug("GUIVideoFiles: MountImageFile");
      if (!DaemonTools.IsMounted(file))
      {
        if (_askBeforePlayingDVDImage)
        {
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);

          if (dlgYesNo != null)
          {
            dlgYesNo.SetHeading(713);
            dlgYesNo.SetLine(1, 531);
            dlgYesNo.DoModal(windowID);
            if (!dlgYesNo.IsConfirmed)
            {
              return false;
            }
          }
        }

        List<GUIListItem> items = new List<GUIListItem>();

        if (requestPin)
        {
          items = _virtualDirectory.GetDirectoryExt(file);
        }
        else
        {
          items = _virtualDirectory.GetDirectoryUnProtectedExt(file, true);
        }

        if (items.Count == 1 && file != string.Empty)
        {
          return false; // protected share, with wrong pincode
        }
      }
      return DaemonTools.IsMounted(file);
    }

    /// <summary>
    /// Returns true if the specified window belongs to the my videos plugin
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: belongs to the my videos plugin
    /// false: does not belong to the my videos plugin</returns>
    public static bool IsVideoWindow(int windowId)
    {
      if (windowId == (int)Window.WINDOW_DVD)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_FULLSCREEN_VIDEO)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_ARTIST_INFO)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_INFO)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_PLAYLIST)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_TITLE)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEOS)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Returns true if the specified window should maintain virtual directory
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: if the specified window should maintain virtual directory
    /// false: if the specified window should not maintain virtual directory</returns>
    public static bool KeepVirtualDirectory(int windowId)
    {
      if (windowId == (int)Window.WINDOW_DVD)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_FULLSCREEN_VIDEO)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_ARTIST_INFO)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_INFO)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEO_PLAYLIST)
      {
        return true;
      }

      if (windowId == (int)Window.WINDOW_VIDEOS)
      {
        return true;
      }

      return false;
    }

    public static void Reset()
    {
      Log.Debug("GUIVideoFiles: Resetting virtual directory");
      _virtualDirectory.Reset();
    }

    [Obsolete("This method is obsolete; use method PlayMovie(int idMovie, bool requestPin) instead.")]
    public static void PlayMovie(int idMovie)
    {
      PlayMovie(idMovie, false);
    }

    public static void PlayMovie(int idMovie, bool requestPin)
    {
      int selectedFileIndex = 1;

      if (IsStacked)
      {
        selectedFileIndex = 0;
      }

      ArrayList movieFiles = new ArrayList();
      VideoDatabase.GetFilesForMovie(idMovie, ref movieFiles);

      if (movieFiles.Count <= 0 || !CheckMovie(idMovie))
      {
        return;
      }

      bool askForResumeMovie = true;
      int movieDuration = 0;

      List<GUIListItem> items = new List<GUIListItem>();

      foreach (string file in movieFiles)
      {
        FileInformation fi = new FileInformation();
        GUIListItem item = new GUIListItem(Util.Utils.GetFilename(file), "", file, false, fi);
        items.Add(item);
      }

      if (items.Count <= 0)
      {
        return;
      }

      if (requestPin)
      {
        string strDir = Path.GetDirectoryName(items[0].Path);

        if (strDir != null && strDir.EndsWith(@"\"))
        {
          strDir = strDir.Substring(0, strDir.Length - 1);
        }

        if (strDir == null || strDir.Length > 254)
        {
          Log.Warn("GUIVideoFiles.PlayTitleMovie: Received a path which contains too many chars");
          return;
        }

        int iPincodeCorrect;
        if (_virtualDirectory.IsProtectedShare(strDir, out iPincodeCorrect))
        {
          #region Pin protected

          bool retry = true;
          {
            while (retry)
            {
              //no, then ask user to enter the pincode
              GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
              GUIWindowManager.SendMessage(msgGetPassword);
              int iPincode = -1;

              try
              {
                iPincode = Int32.Parse(msgGetPassword.Label);
              }
              catch (Exception) { }

              if (iPincode != iPincodeCorrect)
              {
                GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0,
                                                             0);
                GUIWindowManager.SendMessage(msgWrongPassword);

                if (!(bool)msgWrongPassword.Object)
                {
                  return;
                }
              }
              else
              {
                retry = false;
              }
            }
          }

          #endregion
        }
      }

      //check if we can resume 1 of those movies
      if (items.Count > 1)
      {
        bool asked = false;

        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];

          if (!asked)
          {
            selectedFileIndex++;
          }

          IMDBMovie movieDetails = new IMDBMovie();
          int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);

          if ((idMovie >= 0) && (idFile >= 0))
          {
            VideoDatabase.GetMovieInfo((string)movieFiles[0], ref movieDetails);
            string title = Path.GetFileName((string)movieFiles[0]);

            if ((VirtualDirectory.IsValidExtension((string)movieFiles[0], Util.Utils.VideoExtensions, false)))
            {
              Util.Utils.RemoveStackEndings(ref title);
            }

            if (movieDetails.Title != string.Empty)
            {
              title = movieDetails.Title;
            }

            int timeMovieStopped = VideoDatabase.GetMovieStopTime(idFile);

            if (timeMovieStopped > 0)
            {
              if (!asked)
              {
                asked = true;

                GUIResumeDialog.Result result =
                  GUIResumeDialog.ShowResumeDialog(title, movieDuration + timeMovieStopped,
                                                   GUIResumeDialog.MediaType.Video);

                if (result == GUIResumeDialog.Result.Abort)
                  return;

                if (result == GUIResumeDialog.Result.PlayFromBeginning)
                {
                  VideoDatabase.DeleteMovieStopTime(idFile);
                }
                else
                {
                  askForResumeMovie = false;
                }
              }
            }
            // Total movie duration
            movieDuration += VideoDatabase.GetVideoDuration(idFile);
          }
        }

        if (askForResumeMovie)
        {
          GUIDialogFileStacking dlg =
            (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_FILESTACKING);

          if (null != dlg)
          {
            dlg.SetFiles(movieFiles);
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            selectedFileIndex = dlg.SelectedFile;
            if (selectedFileIndex < 1)
            {
              return;
            }
          }
        }
      }

      _playlistPlayer.Reset();
      _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
      PlayList playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
      playlist.Clear();

      for (int i = selectedFileIndex - 1; i < movieFiles.Count; ++i)
      {
        string movieFileName = (string)movieFiles[i];
        PlayListItem newitem = new PlayListItem();
        newitem.FileName = movieFileName;
        newitem.Type = PlayListItem.PlayListItemType.Video;
        playlist.Add(newitem);
      }

      // play movie...
      PlayMovieFromPlayList(askForResumeMovie, requestPin);
    }

    public static bool IsMovieFolder(string path)
    {
      ISelectDVDHandler selectDvdHandler = GetSelectDvdHandler();
      ISelectBDHandler selectBdHandler = GetSelectBDHandler();

      if (selectBdHandler.IsBDDirectory(path) || selectDvdHandler.IsDvdDirectory(path))
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get/Set BDHandler interface from/to registered services.
    /// </summary>
    /// <returns>BDHandler interface</returns>
    public static ISelectBDHandler GetSelectBDHandler()
    {
      ISelectBDHandler selectBDHandler;
      if (GlobalServiceProvider.IsRegistered<ISelectBDHandler>())
      {
        selectBDHandler = GlobalServiceProvider.Get<ISelectBDHandler>();
      }
      else
      {
        selectBDHandler = new SelectBDHandler();
        GlobalServiceProvider.Add<ISelectBDHandler>(selectBDHandler);
      }
      return selectBDHandler;
    }

    /// <summary>
    /// Get/Set DVDHandler interface from/to registered services.
    /// </summary>
    /// <returns>DVDHandler interface</returns>
    public static ISelectDVDHandler GetSelectDvdHandler()
    {
      ISelectDVDHandler selectDVDHandler;

      if (GlobalServiceProvider.IsRegistered<ISelectDVDHandler>())
      {
        selectDVDHandler = GlobalServiceProvider.Get<ISelectDVDHandler>();
      }
      else
      {
        selectDVDHandler = new SelectDVDHandler();
        GlobalServiceProvider.Add<ISelectDVDHandler>(selectDVDHandler);
      }

      return selectDVDHandler;
    }

    public static void UpdateGrabberScripts(bool internalScript)
    {
      // Check Internet connection

      if (!Win32API.IsConnectedToInternet())
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(257);
        dlgOk.SetLine(1, GUILocalizeStrings.Get(703));
        dlgOk.DoModal(GUIWindowManager.ActiveWindow);
        return;
      }

      // Initialize progress bar
      GUIDialogProgress progressDialog =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      progressDialog.Reset();
      progressDialog.SetHeading("Updating MovieInfo grabber scripts...");
      progressDialog.ShowProgressBar(true);
      progressDialog.SetLine(1, "Downloading the index file...");
      progressDialog.SetLine(2, "Downloading...");
      progressDialog.SetPercentage(50);
      progressDialog.StartModal(GUIWindowManager.ActiveWindow);

      if (internalScript)
      {
        string parserIndexFile = Config.GetFile(Config.Dir.Config, "scripts\\VDBParserStrings.xml");
        string parserIndexUrl = @"http://install.team-mediaportal.com/MP1/VDBParserStrings.xml";
        string internalGrabberScriptFile = Config.GetFile(Config.Dir.Config, "scripts\\InternalActorMoviesGrabber.csscript");
        string internalGrabberScriptUrl = @"http://install.team-mediaportal.com/MP1/InternalGrabber/InternalActorMoviesGrabber.csscript";

        // VDB parser update
        progressDialog.SetHeading("Updating VDBparser file......");
        progressDialog.ShowProgressBar(true);
        progressDialog.SetLine(1, "Downloading VDBparser file...");
        progressDialog.SetLine(2, "Downloading...");
        progressDialog.SetPercentage(75);
        progressDialog.StartModal(GUIWindowManager.ActiveWindow);

        if (DownloadFile(parserIndexFile, parserIndexUrl) == false)
        {
          progressDialog.Close();
          return;
        }

        // Internal grabber script update
        progressDialog.SetHeading("Updating InternalGrabberScript file......");
        progressDialog.ShowProgressBar(true);
        progressDialog.SetLine(1, "Downloading InternalGrabberScript file...");
        progressDialog.SetLine(2, "Downloading...");
        progressDialog.SetPercentage(100);
        progressDialog.StartModal(GUIWindowManager.ActiveWindow);

        if (DownloadFile(internalGrabberScriptFile, internalGrabberScriptUrl) == false)
        {
          progressDialog.Close();
          return;
        }

        _internalGrabber.LoadScript();
        progressDialog.Close();
      }

      if (DownloadFile(_grabberIndexFile, _grabberIndexUrl) == false)
      {
        progressDialog.Close();
        return;
      }
      
      
      // read index file
      if (!File.Exists(_grabberIndexFile))
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(257);
        dlgOk.SetLine(1, GUILocalizeStrings.Get(1261)); // No grabber index found
        dlgOk.DoModal(GUIWindowManager.ActiveWindow);
        progressDialog.Close();
        return;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(_grabberIndexFile);
      XmlNodeList sectionNodes = doc.SelectNodes("MovieInfoGrabber/grabber");

      // download all grabbers
      int percent = 0;

      if (sectionNodes != null)
      {
        for (int i = 0; i < sectionNodes.Count; i++)
        {
          if (progressDialog.IsCanceled)
          {
            break;
          }

          string url = sectionNodes[i].Attributes["url"].Value;
          string id = Path.GetFileName(url);
          progressDialog.SetLine(1, "Downloading grabber: " + id);
          progressDialog.SetLine(2, "Processing grabbers...");
          progressDialog.SetPercentage(percent);
          percent += 100 / (sectionNodes.Count - 1);
          progressDialog.Progress();

          if (DownloadFile(IMDB.ScriptDirectory + @"\" + id, url) == false)
          {
            progressDialog.Close();
            return;
          }
        }
      }
      progressDialog.Close();
    }

    private static bool DownloadFile(string filepath, string url)
    {
      string grabberTempFile = Path.GetTempFileName();

      //Application.DoEvents();
      try
      {
        if (File.Exists(grabberTempFile))
        {
          File.Delete(grabberTempFile);
        }

        //Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // request.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception)
        {
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          //Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream, Encoding.Default))
            {
              using (TextWriter tout = File.CreateText(grabberTempFile))
              {
                while (true)
                {
                  string line = tin.ReadLine();

                  if (line == null)
                  {
                    break;
                  }

                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        File.Delete(filepath);
        File.Move(grabberTempFile, filepath);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("EXCEPTION in DownloadFile | {0}\r\n{1}", ex.Message, ex.Source);
        return false;
      }
    }

    public static void SetDefaultGrabber()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(1263); // menu

      // read index file
      if (!File.Exists(_grabberIndexFile))
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(257);
        dlgOk.SetLine(1, GUILocalizeStrings.Get(1261)); // No grabber index found
        dlgOk.DoModal(GUIWindowManager.ActiveWindow);
        return;
      }

      _grabberList = new Dictionary<string, IIMDBScriptGrabber>();

      Directory.CreateDirectory(IMDB.ScriptDirectory);
      DirectoryInfo di = new DirectoryInfo(IMDB.ScriptDirectory);

      FileInfo[] fileList = di.GetFiles("*.csscript", SearchOption.AllDirectories);
      GUIWaitCursor.Show();

      foreach (FileInfo f in fileList)
      {
        try
        {
          AsmHelper script = new AsmHelper(CSScript.Load(f.FullName, null, false));
          IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");

          _grabberList.Add(Path.GetFileNameWithoutExtension(f.FullName), grabber);
        }
        catch (Exception ex)
        {
          Log.Error("Script grabber error file: {0}, message : {1}", f.FullName, ex.Message);
        }
      }

      string defaultDatabase = string.Empty;
      int defaultIndex = 0;
      int dbNumber;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        defaultDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + 0, "IMDB");
        dbNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
      }

      foreach (KeyValuePair<string, IIMDBScriptGrabber> grabber in _grabberList)
      {
        dlg.Add(grabber.Value.GetName() + " - " + grabber.Value.GetLanguage());

        if (defaultDatabase == grabber.Key)
        {
          dlg.SelectedLabel = defaultIndex;
        }
        else
        {
          defaultIndex++;
        }
      }

      GUIWaitCursor.Hide();

      dlg.DoModal(GUIWindowManager.ActiveWindow);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      using (Profile.Settings xmlwriter = new MPSettings())
      {
        KeyValuePair<string, IIMDBScriptGrabber> grabber = _grabberList.ElementAt(dlg.SelectedLabel);


        if (grabber.Key != "IMDB")
        {
          if (dbNumber == 0)
          {
            dbNumber = 1;
          }
          xmlwriter.SetValue("moviedatabase", "number", dbNumber);
          xmlwriter.SetValue("moviedatabase", "database" + 0, grabber.Key);
          xmlwriter.SetValue("moviedatabase", "title" + 0, grabber.Value.GetName());
          xmlwriter.SetValue("moviedatabase", "language" + 0, grabber.Value.GetLanguage());
          xmlwriter.SetValue("moviedatabase", "limit" + 0, 25);
        }
        else
        {
          for (int i = 0; i < 4; i++)
          {
            xmlwriter.SetValue("moviedatabase", "number", 0);
            xmlwriter.RemoveEntry("moviedatabase", "database" + i);
            xmlwriter.RemoveEntry("moviedatabase", "title" + i);
            xmlwriter.RemoveEntry("moviedatabase", "language" + i);
            xmlwriter.RemoveEntry("moviedatabase", "limit" + i);
          }
        }
      }
    }
    
    public static void ResetShares()
    {
      _virtualDirectory.Reset();
      _virtualDirectory.DefaultShare = null;
      _virtualDirectory.LoadSettings("movies");

      if (_virtualDirectory.DefaultShare != null)
      {
        int pincode;
        bool folderPinProtected = _virtualDirectory.IsProtectedShare(_virtualDirectory.DefaultShare.Path, out pincode);

        if (folderPinProtected)
        {
          _currentFolder = string.Empty;
        }
        else
        {
          _currentFolder = _virtualDirectory.DefaultShare.Path;
        }
      }
    }

    public static void ResetExtensions(ArrayList extensions)
    {
      _virtualDirectory.SetExtensions(extensions);
    }

    public static IMDB.InternalMovieInfoScraper InternalGrabber
    {
      get { return _internalGrabber; }
    }

    #endregion

    #region Private methods

    private void LoadDirectory(string newFolderName, bool useCache)
    {
      if (newFolderName == null)
      {
        Log.Warn("GUIVideoFiles::LoadDirectory called with invalid argument. newFolderName is null!");
        return;
      }

      if (facadeLayout == null)
      {
        return;
      }

      GUIWaitCursor.Show();

      if (newFolderName != _currentFolder && _mapSettings != null)
      {
        SaveFolderSettings(_currentFolder);
      }

      if (newFolderName != _currentFolder || _mapSettings == null)
      {
        LoadFolderSettings(newFolderName);
      }
      // Image file is not listed as a valid movie so we need to handle it 
      // as a folder and enable browsing for it
      if (VirtualDirectory.IsImageFile(Path.GetExtension(newFolderName)))
      {
        if (!MountImageFile(GetID, newFolderName, true))
        {
          return;
        }

        _currentFolder = DaemonTools.GetVirtualDrive();
      }
      else
      {
        _currentFolder = newFolderName;
      }

      IMDBMovie.ResetMovieProperties(); // Clear skin properties values
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      List<GUIListItem> itemlist = null;
      ISelectDVDHandler selectDvdHandler = GetSelectDvdHandler();
      ISelectBDHandler selectBDHandler = GetSelectBDHandler();

      //Tweak to boost performance when starting/stopping playback
      //For further details see comment in Core\Util\VirtualDirectory.cs
      if (useCache && _cachedDir == _currentFolder && _cachedItems != null && _cachedItems.Count > 0)
      {
        itemlist = _cachedItems;
        
        foreach (GUIListItem item in itemlist)
        {
          // set label 1 & 2
          SetLabel(item);

          // Set watched status and label 3
          if (item.IsBdDvdFolder || Util.Utils.IsVideo(item.Path))
          {
            // Check db for watched status for played movie or changed status in movie info window
            string file = item.Path;

            if (item.IsFolder)
            {
              file = selectDvdHandler.GetFolderVideoFile(item.Path);

              if (file == string.Empty)
              {
                file = selectBDHandler.GetFolderVideoFile(item.Path);
              }
            }

            int percentWatched = 0;
            int timesWatched = 0;
            int movieId = VideoDatabase.GetMovieId(file);
            bool played = VideoDatabase.GetmovieWatchedStatus(movieId, out percentWatched, out timesWatched);

            if (_markWatchedFiles)
            {
              item.IsPlayed = played;
            }
            
            if (item.IsBdDvdFolder)
            {
              if (selectDvdHandler.IsDvdDirectory(item.Path))
              {
                item.Label2 = MediaTypes.DVD.ToString();
                item.Label3 = string.Empty;
              }
              else
              {
                item.Label2 = MediaTypes.BD.ToString();
                item.Label3 = string.Empty;
              }
            }
            else if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
            {
              item.Label3 = MediaTypes.ISO.ToString();
            }
            else
            {
              item.Label3 = percentWatched + "% #" + timesWatched;
            }
          }
          
          //Do NOT add OnItemSelected event handler here, because its still there...
          facadeLayout.Add(item);
        }
      } // End Cached items
      else
      {
        // here we get ALL files in every subdir, look for folderthumbs, defaultthumbs, etc
        itemlist = _virtualDirectory.GetDirectoryExt(_currentFolder);

        if (_mapSettings != null && _mapSettings.Stack)
        {
          Dictionary<string, List<GUIListItem>> stackings = new Dictionary<string, List<GUIListItem>>(itemlist.Count);

          for (int i = 0; i < itemlist.Count; ++i)
          {
            GUIListItem item1 = itemlist[i];
            string cleanFilename = item1.Label;
            Util.Utils.RemoveStackEndings(ref cleanFilename);
            List<GUIListItem> innerList;

            if (stackings.TryGetValue(cleanFilename, out innerList))
            {
              for (int j = 0; j < innerList.Count; j++)
              {
                GUIListItem item2 = innerList[j];

                if (((!item1.IsFolder || !item2.IsFolder) || (item1.IsFolder && IsMovieFolder(item1.Path) || item2.IsFolder && IsMovieFolder(item2.Path)))
                    && (!item1.IsRemote && !item2.IsRemote)
                    && Util.Utils.ShouldStack(item1.Path, item2.Path))
                {
                  if (String.Compare(item1.Path, item2.Path, StringComparison.OrdinalIgnoreCase) > 0)
                  {
                    item2.FileInfo.Length += item1.FileInfo.Length;
                  }
                  else
                  {
                    // keep item1, it's path is lexicographically before item2 path
                    item1.FileInfo.Length += item2.FileInfo.Length;
                    innerList[j] = item1;
                  }
                  item1 = null;
                  break;
                }
              }

              if (item1 != null) // not stackable
              {
                innerList.Add(item1);
              }
            }
            else
            {
              innerList = new List<GUIListItem> { item1 };
              stackings.Add(cleanFilename, innerList);
            }
          }

          foreach (KeyValuePair<string, List<GUIListItem>> pair in stackings)
          {
            List<GUIListItem> innerList = pair.Value;
            
            for (int i = 0; i < innerList.Count; i++)
            {
              GUIListItem item = innerList[i];
              bool isMovieFolder = IsMovieFolder(item.Path);

              if (VirtualDirectory.IsValidExtension(item.Path, Util.Utils.VideoExtensions, false) || isMovieFolder)
              {
                item.Label = pair.Key;
              }

              SetLabel(item);
              // Check db for watched status for played movie or changed status in movie info window
              string file = item.Path;
              
              if (!item.IsFolder || isMovieFolder)
              {
                // Special folders (DVD/Blu-Ray)
                if (isMovieFolder)
                {
                  item.IsBdDvdFolder = true;
                  file = selectDvdHandler.GetFolderVideoFile(item.Path);

                  if (file == string.Empty)
                  {
                    file = selectBDHandler.GetFolderVideoFile(item.Path);
                  }
                }

                int percentWatched = 0;
                int timesWatched = 0;
                int movieId = VideoDatabase.GetMovieId(file);
                bool played = VideoDatabase.GetmovieWatchedStatus(movieId, out percentWatched, out timesWatched);

                if (_markWatchedFiles)
                {
                  item.IsPlayed = played;
                }

                if (item.IsBdDvdFolder)
                {
                  if (selectDvdHandler.IsDvdDirectory(item.Path))
                  {
                    item.Label3 = string.Empty;
                    item.Label2 = MediaTypes.DVD.ToString();
                  }
                  else
                  {
                    item.Label3 = string.Empty;
                    item.Label2 = MediaTypes.BD.ToString();
                  }
                }
                else if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
                {
                  item.Label3 = MediaTypes.ISO.ToString();
                }
                else
                {
                  item.Label3 = percentWatched + "% #" + timesWatched;
                }
              }

              item.OnItemSelected += item_OnItemSelected;
              facadeLayout.Add(item);
            }
          }
          itemlist = facadeLayout.ListLayout.ListItems;
        }
        else
        {
          foreach (GUIListItem item in itemlist)
          {
            SetLabel(item);

            // Check db for watched status for played movie or changed status in movie info window
            string file = item.Path;
            bool isMovieFolder = IsMovieFolder(item.Path);

            if (!item.IsFolder || isMovieFolder)
            {
              // Special folders (DVD/BluRay)
              if (isMovieFolder)
              {
                item.IsBdDvdFolder = true;
                file = selectDvdHandler.GetFolderVideoFile(item.Path);

                if (file == string.Empty)
                {
                  file = selectBDHandler.GetFolderVideoFile(item.Path);
                }
              }

              int percentWatched = 0;
              int timesWatched = 0;
              int movieId = VideoDatabase.GetMovieId(file);
              bool played = VideoDatabase.GetmovieWatchedStatus(movieId, out percentWatched, out timesWatched);

              if (_markWatchedFiles)
              {
                item.IsPlayed = played;
              }

              if (item.IsBdDvdFolder)
              {
                if (selectDvdHandler.IsDvdDirectory(item.Path))
                {
                  item.Label3 = MediaTypes.DVD.ToString();
                }
                else
                {
                  item.Label3 = MediaTypes.BD.ToString();
                }
              }
              else if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
              {
                item.Label3 = MediaTypes.ISO.ToString();
              }
              else
              {
                item.Label3 = percentWatched + "% #" + timesWatched;
              }
            }

            item.OnItemSelected += item_OnItemSelected;
            facadeLayout.Add(item);
          }
        }

        _cachedItems = itemlist;
        _cachedDir = _currentFolder;
      } // End non cache items

      facadeLayout.Sort(new VideoSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
      
      // Update thumbs for all items (cached also - > cover can be changed in VideoInfoScreen and if we do
      // not update cover, old one will be visible)
      SetImdbThumbs(itemlist, selectDvdHandler);
      
      int selectedIndex = -1;
      Int32.TryParse(_history.Get(_currentFolder), out selectedIndex);
      int maxIndex = itemlist.Count - 1;

      if (maxIndex >= selectedIndex)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedIndex);
      }

      int totalItems = itemlist.Count;

      if (totalItems > 0)
      {
        GUIListItem rootItem = itemlist[0];
        if (rootItem.Label == "..")
        {
          totalItems--;
        }
      }
      else
      {
        if (_resetCount == 0)
        {
          _resetCount++;
          ResetShares();

          if (_virtualDirectory.DefaultShare != null && _virtualDirectory.DefaultShare.Path != string.Empty)
          {
            LoadDirectory(_virtualDirectory.DefaultShare.Path, false);
          }
          else
          {
            LoadDirectory(string.Empty, false);
          }
          GUIWaitCursor.Hide();
          return;
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItems));

      // Reload thumbs if previous load thumb thread aborted (only when cache is active)
      // If thumb thread was aborted and on next loaddirectory with cache "on", some thumbs
      // can be missing and wan't be loaded, so here we reload them again.
      if (_setThumbsThreadAborted)
      {
        SetImdbThumbs(itemlist, selectDvdHandler);
      }

      GUIWaitCursor.Hide();
    }

    private void LoadFolderSettings(string folderName)
    {
      if (folderName == string.Empty)
      {
        folderName = "root";
      }
      object o;
      FolderSettings.GetFolderSetting(folderName, "VideoFiles", typeof(MapSettings), out o);
      if (o != null)
      {
        _mapSettings = o as MapSettings;

        if (_mapSettings == null)
        {
          _mapSettings = new MapSettings();
        }

        CurrentSortAsc = _mapSettings.SortAscending;
        CurrentSortMethod = (VideoSort.SortMethod)_mapSettings.SortBy;
        currentLayout = (Layout)_mapSettings.ViewAs;
      }
      else
      {
        Share share = _virtualDirectory.GetShare(folderName);

        if (share != null)
        {
          if (_mapSettings == null)
          {
            _mapSettings = new MapSettings();
          }
          CurrentSortAsc = _mapSettings.SortAscending;
          CurrentSortMethod = (VideoSort.SortMethod)_mapSettings.SortBy;
          currentLayout = (Layout)share.DefaultLayout;
          CurrentLayout = (Layout)share.DefaultLayout;
        }
      }

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        if (xmlreader.GetValueAsBool("movies", "rememberlastfolder", false))
        {
          xmlreader.SetValue("movies", "lastfolder", folderName);
        }
      }

      SwitchLayout();
      UpdateButtonStates();
    }

    private void SaveFolderSettings(string folderName)
    {
      if (folderName == string.Empty)
      {
        folderName = "root";
      }
      FolderSettings.AddFolderSetting(folderName, "VideoFiles", typeof(MapSettings), _mapSettings);
    }

    private void ShowFileMenu(bool preselectDelete)
    {
      // get pincode
      if (_fileMenuPinCode != string.Empty)
      {
        string userCode = string.Empty;
        if (GetUserPasswordString(ref userCode) && userCode == _fileMenuPinCode)
        {
          OnShowFileMenu(preselectDelete);
        }
      }
      else
      {
        OnShowFileMenu(preselectDelete);
      }
    }

    private static bool CheckExternalPlayer(string filename, bool isImage)
    {
      if (!_useInternalVideoPlayer)
      {
        if (!isImage)
        {
          // extensions filter
          if (!string.IsNullOrEmpty(_externalPlayerExtensions))
          {
            // Use external player if extension is valid
            if (CheckExtension(filename))
            {
              return true;
            }
          }
          else
          {
            return true;
          }
        }
      }
      // DVD files
      if (!_useInternalDVDVideoPlayer)
      {
        if (isImage && Util.Utils.IsDVDImage(filename))
        {
          return true;
        }
        else if (!isImage)
        {
          if (filename.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase) >= 0)
            return true;
        }
      }

      return false;
    }

    private static bool CheckExtension(string filename)
    {
      char[] splitter = { ';' };
      string[] extensions = _externalPlayerExtensions.Split(splitter);

      foreach (string extension in extensions)
      {
        if (extension.Trim().Equals(Path.GetExtension(filename), StringComparison.InvariantCultureIgnoreCase))
        {
          return true;
        }
      }
      return false;
    }

    private void DoOnPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename,
                                              string caller)
    {
      if (type != g_Player.MediaType.Video || filename.EndsWith("&txe=.wmv"))
      {
        return;
      }

      // BD and IMAGES stop time (change to index.bdmv if mpls file is played or to IMG file)
      ISelectBDHandler selectBDHandler = GetSelectBDHandler();
      selectBDHandler.IsBDPlayList(ref filename);
      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      int iidMovie = VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFilesForMovie(iidMovie, ref movies);
      HashSet<string> watchedMovies = new HashSet<string>();

      int playTimePercentage = 0;

      // Stacked movies duration
      if (IsStacked && TotalMovieDuration != 0)
      {
        int duration = 0;

        for (int i = 0; i < StackedMovieFiles.Count; i++)
        {
          int fileID = VideoDatabase.GetFileId((string)StackedMovieFiles[i]);

          if (g_Player.CurrentFile != (string)StackedMovieFiles[i])
          {
            duration += VideoDatabase.GetVideoDuration(fileID);
            continue;
          }
          playTimePercentage = (100 * (duration + timeMovieStopped) / TotalMovieDuration);
          break;
        }
      }
      else
      {
        if (g_Player.Player.Duration >= 1)
        {
          playTimePercentage = (int)Math.Ceiling((timeMovieStopped / g_Player.Player.Duration) * 100);
        }
      }

      if (movies.Count <= 0)
      {
        return;
      }

      for (int i = 0; i < movies.Count; i++)
      {
        string strFilePath = (string)movies[i];

        int idFile = VideoDatabase.GetFileId(strFilePath);

        if (idFile < 0)
        {
          break;
        }

        if (g_Player.IsDVDMenu)
        {
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, 0, null);
          watchedMovies.Add(strFilePath);
          VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), true, 100);
          VideoDatabase.MovieWatchedCountIncrease(VideoDatabase.GetMovieId(strFilePath));
        }
        else if ((filename.Trim().ToLower().Equals(strFilePath.Trim().ToLower())) && (timeMovieStopped > 0))
        {
          byte[] resumeData = null;
          g_Player.Player.GetResumeState(out resumeData);
          Log.Info("GUIVideoFiles: {0} idFile={1} timeMovieStopped={2} resumeData={3}", caller, idFile, timeMovieStopped,
                   resumeData);
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, timeMovieStopped, resumeData);
          Log.Debug("GUIVideoFiles: {0} store resume time", caller);

          //Set file "watched" only if  user % value or higher played time (share view)
          if (playTimePercentage >= _watchedPercentage)
          {
            watchedMovies.Add(strFilePath);
            VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), true, playTimePercentage);
            VideoDatabase.MovieWatchedCountIncrease(VideoDatabase.GetMovieId(strFilePath));
          }
          else
          {
            int iPercent = 0; // Not used, just needed for the watched status call
            int iTImesWatched = 0;
            bool watched = VideoDatabase.GetmovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), out iPercent,
                                                               out iTImesWatched);

            if (!watched)
            {
              VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), false, playTimePercentage);
            }
            else // Update new percentage if already watched
            {
              VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), true, playTimePercentage);
            }
          }
        }
        else
        {
          VideoDatabase.DeleteMovieStopTime(idFile);
        }
      }
      if (_markWatchedFiles)
      {
        // Update db view watched status for played movie
        IMDBMovie movie = new IMDBMovie();
        VideoDatabase.GetMovieInfo(filename, ref movie);

        if (!movie.IsEmpty && (playTimePercentage >= _watchedPercentage || g_Player.IsDVDMenu))
        //Flag movie "watched" status only if user % value or higher played time (database view)
        {
          movie.Watched = 1;
          movie.DateWatched = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
          VideoDatabase.SetMovieInfoById(movie.ID, ref movie);
        }

        if (VideoState.StartWindow != GetID) // Is play initiator dbview?
        {
          UpdateButtonStates();
        }
      }

      if (SubEngine.GetInstance().IsModified())
      {
        bool shouldSave = false;

        if (SubEngine.GetInstance().AutoSaveType == AutoSaveTypeEnum.ASK)
        {
          if (!g_Player.Paused)
          {
            g_Player.Pause();
          }

          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
          dlgYesNo.SetHeading("Save subtitle");
          dlgYesNo.SetLine(1, "Save modified subtitle file?");
          dlgYesNo.SetDefaultToYes(true);
          dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
          shouldSave = dlgYesNo.IsConfirmed;
        }

        if (shouldSave || SubEngine.GetInstance().AutoSaveType == AutoSaveTypeEnum.ALWAYS)
        {
          SubEngine.GetInstance().SaveToDisk();
        }
      }
    }

    private void OnPlayBackChanged(g_Player.MediaType type, int timeMovieStopped, string filename)
    {
      DoOnPlayBackStoppedOrChanged(type, timeMovieStopped, filename, "OnPlayBackChanged");
    }

    private void OnPlayBackStopped(g_Player.MediaType type, int timeMovieStopped, string filename)
    {
      DoOnPlayBackStoppedOrChanged(type, timeMovieStopped, filename, "OnPlayBackStopped");
    }

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video)
      {
        return;
      }

      // BD-DVD image stop time (change to index.bdmv if mpls file is played)
      ISelectBDHandler selectBDHandler = GetSelectBDHandler();
      selectBDHandler.IsBDPlayList(ref filename);

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      HashSet<string> watchedMovies = new HashSet<string>();

      int idMovie = VideoDatabase.GetMovieId(filename);

      if (idMovie >= 0)
      {
        VideoDatabase.GetFilesForMovie(idMovie, ref movies);

        for (int i = 0; i < movies.Count; i++)
        {
          string strFilePath = (string)movies[i];
          byte[] resumeData = null;
          int idFile = VideoDatabase.GetFileId(strFilePath);
          if (idFile < 0)
          {
            break;
          }
          // Set resumedata to zero
          VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, 0, resumeData);
          watchedMovies.Add(strFilePath);
        }

        int playTimePercentage = 0;

        if (IsStacked && TotalMovieDuration != 0)
        {
          int duration = 0;

          for (int i = 0; i < StackedMovieFiles.Count; i++)
          {
            int fileID = VideoDatabase.GetFileId((string)StackedMovieFiles[i]);

            if (filename != (string)StackedMovieFiles[i])
            {
              duration += VideoDatabase.GetVideoDuration(fileID);
              continue;
            }
            playTimePercentage = (int)(100 * (duration + g_Player.Player.CurrentPosition) / TotalMovieDuration);
            break;
          }
        }
        else
        {
          playTimePercentage = 100;
        }

        IMDBMovie details = new IMDBMovie();
        VideoDatabase.GetMovieInfoById(idMovie, ref details);

        if (playTimePercentage >= _watchedPercentage)
        {
          details.Watched = 1;
          VideoDatabase.SetWatched(details);
          VideoDatabase.SetMovieWatchedStatus(idMovie, true, playTimePercentage);
          VideoDatabase.MovieWatchedCountIncrease(idMovie);
          // Set date watched
          VideoDatabase.SetDateWatched(details);
        }
        else
        {
          int percent = 0;
          int timesWatched = 0;
          bool wStatus = VideoDatabase.GetmovieWatchedStatus(idMovie, out percent, out timesWatched);
          VideoDatabase.SetMovieWatchedStatus(idMovie, wStatus, playTimePercentage);
        }
      }
    }

    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video)
      {
        return;
      }

      if (!Util.Utils.IsRemoteUrl(filename))
      {
        int idFile = VideoDatabase.GetFileId(filename);

        if (idFile == -1)
        {
          AddFileToDatabase(filename);
          idFile = VideoDatabase.GetFileId(filename);
        }

        if (idFile != -1)
        {
          int videoDuration = (int)g_Player.Duration;
          VideoDatabase.SetVideoDuration(idFile, videoDuration);
        }
      }
    }

    // Play all files in selected directory
    private void OnPlayAll(string path)
    {
      // Get all video files in selected folder and it's subfolders
      ArrayList playFiles = new ArrayList();
      AddVideoFiles(path, ref playFiles);
      int selectedOption = 0;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      // Check and play according to setting value
      if (_howToPlayAll == 3) // Ask, select sort method from options in GUIDialogMenu
      {
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(498); // menu
        dlg.AddLocalizedString(103); // By Name
        dlg.AddLocalizedString(104); // By Date
        dlg.AddLocalizedString(191); // Shuffle
        // Show GUIDialogMenu
        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        selectedOption = dlg.SelectedId;
      }
      else // Don't ask, sort according to setting and play videos
      {
        selectedOption = _howToPlayAll;
      }

      // Reset playlist
      _playlistPlayer.Reset();
      _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
      PlayList tmpPlayList = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
      tmpPlayList.Clear();

      // Do sorting
      switch (selectedOption)
      {
        //
        // ****** Watch out for fallthrough of empty cases if reordering CASE *******
        //
        case 0: // By name == 103
        case 103:
          IOrderedEnumerable<object> sortedPlayList = GetSortedPlayListbyName(playFiles);
          // Add all files in temporary playlist
          AddToPlayList(tmpPlayList, sortedPlayList);
          break;

        case 1: // By date (date modified) == 104
        case 104:
          sortedPlayList = GetSortedPlayListbyDate(playFiles);
          AddToPlayList(tmpPlayList, sortedPlayList);
          break;

        case 2: // Shuffle == 191
        case 191:
          sortedPlayList = GetSortedPlayListbyName(playFiles);
          AddToPlayList(tmpPlayList, sortedPlayList);
          tmpPlayList.Shuffle();
          break;
      }
      // Play movies
      PlayMovieFromPlayList(false, true);
    }

    private void AddToPlayList(PlayList tmpPlayList, IOrderedEnumerable<object> sortedPlayList)
    {
      foreach (string file in sortedPlayList)
      {
        // Remove stop data if exists
        int idFile = VideoDatabase.GetFileId(file);

        if (idFile >= 0)
        {
          VideoDatabase.DeleteMovieStopTime(idFile);
        }

        // Add file to tmp playlist
        PlayListItem newItem = new PlayListItem();
        newItem.FileName = file;
        // Set file description (for sorting by name -> DVD IFO file problem)
        string description = string.Empty;

        if (file.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          string dvdFolder = file.Substring(0, file.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO", StringComparison.InvariantCultureIgnoreCase));
          description = Path.GetFileName(dvdFolder);
        }
        if (file.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase) >= 0)
        {
          string bdFolder = file.Substring(0, file.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV", StringComparison.InvariantCultureIgnoreCase));
          description = Path.GetFileName(bdFolder);
        }
        else
        {
          description = Path.GetFileName(file);
        }

        newItem.Description = description;
        newItem.Type = PlayListItem.PlayListItemType.Video;
        tmpPlayList.Add(newItem);
      }
    }

    // Sort by item description (Filename or DVD folder)
    private IOrderedEnumerable<object> GetSortedPlayListbyName(ArrayList playFiles)
    {
      return playFiles.ToArray().OrderBy(fn => new PlayListItem().Description);
    }

    // Sort by modified date without path
    private IOrderedEnumerable<object> GetSortedPlayListbyDate(ArrayList playFiles)
    {
      return playFiles.ToArray().OrderBy(fn => new FileInfo((string)fn).LastWriteTime);
    }

    /// <summary>
    /// Adds file (full path) and it's stacked parts (method will search
    /// for them inside strFile folder) into videodatabase (movie table)
    /// This method will also fill mediaInfo data and file duration id
    /// if they are not exists.
    /// </summary>
    /// <param name="strFile"></param>
    /// <returns></returns>
    private ArrayList AddFileToDatabase(string strFile)
    {
      ArrayList files = new ArrayList();
      // Stop is file is not video
      if (!Util.Utils.IsVideo(strFile))
      {
        return files;
      }
      // ??
      if (PlayListFactory.IsPlayList(strFile))
      {
        return files;
      }
      // Don't add web streams (e.g.: from Online videos)
      if (strFile.StartsWith("http:"))
      {
        return files;
      }

      ArrayList allFiles = new ArrayList();

      // If this is database movie, get files and return
      if (VideoDatabase.HasMovieInfo(strFile))
      {
        IMDBMovie movie = new IMDBMovie();
        VideoDatabase.GetMovieInfo(strFile, ref movie);
        VideoDatabase.GetFilesForMovie(movie.ID, ref allFiles);

        // Check if we have mediainfo for files and add it if necessary
        foreach (string file in allFiles)
        {
          if (!VideoDatabase.HasMediaInfo(file))
          {
            VideoDatabase.AddMovieFile(file);
          }
        }

        return allFiles;
      }
      // If file is not scanned (movie is not in the db ie. user video)
      int movieId = VideoDatabase.AddMovieFile(strFile);
      files.Add(strFile);

      // Check if file is stackable and get rest of the stack
      if (IsFileStackable(strFile))
      {
        List<GUIListItem> items = _virtualDirectory.GetDirectoryUnProtectedExt(_currentFolder, true);

        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          
          if (temporaryListItem.IsFolder)
          {
            if(!IsMovieFolder(temporaryListItem.Path))
            {
              continue;
            }
            else
            {
              temporaryListItem.IsBdDvdFolder = true;
            }
          }

          IMDBMovie movie = (IMDBMovie) temporaryListItem.AlbumInfoTag;
          
          if (temporaryListItem.AlbumInfoTag == null)
          {
            IMDBMovie.SetMovieData(temporaryListItem);
            movie = temporaryListItem.AlbumInfoTag as IMDBMovie;
          }

          if (movie == null)
          {
            continue;
          }

          if (movie.VideoFileName != strFile)
          {
            // Add rest of stackable items for main file (main file is already in array)
            if (!temporaryListItem.IsBdDvdFolder)
            {
              if (Util.Utils.ShouldStack(movie.VideoFileName, strFile))
              {
                allFiles.Add(items[i]);
              }
            }
            else
            {
              string strPath, strFileName;
              Util.Utils.Split(strFile, out strPath, out strFileName);

              if (Util.Utils.PathShouldStack(movie.VideoFilePath, strPath))
              {
                allFiles.Add(items[i]);
              }
            }
          }
        }

        // Add rest of the stackable files
        foreach (GUIListItem item in allFiles)
        {
          IMDBMovie movie = (IMDBMovie) item.AlbumInfoTag;
          int idFile = VideoDatabase.GetFileId(movie.VideoFileName);

          if (idFile == -1)
          {
            string path, filename;
            DatabaseUtility.Split(movie.VideoFileName, out path, out filename);
            int pathId = VideoDatabase.AddPath(path);
            VideoDatabase.AddFile(movieId, pathId, filename);
          }

          files.Add(movie.VideoFileName);
        }
      }

      return files;
    }

    private void SetLabel(GUIListItem item)
    {
      ISelectDVDHandler sDvd = GUIVideoFiles.GetSelectDvdHandler();
      ISelectBDHandler sBd = GUIVideoFiles.GetSelectBDHandler();
      string strSize1 = string.Empty, strDate = string.Empty;

      if (item.FileInfo != null && !item.IsFolder)
      {
        strSize1 = Util.Utils.GetSize(item.FileInfo.Length);
      }

      if (item.FileInfo != null && !item.IsFolder)
      {
        if (CurrentSortMethod == VideoSort.SortMethod.Modified)
        {
          strDate = item.FileInfo.ModificationTime.ToShortDateString() + " " +
                    item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        }
        else
        {
          strDate = item.FileInfo.CreationTime.ToShortDateString() + " " +
                    item.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        }
      }

      if (CurrentSortMethod == VideoSort.SortMethod.Name)
      {
        if (item.IsFolder && (sDvd.IsDvdDirectory(item.Path) || sBd.IsBDDirectory(item.Path)))
        {
          // Reserved if needed
        }
        else
        {
          item.Label2 = strSize1;
        }
      }
      else if (CurrentSortMethod == VideoSort.SortMethod.Created || CurrentSortMethod == VideoSort.SortMethod.Date || CurrentSortMethod == VideoSort.SortMethod.Modified)
      {
        item.Label2 = strDate;
      }
      else
      {
        if (item.IsFolder && (sDvd.IsDvdDirectory(item.Path) || sBd.IsBDDirectory(item.Path)))
        {
          //Reserved if needed
        }
        else
        {
          item.Label2 = strSize1;
        }
      }
    }

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          if (message.Param1 == (int)Ripper.AutoPlay.MediaType.VIDEO)
          {
            if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.DVD)
              OnPlayDVD(message.Label, GetID);

            if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.BLURAY)
            {
              OnPlayBD(message.Label, GetID);
            }

            else if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.VCD ||
                     message.Param2 == (int)Ripper.AutoPlay.MediaSubType.FILES)
              OnPlayFiles((System.Collections.ArrayList)message.Object);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          if (g_Player.Playing && g_Player.IsVideo &&
              message.Label.Equals(g_Player.CurrentFile.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase))
          {
            if (!File.Exists(g_Player.CurrentFile))
            {
              Log.Info("GUIVideoFiles: Stop since media is ejected");
              g_Player.Stop();
              _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP).Clear();
              _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Clear();
            }
            else
            {
              return;
            }
          }

          if (GUIWindowManager.ActiveWindow == GetID)
          {
            if (Util.Utils.IsDVD(_currentFolder))
            {
              _currentFolder = string.Empty;
              LoadDirectory(_currentFolder);
            }
          }
          break;
      }
    }

    private void SetMovieWatchStatus(string movieFileName, bool isFolder, bool watched)
    {
      ISelectDVDHandler selectDvdHandler = GetSelectDvdHandler();
      ISelectBDHandler selectBDHandler = GetSelectBDHandler();

      if (isFolder && selectDvdHandler.IsDvdDirectory(movieFileName))
      {
        movieFileName = selectDvdHandler.GetFolderVideoFile(movieFileName);
      }
      if (isFolder && selectBDHandler.IsBDDirectory(movieFileName))
      {
        movieFileName = selectBDHandler.GetFolderVideoFile(movieFileName);
      }

      VideoDatabase.AddMovieFile(movieFileName);

      if (VideoDatabase.HasMovieInfo(movieFileName))
      {
        IMDBMovie movieDetails = new IMDBMovie();
        VideoDatabase.GetMovieInfo(movieFileName, ref movieDetails);

        if (!watched)
        {
          movieDetails.Watched = 0;
        }
        else
        {
          movieDetails.Watched = 1;
        }

        VideoDatabase.SetWatched(movieDetails);
      }

      int iPercent = 0;
      int iTimesWatched = 0;
      int movieId = VideoDatabase.GetMovieId(movieFileName);

      if (!watched)
      {
        VideoDatabase.GetmovieWatchedStatus(movieId, out iPercent, out iTimesWatched);
        VideoDatabase.SetMovieWatchedStatus(movieId, false, iPercent);
      }
      else
      {
        iPercent = 100;
        VideoDatabase.GetmovieWatchedStatus(movieId, out iPercent, out iTimesWatched);
        VideoDatabase.SetMovieWatchedStatus(movieId, true, iPercent);

        if (iTimesWatched <= 0)
        {
          VideoDatabase.MovieWatchedCountIncrease(movieId);
        }
      }
    }
    
    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIPropertyManager.SetProperty("#groupmovielist", string.Empty);

      _currentSelectedItem = facadeLayout.SelectedListItemIndex;
      IMDBMovie info = item.AlbumInfoTag as IMDBMovie;
      
      // Read database only once per item
      if (item.AlbumInfoTag == null)
      {
        IMDBMovie.SetMovieData(item);
        info = item.AlbumInfoTag as IMDBMovie;
      }

      if (info == null)
      {
        return;
      }

      _selectedFilename = info.VideoFileName;
      IMDBMovie.SetMovieProperties(item);
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;

      if (filmstrip != null)
      {
        filmstrip.InfoImageFileName = item.ThumbnailImage;
      }
    }
    
    private bool GetUserPasswordString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);

      if (null == keyboard)
      {
        return false;
      }

      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Password = true;
      keyboard.Text = sString;
      keyboard.DoModal(GetID); // show it...

      if (keyboard.IsConfirmed)
      {
        sString = keyboard.Text;
      }

      return keyboard.IsConfirmed;
    }

    private void OnShowFileMenu(bool preselectDelete)
    {
      GUIListItem item = facadeLayout.SelectedListItem;

      if (item == null)
      {
        return;
      }

      if (item.IsFolder && item.Label == "..")
      {
        return;
      }

      if (!_virtualDirectory.RequestPin(item.Path))
      {
        return;
      }

      // init
      GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_FILE);

      if (dlgFile == null)
      {
        return;
      }

      // File operation settings
      dlgFile.SetSourceItem(item);
      dlgFile.SetSourceDir(_currentFolder);
      dlgFile.SetDestinationDir(_fileMenuDestinationDir);
      dlgFile.SetDirectoryStructure(_virtualDirectory);

      if (preselectDelete)
      {
        dlgFile.PreselectDelete();
      }

      dlgFile.DoModal(GetID);
      _fileMenuDestinationDir = dlgFile.GetDestinationDir();
      //final
      _oldStateSMSsearch = facadeLayout.EnableSMSsearch;
      facadeLayout.EnableSMSsearch = false;

      if (dlgFile.Reload())
      {
        int selectedItem = facadeLayout.SelectedListItemIndex;

        if (_currentFolder != dlgFile.GetSourceDir())
        {
          selectedItem = -1;
        }

        //_currentFolder = Path.GetDirectoryName(dlgFile.GetSourceDir());
        LoadDirectory(_currentFolder);

        if (selectedItem >= 0)
        {
          if (selectedItem >= facadeLayout.Count)
            selectedItem = facadeLayout.Count - 1;
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, selectedItem);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
      _resetSMSsearchDelay = DateTime.Now;
      _resetSMSsearch = true;
    }

    private static bool IsFolderPinProtected(string folder)
    {
      int pinCode = 0;
      return _virtualDirectory.IsProtectedShare(folder, out pinCode);
    }

    private static void AddVideoFiles(string path, ref ArrayList availableFiles)
    {
      //
      // Count the files in the current directory
      //
      bool currentCreateVideoThumbs = false;

      try
      {
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.VideoExtensions);
        // Temporary disable thumbcreation
        using (Profile.Settings xmlreader = new MPSettings())
        {
          currentCreateVideoThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
        }
        using (Profile.Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", false);
        }

        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);

        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              if (item.Path.ToUpperInvariant().IndexOf("VIDEO_TS") >= 0)
              {
                string strFile = String.Format(@"{0}\VIDEO_TS.IFO", item.Path);
                availableFiles.Add(strFile);
              }
              if (item.Path.ToUpperInvariant().IndexOf("BDMV") >= 0)
              {
                string strFile = String.Format(@"{0}\index.bdmv", item.Path);
                availableFiles.Add(strFile);
              }
              else
              {
                AddVideoFiles(item.Path, ref availableFiles);
              }
            }
          }
          else
          {
            availableFiles.Add(item.Path);
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("Exception counting files:{0}", e);
        // Ignore
      }
      finally
      {
        // Restore thumbcreation setting
        using (Profile.Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", currentCreateVideoThumbs);
        }
      }
    }

    private void DownloadSubtitles()
    {
      try
      {
        GUIListItem item = facadeLayout.SelectedListItem;

        if (item == null)
        {
          return;
        }
        string path = item.Path;
        bool isDVD = (path.ToUpper().IndexOf("VIDEO_TS") >= 0);
        List<GUIListItem> listFiles = _virtualDirectory.GetDirectoryUnProtectedExt(_currentFolder, false);
        string[] subExts = {
                              ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss",
                              ".ass", ".idx", ".ifo"
                            };
        if (!isDVD)
        {
          // check if movie has subtitles
          for (int i = 0; i < subExts.Length; i++)
          {
            for (int x = 0; x < listFiles.Count; ++x)
            {
              if (listFiles[x].IsFolder)
              {
                continue;
              }
              string subTitleFileName = listFiles[x].Path;
              subTitleFileName = Path.ChangeExtension(subTitleFileName, subExts[i]);

              if (String.Compare(listFiles[x].Path, subTitleFileName, true) == 0)
              {
                string localSubtitleFileName = _virtualDirectory.GetLocalFilename(subTitleFileName);
                Util.Utils.FileDelete(localSubtitleFileName);
                _virtualDirectory.DownloadRemoteFile(subTitleFileName, 0);
              }
            }
          }
        }
        else //download entire DVD
        {
          for (int i = 0; i < listFiles.Count; ++i)
          {
            if (listFiles[i].IsFolder)
            {
              continue;
            }

            if (String.Compare(listFiles[i].Path, path, true) == 0)
            {
              continue;
            }

            _virtualDirectory.DownloadRemoteFile(listFiles[i].Path, 0);
          }
        }
      }
      catch (ThreadAbortException) { }
    }

    private bool IsFileStackable(string filename)
    {
      var pattern = Util.Utils.StackExpression();

      for (int i = 0; i < pattern.Length; i++)
      {
        if (pattern[i].IsMatch(filename))
        {
          return true;
        }
      }
      return false;
    }

    private void GetNfoFiles(string path, ref ArrayList nfoFiles)
    {
      string[] files = Directory.GetFiles(path, "*.nfo", SearchOption.AllDirectories);
      var sortedFiles = files.OrderBy(f => f);

      foreach (string file in sortedFiles)
      {
        nfoFiles.Add(file);
      }
    }
    
    #endregion

    #region Thread Set thumbs

    private void SetImdbThumbs(List<GUIListItem> itemlist, ISelectDVDHandler selectDVDHandler)
    {
      if (_setThumbs != null && _setThumbs.IsAlive)
      {
        _setThumbs.Abort();
        _setThumbs = null;
      }

      _setThumbsThreadAborted = false;

      _threadGUIItems.Clear();
      _threadGUIItems.AddRange(itemlist);
      _threadISelectDVDHandler = selectDVDHandler;

      _setThumbs = new Thread(ThreadSetIMDBThumbs);
      _setThumbs.Priority = ThreadPriority.Lowest;
      _setThumbs.IsBackground = true;
      _setThumbs.Start();
    }

    private void ThreadSetIMDBThumbs()
    {
      try
      {
        _threadISelectDVDHandler.SetIMDBThumbs(_threadGUIItems, _markWatchedFiles);
        // Refresh thumb on selected item
        _currentSelectedItem = facadeLayout.SelectedListItemIndex;
        SelectCurrentItem();
      }
      catch (ThreadAbortException)
      {
        Log.Info("GUIVideoFiles: Thread SetIMDBThumbs aborted.");
        _setThumbsThreadAborted = true;
      }
    }

    #endregion

    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsInstance(fetcher))
      {
        pDlgProgress.DisableCancel(true);
      }
      return true;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted)
      {
        return;
      }
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      if (percent > 0)
      {
        pDlgProgress.SetPercentage(percent);
      }
      pDlgProgress.Progress();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're busy querying www.imdb.com
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(197), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(197);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        return false;
      }
      // show dialog...
      GUIDialogOK pDlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      pDlgOk.SetHeading(195);
      pDlgOk.SetLine(1, fetcher.MovieName);
      pDlgOk.SetLine(2, string.Empty);
      pDlgOk.DoModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the movie info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(198), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(198);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(1301);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(1302);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.ActorName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      return true;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        return false;
      }
      // show dialog...
      GUIDialogOK pDlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      // show dialog...
      pDlgOk.SetHeading(195);
      pDlgOk.SetLine(1, fetcher.MovieName);
      pDlgOk.SetLine(2, string.Empty);
      pDlgOk.DoModal(GUIWindowManager.ActiveWindow);
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      movieName = string.Empty;

      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        movieName = string.Empty;
        return false;
      }

      string filename = string.Empty;
      
      if (!string.IsNullOrEmpty(_selectedFilename))
      {
        try
        {
          if (_selectedFilename.ToUpperInvariant().Contains(@"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            filename = _selectedFilename.ToUpperInvariant().Replace(@"\VIDEO_TS\VIDEO_TS.IFO", string.Empty);
            int iIndex = 0;
            iIndex = filename.LastIndexOf(@"\");
            movieName = filename.Substring(iIndex + 1);
          }
          else if (_selectedFilename.ToUpperInvariant().Contains(@"\BDMV\INDEX.BDMV"))
          {
            filename = _selectedFilename.ToUpperInvariant().Replace(@"\BDMV\INDEX.BDMV", string.Empty);
            int iIndex = 0;
            iIndex = filename.LastIndexOf(@"\");
            movieName = filename.Substring(iIndex + 1);
          }
          else
          {
            movieName = Path.GetFileNameWithoutExtension(_selectedFilename);
            Util.Utils.RemoveStackEndings(ref filename);
          }
        }
        catch (Exception){}
      }
      else
      {
        movieName = fetcher.MovieName;
      }

      if (GetKeyboard(ref movieName))
      {
        if (movieName == string.Empty)
        {
          return false;
        }
        return true;
      }

      movieName = string.Empty;
      return false;
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        selectedMovie = -1;
        return false;
      }
      GUIDialogSelect pDlgSelect = (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
      // more then 1 movie found
      // ask user to select 1
      string filename = string.Empty;

      if (!string.IsNullOrEmpty(_selectedFilename))
      {
        if (_selectedFilename.ToUpperInvariant().Contains(@"VIDEO_TS\VIDEO_TS.IFO"))
        {
          filename = _selectedFilename.ToUpperInvariant().Replace(@"\VIDEO_TS\VIDEO_TS.IFO", string.Empty);
          int iIndex = 0;
          iIndex = filename.LastIndexOf(@"\");
          filename = filename.Substring(iIndex + 1);
        }
        else if (_selectedFilename.ToUpperInvariant().Contains(@"BDMV\INDEX.BDMV"))
        {
          filename = _selectedFilename.ToUpperInvariant().Replace(@"\BDMV\INDEX.BDMV", string.Empty);
          int iIndex = 0;
          iIndex = filename.LastIndexOf(@"\");
          filename = filename.Substring(iIndex + 1);
        }
        else
        {
          filename = Path.GetFileNameWithoutExtension(_selectedFilename);
          Util.Utils.RemoveStackEndings(ref filename);
        }
      }

      string strHeading = GUILocalizeStrings.Get(196);

      if (!string.IsNullOrEmpty(filename))
      {
        GUIPropertyManager.SetProperty("#selecteditem", filename);
      }

      pDlgSelect.SetHeading(strHeading);
      pDlgSelect.Reset();

      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      
      pDlgSelect.EnableButton(true);
      pDlgSelect.SetButtonLabel(413); // manual
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);
      // and wait till user selects one
      selectedMovie = pDlgSelect.SelectedLabel;
      
      if (pDlgSelect.IsButtonPressed)
      {
        return true;
      }
      if (selectedMovie == -1)
      {
        return false;
      }
      return true;
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selectedActor)
    {
      GUIDialogSelect pDlgSelect = (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
      // more then 1 actor found
      // ask user to select 1
      pDlgSelect.SetHeading("Select actor:"); //select actor
      pDlgSelect.Reset();
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      pDlgSelect.EnableButton(false);
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedActor = pDlgSelect.SelectedLabel;
      if (selectedActor != -1)
      {
        return true;
      }
      return false;
    }

    public bool OnScanStart(int total)
    {
      _scanning = true;
      _conflictFiles.Clear();
      _scanningFileTotal = total;
      _scanningFileNumber = 1;
      return true;
    }

    public bool OnScanEnd()
    {
      _scanning = false;
      if (_conflictFiles.Count > 0)
      {
        GUIDialogSelect pDlgSelect = (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
        // more than 1 movie found
        // ask user to select 1
        do
        {
          pDlgSelect.Reset();
          pDlgSelect.SetHeading(892); //select movie
          for (int i = 0; i < _conflictFiles.Count; ++i)
          {
            IMDBMovie currentMovie = (IMDBMovie) _conflictFiles[i];
            string strFileName = string.Empty;
            string path = currentMovie.Path;
            string filename = currentMovie.File;
            if (path != string.Empty)
            {
              if (path.EndsWith(@"\"))
              {
                path = path.Substring(0, path.Length - 1);
                currentMovie.Path = path;
              }
              if (filename.StartsWith(@"\"))
              {
                filename = filename.Substring(1);
                currentMovie.File = filename;
              }
              strFileName = path + @"\" + filename;
            }
            else
            {
              strFileName = filename;
            }
            pDlgSelect.Add(strFileName);
          }
          pDlgSelect.EnableButton(true);
          pDlgSelect.SetButtonLabel(4517); // manual
          pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

          // and wait till user selects one
          int selectedMovie = pDlgSelect.SelectedLabel;
          if (pDlgSelect.IsButtonPressed)
          {
            break;
          }
          if (selectedMovie == -1)
          {
            break;
          }
          IMDBMovie movieDetails = (IMDBMovie) _conflictFiles[selectedMovie];
          string searchText = movieDetails.Title;
          if (searchText == string.Empty)
          {
            searchText = movieDetails.SearchString;
          }
          if (GetKeyboard(ref searchText))
          {
            if (searchText != string.Empty)
            {
              movieDetails.SearchString = searchText;
              if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, true))
              {
                if (movieDetails != null)
                {
                  _conflictFiles.RemoveAt(selectedMovie);
                }
              }
            }
          }
        } while (_conflictFiles.Count > 0);
      }
      return true;
    }

    public bool OnScanIterating(int count)
    {
      _scanningFileNumber = count;
      return true;
    }

    public bool OnScanIterated(int count)
    {
      _scanningFileNumber = count;
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public string PluginName()
    {
      return "Videos";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(3);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my videos.png";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Watch and organize your video files";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIVideoFiles.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}