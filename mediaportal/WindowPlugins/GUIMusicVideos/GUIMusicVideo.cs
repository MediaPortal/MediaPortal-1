#region Copyright (C) 2006 Team MediaPortal

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
using System.Text;
using System.Threading;
using System.Xml;
using MediaPortal;
using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;
using MediaPortal.GUI;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.MusicVideos.Database;
using System.ComponentModel;
using System.IO;

namespace MediaPortal.GUI.MusicVideos
{
    public class GUIMusicVideos : GUIWindow, ISetupForm, IShowPlugin
    {
        #region SkinControlAttributes
        [SkinControlAttribute(2)]
        protected GUIButtonControl btnTop = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl btnNew = null;
        [SkinControlAttribute(8)]
        protected GUIButtonControl btnPlayAll = null;
        [SkinControlAttribute(3)]
        protected GUIButtonControl btnSearch = null;
        [SkinControlAttribute(6)]
        protected GUIButtonControl btnFavorites = null;
        [SkinControlAttribute(9)]
        protected GUIButtonControl btnBack = null;
        [SkinControlAttribute(25)]
        protected GUIButtonControl btnPlayList = null;
        [SkinControlAttribute(34)]
        protected GUIButtonControl btnNextPage = null;
        [SkinControlAttribute(35)]
        protected GUIButtonControl btnPreviousPage = null;
        [SkinControlAttribute(36)]
        protected GUIImage imgCountry = null;
        [SkinControlAttribute(37)]
        protected GUIButtonControl btnGenre = null;
        [SkinControlAttribute(38)]
        protected GUIButtonControl btnCountry = null;
        [SkinControlAttribute(39)]
        protected GUIButtonControl btnMyPlaylists = null;
        [SkinControlAttribute(50)]
        protected GUIListControl listSongs = null;
        [SkinControlAttribute(111)]
        protected GUIGroup grp1 = null;
        [SkinControlAttribute(222)]
        protected GUIGroup grp2 = null;
        #endregion
        #region Enumerations
        enum State
        {
            HOME = -1,
            TOP = 0,
            SEARCH = 1,
            FAVORITE = 2,
            NEW = 3,
            GENRE = 4
            //,PLAYLIST = 5
        };

        #endregion

        #region variables
        private int WINDOW_ID = 4734;
        private YahooSettings moSettings;

        //private Hashtable moYahooSiteTable;
        YahooTopVideos moTopVideos;
        YahooNewVideos moNewVideos;
        YahooSearch moYahooSearch;
        YahooFavorites moFavoriteManager;
        YahooGenres moGenre;
        //String lsSelectedBitRate = "";
        //String lsSelectedCountry = "";
        public int CURRENT_STATE = (int)State.HOME;
        int miSelectedIndex = 0;
        YahooVideo moCurrentPlayingVideo;
        string msSelectedGenre;
        #endregion

        public GUIMusicVideos()
        {
        }


        #region ISetupForm Members
        public bool CanEnable()
        {
            return true;
        }

        public string PluginName()
        {
            return "My Music Videos ";
        }

        public bool DefaultEnabled()
        {
            return false;
        }

        public bool HasSetup()
        {
            return true;
        }


        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {

            strButtonText = GUILocalizeStrings.Get(30000);// "My MusicVideos";
            strButtonImage = "";
            strButtonImageFocus = "";
            strPictureImage = "hover_musicvideo.png";
            return true;
        }

        public string Author()
        {
            return "Gregmac45";
        }

        public string Description()
        {
            return "This plugin shows online music videos from Yahoo";
        }


        public bool ShowDefaultHome()
        {
            return true;
        }
        public void ShowPlugin() // show the setup dialog
        {
            System.Windows.Forms.Form setup = new SetupForm();
            setup.ShowDialog();
        }
        public int GetWindowId()
        {
            return GetID;
        }
        #endregion


        #region GUIWindow Overrides

        public override int GetID
        {
            get { return WINDOW_ID; }
            set { base.GetID = value; }
        }

        public override bool Init()
        {            
            return Load(GUIGraphicsContext.Skin + @"\mymusicvideos.xml");
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            //labelState.Label = "";
            base.OnPageDestroy(new_windowId);
        }

