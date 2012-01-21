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
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
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

    private static IMDB _imdb;
    private static bool _askBeforePlayingDVDImage = false;
    private static VirtualDirectory _virtualDirectory;
    private static PlayListPlayer _playlistPlayer;

    private MapSettings _mapSettings = new MapSettings();
    private DirectoryHistory _history = new DirectoryHistory();
    private string _virtualStartDirectory = string.Empty;
    private int _currentSelectedItem = -1;

    // File menu
    private string _fileMenuDestinationDir = string.Empty;
    private bool _fileMenuEnabled = false;
    private string _fileMenuPinCode = string.Empty;

    private bool _scanning = false;
    private int scanningFileNumber = 1;
    private int scanningFileTotal = 1;
    private bool _isFuzzyMatching = false;
    private bool _scanSkipExisting = false;
    private bool _getActors = true;
    private bool _markWatchedFiles = true;
    private bool _eachFolderIsMovie = false;
    private ArrayList _conflictFiles = new ArrayList();
    private bool _switchRemovableDrives;
    // Stacked files duration - for watched status/also used in GUIVideoTitle
    public static int _totalMovieDuration = 0;
    public static ArrayList _stackedMovieFiles = new ArrayList();
    public static bool _isStacked = false;

    private List<GUIListItem> _cachedItems = new List<GUIListItem>();
    private string _cachedDir = null;

    private bool _resetSMSsearch = false;
    private bool _oldStateSMSsearch;
    private DateTime _resetSMSsearchDelay;

    private int _howToPlayAll = 3;

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

    #region BaseWindow Members

    protected override bool CurrentSortAsc
    {
      get { return _mapSettings.SortAscending; }
      set { _mapSettings.SortAscending = value; }
    }

    protected override VideoSort.SortMethod CurrentSortMethod
    {
      get { return (VideoSort.SortMethod)_mapSettings.SortBy; }
      set { _mapSettings.SortBy = (int)value; }
    }

    protected override Layout CurrentLayout
    {
      get { return (Layout)_mapSettings.ViewAs; }
      set { _mapSettings.ViewAs = (int)value; }
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myVideo.xml");
    }

    public override void OnAdded()
    {
      base.OnAdded();
      _imdb = new IMDB(this);
      // _currentFolder = null;

      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackChanged += new g_Player.ChangedHandler(OnPlayBackChanged);
      GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);
      LoadSettings();
    }

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        currentLayout = (Layout)xmlreader.GetValueAsInt(SerializeName, "layout", (int)Layout.List);
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
        _eachFolderIsMovie = xmlreader.GetValueAsBool("movies", "eachFolderIsMovie", false);
        _fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        _fileMenuPinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
        _howToPlayAll = xmlreader.GetValueAsInt("movies", "playallinfolder", 3);

        _virtualDirectory = VirtualDirectories.Instance.Movies;

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
      }

      if (_currentFolder.Length > 0 && _currentFolder == _virtualStartDirectory)
      {
        VirtualDirectory vDir = new VirtualDirectory();
        vDir.LoadSettings("movies");
        int pincode = 0;
        bool FolderPinProtected = vDir.IsProtectedShare(_currentFolder, out pincode);
        if (FolderPinProtected)
        {
          _currentFolder = string.Empty;
        }
      }

      if (_currentFolder.Length > 0 && !_virtualDirectory.IsRemote(_currentFolder))
      {
        DirectoryInfo dirInfo = new DirectoryInfo(_currentFolder);

        while (dirInfo.Parent != null)
        {
          string dirName = dirInfo.Name;
          dirInfo = dirInfo.Parent;
          string currentParentFolder = @dirInfo.FullName;
          _history.Set(dirName, currentParentFolder);
        }
      }
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "layout", (int)currentLayout);
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
        if ((item != null) && item.IsFolder && (item.Label == "..") && (_currentFolder != _virtualStartDirectory))
        {
          LoadDirectory(item.Path);
          return;
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeLayout[0];
        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          LoadDirectory(item.Path);
        }
        return;
      }

      if (action.wID == Action.ActionType.ACTION_DELETE_ITEM)
      {
        ShowFileMenu(true);
      }

      base.OnAction(action);
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

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
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

      LoadFolderSettings(_currentFolder);

      //OnPageLoad is sometimes called when stopping playback.
      //So we use the cached version of the function here.
      LoadDirectory(_currentFolder, true);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _currentSelectedItem = facadeLayout.SelectedListItemIndex;
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

    private void LoadFolderSettings(string folderName)
    {
      if (folderName == string.Empty)
      {
        folderName = "root";
      }
      object o;
      FolderSettings.GetFolderSetting(folderName, "VideoFiles", typeof (MapSettings), out o);
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
      FolderSettings.AddFolderSetting(folderName, "VideoFiles", typeof (MapSettings), _mapSettings);
    }

    protected override void LoadDirectory(string newFolderName)
    {
      this.LoadDirectory(newFolderName, false);
    }

    private void LoadDirectory(string newFolderName, bool useCache)
    {
      this.LoadDirectory(newFolderName, useCache, null);
    }

    private void LoadDirectory(string newFolderName, bool useCache, HashSet<string> watchedFiles)
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
      GUIListItem selectedListItem = null;

      if (facadeLayout != null)
      {
        selectedListItem = facadeLayout.SelectedListItem;
      }
      if (selectedListItem != null)
      {
        if (selectedListItem.IsFolder && selectedListItem.Label != "..")
        {
          _history.Set(selectedListItem.Label, _currentFolder);
        }
      }

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
          return;

        _currentFolder = DaemonTools.GetVirtualDrive();
      }
      else
      {
        _currentFolder = newFolderName;
      }
      string objectCount = string.Empty;

      if (facadeLayout != null)
      {
        GUIControl.ClearControl(GetID, facadeLayout.GetID);
      }


      List<GUIListItem> itemlist = null;

      //Tweak to boost performance when starting/stopping playback
      //For further details see comment in Core\Util\VirtualDirectory.cs
      if (useCache && _cachedDir == _currentFolder)
      {
        itemlist = _cachedItems;
        SelectDVDHandler selectDvdHandler = new SelectDVDHandler();

        foreach (GUIListItem item in itemlist)
        {
          if (watchedFiles != null && watchedFiles.Contains(item.Path))
          {
            item.IsPlayed = true;
          }
          else if (_markWatchedFiles &&
                   (item.IsFolder && selectDvdHandler.IsDvdDirectory(item.Path) || Util.Utils.IsVideo(item.Path)))
          {
            string file = item.Path;
            if (item.IsFolder)
              file = selectDvdHandler.GetFolderVideoFile(item.Path);
            // Check db for watched status for played movie or changed status in movie info window
            item.IsPlayed = VideoDatabase.GetmovieWatchedStatus(VideoDatabase.GetMovieId(file));
          }
          //Do NOT add OnItemSelected event handler here, because its still there...
          facadeLayout.Add(item);
        }
      }
      else
      {
        // here we get ALL files in every subdir, look for folderthumbs, defaultthumbs, etc
        itemlist = _virtualDirectory.GetDirectoryExt(_currentFolder);
        if (_mapSettings.Stack)
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
                if ((!item1.IsFolder || !item2.IsFolder)
                    && (!item1.IsRemote && !item2.IsRemote)
                    && Util.Utils.ShouldStack(item1.Path, item2.Path))
                {
                  if (String.Compare(item1.Path, item2.Path, true) > 0)
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
              innerList = new List<GUIListItem> {item1};
              stackings.Add(cleanFilename, innerList);
            }
          }

          List<GUIListItem> itemfiltered = new List<GUIListItem>(itemlist.Count);
          foreach (KeyValuePair<string, List<GUIListItem>> pair in stackings)
          {
            List<GUIListItem> innerList = pair.Value;
            for (int i = 0; i < innerList.Count; i++)
            {
              GUIListItem item = innerList[i];
              if ((VirtualDirectory.IsValidExtension(item.Path, Util.Utils.VideoExtensions, false)))
                item.Label = pair.Key;
              itemfiltered.Add(item);
            }
          }
          itemlist = itemfiltered;
        }

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
        // folder.jpg will already be assigned from "itemlist = virtualDirectory.GetDirectory(_currentFolder);" here
        selectDVDHandler.SetIMDBThumbs(itemlist, _markWatchedFiles, _eachFolderIsMovie);

        foreach (GUIListItem item in itemlist)
        {
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeLayout.Add(item);
        }

        _cachedItems = itemlist;
        _cachedDir = _currentFolder;
      }

      OnSort();

      bool itemSelected = false;

      //Sometimes the last selected item wasn't restored correcly after playback stop
      //The !useCache fixes this
      if (selectedListItem != null && !useCache)
      {
        string selectedItemLabel = _history.Get(_currentFolder);
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem item = facadeLayout[i];
          if (item.Label == selectedItemLabel)
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, i);
            itemSelected = true;
            break;
          }
        }
      }
      int totalItems = itemlist.Count;
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = (GUIListItem)itemlist[0];
        if (rootItem.Label == "..")
        {
          totalItems--;
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItems));

      if (!itemSelected)
      {
        UpdateButtonStates();
        SelectCurrentItem();
      }

      GUIWaitCursor.Hide();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeLayout.SelectedListItem;
      _totalMovieDuration = 0;
      _isStacked = false;
      _stackedMovieFiles.Clear();

      if (item == null)
      {
        return;
      }
      bool isFolderAMovie = false;
      string path = item.Path;

      if (item.IsFolder && !item.IsRemote)
      {
        // Check if folder is actually a DVD. If so don't browse this folder, but play the DVD!
        if ((File.Exists(path + @"\VIDEO_TS\VIDEO_TS.IFO")) && (item.Label != ".."))
        {
          isFolderAMovie = true;
          path = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else if ((File.Exists(path + @"\BDMV\index.bdmv")) && (item.Label != ".."))
        {
          isFolderAMovie = true;
          path = item.Path + @"\BDMV\index.bdmv";
        }
        else
        {
          isFolderAMovie = false;
        }
      }

      if ((item.IsFolder && !isFolderAMovie))
        //-- Mars Warrior @ 03-sep-2004
      {
        _currentSelectedItem = -1;
        LoadDirectory(path);
      }
      else
      {
        if (!_virtualDirectory.RequestPin(path))
        {
          return;
        }
        if (_virtualDirectory.IsRemote(path))
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(path, item.FileInfo.Length))
          {
            if (!_virtualDirectory.ShouldWeDownloadFile(path))
            {
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

              return;
            }
            else
            {
              //download subtitle files
              Thread subLoaderThread = new Thread(new ThreadStart(this._downloadSubtitles));
              subLoaderThread.IsBackground = true;
              subLoaderThread.Name = "SubtitleLoader";
              subLoaderThread.Start();
            }
          }
        }

        if (item.FileInfo != null)
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(path, item.FileInfo.Length))
          {
            return;
          }
        }
        string movieFileName = path;
        movieFileName = _virtualDirectory.GetLocalFilename(movieFileName);

        // Set selected item
        _currentSelectedItem = facadeLayout.SelectedListItemIndex;
        if (PlayListFactory.IsPlayList(movieFileName))
        {
          LoadPlayList(movieFileName);
          return;
        }

        if (!CheckMovie(movieFileName))
        {
          return;
        }
        bool askForResumeMovie = true;
        if (_mapSettings.Stack)
        {
          int selectedFileIndex = 0;
          int movieDuration = 0;
          ArrayList movies = new ArrayList();

          #region Is all this really neccessary?!

          //get all movies belonging to each other
          List<GUIListItem> items = _virtualDirectory.GetDirectoryUnProtectedExt(_currentFolder, true);

          //check if we can resume 1 of those movies
          int timeMovieStopped = 0;
          bool asked = false;
          ArrayList newItems = new ArrayList();
          for (int i = 0; i < items.Count; ++i)
          {
            GUIListItem temporaryListItem = (GUIListItem)items[i];
            if (Util.Utils.ShouldStack(temporaryListItem.Path, path))
            {
              _isStacked = true;
              _stackedMovieFiles.Add(temporaryListItem.Path);
              if (!asked)
              {
                selectedFileIndex++;
              }
              IMDBMovie movieDetails = new IMDBMovie();
              int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);
              int idMovie = VideoDatabase.GetMovieId(path);
              if ((idMovie >= 0) && (idFile >= 0))
              {
                VideoDatabase.GetMovieInfo(path, ref movieDetails);
                string title = Path.GetFileName(path);
                if ((VirtualDirectory.IsValidExtension(path, Util.Utils.VideoExtensions, false)))
                {
                  Util.Utils.RemoveStackEndings(ref title);
                }
                if (movieDetails.Title != string.Empty)
                {
                  title = movieDetails.Title;
                }

                timeMovieStopped = VideoDatabase.GetMovieStopTime(idFile);
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
                      newItems.Add(temporaryListItem);
                    }
                    else
                    {
                      askForResumeMovie = false;
                      newItems.Add(temporaryListItem);
                    }
                  } //if (!asked)
                  else
                  {
                    newItems.Add(temporaryListItem);
                  }
                } //if (timeMovieStopped>0)
                else
                {
                  newItems.Add(temporaryListItem);
                }

                // Total movie duration
                movieDuration += VideoDatabase.GetMovieDuration(idFile);
                _totalMovieDuration = movieDuration;
              }
              else //if (idMovie >=0)
              {
                newItems.Add(temporaryListItem);
              }
            } //if ( MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, path))
          }

          for (int i = 0; i < newItems.Count; ++i)
          {
            GUIListItem temporaryListItem = (GUIListItem)newItems[i];
            if (Util.Utils.IsVideo(temporaryListItem.Path) && !PlayListFactory.IsPlayList(temporaryListItem.Path))
            {
              if (Util.Utils.ShouldStack(temporaryListItem.Path, path))
              {
                movies.Add(temporaryListItem.Path);
              }
            }
          }
          if (movies.Count == 0)
          {
            movies.Add(movieFileName);
          }

          #endregion

          if (movies.Count <= 0)
          {
            return;
          }
          if (movies.Count > 1)
          {
            //TODO
            movies.Sort();

            for (int i = 0; i < movies.Count; ++i)
            {
              AddFileToDatabase((string)movies[i]);
            }
            // Stacked movies duration
            if (_totalMovieDuration == 0)
            {
              MovieDuration(movies);
              _stackedMovieFiles = movies;
            }

            if (askForResumeMovie)
            {
              GUIDialogFileStacking dlg =
                (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_FILESTACKING);
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
          else if (movies.Count == 1)
          {
            AddFileToDatabase((string)movies[0]);
            MovieDuration(movies);
          }
          _playlistPlayer.Reset();
          _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
          PlayList playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
          playlist.Clear();
          for (int i = 0; i < (int)movies.Count; ++i)
          {
            movieFileName = (string)movies[i];
            PlayListItem itemNew = new PlayListItem();
            itemNew.FileName = movieFileName;
            itemNew.Type = PlayListItem.PlayListItemType.Video;
            playlist.Add(itemNew);
          }

          // play movie...
          PlayMovieFromPlayList(askForResumeMovie, selectedFileIndex - 1, true);
          return;
        }

        // play movie...
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
        /*
                //TODO
                if (g_Player.Play(movieFileName))
                {
                  if ( MediaPortal.Util.Utils.IsVideo(movieFileName))
                  {
                    GUIGraphicsContext.IsFullScreenVideo = true;
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                  }
                }*/
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
        //TODO
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

    public static int MovieDuration(ArrayList files)
    {
      _totalMovieDuration = 0;

      foreach (string file in files)
      {
        int fileID = VideoDatabase.GetFileId(file);
        int tempDuration = VideoDatabase.GetMovieDuration(fileID);

        if (tempDuration > 0)
        {
          _totalMovieDuration += tempDuration;
        }
        else
        {
          MediaInfoWrapper mInfo = new MediaInfoWrapper(file);

          if (fileID > -1)
            VideoDatabase.SetMovieDuration(fileID, mInfo.VideoDuration / 1000);
          _totalMovieDuration += mInfo.VideoDuration / 1000;
        }
      }
      return _totalMovieDuration;
    }

    private void AddFileToDatabase(string strFile)
    {
      if (!Util.Utils.IsVideo(strFile))
      {
        return;
      }
      //if (  MediaPortal.Util.Utils.IsNFO(strFile)) return;
      if (PlayListFactory.IsPlayList(strFile))
      {
        return;
      }

      if (!VideoDatabase.HasMovieInfo(strFile))
      {
        ArrayList allFiles = new ArrayList();
        List<GUIListItem> items = _virtualDirectory.GetDirectoryUnProtectedExt(_currentFolder, true);
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          if (temporaryListItem.IsFolder)
          {
            continue;
          }
          if (temporaryListItem.Path != strFile)
          {
            if (Util.Utils.ShouldStack(temporaryListItem.Path, strFile))
            {
              allFiles.Add(items[i]);
            }
          }
        }
        int iidMovie = VideoDatabase.AddMovieFile(strFile);
        foreach (GUIListItem item in allFiles)
        {
          string strPath, strFileName;

          DatabaseUtility.Split(item.Path, out strPath, out strFileName);
          DatabaseUtility.RemoveInvalidChars(ref strPath);
          DatabaseUtility.RemoveInvalidChars(ref strFileName);
          int pathId = VideoDatabase.AddPath(strPath);
          VideoDatabase.AddFile(iidMovie, pathId, strFileName);
        }
      }
    }

    // CHANGED

    // GUI Item file name handler - share view ->IMDB
    protected override void OnInfo(int iItem)
    {
      _currentSelectedItem = facadeLayout.SelectedListItemIndex;
      GUIListItem pItem = facadeLayout.SelectedListItem;
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
        if (pItem.Label == "..")
        {
          return;
        }
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
        strFile = selectDVDHandler.GetFolderVideoFile(pItem.Path);

        if (strFile == string.Empty)
        {
          bFoundFile = false;
          strFile = pItem.Path;
        }
        else if (strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
        {
          //DVD folder
          string dvdFolder = strFile.Substring(0, strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
          strMovie = Path.GetFileName(dvdFolder);
        }
        else if (strFile.ToUpper().IndexOf(@"\BDMV\index.bdmv") >= 0)
        {
          //Blu-Ray folder
          string dvdFolder = strFile.Substring(0, strFile.ToUpper().IndexOf(@"\BDMV\index.bdmv"));
          strMovie = Path.GetFileName(dvdFolder);
        }
        else
        {
          //Movie 
          strMovie = Path.GetFileNameWithoutExtension(strFile);
        }
      }
      // Use DVD label as movie name
      if (Util.Utils.IsDVD(pItem.Path) && (pItem.DVDLabel != string.Empty))
      {
        if (File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          strFile = pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else if (File.Exists(pItem.Path + @"\BDMV\index.bdmv"))
        {
          strFile = pItem.Path + @"\BDMV\index.bdmv";
        }
        strMovie = pItem.DVDLabel;
      }
      IMDBMovie movieDetails = new IMDBMovie();
      if ((VideoDatabase.GetMovieInfo(strFile, ref movieDetails) == -1) ||
          (movieDetails.IsEmpty))
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
        // Movie is not in the database
        if (bFoundFile)
        {
          AddFileToDatabase(strFile);
        }

        // Changed - Movie folder title
        using (MediaPortal.Profile.Settings xmlreader = new MPSettings())
        {
          bool foldercheck = xmlreader.GetValueAsBool("moviedatabase", "usefolderastitle", false);
          if (foldercheck == true)
          {
            movieDetails.SearchString = Path.GetFileName(Path.GetDirectoryName(strMovie));
          }
          else
          {
            movieDetails.SearchString = Path.GetFileNameWithoutExtension(strMovie);
          }
          movieDetails.File = Path.GetFileName(strFile);
        }
        // End change
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
        if (!IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, _getActors))
        {
          return;
        }
      }
      // Movie is in the database
      GUIVideoInfo videoInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_INFO);
      videoInfo.Movie = movieDetails;
      if (pItem.IsFolder)
      {
        videoInfo.FolderForThumbs = pItem.Path;
      }
      else
      {
        videoInfo.FolderForThumbs = string.Empty;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_INFO);

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
        //return true;
      }
      //else
      //{
      //  Log.Debug("GUIVideoFiles: SelectCurrentItem - nothing to do for item {0}", _currentSelectedItem.ToString());
      //  return false;
      //}
    }

    #endregion

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          if (message.Param1 == (int)Ripper.AutoPlay.MediaType.VIDEO)
          {
            if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.DVD)
              OnPlayDVD(message.Label, GetID);
            else if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.BLURAY)
            {
              g_Player.Play((message.Label + "\\BDMV\\index.bdmv"), g_Player.MediaType.Video);
              g_Player.ShowFullScreenWindow();
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
            Log.Info("GUIVideoFiles: Stop since media is ejected");
            g_Player.Stop();
            _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP).Clear();
            _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Clear();
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

    private void SetMovieUnwatched(string movieFileName, bool isFolder)
    {
      SelectDVDHandler isDvdFolder = new SelectDVDHandler();

      if (isFolder && isDvdFolder.IsDvdDirectory(movieFileName))
        movieFileName = isDvdFolder.GetFolderVideoFile(movieFileName);

      if (VideoDatabase.HasMovieInfo(movieFileName))
      {
        IMDBMovie movieDetails = new IMDBMovie();
        int idMovie = VideoDatabase.GetMovieInfo(movieFileName, ref movieDetails);
        movieDetails.Watched = 0;
        VideoDatabase.SetWatched(movieDetails);
      }
      int idFile = VideoDatabase.GetFileId(movieFileName);
      VideoDatabase.DeleteMovieStopTime(idFile);
      VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(movieFileName), false);
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

      GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
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
            VideoDatabase.GetFiles(idMovie, ref movies);
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

    private void FetchMatroskaInfo(string path, bool pathIsDirectory, ref IMDBMovie movie)
    {
      string xmlFile = string.Empty;
      if (!pathIsDirectory)
      {
        xmlFile = Path.ChangeExtension(path, ".xml");
      }

      MatroskaTagInfo minfo = MatroskaTagHandler.Fetch(xmlFile);
      if (minfo != null)
      {
        movie.Title = minfo.title;
        movie.Plot = minfo.description;
        movie.Genre = minfo.genre;
      }
    }

    private void SetMovieProperties(string path)
    {
      bool isFile = false;
      if (Util.Utils.IsVideo(path))
        isFile = true;
      IMDBMovie info = new IMDBMovie();
      if (path == "..")
      {
        info.Reset();
        info.SetProperties(true);
        return;
      }
      bool isDirectory = false;
      bool isMultiMovieFolder = false;
      bool isFound = false;
      try
      {
        if (Directory.Exists(path))
        {
          isDirectory = true;
          string[] files = Directory.GetFiles(path);
          foreach (string file in files)
          {
            IMDBMovie movie = new IMDBMovie();
            VideoDatabase.GetMovieInfo(file, ref movie);
            if (!movie.IsEmpty)
            {
              if (!isFound)
              {
                info = movie;
                isFound = true;
              }
              else
              {
                isMultiMovieFolder = true;
                break;
              }
            }
          }
        }
        else
        {
          VideoDatabase.GetMovieInfo(path, ref info);
        }
        if (info.IsEmpty)
        {
          FetchMatroskaInfo(path, isDirectory, ref info);
        }
        if (info.IsEmpty && File.Exists(path + @"\VIDEO_TS\VIDEO_TS.IFO")) //still empty and is ripped DVD
        {
          VideoDatabase.GetMovieInfo(path + @"\VIDEO_TS\VIDEO_TS.IFO", ref info);
          isFile = true;
        }
        if (info.IsEmpty && File.Exists(path + @"\BDMV\index.bdmv")) //still empty and is ripped DVD
        {
          VideoDatabase.GetMovieInfo(path + @"\BDMV\index.bdmv", ref info);
          isFile = true;
        }
        if (info.IsEmpty)
        {
          if (_markWatchedFiles)
          {
            int fID = VideoDatabase.GetFileId(path);
            byte[] resumeData = null;
            int timeStopped = VideoDatabase.GetMovieStopTimeAndResumeData(fID, out resumeData);
            if (timeStopped > 0 || resumeData != null)
              info.Watched = 1;
          }
        }
        if (isMultiMovieFolder || !isFile)
        {
          info.Reset();
          info.SetProperties(true);
          return;
        }
        info.SetProperties(false);
      }
      catch (Exception) {}
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      SetMovieProperties(item.Path);
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip != null)
      {
        filmstrip.InfoImageFileName = item.ThumbnailImage;
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

      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfo(filename, ref movieDetails);
      int idFile = VideoDatabase.GetFileId(filename);
      int idMovie = VideoDatabase.GetMovieId(filename);
      int timeMovieStopped = 0;
      byte[] resumeData = null;
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
          if (askForResumeMovie && !filename.EndsWith(@"\BDMV\index.bdmv"))
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
      // If the file is an image file, it should be mounted before playing
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(filename)))
      {
        if (!MountImageFile(GUIWindowManager.ActiveWindow, filename, requestPin))
          return;
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
          msg.Param1 = (int)timeMovieStopped;
          GUIGraphicsContext.SendMessage(msg);
        }
      }
    }

    // obsolete function - not used anymore
    // PlayMountedImageFile(int WindowID, string file)
    /*
    public static bool PlayMountedImageFile(int WindowID, string file)
    {
      Log.Info("GUIVideoFiles: PlayMountedImageFile - {0}", file);
      if (MountImageFile(WindowID, file))
      {
        string strDir = DaemonTools.GetVirtualDrive();

        // Check if the mounted image is actually a DVD. If so, bypass
        // autoplay to play the DVD without user intervention
        if (File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          _playlistPlayer.Reset();
          _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
          PlayList playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
          playlist.Clear();

          PlayListItem newitem = new PlayListItem();
          newitem.FileName = file; //strDir + @"\VIDEO_TS\VIDEO_TS.IFO";
          newitem.Type = PlayListItem.PlayListItemType.Video;
          playlist.Add(newitem);

          Log.Debug("GUIVideoFiles: Autoplaying DVD image mounted on {0}", strDir);
          PlayMovieFromPlayList(true);
          return true;
        }
      }
      return false;
    }
    */

    [Obsolete("This method is obsolete; use method MountImageFile(int WindowID, string file) instead.")]
    public static bool MountImageFile(int WindowID, string file)
    {
      return MountImageFile(WindowID, file, false);
    }

    public static bool MountImageFile(int WindowID, string file, bool requestPin)
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
            dlgYesNo.DoModal(WindowID);
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

    private void doOnPlayBackStoppedOrChanged(g_Player.MediaType type, int timeMovieStopped, string filename,
                                              string caller)
    {
      if (type != g_Player.MediaType.Video || filename.EndsWith("&txe=.wmv"))
      {
        return;
      }

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      int iidMovie = VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iidMovie, ref movies);
      HashSet<string> watchedMovies = new HashSet<string>();

      int playTimePercentage = 0; // Set watched flag after 80% of total played time

      // Stacked movies duration
      if (_isStacked && _totalMovieDuration != 0)
      {
        int duration = 0;

        for (int i = 0; i < _stackedMovieFiles.Count; i++)
        {
          int fileID = VideoDatabase.GetFileId((string)_stackedMovieFiles[i]);

          if (g_Player.CurrentFile != (string)_stackedMovieFiles[i])
          {
            //(int)Math.Ceiling((timeMovieStopped / g_Player.Player.Duration) * 100);
            duration += VideoDatabase.GetMovieDuration(fileID);
            continue;
          }
          playTimePercentage = (100 * (duration + timeMovieStopped) / _totalMovieDuration);
          break;
        }
      }
      else
      {
        if (g_Player.Player.Duration >= 1)
          playTimePercentage = (int)Math.Ceiling((timeMovieStopped / g_Player.Player.Duration) * 100);
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
          VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), true);
        }

        else if ((filename.Trim().ToLower().Equals(strFilePath.Trim().ToLower())) && (timeMovieStopped > 0))
        {
          byte[] resumeData = null;
          g_Player.Player.GetResumeState(out resumeData);
          Log.Info("GUIVideoFiles: {0} idFile={1} timeMovieStopped={2} resumeData={3}", caller, idFile, timeMovieStopped,
                   resumeData);
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, timeMovieStopped, resumeData);
          Log.Debug("GUIVideoFiles: {0} store resume time", caller);

          //Set file "watched" only if 80% or higher played time (share view)
          if (playTimePercentage >= 80)
          {
            watchedMovies.Add(strFilePath);
            VideoDatabase.SetMovieWatchedStatus(VideoDatabase.GetMovieId(strFilePath), true);
          }
        }
        else
        {
          VideoDatabase.DeleteMovieStopTime(idFile);
        }
      }
      if (_markWatchedFiles) // save a little performance
      {
        // Update db view watched status for played movie
        IMDBMovie movie = new IMDBMovie();
        VideoDatabase.GetMovieInfo(filename, ref movie);
        if (!movie.IsEmpty && (playTimePercentage >= 80 || g_Player.IsDVDMenu)) //Flag movie "watched" status only if 80% or higher played time (database view)
        {
          movie.Watched = 1;
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
            g_Player.Pause();
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
      doOnPlayBackStoppedOrChanged(type, timeMovieStopped, filename, "OnPlayBackChanged");
    }

    private void OnPlayBackStopped(g_Player.MediaType type, int timeMovieStopped, string filename)
    {
      doOnPlayBackStoppedOrChanged(type, timeMovieStopped, filename, "OnPlayBackStopped");
    }

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video)
      {
        return;
      }

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      HashSet<string> watchedMovies = new HashSet<string>();

      int iidMovie = VideoDatabase.GetMovieId(filename);
      if (iidMovie >= 0)
      {
        VideoDatabase.GetFiles(iidMovie, ref movies);

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

        if (_isStacked && _totalMovieDuration != 0)
        {
          int duration = 0;

          for (int i = 0; i < _stackedMovieFiles.Count; i++)
          {
            int fileID = VideoDatabase.GetFileId((string)_stackedMovieFiles[i]);

            if (filename != (string)_stackedMovieFiles[i])
            {
              duration += VideoDatabase.GetMovieDuration(fileID);
              continue;
            }
            playTimePercentage = (int)(100 * (duration + g_Player.Player.CurrentPosition) / _totalMovieDuration);
            break;
          }
        }
        else
        {
          playTimePercentage = 100;
        }

        if (playTimePercentage >= 80)
        {
          IMDBMovie details = new IMDBMovie();
          VideoDatabase.GetMovieInfoById(iidMovie, ref details);
          details.Watched = 1;
          VideoDatabase.SetWatched(details);
          VideoDatabase.SetMovieWatchedStatus(iidMovie, true);
        }
      }
    }

    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video)
      {
        return;
      }
      AddFileToDatabase(filename);

      int idFile = VideoDatabase.GetFileId(filename);
      if (idFile != -1)
      {
        int movieDuration = (int)g_Player.Duration;
        VideoDatabase.SetMovieDuration(idFile, movieDuration);
      }
    }

    //
    // Play all files in selected directory
    //
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
          VideoDatabase.DeleteMovieStopTime(idFile);

        // Add file to tmp playlist
        PlayListItem newItem = new PlayListItem();
        newItem.FileName = file;
        // Set file description (for sorting by name -> DVD IFO file problem)
        string description = string.Empty;
        if (file.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
        {
          string dvdFolder = file.Substring(0, file.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
          description = Path.GetFileName(dvdFolder);
        }
        else if (file.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV") >= 0)
        {
          string dvdFolder = file.Substring(0, file.ToUpper().IndexOf(@"\BDMV\INDEX.BDMV"));
          description = Path.GetFileName(dvdFolder);
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

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeLayout.SelectedListItem;
      int itemNo = facadeLayout.SelectedListItemIndex;
      if (item == null)
      {
        return;
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

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
        if ((Path.GetFileName(item.Path) != string.Empty) || Util.Utils.IsDVD(item.Path))
        {
          if (item.IsRemote)
          {
            return;
          }
          if ((item.IsFolder) && (item.Label == ".."))
          {
            return;
          }
          if (Util.Utils.IsDVD(item.Path))
          {
            if (File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO") || File.Exists(item.Path + @"\BDMV\index.bdmv"))
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
          else if (item.IsFolder)
          {
            if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
            {
              dlg.AddLocalizedString(208); //play             
            }
            dlg.AddLocalizedString(926); //Queue
            if (!VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
            {
              //
              // Play all
              //
              SelectDVDHandler checkIfIsDvd = new SelectDVDHandler();
              if (!checkIfIsDvd.IsDvdDirectory(item.Path))
              {
                dlg.AddLocalizedString(1204); // Play All in selected folder
              }
              else
              {
                if (item.IsPlayed)
                {
                  dlg.AddLocalizedString(830); //Reset watched status for DVD folder
                }
              }
              //
              dlg.AddLocalizedString(102); //Scan            
            }
            dlg.AddLocalizedString(368); //IMDB
            if (Util.Utils.getDriveType(item.Path) == 5)
            {
              dlg.AddLocalizedString(654); //Eject            
            }
            if (!IsFolderPinProtected(item.Path) && _fileMenuEnabled)
            {
              dlg.AddLocalizedString(500); // FileMenu            
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

            if (!IsFolderPinProtected(item.Path) && !item.IsRemote && _fileMenuEnabled)
            {
              dlg.AddLocalizedString(500); // FileMenu
            }
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
          ArrayList availablePaths = new ArrayList();
          availablePaths.Add(item.Path);
          IMDBFetcher.ScanIMDB(this, availablePaths, _isFuzzyMatching, _scanSkipExisting, _getActors, false);
          LoadDirectory(_currentFolder);
          break;

        case 830: // Reset watched status
          SetMovieUnwatched(item.Path, item.IsFolder);
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
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
          // Play all
        case 1204:
          {
            if (!_virtualDirectory.RequestPin(item.Path))
            {
              return;
            }
            OnPlayAll(item.Path);
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

    private bool GetUserInputString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = sString;
      keyboard.DoModal(GetID); // show it...
      if (keyboard.IsConfirmed)
      {
        sString = keyboard.Text;
      }
      return keyboard.IsConfirmed;
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
        dlgFile.PreselectDelete();
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

    private static bool IsFolderPinProtected(string folder)
    {
      int pinCode = 0;
      return _virtualDirectory.IsProtectedShare(folder, out pinCode);
    }

    private static void DownloadThumbnail(string folder, string url, string name)
    {
      if (url == null)
      {
        return;
      }
      if (url.Length == 0)
      {
        return;
      }
      string strThumb = Util.Utils.GetCoverArtName(folder, name);
      string LargeThumb = Util.Utils.GetLargeCoverArtName(folder, name);
      if (!Util.Utils.FileExistsInCache(strThumb))
      {
        string strExtension;
        strExtension = Path.GetExtension(url);
        if (strExtension.Length > 0)
        {
          string strTemp = "temp";
          strTemp += strExtension;
          strThumb = Path.ChangeExtension(strThumb, strExtension);
          LargeThumb = Path.ChangeExtension(LargeThumb, strExtension);
          Util.Utils.FileDelete(strTemp);

          Util.Utils.DownLoadImage(url, strTemp);
          if (Util.Utils.FileExistsInCache(strTemp))
          {
            if (Util.Picture.CreateThumbnail(strTemp, strThumb, (int)Thumbs.ThumbResolution,
                                             (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
            {
              Util.Picture.CreateThumbnail(strTemp, LargeThumb, (int)Thumbs.ThumbLargeResolution,
                                           (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
            }
          }
          else
          {
            Log.Debug("GUIVideoFiles: unable to download thumb {0}->{1}", url, strTemp);
          }
          Util.Utils.FileDelete(strTemp);
        }
      }
    }

    private static void DownloadDirector(IMDBMovie movieDetails)
    {
      string actor = movieDetails.Director;
      string strThumb = Util.Utils.GetCoverArtName(Thumbs.MovieActors, actor);
      if (!File.Exists(strThumb))
      {
        _imdb.FindActor(actor);
        IMDBActor imdbActor = new IMDBActor();
        for (int x = 0; x < _imdb.Count; ++x)
        {
          _imdb.GetActorDetails(_imdb[x], true, out imdbActor);
          if (imdbActor.ThumbnailUrl != null && imdbActor.ThumbnailUrl.Length > 0)
          {
            break;
          }
        }
        if (imdbActor.ThumbnailUrl != null)
        {
          if (imdbActor.ThumbnailUrl.Length != 0)
          {
            //ShowProgress(GUILocalizeStrings.Get(1009), actor, "", 0);
            DownloadThumbnail(Thumbs.MovieActors, imdbActor.ThumbnailUrl, actor);
          }
          else
          {
            Log.Debug("GUIVideoFiles: url=empty for director {0}", actor);
          }
        }
        else
        {
          Log.Debug("GUIVideoFiles: url=null for director {0}", actor);
        }
      }
    }

    private static void DownloadActors(IMDBMovie movieDetails)
    {
      char[] splitter = {'\n', ','};
      string[] actors = movieDetails.Cast.Split(splitter);
      if (actors.Length > 0)
      {
        for (int i = 0; i < actors.Length; ++i)
        {
          int percent = (int)(i * 100) / (1 + actors.Length);
          int pos = actors[i].IndexOf(" as ");
          string actor = actors[i];
          if (pos >= 0)
          {
            actor = actors[i].Substring(0, pos);
          }
          actor = actor.Trim();
          string strThumb = Util.Utils.GetCoverArtName(Thumbs.MovieActors, actor);
          if (!File.Exists(strThumb))
          {
            _imdb.FindActor(actor);
            IMDBActor imdbActor = new IMDBActor();
            for (int x = 0; x < _imdb.Count; ++x)
            {
              _imdb.GetActorDetails(_imdb[x], false, out imdbActor);
              if (imdbActor.ThumbnailUrl != null && imdbActor.ThumbnailUrl.Length > 0)
              {
                break;
              }
            }
            if (imdbActor.ThumbnailUrl != null)
            {
              if (imdbActor.ThumbnailUrl.Length != 0)
              {
                int actorId = VideoDatabase.AddActor(actor);
                if (actorId > 0)
                {
                  VideoDatabase.SetActorInfo(actorId, imdbActor);
                }
                //ShowProgress(GUILocalizeStrings.Get(1009), actor, "", percent);
                DownloadThumbnail(Thumbs.MovieActors, imdbActor.ThumbnailUrl, actor);
              }
              else
              {
                Log.Debug("GUIVideoFiles: url=empty for actor {0}", actor);
              }
            }
            else
            {
              Log.Debug("GUIVideoFiles: url=null for actor {0}", actor);
            }
          }
        }
      }
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
              if (item.Path.ToLower().IndexOf("video_ts") >= 0)
              {
                string strFile = String.Format(@"{0}\VIDEO_TS.IFO", item.Path);
                availableFiles.Add(strFile);
              }
              else if (item.Path.ToLower().IndexOf("bdmv") >= 0)
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

    private void _downloadSubtitles()
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
        string[] sub_exts = {
                              ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss",
                              ".ass", ".idx", ".ifo"
                            };
        if (!isDVD || path.ToUpper().IndexOf("BDMV") < 0)
        {
          // check if movie has subtitles
          for (int i = 0; i < sub_exts.Length; i++)
          {
            for (int x = 0; x < listFiles.Count; ++x)
            {
              if (listFiles[x].IsFolder)
              {
                continue;
              }
              string subTitleFileName = listFiles[x].Path;
              subTitleFileName = Path.ChangeExtension(subTitleFileName, sub_exts[i]);
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
      catch (ThreadAbortException) {}
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

      if (_isStacked)
      {
        selectedFileIndex = 0;
      }

      ArrayList movies = new ArrayList();
      VideoDatabase.GetFiles(idMovie, ref movies);
      if (movies.Count <= 0)
      {
        return;
      }
      
      if (!CheckMovie(idMovie))
      {
        return;
      }

      bool askForResumeMovie = true;
      int movieDuration = 0;

      //get all movies belonging to each other
      List<GUIListItem> items = new List<GUIListItem>();
      
      // Pin protection
      if (!requestPin)
      {
        items = _virtualDirectory.GetDirectoryUnProtectedExt(Path.GetDirectoryName((string)movies[0]), false);
      }
      else
      {
        items = _virtualDirectory.GetDirectoryExt(Path.GetDirectoryName((string)movies[0]));
      }

      if (items.Count <= 1)
      {
        return; // first item always ".." so 1 item means protected share
      }

      //check if we can resume 1 of those movies
      int timeMovieStopped = 0;
      bool asked = false;
      ArrayList newItems = new ArrayList();
      
      for (int i = 0; i < items.Count; ++i)
      {
        GUIListItem temporaryListItem = (GUIListItem)items[i];
        bool found = false;

        // reduce number of items to movie files only
        foreach (var movie in movies)
        {
          if (movie.ToString() == items[i].Path)
          {
            found = true;
            break;
          }
        }

        if (!found)
          continue;

        if ((Util.Utils.ShouldStack(temporaryListItem.Path, (string)movies[0])) && (movies.Count > 1))
        {
          if (!asked)
          {
            selectedFileIndex++;
          }
          IMDBMovie movieDetails = new IMDBMovie();
          int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);
          if ((idMovie >= 0) && (idFile >= 0))
          {
            VideoDatabase.GetMovieInfo((string)movies[0], ref movieDetails);
            string title = Path.GetFileName((string)movies[0]);
            if ((VirtualDirectory.IsValidExtension((string)movies[0], Util.Utils.VideoExtensions, false)))
            {
              Util.Utils.RemoveStackEndings(ref title);
            }
            if (movieDetails.Title != string.Empty)
            {
              title = movieDetails.Title;
            }

            timeMovieStopped = VideoDatabase.GetMovieStopTime(idFile);
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
                  newItems.Add(temporaryListItem);
                }
                else
                {
                  askForResumeMovie = false;
                  newItems.Add(temporaryListItem);
                }
              } //if (!asked)
              else
              {
                newItems.Add(temporaryListItem);
              }
            } //if (timeMovieStopped>0)
            else
            {
              newItems.Add(temporaryListItem);
            }

            // Total movie duration
            movieDuration += VideoDatabase.GetMovieDuration(idFile);
          }
          else //if (idMovie >=0)
          {
            newItems.Add(temporaryListItem);
          }
        } //if ( MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, item.Path))
      }

      // If we have found stackable items, clear the movies array
      // so, that we can repopulate it.
      if (newItems.Count > 0)
      {
        movies.Clear();
      }

      for (int i = 0; i < newItems.Count; ++i)
      {
        GUIListItem temporaryListItem = (GUIListItem)newItems[i];
        if (Util.Utils.IsVideo(temporaryListItem.Path) && !PlayListFactory.IsPlayList(temporaryListItem.Path))
        {
          movies.Add(temporaryListItem.Path);
        }
      }

      if (movies.Count > 1)
      {
        if (askForResumeMovie)
        {
          GUIDialogFileStacking dlg =
            (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_FILESTACKING);
          if (null != dlg)
          {
            dlg.SetFiles(movies);
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
      for (int i = selectedFileIndex - 1; i < movies.Count; ++i)
      {
        string movieFileName = (string)movies[i];
        PlayListItem newitem = new PlayListItem();
        newitem.FileName = movieFileName;
        newitem.Type = PlayListItem.PlayListItemType.Video;
        playlist.Add(newitem);
      }

      // play movie...
      PlayMovieFromPlayList(askForResumeMovie, requestPin);
    }

    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're busy querying www.imdb.com
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(197), scanningFileNumber, scanningFileTotal);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
      else
      {
        // show dialog...
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        pDlgOK.SetHeading(195);
        pDlgOK.SetLine(1, fetcher.MovieName);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
        return true;
      }
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the movie info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(198), scanningFileNumber, scanningFileTotal);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), scanningFileNumber, scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(986);
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

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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
      else
      {
        // show dialog...
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        // show dialog...
        pDlgOK.SetHeading(195);
        pDlgOK.SetLine(1, fetcher.MovieName);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
        return false;
      }
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        movieName = string.Empty;
        return false;
      }
      else
      {
        movieName = fetcher.MovieName;
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
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        selectedMovie = -1;
        return false;
      }
      else
      {
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        // more then 1 movie found
        // ask user to select 1
        pDlgSelect.Reset();
        pDlgSelect.SetHeading(196); //select movie
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
        else
        {
          return true;
        }
      }
    }

    public bool OnScanStart(int total)
    {
      _scanning = true;
      _conflictFiles.Clear();
      scanningFileTotal = total;
      scanningFileNumber = 1;
      return true;
    }

    public bool OnScanEnd()
    {
      _scanning = false;
      if (_conflictFiles.Count > 0)
      {
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        // more than 1 movie found
        // ask user to select 1
        do
        {
          pDlgSelect.Reset();
          pDlgSelect.SetHeading(892); //select movie
          for (int i = 0; i < _conflictFiles.Count; ++i)
          {
            IMDBMovie currentMovie = (IMDBMovie)_conflictFiles[i];
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
          IMDBMovie movieDetails = (IMDBMovie)_conflictFiles[selectedMovie];
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
              if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, _getActors))
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
      scanningFileNumber = count;
      return true;
    }

    public bool OnScanIterated(int count)
    {
      scanningFileNumber = count;
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
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