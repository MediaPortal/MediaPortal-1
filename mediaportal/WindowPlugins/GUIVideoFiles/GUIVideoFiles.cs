#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Globalization;
using System.Xml.Serialization;
using MediaPortal.Dispatcher;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.Video.Database;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

#pragma warning disable 108

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// 
  /// </summary>
  public class GUIVideoFiles : GUIVideoBaseWindow, ISetupForm, IShowPlugin, IMDB.IProgress
  {

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

    static IMDB _imdb;
    DirectoryHistory m_history = new DirectoryHistory();
    string currentFolder = String.Empty;
    string m_strDirectoryStart = String.Empty;
    int currentSelectedItem = -1;
    static VirtualDirectory m_directory = new VirtualDirectory();
    MapSettings mapSettings = new MapSettings();
    bool m_askBeforePlayingDVDImage = false;
    // File menu
    string destinationFolder = String.Empty;
    bool fileMenuEnabled = false;
    string fileMenuPinCode = String.Empty;
    static PlayListPlayer playlistPlayer;
    bool ShowTrailerButton = true;

    static GUIVideoFiles()
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public GUIVideoFiles()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIDEOS;

      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.VideoExtensions);
    }

    protected override bool CurrentSortAsc
    {
      get
      {
        return mapSettings.SortAscending;
      }
      set
      {
        mapSettings.SortAscending = value;
      }
    }
    protected override VideoSort.SortMethod CurrentSortMethod
    {
      get
      {
        return (VideoSort.SortMethod)mapSettings.SortBy;
      }
      set
      {
        mapSettings.SortBy = (int)value;
      }
    }
    protected override View CurrentView
    {
      get
      {
        return (View)mapSettings.ViewAs;
      }
      set
      {
        mapSettings.ViewAs = (int)value;
      }
    }

    public override bool Init()
    {
      _imdb = new IMDB(this);
      g_Player.PlayBackStopped += new MediaPortal.Player.g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
      currentFolder = String.Empty;

      LoadSettings();
      bool result = Load(GUIGraphicsContext.Skin + @"\myVideo.xml");
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        VideoState.StartWindow = xmlreader.GetValueAsInt("movies", "startWindow", GetID);
      }
      LoadSettings();
      return result;
    }

    #region Serialisation
    protected override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        ShowTrailerButton = xmlreader.GetValueAsBool("plugins", "My Trailers", true);
        fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        fileMenuPinCode = Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", String.Empty));
        m_directory.Clear();
        string strDefault = xmlreader.GetValueAsString("movies", "default", String.Empty);
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);
          string shareViewPath = String.Format("shareview{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("movies", strShareName, String.Empty);
          share.Path = xmlreader.GetValueAsString("movies", strSharePath, String.Empty);
          string pinCode = Utils.DecryptPin(xmlreader.GetValueAsString("movies", strPincode, string.Empty));
          if (pinCode != string.Empty)
            share.Pincode = Convert.ToInt32(pinCode);
          else
            share.Pincode = -1;

          share.IsFtpShare = xmlreader.GetValueAsBool("movies", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("movies", shareServer, String.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("movies", shareLogin, String.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("movies", sharePwd, String.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("movies", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("movies", remoteFolder, "/");
          share.DefaultView = (Share.Views)xmlreader.GetValueAsInt("movies", shareViewPath, (int)Share.Views.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
              if (currentFolder.Length == 0)
              {
                currentFolder = share.Path;
                m_strDirectoryStart = share.Path;
              }
            }
            m_directory.Add(share);
          }
          else break;
        }
        m_askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
      }
    }


    #endregion

    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = facadeView[0];
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              if (currentFolder != m_strDirectoryStart)
              {
                LoadDirectory(item.Path);
                return;
              }
            }
          }
        }
      }
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (VideoState.StartWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(VideoState.StartWindow);
        return;
      }
      // Check if mytrailers-plugin is enabled
      if (ShowTrailerButton != true)
      {
        btnTrailers.Visible = false;
        btnPlayDVD.NavigateDown = 99;
      }
      LoadFolderSettings(currentFolder);
      LoadDirectory(currentFolder);
    }


    protected override void OnPageDestroy(int newWindowId)
    {
      currentSelectedItem = facadeView.SelectedListItemIndex;

      SaveFolderSettings(currentFolder);
      base.OnPageDestroy(newWindowId);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
          if (g_Player.Playing && g_Player.IsDVD)
          {
            Log.Write("GUIVideo:stop dvd since DVD is ejected");
            g_Player.Stop();
          }

          if (GUIWindowManager.ActiveWindow == GetID)
          {
            if (Utils.IsDVD(currentFolder))
            {
              currentFolder = String.Empty;
              LoadDirectory(currentFolder);
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
          currentFolder = message.Label;
          LoadDirectory(currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          if (currentFolder == String.Empty || currentFolder.Substring(0, 2) == message.Label)
          {
            currentFolder = String.Empty;
            LoadDirectory(currentFolder);
          }
          break;
      }
      return base.OnMessage(message);
    }



    void LoadFolderSettings(string folderName)
    {
      if (folderName == String.Empty) folderName = "root";
      object o;
      FolderSettings.GetFolderSetting(folderName, "VideoFiles", typeof(GUIVideoFiles.MapSettings), out o);
      if (o != null)
      {
        mapSettings = o as MapSettings;
        if (mapSettings == null) mapSettings = new MapSettings();
        CurrentSortAsc = mapSettings.SortAscending;
        CurrentSortMethod = (VideoSort.SortMethod)mapSettings.SortBy;
        currentView = (View)mapSettings.ViewAs;
      }
      else
      {
        Share share = m_directory.GetShare(folderName);
        if (share != null)
        {
          if (mapSettings == null) mapSettings = new MapSettings();
          CurrentSortAsc = mapSettings.SortAscending;
          CurrentSortMethod = (VideoSort.SortMethod)mapSettings.SortBy;
          currentView = (View)share.DefaultView;
        }
      }
      SwitchView();
      UpdateButtonStates();
    }
    void SaveFolderSettings(string folderName)
    {
      if (folderName == String.Empty) folderName = "root";
      FolderSettings.AddFolderSetting(folderName, "VideoFiles", typeof(GUIVideoFiles.MapSettings), mapSettings);
    }

    protected override void LoadDirectory(string newFolderName)
    {
      GUIListItem selectedListItem = facadeView.SelectedListItem;
      if (selectedListItem != null)
      {
        if (selectedListItem.IsFolder && selectedListItem.Label != "..")
        {
          m_history.Set(selectedListItem.Label, currentFolder);
        }
      }
      if (newFolderName != currentFolder && mapSettings != null)
      {
        SaveFolderSettings(currentFolder);
      }

      if (newFolderName != currentFolder || mapSettings == null)
      {
        LoadFolderSettings(newFolderName);
      }

      currentFolder = newFolderName;

      string objectCount = String.Empty;

      ArrayList itemlist;

      // Mounting and loading a DVD image file takes a long time,
      // so display a message letting the user know that something 
      // is happening.
      if (!m_askBeforePlayingDVDImage && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(currentFolder)))
      {
        itemlist = PlayMountedImageFile(GetID, currentFolder);

        // Remember the directory that the image file is in rather than the
        // image file itself.  This prevents repeated playing of the image file.
        if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(currentFolder)))
        {
          currentFolder = System.IO.Path.GetDirectoryName(currentFolder);
        }
      }
      else
      {
        GUIControl.ClearControl(GetID, facadeView.GetID);
        itemlist = m_directory.GetDirectory(currentFolder);
      }

      if (mapSettings.Stack)
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
                  if (Utils.ShouldStack(item1.Path, item2.Path))
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

            Utils.RemoveStackEndings(ref label);
            item1.Label = label;
            itemfiltered.Add(item1);
          }
        }
        itemlist = itemfiltered;
      }

      SetIMDBThumbs(itemlist);
      string selectedItemLabel = m_history.Get(currentFolder);
      int itemIndex = 0;
      foreach (GUIListItem item in itemlist)
      {
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
      }
      OnSort();
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        if (item.Label == selectedItemLabel)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, i);
          break;
        }
        itemIndex++;
      }
      int totalItems = itemlist.Count;
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = (GUIListItem)itemlist[0];
        if (rootItem.Label == "..") totalItems--;
      }
      objectCount = String.Format("{0} {1}", totalItems, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", objectCount);
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSelectedItem);
      }
    }
    #endregion



    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null) return;
      bool isFolderAMovie = false;

      if (item.IsFolder && !item.IsRemote)
      {
        // Check if folder is actually a DVD. If so don't browse this folder, but play the DVD!
        if ((System.IO.File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO")) && (item.Label != ".."))
        {
          isFolderAMovie = true;
          item.Path = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else
        {
          isFolderAMovie = false;
        }
      }

      if ((item.IsFolder) && (!isFolderAMovie))
      //-- Mars Warrior @ 03-sep-2004
      {
        currentSelectedItem = -1;
        LoadDirectory(item.Path);
      }
      else
      {
        if (m_directory.IsRemote(item.Path))
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!m_directory.ShouldWeDownloadFile(item.Path)) return;
            if (!m_directory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
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
              bool isDVD = (item.Path.ToUpper().IndexOf("VIDEO_TS") >= 0);
              List<GUIListItem> listFiles = m_directory.GetDirectoryUnProtectedExt(currentFolder, false);
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
                      string localSubtitleFileName = m_directory.GetLocalFilename(subTitleFileName);
                      Utils.FileDelete(localSubtitleFileName);
                      m_directory.DownloadRemoteFile(subTitleFileName, 0);
                    }
                  }
                }
              }
              else //download entire DVD
              {
                for (int i = 0; i < listFiles.Count; ++i)
                {
                  if (listFiles[i].IsFolder) continue;
                  if (String.Compare(listFiles[i].Path, item.Path, true) == 0) continue;
                  m_directory.DownloadRemoteFile(listFiles[i].Path, 0);
                }
              }
            }
          }
        }

        if (item.FileInfo != null)
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length)) return;
        }
        string movieFileName = item.Path;
        movieFileName = m_directory.GetLocalFilename(movieFileName);

        // Set selected item
        currentSelectedItem = facadeView.SelectedListItemIndex;
        if (PlayListFactory.IsPlayList(movieFileName))
        {
          LoadPlayList(movieFileName);
          return;
        }


        if (!CheckMovie(movieFileName)) return;
        bool askForResumeMovie = true;
        if (mapSettings.Stack)
        {
          int selectedFileIndex = 0;
          int movieDuration = 0;
          ArrayList movies = new ArrayList();
          {
            //get all movies belonging to each other
            ArrayList items = m_directory.GetDirectory(currentFolder);

            //check if we can resume 1 of those movies
            int timeMovieStopped = 0;
            bool asked = false;
            ArrayList newItems = new ArrayList();
            for (int i = 0; i < items.Count; ++i)
            {
              GUIListItem temporaryListItem = (GUIListItem)items[i];
              if (Utils.ShouldStack(temporaryListItem.Path, item.Path))
              {
                if (!asked) selectedFileIndex++;
                IMDBMovie movieDetails = new IMDBMovie();
                int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);
                int idMovie = VideoDatabase.GetMovieId(item.Path);
                if ((idMovie >= 0) && (idFile >= 0))
                {
                  VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);
                  string title = System.IO.Path.GetFileName(item.Path);
                  Utils.RemoveStackEndings(ref title);
                  if (movieDetails.Title != String.Empty) title = movieDetails.Title;

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
                      dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + Utils.SecondsToHMSString(movieDuration + timeMovieStopped));
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
              }//if (Utils.ShouldStack(temporaryListItem.Path, item.Path))
            }

            for (int i = 0; i < newItems.Count; ++i)
            {
              GUIListItem temporaryListItem = (GUIListItem)newItems[i];
              if (Utils.IsVideo(temporaryListItem.Path) && !PlayListFactory.IsPlayList(temporaryListItem.Path))
              {
                if (Utils.ShouldStack(temporaryListItem.Path, item.Path))
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
          playlistPlayer.Reset();
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
          PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
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

        playlistPlayer.Reset();
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
        PlayList newPlayList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
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
                  if (Utils.IsVideo(movieFileName))
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
        currentFolder = listItem.Path;

        ArrayList itemlist = m_directory.GetDirectory(currentFolder);
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
      }
      else
      {
        //TODO
        if (Utils.IsVideo(listItem.Path) && !PlayListFactory.IsPlayList(listItem.Path))
        {
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.FileName = listItem.Path;
          playlistItem.Description = listItem.Label;
          playlistItem.Duration = listItem.Duration;
          playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Video;
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(playlistItem);
        }
      }
    }

    void LoadPlayList(string playListFileName)
    {
      IPlayListIO loader = PlayListFactory.CreateIO(playListFileName);
      PlayList playlist = new PlayList();

      if (!loader.Load(playlist, playListFileName))
      {
        GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK != null)
        {
          dlgOK.SetHeading(6);
          dlgOK.SetLine(1, 477);
          dlgOK.SetLine(2, String.Empty);
          dlgOK.DoModal(GetID);
        }
        return;
      }

      if (playlist.Count == 1)
      {
        //TODO
        Log.Write("GUIVideoFiles play:{0}", playlist[0].FileName);
        if (g_Player.Play(playlist[0].FileName))
        {
          if (Utils.IsVideo(playlist[0].FileName))
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
        }
        return;
      }

      // clear current playlist
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Clear();

      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayListItem playListItem = playlist[i];
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(playListItem);
      }


      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Count > 0)
      {
        // then get 1st song
        playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
        PlayListItem item = playlist[0];

        // and start playing it
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        playlistPlayer.Reset();
        playlistPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
        }
      }
    }

    void AddFileToDatabase(string strFile)
    {
      if (!Utils.IsVideo(strFile)) return;
      //if ( Utils.IsNFO(strFile)) return;
      if (PlayListFactory.IsPlayList(strFile)) return;

      if (!VideoDatabase.HasMovieInfo(strFile))
      {
        ArrayList allFiles = new ArrayList();
        ArrayList items = m_directory.GetDirectory(currentFolder);
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          if (temporaryListItem.IsFolder) continue;
          if (Utils.ShouldStack(temporaryListItem.Path, strFile) || temporaryListItem.Path == strFile)
          {
            allFiles.Add(items[i]);
          }
        }
        // set initial movie info
        if (allFiles.Count == 0)
        {
          GUIListItem item = new GUIListItem();
          item.Path = strFile;
          allFiles.Add(item);
        }
        foreach (GUIListItem item in allFiles)
        {
          VideoDatabase.AddMovieFile(item.Path);
        }

        IMDBMovie movieDetails = new IMDBMovie();
        int iidMovie = VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        if (iidMovie >= 0)
        {
          if (Utils.IsDVD(strFile))
          {
            //DVD
            movieDetails.DVDLabel = Utils.GetDriveName(System.IO.Path.GetPathRoot(strFile));
            movieDetails.Title = movieDetails.DVDLabel;
          }
          else if (strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO") >= 0)
          {
            //DVD folder
            strFile = strFile.Substring(0, strFile.ToUpper().IndexOf(@"\VIDEO_TS\VIDEO_TS.IFO"));
            movieDetails.DVDLabel = System.IO.Path.GetFileName(strFile);
            movieDetails.Title = movieDetails.DVDLabel;
          }
          else
          {
            //Movie 
            movieDetails.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);
          }
          VideoDatabase.SetMovieInfoById(iidMovie, ref movieDetails);
        }
      }
    }

    bool OnScan(ArrayList items)
    {
      // remove username + password from currentFolder for display in Dialog

      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(189);
        dlgProgress.SetLine(1, String.Empty);
        dlgProgress.SetLine(2, currentFolder);
        dlgProgress.StartModal(GetID);
      }

      OnRetrieveVideoInfo(items);
      if (dlgProgress != null)
      {
        dlgProgress.SetLine(2, currentFolder);
        if (dlgProgress.IsCanceled) return false;
      }

      bool bCancel = false;
      for (int i = 0; i < items.Count; ++i)
      {
        GUIListItem pItem = (GUIListItem)items[i];
        if (dlgProgress != null)
        {
          if (dlgProgress.IsCanceled)
          {
            bCancel = true;
            break;
          }
        }
        if (pItem.IsFolder)
        {
          if (pItem.Label != "..")
          {
            // load subfolder
            string strDir = currentFolder;
            currentFolder = pItem.Path;

            bool FolderIsDVD = false;
            // Mars Warrior @ 03-sep-2004
            // Check for single movie in directory and make sure (just as with DVDs) that
            // the folder gets the nice folder.jpg image ;-)
            bool isFolderAMovie = false;

            ArrayList subDirItems = m_directory.GetDirectory(pItem.Path);
            foreach (GUIListItem item in subDirItems)
            {
              if (item.Label.ToLower().Equals("video_ts"))
              {
                FolderIsDVD = true;
                break;
              }
            }

            // Check if folder is a folder containig a single movie file. If so, (again), don't
            // browse the folder, but play the movie!
            int iVideoFilesCount = 0;
            string strVideoFile = String.Empty;
            foreach (GUIListItem item in subDirItems)
            {
              if (Utils.IsVideo(item.Path) && !PlayListFactory.IsPlayList(item.Path))
              {
                iVideoFilesCount++;
                if (iVideoFilesCount == 1) strVideoFile = item.Path;
              }
            }
            if (iVideoFilesCount == 1)
            {
              isFolderAMovie = true;
            }
            else isFolderAMovie = false;

            if ((!FolderIsDVD) && (!isFolderAMovie))
            {
              if (!OnScan(subDirItems))
              {
                bCancel = true;
              }
            }
            else if (FolderIsDVD)
            {
              string strFilePath = String.Format(@"{0}\VIDEO_TS\VIDEO_TS.IFO", pItem.Path);
              OnRetrieveVideoInfo(strFilePath, pItem.Label, pItem.Path);
            }
            else if (isFolderAMovie)
            {
              OnRetrieveVideoInfo(strVideoFile, pItem.Label, pItem.Path);
            }
            //-- Mars Warrior

            currentFolder = strDir;
            if (bCancel) break;
          }
        }
      }

      if (dlgProgress != null) dlgProgress.Close();
      return !bCancel;
    }



    /// <summary>
    /// Searches IMDB for a movie and if found gets the details about the 1st movie found
    /// details are put in the video database under the file mentioned by movieFileName
    /// also a thumbnail is downloaded to thumbs\ and
    /// if strPath is filled in a srtpath\folder.jpg is created
    /// </summary>
    /// <param name="movieFileName">path+filename to which this imdb info will belong</param>
    /// <param name="strMovieName">IMDB search string</param>
    /// <param name="strPath">path where folder.jpg should be created</param>
    void OnRetrieveVideoInfo(string movieFileName, string strMovieName, string strPath)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      AddFileToDatabase(movieFileName);

      if (!VideoDatabase.HasMovieInfo(movieFileName))
      {
#if BACKGROUND_IMDB
			Job job = new Job();

		  job.Argument = new object[] { movieFileName, strMovieName, strPath };
		  job.DoWork += new DoWorkEventHandler(BackgroundImdbWorker);
		  job.Name = string.Format("Imdb: {0}", movieFileName);
		  job.Priority = JobPriority.BelowNormal;
		  job.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundImdbCompleted);
		  job.Dispatch();

			return;

