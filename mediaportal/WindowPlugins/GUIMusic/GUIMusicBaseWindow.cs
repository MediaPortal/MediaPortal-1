/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using MediaPortal.MusicImport;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for GUIMusicBaseWindow.
  /// </summary>
  public class GUIMusicBaseWindow : GUIWindow
  {

    protected enum Level
    {
      Root,
      Sub
    }
    public enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      Albums = 3,
      FilmStrip = 4
    }


    protected View currentView = View.List;
    protected View currentViewRoot = View.List;
    protected MusicSort.SortMethod currentSortMethod = MusicSort.SortMethod.Name;
    protected MusicSort.SortMethod currentSortMethodRoot = MusicSort.SortMethod.Name;
    protected bool m_bSortAscending;
    protected bool m_bSortAscendingRoot;
    private bool m_bUseID3 = false;
    protected MusicViewHandler handler;
    protected MusicDatabase m_database;
    [SkinControlAttribute(50)]
    protected GUIFacadeControl facadeView = null;
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]
    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnViews = null;

    public GUIMusicBaseWindow()
    {
    }
    protected bool UseID3
    {
      get { return m_bUseID3; }
      set { m_bUseID3 = value; }
    }

    protected virtual bool AllowView(View view)
    {
      return true;
    }
    protected virtual bool AllowSortMethod(MusicSort.SortMethod method)
    {
      return true;
    }
    protected virtual View CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }

    protected virtual MusicSort.SortMethod CurrentSortMethod
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
      get
      {
        return String.Empty;
      }
    }
    #region Serialisation
    protected virtual void LoadSettings()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        currentView = (View)xmlreader.GetValueAsInt(SerializeName, "view", (int)View.List);
        currentViewRoot = (View)xmlreader.GetValueAsInt(SerializeName, "viewroot", (int)View.List);

        currentSortMethod = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethod", (int)MusicSort.SortMethod.Name);
        currentSortMethodRoot = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethodroot", (int)MusicSort.SortMethod.Name);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);
        m_bSortAscendingRoot = xmlreader.GetValueAsBool(SerializeName, "sortascroot", true);
        m_bUseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
      }

      SwitchView();
    }

    protected virtual void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue(SerializeName, "view", (int)currentView);
        xmlwriter.SetValue(SerializeName, "viewroot", (int)currentViewRoot);
        xmlwriter.SetValue(SerializeName, "sortmethod", (int)currentSortMethod);
        xmlwriter.SetValue(SerializeName, "sortmethodroot", (int)currentSortMethodRoot);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
        xmlwriter.SetValueAsBool(SerializeName, "sortascroot", m_bSortAscendingRoot);
      }
    }
    #endregion




    protected bool ViewByIcon
    {
      get
      {
        if (CurrentView != View.List) return true;
        return false;
      }
    }

    protected bool ViewByLargeIcon
    {
      get
      {
        if (CurrentView == View.LargeIcons) return true;
        return false;
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_TRACK)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeTrack(facadeView, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_DISC)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeDisc(facadeView, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_CANCEL_IMPORT)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.Cancel();
        return;
      }
      base.OnAction(action);
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
              CurrentView = View.Icons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              break;
            case View.Icons:
              CurrentView = View.LargeIcons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              break;
            case View.LargeIcons:
              CurrentView = View.Albums;
              if (!AllowView(CurrentView) || facadeView.AlbumListView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
              break;
            case View.Albums:
              CurrentView = View.FilmStrip;
              if (!AllowView(CurrentView) || facadeView.FilmstripView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
              break;
            case View.FilmStrip:
              CurrentView = View.List;
              if (!AllowView(CurrentView) || facadeView.ListView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.List;
              break;
          }
        } while (shouldContinue);
        SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      }//if (control == btnViewAs)

      if (control == btnSortBy)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (CurrentSortMethod)
          {
            case MusicSort.SortMethod.Name:
              CurrentSortMethod = MusicSort.SortMethod.Date;
              break;
            case MusicSort.SortMethod.Date:
              CurrentSortMethod = MusicSort.SortMethod.Size;
              break;
            case MusicSort.SortMethod.Size:
              CurrentSortMethod = MusicSort.SortMethod.Track;
              break;
            case MusicSort.SortMethod.Track:
              CurrentSortMethod = MusicSort.SortMethod.Duration;
              break;
            case MusicSort.SortMethod.Duration:
              CurrentSortMethod = MusicSort.SortMethod.Title;
              break;
            case MusicSort.SortMethod.Title:
              CurrentSortMethod = MusicSort.SortMethod.Album;
              break;
            case MusicSort.SortMethod.Album:
              CurrentSortMethod = MusicSort.SortMethod.Filename;
              break;
            case MusicSort.SortMethod.Filename:
              CurrentSortMethod = MusicSort.SortMethod.Rating;
              break;
            case MusicSort.SortMethod.Rating:
              CurrentSortMethod = MusicSort.SortMethod.Name;
              break;
          }
          if (!AllowSortMethod(CurrentSortMethod))
            shouldContinue = true;
        } while (shouldContinue);
        OnSort();
        GUIControl.FocusControl(GetID, control.GetID);
      }//if (control==btnSortBy)

      if (control == btnViews)
      {
        OnShowViews();
      }


      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnInfo(iItem);
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
      GUIPropertyManager.SetProperty("#view", handler.CurrentView);
      if (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_GENRE)
      {
        GUIPropertyManager.SetProperty("#currentmodule", String.Format("{0}/{1}", GUILocalizeStrings.Get(100005), handler.CurrentView));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      }

      facadeView.IsVisible = false;
      facadeView.IsVisible = true;
      GUIControl.FocusControl(GetID, facadeView.GetID);


      string strLine = String.Empty;
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
        case View.Albums:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.FilmStrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
      }
      btnViewAs.Label = strLine;

      switch (CurrentSortMethod)
      {
        case MusicSort.SortMethod.Name:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case MusicSort.SortMethod.Date:
          strLine = GUILocalizeStrings.Get(104);
          break;
        case MusicSort.SortMethod.Size:
          strLine = GUILocalizeStrings.Get(105);
          break;
        case MusicSort.SortMethod.Track:
          strLine = GUILocalizeStrings.Get(266);
          break;
        case MusicSort.SortMethod.Duration:
          strLine = GUILocalizeStrings.Get(267);
          break;
        case MusicSort.SortMethod.Title:
          strLine = GUILocalizeStrings.Get(268);
          break;
        case MusicSort.SortMethod.Artist:
          strLine = GUILocalizeStrings.Get(269);
          break;
        case MusicSort.SortMethod.Album:
          strLine = GUILocalizeStrings.Get(270);
          break;
        case MusicSort.SortMethod.Filename:
          strLine = GUILocalizeStrings.Get(363);
          break;
        case MusicSort.SortMethod.Rating:
          strLine = GUILocalizeStrings.Get(367);
          break;
      }

      if (btnSortBy != null)
        btnSortBy.Label = strLine;
    }

    protected virtual void OnClick(int item)
    {
    }
    protected virtual void OnQueueItem(int item)
    {
    }


    protected void OnSetRating(int itemNumber)
    {
      GUIListItem item = facadeView[itemNumber];
      if (item == null) return;
      MusicTag tag = item.MusicTag as MusicTag;
      GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_RATING);
      if (tag != null)
      {
        dialog.Rating = tag.Rating;
        dialog.SetTitle(String.Format("{0}-{1}", tag.Artist, tag.Title));
      }
      dialog.FileName = item.Path;
      dialog.DoModal(GetID);
      if (tag != null)
      {
        tag.Rating = dialog.Rating;
      }
      m_database.SetRating(item.Path, dialog.Rating);
      if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
      {
        while (itemNumber > 0)
        {
          itemNumber--;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }

      if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
      {
        while (itemNumber + 1 < facadeView.Count)
        {
          itemNumber++;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }
    }

    protected override void OnPageLoad()
    {
      if (m_database == null)
      {
        m_database = new MusicDatabase();
      }
      if (handler == null)
      {
        handler = new MusicViewHandler();
      }
      LoadSettings();

      if (btnSortBy != null)
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      // Save view
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("music", "startWindow", MusicState.StartWindow.ToString());
        xmlwriter.SetValue("music", "startview", MusicState.View);
      }
    }

    protected void LoadPlayList(string strPlayList)
    {
      PlayList playlist = PlayListFactory.Create(strPlayList);
      if (playlist == null) return;
      if (!playlist.Load(strPlayList))
      {
        GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK != null)
        {
          dlgOK.SetHeading(6);
          dlgOK.SetLine(1, 477);
          dlgOK.SetLine(2, String.Empty);
          dlgOK.DoModal(GetID);
        }
        return;
      }

      if (playlist.Count == 1)
      {
        Log.Write("GUIMusicYears:Play:{0}", playlist[0].FileName);
        g_Player.Play(playlist[0].FileName);
        return;
      }

      // clear current playlist
      PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();

      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayList.PlayListItem playListItem = playlist[i];
        PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playListItem);
      }


      // if we got a playlist
      if (PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // then get 1st song
        playlist = PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC);
        PlayList.PlayListItem item = playlist[0];

        // and start playing it
        PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Reset();
        PlayListPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        }
      }
    }


    protected virtual void OnSort()
    {
      SetLabels();
      facadeView.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
    }

    protected virtual void SetLabels()
    {
      MusicSort.SortMethod method = CurrentSortMethod;

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        MusicTag tag = (MusicTag)item.MusicTag;
        if (tag != null)
        {
          if (tag.Title.Length > 0)
          {
            if (tag.Artist.Length > 0)
            {
              if (tag.Track > 0)
                item.Label = String.Format("{0:00}. {1} - {2}", tag.Track, tag.Artist, tag.Title);
              else
                item.Label = String.Format("{0} - {1}", tag.Artist, tag.Title);
            }
            else
            {
              if (tag.Track > 0)
                item.Label = String.Format("{0:00}. {1} ", tag.Track, tag.Title);
              else
                item.Label = String.Format("{0}", tag.Title);
            }
            if (method == MusicSort.SortMethod.Album)
            {
              if (tag.Album.Length > 0 && tag.Title.Length > 0)
              {
                item.Label = String.Format("{0} - {1}", tag.Album, tag.Title);
              }
            }
            if (method == MusicSort.SortMethod.Rating)
            {
              item.Label2 = String.Format("{0}", tag.Rating);
            }
          }
        }


        if (method == MusicSort.SortMethod.Size || method == MusicSort.SortMethod.Filename)
        {
          if (item.IsFolder) item.Label2 = String.Empty;
          else
          {
            if (item.Size > 0)
            {
              item.Label2 = Utils.GetSize(item.Size);
            }
            if (method == MusicSort.SortMethod.Filename)
            {
              item.Label = Utils.GetFilename(item.Path);
            }
          }
        }
        else if (method == MusicSort.SortMethod.Date)
        {
          if (item.FileInfo != null)
          {
            item.Label2 = item.FileInfo.ModificationTime.ToShortDateString() + " " + item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }
        }
        else if (method != MusicSort.SortMethod.Rating)
        {
          if (tag != null)
          {
            int nDuration = tag.Duration;
            if (nDuration > 0)
            {
              item.Label2 = Utils.SecondsToHMSString(nDuration);
            }
          }
        }
      }
    }
    protected void SwitchView()
    {
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
        case View.Albums:
          facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
          break;
        case View.FilmStrip:
          facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
          break;
      }
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


    protected virtual void OnRetrieveCoverArt(GUIListItem item)
    {
      Utils.SetDefaultIcons(item);
      if (item.Label == "..") return;
      MusicTag tag = (MusicTag)item.MusicTag;
      string strThumb = GUIMusicFiles.GetCoverArt(item.IsFolder, item.Path, tag);
      if (strThumb != String.Empty)
      {
        item.ThumbnailImage = strThumb;
        item.IconImageBig = strThumb;
        item.IconImage = strThumb;
      }
    }

    protected void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(924); // menu
      dlg.Add(GUILocalizeStrings.Get(134));//songs
      foreach (ViewDefinition view in handler.Views)
      {
        dlg.Add(view.Name); //play
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      if (dlg.SelectedLabel == 0)
      {
        int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
        MusicState.StartWindow = nNewWindow;
        if (nNewWindow != GetID)
        {
          GUIWindowManager.ReplaceWindow(nNewWindow);
        }
      }
      else
      {
        ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel - 1];
        handler.CurrentView = selectedView.Name;
        MusicState.View = selectedView.Name;
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
    }
    protected virtual void LoadDirectory(string path)
    {
    }

    static public string GetArtistCoverArtName(string artist)
    {
      return Utils.GetCoverArtName(Thumbs.MusicArtists, artist);
    }

    void OnInfoFile(GUIListItem item)
    {
    }

    void OnInfoFolder(GUIListItem item)
    {

    }

    protected virtual void OnInfo(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];

      Song song = pItem.AlbumInfoTag as Song;
      if (song == null)
      {
        if (!pItem.IsFolder)
        {
          if (pItem.Path != String.Empty) OnInfoFile(pItem);
        }
        else
        {
          if (pItem.Path != String.Empty) OnInfoFolder(pItem);
        }
        facadeView.RefreshCoverArt();
        return;
      }
      else if (song.songId >= 0)
      {
        ShowAlbumInfo(false, song.Artist, song.Album, song.FileName, pItem.MusicTag as MusicTag, song.albumId);
      }
      else if (song.albumId >= 0)
      {

        ShowAlbumInfo(false, song.Artist, song.Album, song.FileName, pItem.MusicTag as MusicTag, song.albumId);
      }
      else if (song.artistId >= 0)
      {
        ShowArtistInfo(song.Artist, song.Album, song.artistId, song.albumId);
      }
      facadeView.RefreshCoverArt();

    }

    protected virtual void ShowArtistInfo(string artistName, string albumName, int artistId, int albumId)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

      // check cache
      bool bSaveDb = true;
      ArtistInfo artistinfo = new ArtistInfo();
      if (m_database.GetArtistInfo(artistName, ref artistinfo))
      {
        List<Song> songs = new List<Song>();
        MusicArtistInfo artist = new MusicArtistInfo();
        artist.Set(artistinfo);

        // ok, show Artist info
        GUIMusicArtistInfo pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
        if (null != pDlgArtistInfo)
        {
          pDlgArtistInfo.Artist = artist;
          pDlgArtistInfo.DoModal(GetID);

          if (pDlgArtistInfo.NeedsRefresh)
          {
            m_database.DeleteArtistInfo(artist.Artist);
            ShowArtistInfo(artistName, albumName, artistId, albumId);
            return;
          }
        }
        return;
      }


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

      // show dialog box indicating we're searching the artist
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(320);
        dlgProgress.SetLine(1, artistName);
        dlgProgress.SetLine(2, String.Empty);
        dlgProgress.SetPercentage(0);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
        dlgProgress.ShowProgressBar(true);
      }
      bool bDisplayErr = false;

      // find artist info
      AllmusicSiteScraper scraper = new AllmusicSiteScraper();
      if (scraper.FindInfo(AllmusicSiteScraper.SearchBy.Artists, artistName))
      {
        if (dlgProgress != null) dlgProgress.Close();
        // did we found at least 1 album?
        if (scraper.IsMultiple())
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          string[] artistsFound = scraper.GetItemsFound();
          //show dialog with all albums found
          string szText = GUILocalizeStrings.Get(181);
          GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
          if (null != pDlg)
          {
            pDlg.Reset();
            pDlg.SetHeading(szText);
            for (int i = 0; i < artistsFound.Length; ++i)
            {
              pDlg.Add(artistsFound[i]);
            }
            pDlg.DoModal(GetID);

            // and wait till user selects one
            iSelectedAlbum = pDlg.SelectedLabel;
            if (iSelectedAlbum < 0) return;
          }

          // ok, now show dialog we're downloading the artist info
          if (null != dlgProgress)
          {
            dlgProgress.SetHeading(320);
            dlgProgress.SetLine(1, artistName);
            dlgProgress.SetLine(2, String.Empty);
            dlgProgress.SetPercentage(40);
            dlgProgress.StartModal(GetID);
            dlgProgress.ShowProgressBar(true);
            dlgProgress.Progress();
          }

          // download the artist info
          if (scraper.FindInfoByIndex(iSelectedAlbum))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            MusicArtistInfo artistInfo = new MusicArtistInfo();
            if (artistInfo.Parse(scraper.GetHtmlContent()))
            {
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(80);
                dlgProgress.Progress();
              }
              // if the artist selected from allmusic.com does not match
              // the one from the file, override the one from the allmusic
              // with the one from the file so the info is correct in the
              // database...
              if (!artistInfo.Artist.Equals(artistName))
                artistInfo.Artist = artistName;

              if (bSaveDb)
              {
                m_database.AddArtistInfo(artistInfo.Get());
              }
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(100);
                dlgProgress.Progress();
                dlgProgress.Close();
                dlgProgress = null;
              }

              // ok, show Artist info
              GUIMusicArtistInfo pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
              if (null != pDlgArtistInfo)
              {
                pDlgArtistInfo.Artist = artistInfo;
                pDlgArtistInfo.DoModal(GetID);

                if (pDlgArtistInfo.NeedsRefresh)
                {
                  m_database.DeleteArtistInfo(artistInfo.Artist);
                  ShowArtistInfo(artistName, albumName, artistId, albumId);
                  return;
                }
              }
            }
          }

          if (null != dlgProgress)
            dlgProgress.Close();
        }
        else // single
        {
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(40);
            dlgProgress.Progress();
          }
          MusicArtistInfo artistInfo = new MusicArtistInfo();
          if (artistInfo.Parse(scraper.GetHtmlContent()))
          {

            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            // if the artist selected from allmusic.com does not match
            // the one from the file, override the one from the allmusic
            // with the one from the file so the info is correct in the
            // database...
            if (!artistInfo.Artist.Equals(artistName))
              artistInfo.Artist = artistName;

            if (bSaveDb)
            {
              // save to database
              m_database.AddArtistInfo(artistInfo.Get());
            }

            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
              dlgProgress = null;
            }
            // ok, show Artist info
            GUIMusicArtistInfo pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
            if (null != pDlgArtistInfo)
            {
              pDlgArtistInfo.Artist = artistInfo;
              pDlgArtistInfo.DoModal(GetID);

              if (pDlgArtistInfo.NeedsRefresh)
              {
                m_database.DeleteArtistInfo(artistInfo.Artist);
                ShowArtistInfo(artistName, albumName, artistId, albumId);
                return;
              }
            }
          }
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress)
          dlgProgress.Close();
        if (null != pDlgOK)
        {
          pDlgOK.SetHeading(702);
          pDlgOK.SetLine(1, 702);
          pDlgOK.SetLine(2, String.Empty);
          pDlgOK.DoModal(GetID);
        }
      }
    }

    protected void ShowAlbumInfo(bool isFolder, string artistName, string strAlbumName, string strPath, MusicTag tag, int albumId)
    {
      // check cache
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      AlbumInfo albuminfo = new AlbumInfo();
      if (m_database.GetAlbumInfo(albumId, ref albuminfo))
      {
        List<Song> songs = new List<Song>();
        MusicAlbumInfo album = new MusicAlbumInfo();
        album.Set(albuminfo);

        GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
        if (null != pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album = album;
          pDlgAlbumInfo.Tag = tag;

          pDlgAlbumInfo.DoModal(GetID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(strAlbumName);
            ShowAlbumInfo(isFolder, artistName, strAlbumName, strPath, tag, albumId);
          }
          return;
        }
      }

      // show dialog box indicating we're searching the album
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(185);
        dlgProgress.SetLine(1, strAlbumName);
        dlgProgress.SetLine(2, artistName);
        dlgProgress.SetLine(3, tag.Year.ToString());
        dlgProgress.SetPercentage(0);
        dlgProgress.StartModal(GetID);
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Progress();
      }
      bool bDisplayErr = false;

      // find album info
      MusicInfoScraper scraper = new MusicInfoScraper();
      if (scraper.FindAlbuminfo(strAlbumName))
      {
        if (dlgProgress != null)
        {
          dlgProgress.SetPercentage(30);
          dlgProgress.Progress();
          dlgProgress.Close();
        }
        // did we found at least 1 album?
        int iAlbumCount = scraper.Count;
        if (iAlbumCount >= 1)
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          if (iAlbumCount > 1)
          {
            //show dialog with all albums found
            string szText = GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              for (int i = 0; i < iAlbumCount; ++i)
              {
                MusicAlbumInfo info = scraper[i];
                pDlg.Add(info.Title2);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              iSelectedAlbum = pDlg.SelectedLabel;
              if (iSelectedAlbum < 0) return;
            }
          }

          // ok, now show dialog we're downloading the album info
          MusicAlbumInfo album = scraper[iSelectedAlbum];
          if (null != dlgProgress)
          {
            dlgProgress.SetHeading(185);
            dlgProgress.SetLine(1, album.Title2);
            dlgProgress.SetLine(2, album.Artist);
            dlgProgress.StartModal(GetID);
            dlgProgress.ShowProgressBar(true);
            dlgProgress.SetPercentage(40);
            dlgProgress.Progress();
          }

          // download the album info
          bool bLoaded = album.Loaded;
          if (!bLoaded)
            bLoaded = album.Load();
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(70);
            dlgProgress.Progress();
          }
          if (bLoaded)
          {
            // set album title from musicinfotag, not the one we got from allmusic.com
            album.Title = strAlbumName;
            // set path, needed to store album in database
            album.AlbumPath = strPath;
            albuminfo = new AlbumInfo();
            albuminfo.Album = album.Title;
            albuminfo.Artist = album.Artist;
            albuminfo.Genre = album.Genre;
            albuminfo.Tones = album.Tones;
            albuminfo.Styles = album.Styles;
            albuminfo.Review = album.Review;
            albuminfo.Image = album.ImageURL;
            albuminfo.Rating = album.Rating;
            albuminfo.Tracks = album.Tracks;
            try
            {
              albuminfo.Year = Int32.Parse(album.DateOfRelease);
            }
            catch (Exception)
            {
            }
            //albuminfo.Path   = album.AlbumPath;
            // save to database
            m_database.AddAlbumInfo(albuminfo);
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
            }

            // ok, show album info
            GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
            if (null != pDlgAlbumInfo)
            {
              pDlgAlbumInfo.Album = album;
              pDlgAlbumInfo.Tag = tag;

              pDlgAlbumInfo.DoModal(GetID);
              if (pDlgAlbumInfo.NeedsRefresh)
              {
                m_database.DeleteAlbumInfo(album.Title);
                ShowAlbumInfo(isFolder, artistName, strAlbumName, strPath, tag, albumId);
                return;
              }
              if (isFolder)
              {
                string thumb = GetAlbumThumbName(album.Artist, album.Title);
                if (System.IO.File.Exists(thumb))
                {
                  try
                  {
                    string folderjpg = String.Format(@"{0}\folder.jpg", Utils.RemoveTrailingSlash(strPath));
                    Utils.FileDelete(folderjpg);
                    System.IO.File.Copy(thumb, folderjpg);
                  }
                  catch (Exception)
                  {
                  }
                }
              }
            }
          }
          else
          {
            // failed 2 download album info
            bDisplayErr = true;
          }
        }
        else
        {
          // no albums found
          bDisplayErr = true;
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress)
          dlgProgress.Close();
        if (null != dlgOk)
        {
          dlgOk.SetHeading(187);
          dlgOk.SetLine(1, 187);
          dlgOk.SetLine(2, String.Empty);
          dlgOk.DoModal(GetID);
        }
      }
    }

    static public string GetAlbumThumbName(string ArtistName, string AlbumName)
    {
      if (ArtistName == String.Empty) return String.Empty;
      if (AlbumName == String.Empty) return String.Empty;
      string name = String.Format("{0}-{1}", ArtistName, AlbumName);
      return Utils.GetCoverArtName(Thumbs.MusicAlbum, name);
    }
    protected virtual void AddSongToFavorites(GUIListItem item)
    {
      Song song = item.AlbumInfoTag as Song;
      if (song == null) return;
      if (song.songId < 0) return;
      song.Favorite = true;
      m_database.SetFavorite(song);
    }

    void SortChanged(object sender, SortEventArgs args)
    {
      this.CurrentSortAsc = args.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();
      UpdateButtonStates();

      //			GUIControl.FocusControl(GetID, control.GetID);
    }
  }
}
