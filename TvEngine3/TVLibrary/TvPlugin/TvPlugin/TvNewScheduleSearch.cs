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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;


using TvDatabase;
using TvControl;

using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin
{
  public class TvNewScheduleSearch : GUIWindow
  {
    #region enums
    public enum SearchType : int
    {
      Title = 0,
      KeyWord,
      Genres,
    };
    #endregion

    #region variables
    static SearchType _searchType = SearchType.Title;
    [SkinControlAttribute(50)]    protected GUIListControl listResults = null;
    [SkinControlAttribute(51)]    protected GUISMSInputControl smsInputControl = null;
    public string _searchKeyword = "";
    public bool _refreshList = false;

    private Action LastAction = null; // Keeps the Last received Action from the OnAction Methode
    private int LastActionTime = 0; // stores the time of the last action from the OnAction Methode
    #endregion

    public TvNewScheduleSearch()
    {
      Log.Info("newsearch ctor");
      GetID = (int)GUIWindow.Window.WINDOW_TV_SEARCH;
    }
    ~TvNewScheduleSearch()
    {
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public override bool Init()
    {
      Log.Info("newsearch init");
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvschedulerserverSearch.xml");

      Log.Info("newsearch init result:{0}", bResult);
      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (LastActionTime + 100 > System.Environment.TickCount && action == LastAction) return; // don't do anything if the keypress is comes to soon after the previos one and the action is the same as before.

      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
        case Action.ActionType.ACTION_SELECT_ITEM:
          {
            if (GetFocusControlId() == smsInputControl.GetID)
            {
              _refreshList = true;
              return;
            }
          }
          break;
      }

      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
      {
        // Check focus on sms input control
        if (GetFocusControlId() != smsInputControl.GetID)
        {
          // set focus to the default control then
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int)smsInputControl.GetID, 0, 0, null);
          OnMessage(msg);
        }
        smsInputControl.OnAction(action);
        return;
      }
      //else
      //{
      //  // translate all other actions from regular keypresses back to keypresses
      //  if (action.m_key != null && action.m_key.KeyChar >= 32)
      //  {
      //    action.wID = Action.ActionType.ACTION_KEY_PRESSED;
      //  }
      //}
      base.OnAction(action);
    }

    public static SearchType SearchFor
    {
      get
      {
        return _searchType;
      }
      set
      {
        _searchType = value;
      }
    }


    protected override void OnPageLoad()
    {
      Log.Info("newsearch OnPageLoad");
      smsInputControl.OnTextChanged += new GUISMSInputControl.OnTextChangedHandler(OnTextChanged);
      base.OnPageLoad();
    }
    protected override void OnPageDestroy(int new_windowId)
    {
      Log.Info("newsearch OnPageDestroy");
      smsInputControl.OnTextChanged -= new GUISMSInputControl.OnTextChangedHandler(OnTextChanged);
      base.OnPageDestroy(new_windowId);
    }
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listResults)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, listResults.GetID, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }
    GUIListItem GetItem(int index)
    {
      if (index < 0 || index >= listResults.Count) return null;
      return listResults[index];
    }
    void OnClick(int itemNo)
    {
      GUIListItem item = GetItem(itemNo);
      if (item == null) return;
      TVProgramInfo.CurrentProgram = item.TVTag as Program;
      if (TVProgramInfo.CurrentProgram != null)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
      return;
    }
    public void OnTextChanged()
    {
      Log.Info("newsearch OnTextChanged:{0}", smsInputControl.Text);
      if (_searchKeyword != smsInputControl.Text)
      {
        _searchKeyword = smsInputControl.Text;
        //_refreshList = true;
      }
    }
    public override void Process()
    {
      base.Process();
      if (_refreshList)
      {
        Search();
        _refreshList = false;
      }
      TVHome.UpdateProgressPercentageBar();
    }
    void Search()
    {
      Log.Info("newsearch Search:{0} {1}", _searchKeyword, SearchFor);
      GUIControl.ClearControl(GetID, listResults.GetID);
      TvBusinessLayer layer = new TvBusinessLayer();
      IList<Program> listPrograms = null;
      switch (SearchFor)
      {
        case SearchType.Genres:
          listPrograms = layer.SearchProgramsPerGenre("%" + _searchKeyword + "%", "");
          break;
        case SearchType.KeyWord:
          listPrograms = layer.SearchProgramsByDescription("%" + _searchKeyword);
          break;
        case SearchType.Title:
          listPrograms = layer.SearchPrograms("%" + _searchKeyword);
          break;
      }
      if (listPrograms == null) return;
      if (listPrograms.Count == 0) return;
      Log.Info("newsearch found:{0} progs", listPrograms.Count);
      foreach (Program program in listPrograms)
      {
        GUIListItem item = new GUIListItem();
        item.Label = program.Title;
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, program.ReferencedChannel().DisplayName);
        if (!System.IO.File.Exists(logo))
        {
          logo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = logo;
        item.IconImageBig = logo;
        item.IconImage = logo;
        item.TVTag = program;
        listResults.Add(item);
      }
    }
  }
}
