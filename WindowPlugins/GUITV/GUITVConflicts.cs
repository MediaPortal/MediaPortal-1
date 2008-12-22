#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.GUI.TV
{
  public class GUITVConflicts : GUIWindow
  {
    #region variables, ctor/dtor

    [SkinControlAttribute(10)]
    protected GUIListControl listConflicts = null;
    int m_iSelectedItem = 0;
    TVRecording currentShow = null;
    TVRecording currentEpisode = null;
    bool needUpdate = false;

    public GUITVConflicts()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_CONFLICTS;
    }
    ~GUITVConflicts()
    {
    }

    #endregion

    #region overrides
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvconflicts.xml");
      return bResult;
    }


    public override void OnAction(Action action)
    {
      //switch (action.wID)
      //{
      //}
      base.OnAction(action);
    }
    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      ConflictManager.OnConflictsUpdated += new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);

      needUpdate = false;
      LoadDirectory();

      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0) m_iSelectedItem--;
      GUIControl.SelectItemControl(GetID, listConflicts.GetID, m_iSelectedItem);

    }
    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      m_iSelectedItem = GetSelectedItemNo();
      ConflictManager.OnConflictsUpdated -= new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);

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
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == listConflicts)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
      }

    }

    protected override void OnShowContextMenu()
    {
      OnClick(GetSelectedItemNo());
    }


    #endregion

    #region list management
    GUIListItem GetSelectedItem()
    {
      return listConflicts.SelectedListItem;
    }

    GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listConflicts.Count) return null;
      return listConflicts[index];
    }

    int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listConflicts.GetID, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      return listConflicts.Count;
    }
    #endregion

    #region scheduled tv methods
    void LoadDirectory()
    {
      GUIWaitCursor.Show();
      GUIControl.ClearControl(GetID, listConflicts.GetID);

      int total = 0;
      if (currentShow != null)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "..";
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        listConflicts.Add(item);
        if (currentEpisode == null && currentShow.RecType != TVRecording.RecordingType.Once)
        {
          List<TVRecording> showEpisode = new List<TVRecording>();
          ConflictManager.GetConflictingSeries(currentShow, showEpisode);
          foreach (TVRecording showSerie in showEpisode)
          {
            item = new GUIListItem();
            item.Label = showSerie.Title;
            item.TVTag = showSerie;

            item.PinImage = Thumbs.TvConflictRecordingIcon;
            item.IsFolder = true;
            MediaPortal.Util.Utils.SetDefaultIcons(item);
            listConflicts.Add(item);
          }
          total++;
        }
        else
        {
          TVRecording eps = currentEpisode;
          if (eps == null)
            eps = currentShow;

          TVRecording[] conflicts = ConflictManager.GetConflictingRecordings(eps);

          eps.RecType = TVRecording.RecordingType.Once;
          item = new GUIListItem();
          item.Label = eps.Title;
          item.TVTag = eps;
          string strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, eps.Channel);
          if (!System.IO.File.Exists(strLogo))
          {
            strLogo = "defaultVideoBig.png";
          }
          item.PinImage = Thumbs.TvConflictRecordingIcon;
          item.ThumbnailImage = strLogo;
          item.IconImageBig = strLogo;
          item.IconImage = strLogo;
          listConflicts.Add(item);
          total++;

          for (int i = 0; i < conflicts.Length; ++i)
          {
            conflicts[i].RecType = TVRecording.RecordingType.Once;
            item = new GUIListItem();
            item.Label = conflicts[i].Title;
            item.TVTag = conflicts[i];
            strLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, conflicts[i].Channel);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            item.PinImage = Thumbs.TvConflictRecordingIcon;
            item.ThumbnailImage = strLogo;
            item.IconImageBig = strLogo;
            item.IconImage = strLogo;
            listConflicts.Add(item);
            total++;
          }
        }

        //set object count label
        GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(total));

        SetLabels();
        GUIWaitCursor.Hide();

        return;
      }
      List<TVRecording> itemlist = new List<TVRecording>();
      TVDatabase.GetRecordings(ref itemlist);
      foreach (TVRecording rec in itemlist)
      {
        if (!ConflictManager.IsConflict(rec)) continue;
        GUIListItem item = new GUIListItem();
        item.Label = rec.Title;
        item.TVTag = rec;
        item.PinImage = Thumbs.TvConflictRecordingIcon;
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        listConflicts.Add(item);
        total++;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(total));

      SetLabels();
      GUIWaitCursor.Hide();
    }


    void SetLabels()
    {
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label.Equals("..")) continue;
        TVRecording rec = (TVRecording)item.TVTag;
        item.Label = rec.Title;
        string strTime = String.Format("{0} {1} - {2}",
          rec.StartTime.ToShortDateString(),
          rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
          rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        string strType = "";
        switch (rec.RecType)
        {
          case TVRecording.RecordingType.Once:
            item.Label2 = String.Format("{0} {1} - {2}",
              MediaPortal.Util.Utils.GetShortDayString(rec.StartTime),
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)); ;
            break;
          case TVRecording.RecordingType.Daily:
            strTime = String.Format("{0}-{1}",
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(648);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case TVRecording.RecordingType.WeekDays:
            strTime = String.Format("{0}-{1} {2}-{3}",
              GUILocalizeStrings.Get(657),//657=Mon
              GUILocalizeStrings.Get(661),//661=Fri
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(648);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case TVRecording.RecordingType.WeekEnds:
            strTime = String.Format("{0}-{1} {2}-{3}",
                GUILocalizeStrings.Get(662),//662=Sat
                GUILocalizeStrings.Get(663),//663=Sun
                rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(649);
            item.Label2 = String.Format("{0} {1}", strType, strTime);
            break;

          case TVRecording.RecordingType.Weekly:
            string day;
            switch (rec.StartTime.DayOfWeek)
            {
              case DayOfWeek.Monday: day = GUILocalizeStrings.Get(11); break;
              case DayOfWeek.Tuesday: day = GUILocalizeStrings.Get(12); break;
              case DayOfWeek.Wednesday: day = GUILocalizeStrings.Get(13); break;
              case DayOfWeek.Thursday: day = GUILocalizeStrings.Get(14); break;
              case DayOfWeek.Friday: day = GUILocalizeStrings.Get(15); break;
              case DayOfWeek.Saturday: day = GUILocalizeStrings.Get(16); break;
              default: day = GUILocalizeStrings.Get(17); break;
            }

            strTime = String.Format("{0}-{1}",
              rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            strType = GUILocalizeStrings.Get(649);
            item.Label2 = String.Format("{0} {1} {2}", strType, day, strTime);
            break;
          case TVRecording.RecordingType.EveryTimeOnThisChannel:
            item.Label = rec.Title;
            item.Label2 = GUILocalizeStrings.Get(650, new object[] { rec.Channel });
            break;
          case TVRecording.RecordingType.EveryTimeOnEveryChannel:
            item.Label = rec.Title;
            item.Label2 = GUILocalizeStrings.Get(651);
            break;
        }
      }
    }

    void OnClick(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem item = GetItem(iItem);
      if (item == null) return;
      TVRecording rec = item.TVTag as TVRecording;
      if (item.IsFolder)
      {
        if (item.Label == "..")
        {
          if (currentEpisode != null)
            currentEpisode = null;
          else
            currentShow = null;
          LoadDirectory();
          return;
        }
        if (currentShow == null)
          currentShow = rec;
        else
          currentEpisode = rec;
        LoadDirectory();
        return;
      }
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null != dlgYesNo)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
        dlgYesNo.SetLine(1, rec.Channel);
        dlgYesNo.SetLine(2, rec.Title);
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

        if (dlgYesNo.IsConfirmed)
        {
          if (rec.Series)
          {
            rec.CanceledSeries.Add(MediaPortal.Util.Utils.datetolong(rec.StartTime));
            TVDatabase.AddCanceledSerie(rec, MediaPortal.Util.Utils.datetolong(rec.StartTime));
          }
          else
          {
            TVDatabase.RemoveRecording(rec);
          }
          currentShow = null;
          LoadDirectory();
        }
      }

    }
    #endregion


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateDescription();
          break;
      }
      return base.OnMessage(message);
    }
    void UpdateDescription()
    {
      TVRecording rec = new TVRecording();
      rec.SetProperties(null);
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        return;
      }
      rec = pItem.TVTag as TVRecording;
      if (rec == null) return;

      TVProgram prog = TVDatabase.GetProgramByTime(rec.Channel, rec.StartTime.AddMinutes(1));
      rec.SetProperties(prog);
    }

    private void ConflictManager_OnConflictsUpdated()
    {
      needUpdate = true;
    }
    public override void Process()
    {
      if (needUpdate)
      {
        needUpdate = false;
        LoadDirectory();
      }
    }
  }
}
