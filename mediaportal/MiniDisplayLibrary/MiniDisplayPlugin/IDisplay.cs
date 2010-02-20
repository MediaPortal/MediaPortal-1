#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
    void Configure();
    void DrawImage(Bitmap bitmap);
    void Initialize();
    void SetCustomCharacters(int[][] customCharacters);
    void SetLine(int line, string message);

    void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight,
               int backLightLevel, bool contrast, int contrastLevel, bool BlankOnExit);

    string Description { get; }

    string ErrorMessage { get; }

    bool IsDisabled { get; }

    string Name { get; }

    bool SupportsGraphics { get; }

    bool SupportsText { get; }
  }
}