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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using TvControl;
using TvDatabase;

namespace TvPlugin
{
  /// <summary>
  /// </summary>
  public class TvSearch : GUIInternalWindow, IComparer<GUIListItem>
  {
    [SkinControl(2)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(4)] protected GUIToggleButtonControl btnSearchByGenre = null;
    [SkinControl(5)] protected GUIToggleButtonControl btnSearchByTitle = null;
    [SkinControl(6)] protected GUIToggleButtonControl btnSearchByDescription = null;
    [SkinControl(7)] protected GUISelectButtonControl btnLetter = null;
    [SkinControl(19)] protected GUIButtonControl btnSMSInput = null;
    [SkinControl(8)] protected GUISelectButtonControl btnShow = null;
    [SkinControl(9)] protected GUISelectButtonControl btnEpisode = null;
    [SkinControl(10)] protected GUIListControl listView = null;
    [SkinControl(11)] protected GUIListControl titleView = null;
    [SkinControl(12)] protected GUILabelControl lblNumberOfItems = null;
    [SkinControl(13)] protected GUIFadeLabel lblProgramTitle = null;
    [SkinControl(14)] protected GUILabelControl lblProgramTime = null;
    [SkinControl(15)] protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControl(16)] protected GUILabelControl lblChannel = null;
    [SkinControl(17)] protected GUILabelControl lblProgramGenre = null;
    [SkinControl(18)] protected GUIImage imgTvLogo = null;

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
    private IList<Schedule> listRecordings;
    //		int        currentSearchKind=-1;

    private SearchMode currentSearchMode = SearchMode.Genre;
    private int currentLevel = 0;
    private string currentGenre = String.Empty;
    private string filterLetter = "A";
    private string filterShow = String.Empty;
    private string filterEpisode = String.Empty;


    private SearchMode prevcurrentSearchMode = SearchMode.Title;
    private int prevcurrentLevel = 0;
    private string prevcurrentGenre = String.Empty;
    private string prevfilterLetter = "a";
    private string prevfilterShow = String.Empty;
    private string prevfilterEpisode = String.Empty;

    public TvSearch()
    {
      GetID = (int) Window.WINDOW_SEARCHTV;
    }

    public override bool IsTv
    {
      get { return true; }
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

      /*switch (action.wID)
      {
      }*/
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      listRecordings.Clear();
      listRecordings = null;
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
      }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      //currentSearchKind=-1;
      listRecordings = Schedule.ListAll();

      btnShow.RestoreSelection = false;
      btnEpisode.RestoreSelection = false;
      if (btnLetter != null)
      {
        btnLetter.RestoreSelection = false;
      }

      btnShow.Clear();
      btnEpisode.Clear();
      if (btnLetter != null)
      {
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
        filterEpisode = String.Empty;
        filterLetter = "";
        filterShow = String.Empty;
        //currentSearchKind=-1;
        Update();
        GUIControl.FocusControl(GetID, btnSearchByGenre.GetID);
      }
      if (control == btnSearchByTitle)
      {
        btnShow.Clear();
        btnEpisode.Clear();
        filterEpisode = String.Empty;
        filterShow = String.Empty;
        currentSearchMode = SearchMode.Title;
        currentLevel = 0;
        //currentSearchKind=-1;
        filterLetter = "A";
        Update();
        GUIControl.FocusControl(GetID, btnSearchByTitle.GetID);
      }
      if (control == btnSearchByDescription)
      {
        btnShow.Clear();
        btnEpisode.Clear();
        filterEpisode = String.Empty;
        filterShow = String.Empty;
        currentSearchMode = SearchMode.Description;
        currentLevel = 0;
        //currentSearchKind=-1;
        Update();
        GUIControl.FocusControl(GetID, btnSearchByDescription.GetID);
      }

      if (control == btnSMSInput)
      {
        VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
        if (null == keyboard)
        {
          return;
        }
        String searchterm = string.Empty;
        keyboard.Reset();

        String tmpFilterLetter = filterLetter;
        if (tmpFilterLetter.StartsWith("%"))
        {
          tmpFilterLetter = tmpFilterLetter.Substring(1); // cut of leading % for display in dialog
        }
        keyboard.Text = tmpFilterLetter;
        keyboard.DoModal(GetID); // show it...

        if (keyboard.IsConfirmed)
        {
          filterLetter = "%" + keyboard.Text; // re-add % to perform fulltext search
          Update();
        }
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
      if (control == btnLetter)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        filterLetter = msg.Label;
        filterShow = String.Empty;
        filterEpisode = String.Empty;
        Update();
        GUIControl.FocusControl(GetID, btnLetter.GetID);
      }
      if (control == btnShow)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        filterShow = msg.Label;
        filterEpisode = String.Empty;
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

