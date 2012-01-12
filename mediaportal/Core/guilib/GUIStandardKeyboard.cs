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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public class GUIStandardKeyboard : GUIKeyboard
  {
    public GUIStandardKeyboard(int dwParentID)
      : base(dwParentID) {}

    public override void InitializeInstance() {}

    protected override void MoveLeft()
    {
      if (_currentKey == -1)
      {
        _currentKey = _lastColumn;
      }

      do
      {
        if (_currentKey <= 0)
        {
          ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
          ArrayList row = (ArrayList)board[_currentRow];
          _currentKey = row.Count - 1;
        }
        else
        {
          --_currentKey;
        }
      } while (IsKeyDisabled());

      SetLastColumn();
    }

    protected override void MoveRight()
    {
      if (_currentKey == -1)
      {
        _currentKey = _lastColumn;
      }

      do
      {
        ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
        ArrayList row = (ArrayList)board[_currentRow];

        if (_currentKey == row.Count - 1)
        {
          _currentKey = 0;
        }
        else
        {
          ++_currentKey;
        }
      } while (IsKeyDisabled());

      SetLastColumn();
    }

    protected override void Press(char k)
    {
      ClearLabelAsInitialText();

      // Don't add more than the maximum characters, and don't allow 
      // text to exceed the width of the text entry field
      if (_textEntered.Length < MAX_CHARS)
      {
        float fWidth = 0, fHeight = 0;
        _fontSearchText.GetTextExtent(_textEntered, ref fWidth, ref fHeight);

        if (fWidth < (GUIGraphicsContext.ScaleHorizontal((int)fTextBoxWidth)))
        {
          if (_position >= _textEntered.Length)
          {
            _textEntered += k.ToString();
            OnTextChanged();
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, k.ToString());
            OnTextChanged();
          }
          ++_position; // move the caret
        }
      }

      // Unstick the shift key
      _shiftTurnedOn = false;
    }

    protected override void RenderKeyboardLatin(float timePassed)
    {
      // Compute the width and height of the keyboard (not including the input text box)
      int keyboardWidth = _modeKeyWidth + _modeKeySpacing + (_keyWidth * 10) + (_keyHorizontalSpacing * 9);
      int keyboardHeight = (_keyHeight * 5);

      keyboardX = _keyboardPosX;
      keyboardY = _keyboardPosY;

      if (_keyboardPosX < 0)
      {
        keyboardX = (GUIGraphicsContext.SkinSize.Width - keyboardWidth) / 2;
      }
      if (_keyboardPosY < 0)
      {
        keyboardY = (GUIGraphicsContext.SkinSize.Height - keyboardHeight) / 2;
      }

      GUIGraphicsContext.ScalePosToScreenResolution(ref keyboardX, ref keyboardY);

      int x1, y1, x2, y2;

      // Skip drawing the label if there is no text or it should be used as initial text.
      if (_labelText != "" && !_showLabelAsInitialText)
      {
        // Show the label
        x1 = _labelBoxPosX;
        y1 = _labelBoxPosY;
        x2 = x1 + _labelBoxWidth;
        y2 = _labelBoxPosY + _labelBoxHeight;

        // Negative value for the label box width indicates that it should be calculated to be the same width as the keyboard.
        if (_labelBoxWidth < 0)
        {
          int modeKeyWidth = _modeKeyWidth;
          if (_useSearchLayout)
          {
            modeKeyWidth = _searchModeKeyWidth;
          }
          x2 = x1 + keyboardWidth;
        }
        fLabelBoxWidth = (float)(x2 - x1);

        DrawLabelBox(timePassed, x1, y1, x2, y2);

        // Show text input and caret
        switch (_labelAlign)
        {
          case GUIControl.Alignment.ALIGN_LEFT:
            x1 = _labelBoxPosX + _labelOffX;
            y1 = _labelBoxPosY + _labelOffY;
            break;
          case GUIControl.Alignment.ALIGN_CENTER:
            x1 = _labelBoxPosX + (int)(fLabelBoxWidth / 2.0);
            y1 = _labelBoxPosY + _labelOffY;
            break;
          case GUIControl.Alignment.ALIGN_RIGHT:
            x1 = _labelBoxPosX + (int)fLabelBoxWidth - _labelOffX;
            y1 = _labelBoxPosY + _labelOffY;
            break;
        }

        DrawLabel(timePassed, x1, y1);
      }

      // Show the text input box
      x1 = _inputTextBoxPosX;
      y1 = _inputTextBoxPosY;
      x2 = x1 + _inputTextBoxWidth;
      y2 = _inputTextBoxPosY + _inputTextBoxHeight;

      // Negative value for the text box width indicates that it shold be calculated to be the same width as the keyboard.
      if (_inputTextBoxWidth < 0)
      {
        int modeKeyWidth = _modeKeyWidth;
        if (_useSearchLayout)
        {
          modeKeyWidth = _searchModeKeyWidth;
        }
        x2 = x1 + keyboardWidth;
      }
      fTextBoxWidth = (float)(x2 - x1 - 2 * _inputTextOffX);

      DrawTextBox(timePassed, x1, y1, x2, y2);

      // Show text input and caret
      switch (_inputTextAlign)
      {
        case GUIControl.Alignment.ALIGN_LEFT:
          x1 = _inputTextBoxPosX + _inputTextOffX;
          y1 = _inputTextBoxPosY + _inputTextOffY;
          break;
        case GUIControl.Alignment.ALIGN_CENTER:
          x1 = _inputTextBoxPosX + (int)(fTextBoxWidth / 2.0);
          y1 = _inputTextBoxPosY + _inputTextOffY;
          break;
        case GUIControl.Alignment.ALIGN_RIGHT:
          x1 = _inputTextBoxPosX + (int)fTextBoxWidth - _inputTextOffX;
          y1 = _inputTextBoxPosY + _inputTextOffY;
          break;
      }

      DrawText(timePassed, x1, y1);

      x1 = keyboardX;
      y1 = keyboardY;

      // Draw each row
      float fY = y1;
      ArrayList keyBoard = (ArrayList)_keyboardList[(int)_currentKeyboard];
      for (int row = 0; row < _maxRows; ++row, fY += (_keyHeightScaled + _keyVerticalSpacing))
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList)keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          // Determine key name
          Key key = (Key)keyRow[i];
          long selKeyColor = 0xffffffff;
          long selTextColor = _keyFontColor;

          // Handle special key coloring
          switch (key.xKey)
          {
            case Xkey.XK_SHIFT:
              switch (_currentKeyboard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (_shiftTurnedOn)
                  {
                    selKeyColor = _keyPressedColor;
                  }
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = _keyDisabledColor;
                  selTextColor = _keyDisabledFontColor;
                  break;
              }
              break;
            case Xkey.XK_CAPSLOCK:
              switch (_currentKeyboard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (_capsLockTurnedOn)
                  {
                    selKeyColor = _keyPressedColor;
                  }
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = _keyDisabledColor;
                  selTextColor = _keyDisabledFontColor;
                  break;
              }
              break;
            case Xkey.XK_SMS:
              if (SmsStyleText)
              {
                selKeyColor = _keyPressedColor;
                //key.name = "SMS";
              }
              else
              {
                //key.name = "STANDARD";
              }
              break;
          }

          // Highlight the current key
          key.inFocus = false;
          if (row == _currentRow && dwIndex == _currentKey)
          {
            selKeyColor = _keyHighlightColor;
            selTextColor = _keySelFontColor;
            key.inFocus = true;
          }

          RenderKey(timePassed, fX + fWidthSum, fY, key, selKeyColor, selTextColor);

          int width = key.dwWidth;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          // There's a slightly larger gap between the leftmost keys (mode keys) and the main keyboard
          if (dwIndex == 0)
          {
            width = _modeKeySpacing;
          }
          else
          {
            width = _keyHorizontalSpacing;
          }
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          ++dwIndex;
        }
      }
    }
  }
}