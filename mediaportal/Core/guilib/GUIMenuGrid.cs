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

#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Animation;
using MediaPortal.Profile;
using Microsoft.DirectX.Direct3D;
using MediaPortal.ExtensionMethods;

// used for Keys definition

#endregion

namespace MediaPortal.GUI.Library
{
  public class GUIMenuGrid : GUIMenuControl
  {
    #region Properties (Skin)    

    [XMLSkinElement("numberOfColumns")]
    protected int _numberOfColumns = 1;
    [XMLSkinElement("numberOfRows")]
    protected int _numberOfRows = 1;
    [XMLSkinElement("scrollOffset")]
    protected int _scrollStartOffset = 0;

    #endregion

    #region Enums

    protected enum State
    {
      Idle,
      ScrollUp,
      ScrollDown,
      ScrollUpFinal,
      ScrollDownFinal
    }

    #endregion

    #region Variables

    protected int _horizontalSpaceBetweenButtons;
    protected int _verticalSpaceBetweenButtons;
    protected int _horizontalButtonOffset;
    protected int _verticalButtonOffset;

    private int _gridRows;
    private int _gridColumns;
    private int _firstRow = 0;
    private int _firstColumn = 0;

    #endregion

    #region Constructors/Destructors

    public GUIMenuGrid(int dwParentID)
      : base(dwParentID) { }

    #endregion

    #region Base class overrides

    #region Methods

    public override void OnInit()
    {
      LoadHoverImage(FocusedButton);

      base.OnInit();
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      _horizontalSpaceBetweenButtons = _spaceBetweenButtons;
      _verticalSpaceBetweenButtons = _spaceBetweenButtons;
      GUIGraphicsContext.ScalePosToScreenResolution(ref _horizontalSpaceBetweenButtons, ref _verticalSpaceBetweenButtons);

      _horizontalButtonOffset = _buttonOffset;
      _verticalButtonOffset = _buttonOffset;
      GUIGraphicsContext.ScalePosToScreenResolution(ref _horizontalButtonOffset, ref _verticalButtonOffset);
    }

    public override void FinalizeConstruction()
    {
      // Check grid values
      if (_numberOfColumns < 1)
        _numberOfColumns = _horizontal ? 3 : 2;
      if (_numberOfRows < 1)
        _numberOfRows = _horizontal ? 2 : 3;
      if (_scrollStartOffset < 0)
        _scrollStartOffset = 0;

      // Calculate menu and button sizes
      if (Width > 0 && Height > 0)
      {
        // Menu size is fully defined, so ignore defined button size and resize buttons
        _buttonWidth = (Width - 2 * _horizontalButtonOffset) / _numberOfColumns - _horizontalSpaceBetweenButtons;
        _buttonHeight = (Height - 2 * _verticalButtonOffset) / _numberOfRows - _verticalSpaceBetweenButtons;
      }
      else
      {
        // Menu size is not fully defined, so take button size to calculate menu size
        decimal whRatio;

        // Get button size
        if (_buttonWidth > 0 && _buttonHeight > 0)
        {
          // Button size is fully defined
          whRatio = (decimal)_buttonWidth / (decimal)_buttonHeight;
        }
        else
        {
          // Button size is not fully defined, so take texture size to calculate button size
          int textureWidth;
          int textureHeight;
          GUIImage texture = new GUIImage(GetID);
          texture.SetFileName(_textureButtonFocus);
          textureWidth = texture.TextureWidth;
          textureHeight = texture.TextureHeight;
          whRatio = (decimal)textureWidth / (decimal)textureHeight;
          texture.SafeDispose();
 
          // Set missing button sizes
         if (_buttonWidth <= 0 && _buttonHeight <= 0)
          {
            // Nothing defined, take texture size as button size
            _buttonWidth = textureWidth;
            _buttonHeight = textureHeight;
            GUIGraphicsContext.ScalePosToScreenResolution(ref _buttonWidth, ref _buttonHeight);
          }
          else
          {
            // Calculate button's width / height from texture's aspect ratio 
            if (_buttonWidth > 0)
              _buttonHeight = (int)Math.Round(_buttonWidth / whRatio);
            else // (_buttonHeight > 0)
              _buttonWidth = (int)Math.Round(_buttonHeight * whRatio);
          }
        }

        // Calculate menu size
        if (Width <= 0 && Height <= 0)
        {
          // Nothing defined, so calculate menu size from button size
          Width = (_buttonWidth + _horizontalSpaceBetweenButtons) * _numberOfColumns + 2 * _horizontalButtonOffset;
          Height = (_buttonHeight + _verticalSpaceBetweenButtons) * _numberOfRows + 2 * _verticalButtonOffset;
        }
        else
        {
          if (Width > 0)
          {
            // Scale everything to fit the defined menu width
            _buttonWidth = (int)Math.Round((Width - 2 * _horizontalButtonOffset) / (decimal)_numberOfColumns) - _horizontalSpaceBetweenButtons;
            _buttonHeight = (int)Math.Round(_buttonWidth / whRatio);
            Height = (_buttonHeight + _verticalSpaceBetweenButtons) * _numberOfRows + 2 * _verticalButtonOffset;
          }
          else // (Height > 0)
          {
            // Scale everything to fit the defined menu height
            _buttonHeight = (int)Math.Round((Height - 2 * _verticalButtonOffset) / (decimal)_numberOfRows) - _verticalSpaceBetweenButtons;
            _buttonWidth = (int)Math.Round(_buttonHeight * whRatio);
            Width = (_buttonWidth + _horizontalSpaceBetweenButtons) * _numberOfColumns + 2 * _horizontalButtonOffset;
          }
        }
      }

      // Set initially focused button
      _focusPosition = (_numberOfColumns - 1) / 2;

      base.FinalizeConstruction();
    }

