/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using TvLibrary.Interfaces;
using TvLibrary.Streaming;

namespace TvControl
{
  /// <summary>
  /// enum describing the possible result codes for the tv engine when TV suddenly stops
  /// </summary>
  public enum TvStoppedReason
  {
    /// <summary>
    /// Timeshifting stopped because of an unknown reason.
    /// </summary>
    UnknownReason,
    /// <summary>
    /// Timeshifting stopped because a recording started which needed the card.
    /// </summary>
    RecordingStarted,
    /// <summary>
    /// Timeshifting stopped because client was kicked by server admin.
    /// </summary>
    KickedByAdmin,
    /// <summary>
    /// Timeshifting stopped because client heartbeat timed out.
    /// </summary>
    HeartBeatTimeOut,
    /// <summary>
    /// Timeshifting stopped because the owner of the same transponder has decided to change transponder.
    /// </summary>
    OwnerChangedTS
  }

  /// <summary>
  /// enum describing the possible result codes for the tv engine
  /// </summary>
  public enum TvResult
  {
    /// <summary>
    /// Operation succeeded
    /// </summary>
    Succeeded,
    /// <summary>
    /// Operation failed since all cards are busy and no free card could be found
    /// </summary>
    AllCardsBusy,
    /// <summary>
    /// Operation failed since channel is encrypted
    /// </summary>
    ChannelIsScrambled,
    /// <summary>
    /// Opetation failed since no audio/video was detected after tuning
    /// </summary>
    NoVideoAudioDetected,
    /// <summary>
    /// Operation failed since no signal was detected
    /// </summary>
    NoSignalDetected,
    /// <summary>
    /// Operation failed due to an unknown error
    /// </summary>
    UnknownError,
    /// <summary>
    /// Operation failed since the graph could not be build or started
    /// </summary>
    UnableToStartGraph,
    /// <summary>
    /// Operation failed since the channel is unknown
    /// </summary>
    UnknownChannel,
    /// <summary>
    /// Operation failed since the there is no tuning information for the channel
    /// </summary>
    NoTuningDetails,
    /// <summary>
    /// Operation failed since the channel is not mapped to any card
    /// </summary>
    ChannelNotMappedToAnyCard,
    /// <summary>
    /// Operation failed since the card is disabled
    /// </summary>
    CardIsDisabled,
    /// <summary>
    /// Operation failed since we are unable to connect to the slave server
    /// </summary>
    ConnectionToSlaveFailed,
    /// <summary>
    /// Operation failed since we are not the owner of the card
    /// </summary>
    NotTheOwner,
    /// <summary>
    /// Operation failed since we are unable to build the graph
    /// </summary>
    GraphBuildingFailed,
    /// <summary>
    /// Operation failed since we can't find a suitable software encoder
    /// </summary>
    SWEncoderMissing
  }

  /// <summary>
  /// current availability of a specific channel
  /// </summary>
  public enum ChannelState
  {
    /// <summary>
    /// the channel cannot be tuned right now - maybe all cards are busy
    /// </summary>
    nottunable = 0,
    /// <summary>
    /// the channel can be zapped
    /// </summary>
    tunable = 1,
    /// <summary>
    /// this channel is currently timeshifted by one card
    /// </summary>
    timeshifting = 2,
    /// <summary>
    /// this channel is currently being recorded
    /// </summary>
    recording = 3,
  }

  /// <summary>
  /// interface class describing all methods available
  /// to remote-control the TVService
  /// </summary>
  public interface IController
  {
    #region internal interface

    /// <summary>
    /// Gets the assembly of tvservice.exe
    /// </summary>
    /// <value>Returns the AssemblyVersion of tvservice.exe</value>
    string GetAssemblyVersion { get; }

    /// <summary>
    /// Gets the server.
    /// </summary>
    /// <value>The server.</value>
    int IdServer { get; }

    ///<summary>
    ///Gets the total number of tv-cards installed.
    ///</summary>
    ///<value>Number which indicates the cards installed</value>
    int Cards { get; }

    /// <summary>
    /// Initialized Conditional Access handler
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true if successful</returns>
    bool InitConditionalAccess(int cardId);

