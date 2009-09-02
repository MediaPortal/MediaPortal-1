#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
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
  public class GUIMusicGenres : GUIMusicBaseWindow
  {
    #region comparer

    private class TrackComparer : IComparer<Song>
    {
      public int Compare(Song s1, Song s2)
      {
        return s1.Track - s2.Track;
      }
    }

    #endregion

    #region Base variables

    private DirectoryHistory m_history = new DirectoryHistory();
    private string m_strDirectory = string.Empty;
    private int m_iItemSelected = -1;
    private VirtualDirectory m_directory = new VirtualDirectory();
    private View[,] views;
    private bool[,] sortasc;
    private MusicSort.SortMethod[,] sortby;
    private static string _showArtist = string.Empty;

    private int _currentLevel;
    private ViewDefinition _currentView;
    private List<Share> _shareList = new List<Share>();

    private string m_strCurrentFolder = string.Empty;
    private string currentFolder = string.Empty;

    private DateTime Previous_ACTION_PLAY_Time = DateTime.Now;
    private TimeSpan AntiRepeatInterval = new TimeSpan(0, 0, 0, 0, 500);

    private bool m_foundGlobalSearch = false;

    #endregion

    public GUIMusicGenres()
    {
      GetID = (int) Window.WINDOW_MUSIC_GENRE;

      m_directory.AddDrives();
      m_directory.SetExtensions(Util.Utils.AudioExtensions);
      playlistPlayer = PlayListPlayer.SingletonPlayer;

      GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
    }

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        string strDefault = xmlreader.GetValueAsString("music", "default", string.Empty);

        if (PluginManager.PluginEntryExists("Search music") && PluginManager.IsPluginNameEnabled2("Search music"))
        {
          m_foundGlobalSearch = true;
        }

        _shareList.Clear();
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);
          ;

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
          share.Pincode = xmlreader.GetValueAsInt("music", strPincode, -1);

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
            }
            _shareList.Add(share);
          }
          else
          {
            break;
          }
        }
      }
    }

    #endregion

    // Make sure we get all of the ACTION_PLAY event (OnAction only receives the ACTION_PLAY event when 
    // the player is not playing)...
    private void GUIWindowManager_OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY
           || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
          && GUIWindowManager.ActiveWindow == GetID)
      {
        GUIListItem item = facadeView.SelectedListItem;

        if (AntiRepeatActive() || item == null || item.Label == "..")
        {
          return;
        }

        OnPlayNow(item, facadeView.SelectedListItemIndex);
      }
    }

    #region overrides

    public override bool Init()
    {
      m_strDirectory = string.Empty;
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return Load(GUIGraphicsContext.Skin + @"\mymusicgenres.xml");
    }

    protected override string SerializeName
    {
      get { return "mymusic" + handler.CurrentView; }
    }

    protected override View CurrentView
    {
      get
      {
        if (handler.View != null)
        {
          if (views == null)
          {
            views = new View[handler.Views.Count,50];

            ArrayList viewStrings = new ArrayList();
            viewStrings.Add("List");
            viewStrings.Add("Icons");
            viewStrings.Add("Big Icons");
            viewStrings.Add("Albums");
            viewStrings.Add("Filmstrip");

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition) handler.Views[i].Filters[j];
                views[i, j] = GetViewNumber(def.DefaultView);
              }
            }
          }

          return views[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }
        else
        {
          return View.List;
        }
      }
      set { views[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
    }

    protected override bool CurrentSortAsc
    {
      get
      {
        if (handler.View != null)
        {
          if (sortasc == null)
          {
            sortasc = new bool[handler.Views.Count,50];

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition) handler.Views[i].Filters[j];
                sortasc[i, j] = def.SortAscending;
              }
            }
          }

          return sortasc[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }

        return true;
      }
      set { sortasc[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
    }

    protected override MusicSort.SortMethod CurrentSortMethod
    {
      get
      {
        if (handler.View != null)
        {
          if (sortby == null)
          {
            sortby = new MusicSort.SortMethod[handler.Views.Count,50];

            ArrayList sortStrings = new ArrayList();
            sortStrings.Add("Name");
            sortStrings.Add("Date");
            sortStrings.Add("Size");
            sortStrings.Add("Track");
            sortStrings.Add("Duration");
            sortStrings.Add("Title");
            sortStrings.Add("Artist");
            sortStrings.Add("Album");
            sortStrings.Add("Filename");
            sortStrings.Add("Rating");
            sortStrings.Add("AlbumArtist");
            sortStrings.Add("Year");

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition) handler.Views[i].Filters[j];
                // Convert sort string to sort enumeration
                int defaultSort = sortStrings.IndexOf(def.DefaultSort);

                if (defaultSort != -1)
                {
                  if ((def.Where == "albumartist" || def.Where == "album") && def.DefaultSort == "Artist")
                  {
                    sortby[i, j] = MusicSort.SortMethod.AlbumArtist;
                  }
                  else
                  {
                    sortby[i, j] = (MusicSort.SortMethod) defaultSort;
                  }
                }
                else
                {
                  sortby[i, j] = MusicSort.SortMethod.Name;
                }
              }
            }
          }

          return sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }
        else
        {
          return MusicSort.SortMethod.Name;
        }
      }
      set
      {
        FilterDefinition def = (FilterDefinition) handler.View.Filters[handler.CurrentLevel];
        if (def.DefaultSort.ToLower() == "year")
        {
          sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = MusicSort.SortMethod.Year;
        }
        else if ((def.Where == "albumartist" || def.Where == "album") && value == MusicSort.SortMethod.Artist)
        {
          sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = MusicSort.SortMethod.AlbumArtist;
        }
        else
        {
          sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value;
        }
      }
    }

    protected override bool AllowView(View view)
    {
      return base.AllowView(view);
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
            handler.CurrentLevel--;
            m_iItemSelected = -1;
            LoadDirectory((handler.CurrentLevel + 1).ToString());
            return;
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];

        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          handler.CurrentLevel--;
          m_iItemSelected = -1;
          LoadDirectory((handler.CurrentLevel + 1).ToString());
          return;
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      string view = MusicState.View;

      if (view == string.Empty)
      {
        view = ((ViewDefinition) handler.Views[0]).Name;
      }

      if (_currentView != null && _currentView.Name == view)
      {
        handler.Restore(_currentView, _currentLevel);
      }
      else
      {
        handler.CurrentView = view;
      }


      LoadDirectory(m_strDirectory);

      if (facadeView.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnViewAs.GetID);
      }


      if (_showArtist != string.Empty)
      {
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          MusicTag tag = item.MusicTag as MusicTag;
          if (tag != null)
          {
            if (String.Compare(tag.Artist, _showArtist, true) == 0)
            {
              OnClick(i);
              break;
            }
          }
        }
      }
      _showArtist = string.Empty;

      using (Profile.Settings settings = new Profile.MPSettings())
      {
        playlistPlayer.RepeatPlaylist = settings.GetValueAsBool("musicfiles", "repeat", true);
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
      _currentLevel = handler.CurrentLevel;
      _currentView = handler.GetView();
      m_iItemSelected = facadeView.SelectedListItemIndex;

      if (GUIMusicFiles.IsMusicWindow(newWindowId))
      {
        MusicState.StartWindow = newWindowId;
      }
      else
      {
        MusicState.StartWindow = GetID;
      }

      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnSearch)
      {
        if (m_foundGlobalSearch)
        {
          GUIWindowManager.ActivateWindow(30885); // Check in GlobalSerach source
        }
        else
        {
          int activeWindow = (int) GUIWindowManager.ActiveWindow;
          VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard)
          {
            return;
          }
          keyboard.IsSearchKeyboard = true;
          keyboard.Text = string.Empty;
          keyboard.Reset();
          //keyBoard.KindOfSearch=(int)MediaPortal.Dialogs.VirtualSearchKeyboard.SearchKinds.SEARCH_STARTS_WITH;
          keyboard_TextChanged(0, "");
          keyboard.TextChanged += new VirtualKeyboard.TextChangedEventHandler(keyboard_TextChanged);
            // add the event handler
          keyboard.DoModal(activeWindow); // show it...
          keyboard.TextChanged -= new VirtualKeyboard.TextChangedEventHandler(keyboard_TextChanged);
            // remove the handler			
        }
      }

      if (control == btnPlayCd)
      {
        GUIMusicFiles musicFilesWnd = (GUIMusicFiles) GUIWindowManager.GetWindow((int) Window.WINDOW_MUSIC_FILES);
        musicFilesWnd.PlayCD();
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
      }

      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnRetrieveCoverArt(GUIListItem item)
    {
      if (item.Label == "..")
      {
        return;
      }
      Util.Utils.SetDefaultIcons(item);
      if (item.IsRemote)
      {
        return;
      }

      string strThumb = string.Empty;
      Song song = item.AlbumInfoTag as Song;
      if (song == null)
      {
        return;
      }

      // Get Cover Art for Index display
      FilterDefinition filter = (FilterDefinition) handler.View.Filters[handler.CurrentLevel];
      if (filter.SqlOperator == "group")
      {
        // Add Code here if users want to add pics showing "A", "B" and "C" ;)
      }
      else
      {
        switch (filter.Where)
        {
          case "genre":
            strThumb = Util.Utils.GetCoverArt(Thumbs.MusicGenre, item.Label);
            if (File.Exists(strThumb))
            {
              item.IconImage = strThumb;
              item.IconImageBig = strThumb;
              item.ThumbnailImage = strThumb;
            }
            break;

          case "albumartist":
          case "composer":
          case "conductor":
            goto case "artist";

          case "artist":
            strThumb = Util.Utils.GetCoverArt(Thumbs.MusicArtists, item.Label);
            if (File.Exists(strThumb))
            {
              item.IconImage = strThumb;
              item.IconImageBig = strThumb;
              item.ThumbnailImage = strThumb;
            }
            break;

          case "track":
            goto case "album";

          case "album":
            if (item.IsFolder && _useFolderThumbs)
            {
              strThumb = Util.Utils.GetLocalFolderThumb(item.Path);
              if (File.Exists(strThumb))
              {
                item.IconImage = strThumb;
                item.IconImageBig = strThumb;
                item.ThumbnailImage = strThumb;
              }
              else
              {
                // build folder.jpg from coverart
                if (_createMissingFolderThumbs)
                {
                  FolderThumbCreator thumbbuilder = new FolderThumbCreator(item.Path, song.ToMusicTag());
                }
                // cache the folder thumb - created thumbs will be cached automatically
                if (_createMissingFolderThumbCache)
                {
                  strThumb = Util.Utils.GetFolderThumb(item.Path);
                  if (File.Exists(strThumb))
                  {
                    FolderThumbCacher thumbworker = new FolderThumbCacher(Path.GetDirectoryName(strThumb), false);
                  }
                }
              }
            }
            else
            {
              MusicTag tag = item.MusicTag as MusicTag;
              strThumb = Util.Utils.GetAlbumThumbName(tag.Artist, tag.Album);
              if (File.Exists(strThumb))
              {
                item.IconImage = strThumb;
                item.IconImageBig = strThumb;
                item.ThumbnailImage = strThumb;
              }
            }
            break;

          default:
            Log.Warn("GUIMusicGenres: OnRetrieveCoverArt - no filter definition matched for item {0}", item.Label);
            base.OnRetrieveCoverArt(item);
            break;
        }

        if (!string.IsNullOrEmpty(strThumb))
        {
          // let us test if there is a larger cover art image
          string strLarge = Util.Utils.ConvertToLargeCoverArt(strThumb);
          if (File.Exists(strLarge))
          {
            item.ThumbnailImage = strLarge;
          }
        }
      }
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;

      if (item == null)
      {
        return;
      }

      if (PlayListFactory.IsPlayList(item.Path))
      {
        LoadPlayList(item.Path);
        return;
      }

      if (item.IsFolder)
      {
        if (item.Label == ".." && item.Path != string.Empty)
        {
          // Remove selection
          m_iItemSelected = -1;
          LoadDirectory(m_strDirectory);
        }
        else
        {
          if (item.Label == "..")
          {
            handler.CurrentLevel--;
          }
          else
          {
            handler.Select(item.AlbumInfoTag as Song);
          }

          m_iItemSelected = -1;
          //set level if no path is set
          if (item.Path == "")
          {
            LoadDirectory((handler.CurrentLevel + 1).ToString());
          }
          else
          {
            LoadDirectory(item.Path);
          }
        }
      }
      else
      {
        // play item
        //play and add current directory to temporary playlist
        int nFolderCount = 0;
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
        playlistPlayer.Reset();
        for (int i = 0; i < (int) facadeView.Count; i++)
        {
          GUIListItem pItem = facadeView[i];
          if (pItem.IsFolder)
          {
            nFolderCount++;
            continue;
          }
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.Type = PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName = pItem.Path;
          playlistItem.Description = pItem.Label;
          int iDuration = 0;
          MusicTag tag = pItem.MusicTag as MusicTag;
          if (tag != null)
          {
            iDuration = tag.Duration;
          }
          playlistItem.Duration = iDuration;
          playlistItem.MusicTag = tag;
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
        }

        //	Save current window and directory to know where the selected item was
        MusicState.TempPlaylistWindow = GetID;
        MusicState.TempPlaylistDirectory = m_strDirectory;

        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_TEMP;
        playlistPlayer.Play(iItem - nFolderCount);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;

      if (item == null)
      {
        return;
      }

      int itemNo = facadeView.SelectedListItemIndex;

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.AddLocalizedString(926); // Add to playlist
      dlg.AddLocalizedString(4557); // Add all to playlist
      dlg.AddLocalizedString(4551); // Play next
      dlg.AddLocalizedString(4552); // Play now
      dlg.AddLocalizedString(4521); // Show Album Info
      dlg.AddLocalizedString(4553); // Show playlist

      if (!item.IsFolder && !item.IsRemote)
      {
        Song song = item.AlbumInfoTag as Song;
        if (song.Id >= 0)
        {
          dlg.AddLocalizedString(930); //Add to favorites
          dlg.AddLocalizedString(931); //Rating
        }
      }
      if (handler.CurrentView == "271")
      {
        dlg.AddLocalizedString(718); //Clear top100
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 4521: // Show album info
          OnInfo(itemNo);
          break;

        case 4552: // Play now (clear playlist, play, and jump to Now playing)
          //OnPlayNow(itemNo);
          OnPlayNow(item, itemNo);
          break;

        case 4551: // Play next (insert after current song)
          //OnPlayNext(itemNo);
          OnPlayNext(item, itemNo);
          break;

        case 926: // add to playlist (add to end of playlist)
          OnQueueItem(itemNo);
          break;

        case 4557: // add all items in current list to end of playlist
          OnQueueAllItems();
          break;

          //case 136: // show playlist
        case 4553: // show playlist
          GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
          break;
        case 930: // add to favorites
          AddSongToFavorites(item);
          break;
        case 931: // Rating
          OnSetRating(facadeView.SelectedListItemIndex);
          break;
        case 718: // Clear top 100
          m_database.ResetTop100();
          LoadDirectory(m_strDirectory);
          break;
      }
    }

    protected override void OnQueueItem(int iItem)
    {
      CreatePlaylist((Song) ((GUIListItem) facadeView[iItem]).AlbumInfoTag);

      //move to next item and start playing
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem + 1);
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0 && !g_Player.Playing)
      {
        playlistPlayer.Reset();
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Play(0);
      }
    }

    protected void CreatePlaylist(Song song)
    {
      // if not at the maxlevel
      if (handler.CurrentLevel < handler.MaxLevels - 1)
      {
        // load all items of next level
        handler.Select(song);
        List<Song> songs = handler.Execute();


        //if current view is albums, then queue items by track
        FilterDefinition def = (FilterDefinition) handler.View.Filters[handler.CurrentLevel];
        if (def.Where == "title")
        {
          songs.Sort(new TrackComparer());
        }

        //loop through each song recursively
        for (int i = 0; i < songs.Count; ++i)
        {
          CreatePlaylist(songs[i]);
        }

        handler.CurrentLevel--;
        return;
      }

      else
      {
        if (song.Id > 0)
        {
          AddItemToPlayList(song);
        }
      }
    }

    #endregion

    protected void AddItemToPlayList(Song song)
    {
      if (Util.Utils.IsAudio(song.FileName) && !PlayListFactory.IsPlayList(song.FileName))
      {
        PlayListItem playlistItem = new PlayListItem();
        playlistItem.Type = PlayListItem.PlayListItemType.Audio;
        playlistItem.FileName = song.FileName;
        playlistItem.Description = song.Title;
        playlistItem.Duration = song.Duration;

        MusicTag tag = new MusicTag();
        tag = song.ToMusicTag();
        playlistItem.MusicTag = tag;

        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);
      }
    }

    private void keyboard_TextChanged(int kindOfSearch, string data)
    {
      facadeView.Filter(kindOfSearch, data);
    }

    protected override void LoadDirectory(string strNewDirectory)
    {
      GUIWaitCursor.Show();
      GUIListItem SelectedItem = facadeView.SelectedListItem;
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          m_history.Set(SelectedItem.Label, handler.CurrentLevel.ToString());
        }
      }
      m_strDirectory = strNewDirectory;

      GUIControl.ClearControl(GetID, facadeView.GetID);

      string strObjects = string.Empty;

      List<Song> songs = handler.Execute();
      if (songs.Count > 0) // some songs in there?
      {
        Song song = songs[0];
        if (song.FileName.Length > 0) // does a filename exits
        {
          foreach (Share share in _shareList)
          {
            if (song.FileName.Contains(share.Path)) // compare it with shares
            {
              if (share.Pincode != -1) // does it have a pincode?
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
                GUIWindowManager.SendMessage(msg); // ask for the userinput
                int iPincode = -1;
                try
                {
                  iPincode = Int32.Parse(msg.Label);
                }
                catch (Exception)
                {
                }
                if (iPincode != share.Pincode)
                {
                  songs.Clear();
                }
                break;
              }
            }
          }
        }
      }

      if (handler.CurrentLevel > 0)
      {
        GUIListItem pItem = new GUIListItem("..");
        pItem.Path = string.Empty;
        pItem.IsFolder = true;
        Util.Utils.SetDefaultIcons(pItem);
        facadeView.Add(pItem);
      }

      for (int i = 0; i < songs.Count; ++i)
      {
        Song song = songs[i];
        GUIListItem item = new GUIListItem();
        item.Label = song.Title;
        if (handler.CurrentLevel + 1 < handler.MaxLevels)
        {
          item.IsFolder = true;
        }
        else
        {
          item.IsFolder = false;
        }
        item.Path = song.FileName;
        item.Duration = song.Duration;

        MusicTag tag = new MusicTag();
        tag = song.ToMusicTag();
        item.Duration = tag.Duration;
        tag.TimesPlayed = song.TimesPlayed;

        item.Rating = song.Rating;
        item.Year = song.Year;
        item.AlbumInfoTag = song;
        item.MusicTag = tag;
        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
      }

      string strSelectedItem = m_history.Get(m_strDirectory);
      int iItem = 0;

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        if (item.Path.Equals(_currentPlaying, StringComparison.OrdinalIgnoreCase))
        {
          item.Selected = true;
          break;
        }
      }

      OnSort();
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        if (item.Label == strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
          break;
        }
        iItem++;
      }
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
      }

      SwitchView();
      GUIWaitCursor.Hide();
    }

    protected override void SetLabels()
    {
      ////base.SetLabels();
      ////for (int i = 0; i < facadeView.Count; ++i)
      ////{
      ////    GUIListItem item = facadeView[i];
      ////    handler.SetLabel(item.AlbumInfoTag as Song, ref item);
      ////}


      // SV
      // "Top100" can be renamed so this won't work...
      //if (handler.CurrentView == "Top100")

      // So we do this instead...
      if (!IsSortableView(handler.View, handler.CurrentLevel))
      {
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem item = facadeView[i];
          MusicTag tag = (MusicTag) item.MusicTag;
          if (tag != null)
          {
            int playCount = tag.TimesPlayed;
            string duration = Util.Utils.SecondsToHMSString(playCount*tag.Duration);
            item.Label = string.Format("{0:00}. {1} - {2}", playCount, tag.Artist, tag.Title);
            item.Label2 = duration;
          }
        }
      }

      else
      {
        base.SetLabels();
      }

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        handler.SetLabel(item.AlbumInfoTag as Song, ref item);
      }
    }

    private void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC:
          if (GUIWindowManager.ActiveWindow == GetID)
          {
            // SV
            //if (handler != null && handler.CurrentView == "Top100") return;
          }
          string strFile = message.Label;
          if (strFile.StartsWith(@"http://"))
          {
            break; // Don't try increasing the Top100 for streams
          }
          if (Util.Utils.IsAudio(strFile))
          {
            MusicDatabase dbs = MusicDatabase.Instance;
            dbs.IncrTop100CounterByFileName(strFile);
          }
          break;
      }
    }

    public static void SelectArtist(string artist)
    {
      _showArtist = artist;
    }

    private void AddItemToPlayList(GUIListItem pItem)
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      AddItemToPlayList(pItem, ref playList);
    }

    private void AddSongToPlayList(Song song)
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      AddSongToPlayList(song, playList);
    }

    private void AddSongToPlayList(Song song, PlayList playList)
    {
      MusicTag tag = new MusicTag();
      tag = song.ToMusicTag();

      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = PlayListItem.PlayListItemType.Audio;
      playlistItem.FileName = song.FileName;
      playlistItem.Description = song.Title;
      playlistItem.Duration = song.Duration;
      playlistItem.MusicTag = tag;
      playList.Add(playlistItem);
    }

    private void AddSongsToPlayList(List<Song> songs)
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      AddSongsToPlayList(songs, playList);
    }

    private void AddSongsToPlayList(List<Song> songs, PlayList playList)
    {
      foreach (Song song in songs)
      {
        AddSongToPlayList(song, playList);
      }
    }

    private void AddAlbumsToPlayList(List<Song> songs)
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      AddAlbumsToPlayList(songs, playList);
    }

    private void AddAlbumsToPlayList(List<Song> albums, PlayList playList)
    {
      foreach (Song album in albums)
      {
        List<Song> albumSongs = new List<Song>();
        MusicDatabase.Instance.GetSongsByAlbumArtistAlbum(album.AlbumArtist, album.Album, ref albumSongs);

        foreach (Song albumSong in albumSongs)
        {
          AddSongToPlayList(albumSong, playList);
        }
      }
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
        if (pItem.Label == "..")
        {
          return;
        }

        if (pItem.AlbumInfoTag != null)
        {
          List<Song> songs = new List<Song>();
          Song s = (Song) pItem.AlbumInfoTag;

          FilterDefinition filter = (FilterDefinition) handler.View.Filters[handler.CurrentLevel];

          switch (filter.Where)
          {
            case "artist":
              if (MusicDatabase.Instance.GetSongsByArtist(s.Artist, ref songs))
              {
                AddSongsToPlayList(songs, playList);
              }
              break;
            case "albumartist":
              if (MusicDatabase.Instance.GetSongsByAlbumArtist(s.AlbumArtist, ref songs))
              {
                AddSongsToPlayList(songs, playList);
              }
              break;
            case "album":
              songs.Add(s);
              AddAlbumsToPlayList(songs, playList);
              break;
            case "genre":
              if (MusicDatabase.Instance.GetSongsByGenre(s.Genre, ref songs))
              {
                AddSongsToPlayList(songs, playList);
              }
              break;
            case "year":
              if (MusicDatabase.Instance.GetSongsByYear(s.Year, ref songs))
              {
                AddSongsToPlayList(songs, playList);
              }
              break;
            case "composer":
              if (MusicDatabase.Instance.GetSongsByComposer(s.Composer, ref songs))
              {
                AddSongsToPlayList(songs, playList);
              }
              break;
            case "conductor":
              if (MusicDatabase.Instance.GetSongsByComposer(s.Conductor, ref songs))
              {
                AddSongsToPlayList(songs, playList);
              }
              break;
            default:
              Log.Debug("GUIMusicGenres: AddItemToPlayList - could not determine type for {0}", s.ToShortString());
              break;
          }
        }
      }
      else // item is not a folder
      {
        if (pItem.AlbumInfoTag != null)
        {
          Song song = (Song) pItem.AlbumInfoTag;
          MusicTag tag = new MusicTag();
          tag = song.ToMusicTag();

          PlayListItem pli = new PlayListItem();
          pli.MusicTag = tag;
          pli.FileName = pItem.Path;
          pli.Description = tag.Title;
          pli.Duration = tag.Duration;
          pli.Type = PlayListItem.PlayListItemType.Audio;
          playList.Add(pli);
        }
      }
    }

    protected void OnPlayNext(GUIListItem pItem, int iItem)
    {
      if (pItem == null)
      {
        return;
      }

      if (PlayListFactory.IsPlayList(pItem.Path))
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
        PlayList tempPlayList = new PlayList();

        for (int i = 0; i < playList.Count; i++)
        {
          if (i == index + 1)
          {
            AddItemToPlayList(pItem, ref tempPlayList);
          }

          tempPlayList.Add(playList[i]);
        }

        playList.Clear();

        // add each item of the playlist to the playlistplayer
        foreach (PlayListItem pli in tempPlayList)
        {
          playList.Add(pli);
        }
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

    protected void OnPlayNow(GUIListItem pItem, int iItem)
    {
      if (pItem == null)
      {
        return;
      }

      if (PlayListFactory.IsPlayList(pItem.Path))
      {
        return;
      }

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

      if (playList.Count > 0)
      {
        //SV
        //playlistPlayer.Reset();

        if (!g_Player.IsMusic || !UsingInternalMusicPlayer)
        {
          playlistPlayer.Reset();
        }

        playlistPlayer.Play(playStartIndex);

        //if (PlayNowJumpToWindowID != -1)
        //{
        //    if (PlayNowJumpToWindowID == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
        //    {
        //        GUIMusicPlayingNow nowPlayingWnd = (GUIMusicPlayingNow)GUIWindowManager.GetWindow(PlayNowJumpToWindowID);

        //        if (nowPlayingWnd != null)
        //            nowPlayingWnd.MusicWindow = this;
        //    }

        //    GUIWindowManager.ActivateWindow(PlayNowJumpToWindowID);
        //}

        if (!g_Player.Playing)
        {
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
          playlistPlayer.Play(playStartIndex);
        }

        //SV
        bool didJump = DoPlayNowJumpTo(playList.Count);
        Log.Info("GUIMusicGenres: Doing Play now jump to: {0} ({1})", PlayNowJumpTo, didJump);
      }
    }

    // Need to remove this and allow the remote plugins to handle anti-repeat logic.
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

    private void OnQueueAllItems()
    {
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      int index = Math.Max(playlistPlayer.CurrentSong, 0);

      for (int i = 0; i < facadeView.Count; i++)
      {
        GUIListItem item = facadeView[i];

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

    protected override void OnSort()
    {
      SetLabels();

      bool isSortable = true;

      // "Top100" can be renamed so this won't work...
      // isSortable = handler.CurrentView != "Top100";

      // So we do this instead...
      isSortable = IsSortableView(handler.View, handler.CurrentLevel);

      if (isSortable)
      {
        facadeView.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      }

      UpdateButtonStates();
      SelectCurrentItem();

      if (btnSortBy != null)
      {
        btnSortBy.Disabled = !isSortable;
      }
    }
  }
}