        public override void OnAction(Action action)
        {
            if (action.wID != Action.ActionType.ACTION_MOUSE_MOVE)
            {
                Log.Info("action wID = {0}", action.wID);
            }

            if (action.wID == Action.ActionType.ACTION_NEXT_ITEM)
            {
                //Log.Info("Next item values: {0},{1},{2},{3}", action.fAmount1, action.fAmount2, action.m_key.KeyCode, action.IsUserAction());
                //MusicVideoPlaylist.getInstance().PlayNext();
                //listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            }
            if (action.wID == Action.ActionType.ACTION_PREV_ITEM)
            {
                //MusicVideoPlaylist.getInstance().PlayPrevious();
                //listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            }
            if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU && CURRENT_STATE != (int)State.HOME)
            {
                //EnableHomeButtons();
                //listSongs.Clear();

                //clear the state label
                //labelState.Label = "";
                //labelSelected.Label = "";
                CURRENT_STATE = (int)State.HOME;
                updateButtonStates();
                //GUI                
                this.LooseFocus();
                btnTop.Focus = true;
                return;
            }

            base.OnAction(action);
        }
        public override bool OnMessage(GUIMessage message)
        {
            if (GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED != message.Message
                && GUIMessage.MessageType.GUI_MSG_SETFOCUS != message.Message)
            {
                Log.Info("Message = {0}", message.Message);
            }
            
            if (GUIMessage.MessageType.GUI_MSG_SETFOCUS == message.Message)
            {
                if (message.TargetControlId == listSongs.GetID && listSongs.Count > 0)
                {
                    ////labelSelected.Label = "Press Menu or F9 for more options.";
                    // todo : show current title here...
                    //GUIPropertyManager.SetProperty("#title", listSongs.SelectedItem);
                }
                else
                {
                    //labelSelected.Label = "";
                }
            }
            //else if (GUIMessage.MessageType.GUI_MSG_WINDOW_INIT == message.Message && g_Player.Playing() ){
            //{
            //    miSelectedIndex= MusicVideoPlaylist.getInstance().getPlayListIndex();
            //    listSongs.SelectedListItemIndex = miSelectedIndex;
            //}

            return base.OnMessage(message);
        }
        protected override void OnPreviousWindow()
        {
            if (g_Player.Playing)
            {
                Log.Info("in OnPreviousWindow and g_player is playing");
                //if (MusicVideoPlaylist.getInstance().isPlaying())
                //{
                //    moCurrentPlayingVideo = MusicVideoPlaylist.getInstance().getCurrentPlayingVideo();
                //}

                GUIPropertyManager.SetProperty("#Play.Current.Title", moCurrentPlayingVideo.artistName + "-" + moCurrentPlayingVideo.songName);
                //GUIPropertyManager.SetProperty("#Play.Current.File", loVideo.songName);
            }
            base.OnPreviousWindow();
        }
        protected override void OnPageLoad()
        {            
            if (moSettings == null)
            {
                moSettings = YahooSettings.getInstance();
            }
            Log.Info("Image filename = '{0}'", imgCountry.FileName);
            if (String.IsNullOrEmpty(imgCountry.FileName))
            {
                Log.Info("Updating country image");
                YahooUtil loUtil = YahooUtil.getInstance();
                string lsCountryId = loUtil.getYahooSite(moSettings.msDefaultCountryName).countryId;
                Log.Info("country image -country id = {0}", lsCountryId);
                imgCountry.SetFileName(GUIGraphicsContext.Skin + @"\media\" + lsCountryId + ".png");
            }

            if (CURRENT_STATE == (int)State.HOME)
            {
                //EnableHomeButtons();
                updateButtonStates();
                //moGenre = new YahooGenres();
                this.LooseFocus();
                btnTop.Focus = true;
            }
            else
            {
                if (CURRENT_STATE == (int)State.TOP)
                { // 30001 = Most wanted , 30008 = Rank
                    //refreshStage2Screen(GUILocalizeStrings.Get(30001),GUILocalizeStrings.Get(30008) + " " + moTopVideos.getFirstVideoRank() + "-" + moTopVideos.getLastVideoRank());
                    refreshStage2Screen();
                }
                else if (CURRENT_STATE == (int)State.NEW)
                { // 30002 = New on Yahoo , 30009 = Page
                    //refreshStage2Screen(GUILocalizeStrings.Get(30002),GUILocalizeStrings.Get(30009) + " " + moNewVideos.getCurrentPageNumber());
                    refreshStage2Screen();
                    //refreshStage2Screen(String.Format("New Yahoo Videos - Page {0} ", moNewVideos.getCurrentPageNumber()));
                }
                else if (CURRENT_STATE == (int)State.FAVORITE)
                { // 932 = Favorites
                    //refreshStage2Screen(GUILocalizeStrings.Get(932) + moFavoriteManager.getSelectedFavorite(),moFavoriteManager.moFavoriteList.Count+"");
                    refreshStage2Screen();
                    //refreshStage2Screen(String.Format("Favorite - {0}", moFavoriteManager.getSelectedFavorite()));
                }
                else if (CURRENT_STATE == (int)State.SEARCH)
                { // 30010 = Search results for {0} - Page {1}
                    //refreshStage2Screen(String.Format(GUILocalizeStrings.Get(30010), moYahooSearch.getLastSearchText(), moYahooSearch.getCurrentPageNumber()),moYahooSearch.moLastSearchResult.Count+"");
                    refreshStage2Screen();
                }
                    /*
                else if (CURRENT_STATE == (int)State.PLAYLIST)
                {
                    //labelState.Label = GUILocalizeStrings.Get(136); //"PlayList";
                    DisableAllButtons();
                    enablePlaylistButtons();
                    //miSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
                    refreshScreenVideoList();
                }
                     * */
                else if (CURRENT_STATE == (int)State.GENRE)
                {
                    //refreshStage2Screen(String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber()),moGenre.moGenreVideoList.Count+"");
                    refreshStage2Screen();
                }
                this.LooseFocus();
                listSongs.Focus = true;
                ////labelSelected.Label = "Press Menu or F9 for more options.";

            }
            if (g_Player.Playing)
            {
                //if (MusicVideoPlaylist.getInstance().isPlaying())
                //{
                 //   moCurrentPlayingVideo = MusicVideoPlaylist.getInstance().getCurrentPlayingVideo();
                //}
                if (moCurrentPlayingVideo != null)
                {
                    GUIPropertyManager.SetProperty("#Play.Current.Title", moCurrentPlayingVideo.artistName + " - " + moCurrentPlayingVideo.songName);
                }
            }

        }
        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            //Log.Info("GUIMusicVideo: Clicked control = {0}", control);
            Log.Info("GUIMusicVideo: Clicked");
            if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
            {
                Log.Info("Caught on Queue action for list item {0}", listSongs.SelectedListItemIndex);
                OnQueueItem();
                return;
            }
            if (control == listSongs)
            {
                miSelectedIndex = listSongs.SelectedListItemIndex;
                //if (CURRENT_STATE == (int)State.PLAYLIST)
                //{
                //    MusicVideoPlaylist.getInstance().Play(miSelectedIndex);
                //}
                //else
                //{
                    playVideo(getSelectedVideo());
                //}

            }
            else if (control == btnTop)
            {
                onClickTopVideos();
            }
            else if (control == btnNew)
            {
                //btnNew.Focus = false;
                onClickNewVideos();
            }
            else if (control == btnSearch)
            {
                SearchVideos(true, String.Empty);
                //btnSearch.Focus = false;
            }
            else if (control == btnFavorites)
            {
                onClickFavorites();
                //btnFavorites.Focus = false;
            }
            else if (control == btnGenre)
            {
                onClickGenre();
                //btnGenre.Focus = false;
            }
            else if (control == btnBack)
            {
                //EnableHomeButtons();
                //clear the list
                //listSongs.Clear();
                //clear the state label
                //labelState.Label = "";
                CURRENT_STATE = (int)State.HOME;
                updateButtonStates();
                this.LooseFocus();
                btnTop.Focus = true;
            }

            else if (control == btnPlayAll)
            {

                //BackgroundWorker worker = new BackgroundWorker();          
                //PlayListLoader loLoader = new PlayListLoader();
                //worker.DoWork += new DoWorkEventHandler(loLoader.LoadWorker);
                //worker.RunWorkerAsync(getStateVideoList());
                //using (WaitCursor cursor = new WaitCursor())
                // {
                //    while (loLoader.mbFirstVideoLoaded == false)
                //Log.Info("loLoader.mbFirstVideoLoaded = " + loLoader.mbFirstVideoLoaded);
                //Log.Info("PlayListCount = " + PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Count);
                //        GUIWindowManager.Process();
                //}
                //Log.Info("PlayListCount = " + PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO_TEMP).Count);
                //PlayListPlayer.SingletonPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO_TEMP;
                //PlayListPlayer.SingletonPlayer.PlayNext();
                //////////////

                //new as of 07/25/2006

                OnQueueAllItems(getStateVideoList());
                //MusicVideoPlaylist.getInstance().Play();
                //listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
                //miSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();        
                //////////////
                //btnPlayAll.Focus = false;
                //this.LooseFocus();

            }
            else if (control == btnPlayList)
            {
                onClickPlaylist();
                //btnPlayList.Focus = false;
            }
            else if (control == btnMyPlaylists)
            {
                MusicVideoDatabase loDatabase = MusicVideoDatabase.getInstance();
                ArrayList loPlayListNames = loDatabase.getPlaylists();
                if(loPlayListNames.Count>0){
                    
                
                GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dlgSel.Reset();
                if (dlgSel != null)
                {
                    foreach(String lsName in loPlayListNames){
                        dlgSel.Add(lsName);
                    }
                    
                    dlgSel.SetHeading(GUILocalizeStrings.Get(983)); // My Playlists
                    dlgSel.DoModal(GetID);
                    int liSelectedIdx = dlgSel.SelectedId;
                    if (liSelectedIdx > 0)
                    {
                        Log.Info("you selected playlist :{0}", loPlayListNames[liSelectedIdx-1]);
                        PlayListPlayer loPlayer = PlayListPlayer.SingletonPlayer;
                        if (g_Player.Playing)
                        {
                            g_Player.Stop();
                        }
                        
                        loPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_VIDEO).Clear();
                        loPlayer.Reset();
                        OnQueueAllItems(loDatabase.getPlayListVideos(loPlayListNames[liSelectedIdx -1].ToString()));
                    }
                }
            }
            }
            //else if (control == btnPlayListPlay)
            //{
            //MusicVideoPlaylist.getInstance().Play();
            //listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //miSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //btnPlayListPlay.Focus = false;
            // }
            //else if (control == btnPlayListStop)
            //{
            //    Log.Info("GUIMusicVideo: Playlist Stop button clicked.");
            //MusicVideoPlaylist.getInstance().Stop();
            //}
            //else if (control == btnPlayListNext)
            //{
            //MusicVideoPlaylist.getInstance().PlayNext();
            //listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //miSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //}
            //else if (control == btnPlayListPrevious)
            // {
            //MusicVideoPlaylist.getInstance().PlayPrevious();
            //listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //miSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //}
            //else if (control == btnPlayListBack)
            //{
            //    EnableHomeButtons();
            //    //clear the list
            //    listSongs.Clear();
            //clear the state label
            //    //labelState.Label = "";
            //    CURRENT_STATE = (int)State.HOME;
            //    this.LooseFocus();
            //    btnTop.Focus = true;
            //}
            //else if (control == btnPlayListRepeat)
            //{
            //MusicVideoPlaylist loPlayList = MusicVideoPlaylist.getInstance();

//loPlayList.repeat(!loPlayList.getRepeatState());
            //btnPlayListRepeat.
            //}
            //else if (control == btnPlayListShuffle)
            //{
            //MusicVideoPlaylist loPlayList = MusicVideoPlaylist.getInstance();
            //loPlayList.shuffle();
            //DisplayVideoList(loPlayList.getPlayListVideos());
            //listSongs.SelectedListItemIndex = loPlayList.getPlayListIndex();
            //miSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
            //}
            //else if (control == btnPlayListClear)
            //{
            //MusicVideoPlaylist.getInstance().Clear();
            //    listSongs.Clear();
            //}
            else if (control == btnNextPage)
            {
                OnClickNextPage();
            }
            else if (control == btnPreviousPage)
            {
                OnClickPreviousPage();
            }
            else if (control == btnCountry)
            {
                onClickChangeCountry();
            }

            //RefreshPage();
        }
        public override void Process()
        {
            base.Process();
        }
        protected override void OnShowContextMenu()
        {
            YahooVideo loVideo = getSelectedVideo();
            if (loVideo == null)
            {
                return;
            }
            GUIListItem loSelectVideo = listSongs.SelectedListItem;
            int liSelectedIndex = listSongs.SelectedListItemIndex;
            if (liSelectedIndex > -1)
            {
                GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dlgSel.Reset();
                if (dlgSel != null)
                {
                    dlgSel.Add(GUILocalizeStrings.Get(208)); // Play
                    dlgSel.Add(GUILocalizeStrings.Get(926)); // Add to playList
                    if ((int)State.FAVORITE == CURRENT_STATE)
                        dlgSel.Add(GUILocalizeStrings.Get(933)); // Remove from favorites
                    else
                        dlgSel.Add(GUILocalizeStrings.Get(930)); // Add to favorites
                    dlgSel.Add(GUILocalizeStrings.Get(30007)); // Search other videos by this artist
                    //dlgSel.Add("Artist Biography");
                    dlgSel.SetHeading(GUILocalizeStrings.Get(924)); // Menu 
                    dlgSel.DoModal(GetID);
                    int liSelectedIdx = dlgSel.SelectedId;
                    Log.Info("you selected action :{0}", liSelectedIdx);
                    switch (liSelectedIdx)
                    {
                        case 1:
                            playVideo(loVideo);
                            break;
                        case 2:
                            //MusicVideoPlaylist.getInstance().AddToPlayList(loVideo); 
                            //if(listSongs.SelectedListItemIndex+1<listSongs.Count){
                            //    listSongs.SelectedListItemIndex = listSongs.SelectedListItemIndex+1;
                            //}
                            OnQueueItem();
                            break;
                        case 3:
                            if (CURRENT_STATE == (int)State.FAVORITE)
                            {
                                moFavoriteManager.removeFavorite(loVideo);
                                DisplayVideoList(moFavoriteManager.getFavoriteVideos());
                            }
                            else
                            {
                                //prompt user for favorite list to add to
                                string lsSelectedFav = promptForFavoriteList();
                                Log.Info("adding to favorites.");
                                if (moFavoriteManager == null)
                                {
                                    moFavoriteManager = new YahooFavorites();
                                }
                                moFavoriteManager.setSelectedFavorite(lsSelectedFav);
                                moFavoriteManager.addFavorite(loVideo);
                            }
                            break;
                        case 4:
                            SearchVideos(false, loVideo.artistName);
                            break;
                            /*
                        case 5:
                            String lsBio = YahooUtil.getInstance().getArtistBio(loVideo.artistId);
                            GUIDialogText loDlgTxt = (GUIDialogText)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TEXT);
                            loDlgTxt.Reset();
                            loDlgTxt.SetText(lsBio.Trim());
                            loDlgTxt.SetHeading(GUILocalizeStrings.Get(1011) + " - " + loVideo.artistName);
                            if (File.Exists(@"thumbs\MPTemp -" + loVideo.artistId + ".jpg") == true)
                            {
                                loDlgTxt.SetImage(@"thumbs\MPTemp -" + loVideo.artistId + ".jpg");
                            }
                            loDlgTxt.DoModal(GetID);
                            break;
                             */
                    }
                }
            }
        }
        #endregion
        #region userdefined methods
        private void onClickFavorites()
        {
            if (moFavoriteManager == null)
            {
                moFavoriteManager = new YahooFavorites();
            }

            string lsSelectedFav = promptForFavoriteList();
            if (String.IsNullOrEmpty(lsSelectedFav))
            {
                return;
            }
            /*
            DisableAllButtons();
            btnPlayAll.Visible = true;
            btnBack.Visible = true;
            btnPlayList.Visible = true;
            btnNextPage.Visible = true;
            btnPreviousPage.Visible = true;

            listSongs.NavigateLeft = btnBack.GetID;
            listSongs.NavigateRight = btnBack.GetID;
            */
            /*
            miSelectedIndex = 0;
            btnNextPage.Visible = false;
            btnPreviousPage.Visible = false;
            btnPlayList.NavigateUp = btnPlayAll.GetID;
            btnPlayList.NavigateDown = btnBack.GetID;
            */
            CURRENT_STATE = (int)State.FAVORITE;
            

            if (lsSelectedFav != null || lsSelectedFav.Length > 0)
            {
                moFavoriteManager.setSelectedFavorite(lsSelectedFav);
            }
            DisplayVideoList(moFavoriteManager.getFavoriteVideos());
            //labelState.Label = (GUILocalizeStrings.Get(932) + " - " + moFavoriteManager.getSelectedFavorite());
            updateButtonStates();
            if (listSongs.Count == 0)
            {
                this.LooseFocus();
                btnBack.Focus = true;
            }
        }

