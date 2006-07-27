using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using TvLibrary.Interfaces;

namespace TvControl
{
  /// <summary>
  /// Virtual Card class
  /// This class provides methods and properties which a client can use
  /// The class will handle the communication and control with the
  /// tv service backend
  /// </summary>
  [Serializable]
  public class VirtualCard
  {
    #region variables
    int _cardId=-1;
    string _server;
    string _recordingFolder;
    #endregion

    #region ctor
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="cardId"></param>
    /// <param name="server"></param>
    public VirtualCard(int cardId, string server)
    {
      _cardId = cardId;
      _server = server;
    }
    #endregion

    #region properties
    /// <summary>
    /// returns the card id of this virtual card
    /// </summary>
    public int Id
    {
      get
      {
        return _cardId;
      }
    }
    /// <summary>
    /// gets the ip adress of the tvservice
    /// </summary>
    public string RemoteServer
    {
      get
      {
        return _server;
      }
    }

    /// <summary>
    /// gets/sets the recording folder for the card
    /// </summary>
    public string RecordingFolder
    {
      get
      {
        return _recordingFolder;
      }
      set
      {
        _recordingFolder = value;
        if (_recordingFolder == String.Empty)
          _recordingFolder = System.IO.Directory.GetCurrentDirectory();

      }
    }
    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <value>cardtype</value>
    [XmlIgnore]
    public CardType Type
    {
      get
      {
        if (_cardId < 0) return CardType.Analog;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.Type(_cardId);
      }
    }

    /// <summary>
    /// Gets the name 
    /// </summary>
    /// <returns>name of card</returns>
    [XmlIgnore]
    public string Name
    {
      get
      {
        if (_cardId < 0) return "";
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.CardName(_cardId);
      }
    }

    /// <summary>
    /// Gets the device path 
    /// </summary>
    /// <returns>devicePath of card</returns>
    [XmlIgnore]
    public string Device
    {
      get
      {
        if (_cardId < 0) return "";
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.CardDevice(_cardId);
      }
    }


    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <returns>filename of the recording or null when not recording</returns>
    [XmlIgnore]
    public string RecordingFileName
    {
      get
      {
        if (_cardId < 0) return "";
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.FileName(_cardId);
      }
    }

    /// <summary>
    /// gets/sets the current audio stream 
    /// </summary>
    /// <returns>current audio stream</returns>
    [XmlIgnore]
    public IAudioStream AudioStream
    {
      get
      {
        if (_cardId < 0) return null;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetCurrentAudioStream(_cardId);
      }
      set
      {
        if (_cardId < 0) return;
        RemoteControl.HostName = _server;
        RemoteControl.Instance.SetCurrentAudioStream(_cardId, value);
      }
    }
    [XmlIgnore]
    public IAudioStream[] AvailableAudioStreams
    {
      get
      {
        if (_cardId < 0) return null;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.AvailableAudioStreams(_cardId);
      }
    }

