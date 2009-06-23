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
using System.Windows.Forms;
// used for Keys definition

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for GUISMSInputControl.
  /// </summary>
  public class GUISMSInputControl : GUIControl
  {
    public delegate void OnTextChangedHandler();

    public event OnTextChangedHandler OnTextChanged;
    // How often (per second) the caret blinks
    private const float fCARET_BLINK_RATE = 1.0f;
    // During the blink period, the amount the caret is visible. 0.5 equals
    // half the time, 0.75 equals 3/4ths of the time, etc.
    private const float fCARET_ON_RATIO = 0.75f;

    private GUIFont _font = null;
    private GUIFont _font2 = null;
    private GUIFont _fontTextBox = null;
    [XMLSkinElement("font")] protected string _fontName = "font14";
    [XMLSkinElement("font2")] protected string _fontName2 = "font13";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolor2")] protected long _textColor2 = 0xFFFFFFFF;

    [XMLSkinElement("textboxFont")] protected string _textBoxFontName = "font13";
    [XMLSkinElement("textboxXpos")] protected int _xPositionTextBox = 200;
    [XMLSkinElement("textboxYpos")] protected int _yPositionTextBox = 300;
    [XMLSkinElement("textboxWidth")] protected int _widthTextBox = 100;
    [XMLSkinElement("textboxHeight")] protected int _heightTextBox = 30;
    [XMLSkinElement("textboxColor")] protected long _textBoxColor = 0xFFFFFFFF;
    [XMLSkinElement("textboxBgColor")] protected long _textBoxBackgroundColor = 0xFFFFFFFF;
    protected string _lineData = "";
    protected int _position = 0;
    private DateTime _timerCaret = DateTime.Now;
    private DateTime _timerKey = DateTime.Now;
    private char _currentKey = (char) 0;
    private char _previousKey = (char) 0;
    private bool _usingKeyboard = false;
    private bool _needRefresh = false;
    private GUIImage _image;

    public GUISMSInputControl(int dwParentID)
      : base(dwParentID)
    {
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_fontName != "" && _fontName != "-")
      {
        _font = GUIFontManager.GetFont(_fontName);
      }

      if (_fontName2 != "" && _fontName2 != "-")
      {
        _font2 = GUIFontManager.GetFont(_fontName2);
      }

      if (_textBoxFontName != "" && _textBoxFontName != "-")
      {
        _fontTextBox = GUIFontManager.GetFont(_textBoxFontName);
      }
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleHorizontal(ref _xPositionTextBox);
      GUIGraphicsContext.ScaleVertical(ref _yPositionTextBox);
      GUIGraphicsContext.ScaleHorizontal(ref _widthTextBox);
      GUIGraphicsContext.ScaleVertical(ref _heightTextBox);
    }

    public override void AllocResources()
    {
      _usingKeyboard = false;
      base.AllocResources();
      _timerCaret = DateTime.Now;
      _timerKey = DateTime.Now;
      _lineData = "";
      _position = 0;
      if (_fontName != "" && _fontName != "-")
      {
        _font = GUIFontManager.GetFont(_fontName);
      }

      if (_fontName2 != "" && _fontName2 != "-")
      {
        _font2 = GUIFontManager.GetFont(_fontName2);
      }

      if (_textBoxFontName != "" && _textBoxFontName != "-")
      {
        _fontTextBox = GUIFontManager.GetFont(_textBoxFontName);
      }

      _image = new GUIImage(this.GetID, 1, 0, 0, _widthTextBox, 10, "bar_hor.png", 1);
      _image.ParentControl = this;
      _image.DimColor = DimColor;
      _image.AllocResources();
    }

    public override void FreeResources()
    {
      if (_image != null)
      {
        _image.FreeResources();
      }
      _image = null;

      base.FreeResources();
    }

    //TODO: add implementation
    public override bool OnMessage(GUIMessage message)
    {
      return base.OnMessage(message);
    }

    public override bool CanFocus()
    {
      return true;
    }

    public string Text
    {
      get { return _lineData; }
      set { _lineData = value; }
    }

    //TODO: add implementation
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOVE_LEFT:
          {
            if (_position > 0)
            {
              _position--;
              _needRefresh = true;
            }
            return;
          }
        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            if (_position < _lineData.Length)
            {
              _position++;
              _needRefresh = true;
            }
            return;
          }

        case Action.ActionType.ACTION_SELECT_ITEM:
          {
            // Enter key or Ok button (not working when clicked outside the box)
            return;
          }

        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            // Enter key or OK button
            if (action.m_key.KeyChar == (int) Keys.Enter)
            {
              if (_currentKey != (char) 0)
              {
                _lineData = _lineData.Insert(_position, _currentKey.ToString());
              }
              _previousKey = (char) 0;
              _currentKey = (char) 0;
              _timerKey = DateTime.Now;
              _position = 0;
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED, WindowId, GetID, ParentID,
                                              0, 0, null);
              msg.Label = _lineData;
              if (OnTextChanged != null)
              {
                OnTextChanged();
              }
              _lineData = "";
              GUIGraphicsContext.SendMessage(msg);
              return;
            }

            if ((action.m_key.KeyChar >= 32) || (action.m_key.KeyChar == (int) Keys.Back))
            {
              Press((char) action.m_key.KeyChar);
              if (OnTextChanged != null)
              {
                OnTextChanged();
              }
              return;
            }
          }
          break;
      }
      base.OnAction(action);
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 800)
      {
        if (_currentKey != (char) 0)
        {
          if (_position == _lineData.Length)
          {
            _lineData += _currentKey;
          }
          else
          {
            _lineData = _lineData.Insert(_position, _currentKey.ToString());
          }
          _position++;
          _needRefresh = true;
        }
        _previousKey = (char) 0;
        _currentKey = (char) 0;
        _timerKey = DateTime.Now;
      }
    }

    public override bool NeedRefresh()
    {
      if (_needRefresh)
      {
        _needRefresh = false;
        return true;
      }
      return false;
    }

    private void Press(char Key)
    {
      // Check keyboard
      if (Key < '0' || Key > '9')
      {
        _usingKeyboard = true;
      }

      if (!_usingKeyboard)
      {
        // Check different key pressed
        if (Key != _previousKey && _currentKey != (char) 0)
        {
          if (_position == _lineData.Length)
          {
            _lineData += _currentKey;
          }
          else
          {
            _lineData = _lineData.Insert(_position, _currentKey.ToString());
          }

          _previousKey = (char) 0;
          _currentKey = (char) 0;
          _timerKey = DateTime.Now;
          _position++;
        }

        CheckTimer();
        if (Key >= '0' && Key <= '9')
        {
          _previousKey = Key;
        }
        if (Key == '0')
        {
          _timerKey = DateTime.Now;
          if (_position > 0)
          {
            _lineData = _lineData.Remove(_position - 1, 1);
            _position--;
          }
          _timerKey = DateTime.Now;
          _previousKey = (char) 0;
          _currentKey = (char) 0;
        }
        if (Key == '1')
        {
          if (_currentKey == 0)
          {
            _currentKey = ' ';
          }
          else if (_currentKey == ' ')
          {
            _currentKey = '!';
          }
          else if (_currentKey == '!')
          {
            _currentKey = '?';
          }
          else if (_currentKey == '?')
          {
            _currentKey = '.';
          }
          else if (_currentKey == '.')
          {
            _currentKey = '0';
          }
          else if (_currentKey == '0')
          {
            _currentKey = '1';
          }
          else if (_currentKey == '1')
          {
            _currentKey = 'ß';
          }
          else if (_currentKey == 'ß')
          {
            _currentKey = '-';
          }
          else if (_currentKey == '-')
          {
            _currentKey = '+';
          }
          else if (_currentKey == '+')
          {
            _currentKey = ' ';
          }
          _timerKey = DateTime.Now;
        }

        if (Key == '2')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'a';
          }
          else if (_currentKey == 'a')
          {
            _currentKey = 'b';
          }
          else if (_currentKey == 'b')
          {
            _currentKey = 'c';
          }
          else if (_currentKey == 'c')
          {
            _currentKey = '2';
          }
          else if (_currentKey == '2')
          {
            _currentKey = 'ä';
          }
          else if (_currentKey == 'ä')
          {
            _currentKey = 'à';
          }
          else if (_currentKey == 'à')
          {
            _currentKey = 'á';
          }
          else if (_currentKey == 'á')
          {
            _currentKey = 'â';
          }
          else if (_currentKey == 'â')
          {
            _currentKey = 'a';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '3')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'd';
          }
          else if (_currentKey == 'd')
          {
            _currentKey = 'e';
          }
          else if (_currentKey == 'e')
          {
            _currentKey = 'f';
          }
          else if (_currentKey == 'f')
          {
            _currentKey = '3';
          }
          else if (_currentKey == '3')
          {
            _currentKey = 'è';
          }
          else if (_currentKey == 'è')
          {
            _currentKey = 'é';
          }
          else if (_currentKey == 'é')
          {
            _currentKey = 'ê';
          }
          else if (_currentKey == 'ê')
          {
            _currentKey = 'd';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '4')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'g';
          }
          else if (_currentKey == 'g')
          {
            _currentKey = 'h';
          }
          else if (_currentKey == 'h')
          {
            _currentKey = 'i';
          }
          else if (_currentKey == 'i')
          {
            _currentKey = '4';
          }
          else if (_currentKey == '4')
          {
            _currentKey = 'ì';
          }
          else if (_currentKey == 'ì')
          {
            _currentKey = 'í';
          }
          else if (_currentKey == 'í')
          {
            _currentKey = 'î';
          }
          else if (_currentKey == 'î')
          {
            _currentKey = 'g';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '5')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'j';
          }
          else if (_currentKey == 'j')
          {
            _currentKey = 'k';
          }
          else if (_currentKey == 'k')
          {
            _currentKey = 'l';
          }
          else if (_currentKey == 'l')
          {
            _currentKey = '5';
          }
          else if (_currentKey == '5')
          {
            _currentKey = 'j';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '6')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'm';
          }
          else if (_currentKey == 'm')
          {
            _currentKey = 'n';
          }
          else if (_currentKey == 'n')
          {
            _currentKey = 'o';
          }
          else if (_currentKey == 'o')
          {
            _currentKey = '6';
          }
          else if (_currentKey == '6')
          {
            _currentKey = 'ö';
          }
          else if (_currentKey == 'ö')
          {
            _currentKey = 'ò';
          }
          else if (_currentKey == 'ò')
          {
            _currentKey = 'ó';
          }
          else if (_currentKey == 'ó')
          {
            _currentKey = 'ô';
          }
          else if (_currentKey == 'ô')
          {
            _currentKey = 'm';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '7')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'p';
          }
          else if (_currentKey == 'p')
          {
            _currentKey = 'q';
          }
          else if (_currentKey == 'q')
          {
            _currentKey = 'r';
          }
          else if (_currentKey == 'r')
          {
            _currentKey = 's';
          }
          else if (_currentKey == 's')
          {
            _currentKey = '7';
          }
          else if (_currentKey == '7')
          {
            _currentKey = 'p';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '8')
        {
          if (_currentKey == 0)
          {
            _currentKey = 't';
          }
          else if (_currentKey == 't')
          {
            _currentKey = 'u';
          }
          else if (_currentKey == 'u')
          {
            _currentKey = 'v';
          }
          else if (_currentKey == 'v')
          {
            _currentKey = '8';
          }
          else if (_currentKey == '8')
          {
            _currentKey = 'ü';
          }
          else if (_currentKey == 'ü')
          {
            _currentKey = 'ù';
          }
          else if (_currentKey == 'ù')
          {
            _currentKey = 'ú';
          }
          else if (_currentKey == 'ú')
          {
            _currentKey = 'û';
          }
          else if (_currentKey == 'û')
          {
            _currentKey = 't';
          }
          _timerKey = DateTime.Now;
        }
        if (Key == '9')
        {
          if (_currentKey == 0)
          {
            _currentKey = 'w';
          }
          else if (_currentKey == 'w')
          {
            _currentKey = 'x';
          }
          else if (_currentKey == 'x')
          {
            _currentKey = 'y';
          }
          else if (_currentKey == 'y')
          {
            _currentKey = 'z';
          }
          else if (_currentKey == 'z')
          {
            _currentKey = '9';
          }
          else if (_currentKey == '9')
          {
            _currentKey = 'w';
          }
          _timerKey = DateTime.Now;
        }
        if (Key < '0' || Key > '9')
        {
          _usingKeyboard = true;
          if (_position == _lineData.Length)
          {
            _lineData += _currentKey;
          }
          else
          {
            _lineData = _lineData.Insert(_position, _currentKey.ToString());
          }

          _previousKey = (char) 0;
          _currentKey = (char) 0;
          _timerKey = DateTime.Now;
          _position++;
        }
      }
      else
      {
        if (Key == (char) 8) // Backspace
        {
          if (_position > 0)
          {
            _lineData = _lineData.Remove(_position - 1, 1);
            _position--;
          }
        }
        else
        {
          if (_position >= _lineData.Length)
          {
            _lineData += Key;
          }
          else
          {
            _lineData = _lineData.Insert(_position, Key.ToString());
          }
          _position++;
        }

        _previousKey = (char) 0;
        _currentKey = (char) 0;
        _timerKey = DateTime.Now;
      }

      _needRefresh = true;
    }

    public override void Render(float timePassed)
    {
      DrawInput();
      DrawTextBox(timePassed);
      DrawText();
      CheckTimer();
      base.Render(timePassed);
    }

    private void DrawInput()
    {
      int posY = _positionY;
      int step = 20;
      GUIGraphicsContext.ScaleVertical(ref step);
      _font.DrawText(_positionX, posY, _textColor, " 1     2       3", Alignment.ALIGN_LEFT, -1);
      posY += step;
      _font2.DrawText(_positionX, posY, _textColor2, " _    abc    def", Alignment.ALIGN_LEFT, -1);
      posY += step;

      posY += step;
      _font.DrawText(_positionX, posY, _textColor, " 4     5      6", Alignment.ALIGN_LEFT, -1);
      posY += step;
      _font2.DrawText(_positionX, posY, _textColor2, "ghi   jkl    mno", Alignment.ALIGN_LEFT, -1);
      posY += step;

      posY += step;
      _font.DrawText(_positionX, posY, _textColor, " 7     8      9", Alignment.ALIGN_LEFT, -1);
      posY += step;
      _font2.DrawText(_positionX, posY, _textColor2, "pqrs tuv wxyz", Alignment.ALIGN_LEFT, -1);
      posY += step;
    }

    private void DrawTextBox(float timePassed)
    {
      int x1 = _xPositionTextBox;
      int y1 = _yPositionTextBox;
      int x2 = _xPositionTextBox + _widthTextBox;
      int y2 = _yPositionTextBox + _heightTextBox;

      //GUIGraphicsContext.ScalePosToScreenResolution(ref x2, ref y2);   <-- Remarked because it seems to be done twice and lead to a distortion of the textbox in fullscreen (resolution depeding)

      _image.SetPosition(x1, y1);
      _image.Width = (x2 - x1);
      _image.Height = (y2 - y1);
      _image.Render(timePassed);
    }

    private void DrawText()
    {
      string line = _lineData;

      if (_currentKey != (char) 0)
      {
        line = line.Insert(_position, _currentKey.ToString());
        line = line.Insert(_position + 1, "_");
      }
      else
      {
        line = line.Insert(_position, "_");
      }

      float fCaretWidth = 0.0f;
      float fCaretHeight = 0.0f;

      _fontTextBox.GetTextExtent(line, ref fCaretWidth, ref fCaretHeight);
      if (GUIGraphicsContext.graphics != null)
      {
        fCaretWidth += line.Length;
      }
      while (fCaretWidth > _widthTextBox)
      {
        line = line.Remove(0, 3);
        line = line.Insert(0, ".");
        line = line.Insert(0, ".");
        _fontTextBox.GetTextExtent(line, ref fCaretWidth, ref fCaretHeight);
        if (GUIGraphicsContext.graphics != null)
        {
          fCaretWidth += line.Length;
        }
      }

      _fontTextBox.DrawText(_xPositionTextBox, _yPositionTextBox, _textBoxColor, line, Alignment.ALIGN_LEFT, -1);

      /*
            // Draw blinking caret using line primitives.
            TimeSpan ts=DateTime.Now-_timerCaret;
            if(  (ts.TotalSeconds % fCARET_BLINK_RATE ) < fCARET_ON_RATIO )
            {
              string strLine=_lineData.Substring( 0, _position );

              float fCaretWidth = 0.0f;
              float fCaretHeight=0.0f;
              _fontTextBox.GetTextExtent( strLine, ref fCaretWidth, ref fCaretHeight );
              if (GUIGraphicsContext.graphics!=null) fCaretWidth+=strLine.Length;
              _fontTextBox.DrawText( _xPositionTextBox+(int)fCaretWidth, _yPositionTextBox, 0xff202020, "|", GUIControl.Alignment.ALIGN_LEFT, -1 ); 
            }
      */
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
        _font = GUIFontManager.GetFont(value);
        _fontName = value;
      }
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName2
    {
      get { return _fontName2; }
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
        _font2 = GUIFontManager.GetFont(value);
        _fontName2 = value;
      }
    }

    /// <summary>
    /// Get/set the name of the font used in the textbox.
    /// </summary>
    public string TextBoxFontName
    {
      get { return _textBoxFontName; }
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
        _fontTextBox = GUIFontManager.GetFont(value);
        _textBoxFontName = value;
      }
    }


    /// <summary>
    /// Get/set the textcolor of the text 0-9
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/set the textcolor of the text a-z
    /// </summary>
    public long TextColor2
    {
      get { return _textColor2; }
      set { _textColor2 = value; }
    }

    /// <summary>
    /// Get/set the textcolor of the textbox
    /// </summary>
    public long TextBoxColor
    {
      get { return _textBoxColor; }
      set { _textBoxColor = value; }
    }

    /// <summary>
    /// Get/set the backgroundcolor of the textbox
    /// </summary>
    public long TextBoxBackGroundColor
    {
      get { return _textBoxBackgroundColor; }
      set { _textBoxBackgroundColor = value; }
    }

    /// <summary>
    /// Get/set the x position of the textbox
    /// </summary>
    public int TextBoxX
    {
      get { return _xPositionTextBox; }
      set { _xPositionTextBox = value; }
    }

    /// <summary>
    /// Get/set the y position of the textbox
    /// </summary>
    public int TextBoxY
    {
      get { return _yPositionTextBox; }
      set { _yPositionTextBox = value; }
    }

    /// <summary>
    /// Get/set the Width of the textbox
    /// </summary>
    public int TextBoxWidth
    {
      get { return _widthTextBox; }
      set { _widthTextBox = value; }
    }

    /// <summary>
    /// Get/set the Height of the textbox
    /// </summary>
    public int TextBoxHeight
    {
      get { return _heightTextBox; }
      set { _heightTextBox = value; }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        _image.DimColor = value;
      }
    }

    /// <summary>
    /// Get/set the current cursor position
    /// </summary>
    public int CursorPosition
    {
      get { return _position; }
      set { _position = value; }
    }
  }
}