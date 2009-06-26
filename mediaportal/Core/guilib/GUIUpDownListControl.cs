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

// used for Keys definition

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The implementation of a GUIUpDownListControl
  /// </summary>
  public class GUIUpDownListControl : GUIListControl
  {
    private enum Selection
    {
      Button,
      Up,
      Down,
      PageCounter
    }

    private Selection currentSelection = Selection.Button;

    public GUIUpDownListControl(int dwParentID) : base(dwParentID)
    {
    }

    /// <summary>
    /// The constructor of the GUIUpDownListControl.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="dwSpinWidth">TODO </param>
    /// <param name="dwSpinHeight">TODO</param>
    /// <param name="strUp">The name of the scroll up unfocused texture.</param>
    /// <param name="strDown">The name of the scroll down unfocused texture.</param>
    /// <param name="strUpFocus">The name of the scroll up focused texture.</param>
    /// <param name="strDownFocus">The name of the scroll down unfocused texture.</param>
    /// <param name="dwSpinColor">TODO </param>
    /// <param name="dwSpinX">TODO </param>
    /// <param name="dwSpinY">TODO </param>
    /// <param name="strFont">The font used in the spin control.</param>
    /// <param name="dwTextColor">The color of the text.</param>
    /// <param name="dwSelectedColor">The color of the text when it is selected.</param>
    /// <param name="strButton">The name of the unfocused button texture.</param>
    /// <param name="strButtonFocus">The name of the focused button texture.</param>
    /// <param name="strScrollbarBackground">The name of the background of the scrollbar texture.</param>
    /// <param name="strScrollbarTop">The name of the top of the scrollbar texture.</param>
    /// <param name="strScrollbarBottom">The name of the bottom of the scrollbar texture.</param>
    public GUIUpDownListControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                int dwSpinWidth, int dwSpinHeight,
                                string strUp, string strDown,
                                string strUpFocus, string strDownFocus,
                                long dwSpinColor, int dwSpinX, int dwSpinY,
                                string strFont, long dwTextColor, long dwSelectedColor,
                                string strButton, string strButtonFocus,
                                string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight,
             dwSpinWidth, dwSpinHeight,
             strUp, strDown,
             strUpFocus, strDownFocus,
             dwSpinColor, dwSpinX, dwSpinY,
             strFont, dwTextColor, dwSelectedColor,
             strButton, strButtonFocus,
             strScrollbarBackground, strScrollbarTop, strScrollbarBottom)

    {
      FinalizeConstruction();
    }

    protected override void AllocButtons()
    {
      currentSelection = Selection.Button;
      for (int i = 0; i < _itemsPerPage; ++i)
      {
        GUIUpDownButton cntl = new GUIUpDownButton(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _width,
                                                   _itemHeight, _buttonFocusName, _buttonNonFocusName,
                                                   _spinControlWidth, _spinControlHeight,
                                                   _upTextureName, _downTextureName,
                                                   _upTextureNameFocus, _downTextureNameFocus,
                                                   _spinControlColor, _spinControlPositionX, _spinControlPositionY);

        cntl.ParentControl = this;
        cntl.AllocResources();
        _listButtons.Add(cntl);
      }
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      int id;
      bool focus;
      if (_verticalScrollbar.HitTest(x, y, out id, out focus))
      {
        return true;
      }

      if (_upDownControl.HitTest(x, y, out id, out focus))
      {
        if (_upDownControl.GetMaximum() > 1)
        {
          _listType = ListType.CONTROL_UPDOWN;
          _upDownControl.Focus = true;
          if (!_upDownControl.Focus)
          {
            _listType = ListType.CONTROL_LIST;
          }
          return true;
        }
        return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        return false;
      }
      _listType = ListType.CONTROL_LIST;
      int posy = y - (int) _positionY;
      _cursorX = (posy/(_itemHeight + _spaceBetweenItems));
      while (_offset + _cursorX >= _listItems.Count)
      {
        _cursorX--;
      }
      if (_cursorX >= _itemsPerPage)
      {
        _cursorX = _itemsPerPage - 1;
      }
      OnSelectionChanged();
      _refresh = true;

      if (_listButtons != null)
      {
        int cntlId;
        bool gotFocus;
        for (int i = 0; i < _itemsPerPage; ++i)
        {
          GUIUpDownButton btn = (GUIUpDownButton) _listButtons[i];
          btn.HitTest(x, y, out cntlId, out gotFocus);
          if (i == _cursorX)
          {
            currentSelection = Selection.Button;
            if (btn.UpDownControl.Focus)
            {
              if (btn.UpDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN)
              {
                currentSelection = Selection.Down;
              }
              if (btn.UpDownControl.SelectedButton == GUISpinControl.SpinSelect.SPIN_BUTTON_UP)
              {
                currentSelection = Selection.Up;
              }
            }
          }
        }
      }

      return true;
    }

    protected override void OnLeft()
    {
      switch (currentSelection)
      {
        case Selection.PageCounter:
          base.OnLeft();
          if (_listType == ListType.CONTROL_LIST)
          {
            currentSelection = Selection.Button;
          }
          break;

        case Selection.Button:
          //select down..
          currentSelection = Selection.PageCounter;
          base.OnLeft();
          break;

        case Selection.Down:
          //select Button..
          currentSelection = Selection.Button;
          break;

        case Selection.Up:
          //select Down..
          currentSelection = Selection.Down;
          break;
      }
      UpdateUpDownControls();
    }

    protected override void OnRight()
    {
      switch (currentSelection)
      {
        case Selection.Button:
          //select down..
          currentSelection = Selection.Down;
          break;
        case Selection.Down:
          //select up..
          currentSelection = Selection.Up;
          break;
        case Selection.Up:
          //select button..
          currentSelection = Selection.PageCounter;
          base.OnRight();
          break;
        case Selection.PageCounter:
          base.OnRight();
          if (_listType == ListType.CONTROL_LIST)
          {
            currentSelection = Selection.Button;
          }
          break;
      }
      UpdateUpDownControls();
    }


    protected override void OnUp()
    {
      base.OnUp();
      currentSelection = Selection.Button;
      UpdateUpDownControls();
    }

    protected override void OnDown()
    {
      base.OnDown();
      currentSelection = Selection.Button;
      UpdateUpDownControls();
    }

    private void UpdateUpDownControls()
    {
      for (int i = 0; i < _itemsPerPage; ++i)
      {
        GUIUpDownButton btn = (GUIUpDownButton) _listButtons[i];
        if (i == _cursorX)
        {
          switch (currentSelection)
          {
            case Selection.Button:
              btn.UpDownControl.Focus = false;
              break;
            case Selection.Down:
              btn.UpDownControl.Focus = true;
              btn.UpDownControl.SelectedButton = GUISpinControl.SpinSelect.SPIN_BUTTON_DOWN;
              break;
            case Selection.Up:
              btn.UpDownControl.Focus = true;
              btn.UpDownControl.SelectedButton = GUISpinControl.SpinSelect.SPIN_BUTTON_UP;
              break;
            case Selection.PageCounter:
              btn.UpDownControl.Focus = false;
              break;
          }
        }
        else
        {
          btn.UpDownControl.Focus = false;
        }
      }
    }

    protected override void OnMouseClick(Action action)
    {
      if (currentSelection == Selection.Down || currentSelection == Selection.Up)
      {
        // don't send the messages to a dialog menu
        if ((WindowId != (int) GUIWindow.Window.WINDOW_DIALOG_MENU) ||
            (action.wID == Action.ActionType.ACTION_SELECT_ITEM))
        {
          GUIMessage.MessageType msgType = GUIMessage.MessageType.GUI_MSG_CLICKED_UP;
          if (currentSelection == Selection.Down)
          {
            msgType = GUIMessage.MessageType.GUI_MSG_CLICKED_DOWN;
          }

          GUIMessage msg = new GUIMessage(msgType, WindowId, GetID, ParentID, (int) action.wID, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
      }
      else
      {
        base.OnMouseClick(action);
      }
    }

    protected override void OnDefaultAction(Action action)
    {
      if (currentSelection == Selection.Down || currentSelection == Selection.Up)
      {
        // don't send the messages to a dialog menu
        if ((WindowId != (int) GUIWindow.Window.WINDOW_DIALOG_MENU) ||
            (action.wID == Action.ActionType.ACTION_SELECT_ITEM))
        {
          GUIMessage.MessageType msgType = GUIMessage.MessageType.GUI_MSG_CLICKED_UP;
          if (currentSelection == Selection.Down)
          {
            msgType = GUIMessage.MessageType.GUI_MSG_CLICKED_DOWN;
          }

          GUIMessage msg = new GUIMessage(msgType, WindowId, GetID, ParentID, (int) action.wID, 0, null);
          GUIGraphicsContext.SendMessage(msg);
        }
      }
      else
      {
        base.OnDefaultAction(action);
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      bool result = base.OnMessage(message);
      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
      {
        UpdateUpDownControls();
      }
      return result;
    }
  }
}