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
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
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
    [SkinControl(2)] protected GUIButtonControl btnLayouts = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnViews = null;

    #endregion

    #region Serialisation

    protected virtual void LoadSettings() { }

    protected virtual void SaveSettings() { }

    #endregion

    protected int SelectedFacadeItem()
    {
      if (facadeLayout == null) return -1;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, facadeLayout.GetID, 0, 0, null);
      OnMessage(msg);
      int iItem = (int)msg.Param1;
      return iItem;
    }

    protected virtual Layout GetLayoutNumber(string s)
    {
      switch (s.Trim().ToLower())
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
      
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnLayouts)
      {
        OnShowLayouts();
        SelectCurrentItem();
        UpdateButtonStates();
        GUIControl.FocusControl(GetID, controlId);
      }

      if (control == btnSortBy)
      {
        OnShowSort();
      }

      if (control == btnViews)
      {
        OnShowViews();
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
      }
    }

    protected virtual void OnShowLayouts()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(792); // Layouts menu
      int dlgItems = 0;
      int totalLayouts = Enum.GetValues(typeof (GUIFacadeControl.Layout)).Length;
      bool[] allowedLayouts = new bool[totalLayouts];
      for (int i = 0; i < totalLayouts; i++)
      {
        string layoutName = Enum.GetName(typeof (GUIFacadeControl.Layout), i);
        GUIFacadeControl.Layout layout = GetLayoutNumber(layoutName);
        if (AllowLayout(layout))
        {
          if (!facadeLayout.IsNullLayout(layout))
          {
            dlg.Add(GUIFacadeControl.GetLayoutLocalizedName(layout));
            dlgItems++;
            allowedLayouts[i] = true;
          }
        }
      }
      dlg.SelectedLabel = -1;
      for (int i = 0; i <= (int)CurrentLayout; i++)
      {
        if (allowedLayouts[i])
        {
          dlg.SelectedLabel++;
        }
      }
      if (dlg.SelectedLabel >= dlgItems)
      {
        dlg.SelectedLabel = dlgItems;
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }
      int iSelectedLayout = dlg.SelectedLabel;
      int allowedItemsFound = -1;
      for (int i = 0; i < allowedLayouts.Length; i++)
      {
        if (allowedLayouts[i])
        {
          iSelectedLayout = i;
          allowedItemsFound++;
          if (allowedItemsFound == dlg.SelectedLabel)
            break;
        }
      }
      CurrentLayout = (Layout)iSelectedLayout;
      SwitchLayout();

      UpdateButtonStates();
    }

    protected virtual void OnInfo(int iItem) {}

    protected virtual void OnClick(int iItem) {}

    protected virtual void OnQueueItem(int item) {}

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
        GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
        GUIPropertyManager.SetProperty("#itemtype", handler.LocalizedCurrentViewLevel);
      }

      if (facadeLayout == null)
      {
        return;
      }

      GUIControl.HideControl(GetID, facadeLayout.GetID);
      int iControl = facadeLayout.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);


      string strLine = string.Empty;
      Layout layout = CurrentLayout;
      switch (layout)
      {
        case Layout.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case Layout.SmallIcons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case Layout.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
        case Layout.AlbumView:
          strLine = GUILocalizeStrings.Get(529);
          break;
        case Layout.Filmstrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
        case Layout.Playlist:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case Layout.CoverFlow:
          strLine = GUILocalizeStrings.Get(791);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnLayouts.GetID, strLine);

      if (btnSortBy != null)
      {
        btnSortBy.IsAscending = CurrentSortAsc;
      }
    }

    protected virtual void OnShowSort() {}

    protected virtual void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(499); // Views menu

      dlg.AddLocalizedString(134); // Shares
      foreach (ViewDefinition view in handler.Views)
      {
        dlg.Add(view.LocalizedName);
      }
      bool isVideoWindow = (this.GetID == (int)Window.WINDOW_VIDEOS || this.GetID == (int)Window.WINDOW_VIDEO_TITLE);
      // set the focus to currently used view
      if (this.GetID == (int)Window.WINDOW_VIDEOS || this.GetID == (int)Window.WINDOW_MUSIC_FILES)
      {
        dlg.SelectedLabel = 0;
      }
      else if (this.GetID == (int)Window.WINDOW_VIDEO_TITLE || this.GetID == (int)Window.WINDOW_MUSIC_GENRE)
      {
        dlg.SelectedLabel = handler.CurrentViewIndex + 1;
      }

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 134: // Shares
          {
            int nNewWindow;
            if (isVideoWindow)
            {
              nNewWindow = (int)Window.WINDOW_VIDEOS;
            }
            else
            {
              nNewWindow = (int)Window.WINDOW_MUSIC_FILES;
            }
            StateBase.StartWindow = nNewWindow;
            if (nNewWindow != GetID)
            {
              if (isVideoWindow)
              {
                MediaPortal.GUI.Video.GUIVideoFiles.Reset();
              }
              GUIWindowManager.ReplaceWindow(nNewWindow);
            }
          }
          break;

        case 4540: // Now playing
          {
            int nPlayingNowWindow = (int)Window.WINDOW_MUSIC_PLAYING_NOW;

            MediaPortal.GUI.Music.GUIMusicPlayingNow guiPlayingNow =
              (MediaPortal.GUI.Music.GUIMusicPlayingNow)GUIWindowManager.GetWindow(nPlayingNowWindow);

            if (guiPlayingNow != null)
            {
              guiPlayingNow.MusicWindow = (MediaPortal.GUI.Music.GUIMusicBaseWindow)this;
              GUIWindowManager.ActivateWindow(nPlayingNowWindow);
            }
          }
          break;

        default: // a db view
          {
            ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel - 1];
            handler.CurrentView = selectedView.Name;
            StateBase.View = selectedView.Name;
            int nNewWindow;
            if (isVideoWindow)
            {
              nNewWindow = (int)Window.WINDOW_VIDEO_TITLE;
            }
            else
            {
              nNewWindow = (int)Window.WINDOW_MUSIC_GENRE;
            }
            if (GetID != nNewWindow)
            {
              StateBase.StartWindow = nNewWindow;
              if (nNewWindow != GetID)
              {
                GUIWindowManager.ReplaceWindow(nNewWindow);
              }
            }
            else
            {
              LoadDirectory(string.Empty);
              if (facadeLayout.Count <= 0)
              {
                GUIControl.FocusControl(GetID, btnLayouts.GetID);
              }
            }
          }
          break;
      }
    }

    protected virtual void LoadDirectory(string path) {}

    protected virtual bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
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