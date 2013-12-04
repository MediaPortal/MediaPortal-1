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
using System.ComponentModel;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Class is for GUI interface to music playist
  /// </summary>
  public class GUIMusicPlayList : GUIMusicBaseWindow
  {

    #region Variables

    private int m_iItemSelected = -1;
    private int m_iLastControl = 0;
    private string _playlistFolder = string.Empty;
    private string m_strCurrentFile = string.Empty;
    private bool _savePlaylistOnExit = false;
    private bool _resumePlaylistOnEnter = false;
    private string _defaultPlaylist = "default.m3u";
    private WaitCursor waitCursor;
    private static bool _movingItem = false;

    #endregion

    protected delegate void ThreadRefreshList();
    protected delegate void ThreadHideWaitCursor();

    [SkinControl(20)] protected GUIButtonControl btnShuffle = null;
    [SkinControl(21)] protected GUIButtonControl btnSave = null;
    [SkinControl(22)] protected GUIButtonControl btnClear = null;
    [SkinControl(26)] protected GUIButtonControl btnNowPlaying = null;
    [SkinControl(27)] protected GUICheckButton btnScrobble = null;
    [SkinControl(28)] protected GUIButtonControl btnScrobbleMode = null;
    [SkinControl(29)] protected GUIButtonControl btnScrobbleUser = null;
    [SkinControl(30)] protected GUICheckButton btnRepeatPlaylist = null;


    public GUIMusicPlayList()
    {
      GetID = (int)Window.WINDOW_MUSIC_PLAYLIST;
    }

    #region overrides

    public override bool Init()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _playlistFolder = xmlreader.GetValueAsString("music", "playlists", string.Empty);
        _savePlaylistOnExit = xmlreader.GetValueAsBool("musicfiles", "savePlaylistOnExit", false);
        _resumePlaylistOnEnter = xmlreader.GetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", false);
        playlistPlayer.RepeatPlaylist = xmlreader.GetValueAsBool("musicfiles", "repeat", true);
      }

      if (_resumePlaylistOnEnter)
      {
        Log.Info("GUIMusicPlaylist: Loading default playlist {0}", _defaultPlaylist);
        bw = new BackgroundWorker();
        bw.WorkerSupportsCancellation = true;
        bw.WorkerReportsProgress = false;
        bw.DoWork += new DoWorkEventHandler(bw_DoWork);
        bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        bw.RunWorkerAsync();
      }
      else
      {
        defaultPlaylistLoaded = true;
      }

      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      GUIWindowManager.OnNewAction += new OnActionHandler(this.OnNewAction);
      playlistPlayer.PlaylistChanged += playlistPlayer_Changed; 
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\myMusicplaylist.xml"));
    }

    public override void DeInit()
    {
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnThreadMessage);
      GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnNewAction);

      // Save the default Playlist
      if (_savePlaylistOnExit)
      {
          Log.Info("Playlist: Saving default playlist {0}", _defaultPlaylist);
          IPlayListIO saver = new PlayListM3uIO();
          PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
          PlayList playlistTmp = new PlayList();
          // Sort out Playlist Items residing on a CD, as they are gonna most likely to change
          foreach (PlayListItem item in playlist)
          {
              if (Path.GetExtension(item.FileName) != ".cda")
              {
                  playlistTmp.Add(item);
              }
          }

          if (playlistTmp.Count > 0)
          {
              saver.Save(playlistTmp, Path.Combine(_playlistFolder, _defaultPlaylist));
          }
      }

      base.DeInit();
    }

    protected override string SerializeName
    {
      get { return "mymusicplaylist"; }
    }

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        currentLayout = (Layout)xmlreader.GetValueAsInt(SerializeName, "layout", (int)Layout.List);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);
        currentSortMethod = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethod", (int)MusicSort.SortMethod.Name);
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
  
    // Fires every time - especially ACTION_MUSIC_PLAY even if we're already playing stuff
    private void OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_MUSIC_PLAY || action.wID == Action.ActionType.ACTION_PLAY) &&
          GUIWindowManager.ActiveWindow == GetID)
      {
        try
        {
          // Avoid double action if e.g. jumped to playlist screen.
          if (base.PlayNowJumpTo != PlayNowJumpToType.CurrentPlaylistAlways &&
              base.PlayNowJumpTo != PlayNowJumpToType.CurrentPlaylistMultipleItems)
          {
            if (GetFocusControlId() == facadeLayout.GetID)
            {
              if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC)
              {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
              }

              playlistPlayer.Play(facadeLayout.SelectedListItemIndex);
              bool didJump = DoPlayNowJumpTo(facadeLayout.Count);
              Log.Debug("GUIMusicPlaylist: Doing play now jump to: {0} ({1})", PlayNowJumpTo, didJump);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("GUIMusicPlaylist: Error in ACTION_PLAY: {0}", ex.Message);
        }
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      else if (action.wID == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP)
      {
        MovePlayListItemUp();
      }

      else if (action.wID == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN)
      {
        MovePlayListItemDown();
      }

      else if (action.wID == Action.ActionType.ACTION_DELETE_SELECTED_ITEM)
      {
        DeletePlayListItem();
      }

        // Handle case where playlist has been stopped and we receive a player action.
        // This allows us to restart the playback proccess...
      else if (action.wID == Action.ActionType.ACTION_MUSIC_PLAY
               || action.wID == Action.ActionType.ACTION_NEXT_ITEM
               || action.wID == Action.ActionType.ACTION_PAUSE
               || action.wID == Action.ActionType.ACTION_PREV_ITEM
        )
      {
        if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC)
        {
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

          if (string.IsNullOrEmpty(g_Player.CurrentFile))
          {
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

            if (playList != null && playList.Count > 0)
            {
              playlistPlayer.Play(0);
              UpdateButtonStates();
            }
          }
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      currentLayout = Layout.Playlist;
      facadeLayout.CurrentLayout = Layout.Playlist;

      LoadFacade();
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, m_iItemSelected);
      }
      if ((m_iLastControl == facadeLayout.GetID) && facadeLayout.Count <= 0)
      {
        m_iLastControl = btnNowPlaying.GetID;
        GUIControl.FocusControl(GetID, m_iLastControl);
      }
      if (facadeLayout.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnLayouts.GetID);
      }

      if (btnRepeatPlaylist != null)
      {
        btnRepeatPlaylist.Selected = playlistPlayer.RepeatPlaylist;
      }

      SelectCurrentPlayingSong();

      if (null != bw && bw.IsBusy && !bw.CancellationPending)
        ShowWaitCursor();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iItemSelected = facadeLayout.SelectedListItemIndex;
      using (Profile.Settings settings = new Profile.MPSettings())
      {
        settings.SetValueAsBool("musicfiles", "repeat", playlistPlayer.RepeatPlaylist);
      }
      HideWaitCursor();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnShuffle)
      {
        ShufflePlayList();
      }
      else if (control == btnSave)
      {
        SavePlayList();
      }
      else if (control == btnClear)
      {
        ClearPlayList();
      }
      else if ((btnRepeatPlaylist != null) && (control == btnRepeatPlaylist))
      {
        playlistPlayer.RepeatPlaylist = btnRepeatPlaylist.Selected;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          {
            for (int i = 0; i < facadeLayout.Count; ++i)
            {
              GUIListItem item = facadeLayout[i];
              if (item != null && item.Selected)
              {
                item.Selected = false;
                break;
              }
            }

            UpdateButtonStates();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED:
          {
            if (m_iLastControl == facadeLayout.GetID && facadeLayout.Count <= 0)
            {
              if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_MUSIC_PLAYLIST)
              {
                m_iLastControl = btnNowPlaying.GetID;
                GUIControl.FocusControl(GetID, m_iLastControl);
              }
            }

            SelectCurrentPlayingSong();
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void UpdateButtonStates()
    {
      // only update while playlist is visible
      if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_MUSIC_PLAYLIST)
      {
        if (facadeLayout != null)
        {
          if (facadeLayout.Count > 0)
          {
            btnClear.Disabled = false;
            btnSave.Disabled = false;
            }
            else
            {
            btnClear.Disabled = true;
            btnSave.Disabled = true;
          }
        }
        else
        {
          btnClear.Disabled = true;
          btnSave.Disabled = true;
        }
      }
      base.UpdateButtonStates();
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
        return;
      }

      string strPath = item.Path;
      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      playlistPlayer.Reset();
      playlistPlayer.Play(iItem);
      SelectCurrentPlayingSong();
      UpdateButtonStates();
    }

    public override void Process()
    {
      if (!m_strCurrentFile.Equals(g_Player.CurrentFile))
      {
        m_strCurrentFile = g_Player.CurrentFile;
        GUIMessage msg;
        if (g_Player.Playing)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYLIST_CHANGED, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }
        else
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED, GetID, 0, 0, 0, 0, null);
          OnMessage(msg);
        }
      }
    }

    #endregion

    private void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
          //special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC && playlistPlayer.CurrentSong != 0)
          {
            //
          }
          break;
      }
    }

    private void LoadFacade()
    {
      TimeSpan totalPlayingTime = new TimeSpan();
      PlayList pl = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      if (facadeLayout != null)
      {
        facadeLayout.Clear();
      }

      for (int i = 0; i < pl.Count; i++)
      {
        PlayListItem pi = pl[i];
        GUIListItem pItem = new GUIListItem(pi.Description);
        MusicTag tag = (MusicTag)pi.MusicTag;
        bool dirtyTag = false;
        if (tag != null)
        {
          pItem.MusicTag = tag;
          if (tag.Title == ("unknown") || tag.Title.IndexOf("unknown") > 0 || tag.Title == string.Empty)
          {
            dirtyTag = true;
          }
        }
        else
        {
          dirtyTag = true;
        }

        if (tag != null && !dirtyTag)
        {
          int playCount = tag.TimesPlayed;
          string duration = Util.Utils.SecondsToHMSString(tag.Duration);
          pItem.Label = string.Format("{0} - {1}", tag.Artist, tag.Title);
          pItem.Label2 = duration;
          pItem.MusicTag = pi.MusicTag;
        }

        pItem.Path = pi.FileName;
        pItem.IsFolder = false;

        if (pi.Duration > 0)
        {
          totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, pi.Duration));
        }

        if (pi.Played)
        {
          pItem.Shaded = true;
        }

        Util.Utils.SetDefaultIcons(pItem);

        pItem.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        pItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

        facadeLayout.Add(pItem);
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

      SelectCurrentItem();
      UpdateButtonStates();
    }

    private void ClearFileItems()
    {
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
    }

    private void SelectCurrentPlayingSong()
    {
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_MUSIC_PLAYLIST)
        {
          // delete prev. selected item
          for (int i = 0; i < facadeLayout.Count; ++i)
          {
            GUIListItem item = facadeLayout[i];
            if (item != null && item.Selected)
            {
              item.Selected = false;
              break;
            }
          }

          // set current item selected
          int iSong = playlistPlayer.CurrentSong;
          if (iSong >= 0 && iSong <= facadeLayout.Count)
          {
            GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iSong);
            GUIListItem item = facadeLayout[iSong];
            if (item != null)
            {
              item.Selected = true;
            }
          }
        }
      }
    }

    protected void SetLabels()
    {
      if (facadeLayout != null)
      {
        try
        {
          for (int i = 0; i < facadeLayout.Count; ++i)
          {
            GUIListItem item = facadeLayout[i];
            MusicTag tag = null;
            bool dirtyTag = false;
            if (item.MusicTag != null)
            {
              tag = (MusicTag)item.MusicTag;

              if (tag.Title == ("unknown") || tag.Title.IndexOf("unknown") > 0 || tag.Title == string.Empty)
              {
                dirtyTag = true;
              }
            }
            else
            {
              dirtyTag = true;
            }

            if (tag != null && !dirtyTag)
            {
              int playCount = tag.TimesPlayed;
              string duration = Util.Utils.SecondsToHMSString(tag.Duration);
              item.Label = string.Format("{0} - {1}", tag.Artist, tag.Title);
              item.Label2 = duration;
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("GUIMusicPlaylist: exception occured - item without Albumtag? - {0} / {1}", ex.Message,
                    ex.StackTrace);
        }
      }
    }

    protected override bool AllowLayout(Layout layout)
    {
      if (layout == Layout.List)
      {
        return false;
      }
      if (layout == Layout.AlbumView)
      {
        return false;
      }
      if (layout == Layout.Filmstrip)
      {
        return false;
      }
      if (layout == Layout.CoverFlow)
      {
        return false;
      }
      return true;
    }

    private void DoRefreshList()
    {
      if (facadeLayout != null)
      {
        // only focus the file while playlist is visible
        if (GUIWindowManager.ActiveWindow == (int)Window.WINDOW_MUSIC_PLAYLIST)
        {
          LoadFacade();
          SelectCurrentPlayingSong();
        }
      }
    }

    private void playlistPlayer_Changed(PlayListType nPlayList, PlayList playlist)
    {
      // update of playlist control is done by skin engine when moving item up / down
      // but moving the item in the playlist triggers an event
      // we do not want to reload if an item has been moved
      if (!_movingItem)
      {
        DoRefreshList();
      }
    }

    #region modify playlist

    private void ClearPlayList()
    {
      ClearFileItems();
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        playlistPlayer.Reset();

        if (g_Player.Playing && g_Player.IsMusic)
        {
          g_Player.Stop();
        }
      }

      LoadFacade();
      UpdateButtonStates();
      GUIControl.FocusControl(GetID, btnNowPlaying.GetID);
    }

    private void RemovePlayListItem(int iItem)
    {
      GUIListItem pItem = facadeLayout[iItem];
      if (pItem == null)
      {
        return;
      }
      string strFileName = pItem.Path;

      playlistPlayer.Remove(PlayListType.PLAYLIST_MUSIC, strFileName);

      LoadFacade();
      UpdateButtonStates();
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iItem);
      SelectCurrentPlayingSong();
    }

    private void ShufflePlayList()
    {
      ClearFileItems();
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      if (playlist.Count <= 0)
      {
        return;
      }
      string strFileName = string.Empty;
      if (playlistPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
        {
          PlayListItem item = playlist[playlistPlayer.CurrentSong];
          strFileName = item.FileName;
        }
      }
      playlist.Shuffle();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        playlistPlayer.Reset();
      }

      if (strFileName.Length > 0)
      {
        for (int i = 0; i < playlist.Count; i++)
        {
          PlayListItem item = playlist[i];
          if (item.FileName == strFileName)
          {
            // Swap the current playing item with item #0
            // So that the current playing song is always the first song on the first page and we don#t loose any songs
            PlayListItem item0 = playlist[0];
            playlist[0] = item;
            playlist[i] = item0;
            playlistPlayer.CurrentSong = 0;
          }
        }
      }
      LoadFacade();
      SelectCurrentPlayingSong();
    }

    private void SavePlayList()
    {
      string strNewFileName = playlistPlayer.CurrentPlaylistName;
      if (VirtualKeyboard.GetKeyboard(ref strNewFileName, GetID))
      {
        string strPath = Path.GetFileNameWithoutExtension(strNewFileName);
        string strPlayListPath = string.Empty;
        using (Profile.Settings xmlreader = new Profile.MPSettings())
        {
          strPlayListPath = xmlreader.GetValueAsString("music", "playlists", string.Empty);
          strPlayListPath = Util.Utils.RemoveTrailingSlash(strPlayListPath);
        }

        strPath += ".m3u";
        if (strPlayListPath.Length != 0)
        {
          strPath = strPlayListPath + @"\" + strPath;
        }
        PlayList playlist = new PlayList();
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem pItem = facadeLayout[i];
          PlayListItem newItem = new PlayListItem();
          newItem.FileName = pItem.Path;
          newItem.Description = pItem.Label;
          newItem.Duration = pItem.Duration;
          newItem.Type = PlayListItem.PlayListItemType.Audio;
          playlist.Add(newItem);
        }
        IPlayListIO saver = new PlayListM3uIO();
        saver.Save(playlist, strPath);
      }
    }

    private void MovePlayListItemUp()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeLayout.CurrentLayout != GUIFacadeControl.Layout.Playlist
          || facadeLayout.PlayListLayout == null)
      {
        return;
      }

      _movingItem = true;
      int iItem = facadeLayout.SelectedListItemIndex;

      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      playList.MovePlayListItemUp(iItem);
      int selectedIndex = facadeLayout.MoveItemUp(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
      {
        playlistPlayer.CurrentSong = selectedIndex;
      }

      facadeLayout.SelectedListItemIndex = selectedIndex;
      UpdateButtonStates();
      _movingItem = false;
    }

    private void MovePlayListItemDown()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeLayout.CurrentLayout != Layout.Playlist
          || facadeLayout.PlayListLayout == null)
      {
        return;
      }

      _movingItem = true;
      int iItem = facadeLayout.SelectedListItemIndex;
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      playList.MovePlayListItemDown(iItem);
      int selectedIndex = facadeLayout.MoveItemDown(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
      {
        playlistPlayer.CurrentSong = selectedIndex;
      }

      UpdateButtonStates();
      _movingItem = true;
    }

    private void DeletePlayListItem()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeLayout.CurrentLayout != Layout.Playlist
          || facadeLayout.PlayListLayout == null)
      {
        return;
      }

      int iItem = facadeLayout.SelectedListItemIndex;

      string currentFile = g_Player.CurrentFile;
      GUIListItem item = facadeLayout[iItem];
      RemovePlayListItem(iItem);

      if (currentFile.Length > 0 && currentFile == item.Path)
      {
        string nextTrackPath = playlistPlayer.GetNext();

        if (nextTrackPath.Length == 0)
        {
          g_Player.Stop();
        }

        else
        {
          if (iItem == facadeLayout.Count)
          {
            playlistPlayer.Play(iItem - 1);
          }

          else
          {
            playlistPlayer.PlayNext();
          }
        }
      }

      if (facadeLayout.Count == 0)
      {
        g_Player.Stop();
      }

      else
      {
        facadeLayout.PlayListLayout.SelectedListItemIndex = iItem;
      }

      UpdateButtonStates();
    }

    #endregion

    #region background load of default playlist

    private void bw_DoWork(object sender, DoWorkEventArgs e)
    {
      LoadPlayList(Path.Combine(_playlistFolder, _defaultPlaylist), false, true, true);
      if (null != bw && bw.CancellationPending)
        e.Cancel = true;
    }

    private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      defaultPlaylistLoaded = false;
      if (e.Cancelled == true)
      {
        Log.Info("GUIMusicPlaylist: Loading of default playlist cancelled - user changed playlist during loading");
      }
      else if (e.Error != null)
      {
        Log.Info("GUIMusicPlaylist: Error loading default playlist: {0}", e.Error.Message);
      }
      else
      {
        Log.Info("GUIMusicPlaylist: Default Playlist loaded");
        defaultPlaylistLoaded = true;
      }

      if (defaultPlaylistLoaded)
        GUIGraphicsContext.form.Invoke(new ThreadRefreshList(DoRefreshList));

      GUIGraphicsContext.form.Invoke(new ThreadHideWaitCursor(HideWaitCursor));
    }
 
    private void ShowWaitCursor()
    {
      waitCursor = new WaitCursor();
    }

    private void HideWaitCursor()
    {
      if (waitCursor != null)
      {
        waitCursor.Dispose();
        waitCursor = null;
      }
    }

    #endregion
  }
}