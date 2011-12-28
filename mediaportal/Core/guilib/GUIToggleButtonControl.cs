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

using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIToggleButtonControl : GUIControl
  {
    [XMLSkinElement("textureFocus")] protected string _focusedTextureName = "";
    [XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = "";
    [XMLSkinElement("AltTextureFocus")] protected string _alternativeFocusTextureName = "";
    [XMLSkinElement("AltTextureNoFocus")] protected string _alternativeNonFocusTextureName = "";
    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;
    protected GUIAnimation _imageAlternativeFocused = null;
    protected GUIAnimation _imageAlternativeNonFocused = null;
    protected int _frameCounter = 0;
    [XMLSkinElement("font")] protected string _fontName;
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected long _textColorNoFocus = 0xFFFFFFFF;
    [XMLSkinElement("disabledcolor")] protected long _disabledColor = 0xFF606060;
    [XMLSkinElement("hyperlink")] protected int _hyperLinkWindowId = -1;
    [XMLSkinElement("onclick")] protected string _onclick = "";

    protected string _scriptAction = "";
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkin("textXOff", "hasMargin")] protected bool _textOffsetXHasMargin = true;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("textvalign")] protected VAlignment _textVAlignment = VAlignment.ALIGN_MIDDLE;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    protected GUILabelControl _labelControl = null;

    private bool _shadow = false;

    public GUIToggleButtonControl(int parentId)
      : base(parentId) {}

    public GUIToggleButtonControl(int parentId, int controlid, int posX, int posY, int width, int height,
                                  string textureFocusName, string textureNoFocusName, string textureAltFocusName,
                                  string textureAltNoFocusName,
                                  int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(parentId, controlid, posX, posY, width, height)
    {
      _focusedTextureName = textureFocusName;
      _nonFocusedTextureName = textureNoFocusName;
      _alternativeFocusTextureName = textureAltFocusName;
      _alternativeNonFocusTextureName = textureAltNoFocusName;
      _isSelected = false;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      FinalizeConstruction();
      DimColor = base.DimColor;
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      _imageFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                           _focusedTextureName);
      _imageFocused.ParentControl = this;

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _nonFocusedTextureName);
      _imageNonFocused.ParentControl = this;

      _imageAlternativeFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width,
                                                      _height, _alternativeFocusTextureName);
      _imageAlternativeFocused.ParentControl = this;

      _imageAlternativeNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width,
                                                         _height, _alternativeNonFocusTextureName);
      _imageAlternativeNonFocused.ParentControl = this;
      GUILocalizeStrings.LocalizeLabel(ref _label);

      _shadow = (_shadowAngle > 0) || (_shadowDistance > 0);

      _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                          _label, _textColor, Alignment.ALIGN_LEFT, VAlignment.ALIGN_MIDDLE, false,
                                          _shadowAngle, _shadowDistance, _shadowColor);
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.TextVAlignment = _textVAlignment;
      _labelControl.DimColor = DimColor;
      _labelControl.ParentControl = this;
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
    }

    public override void Render(float timePassed)
    {
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }

      // The GUIButtonControl has the focus
      if (Focus)
      {
        int dwAlphaCounter = _frameCounter + 2;
        int dwAlphaChannel;
        if ((dwAlphaCounter % 128) >= 64)
        {
          dwAlphaChannel = dwAlphaCounter % 64;
        }
        else
        {
          dwAlphaChannel = 63 - (dwAlphaCounter % 64);
        }

        dwAlphaChannel += 192;
        SetAlpha(dwAlphaChannel);
        if (_isSelected)
        {
          _imageFocused.Render(timePassed);
        }
        else
        {
          _imageAlternativeFocused.Render(timePassed);
        }
        _frameCounter++;
      }
      else
      {
        SetAlpha(0xff);
        if (_isSelected)
        {
          _imageNonFocused.Render(timePassed);
        }
        else
        {
          _imageAlternativeNonFocused.Render(timePassed);
        }
      }

      int labelWidth = _width;
      if (_textOffsetXHasMargin)
      {
        labelWidth = _width - 2 * _textOffsetX;
      }

      if (labelWidth <= 0)
      {
        base.Render(timePassed);
        return;
      }
      _labelControl.Width = labelWidth;
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.TextVAlignment = _textVAlignment;
      _labelControl.Label = _label;
      _labelControl.TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;

      // render the text on the button
      int x = 0;
      int y = 0;

      switch (_textAlignment)
      {
        case Alignment.ALIGN_LEFT:
          x = _textOffsetX + _positionX;
          break;

        case Alignment.ALIGN_RIGHT:
          x = _positionX + _width - _textOffsetX;
          break;

        case Alignment.ALIGN_CENTER:
          x = _positionX + ((_width / 2) - (labelWidth / 2));
          break;
      }

      switch (_textVAlignment)
      {
        case VAlignment.ALIGN_TOP:
          y = _textOffsetY + _positionY;
          break;

        case VAlignment.ALIGN_BOTTOM:
          y = _positionY + _height - _textOffsetY;
          break;

        case VAlignment.ALIGN_MIDDLE:
          y = _positionY + ((_height / 2) - (_labelControl.Height / 2));
          break;
      }

      _labelControl.SetPosition(x, y);
      _labelControl.Render(timePassed);
      base.Render(timePassed);
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
      GUIMessage message;
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          _isSelected = !_isSelected;

          // If this button has a click setting then execute the setting.
          if (_onclick.Length != 0)
          {
            GUIPropertyManager.Parse(_onclick, GUIExpressionManager.ExpressionOptions.EVALUATE_ALWAYS);
          }

          if (_hyperLinkWindowId >= 0)
          {
            GUIWindowManager.ActivateWindow(_hyperLinkWindowId);
            return;
          }
          // button selected.
          // send a message
          int iParam = 1;
          if (!_isSelected)
          {
            iParam = 0;
          }
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, iParam, 0, null);
          GUIGraphicsContext.SendMessage(message);
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          _label = message.Label;

          return true;
        }
      }
      if (base.OnMessage(message))
      {
        return true;
      }
      return false;
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();
      _imageAlternativeFocused.PreAllocResources();
      _imageAlternativeNonFocused.PreAllocResources();
    }

    public override void AllocResources()
    {
      base.AllocResources();
      _frameCounter = 0;
      _imageFocused.AllocResources();
      _imageNonFocused.AllocResources();
      _imageAlternativeFocused.AllocResources();
      _imageAlternativeNonFocused.AllocResources();
      _width = _imageFocused.Width;
      _height = _imageFocused.Height;
      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.AllocResources();
    }

    public override void Dispose()
    {
      base.Dispose();
      _imageFocused.SafeDispose();
      _imageNonFocused.SafeDispose();
      _imageAlternativeFocused.SafeDispose();
      _imageAlternativeNonFocused.SafeDispose();
      _labelControl.SafeDispose();
    }

    public override void SetPosition(int posX, int posY)
    {
      base.SetPosition(posX, posY);
      _imageFocused.SetPosition(posX, posY);
      _imageNonFocused.SetPosition(posX, posY);
      _imageAlternativeFocused.SetPosition(posX, posY);
      _imageAlternativeNonFocused.SetPosition(posX, posY);
    }

    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      _imageFocused.SetAlpha(dwAlpha);
      _imageNonFocused.SetAlpha(dwAlpha);
      _imageAlternativeFocused.SetAlpha(dwAlpha);
      _imageAlternativeNonFocused.SetAlpha(dwAlpha);
    }

    public long DisabledColor
    {
      get { return _disabledColor; }
      set { _disabledColor = value; }
    }

    public string TexutureNoFocusName
    {
      get { return _imageNonFocused.FileName; }
    }

    public string TexutureFocusName
    {
      get { return _imageFocused.FileName; }
    }

    public string AltTexutureNoFocusName
    {
      get { return _imageAlternativeNonFocused.FileName; }
    }

    public string AltTexutureFocusName
    {
      get { return _imageAlternativeFocused.FileName; }
    }

    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName = value;
        _labelControl.FontName = _fontName;
      }
    }

    public void SetLabel(string strFontName, string strLabel, long dwColor)
    {
      if (strFontName == null)
      {
        return;
      }
      if (strLabel == null)
      {
        return;
      }
      Label = strLabel;
      _textColor = dwColor;
      _fontName = strFontName;

      _labelControl.FontName = _fontName;
      _labelControl.TextColor = dwColor;
      _labelControl.Label = strLabel;
    }

    public string Label
    {
      get { return _label; }
      set
      {
        if (value == null)
        {
          return;
        }
        _label = value;
        _labelControl.Label = _label;
      }
    }

    public int HyperLink
    {
      get { return _hyperLinkWindowId; }
      set { _hyperLinkWindowId = value; }
    }

    public string ScriptAction
    {
      get { return _scriptAction; }
      set { _scriptAction = value; }
    }

    protected override void Update()
    {
      base.Update();

      _imageFocused.Width = _width;
      _imageFocused.Height = _height;

      _imageNonFocused.Width = _width;
      _imageNonFocused.Height = _height;

      _imageAlternativeFocused.Width = _width;
      _imageAlternativeFocused.Height = _height;

      _imageAlternativeNonFocused.Width = _width;
      _imageAlternativeNonFocused.Height = _height;

      _imageFocused.SetPosition(_positionX, _positionY);
      _imageNonFocused.SetPosition(_positionX, _positionY);
      _imageAlternativeFocused.SetPosition(_positionX, _positionY);
      _imageAlternativeNonFocused.SetPosition(_positionX, _positionY);
    }

    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX
    {
      get { return _textOffsetX; }
      set { _textOffsetX = value; }
    }

    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY
    {
      get { return _textOffsetY; }
      set { _textOffsetY = value; }
    }

    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageFocused != null)
        {
          _imageFocused.DimColor = value;
        }
        if (_imageNonFocused != null)
        {
          _imageNonFocused.DimColor = value;
        }
        if (_imageAlternativeFocused != null)
        {
          _imageAlternativeFocused.DimColor = value;
        }
        if (_imageAlternativeNonFocused != null)
        {
          _imageAlternativeNonFocused.DimColor = value;
        }
        if (_labelControl != null)
        {
          _labelControl.DimColor = value;
        }
      }
    }
  }
}