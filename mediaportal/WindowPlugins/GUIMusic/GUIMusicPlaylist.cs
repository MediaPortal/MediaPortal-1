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
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;


namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicPlayList : GUIMusicBaseWindow
  {
    #region Base variabeles
    DirectoryHistory m_history = new DirectoryHistory();
    string m_strDirectory = String.Empty;
    int m_iItemSelected = -1;
    int m_iLastControl = 0;
    int m_nTempPlayListWindow = 0;
    string m_strTempPlayListDirectory = String.Empty;
    string m_strCurrentFile = String.Empty;
    VirtualDirectory m_directory = new VirtualDirectory();
    const int MaxNumPShuffleSongPredict = 12;
    const int _maxScrobbledSongsPerArtist = 1;
    const int _maxScrobbledArtistsForSongs = 4;
    private bool PShuffleOn = false;
    private bool ScrobblerOn = false;
    private bool _enableScrobbling = false;
    #endregion

    [SkinControlAttribute(20)]    protected GUIButtonControl btnShuffle = null;
    [SkinControlAttribute(21)]    protected GUIButtonControl btnSave = null;
    [SkinControlAttribute(22)]    protected GUIButtonControl btnClear = null;
    [SkinControlAttribute(23)]    protected GUIButtonControl btnPlay = null;
    //[SkinControlAttribute(24)]        protected GUIButtonControl btnNext = null;
    //[SkinControlAttribute(25)]        protected GUIButtonControl btnPrevious = null;
    [SkinControlAttribute(26)]    protected GUIToggleButtonControl btnPartyShuffle = null;
    [SkinControlAttribute(27)]    protected GUIToggleButtonControl btnScrobble = null;


    public GUIMusicPlayList()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;

      m_directory.AddDrives();
      m_directory.SetExtensions(MediaPortal.Util.Utils.AudioExtensions);
    }

    #region overrides
    public override bool Init()
    {
      m_strDirectory = System.IO.Directory.GetCurrentDirectory();

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        _enableScrobbling = xmlreader.GetValueAsBool("plugins", "audioscrobbler", false);
            
      //added by Sam
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
      return Load(GUIGraphicsContext.Skin + @"\myMusicplaylist.xml");
    }
    protected override string SerializeName
    {
      get
      {
        return "mymusicplaylist";
      }
    }
    protected override bool AllowView(View view)
    {
      if (view == View.List)
        return false;
      if (view == View.Albums)
        return false;
      if (view == View.FilmStrip)
        return false;
      return true;
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      else if (action.wID == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP)
        MovePlayListItemUp();

      else if (action.wID == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN)
        MovePlayListItemDown();

      else if (action.wID == Action.ActionType.ACTION_DELETE_SELECTED_ITEM)
        DeletePlayListItem();

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

          if (g_Player.CurrentFile == "")
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
      facadeView.View = GUIFacadeControl.ViewMode.Playlist;

      LoadDirectory(String.Empty);
      if (m_iItemSelected >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
      }
      if ((m_iLastControl == facadeView.GetID) && facadeView.Count <= 0)
      {
        m_iLastControl = btnViewAs.GetID;
        GUIControl.FocusControl(GetID, m_iLastControl);
      }
      if (facadeView.Count <= 0)
      {
        GUIControl.FocusControl(GetID, btnViewAs.GetID);
      }
      SelectCurrentPlayingSong();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iItemSelected = facadeView.SelectedListItemIndex;
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
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
      else if (control == btnPlay)
      {
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Reset();
        if (PShuffleOn == true)
        {
          for (int i = 0; i < facadeView.SelectedListItemIndex; i++)
          {
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
          UpdatePartyShuffle();
          //LoadDirectory(String.Empty);
          playlistPlayer.Play(0);
        }
        else
        {
          playlistPlayer.Play(facadeView.SelectedListItemIndex);
        }
        UpdateButtonStates();
      }
      //else if (control == btnNext)
      //{
      //    playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      //    playlistPlayer.PlayNext();
      //    SelectCurrentPlayingSong();
      //}
      //else if (control == btnPrevious)
      //{
      //    playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      //    playlistPlayer.PlayPrevious();
      //    SelectCurrentPlayingSong();
      //}
      else if (control == btnPartyShuffle)
      {
        //get state of button
        if (btnPartyShuffle.Selected)
        {
          // Clear the existing playlist before entering Party Shuffle mode
          ClearPlayList();

          PShuffleOn = true;
          ScrobblerOn = false;
          UpdatePartyShuffle();
          LoadDirectory(String.Empty);
          GUIListItem item = facadeView[0];
          if (item != null)
            item.Shaded = false;
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
          playlistPlayer.Reset();
          playlistPlayer.Play(0);
        }
        else
          PShuffleOn = false;

        if (facadeView.PlayListView != null)
        {
          // Prevent the currently playing track from being scrolled off the top 
          // or bottom of the screen when other items are re-ordered
          facadeView.PlayListView.AllowLastVisibleListItemDown = !PShuffleOn;
          facadeView.PlayListView.AllowMoveFirstVisibleListItemUp = !PShuffleOn;
        }

        UpdateButtonStates();
      }
      else if (control == btnScrobble)
      {
        //if (_enableScrobbling)
        //{
          //get state of button
          if (btnScrobble.Selected)
          {
            // Clear the existing playlist before entering Scrobble mode
            //ClearPlayList();

            ScrobblerOn = true;
            PShuffleOn = false;
            // Fill playlist here!
            //LoadDirectory(String.Empty);
            //GUIListItem item = facadeView[0];
            //if (item != null)
            //  item.Shaded = false;
            //playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
            //playlistPlayer.Reset();
            //playlistPlayer.Play(0);
          }
          else
            ScrobblerOn = false;
        //}
        //else
        //{
        //  ScrobblerOn = false;
        //  btnScrobble.Selected = false;
        //}
        if (facadeView.PlayListView != null)
        {
          // Prevent the currently playing track from being scrolled off the top 
          // or bottom of the screen when other items are re-ordered
          facadeView.PlayListView.AllowLastVisibleListItemDown = !ScrobblerOn;
          facadeView.PlayListView.AllowMoveFirstVisibleListItemUp = !ScrobblerOn;
        }
        UpdateButtonStates();
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
            //if party shuffle...
            if (PShuffleOn)// || ScrobblerOn)
            {
              LoadDirectory(String.Empty);
              UpdateButtonStates();
            }
            //ended changes

            if (m_iLastControl == facadeView.GetID && facadeView.Count <= 0)
            {
              m_iLastControl = btnViewAs.GetID;
              GUIControl.FocusControl(GetID, m_iLastControl);
            }

            SelectCurrentPlayingSong();
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void UpdateButtonStates()
    {
      base.UpdateButtonStates();

      if (facadeView.Count > 0)
      {
        btnClear.Disabled = false;
        btnPlay.Disabled = false;
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
        {
          //btnNext.Disabled = false;
          //btnPrevious.Disabled = false;
          btnSave.Disabled = false;
        }
        else
        {
          //btnNext.Disabled = true;
          //btnPrevious.Disabled = true;
          btnSave.Disabled = true;
        }
      }
      else
      {
        btnClear.Disabled = true;
        btnPlay.Disabled = true;        
        //btnNext.Disabled = true;
        //btnPrevious.Disabled = true;
      }

      if (_enableScrobbling)
        btnScrobble.Disabled = false;
      else
        btnScrobble.Disabled = true;

      //disable shuffle/save/previous if party shuffle is on
      if (btnPartyShuffle.Selected)
      {
        btnShuffle.Disabled = true;
        btnPlay.Disabled = true;
        btnClear.Disabled = true;
        btnSave.Disabled = true;
        btnScrobble.Disabled = true;
        //btnPrevious.Disabled = true;
      }
      else
      {
        btnScrobble.Disabled = false;
        btnShuffle.Disabled = false;
      }
    }

    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null)
        return;
      if (item.IsFolder)
        return;

      //added/changed by Sam
      //check if party shuffle is on
      if (PShuffleOn)// || ScrobblerOn)
      {
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
        {
          for (int i = 1; i < facadeView.SelectedListItemIndex; i++)
          {
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
          //LoadDirectory(String.Empty);
          if (PShuffleOn)
            UpdatePartyShuffle();
          LoadDirectory(String.Empty);
        }
        else
        {
          for (int i = 0; i < facadeView.SelectedListItemIndex; i++)
          {
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
          if (PShuffleOn)
            UpdatePartyShuffle();
          //LoadDirectory(String.Empty);
          playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
          playlistPlayer.Reset();
          playlistPlayer.Play(0);
        }
      }
      //otherwise if party shuffle is not on, do this...
      else
      {
        string strPath = item.Path;
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Reset();
        playlistPlayer.Play(iItem);
        SelectCurrentPlayingSong();
        UpdateButtonStates();
      }
      //ended changes
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
    void OnThreadMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
          {
            if (PShuffleOn)// || ScrobblerOn)
            {
              PlayList pl = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
              pl.Remove(pl[0].FileName);
              if (PShuffleOn)
                UpdatePartyShuffle();
              playlistPlayer.CurrentSong = 0;
            }
          }
          break;

        //special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC && playlistPlayer.CurrentSong != 0)
          {
            if (PShuffleOn)// || ScrobblerOn)
            {
              PlayList pl = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
              pl.Remove(pl[0].FileName);
              if (PShuffleOn)
                UpdatePartyShuffle();
              playlistPlayer.CurrentSong = 0;
              LoadDirectory(String.Empty);
            }
          }
          break;
        // delaying internet lookups for smooth playback start
        case GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC:
          if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC && ScrobblerOn == true) // && playlistPlayer.CurrentSong != 0)
          {
            AudioscrobblerBase ascrobbler = new AudioscrobblerBase();
            ascrobbler.Disconnect();
            MusicDatabase dbs = new MusicDatabase();
            Song current10SekSong = new Song();
            List<Song> scrobbledArtists = new List<Song>();
            string strFile = g_Player.Player.CurrentFile;
            bool songFound = dbs.GetSongByFileName(strFile, ref current10SekSong);
            if (songFound)
            {
              ascrobbler.ArtistMatchPercent = 75;
              scrobbledArtists = ascrobbler.getSimilarArtists(current10SekSong.Artist);

              if (scrobbledArtists.Count < _maxScrobbledArtistsForSongs)
              {
                ascrobbler.ArtistMatchPercent = 50;
                scrobbledArtists = ascrobbler.getSimilarArtists(current10SekSong.Artist);
              }
              if (scrobbledArtists.Count < _maxScrobbledArtistsForSongs)
              {
                if (scrobbledArtists.Count > 0)
                  for (int i = 0; i < scrobbledArtists.Count; i++)
                    ScrobbleSimilarArtists(scrobbledArtists[i].Artist);
              }
              else // finally enough artists
              {
                // add random factor for list here
                for (int i = 0; i < _maxScrobbledArtistsForSongs; i++)
                {
                  ScrobbleSimilarArtists(scrobbledArtists[i].Artist);
                }
              }
            }
          }
          break;
      }
    }

    void OnRetrieveMusicInfo(ref List<GUIListItem> items)
    {
      if (items.Count <= 0)
        return;
      MusicDatabase dbs = new MusicDatabase();
      Song song = new Song();
      foreach (GUIListItem item in items)
      {
        if (item.MusicTag == null)
        {
          if (dbs.GetSongByFileName(item.Path, ref song))
          {
            MusicTag tag = new MusicTag();
            tag.Album = song.Album;
            tag.Artist = song.Artist;
            tag.Genre = song.Genre;
            tag.Duration = song.Duration;
            tag.Title = song.Title;
            tag.Track = song.Track;
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

      string strObjects = String.Empty;

      List<GUIListItem> itemlist = new List<GUIListItem>();

      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      /* copy playlist from general playlist*/
      int iCurrentSong = -1;
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
        iCurrentSong = playlistPlayer.CurrentSong;

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

        MediaPortal.Util.Utils.SetDefaultIcons(pItem);
        if (item.Played)
        {
          pItem.Shaded = true;
        }

        if (item.Duration > 0)
        {
          int nDuration = item.Duration;
          if (nDuration > 0)
          {
            string str = MediaPortal.Util.Utils.SecondsToHMSString(nDuration);
            pItem.Label2 = str;
          }
          else
            pItem.Label2 = String.Empty;
        }
        pItem.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        itemlist.Add(pItem);
      }
      OnRetrieveMusicInfo(ref itemlist);
      iCurrentSong = 0;
      strFileName = String.Empty;
      //	Search current playlist item
      if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(m_strDirectory) >= 0 && g_Player.Playing
        && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
        || (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC
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
            totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
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
          iTotalItems--;
      }
      strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
      if (totalPlayingTime.Seconds > 0)
      {
        strObjects = String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(1052),
                    MediaPortal.Util.Utils.SecondsToHMSString((int)totalPlayingTime.TotalSeconds));//songs
      }

      GUIPropertyManager.SetProperty("#itemcount", strObjects);
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
    }



    void ClearFileItems()
    {
      GUIControl.ClearControl(GetID, facadeView.GetID);
    }

    void ClearPlayList()
    {
      //added/changed by Sam
      //if party shuffle
      if (PShuffleOn)// || ScrobblerOn)
      {
        if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
        {
          for (int i = 1; i < facadeView.Count; i++)
          {
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
        }
        else
        {
          for (int i = 0; i < facadeView.Count; i++)
          {
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
        }
        if (PShuffleOn)
          UpdatePartyShuffle();
        LoadDirectory(String.Empty);
      }
      //otherwise, if not party shuffle...
      else
      {
        ClearFileItems();
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
        if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
          playlistPlayer.Reset();
        LoadDirectory(String.Empty);
        UpdateButtonStates();
        GUIControl.FocusControl(GetID, btnViewAs.GetID);
      }
      //ended changes
    }


    void RemovePlayListItem(int iItem)
    {
      //added by Sam
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
      {
        if (PShuffleOn)// || ScrobblerOn)
          if (iItem == 0)
            return;
      }

      GUIListItem pItem = facadeView[iItem];
      if (pItem == null)
        return;
      string strFileName = pItem.Path;

      playlistPlayer.Remove(PlayListType.PLAYLIST_MUSIC, strFileName);

      //added by Sam
      //check if party shuffle is on
      if (PShuffleOn)
        UpdatePartyShuffle();

      LoadDirectory(m_strDirectory);
      UpdateButtonStates();
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      SelectCurrentPlayingSong();
    }

    void ShufflePlayList()
    {

      ClearFileItems();
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      if (playlist.Count <= 0)
        return;
      string strFileName = String.Empty;
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
        playlistPlayer.Reset();

      if (strFileName.Length > 0)
      {
        for (int i = 0; i < playlist.Count; i++)
        {
          PlayListItem item = playlist[i];
          if (item.FileName == strFileName)
            playlistPlayer.CurrentSong = i;
        }
      }

      LoadDirectory(m_strDirectory);

      SelectCurrentPlayingSong();
    }


    void SavePlayList()
    {
      string strNewFileName = String.Empty;
      if (GetKeyboard(ref strNewFileName))
      {
        string strPath = System.IO.Path.GetFileNameWithoutExtension(strNewFileName);
        string strPlayListPath = String.Empty;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          strPlayListPath = xmlreader.GetValueAsString("music", "playlists", String.Empty);
          strPlayListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(strPlayListPath);
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

    void SelectCurrentPlayingSong()
    {
      if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
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
            item.Selected = true;
        }
      }
    }

    //added by Sam
    bool AddRandomSongToPlaylist(ref Song song)
    {
      //check duplication
      PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      for (int i = 0; i < playlist.Count; i++)
      {
        PlayListItem item = playlist[i];
        if (item.FileName == song.FileName)
          return false;
      }

      //add to playlist
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
      playlistItem.FileName = song.FileName;
      playlistItem.Description = song.Track + ". " + song.Artist + " - " + song.Title;
      playlistItem.Duration = song.Duration;

      MusicTag tag = new MusicTag();
      tag.Title = song.Title;
      tag.Album = song.Album;
      tag.Artist = song.Artist;
      tag.Duration = song.Duration;
      tag.Genre = song.Genre;
      tag.Track = song.Track;
      tag.Year = song.Year;
      tag.Rating = song.Rating;

      playlistItem.MusicTag = tag;

      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);
      return true;
    }

    //added by Sam
    void UpdatePartyShuffle()
    {
      MusicDatabase dbs = new MusicDatabase();

      PlayList list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      if (list.Count >= MaxNumPShuffleSongPredict)
        return;

      int i;
      Song song = new Song();
      //if not enough songs, add all available songs
      if (dbs.GetNumOfSongs() < MaxNumPShuffleSongPredict)
      {
        List<Song> songs = new List<Song>();
        dbs.GetAllSongs(ref songs);

        for (i = 0; i < songs.Count; i++)
        {
          song = songs[i];
          AddRandomSongToPlaylist(ref song);
        }
      }
      //otherwise add until number of songs = MaxNumPShuffleSongPredict
      else
      {
        i = list.Count;
        while (i < MaxNumPShuffleSongPredict)
        {
          song.Clear();
          dbs.GetRandomSong(ref song);
          AddRandomSongToPlaylist(ref song);
          i = list.Count;
        }
      }
      //LoadDirectory(String.Empty); - will cause errors when playlist screen is not active
    }


    private void MovePlayListItemUp()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeView.View != GUIFacadeControl.ViewMode.Playlist
          || facadeView.PlayListView == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;

      // Prevent moving backwards past the top song in the list
      if (ScrobblerOn)// || PShuffleOn)
        if (iItem == 0)
          return;

      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      playList.MovePlayListItemUp(iItem);
      int selectedIndex = facadeView.MoveItemUp(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
        playlistPlayer.CurrentSong = selectedIndex;

      facadeView.SelectedListItemIndex = selectedIndex;
      UpdateButtonStates();

      if (PShuffleOn)// || ScrobblerOn)
      {
        // If the item we moved is now the first item in the list view should remove it from the 
        // playlist so the CurrentSong is the first item displayed...
        if (selectedIndex == 0)
        {
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[0].Path);
          playlistPlayer.CurrentSong = 0;
          if (PShuffleOn)
            UpdatePartyShuffle();
          LoadDirectory(String.Empty);
          SelectCurrentPlayingSong();
          facadeView.SelectedListItemIndex = 1;
        }
      }
    }

    private void MovePlayListItemDown()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

      if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC
          || facadeView.View != GUIFacadeControl.ViewMode.Playlist
          || facadeView.PlayListView == null)
      {
        return;
      }

      int iItem = facadeView.SelectedListItemIndex;
      PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      // Prevent moving fowards past the last song in the list
      // as this would cause the currently playing song to scroll
      // off of the list view...
      if (PShuffleOn)// || ScrobblerOn)
        if (iItem == playList.Count - 1)
          return;

      playList.MovePlayListItemDown(iItem);
      int selectedIndex = facadeView.MoveItemDown(iItem, true);

      if (iItem == playlistPlayer.CurrentSong)
        playlistPlayer.CurrentSong = selectedIndex;

      if (PShuffleOn)// || ScrobblerOn)
        if (selectedIndex == MaxNumPShuffleSongPredict - 1)
          facadeView.SelectedListItemIndex = MaxNumPShuffleSongPredict - 2;
        else
          facadeView.SelectedListItemIndex = selectedIndex;

      UpdateButtonStates();

      if (PShuffleOn)// || ScrobblerOn)
      {
        if (selectedIndex == playlistPlayer.CurrentSong)
        {
          for (int i = 0; i < selectedIndex; i++)
          {
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }

          playlistPlayer.CurrentSong = 0;
          if (PShuffleOn)
            UpdatePartyShuffle();
          LoadDirectory(String.Empty);
          SelectCurrentPlayingSong();
        }
      }
    }

    private void DeletePlayListItem()
    {
      if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

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
          g_Player.Stop();

        else
        {
          if (iItem == facadeView.Count)
            playlistPlayer.Play(iItem - 1);

          else
            playlistPlayer.PlayNext();
        }
      }

      if (facadeView.Count == 0)
        g_Player.Stop();

      else
        facadeView.PlayListView.SelectedListItemIndex = iItem;

      UpdateButtonStates();

      if (PShuffleOn == true)
        UpdatePartyShuffle();
    }

    //added by rtv
    void ScrobbleSimilarArtists(string Artist_)
    {
      MusicDatabase dbs = new MusicDatabase();

      PlayList list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      //int i;
      ArrayList similarSongList = new ArrayList();
      Song[] songs = null;
      int songsCount = 0;
      int songsAdded = 0;
      int j = 0;
      //i = list.Count;
      

      dbs.GetSongsByArtist(Artist_, ref similarSongList);
      songs = (Song[])similarSongList.ToArray(typeof(Song));

      foreach (Song singlesong in songs)
      {
        songsCount++;
      }
      // exit if not enough songs were found
      if (songsCount < _maxScrobbledSongsPerArtist)
        return;

      // TODO: add randomness here
      while (songsAdded < _maxScrobbledSongsPerArtist)
      {
        if (AddRandomSongToPlaylist(ref songs[j]))
          songsAdded++;

        j++;
        if (j >= songsCount)
          return;
      }

      // TODO: decide of this should be placed in calling function
      LoadDirectory(String.Empty);
    }
    

    protected override void SetLabels()
    {
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        MusicTag tag = (MusicTag)item.MusicTag;
        if (tag != null)
        {
          int playCount = tag.TimesPlayed;
          string duration = MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);
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
  }
}