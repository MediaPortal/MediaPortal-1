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
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts
{
  /// <summary>
  /// An <see cref="ISubChannel"/> implementation for MPEG 2 transport stream sub-channels (programs).
  /// </summary>
  internal class SubChannelMpeg2Ts : SubChannelBase, IPmtCallBack, ICaCallBack, IChannelObserver
  {
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
    private ITsFilter _tsFilterInterface = null;

    /// <summary>
    /// Lock that must be acquired to access the TS filter interface.
    /// </summary>
    private object _lockTsfi = new object();

    /// <summary>
    /// The handle that links this sub-channel with a corresponding sub-channel instance in TsWriter.
    /// </summary>
    private int _subChannelIndex = -1;

    private TableProgramMap _pmt = null;
    private TableConditionalAccess _cat = null;
    private ISet<ushort> _pids = null;
    private bool _isConditionalAccessTableRequired = false;

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
    /// <param name="tsWriter">The TsWriter filter instance used to perform/implement timeshifting and recording.</param>
    public SubChannelMpeg2Ts(int subChannelId, ITsFilter tsWriter)
      : base(subChannelId)
    {
      _eventPmt = new ManualResetEvent(false);
      _eventCat = new ManualResetEvent(false);

      _tsFilterInterface = tsWriter;
      _tsFilterInterface.AddChannel(out _subChannelIndex);
      this.LogDebug("MPEG 2 sub-channel: new sub-channel {0} index {1}", _subChannelId, _subChannelIndex);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~SubChannelMpeg2Ts()
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

    public ICollection<ushort> Pids
    {
      get
      {
        return _pids;
      }
    }

    public TableProgramMap ProgramMapTable
    {
      get
      {
        return _pmt;
      }
    }

    public TableConditionalAccess ConditionalAccessTable
    {
      get
      {
        return _cat;
      }
    }

    public bool IsConditionalAccessTableRequired
    {
      get
      {
        return _isConditionalAccessTableRequired;
      }
      set
      {
        _isConditionalAccessTableRequired = value;
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

      // Pass the core PIDs to the tuner's hardware PID filter so that we can
      // do basic tuning and scanning.
      _pids = new HashSet<ushort>();
      _pids.Add(0x0);         // PAT - for program lookup

      // Include the CAT PID when the program needs to be decrypted.
      if (_isConditionalAccessTableRequired && _currentChannel.IsEncrypted)
      {
        _pids.Add(0x1);
      }

      ChannelMpeg2Base mpeg2Channel = _currentChannel as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        // Include the PMT PID if we know it. We don't know what the PMT PID is
        // when scanning.
        if (mpeg2Channel.PmtPid > 0)
        {
          _pids.Add((ushort)mpeg2Channel.PmtPid);
        }

        if (_currentChannel is ChannelAtsc || _currentChannel is ChannelScte)
        {
          _pids.Add(0x1ffb);  // ATSC VCT - for terrestrial service info
          _pids.Add(0x1ffc);  // SCTE VCT - for cable service info
        }
        else
        {
          _pids.Add(0x10);    // DVB NIT - for network info
          _pids.Add(0x11);    // DVB SDT, BAT - for service info
        }
      }
    }

    /// <summary>
    /// Wait for TsWriter to find PMT in the transport stream.
    /// </summary>
    /// <param name="programNumber">The program number of the program being tuned.</param>
    /// <param name="pmtPid">The PMT PID of the program being tuned.</param>
    private void WaitForPmt(int programNumber, int pmtPid)
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} wait for PMT, program number = {1}, PMT PID = {2}", _subChannelId, programNumber, pmtPid);

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
      if (programNumber < 0)
      {
        return;
      }

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
      TimeSpan waitLength = TimeSpan.MinValue;
      while (!pmtFound)
      {
        this.LogDebug("MPEG 2 sub-channel: configure PMT grabber, PMT PID = {0}", pmtPidToSearchFor);
        lock (_lockTsfi)
        {
          if (_tsFilterInterface == null)
          {
            throw new TvExceptionTuneCancelled();
          }
          _tsFilterInterface.PmtSetCallBack(_subChannelIndex, this);
          _tsFilterInterface.PmtSetPmtPid(_subChannelIndex, pmtPidToSearchFor, programNumber);
        }

        OnAfterTuneEvent();

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
        waitLength = DateTime.Now - dtStartWait;
        if (!pmtFound)
        {
          this.LogWarn("MPEG 2 sub-channel: timed out waiting for PMT after {0} seconds", waitLength.TotalSeconds);
          // One retry allowed...
          if (pmtPidToSearchFor == 0)
          {
            this.LogError("MPEG 2 sub-channel: giving up waiting for PMT - you might need to increase the PMT timeout");
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

      this.LogDebug("MPEG 2 sub-channel: found PMT after {0} seconds", waitLength.TotalSeconds);
      try
      {
        HandlePmt();
      }
      catch (TvExceptionTuneCancelled)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new TvException(ex, "Failed to handle PMT.");
      }
    }

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// </summary>
    public override void OnGraphRunning()
    {
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} OnGraphRunning()", _subChannelId);

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
      WaitForPmt(programNumber, pmtPid);
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
      lock (_lockTsfi)
      {
        if (_tsFilterInterface == null)
        {
          this.LogWarn("subch:{0} failed to start recording, TS filter interface is null", _subChannelId);
          return;
        }

        int hr = _tsFilterInterface.RecordSetRecordingFileNameW(_subChannelIndex, fileName);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("subch:{0} SetRecordingFileName failed:{1:X}", _subChannelId, hr);
        }
        this.LogDebug("subch:{0}-{1} tswriter StartRecording...", _subChannelId, _subChannelIndex);
        SetRecorderPids(false);

        this.LogDebug("Set video / audio observer");
        _tsFilterInterface.RecorderSetVideoAudioObserver(_subChannelIndex, this);

        hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
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
      if (!IsRecording)
      {
        this.LogWarn("tvdvbchannel.OnStopRecording subch:{0}-{1} not recording", _subChannelId, _subChannelIndex);
        return;
      }

      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null)
        {
          this.LogDebug("tvdvbchannel.OnStopRecording subch:{0}-{1}...", _subChannelId, _subChannelIndex);
          _tsFilterInterface.RecordStopRecord(_subChannelIndex);
        }
      }
    }

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override void OnStartTimeShifting(string fileName, int fileCount, int fileCountMaximum, ulong fileSize)
    {
      this.LogDebug("subch:{0} StartTimeShift:{1}", _subChannelId, fileName);
      lock (_lockTsfi)
      {
        if (_tsFilterInterface == null)
        {
          this.LogWarn("subch:{0} failed to start timeshifting, TS filter interface is null", _subChannelId);
          return;
        }

        _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);
        _tsFilterInterface.TimeShiftSetParams(_subChannelIndex, fileCount, fileCountMaximum, (uint)fileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileNameW(_subChannelIndex, fileName);

        this.LogDebug("subch:{0} SetTimeShiftFileName fill in pids", _subChannelId);
        SetTimeShiftPids(false);
        this.LogDebug("subch:{0}-{1} tswriter StartTimeshifting...", _subChannelId, _subChannelIndex);
        int hr = _tsFilterInterface.TimeShiftStart(_subChannelIndex);
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
      if (!IsRecording)
      {
        this.LogWarn("tvdvbchannel.OnStopTimeShifting subch:{0}-{1} not time shifting", _subChannelId, _subChannelIndex);
        return;
      }

      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null)
        {
          this.LogDebug("tvdvbchannel.OnStopTimeShifting subch:{0}-{1}...", _subChannelId, _subChannelIndex);
          _tsFilterInterface.TimeShiftStop(_subChannelIndex);
        }
      }
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    protected override void OnGetTimeShiftFilePosition(out long position, out long bufferId)
    {
      position = 0;
      bufferId = 0;
      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.TimeShiftGetCurrentFilePosition(_subChannelIndex, out position, out bufferId);
        }
      }
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public override void CancelTune()
    {
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} cancel tune", _subChannelId);
      CancelTunePrivate();
    }

    private void CancelTunePrivate()
    {
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
      CancelTunePrivate();
      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null && _subChannelIndex >= 0)
        {
          _tsFilterInterface.DeleteChannel(_subChannelIndex);
          _subChannelIndex = -1;
          _tsFilterInterface = null;
        }
      }
    }

    #endregion

    #region pidmapping

    /// <summary>
    /// maps the correct pidSet to the TsFileSink filter and teletext pins
    /// </summary>
    private void BuildPidList()
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} build PID list", _subChannelId);
      if (_pmt == null)
      {
        this.LogError("MPEG 2 sub-channel: PMT not available");
        return;
      }

      _pids = new HashSet<ushort>();
      _pids.Add(0x0);   // PAT - for program monitoring

      // Include the PMT PID to handle elementary stream and conditional
      // access changes.
      if (_pmtPid > 0)
      {
        _pids.Add((ushort)_pmtPid);
      }

      // Include video, audio, subtitles and teletext PIDs.
      lock (_lockTsfi)
      {
        if (_tsFilterInterface == null)
        {
          throw new TvExceptionTuneCancelled();
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
      }

      // Include the PCR PID, if valid and not already included. Often the
      // primary video or audio PID doubles as the PCR PID.
      if (_pmt.PcrPid > 0 && _pmt.PcrPid != 0x1fff && !_pids.Contains(_pmt.PcrPid))
      {
        _pids.Add(_pmt.PcrPid);
      }

      // Include the CAT, ECM and EMM PID when the program needs to be
      // decrypted and the conditional access provider(s) require it.
      if (!_isConditionalAccessTableRequired || !_currentChannel.IsEncrypted)
      {
        return;
      }

      _pids.Add(0x1);
      if (_cat == null)
      {
        return;
      }

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
        _pids.UnionWith(cad.Pids.Keys);
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
          _pids.UnionWith(cad.Pids.Keys);
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
        _pids.UnionWith(cad.Pids.Keys);
      }
    }

    /// <summary>
    /// Sets the pidSet for the timeshifter
    /// </summary>
    private void SetTimeShiftPids(bool isDynamicPmtChange)
    {
      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null)
        {
          return;
        }

        try
        {
          ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
          byte[] rawPmt = new byte[readOnlyPmt.Count];
          readOnlyPmt.CopyTo(rawPmt, 0);
          _tsFilterInterface.TimeShiftSetPmtPid(_subChannelIndex, _pmtPid, _pmt.ProgramNumber, rawPmt, rawPmt.Length, isDynamicPmtChange);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "MPEG 2 sub-channel: failed to set timeshifter PIDs");
        }
      }
    }

    /// <summary>
    /// Sets the pidSet for the recorder
    /// </summary>
    private void SetRecorderPids(bool isDynamicPmtChange)
    {
      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null)
        {
          return;
        }

        try
        {
          ReadOnlyCollection<byte> readOnlyPmt = _pmt.GetRawPmt();
          byte[] rawPmt = new byte[readOnlyPmt.Count];
          readOnlyPmt.CopyTo(rawPmt, 0);
          _tsFilterInterface.RecordSetPmtPid(_subChannelIndex, _pmtPid, _pmt.ProgramNumber, rawPmt, rawPmt.Length, isDynamicPmtChange);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "MPEG 2 sub-channel: failed to set recorder PIDs");
        }
      }
    }

    /// <summary>
    /// Decode and process PMT data received from TsWriter.
    /// </summary>
    private void HandlePmt()
    {
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} handle PMT", _subChannelId);

      if (_currentChannel == null)
      {
        throw new TvException("Failed to handle PMT, current channel is not set.");
      }

      IntPtr pmtBuffer = Marshal.AllocCoTaskMem(TableProgramMap.MAX_SIZE);
      try
      {
        int pmtLength;
        lock (_lockTsfi)
        {
          if (_tsFilterInterface == null)
          {
            throw new TvExceptionTuneCancelled();
          }
          pmtLength = _tsFilterInterface.PmtGetPmtData(_subChannelIndex, pmtBuffer);
        }
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

        // Attempt to grab the CAT if the program is encrypted. Note that we
        // trust the setting on the channel because we are not currently able
        // to detect elementary stream level encryption. Better to allow the
        // user to do what they want - "user knows best".
        if (_currentChannel.IsEncrypted)
        {
          //TODO get this handling out of the OnPmtReceived() call back
          GrabCat();
        }

        // TODO: this code is hacked and needs a full and proper rework with good design
        BuildPidList();
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

    /// <summary>
    /// Attempt to retrieve CAT data from TsWriter.
    /// </summary>
    private void GrabCat()
    {
      _cat = null;
      if (!_isConditionalAccessTableRequired)
      {
        return;
      }
      ThrowExceptionIfTuneCancelled();
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} grab CAT", _subChannelId);
      IntPtr catBuffer = Marshal.AllocCoTaskMem(TableConditionalAccess.MAX_SIZE);
      try
      {
        DateTime dtNow = DateTime.Now;
        _eventCat.Reset();
        lock (_lockTsfi)
        {
          if (_tsFilterInterface == null)
          {
            throw new TvExceptionTuneCancelled();
          }
          _tsFilterInterface.CaSetCallBack(_subChannelIndex, this);
          _tsFilterInterface.CaReset(_subChannelIndex);
        }
        bool found = _eventCat.WaitOne(5000, true);
        ThrowExceptionIfTuneCancelled();
        TimeSpan ts = DateTime.Now - dtNow;
        if (!found)
        {
          this.LogDebug("MPEG 2 sub-channel: CAT not found after {0} seconds", ts.TotalSeconds);
          return;
        }

        this.LogDebug("MPEG 2 sub-channel: CAT found after {0} seconds", ts.TotalSeconds);
        int catLength;
        lock (_lockTsfi)
        {
          if (_tsFilterInterface == null)
          {
            throw new TvExceptionTuneCancelled();
          }
          catLength = _tsFilterInterface.CaGetCaData(_subChannelIndex, catBuffer);
        }
        byte[] catData = new byte[catLength];
        Marshal.Copy(catBuffer, catData, 0, catLength);
        _cat = TableConditionalAccess.Decode(catData);
      }
      catch (TvExceptionTuneCancelled)
      {
        throw;
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
    public override void GetStreamQualityCounters(out int totalBytes, out int discontinuityCounter)
    {
      discontinuityCounter = 0;
      totalBytes = 0;

      int totalTsBytes = 0;
      int tsDiscontinuity = 0;
      int totalRecordingBytes = 0;
      int recordingDiscontinuity = 0;

      lock (_lockTsfi)
      {
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.GetStreamQualityCounters(_subChannelIndex, out totalTsBytes, out totalRecordingBytes,
                                                      out tsDiscontinuity, out recordingDiscontinuity);
        }
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
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} OnCaReceived()", _subChannelId);
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
      this.LogDebug("MPEG 2 sub-channel: sub-channel {0} OnPmtReceived(), PMT PID = {1}, program number = {2}, is program running = {3}, dynamic = {4}",
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

    #region IChannelObserver member

    /// <summary>
    /// This function is invoked when the first unencrypted PES packet is received from a PID.
    /// </summary>
    /// <param name="pid">The PID that was seen.</param>
    /// <param name="pidType">The type of <paramref name="pid">PID</paramref>.</param>
    public void OnSeen(ushort pid, PidType pidType)
    {
      try
      {
        this.LogDebug("PID seen - type = {0}", pidType);
        OnAudioVideoEvent(pidType);
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    #endregion

    #endregion
  }
}