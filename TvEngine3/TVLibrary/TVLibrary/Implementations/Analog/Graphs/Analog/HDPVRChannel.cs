/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tv cards
  /// </summary>
  public class HDPVRChannel : BaseSubChannel, ITvSubChannel, IVideoAudioObserver, IPMTCallback
  {
    #region variables
    private readonly TvCardHDPVR _card;
    private readonly IBaseFilter _filterTsWriter;
    private readonly IFilterGraph2 _graphBuilder;
    private readonly ITsFilter _tsFilterInterface;
    private Int32 _pmtPid = 256; //always 256 for HDPVR
    private const int _serviceID = 1;
    private Int32 _pmtVersion = -1;
    private int _pmtLength;
    private byte[] _pmtData;

    private ManualResetEvent _eventPMT; // gets signaled when PMT arrives

    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogSubChannel"/> class.
    /// </summary>
    public HDPVRChannel(TvCardHDPVR card, IFilterGraph2 graphBuilder, Int32 subchannelId, IChannel channel, IBaseFilter filterTsWriter)
    {
      _card = card;
      _graphBuilder = graphBuilder;
      _subChannelId = subchannelId;
      _currentChannel = channel;
      _filterTsWriter = filterTsWriter;
      _hasTeletext = false;
      _parameters = new ScanParameters();
      _tsFilterInterface = (ITsFilter)_filterTsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelId);
      _pmtData = null;
      _pmtLength = 0;
    }
    #endregion

    #region tuning and graph methods

    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public override void OnBeforeTune()
    {
      Log.Log.WriteFile("HDPVR: subch:{0} OnBeforeTune", _subChannelId);
      if (IsTimeShifting && _subChannelId >= 0)
      {
        _tsFilterInterface.TimeShiftPause(_subChannelId, 1);
      }
    }

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public override void OnAfterTune()
    {
      Log.Log.WriteFile("HDPVR: subch:{0} OnAfterTune", _subChannelId);
      if (IsTimeShifting && _subChannelId >= 0)
      {
        _tsFilterInterface.TimeShiftPause(_subChannelId, 0);
        //hack
        OnNotify(PidType.Audio);
        OnNotify(PidType.Video);
      }
    }

    private void WaitForPMT()
    {
      _pmtPid = -1;
      _pmtVersion = -1;

      HDPVRChannel channel = _currentChannel as HDPVRChannel;
      if (channel != null)
      {
        _eventPMT = new ManualResetEvent(false);
        if (SetupPmtGrabber(_pmtPid, _serviceID))
        {
          DateTime dtNow = DateTime.Now;
          int timeoutPMT = _parameters.TimeOutPMT * 1000;
          Log.Log.Debug("WaitForPMT: Waiting for PMT.");
          if (_eventPMT.WaitOne(timeoutPMT, true))
          {
            _eventPMT.Close();
            _eventPMT = null;
            TimeSpan ts = DateTime.Now - dtNow;
            Log.Log.Debug("WaitForPMT: Found PMT after {0} seconds.", ts.TotalSeconds);
            IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
            _pmtLength = _tsFilterInterface.PmtGetPMTData(_subChannelId, pmtMem);
            if (_pmtLength > 6)
            {
              _pmtData = new byte[_pmtLength];
            }
            _pmtData = new byte[_pmtLength];
            Marshal.Copy(pmtMem, _pmtData, 0, _pmtLength);
            Marshal.FreeCoTaskMem(pmtMem);
            _pmtVersion = ((_pmtData[5] >> 1) & 0x1F);
          }
          else
          {
            TimeSpan ts = DateTime.Now - dtNow;
            Log.Log.Debug("WaitForPMT: Timed out waiting for PMT after {0} seconds. Increase the PMT timeout value?", ts.TotalSeconds);
          }
        }
      }
      if (_eventPMT != null)
      {
        _eventPMT.Close();
        _eventPMT = null;
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
        } catch (InvalidComObjectException)
        {
          // RCW error
          // ignore this error as, the graphbuilder is being disposed of in another thread.         
          return false;
        } catch (Exception e)
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
        WaitForPMT();
      }
      else
      {
        Log.Log.WriteFile("subch:{0} Graph already running - skip WaitForPMT", _subChannelId);
        if (_teletextDecoder != null)
          _teletextDecoder.ClearBuffer();

        _pmtPid = -1;
        _pmtVersion = -1;
      }
    }

    /// <summary>
    /// Instructs the ts analyzer filter to start grabbing the PMT
    /// </summary>
    /// <param name="pmtPid">pid of the PMT</param>
    /// <param name="serviceId">The service id.</param>
    protected bool SetupPmtGrabber(int pmtPid, int serviceId)
    {
      Log.Log.Info("HDPVR: subch:{0} SetupPmtGrabber:pid {1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
      if (pmtPid < 0)
        return false;
      //if (pmtPid == _pmtPid) return false;
      _pmtVersion = -1;
      _pmtPid = pmtPid;
      Log.Log.Write("HDPVR: subch:{0} set pmt grabber pmt:{1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
      _tsFilterInterface.PmtSetCallBack(_subChannelId, this);
      _tsFilterInterface.PmtSetPmtPid(_subChannelId, pmtPid, serviceId);
      return true;
    }

    /// <summary>
    /// Should be called when the graph has been started
    /// sets up the pmt grabber to grab the pmt of the channel
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
      _currentChannel = null;
      _recordTransportStream = false;

      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.RecordStopRecord(_subChannelId);
        _tsFilterInterface.TimeShiftStop(_subChannelId);
        _graphState = GraphState.Created;
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
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override bool OnStartTimeShifting(string fileName)
    {
      if (!PMTreceived)
      {
        Log.Log.WriteFile("subch:{0} SetTimeShiftFileName no pmt received. Timeshifting failed", _subChannelId);
        return false;
      }
      if (_card.SupportsQualityControl && !IsRecording)
      {
        _card.Quality.StartPlayback();
      }
      _timeshiftFileName = fileName;
      Log.Log.WriteFile("HDPVR: SetTimeShiftFileName:{0}", fileName);
      Log.Log.WriteFile("HDPVR: SetTimeShiftFileName: _subChannelId {0}", _subChannelId);
      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.SetVideoAudioObserver(_subChannelId, this);

        _tsFilterInterface.TimeShiftSetParams(_subChannelId, _parameters.MinimumFiles, _parameters.MaximumFiles,
                                              _parameters.MaximumFileSize);
        _tsFilterInterface.TimeShiftSetTimeShiftingFileName(_subChannelId, fileName);
        _tsFilterInterface.TimeShiftSetMode(_subChannelId, TimeShiftingMode.TransportStream);

        _startTimeShifting = false;

        _tsFilterInterface.TimeShiftPause(_subChannelId, 1);
        //HDPVR pmt is always 256
        _tsFilterInterface.TimeShiftSetPmtPid(_subChannelId, 0x0100, 1, _pmtData, _pmtLength);
        //HDPVR video pid is always 4113
        _tsFilterInterface.AnalyzerSetVideoPid(_subChannelId, 0x1011);
        //HDPVR audio pdi is always 4352
        _tsFilterInterface.AnalyzerSetAudioPid(_subChannelId, 0x1100);
        _tsFilterInterface.TimeShiftPause(_subChannelId, 0);
        _tsFilterInterface.TimeShiftStart(_subChannelId);
        _dateTimeShiftStarted = DateTime.Now;
      }
      return true;
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns></returns>
    protected override void OnStopTimeShifting()
    {
      if (_timeshiftFileName != "")
      {
        Log.Log.WriteFile("subch:{0}-{1} tswriter StopTimeshifting...", _subChannelId, _subChannelId);
        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.TimeShiftStop(_subChannelId);
        }
        _graphState = GraphState.Created;
      }
      _timeshiftFileName = "";
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns></returns>
    protected override void OnStartRecording(bool transportStream, string fileName)
    {
      if (_card.SupportsQualityControl)
      {
        _card.Quality.StartRecord();
      }
      _recordingFileName = fileName;
      _tsFilterInterface.RecordSetRecordingFileName(_subChannelId, fileName);
      _tsFilterInterface.RecordSetMode(_subChannelId, TimeShiftingMode.TransportStream);
      if (!PMTreceived)
      {
        Log.Log.WriteFile("subch:{0} StartRecord no pmt received yet", _subChannelId);
        _startRecording = true;
      }
      else
      {

        _tsFilterInterface.RecordSetPmtPid(_subChannelId, 0x0100, 1, _pmtData, _pmtLength);
        _startRecording = true;
        Int32 hr = _tsFilterInterface.RecordStartRecord(_subChannelId);
        if (hr != 0)
        {
          Log.Log.Error("HDPVR: subch:{0} StartRecord failed:{1:X}", _subChannelId, hr);
        }
        _graphState = GraphState.Recording;
      }
      _dateRecordingStarted = DateTime.Now;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopRecording()
    {
      if (IsRecording)
      {
        Log.Log.WriteFile("subch:{0}-{1} tswriter StopRecording...", _subChannelId, _subChannelId);

        if (_tsFilterInterface != null)
        {
          _tsFilterInterface.RecordStopRecord(_subChannelId);
          if (_card.SupportsQualityControl && IsTimeShifting)
          {
            _card.Quality.StartPlayback();
          }
          _graphState = _timeshiftFileName != "" ? GraphState.TimeShifting : GraphState.Created;
        }
      }
    }

    #endregion

    #region audio streams

    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public override List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        //TODO: better info on audio stream
        return null;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public override IAudioStream CurrentAudioStream
    {
      get
      {
        //TODO: better info on audio stream
        return null;
      }
      set
      {
      }
    }

    #endregion

    #region video stream

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public override bool IsReceivingAudioVideo
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Returns the video format (always returns MPEG2). 
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public override int GetCurrentVideoStream
    {
      get
      {
        return 2;
      }
    }

    #endregion

    #region teletext

    /// <summary>
    /// A derrived class should activate or deactivate the teletext grabbing on the tv card.
    /// </summary>
    protected override void OnGrabTeletext()
    {
    }

    #endregion

    #region OnDecompose

    /// <summary>
    /// Decomposes this subchannel
    /// </summary>
    protected override void OnDecompose()
    {
      if (_tsFilterInterface != null && _subChannelId >=0)
      {
        _tsFilterInterface.DeleteChannel(_subChannelId);
        _subChannelId = -1;
      }
    }

    #endregion

    #region tswriter callback handlers
    /// <summary>
    /// Called when the PMT has been received.
    /// </summary>
    /// <returns></returns>
    public int OnPMTReceived()
    {
      if (_eventPMT != null)
      {
        Log.Log.WriteFile("subch:{0} OnPMTReceived() {1}", _subChannelId, GraphRunning());
        _eventPMT.Set();
      }
      return 0;
    }

    ///<summary>
    /// Called when a pid is detected
    ///</summary>
    ///<param name="pidType">The pid type</param>
    ///<returns>Error code</returns>
    public new int OnNotify(PidType pidType)
    {
      if (pidType == PidType.Video)
      {
        //adds delay to avoid stutter
        Thread.Sleep(1000);
      }
      return base.OnNotify(pidType);
    }
    #endregion

    #region properties

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public bool PMTreceived
    {
      get
      {
        return _pmtVersion > -1;
      }
    }

    #endregion


  }
}
