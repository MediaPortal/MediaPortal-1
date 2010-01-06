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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying text.
  /// </summary>
  public class GUILabelControl : GUIControl
  {
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("label")] protected string _labelText = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("align")] private Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("valign")] private VAlignment _textVAlignment = VAlignment.ALIGN_TOP;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;

    private string _cachedTextLabel = "";
    private bool _containsProperty = false;
    private int _textwidth = 0;
    private int _textheight = 0;
    private bool _useFontCache = false;

    private GUIFont _font = null;
    private bool _useViewPort = true;
    private bool _propertyHasChanged = false;
    private bool _reCalculate = false;

    /// <summary>
    /// The constructor of the GUILabelControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strFont">The indication of the font of this control.</param>
    /// <param name="strLabel">The text of this control.</param>
    /// <param name="dwTextColor">The color of this control.</param>
    /// <param name="dwTextAlign">The alignment of this control.</param>
    /// <param name="dwTextVAlign">The vertical alignment of this control.</param>
    /// <param name="bHasPath">Indicates if the label is containing a path.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degress along x-axis.</param>
    /// <param name="dwShadowDistance">The distance of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    public GUILabelControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                           string strFont, string strLabel, long dwTextColor, Alignment dwTextAlign,
                           VAlignment dwTextVAlign, bool bHasPath,
                           int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _labelText = strLabel;
      _fontName = strFont;
      _textColor = dwTextColor;
      _textAlignment = dwTextAlign;
      _textVAlignment = dwTextVAlign;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;

      FinalizeConstruction();
    }

    public GUILabelControl(int dwParentID)
      : base(dwParentID) {}

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      if (_fontName == null)
      {
        _fontName = string.Empty;
      }
      if (_fontName != "" && _fontName != "-")
      {
        _font = GUIFontManager.GetFont(_fontName);
      }

      GUILocalizeStrings.LocalizeLabel(ref _labelText);

      if (_labelText == null)
      {
        _labelText = string.Empty;
      }
      if (_labelText.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }

      _cachedTextLabel = _labelText;
      _propertyHasChanged = true;
    }

    /// <summary>
    /// Renders the text onscreen.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }
      if (_containsProperty && _propertyHasChanged)
      {
        _propertyHasChanged = false;
        string newLabel = GUIPropertyManager.Parse(_labelText);
        if (_cachedTextLabel != newLabel)
        {
          if (newLabel == null)
          {
            newLabel = "";
          }
          _cachedTextLabel = newLabel;
          _textwidth = 0;
          _textheight = 0;
          _reCalculate = true;
        }
      }

      if (_reCalculate)
      {
        _reCalculate = false;
        ClearFontCache();
      }

      if (_cachedTextLabel == null)
      {
        base.Render(timePassed);
        return;
      }
      if (_cachedTextLabel.Length == 0)
      {
        base.Render(timePassed);
        return;
      }

      long color = _textColor;
      if (Dimmed)
      {
        color &= (DimColor);
      }

      if (null != _font)
      {
        if (GUIGraphicsContext.graphics != null)
        {
          if (_width > 0)
          {
            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);
            this.DrawTextWidth(_positionX, _positionY, (int)c, _cachedTextLabel, _width, _textAlignment);
          }
          else
          {
            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);
            this.DrawText(_positionX, _positionY, (int)c, _cachedTextLabel, _textAlignment, -1);
          }
          base.Render(timePassed);
          return;
        }

        if (_textwidth == 0 || _textheight == 0)
        {
          float width = _textwidth;
          float height = _textheight;
          _font.GetTextExtent(_cachedTextLabel, ref width, ref height);
          _textwidth = (int)width;
          _textheight = (int)height;
        }

        // Compute the vertical position of text in the controls height extent.
        // Default is align to top.
        float vpos = _positionY;
        if (_textVAlignment == VAlignment.ALIGN_MIDDLE)
        {
          vpos += (float)Math.Ceiling((this.Height - this._textheight) / 2.0f);
        }
        else if (_textVAlignment == VAlignment.ALIGN_BOTTOM)
        {
          vpos += this.Height - this._textheight;
        }

        if (_textAlignment == Alignment.ALIGN_CENTER)
        {
          int xoff = (int)((_width - _textwidth) / 2);
          int yoff = (int)((_height - _textheight) / 2);
          uint c = (uint)color;
          c = GUIGraphicsContext.MergeAlpha(c);

          this.DrawText((float)_positionX + xoff, (float)_positionY + yoff, (int)c, _cachedTextLabel,
                        Alignment.ALIGN_LEFT, _width);
        }
        else
        {
          if (_textAlignment == Alignment.ALIGN_RIGHT)
          {
            if (_width == 0 || _textwidth < _width)
            {
              uint c = (uint)color;
              c = GUIGraphicsContext.MergeAlpha(c);

              this.DrawText((float)_positionX - _textwidth, vpos, (int)c, _cachedTextLabel,
                            Alignment.ALIGN_LEFT, -1);
            }
            else
            {
/*
              float fPosCX = (float)_positionX;
              float fPosCY = (float)_positionY;
              GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
              if (fPosCX < 0) fPosCX = 0.0f;
              if (fPosCY < 0) fPosCY = 0.0f;
              if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
              float heighteight = 60.0f;
              if (heighteight + fPosCY >= GUIGraphicsContext.Height)
                heighteight = GUIGraphicsContext.Height - fPosCY - 1;
              if (heighteight <= 0) return;

              float fwidth = _width - 5.0f;

              if (fPosCX <= 0) fPosCX = 0;
              if (fPosCY <= 0) fPosCY = 0;
              if (fwidth < 1) return;
              if (heighteight < 1) return;
              */
              if (_width < 6)
              {
                base.Render(timePassed);
                return;
              }
              uint c = (uint)color;
              c = GUIGraphicsContext.MergeAlpha(c);

              this.DrawText((float)_positionX - _textwidth, vpos, (int)c, _cachedTextLabel,
                            Alignment.ALIGN_LEFT, (int)_width - 5);
              //if (_useViewPort)
              //  GUIGraphicsContext.DX9Device.Viewport = oldviewport;
            }
            base.Render(timePassed);
            return;
          }

          if (_width == 0 || _textwidth < _width)
          {
            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);

            this.DrawText((float)_positionX, vpos, (int)c, _cachedTextLabel, _textAlignment,
                          (int)_width);
          }
          else
          {
            /*
            float fPosCX = (float)_positionX;
            float fPosCY = (float)_positionY;
            GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
            if (fPosCX < 0) fPosCX = 0.0f;
            if (fPosCY < 0) fPosCY = 0.0f;
            if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
            float heighteight = 60.0f;
            if (heighteight + fPosCY >= GUIGraphicsContext.Height)
              heighteight = GUIGraphicsContext.Height - fPosCY - 1;
            if (heighteight <= 0) return;

            float fwidth = _width - 5.0f;
            if (fwidth < 1) return;
            if (heighteight < 1) return;

            if (fPosCX <= 0) fPosCX = 0;
            if (fPosCY <= 0) fPosCY = 0;*/
            if (_width < 6)
            {
              return;
            }

            uint c = (uint)color;
            c = GUIGraphicsContext.MergeAlpha(c);
            this.DrawText((float)_positionX, vpos, (int)c, _cachedTextLabel, _textAlignment,
                          (int)_width - 5);
          }
        }
      }
      base.Render(timePassed);
    }

    // Wraps the calls to the GUIFont.  This provides opportunity to shadow the text if requested.
    public void DrawTextWidth(float xpos, float ypos, long color, string label, float fMaxWidth,
                              GUIControl.Alignment alignment)
    {
      if (Shadow)
      {
        _font.DrawShadowTextWidth(xpos, ypos, color, label, alignment, _shadowAngle, _shadowDistance, _shadowColor,
                                  fMaxWidth);
      }
      else
      {
        _font.DrawTextWidth(xpos, ypos, color, label, fMaxWidth, alignment);
      }
    }

    // Wraps the calls to the GUIFont.  This provides opportunity to shadow the text if requested.
    public void DrawText(float xpos, float ypos, long color, string label, GUIControl.Alignment alignment, int width)
    {
      if (Shadow)
      {
        _font.DrawShadowText(xpos, ypos, color, label, alignment, _shadowAngle, _shadowDistance, _shadowColor);
      }
      else
      {
        _font.DrawText(xpos, ypos, color, label, alignment, width);
      }
    }

    public bool UseViewPort
    {
      get { return _useViewPort; }
      set { _useViewPort = value; }
    }

    /// <summary>
    /// Checks if the control can focus.
    /// </summary>
    /// <returns>false</returns>
    public override bool CanFocus()
    {
      return false;
    }

    /// <summary>
    /// This method is called when a message was recieved by this control.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      // Check if the message was ment for this control.
      if (message.TargetControlId == GetID)
      {
        // Set the text of the label.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
          else
          {
            Label = string.Empty;
          }
          return true;
        }
      }
      return base.OnMessage(message);
    }

    public override int Width
    {
      get { return base.Width; }
      set
      {
        if (base.Width != value)
        {
          base.Width = value;
          _reCalculate = true;
        }
      }
    }

    public override int Height
    {
      get { return base.Height; }
      set
      {
        if (base.Height != value)
        {
          base.Height = value;
          _reCalculate = true;
        }
      }
    }

    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (_positionX == dwPosX && _positionY == dwPosY)
      {
        return;
      }
      _positionX = dwPosX;
      _positionY = dwPosY;
      _reCalculate = true;
    }

    /// <summary>
    /// Get/set the color of the text
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set
      {
        if (_textColor != value)
        {
          _textColor = value;

          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the alignment of the text
    /// </summary>
    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set
      {
        if (_textAlignment != value)
        {
          _textAlignment = value;

          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the vertical alignment of the text
    /// </summary>
    public VAlignment TextVAlignment
    {
      get { return _textVAlignment; }
      set
      {
        if (_textVAlignment != value)
        {
          _textVAlignment = value;

          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        if (value == string.Empty)
        {
          return;
        }
        if (_font == null)
        {
          _font = GUIFontManager.GetFont(value);
          _fontName = value;
          _reCalculate = true;
        }
        else if (value != _font.FontName)
        {
          _font = GUIFontManager.GetFont(value);
          _fontName = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Label
    {
      get
      {
        //if (_labelText.Length == 0 && _cachedTextLabel.Length > 0)
        //  _labelText = _cachedTextLabel;

        return _labelText;
      }
      set
      {
        if (value == null)
        {
          return;
        }
        if (value.Equals(_labelText))
        {
          return;
        }

        _labelText = value;
        _cachedTextLabel = _labelText;

        if (_labelText.IndexOf("#") >= 0)
        {
          _containsProperty = true;
        }
        else
        {
          _containsProperty = false;
        }

        _textwidth = 0;
        _textheight = 0;
        _reCalculate = true;
      }
    }

    /// <summary>
    /// Set the shadow properties
    /// </summary>
    public void SetShadow(int angle, int distance, long color)
    {
      _shadowAngle = angle;
      _shadowDistance = distance;
      _shadowColor = color;
    }

    public int ShadowAngle
    {
      get { return _shadowAngle; }
      set { _shadowAngle = value; }
    }

    public int ShadowDistance
    {
      get { return _shadowDistance; }
      set { _shadowDistance = value; }
    }

    public long ShadowColor
    {
      get { return _shadowColor; }
      set { _shadowColor = value; }
    }

    private bool Shadow
    {
      get { return (_shadowDistance > 0) && ((_shadowColor >> 24) > 0); }
    }

    /// <summary>
    /// Property which returns true if the label contains a property
    /// or false when it doenst
    /// </summary>
    public bool _containsPropertyKey
    {
      get { return _containsProperty; }
    }

    /// <summary>
    /// Allocate any direct3d sources
    /// </summary>
    public override void AllocResources()
    {
      _propertyHasChanged = true;
      GUIPropertyManager.OnPropertyChanged +=
        new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
      _font = GUIFontManager.GetFont(_fontName);
      Update();
      base.AllocResources();
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (!_containsProperty)
      {
        return;
      }
      if (_labelText.IndexOf(tag) >= 0)
      {
        _propertyHasChanged = true;
      }
    }

    /// <summary>
    /// Free any direct3d resources
    /// </summary>
    public override void FreeResources()
    {
      //_labelText = string.Empty;
      _reCalculate = true;
      GUIPropertyManager.OnPropertyChanged -=
        new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
      base.FreeResources();
    }

    /// <summary>
    /// Property to get/set the usage of the font cache
    /// if enabled the renderd text is cached
    /// if not it will be re-created on every render() call
    /// </summary>
    public bool CacheFont
    {
      get { return _useFontCache; }
      set { _useFontCache = false; }
    }

    /// <summary>
    /// updates the current label by deleting the fontcache 
    /// </summary>
    protected override void Update() {}

    /// <summary>
    /// Returns the width of the current text
    /// </summary>
    public int TextWidth
    {
      get
      {
        if (_textwidth == 0 || _textheight == 0)
        {
          if (_font == null)
          {
            return 0;
          }
          _cachedTextLabel = GUIPropertyManager.Parse(_labelText);
          if (_cachedTextLabel == null)
          {
            _cachedTextLabel = "";
          }
          float width = _textwidth;
          float height = _textheight;
          _font.GetTextExtent(_cachedTextLabel, ref width, ref height);
          _textwidth = (int)width;
          _textheight = (int)height;
        }
        return _textwidth;
      }
    }

    /// <summary>
    /// Returns the height of the current text
    /// </summary>
    public int TextHeight
    {
      get
      {
        if (_textwidth == 0 || _textheight == 0)
        {
          if (_font == null)
          {
            return 0;
          }
          _cachedTextLabel = GUIPropertyManager.Parse(_labelText);
          if (_cachedTextLabel == null)
          {
            _cachedTextLabel = "";
          }
          float width = _textwidth;
          float height = _textheight;
          _font.GetTextExtent(_cachedTextLabel, ref width, ref height);
          _textwidth = (int)width;
          _textheight = (int)height;
        }
        return _textheight;
      }
    }

    private void ClearFontCache()
    {
      Update();
    }

    public override void Animate(float timePassed, Animator animator)
    {
      base.Animate(timePassed, animator);
      _reCalculate = true;
    }
  }
}