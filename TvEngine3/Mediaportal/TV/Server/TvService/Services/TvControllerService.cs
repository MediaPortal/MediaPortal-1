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
using System.IO;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVService.Services
{
  /// <summary>
  /// This class servers all requests from remote clients  
  /// </summary>  
  public class TvControllerService : IControllerService
  {
    private readonly IInternalControllerService _service;
    public TvControllerService ()
    {
      _service = GlobalServiceProvider.Get<IInternalControllerService>();
    }

    #region Implementation of IControllerService

    /// <summary>
    /// Gets the assembly of tvservice.exe
    /// </summary>
    /// <value>Returns the AssemblyVersion of tvservice.exe</value>    
    public string GetAssemblyVersion
    {
      get
      {
        return _service.GetAssemblyVersion;
      }
    }

    ///<summary>
    ///Gets the total number of tv-cards installed.
    ///</summary>
    ///<value>Number which indicates the cards installed</value>
    public int Cards
    {
      get { return _service.Cards; }
    }

    /// <summary>
    /// Initialized Conditional Access handler
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true if successful</returns>
    public bool InitConditionalAccess(int cardId)
    {
      return _service.InitConditionalAccess(cardId);
    }

    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>cardtype</value>
    public CardType Type(int cardId)
    {
      return _service.Type(cardId);
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>name of card</returns>
    public string CardName(int cardId)
    {
      return _service.CardName(cardId);
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(int cardId, IChannel channel)
    {
      return _service.CanTune(cardId, channel);
    }

    /// <summary>
    /// Method to check if card is currently present and detected
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>
    public bool CardPresent(int cardId)
    {
      return _service.CardPresent(cardId);
    }

    /// <summary>
    /// Method to remove a non-present card from the local card collection
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    public void CardRemove(int cardId)
    {
      _service.CardRemove(cardId);
    }

    /// <summary>
    /// Gets the device path for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>devicePath of card</returns>
    public string CardDevice(int cardId)
    {
      return _service.CardDevice(cardId);
    }

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked(int cardId)
    {
      return _service.TunerLocked(cardId);
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality(int cardId)
    {
      return _service.SignalQuality(cardId);
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel(int cardId)
    {
      return _service.SignalLevel(cardId);
    }

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void UpdateSignalSate(int cardId)
    {
      _service.UpdateSignalSate(cardId);
    }

    /// <summary>
    /// Returns if the card is currently scanning for channels or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning(int cardId)
    {
      return _service.IsScanning(cardId);
    }

    /// <summary>
    /// Returns if the card is currently grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg(int cardId)
    {
      return _service.IsGrabbingEpg(cardId);
    }

    /// <summary>
    /// scans current transponder for channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">contains tuningdetails for the transponder.</param>
    /// <returns>list of all channels found</returns>    
    public IChannel[] Scan(int cardId, IChannel channel)
    {
      return _service.Scan(cardId, channel);
    }

    /// <summary>
    /// scans nit the current transponder for channels
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">contains tuningdetails for the transponder.</param>
    /// <returns>list of all channels found</returns>
    public IChannel[] ScanNIT(int cardId, IChannel channel)
    {
      return _service.ScanNIT(cardId, channel);
    }

    /// <summary>
    /// returns the minium channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>minium channel number</returns>
    public int MinChannel(int cardId)
    {
      return _service.MinChannel(cardId);
    }

    /// <summary>
    /// returns the maximum channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>maximum channel number</returns>
    public int MaxChannel(int cardId)
    {
      return _service.MaxChannel(cardId);
    }

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="channelId">channel id</param>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    public int GetRecordingSchedule(int cardId, int channelId)
    {
      return _service.GetRecordingSchedule(cardId, channelId);
    }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param>
    /// <returns>URL containing the RTSP adress on which the recording can be found</returns>
    public string GetRecordingUrl(int idRecording)
    {
      return _service.GetRecordingUrl(idRecording);
    }

    /// <summary>
    /// Returns the contents of the chapters file (if any) for a recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param>
    /// <returns>The contents of the chapters file of the recording</returns>
    public string GetRecordingChapters(int idRecording)
    {
      return _service.GetRecordingChapters(idRecording);
    }

    /// <summary>
    /// Deletes the recording from database and disk
    /// </summary>
    /// <param name="idRecording">The id recording.</param>
    public bool DeleteRecording(int idRecording)
    {
      return _service.DeleteRecording(idRecording);
    }

    /// <summary>
    /// Deletes invalid recordings from database. A recording is invalid if the corresponding file no longer exists.
    /// </summary>
    public bool DeleteInvalidRecordings()
    {
      return _service.DeleteInvalidRecordings();
    }

    /// <summary>
    /// Deletes watched recordings from database.
    /// </summary>
    public bool DeleteWatchedRecordings(string currentTitle)
    {
      return _service.DeleteWatchedRecordings(currentTitle);
    }

    /// <summary>
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="card">returns card is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    public bool IsRecordingSchedule(int idSchedule, out IVirtualCard card)
    {
      return _service.IsRecordingSchedule(idSchedule, out card);
    }

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="idChannel"></param>
    /// <param name="card">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecording(int idChannel, out IVirtualCard card)
    {
      return _service.IsRecording(idChannel, out card);
    }

    /// <summary>
    /// Determines if any card is currently busy recording
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if a card is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnyCardRecording()
    {
      return _service.IsAnyCardRecording();
    }

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
    public bool IsAnyCardRecordingOrTimeshifting(IUser userTS, out bool isUserTS, out bool isAnyUserTS, out bool isRec)
    {
      return _service.IsAnyCardRecordingOrTimeshifting(userTS, out isUserTS, out isAnyUserTS, out isRec);
    }

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    public void StopRecordingSchedule(int idSchedule)
    {
      _service.StopRecordingSchedule(idSchedule);
    }

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    public void OnNewSchedule()
    {
      _service.OnNewSchedule();
    }

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    public bool EpgGrabberEnabled
    {
      get { return _service.EpgGrabberEnabled; }
      set { _service.EpgGrabberEnabled = value; }
    }

    /// <summary>
    /// Restarts the service.
    /// </summary>
    public void Restart()
    {
      _service.Restart();
    }

    /// <summary>
    /// Determines whether the card is in use
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user who uses the card.</param>
    /// <returns>
    /// 	<c>true</c> if card is in use; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCardInUse(int cardId, out IUser user)
    {
      return _service.IsCardInUse(cardId, out user);
    }

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>
    /// <param name="user"></param>
    public Dictionary<int, ChannelState> GetAllChannelStatesCached(IUser user)
    {
      return _service.GetAllChannelStatesCached(user);
    }

    /// <summary>
    /// Finds out whether a channel is currently tuneable or not
    /// </summary>
    /// <param name="idChannel">The channel id</param>
    /// <param name="user">User</param>
    /// <returns>an enum indicating tunable/timeshifting/recording</returns>
    public ChannelState GetChannelState(int idChannel, IUser user)
    {
      return _service.GetChannelState(idChannel, user);
    }

    /// <summary>
    /// Returns a list of all ip adresses on the server.
    /// </summary>
    /// <value>The server ip adresses.</value>
    public IEnumerable<string> ServerIpAdresses
    {
      get { return _service.ServerIpAdresses; }
    }

    /// <summary>
    /// Returns the port used for RTSP streaming.
    /// If streaming is not initialized, returns 0.
    /// </summary>
    /// <value>The streaming port</value>
    public int StreamingPort
    {
      get { return _service.StreamingPort; }
    }

    /// <summary>
    /// Gets a list of all streaming clients.
    /// </summary>
    /// <value>The streaming clients.</value>
    public List<RtspClient> StreamingClients
    {
      get { return _service.StreamingClients; }
    }

    /// <summary>
    /// Reset DiSEqC for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    public void DiSEqCReset(int cardId)
    {
      _service.DiSEqCReset(cardId);
    }

    /// <summary>
    /// Stops the DiSEqC motor for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    public void DiSEqCStopMotor(int cardId)
    {
      _service.DiSEqCStopMotor(cardId);
    }

    /// <summary>
    /// Sets the DiSEqC east limit for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    public void DiSEqCSetEastLimit(int cardId)
    {
      _service.DiSEqCSetEastLimit(cardId);
    }

    /// <summary>
    /// Sets the DiSEqC west limit for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    public void DiSEqCSetWestLimit(int cardId)
    {
      _service.DiSEqCSetWestLimit(cardId);
    }

    /// <summary>
    /// DiSEqC force limit  for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="onoff">on/off</param>
    public void DiSEqCForceLimit(int cardId, bool onoff)
    {
      _service.DiSEqCForceLimit(cardId, onoff);
    }

    /// <summary>
    /// Moves the DiSEqC motor for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="direction">direction</param>
    /// <param name="numberOfSteps">Number of steps</param>
    public void DiSEqCDriveMotor(int cardId, DiSEqCDirection direction, byte numberOfSteps)
    {
      _service.DiSEqCDriveMotor(cardId, direction, numberOfSteps);
    }

    /// <summary>
    /// Stores the current DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="position">position</param>
    public void DiSEqCStorePosition(int cardId, byte position)
    {
      _service.DiSEqCStorePosition(cardId, position);
    }

    /// <summary>
    /// DiSEqC move to the reference position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    public void DiSEqCGotoReferencePosition(int cardId)
    {
      _service.DiSEqCGotoReferencePosition(cardId);
    }

    /// <summary>
    /// Go to the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="position">position</param>
    public void DiSEqCGotoPosition(int cardId, byte position)
    {
      _service.DiSEqCGotoPosition(cardId, position);
    }

    /// <summary>
    /// Gets the DiSEqC position for the given card
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="satellitePosition">satellite position</param>
    /// <param name="stepsAzimuth">azimuth</param>
    /// <param name="stepsElevation">elvation</param>
    public void DiSEqCGetPosition(int cardId, out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      _service.DiSEqCGetPosition(cardId, out satellitePosition, out stepsAzimuth, out stepsElevation);
    }

    /// <summary>
    /// Returns the subchannels count for the selected card
    /// stream for the selected card
    /// </summary>
    /// <param name="idCard">card id.</param>
    /// <returns>
    /// subchannels count
    /// </returns>
    public int GetSubChannels(int idCard)
    {
      return _service.GetSubChannels(idCard);
    }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for the selected card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// URL containing the RTSP adress on which the card transmits its stream
    /// </returns>
    public string GetStreamingUrl(IUser user)
    {
      return _service.GetStreamingUrl(user);
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// filename of the recording or null when not recording
    /// </returns>
    public string RecordingFileName(ref IUser user)
    {
      return _service.RecordingFileName(ref user);
    }

    /// <summary>
    /// Gets the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>IChannel</returns>
    public IChannel CurrentChannel(ref IUser user)
    {
      return _service.CurrentChannel(ref user);
    }

    /// <summary>
    /// returns the id of the current channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public int CurrentDbChannel(ref IUser user)
    {
      return _service.CurrentDbChannel(ref user);
    }

    /// <summary>
    /// Gets the name of the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>channel name</returns>
    public string CurrentChannelName(ref IUser user)
    {
      return _service.CurrentChannelName(ref user);
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// yes if channel is scrambled and CI/CAM cannot decode it, otherwise false
    /// </returns>
    public bool IsScrambled(ref IUser user)
    {
      return _service.IsScrambled(ref user);
    }

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// timeshifting filename null when not timeshifting
    /// </returns>
    public string TimeShiftFileName(ref IUser user)
    {
      return _service.TimeShiftFileName(ref user);
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool TimeShiftGetCurrentFilePosition(ref IUser user, ref long position, ref long bufferId)
    {
      return _service.TimeShiftGetCurrentFilePosition(ref user, ref position, ref bufferId);
    }

    /// <summary>
    /// Returns if the card is currently timeshifting or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref IUser user)
    {
      return _service.IsTimeShifting(ref user);
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(IUser user, int pageNumber)
    {
      return _service.TeletextRotation(user, pageNumber);
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// yes if channel has teletext otherwise false
    /// </returns>
    public bool HasTeletext(IUser user)
    {
      return _service.HasTeletext(user);
    }

    /// <summary>
    /// turn on/off teletext grabbing for a card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="onOff">boolean indicating if teletext grabbing should be enabled or not</param>
    public void GrabTeletext(IUser user, bool onOff)
    {
      _service.GrabTeletext(user, onOff);
    }

    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>
    /// byte[] array containing the raw teletext page or null if page is not found
    /// </returns>
    public byte[] GetTeletextPage(IUser user, int pageNumber, int subPageNumber)
    {
      return _service.GetTeletextPage(user, pageNumber, subPageNumber);
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>
    /// number of teletext subpages for the pagenumber
    /// </returns>
    public int SubPageCount(IUser user, int pageNumber)
    {
      return _service.SubPageCount(user, pageNumber);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber(IUser user)
    {
      return _service.GetTeletextRedPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber(IUser user)
    {
      return _service.GetTeletextGreenPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber(IUser user)
    {
      return _service.GetTeletextYellowPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber(IUser user)
    {
      return _service.GetTeletextBluePageNumber(user);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="bufferFile1"></param>
    /// <param name="position2"></param>
    /// <param name="bufferFile2"></param>
    /// <param name="recordingFile"></param>
    public void CopyTimeShiftFile(long position1, string bufferFile1, long position2, string bufferFile2, string recordingFile)
    {
      _service.CopyTimeShiftFile(position1, bufferFile1, position2, bufferFile2, recordingFile);
    }

    /// <summary>
    /// returns the list of available audio streams for the card specified
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>List containing all audio streams</returns>
    public IEnumerable<IAudioStream> AvailableAudioStreams(IUser user)
    {
      return _service.AvailableAudioStreams(user);
    }

    /// <summary>
    /// returns the current video stream on the virtual card. 
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>List containing all audio streams</returns>
    public IVideoStream GetCurrentVideoStream(IUser user)
    {
      return _service.GetCurrentVideoStream(user);
    }

    /// <summary>
    /// Stops the card.
    /// </summary>
    /// <param name="user">The user.</param>
    public void StopCard(IUser user)
    {
      _service.StopCard(user);
    }

    /// <summary>
    /// Query what card would be used for timeshifting on any given channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>
    /// returns card id which would be used when doing the actual timeshifting.
    /// </returns>
    public int TimeShiftingWouldUseCard(ref IUser user, int idChannel)
    {
      return _service.TimeShiftingWouldUseCard(ref user, idChannel);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>        
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out IVirtualCard card, bool forceCardId)
    {
      return _service.StartTimeShifting(ref user, idChannel, out card, forceCardId);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out IVirtualCard card, out bool cardChanged)
    {
      return _service.StartTimeShifting(ref user, idChannel, out card, out cardChanged);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out IVirtualCard card)
    {
      return _service.StartTimeShifting(ref user, idChannel, out card);
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="reason">reason why timeshifting is stopped.</param>
    /// <returns>true if success otherwise false</returns>
    public bool StopTimeShifting(ref IUser user, TvStoppedReason reason)
    {
      return _service.StopTimeShifting(ref user, reason);
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <returns>true if success otherwise false</returns>
    public bool StopTimeShifting(ref IUser user)
    {
      return _service.StopTimeShifting(ref user);
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns>true if success otherwise false</returns>
    public TvResult StartRecording(ref IUser user, ref string fileName)
    {
      return _service.StartRecording(ref user, ref fileName);
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>true if success otherwise false</returns>
    public bool StopRecording(ref IUser user)
    {
      return _service.StopRecording(ref user);
    }

    /// <summary>
    /// Scan the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    public TvResult Scan(ref IUser user, IChannel channel, int idChannel)
    {
      return _service.Scan(ref user, channel, idChannel);
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    public TvResult Tune(ref IUser user, IChannel channel, int idChannel)
    {
      return _service.Tune(ref user, channel, idChannel);
    }

    /// <summary>
    /// Gets the users for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public IDictionary<string, IUser> GetUsersForCard(int cardId)
    {
      return _service.GetUsersForCard(cardId);
    }

    /// <summary>
    /// Determines whether the the user is the owner of the card
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is the card owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(int cardId, IUser user)
    {
      return _service.IsOwner(cardId, user);
    }

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsQualityControl(int cardId)
    {
      return _service.SupportsQualityControl(cardId);
    }

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsBitRateModes(int cardId)
    {
      return _service.SupportsBitRateModes(cardId);
    }

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode(int cardId)
    {
      return _service.SupportsPeakBitRateMode(cardId);
    }

    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsBitRate(int cardId)
    {
      return _service.SupportsBitRate(cardId);
    }

    /// <summary>
    /// Reloads the configuration for the given card
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    public void ReloadCardConfiguration(int cardId)
    {
      _service.ReloadCardConfiguration(cardId);
    }

    /// <summary>
    /// Gets the current quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    public QualityType GetQualityType(int cardId)
    {
      return _service.GetQualityType(cardId);
    }

    /// <summary>
    /// Sets the quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="qualityType">The new quality type</param>
    public void SetQualityType(int cardId, QualityType qualityType)
    {
      _service.SetQualityType(cardId, qualityType);
    }

    /// <summary>
    /// Gets the current bitrate mdoe
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    public VIDEOENCODER_BITRATE_MODE GetBitRateMode(int cardId)
    {
      return _service.GetBitRateMode(cardId);
    }

    /// <summary>
    /// Sets the bitrate mode
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="bitRateMode">The new bitrate mdoe</param>
    public void SetBitRateMode(int cardId, VIDEOENCODER_BITRATE_MODE bitRateMode)
    {
      _service.SetBitRateMode(cardId, bitRateMode);
    }

    /// <summary>
    /// Indicates if CI Menu is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool CiMenuSupported(int cardId)
    {
      return _service.CiMenuSupported(cardId);
    }

    /// <summary>
    /// Enters the ci menu of card
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    public bool EnterCiMenu(int cardId)
    {
      return _service.EnterCiMenu(cardId);
    }

    /// <summary>
    /// Selects a ci menu option
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="choice">choice</param>
    /// <returns>true if successful</returns>
    public bool SelectMenu(int cardId, byte choice)
    {
      return _service.SelectMenu(cardId, choice);
    }

    /// <summary>
    /// CloseMenu closes the menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    public bool CloseMenu(int cardId)
    {
      return _service.CloseMenu(cardId);
    }

    /// <summary>
    /// Sends a answer to cam after a request
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="cancel">cancel request</param>
    /// <param name="answer">answer string</param>
    /// <returns></returns>
    public bool SendMenuAnswer(int cardId, bool cancel, string answer)
    {
      return _service.SendMenuAnswer(cardId, cancel, answer);
    }

    /// <summary>
    /// Registers a ci menu callback handler for user interaction
    /// </summary>
    /// <param name="cardId"></param>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    public bool SetCiMenuHandler(int cardId, ICiMenuCallbacks callbackHandler)
    {
      return _service.SetCiMenuHandler(cardId, callbackHandler);
    }

    public event CiMenuCallback OnCiMenu;

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="user">user</param>
    /// <param name="totalTSpackets">Amount of packets processed</param>
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    public void GetStreamQualityCounters(IUser user, out int totalTSpackets, out int discontinuityCounter)
    {
      _service.GetStreamQualityCounters(user, out totalTSpackets, out discontinuityCounter);
    }

    public void RegisterUserForHeartbeatMonitoring (string username)
    {
      _service.RegisterUserForHeartbeatMonitoring(username);
    }    

    public void RegisterUserForCiMenu(string username)
    {
      _service.RegisterUserForHeartbeatMonitoring(username);
    }

    public void UnRegisterUserForHeartbeatMonitoring(string username)
    {
      _service.UnRegisterUserForHeartbeatMonitoring(username);
    }

    public void UnRegisterUserForCiMenu(string username)
    {
      _service.UnRegisterUserForCiMenu(username);
    }

    public void RegisterUserForTvServerEvents(string username)
    {
      _service.RegisterUserForTvServerEvents(username);
    }
    public void UnRegisterUserForTvServerEvents(string username)
    {
      _service.UnRegisterUserForTvServerEvents(username);
    }


    public IDictionary<string, byte[]> GetPluginBinaries()
    {
      return _service.GetPluginBinaries();
    }



    #endregion
  }
}