    /// <summary>
    /// returns which schedule is currently being recorded
    /// </summary>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    [XmlIgnore]
    public int RecordingScheduleId
    {
      get
      {
        if (_cardId < 0) return -1;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetRecordingSchedule(_cardId);
      }
    }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>
    [XmlIgnore]
    public string RTSPUrl
    {
      get
      {
        if (_cardId < 0) return "";
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetStreamingUrl(_cardId);
      }
    }


    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="cardId">onOff when true start grabbing teletext otherwise stop grabbing teletext</param>
    [XmlIgnore]
    public bool GrabTeletext
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsGrabbingTeletext(_cardId);
      }
      set
      {
        if (_cardId < 0) return;
        RemoteControl.HostName = _server;
        RemoteControl.Instance.GrabTeletext(_cardId, value);
      }
    }

    /// <summary>
    /// Returns if the current channel has teletext or not
    /// </summary>
    /// <returns>yes if channel has teletext otherwise false</returns>
    [XmlIgnore]
    public bool HasTeletext
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.HasTeletext(_cardId);
      }
    }

    /// <summary>
    /// Returns if we arecurrently grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    [XmlIgnore]
    public bool IsGrabbingEpg
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsGrabbingEpg(_cardId);
      }
    }

    /// <summary>
    /// Returns if card is currently recording or not
    /// </summary>
    /// <returns>true when card is recording otherwise false</returns>
    [XmlIgnore]
    public bool IsRecording
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsRecording(_cardId);
      }
    }

    /// <summary>
    /// Returns if card is currently scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>
    [XmlIgnore]
    public bool IsScanning
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsScanning(_cardId);
      }
    }

    /// <summary>
    /// Returns whether the current channel is scrambled or not.
    /// </summary>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    [XmlIgnore]
    public bool IsScrambled
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsScrambled(_cardId);
      }
    }

    /// <summary>
    /// Returns if card is currently timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    [XmlIgnore]
    public bool IsTimeShifting
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsTimeShifting(_cardId);
      }
    }

    /// <summary>
    /// returns the minium channel number
    /// </summary>
    /// <returns>minium channel number</returns>
    [XmlIgnore]
    public int MinChannel
    {
      get
      {
        if (_cardId < 0) return 0;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.MinChannel(_cardId);
      }
    }

    /// <summary>
    /// returns the maximum channel number
    /// </summary>
    /// <returns>maximum channel number</returns>
    [XmlIgnore]
    public int MaxChannel
    {
      get
      {
        if (_cardId < 0) return 0;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.MaxChannel(_cardId);
      }
    }
    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <returns>timeshifting filename null when not timeshifting</returns>
    [XmlIgnore]
    public string TimeShiftFileName
    {
      get
      {
        if (_cardId < 0) return "";
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.TimeShiftFileName(_cardId);
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started 
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    [XmlIgnore]
    public DateTime TimeShiftStarted
    {
      get
      {
        if (_cardId < 0) return DateTime.MinValue;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.TimeShiftStarted(_cardId);
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started 
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    [XmlIgnore]
    public DateTime RecordingStarted
    {
      get
      {
        if (_cardId < 0) return DateTime.MinValue;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.RecordingStarted(_cardId);
      }
    }

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <returns>true if tuner is locked otherwise false
    [XmlIgnore]
    public bool IsTunerLocked
    {
      get
      {
        if (_cardId < 0) return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.TunerLocked(_cardId);
      }
    }

    /// <summary>
    /// Gets the name of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>
    [XmlIgnore]
    public string ChannelName
    {
      get
      {
        if (_cardId < 0) return "";
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.CurrentChannelName(_cardId);
      }
    }


    /// <summary>
    /// Gets the of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>
    [XmlIgnore]
    public IChannel Channel
    {
      get
      {
        if (_cardId < 0) return null;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.CurrentChannel(_cardId);
      }
    }

    /// <summary>
    /// Returns the signal level 
    /// </summary>
    /// <returns>signal level (0-100)</returns>
    [XmlIgnore]
    public int SignalLevel
    {
      get
      {
        if (_cardId < 0) return 0;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SignalLevel(_cardId);
      }
    }

    /// <summary>
    /// Returns the signal quality 
    /// </summary>
    /// <returns>signal quality (0-100)</returns>
    [XmlIgnore]
    public int SignalQuality
    {
      get
      {
        if (_cardId < 0) return 0;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SignalQuality(_cardId);
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>byte[] array containing the raw teletext page or null if page is not found</returns>
    public byte[] GetTeletextPage(int pageNumber, int subPageNumber)
    {
      if (_cardId < 0) return new byte[] { 1 };
      RemoteControl.HostName = _server;
      return RemoteControl.Instance.GetTeletextPage(_cardId, pageNumber, subPageNumber);
    }

    /// <summary>
    /// Starts the epg grabber to grab the epg and update the tvguide
    /// </summary>
    /// <returns></returns>
    public void GrabEpg()
    {
      if (_cardId < 0) return;
      RemoteControl.HostName = _server;
      RemoteControl.Instance.GrabEpg(_cardId);
    }

    /// <summary>
    /// scans current transponder for channels.
    /// </summary>
    /// <returns>list of all channels found</returns>
    public IChannel[] Scan(IChannel channel)
    {
      if (_cardId < 0) return null;
      RemoteControl.HostName = _server;
      return RemoteControl.Instance.Scan(_cardId, channel);
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopTimeShifting()
    {
      if (_cardId < 0) return;
      RemoteControl.HostName = _server;
      RemoteControl.Instance.StopTimeShifting(_cardId);
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopRecording()
    {
      if (_cardId < 0) return;
      RemoteControl.HostName = _server;
      RemoteControl.Instance.StopRecording(_cardId);
    }
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <returns>true if success otherwise false</returns>
    public bool StartRecording(string fileName, bool contentRecording, long startTime)
    {
      if (_cardId < 0) return false;
      RemoteControl.HostName = _server;
      return RemoteControl.Instance.StartRecording(_cardId, fileName, contentRecording, startTime);
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>number of teletext subpages for the pagenumber</returns>
    public int SubPageCount(int pageNumber)
    {
      if (_cardId < 0) return -1;
      RemoteControl.HostName = _server;
      return RemoteControl.Instance.SubPageCount(_cardId, pageNumber);
    }

    /// <summary>f
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(int pageNumber)
    {
      if (_cardId < 0) return new TimeSpan(0, 0, 0, 15);
      RemoteControl.HostName = _server;
      return RemoteControl.Instance.TeletextRotation(_cardId, pageNumber);
    }
    #endregion

  }
}
