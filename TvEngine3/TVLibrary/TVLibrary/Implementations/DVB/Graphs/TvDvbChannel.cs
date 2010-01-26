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
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using System.Runtime.CompilerServices;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.Helper;
using TvLibrary.Teletext;
using System.Threading;
using TvDatabase;

namespace TvLibrary.Implementations.DVB
{
  ///<summary>
  /// Base class for all dvb channels
  ///</summary>
  public class TvDvbChannel : BaseSubChannel, ITeletextCallBack, IPMTCallback, ICACallback, ITvSubChannel,
                              IVideoAudioObserver
  {
    #region variables

    #region local variables

    private bool _listenCA = false;

    /// <summary>
    /// Indicates that PMT was grabbed
    /// true : requested
    /// false: not requested
    /// </summary>
    protected bool _pmtRequested;

    /// <summary>
    /// PMT version
    /// </summary>
    protected int _pmtVersion;

    /// <summary>
    /// PMT Pid
    /// </summary>
    protected int _pmtPid = -1;

    /// <summary>
    /// Channel Info
    /// </summary>
    protected ChannelInfo _channelInfo;

    /// <summary>
    /// Ts filter instance
    /// </summary>
    protected ITsFilter _tsFilterInterface;

    /// <summary>
    /// SubChannel index
    /// </summary>
    protected int _subChannelIndex = -1;

    /// <summary>
    /// Current audio stream
    /// </summary>
    protected DVBAudioStream _currentAudioStream;

    /// <summary>
    /// MD Plugs
    /// </summary>
    protected MDPlugs _mdplugs;

    /// set to true to enable PAT lookup of PMT
    private bool alwaysUsePATLookup = DebugSettings.UsePATLookup;

    #region teletext

    private int _pmtLength;
    private byte[] _pmtData;

    #endregion

    #endregion

    #region events

    private ManualResetEvent _eventPMT; // gets signaled when PMT arrives
    private ManualResetEvent _eventCA; // gets signaled when CA arrives    

    #endregion

    #region graph variables

    private readonly ConditionalAccess _conditionalAccess;
    private readonly IBaseFilter _filterTIF;
    private readonly IBaseFilter _filterTsWriter;
    private readonly IFilterGraph2 _graphBuilder;

    #endregion

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvDvbChannel"/> class.
    /// </summary>
    public TvDvbChannel()
    {
      _listenCA = false;
      _eventPMT = new ManualResetEvent(false);
      _eventCA = new ManualResetEvent(false);
      _graphState = GraphState.Created;
      _teletextDecoder = new DVBTeletext();
      _packetHeader = new TSHelperTools.TSHeader();
      _tsHelper = new TSHelperTools();
      _channelInfo = new ChannelInfo();
      _pmtPid = -1;
      _parameters = new ScanParameters();
      _subChannelId = 0;
      _timeshiftFileName = "";
      _recordingFileName = "";
      _pmtData = null;
      _pmtLength = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvDvbChannel"/> class.
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="ca">The ca.</param>
    /// <param name="mdplugs">The mdplugs class.</param>
    /// <param name="tif">The tif filter.</param>
    /// <param name="tsWriter">The ts writer filter.</param>
    /// <param name="subChannelId">The subchannel id</param>
    /// <param name="channel">The corresponding channel</param>
    public TvDvbChannel(IFilterGraph2 graphBuilder, ConditionalAccess ca, MDPlugs mdplugs, IBaseFilter tif,
                        IBaseFilter tsWriter, int subChannelId, IChannel channel)
    {
      _listenCA = false;
      _eventPMT = new ManualResetEvent(false);
      _eventCA = new ManualResetEvent(false);
      _graphState = GraphState.Created;
      _graphBuilder = graphBuilder;
      _conditionalAccess = ca;
      _mdplugs = mdplugs;
      _filterTIF = tif;
      _filterTsWriter = tsWriter;
      _teletextDecoder = new DVBTeletext();
      _packetHeader = new TSHelperTools.TSHeader();
      _tsHelper = new TSHelperTools();
      _channelInfo = new ChannelInfo();
      _pmtPid = -1;
      _subChannelIndex = -1;
      _tsFilterInterface = (ITsFilter)_filterTsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelIndex);

      Log.Log.WriteFile("TvDvbChannel ctor new subchIndex:{0}", _subChannelIndex);

      _parameters = new ScanParameters();
      _subChannelId = subChannelId;
      _conditionalAccess.AddSubChannel(_subChannelId, channel);
      _timeshiftFileName = "";
      _recordingFileName = "";
      _pmtData = null;
      _pmtLength = 0;
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~TvDvbChannel()
    {
      if (_eventPMT != null)
      {
        _eventPMT.Close();
      }

      if (_eventCA != null)
      {
        _eventCA.Close();
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
      if (IsTimeShifting)
      {
        if (_subChannelIndex >= 0)
        {
          _tsFilterInterface.TimeShiftPause(_subChannelIndex, 1);
        }
      }
      _startTimeShifting = false;
      _startRecording = false;
      _channelInfo = new ChannelInfo();
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
      List<ushort> pids = new List<ushort>();
      pids.Add(0x0); //pat
      pids.Add(0x11); //sdt
      pids.Add(0x1fff); //padding stream
      if (_currentChannel != null)
      {
        DVBBaseChannel ch = (DVBBaseChannel)_currentChannel;
        if (ch.PmtPid > 0)
        {
          pids.Add((ushort)ch.PmtPid); //sdt
        }
      }

      _conditionalAccess.SendPids(_subChannelId, (DVBBaseChannel)_currentChannel, pids);

      _pmtPid = -1;
      _pmtVersion = -1;
    }

    private void WaitForPMT()
    {
      int retryCount = 0;
      int lookForPid;
      _pmtPid = -1;
      _pmtVersion = -1;

      DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
      if (channel != null)
      {
        if (alwaysUsePATLookup && channel.PmtPid != -1) // -1 is used for scanning. in this case it must grab PMT's
        {
          lookForPid = 0; // PAT
        }
        else
        {
          lookForPid = channel.PmtPid;
        }
        // allow retry to look for PMT in PAT if original one times out
        while (++retryCount <= 2)
        {
          if (SetupPmtGrabber(lookForPid, channel.ServiceId)) // pat lookup by sid or PMT pid
          {
            DateTime dtNow = DateTime.Now;
            int timeoutPMT = _parameters.TimeOutPMT * 1000;
            if (alwaysUsePATLookup)
            {
              Log.Log.Debug("WaitForPMT: Using new way for PMT grabbing via PAT");
              Log.Log.Debug("WaitForPMT: Waiting for SID {0}", channel.ServiceId);
            }
            else
            {
              Log.Log.Debug("WaitForPMT: Waiting for PMT {0:X}", _pmtPid);
            }

            if (_eventPMT.WaitOne(timeoutPMT, true))
            {
              TimeSpan ts = DateTime.Now - dtNow;
              Log.Log.Debug("WaitForPMT: Found PMT after {0} seconds.", ts.TotalSeconds);
              DateTime dtNowPMT2CAM = DateTime.Now;
              bool sendPmtToCamDone = false;
              try
              {
                while (ts.TotalMilliseconds < timeoutPMT && !sendPmtToCamDone)
                  //lets keep trying to send pmt2cam and at the same time obey the timelimit specified in timeoutPMT
                {
                  ts = DateTime.Now - dtNow;
                  bool updatePids;
                  int waitInterval; //ms         
                  sendPmtToCamDone = SendPmtToCam(out updatePids, out waitInterval);
                  if (sendPmtToCamDone)
                  {
                    if (updatePids)
                    {
                      if (_channelInfo != null)
                      {
                        SetMpegPidMapping(_channelInfo);
                        if (_mdplugs != null)
                          _mdplugs.SetChannel(_currentChannel, _channelInfo, false);
                      }
                      Log.Log.Info("subch:{0} stop tif", _subChannelId);
                      if (_filterTIF != null)
                      {
                        _filterTIF.Stop();
                      }
                    }
                  }
                  else
                  {
                    Log.Log.Debug("WaitForPMT: waiting for SendPmtToCam {0} ms.", ts.TotalMilliseconds);
                    Thread.Sleep(waitInterval);
                  }
                }
              }
              catch (Exception ex)
              {
                Log.Log.WriteFile("subch:{0}", ex.Message);
                Log.Log.WriteFile("subch:{0}", ex.Source);
                Log.Log.WriteFile("subch::{0}", ex.StackTrace);
              }
              TimeSpan tsPMT2CAM = DateTime.Now - dtNowPMT2CAM;
              _listenCA = false;
              if (!sendPmtToCamDone)
              {
                Log.Log.Debug("WaitForPMT: Timed out sending PMT to CAM {0} seconds.", tsPMT2CAM.TotalSeconds);
              }
              else
              {
                Log.Log.Debug("WaitForPMT: sending PMT to CAM took {0} seconds.", tsPMT2CAM.TotalSeconds);
              }

              // PMT was found so exit here
              break;
            }
            else
            {
              // Timeout waiting for PMT
              TimeSpan ts = DateTime.Now - dtNow;
              Log.Log.Debug("WaitForPMT: Timed out waiting for PMT after {0} seconds. Increase the PMT timeout value?",
                            ts.TotalSeconds);
              Log.Log.Debug("Setting to 0 to search for new PMT.");
              lookForPid = 0;
            }
          }
        } // retry loop
      }

      return;
    }

    /// <summary>
    /// Checks if the graph is running
    /// </summary>
    /// <returns>true, when the graph is running</returns>
    protected bool GraphRunning()
    {
      bool graphRunning = false;

      if (_graphBuilder != null)
      {
        try
        {
          FilterState state;
          ((IMediaControl)_graphBuilder).GetState(10, out state);
          graphRunning = (state == FilterState.Running);
        }
        catch (InvalidComObjectException)
        {
          // RCW error
          // ignore this error as, the graphbuilder is being disposed of in another thread.         
          return false;
        }
        catch (Exception e)
        {
          Log.Log.Error("GraphRunning error : {0}", e.Message);
        }
      }
      //Log.Log.WriteFile("subch:{0} GraphRunning: {1}", _subChannelId, graphRunning);
      return graphRunning;
    }

    /// <summary>
    /// Should be called when the graph is about to start
    /// Resets the state 
    /// If graph is already running, starts the pmt grabber to grab the
    /// pmt for the new channel
    /// </summary>
    public override void OnGraphStart()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStart", _subChannelId);

      if (GraphRunning())
      {
        Log.Log.WriteFile("subch:{0} Graph already running - WaitForPMT", _subChannelId);
        WaitForPMT();
      }
      else
      {
        if (_teletextDecoder != null)
          _teletextDecoder.ClearBuffer();

        _pmtPid = -1;
        _pmtVersion = -1;
      }
    }

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// when the graph hasn't been running previously
    /// </summary>
    public override void OnGraphStarted()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStarted", _subChannelId);

      if (!GraphRunning())
      {
        _pmtPid = -1;
        _pmtVersion = -1;

        Log.Log.WriteFile("subch:{0} Graph not started - skip WaitForPMT", _subChannelId);
        return;
      }

      WaitForPMT();
    }

    /// <summary>
    /// Should be called when graph is about to stop.
    /// stops any timeshifting/recording on this channel
    /// </summary>
    public override void OnGraphStop()
    {
      Log.Log.WriteFile("subch:{0} OnGraphStop", _subChannelId);
      _pmtPid = -1;
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;
      _startTimeShifting = false;
      _startRecording = false;
      _pmtVersion = -1;
      _recordingFileName = "";
      _channelInfo = new ChannelInfo();
      _currentChannel = null;

      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.RecordStopRecord(_subChannelIndex);
        _tsFilterInterface.TimeShiftStop(_subChannelIndex);
        _graphState = GraphState.Created;
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
      _graphState = GraphState.Created;
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
        int hr = _tsFilterInterface.RecordSetRecordingFileName(_subChannelIndex, fileName);
        if (hr != 0)
        {
          Log.Log.Error("subch:{0} SetRecordingFileName failed:{1:X}", _subChannelId, hr);
        }
        //if (_channelInfo.pids.Count == 0)
        if (!PMTreceived)
        {
          Log.Log.WriteFile("subch:{0} StartRecord no pmt received yet", _subChannelId);
          _startRecording = true;
        }
        else
        {
          Log.Log.WriteFile("subch:{0}-{1} tswriter StartRecording...", _subChannelId, _subChannelIndex);
          SetRecorderPids();
          hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
          if (hr != 0)
          {
            Log.Log.Error("subch:{0} tswriter StartRecord failed:{1:X}", _subChannelId, hr);
          }
          Log.Log.WriteFile("Set video / audio observer");
          _tsFilterInterface.RecorderSetVideoAudioObserver(_subChannelIndex, this);
          _graphState = GraphState.Recording;
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
          _graphState = _timeshiftFileName != "" ? GraphState.TimeShifting : GraphState.Created;
        }
      }
      {
        Log.Log.WriteFile("tvdvbchannel.OnStopRecording - not recording");
      }
    }

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override bool OnStartTimeShifting(string fileName)
    {
      //if (_channelInfo.pids.Count == 0)
      if (!PMTreceived)
      {
        Log.Log.WriteFile("subch:{0} SetTimeShiftFileName no pmt received. Timeshifting failed", _subChannelId);
        return false;
      }

      _timeshiftFileName = fileName;
      Log.Log.WriteFile("subch:{0} SetTimeShiftFileName:{1}", _subChannelId, fileName);
      //int hr;
      if (_tsFilterInterface != null)
      {
        Log.Log.WriteFile("Set video / audio observer");
        _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);
        _tsFilterInterface.TimeShiftSetParams(_subChannelIndex, _parameters.MinimumFiles, _parameters.MaximumFiles,
                                              _parameters.MaximumFileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileName(_subChannelIndex, fileName);
        Log.Log.WriteFile("subch:{0} SetTimeShiftFileName fill in pids", _subChannelId);
        _startTimeShifting = false;
        SetTimeShiftPids();
        Log.Log.WriteFile("subch:{0}-{1} tswriter StartTimeshifting...", _subChannelId, _subChannelIndex);
        _tsFilterInterface.TimeShiftStart(_subChannelIndex);

        _graphState = GraphState.TimeShifting;
      }
      return (_channelInfo.pids.Count != 0);
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopTimeShifting()
    {
      if (_timeshiftFileName != "")
      {
        Log.Log.WriteFile("subch:{0}-{1} tswriter StopTimeshifting...", _subChannelId, _subChannelIndex);
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.TimeShiftStop(_subChannelIndex);
        }
        _graphState = GraphState.Created;
      }
      _timeshiftFileName = "";
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
        List<IAudioStream> streams = new List<IAudioStream>();
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isAC3Audio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            stream.StreamType = AudioStreamType.AC3;
            streams.Add(stream);
          }
          else if (info.isEAC3Audio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            stream.StreamType = AudioStreamType.EAC3;
            streams.Add(stream);
          }
          else if (info.isAudio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            if (info.IsMpeg1Audio)
              stream.StreamType = AudioStreamType.Mpeg1;
            if (info.IsMpeg2Audio)
              stream.StreamType = AudioStreamType.Mpeg2;
            if (info.IsAACAudio)
              stream.StreamType = AudioStreamType.AAC;
            stream.StreamType = info.IsLATMAACAudio ? AudioStreamType.LATMAAC : AudioStreamType.Unknown;
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
        _pmtVersion = -1;
        bool updatePids;
        int interval;
        SendPmtToCam(out updatePids, out interval);
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
        if (GraphRunning() == false)
          return false;
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
    public override int GetCurrentVideoStream
    {
      get
      {
        if (_channelInfo == null)
          return -1;
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isVideo)
          {
            return info.stream_type;
          }
        }
        return -1;
      }
    }

    #endregion

    #region teletext

    /// <summary>
    /// A derrived class should activate or deactivate the teletext grabbing on the tv card.
    /// </summary>
    protected override void OnGrabTeletext()
    {
      if (_grabTeletext)
      {
        int teletextPid = -1;
        foreach (PidInfo pidInfo in _channelInfo.pids)
        {
          if (pidInfo.isTeletext)
          {
            teletextPid = pidInfo.pid;
            break;
          }
        }

        if (teletextPid == -1)
        {
          Log.Log.Info("subch: stop grabbing teletext");
          _tsFilterInterface.TTxStop(_subChannelIndex);
          _grabTeletext = false;
          return;
        }
        Log.Log.Info("subch: start grabbing teletext");
        _tsFilterInterface.TTxSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.TTxSetTeletextPid(_subChannelIndex, teletextPid);
        _tsFilterInterface.TTxStart(_subChannelIndex);
      }
      else
      {
        Log.Log.Info("subch: stop grabbing teletext");
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
      _conditionalAccess.FreeSubChannel(_subChannelId);
    }

    #endregion

    #region pidmapping

    /// <summary>
    /// Instructs the ts analyzer filter to start grabbing the PMT
    /// </summary>
    /// <param name="pmtPid">pid of the PMT</param>
    /// <param name="serviceId">The service id.</param>
    protected bool SetupPmtGrabber(int pmtPid, int serviceId)
    {
      Log.Log.Info("subch:{0} SetupPmtGrabber:pid {1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);

      // reset event before starting to wait
      _eventPMT.Reset();

      if (pmtPid < 0)
        return false;
      if (pmtPid == _pmtPid)
        return false;
      _pmtVersion = -1;
      _pmtPid = pmtPid;
      _pmtRequested = true; // requested
      if (_conditionalAccess != null)
        _conditionalAccess.OnRunGraph(serviceId);
      Log.Log.Write("subch:{0} set pmt grabber pmt:{1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
      _tsFilterInterface.PmtSetCallBack(_subChannelIndex, this);
      _tsFilterInterface.PmtSetPmtPid(_subChannelIndex, pmtPid, serviceId);
      if (_mdplugs != null)
      {
        Log.Log.Write("subch:{0} set ca grabber ", _subChannelId);
        _listenCA = true;
        _tsFilterInterface.CaSetCallBack(_subChannelIndex, this);
        _tsFilterInterface.CaReset(_subChannelIndex);
      }
      return true;
    }

    /// <summary>
    /// maps the correct pids to the TsFileSink filter and teletext pins
    /// </summary>
    /// <param name="info"></param>
    protected void SetMpegPidMapping(ChannelInfo info)
    {
      if (info == null)
        return;
      try
      {
        Log.Log.WriteFile("subch:{0} SetMpegPidMapping", _subChannelId);

        List<ushort> hwPids = new List<ushort>();
        hwPids.Add(0x0); //PAT
        hwPids.Add(0x1); //CAT
        hwPids.Add(0x10); //NIT
        hwPids.Add(0x11); //SDT
        hwPids.Add(0x12); //EPG

        Log.Log.WriteFile("subch:{0}  pid:{1:X} pcr", _subChannelId, info.pcr_pid);
        Log.Log.WriteFile("subch:{0}  pid:{1:X} pmt", _subChannelId, info.network_pmt_PID);

        if (info.network_pmt_PID >= 0 && ((DVBBaseChannel)_currentChannel).ServiceId >= 0)
        {
          hwPids.Add((ushort)info.network_pmt_PID);
        }

        if (info.pids != null)
        {
          foreach (PidInfo pidInfo in info.pids)
          {
            Log.Log.WriteFile("subch:{0}  {1}", _subChannelId, pidInfo.ToString());
            if (pidInfo.pid == 0 || pidInfo.pid > 0x1fff)
              continue;
            if (pidInfo.isTeletext)
            {
              Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
              if (GrabTeletext)
              {
                _tsFilterInterface.TTxSetTeletextPid(_subChannelIndex, pidInfo.pid);
              }
              hwPids.Add((ushort)pidInfo.pid);
              _hasTeletext = true;
            }
            if (pidInfo.isAC3Audio || pidInfo.isEAC3Audio || pidInfo.isAudio)
            {
              if (_currentAudioStream == null || pidInfo.isAC3Audio || pidInfo.isEAC3Audio)
              {
                _currentAudioStream = new DVBAudioStream();
                _currentAudioStream.Pid = pidInfo.pid;
                _currentAudioStream.Language = pidInfo.language;
                if (pidInfo.IsMpeg1Audio)
                  _currentAudioStream.StreamType = AudioStreamType.Mpeg1;
                else if (pidInfo.IsMpeg2Audio)
                  _currentAudioStream.StreamType = AudioStreamType.Mpeg2;
                if (pidInfo.isAC3Audio)
                  _currentAudioStream.StreamType = AudioStreamType.AC3;
                if (pidInfo.isEAC3Audio)
                  _currentAudioStream.StreamType = AudioStreamType.EAC3;
                if (pidInfo.IsAACAudio)
                  _currentAudioStream.StreamType = AudioStreamType.AAC;
                if (pidInfo.IsLATMAACAudio)
                  _currentAudioStream.StreamType = AudioStreamType.LATMAAC;
              }

              if (_currentAudioStream.Pid == pidInfo.pid)
              {
                Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
                _tsFilterInterface.AnalyzerSetAudioPid(_subChannelIndex, pidInfo.pid);
              }
              hwPids.Add((ushort)pidInfo.pid);
            }

            if (pidInfo.isVideo)
            {
              Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
              hwPids.Add((ushort)pidInfo.pid);
              _tsFilterInterface.AnalyzerSetVideoPid(_subChannelIndex, pidInfo.pid);
              if (info.pcr_pid > 0 && info.pcr_pid != pidInfo.pid)
              {
                hwPids.Add((ushort)info.pcr_pid);
              }
            }

            if (pidInfo.isDVBSubtitle)
            {
              Log.Log.WriteFile("subch:{0}    map {1}", _subChannelId, pidInfo);
              hwPids.Add((ushort)pidInfo.pid);
            }
          }
        }

        if (_mdplugs != null)
        {
          // MDPlugins Active.. 
          // It's important that the ECM pids are not blocked by the HWPID Filter in the Tuner.
          // therefore they need to be explicitly added to HWPID list.

          // HWPIDS supports max number of 16 filtered pids - Total max of 16.
          // ECM Pids
          foreach (ECMEMM ecmValue in _channelInfo.caPMT.GetECM())
          {
            if (ecmValue.Pid != 0 && !hwPids.Contains((ushort)ecmValue.Pid))
            {
              hwPids.Add((ushort)ecmValue.Pid);
            }
          }
          //EMM Pids
          foreach (ECMEMM emmValue in _channelInfo.caPMT.GetECM())
          {
            if (emmValue.Pid != 0 && !hwPids.Contains((ushort)emmValue.Pid))
            {
              hwPids.Add((ushort)emmValue.Pid);
            }
          }
          Log.Log.WriteFile("Number of HWPIDS that needs to be sent to tuner :{0} ", hwPids.Count);
        }

        if (info.network_pmt_PID >= 0 && ((DVBBaseChannel)_currentChannel).ServiceId >= 0)
        {
          _conditionalAccess.SendPids(_subChannelId, (DVBBaseChannel)_currentChannel, hwPids);
        }

        if (_startTimeShifting)
        {
          _startTimeShifting = false;
          _tsFilterInterface.TimeShiftReset(_subChannelIndex);
          SetTimeShiftPids();
          _tsFilterInterface.TimeShiftStart(_subChannelIndex);

          Log.Log.WriteFile("Set video / audio observer");
          _tsFilterInterface.SetVideoAudioObserver(_subChannelIndex, this);

          _graphState = GraphState.TimeShifting;
        }
        if (_startRecording)
        {
          _startRecording = false;
          SetRecorderPids();

          int hr = _tsFilterInterface.RecordStartRecord(_subChannelIndex);
          if (hr != 0)
          {
            Log.Log.Error("subch:[0} StartRecord failed:{1:X}", _subChannelId, hr);
          }

          Log.Log.WriteFile("Set video / audio observer");
          _tsFilterInterface.RecorderSetVideoAudioObserver(_subChannelIndex, this);

          _graphState = GraphState.Recording;
        }
        else
        {
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
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    /// <summary>
    /// Sets the pids for the timeshifter
    /// </summary>
    private void SetTimeShiftPids()
    {
      //Log.Log.WriteFile("SetTimeShiftPids new DLL");
      if (_channelInfo == null)
        return;
      if (_channelInfo.pids.Count == 0)
        return;
      if (_currentChannel == null)
        return;
      //if (_currentAudioStream == null) return;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel == null)
        return;

      try
      {
        _tsFilterInterface.TimeShiftPause(_subChannelIndex, 1);
        _tsFilterInterface.TimeShiftSetPmtPid(_subChannelIndex, dvbChannel.PmtPid, dvbChannel.ServiceId, _pmtData,
                                              _pmtLength);

        //_linkageScannerEnabled = (layer.GetSetting("linkageScannerEnabled", "no").Value == "yes");

        _tsFilterInterface.TimeShiftPause(_subChannelIndex, 0);
        _dateTimeShiftStarted = DateTime.Now;
      }
      catch (Exception e)
      {
        Log.Log.Error("could not set TimeShiftSetPmtPid {0}", e.Message);
      }
    }

    /// <summary>
    /// Sets the pids for the recorder
    /// </summary>
    private void SetRecorderPids()
    {
      Log.Log.WriteFile("SetRecorderPids");
      if (_channelInfo == null)
        return;
      if (_channelInfo.pids.Count == 0)
        return;
      if (_currentChannel == null)
        return;
      if (_currentAudioStream == null)
        return;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel == null)
        return;

      if (dvbChannel.PmtPid > 0)
      {
        _tsFilterInterface.RecordSetPmtPid(_subChannelIndex, dvbChannel.PmtPid, dvbChannel.ServiceId, _pmtData,
                                           _pmtLength);
      }
      _dateRecordingStarted = DateTime.Now;
    }

    /// <summary>
    /// Decodes the PMT and sends the PMT to cam.
    /// </summary>
    protected bool SendPmtToCam(out bool updatePids, out int waitInterval)
    {
      lock (this)
      {
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        updatePids = false;
        waitInterval = 100;
        bool foundCA = false;
        if (_mdplugs != null)
        {
          if (channel != null)
          {
            //HACK: Currently Premiere Direkt Feeds (nid=133) have the free_ca flag in SDT set to true (means not scrambled), so we have to override this
            if ((!channel.FreeToAir) || (channel.NetworkId == 133 && !channel.Provider.Equals("BetaDigital")))
            {
              DateTime dtNow = DateTime.Now;
              foundCA = false;
              //Log.Log.Info("subch:{0} listen for CA", _listenCA);
              if (!_eventCA.WaitOne(10000, true)) //wait 10 sec for CA to arrive.
              {
                TimeSpan ts = DateTime.Now - dtNow;
                Log.Log.Info("subch:{0} SendPmt:no CA found after {1} seconds", _subChannelId, ts.TotalSeconds);
                return false;
              }
              else
              {
                foundCA = true;
                TimeSpan ts = DateTime.Now - dtNow;
                Log.Log.Info("subch:{0} SendPmt:CA found after {1} seconds", _subChannelId, ts.TotalSeconds);
              }
            }
          }
        }

        if (channel == null)
        {
          Log.Log.Info("subch:{0} SendPmt:no channel set", _subChannelId);
          return true;
        }
        IntPtr pmtMem = Marshal.AllocCoTaskMem(4096); // max. size for pmt
        IntPtr catMem = Marshal.AllocCoTaskMem(4096); // max. size for cat
        try
        {
          _pmtLength = _tsFilterInterface.PmtGetPMTData(_subChannelIndex, pmtMem);
          if (_pmtLength > 6)
          {
            _pmtData = new byte[_pmtLength];
            Marshal.Copy(pmtMem, _pmtData, 0, _pmtLength);
            int version = ((_pmtData[5] >> 1) & 0x1F);
            int pmtProgramNumber = (_pmtData[3] << 8) + _pmtData[4];
            Log.Log.Info("subch:{0} SendPmt:{1:X} {2:X} {3:X} {4:X}", _subChannelId, pmtProgramNumber, channel.ServiceId,
                         _pmtVersion, version);
            if (pmtProgramNumber == channel.ServiceId)
            {
              if (_pmtVersion != version)
              {
                _channelInfo = new ChannelInfo();
                _channelInfo.DecodePmt(_pmtData);
                _channelInfo.network_pmt_PID = channel.PmtPid;

                // always set pcr_pid despite useless database info, as it's required for setup HW filtering ( ambass )
                channel.PcrPid = _channelInfo.pcr_pid;

                // update any service scrambled / unscambled changes
                if (_channelInfo.scrambled == channel.FreeToAir)
                {
                  channel.FreeToAir = !_channelInfo.scrambled;
                }
                if ((_mdplugs != null) && (foundCA))
                {
                  try
                  {
                    int catLength = _tsFilterInterface.CaGetCaData(_subChannelIndex, catMem);
                    if (catLength > 0)
                    {
                      byte[] cat = new byte[catLength];
                      Marshal.Copy(catMem, cat, 0, catLength);
                      _channelInfo.DecodeCat(cat, catLength);
                    }
                  }
                  catch (Exception ex)
                  {
                    Log.Log.Write(ex);
                  }
                }

                updatePids = true;
                Log.Log.WriteFile("subch:{0} SendPMT version:{1} len:{2} {3}", _subChannelId, version, _pmtLength,
                                  _channelInfo.caPMT.ProgramNumber);
                if (_conditionalAccess != null)
                {
                  int audioPid = -1;
                  if (_currentAudioStream != null)
                  {
                    audioPid = _currentAudioStream.Pid;
                  }

                  if (_conditionalAccess.SendPMT(_subChannelId, (DVBBaseChannel)CurrentChannel, _pmtData, _pmtLength,
                                                 audioPid))
                  {
                    _pmtVersion = version;
                    Log.Log.WriteFile("subch:{0} cam flags:{1}", _subChannelId, _conditionalAccess.IsCamReady());
                    return true;
                  }
                  else
                  {
                    //cam is not ready yet
                    Log.Log.WriteFile("subch:{0} SendPmt failed cam flags:{1}", _subChannelId,
                                      _conditionalAccess.IsCamReady());
                    _pmtVersion = -1;
                    waitInterval = 3000;
                    return false;
                  }
                }
                else
                {
                  Log.Log.Info("subch:{0} No cam in use", _subChannelId);
                }
                _pmtVersion = version;
                return true;
              }
              else
              {
                //already received this pmt                
                return true;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Log.Write(ex);
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmtMem);
          Marshal.FreeCoTaskMem(catMem);
        }
      }
      return false;
    }

    #endregion

    #region properties

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public bool PMTreceived
    {
      get { return (_pmtVersion > -1 && _channelInfo.pids.Count > 0); }
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
      if (_eventCA != null && _listenCA)
      {
        Log.Log.WriteFile("subch:OnCaReceived()");
        _eventCA.Set();
      }
      return 0;
    }

    #endregion

    #region IPMTCallback Members

    /// <summary>
    /// Called when tswriter.ax has received a new pmt
    /// </summary>
    /// <returns></returns>
    public int OnPMTReceived(int pmtPid)
    {
      if (_eventPMT != null)
      {
        Log.Log.WriteFile("subch:{0} OnPMTReceived() pmt:{3:X} ran:{1} dynamic:{2}", _subChannelId, GraphRunning(),
                          !_pmtRequested, pmtPid);
        _eventPMT.Set();
        // PMT callback is done on each new PMT version
        // check if the arrived PMT was _NOT_ requested (WaitForPMT), than it means dynamical change
        if (_pmtRequested == false)
        {
          bool updatePids;
          int waitInterval;
          if (SendPmtToCam(out updatePids, out waitInterval))
          {
            if (updatePids)
            {
              if (_channelInfo != null)
              {
                SetMpegPidMapping(_channelInfo);
                if (_mdplugs != null)
                  _mdplugs.SetChannel(_currentChannel, _channelInfo, true);
              }
            }
          }
          else
          {
            Log.Log.Debug("Failed SendPmtToCam in callback handler");
          }
        }
      }
      // check if PMT has changed, in this case update tuning details
      DVBBaseChannel CurrentDVBChannel = _currentChannel as DVBBaseChannel;
      if (pmtPid != CurrentDVBChannel.PmtPid && !alwaysUsePATLookup)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Channel dbChannel = layer.GetChannelByTuningDetail(CurrentDVBChannel.NetworkId, CurrentDVBChannel.TransportId,
                                                           CurrentDVBChannel.ServiceId);
        TuningDetail currentDetail = layer.GetChannel(CurrentDVBChannel.Provider, CurrentDVBChannel.Name,
                                                      CurrentDVBChannel.ServiceId);
        currentDetail.PmtPid = pmtPid;
        CurrentDVBChannel.PmtPid = pmtPid;
        TvDatabase.TuningDetail td = layer.UpdateTuningDetails(dbChannel, CurrentDVBChannel, currentDetail);
        Log.Log.Debug("Updated PMT Pid to {0:X}!", pmtPid);
        td.Persist();
      }
      _pmtRequested = false; // once received, reset
      return 0;
    }

    #endregion

    #endregion
  }
}