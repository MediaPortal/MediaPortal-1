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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
///@using MediaPortal.Video.Database;
using Toub.MediaCenter.Dvrms.Metadata;
///@using MediaPortal.TV.DiskSpace;

using TvDatabase;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvRecorded : GUIWindow, IComparer<GUIListItem>
  {
    #region variables
    enum Controls
    {
      LABEL_PROGRAMTITLE = 13,
      LABEL_PROGRAMTIME = 14,
      LABEL_PROGRAMDESCRIPTION = 15,
      LABEL_PROGRAMGENRE = 17,
    };

    enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Genre = 3,
      Played = 4,
      Duration = 5
    }
    enum ViewAs
    {
      List,
      Album
    }

    ViewAs currentViewMethod = ViewAs.Album;
    SortMethod currentSortMethod = SortMethod.Date;
    bool m_bSortAscending = true;
    bool _deleteWatchedShows = false;
    int m_iSelectedItem = 0;
    string currentShow = String.Empty;
    bool _creatingThumbNails = false;

    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]
    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnView = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnCleanup = null;

    [SkinControlAttribute(10)]
    protected GUIListControl listAlbums = null;
    [SkinControlAttribute(11)]
    protected GUIListControl listViews = null;

    #endregion
    public TvRecorded()
    {
      GetID = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
    }
    public override void OnAdded()
    {
      Log.Write("TvRecorded:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_RECORDEDTV, this);
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string strTmp = String.Empty;
        strTmp = (string)xmlreader.GetValue("tvrecorded", "sort");
        if (strTmp != null)
        {
          if (strTmp == "channel") currentSortMethod = SortMethod.Channel;
          else if (strTmp == "date") currentSortMethod = SortMethod.Date;
          else if (strTmp == "name") currentSortMethod = SortMethod.Name;
          else if (strTmp == "type") currentSortMethod = SortMethod.Genre;
          else if (strTmp == "played") currentSortMethod = SortMethod.Played;
          else if (strTmp == "duration") currentSortMethod = SortMethod.Duration;
        }
        strTmp = (string)xmlreader.GetValue("tvrecorded", "view");
        if (strTmp != null)
        {
          if (strTmp == "album") currentViewMethod = ViewAs.Album;
          else if (strTmp == "list") currentViewMethod = ViewAs.List;
        }

        m_bSortAscending = xmlreader.GetValueAsBool("tvrecorded", "sortascending", true);
        _deleteWatchedShows = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecorded", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecorded", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecorded", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecorded", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecorded", "sort", "played");
            break;
          case SortMethod.Duration:
            xmlwriter.SetValue("tvrecorded", "sort", "duration");
            break;
        }
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            xmlwriter.SetValue("tvrecorded", "view", "album");
            break;
          case ViewAs.List:
            xmlwriter.SetValue("tvrecorded", "view", "list");
            break;
        }
        xmlwriter.SetValueAsBool("tvrecorded", "sortascending", m_bSortAscending);
      }
    }
    #endregion

    #region overrides
    public override bool Init()
    {
      g_Player.PlayBackStopped += new MediaPortal.Player.g_Player.StoppedHandler(OnPlayRecordingBackStopped);
      g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(OnPlayRecordingBackEnded);
      g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayRecordingBackStarted);

      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvrecordedtv.xml");
      LoadSettings();
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_RECORDEDTV, this);
      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (listAlbums.Focus || listViews.Focus)
        {
          GUIListItem item = GetItem(0);
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              currentShow = String.Empty;
              LoadDirectory();
              return;
            }
          }
        }
      }
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_GUI:
          if (!g_Player.Playing && TVHome.Card.IsTimeShifting)
          {
            //if we're watching tv
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          else if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
          {
            //if we're watching a tv recording
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          else if (g_Player.Playing && g_Player.HasVideo)
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          break;

        case Action.ActionType.ACTION_DELETE_ITEM:
          {
            int item = GetSelectedItemNo();
            if (item >= 0)
              OnDeleteRecording(item);
            UpdateProperties();
          }
          break;
      }
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iSelectedItem = GetSelectedItemNo();
      SaveSettings();
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (TVHome.Card.IsTimeShifting && !(TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            //@Recorder.StopViewing();
          }
        }
      }
      base.OnPageDestroy(newWindowId);
    }
    protected override void OnPageLoad()
    {
      base.OnPageLoad();


      //DiskManagement.ImportDvrMsFiles();
      LoadSettings();
      LoadDirectory();

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV, GetID, 0, 0, 0, null);
      msg.SendToTargetWindow = true;
      GUIWindowManager.SendThreadMessage(msg);

      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      CreateThumbnails();
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnView)
      {
        ShowViews();
        return;
      }


      if (control == btnViewAs)
      {
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            currentViewMethod = ViewAs.List;
            break;
          case ViewAs.List:
            currentViewMethod = ViewAs.Album;
            break;
        }
        LoadDirectory();
      }

      if (control == btnSortBy) // sort by
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            currentSortMethod = SortMethod.Date;
            break;
          case SortMethod.Date:
            currentSortMethod = SortMethod.Name;
            break;
          case SortMethod.Name:
            currentSortMethod = SortMethod.Genre;
            break;
          case SortMethod.Genre:
            currentSortMethod = SortMethod.Played;
            break;
          case SortMethod.Played:
            currentSortMethod = SortMethod.Duration;
            break;
          case SortMethod.Duration:
            currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }

      if (control == btnCleanup)
      {
        OnDeleteWatchedRecordings();
      }
      if (control == listAlbums || control == listViews)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnPlayRecording(iItem);
        }
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnShowContextMenu();
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateProperties();
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      int iItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null) return;
      if (pItem.IsFolder) return;
      Recording rec = (Recording)pItem.TVTag;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(rec.Title);

      for (int i = 655; i <= 656; ++i)
      {
        dlg.Add(GUILocalizeStrings.Get(i));
      }
      dlg.Add(GUILocalizeStrings.Get(1048));//Settings

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedLabel)
      {
        case 1: // delete
          {
            OnDeleteRecording(iItem);
          }
          break;

        case 0: // play
          {
            if (OnPlayRecording(iItem))
              return;
          }
          break;

        case 2: // Settings
          {
            TvRecordedInfo.CurrentProgram = rec;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_RECORDED_INFO, true);
          }
          break;
      }
    }
    #endregion

    #region recording methods
    void ShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(652); // my recorded tv
      dlg.AddLocalizedString(914);
      dlg.AddLocalizedString(135);
      dlg.AddLocalizedString(915);
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      int nNewWindow = GetID;
      switch (dlg.SelectedId)
      {
        case 914: //	all
          nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTV;
          break;
        case 135: //	genres
          nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE;
          break;
        case 915: //	channel
          nNewWindow = (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL;
          break;
      }
      if (nNewWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(nNewWindow);
      }
    }

    void LoadDirectory()
    {
      GUIControl.ClearControl(GetID, listAlbums.GetID);
      GUIControl.ClearControl(GetID, listViews.GetID);

      List<GUIListItem> itemlist = new List<GUIListItem>();
      EntityList<Recording> recordings = DatabaseManager.Instance.GetEntities<Recording>();
      recordings.ShouldRemoveDeletedEntities = false;
      if (currentShow == String.Empty)
      {
        foreach (Recording rec in recordings)
        {
          bool add = true;
          foreach (GUIListItem item in itemlist)
          {
            Recording rec2 = item.TVTag as Recording;
            if (rec.Title.Equals(rec2.Title))
            {
              item.IsFolder = true;
              Utils.SetDefaultIcons(item);
              string strLogo = Utils.GetCoverArt(Thumbs.TVShows, rec.Title);
              if (System.IO.File.Exists(strLogo))
              {
                item.ThumbnailImage = strLogo;
                item.IconImageBig = strLogo;
                item.IconImage = strLogo;
              }
              add = false;
              break;
            }
          }
          if (add)
          {
            GUIListItem item = new GUIListItem();
            item.Label = rec.Title;
            item.TVTag = rec;
            string strLogo = System.IO.Path.ChangeExtension(rec.FileName, ".jpg");
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.Name);
              if (!System.IO.File.Exists(strLogo))
              {
                strLogo = "defaultVideoBig.png";
              }
            }
            item.ThumbnailImage = strLogo;
            item.IconImageBig = strLogo;
            item.IconImage = strLogo;
            itemlist.Add(item);
          }
        }
      }
      else
      {
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        Utils.SetDefaultIcons(item);
        itemlist.Add(item);
        foreach (Recording rec in recordings)
        {
          if (rec.Title.Equals(currentShow))
          {
            item = new GUIListItem();
            item.Label = rec.Title;
            item.TVTag = rec;
            string strLogo = System.IO.Path.ChangeExtension(rec.FileName, ".jpg");
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.Name);
              if (!System.IO.File.Exists(strLogo))
              {
                strLogo = "defaultVideoBig.png";
              }
            }
            item.ThumbnailImage = strLogo;
            item.IconImageBig = strLogo;
            item.IconImage = strLogo;
            itemlist.Add(item);
          }
        }
      }
      foreach (GUIListItem item in itemlist)
      {
        listAlbums.Add(item);
        listViews.Add(item);
      }

      string strObjects = String.Format("{0} {1}", itemlist.Count, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", strObjects);
      GUIControl cntlLabel = GetControl(12);

      if (currentViewMethod == ViewAs.Album)
        cntlLabel.YPosition = listAlbums.SpinY;
      else
        cntlLabel.YPosition = listViews.SpinY;

      OnSort();
      UpdateButtonStates();
      UpdateProperties();

    }

    void UpdateButtonStates()
    {
      string strLine = String.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620);//Sort by: Channel
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621);//Sort by: Date
          break;
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(268);//Sort by: Title
          break;
        case SortMethod.Genre:
          strLine = GUILocalizeStrings.Get(678);//Sort by: Genre
          break;
        case SortMethod.Played:
          strLine = GUILocalizeStrings.Get(671);//Sort by: Watched
          break;
        case SortMethod.Duration:
          strLine = GUILocalizeStrings.Get(1017);//Sort by: Duration
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, strLine);
      switch (currentViewMethod)
      {
        case ViewAs.Album:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case ViewAs.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);


      btnSortBy.IsAscending = m_bSortAscending;

      if (currentViewMethod == ViewAs.List)
      {
        GUIControl.HideControl(GetID, (int)Controls.LABEL_PROGRAMTITLE);
        GUIControl.HideControl(GetID, (int)Controls.LABEL_PROGRAMDESCRIPTION);
        GUIControl.HideControl(GetID, (int)Controls.LABEL_PROGRAMGENRE);
        GUIControl.HideControl(GetID, (int)Controls.LABEL_PROGRAMTIME);
        listAlbums.IsVisible = false;
        listViews.IsVisible = true;
      }
      else
      {
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTITLE);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMDESCRIPTION);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMGENRE);
        GUIControl.ShowControl(GetID, (int)Controls.LABEL_PROGRAMTIME);
        listAlbums.IsVisible = true;
        listViews.IsVisible = false;
      }
    }

    void SetLabels()
    {
      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < listAlbums.Count; ++i)
      {
        GUIListItem item1 = listAlbums[i];
        GUIListItem item2 = listViews[i];
        if (item1.Label == "..") continue;
        Recording rec = (Recording)item1.TVTag;
        item1.Label = item2.Label = rec.Title;
        TimeSpan ts = rec.EndTime - rec.StartTime;
        string strTime = String.Format("{0} {1} ({2})",
          Utils.GetShortDayString(rec.StartTime),
          rec.StartTime.ToShortTimeString(),
          Utils.SecondsToHMString((int)ts.TotalSeconds));
        item1.Label2 = item2.Label2 = strTime;
        if (currentViewMethod == ViewAs.Album)
        {
          item1.Label3 = item2.Label3 = rec.Genre;
        }
        else
        {
          if (currentSortMethod == SortMethod.Channel)
            item1.Label2 = item2.Label2 = rec.Channel.Name;
        }
      }
    }

    bool OnPlayRecording(int iItem)
    {
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null) return false;
      if (pItem.IsFolder)
      {
        if (pItem.Label.Equals(".."))
          currentShow = String.Empty;
        else
          currentShow = pItem.Label;
        LoadDirectory();
        return false;
      }

      Recording rec = (Recording)pItem.TVTag;
      //if (System.IO.File.Exists(rec.FileName))
      {
        Log.Write("TVRecording:play:{0}", rec.FileName);
        g_Player.Stop();
        TVHome.Card.StopTimeShifting();

        rec.TimesWatched++;
        DatabaseManager.SaveChanges();
        ///@
        int stoptime = 0;
        /*
                IMDBMovie movieDetails = new IMDBMovie();
                VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
                int idMovie = VideoDatabase.GetMovieId(rec.FileName);
                int idFile = VideoDatabase.GetFileId(rec.FileName);
                if (idMovie >= 0 && idFile >= 0 )
                {
                  Log.Write("play got movie id:{0} for {1}", idMovie, rec.FileName);
                  stoptime = VideoDatabase.GetMovieStopTime(idMovie);
                  if (stoptime > 0)
                  {
                    string title = System.IO.Path.GetFileName(rec.FileName);
                    if (movieDetails.Title != String.Empty) title = movieDetails.Title;

                    GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                    if (null == dlgYesNo) return false;
                    dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                    dlgYesNo.SetLine(1, rec.Channel);
                    dlgYesNo.SetLine(2, title);
                    dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + Utils.SecondsToHMSString(stoptime));
                    dlgYesNo.SetDefaultToYes(true);
                    dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                    if (!dlgYesNo.IsConfirmed) stoptime = 0;
                  }
                }
        */
        string fileName = rec.FileName;
        if (!System.IO.File.Exists(fileName))
        {
          fileName = TVHome.TvServer.GetStreamUrlForFileName(rec.IdRecording);
        }
        Log.Write("TvRecorded Play:{0}", fileName);
        if (g_Player.Play(fileName))
        {
          if (Utils.IsVideo(fileName))
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          if (stoptime > 0)
          {
            g_Player.SeekAbsolute(stoptime);
          }
          return true;
        }
      }
      return false;
    }

    void OnDeleteRecording(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null) return;
      if (pItem.IsFolder) return;
      Recording rec = (Recording)pItem.TVTag;

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo) return;
      if (rec.TimesWatched > 0) dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
      else dlgYesNo.SetHeading(GUILocalizeStrings.Get(820));
      dlgYesNo.SetLine(1, rec.Channel.Name);
      dlgYesNo.SetLine(2, rec.Title);
      dlgYesNo.SetLine(3, String.Empty);
      dlgYesNo.SetDefaultToYes(false);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      //@
      //VideoDatabase.DeleteMovieInfo(rec.FileName);
      //VideoDatabase.DeleteMovie(rec.FileName);
      Utils.DeleteRecording(rec.FileName);

      rec.Delete();
      DatabaseManager.SaveChanges();
      DatabaseManager.Instance.ClearQueryCache();

      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);
    }

    void OnDeleteWatchedRecordings()
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo) return;
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(676));//delete watched recordings?
      dlgYesNo.SetLine(1, String.Empty);
      dlgYesNo.SetLine(2, String.Empty);
      dlgYesNo.SetLine(3, String.Empty);
      dlgYesNo.SetDefaultToYes(true);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed) return;
      EntityList<Recording> itemlist = DatabaseManager.Instance.GetEntities<Recording>();
      itemlist.ShouldRemoveDeletedEntities = false;
      foreach (Recording rec in itemlist)
      {
        if (rec.TimesWatched > 0)
        {
          rec.Delete();
          DatabaseManager.SaveChanges();
          DatabaseManager.Instance.Clear();
          //@Recorder.DeleteRecording(rec);
        }
        else if (!System.IO.File.Exists(rec.FileName))
        {
          //@Recorder.DeleteRecording(rec);
          rec.Delete();
          DatabaseManager.SaveChanges();
          DatabaseManager.Instance.Clear();
        }
      }

      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);
    }

    void UpdateProperties()
    {
      Recording rec;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        SetProperties(null);
        return;
      }
      rec = pItem.TVTag as Recording;
      if (rec == null)
      {
        SetProperties(null);
        return;
      }
      SetProperties(rec);
    }

    void SetProperties(Recording rec)
    {
      if (rec == null)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", "");
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "");
        return;
      }
      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(rec.StartTime),
        rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.Title);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", rec.Genre);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
      GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", rec.Description);
      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel.Name);
      if (System.IO.File.Exists(strLogo))
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
    }
    #endregion

    #region album/list view management
    GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl = listAlbums.GetID;
      if (currentViewMethod == ViewAs.List)
        iControl = listViews.GetID;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      if (currentViewMethod == ViewAs.List)
      {
        if (iItem < 0 || iItem >= listViews.Count) return null;
        return listViews[iItem];
      }
      else
      {
        if (iItem < 0 || iItem >= listAlbums.Count) return null;
        return listAlbums[iItem];
      }
    }

    int GetSelectedItemNo()
    {
      int iControl;
      iControl = listAlbums.GetID;
      if (currentViewMethod == ViewAs.List)
        iControl = listViews.GetID;

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      if (currentViewMethod == ViewAs.List)
        return listViews.Count;
      else
        return listAlbums.Count;
    }
    #endregion

    #region Sort Members
    void OnSort()
    {
      SetLabels();
      listAlbums.Sort(this);
      listViews.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;

      int iComp = 0;
      TimeSpan ts;
      Recording rec1 = (Recording)item1.TVTag;
      Recording rec2 = (Recording)item2.TVTag;
      switch (currentSortMethod)
      {
        case SortMethod.Played:
          item1.Label2 = String.Format("{0} {1}", rec1.TimesWatched, GUILocalizeStrings.Get(677));//times
          item2.Label2 = String.Format("{0} {1}", rec2.TimesWatched, GUILocalizeStrings.Get(677));//times
          if (rec1.TimesWatched == rec2.TimesWatched) goto case SortMethod.Name;
          else
          {
            if (m_bSortAscending) return rec1.TimesWatched - rec2.TimesWatched;
            else return rec2.TimesWatched - rec1.TimesWatched;
          }

        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Title, rec2.Title, true);
            if (iComp == 0) goto case SortMethod.Channel;
            else return iComp;
          }
          else
          {
            iComp = String.Compare(rec2.Title, rec1.Title, true);
            if (iComp == 0) goto case SortMethod.Channel;
            else return iComp;
          }


        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Channel.Name, rec2.Channel.Name, true);
            if (iComp == 0) goto case SortMethod.Date;
            else return iComp;
          }
          else
          {
            iComp = String.Compare(rec2.Channel.Name, rec1.Channel.Name, true);
            if (iComp == 0) goto case SortMethod.Date;
            else return iComp;
          }

        case SortMethod.Duration:
          {
            TimeSpan duration1 = (rec1.EndTime - rec1.StartTime);
            TimeSpan duration2 = rec2.EndTime - rec2.StartTime;
            if (m_bSortAscending)
            {
              if (duration1 == duration2) goto case SortMethod.Date;
              if (duration1 > duration2) return 1;
              return -1;
            }
            else
            {
              if (duration1 == duration2) goto case SortMethod.Date;
              if (duration1 < duration2) return 1;
              return -1;
            }
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime == rec2.StartTime) return 0;
            if (rec1.StartTime < rec2.StartTime) return 1;
            return -1;
          }
          else
          {
            if (rec1.StartTime == rec2.StartTime) return 0;
            if (rec1.StartTime > rec2.StartTime) return 1;
            return -1;
          }

        case SortMethod.Genre:
          item1.Label2 = rec1.Genre;
          item2.Label2 = rec2.Genre;
          if (rec1.Genre != rec2.Genre)
          {
            if (m_bSortAscending)
              return String.Compare(rec1.Genre, rec2.Genre, true);
            else
              return String.Compare(rec2.Genre, rec1.Genre, true);
          }
          if (rec1.StartTime != rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts = rec1.StartTime - rec2.StartTime;
              return (int)(ts.Minutes);
            }
            else
            {
              ts = rec2.StartTime - rec1.StartTime;
              return (int)(ts.Minutes);
            }
          }
          if (rec1.Channel != rec2.Channel)
            if (m_bSortAscending)
              return String.Compare(rec1.Channel.Name, rec2.Channel.Name);
            else
              return String.Compare(rec2.Channel.Name, rec1.Channel.Name);
          if (rec1.Title != rec2.Title)
            if (m_bSortAscending)
              return String.Compare(rec1.Title, rec2.Title);
            else
              return String.Compare(rec2.Title, rec1.Title);
          return 0;
      }
      return 0;
    }
    #endregion


    #region playback events
    private void OnPlayRecordingBackStopped(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Recording) return;
      //@
      /*
      int movieid = VideoDatabase.GetMovieId(filename);
      if (movieid < 0) return;
      if (stoptime > 0)
        VideoDatabase.SetMovieStopTime(movieid, stoptime);
      else
        VideoDatabase.DeleteMovieStopTime(movieid);
      if (GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV, GetID, 0, 0, 0, null);
        msg.SendToTargetWindow = true;
        GUIWindowManager.SendThreadMessage(msg);
      }*/
    }

    private void OnPlayRecordingBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording) return;
      //@int movieid = VideoDatabase.GetMovieId(filename);
      //@if (movieid < 0) return;

      //@VideoDatabase.DeleteMovieStopTime(movieid);

      g_Player.Stop();

      EntityList<Recording> itemlist = DatabaseManager.Instance.GetEntities<Recording>();
      itemlist.ShouldRemoveDeletedEntities = false;
      foreach (Recording rec in itemlist)
      {
        if (_deleteWatchedShows || rec.KeepUntil == (int)KeepMethodType.UntilWatched)
        {
          if (String.Compare(rec.FileName, filename, true) == 0)
          {
            rec.Delete();
            DatabaseManager.SaveChanges();
            DatabaseManager.Instance.Clear();
            return;
          }
        }
      }
      //@IMDBMovie details = new IMDBMovie();
      //@VideoDatabase.GetMovieInfoById(movieid, ref details);
      //@details.Watched++;
      //@VideoDatabase.SetWatched(details);
    }

    private void OnPlayRecordingBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording) return;
      //@VideoDatabase.AddMovieFile(filename);
    }

    #endregion

    void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;
      OnSort();
    }
    void CreateThumbnails()
    {
      if (_creatingThumbNails) return;
      Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));
      WorkerThread.SetApartmentState(ApartmentState.STA);
      WorkerThread.IsBackground = true;
      WorkerThread.Priority = ThreadPriority.BelowNormal;
      WorkerThread.Start();
    }

    void WorkerThreadFunction()
    {
      if (_creatingThumbNails) return;
      try
      {
        _creatingThumbNails = true;
        EntityList<Recording> recordings = DatabaseManager.Instance.GetEntities<Recording>();
        foreach (Recording rec in recordings)
        {
          string thumbNail = System.IO.Path.ChangeExtension(rec.FileName, ".jpg");
          if (!System.IO.File.Exists(thumbNail))
          {
            DvrMsImageGrabber.GrabFrame(rec.FileName, thumbNail, System.Drawing.Imaging.ImageFormat.Jpeg, 128, 128);
          }
        }
      }
      finally
      {
        _creatingThumbNails = false;
      }
    }
  }
}
