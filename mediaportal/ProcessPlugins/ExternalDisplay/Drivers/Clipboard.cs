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
using System.Text;
using System.Threading;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// This class copies the generated text lines to the Windows Clipboard
  /// </summary>
  /// <remarks>
  /// The copying is done in a separate thread because communication with a COM component needs a 
  /// Single Threaded Appartment (STA) thread</remarks>
  /// <author>JoeDalton</author>
  public class Clipboard : BaseDisplay, IDisplay
  {
    private int maxLines;
    private string[] lines;
    private Thread th; //pointer to the thread that does the copying
    private bool isDisabled = false;
    private string errorMessage = "";

    /// <summary>
    /// Constructor
    /// </summary>
    public Clipboard()
    {
      try
      {
        lines = new string[Settings.Instance.TextHeight];
        Initialize();
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
    /// Stops the display.
    /// </summary>
    public void CleanUp()
    {
      if (th != null)
      {
        if (th.IsAlive)
        {
          th.Abort();
        }
      }
      System.Windows.Forms.Clipboard.SetDataObject("");
    }


    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="_line">The line to thow the message on.</param>
    /// <param name="_message">The message to show.</param>
    public void SetLine(int _line, string _message)
    {
      lines[_line] = _message;
      if (_line == maxLines)
      {
        SendToClipboard();
      }
    }

    /// <summary>
    /// Copies the generated text to the Windows clipboard
    /// </summary>
    private void SendToClipboard()
    {
      if (th != null)
      {
        if (th.IsAlive)
        {
          th.Abort();
        }
      }
      th = new Thread(new ThreadStart(CopySTA));
      th.SetApartmentState(ApartmentState.STA);
      th.IsBackground = true;
      th.Name = "VFD-Clipboard";
      th.Start();
      th.Join();
    }


    /// <summary>
    /// Because the .NET Clipboard class uses COM, the thread that uses it needs to be STA.
    /// That is why we start this method in a separate thread and wait until it is done.
    /// </summary>
    [STAThread]
    private void CopySTA()
    {
      try
      {
        StringBuilder b = new StringBuilder();
        for (int i = 0; i < Settings.Instance.TextHeight; i++)
        {
          b.Append(lines[i]);
          b.Append(Environment.NewLine);
        }
        System.Windows.Forms.Clipboard.SetDataObject(b.ToString(), true);
      }
      catch (Exception)
      {
      }
    }


    /// <summary>
    /// Cleanup when disposed
    /// </summary>
    public void Dispose()
    {
      CleanUp();
    }

    /// <summary>
    /// Advanced configuration for this display driver
    /// </summary>
    public void Configure()
    {
    }

    /// <summary>
    /// Initializes the display driver
    /// </summary>
    /// <param name="_port">ignored</param>
    /// <param name="_lines">ignored</param>
    /// <param name="_cols">ignored</param>
    /// <param name="_time">ignored</param>
    /// <param name="_linesG">ignored</param>
    /// <param name="_colsG">ignored</param>
    /// <param name="_timeG">ignored</param>
    /// <param name="_backLight">ignored</param>
    /// <param name="_contrast">ignored</param>
    public void Setup(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG,
                      bool _backLight, int _contrast)
    {
      maxLines = _lines - 1;
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      Clear();
    }

    private void Clear()
    {
      for (int i = 0; i < Settings.Instance.TextHeight; i++)
      {
        lines[i] = new string(' ', Settings.Instance.TextWidth);
      }
    }

    /// <summary>
    /// The display driver's (short) name
    /// </summary>
    public string Name
    {
      get { return "Clipboard"; }
    }

    /// <summary>
    /// The display driver's description
    /// </summary>
    public string Description
    {
      get { return "Clipboard driver V1.0"; }
    }

    /// <summary>
    /// Does this driver supports textmode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this driver supports graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }
  }
}