    public override void AllocResources()
    {
      // Dispose button and hover lists
      _buttonList.DisposeAndClear();
      _hoverList.DisposeAndClear();

      // Extend button grid horizontally or vertically to be large enough for all buttonInfos 
      if (_horizontal)
      {
        _gridRows = _numberOfRows;
        _gridColumns = (int)Math.Max(Math.Ceiling((decimal)_buttonInfos.Count / _gridRows), _numberOfColumns);
      }
      else
      {
        _gridColumns = _numberOfColumns;
        _gridRows = (int)Math.Max(Math.Ceiling((decimal)_buttonInfos.Count / _gridColumns), _numberOfRows);
      }

      int controlID = 0;

      // Create all the buttons needed
      for (int i = 0; i < _buttonInfos.Count; i++)
      {
        MenuButtonInfo info = _buttonInfos[i];
        GUIButtonControl button;

        // Create a button with hover image or texture
        if (_showAllHover)
        {
          button = new GUIButtonControl(GetID, controlID, 0, 0, _buttonWidth, _buttonHeight, _textColor, _textColorNoFocus,
                                        (info != null && !string.IsNullOrEmpty(info.HoverName)) ? info.HoverName : _textureButtonFocus,
                                        (info != null && !string.IsNullOrEmpty(info.NonFocusHoverName)) ? info.NonFocusHoverName : _textureHoverNoFocus,
                                        _shadowAngle, _shadowDistance, _shadowColor);
        }
        else
        {
          button = new GUIButtonControl(GetID, controlID, 0, 0, _buttonWidth, _buttonHeight, _textColor, _textColorNoFocus,
                                        (info != null && !string.IsNullOrEmpty(info.FocusTextureName)) ? info.FocusTextureName : _textureButtonFocus,
                                        (info != null && !string.IsNullOrEmpty(info.NonFocusTextureName)) ? info.NonFocusTextureName : _textureButtonNoFocus,
                                        _shadowAngle, _shadowDistance, _shadowColor);
        }

        button.TextAlignment = _textAlignment;
        button.Label = info == null ? String.Empty : info.Text;
        button.Data = info;
        button.ParentControl = this;
        button.FontName = _buttonFont;
        button.TextOffsetX = _buttonTextXOffset;
        button.TextOffsetY = _buttonTextYOffset;
        button.DimColor = DimColor;
        if (ThumbAnimations != null && ThumbAnimations.Count > 0)
          button.SetAnimations(ThumbAnimations);

        button.AllocResources();
        _buttonList.Add(button);
        controlID++;
      }

      // Create the background image
      _backgroundImage = LoadAnimationControl(GetID, controlID, _positionX, _positionY, Width, Height, _textureBackground);
      _backgroundImage.AllocResources();
      controlID++;

      if ((_hoverHeight > 0) && (_hoverWidth > 0))
      {
        foreach (GUIButtonControl btn in _buttonList)
        {
          if (btn.GetID < _buttonInfos.Count)
          {
            GUIAnimation hover = null;
            string fileName = _buttonInfos[btn.GetID].HoverName;
            if (!string.IsNullOrEmpty(fileName))
            {
              hover = LoadAnimationControl(GetID, btn.GetID, _hoverPositionX, _hoverPositionX, _hoverWidth, _hoverHeight, fileName);
              hover.FlipX = _flipX;
              hover.FlipY = _flipY;
              hover.DiffuseFileName = _diffuseFileName;
              hover.KeepAspectRatio = _hoverKeepAspectRatio;
              hover.RepeatBehavior = new RepeatBehavior(1);
              hover.AllocResources();
            }
            _hoverList.Add(hover);
          }
        }
      }

      base.AllocResources();
    }

