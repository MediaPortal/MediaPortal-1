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

namespace MediaPortal.GUI.MusicVideos
{
  public class GUIMusicVideos : GUIWindow, ISetupForm, IShowPlugin
  {
    #region SkinControlAttributes

    [SkinControlAttribute(2)]     protected GUIButtonControl btnTop = null;
    [SkinControlAttribute(7)]     protected GUIButtonControl btnNew = null;
    [SkinControlAttribute(8)]     protected GUIButtonControl btnPlayAll = null;
    [SkinControlAttribute(3)]     protected GUIButtonControl btnSearch = null;
    [SkinControlAttribute(6)]     protected GUIButtonControl btnFavorites = null;
    [SkinControlAttribute(9)]     protected GUIButtonControl btnBack = null;
    [SkinControlAttribute(25)]    protected GUIButtonControl btnPlayList = null;
    [SkinControlAttribute(26)]    protected GUIButtonControl btnPlayListBack = null;
    [SkinControlAttribute(27)]    protected GUIButtonControl btnPlayListPlay = null;
    [SkinControlAttribute(28)]    protected GUIButtonControl btnPlayListNext = null;
    [SkinControlAttribute(29)]    protected GUIButtonControl btnPlayListPrevious = null;
    [SkinControlAttribute(30)]    protected GUIButtonControl btnPlayListStop = null;
    [SkinControlAttribute(31)]    protected GUIButtonControl btnPlayListShuffle = null;
    [SkinControlAttribute(32)]    protected GUIToggleButtonControl btnPlayListRepeat = null;
    [SkinControlAttribute(33)]    protected GUIButtonControl btnPlayListClear = null;
    [SkinControlAttribute(34)]    protected GUIButtonControl btnNextPage = null;
    [SkinControlAttribute(35)]    protected GUIButtonControl btnPreviousPage = null;
    [SkinControlAttribute(36)]    protected GUIImage imgCountry = null;
    [SkinControlAttribute(37)]    protected GUIButtonControl btnGenre = null;
    [SkinControlAttribute(38)]    protected GUIButtonControl btnCountry = null;
    [SkinControlAttribute(50)]    protected GUIListControl listSongs = null;
    [SkinControlAttribute(100)]   protected GUILabelControl labelState = null;
    [SkinControlAttribute(101)]   protected GUILabelControl labelSelected = null;

    #endregion

    enum State
    {
      HOME = -1,
      TOP = 0,
      SEARCH = 1,
      FAVORITE = 2,
      NEW = 3,
      GENRE = 4,
      PLAYLIST = 5
    };

    #region variables
    private int WINDOW_ID = 4734;
    private YahooSettings _yahooSettings;
 
    YahooTopVideos _yahooTopVideos;
    YahooNewVideos _yahooNewVideos;
    YahooSearch _yahooSearch;
    YahooFavorites _yahooFavoriteManager;
    YahooGenres _yahooGenre;

    public int CURRENT_STATE = (int)State.HOME;
    int _playListSelectedIndex = 0;
    //YahooVideo loCurrentPlayingVideo;
    string _selectedGenre;
    #endregion

    #region ISetupForm Members
    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "My Music Videos";
    }