    /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardIndex">Index of the card.</param>
    /// <value>id of card</value>
    int CardId(int cardIndex);

    /// <summary>
    /// returns if the card is enabled or disabled
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>true if enabled, otherwise false</value>
    bool Enabled(int cardId);

    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>cardtype</value>
    CardType Type(int cardId);

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>name of card</returns>
    string CardName(int cardId);

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    bool CanTune(int cardId, IChannel channel);

    /// <summary>
    /// Method to check if card is currently present and detected
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>
    bool CardPresent(int cardId);

    /// <summary>
    /// Method to remove a non-present card from the local card collection
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    void CardRemove(int cardId);

    /// <summary>
    /// Gets the device path for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>devicePath of card</returns>
    string CardDevice(int cardId);

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    bool TunerLocked(int cardId);

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    int SignalQuality(int cardId);

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    int SignalLevel(int cardId);

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    void UpdateSignalSate(int cardId);

    /// <summary>
    /// Returns if the card is currently scanning for channels or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    bool IsScanning(int cardId);

    /// <summary>
    /// Returns if the card is currently grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    bool IsGrabbingEpg(int cardId);

    /// <summary>
    /// scans current transponder for channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">contains tuningdetails for the transponder.</param>
    /// <returns>list of all channels found</returns>
    IChannel[] Scan(int cardId, IChannel channel);

    /// <summary>
    /// scans nit the current transponder for channels
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">contains tuningdetails for the transponder.</param>
    /// <returns>list of all channels found</returns>
    IChannel[] ScanNIT(int cardId, IChannel channel);

    /// <summary>
    /// returns the minium channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>minium channel number</returns>
    int MinChannel(int cardId);

    /// <summary>
    /// returns the maximum channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>maximum channel number</returns>
    int MaxChannel(int cardId);

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="channelId">channel id</param>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    int GetRecordingSchedule(int cardId, int channelId);

    /// <summary>
    /// Clears the cache.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param>
    /// <returns>URL containing the RTSP adress on which the recording can be found</returns>
    string GetRecordingUrl(int idRecording);

    /// <summary>
    /// Deletes the recording from database and disk
    /// </summary>
    /// <param name="idRecording">The id recording.</param>
    bool DeleteRecording(int idRecording);

    /// <summary>
    /// Checks if the files of a recording still exist
    /// </summary>
    /// <param name="idRecording">The id of the recording</param>
    bool IsRecordingValid(int idRecording);

    /// <summary>
    /// Gets the rtsp URL for file located on the tvserver.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>rtsp url</returns>
    string GetUrlForFile(string fileName);

    /// <summary>
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="card">returns card is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    bool IsRecordingSchedule(int idSchedule, out VirtualCard card);

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="card">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    bool IsRecording(string channelName, out VirtualCard card);

    /// <summary>
    /// Determines if any card is currently busy recording
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if a card is recording; otherwise, <c>false</c>.
    /// </returns>
    bool IsAnyCardRecording();

    /// <summary>
    /// Determines if any card is currently busy recording or timeshifting
    /// </summary>
    /// <param name="userTS">timeshifting user</param>
    /// <param name="isUserTS">true if the specified user is timeshifting</param>
    /// <param name="isAnyUserTS">true if any user (except for the userTS) is timeshifting</param>
    /// <param name="isRec">true if recording</param>
    /// <returns>
    /// 	<c>true</c> if a card is recording or timeshifting; otherwise, <c>false</c>.
    /// </returns>
    bool IsAnyCardRecordingOrTimeshifting(User userTS, out bool isUserTS, out bool isAnyUserTS, out bool isRec);

    /// <summary>
    /// Determines if any card is not locked by a user
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if a card is idle; otherwise, <c>false</c>.
    /// </returns>
    bool IsAnyCardIdle();

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    void StopRecordingSchedule(int idSchedule);

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    void OnNewSchedule();

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    void OnNewSchedule(EventArgs args);

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    bool EpgGrabberEnabled { get; set; }


    /// <summary>
    /// Returns the SQl connection string to the database
    /// </summary>
    void GetDatabaseConnectionString(out string connectionString, out string provider);

    /// <summary>
    /// Sets the database connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="provider">The provider.</param>
    void SetDatabaseConnectionString(string connectionString, string provider);

