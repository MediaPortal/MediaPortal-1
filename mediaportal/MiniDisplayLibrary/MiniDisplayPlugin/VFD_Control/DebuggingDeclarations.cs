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

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.VFD_Control
{
  internal sealed class DebuggingDeclarations
  {
    // API declarations relating to Windows error messages.

    // ******************************************************************************
    // API constants
    // ******************************************************************************

    public const short FORMAT_MESSAGE_FROM_SYSTEM = 0x1000; // S;

    // ******************************************************************************
    // API functions, listed alphabetically
    // ******************************************************************************

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern int FormatMessage(int dwFlags, ref long lpSource, int dwMessageId, int dwLanguageZId,
                                           string lpBuffer, int nSize, int Arguments);
  }
}