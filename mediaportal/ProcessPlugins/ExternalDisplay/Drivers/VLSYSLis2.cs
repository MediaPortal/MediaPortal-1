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
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// VL System L.I.S. 2 Driver
  /// </summary>
  /// <author>Nopap & JoeDalton</author>
  public class VLSYSLis2 : BaseDisplay, IDisplay
  {
    private SerialPort commPort = null;
    private int lines = 2;
    private int cols = 40;
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
      commPort.Write(new byte[] {0}, 0, 1);
      if (line == 0)
      {
        commPort.Write(new byte[] {0xA1}, 0, 1);
      }
      else if (line == 1)
      {
        commPort.Write(new byte[] {0xA2}, 0, 1);
      }
      else
      {
        Log.Error("VLSYSLis2.SetLine: error bad line number" + line);
        return;
      }
      commPort.Write(new byte[] {0, 0xA7}, 0, 2);
      commPort.Write(message);
      commPort.Write(new byte[] {0}, 0, 1);
    }

    /// <summary>
    /// Short name of this display driver
    /// </summary>
    public string Name
    {
      get { return "VLSYSLis2"; }
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "VL System L.I.S 2 driver V1.0, by Nopap"; }
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
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _contrast)
    {
      try
      {
        commPort = new SerialPort(_port, 19200, Parity.None, 8, StopBits.One);
        commPort.Open();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

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
      string s = new string(' ', cols);
      for (int i = 0; i < lines; i++)
      {
        SetLine(i, s);
      }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
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