    public bool DefaultEnabled()
    {
      return true;
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
      return "Gregmac45/rtv";
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
      labelState.Label = "";
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      //Log.Write("action wID = {0}",action.wID);
      if (action.wID == Action.ActionType.ACTION_NEXT_ITEM)
      {       
        MusicVideoPlaylist.getInstance().PlayNext();
        listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      if (action.wID == Action.ActionType.ACTION_PREV_ITEM)
      {
        MusicVideoPlaylist.getInstance().PlayPrevious();
        listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU && CURRENT_STATE != (int)State.HOME)
      {
        EnableHomeButtons();
        listSongs.Clear();

        //clear the state label
        labelState.Label = "";
        labelSelected.Label = "";
        CURRENT_STATE = (int)State.HOME;
        //GUI                
        this.LooseFocus();
        btnTop.Focus = true;
        return;
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      //Log.Write("Message = {0}", message.Message);
      if (GUIMessage.MessageType.GUI_MSG_SETFOCUS == message.Message)
      {
        if (message.TargetControlId == listSongs.GetID && listSongs.Count > 0)
        {
          //labelSelected.Label = "Press Menu or F9 for more options.";
          // todo : show current title here...
          //GUIPropertyManager.SetProperty("#title", listSongs.SelectedItem);
        }
        else
        {
          labelSelected.Label = "";
        }
      }
      //else if (GUIMessage.MessageType.GUI_MSG_WINDOW_INIT == message.Message && g_Player.Playing() ){
      //{
      //    _playListSelectedIndex= MusicVideoPlaylist.getInstance().getPlayListIndex();
      //    listSongs.SelectedListItemIndex = _playListSelectedIndex;
      //}

      return base.OnMessage(message);
    }

    private string BuildStateLabel(State aState)
    {
      switch (aState)
      {
        case State.HOME: //HOME
          return "";
        case State.TOP: //TOP           // 30001 = Most wanted , 30008 = Rank
          return GUILocalizeStrings.Get(30001) + " " + GUILocalizeStrings.Get(30008) + " " + _yahooTopVideos.getFirstVideoRank() + "-" + _yahooTopVideos.getLastVideoRank();
        case State.SEARCH: //SEARCH     // 30010 = Search results for {0} - Page {1}
          return String.Format(GUILocalizeStrings.Get(30010), _yahooSearch.getLastSearchText(), _yahooSearch.getCurrentPageNumber());
        case State.FAVORITE: //FAVORITE // 932 = Favorites
          return GUILocalizeStrings.Get(932) + " - " + _yahooFavoriteManager.getSelectedFavorite();
        case State.NEW: //NEW           // 30002 = New on Yahoo , 30009 = Page
          return GUILocalizeStrings.Get(30002) + " - " + GUILocalizeStrings.Get(30009) + " " + _yahooNewVideos.getCurrentPageNumber();
        case State.GENRE: //GENRE     
          return String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), _selectedGenre, GUILocalizeStrings.Get(30009), _yahooGenre.getCurrentPageNumber());
        case State.PLAYLIST: //PLAYLIST // 136 = PlayList
          return GUILocalizeStrings.Get(136);
        default:
          return "";
      }
    }

