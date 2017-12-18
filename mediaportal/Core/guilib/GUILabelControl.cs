#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
    [XMLSkinElement("maxWidth")] protected int _maxWidth = 0;
    [XMLSkinElement("maxHeight")] protected int _maxHeight = 0;
    [XMLSkinElement("trimText")] protected bool _trimText = false; // For autosized label always True

    private string _cachedTextLabel = "";
    private bool _containsProperty = false;
    private int _textwidth = 0;
    private int _textheight = 0;
    private bool _useFontCache = false;
    private bool _registeredForEvent = false;

    private GUIFont _font = null;
    private bool _propertyHasChanged = false;
    private bool _reCalculate = false;
    private FontRenderContext _context;

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
      : this(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, 
             strFont, strLabel, dwTextColor, dwTextAlign, dwTextVAlign, bHasPath, 
             dwShadowAngle, dwShadowDistance, dwShadowColor, 0, 0) { }

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
    /// <param name="dwMaxWidth">Max Width for autosized Label. From Width to MaxWidth.</param>
    /// <param name="dwMaxHeight">Max Height for autosized Label. From Height to MaxHeight.</param>
    public GUILabelControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                           string strFont, string strLabel, long dwTextColor, Alignment dwTextAlign,
                           VAlignment dwTextVAlign, bool bHasPath,
                           int dwShadowAngle, int dwShadowDistance, long dwShadowColor, int dwMaxWidth, int dwMaxHeight)
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
      _maxWidth = dwMaxWidth;
      _maxHeight = dwMaxHeight;

      FinalizeConstruction();
    }

    public GUILabelControl(int dwParentID) : base(dwParentID) {}

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override sealed void FinalizeConstruction()
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

      if (_maxWidth > 0)
      {
        _trimText = true;
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
      CachedLabel();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control to scale itself to the current screen resolution
    /// </summary>
    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _maxWidth, ref _maxHeight);
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
        CachedLabel();
      }

      if (string.IsNullOrEmpty(_cachedTextLabel))
      {
        base.Render(timePassed);
        return;
      }

      if (_reCalculate)
      {
        _reCalculate = false;
        ClearFontCache();
      }

      if (null != _font)
      {
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
          vpos += (float)Math.Ceiling((Height - _textheight) / 2.0f);
        }
        else if (_textVAlignment == VAlignment.ALIGN_BOTTOM)
        {
          vpos += Height - _textheight;
        }

        if (_textAlignment == Alignment.ALIGN_CENTER)
        {
          int xoff = (Width - _textwidth) / 2;
          if (xoff < 0 && Width > 0)
          {
            xoff = 0;
          }
          int yoff = (Height - _textheight) / 2;

          DrawText((float)_positionX + xoff, (float)_positionY + yoff, _cachedTextLabel, Width);
        }
        else
        {
          if (_textAlignment == Alignment.ALIGN_RIGHT)
          {
            if (Width == 0 || _textwidth < Width)
            {
              DrawText((float)_positionX - _textwidth, vpos, _cachedTextLabel, -1);
            }
            else
            {
              if (Width < 6)
              {
                base.Render(timePassed);
                return;
              }

              DrawText((float)_positionX - _textwidth, vpos, _cachedTextLabel, Width - 5);
            }
            base.Render(timePassed);
            return;
          }

          if (Width == 0 || _textwidth < Width)
          {
            DrawText(_positionX, vpos, _cachedTextLabel, Width);
          }
          else
          {
            if (Width < 6)
            {
              return;
            }
            DrawText(_positionX, vpos, _cachedTextLabel, Width - 5);
          }
        }
      }
      base.Render(timePassed);
    }

    // Wraps the calls to the GUIFont.  This provides opportunity to shadow the text if requested.
    public void DrawTextWidth(float xpos, float ypos, string label, float fMaxWidth)
    {
      string _label = label;
      if (_trimText)
      {
        _label = GetShortenedText(_label, _maxWidth > 0 ? _maxWidth : _width);
      }
      long c = (uint)_textColor;
      if (Dimmed)
      {
        c &= (DimColor);
      }
      c = GUIGraphicsContext.MergeAlpha((uint)c);

      if (Shadow)
      {
        long sc = (uint)_shadowColor;
        if (Dimmed)
        {
          sc &= DimColor;
        }
        sc = GUIGraphicsContext.MergeAlpha((uint)sc);

        if (_font != null)
        {
          _font.DrawShadowTextWidth(xpos, ypos, c, _label, Alignment.ALIGN_LEFT, _shadowAngle, _shadowDistance, sc, fMaxWidth);
        }
      }
      else
      {
        if (_font != null)
        {
          _font.DrawTextWidth(xpos, ypos, c, _label, fMaxWidth, Alignment.ALIGN_LEFT);
        }
      }
    }

    // Wraps the calls to the GUIFont.  This provides opportunity to shadow the text if requested.
    public void DrawText(float xpos, float ypos, string label, int width)
    {
      string _label = label;
      if (_trimText)
      {
        _label = GetShortenedText(_label, _maxWidth > 0 ? _maxWidth : _width);
      }
      long c = _textColor;
      if (Dimmed)
      {
        c &= DimColor;
      }
      c = GUIGraphicsContext.MergeAlpha((uint)c);

      if (Shadow)
      {
        long sc = _shadowColor;
        if (Dimmed)
        {
          sc &= DimColor;
        }
        sc = GUIGraphicsContext.MergeAlpha((uint)sc);
        if (_font != null)
        {
          _font.DrawShadowText(xpos, ypos, c, _label, Alignment.ALIGN_LEFT, width, _shadowAngle, _shadowDistance, sc);
        }
      }
      else
      {
        var clipRect = new Rectangle();
        clipRect.X      = (int)xpos;
        clipRect.Y      = (int)ypos;
        clipRect.Width  = width > 0 ? width : GUIGraphicsContext.Width - clipRect.X;
        clipRect.Height = GUIGraphicsContext.Height - clipRect.Y;

        GUIGraphicsContext.BeginClip(clipRect);
        if (_font != null) 
        {
          _font.DrawTextEx(xpos, ypos, c, _label, ref _context, width);
        }
        GUIGraphicsContext.EndClip();
      }
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
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      // Check if the message was meant for this control.
      if (message.TargetControlId == GetID)
      {
        // Set the text of the label.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          Label = message.Label ?? string.Empty;
          return true;
        }
      }
      return base.OnMessage(message);
    }

    public override int Width
    {
      get 
      { 
        if (_maxWidth > 0)
        {
          int textWidth = TextWidth;
          if (textWidth < base.Width)
          {
            return base.Width;
          }
          else if (textWidth > _maxWidth)
          {
            return _maxWidth;
          }
          else
          {
            return textWidth + 1; // + 1 - Margin for not fade last char in label text
          }
        }
        else
        {
          return base.Width;
        }
      }
      set
      {
        if (base.Width != value)
        {
          base.Width = value;
          _reCalculate = true;

          if (_maxWidth > 0 && _maxWidth < value)
          {
            _maxWidth = value;
          }
        }
      }
    }

    public override int Height
    {
      get 
      {  
        if (_maxHeight > 0)
        {
          int textHeight = TextHeight;
          if (textHeight < base.Height)
          {
            return base.Height;
          }
          else if (textHeight > _maxHeight)
          {
            return _maxHeight;
          }
          else
          {
            return textHeight;
          }
        }
        else
        {
          return base.Height;
        }
      }
      set
      {
        if (base.Height != value)
        {
          base.Height = value;
          _reCalculate = true;

          if (_maxHeight > 0 && _maxHeight < value)
          {
            _maxHeight = value;
          }
        }
      }
    }

    public int MinWidth
    {
      get 
      { 
        if (base.Width == 0)
        {
          return TextWidth + 1; // + 1 - Margin for not fade last char in label text
        }
        else
        {
          return base.Width;
        }
      }
      set
      {
        if (base.Width != value)
        {
          base.Width = value;
          _reCalculate = true;

          if (_maxWidth > 0 && _maxWidth < value)
          {
            _maxWidth = value;
          }
        }
      }
    }

    public int MinHeight
    {
      get 
      { 
        if (base.Height == 0)
        {
          return TextHeight;
        }
        else
        {
          return base.Height;
        }
      }
      set
      {
        if (base.Height != value)
        {
          base.Height = value;
          _reCalculate = true;

          if (_maxHeight > 0 && _maxHeight < value)
          {
            _maxHeight = value;
          }
        }
      }
    }

    public int MaxWidth
    {
      get { return _maxWidth; }
      set
      {
        if (_maxWidth != value)
        {
          _maxWidth = value;
          _reCalculate = true;

          if (_maxWidth > 0 && _maxWidth < base.Width)
          {
            base.Width = _maxWidth;
          }
        }
      }
    }

    public int MaxHeight
    {
      get { return _maxHeight; }
      set
      {
        if (_maxHeight != value)
        {
          _maxHeight = value;
          _reCalculate = true;

          if (_maxHeight > 0 && _maxHeight < base.Height)
          {
            base.Height = _maxHeight;
          }
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
        _containsProperty = _labelText.IndexOf("#") >= 0;
        CachedLabel();
      }
    }

    public bool TrimText
    {
      get { return _trimText; }
      set { _trimText = value; }
    }

    private void CachedLabel()
    {
      string v;

      if (_containsProperty)
      {
        v = GUIPropertyManager.Parse(_labelText) ?? String.Empty;
      }
      else
      {
        v = _labelText;
      }
      if (v != _cachedTextLabel)
      {
        _textwidth = 0;
        _textheight = 0;
        _reCalculate = true;
        _cachedTextLabel = v;
        _context = null;
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

      if (_registeredForEvent == false)
      {
        GUIPropertyManager.OnPropertyChanged -= GUIPropertyManager_OnPropertyChanged;
        GUIPropertyManager.OnPropertyChanged += GUIPropertyManager_OnPropertyChanged;
        _registeredForEvent = true;
      }
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
      if (_labelText.IndexOf(tag, StringComparison.Ordinal) >= 0)
      {
        _propertyHasChanged = true;
      }
    }

    /// <summary>
    /// Free any direct3d resources
    /// </summary>
    public override void Dispose()
    {
      //_labelText = string.Empty;
      _font = null;
      _reCalculate = true;
      GUIPropertyManager.OnPropertyChanged -= GUIPropertyManager_OnPropertyChanged;
      base.Dispose();
    }

    /// <summary>
    /// Property to get/set the usage of the font cache
    /// if enabled the rendered text is cached
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
          CachedLabel();
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
          CachedLabel();
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

    private string GetShortenedText(string strLabel, int iMaxWidth)
    {
      if (string.IsNullOrEmpty(strLabel))
      {
        return string.Empty;
      }
      if (_font == null)
      {
        return strLabel;
      }
      if (strLabel.Length > 0)
      {
        bool bTooLong;
        float fw = 0;
        float fh = 0;
        do
        {
          bTooLong = false;
          _font.GetTextExtent(strLabel, ref fw, ref fh);
          if (fw > iMaxWidth)
          {
            strLabel = strLabel.Substring(0, strLabel.Length - 1);
            bTooLong = true;
          }
        } while (bTooLong && strLabel.Length > 1);
      }
      return strLabel;
    }
  }
}
