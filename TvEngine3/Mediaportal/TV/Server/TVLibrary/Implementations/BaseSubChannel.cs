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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Teletext.Implementations;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// Base class for a sub channel of a tv card
  /// </summary>
  public abstract class BaseSubChannel : ITvSubChannel
  {
    #region events

    /// <summary>
    /// Delegate for the audio/video oberserver events.
    /// </summary>
    /// <param name="pidType">Type of the pid</param>
    public delegate void AudioVideoObserverEvent(PidType pidType);

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
    /// Delegate for the after tune event.
    /// </summary>
    public delegate void OnAfterTuneDelegate();

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
    /// Indicates, if the channel has teletext
    /// </summary>
    protected bool _hasTeletext;

    /// <summary>
    /// Indicates, if teletext grabbing is activated
    /// </summary>
    protected bool _grabTeletext;

    /// <summary>
    /// Instance of the teletext decoder
    /// </summary>
    protected DVBTeletext _teletextDecoder;

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
    /// ID of this subchannel
    /// </summary>
    protected int _subChannelId;

    /// <summary>
    /// Scanning parameters
    /// </summary>
    protected ScanParameters _parameters;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process before it is completed.
    /// </summary>
    protected bool _cancelTune;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSubChannel"/> class.
    /// </summary>
    protected BaseSubChannel(int subChannelId)
    {
      _cancelTune = false;
      _subChannelId = subChannelId;
      _teletextDecoder = new DVBTeletext();
      _timeshiftFileName = String.Empty;
      _recordingFileName = String.Empty;
      _dateRecordingStarted = DateTime.MinValue;
      _dateTimeShiftStarted = DateTime.MinValue;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets the sub channel id.
    /// </summary>
    /// <value>The sub channel id.</value>
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

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    public ScanParameters Parameters
    {
      get { return _parameters; }
      set { _parameters = value; }
    }

    #endregion

    #region teletext

    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    /// <value></value>
    public bool HasTeletext
    {
      get { return _hasTeletext; }
    }

    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    public bool GrabTeletext
    {
      get { return _grabTeletext; }
      set
      {
        _grabTeletext = value;
        OnGrabTeletext();
      }
    }

    /// <summary>
    /// returns the ITeletext interface used for retrieving the teletext pages
    /// </summary>
    public ITeletext TeletextDecoder
    {
      get
      {
        if (!_hasTeletext) return null;
        return _teletextDecoder;
      }
    }

    #endregion

    #region timeshifting and recording

    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns></returns>
    public bool StartTimeShifting(string fileName)
    {
      Log.Debug("BaseSubChannel: subchannel {0} start timeshifting to {1}", _subChannelId, fileName);
      try
      {
        OnStartTimeShifting(fileName);
        _timeshiftFileName = fileName;
        _dateTimeShiftStarted = DateTime.Now;
      }
      catch (Exception ex)
      {
        Log.Debug("BaseSubChannel: failed to start timeshifting\r\n{0}", ex.ToString());
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
      Log.Debug("BaseSubChannel: subchannel {0} stop timeshifting", _subChannelId);
      OnStopTimeShifting();
      _timeshiftFileName = String.Empty;
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
      Log.Debug("BaseSubChannel: subchannel {0} start recording to {1}", _subChannelId, fileName);
      try
      {
        OnStartRecording(fileName);
        _recordingFileName = fileName;
        _dateRecordingStarted = DateTime.Now;
      }
      catch (Exception ex)
      {
        Log.Debug("BaseSubChannel: failed to start recording\r\n{0}", ex.ToString());
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
      Log.Debug("BaseSubChannel: subchannel {0} stop recording", _subChannelId);
      OnStopRecording();
      _recordingFileName = String.Empty;
      _dateRecordingStarted = DateTime.MinValue;
      return true;
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public void TimeShiftGetCurrentFilePosition(ref Int64 position, ref long bufferId)
    {
      OnGetTimeShiftFilePosition(ref position, ref bufferId);
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public virtual void CancelTune()
    {
      Log.Debug("BaseSubChannel: subchannel {0} cancel tune", _subChannelId);
      _cancelTune = true;
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw and exception if it has.
    /// </summary>
    protected void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    #endregion

    #region IAnalogTeletextCallBack and ITeletextCallBack Members

    /// <summary>
    /// callback from the TsWriter filter when it received a new teletext packets
    /// </summary>
    /// <param name="data">teletext data</param>
    /// <param name="packetCount">number of packets in data</param>
    /// <returns></returns>
    public int OnTeletextReceived(IntPtr data, short packetCount)
    {
      try
      {
        for (int i = 0; i < packetCount; ++i)
        {
          IntPtr packetPtr = new IntPtr(data.ToInt64() + i * 188);
          ProcessPacket(packetPtr);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(ex.ToString());
      }
      return 0;
    }

    /// <summary>
    /// processes a single transport packet
    /// Called from BufferCB
    /// </summary>
    /// <param name="ptr">pointer to the transport packet</param>
    public void ProcessPacket(IntPtr ptr)
    {
      if (_teletextDecoder != null)
      {
        _teletextDecoder.SaveData(ptr);
      }
    }

    #endregion

    #region IAnalogVideoAudioObserver

    /// <summary>
    /// Called when tswriter.ax has seen the video / audio data for the first time
    /// </summary>
    /// <returns></returns>
    public int OnNotify(PidType pidType)
    {
      try
      {
        Log.WriteFile("PID seen - type = {0}", pidType);
        OnAudioVideoEvent(pidType);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return 0;
    }

    #endregion

    #region public helper

    /// <summary>
    /// Decomposes the sub channel
    /// </summary>
    public void Decompose()
    {
      Log.Debug("BaseSubChannel: subchannel {0} decompose", _subChannelId);

      if (IsRecording)
      {
        StopRecording();
      }
      if (IsTimeShifting)
      {
        StopTimeShifting();
      }
      _timeshiftFileName = String.Empty;
      _recordingFileName = String.Empty;
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
      OnDecompose();
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

    /// <summary>
    /// Should be called when graph is about to stop.
    /// stops any timeshifting/recording on this channel
    /// </summary>
    public abstract void OnGraphStop();

    /// <summary>
    /// should be called when graph has been stopped
    /// Resets the graph state
    /// </summary>
    public abstract void OnGraphStopped();

    #endregion

    #region protected abstract methods

    /// <summary>
    /// A derrived class should do it's specific cleanup here. It will be called from called from Decompose()
    /// </summary>
    protected abstract void OnDecompose();

    /// <summary>
    /// A derrived class should start here the timeshifting on the tv card. It will be called from StartTimeshifting()
    /// </summary>
    protected abstract void OnStartTimeShifting(string fileName);

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
    protected abstract void OnGetTimeShiftFilePosition(ref Int64 position, ref long bufferId);

    /// <summary>
    /// A derrived class should activate or deactivate the teletext grabbing on the tv card.
    /// </summary>
    protected abstract void OnGrabTeletext();

    #endregion

    #region abstract ITvSubChannel members

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public abstract bool IsReceivingAudioVideo { get; }

    #endregion
  }
}