    protected override void OnPageLoad()
    {
      if (_yahooSettings == null)
      {
        _yahooSettings = YahooSettings.getInstance();
      }
      //Log.Write("Image filename = '{0}'", imgCountry.FileName);
      if (String.IsNullOrEmpty(imgCountry.FileName))
      {
        //Log.Write("Updating country image");
        YahooUtil loUtil = YahooUtil.getInstance();
        string lsCountryId = loUtil.getYahooSite(_yahooSettings._defaultCountryName)._yahooSiteCountryId;
        //Log.Write("country image -country id = {0}", lsCountryId);
        imgCountry.SetFileName(GUIGraphicsContext.Skin + @"\media\" + lsCountryId + ".png");
      }

      if (CURRENT_STATE == (int)State.HOME)
      {
        EnableHomeButtons();
        _yahooGenre = new YahooGenres();
        this.LooseFocus();
        btnTop.Focus = true;
      }
      else
      {
        if (CURRENT_STATE == (int)State.TOP)
          refreshStage2Screen(BuildStateLabel(State.TOP));
        else if (CURRENT_STATE == (int)State.NEW)
          refreshStage2Screen(BuildStateLabel(State.NEW));
        else if (CURRENT_STATE == (int)State.FAVORITE)
          refreshStage2Screen(BuildStateLabel(State.FAVORITE));
        else if (CURRENT_STATE == (int)State.SEARCH)
          refreshStage2Screen(BuildStateLabel(State.SEARCH));
        else if (CURRENT_STATE == (int)State.PLAYLIST)
        {
          labelState.Label = BuildStateLabel(State.PLAYLIST);
          DisableAllButtons();
          enablePlaylistButtons();
          _playListSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
          refreshScreenVideoList();
        }
        else if (CURRENT_STATE == (int)State.GENRE)
          refreshStage2Screen(BuildStateLabel(State.GENRE));
        this.LooseFocus();
        listSongs.Focus = true;
      }

    }
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      //Log.Write("GUIMusicVideo: Clicked control = {0}", control);
      if (control == listSongs)
      {
        _playListSelectedIndex = listSongs.SelectedListItemIndex;
        if (CURRENT_STATE == (int)State.PLAYLIST)
        {
          MusicVideoPlaylist.getInstance().Play(_playListSelectedIndex);
        }
        else
        {
          playVideo(getSelectedVideo());
        }

      }
      else if (control == btnTop)
      {
        onClickTopVideos();
      }
      else if (control == btnNew)
      {
        onClickNewVideos();
      }
      else if (control == btnSearch)
      {
        SearchVideos(true, String.Empty);
      }
      else if (control == btnFavorites)
      {
        onClickFavorites();
      }
      else if (control == btnGenre)
      {
        onClickGenre();
      }
      else if (control == btnBack)
      {
        EnableHomeButtons();
        //clear the list
        listSongs.Clear();
        //clear the state label
        labelState.Label = "";
        CURRENT_STATE = (int)State.HOME;
        this.LooseFocus();
        btnTop.Focus = true;
      }
      else if (control == btnPlayAll)
      {
        MusicVideoPlaylist.getInstance().AddAllToPlayList(getStateVideoList());
        CURRENT_STATE = (int)State.PLAYLIST;
        MusicVideoPlaylist.getInstance().Play();
        listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
        _playListSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      else if (control == btnPlayList)
      {
        onClickPlaylist();
      }
      else if (control == btnPlayListPlay)
      {
        MusicVideoPlaylist.getInstance().Play();
        listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
        _playListSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      else if (control == btnPlayListStop)
      {
        MusicVideoPlaylist.getInstance().Stop();
      }
      else if (control == btnPlayListNext)
      {
        MusicVideoPlaylist.getInstance().PlayNext();
        listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
        _playListSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      else if (control == btnPlayListPrevious)
      {
        MusicVideoPlaylist.getInstance().PlayPrevious();
        listSongs.SelectedListItemIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
        _playListSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      else if (control == btnPlayListBack)
      {
        EnableHomeButtons();
        //clear the list
        listSongs.Clear();
        //clear the state label
        labelState.Label = "";
        CURRENT_STATE = (int)State.HOME;
        this.LooseFocus();
        btnTop.Focus = true;
      }
      else if (control == btnPlayListRepeat)
      {
        MusicVideoPlaylist loPlayList = MusicVideoPlaylist.getInstance();

        loPlayList.repeat(!loPlayList.getRepeatState());
        //btnPlayListRepeat.
      }
      else if (control == btnPlayListShuffle)
      {
        MusicVideoPlaylist loPlayList = MusicVideoPlaylist.getInstance();
        loPlayList.shuffle();
        DisplayVideoList(loPlayList.getPlayListVideos());
        listSongs.SelectedListItemIndex = loPlayList.getPlayListIndex();
        _playListSelectedIndex = MusicVideoPlaylist.getInstance().getPlayListIndex();
      }
      else if (control == btnPlayListClear)
      {
        MusicVideoPlaylist.getInstance().Clear();
        listSongs.Clear();
      }
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
          dlgSel.Add(GUILocalizeStrings.Get(926)); // Add to playList
          if ((int)State.FAVORITE == CURRENT_STATE)
            dlgSel.Add(GUILocalizeStrings.Get(933)); // Remove from favorites
          else
            dlgSel.Add(GUILocalizeStrings.Get(930)); // Add to favorites
          dlgSel.Add(GUILocalizeStrings.Get(208)); // Play
          dlgSel.Add(GUILocalizeStrings.Get(30007)); // Search other videos by this artist
          dlgSel.SetHeading(GUILocalizeStrings.Get(924)); // Menu 
          dlgSel.DoModal(GetID);
          int liSelectedIdx = dlgSel.SelectedId;
          //Log.Write("you selected action :{0}", liSelectedIdx);
          switch (liSelectedIdx)
          {
            case 1: MusicVideoPlaylist.getInstance().AddToPlayList(loVideo); break;
            case 2:
              if (CURRENT_STATE == (int)State.FAVORITE)
              {
                _yahooFavoriteManager.removeFavorite(loVideo);
                DisplayVideoList(_yahooFavoriteManager.getFavoriteVideos());
              }
              else
              {
                //prompt user for favorite list to add to
                string lsSelectedFav = promptForFavoriteList();
                //Log.Write("adding to favorites.");
                if (_yahooFavoriteManager == null)
                {
                  _yahooFavoriteManager = new YahooFavorites();
                }
                _yahooFavoriteManager.setSelectedFavorite(lsSelectedFav);
                _yahooFavoriteManager.addFavorite(loVideo);
              }
              break;
            case 3: playVideo(loVideo); break;
            case 4: SearchVideos(false, loVideo._yahooVideoArtistName); break;
          }
        }
      }
    }
    #endregion

    #region userdefined methods
    private void onClickFavorites()
    {
      if (_yahooFavoriteManager == null)
      {
        _yahooFavoriteManager = new YahooFavorites();
      }

      string lsSelectedFav = promptForFavoriteList();
      if (String.IsNullOrEmpty(lsSelectedFav))
      {
        return;
      }
      DisableAllButtons();
      btnPlayAll.Visible = true;
      btnBack.Visible = true;
      btnPlayList.Visible = true;
      btnNextPage.Visible = true;
      btnPreviousPage.Visible = true;

      listSongs.NavigateLeft = btnBack.GetID;
      listSongs.NavigateRight = btnBack.GetID;

      _playListSelectedIndex = 0;
      btnNextPage.Visible = false;
      btnPreviousPage.Visible = false;
      btnPlayList.NavigateUp = btnPlayAll.GetID;
      btnPlayList.NavigateDown = btnBack.GetID;
      CURRENT_STATE = (int)State.FAVORITE;

      if (lsSelectedFav != null || lsSelectedFav.Length > 0)
      {
        _yahooFavoriteManager.setSelectedFavorite(lsSelectedFav);
      }
      DisplayVideoList(_yahooFavoriteManager.getFavoriteVideos());
      labelState.Label = BuildStateLabel(State.FAVORITE);
      if (listSongs.Count == 0)
      {
        this.LooseFocus();
        btnBack.Focus = true;
      }
    }

    private void onClickPlaylist()
    {
      enablePlaylistButtons();
      CURRENT_STATE = (int)State.PLAYLIST;
      _playListSelectedIndex = 0;
      DisplayVideoList(MusicVideoPlaylist.getInstance().getPlayListVideos());
      labelState.Label = BuildStateLabel(State.PLAYLIST);
      if (listSongs.Count == 0)
      {
        this.LooseFocus();
        btnPlayListBack.Focus = true;
      }
    }

    private string promptForGenre()
    {
      string lsSelectedGenre = "";
      ArrayList loGenreNames = _yahooGenre._yahooSortedGenreList;

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
        lsSelectedGenre = dlgSel.SelectedLabelText;
      }
      return lsSelectedGenre;
    }

    private string promptForFavoriteList()
    {
      string lsSelectedFav = "";
      if (_yahooFavoriteManager == null)
      {
        _yahooFavoriteManager = new YahooFavorites();
      }
      ArrayList loFavNames = _yahooFavoriteManager.getFavoriteNames();
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
        String[] loCountryArray = new String[_yahooSettings._yahooSiteTable.Keys.Count];
        _yahooSettings._yahooSiteTable.Keys.CopyTo(loCountryArray, 0);
        Array.Sort(loCountryArray);

        foreach (string country in loCountryArray)
        {
          dlgSel.Add(country);
        }
        dlgSel.SetHeading(GUILocalizeStrings.Get(924)); // Menu
        dlgSel.DoModal(GetID);
        if (dlgSel.SelectedLabel == -1)
        {
          return;
        }
        _yahooSettings._defaultCountryName = dlgSel.SelectedLabelText;
        _yahooTopVideos = new YahooTopVideos(_yahooSettings._defaultCountryName);
        RefreshPage();
      }
      //return lsSelectedGenre;
    }
    private void onClickNewVideos()
    {
      _playListSelectedIndex = 0;
      CURRENT_STATE = (int)State.NEW;

      if (_yahooNewVideos == null)
      {
        _yahooNewVideos = new YahooNewVideos();
      }
      _yahooNewVideos.loadNewVideos(_yahooSettings._defaultCountryName);

      if (_yahooNewVideos.hasNext())
        btnNextPage.Disabled = false;
      else
        btnNextPage.Disabled = true;

      btnPreviousPage.Disabled = true;
      
      refreshStage2Screen(BuildStateLabel(State.NEW));
    }

