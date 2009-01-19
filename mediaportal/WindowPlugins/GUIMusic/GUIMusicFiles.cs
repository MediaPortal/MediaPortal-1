#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.Freedb;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  [PluginIcons("WindowPlugins.GUIMusic.Music.gif", "WindowPlugins.GUIMusic.MusicDisabled.gif")]
  public class GUIMusicFiles : GUIMusicBaseWindow, ISetupForm, IShowPlugin
  {
    #region comparer

    private class TrackComparer : IComparer<GUIListItem>
    {
      public int Compare(GUIListItem item1, GUIListItem item2)
      {
        // Is this a top level artist folder?  If so, sort by path.
        if (item1.MusicTag == null || item2.MusicTag == null)
        {
          return item1.Path.CompareTo(item2.Path);
        }

          // Is it album folder or a song file. If album folder, sort by album name. Otherwise, sort by track number
        else
        {
          MusicTag tag1 = (MusicTag) item1.MusicTag;
          MusicTag tag2 = (MusicTag) item2.MusicTag;

          if (tag1.Track < 1)
          {
            return CompareAlbumNames(tag1.Album, tag2.Album);
          }

          else
          {
            return CompareTracks(tag1.Track, tag2.Track);
          }
        }
      }

      private int CompareTracks(int track1, int track2)
      {
        return track1.CompareTo(track2);
      }

      private int CompareAlbumNames(string albumTitle1, string albumTitle2)
      {
        if (albumTitle1 == null || albumTitle2 == null)
        {
          return 0;
        }

        return albumTitle1.CompareTo(albumTitle2);
      }
    }

    #endregion

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

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }
    }

    #region Base variables

    private static CDInfoDetail _freeDbCd = null;
    private MapSettings _mapSettings = new MapSettings();
    private DirectoryHistory _dirHistory = new DirectoryHistory();
    private GUIListItem _selectedListItem = null;
    private VirtualDirectory _virtualDirectory = new VirtualDirectory();

    private int _selectedAlbum = -1;
    private int _selectedItem = -1;
    private string _discId = string.Empty;
    private string currentFolder = string.Empty;
    private string _startDirectory = string.Empty;
    private string _destination = string.Empty;
    private string _fileMenuPinCode = string.Empty;
    private bool _useFileMenu = false;
    private bool m_bScan = false;
    private bool _shuffleOnLoad = true;

    private DateTime Previous_ACTION_PLAY_Time = DateTime.Now;
    private TimeSpan AntiRepeatInterval = new TimeSpan(0, 0, 0, 0, 500);
    private bool _switchRemovableDrives;
    #endregion

    public GUIMusicFiles()
    {
      GetID = (int) Window.WINDOW_MUSIC_FILES;

      _virtualDirectory.AddDrives();
      _virtualDirectory.SetExtensions(Util.Utils.AudioExtensions);

      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        MusicState.StartWindow = xmlreader.GetValueAsInt("music", "startWindow", GetID);
        MusicState.View = xmlreader.GetValueAsString("music", "startview", string.Empty);
      }

      GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
    }

    // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when 
    // the player is not playing)...
    private void GUIWindowManager_OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
          && GUIWindowManager.ActiveWindow == GetID)
      {
        GUIListItem item = facadeView.SelectedListItem;

        if (AntiRepeatActive() || item == null || item.Label == ".." || IsShare(item) || IsDVD(item.Path))
        {
          return;
        }

        OnPlayNow(item);
      }
    }

    public static CDInfoDetail MusicCD
    {
      get { return _freeDbCd; }
      set { _freeDbCd = value; }
    }

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _useFileMenu = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        _fileMenuPinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));
        _shuffleOnLoad = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", true);

        string strDefault = xmlreader.GetValueAsString("music", "default", string.Empty);
        _virtualDirectory.Clear();
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
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
          share.Name = xmlreader.GetValueAsString("music", strShareName, string.Empty);
          share.Path = xmlreader.GetValueAsString("music", strSharePath, string.Empty);
          string pinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("music", strPincode, string.Empty));
          if (pinCode != string.Empty)
          {
            share.Pincode = Convert.ToInt32(pinCode);
          }
          else
          {
            share.Pincode = -1;
          }

          share.IsFtpShare = xmlreader.GetValueAsBool("music", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("music", shareServer, string.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("music", shareLogin, string.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("music", sharePwd, string.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("music", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("music", remoteFolder, "/");
          share.DefaultView = (Share.Views) xmlreader.GetValueAsInt("music", shareViewPath, (int) Share.Views.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
              if (currentFolder.Length == 0)
              {
                if (share.IsFtpShare)
                {
                  //remote:hostname?port?login?password?folder
                  currentFolder = _virtualDirectory.GetShareRemoteURL(share);
                  _startDirectory = currentFolder;
                }
                else
                {
                  currentFolder = share.Path;
                  _startDirectory = share.Path;
                }
              }
            }
            _virtualDirectory.Add(share);
          }
          else
          {
            break;
          }
        }
        if (xmlreader.GetValueAsBool("music", "rememberlastfolder", false))
        {
          string lastFolder = xmlreader.GetValueAsString("music", "lastfolder", currentFolder);
          if (lastFolder != "root")
          {
            currentFolder = lastFolder;
          }
        }
        _switchRemovableDrives = xmlreader.GetValueAsBool("music", "SwitchRemovableDrives", true);
      }
    }

    #endregion

    #region overrides

    protected override string SerializeName
    {
      get { return "mymusic"; }
    }

    protected override bool AllowView(View view)
    {
      if (view == View.Albums)
      {
        return false;
      }
      return base.AllowView(view);
    }


    public override bool Init()
    {
      currentFolder = string.Empty;

      bool bResult = Load(GUIGraphicsContext.Skin + @"\mymusicsongs.xml");
      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = facadeView[0];

          if ((item != null) && item.IsFolder && (item.Label == ".."))
          {
            LoadDirectory(item.Path);
            return;
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];

        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          LoadDirectory(item.Path);
          return;
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        _virtualDirectory.Reset();
      }

      base.OnPageLoad();

      if (MusicState.StartWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow((int) Window.WINDOW_MUSIC_GENRE);
        return;
      }

      LoadFolderSettings(currentFolder);
      LoadDirectory(currentFolder);

      using (Profile.Settings settings = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        playlistPlayer.RepeatPlaylist = settings.GetValueAsBool("musicfiles", "repeat", true);
      }

      if (btnSearch != null)
      {
        btnSearch.Disabled = true;
      }

      if (btnSortBy != null)
      {
        if (!_showSortButton)
        {
          btnSortBy.Visible = false;
          btnSortBy.FreeResources();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _selectedItem = facadeView.SelectedListItemIndex;

      SaveFolderSettings(currentFolder);

      base.OnPageDestroy(newWindowId);
    }

    protected override void LoadDirectory(string strNewDirectory)
    {
      GUIWaitCursor.Show();

      try
      {
        GUIListItem SelectedItem = facadeView.SelectedListItem;
        if (SelectedItem != null)
        {
          if (SelectedItem.IsFolder && SelectedItem.Label != "..")
          {
            _dirHistory.Set(SelectedItem.Label, currentFolder);
          }
        }
        if (strNewDirectory != currentFolder && _mapSettings != null)
        {
          SaveFolderSettings(currentFolder);
        }
        if (strNewDirectory != currentFolder || _mapSettings == null)
        {
          LoadFolderSettings(strNewDirectory);
        }

        currentFolder = strNewDirectory;
        GUIControl.ClearControl(GetID, facadeView.GetID);

        List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(currentFolder);

        string strSelectedItem = _dirHistory.Get(currentFolder);
        int iItem = 0;
        OnRetrieveMusicInfo(ref itemlist, false);
        foreach (GUIListItem item in itemlist)
        {
          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeView.Add(item);
        }
        OnSort();
        bool itemSelected = false;
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          if (item.Label == strSelectedItem)
          {
            GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
            itemSelected = true;
            break;
          }
          iItem++;
        }
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          if (item.Path.Equals(_currentPlaying, StringComparison.OrdinalIgnoreCase))
          {
            item.Selected = true;
            break;
          }
        }

        //set selected item
        if (_selectedItem >= 0 && !itemSelected)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, _selectedItem);
        }

        GUIWaitCursor.Hide();
      }
      catch (Exception ex)
      {
        GUIWaitCursor.Hide();
        Log.Error("GUIMusicFiles: An error occured while loading the directory {0}", ex.Message);
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnPlayCd)
      {
        for (char c = 'C'; c <= 'Z'; c++)
        {
          if ((Util.Utils.GetDriveType(c + ":") & 5) == 5)
          {
            // Only try to play a CD if we got a valid Serial Number, which means a CD is inserted.
            if (Util.Utils.GetDriveSerial(c + ":") != string.Empty)
            {
              OnPlayCD(c + ":", false);
              break;
            }
          }
        }
      }

      base.OnClicked(controlId, control, actionType);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD:
          OnPlayCD(message.Label, false);
          break;

        case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
          MusicCD = null;
          if (g_Player.Playing && Util.Utils.IsCDDA(g_Player.CurrentFile) &&
              message.Label.Equals(g_Player.CurrentFile.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase))
            // test if it is our drive
          {
            g_Player.Stop();
          }
          if (GUIWindowManager.ActiveWindow == GetID)
          {
            if (Util.Utils.IsDVD(currentFolder))
            {
              currentFolder = string.Empty;
              LoadDirectory(currentFolder);
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          OnPlayCD(message.Label, false);
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

        case GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE:
          if (_switchRemovableDrives)
          {
            currentFolder = message.Label;
            if (!Util.Utils.IsRemovable(message.Label))
            {
              _virtualDirectory.AddRemovableDrive(message.Label, message.Label2);
            }
          }
          LoadDirectory(currentFolder);
          break;
        case GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE:
          if (!Util.Utils.IsRemovable(message.Label))
          {
            _virtualDirectory.Remove(message.Label);
          }
          if (currentFolder.Contains(message.Label))
          {
            currentFolder = string.Empty;
          }
          LoadDirectory(currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          if (currentFolder == string.Empty || currentFolder.Substring(0, 2) == message.Label)
          {
            currentFolder = string.Empty;
            LoadDirectory(currentFolder);
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;
      _selectedListItem = item;
      int itemNo = facadeView.SelectedListItemIndex;
      if (item == null)
      {
        return;
      }

      bool isCD = IsCD(item.Path);
      bool isDVD = IsDVD(item.Path);
      bool isUpFolder = false;

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (!facadeView.Focus)
      {
        // control view has no focus
        dlg.AddLocalizedString(368); //IMDB
      }
      else
      {
        if (item.Label == "..")
        {
          isUpFolder = true;
        }
        if ((Path.GetFileName(item.Path) != string.Empty) || isCD && !isDVD)
        {
          if (!isUpFolder)
          {
            dlg.AddLocalizedString(926); // Add to playlist     
            dlg.AddLocalizedString(4557); // Add all to playlist
            dlg.AddLocalizedString(4552); // Play now
            if (!item.IsFolder)
            {
              dlg.AddLocalizedString(4551); // Play next
            }
            if (isCD)
            {
              dlg.AddLocalizedString(890); // Play CD
            }
            if (!item.IsFolder && !item.IsRemote)
            {
              dlg.AddLocalizedString(930); //Add to favorites
              dlg.AddLocalizedString(931); //Rating
            }
            dlg.AddLocalizedString(4521); //Show Album Info
            dlg.AddLocalizedString(928); //find coverart               

            if (!item.IsFolder && Util.Utils.getDriveType(item.Path.Substring(0, 2)) == 5)
            {
              dlg.AddLocalizedString(1100); //Import CD              
              dlg.AddLocalizedString(1101); //Import Track
              if (MusicImport.MusicImport.Ripping)
              {
                dlg.AddLocalizedString(1102); //Cancel Import
              }
            }

            if (!_virtualDirectory.IsRemote(currentFolder))
            {
              dlg.AddLocalizedString(102); //Scan
            }
          }
          else // ".."
          {
            dlg.AddLocalizedString(4557); // Add all to playlist
            dlg.AddLocalizedString(102); //Scan
          }
        }

        if (Util.Utils.getDriveType(item.Path) == 5)
        {
          dlg.AddLocalizedString(654); //Eject
        }

        int iPincodeCorrect;
        if (!_virtualDirectory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && _useFileMenu)
        {
          dlg.AddLocalizedString(500); // FileMenu
        }
      }

      if (g_Player.Playing && g_Player.IsMusic)
      {
        string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
        if (artist.Length > 0)
        {
          dlg.AddLocalizedString(751); // Show all songs from current artist
        }
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
        case 928: // find coverart
          OnFindCoverArt(itemNo);
          break;

        case 4521: // Show album info
          OnInfo(itemNo);
          break;

        case 926: // add to playlist
          OnQueueItem(itemNo);
          break;

        case 4557: // add all items in current list to end of playlist
          OnQueueAllItems();
          break;

        case 4551: // Play next
          OnPlayNext(item);
          break;

        case 4552: // Play now
          OnPlayNow(item);
          break;

        case 890:
          OnPlayCD(item.Path, false);
          break;

        case 136: // show playlist
          _selectedItem = facadeView.SelectedListItemIndex;
          SaveFolderSettings(currentFolder);
          GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
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

        case 930: // add to favorites
          AddSongToFavorites(item);
          break;

        case 931: // Rating
          OnSetRating(facadeView.SelectedListItemIndex);
          break;

        case 102:
          OnScan();
          break;

        case 500: // File menu
          {
            // get pincode
            if (_fileMenuPinCode != string.Empty)
            {
              string strUserCode = string.Empty;
              if (GetUserInputString(ref strUserCode) && strUserCode == _fileMenuPinCode)
              {
                OnShowFileMenu();
              }
            }
            else
            {
              OnShowFileMenu();
            }
          }
          break;

        case 1100: // Import CD
          // Stop playback before importing
          if (g_Player.Playing)
          {
            g_Player.Stop();
          }

          OnAction(new Action(Action.ActionType.ACTION_IMPORT_DISC, 0, 0));
          break;

        case 1101: // Import seltected track
          // Stop playback before importing
          if (g_Player.Playing)
          {
            g_Player.Stop();
          }

          OnAction(new Action(Action.ActionType.ACTION_IMPORT_TRACK, 0, 0));
          break;

        case 1102: // Cancel CD import
          OnAction(new Action(Action.ActionType.ACTION_CANCEL_IMPORT, 0, 0));
          break;

        case 751: // Show all songs from this artist
          {
            string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
            int viewNr = -1;
            for (int x = 0; x < handler.Views.Count; ++x)
            {
              ViewDefinition view = (ViewDefinition) handler.Views[x];
              if (view.Name.ToLower().IndexOf("artist") >= 0)
              {
                viewNr = x;
              }
            }
            if (viewNr < 0)
            {
              return;
            }
            ViewDefinition selectedView = (ViewDefinition) handler.Views[viewNr];
            handler.CurrentView = selectedView.Name;
            MusicState.View = selectedView.Name;
            GUIMusicGenres.SelectArtist(artist);
            int nNewWindow = (int) Window.WINDOW_MUSIC_GENRE;
            if (GetID != nNewWindow)
            {
              MusicState.StartWindow = nNewWindow;
              if (nNewWindow != GetID)
              {
                GUIWindowManager.ReplaceWindow(nNewWindow);
              }
            }
            else
            {
              LoadDirectory(string.Empty);
              if (facadeView.Count <= 0)
              {
                GUIControl.FocusControl(GetID, btnViewAs.GetID);
              }
            }
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
      }
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;

      if (item == null)
      {
        return;
      }

      if (item.IsFolder)
      {
        _selectedItem = -1;

        LoadDirectory(item.Path);
      }
      else
      {
        if (_virtualDirectory.IsRemote(item.Path))
        {
          if (!_virtualDirectory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!_virtualDirectory.ShouldWeDownloadFile(item.Path))
            {
              return;
            }
            if (!_virtualDirectory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
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
          }
          return;
        }

        if (PlayListFactory.IsPlayList(item.Path))
        {
          LoadPlayList(item.Path);
          return;
        }
        //play and add current directory to temporary playlist
        int nFolderCount = 0;
        int nRemoteCount = 0;
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
        playlistPlayer.Reset();
        List<GUIListItem> queueItems = new List<GUIListItem>();
        for (int i = 0; i < (int) facadeView.Count; i++)
        {
          GUIListItem pItem = facadeView[i];
          if (pItem.IsFolder)
          {
            nFolderCount++;
            continue;
          }
          if (pItem.IsRemote)
          {
            nRemoteCount++;
            continue;
          }
          if (!PlayListFactory.IsPlayList(pItem.Path))
          {
            queueItems.Add(pItem);
          }
          else
          {
            if (i < facadeView.SelectedListItemIndex)
            {
              nFolderCount++;
            }
            continue;
          }
        }
        m_bScan = true;
        OnRetrieveMusicInfo(ref queueItems, false);
        // m_database.CheckVariousArtistsAndCoverArt();
        m_bScan = false;

        foreach (GUIListItem queueItem in queueItems)
        {
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.Type = PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName = queueItem.Path;
          playlistItem.Description = queueItem.Label;
          playlistItem.Duration = queueItem.Duration;
          playlistItem.MusicTag = queueItem.MusicTag;
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
        }

        //	Save current window and directory to know where the selected item was
        MusicState.TempPlaylistWindow = GetID;
        MusicState.TempPlaylistDirectory = currentFolder;

        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_TEMP;
        playlistPlayer.Play(item.Path);
      }
    }

    protected override void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem = facadeView[iItem];
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsRemote)
      {
        return;
      }
      if (PlayListFactory.IsPlayList(pItem.Path))
      {
        LoadPlayList(pItem.Path);
        return;
      }
      AddItemToPlayList(pItem);

      //move to next item
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem + 1);
      if (!g_Player.Playing)
      {
        if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
        {
          playlistPlayer.Reset();
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
          playlistPlayer.Play(0);
        }
      }
    }

    private bool GetUserInputString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
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

    private void OnShowFileMenu()
    {
      GUIListItem item = _selectedListItem;
      if (item == null)
      {
        return;
      }
      if (item.IsFolder && item.Label == "..")
      {
        return;
      }

      // init
      GUIDialogFile dlgFile = (GUIDialogFile) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_FILE);
      if (dlgFile == null)
      {
        return;
      }

      // File operation settings
      dlgFile.SetSourceItem(item);
      dlgFile.SetSourceDir(currentFolder);
      dlgFile.SetDestinationDir(_destination);
      dlgFile.SetDirectoryStructure(_virtualDirectory);
      dlgFile.DoModal(GetID);
      _destination = dlgFile.GetDestinationDir();

      //final		
      if (dlgFile.Reload())
      {
        LoadDirectory(currentFolder);
        if (_selectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, _selectedItem);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
    }

    protected override void OnRetrieveCoverArt(GUIListItem item)
    {
      Util.Utils.SetDefaultIcons(item);
      if (item.Label == "..")
      {
        return;
      }
      int pin;
      if (item.IsFolder && (_virtualDirectory.IsProtectedShare(item.Path, out pin)))
      {
        return;
      }
      base.OnRetrieveCoverArt(item);
    }

    protected override void OnFindCoverArt(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];

      if (pItem.IsFolder && pItem.Label != "..")
      {
        GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);

        if (null != pDlgOK && !Win32API.IsConnectedToInternet())
        {
          pDlgOK.SetHeading(703);
          pDlgOK.SetLine(1, 703);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);

          //throw new Exception("no internet");
          return;
        }

        else if (!Win32API.IsConnectedToInternet())
        {
          //throw new Exception("no internet");
          return;
        }

        string oldFolder = currentFolder;
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.AudioExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(pItem.Path, true);

        if (items.Count < 2)
        {
          return;
        }

        m_bScan = true;
        currentFolder = pItem.Path;
        OnRetrieveMusicInfo(ref items, false);
        currentFolder = oldFolder;
        m_bScan = false;
        GUIListItem item = items[1] as GUIListItem;
        MusicTag tag = item.MusicTag as MusicTag;

        // Is this an album?
        if (tag != null && tag.Album.Length > 0)
        {
          FindCoverArt(true, tag.Artist, tag.Album, pItem.Path, tag, -1);
        }

          // Nope, it's a artist folder or share
        else
        {
          int windowID = (int) Window.WINDOW_MUSIC_COVERART_GRABBER_PROGRESS;
          GUICoverArtGrabberProgress guiCoverArtProgress =
            (GUICoverArtGrabberProgress) GUIWindowManager.GetWindow(windowID);

          if (guiCoverArtProgress != null)
          {
            guiCoverArtProgress.CoverArtSelected +=
              new GUICoverArtGrabberProgress.CoverArtSelectedHandler(OnCoverArtGrabberCoverArtSelected);
            guiCoverArtProgress.CoverArtGrabDone +=
              new GUICoverArtGrabberProgress.CoverArtGrabDoneHandler(OnCoverArtGrabberDone);
            guiCoverArtProgress.TopLevelFolderName = pItem.Path;
            guiCoverArtProgress.UseID3 = UseID3;
            guiCoverArtProgress.Show(GetID);
          }
        }
      }

      base.OnFindCoverArt(iItem);
    }

    protected override void OnInfo(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];

      if (pItem.IsFolder && pItem.Label != "..")
      {
        string oldFolder = currentFolder;
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.AudioExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(pItem.Path, true);

        if (items.Count < 2)
        {
          return;
        }

        m_bScan = true;
        currentFolder = pItem.Path;
        OnRetrieveMusicInfo(ref items, false);
        currentFolder = oldFolder;
        m_bScan = false;
        GUIListItem item = items[1] as GUIListItem;
        MusicTag tag = item.MusicTag as MusicTag;

        // Is this an album?
        if (tag != null && tag.Album.Length > 0)
        {
          ShowAlbumInfo(true, tag.Artist, tag.Album, pItem.Path, tag);
          facadeView.RefreshCoverArt();
        }

          // Nope, it's a artist folder or share
        else
        {
          return;
        }
      }

      Song song = pItem.AlbumInfoTag as Song;

      if (song == null)
      {
        List<GUIListItem> list = new List<GUIListItem>();
        list.Add(pItem);
        m_bScan = true;
        OnRetrieveMusicInfo(ref list, false);
        m_bScan = false;
      }

      facadeView.RefreshCoverArt();
      base.OnInfo(iItem);
    }

    protected override void AddSongToFavorites(GUIListItem item)
    {
      Song song = item.AlbumInfoTag as Song;
      if (song == null)
      {
        List<GUIListItem> list = new List<GUIListItem>();
        list.Add(item);
        m_bScan = true;
        OnRetrieveMusicInfo(ref list, false);
        m_bScan = false;
      }
      base.AddSongToFavorites(item);
    }

    #endregion

    public void PlayCD()
    {
      for (char c = 'C'; c <= 'Z'; c++)
      {
        if ((Util.Utils.GetDriveType(c + ":") & 5) == 5)
        {
          OnPlayCD(c + ":", false);
          break;
        }
      }
    }

    private void DisplayFilesList(int searchKind, string strSearchText)
    {
      GUIControl.ClearControl(GetID, facadeView.GetID);
      List<GUIListItem> itemlist = new List<GUIListItem>();
      m_database.GetSongs(searchKind, strSearchText, ref itemlist);
      // this will set all to move up
      // from a search result
      _dirHistory.Set(currentFolder, currentFolder); //save where we are
      GUIListItem dirUp = new GUIListItem("..");
      dirUp.Path = currentFolder; // to get where we are
      dirUp.IsFolder = true;
      dirUp.ThumbnailImage = string.Empty;
      dirUp.IconImage = "defaultFolderBack.png";
      dirUp.IconImageBig = "defaultFolderBackBig.png";
      itemlist.Insert(0, dirUp);

      OnRetrieveMusicInfo(ref itemlist, false);

      foreach (GUIListItem item in itemlist)
      {
        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        GUIControl.AddListItemControl(GetID, facadeView.GetID, item);
      }
      OnSort();
      int iTotalItems = itemlist.Count;
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = itemlist[0];
        if (rootItem.Label == "..")
        {
          iTotalItems--;
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));
    }

    private void LoadFolderSettings(string folderName)
    {
      if (folderName == string.Empty)
      {
        folderName = "root";
      }
      object o;
      FolderSettings.GetFolderSetting(folderName, "MusicFiles", typeof (MapSettings), out o);
      if (o != null)
      {
        _mapSettings = o as MapSettings;
        if (_mapSettings == null)
        {
          _mapSettings = new MapSettings();
        }
        CurrentSortAsc = _mapSettings.SortAscending;
        CurrentSortMethod = (MusicSort.SortMethod) _mapSettings.SortBy;
        currentView = (View) _mapSettings.ViewAs;
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
          CurrentSortMethod = (MusicSort.SortMethod) _mapSettings.SortBy;
          currentView = (View) share.DefaultView;
        }
      }
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (xmlreader.GetValueAsBool("music", "rememberlastfolder", false))
        {
          xmlreader.SetValue("music", "lastfolder", folderName);
        }
      }

      SwitchView();
      UpdateButtonStates();
    }

    private void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory == string.Empty)
      {
        strDirectory = "root";
      }
      _mapSettings.SortAscending = CurrentSortAsc;
      _mapSettings.SortBy = (int) CurrentSortMethod;
      _mapSettings.ViewAs = (int) currentView;
      FolderSettings.AddFolderSetting(strDirectory, "MusicFiles", typeof (MapSettings), _mapSettings);
    }

    private void AddItemToPlayList(GUIListItem pItem)
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      AddItemToPlayList(pItem, ref playList);
    }

    private void AddItemToPlayList(GUIListItem pItem, ref PlayList playList)
    {
      if (playList == null || pItem == null)
      {
        return;
      }

      if (m_database == null)
      {
        m_database = MusicDatabase.Instance;
      }

      if (pItem.IsFolder)
      {
        // recursive
        if (pItem.Label == "..")
        {
          return;
        }
        string strDirectory = currentFolder;
        currentFolder = pItem.Path;

        List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(currentFolder);
        OnRetrieveMusicInfo(ref itemlist, false);

        // Sort share folder tracks.  
        try
        {
          itemlist.Sort(new TrackComparer());
        }

        catch (Exception ex)
        {
          Log.Error("GUIMusicFiles.AddItemToPlayList at itemlist.Sort: {0}", ex.Message);
        }

        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item, ref playList);
        }
        currentFolder = strDirectory;
      }
      else
      {
        //TODO
        if (Util.Utils.IsAudio(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
        {
          List<GUIListItem> list = new List<GUIListItem>();
          list.Add(pItem);
          m_bScan = true;
          OnRetrieveMusicInfo(ref list, true);
          m_bScan = false;

          PlayListItem playlistItem = new PlayListItem();
          playlistItem.Type = PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName = pItem.Path;
          playlistItem.Description = pItem.Label;
          playlistItem.Duration = pItem.Duration;
          playlistItem.MusicTag = pItem.MusicTag;
          playList.Add(playlistItem);
        }
      }
    }

    private void keyboard_TextChanged(int kindOfSearch, string data)
    {
      DisplayFilesList(kindOfSearch, data);
    }

    private void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    public static string GetCoverArt(bool isfolder, string filename, MusicTag tag)
    {
      if (isfolder)
      {
        string strFolderThumb = string.Empty;
        strFolderThumb = Util.Utils.GetLocalFolderThumbForDir(filename);

        if (File.Exists(strFolderThumb))
        {
          return strFolderThumb;
        }
        else
        {
          if (_createMissingFolderThumbCache)
          {
            FolderThumbCacher thumbworker = new FolderThumbCacher(filename, false);
          }
        }
        return string.Empty;
      }

      string strAlbumName = string.Empty;
      string strArtistName = string.Empty;
      if (tag != null)
      {
        if (tag.Album.Length > 0)
        {
          strAlbumName = tag.Album;
        }
        if (tag.Artist.Length > 0)
        {
          strArtistName = tag.Artist;
        }
      }
      if (!isfolder && strAlbumName.Length == 0 && strArtistName.Length == 0)
      {
        FileInfo fI = new FileInfo(filename);
        string dir = fI.Directory.FullName;

        if (dir.Length > 0)
        {
          string strFolderThumb = string.Empty;
          strFolderThumb = Util.Utils.GetLocalFolderThumbForDir(dir);

          if (File.Exists(strFolderThumb))
          {
            return strFolderThumb;
          }
          else
          {
            if (_createMissingFolderThumbCache)
            {
              FolderThumbCacher thumbworker = new FolderThumbCacher(dir, false);
            }
          }
        }
        return string.Empty;
      }

      // use covert art thumbnail for albums
      string strThumb = Util.Utils.GetAlbumThumbName(strArtistName, strAlbumName);
      if (File.Exists(strThumb))
      {
        if (_createMissingFolderThumbs && _createMissingFolderThumbCache)
        {
          string folderThumb = Util.Utils.GetFolderThumb(filename);
          if (!File.Exists(folderThumb))
          {
            FolderThumbCreator thumbCreator = new FolderThumbCreator(filename, tag);
          }
        }
        return strThumb;
      }

      return string.Empty;
    }

    private int GetCDATrackNumber(string strFile)
    {
      string strTrack = string.Empty;
      int pos = strFile.IndexOf(".cda");
      if (pos >= 0)
      {
        pos--;
        while (Char.IsDigit(strFile[pos]) && pos > 0)
        {
          strTrack = strFile[pos] + strTrack;
          pos--;
        }
      }

      try
      {
        int iTrack = Convert.ToInt32(strTrack);
        return iTrack;
      }
      catch (Exception)
      {
      }
      return 1;
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
      {
        return;
      }

      if (item.Label == "..")
      {
        filmstrip.InfoImageFileName = string.Empty;
        return;
      }
      else
      {
        filmstrip.InfoImageFileName = item.ThumbnailImage;
      }
    }

    public static bool IsMusicWindow(int window)
    {
      if (window == (int) Window.WINDOW_MUSIC)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_PLAYLIST)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_FILES)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_ALBUM)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_ARTIST)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_GENRE)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_TOP100)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_FAVORITES)
      {
        return true;
      }
      if (window == (int) Window.WINDOW_MUSIC_PLAYING_NOW)
      {
        return true;
      }
      return false;
    }

    #region Handlers

    private void OnPlayCD(string strDriveLetter, bool AskForAlbum)
    {
      // start playing current CD        
      PlayList list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP);
      list.Clear();

      list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      list.Clear();
      GUIListItem pItem = new GUIListItem();
      pItem.Path = strDriveLetter;
      pItem.IsFolder = true;
      m_bScan = AskForAlbum;
      AddItemToPlayList(pItem);
      m_bScan = false;
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // waeberd: mantis #470
        if (g_Player.Playing)
        {
          // SV: this causes a problem! Once the player is stopped music file playback cannot be restarted
          // without exiting and re-entering My Music.  Suspect the issue has to do with _player.Release()
          // and _player = null;
          // g_Player.Stop();
        }
        playlistPlayer.Reset();
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Play(0);
      }
    }

    private void OnRetrieveMusicInfo(ref List<GUIListItem> items, bool aPlaylistAdd)
    {
      int nFolderCount = 0;

      if (m_database == null)
      {
        m_database = MusicDatabase.Instance;
      }

      for (int i = 0; i < items.Count; i++)
      {
        if (items[i].IsFolder)
        {
          nFolderCount++;
        }
      }

      // Skip items with folders only
      if (nFolderCount == (int) items.Count)
      {
        return;
      }

      GUIDialogProgress dlg = (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if (!aPlaylistAdd)
      {
        dlg = null;
      }

      if (currentFolder.Length == 0)
      {
        return;
      }

      List<SongMap> songsMap = new List<SongMap>();

      if (aPlaylistAdd)
      {
        Song AddSong = new Song();
        SongMap AddMap = new SongMap();
        if (m_database.GetSongByFileName(items[0].Path, ref AddSong))
        {
          AddMap.m_song = AddSong;
          AddMap.m_strPath = items[0].Path;
          songsMap.Add(AddMap);
        }
      }
      else
      {
        // get all information for all files in current directory from database 
        m_database.GetSongsByPath(currentFolder, ref songsMap);
      }
      //musicCD is the information about the cd...
      //delete old CD info
      //GUIMusicFiles.MusicCD = null;

      bool bCDDAFailed = false;
      // for every file found, but skip folder
      for (int i = 0; i < (int) items.Count; ++i)
      {
        GUIListItem pItem = items[i];
        if (pItem.IsRemote)
        {
          continue;
        }
        if (pItem.IsFolder)
        {
          continue;
        }
        if (pItem.Path.Length == 0)
        {
          continue;
        }

        string strFilePath = Path.GetFullPath(pItem.Path);
        strFilePath = strFilePath.Substring(0, strFilePath.Length - (1 + Path.GetFileName(pItem.Path).Length));
        //strFilePath = System.IO.Path.GetDirectoryName(strFilePath);

        if (strFilePath != currentFolder)
        {
          return;
        }

        string strExtension = Path.GetExtension(pItem.Path);
        if (m_bScan && strExtension.ToLower().Equals(".cda"))
        {
          continue;
        }

        if (m_bScan && dlg != null)
        {
          dlg.SetPercentage((int) ((double) i/(double) items.Count*100.00));
          dlg.Progress();
          dlg.ShowProgressBar(true);
        }

        // dont try reading id3tags for folders or playlists
        if (!pItem.IsFolder && !PlayListFactory.IsPlayList(pItem.Path))
        {
          // is tag for this file already loaded?
          bool bNewFile = false;
          MusicTag tag = (MusicTag) pItem.MusicTag;
          if (tag == null)
          {
            // no, then we gonna load it. But dont load tags from cdda files
            if (strExtension != ".cda") // int_20h: changed cdda to cda.
            {
              // first search for file in our list of the current directory
              Song song = new Song();
              bool bFound = false;
              foreach (SongMap song1 in songsMap)
              {
                if (song1.m_strPath == pItem.Path)
                {
                  song = song1.m_song;
                  bFound = true;
                  tag = new MusicTag();
                  pItem.MusicTag = tag;
                  break;
                }
              }

              if (!bFound && !m_bScan)
              {
                // try finding it in the database
                string strPathName;
                string strFileName;
                DatabaseUtility.Split(pItem.Path, out strPathName, out strFileName);
                if (strPathName != currentFolder)
                {
                  if (m_database.GetSongByFileName(pItem.Path, ref song))
                  {
                    bFound = true;
                  }
                }
              }

              if (!bFound)
              {
                // if id3 tag scanning is turned on AND we're scanning the directory
                // then parse id3tag from file
                if (UseID3 && m_bScan)
                {
                  // get correct tag parser
                  tag = TagReader.TagReader.ReadTag(pItem.Path);
                  if (tag != null)
                  {
                    pItem.MusicTag = tag;
                    bNewFile = true;
                  }
                }
              }
              else // of if ( !bFound )
              {
                tag.Album = song.Album;
                tag.Artist = song.Artist;
                tag.Genre = song.Genre;
                tag.Duration = song.Duration;
                tag.Title = song.Title;
                tag.Track = song.Track;
                tag.Rating = song.Rating;
                tag.Year = song.Year;
              }
            } //if (strExtension!=".cda" )
            else // int_20h: if it is .cda then get info from freedb
            {
              if (m_bScan || bCDDAFailed || !Win32API.IsConnectedToInternet() || aPlaylistAdd)
              {
                continue;
              }

              try
              {
                // check internet connectivity
                GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                if (null != pDlgOK && !Win32API.IsConnectedToInternet())
                {
                  pDlgOK.SetHeading(703);
                  //pDlgOK.SetLine(0, string.Empty);
                  pDlgOK.SetLine(1, 703);
                  pDlgOK.SetLine(2, string.Empty);
                  pDlgOK.DoModal(GetID);

                  //throw new Exception("no internet");
                  return;
                }
                else if (!Win32API.IsConnectedToInternet())
                {
                  //throw new Exception("no internet");
                  return;
                }

                FreeDBHttpImpl freedb = new FreeDBHttpImpl();
                char driveLetter = Path.GetFullPath(pItem.Path).ToCharArray()[0];
                // try finding it in the database
                string strPathName, strCDROMPath;
                //int_20h fake the path with the cdInfo
                strPathName = driveLetter + ":/" + freedb.GetCDDBDiscIDInfo(driveLetter, '+');
                strCDROMPath = strPathName + "+" + Path.GetFileName(pItem.Path);

                Song song = new Song();
                bool bFound = false;
                if (m_database.GetSongByFileName(strCDROMPath, ref song))
                {
                  bFound = true;
                }

                // Disk changed (or other drive)
                if (MusicCD != null)
                {
                  if (freedb.GetCDDBDiscID(driveLetter).ToLower() != MusicCD.DiscID)
                  {
                    MusicCD = null;
                  }
                }

                if (!bFound && MusicCD == null)
                {
                  try
                  {
                    freedb.Connect(); // should be replaced with the Connect that receives a http freedb site...
                    CDInfo[] cds = freedb.GetDiscInfo(driveLetter);
                    if (cds != null)
                    {
                      if (cds.Length == 1)
                      {
                        MusicCD = freedb.GetDiscDetails(cds[0].Category, cds[0].DiscId);
                        _discId = cds[0].DiscId;
                      }
                      else if (cds.Length > 1)
                      {
                        if (_discId == cds[0].DiscId)
                        {
                          MusicCD = freedb.GetDiscDetails(cds[_selectedAlbum].Category, cds[_selectedAlbum].DiscId);
                        }
                        else
                        {
                          _discId = cds[0].DiscId;
                          //show dialog with all albums found
                          string szText = GUILocalizeStrings.Get(181);
                          GUIDialogSelect pDlg =
                            (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
                          if (null != pDlg)
                          {
                            pDlg.Reset();
                            pDlg.SetHeading(szText);
                            for (int j = 0; j < cds.Length; j++)
                            {
                              CDInfo info = cds[j];
                              pDlg.Add(info.Title);
                            }
                            pDlg.DoModal(GetID);

                            // and wait till user selects one
                            _selectedAlbum = pDlg.SelectedLabel;
                            if (_selectedAlbum < 0)
                            {
                              return;
                            }
                            MusicCD = freedb.GetDiscDetails(cds[_selectedAlbum].Category, cds[_selectedAlbum].DiscId);
                          }
                        }
                      }
                    }
                    freedb.Disconnect();
                    if (MusicCD == null)
                    {
                      bCDDAFailed = true;
                    }
                  }
                  catch (Exception)
                  {
                    MusicCD = null;
                    bCDDAFailed = true;
                  }
                }

                if (!bFound && MusicCD != null) // if musicCD was configured correctly...
                {
                  int trackno = GetCDATrackNumber(pItem.Path);
                  CDTrackDetail track = MusicCD.getTrack(trackno);

                  tag = new MusicTag();
                  tag.Album = MusicCD.Title;
                  tag.Genre = MusicCD.Genre;
                  if (track == null)
                  {
                    // prob hidden track									
                    tag.Artist = MusicCD.Artist;
                    tag.Duration = -1;
                    tag.Title = string.Empty;
                    tag.Track = -1;
                    pItem.Label = pItem.Path;
                  }
                  else
                  {
                    tag.Artist = track.Artist == null ? MusicCD.Artist : track.Artist;
                    tag.Duration = track.Duration;
                    tag.Title = track.Title;
                    tag.Track = track.TrackNumber;
                    pItem.Label = track.Title;
                  }
                  bNewFile = true;
                  pItem.MusicTag = tag;
                  //pItem.Path = strCDROMPath; // to be stored in the database
                }
                else if (bFound)
                {
                  tag = new MusicTag();
                  tag = song.ToMusicTag();
                  pItem.MusicTag = tag;
                  pItem.Label = song.Title;
                  //pItem.Path = strCDROMPath;
                }
              } // end of try
              catch (Exception e)
              {
                // log the problem...
                Log.Error("GUIMusicFiles: OnRetrieveMusicInfo: {0}", e.ToString());
              }
            }
          } //if (!tag.Loaded() )
          else if (m_bScan)
          {
            bNewFile = true;
            foreach (SongMap song1 in songsMap)
            {
              if (song1.m_strPath == pItem.Path)
              {
                bNewFile = false;
                break;
              }
            }
          }

          for (int j = 0; j < songsMap.Count; j++)
          {
            if (songsMap[j].m_song.FileName == pItem.Path)
            {
              pItem.AlbumInfoTag = songsMap[j].m_song;
              break;
            }
          }

          if (tag != null && m_bScan && bNewFile)
          {
            Song song = new Song();
            song.Title = tag.Title;
            song.Genre = tag.Genre;
            song.FileName = pItem.Path;
            song.Artist = tag.Artist;
            song.Album = tag.Album;
            song.Year = tag.Year;
            song.Track = tag.Track;
            song.Duration = tag.Duration;
            pItem.AlbumInfoTag = song;

            m_database.AddSong(pItem.Path);
          }
        } //if (!pItem.IsFolder)
      }
    }

    private bool DoScan(string strDir, ref List<GUIListItem> items)
    {
      GUIDialogProgress dlg = (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      if (dlg != null)
      {
        string strPath = Path.GetFileName(strDir);
        dlg.SetLine(2, strPath);
        dlg.Progress();
      }

      OnRetrieveMusicInfo(ref items, false);
      // m_database.CheckVariousArtistsAndCoverArt();

      if (dlg != null && dlg.IsCanceled)
      {
        return false;
      }

      bool bCancel = false;
      for (int i = 0; i < (int) items.Count; ++i)
      {
        GUIListItem pItem = items[i];
        if (pItem.IsRemote)
        {
          continue;
        }
        if (dlg != null && dlg.IsCanceled)
        {
          bCancel = true;
          break;
        }
        if (pItem.IsFolder)
        {
          if (pItem.Label != "..")
          {
            // load subfolder
            string strPrevDir = currentFolder;
            currentFolder = pItem.Path;
            List<GUIListItem> subDirItems = _virtualDirectory.GetDirectoryExt(currentFolder);
            if (!DoScan(currentFolder, ref subDirItems))
            {
              bCancel = true;
            }
            currentFolder = strPrevDir;
            if (bCancel)
            {
              break;
            }
          }
        }
      }

      return !bCancel;
    }

    private void OnScan()
    {
      GUIDialogProgress dlg = (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      GUIGraphicsContext.Overlay = false;

      m_bScan = true;
      List<GUIListItem> items = new List<GUIListItem>();
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem pItem = facadeView[i];
        if (!pItem.IsRemote)
        {
          items.Add(pItem);
        }
      }
      if (null != dlg)
      {
        string strPath = Path.GetFileName(currentFolder);
        dlg.SetHeading(189);
        dlg.SetLine(1, 330);
        //dlg.SetLine(1, string.Empty);
        dlg.SetLine(2, strPath);
        dlg.StartModal(GetID);
      }

      m_database.BeginTransaction();
      if (DoScan(currentFolder, ref items))
      {
        dlg.SetLine(1, 328);
        dlg.SetLine(2, string.Empty);
        dlg.SetLine(3, 330);
        dlg.Progress();
        m_database.CommitTransaction();
      }
      else
      {
        m_database.RollbackTransaction();
      }
      dlg.Close();
      // disable scan mode
      m_bScan = false;
      GUIGraphicsContext.Overlay = _isOverlayAllowed;

      LoadDirectory(currentFolder);
    }

    private void OnCoverArtGrabberCoverArtSelected(AlbumInfo albumInfo, string albumPath, bool bSaveToAlbumFolder,
                                                   bool bSaveToThumbsFolder)
    {
      SaveCoverArtImage(albumInfo, albumPath, bSaveToAlbumFolder, bSaveToThumbsFolder);
    }

    private void OnCoverArtGrabberDone(GUICoverArtGrabberProgress coverArtGrabberProgress)
    {
      facadeView.RefreshCoverArt();
    }

    protected void OnQueueItem(GUIListItem pItem)
    {
      if (pItem == null || pItem.IsRemote || PlayListFactory.IsPlayList(pItem.Path))
      {
        return;
      }

      AddItemToPlayList(pItem);
      this.facadeView.SelectedListItemIndex = facadeView.SelectedListItemIndex;

      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0 && !g_Player.Playing)
      {
        playlistPlayer.Reset();
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Play(0);
      }
    }

    private void OnQueueAllItems()
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      int index = Math.Max(playlistPlayer.CurrentSong, 0);

      for (int i = 0; i < facadeView.Count; i++)
      {
        GUIListItem item = facadeView[i];

        if (item == null)
        {
          continue;
        }

        if (item.Label != "...")
        {
          AddItemToPlayList(item);
        }
      }

      if (!g_Player.Playing)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Play(index);
      }
    }

    protected void OnPlayNext(GUIListItem pItem)
    {
      if (pItem == null || pItem.IsRemote || PlayListFactory.IsPlayList(pItem.Path))
      {
        return;
      }

      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      if (playList == null)
      {
        return;
      }

      int index = Math.Max(playlistPlayer.CurrentSong, 0);

      if (playList.Count == 1)
      {
        AddItemToPlayList(pItem, ref playList);
      }
      else if (playList.Count > 1)
      {
        List<GUIListItem> itemList = new List<GUIListItem>();
        itemList.Add(pItem);

        OnRetrieveMusicInfo(ref itemList, true);
        pItem = itemList[0];

        PlayListItem playlistItem = new PlayListItem();
        playlistItem.Type = PlayListItem.PlayListItemType.Audio;
        playlistItem.FileName = pItem.Path;
        playlistItem.Description = pItem.Label;
        playlistItem.Duration = pItem.Duration;
        playlistItem.MusicTag = pItem.MusicTag;

        playList.Insert(playlistItem, index);

        //PlayList tempPlayList = new PlayList();
        //for (int i = 0; i < playList.Count; i++)
        //{
        //  if (i == index + 1)
        //  {
        //    AddItemToPlayList(pItem, ref tempPlayList);
        //  }

        //  tempPlayList.Add(playList[i]);
        //}

        //playList.Clear();

        //// add each item of the playlist to the playlistplayer
        //for (int i = 0; i < tempPlayList.Count; ++i)
        //{
        //  playList.Add(tempPlayList[i]);
        //}
      }

      else
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        AddItemToPlayList(pItem);
      }

      if (!g_Player.Playing)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Play(index);
      }
    }

    protected void OnPlayNow(GUIListItem pItem)
    {
      if (pItem == null || pItem.IsRemote || PlayListFactory.IsPlayList(pItem.Path))
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;
      int playStartIndex = 0;
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      if (playList == null)
      {
        return;
      }

      playList.Clear();
      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

      // If this is an individual track find all of the tracks in the list and add them to 
      // the playlist.  Start playback at the currently selected track.
      if (!pItem.IsFolder && PlayAllOnSingleItemPlayNow)
      {
        for (int i = 0; i < facadeView.Count; i++)
        {
          GUIListItem item = facadeView[i];
          AddItemToPlayList(item, ref playList);
        }

        if (iItem < facadeView.Count)
        {
          if (facadeView.Count > 0)
          {
            playStartIndex = iItem;

            if (facadeView[0].Label == "..")
            {
              playStartIndex--;
            }
          }
        }
      }

      else
      {
        AddItemToPlayList(pItem, ref playList);
      }

      // We might have added a lot of new songs to the DB, so the Various Artist count needs to be updated.
      // m_database.CheckVariousArtistsAndCoverArt();

      if (playList.Count > 0)
      {
        if (!g_Player.IsMusic || !UsingInternalMusicPlayer)
        {
          playlistPlayer.Reset();
        }

        playlistPlayer.Play(playStartIndex);

        if (!g_Player.Playing)
        {
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
          playlistPlayer.Play(playStartIndex);
        }

        bool didJump = DoPlayNowJumpTo(playList.Count);
        Log.Debug("GUIMusicFiles: Doing play now jump to: {0} ({1})", PlayNowJumpTo, didJump);
      }
    }

    #endregion

    private bool IsShare(GUIListItem pItem)
    {
      if (pItem.Path.Length == 0)
      {
        return false;
      }

      Share share = _virtualDirectory.GetShare(pItem.Path);
      bool isCdOrDvd = Util.Utils.IsDVD(pItem.Path);

      if (!isCdOrDvd && share != null && share.Path == pItem.Path)
      {
        return true;
      }

      else
      {
        return false;
      }
    }

    // Need to remove this and allow the rmote plugins to handle anti-repeat logic.
    // We also need some way for MP to handle anti-repeat for keyboard events
    private bool AntiRepeatActive()
    {
      TimeSpan ts = DateTime.Now - Previous_ACTION_PLAY_Time;

      // Ignore closely spaced calls due to rapid-fire ACTION_PLAY events...
      if (ts < AntiRepeatInterval)
      {
        return true;
      }

      else
      {
        return false;
      }
    }

    private bool IsCD(string path)
    {
      if (Util.Utils.IsDVD(path))
      {
        string rootDir = path.Substring(0, 2);
        string video_tsPath = Path.Combine(rootDir, "VIDEO_TS");
        if (!Directory.Exists(video_tsPath))
        {
          return true;
        }
      }

      return false;
    }

    private bool IsDVD(string path)
    {
      if (Util.Utils.IsDVD(path))
      {
        string rootDir = path.Substring(0, 2);
        string video_tsPath = Path.Combine(rootDir, "VIDEO_TS");
        if (Directory.Exists(video_tsPath))
        {
          return true;
        }
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
      if (windowId == (int) Window.WINDOW_ARTIST_INFO)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_FULLSCREEN_VIDEO)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_FULLSCREEN_MUSIC)
      {
        return true; //SV Added by SteveV 2006-09-07
      }
      if (windowId == (int) Window.WINDOW_MUSIC)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_ALBUM)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_ARTIST)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_COVERART_GRABBER_PROGRESS)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_FAVORITES)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_FILES)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_GENRE)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_INFO)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_PLAYING_NOW)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_PLAYLIST)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_TOP100)
      {
        return true;
      }
      if (windowId == (int) Window.WINDOW_MUSIC_YEARS)
      {
        return true;
      }
      return false;
    }

    #region ISetupForm Members

    public bool DefaultEnabled()
    {
      return true;
    }

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
      return "Music";
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(2);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my music.png";
      return true;
    }

    public string Author()
    {
      return "Frodo, SteveV, rtv, hwahrmann";
    }

    public string Description()
    {
      return "Plugin to play & organize your music";
    }

    public void ShowPlugin()
    {
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