        private void onClickPlaylist()
        {
            /*
            enablePlaylistButtons();
            CURRENT_STATE = (int)State.PLAYLIST;
            miSelectedIndex = 0;
            DisplayVideoList(MusicVideoPlaylist.getInstance().getPlayListVideos());
            //labelState.Label = GUILocalizeStrings.Get(136); //"PlayList";
            if (listSongs.Count == 0)
            {
              this.LooseFocus();
              btnPlayListBack.Focus = true;
            }
             */
            if (GetID == GUIWindowManager.ActiveWindow)
            {
                GUIWindowManager.ActivateWindow(4735);
            }
        }

        private string promptForGenre()
        {
            string lsSelectedGenre = "";
            ArrayList loGenreNames = moGenre.moSortedGenreList;

            GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlgSel.Reset();
            if (dlgSel != null)
            {
                foreach (string lsGenreNm in loGenreNames)
                {
                    dlgSel.Add(lsGenreNm);
                }
                dlgSel.SetHeading(GUILocalizeStrings.Get(924)); // Menu
                dlgSel.DoModal(GetID);
                if (dlgSel.SelectedLabel == -1)
                {
                    return "";
                }
                Log.Info("you selected genre :{0}", dlgSel.SelectedLabelText);
                lsSelectedGenre = dlgSel.SelectedLabelText;
            }
            return lsSelectedGenre;
        }

