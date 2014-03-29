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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  ///<summary>
  /// A base class for digital services ("subchannels").
  ///</summary>
  public class Mpeg2SubChannel : BaseSubChannel, IPmtCallBack, ICaCallBack, IVideoAudioObserver
  {
    #region variables

    #region local variables

    /// <summary>
    /// The current PMT PID for the service that this subchannel represents.
    /// </summary>
    private int _pmtPid = -1;

    /// <summary>
    /// Set by the TsWriter OnPmtReceived() call back. Indicates whether the
    /// service that this subchannel represents is currently active.
    /// </summary>
    private bool _isServiceRunning = false;

    /// <summary>
    /// Ts filter instance
    /// </summary>
    private ITsFilter _tsFilterInterface;

    /// <summary>
    /// The handle that links this subchannel with a corresponding subchannel instance in TsWriter.
    /// </summary>
    private int _subChannelIndex = -1;

    /// set to true to enable PAT lookup of PMT
    private bool _alwaysLookupPmtPidInPat = DebugSettings.UsePATLookup;

    private Pmt _pmt;
    private Cat _cat;
    private List<ushort> _pids;

    #endregion

    #region events

    /// <summary>
    /// Event that gets signaled when a new PMT section is seen.
    /// </summary>
    protected ManualResetEvent _eventPmt;

    /// <summary>
    /// Event that gets signaled when a new CAT section is seen.
    /// </summary>
    private ManualResetEvent _eventCat;

    #endregion

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="Mpeg2SubChannel"/> class.
    /// </summary>
    /// <param name="subChannelId">The subchannel ID to associate with this instance.</param>
    /// <param name="tuner">The tuner that this instance is associated with.</param>
    /// <param name="tsWriter">The TsWriter filter instance used to perform/implement timeshifting and recording.</param>
    public Mpeg2SubChannel(int subChannelId, ITVCard tuner, ITsFilter tsWriter)
      : base(subChannelId)
    {
      _eventPmt = new ManualResetEvent(false);
      _eventCat = new ManualResetEvent(false);

      _tuner = tuner;
      _subChannelIndex = -1;
      _tsFilterInterface = tsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelIndex);
      this.LogDebug("MPEG 2 sub-channel: new subchannel {0} index {1}", _subChannelId, _subChannelIndex);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~Mpeg2SubChannel()
    {
      if (_eventPmt != null)
      {
        _eventPmt.Close();
        _eventPmt = null;
      }

      if (_eventCat != null)
      {
        _eventCat.Close();
        _eventCat = null;
      }
    }

    #endregion

    #region properties

    public List<ushort> Pids
    {
      get
      {
        return _pids;
      }
    }

    public Pmt Pmt
    {
      get
      {
        return _pmt;
      }
    }

    public Cat Cat
    {
      get
      {
        return _cat;
      }
    }

    #endregion

    #region tuning and graph methods

    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public override void OnBeforeTune()
    {
      this.LogDebug("subch:{0} OnBeforeTune", _subChannelId);
    }

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public override void OnAfterTune()
    {
      this.LogDebug("subch:{0} OnAfterTune", _subChannelId);

      // Pass the core PIDs to the tuner's hardware PID filter so that we can do
      // basic tuning and scanning.
      _pids = new List<ushort>();
      _pids.Add(0x0);         // PAT - for service lookup
      _pids.Add(0x1);         // CAT - for conditional access info when the service needs to be decrypted
      DVBBaseChannel digitalChannel = _currentChannel as DVBBaseChannel;
      if (digitalChannel != null)
      {
        // If we can, also pass the PMT PID. We don't know what the PMT PID is when scanning.
        if (digitalChannel.PmtPid > 0)
        {
          _pids.Add((ushort)digitalChannel.PmtPid);
        }

        ATSCChannel atscChannel = _currentChannel as ATSCChannel;
        if (atscChannel == null)
        {
          _pids.Add(0x10);    // DVB NIT - for service info
          _pids.Add(0x11);    // DVB SDT, BAT - for service info
        }
        else
        {
          _pids.Add(0x1ffb);  // ATSC VCT - for terretrial service info
          _pids.Add(0x1ffc);  // SCTE VCT - for cable service info
        }
      }
    }

    /// <summary>
    /// Wait for TsWriter to find PMT in the transport stream.
    /// </summary>
    /// <param name="serviceId">The service ID of the service being tuned.</param>
    /// <param name="pmtPid">The PMT PID of the service being tuned.</param>
    /// <returns><c>true</c> if PMT was found, otherwise <c>false</c></returns>
    protected bool WaitForPmt(int serviceId, int pmtPid)
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} wait for PMT, service ID = {1}, PMT PID = {2}", _subChannelId, serviceId, pmtPid);

      // There 3 classes of service ID settings:
      // -1 = Scanning behaviour, where we don't care about PMT.
      // 0 = The service is expected to be the only service in the transport stream. TsWriter should take the
      //      first service that it sees and grab the associated PMT sections for that service. This situation
      //      is most applicable for providers that re-broadcast services without updating/fixing the SI.
      // <anything else> = A valid service ID for the service that we are trying to tune. TsWriter should grab
      //                    the associated PMT sections.

      // There are also 3 classes of PMT PID settings:
      // -1 = We don't know the correct/current PMT PID, so we ask TsWriter to determine what it should be,
      //      and then grab the associated PMT sections. Once PMT is received, we update the channel/tuning
      //      detail with the correct/current PID.
      // 0 = We don't know the correct/current PMT PID, so we ask TsWriter to determine what it should be,
      //      and then grab the associated PMT sections. We do *not* update the channel/tuning detail.
      // <anything else> = A valid PMT PID for the service that we are trying to tune. TsWriter should grab
      //                    the associated PMT sections.

      if (serviceId < 0)
      {
        return true;
      }

      int pmtPidToSearchFor;
      if (_alwaysLookupPmtPidInPat || pmtPid < 0)
      {
        pmtPidToSearchFor = 0;
      }
      else
      {
        pmtPidToSearchFor = pmtPid;
      }

      bool pmtFound = false;
      TimeSpan waitLength = TimeSpan.MinValue;
      while (!pmtFound)
      {
        if (pmtPidToSearchFor == 0)
        {
          this.LogDebug("MPEG 2 sub-channel: search for updated PMT PID in PAT");
        }
        this.LogDebug("MPEG 2 sub-channel: configure PMT grabber, PMT PID = {0}", pmtPidToSearchFor);
        _tsFilterInterface.PmtSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.PmtSetPmtPid(_subChannelIndex, pmtPidToSearchFor, serviceId);

        OnAfterTuneEvent();

        // Do this as late as possible. Any PMT that arrives between when the PMT call back was set and
        // when we start waiting for PMT will cause us to miss or mess up the PMT handling.
        _pmtPid = -1;
        _isServiceRunning = false;
        _pmt = null;
        _cat = null;
        _eventPmt.Reset();
        DateTime dtStartWait = DateTime.Now;
        ThrowExceptionIfTuneCancelled();
        pmtFound = _eventPmt.WaitOne(_parameters.TimeOutPMT * 1000, true);
        ThrowExceptionIfTuneCancelled();
        waitLength = DateTime.Now - dtStartWait;
        if (!pmtFound)
        {
          this.LogDebug("MPEG 2 sub-channel: timed out waiting for PMT after {0} seconds", waitLength.TotalSeconds);
          // One retry allowed...
          if (pmtPidToSearchFor == 0)
          {
            this.LogDebug("MPEG 2 sub-channel: giving up waiting for PMT - you might need to increase the PMT timeout");
            return false;
          }
          else
          {
            pmtPidToSearchFor = 0;
          }
        }
      }

      if (!_isServiceRunning)
      {
        throw new TvExceptionServiceNotRunning();
      }

      this.LogDebug("MPEG 2 sub-channel: found PMT after {0} seconds", waitLength.TotalSeconds);
      bool pmtIsValid = HandlePmt();
      if (pmtIsValid)
      {
        BuildPidList();
        if (IsTimeShifting)
        {
          SetTimeShiftPids();
        }
        if (IsRecording)
        {
          SetRecorderPids();
        }
      }

      return pmtIsValid;
    }

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// </summary>
    public override void OnGraphRunning()
    {
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} OnGraphRunning()", _subChannelId);

      DVBBaseChannel dvbService = _currentChannel as DVBBaseChannel;
      if (dvbService == null)
      {
        throw new TvException("MPEG 2 sub-channel: current service is not set");
      }
      if (!WaitForPmt(dvbService.ServiceId, dvbService.PmtPid))
      {
        throw new TvExceptionNoPMT("MPEG 2 sub-channel: PMT not received");
      }
    }

    #endregion

    #region Timeshifting - Recording methods

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="fileName">filename to which to recording should be saved</param>
    protected override void OnStartRecording(string fileName)
    {
      this.LogDebug("subch:{0} StartRecord({1})", _subChannelId, fileName);
      if (_tsFilterInterface != null)
      {
        int hr = _tsFilterInterface.RecordSetRecordingFileNameW(_subChannelIndex, fileName);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("subch:{0} SetRecordingFileName failed:{1:X}", _subChannelId, hr);
        }
        this.LogDebug("subch:{0}-{1} tswriter StartRecording...", _subChannelId, _subChannelIndex);
        SetRecorderPids();

        this.LogDebug("Set video / audio observer");
        _tsFilterInterface.RecorderSetVideoAudioObserver(_subChannelIndex, this);

        hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("subch:{0} tswriter StartRecord failed:{1:X}", _subChannelId, hr);
        }
      }
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopRecording()
    {
      this.LogDebug("tvdvbchannel.OnStopRecording subch={0}, subch index={1}", _subChannelId, _subChannelIndex);
      if (IsRecording)
      {
        if (_tsFilterInterface != null)
        {
          this.LogDebug("tvdvbchannel.OnStopRecording subch:{0}-{1} tswriter StopRecording...", _subChannelId,
                            _subChannelIndex);
          _tsFilterInterface.RecordStopRecord(_subChannelIndex);
        }
      }
      else
      {
        this.LogDebug("tvdvbchannel.OnStopRecording - not recording");
      }
    }

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override void OnStartTimeShifting(string fileName)
    {
      this.LogDebug("subch:{0} SetTimeShiftFileName:{1}", _subChannelId, fileName);
      //int hr;
      if (_tsFilterInterface != null)
      {
        this.LogDebug("Set video / audio observer");
        _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);
        _tsFilterInterface.TimeShiftSetParams(_subChannelIndex, _parameters.MinimumFiles, _parameters.MaximumFiles,
                                              _parameters.MaximumFileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileNameW(_subChannelIndex, fileName);

        if (CurrentChannel == null)
        {
          this.LogError("CurrentChannel is null when trying to start timeshifting");
          throw new TvException("MPEG 2 sub-channel: current channel is null");
        }

        //  Set the channel type (0=tv, 1=radio)
        _tsFilterInterface.TimeShiftSetChannelType(_subChannelId, (CurrentChannel.MediaType == MediaTypeEnum.TV ? 0 : 1));

        this.LogDebug("subch:{0} SetTimeShiftFileName fill in pids", _subChannelId);
        SetTimeShiftPids();
        this.LogDebug("subch:{0}-{1} tswriter StartTimeshifting...", _subChannelId, _subChannelIndex);
        _tsFilterInterface.TimeShiftStart(_subChannelIndex);
      }
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopTimeShifting()
    {
      if (IsTimeShifting)
      {
        this.LogDebug("subch:{0}-{1} tswriter StopTimeshifting...", _subChannelId, _subChannelIndex);
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.TimeShiftStop(_subChannelIndex);
        }
      }
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    protected override void OnGetTimeShiftFilePosition(ref long position, ref long bufferId)
    {
      _tsFilterInterface.TimeShiftGetCurrentFilePosition(_subChannelId, out position, out bufferId);
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public override void CancelTune()
    {
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} cancel tune", _subChannelId);
      _cancelTune = true;
      if (_eventCat != null)
      {
        _eventCat.Set();
      }
      if (_eventPmt != null)
      {
        _eventPmt.Set();
      }
    }

    #endregion

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public override bool IsReceivingAudioVideo
    {
      get
      {
        if (_tsFilterInterface == null)
          return false;
        if (_currentChannel == null)
          return false;

        //TODO
        return true;
        /*int audioEncrypted;
        int videoEncrypted = 0;
        _tsFilterInterface.AnalyzerIsAudioEncrypted(_subChannelIndex, out audioEncrypted);
        if (_currentChannel.IsTv)
        {
          _tsFilterInterface.AnalyzerIsVideoEncrypted(_subChannelIndex, out videoEncrypted);
        }
        return ((audioEncrypted == 0) && (videoEncrypted == 0));*/
      }
    }

    #region OnDecompose

    /// <summary>
    /// disposes this channel
    /// </summary>
    protected override void OnDecompose()
    {
      if (_tsFilterInterface != null && _subChannelIndex >= 0)
      {
        _tsFilterInterface.DeleteChannel(_subChannelIndex);
        _subChannelIndex = -1;
      }
    }

    #endregion

    #region pidmapping

    /// <summary>
    /// maps the correct pidSet to the TsFileSink filter and teletext pins
    /// </summary>
    protected void BuildPidList()
    {
      try
      {
        ThrowExceptionIfTuneCancelled();
        this.LogDebug("MPEG 2 sub-channel: subchannel {0} build PID list", _subChannelId);
        if (_pmt == null)
        {
          this.LogDebug("MPEG 2 sub-channel: PMT not available");
          return;
        }

        _pids = new List<ushort>();
        _pids.Add(0x0);             // PAT - for PMT monitoring
        _pids.Add(0x1);             // CAT - for conditional access info when the service needs to be decrypted
        DVBBaseChannel digitalChannel = _currentChannel as DVBBaseChannel;
        if (digitalChannel != null)
        {
          if (_currentChannel is ATSCChannel)
          {
            _pids.Add(0x1ffb);      // ATSC VCT - for terrestrial EPG info
            _pids.Add(0x1ffc);      // SCTE VCT - for cable EPG info
          }
          else
          {
            _pids.Add(0x12);        // DVB EIT - for EPG info
          }
        }
        if (_pmtPid > 0)
        {
          _pids.Add((ushort)_pmtPid); // PMT - for elementary stream and conditional access changes
        }

        foreach (PmtElementaryStream es in _pmt.ElementaryStreams)
        {
          if (StreamTypeHelper.IsVideoStream(es.LogicalStreamType) ||
            StreamTypeHelper.IsAudioStream(es.LogicalStreamType))
          {
            _pids.Add(es.Pid);
            _tsFilterInterface.AnalyserAddPid(_subChannelIndex, es.Pid);
          }
          else if (es.LogicalStreamType == LogicalStreamType.Subtitles ||
            es.LogicalStreamType == LogicalStreamType.Teletext)
          {
            _pids.Add(es.Pid);
          }
        }

        if (_pmt.PcrPid > 0 && !_pids.Contains(_pmt.PcrPid))
        {
          _pids.Add(_pmt.PcrPid);
        }
        // TODO: fix this so that tuners with PID filtering can use MDAPI.
        /*if (_mdplugs != null)
        {
          // MDPlugins Active.. 
          // It's important that the ECM pidSet are not blocked by the HWPID Filter in the Tuner.
          // therefore they need to be explicitly added to HWPID list.

          // HWPIDS supports max number of 16 filtered pidSet - Total max of 16.
          // ECM Pids
          foreach (ECMEMM ecmValue in _channelInfo.caPMT.GetECM())
          {
            if (ecmValue.Pid != 0 && !hwPids.Contains(ecmValue.Pid))
            {
              hwPids.Add(ecmValue.Pid);
            }
          }
          //EMM Pids
          foreach (ECMEMM emmValue in _channelInfo.caPMT.GetEMM())
          {
            if (emmValue.Pid != 0 && !hwPids.Contains(emmValue.Pid))
            {
              hwPids.Add(emmValue.Pid);
            }
          }
          this.LogDebug("Number of HWPIDS that needs to be sent to tuner :{0} ", hwPids.Count);
        }*/


      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    /// <summary>
    /// Sets the pidSet for the timeshifter
    /// </summary>
    private void SetTimeShiftPids()
    {
      try
      {
        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        _tsFilterInterface.TimeShiftSetPmtPid(_subChannelIndex, _pmtPid, _pmt.ProgramNumber, rawPmt, rawPmt.Length);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MPEG 2 sub-channel: failed to set timeshifter PIDs");
      }
    }

    /// <summary>
    /// Sets the pidSet for the recorder
    /// </summary>
    private void SetRecorderPids()
    {
      try
      {
        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        _tsFilterInterface.RecordSetPmtPid(_subChannelIndex, _pmtPid, _pmt.ProgramNumber, rawPmt, rawPmt.Length);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MPEG 2 sub-channel: failed to set recorder PIDs");
      }
    }

    /// <summary>
    /// Decode and process PMT data received from TsWriter.
    /// </summary>
    private bool HandlePmt()
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} handle PMT", _subChannelId);

      if (_currentChannel == null)
      {
        this.LogDebug("MPEG 2 sub-channel: current channel is not set");
        return false;
      }

      IntPtr pmtBuffer = Marshal.AllocCoTaskMem(Pmt.MAX_SIZE);
      try
      {
        int pmtLength = _tsFilterInterface.PmtGetPmtData(_subChannelIndex, pmtBuffer);
        byte[] pmtData = new byte[pmtLength];
        Marshal.Copy(pmtBuffer, pmtData, 0, pmtLength);
        Pmt pmt = Pmt.Decode(pmtData, _tuner.CamType);
        if (pmt == null)
        {
          this.LogDebug("MPEG 2 sub-channel: invalid PMT detected");
          return false;
        }

        this.LogDebug("MPEG 2 sub-channel: service ID = {0}, PMT PID = {1}, version = {2}",
                        pmt.ProgramNumber, _pmtPid, pmt.Version);

        // Have we already seen this PMT? If yes, then stop processing here. Theoretically this is a
        // redundant check as TsWriter should only pass us new PMT when the version changes.
        if (_pmt != null && _pmt.Version == pmt.Version)
        {
          return false;
        }
        this.LogDebug("MPEG 2 sub-channel: new PMT version");
        _pmt = pmt;

        // Attempt to grab the CAT if the service is encrypted. Note that we trust the setting on the
        // channel because we are not currently able to detect elementary stream level encryption. Better
        // to allow the user to do what they want - "user knows best".
        if (!_currentChannel.FreeToAir)
        {
          //TODO fix this
          GrabCat();
        }

        // TODO: call _tuner.OnPmtReady().

        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MPEG 2 sub-channel: caught exception while handling PMT");
      }
      finally
      {
        Marshal.FreeCoTaskMem(pmtBuffer);
      }
      return false;
    }

    /// <summary>
    /// Attempt to retrieve CAT data from TsWriter.
    /// </summary>
    private void GrabCat()
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} grab CAT", _subChannelId);
      IntPtr catBuffer = Marshal.AllocCoTaskMem(Cat.MAX_SIZE);
      try
      {
        DateTime dtNow = DateTime.Now;
        _eventCat.Reset();
        _tsFilterInterface.CaSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.CaReset(_subChannelIndex);
        bool found = _eventCat.WaitOne(_parameters.TimeOutCAT * 1000, true);
        ThrowExceptionIfTuneCancelled();
        TimeSpan ts = DateTime.Now - dtNow;
        if (!found)
        {
          this.LogDebug("MPEG 2 sub-channel: CAT not found after {0} seconds", ts.TotalSeconds);
          return;
        }
        this.LogDebug("MPEG 2 sub-channel: CAT found after {0} seconds", ts.TotalSeconds);
        int catLength = _tsFilterInterface.CaGetCaData(_subChannelIndex, catBuffer);
        byte[] catData = new byte[catLength];
        Marshal.Copy(catBuffer, catData, 0, catLength);
        _cat = Cat.Decode(catData);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MPEG 2 sub-channel: caught exception while grabbing CAT");
      }
      finally
      {
        Marshal.FreeCoTaskMem(catBuffer);
      }
    }

    #endregion

    #region properties

    /// <summary>
    /// Fetch stream quality information from TsWriter.
    /// </summary>   
    /// <param name="totalBytes">The number of packets processed.</param>    
    /// <param name="discontinuityCounter">The number of stream discontinuities.</param>
    public void GetStreamQualityCounters(out int totalBytes, out int discontinuityCounter)
    {
      discontinuityCounter = 0;
      totalBytes = 0;

      int totalTsBytes = 0;
      int tsDiscontinuity = 0;
      int totalRecordingBytes = 0;
      int recordingDiscontinuity = 0;

      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.GetStreamQualityCounters(_subChannelId, out totalTsBytes, out totalRecordingBytes,
                                                    out tsDiscontinuity, out recordingDiscontinuity);
      }

      if (IsRecording)
      {
        totalBytes = totalRecordingBytes;
        discontinuityCounter = recordingDiscontinuity;
      }
      else if (IsTimeShifting)
      {
        totalBytes = totalTsBytes;
        discontinuityCounter = tsDiscontinuity;
      }
    }

    #endregion

    #region TsWriter call back handlers

    #region ICaCallBack members

    /// <summary>
    /// Called when tswriter.ax has received a new ca section
    /// </summary>
    /// <returns></returns>
    public int OnCaReceived()
    {
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} OnCaReceived()", _subChannelId);
      if (_eventCat != null)
      {
        _eventCat.Set();
      }
      return 0;
    }

    #endregion

    #region IPmtCallBack member

    /// <summary>
    /// Called by TsWriter when:
    /// - a new PMT section for the current service is received
    /// - the PMT PID for the current service changes
    /// - the service ID for the current service changes
    /// </summary>
    /// <param name="pmtPid">The PID of the elementary stream from which the PMT section received.</param>
    /// <param name="serviceId">The ID associated with the service which the PMT section is associated with.</param>
    /// <param name="isServiceRunning">Indicates whether the service that the grabber is monitoring is active.
    ///   The grabber will not wait for PMT to be received if it thinks the service is not running.</param>
    /// <returns>an HRESULT indicating whether the PMT section was successfully handled</returns>
    public int OnPmtReceived(int pmtPid, int serviceId, bool isServiceRunning)
    {
      this.LogDebug("MPEG 2 sub-channel: subchannel {0} OnPmtReceived(), PMT PID = {1}, service ID = {2}, is service running = {3}, dynamic = {4}",
          _subChannelId, pmtPid, serviceId, isServiceRunning, _pmt != null);
      _pmtPid = pmtPid;
      _isServiceRunning = isServiceRunning;
      if (_eventPmt != null)
      {
        _eventPmt.Set();
      }
      // Was the PMT requested? If not, we are responsible for handling it.
      if (_pmt != null)
      {
        if (HandlePmt())
        {
          BuildPidList();
          if (IsTimeShifting)
          {
            SetTimeShiftPids();
          }
          if (IsRecording)
          {
            SetRecorderPids();
          }
        }
      }
      PersistPmtPid(pmtPid);
      return 0;
    }

    /// <summary>
    /// Update the database with the service's current PMT PID.
    /// </summary>
    /// <param name="pmtPid">The service's current PMT PID.</param>
    private void PersistPmtPid(int pmtPid)
    {
      // We perform the update if:
      // - the PID has changed AND
      // - the PID has not been set to zero (setting to zero indicates that the user wants the PID to be
      //    looked up in the PAT each time this service is tuned)
      DVBBaseChannel dvbService = _currentChannel as DVBBaseChannel;
      if (dvbService != null && pmtPid != dvbService.PmtPid && dvbService.PmtPid != 0)
      {
        dvbService.PmtPid = pmtPid;   // Set the value here so we don't hammer this function, regardless of update success/fail.

        TuningDetail currentDetail = ChannelManagement.GetTuningDetail(dvbService);
        if (currentDetail != null)
        {
          try
          {
            int oldPid = currentDetail.PmtPid;
            currentDetail.PmtPid = pmtPid;
            ChannelManagement.SaveTuningDetail(currentDetail);
            this.LogDebug("MPEG 2 sub-channel: updated PMT PID for service {0} from {1} to {2}",
                            dvbService.ServiceId, oldPid, pmtPid);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "MPEG 2 sub-channel: failed to persist new PMT PID for service {0}", dvbService.ServiceId);
          }
        }
        else
        {
          this.LogDebug("MPEG 2 sub-channel: unable to persist new PMT PID for service {0}", dvbService.ServiceId);
        }
      }
    }

    #endregion

    #endregion
  }
}