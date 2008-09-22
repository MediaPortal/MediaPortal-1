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
    string Name { get; }

    /// <summary>
    /// Gets the description of the display
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    bool SupportsText { get; }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    bool SupportsGraphics { get; }

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
    void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight,
               int contrast);

    /// <summary>
    /// Initializes display
    /// </summary>
    void Initialize();

    /// <summary>
    /// Cleans up all your mess.  
    /// </summary>
    /// <remarks>
    /// The plugin will call this method when MP shuts down.  So here is the place to clear the display and 
    /// close any open ports you use.
    /// </remarks>
    void CleanUp();

    /// <summary>
    /// Returns whether the display is disabled.
    /// </summary>
    /// <value>A <b>bool</b> indicating whether the display is disabled or not.</value>
    /// <seealso cref="ErrorMessage"/>
    /// <remarks>
    /// The plugin configuration will create an instance of each display it knows of, so it is important 
    /// to try and catch as much errors as possible that could occur during the display initialization (driver not
    /// installed, port not found, etc...).  If any problems occur set the IsDisabled property to true.  This will
    /// prohibit the user from choosing this display.  The value of the <see cref="ErrorMessage"/> property 
    /// will be displayed to the user.
    /// </remarks>
    bool IsDisabled { get; }

    /// <summary>
    /// Returns the reason why the display was disabled.
    /// </summary>
    /// <value>
    /// The errormessage to display to the user.
    /// </value>
    /// <seealso cref="IsDisabled"/>
    string ErrorMessage { get; }

    /// <summary>
    /// Declares a number of custom characters.
    /// </summary>
    /// <param name="customCharacters">A 2 dimensional array of integers.  The first dimension represents
    /// the characters, the second array the lines for each character</param>
    void SetCustomCharacters(int[][] customCharacters);

    /// <summary>
    /// Draws an image on the display
    /// </summary>
    /// <param name="bitmap">A <see cref="Bitmap"/> representing the image to draw</param>
    void DrawImage(Bitmap bitmap);
  }
}