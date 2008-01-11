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

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// CrystalFontz 634 driver
  /// </summary>
  /// <author>JoeDalton</author>
  public class CrystalFontz634 : BaseDisplay, IDisplay
  {
    private SerialPort commPort = null;
    private int lines = 4;

    private bool isDisabled = false;
    private string errorMessage = "";

    #region IDisplay Members

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
        Log.Error("CrystalFontz634.SetLine: error bad line number" + line);
        return;
      }
      try
      {
        commPort.Write(new byte[] {17, 0, (byte) line}, 0, 3);
        commPort.Write(message);
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
      get { return "CrystalFontz634"; }
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "CrystalFontz 634 driver"; }
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
        commPort = new SerialPort(_port, 19200, Parity.None, 8, StopBits.One);
        commPort.Handshake = Handshake.None;
        //enable RTS and DTR lines to power the display
        commPort.DtrEnable = true;
        commPort.RtsEnable = true;
        commPort.Open();
        commPort.Write(new byte[] {20, 24}, 0, 2); //Turn off scrolling and wrapping
      }
      catch (Exception ex)
      {
        Log.Info("CrystalFontz634.Initialize: " + ex.Message);
      }
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      commPort.Write(new byte[] {12}, 0, 1);
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      CleanUp();
    }

    public void CleanUp()
    {
      try
      {
        if ((commPort != null) && (commPort.IsOpen))
        {
          commPort.Close();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    #endregion
  }
}