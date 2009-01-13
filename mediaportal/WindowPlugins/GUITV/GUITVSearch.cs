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
using System.IO;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// </summary>
  public class GUITVSearch : GUIWindow, IComparer<GUIListItem>
  {
    [SkinControl(2)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(4)] protected GUIToggleButtonControl btnSearchByGenre = null;
    [SkinControl(5)] protected GUIToggleButtonControl btnSearchByTitle = null;
    [SkinControl(6)] protected GUIToggleButtonControl btnSearchByDescription = null;
    [SkinControl(7)] protected GUISelectButtonControl btnLetter = null;
    [SkinControl(8)] protected GUISelectButtonControl btnShow = null;
    [SkinControl(9)] protected GUISelectButtonControl btnEpisode = null;
    [SkinControl(10)] protected GUIListControl listView = null;
    [SkinControl(11)] protected GUIListControl titleView = null;
    [SkinControl(12)] protected GUILabelControl lblNumberOfItems = null;
    [SkinControl(13)] protected GUIFadeLabel lblProgramTitle = null;
    [SkinControl(14)] protected GUILabelControl lblProgramTime = null;
    [SkinControl(15)] protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControl(17)] protected GUILabelControl lblProgramGenre = null;
    [SkinControl(18)] protected GUIImage imgTvLogo = null;
    [SkinControl(19)] protected GUIButtonControl btnSMSInput = null;


    private DirectoryHistory history = new DirectoryHistory();

    private enum SearchMode
    {
      Genre,
      Title,
      Description
    }

    private enum SortMethod
    {
      Name,
      Date,
      Channel
    }

    private SortMethod currentSortMethod = SortMethod.Name;
    private bool sortAscending = true;
    private List<TVRecording> listRecordings = new List<TVRecording>();
    //		int        currentSearchKind=-1;

    private SearchMode currentSearchMode = SearchMode.Title;
    private int currentLevel = 0;
    private string currentGenre = string.Empty;
    private string currentSearchCriteria = string.Empty;
    private string filterLetter = "#";
    private string filterShow = string.Empty;
    private string filterEpisode = string.Empty;


    private SearchMode prevcurrentSearchMode = SearchMode.Title;
    private int prevcurrentLevel = 0;
    private string prevcurrentGenre = string.Empty;
    private string prevcurrentSearchCriteria = string.Empty;
    private string prevfilterLetter = "a";
    private string prevfilterShow = string.Empty;
    private string prevfilterEpisode = string.Empty;

    public GUITVSearch()
    {
      GetID = (int) Window.WINDOW_SEARCHTV;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvsearch.xml");
      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (listView.Focus || titleView.Focus)
        {
          GUIListItem item = GetItem(0);
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              OnClick(0);
              return;
            }
          }
        }
      }

      //switch (action.wID)
      //{
      //}
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      listRecordings.Clear();
      listRecordings = null;

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

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      listRecordings = new List<TVRecording>();
      //currentSearchKind=-1;
      currentSearchCriteria = string.Empty;
      TVDatabase.GetRecordings(ref listRecordings);


      btnShow.RestoreSelection = false;
      btnEpisode.RestoreSelection = false;
      btnShow.Clear();
      btnEpisode.Clear();
      if (btnLetter != null)
      {
        btnLetter.RestoreSelection = false;
        btnLetter.AddSubItem("#");
        for (char k = 'A'; k <= 'Z'; k++)
        {
          btnLetter.AddSubItem(k.ToString());
        }
      }
      Update();

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnSearchByGenre)
      {
        btnShow.Clear();
        btnEpisode.Clear();
        currentSearchMode = SearchMode.Genre;
        currentLevel = 0;
        filterEpisode = string.Empty;
        filterLetter = "#";
        filterShow = string.Empty;
        //currentSearchKind=-1;
        currentSearchCriteria = string.Empty;
        Update();
        GUIControl.FocusControl(GetID, btnSearchByGenre.GetID);
      }
      if (control == btnSearchByTitle)
      {
        btnShow.Clear();
        btnEpisode.Clear();
        filterEpisode = string.Empty;
        filterLetter = "a";
        filterShow = string.Empty;
        currentSearchMode = SearchMode.Title;
        currentLevel = 0;
        //currentSearchKind=-1;
        currentSearchCriteria = string.Empty;
        Update();
        GUIControl.FocusControl(GetID, btnSearchByTitle.GetID);
      }
      if (control == btnSearchByDescription)
      {
        btnShow.Clear();
        btnEpisode.Clear();
        filterEpisode = string.Empty;
        filterLetter = "#";
        filterShow = string.Empty;
        currentSearchMode = SearchMode.Description;
        currentLevel = 0;
        //currentSearchKind=-1;
        currentSearchCriteria = string.Empty;
        Update();
        GUIControl.FocusControl(GetID, btnSearchByDescription.GetID);
      }

      if (control == btnSortBy)
      {
        switch (currentSortMethod)
        {
          case SortMethod.Name:
            currentSortMethod = SortMethod.Channel;
            break;
          case SortMethod.Channel:
            currentSortMethod = SortMethod.Date;
            break;
          case SortMethod.Date:
            currentSortMethod = SortMethod.Name;
            break;
        }
        Update();
        GUIControl.FocusControl(GetID, btnSortBy.GetID);
      }

      if (control == listView || control == titleView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int) msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
      }
      if ((btnLetter != null) && (control == btnLetter))
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        filterLetter = msg.Label;
        filterShow = string.Empty;
        filterEpisode = string.Empty;
        Update();
        GUIControl.FocusControl(GetID, btnLetter.GetID);
      }
      if (control == btnSMSInput)
      {
        VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
        if (null == keyboard)
        {
          return;
        }
        keyboard.Reset();
        keyboard.Text = filterLetter;
        keyboard.DoModal(GetID); // show it...
        if (keyboard.IsConfirmed)
        {
          filterLetter = keyboard.Text;
          Update();
        }
      }
      if (control == btnShow)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        filterShow = msg.Label;
        filterEpisode = string.Empty;
        Update();
        GUIControl.FocusControl(GetID, btnShow.GetID);
      }
      if (control == btnEpisode)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        filterEpisode = msg.Label;
        Update();
        GUIControl.FocusControl(GetID, btnEpisode.GetID);
      }
    }

