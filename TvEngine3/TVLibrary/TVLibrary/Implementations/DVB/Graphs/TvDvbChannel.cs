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
//using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Teletext;

namespace TvLibrary.Implementations.DVB
{
  ///<summary>
  /// A base class for digital services ("subchannels").
  ///</summary>
  public class TvDvbChannel : BaseSubChannel, ITeletextCallBack, IPMTCallback, ICACallback, ITvSubChannel,
                              IVideoAudioObserver
  {
    #region variables

    #region local variables

    /// <summary>
    /// PMT Pid
    /// </summary>
    private int _pmtPid = -1;

    /// <summary>
    /// Ts filter instance
    /// </summary>
    private ITsFilter _tsFilterInterface;

    /// <summary>
    /// SubChannel index
    /// </summary>
    private int _subChannelIndex = -1;

    /// <summary>
    /// Current audio stream
    /// </summary>
    private DVBAudioStream _currentAudioStream;

    /// set to true to enable PAT lookup of PMT
    private bool _alwaysLookupPmtPidInPat = DebugSettings.UsePATLookup;

    private Pmt _pmt;
    private Cat _cat;
    private List<UInt16> _pids;
    private TvCardBase _tuner;

    #endregion

    #region events

    /// <summary>
    /// Event that gets signaled when PMT arrives.
    /// </summary>
    protected ManualResetEvent _eventPmt;

    /// <summary>
    /// Event that gets signaled when CA arrives.
    /// </summary>
    private ManualResetEvent _eventCa;

    #endregion

    #region graph variables

    private readonly IBaseFilter _filterTif;

    #endregion

    #endregion

    #region ctor

    /// <summary>
    /// Initialise a new instance of the <see cref="TvDvbChannel"/> class.
    /// </summary>
    /// <param name="subChannelId">The subchannel ID to associate with this instance.</param>
    /// <param name="tuner">The tuner that this instance is associated with.</param>
    /// <param name="tsWriter">The TsWriter filter instance, used to handle timeshifting and recording.</param>
    /// <param name="tif">The transport information filter.</param>
    public TvDvbChannel(int subChannelId, TvCardBase tuner, IBaseFilter tsWriter, IBaseFilter tif)
      : base(subChannelId)
    {
      _eventPmt = new ManualResetEvent(false);
      _eventCa = new ManualResetEvent(false);

      _tuner = tuner;
      _subChannelIndex = -1;
      _tsFilterInterface = (ITsFilter)tsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelIndex);
      Log.Log.Debug("TvDvbChannel: new subchannel {0} index {1}", _subChannelId, _subChannelIndex);
      _filterTif = tif;
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~TvDvbChannel()
    {
      if (_eventPmt != null)
      {
        _eventPmt.Close();
      }

      if (_eventCa != null)
      {
        _eventCa.Close();
      }
    }

    #endregion

    #region properties

    public List<UInt16> Pids
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
      Log.Log.WriteFile("subch:{0} OnBeforeTune", _subChannelId);
      _hasTeletext = false;
      _currentAudioStream = null;
    }

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public override void OnAfterTune()
    {
      Log.Log.WriteFile("subch:{0} OnAfterTune", _subChannelId);

      // Pass the core PIDs to the tuner's hardware PID filter so that we can do
      // basic tuning and scanning.
      _pids = new List<ushort>();
      _pids.Add(0x0);    // PAT - for service lookup
      _pids.Add(0x1);    // CAT - for conditional access info when the service needs to be decrypted
      _pids.Add(0x10);   // DVB NIT - for service info
      _pids.Add(0x11);   // DVB SDT - for service info
      _pids.Add(0x1ffb); // ATSC VCT - for service info

      // If we can, also pass the PMT PID. We don't know what the PMT PID is when scanning.
      DVBBaseChannel ch = (DVBBaseChannel)_currentChannel;
      if (ch != null && ch.PmtPid > 0)
      {
        _pids.Add((UInt16)ch.PmtPid);
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
      Log.Log.Debug("TvDvbChannel: subchannel {0} wait for PMT, service ID = {1} (0x{1:x}), PMT PID = {2} (0x{2:x})", _subChannelId, serviceId, pmtPid);
      // There are 3 classes of PMT PID settings:
      // -1 = Scanning behaviour, where we don't care about PMT.
      // 0 = We don't know the correct/current PMT PID, so we ask TsWriter to determine what it should be,
      //      and then grab the associated PMT data.
      // <anything else> = A valid PMT PID for the service that we are trying to tune. TsWriter should grab
      //                    the associated PMT data.
      if (pmtPid < 0)
      {
        return true;
      }

      int pmtPidToSearchFor;
      if (_alwaysLookupPmtPidInPat)
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
          Log.Log.Debug("TvDvbChannel: search for updated PMT PID in PAT");
        }
        Log.Log.Debug("TvDvbChannel: configure PMT grabber, PMT PID = {0} (0x{0:x})", pmtPidToSearchFor);
        _tsFilterInterface.PmtSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.PmtSetPmtPid(_subChannelIndex, pmtPidToSearchFor, serviceId);

        OnAfterTuneEvent();

        // Do this as late as possible. Any PMT that arrives between when the PMT callback was set and
        // when we start waiting for PMT will cause us to miss or mess up the PMT handling.
        _pmtPid = -1;
        _pmt = null;
        _eventPmt.Reset();
        DateTime dtStartWait = DateTime.Now;
        pmtFound = _eventPmt.WaitOne(_parameters.TimeOutPMT * 1000, true);
        waitLength = DateTime.Now - dtStartWait;
        if (!pmtFound)
        {
          Log.Log.Debug("TvDvbChannel: timed out waiting for PMT after {0} seconds", waitLength.TotalSeconds);
          // One retry allowed...
          if (pmtPidToSearchFor == 0)
          {
            Log.Log.Debug("TvDvbChannel: giving up waiting for PMT - you might need to increase the PMT timeout");
            return false;
          }
          else
          {
            pmtPidToSearchFor = 0;
          }
        }
      }

