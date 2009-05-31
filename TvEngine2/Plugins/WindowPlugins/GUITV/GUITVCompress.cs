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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITVCompress : GUIWindow, IComparer<GUIListItem>
  {
    #region enums

    private enum Controls
    {
      LABEL_PROGRAMTITLE = 13,
      LABEL_PROGRAMTIME = 14,
      LABEL_PROGRAMDESCRIPTION = 15,
      LABEL_PROGRAMGENRE = 17,
    } ;

    private enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Genre = 3,
      Played = 4,
    }

    private enum ViewAs
    {
      List,
      Album
    }

    #endregion

    #region variables

    private ViewAs currentViewMethod = ViewAs.Album;
    private SortMethod currentSortMethod = SortMethod.Date;
    private bool m_bSortAscending = true;
    private bool m_bDeleteWatchedShow = false;
    private int m_iSelectedItem = 0;
    private string currentShow = string.Empty;

    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnSelectAll = null;
    [SkinControl(6)] protected GUIButtonControl btnSelectNone = null;
    [SkinControl(7)] protected GUIButtonControl btnOK = null;
    [SkinControl(10)] protected GUIListControl listAlbums = null;
    [SkinControl(11)] protected GUIListControl listViews = null;

    #endregion

    public GUITVCompress()
    {
      GetID = (int) Window.WINDOW_TV_COMPRESS_COMPRESS;
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strTmp = string.Empty;
        strTmp = (string) xmlreader.GetValue("tvcompress", "sort");
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
        strTmp = (string) xmlreader.GetValue("tvcompress", "view");
        if (strTmp != null)
        {
          if (strTmp == "albu,")
          {
            currentViewMethod = ViewAs.Album;
          }
          else if (strTmp == "list")
          {
            currentViewMethod = ViewAs.List;
          }
        }
        m_bSortAscending = xmlreader.GetValueAsBool("tvcompress", "sortascending", true);
        m_bDeleteWatchedShow = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvcompress", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvcompress", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvcompress", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvcompress", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvcompress", "sort", "played");
            break;
        }
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            xmlwriter.SetValue("tvcompress", "view", "album");
            break;
          case ViewAs.List:
            xmlwriter.SetValue("tvcompress", "view", "list");
            break;
        }
        xmlwriter.SetValueAsBool("tvcompress", "sortascending", m_bSortAscending);
      }
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvcompress_TVE2.xml");
      LoadSettings();
      return bResult;
    }

    public override void OnAction(Action action)
    {
      //switch (action.wID)
      //{
      //}
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iSelectedItem = GetSelectedItemNo();
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
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int) Window.WINDOW_TV, GetID, 0, 0, 0,
                                      null);
      msg.SendToTargetWindow = true;
      GUIWindowManager.SendThreadMessage(msg);
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, listViews.GetID, m_iSelectedItem);
      GUIControl.SelectItemControl(GetID, listAlbums.GetID, m_iSelectedItem);
      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnSelectAll)
      {
        for (int i = 0; i < GetItemCount(); ++i)
        {
          listAlbums[i].Selected = true;
          listViews[i].Selected = true;
        }
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
            currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }
      if (control == btnSelectNone)
      {
        for (int i = 0; i < GetItemCount(); ++i)
        {
          listAlbums[i].Selected = false;
          listViews[i].Selected = false;
        }
      }
      if (control == listAlbums || control == listViews)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int) msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          GUIListItem item = GetItem(iItem);
          if (item != null)
          {
            item.Selected = !item.Selected;
          }
        }
      }
      if (control == btnOK)
      {
        OnTranscode();
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

    private void LoadDirectory()
    {
      GUIWaitCursor.Show();
      GUIControl.ClearControl(GetID, listAlbums.GetID);
      GUIControl.ClearControl(GetID, listViews.GetID);
      List<TVRecorded> recordings = new List<TVRecorded>();
      List<GUIListItem> itemlist = new List<GUIListItem>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        if (Transcoder.IsTranscoding(rec))
        {
          continue; //already transcoding...
        }
        GUIListItem item = new GUIListItem();
        item.Label = rec.Title;
        item.TVTag = rec;
        string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel);
        if (!File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        itemlist.Add(item);
      }
      foreach (GUIListItem item in itemlist)
      {
        listAlbums.Add(item);
        listViews.Add(item);
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(itemlist.Count));

      OnSort();
      UpdateProperties();
      GUIWaitCursor.Hide();
    }

    private void UpdateButtonStates()
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
        GUIControl.HideControl(GetID, (int) Controls.LABEL_PROGRAMTITLE);
        GUIControl.HideControl(GetID, (int) Controls.LABEL_PROGRAMDESCRIPTION);
        GUIControl.HideControl(GetID, (int) Controls.LABEL_PROGRAMGENRE);
        GUIControl.HideControl(GetID, (int) Controls.LABEL_PROGRAMTIME);
        listAlbums.IsVisible = false;
        listViews.IsVisible = true;
      }
      else
      {
        GUIControl.ShowControl(GetID, (int) Controls.LABEL_PROGRAMTITLE);
        GUIControl.ShowControl(GetID, (int) Controls.LABEL_PROGRAMDESCRIPTION);
        GUIControl.ShowControl(GetID, (int) Controls.LABEL_PROGRAMGENRE);
        GUIControl.ShowControl(GetID, (int) Controls.LABEL_PROGRAMTIME);
        listAlbums.IsVisible = true;
        listViews.IsVisible = false;
      }
    }

    private void SetLabels()
    {
      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;
      for (int i = 0; i < listAlbums.Count; ++i)
      {
        GUIListItem item1 = listAlbums[i];
        GUIListItem item2 = listViews[i];
        if (item1.Label == "..")
        {
          continue;
        }
        TVRecorded rec = (TVRecorded) item1.TVTag;
        item1.Label = item2.Label = rec.Title;
        TimeSpan ts = rec.EndTime - rec.StartTime;
        string strTime = String.Format("{0} {1} ",
                                       Util.Utils.GetShortDayString(rec.StartTime),
                                       Util.Utils.SecondsToHMString((int) ts.TotalSeconds));
        item1.Label2 = item2.Label2 = strTime;
        if (currentViewMethod == ViewAs.Album)
        {
          item1.Label3 = item2.Label3 = rec.Genre;
        }
        else
        {
          if (currentSortMethod == SortMethod.Channel)
          {
            item1.Label2 = item2.Label2 = rec.Channel;
          }
        }
      }
    }

    private void OnTranscode()
    {
      ArrayList transcodings = new ArrayList();
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.Selected)
        {
          TVRecorded rec = item.TVTag as TVRecorded;
          transcodings.Add(rec);
        }
      }
      if (transcodings.Count == 0)
      {
        return; //nothing selected
      }
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      dlgYesNo.SetHeading(894);
      dlgYesNo.SetLine(1, 995);
      dlgYesNo.SetLine(2, 996);
      dlgYesNo.DoModal(GetID);
      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      foreach (TVRecorded rec in transcodings)
      {
        Transcoder.Transcode(rec, true);
      }
      //now switch to status screen....
      GUIWindowManager.ActivateWindow((int) Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS, true);
    }

    private void UpdateProperties()
    {
      TVRecorded rec;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        rec = new TVRecorded();
        rec.SetProperties();
        return;
      }
      rec = pItem.TVTag as TVRecorded;
      if (rec == null)
      {
        rec = new TVRecorded();
        rec.SetProperties();
        return;
      }
      rec.SetProperties();
    }

    #endregion

    #region album/list view management

    private GUIListItem GetSelectedItem()
    {
      int iControl;
      iControl = listAlbums.GetID;
      if (currentViewMethod == ViewAs.List)
      {
        iControl = listViews.GetID;
      }
      GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return item;
    }

    private GUIListItem GetItem(int iItem)
    {
      if (currentViewMethod == ViewAs.List)
      {
        if (iItem < 0 || iItem >= listViews.Count)
        {
          return null;
        }
        return listViews[iItem];
      }
      else
      {
        if (iItem < 0 || iItem >= listAlbums.Count)
        {
          return null;
        }
        return listAlbums[iItem];
      }
    }

    private int GetSelectedItemNo()
    {
      int iControl;
      iControl = listAlbums.GetID;
      if (currentViewMethod == ViewAs.List)
      {
        iControl = listViews.GetID;
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
      OnMessage(msg);
      int iItem = (int) msg.Param1;
      return iItem;
    }

    private int GetItemCount()
    {
      if (currentViewMethod == ViewAs.List)
      {
        return listViews.Count;
      }
      else
      {
        return listAlbums.Count;
      }
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      SetLabels();
      listAlbums.Sort(this);
      listViews.Sort(this);
      UpdateButtonStates();
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
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }
      int iComp = 0;
      TimeSpan ts;
      TVRecorded rec1 = (TVRecorded) item1.TVTag;
      TVRecorded rec2 = (TVRecorded) item2.TVTag;
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

    private void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != SortOrder.Descending;
      OnSort();
      GUIControl.FocusControl(GetID, ((GUIControl) sender).GetID);
    }
  }
}