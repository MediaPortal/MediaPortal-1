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
using GIRDERLib;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// Girder Module driver
  /// </summary>
  /// <author>JoeDalton</author>
  public class Girder : BaseDisplay, IDisplay
  {
    private int maxLines = 2;
    private int maxColumns = 16;
    private string[] textLines;
    private GirderEvent girder;
    private bool isDisabled = true;
    private string errorMessage;

    public Girder()
    {
      try
      {
        //this will throw a COMException if Girder is not installed or the girder.dll is not registered.
        //If it does, the girderInstalled variable will not be set to true
        girder = new GirderEvent();
        isDisabled = false;
      }
      catch (COMException)
      {
        errorMessage = "Girder is not installed";
      }
      catch (Exception ex)
      {
        errorMessage = ex.Message;
      }
    }

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
    /// Puts the passed message in the buffer for the given line
    /// </summary>
    /// <param name="line">The line in the buffer to put the message in</param>
    /// <param name="message">The message to put in the buffer</param>
    public void SetLine(int line, string message)
    {
      textLines[line++] = message;
      if (line == maxLines)
      {
        SendToGirder();
      }
    }

    /// <summary>
    /// Returns the name of this driver
    /// </summary>
    public string Name
    {
      get { return "Girder"; }
    }

    /// <summary>
    /// Returns the description of this driver
    /// </summary>
    public string Description
    {
      get
      {
        string description = "Girder Module V1.0";
        if (isDisabled)
        {
          return description + " (disabled)";
        }
        return description;
      }
    }

    /// <summary>
    /// Returns whether this driver supports text mode
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Returns whether this driver supports graphics mode
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public void Configure()
    {
    }

    /// <summary>
    /// Initializes the driver
    /// </summary>
    /// <param name="port">ignored</param>
    /// <param name="lines">the number of lines to keep in the buffer</param>
    /// <param name="cols">the number of columns for each line</param>
    /// <param name="delay">ignored</param>
    /// <param name="linesG">ignored</param>
    /// <param name="colsG">ignored</param>
    /// <param name="timeG">ignored</param>
    /// <param name="backLight">ignored</param>
    /// <param name="contrast">ignored</param>
    public void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight,
                      int contrast)
    {
      maxLines = lines;
      maxColumns = cols;
      textLines = new string[lines];
    }

    /// <summary>
    /// Clears the buffer, and sends it to Girder
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
      for (int i = 0; i < maxLines; i++)
      {
        textLines[i] = new string(' ', maxColumns);
      }
      SendToGirder();
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Cleanup 
    /// </summary>
    public void Dispose()
    {
      girder = null;
    }

    #endregion

    /// <summary>
    /// Sends the buffer to Girder by raising a MPDisplayUpdated event
    /// </summary>
    private void SendToGirder()
    {
      if (isDisabled)
      {
        return;
      }
      Array lines = textLines;
      girder.SendEvent("MPDisplayUpdated", 19, ref lines);
    }
  }
}