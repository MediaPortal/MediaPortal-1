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
using System.Collections.Generic;
using System.IO;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
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

    private static int CompareSongsByTimesPlayed(Song x, Song y)
    {
      // ...and y is not null, compare 
      int retval = 0;
      try
      {
        if (x.TimesPlayed == 0)
        {
          return 0;
        }

        if (y.TimesPlayed == 0)
        {
          return 0;
        }

        if (x.TimesPlayed == y.TimesPlayed)
        {
          return 0;
        }
        else if (x.TimesPlayed < y.TimesPlayed)
        {
          retval = 1;
        }
        else
        {
          retval = -1;
        }

        if (retval != 0)
        {
          return retval;
        }
        else
        {
          return 0;
        }
      }

      catch (Exception)
      {
        return 0;
      }
    }

    // add the beginning artist again to avoid drifting away in style.
    private const int REINSERT_AFTER_THIS_MANY_SONGS = 10;

    #region Variables

    private MusicDatabase mdb = null;
    private DirectoryHistory m_history = new DirectoryHistory();
    private string m_strDirectory = string.Empty;
    private int m_iItemSelected = -1;
    private int m_iLastControl = 0;
    private int m_nTempPlayListWindow = 0;
    private string m_strTempPlayListDirectory = string.Empty;
    private string m_strCurrentFile = string.Empty;
    private string _currentScrobbleUser = string.Empty;
    private Song _scrobbleStartTrack;
    private VirtualDirectory _virtualDirectory = new VirtualDirectory();
    //const int MaxNumPShuffleSongPredict = 12;
    private int _totalScrobbledSongs = 0;
    private int _maxScrobbledSongsPerArtist = 1;
    private int _maxScrobbledArtistsForSongs = 4;
    private int _preferCountForTracks = 2;
    private bool ScrobblerOn = false;
    private bool _enableScrobbling = false;
    private bool _useSimilarRandom = true;
    private bool _rememberStartTrack = true;
    private List<string> _scrobbleUsers = new List<string>(1);
    private AudioscrobblerUtils ascrobbler = null;
    private ScrobblerUtilsRequest _lastRequest;

    #endregion

    protected delegate void ThreadRefreshList();

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
      GetID = (int) Window.WINDOW_MUSIC_PLAYLIST;

      _virtualDirectory.AddDrives();
      _virtualDirectory.SetExtensions(Util.Utils.AudioExtensions);
    }

    private void LoadScrobbleUserSettings()
    {
      string currentUID = Convert.ToString(mdb.AddScrobbleUser(_currentScrobbleUser));
      ScrobblerOn = (mdb.AddScrobbleUserSettings(currentUID, "iScrobbleDefault", -1) == 1) ? true : false;
      _maxScrobbledArtistsForSongs = mdb.AddScrobbleUserSettings(currentUID, "iAddArtists", -1);
      _maxScrobbledSongsPerArtist = mdb.AddScrobbleUserSettings(currentUID, "iAddTracks", -1);
      _preferCountForTracks = mdb.AddScrobbleUserSettings(currentUID, "iPreferCount", -1);
      _rememberStartTrack = (mdb.AddScrobbleUserSettings(currentUID, "iRememberStartArtist", -1) == 1) ? true : false;
      _maxScrobbledArtistsForSongs = (_maxScrobbledArtistsForSongs > 0) ? _maxScrobbledArtistsForSongs : 3;
      _maxScrobbledSongsPerArtist = (_maxScrobbledSongsPerArtist > 0) ? _maxScrobbledSongsPerArtist : 1;
      int tmpRMode = mdb.AddScrobbleUserSettings(currentUID, "iOfflineMode", -1);

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

    #region overrides

    public override bool Init()
    {
      mdb = MusicDatabase.Instance;
      m_strDirectory = Directory.GetCurrentDirectory();

      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _enableScrobbling = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
        _currentScrobbleUser = xmlreader.GetValueAsString("audioscrobbler", "user", "Username");
        _useSimilarRandom = xmlreader.GetValueAsBool("audioscrobbler", "usesimilarrandom", true);
      }

      _scrobbleUsers = mdb.GetAllScrobbleUsers();
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
      return Load(GUIGraphicsContext.Skin + @"\myMusicplaylist.xml");
    }

    public override void DeInit()
    {
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnThreadMessage);
      GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnNewAction);

      if (_lastRequest != null)
      {
        ascrobbler.RemoveRequest(_lastRequest);
      }

      base.DeInit();
    }

    protected override string SerializeName
    {
      get { return "mymusicplaylist"; }
    }

    protected override bool AllowView(View view)
    {
      if (view == View.List)
      {
        return false;
      }
      if (view == View.Albums)
      {
        return false;
      }
      if (view == View.FilmStrip)
      {
        return false;
      }
      return true;
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
            if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC)
            {
              playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
            }

            playlistPlayer.Play(facadeView.SelectedListItemIndex);
            bool didJump = DoPlayNowJumpTo(facadeView.Count);
            Log.Debug("GUIMusicPlaylist: Doing play now jump to: {0} ({1})", PlayNowJumpTo, didJump);
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
      currentView = View.PlayList;
      facadeView.View = GUIFacadeControl.ViewMode.Playlist;

      if (ScrobblerOn)
      {
        btnScrobble.Selected = true;
      }

      if (_scrobbleUsers.Count < 2)
      {
        btnScrobbleUser.Visible = false;
      }

      btnScrobbleUser.Label = GUILocalizeStrings.Get(33005) + _currentScrobbleUser;

      LoadDirectory(string.Empty);
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
      }
      if ((m_iLastControl == facadeView.GetID) && facadeView.Count <= 0)
      {
        m_iLastControl = btnNowPlaying.GetID;
        GUIControl.FocusControl(GetID, m_iLastControl);
      }
      if (facadeView.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnViewAs.GetID);
      }

      using (Profile.Settings settings = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        playlistPlayer.RepeatPlaylist = settings.GetValueAsBool("musicfiles", "repeat", true);
      }

      if (btnRepeatPlaylist != null)
      {
        btnRepeatPlaylist.Selected = playlistPlayer.RepeatPlaylist;
      }

      SelectCurrentPlayingSong();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iItemSelected = facadeView.SelectedListItemIndex;
      using (Profile.Settings settings = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        settings.SetValueAsBool("musicfiles", "repeat", playlistPlayer.RepeatPlaylist);
      }
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
        GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
                                          mdb.AddScrobbleUserPassword(
                                            Convert.ToString(mdb.AddScrobbleUser(_currentScrobbleUser)), ""));
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
          GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
                if (checkdb.GetTotalFavorites() <= _maxScrobbledArtistsForSongs*2)
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
        //else if (control == btnPlay)
        //{
        //  playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        //  playlistPlayer.Reset();
        //  playlistPlayer.Play(facadeView.SelectedListItemIndex);

        //  UpdateButtonStates();
        //}

      else if (control == btnScrobble)
      {
        //if (_enableScrobbling)
        //{
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

        if (facadeView.PlayListView != null)
        {
          //{
          //  // Prevent the currently playing track from being scrolled off the top 
          //  // or bottom of the screen when other items are re-ordered
          //  facadeView.PlayListView.AllowLastVisibleListItemDown = !ScrobblerOn;
          //  facadeView.PlayListView.AllowMoveFirstVisibleListItemUp = !ScrobblerOn;
          //}
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
            for (int i = 0; i < facadeView.Count; ++i)
            {
              GUIListItem item = facadeView[i];
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
            //	global playlist changed outside playlist window
            //added by Sam
            ////if party shuffle...
            //if (PShuffleOn)// || ScrobblerOn)
            //{
            //  LoadDirectory(string.Empty);
            //  UpdateButtonStates();
            //}
            //ended changes

            if (m_iLastControl == facadeView.GetID && facadeView.Count <= 0)
            {
              if (GUIWindowManager.ActiveWindow == (int) Window.WINDOW_MUSIC_PLAYLIST)
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
      if (GUIWindowManager.ActiveWindow == (int) Window.WINDOW_MUSIC_PLAYLIST)
      {
        if (facadeView != null)
        {
          if (facadeView.Count > 0)
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
        //PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

        //if (playList != null && playList.Count > 0)
        //  btnSave.Disabled = false;
        //else
        //  btnSave.Disabled = true;
      }
      base.UpdateButtonStates();
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
        return;
      }

      string strPath = item.Path;
      playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      playlistPlayer.Reset();
      playlistPlayer.Play(iItem);
      SelectCurrentPlayingSong();
      UpdateButtonStates();
    }

    protected override void OnQueueItem(int iItem)
    {
      RemovePlayListItem(iItem);
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

    private void OnRetrieveMusicInfo(ref List<GUIListItem> items)
    {
      if (items.Count <= 0)
      {
        return;
      }
      MusicDatabase dbs = MusicDatabase.Instance;
      Song song = new Song();
      foreach (GUIListItem item in items)
      {
        if (item.MusicTag == null)
        {
          if (dbs.GetSongByFileName(item.Path, ref song))
          {
            MusicTag tag = new MusicTag();
            tag = song.ToMusicTag();
            item.MusicTag = tag;
          }
          else if (UseID3)
          {
            item.MusicTag = TagReader.TagReader.ReadTag(item.Path);
          }
        }
      }
    }

    protected override void LoadDirectory(string strNewDirectory)
    {
      if (facadeView != null)
      {
        GUIWaitCursor.Show();
        try
        {
          TimeSpan totalPlayingTime = new TimeSpan();
          GUIListItem SelectedItem = facadeView.SelectedListItem;
          if (SelectedItem != null)
          {
            if (SelectedItem.IsFolder && SelectedItem.Label != "..")
            {
              m_history.Set(SelectedItem.Label, m_strDirectory);
            }
          }
          m_strDirectory = strNewDirectory;
          GUIControl.ClearControl(GetID, facadeView.GetID);

          List<GUIListItem> itemlist = new List<GUIListItem>();

          PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
          /* copy playlist from general playlist*/
          int iCurrentSong = -1;
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
          {
            iCurrentSong = playlistPlayer.CurrentSong;
          }

          string strFileName;
          for (int i = 0; i < playlist.Count; ++i)
          {
            PlayListItem item = playlist[i];
            strFileName = item.FileName;

            GUIListItem pItem = new GUIListItem(item.Description);
            pItem.Path = strFileName;
            pItem.MusicTag = item.MusicTag;
            pItem.IsFolder = false;
            //pItem.m_bIsShareOrDrive = false;

            Util.Utils.SetDefaultIcons(pItem);
            if (item.Played)
            {
              pItem.Shaded = true;
            }

            if (item.Duration > 0)
            {
              int nDuration = item.Duration;
              if (nDuration > 0)
              {
                string str = Util.Utils.SecondsToHMSString(nDuration);
                pItem.Label2 = str;
              }
              else
              {
                pItem.Label2 = string.Empty;
              }
            }
            pItem.OnRetrieveArt += new GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            itemlist.Add(pItem);
          }
          OnRetrieveMusicInfo(ref itemlist);
          iCurrentSong = 0;
          strFileName = string.Empty;
          //	Search current playlist item
          if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(m_strDirectory) >= 0 &&
               g_Player.Playing
               && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
              ||
              (GUIWindowManager.ActiveWindow == (int) Window.WINDOW_MUSIC_PLAYLIST &&
               playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC
               && g_Player.Playing))
          {
            iCurrentSong = playlistPlayer.CurrentSong;
            if (iCurrentSong >= 0)
            {
              playlist = playlistPlayer.GetPlaylist(playlistPlayer.CurrentPlaylistType);
              if (iCurrentSong < playlist.Count)
              {
                PlayListItem item = playlist[iCurrentSong];
                strFileName = item.FileName;
              }
            }
          }

          string strSelectedItem = m_history.Get(m_strDirectory);
          int iItem = 0;
          foreach (GUIListItem item in itemlist)
          {
            MusicTag tag = item.MusicTag as MusicTag;
            if (tag != null)
            {
              if (tag.Duration > 0)
              {
                totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
              }
            }
            facadeView.Add(item);
            //	synchronize playlist with current directory
            if (strFileName.Length > 0 && item.Path == strFileName)
            {
              item.Selected = true;
            }
          }
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
          if (totalPlayingTime.TotalSeconds > 0)
          {
            GUIPropertyManager.SetProperty("#itemcount",
                                           Util.Utils.GetSongCountLabel(iTotalItems, (int) totalPlayingTime.TotalSeconds));
          }
          else
          {
            GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));
          }

          SetLabels();
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
          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];
            if (item.Path.Equals(_currentPlaying, StringComparison.OrdinalIgnoreCase))
            {
              item.Selected = true;
              break;
            }
          }
          UpdateButtonStates();
          GUIWaitCursor.Hide();
        }
        catch (Exception ex)
        {
          GUIWaitCursor.Hide();
          Log.Error("GUIMusicPlaylist: An error occured while loading the directory - {0}", ex.Message);
        }
      }
    }

    private void ClearScrobbleStartTrack()
    {
      if (_rememberStartTrack && _scrobbleStartTrack != null)
      {
        _scrobbleStartTrack = null;
      }
    }

    private void ClearFileItems()
    {
      GUIControl.ClearControl(GetID, facadeView.GetID);
    }

    private void ClearPlayList()
    {
      ClearFileItems();
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        playlistPlayer.Reset();
      }
      ClearScrobbleStartTrack();
      LoadDirectory(string.Empty);
      UpdateButtonStates();
      GUIControl.FocusControl(GetID, btnNowPlaying.GetID);
    }

    private void RemovePlayListItem(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];
      if (pItem == null)
      {
        return;
      }
      string strFileName = pItem.Path;

      playlistPlayer.Remove(PlayListType.PLAYLIST_MUSIC, strFileName);

      LoadDirectory(m_strDirectory);
      UpdateButtonStates();
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
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
      LoadDirectory(m_strDirectory);
      SelectCurrentPlayingSong();
    }

    private void SavePlayList()
    {
      string strNewFileName = string.Empty;
      if (GetKeyboard(ref strNewFileName))
      {
        string strPath = Path.GetFileNameWithoutExtension(strNewFileName);
        string strPlayListPath = string.Empty;
        using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
        for (int i = 0; i < facadeView.Count; ++i)
        {
          GUIListItem pItem = facadeView[i];
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

    private void SelectCurrentPlayingSong()
    {
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        if (GUIWindowManager.ActiveWindow == (int) Window.WINDOW_MUSIC_PLAYLIST)
        {
          // delete prev. selected item
          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];
            if (item != null && item.Selected)
            {
              item.Selected = false;
              break;
            }
          }

          // set current item selected
          int iSong = playlistPlayer.CurrentSong;
          if (iSong >= 0 && iSong <= facadeView.Count)
          {
            GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
            GUIListItem item = facadeView[iSong];
            if (item != null)
            {
              item.Selected = true;
            }
          }
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

    private void MovePlayListItemUp()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeView.View != GUIFacadeControl.ViewMode.Playlist
          || facadeView.PlayListView == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;

      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      playList.MovePlayListItemUp(iItem);
      int selectedIndex = facadeView.MoveItemUp(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
      {
        playlistPlayer.CurrentSong = selectedIndex;
      }

      facadeView.SelectedListItemIndex = selectedIndex;
      UpdateButtonStates();
    }

    private void MovePlayListItemDown()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeView.View != GUIFacadeControl.ViewMode.Playlist
          || facadeView.PlayListView == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      playList.MovePlayListItemDown(iItem);
      int selectedIndex = facadeView.MoveItemDown(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
      {
        playlistPlayer.CurrentSong = selectedIndex;
      }

      UpdateButtonStates();
    }

    private void DeletePlayListItem()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      }

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeView.View != GUIFacadeControl.ViewMode.Playlist
          || facadeView.PlayListView == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;

      string currentFile = g_Player.CurrentFile;
      GUIListItem item = facadeView[iItem];
      RemovePlayListItem(iItem);

      if (currentFile.Length > 0 && currentFile == item.Path)
      {
        string nextTrackPath = PlayListPlayer.SingletonPlayer.GetNext();

        if (nextTrackPath.Length == 0)
        {
          g_Player.Stop();
        }

        else
        {
          if (iItem == facadeView.Count)
          {
            playlistPlayer.Play(iItem - 1);
          }

          else
          {
            playlistPlayer.PlayNext();
          }
        }
      }

      if (facadeView.Count == 0)
      {
        g_Player.Stop();
      }

      else
      {
        facadeView.PlayListView.SelectedListItemIndex = iItem;
      }

      UpdateButtonStates();
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
            // make sure we get a few songs at least...
            //if (_preferCountForTracks != 2)
            //{
            //  _preferCountForTracks = 2;
            //  Log.Info("ScrobbleLookupThread: could not find enough songs - temporarily accepting all songs");
            //  loops = 0;
            //}
            //else
            break;
          }
        }
        _preferCountForTracks = previouspreferCount;
      }

      GUIGraphicsContext.form.Invoke(new ThreadRefreshList(DoRefreshList));
    }

    private void DoRefreshList()
    {
      if (facadeView != null)
      {
        // only focus the file while playlist is visible
        if (GUIWindowManager.ActiveWindow == (int) Window.WINDOW_MUSIC_PLAYLIST)
        {
          LoadDirectory(string.Empty);
          SelectCurrentPlayingSong();
        }
      }
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
          randomPosition = rand.Next(0, songList.Count/2);
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

        if (j > songList.Count*5)
        {
          break;
        }
      }
      // _maxScrobbledSongsPerArtist are inserted      
      return true;
    }

    protected override void SetLabels()
    {
      if (facadeView != null)
      {
        try
        {
          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];
            MusicTag tag = null;
            bool dirtyTag = false;
            if (item.MusicTag != null)
            {
              tag = (MusicTag) item.MusicTag;

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

          for (int i = 0; i < facadeView.Count; ++i)
          {
            GUIListItem item = facadeView[i];

            handler.SetLabel(item.AlbumInfoTag as Song, ref item);
          }
        }
        catch (Exception ex)
        {
          Log.Error("GUIMusicPlaylist: exception occured - item without Albumtag? - {0} / {1}", ex.Message,
                    ex.StackTrace);
        }
      }
    }
  }
}