      Log.Log.Debug("TvDvbChannel: found PMT after {0} seconds", waitLength.TotalSeconds);
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
      if (_filterTif != null)
      {
        Log.Log.Debug("TvDvbChannel: stop TIF");
        _filterTif.Stop();
      }

      return pmtIsValid;
    }

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// </summary>
    public override void OnGraphRunning()
    {
      Log.Log.Debug("TvDvbChannel: subchannel {0} OnGraphRunning()", _subChannelId);

      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }

      DVBBaseChannel dvbService = _currentChannel as DVBBaseChannel;
      if (dvbService == null)
      {
        throw new TvException("TvDvbChannel: current service is not set");
      }
      if (!WaitForPmt(dvbService.ServiceId, dvbService.PmtPid))
      {
        throw new TvExceptionNoPMT("TvDvbChannel: PMT not received");
      }
    }

    /// <summary>
    /// Should be called when graph is about to stop.
    /// stops any timeshifting/recording on this channel
    /// </summary>
    public override void OnGraphStop()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStop", _subChannelId);
      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.RecordStopRecord(_subChannelIndex);
        _tsFilterInterface.TimeShiftStop(_subChannelIndex);
      }
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
    }

    /// <summary>
    /// should be called when graph has been stopped
    /// Resets the graph state
    /// </summary>
    public override void OnGraphStopped()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStopped", _subChannelId);
    }

    #endregion

    #region Timeshifting - Recording methods

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="fileName">filename to which to recording should be saved</param>
    protected override void OnStartRecording(string fileName)
    {
      Log.Log.WriteFile("subch:{0} StartRecord({1})", _subChannelId, fileName);
      if (_tsFilterInterface != null)
      {
        int hr = _tsFilterInterface.RecordSetRecordingFileNameW(_subChannelIndex, fileName);
        if (hr != 0)
        {
          Log.Log.Error("subch:{0} SetRecordingFileName failed:{1:X}", _subChannelId, hr);
        }
        Log.Log.WriteFile("subch:{0}-{1} tswriter StartRecording...", _subChannelId, _subChannelIndex);
        SetRecorderPids();

        Log.Log.WriteFile("Set video / audio observer");
        _tsFilterInterface.RecorderSetVideoAudioObserver(_subChannelIndex, this);

        hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
        if (hr != 0)
        {
          Log.Log.Error("subch:{0} tswriter StartRecord failed:{1:X}", _subChannelId, hr);
        }
      }
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopRecording()
    {
      Log.Log.WriteFile("tvdvbchannel.OnStopRecording subch={0}, subch index={1}", _subChannelId, _subChannelIndex);
      if (IsRecording)
      {
        if (_tsFilterInterface != null)
        {
          Log.Log.WriteFile("tvdvbchannel.OnStopRecording subch:{0}-{1} tswriter StopRecording...", _subChannelId,
                            _subChannelIndex);
          _tsFilterInterface.RecordStopRecord(_subChannelIndex);
        }
      }
      else
      {
        Log.Log.WriteFile("tvdvbchannel.OnStopRecording - not recording");
      }
    }

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override void OnStartTimeShifting(string fileName)
    {
      Log.Log.WriteFile("subch:{0} SetTimeShiftFileName:{1}", _subChannelId, fileName);
      //int hr;
      if (_tsFilterInterface != null)
      {
        Log.Log.WriteFile("Set video / audio observer");
        _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);
        _tsFilterInterface.TimeShiftSetParams(_subChannelIndex, _parameters.MinimumFiles, _parameters.MaximumFiles,
                                              _parameters.MaximumFileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileNameW(_subChannelIndex, fileName);

        if (CurrentChannel == null)
        {
          Log.Log.Error("CurrentChannel is null when trying to start timeshifting");
          throw new Exception("TvDvbChannel: current channel is null");
        }

        //  Set the channel type (0=tv, 1=radio)
        _tsFilterInterface.TimeShiftSetChannelType(_subChannelId, (CurrentChannel.IsTv ? 0 : 1));

        Log.Log.WriteFile("subch:{0} SetTimeShiftFileName fill in pids", _subChannelId);
        SetTimeShiftPids();
        Log.Log.WriteFile("subch:{0}-{1} tswriter StartTimeshifting...", _subChannelId, _subChannelIndex);
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
        Log.Log.WriteFile("subch:{0}-{1} tswriter StopTimeshifting...", _subChannelId, _subChannelIndex);
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
    protected override void OnGetTimeShiftFilePosition(ref Int64 position, ref long bufferId)
    {
      _tsFilterInterface.TimeShiftGetCurrentFilePosition(_subChannelId, out position, out bufferId);
    }

    #endregion

    #region audio streams methods

    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public override List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        DVBAudioStream stream = null;
        List<IAudioStream> streams = new List<IAudioStream>();
        if (_pmt == null)
        {
          Log.Log.Debug("TvDvbChannel: PMT is not available");
          return streams;
        }
        foreach (PmtElementaryStream es in _pmt.ElementaryStreams)
        {
          stream = new DVBAudioStream();
          if (es.StreamType == StreamType.Ac3Audio ||
            es.PrimaryDescriptorTag == DescriptorTag.Ac3)
          {
            stream.StreamType = AudioStreamType.AC3;
          }
          else if (es.StreamType == StreamType.EnhancedAc3Audio ||
            es.PrimaryDescriptorTag == DescriptorTag.EnhancedAc3)
          {
            stream.StreamType = AudioStreamType.EAC3;
          }
          else if (es.StreamType == StreamType.Mpeg1Part3Audio)
          {
            stream.StreamType = AudioStreamType.MPEG1;
          }
          else if (es.StreamType == StreamType.Mpeg2Part3Audio)
          {
            stream.StreamType = AudioStreamType.MPEG2;
          }
          else if (es.StreamType == StreamType.Mpeg2Part7Audio)
          {
            stream.StreamType = AudioStreamType.AAC;
          }
          else if (es.StreamType == StreamType.Mpeg4Part3Audio)
          {
            stream.StreamType = AudioStreamType.LATMAAC;
          }
          else if (es.PrimaryDescriptorTag == DescriptorTag.AudioStream ||
            es.PrimaryDescriptorTag == DescriptorTag.Mpeg4Audio ||
            es.PrimaryDescriptorTag == DescriptorTag.Mpeg2AacAudio ||
            es.PrimaryDescriptorTag == DescriptorTag.Aac ||
            es.PrimaryDescriptorTag == DescriptorTag.Dts)
          {
            stream.StreamType = AudioStreamType.Unknown;
          }
          else
          {
            stream = null;
          }

          if (stream != null)
          {
            // TODO audio language descriptor handling;
            //stream.Language = info.language;
            stream.Language = "eng";
            stream.Pid = es.Pid;
            streams.Add(stream);
          }
        }
        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public override IAudioStream CurrentAudioStream
    {
      get { return _currentAudioStream; }
      set
      {
        DVBAudioStream audioStream = (DVBAudioStream)value;
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.AnalyzerSetAudioPid(_subChannelIndex, audioStream.Pid);
        }
        _currentAudioStream = audioStream;
      }
    }

    #endregion

    #region video streams methods

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

        int audioEncrypted;
        int videoEncrypted = 0;
        _tsFilterInterface.AnalyzerIsAudioEncrypted(_subChannelIndex, out audioEncrypted);
        if (_currentChannel.IsTv)
        {
          _tsFilterInterface.AnalyzerIsVideoEncrypted(_subChannelIndex, out videoEncrypted);
        }
        return ((audioEncrypted == 0) && (videoEncrypted == 0));
      }
    }

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public override IVideoStream GetCurrentVideoStream
    {
      get
      {
        VideoStream stream = null;
        if (_pmt == null)
        {
          Log.Log.Debug("TvDvbChannel: PMT is not available");
          return stream;
        }

        stream = new VideoStream();
        stream.PcrPid = _pmt.PcrPid;

        foreach (PmtElementaryStream es in _pmt.ElementaryStreams)
        {
          bool isVideoStream = true;
          if (es.StreamType == StreamType.Mpeg1Part2Video)
          {
            stream.StreamType = VideoStreamType.MPEG1;
          }
          else if (es.StreamType == StreamType.Mpeg2Part2Video)
          {
            stream.StreamType = VideoStreamType.MPEG2;
          }
          else if (es.StreamType == StreamType.Mpeg4Part2Video ||
            es.PrimaryDescriptorTag == DescriptorTag.Mpeg4Video)
          {
            stream.StreamType = VideoStreamType.MPEG4;
          }
          else if (es.StreamType == StreamType.Mpeg4Part10Video ||
            es.PrimaryDescriptorTag == DescriptorTag.AvcVideo)
          {
            stream.StreamType = VideoStreamType.H264;
          }
          else if (es.PrimaryDescriptorTag == DescriptorTag.VideoStream)
          {
            stream.StreamType = VideoStreamType.Unknown;
          }
          else
          {
            isVideoStream = false;
          }

          if (isVideoStream)
          {
            stream.Pid = es.Pid;
            return stream;
          }
        }
        return stream;
      }
    }

    #endregion

    #region teletext

    /// <summary>
    /// A derrived class should activate or deactivate the teletext grabbing on the tv card.
    /// </summary>
    protected override void OnGrabTeletext()
    {
      Log.Log.Debug("TvDvbChannel: subchannel {0} OnGrabTeletext()", _subChannelId);
      int teletextPid = -1;
      if (_grabTeletext)
      {
        foreach (PmtElementaryStream es in _pmt.ElementaryStreams)
        {
          if (es.PrimaryDescriptorTag == DescriptorTag.Teletext ||
            es.PrimaryDescriptorTag == DescriptorTag.VbiTeletext)
          {
            teletextPid = es.Pid;
            break;
          }
        }

        if (teletextPid == -1 || _pmt == null || _tsFilterInterface == null)
        {
          Log.Log.Debug("TvDvbChannel: not able to grab teletext");
          _grabTeletext = false;
        }
      }

      if (_grabTeletext)
      {
        Log.Log.Debug("TvDvbChannel: start grabbing teletext");
        _tsFilterInterface.TTxSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.TTxSetTeletextPid(_subChannelIndex, teletextPid);
        _tsFilterInterface.TTxStart(_subChannelIndex);
      }
      else
      {
        Log.Log.Debug("TvDvbChannel: stop grabbing teletext");
        _tsFilterInterface.TTxStop(_subChannelIndex);
      }
    }

    #endregion

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
        Log.Log.Debug("TvDvbChannel: subchannel {0} build PID list", _subChannelId);
        if (_pmt == null)
        {
          Log.Log.Debug("TvDvbChannel: PMT not available");
          return;
        }

        _pids = new List<ushort>();
        _pids.Add(0x1);             // CAT - for conditional access info when the service needs to be decrypted
        _pids.Add(0x12);            // DVB EIT - for EPG info
        _pids.Add(0x1ffb);          // ATSC VCT - for EPG info
        _pids.Add((UInt16)_pmtPid); // PMT - for elementary stream and conditional access changes

        _hasTeletext = false;
        _currentAudioStream = null;
        foreach (PmtElementaryStream es in _pmt.ElementaryStreams)
        {
          bool isAudio = false;
          AudioStreamType audioStreamType = AudioStreamType.Unknown;
          if (es.PrimaryDescriptorTag == DescriptorTag.Teletext ||
            es.PrimaryDescriptorTag == DescriptorTag.VbiTeletext)
          {
            _pids.Add(es.Pid);
            if (!_hasTeletext && _grabTeletext)
            {
              _tsFilterInterface.TTxSetTeletextPid(_subChannelIndex, es.Pid);
              _hasTeletext = true;
            }
          }
          else if (es.StreamType == StreamType.Ac3Audio ||
            es.PrimaryDescriptorTag == DescriptorTag.Ac3)
          {
            audioStreamType = AudioStreamType.AC3;
          }
          else if (es.StreamType == StreamType.EnhancedAc3Audio ||
            es.PrimaryDescriptorTag == DescriptorTag.EnhancedAc3)
          {
            audioStreamType = AudioStreamType.EAC3;
          }
          else if (es.StreamType == StreamType.Mpeg1Part3Audio)
          {
            audioStreamType = AudioStreamType.MPEG1;
          }
          else if (es.StreamType == StreamType.Mpeg2Part3Audio)
          {
            audioStreamType = AudioStreamType.MPEG2;
          }
          else if (es.StreamType == StreamType.Mpeg2Part7Audio)
          {
            audioStreamType = AudioStreamType.AAC;
          }
          else if (es.StreamType == StreamType.Mpeg4Part3Audio)
          {
            audioStreamType = AudioStreamType.LATMAAC;
          }
          else if (es.PrimaryDescriptorTag == DescriptorTag.AudioStream ||
            es.PrimaryDescriptorTag == DescriptorTag.Mpeg4Audio ||
            es.PrimaryDescriptorTag == DescriptorTag.Mpeg2AacAudio ||
            es.PrimaryDescriptorTag == DescriptorTag.Aac ||
            es.PrimaryDescriptorTag == DescriptorTag.Dts)
          {
            isAudio = true;
          }
          else if (es.StreamType == StreamType.Mpeg1Part2Video ||
            es.StreamType == StreamType.Mpeg2Part2Video ||
            es.StreamType == StreamType.Mpeg4Part2Video ||
            es.StreamType == StreamType.Mpeg4Part10Video ||
            es.PrimaryDescriptorTag == DescriptorTag.VideoStream ||
            es.PrimaryDescriptorTag == DescriptorTag.Mpeg4Video ||
            es.PrimaryDescriptorTag == DescriptorTag.AvcVideo)
          {
            _pids.Add(es.Pid);
            _tsFilterInterface.AnalyzerSetVideoPid(_subChannelIndex, es.Pid);
          }
          else if (es.PrimaryDescriptorTag == DescriptorTag.Subtitling)
          {
            _pids.Add(es.Pid);
          }

          if (isAudio || audioStreamType != AudioStreamType.Unknown)
          {
            _pids.Add(es.Pid);
            if (_currentAudioStream == null)
            {
              _currentAudioStream = new DVBAudioStream();
              _currentAudioStream.Language = "eng";
              _currentAudioStream.Pid = es.Pid;
              _currentAudioStream.StreamType = audioStreamType;
              _tsFilterInterface.AnalyzerSetAudioPid(_subChannelIndex, es.Pid);
            }
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
          Log.Log.WriteFile("Number of HWPIDS that needs to be sent to tuner :{0} ", hwPids.Count);
        }*/


      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
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
        Log.Log.Error("TvDvbChannel: failed to set timeshifter PIDs\r\n{0}", ex.ToString());
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
        Log.Log.Error("TvDvbChannel: failed to set recorder PIDs\r\n{0}", ex.ToString());
      }
    }

    /// <summary>
    /// Decode and process PMT data received from TsWriter.
    /// </summary>
    private bool HandlePmt()
    {
      Log.Log.Debug("TvDvbChannel: subchannel {0} handle PMT", _subChannelId);
      lock (this)
      {
        if (_currentChannel == null)
        {
          Log.Log.Debug("TvDvbChannel: current channel is not set");
          return false;
        }

        IntPtr pmtBuffer = Marshal.AllocCoTaskMem(2048);
        try
        {
          int pmtLength = _tsFilterInterface.PmtGetPMTData(_subChannelIndex, pmtBuffer);
          byte[] pmtData = new byte[pmtLength];
          Marshal.Copy(pmtBuffer, pmtData, 0, pmtLength);
          Pmt pmt = Pmt.Decode(pmtData, _tuner.CamType);
          if (pmt == null)
          {
            Log.Log.Debug("TvDvbChannel: invalid PMT detected");
            return false;
          }

          Log.Log.Debug("TvDvbChannel: SID = {0} (0x{0:x}), PMT PID = {1} (0x{1:x}), version = {2}",
                          pmt.ProgramNumber, _pmtPid, pmt.Version);

          // Have we already seen this PMT? If yes, then stop processing here. Theoretically this is a
          // redundant check as TsWriter should only pass us new PMT when the version changes.
          if (_pmt != null && _pmt.Version == pmt.Version)
          {
            return false;
          }
          Log.Log.Debug("TvDvbChannel: new PMT version");
          _pmt = pmt;

          bool serviceFreeToAir = pmt.ProgramCaDescriptors.Count == 0;
          if (serviceFreeToAir != _currentChannel.FreeToAir)
          {
            _currentChannel.FreeToAir = serviceFreeToAir;
            Log.Log.Debug("TvDvbChannel: service FTA status changed to {0} according to descriptors in PMT", serviceFreeToAir);
          }
          // Attempt to grab the CAT if the service is encrypted.
          if (!serviceFreeToAir)
          {
            GrabCat();
          }

          // TODO: call _tuner.OnPmtReady().

          return true;
        }
        catch (Exception ex)
        {
          Log.Log.Debug("TvDvbChannel: caught exception while handling PMT\r\n" + ex.ToString());
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmtBuffer);
        }
      }
      return false;
    }

    /// <summary>
    /// Attempt to retrieve CAT data from TsWriter.
    /// </summary>
    private void GrabCat()
    {
      Log.Log.Debug("TvDvbChannel: subchannel {0} grab CAT", _subChannelId);
      IntPtr catBuffer = Marshal.AllocCoTaskMem(4096);
      try
      {
        DateTime dtNow = DateTime.Now;
        _eventCa.Reset();
        _tsFilterInterface.CaSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.CaReset(_subChannelIndex);
        bool found = _eventCa.WaitOne(_parameters.TimeOutCAT * 1000, true);
        TimeSpan ts = DateTime.Now - dtNow;
        if (!found)
        {
          Log.Log.Debug("TvDvbChannel: CAT not found after {0} seconds", ts.TotalSeconds);
          return;
        }
        Log.Log.Debug("TvDvbChannel: CAT found after {0} seconds", ts.TotalSeconds);
        int catLength = _tsFilterInterface.CaGetCaData(_subChannelIndex, catBuffer);
        byte[] catData = new byte[catLength];
        Marshal.Copy(catBuffer, catData, 0, catLength);
        _cat = Cat.Decode(catData);
      }
      catch (Exception ex)
      {
        Log.Log.Debug("TvDvbChannel: caught exception while grabbing CAT\r\n" + ex.ToString());
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

    #region tswriter callback handlers

    #region ICaCallback Members

    /// <summary>
    /// Called when tswriter.ax has received a new ca section
    /// </summary>
    /// <returns></returns>
    public int OnCaReceived()
    {
      Log.Log.Debug("TvDvbChannel: subchannel {0} OnCaReceived()", _subChannelId);
      if (_eventCa != null)
      {
        _eventCa.Set();
      }
      return 0;
    }

    #endregion

    #region IPMTCallback Members

    /// <summary>
    /// Called when TsWriter receives updated PMT for a service.
    /// </summary>
    /// <param name="pmtPid">The PID of the elementary stream from which the PMT data was received.</param>
    /// <returns>an HRESULT indicating whether the event was successfully handled</returns>
    public int OnPMTReceived(int pmtPid)
    {
      // TsWriter calls this PMT callback delegate when the PMT version changes. Check if the PMT that
      // TsWriter has just received was requested. If it wasn't requested, we have to ensure that it gets
      // handled appropriately.
      Log.Log.Debug("TvDvbChannel: subchannel {0} OnPMTReceived(), PMT PID = {1} (0x{1:x}), dynamic = {2}", _subChannelId, pmtPid, _pmt != null);
      _pmtPid = pmtPid;
      if (_eventPmt != null)
      {
        _eventPmt.Set();
      }
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
      if (dvbService != null && pmtPid != dvbService.PmtPid && dvbService.PmtPid > 0)
      {
        dvbService.PmtPid = pmtPid;   // Set the value here so we don't hammer this function, regardless of update success/fail.
        TvBusinessLayer layer = new TvBusinessLayer();
        TuningDetail currentDetail = layer.GetTuningDetail(dvbService);
        if (currentDetail != null)
        {
          try
          {
            int oldPid = currentDetail.PmtPid;
            currentDetail.PmtPid = pmtPid;
            currentDetail.Persist();
            Log.Log.Debug("TvDvbChannel: updated PMT PID for service {0} (0x{0:x}) from {1} (0x{1:x}) to {2} (0x{2:x})",
                            dvbService.ServiceId, oldPid, pmtPid);
          }
          catch (Exception ex)
          {
            Log.Log.Debug("TvDvbChannel: failed to persist new PMT PID for service {0} (0x{0:x})\r\n{1}", dvbService.ServiceId, ex.ToString());
          }
        }
        else
        {
          Log.Log.Debug("TvDvbChannel: unable to persist new PMT PID for service {0} (0x{0:x})", dvbService.ServiceId);
        }
      }
    }

    #endregion

    #endregion
  }
}