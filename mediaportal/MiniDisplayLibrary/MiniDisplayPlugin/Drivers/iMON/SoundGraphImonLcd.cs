#region Copyright (C) 2014 Team MediaPortal

// Copyright (C) 2014 Team MediaPortal
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
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// SoundGraph iMON LCD.
    /// </summary>
    public class SoundGraphImonLcd : ISoundGraphImonDisplay
    {
        public override void SetLine(int line, string message){}
        public override string Name() { return "iMON LCD"; }
    }

    

}