    private void onClickGenre()
    {
      _playListSelectedIndex = 0;
      _selectedGenre = promptForGenre();
      if (String.IsNullOrEmpty(_selectedGenre))
      {
        return;
      }
      CURRENT_STATE = (int)State.GENRE;

      if (_yahooGenre == null)
        _yahooGenre = new YahooGenres();

      _yahooGenre.loadFirstGenreVideos(_selectedGenre);

      if (_yahooGenre.hasNext())
        btnNextPage.Disabled = false;
      else
        btnNextPage.Disabled = true;

      btnPreviousPage.Disabled = true;
      refreshStage2Screen(BuildStateLabel(State.GENRE));
    }

    private void SearchVideos(bool fbClicked, String fsSearchTxt)
    {
      DisableAllButtons();
      btnPlayAll.Visible = true;
      btnBack.Visible = true;
      btnPlayList.Visible = true;
      btnNextPage.Visible = true;
      btnPreviousPage.Visible = true;
      listSongs.NavigateLeft = btnBack.GetID;
      listSongs.NavigateRight = btnBack.GetID;

      _playListSelectedIndex = 0;
      CURRENT_STATE = (int)State.SEARCH;
      if (_yahooSearch == null)
        _yahooSearch = new YahooSearch(_yahooSettings._defaultCountryName);

      //clear the list
      listSongs.Clear();
      if (fbClicked)
      {
        _yahooSearch.searchVideos(getUserTypedText());
      }
      else
      {
        _yahooSearch.searchVideos(fsSearchTxt);
      }
      DisplayVideoList(_yahooSearch._yahooLastSearchResult);

      labelState.Label = BuildStateLabel(State.SEARCH);
      btnNextPage.Disabled = !_yahooSearch.hasNext();
      btnPreviousPage.Disabled = true;
    }

