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

using System.Runtime.InteropServices;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// A channel observer interface.
  /// </summary>
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IChannelObserver
  {
    /// <summary>
    /// This function is invoked when the first unencrypted PES packet is received from a PID.
    /// </summary>
    /// <param name="pid">The PID that was seen.</param>
    /// <param name="pidType">The type of <paramref name="pid">PID</paramref>.</param>
    [PreserveSig]
    void OnSeen(ushort pid, PidType pidType);
  }
}