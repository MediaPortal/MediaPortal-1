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

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
using MediaPortal.MusicVideos.Database;


namespace MediaPortal.GUI.MusicVideo
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class GUIMusicVideoPlayList : GUIWindow
    {
        #region Base variabeles


        DirectoryHistory m_history = new DirectoryHistory();
        string currentFolder = String.Empty;
        int currentSelectedItem = -1;
        int previousControlId = 0;
        int m_nTempPlayListWindow = 0;
        string m_strTempPlayListDirectory = String.Empty;
        VirtualDirectory m_directory = new VirtualDirectory();
        PlayListPlayer playlistPlayer = PlayListPlayer.SingletonPlayer;
        #endregion
        [SkinControlAttribute(50)]
        protected GUIFacadeControl facadeView = null;
        [SkinControlAttribute(2)]
        protected GUIButtonControl btnViewAs = null;
        [SkinControlAttribute(3)]
        protected GUISortButtonControl btnSortBy = null;
        [SkinControlAttribute(5)]
        protected GUIButtonControl btnViews = null;
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
        private int WINDOW_ID = 4735;

        public GUIMusicVideoPlayList()
        {
            //GetID = (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST;
            playlistPlayer = PlayListPlayer.SingletonPlayer;
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

        public override bool Init()
        {
            currentFolder = System.IO.Directory.GetCurrentDirectory();
            bool result = Load(GUIGraphicsContext.Skin + @"\mymusicvideoplaylist.xml");
            m_directory.AddDrives();
            m_directory.SetExtensions(MediaPortal.Util.Utils.VideoExtensions);
            return result;
        }



        #region BaseWindow Members
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

            base.OnAction(action);
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            //LoadSettings();
            facadeView.View = GUIFacadeControl.ViewMode.Playlist;
            LoadDirectory(String.Empty);
            if ((previousControlId == facadeView.GetID) && facadeView.Count <= 0)
            {
                previousControlId = btnViewAs.GetID;
                GUIControl.FocusControl(GetID, previousControlId);
            }
            if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
            {
                int iSong = playlistPlayer.CurrentSong;
                if (iSong >= 0 && iSong <= facadeView.Count)
                {
                    GUIControl.SelectItemControl(GetID, facadeView.GetID, iSong);
                }
            }
            SelectCurrentVideo();
            //UpdateButtonStates();
        }
        protected override void OnPageDestroy(int newWindowId)
        {
            base.OnPageDestroy(newWindowId);
            currentSelectedItem = facadeView.SelectedListItemIndex;
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            base.OnClicked(controlId, control, actionType);
            if (control == btnShuffle)
            {
                OnShufflePlayList();
            }
            else if (control == btnSave)
            {
                //OnSavePlayList();
            }
            else if (control == btnClear)
            {
                OnClearPlayList();
            }
            else if (control == btnPlay)
            {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                playlistPlayer.Reset();
                playlistPlayer.Play(facadeView.SelectedListItemIndex);
                UpdateButtonStates();
            }
            else if (control == btnNext)
            {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;                
                playlistPlayer.PlayNext();
            }
            else if (control == btnPrevious)
            {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                playlistPlayer.PlayPrevious();
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
                        LoadDirectory(String.Empty);

                        if (previousControlId == facadeView.GetID && facadeView.Count <= 0)
                        {
                            previousControlId = btnViewAs.GetID;
                            GUIControl.FocusControl(GetID, previousControlId);
                        }
                        SelectCurrentVideo();

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
                if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
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
        }

        protected void LoadDirectory(string strNewDirectory)
        {
            GUIListItem SelectedItem = facadeView.SelectedListItem;
            if (SelectedItem != null)
            {
                if (SelectedItem.IsFolder && SelectedItem.Label != "..")
                {
                    m_history.Set(SelectedItem.Label, currentFolder);
                }
            }
            currentFolder = strNewDirectory;
            facadeView.Clear();

            string strObjects = String.Empty;

            ArrayList itemlist = new ArrayList();

            PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
            /* copy playlist from general playlist*/
            int iCurrentSong = -1;
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                iCurrentSong = playlistPlayer.CurrentSong;

            string strFileName;
            YahooVideo loVideo;
            for (int i = 0; i < playlist.Count; ++i)
            {

                MVPlayListItem item = (MVPlayListItem)playlist[i];
                loVideo = item.YahooVideo;
                strFileName = loVideo.artistName+" - "+loVideo.songName;

                GUIListItem pItem = new GUIListItem(strFileName);
                //pItem.Path = strFileName;
                pItem.IsFolder = false;
                //pItem.m_bIsShareOrDrive = false;

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
                itemlist.Add(pItem);
                MediaPortal.Util.Utils.SetDefaultIcons(pItem);

            }

            iCurrentSong = 0;
            strFileName = String.Empty;
            //	Search current playlist item
        //    if ((m_nTempPlayListWindow == GetID && m_strTempPlayListDirectory.IndexOf(currentFolder) >= 0 && g_Player.Playing
        //&& playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO_TEMP)
        //|| (GetID == (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO
        //        && g_Player.Playing))
        //    {
        //        iCurrentSong = playlistPlayer.CurrentSong;
        //        if (iCurrentSong >= 0)
        //        {
        //            playlist = playlistPlayer.GetPlaylist(playlistPlayer.CurrentPlaylistType);
        //            if (iCurrentSong < playlist.Count)
        //            {
        //                PlayListItem item = playlist[iCurrentSong];
        //                strFileName = item.FileName;
        //            }
        //        }
        //    }

            //SetIMDBThumbs(itemlist);

            string strSelectedItem = m_history.Get(currentFolder);
            int iItem = 0;
            foreach (GUIListItem item in itemlist)
            {
                facadeView.Add(item);

                //	synchronize playlist with current directory
                if (strFileName.Length > 0 && item.Path == strFileName)
                {
                    item.Selected = true;
                }
            }
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
            int iTotalItems = itemlist.Count;
            if (itemlist.Count > 0)
            {
                GUIListItem rootItem = (GUIListItem)itemlist[0];
                if (rootItem.Label == "..") iTotalItems--;
            }
            strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
            GUIPropertyManager.SetProperty("#itemcount", strObjects);
            if (currentSelectedItem >= 0)
            {
                GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSelectedItem);
            }

        }


        #endregion
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
        }
        void ClearFileItems()
        {
            facadeView.Clear();
        }

        void OnClearPlayList()
        {
            currentSelectedItem = -1;
            ClearFileItems();
            playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO).Clear();
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                playlistPlayer.Reset();
            LoadDirectory(String.Empty);
            UpdateButtonStates();
            GUIControl.FocusControl(GetID, btnViewAs.GetID);
        }
        

        protected void OnClick(int itemIndex)
        {
            currentSelectedItem = facadeView.SelectedListItemIndex;
            GUIListItem item = facadeView.SelectedListItem;
            if (item == null) return;
            if (item.IsFolder) return;

            playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
            playlistPlayer.Reset();
            playlistPlayer.Play(itemIndex);
        }

        protected void OnQueueItem(int itemIndex)
        {
            RemovePlayListItem(itemIndex);
        }

        void RemovePlayListItem(int itemIndex)
        {
            GUIListItem pItem = this.facadeView[itemIndex];
            if (pItem == null)
            {
                return;
            }
            PlayList loPlayList = this.playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);
            string strFileName = string.Empty;
            foreach (MVPlayListItem loItem in loPlayList)
            {
                  loItem.UpdateUrl = false;
                  YahooVideo loVideo = loItem.YahooVideo;
                  string lsDesc = loVideo.artistName + "-" + loVideo.songName;
                  if (lsDesc.Equals(pItem.Label))
                  {
                        strFileName = loItem.FileName;
                  }
            }
            this.playlistPlayer.Remove(PlayListType.PLAYLIST_MUSIC_VIDEO, strFileName);
            foreach (MVPlayListItem loItem in loPlayList)
            {
                  loItem.UpdateUrl = true;
            }
            this.LoadDirectory(currentFolder);
            this.UpdateButtonStates();
            GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);            
            SelectCurrentVideo();
        }

        void OnShufflePlayList()
        {
            currentSelectedItem = facadeView.SelectedListItemIndex;
            ClearFileItems();
            PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO);

            if (playlist.Count <= 0) return;
            string currentSongFileName = String.Empty;
            if (playlistPlayer.CurrentSong >= 0)
            {
                if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                {
                    PlayListItem item = playlist[playlistPlayer.CurrentSong];
                    currentSongFileName = item.FileName;
                }
            }
            playlist.Shuffle();
            if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
                playlistPlayer.Reset();

            if (currentSongFileName.Length > 0)
            {
                for (int i = 0; i < playlist.Count; i++)
                {
                    PlayListItem playListItem = playlist[i];
                    if (playListItem.FileName == currentSongFileName)
                        playlistPlayer.CurrentSong = i;
                }
            }

            LoadDirectory(currentFolder);
        }
        /*
        void OnSavePlayList()
        {
            currentSelectedItem = facadeView.SelectedListItemIndex;
            string playlistFileName = String.Empty;
            if (GetKeyboard(ref playlistFileName))
            {
                string playListPath = String.Empty;
                using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
                {
                    playListPath = xmlreader.GetValueAsString("movies", "playlists", String.Empty);
                    playListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(playListPath);
                }

                string fullPlayListPath = System.IO.Path.GetFileNameWithoutExtension(playlistFileName);

                fullPlayListPath += ".m3u";
                if (playListPath.Length != 0)
                {
                    fullPlayListPath = playListPath + @"\" + fullPlayListPath;
                }
                PlayList playlist = new PlayList();
                for (int i = 0; i < facadeView.Count; ++i)
                {
                    GUIListItem listItem = facadeView[i];
                    PlayListItem playListItem = new PlayListItem();
                    playListItem.FileName = listItem.Path;
                    playListItem.Description = listItem.Label;
                    playListItem.Duration = listItem.Duration;
                    playListItem.Type = Playlists.PlayListItem.PlayListItemType.Video;
                    playlist.Add(playListItem);
                }
                PlayListM3uIO saver = new PlayListM3uIO();
                saver.Save(playlist, fullPlayListPath);
            }
        }
         * */
        void SelectCurrentVideo()
        {
            if (g_Player.Playing && playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_VIDEO)
            {
                int currentSongIndex = playlistPlayer.CurrentSong;
                if (currentSongIndex >= 0 && currentSongIndex <= facadeView.Count)
                {
                    GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSongIndex);
                }
            }
        }
    }
}