        private string promptForFavoriteList()
        {

            string lsSelectedFav = "";
            if (moFavoriteManager == null)
            {
                moFavoriteManager = new YahooFavorites();
            }
            ArrayList loFavNames = moFavoriteManager.getFavoriteNames();
            if (loFavNames.Count > 1)
            {
                GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                dlgSel.Reset();
                if (dlgSel != null)
                {
                    foreach (string lsFavNm in loFavNames)
                    {
                        dlgSel.Add(lsFavNm);
                    }
                    dlgSel.SetHeading(GUILocalizeStrings.Get(924)); // Menu
                    dlgSel.DoModal(GetID);
                    if (dlgSel.SelectedLabel == -1)
                    {
                        return "";
                    }
                    Log.Info("you selected favorite :{0}", dlgSel.SelectedLabelText);
                    lsSelectedFav = dlgSel.SelectedLabelText;
                }
            }
            else
            {
                lsSelectedFav = (string)loFavNames[0];
            }
            return lsSelectedFav;
        }
        private void onClickChangeCountry()
        {
            GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlgSel.Reset();
            if (dlgSel != null)
            {
                String[] loCountryArray = new String[moSettings.moYahooSiteTable.Keys.Count];
                moSettings.moYahooSiteTable.Keys.CopyTo(loCountryArray, 0);
                Array.Sort(loCountryArray);

                foreach (string country in loCountryArray)
                {
                    //Console.WriteLine("country = {0}", country);
                    dlgSel.Add(country);
                }
                dlgSel.SetHeading(GUILocalizeStrings.Get(924)); // Menu
                dlgSel.DoModal(GetID);
                if (dlgSel.SelectedLabel == -1)
                {
                    return;
                }
                Log.Info("you selected country :{0}", dlgSel.SelectedLabelText);
                moSettings.msDefaultCountryName = dlgSel.SelectedLabelText;
                moTopVideos = new YahooTopVideos(moSettings.msDefaultCountryName);
                RefreshPage();
            }
            //return lsSelectedGenre;
        }
        private void onClickNewVideos()
        {
            miSelectedIndex = 0;

            CURRENT_STATE = (int)State.NEW;
            Log.Info("button new clicked");
            if (moNewVideos == null)
            {
                moNewVideos = new YahooNewVideos();
            }
            moNewVideos.loadNewVideos(moSettings.msDefaultCountryName);
            Log.Info("The new video page has next video ={0}", moNewVideos.hasNext());
            if (moNewVideos.hasNext())
                btnNextPage.Disabled = false;
            else
                btnNextPage.Disabled = true;

            btnPreviousPage.Disabled = true;
            //refreshStage2Screen(//labelState.Label = String.Format("New Yahoo Videos - Page {0} ", moNewVideos.getCurrentPageNumber()));
            //refreshStage2Screen(GUILocalizeStrings.Get(30002) + " - " + GUILocalizeStrings.Get(30009) + " " + moNewVideos.getCurrentPageNumber(),moNewVideos.moNewVideoList.Count+"");
            refreshStage2Screen();
        }
        private void onClickGenre()
        {
            miSelectedIndex = 0;
            if (moGenre == null)
                moGenre = new YahooGenres();

            msSelectedGenre = promptForGenre();
            if (String.IsNullOrEmpty(msSelectedGenre))
            {
                return;
            }
            CURRENT_STATE = (int)State.GENRE;

            Log.Info("button GENRE clicked");

            moGenre.loadFirstGenreVideos(msSelectedGenre);

            if (moGenre.hasNext())
                btnNextPage.Disabled = false;
            else
                btnNextPage.Disabled = true;

            btnPreviousPage.Disabled = true;
            //refreshStage2Screen(String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber()));
            //refreshStage2Screen(String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber()),moGenre.moGenreVideoList.Count+"");
            refreshStage2Screen();
        }
        private void OnQueueItem()
        {
            //if (CURRENT_STATE != (int)State.PLAYLIST)
            //{
                PlayListPlayer loPlaylistPlayer = PlayListPlayer.SingletonPlayer;
                loPlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
                PlayList loPlaylist = loPlaylistPlayer.GetPlaylist(loPlaylistPlayer.CurrentPlaylistType);
                MVPlayListItem loItem;
                YahooVideo loVideo = getSelectedVideo();


                loItem = new MVPlayListItem();
                loItem.YahooVideo = loVideo;
                loPlaylist.Add(loItem);


                if (listSongs.SelectedListItemIndex + 1 < listSongs.Count)
                {
                    listSongs.SelectedListItemIndex = listSongs.SelectedListItemIndex + 1;
                }
            //}
        }
        private void OnQueueAllItems( List<YahooVideo> foVideoList)
        {
            Log.Info("in Onqueue All");
            //List<YahooVideo> loVideoList = getStateVideoList();
            PlayListPlayer loPlaylistPlayer = PlayListPlayer.SingletonPlayer;
            loPlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
            PlayList loPlaylist = loPlaylistPlayer.GetPlaylist(loPlaylistPlayer.CurrentPlaylistType);
            MVPlayListItem loItem;
            foreach (YahooVideo loVideo in foVideoList)
            {
                loItem = new MVPlayListItem();
                loItem.YahooVideo = loVideo;
                loPlaylist.Add(loItem);
            }
            Log.Info("current playlist type:{0}", loPlaylistPlayer.CurrentPlaylistType);
            Log.Info("playlist count:{0}", loPlaylistPlayer.GetPlaylist(loPlaylistPlayer.CurrentPlaylistType));

            onClickPlaylist();
            loPlaylistPlayer.PlayNext();

            //g_Player.FullScreen = true;
            //GUIGraphicsContext.IsFullScreenVideo = true;
            //GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);


        }
        //private void DeQueueItem()
        //{
        //    MusicVideoPlaylist.getInstance().(getSelectedVideo);
        // }
        private void SearchVideos(bool fbClicked, String fsSearchTxt)
        {
            /*
            DisableAllButtons();
            btnPlayAll.Visible = true;
            btnBack.Visible = true;
            btnPlayList.Visible = true;
            btnNextPage.Visible = true;
            btnPreviousPage.Visible = true;
            listSongs.NavigateLeft = btnBack.GetID;
            listSongs.NavigateRight = btnBack.GetID;
            
            miSelectedIndex = 0;
             */
            CURRENT_STATE = (int)State.SEARCH;
            if (moYahooSearch == null)
            {
                moYahooSearch = new YahooSearch(moSettings.msDefaultCountryName);
            }

            //clear the list
            listSongs.Clear();
            if (fbClicked)
            {
                moYahooSearch.searchVideos(getUserTypedText());
            }
            else
            {
                moYahooSearch.searchVideos(fsSearchTxt);
            }
            DisplayVideoList(moYahooSearch.moLastSearchResult);

            //labelState.Label = String.Format(GUILocalizeStrings.Get(30010), moYahooSearch.getLastSearchText(), moYahooSearch.getCurrentPageNumber());
            btnNextPage.Disabled = !moYahooSearch.hasNext();
            btnPreviousPage.Disabled = true;
        }
        private void onClickTopVideos()
        {
            CURRENT_STATE = (int)State.TOP;
            miSelectedIndex = 0;
            if (moTopVideos == null)
            {
                moTopVideos = new YahooTopVideos(moSettings.msDefaultCountryName);
            }
            moTopVideos.loadFirstPage();
            if (moTopVideos.hasMorePages())
            {
                btnNextPage.Disabled = false;
            }
            else
            {
                btnNextPage.Disabled = true;
            }
            btnPreviousPage.Disabled = true;
            //refreshStage2Screen(GUILocalizeStrings.Get(30001) + " " + GUILocalizeStrings.Get(30008) + " " + moTopVideos.getFirstVideoRank() + "-" + moTopVideos.getLastVideoRank(),moTopVideos.getLastLoadedList().Count+"");
            refreshStage2Screen();
        }
        private void OnClickNextPage()
        {
            miSelectedIndex = 0;
            bool lbNext = false;
            bool lbPrevious = false;
            switch (CURRENT_STATE)
            {
                case (int)State.NEW:
                    moNewVideos.loadNextVideos(moSettings.msDefaultCountryName);
                    lbNext = moNewVideos.hasNext();
                    lbPrevious = moNewVideos.hasPrevious();
                    DisplayVideoList(moNewVideos.moNewVideoList);
                    //labelState.Label = GUILocalizeStrings.Get(30002) + " - " + GUILocalizeStrings.Get(30009) + " " + moNewVideos.getCurrentPageNumber();
                    break;
                case (int)State.TOP:
                    moTopVideos.loadNextPage();
                    lbNext = moTopVideos.hasMorePages();
                    lbPrevious = moTopVideos.hasPreviousPage();
                    DisplayVideoList(moTopVideos.getLastLoadedList());
                    ////labelState.Label = String.Format("Top Yahoo Videos {0} - {1} ", moTopVideos.getFirstVideoRank(), moTopVideos.getLastVideoRank());
                    //labelState.Label = GUILocalizeStrings.Get(30001) + " " + GUILocalizeStrings.Get(30008) + " " + moTopVideos.getFirstVideoRank() + "-" + moTopVideos.getLastVideoRank();
                    break;
                case (int)State.SEARCH:
                    moYahooSearch.loadNextVideos();
                    lbNext = moYahooSearch.hasNext();
                    lbPrevious = moYahooSearch.hasPrevious();
                    DisplayVideoList(moYahooSearch.moLastSearchResult);
                    //labelState.Label = String.Format(GUILocalizeStrings.Get(30010), moYahooSearch.getLastSearchText(), moYahooSearch.getCurrentPageNumber());
                    break;
                case (int)State.GENRE:
                    moGenre.loadNextVideos();
                    lbNext = moGenre.hasNext();
                    lbPrevious = moGenre.hasPrevious();
                    DisplayVideoList(moGenre.moGenreVideoList);
                    //labelState.Label = String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber());
                    //          //labelState.Label = String.Format("Genre: {0} - Page {1} ", msSelectedGenre, moGenre.getCurrentPageNumber());
                    break;
            }
            Log.Info("The video page has next video ={0}", lbNext);
            Log.Info("The video page has previous video ={0}", lbPrevious);

            btnNextPage.Disabled = !lbNext;
            btnPreviousPage.Disabled = !lbPrevious;
            updateButtonStates();
        }
        private void OnClickPreviousPage()
        {
            miSelectedIndex = 0;
            bool lbNext = false;
            bool lbPrevious = false;
            switch (CURRENT_STATE)
            {
                case (int)State.NEW:
                    moNewVideos.loadPreviousVideos(moSettings.msDefaultCountryName);
                    lbNext = moNewVideos.hasNext();
                    lbPrevious = moNewVideos.hasPrevious();
                    DisplayVideoList(moNewVideos.moNewVideoList);
                    //labelState.Label = GUILocalizeStrings.Get(30002) + " - " + GUILocalizeStrings.Get(30009) + " " + moNewVideos.getCurrentPageNumber();
                    break;
                case (int)State.TOP:
                    moTopVideos.loadPreviousPage();
                    lbNext = moTopVideos.hasMorePages();
                    lbPrevious = moTopVideos.hasPreviousPage();
                    DisplayVideoList(moTopVideos.getLastLoadedList());
                    //labelState.Label = GUILocalizeStrings.Get(30001) + " " + GUILocalizeStrings.Get(30008) + " " + moTopVideos.getFirstVideoRank() + "-" + moTopVideos.getLastVideoRank();
                    break;
                case (int)State.SEARCH:
                    moYahooSearch.loadPreviousVideos();
                    lbNext = moYahooSearch.hasNext();
                    lbPrevious = moYahooSearch.hasPrevious();
                    DisplayVideoList(moYahooSearch.moLastSearchResult);
                    //labelState.Label = String.Format(GUILocalizeStrings.Get(30010), moYahooSearch.getLastSearchText(), moYahooSearch.getCurrentPageNumber());
                    break;
                case (int)State.GENRE:
                    moGenre.loadPreviousVideos();
                    lbNext = moGenre.hasNext();
                    lbPrevious = moGenre.hasPrevious();
                    DisplayVideoList(moGenre.moGenreVideoList);
                    //labelState.Label = String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber());
                    break;
            }
            Log.Info("The video page has next video ={0}", lbNext);
            Log.Info("The video page has previous video ={0}", lbPrevious);

            btnNextPage.Disabled = !lbNext;
            btnPreviousPage.Disabled = !lbPrevious;
            updateButtonStates();
        }
        private List<YahooVideo> getStateVideoList()
        {
            List<YahooVideo> loCurrentDisplayVideoList = null;
            switch (CURRENT_STATE)
            {
                case (int)State.TOP:
                    loCurrentDisplayVideoList = moTopVideos.getLastLoadedList();
                    break;
                case (int)State.NEW:
                    loCurrentDisplayVideoList = moNewVideos.moNewVideoList;
                    break;
                case (int)State.SEARCH:
                    loCurrentDisplayVideoList = moYahooSearch.moLastSearchResult;
                    break;
                case (int)State.FAVORITE:
                    loCurrentDisplayVideoList = moFavoriteManager.getFavoriteVideos();
                    break;
                //case (int)State.PLAYLIST:
                    //loCurrentDisplayVideoList = MusicVideoPlaylist.getInstance().getPlayListVideos();
                //    break;
                case (int)State.GENRE:
                    loCurrentDisplayVideoList = moGenre.moGenreVideoList;
                    break;
                default: break;
            }
            return loCurrentDisplayVideoList;
        }
        private YahooVideo getSelectedVideo()
        {
            YahooVideo loVideo = null;

            List<YahooVideo> loCurrentDisplayVideoList = getStateVideoList();

            if (loCurrentDisplayVideoList != null && loCurrentDisplayVideoList.Count > 0)
            {
                loVideo = loCurrentDisplayVideoList[listSongs.SelectedListItemIndex];
            }
            return loVideo;
        }
        private string getUserTypedText()
        {
            string KB_Search_Str = "";
            VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            keyBoard.Text = "";
            keyBoard.Reset();
            keyBoard.DoModal(GUIWindowManager.ActiveWindow); // show it...            
            System.GC.Collect(); // collect some garbage
            if (keyBoard.Text == "" || keyBoard.Text == null)
            {
                return "";
            }
            KB_Search_Str = keyBoard.Text;
            return KB_Search_Str;
        }
        private void refreshScreenVideoList()
        {
            Log.Info("Refreshing video list on screen");
            List<YahooVideo> loCurrentDisplayVideoList = getStateVideoList();
            DisplayVideoList(loCurrentDisplayVideoList);
            listSongs.SelectedListItemIndex = miSelectedIndex;
        }
        private void DisplayVideoList(List<YahooVideo> foVideoList)
        {
            if (foVideoList == null && foVideoList.Count < 1) { return; }
            listSongs.Clear();
            GUIListItem item = null;
            int liVideoListSize = foVideoList.Count;
            foreach (YahooVideo loYahooVideo in foVideoList)
            {
                item = new GUIListItem();
                item.DVDLabel = loYahooVideo.songId;
                if (loYahooVideo.artistName == null || loYahooVideo.artistName.Equals(""))
                {
                    item.Label = loYahooVideo.songName;
                }
                else
                {
                    item.Label = loYahooVideo.artistName + " - " + loYahooVideo.songName;
                }
                item.IsFolder = false;
                //item.MusicTag = true;
                listSongs.Add(item);
            }
            this.LooseFocus();
            listSongs.Focus = true;
            if (listSongs.Count > 0)
            {
                ////labelSelected.Label = "Press Menu or F9 for more options.";
            }
            else
            {
                //labelSelected.Label = "";
            }
        }
        void playVideo(YahooVideo video)
        {
            Log.Info("in playVideo()");
            string lsVideoLink = null;
            YahooSite loSite;
            YahooUtil loUtil = YahooUtil.getInstance();
            loSite = loUtil.getYahooSiteById(video.countryId);
            lsVideoLink = loUtil.getVideoMMSUrl(video, moSettings.msDefaultBitRate);
            lsVideoLink = lsVideoLink.Substring(0, lsVideoLink.Length - 2) + "&txe=.wmv";
            if (moSettings.mbUseVMR9)
            {
                g_Player.PlayVideoStream(lsVideoLink);
            }
            else
            {
                g_Player.PlayAudioStream(lsVideoLink);
            }
            if (g_Player.Playing)
            {
                Log.Info("Playing Video:{0}", video.songName);
                GUIGraphicsContext.IsFullScreenVideo = true;
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                moCurrentPlayingVideo = video;
            }
            else
            {
                Log.Info("GUIMusicVideo: Unable to play {0}", lsVideoLink);
            }
        }
        
