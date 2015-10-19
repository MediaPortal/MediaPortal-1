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
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// Base class for a tuner sub-channel.
  /// </summary>
  internal abstract class SubChannelBase : ISubChannelInternal
  {
    #region variables

    /// <summary>
    /// The sub-channel's unique identifier.
    /// </summary>
    protected int _subChannelId;

    /// <summary>
    /// The time-shift buffer register file name.
    /// </summary>
    private string _timeShiftFileName = string.Empty;

    /// <summary>
    /// The date/time when time-shifting was started.
    /// </summary>
    private DateTime _timeShiftStartTime = DateTime.MinValue;

    /// <summary>
    /// The recording file name.
    /// </summary>
    private string _recordFileName = string.Empty;

    /// <summary>
    /// The date/time when recording was started.
    /// </summary>
    private DateTime _recordStartTime = DateTime.MinValue;

    /// <summary>
    /// The channel which the sub-channel is tuned to.
    /// </summary>
    private IChannel _currentChannel = null;

    /// <summary>
    /// Should the current tuning process be aborted immediately?
    /// </summary>
    private volatile bool _cancelTune = false;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelBase"/> class.
    /// </summary>
    /// <param name="subChannelId">The sub-channel's identifier.</param>
    protected SubChannelBase(int subChannelId)
    {
      _subChannelId = subChannelId;
      _timeShiftFileName = string.Empty;
      _timeShiftStartTime = DateTime.MinValue;
      _recordFileName = string.Empty;
      _recordStartTime = DateTime.MinValue;
    }

    #endregion

    #region ISubChannel members

    #region properties

    /// <summary>
    /// Get the sub-channel's identifier.
    /// </summary>
    public int SubChannelId
    {
      get
      {
        return _subChannelId;
      }
    }

    /// <summary>
    /// Get the time-shift buffer register file name.
    /// </summary>
    public string TimeShiftFileName
    {
      get
      {
        return _timeShiftFileName;
      }
    }

    /// <summary>
    /// Get the date/time when time-shifting was started.
    /// </summary>
    public DateTime TimeShiftStartTime
    {
      get
      {
        return _timeShiftStartTime;
      }
    }

    /// <summary>
    /// Is the sub-channel currently time-shifting?
    /// </summary>
    public bool IsTimeShifting
    {
      get
      {
        return !string.IsNullOrEmpty(_timeShiftFileName);
      }
    }

    /// <summary>
    /// Get the recording file name.
    /// </summary>
    public string RecordFileName
    {
      get
      {
        return _recordFileName;
      }
    }

    /// <summary>
    /// Get the date/time when recording was started.
    /// </summary>
    public DateTime RecordStartTime
    {
      get
      {
        return _recordStartTime;
      }
    }

    /// <summary>
    /// Is the sub-channel currently recording?
    /// </summary>
    public bool IsRecording
    {
      get
      {
        return !string.IsNullOrEmpty(_recordFileName);
      }
    }

    /// <summary>
    /// Get or set the channel which the sub-channel is tuned to.
    /// </summary>
    public IChannel CurrentChannel
    {
      get
      {
        return _currentChannel;
      }
      set
      {
        _currentChannel = value;
      }
    }

    #endregion

    #region time-shifting and recording

    /// <summary>
    /// Start time-shifting.
    /// </summary>
    /// <param name="fileName">The name to use for the time-shift buffer register file.</param>
    /// <param name="fileCount">The number of buffer files to use during normal time-shifting.</param>
    /// <param name="fileCountMaximum">The maximum number of buffer files to use when time-shifting is paused.</param>
    /// <param name="fileSize">The size of each buffer file.</param>
    /// <param name="isEncrypted"><c>True</c> if time-shifting failed to start because the video and/or audio streams are encrypted.</param>
    /// <returns><c>true</c> if time-shifting was started successfully, otherwise <c>false</c></returns>
    public bool StartTimeShifting(string fileName, uint fileCount, uint fileCountMaximum, ulong fileSize, out bool isEncrypted)
    {
      this.LogDebug("sub-channel base: start time-shifting, ID = {0}, file name = {1}", _subChannelId, fileName);
      isEncrypted = false;
      _cancelTune = false;
      try
      {
        if (IsTimeShifting)
        {
          this.LogError("sub-channel base: already time-shifting, ID = {0}, file name = {1}", _subChannelId, _timeShiftFileName);
          return false;
        }
        OnStartTimeShifting(fileName, fileCount, fileCountMaximum, fileSize);
        _timeShiftFileName = fileName;
        _timeShiftStartTime = DateTime.Now;
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to start time-shifting, ID = {0}, file name = {1}", _subChannelId, fileName);
        if (ex is TvExceptionServiceEncrypted)
        {
          isEncrypted = true;
        }
        StopTimeShifting();
        return false;
      }
    }

    /// <summary>
    /// Get the current time-shift position.
    /// </summary>
    /// <param name="bufferId">The identifier of the current buffer file.</param>
    /// <param name="position">The position within the current buffer file.</param>
    /// <returns><c>true</c> if the position was retrieved successfully, otherwise <c>false</c></returns>
    public bool GetCurrentTimeShiftPosition(out uint bufferId, out ulong position)
    {
      bufferId = 0;
      position = 0;
      try
      {
        if (!IsTimeShifting)
        {
          this.LogWarn("sub-channel base: not time-shifting, ID = {0}", _subChannelId);
          return false;
        }
        OnGetCurrentTimeShiftPosition(out bufferId, out position);
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to get the current time-shift position, ID = {0}", _subChannelId);
        return false;
      }
    }

    /// <summary>
    /// Stop time-shifting.
    /// </summary>
    /// <returns><c>true</c> if time-shifting was stopped successfully, otherwise <c>false</c></returns>
    public bool StopTimeShifting()
    {
      this.LogDebug("sub-channel base: stop time-shifting, ID = {0}", _subChannelId);
      try
      {
        if (!IsTimeShifting)
        {
          this.LogWarn("sub-channel base: not time-shifting, ID = {0}", _subChannelId);
        }
        OnStopTimeShifting();
        _timeShiftFileName = string.Empty;
        _timeShiftStartTime = DateTime.MinValue;
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to stop time-shifting, ID = {0}", _subChannelId);
        return false;
      }
    }

    /// <summary>
    /// Start recording.
    /// </summary>
    /// <param name="fileName">The name to use for the recording file.</param>
    /// <param name="isEncrypted"><c>True</c> if recording failed to start because the video and/or audio streams are encrypted.</param>
    /// <returns><c>true</c> if recording was started successfully, otherwise <c>false</c></returns>
    public bool StartRecording(string fileName, out bool isEncrypted)
    {
      this.LogDebug("sub-channel base: start recording, ID = {0}, file name = {1}", _subChannelId, fileName);
      isEncrypted = false;
      _cancelTune = false;
      try
      {
        if (IsRecording)
        {
          this.LogError("sub-channel base: already recording, ID = {0}, file name = {1}", _subChannelId, _recordFileName);
          return false;
        }
        OnStartRecording(fileName);
        _recordFileName = fileName;
        _recordStartTime = DateTime.Now;
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to start recording, ID = {0}, file name = {1}", _subChannelId, fileName);
        if (ex is TvExceptionServiceEncrypted)
        {
          isEncrypted = true;
        }
        StopRecording();
        return false;
      }
    }

    /// <summary>
    /// Stop recording.
    /// </summary>
    /// <returns><c>true</c> if recording was stopped successfully, otherwise <c>false</c></returns>
    public bool StopRecording()
    {
      this.LogDebug("sub-channel base: stop recording, ID = {0}", _subChannelId);
      try
      {
        if (!IsRecording)
        {
          this.LogWarn("sub-channel base: not recording, ID = {0}", _subChannelId);
        }
        OnStopRecording();
        _recordFileName = string.Empty;
        _recordStartTime = DateTime.MinValue;
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to stop recording, ID = {0}", _subChannelId);
        return false;
      }
    }

    #endregion

    /// <summary>
    /// Get the stream state.
    /// </summary>
    /// <param name="isReceivingVideo"><c>True</c> if video is being received.</param>
    /// <param name="isEncryptedVideo"><c>True</c> if the received video is currently encrypted.</param>
    /// <param name="isReceivingAudio"><c>True</c> if audio is being received.</param>
    /// <param name="isEncryptedAudio"><c>True</c> if the received audio is currently encrypted.</param>
    public abstract void GetStreamState(out bool isReceivingVideo, out bool isEncryptedVideo, out bool isReceivingAudio, out bool isEncryptedAudio);

    /// <summary>
    /// Get information about the stream's quality.
    /// </summary>
    /// <param name="countBytes">The number of bytes processed.</param>    
    /// <param name="countDiscontinuities">The number of discontinuities encountered.</param>
    /// <param name="countDroppedBytes">The number of bytes dropped.</param>
    public abstract void GetStreamQuality(out ulong countBytes, out ulong countDiscontinuities, out ulong countDroppedBytes);

    #endregion

    #region ISubChannelInternal members

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public void CancelTune()
    {
      this.LogDebug("sub-channel base: cancel tune, ID = {0}", _subChannelId);
      _cancelTune = true;
    }

    /// <summary>
    /// Decompose the sub-channel.
    /// </summary>
    public void Decompose()
    {
      this.LogDebug("sub-channel base: decompose, ID = {0}", _subChannelId);

      if (IsRecording)
      {
        StopRecording();
      }
      if (IsTimeShifting)
      {
        StopTimeShifting();
      }
      OnDecompose();
      _currentChannel = null;
    }

    #endregion

    #region protected members

    #region abstract members

    /// <summary>
    /// Implementation of starting time-shifting.
    /// </summary>
    /// <param name="fileName">The name to use for the time-shift buffer register file.</param>
    /// <param name="fileCount">The number of buffer files to use during normal time-shifting.</param>
    /// <param name="fileCountMaximum">The maximum number of buffer files to use when time-shifting is paused.</param>
    /// <param name="fileSize">The size of each buffer file.</param>
    protected abstract void OnStartTimeShifting(string fileName, uint fileCount, uint fileCountMaximum, ulong fileSize);

    /// <summary>
    /// Implementation of getting the current time-shift position.
    /// </summary>
    /// <param name="bufferId">The identifier of the current buffer file.</param>
    /// <param name="position">The position within the current buffer file.</param>
    protected abstract void OnGetCurrentTimeShiftPosition(out uint bufferId, out ulong position);

    /// <summary>
    /// Implementation of stopping time-shifting.
    /// </summary>
    protected abstract void OnStopTimeShifting();

    /// <summary>
    /// Implementation of starting recording.
    /// </summary>
    /// <param name="fileName">The name to use for the recording file.</param>
    protected abstract void OnStartRecording(string fileName);

    /// <summary>
    /// Implementation of stopping recording.
    /// </summary>
    protected abstract void OnStopRecording();

    /// <summary>
    /// Implementation of tune cancellation.
    /// </summary>
    protected abstract void OnCancelTune();

    /// <summary>
    /// Implementation of decomposition.
    /// </summary>
    protected abstract void OnDecompose();

    #endregion

    /// <summary>
    /// Get the maximum time to wait for the video and/or audio streams to
    /// start being received.
    /// </summary>
    protected int TimeLimitReceiveVideoAudio
    {
      get
      {
        return SettingsManagement.GetValue("timeLimitReceiveVideoAudio", 5000);
      }
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an
    /// exception if it has.
    /// </summary>
    protected void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    #endregion
  }
}