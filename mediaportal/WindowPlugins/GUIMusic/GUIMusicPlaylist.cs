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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
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
  /// Class is for GUI interface to music playist
  /// </summary>
  public class GUIMusicPlayList : GUIMusicBaseWindow
  {
    public enum ScrobbleMode
    {
      Similar = 0,
      Neighbours = 1,
      Friends = 2,
      Tags = 3,
      Recent = 4,
      Random = 5,
    }

    // add the beginning artist again to avoid drifting away in style.
    private const int REINSERT_AFTER_THIS_MANY_SONGS = 10;

    #region Variables

    private int m_iItemSelected = -1;
    private int m_iLastControl = 0;
    private string _playlistFolder = string.Empty;
    private string m_strCurrentFile = string.Empty;
    private string _currentScrobbleUser = string.Empty;
    private Song _scrobbleStartTrack;
    private int _totalScrobbledSongs = 0;
    private int _maxScrobbledSongsPerArtist = 1;
    private int _maxScrobbledArtistsForSongs = 4;
    private int _preferCountForTracks = 2;
    private bool ScrobblerOn = false;
    private bool _enableScrobbling = false;
    private bool _useSimilarRandom = true;
    private bool _rememberStartTrack = true;
    private bool _savePlaylistOnExit = false;
    private bool _resumePlaylistOnEnter = false;
    private bool _autoShuffleOnLoad = false;
    private List<string> _scrobbleUsers = new List<string>(1);
    private AudioscrobblerUtils ascrobbler = null;
    private ScrobblerUtilsRequest _lastRequest;
    private string _defaultPlaylist = "default.m3u";
    private WaitCursor waitCursor;
    private static bool _movingItem = false;

    #endregion

    protected delegate void ThreadRefreshList();
    protected delegate void ThreadHideWaitCursor();

    private ScrobbleMode currentScrobbleMode = ScrobbleMode.Similar;
    private offlineMode currentOfflineMode = offlineMode.random;

    [SkinControl(20)] protected GUIButtonControl btnShuffle = null;
    [SkinControl(21)] protected GUIButtonControl btnSave = null;
    [SkinControl(22)] protected GUIButtonControl btnClear = null;
    [SkinControl(26)] protected GUIButtonControl btnNowPlaying = null;
    [SkinControl(27)] protected GUIToggleButtonControl btnScrobble = null;
    [SkinControl(28)] protected GUIButtonControl btnScrobbleMode = null;
    [SkinControl(29)] protected GUIButtonControl btnScrobbleUser = null;
    [SkinControl(30)] protected GUIToggleButtonControl btnRepeatPlaylist = null;


    public GUIMusicPlayList()
    {
      GetID = (int)Window.WINDOW_MUSIC_PLAYLIST;
    }

    ~GUIMusicPlayList()
    {
    }

    #region overrides

    public override bool Init()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _enableScrobbling = MediaPortal.GUI.Library.PluginManager.IsPluginNameEnabled("Audioscrobbler");
        _currentScrobbleUser = xmlreader.GetValueAsString("audioscrobbler", "user", "Username");
        _useSimilarRandom = xmlreader.GetValueAsBool("audioscrobbler", "usesimilarrandom", true); _playlistFolder = xmlreader.GetValueAsString("music", "playlists", "");
        _savePlaylistOnExit = xmlreader.GetValueAsBool("musicfiles", "savePlaylistOnExit", false);
        _resumePlaylistOnEnter = xmlreader.GetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", false);
        _autoShuffleOnLoad = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", false);
        playlistPlayer.RepeatPlaylist = xmlreader.GetValueAsBool("musicfiles", "repeat", true);
      }

      if (_resumePlaylistOnEnter)
      {
        Log.Info("GUIMusicPlaylist: Loading default playlist {0}", _defaultPlaylist);
        //LoadPlayList(Path.Combine(_playlistFolder, _defaultPlaylist), false);
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

      _scrobbleUsers = m_database.GetAllScrobbleUsers();
      // no users in database
      if (_scrobbleUsers.Count > 0 && _enableScrobbling)
      {
        LoadScrobbleUserSettings();
      }

      ascrobbler = AudioscrobblerUtils.Instance;
      //      ScrobbleLock = new object();
      //added by Sam
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      GUIWindowManager.OnNewAction += new OnActionHandler(this.OnNewAction);
      playlistPlayer.PlaylistChanged += playlistPlayer_Changed; 
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\myMusicplaylist.xml"));
    }

    public override void DeInit()
    {
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnThreadMessage);
      GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnNewAction);

      if (_lastRequest != null)
      {
        ascrobbler.RemoveRequest(_lastRequest);
      }

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
              ClearScrobbleStartTrack();
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

      if (ScrobblerOn)
      {
        btnScrobble.Selected = true;
      }

      if (_scrobbleUsers.Count < 2)
      {
        btnScrobbleUser.Visible = false;
      }

      btnScrobbleUser.Label = GUILocalizeStrings.Get(33005) + _currentScrobbleUser;

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

      if (control == btnScrobbleUser)
      {
        // no users in database
        if (_scrobbleUsers.Count == 0)
        {
          return;
        }
        //for (int i = 0; i < scrobbleusers.Count; i++)
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(GUILocalizeStrings.Get(497)); //Menu
          int selected = 0;
          int count = 0;
          foreach (string scrobbler in _scrobbleUsers)
          {
            dlg.Add(scrobbler);
            if (scrobbler == _currentScrobbleUser)
            {
              selected = count;
            }
            count++;
          }
          dlg.SelectedLabel = selected;
        }
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }

        if (_currentScrobbleUser != dlg.SelectedLabelText)
        {
          _currentScrobbleUser = dlg.SelectedLabelText;
          btnScrobbleUser.Label = GUILocalizeStrings.Get(33005) + _currentScrobbleUser;

          AudioscrobblerBase.DoChangeUser(_currentScrobbleUser,
                                          m_database.AddScrobbleUserPassword(
                                            Convert.ToString(m_database.AddScrobbleUser(_currentScrobbleUser)), ""));
          LoadScrobbleUserSettings();
          UpdateButtonStates();
        }

        GUIControl.FocusControl(GetID, controlId);
        return;
      } //if (control == btnScrobbleUser)


      if (control == btnScrobbleMode)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
          if (dlg != null)
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(33010)); // Automatically fill playlist with

            dlg.Add(GUILocalizeStrings.Get(33011)); // similar tracks
            dlg.Add(GUILocalizeStrings.Get(33017)); // random tracks
            if (_enableScrobbling)
            {
              dlg.Add(GUILocalizeStrings.Get(33012)); // tracks your neighbours like
              dlg.Add(GUILocalizeStrings.Get(33016)); // tracks your friends like
              //dlg.Add(GUILocalizeStrings.Get(33014)); // tracks played recently
              //dlg.Add(GUILocalizeStrings.Get(33013)); // tracks suiting configured tag {0}
            }

            dlg.DoModal(GetID);
            if (dlg.SelectedLabel < 0)
            {
              return;
            }

            switch (dlg.SelectedId)
            {
              case 1:
                currentScrobbleMode = ScrobbleMode.Similar;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33001);
                break;
              case 2:
                currentScrobbleMode = ScrobbleMode.Random;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33007);
                break;
              case 3:
                currentScrobbleMode = ScrobbleMode.Neighbours;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33002);
                break;
              case 4:
                currentScrobbleMode = ScrobbleMode.Friends;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33006);
                break;
              case 5:
                currentScrobbleMode = ScrobbleMode.Recent;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33004);
                break;
              case 6:
                currentScrobbleMode = ScrobbleMode.Tags;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33003);
                break;
              default:
                currentScrobbleMode = ScrobbleMode.Random;
                btnScrobbleMode.Label = GUILocalizeStrings.Get(33007);
                break;
            }

            if (currentScrobbleMode == ScrobbleMode.Random)
            {
              if (currentOfflineMode == offlineMode.favorites)
              {
                MusicDatabase checkdb = MusicDatabase.Instance;
                if (checkdb.GetTotalFavorites() <= _maxScrobbledArtistsForSongs * 2)
                {
                  shouldContinue = true;
                  Log.Warn(
                    "Audioscrobbler playlist: Cannot activate offline mode: favorites because there are not enough tracks");
                }
              }
            }
          }
        } while (shouldContinue);

        CheckScrobbleInstantStart();
        GUIControl.FocusControl(GetID, controlId);
        return;
      } //if (control == btnScrobbleMode)

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
      else if (control == btnScrobble)
      {
        //get state of button
        if (btnScrobble.Selected)
        {
          ScrobblerOn = true;
          CheckScrobbleInstantStart();
        }
        else
        {
          ScrobblerOn = false;
        }

        if (facadeLayout.PlayListLayout != null)
        {
          UpdateButtonStates();
        }
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
            //            btnPlay.Disabled = false;
            btnSave.Disabled = false;

            if (ScrobblerOn)
            {
              btnScrobble.Selected = true;
            }
            else
            {
              btnScrobble.Selected = false;
            }
          }
          else
          {
            btnClear.Disabled = true;
            //            btnPlay.Disabled = true;
            btnSave.Disabled = true;
          }
        }
        else
        {
          btnClear.Disabled = true;
          //          btnPlay.Disabled = true;
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
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          ClearScrobbleStartTrack();
          break;

          //special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC && playlistPlayer.CurrentSong != 0)
          {
            //
          }
          break;
          // delaying internet lookups for smooth playback start
        case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC:
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC && ScrobblerOn && _enableScrobbling)
            // && playlistPlayer.CurrentSong != 0)
          {
            DoScrobbleLookups();
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

    #region modify playlist

    private void ClearPlayList()
    {
      ClearFileItems();
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        playlistPlayer.Reset();
        ClearScrobbleStartTrack();

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
      if (GetKeyboard(ref strNewFileName))
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

    #region scrobbling methods

    private void LoadScrobbleUserSettings()
    {
      string currentUID = Convert.ToString(m_database.AddScrobbleUser(_currentScrobbleUser));
      ScrobblerOn = (m_database.AddScrobbleUserSettings(currentUID, "iScrobbleDefault", -1) == 1) ? true : false;
      _maxScrobbledArtistsForSongs = m_database.AddScrobbleUserSettings(currentUID, "iAddArtists", -1);
      _maxScrobbledSongsPerArtist = m_database.AddScrobbleUserSettings(currentUID, "iAddTracks", -1);
      _preferCountForTracks = m_database.AddScrobbleUserSettings(currentUID, "iPreferCount", -1);
      _rememberStartTrack = (m_database.AddScrobbleUserSettings(currentUID, "iRememberStartArtist", -1) == 1)
                              ? true
                              : false;
      _maxScrobbledArtistsForSongs = (_maxScrobbledArtistsForSongs > 0) ? _maxScrobbledArtistsForSongs : 3;
      _maxScrobbledSongsPerArtist = (_maxScrobbledSongsPerArtist > 0) ? _maxScrobbledSongsPerArtist : 1;
      int tmpRMode = m_database.AddScrobbleUserSettings(currentUID, "iOfflineMode", -1);

      switch (tmpRMode)
      {
        case 0:
          currentOfflineMode = offlineMode.random;
          break;
        case 1:
          currentOfflineMode = offlineMode.timesplayed;
          break;
        case 2:
          currentOfflineMode = offlineMode.favorites;
          break;
        default:
          currentOfflineMode = offlineMode.random;
          break;
      }
      Log.Info("GUIMusicPlayList: Scrobblesettings loaded for {0} - dynamic playlist inserts: {1}", _currentScrobbleUser,
               Convert.ToString(ScrobblerOn));
    }

    private void ClearScrobbleStartTrack()
    {
      if (_rememberStartTrack && _scrobbleStartTrack != null)
      {
        _scrobbleStartTrack = null;
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

    private void CheckScrobbleInstantStart()
    {
      if (ScrobblerOn)
      {
        PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

        if (playList != null)
        {
          // if scrobbling gets activated after 10 sec event nothing would happen without this
          if (playList.Count == 1 && g_Player.CurrentPosition > 10)
          {
            DoScrobbleLookups();
          }
        }
        if (playList.Count == 0)
        {
          if (currentScrobbleMode != ScrobbleMode.Similar)
          {
            DoScrobbleLookups();
          }
        }
      }
    }

    private void UpdateSimilarArtists(string _trackArtist)
    {
      if (_trackArtist == null)
      {
        return;
      }
      if (_trackArtist != string.Empty)
      {
        SimilarArtistRequest request2 = new SimilarArtistRequest(
          _trackArtist,
          _useSimilarRandom,
          new SimilarArtistRequest.SimilarArtistRequestHandler(OnUpdateSimilarArtistsCompleted));
        _lastRequest = request2;
        ascrobbler.AddRequest(request2);
      }
    }

    private void UpdateNeighboursArtists(bool randomizeList)
    {
      NeighboursArtistsRequest request = new NeighboursArtistsRequest(
        randomizeList,
        new NeighboursArtistsRequest.NeighboursArtistsRequestHandler(OnUpdateNeighboursArtistsCompleted));
      _lastRequest = request;
      ascrobbler.AddRequest(request);
    }

    private void UpdateFriendsArtists(bool randomizeList)
    {
      FriendsArtistsRequest request = new FriendsArtistsRequest(
        randomizeList,
        new FriendsArtistsRequest.FriendsArtistsRequestHandler(OnUpdateFriendsArtistsCompleted));
      _lastRequest = request;
      ascrobbler.AddRequest(request);
    }

    private void UpdateRandomTracks()
    {
      RandomTracksRequest request = new RandomTracksRequest(
        new RandomTracksRequest.RandomTracksRequestHandler(OnUpdateRandomTracksCompleted));
      _lastRequest = request;
      ascrobbler.AddRequest(request);
    }

    private void UpdateUnheardTracks()
    {
      UnheardTracksRequest request = new UnheardTracksRequest(
        new UnheardTracksRequest.UnheardTracksRequestHandler(OnUpdateUnheardTracksCompleted));
      _lastRequest = request;
      ascrobbler.AddRequest(request);
    }

    private void UpdateFavoriteTracks()
    {
      FavoriteTracksRequest request = new FavoriteTracksRequest(
        new FavoriteTracksRequest.FavoriteTracksRequestHandler(OnUpdateFavoriteTracksCompleted));
      _lastRequest = request;
      ascrobbler.AddRequest(request);
    }

    private void DoScrobbleLookups()
    {
      PlayList currentPlaylist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      MusicDatabase dbs = MusicDatabase.Instance;
      Song current10SekSong = new Song();
      List<Song> scrobbledArtists = new List<Song>();

      ascrobbler.RemoveRequest(_lastRequest);
      switch (currentScrobbleMode)
      {
        case ScrobbleMode.Similar:
          if (_rememberStartTrack && _scrobbleStartTrack != null)
          {
            if (_totalScrobbledSongs > REINSERT_AFTER_THIS_MANY_SONGS)
            {
              _totalScrobbledSongs = 0;
              Song tmpArtist = new Song();
              tmpArtist = _scrobbleStartTrack;
              scrobbledArtists.Add(tmpArtist);
              break;
            }
          }
          string strFile = string.Empty;
          if (g_Player.Player.CurrentFile != null && g_Player.Player.CurrentFile != string.Empty && g_Player.IsMusic)
          {
            strFile = g_Player.Player.CurrentFile;

            bool songFound = dbs.GetSongByFileName(strFile, ref current10SekSong);
            if (songFound)
            {
              if (_scrobbleStartTrack == null || _scrobbleStartTrack.Artist == string.Empty)
              {
                _scrobbleStartTrack = current10SekSong.Clone();
              }

              try
              {
                UpdateSimilarArtists(current10SekSong.Artist);
                //scrobbledArtists = ascrobbler.getSimilarArtists(current10SekSong.ToURLArtistString(), _useSimilarRandom);
                return;
              }
              catch (Exception ex)
              {
                Log.Error("ScrobbleLookupThread: exception on lookup Similar - {0}", ex.Message);
              }
            }
          }
          break;

        case ScrobbleMode.Neighbours:
          //lock (ScrobbleLock)
          //{
          try
          {
            UpdateNeighboursArtists(true);
          }
          catch (Exception ex)
          {
            Log.Error("ScrobbleLookupThread: exception on lookup Neighbourhood - {0}", ex.Message);
          }
          //}
          break;

        case ScrobbleMode.Friends:
          //lock (ScrobbleLock)
          //{
          try
          {
            UpdateFriendsArtists(true);
          }
          catch (Exception ex)
          {
            Log.Error("ScrobbleLookupThread: exception on lookup - Friends {0}", ex.Message);
          }
          //}
          break;
        case ScrobbleMode.Random:
          try
          {
            switch (currentOfflineMode)
            {
              case offlineMode.random:
                UpdateRandomTracks();
                break;
              case offlineMode.timesplayed:
                UpdateUnheardTracks();
                break;
              case offlineMode.favorites:
                UpdateFavoriteTracks();
                break;
              default:
                UpdateRandomTracks();
                break;
            }
          }
          catch (Exception ex)
          {
            Log.Error("ScrobbleLookupThread: exception on lookup - Random {0}", ex.Message);
          }
          break;
      }

      OnScrobbleLookupsCompleted(scrobbledArtists);
    }

    public void OnUpdateSimilarArtistsCompleted(SimilarArtistRequest request2, List<Song> SimilarArtists)
    {
      if (request2.Equals(_lastRequest))
      {
        OnScrobbleLookupsCompleted(SimilarArtists);
      }
      else
      {
        Log.Warn("GUIMusicPlaylist: OnUpdateSimilarArtistsCompleted: unexpected response for request: {0}",
                 request2.Type);
      }
    }

    public void OnUpdateNeighboursArtistsCompleted(NeighboursArtistsRequest request, List<Song> NeighboursArtists)
    {
      if (request.Equals(_lastRequest))
      {
        OnScrobbleLookupsCompleted(NeighboursArtists);
      }
      else
      {
        Log.Warn("GUIMusicPlaylist: OnUpdateNeighboursArtistsCompleted: unexpected response for request: {0}",
                 request.Type);
      }
    }

    public void OnUpdateFriendsArtistsCompleted(FriendsArtistsRequest request, List<Song> FriendsArtists)
    {
      if (request.Equals(_lastRequest))
      {
        OnScrobbleLookupsCompleted(FriendsArtists);
      }
      else
      {
        Log.Warn("GUIMusicPlaylist: OnUpdateFriendsArtistsCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateRandomTracksCompleted(RandomTracksRequest request, List<Song> RandomTracks)
    {
      if (request.Equals(_lastRequest))
      {
        OnScrobbleLookupsCompleted(RandomTracks);
      }
      else
      {
        Log.Warn("GUIMusicPlaylist: OnUpdateRandomTracksCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateUnheardTracksCompleted(UnheardTracksRequest request, List<Song> UnheardTracks)
    {
      if (request.Equals(_lastRequest))
      {
        OnScrobbleLookupsCompleted(UnheardTracks);
      }
      else
      {
        Log.Warn("GUIMusicPlaylist: OnUpdateRandomTracksCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateFavoriteTracksCompleted(FavoriteTracksRequest request, List<Song> FavoriteTracks)
    {
      if (request.Equals(_lastRequest))
      {
        OnScrobbleLookupsCompleted(FavoriteTracks);
      }
      else
      {
        Log.Warn("GUIMusicPlaylist: OnUpdateRandomTracksCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    private void OnScrobbleLookupsCompleted(List<Song> LookupArtists)
    {
      Log.Debug("GUIMusicPlaylist: OnScrobbleLookupsCompleted - processing {0} results",
                Convert.ToString(LookupArtists.Count));

      if (LookupArtists.Count < _maxScrobbledArtistsForSongs)
      {
        if (LookupArtists.Count > 0)
        {
          for (int i = 0; i < LookupArtists.Count; i++)
          {
            ScrobbleSimilarArtists(LookupArtists[i].Artist);
          }
        }
      }
      else // enough artists
      {
        int addedSimilarSongs = 0;
        int loops = 0;
        int previouspreferCount = _preferCountForTracks;
        // we WANT to get songs from _maxScrobbledArtistsForSongs
        while (addedSimilarSongs < _maxScrobbledArtistsForSongs)
        {
          if (ScrobbleSimilarArtists(LookupArtists[loops].Artist))
          {
            addedSimilarSongs++;
          }
          loops++;
          // okay okay seems like there aren't enough files to add
          if (loops == LookupArtists.Count - 1)
          {
            break;
          }
        }
        _preferCountForTracks = previouspreferCount;
      }

      GUIGraphicsContext.form.Invoke(new ThreadRefreshList(DoRefreshList));
    }

    private bool ScrobbleSimilarArtists(string Artist_)
    {
      MusicDatabase dbs = MusicDatabase.Instance;
      PlayList list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      List<Song> songList = new List<Song>();
      double avgPlayCount = 0;
      int songsAdded = 0;
      int j = 0;

      dbs.GetSongsByArtist(Artist_, ref songList);

      //      Log.Debug("GUIMusicPlaylist: ScrobbleSimilarArtists found {0} songs allowed to add", Convert.ToString(songList.Count));

      // exit if not enough songs were found
      if (songList.Count < _maxScrobbledSongsPerArtist)
      {
        return false;
      }

      // lookup how many times this artist's songs were played
      avgPlayCount = dbs.GetAveragePlayCountForArtist(Artist_);

      switch (_preferCountForTracks)
      {
        case 0:
          // delete all heard songs
          for (int s = 0; s < songList.Count; s++)
          {
            if (songList[s].TimesPlayed > 0)
            {
              songList.Remove(songList[s]);
              s--;
            }
          }
          break;
        case 1:
          // delete all well known songs
          if (avgPlayCount < 0.5)
          {
            goto case 0;
          }
          else
          {
            for (int s = 0; s < songList.Count; s++)
            {
              // song was played more often than average
              if (songList[s].TimesPlayed > avgPlayCount)
              {
                // give 1x played songs a chance...
                if (songList[s].TimesPlayed > 1)
                {
                  songList.Remove(songList[s]);
                }
              }
            }
          }
          break;
        case 2:
          break;
        case 3:
          // only well known songs
          for (int s = 0; s < songList.Count; s++)
          {
            // delete all rarely heard songs
            if (songList[s].TimesPlayed < avgPlayCount)
            {
              songList.Remove(songList[s]);
              s--;
            }
          }

          // get new average playcount of remaining files
          if (songList.Count > 0)
          {
            int avgOfKnownSongs = 0;
            foreach (Song favSong in songList)
            {
              avgOfKnownSongs += favSong.TimesPlayed;
            }
            avgOfKnownSongs /= songList.Count;
            avgOfKnownSongs = avgOfKnownSongs > 0 ? avgOfKnownSongs : 2;

            int songListCount = songList.Count;
            for (int s = 0; s < songListCount; s++)
            {
              if (songList[s].TimesPlayed < avgOfKnownSongs)
              {
                songList.Remove(songList[s]);
                songListCount = songList.Count;
                s--;
              }
            }
          }
          //songList.Sort(CompareSongsByTimesPlayed);
          break;
      }

      // check if there are still enough songs
      if (songList.Count < _maxScrobbledSongsPerArtist)
      {
        return false;
      }

      PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();

      int randomPosition;

      while (songsAdded < _maxScrobbledSongsPerArtist)
      {
        if (_preferCountForTracks == 3)
        {
          randomPosition = rand.Next(0, songList.Count / 2);
        }
        else
        {
          randomPosition = rand.Next(0, songList.Count - 1);
        }

        Song refSong = new Song();

        refSong = songList[randomPosition];

        //        Log.Debug("GUIMusicPlaylist: ScrobbleSimilarArtists tries to add this song - {0}", refSong.ToShortString());

        if (AddRandomSongToPlaylist(ref refSong))
        {
          songsAdded++;
          _totalScrobbledSongs++;
        }

        j++;

        if (j > songList.Count * 5)
        {
          break;
        }
      }
      // _maxScrobbledSongsPerArtist are inserted
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

    private bool AddRandomSongToPlaylist(ref Song song)
    {
      //check duplication
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      for (int i = 0; i < playlist.Count; i++)
      {
        PlayListItem item = playlist[i];
        if (item.FileName == song.FileName)
        {
          return false;
        }
      }

      //add to playlist
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = PlayListItem.PlayListItemType.Audio;
      StringBuilder sb = new StringBuilder();

      playlistItem.FileName = song.FileName;
      sb.Append(song.Track);
      sb.Append(". ");
      sb.Append(song.Artist);
      sb.Append(" - ");
      sb.Append(song.Title);
      playlistItem.Description = sb.ToString();
      playlistItem.Duration = song.Duration;

      MusicTag tag = new MusicTag();
      tag = song.ToMusicTag();

      playlistItem.MusicTag = tag;

      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);
      return true;
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