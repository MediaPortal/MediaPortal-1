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

using System.Runtime.InteropServices;

namespace Yeti.Sys

{
  public enum BeepType

  {
    SimpleBeep = -1,

    SystemAsterisk = 0x00000040,

    SystemExclamation = 0x00000030,

    SystemHand = 0x00000010,

    SystemQuestion = 0x00000020,

    SystemDefault = 0
  }

  /// <summary>
  /// Win32 API functions
  /// </summary>
  public sealed class Win32

  {
    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool MessageBeep(BeepType Type);
  }
}