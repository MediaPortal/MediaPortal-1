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
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Music
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class GUIMusicGenres : GUIMusicBaseWindow
    {
        class TrackComparer : IComparer<Song>
        {
            public int Compare(Song s1, Song s2)
            {
                return s1.Track - s2.Track;
            }
        }

        #region Base variables

        DirectoryHistory m_history = new DirectoryHistory();
        string m_strDirectory = String.Empty;
        int m_iItemSelected = -1;
        VirtualDirectory m_directory = new VirtualDirectory();
        int[] views = new int[50];
        bool[] sortasc = new bool[50];
        int[] sortby = new int[50];
        static string _showArtist = String.Empty;

        int _currentLevel;
        ViewDefinition _currentView;
        List<Share> _shareList = new List<Share>();

        [SkinControlAttribute(9)]
        protected GUIButtonControl btnSearch = null;

        [SkinControlAttribute(14)]
        protected GUIButtonControl btnPlaylistFolder;

        [SkinControlAttribute(12)]
        protected GUIButtonControl btnPlayCd;

        string m_strCurrentFolder = String.Empty;

        private DateTime Previous_ACTION_PLAY_Time = DateTime.Now;
        private TimeSpan AntiRepeatInterval = new TimeSpan(0, 0, 0, 0, 500);

        #endregion

        public GUIMusicGenres()
        {
            for (int i = 0; i < sortasc.Length; ++i)
                sortasc[i] = true;
            GetID = (int)GUIWindow.Window.WINDOW_MUSIC_GENRE;

            m_directory.AddDrives();
            m_directory.SetExtensions(MediaPortal.Util.Utils.AudioExtensions);
            playlistPlayer = PlayListPlayer.SingletonPlayer;

            GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
        }


        #region Serialisation
        protected override void LoadSettings()
        {
            base.LoadSettings();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(_config.Get(MediaPortal.Utils.Services.Config.Options.ConfigPath) + "MediaPortal.xml"))
            {
                string strDefault = xmlreader.GetValueAsString("music", "default", String.Empty);
                _shareList.Clear();
                for (int i = 0; i < 20; i++)
                {
                    string strShareName = String.Format("sharename{0}", i);
                    string strSharePath = String.Format("sharepath{0}", i);
                    string strPincode = String.Format("pincode{0}", i); ;

                    string shareType = String.Format("sharetype{0}", i);
                    string shareServer = String.Format("shareserver{0}", i);
                    string shareLogin = String.Format("sharelogin{0}", i);
                    string sharePwd = String.Format("sharepassword{0}", i);
                    string sharePort = String.Format("shareport{0}", i);
                    string remoteFolder = String.Format("shareremotepath{0}", i);
                    string shareViewPath = String.Format("shareview{0}", i);

                    Share share = new Share();
                    share.Name = xmlreader.GetValueAsString("music", strShareName, String.Empty);
                    share.Path = xmlreader.GetValueAsString("music", strSharePath, String.Empty);
                    share.Pincode = xmlreader.GetValueAsInt("music", strPincode, -1);

                    share.IsFtpShare = xmlreader.GetValueAsBool("music", shareType, false);
                    share.FtpServer = xmlreader.GetValueAsString("music", shareServer, String.Empty);
                    share.FtpLoginName = xmlreader.GetValueAsString("music", shareLogin, String.Empty);
                    share.FtpPassword = xmlreader.GetValueAsString("music", sharePwd, String.Empty);
                    share.FtpPort = xmlreader.GetValueAsInt("music", sharePort, 21);
                    share.FtpFolder = xmlreader.GetValueAsString("music", remoteFolder, "/");
                    share.DefaultView = (Share.Views)xmlreader.GetValueAsInt("music", shareViewPath, (int)Share.Views.List);

                    if (share.Name.Length > 0)
                    {
                        if (strDefault == share.Name) share.Default = true;
                        _shareList.Add(share);
                    }
                    else break;
                }
            }
        }
        #endregion

        // Make sure we get all of the ACTION_PLAY event (OnAction only receives the ACTION_PLAY event when 
        // the player is not playing)...
        void GUIWindowManager_OnNewAction(Action action)
        {
            if ((action.wID == Action.ActionType.ACTION_PLAY
                || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
                && GUIWindowManager.ActiveWindow == GetID)
            {
                GUIListItem item = facadeView.SelectedListItem;

                if (AntiRepeatActive() || item == null || item.Label == "..")
                    return;

                OnPlayNow(item, facadeView.SelectedListItemIndex);
            }
        }

        #region overrides
        public override bool Init()
        {
            m_strDirectory = String.Empty;
            GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);
            return Load(GUIGraphicsContext.Skin + @"\mymusicgenres.xml");
        }
        protected override string SerializeName
        {
            get
            {
                return "mymusic" + handler.CurrentView;
            }
        }

        protected override View CurrentView
        {
            get
            {
                return (View)views[handler.CurrentLevel];
            }
            set
            {
                views[handler.CurrentLevel] = (int)value;
            }
        }

        protected override bool CurrentSortAsc
        {
            get
            {
                return sortasc[handler.CurrentLevel];
            }
            set
            {
                sortasc[handler.CurrentLevel] = value;
            }
        }
        protected override MusicSort.SortMethod CurrentSortMethod
        {
            get
            {
                return (MusicSort.SortMethod)sortby[handler.CurrentLevel];
            }
            set
            {
                sortby[handler.CurrentLevel] = (int)value;
            }
        }
        protected override bool AllowView(View view)
        {
            return true;
        }

        public override void OnAction(Action action)
        {
            if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
            {
                if (m_strDirectory != m_strPlayListPath)
                {
                    if (facadeView.Focus)
                    {
                        GUIListItem item = facadeView[0];
                        if (item != null && item.IsFolder)
                        {
                            if (item.Label == "..")
                            {
                                if (handler.CurrentLevel > 0)
                                {
                                    handler.CurrentLevel--;
                                    //LoadDirectory(item.Path);
                                    LoadDirectory((handler.CurrentLevel + 1).ToString());
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    LoadDirectory(m_strCurrentFolder);
                    return;
                }
            }

            if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
            {
                GUIListItem item = facadeView[0];
                if (item != null && item.IsFolder)
                {

                    if (item.Label == ".." && item.Path != String.Empty)
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
                            handler.Select(item.AlbumInfoTag as Song);

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
                return;
            }

            base.OnAction(action);
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            string view = MusicState.View;
            if (view == String.Empty)
                view = ((ViewDefinition)handler.Views[0]).Name;

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
            if (_showArtist != String.Empty)
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
            _showArtist = String.Empty;
            //btnPlaylistFolder.Disabled = true;
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
        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (control == btnSearch)
            {
                int activeWindow = (int)GUIWindowManager.ActiveWindow;
                VirtualSearchKeyboard keyBoard = (VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
                keyBoard.Text = String.Empty;
                keyBoard.Reset();
                //keyBoard.KindOfSearch=(int)MediaPortal.Dialogs.VirtualSearchKeyboard.SearchKinds.SEARCH_STARTS_WITH;
                keyboard_TextChanged(0, "");
                keyBoard.TextChanged += new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged); // add the event handler
                keyBoard.DoModal(activeWindow); // show it...
                keyBoard.TextChanged -= new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged);	// remove the handler			
            }

            else if (control == btnPlaylistFolder)
            {
                if (m_strDirectory != m_strPlayListPath)
                {
                    m_strCurrentFolder = m_strDirectory;
                    m_strDirectory = m_strPlayListPath;
                }

                else
                {
                    m_strDirectory = m_strCurrentFolder;
                }

                Console.WriteLine("m_strCurrentFolder:{0}  m_strDirectory:{1}", m_strCurrentFolder, m_strDirectory);
                LoadPlaylistDirectory(m_strDirectory);
                btnSortBy.Disabled = false;
            }

            if (control == btnPlayCd)
            {
                GUIMusicFiles musicFilesWnd = (GUIMusicFiles)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_FILES);
                musicFilesWnd.PlayCD();
            }

            base.OnClicked(controlId, control, actionType);
        }

        protected override void OnRetrieveCoverArt(GUIListItem item)
        {
            if (item.Label == "..") return;
            MediaPortal.Util.Utils.SetDefaultIcons(item);
            if (item.IsRemote) return;
            Song song = item.AlbumInfoTag as Song;
            if (song == null) return;
            if (song.genreId >= 0 && song.albumId < 0 && song.artistId < 0 && song.songId < 0)
            {
                string strThumb = MediaPortal.Util.Utils.GetCoverArt(Thumbs.MusicGenre, item.Label);
                if (System.IO.File.Exists(strThumb))
                {
                    item.IconImage = strThumb;
                    item.IconImageBig = strThumb;
                    item.ThumbnailImage = strThumb;
                }
                else
                {
                    strThumb = MediaPortal.Util.Utils.GetFolderThumb(item.Path);
                    if (System.IO.File.Exists(strThumb))
                    {
                        item.IconImage = strThumb;
                        item.IconImageBig = strThumb;
                        item.ThumbnailImage = strThumb;
                    }
                }
            }
            else if (song.artistId >= 0 && song.albumId < 0 && song.songId < 0)
            {
                string strThumb = MediaPortal.Util.Utils.GetCoverArt(Thumbs.MusicArtists, item.Label);
                if (System.IO.File.Exists(strThumb))
                {
                    item.IconImage = strThumb;
                    item.IconImageBig = strThumb;
                    item.ThumbnailImage = strThumb;
                }
            }
            else if (song.albumId >= 0)
            {
                MusicTag tag = item.MusicTag as MusicTag;
                string strThumb = GUIMusicFiles.GetAlbumThumbName(tag.Artist, tag.Album);
                string albumThumb = strThumb;
                if (System.IO.File.Exists(strThumb))
                {
                    item.IconImage = strThumb;
                    item.IconImageBig = strThumb;
                    item.ThumbnailImage = strThumb;
                }
                else
                {
                    strThumb = MediaPortal.Util.Utils.GetFolderThumb(item.Path);
                    if (System.IO.File.Exists(strThumb))
                    {
                        item.IconImage = strThumb;
                        item.IconImageBig = strThumb;
                        item.ThumbnailImage = strThumb;
                    }

                    // MediaPortal.Util.Utils.GetFolderThumb returns an empty string when item.Path.Length == 0
                    // so we'll pull to info from the db to reconstruct the full album path
                    else if (strThumb.Length == 0)
                    {
                        string albumPath = m_database.GetAlbumPath(song.artistId, song.albumId);

                        if (albumPath.Length > 0)
                        {
                            albumPath = albumPath.TrimEnd(new char[] { '\\' });
                            albumPath += "\\";

                            strThumb = MediaPortal.Util.Utils.GetFolderThumb(albumPath);
                            if (System.IO.File.Exists(strThumb))
                            {
                                item.IconImage = strThumb;
                                item.IconImageBig = strThumb;
                                item.ThumbnailImage = strThumb;

                                // Save a copy to the \thumbs folder so we don't need to do this again
                                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(strThumb);

                                if (bmp != null)
                                    bmp.Save(albumThumb);
                            }
                        }
                    }
                }
            }
            else
            {
                base.OnRetrieveCoverArt(item);
            }
        }

        protected override void OnClick(int iItem)
        {
            GUIListItem item = facadeView.SelectedListItem;
            if (item == null) return;

            if (PlayListFactory.IsPlayList(item.Path))
            {
                LoadPlayList(item.Path);
                return;
            }

            if (item.IsFolder)
            {
                if (item.Label == ".." && item.Path != String.Empty)
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
                        handler.Select(item.AlbumInfoTag as Song);

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
                for (int i = 0; i < (int)facadeView.Count; i++)
                {
                    GUIListItem pItem = facadeView[i];
                    if (pItem.IsFolder)
                    {
                        nFolderCount++;
                        continue;
                    }
                    PlayListItem playlistItem = new Playlists.PlayListItem();
                    playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
                    playlistItem.FileName = pItem.Path;
                    playlistItem.Description = pItem.Label;
                    int iDuration = 0;
                    MusicTag tag = pItem.MusicTag as MusicTag;
                    if (tag != null) iDuration = tag.Duration;
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
            int itemNo = facadeView.SelectedListItemIndex;
            if (item == null) return;

            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null) return;
            dlg.Reset();
            dlg.SetHeading(924); // menu

            dlg.AddLocalizedString(926);    // Add to playlist
            dlg.AddLocalizedString(4557);    // Add all to playlist
            dlg.AddLocalizedString(4551);   // Play next
            dlg.AddLocalizedString(4552);   // Play now
            dlg.AddLocalizedString(4553);   // Show playlist

            if (!item.IsFolder && !item.IsRemote)
            {
                Song song = item.AlbumInfoTag as Song;
                if (song.songId >= 0)
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
            if (dlg.SelectedLabel == -1) return;
            switch (dlg.SelectedId)
            {
                case 4552:  // Play now (clear playlist, play, and jump to Now playing)
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
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
                    break;
                case 930: // add to favorites
                    AddSongToFavorites(item);
                    break;
                case 931:// Rating
                    OnSetRating(facadeView.SelectedListItemIndex);
                    break;
                case 718:// Clear top 100
                    m_database.ResetTop100();
                    LoadDirectory(m_strDirectory);
                    break;
            }
        }

        protected override void OnQueueItem(int iItem)
        {
            CreatePlaylist((Song)((GUIListItem)facadeView[iItem]).AlbumInfoTag);

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
                FilterDefinition def = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];
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
                if (song.songId > 0) AddItemToPlayList(song);
            }
        }
        #endregion

        protected void AddItemToPlayList(Song song)
        {
            if (MediaPortal.Util.Utils.IsAudio(song.FileName) && !PlayListFactory.IsPlayList(song.FileName))
            {
                PlayListItem playlistItem = new PlayListItem();
                playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
                playlistItem.FileName = song.FileName;
                playlistItem.Description = song.Title;
                playlistItem.Duration = song.Duration;

                MusicTag tag = new MusicTag();
                tag.Album = song.Album;
                tag.Artist = song.Artist;
                tag.Duration = song.Duration;
                tag.Genre = song.Genre;
                tag.Rating = song.Rating;
                tag.TimesPlayed = song.TimesPlayed;
                tag.Title = song.Title;
                tag.Track = song.Track;
                tag.Year = song.Year;
                playlistItem.MusicTag = tag;

                playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);
            }
        }

        void keyboard_TextChanged(int kindOfSearch, string data)
        {
            facadeView.Filter(kindOfSearch, data);
        }


        protected override void LoadDirectory(string strNewDirectory)
        {
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

            string strObjects = String.Empty;
            TimeSpan totalPlayingTime = new TimeSpan();

            List<Song> songs = handler.Execute();
            if (songs.Count > 0)    // some songs in there?
            {
                Song song = songs[0];
                if (song.FileName.Length > 0)  // does a filename exits
                {
                    foreach (Share share in _shareList)
                    {
                        if (song.FileName.Contains(share.Path)) // compare it with shares
                        {
                            if (share.Pincode != -1)              // does it have a pincode?
                            {
                                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
                                GUIWindowManager.SendMessage(msg);   // ask for the userinput
                                int iPincode = -1;
                                try
                                {
                                    iPincode = Int32.Parse(msg.Label);
                                }
                                catch (Exception) { }
                                if (iPincode != share.Pincode) songs.Clear();
                                break;
                            }
                        }
                    }
                }
            }

            if (handler.CurrentLevel > 0)
            {
                GUIListItem pItem = new GUIListItem("..");
                pItem.Path = String.Empty;
                pItem.IsFolder = true;
                MediaPortal.Util.Utils.SetDefaultIcons(pItem);
                facadeView.Add(pItem);
            }

            for (int i = 0; i < songs.Count; ++i)
            {
                Song song = songs[i];
                GUIListItem item = new GUIListItem();
                item.Label = song.Title;
                if (handler.CurrentLevel + 1 < handler.MaxLevels)
                    item.IsFolder = true;
                else
                    item.IsFolder = false;
                item.Path = song.FileName;
                item.Duration = song.Duration;

                MusicTag tag = new MusicTag();
                tag.Title = song.Title;
                tag.Album = song.Album;
                tag.Artist = song.Artist;
                tag.Duration = song.Duration;
                tag.Genre = song.Genre;
                tag.Track = song.Track;
                tag.Year = song.Year;
                tag.Rating = song.Rating;
                item.Duration = tag.Duration;
                tag.TimesPlayed = song.TimesPlayed;

                if (item.Duration > 0)
                    totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, item.Duration));
                item.Rating = song.Rating;
                item.Year = song.Year;
                item.AlbumInfoTag = song;
                item.MusicTag = tag;
                item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
                facadeView.Add(item);
            }

            string strSelectedItem = m_history.Get(m_strDirectory);
            int iItem = 0;

            int iTotalItems = facadeView.Count;
            if (facadeView.Count > 0)
            {
                GUIListItem rootItem = facadeView[0];
                if (rootItem.Label == "..") iTotalItems--;
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

            strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
            if (totalPlayingTime.Seconds > 0)
            {
                strObjects = String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(1052),
                            MediaPortal.Util.Utils.SecondsToHMSString((int)totalPlayingTime.TotalSeconds));//songs
            }
            GUIPropertyManager.SetProperty("#itemcount", strObjects);
            SetLabels();
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

            FilterDefinition def = handler.View.Filters[handler.CurrentLevel] as FilterDefinition;
            if (def != null)
            {
                if (def.DefaultView == "List")
                    CurrentView = GUIMusicBaseWindow.View.List;
                if (def.DefaultView == "Icons")
                    CurrentView = GUIMusicBaseWindow.View.Icons;
                if (def.DefaultView == "Big Icons")
                    CurrentView = GUIMusicBaseWindow.View.LargeIcons;
                if (def.DefaultView == "Albums")
                    CurrentView = GUIMusicBaseWindow.View.Albums;
                if (def.DefaultView == "Filmstrip")
                    CurrentView = GUIMusicBaseWindow.View.FilmStrip;
            }
            SwitchView();
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
            if(!IsSortableView(handler.View, handler.CurrentLevel))
            {
                for (int i = 0; i < facadeView.Count; ++i)
                {
                    GUIListItem item = facadeView[i];
                    MusicTag tag = (MusicTag)item.MusicTag;
                    if (tag != null)
                    {
                        int playCount = tag.TimesPlayed;
                        string duration = MediaPortal.Util.Utils.SecondsToHMSString(playCount * tag.Duration);
                        item.Label = string.Format("{0:00}. {1} - {2}", playCount, tag.Artist, tag.Title);
                        item.Label2 = duration;
                    }
                }
            }

            else
                base.SetLabels();

            for (int i = 0; i < facadeView.Count; ++i)
            {
                GUIListItem item = facadeView[i];
                handler.SetLabel(item.AlbumInfoTag as Song, ref item);
            }
        }

        void OnThreadMessage(GUIMessage message)
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
                    if (MediaPortal.Util.Utils.IsAudio(strFile))
                    {
                        MusicDatabase dbs = new MusicDatabase();
                        dbs.IncrTop100CounterByFileName(strFile);
                    }
                    break;
            }
        }

        static public void SelectArtist(string artist)
        {
            _showArtist = artist;
        }

        void AddItemToPlayList(GUIListItem pItem)
        {
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            AddItemToPlayList(pItem, ref playList);
        }

        void AddSongToPlayList(Song song)
        {
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            AddSongToPlayList(song, playList);
        }

        void AddSongToPlayList(Song song, PlayList playList)
        {
            MusicTag tag = new MusicTag();
            tag.Album = song.Album;
            tag.Artist = song.Artist;
            tag.Duration = song.Duration;
            tag.Genre = song.Genre;
            tag.Rating = song.Rating;
            tag.TimesPlayed = song.TimesPlayed;
            tag.Title = song.Title;
            tag.Track = song.Track;
            tag.Year = song.Year;

            PlayListItem playlistItem = new PlayListItem();
            playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
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
                AddSongToPlayList(song, playList);
        }

        void AddAlbumsToPlayList(List<Song> songs)
        {
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            AddAlbumsToPlayList(songs, playList);
        }

        void AddAlbumsToPlayList(List<Song> albums, PlayList playList)
        {
            foreach (Song album in albums)
            {
                List<Song> albumSongs = new List<Song>();
                m_database.GetSongsByArtistAlbum(album.artistId, album.albumId, ref albumSongs);

                foreach (Song albumSong in albumSongs)
                {
                    AddSongToPlayList(albumSong, playList);
                }
            }
        }

        void AddItemToPlayList(GUIListItem pItem, ref PlayList playList)
        {
            if (playList == null || pItem == null)
                return;

            if (m_database == null)
                m_database = new MusicDatabase();

            if (pItem.IsFolder)
            {
                if (pItem.Label == "..") return;
                string artist = string.Empty;

                if (pItem.AlbumInfoTag != null)
                {
                    List<Song> songs = new List<Song>();
                    Song s = (Song)pItem.AlbumInfoTag;
                    bool isArtistItem = s.artistId != -1 && s.albumId == -1 && s.genreId == -1 && s.Year <= 0;
                    bool isAlbumItem = s.albumId != -1;
                    bool isGenreItem = s.genreId != -1;
                    bool isYearItem = s.Year != -1 && s.artistId == -1 && s.albumId == -1 && s.genreId == -1;

                    if (isArtistItem)
                    {
                        m_database.GetSongsByArtist(s.artistId, ref songs);
                        AddAlbumsToPlayList(songs, playList);
                    }

                    else if (isAlbumItem)
                    {
                        songs.Add(s);
                        AddAlbumsToPlayList(songs, playList);
                    }

                    else if (isGenreItem)
                    {
                        m_database.GetSongsByGenre(s.genreId, ref songs);
                        AddSongsToPlayList(songs, playList);
                    }

                    else if (isYearItem)
                    {
                        m_database.GetSongsByYear(s.Year, ref songs);
                        AddSongsToPlayList(songs, playList);
                    }
                }
            }

            // item is not a folder
            else
            {
                if (pItem.AlbumInfoTag != null)
                {
                    Song song = (Song)pItem.AlbumInfoTag;
                    MusicTag tag = new MusicTag();
                    tag.Title = song.Title;
                    tag.Album = song.Album;
                    tag.Artist = song.Artist;
                    tag.Duration = song.Duration;
                    tag.Genre = song.Genre;
                    tag.Track = song.Track;
                    tag.Year = song.Year;
                    tag.Rating = song.Rating;

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
                return;

            if (PlayListFactory.IsPlayList(pItem.Path))
                return;
            
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

            if (playList == null)
                return;

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
                return;
            
            if (PlayListFactory.IsPlayList(pItem.Path))
                return;
            
            int playStartIndex = 0;
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

            if (playList == null)
                return;

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
                            playStartIndex--;
                    }
                }
            }

            else
                AddItemToPlayList(pItem, ref playList);

            if (playList.Count > 0)
            {
                playlistPlayer.Reset();
                playlistPlayer.Play(playStartIndex);

                if (PlayNowJumpToWindowID != -1)
                {
                    if (PlayNowJumpToWindowID == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
                    {
                        GUIMusicPlayingNow nowPlayingWnd = (GUIMusicPlayingNow)GUIWindowManager.GetWindow(PlayNowJumpToWindowID);

                        if (nowPlayingWnd != null)
                            nowPlayingWnd.MusicWindow = this;
                    }

                    GUIWindowManager.ActivateWindow(PlayNowJumpToWindowID);
                }

                if (!g_Player.Playing)
                {
                    playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
                    playlistPlayer.Play(playStartIndex);
                }
            }
        }

        // Need to remove this and allow the remote plugins to handle anti-repeat logic.
        // We also need some way for MP to handle anti-repeat for keyboard events
        private bool AntiRepeatActive()
        {
            TimeSpan ts = DateTime.Now - Previous_ACTION_PLAY_Time;

            // Ignore closely spaced calls due to rapid-fire ACTION_PLAY events...
            if (ts < AntiRepeatInterval)
                return true;

            else
                return false;
        }

        protected void LoadPlaylistDirectory(string strNewDirectory)
        {
            GUIListItem SelectedItem = facadeView.SelectedListItem;

            if (SelectedItem != null)
            {
                if (SelectedItem.IsFolder && SelectedItem.Label != "..")
                    m_history.Set(SelectedItem.Label, m_strDirectory);
            }

            m_strDirectory = strNewDirectory;

            GUIControl.ClearControl(GetID, facadeView.GetID);

            string strObjects = String.Empty;

            List<GUIListItem> itemlist = m_directory.GetDirectoryExt(m_strPlayListPath);
            //int iItem = 0;

            foreach (GUIListItem item in itemlist)
            {
                if (item.Label == ".." && m_strDirectory == m_strPlayListPath)
                    continue;

                facadeView.Add(item);
            }

            int iTotalItems = itemlist.Count;

            if (itemlist.Count > 0)
            {
                GUIListItem rootItem = itemlist[0];
                if (rootItem.Label == "..") iTotalItems--;
            }

            strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
            GUIPropertyManager.SetProperty("#itemcount", strObjects);

            if (m_iItemSelected >= 0)
                GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);

            else if (itemlist.Count > 1)
                GUIControl.SelectItemControl(GetID, facadeView.GetID, 1);
        }

        private void OnQueueAllItems()
        {
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            int index = Math.Max(playlistPlayer.CurrentSong, 0);

            for (int i = 0; i < facadeView.Count; i++)
            {
                GUIListItem item = facadeView[i];

                if (item.Label != "...")
                    AddItemToPlayList(item);
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
                facadeView.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));

            UpdateButtonStates();
            btnSortBy.Disabled = !isSortable;
        }
    }
}