    private void onClickTopVideos()
    {
      CURRENT_STATE = (int)State.TOP;
      _playListSelectedIndex = 0;
      if (_yahooTopVideos == null)
      {
        _yahooTopVideos = new YahooTopVideos(_yahooSettings._defaultCountryName);
      }
      _yahooTopVideos.loadFirstPage();
      if (_yahooTopVideos.hasMorePages())
      {
        btnNextPage.Disabled = false;
      }
      else
      {
        btnNextPage.Disabled = true;
      }
      btnPreviousPage.Disabled = true;
      refreshStage2Screen(BuildStateLabel(State.TOP));
    }

    private void OnClickNextPage()
    {
      _playListSelectedIndex = 0;
      bool lbNext = false;
      bool lbPrevious = false;
      switch (CURRENT_STATE)
      {
        case (int)State.NEW:
          _yahooNewVideos.loadNextVideos(_yahooSettings._defaultCountryName);
          lbNext = _yahooNewVideos.hasNext();
          lbPrevious = _yahooNewVideos.hasPrevious();
          DisplayVideoList(_yahooNewVideos._yahooNewVideoList);
          labelState.Label = BuildStateLabel(State.NEW);
          break;
        case (int)State.TOP:
          _yahooTopVideos.loadNextPage();
          lbNext = _yahooTopVideos.hasMorePages();
          lbPrevious = _yahooTopVideos.hasPreviousPage();
          DisplayVideoList(_yahooTopVideos.getLastLoadedList());
          //labelState.Label = String.Format("Top Yahoo Videos {0} - {1} ", _yahooTopVideos.getFirstVideoRank(), _yahooTopVideos.getLastVideoRank());
          labelState.Label = BuildStateLabel(State.TOP);
          break;
        case (int)State.SEARCH:
          _yahooSearch.loadNextVideos();
          lbNext = _yahooSearch.hasNext();
          lbPrevious = _yahooSearch.hasPrevious();
          DisplayVideoList(_yahooSearch._yahooLastSearchResult);
          labelState.Label = BuildStateLabel(State.SEARCH);
          break;
        case (int)State.GENRE:
          _yahooGenre.loadNextVideos();
          lbNext = _yahooGenre.hasNext();
          lbPrevious = _yahooGenre.hasPrevious();
          DisplayVideoList(_yahooGenre._yahooGenreVideoList);
          labelState.Label = BuildStateLabel(State.GENRE);//        
          break;
      }
      //Log.Write("The video page has next video ={0}", lbNext);
      //Log.Write("The video page has previous video ={0}", lbPrevious);

      btnNextPage.Disabled = !lbNext;
      btnPreviousPage.Disabled = !lbPrevious;
    }

