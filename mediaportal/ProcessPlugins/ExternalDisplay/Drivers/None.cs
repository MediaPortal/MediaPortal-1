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

using System.Drawing;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// This <see cref="IDisplay"/> implementation can be used to send the display lines this 
  /// control produces to LCDSmartie, by exposing the lines as properties in the 
  /// GUIPropertyManager, where they can be picked up by the LCDSmartie plugin 
  /// by AllenConquest 
  /// </summary>
  /// <author>JoeDalton</author>
  public class None : BaseDisplay, IDisplay
  {
    private string[] lines;
    private int row = 0;
    private int col = 0;

    /// <summary>
    /// Instructs our plugin to display the indicated message on the indicated line.
    /// Or (in our case) set the value of the property #externaldisplay.lineX where X
    /// is the indicated line.
    /// </summary>
    /// <param name="_line">The line to show the message on</param>
    /// <param name="_message">The message to show</param>
    public void SetLine(int _line, string _message)
    {
      GUIPropertyManager.SetProperty("#externaldisplay.line" + _line.ToString(), _message);
    }

    public void CleanUp()
    {
      Clear();
    }

    /// <summary>
    /// Cleans-up all used resources
    /// </summary>
    public void Dispose()
    {
      CleanUp();
    }

    public void Configure()
    {
    }

    public void Setup(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG,
                      bool _backLight, int _contrast)
    {
      lines = new string[_lines];
      Initialize();
    }

    public void SetPosition(int x, int y)
    {
      row = y;
      col = x;
    }

    public void SendText(string _text)
    {
      int j = 0;
      char[] text = lines[row].ToCharArray();
      for (int i = col; i < text.Length; i++)
      {
        text[i] = _text[j++];
      }
      lines[row] = new string(text);
    }


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

    public string Name
    {
      get { return "PropertySetter"; }
    }

    public string Description
    {
      get { return "MediaPortal Property Setter V1.0"; }
    }

    public bool SupportsText
    {
      get { return true; }
    }

    public bool SupportsGraphics
    {
      get { return false; }
    }

    public bool IsDisabled
    {
      get { return false; }
    }

    public string ErrorMessage
    {
      get { return ""; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    public void DrawImage(Bitmap bitmap)
    {
    }
  }
}