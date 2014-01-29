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
using System.Threading;
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
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Class is for GUI interface to music shares view
  /// </summary>
  [PluginIcons("GUIMusic.Music.gif", "GUIMusic.MusicDisabled.gif")]
  public class GUIMusicFiles : GUIMusicBaseWindow, ISetupForm, IShowPlugin
  {
    #region comparer

    private class TrackComparer : IComparer<PlayListItem>
    {
      public int Compare(PlayListItem item1, PlayListItem item2)
      {
        // Is this a top level artist folder?  If so, sort by path.
        if (item1.MusicTag == null || item2.MusicTag == null)
        {
          return item1.FileName.CompareTo(item2.FileName);
        }

          // Is it album folder or a song file. If album folder, sort by album name. Otherwise, sort by track number
        else
        {
          MusicTag tag1 = (MusicTag)item1.MusicTag;
          MusicTag tag2 = (MusicTag)item2.MusicTag;
          if (!string.IsNullOrEmpty(tag1.AlbumArtist) &&
              !string.IsNullOrEmpty(tag2.AlbumArtist) &&
              tag1.AlbumArtist != tag2.AlbumArtist)
          {
            return string.Compare(tag1.AlbumArtist, tag2.AlbumArtist);
          }
          if (!string.IsNullOrEmpty(tag1.Album) &&
              !string.IsNullOrEmpty(tag2.Album) &&
              tag1.Album != tag2.Album)
          {
            return string.Compare(tag1.Album, tag2.Album);
          }
          if (tag1.DiscID != tag2.DiscID)
          {
            return tag1.DiscID.CompareTo(tag2.DiscID);
          }
          return tag1.Track - tag2.Track;
        }
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
    private static VirtualDirectory _virtualDirectory;

    private int _selectedAlbum = -1;
    private int _selectedItem = -1;
    private string _discId = string.Empty;
    private static string currentFolder = string.Empty;
    private string _startDirectory = string.Empty;
    private string _destination = string.Empty;
    private string _fileMenuPinCode = string.Empty;
    private bool _useFileMenu = false;
    private bool _stripArtistPrefixes = false;

    private DateTime Previous_ACTION_PLAY_Time = DateTime.Now;
    private TimeSpan AntiRepeatInterval = new TimeSpan(0, 0, 0, 0, 500);
    private bool _switchRemovableDrives;

    private Thread _importFolderThread = null;
    private Queue<string> _scanQueue = new Queue<string>();

    #endregion

    public GUIMusicFiles()
    {
      GetID = (int)Window.WINDOW_MUSIC_FILES;
    }

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          if (message.Param1 == (int)Ripper.AutoPlay.MediaType.AUDIO)
          {
            if (message.Param2 == (int)Ripper.AutoPlay.MediaSubType.AUDIO_CD ||
                message.Param2 == (int)Ripper.AutoPlay.MediaSubType.FILES)
              PlayCD(message.Label);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          MusicCD = null;
          if (g_Player.Playing && g_Player.IsMusic &&
              message.Label.Equals(g_Player.CurrentFile.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase))
          {
            Log.Info("GUIMusicFiles: Stop since media is ejected");
            g_Player.Stop();
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
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
      }
    }

    // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when
    // the player is not playing)...
    private void GUIWindowManager_OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
          && GUIWindowManager.ActiveWindow == GetID)
      {
        GUIListItem item = facadeLayout.SelectedListItem;

        // if we do ff or rew, then reset speed to normal and ignore the play command
        if (g_Player.IsMusic && g_Player.Speed != 1)
        {
          g_Player.Speed = 1;
          return;
        }

        if (AntiRepeatActive() || item == null || item.Label == ".." || IsShare(item) || IsDVD(item.Path))
        {
          return;
        }

        if (GetFocusControlId() == facadeLayout.GetID)
        {
          AddSelectionToCurrentPlaylist(true, false);
        }
      }
    }

    #region Serialisation

    protected override void LoadSettings()
    {   
      base.LoadSettings();

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        currentLayout = (Layout)xmlreader.GetValueAsInt(SerializeName, "layout", (int)Layout.List);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);
        currentSortMethod = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethod", (int)MusicSort.SortMethod.Name);
        _stripArtistPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        MusicState.StartWindow = xmlreader.GetValueAsInt("music", "startWindow", GetID);
        MusicState.View = xmlreader.GetValueAsString("music", "startview", string.Empty);
        _useFileMenu = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        _fileMenuPinCode = Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", string.Empty));

        //string strDefault = xmlreader.GetValueAsString("music", "default", string.Empty);
        _virtualDirectory = VirtualDirectories.Instance.Music;
        if (currentFolder == string.Empty)
        {
          if (_virtualDirectory.DefaultShare != null)
          {
            if (_virtualDirectory.DefaultShare.IsFtpShare)
            {
              //remote:hostname?port?login?password?folder
              currentFolder = _virtualDirectory.GetShareRemoteURL(_virtualDirectory.DefaultShare);
              _startDirectory = currentFolder;
            }
            else
            {
              currentFolder = _virtualDirectory.DefaultShare.Path;
              _startDirectory = _virtualDirectory.DefaultShare.Path;
            }
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

      if (currentFolder.Length > 0 && currentFolder == _startDirectory)
      {
        VirtualDirectory vDir = new VirtualDirectory();
        vDir.LoadSettings("music");
        int pincode = 0;
        bool FolderPinProtected = vDir.IsProtectedShare(currentFolder, out pincode);
        if (FolderPinProtected)
        {
          currentFolder = string.Empty;
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
        xmlwriter.SetValue(SerializeName, "sortmethod", (int)currentSortMethod);
      }
    }

    #endregion

    #region overrides

    protected override string SerializeName
    {
      get { return "mymusic"; }
    }

    protected override bool AllowLayout(Layout layout)
    {
      if (layout == Layout.AlbumView)
      {
        return false;
      }
      return base.AllowLayout(layout);
    }

    public override void OnAdded()
    {
      base.OnAdded();
      currentFolder = string.Empty;

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        MusicState.StartWindow = xmlreader.GetValueAsInt("music", "startWindow", GetID);
        MusicState.View = xmlreader.GetValueAsString("music", "startview", string.Empty);
      }

      GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
      GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);
      LoadSettings();
      _virtualDirectory.AddDrives();
      _virtualDirectory.SetExtensions(Util.Utils.AudioExtensions);
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\mymusicsongs.xml"));
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeLayout.Focus)
        {
          GUIListItem item = facadeLayout[0];

          if ((item != null) && item.IsFolder && (item.Label == ".."))
          {
            LoadDirectory(item.Path);
            return;
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeLayout[0];

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
      base.OnPageLoad();

      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        _virtualDirectory.Reset();
      }

      if (MusicState.StartWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow((int)Window.WINDOW_MUSIC_GENRE);
        return;
      }

      LoadFolderSettings(currentFolder);
      LoadDirectory(currentFolder);

      if (btnSearch != null)
      {
        btnSearch.Disabled = true;
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _selectedItem = facadeLayout.SelectedListItemIndex;

      SaveFolderSettings(currentFolder);

      base.OnPageDestroy(newWindowId);
    }

    protected override void LoadDirectory(string strNewDirectory)
    {
      DateTime dtStart = DateTime.Now;
      GUIWaitCursor.Show();

      try
      {
        GUIListItem SelectedItem = facadeLayout.SelectedListItem;
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

        GUIControl.ClearControl(GetID, facadeLayout.GetID);

        if (strNewDirectory != currentFolder || _mapSettings == null)
        {
          LoadFolderSettings(strNewDirectory);
        }

        currentFolder = strNewDirectory;

        List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(currentFolder);

        string strSelectedItem = _dirHistory.Get(currentFolder);

        int iItem = 0;
        bool itemSelected = false;
        TimeSpan totalPlayingTime = new TimeSpan();

        GetTagInfo(ref itemlist);

        itemlist.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));

        for (int i = 0; i < itemlist.Count; ++i)
        {
          GUIListItem item = itemlist[i];

          if (!item.IsFolder)
          {
            // labels for folders are set by the virtual directory
            GUIMusicBaseWindow.SetTrackLabels(ref item, CurrentSortMethod);
          }

          MusicTag tag = (MusicTag)item.MusicTag;
          if (tag != null)
          {
            if (tag.Duration > 0)
            {
              totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
            }
          }

          if (!itemSelected && item.Label == strSelectedItem)
          {
            itemSelected = true;
            iItem = i;
          }

          if (!string.IsNullOrEmpty(_currentPlaying) &&
              item.Path.Equals(_currentPlaying, StringComparison.OrdinalIgnoreCase))
          {
            item.Selected = true;
          }

          item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

          facadeLayout.Add(item);
        }

        int iTotalItems = facadeLayout.Count;
        if (iTotalItems > 0)
        {
          GUIListItem rootItem = facadeLayout[0];
          if (rootItem.Label == "..")
          {
            iTotalItems--;
          }
        }

        //set object count label, total duration
        GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));

        if (totalPlayingTime.TotalSeconds > 0)
        {
          GUIPropertyManager.SetProperty("#totalduration",
                                         Util.Utils.SecondsToHMSString((int)totalPlayingTime.TotalSeconds));
        }
        else
        {
          GUIPropertyManager.SetProperty("#totalduration", string.Empty);
        }

        if (itemSelected)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iItem);
        }
        else if (_selectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItem);
        }
        else
        {
          SelectCurrentItem();
        }

        UpdateButtonStates();

        GUIWaitCursor.Hide();
      }
      catch (Exception ex)
      {
        GUIWaitCursor.Hide();
        Log.Error("GUIMusicFiles: An error occured while loading the directory {0}", ex.Message);
      }
      TimeSpan ts = DateTime.Now.Subtract(dtStart);
      Log.Debug("Folder: {0} : took : {1} s to load", strNewDirectory, ts.TotalSeconds);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnPlayCd)
      {
        PlayCD();
      }

      base.OnClicked(controlId, control, actionType);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD:
          PlayCD(message.Label);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          facadeLayout.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          facadeLayout.OnMessage(message);
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
      GUIListItem item = facadeLayout.SelectedListItem;
      _selectedListItem = item;
      int itemNo = facadeLayout.SelectedListItemIndex;

      if (item == null)
      {
        return;
      }

      bool isCD = IsCD(item.Path);
      bool isDVD = IsDVD(item.Path);
      bool isUpFolder = false;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (facadeLayout.Focus)
      {
        if (item.Label == "..")
        {
          isUpFolder = true;
        }
        if ((Path.GetFileName(item.Path) != string.Empty) || isCD && !isDVD)
        {
          if (!isUpFolder)
          {
            dlg.AddLocalizedString(4552); // Play now
            if (!item.IsFolder && g_Player.Playing && g_Player.IsMusic)
            {
              dlg.AddLocalizedString(4551); // Play next
            }

            // only offer to queue items if
            // (a) playlist screen shows now playing list (_playlistIsCurrent is true) OR
            // (b) playlist screen is showing playlist (not necessarily what is playing) and music 
            //     is being played from TEMP playlist
            if (_playlistIsCurrent || playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
            {
              dlg.AddLocalizedString(1225); // Queue item
              if (!item.IsFolder)
              {
                dlg.AddLocalizedString(1226); // Queue all items
              }
            }

            if (!_playlistIsCurrent)
            {
              dlg.AddLocalizedString(926); // add to playlist
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
            dlg.AddLocalizedString(1226); // Queue all items
            dlg.AddLocalizedString(102); //Scan
          }
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

      #region Eject/Load

      // CD/DVD/BD
      if (Util.Utils.getDriveType(item.Path) == 5)
      {
        if (item.Path != null)
        {
          var driveInfo = new DriveInfo(Path.GetPathRoot(item.Path));

          // There is no easy way in NET to detect open tray so we will check
          // if media is inside (load will be visible also in case that tray is closed but
          // media is not loaded)
          if (!driveInfo.IsReady)
          {
            dlg.AddLocalizedString(607); //Load  
          }

          dlg.AddLocalizedString(654); //Eject  
        }
      }

      if (Util.Utils.IsRemovable(item.Path) || Util.Utils.IsUsbHdd(item.Path))
      {
        dlg.AddLocalizedString(831);
      }

      #endregion

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

        case 1225: // Queue item
          AddSelectionToCurrentPlaylist(false, false);
          break;

        case 1226: // Queue all items
          AddSelectionToCurrentPlaylist(false, true);
          break;

        case 4551: // Play next
          InsertSelectionToPlaylist(false);
          break;

        case 4552: // Play now
          AddSelectionToCurrentPlaylist(true, false);
          break;

        case 926: // add to playlist
          AddSelectionToPlaylist();
          break;

        case 136: // show playlist
          _selectedItem = facadeLayout.SelectedListItemIndex;
          SaveFolderSettings(currentFolder);
          GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYLIST);
          break;

        case 607: // Load (only CDROM)
          Util.Utils.CloseCDROM(Path.GetPathRoot(item.Path));
          break;

        case 654: // Eject
          if (Util.Utils.getDriveType(item.Path) != 5)
          {
            Util.Utils.EjectCDROM();
          }
          else
          {
            if (item.Path != null)
            {
              var driveInfo = new DriveInfo(Path.GetPathRoot(item.Path));

              if (!driveInfo.IsReady)
              {
                Util.Utils.CloseCDROM(Path.GetPathRoot(item.Path));
              }
              else
              {
                Util.Utils.EjectCDROM(Path.GetPathRoot(item.Path));
              }
            }
          }
          LoadDirectory(string.Empty);
          break;

        case 930: // add to favorites
          AddSongToFavorites(item);
          break;

        case 931: // Rating
          OnSetRating(facadeLayout.SelectedListItemIndex);
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
              if (GetUserPasswordString(ref strUserCode) && strUserCode == _fileMenuPinCode)
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
              ViewDefinition view = (ViewDefinition)handler.Views[x];
              if (view.Name.ToLowerInvariant().IndexOf("artist") >= 0)
              {
                viewNr = x;
              }
            }
            if (viewNr < 0)
            {
              return;
            }
            ViewDefinition selectedView = (ViewDefinition)handler.Views[viewNr];
            handler.CurrentView = selectedView.Name;
            MusicState.View = selectedView.Name;
            GUIMusicGenres.SelectArtist(artist);
            int nNewWindow = (int)Window.WINDOW_MUSIC_GENRE;
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
              if (facadeLayout.Count <= 0)
              {
                GUIControl.FocusControl(GetID, btnLayouts.GetID);
              }
            }
          }

          break;
        case 831:
          string message = string.Empty;

          if (Util.Utils.IsUsbHdd(item.Path) || Util.Utils.IsRemovableUsbDisk(item.Path))
          {
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
          }
          else if (!RemovableDriveHelper.EjectMedia(item.Path, out message))
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
      GUIListItem item = facadeLayout.SelectedListItem;

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

        bool clearPlaylist = false;
        if (_selectOption == "play"  || !g_Player.Playing || !g_Player.IsMusic)
        {
          clearPlaylist = true;
        }
        AddSelectionToCurrentPlaylist(clearPlaylist, _addAllOnSelect);
      }
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
      GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_FILE);
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
          if (_selectedItem >= facadeLayout.Count)
            _selectedItem = facadeLayout.Count - 1;
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, _selectedItem);
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

    protected void OnFindCoverArt(int iItem)
    {
      GUIListItem pItem = facadeLayout[iItem];

      if (pItem.IsFolder && pItem.Label != "..")
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

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

        currentFolder = pItem.Path;
        GetTagInfo(ref items);
        currentFolder = oldFolder;
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
          int windowID = (int)Window.WINDOW_MUSIC_COVERART_GRABBER_PROGRESS;
          GUICoverArtGrabberProgress guiCoverArtProgress =
            (GUICoverArtGrabberProgress)GUIWindowManager.GetWindow(windowID);

          if (guiCoverArtProgress != null)
          {
            guiCoverArtProgress.CoverArtSelected +=
              new GUICoverArtGrabberProgress.CoverArtSelectedHandler(OnCoverArtGrabberCoverArtSelected);
            guiCoverArtProgress.CoverArtGrabDone +=
              new GUICoverArtGrabberProgress.CoverArtGrabDoneHandler(OnCoverArtGrabberDone);
            guiCoverArtProgress.TopLevelFolderName = pItem.Path;
            guiCoverArtProgress.Show(GetID);
          }
        }
      }
    }

    protected override void OnInfo(int iItem)
    {
      var pItem = facadeLayout[iItem];

      if (pItem.IsFolder && pItem.Label != "..")
      {  // read next level under folder to try and find tags
        string oldFolder = currentFolder;
        var dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.AudioExtensions);
        var items = dir.GetDirectoryUnProtectedExt(pItem.Path, true);

        if (items.Count < 2)
        {
          return;
        }

        currentFolder = pItem.Path;
        GetTagInfo(ref items);
        currentFolder = oldFolder;
        var item = items[1] as GUIListItem;
        var tag = item.MusicTag as MusicTag;

        // Is this an album?
        if (tag != null && !string.IsNullOrEmpty(tag.Album))
        {
          ShowAlbumInfo(tag.Artist, tag.Album);
          facadeLayout.RefreshCoverArt();
        }
        else
        { // Nope, it's a artist folder or share
          return;
        }
      }
      else
      {
        var song = new Song();
        var tag = pItem.MusicTag as MusicTag;
        if (tag != null)
        {
          song.Album = tag.Album;
          song.Artist = tag.Artist;
          song.AlbumArtist = tag.AlbumArtist;
          facadeLayout[iItem].AlbumInfoTag = song;
        }

        base.OnInfo(iItem);
      }
    }

    protected override void AddSongToFavorites(GUIListItem item)
    {
      Song song = item.AlbumInfoTag as Song;
      if (song == null)
      {
        List<GUIListItem> list = new List<GUIListItem>();
        list.Add(item);
        GetTagInfo(ref list);
      }
      base.AddSongToFavorites(item);
    }

    #endregion

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
        CurrentSortMethod = (MusicSort.SortMethod)_mapSettings.SortBy;
        CurrentLayout = (Layout)_mapSettings.ViewAs;
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
          CurrentSortMethod = (MusicSort.SortMethod)_mapSettings.SortBy;
          CurrentLayout = (Layout)share.DefaultLayout;
        }
      }
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        if (xmlreader.GetValueAsBool("music", "rememberlastfolder", false))
        {
          xmlreader.SetValue("music", "lastfolder", folderName);
        }
      }

      if (AllowLayout(CurrentLayout) == false)
      {
        // Switch to next valid layout.
        string layoutName = Enum.GetName(typeof(GUIFacadeControl.Layout), (int)CurrentLayout + 1);
        GUIFacadeControl.Layout nextLayout = GetLayoutNumber(layoutName);
        SwitchToNextAllowedLayout(nextLayout);
      }
      else
      {
        SwitchLayout();
      }

      UpdateButtonStates();
    }

    private void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory == string.Empty)
      {
        strDirectory = "root";
      }
      _mapSettings.SortAscending = CurrentSortAsc;
      _mapSettings.SortBy = (int)CurrentSortMethod;
      _mapSettings.ViewAs = (int)CurrentLayout;
      FolderSettings.AddFolderSetting(strDirectory, "MusicFiles", typeof (MapSettings), _mapSettings);
    }

    protected override void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      base.item_OnItemSelected(item, parent);
    }

    public static bool IsMusicWindow(int window)
    {
      if (window == (int)Window.WINDOW_MUSIC_PLAYLIST)
      {
        return true;
      }
      if (window == (int)Window.WINDOW_MUSIC_FILES)
      {
        return true;
      }
      if (window == (int)Window.WINDOW_MUSIC_GENRE)
      {
        return true;
      }
      if (window == (int)Window.WINDOW_MUSIC_PLAYING_NOW)
      {
        return true;
      }
      return false;
    }

    #region Handlers

    /// <summary>
    /// Queue the selected folder for scanning
    /// </summary>
    private void OnScan()
    {
      GUIListItem pItem = facadeLayout.SelectedListItem;
      if (pItem == null)
      {
        return;
      }

      string path = pItem.Path;
      if (!pItem.IsFolder)
      {
        path = Path.GetDirectoryName(pItem.Path);
      }

      _scanQueue.Enqueue(path);
      DoScan();
    }

    /// <summary>
    /// Retrieve item from queue and start scanning
    /// </summary>
    private void DoScan()
    {
      if (_importFolderThread == null)
      {
        MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(ReorgStatusChange);
        _importFolderThread = new Thread(m_database.ImportFolder);
        _importFolderThread.Name = "Import Folder";
        _importFolderThread.Priority = ThreadPriority.Lowest;
      }

      if (_importFolderThread.ThreadState != ThreadState.Running && _scanQueue.Count > 0)
      {
        _importFolderThread = new Thread(m_database.ImportFolder);
        _importFolderThread.Start(_scanQueue.Dequeue());
      }
    }

    /// <summary>
    /// When Scanning has finished, start scan of next folder
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReorgStatusChange(object sender, DatabaseReorgEventArgs e)
    {
      if (e.progress < 100)
      {
        return;
      }

      // Scan has finished, let's see, if we have more scans pending
      if (_scanQueue.Count > 0)
      {
        m_database.ImportFolder(_scanQueue.Dequeue());
      }
      else
      {
        GUIDialogNotify dlgNotify =
          (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(313));
          dlgNotify.SetText(GUILocalizeStrings.Get(317));
          dlgNotify.DoModal(GetID);
        }
      }
    }

    private void OnCoverArtGrabberCoverArtSelected(AlbumInfo albumInfo, string albumPath, bool bSaveToAlbumFolder,
                                                   bool bSaveToThumbsFolder)
    {
      SaveCoverArtImage(albumInfo, albumPath, bSaveToAlbumFolder, bSaveToThumbsFolder);
    }

    private void OnCoverArtGrabberDone(GUICoverArtGrabberProgress coverArtGrabberProgress)
    {
      facadeLayout.RefreshCoverArt();
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
      if (windowId == (int)Window.WINDOW_ARTIST_INFO)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_FULLSCREEN_VIDEO)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_FULLSCREEN_MUSIC)
      {
        return true; //SV Added by SteveV 2006-09-07
      }
      if (windowId == (int)Window.WINDOW_MUSIC_COVERART_GRABBER_PROGRESS)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_MUSIC_FILES)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_MUSIC_GENRE)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_MUSIC_INFO)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_MUSIC_PLAYING_NOW)
      {
        return true;
      }
      if (windowId == (int)Window.WINDOW_MUSIC_PLAYLIST)
      {
        return true;
      }
      return false;
    }

    #region playlist management

    /// <summary>
    /// Reads the tags of GUIListItem
    /// </summary>
    /// <param name="items">GUIListItem to be read</param>
    private void GetTagInfo(ref List<GUIListItem> items)
    {
      MusicTag tag;
      bool CDLookupAlreadyFailed = false;
      string strExtension;

      for (int i = 0; i < items.Count; i++)
      {
        GUIListItem pItem = items[i];
        if (pItem.IsFolder)
        {
          // no need to get tags for folders in shares view
          continue;
        }
        strExtension = Path.GetExtension(pItem.Path).ToLowerInvariant();

        if (strExtension == ".cda")
        {
          // we have a CD track so look up info
          if (!GetCDInfo(ref pItem, CDLookupAlreadyFailed))
          {
            // if CD info fails set failure flag to prevent further lookups
            Log.Error("Error looking up CD Track: {0}", pItem.Label);
            CDLookupAlreadyFailed = true;
          }
        }
        else
        {
          // not a CD track so attempt to pick up tag info
          tag = TagReader.TagReader.ReadTag(pItem.Path);
          if (tag != null)
          {
            tag.Artist = Util.Utils.FormatMultiItemMusicStringTrim(tag.Artist, _stripArtistPrefixes);
            tag.AlbumArtist = Util.Utils.FormatMultiItemMusicStringTrim(tag.AlbumArtist, _stripArtistPrefixes);
            tag.Genre = Util.Utils.FormatMultiItemMusicStringTrim(tag.Genre, false);
            tag.Composer = Util.Utils.FormatMultiItemMusicStringTrim(tag.Composer, _stripArtistPrefixes);
            pItem.MusicTag = tag;
            pItem.Duration = tag.Duration;
            pItem.Year = tag.Year;
            pItem.Rating = tag.Rating;
          }
        }
      }
    }

    /// <summary>
    /// Will add the current folder (and any sub-folders) to playlist
    /// </summary>
    /// <param name="clearPlaylist">If True then current playlist will be cleared</param>
    /// <param name="addAllTracks">Whether to add all tracks in folder</param>
    protected override void AddSelectionToCurrentPlaylist(bool clearPlaylist, bool addAllTracks)
    {
      GUIListItem selectedItem = facadeLayout.SelectedListItem;

      if (IsCD(selectedItem.Path) && selectedItem.Path.Length == 2)
      {
        // if user selects the drive itself from shares view for a CD
        // then treat as CD rather than normal share folder
        PlayCD(selectedItem.Path);
        return;
      }

      List<PlayListItem> pl = new List<PlayListItem>();
      AddFolderToPlaylist(selectedItem, ref pl, false, addAllTracks);

      // only apply further sort if a folder has been selected
      // if user has selected a track then add in order displayed
      if (selectedItem.IsFolder)
      {
        pl.Sort(new TrackComparer());
      }
      base.AddItemsToCurrentPlaylist(pl, clearPlaylist, addAllTracks);
    }

    /// <summary>
    /// Add tracks to playlist without affecting what is playing
    /// </summary>
    protected override void AddSelectionToPlaylist()
    {
      GUIListItem selectedItem = facadeLayout.SelectedListItem;

      List<PlayListItem> pl = new List<PlayListItem>();
      AddFolderToPlaylist(selectedItem, ref pl, false, false);

      // only apply further sort if a folder has been selected
      // if user has selected a track then add in order displayed
      if (selectedItem.IsFolder)
      {
        pl.Sort(new TrackComparer());
      }
      base.AddItemsToPlaylist(pl);
    }

    private void InsertSelectionToPlaylist(bool addAllTracks)
    {
      List<PlayListItem> pl = new List<PlayListItem>();
      AddFolderToPlaylist(facadeLayout.SelectedListItem, ref pl, false, addAllTracks);

      // only apply further sort if a folder has been selected
      // if user has selected a track then add in order displayed
      GUIListItem selectedItem = facadeLayout.SelectedListItem;
      if (selectedItem.IsFolder)
      {
        pl.Sort(new TrackComparer());
      }
      base.InsertItemsToPlaylist(pl);
    }

    /// <summary>
    /// Recursively adds songs to playlist
    /// </summary>
    /// <param name="item">GUIListItem to be added to playlist</param>
    /// <param name="pl">Playlist to be added to</param>
    /// <param name="playCD">Determines is whole CD playback has been requested</param>
    /// <param name="addAllTracks">Whether to add all tracks in folder</param>
    private void AddFolderToPlaylist(GUIListItem item, ref List<PlayListItem> pl, bool playCD, bool addAllTracks)
    {
      if (item.Label == "..")
      {
        // skip these navigation entries
        return;
      }
      if (item.IsFolder)
      {
        // recursively add sub folders
        List<GUIListItem> subFolders = _virtualDirectory.GetDirectoryExt(item.Path);
        GetTagInfo(ref subFolders);
        foreach (GUIListItem subItem in subFolders)
        {
          AddFolderToPlaylist(subItem, ref pl, playCD, addAllTracks);
        }
      }
      else
      {
        // add tracks
        if (addAllTracks)
        {
          GUIListItem selectedItem = null;
          if (facadeLayout != null)
          {
            selectedItem = facadeLayout.SelectedListItem; 
          }

          if (playCD)
          {
            // this is only set in PlayCD.  Playback is requested for whole drive
            // so only need to add the individual items as this is called
            // recursively
            pl.Add(ConvertItemToPlaylist(item));
          }
          else if (selectedItem == null  || selectedItem.IsFolder)
          {
            // selected item was a folder (or playback started from outside of music plugin)
            // so contents will get recursively added so just add item to playlist
            pl.Add(ConvertItemToPlaylist(item));
          }
          else
          {
            // selected item was not a folder so add any other tracks which
            // are on showing on the facade
            for (int i = 0; i < facadeLayout.Count; i++)
            {
              GUIListItem trackItem = facadeLayout[i];
              if (!trackItem.IsFolder)
              {
                pl.Add(ConvertItemToPlaylist(trackItem));
              }
            }
          }
        }
        else
        {
          pl.Add(ConvertItemToPlaylist(item));
        }
      }
    }

    /// <summary>
    /// Converts a GUIListItem into a list of PlayListItem
    /// </summary>
    /// <param name="items">GUIListItem to convert</param>
    /// <returns>Converted PlayListItem</returns>
    private PlayListItem ConvertItemToPlaylist(GUIListItem item)
    {
      PlayListItem pi = new PlayListItem();
      pi.Type = PlayListItem.PlayListItemType.Audio;
      pi.FileName = item.Path;
      pi.Description = item.Label;
      pi.Duration = item.Duration;
      pi.MusicTag = item.MusicTag;

      return pi;
    }

    #endregion

    #region cd playback

    public static CDInfoDetail MusicCD
    {
      get { return _freeDbCd; }
      set { _freeDbCd = value; }
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
      catch (Exception) {}
      return 1;
    }

    /// <summary>
    /// This will locate the first CD drive on the system and attempt to play
    /// This is also called from within GUIMusicGenres
    /// </summary>
    public void PlayCD()
    {
      string strFirstCDDrive = string.Empty;
      for (char c = 'C'; c <= 'Z'; c++)
      {
        if ((Util.Utils.GetDriveType(c + ":") & 5) == 5)
        {
          strFirstCDDrive = c + ":";
          break;
        }
      }
      PlayCD(strFirstCDDrive);
    }

    /// <summary>
    /// Attempt to play CD drive
    /// </summary>
    /// <param name="strDrive">Drive Letter of CD drive</param>
    private void PlayCD(string strDrive)
    {
      if (strDrive.Length == 1)
      {
        strDrive = strDrive + ":";
      }
      // Only try to play a CD if we got a valid Serial Number, which means a CD is inserted.
      if (Util.Utils.GetDriveSerial(strDrive) != string.Empty)
      {
        List<PlayListItem> pl = new List<PlayListItem>();
        GUIListItem item = new GUIListItem("CD_ROOT_FOLDER");
        item.IsFolder = true;
        item.Path = strDrive;
        AddFolderToPlaylist(item, ref pl, true, true);

        pl.Sort(new TrackComparer());

        base.AddItemsToCurrentPlaylist(pl, true, true);
      }
    }

    /// <summary>
    /// This looks up GUIListItem against FreeDB info to get CD track details
    /// </summary>
    /// <param name="pItem">A GUIListItem to lookup</param>
    /// <param name="CDLookupAlreadyFailed">Flag to indicate whether lookup has already failed</param>
    /// <returns>True: CDInfo has been obtained
    ///          False: there was an issue getting data</returns>
    private bool GetCDInfo(ref GUIListItem pItem, bool CDLookupAlreadyFailed)
    {
      bool cdInfoSuccessful = true;

      if (CDLookupAlreadyFailed || !Win32API.IsConnectedToInternet())
      {
        // no point in keep trying if previous call failed
        Log.Debug("MusicFiles: CD lookup already failed or not connected to internet");
        return false;
      }

      try
      {
        // check internet connectivity
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (null != pDlgOK && !Win32API.IsConnectedToInternet())
        {
          pDlgOK.SetHeading(703);
          pDlgOK.SetLine(1, 703);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);

          return false;
        }
        else if (!Win32API.IsConnectedToInternet())
        {
          return false;
        }

        FreeDBHttpImpl freedb = new FreeDBHttpImpl();
        char driveLetter = Path.GetFullPath(pItem.Path).ToCharArray()[0];

        Song song = new Song();

        // MusicCD stores details of current CD
        // this is to pick up if disc has been changed
        if (MusicCD != null)
        {
          if (freedb.GetCDDBDiscID(driveLetter).ToLowerInvariant() != MusicCD.DiscID)
          {
            MusicCD = null;
          }
        }

        if (MusicCD == null)
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
                // If we have "Autoplay" set to "Yes", we get the first element of the list, to avoid user input.
                if ((_discId == cds[0].DiscId))
                {
                  _discId = cds[0].DiscId;
                  MusicCD = freedb.GetDiscDetails(cds[0].Category, cds[0].DiscId);
                }
                else
                {
                  _discId = cds[0].DiscId;
                  //show dialog with all albums found
                  string szText = GUILocalizeStrings.Get(181);
                  GUIDialogSelect pDlg =
                    (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
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
                      return false;
                    }
                    MusicCD = freedb.GetDiscDetails(cds[_selectedAlbum].Category, cds[_selectedAlbum].DiscId);
                  }
                }
              }
            }
            freedb.Disconnect();
            if (MusicCD == null)
            {
              cdInfoSuccessful = false;
            }
          }
          catch (Exception)
          {
            MusicCD = null;
            cdInfoSuccessful = false;
          }
        }

        if (MusicCD != null) // if musicCD was configured correctly...
        {
          int trackno = GetCDATrackNumber(pItem.Path);
          CDTrackDetail track = MusicCD.getTrack(trackno);

          MusicTag tag = new MusicTag();
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
            pItem.Duration = track.Duration;
            tag.Title = track.Title;
            tag.Track = track.TrackNumber;
            tag.Genre = MusicCD.Genre;
            tag.Year = MusicCD.Year;
            pItem.Year = MusicCD.Year;
          }
          pItem.MusicTag = tag;
        }
      } // end of try
      catch (Exception e)
      {
        // log the problem...
        Log.Error("Unable to get CD Info: {0}", e.ToString());
      }

      return cdInfoSuccessful;
    }

    #endregion

    public static void ResetShares()
    {
      _virtualDirectory.Reset();
      _virtualDirectory.DefaultShare = null;
      _virtualDirectory.LoadSettings("music");
      
      if (_virtualDirectory.DefaultShare != null)
      {
        int pincode;
        bool folderPinProtected = _virtualDirectory.IsProtectedShare(_virtualDirectory.DefaultShare.Path, out pincode);
        if (folderPinProtected)
        {
          currentFolder = string.Empty;
        }
        else
        {
          currentFolder = _virtualDirectory.DefaultShare.Path;
        }
      }
    }

    public static void ResetExtensions(ArrayList extensions)
    {
      _virtualDirectory.SetExtensions(extensions);
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
      return "Frodo, SteveV, rtv, hwahrmann, JamesonUK";
    }

    public string Description()
    {
      return "Plugin to play & organize your music";
    }

    public void ShowPlugin() {}

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}