        //public void refreshStage2Screen(String title,String itemCount)
        public void refreshStage2Screen()
        {
            updateButtonStates();
            //GUIPropertyManager.SetProperty("#header.label", GUILocalizeStrings.Get(30000) + ":" + title);
            //GUIPropertyManager.SetProperty("#itemcount", itemCount);
            refreshScreenVideoList();
        }
        void RefreshPage()
        {
            this.Restore();
            this.Init();
            this.Render(0);
            this.OnPageLoad();
        }
        private void updateButtonStates()
        {
            if ((int)State.HOME == CURRENT_STATE)
            {
                
                
                btnTop.Visible = true;
                btnSearch.Visible = true;
                btnFavorites.Visible = true;                
                btnGenre.Visible = true;                
                btnNew.Visible = true;
                btnCountry.Visible = true;

                btnBack.Visible = false;
                btnPlayAll.Visible = false;
                btnNextPage.Visible = false;
                btnPreviousPage.Visible = false;                
                
                btnPlayList.NavigateUp = btnCountry.GetID;
                btnMyPlaylists.NavigateDown = btnTop.GetID;
                listSongs.NavigateLeft = btnTop.GetID;
                listSongs.Clear();
                //GUIPropertyManager.SetProperty("#header.label", GUILocalizeStrings.Get(30000));
                //GUIPropertyManager.SetProperty("#selecteditem","");
                GUIPropertyManager.SetProperty("#itemcount", "");
            }
            else
            {
                btnNextPage.Visible = true;
                btnPreviousPage.Visible = true;
                btnBack.Visible = true;
                btnPlayAll.Visible = true;

                btnTop.Visible = false;
                btnSearch.Visible = false;
                btnFavorites.Visible = false;
                btnGenre.Visible = false;
                btnNew.Visible = false;
                btnCountry.Visible = false;

                
                btnPlayList.NavigateUp = btnPlayAll.GetID;
                btnMyPlaylists.NavigateDown = btnBack.GetID;

                listSongs.NavigateLeft = btnBack.GetID;
                miSelectedIndex = 0;

                String lsItemCount = String.Empty;
                //String lsHeaderLbl = String.Empty;
                //String lsSelectedItem = String.Empty;
                switch (CURRENT_STATE)
                {
                    case (int)State.FAVORITE:
                        lsItemCount = GUILocalizeStrings.Get(932) + " - " + moFavoriteManager.getSelectedFavorite();
                        break;
                    case (int)State.GENRE:
                        lsItemCount = String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber());
                        break;
                    case (int)State.NEW:
                        //lsSelectedItem = GUILocalizeStrings.Get(30002);
                        lsItemCount = GUILocalizeStrings.Get(30002) + " - " + GUILocalizeStrings.Get(30009) + " " + moNewVideos.getCurrentPageNumber();
                        //lsHeaderLbl = GUILocalizeStrings.Get(30000) + ":" + GUILocalizeStrings.Get(30002);
                        break;
                    case (int)State.SEARCH:
                        //lsSelectedItem = GUILocalizeStrings.Get(137);
                        lsItemCount = String.Format(GUILocalizeStrings.Get(30010), moYahooSearch.getLastSearchText(), moYahooSearch.getCurrentPageNumber());
                       // lsHeaderLbl = GUILocalizeStrings.Get(30000) + ":" + GUILocalizeStrings.Get(137);
                        break;
                    case (int)State.TOP:
                        //lsSelectedItem = GUILocalizeStrings.Get(30001);
                        lsItemCount = GUILocalizeStrings.Get(30001) + " " + GUILocalizeStrings.Get(30008) + " " + moTopVideos.getFirstVideoRank() + "-" + moTopVideos.getLastVideoRank();
                        //lsHeaderLbl = GUILocalizeStrings.Get(30000) + ":" + GUILocalizeStrings.Get(30001);
                        break;

                }
                //GUIPropertyManager.SetProperty("#header.label", lsHeaderLbl);
                //GUIPropertyManager.SetProperty("#selecteditem", lsSelectedItem);
                GUIPropertyManager.SetProperty("#itemcount", lsItemCount);
            }
        }
        #endregion
    }
}
