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
using System.Runtime.InteropServices;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// An observer interface.
  /// </summary>
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IObserver
  {
    /// <summary>
    /// This function is invoked when an initial or updated program association table is received.
    /// </summary>
    /// <param name="transportStreamId">The transport stream's identifier.</param>
    /// <param name="networkPid">The PID which delivers the transport stream's network information.</param>
    /// <param name="programCount">The number of programs in the transport stream.</param>
    [PreserveSig]
    void OnProgramAssociationTable(ushort transportStreamId, ushort networkPid, ushort programCount);

    /// <summary>
    /// This function is invoked when an initial or updated conditional access table is received.
    /// </summary>
    /// <param name="cat">The new conditional access table. The callee must not change this array.</param>
    /// <param name="catBufferSize">The size of the <paramref name="cat">conditional access table</paramref>.</param>
    [PreserveSig]
    void OnConditionalAccessTable([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] cat,
                                  ushort catBufferSize);

    /// <summary>
    /// This function is invoked when an initial or updated program details are received.
    /// </summary>
    /// <remarks>
    /// The program map table <paramref name="pmt">buffer</paramref> and
    /// <paramref name="pmtBufferSize">buffer size</paramref> parameters will not be populated
    /// until the PMT is available. If the program is not running, PMT will not be available.
    /// </remarks>
    /// <param name="programNumber">The program's identifier.</param>
    /// <param name="pmtPid">The PID which delivers the program's map table sections.</param>
    /// <param name="isRunning">An indication of whether the program is running.</param>
    /// <param name="pmt">The program's map table. The callee must not change this array.</param>
    /// <param name="pmtBufferSize">The size of the <paramref name="pmt">program map table</paramref>.</param>
    [PreserveSig]
    void OnProgramDetail(ushort programNumber,
                          ushort pmtPid,
                          [MarshalAs(UnmanagedType.I1)] bool isRunning,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pmt,
                          ushort pmtBufferSize);

    /// <summary>
    /// This function is invoked when an elementary stream is detected as encrypted.
    /// </summary>
    /// <remarks>
    /// The function should be invoked for each encrypted elementary stream shortly after streaming
    /// starts, and for each elementary stream that becomes encrypted during the course of
    /// streaming. It will not be invoked for non-encrypted streams/changes.
    /// </remarks>
    /// <param name="pid">The elementary stream's PID.</param>
    /// <param name="state">The elementary stream's encryption state.</param>
    [PreserveSig]
    void OnPidEncryptionStateChange(ushort pid, EncryptionState state);

    /// <summary>
    /// This function is invoked when access to one or more PIDs is required.
    /// </summary>
    /// <param name="pids">The PIDs that are required.</param>
    /// <param name="pidCount">The number of PIDs in <paramref name="pids">the PID array</paramref>.</param>
    /// <param name="usage">The reason that access is required.</param>
    [PreserveSig]
    void OnPidsRequired([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ushort[] pids,
                        byte pidCount,
                        PidUsage usage);

    /// <summary>
    /// This function is invoked when access to one or more PIDs is no longer required.
    /// </summary>
    /// <param name="pids">The PIDs that are not required.</param>
    /// <param name="pidCount">The number of PIDs in <paramref name="pids">the PID array</paramref>.</param>
    /// <param name="usage">The reason that access was previously required.</param>
    [PreserveSig]
    void OnPidsNotRequired([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ushort[] pids,
                            byte pidCount,
                            PidUsage usage);
  }
}