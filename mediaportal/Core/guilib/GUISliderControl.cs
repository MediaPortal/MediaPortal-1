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
using System.Collections;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implementation of a slider control.
  /// </summary>
  public class GUISliderControl : GUIControl
  {
    [XMLSkinElement("textureSliderBar")] protected string _backgroundTextureName;
    [XMLSkinElement("textureSliderNib")] protected string _sliderTextureName;
    [XMLSkinElement("textureSliderNibFocus")] protected string _sliderFocusTextureName;
    [XMLSkinElement("font")] protected string _valueFont = "";
    [XMLSkinElement("showrange")] protected bool _showValue = true;
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;

    [XMLSkinElement("spintype")] private GUISpinControl.SpinType _spinType =
      GUISpinControl.SpinType.SPIN_CONTROL_TYPE_TEXT;

    protected int _percentage = 0;
    protected int _intStartValue = 0;
    protected int _intEndValue = 100;
    protected float _floatStartValue = 0.0f;
    protected float _floatEndValue = 1.0f;
    protected int _intValue = 0;
    protected float _floatValue = 0.0f;
    protected float _floatInterval = 0.1f;
    protected GUIAnimation _imageBackGround = null;
    protected GUIAnimation _imageMid = null;
    protected GUIAnimation _imageMidFocus = null;

    public GUISliderControl(int dwParentID)
      : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUISliderControl.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strBackGroundTexture">The background texture of the </param>
    /// <param name="strMidTexture">The unfocused texture.</param>
    /// <param name="strMidTextureFocus">The focused texture</param>
    /// <param name="iType">The type of control.</param>
    public GUISliderControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                            string strBackGroundTexture, string strMidTexture, string strMidTextureFocus,
                            GUISpinControl.SpinType iType)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _backgroundTextureName = strBackGroundTexture;
      _sliderTextureName = strMidTexture;
      _sliderFocusTextureName = strMidTextureFocus;
      _spinType = iType;
      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageBackGround = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _backgroundTextureName);
      _imageBackGround.ParentControl = this;
      _imageBackGround.DimColor = DimColor;

      _imageMid = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                       _sliderTextureName);
      _imageMid.ParentControl = this;
      _imageMid.DimColor = DimColor;

      _imageMidFocus = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                            _sliderFocusTextureName);
      _imageMidFocus.ParentControl = this;
      _imageMidFocus.DimColor = DimColor;
    }


    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }
      string strValue = "";
      float fRange = 0.0f;
      float fPos = 0.0f;
      float fPercent = 0.0f;
      float fTextWidth = 0, fTextHeight = 0;
      GUIFont _font = GUIFontManager.GetFont(_valueFont);
      switch (_spinType)
      {
          // Float based slider
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
          if (null != _font && _showValue)
          {
            strValue = String.Format("{0}", _floatValue);
            _font.DrawText((float)_positionX, (float)_positionY,
                           _textColor, strValue, Alignment.ALIGN_LEFT, -1);
            _font.GetTextExtent(strValue, ref fTextWidth, ref fTextHeight);
            _imageBackGround.SetPosition(_positionX + (int)fTextWidth + 10, _positionY);
          }

          fRange = (float)(_floatEndValue - _floatStartValue);
          fPos = (float)(_floatValue - _floatStartValue);
          fPercent = (fPos / fRange) * 100.0f;
          _percentage = (int)fPercent;
          break;

          // Integer based slider
        case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
          if (null != _font && _showValue)
          {
            strValue = String.Format("{0}/{1}", _intValue, _intEndValue);
            _font.DrawText((float)_positionX, (float)_positionY,
                           _textColor, strValue, Alignment.ALIGN_LEFT, -1);
            _font.GetTextExtent(strValue, ref fTextWidth, ref fTextHeight);
            _imageBackGround.SetPosition(_positionX + (int)fTextWidth + 10, _positionY);
          }

          fRange = (float)(_intEndValue - _intStartValue);
          fPos = (float)(_intValue - _intStartValue);
          _percentage = (int)((fPos / fRange) * 100.0f);
          break;
      }

      //int iHeight=25;
      _imageBackGround.Render(timePassed);
      //_imageBackGround.SetHeight(iHeight);
      //_height = _imageBackGround.Height;
      //_width = _imageBackGround.Width + (int)fTextWidth + 10;

      float fWidth = (float)(_imageBackGround.TextureWidth - _imageMid.TextureWidth); //-20.0f;

      fPos = (float)_percentage;
      fPos /= 100.0f;
      fPos *= fWidth;
      fPos += (float)_imageBackGround.XPosition;
      //fPos += 10.0f;
      if ((int)fWidth > 1)
      {
        if (Focus)
        {
          _imageMidFocus.SetPosition((int)fPos, _imageBackGround.YPosition);
          _imageMidFocus.Render(timePassed);
        }
        else
        {
          _imageMid.SetPosition((int)fPos, _imageBackGround.YPosition);
          _imageMid.Render(timePassed);
        }
      }
      base.Render(timePassed);
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      GUIMessage message;

      switch (action.wID)
      {
          /* mouse handling not working
        case Action.ActionType.ACTION_MOUSE_CLICK:
          float x = (float)action.fAmount1 - _imageBackGround.XPosition;
          if (x < 0)
          {
            x = 0;
          }
          if (x > _imageBackGround.RenderWidth)
          {
            x = _imageBackGround.RenderWidth;
          }
          x /= (float)_imageBackGround.RenderWidth;
          float total, pos;
          switch (_spinType)
          {
            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
              total = _floatEndValue - _floatStartValue;
              pos = (x * total);
              _floatValue = _floatStartValue + pos;
              _floatValue = (float)Math.Round(_floatValue, 1);
              break;

            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
              float start = _intStartValue;
              float end = _intEndValue;
              total = end - start;
              pos = (x * total);
              _intValue = _intStartValue + (int)pos;
              break;

            default:
              _percentage = (int)(100f * x);
              break;
          }
          _floatValue = (float)Math.Round(_floatValue, 1);
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
          break;
        */

          // decrease the slider value
        case Action.ActionType.ACTION_MOVE_LEFT:
          switch (_spinType)
          {
            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
              if (_floatValue > _floatStartValue)
              {
                _floatValue -= _floatInterval;
              }
              break;

            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
              if (_intValue > _intStartValue)
              {
                _intValue--;
              }
              break;

            default:
              if (_percentage > 0)
              {
                _percentage--;
              }
              break;
          }
          _floatValue = (float)Math.Round(_floatValue, 1);
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
          break;

          // increase the slider value
        case Action.ActionType.ACTION_MOVE_RIGHT:
          switch (_spinType)
          {
            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
              if (_floatValue < _floatEndValue)
              {
                _floatValue += _floatInterval;
              }
              break;

            case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
              if (_intValue < _intEndValue)
              {
                _intValue++;
              }
              break;

            default:
              if (_percentage < 100)
              {
                _percentage++;
              }
              break;
          }
          _floatValue = (float)Math.Round(_floatValue, 1);
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
          break;

        default:
          base.OnAction(action);
          break;
      }
    }

    /// <summary>
    /// OnMessage() This method gets called when there's a new message. 
    /// Controls send messages to notify their parents about their state (changes)
    /// By overriding this method a control can respond to the messages of its controls
    /// </summary>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        switch (message.Message)
        {
            // Move the slider to a certain position
          case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
            Percentage = message.Param1;
            return true;

            // Reset the slider
          case GUIMessage.MessageType.GUI_MSG_LABEL_RESET:
            {
              Percentage = 0;
              return true;
            }
          case GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED:
            {
              message.Param1 = Percentage;
              return true;
            }
        }
      }
      return base.OnMessage(message);
    }

    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (value != IsFocused)
        {
          if (value == true)
          {
            if (_imageMidFocus != null)
              _imageMidFocus.Begin();
          }
          else
          {
            if (_imageMid != null)
              _imageMid.Begin();
          }
        }
        base.Focus = value;
      }
    }

    public override bool InControl(int x, int y, out int iControlId)
    {
      iControlId = GetID;
      if (x >= _imageBackGround.XPosition && x <= _imageBackGround.XPosition + _imageBackGround.TextureWidth)
      {
        if (y >= _imageBackGround.YPosition && y <= _imageBackGround.YPosition + _imageBackGround.TextureHeight)
        {
          return true;
        }
      }
      return false;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      if (x >= _imageBackGround.XPosition && x <= _imageBackGround.XPosition + _imageBackGround.TextureWidth)
      {
        if (y >= _imageBackGround.YPosition && y <= _imageBackGround.YPosition + _imageBackGround.TextureHeight)
        {
          return true;
        }
      }
      Focus = false;
      return false;
    }

    public override bool CanFocus()
    {
      if (!IsVisible)
      {
        return false;
      }
      if (Disabled)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Get/set the percentage the slider indicates.
    /// </summary>
    public int Percentage
    {
      get { return _percentage; }
      set
      {
        if (value >= 0 && value <= 100)
        {
          _percentage = value;
        }
      }
    }

    /// <summary>
    /// Get/set the integer value of the slider.
    /// </summary>
    public int IntValue
    {
      get { return _intValue; }
      set
      {
        if (value >= _intStartValue && value <= _intEndValue)
        {
          _intValue = value;
        }
      }
    }

    /// <summary>
    /// Get/set the float value of the slider.
    /// </summary>
    public float FloatValue
    {
      get { return _floatValue; } // <--(I think this was intended) changed from: get { return _floatInterval; } 
      set
      {
        if (value >= _floatStartValue && value <= _floatEndValue)
        {
          _floatValue = value; // <--(I think this was intended) changed from: _floatInterval = value;
        }
      }
    }

    /// <summary>
    /// Get/Set the spintype of the control.
    /// </summary>
    public GUISpinControl.SpinType SpinType
    {
      get { return _spinType; }
      set { _spinType = value; }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageBackGround.PreAllocResources();
      _imageMid.PreAllocResources();
      _imageMidFocus.PreAllocResources();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _imageBackGround.AllocResources();
      _imageMid.AllocResources();
      _imageMidFocus.AllocResources();
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageBackGround.FreeResources();
      _imageMid.FreeResources();
      _imageMidFocus.FreeResources();
    }

    /// <summary>
    /// Sets the integer range of the slider.
    /// </summary>
    /// <param name="iStart">Start point</param>
    /// <param name="iEnd">End point</param>
    public void SetRange(int iStart, int iEnd)
    {
      if (iEnd > iStart)
      {
        _intStartValue = iStart;
        _intEndValue = iEnd;
      }
    }

    /// <summary>
    /// Sets the float range of the slider.
    /// </summary>
    /// <param name="fStart">Start point</param>
    /// <param name="fEnd">End point</param>
    public void SetFloatRange(float fStart, float fEnd)
    {
      if (fEnd > _floatStartValue)
      {
        _floatStartValue = fStart;
        _floatEndValue = fEnd;
      }
    }

    /// <summary>
    /// Get/set the interval for the float. 
    /// </summary>
    public float FloatInterval
    {
      get { return _floatInterval; }
      set { _floatInterval = value; }
    }

    /// <summary>
    /// Get the name of the background texture.
    /// </summary>
    public string BackGroundTextureName
    {
      get { return _imageBackGround.FileName; }
    }

    /// <summary>
    /// Get the name of the middle texture.
    /// </summary>
    public string BackTextureMidName
    {
      get { return _imageMid.FileName; }
    }

    /// <summary>
    /// Get the name of the middle texture when the control has the focus
    /// </summary>
    public string BackTextureMidNameFocus
    {
      get { return _imageMidFocus.FileName; }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>		
    protected override void Update()
    {
      _imageBackGround.SetPosition(XPosition, YPosition);
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageBackGround != null)
        {
          _imageBackGround.DimColor = value;
        }
        if (_imageMid != null)
        {
          _imageMid.DimColor = value;
        }
        if (_imageMidFocus != null)
        {
          _imageMidFocus.DimColor = value;
        }
      }
    }
  }
}