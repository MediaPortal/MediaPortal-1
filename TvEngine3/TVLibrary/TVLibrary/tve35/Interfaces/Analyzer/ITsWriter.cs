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
  /// The MediaPortal transport stream writer/analyser filter class.
  /// </summary>
  [Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
  internal class MediaPortalTsWriter
  {
  }

  /// <summary>
  /// The main interface on the MediaPortal transport stream writer/analyser.
  /// </summary>
  [Guid("5eb9f392-e7fd-4071-8e44-3590e5e767ba"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsWriter
  {
    /// <summary>
    /// Configure the logging subsystem for the writer.
    /// </summary>
    /// <remarks>
    /// This function must be called immediately after the writer is instanciated.
    /// </remarks>
    /// <param name="path">The path to the log and debug files.</param>
    /// <returns>an HRESULT indicating whether the function succeeded</returns>
    [PreserveSig]
    int ConfigureLogging([MarshalAs(UnmanagedType.LPWStr)] string path);

    /// <summary>
    /// Enable or disable input stream dumping for one or more input pins.
    /// </summary>
    /// <remarks>
    /// The writer will copy the raw input stream received from enabled input pins to file. One
    /// file per enabled pin. The files will be overwritten/recreated each time streaming is
    /// started.
    /// </remarks>
    /// <param name="enableTs"><c>True</c> to enable dumping for the transport stream input pin.</param>
    /// <param name="enableOobSi"><c>True</c> to enable dumping for the out-of-band service information input pin.</param>
    [PreserveSig]
    void DumpInput([MarshalAs(UnmanagedType.I1)] bool enableTs,
                    [MarshalAs(UnmanagedType.I1)] bool enableOobSi);

    /// <summary>
    /// Enable or disable cyclic redundancy code checking for non-essential service information
    /// sections.
    /// </summary>
    /// <remarks>
    /// Cyclic redundancy codes are always checked for program association, program map and
    /// conditional access table sections.
    /// </remarks>
    /// <param name="enable"><c>True</c> to enable CRC checking.</param>
    [PreserveSig]
    void CheckSectionCrcs([MarshalAs(UnmanagedType.I1)] bool enable);

    /// <summary>
    /// Set the writer's observer call back delegate.
    /// </summary>
    /// <param name="observer">The delegate.</param>
    [PreserveSig]
    void SetObserver(IObserver observer);

    /// <summary>
    /// Start the writer.
    /// </summary>
    [PreserveSig]
    void Start();

    /// <summary>
    /// Stop the writer.
    /// </summary>
    [PreserveSig]
    void Stop();

    /// <summary>
    /// Add a new channel.
    /// </summary>
    /// <param name="observer">The channel's observer.</param>
    /// <param name="handle">The channel's handle (identifier).</param>
    /// <returns>an HRESULT indicating whether the channel was added successfully</returns>
    [PreserveSig]
    int AddChannel(IChannelObserver observer, out int handle);

    /// <summary>
    /// Get the state of a PID.
    /// </summary>
    /// <param name="pid">The PID.</param>
    /// <param name="state">The PID's state.</param>
    [PreserveSig]
    void GetPidState(ushort pid, out EncryptionState state);

    /// <summary>
    /// Delete a channel.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    [PreserveSig]
    void DeleteChannel(int handle);

    /// <summary>
    /// Deletes all channels.
    /// </summary>
    [PreserveSig]
    void DeleteAllChannels();

    /// <summary>
    /// Set the file name for a channel's recorder.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>an HRESULT indicating whether the recorder's file name was set successfully</returns>
    [PreserveSig]
    int RecorderSetFileName(int handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);

    /// <summary>
    /// Set the program map table for a channel's recorder.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="pmt">The program map table.</param>
    /// <param name="pmtSize">The size of the <paramref name="pmt">program map table</paramref>.</param>
    /// <param name="isDynamicPmtChange">An indication of whether the PMT change represents a channel change.</param>
    /// <returns>an HRESULT indicating whether the recorder's program map table was set successfully</returns>
    [PreserveSig]
    int RecorderSetPmt(int handle,
                        [MarshalAs(UnmanagedType.LPArray)] byte[] pmt,
                        ushort pmtSize,
                        [MarshalAs(UnmanagedType.I1)] bool isDynamicPmtChange);

    /// <summary>
    /// Start a channel's recorder.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <returns>an HRESULT indicating whether the recorder was started successfully</returns>
    [PreserveSig]
    int RecorderStart(int handle);

    /// <summary>
    /// Pause or unpause a channel's recorder.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="isPause"><c>True</c> if pausing.</param>
    /// <returns>an HRESULT indicating whether the recorder was paused or unpaused successfully</returns>
    [PreserveSig]
    int RecorderPause(int handle, [MarshalAs(UnmanagedType.I1)] bool isPause);

    /// <summary>
    /// Retrieve the stream quality statistics for a channel's recorder.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="countTsPackets">The number of transport stream packets written by the recorder.</param>
    /// <param name="countDiscontinuities">The number of discontinuities encountered by the recorder.</param>
    /// <param name="countDroppedBytes">The number of bytes dropped by the recorder.</param>
    /// <returns>an HRESULT indicating whether the stream quality statistics were retrieved successfully</returns>
    [PreserveSig]
    int RecorderGetStreamQuality(int handle,
                                  out ulong countTsPackets,
                                  out ulong countDiscontinuities,
                                  out ulong countDroppedBytes);

    /// <summary>
    /// Stop a channel's recorder.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <returns>an HRESULT indicating whether the recorder was stopped successfully</returns>
    [PreserveSig]
    int RecorderStop(int handle);

    /// <summary>
    /// Set the file name for a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>an HRESULT indicating whether the time-shifter's file name was set successfully</returns>
    [PreserveSig]
    int TimeShifterSetFileName(int handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);

    /// <summary>
    /// Set the parameters for a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="fileCountMinimum">The minimum number of files for the time-shifter to use.</param>
    /// <param name="fileCountMaximum">The maximum number of files for the time-shifter to use [when time-shifting is paused].</param>
    /// <param name="fileSizeBytes">The size of each file in bytes.</param>
    /// <returns>an HRESULT indicating whether the time-shifting parameters were set successfully</returns>
    [PreserveSig]
    int TimeShifterSetParameters(int handle,
                                  uint fileCountMinimum,
                                  uint fileCountMaximum,
                                  ulong fileSizeBytes);

    /// <summary>
    /// Set the program map table for a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="pmt">The program map table.</param>
    /// <param name="pmtSize">The size of the <paramref name="pmt">program map table</paramref>.</param>
    /// <param name="isDynamicPmtChange">An indication of whether the PMT change represents a channel change.</param>
    /// <returns>an HRESULT indicating whether the time-shifter's program map table was set successfully</returns>
    [PreserveSig]
    int TimeShifterSetPmt(int handle,
                          [MarshalAs(UnmanagedType.LPArray)] byte[] pmt,
                          ushort pmtSize,
                          [MarshalAs(UnmanagedType.I1)] bool isDynamicPmtChange);

    /// <summary>
    /// Start a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <returns>an HRESULT indicating whether the time-shifter was started successfully</returns>
    [PreserveSig]
    int TimeShifterStart(int handle);

    /// <summary>
    /// Pause or unpause a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="isPause"><c>True</c> if pausing.</param>
    /// <returns>an HRESULT indicating whether the time-shifter was paused or unpaused successfully</returns>
    [PreserveSig]
    int TimeShifterPause(int handle, [MarshalAs(UnmanagedType.I1)] bool isPause);

    /// <summary>
    /// Retrieve the stream quality statistics for a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="countTsPackets">The number of transport stream packets written by the time-shifter.</param>
    /// <param name="countDiscontinuities">The number of discontinuities encountered by the time-shifter.</param>
    /// <param name="countDroppedBytes">The number of bytes dropped by the time-shifter.</param>
    /// <returns>an HRESULT indicating whether the stream quality statistics were retrieved successfully</returns>
    [PreserveSig]
    int TimeShifterGetStreamQuality(int handle,
                                    out ulong countTsPackets,
                                    out ulong countDiscontinuities,
                                    out ulong countDroppedBytes);

    /// <summary>
    /// Retrieve the current file position for a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <param name="position">The file position/offset in bytes.</param>
    /// <param name="bufferId">The identifier of the current buffer file.</param>
    /// <returns>an HRESULT indicating whether the file position was retrieved successfully</returns>
    [PreserveSig]
    int TimeShifterGetCurrentFilePosition(int handle, out ulong position, out uint bufferId);

    /// <summary>
    /// Stop a channel's time-shifter.
    /// </summary>
    /// <param name="handle">The channel's handle.</param>
    /// <returns>an HRESULT indicating whether the time-shifter was stopped successfully</returns>
    [PreserveSig]
    int TimeShifterStop(int handle);
  }
}