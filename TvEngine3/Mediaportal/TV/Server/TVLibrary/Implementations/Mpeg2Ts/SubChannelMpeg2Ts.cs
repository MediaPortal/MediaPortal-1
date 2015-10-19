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
using System.Collections.ObjectModel;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts
{
  /// <summary>
  /// An <see cref="ISubChannel"/> implementation for MPEG 2 transport stream sub-channels (programs).
  /// </summary>
  internal class SubChannelMpeg2Ts : SubChannelBase, IChannelObserver
  {
    #region variables

    private object _lock = new object();

    /// <summary>
    /// The TS writer instance used to perform/implement time-shifting and recording.
    /// </summary>
    private ITsWriter _tsWriter = null;

    /// <summary>
    /// The handle that links this sub-channel with a corresponding sub-channel instance in the TS writer.
    /// </summary>
    private int _tsWriterHandle = -1;

    /// <summary>
    /// The current program map table for the program that the sub-channel is tuned to.
    /// </summary>
    private TableProgramMap _pmt = null;

    // Variables used to wait for video and/or audio.
    private bool _isExpectedVideo;
    private ManualResetEvent _eventVideo = null;
    private bool _isExpectedAudio;
    private ManualResetEvent _eventAudio = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelMpeg2Ts"/> class.
    /// </summary>
    /// <param name="subChannelId">The sub-channel ID to associate with this instance.</param>
    /// <param name="tsWriter">The TS writer instance used to perform/implement time-shifting and recording.</param>
    public SubChannelMpeg2Ts(int subChannelId, ITsWriter tsWriter)
      : base(subChannelId)
    {
      int hr = tsWriter.AddChannel(this, out _tsWriterHandle);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        throw new TvException("Failed to add TS writer channel, hr = 0x{0:x}.", hr);
      }
      this.LogDebug("MPEG 2 sub-channel: new sub-channel, ID = {0}, handle = {1}", _subChannelId, _tsWriterHandle);
      _tsWriter = tsWriter;
      _eventVideo = new ManualResetEvent(false);
      _eventAudio = new ManualResetEvent(false);
    }

    #region IChannelObserver member

    /// <summary>
    /// This function is invoked when the first unencrypted PES packet is received from a PID.
    /// </summary>
    /// <param name="pid">The PID that was seen.</param>
    /// <param name="pidType">The type of <paramref name="pid">PID</paramref>.</param>
    public void OnSeen(ushort pid, PidType pidType)
    {
      this.LogDebug("MPEG 2 sub-channel: on PID seen, ID = {0}, PID = {1}, PID type = {2}", _subChannelId, pid, pidType);
      if (pidType == PidType.Audio)
      {
        _eventAudio.Set();
      }
      else if (pidType == PidType.Video)
      {
        _eventVideo.Set();
      }
    }

    #endregion

    #region sub-channel base implementations/overrides

    /// <summary>
    /// Get the stream state.
    /// </summary>
    /// <param name="isReceivingVideo"><c>True</c> if video is being received.</param>
    /// <param name="isEncryptedVideo"><c>True</c> if the received video is currently encrypted.</param>
    /// <param name="isReceivingAudio"><c>True</c> if audio is being received.</param>
    /// <param name="isEncryptedAudio"><c>True</c> if the received audio is currently encrypted.</param>
    public override void GetStreamState(out bool isReceivingVideo, out bool isEncryptedVideo, out bool isReceivingAudio, out bool isEncryptedAudio)
    {
      isReceivingVideo = false;
      isEncryptedVideo = true;
      isReceivingAudio = false;
      isEncryptedAudio = true;
      lock (_lock)
      {
        if (_tsWriter == null || _pmt == null)
        {
          return;
        }
        foreach (var es in _pmt.ElementaryStreams)
        {
          if (StreamTypeHelper.IsVideoStream(es.LogicalStreamType))
          {
            EncryptionState state;
            _tsWriter.GetPidState(es.Pid, out state);
            if (state != EncryptionState.NotSet)
            {
              isReceivingVideo = true;
              isEncryptedVideo &= state == EncryptionState.Encrypted;
            }
          }
          else if (StreamTypeHelper.IsAudioStream(es.LogicalStreamType))
          {
            EncryptionState state;
            _tsWriter.GetPidState(es.Pid, out state);
            if (state != EncryptionState.NotSet)
            {
              isReceivingAudio = true;
              isEncryptedAudio &= state == EncryptionState.Encrypted;
            }
          }
        }
      }
    }

    /// <summary>
    /// Get information about the stream's quality.
    /// </summary>
    /// <param name="countBytes">The number of bytes processed.</param>    
    /// <param name="countDiscontinuities">The number of discontinuities encountered.</param>
    /// <param name="countDroppedBytes">The number of bytes dropped.</param>
    public override void GetStreamQuality(out ulong countBytes, out ulong countDiscontinuities, out ulong countDroppedBytes)
    {
      ulong countTsPackets;
      int hr;
      lock (_lock)
      {
        if (_tsWriter == null)
        {
          countBytes = 0;
          countDiscontinuities = 0;
          countDroppedBytes = 0;
          return;
        }

        if (IsRecording)
        {
          hr = _tsWriter.RecorderGetStreamQuality(_tsWriterHandle, out countTsPackets, out countDiscontinuities, out countDroppedBytes);
        }
        else
        {
          hr = _tsWriter.TimeShifterGetStreamQuality(_tsWriterHandle, out countTsPackets, out countDiscontinuities, out countDroppedBytes);
        }
      }
      countBytes = countTsPackets * 188;
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogWarn("MPEG 2 sub-channel: failed to get stream quality information, ID = {0}, hr = 0x{1:x}", _subChannelId, hr);
      }
    }

    /// <summary>
    /// Implementation of starting time-shifting.
    /// </summary>
    /// <param name="fileName">The name to use for the time-shift buffer register file.</param>
    /// <param name="fileCount">The number of buffer files to use during normal time-shifting.</param>
    /// <param name="fileCountMaximum">The maximum number of buffer files to use when time-shifting is paused.</param>
    /// <param name="fileSize">The size of each buffer file.</param>
    protected override void OnStartTimeShifting(string fileName, uint fileCount, uint fileCountMaximum, ulong fileSize)
    {
      lock (_lock)
      {
        if (_tsWriter == null)
        {
          throw new TvException("TS writer is null.");
        }

        int hr = _tsWriter.TimeShifterSetFileName(_tsWriterHandle, fileName);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to set time-shifter file name, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        hr = _tsWriter.TimeShifterSetParameters(_tsWriterHandle, fileCount, fileCountMaximum, fileSize);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to set time-shifter parameters, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        hr = _tsWriter.TimeShifterSetPmt(_tsWriterHandle, rawPmt, (ushort)rawPmt.Length, false);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to set time-shifter PMT, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        hr = _tsWriter.TimeShifterStart(_tsWriterHandle);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to start time-shifter, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        WaitForVideoAndAudio();
      }
    }

    /// <summary>
    /// Implementation of getting the current time-shift position.
    /// </summary>
    /// <param name="bufferId">The identifier of the current buffer file.</param>
    /// <param name="position">The position within the current buffer file.</param>
    protected override void OnGetCurrentTimeShiftPosition(out uint bufferId, out ulong position)
    {
      bufferId = 0;
      position = 0;
      lock (_lock)
      {
        if (_tsWriter == null)
        {
          throw new TvException("TS writer is null.");
        }

        int hr = _tsWriter.TimeShifterGetCurrentFilePosition(_tsWriterHandle, out position, out bufferId);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to get time-shift position, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }
      }
    }

    /// <summary>
    /// Implementation of stopping time-shifting.
    /// </summary>
    protected override void OnStopTimeShifting()
    {
      lock (_lock)
      {
        if (_tsWriter == null)
        {
          throw new TvException("TS writer is null.");
        }

        int hr = _tsWriter.TimeShifterStop(_tsWriterHandle);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to stop time-shifter, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }
      }
    }

    /// <summary>
    /// Implementation of starting recording.
    /// </summary>
    /// <param name="fileName">The name to use for the recording file.</param>
    protected override void OnStartRecording(string fileName)
    {
      lock (_lock)
      {
        if (_tsWriter == null)
        {
          throw new TvException("TS writer is null.");
        }

        int hr = _tsWriter.RecorderSetFileName(_tsWriterHandle, fileName);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to set recorder file name, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        hr = _tsWriter.RecorderSetPmt(_tsWriterHandle, rawPmt, (ushort)rawPmt.Length, false);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to set recorder PMT, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        hr = _tsWriter.RecorderStart(_tsWriterHandle);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to start recorder, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }

        WaitForVideoAndAudio();
      }
    }

    /// <summary>
    /// Implementation of stopping recording.
    /// </summary>
    protected override void OnStopRecording()
    {
      lock (_lock)
      {
        if (_tsWriter == null)
        {
          throw new TvException("TS writer is null.");
        }

        int hr = _tsWriter.RecorderStop(_tsWriterHandle);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to stop recorder, handle = {0}, hr = 0x{1:x}.", _tsWriterHandle, hr);
        }
      }
    }

    /// <summary>
    /// Implementation of tune cancellation.
    /// </summary>
    protected override void OnCancelTune()
    {
      if (_eventAudio != null)
      {
        _eventAudio.Set();
      }
      if (_eventVideo != null)
      {
        _eventVideo.Set();
      }
    }

    /// <summary>
    /// Implementation of decomposition.
    /// </summary>
    protected override void OnDecompose()
    {
      lock (_lock)
      {
        if (_tsWriter != null)
        {
          _tsWriter.DeleteChannel(_tsWriterHandle);
          _tsWriter = null;
        }
        _tsWriterHandle = -1;
      }

      if (_eventVideo != null)
      {
        _eventVideo.Set();
        _eventVideo.Close();
        _eventVideo.Dispose();
        _eventVideo = null;
      }
      if (_eventAudio != null)
      {
        _eventAudio.Set();
        _eventAudio.Close();
        _eventAudio.Dispose();
        _eventAudio = null;
      }
    }

    #endregion

    /// <summary>
    /// Update the sub-channel's program map table.
    /// </summary>
    /// <param name="pmt">The new program map table.</param>
    /// <param name="isDynamic"><c>True</c> if the update represents a dynamic PMT change.</param>
    /// <param name="hasVideo"><c>True</c> if the program has one or more video streams.</param>
    /// <param name="hasAudio"><c>True</c> if the program has one or more audio streams.</param>
    public void OnPmtUpdate(TableProgramMap pmt, bool isDynamic, bool hasVideo, bool hasAudio)
    {
      lock (_lock)
      {
        _pmt = pmt;

        if (_tsWriter == null || _pmt == null)
        {
          return;
        }

        _isExpectedVideo = hasVideo;
        _eventVideo.Reset();
        _isExpectedAudio = hasAudio;
        _eventAudio.Reset();

        bool isTimeShifting = IsTimeShifting;
        bool isRecording = IsRecording;
        if (!isTimeShifting && !isRecording)
        {
          return;
        }

        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        int hr;
        if (isTimeShifting)
        {
          hr = _tsWriter.TimeShifterSetPmt(_tsWriterHandle, rawPmt, (ushort)rawPmt.Length, isDynamic);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("MPEG 2 sub-channel: failed to update time-shifter PMT, ID = {0}, hr = 0x{1:x}", _subChannelId, hr);
          }
        }
        if (isRecording)
        {
          hr = _tsWriter.RecorderSetPmt(_tsWriterHandle, rawPmt, (ushort)rawPmt.Length, isDynamic);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("MPEG 2 sub-channel: failed to update recorder PMT, ID = {0}, hr = 0x{1:x}", _subChannelId, hr);
          }
        }

        WaitForVideoAndAudio();
      }
    }

    /// <summary>
    /// Wait for unencrypted video and/or audio to be received.
    /// </summary>
    private void WaitForVideoAndAudio()
    {
      this.LogDebug("MPEG 2 sub-channel: wait for video and/or audio, ID = {0}, video = {1}, audio = {2}", _subChannelId, _isExpectedVideo, _isExpectedAudio);
      int timeLimit = TimeLimitReceiveVideoAudio;
      DateTime startTime = DateTime.Now;
      if (
        (!_isExpectedVideo || _eventVideo.WaitOne(timeLimit)) &&
        (!_isExpectedAudio || _eventAudio.WaitOne(Math.Max(0, timeLimit - (int)((DateTime.Now - startTime).TotalMilliseconds))))
      )
      {
        ThrowExceptionIfTuneCancelled();
        return;
      }

      bool isReceivingVideo;
      bool isEncryptedVideo;
      bool isReceivingAudio;
      bool isEncryptedAudio;
      GetStreamState(out isReceivingVideo, out isEncryptedVideo, out isReceivingAudio, out isEncryptedAudio);
      if (_isExpectedVideo && _isExpectedAudio)
      {
        this.LogError("MPEG 2 sub-channel: stream state, ID = {0}, is receiving video = {1}, is video encrypted = {2}, is receiving audio = {3}, is audio encrypted = {4}",
                      _subChannelId, isReceivingVideo, isEncryptedVideo, isReceivingAudio, isEncryptedAudio);
        if (isEncryptedVideo || isEncryptedAudio)
        {
          throw new TvExceptionServiceEncrypted(CurrentChannel, isEncryptedVideo, isEncryptedAudio);
        }
        throw new TvExceptionStreamNotReceived(CurrentChannel, isReceivingVideo, isReceivingAudio);
      }
      else if (_isExpectedAudio)
      {
        this.LogError("MPEG 2 sub-channel: audio stream state, ID = {0}, is receiving = {1}, is encrypted = {2}", _subChannelId, isReceivingAudio, isEncryptedAudio);
        if (isEncryptedAudio)
        {
          throw new TvExceptionServiceEncrypted(CurrentChannel, false, isEncryptedAudio);
        }
        throw new TvExceptionStreamNotReceived(CurrentChannel, true, isReceivingAudio);
      }
      else if (_isExpectedVideo)
      {
        this.LogError("MPEG 2 sub-channel: video stream state, ID = {0}, is receiving = {1}, is encrypted = {2}", _subChannelId, isReceivingVideo, isEncryptedVideo);
        if (isEncryptedVideo)
        {
          throw new TvExceptionServiceEncrypted(CurrentChannel, isEncryptedVideo, false);
        }
        throw new TvExceptionStreamNotReceived(CurrentChannel, isReceivingVideo, true);
      }
    }
  }
}