    /// <summary>
    /// Restarts the service.
    /// </summary>
    void Restart();

    /// <summary>
    /// Determines whether the card is in use
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user who uses the card.</param>
    /// <returns>
    /// 	<c>true</c> if card is in use; otherwise, <c>false</c>.
    /// </returns>
    bool IsCardInUse(int cardId, out User user);

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>    
    /// <param name="user"></param>      
    Dictionary<int, ChannelState> GetAllChannelStatesCached(User user);

    /// <summary>
    /// Fetches all channel states for a specific group
    /// </summary>
    /// <param name="idGroup"></param>    
    /// <param name="user"></param>        
    Dictionary<int, ChannelState> GetAllChannelStatesForGroup(int idGroup, User user);

    /// <summary>
    /// Finds out whether a channel is currently tuneable or not
    /// </summary>
    /// <param name="idChannel">The channel id</param>
    /// <param name="user">User</param>
    /// <returns>an enum indicating tunable/timeshifting/recording</returns>
    ChannelState GetChannelState(int idChannel, User user);

    /// <summary>
    /// Fetches all channels with backbuffer
    /// </summary>
    /// <param name="currentRecChannels"></param>
    /// <param name="currentTSChannels"></param>
    /// <param name="currentUnavailChannels"></param>
    /// <param name="currentAvailChannels"></param>
    void GetAllRecordingChannels(out List<int> currentRecChannels, out List<int> currentTSChannels,
                                 out List<int> currentUnavailChannels, out List<int> currentAvailChannels);

    /// <summary>
    /// Returns a list of all ip adresses on the server.
    /// </summary>
    /// <value>The server ip adresses.</value>
    List<string> ServerIpAdresses { get; }

    #endregion

    #region streaming

    /// <summary>
    /// Gets a list of all streaming clients.
    /// </summary>
    /// <value>The streaming clients.</value>
    List<RtspClient> StreamingClients { get; }

    #endregion

    #region DiSEqC

    /// <summary>
    /// Reset DiSEqC for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    void DiSEqCReset(int cardId);

    /// <summary>
    /// Stops the DiSEqC motor for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    void DiSEqCStopMotor(int cardId);

    /// <summary>
    /// Sets the DiSEqC east limit for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    void DiSEqCSetEastLimit(int cardId);

    /// <summary>
    /// Sets the DiSEqC west limit for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    void DiSEqCSetWestLimit(int cardId);

    /// <summary>
    /// DiSEqC force limit  for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="onoff">on/off</param>
    void DiSEqCForceLimit(int cardId, bool onoff);

    /// <summary>
    /// Moves the DiSEqC motor for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="direction">direction</param>
    /// <param name="numberOfSteps">Number of steps</param>
    void DiSEqCDriveMotor(int cardId, DiSEqCDirection direction, byte numberOfSteps);

    /// <summary>
    /// Stores the current DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="position">position</param>
    void DiSEqCStorePosition(int cardId, byte position);

    /// <summary>
    /// DiSEqC move to the reference position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    void DiSEqCGotoReferencePosition(int cardId);

    /// <summary>
    /// Go to the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="position">position</param>
    void DiSEqCGotoPosition(int cardId, byte position);

    /// <summary>
    /// Gets the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="satellitePosition">satellite position</param>
    /// <param name="stepsAzimuth">azimuth</param>
    /// <param name="stepsElevation">elvation</param>
    void DiSEqCGetPosition(int cardId, out int satellitePosition, out int stepsAzimuth, out int stepsElevation);

    #endregion

    #region sub channels

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for the selected card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// URL containing the RTSP adress on which the card transmits its stream
    /// </returns>
    string GetStreamingUrl(User user);

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// filename of the recording or null when not recording
    /// </returns>
    string RecordingFileName(ref User user);

    /// <summary>
    /// Gets the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>IChannel</returns>
    IChannel CurrentChannel(ref User user);

    /// <summary>
    /// returns the id of the current channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    int CurrentDbChannel(ref User user);

