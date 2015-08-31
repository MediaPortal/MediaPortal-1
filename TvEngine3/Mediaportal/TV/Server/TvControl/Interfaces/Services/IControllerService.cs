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
using System.ServiceModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  /// <summary>
  /// interface class describing all methods available
  /// to remote-control the TVService
  /// </summary>
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  [ServiceKnownType(typeof(User))]
  [ServiceKnownType(typeof(SubChannel))]
  [ServiceKnownType(typeof(VirtualCard))]
  [ServiceKnownType(typeof(TvResult))]
  [ServiceKnownType(typeof(ChannelAnalogTv))]
  [ServiceKnownType(typeof(ChannelAtsc))]
  [ServiceKnownType(typeof(ChannelCapture))]
  [ServiceKnownType(typeof(ChannelDigiCipher2))]
  [ServiceKnownType(typeof(ChannelDvbC))]
  [ServiceKnownType(typeof(ChannelDvbC2))]
  [ServiceKnownType(typeof(ChannelDvbS))]
  [ServiceKnownType(typeof(ChannelDvbS2))]
  [ServiceKnownType(typeof(ChannelDvbT))]
  [ServiceKnownType(typeof(ChannelDvbT2))]
  [ServiceKnownType(typeof(ChannelFmRadio))]
  [ServiceKnownType(typeof(ChannelSatelliteTurboFec))]
  [ServiceKnownType(typeof(ChannelScte))]
  [ServiceKnownType(typeof(ChannelStream))]
  [ServiceKnownType(typeof(LnbTypeBLL))]
  public interface IControllerService
  {
    #region internal interface

    /// <summary>
    /// Gets the assembly of tvservice.exe
    /// </summary>
    /// <value>Returns the AssemblyVersion of tvservice.exe</value>    
    string GetAssemblyVersion { [OperationContract] get; }

    ///<summary>
    ///Gets the total number of tv-cards installed.
    ///</summary>
    ///<value>Number which indicates the cards installed</value>
    int Cards { [OperationContract] get; }

    /// <summary>
    /// Get the broadcast standards supported by the tuner hardware.
    /// </summary>
    /// <remarks>
    /// This property is configurable.
    /// </remarks>
    [OperationContract]
    BroadcastStandard SupportedBroadcastStandards(int cardId);

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    /// <remarks>
    /// This property is based on detected limitations and hard code capabilities.
    /// </remarks>
    [OperationContract]
    BroadcastStandard PossibleBroadcastStandards(int cardId);

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>name of card</returns>
    [OperationContract]
    string CardName(int cardId);

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    [OperationContract]
    bool CanTune(int cardId, IChannel channel);

    /// <summary>
    /// Method to check if card is currently present and detected
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>
    [OperationContract]
    bool IsCardPresent(int cardId);

    /// <summary>
    /// Method to remove a non-present card from the local card collection
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    [OperationContract]
    void CardRemove(int cardId);

    /// <summary>
    /// Reload the configuration for a tuner.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    [OperationContract]
    void ReloadTunerConfiguration(int tunerId);

    /// <summary>
    /// Reload the configuration for a set of tuners.
    /// </summary>
    /// <param name="tunerIds">The tuner identifiers.</param>
    [OperationContract(Name = "ReloadTunerConfigurationMultiple")]
    void ReloadTunerConfiguration(IEnumerable<int> tunerIds);

    /// <summary>
    /// Get a tuner's signal status.
    /// </summary>
    /// <param name="cardId">The tuner's identifier.</param>
    /// <param name="forceUpdate"><c>True</c> to force the signal status to be updated, and not use cached information.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    [OperationContract]
    void GetSignalStatus(int cardId, bool forceUpdate, out bool isLocked, out bool isPresent, out int strength, out int quality);

    /// <summary>
    /// Returns if the card is currently scanning for channels or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    [OperationContract]
    bool IsScanning(int cardId);

    /// <summary>
    /// Returns if the card is currently grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    [OperationContract]
    bool IsGrabbingEpg(int cardId);

    /// <summary>
    /// scans current transponder for channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">contains tuningdetails for the transponder.</param>
    /// <returns>list of all channels found</returns>    
    [OperationContract(Name = "Scan")]
    IChannel[] Scan(int cardId, IChannel channel);

    /// <summary>
    /// scans nit the current transponder for channels
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">contains tuningdetails for the transponder.</param>
    /// <returns>list of all channels found</returns>
    [OperationContract]
    TuningDetail[] ScanNIT(int cardId, IChannel channel);

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="userName"> </param>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    [OperationContract]
    int GetRecordingSchedule(int cardId, string userName);

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param>
    /// <returns>URL containing the RTSP adress on which the recording can be found</returns>
    [OperationContract]
    string GetRecordingUrl(int idRecording);

    /// <summary>
    /// Returns the contents of the chapters file (if any) for a recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param>
    /// <returns>The contents of the chapters file of the recording</returns>
    [OperationContract]
    string GetRecordingChapters(int idRecording);

    /// <summary>
    /// Deletes the recording from database and disk
    /// </summary>
    /// <param name="idRecording">The id recording.</param>
    [OperationContract]
    bool DeleteRecording(int idRecording);

    /// <summary>
    /// Deletes invalid recordings from database. A recording is invalid if the corresponding file no longer exists.
    /// </summary>
    [OperationContract]
    bool DeleteInvalidRecordings();

    /// <summary>
    /// Deletes watched recordings from database.
    /// </summary>
    [OperationContract]
    bool DeleteWatchedRecordings(string currentTitle);

    /// <summary>
    /// Checks if the schedule specified is currently being recorded.
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    [OperationContract]
    bool IsRecordingSchedule(int idSchedule);

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="idChannel"></param>
    /// <param name="card">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    [OperationContract]
    bool IsRecording(int idChannel, out IVirtualCard card);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idChannel"></param>
    /// <param name="idCard"></param>
    /// <returns></returns>
    [OperationContract(Name = "IsRecordingCard")]        
    bool IsRecording(int idChannel, int idCard);

    /// <summary>
    /// Determines if any card is currently busy recording
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if a card is recording; otherwise, <c>false</c>.
    /// </returns>
    [OperationContract]
    bool IsAnyCardRecording();

    /// <summary>
    /// Determines if any card is currently busy recording or timeshifting
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="isUserTS">true if the specified user is timeshifting</param>
    /// <param name="isAnyUserTS">true if any user (except for the userTS) is timeshifting</param>
    /// <param name="isRec">true if recording</param>
    /// <returns>
    /// 	<c>true</c> if a card is recording or timeshifting; otherwise, <c>false</c>.
    /// </returns>
    [OperationContract]
    bool IsAnyCardRecordingOrTimeshifting(string userName, out bool isUserTS, out bool isAnyUserTS, out bool isRec);

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    [OperationContract]
    void StopRecordingSchedule(int idSchedule);

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    [OperationContract]
    void OnNewSchedule();

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    bool EpgGrabberEnabled { [OperationContract] get; [OperationContract] set; }

    /// <summary>
    /// Restarts the service.
    /// </summary>
    [OperationContract]
    void Restart();

    /// <summary>
    /// Determines whether the card is in use
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user who uses the card.</param>
    /// <returns>
    /// 	<c>true</c> if card is in use; otherwise, <c>false</c>.
    /// </returns>
    [OperationContract]
    bool IsCardInUse(int cardId, out IUser user);

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>
    /// <param name="userName"> </param>
    [OperationContract]
    IDictionary<int, ChannelState> GetAllChannelStatesCached(string userName);

    /// <summary>
    /// Finds out whether a channel is currently tuneable or not
    /// </summary>
    /// <param name="idChannel">The channel id</param>
    /// <param name="userName"> </param>
    /// <returns>an enum indicating tunable/timeshifting/recording</returns>
    [OperationContract]
    ChannelState GetChannelState(int idChannel, string userName);

    /// <summary>
    /// Returns a list of all IP addresses on the server.
    /// </summary>
    /// <value>The server IP addresses.</value>
    IEnumerable<string> ServerIpAddresses { [OperationContract] get; }

    #endregion

    #region streaming

    /// <summary>
    /// Get the streaming server's information.
    /// </summary>
    /// <param name="boundInterface">The interface (IP address) that the server is bound to.</param>
    /// <param name="port">The port that the server is listening on.</param>
    [OperationContract]
    void GetStreamingServerInformation(out string boundInterface, out ushort port);

    /// <summary>
    /// Gets a list of all streaming clients.
    /// </summary>
    /// <value>The streaming clients.</value>
    ICollection<RtspClient> StreamingClients { [OperationContract] get; }

    /// <summary>
    /// Disconnect a streaming client.
    /// </summary>
    /// <param name="sessionId">The client's session identifier.</param>
    [OperationContract]
    void DisconnectStreamingClient(uint sessionId);

    #endregion

    #region DiSEqC

    /// <summary>
    /// Reset DiSEqC for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    [OperationContract]
    void DiSEqCReset(int cardId);

    /// <summary>
    /// Stops the DiSEqC motor for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    [OperationContract]
    void DiSEqCStopMotor(int cardId);

    /// <summary>
    /// Sets the DiSEqC east limit for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    [OperationContract]
    void DiSEqCSetEastLimit(int cardId);

    /// <summary>
    /// Sets the DiSEqC west limit for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    [OperationContract]
    void DiSEqCSetWestLimit(int cardId);

    /// <summary>
    /// DiSEqC force limit  for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="onoff">on/off</param>
    [OperationContract]
    void DiSEqCForceLimit(int cardId, bool onoff);

    /// <summary>
    /// Moves the DiSEqC motor for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="direction">direction</param>
    /// <param name="numberOfSteps">Number of steps</param>
    [OperationContract]
    void DiSEqCDriveMotor(int cardId, DiseqcDirection direction, byte numberOfSteps);

    /// <summary>
    /// Stores the current DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="position">position</param>
    [OperationContract]
    void DiSEqCStorePosition(int cardId, byte position);

    /// <summary>
    /// DiSEqC move to the reference position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    [OperationContract]
    void DiSEqCGotoReferencePosition(int cardId);

    /// <summary>
    /// Go to the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="position">position</param>
    [OperationContract]
    void DiSEqCGotoStoredPosition(int cardId, byte position);

    /// <summary>
    /// Gets the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="satellitePosition">satellite position</param>
    /// <param name="satelliteLongitude">The satellite's longitude.</param>
    /// <param name="stepsAzimuth">azimuth</param>
    /// <param name="stepsElevation">elvation</param>
    [OperationContract]
    void DiSEqCGetPosition(int cardId, out int satellitePosition, out double satelliteLongitude, out int stepsAzimuth, out int stepsElevation);

    #endregion

    #region sub channels

    /// <summary>
    /// Returns the subchannels count for the selected card
    /// stream for the selected card
    /// </summary>
    /// <param name="idCard">card id.</param>
    /// <returns>
    /// subchannels count
    /// </returns>
    [OperationContract]
    int GetSubChannels(int idCard);

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for the selected card
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>
    /// URL containing the RTSP adress on which the card transmits its stream
    /// </returns>
    [OperationContract]
    string GetStreamingUrl(string userName);

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// filename of the recording or null when not recording
    /// </returns>
    [OperationContract]
    string RecordingFileName(string user);

    /// <summary>
    /// Gets the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel"> </param>
    /// <returns>IChannel</returns>
    [OperationContract]
    IChannel CurrentChannel(string userName, int idChannel);

    /// <summary>
    /// returns the id of the current channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns></returns>
    [OperationContract]
    int CurrentDbChannel(string userName);

    /// <summary>
    /// Gets the name of the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>channel name</returns>
    [OperationContract]
    string CurrentChannelName(string userName);

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>
    /// yes if channel is scrambled and CI/CAM cannot decode it, otherwise false
    /// </returns>
    [OperationContract]
    bool IsScrambled(string userName);

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="cardId"> </param>
    /// <returns>
    /// timeshifting filename null when not timeshifting
    /// </returns>
    [OperationContract]
    string TimeShiftFileName(string userName, int cardId);

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    [OperationContract]
    bool TimeShiftGetCurrentFilePosition(string userName, out long position, out long bufferId);

    /// <summary>
    /// Returns if the card is currently timeshifting or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    [OperationContract]
    bool IsTimeShifting(string userName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="position1"></param>
    /// <param name="bufferId1"></param>
    /// <param name="position2"></param>
    /// <param name="bufferId2"></param>
    /// <param name="destination"></param>
    [OperationContract]
    void CopyTimeShiftBuffer(string userName, long position1, long bufferId1, long position2, long bufferId2, string destination);

    /// <summary>
    /// Stops the card.
    /// </summary>
    /// <param name="idCard"> </param>
    [OperationContract]
    void StopCard(int idCard);

    /// <summary>
    /// Park timeshifting for the user supplied
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="duration"> </param>
    /// <param name="idChannel"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [OperationContract]
    bool ParkTimeShifting(string userName, double duration, int idChannel, out IUser user);

    /// <summary>
    /// UnPark timeshifting for the user supplied
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="duration"> </param>
    /// <param name="idChannel"></param>
    /// <param name="user"></param>
    /// <param name="card"> </param>
    /// <returns></returns>
    [OperationContract]
    bool UnParkTimeShifting(string userName, double duration, int idChannel, out IUser user, out IVirtualCard card);

    /// <summary>
    /// Query what card would be used for timeshifting on any given channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>
    /// returns card id which would be used when doing the actual timeshifting.
    /// </returns>
    [OperationContract]
    int TimeShiftingWouldUseCard(string userName, int idChannel);

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <param name="user">user credentials.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>    
    [OperationContract(Name = "StartTimeShiftingForceCardId")]    
    TvResult StartTimeShifting(string userName, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, bool forceCardId, out IUser user);

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <param name="parkedDuration"> </param>
    /// <param name="user">user credentials.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    [OperationContract(Name = "StartTimeShiftingGetCardChanged")]
    TvResult StartTimeShifting(string userName, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, out bool cardChanged, out double? parkedDuration, out IUser user);

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <param name="user">user credentials.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    [OperationContract(Name = "StartTimeShiftingGetCard")]
    TvResult StartTimeShifting(string userName, int idChannel, out IVirtualCard card, out IUser user);

    [OperationContract(Name = "StartTimeShiftingPriorityGetCard")]
    TvResult StartTimeShifting(string userName, int userPriority, int idChannel, out IVirtualCard card, out IUser user);


    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="user">user credentials.</param>
    /// <param name="reason">reason why timeshifting is stopped.</param>
    /// <returns>true if success otherwise false</returns>
    [OperationContract(Name = "StopTimeShiftingGetReason")]
    bool StopTimeShifting(string userName, out IUser user, TvStoppedReason reason);

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="user">user credentials.</param>
    /// <param name="channelId"> </param>
    /// <returns>true if success otherwise false</returns>
    [OperationContract]
    bool StopTimeShifting(string userName, out IUser user);

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="cardId"> </param>
    /// <param name="user">The user.</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns>true if success otherwise false</returns>
    [OperationContract]
    TvResult StartRecording(string userName, int cardId, out IUser user, ref string fileName);

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idCard"> </param>
    /// <param name="user">The user.</param>
    /// <returns>true if success otherwise false</returns>
    [OperationContract]
    bool StopRecording(string userName, int idCard, out IUser user);

    /// <summary>
    /// Scan the specified card to the channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idCard"> </param>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    [OperationContract(Name = "ScanByUser")]
    TvResult Scan(string userName, int idCard, out IUser user, IChannel channel, int idChannel);

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idCard"> </param>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    [OperationContract]
    TvResult Tune(string userName, int idCard, out IUser user, IChannel channel, int idChannel);

    /// <summary>
    /// Gets the users for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    [OperationContract]
    IDictionary<string, IUser> GetUsersForCard(int cardId);

    /// <summary>
    /// Determines whether the the user is the owner of the card
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="userName"> </param>
    /// <returns>
    /// 	<c>true</c> if the specified user is the card owner; otherwise, <c>false</c>.
    /// </returns>
    [OperationContract]
    bool IsOwner(int cardId, string userName);

    #endregion

    #region quality control

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    [OperationContract]
    bool SupportsQualityControl(int cardId);

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    [OperationContract]
    bool SupportsBitRateModes(int cardId);

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    [OperationContract]
    bool SupportsPeakBitRateMode(int cardId);

    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    [OperationContract]
    bool SupportsBitRate(int cardId);

    /// <summary>
    /// Gets the current quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    [OperationContract]
    QualityType GetQualityType(int cardId);

    /// <summary>
    /// Sets the quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="qualityType">The new quality type</param>
    [OperationContract]
    void SetQualityType(int cardId, QualityType qualityType);

    /// <summary>
    /// Gets the current bitrate mdoe
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    [OperationContract]
    EncoderBitRateMode GetBitRateMode(int cardId);

    /// <summary>
    /// Sets the bitrate mode
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="bitRateMode">The new bitrate mdoe</param>
    [OperationContract]
    void SetBitRateMode(int cardId, EncoderBitRateMode bitRateMode);

    #endregion

    #region CI Menu support

    /// <summary>
    /// Indicates if CI Menu is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    [OperationContract]
    bool CiMenuSupported(int cardId);

    /// <summary>
    /// Enters the ci menu of card
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    [OperationContract]
    bool EnterCiMenu(int cardId);

    /// <summary>
    /// Selects a ci menu option
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="choice">choice</param>
    /// <returns>true if successful</returns>
    [OperationContract]
    bool SelectMenu(int cardId, byte choice);

    /// <summary>
    /// CloseMenu closes the menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    [OperationContract]
    bool CloseMenu(int cardId);

    /// <summary>
    /// Sends a answer to cam after a request
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="cancel">cancel request</param>
    /// <param name="answer">answer string</param>
    /// <returns></returns>
    [OperationContract]
    bool SendMenuAnswer(int cardId, bool cancel, string answer);

    /// <summary>
    /// Registers a ci menu callback handler for user interaction
    /// </summary>
    /// <param name="cardId"></param>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    [OperationContract]
    bool SetCiMenuHandler(int cardId, IConditionalAccessMenuCallBack callbackHandler);

    #endregion

    #region stream quality / statistics

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="totalTSpackets">Amount of packets processed</param>
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    [OperationContract]
    void GetStreamQualityCounters(string userName, out int totalTSpackets, out int discontinuityCounter);

    #endregion

    [OperationContract]
    void RegisterUserForHeartbeatMonitoring (string username);

    [OperationContract]
    void RegisterUserForCiMenu(string username);

    [OperationContract]
    void UnRegisterUserForHeartbeatMonitoring(string username);

    [OperationContract]
    void UnRegisterUserForCiMenu(string username);

    [OperationContract]
    void RegisterUserForTvServerEvents(string username);

    [OperationContract]
    void UnRegisterUserForTvServerEvents(string username);

    [OperationContract]    
    IDictionary<string, byte[]> GetPluginBinaries();

    [OperationContract]
    IDictionary<string, byte[]> GetPluginBinariesTunerExtensions();

    [OperationContract]
    IDictionary<string, byte[]> GetPluginBinariesResources();

    [OperationContract]    
    IList<StreamPresentation> ListAllStreamingChannels();

    [OperationContract]
    bool IsAnyCardParkedByUser(string userName);

    [OperationContract]
    IList<CardPresentation> ListAllCards();

    [OperationContract]
    void ReloadControllerConfiguration();

    [OperationContract]
    void GetBdaFixStatus(out bool isApplicable, out bool isNeeded);

    [OperationContract]
    void GetMceServiceStatus(out bool isServiceInstalled, out bool isServiceRunning, out bool isPolicyActive);

    [OperationContract]
    void ApplyMceServicePolicy();

    [OperationContract]
    void RemoveMceServicePolicy();

    #region thumbnails

    [OperationContract]
    byte[] GetThumbnailForRecording(string recordingFileName);

    [OperationContract]
    void CreateMissingThumbnails();

    [OperationContract]
    void DeleteExistingThumbnails();

    #endregion
  }
}