#endif // BACKGROUND_IMDB

        // do IMDB lookup...
        if (dlgProgress != null)
        {
          dlgProgress.SetHeading(197);
          dlgProgress.SetLine(1, strMovieName);
          dlgProgress.SetLine(2, String.Empty);
          dlgProgress.SetLine(3, String.Empty);
          dlgProgress.Progress();
          if (dlgProgress.IsCanceled) return;
        }


        _imdb.Find(strMovieName);

        int iMoviesFound = _imdb.Count;
        if (iMoviesFound > 0)
        {
          IMDBMovie movieDetails = new IMDBMovie();
          movieDetails.SearchString = strMovieName;
          IMDB.IMDBUrl url = _imdb[0];

          // show dialog that we're downloading the movie info
          if (dlgProgress != null)
          {
            dlgProgress.SetHeading(198);
            //dlgProgress.SetLine(0, strMovieName);
            dlgProgress.SetLine(1, url.Title);
            dlgProgress.SetLine(2, String.Empty);
            dlgProgress.Progress();
            if (dlgProgress.IsCanceled) return;
          }

          if (_imdb.GetDetails(url, ref movieDetails))
          {
            // get & save thumbnail
            AmazonImageSearch search = new AmazonImageSearch();
            search.Search(movieDetails.Title);
            if (search.Count > 0)
            {
              movieDetails.ThumbURL = search[0];
            }
            string orgMovieTitle = movieDetails.Title;
            VideoDatabase.SetMovieInfo(movieFileName, ref movieDetails);
            string strThumb = String.Empty;
            string strImage = movieDetails.ThumbURL;
            if (strImage.Length > 0 && movieDetails.ThumbURL.Length > 0)
            {
              string LargeThumb = Utils.GetLargeCoverArtName(Thumbs.MovieTitle, orgMovieTitle);
              strThumb = Utils.GetCoverArtName(Thumbs.MovieTitle, orgMovieTitle);
              Utils.FileDelete(strThumb);
              Utils.FileDelete(LargeThumb);

              string strExtension = System.IO.Path.GetExtension(strImage);
              if (strExtension.Length > 0)
              {
                string strTemp = "temp" + strExtension;
                Utils.FileDelete(strTemp);
                if (dlgProgress != null)
                {
                  dlgProgress.SetLine(2, 415);
                  dlgProgress.Progress();
                  if (dlgProgress.IsCanceled) return;
                }

                Utils.DownLoadImage(strImage, strTemp);
                if (System.IO.File.Exists(strTemp))
                {
                  MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
                  MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
                  if (strPath.Length > 0)
                  {
                    try
                    {
                      Utils.FileDelete(strPath + @"\folder.jpg");
                      System.IO.File.Copy(strThumb, strPath + @"\folder.jpg");
                    }
                    catch (Exception) { }
                  }
                }
                Utils.FileDelete(strTemp);
              }//if ( strExtension.Length>0)
              else
              {
                Log.Write("image has no extension:{0}", strImage);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Retrieves the imdb info for an array of items.
    /// </summary>
    /// <param name="items"></param>
    void OnRetrieveVideoInfo(ArrayList items)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      // for every file found
      for (int i = 0; i < items.Count; ++i)
      {
        GUIListItem pItem = (GUIListItem)items[i];
        if (!pItem.IsFolder
          || (pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
        {
          if (Utils.IsVideo(pItem.Path) && /*!Utils.IsNFO(pItem.Path) && */!PlayListFactory.IsPlayList(pItem.Path))
          {
            string strItem = String.Format("{0}/{1}", i + 1, items.Count);
            if (dlgProgress != null)
            {
              dlgProgress.SetLine(1, strItem);
              dlgProgress.SetLine(2, System.IO.Path.GetFileName(pItem.Path));
              dlgProgress.Progress();
              if (dlgProgress.IsCanceled) return;
            }
            string strMovieName = System.IO.Path.GetFileName(pItem.Path);
            OnRetrieveVideoInfo(pItem.Path, strMovieName, String.Empty);
          }
        }
      }
    }

    protected override void OnInfo(int iItem)
    {
      currentSelectedItem = facadeView.SelectedListItemIndex;
      GUIDialogSelect dlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      bool bFolder = false;
      string strFolder = String.Empty;
      int iSelectedItem = facadeView.SelectedListItemIndex;
      GUIListItem pItem = facadeView.SelectedListItem;
      if (pItem == null) return;
      if (pItem.IsRemote) return;
      string strFile = pItem.Path;
      string strMovie = pItem.Label;
      // Use DVD label as movie name
      if (Utils.IsDVD(pItem.Path) && (pItem.DVDLabel != String.Empty))
      {
        strMovie = pItem.DVDLabel;
      }
      if (pItem.IsFolder)
      {
        // IMDB is done on a folder, find first file in folder
        strFolder = pItem.Path;
        bFolder = true;

        bool bFoundFile = false;

        string strExtension = System.IO.Path.GetExtension(pItem.Path).ToLower();
        if (VirtualDirectory.IsImageFile(strExtension))
        {
          strFile = pItem.Path;
          bFoundFile = true;
        }
        else
        {
          ArrayList vecitems = m_directory.GetDirectory(pItem.Path);
          for (int i = 0; i < vecitems.Count; ++i)
          {
            pItem = (GUIListItem)vecitems[i];
            if (!pItem.IsFolder
            || (pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
            {
              if (Utils.IsVideo(pItem.Path) && /*!Utils.IsNFO(pItem.Path) && */!PlayListFactory.IsPlayList(pItem.Path))
              {
                strFile = pItem.Path;
                bFoundFile = true;
                break;
              }
            }
            else
            {
              if (pItem.Path.ToLower().IndexOf("video_ts") >= 0)
              {
                strFile = String.Format(@"{0}\VIDEO_TS.IFO", pItem.Path);
                bFoundFile = true;
                break;
              }

            }
          }
        }
        if (!bFoundFile)
        {
          // no Video file in this folder?
          // then just lookup IMDB info and show it
          ShowIMDB(-1, strMovie, strFolder, strFolder, false);
          facadeView.RefreshCoverArt();
          LoadDirectory(currentFolder);
          GUIControl.SelectItemControl(GetID, facadeView.GetID, iSelectedItem);
          return;
        }
      }

      AddFileToDatabase(strFile);



      ShowIMDB(-1, strMovie, strFile, strFolder, bFolder);
      facadeView.RefreshCoverArt();
      LoadDirectory(currentFolder);
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iSelectedItem);
    }


    static public bool ShowIMDB(int iidMovie)
    {
      ArrayList movies = new ArrayList();
      VideoDatabase.GetFiles(iidMovie, ref movies);
      if (movies.Count <= 0) return false;
      string strFilePath = (string)movies[0];
      string strFile = System.IO.Path.GetFileName(strFilePath);
      return ShowIMDB(iidMovie, strFile, strFilePath, String.Empty, false);

    }
    /// <summary>
    /// Download & shows IMDB info for a file
    /// </summary>
    /// <param name="strMovie">IMDB search criteria</param>
    /// <param name="strFile">path+file where imdb info should be saved for</param>
    /// <param name="strFolder">path where folder.jpg should be created (if bFolder==true)</param>
    /// <param name="bFolder">true create a folder.jpg, false dont create a folder.jpg</param>
    static public bool ShowIMDB(int iidMovie, string strMovie, string strFile, string strFolder, bool bFolder)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      GUIVideoInfo pDlgInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);

      bool bUpdate = false;
      bool bFound = false;

      if (null == pDlgOK) return false;
      if (null == pDlgProgress) return false;
      if (null == pDlgSelect) return false;
      if (null == pDlgInfo) return false;
      string strMovieName = System.IO.Path.GetFileNameWithoutExtension(strMovie);

      IMDBMovie movieDetails = new IMDBMovie();
      if (iidMovie >= 0)
        VideoDatabase.GetMovieInfoById(iidMovie, ref movieDetails);
      else
      {
        if (VideoDatabase.HasMovieInfo(strFile))
        {
          VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        }
      }

      if (movieDetails.ID >= 0)
      {
        pDlgInfo.Movie = movieDetails;
        pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
        if (!pDlgInfo.NeedsRefresh)
        {
          if (bFolder && strFile != String.Empty)
          {
            // copy icon to folder also;
            string strThumbOrg = Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title);
            string strFolderImage = System.IO.Path.GetFullPath(strFolder);
            strFolderImage += "\\folder.jpg";
            if (System.IO.File.Exists(strThumbOrg))
            {
              Utils.FileDelete(strFolderImage);
              try
              {
                Utils.FileDelete(strFolderImage);
                System.IO.File.Copy(strThumbOrg, strFolderImage);
              }
              catch (Exception)
              {
              }
            }
          }
          return true;
        }

        if (!Util.Win32API.IsConnectedToInternet()) return false;
        if (iidMovie >= 0)
          VideoDatabase.DeleteMovieInfoById(iidMovie);
        else
          VideoDatabase.DeleteMovieInfo(strFile);
        strMovieName = movieDetails.Title;
        GetStringFromKeyboard(ref strMovieName);
      }
      else
      {
        if (Utils.IsDVD(strFile))
        {
          GetStringFromKeyboard(ref strMovieName);
        }
      }

      if (!Util.Win32API.IsConnectedToInternet()) return false;

      bool bContinue = false;
      do
      {
        bContinue = false;
        if (!bFound)
        {
          // show dialog that we're busy querying www.imdb.com
          pDlgProgress.SetHeading(197);
          //pDlgProgress.SetLine(0, strMovieName);
          pDlgProgress.SetLine(1, strMovieName);
          pDlgProgress.SetLine(2, String.Empty);
          pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
          pDlgProgress.Progress();

          bool bError = true;
          _imdb.Find(strMovieName);
          if (_imdb.Count > 0)
          {
            pDlgProgress.Close();

            int iMoviesFound = _imdb.Count;
            if (iMoviesFound > 0)
            {
              int iSelectedMovie = 0;
              if (iMoviesFound > 1)
              {
                // more then 1 movie found
                // ask user to select 1
                pDlgSelect.SetHeading(196);//select movie
                pDlgSelect.Reset();
                for (int i = 0; i < iMoviesFound; ++i)
                {
                  IMDB.IMDBUrl url = _imdb[i];
                  pDlgSelect.Add(url.Title);
                }
                pDlgSelect.EnableButton(true);
                pDlgSelect.SetButtonLabel(413); // manual
                pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

                // and wait till user selects one
                iSelectedMovie = pDlgSelect.SelectedLabel;
                if (iSelectedMovie < 0)
                {
                  if (!pDlgSelect.IsButtonPressed) return false;
                  GetStringFromKeyboard(ref strMovieName);
                  if (strMovieName == String.Empty) return false;
                  bContinue = true;
                  bError = false;
                }
              }

              if (iSelectedMovie >= 0)
              {
                movieDetails = new IMDBMovie();
                movieDetails.SearchString = strFile;
                IMDB.IMDBUrl url = _imdb[iSelectedMovie];

                // show dialog that we're downloading the movie info
                pDlgProgress.SetHeading(198);
                //pDlgProgress.SetLine(0, strMovieName);
                pDlgProgress.SetLine(1, url.Title);
                pDlgProgress.SetLine(2, String.Empty);
                pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
                pDlgProgress.Progress();
                if (_imdb.GetDetails(url, ref movieDetails))
                {
                  // got all movie details :-)
                  AmazonImageSearch search = new AmazonImageSearch();
                  search.Search(movieDetails.Title);
                  if (search.Count > 0)
                  {
                    movieDetails.ThumbURL = search[0];
                  }
                  //get all actors...
                  DownloadActors(movieDetails);
                  DownloadDirector(movieDetails);
                  pDlgProgress.Close();
                  bError = false;

                  // now show the imdb info
                  if (iidMovie >= 0)
                    VideoDatabase.SetMovieInfoById(iidMovie, ref movieDetails);
                  else
                    VideoDatabase.SetMovieInfo(strFile, ref movieDetails);
                  pDlgInfo.Movie = movieDetails;
                  pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
                  if (!pDlgInfo.NeedsRefresh)
                  {
                    bUpdate = true;
                    if (bFolder && strFile != String.Empty)
                    {
                      // copy icon to folder also;
                      string strThumbOrg = Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title);
                      if (System.IO.File.Exists(strThumbOrg))
                      {
                        string strFolderImage = System.IO.Path.GetFullPath(strFolder);
                        strFolderImage += "\\folder.jpg"; //TODO                  
                        try
                        {
                          Utils.FileDelete(strFolderImage);
                          System.IO.File.Copy(strThumbOrg, strFolderImage);
                        }
                        catch (Exception)
                        {
                        }

                      }
                    }
                  }
                  else
                  {
                    bContinue = true;
                    //strMovieName = System.IO.Path.GetFileNameWithoutExtension(strMovie);
                    strMovieName = movieDetails.Title;
                    GetStringFromKeyboard(ref strMovieName);
                  }
                }
                else
                {
                  pDlgProgress.Close();
                }
              }
            }
            else
            {
              pDlgProgress.Close();
              GetStringFromKeyboard(ref strMovieName);
              if (strMovieName == String.Empty) return false;
              bContinue = true;
              bError = false;
            }
          }
          else
          {
            pDlgProgress.Close();
            GetStringFromKeyboard(ref strMovieName);
            if (strMovieName == String.Empty) return false;
            bContinue = true;
            bError = false;
          }

          if (bError)
          {
            // show dialog...
            pDlgOK.SetHeading(195);
            pDlgOK.SetLine(1, strMovieName);
            pDlgOK.SetLine(2, String.Empty);
            pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          }
        }
      } while (bContinue);

      if (bUpdate)
      {
        return true;
      }
      return false;
    }

    void SetIMDBThumbs(ArrayList items)
    {
      GUIListItem pItem;
      ArrayList movies = new ArrayList();
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (pItem.IsFolder)
        {
          if (pItem.ThumbnailImage != String.Empty) continue;
          if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
          {
            movies.Clear();
            string file = pItem.Path + @"\VIDEO_TS";
            VideoDatabase.GetMoviesByPath(file, ref movies);
            for (int i = 0; i < movies.Count; ++i)
            {
              IMDBMovie info = (IMDBMovie)movies[i];
              string strFile = "VIDEO_TS.IFO";
              if (info.File[0] == '\\' || info.File[0] == '/')
                info.File = info.File.Substring(1);

              if (strFile.Length > 0)
              {
                if (info.File == strFile /*|| pItem->GetLabel() == info.Title*/)
                {
                  string strThumb;
                  if (Utils.IsDVD(pItem.Path))
                    pItem.Label = String.Format("({0}:) {1}", pItem.Path.Substring(0, 1), info.Title);
                  strThumb = Utils.GetCoverArt(Thumbs.MovieTitle, info.Title);
                  if (System.IO.File.Exists(strThumb))
                  {
                    pItem.ThumbnailImage = strThumb;
                    pItem.IconImageBig = strThumb;
                    pItem.IconImage = strThumb;
                  }
                  break;
                }
              }
            } // of for (int i = 0; i < movies.Count; ++i)
          } // of if (System.IO.File.Exists(pItem.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        } // of if (pItem.IsFolder)
      } // of for (int x = 0; x < items.Count; ++x)

      movies.Clear();
      VideoDatabase.GetMoviesByPath(currentFolder, ref movies);
      for (int x = 0; x < items.Count; ++x)
      {
        pItem = (GUIListItem)items[x];
        if (!pItem.IsFolder
        || (pItem.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(pItem.Path).ToLower())))
        {
          if (pItem.ThumbnailImage != String.Empty) continue;
          for (int i = 0; i < movies.Count; ++i)
          {
            IMDBMovie info = (IMDBMovie)movies[i];
            string strFile = System.IO.Path.GetFileName(pItem.Path);
            if (info.File[0] == '\\' || info.File[0] == '/')
              info.File = info.File.Substring(1);

            if (strFile.Length > 0)
            {
              if (info.File == strFile /*|| pItem->GetLabel() == info.Title*/)
              {
                string strThumb;
                strThumb = Utils.GetCoverArt(Thumbs.MovieTitle, info.Title);
                if (System.IO.File.Exists(strThumb))
                {
                  pItem.ThumbnailImage = strThumb;
                  pItem.IconImageBig = strThumb;
                  pItem.IconImage = strThumb;
                }
                break;
              }
            }
          } // of for (int i = 0; i < movies.Count; ++i)
        } // of if (!pItem.IsFolder)
      } // of for (int x = 0; x < items.Count; ++x)
    }

    public bool CheckMovie(string movieFileName)
    {
      if (!VideoDatabase.HasMovieInfo(movieFileName)) return true;

      IMDBMovie movieDetails = new IMDBMovie();
      int idMovie = VideoDatabase.GetMovieInfo(movieFileName, ref movieDetails);
      if (idMovie < 0) return true;
      return CheckMovie(idMovie);
    }

    static public bool CheckMovie(int idMovie)
    {
      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);

      if (!Utils.IsDVD(movieDetails.Path)) return true;
      string cdlabel = String.Empty;
      cdlabel = Utils.GetDriveSerial(movieDetails.Path);
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
              cdlabel = Utils.GetDriveSerial(movieDetails.Path);
              VideoDatabase.UpdateCDLabel(movieDetails, cdlabel);
              movieDetails.CDLabel = cdlabel;
              return true;
            }
          }
          else
          {
            cdlabel = Utils.GetDriveSerial(movieDetails.Path);
            if (cdlabel.Equals(movieDetails.CDLabel)) return true;
          }
        }
        else break;
      }
      return false;
    }

    void OnManualIMDB()
    {
      currentSelectedItem = facadeView.SelectedListItemIndex;
      string strInput = String.Empty;
      GetStringFromKeyboard(ref strInput);
      if (strInput == String.Empty) return;

      //string strThumb;
      //      CUtil::GetThumbnail("Z:\\",strThumb);
      //      ::DeleteFile(strThumb.c_str());

      if (ShowIMDB(-1, strInput, String.Empty, String.Empty, false))
        LoadDirectory(currentFolder);
      return;
    }

    static public void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      strLine = String.Empty;
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

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
      return "My Videos";
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
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
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

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null) return;
      if (item.Label == "..")
      {
        filmstrip.InfoImageFileName = String.Empty;
        return;
      }

      if (item.IsFolder) filmstrip.InfoImageFileName = item.ThumbnailImage;
      else filmstrip.InfoImageFileName = Utils.ConvertToLargeCoverArt(item.ThumbnailImage);
    }

    static public void PlayMovieFromPlayList(bool askForResumeMovie)
    {
      PlayMovieFromPlayList(askForResumeMovie, -1);
    }
    static public void PlayMovieFromPlayList(bool askForResumeMovie, int iMovieIndex)
    {
      string filename;
      if (iMovieIndex == -1)
        filename = playlistPlayer.GetNext();
      else
        filename = playlistPlayer.Get(iMovieIndex);

      IMDBMovie movieDetails = new IMDBMovie();
      VideoDatabase.GetMovieInfo(filename, ref movieDetails);
      int idFile = VideoDatabase.GetFileId(filename);
      int idMovie = VideoDatabase.GetMovieId(filename);
      int timeMovieStopped = 0;
      byte[] resumeData = null;
      if ((idMovie >= 0) && (idFile >= 0))
      {
        timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
        Log.Write("GUIVideoFiles::OnPlayBackStopped idFile={0} timeMovieStopped={1} resumeData={2}", idFile, timeMovieStopped, resumeData);
        if (timeMovieStopped > 0)
        {
          string title = System.IO.Path.GetFileName(filename);
          VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
          if (movieDetails.Title != String.Empty) title = movieDetails.Title;

          if (askForResumeMovie)
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, title);
            dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSString(timeMovieStopped));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (!dlgYesNo.IsConfirmed) timeMovieStopped = 0;
          }
        }
      }

      if (iMovieIndex == -1)
      {
        playlistPlayer.PlayNext();
      }
      else
      {
        playlistPlayer.Play(iMovieIndex);
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

    static public ArrayList PlayMountedImageFile(int WindowID, string file)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(13013);
        dlgProgress.SetLine(1, System.IO.Path.GetFileNameWithoutExtension(file));
        dlgProgress.StartModal(WindowID);
        dlgProgress.Progress();
      }

      ArrayList itemlist = m_directory.GetDirectory(file);
      if (itemlist.Count == 1 && file != String.Empty) return itemlist; // protected share, with wrong pincode

      if (DaemonTools.IsMounted(file) && !g_Player.Playing)
      {
        string strDir = DaemonTools.GetVirtualDrive();

        // Check if the mounted image is actually a DVD. If so, bypass
        // autoplay to play the DVD without user intervention
        if (System.IO.File.Exists(strDir + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          playlistPlayer.Reset();
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
          PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
          playlist.Clear();

          PlayListItem newitem = new PlayListItem();
          newitem.FileName = strDir + @"\VIDEO_TS\VIDEO_TS.IFO";
          newitem.Type = Playlists.PlayListItem.PlayListItemType.Video;
          playlist.Add(newitem);

          Log.Write("\"Autoplaying\" DVD image mounted on {0}", strDir);
          PlayMovieFromPlayList(true);
        }
      }

      if (dlgProgress != null) dlgProgress.Close();

      return itemlist;
    }

    private void OnPlayBackStopped(MediaPortal.Player.g_Player.MediaType type, int timeMovieStopped, string filename)
    {
      if (type != g_Player.MediaType.Video) return;

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      int iidMovie = VideoDatabase.GetMovieId(filename);
      VideoDatabase.GetFiles(iidMovie, ref movies);
      if (movies.Count <= 0) return;
      for (int i = 0; i < movies.Count; i++)
      {
        string strFilePath = (string)movies[i];
        int idFile = VideoDatabase.GetFileId(strFilePath);
        if (idFile < 0) break;
        if ((filename == strFilePath) && (timeMovieStopped > 0))
        {
          byte[] resumeData = null;
          g_Player.Player.GetResumeState(out resumeData);
          Log.Write("GUIVideoFiles::OnPlayBackStopped idFile={0} timeMovieStopped={1} resumeData={2}", idFile, timeMovieStopped, resumeData);
          VideoDatabase.SetMovieStopTimeAndResumeData(idFile, timeMovieStopped, resumeData);
          Log.Write("GUIVideoFiles::OnPlayBackStopped store resume time");
        }
        else
          VideoDatabase.DeleteMovieStopTime(idFile);
      }
    }

    private void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Video) return;

      // Handle all movie files from idMovie
      ArrayList movies = new ArrayList();
      int iidMovie = VideoDatabase.GetMovieId(filename);
      if (iidMovie >= 0)
      {
        VideoDatabase.GetFiles(iidMovie, ref movies);
        for (int i = 0; i < movies.Count; i++)
        {
          string strFilePath = (string)movies[i];
          int idFile = VideoDatabase.GetFileId(strFilePath);
          if (idFile < 0) break;
          VideoDatabase.DeleteMovieStopTime(idFile);
        }

        IMDBMovie details = new IMDBMovie();
        VideoDatabase.GetMovieInfoById(iidMovie, ref details);
        details.Watched++;
        VideoDatabase.SetWatched(details);
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
      dlg.SetHeading(924); // menu

      if (!mapSettings.Stack) dlg.AddLocalizedString(346); //Stack
      else dlg.AddLocalizedString(347); //Unstack
      dlg.AddLocalizedString(654); //Eject

      if (!facadeView.Focus)
      {
        // Menu button context menuu
        dlg.AddLocalizedString(368); //IMDB
        if (!m_directory.IsRemote(currentFolder)) dlg.AddLocalizedString(102); //Scan
      }
      else
      {
        if ((System.IO.Path.GetFileName(item.Path) != String.Empty) || Utils.IsDVD(item.Path))
        {
          if (Utils.getDriveType(item.Path) != 5) dlg.AddLocalizedString(925); //delete
          dlg.AddLocalizedString(368); //IMDB
          dlg.AddLocalizedString(99845); //TV.com
          dlg.AddLocalizedString(208); //play
          dlg.AddLocalizedString(926); //Queue
        }
        if (Utils.getDriveType(item.Path) == 5) dlg.AddLocalizedString(654); //Eject

        int iPincodeCorrect;
        if (!m_directory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && fileMenuEnabled)
        {
          dlg.AddLocalizedString(500); // FileMenu
        }
      }

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

        case 99845: // TV.com
          onTVcom(itemNo);
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
          if (Utils.getDriveType(item.Path) != 5) Utils.EjectCDROM();
          else Utils.EjectCDROM(System.IO.Path.GetPathRoot(item.Path));
          LoadDirectory(String.Empty);
          break;

        case 341: //Play dvd
          OnPlayDVD();
          break;

        case 346: //Stack
          mapSettings.Stack = true;
          LoadDirectory(currentFolder);
          UpdateButtonStates();
          break;

        case 347: //Unstack
          mapSettings.Stack = false;
          LoadDirectory(currentFolder);
          UpdateButtonStates();
          break;

        case 102: //Scan
          ArrayList itemlist = m_directory.GetDirectory(currentFolder);
          OnScan(itemlist);
          LoadDirectory(currentFolder);
          break;

        case 500: // File menu
          {
            // get pincode
            if (fileMenuPinCode != String.Empty)
            {
              string userCode = String.Empty;
              if (GetUserInputString(ref userCode) && userCode == fileMenuPinCode)
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

    bool GetUserInputString(ref string sString)
    {
      VirtualSearchKeyboard keyBoard = (VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
      keyBoard.Reset();
      keyBoard.Text = sString;
      keyBoard.DoModal(GetID); // show it...
      if (keyBoard.IsConfirmed) sString = keyBoard.Text;
      return keyBoard.IsConfirmed;
    }

    void OnShowFileMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null) return;
      if (item.IsFolder && item.Label == "..") return;

      // init
      GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
      if (dlgFile == null) return;

      // File operation settings
      dlgFile.SetSourceItem(item);
      dlgFile.SetSourceDir(currentFolder);
      dlgFile.SetDestinationDir(destinationFolder);
      dlgFile.SetDirectoryStructure(m_directory);
      dlgFile.DoModal(GetID);
      destinationFolder = dlgFile.GetDestinationDir();

      //final
      if (dlgFile.Reload())
      {
        int selectedItem = facadeView.SelectedListItemIndex;
        if (currentFolder != dlgFile.GetSourceDir()) selectedItem = -1;

        //currentFolder = System.IO.Path.GetDirectoryName(dlgFile.GetSourceDir());
        LoadDirectory(currentFolder);
        if (selectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItem);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
    }

    void OnDeleteItem(GUIListItem item)
    {
      if (item.IsRemote) return;

      IMDBMovie movieDetails = new IMDBMovie();

      int idMovie = VideoDatabase.GetMovieInfo(item.Path, ref movieDetails);

      string movieFileName = System.IO.Path.GetFileName(item.Path);
      string movieTitle = movieFileName;
      if (idMovie >= 0) movieTitle = movieDetails.Title;

      //get all movies belonging to each other
      if (mapSettings.Stack)
      {
        bool bStackedFile = false;
        ArrayList items = m_directory.GetDirectory(currentFolder);
        int iPart = 1;
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          string fname1 = System.IO.Path.GetFileNameWithoutExtension(temporaryListItem.Path).ToLower();
          string fname2 = System.IO.Path.GetFileNameWithoutExtension(item.Path).ToLower();
          if (Utils.ShouldStack(temporaryListItem.Path, item.Path) || fname1.Equals(fname2))
          {
            bStackedFile = true;
            movieFileName = System.IO.Path.GetFileName(temporaryListItem.Path);
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
            dlgYesNo.SetLine(1, movieFileName);
            dlgYesNo.SetLine(2, String.Format("Part:{0}", iPart++));
            dlgYesNo.SetLine(3, String.Empty);
            dlgYesNo.DoModal(GetID);

            if (!dlgYesNo.IsConfirmed) break;
            DoDeleteItem(temporaryListItem);
          }
        }

        if (!bStackedFile)
        {
          // delete single file
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null == dlgYesNo) return;
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
          dlgYesNo.SetLine(1, movieTitle);
          dlgYesNo.SetLine(2, String.Empty);
          dlgYesNo.SetLine(3, String.Empty);
          dlgYesNo.DoModal(GetID);

          if (!dlgYesNo.IsConfirmed) return;
          DoDeleteItem(item);
        }
      }
      else // stacking off
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo) return;

        if (!dlgYesNo.IsConfirmed) return;
        ArrayList items = m_directory.GetDirectory(currentFolder);
        int iPart = 1;
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          string fname1 = System.IO.Path.GetFileNameWithoutExtension(temporaryListItem.Path).ToLower();
          string fname2 = System.IO.Path.GetFileNameWithoutExtension(item.Path).ToLower();
          if (fname1.Equals(fname2))
          {
            movieFileName = System.IO.Path.GetFileName(temporaryListItem.Path);
            dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo) return;
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
            dlgYesNo.SetLine(1, movieFileName);
            dlgYesNo.SetLine(2, String.Format("Part:{0}", iPart++));
            dlgYesNo.SetLine(3, String.Empty);
            dlgYesNo.DoModal(GetID);

            if (dlgYesNo.IsConfirmed)
            {
              DoDeleteItem(temporaryListItem);
            }
          }
        }
      }

      currentSelectedItem = facadeView.SelectedListItemIndex;
      if (currentSelectedItem > 0) currentSelectedItem--;
      LoadDirectory(currentFolder);
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSelectedItem);
      }
    }

    void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
        if (item.IsRemote) return;
        if (item.Label != "..")
        {
          ArrayList items = new ArrayList();
          items = m_directory.GetDirectoryUnProtected(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }
          Utils.DirectoryDelete(item.Path);
        }
      }
      else
      {
        VideoDatabase.DeleteMovie(item.Path);
        TVDatabase.RemoveRecordedTVByFileName(item.Path);

        if (item.IsRemote) return;
        Utils.FileDelete(item.Path);
      }
    }

    static public bool IsFolderPinProtected(string folder)
    {
      int pinCode = 0;
      if (m_directory.IsProtectedShare(folder, out pinCode)) return true;
      return false;
    }
    static void DownloadThumnail(string folder, string url, string name)
    {
      if (url == null) return;
      if (url.Length == 0) return;
      string strThumb = Utils.GetCoverArtName(folder, name);
      string LargeThumb = Utils.GetLargeCoverArtName(folder, name);
      if (!System.IO.File.Exists(strThumb))
      {
        string strExtension;
        strExtension = System.IO.Path.GetExtension(url);
        if (strExtension.Length > 0)
        {
          string strTemp = "temp";
          strTemp += strExtension;
          Utils.FileDelete(strTemp);

          Utils.DownLoadImage(url, strTemp);
          if (System.IO.File.Exists(strTemp))
          {
            MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
            MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);
          }
          else Log.Write("Unable to download {0}->{1}", url, strTemp);
          Utils.FileDelete(strTemp);
        }
      }
    }
    static void DownloadDirector(IMDBMovie movieDetails)
    {
      string actor = movieDetails.Director;
      string strThumb = Utils.GetCoverArtName(Thumbs.MovieActors, actor);
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
            ShowProgress(GUILocalizeStrings.Get(1009), actor, "", 0);
            DownloadThumnail(Thumbs.MovieActors, imdbActor.ThumbnailUrl, actor);
          }
          else Log.Write("url=empty for actor {0}", actor);
        }
        else Log.Write("url=null for actor {0}", actor);
      }
    }
    static void DownloadActors(IMDBMovie movieDetails)
    {
      string[] actors = movieDetails.Cast.Split('\n');
      if (actors.Length > 1)
      {
        for (int i = 1; i < actors.Length; ++i)
        {
          int percent = (int)(i * 100) / (1 + actors.Length);
          int pos = actors[i].IndexOf(" as ");
          if (pos < 0) continue;
          string actor = actors[i].Substring(0, pos);
          string strThumb = Utils.GetCoverArtName(Thumbs.MovieActors, actor);
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
                  VideoDatabase.AddActorInfo(actorId, imdbActor);
                }
                ShowProgress(GUILocalizeStrings.Get(1009), actor, "", percent);
                DownloadThumnail(Thumbs.MovieActors, imdbActor.ThumbnailUrl, actor);
              }
              else Log.Write("url=empty for actor {0}", actor);
            }
            else Log.Write("url=null for actor {0}", actor);
          }
        }
      }
    }

    static public void Reset()
    {
      m_directory.Reset();
    }

    static public void PlayMovie(int idMovie)
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

        bool askBeforePlayingDVDImage = false;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          askBeforePlayingDVDImage = xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
        }

        if (!askBeforePlayingDVDImage)
        {
          GUIVideoFiles.PlayMountedImageFile(GUIWindowManager.ActiveWindow, (string)movies[0]);
        }
        else
        {
          // GetDirectory mounts the image file
          ArrayList itemlist = m_directory.GetDirectory((string)movies[0]);
        }

        return;
      }

      bool askForResumeMovie = true;
      int movieDuration = 0;
      {
        //get all movies belonging to each other
        ArrayList items = m_directory.GetDirectory(System.IO.Path.GetDirectoryName((string)movies[0]));
        if (items.Count <= 1) return; // first item always ".." so 1 item means prob. protected share

        //check if we can resume 1 of those movies
        int timeMovieStopped = 0;
        bool asked = false;
        ArrayList newItems = new ArrayList();
        for (int i = 0; i < items.Count; ++i)
        {
          GUIListItem temporaryListItem = (GUIListItem)items[i];
          if ((Utils.ShouldStack(temporaryListItem.Path, (string)movies[0])) && (movies.Count > 1))
          {
            if (!asked) selectedFileIndex++;
            IMDBMovie movieDetails = new IMDBMovie();
            int idFile = VideoDatabase.GetFileId(temporaryListItem.Path);
            if ((idMovie >= 0) && (idFile >= 0))
            {
              VideoDatabase.GetMovieInfo((string)movies[0], ref movieDetails);
              string title = System.IO.Path.GetFileName((string)movies[0]);
              Utils.RemoveStackEndings(ref title);
              if (movieDetails.Title != String.Empty) title = movieDetails.Title;

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
                  dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + Utils.SecondsToHMSString(movieDuration + timeMovieStopped));
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
          }//if (Utils.ShouldStack(temporaryListItem.Path, item.Path))
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
          if (Utils.IsVideo(temporaryListItem.Path) && !PlayListFactory.IsPlayList(temporaryListItem.Path))
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

      playlistPlayer.Reset();
      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP);
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


    private void onTVcom(string pathAndFilename, out string newPathAndFilename)
    {

      string strFileName = pathAndFilename.Remove(0, pathAndFilename.LastIndexOf("\\") + 1);
      string strPath = pathAndFilename.Substring(0, pathAndFilename.LastIndexOf("\\"));
      newPathAndFilename = pathAndFilename;
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);

      // show dialog that we're downloading the movie info

      pDlgProgress.SetHeading("Now querying TV.com");
      pDlgProgress.SetLine(1, "Trying to gather Info from filename...");
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      pDlgProgress.Progress();

      currentSelectedItem = facadeView.SelectedListItemIndex;
      GUIDialogSelect dlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();

      tvDotComParser tvParser = new tvDotComParser();

      Log.WriteFile(Log.LogType.TVCom, "Starting Operation on \"" + strFileName + "\"");
      // we get info from the filename
      int season, ep;
      string showname, episodeTitle = string.Empty;
      string oldFilename = "";
      string[] searchResults;
      int pick = 0;
      bool repeat = false; // if set to false we dont repeat and cancel out


      bool searchDone = false;
      do
      {
        string tmpFilename = strFileName;
        while (searchDone || (!tvParser.getSeasonEpisode(tmpFilename, out season, out ep, out showname) && !(TVcomSettings.lookupIfNoSEinFilename && tvParser.getShownameEpisodeTitleOnly(tmpFilename, out showname, out episodeTitle))))
        {
          if (searchDone)
            Log.WriteFile(Log.LogType.TVCom, "Showing Keyboard..");
          else
            Log.WriteFile(Log.LogType.TVCom, "Could not get enough Info from Filename. Showing Keyboard..");

          searchDone = false;
          oldFilename = tmpFilename;
          GetStringFromKeyboard(ref tmpFilename);
          Log.WriteFile(Log.LogType.TVCom, "User entered: " + tmpFilename);
          if (oldFilename == tmpFilename || tmpFilename.Length == 0)
          {
            return;
          }
        }

        //ShowProgress("Now querying TV.com","Info from Filename found!","",10);
        pDlgProgress.SetPercentage(10);
        showname = showname.Replace("_", " ").Replace("-", "").Replace(".", " ").Replace(":", "").Replace("  ", " ").Trim().ToLower();

        if (season > 0)
        {
          // we use seasonno + epno to search

          Log.WriteFile(Log.LogType.TVCom, "I think this file is \"" + showname + "\" Season: " + season.ToString() + " Episode: " + ep.ToString());
          pDlgProgress.SetLine(2, "\"" + showname + "\" - Season: " + season.ToString() + " - Episode: " + ep.ToString());
          //ShowProgress("Now querying TV.com","Info from Filename found!","\"" + showname + "\" - Season: " + season.ToString() + " - Episode: " + ep.ToString(),15);
        }
        else if (episodeTitle != string.Empty)
        {
          // we use the episodetitle to search

          Log.WriteFile(Log.LogType.TVCom, "I think this file is \"" + showname + "\" EpisodeTitle: " + episodeTitle);
          pDlgProgress.SetLine(2, "\"" + showname + "\" - Episode: \"" + episodeTitle + "\"");
          //ShowProgress("Now querying TV.com","Info from Filename found!","\"" + showname + "\" - Episode: " + episodeTitle,15);
        }

        Log.WriteFile(Log.LogType.TVCom, "Calling Search..");


        pDlgProgress.SetLine(1, "Starting Search...");

        pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
        pDlgProgress.Progress();
        pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);

        searchResults = tvParser.searchMapping(showname);

        if (searchResults[0] != "-1")
          Log.WriteFile(Log.LogType.TVCom, "Mapping found, skipping search.");
        else
        {
          // means no mapping found, we will search, otherwise we skip search altogether
          oldFilename = (showname);
          searchDone = true;
          // we search for shows matching
          searchResults = tvParser.getSearchResultsFromTitle(showname);
          pick = 0;
          // we display the results (if more than one match)
          if (searchResults.Length > 2)
          {
            Log.WriteFile(Log.LogType.TVCom, searchResults.Length / 2 + " Results found, awaiting user selection...");
            pDlgProgress.Close();
            dlg.Reset();
            dlg.SetHeading("Search results for: " + showname);
            for (int i = 0; i < searchResults.Length; i += 2)
              dlg.Add(searchResults[i]);
            dlg.Add("Manual Input");
            dlg.DoModal(GetID);
            pick = dlg.SelectedId;
            pick = (pick * 2 - 2);
            try
            {
              Log.WriteFile(Log.LogType.TVCom, "User picked number " + pick.ToString() + ": " + searchResults[pick]);
            }
            catch
            {
              // most likely the user selected manual on the resultlit
              // or pressed esc
              // we want to return to the keyboard input
              Log.WriteFile(Log.LogType.TVCom, "User selected to manually input...");
              searchResults = new string[0];
              repeat = true;

            }

            pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
          }
          else if (searchResults.Length == 0)
          {
            Log.WriteFile(Log.LogType.TVCom, "No results for: " + showname + " -> aborting...");
            Log.WriteFile(Log.LogType.TVCom, "--------------------------");
            //pDlgProgress.Close();
            pDlgOK.SetHeading("No results");
            pDlgOK.SetLine(1, "Sorry, no results returned on: " + showname);
            pDlgOK.DoModal(GetID);
            if (repeat)
              return;
            searchResults = new string[0];
            repeat = true;
          }
          else
            Log.WriteFile(Log.LogType.TVCom, "Only one result: " + searchResults[pick] + " , skipping Resultlist.");
        }

      } while (searchResults.Length == 0 && repeat && searchDone);

      if (searchDone)
      {
        tvParser.writeMapping(showname, searchResults[pick], searchResults[pick + 1]);
        if (oldFilename != "" && oldFilename != showname)
          tvParser.writeMapping(oldFilename, searchResults[pick], searchResults[pick + 1]);
      }

      pDlgProgress.SetLine(1, "Working, please be patient...");
      pDlgProgress.SetLine(2, "\"" + searchResults[pick] + "\" - Season: " + season.ToString() + " - Episode: " + ep.ToString());
      pDlgProgress.SetPercentage(30);
      pDlgProgress.Progress();
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);

      //ShowProgress("Now querying TV.com","Show and Episode identified!","\"" + searchResults[pick] + "\" - Season: " + season.ToString() + " - Episode: " + ep.ToString(),35);

      Log.WriteFile(Log.LogType.TVCom, "Now downloading Guides, this may take some time...");



      // we download the guides

      bool reDownload = false;
      bool freshlydownloaded = false;
      int count = 0;

      // ** case: we do a search by episode title
      if (season == -1 && ep == -1)
      {
        System.Collections.SortedList sl;


        if (episodeTitle != string.Empty)
        {
          pDlgProgress.SetLine(1, "\"" + searchResults[pick] + "\" - Episode: " + episodeTitle + "\"");
          pDlgProgress.SetLine(2, "Trying to match Episodetitle ...");
          pDlgProgress.ContinueModal();
          pDlgProgress.Progress();

          //ShowProgress("Now querying TV.com","Trying to match Episodetitle now...","\"" + searchResults[pick] + "\" - Episode: " + episodeTitle + "\"",35);
          tvParser.searchEpisodebyTitle(searchResults[pick], searchResults[pick + 1], episodeTitle, out sl, false);
        }
        else
        {

          sl = new SortedList();
        }

        string correctTitle;
        if (sl.Count != 0)
        {

          if (sl.ContainsKey(1))
          {
            Log.WriteFile(Log.LogType.TVCom, "Exact Match on Episode Title found:");

            string[] split;
            string tmp = (string)sl[1];
            split = tmp.Split('|');
            correctTitle = tmp.Split('|')[0];
            season = Convert.ToInt32(tmp.Split('|')[1]);
            ep = Convert.ToInt32(tmp.Split('|')[2]);
            // we had a direct hit

            Log.WriteFile(Log.LogType.TVCom, "Direct Hit on EpisodeTitle Search: " + correctTitle + " - " + season.ToString() + "x" + ep.ToString());
          }
          else
          {
            Log.WriteFile(Log.LogType.TVCom, "Possible Episode Hits:");
            Log.WriteFile(Log.LogType.TVCom, sl.Count + " Results found, awaiting user selection...");
            pDlgProgress.Close();
            dlg.Reset();
            dlg.SetHeading("Search results for: " + showname + " - " + episodeTitle);
            for (int i = sl.Count - 1; i >= 0; i--)
            {
              string tmp1 = (string)sl.GetByIndex(i);
              correctTitle = tmp1.Split('|')[0];
              season = Convert.ToInt32(tmp1.Split('|')[1]);
              ep = Convert.ToInt32(tmp1.Split('|')[2]);

              Log.WriteFile(Log.LogType.TVCom, correctTitle + " - " + season.ToString() + "x" + ep.ToString());

              dlg.Add(correctTitle + " - " + season.ToString() + "x" + ep.ToString());

            }
            dlg.Add("Show all Episodes!");

            try
            {
              dlg.DoModal(GetID);
              int pick2 = sl.Count - dlg.SelectedId;
              if (pick2 < 0)
              {
                // user selected to show all episodes
                // we reset sl to force an all eps lookup below
                sl = new SortedList();
                // we also erase the episodetitle
                episodeTitle = string.Empty;
                Log.WriteFile(Log.LogType.TVCom, "User manually selected to show all Episodes");

              }
              //string tmp2 = (string)sl.GetByIndex(sl.Count - Convert.ToInt32(EnterMessageBox.input) +1);
              string tmp = (string)sl.GetByIndex(pick2);
              //correctTitle = tmp.Split(';')[0];
              season = Convert.ToInt32(tmp.Split('|')[1]);
              ep = Convert.ToInt32(tmp.Split('|')[2]);
              Log.WriteFile(Log.LogType.TVCom, "User picked number " + season.ToString() + "x" + ep.ToString());
            }
            catch
            {
              // most likely the user pressed escape on the resultlist
              Log.WriteFile(Log.LogType.TVCom, "Pressed ESC on Episode Results?");
              if (episodeTitle != string.Empty)
                return;
            }


          }
        }
        if (sl.Count == 0)
        {
          Log.WriteFile(Log.LogType.TVCom, "Showing all Episodes...");
          pDlgProgress.SetPercentage(80);
          if (episodeTitle != string.Empty)
          {
            pDlgProgress.SetLine(1, "Sorry, no results found!");

          }
          pDlgProgress.SetLine(2, "\"" + searchResults[pick] + "\"");
          pDlgProgress.SetLine(3, "Getting List of Episodes ...");
          //pDlgProgress.ContinueModal();
          pDlgProgress.Progress();

          //return;

          // showing all episodes

          tvParser.searchEpisodebyTitle(searchResults[pick], searchResults[pick + 1], episodeTitle, out sl, true);
          Log.WriteFile(Log.LogType.TVCom, sl.Count.ToString());
          pDlgProgress.Close();
          dlg.Reset();
          dlg.SetHeading("Episodes for: " + searchResults[pick]);

          for (int i = 0; i < sl.Count; i++)
          {
            string tmp1 = (string)sl.GetByIndex(i);
            correctTitle = tmp1.Split('|')[0];
            season = Convert.ToInt32(tmp1.Split('|')[1]);
            ep = Convert.ToInt32(tmp1.Split('|')[2]);

            Log.WriteFile(Log.LogType.TVCom, correctTitle + " - " + season.ToString() + "x" + ep.ToString());

            dlg.Add(season.ToString() + "x" + ep.ToString("00") + " - " + correctTitle);

          }
          dlg.Add("Start over!");

          try
          {
            dlg.DoModal(GetID);

            int pick2 = dlg.SelectedId - 1;
            Log.WriteFile(Log.LogType.TVCom, pick2.ToString());

            //string tmp2 = (string)sl.GetByIndex(sl.Count - Convert.ToInt32(EnterMessageBox.input) +1);
            string tmp = (string)sl.GetByIndex(pick2);
            //correctTitle = tmp.Split(';')[0];
            season = Convert.ToInt32(tmp.Split('|')[1]);
            ep = Convert.ToInt32(tmp.Split('|')[2]);
            Log.WriteFile(Log.LogType.TVCom, "User picked number " + season.ToString() + "x" + ep.ToString());
          }
          catch
          {
            // most likely the user pressed escape on the resultlist
            // or he selected Start over

            Log.WriteFile(Log.LogType.TVCom, "Either selected to Start over or pressed ESC on Episode Results?");
            onTVcom(strFileName, out newPathAndFilename);
            return;
          }
        }


      }

      // end case episodetitle search

      pDlgProgress.SetPercentage(50);
      pDlgProgress.SetLine(2, "\"" + searchResults[pick] + "\" - Season: " + season.ToString() + " - Episode: " + ep.ToString());
      pDlgProgress.SetLine(1, "Currently Downloading...");
      pDlgProgress.Progress();
      pDlgProgress.ContinueModal();



      do
      {
        count++;
        Log.WriteFile(Log.LogType.TVCom, "starting download - loop: " + count.ToString());
        if ((freshlydownloaded = tvParser.downloadGuides(searchResults[pick + 1], season, ep, searchResults[pick], reDownload)))
        {
          Log.WriteFile(Log.LogType.TVCom, "Download complete!");
        }
        else
        {
          Log.WriteFile(Log.LogType.TVCom, "There was an error during the download, or the download was skipped, it might still work though...");
        }

        pDlgProgress.SetPercentage(90);
        pDlgProgress.SetLine(1, "Now parsing info...");
        pDlgProgress.Progress();
        pDlgProgress.ContinueModal();

        // we parse the info and write to the db
        try
        {
          Log.WriteFile(Log.LogType.TVCom, "Trying to get info from Downloaded Files...");

          episode_info ef = tvParser.getEpisodeInfo(searchResults[pick], season, ep);

          if (ef.description.ToLower().IndexOf("coming soon") != -1 && ef.description.Length < 75 && !freshlydownloaded)
          {
            Log.WriteFile(Log.LogType.TVCom, "Episode Descripton contained \"coming soon\", so we will redownload a fresh copy of the guide...");
            reDownload = true;
          }
          else if (freshlydownloaded && ef.description.IndexOf("coming soon") != -1 && ef.description.Length < 75)
          {

          }
          else
          {

            pDlgProgress.SetPercentage(95);
            pDlgProgress.SetLine(1, "Performing Final Steps...");
            pDlgProgress.ContinueModal();
            pDlgProgress.Progress();

            IMDBMovie movieDetails = new IMDBMovie();
            movieDetails.Director = ef.director;
            movieDetails.WritingCredits = ef.writer;
            movieDetails.Year = ef.firstAired.Year;
            //movieDetails.Title = searchResults[pick];

            Log.WriteFile(Log.LogType.TVCom, "Trying to construct Title...");

            // for title and filename we need to clear genre of the "/" seperator for multiple genres
            ef.genre = ef.genre.Replace("/", " ").Replace("  ", " ");

            movieDetails.Genre = TVcomSettings.genreFormat
              .Replace("[SHOWNAME]", searchResults[pick])
              .Replace("[SEASONNO]", season.ToString())
              .Replace("[EPISODENO]", ep.ToString("00"))
              .Replace("[EPISODETITLE]", ef.title)
              .Replace("[AIRDATE]", ef.firstAired.ToShortDateString())
              .Replace("[AIRTIME]", ef.airtime)
              .Replace("[CHANNEL]", ef.network)
              .Replace("[GENRE]", ef.genre);

            movieDetails.Title = TVcomSettings.titleFormat
              .Replace("[SHOWNAME]", searchResults[pick])
              .Replace("[SEASONNO]", season.ToString())
              .Replace("[EPISODENO]", ep.ToString("00"))
              .Replace("[EPISODETITLE]", ef.title)
              .Replace("[AIRDATE]", ef.firstAired.ToShortDateString())
              .Replace("[AIRTIME]", ef.airtime)
              .Replace("[CHANNEL]", ef.network)
              .Replace("[GENRE]", ef.genre);

            // since we need to rename the thumbnail to the title, we have to ensure no illegal chars

            movieDetails.Title = tvParser.getFilennameFriendlyString(movieDetails.Title);

            Log.WriteFile(Log.LogType.TVCom, "Title constructed: " + movieDetails.Title);

            movieDetails.RunTime = ef.runtime;

            movieDetails.TagLine = season.ToString() + "x" + ep.ToString("00") + " - " + ef.title;
            movieDetails.PlotOutline = movieDetails.TagLine;
            movieDetails.MPARating = movieDetails.TagLine; // we use this to display the episodes title, there is no real use for this field anyways.

            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");

            System.Threading.Thread.CurrentThread.CurrentCulture = culture;

            movieDetails.Plot = "Episode Premiered: " + ef.firstAired.ToLongDateString() +
              "\n\n" + ef.description +
              "\n-------------\n" + searchResults[pick] + ":" +
              "\n  Premiered: " + ef.seriesPremiere.ToLongDateString() +
              "\n  Airs: " + ef.airtime + " on " + ef.network +
              "\n  Runtime: " + ef.runtime.ToString() + " mins" +
              "\n  Status: " + ef.status +
              "\n  Genre: " + ef.genre +
              "\n\n" + ef.seriesDescription +
              "\n-------------\n-------------\n";

            movieDetails.Rating = (float)ef.rating;
            movieDetails.Votes = ef.numberOfRatings.ToString();

            string cast = "";
            if (ef.stars.Count > 0 && ef.starsCharacters.Count > 0)
            {
              for (int i = 0; i < ef.stars.Count && i < ef.starsCharacters.Count; i++)
              {
                cast += ef.stars[i] + " as " + ef.starsCharacters[i] + "\n";
              }
            }
            if (ef.guestStars.Count > 0 && ef.guestStarsCharacters.Count > 0)
            {
              for (int i = 0; i < ef.guestStars.Count && i < ef.guestStarsCharacters.Count; i++)
              {
                cast += ef.guestStars[i] + " as " + ef.guestStarsCharacters[i] + "\n";
              }
              cast.Remove(cast.Length - 2, 2); // remove the last \n
            }

            movieDetails.Cast = cast;

            movieDetails.File = pathAndFilename.Remove(0, pathAndFilename.LastIndexOf("\\") + 1);
            movieDetails.Path = strPath;

            if (TVcomSettings.lookupActors)
            {
              Log.WriteFile(Log.LogType.TVCom, "Getting info for Actors and Director (imdb function). This might take some time...");
              DownloadActors(movieDetails);
              DownloadDirector(movieDetails);
              Log.WriteFile(Log.LogType.TVCom, "Actor Information downloaded!");
            }

            if (TVcomSettings.renameFiles)
            {
              if (episodeTitle == string.Empty || !TVcomSettings.renameOnlyIfNoSEinFilename)
              {
                string newFilename = string.Empty;
                if (
                  (TVcomSettings.renameFormat.IndexOf("[EPISODETITLE]") != -1) ||
                  (TVcomSettings.renameFormat.IndexOf("[EPISODENO]") != -1 && TVcomSettings.renameFormat.IndexOf("[SEASONNO]") != -1))
                {

                  newFilename = TVcomSettings.renameFormat
                    .Replace("[SHOWNAME]", searchResults[pick])
                    .Replace("[SEASONNO]", season.ToString())
                    .Replace("[EPISODENO]", ep.ToString("00"))
                    .Replace("[EPISODETITLE]", ef.title)
                    .Replace("[GENRE]", ef.genre)
                    .Replace("[AIRDATE]", ef.firstAired.ToShortDateString())
                    .Replace("[AIRTIME]", ef.airtime)
                    .Replace("[CHANNEL]", ef.network)
                    + Path.GetExtension(pathAndFilename);

                  newFilename = tvParser.getFilennameFriendlyString(newFilename);

                  Log.WriteFile(Log.LogType.TVCom, "Renaming File...");
                  Log.WriteFile(Log.LogType.TVCom, "...Old Filename: " + strFileName);


                  if (TVcomSettings.replaceSpacesWith != ' ')
                    newFilename = newFilename.Replace(' ', TVcomSettings.replaceSpacesWith);
                  Log.WriteFile(Log.LogType.TVCom, "...New Filename: " + newFilename);

                  try
                  {
                    pDlgProgress.SetPercentage(92);
                    pDlgProgress.SetLine(3, "Renaming File...");
                    pDlgProgress.Progress();
                    System.IO.File.Move(pathAndFilename, Path.GetDirectoryName(pathAndFilename) + "/" + newFilename);
                    movieDetails.File = newFilename.Remove(0, pathAndFilename.LastIndexOf("\\") + 1);
                    movieDetails.Path = Path.GetDirectoryName(newFilename);
                    pathAndFilename = Path.GetDirectoryName(pathAndFilename) + "\\" + newFilename;
                    newPathAndFilename = pathAndFilename;
                    Log.WriteFile(Log.LogType.TVCom, "Sucessfully renamed File!");

                  }
                  catch (Exception e)
                  {
                    Log.WriteFile(Log.LogType.TVCom, "Could not rename File...is the Format correct?");
                    Log.WriteFile(Log.LogType.TVCom, e.Message);
                  }
                }
                else
                  Log.WriteFile(Log.LogType.TVCom, "Tried to rename, but the specified format is invalid - " + TVcomSettings.renameFormat);
              }
            }


            // rename the shows picture to the constructed title
            string picturePath = "thumbs/videos/title/";

            string currentPictureFilename = picturePath + searchResults[pick] + ".jpg";

            try
            {
              pDlgProgress.SetLine(3, "Setting Thumbnail...");
              pDlgProgress.SetPercentage(95);
              pDlgProgress.Progress();

              Log.WriteFile(Log.LogType.TVCom, "Trying to copy Image according to constructed Title: " + movieDetails.Title);

              System.IO.File.Copy(currentPictureFilename, picturePath + movieDetails.Title + ".jpg", false);
              System.IO.File.Copy(currentPictureFilename.Replace(".jpg", "L.jpg"), picturePath + movieDetails.Title + "L.jpg", false);

              Log.WriteFile(Log.LogType.TVCom, "Sucessfully created Thumbnail!");

            }
            catch (Exception e)
            {
              Log.WriteFile(Log.LogType.TVCom, "Could not copy Image...(probably the image already exists, however you might also have your title constructed in a way that you cannot name files, please double check:)");
              Log.WriteFile(Log.LogType.TVCom, e.Message);
            }

            try
            {
              if (TVcomSettings.genreFormat.IndexOf("[SHOWNAME]") != -1)
              {
                Log.WriteFile(Log.LogType.TVCom, "Since the Genre Format includes the Showname, we will also create a thumbnail for the Genre!");
                System.IO.File.Copy(currentPictureFilename, picturePath.Replace("title", "genre") + movieDetails.Genre + ".jpg", false);
                System.IO.File.Copy(currentPictureFilename.Replace(".jpg", "L.jpg"), picturePath.Replace("title", "genre") + movieDetails.Genre + "L.jpg", false);
                Log.WriteFile(Log.LogType.TVCom, "Sucessfully created Thumbnail!");
              }
            }
            catch (Exception e)
            {
              Log.WriteFile(Log.LogType.TVCom, "Could not copy Image...(probably the image already exists, however you might also have your genre constructed in a way that you cannot name files, please double check:)");
              Log.WriteFile(Log.LogType.TVCom, e.Message);
            }
            // write to db

            pDlgProgress.SetPercentage(98);
            pDlgProgress.SetLine(3, "Writing to Database..");
            pDlgProgress.ContinueModal();
            pDlgProgress.Progress();
            Log.WriteFile(Log.LogType.TVCom, "Now writing to DB...");
            VideoDatabase.AddMovie(pathAndFilename, false);
            VideoDatabase.SetMovieInfo(pathAndFilename, ref movieDetails);
          }
        }
        catch (Exception except)
        {
          Log.WriteFile(Log.LogType.TVCom, reDownload.ToString() + freshlydownloaded.ToString());
          if (reDownload)
          {
            Log.WriteFile(Log.LogType.TVCom, except.Message);
            dlg.Reset();
            dlg.Add("There was an error...");
            dlg.Add(except.Message);
            dlg.Add("Please see Log for more information.");
            dlg.DoModal(GetID);
          }
          else
          {
            if (!freshlydownloaded)
              reDownload = true;
          }

        }
      } while (reDownload && count < 2);

      pDlgProgress.Close();

    }

    private void onTVcom(int id)
    {

      int iSelectedItem = facadeView.SelectedListItemIndex;
      GUIListItem pItem = facadeView.SelectedListItem;
      GUIVideoInfo pDlgInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);

      if (VideoDatabase.HasMovieInfo(pItem.Path))
      {

        IMDBMovie existingInfo = new IMDBMovie();
        VideoDatabase.GetMovieInfo(pItem.Path, ref existingInfo);
        pDlgInfo.Movie = existingInfo;
        pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);
        return;
      }

      IMDBMovie movieDetails = new IMDBMovie();
      GUIListItem item = facadeView.SelectedListItem;
      int itemNo = facadeView.SelectedListItemIndex;
      if (item == null) return;
      if (pItem == null) return;
      if (pItem.IsRemote) return;
      if (pItem.IsFolder)
      {

        if (item.Label != "..")
        {
          Log.WriteFile(Log.LogType.TVCom, "Beginning Folder Operation!");
          Log.WriteFile(Log.LogType.TVCom, "___________________________");
          ArrayList items = new ArrayList();

          items = m_directory.GetDirectoryUnProtected(item.Path, true);
          bool skipFirstItem = true;
          string pathandFilename = string.Empty;
          foreach (GUIListItem subItem in items)
          {
            if (!skipFirstItem) // first one seems rubish
            {
              if (!subItem.IsFolder) // we dont want recursive
              {
                Log.WriteFile(Log.LogType.TVCom, "--------------------------");
                onTVcom(subItem.Path, out pathandFilename);
                Log.WriteFile(Log.LogType.TVCom, "--------------------------");
              }
            }
            skipFirstItem = false;
          }
          VideoDatabase.GetMovieInfo(pathandFilename, ref movieDetails);
          Log.WriteFile(Log.LogType.TVCom, "End Folder Operation!");
          Log.WriteFile(Log.LogType.TVCom, "___________________________");
          return;
        }
        else
          return;
      }
      else
      {
        // single file operation
        string pathandFilename;
        Log.WriteFile(Log.LogType.TVCom, "--------------------------");
        onTVcom(pItem.Path, out pathandFilename);
        Log.WriteFile(Log.LogType.TVCom, "--------------------------");
        VideoDatabase.GetMovieInfo(pathandFilename, ref movieDetails);
      }

      // display result

      facadeView.RefreshCoverArt();
      LoadDirectory(currentFolder);

      pDlgInfo.Movie = movieDetails;
      pDlgInfo.DoModal(GUIWindowManager.ActiveWindow);

    }

    #region IProgress Members

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
    static public void ShowProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted) return;
      GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      pDlgProgress.SetPercentage(percent);
      pDlgProgress.Progress();
    }

    #endregion

    private static void BackgroundImdbWorker(object sender, DoWorkEventArgs e)
    {
      using (WaitCursor wait = new WaitCursor())
      {
        IMDB imdb = new IMDB();

        object[] args = (object[])e.Argument;

        string movieFileName = (string)args[0];
        string strMovieName = (string)args[1];
        string strPath = (string)args[2];

        imdb.Find(strMovieName);

        if (imdb.Count == 0)
          return;

        IMDBMovie movieDetails = new IMDBMovie();
        movieDetails.SearchString = strMovieName;
        IMDB.IMDBUrl url = imdb[0];

        if (imdb.GetDetails(url, ref movieDetails) == false)
          return;

        // get & save thumbnail
        AmazonImageSearch search = new AmazonImageSearch();

        search.Search(movieDetails.Title);

        if (search.Count == 0)
          return;

        movieDetails.ThumbURL = search[0];

        VideoDatabase.SetMovieInfo(movieFileName, ref movieDetails);

        string strThumb = String.Empty;
        string strImage = movieDetails.ThumbURL;

        if (strImage.Length > 0 && movieDetails.ThumbURL.Length > 0)
        {
          string LargeThumb = Utils.GetLargeCoverArtName(Thumbs.MovieTitle, movieDetails.Title);
          strThumb = Utils.GetCoverArtName(Thumbs.MovieTitle, movieDetails.Title);
          Utils.FileDelete(strThumb);
          Utils.FileDelete(LargeThumb);

          string strExtension = System.IO.Path.GetExtension(strImage);
          if (strExtension.Length > 0)
          {
            string strTemp = "temp" + strExtension;

            Utils.FileDelete(strTemp);
            Utils.DownLoadImage(strImage, strTemp);

            if (System.IO.File.Exists(strTemp))
            {
              MediaPortal.Util.Picture.CreateThumbnail(strTemp, strThumb, 128, 128, 0);
              MediaPortal.Util.Picture.CreateThumbnail(strTemp, LargeThumb, 512, 512, 0);

              if (strPath.Length > 0)
              {
                try
                {
                  Utils.FileDelete(strPath + @"\folder.jpg");
                  System.IO.File.Copy(strThumb, strPath + @"\folder.jpg");
                }
                catch (Exception) { }
              }
            }

            Utils.FileDelete(strTemp);
          }//if ( strExtension.Length>0)
          else
          {
            Log.Write("image has no extension:{0}", strImage);
          }
        }
      }
    }

    private static void BackgroundImdbCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      Log.Write("Completed Imdb lookup");
    }

  }
}