    /// <summary>
    /// Gets the name of the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>channel name</returns>
    string CurrentChannelName(ref User user);

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// yes if channel is scrambled and CI/CAM cannot decode it, otherwise false
    /// </returns>
    bool IsScrambled(ref User user);

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// timeshifting filename null when not timeshifting
    /// </returns>
    string TimeShiftFileName(ref User user);

    /// <summary>
    /// Returns if the card is currently timeshifting or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    bool IsTimeShifting(ref User user);


    /// <summary>
    /// This function checks whether something should be recorded at the given time.
    /// </summary>
    /// <param name="time">the time to check for recordings.</param>
    /// <returns>true if any recording due to time</returns>
    bool IsTimeToRecord(DateTime time);

    /// <summary>
    /// This function checks whether something should be recorded at the given time.
    /// </summary>
    /// <param name="time">the time to check for recordings.</param>
    /// <param name="scheduleId">schedule id</param>
    /// <returns>true if any recording due to time</returns>
    bool IsTimeToRecord(DateTime time, int scheduleId);


    /// <summary>
    /// Returns if the card is currently recording or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// true when card is recording otherwise false
    /// </returns>
    bool IsRecording(ref User user);

    /// <summary>
    /// Returns if the card is currently grabbing teletext or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// true when card is grabbing teletext otherwise false
    /// </returns>
    bool IsGrabbingTeletext(User user);

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    TimeSpan TeletextRotation(User user, int pageNumber);

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// yes if channel has teletext otherwise false
    /// </returns>
    bool HasTeletext(User user);

    /// <summary>
    /// turn on/off teletext grabbing for a card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="onOff">boolean indicating if teletext grabbing should be enabled or not</param>
    void GrabTeletext(User user, bool onOff);

    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>
    /// byte[] array containing the raw teletext page or null if page is not found
    /// </returns>
    byte[] GetTeletextPage(User user, int pageNumber, int subPageNumber);

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>
    /// number of teletext subpages for the pagenumber
    /// </returns>
    int SubPageCount(User user, int pageNumber);

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the red button</returns>
    int GetTeletextRedPageNumber(User user);

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the green button</returns>
    int GetTeletextGreenPageNumber(User user);

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    int GetTeletextYellowPageNumber(User user);

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    int GetTeletextBluePageNumber(User user);

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// DateTime containg the date/time when timeshifting was started
    /// </returns>
    DateTime TimeShiftStarted(User user);

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// DateTime containg the date/time when recording was started
    /// </returns>
    DateTime RecordingStarted(User user);

    #region audio stream selection

    /// <summary>
    /// returns the list of available audio streams for the card specified
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>List containing all audio streams</returns>
    IAudioStream[] AvailableAudioStreams(User user);


    /// <summary>
    /// returns the current selected audio stream for the card specified
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>current audio stream</returns>
    IAudioStream GetCurrentAudioStream(User user);


    /// <summary>
    /// Sets the current audio stream for the card specified
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="stream">audio stream</param>
    void SetCurrentAudioStream(User user, IAudioStream stream);

    #endregion

    /// <summary>
    /// returns the current video stream on the virtual card. 
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>List containing all audio streams</returns>
    int GetCurrentVideoStream(User user);


    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    TvResult StartTimeShifting(ref User user, ref string fileName);

    /// <summary>
    /// Stops the card.
    /// </summary>
    /// <param name="user">The user.</param>
    void StopCard(User user);


    /// <summary>
    /// Query what card would be used for timeshifting on any given channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>    
    /// <returns>
    /// returns card id which would be used when doing the actual timeshifting.
    /// </returns>
    int TimeShiftingWouldUseCard(ref User user, int idChannel);

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="user">user credentials.</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    TvResult StartTimeShifting(ref User user, int idChannel, out VirtualCard card);


    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="reason">reason why timeshifting is stopped.</param>
    /// <returns>true if success otherwise false</returns>
    bool StopTimeShifting(ref User user, TvStoppedReason reason);

    /// <summary>
    /// Gets the reason why timeshifting stopped.
    /// </summary>
    /// <param name="user">The user.</param>		
    /// <returns>TvStoppedReason</returns>
    TvStoppedReason GetTvStoppedReason(User user);

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <returns>true if success otherwise false</returns>
    bool StopTimeShifting(ref User user);

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">not used</param>
    /// <param name="startTime">not used</param>
    /// <returns>true if success otherwise false</returns>
    bool StartRecording(ref User user, ref string fileName, bool contentRecording, long startTime);

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>true if success otherwise false</returns>
    bool StopRecording(ref User user);

