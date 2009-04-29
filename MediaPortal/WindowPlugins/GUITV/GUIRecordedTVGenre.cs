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
using MediaPortal.Player;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIRecordedTVGenre : GUIWindow, IComparer<GUIListItem>
  {
    private enum Controls
    {
      CONTROL_BTNSORTBY = 3,
      CONTROL_BTNVIEW = 5,
      CONTROL_BTNCLEANUP = 6,
      CONTROL_ALBUM = 10,
      CONTROL_LIST = 11,
    } ;

    private enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Genre = 3,
      Played = 4,
    }

    private SortMethod currentSortMethod = SortMethod.Date;
    private bool m_bSortAscending = true;
    private bool showRoot = true;
    private string currentGenre = string.Empty;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnView = null;
    [SkinControl(6)] protected GUIButtonControl btnCleanup = null;
    [SkinControl(50)] protected GUIFacadeControl facadeView = null;

    public GUIRecordedTVGenre()
    {
      GetID = (int) Window.WINDOW_RECORDEDTVGENRE;
    }

    ~GUIRecordedTVGenre()
    {
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvrecordedtvgenre.xml");
      LoadSettings();
      return bResult;
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strTmp = "";
        strTmp = (string) xmlreader.GetValue("tvrecordedgenre", "sort");
        if (strTmp != null)
        {
          if (strTmp == "channel")
          {
            currentSortMethod = SortMethod.Channel;
          }
          else if (strTmp == "date")
          {
            currentSortMethod = SortMethod.Date;
          }
          else if (strTmp == "name")
          {
            currentSortMethod = SortMethod.Name;
          }
          else if (strTmp == "type")
          {
            currentSortMethod = SortMethod.Genre;
          }
          else if (strTmp == "played")
          {
            currentSortMethod = SortMethod.Played;
          }
        }
        m_bSortAscending = xmlreader.GetValueAsBool("tvrecordedgenre", "sortascending", true);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecordedgenre", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecordedgenre", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecordedgenre", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecordedgenre", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecordedgenre", "sort", "played");
            break;
        }
        xmlwriter.SetValueAsBool("tvrecordedgenre", "sortascending", m_bSortAscending);
      }
    }

    #endregion

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = GetItem(0);
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              showRoot = true;
              LoadDirectory();
              return;
            }
          }
        }
      }
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
        case Action.ActionType.ACTION_DELETE_ITEM:
          {
            int item = GetSelectedItemNo();
            if (item >= 0)
            {
              OnDeleteItem(item);
            }
          }
          break;
      }
      if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        ShowContextMenu();
      }
      base.OnAction(action);
      Update();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      LoadSettings();
      LoadDirectory();
      if (Recorder.IsViewing())
      {
        GUIControl cntl = GetControl(300);
        if (cntl != null)
        {
          cntl.Visible = true;
        }
        cntl = GetControl(99);
        if (cntl != null)
        {
          cntl.Visible = true;
        }

        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int) Window.WINDOW_TV, GetID, 0, 0, 0,
                                        null);
        msg.SendToTargetWindow = true;
        GUIWindowManager.SendThreadMessage(msg);
      }
      else
      {
        GUIControl cntl = GetControl(300);
        if (cntl != null)
        {
          cntl.Visible = false;
        }
        cntl = GetControl(99);
        if (cntl != null)
        {
          cntl.Visible = false;
        }
      }
      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    private void ShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(652); // my recorded tv
      dlg.AddLocalizedString(914);
      dlg.AddLocalizedString(135);
      dlg.AddLocalizedString(915);
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      int nNewWindow = GetID;
      switch (dlg.SelectedId)
      {
        case 914: //	all
          nNewWindow = (int) Window.WINDOW_RECORDEDTV;
          break;
        case 135: //	genres
          nNewWindow = (int) Window.WINDOW_RECORDEDTVGENRE;
          break;
        case 915: //	channel
          nNewWindow = (int) Window.WINDOW_RECORDEDTVCHANNEL;
          break;
      }
      if (nNewWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(nNewWindow);
      }
    }


    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnSortBy)
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
            currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }

      if (control == btnView)
      {
        ShowViews();
        return;
      }
      if (control == btnCleanup)
      {
        OnDeleteWatchedRecordings();
      }
      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int) msg.Param1;
        GUIListItem item = GetSelectedItem();
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (showRoot)
          {
            currentGenre = item.Label;
            showRoot = false;
            LoadDirectory();
          }
          else
          {
            if (item.Label == "..")
            {
              showRoot = true;
              LoadDirectory();
            }
            else
            {
              OnPlay(iItem);
            }
          }
        }

        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnShowContextMenu();
        }
      }
    }

    private void LoadDirectory()
    {
      GUIWaitCursor.Show();
      String strDefaultUnseenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoBig.png";
      String strDefaultSeenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoSeenBig.png";

      int iControl = facadeView.GetID;
      //if (showRoot)
      //  iControl = listViews.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);

      GUIControl.ClearControl(GetID, facadeView.GetID);

      int objects = 0;
      List<TVRecorded> itemlist = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref itemlist);
      if (showRoot)
      {
        facadeView.View = GUIFacadeControl.ViewMode.List;
        List<string> genres = new List<string>();
        foreach (TVRecorded rec in itemlist)
        {
          bool add = true;
          string genre = rec.Genre;
          if (rec.Genre.Length < 2)
          {
            rec.Genre = GUILocalizeStrings.Get(2014); //unknown
          }
          for (int i = 0; i < genres.Count; ++i)
          {
            string tmpGenre = (string) genres[i];
            if (tmpGenre == genre)
            {
              add = false;
              break;
            }
          }
          if (add)
          {
            genres.Add(rec.Genre);
            objects++;
            GUIListItem item = new GUIListItem();
            item.Label = rec.Genre;
            string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel);
            if (!File.Exists(strLogo))
            {
              strLogo = rec.Played > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
            }
            item.ThumbnailImage = strLogo;
            item.IconImageBig = strLogo;
            item.IconImage = strLogo;
            facadeView.Add(item);
          }
        }
      }
      else
      {
        facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
        GUIListItem item = new GUIListItem();
        item.IsFolder = true;
        item.Label = "..";
        Util.Utils.SetDefaultIcons(item);
        item.IconImage = item.IconImageBig;
        facadeView.Add(item);
        objects++;
        foreach (TVRecorded rec in itemlist)
        {
          if (rec.Genre.Length < 2)
          {
            rec.Genre = GUILocalizeStrings.Get(2014); //unknown
          }
          if (rec.Genre == currentGenre)
          {
            objects++;
            item = new GUIListItem();
            item.Label = rec.Title;
            item.TVTag = rec;
            string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel);
            if (!File.Exists(strLogo))
            {
              strLogo = rec.Played > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
            }
            item.ThumbnailImage = strLogo;
            item.IconImageBig = strLogo;
            item.IconImage = strLogo;
            facadeView.Add(item);
          }
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(objects));

      OnSort();
      //UpdateButtons(); done in OnSort()
      Update();
      GUIWaitCursor.Hide();
    }

    private void UpdateButtons()
    {
      string strLine = string.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620); //Sort by: Channel
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621); //Sort by: Date
          break;
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(268); //Sort by: Title
          break;
        case SortMethod.Genre:
          strLine = GUILocalizeStrings.Get(678); //Sort by: Genre
          break;
        case SortMethod.Played:
          strLine = GUILocalizeStrings.Get(671); //Sort by: Watched
          break;
      }
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNSORTBY, strLine);

      btnSortBy.IsAscending = m_bSortAscending;
    }

    private GUIListItem GetSelectedItem()
    {
      //int iControl;
      //iControl = listAlbums.GetID;
      //if (showRoot)
      //  iControl = listViews.GetID;
      //GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      //return item;
      return facadeView.SelectedListItem;
    }

    private GUIListItem GetItem(int iItem)
    {
      if (iItem < 0 || iItem >= facadeView.Count)
      {
        return null;
      }
      return facadeView[iItem];
    }

    private int GetSelectedItemNo()
    {
      //int iControl;
      //iControl = listAlbums.GetID;
      //if (showRoot)
      //  iControl = listViews.GetID;

      //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
      //OnMessage(msg);
      //int iItem = (int)msg.Param1;
      //return iItem;
      return facadeView.SelectedListItemIndex;
    }

    private int GetItemCount()
    {
      return facadeView.Count;
    }

    #region Sort Members

    private void OnSort()
    {
      SetLabels();
      facadeView.Sort(this);
      UpdateButtons();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }

      if (item1.Label == "..")
      {
        return -1;
      }
      if (item2.Label == "..")
      {
        return 1;
      }
      if (showRoot)
      {
        if (m_bSortAscending)
        {
          return String.Compare(item1.Label, item2.Label, true);
        }
        else
        {
          return String.Compare(item2.Label, item1.Label, true);
        }
      }

      int iComp = 0;
      TimeSpan ts;
      TVRecorded rec1 = item1.TVTag as TVRecorded;
      TVRecorded rec2 = item2.TVTag as TVRecorded;
      switch (currentSortMethod)
      {
        case SortMethod.Played:
          item1.Label2 = String.Format("{0} {1}", rec1.Played, GUILocalizeStrings.Get(677)); //times
          item2.Label2 = String.Format("{0} {1}", rec2.Played, GUILocalizeStrings.Get(677)); //times
          if (rec1.Played == rec2.Played)
          {
            goto case SortMethod.Name;
          }
          else
          {
            if (m_bSortAscending)
            {
              return rec1.Played - rec2.Played;
            }
            else
            {
              return rec2.Played - rec1.Played;
            }
          }

        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Title, rec2.Title, true);
            if (iComp == 0)
            {
              goto case SortMethod.Channel;
            }
            else
            {
              return iComp;
            }
          }
          else
          {
            iComp = String.Compare(rec2.Title, rec1.Title, true);
            if (iComp == 0)
            {
              goto case SortMethod.Channel;
            }
            else
            {
              return iComp;
            }
          }


        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Channel, rec2.Channel, true);
            if (iComp == 0)
            {
              goto case SortMethod.Date;
            }
            else
            {
              return iComp;
            }
          }
          else
          {
            iComp = String.Compare(rec2.Channel, rec1.Channel, true);
            if (iComp == 0)
            {
              goto case SortMethod.Date;
            }
            else
            {
              return iComp;
            }
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime == rec2.StartTime)
            {
              return 0;
            }
            if (rec1.StartTime > rec2.StartTime)
            {
              return 1;
            }
            return -1;
          }
          else
          {
            if (rec2.StartTime == rec1.StartTime)
            {
              return 0;
            }
            if (rec2.StartTime > rec1.StartTime)
            {
              return 1;
            }
            return -1;
          }

        case SortMethod.Genre:
          item1.Label2 = rec1.Genre;
          item2.Label2 = rec2.Genre;
          if (rec1.Genre != rec2.Genre)
          {
            if (m_bSortAscending)
            {
              return String.Compare(rec1.Genre, rec2.Genre, true);
            }
            else
            {
              return String.Compare(rec2.Genre, rec1.Genre, true);
            }
          }
          if (rec1.StartTime != rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts = rec1.StartTime - rec2.StartTime;
              return (int) (ts.Minutes);
            }
            else
            {
              ts = rec2.StartTime - rec1.StartTime;
              return (int) (ts.Minutes);
            }
          }
          if (rec1.Channel != rec2.Channel)
          {
            if (m_bSortAscending)
            {
              return String.Compare(rec1.Channel, rec2.Channel);
            }
            else
            {
              return String.Compare(rec2.Channel, rec1.Channel);
            }
          }
          if (rec1.Title != rec2.Title)
          {
            if (m_bSortAscending)
            {
              return String.Compare(rec1.Title, rec2.Title);
            }
            else
            {
              return String.Compare(rec2.Title, rec1.Title);
            }
          }
          return 0;
      }
      return 0;
    }

    #endregion

    private void SetLabels()
    {
      if (showRoot)
      {
        return;
      }
      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        TVRecorded rec = item.TVTag as TVRecorded;
        if (rec != null)
        {
          item.Label = rec.Title;
          TimeSpan ts = rec.EndTime - rec.StartTime;
          string strTime = String.Format("{0} {1} ",
                                         Util.Utils.GetShortDayString(rec.StartTime),
                                         Util.Utils.SecondsToHMString((int) ts.TotalSeconds));
          item.Label2 = strTime;
          if (rec.Genre != "unknown")
          {
            item.Label3 = rec.Genre;
          }
          else
          {
            item.Label3 = string.Empty;
          }
          if (rec.Played > 0)
          {
            item.IsPlayed = true;
          }
        }
      }
    }


    private void ShowContextMenu()
    {
      int iItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      TVRecorded rec = pItem.TVTag as TVRecorded;

      if (pItem.IsFolder && !showRoot && pItem.Label == "..")
      {
        showRoot = true;
        LoadDirectory();
        return;
      }
      if (pItem.IsFolder)
      {
        return;
      }

      if (showRoot)
      {
        currentGenre = pItem.Label;
        showRoot = false;
        LoadDirectory();
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(rec.Title);

      for (int i = 655; i <= 656; ++i)
      {
        dlg.Add(GUILocalizeStrings.Get(i));
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedLabel)
      {
        case 1: // delete
          {
            OnDeleteItem(iItem);
            LoadDirectory();
          }
          break;

        case 0: // play
          {
            if (OnPlay(iItem))
            {
              return;
            }
          }
          break;
      }
    }

    private bool OnPlay(int iItem)
    {
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return false;
      }
      if (pItem.IsFolder)
      {
        return false;
      }

      TVRecorded rec = (TVRecorded) pItem.TVTag;
      if (File.Exists(rec.FileName))
      {
        Log.Info("TVRecording:play:{0}", rec.FileName);
        g_Player.Stop();
        Recorder.StopViewing();

        rec.Played++;
        TVDatabase.PlayedRecordedTV(rec);
        IMDBMovie movieDetails = new IMDBMovie();
        int movieid = VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
        int stoptime = 0;
        if (movieid >= 0)
        {
          stoptime = VideoDatabase.GetMovieStopTime(movieid);
          if (stoptime > 0)
          {
            string title = Path.GetFileName(rec.FileName);
            if (movieDetails.Title != string.Empty)
            {
              title = movieDetails.Title;
            }

            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo)
            {
              return false;
            }
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, rec.Channel);
            dlgYesNo.SetLine(2, title);
            dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + Util.Utils.SecondsToHMSString(stoptime));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (!dlgYesNo.IsConfirmed)
            {
              stoptime = 0;
            }
          }
        }
        Log.Info("GUIRecordedTV Play:{0}", rec.FileName);
        if (g_Player.Play(rec.FileName))
        {
          if (Util.Utils.IsVideo(rec.FileName))
          {
            g_Player.ShowFullScreenWindow();
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

    private void OnDeleteItem(int iItem)
    {
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsFolder)
      {
        return;
      }
      TVRecorded rec = (TVRecorded) pItem.TVTag;
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
      dlgYesNo.SetLine(1, rec.Channel);
      dlgYesNo.SetLine(2, rec.Title);
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.SetDefaultToYes(true);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      Recorder.DeleteRecording(rec);
      LoadDirectory();
    }

    private void OnDeleteWatchedRecordings()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(676)); //delete watched recordings?
      dlgYesNo.SetLine(1, string.Empty);
      dlgYesNo.SetLine(2, string.Empty);
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.SetDefaultToYes(true);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      List<TVRecorded> itemlist = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref itemlist);
      foreach (TVRecorded rec in itemlist)
      {
        if (rec.Played > 0)
        {
          Recorder.DeleteRecording(rec);
        }
        else if (!File.Exists(rec.FileName))
        {
          Recorder.DeleteRecording(rec);
        }
      }

      LoadDirectory();
    }

    private void Update()
    {
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", string.Empty);
        return;
      }
      TVRecorded rec = pItem.TVTag as TVRecorded;
      if (rec != null)
      {
        string strTime = String.Format("{0} {1} - {2}",
                                       Util.Utils.GetShortDayString(rec.StartTime),
                                       rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                       rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", rec.Title);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", rec.Genre);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", strTime);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", rec.Description);

        string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel);
        if (File.Exists(strLogo))
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", strLogo);
        }
        else
        {
          GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
        }
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Title", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Genre", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Time", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.Description", string.Empty);
        GUIPropertyManager.SetProperty("#TV.RecordedTV.thumb", "defaultVideoBig.png");
      }
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != SortOrder.Descending;
      OnSort();
    }
  }
}