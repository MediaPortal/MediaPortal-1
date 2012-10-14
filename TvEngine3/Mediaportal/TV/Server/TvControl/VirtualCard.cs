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
using System.Net;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl
{
  /// <summary>
  /// Virtual Card class
  /// This class provides methods and properties which a client can use
  /// The class will handle the communication and control with the
  /// tv service backend
  /// </summary>
  [DataContract]
  public class VirtualCard : IVirtualCard
  {
    #region variables

    [DataMember]
    private int _nrOfOtherUsersTimeshiftingOnCard = 0;

    [DataMember]
    private string _server;

    [DataMember]
    private string _recordingFolder;

    [DataMember]
    private string _timeShiftFolder;

    [DataMember]
    private int _recordingFormat;

    [DataMember]
    private IUser _user;

    [DataMember]
    private const int CommandTimeOut = 3000;

    [DataMember]
    private bool _isTimeshifting;

    [DataMember]
    private bool _isScrambled;

    [DataMember]
    private bool _isScanning;

    [DataMember]
    private bool _isRecording;

    [DataMember]
    private bool _isGrabbingEpg;

    [DataMember]
    private bool _hasTeletext;

    [DataMember]
    private string _rtspUrl;

    [DataMember]
    private string _recordingFileName;

    [DataMember]
    private string _name;

    [DataMember]
    private CardType _cardType = CardType.Analog;

    [DataMember]
    private string _timeShiftFileName;

    [DataMember]
    private string _channelName;    

    [DataMember]
    private int _idChannel = -1;

    [DataMember]
    private MediaTypeEnum? _mediaType;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="server">The server.</param>
    /// <param name="recordingFormat">The recording format.</param>
    public VirtualCard(User user, string server, int recordingFormat)
    {      
      _user = user;
      _server = server;

      InitStaticProperties();

      _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                       Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      _recordingFormat = recordingFormat;

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="server">The server.</param>
    public VirtualCard(IUser user, string server)
    {
      _user = user;
      _server = server;
      _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                       Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

      InitStaticProperties();
    }

    private void InitStaticProperties()
    {
      string userName = _user.Name;
      int cardId = _user.CardId;

      var controllerService = GlobalServiceProvider.Get<IControllerService>();
      if (!string.IsNullOrWhiteSpace(userName))
      {
        _isTimeshifting = controllerService.IsTimeShifting(userName);
        _isScrambled = controllerService.IsScrambled(userName);
        _hasTeletext = controllerService.HasTeletext(userName);
        _rtspUrl = controllerService.GetStreamingUrl(userName);
        _recordingFileName = controllerService.RecordingFileName(userName);
        _idChannel = controllerService.CurrentDbChannel(userName);
        _channelName = controllerService.CurrentChannelName(userName);

        if (_idChannel > 0)
        {
          IChannel channel = controllerService.CurrentChannel(userName, _idChannel);
          if (channel != null)
          {
            _mediaType = channel.MediaType;
          }
        }
        if (cardId > 0 && _user.UserType == UserType.Normal)
        {
          _timeShiftFileName = controllerService.TimeShiftFileName(userName, cardId);
        }
      }

      if (cardId > 0)
      {               
        if (_idChannel > 0)
        {
          _isRecording = controllerService.IsRecording(_idChannel, cardId);
        }

        _isScanning = controllerService.IsScanning(cardId);
        _isGrabbingEpg = controllerService.IsGrabbingEpg(cardId);
        _name = controllerService.CardName(cardId);
        _cardType = controllerService.Type(cardId);        
      }                        
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    public VirtualCard(IUser user)
    {
      _user = user;
      _server = Dns.GetHostName();

      InitStaticProperties();

      _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                       Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }

    #endregion
    
    #region properties

    #region static properties

    public MediaTypeEnum? MediaType
    {
      get { return _mediaType; }
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public IUser User
    {
      get { return _user; }
    }

    /// <summary>
    /// returns the card id of this virtual card
    /// </summary>
    public int Id
    {
      get { return _user.CardId; }
    }

    /// <summary>
    /// gets the ip adress of the tvservice
    /// </summary>
    public string RemoteServer
    {
      get { return _server; }
      set { _server = value; }
    }

    ///<summary>
    /// Gets/Set the recording format
    ///</summary>
    public int RecordingFormat
    {
      get { return _recordingFormat; }
      set { _recordingFormat = value; }
    }

    /// <summary>
    /// gets/sets the recording folder for the card
    /// </summary>    
    public string RecordingFolder
    {
      get { return _recordingFolder; }
      set
      {
        _recordingFolder = value;
        if (_recordingFolder == String.Empty)
        {
          _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                           Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        }
      }
    }

    /// <summary>
    /// gets/sets the timeshifting folder for the card
    /// </summary>    
    public string TimeshiftFolder
    {
      get
      {
        return _timeShiftFolder;
      }
      set
      {
        _timeShiftFolder = value;
        if (_timeShiftFolder == String.Empty)
        {
          _timeShiftFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
                                           Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        }
      }
    }

    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <value>cardtype</value>    
    public CardType Type
    {
      get
      {
        return _cardType;
        /*try
        {
          if (User.CardId < 0)
          {
            return CardType.Analog;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().Type(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return CardType.Analog;*/
      }
      set { _cardType = value; }
    }

    /// <summary>
    /// Gets the name 
    /// </summary>
    /// <returns>name of card</returns>    
    public string Name
    {
      get
      {
        return _name;
        /*try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CardName(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _name = value; }
    }


    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <returns>filename of the recording or null when not recording</returns>    
    public string RecordingFileName
    {
      get
      {
        return _recordingFileName;
        /*try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().RecordingFileName(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _recordingFileName = value; }
    }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>

    public string RTSPUrl
    {
      get
      {
        return _rtspUrl;
        /*
        try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().GetStreamingUrl(User.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _rtspUrl = value; }
    }

    /// <summary>
    /// Returns if the current channel has teletext or not
    /// </summary>
    /// <returns>yes if channel has teletext otherwise false</returns>    
    public bool HasTeletext
    {
      get
      {
        return _hasTeletext;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().HasTeletext(User.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _hasTeletext = value; }
    }

    /// <summary>
    /// Returns if we arecurrently grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg otherwise false</returns>    
    public bool IsGrabbingEpg
    {
      get
      {
        return _isGrabbingEpg;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsGrabbingEpg(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isGrabbingEpg = value; }
    }

    /// <summary>
    /// Returns if card is currently recording or not
    /// </summary>
    /// <returns>true when card is recording otherwise false</returns>    
    public bool IsRecording
    {
      get
      {
        return _isRecording;
        /*try
        {
          //if (User.CardId < 0) return false;
          RemoteControl.HostName = _server;          
          IVirtualCard vc = null;
          bool isRec = WaitFor<bool>.Run(CommandTimeOut, () => GlobalServiceProvider.Get<IControllerService>().IsRecording(IdChannel, out vc));
          return (isRec && vc.Id == Id && vc.User.UserType == UserType.Scheduler);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isRecording = value; }
    }

    /// <summary>
    /// Returns if card is currently scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>    
    public bool IsScanning
    {
      get
      {
        return _isScanning;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsScanning(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isScanning = value; }
    }

    /// <summary>
    /// Returns whether the current channel is scrambled or not.
    /// </summary>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>    
    public bool IsScrambled
    {
      get
      {
        return _isScrambled;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsScrambled(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isScrambled = value; }
    }



    /// <summary>
    /// Returns if card is currently timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>    
    public bool IsTimeShifting
    {
      get
      {
        return _isTimeshifting;
        /*try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return WaitFor<bool>.Run(CommandTimeOut, () => GlobalServiceProvider.Get<IControllerService>().IsTimeShifting(_user.Name));
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;*/
      }
      set { _isTimeshifting = value; }
    }

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <returns>timeshifting filename null when not timeshifting</returns>    
    public string TimeShiftFileName
    {
      get
      {
        return _timeShiftFileName;
        /*try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().TimeShiftFileName(User.Name, User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _timeShiftFileName = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public int NrOfOtherUsersTimeshiftingOnCard
    {
      get { return _nrOfOtherUsersTimeshiftingOnCard; }
      set { _nrOfOtherUsersTimeshiftingOnCard = value; }
    }

    #endregion

    #region dynamic properties

   

    /// <summary>
    /// returns which schedule is currently being recorded
    /// </summary>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    [XmlIgnore]
    public int RecordingScheduleId
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return -1;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().GetRecordingSchedule(User.CardId, User.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return -1;
      }
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    [XmlIgnore]
    public bool GrabTeletext
    {
      set
      {
        try
        {
          if (User.CardId < 0)
          {
            return;
          }
          RemoteControl.HostName = _server;
          GlobalServiceProvider.Get<IControllerService>().GrabTeletext(User.Name, value);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
      }
    }

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <returns>true if tuner is locked otherwise false</returns>
    [XmlIgnore]
    public bool IsTunerLocked
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().TunerLocked(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>    
    public void GetStreamQualityCounters(out int totalTSpackets, out int discontinuityCounter)
    {
      totalTSpackets = 0;
      discontinuityCounter = 0;
      try
      {
        if (User.CardId > 0)
        {
          RemoteControl.HostName = _server;
          GlobalServiceProvider.Get<IControllerService>().GetStreamQualityCounters(User.Name, out totalTSpackets, out discontinuityCounter);
        }
      }
      catch (Exception)
      {
        //HandleFailure();
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
        try
        {
          if (User.CardId < 0)
          {
            return 0;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().SignalLevel(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return 0;
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
        try
        {
          if (User.CardId < 0)
          {
            return 0;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().SignalQuality(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return 0;
      }
    }



    /// <summary>
    /// Gets the name of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>    
    public string ChannelName
    {
      get
      {
        return _channelName;
        /*
        try
        {
          if (User.CardId < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CurrentChannelName(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
    }


   

    /// <summary>
    /// returns the database channel
    /// </summary>
    /// <returns>int</returns>    
    public int IdChannel
    {
      get
      {
        return _idChannel;
        /*
        try
        {
          if (User.CardId < 0)
          {
            return -1;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CurrentDbChannel(_user.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return -1;*/
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
      try
      {
        if (User.CardId < 0)
        {
          return new byte[] { 1 };
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().GetTeletextPage(User.Name, pageNumber, subPageNumber);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return new byte[] { 1 };
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    /*public void StartTimeShifting()
    {
      try
      {
        RemoteControl.HostName = _server;
        IUser userResult;
        GlobalServiceProvider.Get<IControllerService>().StartTimeShifting(_user.Name, out userResult);

        if (userResult != null)
        {
          _user = userResult;
          _isTimeshifting = false;
        }
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }*/

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopTimeShifting()
    {
      try
      {
        if (User.CardId < 0)
        {
          return;
        }
        if (IsTimeShifting == false)
        {
          return;
        }
        RemoteControl.HostName = _server;
        IUser userResult;
        GlobalServiceProvider.Get<IControllerService>().StopTimeShifting(_user.Name, out userResult);

        if (userResult != null)
        {
          _user = userResult;          
        }
        _isTimeshifting = false;
        _timeShiftFileName = null;
        _isScrambled = false;
        _hasTeletext = false;
        _rtspUrl = null;
        _name = null;
        _cardType = CardType.Analog;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopRecording()
    {
      try
      {
        if (User.CardId < 0)
        {
          return;
        }
        RemoteControl.HostName = _server;
        IUser userResult;
        GlobalServiceProvider.Get<IControllerService>().StopRecording(_user.Name, _user.CardId, out userResult);
        if (userResult != null)
        {
          _user = userResult;
        }
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns>true if success otherwise false</returns>
    public TvResult StartRecording(ref string fileName)
    {
      try
      {
        if (User.CardId < 0)
        {
          return TvResult.UnknownError;
        }
        RemoteControl.HostName = _server;
        IUser userResult;
        TvResult startRecording = GlobalServiceProvider.Get<IControllerService>().StartRecording(_user.Name, _user.CardId, out userResult, ref fileName);

        if (userResult != null)
        {
          _user = userResult;
        }

        return startRecording;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>number of teletext subpages for the pagenumber</returns>
    public int SubPageCount(int pageNumber)
    {
      try
      {
        if (User.CardId < 0)
        {
          return -1;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().SubPageCount(User.Name, pageNumber);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber()
    {
      try
      {
        if (User.CardId < 0)
        {
          return -1;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().GetTeletextRedPageNumber(User.Name);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber()
    {
      try
      {
        if (User.CardId < 0)
        {
          return -1;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().GetTeletextGreenPageNumber(User.Name);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber()
    {
      try
      {
        if (User.CardId < 0)
        {
          return -1;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().GetTeletextYellowPageNumber(User.Name);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber()
    {
      try
      {
        if (User.CardId < 0)
        {
          return -1;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().GetTeletextBluePageNumber(User.Name);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return -1;
    }

    /// <summary>f
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(int pageNumber)
    {
      try
      {
        if (User.CardId < 0)
        {
          return new TimeSpan(0, 0, 0, 15);
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().TeletextRotation(User.Name, pageNumber);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return new TimeSpan(0, 0, 0, 15);
    }



    #endregion

    #region quality control

    /// <summary>
    /// Indicates, if the user is the owner of the card
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsOwner()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().IsOwner(User.CardId, User.Name);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }


    /// <summary>
    /// Indicates, if the card supports quality control
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsQualityControl()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().SupportsQualityControl(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rates
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRate()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().SupportsBitRate(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rate modes 
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRateModes()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().SupportsBitRateModes(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rate peak mode
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode()
    {
      try
      {
        if (User.CardId < 0)
        {
          return false;
        }
        RemoteControl.HostName = _server;
        return GlobalServiceProvider.Get<IControllerService>().SupportsPeakBitRateMode(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Gets/Sts the quality type
    /// </summary>
    public QualityType QualityType
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return QualityType.Default;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().GetQualityType(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return QualityType.Default;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
          {
            return;
          }
          RemoteControl.HostName = _server;
          GlobalServiceProvider.Get<IControllerService>().SetQualityType(User.CardId, value);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
      }
    }

    /// <summary>
    /// Gets/Sts the bitrate mode
    /// </summary>
    public VIDEOENCODER_BITRATE_MODE BitRateMode
    {
      get
      {
        try
        {
          if (User.CardId < 0)
          {
            return VIDEOENCODER_BITRATE_MODE.Undefined;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().GetBitRateMode(User.CardId);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return VIDEOENCODER_BITRATE_MODE.Undefined;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
          {
            return;
          }
          RemoteControl.HostName = _server;
          GlobalServiceProvider.Get<IControllerService>().SetBitRateMode(User.CardId, value);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
      }
    }

    #region CI Menu Handling

    /// <summary>
    /// Indicates, if the card supports CI Menu
    /// </summary>
    /// <returns>true/false</returns>
    public bool CiMenuSupported()
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().CiMenuSupported(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Enters the CI Menu for current card
    /// </summary>
    /// <returns>true if successful</returns>
    public bool EnterCiMenu()
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().EnterCiMenu(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Selects a ci menu entry
    /// </summary>
    /// <param name="Choice">Choice (1 based), 0 for "back"</param>
    /// <returns>true if successful</returns>
    public bool SelectCiMenu(byte Choice)
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().SelectMenu(User.CardId, Choice);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Closes the CI Menu for current card
    /// </summary>
    /// <returns>true if successful</returns>
    public bool CloseMenu()
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().CloseMenu(User.CardId);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Sends an answer to CAM after a request
    /// </summary>
    /// <param name="Cancel">cancel request</param>
    /// <param name="Answer">answer string</param>
    /// <returns>true if successful</returns>
    public bool SendMenuAnswer(bool Cancel, string Answer)
    {
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        return GlobalServiceProvider.Get<IControllerService>().SendMenuAnswer(User.CardId, Cancel, Answer);
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Sets a callback handler
    /// </summary>
    /// <param name="CallbackHandler"></param>
    /// <returns></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks CallbackHandler)
    {
      Log.Debug("VC: SetCiMenuHandler");
      try
      {
        if (User.CardId < 0 || !IsOwner())
        {
          return false;
        }
        Log.Debug("VC: SetCiMenuHandler card: {0}, {1}", User.CardId, CallbackHandler);
        return GlobalServiceProvider.Get<IControllerService>().SetCiMenuHandler(User.CardId, CallbackHandler);
      }
      catch (Exception Ex)
      {
        Log.Error("Exception: {0}", Ex.ToString());
        //HandleFailure();
      }
      return false;
    }

    #endregion

    #endregion    

    #endregion

   
  }
}