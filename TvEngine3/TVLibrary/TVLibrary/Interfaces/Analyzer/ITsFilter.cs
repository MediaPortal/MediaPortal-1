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
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// The main TsWriter interface
  /// </summary>
  [ComVisible(true), ComImport,
   Guid("5EB9F392-E7FD-4071-8E44-3590E5E767BA"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsFilter
  {
    /// <summary>
    /// Adds a new sub channel
    /// </summary>
    /// <param name="handle">Handle of the channel</param>
    /// <returns></returns>
    [PreserveSig]
    int AddChannel(ref int handle);

    /// <summary>
    /// Deletes the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel to delete</param>
    /// <returns></returns>
    [PreserveSig]
    int DeleteChannel(int handle);

    /// <summary>
    /// Deletes all sub channels
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int DeleteAllChannels();

    /// <summary>
    /// Sets the video pid on the analyzer for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="videoPid">Video PID</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerSetVideoPid(int handle, int videoPid);

    /// <summary>
    /// Gets the video pid of the analyzer for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="videoPid">Video pid</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerGetVideoPid(int handle, out int videoPid);

    /// <summary>
    /// Sets the audio pid on the analyzer for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="audioPid">Audio pid</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerSetAudioPid(int handle, int audioPid);

    /// <summary>
    /// Gets the audio pid on the analyzer for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="audioPid">Audio pid</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerGetAudioPid(int handle, out int audioPid);

    /// <summary>
    /// Checks if video of the channel is encrypted for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="yesNo">true, if the channel is encrypted</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerIsVideoEncrypted(int handle, out int yesNo);

    /// <summary>
    /// Checks if audioof the channel is encrypted for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="yesNo">true, if the channel is encrypted</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerIsAudioEncrypted(int handle, out int yesNo);

    /// <summary>
    /// Resets the analyzer of the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int AnalyzerReset(int handle);

    /// <summary>
    /// Sets the pmt for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="pmtPid">The PMT pid</param>
    /// <param name="serviceId">The service id</param>
    /// <returns></returns>
    [PreserveSig]
    int PmtSetPmtPid(int handle, int pmtPid, long serviceId);

    /// <summary>
    /// Sets the pmt callback for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="callback">The pmt callback</param>
    /// <returns></returns>
    [PreserveSig]
    int PmtSetCallBack(int handle, IPMTCallback callback);

    /// <summary>
    /// Gets the pmt data of the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="pmtData">Pointer for the PMT data</param>
    /// <returns></returns>
    [PreserveSig]
    int PmtGetPMTData(int handle, IntPtr pmtData);

    /// <summary>
    /// Sets the recorder filename for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="fileName">Filename for the reocrding</param>
    /// <returns></returns>
    [PreserveSig]
    int RecordSetRecordingFileName(int handle, [In, MarshalAs(UnmanagedType.LPStr)] string fileName);

    /// <summary>
    /// Starts recording on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int RecordStartRecord(int handle);

    /// <summary>
    /// Stops recording on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int RecordStopRecord(int handle);

    /// <summary>
    /// Sets the pmt pid for recording on the sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="pmtPid">The PMT pid</param>
    /// <param name="serviceId">The service id</param>
    /// <param name="pmtData">The PMT data</param>
    /// <param name="pmtLength">The length of the PMT</param>
    /// <returns></returns>
    [PreserveSig]
    int RecordSetPmtPid(int handle, int pmtPid, int serviceId, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pmtData,
                        int pmtLength);

    /// <summary>
    /// Sets the video/audio observer callback for recorder
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="observer">Oberserver callback</param>
    /// <returns></returns>
    [PreserveSig]
    int RecorderSetVideoAudioObserver(int handle, IVideoAudioObserver observer);

    /// <summary>
    /// Sets the timeshifting filename
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="fileName">Filename</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftSetTimeShiftingFileName(int handle, [In, MarshalAs(UnmanagedType.LPStr)] string fileName);

    /// <summary>
    /// Starts timeshifting on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftStart(int handle);

    /// <summary>
    /// Stops timeshifting on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftStop(int handle);

    /// <summary>
    /// Resets timeshifting on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftReset(int handle);

    /// <summary>
    /// Gets the timeshifting buffer size of the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="size">Size of the buffer</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftGetBufferSize(int handle, out long size);

    /// <summary>
    /// Sets the PMT pid on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="pmtPid">The PMT pid</param>
    /// <param name="serviceId">The service id</param>
    /// <param name="pmtData">The PMT data</param>
    /// <param name="pmtLength">The length of the PMT</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftSetPmtPid(int handle, int pmtPid, int serviceId, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pmtData,
                           int pmtLength);

    /// <summary>
    /// Pauses/Continues timeshifting on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="onOff">Flag for pausing the timeshifting</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftPause(int handle, byte onOff);

    /// <summary>
    /// Sets the timeshifting parameters on the given subchannel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="minFiles">Minimum timeshifting files</param>
    /// <param name="maxFiles">Maximum timeshifting files</param>
    /// <param name="chunkSize">Chunk size</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftSetParams(int handle, int minFiles, int maxFiles, UInt32 chunkSize);

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    [PreserveSig]
    int TimeShiftGetCurrentFilePosition(int handle, [Out] out Int64 position, [Out] out long bufferId);

    /// <summary>
    /// Sets the video/audio observer callback
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="observer">Oberserver callback</param>
    /// <returns></returns>
    [PreserveSig]
    int SetVideoAudioObserver(int handle, IVideoAudioObserver observer);

    /// <summary>
    /// Start collecting teletext data for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int TTxStart(int handle);

    /// <summary>
    /// Stops collecting teletext data for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int TTxStop(int handle);

    /// <summary>
    /// Sets the teletext pid on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="teletextPid">Teletext pid</param>
    /// <returns></returns>
    [PreserveSig]
    int TTxSetTeletextPid(int handle, int teletextPid);

    /// <summary>
    /// Sets the teletext callback on the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="callback">The callback</param>
    /// <returns></returns>
    [PreserveSig]
    int TTxSetCallBack(int handle, ITeletextCallBack callback);

    /// <summary>
    /// Set the CA callback
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="callback">The callback</param>
    /// <returns></returns>
    [PreserveSig]
    int CaSetCallBack(int handle, ICACallback callback);

    /// <summary>
    /// Gets the ca data of the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="caData">Pointer for the ca data</param>
    /// <returns></returns>
    [PreserveSig]
    int CaGetCaData(int handle, IntPtr caData);

    /// <summary>
    /// Resets the ca for the given sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <returns></returns>
    [PreserveSig]
    int CaReset(int handle);
  }
}