    public override void Dispose()
    {
      _buttonList.DisposeAndClear();
      _hoverList.DisposeAndClear();
      _backgroundImage.SafeDispose();
      _hoverImage.SafeDispose();

      base.Dispose();
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
      case Action.ActionType.ACTION_MOVE_UP:
        if (OnUp())
          return;
        break;

      case Action.ActionType.ACTION_MOVE_DOWN:
        if (OnDown())
          return;
        break;

      case Action.ActionType.ACTION_MOVE_LEFT:
        if (OnLeft())
          return;
        break;

      case Action.ActionType.ACTION_MOVE_RIGHT:
        if (OnRight())
          return;
        break;

      case Action.ActionType.ACTION_SELECT_ITEM:
      case Action.ActionType.ACTION_MOUSE_CLICK:
        MenuButtonInfo info = null;
        if (FocusedButton >= 0 && FocusedButton < _buttonList.Count)
          info = _buttonList[FocusedButton].Data as MenuButtonInfo;
        if (info != null)
        {
          // Button selected - send a message to the parent window
          GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID,
                                              info.PluginID, 0, info);
          GUIGraphicsContext.SendMessage(message);

          // If this button has a click setting then execute the setting.
          if (_onclick.Length != 0)
          {
            GUIPropertyManager.Parse(_onclick, GUIExpressionManager.ExpressionOptions.EVALUATE_ALWAYS);
          }
        }
        break;
      }

