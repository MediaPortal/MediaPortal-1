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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using ProcessPlugins.ExternalDisplay.Noritake;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// CrystalFontz 634 driver
  /// </summary>
  /// <author>JoeDalton</author>
  public class NoritakeGU7000 : BaseDisplay, IDisplay
  {
    private SerialPort commPort = null;
    private int lines = 4;

    private bool isDisabled = false;
    private string errorMessage = "";

    public bool IsDisabled
    {
      get { return isDisabled; }
    }

    public string ErrorMessage
    {
      get { return errorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    public void DrawImage(Bitmap bitmap)
    {
    }


    /// <summary>
    /// Displays the message on the indicated line
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      if (line >= lines)
      {
        Log.Error("NoritakeGU7000.SetLine: error bad line number" + line);
        return;
      }
      try
      {
        SetPosition(line, 0);
        Write(message);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Short name of this display driver
    /// </summary>
    public string Name
    {
      get { return "NoritakeGU7000"; }
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "Noritake GU7000 Serial driver v0.1b"; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this driver support graphic mode?
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
      MessageBox.Show("No advanced configuration", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _contrast)
    {
      lines = _lines;
      try
      {
        commPort = new SerialPort(_port, 38400, Parity.None, 8, StopBits.One);
        commPort.Handshake = Handshake.RequestToSend;
        //enable RTS and DTR lines to power the display
        commPort.DtrEnable = true;
        commPort.RtsEnable = true;
        commPort.Open();
        Write(new byte[] {0x1B, 0x40});
        SetFont(FontSet.America);
        SetCodePage(CodePage.PC850);
        Brightness(_contrast); //Use contrast slider to control brightness
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    #region IDisposable Members

    public void Dispose()
    {
      try
      {
        if (commPort != null)
        {
          if (commPort.IsOpen)
          {
            commPort.Close();
          }
          commPort.Dispose();
          commPort = null;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    #endregion

    #region Display functions

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      Clear();
    }

    public void CleanUp()
    {
      Clear();
    }

    private void Clear()
    {
      Write(new byte[] {0xC});
    }

    private void SetPosition(int line, int pos)
    {
      Write(
        new byte[]
          {0x1F, 0x24, (byte) (pos%0x100), (byte) (pos/0x100), (byte) (line%0x100), (byte) (line/0x100)});
    }

    private void SetFont(FontSet fontSet)
    {
      Write(new byte[] {0x1B, 0x52, (byte) fontSet});
    }

    private void SetCodePage(CodePage codePage)
    {
      Write(new byte[] {0x1B, 0x74, (byte) codePage});
    }

    private void Write(string text)
    {
      commPort.Write(text);
    }

    private void Brightness(int level)
    {
      Write(new byte[] {0x1F, 0x58, (byte) (level/12.5)});
    }

    #endregion

    private void Write(byte[] bytes)
    {
      commPort.Write(bytes, 0, bytes.Length);
    }
  }
}

namespace ProcessPlugins.ExternalDisplay.Noritake
{
  public enum FontSet : byte
  {
    America = 0,
    France,
    Germany,
    England,
    Denmark1,
    Sweden,
    Italy,
    Spain1,
    Japan,
    Norway,
    Denmark2,
    Spain2,
    LatinAmerica,
    Korea
  }

  public enum CodePage : byte
  {
    PC437 = 0, //USA, Europe
    Katakana, //Japanese
    PC850, //Multilingual
    PC860, //Portugese
    PC863, //Canadian-French
    PC865, //Nordic
    WPC1252,
    PC866, //Cyrillic 2
    PC842, //Latin 2
    PC858
  }
}