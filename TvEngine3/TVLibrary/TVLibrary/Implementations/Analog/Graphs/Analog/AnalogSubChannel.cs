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
  public class AnalogSubChannel : AVObserverSubChannel, ITvSubChannel, IAnalogTeletextCallBack, IAnalogVideoAudioObserver
  {
    #region variables
    private bool _hasTeletext;
    private bool _grabTeletext;
    private DVBTeletext _teletextDecoder;
    private IVbiCallback _teletextCallback;
    private TSHelperTools.TSHeader _packetHeader;
    private TSHelperTools _tsHelper;

    private IChannel _currentChannel;
    private string _timeshiftFileName;
    private string _recordingFileName;
    private DateTime _dateTimeShiftStarted;
    private DateTime _dateRecordingStarted;
    private TvCardAnalog _card;
    private IBaseFilter _filterTvTunerFilter;
    private IBaseFilter _filterTvAudioTuner;
    private int _subChannelId;
    private IBaseFilter _mpFileWriter;
    private IMPRecord _mpRecord;
    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogSubChannel"/> class.
    /// </summary>
    public AnalogSubChannel(TvCardAnalog card, int subchnnelId, IBaseFilter filterTvTunerFilter, IBaseFilter filterTvAudioTuner, IPin pinVBI, IBaseFilter mpFileWriter)
    {
      _card = card;
      _hasTeletext = (pinVBI != null);
      _filterTvTunerFilter = filterTvTunerFilter;
      _filterTvAudioTuner = filterTvAudioTuner;
      _mpFileWriter = mpFileWriter;
      _mpRecord = (IMPRecord)_mpFileWriter;
      _teletextDecoder = new DVBTeletext();
      _timeshiftFileName = String.Empty;
      _recordingFileName = String.Empty;
      _dateRecordingStarted = DateTime.MinValue;
      _dateTimeShiftStarted = DateTime.MinValue;
      _subChannelId = 0;
      _tsHelper = new TSHelperTools();
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the sub channel id.
    /// </summary>
    /// <value>The sub channel id.</value>
    public int SubChannelId
    {
      get
      {
        return _subChannelId;
      }
    }

    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    public string TimeShiftFileName
    {
      get
      {
        return _timeshiftFileName;
      }
    }
    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime StartOfTimeShift
    {
      get
      {
        return _dateTimeShiftStarted;
      }
    }
    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get
      {
        return _dateRecordingStarted;
      }
    }
    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public bool IsReceivingAudioVideo
    {
      get
      {
        return true;
      }
    }
    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    public string RecordingFileName
    {
      get
      {
        return _recordingFileName;
      }
    }

    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get
      {
        return (_recordingFileName.Length > 0);
      }
    }

    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    public bool IsTimeShifting
    {
      get
      {
        return (_timeshiftFileName.Length > 0);
      }
    }
    /// <summary>
    /// returns the IChannel to which the card is currently tuned
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
    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsTimeshiftingTransportStream
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsRecordingTransportStream
    {
      get
      {
        return false;
      }
    }
    #endregion

    /// <summary>
    /// Retursn the video format (always returns MPEG2). 
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int GetCurrentVideoStream
    {
      get
      {
        return 2;
      }
    }

    #region teletext
    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    /// <value></value>
    public bool HasTeletext
    {
      get
      {
        return _hasTeletext;
      }
    }

    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    public bool GrabTeletext
    {
      get
      {
        return _grabTeletext;
      }
      set
      {
        if (_hasTeletext)
        {
          _grabTeletext = value;
          if (_grabTeletext)
          {
            _mpRecord.TTxSetCallback(this);
          } else
          {
            _mpRecord.TTxSetCallback(null);
          }
        } else
        {
          _grabTeletext = false;
          _mpRecord.TTxSetCallback(null);
        }
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
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("Analog: StartTimeShifting()");
      SetTimeShiftFileNameAndStartTimeShifting(fileName);
      return true;
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns></returns>
    public bool StopTimeShifting()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("Analog: StopTimeShifting()");
      _mpRecord.SetVideoAudioObserver(null);
      _mpRecord.StopTimeShifting();
      _dateTimeShiftStarted = DateTime.MinValue;
      return true;
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns></returns>
    public bool StartRecording(bool transportStream, string fileName)
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("Analog:StartRecording to {0}", fileName);

      //fileName = System.IO.Path.ChangeExtension(fileName, ".mpg");
      StartRecord(transportStream, fileName);


      _recordingFileName = fileName;
      Log.Log.WriteFile("Analog:Started recording");
      return true;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    public bool StopRecording()
    {
      if (!CheckThreadId()) return false;
      Log.Log.WriteFile("Analog:StopRecording");
      StopRecord();
      _recordingFileName = "";
      return true;
    }
    #endregion

    #region audio streams
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        List<IAudioStream> streams = new List<IAudioStream>();
        if (_filterTvAudioTuner == null) return streams;
        IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
        TVAudioMode availableAudioModes;
        tvAudioTunerInterface.GetAvailableTVAudioModes(out availableAudioModes);
        if ((availableAudioModes & (TVAudioMode.Stereo)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.Stereo;
          stream.Language = "Stereo";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.Mono)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.Mono;
          stream.Language = "Mono";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.LangA)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.LangA;
          stream.Language = "LangA";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.LangB)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.LangB;
          stream.Language = "LangB";
          streams.Add(stream);
        }
        if ((availableAudioModes & (TVAudioMode.LangC)) != 0)
        {
          AnalogAudioStream stream = new AnalogAudioStream();
          stream.AudioMode = TVAudioMode.LangC;
          stream.Language = "LangC";
          streams.Add(stream);
        }

        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public IAudioStream CurrentAudioStream
    {
      get
      {
        if (_filterTvAudioTuner == null) return null;
        IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
        TVAudioMode mode;
        tvAudioTunerInterface.get_TVAudioMode(out mode);
        List<IAudioStream> streams = AvailableAudioStreams;
        foreach (AnalogAudioStream stream in streams)
        {
          if (stream.AudioMode == mode) return stream;
        }
        return null;
      }
      set
      {
        AnalogAudioStream stream = value as AnalogAudioStream;
        if (stream != null && _filterTvAudioTuner != null)
        {
          IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
          tvAudioTunerInterface.put_TVAudioMode(stream.AudioMode);
        }
      }
    }
    #endregion


    #region IAnalogTeletextCallBack Member

    public IVbiCallback TeletextCallback
    {
      get
      {
        return _teletextCallback;
      }
      set
      {
        _teletextCallback = value;
      }
    }

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
        if (_teletextCallback != null)
        {
          _teletextCallback.OnVbiData(data, packetCount, false);
        }
        for (int i = 0; i < packetCount; ++i)
        {
          IntPtr packetPtr = new IntPtr(data.ToInt32() + i * 188);
          ProcessPacket(packetPtr);
        }
      } catch (Exception ex)
      {
        Log.Log.WriteFile(ex.ToString());
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
      if (ptr == IntPtr.Zero) return;

      _packetHeader = _tsHelper.GetHeader((IntPtr)ptr);
      if (_packetHeader.SyncByte != 0x47)
      {
        Log.Log.WriteFile("packet sync error");
        return;
      }
      if (_packetHeader.TransportError == true)
      {
        Log.Log.WriteFile("packet transport error");
        return;
      }
      // teletext
      //if (_grabTeletext)
      {
        if (_teletextDecoder != null)
        {
          _teletextDecoder.SaveData((IntPtr)ptr);
        }
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
        Log.Log.WriteFile("PID seen - type = {0}", pidType);
        OnAudioVideoEvent(pidType);
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return 0;
    }
    #endregion

    #region public helper
    public void Decompose()
    {
      Log.Log.Info("analog subch:{0} Decompose()", _subChannelId);
      if (IsRecording)
      {
        StopRecording();
      }
      if (IsTimeShifting)
      {
        StopTimeShifting();
      }
      _timeshiftFileName = "";
      _recordingFileName = "";
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
    }

    /// <summary>
    /// Should be called before tuning to a new channel
    /// resets the state
    /// </summary>
    public void OnBeforeTune()
    {
      Log.Log.WriteFile("analog subch:{0} OnBeforeTune", _subChannelId);
      if (IsTimeShifting)
      {
        _mpRecord.PauseTimeShifting(1);
      }
    }

    /// <summary>
    /// Should be called when the graph is tuned to the new channel
    /// resets the state
    /// </summary>
    public void OnAfterTune()
    {
      Log.Log.WriteFile("analog subch:{0} OnAfterTune", _subChannelId);
      if (IsTimeShifting)
      {
        _mpRecord.PauseTimeShifting(0);
      }
    }

    /// <summary>
    /// Should be called when the graph is about to start
    /// Resets the state 
    /// If graph is already running, starts the pmt grabber to grab the
    /// pmt for the new channel
    /// </summary>
    public void OnGraphStart()
    {
      Log.Log.WriteFile("analog subch:{0} OnGraphStart", _subChannelId);
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
    }

    /// <summary>
    /// Should be called when the graph has been started
    /// sets up the pmt grabber to grab the pmt of the channel
    /// </summary>
    public void OnGraphStarted()
    {
      Log.Log.WriteFile("analog subch:{0} OnGraphStarted", _subChannelId);
      _dateTimeShiftStarted = DateTime.MinValue;
    }

    #endregion

    #region private helper
    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    private bool CheckThreadId()
    {
      return true;
    }

    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    private void SetTimeShiftFileNameAndStartTimeShifting(string fileName)
    {
      if (!CheckThreadId()) return;
      _timeshiftFileName = fileName;
      Log.Log.WriteFile("analog:SetTimeShiftFileName:{0}", fileName);
      Log.Log.WriteFile("analog:SetTimeShiftFileName: uses .ts");
      ScanParameters _parameters = _card.Parameters;
      _mpRecord.SetVideoAudioObserver(this);
      _mpRecord.SetTimeShiftParams(_parameters.MinimumFiles, _parameters.MaximumFiles, _parameters.MaximumFileSize);
      _mpRecord.SetTimeShiftFileName(fileName);
      _mpRecord.StartTimeShifting();
      _dateTimeShiftStarted = DateTime.Now;
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns></returns>
    private void StartRecord(bool transportStream, string fileName)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("analog:StartRecord({0})", fileName);
      //int hr;
      if (transportStream)
      {
        Log.Log.WriteFile("dvb:SetRecording: uses .ts");
        _mpRecord.SetRecordingMode(TimeShiftingMode.TransportStream);
      } else
      {
        Log.Log.WriteFile("dvb:SetRecording: uses .mpg");
        _mpRecord.SetRecordingMode(TimeShiftingMode.ProgramStream);
      }
      _mpRecord.SetRecordingFileName(fileName);
      _mpRecord.StartRecord();
      _dateRecordingStarted = DateTime.Now;
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    private void StopRecord()
    {
      if (!CheckThreadId()) return;
      //int hr;
      Log.Log.WriteFile("analog:StopRecord()");

      _mpRecord.StopRecord();
      _recordingFileName = "";
      _dateRecordingStarted = DateTime.MinValue;
    }

    #endregion

  }
}

