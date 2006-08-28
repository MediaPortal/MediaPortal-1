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
    public class GUIMusicPlayingNow : GUIWindow
    {
        private enum ControlIDs
        {
            LBL_CAPTION = 1,
            IMG_COVERART = 2,
            LBL_TRACK_NAME = 3,
            LBL_ALBUM_NAME = 4,
            LBL_ALBUM_YEAR = 5,
            LBL_ARTIST_NAME = 6,
            IMG_TRACK_PROGRESS_BG = 7,
            PROG_TRACK = 8,

            IMGLIST_RATING = 11,
            IMGLIST_NEXTRATING = 12,

            LBL_UP_NEXT = 20,
            LBL_NEXT_TRACK_NAME = 21,
            LBL_NEXT_ALBUM_NAME = 22,
            LBL_NEXT_ARTIST_NAME = 23,

            // Transport Buttons
            BTN_BACK = 30,
            BTN_PREVIOUS = 31,
            BTN_PLAY = 32,
            BTN_PAUSE = 33,
            BTN_STOP = 34,
            BTN_NEXT = 35,
        }

        #region Properties

        public GUIMusicBaseWindow MusicWindow
        {
            set { _MusicWindow = value; }
        }

        #endregion

        [SkinControlAttribute((int)ControlIDs.LBL_CAPTION)]
        protected GUILabelControl LblCaption = null;

        [SkinControlAttribute((int)ControlIDs.IMG_COVERART)]
        protected GUIImage ImgCoverArt = null;

        [SkinControlAttribute((int)ControlIDs.LBL_TRACK_NAME)]
        protected GUIFadeLabel LblTrackName = null;

        [SkinControlAttribute((int)ControlIDs.LBL_ALBUM_NAME)]
        protected GUIFadeLabel LblAlbumName = null;

        [SkinControlAttribute((int)ControlIDs.LBL_ALBUM_YEAR)]
        protected GUILabelControl LblAlbumYear = null;

        [SkinControlAttribute((int)ControlIDs.LBL_ARTIST_NAME)]
        protected GUIFadeLabel LblArtistName = null;

        [SkinControlAttribute((int)ControlIDs.PROG_TRACK)]
        protected GUIProgressControl ProgTrack = null;

        [SkinControlAttribute((int)ControlIDs.IMG_TRACK_PROGRESS_BG)]
        protected GUIImage ImgTrackProgressBkGrnd = null;

        [SkinControlAttribute((int)ControlIDs.LBL_UP_NEXT)]
        protected GUILabelControl LblUpNext = null;

        [SkinControlAttribute((int)ControlIDs.LBL_NEXT_TRACK_NAME)]
        protected GUIFadeLabel LblNextTrackName = null;

        [SkinControlAttribute((int)ControlIDs.LBL_NEXT_ALBUM_NAME)]
        protected GUIFadeLabel LblNextAlbumName = null;

        [SkinControlAttribute((int)ControlIDs.LBL_NEXT_ARTIST_NAME)]
        protected GUIFadeLabel LblNextArtistName = null;

        [SkinControlAttribute((int)ControlIDs.BTN_BACK)]
        protected GUIButtonControl BtnBack = null;

        [SkinControlAttribute((int)ControlIDs.BTN_PREVIOUS)]
        protected GUIButtonControl BtnPrevious = null;

        [SkinControlAttribute((int)ControlIDs.BTN_PLAY)]
        protected GUIButtonControl BtnPlay = null;

        [SkinControlAttribute((int)ControlIDs.BTN_PAUSE)]
        protected GUIButtonControl BtnPause = null;

        [SkinControlAttribute((int)ControlIDs.BTN_STOP)]
        protected GUIButtonControl BtnStop = null;

        [SkinControlAttribute((int)ControlIDs.BTN_NEXT)]
        protected GUIButtonControl BtnNext = null;

        [SkinControlAttribute((int)ControlIDs.IMGLIST_RATING)]
        protected GUIImageList ImgListRating = null;

        [SkinControlAttribute((int)ControlIDs.IMGLIST_NEXTRATING)]
        protected GUIImageList ImgListNextRating = null;

        //[SkinControlAttribute((int)ControlIDs.BTN_NEXT)]
        //protected GUIButtonControl BtnNext = null;
        
        public enum TrackProgressType { Elapsed, CountDown };

        private bool ControlsInitialized = false;
        private PlayListPlayer PlaylistPlayer = null;
        private string CurrentThumbFileName = string.Empty;
        private string CurrentTrackFileName = string.Empty;
        private string NextTrackFileName = string.Empty;
        private MusicTag CurrentTrackTag = null;
        private MusicTag NextTrackTag = null;
        private DateTime LastUpdateTime = DateTime.Now;
        private TimeSpan UpdateInterval = new TimeSpan(0, 0, 1);
        private GUIMusicBaseWindow _MusicWindow = null;
        private bool UseID3 = false;



        public GUIMusicPlayingNow()
        {
            GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
            PlaylistPlayer = PlayListPlayer.SingletonPlayer;

            g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
            g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
            {
                UseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
            }
        }

        void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
        {
            if (!ControlsInitialized)
                return;
        }

        void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
        {
            if (!ControlsInitialized)
                return;

            if (GUIWindowManager.ActiveWindow == GetID)
            {
                Action action = new Action();
                action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
                GUIGraphicsContext.OnAction(action);
            }
        }

        void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
        {
            if (!ControlsInitialized)
                return;

            CurrentTrackFileName = filename;
            NextTrackFileName = PlaylistPlayer.GetNext();
            GetTrackTags();

            CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, filename, CurrentTrackTag);

            if (CurrentThumbFileName.Length == 0)
                CurrentThumbFileName = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";

            ImgCoverArt.SetFileName(CurrentThumbFileName);
            UpdateTrackInfo();
            UpdateTrackPosition();
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\MyMusicPlayingNow.xml");
        }

        public override void OnAction(Action action)
        {
            base.OnAction(action);

            switch (action.wID)
            {
                case Action.ActionType.ACTION_STOP:
                    {
                        if (GUIWindowManager.ActiveWindow == GetID)
                        {
                            Action act = new Action();
                            act.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
                            GUIGraphicsContext.OnAction(act);
                        }

                        break;
                    }

                // Since a ACTION_STOP action clears the player and CurrentPlaylistType type
                // we need a way to restart playback after an ACTION_STOP has been received
                case Action.ActionType.ACTION_MUSIC_PLAY:
                case Action.ActionType.ACTION_NEXT_ITEM:
                case Action.ActionType.ACTION_PAUSE:
                case Action.ActionType.ACTION_PREV_ITEM:
                    {
                        //if (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC)
                        if ((PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC) && 
                            (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_TEMP))
                        {
                            LoadAndStartPlayList();
                        }

                        break;
                    }
            }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();

            GUIPropertyManager.SetProperty("#currentmodule", String.Format("{0}/{1}", GUILocalizeStrings.Get(100005), GUILocalizeStrings.Get(4540)));
            LblUpNext.Label = GUILocalizeStrings.Get(4541);

            if (LblUpNext.Label.Length == 0)
                LblUpNext.Label = "Playing next";

            ControlsInitialized = true;
            g_Player_PlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            base.OnPageDestroy(new_windowId);
            ControlsInitialized = false;
        }

        public override void Render(float timePassed)
        {
            base.Render(timePassed);

            if (!g_Player.Playing || !g_Player.IsMusic)
                return;

            TimeSpan ts = DateTime.Now - LastUpdateTime;

            if (ts >= UpdateInterval)
            {
                LastUpdateTime = DateTime.Now;
                UpdateTrackInfo();
            }
        }

        protected override void OnShowContextMenu()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

            if (dlg == null)
                return;

            dlg.Reset();
            dlg.SetHeading(924);                // Menu
            dlg.AddLocalizedString(928);        // Find Coverart
            dlg.AddLocalizedString(4521);       // Show Album Info

            if (IsCdTrack(CurrentTrackFileName))
                dlg.AddLocalizedString(4554);   // Lookup CD info

            dlg.DoModal(GetID);

            if (dlg.SelectedId == -1)
                return;

            switch (dlg.SelectedId)
            {
                case 928:       // Find Coverart
                    {
                        string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);
                        _MusicWindow.FindCoverArt(false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag, -1);
                        CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

                        if (CurrentThumbFileName.Length == 0)
                            CurrentThumbFileName = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";

                        ImgCoverArt.SetFileName(CurrentThumbFileName);
                        break;
                    }

                case 4521:      // Show Album Info
                    {
                        string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);
                        _MusicWindow.ShowAlbumInfo(GetID, false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag, -1);
                        break;
                    }

                case 4554:      // Lookup CD info
                    {
                        MusicTag tag = new MusicTag();
                        GetCDInfoFromFreeDB(CurrentTrackFileName, tag);
                        GetTrackTags();
                        break;
                    }
            }
        }

        private void UpdateTrackInfo()
        {
            UpdateTrackPosition();

            if (CurrentTrackTag != null)
            {
                LblTrackName.Label = CurrentTrackTag.Title;
                LblAlbumName.Label = CurrentTrackTag.Album;
                LblArtistName.Label = CurrentTrackTag.Artist;

                if (CurrentTrackTag.Year > 0)
                    LblAlbumYear.Label = CurrentTrackTag.Year.ToString();

                else
                    LblAlbumYear.Label = "";

                if (ImgListNextRating != null)
                {
                    int rating = CurrentTrackTag.Rating;
                    ImgListRating.Percentage = rating;
                }
            }

            else
            {
                string noTrackInfo = GUILocalizeStrings.Get(4543);
                if (noTrackInfo.Length == 0)
                    noTrackInfo = "Track information not available";

                if (PlaylistPlayer == null || PlaylistPlayer.GetCurrentItem() == null)
                {
                    string noPlayList = GUILocalizeStrings.Get(4542);
                    if (noPlayList.Length == 0)
                        noPlayList = "No playlists loaded";

                    LblTrackName.Label = noPlayList;
                    LblAlbumName.Label = noTrackInfo;
                }

                else
                {
                    LblTrackName.Label = noTrackInfo;
                    LblAlbumName.Label = "";
                }

                LblArtistName.Label = "";
                LblAlbumYear.Label = "";
            }

            if (NextTrackTag != null)
            {
                LblNextTrackName.Label = NextTrackTag.Title;
                LblNextAlbumName.Label = NextTrackTag.Album;
                LblNextArtistName.Label = NextTrackTag.Artist;

                if (ImgListNextRating != null)
                {
                    int rating = NextTrackTag.Rating;
                    ImgListNextRating.Percentage = rating;
                }
            }

            else
            {
                string noTrackInfo = GUILocalizeStrings.Get(4543);
                if (noTrackInfo.Length == 0)
                    noTrackInfo = "Track information not available";

                LblNextTrackName.Label = noTrackInfo;
                LblNextAlbumName.Label = "";
                LblNextArtistName.Label = "";
            }
        }

        private void ResetTrackInfo()
        {
            ImgCoverArt.SetFileName("");
            LblTrackName.Label = "";
            LblAlbumName.Label = "";
            LblArtistName.Label = "";
            LblUpNext.Label = "";
            LblNextTrackName.Label = "";
            LblNextAlbumName.Label = "";
            LblNextArtistName.Label = "";
        }

        private void UpdateTrackPosition()
        {
            double trackDuration = g_Player.Duration;
            double curTrackPostion = g_Player.CurrentPosition;

            int progPrecent = (int)(curTrackPostion / trackDuration * 100d);

            this.ProgTrack.Percentage = progPrecent;
            ProgTrack.Visible = ProgTrack.Percentage > 0;
        }

        private void GetTrackTags()
        {
            bool isCurSongCdTrack = IsCdTrack(CurrentTrackFileName);
            bool isNextSongCdTrack = IsCdTrack(NextTrackFileName);
            MusicDatabase dbs = new MusicDatabase();

            if (!isCurSongCdTrack)
                CurrentTrackTag = GetTrackTag(dbs, CurrentTrackFileName, UseID3);

            if (!isNextSongCdTrack)
                NextTrackTag = GetTrackTag(dbs, NextTrackFileName, UseID3);

            if (isCurSongCdTrack || isNextSongCdTrack)
            {
                PlayList curPlaylist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

                int iCurItemIndex = PlaylistPlayer.CurrentSong;
                PlayListItem curPlaylistItem = curPlaylist[iCurItemIndex];

                if (curPlaylistItem == null)
                {
                    CurrentTrackTag = null;
                    NextTrackTag = null;
                    return;
                }

                int playListItemCount = curPlaylist.Count;
                int nextItemIndex = 0;

                if (iCurItemIndex < playListItemCount - 1)
                    nextItemIndex = iCurItemIndex + 1;

                PlayListItem nextPlaylistItem = curPlaylist[nextItemIndex];

                if (isCurSongCdTrack)
                    CurrentTrackTag = (MusicTag)curPlaylistItem.MusicTag;

                if (isNextSongCdTrack && nextPlaylistItem != null)
                    NextTrackTag = (MusicTag)nextPlaylistItem.MusicTag;

                // There's no MusicTag info in the Playlist so check is we have a valid 
                // GUIMusicFiles.MusicCD object
                if ((CurrentTrackTag == null || NextTrackTag == null) && GUIMusicFiles.MusicCD != null)
                {
                    int curCDTrackNum = GetCDATrackNumber(CurrentTrackFileName);
                    int nextCDTrackNum = GetCDATrackNumber(NextTrackFileName);

                    if (curCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
                    {
                        MediaPortal.Freedb.CDTrackDetail curTrack = GUIMusicFiles.MusicCD.getTrack(curCDTrackNum);
                        CurrentTrackTag = GetTrackTag(curTrack);
                    }

                    if (nextCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
                    {
                        MediaPortal.Freedb.CDTrackDetail nextTrack = GUIMusicFiles.MusicCD.getTrack(nextCDTrackNum);
                        NextTrackTag = GetTrackTag(nextTrack);
                    }

                    if (CurrentTrackTag != null)
                    {
                        CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

                        if (CurrentThumbFileName.Length == 0)
                            CurrentThumbFileName = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";

                        ImgCoverArt.SetFileName(CurrentThumbFileName);
                    }
                }
            }
        }

        private MusicTag GetTrackTag(MediaPortal.Freedb.CDTrackDetail cdTrack)
        {
            if (GUIMusicFiles.MusicCD == null)
                return null;

            MusicTag tag = new MusicTag();
            tag.Artist = GUIMusicFiles.MusicCD.Artist;
            tag.Album = GUIMusicFiles.MusicCD.Title;
            tag.Genre = GUIMusicFiles.MusicCD.Genre;
            tag.Year = GUIMusicFiles.MusicCD.Year;
            tag.Duration = cdTrack.Duration;
            tag.Title = cdTrack.Title;

            return tag;
        }

        private MusicTag GetTrackTag(MusicDatabase dbs, string strFile, bool useID3)
        {
            MusicTag tag = null;
            Song song = new Song();
            bool bFound = dbs.GetSongByFileName(strFile, ref song);

            if (!bFound)
            {
                if (useID3)
                    tag = TagReader.TagReader.ReadTag(strFile);
            }

            else
            {
                tag = new MusicTag();
                tag.Album = song.Album;
                tag.Artist = song.Artist;
                tag.Duration = song.Duration;
                tag.Genre = song.Genre;
                tag.Title = song.Title;
                tag.Track = song.Track;
                tag.Year = song.Year;
            }

            return tag;
        }

        private bool IsCdTrack(string fileName)
        {
            return System.IO.Path.GetExtension(CurrentTrackFileName).ToLower() == ".cda";
        }

        private void GetCDInfoFromFreeDB(string path, MusicTag tag)
        {
            try
            {
                // check internet connectivity
                GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
                {
                    pDlgOK.SetHeading(703);
                    pDlgOK.SetLine(1, 703);
                    pDlgOK.SetLine(2, String.Empty);
                    pDlgOK.DoModal(GetID);

                    return;
                }
                else if (!Util.Win32API.IsConnectedToInternet())
                {
                    return;
                }

                string m_strDiscId = string.Empty;
                int m_iSelectedAlbum = 0;

                Freedb.FreeDBHttpImpl freedb = new Freedb.FreeDBHttpImpl();
                char driveLetter = System.IO.Path.GetFullPath(path).ToCharArray()[0];

                // try finding it in the database
                string strPathName, strCDROMPath;

                //int_20h fake the path with the cdInfo
                strPathName = driveLetter + ":/" + freedb.GetCDDBDiscIDInfo(driveLetter, '+');
                strCDROMPath = strPathName + "+" + System.IO.Path.GetFileName(path);

                Song song = new Song();
                bool bFound = false;

                try
                {
                    freedb.Connect(); // should be replaced with the Connect that receives a http freedb site...
                    Freedb.CDInfo[] cds = freedb.GetDiscInfo(driveLetter);
                    if (cds != null)
                    {
                        // freedb returned one album
                        if (cds.Length == 1)
                        {
                            GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[0].Category, cds[0].DiscId);
                            m_strDiscId = cds[0].DiscId;
                        }

                        // freedb returned more than one album
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
                    //if (GUIMusicFiles.MusicCD == null) bCDDAFailed = true;
                }

                catch (Exception)
                {
                    GUIMusicFiles.MusicCD = null;
                }

                if (!bFound && GUIMusicFiles.MusicCD != null) // if musicCD was configured correctly...
                {
                    int trackno = GetCDATrackNumber(path);
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
                    }
                    else
                    {
                        tag.Artist = track.Artist == null ? GUIMusicFiles.MusicCD.Artist : track.Artist;
                        tag.Duration = track.Duration;
                        tag.Title = track.Title;
                        tag.Track = track.TrackNumber;
                    }
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
                }

            }// end of try
            catch (Exception e)
            {
                // log the problem...
                Log.Info("GUIMusicPlayingNow.GetCDInfoFromFreeDB: {0}", e.ToString());
            }
        }

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

        private void LoadAndStartPlayList()
        {
            PlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

            if (g_Player.CurrentFile == "")
            {
                PlayList playList = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

                if (playList != null && playList.Count > 0)
                {
                    this.CurrentTrackFileName = playList[0].FileName;
                    PlaylistPlayer.Play(0);
                    g_Player_PlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);
                }
            }
        }
    }
}