    private void OnClickPreviousPage()
    {
      _playListSelectedIndex = 0;
      bool lbNext = false;
      bool lbPrevious = false;
      switch (CURRENT_STATE)
      {
        case (int)State.NEW:
          _yahooNewVideos.loadPreviousVideos(_yahooSettings._defaultCountryName);
          lbNext = _yahooNewVideos.hasNext();
          lbPrevious = _yahooNewVideos.hasPrevious();
          DisplayVideoList(_yahooNewVideos._yahooNewVideoList);
          labelState.Label = BuildStateLabel(State.NEW);
          break;
        case (int)State.TOP:
          _yahooTopVideos.loadPreviousPage();
          lbNext = _yahooTopVideos.hasMorePages();
          lbPrevious = _yahooTopVideos.hasPreviousPage();
          DisplayVideoList(_yahooTopVideos.getLastLoadedList());
          labelState.Label = labelState.Label = BuildStateLabel(State.TOP);
          break;
        case (int)State.SEARCH:
          _yahooSearch.loadPreviousVideos();
          lbNext = _yahooSearch.hasNext();
          lbPrevious = _yahooSearch.hasPrevious();
          DisplayVideoList(_yahooSearch._yahooLastSearchResult);
          labelState.Label = labelState.Label = BuildStateLabel(State.SEARCH);
          break;
        case (int)State.GENRE:
          _yahooGenre.loadPreviousVideos();
          lbNext = _yahooGenre.hasNext();
          lbPrevious = _yahooGenre.hasPrevious();
          DisplayVideoList(_yahooGenre._yahooGenreVideoList);
          labelState.Label = labelState.Label = BuildStateLabel(State.GENRE);
          break;
      }
      btnNextPage.Disabled = !lbNext;
      btnPreviousPage.Disabled = !lbPrevious;
    }

