#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

#if (UseCaptureCardDefinitions)

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Radio.Database;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using Toub.MediaCenter.Dvrms.Metadata;
using TVCapture;
//using DirectX.Capture;

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
      if (info.CreationTime < fi.info.CreationTime)
      {
        return -1;
      }
      if (info.CreationTime > fi.info.CreationTime)
      {
        return 1;
      }
      return 0;
    }

    #endregion
  }

  #endregion

  [Serializable]
  public class TVCaptureDevice
  {
    #region consts

    private const string recEngineExt = ".dvr-ms"; // Change extension here when switching to TS enginge!!!

    private class RecordingFinished
    {
      public string fileName = string.Empty;
    }

    #endregion

    #region variables

    private string _videoCaptureDevice = string.Empty;

    private string _videoCaptureMoniker = string.Empty;
                   //@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_88010070&rev_01#3&267a616a&0&60#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}";

    private string _commercialName = string.Empty;
    private string _deviceId = string.Empty;
    private string _captureName = string.Empty;
    private bool _useForRecording = false;
    private bool _useForTv = false;
    private string _friendlyName = string.Empty;
    private int _cardPriority = 1;
    private string _recordingPath = string.Empty;
    private CardTypes _cardType;
    private bool _supportsTv;
    private bool _supportsRadio;

    private int _defaultRecordingQuality = -1;
    private DateTime _lastChannelChange = DateTime.Now;

    private enum State
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

    private bool _isAnalogCable;
    private int _defaultCountryCode;
    [NonSerialized] private int _cardId = -1;
    [NonSerialized] private State _currentGraphState = State.None;
    [NonSerialized] private TVRecording _currentTvRecording = null;
    [NonSerialized] private TVProgram _currentTvProgramRecording = null;
    [NonSerialized] private string _currentTvChannelName = string.Empty;
    [NonSerialized] private int _preRecordInterval = 0; // In minutes
    [NonSerialized] private int _postRecordInterval = 0; // In minutes
    [NonSerialized] private IGraph _currentGraph = null;
    [NonSerialized] private TVRecorded _recordedTvObject = null;
    [NonSerialized] private bool _isCardAllocated = false;
    [NonSerialized] private DateTime _timeRecordingStarted;
    [NonSerialized] private DateTime _timeTimeshiftingStarted;
    [NonSerialized] private string _currentRadioStationName = string.Empty;
    [NonSerialized] private int _radioSensitivity = 1;
    [NonSerialized] private DateTime _epgTimeOutTimer = DateTime.Now;
    [NonSerialized] private GraphHelper _graphHelper = null;
    [NonSerialized] private static Hashtable _devices = new Hashtable();
    [NonSerialized] private CropSettings _cropSettings;

    /// <summary>
    /// #MW#
    /// </summary>

    #endregion

    #region events

    public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);

    public event OnTvRecordingHandler OnTvRecordingEnded = null;
    public event OnTvRecordingHandler OnTvRecordingStarted = null;

    #endregion

    public static TVCaptureDevice GetTVCaptureDevice(int cardId)
    {
      return (TVCaptureDevice) _devices[cardId];
    }

    public static Hashtable GetTVCaptureDevices()
    {
      return _devices;
    }

    #region ctor

    /// <summary>
    /// Default constructor
    /// </summary>
    public TVCaptureDevice()
    {
      int countryCode = 31;
      string tunerInput = "Antenna";


      using (Settings xmlReader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        tunerInput = xmlReader.GetValueAsString("capture", "tuner", "Antenna");
        countryCode = xmlReader.GetValueAsInt("capture", "country", 31);
      }

      bool isCableInput = false;
      if (!tunerInput.Equals("Antenna"))
      {
        isCableInput = true;
      }

      this.IsCableInput = isCableInput;
      this.DefaultCountryCode = countryCode;
    }

    #endregion

    #region properties

    public GraphHelper Graph
    {
      get
      {
        if (_graphHelper == null)
        {
          _graphHelper = new GraphHelper();
          _graphHelper.DeviceId = DeviceId;
          _graphHelper.CommercialName = CommercialName;
          _graphHelper.LoadDefinitions(VideoDevice, VideoDeviceMoniker);
        }
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
      get { return _commercialName; }
      set { _commercialName = value; }
    }

    public string DeviceId
    {
      get { return _deviceId; }
      set { _deviceId = value; }
    }

    /*
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
        _recordingPath = Util.Utils.RemoveTrailingSlash(_recordingPath);
        if (_recordingPath == null || _recordingPath.Length == 0)
        {
          _recordingPath = Directory.GetCurrentDirectory();
          _recordingPath = Util.Utils.RemoveTrailingSlash(_recordingPath);
        }
        return _recordingPath;
      }
      set { _recordingPath = value; }
    }

    public int SignalQuality
    {
      get
      {
        if (_currentGraph == null)
        {
          return 0;
        }
        return _currentGraph.SignalQuality();
      }
    }

    public int SignalStrength
    {
      get
      {
        if (_currentGraph == null)
        {
          return 0;
        }
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
        if (Allocated)
        {
          return false;
        }
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
      set { _isCardAllocated = value; }
    }

    /// <summary>
    /// Property to specify if this card can be used for recording or not
    /// </summary>
    public bool UseForRecording
    {
      get
      {
        if (Allocated)
        {
          return false;
        }
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
        if (_cardId != -1)
        {
          _devices.Remove(_cardId);
        }
        _cardId = value;
        if (_devices.ContainsKey(_cardId))
        {
          _devices[_cardId] = this;
        }
        else
        {
          _devices.Add(_cardId, this);
        }
      }
    }

    /// <summary>
    /// Property which returns true if this card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get
      {
        if (_currentGraphState == State.PreRecording)
        {
          return true;
        }
        if (_currentGraphState == State.Recording)
        {
          return true;
        }
        if (_currentGraphState == State.PostRecording)
        {
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Checks if the current recording is still recording x minutes in the future 
    /// </summary>
    /// 
    public bool IsRecordingAt(int minutes)
    {
      if (_currentTvRecording == null)
      {
        return false;
      }
      if (_currentTvRecording.IsRecordingAtTime(DateTime.Now.AddMinutes(minutes), _currentTvProgramRecording,
                                                _preRecordInterval, _postRecordInterval))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Property which returns true if this card is currently has a teletext
    /// </summary>
    public bool HasTeletext
    {
      get
      {
        if (_currentGraph == null)
        {
          return false;
        }
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
        if (ts.TotalMinutes >= 10)
        {
          return true;
        }
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
        if (IsRecording)
        {
          return true;
        }
        if (_currentGraphState == State.Timeshifting)
        {
          return true;
        }
        if (_currentGraphState == State.RadioTimeshifting)
        {
          return true;
        }
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
        if (!IsRecording)
        {
          return null;
        }
        return _currentTvRecording;
      }
      set { _currentTvRecording = value; }
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
        if (!IsRecording)
        {
          return null;
        }
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
        {
          value = GetFirstChannel();
        }
        else if (value != null && value.Length == 0)
        {
          value = GetFirstChannel();
        }

        if (value.Equals(_currentTvChannelName))
        {
          return; //nothing todo
        }

        Log.Info("TVCapture: change channel to :{0}", value);
        if (IsTimeShifting || IsRecording)
        {
          if (g_Player.Playing && g_Player.CurrentFile == TimeShiftFullFileName)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_STOP_FILE, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
        }
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

            _currentGraph.TuneChannel(channel);
            _lastChannelChange = DateTime.Now;
            if (IsTimeShifting)
            {
              _timeTimeshiftingStarted = DateTime.Now;
            }
          } //if (_currentGraph != null)
        } //if (!IsRecording)
      }
    }


    /// <summary>
    /// Property to turn on/off tv viewing
    /// </summary>
    public bool View
    {
      get { return (_currentGraphState == State.Viewing); }
    }

    public bool StopViewing()
    {
      if (_currentGraphState != State.Viewing)
      {
        return false;
      }
      bool result = false;
      StopEpgGrabbing();
      if (_currentGraphState == State.Viewing)
      {
        Log.Info("TVCapture.Stop Viewing() Card:{0} {1}", ID, _currentTvChannelName);
        result = _currentGraph.StopViewing();
        _currentTvChannelName = "";
      }
      _currentGraphState = State.Initialized;
      return result;
    }

    public bool StopEpgGrabbing()
    {
      if (!IsEpgGrabbing)
      {
        return false;
      }
      if (_currentGraph == null)
      {
        return false;
      }
      bool result = _currentGraph.StopEpgGrabbing();
      _currentGraphState = State.Initialized;
      return result;
    }

    public bool StartViewing(string channelName)
    {
      if (channelName == null || channelName.Length == 0)
      {
        Log.Error("TVCapture.Start Viewing channel name is empty");
        return false;
      }
      StopEpgGrabbing();
      StopRadio();

      if (_currentGraphState == State.Viewing)
      {
        TVChannel = channelName;
        return true;
      }
      if (IsRecording)
      {
        return false;
      }
      //DeleteGraph();TESTTEST

      if (CreateGraph())
      {
        Log.Info("TVCapture.Start Viewing() Card:{0} :{1}", ID, channelName);
        TVChannel chan = GetChannel(channelName);
        if (chan == null)
        {
          Log.Error("TVCapture.Start Viewing() Card:{0} :{1} unknown channel", ID, channelName);
          return false;
        }
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
        if (_currentGraph == null)
        {
          return null;
        }
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
          if (_currentGraph == null)
          {
            return false;
          }
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
        {
          return string.Empty;
        }
        if (_recordedTvObject == null)
        {
          return string.Empty;
        }
        return _recordedTvObject.FileName;
      }
    }

    public string RadioStation
    {
      get
      {
        if (_currentRadioStationName == null)
        {
          return string.Empty;
        }
        return _currentRadioStationName;
      }
    }

    /// <summary>
    /// Property to set Radio tuning sensitivity.
    /// sensitivity range from 1MHz for value 1 to 0.1MHZ for value 10
    /// </summary>
    public int RadioSensitivity
    {
      get { return _radioSensitivity; }
      set { _radioSensitivity = value; }
    }

    public string TimeShiftFileName
    {
      get
      {
        if (_currentGraph != null)
        {
          if (IsRadio)
          {
            return _currentGraph.RadioTimeshiftFileName();
          }
          else
          {
            return _currentGraph.TvTimeshiftFileName();
          }
        }
        else
        {
          IGraph g = GraphFactory.CreateGraph(this);
          if (IsRadio)
          {
            return g.RadioTimeshiftFileName();
          }
          else
          {
            return g.TvTimeshiftFileName();
          }
        }
      }
    }

    public string TimeShiftFilePath
    {
      get { return String.Format(@"{0}\card{1}", RecordingPath, ID); }
    }

    public string TimeShiftFullFileName
    {
      get { return String.Format(@"{0}\{1}", TimeShiftFilePath, TimeShiftFileName); }
    }

    public IBaseFilter AudiodeviceFilter
    {
      get
      {
        if (_currentGraph == null)
        {
          return null;
        }
        return _currentGraph.AudiodeviceFilter();
      }
    }

    public bool SupportsCamSelection
    {
      get
      {
        if (_currentGraph == null)
        {
          return false;
        }
        return _currentGraph.SupportsCamSelection();
      }
    }

    public bool SupportsHardwarePidFiltering
    {
      get
      {
        if (_currentGraph == null)
        {
          return false;
        }
        return _currentGraph.SupportsHardwarePidFiltering();
      }
    }

    public bool Supports5vAntennae
    {
      get
      {
        if (_currentGraph == null)
        {
          return false;
        }
        return _currentGraph.Supports5vAntennae();
      }
    }

    public bool SupportsTV
    {
      get { return _supportsTv; }
      set { _supportsTv = value; }
    }

    public bool SupportsRadio
    {
      get { return _supportsRadio; }
      set { _supportsRadio = value; }
    }

    /// <summary>
    /// Property which returns the crop settings for this capture device
    /// </summary>
    public CropSettings CropSettings
    {
      get { return _cropSettings; }
      set
      {
        _cropSettings = value;
        SendCropMessage();
        SaveCropSettings();
      }
    }

    #endregion

    #region public members

    /// <summary>
    /// Property which returns the available audio languages
    /// </summary>
    public ArrayList GetAudioLanguageList()
    {
      if (_currentGraph == null)
      {
        return null;
      }
      return _currentGraph.GetAudioLanguageList();
    }

    /// <summary>
    /// Property which gets the current audio language
    /// </summary>
    public int GetAudioLanguage()
    {
      if (_currentGraph == null)
      {
        return -1;
      }
      return _currentGraph.GetAudioLanguage();
    }

    /// <summary>
    /// Property which sets the new audio language
    /// </summary>
    public void SetAudioLanguage(int audioPid)
    {
      if (_currentGraph == null)
      {
        return;
      }
      _currentGraph.SetAudioLanguage(audioPid);
    }


    public FilterDefinition GetTvFilterDefinition(string filterCategory)
    {
      foreach (FilterDefinition fd in Graph.TvFilterDefinitions)
      {
        if (String.Compare(fd.Category, filterCategory, true) == 0)
        {
          return fd;
        }
      }
      return null;
    }

    public bool GrabEpg(TVChannel channel)
    {
      if (CreateGraph() == false)
      {
        return false;
      }
      bool result = _currentGraph.GrabEpg(channel);
      _epgTimeOutTimer = DateTime.Now;
      return result;
    }

    public int PostRecord
    {
      set { _postRecordInterval = value; }
      get { return _postRecordInterval; }
    }

    /// <summary>
    /// This method can be used to stop the current recording.
    /// After recording is stopped the card will return to timeshifting mode
    /// </summary>
    public void StopRecording()
    {
      if (!IsRecording)
      {
        return;
      }

      Log.Info("TVCapture.StopRecording() Card:{0}", ID);
      // todo : stop recorder
      _currentGraph.StopRecording();

      _recordedTvObject.End = Util.Utils.datetolong(DateTime.Now);
      TVDatabase.AddRecordedTV(_recordedTvObject);

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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

      if (OnTvRecordingEnded != null)
      {
        OnTvRecordingEnded(_recordedTvObject.FileName, _currentTvRecording, _currentTvProgramRecording);
      }

      _recordedTvObject = null;

      //Log.Info("TVCapture.StopRecording():_currentTvProgramRecording=null");
      // cleanup...
      _currentTvProgramRecording = null;
      _currentTvRecording = null;
      _preRecordInterval = 0;
      _postRecordInterval = 0;

      if (!g_Player.Playing)
      {
        StopTimeShifting();
        return;
      }
      string timeshiftFilename = TimeShiftFullFileName;
      if (!g_Player.CurrentFile.Equals(timeshiftFilename))
      {
        StopTimeShifting();
        return;
      }
    } //StopRecording()

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


      StopRadio();
      StopEpgGrabbing();
      if (!UseForRecording)
      {
        return;
      }

      if (currentProgram != null)
      {
        _currentTvProgramRecording = currentProgram.Clone();
      }
      //Log.Info("dev.Record():_currentTvProgramRecording={0}", _currentTvProgramRecording);
      _currentTvRecording = new TVRecording(recording);
      _preRecordInterval = iPreRecordInterval;
      _postRecordInterval = iPostRecordInterval;
      _currentTvChannelName = recording.Channel;

      Log.Info("TVCapture.Record() Card:{0} {1} on {2} from {3}-{4}", ID, recording.Title, _currentTvChannelName,
               recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString());
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
          if (_currentTvRecording.IsRecordingAtTime(DateTime.Now, _currentTvProgramRecording, _preRecordInterval,
                                                    _postRecordInterval))
          {
            if (!_currentTvRecording.IsRecordingAtTime(DateTime.Now, _currentTvProgramRecording, _preRecordInterval, 0))
            {
              _currentGraphState = State.PostRecording;
            }
            else if (
              !_currentTvRecording.IsRecordingAtTime(DateTime.Now, _currentTvProgramRecording, 0, _postRecordInterval))
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
            Log.Info(
              "TVCapture.Process() Card:{0} recording has ended '{1}' on channel:{2} from {3}-{4} id:{5} _cardPriority:{6} quality:{7}",
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
      Log.Info("TVCapture.Stop() Card:{0}", ID);
      StopRecording();
      StopTimeShifting();
      StopViewing();
      StopEpgGrabbing();
      StopRadio();
      //Call the Dispose method to close the TT CI
      DVBGraphBase dvbGraph = _currentGraph as DVBGraphBase;
      if (dvbGraph != null)
      {
        Log.Info("TVCapture.Stop() cardproperties.Dispose()");
        // When using a USB based TV Card and it is not attached, the Cardproperties.Dispose causes a NullReferenceException
        try
        {
          dvbGraph.CardProperties.Dispose();
        }
        catch (Exception)
        {
        }
      }
      DeleteGraph();
    }

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public bool CreateGraph()
    {
      if (Allocated)
      {
        return false;
      }
      if (_currentGraph == null)
      {
        LoadContrastGammaBrightnessSettings();
        LoadCropSettings();
        Log.Info("TVCapture.CreateGraph() Card:{0}", ID);
        _currentGraph = GraphFactory.CreateGraph(this);
        if (_currentGraph == null)
        {
          return false;
        }
        return _currentGraph.CreateGraph(Quality);
      }
      return true;
    }

    public string GetLastError()
    {
      if (_currentGraph == null)
      {
        return string.Empty;
      }
      return _currentGraph.LastError();
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
        Log.Info("TVCapture.DeleteGraph() Card:{0}", ID);
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
      StopRadio();
      if (IsRecording)
      {
        if (channelName != _currentTvChannelName)
        {
          return false;
        }
        return true;
      }
      Log.Info("TVCapture.StartTimeShifting() Card:{0} :{1}", ID, channelName);
      TVChannel channel = GetChannel(channelName);

      if (_currentGraphState == State.Timeshifting)
      {
        _timeTimeshiftingStarted = DateTime.Now; // rtv: should be higher, but record by reference doesn't work else
        if (_currentGraph.GetChannelNumber() != channel.Number)
        {
          _lastChannelChange = DateTime.Now;

          if (!_currentGraph.ShouldRebuildGraph(channel))
          {
            //_timeTimeshiftingStarted = DateTime.Now; rtv: replaced for mantis 745 (progressbar wrong)
            _currentGraph.TuneChannel(channel);
            _currentTvChannelName = channelName;
            return true;
          }
            // Broceliande: Mantis 788 fix test 
          else
          {
            RebuildGraph();
            return true;
          }
        }
        else
        {
          return true;
        }
      }


      if (_currentGraphState == State.Viewing)
      {
        StopViewing();
      }
      else if (_currentGraphState != State.Initialized)
      {
        //DeleteGraph();TESTTEST
      }
      if (!CreateGraph())
      {
        return false;
      }


      string strFileName = TimeShiftFullFileName;
      //Log.Info("Card:{0} timeshift to file:{1}", ID, strFileName);
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
      if (!IsTimeShifting)
      {
        return false;
      }

      //stopping timeshifting will also remove the live.tv file 
      Log.Info("TVCapture.StopTimeShifting() Card:{0}", ID);
      bool result = _currentGraph.StopTimeShifting();
      string fileName = TimeShiftFullFileName;
      Util.Utils.FileDelete(fileName);
      _currentTvChannelName = "";
      _timeTimeshiftingStarted = DateTime.MinValue;
      _currentGraphState = State.Initialized;
      return result;
    }

    public void Tune(TVChannel channel)
    {
      if (_currentGraphState != State.Viewing)
      {
        return;
      }

      Log.Info("TVCapture.Tune({0}", channel.Name);
      _currentGraph.TuneChannel(channel);
      _lastChannelChange = DateTime.Now;
      _currentTvChannelName = channel.Name;
    }


    public long VideoFrequency()
    {
      if (_currentGraph == null)
      {
        return 0;
      }
      return _currentGraph.VideoFrequency();
    }

    public bool SignalPresent()
    {
      if (_currentGraph == null)
      {
        return false;
      }
      return _currentGraph.SignalPresent();
    }

    public bool ViewChannel(TVChannel channel)
    {
      StopEpgGrabbing();
      if (_currentGraph == null)
      {
        if (!CreateGraph())
        {
          return false;
        }
      }
      bool result = _currentGraph.StartViewing(channel);
      SetTvSettings();

      return result;
    }

    public bool SupportsFrameSize(Size framesize)
    {
      if (_currentGraph == null)
      {
        return false;
      }
      return _currentGraph.SupportsFrameSize(framesize);
    }


    public void Tune(object tuningObject, int disecqNo)
    {
      if (_currentGraph == null)
      {
        return;
      }
      _currentGraph.Tune(tuningObject, disecqNo);
    }

    public void StoreTunedChannels(bool radio, bool tv, ref int newTvChannels, ref int updatedTvChannels,
                                   ref int newRadioChannels, ref int updatedRadioChannels)
    {
      if (_currentGraph == null)
      {
        return;
      }
      _currentGraph.StoreChannels(ID, radio, tv, ref newTvChannels, ref updatedTvChannels, ref newRadioChannels,
                                  ref updatedRadioChannels);
    }

    public void StopRadio()
    {
      if (!IsRadio)
      {
        return;
      }
      if (_currentGraph == null)
      {
        return;
      }

      _currentGraph.StopRadio();
      _currentGraphState = State.Initialized;
    }

    public IGraph InternalGraph
    {
      get { return _currentGraph; }
    }

    public void StartRadio(RadioStation station)
    {
      if (!IsRadio)
      {
        StopEpgGrabbing();
        StopTimeShifting();
        StopViewing();
        if (!CreateGraph())
        {
          return;
        }
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
      if (_currentGraph == null)
      {
        return;
      }
      _currentGraph.GrabTeletext(yesNo);
    }

    public bool CanViewTimeShiftFile()
    {
      if (_currentGraph == null)
      {
        return false;
      }
      if (!IsTimeShifting && !IsRecording)
      {
        return false;
      }
      return _currentGraph.CanViewTimeShiftFile();
    }

    #endregion

    #region private members

    private void RebuildGraph()
    {
      Log.Info("TvCaptureDevice:RebuildGraph() Card:{0} chan:{1}", ID, _currentTvChannelName);

      //stop playback of this channel
      if (_currentGraph != null)
      {
        if (g_Player.Playing && g_Player.CurrentFile == TimeShiftFullFileName)
        {
          if (Recorder.CommandProcessor != null)
          {
            Recorder.CommandProcessor.StopPlayer();
          }
        }
        //Log.Info("TvCaptureDevice:RebuildGraph() delete graph");
        _currentGraph.StopEpgGrabbing();
        _currentGraph.StopTimeShifting();
        _currentGraph.StopViewing();
        _currentGraph.StopRadio();
        //Log.Info("TvCaptureDevice:RebuildGraph() graph deleted");
      }

      TVChannel channel = GetChannel(_currentTvChannelName);
      if (_currentGraphState == State.Timeshifting)
      {
        Log.Info("TvCaptureDevice:RebuildGraph() recreate timeshifting graph");
        _currentGraph.StartTimeShifting(channel, TimeShiftFullFileName);
        _lastChannelChange = DateTime.Now;
        if (Recorder.Running)
        {
          Recorder.CommandProcessor.ResetTimeshiftTimer();
        }
      }
      else
      {
        Log.Info("TvCaptureDevice:RebuildGraph() recreate viewing graph");
        _currentGraph.StartViewing(channel);
        _lastChannelChange = DateTime.Now;
      }
      // Log.Info("Card:{0} rebuild graph done", ID);
    }

    private string StripIllegalChars(string recordingAttribute)
    {
      if (recordingAttribute == null)
      {
        return string.Empty;
      }
      if (recordingAttribute.Length == 0)
      {
        return string.Empty;
      }
      recordingAttribute = recordingAttribute.Replace(":", " ");
      recordingAttribute = recordingAttribute.Replace(";", " ");
      return recordingAttribute;
    }

    private Hashtable GetRecordingAttributes()
    {
      // set the meta data in the dvr-ms or .wmv file
      TimeSpan ts = (_recordedTvObject.EndTime - _recordedTvObject.StartTime);
      Hashtable propsToSet = new Hashtable();

      propsToSet.Add("channel",
                     new MetadataItem("channel", StripIllegalChars(_recordedTvObject.Channel), MetadataItemType.String));

      propsToSet.Add("recordedby", new MetadataItem("recordedby", "MediaPortal", MetadataItemType.String));

      if (_recordedTvObject.Title != null && _recordedTvObject.Title.Length > 0)
      {
        propsToSet.Add("title",
                       new MetadataItem("title", StripIllegalChars(_recordedTvObject.Title), MetadataItemType.String));
      }

      if (_recordedTvObject.Genre != null && _recordedTvObject.Genre.Length > 0)
      {
        propsToSet.Add("genre",
                       new MetadataItem("genre", StripIllegalChars(_recordedTvObject.Genre), MetadataItemType.String));
      }

      if (_recordedTvObject.Description != null && _recordedTvObject.Description.Length > 0)
      {
        propsToSet.Add("description",
                       new MetadataItem("details", StripIllegalChars(_recordedTvObject.Description),
                                        MetadataItemType.String));
      }

      propsToSet.Add("id", new MetadataItem("id", (uint) _recordedTvObject.ID, MetadataItemType.Dword));
      propsToSet.Add("cardno", new MetadataItem("cardno", (uint) this.ID, MetadataItemType.Dword));
      propsToSet.Add("duration", new MetadataItem("seconds", (uint) ts.TotalSeconds, MetadataItemType.Dword));
      propsToSet.Add("start", new MetadataItem("start", _recordedTvObject.Start.ToString(), MetadataItemType.String));
      propsToSet.Add("end", new MetadataItem("end", _recordedTvObject.End.ToString(), MetadataItemType.String));

      return propsToSet;
    } //void GetRecordingAttributes()


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
    private bool StartRecording(TVRecording recording)
    {
      Log.Info("TVCapture.StartRecording() Card:{0}  content:{1}", ID, recording.IsContentRecording);

      TVProgram prog = null;
      DateTime dtNow = DateTime.Now.AddMinutes(_preRecordInterval);

      //for reference recordings, find program which runs now
      if (!recording.IsContentRecording)
      {
        dtNow = DateTime.Now;
      }

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
      if (prog != null)
      {
        currentRunningProgram = prog.Clone();
      }


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
        {
          isMovie = true;
        }

        timeProgStart = currentRunningProgram.StartTime;

        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          if (isMovie)
          {
            strInput = xmlreader.GetValueAsString("capture", "moviesformat", string.Empty);
          }
          else
          {
            strInput = xmlreader.GetValueAsString("capture", "seriesformat", string.Empty);
          }
        }

        strInput = Util.Utils.ReplaceTag(strInput, "%channel%", Util.Utils.MakeFileName(currentRunningProgram.Channel),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%title%", Util.Utils.MakeFileName(currentRunningProgram.Title),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%name%", Util.Utils.MakeFileName(currentRunningProgram.Episode),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%series%", Util.Utils.MakeFileName(currentRunningProgram.SeriesNum),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%episode%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EpisodeNum), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%part%", Util.Utils.MakeFileName(currentRunningProgram.EpisodePart),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%date%",
                                         Util.Utils.MakeFileName(
                                           currentRunningProgram.StartTime.Date.ToShortDateString()), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%start%",
                                         Util.Utils.MakeFileName(currentRunningProgram.StartTime.ToShortTimeString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%end%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EndTime.ToShortTimeString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%genre%", Util.Utils.MakeFileName(currentRunningProgram.Genre),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startday%",
                                         Util.Utils.MakeFileName(currentRunningProgram.StartTime.Day.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startmonth%",
                                         Util.Utils.MakeFileName(currentRunningProgram.StartTime.Month.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startyear%",
                                         Util.Utils.MakeFileName(currentRunningProgram.StartTime.Year.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%starthh%",
                                         Util.Utils.MakeFileName(currentRunningProgram.StartTime.Hour.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startmm%",
                                         Util.Utils.MakeFileName(currentRunningProgram.StartTime.Minute.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endday%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EndTime.Day.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endmonth%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EndTime.Month.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startyear%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EndTime.Year.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endhh%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EndTime.Hour.ToString()),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endmm%",
                                         Util.Utils.MakeFileName(currentRunningProgram.EndTime.Minute.ToString()),
                                         "unknown");

        int index = strInput.LastIndexOf('\\');
        if (index != -1)
        {
          subDirectory = strInput.Substring(0, index).Trim();
          fileName = strInput.Substring(index + 1).Trim();
        }
        else
        {
          fileName = strInput.Trim();
        }


        if (subDirectory != string.Empty)
        {
          subDirectory = Util.Utils.RemoveTrailingSlash(subDirectory);
          subDirectory = Util.Utils.MakeDirectoryPath(subDirectory);
          fullPath = RecordingPath + "\\" + subDirectory;
          if (!Directory.Exists(fullPath))
          {
            Directory.CreateDirectory(fullPath);
          }
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
        fileName = Util.Utils.MakeFileName(fileName);
        if (File.Exists(fullPath + "\\" + fileName + recEngineExt))
        {
          int i = 1;
          while (File.Exists(fullPath + "\\" + fileName + "_" + i.ToString() + recEngineExt))
          {
            ++i;
          }
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
      string fullFileName = String.Format(@"{0}\{1}", fullPath, Util.Utils.MakeFileName(fileName));
      ulong freeSpace = Util.Utils.GetFreeDiskSpace(fullFileName);
      if (freeSpace < (1024L*1024L*1024L)) // 1 GB
      {
        Log.WriteFile(LogType.Recorder, true,
                      "Recorder:  failed to start recording since drive {0}: has less then 1GB freediskspace",
                      fullFileName[0]);
        return false;
      }
      Log.Info("Recorder: recording to {0}", fullFileName);

      TVChannel channel = GetChannel(_currentTvChannelName);

      _recordedTvObject = new TVRecorded();
      _recordedTvObject.Start = Util.Utils.datetolong(DateTime.Now);
      _recordedTvObject.Channel = _currentTvChannelName;
      _recordedTvObject.FileName = fullFileName;
      _recordedTvObject.KeepRecordingMethod = recording.KeepRecordingMethod;
      _recordedTvObject.KeepRecordingTill = recording.KeepRecordingTill;
      _recordedTvObject.RecordedCardIndex = ID;
      if (currentRunningProgram != null)
      {
        _recordedTvObject.Title = currentRunningProgram.Title;
        _recordedTvObject.Genre = currentRunningProgram.Genre;
        _recordedTvObject.Description = currentRunningProgram.Description;
        _recordedTvObject.End = currentRunningProgram.End;
      }
      else
      {
        _recordedTvObject.Title = string.Empty;
        _recordedTvObject.Genre = string.Empty;
        _recordedTvObject.Description = string.Empty;
        _recordedTvObject.End = Util.Utils.datetolong(DateTime.Now.AddHours(2));
      }

      Hashtable attribtutes = GetRecordingAttributes();
      if (timeProgStart < _lastChannelChange)
      {
        timeProgStart = _lastChannelChange;
      }
      if (
        !_currentGraph.StartRecording(attribtutes, recording, channel, ref fullFileName, recording.IsContentRecording,
                                      timeProgStart))
      {
        Log.Info("Recorder: StartRecording FAILED");
        return false;
      }
      _recordedTvObject.FileName = fullFileName;

      _timeRecordingStarted = DateTime.Now;
      _currentGraphState = State.Recording;
      SetTvSettings();

      if (OnTvRecordingStarted != null)
      {
        OnTvRecordingStarted(RecordingFileName, _currentTvRecording, _currentTvProgramRecording);
      }
      return true;
    }

    private string GetFirstChannel()
    {
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (chan.Number < (int) ExternalInputs.svhs)
        {
          return chan.Name;
        }
      }
      foreach (TVChannel chan in channels)
      {
        return chan.Name;
      }
      return string.Empty;
    }

    /// <summary>
    /// Returns the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number (or 0 if channelname is unknown)</returns>
    /// <remarks>
    /// Channel names and numbers are stored in the TVDatabase
    /// </remarks>
    private TVChannel GetChannel(string strChannelName)
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
          {
            chan.Country = _defaultCountryCode;
          }
          return chan;
        }
      }
      return retChannel;
    }

    private void SetTvSettings()
    {
      int gamma = GUIGraphicsContext.Gamma;
      GUIGraphicsContext.Gamma = -2;
      GUIGraphicsContext.Gamma = -1;
      if (gamma >= 0)
      {
        GUIGraphicsContext.Gamma = gamma;
      }
    }

    private void LoadContrastGammaBrightnessSettings()
    {
      try
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _friendlyName));
        using (Settings xmlreader = new Settings(filename))
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
      {
      }
    }

    private void SaveContrastGammaBrightnessSettings()
    {
      if (_friendlyName != null && _friendlyName != string.Empty)
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format(@"card_{0}.xml", _friendlyName));
        using (Settings xmlWriter = new Settings(filename))
        {
          xmlWriter.SetValue("tv", "contrast", GUIGraphicsContext.Contrast);
          xmlWriter.SetValue("tv", "brightness", GUIGraphicsContext.Brightness);
          xmlWriter.SetValue("tv", "gamma", GUIGraphicsContext.Gamma);
          xmlWriter.SetValue("tv", "saturation", GUIGraphicsContext.Saturation);
          xmlWriter.SetValue("tv", "sharpness", GUIGraphicsContext.Sharpness);
        }
      }
    }

    /// <summary>
    /// Loads stored crop settings for this capture device
    /// </summary>
    private void LoadCropSettings()
    {
      try
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _friendlyName));
        using (Settings xmlreader = new Settings(filename))
        {
          _cropSettings = new CropSettings(
            xmlreader.GetValueAsInt("tv", "croptop", 0),
            xmlreader.GetValueAsInt("tv", "cropbottom", 0),
            xmlreader.GetValueAsInt("tv", "cropleft", 0),
            xmlreader.GetValueAsInt("tv", "cropright", 0)
            );
        }
      }
      catch (Exception)
      {
      }
    }

    /// <summary>
    /// Saves crop settings for this capture device
    /// </summary>
    private void SaveCropSettings()
    {
      if (_friendlyName != null && _friendlyName != string.Empty)
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format(@"card_{0}.xml", _friendlyName));
        using (Settings xmlWriter = new Settings(filename))
        {
          xmlWriter.SetValue("tv", "croptop", _cropSettings.Top);
          xmlWriter.SetValue("tv", "cropbottom", _cropSettings.Bottom);
          xmlWriter.SetValue("tv", "cropleft", _cropSettings.Left);
          xmlWriter.SetValue("tv", "cropright", _cropSettings.Right);
        }
      }
    }

    /// <summary>
    /// Handles sending the crop message with the crop settings for the current capture device
    /// </summary>
    public void SendCropMessage()
    {
      Log.Info("TvCaptureDevice.SendCropMessage(): {0}, {1}, {2}, {3}", _cropSettings.Top, _cropSettings.Bottom,
               _cropSettings.Left, _cropSettings.Right);
      GUIWindowManager.SendThreadMessage(
        new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP, 0, 0, 0, 0, 0, _cropSettings)
        );
    }

    #endregion
  }
}

#endif