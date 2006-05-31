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
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Video.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for GUIVideoBaseWindow.
  /// </summary>
  public class GUIVideoBaseWindow : GUIWindow
  {

    public enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      FilmStrip = 3
    }

    protected View currentView = View.List;
    protected View currentViewRoot = View.List;
    protected VideoSort.SortMethod currentSortMethod = VideoSort.SortMethod.Name;
    protected VideoSort.SortMethod currentSortMethodRoot = VideoSort.SortMethod.Name;
    protected bool m_bSortAscending;
    protected bool m_bSortAscendingRoot;
    protected VideoViewHandler handler;

    [SkinControlAttribute(50)]
    protected GUIFacadeControl facadeView = null;
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]
    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnViews = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnPlayDVD = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnTrailers = null;
    

    protected PlayListPlayer playlistPlayer;

    public GUIVideoBaseWindow()
    {
      handler = new VideoViewHandler();
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    protected virtual bool AllowView(View view)
    {
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
      get
      {
        return String.Empty;
      }
    }
    #region Serialisation
    protected virtual void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        currentView = (View)xmlreader.GetValueAsInt(SerializeName, "view", (int)View.List);
        currentViewRoot = (View)xmlreader.GetValueAsInt(SerializeName, "viewroot", (int)View.List);

        currentSortMethod = (VideoSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethod", (int)VideoSort.SortMethod.Name);
        currentSortMethodRoot = (VideoSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethodroot", (int)VideoSort.SortMethod.Name);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);
        m_bSortAscendingRoot = xmlreader.GetValueAsBool(SerializeName, "sortascroot", true);

        
      }

      SwitchView();
    }

    protected virtual void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
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
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST);
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
            case VideoSort.SortMethod.Name:
              CurrentSortMethod = VideoSort.SortMethod.Date;
              break;
            case VideoSort.SortMethod.Date:
              CurrentSortMethod = VideoSort.SortMethod.Size;
              break;
            case VideoSort.SortMethod.Size:
              CurrentSortMethod = VideoSort.SortMethod.Year;
              break;
            case VideoSort.SortMethod.Year:
              CurrentSortMethod = VideoSort.SortMethod.Rating;
              break;
            case VideoSort.SortMethod.Rating:
              CurrentSortMethod = VideoSort.SortMethod.Label;
              break;
            case VideoSort.SortMethod.Label:
              CurrentSortMethod = VideoSort.SortMethod.Name;
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


      if (control == btnPlayDVD)
      {
        OnPlayDVD();
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
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
      if (GetID == (int)GUIWindow.Window.WINDOW_VIDEO_TITLE)
      {
          GUIPropertyManager.SetProperty("#currentmodule", String.Format("{0}/{1}", GUILocalizeStrings.Get(100006), handler.LocalizedCurrentView));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      }

      GUIControl.HideControl(GetID, facadeView.GetID);

      int iControl = facadeView.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);


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
        case View.FilmStrip:
          strLine = GUILocalizeStrings.Get(733);
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
      GUIVideoOverlay videoOverlay = (GUIVideoOverlay)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_OVERLAY);
      if ((videoOverlay != null) && (videoOverlay.Focused)) videoOverlay.Focused = false;

      LoadSettings();

      if (btnSortBy != null)
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }
    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      // Save view
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("movies", "startWindow", VideoState.StartWindow.ToString());
      }
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
        if (movie != null && movie.ID > 0)
        {
          if (CurrentSortMethod == VideoSort.SortMethod.Name)
            item.Label2 = movie.Rating.ToString();
          else if (CurrentSortMethod == VideoSort.SortMethod.Year)
            item.Label2 = movie.Year.ToString();
          else if (CurrentSortMethod == VideoSort.SortMethod.Rating)
            item.Label2 = movie.Rating.ToString();
          else if (CurrentSortMethod == VideoSort.SortMethod.Label)
            item.Label2 = movie.DVDLabel.ToString();
          else if (CurrentSortMethod == VideoSort.SortMethod.Size)
            item.Label2 = Utils.SecondsToHMString(movie.RunTime * 60);
        }
        else
        {
          string strSize1 = String.Empty, strDate = String.Empty;
          if (item.FileInfo != null) strSize1 = Utils.GetSize(item.FileInfo.Length);
          if (item.FileInfo != null) strDate = item.FileInfo.ModificationTime.ToShortDateString() + " " + item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          if (CurrentSortMethod == VideoSort.SortMethod.Name)
            item.Label2 = strSize1;
          else if (CurrentSortMethod == VideoSort.SortMethod.Date)
            item.Label2 = strDate;
          else
            item.Label2 = strSize1;
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
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(924); // menu
      dlg.Add(GUILocalizeStrings.Get(134));//videos
      foreach (ViewDefinition view in handler.Views)
      {
        dlg.Add(view.LocalizedName); //play
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      if (dlg.SelectedLabel == 0)
      {
        int nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEOS;
        VideoState.StartWindow = nNewWindow;
        if (nNewWindow != GetID)
        {
          GUIWindowManager.ReplaceWindow(nNewWindow);
        }
      }
      else
      {
        ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel - 1];
        handler.CurrentView = selectedView.Name;
        VideoState.View = selectedView.Name;
        int nNewWindow = (int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
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
          LoadDirectory(String.Empty);
        }
      }
    }
    protected virtual void LoadDirectory(string path)
    {
    }

    void OnInfoFile(GUIListItem item)
    {
    }

    void OnInfoFolder(GUIListItem item)
    {
    }

    protected virtual void OnInfo(int iItem)
    {
    }

    protected void OnPlayDVD()
    {
      Log.Write("GUIVideoFiles playDVD");
      //check if dvd is inserted
      string[] drives = Environment.GetLogicalDrives();

      foreach (string drive in drives)
      {
        if (Util.Utils.getDriveType(drive) == 5) //cd or dvd drive
        {
          string driverLetter = drive.Substring(0, 1);
          string fileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);
          if (System.IO.File.Exists(fileName))
          {
            IMDBMovie movieDetails = new IMDBMovie();
            VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
            int idFile = VideoDatabase.GetFileId(fileName);
            int idMovie = VideoDatabase.GetMovieId(fileName);
            int timeMovieStopped = 0;
            byte[] resumeData = null;
            if ((idMovie >= 0) && (idFile >= 0))
            {
              timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
              Log.Write("GUIVideoFiles::OnPlayBackStopped idFile={0} timeMovieStopped={1} resumeData={2}", idFile, timeMovieStopped, resumeData);
              if (timeMovieStopped > 0)
              {
                string title = System.IO.Path.GetFileName(fileName);
                VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
                if (movieDetails.Title != String.Empty) title = movieDetails.Title;

                GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                if (null == dlgYesNo) return;
                dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                dlgYesNo.SetLine(1, title);
                dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSString(timeMovieStopped));
                dlgYesNo.SetDefaultToYes(true);
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                if (!dlgYesNo.IsConfirmed) timeMovieStopped = 0;
              }
            }

            g_Player.PlayDVD();
            if (g_Player.Playing && timeMovieStopped > 0)
            {
              if (g_Player.IsDVD)
              {
                g_Player.Player.SetResumeState(resumeData);
              }
              else
              {
                g_Player.SeekAbsolute(timeMovieStopped);
              }
            }
            return;
          }
        }
      }
      //no disc in drive...
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(3);//my videos
      dlgOk.SetLine(1, 219);//no disc
      dlgOk.DoModal(GetID);
    }


    protected virtual void AddItemToPlayList(GUIListItem pItem)
    {
      if (!pItem.IsFolder)
      {
        //TODO
        if (Utils.IsVideo(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
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

    void SortChanged(object sender, SortEventArgs e)
    {
      CurrentSortAsc = e.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();
      UpdateButtonStates();
      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
    }
  }
}
