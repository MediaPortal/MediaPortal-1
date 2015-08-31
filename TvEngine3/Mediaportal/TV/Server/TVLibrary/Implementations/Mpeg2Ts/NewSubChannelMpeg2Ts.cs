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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts
{
  /// <summary>
  /// An <see cref="ISubChannel"/> implementation for MPEG 2 transport stream sub-channels (programs).
  /// </summary>
  internal class NewSubChannelMpeg2Ts //TODO : SubChannelBase, IPmtCallBack, ICaCallBack, IVideoAudioObserver
  {
    #region constants

    private const ushort PID_PAT = 0;
    private const ushort PID_CAT = 1;

    #endregion

    #region variables

    #region local variables

    /// <summary>
    /// The current PMT PID for the program that this sub-channel represents.
    /// </summary>
    private int _pmtPid = -1;

    /// <summary>
    /// Set by the TsWriter OnPmtReceived() call back. Indicates whether the
    /// program that this sub-channel represents is currently active.
    /// </summary>
    private bool _isProgramRunning = false;

    /// <summary>
    /// Ts filter instance
    /// </summary>
    private ITsFilter _tsFilterInterface;

    /// <summary>
    /// The handle that links this sub-channel with a corresponding sub-channel instance in TsWriter.
    /// </summary>
    private int _tsFilterHandle = -1;

    private TableProgramMap _pmt;
    private TableConditionalAccess _cat;
    private ISet<ushort> _pids = new HashSet<ushort>();
    private bool _isConditionalAccessTableRequired = false;

    private SubChannelManagerMpeg2Ts _manager = null;

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
    /// Initialise a new instance of the <see cref="SubChannelMpeg2Ts"/> class.
    /// </summary>
    /// <param name="subChannelId">The sub-channel ID to associate with this instance.</param>
    /// <param name="tsFilter">The TS filter instance used to perform/implement timeshifting and recording.</param>
    public NewSubChannelMpeg2Ts(SubChannelManagerMpeg2Ts manager, int subChannelId, ITsFilter tsFilter, bool isConditionalAccessTableRequired)
      //TODO: base(subChannelId)
    {
      _manager = manager;
      _tsFilterInterface = tsFilter;
      _isConditionalAccessTableRequired = isConditionalAccessTableRequired;
      _eventPmt = new ManualResetEvent(false);
      _eventCat = new ManualResetEvent(false);

      _tsFilterInterface.AddChannel(out _tsFilterHandle);
      //TODO this.LogDebug("MPEG 2 sub-channel: new sub-channel {0} index {1}", _subChannelId, _tsFilterHandle);

      //TODO ReloadConfiguration();
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~NewSubChannelMpeg2Ts()
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

    /*#region tuning and graph methods

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public override void OnAfterTune()
    {
      this.LogDebug("MPEG 2 sub-channel: {0} reset", _subChannelId);

      // Pass the core PIDs to the tuner's hardware PID filter so that we can
      // do basic tuning and scanning.
      HashSet<ushort> pids = new HashSet<ushort>();
      pids.Add(PID_PAT);         // PAT - for program lookup

      // Include the CAT PID when the program needs to be decrypted.
      if (_isConditionalAccessTableRequired && _currentChannel.IsEncrypted)
      {
        pids.Add(PID_CAT);
      }

      ChannelMpeg2Base mpeg2Channel = _currentChannel as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        // Include the PMT PID if we know it. We don't know what the PMT PID is
        // when scanning.
        if (mpeg2Channel.PmtPid > 0)
        {
          pids.Add((ushort)mpeg2Channel.PmtPid);
        }

        if (_currentChannel is ChannelAtsc || _currentChannel is ChannelScte)
        {
          pids.Add(0x1ffb);  // ATSC VCT - for terrestrial service info
          pids.Add(0x1ffc);  // SCTE VCT - for cable service info
        }
        else
        {
          pids.Add(0x10);    // DVB NIT - for network info
          pids.Add(0x11);    // DVB SDT, BAT - for service info
        }
      }

      UpdatePidFilters(pids);
    }

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// </summary>
    public override void OnGraphRunning()
    {
      this.LogDebug("MPEG 2 sub-channel: {0} tune", _subChannelId);

      // There 3 classes of program number settings:
      // -1 = Scanning behaviour, where we don't care about PMT.
      // 0 = The program is expected to be the only program in the transport
      //      stream. TsWriter should take the first program that it sees and
      //      grab the associated PMT sections.
      // <anything else> = A valid program number for the program that we are
      //                    trying to tune. TsWriter should grab the associated
      //                    PMT sections.

      // There are also 3 classes of PMT PID settings:
      // -1 = Scanning behaviour, where we don't care about PMT.
      // 0 = We don't know the correct/current PMT PID, so we ask TsWriter to
      //      determine what it should be, and then grab the associated PMT
      //      sections.
      // <anything else> = A valid PMT PID for the program that we are trying
      //                    to tune. TsWriter should grab the associated PMT
      //                    sections.
      int programNumber = 0;
      int pmtPid = 0;
      ChannelMpeg2Base mpeg2Channel = _currentChannel as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        programNumber = mpeg2Channel.ProgramNumber;
        pmtPid = mpeg2Channel.PmtPid;
      }
      else if (string.IsNullOrEmpty(_currentChannel.Name))
      {
        // TODO When the channel name is not set we must be scanning analog. What a terrible hack!!!
        programNumber = -1;
        pmtPid = -1;
      }

      if (pmtPid < 0)
      {
        _pmt = null;
        return;
      }

      OnAfterTuneEvent();
      GrabPmt(programNumber, pmtPid);
      try
      {
        HandlePmt();
      }
      catch (Exception ex)
      {
        throw new TvException(ex, "Failed to handle PMT.");
      }

      // Attempt to grab the CAT if the program is encrypted. Note that we
      // trust the setting on the channel because we are not currently able to
      // detect elementary stream level encryption. Better to allow the user to
      // do what they want - "user knows best".
      if (_currentChannel.IsEncrypted && _isConditionalAccessTableRequired)
      {
        GrabCat();
        IntPtr catBuffer = Marshal.AllocCoTaskMem(TableConditionalAccess.MAX_SIZE);
        try
        {
          int catLength = _tsFilterInterface.CaGetCaData(_tsFilterHandle, catBuffer);
          byte[] catData = new byte[catLength];
          Marshal.Copy(catBuffer, catData, 0, catLength);
          _cat = TableConditionalAccess.Decode(catData);
        }
        finally
        {
          Marshal.FreeCoTaskMem(catBuffer);
        }
      }
      else
      {
        _cat = null;
      }
    }

    #region utility

    private void UpdatePidFilters(HashSet<ushort> pids)
    {
      bool needApply = false;

      HashSet<ushort> newPids = new HashSet<ushort>(pids);
      newPids.ExceptWith(_pids);
      if (newPids.Count > 0)
      {
        _manager.AllowStreams(_subChannelId, newPids);
        needApply = true;
      }

      HashSet<ushort> oldPids = new HashSet<ushort>(_pids);
      oldPids.ExceptWith(pids);
      if (oldPids.Count > 0)
      {
        _manager.BlockStreams(_subChannelId, oldPids);
        needApply = true;
      }

      if (needApply)
      {
        _manager.ApplyConfiguration(_subChannelId);
        _pids = pids;
      }
    }

    /// <summary>
    /// Wait for TsWriter to find PMT in the transport stream.
    /// </summary>
    /// <param name="programNumber">The program number of the program being tuned.</param>
    /// <param name="pmtPid">The PMT PID of the program being tuned.</param>
    private void GrabPmt(int programNumber, int pmtPid)
    {
      this.LogDebug("MPEG 2 sub-channel: {0} grab PMT, program number = {1}, PMT PID = {2}", _subChannelId, programNumber, pmtPid);

      int pmtPidToSearchFor;
      if (pmtPid < 0)
      {
        pmtPidToSearchFor = 0;
      }
      else
      {
        pmtPidToSearchFor = pmtPid;
      }

      bool pmtFound = false;
      TimeSpan waitedTime = TimeSpan.MinValue;
      while (!pmtFound)
      {
        this.LogDebug("MPEG 2 sub-channel: configure PMT grabber, PMT PID = {0}", pmtPidToSearchFor);
        int hr = _tsFilterInterface.PmtSetCallBack(_tsFilterHandle, this);
        hr |= _tsFilterInterface.PmtSetPmtPid(_tsFilterHandle, pmtPidToSearchFor, programNumber);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          throw new TvException("Failed to configure TS filter PMT grabber for handle {0}, PMT PID = {2}, program number = {2}.", _tsFilterHandle, pmtPidToSearchFor, programNumber);
        }

        // Do this as late as possible. Any PMT that arrives between when the
        // PMT call back was set and when we start waiting for PMT will cause
        // us to miss or mess up the PMT handling.
        _pmtPid = -1;
        _isProgramRunning = false;
        _pmt = null;
        _eventPmt.Reset();
        DateTime dtStartWait = DateTime.Now;
        ThrowExceptionIfTuneCancelled();
        pmtFound = _eventPmt.WaitOne(5000, true);
        ThrowExceptionIfTuneCancelled();
        waitedTime = DateTime.Now - dtStartWait;
        if (!pmtFound)
        {
          this.LogWarn("MPEG 2 sub-channel: timed out waiting for PMT after {0} ms", waitedTime.TotalMilliseconds);
          // One retry allowed...
          if (pmtPidToSearchFor == 0)
          {
            this.LogError("MPEG 2 sub-channel: giving up waiting for PMT, you might need to increase the PMT timeout setting");
            throw new TvExceptionServiceNotFound(_currentChannel);
          }
          else
          {
            pmtPidToSearchFor = 0;
          }
        }
      }

      if (!_isProgramRunning)
      {
        throw new TvExceptionServiceNotRunning(_currentChannel);
      }

      this.LogDebug("MPEG 2 sub-channel: found PMT after {0} ms", waitedTime.TotalMilliseconds);
    }

    private void GrabCat()
    {
      this.LogDebug("MPEG 2 sub-channel: {0} grab CAT", _subChannelId);
      int hr = _tsFilterInterface.CaSetCallBack(_tsFilterHandle, this);
      hr |= _tsFilterInterface.CaReset(_tsFilterHandle);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        throw new TvException("Failed to configure TS filter CAT grabber for handle {0}.", _tsFilterHandle);
      }

      _cat = null;
      _eventCat.Reset();
      DateTime dtNow = DateTime.Now;
      ThrowExceptionIfTuneCancelled();
      bool found = _eventCat.WaitOne(5000, true);
      ThrowExceptionIfTuneCancelled();
      TimeSpan ts = DateTime.Now - dtNow;
      if (!found)
      {
        this.LogError("MPEG 2 sub-channel: timed out waiting for CAT after {0} ms, you might need to increase the CAT timeout setting", ts.TotalMilliseconds);
        throw new TvExceptionServiceNotFound(_currentChannel);
      }
      this.LogDebug("MPEG 2 sub-channel: found CAT after {0} ms", ts.TotalMilliseconds);
    }

    private void UpdatePidList()
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: {0} update PID list", _subChannelId);

      HashSet<ushort> pids = new HashSet<ushort>();
      pids.Add(PID_PAT);   // for program monitoring

      // Include the PMT PID to handle elementary stream and conditional
      // access changes.
      if (_pmtPid > 0)
      {
        pids.Add((ushort)_pmtPid);
      }

      // Include video, audio, subtitles and teletext PIDs.
      foreach (PmtElementaryStream es in _pmt.ElementaryStreams)
      {
        if (StreamTypeHelper.IsVideoStream(es.LogicalStreamType) ||
          StreamTypeHelper.IsAudioStream(es.LogicalStreamType))
        {
          pids.Add(es.Pid);
          _tsFilterInterface.AnalyserAddPid(_tsFilterHandle, es.Pid);
        }
        else if (es.LogicalStreamType == LogicalStreamType.Subtitles ||
          es.LogicalStreamType == LogicalStreamType.Teletext)
        {
          pids.Add(es.Pid);
        }
      }

      // Include the PCR PID, if valid and not already included. Often the
      // primary video or audio PID doubles as the PCR PID.
      if (_pmt.PcrPid > 0 && _pmt.PcrPid != 0x1fff)
      {
        pids.Add(_pmt.PcrPid);
      }

      // Include the CAT, ECM and EMM PIDs when the program needs to be
      // decrypted and the conditional access provider(s) require it.
      if (_isConditionalAccessTableRequired && _cat != null)
      {
        pids.Add(PID_CAT);

        IEnumerator<IDescriptor> descEn = _pmt.ProgramCaDescriptors.GetEnumerator();
        while (descEn.MoveNext())
        {
          ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
          if (cad == null)
          {
            this.LogError("MPEG 2 sub-channel: invalid PMT program CA descriptor");
            ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
            Dump.DumpBinary(rawDescriptor);
            continue;
          }
          pids.UnionWith(cad.Pids.Keys);
        }

        IEnumerator<PmtElementaryStream> esEn = _pmt.ElementaryStreams.GetEnumerator();
        while (esEn.MoveNext())
        {
          descEn = esEn.Current.CaDescriptors.GetEnumerator();
          while (descEn.MoveNext())
          {
            ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
            if (cad == null)
            {
              this.LogError("MPEG 2 sub-channel: invalid PMT ES CA descriptor");
              ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
              Dump.DumpBinary(rawDescriptor);
              continue;
            }
            pids.UnionWith(cad.Pids.Keys);
          }
        }

        descEn = _cat.CaDescriptors.GetEnumerator();
        while (descEn.MoveNext())
        {
          ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
          if (cad == null)
          {
            this.LogError("MPEG 2 sub-channel: invalid CAT CA descriptor");
            ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
            Dump.DumpBinary(rawDescriptor);
            continue;
          }
          pids.UnionWith(cad.Pids.Keys);
        }
      }

      UpdatePidFilters(pids);
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
        int hr = _tsFilterInterface.RecordSetRecordingFileNameW(_tsFilterHandle, fileName);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("subch:{0} SetRecordingFileName failed:{1:X}", _subChannelId, hr);
        }
        this.LogDebug("subch:{0}-{1} tswriter StartRecording...", _subChannelId, _tsFilterHandle);
        SetRecorderPids(false);

        this.LogDebug("Set video / audio observer");
        _tsFilterInterface.RecorderSetVideoAudioObserver(_tsFilterHandle, this);

        hr = _tsFilterInterface.RecordStartRecord(_tsFilterHandle);
        if (hr != (int)NativeMethods.HResult.S_OK)
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
      this.LogDebug("tvdvbchannel.OnStopRecording subch={0}, subch index={1}", _subChannelId, _tsFilterHandle);
      if (IsRecording)
      {
        if (_tsFilterInterface != null)
        {
          this.LogDebug("tvdvbchannel.OnStopRecording subch:{0}-{1} tswriter StopRecording...", _subChannelId,
                            _tsFilterHandle);
          _tsFilterInterface.RecordStopRecord(_tsFilterHandle);
        }
      }
      else
      {
        this.LogWarn("tvdvbchannel.OnStopRecording - not recording");
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
        _tsFilterInterface.SetVideoAudioObserver(_tsFilterHandle, this);
        _tsFilterInterface.TimeShiftSetParams(_tsFilterHandle, _timeShiftFileCountMinimum, _timeShiftFileCountMaximum, _timeShiftFileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileNameW(_tsFilterHandle, fileName);

        if (CurrentChannel == null)
        {
          this.LogError("CurrentChannel is null when trying to start timeshifting");
          throw new TvException("MPEG 2 sub-channel: current channel is null");
        }

        this.LogDebug("subch:{0} SetTimeShiftFileName fill in pids", _subChannelId);
        SetTimeShiftPids(false);
        this.LogDebug("subch:{0}-{1} tswriter StartTimeshifting...", _subChannelId, _tsFilterHandle);
        int hr = _tsFilterInterface.TimeShiftStart(_tsFilterHandle);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("subch:{0} tswriter StartTimeShift failed:{1:X}", _subChannelId, hr);
        }
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
        this.LogDebug("subch:{0}-{1} tswriter StopTimeshifting...", _subChannelId, _tsFilterHandle);
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.TimeShiftStop(_tsFilterHandle);
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
      this.LogDebug("MPEG 2 sub-channel: {0} cancel tune", _subChannelId);
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
        return ((audioEncrypted == 0) && (videoEncrypted == 0));*//*
      }
    }

    #region OnDecompose

    /// <summary>
    /// disposes this channel
    /// </summary>
    protected override void OnDecompose()
    {
      if (_tsFilterInterface != null && _tsFilterHandle >= 0)
      {
        _tsFilterInterface.DeleteChannel(_tsFilterHandle);
        _tsFilterHandle = -1;
      }

      if (_pids != null && _pids.Count > 0)
      {
        _manager.BlockStreams(_subChannelId, _pids);
        _pids.Clear();
        _manager.ApplyConfiguration(_subChannelId);
      }
      _manager.OnPmtChanged(_subChannelId, null);
    }

    #endregion

    #region pidmapping



    /// <summary>
    /// Sets the pidSet for the timeshifter
    /// </summary>
    private void SetTimeShiftPids(bool isDynamicPmtChange)
    {
      try
      {
        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        _tsFilterInterface.TimeShiftSetPmtPid(_tsFilterHandle, _pmtPid, _pmt.ProgramNumber, rawPmt, rawPmt.Length, isDynamicPmtChange);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MPEG 2 sub-channel: failed to set timeshifter PIDs");
      }
    }

    /// <summary>
    /// Sets the pidSet for the recorder
    /// </summary>
    private void SetRecorderPids(bool isDynamicPmtChange)
    {
      try
      {
        ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
        byte[] rawPmt = new byte[readOnlyPmt.Count];
        readOnlyPmt.CopyTo(rawPmt, 0);
        _tsFilterInterface.RecordSetPmtPid(_tsFilterHandle, _pmtPid, _pmt.ProgramNumber, rawPmt, rawPmt.Length, isDynamicPmtChange);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MPEG 2 sub-channel: failed to set recorder PIDs");
      }
    }

    /// <summary>
    /// Decode and process PMT data received from TsWriter.
    /// </summary>
    private void HandlePmt()
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: {0} handle PMT", _subChannelId);

      IntPtr pmtBuffer = Marshal.AllocCoTaskMem(TableProgramMap.MAX_SIZE);
      try
      {
        int pmtLength = _tsFilterInterface.PmtGetPmtData(_tsFilterHandle, pmtBuffer);
        byte[] pmtData = new byte[pmtLength];
        Marshal.Copy(pmtBuffer, pmtData, 0, pmtLength);
        TableProgramMap pmt = TableProgramMap.Decode(pmtData);
        if (pmt == null)
        {
          throw new TvException("Invalid PMT detected.");
        }

        this.LogDebug("MPEG 2 sub-channel: program number = {0}, PMT PID = {1}, version = {2}",
                        pmt.ProgramNumber, _pmtPid, pmt.Version);

        // Have we already seen this PMT? If yes, then stop processing here.
        // Theoretically this is a redundant check as TsWriter should only pass
        // us new PMT when the version changes.
        bool isDynamicPmtChange = _pmt != null;
        if (isDynamicPmtChange && _pmt.Version == pmt.Version)
        {
          return;
        }
        this.LogDebug("MPEG 2 sub-channel: new PMT version");
        _pmt = pmt;

        UpdatePidList();
        if (IsTimeShifting)
        {
          SetTimeShiftPids(isDynamicPmtChange);
        }
        if (IsRecording)
        {
          SetRecorderPids(isDynamicPmtChange);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(pmtBuffer);
      }
    }

    #endregion

    #region properties

    /// <summary>
    /// Fetch stream quality information from TsWriter.
    /// </summary>   
    /// <param name="totalBytes">The number of packets processed.</param>    
    /// <param name="discontinuityCounter">The number of stream discontinuities.</param>
    public override void GetStreamQualityCounters(out int totalBytes, out int discontinuityCounter)
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
      this.LogDebug("MPEG 2 sub-channel: {0} OnCaReceived()", _subChannelId);
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
    /// - a new PMT section for the current program is received
    /// - the PMT PID for the current program changes
    /// - the program number for the current program changes
    /// </summary>
    /// <param name="pmtPid">The PID of the elementary stream from which the PMT section was received.</param>
    /// <param name="programNumber">The identifier associated with the program which the PMT section is associated with.</param>
    /// <param name="isProgramRunning">Indicates whether the program that the grabber is monitoring is active.
    ///   The grabber will not wait for PMT to be received if it thinks the program is not running.</param>
    /// <returns>an HRESULT indicating whether the PMT section was successfully handled</returns>
    public int OnPmtReceived(int pmtPid, int programNumber, bool isProgramRunning)
    {
      this.LogDebug("MPEG 2 sub-channel: {0} OnPmtReceived(), PMT PID = {1}, program number = {2}, is program running = {3}, dynamic = {4}",
          _subChannelId, pmtPid, programNumber, isProgramRunning, _pmt != null);
      _pmtPid = pmtPid;
      _isProgramRunning = isProgramRunning;
      if (_eventPmt != null)
      {
        _eventPmt.Set();
      }
      // Was the PMT requested? If not, we are responsible for handling it.
      if (_pmt != null)
      {
        try
        {
          HandlePmt();
        }
        catch (Exception ex)
        {
          this.LogError(ex, "MPEG 2 sub-channel: caught exception while handling PMT");
        }
      }
      return 0;
    }

    #endregion

    #endregion*/
  }
}