/*    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateDescription();
          break;
      }
      return base.OnMessage(message);
    }
*/

    private void Update()
    {
      SetHistory();
      listView.Clear();
      titleView.Clear();
      if (currentLevel == 0 && currentSearchMode == SearchMode.Genre)
      {
        listView.IsVisible = true;
        titleView.IsVisible = false;
        GUIControl.FocusControl(GetID, listView.GetID);
        btnEpisode.Disabled = true;
        if (btnLetter != null)
        {
          btnLetter.Disabled = true;
        }
        btnShow.Disabled = true;
        lblProgramDescription.IsVisible = false;
        if (lblProgramGenre != null)
        {
          lblProgramGenre.IsVisible = false;
        }
        lblProgramTime.IsVisible = false;
        lblProgramTitle.IsVisible = false;
        listView.Height = lblProgramDescription.YPosition - listView.YPosition;
        lblNumberOfItems.YPosition = listView.SpinY;
      }
      else
      {
        listView.IsVisible = false;
        titleView.IsVisible = true;
        GUIControl.FocusControl(GetID, titleView.GetID);

        if (filterShow == string.Empty)
        {
          lblProgramDescription.IsVisible = false;
          if (lblProgramGenre != null)
          {
            lblProgramGenre.IsVisible = false;
          }
          lblProgramTime.IsVisible = false;
          lblProgramTitle.IsVisible = false;
          if (imgTvLogo != null)
          {
            imgTvLogo.IsVisible = false;
          }
          if (titleView.SubItemCount == 2)
          {
            string subItem = (string) titleView.GetSubItem(1);
            int h = Int32.Parse(subItem.Substring(1));
            GUIGraphicsContext.ScaleVertical(ref h);
            titleView.Height = h;
            h = Int32.Parse(subItem.Substring(1));
            h -= 55;
            GUIGraphicsContext.ScaleVertical(ref h);
            titleView.SpinY = titleView.YPosition + h;
            titleView.FreeResources();
            titleView.AllocResources();
          }
        }
        else
        {
          lblProgramDescription.IsVisible = true;
          if (lblProgramGenre != null)
          {
            lblProgramGenre.IsVisible = true;
          }
          lblProgramTime.IsVisible = true;
          lblProgramTitle.IsVisible = true;
          if (imgTvLogo != null)
          {
            imgTvLogo.IsVisible = true;
          }
          if (titleView.SubItemCount == 2)
          {
            string subItem = (string) titleView.GetSubItem(0);
            int h = Int32.Parse(subItem.Substring(1));
            GUIGraphicsContext.ScaleVertical(ref h);
            titleView.Height = h;

            h = Int32.Parse(subItem.Substring(1));
            h -= 50;
            GUIGraphicsContext.ScaleVertical(ref h);
            titleView.SpinY = titleView.YPosition + h;

            titleView.FreeResources();
            titleView.AllocResources();
          }
          lblNumberOfItems.YPosition = titleView.SpinY;
        }
        btnEpisode.Disabled = false;
        if (btnLetter != null)
        {
          btnLetter.Disabled = false;
        }
        btnShow.Disabled = false;
        lblNumberOfItems.YPosition = listView.SpinY;
      }

      List<TVProgram> programs = new List<TVProgram>();
      List<TVProgram> episodes = new List<TVProgram>();
      int itemCount = 0;
      switch (currentSearchMode)
      {
        case SearchMode.Genre:
          if (currentLevel == 0)
          {
            List<string> genres = new List<string>();
            TVDatabase.GetGenres(ref genres);
            foreach (string genre in genres)
            {
              GUIListItem item = new GUIListItem();
              item.IsFolder = true;
              item.Label = genre;
              item.Path = genre;
              Util.Utils.SetDefaultIcons(item);
              listView.Add(item);
              itemCount++;
            }
          }
          else
          {
            listView.Clear();
            titleView.Clear();
            GUIListItem item = new GUIListItem();
            item.IsFolder = true;
            item.Label = "..";
            item.Label2 = string.Empty;
            item.Path = string.Empty;
            item.IconImage = "defaultFolderBackBig.png";
            item.IconImageBig = "defaultFolderBackBig.png";
            listView.Add(item);
            titleView.Add(item);

            List<TVProgram> titles = new List<TVProgram>();
            TVDatabase.SearchProgramsPerGenre(currentGenre, titles, -1, currentSearchCriteria);
            foreach (TVProgram program in titles)
            {
              //dont show programs which have ended
              if (program.EndTime < DateTime.Now)
              {
                continue;
              }
              if (filterLetter != "#")
              {
                if (currentSearchMode == SearchMode.Description)
                {
                  if (!program.Description.ToLower().StartsWith(filterLetter.ToLower()))
                  {
                    continue;
                  }
                }
                else if (currentSearchMode == SearchMode.Title)
                {
                  if (!program.Title.ToLower().StartsWith(filterLetter.ToLower()))
                  {
                    continue;
                  }
                }
              }
              bool add = true;
              foreach (TVProgram prog in programs)
              {
                if (prog.Title == program.Title)
                {
                  add = false;
                }
              }
              if (!add && filterShow == string.Empty)
              {
                continue;
              }
              if (add)
              {
                programs.Add(program);
              }

              if (filterShow != string.Empty)
              {
                if (program.Title == filterShow)
                {
                  episodes.Add(program);
                }
              }

              if (filterShow != string.Empty && program.Title != filterShow)
              {
                continue;
              }

              string strTime = String.Format("{0} {1}",
                                             Util.Utils.GetShortDayString(program.StartTime),
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
              if (filterEpisode != string.Empty && strTime != filterEpisode)
              {
                continue;
              }

              strTime = String.Format("{0} {1} - {2}",
                                      Util.Utils.GetShortDayString(program.StartTime),
                                      program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                      program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = program.Title;
              item.Label2 = strTime;
              item.Path = program.Title;
              item.MusicTag = program;
              item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
              bool isSerie;
              if (IsRecording(program, out isSerie))
              {
                if (isSerie)
                {
                  item.PinImage = Thumbs.TvRecordingSeriesIcon;
                }
                else
                {
                  item.PinImage = Thumbs.TvRecordingIcon;
                }
              }
              Util.Utils.SetDefaultIcons(item);
              SetChannelLogo(program, ref item);
              listView.Add(item);
              titleView.Add(item);
              itemCount++;
            }
          }
          break;
        case SearchMode.Title:
          {
            if (filterShow != string.Empty)
            {
              GUIListItem item = new GUIListItem();
              item.IsFolder = true;
              item.Label = "..";
              item.Label2 = string.Empty;
              item.Path = string.Empty;
              item.IconImage = "defaultFolderBackBig.png";
              item.IconImageBig = "defaultFolderBackBig.png";
              listView.Add(item);
              titleView.Add(item);
            }
            List<TVProgram> titles = new List<TVProgram>();
            long start = Util.Utils.datetolong(DateTime.Now);
            long end = Util.Utils.datetolong(DateTime.Now.AddMonths(1));
            if (filterLetter == "#")
            {
              if (filterShow == string.Empty)
              {
                TVDatabase.GetProgramTitles(start, end, ref titles);
              }
              else
              {
                TVDatabase.SearchPrograms(start, end, ref titles, 3, filterShow, string.Empty);
              }
            }
            else
            {
              if (filterShow == string.Empty)
              {
                TVDatabase.SearchMinimalPrograms(start, end, ref titles, 0, filterLetter, string.Empty);
              }
              else
              {
                TVDatabase.SearchPrograms(start, end, ref titles, 3, filterShow, string.Empty);
              }
            }
            foreach (TVProgram program in titles)
            {
              if (filterLetter != "#")
              {
                if (currentSearchMode == SearchMode.Description)
                {
                  if (!program.Description.ToLower().StartsWith(filterLetter.ToLower()))
                  {
                    continue;
                  }
                }
                else if (currentSearchMode == SearchMode.Title)
                {
                  if (!program.Title.ToLower().StartsWith(filterLetter.ToLower()))
                  {
                    continue;
                  }
                }

                bool add = true;
                foreach (TVProgram prog in programs)
                {
                  if (prog.Title == program.Title)
                  {
                    add = false;
                  }
                }
                if (!add && filterShow == string.Empty)
                {
                  continue;
                }
                if (add)
                {
                  programs.Add(program);
                }

                if (filterShow != string.Empty)
                {
                  if (program.Title == filterShow)
                  {
                    episodes.Add(program);
                  }
                }
              } //if (filterLetter!="#")
              else
              {
                bool add = true;
                foreach (TVProgram prog in programs)
                {
                  if (prog.Title == program.Title)
                  {
                    add = false;
                  }
                }
                if (!add && filterShow == string.Empty)
                {
                  continue;
                }
                if (add)
                {
                  programs.Add(program);
                }

                if (filterShow != string.Empty)
                {
                  if (program.Title == filterShow)
                  {
                    episodes.Add(program);
                  }
                }
              }
              if (filterShow != string.Empty && program.Title != filterShow)
              {
                continue;
              }

              string strTime = String.Format("{0} {1} - {2}",
                                             Util.Utils.GetShortDayString(program.StartTime),
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                             program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

              GUIListItem item = new GUIListItem();
              item.IsFolder = false;
              item.Label = program.Title;
              if (program.Start > 0)
              {
                item.Label2 = strTime;
              }
              item.Path = program.Title;
              item.MusicTag = program;
              item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
              bool isSerie;
              if (IsRecording(program, out isSerie))
              {
                if (isSerie)
                {
                  item.PinImage = Thumbs.TvRecordingSeriesIcon;
                }
                else
                {
                  item.PinImage = Thumbs.TvRecordingIcon;
                }
              }
              Util.Utils.SetDefaultIcons(item);
              SetChannelLogo(program, ref item);
              listView.Add(item);
              titleView.Add(item);
              itemCount++;
            }
          }
          break;


        case SearchMode.Description:
          {
            List<TVProgram> titles = new List<TVProgram>();
            long start = Util.Utils.datetolong(DateTime.Now);
            long end = Util.Utils.datetolong(DateTime.Now.AddMonths(1));
            TVDatabase.SearchProgramsByDescription(start, end, ref titles, -1, currentSearchCriteria);
            foreach (TVProgram program in titles)
            {
              if (filterLetter != "#")
              {
                if (currentSearchMode == SearchMode.Description)
                {
                  if (!program.Description.ToLower().StartsWith(filterLetter.ToLower()))
                  {
                    continue;
                  }
                }
                else if (currentSearchMode == SearchMode.Title)
                {
                  if (!program.Title.ToLower().StartsWith(filterLetter.ToLower()))
                  {
                    continue;
                  }
                }
                bool add = true;
                foreach (TVProgram prog in programs)
                {
                  if (prog.Title == program.Title)
                  {
                    add = false;
                  }
                }
                if (!add && filterShow == string.Empty)
                {
                  continue;
                }
                if (add)
                {
                  programs.Add(program);
                }

                if (filterShow != string.Empty)
                {
                  if (program.Title == filterShow)
                  {
                    episodes.Add(program);
                  }
                }
              }

              if (filterShow != string.Empty && program.Title != filterShow)
              {
                continue;
              }

              string strTime = String.Format("{0} {1}",
                                             Util.Utils.GetShortDayString(program.StartTime),
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
              if (filterEpisode != string.Empty && strTime != filterEpisode)
              {
                continue;
              }

              strTime = String.Format("{0} {1} - {2}",
                                      Util.Utils.GetShortDayString(program.StartTime),
                                      program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                      program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

              GUIListItem item = new GUIListItem();
              item.IsFolder = false;
              item.Label = program.Description;
              item.Label2 = strTime;
              item.Path = program.Title;
              item.MusicTag = program;
              item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
              bool isSerie;
              if (IsRecording(program, out isSerie))
              {
                if (isSerie)
                {
                  item.PinImage = Thumbs.TvRecordingSeriesIcon;
                }
                else
                {
                  item.PinImage = Thumbs.TvRecordingIcon;
                }
              }
              Util.Utils.SetDefaultIcons(item);
              SetChannelLogo(program, ref item);
              listView.Add(item);
              titleView.Add(item);
              itemCount++;
            }
          }
          break;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(itemCount));

      btnShow.Clear();
      programs.Sort();
      int selItem = 0;
      int count = 0;
      foreach (TVProgram prog in programs)
      {
        btnShow.Add(prog.Title.ToString());
        if (filterShow == prog.Title)
        {
          selItem = count;
        }
        count++;
      }
      GUIControl.SelectItemControl(GetID, btnShow.GetID, selItem);

      selItem = 0;
      count = 0;
      btnEpisode.Clear();
      foreach (TVProgram prog in episodes)
      {
        string strTime = String.Format("{0} {1}",
                                       Util.Utils.GetShortDayString(prog.StartTime),
                                       prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
        btnEpisode.Add(strTime.ToString());
        if (filterEpisode == strTime)
        {
          selItem = count;
        }
        count++;
      }
      GUIControl.SelectItemControl(GetID, btnEpisode.GetID, selItem);
      OnSort();

      string strLine = string.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(622);
          break;
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620);
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621);
          break;
      }
      btnSortBy.Label = strLine;
      btnSortBy.IsAscending = sortAscending;

      UpdateButtonStates();
      RestoreHistory();
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      UpdateDescription();
    }


    private void SetHistory()
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
      {
        return;
      }
      string currentFolder = String.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}",
                                           (int) prevcurrentSearchMode, prevcurrentLevel, prevcurrentGenre,
                                           prevcurrentSearchCriteria, prevfilterLetter, prevfilterShow,
                                           prevfilterEpisode);
      prevcurrentSearchMode = currentSearchMode;
      prevcurrentLevel = currentLevel;
      prevcurrentGenre = currentGenre;
      prevcurrentSearchCriteria = currentSearchCriteria;
      prevfilterLetter = filterLetter;
      prevfilterShow = filterShow;
      prevfilterEpisode = filterEpisode;
      if (item.Label == "..")
      {
        return;
      }

      history.Set(item.Label, currentFolder);
    }

    private void RestoreHistory()
    {
      string currentFolder = String.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}",
                                           (int) currentSearchMode, currentLevel, currentGenre,
                                           currentSearchCriteria, filterLetter, filterShow, filterEpisode);
      string selectedItemLabel = history.Get(currentFolder);
      if (selectedItemLabel == null)
      {
        return;
      }
      if (selectedItemLabel.Length == 0)
      {
        return;
      }
      for (int i = 0; i < listView.Count; ++i)
      {
        GUIListItem item = listView[i];
        if (item.Label == selectedItemLabel)
        {
          listView.SelectedListItemIndex = i;
          titleView.SelectedListItemIndex = i;
          break;
        }
      }
    }

    #region Sort Members

    private void OnSort()
    {
      listView.Sort(this);
      titleView.Sort(this);
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
        return 1;
      }

      TVProgram prog1 = item1.MusicTag as TVProgram;
      TVProgram prog2 = item2.MusicTag as TVProgram;

      int iComp = 0;
      switch (currentSortMethod)
      {
        case SortMethod.Name:
          if (sortAscending)
          {
            iComp = String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            iComp = String.Compare(item2.Label, item1.Label, true);
          }
          return iComp;

        case SortMethod.Channel:
          if (prog1 != null && prog2 != null)
          {
            if (sortAscending)
            {
              iComp = String.Compare(prog1.Channel, prog2.Channel, true);
            }
            else
            {
              iComp = String.Compare(prog2.Channel, prog1.Channel, true);
            }
            return iComp;
          }
          return 0;

        case SortMethod.Date:
          if (prog1 != null && prog2 != null)
          {
            if (sortAscending)
            {
              iComp = (int) (prog1.Start - prog2.Start);
            }
            else
            {
              iComp = (int) (prog2.Start - prog1.Start);
            }
            return iComp;
          }
          return 0;
      }
      return iComp;
    }

    #endregion

    private GUIListItem GetItem(int iItem)
    {
      if (currentLevel != 0)
      {
        if (iItem >= titleView.Count || iItem < 0)
        {
          return null;
        }
        return titleView[iItem];
      }
      if (iItem >= listView.Count || iItem < 0)
      {
        return null;
      }
      return listView[iItem];
    }

    private void OnClick(int iItem)
    {
      GUIListItem item = GetItem(iItem);
      if (item == null)
      {
        return;
      }
      switch (currentSearchMode)
      {
        case SearchMode.Genre:
          if (currentLevel == 0)
          {
            filterLetter = "#";
            filterShow = string.Empty;
            filterEpisode = string.Empty;
            currentGenre = item.Label;
            currentLevel++;
            Update();
          }
          else
          {
            if (item.IsFolder)
            {
              if (filterShow != string.Empty)
              {
                filterShow = string.Empty;
              }
              else
              {
                filterLetter = "#";
                filterShow = string.Empty;
                filterEpisode = string.Empty;
                currentGenre = string.Empty;
                currentLevel = 0;
              }
              Update();
            }
            else
            {
              TVProgram program = item.MusicTag as TVProgram;
              if (filterShow == string.Empty)
              {
                filterShow = program.Title;
                Update();
                return;
              }
              OnRecord(program);
            }
          }
          break;
        case SearchMode.Title:
          {
            if (item.Label == ".." && item.IsFolder)
            {
              filterShow = string.Empty;
              currentLevel = 0;
              Update();
              return;
            }
            TVProgram program = item.MusicTag as TVProgram;
            if (filterShow == string.Empty)
            {
              filterShow = program.Title;
              Update();
              return;
            }
            OnRecord(program);
          }
          break;
        case SearchMode.Description:
          {
            TVProgram program = item.MusicTag as TVProgram;

            if (filterShow == string.Empty)
            {
              filterShow = program.Title;
              Update();
              return;
            }
            OnRecord(program);
          }
          break;
      }
    }

    private void SetChannelLogo(TVProgram prog, ref GUIListItem item)
    {
      string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, prog.Channel);
      if (!File.Exists(strLogo))
      {
        strLogo = "defaultVideoBig.png";
      }
      if (filterShow == string.Empty)
      {
        strLogo = Util.Utils.GetCoverArt(Thumbs.TVShows, prog.Title);
        if (!File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
      }


      item.ThumbnailImage = strLogo;
      item.IconImageBig = strLogo;
      item.IconImage = strLogo;
    }

    private void OnRecord(TVProgram program)
    {
      if (program == null)
      {
        return;
      }
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616)); //616=Select Recording type

        //610=None
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i = 610; i <= 615; ++i)
        {
          dlg.Add(GUILocalizeStrings.Get(i));
        }
        dlg.Add(GUILocalizeStrings.Get(672)); // 672=Record Mon-Fri
        dlg.Add(GUILocalizeStrings.Get(1051)); // 1051=Record Sat-Sun

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        TVRecording rec = new TVRecording();
        rec.Title = program.Title;
        rec.Channel = program.Channel;
        rec.Start = program.Start;
        rec.End = program.End;

        switch (dlg.SelectedLabel)
        {
          case 0: //none
            foreach (TVRecording rec1 in listRecordings)
            {
              if (rec1.IsRecordingProgram(program, true))
              {
                if (rec1.RecType != TVRecording.RecordingType.Once)
                {
                  //delete specific series
                  rec1.CanceledSeries.Add(program.Start);
                  TVDatabase.AddCanceledSerie(rec1, program.Start);
                }
                else
                {
                  //cancel recording
                  rec1.Canceled = Util.Utils.datetolong(DateTime.Now);
                  TVDatabase.UpdateRecording(rec1, TVDatabase.RecordingChange.Canceled);
                }
                Recorder.StopRecording(rec1);
              }
            }
            listRecordings.Clear();
            TVDatabase.GetRecordings(ref listRecordings);
            Update();
            return;
          case 1: //once
            rec.RecType = TVRecording.RecordingType.Once;
            break;
          case 2: //everytime, this channel
            rec.RecType = TVRecording.RecordingType.EveryTimeOnThisChannel;
            break;
          case 3: //everytime, all channels
            rec.RecType = TVRecording.RecordingType.EveryTimeOnEveryChannel;
            break;
          case 4: //weekly
            rec.RecType = TVRecording.RecordingType.Weekly;
            break;
          case 5: //daily
            rec.RecType = TVRecording.RecordingType.Daily;
            break;
          case 6: //Mo-Fi
            rec.RecType = TVRecording.RecordingType.WeekDays;
            break;
          case 7: //Sat-Sun
            rec.RecType = TVRecording.RecordingType.WeekEnds;
            break;
        }
        Recorder.AddRecording(ref rec);
        listRecordings.Clear();
        TVDatabase.GetRecordings(ref listRecordings);
        Update();
      }
    }

    private bool IsRecording(TVProgram program, out bool isSerie)
    {
      bool isRecording = false;
      isSerie = false;
      foreach (TVRecording record in listRecordings)
      {
        if (record.IsRecordingProgram(program, true))
        {
          if (record.RecType != TVRecording.RecordingType.Once)
          {
            isSerie = true;
          }
          isRecording = true;
          break;
        }
      }
      return isRecording;
    }


    private GUIListItem GetSelectedItem()
    {
      if (titleView.Focus)
      {
        return titleView.SelectedListItem;
      }
      if (listView.Focus)
      {
        return listView.SelectedListItem;
      }
      return null;
    }

    private void UpdateDescription()
    {
      if (currentLevel == 0 && currentSearchMode == SearchMode.Genre)
      {
        return;
      }
      GUIListItem item = GetSelectedItem();
      TVProgram prog = null;
      if (item != null)
      {
        prog = item.MusicTag as TVProgram;
      }
      if (prog == null)
      {
        GUIPropertyManager.SetProperty("#TV.Search.Title", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Genre", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Time", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Description", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.thumb", string.Empty);
        return;
      }

      string strTime = String.Format("{0} {1} - {2}",
                                     Util.Utils.GetShortDayString(prog.StartTime),
                                     prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                     prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.Search.Title", prog.Title);
      GUIPropertyManager.SetProperty("#TV.Search.Time", strTime);
      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Search.Description", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Search.Genre", prog.Genre);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Search.Description", string.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Genre", string.Empty);
      }


      string strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, prog.Channel);
      if (File.Exists(strLogo))
      {
        GUIPropertyManager.SetProperty("#TV.Search.thumb", strLogo);
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Search.thumb", "defaultVideoBig.png");
      }
    }

    private void UpdateButtonStates()
    {
      btnSearchByDescription.Selected = false;
      btnSearchByTitle.Selected = false;
      btnSearchByGenre.Selected = false;

      if (currentSearchMode == SearchMode.Title)
      {
        btnSearchByTitle.Selected = true;
      }
      if (currentSearchMode == SearchMode.Description)
      {
        btnSearchByDescription.Selected = true;
      }
      if (currentSearchMode == SearchMode.Genre)
      {
        btnSearchByGenre.Selected = true;
      }
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      sortAscending = e.Order != SortOrder.Descending;

      Update();
      GUIControl.FocusControl(GetID, ((GUIControl) sender).GetID);
    }
  }
}