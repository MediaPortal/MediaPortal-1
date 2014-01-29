#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace WindowPlugins
{
  public abstract class WindowPluginBase : GUIInternalWindow
  {
    #region Base Variables

    protected Layout currentLayout = Layout.List;
    protected bool m_bSortAscending;
    protected ViewHandler handler;

    #endregion

    #region SkinControls

    [SkinControl(50)] protected GUIFacadeControl facadeLayout = null;
    [SkinControl(2)] protected GUIMenuButton btnLayouts = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIMenuButton btnViews = null;

    #endregion

    #region Serialisation

    protected virtual void LoadSettings() {}

    protected virtual void SaveSettings() {}

    #endregion

    protected int SelectedFacadeItem()
    {
      if (facadeLayout == null) return -1;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, facadeLayout.GetID, 0, 0,
                                      null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }

    protected virtual Layout GetLayoutNumber(string s)
    {
      switch (s.Trim().ToLowerInvariant())
      {
        case "list":
          return Layout.List;
        case "icons":
        case "smallicons":
          return Layout.SmallIcons;
        case "big icons":
        case "largeicons":
          return Layout.LargeIcons;
        case "albums":
        case "albumview":
          return Layout.AlbumView;
        case "filmstrip":
          return Layout.Filmstrip;
        case "playlist":
          return Layout.Playlist;
        case "coverflow":
        case "cover flow":
          return Layout.CoverFlow;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("{0}::GetLayoutNumber: Unknown String - {1}", "WindowPluginBase", s);
      }
      return Layout.List;
    }

    protected virtual bool AllowLayout(Layout layout)
    {
      return true;
    }

    protected virtual void SwitchLayout()
    {
      if (facadeLayout == null)
      {
        return;
      }

      // if skin has not implemented layout control or requested layout is not allowed
      // then default to list layout
      if (facadeLayout.IsNullLayout(CurrentLayout) || !AllowLayout(CurrentLayout))
      {
        facadeLayout.CurrentLayout = Layout.List;
      }
      else
      {
        facadeLayout.CurrentLayout = CurrentLayout;
      }

      PresentLayout();

      // The layout may be automatically switched via selection of a new view.
      // Here we need to ensure that the layout menu button reflects the proper state (this is redundant when the
      // layout button was used to change the layout).  Need to call facadeLayout to get the current layout since the
      // CurrentLayout getter is algorithmic.
      btnLayouts.SetSelectedItemByValue((int)facadeLayout.CurrentLayout);
    }

    public void PresentLayout()
    {
      GUIControl.HideControl(GetID, facadeLayout.GetID);
      int iControl = facadeLayout.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      InitLayoutSelections();

      // Some implementations of this class may not have a views button.
      if (btnViews != null)
      {
        InitViewSelections();
      }
    }

    protected virtual void InitLayoutSelections()
    {
      btnLayouts.ClearMenu();

      // Add the allowed layouts to choose from to the menu.
      int totalLayouts = Enum.GetValues(typeof (GUIFacadeControl.Layout)).Length;
      for (int i = 0; i < totalLayouts; i++)
      {
        string layoutName = Enum.GetName(typeof (GUIFacadeControl.Layout), i);
        GUIFacadeControl.Layout layout = GetLayoutNumber(layoutName);
        if (AllowLayout(layout))
        {
          if (!facadeLayout.IsNullLayout(layout))
          {
            btnLayouts.AddItem(GUIFacadeControl.GetLayoutLocalizedName(layout), (int)layout);
          }
        }
      }

      // Have the menu select the currently selected layout.
      btnLayouts.SetSelectedItemByValue((int)CurrentLayout);
    }

    protected virtual void InitViewSelections()
    {
      btnViews.ClearMenu();

      // Add the view options to the menu.
      int index = 0;
      btnViews.AddItem(GUILocalizeStrings.Get(134), index++); // Shares

      foreach (ViewDefinition view in handler.Views)
      {
        btnViews.AddItem(view.LocalizedName, index++);
      }

      // Have the menu select the currently selected view.
      if (this.GetID == (int)Window.WINDOW_VIDEOS || this.GetID == (int)Window.WINDOW_MUSIC_FILES)
      {
        btnViews.SetSelectedItemByValue(0);
      }
      else if (this.GetID == (int)Window.WINDOW_VIDEO_TITLE || this.GetID == (int)Window.WINDOW_MUSIC_GENRE)
      {
        btnViews.SetSelectedItemByValue(handler.CurrentViewIndex + 1);
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      bool msgHandled = false;

      // Depending on the mode, handle the GUI_MSG_ITEM_SELECT message from the dialog menu and
      // the GUI_MSG_CLICKED message from the spin control.
      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT ||
          message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
      {
        // Respond to the correct control.  The value is retrived directly from the control by the called handler.
        if (message.SenderControlId == btnLayouts.GetID)
        {
          // Set the new layout and select the currently selected item in the layout.
          SetLayout((Layout)btnLayouts.SelectedItemValue);
          SelectCurrentItem();

          // Refocus facade so item will be selected
          GUIControl.FocusControl(GetID, facadeLayout.GetID);

          msgHandled = true;
        }
        else if (btnViews != null && message.SenderControlId == btnViews.GetID)
        {
          // Set the new view.
          SetView(btnViews.SelectedItemValue);

          // Refocus facade so item will be selected
          GUIControl.FocusControl(GetID, facadeLayout.GetID);

          msgHandled = true;
        }
      }

      msgHandled = msgHandled | base.OnMessage(message);
      return msgHandled;
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnSortBy)
      {
        OnShowSort();
      }

      if (control == facadeLayout)
      {
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnInfo(SelectedFacadeItem());
          facadeLayout.RefreshCoverArt();
        }
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(SelectedFacadeItem());
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          OnQueueItem(SelectedFacadeItem());
        }
        if (actionType == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN ||
            actionType == Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
          OnMessage(msg);
        }
      }

      UpdateButtonStates();
    }

    protected virtual void OnInfo(int iItem) {}

    protected virtual void OnClick(int iItem) {}

    protected virtual void OnQueueItem(int item) {}

    protected virtual void OnSearchNew() {}

    protected virtual void SelectCurrentItem()
    {
      if (facadeLayout == null)
      {
        return;
      }
      int iItem = facadeLayout.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, iItem);
      }
    }

    protected virtual void UpdateButtonStates()
    {
      // skip all this if we're not active window
      if (GUIWindowManager.ActiveWindow != GetID)
      {
        return;
      }

      if (handler != null)
      {
        GUIPropertyManager.SetProperty("#itemtype", handler.LocalizedCurrentViewLevel);
        GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
      }
      if (btnSortBy != null)
      {
        btnSortBy.IsAscending = CurrentSortAsc;
      }
    }

    protected virtual void SetLayout(Layout layout)
    {
      // Set the selected layout.
      SwitchToNextAllowedLayout(layout);
    }

    protected virtual void SwitchToNextAllowedLayout(Layout selectedLayout)
    {
      int iSelectedLayout = (int)selectedLayout;
      int totalLayouts = Enum.GetValues(typeof (Layout)).Length - 1;

      if (iSelectedLayout > totalLayouts)
        iSelectedLayout = 0;

      bool shouldContinue = true;
      do
      {
        if (!AllowLayout(selectedLayout) || facadeLayout.IsNullLayout(selectedLayout))
        {
          iSelectedLayout++;
          if (iSelectedLayout > totalLayouts)
            iSelectedLayout = 0;
        }
        else
        {
          shouldContinue = false;
        }
      } while (shouldContinue);

      CurrentLayout = (Layout)iSelectedLayout;
      SwitchLayout();
    }

    protected virtual void SetView(int selectedViewId) {}

    protected virtual void OnShowSort() {}

    protected virtual void LoadDirectory(string path) {}

    protected virtual Layout CurrentLayout
    {
      get { return currentLayout; }
      set { currentLayout = value; }
    }

    protected virtual string SerializeName
    {
      get { return string.Empty; }
    }

    protected virtual bool CurrentSortAsc
    {
      get { return m_bSortAscending; }
      set { m_bSortAscending = value; }
    }
  }
}