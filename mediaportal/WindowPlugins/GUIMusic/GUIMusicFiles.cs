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
using System.Xml.Serialization;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

namespace MediaPortal.GUI.Music
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class GUIMusicFiles : GUIMusicBaseWindow, ISetupForm, IShowPlugin
    {
         class TrackComparer : IComparer<GUIListItem>
        {
            public int Compare(GUIListItem item1, GUIListItem item2)
            {
                // Is this a top level artist folder?  If so, sort by path.
                if (item1.MusicTag == null || item2.MusicTag == null)
                    return item1.Path.CompareTo(item2.Path);

                // Is it album folder or a song file. If album folder, sort by album name. Otherwise, sort by track number
                else
                {
                    MusicTag tag1 = (MusicTag)item1.MusicTag;
                    MusicTag tag2 = (MusicTag)item2.MusicTag;

                    if (tag1.Track < 1)
                        return CompareAlbumNames(tag1.Album, tag2.Album);

                    else
                        return CompareTracks(tag1.Track, tag2.Track);
                }
            }

            private int CompareTracks(int track1, int track2)
            {
                return track1.CompareTo(track2);
            }

            private int CompareAlbumNames(string albumTitle1, string albumTitle2)
            {
                if (albumTitle1 == null || albumTitle2 == null)
                    return 0;

                return albumTitle1.CompareTo(albumTitle2);
            }
        }


        [Serializable]
        public class MapSettings
        {
            protected int _SortBy;
            protected int _ViewAs;
            protected bool _Stack;
            protected bool _SortAscending;

            public MapSettings()
            {
                _SortBy = 0;//name
                _ViewAs = 0;//list
                _Stack = true;
                _SortAscending = true;
            }


            [XmlElement("SortBy")]
            public int SortBy
            {
                get { return _SortBy; }
                set { _SortBy = value; }
            }

            [XmlElement("ViewAs")]
            public int ViewAs
            {
                get { return _ViewAs; }
                set { _ViewAs = value; }
            }

            [XmlElement("SortAscending")]
            public bool SortAscending
            {
                get { return _SortAscending; }
                set { _SortAscending = value; }
            }
        }


        MapSettings _MapSettings = new MapSettings();

        DirectoryHistory m_history = new DirectoryHistory();
        string m_strDirectory = String.Empty;
        string m_strDirectoryStart = String.Empty;
        int m_iItemSelected = -1;
        GUIListItem m_itemItemSelected = null;
        VirtualDirectory m_directory = new VirtualDirectory();
        bool m_bScan = false;
        bool m_bAutoShuffle = true;
        string m_strDiscId = String.Empty;
        string m_strPlayListPath = String.Empty;
        string m_strCurrentFolder = String.Empty;
        int m_iSelectedAlbum = -1;
        static Freedb.CDInfoDetail m_musicCD = null;
        // File menu
        string m_strDestination = String.Empty;
        bool m_bFileMenuEnabled = false;
        string m_strFileMenuPinCode = String.Empty;

        private DateTime Previous_ACTION_PLAY_Time = DateTime.Now;
        private TimeSpan AntiRepeatInterval = new TimeSpan(0, 0, 0, 0, 500);
        // SV private int PlayNowJumpToWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;

        [SkinControlAttribute(8)]
        protected GUIButtonControl btnPlaylist;
        [SkinControlAttribute(9)]
        protected GUIButtonControl btnPlayCd;
        [SkinControlAttribute(10)]
        protected GUIButtonControl btnPlaylistFolder;

        public GUIMusicFiles()
        {
            GetID = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;

            m_directory.AddDrives();
            m_directory.SetExtensions(Utils.AudioExtensions);

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                MusicState.StartWindow = xmlreader.GetValueAsInt("music", "startWindow", GetID);
                MusicState.View = xmlreader.GetValueAsString("music", "startview", String.Empty);
            }

            //
            //			MusicViewHandler handler = new MusicViewHandler();
            //			handler.CurrentView="Genres";
            //			ArrayList list = handler.Execute();
            //			handler.Select((Song)list[15]);// genre=pop 5
            //			list = handler.Execute();
            //			handler.Select((Song)list[2]);// album=wanadoo top 40 2002 192
            //			list = handler.Execute();
            //			handler.CurrentLevel--;
            //			list = handler.Execute();

            GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
        }

        // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when 
        // the player is not playing)...
        void GUIWindowManager_OnNewAction(Action action)
        {
            if ((action.wID == Action.ActionType.ACTION_PLAY 
                || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
                && GUIWindowManager.ActiveWindow == GetID)
            {
                GUIListItem item = facadeView.SelectedListItem;

                if (AntiRepeatActive() || item == null || item.Label == ".." || IsShare(item) || IsDVD(item.Path))
                    return;

                OnPlayNow(item);
            }
        }

        static public Freedb.CDInfoDetail MusicCD
        {
            get { return m_musicCD; }
            set { m_musicCD = value; }
        }


        #region Serialisation
        protected override void LoadSettings()
        {
            base.LoadSettings();
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                m_bFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
                m_strFileMenuPinCode = xmlreader.GetValueAsString("filemenu", "pincode", String.Empty);

                m_strPlayListPath = xmlreader.GetValueAsString("music", "playlists", String.Empty);
                m_strPlayListPath = Utils.RemoveTrailingSlash(m_strPlayListPath);

                m_bAutoShuffle = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", true);

                string strDefault = xmlreader.GetValueAsString("music", "default", String.Empty);
                m_directory.Clear();
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
                        if (strDefault == share.Name)
                        {
                            share.Default = true;
                            if (m_strDirectory.Length == 0)
                            {
                                m_strDirectory = share.Path;
                                m_strDirectoryStart = share.Path;
                            }
                        }
                        m_directory.Add(share);
                    }
                    else break;
                }
            }
        }
        #endregion

        #region overrides

        protected override string SerializeName
        {
            get
            {
                return "mymusic";
            }
        }
        protected override bool AllowView(View view)
        {
            if (view == View.Albums) return false;
            return base.AllowView(view);
        }


        public override bool Init()
        {
            m_strDirectory = String.Empty;

            bool bResult = Load(GUIGraphicsContext.Skin + @"\mymusicsongs.xml");
            return bResult;
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
                        if (item != null)
                        {
                            if (item.IsFolder && item.Label == "..")
                            {
                                if (m_strDirectory != m_strDirectoryStart)
                                {
                                    LoadDirectory(item.Path);
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (facadeView.Focus)
                    {
                        LoadDirectory(m_strCurrentFolder);
                        return;
                    }
                }
            }
            if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
            {
                GUIListItem item = facadeView[0];
                if (item != null)
                {
                    if (item.IsFolder && item.Label == "..")
                    {
                        LoadDirectory(item.Path);
                    }
                }
                return;
            }
            
            base.OnAction(action);
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            if (MusicState.StartWindow != GetID)
            {
                if (MusicState.StartWindow != (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST)
                {
                    GUIWindowManager.ReplaceWindow((int)GUIWindow.Window.WINDOW_MUSIC_GENRE);
                    return;
                }
            }
            LoadFolderSettings(m_strDirectory);
            LoadDirectory(m_strDirectory);
        }
        protected override void OnPageDestroy(int newWindowId)
        {
            m_iItemSelected = facadeView.SelectedListItemIndex;
            SaveFolderSettings(m_strDirectory);
            base.OnPageDestroy(newWindowId);
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {

            if (control == btnPlayCd)
            {
                for (char c = 'C'; c <= 'Z'; c++)
                {
                    if ((Utils.GetDriveType(c + ":") & 5) == 5)
                    {
                        OnPlayCD(c + ":", false);
                        break;
                    }
                }
            }
            if (control == btnPlaylistFolder)
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
                LoadDirectory(m_strDirectory);
            }
            base.OnClicked(controlId, control, actionType);
        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_PLAY_AUDIO_CD:
                    if (message.Label2 == "Ask")
                    {
                        Log.Write("OnPlayCD (false), vragen dus.");
                        OnPlayCD(message.Label, false);
                    }
                    else
                    {
                        Log.Write("OnPlayCD (true), niet vragen dus");
                        OnPlayCD(message.Label, true);
                    }

                    break;

                case GUIMessage.MessageType.GUI_MSG_CD_REMOVED:
                    GUIMusicFiles.MusicCD = null;
                    if (g_Player.Playing && Utils.IsCDDA(g_Player.CurrentFile))
                    {
                        g_Player.Stop();
                    }
                    if (GUIWindowManager.ActiveWindow == GetID)
                    {
                        if (Utils.IsDVD(m_strDirectory))
                        {
                            m_strDirectory = String.Empty;
                            LoadDirectory(m_strDirectory);
                        }
                    }
                    break;

                case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
                    OnPlayCD(message.Label, false);
                    break;

                case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
                    facadeView.OnMessage(message);
                    break;

                case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
                    facadeView.OnMessage(message);
                    break;

                case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
                    m_strDirectory = message.Label;
                    LoadDirectory(m_strDirectory);
                    break;

                case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
                case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
                    if (m_strDirectory == String.Empty || m_strDirectory.Substring(0, 2) == message.Label)
                    {
                        m_strDirectory = String.Empty;
                        LoadDirectory(m_strDirectory);
                    }
                    break;
            }
            return base.OnMessage(message);
        }

        ////protected override void OnShowContextMenu()
        ////{
        ////    GUIListItem item = facadeView.SelectedListItem;
        ////    m_itemItemSelected = item;
        ////    int itemNo = facadeView.SelectedListItemIndex;
        ////    if (item == null) return;


        ////    GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        ////    if (dlg == null) return;
        ////    dlg.Reset();
        ////    dlg.SetHeading(924); // menu

        ////    if ((!item.IsFolder) && (Utils.getDriveType(item.Path.Substring(0, 2)) == 5))
        ////    {
        ////        dlg.AddLocalizedString(1100); //Import CD
        ////        dlg.AddLocalizedString(1101); //Import Track
        ////        if (MusicImport.MusicImport.Ripping)
        ////            dlg.AddLocalizedString(1102); //Cancel Import
        ////    }

        ////    if (!m_directory.IsRemote(m_strDirectory)) dlg.AddLocalizedString(102); //Scan
        ////    //dlg.AddLocalizedString(654); //Eject

        ////    if (!facadeView.Focus)
        ////    {
        ////        // control view has no focus
        ////        dlg.AddLocalizedString(368); //IMDB
        ////    }
        ////    else
        ////    {
        ////        if ((System.IO.Path.GetFileName(item.Path) != String.Empty) || Utils.IsDVD(item.Path))
        ////        {
        ////            dlg.AddLocalizedString(928); //find coverart
        ////            dlg.AddLocalizedString(4521); //Show Album Info
        ////            dlg.AddLocalizedString(926); //Queue     
        ////            if (!item.IsFolder && !item.IsRemote)
        ////            {
        ////                dlg.AddLocalizedString(930); //Add to favorites
        ////                dlg.AddLocalizedString(931); //Rating
        ////            }
        ////        }

        ////        if (!item.IsFolder || Utils.IsDVD(item.Path))
        ////            dlg.AddLocalizedString(208); //play

        ////        if (Utils.getDriveType(item.Path) == 5)
        ////            dlg.AddLocalizedString(654); //Eject

        ////        int iPincodeCorrect;
        ////        if (!m_directory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && m_bFileMenuEnabled)
        ////            dlg.AddLocalizedString(500); // FileMenu
        ////    }

        ////    if (g_Player.Playing && g_Player.IsMusic)
        ////    {
        ////        string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
        ////        if (artist.Length > 0)
        ////        {
        ////            dlg.AddLocalizedString(751); // Show all songs of this artist
        ////        }
        ////    }

        ////    dlg.DoModal(GetID);
        ////    if (dlg.SelectedId == -1) return;
        ////    switch (dlg.SelectedId)
        ////    {
        ////        case 928: // find coverart
        ////            OnFindCoverArt(itemNo);
        ////            break;

        ////        case 4521: // Show album info
        ////            OnInfo(itemNo);
        ////            break;

        ////        case 208: // play
        ////            if (Utils.getDriveType(item.Path) != 5)
        ////                OnClick(itemNo);
        ////            else
        ////                //Playing the CD from the context menu
        ////                OnPlayCD(item.Path, false);
        ////            break;

        ////        case 926: // add to playlist
        ////            OnQueueItem(itemNo);
        ////            break;

        ////        case 136: // show playlist
        ////            m_iItemSelected = facadeView.SelectedListItemIndex;
        ////            SaveFolderSettings(m_strDirectory);
        ////            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        ////            break;

        ////        case 654: // Eject
        ////            if (Utils.getDriveType(item.Path) != 5) Utils.EjectCDROM();
        ////            else Utils.EjectCDROM(System.IO.Path.GetPathRoot(item.Path));
        ////            LoadDirectory(String.Empty);
        ////            break;

        ////        case 930: // add to favorites
        ////            AddSongToFavorites(item);
        ////            break;

        ////        case 931:// Rating
        ////            OnSetRating(facadeView.SelectedListItemIndex);
        ////            break;

        ////        case 102:
        ////            OnScan();
        ////            break;

        ////        case 500: // File menu
        ////            {
        ////                // get pincode
        ////                if (m_strFileMenuPinCode != String.Empty)
        ////                {
        ////                    string strUserCode = String.Empty;
        ////                    if (GetUserInputString(ref strUserCode) && strUserCode == m_strFileMenuPinCode)
        ////                    {
        ////                        OnShowFileMenu();
        ////                    }
        ////                }
        ////                else
        ////                    OnShowFileMenu();
        ////            }
        ////            break;

        ////        case 1100: // Import CD
        ////            OnAction(new Action(Action.ActionType.ACTION_IMPORT_DISC, 0, 0));
        ////            break;

        ////        case 1101: // Import seltected track
        ////            OnAction(new Action(Action.ActionType.ACTION_IMPORT_TRACK, 0, 0));
        ////            break;

        ////        case 1102: // Cancel CD import
        ////            OnAction(new Action(Action.ActionType.ACTION_CANCEL_IMPORT, 0, 0));
        ////            break;

        ////        case 751: // Show all songs from this artist
        ////            {
        ////                string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
        ////                int viewNr = -1;
        ////                for (int x = 0; x < handler.Views.Count; ++x)
        ////                {
        ////                    ViewDefinition view = (ViewDefinition)handler.Views[x];
        ////                    if (view.Name.ToLower().IndexOf("artist") >= 0)
        ////                    {
        ////                        viewNr = x;
        ////                    }
        ////                }
        ////                if (viewNr < 0) return;
        ////                ViewDefinition selectedView = (ViewDefinition)handler.Views[viewNr];
        ////                handler.CurrentView = selectedView.Name;
        ////                MusicState.View = selectedView.Name;
        ////                GUIMusicGenres.SelectArtist(artist);
        ////                int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
        ////                if (GetID != nNewWindow)
        ////                {
        ////                    MusicState.StartWindow = nNewWindow;
        ////                    if (nNewWindow != GetID)
        ////                    {
        ////                        GUIWindowManager.ReplaceWindow(nNewWindow);
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    LoadDirectory(String.Empty);
        ////                    if (facadeView.Count <= 0)
        ////                    {
        ////                        GUIControl.FocusControl(GetID, btnViewAs.GetID);
        ////                    }
        ////                }
        ////            }
        ////            break;
        ////    }
        ////}

        protected override void OnShowContextMenu()
        {
            GUIListItem item = facadeView.SelectedListItem;
            m_itemItemSelected = item;
            int itemNo = facadeView.SelectedListItemIndex;
            if (item == null) return;

            bool isCD = IsCD(item.Path);
            bool isDVD = IsDVD(item.Path);

            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null) return;
            dlg.Reset();
            dlg.SetHeading(924); // menu

            if ((!item.IsFolder) && (Utils.getDriveType(item.Path.Substring(0, 2)) == 5))
            {
                dlg.AddLocalizedString(1100); //Import CD
                dlg.AddLocalizedString(1101); //Import Track
                if (MusicImport.MusicImport.Ripping)
                    dlg.AddLocalizedString(1102); //Cancel Import
            }

            if (!m_directory.IsRemote(m_strDirectory)) dlg.AddLocalizedString(102); //Scan
            //dlg.AddLocalizedString(654); //Eject

            if (!facadeView.Focus)
            {
                // control view has no focus
                dlg.AddLocalizedString(368); //IMDB
            }
            else
            {
                if ((System.IO.Path.GetFileName(item.Path) != String.Empty) || isCD && !isDVD)
                {
                    dlg.AddLocalizedString(928);    //find coverart
                    dlg.AddLocalizedString(4521);   //Show Album Info
                    dlg.AddLocalizedString(926);    // Add to playlist     
                    dlg.AddLocalizedString(4551);   // Play next
                    dlg.AddLocalizedString(4552);   // Play now

                    if (isCD)
                        dlg.AddLocalizedString(890);   // Play CD


                    if (!item.IsFolder && !item.IsRemote)
                    {
                        dlg.AddLocalizedString(930); //Add to favorites
                        dlg.AddLocalizedString(931); //Rating
                    }
                }

                if (Utils.getDriveType(item.Path) == 5)
                    dlg.AddLocalizedString(654); //Eject

                int iPincodeCorrect;
                if (!m_directory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && m_bFileMenuEnabled)
                    dlg.AddLocalizedString(500); // FileMenu
            }

            if (g_Player.Playing && g_Player.IsMusic)
            {
                string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                if (artist.Length > 0)
                {
                    dlg.AddLocalizedString(751); // Show all songs of this artist
                }
            }

            dlg.DoModal(GetID);
            if (dlg.SelectedId == -1) return;
            switch (dlg.SelectedId)
            {
                case 928: // find coverart
                    OnFindCoverArt(itemNo);
                    break;

                case 4521: // Show album info
                    OnInfo(itemNo);
                    break;

                case 926: // add to playlist
                    OnQueueItem(itemNo);
                    break;

                case 4551: // Play next
                    OnPlayNext(item);
                    break;

                case 4552: // Play now
                    OnPlayNow(item);
                    break;

                case 890:
                    OnPlayCD(item.Path, false);
                    break;

                case 136: // show playlist
                    m_iItemSelected = facadeView.SelectedListItemIndex;
                    SaveFolderSettings(m_strDirectory);
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
                    break;

                case 654: // Eject
                    if (Utils.getDriveType(item.Path) != 5) Utils.EjectCDROM();
                    else Utils.EjectCDROM(System.IO.Path.GetPathRoot(item.Path));
                    LoadDirectory(String.Empty);
                    break;

                case 930: // add to favorites
                    AddSongToFavorites(item);
                    break;

                case 931:// Rating
                    OnSetRating(facadeView.SelectedListItemIndex);
                    break;

                case 102:
                    OnScan();
                    break;

                case 500: // File menu
                    {
                        // get pincode
                        if (m_strFileMenuPinCode != String.Empty)
                        {
                            string strUserCode = String.Empty;
                            if (GetUserInputString(ref strUserCode) && strUserCode == m_strFileMenuPinCode)
                            {
                                OnShowFileMenu();
                            }
                        }
                        else
                            OnShowFileMenu();
                    }
                    break;

                case 1100: // Import CD
                    OnAction(new Action(Action.ActionType.ACTION_IMPORT_DISC, 0, 0));
                    break;

                case 1101: // Import seltected track
                    OnAction(new Action(Action.ActionType.ACTION_IMPORT_TRACK, 0, 0));
                    break;

                case 1102: // Cancel CD import
                    OnAction(new Action(Action.ActionType.ACTION_CANCEL_IMPORT, 0, 0));
                    break;

                case 751: // Show all songs from this artist
                    {
                        string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                        int viewNr = -1;
                        for (int x = 0; x < handler.Views.Count; ++x)
                        {
                            ViewDefinition view = (ViewDefinition)handler.Views[x];
                            if (view.Name.ToLower().IndexOf("artist") >= 0)
                            {
                                viewNr = x;
                            }
                        }
                        if (viewNr < 0) return;
                        ViewDefinition selectedView = (ViewDefinition)handler.Views[viewNr];
                        handler.CurrentView = selectedView.Name;
                        MusicState.View = selectedView.Name;
                        GUIMusicGenres.SelectArtist(artist);
                        int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
                        if (GetID != nNewWindow)
                        {
                            MusicState.StartWindow = nNewWindow;
                            if (nNewWindow != GetID)
                            {
                                GUIWindowManager.ReplaceWindow(nNewWindow);
                            }
                        }
                        else
                        {
                            LoadDirectory(String.Empty);
                            if (facadeView.Count <= 0)
                            {
                                GUIControl.FocusControl(GetID, btnViewAs.GetID);
                            }
                        }
                    }
         
                    break;
            }
        }

        bool GetUserInputString(ref string sString)
        {
            VirtualSearchKeyboard keyBoard = (VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
            keyBoard.Reset();
            keyBoard.Text = sString;
            keyBoard.DoModal(GetID); // show it...
            if (keyBoard.IsConfirmed) sString = keyBoard.Text;
            return keyBoard.IsConfirmed;
        }

        void OnShowFileMenu()
        {
            GUIListItem item = m_itemItemSelected;
            if (item == null) return;
            if (item.IsFolder && item.Label == "..") return;

            // init
            GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
            if (dlgFile == null) return;

            // File operation settings
            dlgFile.SetSourceItem(item);
            dlgFile.SetSourceDir(m_strDirectory);
            dlgFile.SetDestinationDir(m_strDestination);
            dlgFile.SetDirectoryStructure(m_directory);
            dlgFile.DoModal(GetID);
            m_strDestination = dlgFile.GetDestinationDir();

            //final		
            if (dlgFile.Reload())
            {
                LoadDirectory(m_strDirectory);
                if (m_iItemSelected >= 0)
                {
                    GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
                }
            }

            dlgFile.DeInit();
            dlgFile = null;
        }

        protected override void OnClick(int iItem)
        {
            GUIListItem item = facadeView.SelectedListItem;
            if (item == null) return;
            if (item.IsFolder)
            {
                m_iItemSelected = -1;
                LoadDirectory(item.Path);
            }
            else
            {
                if (m_directory.IsRemote(item.Path))
                {
                    if (!m_directory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
                    {
                        if (!m_directory.ShouldWeDownloadFile(item.Path)) return;
                        if (!m_directory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
                        {
                            //show message that we are unable to download the file
                            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
                            msg.Param1 = 916;
                            msg.Param2 = 920;
                            msg.Param3 = 0;
                            msg.Param4 = 0;
                            GUIWindowManager.SendMessage(msg);

                            return;
                        }
                    }
                    return;
                }

                if (PlayListFactory.IsPlayList(item.Path))
                {
                    LoadPlayList(item.Path);
                    return;
                }
                //play and add current directory to temporary playlist
                int nFolderCount = 0;
                int nRemoteCount = 0;
                playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
                playlistPlayer.Reset();
                List<GUIListItem> queueItems = new List<GUIListItem>();
                for (int i = 0; i < (int)facadeView.Count; i++)
                {
                    GUIListItem pItem = facadeView[i];
                    if (pItem.IsFolder)
                    {
                        nFolderCount++;
                        continue;
                    }
                    if (pItem.IsRemote)
                    {
                        nRemoteCount++;
                        continue;
                    }
                    if (!PlayListFactory.IsPlayList(pItem.Path))
                    {
                        queueItems.Add(pItem);
                    }
                    else
                    {
                        if (i < facadeView.SelectedListItemIndex) nFolderCount++;
                        continue;
                    }
                }
                m_bScan = true;
                OnRetrieveMusicInfo(ref queueItems);
                m_database.CheckVariousArtistsAndCoverArt();
                m_bScan = false;

                foreach (GUIListItem queueItem in queueItems)
                {
                    PlayListItem playlistItem = new Playlists.PlayListItem();
                    playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
                    playlistItem.FileName = queueItem.Path;
                    playlistItem.Description = queueItem.Label;
                    playlistItem.Duration = queueItem.Duration;
                    playlistItem.MusicTag = queueItem.MusicTag;
                    playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
                }

                //	Save current window and directory to know where the selected item was
                MusicState.TempPlaylistWindow = GetID;
                MusicState.TempPlaylistDirectory = m_strDirectory;

                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_TEMP;
                playlistPlayer.Play(item.Path);
            }
        }

        protected override void OnQueueItem(int iItem)
        {
            // add item 2 playlist
            GUIListItem pItem = facadeView[iItem];
            if (pItem == null) return;
            if (pItem.IsRemote) return;
            if (PlayListFactory.IsPlayList(pItem.Path))
            {
                LoadPlayList(pItem.Path);
                return;
            }
            AddItemToPlayList(pItem);

            //move to next item
            GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem + 1);
            if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0 && !g_Player.Playing)
            {
                playlistPlayer.Reset();
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
                playlistPlayer.Play(0);
            }

        }

        #endregion

        void OnPlayCD(string strDriveLetter, bool AskForAlbum)
        {
            // start playing current CD        
            PlayList list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP);
            list.Clear();

            list = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            list.Clear();
            GUIListItem pItem = new GUIListItem();
            pItem.Path = strDriveLetter;
            pItem.IsFolder = true;
            m_bScan = AskForAlbum;
            AddItemToPlayList(pItem);
            m_bScan = false;
            if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
            {
                // waeberd: mantis #470
                if (g_Player.Playing)
                {
                    // SV: this causes a problem! Once the player is stopped music file playback cannot be restarted
                    // without exiting and re-entering My Music.  Suspect the issue has to do with _player.Release()
                    // and _player = null;
                    // g_Player.Stop();
                }
                playlistPlayer.Reset();
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
                playlistPlayer.Play(0);
            }
        }

        void DisplayFilesList(int searchKind, string strSearchText)
        {

            string strObjects = String.Empty;
            GUIControl.ClearControl(GetID, facadeView.GetID);
            List<GUIListItem> itemlist = new List<GUIListItem>();
            m_database.GetSongs(searchKind, strSearchText, ref itemlist);
            // this will set all to move up
            // from a search result
            m_history.Set(m_strDirectory, m_strDirectory); //save where we are
            GUIListItem dirUp = new GUIListItem("..");
            dirUp.Path = m_strDirectory; // to get where we are
            dirUp.IsFolder = true;
            dirUp.ThumbnailImage = String.Empty;
            dirUp.IconImage = "defaultFolderBack.png";
            dirUp.IconImageBig = "defaultFolderBackBig.png";
            itemlist.Insert(0, dirUp);
            //
            OnRetrieveMusicInfo(ref itemlist);
            foreach (GUIListItem item in itemlist)
            {
                item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
                item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
                GUIControl.AddListItemControl(GetID, facadeView.GetID, item);
            }
            OnSort();
            int iTotalItems = itemlist.Count;
            if (itemlist.Count > 0)
            {
                GUIListItem rootItem = itemlist[0];
                if (rootItem.Label == "..") iTotalItems--;
            }

            strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
            GUIPropertyManager.SetProperty("#itemcount", strObjects);

        }

        void LoadFolderSettings(string strDirectory)
        {
            if (strDirectory == String.Empty) strDirectory = "root";
            object o;
            FolderSettings.GetFolderSetting(strDirectory, "MusicFiles", typeof(GUIMusicFiles.MapSettings), out o);
            if (o != null)
            {
                _MapSettings = o as MapSettings;
                if (_MapSettings == null) _MapSettings = new MapSettings();
                CurrentSortAsc = _MapSettings.SortAscending;
                CurrentSortMethod = (MusicSort.SortMethod)_MapSettings.SortBy;
                currentView = (View)_MapSettings.ViewAs;
            }
            else
            {
                Share share = m_directory.GetShare(strDirectory);
                if (share != null)
                {
                    if (_MapSettings == null) _MapSettings = new MapSettings();
                    CurrentSortAsc = _MapSettings.SortAscending;
                    CurrentSortMethod = (MusicSort.SortMethod)_MapSettings.SortBy;
                    currentView = (View)share.DefaultView;
                }
            }
            SwitchView();
            UpdateButtonStates();
        }
        
        void SaveFolderSettings(string strDirectory)
        {
            if (strDirectory == String.Empty) strDirectory = "root";
            _MapSettings.SortAscending = CurrentSortAsc;
            _MapSettings.SortBy = (int)CurrentSortMethod;
            _MapSettings.ViewAs = (int)currentView;
            FolderSettings.AddFolderSetting(strDirectory, "MusicFiles", typeof(GUIMusicFiles.MapSettings), _MapSettings);
        }

        protected override void LoadDirectory(string strNewDirectory)
        {
            GUIListItem SelectedItem = facadeView.SelectedListItem;
            if (SelectedItem != null)
            {
                if (SelectedItem.IsFolder && SelectedItem.Label != "..")
                {
                    m_history.Set(SelectedItem.Label, m_strDirectory);
                }
            }
            if (strNewDirectory != m_strDirectory && _MapSettings != null)
            {
                SaveFolderSettings(m_strDirectory);
            }

            if (strNewDirectory != m_strDirectory || _MapSettings == null)
            {
                LoadFolderSettings(strNewDirectory);
            }
            m_strDirectory = strNewDirectory;
            GUIControl.ClearControl(GetID, facadeView.GetID);

            string strObjects = String.Empty;
            TimeSpan totalPlayingTime = new TimeSpan();

            List<GUIListItem> itemlist = m_directory.GetDirectoryExt(m_strDirectory);

            string strSelectedItem = m_history.Get(m_strDirectory);
            int iItem = 0;
            OnRetrieveMusicInfo(ref itemlist);
            foreach (GUIListItem item in itemlist)
            {
                MusicTag tag = item.MusicTag as MusicTag;
                if (tag != null)
                {
                    if (tag.Duration > 0)
                        totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
                }
                if (item.Label == ".." && m_strDirectory == m_strPlayListPath) continue;
                item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
                item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
                facadeView.Add(item);
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

            if (m_iItemSelected >= 0)
            {
                GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iItemSelected);
            }
        }

        void AddItemToPlayList(GUIListItem pItem)
        {
            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
            AddItemToPlayList(pItem, ref playList);
        }

        void AddItemToPlayList(GUIListItem pItem, ref PlayList playList)
        {
            if (playList == null || pItem == null)
                return;

            //Tfro71 6-1-2006 added a check to see if m_database exists. Otherwise is has to be initiated.
            //Because when the autoplay detects a CD and the current screen is not from the Music Gui the musicdatabase 
            //does not have an instance
            if (m_database == null)
                m_database = new MusicDatabase();

            if (pItem.IsFolder)
            {
                // recursive
                if (pItem.Label == "..") return;
                string strDirectory = m_strDirectory;
                m_strDirectory = pItem.Path;

                List<GUIListItem> itemlist = m_directory.GetDirectoryExt(m_strDirectory);
                OnRetrieveMusicInfo(ref itemlist);

                // Sort share folder tracks.  
                try
                {
                    itemlist.Sort(new TrackComparer());
                }

                catch (Exception ex)
                {
                    Log.Write("GUIMusicFiles.AddItemToPlayList at itemlist.Sort: {0}", ex.Message);
                }

                foreach (GUIListItem item in itemlist)
                {
                    AddItemToPlayList(item, ref playList);
                }
                m_strDirectory = strDirectory;
            }
            else
            {
                //TODO
                if (Utils.IsAudio(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
                {
                    List<GUIListItem> list = new List<GUIListItem>();
                    list.Add(pItem);
                    m_bScan = true;
                    OnRetrieveMusicInfo(ref list);
                    m_database.CheckVariousArtistsAndCoverArt();
                    m_bScan = false;

                    PlayListItem playlistItem = new PlayListItem();
                    playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
                    playlistItem.FileName = pItem.Path;
                    playlistItem.Description = pItem.Label;
                    playlistItem.Duration = pItem.Duration;
                    playlistItem.MusicTag = pItem.MusicTag;
                    playList.Add(playlistItem);
                }
            }
        }
        
        void keyboard_TextChanged(int kindOfSearch, string data)
        {
            DisplayFilesList(kindOfSearch, data);
        }

        void GetStringFromKeyboard(ref string strLine)
        {
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            if (null == keyboard) return;
            keyboard.Reset();
            keyboard.Text = strLine;
            keyboard.DoModal(GetID);
            if (keyboard.IsConfirmed)
            {
                strLine = keyboard.Text;
            }
        }

        static public string GetCoverArt(bool isfolder, string filename, MusicTag tag)
        {
            string strFolderThumb = String.Empty;
            if (isfolder)
            {
                strFolderThumb = String.Format(@"{0}\folder.jpg", Utils.RemoveTrailingSlash(filename));
                if (System.IO.File.Exists(strFolderThumb))
                {
                    return strFolderThumb;
                }
                return string.Empty;
            }

            string strAlbumName = String.Empty;
            string strArtistName = String.Empty;
            if (tag != null)
            {
                if (tag.Album.Length > 0) strAlbumName = tag.Album;
                if (tag.Artist.Length > 0) strArtistName = tag.Artist;
            }

            // use covert art thumbnail for albums
            string strThumb = GUIMusicFiles.GetAlbumThumbName(strArtistName, strAlbumName);
            if (System.IO.File.Exists(strThumb))
            {
                return strThumb;
            }

            // no album art? then use folder.jpg
            string strPathName;
            string strFileName;
            DatabaseUtility.Split(filename, out strPathName, out strFileName);
            strFolderThumb = strPathName + @"\folder.jpg";
            if (System.IO.File.Exists(strFolderThumb))
            {
                return strFolderThumb;
            }
            return string.Empty;
        }

        #region ISetupForm Members

        public bool DefaultEnabled()
        {
            return true;
        }
        public bool CanEnable()
        {
            return true;
        }

        public bool HasSetup()
        {
            return false;
        }

        public string PluginName()
        {
            return "My Music";
        }

        public int GetWindowId()
        {
            return GetID;
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = GUILocalizeStrings.Get(2);
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }

        public string Author()
        {
            return "Frodo";
        }

        public string Description()
        {
            return "Plugin to play & organize your music";
        }

        public void ShowPlugin()
        {
        }

        #endregion

        #region IShowPlugin Members

        public bool ShowDefaultHome()
        {
            return true;
        }

        #endregion

        int GetCDATrackNumber(string strFile)
        {
            string strTrack = String.Empty;
            int pos = strFile.IndexOf(".cda");
            if (pos >= 0)
            {
                pos--;
                while (Char.IsDigit(strFile[pos]) && pos > 0)
                {
                    strTrack = strFile[pos] + strTrack;
                    pos--;
                }
            }

            try
            {
                int iTrack = Convert.ToInt32(strTrack);
                return iTrack;
            }
            catch (Exception)
            {
            }
            return 1;
        }
        
        private void item_OnItemSelected(GUIListItem item, GUIControl parent)
        {
            GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
            if (filmstrip == null) return;
            filmstrip.InfoImageFileName = item.ThumbnailImage;
        }
        
        static public bool IsMusicWindow(int window)
        {
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_FILES) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_ALBUM) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_ARTIST) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_GENRE) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_TOP100) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_FAVORITES) return true;
            if (window == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW) return true;
            return false;
        }

        void OnRetrieveMusicInfo(ref List<GUIListItem> items)
        {
            int nFolderCount = 0;
            //Tfro71 6-1-2006 added a check to see if m_database exists. Otherwise is has to be initiated.
            //Because when the autoplay detects a CD and the current screen is not from the Music Gui the musicdatabase 
            //does not have an instance
            if (m_database == null)
                m_database = new MusicDatabase();

            foreach (GUIListItem item in items)
            {
                if (item.IsFolder) nFolderCount++;
            }

            // Skip items with folders only
            if (nFolderCount == (int)items.Count)
                return;

            GUIDialogProgress dlg = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            if (m_strDirectory.Length == 0) return;
            //string strItem;
            List<SongMap> songsMap = new List<SongMap>();
            // get all information for all files in current directory from database 
            m_database.GetSongsByPath2(m_strDirectory, ref songsMap);

            //musicCD is the information about the cd...
            //delete old CD info
            //GUIMusicFiles.MusicCD = null;

            bool bCDDAFailed = false;
            // for every file found, but skip folder
            for (int i = 0; i < (int)items.Count; ++i)
            {
                GUIListItem pItem = items[i];
                if (pItem.IsRemote) continue;
                if (pItem.IsFolder) continue;
                if (pItem.Path.Length == 0) continue;
                string strFilePath = System.IO.Path.GetFullPath(pItem.Path);
                strFilePath = strFilePath.Substring(0, strFilePath.Length - (1 + System.IO.Path.GetFileName(pItem.Path).Length));
                if (strFilePath != m_strDirectory)
                {
                    return;
                }
                string strExtension = System.IO.Path.GetExtension(pItem.Path);
                if (m_bScan && strExtension.ToLower().Equals(".cda")) continue;
                if (m_bScan && dlg != null)
                    dlg.ProgressKeys();

                // dont try reading id3tags for folders or playlists
                if (!pItem.IsFolder && !PlayListFactory.IsPlayList(pItem.Path))
                {
                    // is tag for this file already loaded?
                    bool bNewFile = false;
                    MusicTag tag = (MusicTag)pItem.MusicTag;
                    if (tag == null)
                    {
                        // no, then we gonna load it. But dont load tags from cdda files
                        if (strExtension != ".cda")  // int_20h: changed cdda to cda.
                        {
                            // first search for file in our list of the current directory
                            Song song = new Song();
                            bool bFound = false;
                            foreach (SongMap song1 in songsMap)
                            {
                                if (song1.m_strPath == pItem.Path)
                                {
                                    song = song1.m_song;
                                    bFound = true;
                                    tag = new MusicTag();
                                    pItem.MusicTag = tag;
                                    break;
                                }
                            }

                            if (!bFound && !m_bScan)
                            {
                                // try finding it in the database
                                string strPathName;
                                string strFileName;
                                DatabaseUtility.Split(pItem.Path, out strPathName, out strFileName);
                                if (strPathName != m_strDirectory)
                                {
                                    if (m_database.GetSongByFileName(pItem.Path, ref song))
                                    {
                                        bFound = true;
                                    }
                                }
                            }

                            if (!bFound)
                            {
                                // if id3 tag scanning is turned on OR we're scanning the directory
                                // then parse id3tag from file
                                if (UseID3 || m_bScan)
                                {
                                    // get correct tag parser
                                    tag = TagReader.TagReader.ReadTag(pItem.Path);
                                    if (tag != null)
                                    {
                                        pItem.MusicTag = tag;
                                        bNewFile = true;
                                    }
                                }
                            }
                            else // of if ( !bFound )
                            {
                                tag.Album = song.Album;
                                tag.Artist = song.Artist;
                                tag.Genre = song.Genre;
                                tag.Duration = song.Duration;
                                tag.Title = song.Title;
                                tag.Track = song.Track;
                                tag.Rating = song.Rating;
                            }
                        }//if (strExtension!=".cda" )
                        else // int_20h: if it is .cda then get info from freedb
                        {
                            if (m_bScan)
                                continue;

                            if (bCDDAFailed) continue;
                            if (!Util.Win32API.IsConnectedToInternet()) continue;

                            try
                            {
                                // check internet connectivity
                                GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                                if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
                                {
                                    pDlgOK.SetHeading(703);
                                    //pDlgOK.SetLine(0, String.Empty);
                                    pDlgOK.SetLine(1, 703);
                                    pDlgOK.SetLine(2, String.Empty);
                                    pDlgOK.DoModal(GetID);

                                    //throw new Exception("no internet");
                                    return;
                                }
                                else if (!Util.Win32API.IsConnectedToInternet())
                                {
                                    //throw new Exception("no internet");
                                    return;
                                }

                                Freedb.FreeDBHttpImpl freedb = new Freedb.FreeDBHttpImpl();
                                char driveLetter = System.IO.Path.GetFullPath(pItem.Path).ToCharArray()[0];
                                // try finding it in the database
                                string strPathName, strCDROMPath;
                                //int_20h fake the path with the cdInfo
                                strPathName = driveLetter + ":/" + freedb.GetCDDBDiscIDInfo(driveLetter, '+');
                                strCDROMPath = strPathName + "+" + System.IO.Path.GetFileName(pItem.Path);

                                Song song = new Song();
                                bool bFound = false;
                                if (m_database.GetSongByFileName(strCDROMPath, ref song))
                                {
                                    bFound = true;
                                }

                                // Disk changed (or other drive)
                                if (GUIMusicFiles.MusicCD != null)
                                {
                                    if (freedb.GetCDDBDiscID(driveLetter).ToLower() != GUIMusicFiles.MusicCD.DiscID)
                                    {
                                        GUIMusicFiles.MusicCD = null;
                                    }
                                }

                                if (!bFound && GUIMusicFiles.MusicCD == null)
                                {
                                    try
                                    {
                                        freedb.Connect(); // should be replaced with the Connect that receives a http freedb site...
                                        Freedb.CDInfo[] cds = freedb.GetDiscInfo(driveLetter);
                                        if (cds != null)
                                        {
                                            if (cds.Length == 1)
                                            {
                                                GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[0].Category, cds[0].DiscId);
                                                m_strDiscId = cds[0].DiscId;
                                            }
                                            else if (cds.Length > 1)
                                            {
                                                if (m_strDiscId == cds[0].DiscId)
                                                {
                                                    GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[m_iSelectedAlbum].Category, cds[m_iSelectedAlbum].DiscId);
                                                }
                                                else
                                                {
                                                    m_strDiscId = cds[0].DiscId;
                                                    //show dialog with all albums found
                                                    string szText = GUILocalizeStrings.Get(181);
                                                    GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
                                                    if (null != pDlg)
                                                    {
                                                        pDlg.Reset();
                                                        pDlg.SetHeading(szText);
                                                        for (int j = 0; j < cds.Length; j++)
                                                        {
                                                            Freedb.CDInfo info = cds[j];
                                                            pDlg.Add(info.Title);
                                                        }
                                                        pDlg.DoModal(GetID);

                                                        // and wait till user selects one
                                                        m_iSelectedAlbum = pDlg.SelectedLabel;
                                                        if (m_iSelectedAlbum < 0) return;
                                                        GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[m_iSelectedAlbum].Category, cds[m_iSelectedAlbum].DiscId);
                                                    }
                                                }
                                            }
                                        }
                                        freedb.Disconnect();
                                        if (GUIMusicFiles.MusicCD == null) bCDDAFailed = true;
                                    }
                                    catch (Exception)
                                    {
                                        GUIMusicFiles.MusicCD = null;
                                        bCDDAFailed = true;
                                    }
                                }

                                if (!bFound && GUIMusicFiles.MusicCD != null) // if musicCD was configured correctly...
                                {
                                    int trackno = GetCDATrackNumber(pItem.Path);
                                    Freedb.CDTrackDetail track = GUIMusicFiles.MusicCD.getTrack(trackno);

                                    tag = new MusicTag();
                                    tag.Album = GUIMusicFiles.MusicCD.Title;
                                    tag.Genre = GUIMusicFiles.MusicCD.Genre;
                                    if (track == null)
                                    {
                                        // prob hidden track									
                                        tag.Artist = GUIMusicFiles.MusicCD.Artist;
                                        tag.Duration = -1;
                                        tag.Title = String.Empty;
                                        tag.Track = -1;
                                        pItem.Label = pItem.Path;
                                    }
                                    else
                                    {
                                        tag.Artist = track.Artist == null ? GUIMusicFiles.MusicCD.Artist : track.Artist;
                                        tag.Duration = track.Duration;
                                        tag.Title = track.Title;
                                        tag.Track = track.TrackNumber;
                                        pItem.Label = track.Title;
                                    }
                                    bNewFile = true;
                                    pItem.MusicTag = tag;
                                    pItem.Path = strCDROMPath; // to be stored in the database
                                }
                                else if (bFound)
                                {
                                    tag = new MusicTag();
                                    tag.Album = song.Album;
                                    tag.Artist = song.Artist;
                                    tag.Genre = song.Genre;
                                    tag.Duration = song.Duration;
                                    tag.Title = song.Title;
                                    tag.Track = song.Track;
                                    pItem.MusicTag = tag;
                                    pItem.Label = song.Title;
                                    pItem.Path = strCDROMPath;
                                }

                            }// end of try
                            catch (Exception e)
                            {
                                // log the problem...
                                Log.Write("OnRetrieveMusicInfo: {0}", e.ToString());
                            }
                        }
                    }//if (!tag.Loaded() )
                    else if (m_bScan)
                    {
                        bNewFile = true;
                        foreach (SongMap song1 in songsMap)
                        {
                            if (song1.m_strPath == pItem.Path)
                            {
                                bNewFile = false;
                                break;
                            }
                        }
                    }
                    foreach (SongMap song1 in songsMap)
                    {
                        if (song1.m_song.FileName == pItem.Path)
                        {
                            pItem.AlbumInfoTag = song1.m_song;
                            break;
                        }
                    }

                    if (tag != null && m_bScan && bNewFile)
                    {
                        Song song = new Song();
                        song.Title = tag.Title;
                        song.Genre = tag.Genre;
                        song.FileName = pItem.Path;
                        song.Artist = tag.Artist;
                        song.Album = tag.Album;
                        song.Year = tag.Year;
                        song.Track = tag.Track;
                        song.Duration = tag.Duration;
                        pItem.AlbumInfoTag = song;

                        m_database.AddSong(song, false);
                    }
                }//if (!pItem.IsFolder)
            }
        }

        bool DoScan(string strDir, ref List<GUIListItem> items)
        {
            GUIDialogProgress dlg = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            if (dlg != null)
            {
                string strPath = System.IO.Path.GetFileName(strDir);
                dlg.SetLine(2, strPath);
                dlg.Progress();
            }

            OnRetrieveMusicInfo(ref items);
            m_database.CheckVariousArtistsAndCoverArt();

            if (dlg != null && dlg.IsCanceled) return false;

            bool bCancel = false;
            for (int i = 0; i < (int)items.Count; ++i)
            {
                GUIListItem pItem = items[i];
                if (pItem.IsRemote) continue;
                if (dlg != null && dlg.IsCanceled)
                {
                    bCancel = true;
                    break;
                }
                if (pItem.IsFolder)
                {
                    if (pItem.Label != "..")
                    {
                        // load subfolder
                        string strPrevDir = m_strDirectory;
                        m_strDirectory = pItem.Path;
                        List<GUIListItem> subDirItems = m_directory.GetDirectoryExt(m_strDirectory);
                        if (!DoScan(m_strDirectory, ref subDirItems))
                        {
                            bCancel = true;
                        }
                        m_strDirectory = strPrevDir;
                        if (bCancel) break;
                    }
                }
            }

            return !bCancel;
        }

        void OnScan()
        {
            GUIDialogProgress dlg = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
            GUIGraphicsContext.Overlay = false;

            m_bScan = true;
            List<GUIListItem> items = new List<GUIListItem>();
            for (int i = 0; i < facadeView.Count; ++i)
            {
                GUIListItem pItem = facadeView[i];
                if (!pItem.IsRemote)
                    items.Add(pItem);
            }
            if (null != dlg)
            {
                string strPath = System.IO.Path.GetFileName(m_strDirectory);
                dlg.SetHeading(189);
                dlg.SetLine(1, 330);
                //dlg.SetLine(1, String.Empty);
                dlg.SetLine(2, strPath);
                dlg.StartModal(GetID);
            }

            m_database.BeginTransaction();
            if (DoScan(m_strDirectory, ref items))
            {
                dlg.SetLine(1, 328);
                dlg.SetLine(2, String.Empty);
                dlg.SetLine(3, 330);
                dlg.Progress();
                m_database.CommitTransaction();
            }
            else
                m_database.RollbackTransaction();
            m_database.EmptyCache();
            dlg.Close();
            // disable scan mode
            m_bScan = false;
            GUIGraphicsContext.Overlay = _isOverlayAllowed;

            LoadDirectory(m_strDirectory);
        }

        protected override void OnFindCoverArt(int iItem)
        {
            GUIListItem pItem = facadeView[iItem];

            if (pItem.IsFolder && pItem.Label != "..")
            {
                GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);

                if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
                {
                    pDlgOK.SetHeading(703);
                    pDlgOK.SetLine(1, 703);
                    pDlgOK.SetLine(2, String.Empty);
                    pDlgOK.DoModal(GetID);

                    //throw new Exception("no internet");
                    return;
                }

                else if (!Util.Win32API.IsConnectedToInternet())
                {
                    //throw new Exception("no internet");
                    return;
                }

                string oldFolder = m_strDirectory;
                VirtualDirectory dir = new VirtualDirectory();
                dir.SetExtensions(Utils.AudioExtensions);
                List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(pItem.Path, true);

                if (items.Count < 2)
                    return;

                m_bScan = true;
                m_strDirectory = pItem.Path;
                OnRetrieveMusicInfo(ref items);
                m_strDirectory = oldFolder;
                m_bScan = false;
                GUIListItem item = items[1] as GUIListItem;
                MusicTag tag = item.MusicTag as MusicTag;

                // Is this an album?
                if (tag != null && tag.Album.Length > 0)
                {
                    FindCoverArt(true, tag.Artist, tag.Album, pItem.Path, tag, -1);
                }

                // Nope, it's a artist folder or share
                else
                {
                    int windowID = (int)GUIWindow.Window.WINDOW_MUSIC_COVERART_GRABBER_PROGRESS;
                    GUICoverArtGrabberProgress guiCoverArtProgress = (GUICoverArtGrabberProgress)GUIWindowManager.GetWindow(windowID);

                    if (guiCoverArtProgress != null)
                    {
                        guiCoverArtProgress.CoverArtSelected += new GUICoverArtGrabberProgress.CoverArtSelectedHandler(OnCoverArtGrabberCoverArtSelected);
                        guiCoverArtProgress.CoverArtGrabDone += new GUICoverArtGrabberProgress.CoverArtGrabDoneHandler(OnCoverArtGrabberDone);
                        guiCoverArtProgress.TopLevelFolderName = pItem.Path;
                        guiCoverArtProgress.UseID3 = UseID3;
                        guiCoverArtProgress.Show(GetID);
                    }
                }
            }

            base.OnFindCoverArt(iItem);
        }

        protected override void OnInfo(int iItem)
        {
            GUIListItem pItem = facadeView[iItem];

            if (pItem.IsFolder && pItem.Label != "..")
            {
                string oldFolder = m_strDirectory;
                VirtualDirectory dir = new VirtualDirectory();
                dir.SetExtensions(Utils.AudioExtensions);
                List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(pItem.Path, true);

                if (items.Count < 2)
                    return;

                m_bScan = true;
                m_strDirectory = pItem.Path;
                OnRetrieveMusicInfo(ref items);
                m_strDirectory = oldFolder;
                m_bScan = false;
                GUIListItem item = items[1] as GUIListItem;
                MusicTag tag = item.MusicTag as MusicTag;

                // Is this an album?
                if (tag != null && tag.Album.Length > 0)
                {
                    ShowAlbumInfo(true, tag.Artist, tag.Album, pItem.Path, tag, -1);
                    facadeView.RefreshCoverArt();
                }

                // Nope, it's a artist folder or share
                else
                {
                    return;
                }
            }

            Song song = pItem.AlbumInfoTag as Song;

            if (song == null)
            {
                List<GUIListItem> list = new List<GUIListItem>();
                list.Add(pItem);
                m_bScan = true;
                OnRetrieveMusicInfo(ref list);
                m_bScan = false;
            }

            facadeView.RefreshCoverArt();
            base.OnInfo(iItem);
        }

        void OnCoverArtGrabberCoverArtSelected(AlbumInfo albumInfo, string albumPath, bool bSaveToAlbumFolder, bool bSaveToThumbsFolder)
        {
            SaveCoverArtImage(albumInfo, albumPath, bSaveToAlbumFolder, bSaveToThumbsFolder);
        }

        void OnCoverArtGrabberDone(GUICoverArtGrabberProgress coverArtGrabberProgress)
        {
            facadeView.RefreshCoverArt();
        }

        protected override void AddSongToFavorites(GUIListItem item)
        {
            Song song = item.AlbumInfoTag as Song;
            if (song == null)
            {
                List<GUIListItem> list = new List<GUIListItem>();
                list.Add(item);
                m_bScan = true;
                OnRetrieveMusicInfo(ref list);
                m_bScan = false;
            }
            base.AddSongToFavorites(item);
        }

        protected void OnQueueItem(GUIListItem pItem)
        {
            if (pItem == null || pItem.IsRemote || PlayListFactory.IsPlayList(pItem.Path))
                return;

            AddItemToPlayList(pItem);
            this.facadeView.SelectedListItemIndex = facadeView.SelectedListItemIndex;

            if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0 && !g_Player.Playing)
            {
                playlistPlayer.Reset();
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
                playlistPlayer.Play(0);
            }
        }

        protected void OnPlayNext(GUIListItem pItem)
        {
            if (pItem == null || pItem.IsRemote || PlayListFactory.IsPlayList(pItem.Path))
                return;

            PlayList playList = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

            if (playList == null)
                return;

            int index = Math.Max(playlistPlayer.CurrentSong, 0);

            if (playList.Count > 0)
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
                for (int i = 0; i < tempPlayList.Count; ++i)
                {
                    playList.Add(tempPlayList[i]);
                }
            }

            else
            {
                playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
                AddItemToPlayList(pItem);
            }

            if (!g_Player.Playing)
                playlistPlayer.Play(index);
        }

        protected void OnPlayNow(GUIListItem pItem)
        {
            if (pItem == null || pItem.IsRemote || PlayListFactory.IsPlayList(pItem.Path))
                return;

            int iItem = facadeView.SelectedListItemIndex;
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
                    playlistPlayer.Play(playStartIndex);
            }
        }

        private bool IsShare(GUIListItem pItem)
        {
            if (pItem.Path.Length == 0)
                return false;

            Share share = m_directory.GetShare(pItem.Path);
            bool isCdOrDvd = Utils.IsDVD(pItem.Path);

            if (!isCdOrDvd && share != null && share.Path == pItem.Path)
                return true;

            else
                return false;
        }

        // Need to remove this and allow the rmote plugins to handle anti-repeat logic.
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

        private bool IsCD(string path)
        {
            if (Utils.IsDVD(path))
            {
                string rootDir = path.Substring(0, 2);
                string video_tsPath = System.IO.Path.Combine(rootDir, "VIDEO_TS");
                if (!System.IO.Directory.Exists(video_tsPath))
                    return true;
            }

            return false;
        }

        private bool IsDVD(string path)
        {
            if (Utils.IsDVD(path))
            {
                string rootDir = path.Substring(0, 2);
                string video_tsPath = System.IO.Path.Combine(rootDir, "VIDEO_TS");
                if (System.IO.Directory.Exists(video_tsPath))
                    return true;
            }

            return false;
        }
    }
}
