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

using System.Collections;
using System.Drawing;
using MediaPortal.ExtensionMethods;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Library
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// A GUIControl for displaying fading labels.
  /// </summary>
  public class GUIFadeLabel : GUIControl
  {
    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = 1;
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("align")] private Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("valign")] private VAlignment _textVAlignment = VAlignment.ALIGN_TOP;
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;
    [XMLSkinElement("wrapString")] protected string _userWrapString = "";
    [XMLSkinElement("allowFadeIn")] private bool _allowFadeIn = true;

    private readonly ArrayList _listLabels = new ArrayList();
    private int _currentLabelIndex;
    private int _scrollPosition;
    private double _scrollOffset;
    private int _scrollPosititionX;
    private bool _fadeIn;
    private int _currentFrame;
    private int _frameLimiter = 1;

    private double _timeElapsed;

    public double TimeSlice
    {
      get { return 0.01f + ((6 - GUIGraphicsContext.ScrollSpeedHorizontal) * 0.01f); }
    }

    private bool _allowScrolling = true;
    private bool _isScrolling;
    private bool _containsProperty;

    private string _previousText = "";
    private string _labelTail = " ";
    private string _wrapString = "";
    private GUILabelControl _labelControl;
    private GUIFont _font;

    public GUIFadeLabel(int dwParentID) : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUIFadeLabel class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strFont">The indication of the font of this control.</param>
    /// <param name="dwTextColor">The color of this control.</param>
    /// <param name="dwTextAlign">The alignment of this control.</param>
    /// <param name="dwTextVAlign">The vertical alignment of this control.</param>
    /// <param name="dwShadowAngle">The angle of the shadow; zero degrees along x-axis.</param>
    /// <param name="dwShadowDistance">The distance of the shadow.</param>
    /// <param name="dwShadowColor">The color of the shadow.</param>
    /// <param name="strUserWrapString">The string used to connect a wrapped fade label.</param>
    public GUIFadeLabel(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                        string strFont, long dwTextColor, Alignment dwTextAlign, VAlignment dwTextVAlign,
                        int dwShadowAngle, int dwShadowDistance, long dwShadowColor,
                        string strUserWrapString)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _fontName = strFont;
      _textColor = dwTextColor;
      _textAlignment = dwTextAlign;
      _textVAlignment = dwTextVAlign;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;
      _userWrapString = strUserWrapString;
      FinalizeConstruction();
    }

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override sealed void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      GUILocalizeStrings.LocalizeLabel(ref _label);

      // The labelTail is used to fill the backend of a scrolling label for both wrapping and non-wrapping labels
      // The wrapString is the text that joins the back to the front of a wrapping label (not used if the label should not wrap).
      if (_userWrapString.Length > 0)
      {
        _labelTail = "" + _userWrapString[_userWrapString.Length - 1];
        _wrapString = _userWrapString.Substring(0, _userWrapString.Length - 1);
      }

      _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                          _label, _textColor, _textAlignment, _textVAlignment, false,
                                          _shadowAngle, _shadowDistance, _shadowColor)
                        {
                          CacheFont = false,
                          ParentControl = this
                        };
      _labelControl.SetAnimations(Animations);
      if (_fontName != "" && _fontName != "-")
      {
        _font = GUIFontManager.GetFont(_fontName);
      }
      if (_label.IndexOf("#", System.StringComparison.Ordinal) >= 0)
      {
        _containsProperty = true;
      }
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      if (_labelControl == null)
      {
        return;
      }

      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }
      _isScrolling = false;
      if (!string.IsNullOrEmpty(_label))
      {
        string strText = _label;
        if (_containsProperty)
        {
          strText = GUIPropertyManager.Parse(strText) ?? string.Empty;
        }

        if (_previousText != strText)
        {
          _currentLabelIndex = 0;
          _scrollPosition    = 0;
          _scrollPosititionX = 0;
          _scrollOffset      = 0;
          _currentFrame      = 0;
          _timeElapsed       = 0;
          _fadeIn = _allowFadeIn;
          _listLabels.DisposeAndClearList();
          _previousText = strText;
          strText = strText.Replace("\\r", "\r");
          int ipos;
          do
          {
            ipos = strText.IndexOf("\r", System.StringComparison.Ordinal);
            int ipos2 = strText.IndexOf("\n", System.StringComparison.Ordinal);
            if (ipos >= 0 && ipos2 >= 0 && ipos2 < ipos)
            {
              ipos = ipos2;
            }
            if (ipos < 0 && ipos2 >= 0)
            {
              ipos = ipos2;
            }

            if (ipos >= 0)
            {
              string strLine = strText.Substring(0, ipos);
              if (strLine.Length > 1)
              {
                _listLabels.Add(strLine);
              }
              if (ipos + 1 >= strText.Length)
              {
                break;
              }
              strText = strText.Substring(ipos + 1);
            }
            else
            {
              _listLabels.Add(strText);
            }
          } while (ipos >= 0 && strText.Length > 0);
        }
      }
      else
      {
        _listLabels.DisposeAndClearList();
      }

      // if there are no labels do not render
      if (_listLabels.Count == 0)
      {
        base.Render(timePassed);
        return;
      }

      // reset the current label is index is out of bounds
      if (_currentLabelIndex < 0 || _currentLabelIndex >= _listLabels.Count)
      {
        _currentLabelIndex = 0;
      }

      // get the current label
      var strLabel = (string)_listLabels[_currentLabelIndex];

      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.Label = strLabel;
      _labelControl.SetPosition(_positionX, _positionY);
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.TextVAlignment = _textVAlignment;
      _labelControl.TextColor = _textColor;
      _labelControl.CacheFont = _labelControl.TextWidth < _width;
      if (GUIGraphicsContext.graphics != null)
      {
        _labelControl.Render(timePassed);
        base.Render(timePassed);
        return;
      }

      // if there is only one label just draw the text
      if (_listLabels.Count == 1 && _labelControl.TextWidth < _width)
      {
        _labelControl.Render(timePassed);
        base.Render(timePassed);
        return;
      }

      strLabel += _wrapString;

      _timeElapsed += timePassed;
      _currentFrame = (int)(_timeElapsed / TimeSlice);

      if (_frameLimiter < GUIGraphicsContext.MaxFPS)
      {
        _frameLimiter++;
      }
      else
      {
        _frameLimiter = 1;
      }

      // More than one label
      _isScrolling = true;

      // Make the label fade in
      if (_fadeIn && _allowScrolling)
      {
        long dwAlpha = ((((uint)_textColor) >> 24) * _currentFrame) / 12;
        dwAlpha <<= 24;
        dwAlpha |= (_textColor & 0x00ffffff);
        _labelControl.TextColor = dwAlpha;
        
        dwAlpha = ((((uint)_shadowColor) >> 24) * _currentFrame) / 12;
        dwAlpha <<= 24;
        dwAlpha |= (_shadowColor & 0x00ffffff);
        _labelControl.ShadowColor = dwAlpha;

        float fwt = 0;
        _labelControl.Label = GetShortenedText(strLabel, _width, ref fwt);
        if (_textAlignment == Alignment.ALIGN_RIGHT)
        {
          _labelControl.Width = (int)(fwt);
        }
        _labelControl.Render(timePassed);
        if (_currentFrame >= 12)
        {
          _fadeIn = false;
        }
      }
      else if (_fadeIn && !_allowScrolling)
      {
        _fadeIn = false;
      }

      // not fading in
      if (!_fadeIn)
      {
        if (!_allowScrolling)
        {
          _currentLabelIndex = 0;
          _scrollPosition    = 0;
          _scrollPosititionX = 0;
          _scrollOffset      = 0;
          _currentFrame      = 0;
        }

        // render the text
        bool bDone = RenderText(timePassed, _positionX, _positionY, _width, strLabel);
        if (bDone)
        {
          _currentLabelIndex++;
          _scrollPosition    = 0;
          _scrollPosititionX = 0;
          _scrollOffset      = 0;
          _currentFrame      = 0;
          if (!WrapAround() || _listLabels.Count > 1)
          {
            _timeElapsed = 0f;
          }
          _currentFrame = 0;
        }
      }
      base.Render(timePassed);
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
    /// <returns>true if the message was handled, false if it wasn't</returns>
    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          _previousText = "";
          _listLabels.DisposeAndClearList();
          _currentLabelIndex = 0;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _fadeIn = _allowFadeIn;
          _currentFrame = 0;
          _timeElapsed = 0.0f;
          Label = message.Label ?? string.Empty;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          Add(message.Label);
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          _previousText = "";
          _listLabels.DisposeAndClearList();
          _currentLabelIndex = 0;
          _scrollPosition = 0;
          _scrollPosititionX = 0;
          _scrollOffset = 0.0f;
          _fadeIn = _allowFadeIn;
          _currentFrame = 0;
          _timeElapsed = 0.0f;
        }
      }
      return base.OnMessage(message);
    }


    /// <summary>
    /// Renders the text.
    /// </summary>
    /// <param name="timePassed"></param>
    /// <param name="positionX">The X position of the text.</param>
    /// <param name="positionY">The Y position of the text.</param>
    /// <param name="maxWidth">The maximum render width.</param>
    /// <param name="text">The actual text.</param>
    /// <returns>true if the render was successful</returns>
    private bool RenderText(float timePassed, float positionX, float positionY, float maxWidth, string text)
    {
      bool result = false;

      // do not render if we don't have a font
      if (_font == null)
      {
        return true;
      }

      // get the text dimensions.
      float textHeight = 0;
      float textWidth  = 0;
      _font.GetTextExtent(text, ref textWidth, ref textHeight);
      float fWidth = textWidth;
      float fHeight = textHeight;

      // set text color and apply dimming if requested
      long color = _textColor;
      if (Dimmed)
      {
        color &= DimColor;
      }

      // scroll
      string originalText = text;
      if (_textAlignment != Alignment.ALIGN_RIGHT)
      {
        do
        {
          _font.GetTextExtent(originalText, ref textWidth, ref textHeight);
          originalText += _labelTail;
        } while (textWidth >= 0 && textWidth < maxWidth);
      }

      string scrollText = "";

      if (_timeElapsed > _scrollStartDelay)
      {
        if (_allowScrolling)
        {
          if (GUIGraphicsContext.ScrollSpeedHorizontal < 3)
          {
            if (_frameLimiter % (4 - GUIGraphicsContext.ScrollSpeedHorizontal) == 0)
            {
              _scrollPosititionX++;
            }
          }
          else
          {
            _scrollPosititionX = _scrollPosititionX + GUIGraphicsContext.ScrollSpeedHorizontal - 2;
          }
        }

        string tempText = _scrollPosition >= originalText.Length ? " " : originalText.Substring(_scrollPosition, 1);
        _font.GetTextExtent(tempText, ref fWidth, ref fHeight);

        if (_scrollPosititionX - _scrollOffset >= fWidth)
        {
          _scrollPosition++;
          if (_scrollPosition > text.Length)
          {
            _scrollPosition = 0;
            result = true;
            if (!WrapAround())
            {
               return true;
            }
          }
          _scrollOffset += fWidth;
        }

        int wrapPosition = 0;
        for (int i = 0; i < originalText.Length; i++)
        {
          if (_scrollPosition + i < originalText.Length)
          {
            scrollText += originalText[i + _scrollPosition];
          }
          else
          {
           scrollText += WrapAround() ? originalText[wrapPosition++] : ' ';
          }
        }

        _labelControl.TextAlignment = Alignment.ALIGN_LEFT;
        _labelControl.TextVAlignment = _textVAlignment;
        _labelControl.Label = scrollText;
        _labelControl.Width = (int)(maxWidth + _scrollPosititionX - _scrollOffset);
        _labelControl.TextColor = color;

        double xpos;
        double xoff;
        double xclipoff = 0;
        switch (_textAlignment)
        {
          case Alignment.ALIGN_RIGHT:
            float fwt = 0;
            GetShortenedText(originalText, _width, ref fwt);
            //xoff = textWidth >= _width ? 0 : _width - fwt;
            xoff = textWidth >= _width ? -fwt : _width - fwt;
            xclipoff = xoff;
            xpos = positionX + xoff - _scrollPosititionX + _scrollOffset;
            _labelControl.SetPosition((int)xpos, (int)positionY);
            break;

          case Alignment.ALIGN_CENTER:
            _labelControl.TextColor = color;
            _labelControl.TextVAlignment = VAlignment.ALIGN_TOP;
            xpos = positionX - _scrollPosititionX + _scrollOffset;
            xoff = (_width - textWidth) / 2;
            if (xoff < 0)
            {
              xoff = 0;
            }
            double yoff = (_height - textHeight) / 2;
            _labelControl.SetPosition((int)(xpos + xoff), (int)(positionY + yoff));
            break;

          default:
            xpos = positionX - _scrollPosititionX + _scrollOffset;
            _labelControl.SetPosition((int)xpos, (int)positionY);
            break;
        }

        var clipRect = new Rectangle();
        clipRect.X      = (int)(positionX + xclipoff);
        clipRect.Y      = _labelControl.YPosition;
        clipRect.Width  = maxWidth > 0 ? (int)(maxWidth - xclipoff) : GUIGraphicsContext.Width - clipRect.X;
        clipRect.Height = GUIGraphicsContext.Height - clipRect.Y;

        GUIGraphicsContext.BeginClip(clipRect);
        _labelControl.Render(timePassed);
        GUIGraphicsContext.EndClip();
      }
      else
      {
        float fwt = 0;
        float fht = 0;
        _labelControl.Label = GetShortenedText(text, (int)maxWidth, ref fwt);
        _font.GetTextExtent(_labelControl.Label, ref fwt, ref fht);
        _labelControl.Width = _textAlignment == Alignment.ALIGN_RIGHT ? (int)fwt : (int)maxWidth;
        _labelControl.TextColor = color;
        _labelControl.TextVAlignment = _textVAlignment;

        double xoff = 0;
        if (_textAlignment == Alignment.ALIGN_RIGHT)
        {
          //xoff = textWidth >= maxWidth ? fwt : maxWidth;
          xoff = textWidth >= maxWidth ? 0 : maxWidth;
        }
        _labelControl.SetPosition((int)positionX + (int)xoff, (int)positionY);          
        _labelControl.Render(timePassed);
      }

      return result;
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
        _fontName = value;
        _font = GUIFontManager.GetFont(_fontName);
      }
    }

    /// <summary>
    /// Get/set the color of the text.
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/set the alignment of the text.
    /// </summary>
    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    /// <summary>
    /// Get/set the vertical alignment of the text.
    /// </summary>
    public VAlignment TextVAlignment
    {
      get { return _textVAlignment; }
      set { _textVAlignment = value; }
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      _font = GUIFontManager.GetFont(_fontName);
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _previousText = "";
      _listLabels.DisposeAndClearList();
      _font = null;
    }

    /// <summary>
    /// Clears the control.
    /// </summary>
    public void Clear()
    {
      _currentLabelIndex = 0;
      _previousText = "";
      _listLabels.DisposeAndClearList();
      _currentFrame = 0;
      _scrollPosition = 0;
      _scrollPosititionX = 0;
      _scrollOffset = 0.0f;
      _timeElapsed = 0.0f;
      _frameLimiter = 1;
    }

    /// <summary>
    /// Add a label to the control.
    /// </summary>
    /// <param name="strLabel"></param>
    public void Add(string strLabel)
    {
      if (string.IsNullOrEmpty(strLabel))
      {
        return;
      }
      if (string.IsNullOrEmpty(_label))
      {
        _label = strLabel;
      }
      else
      {
        _label += "\r" + strLabel;
      }
      // control will split labels when rendering
    }

    /// <summary>
    /// Get/set the scrolling property of the control.
    /// </summary>
    public bool AllowScrolling
    {
      get { return _allowScrolling; }
      set
      {
        if (!value)
        {
          _timeElapsed = 0.0f;
        }
        _allowScrolling = value;
      }
    }

    /// <summary>
    /// Get/set the fadeIn property of the control.
    /// </summary>
    public bool AllowFadeIn
    {
      get { return _allowFadeIn; }
      set { _allowFadeIn = value; }
    }

    /// <summary>
    /// Return true if the user has specified that this fade label should wrap around.
    /// </summary>
    public bool WrapAround()
    {
      return (_wrapString.Length > 0);
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
    /// some controls (for example the fadelabel) contain scrolling texts and need 2
    /// ne re-rendered constantly
    /// </summary>
    /// <returns>true or false</returns>
    public override bool NeedRefresh()
    {
      if (_isScrolling && _allowScrolling)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
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
        _containsProperty = _label.IndexOf("#", System.StringComparison.Ordinal) >= 0;
      }
    }

    public bool HasText
    {
      get { return _listLabels.Count > 0; }
    }

    private string GetShortenedText(string strLabel, int iMaxWidth, ref float fw)
    {
      if (strLabel == null)
      {
        return string.Empty;
      }
      if (strLabel.Length == 0)
      {
        return string.Empty;
      }
      if (_font == null)
      {
        return strLabel;
      }
      if (_textAlignment == Alignment.ALIGN_RIGHT)
      {
        if (strLabel.Length > 0)
        {
          bool bTooLong;
          float fh = 0;
          do
          {
            bTooLong = false;
            _font.GetTextExtent(strLabel, ref fw, ref fh);
            if (fw >= iMaxWidth)
            {
              strLabel = strLabel.Substring(0, strLabel.Length - 1);
              bTooLong = true;
            }
          } while (bTooLong && strLabel.Length > 1);
        }
      }
      return strLabel;
    }

    public override int DimColor
    {
      get { return _dimColor; }
      set
      {
        _dimColor = value;
        // Need to pass the dim color to our delegate label if someone tries to set it (e.g., when fadelabel is in a group).
        if (_labelControl != null)
        {
          _labelControl.DimColor = value;
        }
      }
    }

    public int ScrollStartDelay
    {
      get { return _scrollStartDelay; }
      set { _scrollStartDelay = value; }
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

    /// <summary>
    /// Returns the width of the current text
    /// </summary>
    public int TextWidth
    {
      get
      {
        float width = 0f;
        float height = 0f;
        if (_font == null)
        {
          return 0;
        }
        _font.GetTextExtent(_label, ref width, ref height);
        return (int)width;
      }
    }
  }
}