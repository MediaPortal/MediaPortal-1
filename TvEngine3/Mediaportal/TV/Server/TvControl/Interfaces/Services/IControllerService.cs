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
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
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
  [ServiceKnownType(typeof(DVBTChannel))]
  [ServiceKnownType(typeof(DVBCChannel))]
  [ServiceKnownType(typeof(DVBSChannel))]
  [ServiceKnownType(typeof(ATSCChannel))]
  [ServiceKnownType(typeof(DVBIPChannel))]
  [ServiceKnownType(typeof(AnalogChannel))]
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
    /// Initialized Conditional Access handler
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true if successful</returns>
    [OperationContract]
    bool InitConditionalAccess(int cardId);



    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>cardtype</value>
    [OperationContract]
    CardType Type(int cardId);

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
    /// Gets the device path for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>devicePath of card</returns>
    [OperationContract]
    string CardDevice(int cardId);

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    [OperationContract]
    bool TunerLocked(int cardId);

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    [OperationContract]
    int SignalQuality(int cardId);

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    [OperationContract]
    int SignalLevel(int cardId);

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    [OperationContract]
    void UpdateSignalSate(int cardId);

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
    IChannel[] ScanNIT(int cardId, IChannel channel);

    /// <summary>
    /// returns the minium channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>minium channel number</returns>
    [OperationContract]
    int MinChannel(int cardId);

    /// <summary>
    /// returns the maximum channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>maximum channel number</returns>
    [OperationContract]
    int MaxChannel(int cardId);

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
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="card">returns card is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    [OperationContract]
    bool IsRecordingSchedule(int idSchedule, out IVirtualCard card);

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
    /// Returns a list of all ip adresses on the server.
    /// </summary>
    /// <value>The server ip adresses.</value>
    IEnumerable<string> ServerIpAdresses { [OperationContract] get; }

    #endregion

    #region streaming

    /// <summary>
    /// Returns the port used for RTSP streaming.
    /// If streaming is not initialized, returns 0.
    /// </summary>
    /// <value>The streaming port</value>
    int StreamingPort { [OperationContract] get; }

    /// <summary>
    /// Gets a list of all streaming clients.
    /// </summary>
    /// <value>The streaming clients.</value>
    List<RtspClient> StreamingClients { [OperationContract] get; }

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
    void DiSEqCGotoPosition(int cardId, byte position);

    /// <summary>
    /// Gets the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="satellitePosition">satellite position</param>
    /// <param name="stepsAzimuth">azimuth</param>
    /// <param name="stepsElevation">elvation</param>
    [OperationContract]
    void DiSEqCGetPosition(int cardId, out int satellitePosition, out int stepsAzimuth, out int stepsElevation);

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
    bool TimeShiftGetCurrentFilePosition(string userName, ref long position, ref long bufferId);

    /// <summary>
    /// Returns if the card is currently timeshifting or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    [OperationContract]
    bool IsTimeShifting(string userName);


    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    [OperationContract]
    TimeSpan TeletextRotation(string userName, int pageNumber);

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>
    /// yes if channel has teletext otherwise false
    /// </returns>
    [OperationContract]
    bool HasTeletext(string userName);

    /// <summary>
    /// turn on/off teletext grabbing for a card
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="onOff">boolean indicating if teletext grabbing should be enabled or not</param>
    [OperationContract]
    void GrabTeletext(string userName, bool onOff);

    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>
    /// byte[] array containing the raw teletext page or null if page is not found
    /// </returns>
    [OperationContract]
    byte[] GetTeletextPage(string userName, int pageNumber, int subPageNumber);

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>
    /// number of teletext subpages for the pagenumber
    /// </returns>
    [OperationContract]
    int SubPageCount(string userName, int pageNumber);

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the red button</returns>
    [OperationContract]
    int GetTeletextRedPageNumber(string userName);

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the green button</returns>
    [OperationContract]
    int GetTeletextGreenPageNumber(string userName);

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    [OperationContract]
    int GetTeletextYellowPageNumber(string userName);

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    [OperationContract]
    int GetTeletextBluePageNumber(string userName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="bufferFile1"></param>
    /// <param name="position2"></param>
    /// <param name="bufferFile2"></param>
    /// <param name="recordingFile"></param>
    [OperationContract]
    void CopyTimeShiftFile(Int64 position1, string bufferFile1, Int64 position2, string bufferFile2,
                           string recordingFile);




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
    /// Reloads the configuration for the given card
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    [OperationContract]
    void ReloadCardConfiguration(int cardId);

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
    VIDEOENCODER_BITRATE_MODE GetBitRateMode(int cardId);

    /// <summary>
    /// Sets the bitrate mode
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="bitRateMode">The new bitrate mdoe</param>
    [OperationContract]
    void SetBitRateMode(int cardId, VIDEOENCODER_BITRATE_MODE bitRateMode);

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
    bool SetCiMenuHandler(int cardId, ICiMenuCallbacks callbackHandler);

    

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
    IDictionary<string, byte[]> GetPluginBinariesCustomDevices();

    [OperationContract]
    IDictionary<string, byte[]> GetPluginBinariesResources();

    [OperationContract]    
    IList<StreamPresentation> ListAllStreamingChannels();

    [OperationContract]
    bool IsAnyCardParkedByUser(string userName);

    [OperationContract]
    IList<CardPresentation> ListAllCards();
  }
}