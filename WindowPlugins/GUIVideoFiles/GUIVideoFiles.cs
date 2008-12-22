#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *  Copyright (C) 2005-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Video.Database;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Services;
using MediaPortal.Dialogs;


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
        _SortBy = 0;//name
        _ViewAs = 0;//list
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

    static IMDB _imdb;
    static bool _askBeforePlayingDVDImage = false;
    static VirtualDirectory _virtualDirectory;
    static PlayListPlayer _playlistPlayer;

    MapSettings _mapSettings = new MapSettings();
    DirectoryHistory _history = new DirectoryHistory();
    string _virtualStartDirectory = string.Empty;
    int _currentSelectedItem = -1;

    // File menu
    string _fileMenuDestinationDir = string.Empty;
    bool _fileMenuEnabled = false;
    string _fileMenuPinCode = string.Empty;

    bool _scanning = false;
    int scanningFileNumber = 1;
    int scanningFileTotal = 1;
    bool _isFuzzyMatching = false;
    bool _scanSkipExisting = false;
    bool _getActors = true;
    bool _markWatchedFiles = true;
    bool _eachFolderIsMovie = false;
    ArrayList _conflictFiles = new ArrayList();

    #endregion

    #region constructors

    static GUIVideoFiles()
    {
      _playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public GUIVideoFiles()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIDEOS;
    }

    #endregion

    #region BaseWindow Members
    protected override bool CurrentSortAsc
    {
      get
      {
        return _mapSettings.SortAscending;
      }
      set
      {
        _mapSettings.SortAscending = value;
      }
    }
    protected override VideoSort.SortMethod CurrentSortMethod
    {
      get
      {
        return (VideoSort.SortMethod)_mapSettings.SortBy;
      }
      set
      {
        _mapSettings.SortBy = (int)value;
      }
    }
    protected override View CurrentView
    {
      get
      {
        return (View)_mapSettings.ViewAs;
      }
      set
      {
        _mapSettings.ViewAs = (int)value;
      }
    }

    public override bool Init()
    {
      _imdb = new IMDB(this);
      g_Player.PlayBackStopped += new MediaPortal.Player.g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackChanged += new MediaPortal.Player.g_Player.ChangedHandler(OnPlayBackChanged);
      // _currentFolder = null;

      LoadSettings();
      bool result = Load(GUIGraphicsContext.Skin + @"\myVideo.xml");
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        VideoState.StartWindow = xmlreader.GetValueAsInt("movies", "startWindow", GetID);
        VideoState.View = xmlreader.GetValueAsString("movies", "startview", "369");

      }
      return result;
    }

    #region Serialisation
    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _isFuzzyMatching = xmlreader.GetValueAsBool("movies", "fuzzyMatching", false);
        _scanSkipExisting = xmlreader.GetValueAsBool("moviedatabase", "scanskipexisting", false);
        _getActors = xmlreader.GetValueAsBool("moviedatabase", "getactors", true);
        _markWatchedFiles = xmlreader.GetValueAsBool("movies", "markwatched", true);
        _eachFolderIsMovie = xmlreader.GetValueAsBool("movies", "eachFolderIsMovie", false);
        _fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        _fileMenuPinCode = MediaPortal.Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));

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
          if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(lastFolder)))
          {
            lastFolder = "root";
          }
          if (lastFolder != "root")
            _currentFolder = lastFolder;
        }
      }
      
      if (_currentFolder.Length > 0)
      {
        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(_currentFolder);

        while (dirInfo.Parent != null)
        {
          string dirName = dirInfo.Name;
          dirInfo = dirInfo.Parent;
          string currentParentFolder = @dirInfo.FullName;
          _history.Set(dirName, currentParentFolder);                
        }
      }
    }


    #endregion

    protected override bool AllowView(View view)
    {
      return base.AllowView(view);
    }

    public override void OnAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) && (facadeView.Focus))
      {
        GUIListItem item = facadeView[0];
        if ((item != null) && item.IsFolder && (item.Label == "..") && (_currentFolder != _virtualStartDirectory))
        {
          LoadDirectory(item.Path);
          return;
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];
        if ((item != null) && item.IsFolder && (item.Label == ".."))
          LoadDirectory(item.Path);
        return;
      }

      if (action.wID == Action.ActionType.ACTION_DELETE_ITEM)
      {
        GUIListItem item = facadeView.SelectedListItem;
        if (item != null)
        {
          if (item.IsFolder == false)
          {
            OnDeleteItem(item);
            return;
          }
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        Reset();
      }
      if (VideoState.StartWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(VideoState.StartWindow);
        return;
      }
      LoadFolderSettings(_currentFolder);
      LoadDirectory(_currentFolder);
    }


    protected override void OnPageDestroy(int newWindowId)
    {
      _currentSelectedItem = facadeView.SelectedListItemIndex;
      SaveFolderSettings(_currentFolder);
      base.OnPageDestroy(newWindowId);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
          if (g_Player.Playing && g_Player.IsDVD &&
              message.Label.Equals(g_Player.CurrentFile.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase))   // test if it is our drive
          {
            Log.Info("GUIVideoFiles: Stop dvd since DVD is ejected");
            g_Player.Stop();
          }

          if (GUIWindowManager.ActiveWindow == GetID)
          {
            if (MediaPortal.Util.Utils.IsDVD(_currentFolder))
            {
              _currentFolder = string.Empty;
              LoadDirectory(_currentFolder);
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          facadeView.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:

          facadeView.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
          // Make sure file view is the current window
          if (VideoState.StartWindow != GetID)
          {
            VideoState.StartWindow = GetID;
            GUIVideoFiles.Reset();
            GUIWindowManager.ReplaceWindow(GetID);
          }
          _currentFolder = message.Label;
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
      }
      return base.OnMessage(message);
    }



    void LoadFolderSettings(string folderName)
    {
      if (folderName == string.Empty) folderName = "root";
      object o;
      FolderSettings.GetFolderSetting(folderName, "VideoFiles", typeof(GUIVideoFiles.MapSettings), out o);
      if (o != null)
      {
        _mapSettings = o as MapSettings;
        if (_mapSettings == null) _mapSettings = new MapSettings();
        CurrentSortAsc = _mapSettings.SortAscending;
        CurrentSortMethod = (VideoSort.SortMethod)_mapSettings.SortBy;
        currentView = (View)_mapSettings.ViewAs;
      }
      else
      {
        Share share = _virtualDirectory.GetShare(folderName);
        if (share != null)
        {
          if (_mapSettings == null) _mapSettings = new MapSettings();
          CurrentSortAsc = _mapSettings.SortAscending;
          CurrentSortMethod = (VideoSort.SortMethod)_mapSettings.SortBy;
          currentView = (View) share.DefaultView;
          CurrentView = (View)share.DefaultView;
        }
      }

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        if (xmlreader.GetValueAsBool("movies", "rememberlastfolder", false))
          xmlreader.SetValue("movies", "lastfolder", folderName);

      SwitchView();
      UpdateButtonStates();
    }

    void SaveFolderSettings(string folderName)
    {
      if (folderName == string.Empty) folderName = "root";
      FolderSettings.AddFolderSetting(folderName, "VideoFiles", typeof(GUIVideoFiles.MapSettings), _mapSettings);
    }

    protected override void LoadDirectory(string newFolderName)
    {
      if (newFolderName == null) return;

      GUIWaitCursor.Show();

      //newFolderName = System.IO.Path.GetDirectoryName(newFolderName);
      // Mounting and loading a DVD image file takes a long time,
      // so display a message letting the user know that something 
      // is happening.
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(newFolderName)))
      {
        // hide it before playback since it would be on top of the "play from that point" dialog
        GUIWaitCursor.Hide();

        if (PlayMountedImageFile(GetID, newFolderName))
        {
          return;
        }
        else
        {
          if (DaemonTools.IsMounted(newFolderName))   // caused mantis bug 1444: ISO playing problems
            newFolderName = DaemonTools.GetVirtualDrive() + @"\";
          else
            return;
        }
      }

      GUIListItem selectedListItem = facadeView.SelectedListItem;
      if (selectedListItem != null)
        if (selectedListItem.IsFolder && selectedListItem.Label != "..")
          _history.Set(selectedListItem.Label, _currentFolder);

      if (newFolderName != _currentFolder && _mapSettings != null)
        SaveFolderSettings(_currentFolder);

      if (newFolderName != _currentFolder || _mapSettings == null)
        LoadFolderSettings(newFolderName);

      _currentFolder = newFolderName;

      string objectCount = string.Empty;

      ArrayList itemlist = new ArrayList();
      GUIControl.ClearControl(GetID, facadeView.GetID);

      // here we get ALL files in every subdir, look for folderthumbs, defaultthumbs, etc
      itemlist = _virtualDirectory.GetDirectory(_currentFolder);
      if (_mapSettings.Stack)
      {
        ArrayList itemfiltered = new ArrayList();
        for (int x = 0; x < itemlist.Count; ++x)
        {
          bool addItem = true;
          GUIListItem item1 = (GUIListItem)itemlist[x];
          for (int y = 0; y < itemlist.Count; ++y)
          {
            GUIListItem item2 = (GUIListItem)itemlist[y];
            if (x != y)
            {
              if (!item1.IsFolder || !item2.IsFolder)
              {
                if (!item1.IsRemote && !item2.IsRemote)
                {
                  if (MediaPortal.Util.Utils.ShouldStack(item1.Path, item2.Path))
                  {
                    if (String.Compare(item1.Path, item2.Path, true) > 0)
                    {
                      addItem = false;
                      // Update to reflect the stacked size
                      item2.FileInfo.Length += item1.FileInfo.Length;
                    }
                  }
                }
              }
            }
          }

          if (addItem)
          {
            string label = item1.Label;
            if ((VirtualDirectory.IsValidExtension(item1.Path, MediaPortal.Util.Utils.VideoExtensions, false)))
              MediaPortal.Util.Utils.RemoveStackEndings(ref label);
            item1.Label = label;
            itemfiltered.Add(item1);
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
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
      }
      OnSort();

      bool itemSelected = false;

      if (selectedListItem != null)
      {
        string selectedItemLabel = _history.Get(_currentFolder);
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          if (item.Label == selectedItemLabel)
          {
            GUIControl.SelectItemControl(GetID, facadeView.GetID, i);
            itemSelected = true;
            break;
          }
        }
      }
      int totalItems = itemlist.Count;
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = (GUIListItem)itemlist[0];
        if (rootItem.Label == "..") totalItems--;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(totalItems));

      if (!itemSelected)
        SelectCurrentItem();

      GUIWaitCursor.Hide();
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null) return;
      bool isFolderAMovie = false;
      string path = item.Path;

      if (item.IsFolder && !item.IsRemote)
      {
        // Check if folder is actually a DVD. If so don't browse this folder, but play the DVD!
        if ((System.IO.File.Exists(path + @"\VIDEO_TS\VIDEO_TS.IFO")) && (item.Label != ".."))
        {
          isFolderAMovie = true;
          path = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else
        {
          isFolderAMovie = false;
        }
      }

      if ((item.IsFolder) && (!isFolderAMovie))
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
            if (!_virtualDirectory.ShouldWeDownloadFile(path)) return;
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
          if (!_virtualDirectory.IsRemoteFileDownloaded(path, item.FileInfo.Length)) return;
        }
        string movieFileName = path;
        movieFileName = _virtualDirectory.GetLocalFilename(movieFileName);

        // Set selected item
        _currentSelectedItem = facadeView.SelectedListItemIndex;
        if (PlayListFactory.IsPlayList(movieFileName))
        {
          LoadPlayList(movieFileName);
          return;
        }

        if (!CheckMovie(movieFileName)) return;
        bool askForResumeMovie = true;
        if (_mapSettings.Stack)
        {
          int selectedFileIndex = 0;
          int movieDuration = 0;
          ArrayList movies = new ArrayList();
          {
            //get all movies belonging to each other
            ArrayList items = _virtualDirectory.GetDirectoryUnProtected(_currentFolder, true);

            //check if we can resume 1 of those movies
            int timeMovieStopped = 0;
            bool asked = false;
            ArrayList newItems = new ArrayList();
            for (int i = 0; i < items.Count; ++i)
            {
              GUIListItem temporaryListItem = (GUIListItem)items[i];
              if (MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, path))
              {
                if (!asked) selectedFileIndex++;
                IMDBMovie movieDetails = new IMDBMovie();
                int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);
                int idMovie = VideoDatabase.GetMovieId(path);
                if ((idMovie >= 0) && (idFile >= 0))
                {
                  VideoDatabase.GetMovieInfo(path, ref movieDetails);
                  string title = System.IO.Path.GetFileName(path);
                  if ((VirtualDirectory.IsValidExtension(path, MediaPortal.Util.Utils.VideoExtensions, false))) MediaPortal.Util.Utils.RemoveStackEndings(ref title);
                  if (movieDetails.Title != string.Empty) title = movieDetails.Title;

                  timeMovieStopped = VideoDatabase.GetMovieStopTime(idFile);
                  if (timeMovieStopped > 0)
                  {
                    if (!asked)
                    {
                      asked = true;
                      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                      if (null == dlgYesNo) return;
                      dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                      dlgYesNo.SetLine(1, title);
                      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(movieDuration + timeMovieStopped));
                      dlgYesNo.SetDefaultToYes(true);
                      dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                      if (dlgYesNo.IsConfirmed)
                      {
                        askForResumeMovie = false;
                        newItems.Add(temporaryListItem);
                      }
                      else
                      {
                        VideoDatabase.DeleteMovieStopTime(idFile);
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
              }//if ( MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, path))
            }

            for (int i = 0; i < newItems.Count; ++i)
            {
              GUIListItem temporaryListItem = (GUIListItem)newItems[i];
              if (MediaPortal.Util.Utils.IsVideo(temporaryListItem.Path) && !PlayListFactory.IsPlayList(temporaryListItem.Path))
              {
                if (MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, path))
                {
                  movies.Add(temporaryListItem.Path);
                }
              }
            }
            if (movies.Count == 0)
            {
              movies.Add(movieFileName);
            }
          }
          if (movies.Count <= 0) return;
          if (movies.Count > 1)
          {
            //TODO
            movies.Sort();
            for (int i = 0; i < movies.Count; ++i)
            {
              AddFileToDatabase((string)movies[i]);
            }

            if (askForResumeMovie)
            {
              GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
              if (null != dlg)
              {
                dlg.SetNumberOfFiles(movies.Count);
                dlg.DoModal(GetID);
                selectedFileIndex = dlg.SelectedFile;
                if (selectedFileIndex < 1) return;
              }
            }
          }
          else if (movies.Count == 1)
          {
            AddFileToDatabase((string)movies[0]);
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
            itemNew.Type = Playlists.PlayListItem.PlayListItemType.Video;
            playlist.Add(itemNew);
          }

          // play movie...
          PlayMovieFromPlayList(askForResumeMovie, selectedFileIndex - 1);
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
        NewItem.Type = Playlists.PlayListItem.PlayListItemType.Video;
        newPlayList.Add(NewItem);
        PlayMovieFromPlayList(true);
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
      GUIListItem listItem = facadeView[itemIndex];

      if (listItem == null) return;
      if (listItem.IsRemote) return;
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
      GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex + 1);

    }

    protected override void AddItemToPlayList(GUIListItem listItem)
    {
      if (listItem.IsFolder)
      {
        // recursive
        if (listItem.Label == "..") return;
        // Mounting and loading a DVD image file takes a long time,
        // so display a message letting the user know that something 
        // is happening.
        if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(listItem.Path)))
        {
          if (MountImageFile(GetID, listItem.Path))
          {
            string strDir = DaemonTools.GetVirtualDrive();

            // Check if the mounted image is actually a DVD. If so, bypass
            // autoplay to play the DVD without user intervention
            if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
            {
              PlayListItem newitem = new PlayListItem();
              newitem.FileName = strDir + @"\VIDEO_TS\VIDEO_TS.IFO";
              newitem.Description = listItem.Label;
              newitem.Duration = listItem.Duration;
              newitem.Type = Playlists.PlayListItem.PlayListItemType.Video;
              _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(newitem);
            }
          }
          return;
        }
        ArrayList itemlist = _virtualDirectory.GetDirectoryUnProtected(listItem.Path, true);
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
      }
      else
      {
        //TODO
        if (MediaPortal.Util.Utils.IsVideo(listItem.Path) && !PlayListFactory.IsPlayList(listItem.Path))
        {
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.FileName = listItem.Path;
          playlistItem.Description = listItem.Label;
          playlistItem.Duration = listItem.Duration;
          playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Video;
          _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(playlistItem);
        }
      }
    }

    void AddFileToDatabase(string strFile)
    {
      if (!MediaPortal.Util.Utils.IsVideo(strFile)) return;
      //if (  MediaPortal.Util.Utils.IsNFO(strFile)) return;
      if (PlayListFactory.IsPlayList(strFile)) return;

      if (!VideoDatabase.HasMovieInfo(strFile))
      {
        ArrayList allFiles = new ArrayList();
        ArrayList items = _virtualDirectory.GetDirectoryUnProtected(_currentFolder, true);
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          if (temporaryListItem.IsFolder) continue;
          if (temporaryListItem.Path != strFile)
          {
            if (MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, strFile))
            {
              allFiles.Add(items[i]);
            }
          }
        }
        int iidMovie = VideoDatabase.AddMovieFile(strFile);
        foreach (GUIListItem item in allFiles)
        {
          string strPath, strFileName;

          MediaPortal.Database.DatabaseUtility.Split(item.Path, out strPath, out strFileName);
          MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strPath);
          MediaPortal.Database.DatabaseUtility.RemoveInvalidChars(ref strFileName);
          int pathId = VideoDatabase.AddPath(strPath);
          VideoDatabase.AddFile(iidMovie, pathId, strFileName);
        }
      }
    }

    protected override void OnInfo(int iItem)
    {
      _currentSelectedItem = facadeView.SelectedListItemIndex;
      GUIListItem pItem = facadeView.SelectedListItem;
      if (pItem == null) return;
      if (pItem.IsRemote) return;
      if (!_virtualDirectory.RequestPin(pItem.Path))
      {
        return;
      }
      string strFile = pItem.Path;
      string strMovie = pItem.Label;
      bool bFoundFile = true;
      if ((pItem.IsFolder) && (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(strFile))))
      {
        if (MountImageFile(GetID, strFile))
        {
          string strDir = DaemonTools.GetVirtualDrive();
          // Check if the mounted image is actually a DVD. If so, bypass
          // autoplay to play the DVD without user intervention
          if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            strMovie = MediaPortal.Util.Utils.GetDriveName(strDir);
          }
        }
      }
      else if ((pItem.IsFolder) && (!MediaPortal.Util.Utils.IsDVD(pItem.Path)))
      {
        if (pItem.Label == "..") return;
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
          strMovie = System.IO.Path.GetFileName(dvdFolder);
        }
        else
        {
          //Movie 
          strMovie = System.IO.Path.GetFileNameWithoutExtension(strFile);
        }
      }
      // Use DVD label as movie name
      if (MediaPortal.Util.Utils.IsDVD(pItem.Path) && (pItem.DVDLabel != string.Empty))
      {
        if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          strFile = pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        strMovie = pItem.DVDLabel;
      }
      IMDBMovie movieDetails = new IMDBMovie();
      if ((VideoDatabase.GetMovieInfo(strFile, ref movieDetails) == -1) ||
          (movieDetails.IsEmpty))
      {
        if (bFoundFile)
        {
          AddFileToDatabase(strFile);
        }
        movieDetails.SearchString = strMovie;
        movieDetails.File = System.IO.Path.GetFileName(strFile);
        if (movieDetails.File == string.Empty)
        {
          movieDetails.Path = strFile;
        }
        else
        {
          movieDetails.Path = strFile.Substring(0, strFile.IndexOf(movieDetails.File) - 1);
        }
        Log.Info("GUIVideoFiles: IMDB search: {0}, file:{1}, path:{2}", movieDetails.SearchString, movieDetails.File, movieDetails.Path);
        if (!IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, _getActors))
        {
          return;
        }
      }
      GUIVideoInfo videoInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);
      videoInfo.Movie = movieDetails;
      if (pItem.IsFolder)
      {
        videoInfo.FolderForThumbs = pItem.Path;
      }
      else
      {
        videoInfo.FolderForThumbs = string.Empty;
      }
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);
    }

    #endregion

    private void SetMovieUnwatched(string movieFileName)
    {
      if (VideoDatabase.HasMovieInfo(movieFileName))
      {
        IMDBMovie movieDetails = new IMDBMovie();
        int idMovie = VideoDatabase.GetMovieInfo(movieFileName, ref movieDetails);
        movieDetails.Watched = 0;
        VideoDatabase.SetWatched(movieDetails);
      }
      int idFile = VideoDatabase.GetFileId(movieFileName);
      VideoDatabase.DeleteMovieStopTime(idFile);
    }

    public bool CheckMovie(string movieFileName)
    {
      if (!VideoDatabase.HasMovieInfo(movieFileName)) return true;

      IMDBMovie movieDetails = new IMDBMovie();
      int idMovie = VideoDatabase.GetMovieInfo(movieFileName, ref movieDetails);
      if (idMovie < 0) return true;
      return CheckMovie(idMovie);
    }

    public static bool CheckMovie(int idMovie)
    {
      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);

      if (!MediaPortal.Util.Utils.IsDVD(movieDetails.Path)) return true;
      string cdlabel = string.Empty;
      cdlabel = MediaPortal.Util.Utils.GetDriveSerial(movieDetails.Path);
      if (cdlabel.Equals(movieDetails.CDLabel)) return true;

      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlg == null) return true;
      while (true)
      {
        dlg.SetHeading(428);
        dlg.SetLine(1, 429);
        dlg.SetLine(2, movieDetails.DVDLabel);
        dlg.SetLine(3, movieDetails.Title);
        dlg.DoModal(GUIWindowManager.ActiveWindow);
        if (dlg.IsConfirmed)
        {
          if (movieDetails.CDLabel.StartsWith("nolabel"))
          {
            ArrayList movies = new ArrayList();
            VideoDatabase.GetFiles(idMovie, ref movies);
            if (System.IO.File.Exists(/*movieDetails.Path+movieDetails.File*/(string)movies[0]))
            {
              cdlabel = MediaPortal.Util.Utils.GetDriveSerial(movieDetails.Path);
              VideoDatabase.UpdateCDLabel(movieDetails, cdlabel);
              movieDetails.CDLabel = cdlabel;
              return true;
            }
          }
          else
          {
            cdlabel = MediaPortal.Util.Utils.GetDriveSerial(movieDetails.Path);
            if (cdlabel.Equals(movieDetails.CDLabel)) return true;
          }
        }
        else break;
      }
      return false;
    }

    public static void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
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
      if (pathIsDirectory)
      {
        try
        {
          string[] files = System.IO.Directory.GetFiles(path, "*.xml");
          if (files.Length > 0)
            xmlFile = files[0];
        }
        catch (Exception) { } // user might not have enough rights to access all files
      }
      else
        xmlFile = System.IO.Path.ChangeExtension(path, ".xml");

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
      IMDBMovie info = new IMDBMovie();
      bool isDirectory = false;
      try
      {
        if (System.IO.Directory.Exists(path))
        {
          isDirectory = true;
          string[] files = System.IO.Directory.GetFiles(path);
          foreach (string file in files)
          {
            IMDBMovie movie = new IMDBMovie();
            VideoDatabase.GetMovieInfo(file, ref movie);
            if (!movie.IsEmpty)
            {
              info = movie;
              break;
            }
          }
        }
        else
          VideoDatabase.GetMovieInfo(path, ref info);

        if (info.IsEmpty)
          FetchMatroskaInfo(path, isDirectory, ref info);

        info.SetProperties();
      }
      catch (Exception) { }
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item.Label != "..")
        SetMovieProperties(item.Path);
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
        return;

      if (item.Label == "..")
      {
        filmstrip.InfoImageFileName = string.Empty;
        return;
      }
      else
        filmstrip.InfoImageFileName = item.ThumbnailImage;
    }

    public static void PlayMovieFromPlayList(bool askForResumeMovie)
    {
      PlayMovieFromPlayList(askForResumeMovie, -1);
    }

    public static void PlayMovieFromPlayList(bool askForResumeMovie, int iMovieIndex)
    {
      string filename;
      if (iMovieIndex == -1)
        filename = _playlistPlayer.GetNext();
      else
        filename = _playlistPlayer.Get(iMovieIndex);

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
          string title = System.IO.Path.GetFileName(filename);
          VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
          if (movieDetails.Title != string.Empty) title = movieDetails.Title;

          if (askForResumeMovie)
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, title);
            dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(timeMovieStopped));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (!dlgYesNo.IsConfirmed) timeMovieStopped = 0;
          }
        }
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

    public static bool PlayMountedImageFile(int WindowID, string file)
    {
      Log.Info("GUIVideoFiles: PlayMountedImageFile - {0}", file);
      if (MountImageFile(WindowID, file))
      {
        string strDir = DaemonTools.GetVirtualDrive();
        
        // Check if the mounted image is actually a DVD. If so, bypass
        // autoplay to play the DVD without user intervention
        if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          _playlistPlayer.Reset();
          _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
          PlayList playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
          playlist.Clear();

          PlayListItem newitem = new PlayListItem();
          newitem.FileName = strDir + @"\VIDEO_TS\VIDEO_TS.IFO";
          newitem.Type = Playlists.PlayListItem.PlayListItemType.Video;
          playlist.Add(newitem);

          Log.Debug("GUIVideoFiles: Autoplaying DVD image mounted on {0}", strDir);
          PlayMovieFromPlayList(true);
          return true;
        }
      }
      return false;
    }

    public static bool MountImageFile(int WindowID, string file)
    {
      Log.Debug("GUIVideoFiles: MountImageFile");
      if (!DaemonTools.IsMounted(file))
      {
        if (_askBeforePlayingDVDImage)
        {
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (dlgYesNo != null)
          {
            dlgYesNo.SetHeading(713);
            dlgYesNo.SetLine(1, 531);
            dlgYesNo.DoModal(WindowID);
            if (!dlgYesNo.IsConfirmed) return false;
          }
        }
        GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
        if (dlgProgress != null)
        {
          dlgProgress.Reset();
          dlgProgress.SetHeading(13013);
          dlgProgress.SetLine(1, System.IO.Path.GetFileNameWithoutExtension(file));
          dlgProgress.StartModal(WindowID);
          dlgProgress.Progress();
          if (dlgProgress != null) dlgProgress.Close();
        }
        ArrayList items = _virtualDirectory.GetDirectory(file);
        if (items.Count == 1 && file != string.Empty) return false; // protected share, with wrong pincode
      }
      return DaemonTools.IsMounted(file);
    }


    private void doOnPlayBackStoppedOrChanged(MediaPortal.Player.g_Player.MediaType type, int timeMovieStopped, string filename, string caller)
    {
      if (type != g_Player.MediaType.Video || filename.EndsWith("&txe=.wmv"))
        return;

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      int iidMovie = VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iidMovie, ref movies);
      if (movies.Count <= 0)
        return;
      for (int i = 0; i < movies.Count; i++)
      {
        string strFilePath = (string)movies[i];
        int idFile = VideoDatabase.GetFileId(strFilePath);
        if (idFile < 0)
          break;
        if ((filename == strFilePath) && (timeMovieStopped > 0))
        {
          byte[] resumeData = null;
          g_Player.Player.GetResumeState(out resumeData);
          Log.Info("GUIVideoFiles: {0} idFile={1} timeMovieStopped={2} resumeData={3}", caller, idFile, timeMovieStopped, resumeData);
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, timeMovieStopped, resumeData);
          Log.Debug("GUIVideoFiles: {0} store resume time", caller);
        }
        else
          VideoDatabase.DeleteMovieStopTime(idFile);
      }
      if (_markWatchedFiles) // save a little performance
      {
        // only reload the share if we're watching it.
        if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO && GUIWindowManager.ActiveWindow == GetID)
        {
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
        }
        else
          Log.Debug("GUIVideoFiles: No LoadDirectory needed {0}", caller);
      }
    }

    private void OnPlayBackChanged(MediaPortal.Player.g_Player.MediaType type, int timeMovieStopped, string filename)
    {
      doOnPlayBackStoppedOrChanged(type, timeMovieStopped, filename, "OnPlayBackChanged");
    }

    private void OnPlayBackStopped(MediaPortal.Player.g_Player.MediaType type, int timeMovieStopped, string filename)
    {
      doOnPlayBackStoppedOrChanged(type, timeMovieStopped, filename, "OnPlayBackStopped");
    }

    private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video)
        return;

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
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
            break;
          // Set resumedata to zero
          VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, 0, resumeData);

        }

        IMDBMovie details = new IMDBMovie();
        VideoDatabase.GetMovieInfoById(iidMovie, ref details);
        details.Watched++;
        VideoDatabase.SetWatched(details);
      }
      if (_markWatchedFiles) // save a little performance
      {
        if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO && GUIWindowManager.ActiveWindow == GetID)
        {
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
        }
        else
          Log.Debug("GUIVideoFiles: No LoadDirectory needed OnPlaybackEnded");
      }
    }

    private void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video) return;
      AddFileToDatabase(filename);

      int idFile = VideoDatabase.GetFileId(filename);
      if (idFile != -1)
      {
        int movieDuration = (int)g_Player.Duration;
        VideoDatabase.SetMovieDuration(idFile, movieDuration);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;
      int itemNo = facadeView.SelectedListItemIndex;
      if (item == null) return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (!facadeView.Focus)
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
        if ((System.IO.Path.GetFileName(item.Path) != string.Empty) || MediaPortal.Util.Utils.IsDVD(item.Path))
        {
          if (item.IsRemote) return;
          if ((item.IsFolder) && (item.Label == ".."))
          {
            return;
          }
          if (MediaPortal.Util.Utils.IsDVD(item.Path))
          {
            if (System.IO.File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
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
            if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(item.Path)))
              dlg.AddLocalizedString(208); //play             
            dlg.AddLocalizedString(926); //Queue
            if (!VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(item.Path)))
              dlg.AddLocalizedString(102); //Scan            
            dlg.AddLocalizedString(368); //IMDB
            if (MediaPortal.Util.Utils.getDriveType(item.Path) != 5)
              dlg.AddLocalizedString(925); //delete            
            else
              dlg.AddLocalizedString(654); //Eject            
            if (!IsFolderPinProtected(item.Path) && _fileMenuEnabled)
              dlg.AddLocalizedString(500); // FileMenu            
          }
          else
          {
            dlg.AddLocalizedString(208); //Play
            dlg.AddLocalizedString(926); //Queue
            dlg.AddLocalizedString(368); //IMDB
            if (item.IsPlayed)
              dlg.AddLocalizedString(830); //Reset watched status
            if (MediaPortal.Util.Utils.getDriveType(item.Path) != 5)
              dlg.AddLocalizedString(925); //Delete
            if (!IsFolderPinProtected(item.Path) && !item.IsRemote && _fileMenuEnabled)
              dlg.AddLocalizedString(500); // FileMenu

          }
        }
      }
      if (!_mapSettings.Stack)
        dlg.AddLocalizedString(346); //Stack
      else
        dlg.AddLocalizedString(347); //Unstack

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;
      switch (dlg.SelectedId)
      {
        case 925: // Delete
          OnDeleteItem(item);
          break;

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
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
          break;

        case 654: // Eject
          if (MediaPortal.Util.Utils.getDriveType(item.Path) != 5) MediaPortal.Util.Utils.EjectCDROM();
          else MediaPortal.Util.Utils.EjectCDROM(System.IO.Path.GetPathRoot(item.Path));
          LoadDirectory(string.Empty);
          break;

        case 341: //Play dvd
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
          selectDVDHandler.OnPlayDVD(item.Path, GetID);
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
          if (facadeView.Focus)
          {
            if (item.IsFolder)
            {
              if (item.Label == "..") return;
              if (item.IsRemote) return;
            }
          }
          if (!_virtualDirectory.RequestPin(item.Path))
          {
            return;
          }
          ArrayList availablePaths = new ArrayList();
          availablePaths.Add(item.Path);
          IMDBFetcher.ScanIMDB(this, availablePaths, _isFuzzyMatching, _scanSkipExisting, _getActors);
          LoadDirectory(_currentFolder);
          break;

        case 830: // Reset watched status
          SetMovieUnwatched(item.Path);
          LoadDirectory(_currentFolder);
          UpdateButtonStates();
          break;

        case 500: // File menu
          {
            // get pincode
            if (_fileMenuPinCode != string.Empty)
            {
              string userCode = string.Empty;
              if (GetUserInputString(ref userCode) && userCode == _fileMenuPinCode)
              {
                OnShowFileMenu();
              }
            }
            else
              OnShowFileMenu();
          }
          break;
      }
    }

    private bool GetUserInputString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return false;
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = sString;
      keyboard.DoModal(GetID); // show it...
      if (keyboard.IsConfirmed) sString = keyboard.Text;
      return keyboard.IsConfirmed;
    }

    private void OnShowFileMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null) return;
      if (item.IsFolder && item.Label == "..") return;
      if (!_virtualDirectory.RequestPin(item.Path))
      {
        return;
      }
      // init
      GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
      if (dlgFile == null) return;

      // File operation settings
      dlgFile.SetSourceItem(item);
      dlgFile.SetSourceDir(_currentFolder);
      dlgFile.SetDestinationDir(_fileMenuDestinationDir);
      dlgFile.SetDirectoryStructure(_virtualDirectory);
      dlgFile.DoModal(GetID);
      _fileMenuDestinationDir = dlgFile.GetDestinationDir();

      //final
      if (dlgFile.Reload())
      {
        int selectedItem = facadeView.SelectedListItemIndex;
        if (_currentFolder != dlgFile.GetSourceDir()) selectedItem = -1;

        //_currentFolder = System.IO.Path.GetDirectoryName(dlgFile.GetSourceDir());
        LoadDirectory(_currentFolder);
        if (selectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItem);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
    }

    private void OnDeleteItem(GUIListItem item)
    {
      if (item.IsRemote)
      {
        return;
      }
      if (!_virtualDirectory.RequestPin(item.Path))
      {
        return;
      }

      _currentSelectedItem = facadeView.SelectedListItemIndex;

      IMDBMovie movieDetails = new IMDBMovie();

      int idMovie = VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);

      string movieFileName = System.IO.Path.GetFileName(item.Path);
      string movieTitle = movieFileName;
      if (idMovie >= 0) movieTitle = movieDetails.Title;

      //get all movies belonging to each other
      if (_mapSettings.Stack)
      {
        bool bStackedFile = false;
        ArrayList items = _virtualDirectory.GetDirectoryUnProtected(_currentFolder, true);
        int iPart = 1;
        bool decreaseSelectedItem = true;
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          string fname1 = System.IO.Path.GetFileName(temporaryListItem.Path).ToLower();
          string fname2 = System.IO.Path.GetFileName(item.Path).ToLower();
          if (MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, item.Path) || fname1.Equals(fname2))
          {
            bStackedFile = true;
            movieFileName = System.IO.Path.GetFileName(temporaryListItem.Path);
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo)
            {
              return;
            }
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
            dlgYesNo.SetLine(1, movieFileName);
            dlgYesNo.SetLine(2, String.Format("{0}: {1}", GUILocalizeStrings.Get(3021), iPart++));
            dlgYesNo.SetLine(3, string.Empty);
            dlgYesNo.DoModal(GetID);

            if (!dlgYesNo.IsConfirmed)
            {
              return;
            }
            DoDeleteItem(temporaryListItem,true);
            if (decreaseSelectedItem)
            {
              decreaseSelectedItem = false;
            }

          }
        }

        if (!bStackedFile)
        {
          // delete single file
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null == dlgYesNo)
          {
            return;
          }
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
          dlgYesNo.SetLine(1, movieTitle);
          dlgYesNo.SetLine(2, string.Empty);
          dlgYesNo.SetLine(3, string.Empty);
          dlgYesNo.DoModal(GetID);

          if (!dlgYesNo.IsConfirmed)
          {
            return;
          }
          DoDeleteItem(item, decreaseSelectedItem);
        }
      }
      else // stacking off
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          return;
        }
        /*
        if (!dlgYesNo.IsConfirmed)
        {
          return;
        }
        */
        ArrayList items = _virtualDirectory.GetDirectoryUnProtected(_currentFolder, true);
        int iPart = 1;
        bool decreaseSelectedItem = true;
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          string fname1 = System.IO.Path.GetFileNameWithoutExtension(temporaryListItem.Path).ToLower();
          string fname2 = System.IO.Path.GetFileNameWithoutExtension(item.Path).ToLower();
          if (fname1.Equals(fname2))
          {
            movieFileName = System.IO.Path.GetFileName(temporaryListItem.Path);
            dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo)
            {
              return;
            }
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
            dlgYesNo.SetLine(1, movieFileName);
            dlgYesNo.SetLine(2, String.Format("Part:{0}", iPart++));
            dlgYesNo.SetLine(3, string.Empty);
            dlgYesNo.DoModal(GetID);

            if (!dlgYesNo.IsConfirmed)
            {
              return;
            }
            DoDeleteItem(temporaryListItem, decreaseSelectedItem);
            if (decreaseSelectedItem)
            {
              decreaseSelectedItem = false;
            }
          }
        }
      }
      LoadDirectory(_currentFolder);
      SelectCurrentItem();
    }

    private bool SelectCurrentItem()
    {
      if (_currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, _currentSelectedItem);
        return true;
      }
      else
      {
        Log.Debug("GUIVideoFiles: SelectCurrentItem - nothing to do for item {0}", _currentSelectedItem.ToString());
        return false;
      }
    }

    private void DoDeleteItem(GUIListItem item, bool decreaseSelectedItem)
    {
      if (item.IsFolder)
      {
        if (item.IsRemote)
          return;
        if (item.Label != "..")
        {
          ArrayList items = new ArrayList();
          items = _virtualDirectory.GetDirectoryUnProtected(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            DoDeleteItem(subItem,false);
          }
          MediaPortal.Util.Utils.DirectoryDelete(item.Path);
          if (_currentSelectedItem > 0 && decreaseSelectedItem)
            _currentSelectedItem--;
        }
      }
      else
      {
        VideoDatabase.DeleteMovie(item.Path);
        TVDatabase.RemoveRecordedTVByFileName(item.Path);

        if (item.IsRemote)
          return;
        MediaPortal.Util.Utils.DeleteRecording(item.Path);
        if (_currentSelectedItem > 0 && decreaseSelectedItem)
          _currentSelectedItem--;
      }
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
      if (windowId == (int)GUIWindow.Window.WINDOW_DVD) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_ACTOR) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_ARTIST_INFO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_GENRE) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_INFO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_TITLE) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_YEAR) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEOS) return true;
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
      if (windowId == (int)GUIWindow.Window.WINDOW_DVD) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_ACTOR) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_ARTIST_INFO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_GENRE) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_INFO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEO_YEAR) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_VIDEOS) return true;
      return false;
    }

    private static bool IsFolderPinProtected(string folder)
    {
      int pinCode = 0;
      return _virtualDirectory.IsProtectedShare(folder, out pinCode);
    }

    private static void DownloadThumbnail(string folder, string url, string name)
    {
      if (url == null) return;
      if (url.Length == 0) return;
      string strThumb = MediaPortal.Util.Utils.GetCoverArtName(folder, name);
      string LargeThumb = MediaPortal.Util.Utils.GetLargeCoverArtName(folder, name);
      if (!System.IO.File.Exists(strThumb))
      {
        string strExtension;
        strExtension = System.IO.Path.GetExtension(url);
        if (strExtension.Length > 0)
        {
          string strTemp = "temp";
          strTemp += strExtension;
          strThumb = System.IO.Path.ChangeExtension(strThumb, strExtension);
          LargeThumb = System.IO.Path.ChangeExtension(LargeThumb, strExtension);
          MediaPortal.Util.Utils.FileDelete(strTemp);

          MediaPortal.Util.Utils.DownLoadImage(url, strTemp);
          if (System.IO.File.Exists(strTemp))
          {
            if (MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
              MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
          }
          else
            Log.Debug("GUIVideoFiles: unable to download thumb {0}->{1}", url, strTemp);
          MediaPortal.Util.Utils.FileDelete(strTemp);
        }
      }
    }

    private static void DownloadDirector(IMDBMovie movieDetails)
    {
      string actor = movieDetails.Director;
      string strThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MovieActors, actor);
      if (!System.IO.File.Exists(strThumb))
      {
        _imdb.FindActor(actor);
        IMDBActor imdbActor = new IMDBActor();
        for (int x = 0; x < _imdb.Count; ++x)
        {
          _imdb.GetActorDetails(_imdb[x], out imdbActor);
          if (imdbActor.ThumbnailUrl != null && imdbActor.ThumbnailUrl.Length > 0) break;
        }
        if (imdbActor.ThumbnailUrl != null)
        {
          if (imdbActor.ThumbnailUrl.Length != 0)
          {
            //ShowProgress(GUILocalizeStrings.Get(1009), actor, "", 0);
            DownloadThumbnail(Thumbs.MovieActors, imdbActor.ThumbnailUrl, actor);
          }
          else
            Log.Debug("GUIVideoFiles: url=empty for director {0}", actor);
        }
        else
          Log.Debug("GUIVideoFiles: url=null for director {0}", actor);
      }
    }

    private static void DownloadActors(IMDBMovie movieDetails)
    {
      char[] splitter = { '\n', ',' };
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
          string strThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MovieActors, actor);
          if (!System.IO.File.Exists(strThumb))
          {
            _imdb.FindActor(actor);
            IMDBActor imdbActor = new IMDBActor();
            for (int x = 0; x < _imdb.Count; ++x)
            {
              _imdb.GetActorDetails(_imdb[x], out imdbActor);
              if (imdbActor.ThumbnailUrl != null && imdbActor.ThumbnailUrl.Length > 0) break;
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
                Log.Debug("GUIVideoFiles: url=empty for actor {0}", actor);
            }
            else
              Log.Debug("GUIVideoFiles: url=null for actor {0}", actor);
          }
        }
      }
    }

    private void _downloadSubtitles()
    {
      try
      {
        GUIListItem item = facadeView.SelectedListItem;
        if (item == null) return;
        string path = item.Path;
        bool isDVD = (path.ToUpper().IndexOf("VIDEO_TS") >= 0);
        List<GUIListItem> listFiles = _virtualDirectory.GetDirectoryUnProtectedExt(_currentFolder, false);
        string[] sub_exts = { ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss", ".ass", ".idx", ".ifo" };
        if (!isDVD)
        {
          // check if movie has subtitles
          for (int i = 0; i < sub_exts.Length; i++)
          {
            for (int x = 0; x < listFiles.Count; ++x)
            {
              if (listFiles[x].IsFolder) continue;
              string subTitleFileName = listFiles[x].Path;
              subTitleFileName = System.IO.Path.ChangeExtension(subTitleFileName, sub_exts[i]);
              if (String.Compare(listFiles[x].Path, subTitleFileName, true) == 0)
              {
                string localSubtitleFileName = _virtualDirectory.GetLocalFilename(subTitleFileName);
                MediaPortal.Util.Utils.FileDelete(localSubtitleFileName);
                _virtualDirectory.DownloadRemoteFile(subTitleFileName, 0);
              }
            }
          }
        }
        else //download entire DVD
        {
          for (int i = 0; i < listFiles.Count; ++i)
          {
            if (listFiles[i].IsFolder) continue;
            if (String.Compare(listFiles[i].Path, path, true) == 0) continue;
            _virtualDirectory.DownloadRemoteFile(listFiles[i].Path, 0);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
    }

    public static void Reset()
    {
      Log.Debug("GUIVideoFiles: Resetting virtual directory");
      _virtualDirectory.Reset();
    }

    public static void PlayMovie(int idMovie)
    {
      int selectedFileIndex = 1;
      ArrayList movies = new ArrayList();
      VideoDatabase.GetFiles(idMovie, ref movies);
      if (movies.Count <= 0) return;
      if (!GUIVideoFiles.CheckMovie(idMovie)) return;
      // Image file handling.
      // If the only file is an image file, it should be mounted,
      // allowing autoplay to take over the playing of it.
      // There should only be one image file in the stack, since
      // stacking is not currently supported for image files.
      if (movies.Count == 1 && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension((string)movies[0]).ToLower()))
      {

        GUIVideoFiles.PlayMountedImageFile(GUIWindowManager.ActiveWindow, (string)movies[0]);
        return;
      }

      bool askForResumeMovie = true;
      int movieDuration = 0;
      {
        //get all movies belonging to each other
        ArrayList items = _virtualDirectory.GetDirectory(System.IO.Path.GetDirectoryName((string)movies[0]));
        if (items.Count <= 1) return; // first item always ".." so 1 item means protected share

        //check if we can resume 1 of those movies
        int timeMovieStopped = 0;
        bool asked = false;
        ArrayList newItems = new ArrayList();
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          if ((MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, (string)movies[0])) && (movies.Count > 1))
          {
            if (!asked) selectedFileIndex++;
            IMDBMovie movieDetails = new IMDBMovie();
            int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);
            if ((idMovie >= 0) && (idFile >= 0))
            {
              VideoDatabase.GetMovieInfo((string)movies[0], ref movieDetails);
              string title = System.IO.Path.GetFileName((string)movies[0]);
              if ((VirtualDirectory.IsValidExtension((string)movies[0], MediaPortal.Util.Utils.VideoExtensions, false))) MediaPortal.Util.Utils.RemoveStackEndings(ref title);
              if (movieDetails.Title != string.Empty) title = movieDetails.Title;

              timeMovieStopped = VideoDatabase.GetMovieStopTime(idFile);
              if (timeMovieStopped > 0)
              {
                if (!asked)
                {
                  asked = true;
                  GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                  if (null == dlgYesNo) return;
                  dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                  dlgYesNo.SetLine(1, title);
                  dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(movieDuration + timeMovieStopped));
                  dlgYesNo.SetDefaultToYes(true);
                  dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                  if (dlgYesNo.IsConfirmed)
                  {
                    askForResumeMovie = false;
                    newItems.Add(temporaryListItem);
                  }
                  else
                  {
                    VideoDatabase.DeleteMovieStopTime(idFile);
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
          }//if ( MediaPortal.Util.Utils.ShouldStack(temporaryListItem.Path, item.Path))
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
          if (MediaPortal.Util.Utils.IsVideo(temporaryListItem.Path) && !PlayListFactory.IsPlayList(temporaryListItem.Path))
          {
            movies.Add(temporaryListItem.Path);
          }
        }
      }

      if (movies.Count > 1)
      {
        if (askForResumeMovie)
        {
          GUIDialogFileStacking dlg = (GUIDialogFileStacking)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING);
          if (null != dlg)
          {
            dlg.SetNumberOfFiles(movies.Count);
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            selectedFileIndex = dlg.SelectedFile;
            if (selectedFileIndex < 1) return;
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
        newitem.Type = Playlists.PlayListItem.PlayListItemType.Video;
        playlist.Add(newitem);
      }

      // play movie...
      GUIVideoFiles.PlayMovieFromPlayList(askForResumeMovie);
    }

    #region IMDB.IProgress
    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsInstance(fetcher))
      {
        pDlgProgress.DisableCancel(true);
      }
      return true;
    }
    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted) return;
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      if (percent > 0)
        pDlgProgress.SetPercentage(percent);
      pDlgProgress.Progress();
    }
    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        pDlgOK.SetHeading(195);
        pDlgOK.SetLine(1, fetcher.MovieName);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
        return true;
      }
    }
    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }
    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
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
        movieName = fetcher.Movie.Title;
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
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
        // more then 1 movie found
        // ask user to select 1
        pDlgSelect.Reset();
        pDlgSelect.SetHeading(196);//select movie
        for (int i = 0; i < fetcher.Count; ++i)
        {
          pDlgSelect.Add(fetcher[i].Title);
        }
        pDlgSelect.EnableButton(true);
        pDlgSelect.SetButtonLabel(413); // manual
        pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

        // and wait till user selects one
        selectedMovie = pDlgSelect.SelectedLabel;
        if (pDlgSelect.IsButtonPressed) return true;
        if (selectedMovie == -1) return false;
        else return true;
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
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
        // more than 1 movie found
        // ask user to select 1
        do
        {
          pDlgSelect.Reset();
          pDlgSelect.SetHeading(892);//select movie
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
          if (pDlgSelect.IsButtonPressed) break;
          if (selectedMovie == -1) break;
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
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
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

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
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