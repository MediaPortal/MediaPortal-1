/* 
 *	Copyright (C) 2005 Team MediaPortal
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
//#define USEMTSWRITER
#if (UseCaptureCardDefinitions)

#region usings
using System;
using System.IO;
using System.Management;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Video.Database;
using MediaPortal.Player;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using TVCapture;
//using DirectX.Capture;
using Toub.MediaCenter.Dvrms.Metadata;
using System.Threading;
#endregion

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Class which handles recording, viewing & timeshifting for a single tvcapture card
  /// 
  /// Analog TV Tuning Interfaces:
  /// * IAMTVTuner - Control a TV tuner device. 
  /// * IAMTVAudio - Control the audio from a TV tuner. 
  /// * IAMAnalogVideoDecoder - Contains methods for selecting the digitization format, indicating the horizontal lock status, and controlling the time constant on the digitizer phase lock loop (PLL).
  /// * IAMLine21Decoder - Used to control the display of closed captioning. 
  /// * IAMWstDecoder - Used to control the dislay of World Standard Teletext (WST).
  /// </summary>

  #region RecordingFileInfo class
  public class RecordingFileInfo : IComparable<RecordingFileInfo>
  {
    public string filename;
    public FileInfo info;
    public TVRecorded record;
    #region IComparable Members

    public int CompareTo(RecordingFileInfo fi)
    {
      if (info.CreationTime < fi.info.CreationTime) return -1;
      if (info.CreationTime > fi.info.CreationTime) return 1;
      return 0;
    }

    #endregion
  }
  #endregion

  [Serializable]
  public class TVCaptureDevice
  {

    #region consts
    const string recEngineExt = ".dvr-ms";  // Change extension here when switching to TS enginge!!!

    class RecordingFinished
    {
      public string fileName = String.Empty;
    }
    #endregion

    #region variables
    string _videoCaptureDevice = String.Empty;
    string _videoCaptureMoniker = String.Empty;//@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_88010070&rev_01#3&267a616a&0&60#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}";
    string _commercialName = String.Empty;
    string _deviceId = String.Empty;
    string _captureName = String.Empty;
    bool _useForRecording = false;
    bool _useForTv = false;
    string _friendlyName = String.Empty;
    int _cardPriority = 1;
    string _recordingPath = String.Empty;
    CardTypes _cardType;

    int _defaultRecordingQuality = -1;
    DateTime _lastChannelChange = DateTime.Now;
    enum State
    {
      None,
      Initialized,
      Timeshifting,
      PreRecording,
      Recording,
      PostRecording,
      Viewing,
      Radio,
      RadioTimeshifting
    }

    [NonSerialized]
    private bool _isAnalogCable;				// #MW# Should be made serializable...??
    [NonSerialized]
    private int _defaultCountryCode;				// #MW# Should be made serializable...??
    [NonSerialized]
    private int _cardId;
    [NonSerialized]
    private State _currentGraphState = State.None;
    [NonSerialized]
    private TVRecording _currentTvRecording = null;
    [NonSerialized]
    private TVProgram _currentTvProgramRecording = null;
    [NonSerialized]
    private string _currentTvChannelName = String.Empty;
    [NonSerialized]
    private int _preRecordInterval = 0;		// In minutes
    [NonSerialized]
    private int _postRecordInterval = 0;		// In minutes
    [NonSerialized]
    private IGraph _currentGraph = null;
    [NonSerialized]
    private TVRecorded _recordedTvObject = null;
    [NonSerialized]
    private bool _isCardAllocated = false;
    [NonSerialized]
    private DateTime _timeRecordingStarted;
    [NonSerialized]
    private DateTime _timeTimeshiftingStarted;
    [NonSerialized]
    private string _currentRadioStationName = String.Empty;
    [NonSerialized]
    private int _radioSensitivity = 1;
    [NonSerialized]
    DateTime _epgTimeOutTimer = DateTime.Now;
    [NonSerialized]
    GraphHelper _graphHelper = new GraphHelper();
    [NonSerialized]
    CommandProcessor _processor;

    /// <summary>
    /// #MW#
    /// </summary>

    #endregion

    #region events
    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);
    public event OnTvRecordingHandler OnTvRecordingEnded = null;
    public event OnTvRecordingHandler OnTvRecordingStarted = null;
    #endregion

    #region ctor
    /// <summary>
    /// Default constructor
    /// </summary>
    public TVCaptureDevice()
    {
      CtorInit();
    }
    void CtorInit()
    {
      _graphHelper = new GraphHelper();
      int countryCode = 31;
      string tunerInput = "Antenna";
      using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        tunerInput = xmlReader.GetValueAsString("capture", "tuner", "Antenna");
        countryCode = xmlReader.GetValueAsInt("capture", "country", 31);
      }

      bool isCableInput = false;
      if (!tunerInput.Equals("Antenna")) isCableInput = true;

      this.IsCableInput = isCableInput;
      this.DefaultCountryCode = countryCode;
    }
    #endregion

    #region properties
    public void SetCommandProcessor(CommandProcessor processor)
    {
      _processor = processor;
    }
    public GraphHelper Graph
    {
      get
      {
        if (_graphHelper == null)
          LoadDefinitions();
        return _graphHelper;
      }
    }
    public CardTypes CardType
    {
      get { return _cardType; }
      set { _cardType = value; }
    }
    public string CommercialName
    {
      get
      {
        return _commercialName;
      }
      set
      {
        _commercialName = value;
      }
    }
    public string DeviceId
    {
      get
      {
        return _deviceId;
      }
      set
      {
        _deviceId = value;
      }
    }/*
    public string CaptureName
    {
      get
      {
        return _captureName;
      }
      set
      {
        _captureName = value;
      }
    }*/

    public string RecordingPath
    {
      get
      {
        _recordingPath = Utils.RemoveTrailingSlash(_recordingPath);
        if (_recordingPath == null || _recordingPath.Length == 0)
        {
          _recordingPath = System.IO.Directory.GetCurrentDirectory();
          _recordingPath = Utils.RemoveTrailingSlash(_recordingPath);
        }
        return _recordingPath;
      }
      set { _recordingPath = value; }
    }

    public int SignalQuality
    {
      get
      {
        if (_currentGraph == null) return 0;
        return _currentGraph.SignalQuality();
      }
    }

    public int SignalStrength
    {
      get
      {
        if (_currentGraph == null) return 0;
        return _currentGraph.SignalStrength();
      }
    }
    public int Quality
    {
      get { return _defaultRecordingQuality; }
      set { _defaultRecordingQuality = value; }
    }


    public int Priority
    {
      get { return _cardPriority; }
      set { _cardPriority = value; }
    }


    /// <summary>
    /// #MW#
    /// </summary>

    /// <summary>
    /// Will return the filtername of the capture device
    /// </summary>
    /// <returns>filtername of the capture device</returns>
    public override string ToString()
    {
      return _videoCaptureDevice;
    }

    /// <summary>
    /// #MW#
    /// </summary>
    public bool IsCableInput
    {
      get { return _isAnalogCable; }
      set { _isAnalogCable = value; }
    }

    /// <summary>
    /// #MW#
    /// </summary>
    public int DefaultCountryCode
    {
      get { return _defaultCountryCode; }
      set { _defaultCountryCode = value; }
    }


    public string FriendlyName
    {
      get { return _friendlyName; }
      set { _friendlyName = value; }
    }

    /// <summary>
    /// property which returns the date&time when recording was started
    /// </summary>
    public DateTime TimeRecordingStarted
    {
      get { return _timeRecordingStarted; }
    }

    /// <summary>
    /// property which returns the date&time when timeshifting was started
    /// </summary>
    public DateTime TimeShiftingStarted
    {
      get { return _timeTimeshiftingStarted; }
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the TV capture card 
    /// </summary>
    public string VideoDevice
    {
      get { return _videoCaptureDevice; }
      set { _videoCaptureDevice = value; }
    }
    /// <summary>
    /// Property to get/set the (graphedit) monikername name of the TV capture card 
    /// </summary>
    public string VideoDeviceMoniker
    {
      get { return _videoCaptureMoniker; }
      set { _videoCaptureMoniker = value; }
    }


    /// <summary>
    /// Property to specify if this card can be used for TV viewing or not
    /// </summary>
    public bool UseForTV
    {
      get
      {
        if (Allocated) return false;
        return _useForTv;
      }
      set { _useForTv = value; }
    }

    /// <summary>
    /// Property to specify if this card is allocated by other processes
    /// like MyRadio or not.
    /// </summary>
    public bool Allocated
    {
      get { return _isCardAllocated; }
      set
      {
        _isCardAllocated = value;
      }
    }

    /// <summary>
    /// Property to specify if this card can be used for recording or not
    /// </summary>
    public bool UseForRecording
    {
      get
      {
        if (Allocated) return false;
        return _useForRecording;
      }
      set { _useForRecording = value; }
    }

    /// <summary>
    /// Property to specify the ID of this card
    /// </summary>
    public int ID
    {
      get { return _cardId; }
      set
      {
        CtorInit();
        _cardId = value;
        _currentGraphState = State.Initialized;
      }
    }

    /// <summary>
    /// Property which returns true if this card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get
      {
        if (_currentGraphState == State.PreRecording) return true;
        if (_currentGraphState == State.Recording) return true;
        if (_currentGraphState == State.PostRecording) return true;
        return false;
      }
    }


    /// <summary>
    /// Property which returns true if this card is currently has a teletext
    /// </summary>
    public bool HasTeletext
    {
      get
      {
        if (_currentGraph == null) return false;
        return _currentGraph.HasTeletext();
      }
    }


    public bool IsEpgGrabbing
    {
      get
      {
        if (_currentGraph == null)
        {
          return false;
        }
        bool result = _currentGraph.IsEpgGrabbing();
        return result;
      }
    }
    public bool IsEpgFinished
    {
      get
      {
        if (!IsEpgGrabbing)
        {
          return true;
        }
        TimeSpan ts = DateTime.Now - _epgTimeOutTimer;
        if (ts.TotalMinutes >= 10) return true;
        bool result = _currentGraph.IsEpgDone();
        return result;
      }
    }

    /// <summary>
    /// Property which returns true if this card is currently timeshifting
    /// </summary>
    public bool IsTimeShifting
    {
      get
      {
        if (IsRecording) return true;
        if (_currentGraphState == State.Timeshifting) return true;
        if (_currentGraphState == State.RadioTimeshifting) return true;
        return false;
      }
    }

    /// <summary>
    /// Property which returns the current TVRecording schedule when its recording
    /// otherwise it returns null
    /// </summary>
    /// <seealso>MediaPortal.TV.Database.TVRecording</seealso>
    public TVRecording CurrentTVRecording
    {
      get
      {
        if (!IsRecording) return null;
        return _currentTvRecording;
      }
      set
      {
        _currentTvRecording = value;
      }
    }

    /// <summary>
    /// Property which returns the current TVProgram when its recording
    /// otherwise it returns null
    /// </summary>
    /// <seealso>MediaPortal.TV.Database.TVProgram</seealso>
    public TVProgram CurrentProgramRecording
    {
      get
      {
        if (!IsRecording) return null;
        return _currentTvProgramRecording;
      }
    }

    /// <summary>
    /// Property which returns true when we're in the post-processing stage of a recording
    /// if we're not recording it returns false;
    /// if we're recording but are NOT in the post-processing stage it returns false;
    /// </summary>
    public bool IsPostRecording
    {
      get { return _currentGraphState == State.PostRecording; }
    }
    public bool IsRadio
    {
      get { return (_currentGraphState == State.Radio || _currentGraphState == State.RadioTimeshifting); }
    }

    /// <summary>
    /// Propery to get/set the name of the current TV channel. 
    /// If the TV channelname is changed and the card is timeshifting then it will
    /// tune to the newly specified tv channel
    /// </summary>
    public string TVChannel
    {
      get { return _currentTvChannelName; }
      set
      {
        if (value == null)
          value = GetFirstChannel();
        else if (value != null && value.Length == 0)
          value = GetFirstChannel();

        if (value.Equals(_currentTvChannelName)) return;

        Log.Write("TVCapture: change channel to :{0}", value);
        if (!IsRecording)
        {
          bool isFullScreen = GUIGraphicsContext.IsFullScreenVideo;
          _currentTvChannelName = value;
          if (_currentGraph != null)
          {
            TVChannel channel = GetChannel(_currentTvChannelName);
            if (_currentGraph.ShouldRebuildGraph(channel))
            {
              RebuildGraph();
              // for ss2: restore full screen
              //if (_videoCaptureDevice == "B2C2 MPEG-2 Source")
              GUIGraphicsContext.IsFullScreenVideo = isFullScreen;
              _lastChannelChange = DateTime.Now;
              return;
            }
#if USEMTSWRITER
						//stop playback before we zap channels
						if (IsTimeShifting && !View)
						{
							if (g_Player.Playing && g_Player.CurrentFile == _processor.GetTimeShiftFileName(ID-1))
							{
								g_Player.Stop();
								//if (!g_Player.Paused)
								//	g_Player.Pause();
							}
						}
#endif
            _currentGraph.TuneChannel(channel);
            _lastChannelChange = DateTime.Now;
            if (IsTimeShifting)
            {
              _timeTimeshiftingStarted = DateTime.Now;
            }

#if !USEMTSWRITER
            if (IsTimeShifting && !View)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
#endif
          }//if (_currentGraph != null)
        }//if (!IsRecording)
      }
    }


    /// <summary>
    /// Property to turn on/off tv viewing
    /// </summary>
    public bool View
    {
      get
      {
        return (_currentGraphState == State.Viewing);
      }
    }
    public void StopViewing()
    {
      StopEpgGrabbing();
      if (_currentGraphState == State.Viewing)
      {
        Log.WriteFile(Log.LogType.Capture, "TVCapture.Stop Viewing() Card:{0} {1}", ID, _currentTvChannelName);
        _currentGraph.StopViewing();
        //DeleteGraph();TESTTEST
      }
      _currentGraphState = State.Initialized;

    }
    public void StopEpgGrabbing()
    {
      if (!IsEpgGrabbing) return;
      if (_currentGraph == null) return;
      _currentGraph.StopEpgGrabbing();
      _currentGraphState = State.Initialized;
    }
    public bool StartViewing(string channelName)
    {
      StopEpgGrabbing();

      if (_currentGraphState == State.Viewing)
      {
        TVChannel = channelName;
        return true;
      }
      if (IsRecording) return false;
      //DeleteGraph();TESTTEST

      if (CreateGraph())
      {
        Log.WriteFile(Log.LogType.Capture, "TVCapture.Start Viewing() Card:{0} :{1}", ID, channelName);
        TVChannel chan = GetChannel(channelName);
        if (_currentGraph.StartViewing(chan))
        {
          SetTvSettings();
          _currentTvChannelName = channelName;
          _currentGraphState = State.Viewing;
          _lastChannelChange = DateTime.Now;
          return true;
        }
      }
      return false;
    }

    public PropertyPageCollection PropertyPages
    {
      get
      {
        if (_currentGraph == null) return null;
        return _currentGraph.PropertyPages();
      }
    }

    public NetworkType Network
    {
      get
      {
        if (_currentGraph == null)
        {
          _currentGraph = GraphFactory.CreateGraph(this);
          NetworkType netType = _currentGraph.Network();
          _currentGraph = null;
          return netType;
        }
        return _currentGraph.Network();
      }
    }
    /// <summary>
    /// Property indiciating if the card supports timeshifting
    /// </summary>
    /// <returns>boolean indiciating if the graph supports timeshifting</returns>
    public bool SupportsTimeShifting
    {
      get
      {
        bool result = false;
        if (_currentGraph == null)
        {
          _currentGraph = GraphFactory.CreateGraph(this);
          if (_currentGraph == null) return false;
          result = _currentGraph.SupportsTimeshifting();
          _currentGraph = null;
        }
        else
        {
          result = _currentGraph.SupportsTimeshifting();
        }

        return result;
      }
    }


    public string RecordingFileName
    {
      get
      {
        if (!IsRecording)
          return String.Empty;
        if (_recordedTvObject == null)
          return String.Empty;
        return _recordedTvObject.FileName;
      }
    }

    public string RadioStation
    {
      get { return _currentRadioStationName; }
    }
    /// <summary>
    /// Property to set Radio tuning sensitivity.
    /// sensitivity range from 1MHz for value 1 to 0.1MHZ for value 10
    /// </summary>
    public int RadioSensitivity
    {
      get
      {
        return _radioSensitivity;
      }
      set
      {
        _radioSensitivity = value;
      }
    }
    public string TimeShiftFileName
    {
      get
      {
        if (_currentGraph != null)
        {
          if (IsRadio)
            return _currentGraph.RadioTimeshiftFileName();
          else
            return _currentGraph.TvTimeshiftFileName();
        }
        else
        {
          IGraph g = GraphFactory.CreateGraph(this);
          if (IsRadio)
            return g.RadioTimeshiftFileName();
          else
            return g.TvTimeshiftFileName();
        }
      }
    }
    public IBaseFilter AudiodeviceFilter
    {
      get
      {
        if (_currentGraph == null) return null;
        return _currentGraph.AudiodeviceFilter();
      }
    }
    #endregion

    #region public members

    public bool LoadDefinitions()
    {
      if (_graphHelper == null)
      {
        CtorInit();
        _graphHelper = new GraphHelper();
      }
      _graphHelper.DeviceId = DeviceId;
      _graphHelper.CommercialName = CommercialName;
      //_graphHelper.CaptureName = CaptureName;
      return _graphHelper.LoadDefinitions(VideoDevice, VideoDeviceMoniker);
    }


    /// <summary>
    /// Property which returns the available audio languages
    /// </summary>
    public ArrayList GetAudioLanguageList()
    {
      if (_currentGraph == null) return null;
      return _currentGraph.GetAudioLanguageList();
    }

    /// <summary>
    /// Property which gets the current audio language
    /// </summary>
    public int GetAudioLanguage()
    {
      if (_currentGraph == null) return -1;
      return _currentGraph.GetAudioLanguage();
    }

    /// <summary>
    /// Property which sets the new audio language
    /// </summary>
    public void SetAudioLanguage(int audioPid)
    {
      if (_currentGraph == null) return;
      _currentGraph.SetAudioLanguage(audioPid);
    }


    public FilterDefinition GetTvFilterDefinition(string filterCategory)
    {
      foreach (FilterDefinition fd in Graph.TvFilterDefinitions)
      {
        if (String.Compare(fd.Category, filterCategory, true) == 0) return fd;
      }
      return null;
    }
    public void GrabEpg(TVChannel channel)
    {
      if (CreateGraph())
      {
        _currentGraph.GrabEpg(channel);
        _epgTimeOutTimer = DateTime.Now;
      }
    }

    /// <summary>
    /// This method can be used to stop the current recording.
    /// After recording is stopped the card will return to timeshifting mode
    /// </summary>
    public void StopRecording()
    {
      if (!IsRecording) return;

      Log.WriteFile(Log.LogType.Capture, "TVCapture.StopRecording() Card:{0}", ID);
      // todo : stop recorder
      _currentGraph.StopRecording();

      _recordedTvObject.End = Utils.datetolong(DateTime.Now);
      TVDatabase.AddRecordedTV(_recordedTvObject);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        bool addMovieToDatabase = xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);
        if (addMovieToDatabase)
        {
          //add new recorded show to video database
          int movieid = VideoDatabase.AddMovieFile(_recordedTvObject.FileName);
          IMDBMovie movieDetails = new IMDBMovie();
          if (movieid >= 0)
          {
            movieDetails.Title = _recordedTvObject.Title;
            movieDetails.Genre = _recordedTvObject.Genre;
            movieDetails.Plot = _recordedTvObject.Description;
            movieDetails.Year = _recordedTvObject.StartTime.Year;
            VideoDatabase.SetMovieInfoById(movieid, ref movieDetails);
          }
        }
      }
      // back to timeshifting state
      _currentGraphState = State.Timeshifting;

      _recordedTvObject = null;

      if (OnTvRecordingEnded != null)
      {
        OnTvRecordingEnded(RecordingFileName, _currentTvRecording, _currentTvProgramRecording);
      }
      //Log.Write("TVCapture.StopRecording():_currentTvProgramRecording=null");
      // cleanup...
      _currentTvProgramRecording = null;
      _currentTvRecording = null;
      _preRecordInterval = 0;
      _postRecordInterval = 0;

      if (!g_Player.Playing)
      {
        //DeleteGraph();TESTTEST
        _currentGraph.StopTimeShifting();//TESTTEST
        return;
      }
      string timeshiftFilename = String.Format(@"{0}\card{1}\{2}", RecordingPath, ID, TimeShiftFileName);
      if (!g_Player.CurrentFile.Equals(timeshiftFilename))
      {
        //DeleteGraph();TESTTEST
        _currentGraph.StopTimeShifting();//TESTTEST
        return;
      }
    }//StopRecording()

    /// <summary>
    /// This method can be used to start a new recording
    /// </summary>
    /// <param name="recording">TVRecording schedule to record</param>
    /// <param name="currentProgram">TVProgram to record</param>
    /// <param name="iPreRecordInterval">Pre record interval</param>
    /// <param name="iPostRecordInterval">Post record interval</param>
    /// <remarks>
    /// The card will start recording live tv to the harddisk and create a new
    /// <see>MediaPortal.TV.Database.TVRecorded</see> record in the TVDatabase 
    /// which contains all details about the new recording like
    /// start-end time, filename, title,description,channel
    /// </remarks>
    /// <seealso>MediaPortal.TV.Database.TVRecorded</seealso>
    /// <seealso>MediaPortal.TV.Database.TVProgram</seealso>
    public void Record(TVRecording recording, TVProgram currentProgram, int iPreRecordInterval, int iPostRecordInterval)
    {
      if (_currentGraphState != State.Initialized && _currentGraphState != State.Timeshifting)
      {
        //DeleteGraph();TESTTEST
      }

      StopEpgGrabbing();
      if (!UseForRecording) return;

      if (currentProgram != null)
        _currentTvProgramRecording = currentProgram.Clone();
      //Log.Write("dev.Record():_currentTvProgramRecording={0}", _currentTvProgramRecording);
      _currentTvRecording = new TVRecording(recording);
      _preRecordInterval = iPreRecordInterval;
      _postRecordInterval = iPostRecordInterval;
      _currentTvChannelName = recording.Channel;

      Log.WriteFile(Log.LogType.Capture, "TVCapture.Record() Card:{0} {1} on {2} from {3}-{4}", ID, recording.Title, _currentTvChannelName, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString());
      // create sink graph
      if (CreateGraph())
      {
        bool bContinue = false;
        if (_currentGraph.SupportsTimeshifting())
        {
          if (StartTimeShifting(recording.Channel))
          {
            bContinue = true;
          }
        }
        else
        {
          bContinue = true;
        }

        if (bContinue)
        {
          // start sink graph
          if (StartRecording(recording))
          {
          }
        }
      }
      //todo handle errors....
    }

    /// <summary>
    /// Process() method gets called on a regular basis by the Recorder
    /// Here we check if we're currently recording and 
    /// ifso if the recording should be stopped.
    /// </summary>
    public void Process()
    {
      // set postrecording status
      if (IsRecording)
      {
        if (_currentTvRecording != null)
        {
          if (_currentTvRecording.IsRecordingAtTime(DateTime.Now, _currentTvProgramRecording, _preRecordInterval, _postRecordInterval))
          {
            if (!_currentTvRecording.IsRecordingAtTime(DateTime.Now, _currentTvProgramRecording, _preRecordInterval, 0))
            {
              _currentGraphState = State.PostRecording;
            }
            else if (!_currentTvRecording.IsRecordingAtTime(DateTime.Now, _currentTvProgramRecording, 0, _postRecordInterval))
            {
              _currentGraphState = State.PreRecording;
            }
            else
            {
              _currentGraphState = State.Recording;
            }
          }
          else
          {
            //recording ended
            Log.WriteFile(Log.LogType.Capture, "TVCapture.Proces() Card:{0} recording has ended '{1}' on channel:{2} from {3}-{4} id:{5} _cardPriority:{6} quality:{7}",
              ID,
              _currentTvRecording.Title, _currentTvRecording.Channel,
              _currentTvRecording.StartTime.ToLongTimeString(), _currentTvRecording.EndTime.ToLongTimeString(),
              _currentTvRecording.ID, _currentTvRecording.Priority, _currentTvRecording.Quality.ToString());
            StopRecording();
          }
        }
      }

      if (_currentGraph != null)
      {
        _currentGraph.Process();
      }
    }

    /// <summary>
    /// Method to cleanup any resources and free the card. 
    /// Used by the recorder when its stopping or when external assemblies
    /// like MyRadio want access to the capture card
    /// </summary>
    public void Stop()
    {
      Log.WriteFile(Log.LogType.Capture, "TVCapture.Stop() Card:{0}", ID);
      StopRecording();
      StopTimeShifting();
      StopViewing();
      StopEpgGrabbing();
      DeleteGraph();
    }

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public bool CreateGraph()
    {
      if (Allocated) return false;
      if (_currentGraph == null)
      {
        LoadContrastGammaBrightnessSettings();
        Log.WriteFile(Log.LogType.Capture, "TVCapture.CreateGraph() Card:{0}", ID);
        _currentGraph = GraphFactory.CreateGraph(this);
        if (_currentGraph == null) return false;
        return _currentGraph.CreateGraph(Quality);
      }
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool DeleteGraph()
    {
      if (_currentGraph != null)
      {
        SaveContrastGammaBrightnessSettings();
        Log.WriteFile(Log.LogType.Capture, "TVCapture.DeleteGraph() Card:{0}", ID);
        _currentGraph.DeleteGraph();
        _currentGraph = null;
      }
      _currentTvChannelName = "";
      _timeTimeshiftingStarted = DateTime.MinValue;
      _currentGraphState = State.Initialized;
      return true;
    }

    /// <summary>
    /// Starts timeshifting 
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartTimeShifting(string channelName)
    {

      StopEpgGrabbing();
      if (IsRecording) return false;

      Log.WriteFile(Log.LogType.Capture, "TVCapture.StartTimeShifting() Card:{0} :{1}", ID, channelName);
      TVChannel channel = GetChannel(channelName);

      if (_currentGraphState == State.Timeshifting)
      {
        if (_currentGraph.GetChannelNumber() != channel.Number)
        {
          if (!_currentGraph.ShouldRebuildGraph(channel))
          {
            _timeTimeshiftingStarted = DateTime.Now;
            _currentGraph.TuneChannel(channel);
            _lastChannelChange = DateTime.Now;
            _currentTvChannelName = channelName;
            return true;
          }
        }
        else return true;
      }

      
      if (_currentGraphState ==State.Viewing)
      {
        StopViewing();
      } 
      else if (_currentGraphState != State.Initialized)
      {
        //DeleteGraph();TESTTEST
      }
      if (!CreateGraph()) return false;



      string strFileName = _processor.GetTimeShiftFileName(ID - 1);



      //Log.WriteFile(Log.LogType.Capture, "Card:{0} timeshift to file:{1}", ID, strFileName);
      bool bResult = _currentGraph.StartTimeShifting(channel, strFileName);
      if (bResult == true)
      {
        _timeTimeshiftingStarted = DateTime.Now;
        _currentGraphState = State.Timeshifting;
        _currentTvChannelName = channelName;
        SetTvSettings();
        _lastChannelChange = DateTime.Now;
      }
      return bResult;
    }

    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    public bool StopTimeShifting()
    {
      if (!IsTimeShifting) return false;

      //stopping timeshifting will also remove the live.tv file 
      Log.WriteFile(Log.LogType.Capture, "TVCapture.StopTimeShifting() Card:{0}", ID);
      _currentGraph.StopTimeShifting();
      string fileName = _processor.GetTimeShiftFileName(ID - 1);
      Utils.FileDelete(fileName);
      _currentGraphState = State.Initialized;
      return true;
    }

    public void Tune(TVChannel channel)
    {
      if (_currentGraphState != State.Viewing) return;
      Log.Write("TVCapture.Tune({0}", channel.Name);
      _currentGraph.TuneChannel(channel);
      _lastChannelChange = DateTime.Now;
      _currentTvChannelName = channel.Name;
    }


    public long VideoFrequency()
    {
      if (_currentGraph == null) return 0;
      return _currentGraph.VideoFrequency();
    }

    public bool SignalPresent()
    {
      if (_currentGraph == null) return false;
      return _currentGraph.SignalPresent();
    }

    public bool ViewChannel(TVChannel channel)
    {

      StopEpgGrabbing();
      if (_currentGraph == null)
      {
        if (!CreateGraph()) return false;
      }
      _currentGraph.StartViewing(channel);
      SetTvSettings();

      return true;
    }

    public bool SupportsFrameSize(Size framesize)
    {
      if (_currentGraph == null) return false;
      return _currentGraph.SupportsFrameSize(framesize);
    }


    public void Tune(object tuningObject, int disecqNo)
    {
      if (_currentGraph == null) return;
      _currentGraph.Tune(tuningObject, disecqNo);
    }
    public void StoreTunedChannels(bool radio, bool tv, ref int newTvChannels, ref int updatedTvChannels, ref int newRadioChannels, ref int updatedRadioChannels)
    {
      if (_currentGraph == null) return;
      _currentGraph.StoreChannels(ID, radio, tv, ref newTvChannels, ref updatedTvChannels, ref newRadioChannels, ref updatedRadioChannels);
    }
    public void StopRadio()
    {
      if (!IsRadio) return;
      if (_currentGraph==null) return;
        
      _currentGraph.StopRadio();
      _currentGraphState = State.Initialized;
    }

    public void StartRadio(RadioStation station)
    {
      if (!IsRadio) 
      {
        //DeleteGraph();TESTTEST
        StopTimeShifting();
        StopViewing();
        if (!CreateGraph()) return;
        _currentGraphState = State.Radio;
        _currentGraph.StartRadio(station);
        if (_currentGraph.IsTimeShifting())
        {
          _currentGraphState = State.RadioTimeshifting;
        }
      }
      else
      {
        _currentGraph.TuneRadioChannel(station);
      }
      _currentRadioStationName = station.Name;
    }
    public void TuneRadioChannel(RadioStation station)
    {
      if (!IsRadio) 
      {
        StartRadio(station);
      }
      else
      {
        _currentGraph.TuneRadioChannel(station);
      }
      _currentRadioStationName = station.Name;
    }
    public void TuneRadioFrequency(int frequency)
    {
      if (!IsRadio)
      {
        StartRadio(new RadioStation());
        _currentGraph.TuneRadioFrequency(frequency);
      }
      else
      {
        _currentGraph.TuneRadioFrequency(frequency);
      }
    }

    public void RadioChannelMinMax(out int chanmin, out int chanmax)
    {
      bool deleteGraph = false;
      if (_currentGraph == null)
      {
        if (!CreateGraph())
        {
          chanmin = -1;
          chanmax = -1;
          return;
        }
        deleteGraph = true;
      }
      _currentGraph.RadioChannelMinMax(out chanmin, out chanmax);

      if (deleteGraph)
      {
        DeleteGraph();
      }
    }
    public void TVChannelMinMax(out int chanmin, out int chanmax)
    {
      bool deleteGraph = false;
      if (_currentGraph == null)
      {
        if (!CreateGraph())
        {
          chanmin = -1;
          chanmax = -1;
          return;
        }
        deleteGraph = true;
      }
      _currentGraph.TVChannelMinMax(out chanmin, out chanmax);

      if (deleteGraph)
      {
        DeleteGraph();
      }
    }
    public void GrabTeletext(bool yesNo)
    {
      if (_currentGraph == null) return;
      _currentGraph.GrabTeletext(yesNo);
    }

    #endregion

    #region private members
    void RebuildGraph()
    {
      Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() Card:{0} chan:{1}", ID, _currentTvChannelName);

      //stop playback of this channel
      if (g_Player.Playing && g_Player.CurrentFile == _processor.GetTimeShiftFileName(ID - 1))
      {
        //Log.WriteFile(Log.LogType.Capture, "TVCaptureDevice.Rebuildgraph() stop media");

        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);

        //wait till max 500msec until player has stopped...
        int counter = 0;
        while ( (g_Player.Playing || VMR9Util.g_vmr9!=null )&& counter < 20)
        {
          System.Threading.Thread.Sleep(100);
          counter++;
        }
        //Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() player stopped:{0}",
        //g_Player.Playing);
      }

      if (_currentGraph != null)
      {

        //Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() delete graph");
        _currentGraph.DeleteGraph();
        _currentGraph = null;
        //Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() graph deleted");
      }

      TVChannel channel = GetChannel(_currentTvChannelName);
      if (_currentGraphState == State.Timeshifting)
      {
        //Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() recreate timeshifting graph");
        _currentGraph = GraphFactory.CreateGraph(this);
        bool isCreated = _currentGraph.CreateGraph(Quality);
        if (!isCreated)
        {
          _currentGraph = null;
          return;
        }
        //Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() start timeshifting");
        _currentGraph.StartTimeShifting(channel, _processor.GetTimeShiftFileName(ID - 1));
        _lastChannelChange = DateTime.Now;

        //play timeshift file again
        //Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() start playing timeshift file");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_FILE, 0, 0, 0, 0, 0, null);
        msg.Label = _processor.GetTimeShiftFileName(ID - 1);
        GUIGraphicsContext.SendMessage(msg);

      }
      else
      {
        // Log.WriteFile(Log.LogType.Capture, "TvCaptureDevice:RebuildGraph() recreate viewing graph");
        _currentGraph = GraphFactory.CreateGraph(this);
        bool isCreated = _currentGraph.CreateGraph(Quality);
        if (!isCreated)
        {
          _currentGraph = null;
          return;
        }
        _currentGraph.StartViewing(channel);
        _lastChannelChange = DateTime.Now;
      }
      // Log.WriteFile(Log.LogType.Capture, "Card:{0} rebuild graph done", ID);
    }

    string StripIllegalChars(string recordingAttribute)
    {
      if (recordingAttribute == null) return String.Empty;
      if (recordingAttribute.Length == 0) return String.Empty;
      recordingAttribute = recordingAttribute.Replace(":", " ");
      recordingAttribute = recordingAttribute.Replace(";", " ");
      return recordingAttribute;
    }
    Hashtable GetRecordingAttributes()
    {
      // set the meta data in the dvr-ms or .wmv file
      TimeSpan ts = (_recordedTvObject.EndTime - _recordedTvObject.StartTime);
      Hashtable propsToSet = new Hashtable();

      propsToSet.Add("channel", new MetadataItem("channel", StripIllegalChars(_recordedTvObject.Channel), MetadataItemType.String));

      propsToSet.Add("recordedby", new MetadataItem("recordedby", "Mediaportal", MetadataItemType.String));

      if (_recordedTvObject.Title != null && _recordedTvObject.Title.Length > 0)
        propsToSet.Add("title", new MetadataItem("title", StripIllegalChars(_recordedTvObject.Title), MetadataItemType.String));

      if (_recordedTvObject.Genre != null && _recordedTvObject.Genre.Length > 0)
        propsToSet.Add("genre", new MetadataItem("genre", StripIllegalChars(_recordedTvObject.Genre), MetadataItemType.String));

      if (_recordedTvObject.Description != null && _recordedTvObject.Description.Length > 0)
        propsToSet.Add("description", new MetadataItem("details", StripIllegalChars(_recordedTvObject.Description), MetadataItemType.String));

      propsToSet.Add("id", new MetadataItem("id", (uint)_recordedTvObject.ID, MetadataItemType.Dword));
      propsToSet.Add("cardno", new MetadataItem("cardno", (uint)this.ID, MetadataItemType.Dword));
      propsToSet.Add("duration", new MetadataItem("seconds", (uint)ts.TotalSeconds, MetadataItemType.Dword));
      propsToSet.Add("start", new MetadataItem("start", _recordedTvObject.Start.ToString(), MetadataItemType.String));
      propsToSet.Add("end", new MetadataItem("end", _recordedTvObject.End.ToString(), MetadataItemType.String));

      return propsToSet;
    }//void GetRecordingAttributes()


    /// <summary>
    /// Starts recording live TV to a file
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the start of the current tv program till the moment recording is stopped again
    /// </remarks>
    bool StartRecording(TVRecording recording)
    {
      Log.WriteFile(Log.LogType.Capture, "TVCapture.StartRecording() Card:{0}  content:{1}", ID, recording.IsContentRecording);

      TVProgram prog = null;
      DateTime dtNow = DateTime.Now.AddMinutes(_preRecordInterval);

      //for reference recordings, find program which runs now
      if (!recording.IsContentRecording)
        dtNow = DateTime.Now;

      TVProgram currentRunningProgram = null;
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (chan.Name == _currentTvChannelName)
        {
          prog = chan.GetProgramAt(dtNow.AddMinutes(1));
          break;
        }
      }
      if (prog != null) currentRunningProgram = prog.Clone();


      DateTime timeProgStart = new DateTime(1971, 11, 6, 20, 0, 0, 0);
      string fileName = string.Empty;
      string subDirectory = string.Empty;
      string fullPath = RecordingPath;
      if (currentRunningProgram != null)
      {
        string strInput = string.Empty;
        string recFileFormat = string.Empty;
        string recDirFormat = string.Empty;
        bool isMovie = false;
        if (recording.RecType == TVRecording.RecordingType.Once)
          isMovie = true;

        timeProgStart = currentRunningProgram.StartTime;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          if (isMovie)
            strInput = xmlreader.GetValueAsString("capture", "moviesformat", string.Empty);
          else
            strInput = xmlreader.GetValueAsString("capture", "seriesformat", string.Empty);

        strInput = Utils.ReplaceTag(strInput, "%channel%", Utils.MakeFileName(currentRunningProgram.Channel), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%title%", Utils.MakeFileName(currentRunningProgram.Title), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%name%", Utils.MakeFileName(currentRunningProgram.Episode), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%series%", Utils.MakeFileName(currentRunningProgram.SeriesNum), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%episode%", Utils.MakeFileName(currentRunningProgram.EpisodeNum), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%part%", Utils.MakeFileName(currentRunningProgram.EpisodePart), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%date%", Utils.MakeFileName(currentRunningProgram.StartTime.Date.ToShortDateString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%start%", Utils.MakeFileName(currentRunningProgram.StartTime.ToShortTimeString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%end%", Utils.MakeFileName(currentRunningProgram.EndTime.ToShortTimeString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%genre%", Utils.MakeFileName(currentRunningProgram.Genre), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startday%", Utils.MakeFileName(currentRunningProgram.StartTime.Day.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startmonth%", Utils.MakeFileName(currentRunningProgram.StartTime.Month.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startyear%", Utils.MakeFileName(currentRunningProgram.StartTime.Year.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%starthh%", Utils.MakeFileName(currentRunningProgram.StartTime.Hour.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startmm%", Utils.MakeFileName(currentRunningProgram.StartTime.Minute.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endday%", Utils.MakeFileName(currentRunningProgram.EndTime.Day.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endmonth%", Utils.MakeFileName(currentRunningProgram.EndTime.Month.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%startyear%", Utils.MakeFileName(currentRunningProgram.EndTime.Year.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endhh%", Utils.MakeFileName(currentRunningProgram.EndTime.Hour.ToString()), "unknown");
        strInput = Utils.ReplaceTag(strInput, "%endmm%", Utils.MakeFileName(currentRunningProgram.EndTime.Minute.ToString()), "unknown");

        int index = strInput.LastIndexOf('\\');
        if (index != -1)
        {
          subDirectory = strInput.Substring(0, index).Trim();
          fileName = strInput.Substring(index + 1).Trim();
        }
        else
          fileName = strInput.Trim();


        if (subDirectory != string.Empty)
        {
          subDirectory = Utils.RemoveTrailingSlash(subDirectory);
          subDirectory = Utils.MakeDirectoryPath(subDirectory);
          fullPath = RecordingPath + "\\" + subDirectory;
          if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);
        }
        if (fileName == string.Empty)
        {
          DateTime dt = currentRunningProgram.StartTime;
          fileName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                    currentRunningProgram.Channel, currentRunningProgram.Title,
                                    dt.Year, dt.Month, dt.Day,
                                    dt.Hour,
                                    dt.Minute,
                                    DateTime.Now.Minute, DateTime.Now.Second);
        }
        fileName = Utils.MakeFileName(fileName);
        if (File.Exists(fullPath + "\\" + fileName + recEngineExt))
        {
          int i = 1;
          while (File.Exists(fullPath + "\\" + fileName + "_" + i.ToString() + recEngineExt))
            ++i;
          fileName += "_" + i.ToString();
        }
        fileName += recEngineExt;
      }
      else
      {
        DateTime dt = DateTime.Now;
        fileName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}{9}",
          _currentTvChannelName, _currentTvRecording.Title,
          dt.Year, dt.Month, dt.Day,
          dt.Hour,
          dt.Minute,
          DateTime.Now.Minute, DateTime.Now.Second,
          recEngineExt);
      }

      string fullFileName = String.Format(@"{0}\{1}", fullPath, Utils.MakeFileName(fileName));
      Log.Write("Recorder: recording to {0}", fullFileName);

      TVChannel channel = GetChannel(_currentTvChannelName);

      _recordedTvObject = new TVRecorded();
      _recordedTvObject.Start = Utils.datetolong(DateTime.Now);
      _recordedTvObject.Channel = _currentTvChannelName;
      _recordedTvObject.FileName = fullFileName;
      _recordedTvObject.KeepRecordingMethod = recording.KeepRecordingMethod;
      _recordedTvObject.KeepRecordingTill = recording.KeepRecordingTill;
      if (currentRunningProgram != null)
      {
        _recordedTvObject.Title = currentRunningProgram.Title;
        _recordedTvObject.Genre = currentRunningProgram.Genre;
        _recordedTvObject.Description = currentRunningProgram.Description;
        _recordedTvObject.End = currentRunningProgram.End;
      }
      else
      {
        _recordedTvObject.Title = String.Empty;
        _recordedTvObject.Genre = String.Empty;
        _recordedTvObject.Description = String.Empty;
        _recordedTvObject.End = Utils.datetolong(DateTime.Now.AddHours(2));
      }

      Hashtable attribtutes = GetRecordingAttributes();
      if (timeProgStart < _lastChannelChange) timeProgStart = _lastChannelChange;
      bool bResult = _currentGraph.StartRecording(attribtutes, recording, channel, ref fullFileName, recording.IsContentRecording, timeProgStart);
      _recordedTvObject.FileName = fullFileName;

      _timeRecordingStarted = DateTime.Now;
      _currentGraphState = State.Recording;
      SetTvSettings();

      if (OnTvRecordingStarted != null)
      {
        OnTvRecordingStarted(RecordingFileName, _currentTvRecording, _currentTvProgramRecording);
      }
      return bResult;
    }

    string GetFirstChannel()
    {
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (chan.Number < (int)ExternalInputs.svhs) return chan.Name;
      }
      foreach (TVChannel chan in channels)
      {
        return chan.Name;
      }
      return String.Empty;
    }
    /// <summary>
    /// Returns the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number (or 0 if channelname is unknown)</returns>
    /// <remarks>
    /// Channel names and numbers are stored in the TVDatabase
    /// </remarks>
    TVChannel GetChannel(string strChannelName)
    {
      TVChannel retChannel = new TVChannel();
      retChannel.Number = 0;
      retChannel.Name = strChannelName;
      retChannel.ID = 0;
      retChannel.TVStandard = AnalogVideoStandard.None;
      retChannel.Country = _defaultCountryCode;

      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (String.Compare(strChannelName, chan.Name, true) == 0)
        {
          if (chan.Country <= 0)
            chan.Country = _defaultCountryCode;
          return chan;
        }
      }
      return retChannel;
    }
    void SetTvSettings()
    {
      int gamma = GUIGraphicsContext.Gamma;
      GUIGraphicsContext.Gamma = -2;
      GUIGraphicsContext.Gamma = -1;
      if (gamma >= 0)
        GUIGraphicsContext.Gamma = gamma;
    }

    void LoadContrastGammaBrightnessSettings()
    {
      try
      {
        string filename = String.Format(@"database\card_{0}.xml", _friendlyName);
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
        {
          int contrast = xmlreader.GetValueAsInt("tv", "contrast", -1);
          int brightness = xmlreader.GetValueAsInt("tv", "brightness", -1);
          int gamma = xmlreader.GetValueAsInt("tv", "gamma", -1);
          int saturation = xmlreader.GetValueAsInt("tv", "saturation", -1);
          int sharpness = xmlreader.GetValueAsInt("tv", "sharpness", -1);
          GUIGraphicsContext.Contrast = contrast;
          GUIGraphicsContext.Brightness = brightness;
          GUIGraphicsContext.Gamma = gamma;
          GUIGraphicsContext.Saturation = saturation;
          GUIGraphicsContext.Sharpness = sharpness;
        }
      }
      catch (Exception)
      { }
    }
    void SaveContrastGammaBrightnessSettings()
    {
      if (_friendlyName != null && _friendlyName != String.Empty)
      {
        string filename = String.Format(@"database\card_{0}.xml", _friendlyName);
        using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings(filename))
        {
          xmlWriter.SetValue("tv", "contrast", GUIGraphicsContext.Contrast);
          xmlWriter.SetValue("tv", "brightness", GUIGraphicsContext.Brightness);
          xmlWriter.SetValue("tv", "gamma", GUIGraphicsContext.Gamma);
          xmlWriter.SetValue("tv", "saturation", GUIGraphicsContext.Saturation);
          xmlWriter.SetValue("tv", "sharpness", GUIGraphicsContext.Sharpness);
        }
      }
    }


    #endregion


  }
}
#endif