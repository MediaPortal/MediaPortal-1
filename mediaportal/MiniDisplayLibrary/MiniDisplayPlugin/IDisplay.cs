#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public interface IDisplay : IDisposable
  {
    void CleanUp();    
    void DrawImage(Bitmap bitmap);
    void Initialize();
    void SetCustomCharacters(int[][] customCharacters);

    void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight,
           int backLightLevel, bool contrast, int contrastLevel, bool BlankOnExit);

    string ErrorMessage { get; }

    bool IsDisabled { get; }


    /// <summary>
    /// Called by our framework when the user hits the advanced settings button from our MiniDisplay configuration.
    /// Implementation should bring up an advanced settings dialog.
    /// </summary>
    void Configure();

    /// <summary>
    /// Set a line of characters on your display.
    /// Our MiniDisplay framework calls this function for each text field on every frame even if the message has not changed.
    /// </summary>
    /// <param name="line">Index of the line to set. Typically 0 for top line and 1 for bottom line.</param>
    /// <param name="message">String of character. Can include some whitespace padding to implement alignment depending of our auto-scroll setting.</param>
    /// <param name="aAlignment">Define text alignment on the specified line. Needed for displays supporting auto-scroll.</param>
    void SetLine(int line, string message, ContentAlignment aAlignment);

    /// <summary>
    /// Provide the name of this display.
    /// Display name is sent as text field to our display when testing it through our MiniDisplay configuration.
    /// It should be similar the the full description but shorter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Provide a description of this display.
    /// Description is shown in our MiniDisplay configuration driver selection drop-down list. 
    /// It is intended to help the user select the proper driver for her display hardware.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Implementation should return true if graphics are supported.
    /// Bitmaps are then provided through our DrawImage function.
    /// </summary>
    bool SupportsGraphics { get; }

    /// <summary>
    /// Implementation should return true if character mode is supported.
    /// Character data is then passed to our display implementation using our SetLine function.
    /// </summary>
    bool SupportsText { get; }

    /// <summary>
    /// Called by our MiniDisplay framework whenever a frame is ready to send to our hardware.
    /// Optimized display implementation should use it to send a new frame to the display hardware.
    /// Prior to this call text fields and bitmap are set using SetLine and DrawImage.
    /// </summary>
    void Update();
  }
}