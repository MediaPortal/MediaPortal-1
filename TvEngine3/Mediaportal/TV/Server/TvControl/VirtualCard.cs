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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils;

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
    private ISubChannel _subChannel;

    [DataMember]
    private bool _isTimeshifting;

    [DataMember]
    private bool _isScanning;

    [DataMember]
    private bool _isRecording;

    [DataMember]
    private bool _isGrabbingEpg;

    [DataMember]
    private string _rtspUrl;

    [DataMember]
    private string _recordingFileName;

    [DataMember]
    private string _name;

    [DataMember]
    private string _timeShiftFileName;

    [DataMember]
    private string _channelName;    

    [DataMember]
    private int _idChannel = -1;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    public VirtualCard(ISubChannel subChannel)
    {
      _subChannel = subChannel;

      var controllerService = GlobalServiceProvider.Get<IControllerService>();
      /*if (!string.IsNullOrWhiteSpace(userName))
      {
        _isTimeshifting = controllerService.IsTimeShifting(userName);
        _rtspUrl = controllerService.GetStreamingUrl(userName);
        _recordingFileName = controllerService.RecordingFileName(userName);
        _idChannel = controllerService.CurrentDbChannel(userName);
        _channelName = controllerService.CurrentChannelName(userName);

        if (cardId > 0 && _user.Type == UserType.Normal)
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
      }*/
    }

    #endregion
    
    #region properties

    #region static properties

    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public IUser User
    {
      get { return null; }// _user; }
    }

    /// <summary>
    /// returns the card id of this virtual card
    /// </summary>
    public int Id
    {
      get { return _subChannel.IdTuner; }
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
          if (_subChannel.IdTuner < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().CardName(_subChannel.IdTuner);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
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
          if (_subChannel.IdTuner < 0)
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
          if (_subChannel.IdTuner < 0)
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
          if (_subChannel.IdTuner < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsGrabbingEpg(_subChannel.IdTuner);
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
          //if (_subChannel.IdTuner < 0) return false;
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
          if (_subChannel.IdTuner < 0)
          {
            return false;
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().IsScanning(_subChannel.IdTuner);
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
          if (_subChannel.IdTuner < 0)
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
          if (_subChannel.IdTuner < 0)
          {
            return "";
          }
          RemoteControl.HostName = _server;
          return GlobalServiceProvider.Get<IControllerService>().TimeShiftFileName(User.Name, _subChannel.IdTuner);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return "";*/
      }
      set { _timeShiftFileName = value; }
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
          if (_subChannel.IdTuner < 0)
          {
            return -1;
          }
          return GlobalServiceProvider.Get<IControllerService>().GetRecordingSchedule(_subChannel.IdTuner, User.Name);
        }
        catch (Exception)
        {
          //HandleFailure();
        }
        return -1;
      }
    }

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="forceUpdate"><c>True</c> to force the signal status to be updated, and not use cached information.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public void GetSignalStatus(bool forceUpdate, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      try
      {
        if (_subChannel.IdTuner > 0)
        {
          GlobalServiceProvider.Get<IControllerService>().GetSignalStatus(_subChannel.IdTuner, forceUpdate, out isLocked, out isPresent, out strength, out quality);
        }
      }
      catch (Exception)
      {
        //HandleFailure();
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
        if (_subChannel.IdTuner > 0)
        {
          GlobalServiceProvider.Get<IControllerService>().GetStreamQualityCounters(User.Name, out totalTSpackets, out discontinuityCounter);
        }
      }
      catch (Exception)
      {
        //HandleFailure();
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
          if (_subChannel.IdTuner < 0)
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
          if (_subChannel.IdTuner < 0)
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
    /// Stops the time shifting.
    /// </summary>
    /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
    public void StopTimeShifting()
    {
      try
      {
        if (_subChannel.IdTuner < 0 || !IsTimeShifting)
        {
          return;
        }
        IUser userResult;
        GlobalServiceProvider.Get<IControllerService>().StopTimeShifting(_subChannel.UserName, out userResult);

        if (userResult != null)
        {
          //_user = userResult;          
        }
        _isTimeshifting = false;
        _timeShiftFileName = null;
        _rtspUrl = null;
        _name = null;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
    public void StopRecording()
    {
      try
      {
        if (_subChannel.IdTuner < 0)
        {
          return;
        }
        ISubChannel subChannel;
        GlobalServiceProvider.Get<IControllerService>().StopRecording(_subChannel.Id, out subChannel);
        if (subChannel != null)
        {
          _subChannel = subChannel;
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
    /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
    public TvResult StartRecording(ref string fileName)
    {
      try
      {
        if (_subChannel.IdTuner < 0)
        {
          return TvResult.UnexpectedError;
        }
        IUser userResult;
        TvResult startRecording = GlobalServiceProvider.Get<IControllerService>().StartRecording(_subChannel.UserName, _subChannel.IdTuner, out userResult, ref fileName);

        if (userResult != null)
        {
          //_user = userResult;
        }

        return startRecording;
      }
      catch (Exception)
      {
        //HandleFailure();
      }
      return TvResult.UnexpectedError;
    }

    #endregion

    /// <summary>
    /// Indicates, if the user is the owner of the card
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsOwner()
    {
      try
      {
        if (_subChannel.IdTuner > 0)
        {
          return GlobalServiceProvider.Get<IControllerService>().IsOwner(_subChannel.Id);
        }
      }
      catch
      {
        //HandleFailure();
      }
      return false;
    }

    #region quality control

    /// <summary>
    /// Determine which (if any) quality control features are supported by a tuner.
    /// </summary>
    /// <param name="supportedEncodeModes">The encoding modes supported by the tuner.</param>
    /// <param name="canSetBitRate"><c>True</c> if the tuner's average and/or peak encoding bit-rate can be set.</param>
    public void GetSupportedQualityControlFeatures(out EncodeMode supportedEncodeModes, out bool canSetBitRate)
    {
      supportedEncodeModes = EncodeMode.Default;
      canSetBitRate = false;
      try
      {
        if (_subChannel.IdTuner > 0 && IsOwner())
        {
          GlobalServiceProvider.Get<IControllerService>().GetSupportedQualityControlFeatures(_subChannel.IdTuner, out supportedEncodeModes, out canSetBitRate);
        }
      }
      catch
      {
        //HandleFailure();
      }
    }

    /// <summary>
    /// Get and/or set the tuner's video and/or audio encoding mode.
    /// </summary>
    public EncodeMode EncodeMode
    {
      get
      {
        try
        {
          if (_subChannel.IdTuner > 0)
          {
            return GlobalServiceProvider.Get<IControllerService>().GetEncodeMode(_subChannel.IdTuner);
          }
        }
        catch
        {
          //HandleFailure();
        }
        return EncodeMode.Default;
      }
      set
      {
        try
        {
          if (_subChannel.IdTuner > 0 && IsOwner())
          {
            GlobalServiceProvider.Get<IControllerService>().SetEncodeMode(_subChannel.IdTuner, value);
          }
        }
        catch
        {
          //HandleFailure();
        }
      }
    }

    /// <summary>
    /// Get and/or set the tuner's average video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    public int AverageBitRate
    {
      get
      {
        try
        {
          if (_subChannel.IdTuner > 0)
          {
            return GlobalServiceProvider.Get<IControllerService>().GetAverageBitRate(_subChannel.IdTuner);
          }
        }
        catch
        {
          //HandleFailure();
        }
        return 0;
      }
      set
      {
        try
        {
          if (_subChannel.IdTuner > 0 && IsOwner())
          {
            GlobalServiceProvider.Get<IControllerService>().SetAverageBitRate(_subChannel.IdTuner, value);
          }
        }
        catch
        {
          //HandleFailure();
        }
      }
    }

    /// <summary>
    /// Get and/or set the tuner's peak video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    public int PeakBitRate
    {
      get
      {
        try
        {
          if (_subChannel.IdTuner > 0)
          {
            return GlobalServiceProvider.Get<IControllerService>().GetPeakBitRate(_subChannel.IdTuner);
          }
        }
        catch
        {
          //HandleFailure();
        }
        return 0;
      }
      set
      {
        try
        {
          if (_subChannel.IdTuner > 0 && IsOwner())
          {
            GlobalServiceProvider.Get<IControllerService>().SetPeakBitRate(_subChannel.IdTuner, value);
          }
        }
        catch
        {
          //HandleFailure();
        }
      }
    }

    #endregion

    #region CA Menu Handling

    /// <summary>
    /// Indicates, if the card supports CI Menu
    /// </summary>
    /// <returns>true/false</returns>
    public bool CiMenuSupported()
    {
      try
      {
        if (_subChannel.IdTuner > 0 && IsOwner())
        {
          return GlobalServiceProvider.Get<IControllerService>().CiMenuSupported(_subChannel.IdTuner);
        }
      }
      catch
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
        if (_subChannel.IdTuner > 0 && IsOwner())
        {
          return GlobalServiceProvider.Get<IControllerService>().EnterCiMenu(_subChannel.IdTuner);
        }
      }
      catch
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Selects a ci menu entry
    /// </summary>
    /// <param name="choice">Choice (1 based), 0 for "back"</param>
    /// <returns>true if successful</returns>
    public bool SelectCiMenu(byte choice)
    {
      try
      {
        if (_subChannel.IdTuner > 0 && IsOwner())
        {
          return GlobalServiceProvider.Get<IControllerService>().SelectMenu(_subChannel.IdTuner, choice);
        }
      }
      catch
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
        if (_subChannel.IdTuner > 0 && IsOwner())
        {
          return GlobalServiceProvider.Get<IControllerService>().CloseMenu(_subChannel.IdTuner);
        }
      }
      catch
      {
        //HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Sends an answer to CAM after a request
    /// </summary>
    /// <param name="cancel">cancel request</param>
    /// <param name="answer">answer string</param>
    /// <returns>true if successful</returns>
    public bool SendMenuAnswer(bool cancel, string answer)
    {
      try
      {
        if (_subChannel.IdTuner > 0 && IsOwner())
        {
          return GlobalServiceProvider.Get<IControllerService>().SendMenuAnswer(_subChannel.IdTuner, cancel, answer);
        }
      }
      catch
      {
        //HandleFailure();
      }
      return false;
    }

    #endregion

    #endregion
  }
}