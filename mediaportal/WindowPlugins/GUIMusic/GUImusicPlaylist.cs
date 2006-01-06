/* 
 *	Copyright (C) 2005 Team MediaPortal
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
    private bool PShuffleOn = false;
    #endregion

    [SkinControlAttribute(20)]
    protected GUIButtonControl btnShuffle = null;
    [SkinControlAttribute(21)]
    protected GUIButtonControl btnSave = null;
    [SkinControlAttribute(22)]
    protected GUIButtonControl btnClear = null;
    [SkinControlAttribute(23)]
    protected GUIButtonControl btnPlay = null;
    [SkinControlAttribute(24)]
    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(25)]
    protected GUIButtonControl btnPrevious = null;
    [SkinControlAttribute(26)]
    protected GUIToggleButtonControl btnPartyShuffle = null;


    public GUIMusicPlayList()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;

      m_directory.AddDrives();
      m_directory.SetExtensions(Utils.AudioExtensions);
    }

    #region overrides
    public override bool Init()
    {
      m_strDirectory = System.IO.Directory.GetCurrentDirectory();
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
      if (view == View.Albums) return false;
      if (view == View.FilmStrip) return false;
      return true;
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

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
        PlayListPlayer.CurrentPlaylist = PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Reset();
        if (PShuffleOn == true)
        {
          for (int i = 0; i < facadeView.SelectedListItemIndex; i++)
          {
            PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
          UpdatePartyShuffle();
          //LoadDirectory(String.Empty);
          PlayListPlayer.Play(0);
        }
        else
        {
          PlayListPlayer.Play(facadeView.SelectedListItemIndex);
        }
        UpdateButtonStates();
      }
      else if (control == btnNext)
      {
        PlayListPlayer.CurrentPlaylist = PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.PlayNext();
        SelectCurrentPlayingSong();
      }
      else if (control == btnPrevious)
      {
        PlayListPlayer.CurrentPlaylist = PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.PlayPrevious();
        SelectCurrentPlayingSong();
      }
      else if (control == btnPartyShuffle)
      {
        //get state of button
        if (btnPartyShuffle.Selected)
        {
          PShuffleOn = true;
          UpdatePartyShuffle();
          LoadDirectory(String.Empty);
          GUIListItem item = facadeView[0];
          if (item != null) item.Shaded = false;
          PlayListPlayer.CurrentPlaylist = PlayListType.PLAYLIST_MUSIC;
          PlayListPlayer.Reset();
          PlayListPlayer.Play(0);
        }
        else PShuffleOn = false;

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
            if (PShuffleOn == true)
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
        if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
        {
          btnNext.Disabled = false;
          btnPrevious.Disabled = false;
        }
        else
        {
          btnNext.Disabled = true;
          btnPrevious.Disabled = true;
        }
      }
      else
      {
        btnClear.Disabled = true;
        btnPlay.Disabled = true;
        btnNext.Disabled = true;
        btnPrevious.Disabled = true;
      }

      //disable shuffle/save/previous if party shuffle is on
      if (btnPartyShuffle.Selected)
      {
        btnShuffle.Disabled = true;
        btnPlay.Disabled = true;
        //btnNext.Disabled=true;
        //btnSave.Disabled=true;
      }
      else
      {
        btnShuffle.Disabled = false;
      }
    }


    protected override void OnClick(int iItem)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null) return;
      if (item.IsFolder) return;

      //added/changed by Sam
      //check if party shuffle is on
      if (PShuffleOn == true)
      {
        if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
        {
          for (int i = 1; i < facadeView.SelectedListItemIndex; i++)
          {
            PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
          //LoadDirectory(String.Empty);
          UpdatePartyShuffle();
          LoadDirectory(String.Empty);
        }
        else
        {
          for (int i = 0; i < facadeView.SelectedListItemIndex; i++)
          {
            PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
          //LoadDirectory(String.Empty);
          UpdatePartyShuffle();
          //LoadDirectory(String.Empty);
          PlayListPlayer.CurrentPlaylist = PlayListType.PLAYLIST_MUSIC;
          PlayListPlayer.Reset();
          PlayListPlayer.Play(0);
        }
      }
      //otherwise if party shuffle is not on, do this...
      else
      {
        string strPath = item.Path;
        PlayListPlayer.CurrentPlaylist = PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Reset();
        PlayListPlayer.Play(iItem);
        SelectCurrentPlayingSong();
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
          if (PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC && PShuffleOn == true)
          {
            PlayList pl = PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            pl.Remove(pl[0].FileName);
            UpdatePartyShuffle();
            PlayListPlayer.CurrentSong = 0;
          }
          break;

        //special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
          if (PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC && PShuffleOn == true && PlayListPlayer.CurrentSong != 0)
          {
            PlayList pl = PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            pl.Remove(pl[0].FileName);
            UpdatePartyShuffle();
            PlayListPlayer.CurrentSong = 0;
            LoadDirectory(String.Empty);
          }
          break;
      }
    }


    void OnRetrieveMusicInfo(ref List<GUIListItem> items)
    {
      if (items.Count <= 0) return;
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

      PlayList playlist = PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      /* copy playlist from general playlist*/
      int iCurrentSong = -1;
      if (PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
        iCurrentSong = PlayListPlayer.CurrentSong;

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

        Utils.SetDefaultIcons(pItem);
        if (item.Played)
        {
          pItem.Shaded = true;
        }

        if (item.Duration > 0)
        {
          int nDuration = item.Duration;
          if (nDuration > 0)
          {
            string str = Utils.SecondsToHMSString(nDuration);
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
        && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC_TEMP)
        || (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC
        && g_Player.Playing))
      {
        iCurrentSong = PlayListPlayer.CurrentSong;
        if (iCurrentSong >= 0)
        {
          playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.CurrentPlaylist);
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
        if (rootItem.Label == "..") iTotalItems--;
      }
      strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
      if (totalPlayingTime.Seconds > 0)
      {
        strObjects = String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(1052),
                    Utils.SecondsToHMSString((int)totalPlayingTime.TotalSeconds));//songs
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
      if (PShuffleOn == true)
      {
        if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
        {
          for (int i = 1; i < facadeView.Count; i++)
          {
            PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
        }
        else
        {
          for (int i = 0; i < facadeView.Count; i++)
          {
            PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Remove(facadeView[i].Path);
          }
        }
        //LoadDirectory(String.Empty);
        UpdatePartyShuffle();
        LoadDirectory(String.Empty);
      }
      //otherwise, if not party shuffle...
      else
      {
        ClearFileItems();
        PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();
        if (PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
          PlayListPlayer.Reset();
        LoadDirectory(String.Empty);
        UpdateButtonStates();
        GUIControl.FocusControl(GetID, btnViewAs.GetID);
      }
      //ended changes
    }


    void RemovePlayListItem(int iItem)
    {
      //added by Sam
      if (PShuffleOn == true && g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
      {
        if (iItem == 0) return;
      }

      GUIListItem pItem = facadeView[iItem];
      if (pItem == null) return;
      string strFileName = pItem.Path;

      PlayListPlayer.Remove(PlayListType.PLAYLIST_MUSIC, strFileName);

      //added by Sam
      //check if party shuffle is on
      if (PShuffleOn == true) UpdatePartyShuffle();

      LoadDirectory(m_strDirectory);
      UpdateButtonStates();
      GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      SelectCurrentPlayingSong();
    }

    void ShufflePlayList()
    {

      ClearFileItems();
      PlayList playlist = PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      if (playlist.Count <= 0) return;
      string strFileName = String.Empty;
      if (PlayListPlayer.CurrentSong >= 0)
      {
        if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
        {
          PlayListItem item = playlist[PlayListPlayer.CurrentSong];
          strFileName = item.FileName;
        }
      }
      playlist.Shuffle();
      if (PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
        PlayListPlayer.Reset();

      if (strFileName.Length > 0)
      {
        for (int i = 0; i < playlist.Count; i++)
        {
          PlayListItem item = playlist[i];
          if (item.FileName == strFileName)
            PlayListPlayer.CurrentSong = i;
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
        using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          strPlayListPath = xmlreader.GetValueAsString("music", "playlists", String.Empty);
          strPlayListPath = Utils.RemoveTrailingSlash(strPlayListPath);
        }



        strPath += ".m3u";
        if (strPlayListPath.Length != 0)
        {
          strPath = strPlayListPath + @"\" + strPath;
        }
        PlayListM3U playlist = new PlayListM3U();
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
        playlist.Save(strPath);
      }
    }

    void SelectCurrentPlayingSong()
    {
      if (g_Player.Playing && PlayListPlayer.CurrentPlaylist == PlayListType.PLAYLIST_MUSIC)
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
        int iSong = PlayListPlayer.CurrentSong;
        if (iSong >= 0 && iSong <= facadeView.Count)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
          GUIListItem item = facadeView[iSong];
          if (item != null) item.Selected = true;
        }
      }
    }

    //added by Sam
    void AddRandomSongToPlaylist(ref Song song)
    {
      //check duplication
      PlayList playlist = PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      for (int i = 0; i < playlist.Count; i++)
      {
        PlayListItem item = playlist[i];
        if (item.FileName == song.FileName) return;
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

      PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);

    }

    //added by Sam
    void UpdatePartyShuffle()
    {
      MusicDatabase dbs = new MusicDatabase();

      PlayList list = PlayListPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      if (list.Count >= MaxNumPShuffleSongPredict) return;

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
  }
}
