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
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
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
  /// Class is for GUI interface to music database views
  /// </summary>
  public class GUIMusicGenres : GUIMusicBaseWindow
  {
    #region comparer

    private class TrackComparer : IComparer<PlayListItem>
    {
      public int Compare(PlayListItem pi1, PlayListItem pi2)
      {
        MusicTag tag1 = (MusicTag)pi1.MusicTag;
        MusicTag tag2 = (MusicTag)pi2.MusicTag;
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

    #endregion

    #region Base variables

    private DirectoryHistory m_history = new DirectoryHistory();
    private int m_iItemSelected = -1;
    //viewDefaultLayouts stores the default layout (list, album, filmstrip etc)
    //with first dimension being view number and second dimension
    //being level within the view
    private Layout[,] viewDefaultLayouts;
    private bool[,] sortasc;
    private MusicSort.SortMethod[,] sortby;
    private static string _showArtist = string.Empty;

    private int _currentLevel;
    private ViewDefinition _currentView;

    private DateTime Previous_ACTION_PLAY_Time = DateTime.Now;
    private TimeSpan AntiRepeatInterval = new TimeSpan(0, 0, 0, 0, 500);

    private bool m_foundGlobalSearch = false;

    #endregion

    public GUIMusicGenres()
    {
      GetID = (int)Window.WINDOW_MUSIC_GENRE;
      GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
    }

    #region Serialisation

    protected override void LoadSettings()
    {
      base.LoadSettings();

      if (PluginManager.PluginEntryExists("Search music") && PluginManager.IsPluginNameEnabled2("Search music"))
      {
        m_foundGlobalSearch = true;
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
        // if we do ff or rew, then reset speed to normal and ignore the play command
        if (g_Player.IsMusic && g_Player.Speed != 1)
        {
          g_Player.Speed = 1;
          return;
        }

        GUIListItem item = facadeLayout.SelectedListItem;

        if (AntiRepeatActive() || item == null || item.Label == "..")
        {
          return;
        }

        if (GetFocusControlId() == facadeLayout.GetID)
        {
          // only start something is facade is focused
          AddSelectionToCurrentPlaylist(true, false);
        }
      }
    }

    #region overrides

    public override bool Init()
    {
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\mymusicgenres.xml"));
    }

    protected override string SerializeName
    {
      get { return "mymusicgenres"; }
    }

    protected override Layout CurrentLayout
    {
      get
      {
        if (handler.View != null)
        {
          if (viewDefaultLayouts == null)
          {
            viewDefaultLayouts = new Layout[handler.Views.Count,50];

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition)handler.Views[i].Filters[j];
                viewDefaultLayouts[i, j] = GetLayoutNumber(def.DefaultView);
              }
            }
          }

          return viewDefaultLayouts[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }
        else
        {
          return Layout.List;
        }
      }
      set { viewDefaultLayouts[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
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
                FilterDefinition def = (FilterDefinition)handler.Views[i].Filters[j];
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
            sortStrings.Add("Disc#");
            sortStrings.Add("Composer");
            sortStrings.Add("Times Played");

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition)handler.Views[i].Filters[j];
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
                    sortby[i, j] = (MusicSort.SortMethod)defaultSort;
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
        FilterDefinition def = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];

        if ((def.Where == "albumartist" || def.Where == "album") && value == MusicSort.SortMethod.Artist)
        {
          sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = MusicSort.SortMethod.AlbumArtist;
        }
        else
        {
          sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value;
        }
      }
    }

    protected override bool AllowLayout(Layout layout)
    {
      return base.AllowLayout(layout);
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
            if (handler.CurrentLevel < 0)
            {
              handler.CurrentLevel = 0;
            }
            else
            {
              handler.CurrentLevel--;
              m_iItemSelected = -1;
              LoadDirectory("db_view");
              return;
            }
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeLayout[0];

        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          handler.CurrentLevel--;
          m_iItemSelected = -1;
          LoadDirectory("db_view");
          return;
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      // if hyperlink parameter is set (to string ID not actual name)
      // then load that view
      if (_loadParameter != null)
      {
        bool viewFound = false;
        foreach (ViewDefinition v in handler.Views)
        {
          if (v.Name == _loadParameter)
          {
            MusicState.View = v.Name; //don't just set _currentView as this is used below
            m_iItemSelected = -1; //remove any selected item from previous selection
            viewFound = true;
          }
        }
        if (!viewFound)
        {
          // got here as parameter passed from hyperlinkParameter value
          // did not match to a view name
          Log.Error("Invalid view load parameter: {0} when loading music genres, using default view", _loadParameter);
        }
      }

      string view = MusicState.View;
      if (view == string.Empty)
      {
        view = ((ViewDefinition)handler.Views[0]).Name;
      }

      if (_currentView != null && _currentView.Name == view)
      {
        ((MusicViewHandler)handler).Restore(_currentView, _currentLevel);
      }
      else
      {
        handler.CurrentView = view;
      }


      // Set views
      if (btnViews != null)
      {
        InitViewSelections();
      }

      LoadDirectory("db_view");

      if (facadeLayout.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnLayouts.GetID);
      }

      if (_showArtist != string.Empty)
      {
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem item = facadeLayout[i];
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

      // When we return from Fullscreen Music (the Visualisation screen), the page is reloaded again
      // The currently playing item will not be focused.
      // So we check here, if we have something playing and will focus the item
      if (g_Player.Playing && g_Player.IsMusic)
      {
        int currentItem = playlistPlayer.CurrentSong;
        PlayListItem item = playlistPlayer.GetCurrentItem();
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, currentItem, 0, null);
        msg.Label = item.FileName;
        GUIGraphicsContext.SendMessage(msg);
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      _currentLevel = handler.CurrentLevel; 
      _currentView = ((MusicViewHandler)handler).GetView();
      m_iItemSelected = facadeLayout.SelectedListItemIndex;

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
          int activeWindow = (int)GUIWindowManager.ActiveWindow;
          VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
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
        GUIMusicFiles musicFilesWnd = (GUIMusicFiles)GUIWindowManager.GetWindow((int)Window.WINDOW_MUSIC_FILES);
        musicFilesWnd.PlayCD();
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
      FilterDefinition filter = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];
      if (filter.SqlOperator == "group")
      {
        strThumb = GUIGraphicsContext.GetThemedSkinFile(@"\media\alpha\" + item.Label + @".png");
        if (Util.Utils.FileExistsInCache(strThumb))
        {
          item.IconImage = strThumb;
          item.IconImageBig = strThumb;
          item.ThumbnailImage = strThumb;
        }
        // Add Code here if users want to add pics showing "A", "B" and "C" ;)
      }
      else
      {
        switch (filter.Where)
        {
          case "genre":
            strThumb = Util.Utils.GetCoverArt(Thumbs.MusicGenre, item.Label);
            if (Util.Utils.FileExistsInCache(strThumb))
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
            if (Util.Utils.FileExistsInCache(strThumb))
            {
              item.IconImage = strThumb;
              item.IconImageBig = strThumb;
              item.ThumbnailImage = strThumb;
            }
            break;

          case "disc#":
          case "track":
            goto case "album";

          case "album":

            bool thumbFound = false;
            MusicTag tag = item.MusicTag as MusicTag;
            strThumb = Util.Utils.GetAlbumThumbName(tag.Artist, tag.Album);
            if (Util.Utils.FileExistsInCache(strThumb))
            {
              item.IconImage = strThumb;
              item.IconImageBig = strThumb;
              item.ThumbnailImage = strThumb;
              thumbFound = true;
            }

            if (item.IsFolder && _useFolderThumbs && !thumbFound)
            {
              strThumb = Util.Utils.GetLocalFolderThumb(item.Path);
              if (Util.Utils.FileExistsInCache(strThumb))
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
                  if (Util.Utils.FileExistsInCache(strThumb))
                  {
                    FolderThumbCacher thumbworker = new FolderThumbCacher(Path.GetDirectoryName(strThumb), false);
                  }
                }
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
          if (Util.Utils.FileExistsInCache(strLarge))
          {
            item.ThumbnailImage = strLarge;
          }
        }
      }
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeLayout.SelectedListItem;

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
        if (item.Label == "..")
        {
          // we have clicked on the ".." entry
          // so go up a level in view
          handler.CurrentLevel--;
        }
        else
        {
          // this is a level in the view above the bottom
          ((MusicViewHandler)handler).Select(item.AlbumInfoTag as Song);
        }

        m_iItemSelected = -1;
        LoadDirectory("db_view");
      }
      else
      {
        // we have selected an item at the bottom level of the view
        // so play what is selected
        bool clearPlaylist = false;
        if (_selectOption == "play" || !g_Player.Playing || !g_Player.IsMusic)
        {
          clearPlaylist = true;
        }
        AddSelectionToCurrentPlaylist(clearPlaylist, _addAllOnSelect);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeLayout.SelectedListItem;

      if (item == null)
      {
        return;
      }

      int itemNo = facadeLayout.SelectedListItemIndex;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.AddLocalizedString(4552); // Play now
      if (g_Player.Playing && g_Player.IsMusic)
      {
        dlg.AddLocalizedString(4551); // Play next
      }
      // only offer to queue items if
      // (a) playlist screen shows now playing list (_playlistIsCurrent is true) OR
      // (b) playlist screen is showing playlist (not what is playing) but music that is being played
      // is not from playlist (TEMP playlist is being used)
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
          AddSelectionToCurrentPlaylist(true, false);
          break;

        case 4551: // Play next (insert after current song)
          InsertSelectionToPlaylist(false);
          break;

        case 1225: // queue item at end of current playlist
          AddSelectionToCurrentPlaylist(false, false);
          break;

        case 1226: // queue all items at end of current playlist
          AddSelectionToCurrentPlaylist(false, true);
          break;

        case 926:  // add to playlist
          AddSelectionToPlaylist();
          break;

          //case 136: // show playlist
        case 4553: // show playlist
          GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYLIST);
          break;
        case 930: // add to favorites
          AddSongToFavorites(item);
          break;
        case 931: // Rating
          OnSetRating(facadeLayout.SelectedListItemIndex);
          break;
        case 718: // Clear top 100
          m_database.ResetTop100();
          LoadDirectory("db_view");
          break;
      }
    }

    #endregion

    private void keyboard_TextChanged(int kindOfSearch, string data)
    {
      facadeLayout.Filter(kindOfSearch, data);
    }

    /// <summary>
    /// this is actually loading a database view not a directory
    /// and this is done via view handler so parameter is not used
    /// but is needed to override method in base class
    /// </summary>
    /// <param name="strNotUsed">Used to implement method in base class but not used</param>
    protected override void LoadDirectory(string strNotUsed)
    {
      GUIWaitCursor.Show();
      GUIListItem SelectedItem = facadeLayout.SelectedListItem;

      int previousLevel = ((MusicViewHandler)handler).PreviousLevel;
      string strSelectedItem = string.Empty;

      if (SelectedItem != null)
      {
        // if there is an item selected and we are loading a new view
        // then store the existing value so when we navigate back up through
        // the view levels we can focus on the item we had selected
        // we can not use current level in the name for the directory history
        // as current level gets updated before LoadDirectory is called
        // therefore use previous level which is set by the music view handler
        // when that returns (ie. that will be level of the view user has
        // made selection from as it has not been cleared yet)
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          m_history.Set(SelectedItem.Label, handler.LocalizedCurrentView + "." +
                                            previousLevel.ToString());
        }
      }

      List<Song> songs;
      if (!((MusicViewHandler)handler).Execute(out songs))
      {
        GUIWaitCursor.Hide();
        Action action = new Action();
        action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
        GUIGraphicsContext.OnAction(action);
        return;
      }

      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      SwitchLayout();

      List<GUIListItem> itemsToAdd = new List<GUIListItem>();

      TimeSpan totalPlayingTime = new TimeSpan();

      if (previousLevel > handler.CurrentLevel)
      {
        // only need to lookup values when navigating back up through the view
        strSelectedItem = m_history.Get(handler.LocalizedCurrentView + "." + handler.CurrentLevel.ToString());
      }

      #region handle pin protected share

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
                catch (Exception) {}
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

      #endregion

      if (handler.CurrentLevel > 0)
      {
        // add ".." folder item if not at bottom level of view
        GUIListItem pItem = new GUIListItem("..");
        pItem.Path = string.Empty;
        pItem.IsFolder = true;
        Util.Utils.SetDefaultIcons(pItem);
        itemsToAdd.Add(pItem);
      }

      for (int i = 0; i < songs.Count; ++i)
      {
        Song song = songs[i];
        GUIListItem item = new GUIListItem();

        MusicTag tag = new MusicTag();
        tag = song.ToMusicTag();
        item.AlbumInfoTag = song;
        item.MusicTag = tag;

        if (handler.CurrentLevel + 1 < handler.MaxLevels)
        {
          item.IsFolder = true;
          item.Label = MusicViewHandler.GetFieldValue(song, handler.CurrentLevelWhere);
          SetSortLabel(ref item, CurrentSortMethod, handler.CurrentLevelWhere);
        }
        else
        {
          item.IsFolder = false;
          if (!GUIMusicBaseWindow.SetTrackLabels(ref item, CurrentSortMethod))
          {
            item.Label = song.Title;
          }
        }

        if (tag != null)
        {
          if (tag.Duration > 0)
          {
            totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
          }
        }

        item.Path = song.FileName;

        if (!string.IsNullOrEmpty(_currentPlaying) &&
            item.Path.Equals(_currentPlaying, StringComparison.OrdinalIgnoreCase))
        {
          item.Selected = true;
        }

        item.Duration = song.Duration;
        tag.TimesPlayed = song.TimesPlayed;
        item.Rating = song.Rating;
        item.Year = song.Year;
        item.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        itemsToAdd.Add(item);
      }

      itemsToAdd.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));

      int iItem = 0; // used to hold index of item to select			
      bool itemSelected = false;
      for (int i = 0; i < itemsToAdd.Count; ++i)
      {
        if (!itemSelected && itemsToAdd[i].Label == strSelectedItem)
        {
          iItem = i;
          itemSelected = true;
        }
        facadeLayout.Add(itemsToAdd[i]);
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
      else if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, m_iItemSelected);
      }
      else
      {
        SelectCurrentItem();
      }

      UpdateButtonStates();
      GUIWaitCursor.Hide();
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

    protected override void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      base.item_OnItemSelected(item, parent);
    }

    private static void SetSortLabel(ref GUIListItem item, MusicSort.SortMethod CurrentSortMethod,
                                     String CurrentLevelWhere)
    {
      MusicTag tag = (MusicTag)item.MusicTag;
      if (tag == null)
      {
        item.Label2 = string.Empty;
        return;
      }

      switch (CurrentSortMethod)
      {
        case MusicSort.SortMethod.Date:
          item.Label2 = tag.DateTimeModified.ToShortDateString();
          break;
        case MusicSort.SortMethod.Artist:
          if (CurrentLevelWhere != "artist")
          {
            item.Label2 = tag.Artist;
          }
          break;
        case MusicSort.SortMethod.Album:
          if (CurrentLevelWhere != "album")
          {
            item.Label2 = tag.Album;
          }
          break;
        case MusicSort.SortMethod.Rating:
          if (CurrentLevelWhere != "rating")
          {
            item.Label2 = tag.Rating > 0 ? tag.Rating.ToString() : string.Empty;
          }
          break;
        case MusicSort.SortMethod.AlbumArtist:
          if (CurrentLevelWhere != "albumartist")
          {
            item.Label2 = tag.AlbumArtist;
          }
          break;
        case MusicSort.SortMethod.Year:
          if (CurrentLevelWhere != "year")
          {
            item.Label2 = tag.Year >= 1900 ? tag.Year.ToString() : string.Empty;
          }
          break;
        case MusicSort.SortMethod.DiscID:
          item.Label = tag.Album;
          item.Label2 = tag.DiscID > 0 ? tag.DiscID.ToString() : string.Empty;
          break;
        default:
          item.Label2 = string.Empty;
          break;
      }
    }

    protected override void OnSort()
    {
      bool isSortable = true;
      isSortable = IsSortableView(handler.View, handler.CurrentLevel);

      if (isSortable)
      {
        // set labels
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem item = facadeLayout[i];
          if (item.IsFolder)
          {
            SetSortLabel(ref item, CurrentSortMethod, handler.CurrentLevelWhere);
          }
          else
          {
            GUIMusicBaseWindow.SetTrackLabels(ref item, CurrentSortMethod);
          }
        }
        facadeLayout.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      }

      UpdateButtonStates();
      SelectCurrentItem();

      if (btnSortBy != null)
      {
        btnSortBy.Disabled = !isSortable;
      }
    }

    #region playlist management

    /// <summary>
    /// Adds the songs for the selected GUIListItem and determines
    /// what tracks need to be added to the playlist
    /// </summary>
    /// <param name="clearPlaylist">If True then current playlist will be cleared</param>
    /// <param name="addAllTracks">Whether to add all tracks in the current folder</param>
    protected override void AddSelectionToCurrentPlaylist(bool clearPlaylist, bool addAllTracks)
    {
      List<Song> songs = GetSongsForSelection(addAllTracks);
      List<PlayListItem> pl = ConvertSongsToPlaylist(songs);

      // only apply further sort if a folder has been selected
      // if user has selected a track then add in order displayed
      GUIListItem selectedItem = facadeLayout.SelectedListItem;
      if (selectedItem.IsFolder)
      {
        pl.Sort(new TrackComparer());
      }
      base.AddItemsToCurrentPlaylist(pl, clearPlaylist, addAllTracks);
    }

    /// <summary>
    /// Add songs to playlist without affecting what is playing
    /// </summary>
    protected override void AddSelectionToPlaylist()
    {
      List<Song> songs = GetSongsForSelection(false);
      List<PlayListItem> pl = ConvertSongsToPlaylist(songs);

      // only apply further sort if a folder has been selected
      // if user has selected a track then add in order displayed
      GUIListItem selectedItem = facadeLayout.SelectedListItem;
      if (selectedItem.IsFolder)
      {
        pl.Sort(new TrackComparer());
      }
      base.AddItemsToPlaylist(pl);
    }

    /// <summary>
    /// Inserts the songs for selected GUIListItem and determines
    /// what tracks need to be inserted into the playlist
    /// </summary>
    /// <param name="addAllTracks">Whether to insert all tracks in folder</param>
    private void InsertSelectionToPlaylist(bool addAllTracks)
    {
      List<Song> songs = GetSongsForSelection(addAllTracks);
      List<PlayListItem> pl = ConvertSongsToPlaylist(songs);

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
    /// Return a list of songs for the current selected item
    /// </summary>
    /// <returns>A list of songs</returns>
    /// <param name="addAllTracks">Whether to add all tracks</param>
    private List<Song> GetSongsForSelection(bool addAllTracks)
    {
      List<Song> songs = new List<Song>();
      GUIListItem pItem;
      Song s;

      if (facadeLayout.SelectedListItem.IsFolder)
      {
        // this is selecting a folder (eg. pressing play on album or artist)
        songs = GetSongsForFolder();
      }
      else
      {
        // this is selecting at bottom level of view (tracks)

        // normally we will only add the selected track but
        int itemsToAdd = 1;

        if (addAllTracks)
        {
          // PlayAllOnSingleItemPlayNow allows for whole folder to be added so loop over all items
          itemsToAdd = facadeLayout.Count;

          for (int i = 0; i < itemsToAdd; i++)
          {
            if (facadeLayout[i].Label == "..")
            {
              // skip the ".."  parent item
              continue;
            }
            pItem = facadeLayout[i];
            s = (Song)pItem.AlbumInfoTag;
            songs.Add(s);
          }
        }
        else
        {
          pItem = facadeLayout.SelectedListItem;
          if (pItem.Label != "..")
          {
            // do nothing if we have the ".." parent item
            s = (Song)pItem.AlbumInfoTag;
            songs.Add(s);
          }
        }
      }
      return songs;
    }

    /// <summary>
    /// If the selected GUIListItem is a folder then this will
    /// build a list of songs within that folder
    /// eg if selected item is an album it will return all tracks on album
    ///    if selected item is a genre it will return all tracks in genre
    /// </summary>
    /// <returns>A list of songs in that folder</returns>
    private List<Song> GetSongsForFolder()
    {
      List<Song> songs = new List<Song>();
      GUIListItem pItem = facadeLayout.SelectedListItem;
      Song s = (Song)pItem.AlbumInfoTag;

      if (m_database == null)
      {
        // don't think this can ever happen but defensive check was in old code so no harm in leaving it
        m_database = MusicDatabase.Instance;
      }

      FilterDefinition filter = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];

      string strArtist = s.Artist;
      string strAlbumArtist = s.AlbumArtist;
      string strGenre = s.Genre;
      string strComposer = s.Composer;

      if (filter.SqlOperator == "group")
      {
        strArtist = strArtist + "%";
        strAlbumArtist = strAlbumArtist + "%";
        strGenre = strGenre + "%";
        strComposer = strComposer + "%";
      }

      switch (filter.Where)
      {
        case "artist":
          m_database.GetSongsByArtist(strArtist, ref songs);
          break;
        case "albumartist":
          m_database.GetSongsByAlbumArtist(strAlbumArtist, ref songs);
          break;
        case "album":
          m_database.GetSongsByAlbumArtistAlbum(strAlbumArtist, s.Album, ref songs);
          break;
        case "genre":
          m_database.GetSongsByGenre(strGenre, ref songs);
          break;
        case "year":
          m_database.GetSongsByYear(s.Year, ref songs);
          break;
        case "composer":
          m_database.GetSongsByComposer(strComposer, ref songs);
          break;
        case "conductor":
          m_database.GetSongsByComposer(s.Conductor, ref songs);
          break;
        case "disc#":
          m_database.GetSongsByAlbumArtistAlbumDisc(s.AlbumArtist, s.Album, s.DiscId, ref songs);
          break;
        default:
          Log.Debug("GUIMusicGenres: GetSongsForFolder - could not determine type for {0}", s.ToShortString());
          break;
      }
      return songs;
    }

    /// <summary>
    /// Converts a list of Songs into a list of PlayListItems
    /// </summary>
    /// <param name="songs">A list of songs</param>
    /// <returns>A list of playlist items</returns>
    private List<PlayListItem> ConvertSongsToPlaylist(List<Song> songs)
    {
      List<PlayListItem> pl = new List<PlayListItem>();
      foreach (Song s in songs)
      {
        pl.Add(s.ToPlayListItem());
      }

      return pl;
    }

    #endregion
  }
}