    /// <summary>
    /// Tune the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    TvResult Tune(ref User user, IChannel channel, int idChannel);

    /// <summary>
    /// Determines whether the card is currently tuned to the transponder
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="transponder">The transponder.</param>
    /// <returns>
    /// 	<c>true</c> if card is tuned to the transponder; otherwise, <c>false</c>.
    /// </returns>
    bool IsTunedToTransponder(int cardId, IChannel transponder);

    /// <summary>
    /// Gets the user for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    User GetUserForCard(int cardId);

    /// <summary>
    /// Gets the users for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    User[] GetUsersForCard(int cardId);

    /// <summary>
    /// Determines whether the the user is the owner of the card
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is the card owner; otherwise, <c>false</c>.
    /// </returns>
    bool IsOwner(int cardId, User user);

    /// <summary>
    /// Removes the user from other cards then the one specified
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    void RemoveUserFromOtherCards(int cardId, User user);

    /// <summary>
    /// Determines whether or not this is a master controller
    /// </summary>
    bool IsMaster { get; }

    /// <summary>
    /// Determines the number of active streams on the server
    /// </summary>
    int ActiveStreams { get; }

    /// <summary>
    /// Checks if the card supports sub channels
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <returns>true, if the card supports sub channels; false otherwise</returns>
    bool SupportsSubChannels(int cardId);

    /// <summary>
    /// Signals heartbeat to the server
    /// </summary>		
    /// <param name="user">The user.</param>
    void HeartBeat(User user);

    /// <summary>
    /// Gets the number of channels decrypting.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    /// <value>The number of channels decrypting.</value>
    int NumberOfChannelsDecrypting(int cardId);

    /// <summary>
    /// Does the card have a CA module.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    bool HasCA(int cardId);

    #endregion

    #region quality control

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    bool SupportsQualityControl(int cardId);

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    bool SupportsBitRateModes(int cardId);

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    bool SupportsPeakBitRateMode(int cardId);

    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    bool SupportsBitRate(int cardId);

    /// <summary>
    /// Reloads the configuration for the given card
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    void ReloadCardConfiguration(int cardId);

    /// <summary>
    /// Gets the current quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    QualityType GetQualityType(int cardId);

    /// <summary>
    /// Sets the quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="qualityType">The new quality type</param>
    void SetQualityType(int cardId, QualityType qualityType);

    /// <summary>
    /// Gets the current bitrate mdoe
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    VIDEOENCODER_BITRATE_MODE GetBitRateMode(int cardId);

    /// <summary>
    /// Sets the bitrate mode
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="bitRateMode">The new bitrate mdoe</param>
    void SetBitRateMode(int cardId, VIDEOENCODER_BITRATE_MODE bitRateMode);

    #endregion

    #region CI Menu support

    /// <summary>
    /// Indicates if CI Menu is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    bool CiMenuSupported(int cardId);

    /// <summary>
    /// Enters the ci menu of card
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    bool EnterCiMenu(int cardId);

    /// <summary>
    /// Selects a ci menu option
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="choice">choice</param>
    /// <returns>true if successful</returns>
    bool SelectMenu(int cardId, byte choice);

    /// <summary>
    /// CloseMenu closes the menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    bool CloseMenu(int cardId);

    /// <summary>
    /// Sends a answer to cam after a request
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="Cancel">cancel request</param>
    /// <param name="Answer">answer string</param>
    /// <returns></returns>
    bool SendMenuAnswer(int cardId, bool Cancel, string Answer);

    /// <summary>
    /// Registers a ci menu callback handler for user interaction
    /// </summary>
    /// <param name="cardId"></param>
    /// <param name="CallbackHandler"></param>
    /// <returns></returns>
    bool SetCiMenuHandler(int cardId, ICiMenuCallbacks CallbackHandler);

    /// <summary>
    /// Add or remove callback destinations on the client
    /// </summary>
    event CiMenuCallback OnCiMenu;
    
    #endregion
  }
}