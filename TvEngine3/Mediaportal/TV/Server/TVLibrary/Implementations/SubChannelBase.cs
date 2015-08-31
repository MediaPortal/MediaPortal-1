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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// Base class for a sub-channel of a tv card
  /// </summary>
  internal abstract class SubChannelBase : ISubChannelInternal
  {
    #region events

    /// <summary>
    /// Audio/video observer event.
    /// </summary>
    public event AudioVideoObserverEvent AudioVideoEvent;

    /// <summary>
    /// Handles the audio/video observer event.
    /// </summary>
    /// <param name="pidType">Type of the pid</param>
    protected void OnAudioVideoEvent(PidType pidType)
    {
      if (AudioVideoEvent != null)
      {
        AudioVideoEvent(pidType);
      }
    }

    /// <summary>
    /// After tune observer event.
    /// </summary>
    public event OnAfterTuneDelegate AfterTuneEvent;

    /// <summary>
    /// Handles the after tune observer event.
    /// </summary>
    protected void OnAfterTuneEvent()
    {
      if (AfterTuneEvent != null)
      {
        AfterTuneEvent();
      }
    }

    #endregion


    #region variables

    /// <summary>
    /// Instance of the current channel
    /// </summary>
    protected IChannel _currentChannel;

    /// <summary>
    /// Name of the timeshift file
    /// </summary>
    protected string _timeshiftFileName;

    /// <summary>
    /// Name of the recording file
    /// </summary>
    protected string _recordingFileName;

    /// <summary>
    /// Date and time when timeshifting started
    /// </summary>
    protected DateTime _dateTimeShiftStarted;

    /// <summary>
    /// Date  and time when recording started
    /// </summary>
    protected DateTime _dateRecordingStarted;

    /// <summary>
    /// This sub-channel's unique identifier.
    /// </summary>
    protected int _subChannelId;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process before it is completed.
    /// </summary>
    protected volatile bool _cancelTune;

    #endregion

    #region constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SubChannelBase"/> class.
    /// </summary>
    protected SubChannelBase(int subChannelId)
    {
      _cancelTune = false;
      _subChannelId = subChannelId;
      _timeshiftFileName = string.Empty;
      _recordingFileName = string.Empty;
      _dateRecordingStarted = DateTime.MinValue;
      _dateTimeShiftStarted = DateTime.MinValue;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets the sub-channel id.
    /// </summary>
    /// <value>The sub-channel id.</value>
    public int SubChannelId
    {
      get { return _subChannelId; }
    }

    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    public string TimeShiftFileName
    {
      get { return _timeshiftFileName; }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime StartOfTimeShift
    {
      get { return _dateTimeShiftStarted; }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get { return _dateRecordingStarted; }
    }

    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    public string RecordingFileName
    {
      get { return _recordingFileName; }
    }

    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get { return (_recordingFileName.Length > 0); }
    }

    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    public bool IsTimeShifting
    {
      get { return (_timeshiftFileName.Length > 0); }
    }

    /// <summary>
    /// returns the IChannel to which the card is currently tuned
    /// </summary>
    public IChannel CurrentChannel
    {
      get { return _currentChannel; }
      set { _currentChannel = value; }
    }

    #endregion

    #region timeshifting and recording

    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns></returns>
    public bool StartTimeShifting(string fileName, int fileCount, int fileCountMaximum, ulong fileSize)
    {
      this.LogDebug("sub-channel base: {0} start timeshifting to {1}", _subChannelId, fileName);
      try
      {
        OnStartTimeShifting(fileName, fileCount, fileCountMaximum, fileSize);
        _timeshiftFileName = fileName;
        _dateTimeShiftStarted = DateTime.Now;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to start timeshifting");
        StopTimeShifting();
        return false;
      }

      return true;
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns></returns>
    public bool StopTimeShifting()
    {
      this.LogDebug("sub-channel base: {0} stop timeshifting", _subChannelId);
      OnStopTimeShifting();
      _timeshiftFileName = string.Empty;
      _dateTimeShiftStarted = DateTime.MinValue;
      return true;
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns></returns>
    public bool StartRecording(string fileName)
    {
      this.LogDebug("sub-channel base: {0} start recording to {1}", _subChannelId, fileName);
      try
      {
        OnStartRecording(fileName);
        _recordingFileName = fileName;
        _dateRecordingStarted = DateTime.Now;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "sub-channel base: failed to start recording");
        StopRecording();
        return false;
      }

      return true;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    public bool StopRecording()
    {
      this.LogDebug("sub-channel base: {0} stop recording", _subChannelId);
      OnStopRecording();
      _recordingFileName = string.Empty;
      _dateRecordingStarted = DateTime.MinValue;
      return true;
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public void TimeShiftGetCurrentFilePosition(out long position, out long bufferId)
    {
      OnGetTimeShiftFilePosition(out position, out bufferId);
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public virtual void CancelTune()
    {
      this.LogDebug("sub-channel base: {0} cancel tune", _subChannelId);
      _cancelTune = true;
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an exception if it has.
    /// </summary>
    protected void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    #endregion

    #region public helper

    /// <summary>
    /// Decomposes the sub-channel
    /// </summary>
    public void Decompose()
    {
      this.LogDebug("sub-channel base: {0} decompose", _subChannelId);

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

    #region public abstract methods

    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public abstract void OnBeforeTune();

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public abstract void OnAfterTune();

    /// <summary>
    /// Should be called when the graph has been started
    /// sets up the pmt grabber to grab the pmt of the channel
    /// </summary>
    public abstract void OnGraphRunning();

    #endregion

    #region protected abstract methods

    /// <summary>
    /// A derrived class should do it's specific cleanup here. It will be called from called from Decompose()
    /// </summary>
    protected abstract void OnDecompose();

    /// <summary>
    /// A derrived class should start here the timeshifting on the tv card. It will be called from StartTimeshifting()
    /// </summary>
    protected abstract void OnStartTimeShifting(string fileName, int fileCount, int fileCountMaximum, ulong fileSize);

    /// <summary>
    /// A derrived class should stop here the timeshifting on the tv card. It will be called from StopTimeshifting()
    /// </summary>
    protected abstract void OnStopTimeShifting();

    /// <summary>
    /// A derrived class should start here the recording on the tv card. It will be called from StartRecording()
    /// </summary>
    protected abstract void OnStartRecording(string fileName);

    /// <summary>
    /// A derrived class should stop here the recording on the tv card. It will be called from StopRecording()
    /// </summary>
    protected abstract void OnStopRecording();

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    protected abstract void OnGetTimeShiftFilePosition(out long position, out long bufferId);

    #endregion

    #region abstract ITvSubChannel members

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public abstract bool IsReceivingAudioVideo { get; }

    #endregion

    /// <summary>
    /// Fetch stream quality information from TsWriter.
    /// </summary>   
    /// <param name="totalBytes">The number of packets processed.</param>    
    /// <param name="discontinuityCounter">The number of stream discontinuities.</param>
    public abstract void GetStreamQualityCounters(out int totalBytes, out int discontinuityCounter);
  }
}