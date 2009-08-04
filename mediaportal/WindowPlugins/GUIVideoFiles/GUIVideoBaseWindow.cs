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
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for GUIVideoBaseWindow.
  /// </summary>
  public abstract class GUIVideoBaseWindow : GUIInternalWindow
  {
    #region enums

    public enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      FilmStrip = 4,
      PlayList = 5
    }

    #endregion

    #region Base variables

    protected View currentView = View.List;
    protected View currentViewRoot = View.List;
    protected VideoSort.SortMethod currentSortMethod = VideoSort.SortMethod.Name;
    protected VideoSort.SortMethod currentSortMethodRoot = VideoSort.SortMethod.Name;
    protected bool m_bSortAscending;
    protected bool m_bSortAscendingRoot;
    protected VideoViewHandler handler;
    protected string m_strPlayListPath = string.Empty;
    protected string _currentFolder = string.Empty;
    protected string _lastFolder = string.Empty;
    protected bool m_bPlaylistsViewMode = false;
    protected PlayListPlayer playlistPlayer;

    #endregion

    #region SkinControls

    [SkinControl(50)] protected GUIFacadeControl facadeView = null;
    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnViews = null;
    [SkinControl(6)] protected GUIButtonControl btnPlayDVD = null;
    [SkinControl(8)] protected GUIButtonControl btnTrailers = null;
    [SkinControl(9)] protected GUIButtonControl btnSavedPlaylists = null;

    #endregion

    #region Constructor / Destructor

    public GUIVideoBaseWindow()
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;

      if (handler == null)
      {
        handler = new VideoViewHandler();
      }

      GUIWindowManager.OnNewAction += new OnActionHandler(OnNewAction);
    }

    #endregion

    #region Serialisation

    protected virtual void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        int defaultView = (int) View.List;
        int defaultSort = (int) VideoSort.SortMethod.Name;
        bool defaultAscending = true;
        if ((handler != null) && (handler.View != null) && (handler.View.Filters != null) &&
            (handler.View.Filters.Count > 0))
        {
          FilterDefinition def = (FilterDefinition) handler.View.Filters[0];
          defaultView = (int) GetViewNumber(def.DefaultView);
          defaultSort = (int) GetSortMethod(def.DefaultSort);
          defaultAscending = def.SortAscending;
        }
        currentView = (View) xmlreader.GetValueAsInt(SerializeName, "view", defaultView);
        currentViewRoot = (View) xmlreader.GetValueAsInt(SerializeName, "viewroot", defaultView);

        currentSortMethod = (VideoSort.SortMethod) xmlreader.GetValueAsInt(SerializeName, "sortmethod", defaultSort);
        currentSortMethodRoot =
          (VideoSort.SortMethod) xmlreader.GetValueAsInt(SerializeName, "sortmethodroot", defaultSort);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", defaultAscending);
        m_bSortAscendingRoot = xmlreader.GetValueAsBool(SerializeName, "sortascroot", defaultAscending);

        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        m_strPlayListPath = xmlreader.GetValueAsString("movies", "playlists", playListFolder);
        m_strPlayListPath = Util.Utils.RemoveTrailingSlash(m_strPlayListPath);
      }

      SwitchView();
    }

    protected VideoSort.SortMethod GetSortMethod(string s)
    {
      switch (s.Trim().ToLower())
      {
        case "date":
          return VideoSort.SortMethod.Date;
        case "label":
          return VideoSort.SortMethod.Label;
        case "name":
          return VideoSort.SortMethod.Name;
        case "rating":
          return VideoSort.SortMethod.Rating;
        case "size":
          return VideoSort.SortMethod.Size;
        case "year":
          return VideoSort.SortMethod.Year;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("GUIVideoBaseWindow::GetSortMethod: Unknown String - " + s);
      }
      return VideoSort.SortMethod.Name;
    }

    protected View GetViewNumber(string s)
    {
      switch (s.Trim().ToLower())
      {
        case "list":
          return View.List;
        case "icons":
          return View.Icons;
        case "big icons":
          return View.LargeIcons;
        case "largeicons":
          return View.LargeIcons;
        case "filmstrip":
          return View.FilmStrip;
        case "playlist":
          return View.PlayList;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("GUIVideoBaseWindow::GetViewNumber: Unknown String - " + s);
      }
      return View.List;
    }

    protected virtual void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "view", (int) currentView);
        xmlwriter.SetValue(SerializeName, "viewroot", (int) currentViewRoot);
        xmlwriter.SetValue(SerializeName, "sortmethod", (int) currentSortMethod);
        xmlwriter.SetValue(SerializeName, "sortmethodroot", (int) currentSortMethodRoot);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
        xmlwriter.SetValueAsBool(SerializeName, "sortascroot", m_bSortAscendingRoot);
      }
    }

    #endregion

    protected virtual bool AllowView(View view)
    {
      if (view == View.PlayList)
      {
        return false;
      }
      return true;
    }

    protected virtual bool AllowSortMethod(VideoSort.SortMethod method)
    {
      return true;
    }

    protected virtual View CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }

    protected virtual VideoSort.SortMethod CurrentSortMethod
    {
      get { return currentSortMethod; }
      set { currentSortMethod = value; }
    }

    protected virtual bool CurrentSortAsc
    {
      get { return m_bSortAscending; }
      set { m_bSortAscending = value; }
    }

    protected virtual string SerializeName
    {
      get { return string.Empty; }
    }

    protected bool ViewByIcon
    {
      get
      {
        if (CurrentView != View.List)
        {
          return true;
        }
        return false;
      }
    }

    protected bool ViewByLargeIcon
    {
      get
      {
        if (CurrentView == View.LargeIcons)
        {
          return true;
        }
        return false;
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_VIDEO_PLAYLIST);
        return;
      }
      base.OnAction(action);
    }

    // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when 
    // the player is not playing)...
    private void OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY
           || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
          && GUIWindowManager.ActiveWindow == GetID)
      {
        GUIListItem item = facadeView.SelectedListItem;

        if (item == null || item.Label == "..")
        {
          return;
        }

        OnClick(-1);
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (CurrentView)
          {
            case View.List:
              CurrentView = View.PlayList;
              if (!AllowView(CurrentView) || facadeView.PlayListView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.Playlist;
              }
              break;

            case View.PlayList:
              CurrentView = View.Icons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              }
              break;

            case View.Icons:
              CurrentView = View.LargeIcons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              }
              break;

            case View.LargeIcons:
              CurrentView = View.FilmStrip;
              if (!AllowView(CurrentView) || facadeView.FilmstripView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
              }
              break;

            case View.FilmStrip:
              CurrentView = View.List;
              if (!AllowView(CurrentView) || facadeView.ListView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.List;
              }
              break;
          }
        } while (shouldContinue);
        SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      } //if (control == btnViewAs)

      if (control == btnSortBy)
      {
        OnShowSortOptions();
      }

      if (control == btnViews)
      {
        OnShowViews();
      }

      if (control == btnSavedPlaylists)
      {
        OnShowSavedPlaylists(m_strPlayListPath);
      }

      if (control == btnPlayDVD)
      {
        ISelectDVDHandler selectDVDHandler;
        if (GlobalServiceProvider.IsRegistered<ISelectDVDHandler>())
        {
          selectDVDHandler = GlobalServiceProvider.Get<ISelectDVDHandler>();
        }
        else
        {
          selectDVDHandler = new SelectDVDHandler();
          GlobalServiceProvider.Add<ISelectDVDHandler>(selectDVDHandler);
        }
        string dvdToPlay = selectDVDHandler.ShowSelectDVDDialog(GetID);
        if (dvdToPlay != null)
        {
          selectDVDHandler.OnPlayDVD(dvdToPlay, GetID);
        }
        return;
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int) msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnInfo(iItem);
          facadeView.RefreshCoverArt();
        }
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          OnQueueItem(iItem);
        }
      }
    }

    protected void OnShowSavedPlaylists(string _directory)
    {
      VirtualDirectory _virtualDirectory = new VirtualDirectory();
      _virtualDirectory.AddExtension(".m3u");
      _virtualDirectory.AddExtension(".pls");
      _virtualDirectory.AddExtension(".b4s");
      _virtualDirectory.AddExtension(".wpl");

      List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(_directory);
      if (_directory == m_strPlayListPath)
      {
        itemlist.RemoveAt(0);
      }

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(983); // Saved Playlists

      foreach (GUIListItem item in itemlist)
      {
        Util.Utils.SetDefaultIcons(item);
        dlg.Add(item);
      }

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      GUIListItem selectItem = itemlist[dlg.SelectedLabel];
      if (selectItem.IsFolder)
      {
        OnShowSavedPlaylists(selectItem.Path);
        return;
      }

      GUIWaitCursor.Show();
      LoadPlayList(selectItem.Path);
      GUIWaitCursor.Hide();
    }

    protected void SelectCurrentItem()
    {
      int iItem = facadeView.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      }
      UpdateButtonStates();
    }

    protected virtual void UpdateButtonStates()
    {
      GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
      if (GetID == (int) Window.WINDOW_VIDEO_TITLE)
      {
        GUIPropertyManager.SetProperty("#currentmodule",
                                       String.Format("{0}/{1}", GUILocalizeStrings.Get(100006),
                                                     handler.LocalizedCurrentView));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());
      }

      GUIControl.HideControl(GetID, facadeView.GetID);

      int iControl = facadeView.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);


      string strLine = string.Empty;
      View view = CurrentView;
      switch (view)
      {
        case View.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
        case View.FilmStrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
        case View.PlayList:
          strLine = GUILocalizeStrings.Get(101);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);

      switch (CurrentSortMethod)
      {
        case VideoSort.SortMethod.Name:
          strLine = GUILocalizeStrings.Get(365);
          break;
        case VideoSort.SortMethod.Date:
          strLine = GUILocalizeStrings.Get(104);
          break;
        case VideoSort.SortMethod.Size:
          strLine = GUILocalizeStrings.Get(105);
          break;
        case VideoSort.SortMethod.Year:
          strLine = GUILocalizeStrings.Get(366);
          break;
        case VideoSort.SortMethod.Rating:
          strLine = GUILocalizeStrings.Get(367);
          break;
        case VideoSort.SortMethod.Label:
          strLine = GUILocalizeStrings.Get(430);
          break;
        case VideoSort.SortMethod.Unwatched:
          strLine = GUILocalizeStrings.Get(527);
          break;
      }

      if (btnSortBy != null)
      {
        btnSortBy.Label = strLine;
        btnSortBy.IsAscending = CurrentSortAsc;
      }
    }

    protected virtual void OnClick(int item)
    {
    }

    protected virtual void OnQueueItem(int item)
    {
    }

    protected override void OnPageLoad()
    {
      GUIVideoOverlay videoOverlay = (GUIVideoOverlay) GUIWindowManager.GetWindow((int) Window.WINDOW_VIDEO_OVERLAY);
      if ((videoOverlay != null) && (videoOverlay.Focused))
      {
        videoOverlay.Focused = false;
      }

      LoadSettings();

      if (btnSortBy != null)
      {
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      }

      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      // Save view
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("movies", "startWindow", VideoState.StartWindow.ToString());
        xmlwriter.SetValue("movies", "startview", VideoState.View);
      }

      m_bPlaylistsViewMode = false;

      base.OnPageDestroy(newWindowId);
    }

    #region Sort Members

    protected virtual void OnSort()
    {
      SetLabels();
      facadeView.Sort(new VideoSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
    }

    #endregion

    protected virtual void SetLabels()
    {
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

        if (movie != null && movie.ID > 0 && !item.IsFolder)
        {
          if (CurrentSortMethod == VideoSort.SortMethod.Name)
          {
            item.Label2 = Util.Utils.SecondsToHMString(movie.RunTime*60);
          }
          else if (CurrentSortMethod == VideoSort.SortMethod.Year)
          {
            item.Label2 = movie.Year.ToString();
          }
          else if (CurrentSortMethod == VideoSort.SortMethod.Rating)
          {
            item.Label2 = movie.Rating.ToString();
          }
          else if (CurrentSortMethod == VideoSort.SortMethod.Label)
          {
            item.Label2 = movie.DVDLabel.ToString();
          }
          else if (CurrentSortMethod == VideoSort.SortMethod.Size)
          {
            if (item.FileInfo != null)
            {
              item.Label2 = Util.Utils.GetSize(item.FileInfo.Length);
            }
            else
            {
              item.Label2 = Util.Utils.SecondsToHMString(movie.RunTime*60);
            }
          }
        }
        else
        {
          string strSize1 = string.Empty, strDate = string.Empty;
          if (item.FileInfo != null && !item.IsFolder)
          {
            strSize1 = Util.Utils.GetSize(item.FileInfo.Length);
          }
          if (item.FileInfo != null && !item.IsFolder)
          {
            strDate = item.FileInfo.ModificationTime.ToShortDateString() + " " +
                      item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }
          if (CurrentSortMethod == VideoSort.SortMethod.Name)
          {
            item.Label2 = strSize1;
          }
          else if (CurrentSortMethod == VideoSort.SortMethod.Date)
          {
            item.Label2 = strDate;
          }
          else
          {
            item.Label2 = strSize1;
          }
        }
      }
    }

    protected void SwitchView()
    {
      if (facadeView == null)
      {
        return;
      }
      switch (CurrentView)
      {
        case View.List:
          facadeView.View = GUIFacadeControl.ViewMode.List;
          break;
        case View.Icons:
          facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
          break;
        case View.LargeIcons:
          facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
          break;
        case View.FilmStrip:
          facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
          break;
        case View.PlayList:
          facadeView.View = GUIFacadeControl.ViewMode.Playlist;
          break;
      }
    }

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = strLine;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }

    protected void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(499); // Views menu

      dlg.AddLocalizedString(134); // Shares
      foreach (ViewDefinition view in handler.Views)
      {
        dlg.Add(view.LocalizedName);
      }

      // set the focus to currently used view
      if (this.GetID == (int) Window.WINDOW_VIDEOS)
      {
        dlg.SelectedLabel = 0;
      }
      else if (this.GetID == (int) Window.WINDOW_VIDEO_TITLE)
      {
        dlg.SelectedLabel = handler.CurrentViewIndex + 1;
      }

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 134: // Shares
          {
            int nNewWindow = (int) Window.WINDOW_VIDEOS;
            VideoState.StartWindow = nNewWindow;
            if (nNewWindow != GetID)
            {
              GUIVideoFiles.Reset();
              GUIWindowManager.ReplaceWindow(nNewWindow);
            }
          }
          break;

        default: // a db view
          {
            ViewDefinition selectedView = (ViewDefinition) handler.Views[dlg.SelectedLabel - 1];
            handler.CurrentView = selectedView.Name;
            VideoState.View = selectedView.Name;
            int nNewWindow = (int) Window.WINDOW_VIDEO_TITLE;
            if (GetID != nNewWindow)
            {
              VideoState.StartWindow = nNewWindow;
              if (nNewWindow != GetID)
              {
                GUIWindowManager.ReplaceWindow(nNewWindow);
              }
            }
            else
            {
              LoadDirectory(string.Empty);
            }
          }
          break;
      }
    }

    protected void OnShowSortOptions()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(365); // name
      dlg.AddLocalizedString(104); // date
      dlg.AddLocalizedString(105); // size
      dlg.AddLocalizedString(366); // year
      dlg.AddLocalizedString(367); // rating
      dlg.AddLocalizedString(430); // label
      dlg.AddLocalizedString(527); // unwatched

      // set the focus to currently used sort method
      dlg.SelectedLabel = (int) CurrentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 365:
          CurrentSortMethod = VideoSort.SortMethod.Name;
          break;
        case 104:
          CurrentSortMethod = VideoSort.SortMethod.Date;
          break;
        case 105:
          CurrentSortMethod = VideoSort.SortMethod.Size;
          break;
        case 366:
          CurrentSortMethod = VideoSort.SortMethod.Year;
          break;
        case 367:
          CurrentSortMethod = VideoSort.SortMethod.Rating;
          break;
        case 430:
          CurrentSortMethod = VideoSort.SortMethod.Label;
          break;
        case 527:
          CurrentSortMethod = VideoSort.SortMethod.Unwatched;
          break;
        default:
          CurrentSortMethod = VideoSort.SortMethod.Name;
          break;
      }

      OnSort();
      GUIControl.FocusControl(GetID, btnSortBy.GetID);
    }

    protected virtual void LoadDirectory(string path)
    {
    }

    protected void LoadPlayList(string strPlayList)
    {
      IPlayListIO loader = PlayListFactory.CreateIO(strPlayList);
      if (loader == null)
      {
        return;
      }
      PlayList playlist = new PlayList();

      if (!loader.Load(playlist, strPlayList))
      {
        TellUserSomethingWentWrong();
        return;
      }

      playlistPlayer.CurrentPlaylistName = Path.GetFileNameWithoutExtension(strPlayList);
      if (playlist.Count == 1)
      {
        Log.Info("GUIVideoFiles: play single playlist item - {0}", playlist[0].FileName);
        if (g_Player.Play(playlist[0].FileName))
        {
          if (Util.Utils.IsVideo(playlist[0].FileName))
          {
            g_Player.ShowFullScreenWindow();
          }
        }
        return;
      }

      // clear current playlist
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Clear();

      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayListItem playListItem = playlist[i];
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(playListItem);
      }

      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Count > 0)
      {
        // then get 1st song
        playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO);
        PlayListItem item = playlist[0];

        // and start playing it
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_VIDEO;
        playlistPlayer.Reset();
        playlistPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int) Window.WINDOW_VIDEO_PLAYLIST);
        }
      }
    }

    private void TellUserSomethingWentWrong()
    {
      GUIDialogOK dlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(6);
        dlgOK.SetLine(1, 477);
        dlgOK.SetLine(2, string.Empty);
        dlgOK.DoModal(GetID);
      }
    }

    private void OnInfoFile(GUIListItem item)
    {
    }

    private void OnInfoFolder(GUIListItem item)
    {
    }

    protected virtual void OnInfo(int iItem)
    {
    }

    protected virtual void AddItemToPlayList(GUIListItem pItem)
    {
      if (!pItem.IsFolder)
      {
        //TODO
        if (Util.Utils.IsVideo(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
        {
          PlayListItem playlistItem = new PlayListItem();
          playlistItem.Type = PlayListItem.PlayListItemType.Video;
          playlistItem.FileName = pItem.Path;
          playlistItem.Description = pItem.Label;
          playlistItem.Duration = pItem.Duration;
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_VIDEO).Add(playlistItem);
        }
      }
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      CurrentSortAsc = e.Order != SortOrder.Descending;

      OnSort();
      //UpdateButtonStates();
      GUIControl.FocusControl(GetID, ((GUIControl) sender).GetID);
    }
  }
}