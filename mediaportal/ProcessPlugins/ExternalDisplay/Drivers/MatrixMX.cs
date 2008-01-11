#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// Matrix Orbital Character LCD (MX2/MX3/MX6) Driver
  /// </summary>
  /// <author>CybrMage</author>
  public class MatrixMX : BaseDisplay, IDisplay
  {
    private bool _IsDisabled = false;
    private readonly string _ErrorMessage = "";
    private readonly MODisplay MOD = new MODisplay();
    private bool _IsOpen = false;
    private bool _BackLightControl = false;
    private int _Tcols;
    private int _Trows;

    #region IDisplay Members

    public bool IsDisabled
    {
      get { return _IsDisabled; }
    }

    public string ErrorMessage
    {
      get { return _ErrorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters) {}

    public void DrawImage(Bitmap bitmap) {}


    /// <summary>
    /// Displays the message on the indicated line
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      MOD.SetLine(line, message);
    }

    /// <summary>
    /// Short name of this display driver
    /// </summary>
    public string Name
    {
      get { return "MatrixMX"; }
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "Matrix Orbital Character LCD driver V1.0"; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      //Nothing to configure
    }

    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="_port">The port the display is connected to</param>
    /// <param name="_lines">The number of lines in text mode</param>
    /// <param name="_cols">The number of columns in text mode</param>
    /// <param name="_delay">Communication delay in text mode</param>
    /// <param name="_linesG">The height in pixels in graphic mode</param>
    /// <param name="_colsG">The width in pixels in graphic mode</param>
    /// <param name="_delayG">Communication delay in graphic mode</param>
    /// <param name="_backLight">Backlight on?</param>
    /// <param name="_contrast">Contrast (ignored)</param>
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _contrast)
    {
      // MX series display
      _Trows = _lines;
      if (_Trows > 2)
      {
        Log.Info("IDisplay: MatrixMX.Setup() - Invalid Text Lines value");
        _Trows = 2;
      }
      _Tcols = _cols;
      if (_Tcols > 20)
      {
        Log.Info("IDisplay: MatrixMX.Setup() - Invalid Text Columns value");
        _Tcols = 20;
      }
      _IsOpen = MOD.OpenDisplay(_port, _contrast);
      if (_IsOpen)
      {
        _BackLightControl = _backLight;
        _IsDisabled = false;
      }
      else
      {
        _IsDisabled = true;
      }
    }

    public void Initialize()
    {
      Clear();
    }

    public void CleanUp()
    {
      MOD.ClearDisplay();
      MOD.CloseDisplay(_BackLightControl);
    }

    private void Clear()
    {
      MOD.ClearDisplay();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      MOD.CloseDisplay(_BackLightControl);
    }

    #endregion

    private class MODisplay
    {
      private SerialPort commPort = null;
      private bool _isOpen;
      private int _currentContrast;
      private readonly int _currentBrightness = 128;

      private enum KEYPAD_Codes : byte
      {
        CursorUp = 0x4B,
        CursorDown = 0x4C,
        CursorLeft = 0x52,
        CursorRight = 0x46,
        Enter = 0x4A,
        F2 = 0x50,
        F1 = 0x51
      }

      public bool OpenDisplay(string _port, int _contrast)
      {
        try
        {
          _currentContrast = _contrast;
          commPort = new SerialPort(_port, 19200, Parity.None, 8, StopBits.One);
          commPort.DataReceived += WhenDataReceived;
          commPort.ReceivedBytesThreshold = 1;
          commPort.Open();
          BacklightOn();
          SetBacklightBrightness(_currentBrightness);
          ClearDisplay();
          // set up for receiving keypad data
          Key_AutoRepeatModeOn(1);
          Key_ClearKeyBuffer();
          Key_SetDebounceTime(128);
          Key_AutoTransmitKeypressOn();
          commPort.DiscardInBuffer();
          _isOpen = true;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          _isOpen = false;
        }
        return _isOpen;
      }

      public void CloseDisplay(bool _backlight)
      {
        try
        {
          if (_isOpen & commPort.IsOpen)
          {
            if (_backlight)
            {
              for (int i = _currentContrast; i > 0; i--)
              {
                SetBacklightBrightness(i);
                Thread.Sleep(5);
              }
              BacklightOff();
            }
            ClearDisplay();
            if ((commPort != null) && (commPort.IsOpen))
            {
              commPort.Close();
              commPort.DataReceived -= WhenDataReceived;
            }
            _isOpen = false;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }

      private void WhenDataReceived(object sender, SerialDataReceivedEventArgs e)
      {
        // KeyPad data has been received from the serial port
        Log.Info("MODisplay: received KeyPad event");
        Action keyAction;
        byte rByte = (byte) commPort.ReadByte();
        // process the keypress
        if (rByte == (byte) KEYPAD_Codes.CursorUp)
        {
          // cursor up
          Log.Info("MODisplay: received KeyPad event - Cursor Up {0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_MOVE_UP, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else if (rByte == (byte) KEYPAD_Codes.CursorDown)
        {
          // cursor down
          Log.Info("MODisplay: received KeyPad event - Cursor Down {0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_MOVE_DOWN, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else if (rByte == (byte) KEYPAD_Codes.CursorLeft)
        {
          // cursor left
          Log.Info("MODisplay: received KeyPad event - Cursor Left{0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else if (rByte == (byte) KEYPAD_Codes.CursorRight)
        {
          // cursor right
          Log.Info("MODisplay: received KeyPad event - Cursor Right{0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_MOVE_RIGHT, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else if (rByte == (byte) KEYPAD_Codes.Enter)
        {
          // Enter key
          Log.Info("MODisplay: received KeyPad event - Enter {0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_SELECT_ITEM, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else if (rByte == (byte) KEYPAD_Codes.F2)
        {
          // F2 key
          Log.Info("MODisplay: received KeyPad event - F1 {0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_STEP_BACK, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else if (rByte == (byte) KEYPAD_Codes.F1)
        {
          // F1 key
          Log.Info("MODisplay: received KeyPad event - F2 {0}", rByte.ToString("x00"));
          keyAction = new Action(Action.ActionType.ACTION_STOP, 0, 0);
          GUIGraphicsContext.OnAction(keyAction);
        }
        else
        {
          Log.Info("MODisplay: received KeyPad event - received byte {0} Unknown Key", rByte.ToString("x00"));
        }
      }

      public void Key_AutoRepeatModeOn(int _mode)
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x7E, (byte) _mode}, 0, 3);
      }

      public void Key_AutoTransmitKeypressOn()
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x41}, 0, 2);
      }

      public void Key_ClearKeyBuffer()
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x45}, 0, 2);
      }

      public void Key_SetDebounceTime(int _time)
      {
        // _time = ms * 0.6554
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x55, (byte) _time}, 0, 3);
      }

      public void CursorPosition(int column, int row)
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x47, (byte) column, (byte) row}, 0, 4);
      }

      public void ClearDisplay()
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x58}, 0, 2);
      }

      public void SetContrast(int contrast)
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x50, (byte) contrast}, 0, 3);
      }

      public void BacklightOn()
      {
        BacklightOn(0);
      }

      public void BacklightOn(int MinutesTillOff)
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x42, (byte) MinutesTillOff}, 0, 3);
      }

      public void BacklightOff()
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x46}, 0, 2);
      }

      public void SetBacklightBrightness(int brightness)
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        commPort.Write(new byte[] {0xFE, 0x99, (byte) brightness}, 0, 3);
      }

      public void SetLine(int _line, string _message)
      {
        if (!_isOpen | !commPort.IsOpen)
        {
          return;
        }
        // make sure the backlight, brightness and contrast are set properly
        BacklightOn();
        SetBacklightBrightness(_currentBrightness);
        SetContrast(_currentContrast);

        if (_line == 0)
        {
          CursorPosition(1, 1);
        }
        else
        {
          CursorPosition(1, 2);
        }
        for (int i = 0; i < 20; i++)
        {
          if (i < _message.Length)
          {
            commPort.Write(new byte[] {(byte) _message[i]}, 0, 1);
          }
          else
          {
            commPort.Write(new byte[] {0x20}, 0, 1);
          }
        }
      }

      #region Unused Methods

      //public void ReadModuleType()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x37 }, 0, 2);
      //}

      //public void ReadSerialNumber()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x35 }, 0, 2);
      //}

      //public void ReadVersionNumber()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x36 }, 0, 2);
      //}

      //public void FlowControlOn()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x3A }, 0, 2);
      //}

      //public void FlowControlOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x3B }, 0, 2);
      //}

      //public void Key_AutoRepeatModeOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x60 }, 0, 2);
      //}

      //public void Key_AutoTransmitKeypressOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x4F }, 0, 2);
      //}

      //public void Key_PollKeypad()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x26 }, 0, 2);
      //}

      //public void AutoLineWrapOn()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x43 }, 0, 2);
      //}

      //public void AutoLineWrapOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x44 }, 0, 2);
      //}

      //public void AutoScrollOn()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x51 }, 0, 2);
      //}

      //public void AutoScrollOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x52 }, 0, 2);
      //}

      //public void CursorHome()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x48 }, 0, 2);
      //}

      //public void CursorUnderlineOn()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x4A }, 0, 2);
      //}

      //public void CursorUnderlineOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x4B }, 0, 2);
      //}

      //public void CursorBlinkOn()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x53 }, 0, 2);
      //}

      //public void CursorBlinkOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x54 }, 0, 2);
      //}

      //public void CursorLeft()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x4C }, 0, 2);
      //}

      //public void CursorRight()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x4D }, 0, 2);
      //}

      //// Bar Graph Commands
      //public void InitWideVerticalBarGraph()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x76 }, 0, 2);
      //}

      //public void InitNarrowVerticalBarGraph()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x73 }, 0, 2);
      //}

      //public void DrawVerticalBarGraph(int column, int height)
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x3d, (byte)column, (byte)height }, 0, 4);
      //}

      //public void InitHorizontalBarGraph()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x68 }, 0, 2);
      //}

      //public void DrawHorizontalBarGraph(int column, int row, int direction, int length)
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x7C, (byte)column, (byte)row, (byte)direction, (byte)length }, 0, 6);
      //}

      //public void DefineCustomCharacter(int charIndex, int byte1, int byte2, int byte3, int byte4, int byte5, int byte6,
      //                                  int byte7, int byte8)
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(
      //    new byte[]
      //      {
      //        0xFE, 0x4E, (byte) charIndex, (byte) byte1, (byte) byte2, (byte) byte3, (byte) byte4, (byte) byte5,
      //        (byte) byte6, (byte) byte7, (byte) byte8
      //      }, 0, 11);
      //}

      //public void RememberOn()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x93, 0x01 }, 0, 3);
      //}

      //public void RememberOff()
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x93, 0x00 }, 0, 3);
      //}

      //public void SetContrastAndSave(int contrast)
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x91, (byte)contrast }, 0, 3);
      //}

      //public void SetBacklightBrightnessAndSave(int brightness)
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x98, (byte)brightness }, 0, 3);
      //}

      //public void LoadStartupScreen(string line1, string line2)
      //{
      //  if (!_isOpen | !commPort.IsOpen) return;
      //  commPort.Write(new byte[] { 0xFE, 0x40 }, 0, 2);
      //  for (int i = 0; i < 20; i++)
      //  {
      //    if (i < line1.Length)
      //    {
      //      commPort.Write(new byte[] { (byte)line1[i] }, 0, 1);
      //    }
      //    else
      //    {
      //      commPort.Write(new byte[] { 0x20 }, 0, 1);
      //    }
      //  }
      //  for (int i = 0; i < 20; i++)
      //  {
      //    if (i < line1.Length)
      //    {
      //      commPort.Write(new byte[] { (byte)line2[i] }, 0, 1);
      //    }
      //    else
      //    {
      //      commPort.Write(new byte[] { 0x20 }, 0, 1);
      //    }
      //  }
      //}

      #endregion
    }
  }
}