      base.OnAction(action);
    }

    public override void Render(float timePassed)
    {
      GUIButtonControl button;
      int buttonX;
      int buttonY;
      uint currentTime;

      // Draw background image
      if (_backgroundImage != null)
      {
        _backgroundImage.Render(timePassed);
      }

      // Draw hover image
      if (_hoverImage != null && !_showAllHover)
      {
        _hoverImage.Render(timePassed);
      }

      // Move through the button grid
      buttonX = _positionX + _horizontalButtonOffset;
      buttonY = _positionY + _verticalButtonOffset;

      for (int row = _firstRow; row < _firstRow + _numberOfRows; row++)
      {
        
        for (int column = _firstColumn; column < _firstColumn + _numberOfColumns; column++)
        {
          int i = row * _gridColumns + column;
          if (i >= _buttonList.Count)
            break;

          // Set button's position
          button = _buttonList[i];
          button.SetPosition(buttonX, buttonY);

          // Do not draw focused button now
          if (i != FocusedButton)
          {
            // Draw button with animation
            if (GUIGraphicsContext.Animations)
            {
              currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;
              button.UpdateVisibility();
              button.DoRender(timePassed, currentTime);
            }
            else
            {
              button.Render(timePassed);
            }
          }

          buttonX += (_buttonWidth + _horizontalSpaceBetweenButtons);
        }

        buttonX = _positionX + _horizontalButtonOffset;
        buttonY += (_buttonHeight + _verticalSpaceBetweenButtons);        
      }

      // Draw focused button on top of all other buttons
      if (FocusedButton >= 0 && FocusedButton < _buttonList.Count)
      {
        button = _buttonList[FocusedButton];
        if (GUIGraphicsContext.Animations)
        {
          currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;
          button.UpdateVisibility();
          button.DoRender(timePassed, currentTime);
        }
        else
        {
          button.Render(timePassed);
        }
      }

      base.Render(timePassed);
    }

    #endregion

    #region Properties

    public override int FocusedButton
    {
      get { return _focusPosition; }
      set
      {
        _buttonList[_focusPosition].Focus = false;
        _focusPosition = value;
        _buttonList[_focusPosition].Focus = true;
      }
    }

    public override List<MenuButtonInfo> ButtonInfos
    {
      get { return _buttonInfos; }
    }

    #endregion

    #endregion

    #region Private Methods

    // Functions return true if the move is made internally (i.e. from one button to another), otherwise false is returned

    private bool OnLeft()
    {
      int focusColumn = FocusedButton % _gridColumns;

      // Do not move beyond the defined buttons
      if (focusColumn == 0)
        return false;

      // See if we have to scroll
      if (_firstColumn > 0 && focusColumn - _scrollStartOffset <= _firstColumn)
        _firstColumn--;

      // Move button left one column
      FocusedButton--;
      LoadHoverImage(FocusedButton);
      return true;
    }

    private bool OnRight()
    {
      int focusColumn = FocusedButton % _gridColumns;

      // Do not move beyond the grid and the defined buttons
      if (focusColumn + 1 >= _gridColumns || FocusedButton + 1 >= _buttonList.Count)
        return false;

      // See if we have to scroll
      if (_firstColumn + _numberOfColumns < _gridColumns && focusColumn + _scrollStartOffset + 1 >= _firstColumn + _numberOfColumns)
        _firstColumn++;

      // Move button right one column
      FocusedButton++;
      LoadHoverImage(FocusedButton);
      return true;
    }

    private bool OnUp()
    {
      int focusRow = FocusedButton / _gridColumns;

      // Do not move beyond the defined buttons
      if (focusRow == 0)
        return false;

      // See if we have to scroll
      if (_firstRow > 0 && focusRow - _scrollStartOffset <= _firstRow)
        _firstRow--;

      // Move button up one row
      FocusedButton -= _gridColumns;
      LoadHoverImage(FocusedButton);
      return true;
    }

    private bool OnDown()
    {
      int focusRow = FocusedButton / _gridColumns;

      // Do not move beyond the defined buttons
      if (FocusedButton + _gridColumns >= _buttonList.Count)
        return false;

      // See if we have to scroll
      if (_firstRow + _numberOfRows < _gridRows && focusRow + _scrollStartOffset + 1 >= _firstRow + _numberOfRows)
      {
        _firstRow++;
      }

      // Move button down one row
      FocusedButton += _gridColumns;
      LoadHoverImage(FocusedButton);
      return true;
    }

    private void LoadHoverImage(int position)
    {
      _hoverImage = null;
      if ((position < 0) || (position >= _hoverList.Count))
      {
        return;
      }

      foreach (GUIAnimation hover in _hoverList)
      {
        if (hover.GetID == _buttonList[position].GetID)
        {
          _hoverImage = hover;
          _hoverImage.Begin();
          break;
        }
      }
    }

    #endregion

  }
}