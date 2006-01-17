/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// The common interface for all displays that this plug-in supports
  /// </summary>
  /// <author>JoeDalton</author>
  public interface IDisplay : IDisposable
  {
    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="line">The line to thow the message on.</param>
    /// <param name="message">The message to show.</param>
    void SetLine(int line, string message);
    /// <summary>
    /// Gets the short name of the display
    /// </summary>
    string Name {get;}
    /// <summary>
    /// Gets the description of the display
    /// </summary>
    string Description {get;}
    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    bool SupportsText {get;}
    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    bool SupportsGraphics {get;}
    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    void Configure();
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
    void Initialize(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight, int contrast);
    /// <summary>
    /// Clears the display
    /// </summary>
    void Clear();

    bool IsDisabled { get; }
    string ErrorMessage { get; }

  }
}
