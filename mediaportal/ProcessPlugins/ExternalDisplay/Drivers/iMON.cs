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
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// SoundGraph iMON USB Driver
  /// </summary>
  /// <author>JoeDalton</author>
  public class iMON : BaseDisplay, IDisplay
  {
    private const int VfdType = 4;
    private string[] textLines = new string[2];
    private bool isDisabled = false;
    private string errorMessage = "";


    public iMON()
    {
      try
      {
        if (!Open(VfdType, 0))
        {
          isDisabled = true;
          errorMessage = "Could not find an iMON display";
        }
      }
      catch (Exception ex)
      {
        isDisabled = true;
        errorMessage = ex.Message;
      }
    }

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
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="line">The line to thow the message on.</param>
    /// <param name="message">The message to show.</param>
    public void SetLine(int line, string message)
    {
      textLines[line] = message;
      if (line == 1)
      {
        DisplayLines();
      }
    }

    /// <summary>
    /// Gets the short name of the display
    /// </summary>
    public string Name
    {
      get { return "iMON"; }
    }

    /// <summary>
    /// Gets the description of the display
    /// </summary>
    public string Description
    {
      get { return "SoundGraph iMON VFD USB Driver V1.0"; }
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
      //No advanced configuration needed
    }

    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="port">The port the display is connected to</param>
    /// <param name="lines">The number of lines in text mode</param>
    /// <param name="cols">The number of columns in text mode</param>
    /// <param name="delay">Communication delay in text mode</param>
    /// <param name="linesG">The height in pixels in graphic mode</param>
    /// <param name="colsG">The width in pixels in graphic mode</param>
    /// <param name="timeG">Communication delay in graphic mode</param>
    /// <param name="backLight">Backlight on?</param>
    /// <param name="contrast">Contrast</param>
    public void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight,
                      int contrast)
    {
      OpenLcd();
    }

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
      for (int i = 0; i < 2; i++)
      {
        textLines[i] = new string(' ', Settings.Instance.TextWidth);
      }
      DisplayLines();
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
      CloseLcd();
    }

    /// <summary>
    /// Opens the display driver
    /// </summary>
    private void OpenLcd()
    {
      if (!Open(VfdType, 0))
      {
        Log.Error("ExternalDisplay.iMON.Start: Could not open display");
      }
    }

    /// <summary>
    /// Closes the display driver
    /// </summary>
    private void CloseLcd()
    {
      if (IsOpen())
      {
        Close();
      }
    }

    /// <summary>
    /// Sends the text to the display
    /// </summary>
    private void DisplayLines()
    {
      SetText(textLines[0], textLines[1]);
    }

    #region Interop declarations

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_Init")]
    private static extern bool Open(int vfdType, int resevered);

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_Uninit")]
    private static extern void Close();

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_IsInited")]
    private static extern bool IsOpen();

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_SetText")]
    private static extern bool SetText(string firstLine, string secondLine);

    #endregion
  }
}