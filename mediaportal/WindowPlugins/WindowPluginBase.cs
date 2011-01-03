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
    [SkinControl(2)] protected GUIMenuButton btnLayouts = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnViews = null;

    #endregion


    #region Serialisation

    protected virtual void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        int defaultLayout = (int)Layout.List;
        bool defaultAscending = true;
        if ((handler != null) && (handler.View != null) && (handler.View.Filters != null) &&
            (handler.View.Filters.Count > 0))
        {
          FilterDefinition def = (FilterDefinition)handler.View.Filters[0];
          defaultLayout = (int)GetLayoutNumber(def.DefaultView);
          defaultAscending = def.SortAscending;
        }
        currentLayout = (Layout)xmlreader.GetValueAsInt(SerializeName, "layout", defaultLayout);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", defaultAscending);


      }
    }

    protected virtual void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "layout", (int)currentLayout);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
      }
    }

    #endregion

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
      facadeLayout.CurrentLayout = CurrentLayout;
      PresentLayout();
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
      InitLayoutSelections();
      base.OnPageLoad();
    }

    private void InitLayoutSelections()
    {
      btnLayouts.ClearMenu();

      // Add the allowed layouts to choose from to the menu.
      int totalLayouts = Enum.GetValues(typeof(GUIFacadeControl.Layout)).Length;
      for (int i = 0; i < totalLayouts; i++)
      {
        string layoutName = Enum.GetName(typeof(GUIFacadeControl.Layout), i);
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

    private void SetLayout()
    {
      // Set the selected layout.
      SwitchToNextAllowedLayout(btnLayouts.SelectedItemValue);

      // Update properties.
      if (handler != null)
      {
        GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
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
        if (message.TargetControlId == btnLayouts.GetID)
        {
          SetLayout();
          SelectCurrentItem();

          // Refocus on the layout button control.
          GUIControl.FocusControl(GetID, message.TargetControlId);

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

      if (control == btnViews)
      {
        OnShowViews();
      }

      if (control == facadeLayout)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnInfo(iItem);
          facadeLayout.RefreshCoverArt();
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

    protected virtual void OnInfo(int iItem) {}

    protected virtual void OnClick(int iItem) {}
    
    protected virtual void OnQueueItem(int item) {}

    protected  virtual void SelectCurrentItem()
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
      if (btnSortBy != null)
      {
        btnSortBy.IsAscending = CurrentSortAsc;
      }
    }

    protected virtual void SwitchToNextAllowedLayout(int iSelectedLayout)
    {
      int totalLayouts = Enum.GetValues(typeof(Layout)).Length - 1;
      
      if (iSelectedLayout > totalLayouts)
        iSelectedLayout = 0;
      
      bool shouldContinue = true;
      do
      {
        Layout selectedLayout = (Layout)iSelectedLayout;
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

            MediaPortal.GUI.Music.GUIMusicPlayingNow guiPlayingNow = (MediaPortal.GUI.Music.GUIMusicPlayingNow)GUIWindowManager.GetWindow(nPlayingNowWindow);

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
