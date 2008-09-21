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
using System.Xml.Serialization;
using System.Text;
using Microsoft.Win32;
using DirectShowLib;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Teletext;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.Helper;
using TvLibrary.Helper;
using TvLibrary.ChannelLinkage;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tv cards
  /// </summary>
  public class HDPVRChannel : BaseSubChannel, ITvSubChannel, IAnalogTeletextCallBack, IVideoAudioObserver, IPMTCallback
  {
    #region variables
    private TvCardHDPVR _card;
    private IBaseFilter _filterTsWriter;
    private ITsFilter _tsFilterInterface;
    private Int32 _pmtPid = 0;
    private Int32 _serviceID = 1;
    private Int32 _pmtVersion = -1;
    private int _pmtLength;
    private byte[] _pmtData;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogSubChannel"/> class.
    /// </summary>
    public HDPVRChannel(TvCardHDPVR card, Int32 subchannelId, IChannel channel, IBaseFilter filterTsWriter)
      : base()
    {
      _card = card;
      _subChannelId = subchannelId;
      _currentChannel = channel;
      _filterTsWriter = filterTsWriter;
      _hasTeletext = false;
      _tsFilterInterface = (ITsFilter)_filterTsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelId);
      _pmtData = null;
      _pmtLength = 0;
    }

    #region tuning and graph methods

    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public override void OnBeforeTune()
    {
      Log.Log.WriteFile("analog subch:{0} OnBeforeTune", _subChannelId);
      if (IsTimeShifting)
      {
        if (_subChannelId >= 0)
        {
          _tsFilterInterface.TimeShiftPause(_subChannelId, 1);
        }
      }
    }

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public override void OnAfterTune()
    {
      Log.Log.WriteFile("analog subch:{0} OnAfterTune", _subChannelId);
      if (IsTimeShifting)
      {
        if (_subChannelId >= 0)
        {
          _tsFilterInterface.TimeShiftPause(_subChannelId, 0);
          //hack
          OnNotify(PidType.Audio);
          OnNotify(PidType.Video);
        }
      }
    }

    /// <summary>
    /// Should be called when the graph is about to start
    /// Resets the state 
    /// If graph is already running, starts the pmt grabber to grab the
    /// pmt for the new channel
    /// </summary>
    public override void OnGraphStart()
    {
      Log.Log.WriteFile("analog subch:{0} OnGraphStart", _subChannelId);
      DateTime dtNow;
      if (SetupPmtGrabber(256, _serviceID))
      {
        dtNow = DateTime.Now;
        while (_pmtVersion < 0)
        {
          Log.Log.Write("subch:{0} wait for pmt", _subChannelId);
          System.Threading.Thread.Sleep(20);
          TimeSpan ts = DateTime.Now - dtNow;
          if (ts.TotalMilliseconds >= 5000)
          {
            Log.Log.Debug("Timedout waiting for PMT after {0} seconds. Increase the PMT timeout value?", ts.TotalSeconds);
            break;
          }
        }
      }
    }

    protected bool SetupPmtGrabber(int pmtPid, int serviceId)
    {
      Log.Log.Info("subch:{0} SetupPmtGrabber:pid {1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
      if (pmtPid < 0) return false;
      if (pmtPid == _pmtPid) return false;
      _pmtVersion = -1;
      _pmtPid = pmtPid;
      Log.Log.Write("subch:{0} set pmt grabber pmt:{1:X} sid:{2:X}", _subChannelId, pmtPid, serviceId);
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
      Log.Log.WriteFile("analog subch:{0} OnGraphStarted", _subChannelId);
      _graphRunning = true;
      _dateTimeShiftStarted = DateTime.MinValue;
      bool result = false;
      result = SetupPmtGrabber(_pmtPid, _serviceID);
      DateTime dtNow = DateTime.Now;
      while (_pmtVersion < 0)
      {
        Log.Log.Write("subch:{0} wait for pmt:{1:X}", _subChannelId, _pmtPid);
        System.Threading.Thread.Sleep(20);
      }
    }

    public override void OnGraphStop()
    {
      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.RecordStopRecord(_subChannelId);
        _tsFilterInterface.TimeShiftStop(_subChannelId);
      }
    }

    public override void OnGraphStopped()
    {
    }

    #endregion

    #region Timeshifting - Recording methods

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override bool OnStartTimeShifting(string fileName)
    {
      _timeshiftFileName = fileName;
      Log.Log.WriteFile("HDPVR: SetTimeShiftFileName:{0}", fileName);
      Log.Log.WriteFile("HDPVR: SetTimeShiftFileName: _subChannelId {0}", _subChannelId);
      ScanParameters _parameters = _card.Parameters;
      _tsFilterInterface.TimeShiftSetParams(_subChannelId, _parameters.MinimumFiles, _parameters.MaximumFiles, _parameters.MaximumFileSize);
      _tsFilterInterface.TimeShiftSetTimeShiftingFileName(_subChannelId, fileName);
      _tsFilterInterface.TimeShiftSetMode(_subChannelId, TimeShiftingMode.TransportStream);
      _tsFilterInterface.TimeShiftPause(_subChannelId, 1);
      //HDPVR pmt is always 256
      _tsFilterInterface.TimeShiftSetPmtPid(_subChannelId, 0x0100, 1, _pmtData, _pmtLength);
      //HDPVR video pid is always 4113
      _tsFilterInterface.AnalyzerSetVideoPid(_subChannelId, 0x1011);
      //HDPVR audio pdi is always 4352
      _tsFilterInterface.AnalyzerSetAudioPid(_subChannelId, 0x1100);
      _tsFilterInterface.TimeShiftPause(_subChannelId, 0);
      _tsFilterInterface.TimeShiftStart(_subChannelId);
      _tsFilterInterface.SetVideoAudioObserver(_subChannelId, this);
      _dateTimeShiftStarted = DateTime.Now;
      return true;
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns></returns>
    protected override void OnStopTimeShifting()
    {
      Log.Log.WriteFile("HDPVR: StopTimeShifting()");
      _tsFilterInterface.SetVideoAudioObserver(_subChannelId, null);
      _tsFilterInterface.TimeShiftStop(_subChannelId);
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns></returns>
    protected override void OnStartRecording(bool transportStream, string fileName)
    {
      _recordingFileName = fileName;
      ScanParameters _parameters = _card.Parameters;
      _tsFilterInterface.RecordSetRecordingFileName(_subChannelId, fileName);
      _tsFilterInterface.RecordSetMode(_subChannelId, TimeShiftingMode.TransportStream);
      _tsFilterInterface.RecordSetPmtPid(_subChannelId, 0x0100, 1, _pmtData, _pmtLength);
      _startRecording = true;
      Int32 hr = _tsFilterInterface.RecordStartRecord(_subChannelId);
      if (hr != 0)
      {
        Log.Log.Error("subch:{0} StartRecord failed:{1:X}", _subChannelId, hr);
      }
      _graphState = GraphState.Recording;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopRecording()
    {
      Log.Log.WriteFile("HDPVR: StopRecord()");
      _tsFilterInterface.RecordStopRecord(_subChannelId);
      if (_card.SupportsQualityControl && IsTimeShifting)
      {
        _card.Quality.StartPlayback();
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
      if (_tsFilterInterface != null)
      {
        _tsFilterInterface.DeleteChannel(_subChannelId);
      }
    }

    #endregion

    public int OnPMTReceived()
    {
      IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
      _pmtLength = _tsFilterInterface.PmtGetPMTData(_subChannelId, pmtMem);
      if (_pmtLength > 6)
      {
        _pmtData = new byte[_pmtLength];
      }
      Marshal.Copy(pmtMem, _pmtData, 0, _pmtLength);
      Marshal.FreeCoTaskMem(pmtMem);
      _pmtVersion++;
      try
      {
        Log.Log.WriteFile("subch:{0} OnPMTReceived() {1}", _subChannelId, _graphRunning);
        if (_graphRunning == false) return 0;
        _pmtVersion++;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return 0;
    }

    public int OnNotify(PidType pidType)
    {
      if (pidType == PidType.Video)
      {
        //adds delay to avoid stutter
        System.Threading.Thread.Sleep(1000);
      }
      return base.OnNotify(pidType);
    }
  }
}