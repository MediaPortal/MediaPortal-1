#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Drawing;
using System.Linq;

// used for Keys definition

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The implementation of a GUIPlayListItemListControl
  /// </summary>
  public class GUIPlayListItemListControl : GUIListControl
  {
    private enum Selection
    {
      ListItem,
      PageUpDown
    }

    private Selection _currentSelection = Selection.ListItem;
    private bool _moveFirstListVisibleItemUpAllowed = true;
    private bool _moveLastVisibleListItemDownAllowed = true;

    [XMLSkinElement("upBtnWidth")] private int _upBtnWidth = 35;
    [XMLSkinElement("downBtnWidth")] private int _downBtnWidth = 35;
    [XMLSkinElement("deleteBtnWidth")] private int _deleteBtnWidth = 35;
    [XMLSkinElement("upBtnHeight")] private int _upBtnHeight = 38;
    [XMLSkinElement("downBtnHeight")] private int _downBtnHeight = 38;
    [XMLSkinElement("deleteBtnHeight")] private int _deleteBtnHeight = 38;
    [XMLSkinElement("upBtnXOffset")] private int _upBtnXOffset = 326;
    [XMLSkinElement("downBtnXOffset")] private int _downBtnXOffset = 361;
    [XMLSkinElement("deleteBtnXOffset")] private int _deleteBtnXOffset = 396;
    [XMLSkinElement("upBtnYOffset")] private int _upBtnYOffset;
    [XMLSkinElement("downBtnYOffset")] private int _downBtnYOffset;
    [XMLSkinElement("deleteBtnYOffset")] private int _deleteBtnYOffset;
    [XMLSkinElement("textureFocus")] private string _textureFocus = "playlist_sub_focus.png";
    [XMLSkinElement("textureNoFocus")] private string _textureNoFocus = "playlist_sub_nofocus.png";
    [XMLSkinElement("textureMoveUp")] private string _textureMoveUp = "playlist_item_up_nofocus.png";
    [XMLSkinElement("textureMoveUpFocused")] private string _textureMoveUpFocused = "playlist_item_up_focus.png";
    [XMLSkinElement("textureMoveDown")] private string _textureMoveDown = "playlist_item_down_nofocus.png";
    [XMLSkinElement("textureMoveDownFocused")] private string _textureMoveDownFocus = "playlist_item_down_focus.png";
    [XMLSkinElement("textureDelete")] private string _textureDelete = "playlist_item_delete_nofocus.png";
    [XMLSkinElement("textureDeleteFocused")] private string _textureDeleteFocused = "playlist_item_delete_focus.png";

    public GUIPlayListItemListControl(int dwParentID) : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUIPlayListItemListControl.
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
    /// <param name="upBtnWidth">Width of the Up sub-button.</param>
    /// <param name="downBtnWidth">Width of the Down sub-button.</param>
    /// <param name="deleteBtnWidth">Width of the Delete sub-button.</param>
    /// <param name="upBtnHeight">Height of the Up sub-button.</param>
    /// <param name="downBtnHeight">Height of the Down sub-button.</param>
    /// <param name="deleteBtnHeight">Height of the Delete sub-button.</param>
    /// <param name="upBtnXOffset">The X position of the Up sub-button relative to the main button.</param>
    /// <param name="downBtnXOffset">The X position of the Down sub-button relative to the main button.</param>
    /// <param name="deleteBtnXOffset">The X position of the Delete sub-button relative to the main button.</param>
    /// <param name="upBtnYOffset">The Y position of the Up sub-button relative to the main button.</param>
    /// <param name="downBtnYOffset">The Y position of the Down sub-button relative to the main button.</param>
    /// <param name="deleteBtnYOffset">The Y position of the Delete sub-button relative to the main button.</param>
    /// <param name="textureFocus">The name of the Main button focused texture.</param>
    /// <param name="textureNoFocus">The name of the Main button unfocused texture.</param>
    /// <param name="textureMoveUp">The name of the Up sub-button focused texture.</param>
    /// <param name="textureMoveUpFocused">The name of the Up sub-button unfocused texture.</param>
    /// <param name="textureMoveDown">The name of the Down sub-button focused texture.</param>
    /// <param name="textureMoveDownFocus">The name of the Down sub-button unfocused texture.</param>
    /// <param name="textureDelete">The name of the Delete sub-button focused texture.</param>
    /// <param name="textureDeleteFocused">The name of the Delete sub-button unfocused texture.</param>
    /// <param name="dwShadowAngle"></param>
    /// <param name="dwShadowDistance"></param>
    /// <param name="dwShadowColor"></param>
    public GUIPlayListItemListControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                      int upBtnWidth,
                                      int downBtnWidth,
                                      int deleteBtnWidth,
                                      int upBtnHeight,
                                      int downBtnHeight,
                                      int deleteBtnHeight,
                                      int upBtnXOffset,
                                      int downBtnXOffset,
                                      int deleteBtnXOffset,
                                      int upBtnYOffset,
                                      int downBtnYOffset,
                                      int deleteBtnYOffset,
                                      string textureFocus,
                                      string textureNoFocus,
                                      string textureMoveUp,
                                      string textureMoveUpFocused,
                                      string textureMoveDown,
                                      string textureMoveDownFocus,
                                      string textureDelete,
                                      string textureDeleteFocused,
                                      int dwSpinWidth,
                                      int dwSpinHeight,
                                      string strUp,
                                      string strDown,
                                      string strUpFocus,
                                      string strDownFocus,
                                      long dwSpinColor,
                                      int dwSpinX,
                                      int dwSpinY,
                                      string strFont,
                                      long dwTextColor,
                                      long dwSelectedColor,
                                      string strButton,
                                      string strButtonFocus,
                                      string strScrollbarBackground,
                                      string strScrollbarTop,
                                      string strScrollbarBottom,
                                      int dwShadowAngle,
                                      int dwShadowDistance,
                                      long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight,
             dwSpinWidth, dwSpinHeight,
             strUp, strDown,
             strUpFocus, strDownFocus,
             dwSpinColor, dwSpinX, dwSpinY,
             strFont, dwTextColor, dwSelectedColor,
             strButton, strButtonFocus,
             strScrollbarBackground, strScrollbarTop, strScrollbarBottom,
             dwShadowAngle, dwShadowDistance, dwShadowColor)

    {
      _upBtnWidth = upBtnWidth;
      _downBtnWidth = downBtnWidth;
      _deleteBtnWidth = deleteBtnWidth;
      _upBtnHeight = upBtnHeight;
      _downBtnHeight = downBtnHeight;
      _deleteBtnHeight = deleteBtnHeight;
      _upBtnXOffset = upBtnXOffset;
      _downBtnXOffset = downBtnXOffset;
      _deleteBtnXOffset = deleteBtnXOffset;
      _upBtnYOffset = upBtnYOffset;
      _downBtnYOffset = downBtnYOffset;
      _deleteBtnYOffset = deleteBtnYOffset;
      _textureFocus = textureFocus;
      _textureNoFocus = textureNoFocus;
      _textureMoveUp = textureMoveUp;
      _textureMoveUpFocused = textureMoveUpFocused;
      _textureMoveDown = textureMoveDown;
      _textureMoveDownFocus = textureMoveDownFocus;
      _textureDelete = textureDelete;
      _textureDeleteFocused = textureDeleteFocused;

      FinalizeConstruction();
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScalePosToScreenResolution(ref _upBtnWidth, ref _upBtnHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _downBtnWidth, ref _downBtnHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _deleteBtnWidth, ref _deleteBtnHeight);

      GUIGraphicsContext.ScaleHorizontal(ref _upBtnXOffset);
      GUIGraphicsContext.ScaleHorizontal(ref _downBtnXOffset);
      GUIGraphicsContext.ScaleHorizontal(ref _deleteBtnXOffset);

      GUIGraphicsContext.ScaleVertical(ref _upBtnYOffset);
      GUIGraphicsContext.ScaleVertical(ref _downBtnYOffset);
      GUIGraphicsContext.ScaleVertical(ref _deleteBtnYOffset);
    }

    protected override void AllocButtons()
    {
      _currentSelection = Selection.ListItem;
      for (int i = 0; i < _itemsPerPage; ++i)
      {
        GUIPlayListButtonControl cntl = new GUIPlayListButtonControl(_controlId, 0, 0, 0, _width, _itemHeight,
                                                                     _textureFocus, _textureNoFocus,
                                                                     _upBtnWidth, _downBtnWidth, _deleteBtnWidth,
                                                                     _upBtnHeight, _downBtnHeight, _deleteBtnHeight,
                                                                     _textureMoveUp, _textureMoveDown, _textureDelete,
                                                                     _textureMoveUpFocused, _textureMoveDownFocus,
                                                                     _textureDeleteFocused,
                                                                     _upBtnXOffset, _downBtnXOffset, _deleteBtnXOffset,
                                                                     _upBtnYOffset, _downBtnYOffset, _deleteBtnYOffset)
                                          {
                                            ParentControl = this
                                          };

        cntl.FinalizeConstruction();
        cntl.AllocResources();
        cntl.DimColor = DimColor;
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
      int posy = y - _positionY;
      _cursorX = (posy / (_itemHeight + _spaceBetweenItems));

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
        for (int i = 0; i < _itemsPerPage; ++i)
        {
          GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[i];
          int cntlId;
          bool gotFocus;
          btn.HitTest(x, y, out cntlId, out gotFocus);

          if (i == _cursorX)
          {
            _currentSelection = Selection.ListItem;
          }
        }
      }

      return true;
    }

    protected override void OnLeft()
    {
      switch (_currentSelection)
      {
        case Selection.PageUpDown:
          base.OnLeft();
          if (_listType == ListType.CONTROL_LIST)
          {
            _currentSelection = Selection.ListItem;
          }
          break;

        case Selection.ListItem:
          if (_cursorX >= 0)
          {
            GUIPlayListButtonControl btn = (GUIPlayListButtonControl) _listButtons[_cursorX];
            GUIPlayListButtonControl.SuppressActiveButtonReset = false;
            if (btn.CurrentActiveButton != GUIPlayListButtonControl.ActiveButton.Main && btn.CanMoveLeft())
            {
              Action action = new Action {wID = Action.ActionType.ACTION_MOVE_LEFT};
              btn.OnAction(action);
              return;
            }
          }

          // select down..
          _currentSelection = Selection.PageUpDown;
          base.OnLeft();
          break;
      }
    }

    protected override void OnRight()
    {
      Action action = new Action();

      if (_listType == ListType.CONTROL_LIST)
      {
        if (_cursorX >= 0)
        {
          GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[_cursorX];
          GUIPlayListButtonControl.SuppressActiveButtonReset = false;

          if (btn.CurrentActiveButton != GUIPlayListButtonControl.ActiveButton.Delete && btn.CanMoveRight())
          {
            action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
            btn.OnAction(action);
            return;
          }

          btn.CurrentActiveButton = GUIPlayListButtonControl.ActiveButton.None;
          if (!_spinCanFocus)
          {
            btn.CurrentActiveButton = GUIPlayListButtonControl.ActiveButton.Delete;
            return;
          }
        }

        action.wID = Action.ActionType.ACTION_MOVE_RIGHT;

        if (_listType == ListType.CONTROL_LIST)
        {
          if (_upDownControl.GetMaximum() > 1)
          {
            _listType = ListType.CONTROL_UPDOWN;
            _upDownControl.Focus = true;

            if (!_upDownControl.Focus)
            {
              _listType = ListType.CONTROL_LIST;
            }
          }
        }
        else
        {
          _upDownControl.OnAction(action);

          if (!_upDownControl.Focus)
          {
            if (_rightControlId != GetID)
            {
              base.OnAction(action);
            }

            _listType = ListType.CONTROL_LIST;
          }
        }
      }
    }

    protected override void OnUp()
    {
      if (_listType == ListType.CONTROL_LIST)
      {
        GUIPlayListButtonControl.SuppressActiveButtonReset = false;
      }
      base.OnUp();
    }

    /// <summary>
    /// Implementation of the OnDown action.
    /// </summary>
    protected override void OnDown()
    {
      if (_listType == ListType.CONTROL_LIST)
      {
        GUIPlayListButtonControl.SuppressActiveButtonReset = false;
      }
      base.OnDown();
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOVE_DOWN:
        case Action.ActionType.ACTION_MOVE_UP:
          {
            GUIPlayListButtonControl.SuppressActiveButtonReset = false;
            break;
          }

        case Action.ActionType.ACTION_MOUSE_CLICK:
        case Action.ActionType.ACTION_SELECT_ITEM:
          {
            GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[_cursorX];

            if (btn != null)
            {
              Action newAction = new Action();

              if (btn.CurrentActiveButton == GUIPlayListButtonControl.ActiveButton.Up)
              {
                if (btn.UpButtonEnabled)
                {
                  newAction.wID = Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP;
                  GUIPlayListButtonControl.LastActiveButton = GUIPlayListButtonControl.ActiveButton.Up;
                  GUIPlayListButtonControl.SuppressActiveButtonReset = true;
                }
                else
                {
                  return;
                }
              }
              else if (btn.CurrentActiveButton == GUIPlayListButtonControl.ActiveButton.Down)
              {
                if (btn.DownButtonEnabled)
                {
                  newAction.wID = Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN;
                  GUIPlayListButtonControl.LastActiveButton = GUIPlayListButtonControl.ActiveButton.Down;
                  GUIPlayListButtonControl.SuppressActiveButtonReset = true;
                }
                else
                {
                  return;
                }
              }
              else if (btn.CurrentActiveButton == GUIPlayListButtonControl.ActiveButton.Delete)
              {
                if (btn.DownButtonEnabled)
                {
                  newAction.wID = Action.ActionType.ACTION_DELETE_SELECTED_ITEM;
                  GUIPlayListButtonControl.LastActiveButton = GUIPlayListButtonControl.ActiveButton.Delete;
                  GUIPlayListButtonControl.SuppressActiveButtonReset = true;
                }
                else
                {
                  return;
                }
              }
              else if (btn.CurrentActiveButton == GUIPlayListButtonControl.ActiveButton.Main)
              {
                GUIPlayListButtonControl.LastActiveButton = GUIPlayListButtonControl.ActiveButton.None;
                GUIPlayListButtonControl.SuppressActiveButtonReset = false;
                break;
              }
              else
              {
                break;
              }

              GUIGraphicsContext.OnAction(newAction);
              Console.WriteLine("\t**action modified:{0}", newAction.wID);
              return;
            }
            break;
          }
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      bool result = base.OnMessage(message);

      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
      {
        if (_currentSelection == Selection.PageUpDown)
        {
          Console.WriteLine("currentSelection == Selection.PageUpDown");
        }
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
      {
        Console.WriteLine("GUI_MSG_SETFOCUS");
        SetItemButtonState(GUIPlayListButtonControl.ActiveButton.Main);
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS)
      {
        SetItemButtonState(GUIPlayListButtonControl.ActiveButton.None);
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
      {
        SetItemButtonState(GUIPlayListButtonControl.ActiveButton.Main);
      }

      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
      {
        foreach (GUIListItem item in _listItems)
        {
          item.Selected = false;
        }
        foreach (GUIListItem item in _listItems.Where(item => item.Path.Equals(message.Label, StringComparison.OrdinalIgnoreCase)))
        {
          item.Selected = true;
          break;
        }
      }

      return result;
    }

    public override void Render(float timePassed)
    {
      _timeElapsed += timePassed;

      // If there is no font do not render.
      if (null == _font)
      {
        base.Render(timePassed);
        return;
      }

      // If the control is not visible do not render.
      if (GUIGraphicsContext.EditMode == false && !IsVisible)
      {
        base.Render(timePassed);
        return;
      }

      int dwPosY = _positionY;

      // Render the buttons first.
      for (int i = 0; i < _itemsPerPage; i++)
      {
        if (i + _offset < _listItems.Count)
        {
          // render item
          bool gotFocus = _drawFocus && i == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST;
          RenderButton(timePassed, i, _positionX, dwPosY, gotFocus);
        }
        dwPosY += _itemHeight + _spaceBetweenItems;
      }

      // Free unused textures if page has changed
      FreeUnusedThumbnails();

      // Render new item list
      dwPosY = _positionY;

      for (int i = 0; i < _itemsPerPage; i++)
      {
        int dwPosX = _positionX;

        if (i + _offset < _listItems.Count)
        {
          bool gotFocus = _drawFocus && i == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST;

          // render the icon
          RenderIcon(timePassed, i, dwPosX + _iconOffsetX, dwPosY + _iconOffsetY, gotFocus);

          dwPosX += (_imageWidth + GUIGraphicsContext.ScaleHorizontal(10));

          // render the text
          RenderLabel(timePassed, i, dwPosX, dwPosY, gotFocus);
          RenderPinIcon(timePassed, i, _positionX, dwPosY, gotFocus);

          dwPosY += _itemHeight + _spaceBetweenItems;
        }
      }

      RenderScrollbar(timePassed);

      if (Focus)
      {
        GUIPropertyManager.SetProperty("#highlightedbutton", string.Empty);
      }
    }

    protected override void RenderButton(float timePassed, int buttonNr, int x, int y, bool gotFocus)
    {
      if (_listButtons != null && (buttonNr >= 0 && buttonNr < _listButtons.Count))
      {
        GUIControl btn = _listButtons[buttonNr];
        if (btn != null)
        {
          if (gotFocus)
          {
            btn.ColourDiffuse = 0xffffffff;
          }
          else
          {
            btn.ColourDiffuse = Color.FromArgb(_unfocusedAlpha, Color.White).ToArgb();
          }
          btn.Focus = gotFocus;
          btn.SetPosition(x, y);
          btn.Render(timePassed);
        }
      }
    }

    protected override void RenderLabel(float timePassed, int buttonNr, int dwPosX, int dwPosY, bool gotFocus)
    {
      if (buttonNr < 0 || buttonNr >= _listButtons.Count)
      {
        return;
      }

      GUIListItem pItem = _listItems[buttonNr + _offset];
      long dwColor;

      dwPosX += _textOffsetX;
      bool bSelected = buttonNr == _cursorX && IsFocused && _listType == ListType.CONTROL_LIST;

      GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[buttonNr];
      int dMaxWidth = (_width - _imageWidth - PinIconOffsetX - GUIGraphicsContext.ScaleHorizontal(8));

      if (btn != null)
      {
        dMaxWidth = (_upBtnXOffset - _imageWidth - PinIconOffsetX - GUIGraphicsContext.ScaleHorizontal(8));
      }

      if (_text2Visible && pItem.Label2.Length > 0)
      {
        if (_textOffsetY == _textOffsetY2)
        {
          dwColor = _textColor2;
          if (pItem.Selected)
          {
            dwColor = _selectedColor2;
          }
          if (pItem.IsRemote)
          {
            dwColor = _remoteColor;
            if (pItem.IsDownloading)
            {
              dwColor = _downloadColor;
            }
          }

          if (pItem.IsBdDvdFolder)
          {
            dwColor = _bdDvdDirectoryColor;
          }

          int xpos;
          int ypos = dwPosY;

          if (0 == _textOffsetX2)
          {
            xpos = _positionX + _upBtnXOffset - GUIGraphicsContext.ScaleHorizontal(8);
          }
          else
          {
            xpos = _positionX + _textOffsetX2;
          }

          if (_labelControls2 != null)
          {
            if (buttonNr >= 0 && buttonNr < _labelControls2.Count)
            {
              GUILabelControl label2 = _labelControls2[buttonNr];
              if (label2 != null)
              {
                label2.SetPosition(xpos, ypos + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY2);
                label2.TextColor = gotFocus ? dwColor : Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
                label2.Label = pItem.Label2;
                label2.TextAlignment = Alignment.ALIGN_RIGHT;
                label2.FontName = _fontName2Name;
                dMaxWidth = label2._positionX - dwPosX - label2.TextWidth - GUIGraphicsContext.ScaleHorizontal(20);
              }
            }
          }
        }
      }

      if (_text1Visible)
      {
        dwColor = _textColor;
        if (pItem.Selected)
        {
          dwColor = _selectedColor;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }

        if (pItem.IsBdDvdFolder)
        {
          dwColor = _bdDvdDirectoryColor;
        }

        if (!gotFocus)
        {
          dwColor = Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
        }

        int maxWidth = dMaxWidth;
        if (_textPadding > 0)
        {
          maxWidth -= GUIGraphicsContext.ScaleHorizontal(_textPadding);
        }
        
        if (maxWidth <= 0)
        {
          base.Render(timePassed);
        }
        else
        {
          RenderText(timePassed, buttonNr, dwPosX, (float)dwPosY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY, maxWidth, dwColor, pItem.Label, bSelected);
        }
      }

      if (pItem.Label2.Length > 0)
      {
        dwColor = _textColor2;
        if (pItem.Selected)
        {
          dwColor = _selectedColor2;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }

        if (pItem.IsBdDvdFolder)
        {
          dwColor = _bdDvdDirectoryColor;
        }

        if (_textOffsetX2 == 0)
        {
          dwPosX = _positionX + _upBtnXOffset - GUIGraphicsContext.ScaleHorizontal(8);
        }
        else
        {
          dwPosX = _positionX + _textOffsetX2;
        }

        if (_text2Visible)
        {
          if (_labelControls2 != null && (buttonNr >= 0 && buttonNr < _labelControls2.Count))
          {
            GUILabelControl label2 = _labelControls2[buttonNr];
            if (label2 != null)
            {
              label2.SetPosition(dwPosX, dwPosY + GUIGraphicsContext.ScaleVertical(2) + _textOffsetY2);
              label2.TextColor = gotFocus
                                   ? dwColor
                                   : Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int) dwColor)).ToArgb();
              label2.Label = pItem.Label2;
              label2.TextAlignment = Alignment.ALIGN_RIGHT;
              label2.FontName = _fontName2Name;

              float width = label2.Width;
              float height = label2.Height;
              _font.GetTextExtent(label2.Label, ref width, ref height);
              label2.Width = (int)width + 1;
              label2.Height = (int)height;

              if (_textPadding2 > 0)
              {
                label2.Width -= GUIGraphicsContext.ScaleHorizontal(_textPadding2);
              }

              if (label2.Width > 0)
              {
                label2.Render(timePassed);
              }
            }
          }
        }
      }

      if (pItem.Label3.Length > 0)
      {
        dwColor = _textColor3;
        if (pItem.Selected)
        {
          dwColor = _selectedColor3;
        }

        if (pItem.IsRemote)
        {
          dwColor = _remoteColor;
          if (pItem.IsDownloading)
          {
            dwColor = _downloadColor;
          }
        }

        if (pItem.IsBdDvdFolder)
        {
          dwColor = _bdDvdDirectoryColor;
        }

        if (0 == _textOffsetX3)
        {
          dwPosX = _positionX + _textOffsetX;
        }
        else
        {
          dwPosX = _positionX + _textOffsetX3;
        }

        int ypos = dwPosY;

        if (0 == _textOffsetY3)
        {
          ypos += _textOffsetY2;
        }
        else
        {
          ypos += _textOffsetY3;
        }

        if (_text3Visible)
        {
          if (_labelControls3 != null)
          {
            if (buttonNr >= 0 && buttonNr < _labelControls3.Count)
            {
              GUILabelControl label3 = _labelControls3[buttonNr];

              if (label3 != null)
              {
                label3.SetPosition(dwPosX, ypos);

                label3.TextColor = gotFocus ? dwColor : Color.FromArgb(_unfocusedAlpha, Color.FromArgb((int)dwColor)).ToArgb();
                label3.Label = pItem.Label3;
                label3.TextAlignment = Alignment.ALIGN_LEFT;
                label3.FontName = _fontName2Name;

                float width = label3.Width;
                float height = label3.Height;
                _font.GetTextExtent(label3.Label, ref width, ref height);
                label3.Width = (int)width + 1;
                label3.Height = (int)height;

                if (_textPadding3 > 0)
                {
                  label3.Width -= GUIGraphicsContext.ScaleHorizontal(_textPadding3);
                }

                if (label3.Width > 0)
                {
                  label3.Render(timePassed);
                }
              }
            }
          }
        }
      }
    }

    #region Item Button Methods

    private void SetItemButtonState(GUIPlayListButtonControl.ActiveButton activeButton)
    {
      if (_cursorX < 0)
      {
        return;
      }

      GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[_cursorX];

      if (btn != null)
      {
        btn.CurrentActiveButton = activeButton;
      }
    }

    /// <summary>
    /// Prevent/allow first visible list item to be moved upward
    /// </summary>
    public bool AllowMoveFirstVisibleListItemUp
    {
      get { return _moveFirstListVisibleItemUpAllowed; }
      set
      {
        _moveFirstListVisibleItemUpAllowed = value;

        if (_listButtons == null || _listButtons.Count == 0)
        {
          return;
        }

        GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[0];
        btn.UpButtonEnabled = value;
      }
    }

    /// <summary>
    /// Prevent/allow last visible list item to be moved downward
    /// </summary>
    public bool AllowLastVisibleListItemDown
    {
      get { return _moveLastVisibleListItemDownAllowed; }
      set
      {
        _moveLastVisibleListItemDownAllowed = value;

        if (_listButtons == null || _listButtons.Count == 0)
        {
          return;
        }

        GUIPlayListButtonControl btn = (GUIPlayListButtonControl)_listButtons[_listButtons.Count - 1];
        btn.DownButtonEnabled = value;
      }
    }

    #endregion
  }
}