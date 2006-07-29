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
using MediaPortal.MusicVideos.Database;


namespace MediaPortal.GUI.MusicVideos
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class GUIMusicVideoPlayList : GUIWindow
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
        PlayListPlayer playlistPlayer;
        protected string _currentPlaying;
        //private bool PShuffleOn = false;
        #endregion
        [SkinControlAttribute(50)]
        protected GUIFacadeControl facadeView = null;
        [SkinControlAttribute(2)]
        //protected GUIButtonControl btnViewAs = null;
        //[SkinControlAttribute(3)]
        //protected GUISortButtonControl btnSortBy = null;
        //[SkinControlAttribute(5)]
        //protected GUIButtonControl btnViews = null;
        //[SkinControlAttribute(20)]
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
        private int WINDOW_ID = 4735;

        public GUIMusicVideoPlayList()
        {
            GetID = WINDOW_ID;
            playlistPlayer = PlayListPlayer.SingletonPlayer;
            m_directory.AddDrives();
            m_directory.SetExtensions(MediaPortal.Util.Utils.AudioExtensions);
        }

        #region overrides
        public override bool Init()
        {
            m_strDirectory = System.IO.Directory.GetCurrentDirectory();            
            GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
            return Load(GUIGraphicsContext.Skin + @"\mymusicvideoplaylist.xml");
        }
        public override int GetID
        {
            get
            {
                return this.WINDOW_ID;
            }
            set
            {
                base.GetID = value;
            }
        }
        //protected bool AllowView(View view)
        //{
        //    return false;
        //}
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
                if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_VIDEO)
                {
                    playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;

                    if (g_Player.CurrentFile == "")
                    {
                        PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);

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
                m_iLastControl = btnShuffle.GetID;
                GUIControl.FocusControl(GetID, m_iLastControl);
            }
            if (facadeView.Count <= 0)
            {
                GUIControl.FocusControl(GetID, btnShuffle.GetID);
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
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                playlistPlayer.Reset();
                {
                    playlistPlayer.Play(facadeView.SelectedListItemIndex);
                }
                UpdateButtonStates();
            }
            else if (control == btnNext)
            {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                playlistPlayer.PlayNext();
                SelectCurrentPlayingSong();
            }
            else if (control == btnPrevious)
            {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                playlistPlayer.PlayPrevious();
                SelectCurrentPlayingSong();
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
                        

                        if (m_iLastControl == facadeView.GetID && facadeView.Count <= 0)
                        {
                            m_iLastControl = btnShuffle.GetID;
                            GUIControl.FocusControl(GetID, m_iLastControl);
                        }

                        SelectCurrentPlayingSong();
                    }
                    break;
            }
            return base.OnMessage(message);
        }

        protected void UpdateButtonStates()
        {
            //base.UpdateButtonStates();

            if (facadeView.Count > 0)
            {
                btnClear.Disabled = false;
                btnPlay.Disabled = false;
                btnSave.Disabled = false;
                if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                {
                    btnNext.Disabled = false;
                    btnPrevious.Disabled = false;
                    //btnSave.Disabled = false;
                }
                else
                {
                    btnNext.Disabled = true;
                    btnPrevious.Disabled = true;
                    //btnSave.Disabled = true;
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
        }

        protected void OnClick(int iItem)
        {
            GUIListItem item = facadeView.SelectedListItem;
            if (item == null) return;
            if (item.IsFolder) return;

                string strPath = item.Path;
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                playlistPlayer.Reset();
                playlistPlayer.Play(iItem);
                SelectCurrentPlayingSong();
                UpdateButtonStates();
        }

        protected void OnQueueItem(int iItem)
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
                    break;

                //special case for when the next button is pressed - stopping the prev song does not cause a Playback_Ended event
                case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED:
                    break;
            }
        }

        /*
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
                    //else if (UseID3)
                    //{
                    //    item.MusicTag = TagReader.TagReader.ReadTag(item.Path);
                    //}
                }
            }
        }
        */
        protected void LoadDirectory(string strNewDirectory)
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

            PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
            /* copy playlist from general playlist*/
            int iCurrentSong = -1;
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                iCurrentSong = playlistPlayer.CurrentSong;

            string strFileName;
            YahooVideo loVideo;
            for (int i = 0; i < playlist.Count; ++i)
            {
                MVPlayListItem loPlayListItem = (MVPlayListItem)playlist[i];
                loVideo = loPlayListItem.YahooVideo;
                //strFileName = loVideo.artistName+" - "+loVideo.songName;

                GUIListItem pItem = new GUIListItem(loVideo.artistName + " - " + loVideo.songName);
                pItem.Path = loVideo.songId;
                pItem.MusicTag = loPlayListItem.MusicTag;
                pItem.IsFolder = false;
                //pItem.m_bIsShareOrDrive = false;

                MediaPortal.Util.Utils.SetDefaultIcons(pItem);
                if (loPlayListItem.Played)
                {
                    pItem.Shaded = true;
                }

                if (loPlayListItem.Duration > 0)
                {
                    int nDuration = loPlayListItem.Duration;
                    if (nDuration > 0)
                    {
                        string str = MediaPortal.Util.Utils.SecondsToHMSString(nDuration);
                        pItem.Label2 = str;
                    }
                    else
                        pItem.Label2 = String.Empty;
                }
                //pItem.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
                itemlist.Add(pItem);
            }
            //OnRetrieveMusicInfo(ref itemlist);
            iCurrentSong = 0;
            strFileName = String.Empty;
            //	Search current playlist item
            /*
            if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(m_strDirectory) >= 0 && g_Player.Playing
              && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
              || (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO
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
            */
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
                ClearFileItems();
                playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO).Clear();
                if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                    playlistPlayer.Reset();
                LoadDirectory(String.Empty);
                UpdateButtonStates();
                GUIControl.FocusControl(GetID, btnShuffle.GetID);
        }


        void RemovePlayListItem(int iItem)
        {
            

            GUIListItem pItem = facadeView[iItem];
            if (pItem == null) return;
            //string strFileName = pItem.Path;

            PlayList loPlayList = this.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
            string strFileName = string.Empty;
            foreach (MVPlayListItem loItem in loPlayList)
            {
                  loItem.UpdateUrl = false;
                  YahooVideo loVideo = loItem.YahooVideo;             
                  if (loVideo.songId.Equals(pItem.Path))
                  {
                        strFileName = loItem.FileName;
                  }
            }

            playlistPlayer.Remove(PlayListType.PLAYLIST_MUSIC_VIDEO, strFileName);
            foreach (MVPlayListItem loItem in loPlayList)
            {
                  loItem.UpdateUrl = true;
            }

            LoadDirectory(m_strDirectory);
            UpdateButtonStates();
            GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
            SelectCurrentPlayingSong();
        }

        void ShufflePlayList()
        {

            ClearFileItems();
            PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);

            if (playlist.Count <= 0) return;
            string strFileName = String.Empty;
            if (playlistPlayer.CurrentSong >= 0)
            {
                if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                {
                    PlayListItem item = playlist[playlistPlayer.CurrentSong];
                    strFileName = item.FileName;
                }
            }
            playlist.Shuffle();
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
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
                //string strPath = System.IO.Path.GetFileNameWithoutExtension(strNewFileName);
                //string strPlayListPath = String.Empty;
                //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
                //{
                //    strPlayListPath = xmlreader.GetValueAsString("music", "playlists", String.Empty);
                //    strPlayListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(strPlayListPath);
                //}

                //strPath += ".m3u";
                //if (strPlayListPath.Length != 0)
                //{
                //    strPath = strPlayListPath + @"\" + strPath;
                //}
                PlayList loPlaylist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
                YahooVideo loVideo;
                MusicVideoDatabase loDatabase = MusicVideoDatabase.getInstance();
                loDatabase.createPlayList(strNewFileName);
                int i=0;
                foreach(MVPlayListItem loPlayListItem in loPlaylist)
                {
                    i++;
                    loVideo = loPlayListItem.YahooVideo;

                    loDatabase.addPlayListVideo(strNewFileName, loVideo, i);
                }
                
            }
        }
        
        void SelectCurrentPlayingSong()
        {
            if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
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
                    if (item != null) item.Selected = true;
                }
            }
        }

        /*
        void UpdatePartyShuffle()
        {
            MusicDatabase dbs = new MusicDatabase();

            PlayList list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
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
        */

        private void MovePlayListItemUp()
        {
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;

            if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_VIDEO
                || facadeView.View != GUIFacadeControl.ViewMode.Playlist
                || facadeView.PlayListView == null)
            {
                return;
            }

            int iItem = facadeView.SelectedListItemIndex;

            // Prevent moving backwards past the top song in the list

            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
            playList.MovePlayListItemUp(iItem);
            int selectedIndex = facadeView.MoveItemUp(iItem, true);

            if (iItem == playlistPlayer.CurrentSong)
                playlistPlayer.CurrentSong = selectedIndex;

            facadeView.SelectedListItemIndex = selectedIndex;
            UpdateButtonStates();

        }

        private void MovePlayListItemDown()
        {
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;

            if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_VIDEO
                || facadeView.View != GUIFacadeControl.ViewMode.Playlist
                || facadeView.PlayListView == null)
            {
                return;
            }

            int iItem = facadeView.SelectedListItemIndex;
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);

            // Prevent moving fowards past the last song in the list
            // as this would cause the currently playing song to scroll
            // off of the list view...

            playList.MovePlayListItemDown(iItem);
            int selectedIndex = facadeView.MoveItemDown(iItem, true);

            if (iItem == playlistPlayer.CurrentSong)
                playlistPlayer.CurrentSong = selectedIndex;

                facadeView.SelectedListItemIndex = selectedIndex;

            UpdateButtonStates();

        }

        private void DeletePlayListItem()
        {
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;

            if (playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_VIDEO
                || facadeView.View != GUIFacadeControl.ViewMode.Playlist
                || facadeView.PlayListView == null)
            {
                return;
            }

            int iItem = facadeView.SelectedListItemIndex;

            string currentFile = g_Player.CurrentFile;
            GUIListItem item = facadeView[iItem];
            PlayList loPlayList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
            string strFileName = string.Empty;
            MVPlayListItem loItemToDelete = null;
            foreach (MVPlayListItem loItem in loPlayList)
            {                
                YahooVideo loVideo = loItem.YahooVideo;
                //string lsDesc = loVideo.artistName + " - " + loVideo.songName;
                if (loVideo.songId.Equals(item.Path))
                {
                    loItemToDelete = loItem;
                }
            }
            RemovePlayListItem(iItem);
            if (loItemToDelete != null)
            {
                loItemToDelete.UpdateUrl = false;
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
                loItemToDelete.UpdateUrl = true;
            }

            if (facadeView.Count == 0)
                g_Player.Stop();

            else
                facadeView.PlayListView.SelectedListItemIndex = iItem;

            UpdateButtonStates();

        }

        protected  void SetLabels()
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


            //for (int i = 0; i < facadeView.Count; ++i)
            //{
                //GUIListItem item = facadeView[i];
                //handler.SetLabel(item.AlbumInfoTag as Song, ref item);
            //}
        }
        protected bool GetKeyboard(ref string strLine)
        {
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            if (null == keyboard) return false;
            keyboard.Reset();
            keyboard.DoModal(GetID);
            if (keyboard.IsConfirmed)
            {
                strLine = keyboard.Text;
                return true;
            }
            return false;
        }

    }
}