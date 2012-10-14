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

    #region encryption analyser

    /// <summary>
    /// Add an elementary stream to the set of streams that the analyser for a subchannel should monitor.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="pid">The PID associated with the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the elementary stream was successfully registered</returns>
    [PreserveSig]
    int AnalyserAddPid(int handle, int pid);

    /// <summary>
    /// Remove an elementary stream from the set of streams that the analyser for a subchannel is monitoring.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="pid">The PID associated with the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the elementary stream was successfully deregistered</returns>
    [PreserveSig]
    int AnalyserRemovePid(int handle, int pid);

    /// <summary>
    /// Get a count of the elementary streams that the analyser for a subchannel is currently monitoring.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="pidCount">The number of elementary streams that the analyser is currently monitoring.</param>
    /// <returns>an HRESULT indicating whether the elementary stream count was successfully retrieved</returns>
    [PreserveSig]
    int AnalyserGetPidCount(int handle, out int pidCount);

    /// <summary>
    /// Get the encryption state for a specific elementary stream that the analyser for a subchannel is monitoring.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="pidIndex">The PID index. The value of this parameter should be in the range 0..[GetPidCount() - 1] (inclusive).</param>
    /// <param name="pid">The PID associated with the stream.</param>
    /// <param name="encryptionState">The current encryption state of the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the elementary stream state was successfully retrieved</returns>
    [PreserveSig]
    int AnalyserGetPid(int handle, int pidIndex, out int pid, out EncryptionState encryptionState);

    /// <summary>
    /// Set the delegate for the analyser for a subchannel to notify when the encryption state of one of the monitored elementary streams changes.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int AnalyserSetCallBack(int handle, IEncryptionStateChangeCallBack callBack);

    /// <summary>
    /// Reset the encryption analyser for a subchannel.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <returns>an HRESULT indicating whether the analyser was successfully reset</returns>
    [PreserveSig]
    int AnalyserReset(int handle);

    #endregion

    #region PMT grabber

    /// <summary>
    /// Set the PID and service for the PMT grabber for a subchannel to monitor.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="pmtPid">The PID that the grabber should monitor.</param>
    /// <param name="serviceId">The ID of the service that the grabber should monitor.</param>
    /// <returns>an HRESULT indicating whether the grabber parameters were successfully registered</returns>
    [PreserveSig]
    int PmtSetPmtPid(int handle, int pmtPid, int serviceId);

    /// <summary>
    /// Set the delegate for the PMT grabber for a subchannel to notify when a new PMT section is received.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int PmtSetCallBack(int handle, IPmtCallBack callBack);

    /// <summary>
    /// Used by the delegate to retrieve a PMT section from the PMT grabber for a subchannel.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="pmtData">A pointer to a buffer that will be populated with the most recently received PMT section.</param>
    /// <returns>the length of the PMT section in bytes</returns>
    [PreserveSig]
    int PmtGetPmtData(int handle, IntPtr pmtData);

    #endregion

    /// <summary>
    /// Sets the recorder unicode filename for the given subchannel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="fileName">Filename for the recording</param>
    /// <returns></returns>
    [PreserveSig]
    int RecordSetRecordingFileNameW(int handle, [In, MarshalAs(UnmanagedType.LPWStr)] string fileName);

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
    int TimeShiftSetTimeShiftingFileNameW(int handle, [In, MarshalAs(UnmanagedType.LPWStr)] string fileName);

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

    #region CAT grabber

    /// <summary>
    /// Reset the CAT grabber for a subchannel, causing it to forget about previously seen CAT sections.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <returns>an HRESULT indicating whether the grabber was successfully reset</returns>
    [PreserveSig]
    int CaReset(int handle);

    /// <summary>
    /// Set the delegate for the CAT grabber for a subchannel to notify when a new CAT section is received.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int CaSetCallBack(int handle, ICaCallBack callBack);

    /// <summary>
    /// Used by the delegate to retrieve a CAT section from the CAT grabber for a subchannel.
    /// </summary>
    /// <param name="handle">The subchannel handle.</param>
    /// <param name="caData">A pointer to a buffer that will be populated with the most recently received CAT section.</param>
    /// <returns>the length of the CAT section in bytes</returns>
    [PreserveSig]
    int CaGetCaData(int handle, IntPtr caData);

    #endregion

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="totalTsBytes">Amount of packets processed - timeshifting</param>
    /// <param name="totalRecordingBytes">Amount of packets processed - recording</param>
    /// <param name="TsDiscontinuity">Number of stream discontinuities - timeshifting</param>
    /// <param name="recordingDiscontinuity">Number of stream discontinuities - recording</param>
    /// <returns></returns>
    [PreserveSig]
    int GetStreamQualityCounters(int handle, out int totalTsBytes, out int totalRecordingBytes,
                                 out int TsDiscontinuity, out int recordingDiscontinuity);

    /// <summary>
    /// Sets the channel type of the sub channel
    /// </summary>
    /// <param name="handle">Handle of the sub channel</param>
    /// <param name="channelType">Type of the channel (tv=0, radio=1)</param>
    /// <returns></returns>
    [PreserveSig]
    int TimeShiftSetChannelType(int handle, int channelType);
  }
}