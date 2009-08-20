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
using System.Collections;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISpinControl : GUIControl
  {
    public enum SpinType //:int
    {
      SPIN_CONTROL_TYPE_INT,
      SPIN_CONTROL_TYPE_FLOAT,
      SPIN_CONTROL_TYPE_TEXT,
      SPIN_CONTROL_TYPE_DISC_NUMBER,

      // needed for XAML parser
      Int = SPIN_CONTROL_TYPE_INT,
      Float = SPIN_CONTROL_TYPE_FLOAT,
      Text = SPIN_CONTROL_TYPE_TEXT,
      Disc = SPIN_CONTROL_TYPE_DISC_NUMBER,
    } ;

    public enum SpinSelect
    {
      SPIN_BUTTON_DOWN,
      SPIN_BUTTON_UP
    } ;

    [XMLSkinElement("showrange")] protected bool _showRange = true;
    [XMLSkinElement("digits")] protected int _digits = -1;
    [XMLSkinElement("reverse")] protected bool _reverse = false;
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("textureUp")] protected string _upTextureName;
    [XMLSkinElement("textureDown")] protected string _downTextureName;
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus;
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus;
    [XMLSkinElement("align")] protected Alignment _alignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("spintype")] protected SpinType _spinType = SpinType.SPIN_CONTROL_TYPE_TEXT;
    [XMLSkinElement("orientation")] protected eOrientation _orientation = eOrientation.Horizontal;
    [XMLSkinElement("cycleItems")] protected bool _cycleItems = false;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    [XMLSkinElement("prefixText")] protected string _prefixText = "";
    [XMLSkinElement("suffixText")] protected string _suffixText = "";
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;

    protected bool _autoCheck = true;
    protected int _startInt = 0;
    protected int _endInt = 100;
    protected float _startFloat = 0.0f;
    protected float _endFloat = 1.0f;
    protected int _intValue = 0;
    protected float _floatValue = 0.0f;

    protected SpinSelect _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
    protected float _floatInterval = 0.1f;
    protected ArrayList _listLabels = new ArrayList();
    protected ArrayList _listValues = new ArrayList();
    protected GUIAnimation _imageSpinUp = null;
    protected GUIAnimation _imageSpinDown = null;
    protected GUIAnimation _imageSpinUpFocus = null;
    protected GUIAnimation _imageSpinDownFocus = null;


    protected GUIFont _font = null;
    protected string _typed = "";
    private GUILabelControl _labelControl = null;

    // Used to allow classes that encapsulate this class to detect handled actions.
    private bool _actionHandled = false;

    public GUISpinControl(int dwParentID)
      : base(dwParentID)
    {
    }

    public GUISpinControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                          string strUp, string strDown, string strUpFocus, string strDownFocus, string strFont,
                          long dwTextColor, SpinType iType, Alignment dwAlign)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _textColor = dwTextColor;
      _fontName = strFont;
      _alignment = dwAlign;
      _spinType = iType;

      _downTextureName = strDown;
      _upTextureName = strUp;
      _upTextureNameFocus = strUpFocus;
      _downTextureNameFocus = strDownFocus;

      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageSpinUp = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                          _upTextureName);
      _imageSpinUp.ParentControl = this;
      _imageSpinUp.DimColor = DimColor;

      _imageSpinDown = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                            _downTextureName);
      _imageSpinDown.ParentControl = this;
      _imageSpinDown.DimColor = DimColor;

      _imageSpinUpFocus = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                               _upTextureNameFocus);
      _imageSpinUpFocus.ParentControl = this;
      _imageSpinUpFocus.DimColor = DimColor;

      _imageSpinDownFocus = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                                 _downTextureNameFocus);
      _imageSpinDownFocus.ParentControl = this;
      _imageSpinDownFocus.DimColor = DimColor;

      _imageSpinUp.Filtering = false;
      _imageSpinDown.Filtering = false;
      _imageSpinUpFocus.Filtering = false;
      _imageSpinDownFocus.Filtering = false;
      _labelControl = new GUILabelControl(_parentControlId);
      _labelControl.CacheFont = true;
      _labelControl.ParentControl = this;
      _labelControl.DimColor = DimColor;
      _labelControl.SetShadow(_shadowAngle, _shadowDistance, _shadowColor);
    }

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          _typed = string.Empty;
          base.Render(timePassed);
          return;
        }
      }
      if (!Focus)
      {
        _typed = string.Empty;
      }
      int dwPosX = _positionX;
      string wszText;

      if (_spinType == SpinType.SPIN_CONTROL_TYPE_INT)
      {
        string strValue = _intValue.ToString();
        if (_digits > 1)
        {
          while (strValue.Length < _digits)
          {
            strValue = "0" + strValue;
          }
        }
        if (_showRange)
        {
          wszText = strValue + "/" + _endInt.ToString();
        }
        else
        {
          wszText = strValue.ToString();
        }

        if (_prefixText.Length > 0)
        {
          wszText = _prefixText + " " + wszText;
        }

        if (_suffixText.Length > 0)
        {
          wszText = wszText + " " + _suffixText;
        }
      }
      else if (_spinType == SpinType.SPIN_CONTROL_TYPE_FLOAT)
      {
        wszText = String.Format("{0:2}/{1:2}", _floatValue, _endFloat);

        if (_prefixText.Length > 0)
        {
          wszText = _prefixText + " " + wszText;
        }

        if (_suffixText.Length > 0)
        {
          wszText = wszText + " " + _suffixText;
        }
      }
      else
      {
        wszText = "";
        if (_intValue >= 0 && _intValue < _listLabels.Count)
        {
          if (_showRange)
          {
            wszText = String.Format("({0}/{1}) {2}", _intValue + 1, (int) _listLabels.Count, _listLabels[_intValue]);
          }
          else
          {
            wszText = (string) _listLabels[_intValue];
          }
        }
        else
        {
          String.Format("?{0}?", _intValue);
        }
      }

      int iTextXPos = _positionX;
      int iTextYPos = _positionY;
      if (_alignment == Alignment.ALIGN_LEFT)
      {
        if (_font != null)
        {
          if (wszText != null && wszText.Length > 0)
          {
            float fTextHeight = 0, fTextWidth = 0;
            _font.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);
            if (Orientation == eOrientation.Horizontal)
            {
              _imageSpinUpFocus.SetPosition((int) fTextWidth + 5 + dwPosX + _imageSpinDown.Width, _positionY);
              _imageSpinUp.SetPosition((int) fTextWidth + 5 + dwPosX + _imageSpinDown.Width, _positionY);
              _imageSpinDownFocus.SetPosition((int) fTextWidth + 5 + dwPosX, _positionY);
              _imageSpinDown.SetPosition((int) fTextWidth + 5 + dwPosX, _positionY);
            }
            else
            {
              _imageSpinUpFocus.SetPosition((int) fTextWidth + 5 + dwPosX, _positionY - (Height/2));
              _imageSpinUp.SetPosition((int) fTextWidth + 5 + dwPosX, _positionY - (Height/2));
              _imageSpinDownFocus.SetPosition((int) fTextWidth + 5 + dwPosX, _positionY + (Height/2));
              _imageSpinDown.SetPosition((int) fTextWidth + 5 + dwPosX, _positionY + (Height/2));
            }
          }
        }
      }
      if (_alignment == Alignment.ALIGN_CENTER)
      {
        if (_font != null)
        {
          float fTextHeight = 1, fTextWidth = 1;
          if (wszText != null && wszText.Length > 0)
          {
            _font.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);
          }
          if (Orientation == eOrientation.Horizontal)
          {
            iTextXPos = dwPosX + _imageSpinUp.Width;
            iTextYPos = _positionY;
            _imageSpinDownFocus.SetPosition((int) dwPosX, _positionY);
            _imageSpinDown.SetPosition((int) dwPosX, _positionY);
            _imageSpinUpFocus.SetPosition((int) fTextWidth + _imageSpinUp.Width + dwPosX, _positionY);
            _imageSpinUp.SetPosition((int) fTextWidth + _imageSpinUp.Width + dwPosX, _positionY);

            fTextHeight /= 2.0f;
            float fPosY = ((float) _height)/2.0f;
            fPosY -= fTextHeight;
            fPosY += (float) iTextYPos;
            iTextYPos = (int) fPosY;
          }
          else
          {
            iTextXPos = dwPosX;
            iTextYPos = _positionY + Height;
            _imageSpinUpFocus.SetPosition((int) +dwPosX, _positionY - (Height + (int) fTextHeight)/2);
            _imageSpinUp.SetPosition((int) dwPosX, _positionY - (Height + (int) fTextHeight)/2);
            _imageSpinDownFocus.SetPosition((int) dwPosX, _positionY + (Height + (int) fTextHeight)/2);
            _imageSpinDown.SetPosition((int) dwPosX, _positionY + (Height + (int) fTextHeight)/2);
          }
        }
      }

      if (_spinSelect == SpinSelect.SPIN_BUTTON_UP)
      {
        if (_reverse)
        {
          if (!CanMoveDown())
          {
            _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
          }
        }
        else
        {
          if (!CanMoveUp())
          {
            _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
          }
        }
      }

      if (_spinSelect == SpinSelect.SPIN_BUTTON_DOWN)
      {
        if (_reverse)
        {
          if (!CanMoveUp())
          {
            _spinSelect = SpinSelect.SPIN_BUTTON_UP;
          }
        }
        else
        {
          if (!CanMoveDown())
          {
            _spinSelect = SpinSelect.SPIN_BUTTON_UP;
          }
        }
      }

      if (Focus)
      {
        bool bShow = CanMoveUp();
        if (_reverse)
        {
          bShow = CanMoveDown();
        }

        if (_spinSelect == SpinSelect.SPIN_BUTTON_UP && bShow)
        {
          _imageSpinUpFocus.Render(timePassed);
        }
        else
        {
          _imageSpinUp.Render(timePassed);
        }

        bShow = CanMoveDown();
        if (_reverse)
        {
          bShow = CanMoveUp();
        }
        if (_spinSelect == SpinSelect.SPIN_BUTTON_DOWN && bShow)
        {
          _imageSpinDownFocus.Render(timePassed);
        }
        else
        {
          _imageSpinDown.Render(timePassed);
        }
      }
      else
      {
        _imageSpinUp.Render(timePassed);
        _imageSpinDown.Render(timePassed);
      }

      if (_font != null)
      {
        _labelControl.FontName = _fontName;
        _labelControl.TextColor = _textColor;
        _labelControl.Label = wszText;

        if (Disabled)
        {
          _labelControl.TextColor &= 0x80ffffff;
        }
        if (_alignment != Alignment.ALIGN_CENTER)
        {
          if (wszText != null && wszText.Length > 0)
          {
            _labelControl.TextAlignment = _alignment;
            float fHeight = (float) _labelControl.TextHeight;
            fHeight /= 2.0f;
            float fPosY = ((float) _height)/2.0f;
            fPosY -= fHeight;
            fPosY += (float) _positionY;

            // This offset positioning preserves the legacy behavior of a (hardcoded) 3 pixel offset.
            int sign = 1;
            if (_alignment == Alignment.ALIGN_RIGHT)
            {
              sign = -1;
          }
            _labelControl.SetPosition(_positionX - 3 + (_textOffsetX * sign), (int) fPosY + _textOffsetY);
        }
        }
        else
        {
          _labelControl.SetPosition(iTextXPos + _textOffsetX, iTextYPos + _textOffsetY);
          _labelControl.TextAlignment = Alignment.ALIGN_LEFT;
        }
        _labelControl.Render(timePassed);
      }
      base.Render(timePassed);
    }

    public override void OnAction(Action action)
    {
      _actionHandled = false;
      switch (action.wID)
      {
        case Action.ActionType.REMOTE_0:
        case Action.ActionType.REMOTE_1:
        case Action.ActionType.REMOTE_2:
        case Action.ActionType.REMOTE_3:
        case Action.ActionType.REMOTE_4:
        case Action.ActionType.REMOTE_5:
        case Action.ActionType.REMOTE_6:
        case Action.ActionType.REMOTE_7:
        case Action.ActionType.REMOTE_8:
        case Action.ActionType.REMOTE_9:
          {
            if (((_digits == -1) && (_typed.Length >= 3)) ||
                ((_digits != -1) && (_typed.Length >= _digits)))
            {
              _typed = "";
            }
            int iNumber = action.wID - Action.ActionType.REMOTE_0;

            _typed += (char) (iNumber + '0');
            int iValue;
            iValue = Int32.Parse(_typed);
            switch (_spinType)
            {
              case SpinType.SPIN_CONTROL_TYPE_INT:
                {
                  // Value entered
                  if (((_digits == -1) && (_typed.Length >= 3)) ||
                      ((_digits != -1) && (_typed.Length >= _digits)))
                  {
                    // Check value
                    if (iValue < _startInt)
                    {
                      iValue = _startInt;
                    }
                    _typed = iValue.ToString();
                  }

                  if (iValue > _endInt)
                  {
                    _typed = "";
                    _typed += (char) (iNumber + '0');
                    iValue = Int32.Parse(_typed);
                  }

                  _intValue = iValue;
                  if (_intValue >= _startInt && _intValue <= _endInt)
                  {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0,
                                                    0, null);
                    GUIGraphicsContext.SendMessage(msg);
                  }
                }
                break;

              case SpinType.SPIN_CONTROL_TYPE_TEXT:
                {
                  if (iValue < 0 || iValue >= _listLabels.Count)
                  {
                    iValue = 0;
                  }

                  _intValue = iValue;
                  if (_intValue >= 0 && _intValue < _listLabels.Count)
                  {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0,
                                                    0, null);
                    msg.Label = (string) _listLabels[_intValue];
                    GUIGraphicsContext.SendMessage(msg);
                  }
                }
                break;

              case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
                {
                  if (iValue < 0 || iValue >= _listLabels.Count)
                  {
                    iValue = 0;
                  }

                  _intValue = iValue + 1;
                  if (_intValue >= 1 && _intValue < _listLabels.Count)
                  {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0,
                                                    0, null);
                    msg.Label = (string) _listLabels[_intValue];
                    GUIGraphicsContext.SendMessage(msg);
                  }
                }
                break;
            }
          }
          break;
      }
      if (action.wID == Action.ActionType.ACTION_PAGE_UP)
      {
        if (!_reverse)
        {
          PageDown();
        }
        else
        {
          PageUp();
        }
        _actionHandled = true;
        return;
      }
      if (action.wID == Action.ActionType.ACTION_PAGE_DOWN)
      {
        if (!_reverse)
        {
          PageUp();
        }
        else
        {
          PageDown();
        }
        _actionHandled = true;
        return;
      }
      if (action.wID == Action.ActionType.ACTION_MOVE_UP ||
          action.wID == Action.ActionType.ACTION_MOVE_DOWN ||
          action.wID == Action.ActionType.ACTION_MOVE_LEFT ||
          action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
      {
        bool bUp = false;
        bool bDown = false;
        if (Orientation == eOrientation.Horizontal && action.wID == Action.ActionType.ACTION_MOVE_LEFT)
        {
          bUp = true;
        }
        if (Orientation == eOrientation.Vertical && action.wID == Action.ActionType.ACTION_MOVE_DOWN)
        {
          bUp = true;
        }
        if (bUp)
        {
          if (_spinSelect == SpinSelect.SPIN_BUTTON_UP)
          {
            if (_reverse)
            {
              if (CanMoveUp())
              {
                // Only a change in _spinSelect state defines a handled action.
                _actionHandled = _spinSelect != SpinSelect.SPIN_BUTTON_DOWN;
                _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
                return;
              }
            }
            else
            {
              if (CanMoveDown())
              {
                // Only a change in _spinSelect state defines a handled action.
                _actionHandled = _spinSelect != SpinSelect.SPIN_BUTTON_DOWN;
                _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
                return;
              }
            }
          }
        }
        if (Orientation == eOrientation.Horizontal && action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
        {
          bDown = true;
        }
        if (Orientation == eOrientation.Vertical && action.wID == Action.ActionType.ACTION_MOVE_UP)
        {
          bDown = true;
        }

        if (bDown)
        {
          if (_spinSelect == SpinSelect.SPIN_BUTTON_DOWN)
          {
            if (_reverse)
            {
              if (CanMoveDown())
              {
                // Only a change in _spinSelect state defines a handled action.
                _actionHandled = _spinSelect != SpinSelect.SPIN_BUTTON_UP;
                _spinSelect = SpinSelect.SPIN_BUTTON_UP;
                return;
              }
            }
            else
            {
              if (CanMoveUp())
              {
                // Only a change in _spinSelect state defines a handled action.
                _actionHandled = _spinSelect != SpinSelect.SPIN_BUTTON_UP;
                _spinSelect = SpinSelect.SPIN_BUTTON_UP;
                return;
              }
            }
          }
        }
        // causes issues with tvguide date control
        //        Focus = false;   // if not handeled before -> loose focus to go back to main list
      }
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (_spinSelect == SpinSelect.SPIN_BUTTON_UP)
          {
            if (_reverse)
            {
              MoveDown();
            }
            else
            {
              MoveUp();
            }
            action.wID = Action.ActionType.ACTION_INVALID;
            _actionHandled = true;
            return;
          }
          if (_spinSelect == SpinSelect.SPIN_BUTTON_DOWN)
          {
            if (_reverse)
            {
              MoveUp();
            }
            else
            {
              MoveDown();
            }
            action.wID = Action.ActionType.ACTION_INVALID;
            _actionHandled = true;
            return;
          }
        }
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (base.OnMessage(message))
      {
        if (!Focus)
        {
          _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
        }
        else if (message.Param1 == (int) SpinSelect.SPIN_BUTTON_UP)
        {
          _spinSelect = SpinSelect.SPIN_BUTTON_UP;
        }
        else
        {
          _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
        }
        return true;
      }
      if (message.TargetControlId == GetID)
      {
        switch (message.Message)
        {
          case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
            Value = (int) message.Param1;
            return true;


          case GUIMessage.MessageType.GUI_MSG_LABEL_RESET:
            {
              _listLabels.Clear();
              _listValues.Clear();
              Value = 0;
              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_SHOWRANGE:
            if (message.Param1 != 0)
            {
              _showRange = true;
            }
            else
            {
              _showRange = false;
            }
            break;

          case GUIMessage.MessageType.GUI_MSG_LABEL_ADD:
            {
              AddLabel(message.Label, (int) message.Param1);
              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED:
            {
              message.Param1 = (int) Value;
              message.Param2 = (int) _spinSelect;

              if (_spinType == SpinType.SPIN_CONTROL_TYPE_TEXT || _spinType == SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER)
              {
                if (_intValue >= 0 && _intValue < _listLabels.Count)
                {
                  message.Label = (string) _listLabels[_intValue];
                }

                if (_intValue >= 0 && _intValue < _listValues.Count)
                {
                  message.Param1 = (int) _listValues[_intValue];
                }
              }
              return true;
            }
        }
      }
      return false;
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageSpinUp.PreAllocResources();
      _imageSpinUpFocus.PreAllocResources();
      _imageSpinDown.PreAllocResources();
      _imageSpinDownFocus.PreAllocResources();
    }

    public override void AllocResources()
    {
      base.AllocResources();
      _imageSpinUp.AllocResources();
      _imageSpinUpFocus.AllocResources();
      _imageSpinDown.AllocResources();
      _imageSpinDownFocus.AllocResources();

      _font = GUIFontManager.GetFont(_fontName);
      SetPosition(_positionX, _positionY);

      if (SubItemCount > 0)
      {
        _spinType = SpinType.SPIN_CONTROL_TYPE_TEXT;
        _listLabels.Clear();
        _listValues.Clear();
        for (int i = 0; i < SubItemCount; ++i)
        {
          string subitem = (string) GetSubItem(i);

          _listLabels.Add(subitem);
          _listValues.Add(i);
        }
      }
    }

    public override void FreeResources()
    {
      base.FreeResources();
      _imageSpinUp.FreeResources();
      _imageSpinUpFocus.FreeResources();
      _imageSpinDown.FreeResources();
      _imageSpinDownFocus.FreeResources();
      _typed = "";
    }

    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (dwPosX < 0)
      {
        return;
      }
      if (dwPosY < 0)
      {
        return;
      }
      base.SetPosition(dwPosX, dwPosY);

      if (Orientation == eOrientation.Horizontal)
      {
        _imageSpinDownFocus.SetPosition(dwPosX, dwPosY);
        _imageSpinDown.SetPosition(dwPosX, dwPosY);

        _imageSpinUp.SetPosition(_positionX + _imageSpinDown.Width, _positionY);
        _imageSpinUpFocus.SetPosition(_positionX + _imageSpinDownFocus.Width, _positionY);
      }
      else
      {
        _imageSpinUp.SetPosition(_positionX, _positionY + Height/2);
        _imageSpinUpFocus.SetPosition(_positionX, _positionY + Height/2);

        _imageSpinDownFocus.SetPosition(dwPosX, dwPosY - Height/2);
        _imageSpinDown.SetPosition(dwPosX, dwPosY - Height/2);
      }
    }

    public override int Width
    {
      get
      {
        if (Orientation == eOrientation.Horizontal)
        {
          return _imageSpinDown.Width*2;
        }
        else
        {
          return _imageSpinDown.Width;
        }
      }
    }

    public void SetRange(int iStart, int iEnd)
    {
      _startInt = iStart;
      _endInt = iEnd;
    }

    public void SetFloatRange(float fStart, float fEnd)
    {
      _startFloat = fStart;
      _endFloat = fEnd;
    }

    public int Value
    {
      get
      {
        if (_intValue < _startInt)
        {
          _intValue = _startInt;
        }
        if (_intValue > _endInt)
        {
          _intValue = _endInt;
        }
        return _intValue;
      }
      set { _intValue = value; }
    }

    public float FloatValue
    {
      get
      {
        if (_floatValue < _startFloat)
        {
          _floatValue = _startFloat;
        }
        if (_floatValue > _endFloat)
        {
          _floatValue = _endFloat;
        }
        return _floatValue;
      }
      set { _floatValue = value; }
    }

    public void AddLabel(string strLabel, int iValue)
    {
      if (strLabel == null)
      {
        return;
      }
      _listLabels.Add(strLabel);
      _listValues.Add(iValue);
    }

    public void Reset()
    {
      _listLabels.Clear();
      _listValues.Clear();
      Value = 0;
    }

    public string GetLabel()
    {
      if (_intValue < 0 || _intValue >= _listLabels.Count)
      {
        return "";
      }
      string strLabel = (string) _listLabels[_intValue];
      return strLabel;
    }

    public override bool Focus
    {
      get { return base.Focus; }
      set
      {
        if (value != IsFocused)
        {
          if (value == true)
          {
            if (_imageSpinDownFocus != null)
            {
              _imageSpinDownFocus.Begin();
            }
            if (_imageSpinUpFocus != null)
            {
              _imageSpinUpFocus.Begin();
            }
          }
          else
          {
            if (_imageSpinDown != null)
            {
              _imageSpinDown.Begin();
            }
            if (_imageSpinUp != null)
            {
              _imageSpinUp.Begin();
            }
          }
        }
        base.Focus = value;
        if (!IsFocused)
        {
          switch (_spinType)
          {
            case SpinType.SPIN_CONTROL_TYPE_INT:
              if (_intValue < _startInt)
              {
                _intValue = _startInt;
              }
              if (_intValue > _endInt)
              {
                _intValue = _endInt;
              }
              break;

            case SpinType.SPIN_CONTROL_TYPE_TEXT:
            case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
              if (_intValue < 0 || _intValue >= _listLabels.Count)
              {
                _intValue = 0;
              }
              break;

            case SpinType.SPIN_CONTROL_TYPE_FLOAT:
              if (_floatValue < _startFloat)
              {
                _floatValue = _startFloat;
              }
              if (_floatValue > _endFloat)
              {
                _floatValue = _endFloat;
              }
              break;
          }
        }
      }
    }

    public void SetReverse(bool bOnOff)
    {
      _reverse = bOnOff;
    }

    public int GetMaximum()
    {
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          return _endInt;


        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          return (int) _listLabels.Count;

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          return (int) (_endFloat*10.0f);
      }
      return 100;
    }

    public int GetMinimum()
    {
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          return _startInt;


        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          return 1;

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          return (int) (_startFloat*10.0f);
      }
      return 0;
    }

    public string TexutureUpName
    {
      get { return _imageSpinUp.FileName; }
    }

    public string TexutureDownName
    {
      get { return _imageSpinDown.FileName; }
    }

    public string TexutureUpFocusName
    {
      get { return _imageSpinUpFocus.FileName; }
    }

    public string TexutureDownFocusName
    {
      get { return _imageSpinDownFocus.FileName; }
    }

    public long TextColor
    {
      get { return _textColor; }
    }

    public string FontName
    {
      get { return _fontName; }
    }

    public Alignment TextAlignment
    {
      get { return _alignment; }
    }

    public SpinType UpDownType
    {
      get { return _spinType; }
      set { _spinType = value; }
    }

    public int SpinWidth
    {
      get { return _imageSpinUp.Width; }
    }

    public int SpinHeight
    {
      get { return _imageSpinUp.Height; }
    }

    public float FloatInterval
    {
      get { return _floatInterval; }
      set { _floatInterval = value; }
    }

    public bool ShowRange
    {
      get { return _showRange; }
      set { _showRange = value; }
    }

    public int Digits
    {
      get { return _digits; }
      set { _digits = value; }
    }

    protected void PageUp()
    {
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          {
            if (_intValue - 10 >= _startInt)
            {
              _intValue -= 10;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          {
            if (_intValue - 10 >= 0)
            {
              _intValue -= 10;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIGraphicsContext.SendMessage(msg);
            return;
          }
      }
    }

    protected void PageDown()
    {
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          {
            if (_intValue + 10 <= _endInt)
            {
              _intValue += 10;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIGraphicsContext.SendMessage(msg);
            return;
          }
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          {
            if (_intValue + 10 < (int) _listLabels.Count)
            {
              _intValue += 10;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIGraphicsContext.SendMessage(msg);
            return;
          }
      }
    }

    protected bool CanMoveDown()
    {
      if (!AutoCheck || CycleItems)
      {
        return true;
      }
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          {
            if (_intValue + 1 <= _endInt)
            {
              return true;
            }
            return false;
          }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
            if (_floatValue + _floatInterval <= _endFloat)
            {
              return true;
            }
            return false;
          }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          {
            if (_intValue + 1 < (int) _listLabels.Count)
            {
              return true;
            }
            return false;
          }
      }
      return false;
    }

    protected bool CanMoveUp()
    {
      if (!AutoCheck || CycleItems)
      {
        return true;
      }
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          {
            if (_intValue - 1 >= _startInt)
            {
              return true;
            }
            return false;
          }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
            if (_floatValue - _floatInterval >= _startFloat)
            {
              return true;
            }
            return false;
          }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          {
            if (_intValue - 1 >= 0)
            {
              return true;
            }
            return false;
          }
      }
      return false;
    }

    public void MoveUp()
    {
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          {
            if (_intValue - 1 >= _startInt)
            {
              _intValue--;
            }
            else if (_cycleItems)
            {
              _intValue = _endInt;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            msg.Param1 = _intValue;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
            if (_floatValue - _floatInterval >= _startFloat)
            {
              _floatValue -= _floatInterval;
            }
            else if (_cycleItems)
            {
              _floatValue = _endFloat;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIGraphicsContext.SendMessage(msg);
            return;
          }


        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          {
            if (_intValue - 1 >= 0)
            {
              _intValue--;
            }
            else if (_cycleItems)
            {
              _intValue = (_listLabels.Count - 1);
            }

            if (_intValue < _listLabels.Count)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                              null);
              msg.Label = (string) _listLabels[_intValue];
              GUIGraphicsContext.SendMessage(msg);
            }
            return;
          }
      }
    }

    public void MoveDown()
    {
      switch (_spinType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          {
            if (_intValue + 1 <= _endInt)
            {
              _intValue++;
            }
            else if (_cycleItems)
            {
              _intValue = _startInt;
            }

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            msg.Param1 = _intValue;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
            if (_floatValue + _floatInterval <= _endFloat)
            {
              _floatValue += _floatInterval;
            }
            else if (_cycleItems)
            {
              _floatValue = _startFloat;
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                            null);
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        case SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER:
          {
            //int maxNr = (int)_listLabels.Count;

            if (_intValue + 1 < (int) _listLabels.Count)
            {
              _intValue++;
            }
            else if (_cycleItems)
            {
              _intValue = 0;
            }
            if (_intValue < (int) _listLabels.Count)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0,
                                              null);
              msg.Label = (string) _listLabels[_intValue];
              GUIGraphicsContext.SendMessage(msg);
            }
            return;
          }
      }
    }

    public override bool InControl(int x, int y, out int iControlId)
    {
      iControlId = GetID;
      if (x >= _imageSpinUp.XPosition && x <= _imageSpinUp.XPosition + _imageSpinUp.RenderWidth)
      {
        if (y >= _imageSpinUp.YPosition && y <= _imageSpinUp.YPosition + _imageSpinUp.RenderHeight)
        {
          return true;
        }
      }
      if (x >= _imageSpinDown.XPosition && x <= _imageSpinDown.XPosition + _imageSpinDown.RenderWidth)
      {
        if (y >= _imageSpinDown.YPosition && y <= _imageSpinDown.YPosition + _imageSpinDown.RenderHeight)
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
      if (x >= _imageSpinUp.XPosition && x <= _imageSpinUp.XPosition + _imageSpinUp.RenderWidth)
      {
        if (y >= _imageSpinUp.YPosition && y <= _imageSpinUp.YPosition + _imageSpinUp.RenderHeight)
        {
          if (_reverse)
          {
            if (CanMoveDown())
            {
              _spinSelect = SpinSelect.SPIN_BUTTON_UP;
              return true;
            }
          }
          else
          {
            if (CanMoveUp())
            {
              _spinSelect = SpinSelect.SPIN_BUTTON_UP;
              return true;
            }
          }
        }
      }
      if (x >= _imageSpinDown.XPosition && x <= _imageSpinDown.XPosition + _imageSpinDown.RenderWidth)
      {
        if (y >= _imageSpinDown.YPosition && y <= _imageSpinDown.YPosition + _imageSpinDown.RenderHeight)
        {
          if (_reverse)
          {
            if (CanMoveUp())
            {
              _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
              return true;
            }
          }
          else
          {
            if (CanMoveDown())
            {
              _spinSelect = SpinSelect.SPIN_BUTTON_DOWN;
              return true;
            }
          }
        }
      }
      Focus = false;
      return false;
    }

    public eOrientation Orientation
    {
      get { return _orientation; }
      set { _orientation = value; }
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
      if (_spinType == SpinType.SPIN_CONTROL_TYPE_INT)
      {
        if (_startInt == _endInt)
        {
          return false;
        }
      }

      if (_spinType == SpinType.SPIN_CONTROL_TYPE_TEXT || _spinType == SpinType.SPIN_CONTROL_TYPE_DISC_NUMBER)
      {
        if (_listLabels.Count < 2)
        {
          return false;
        }
      }
      return true;
    }

    public bool CycleItems
    {
      get { return _cycleItems; }
      set { _cycleItems = value; }
    }

    public bool AutoCheck
    {
      get { return _autoCheck; }
      set { _autoCheck = value; }
    }

    public SpinSelect SelectedButton
    {
      get { return _spinSelect; }
      set { _spinSelect = value; }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageSpinUp != null)
        {
          _imageSpinUp.DimColor = value;
        }
        if (_imageSpinDown != null)
        {
          _imageSpinDown.DimColor = value;
        }
        if (_imageSpinUpFocus != null)
        {
          _imageSpinUpFocus.DimColor = value;
        }
        if (_imageSpinDownFocus != null)
        {
          _imageSpinDownFocus.DimColor = value;
        }
        if (_labelControl != null)
        {
          _labelControl.DimColor = value;
        }
      }
    }

    public bool ActionHandled
    {
      get { return _actionHandled; }
      set { _actionHandled = value; }
    }

    public void SetShadow(int angle, int distance, long color)
    {
      _shadowAngle = angle;
      _shadowDistance = distance;
      _shadowColor = color;
      _labelControl.SetShadow(_shadowAngle, _shadowDistance, _shadowColor);
    }

    public string PrefixText
    {
      get { return _prefixText; }
      set { _prefixText = value; }
    }

    public string SuffixText
    {
      get { return _suffixText; }
      set { _suffixText = value; }
    }

    public int TextOffsetX
    {
      get { return _textOffsetX; }
      set { _textOffsetX = value; }
    }

    public int TextOffsetY
    {
      get { return _textOffsetY; }
      set { _textOffsetY = value; }
    }
  }
}