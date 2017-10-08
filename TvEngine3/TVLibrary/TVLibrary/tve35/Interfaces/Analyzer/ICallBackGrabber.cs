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
  /// A grabber call back interface.
  /// </summary>
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface ICallBackGrabber
  {
    /// <summary>
    /// This function is invoked when the first section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was received.</param>
    [PreserveSig]
    void OnTableSeen(ushort pid, byte tableId);

    /// <summary>
    /// This function is invoked after the last section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was completed.</param>
    [PreserveSig]
    void OnTableComplete(ushort pid, byte tableId);

    /// <summary>
    /// This function is invoked after any section from a table changes.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that changed.</param>
    [PreserveSig]
    void OnTableChange(ushort pid, byte tableId);

    /// <summary>
    /// This function is invoked after the grabber is reset.
    /// </summary>
    /// <param name="pid">The PID that is associated with the grabber.</param>
    [PreserveSig]
    void OnReset(ushort pid);
  }
}