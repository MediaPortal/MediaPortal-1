#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for GUIExpandingGroup.
  /// </summary>
  public class GUIActionGroup : GUIGroup
  {
    #region skin Properties

    [XMLSkinElement("textureFocus")] protected string _focusedTextureName = string.Empty;
    [XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = string.Empty;
    [XMLSkinElement("buttonX")] protected int _buttonX = 0;
    [XMLSkinElement("buttonY")] protected int _buttonY = 0;
    [XMLSkinElement("buttonwidth")] protected int _buttonWidth = 0;
    [XMLSkinElement("buttonheight")] protected int _buttonHeight = 0;
    [XMLSkinElement("defaultcontrol")] protected int _defaultcontrol = -1;
    [XMLSkinElement("onexit")] protected int _exitcontrol = -1;
    [XMLSkinElement("allowoverlay")] protected string _allowOverlayString = string.Empty;

    #endregion

    public enum NextButtonStates
    {
      Activation,
      Outside,
      Deaktivation,
      Outside2,
    }

    #region Properties

    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;
    protected NextButtonStates _buttonState = NextButtonStates.Activation;
    protected bool _isOverlayAllowed = false;
    protected bool _isWinOverlayAllowed = false;
    protected GUIWindow _parentWin = null;

    #endregion

    #region Constructors

    public GUIActionGroup(int parentId) : base(parentId) {}

    #endregion Constructors

    #region Methods

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageFocused = LoadAnimationControl(_parentControlId, _controlId, _buttonX, _buttonY, _buttonWidth, _buttonHeight,
                                           _focusedTextureName);
      _imageFocused.ParentControl = this;
      _imageFocused.Filtering = false;
      _imageFocused.DimColor = DimColor;
      _imageFocused.ColourDiffuse = ColourDiffuse;

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _buttonX, _buttonY, _buttonWidth,
                                              _buttonHeight, _nonFocusedTextureName);
      _imageNonFocused.ParentControl = null; // null, because to avoid dimming with the parentControl
      _imageNonFocused.Filtering = false;
      _imageNonFocused.DimColor = DimColor;
      _imageNonFocused.ColourDiffuse = ColourDiffuse;

      _isOverlayAllowed = false;
      if ((_allowOverlayString != null) && (_allowOverlayString.Length > 0))
      {
        _allowOverlayString = _allowOverlayString.ToLower();
        if (_allowOverlayString.Equals("yes") || _allowOverlayString.Equals("true"))
        {
          _isOverlayAllowed = true;
        }
      }
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleRectToScreenResolution(ref _buttonX, ref _buttonY, ref _buttonWidth, ref _buttonHeight);
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _imageFocused.AllocResources();
      _imageNonFocused.AllocResources();
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _imageFocused.SafeDispose();
      _imageNonFocused.SafeDispose();
    }

    public override void OnInit()
    {
      base.OnInit();
      // ensure that we are always the last entry in the GUIWindow.Children 
      // -> HitTest always starts from the last entry
      _parentWin = GUIWindowManager.GetWindow(_windowId);
      if (_parentWin != null)
      {
        _parentWin.Children.Remove(this);
        _parentWin.Children.Add(this);
        _isWinOverlayAllowed = _parentWin.IsOverlayAllowed;
      }
      SetDefaultControl();
      _buttonState = NextButtonStates.Activation;
    }

    protected void SetDefaultControl()
    {
      if (_defaultcontrol > 0)
      {
        return;
      }
      foreach (GUIControl cntl in Children)
      {
        if ((cntl != null) && (cntl.GetID > 0))
        {
          _defaultcontrol = cntl.GetID;
          break;
        }
      }
    }

    public override void OnDeInit()
    {
      base.OnDeInit();
      _buttonState = NextButtonStates.Activation;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      switch (_buttonState)
      {
        case NextButtonStates.Activation:
          if (Dimmed == false) // let us check if something changed by keyboard input
          {
            _buttonState = NextButtonStates.Deaktivation;
            focused = !Dimmed;
            controlID = _defaultcontrol;
            return true;
          }
          if (_imageFocused.HitTest(x, y, out controlID, out focused))
          {
            focused = !Dimmed;
            controlID = _defaultcontrol;
            _buttonState = NextButtonStates.Outside;
            return true;
          }
          return false;

        case NextButtonStates.Outside:
          if (Dimmed == true) // let us check if something changed by keyboard input
          {
            _buttonState = NextButtonStates.Activation;
            focused = !Dimmed;
            controlID = -1;
            return false;
          }
          if (!_imageFocused.HitTest(x, y, out controlID, out focused))
          {
            _buttonState = NextButtonStates.Deaktivation;
          }
          break;

        case NextButtonStates.Deaktivation:
          if (Dimmed == true) // let us check if something changed by keyboard input
          {
            _buttonState = NextButtonStates.Activation;
            focused = !Dimmed;
            controlID = -1;
            return false;
          }
          if (_imageFocused.HitTest(x, y, out controlID, out focused))
          {
            GUIMessage msg;
            if (_exitcontrol == -1)
            {
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LOSTFOCUS, _windowId, 0, GetFocusControlId(), 0, 0,
                                   null);
            }
            else
            {
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, _windowId, 0, _exitcontrol, 0, 0, null);
            }
            GUIWindow win = GUIWindowManager.GetWindow(_windowId);
            if (win != null)
            {
              win.OnMessage(msg);
            }
            controlID = -1;
            _buttonState = NextButtonStates.Outside2;
            return false;
          }
          break;

        case NextButtonStates.Outside2:
          if (Dimmed == false) // let us check if something changed by keyboard input
          {
            _buttonState = NextButtonStates.Deaktivation;
            focused = !Dimmed;
            controlID = _defaultcontrol;
            return true;
          }
          if (!_imageFocused.HitTest(x, y, out controlID, out focused))
          {
            _buttonState = NextButtonStates.Activation;
          }
          return false;
      }
      if (base.HitTest(x, y, out controlID, out focused))
      {
        return true;
      }

      int posX = Math.Min(_buttonX, _positionX);
      int posY = Math.Min(_buttonY, _positionY);
      int width = Math.Max(_buttonWidth, _width);
      int height = Math.Max(_buttonHeight, _height);
      focused = !Dimmed;
      controlID = GetFocusControlId();

      if (controlID == -1)
      {
        controlID = _defaultcontrol;
      }
      if ((x >= posX) && (x <= posX + width) && (y >= posY) && (y <= posY + height))
      {
        return true;
      }
      return false;
    }

    public override void Render(float timePassed)
    {
      //base.Render(timePassed);
      if (!Dimmed)
      {
        if (GUIGraphicsContext.Overlay != _isOverlayAllowed)
        {
          GUIGraphicsContext.Overlay = _parentWin.IsOverlayAllowed = _isOverlayAllowed;
        }
        _imageFocused.Render(timePassed);
        GUIFontManager.Present();
      }
      else
      {
        _imageNonFocused.Render(timePassed);
      }
      base.Render(timePassed);
    }

    #endregion
  }
}