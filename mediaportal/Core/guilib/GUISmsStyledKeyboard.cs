using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public class GUISmsStyledKeyboard : GUIKeyboard
  {

    public GUISmsStyledKeyboard(int dwParentID)
      : base(dwParentID)
    {
    }

    public override void InitializeInstance()
    {
      _capsLockTurnedOn = true;
    }

    protected override void MoveLeft()
    {
      if (_position > 0)
      {
        _position -= 1;
      }
    }

    protected override void MoveRight()
    {
      if (_position < _textEntered.Length)
      {
        _position += 1;
      }
    }

    protected override void Press(char k)
    {
      if (k < '0' || k > '9')
      {
        _usingKeyboard = true;
      }
      if ((k == (char)126))
      {
        _capsLockTurnedOn = _capsLockTurnedOn == false;
      }

      if (!_usingKeyboard)
      {
        // Check different key presse
        if (k != _previousKey && _currentKeyb != (char)0)
        {
          if (_position == _textEntered.Length)
          {
            _textEntered += _currentKeyb;
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, _currentKeyb.ToString());
          }

          _previousKey = (char)0;
          _currentKeyb = (char)0;
          _timerKey = DateTime.Now;
          _position++;
        }

        CheckTimer();
        if (k >= '0' && k <= '9')
        {
          _previousKey = k;
        }
        if (k == '0')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = ' ';
          }
          else if (_currentKeyb == ' ')
          {
            _currentKeyb = '0';
          }
          else if (_currentKeyb == '0')
          {
            _currentKeyb = ' ';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '1')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = '1';
          }
          _timerKey = DateTime.Now;
        }

        if (k == '2')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'a';
          }
          else if (_currentKeyb == 'a')
          {
            _currentKeyb = 'b';
          }
          else if (_currentKeyb == 'b')
          {
            _currentKeyb = 'c';
          }
          else if (_currentKeyb == 'c')
          {
            _currentKeyb = '2';
          }
          else if (_currentKeyb == '2')
          {
            _currentKeyb = 'a';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '3')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'd';
          }
          else if (_currentKeyb == 'd')
          {
            _currentKeyb = 'e';
          }
          else if (_currentKeyb == 'e')
          {
            _currentKeyb = 'f';
          }
          else if (_currentKeyb == 'f')
          {
            _currentKeyb = '3';
          }
          else if (_currentKeyb == '3')
          {
            _currentKeyb = 'd';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '4')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'g';
          }
          else if (_currentKeyb == 'g')
          {
            _currentKeyb = 'h';
          }
          else if (_currentKeyb == 'h')
          {
            _currentKeyb = 'i';
          }
          else if (_currentKeyb == 'i')
          {
            _currentKeyb = '4';
          }
          else if (_currentKeyb == '4')
          {
            _currentKeyb = 'g';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '5')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'j';
          }
          else if (_currentKeyb == 'j')
          {
            _currentKeyb = 'k';
          }
          else if (_currentKeyb == 'k')
          {
            _currentKeyb = 'l';
          }
          else if (_currentKeyb == 'l')
          {
            _currentKeyb = '5';
          }
          else if (_currentKeyb == '5')
          {
            _currentKeyb = 'j';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '6')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'm';
          }
          else if (_currentKeyb == 'm')
          {
            _currentKeyb = 'n';
          }
          else if (_currentKeyb == 'n')
          {
            _currentKeyb = 'o';
          }
          else if (_currentKeyb == 'o')
          {
            _currentKeyb = '6';
          }
          else if (_currentKeyb == '6')
          {
            _currentKeyb = 'm';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '7')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'p';
          }
          else if (_currentKeyb == 'p')
          {
            _currentKeyb = 'q';
          }
          else if (_currentKeyb == 'q')
          {
            _currentKeyb = 'r';
          }
          else if (_currentKeyb == 'r')
          {
            _currentKeyb = 's';
          }
          else if (_currentKeyb == 's')
          {
            _currentKeyb = '7';
          }
          else if (_currentKeyb == '7')
          {
            _currentKeyb = 'p';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '8')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 't';
          }
          else if (_currentKeyb == 't')
          {
            _currentKeyb = 'u';
          }
          else if (_currentKeyb == 'u')
          {
            _currentKeyb = 'v';
          }
          else if (_currentKeyb == 'v')
          {
            _currentKeyb = '8';
          }
          else if (_currentKeyb == '8')
          {
            _currentKeyb = 't';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '9')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'w';
          }
          else if (_currentKeyb == 'w')
          {
            _currentKeyb = 'x';
          }
          else if (_currentKeyb == 'x')
          {
            _currentKeyb = 'y';
          }
          else if (_currentKeyb == 'y')
          {
            _currentKeyb = 'z';
          }
          else if (_currentKeyb == 'z')
          {
            _currentKeyb = '9';
          }
          else if (_currentKeyb == '9')
          {
            _currentKeyb = 'w';
          }
          _timerKey = DateTime.Now;
        }
      }
      else
      {
        if ((k != (char)126))
        {
          if (k == (char)8) // Backspace
          {
            if (_position > 0)
            {
              _textEntered = _textEntered.Remove(_position - 1, 1);
              _position--;
            }
          }
          else
          {
            if (_position >= _textEntered.Length)
            {
              _textEntered += k;
            }
            else
            {
              _textEntered = _textEntered.Insert(_position, k.ToString());
            }
            _position++;
          }
        }
        _previousKey = (char)0;
        _currentKeyb = (char)0;
        _timerKey = DateTime.Now;
      }

      _usingKeyboard = false;

      // Unstick the shift key
      _shiftTurnedOn = false;
    }

    protected override void RenderKeyboardLatin(float timePassed)
    {
      // Show text and caret
      DrawTextBox(timePassed, _inputTextBoxPosX, _inputTextBoxPosY, _inputTextBoxPosX + _inputTextBoxWidth, _inputTextBoxPosY + _inputTextBoxHeight);
      DrawText(timePassed, _inputTextBoxPosX + _inputTextOffX, _inputTextBoxPosY + _inputTextOffY);
      CheckTimer();
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 800)
      {
        if (_currentKeyb != (char)0)
        {
          if (_capsLockTurnedOn)
          {
            _currentKeyb = char.ToUpper(_currentKeyb);
          }
          if (_position == _textEntered.Length)
          {
            _textEntered += _currentKeyb;
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, _currentKeyb.ToString());
          }
          _position++;
        }
        _previousKey = (char)0;
        _currentKeyb = (char)0;
        _timerKey = DateTime.Now;
      }
    }

    protected override void InitBoard()
    {
      base.InitBoard();
      _capsLockTurnedOn = true;
    }
  }
}
