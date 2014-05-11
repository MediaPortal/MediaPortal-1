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

using System.Drawing;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
    public abstract class BaseDisplay : IDisplay
    {
        protected BaseDisplay() {}
        
        //From IDisplay
        //  Methods
        public abstract void CleanUp();
        public abstract void Configure();
        public abstract void DrawImage(Bitmap bitmap);
        public abstract void Initialize();
        public abstract void SetCustomCharacters(int[][] customCharacters);
        public abstract void SetLine(int line, string message);
        public abstract void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight, int backLightLevel, bool contrast, int contrastLevel, bool BlankOnExit);
        public virtual void Update() { }
        //  Properties
        public abstract string Description { get; }
        public abstract string ErrorMessage { get; }
        public abstract bool IsDisabled { get; }
        public abstract string Name { get; }
        public abstract bool SupportsGraphics { get; }
        public abstract bool SupportsText { get; }

        //From IDisposable
        public abstract void Dispose();
    }
}