    private void Update()
    {
      SetHistory();
      int currentItemId = 0;
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
        if (btnSMSInput != null)
        {
          btnSMSInput.Disabled = true;
        }
        btnShow.Disabled = true;
        lblProgramDescription.IsVisible = false;
        if (lblProgramGenre != null)
        {
          lblProgramGenre.IsVisible = false;
        }
        lblProgramTime.IsVisible = false;
        lblProgramTitle.IsVisible = false;
        if (lblChannel != null)
        {
          lblChannel.IsVisible = false;
        }
        listView.Height = lblProgramDescription.YPosition - listView.YPosition;
        lblNumberOfItems.YPosition = listView.SpinY;
      }
      else
      {
        if (filterLetter != "#")
        {
          listView.IsVisible = false;
          titleView.IsVisible = true;
          GUIControl.FocusControl(GetID, titleView.GetID);

          if (filterShow == String.Empty)
          {
            lblProgramDescription.IsVisible = false;
            if (lblProgramGenre != null)
            {
              lblProgramGenre.IsVisible = false;
            }
            lblProgramTime.IsVisible = false;
            lblProgramTitle.IsVisible = false;
            if (lblChannel != null)
            {
              lblChannel.IsVisible = false;
            }
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
            if (lblChannel != null)
            {
              lblChannel.IsVisible = true;
            }
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
        }
        else
        {
          listView.IsVisible = true;
          titleView.IsVisible = false;
          GUIControl.FocusControl(GetID, listView.GetID);

          lblProgramDescription.IsVisible = false;
          if (lblProgramGenre != null)
          {
            lblProgramGenre.IsVisible = false;
          }
          lblProgramTime.IsVisible = false;
          lblProgramTitle.IsVisible = false;
          if (lblChannel != null)
          {
            lblChannel.IsVisible = false;
          }
          if (imgTvLogo != null)
          {
            imgTvLogo.IsVisible = false;
          }
        }
        btnEpisode.Disabled = false;
        if (btnLetter != null)
        {
          btnLetter.Disabled = false;
        }
        if (btnSMSInput != null)
        {
          btnSMSInput.Disabled = false;
        }
        btnShow.Disabled = false;
        lblNumberOfItems.YPosition = listView.SpinY;
      }

      List<Program> programs = new List<Program>();
      List<Program> episodes = new List<Program>();
      int itemCount = 0;
      switch (currentSearchMode)
      {
        case SearchMode.Genre:
          if (currentLevel == 0)
          {
            IList<string> genres;
            TvBusinessLayer layer = new TvBusinessLayer();
            genres = layer.GetGenres();
            foreach (string genre in genres)
            {
              GUIListItem item = new GUIListItem();
              item.IsFolder = true;
              item.Label = genre;
              item.Path = genre;
              item.ItemId = currentItemId;
              currentItemId++;
              Utils.SetDefaultIcons(item);
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
            item.Label2 = String.Empty;
            item.Path = String.Empty;
            item.IconImage = "defaultFolderBackBig.png";
            item.IconImageBig = "defaultFolderBackBig.png";
            listView.Add(item);
            titleView.Add(item);

            IList<Program> titles;
            TvBusinessLayer layer = new TvBusinessLayer();
            titles = layer.SearchProgramsPerGenre(currentGenre, filterShow);
            foreach (Program program in titles)
            {
              //dont show programs which have ended
              if (program.EndTime < DateTime.Now)
              {
                continue;
              }
              bool add = true;
              foreach (Program prog in programs)
              {
                if (prog.Title == program.Title)
                {
                  add = false;
                }
              }
              if (!add && filterShow == String.Empty)
              {
                continue;
              }
              if (add)
              {
                programs.Add(program);
              }

              if (filterShow != String.Empty)
              {
                if (program.Title == filterShow)
                {
                  episodes.Add(program);
                }
              }

              if (filterShow != String.Empty && program.Title != filterShow)
              {
                continue;
              }

              string strTime = String.Format("{0} {1}",
                                             Utils.GetShortDayString(program.StartTime),
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
              if (filterEpisode != String.Empty && strTime != filterEpisode)
              {
                continue;
              }

              strTime = String.Format("{0} {1} - {2}",
                                      Utils.GetShortDayString(program.StartTime),
                                      program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                      program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

              item = new GUIListItem();
              item.IsFolder = false;
              item.Label = program.Title;
              item.Label2 = strTime;
              item.Path = program.Title;
              item.MusicTag = program;
              item.ItemId = currentItemId;
              currentItemId++;
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
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program, ref item);
              listView.Add(item);
              titleView.Add(item);
              itemCount++;
            }
          }
          break;
        case SearchMode.Title:
          {
            if (filterShow != String.Empty)
            {
              GUIListItem item = new GUIListItem();
              item.IsFolder = true;
              item.Label = "..";
              item.Label2 = String.Empty;
              item.Path = String.Empty;
              item.IconImage = "defaultFolderBackBig.png";
              item.IconImageBig = "defaultFolderBackBig.png";
              listView.Add(item);
              titleView.Add(item);
            }
            IList<Program> titles = new List<Program>();
            TvBusinessLayer layer = new TvBusinessLayer();
            if (filterLetter == "#")
            {
              if (filterShow == String.Empty)
              {
                titles = layer.SearchPrograms("");
              }
              else
              {
                titles = layer.SearchPrograms("%"+filterShow);
              }
            }
            else
            {
              if (filterShow == String.Empty)
              {
                titles = layer.SearchPrograms(filterLetter);
              }
              else
              {
                titles = layer.SearchPrograms("%"+filterShow);
              }
            }
            foreach (Program program in titles)
            {
              if (filterLetter != "#")
              {
                bool add = true;
                foreach (Program prog in programs)
                {
                  if (prog.Title == program.Title)
                  {
                    add = false;
                  }
                }
                if (!add && filterShow == String.Empty)
                {
                  continue;
                }
                if (add)
                {
                  programs.Add(program);
                }

                if (filterShow != String.Empty)
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
                foreach (Program prog in programs)
                {
                  if (prog.Title == program.Title)
                  {
                    add = false;
                  }
                }
                if (!add && filterShow == String.Empty)
                {
                  continue;
                }
                if (add)
                {
                  programs.Add(program);
                }

                if (filterShow != String.Empty)
                {
                  if (program.Title == filterShow)
                  {
                    episodes.Add(program);
                  }
                }
              }
              if (filterShow != String.Empty && program.Title != filterShow)
              {
                continue;
              }

              string strTime = String.Format("{0} {1} - {2}",
                                             Utils.GetShortDayString(program.StartTime),
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                             program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

              GUIListItem item = new GUIListItem();
              item.IsFolder = false;
              item.Label = program.Title;
              if (program.StartTime > DateTime.MinValue)
              {
                item.Label2 = strTime;
              }
              item.Path = program.Title;
              item.MusicTag = program;
              item.ItemId = currentItemId;
              currentItemId++;
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
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program, ref item);
              listView.Add(item);
              titleView.Add(item);
              itemCount++;
            }
          }
          break;


        case SearchMode.Description:
          {
            IList<Program> titles = new List<Program>();
            long start = Utils.datetolong(DateTime.Now);
            long end = Utils.datetolong(DateTime.Now.AddMonths(1));
            TvBusinessLayer layer = new TvBusinessLayer();

            if (filterLetter == "#")
            {
              if (filterShow == String.Empty)
              {
                titles = layer.SearchProgramsByDescription("");
              }
              else
              {
                titles = layer.SearchProgramsByDescription(filterShow);
              }
            }
            else
            {
              if (filterShow == String.Empty)
              {
                titles = layer.SearchProgramsByDescription(filterLetter);
              }
              else
              {
                titles = layer.SearchProgramsByDescription(filterShow);
              }
            }
            foreach (Program program in titles)
            {
              if (program.Description.Length == 0)
              {
                continue;
              }
              if (filterLetter != "#")
              {
                bool add = true;
                foreach (Program prog in programs)
                {
                  if (prog.Title == program.Title)
                  {
                    add = false;
                  }
                }
                if (!add && filterShow == String.Empty)
                {
                  continue;
                }
                if (add)
                {
                  programs.Add(program);
                }

                if (filterShow != String.Empty)
                {
                  if (program.Title == filterShow)
                  {
                    episodes.Add(program);
                  }
                }
              }

              if (filterShow != String.Empty && program.Title != filterShow)
              {
                continue;
              }

              string strTime = String.Format("{0} {1}",
                                             Utils.GetShortDayString(program.StartTime),
                                             program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
              if (filterEpisode != String.Empty && strTime != filterEpisode)
              {
                continue;
              }

              strTime = String.Format("{0} {1} - {2}",
                                      Utils.GetShortDayString(program.StartTime),
                                      program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                      program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

              GUIListItem item = new GUIListItem();
              item.IsFolder = false;
              item.Label = program.Description;
              item.Label2 = strTime;
              item.Path = program.Title;
              item.MusicTag = program;
              item.ItemId = currentItemId;
              currentItemId++;
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
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program, ref item);
              listView.Add(item);
              titleView.Add(item);
              itemCount++;
            }
          }
          break;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Utils.GetObjectCountLabel(itemCount));

      btnShow.Clear();
      try
      {
        programs.Sort();
      }
      catch (Exception)
      {
      }
      int selItem = 0;
      int count = 0;
      foreach (Program prog in programs)
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
      foreach (Program prog in episodes)
      {
        string strTime = String.Format("{0} {1}",
                                       Utils.GetShortDayString(prog.StartTime),
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

      string strLine = String.Empty;
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


    private void SetHistory()
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
      {
        return;
      }
      string currentFolder = String.Format("{0}.{1}.{2}.{3}.{4}.{5}",
                                           (int) prevcurrentSearchMode, prevcurrentLevel, prevcurrentGenre,
                                           prevfilterLetter, prevfilterShow, prevfilterEpisode);
      prevcurrentSearchMode = currentSearchMode;
      prevcurrentLevel = currentLevel;
      prevcurrentGenre = currentGenre;
      prevfilterLetter = filterLetter;
      prevfilterShow = filterShow;
      prevfilterEpisode = filterEpisode;
      if (item.Label == "..")
      {
        return;
      }

      history.Set(item.ItemId.ToString(), currentFolder);
      //Log.Info("history.Set({0},{1}", item.ItemId.ToString(), currentFolder);
    }

    private void RestoreHistory()
    {
      string currentFolder = String.Format("{0}.{1}.{2}.{3}.{4}.{5}",
                                           (int) currentSearchMode, currentLevel, currentGenre,
                                           filterLetter, filterShow, filterEpisode);
      //Log.Info("history.Get({0})", currentFolder);
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
        //if (item.Label == selectedItemLabel)
        Log.Info(item.ItemId.ToString() + "==" + selectedItemLabel);
        if (item.ItemId.ToString() == selectedItemLabel)
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

      Program prog1 = item1.MusicTag as Program;
      Program prog2 = item2.MusicTag as Program;

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
              iComp = String.Compare(prog1.ReferencedChannel().DisplayName, prog2.ReferencedChannel().DisplayName, true);
            }
            else
            {
              iComp = String.Compare(prog2.ReferencedChannel().DisplayName, prog1.ReferencedChannel().DisplayName, true);
            }
            return iComp;
          }
          return 0;

        case SortMethod.Date:
          if (prog1 != null && prog2 != null)
          {
            if (sortAscending)
            {
              iComp = prog1.StartTime.CompareTo(prog2.StartTime);
            }
            else
            {
              iComp = prog2.StartTime.CompareTo(prog1.StartTime);
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
            filterShow = String.Empty;
            filterEpisode = String.Empty;
            currentGenre = item.Label;
            currentLevel++;
            Update();
          }
          else
          {
            if (item.IsFolder)
            {
              if (filterShow != String.Empty)
              {
                filterShow = String.Empty;
              }
              else
              {
                filterLetter = "#";
                filterShow = String.Empty;
                filterEpisode = String.Empty;
                currentGenre = String.Empty;
                currentLevel = 0;
              }
              Update();
            }
            else
            {
              Program program = item.MusicTag as Program;
              if (filterShow == String.Empty)
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
              filterShow = String.Empty;
              currentLevel = 0;
              Update();
              return;
            }
            Program program = item.MusicTag as Program;
            if (filterShow == String.Empty)
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
            Program program = item.MusicTag as Program;

            if (filterShow == String.Empty)
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

    private void SetChannelLogo(Program prog, ref GUIListItem item)
    {
      string strLogo = String.Empty;

      if (filterShow == String.Empty)
      {
        strLogo = Utils.GetCoverArt(Thumbs.TVShows, prog.Title);
      }

      if (!File.Exists(strLogo) || strLogo == String.Empty)
      {
        strLogo = Utils.GetCoverArt(Thumbs.TVChannel, prog.ReferencedChannel().DisplayName);
      }

      if (!File.Exists(strLogo))
      {
        strLogo = "defaultVideoBig.png";
      }

      item.ThumbnailImage = strLogo;
      item.IconImageBig = strLogo;
      item.IconImage = strLogo;
    }

    private void OnRecord(Program program)
    {
      WeekEndTool weekEndTool = Setting.GetWeekEndTool();
      TvServer server = new TvServer();
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
        dlg.Add(GUILocalizeStrings.Get(weekEndTool.GetText(DayType.Record_WorkingDays)));
        dlg.Add(GUILocalizeStrings.Get(weekEndTool.GetText(DayType.Record_WeekendDays)));

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        Schedule rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);

        TvBusinessLayer layer = new TvBusinessLayer();
        rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
        rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
        switch (dlg.SelectedLabel)
        {
          case 0: //none
            foreach (Schedule rec1 in listRecordings)
            {
              if (rec1.IsRecordingProgram(program, true))
              {
                if (rec1.ScheduleType != (int) ScheduleRecordingType.Once)
                {
                  //delete specific series
                  Schedule sched = Schedule.Retrieve(rec1.IdSchedule);
                  TVHome.PromptAndDeleteRecordingSchedule(sched.IdSchedule, sched.ReferencedChannel().CurrentProgram,
                                                          false, false);

                  //TVHome.DeleteRecordingSchedule(Schedule.Retrieve(rec1.IdSchedule));

                  /*
                  server.StopRecordingSchedule(rec1.IdSchedule);
                  CanceledSchedule schedule = new CanceledSchedule(rec1.IdSchedule, program.StartTime);
                  schedule.Persist();
                  server.OnNewSchedule();
                  */
                }
                else
                {
                  //cancel recording                                    
                  server.StopRecordingSchedule(rec1.IdSchedule);
                  rec1.Delete();
                  server.OnNewSchedule();
                }
              }
            }
            listRecordings = Schedule.ListAll();
            Update();
            return;
          case 1: //once
            rec.ScheduleType = (int) ScheduleRecordingType.Once;
            break;
          case 2: //everytime, this channel
            rec.ScheduleType = (int) ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 3: //everytime, all channels
            rec.ScheduleType = (int) ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 4: //weekly
            rec.ScheduleType = (int) ScheduleRecordingType.Weekly;
            break;
          case 5: //daily
            rec.ScheduleType = (int) ScheduleRecordingType.Daily;
            break;
          case 6: //WorkingDays
            rec.ScheduleType = (int) ScheduleRecordingType.WorkingDays;
            break;
          case 7: //Weekends
            rec.ScheduleType = (int) ScheduleRecordingType.Weekends;
            break;
        }
        rec.Persist();
        server.OnNewSchedule();
        listRecordings = Schedule.ListAll();
        Update();
      }
    }

    private bool IsRecording(Program program, out bool isSerie)
    {
      bool isRecording = false;
      isSerie = false;
      foreach (Schedule record in listRecordings)
      {
        if (record.IsRecordingProgram(program, true))
        {
          if (record.ScheduleType != (int) ScheduleRecordingType.Once)
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
      Program prog = null;
      if (item != null)
      {
        prog = item.MusicTag as Program;
      }
      if (prog == null)
      {
        GUIPropertyManager.SetProperty("#TV.Search.Title", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Genre", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Time", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Description", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.thumb", String.Empty);
        return;
      }

      string strTime = String.Format("{0} {1} - {2}",
                                     Utils.GetShortDayString(prog.StartTime),
                                     prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                     prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      GUIPropertyManager.SetProperty("#TV.Search.Title", prog.Title);
      GUIPropertyManager.SetProperty("#TV.Search.Time", strTime);
      lblProgramTime.Label = strTime;
      lblProgramTime.IsVisible = true;
      if (prog != null)
      {
        GUIPropertyManager.SetProperty("#TV.Search.Description", prog.Description);
        GUIPropertyManager.SetProperty("#TV.Search.Genre", prog.Genre);

        lblProgramDescription.Label = prog.Description;
        lblProgramDescription.IsVisible = true;
        lblProgramGenre.Label = prog.Genre;
        lblProgramGenre.IsVisible = true;
      }
      else
      {
        GUIPropertyManager.SetProperty("#TV.Search.Description", String.Empty);
        GUIPropertyManager.SetProperty("#TV.Search.Genre", String.Empty);

        lblProgramDescription.Label = String.Empty;
        lblProgramDescription.IsVisible = false;
        lblProgramGenre.Label = String.Empty;
        lblProgramGenre.IsVisible = false;
      }

      if (lblChannel != null)
      {
        lblChannel.IsVisible = true;
        lblChannel.Label = prog.ReferencedChannel().DisplayName;
      }

      string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, prog.ReferencedChannel().DisplayName);
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

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}