    private List<YahooVideo> getStateVideoList()
    {
      List<YahooVideo> loCurrentDisplayVideoList = null;
      switch (CURRENT_STATE)
      {
        case (int)State.TOP:
          loCurrentDisplayVideoList = _yahooTopVideos.getLastLoadedList();
          break;
        case (int)State.NEW:
          loCurrentDisplayVideoList = _yahooNewVideos._yahooNewVideoList;
          break;
        case (int)State.SEARCH:
          loCurrentDisplayVideoList = _yahooSearch._yahooLastSearchResult;
          break;
        case (int)State.FAVORITE:
          loCurrentDisplayVideoList = _yahooFavoriteManager.getFavoriteVideos();
          break;
        case (int)State.PLAYLIST:
          loCurrentDisplayVideoList = MusicVideoPlaylist.getInstance().getPlayListVideos();
          break;
        case (int)State.GENRE:
          loCurrentDisplayVideoList = _yahooGenre._yahooGenreVideoList;
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
      //System.GC.Collect(); // collect some garbage
      if (keyBoard.Text == "" || keyBoard.Text == null)
        return "";
      KB_Search_Str = keyBoard.Text;
      return KB_Search_Str;
    }

    private void refreshScreenVideoList()
    {
      //Log.Write("Refreshing video list on screen");
      List<YahooVideo> loCurrentDisplayVideoList = getStateVideoList();
      DisplayVideoList(loCurrentDisplayVideoList);
      listSongs.SelectedListItemIndex = _playListSelectedIndex;
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
        item.DVDLabel = loYahooVideo._yahooVideoSongId;
        if (loYahooVideo._yahooVideoArtistName == null || loYahooVideo._yahooVideoArtistName.Equals(""))
        {
          item.Label3 = loYahooVideo._yahooVideoSongName;
        }
        else
        {
          item.Label3 = loYahooVideo._yahooVideoArtistName + " - " + loYahooVideo._yahooVideoSongName;
        }
        item.IsFolder = false;
        item.MusicTag = true;
        listSongs.Add(item);
      }
      this.LooseFocus();
      listSongs.Focus = true;
      if (listSongs.Count > 0)
      {
        //labelSelected.Label = "Press Menu or F9 for more options.";
      }
      else
        labelSelected.Label = "";
    }
    void playVideo(YahooVideo video)
    {
      string lsVideoLink = null;
      YahooSite loSite;
      YahooUtil loUtil = YahooUtil.getInstance();
      loSite = loUtil.getYahooSiteById(video._yahooVideoCountryId);
      lsVideoLink = loUtil.getVideoMMSUrl(video, _yahooSettings._defaultBitRate);
      lsVideoLink = lsVideoLink.Substring(0, lsVideoLink.Length - 2) + "&txe=.wmv";
      if (g_Player.PlayVideoStream(lsVideoLink))
      {
        //Log.Write("Playing Video:{0}", video._yahooVideoSongName);
        GUIGraphicsContext.IsFullScreenVideo = true;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
      else
      {
        Log.Write("GUIMusicVideo: Unable to play {0}", lsVideoLink);
      }
    }

    private void EnableAllButtons()
    {
      //btnbitrate.Visible = true;
      btnTop.Visible = true;
      btnSearch.Visible = true;
      btnFavorites.Visible = true;
      //btncountry.Visible = true;
      //btnGenre.Visible = true;
      btnNew.Visible = true;
      btnBack.Visible = true;
      btnPlayAll.Visible = true;
      btnPlayList.Visible = true;
      //btnNewFavorite.Visible = true;
    }

    private void EnableHomeButtons()
    {
      DisableAllButtons();
      //btnbitrate.Visible = true;
      btnTop.Visible = true;
      btnSearch.Visible = true;
      btnFavorites.Visible = true;
      //btncountry.Visible = true;
      btnGenre.Visible = true;
      //btnGenreSelect.Visible = true;
      btnNew.Visible = true;
      //btnBack.Visible = true;
      //btnPlayAll.Visible = true;
      btnPlayList.Visible = true;
      //btnNewFavorite.Visible = true;
      //btnPlayListNext.Disabled = true;
      //btnPlayListPrevious.Disabled = true;
      btnCountry.Visible = true;

      listSongs.NavigateLeft = btnTop.GetID;
      listSongs.NavigateRight = btnTop.GetID;

      btnPlayList.NavigateDown = btnSearch.GetID;
      btnPlayList.NavigateUp = btnNew.GetID;
    }

    private void DisableAllButtons()
    {
      //btnbitrate.Visible = false;
      btnTop.Visible = false;
      btnSearch.Visible = false;
      btnFavorites.Visible = false;
      //btncountry.Visible = false;
      btnGenre.Visible = false;
      //btnGenreSelect.Visible = false;
      btnNew.Visible = false;
      btnBack.Visible = false;
      btnPlayAll.Visible = false;
      btnPlayListBack.Visible = false;
      btnPlayList.Visible = false;
      btnPlayListPlay.Visible = false;
      btnPlayListNext.Visible = false;
      btnPlayListPrevious.Visible = false;
      btnPlayListStop.Visible = false;
      btnPlayListShuffle.Visible = false;
      btnPlayListRepeat.Visible = false;
      btnPlayListClear.Visible = false;
      btnNextPage.Visible = false;
      btnPreviousPage.Visible = false;
      btnCountry.Visible = false;
      //btnNewFavorite.Visible = false;
    }

    private void enablePlaylistButtons()
    {
      DisableAllButtons();
      btnPlayListBack.Visible = true;
      btnPlayListPlay.Visible = true;
      btnPlayListNext.Visible = true;
      btnPlayListPrevious.Visible = true;
      btnPlayListStop.Visible = true;
      btnPlayListShuffle.Visible = true;
      btnPlayListRepeat.Visible = true;
      btnPlayListRepeat.Selected = MusicVideoPlaylist.getInstance().getRepeatState();
      btnPlayListClear.Visible = true;
      listSongs.NavigateLeft = btnPlayListBack.GetID;
      listSongs.NavigateRight = btnPlayListBack.GetID;
    }

    public void refreshStage2Screen(String title)
    {
      DisableAllButtons();
      btnBack.Visible = true;
      btnPlayAll.Visible = true;
      btnPlayList.Visible = true;
      btnPlayList.NavigateDown = btnNextPage.GetID;
      btnPlayList.NavigateUp = btnPlayAll.GetID;
      btnNextPage.Visible = true;
      btnPreviousPage.Visible = true;
      listSongs.NavigateLeft = btnBack.GetID;
      labelState.Label = title;
      refreshScreenVideoList();
    }

    void RefreshPage()
    {
      this.Restore();
      this.Init();
      this.Render(0);
      this.OnPageLoad();
    }

    #endregion
  }
}
